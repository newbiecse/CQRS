#Requires -Version 5.1
<#
.SYNOPSIS
  Start a local Kubernetes cluster and open the Kubernetes Dashboard.

.EXAMPLE
  .\scripts\start-local-k8s.ps1

.EXAMPLE
  .\scripts\start-local-k8s.ps1 -Provider minikube

.EXAMPLE
  .\scripts\start-local-k8s.ps1 -SkipBrowser
#>
param(
    [ValidateSet("auto", "minikube", "docker-desktop")]
    [string]$Provider = "auto",
    [switch]$SkipDashboard,
    [switch]$SkipBrowser,
    [int]$DashboardPort = 8443,
    [int]$MinikubeMemoryMb = 16384,
    [int]$MinikubeCpus = 4
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$DashboardNamespace = "kubernetes-dashboard"
$PortForwardJobName = "cqrs-k8s-dashboard-portforward"

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command not found: $Name"
    }
}

function Test-ClusterReachable {
    kubectl cluster-info *> $null
    return $LASTEXITCODE -eq 0
}

function Get-ClusterFlavor {
    $context = kubectl config current-context 2>$null
    if ($context -match "docker-desktop") { return "docker-desktop" }
    if ($context -match "minikube") { return "minikube" }
    if ($context -match "kind-") { return "kind" }
    return "unknown"
}

function Resolve-Provider {
    if ($Provider -ne "auto") { return $Provider }
    if (Get-Command minikube -ErrorAction SilentlyContinue) {
        $status = minikube status --format='{{.Host}}' 2>$null
        if ($status -eq "Running") { return "minikube" }
    }
    if ((Get-ClusterFlavor) -eq "docker-desktop") { return "docker-desktop" }
    if (Get-Command minikube -ErrorAction SilentlyContinue) { return "minikube" }
    return "docker-desktop"
}

function Start-DockerDesktopKubernetes {
    Write-Step "Starting Docker Desktop Kubernetes..."

    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker desktop start 2>$null | Out-Null
        docker desktop kubernetes enable 2>$null | Out-Null
    }

    $deadline = (Get-Date).AddMinutes(5)
    while ((Get-Date) -lt $deadline) {
        if (Test-ClusterReachable) {
            Write-Host "Cluster is ready (context: $(kubectl config current-context))."
            return
        }
        Write-Host "Waiting for Kubernetes..."
        Start-Sleep -Seconds 5
    }

    throw @"
Kubernetes is not reachable.

Enable it manually:
  Docker Desktop -> Settings -> Kubernetes -> Enable Kubernetes -> Apply

Then run this script again.
"@
}

function Start-MinikubeCluster {
    Write-Step "Starting minikube..."

    $running = minikube status --format='{{.Host}}' 2>$null
    if ($running -ne "Running") {
        minikube start --memory=$MinikubeMemoryMb --cpus=$MinikubeCpus
    }
    else {
        Write-Host "minikube is already running."
    }

    if (-not (Test-ClusterReachable)) {
        throw "minikube started but kubectl cannot reach the cluster."
    }
}

function Install-DashboardHelm {
    Write-Step "Installing Kubernetes Dashboard (Helm)..."

    Assert-Command helm
    helm repo add kubernetes-dashboard https://kubernetes.github.io/dashboard/ 2>$null | Out-Null
    helm repo update | Out-Null

    helm upgrade --install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard `
        --namespace $DashboardNamespace `
        --create-namespace `
        --wait --timeout 5m

    kubectl apply -k "$RepoRoot\infra\k8s\base\dashboard"

    kubectl wait --for=condition=ready pod `
        -l "app.kubernetes.io/instance=kubernetes-dashboard" `
        -n $DashboardNamespace `
        --timeout=300s 2>$null
}

function Enable-MinikubeDashboardAddon {
    Write-Step "Enabling minikube dashboard addon..."
    minikube addons enable dashboard | Out-Null
    minikube addons enable metrics-server 2>$null | Out-Null
    kubectl apply -k "$RepoRoot\infra\k8s\base\dashboard" 2>$null
}

function Get-DashboardServiceName {
    $services = kubectl get svc -n $DashboardNamespace -o jsonpath='{.items[*].metadata.name}' 2>$null
    if ($services -match "kubernetes-dashboard-kong-proxy") { return "kubernetes-dashboard-kong-proxy" }
    if ($services -match "kubernetes-dashboard") { return "kubernetes-dashboard" }
    return ($services -split ' ' | Select-Object -First 1)
}

function Stop-ExistingPortForward {
    Get-Job -Name $PortForwardJobName -ErrorAction SilentlyContinue | Stop-Job -PassThru | Remove-Job -Force
}

function Start-DashboardPortForward([string]$ServiceName, [int]$LocalPort) {
    Stop-ExistingPortForward

    Write-Host "Port-forward: localhost:$LocalPort -> $ServiceName (namespace: $DashboardNamespace)"
    $job = Start-Job -Name $PortForwardJobName -ScriptBlock {
        param($Ns, $Svc, $Port)
        kubectl port-forward -n $Ns "svc/$Svc" "${Port}:443"
    } -ArgumentList $DashboardNamespace, $ServiceName, $LocalPort

    Start-Sleep -Seconds 3
    if ($job.State -eq "Failed") {
        Receive-Job $job
        throw "kubectl port-forward failed."
    }
}

function Get-DashboardLoginToken {
    try {
        return kubectl -n $DashboardNamespace create token admin-user 2>$null
    }
    catch {
        return $null
    }
}

function Open-DashboardBrowser([string]$Url) {
    if ($SkipBrowser) { return }
    Write-Host "Opening browser: $Url"
    Start-Process $Url
}

function Start-MinikubeDashboard {
    Enable-MinikubeDashboardAddon
    Write-Step "Starting minikube dashboard..."
    $url = minikube dashboard --url 2>$null
    if ($url) {
        Write-Host "Dashboard URL: $url"
        if (-not $SkipBrowser) { Start-Process $url }
    }
    else {
        minikube dashboard
    }

    $token = Get-DashboardLoginToken
    if ($token) {
        Write-Host ""
        Write-Host "Login token (admin-user):" -ForegroundColor Yellow
        Write-Host $token
    }
}

function Start-HelmDashboard {
    Install-DashboardHelm

    $serviceName = Get-DashboardServiceName
    if (-not $serviceName) {
        throw "Could not find a Kubernetes Dashboard service in namespace $DashboardNamespace"
    }

    Start-DashboardPortForward -ServiceName $serviceName -LocalPort $DashboardPort

    $url = "https://localhost:$DashboardPort"
    Write-Host ""
    Write-Host "Dashboard URL: $url" -ForegroundColor Green
    Write-Host "Accept the self-signed certificate warning in your browser." -ForegroundColor DarkYellow

    $token = Get-DashboardLoginToken
    if ($token) {
        Write-Host ""
        Write-Host "Login token (paste on dashboard sign-in -> Token):" -ForegroundColor Yellow
        Write-Host $token
    }
    else {
        Write-Host ""
        Write-Host "Create a token with:" -ForegroundColor Yellow
        Write-Host "  kubectl -n $DashboardNamespace create token admin-user"
    }

    Open-DashboardBrowser -Url $url
    Write-Host ""
    Write-Host "Port-forward runs in background job '$PortForwardJobName'."
    Write-Host "Stop it with:  Get-Job $PortForwardJobName | Stop-Job; Get-Job $PortForwardJobName | Remove-Job"
}

# --- main ---
Write-Step "CQRS Demo — start local Kubernetes + Dashboard"
Assert-Command kubectl
Assert-Command docker

$resolved = Resolve-Provider
Write-Host "Provider: $resolved"

switch ($resolved) {
    "minikube" { Start-MinikubeCluster }
    "docker-desktop" { Start-DockerDesktopKubernetes }
    default { Start-DockerDesktopKubernetes }
}

if (-not $SkipDashboard) {
    if ($resolved -eq "minikube") {
        Start-MinikubeDashboard
    }
    else {
        Start-HelmDashboard
    }
}

Write-Step "Next steps"
Write-Host @"
  Deploy CQRS stack:  .\scripts\deploy-local-k8s.ps1
  Cluster status:     kubectl get nodes
  CQRS pods:          kubectl get pods -n cqrs-demo
"@

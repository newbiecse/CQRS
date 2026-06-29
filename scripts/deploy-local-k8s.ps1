#Requires -Version 5.1
<#
.SYNOPSIS
  Deploy CQRS Demo to a local Kubernetes cluster (Docker Desktop, minikube, or kind).

.EXAMPLE
  .\scripts\deploy-local-k8s.ps1

.EXAMPLE
  .\scripts\deploy-local-k8s.ps1 -InfraOnly

.EXAMPLE
  .\scripts\deploy-local-k8s.ps1 -SkipBuild -SkipIngress
#>
param(
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',
    [switch]$InfraOnly,
    [switch]$SkipBuild,
    [switch]$SkipIngress,
    [switch]$SkipApps,
    [string]$ImageRegistry = "cqrsdemo",
    [string]$ImageTag = "latest"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
. "$PSScriptRoot\_infra-env.ps1"
$InfraPaths = Get-InfraEnvironmentPaths -Environment $Environment
$Namespace = $InfraPaths.Namespace

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command not found: $Name"
    }
}

function Test-ClusterReady {
    kubectl cluster-info *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "kubectl cannot reach a cluster. Enable Kubernetes in Docker Desktop or start minikube."
    }
}

function Get-ClusterFlavor {
    $context = kubectl config current-context 2>$null
    if ($context -match "docker-desktop") { return "docker-desktop" }
    if ($context -match "minikube") { return "minikube" }
    if ($context -match "kind-") { return "kind" }
    return "unknown"
}

function Install-IngressNginx {
    $release = helm list -n ingress-nginx -o json 2>$null | ConvertFrom-Json
    if ($release | Where-Object { $_.name -eq "ingress-nginx" }) {
        Write-Host "ingress-nginx already installed."
        return
    }

    Write-Step "Installing ingress-nginx (Helm)..."
    helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx 2>$null | Out-Null
    helm repo update | Out-Null
    helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx `
        --namespace ingress-nginx `
        --create-namespace `
        --set controller.watchIngressWithoutClass=true `
        --set controller.ingressClassResource.default=true `
        --wait --timeout 5m
}

function Wait-ForPods([string]$LabelSelector, [int]$TimeoutSeconds = 600) {
    Write-Host "Waiting for pods ($LabelSelector)..."
    kubectl wait --for=condition=ready pod -l $LabelSelector -n $Namespace --timeout="${TimeoutSeconds}s"
}

function Build-Images {
    Write-Step "Building container images..."
    & "$RepoRoot\scripts\build-container-images.ps1" -Registry $ImageRegistry -Tag $ImageTag
}

function Use-MinikubeDocker {
    if ((Get-ClusterFlavor) -ne "minikube") { return }
    Write-Host "Using minikube Docker daemon..."
    Invoke-Expression (minikube docker-env --shell powershell)
}

function Import-ImagesToMinikube([string]$Registry, [string]$Tag) {
    $flavor = Get-ClusterFlavor
    if ($flavor -ne "minikube") { return }

    Write-Step "Loading images into minikube..."
    $images = @(
        "db-initializer",
        "shop-gateway", "shop-admin-api", "shop",
        "product-commands", "product-queries", "product-projection-worker",
        "cart-commands", "cart-queries", "cart-projection-worker",
        "order-commands", "order-queries", "order-projection-worker", "order-integration-worker",
        "payment-commands", "payment-queries", "payment-projection-worker",
        "user-commands", "user-queries", "user-projection-worker",
        "reporting-queries", "reporting-projection-worker",
        "checkout-saga-api", "checkout-saga-worker",
        "audit-projection-worker"
    )
    foreach ($image in $images) {
        minikube image load "${Registry}/${image}:${Tag}"
    }
}

function Deploy-Infra {
    Write-Step "Applying platform infrastructure (Kustomize, environment: $Environment)..."
    kubectl apply -k $InfraPaths.K8sOverlay

    Write-Host "Waiting for SQL Server (can take 2-5 minutes on first run)..."
    kubectl wait --for=condition=ready pod -l app=sqlserver -n $Namespace --timeout=600s

    Write-Host "Waiting for Elasticsearch..."
    kubectl wait --for=condition=ready pod -l app=elasticsearch -n $Namespace --timeout=600s

    Write-Host "Waiting for Kafka..."
    kubectl wait --for=condition=ready pod -l app=kafka -n $Namespace --timeout=600s
}

function Deploy-Apps {
    Write-Step "Installing microservices (Helm, environment: $Environment)..."

    $setArgs = @(
        '--set', "image.registry=$ImageRegistry",
        '--set', "image.tag=$ImageTag",
        '--set', "sql.password=Your_password123",
        '--set', "dbInit.image=${ImageRegistry}/db-initializer"
    )

    if (Get-Command helmfile -ErrorAction SilentlyContinue) {
        Write-Host "Using Helmfile (helmfile -e $Environment apply)..."
        Push-Location (Join-Path $RepoRoot 'infra\helm')
        try {
            helmfile -e $Environment apply --set image.registry=$ImageRegistry --set image.tag=$ImageTag `
                --set sql.password=Your_password123 --set dbInit.image="${ImageRegistry}/db-initializer"
            if ($LASTEXITCODE -ne 0) { throw "helmfile apply failed" }
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "Helmfile not found — using helm with layered values files..."
        $helmArgs = @(
            'upgrade', '--install', 'cqrs-apps', $InfraPaths.HelmChart,
            '--namespace', $Namespace,
            '--create-namespace',
            '-f', $InfraPaths.HelmValuesBase,
            '-f', $InfraPaths.HelmValuesEnv
        ) + $setArgs + @('--wait', '--timeout', '15m')
        helm @helmArgs
        if ($LASTEXITCODE -ne 0) { throw "helm upgrade failed" }
    }

    if ($Environment -ne 'prod') {
        Write-Host "Waiting for database initializer job..."
        kubectl wait --for=condition=complete job/db-initializer -n $Namespace --timeout=600s
    }
}

function Show-AccessInfo {
    $flavor = Get-ClusterFlavor
    Write-Step "Deployment complete"
    Write-Host @"

Access (choose one):

A) Ingress hosts — add to C:\Windows\System32\drivers\etc\hosts:
   127.0.0.1 shop.local admin-api.local shop-fe.local

   Shop gateway:  http://shop.local
   Admin API:     http://admin-api.local
   Shop UI:       http://shop-fe.local
   Product search: http://shop.local/product-queries/api/products/search?q=phone

B) NodePort (no hosts file):
   Shop gateway:  http://localhost:30500
   Admin API:     http://localhost:30510
   Shop UI:       http://localhost:30501
   Kibana:        http://localhost:30561

C) Port-forward (any cluster):
   kubectl port-forward -n cqrs-demo svc/shop-gateway 5000:8080
   kubectl port-forward -n cqrs-demo svc/kibana 5601:5601

Cluster: $flavor
Namespace: $Namespace
Status:  kubectl get pods -n $Namespace

"@
}

# --- main ---
Write-Step "CQRS Demo — Kubernetes deploy ($Environment)"
Assert-Command kubectl
Assert-Command helm
Assert-Command docker
Test-ClusterReady

$flavor = Get-ClusterFlavor
Write-Host "Detected cluster: $flavor"

if ($flavor -eq "minikube" -and -not $SkipBuild) {
    Write-Host "Tip: run 'minikube start --memory=16384 --cpus=4' for SQL Server + Elasticsearch."
}

if (-not $SkipIngress) {
    Install-IngressNginx
}

Deploy-Infra

if ($InfraOnly) {
    Write-Host "Infra only (-InfraOnly). Done."
    kubectl get pods -n $Namespace
    exit 0
}

if (-not $SkipBuild) {
    if ((Get-ClusterFlavor) -eq "minikube") {
        Use-MinikubeDocker
    }
    Build-Images
    Import-ImagesToMinikube -Registry $ImageRegistry -Tag $ImageTag
}

if (-not $SkipApps) {
    Deploy-Apps
}

Show-AccessInfo

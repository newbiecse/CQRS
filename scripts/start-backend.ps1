# Start all backend APIs and workers in the background. Logs -> scripts/logs/
# Run build-backend.ps1 first. Requires infra (SQL, Kafka, ES) to be up.

param(
    [switch]$SkipBuildCheck
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\_backend-services.ps1"

$repoRoot = Split-Path -Parent $PSScriptRoot
$logDir = Join-Path $repoRoot "scripts\logs"
$pidFile = Join-Path $repoRoot "scripts\.backend.pids.json"

if (Test-Path $pidFile) {
    Write-Error "Backend may already be running ($pidFile exists). Run .\scripts\stop-backend.ps1 first."
}

if (-not $SkipBuildCheck) {
    $sampleDll = Join-Path $repoRoot "backend\src\Gateway\Shop.Admin.Api\bin\Release\net8.0\Shop.Admin.Api.dll"
    if (-not (Test-Path $sampleDll)) {
        Write-Warning "Release build not found. Running build-backend.ps1 ..."
        & "$PSScriptRoot\build-backend.ps1"
    }
}

New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$started = @()
foreach ($svc in $BackendServices) {
    $projectPath = Join-Path $repoRoot $svc.Project
    if (-not (Test-Path $projectPath)) {
        Write-Warning "Skipping missing project: $($svc.Project)"
        continue
    }

    $logPath = Join-Path $logDir "$($svc.Name).log"
    $proc = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @("run", "--no-build", "-c", "Release", "--project", $projectPath) `
        -WorkingDirectory $repoRoot `
        -RedirectStandardOutput $logPath `
        -RedirectStandardError (Join-Path $logDir "$($svc.Name).err.log") `
        -PassThru `
        -WindowStyle Hidden

    $started += [ordered]@{
        name = $svc.Name
        pid  = $proc.Id
        log  = $logPath
    }
    Write-Host ("Started {0,-24} PID {1}" -f $svc.Name, $proc.Id)
    Start-Sleep -Milliseconds 400
}

$started | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 $pidFile

Write-Host ""
Write-Host "Started $($started.Count) backend processes."
Write-Host "  PID file : $pidFile"
Write-Host "  Logs     : $logDir"
Write-Host "  Admin BFF: http://localhost:5100"
Write-Host "  Shop GW  : http://localhost:5000"
Write-Host ""
Write-Host "Stop with: .\scripts\stop-backend.ps1"

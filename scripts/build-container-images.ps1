#Requires -Version 5.1
param(
    [string]$Registry = "cqrsdemo",
    [string]$Tag = "latest",
    [string[]]$Only
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ManifestPath = Join-Path $RepoRoot "infra\docker\images.manifest.json"

if (-not (Test-Path $ManifestPath)) {
    Write-Host "Manifest not found. Running generate-service-dockerfiles.ps1 ..."
    & "$PSScriptRoot\generate-service-dockerfiles.ps1"
}

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json

function Build-Image {
    param(
        [string]$Name,
        [string]$Dockerfile
    )

    if ($Only -and $Only -notcontains $Name) {
        return
    }

    $dockerfilePath = Join-Path $RepoRoot ($Dockerfile -replace '/', '\')
    if (-not (Test-Path $dockerfilePath)) {
        throw "Dockerfile not found: $dockerfilePath (run generate-service-dockerfiles.ps1)"
    }

    Write-Host "Building $Name..."
    docker build -f $dockerfilePath -t "${Registry}/${Name}:${Tag}" $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "docker build failed for $Name" }
}

foreach ($img in $manifest.images) {
    Build-Image -Name $img.name -Dockerfile $img.dockerfile
}

Write-Host "Done. Images: ${Registry}/*:${Tag}"

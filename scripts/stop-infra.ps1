# Stop local infrastructure containers (SQL Server, Kafka, Elasticsearch, ...).

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $repoRoot "infra\docker\docker-compose.yml"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed or not on PATH."
}

Write-Host "Stopping infrastructure (docker compose down)..."
docker compose -f $composeFile down

Write-Host "Infrastructure stopped."

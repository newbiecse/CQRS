# Start local infrastructure: SQL Server, Kafka, Elasticsearch, Kibana, OTEL collector.
# Requires Docker Desktop (or Docker Engine) with compose v2.

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $repoRoot "infra\docker\docker-compose.yml"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed or not on PATH."
}

Write-Host "Starting infrastructure (docker compose)..."
docker compose -f $composeFile up -d

Write-Host ""
Write-Host "Waiting for containers to become healthy (up to 90s)..."
$deadline = (Get-Date).AddSeconds(90)
do {
    $ps = docker compose -f $composeFile ps --format json 2>$null | ConvertFrom-Json
    $unhealthy = @($ps | Where-Object { $_.Health -and $_.Health -ne "healthy" })
    if ($unhealthy.Count -eq 0) { break }
    Start-Sleep -Seconds 3
} while ((Get-Date) -lt $deadline)

Write-Host ""
docker compose -f $composeFile ps
Write-Host ""
Write-Host "Infrastructure started."
Write-Host "  SQL Server     localhost:1433  (sa / Your_password123)"
Write-Host "  Kafka          localhost:9092"
Write-Host "  Elasticsearch  localhost:9200"
Write-Host "  Kibana         http://localhost:5601"
Write-Host "  OTLP gRPC      localhost:4317"
Write-Host ""
Write-Host "Next: .\scripts\initialize-databases.ps1"

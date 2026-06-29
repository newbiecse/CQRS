# Start infrastructure: SQL Server, Kafka, Elasticsearch, Kibana, OTEL collector.
# Requires Docker Desktop (or Docker Engine) with compose v2.

param(
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev'
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\_infra-env.ps1"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed or not on PATH."
}

$paths = Get-InfraEnvironmentPaths -Environment $Environment

Write-Host "Starting infrastructure (docker compose, environment: $Environment)..."
Invoke-DockerCompose -Environment $Environment up -d

Write-Host ""
Write-Host "Waiting for containers (30s)..."
Start-Sleep -Seconds 30

Write-Host ""
Invoke-DockerCompose -Environment $Environment ps
Write-Host ""
Write-Host "Infrastructure started ($Environment)."
Write-Host "  SQL Server     localhost:1433"
Write-Host "  Kafka          localhost:9092"
Write-Host "  Elasticsearch  localhost:9200"
Write-Host "  Kibana         http://localhost:5601"
Write-Host "  OTLP gRPC      localhost:4317"
Write-Host ""
Write-Host "Copy infra/docker/env/$Environment/.env.example to .env to override secrets."
Write-Host "Next: .\scripts\initialize-databases.ps1"

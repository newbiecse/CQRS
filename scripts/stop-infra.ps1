# Stop infrastructure containers (SQL Server, Kafka, Elasticsearch, ...).

param(
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev'
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\_infra-env.ps1"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed or not on PATH."
}

Write-Host "Stopping infrastructure (docker compose down, environment: $Environment)..."
Invoke-DockerCompose -Environment $Environment down

Write-Host "Infrastructure stopped."

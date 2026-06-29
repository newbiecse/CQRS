# Requires SQL Server from infra/docker/docker-compose.yml (sa / Your_password123 on localhost:1433).

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Building database initializer..."
dotnet build "$repoRoot\backend\tools\CqrsDemo.DatabaseInitializer\CqrsDemo.DatabaseInitializer.csproj" -c Release

Write-Host "Creating databases and tables..."
dotnet run --project "$repoRoot\backend\tools\CqrsDemo.DatabaseInitializer\CqrsDemo.DatabaseInitializer.csproj" -c Release --no-build

Write-Host "Exporting SQL scripts..."
dotnet run --project "$repoRoot\backend\tools\CqrsDemo.DatabaseInitializer\CqrsDemo.DatabaseInitializer.csproj" -c Release --no-build -- --export-sql

Write-Host "Done."

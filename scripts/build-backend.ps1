# Build the full .NET backend solution (Release).

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$sln = Join-Path $repoRoot "backend\CqrsDemo.Distributed.sln"

Write-Host "Restoring NuGet packages..."
dotnet restore $sln

Write-Host "Building solution (Release)..."
dotnet build $sln -c Release --no-restore

Write-Host "Building database initializer..."
dotnet build (Join-Path $repoRoot "backend\tools\CqrsDemo.DatabaseInitializer\CqrsDemo.DatabaseInitializer.csproj") -c Release

Write-Host ""
Write-Host "Backend build complete."

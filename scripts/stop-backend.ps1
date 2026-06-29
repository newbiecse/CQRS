# Stop all backend processes started by start-backend.ps1.

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$pidFile = Join-Path $repoRoot "scripts\.backend.pids.json"

if (-not (Test-Path $pidFile)) {
    Write-Host "No PID file found ($pidFile). Nothing to stop."
    exit 0
}

$entries = Get-Content $pidFile -Raw | ConvertFrom-Json
if ($entries -isnot [array]) { $entries = @($entries) }

$stopped = 0
foreach ($entry in $entries) {
    $pid = [int]$entry.pid
    $name = $entry.name
    if (-not (Get-Process -Id $pid -ErrorAction SilentlyContinue)) {
        Write-Host "Already stopped: $name (PID $pid)"
        continue
    }

    # Kill process tree (dotnet run spawns child host)
    & taskkill /T /F /PID $pid 2>$null | Out-Null
    Write-Host "Stopped $name (PID $pid)"
    $stopped++
}

Remove-Item $pidFile -Force
Write-Host ""
Write-Host "Stopped $stopped process(es)."

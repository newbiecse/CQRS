# Run the Shop frontend (Next.js) dev server on http://localhost:3001

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$shopDir = Join-Path $repoRoot "frontend\shop"

if (-not (Test-Path $shopDir)) {
    Write-Error "Shop frontend not found at $shopDir"
}

Push-Location $shopDir
try {
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing dependencies..."
        if (Get-Command pnpm -ErrorAction SilentlyContinue) {
            pnpm install
        } else {
            npm install
        }
    }

    Write-Host "Starting shop dev server -> http://localhost:3001"
    if (Get-Command pnpm -ErrorAction SilentlyContinue) {
        pnpm dev
    } else {
        npm run dev
    }
}
finally {
    Pop-Location
}

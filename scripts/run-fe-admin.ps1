# Run the Admin frontend (Ant Design Pro) dev server on http://localhost:8000
# Proxies /api/admin -> Shop.Admin.Api (localhost:5100)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$adminDir = Join-Path $repoRoot "frontend\admin"

if (-not (Test-Path $adminDir)) {
    Write-Error "Admin frontend not found at $adminDir"
}

Push-Location $adminDir
try {
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing dependencies (pnpm)..."
        if (Get-Command pnpm -ErrorAction SilentlyContinue) {
            pnpm install
        } else {
            npm install
        }
    }

    Write-Host "Starting admin dev server -> http://localhost:8000"
    if (Get-Command pnpm -ErrorAction SilentlyContinue) {
        pnpm dev
    } else {
        npm run dev
    }
}
finally {
    Pop-Location
}

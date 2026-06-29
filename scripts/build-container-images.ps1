#Requires -Version 5.1
param(
    [string]$Registry = "cqrsdemo",
    [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$DockerfileDotnet = "$RepoRoot\infra\dockerfiles\Dockerfile.dotnet"
$DockerfileDb = "$RepoRoot\infra\dockerfiles\Dockerfile.db-init"
$DockerfileNext = "$RepoRoot\infra\dockerfiles\Dockerfile.nextjs"

function Build-Dotnet([string]$Name, [string]$ProjectPath, [string]$EntryDll) {
    Write-Host "Building $Name..."
    docker build -f $DockerfileDotnet `
        --build-arg "PROJECT_PATH=$ProjectPath" `
        --build-arg "ENTRY_DLL=$EntryDll" `
        -t "${Registry}/${Name}:${Tag}" `
        $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "docker build failed for $Name" }
}

Write-Host "Building db-initializer..."
docker build -f $DockerfileDb -t "${Registry}/db-initializer:${Tag}" $RepoRoot
if ($LASTEXITCODE -ne 0) { throw "docker build failed for db-initializer" }

$services = @(
    @{ Name = "shop-gateway"; Project = "src/Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj"; Dll = "Shop.Gateway.Api.dll" },
    @{ Name = "shop-admin-api"; Project = "src/Gateway/Shop.Admin.Api/Shop.Admin.Api.csproj"; Dll = "Shop.Admin.Api.dll" },
    @{ Name = "product-commands"; Project = "src/Services/Product/Product.Commands.Api/Product.Commands.Api.csproj"; Dll = "Product.Commands.Api.dll" },
    @{ Name = "product-queries"; Project = "src/Services/Product/Product.Queries.Api/Product.Queries.Api.csproj"; Dll = "Product.Queries.Api.dll" },
    @{ Name = "product-projection-worker"; Project = "src/Services/Product/Product.Projection.Worker/Product.Projection.Worker.csproj"; Dll = "Product.Projection.Worker.dll" },
    @{ Name = "cart-commands"; Project = "src/Services/Cart/Cart.Commands.Api/Cart.Commands.Api.csproj"; Dll = "Cart.Commands.Api.dll" },
    @{ Name = "cart-queries"; Project = "src/Services/Cart/Cart.Queries.Api/Cart.Queries.Api.csproj"; Dll = "Cart.Queries.Api.dll" },
    @{ Name = "cart-projection-worker"; Project = "src/Services/Cart/Cart.Projection.Worker/Cart.Projection.Worker.csproj"; Dll = "Cart.Projection.Worker.dll" },
    @{ Name = "order-commands"; Project = "src/Services/Order/Order.Commands.Api/Order.Commands.Api.csproj"; Dll = "Order.Commands.Api.dll" },
    @{ Name = "order-queries"; Project = "src/Services/Order/Order.Queries.Api/Order.Queries.Api.csproj"; Dll = "Order.Queries.Api.dll" },
    @{ Name = "order-projection-worker"; Project = "src/Services/Order/Order.Projection.Worker/Order.Projection.Worker.csproj"; Dll = "Order.Projection.Worker.dll" },
    @{ Name = "order-integration-worker"; Project = "src/Services/Order/Order.Integration.Worker/Order.Integration.Worker.csproj"; Dll = "Order.Integration.Worker.dll" },
    @{ Name = "payment-commands"; Project = "src/Services/Payment/Payment.Commands.Api/Payment.Commands.Api.csproj"; Dll = "Payment.Commands.Api.dll" },
    @{ Name = "payment-queries"; Project = "src/Services/Payment/Payment.Queries.Api/Payment.Queries.Api.csproj"; Dll = "Payment.Queries.Api.dll" },
    @{ Name = "payment-projection-worker"; Project = "src/Services/Payment/Payment.Projection.Worker/Payment.Projection.Worker.csproj"; Dll = "Payment.Projection.Worker.dll" },
    @{ Name = "user-commands"; Project = "src/Services/User/User.Commands.Api/User.Commands.Api.csproj"; Dll = "User.Commands.Api.dll" },
    @{ Name = "user-queries"; Project = "src/Services/User/User.Queries.Api/User.Queries.Api.csproj"; Dll = "User.Queries.Api.dll" },
    @{ Name = "user-projection-worker"; Project = "src/Services/User/User.Projection.Worker/User.Projection.Worker.csproj"; Dll = "User.Projection.Worker.dll" },
    @{ Name = "reporting-queries"; Project = "src/Services/Reporting/Reporting.Queries.Api/Reporting.Queries.Api.csproj"; Dll = "Reporting.Queries.Api.dll" },
    @{ Name = "reporting-projection-worker"; Project = "src/Services/Reporting/Reporting.Projection.Worker/Reporting.Projection.Worker.csproj"; Dll = "Reporting.Projection.Worker.dll" },
    @{ Name = "checkout-saga-api"; Project = "src/Services/Saga/CheckoutSaga.Api/CheckoutSaga.Api.csproj"; Dll = "CheckoutSaga.Api.dll" },
    @{ Name = "checkout-saga-worker"; Project = "src/Services/Saga/CheckoutSaga.Worker/CheckoutSaga.Worker.csproj"; Dll = "CheckoutSaga.Worker.dll" },
    @{ Name = "audit-projection-worker"; Project = "src/Services/Audit/Audit.Projection.Worker/Audit.Projection.Worker.csproj"; Dll = "Audit.Projection.Worker.dll" }
)

foreach ($svc in $services) {
    Build-Dotnet -Name $svc.Name -ProjectPath $svc.Project -EntryDll $svc.Dll
}

Write-Host "Building shop frontend..."
docker build -f $DockerfileNext --build-arg APP_DIR=shop -t "${Registry}/shop:${Tag}" $RepoRoot
if ($LASTEXITCODE -ne 0) { throw "docker build failed for shop" }

Write-Host "Done. Images: ${Registry}/*:${Tag}"

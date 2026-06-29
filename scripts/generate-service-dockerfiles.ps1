# Generates a Dockerfile next to each deployable and infra/docker/images.manifest.json.
# Single source for container image builds (used by build-container-images.ps1 / .sh).

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ManifestPath = Join-Path $RepoRoot 'infra\docker\images.manifest.json'

$DotnetServices = @(
    @{ Name = 'shop-gateway';           Project = 'src/Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj';           EntryDll = 'Shop.Gateway.Api.dll' }
    @{ Name = 'shop-admin-api';         Project = 'src/Gateway/Shop.Admin.Api/Shop.Admin.Api.csproj';             EntryDll = 'Shop.Admin.Api.dll' }
    @{ Name = 'auth-api';               Project = 'src/Services/Auth/Auth.Api/Auth.Api.csproj';                       EntryDll = 'Auth.Api.dll' }
    @{ Name = 'chat-api';               Project = 'src/Services/Chat/Chat.Api/Chat.Api.csproj';                       EntryDll = 'Chat.Api.dll' }
    @{ Name = 'product-commands';       Project = 'src/Services/Product/Product.Commands.Api/Product.Commands.Api.csproj'; EntryDll = 'Product.Commands.Api.dll' }
    @{ Name = 'product-queries';        Project = 'src/Services/Product/Product.Queries.Api/Product.Queries.Api.csproj'; EntryDll = 'Product.Queries.Api.dll' }
    @{ Name = 'product-projection-worker'; Project = 'src/Services/Product/Product.Projection.Worker/Product.Projection.Worker.csproj'; EntryDll = 'Product.Projection.Worker.dll' }
    @{ Name = 'cart-commands';          Project = 'src/Services/Cart/Cart.Commands.Api/Cart.Commands.Api.csproj'; EntryDll = 'Cart.Commands.Api.dll' }
    @{ Name = 'cart-queries';           Project = 'src/Services/Cart/Cart.Queries.Api/Cart.Queries.Api.csproj'; EntryDll = 'Cart.Queries.Api.dll' }
    @{ Name = 'cart-projection-worker'; Project = 'src/Services/Cart/Cart.Projection.Worker/Cart.Projection.Worker.csproj'; EntryDll = 'Cart.Projection.Worker.dll' }
    @{ Name = 'order-commands';         Project = 'src/Services/Order/Order.Commands.Api/Order.Commands.Api.csproj'; EntryDll = 'Order.Commands.Api.dll' }
    @{ Name = 'order-queries';          Project = 'src/Services/Order/Order.Queries.Api/Order.Queries.Api.csproj'; EntryDll = 'Order.Queries.Api.dll' }
    @{ Name = 'order-projection-worker'; Project = 'src/Services/Order/Order.Projection.Worker/Order.Projection.Worker.csproj'; EntryDll = 'Order.Projection.Worker.dll' }
    @{ Name = 'order-integration-worker'; Project = 'src/Services/Order/Order.Integration.Worker/Order.Integration.Worker.csproj'; EntryDll = 'Order.Integration.Worker.dll' }
    @{ Name = 'payment-commands';      Project = 'src/Services/Payment/Payment.Commands.Api/Payment.Commands.Api.csproj'; EntryDll = 'Payment.Commands.Api.dll' }
    @{ Name = 'payment-queries';       Project = 'src/Services/Payment/Payment.Queries.Api/Payment.Queries.Api.csproj'; EntryDll = 'Payment.Queries.Api.dll' }
    @{ Name = 'payment-projection-worker'; Project = 'src/Services/Payment/Payment.Projection.Worker/Payment.Projection.Worker.csproj'; EntryDll = 'Payment.Projection.Worker.dll' }
    @{ Name = 'user-commands';         Project = 'src/Services/User/User.Commands.Api/User.Commands.Api.csproj'; EntryDll = 'User.Commands.Api.dll' }
    @{ Name = 'user-queries';          Project = 'src/Services/User/User.Queries.Api/User.Queries.Api.csproj'; EntryDll = 'User.Queries.Api.dll' }
    @{ Name = 'user-projection-worker'; Project = 'src/Services/User/User.Projection.Worker/User.Projection.Worker.csproj'; EntryDll = 'User.Projection.Worker.dll' }
    @{ Name = 'inventory-commands';    Project = 'src/Services/Inventory/Inventory.Commands.Api/Inventory.Commands.Api.csproj'; EntryDll = 'Inventory.Commands.Api.dll' }
    @{ Name = 'inventory-queries';     Project = 'src/Services/Inventory/Inventory.Queries.Api/Inventory.Queries.Api.csproj'; EntryDll = 'Inventory.Queries.Api.dll' }
    @{ Name = 'inventory-integration-worker'; Project = 'src/Services/Inventory/Inventory.Integration.Worker/Inventory.Integration.Worker.csproj'; EntryDll = 'Inventory.Integration.Worker.dll' }
    @{ Name = 'reporting-queries';     Project = 'src/Services/Reporting/Reporting.Queries.Api/Reporting.Queries.Api.csproj'; EntryDll = 'Reporting.Queries.Api.dll' }
    @{ Name = 'reporting-projection-worker'; Project = 'src/Services/Reporting/Reporting.Projection.Worker/Reporting.Projection.Worker.csproj'; EntryDll = 'Reporting.Projection.Worker.dll' }
    @{ Name = 'checkout-saga-api';      Project = 'src/Services/Saga/CheckoutSaga.Api/CheckoutSaga.Api.csproj'; EntryDll = 'CheckoutSaga.Api.dll' }
    @{ Name = 'checkout-saga-worker';  Project = 'src/Services/Saga/CheckoutSaga.Worker/CheckoutSaga.Worker.csproj'; EntryDll = 'CheckoutSaga.Worker.dll' }
    @{ Name = 'audit-projection-worker'; Project = 'src/Services/Audit/Audit.Projection.Worker/Audit.Projection.Worker.csproj'; EntryDll = 'Audit.Projection.Worker.dll' }
)

function New-DotnetServiceDockerfile {
    param(
        [string]$ImageName,
        [string]$PublishProject,
        [string]$EntryDll,
        [string]$DockerfilePath
    )

    $relativeDockerfile = $DockerfilePath.Replace($RepoRoot + '\', '').Replace('\', '/')
    $content = @"
# Generated by scripts/generate-service-dockerfiles.ps1 - edit the generator, not this file.
# image: cqrsdemo/$ImageName
# Build from repository root:
#   docker build -f $relativeDockerfile -t cqrsdemo/${ImageName}:latest .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/CqrsDemo.Distributed.sln ./
COPY backend/src/ ./src/
COPY backend/tools/ ./tools/

RUN dotnet publish "./$PublishProject" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "$EntryDll"]
"@

    $dir = Split-Path -Parent $DockerfilePath
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Set-Content -Path $DockerfilePath -Value $content.TrimEnd() -Encoding UTF8
}

$manifestImages = [System.Collections.Generic.List[object]]::new()

foreach ($svc in $DotnetServices) {
    $projectDir = Split-Path -Parent $svc.Project
    $dockerfilePath = Join-Path $RepoRoot "backend\$($projectDir -replace '/', '\')\Dockerfile"
    New-DotnetServiceDockerfile -ImageName $svc.Name -PublishProject $svc.Project -EntryDll $svc.EntryDll -DockerfilePath $dockerfilePath
    $manifestImages.Add([ordered]@{
        name       = $svc.Name
        dockerfile = "backend/$projectDir/Dockerfile".Replace('\', '/')
    })
    Write-Host "Generated $($manifestImages[-1].dockerfile)"
}

# Database initializer
$dbDockerfile = Join-Path $RepoRoot 'backend\tools\CqrsDemo.DatabaseInitializer\Dockerfile'
$dbContent = @'
# Generated by scripts/generate-service-dockerfiles.ps1
# image: cqrsdemo/db-initializer
# Build from repository root:
#   docker build -f backend/tools/CqrsDemo.DatabaseInitializer/Dockerfile -t cqrsdemo/db-initializer:latest .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/CqrsDemo.Distributed.sln ./
COPY backend/src/ ./src/
COPY backend/tools/ ./tools/

RUN dotnet publish "./tools/CqrsDemo.DatabaseInitializer/CqrsDemo.DatabaseInitializer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CqrsDemo.DatabaseInitializer.dll"]
'@
Set-Content -Path $dbDockerfile -Value $dbContent.TrimEnd() -Encoding UTF8
$manifestImages.Add([ordered]@{ name = 'db-initializer'; dockerfile = 'backend/tools/CqrsDemo.DatabaseInitializer/Dockerfile' })
Write-Host 'Generated backend/tools/CqrsDemo.DatabaseInitializer/Dockerfile'

# Shop frontend
$shopDockerfile = Join-Path $RepoRoot 'frontend\shop\Dockerfile'
$shopContent = @'
# Generated by scripts/generate-service-dockerfiles.ps1
# image: cqrsdemo/shop
# Build from repository root:
#   docker build -f frontend/shop/Dockerfile -t cqrsdemo/shop:latest .

FROM node:22-alpine AS deps
WORKDIR /app
COPY frontend/shop/package.json frontend/shop/package-lock.json ./
RUN npm ci

FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY frontend/shop/ ./
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM node:22-alpine AS final
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

COPY --from=build /app/public ./public
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static

EXPOSE 3000
CMD ["node", "server.js"]
'@
Set-Content -Path $shopDockerfile -Value $shopContent.TrimEnd() -Encoding UTF8
$manifestImages.Add([ordered]@{ name = 'shop'; dockerfile = 'frontend/shop/Dockerfile' })
Write-Host 'Generated frontend/shop/Dockerfile'

$manifest = @{ images = $manifestImages.ToArray() }
$manifest | ConvertTo-Json -Depth 5 | Set-Content -Path $ManifestPath -Encoding UTF8
Write-Host ""
Write-Host "Wrote $ManifestPath ($($manifestImages.Count) images)"

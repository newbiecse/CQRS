#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
REGISTRY="${REGISTRY:-cqrsdemo}"
TAG="${TAG:-latest}"

build_dotnet() {
  docker build -f "$ROOT/infra/dockerfiles/Dockerfile.dotnet" \
    --build-arg "PROJECT_PATH=$2" \
    --build-arg "ENTRY_DLL=$3" \
    -t "${REGISTRY}/$1:${TAG}" \
    "$ROOT"
}

echo "Building db-initializer..."
docker build -f "$ROOT/infra/dockerfiles/Dockerfile.db-init" \
  -t "${REGISTRY}/db-initializer:${TAG}" "$ROOT"

declare -a SERVICES=(
  "shop-gateway|src/Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj|Shop.Gateway.Api.dll"
  "shop-admin-api|src/Gateway/Shop.Admin.Api/Shop.Admin.Api.csproj|Shop.Admin.Api.dll"
  "product-commands|src/Services/Product/Product.Commands.Api/Product.Commands.Api.csproj|Product.Commands.Api.dll"
  "product-queries|src/Services/Product/Product.Queries.Api/Product.Queries.Api.csproj|Product.Queries.Api.dll"
  "product-projection-worker|src/Services/Product/Product.Projection.Worker/Product.Projection.Worker.csproj|Product.Projection.Worker.dll"
  "cart-commands|src/Services/Cart/Cart.Commands.Api/Cart.Commands.Api.csproj|Cart.Commands.Api.dll"
  "cart-queries|src/Services/Cart/Cart.Queries.Api/Cart.Queries.Api.csproj|Cart.Queries.Api.dll"
  "cart-projection-worker|src/Services/Cart/Cart.Projection.Worker/Cart.Projection.Worker.csproj|Cart.Projection.Worker.dll"
  "order-commands|src/Services/Order/Order.Commands.Api/Order.Commands.Api.csproj|Order.Commands.Api.dll"
  "order-queries|src/Services/Order/Order.Queries.Api/Order.Queries.Api.csproj|Order.Queries.Api.dll"
  "order-projection-worker|src/Services/Order/Order.Projection.Worker/Order.Projection.Worker.csproj|Order.Projection.Worker.dll"
  "order-integration-worker|src/Services/Order/Order.Integration.Worker/Order.Integration.Worker.csproj|Order.Integration.Worker.dll"
  "payment-commands|src/Services/Payment/Payment.Commands.Api/Payment.Commands.Api.csproj|Payment.Commands.Api.dll"
  "payment-queries|src/Services/Payment/Payment.Queries.Api/Payment.Queries.Api.csproj|Payment.Queries.Api.dll"
  "payment-projection-worker|src/Services/Payment/Payment.Projection.Worker/Payment.Projection.Worker.csproj|Payment.Projection.Worker.dll"
  "user-commands|src/Services/User/User.Commands.Api/User.Commands.Api.csproj|User.Commands.Api.dll"
  "user-queries|src/Services/User/User.Queries.Api/User.Queries.Api.csproj|User.Queries.Api.dll"
  "user-projection-worker|src/Services/User/User.Projection.Worker/User.Projection.Worker.csproj|User.Projection.Worker.dll"
  "reporting-queries|src/Services/Reporting/Reporting.Queries.Api/Reporting.Queries.Api.csproj|Reporting.Queries.Api.dll"
  "reporting-projection-worker|src/Services/Reporting/Reporting.Projection.Worker/Reporting.Projection.Worker.csproj|Reporting.Projection.Worker.dll"
  "checkout-saga-api|src/Services/Saga/CheckoutSaga.Api/CheckoutSaga.Api.csproj|CheckoutSaga.Api.dll"
  "checkout-saga-worker|src/Services/Saga/CheckoutSaga.Worker/CheckoutSaga.Worker.csproj|CheckoutSaga.Worker.dll"
  "audit-projection-worker|src/Services/Audit/Audit.Projection.Worker/Audit.Projection.Worker.csproj|Audit.Projection.Worker.dll"
)

for entry in "${SERVICES[@]}"; do
  IFS='|' read -r name project dll <<< "$entry"
  echo "Building ${name}..."
  build_dotnet "$name" "$project" "$dll"
done

echo "Building shop frontend..."
docker build -f "$ROOT/infra/dockerfiles/Dockerfile.nextjs" \
  --build-arg APP_DIR=shop \
  -t "${REGISTRY}/shop:${TAG}" "$ROOT"

echo "Done. Images: ${REGISTRY}/*:${TAG}"

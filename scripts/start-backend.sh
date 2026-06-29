#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOG_DIR="$ROOT/scripts/logs"
PID_FILE="$ROOT/scripts/.backend.pids"
mkdir -p "$LOG_DIR"

if [[ -f "$PID_FILE" ]]; then
  echo "Backend may already be running ($PID_FILE exists). Run ./scripts/stop-backend.sh first." >&2
  exit 1
fi

if [[ ! -f "$ROOT/backend/src/Gateway/Shop.Admin.Api/bin/Release/net8.0/Shop.Admin.Api.dll" ]]; then
  echo "Release build not found. Running build-backend.sh ..."
  "$ROOT/scripts/build-backend.sh"
fi

# name|project-path (keep in sync with _backend-services.ps1)
SERVICES=(
  "audit-projection|backend/src/Services/Audit/Audit.Projection.Worker/Audit.Projection.Worker.csproj"
  "reporting-projection|backend/src/Services/Reporting/Reporting.Projection.Worker/Reporting.Projection.Worker.csproj"
  "user-projection|backend/src/Services/User/User.Projection.Worker/User.Projection.Worker.csproj"
  "product-projection|backend/src/Services/Product/Product.Projection.Worker/Product.Projection.Worker.csproj"
  "cart-projection|backend/src/Services/Cart/Cart.Projection.Worker/Cart.Projection.Worker.csproj"
  "order-projection|backend/src/Services/Order/Order.Projection.Worker/Order.Projection.Worker.csproj"
  "order-integration|backend/src/Services/Order/Order.Integration.Worker/Order.Integration.Worker.csproj"
  "payment-projection|backend/src/Services/Payment/Payment.Projection.Worker/Payment.Projection.Worker.csproj"
  "checkout-saga-worker|backend/src/Services/Saga/CheckoutSaga.Worker/CheckoutSaga.Worker.csproj"
  "inventory-integration|backend/src/Services/Inventory/Inventory.Integration.Worker/Inventory.Integration.Worker.csproj"
  "product-commands|backend/src/Services/Product/Product.Commands.Api/Product.Commands.Api.csproj"
  "product-queries|backend/src/Services/Product/Product.Queries.Api/Product.Queries.Api.csproj"
  "cart-commands|backend/src/Services/Cart/Cart.Commands.Api/Cart.Commands.Api.csproj"
  "cart-queries|backend/src/Services/Cart/Cart.Queries.Api/Cart.Queries.Api.csproj"
  "order-commands|backend/src/Services/Order/Order.Commands.Api/Order.Commands.Api.csproj"
  "order-queries|backend/src/Services/Order/Order.Queries.Api/Order.Queries.Api.csproj"
  "payment-commands|backend/src/Services/Payment/Payment.Commands.Api/Payment.Commands.Api.csproj"
  "payment-queries|backend/src/Services/Payment/Payment.Queries.Api/Payment.Queries.Api.csproj"
  "user-commands|backend/src/Services/User/User.Commands.Api/User.Commands.Api.csproj"
  "user-queries|backend/src/Services/User/User.Queries.Api/User.Queries.Api.csproj"
  "inventory-commands|backend/src/Services/Inventory/Inventory.Commands.Api/Inventory.Commands.Api.csproj"
  "inventory-queries|backend/src/Services/Inventory/Inventory.Queries.Api/Inventory.Queries.Api.csproj"
  "reporting-queries|backend/src/Services/Reporting/Reporting.Queries.Api/Reporting.Queries.Api.csproj"
  "checkout-saga-api|backend/src/Services/Saga/CheckoutSaga.Api/CheckoutSaga.Api.csproj"
  "shop-admin-api|backend/src/Gateway/Shop.Admin.Api/Shop.Admin.Api.csproj"
  "shop-gateway-api|backend/src/Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj"
)

: > "$PID_FILE"
count=0
for entry in "${SERVICES[@]}"; do
  name="${entry%%|*}"
  project="${entry#*|}"
  project_path="$ROOT/$project"
  if [[ ! -f "$project_path" ]]; then
    echo "Skipping missing project: $project"
    continue
  fi
  log="$LOG_DIR/$name.log"
  nohup dotnet run --no-build -c Release --project "$project_path" >"$log" 2>&1 &
  pid=$!
  echo "$name $pid" >> "$PID_FILE"
  echo "Started $name PID $pid"
  count=$((count + 1))
  sleep 0.4
done

echo ""
echo "Started $count backend processes."
echo "  PID file : $PID_FILE"
echo "  Logs     : $LOG_DIR"
echo "  Admin BFF: http://localhost:5100"
echo "  Shop GW  : http://localhost:5000"
echo ""
echo "Stop with: ./scripts/stop-backend.sh"

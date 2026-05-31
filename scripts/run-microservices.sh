#!/usr/bin/env bash
set -euo pipefail

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "Starting SQL Server + Azure Service Bus Emulator (docker compose)..."
docker compose up -d

echo "Waiting for SQL Server..."
sleep 20

echo "Run these in separate terminals:"
echo "  dotnet run --project src/CqrsDemo.Commands.Api"
echo "  dotnet run --project src/CqrsDemo.Projection.Worker"
echo "  dotnet run --project src/CqrsDemo.Queries.Api"

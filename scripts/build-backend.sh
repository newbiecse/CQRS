#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SLN="$ROOT/backend/CqrsDemo.Distributed.sln"

echo "Restoring NuGet packages..."
dotnet restore "$SLN"

echo "Building solution (Release)..."
dotnet build "$SLN" -c Release --no-restore

echo "Building database initializer..."
dotnet build "$ROOT/backend/tools/CqrsDemo.DatabaseInitializer/CqrsDemo.DatabaseInitializer.csproj" -c Release

echo ""
echo "Backend build complete."

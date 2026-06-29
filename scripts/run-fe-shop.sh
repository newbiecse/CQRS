#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SHOP_DIR="$ROOT/frontend/shop"

cd "$SHOP_DIR"

if [[ ! -d node_modules ]]; then
  echo "Installing dependencies..."
  if command -v pnpm >/dev/null 2>&1; then pnpm install; else npm install; fi
fi

echo "Starting shop dev server -> http://localhost:3001"
if command -v pnpm >/dev/null 2>&1; then pnpm dev; else npm run dev; fi

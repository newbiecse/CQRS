#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ADMIN_DIR="$ROOT/frontend/admin"

cd "$ADMIN_DIR"

if [[ ! -d node_modules ]]; then
  echo "Installing dependencies..."
  if command -v pnpm >/dev/null 2>&1; then pnpm install; else npm install; fi
fi

echo "Starting admin dev server -> http://localhost:8000"
if command -v pnpm >/dev/null 2>&1; then pnpm dev; else npm run dev; fi

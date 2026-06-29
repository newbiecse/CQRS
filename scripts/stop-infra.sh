#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$ROOT/infra/docker/docker-compose.yml"

command -v docker >/dev/null 2>&1 || { echo "Docker is not installed or not on PATH." >&2; exit 1; }

echo "Stopping infrastructure (docker compose down)..."
docker compose -f "$COMPOSE_FILE" down

echo "Infrastructure stopped."

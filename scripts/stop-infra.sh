#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ENVIRONMENT="${1:-dev}"
# shellcheck source=scripts/_infra-env.sh
source "$ROOT/scripts/_infra-env.sh"

command -v docker >/dev/null 2>&1 || { echo "Docker is not installed or not on PATH." >&2; exit 1; }

echo "Stopping infrastructure (docker compose down, environment: $ENVIRONMENT)..."
docker_compose_env "$ENVIRONMENT" down

echo "Infrastructure stopped."

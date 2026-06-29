#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ENVIRONMENT="${1:-dev}"
# shellcheck source=scripts/_infra-env.sh
source "$ROOT/scripts/_infra-env.sh"

command -v docker >/dev/null 2>&1 || { echo "Docker is not installed or not on PATH." >&2; exit 1; }

echo "Starting infrastructure (docker compose, environment: $ENVIRONMENT)..."
docker_compose_env "$ENVIRONMENT" up -d

echo ""
echo "Waiting for containers (30s)..."
sleep 30

docker_compose_env "$ENVIRONMENT" ps
echo ""
echo "Infrastructure started ($ENVIRONMENT)."
echo "  SQL Server     localhost:1433"
echo "  Kafka          localhost:9092"
echo "  Elasticsearch  localhost:9200"
echo "  Kibana         http://localhost:5601"
echo "  OTLP gRPC      localhost:4317"
echo ""
echo "Copy infra/docker/env/$ENVIRONMENT/.env.example to .env to override secrets."
echo "Next: ./scripts/initialize-databases.ps1"

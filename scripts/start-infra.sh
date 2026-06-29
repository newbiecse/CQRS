#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$ROOT/infra/docker/docker-compose.yml"

command -v docker >/dev/null 2>&1 || { echo "Docker is not installed or not on PATH." >&2; exit 1; }

echo "Starting infrastructure (docker compose)..."
docker compose -f "$COMPOSE_FILE" up -d

echo ""
echo "Waiting for containers (30s)..."
sleep 30

docker compose -f "$COMPOSE_FILE" ps
echo ""
echo "Infrastructure started."
echo "  SQL Server     localhost:1433  (sa / Your_password123)"
echo "  Kafka          localhost:9092"
echo "  Elasticsearch  localhost:9200"
echo "  Kibana         http://localhost:5601"
echo "  OTLP gRPC      localhost:4317"
echo ""
echo "Next: ./scripts/initialize-databases.ps1  (or dotnet run database initializer)"

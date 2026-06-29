#!/usr/bin/env bash
# Shared paths for dev / staging / prod infrastructure.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

infra_environment() {
  local env="${1:-dev}"
  case "$env" in
    dev|staging|prod) ;;
    *) echo "environment must be dev, staging, or prod" >&2; return 1 ;;
  esac

  local namespace
  case "$env" in
    dev) namespace="cqrs-demo" ;;
    staging) namespace="cqrs-demo-staging" ;;
    prod) namespace="cqrs-demo-prod" ;;
  esac

  echo "ENVIRONMENT=$env"
  echo "NAMESPACE=$namespace"
  echo "DOCKER_COMPOSE_BASE=$ROOT/infra/docker/base/docker-compose.yml"
  echo "DOCKER_COMPOSE_OVERRIDE=$ROOT/infra/docker/env/$env/docker-compose.override.yml"
  echo "DOCKER_ENV_FILE=$ROOT/infra/docker/env/$env/.env"
  echo "K8S_OVERLAY=$ROOT/infra/k8s/overlays/$env"
  echo "HELM_CHART=$ROOT/infra/helm/cqrs-apps"
  echo "HELMFILE=$ROOT/infra/helm/helmfile.yaml"
  echo "HELM_VALUES_BASE=$ROOT/infra/helm/cqrs-apps/values.yaml"
  echo "HELM_VALUES_ENV=$ROOT/infra/helm/cqrs-apps/values-$env.yaml"
  echo "TERRAFORM_TFVARS=$ROOT/infra/terraform/env/$env.tfvars"
}

docker_compose_env() {
  local env="${1:-dev}"
  shift || true
  local base="$ROOT/infra/docker/base/docker-compose.yml"
  local override="$ROOT/infra/docker/env/$env/docker-compose.override.yml"
  local env_file="$ROOT/infra/docker/env/$env/.env"
  local args=(-f "$base" -f "$override")
  if [[ -f "$env_file" ]]; then
    args+=(--env-file "$env_file")
  fi
  docker compose "${args[@]}" "$@"
}

#!/usr/bin/env bash
# Deploy CQRS Demo to a local Kubernetes cluster (kind, minikube, or Docker Desktop).
# Used by GitHub Actions and Linux/macOS developers.

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=scripts/_infra-env.sh
source "$ROOT/scripts/_infra-env.sh"

ENVIRONMENT="${ENVIRONMENT:-dev}"
REGISTRY="${REGISTRY:-cqrsdemo}"
TAG="${TAG:-latest}"
SKIP_BUILD="${SKIP_BUILD:-false}"
SKIP_INGRESS="${SKIP_INGRESS:-false}"
SKIP_APPS="${SKIP_APPS:-false}"
INFRA_ONLY="${INFRA_ONLY:-false}"
CLUSTER_PROVIDER="${CLUSTER_PROVIDER:-existing}" # existing | kind

eval "$(infra_environment "$ENVIRONMENT")"

step() { echo ""; echo "==> $*"; }

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || { echo "Required command not found: $1" >&2; exit 1; }
}

cluster_flavor() {
  local ctx
  ctx="$(kubectl config current-context 2>/dev/null || true)"
  case "$ctx" in
    *docker-desktop*) echo docker-desktop ;;
    *minikube*) echo minikube ;;
    kind-*) echo kind ;;
    *) echo unknown ;;
  esac
}

ensure_kind_cluster() {
  if [[ "$CLUSTER_PROVIDER" != "kind" ]]; then
    return 0
  fi

  step "Ensuring kind cluster (cqrs-demo)..."
  if kind get clusters 2>/dev/null | grep -qx 'cqrs-demo'; then
    echo "kind cluster cqrs-demo already exists."
  else
    kind create cluster --name cqrs-demo --config "$ROOT/infra/k8s/kind/kind-config.yaml" --wait 5m
  fi
  kubectl cluster-info --context "kind-cqrs-demo"
}

install_ingress_nginx() {
  if [[ "$SKIP_INGRESS" == "true" ]]; then
    return 0
  fi

  if helm status ingress-nginx -n ingress-nginx &>/dev/null; then
    echo "ingress-nginx already installed."
    return 0
  fi

  step "Installing ingress-nginx..."
  helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx 2>/dev/null || true
  helm repo update
  helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
    --namespace ingress-nginx \
    --create-namespace \
    --set controller.watchIngressWithoutClass=true \
    --set controller.ingressClassResource.default=true \
    --wait --timeout 5m
}

load_images_kind() {
  local flavor img
  flavor="$(cluster_flavor)"
  [[ "$flavor" == "kind" ]] || return 0

  step "Loading images into kind..."
  local images=(
    db-initializer
    shop-gateway shop-admin-api shop
    product-commands product-queries product-projection-worker
    cart-commands cart-queries cart-projection-worker
    order-commands order-queries order-projection-worker order-integration-worker
    payment-commands payment-queries payment-projection-worker
    user-commands user-queries user-projection-worker
    reporting-queries reporting-projection-worker
    checkout-saga-api checkout-saga-worker
    audit-projection-worker
  )
  for img in "${images[@]}"; do
    kind load docker-image "${REGISTRY}/${img}:${TAG}" --name cqrs-demo
  done
}

load_images_minikube() {
  local flavor img
  flavor="$(cluster_flavor)"
  [[ "$flavor" == "minikube" ]] || return 0

  step "Loading images into minikube..."
  local images=(
    db-initializer
    shop-gateway shop-admin-api shop
    product-commands product-queries product-projection-worker
    cart-commands cart-queries cart-projection-worker
    order-commands order-queries order-projection-worker order-integration-worker
    payment-commands payment-queries payment-projection-worker
    user-commands user-queries user-projection-worker
    reporting-queries reporting-projection-worker
    checkout-saga-api checkout-saga-worker
    audit-projection-worker
  )
  for img in "${images[@]}"; do
    minikube image load "${REGISTRY}/${img}:${TAG}"
  done
}

deploy_infra() {
  step "Applying platform infrastructure (Kustomize, environment: $ENVIRONMENT)..."
  kubectl apply -k "$K8S_OVERLAY"

  step "Waiting for SQL Server (up to 10m)..."
  kubectl wait --for=condition=ready pod -l app=sqlserver -n "$NAMESPACE" --timeout=600s

  step "Waiting for Elasticsearch..."
  kubectl wait --for=condition=ready pod -l app=elasticsearch -n "$NAMESPACE" --timeout=600s

  step "Waiting for Kafka..."
  kubectl wait --for=condition=ready pod -l app=kafka -n "$NAMESPACE" --timeout=600s
}

deploy_apps() {
  step "Installing microservices (Helm, environment: $ENVIRONMENT)..."

  if command -v helmfile >/dev/null 2>&1; then
    pushd "$ROOT/infra/helm" >/dev/null
    helmfile -e "$ENVIRONMENT" apply \
      --set "image.registry=${REGISTRY}" \
      --set "image.tag=${TAG}" \
      --set "sql.password=Your_password123" \
      --set "dbInit.image=${REGISTRY}/db-initializer"
    popd >/dev/null
  else
    helm upgrade --install cqrs-apps "$HELM_CHART" \
      --namespace "$NAMESPACE" \
      --create-namespace \
      -f "$HELM_VALUES_BASE" \
      -f "$HELM_VALUES_ENV" \
      --set "image.registry=${REGISTRY}" \
      --set "image.tag=${TAG}" \
      --set "sql.password=Your_password123" \
      --set "dbInit.image=${REGISTRY}/db-initializer" \
      --wait --timeout 15m
  fi

  if [[ "$ENVIRONMENT" != "prod" ]]; then
    step "Waiting for database initializer job..."
    kubectl wait --for=condition=complete job/db-initializer -n "$NAMESPACE" --timeout=600s
  fi
}

smoke_test() {
  step "Smoke test — shop-gateway pod ready"
  kubectl wait --for=condition=ready pod -l app=shop-gateway -n "$NAMESPACE" --timeout=300s
  kubectl get pods -n "$NAMESPACE"
  echo ""
  echo "NodePort (kind / local):"
  echo "  Shop gateway: http://localhost:30500"
  echo "  Admin API:    http://localhost:30510"
  echo "  Shop UI:      http://localhost:30501"
}

# --- main ---
step "CQRS Demo — local Kubernetes deploy ($ENVIRONMENT)"
require_cmd kubectl
require_cmd helm
require_cmd docker

ensure_kind_cluster
kubectl cluster-info

install_ingress_nginx
deploy_infra

if [[ "$INFRA_ONLY" == "true" ]]; then
  kubectl get pods -n "$NAMESPACE"
  exit 0
fi

if [[ "$SKIP_BUILD" != "true" ]]; then
  flavor="$(cluster_flavor)"
  if [[ "$flavor" == "minikube" ]]; then
    # shellcheck disable=SC1091
    eval "$(minikube docker-env)"
  fi
  REGISTRY="$REGISTRY" TAG="$TAG" "$ROOT/scripts/build-container-images.sh"
  load_images_kind
  load_images_minikube
fi

if [[ "$SKIP_APPS" != "true" ]]; then
  deploy_apps
  smoke_test
fi

echo ""
echo "Deploy complete. Namespace: $NAMESPACE"

#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROVIDER="${PROVIDER:-auto}"
DASHBOARD_PORT="${DASHBOARD_PORT:-8443}"
SKIP_DASHBOARD=false
SKIP_BROWSER=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --provider) PROVIDER="$2"; shift 2 ;;
    --skip-dashboard) SKIP_DASHBOARD=true; shift ;;
    --skip-browser) SKIP_BROWSER=true; shift ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

step() { echo ""; echo "==> $1"; }

cluster_ready() { kubectl cluster-info >/dev/null 2>&1; }

resolve_provider() {
  if [[ "$PROVIDER" != "auto" ]]; then echo "$PROVIDER"; return; fi
  if command -v minikube >/dev/null && [[ "$(minikube status --format='{{.Host}}' 2>/dev/null || true)" == "Running" ]]; then
    echo "minikube"; return
  fi
  ctx="$(kubectl config current-context 2>/dev/null || true)"
  if [[ "$ctx" == *docker-desktop* ]]; then echo "docker-desktop"; return; fi
  if command -v minikube >/dev/null; then echo "minikube"; return; fi
  echo "docker-desktop"
}

start_minikube() {
  step "Starting minikube..."
  if [[ "$(minikube status --format='{{.Host}}' 2>/dev/null || true)" != "Running" ]]; then
    minikube start --memory=16384 --cpus=4
  fi
}

start_docker_desktop() {
  step "Starting Docker Desktop Kubernetes..."
  docker desktop start 2>/dev/null || true
  docker desktop kubernetes enable 2>/dev/null || true
  for _ in $(seq 1 60); do
    cluster_ready && return 0
    sleep 5
  done
  echo "Enable Kubernetes in Docker Desktop: Settings -> Kubernetes -> Enable"
  exit 1
}

install_dashboard_helm() {
  step "Installing Kubernetes Dashboard (Helm)..."
  helm repo add kubernetes-dashboard https://kubernetes.github.io/dashboard/ 2>/dev/null || true
  helm repo update
  helm upgrade --install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard \
    --namespace kubernetes-dashboard --create-namespace --wait --timeout 5m
  kubectl apply -k "$ROOT/infra/k8s/base/dashboard"
}

start_minikube_dashboard() {
  step "Enabling minikube dashboard..."
  minikube addons enable dashboard
  kubectl apply -k "$ROOT/infra/k8s/base/dashboard" 2>/dev/null || true
  URL="$(minikube dashboard --url 2>/dev/null || true)"
  if [[ -n "$URL" && "$SKIP_BROWSER" == false ]]; then
    echo "Dashboard: $URL"
    command -v xdg-open >/dev/null && xdg-open "$URL" || open "$URL" 2>/dev/null || true
  else
    minikube dashboard &
  fi
}

start_helm_dashboard() {
  install_dashboard_helm
  SVC="$(kubectl get svc -n kubernetes-dashboard -o jsonpath='{.items[0].metadata.name}')"
  step "Port-forward dashboard on https://localhost:$DASHBOARD_PORT"
  kubectl port-forward -n kubernetes-dashboard "svc/$SVC" "$DASHBOARD_PORT:443" &
  sleep 2
  echo "Token: kubectl -n kubernetes-dashboard create token admin-user"
  if [[ "$SKIP_BROWSER" == false ]]; then
    command -v xdg-open >/dev/null && xdg-open "https://localhost:$DASHBOARD_PORT" || open "https://localhost:$DASHBOARD_PORT" 2>/dev/null || true
  fi
}

step "Start local Kubernetes + Dashboard"
RESOLVED="$(resolve_provider)"
echo "Provider: $RESOLVED"

case "$RESOLVED" in
  minikube) start_minikube ;;
  *) start_docker_desktop ;;
esac

if [[ "$SKIP_DASHBOARD" == false ]]; then
  if [[ "$RESOLVED" == "minikube" ]]; then start_minikube_dashboard; else start_helm_dashboard; fi
fi

step "Next: ./scripts/deploy-local-k8s.sh (or deploy-local-k8s.ps1 on Windows)"

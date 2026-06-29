#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
REGISTRY="${REGISTRY:-cqrsdemo}"
TAG="${TAG:-latest}"
MANIFEST="$ROOT/infra/docker/images.manifest.json"

if [[ ! -f "$MANIFEST" ]]; then
  echo "Manifest not found. Run: pwsh scripts/generate-service-dockerfiles.ps1" >&2
  exit 1
fi

command -v jq >/dev/null 2>&1 || { echo "jq is required (apt install jq / brew install jq)" >&2; exit 1; }

build_image() {
  local name="$1"
  local dockerfile="$2"
  echo "Building ${name}..."
  docker build -f "$ROOT/$dockerfile" -t "${REGISTRY}/${name}:${TAG}" "$ROOT"
}

while IFS=$'\t' read -r name dockerfile; do
  build_image "$name" "$dockerfile"
done < <(jq -r '.images[] | [.name, .dockerfile] | @tsv' "$MANIFEST")

echo "Done. Images: ${REGISTRY}/*:${TAG}"

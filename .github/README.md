# GitHub Actions

## Workflows

| Workflow | File | Trigger | Purpose |
|----------|------|---------|---------|
| **CI** | [ci.yml](workflows/ci.yml) | Push / PR to `main` | Build backend, admin, shop, and Docker images |
| **Local Kubernetes** | [local-k8s.yml](workflows/local-k8s.yml) | Push to `main` (infra/backend paths), manual | Build images and deploy to kind (local-style K8s) |

## Local Kubernetes workflow

### GitHub-hosted (default) — kind

Creates an ephemeral **kind** cluster with the same NodePort mappings as local dev (`30500`, `30510`, `30501`):

1. `dotnet build` backend
2. `kind` cluster via [helm/kind-action](https://github.com/helm/kind-action)
3. `./scripts/build-container-images.sh`
4. `kind load docker-image` → `./scripts/ci-local-k8s.sh`
5. HTTP smoke test against `http://localhost:30500`

**Manual run:** Actions → Local Kubernetes → Run workflow.

### Self-hosted — Docker Desktop / minikube

Use a [self-hosted runner](https://docs.github.com/en/actions/hosting-your-own-runners) on your machine with Kubernetes enabled:

1. Run workflow with **cluster_provider = existing**
2. Ensure `kubectl` points at your local cluster
3. Images are built locally and loaded into kind/minikube automatically when detected

Or run the same script directly:

```bash
export CLUSTER_PROVIDER=existing
export ENVIRONMENT=dev
export REGISTRY=cqrsdemo
export TAG=latest
./scripts/ci-local-k8s.sh
```

### Linux / macOS (no Actions)

```bash
export CLUSTER_PROVIDER=kind   # or existing
./scripts/ci-local-k8s.sh
```

Kind config: [infra/k8s/kind/kind-config.yaml](../infra/k8s/kind/kind-config.yaml)

## CI workflow jobs

- **backend** — `dotnet build` solution + database initializer
- **frontend-admin** — `npm ci` + `npm run build` (Ant Design Pro)
- **frontend-shop** — `npm ci` + `npm run build` (Next.js)
- **docker-images** — builds all `cqrsdemo/*` container images (after backend job)

## Notes

- Local K8s deploy can take **30–60+ minutes** (SQL Server + Elasticsearch startup on kind).
- Image tag in CI: `ci-<git-sha>`.
- Secrets remain demo defaults (`Your_password123`) — not for production.

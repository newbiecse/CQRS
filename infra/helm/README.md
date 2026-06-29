# Helm deployment (industry-standard layout)

This follows the **layered values** pattern used by most production Helm deployments:

```
helm/
├── helmfile.yaml              # Multi-env orchestration (Helmfile)
└── cqrs-apps/
    ├── Chart.yaml
    ├── values.yaml            # Shared defaults + full service catalog
    ├── values-dev.yaml        # Dev overrides only
    ├── values-staging.yaml    # Staging overrides only
    ├── values-prod.yaml       # Production overrides only
    └── templates/
```

## Why this layout

| Pattern | Used by |
|---------|---------|
| `values.yaml` + `values-{env}.yaml` | Default Helm multi-env (CNCF, most charts) |
| [Helmfile](https://helmfile.readthedocs.io/) | Teams managing many releases / environments |
| Single chart, generic templates | Homogeneous microservices (same Deployment shape) |
| GitOps (Argo CD / Flux) | Large orgs — point Application at this chart + env values file |

Per-service **folder per environment** (84+ files) is **not** common at scale — it duplicates config and drifts easily. Service catalog lives once in `values.yaml`; environments only override what differs.

## Deploy

**Helmfile (recommended):**

```bash
cd infra/helm
helmfile -e dev apply
helmfile -e staging apply
helmfile -e prod apply
```

**Plain Helm (same layering):**

```bash
helm upgrade --install cqrs-apps ./cqrs-apps \
  --namespace cqrs-demo \
  -f ./cqrs-apps/values.yaml \
  -f ./cqrs-apps/values-dev.yaml
```

**Script (local K8s):**

```powershell
.\scripts\deploy-local-k8s.ps1 -Environment dev
```

## Regenerate values

After adding a microservice, update `scripts/generate-helm-values.ps1` and run:

```powershell
.\scripts\generate-helm-values.ps1
```

## GitOps (Argo CD example)

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: cqrs-apps-dev
spec:
  source:
    repoURL: https://github.com/your-org/cqrs-demo
    path: infra/helm/cqrs-apps
    helm:
      valueFiles:
        - values.yaml
        - values-dev.yaml
  destination:
    namespace: cqrs-demo
```

## When to use subcharts

Split into `charts/shop-gateway/` subcharts only if services need **different Kubernetes resources** (CronJob vs Deployment, different probes, HPA per service). This project uses one generic template — a single chart is correct.

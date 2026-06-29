# Environments: dev, staging, prod

Infrastructure is split by environment so you can run local dev, pre-production validation, and production with distinct names, secrets, and Kubernetes settings.

## Layout

```
infra/
├── docker/
│   ├── base/                    # Shared compose services
│   ├── env/
│   │   ├── dev/                 # override + .env.example
│   │   ├── staging/
│   │   └── prod/
│   └── docker-compose.yml       # Backward-compatible (includes dev)
├── k8s/
│   ├── base/
│   └── overlays/
│       ├── dev/
│       ├── staging/
│       ├── prod/
│       └── local/               # Deprecated alias → dev
├── helm/
│   ├── helmfile.yaml           # Multi-env orchestration
│   └── cqrs-apps/
│       ├── values.yaml         # Shared catalog + defaults
│       ├── values-dev.yaml
│       ├── values-staging.yaml
│       └── values-prod.yaml
└── terraform/
    ├── env/
    │   ├── dev.tfvars
    │   ├── staging.tfvars
    │   └── prod.tfvars
    └── main.tf                  # Uses var.environment
```

## Environment matrix

| | **dev** | **staging** | **prod** |
|---|---------|-------------|----------|
| Docker project | `cqrs-dev` | `cqrs-staging` | `cqrs-prod` |
| K8s namespace | `cqrs-demo` | `cqrs-demo-staging` | `cqrs-demo-prod` |
| Ingress | On (local hosts) | On | On |
| NodePorts | 30500+ (dev) | 31080+ | Disabled |
| DB init job | Yes | Yes | No |
| Image pull | IfNotPresent | Always | Always |
| Replicas (Helm) | 1 | 1 | 2 |

## Docker Compose

```powershell
# Default: dev
.\scripts\start-infra.ps1
.\scripts\start-infra.ps1 -Environment staging
.\scripts\stop-infra.ps1 -Environment prod
```

```bash
./scripts/start-infra.sh dev
./scripts/start-infra.sh staging
```

Optional secrets: copy `infra/docker/env/<env>/.env.example` to `.env` in the same folder.

Manual compose:

```bash
docker compose \
  -f infra/docker/base/docker-compose.yml \
  -f infra/docker/env/dev/docker-compose.override.yml \
  up -d
```

## Kubernetes

```powershell
.\scripts\deploy-local-k8s.ps1 -Environment dev
.\scripts\deploy-local-k8s.ps1 -Environment staging
```

```bash
kubectl apply -k infra/k8s/overlays/dev
kubectl apply -k infra/k8s/overlays/staging
```

`overlays/local` still works and points at `dev` for backward compatibility.

## Terraform

```bash
cd infra/terraform
terraform init
terraform apply -var-file=env/dev.tfvars
terraform apply -var-file=env/staging.tfvars
terraform apply -var-file=env/prod.tfvars
```

Override `sql_password`, `image_registry`, and `image_tag` in the tfvars file or on the CLI.

## Production notes

- Replace SQL password patches with External Secrets / Sealed Secrets.
- Prefer managed SQL Server, Kafka, and Elasticsearch instead of in-cluster StatefulSets.
- Set real ingress hostnames in `helm/cqrs-apps/values-prod.yaml`.
- See **[helm/README.md](../helm/README.md)** for Helmfile and GitOps patterns.

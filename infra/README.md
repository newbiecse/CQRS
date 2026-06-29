# CQRS Demo — Infrastructure deployment

Deploy **SQL Server, Kafka, Elasticsearch, Kibana, OTEL Collector** and microservices to Kubernetes using **Terraform + Helm + Kustomize**.

Environments **dev**, **staging**, and **prod** are documented in **[environments/README.md](environments/README.md)**.

## Layout

```
infra/
├── dockerfiles/
├── docker/
│   ├── base/                 # Shared compose services
│   ├── env/{dev,staging,prod}/
│   └── docker-compose.yml    # Backward-compatible (dev)
├── k8s/base/
├── k8s/overlays/{dev,staging,prod,local}
├── helm/
│   ├── helmfile.yaml         # Helmfile (dev / staging / prod)
│   └── cqrs-apps/            # Chart + values.yaml + values-{env}.yaml
└── terraform/
    └── env/*.tfvars
```

## Prerequisites

- Docker
- Kubernetes cluster (minikube, kind, AKS, EKS, …)
- `kubectl`, `helm`, `terraform` >= 1.5
- NGINX Ingress Controller (staging/prod ingress)

## 1. Build container images

```bash
./scripts/build-container-images.sh
```

## 2. Deploy infrastructure (Kustomize)

```bash
kubectl apply -k infra/k8s/overlays/dev
```

## 3. Deploy with Terraform

```bash
cd infra/terraform
terraform init
terraform apply -var-file=env/dev.tfvars
```

Or with **Helmfile** (common in production pipelines):

```bash
cd infra/helm
helmfile -e dev apply
```

## 4. Local dev (Docker Compose)

```powershell
.\scripts\start-infra.ps1 -Environment dev
.\scripts\initialize-databases.ps1
```

## 5. Local Kubernetes

```powershell
.\scripts\deploy-local-k8s.ps1 -Environment dev
```

See **[k8s/overlays/local/README.md](k8s/overlays/local/README.md)** (alias for dev) for access URLs.

## Notes

- Default credentials are for local/demo only.
- StatefulSets need a default StorageClass.
- SQL Server on K8s is not recommended for production.

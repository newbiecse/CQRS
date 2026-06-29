# CQRS Demo — Infrastructure deployment

Deploy **SQL Server, Kafka, Elasticsearch, Kibana, OTEL Collector** and all microservices to Kubernetes using **Terraform + Helm + Kustomize**.

## Layout

```
infra/
├── dockerfiles/          # Dockerfile.dotnet, Dockerfile.db-init, Dockerfile.nextjs
├── docker/               # docker-compose (local dev)
├── k8s/base/             # Kustomize manifests for platform infra
├── k8s/overlays/local/   # Local cluster overlay
├── helm/cqrs-apps/       # Helm chart for .NET services + shop FE
└── terraform/            # Terraform entrypoint (infra + apps)
```

## Prerequisites

- Docker
- Kubernetes cluster (minikube, kind, AKS, EKS, …)
- `kubectl`, `helm`, `terraform` >= 1.5
- NGINX Ingress Controller (for ingress hosts)

## 1. Build container images

```bash
./scripts/build-container-images.sh
# REGISTRY=myregistry TAG=v1 ./scripts/build-container-images.sh
```

For minikube, load images into the cluster:

```bash
minikube image load cqrsdemo/shop-gateway:latest
# … or use eval $(minikube docker-env) before building
```

## 2. Deploy infrastructure only (Kustomize)

```bash
kubectl apply -k infra/k8s/overlays/local
```

Wait until SQL Server and Elasticsearch are ready (~2–5 min).

## 3. Deploy everything with Terraform

```bash
cd infra/terraform
terraform init
terraform apply \
  -var="image_registry=cqrsdemo" \
  -var="image_tag=latest"
```

Terraform will:

1. Apply Kustomize infra manifests (`sqlserver`, `kafka`, `elasticsearch`, …)
2. Install Helm chart `cqrs-apps` (microservices, db-init Job, ingress)

## 4. Hosts / Ingress

Add to `/etc/hosts` (or equivalent):

```
127.0.0.1 shop.local admin-api.local shop-fe.local
```

| Host | Service |
|------|---------|
| `shop.local` | Shop Gateway (YARP) |
| `admin-api.local` | Admin BFF |
| `shop-fe.local` | Shop Next.js |

Product search: `http://shop.local/product-queries/api/products/search?q=phone`

## 5. Local dev (Docker Compose)

Unchanged — for development without Kubernetes:

```bash
docker compose -f infra/docker/docker-compose.yml up -d
dotnet run --project backend/tools/CqrsDemo.DatabaseInitializer
```

## 6. Local Kubernetes (recommended script)

**Start cluster + dashboard:**

```powershell
.\scripts\start-local-k8s.ps1
```

**Deploy CQRS stack:**

```powershell
.\scripts\deploy-local-k8s.ps1
```

See **[k8s/overlays/local/README.md](../k8s/overlays/local/README.md)** for hosts, NodePort URLs, and troubleshooting.

## Notes

- **Secrets**: default SQL password is for local/demo. Override `sql_password` in Terraform for other environments.
- **Storage**: StatefulSets use PVCs — ensure your cluster has a default StorageClass.
- **SQL Server on K8s**: requires adequate memory (2Gi+). Not recommended for production; use Azure SQL / RDS instead.
- **Gateway config**: `appsettings.Kubernetes.json` uses in-cluster DNS (`http://product-commands:8080`, …).

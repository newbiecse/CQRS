# Local Kubernetes (Windows / Docker Desktop / minikube)

## Prerequisites

1. **Docker Desktop** with **Kubernetes enabled**  
   *Settings → Kubernetes → Enable Kubernetes → Apply*

   Or **minikube**:
   ```powershell
   minikube start --memory=16384 --cpus=4
   ```

2. **Helm** — https://helm.sh/docs/intro/install/

3. RAM khuyến nghị **≥ 16 GB** (SQL Server + Elasticsearch trên K8s tốn bộ nhớ)

4. Cài CLI (nếu chưa có):
   ```powershell
   winget install Kubernetes.kubectl
   winget install Helm.Helm
   ```

## Start cluster + Kubernetes Dashboard

```powershell
.\scripts\start-local-k8s.ps1
```

| Option | Mô tả |
|--------|--------|
| `-Provider minikube` | Dùng minikube thay vì Docker Desktop |
| `-Provider docker-desktop` | Bật K8s trong Docker Desktop |
| `-SkipDashboard` | Chỉ start cluster, không mở dashboard |
| `-SkipBrowser` | Cài dashboard nhưng không mở trình duyệt |

**minikube:** mở dashboard qua `minikube dashboard` (addon).

**Docker Desktop:** cài dashboard bằng Helm, port-forward `https://localhost:8443`, in **login token** ra console.

Dừng port-forward:

```powershell
.\scripts\stop-local-k8s-dashboard.ps1
```

Sau khi cluster chạy, deploy app:

```powershell
.\scripts\deploy-local-k8s.ps1
```

## One-command deploy

Từ thư mục gốc repo (PowerShell):

```powershell
.\scripts\deploy-local-k8s.ps1
```

Script sẽ tự động:

1. Cài **ingress-nginx** (Helm)
2. Deploy **infra** (SQL Server, Kafka, Elasticsearch, Kibana, OTEL)
3. **Build** Docker images
4. Deploy **microservices** + Job khởi tạo DB (Helm)

### Tuỳ chọn

```powershell
# Chỉ infra (chưa build app)
.\scripts\deploy-local-k8s.ps1 -InfraOnly

# Đã build images trước đó
.\scripts\deploy-local-k8s.ps1 -SkipBuild

# Bỏ ingress-nginx (dùng NodePort)
.\scripts\deploy-local-k8s.ps1 -SkipIngress
```

## Truy cập sau khi deploy

### Cách A — Ingress (hosts file)

Thêm vào `C:\Windows\System32\drivers\etc\hosts` (Run as Administrator):

```
127.0.0.1 shop.local admin-api.local shop-fe.local
```

| URL | Mô tả |
|-----|--------|
| http://shop.local | Shop Gateway |
| http://admin-api.local | Admin BFF |
| http://shop-fe.local | Shop UI |

### Cách B — NodePort (không cần hosts file)

| URL | Service |
|-----|---------|
| http://localhost:30500 | Shop Gateway |
| http://localhost:30510 | Admin API |
| http://localhost:30501 | Shop UI |
| http://localhost:30561 | Kibana |

### Cách C — Port-forward

```powershell
kubectl port-forward -n cqrs-demo svc/shop-gateway 5000:8080
kubectl port-forward -n cqrs-demo svc/kibana 5601:5601
```

## Kiểm tra trạng thái

```powershell
kubectl get pods -n cqrs-demo
kubectl logs -n cqrs-demo job/db-initializer
kubectl get ingress -n cqrs-demo
```

## Gỡ bỏ

```powershell
helm uninstall cqrs-apps -n cqrs-demo
kubectl delete -k infra/k8s/overlays/local
helm uninstall ingress-nginx -n ingress-nginx
```

## minikube

Trước khi deploy, dùng Docker daemon của minikube để build image:

```powershell
minikube start --memory=16384 --cpus=4
.\scripts\deploy-local-k8s.ps1
```

Script tự `minikube image load` sau khi build.

## Troubleshooting

| Vấn đề | Gợi ý |
|--------|--------|
| SQL Server `Pending` | Tăng RAM Docker Desktop (Settings → Resources → 8GB+) |
| `ImagePullBackOff` | Chạy lại với build: `.\scripts\build-container-images.ps1` |
| Ingress 404 | Đợi ingress-nginx ready: `kubectl get pods -n ingress-nginx` |
| DB init failed | `kubectl logs -n cqrs-demo job/db-initializer` — đợi sqlserver Ready |

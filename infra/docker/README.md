# Container images

Each deployable has its own **Dockerfile** next to the project (ownership model). Build orchestration uses `images.manifest.json`.

## Layout

```
backend/src/Gateway/Shop.Gateway.Api/Dockerfile
backend/src/Services/Product/Product.Commands.Api/Dockerfile
backend/tools/CqrsDemo.DatabaseInitializer/Dockerfile
frontend/shop/Dockerfile
infra/docker/images.manifest.json    # image name -> dockerfile path
```

## Build one service

From **repository root**:

```bash
docker build -f backend/src/Gateway/Shop.Gateway.Api/Dockerfile -t cqrsdemo/shop-gateway:latest .
```

## Build all images

```powershell
.\scripts\build-container-images.ps1
```

```bash
REGISTRY=cqrsdemo TAG=latest ./scripts/build-container-images.sh
```

Build a single image:

```powershell
.\scripts\build-container-images.ps1 -Only shop-gateway
```

## Add a new service

1. Add an entry to `scripts/generate-service-dockerfiles.ps1`
2. Run `.\scripts\generate-service-dockerfiles.ps1`
3. Add the service to Helm (`scripts/generate-helm-values.ps1`) if it deploys to K8s

## Legacy

`infra/dockerfiles/Dockerfile.dotnet` is **deprecated** — kept as a reference template only.

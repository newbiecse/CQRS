# CQRS Demo

Distributed **CQRS** + **event-driven** microservices with **transactional outbox**, **MassTransit**, and **Kafka**.

## Repository structure

```
CQRS/
├── backend/                      # .NET 8 microservices
│   ├── src/
│   │   ├── BuildingBlocks/       # Shared libraries (domain, messaging, rate limiting, …)
│   │   ├── CqrsDemo.Contracts/   # Integration events & contracts
│   │   ├── Gateway/              # Shop.Gateway.Api (:5000), Shop.Admin.Api (:5100)
│   │   └── Services/             # Product, Cart, Order, Payment, User, Auth, Chat, …
│   ├── tools/
│   │   └── CqrsDemo.DatabaseInitializer/
│   ├── tests/
│   └── CqrsDemo.Distributed.sln
│
├── frontend/
│   ├── admin/                    # Admin portal — Ant Design Pro (Umi) → :8000
│   └── shop/                     # Shop storefront — Next.js (App Router, SSR) → :3001
│
├── infra/
│   ├── docker/                   # Docker Compose (dev / staging / prod overlays)
│   ├── dockerfiles/              # Container build definitions
│   ├── k8s/                      # Kustomize — base + overlays (dev, staging, prod)
│   ├── helm/
│   │   ├── helmfile.yaml         # Multi-environment Helm orchestration
│   │   └── cqrs-apps/            # Chart + values.yaml + values-{env}.yaml
│   ├── terraform/                # Terraform deploy to Kubernetes
│   └── environments/             # Cross-cutting env documentation
│
├── docs/                         # Architecture, flows, database schemas
└── scripts/                      # Local dev, build, deploy helpers
```

| Area | Stack |
|------|--------|
| Backend | .NET 8, ASP.NET Core, MassTransit, SQL Server, Elasticsearch |
| Admin UI | React, Ant Design Pro (Umi) |
| Shop UI | Next.js 15, React 19 |
| Infra (local) | Docker Compose — SQL Server, Kafka, Elasticsearch, Kibana, OTEL |
| Infra (K8s) | Kustomize + Helm + Helmfile + Terraform |

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|--------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.x | Backend build & run |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Compose v2 | Local SQL, Kafka, Elasticsearch |
| [Node.js](https://nodejs.org/) | 20+ | Frontends |
| pnpm or npm | latest | Frontends (`pnpm` preferred) |

Optional for Kubernetes: `kubectl`, `helm`, `helmfile`, `terraform` — see [infra/README.md](infra/README.md).

---

## Local development (quick start)

Run from the **repository root**. On Windows, use **PowerShell**; bash equivalents are listed where available.

### 1. Start infrastructure

Starts SQL Server, Kafka, Elasticsearch, Kibana, and OpenTelemetry collector.

```powershell
.\scripts\start-infra.ps1
```

```bash
./scripts/start-infra.sh dev
```

Default environment is **dev**. Optional: `-Environment staging|prod` (PowerShell) or `./scripts/start-infra.sh staging`.

| Service | URL | Credentials |
|---------|-----|-------------|
| SQL Server | `localhost:1433` | `sa` / `Your_password123` |
| Kafka | `localhost:9092` | — |
| Elasticsearch | `http://localhost:9200` | — |
| Kibana | `http://localhost:5601` | — |
| OTLP (gRPC) | `localhost:4317` | — |

Stop infrastructure:

```powershell
.\scripts\stop-infra.ps1
```

### 2. Initialize databases

Creates all write/read databases and exports SQL scripts to `scripts/sql/`.

```powershell
.\scripts\initialize-databases.ps1
```

Requires SQL Server from step 1.

### 3. Build backend

```powershell
.\scripts\build-backend.ps1
```

```bash
./scripts/build-backend.sh
```

Or manually:

```bash
dotnet build backend/CqrsDemo.Distributed.sln -c Release
dotnet build backend/tools/CqrsDemo.DatabaseInitializer/CqrsDemo.DatabaseInitializer.csproj -c Release
```

### 4. Start backend

Starts all APIs and workers in the background. Logs go to `scripts/logs/`.

```powershell
.\scripts\start-backend.ps1
```

Auto-runs `build-backend.ps1` if Release binaries are missing. Stop with:

```powershell
.\scripts\stop-backend.ps1
```

**Main entry points:**

| Service | URL | Used by |
|---------|-----|---------|
| Shop Gateway (YARP) | http://localhost:5000 | Shop frontend |
| Admin BFF | http://localhost:5100 | Admin frontend |
| Auth API | http://localhost:5207 | Login / OAuth |
| Chat API | http://localhost:5220 | AI chat agent |

### 5. Run frontends

Use **separate terminals** (backend must be running).

**Admin portal** (Ant Design Pro):

```powershell
.\scripts\run-fe-admin.ps1
```

Open **http://localhost:8000** — proxies `/api/admin` → Admin BFF (`:5100`).

**Shop** (Next.js):

```powershell
.\scripts\run-fe-shop.ps1
```

Open **http://localhost:3001** — SSR pages call the gateway at `:5000`.

### 6. Login (admin)

Default seeded user:

- **Email:** `admin@cqrs.local`
- **Password:** `Admin123!`

---

## Build frontends (production)

### Admin

```bash
cd frontend/admin
pnpm install    # or npm install
pnpm build      # output: frontend/admin/dist
pnpm preview    # preview on :8000
```

### Shop

```bash
cd frontend/shop
pnpm install
pnpm build      # Next.js production build
pnpm start      # serves on :3001
```

Shop SSR uses `GATEWAY_URL` (default `http://localhost:5000`) for server-side API calls.

---

## Local dev checklist

```
1. .\scripts\start-infra.ps1
2. .\scripts\initialize-databases.ps1
3. .\scripts\build-backend.ps1
4. .\scripts\start-backend.ps1
5. .\scripts\run-fe-admin.ps1      # terminal A
6. .\scripts\run-fe-shop.ps1       # terminal B
```

---

## Kubernetes (optional)

**Docker Desktop Kubernetes** or minikube:

```powershell
.\scripts\start-local-k8s.ps1              # enable cluster + dashboard
.\scripts\deploy-local-k8s.ps1 -Environment dev
```

Or with Helmfile:

```bash
cd infra/helm
helmfile -e dev apply
```

Details: [infra/README.md](infra/README.md) · [infra/helm/README.md](infra/helm/README.md) · [infra/k8s/overlays/local/README.md](infra/k8s/overlays/local/README.md)

**Linux / macOS script (same steps as `deploy-local-k8s.ps1`):**

```bash
export CLUSTER_PROVIDER=existing   # or kind
./scripts/ci-local-k8s.sh
```

---

## GitHub Actions

| Workflow | When | What |
|----------|------|------|
| [CI](.github/workflows/ci.yml) | Push / PR → `main` | Build backend, frontends, Docker images |
| [Local Kubernetes](.github/workflows/local-k8s.yml) | Push `main` (selected paths) or manual | Deploy full stack to **kind** (CI) or **self-hosted** local K8s |

See **[.github/README.md](.github/README.md)** for self-hosted runner setup and workflow inputs.

```bash
# Same deploy logic as the Local Kubernetes workflow (on your machine)
CLUSTER_PROVIDER=kind ./scripts/ci-local-k8s.sh
```

---

## Documentation

| Doc | Description |
|-----|-------------|
| [docs/README-DISTRIBUTED.md](docs/README-DISTRIBUTED.md) | Service ports, curl examples, saga flows |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | System overview |
| [docs/CODE-FLOWS.md](docs/CODE-FLOWS.md) | Request and event flows |
| [docs/DATABASE-SCHEMA.md](docs/DATABASE-SCHEMA.md) | Per-service database schemas |
| [infra/environments/README.md](infra/environments/README.md) | Dev / staging / prod infra layout |

**Solution file:** `backend/CqrsDemo.Distributed.sln`

---

## Useful scripts

| Script | Purpose |
|--------|---------|
| `start-infra.ps1` / `stop-infra.ps1` | Docker Compose infrastructure |
| `initialize-databases.ps1` | Create DBs & schema |
| `build-backend.ps1` | `dotnet build` entire backend |
| `start-backend.ps1` / `stop-backend.ps1` | Run/stop all microservices |
| `run-fe-admin.ps1` / `run-fe-shop.ps1` | Frontend dev servers |
| `build-container-images.ps1` | Build Docker images for K8s |
| `deploy-local-k8s.ps1` | Deploy full stack to local Kubernetes |
| `ci-local-k8s.sh` | Same deploy flow for Linux/macOS and GitHub Actions |
| `generate-helm-values.ps1` | Regenerate Helm `values*.yaml` after adding a service |

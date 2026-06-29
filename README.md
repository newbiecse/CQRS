# CQRS Demo

Distributed **CQRS** + **event-driven** microservices with **transactional outbox**, **MassTransit**, and **Kafka**.

## Repository layout

```
CQRS/
├── backend/                 # .NET microservices
│   ├── src/                 # Services, Gateway, BuildingBlocks
│   ├── tools/               # Database initializer
│   ├── tests/               # Backend tests (placeholder)
│   └── CqrsDemo.Distributed.sln
├── frontend/
│   ├── admin/               # Admin portal (Next.js)
│   └── shop/                # Shopping site (future)
├── infra/
│   ├── docker/              # Docker Compose (local dev)
│   ├── dockerfiles/         # Container build definitions
│   ├── k8s/                 # Kustomize manifests (platform infra)
│   ├── helm/                # Helm chart (microservices)
│   └── terraform/           # Terraform deploy to Kubernetes
├── docs/                    # Architecture & runbooks
└── scripts/                 # Dev scripts & SQL
```

## Quick start

```bash
docker compose -f infra/docker/docker-compose.yml up -d
dotnet run --project backend/tools/CqrsDemo.DatabaseInitializer
dotnet build backend/CqrsDemo.Distributed.sln
```

See **[docs/README-DISTRIBUTED.md](docs/README-DISTRIBUTED.md)** for services, ports, and run instructions.

**Kubernetes / Terraform:** see **[infra/README.md](infra/README.md)**.

**Local K8s one-liner (Windows):**

```powershell
.\scripts\start-local-k8s.ps1          # start cluster + dashboard
.\scripts\deploy-local-k8s.ps1         # deploy CQRS stack
```

## Documentation

| Doc | Description |
|-----|-------------|
| [docs/README-DISTRIBUTED.md](docs/README-DISTRIBUTED.md) | Run guide, ports, curl examples |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | System overview |
| [docs/CODE-FLOWS.md](docs/CODE-FLOWS.md) | Request and event flows |
| [docs/DATABASE-SCHEMA.md](docs/DATABASE-SCHEMA.md) | Per-service database schemas |

Solution: **`backend/CqrsDemo.Distributed.sln`**

## Admin portal

Next.js UI in **`frontend/admin/`** — calls **`Shop.Admin.Api`** BFF at `:5100`.

```bash
dotnet run --project backend/src/Gateway/Shop.Admin.Api
cd frontend/admin && pnpm install && pnpm dev
```

Open http://localhost:3000 (requires Admin API + backend microservices).

| API | Port | Consumer |
|-----|------|----------|
| `Shop.Gateway.Api` | 5000 | Shopping site (`frontend/shop`) |
| `Shop.Admin.Api` | 5100 | Admin portal (`frontend/admin`) |

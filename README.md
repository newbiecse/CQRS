# CQRS Demo

Distributed **CQRS** + **event-driven** microservices with **transactional outbox**, **MassTransit**, and **Kafka**.

## Quick start

```bash
docker compose -f docker/docker-compose.yml up -d
dotnet run --project tools/CqrsDemo.DatabaseInitializer
dotnet build CqrsDemo.Distributed.sln
```

See **[README-DISTRIBUTED.md](README-DISTRIBUTED.md)** for services, ports, and run instructions.

## Documentation

| Doc | Description |
|-----|-------------|
| [README-DISTRIBUTED.md](README-DISTRIBUTED.md) | Run guide, ports, curl examples |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System overview |
| [CODE-FLOWS.md](CODE-FLOWS.md) | Request and event flows |
| [DATABASE-SCHEMA.md](DATABASE-SCHEMA.md) | Per-service database schemas |

Solution: **`CqrsDemo.Distributed.sln`**

# CQRS Microservices + Azure Service Bus

Production-style **CQRS** + **Event Sourcing** with **eventual consistency**:

- **Commands API** — appends domain events to **event store** (`StoredEvents`) + **transactional outbox**
- **Outbox publisher** — sends integration events to **Azure Service Bus**
- **Projection Worker** — consumes events, updates **read database**
- **Queries API** — reads only from read database

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────────┐
│ Commands.Api    │────►│ CqrsDemo_Write   │     │ Azure Service Bus   │
│ (5181)          │     │ StoredEvents     │────►│ topic: product-events│
│                 │     │ + OutboxMessages │     │                     │
└─────────────────┘     └──────────────────┘     └──────────┬──────────┘
        │                        ▲                           │
        │ same transaction       │ outbox poll               │ subscription:
        │ events + outbox        │ (background)              │ product-projection
        ▼                        │                           ▼
   MediatR command          OutboxPublisher          ┌─────────────────────┐
                                                      │ Projection.Worker   │
                                                      └──────────┬──────────┘
                                                                 ▼
                                                      ┌──────────────────┐
┌─────────────────┐                                   │ CqrsDemo_Read    │
│ Queries.Api     │◄──────────────────────────────────│ (read model)     │
│ (5182)          │                                   └──────────────────┘
└─────────────────┘
```

## Services

| Service | Port | Responsibility |
|---------|------|----------------|
| `CqrsDemo.Commands.Api` | 5181 | POST/PUT products, event store, outbox, publish to bus |
| `CqrsDemo.Projection.Worker` | — | Consume bus → project read DB |
| `CqrsDemo.Queries.Api` | 5182 | GET products (read DB only) |

## Prerequisites

- .NET 8 SDK
- Docker (SQL Server + [Service Bus Emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator))

## Quick start

### 1. Infrastructure

```bash
cd /Users/admin/workspace/learning/CQRS
docker compose up -d
```

Wait ~30s for SQL Server and Service Bus emulator.

### 2. Run services (3 terminals)

```bash
export PATH="$HOME/.dotnet:$PATH"

dotnet run --project src/CqrsDemo.Commands.Api
dotnet run --project src/CqrsDemo.Projection.Worker
dotnet run --project src/CqrsDemo.Queries.Api
```

### 3. Shopping flow (full site)

```bash
# 1) Catalog — create product
curl -X POST http://localhost:5181/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Laptop", "price": 999.99}'
# Save productId from response

# Wait ~3s, then browse catalog (Queries API)
curl http://localhost:5182/api/products

# 2) Cart — create cart for a customer
curl -X POST http://localhost:5181/api/carts \
  -H "Content-Type: application/json" \
  -d '{"customerId": "11111111-1111-1111-1111-111111111111"}'
# Save cartId

# 3) Add item (use product name/price from catalog)
curl -X POST http://localhost:5181/api/carts/{cartId}/items \
  -H "Content-Type: application/json" \
  -d '{"productId": "{productId}", "productName": "Laptop", "unitPrice": 999.99, "quantity": 1}'

# View cart (read model)
curl http://localhost:5182/api/carts/{cartId}

# 4) Checkout → creates Order
curl -X POST http://localhost:5181/api/carts/{cartId}/checkout
# Save orderId

# 5) Pay order (simulated instant payment)
curl -X POST http://localhost:5181/api/orders/{orderId}/pay

# 6) View order & payment (after projection)
curl http://localhost:5182/api/orders/{orderId}
curl http://localhost:5182/api/orders/{orderId}/payment

# Event store (write side)
curl http://localhost:5181/api/products/{productId}/events
```

## Event Sourcing (write side)

| Concept | Implementation |
|---------|----------------|
| Source of truth | `StoredEvents` table (append-only) |
| Aggregate state | `Product.LoadFromHistory()` replays events |
| New changes | `Apply` + append events with optimistic concurrency (`Version`) |
| Integration events | Still via outbox → Azure Service Bus → read projection |

**Flow:** Command → domain events → append to `StoredEvents` + outbox (one transaction) → outbox publisher → bus → projection worker → `CqrsDemo_Read`.

**Concurrency:** duplicate writes return `409 Conflict` (`ConcurrencyException`).

## Configuration

`AzureServiceBus` in `Commands.Api` and `Projection.Worker`:

```json
{
  "ConnectionString": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
  "TopicName": "product-events"
}
```

For **Azure cloud**, replace with your namespace connection string from Azure Portal.

## Key patterns

### Transactional outbox

`EventSourcedProductRepository` appends **stored events + outbox rows** in one `SaveChangesAsync` — no dual-write to bus inside the HTTP request.

### Event store

`EventSourcedProductRepository` replaces the old `Products` state table. Each stream (`StreamId` + `StreamType`) has monotonically increasing `Version` numbers.

### Integration events (cross-service contract)

Defined in `CqrsDemo.Contracts` — not domain entities:

- `ProductCreatedIntegrationEvent`
- `ProductPriceUpdatedIntegrationEvent`

### Eventual consistency

After POST to Commands API, Queries API may return empty briefly until the worker projects the event.

## Domain aggregates (Event Sourcing)

| Aggregate | Stream | Events |
|-----------|--------|--------|
| **Product** | `Product` | Created, PriceUpdated |
| **Cart** | `Cart` | Created, ItemAdded, ItemRemoved, CheckedOut |
| **Order** | `Order` | Created, Paid |
| **Payment** | `Payment` | Initiated, Completed, Failed |

## Project structure

```
src/
├── CqrsDemo.Domain/             # Product, Cart, Order, Payment aggregates
├── CqrsDemo.Contracts/          # Integration events + message types
├── CqrsDemo.Messaging/          # Azure Service Bus publisher + topology
├── CqrsDemo.Commands.*          # Write side + event store + outbox
├── CqrsDemo.Queries.*           # Read side (catalog, carts, orders, payments)
└── CqrsDemo.Projection.Worker/  # ShopProjectionHandler → read DB
```

## Legacy monolith

The original single `CqrsDemo.Api` (in-process MediatR projection) remains under `src/CqrsDemo.Api` but is **not** in the solution. Use the microservices above.

## Azure deployment notes

1. Create Service Bus namespace, topic `product-events`, subscription `product-projection`.
2. Deploy Commands API, Projection Worker, Queries API (Container Apps / AKS / App Service).
3. Use separate connection strings for Write DB, Read DB, and Service Bus via Key Vault.

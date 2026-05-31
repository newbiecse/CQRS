# CQRS Distributed Microservices

Domain-based microservices with **Event Sourcing**, **transactional outbox**, **Azure Service Bus** (`shop-events` topic), and a **Checkout Saga** orchestrator.

**Architecture diagrams:** [ARCHITECTURE.md](./ARCHITECTURE.md) · **Code flows:** [CODE-FLOWS.md](./CODE-FLOWS.md) · **Database schemas:** [DATABASE-SCHEMA.md](./DATABASE-SCHEMA.md) · **Visual DB diagrams:** open [docs/database-diagrams/index.html](./docs/database-diagrams/index.html) in a browser

## Services (21 deployables)

| Domain | Commands | Queries | Workers |
|--------|----------|---------|---------|
| **Reporting** | — | :5217 | Reporting.Projection.Worker |
| **User** | :5206 | :5216 | User.Projection.Worker |
| **Product** | :5201 | :5211 | Product.Projection.Worker |
| **Cart** | :5202 | :5212 | Cart.Projection.Worker |
| **Order** | :5203 | :5213 | Order.Projection.Worker + **Order.Integration.Worker** |
| **Payment** | :5204 | :5214 | Payment.Projection.Worker |
| **Checkout Saga** | :5205 | — | **CheckoutSaga.Worker** |
| **Gateway** | :5000 (YARP) | | |

Each domain has **its own databases**:
- `CqrsDemo_{Domain}_Write` — event store + outbox
- `CqrsDemo_{Domain}_Read` — query projections
- `CqrsDemo_Saga` — saga orchestration state (`CheckoutSagas` table)

## Checkout Saga (orchestration)

The saga **coordinates** cart checkout, order creation, payment, and **compensation** (cancel order on payment failure). This replaces the old choreography where `Order.Integration` reacted to `payment.completed`.

```
Client
  │
  ▼ POST /api/sagas/checkout { cartId }
CheckoutSaga.Api (:5205)
  │ 1. HTTP POST Cart.Commands /checkout
  │ 2. Persist saga state (CartCheckedOut)
  ▼
Cart_Write ──outbox──► shop-events ──► Order.Integration ──► Order_Write (order.created)
  │
  ▼ CheckoutSaga.Worker (subscription: checkout-saga-orchestration)
  │ 3. On order.created → HTTP POST Payment.Commands /pay
  │ 4. On payment.completed → HTTP POST Order.Commands /mark-paid → Completed
  │ 5. On payment.failed → HTTP POST Order.Commands /cancel → Compensated
```

### Saga states

`Started` → `CartCheckedOut` → `OrderCreated` → `PaymentPending` → `Completed`

On failure: `PaymentFailed` → `Compensating` → `Compensated` (or `Failed`)

### API

```bash
# Start checkout (happy path)
curl -X POST http://localhost:5205/api/sagas/checkout \
  -H "Content-Type: application/json" \
  -d '{"cartId":"<cart-guid>"}'

# Via gateway
curl -X POST http://localhost:5000/checkout-saga/api/sagas/checkout \
  -H "Content-Type: application/json" \
  -d '{"cartId":"<cart-guid>"}'

# Poll saga status
curl http://localhost:5205/api/sagas/<saga-id>

# Demo compensation (simulated payment failure)
curl -X POST http://localhost:5205/api/sagas/checkout \
  -H "Content-Type: application/json" \
  -d '{"cartId":"<cart-guid>","simulatePaymentFailure":true}'
```

### What changed vs choreography

| Before | After (Saga) |
|--------|----------------|
| Client calls cart checkout + payment separately | Single saga entry point |
| `Order.Integration` handled `payment.completed` | Saga calls `Order.Commands` `/mark-paid` |
| No compensation | `Order.Commands` `/cancel` on `payment.failed` |

`Order.Integration.Worker` still creates orders from `cart.checked-out` (reaction to cart checkout step).

## Build & run

```bash
dotnet build CqrsDemo.Distributed.sln
docker compose up -d
# See scripts/run-distributed.sh for all processes
```

## Typical flow (with User)

```bash
# 1. Register user → use userId as cart customerId
curl -X POST http://localhost:5000/user-commands/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","displayName":"Alice"}'

# 2. Create cart for that user
curl -X POST http://localhost:5000/cart-commands/api/carts \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<userId-from-step-1>"}'
```

`Cart.CustomerId` and `Order.CustomerId` reference the **User** aggregate id.

## Reporting (analytics read model)

Dedicated DB **`CqrsDemo_Reporting`** fed by **`Reporting.Projection.Worker`** (subscription `reporting-projection`).  
Listens to `user.*` and `order.*` integration events → denormalized `UserProfiles` + `OrderFacts`.  
**No HTTP** between services at query time.

| Service | Port |
|---------|------|
| Reporting.Queries.Api | 5217 |

```bash
# Top users by order amount (day / week / month)
curl "http://localhost:5000/reporting-queries/api/reports/top-users/day?limit=10"
curl "http://localhost:5000/reporting-queries/api/reports/top-users/week"
curl "http://localhost:5000/reporting-queries/api/reports/top-users/month?date=2026-05-23"
```

Run **`Reporting.Projection.Worker`** so the report DB stays in sync after orders/users change.

## Gateway example

```bash
curl -X POST http://localhost:5000/product-commands/api/products \
  -H "Content-Type: application/json" -d '{"name":"Phone","price":599}'

curl http://localhost:5000/product-queries/api/products
```

## Legacy monolith

Older `CqrsDemo.Commands.Api` / `CqrsDemo.Queries.Api` remain in `src/` for reference. Use **`CqrsDemo.Distributed.sln`** for domain microservices.

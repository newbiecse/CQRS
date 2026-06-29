# Distributed CQRS Architecture

Visual reference for **CqrsDemo.Distributed.sln** — domain microservices, event sourcing, transactional outbox, Azure Service Bus, and checkout saga orchestration.

> **Runbook:** [README-DISTRIBUTED.md](./README-DISTRIBUTED.md) · **Code flows:** [CODE-FLOWS.md](./CODE-FLOWS.md) · **DB schemas:** [DATABASE-SCHEMA.md](./DATABASE-SCHEMA.md)

---

## 1. System overview

```mermaid
flowchart TB
    subgraph Clients
        Client[Client / Browser / curl]
    end

    subgraph Edge
        GW["Shop.Gateway.Api :5000<br/>YARP reverse proxy"]
    end

    subgraph CommandSide["Command APIs (writes)"]
        PC["Product.Commands :5201"]
        CC["Cart.Commands :5202"]
        OC["Order.Commands :5203"]
        PayC["Payment.Commands :5204"]
        SagaAPI["CheckoutSaga.Api :5205"]
    end

    subgraph QuerySide["Query APIs (reads)"]
        PQ["Product.Queries :5211"]
        CQ["Cart.Queries :5212"]
        OQ["Order.Queries :5213"]
        PayQ["Payment.Queries :5214"]
    end

    subgraph Workers["Background workers"]
        PPW[Product.Projection.Worker]
        CPW[Cart.Projection.Worker]
        OPW[Order.Projection.Worker]
        OIW[Order.Integration.Worker]
        PayPW[Payment.Projection.Worker]
        SagaW[CheckoutSaga.Worker]
    end

    subgraph Infra["Shared infrastructure"]
        SB[("Azure Service Bus<br/>topic: shop-events")]
        SQL[("SQL Server")]
    end

    Client --> GW
    GW --> PC & CC & OC & PayC & SagaAPI
    GW --> PQ & CQ & OQ & PayQ

    PC & CC & OC & PayC --> SQL
    PQ & CQ & OQ & PayQ --> SQL
    SagaAPI & SagaW --> SQL

    PC & CC & OC & PayC --> SB
    SB --> PPW & CPW & OPW & OIW & PayPW & SagaW
    PPW & CPW & OPW & PayPW --> SQL

    SagaAPI -. HTTP orchestration .-> CC & OC & PayC
    SagaW -. HTTP orchestration .-> OC & PayC
    PayC -. validate amount .-> OQ
```

---

## 2. CQRS pattern (per domain service)

Each bounded context splits **writes** and **reads** into separate deployables and databases.

```mermaid
flowchart LR
    subgraph WritePath["Write path"]
        CMD[Commands API]
        ES[(Write DB<br/>StoredEvents + Outbox)]
        OB[Outbox publisher]
    end

    subgraph Messaging
        TOPIC[shop-events topic]
    end

    subgraph ReadPath["Read path"]
        SUB[Projection / Integration worker]
        RD[(Read DB<br/>denormalized tables)]
        QRY[Queries API]
    end

    CMD -->|append domain events| ES
    ES -->|same transaction| OB
    OB -->|publish integration events| TOPIC
    TOPIC --> SUB
    SUB -->|upsert projections| RD
    QRY -->|SELECT| RD
```

| Layer | Responsibility |
|-------|----------------|
| **Commands API** | Accept HTTP → MediatR command → load/append aggregate → save to event store |
| **Write DB** | Append-only `StoredEvents` + `OutboxMessages` (transactional outbox) |
| **Outbox publisher** | Poll outbox → publish to Service Bus (at-least-once) |
| **Worker** | Consume subscription → update read model (or cross-service integration) |
| **Queries API** | Read-only HTTP over projected tables |

---

## 3. Database topology

```mermaid
flowchart TB
    subgraph Product
        PW[(CqrsDemo_Product_Write)]
        PR[(CqrsDemo_Product_Read)]
    end

    subgraph Cart
        CW[(CqrsDemo_Cart_Write)]
        CR[(CqrsDemo_Cart_Read)]
    end

    subgraph Order
        OW[(CqrsDemo_Order_Write)]
        OR[(CqrsDemo_Order_Read)]
    end

    subgraph Payment
        PayW[(CqrsDemo_Payment_Write)]
        PayR[(CqrsDemo_Payment_Read)]
    end

    subgraph Saga
        SD[(CqrsDemo_Saga<br/>CheckoutSagas)]
    end

    PW --- PR
    CW --- CR
    OW --- OR
    PayW --- PayR
```

**Rule:** no shared write database between domains. Cross-domain consistency uses **integration events** and the **checkout saga**, not distributed transactions.

---

## 4. API gateway routing

```mermaid
flowchart LR
    Client --> GW["Gateway :5000"]

    GW -->|/product-commands/*| P5201[5201]
    GW -->|/product-queries/*| P5211[5211]
    GW -->|/cart-commands/*| C5202[5202]
    GW -->|/cart-queries/*| C5212[5212]
    GW -->|/order-commands/*| O5203[5203]
    GW -->|/order-queries/*| O5213[5213]
    GW -->|/payment-commands/*| Pay5204[5204]
    GW -->|/payment-queries/*| Pay5214[5214]
    GW -->|/checkout-saga/*| S5205[5205]
```

Example: `GET http://localhost:5000/product-queries/api/products` → `http://localhost:5211/api/products`.

---

## 5. Event bus topology

Single topic **`shop-events`** with competing subscriptions (pub/sub fan-out).

```mermaid
flowchart TB
  subgraph Publishers
    PUB1[Product outbox]
    PUB2[Cart outbox]
    PUB3[Order outbox]
    PUB4[Payment outbox]
    PUB5[Saga notifier]
  end

  TOPIC[shop-events]

  subgraph Subscriptions
    S1[product-projection]
    S2[cart-projection]
    S3[order-projection]
    S4[order-integration]
    S5[payment-projection]
    S6[checkout-saga-orchestration]
  end

  subgraph Consumers
    W1[Product.Projection.Worker]
    W2[Cart.Projection.Worker]
    W3[Order.Projection.Worker]
    W4[Order.Integration.Worker]
    W5[Payment.Projection.Worker]
    W6[CheckoutSaga.Worker]
  end

  PUB1 & PUB2 & PUB3 & PUB4 & PUB5 --> TOPIC
  TOPIC --> S1 & S2 & S3 & S4 & S5 & S6
  S1 --> W1
  S2 --> W2
  S3 --> W3
  S4 --> W4
  S5 --> W5
  S6 --> W6
```

### Integration event catalog

| Event type | Typical publisher | Main consumers |
|------------|-------------------|----------------|
| `product.created.v1` | Product | product-projection |
| `product.price-updated.v1` | Product | product-projection |
| `cart.created.v1` | Cart | cart-projection |
| `cart.item-added.v1` | Cart | cart-projection |
| `cart.item-removed.v1` | Cart | cart-projection |
| `cart.checked-out.v1` | Cart | cart-projection, **order-integration** |
| `order.created.v1` | Order | order-projection, **checkout-saga-orchestration** |
| `order.paid.v1` | Order | order-projection |
| `order.cancelled.v1` | Order | order-projection |
| `payment.initiated.v1` | Payment | payment-projection |
| `payment.completed.v1` | Payment | payment-projection, **checkout-saga-orchestration** |
| `payment.failed.v1` | Payment | payment-projection, **checkout-saga-orchestration** |
| `checkout-saga.completed.v1` | Saga | (observers / future) |
| `checkout-saga.failed.v1` | Saga | (observers / future) |

---

## 6. Checkout saga — orchestration flow

The saga is the **single entry point** for distributed checkout. It uses **HTTP commands** for steps and **async events** to know when downstream work finished.

### 6.1 Happy path (sequence)

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant S as CheckoutSaga.Api
    participant SW as CheckoutSaga.Worker
    participant Cart as Cart.Commands
    participant Bus as shop-events
    participant OInt as Order.Integration
    participant Pay as Payment.Commands
    participant Ord as Order.Commands

    C->>S: POST /api/sagas/checkout { cartId }
    S->>Cart: HTTP POST /checkout
    Cart->>Bus: cart.checked-out.v1
    Note over S: Saga state: CartCheckedOut

    Bus->>OInt: cart.checked-out
    OInt->>Bus: order.created.v1

    Bus->>SW: order.created
    SW->>Pay: HTTP POST /pay
  Pay->>Bus: payment.completed.v1
    Note over SW: Saga state: PaymentPending

    Bus->>SW: payment.completed
    SW->>Ord: HTTP POST /mark-paid
    Note over SW: Saga state: Completed
    SW->>Bus: checkout-saga.completed.v1 (optional notify)
```

### 6.2 Compensation path (payment failure)

```mermaid
sequenceDiagram
    autonumber
    participant SW as CheckoutSaga.Worker
    participant Pay as Payment.Commands
    participant Bus as shop-events
    participant Ord as Order.Commands

    SW->>Pay: HTTP POST /pay?simulateFailure=true
    Pay->>Bus: payment.failed.v1
    Note over SW: Saga state: PaymentFailed

    Bus->>SW: payment.failed
    SW->>Ord: HTTP POST /cancel
    Note over SW: Compensating → Compensated
    SW->>Bus: checkout-saga.failed.v1
```

### 6.3 Saga state machine

```mermaid
stateDiagram-v2
    [*] --> Started: POST /sagas/checkout
    Started --> CartCheckedOut: cart checkout OK
    Started --> Failed: cart checkout error

    CartCheckedOut --> OrderCreated: order.created event
    OrderCreated --> PaymentPending: HTTP pay order

    PaymentPending --> Completed: payment.completed → mark-paid
    PaymentPending --> PaymentFailed: payment.failed

    PaymentFailed --> Compensating: start compensation
    Compensating --> Compensated: order cancelled
    Compensating --> Failed: cancel failed

    Completed --> [*]
    Compensated --> [*]
    Failed --> [*]
```

---

## 7. Service map (deployables)

```mermaid
mindmap
  root((CqrsDemo Distributed))
    Gateway
      Shop.Gateway.Api 5000
    Product
      Commands 5201
      Queries 5211
      Projection Worker
    Cart
      Commands 5202
      Queries 5212
      Projection Worker
    Order
      Commands 5203
      Queries 5213
      Projection Worker
      Integration Worker
    Payment
      Commands 5204
      Queries 5214
      Projection Worker
    Saga
      CheckoutSaga.Api 5205
      CheckoutSaga.Worker
    BuildingBlocks
      Domain
      EventStore
      Messaging
    Contracts
      Integration events
```

| # | Process | Port | Role |
|---|---------|------|------|
| 1 | Shop.Gateway.Api | 5000 | Edge routing |
| 2 | Product.Commands.Api | 5201 | Write products |
| 3 | Product.Queries.Api | 5211 | Read products |
| 4 | Cart.Commands.Api | 5202 | Write carts |
| 5 | Cart.Queries.Api | 5212 | Read carts |
| 6 | Order.Commands.Api | 5203 | Mark paid / cancel (saga) |
| 7 | Order.Queries.Api | 5213 | Read orders |
| 8 | Payment.Commands.Api | 5204 | Process payments |
| 9 | Payment.Queries.Api | 5214 | Read payments |
| 10 | CheckoutSaga.Api | 5205 | Start & query sagas |
| 11–16 | 6 workers | — | Projections, integration, saga |

---

## 8. Solution structure (code)

```mermaid
flowchart TB
    subgraph src
        BB[BuildingBlocks<br/>Domain · EventStore · Messaging]
        CT[CqrsDemo.Contracts]
        GW[src/Gateway]
        subgraph Services
            PR[Product/*]
            CA[Cart/*]
            OR[Order/*]
            PA[Payment/*]
            SG[Saga/CheckoutSaga.*]
        end
    end

    PR & CA & OR & PA --> BB
    PR & CA & OR & PA & SG --> CT
    SG --> BB
    GW --> Services
```

---

## 9. Design decisions (quick reference)

| Decision | Choice | Why |
|----------|--------|-----|
| CQRS | Separate command/query APIs + DBs | Scale reads/writes independently; simple query models |
| Writes | Event sourcing (`StoredEvents`) | Audit trail; rebuild aggregates from history |
| Cross-service messaging | Transactional outbox + Service Bus | Reliable publish after DB commit |
| Checkout consistency | **Saga orchestration** | Explicit steps, compensation, visible state |
| Order creation | `Order.Integration` reacts to `cart.checked-out` | Keeps cart service unaware of order schema |
| Payment validation | HTTP to Order.Queries | Sync read of projected total before charge |

---

## 10. Local infrastructure

```mermaid
flowchart LR
    subgraph Docker
        SQL[(SQL Server :1433)]
        SBEmu[Service Bus Emulator]
    end

    Apps[.NET services<br/>5201–5214, 5205, 5000] --> SQL
    Apps --> SBEmu
```

Config: `docker/docker-compose.yml` (SQL Server + Kafka).

---

*Generated for the distributed microservices demo. Diagrams use [Mermaid](https://mermaid.js.org/) — render in GitHub, VS Code, or Cursor markdown preview.*

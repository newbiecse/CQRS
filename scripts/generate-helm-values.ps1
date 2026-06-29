# Generates Helm values using the industry-standard layered pattern:
#   values.yaml          — shared defaults + full service catalog
#   values-{env}.yaml    — environment-specific overrides only

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ChartDir = Join-Path $RepoRoot 'infra\helm\cqrs-apps'

$ServicesYaml = @'
services:
  shop-gateway:
    image: shop-gateway
    kind: api
    port: 8080
    env:
      ASPNETCORE_ENVIRONMENT: Kubernetes
  shop-admin-api:
    image: shop-admin-api
    kind: api
    port: 8080
    env:
      ASPNETCORE_ENVIRONMENT: Kubernetes
  product-commands:
    image: product-commands
    kind: api
    port: 8080
    writeDb: CqrsDemo_Product_Write
  product-queries:
    image: product-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_Product_Read
  product-projection-worker:
    image: product-projection-worker
    kind: worker
    readDb: CqrsDemo_Product_Read
  cart-commands:
    image: cart-commands
    kind: api
    port: 8080
    writeDb: CqrsDemo_Cart_Write
  cart-queries:
    image: cart-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_Cart_Read
  cart-projection-worker:
    image: cart-projection-worker
    kind: worker
    readDb: CqrsDemo_Cart_Read
  order-commands:
    image: order-commands
    kind: api
    port: 8080
    writeDb: CqrsDemo_Order_Write
  order-queries:
    image: order-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_Order_Read
  order-projection-worker:
    image: order-projection-worker
    kind: worker
    readDb: CqrsDemo_Order_Read
  order-integration-worker:
    image: order-integration-worker
    kind: worker
    readDb: CqrsDemo_Order_Read
  payment-commands:
    image: payment-commands
    kind: api
    port: 8080
    writeDb: CqrsDemo_Payment_Write
    extraEnv:
      OrderService__BaseUrl: http://order-queries:8080
  payment-queries:
    image: payment-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_Payment_Read
  payment-projection-worker:
    image: payment-projection-worker
    kind: worker
    readDb: CqrsDemo_Payment_Read
  user-commands:
    image: user-commands
    kind: api
    port: 8080
    writeDb: CqrsDemo_User_Write
  user-queries:
    image: user-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_User_Read
  user-projection-worker:
    image: user-projection-worker
    kind: worker
    readDb: CqrsDemo_User_Read
  reporting-queries:
    image: reporting-queries
    kind: api
    port: 8080
    readDb: CqrsDemo_Reporting
  reporting-projection-worker:
    image: reporting-projection-worker
    kind: worker
    readDb: CqrsDemo_Reporting
  checkout-saga-api:
    image: checkout-saga-api
    kind: api
    port: 8080
  checkout-saga-worker:
    image: checkout-saga-worker
    kind: worker
  audit-projection-worker:
    image: audit-projection-worker
    kind: worker

frontends:
  shop:
    image: shop
    port: 3000
    env:
      GATEWAY_URL: http://shop-gateway:8080
'@

$BaseYaml = @'
# Shared defaults and service catalog (environment-agnostic).
# Override per env with values-dev.yaml | values-staging.yaml | values-prod.yaml

namespace: cqrs-demo

image:
  registry: cqrsdemo
  tag: latest
  pullPolicy: IfNotPresent

replicaCount: 1

sql:
  host: sqlserver
  port: 1433
  user: sa
  password: Your_password123
  passwordSecret: cqrs-secrets
  passwordKey: MSSQL_SA_PASSWORD

kafka:
  bootstrapServers: kafka:9092
  topicName: shop-events

elasticsearch:
  url: http://elasticsearch:9200

observability:
  otlpEndpoint: http://otel-collector:4317

dbInit:
  enabled: true
  image: cqrsdemo/db-initializer

ingress:
  enabled: false
  className: nginx
  shopGatewayHost: shop.local
  adminApiHost: admin-api.local
  shopFrontendHost: shop-fe.local

nodePorts:
  enabled: false
  shopGateway: 30500
  shopAdminApi: 30510
  shopFrontend: 30501
  kibana: 30561

'@

$EnvOverrides = @{
    dev = @'
# Development - local cluster / Docker Desktop
namespace: cqrs-demo

image:
  pullPolicy: IfNotPresent

replicaCount: 1

ingress:
  enabled: true
  shopGatewayHost: shop.local
  adminApiHost: admin-api.local
  shopFrontendHost: shop-fe.local

nodePorts:
  enabled: true

dbInit:
  enabled: true
'@
    staging = @'
# Staging - pre-production
namespace: cqrs-demo-staging

image:
  pullPolicy: Always

replicaCount: 1

ingress:
  enabled: true
  shopGatewayHost: shop.staging.example.com
  adminApiHost: admin-api.staging.example.com
  shopFrontendHost: shop-fe.staging.example.com

nodePorts:
  enabled: true
  shopGateway: 31080
  shopAdminApi: 31081
  shopFrontend: 31082
  kibana: 31061

dbInit:
  enabled: true
'@
    prod = @'
# Production - use External Secrets for sql.password in real deployments
namespace: cqrs-demo-prod

image:
  pullPolicy: Always

replicaCount: 2

ingress:
  enabled: true
  shopGatewayHost: shop.example.com
  adminApiHost: admin-api.example.com
  shopFrontendHost: shop-fe.example.com

nodePorts:
  enabled: false

dbInit:
  enabled: false
'@
}

Set-Content -Path (Join-Path $ChartDir 'values.yaml') -Value ($BaseYaml.TrimEnd() + "`n" + $ServicesYaml.Trim()) -Encoding UTF8

foreach ($env in $EnvOverrides.Keys) {
    $path = Join-Path $ChartDir "values-$env.yaml"
    Set-Content -Path $path -Value $EnvOverrides[$env].Trim() -Encoding UTF8
    Write-Host "Wrote $path"
}

Write-Host "Wrote $(Join-Path $ChartDir 'values.yaml')"

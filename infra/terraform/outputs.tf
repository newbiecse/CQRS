output "namespace" {
  value = var.namespace
}

output "ingress_hosts" {
  value = {
    shop_gateway  = "shop.local"
    admin_api     = "admin-api.local"
    shop_frontend = "shop-fe.local"
  }
}

output "infra_services" {
  value = [
    "sqlserver:1433",
    "kafka:9092",
    "elasticsearch:9200",
    "kibana:5601",
    "otel-collector:4317",
  ]
}

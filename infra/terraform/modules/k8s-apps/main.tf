variable "namespace" {
  type = string
}

variable "image_registry" {
  type = string
}

variable "image_tag" {
  type = string
}

variable "helm_chart_path" {
  type = string
}

variable "sql_password" {
  type      = string
  sensitive = true
}

variable "deploy_apps" {
  type = bool
}

variable "deploy_db_init" {
  type = bool
}

variable "ingress_enabled" {
  type = bool
}

resource "helm_release" "cqrs_apps" {
  count = var.deploy_apps ? 1 : 0

  name       = "cqrs-apps"
  chart      = var.helm_chart_path
  namespace  = var.namespace
  create_namespace = false

  values = [
    yamlencode({
      namespace = var.namespace
      image = {
        registry   = var.image_registry
        tag        = var.image_tag
        pullPolicy = "IfNotPresent"
      }
      sql = {
        password = var.sql_password
      }
      dbInit = {
        enabled = var.deploy_db_init
        image   = "${var.image_registry}/db-initializer"
      }
      ingress = {
        enabled = var.ingress_enabled
      }
    })
  ]

  timeout = 600
}

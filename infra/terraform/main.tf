terraform {
  required_version = ">= 1.5.0"

  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.35"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.17"
    }
    kubectl = {
      source  = "alekc/kubectl"
      version = "~> 2.1"
    }
  }
}

provider "kubernetes" {
  config_path = var.kubeconfig_path
}

provider "helm" {
  kubernetes {
    config_path = var.kubeconfig_path
  }
}

provider "kubectl" {
  config_path = var.kubeconfig_path
}

locals {
  k8s_namespace     = var.environment == "dev" ? "cqrs-demo" : "cqrs-demo-${var.environment}"
  kustomize_path    = "${path.module}/../k8s/overlays/${var.environment}"
  helm_chart_path   = "${path.module}/../helm/cqrs-apps"
  helm_values_base  = "${local.helm_chart_path}/values.yaml"
  helm_values_env   = "${local.helm_chart_path}/values-${var.environment}.yaml"
}

module "k8s_infra" {
  source = "./modules/k8s-infra"

  kubeconfig_path = var.kubeconfig_path
  kustomize_path  = local.kustomize_path
}

module "k8s_apps" {
  source = "./modules/k8s-apps"

  depends_on = [module.k8s_infra]

  namespace          = local.k8s_namespace
  image_registry     = var.image_registry
  image_tag          = var.image_tag
  helm_chart_path    = local.helm_chart_path
  helm_values_base   = local.helm_values_base
  helm_values_env    = local.helm_values_env
  sql_password       = var.sql_password
  deploy_apps      = var.deploy_apps
  deploy_db_init   = var.deploy_db_init
  ingress_enabled  = var.ingress_enabled
}

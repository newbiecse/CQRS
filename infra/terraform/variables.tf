variable "environment" {
  type        = string
  description = "Target environment: dev, staging, or prod"
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be one of: dev, staging, prod."
  }
}

variable "kubeconfig_path" {
  type        = string
  description = "Path to kubeconfig file"
  default     = "~/.kube/config"
}

variable "image_registry" {
  type    = string
  default = "cqrsdemo"
}

variable "image_tag" {
  type    = string
  default = "latest"
}

variable "sql_password" {
  type      = string
  sensitive = true
  default   = "Your_password123"
}

variable "deploy_apps" {
  type    = bool
  default = true
}

variable "deploy_db_init" {
  type    = bool
  default = true
}

variable "ingress_enabled" {
  type    = bool
  default = true
}

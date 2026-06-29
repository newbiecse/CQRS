variable "kubeconfig_path" {
  type = string
}

variable "kustomize_path" {
  type = string
}

data "kubectl_kustomize" "infra" {
  directory = var.kustomize_path
}

resource "kubectl_manifest" "infra" {
  for_each  = data.kubectl_kustomize.infra.manifests
  yaml_body = each.value
}

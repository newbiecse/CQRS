#Requires -Version 5.1
<#
.SYNOPSIS
  Stop Kubernetes Dashboard port-forward started by start-local-k8s.ps1
#>
$JobName = "cqrs-k8s-dashboard-portforward"
Get-Job -Name $JobName -ErrorAction SilentlyContinue | Stop-Job -PassThru | Remove-Job -Force
Write-Host "Stopped dashboard port-forward job (if it was running)."

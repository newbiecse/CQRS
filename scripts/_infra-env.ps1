# Shared paths for dev / staging / prod infrastructure.

param(
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev'
)

$ErrorActionPreference = 'Stop'

$script:RepoRoot = if ($PSScriptRoot) {
    Split-Path -Parent $PSScriptRoot
} else {
    Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
}

function Get-InfraEnvironmentPaths {
    param(
        [ValidateSet('dev', 'staging', 'prod')]
        [string]$Environment = 'dev'
    )

    $dockerBase = Join-Path $script:RepoRoot 'infra\docker\base\docker-compose.yml'
    $dockerOverride = Join-Path $script:RepoRoot "infra\docker\env\$Environment\docker-compose.override.yml"
    $dockerEnvFile = Join-Path $script:RepoRoot "infra\docker\env\$Environment\.env"
    $helmChart = Join-Path $script:RepoRoot 'infra\helm\cqrs-apps'
    $helmfile = Join-Path $script:RepoRoot 'infra\helm\helmfile.yaml'

    $namespace = switch ($Environment) {
        'dev' { 'cqrs-demo' }
        'staging' { 'cqrs-demo-staging' }
        'prod' { 'cqrs-demo-prod' }
    }

    return [ordered]@{
        Environment        = $Environment
        Namespace          = $namespace
        DockerComposeFiles = @($dockerBase, $dockerOverride)
        DockerEnvFile      = if (Test-Path $dockerEnvFile) { $dockerEnvFile } else { $null }
        K8sOverlay         = Join-Path $script:RepoRoot "infra\k8s\overlays\$Environment"
        HelmChart          = $helmChart
        Helmfile           = $helmfile
        HelmValuesBase     = Join-Path $helmChart 'values.yaml'
        HelmValuesEnv      = Join-Path $helmChart "values-$Environment.yaml"
        TerraformTfvars    = Join-Path $script:RepoRoot "infra\terraform\env\$Environment.tfvars"
    }
}

function Invoke-DockerCompose {
    param(
        [ValidateSet('dev', 'staging', 'prod')]
        [string]$Environment = 'dev',
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$ComposeArgs
    )

    $paths = Get-InfraEnvironmentPaths -Environment $Environment
    $args = @('compose')
    foreach ($file in $paths.DockerComposeFiles) {
        $args += '-f'
        $args += $file
    }
    if ($paths.DockerEnvFile) {
        $args += '--env-file'
        $args += $paths.DockerEnvFile
    }
    $args += $ComposeArgs

    & docker @args
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed with exit code $LASTEXITCODE"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Get-InfraEnvironmentPaths -Environment $Environment
}

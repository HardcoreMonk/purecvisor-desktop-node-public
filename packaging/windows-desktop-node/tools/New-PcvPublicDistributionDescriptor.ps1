[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_PUBLIC_DISTRIBUTION_DESCRIPTOR_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the dry-run descriptor.'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$gateNames = @(
    'public-signing-preflight',
    'burn-bootstrapper-plan',
    'msix-feasibility-plan',
    'winget-manifest-plan',
    'updater-catalog-publication-plan',
    'public-signed-update-rollback-smoke-plan',
    'credential-manager-transition-plan',
    'eventlog-provider-default-plan',
    'tls-certificate-lifecycle-plan',
    'token-rotation-mutation-plan',
    'diagnostics-server-action-plan',
    'timeout-rate-limit-hardening-plan'
)

$gates = foreach ($name in $gateNames) {
    [ordered]@{
        name = $name
        status = 'planned'
        required_before_public_claim = $true
        actual_execution = 'not-run'
        host_mutation = 'not-run'
        evidence = 'missing'
    }
}

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    version = $Version
    scope = 'public-distribution-operations-expansion-candidate'
    artifact_root = $artifactRootFull
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    mutates_host = $false
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    public_release = 'not-claimed'
    created_at = (Get-Date).ToUniversalTime().ToString('o')
    gates = @($gates)
    command_plan = [ordered]@{
        public_distribution = [ordered]@{
            inputs_required = @(
                'public_trusted_signing_provider',
                'release_approval',
                'publication_target',
                'installer_url',
                'installer_sha256'
            )
            gates = @(
                'public-trusted-signing',
                'burn-bootstrapper',
                'msix',
                'winget-manifest',
                'updater-catalog-publication',
                'public-signed-update-rollback-smoke'
            )
        }
        operations_expansion = [ordered]@{
            gates = @(
                'windows-credential-manager-transition',
                'eventlog-provider-default',
                'built-in-tls-certificate-lifecycle',
                'token-rotation-mutation-api',
                'diagnostics-server-action',
                'timeout-rate-limit-hardening'
            )
        }
        safety = [ordered]@{
            allowed_mode = 'dry-run-descriptor-only'
            actual_execution = 'not-run'
            host_mutation = 'blocked-by-contract'
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
$summaryPath = Join-Path $artifactRootFull 'summary.json'
Set-Content -LiteralPath $summaryPath -Value $summaryJson -Encoding utf8

$summaryJson

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$CredentialTarget,

    [ValidateNotNullOrEmpty()]
    [string]$CurrentTokenStorage = 'dpapi-local-machine-protected-file',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_WINDOWS_CREDENTIAL_MANAGER_TRANSITION_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the transition preflight descriptor.'
}

if ([string]::IsNullOrWhiteSpace($ServiceName)) {
    throw 'PCV_WINDOWS_CREDENTIAL_MANAGER_TRANSITION_SERVICE_REQUIRED'
}

if ([string]::IsNullOrWhiteSpace($CredentialTarget)) {
    throw 'PCV_WINDOWS_CREDENTIAL_MANAGER_TRANSITION_TARGET_REQUIRED'
}

if ($CredentialTarget -match '[\x00-\x1F]') {
    throw 'PCV_WINDOWS_CREDENTIAL_MANAGER_TRANSITION_TARGET_INVALID'
}

if ([string]::IsNullOrWhiteSpace($CurrentTokenStorage)) {
    throw 'PCV_WINDOWS_CREDENTIAL_MANAGER_TRANSITION_CURRENT_STORAGE_REQUIRED'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$transitionPlanPath = Join-Path $artifactRootFull 'windows-credential-manager-transition.plan-preview.json'
$plannedOperations = @(
    'read-existing-protected-token-metadata',
    'write-credential-target',
    'reload-service-token',
    'verify-old-token-rejected',
    'rollback-to-protected-file'
)

$transitionPlan = [ordered]@{
    schema_version = 1
    scope = 'windows-credential-manager-transition-preflight'
    service_name = $ServiceName
    credential_target = $CredentialTarget
    current_token_storage = $CurrentTokenStorage
    target_token_storage = 'windows-credential-manager'
    migration_status = 'not-run'
    rollback_diagnostics = 'required-before-pass'
    token_value_observed = $false
    planned_operations = $plannedOperations
    planned_observations = @(
        'existing-token-metadata-only',
        'new-token-target-boundary',
        'service-token-reload-required',
        'old-token-rejection-required',
        'rollback-diagnostics-required'
    )
}

$transitionPlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $transitionPlanPath -Encoding utf8

$transitionChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'credential-target-present'; status = 'pass' },
    [ordered]@{ name = 'current-token-storage-recorded'; status = 'pass' },
    [ordered]@{ name = 'target-token-storage-recorded'; status = 'pass' },
    [ordered]@{ name = 'token-value-not-read'; status = 'not-run' },
    [ordered]@{ name = 'credential-write-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'credential-delete-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'rollback-diagnostics-required'; status = 'blocked' },
    [ordered]@{ name = 'service-reload-required'; status = 'blocked' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'windows-credential-manager-transition-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    credential_manager_transition = 'blocked-by-no-mutation-preflight'
    credential_manager_mutation = 'not-run'
    token_value_observed = $false
    service_name = $ServiceName
    credential_target = $CredentialTarget
    current_token_storage = $CurrentTokenStorage
    target_token_storage = 'windows-credential-manager'
    transition_plan_path = $transitionPlanPath
    transition_checks = @($transitionChecks)
    blockers = @(
        'credential mutation is not implemented in this preflight',
        'rollback diagnostics must be implemented before pass',
        'service token reload verification must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson

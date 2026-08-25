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
    [string]$ProtectedTokenPath,

    [ValidateNotNullOrEmpty()]
    [string]$CurrentTokenStorage = 'dpapi-local-machine-protected-file',

    [ValidateSet('rotate-and-revoke-old-token')]
    [string]$RotationMode = 'rotate-and-revoke-old-token',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_SERVICE_TOKEN_ROTATION_REVOKE_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the rotation preflight descriptor.'
}

foreach ($value in @($ServiceName, $ProtectedTokenPath, $CurrentTokenStorage, $RotationMode)) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw 'PCV_SERVICE_TOKEN_ROTATION_REVOKE_FIELD_REQUIRED'
    }

    if ($value -match '[\x00-\x1F]') {
        throw 'PCV_SERVICE_TOKEN_ROTATION_REVOKE_FIELD_INVALID'
    }
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$rotationPlanPath = Join-Path $artifactRootFull 'service-token-rotation-revoke.plan-preview.json'
$plannedOperations = @(
    'inspect-current-token-storage-policy',
    'plan-new-protected-token-record',
    'replace-protected-token-record',
    'reload-service-token-policy',
    'verify-old-token-rejected',
    'write-token-rotation-audit-record',
    'rollback-to-previous-protected-token'
)

$rotationPlan = [ordered]@{
    schema_version = 1
    scope = 'service-token-rotation-revoke-preflight'
    service_name = $ServiceName
    protected_token_path = $ProtectedTokenPath
    current_token_storage = $CurrentTokenStorage
    rotation_mode = $RotationMode
    service_token_value_observed = $false
    new_token_value_created = $false
    protected_token_write_status = 'not-run'
    service_reload_status = 'not-run'
    old_token_rejection_status = 'not-run'
    token_rotation_audit_status = 'not-run'
    rollback_status = 'not-run'
    planned_operations = $plannedOperations
    planned_observations = @(
        'existing-token-metadata-policy-only',
        'new-token-record-required-before-pass',
        'protected-token-replace-required-before-pass',
        'service-token-policy-reload-required-before-pass',
        'old-token-rejection-required-before-pass',
        'audit-record-required-before-pass',
        'rollback-diagnostics-required-before-pass'
    )
}

$rotationPlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $rotationPlanPath -Encoding utf8

$rotationChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'current-token-storage-recorded'; status = 'pass' },
    [ordered]@{ name = 'protected-token-path-recorded'; status = 'pass' },
    [ordered]@{ name = 'rotation-mode-recorded'; status = 'pass' },
    [ordered]@{ name = 'token-value-not-read'; status = 'not-run' },
    [ordered]@{ name = 'new-token-not-generated'; status = 'not-run' },
    [ordered]@{ name = 'protected-token-write-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'service-reload-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'old-token-rejection-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'audit-record-not-written'; status = 'not-run' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'service-token-rotation-revoke-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    service_token_rotation_revoke = 'blocked-by-no-mutation-preflight'
    service_token_mutation = 'not-run'
    service_token_value_observed = $false
    new_token_value_created = $false
    service_reload_status = 'not-run'
    old_token_rejection_status = 'not-run'
    token_rotation_audit_status = 'not-run'
    service_name = $ServiceName
    protected_token_path = $ProtectedTokenPath
    current_token_storage = $CurrentTokenStorage
    rotation_mode = $RotationMode
    rotation_plan_path = $rotationPlanPath
    rotation_checks = @($rotationChecks)
    blockers = @(
        'token value read and creation are not implemented in this preflight',
        'protected token replacement and service token policy reload must be implemented before pass',
        'old token rejection, audit record, and rollback diagnostics must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson

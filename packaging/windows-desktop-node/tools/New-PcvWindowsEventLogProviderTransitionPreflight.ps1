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
    [string]$ProviderName,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$LogName,

    [ValidateNotNullOrEmpty()]
    [string]$CurrentWriter = 'jsonl-first-eventlog-opt-in',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_WINDOWS_EVENT_LOG_PROVIDER_TRANSITION_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the transition preflight descriptor.'
}

foreach ($value in @($ServiceName, $ProviderName, $LogName, $CurrentWriter)) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw 'PCV_WINDOWS_EVENT_LOG_PROVIDER_TRANSITION_FIELD_REQUIRED'
    }

    if ($value -match '[\x00-\x1F]') {
        throw 'PCV_WINDOWS_EVENT_LOG_PROVIDER_TRANSITION_FIELD_INVALID'
    }
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$transitionPlanPath = Join-Path $artifactRootFull 'windows-event-log-provider-transition.plan-preview.json'
$plannedOperations = @(
    'inspect-current-jsonl-writer-policy',
    'register-event-log-provider',
    'switch-default-writer-to-event-log',
    'verify-provider-write-and-query',
    'remove-provider-on-uninstall-or-rollback',
    'enforce-log-volume-guard'
)

$transitionPlan = [ordered]@{
    schema_version = 1
    scope = 'windows-event-log-provider-transition-preflight'
    service_name = $ServiceName
    provider_name = $ProviderName
    log_name = $LogName
    current_writer = $CurrentWriter
    target_writer = 'default-windows-event-log-provider'
    provider_registration_status = 'not-run'
    provider_removal_status = 'not-run'
    event_write_status = 'not-run'
    retention_volume_guard = 'required-before-pass'
    planned_operations = $plannedOperations
    planned_observations = @(
        'provider-registration-required',
        'default-writer-switch-required',
        'provider-write-query-required',
        'provider-removal-repair-required',
        'log-volume-guard-required'
    )
}

$transitionPlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $transitionPlanPath -Encoding utf8

$transitionChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'provider-name-present'; status = 'pass' },
    [ordered]@{ name = 'log-name-present'; status = 'pass' },
    [ordered]@{ name = 'current-writer-recorded'; status = 'pass' },
    [ordered]@{ name = 'target-writer-recorded'; status = 'pass' },
    [ordered]@{ name = 'provider-registration-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'provider-removal-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'event-write-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'retention-volume-guard-required'; status = 'blocked' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'windows-event-log-provider-transition-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    event_log_provider_transition = 'blocked-by-no-mutation-preflight'
    event_log_provider_mutation = 'not-run'
    event_log_write_status = 'not-run'
    service_name = $ServiceName
    provider_name = $ProviderName
    log_name = $LogName
    current_writer = $CurrentWriter
    target_writer = 'default-windows-event-log-provider'
    transition_plan_path = $transitionPlanPath
    transition_checks = @($transitionChecks)
    blockers = @(
        'provider registration is not implemented in this preflight',
        'default writer switch must be implemented before pass',
        'event query and log volume guard evidence must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson

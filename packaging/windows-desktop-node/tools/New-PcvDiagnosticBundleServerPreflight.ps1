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
    [string]$DiagnosticsRoot,

    [ValidateNotNullOrEmpty()]
    [string]$ApiRoute = '/api/v1/diagnostics/bundles',

    [ValidateNotNullOrEmpty()]
    [string]$DownloadRouteTemplate = '/api/v1/diagnostics/bundles/{bundle_id}/download',

    [ValidateRange(1, 365)]
    [int]$RetentionDays = 14,

    [ValidateRange(1, 1000)]
    [int]$MaxBundleCount = 50,

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the diagnostic bundle preflight descriptor.'
}

foreach ($value in @($ServiceName, $DiagnosticsRoot, $ApiRoute, $DownloadRouteTemplate)) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_FIELD_REQUIRED'
    }

    if ($value -match '[\x00-\x1F]') {
        throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_FIELD_INVALID'
    }
}

if (-not $ApiRoute.StartsWith('/api/v1/', [System.StringComparison]::Ordinal)) {
    throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_API_ROUTE_INVALID'
}

if (-not $DownloadRouteTemplate.StartsWith('/api/v1/', [System.StringComparison]::Ordinal)) {
    throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_DOWNLOAD_ROUTE_INVALID'
}

if (-not $DownloadRouteTemplate.Contains('{bundle_id}', [System.StringComparison]::Ordinal)) {
    throw 'PCV_DIAGNOSTIC_BUNDLE_SERVER_PREFLIGHT_DOWNLOAD_ROUTE_MISSING_ID'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$diagnosticPlanPath = Join-Path $artifactRootFull 'diagnostic-bundle-server.plan-preview.json'
$plannedOperations = @(
    'validate-bearer-authorization',
    'request-diagnostic-bundle-generation',
    'write-server-side-bundle-archive',
    'serve-diagnostic-bundle-download',
    'apply-bundle-retention-policy',
    'record-bundle-audit-metadata'
)

$diagnosticPlan = [ordered]@{
    schema_version = 1
    scope = 'diagnostic-bundle-server-preflight'
    service_name = $ServiceName
    diagnostics_root = $DiagnosticsRoot
    api_route = $ApiRoute
    download_route_template = $DownloadRouteTemplate
    authz_policy = 'bearer-token-required'
    redaction_policy = 'token-and-host-path-redaction-required'
    retention_days = $RetentionDays
    max_bundle_count = $MaxBundleCount
    archive_status = 'not-run'
    download_status = 'not-run'
    retention_status = 'not-run'
    wrapper_collect_diagnostics_execution = 'not-run'
    planned_operations = $plannedOperations
    planned_observations = @(
        'local-api-action-required-before-pass',
        'server-side-archive-required-before-pass',
        'download-streaming-required-before-pass',
        'redaction-verification-required-before-pass',
        'authorization-policy-required-before-pass',
        'retention-policy-required-before-pass'
    )
}

$diagnosticPlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $diagnosticPlanPath -Encoding utf8

$diagnosticChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'diagnostics-root-recorded'; status = 'pass' },
    [ordered]@{ name = 'api-route-recorded'; status = 'pass' },
    [ordered]@{ name = 'download-route-recorded'; status = 'pass' },
    [ordered]@{ name = 'authz-policy-recorded'; status = 'pass' },
    [ordered]@{ name = 'archive-creation-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'download-serving-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'redaction-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'retention-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'wrapper-execution-not-delegated'; status = 'not-run' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'diagnostic-bundle-server-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    diagnostic_bundle_server_generation = 'blocked-by-no-mutation-preflight'
    diagnostic_bundle_api_action = 'not-run'
    diagnostic_bundle_archive_created = $false
    diagnostic_bundle_download_served = $false
    diagnostic_bundle_redaction_status = 'not-run'
    diagnostic_bundle_authz_status = 'not-run'
    diagnostic_bundle_retention_status = 'not-run'
    wrapper_collect_diagnostics_execution = 'not-run'
    service_name = $ServiceName
    diagnostics_root = $DiagnosticsRoot
    api_route = $ApiRoute
    download_route_template = $DownloadRouteTemplate
    retention_days = $RetentionDays
    max_bundle_count = $MaxBundleCount
    diagnostic_plan_path = $diagnosticPlanPath
    diagnostic_checks = @($diagnosticChecks)
    blockers = @(
        'Local API generation and download actions are not implemented in this preflight',
        'server-side archive, redaction, authorization, and retention verification must be implemented before pass',
        'product diagnostics runner delegation and audit metadata must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson

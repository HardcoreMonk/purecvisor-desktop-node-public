[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName,

    [ValidateNotNullOrEmpty()]
    [string]$ApiRoutePrefix = '/api/v1/',

    [ValidateRange(1, 3600)]
    [int]$RouteTimeoutSeconds = 30,

    [ValidateRange(1, 100000)]
    [int]$RequestLimitPerMinute = 120,

    [ValidateRange(1, 10000)]
    [int]$BurstLimit = 20,

    [ValidateRange(1, 3600)]
    [int]$RetryAfterSeconds = 15,

    [ValidateSet('problem-details-json')]
    [string]$ErrorContract = 'problem-details-json',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the timeout and rate-limit hardening descriptor.'
}

foreach ($value in @($ServiceName, $ApiRoutePrefix, $ErrorContract)) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw 'PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_FIELD_REQUIRED'
    }

    if ($value -match '[\x00-\x1F]') {
        throw 'PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_FIELD_INVALID'
    }
}

if (-not $ApiRoutePrefix.StartsWith('/api/v1/', [System.StringComparison]::Ordinal)) {
    throw 'PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_API_ROUTE_PREFIX_INVALID'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$hardeningPlanPath = Join-Path $artifactRootFull 'timeout-rate-limit-hardening.plan-preview.json'
$plannedOperations = @(
    'configure-route-timeout-middleware',
    'configure-request-rate-limit-middleware',
    'emit-retry-after-contract',
    'map-timeout-rate-limit-errors',
    'verify-ui-api-error-contract',
    'run-rate-limit-load-test',
    'record-operational-metrics'
)

$hardeningPlan = [ordered]@{
    schema_version = 1
    scope = 'timeout-rate-limit-hardening-preflight'
    service_name = $ServiceName
    api_route_prefix = $ApiRoutePrefix
    route_timeout_seconds = $RouteTimeoutSeconds
    request_limit_per_minute = $RequestLimitPerMinute
    burst_limit = $BurstLimit
    retry_after_seconds = $RetryAfterSeconds
    error_contract = $ErrorContract
    middleware_status = 'not-enabled'
    load_test_status = 'not-run'
    server_config_status = 'not-mutated'
    planned_operations = $plannedOperations
    planned_observations = @(
        'route-timeout-middleware-required-before-pass',
        'request-rate-limit-middleware-required-before-pass',
        'retry-after-and-problem-details-contract-required-before-pass',
        'web-ui-error-contract-required-before-pass',
        'load-test-evidence-required-before-pass',
        'operational-metrics-required-before-pass'
    )
}

$hardeningPlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $hardeningPlanPath -Encoding utf8

$hardeningChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'api-route-prefix-recorded'; status = 'pass' },
    [ordered]@{ name = 'timeout-policy-recorded'; status = 'pass' },
    [ordered]@{ name = 'request-limit-policy-recorded'; status = 'pass' },
    [ordered]@{ name = 'retry-semantics-recorded'; status = 'pass' },
    [ordered]@{ name = 'ui-api-error-contract-recorded'; status = 'pass' },
    [ordered]@{ name = 'server-config-not-mutated'; status = 'not-run' },
    [ordered]@{ name = 'middleware-not-enabled'; status = 'not-run' },
    [ordered]@{ name = 'load-test-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'timeout-rate-limit-hardening-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    timeout_rate_limit_hardening = 'blocked-by-no-mutation-preflight'
    route_timeout_policy = 'not-applied'
    request_limit_policy = 'not-applied'
    retry_semantics_status = 'not-run'
    ui_api_error_contract_status = 'not-run'
    load_test_status = 'not-run'
    server_config_mutation = 'not-run'
    service_name = $ServiceName
    api_route_prefix = $ApiRoutePrefix
    route_timeout_seconds = $RouteTimeoutSeconds
    request_limit_per_minute = $RequestLimitPerMinute
    burst_limit = $BurstLimit
    retry_after_seconds = $RetryAfterSeconds
    error_contract = $ErrorContract
    hardening_plan_path = $hardeningPlanPath
    hardening_checks = @($hardeningChecks)
    blockers = @(
        'route timeout middleware and request rate-limit middleware are not enabled in this preflight',
        'retry semantics and UI/API error contract verification must be implemented before pass',
        'server config mutation, load test evidence, and operational metrics must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson

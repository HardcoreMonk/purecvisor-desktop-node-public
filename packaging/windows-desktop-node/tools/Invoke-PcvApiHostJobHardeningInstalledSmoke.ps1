[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("api-host-job-hardening-installed-evidence-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$ApiBaseUri = 'http://127.0.0.1:7777',
    [string]$BearerToken = '',
    [AllowEmptyString()]
    [string]$BearerTokenEnvironmentVariableName = 'PCV_API_HOST_JOB_HARDENING_SMOKE_TOKEN',
    [ValidateRange(1048577, 67108864)]
    [int]$OversizedBodyBytes = 2097152,
    [ValidateRange(1, 300)]
    [int]$ResponsivenessTimeoutSeconds = 20,
    [ValidateRange(1, 1000)]
    [int]$RateLimitProbeRequests = 145,
    [switch]$RunRouteTimeoutProbe,
    [switch]$RunRateLimitProbe,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
} else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$started = Get-Date
$environmentBearerToken = if ([string]::IsNullOrWhiteSpace($BearerTokenEnvironmentVariableName)) {
    ''
} else {
    [string][System.Environment]::GetEnvironmentVariable($BearerTokenEnvironmentVariableName, 'Process')
}
$resolvedBearerToken = if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
    $BearerToken
} elseif (-not [string]::IsNullOrWhiteSpace($environmentBearerToken)) {
    $environmentBearerToken
} else {
    ''
}
$bearerTokenSource = if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
    'parameter-explicit-compatibility'
} elseif (-not [string]::IsNullOrWhiteSpace($environmentBearerToken)) {
    'environment-variable'
} else {
    'none'
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 24 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-Sha256Hex {
    param([AllowNull()][string]$Value)

    if ($null -eq $Value) {
        return $null
    }

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    ([System.BitConverter]::ToString($hash)).Replace('-', '').ToLowerInvariant()
}

function ConvertTo-PcvResponseBodyText {
    param([AllowNull()]$Content)

    if ($null -eq $Content) {
        return ''
    }

    if ($Content -is [byte[]]) {
        return [System.Text.Encoding]::UTF8.GetString($Content)
    }

    [string]$Content
}

function Get-PcvProblemCode {
    param([AllowNull()][string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $null
    }

    try {
        $json = $Body | ConvertFrom-Json -ErrorAction Stop
        if ($null -ne $json.code) {
            return [string]$json.code
        }

        if ($null -ne $json.error -and $null -ne $json.error.code) {
            return [string]$json.error.code
        }
    }
    catch {
    }

    if ($Body -match 'PCV_[A-Z0-9_]+') {
        return $Matches[0]
    }

    return $null
}

function Get-PcvProblemField {
    param(
        [AllowNull()][string]$Body,
        [Parameter(Mandatory)][string]$Field
    )

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $null
    }

    try {
        $json = $Body | ConvertFrom-Json -ErrorAction Stop
        if ($null -ne $json.$Field) {
            return $json.$Field
        }

        if ($null -ne $json.error -and $null -ne $json.error.$Field) {
            return $json.error.$Field
        }
    }
    catch {
    }

    return $null
}

function ConvertTo-PcvResponseEvidence {
    param(
        [Parameter(Mandatory)]$Response,
        [AllowEmptyString()]
        [Parameter(Mandatory)][string]$ExpectedErrorCode
    )

    $body = [string]$Response.body
    $statusCode = [int]$Response.status_code
    $errorCode = if ([string]::IsNullOrWhiteSpace($ExpectedErrorCode) -and $statusCode -ge 200 -and $statusCode -lt 300) {
        $null
    } else {
        Get-PcvProblemCode -Body $body
    }
    $expectedErrorObserved = if ([string]::IsNullOrWhiteSpace($ExpectedErrorCode)) {
        [string]::IsNullOrWhiteSpace($errorCode)
    } else {
        $errorCode -eq $ExpectedErrorCode
    }

    [pscustomobject][ordered]@{
        method = [string]$Response.method
        path = [string]$Response.path
        status_code = $statusCode
        content_type = [string]$Response.content_type
        duration_ms = [int]$Response.duration_ms
        error_code = $errorCode
        expected_error_code = $ExpectedErrorCode
        expected_error_observed = $expectedErrorObserved
        retry_after = [string]$Response.retry_after
        request_id = [string](Get-PcvProblemField -Body $body -Field 'request_id')
        body_sha256 = Get-Sha256Hex -Value $body
        body_length = $body.Length
    }
}

function Add-PcvEvidenceProperty {
    param(
        [Parameter(Mandatory)]$Object,
        [Parameter(Mandatory)][string]$Name,
        [AllowNull()]$Value
    )

    $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value -Force
}

function Invoke-PcvSmokeRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Body,
        [int]$TimeoutSec = 20
    )

    $headers = @{
        'X-PCV-Request-Id' = ('req-installed-hardening-' + [Guid]::NewGuid().ToString('N'))
    }
    if (-not [string]::IsNullOrWhiteSpace($resolvedBearerToken)) {
        $headers['Authorization'] = "Bearer $resolvedBearerToken"
    }

    $request = @{
        Method = $Method
        Uri = ($ApiBaseUri.TrimEnd('/') + $Path)
        Headers = $headers
        TimeoutSec = $TimeoutSec
        ErrorAction = 'Stop'
    }
    if ($null -ne $Body) {
        $request.Body = $Body
        $request.ContentType = 'application/json'
    }
    $invokeWebRequest = Get-Command Invoke-WebRequest
    if ($invokeWebRequest.Parameters.ContainsKey('UseBasicParsing')) {
        $request.UseBasicParsing = $true
    }
    if ($invokeWebRequest.Parameters.ContainsKey('SkipHttpErrorCheck')) {
        $request.SkipHttpErrorCheck = $true
    }

    $requestStarted = Get-Date
    try {
        $response = Invoke-WebRequest @request
        $finished = Get-Date
        [pscustomobject][ordered]@{
            ok = ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300)
            method = $Method
            path = $Path
            status_code = [int]$response.StatusCode
            content_type = [string]$response.Headers['Content-Type']
            duration_ms = [int]($finished - $requestStarted).TotalMilliseconds
            retry_after = [string]$response.Headers['Retry-After']
            body = ConvertTo-PcvResponseBodyText -Content $response.Content
            transport_error = $null
        }
    }
    catch {
        $finished = Get-Date
        $statusCode = 0
        $contentType = ''
        $retryAfter = ''
        $body = ''
        if ($null -ne $_.Exception.Response) {
            try {
                $statusCode = [int]$_.Exception.Response.StatusCode
                $contentType = [string]$_.Exception.Response.Headers['Content-Type']
                $retryAfter = [string]$_.Exception.Response.Headers['Retry-After']
                $stream = $_.Exception.Response.GetResponseStream()
                if ($null -ne $stream) {
                    $reader = [System.IO.StreamReader]::new($stream)
                    try {
                        $body = $reader.ReadToEnd()
                    }
                    finally {
                        $reader.Dispose()
                    }
                }
            }
            catch {
                $body = [string]$_.ErrorDetails.Message
            }
        }

        [pscustomobject][ordered]@{
            ok = $false
            method = $Method
            path = $Path
            status_code = $statusCode
            content_type = $contentType
            duration_ms = [int]($finished - $requestStarted).TotalMilliseconds
            retry_after = $retryAfter
            body = $body
            transport_error = $_.Exception.Message
        }
    }
}

function Get-ServiceSnapshot {
    param([Parameter(Mandatory)][string]$Name)

    $service = Get-CimInstance Win32_Service -Filter "Name='$Name'" -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return [pscustomobject][ordered]@{
            exists = $false
            name = $Name
            state = $null
            start_mode = $null
            process_id = $null
        }
    }

    [pscustomobject][ordered]@{
        exists = $true
        name = [string]$service.Name
        state = [string]$service.State
        start_mode = [string]$service.StartMode
        process_id = [int]$service.ProcessId
    }
}

function New-BaseSummary {
    [ordered]@{
        schema_version = 1
        scope = 'api-host-job-hardening-installed-smoke'
        artifact_root = $artifactRootFull
        service_name = $ServiceName
        api_base_uri = $ApiBaseUri
        installed_listener_required = $true
        bearer_token_supplied = -not [string]::IsNullOrWhiteSpace($resolvedBearerToken)
        bearer_token_source = $bearerTokenSource
        bearer_token_environment_variable_name = $BearerTokenEnvironmentVariableName
        bearer_token_parameter_warning = if ($bearerTokenSource -eq 'parameter-explicit-compatibility') { 'compatibility-only; prefer environment variable token source to avoid argv exposure' } else { $null }
        token_value_observed = $false
        password_value_observed = $false
        refresh_token_value_observed = $false
        host_mutation_performed = $false
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        cooperative_cancellation_scope = 'route-timeout-token-and-background-worker-token-code-level; installed smoke records listener behavior and does not claim forced WMI abort'
        wmi_abort_claim = 'not-claimed'
        body_cap = [ordered]@{
            method = 'POST'
            path = '/api/v1/auth/login'
            oversized_body_bytes = $OversizedBodyBytes
            expected_status_code = 413
            expected_error_code = 'PCV_REQUEST_BODY_TOO_LARGE'
            expected_content_type = 'application/problem+json'
        }
        route_timeout = [ordered]@{
            status = if ($RunRouteTimeoutProbe) { 'pending-controlled-route-timeout-probe' } else { 'not-run-by-default-controlled-route-timeout-probe' }
            method = 'GET'
            path = '/api/v1/runtime/route-timeout-probe'
            expected_status_code = 504
            expected_error_code = 'PCV_ROUTE_TIMEOUT'
            expected_content_type = 'application/problem+json'
            expected_retry_after_present = $true
            note = 'The controlled route-timeout probe is disabled by default and requires an installed listener started with --controlled-route-timeout-probe-delay-ms.'
        }
        rate_limit = [ordered]@{
            status = if ($RunRateLimitProbe) { 'pending' } else { 'not-run-by-default-controlled-load-probe' }
            request_count = if ($RunRateLimitProbe) { $RateLimitProbeRequests } else { 0 }
            expected_status_code = 429
            expected_error_code = 'PCV_RATE_LIMIT_EXCEEDED'
            expected_content_type = 'application/problem+json'
            expected_retry_after_present = $true
        }
        job_readability = [ordered]@{
            method = 'GET'
            path = '/api/v1/jobs?limit=1&offset=0'
            expected_status_code = 200
        }
        job_cancellation = [ordered]@{
            method = 'POST'
            path = '/api/v1/jobs/pcv-installed-hardening-missing-job/cancel'
            expected_non_mutating_status_codes = @(404, 401, 403)
            note = '404 proves the cancellation route reached job policy without creating or mutating a job; 401/403 means the supplied token cannot exercise operate permission.'
        }
        worker_responsiveness = [ordered]@{
            threshold_ms = $ResponsivenessTimeoutSeconds * 1000
            observed_nonblocking = $false
        }
    }
}

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

if ($DryRun) {
    $summary = New-BaseSummary
    $summary.ok = $true
    $summary.actual_execution = 'dry-run-no-http'
    $summary.completed_at = (Get-Date).ToString('o')
    $summary.started_at = $started.ToString('o')
    Write-JsonFile -Path $summaryPath -Value $summary
    Write-Output ($summary | ConvertTo-Json -Depth 24)
    exit 0
}

$errorRecord = $null
$ok = $false
try {
    $beforeService = Get-ServiceSnapshot -Name $ServiceName
    $oversizedBody = '{"username":"pcv-installed-hardening-smoke","password":"' + ('x' * [Math]::Max(1, $OversizedBodyBytes)) + '"}'

    $bodyCapResponse = Invoke-PcvSmokeRequest -Method 'POST' -Path '/api/v1/auth/login' -Body $oversizedBody -TimeoutSec $ResponsivenessTimeoutSeconds
    $runtimePolicyResponse = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/runtime/policy' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    $jobsResponse = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/jobs?limit=1&offset=0' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    $diagnosticsResponse = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/diagnostics/bundles?limit=1&offset=0' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    $consoleResponse = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/console/capabilities' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    $cancelResponse = Invoke-PcvSmokeRequest -Method 'POST' -Path '/api/v1/jobs/pcv-installed-hardening-missing-job/cancel' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    $routeTimeoutResponse = $null
    if ($RunRouteTimeoutProbe) {
        $routeTimeoutResponse = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/runtime/route-timeout-probe' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
    }

    $rateLimitResponses = @()
    if ($RunRateLimitProbe) {
        for ($index = 0; $index -lt $RateLimitProbeRequests; $index++) {
            $rateLimitResponses += Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/runtime/policy' -Body $null -TimeoutSec $ResponsivenessTimeoutSeconds
            if (($rateLimitResponses[-1]).status_code -eq 429) {
                break
            }
        }
    }

    $afterService = Get-ServiceSnapshot -Name $ServiceName
    $bodyCapEvidence = ConvertTo-PcvResponseEvidence -Response $bodyCapResponse -ExpectedErrorCode 'PCV_REQUEST_BODY_TOO_LARGE'
    $runtimeEvidence = ConvertTo-PcvResponseEvidence -Response $runtimePolicyResponse -ExpectedErrorCode ''
    $jobsEvidence = ConvertTo-PcvResponseEvidence -Response $jobsResponse -ExpectedErrorCode ''
    $diagnosticsEvidence = ConvertTo-PcvResponseEvidence -Response $diagnosticsResponse -ExpectedErrorCode ''
    $consoleEvidence = ConvertTo-PcvResponseEvidence -Response $consoleResponse -ExpectedErrorCode ''
    $cancelEvidence = ConvertTo-PcvResponseEvidence -Response $cancelResponse -ExpectedErrorCode 'PCV_JOB_NOT_FOUND'
    $routeTimeoutEvidence = $null
    if ($RunRouteTimeoutProbe) {
        $routeTimeoutEvidence = ConvertTo-PcvResponseEvidence -Response $routeTimeoutResponse -ExpectedErrorCode 'PCV_ROUTE_TIMEOUT'
    }
    $cancelExpectedErrorCodes = @('PCV_JOB_NOT_FOUND', 'PCV_AUTH_REQUIRED', 'PCV_AUTH_FORBIDDEN', 'PCV_RBAC_FORBIDDEN')
    $readProbeExpectedStatusCodes = @(200)

    $readDurations = @(
        $runtimePolicyResponse.duration_ms
        $jobsResponse.duration_ms
        $diagnosticsResponse.duration_ms
        $consoleResponse.duration_ms
    )
    $workerResponsiveness = [ordered]@{
        threshold_ms = $ResponsivenessTimeoutSeconds * 1000
        runtime_policy_duration_ms = $runtimePolicyResponse.duration_ms
        jobs_duration_ms = $jobsResponse.duration_ms
        diagnostics_duration_ms = $diagnosticsResponse.duration_ms
        console_capabilities_duration_ms = $consoleResponse.duration_ms
        observed_nonblocking = (@($readDurations | Where-Object { $_ -ge ($ResponsivenessTimeoutSeconds * 1000) }).Count -eq 0)
    }

    $rateLimitEvidence = [ordered]@{
        status = if ($RunRateLimitProbe) { 'executed-controlled-load-probe' } else { 'not-run-by-default-controlled-load-probe' }
        request_count = @($rateLimitResponses).Count
        expected_status_code = 429
        expected_error_code = 'PCV_RATE_LIMIT_EXCEEDED'
        expected_content_type = 'application/problem+json'
        expected_retry_after_present = $true
        observed_200 = @($rateLimitResponses | Where-Object { $_.status_code -eq 200 }).Count
        observed_429 = @($rateLimitResponses | Where-Object { $_.status_code -eq 429 }).Count
        unexpected_status_codes = @($rateLimitResponses | Where-Object { $_.status_code -notin @(200, 429) } | ForEach-Object { $_.status_code } | Select-Object -Unique)
        first_429 = $null
        expected_content_type_observed = $false
        expected_retry_after_observed = $false
    }
    $firstRateLimit = @($rateLimitResponses | Where-Object { $_.status_code -eq 429 } | Select-Object -First 1)
    if (@($firstRateLimit).Count -gt 0) {
        $rateLimitEvidence.first_429 = ConvertTo-PcvResponseEvidence -Response $firstRateLimit[0] -ExpectedErrorCode 'PCV_RATE_LIMIT_EXCEEDED'
        $rateLimitEvidence.expected_content_type_observed = $rateLimitEvidence.first_429.content_type -like 'application/problem+json*'
        $rateLimitEvidence.expected_retry_after_observed = -not [string]::IsNullOrWhiteSpace($rateLimitEvidence.first_429.retry_after)
    }

    $summary = New-BaseSummary
    $summary.actual_execution = 'installed-listener-readonly-http-smoke'
    $summary.before_service = $beforeService
    $summary.after_service = $afterService
    $summary.body_cap = $bodyCapEvidence
    Add-PcvEvidenceProperty -Object $summary.body_cap -Name 'expected_content_type' -Value 'application/problem+json'
    Add-PcvEvidenceProperty -Object $summary.body_cap -Name 'expected_content_type_observed' -Value ($bodyCapEvidence.content_type -like 'application/problem+json*')
    if ($RunRouteTimeoutProbe) {
        $summary.route_timeout = $routeTimeoutEvidence
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'status' -Value 'executed-controlled-route-timeout-probe'
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'expected_status_code' -Value 504
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'expected_content_type' -Value 'application/problem+json'
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'expected_content_type_observed' -Value ($routeTimeoutEvidence.content_type -like 'application/problem+json*')
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'expected_retry_after_present' -Value $true
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'expected_retry_after_observed' -Value (-not [string]::IsNullOrWhiteSpace($routeTimeoutEvidence.retry_after))
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'cooperative_cancellation_scope' -Value $summary.cooperative_cancellation_scope
        Add-PcvEvidenceProperty -Object $summary.route_timeout -Name 'wmi_abort_claim' -Value $summary.wmi_abort_claim
    } else {
        $summary.route_timeout = [ordered]@{
            status = 'not-run-by-default-controlled-route-timeout-probe'
            method = 'GET'
            path = '/api/v1/runtime/route-timeout-probe'
            expected_status_code = 504
            expected_error_code = 'PCV_ROUTE_TIMEOUT'
            expected_content_type = 'application/problem+json'
            expected_retry_after_present = $true
            cooperative_cancellation_scope = $summary.cooperative_cancellation_scope
            wmi_abort_claim = $summary.wmi_abort_claim
        }
    }
    $summary.rate_limit = $rateLimitEvidence
    $summary.runtime_policy = $runtimeEvidence
    $summary.job_readability = $jobsEvidence
    $summary.diagnostics_readability = $diagnosticsEvidence
    Add-PcvEvidenceProperty -Object $summary.diagnostics_readability -Name 'expected_status_codes' -Value $readProbeExpectedStatusCodes
    Add-PcvEvidenceProperty -Object $summary.diagnostics_readability -Name 'expected_contract_observed' -Value ($diagnosticsResponse.status_code -eq 200)
    $summary.console_capabilities = $consoleEvidence
    Add-PcvEvidenceProperty -Object $summary.console_capabilities -Name 'expected_status_codes' -Value $readProbeExpectedStatusCodes
    Add-PcvEvidenceProperty -Object $summary.console_capabilities -Name 'expected_contract_observed' -Value ($consoleResponse.status_code -eq 200)
    $summary.job_cancellation = $cancelEvidence
    Add-PcvEvidenceProperty -Object $summary.job_cancellation -Name 'expected_non_mutating_status_codes' -Value @(404, 401, 403)
    Add-PcvEvidenceProperty -Object $summary.job_cancellation -Name 'expected_error_codes' -Value $cancelExpectedErrorCodes
    Add-PcvEvidenceProperty -Object $summary.job_cancellation -Name 'expected_contract_observed' -Value (
        $cancelResponse.status_code -in @(404, 401, 403) -and
        $cancelEvidence.error_code -in @('PCV_JOB_NOT_FOUND', 'PCV_AUTH_REQUIRED', 'PCV_AUTH_FORBIDDEN', 'PCV_RBAC_FORBIDDEN'))
    $summary.worker_responsiveness = $workerResponsiveness
    $summary.started_at = $started.ToString('o')
    $summary.completed_at = (Get-Date).ToString('o')

    $ok = $bodyCapResponse.status_code -eq 413 -and
        $bodyCapEvidence.error_code -eq 'PCV_REQUEST_BODY_TOO_LARGE' -and
        $bodyCapEvidence.content_type -like 'application/problem+json*' -and
        $runtimePolicyResponse.status_code -eq 200 -and
        $jobsResponse.status_code -eq 200 -and
        $diagnosticsResponse.status_code -eq 200 -and
        $consoleResponse.status_code -eq 200 -and
        $cancelResponse.status_code -in @(404, 401, 403) -and
        $cancelEvidence.error_code -in @('PCV_JOB_NOT_FOUND', 'PCV_AUTH_REQUIRED', 'PCV_AUTH_FORBIDDEN', 'PCV_RBAC_FORBIDDEN') -and
        [bool]$workerResponsiveness.observed_nonblocking -and
        [bool]$afterService.exists -and
        [string]$afterService.state -eq 'Running'

    if ($RunRateLimitProbe) {
        $ok = $ok -and
            $rateLimitEvidence.observed_429 -gt 0 -and
            $null -ne $rateLimitEvidence.first_429 -and
            $rateLimitEvidence.first_429.error_code -eq 'PCV_RATE_LIMIT_EXCEEDED' -and
            $rateLimitEvidence.first_429.content_type -like 'application/problem+json*' -and
            -not [string]::IsNullOrWhiteSpace($rateLimitEvidence.first_429.retry_after)
    }

    if ($RunRouteTimeoutProbe) {
        $ok = $ok -and
            $routeTimeoutEvidence.status_code -eq 504 -and
            $routeTimeoutEvidence.error_code -eq 'PCV_ROUTE_TIMEOUT' -and
            $routeTimeoutEvidence.content_type -like 'application/problem+json*' -and
            -not [string]::IsNullOrWhiteSpace($routeTimeoutEvidence.retry_after)
    }

    $summary.ok = [bool]$ok
    Write-JsonFile -Path $summaryPath -Value $summary
    Write-Output ($summary | ConvertTo-Json -Depth 24)
}
catch {
    $errorRecord = [ordered]@{
        message = $_.Exception.Message
        category = [string]$_.CategoryInfo.Category
    }
    $summary = New-BaseSummary
    $summary.ok = $false
    $summary.actual_execution = 'failed-before-summary-complete'
    $summary.error = $errorRecord
    $summary.started_at = $started.ToString('o')
    $summary.completed_at = (Get-Date).ToString('o')
    Write-JsonFile -Path $summaryPath -Value $summary
    throw
}

if (-not $ok) {
    exit 1
}

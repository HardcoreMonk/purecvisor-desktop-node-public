Set-StrictMode -Version Latest

$script:PcvApiJobs = @{}
$script:PcvApiJobQueue = [System.Collections.Generic.Queue[string]]::new()

function New-PcvApiError {
    param(
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    [ordered]@{
        code = $Code
        message = $Message
        detail = $Detail
        retryable = $Retryable
    }
}

function New-PcvApiBody {
    param(
        [Parameter(Mandatory)][bool]$Ok,
        [Parameter(Mandatory)][string]$Operation,
        [AllowNull()]$Data,
        [AllowNull()]$ErrorObject
    )

    [ordered]@{
        ok = $Ok
        operation = $Operation
        data = $Data
        error = $ErrorObject
    }
}

function ConvertTo-PcvApiJson {
    param([Parameter(Mandatory, ValueFromPipeline)]$Value)
    $Value | ConvertTo-Json -Depth 30 -Compress
}

function New-PcvApiResponse {
    param(
        [Parameter(Mandatory)][int]$Status,
        [Parameter(Mandatory)]$Body
    )

    [ordered]@{
        status = $Status
        headers = [ordered]@{
            'Content-Type' = 'application/json; charset=utf-8'
        }
        body = ($Body | ConvertTo-PcvApiJson)
    }
}

function New-PcvApiFailureResponse {
    param(
        [Parameter(Mandatory)][int]$Status,
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    New-PcvApiResponse `
        -Status $Status `
        -Body (New-PcvApiBody `
            -Ok $false `
            -Operation $Operation `
            -Data $null `
            -ErrorObject (New-PcvApiError `
                -Code $Code `
                -Message $Message `
                -Detail $Detail `
                -Retryable $Retryable))
}

function New-PcvHelperFailure {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    New-PcvApiBody `
        -Ok $false `
        -Operation $Operation `
        -Data $null `
        -ErrorObject (New-PcvApiError `
            -Code $Code `
            -Message $Message `
            -Detail $Detail `
            -Retryable $Retryable)
}

function Get-PcvDefaultHyperVHelperPath {
    $desktopRoot = Split-Path -Parent $PSScriptRoot
    Join-Path (Join-Path $desktopRoot 'hyperv') 'Invoke-PcvHyperV.ps1'
}

function Clear-PcvApiJobStore {
    $script:PcvApiJobs = @{}
    $script:PcvApiJobQueue = [System.Collections.Generic.Queue[string]]::new()
}

function Get-PcvApiJobQueueIds {
    @($script:PcvApiJobQueue.ToArray())
}

function Get-PcvApiTimestamp {
    [DateTimeOffset]::UtcNow.ToString('o')
}

function Get-PcvApiRuntimePolicy {
    param(
        [string]$TokenStorage = 'external_token_file',
        [ValidateSet('loopback', 'lan')][string]$CurrentExposure = 'loopback'
    )

    [ordered]@{
        persistence = [ordered]@{
            backend = 'json-file'
            database_backed = $false
        }
        retry = [ordered]@{
            automatic = $false
            manual = $true
            max_attempts = 3
            backoff = 'deferred'
        }
        cancel = [ordered]@{
            queued = $true
            running = $false
        }
        worker = [ordered]@{
            mode = 'bounded_tick'
            threaded = $false
        }
        job_runtime = [ordered]@{
            contract_version = 1
            owner = 'local-api'
            state_store = [ordered]@{
                backend = 'script-scope-memory'
                persistence = 'json-file-snapshot'
                corrupt_store = 'quarantine-and-start-empty'
                unsupported_future_version = 'quarantine-and-start-empty'
            }
            dispatch = [ordered]@{
                mode = 'bounded-synchronous-worker-tick'
                helper_boundary = 'hyperv-helper-process'
            }
            control = [ordered]@{
                cancel = [ordered]@{
                    queued_only = $true
                    running_interrupt = $false
                }
                retry = [ordered]@{
                    manual_only = $true
                    failed_error_retryable_only = $true
                    max_attempts = 3
                    creates_new_job = $true
                }
            }
            host_mutation = 'helper-process-only'
            orchestration = [ordered]@{
                primary = 'powershell'
                contract = 'plan-contract-injectable-runner-diagnostics'
            }
            native_core = [ordered]@{
                status = 'not-planned-unless-runtime-boundary-deepens'
                reason = 'windows-hyperv-orchestration-not-dataplane'
                revisit_when = 'state-machine-or-supervision-outgrows-powershell'
            }
        }
        cors = [ordered]@{
            enabled = $false
            options_preflight = $false
        }
        auth = [ordered]@{
            mode = 'single_bearer_token'
            multi_user = $false
            rbac = $false
            token_storage = $TokenStorage
        }
        network = [ordered]@{
            default_exposure = 'loopback'
            current_exposure = $CurrentExposure
            lan_mode = 'preview-admin-opt-in'
            static_asset_auth = [ordered]@{
                loopback = 'unauthenticated-static-only'
                non_loopback = 'bearer-required'
            }
            tls = [ordered]@{
                provided_by_product_wrapper = $false
                required_for_lan = $true
                termination = 'external-reverse-proxy-or-tls-terminator'
            }
            firewall = [ordered]@{
                enabled_by_default = $false
                lifecycle_owner = 'admin-opt-in-product-action-or-manual-command'
                installer_auto_enable = $false
                default_profile = 'private'
            }
        }
    }
}

function New-PcvApiJob {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [AllowNull()]$Params,
        [AllowNull()][string]$RetryOf,
        [ValidateRange(1, 1000000)][int]$Attempt = 1
    )

    $now = Get-PcvApiTimestamp
    $jobId = 'job-' + ([guid]::NewGuid().ToString('N'))
    $job = [ordered]@{
        job_id = $jobId
        operation = $Operation
        status = 'queued'
        params = $Params
        result = $null
        error = $null
        retry_of = $RetryOf
        attempt = $Attempt
        canceled_at = $null
        created_at = $now
        updated_at = $now
    }

    $script:PcvApiJobs[$jobId] = $job
    $job
}

function Get-PcvApiJob {
    param([Parameter(Mandatory)][string]$JobId)

    if (-not $script:PcvApiJobs.ContainsKey($JobId)) {
        return $null
    }

    $script:PcvApiJobs[$JobId]
}

function Add-PcvApiJobToQueue {
    param([Parameter(Mandatory)]$Job)

    $script:PcvApiJobQueue.Enqueue([string]$Job.job_id)
}

function Remove-PcvApiJobFromQueue {
    param([Parameter(Mandatory)][string]$JobId)

    $existingIds = @($script:PcvApiJobQueue.ToArray())
    $remainingIds = @($existingIds | Where-Object { $_ -ne $JobId })

    $script:PcvApiJobQueue = [System.Collections.Generic.Queue[string]]::new()
    foreach ($queuedJobId in $remainingIds) {
        $script:PcvApiJobQueue.Enqueue([string]$queuedJobId)
    }

    $existingIds.Count -ne $remainingIds.Count
}

function Get-PcvApiJobStoreSnapshot {
    [ordered]@{
        version = 1
        saved_at = (Get-PcvApiTimestamp)
        jobs = @($script:PcvApiJobs.Values | ForEach-Object { Convert-PcvJobToApiData -Job $_ })
        queue = @(Get-PcvApiJobQueueIds)
    }
}

function Save-PcvApiJobStore {
    param([AllowNull()][string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return [ordered]@{ ok = $true; path = $null }
    }

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Get-PcvApiJobStoreSnapshot |
        ConvertTo-Json -Depth 50 |
        Set-Content -LiteralPath $Path -Encoding UTF8

    [ordered]@{ ok = $true; path = $Path }
}

function Initialize-PcvApiJobStore {
    param([AllowNull()][string]$Path)

    Clear-PcvApiJobStore

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return [ordered]@{ ok = $true; path = $null; loaded_jobs = 0; queued_jobs = 0 }
    }

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [ordered]@{ ok = $true; path = $Path; loaded_jobs = 0; queued_jobs = 0 }
    }

    try {
        $store = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -Depth 50
    }
    catch {
        $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddHHmmssfff')
        $quarantinePath = "$Path.corrupt.$stamp"
        Move-Item -LiteralPath $Path -Destination $quarantinePath -Force

        return [ordered]@{
            ok = $false
            path = $Path
            quarantine_path = $quarantinePath
            error = (New-PcvApiError `
                -Code 'PCV_JOB_STORE_CORRUPT' `
                -Message 'The persisted job store could not be parsed.' `
                -Detail $_.Exception.Message `
                -Retryable $false)
        }
    }

    $supportedVersion = 1
    $storeVersion = 1
    if ($store.PSObject.Properties.Name -contains 'version' -and $null -ne $store.version) {
        $storeVersion = [int]$store.version
    }

    if ($storeVersion -gt $supportedVersion) {
        $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddHHmmssfff')
        $quarantinePath = "$Path.unsupported.$storeVersion.$stamp"
        Move-Item -LiteralPath $Path -Destination $quarantinePath -Force

        return [ordered]@{
            ok = $false
            path = $Path
            quarantine_path = $quarantinePath
            error = (New-PcvApiError `
                -Code 'PCV_JOB_STORE_UNSUPPORTED_VERSION' `
                -Message 'The persisted job store version is not supported by this Local API runtime.' `
                -Detail "Persisted job store version $storeVersion is newer than supported version $supportedVersion." `
                -Retryable $false)
        }
    }

    $jobs = @()
    if ($store.PSObject.Properties.Name -contains 'jobs' -and $null -ne $store.jobs) {
        $jobs = @($store.jobs)
    }

    foreach ($job in $jobs) {
        if ($null -eq $job.job_id) {
            continue
        }

        $retryOf = $null
        if ($job.PSObject.Properties.Name -contains 'retry_of') {
            $retryOf = $job.retry_of
        }

        $attempt = 1
        if ($job.PSObject.Properties.Name -contains 'attempt' -and $null -ne $job.attempt) {
            $attempt = [int]$job.attempt
        }

        $canceledAt = $null
        if ($job.PSObject.Properties.Name -contains 'canceled_at') {
            $canceledAt = $job.canceled_at
        }

        $normalized = [ordered]@{
            job_id = [string]$job.job_id
            operation = [string]$job.operation
            status = [string]$job.status
            params = $job.params
            result = $job.result
            error = $job.error
            retry_of = $retryOf
            attempt = $attempt
            canceled_at = $canceledAt
            created_at = [string]$job.created_at
            updated_at = [string]$job.updated_at
        }

        if ($normalized.status -eq 'running') {
            $normalized.status = 'failed'
            $normalized.result = $null
            $normalized.error = New-PcvApiError `
                -Code 'PCV_JOB_INTERRUPTED' `
                -Message 'The job was interrupted before the API process stopped.' `
                -Detail 'A persisted running job cannot be resumed automatically after restart.' `
                -Retryable $true
            $normalized.updated_at = Get-PcvApiTimestamp
        }

        $script:PcvApiJobs[$normalized.job_id] = $normalized
    }

    $queue = @()
    if ($store.PSObject.Properties.Name -contains 'queue' -and $null -ne $store.queue) {
        $queue = @($store.queue)
    }

    foreach ($jobIdValue in $queue) {
        $jobId = [string]$jobIdValue
        if ($script:PcvApiJobs.ContainsKey($jobId) -and $script:PcvApiJobs[$jobId].status -eq 'queued') {
            $script:PcvApiJobQueue.Enqueue($jobId)
        }
    }

    [ordered]@{
        ok = $true
        path = $Path
        loaded_jobs = $script:PcvApiJobs.Count
        queued_jobs = $script:PcvApiJobQueue.Count
    }
}

function Set-PcvApiJobRunning {
    param([Parameter(Mandatory)]$Job)

    $Job.status = 'running'
    $Job.updated_at = Get-PcvApiTimestamp
    $Job
}

function Set-PcvApiJobCompleted {
    param(
        [Parameter(Mandatory)]$Job,
        [Parameter(Mandatory)]$HelperResult
    )

    if ($HelperResult.ok) {
        $Job.status = 'succeeded'
        $Job.result = $HelperResult
        $Job.error = $null
    }
    else {
        $Job.status = 'failed'
        $Job.result = $null
        $Job.error = $HelperResult.error
    }

    $Job.updated_at = Get-PcvApiTimestamp
    $Job
}

function Cancel-PcvApiJob {
    param([Parameter(Mandatory)]$Job)

    if ($Job.status -ne 'queued') {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 409 `
                -Operation 'job.cancel' `
                -Code 'PCV_JOB_NOT_CANCELABLE' `
                -Message "Job '$($Job.job_id)' cannot be canceled." `
                -Detail "Only queued jobs can be canceled. Current status is '$($Job.status)'." `
                -Retryable $false)
        }
    }

    $now = Get-PcvApiTimestamp
    [void](Remove-PcvApiJobFromQueue -JobId $Job.job_id)
    $Job.status = 'canceled'
    $Job.result = $null
    $Job.error = New-PcvApiError `
        -Code 'PCV_JOB_CANCELED' `
        -Message 'The job was canceled before it started.' `
        -Detail 'Queued jobs can be canceled before a worker begins processing.' `
        -Retryable $false
    $Job.canceled_at = $now
    $Job.updated_at = $now

    [ordered]@{
        ok = $true
        job = $Job
    }
}

function Retry-PcvApiJob {
    param(
        [Parameter(Mandatory)]$Job,
        [ValidateRange(1, 1000000)][int]$MaxAttempts = 3
    )

    if ($Job.status -ne 'failed') {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 409 `
                -Operation 'job.retry' `
                -Code 'PCV_JOB_NOT_RETRYABLE' `
                -Message "Job '$($Job.job_id)' cannot be retried." `
                -Detail "Only failed jobs can be retried. Current status is '$($Job.status)'." `
                -Retryable $false)
        }
    }

    $retryableFailure = $false
    if ($null -ne $Job.error) {
        if ($Job.error -is [System.Collections.IDictionary] -and $Job.error.Contains('retryable')) {
            $retryableFailure = [bool]$Job.error['retryable']
        }
        elseif ($Job.error.PSObject.Properties.Name -contains 'retryable') {
            $retryableFailure = [bool]$Job.error.retryable
        }
    }

    if (-not $retryableFailure) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 409 `
                -Operation 'job.retry' `
                -Code 'PCV_JOB_NOT_RETRYABLE' `
                -Message "Job '$($Job.job_id)' cannot be retried." `
                -Detail 'Only failed jobs with retryable errors can be retried manually.' `
                -Retryable $false)
        }
    }

    $nextAttempt = [int]$Job.attempt + 1
    if ($nextAttempt -gt $MaxAttempts) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 409 `
                -Operation 'job.retry' `
                -Code 'PCV_JOB_RETRY_LIMIT_REACHED' `
                -Message "Job '$($Job.job_id)' reached the manual retry attempt limit." `
                -Detail "Maximum attempts is $MaxAttempts. Current attempt is $($Job.attempt)." `
                -Retryable $false)
        }
    }

    $retryJob = New-PcvApiJob `
        -Operation $Job.operation `
        -Params $Job.params `
        -RetryOf $Job.job_id `
        -Attempt $nextAttempt
    Add-PcvApiJobToQueue -Job $retryJob

    [ordered]@{
        ok = $true
        job = $retryJob
    }
}

function Convert-PcvJobToApiData {
    param([Parameter(Mandatory)]$Job)

    [ordered]@{
        job_id = $Job.job_id
        operation = $Job.operation
        status = $Job.status
        params = $Job.params
        result = $Job.result
        error = $Job.error
        retry_of = $Job.retry_of
        attempt = $Job.attempt
        canceled_at = $Job.canceled_at
        created_at = $Job.created_at
        updated_at = $Job.updated_at
    }
}

function Invoke-PcvApiWorkerTick {
    param(
        [string]$HelperScriptPath = (Get-PcvDefaultHyperVHelperPath),
        [scriptblock]$InvokeHelper,
        [string]$JobStorePath,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30
    )

    while ($script:PcvApiJobQueue.Count -gt 0) {
        $jobId = $script:PcvApiJobQueue.Dequeue()
        $job = Get-PcvApiJob -JobId $jobId
        if ($null -eq $job -or $job.status -ne 'queued') {
            continue
        }

        [void](Set-PcvApiJobRunning -Job $job)
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        if ($null -eq $InvokeHelper) {
            $helperResult = Invoke-PcvHyperVHelper `
                -Operation $job.operation `
                -Params $job.params `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }
        else {
            $helperResult = & $InvokeHelper `
                -Operation $job.operation `
                -Params $job.params `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }

        [void](Set-PcvApiJobCompleted -Job $job -HelperResult $helperResult)
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return [ordered]@{
            processed = $true
            job = (Convert-PcvJobToApiData -Job $job)
        }
    }

    [ordered]@{
        processed = $false
        job = $null
    }
}

function Invoke-PcvApiWorkerPoolTick {
    param(
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [string]$HelperScriptPath = (Get-PcvDefaultHyperVHelperPath),
        [scriptblock]$InvokeHelper,
        [string]$JobStorePath,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30
    )

    $processedJobs = @()
    for ($workerIndex = 0; $workerIndex -lt $WorkerCount; $workerIndex++) {
        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath $HelperScriptPath `
            -InvokeHelper $InvokeHelper `
            -JobStorePath $JobStorePath `
            -TimeoutSec $TimeoutSec

        if (-not $tick.processed) {
            break
        }

        $processedJobs += $tick.job
    }

    [ordered]@{
        processed = ($processedJobs.Count -gt 0)
        processed_count = $processedJobs.Count
        jobs = @($processedJobs)
        remaining_queue = @(Get-PcvApiJobQueueIds)
    }
}

function ConvertFrom-PcvApiRequestJson {
    param(
        [AllowNull()][string]$Body,
        [Parameter(Mandatory)][string]$Operation
    )

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_REQUEST_BODY_MISSING' `
                -Message 'The request body is required.' `
                -Detail 'Pass a JSON object body for this endpoint.' `
                -Retryable $false)
        }
    }

    try {
        return [ordered]@{
            ok = $true
            value = ($Body | ConvertFrom-Json -Depth 30)
        }
    }
    catch {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_INVALID_JSON' `
                -Message 'The request body is not valid JSON.' `
                -Detail $_.Exception.Message `
                -Retryable $false)
        }
    }
}

function ConvertFrom-PcvApiRouteId {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$EncodedValue,
        [Parameter(Mandatory)][string]$Operation
    )

    if ($EncodedValue -match '%(?![0-9A-Fa-f]{2})') {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_ROUTE_ID_INVALID' `
                -Message 'The route id could not be decoded.' `
                -Detail 'The route id contains a malformed percent escape.' `
                -Retryable $false)
        }
    }

    try {
        $decoded = [System.Uri]::UnescapeDataString($EncodedValue)
    }
    catch {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_ROUTE_ID_INVALID' `
                -Message 'The route id could not be decoded.' `
                -Detail $_.Exception.Message `
                -Retryable $false)
        }
    }

    if ([string]::IsNullOrWhiteSpace($decoded)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_ROUTE_ID_INVALID' `
                -Message 'The route id is required.' `
                -Detail 'Pass a non-empty VM id or VM name in the route path.' `
                -Retryable $false)
        }
    }

    [ordered]@{
        ok = $true
        value = $decoded
    }
}

function Get-PcvApiObjectProperty {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory)][string[]]$Names
    )

    if ($null -eq $Value) {
        return $null
    }

    foreach ($name in $Names) {
        if ($Value -is [System.Collections.IDictionary] -and $Value.Contains($name)) {
            return $Value[$name]
        }

        if ($null -ne $Value.PSObject) {
            $property = $Value.PSObject.Properties[$name]
            if ($null -ne $property) {
                return $property.Value
            }
        }
    }

    $null
}

function ConvertTo-PcvVmInventoryList {
    param([AllowNull()]$Data)

    if ($null -eq $Data) {
        return @()
    }

    if ($Data -is [System.Array]) {
        return @($Data)
    }

    if ($Data -is [System.Collections.IEnumerable] -and -not ($Data -is [string]) -and -not ($Data -is [System.Collections.IDictionary])) {
        return @($Data)
    }

    $propertyNames = @()
    $dictionaryKeys = @()
    if ($null -ne $Data.PSObject) {
        $propertyNames = @($Data.PSObject.Properties.Name)
    }
    if ($Data -is [System.Collections.IDictionary]) {
        $dictionaryKeys = @($Data.Keys)
    }

    foreach ($propertyName in @('vms', 'items', 'data')) {
        if ($propertyNames -contains $propertyName -and $null -ne $Data.$propertyName) {
            return ConvertTo-PcvVmInventoryList -Data $Data.$propertyName
        }
        if ($dictionaryKeys -contains $propertyName -and $null -ne $Data[$propertyName]) {
            return ConvertTo-PcvVmInventoryList -Data $Data[$propertyName]
        }
    }

    @($Data)
}

function Find-PcvVmInInventoryData {
    param(
        [AllowNull()]$Data,
        [Parameter(Mandatory)][string]$VmId
    )

    $vms = ConvertTo-PcvVmInventoryList -Data $Data
    foreach ($vm in $vms) {
        if ($null -eq $vm) {
            continue
        }

        $candidateIds = @()
        if ($vm.PSObject.Properties.Name -contains 'id' -and $null -ne $vm.id) {
            $candidateIds += [string]$vm.id
        }
        if ($vm.PSObject.Properties.Name -contains 'name' -and $null -ne $vm.name) {
            $candidateIds += [string]$vm.name
        }
        if ($vm -is [System.Collections.IDictionary]) {
            if ($vm.Contains('id') -and $null -ne $vm['id']) {
                $candidateIds += [string]$vm['id']
            }
            if ($vm.Contains('name') -and $null -ne $vm['name']) {
                $candidateIds += [string]$vm['name']
            }
        }

        foreach ($candidateId in $candidateIds) {
            if ([string]::Equals($candidateId, $VmId, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $vm
            }
        }
    }

    $null
}

function New-PcvApiJobCreateResponse {
    param(
        [Parameter(Mandatory)]$Job,
        [int]$Status = 202
    )

    New-PcvApiResponse `
        -Status $Status `
        -Body (New-PcvApiBody `
            -Ok $true `
            -Operation 'job.create' `
            -Data (Convert-PcvJobToApiData -Job $Job) `
            -ErrorObject $null)
}

function Get-PcvApiPrefixInfo {
    param([Parameter(Mandatory)][string]$Prefix)

    try {
        $uri = [System.Uri]::new($Prefix, [System.UriKind]::Absolute)
    }
    catch {
        throw "PCV_PREFIX_INVALID|The API prefix is not a valid absolute URI.|$($_.Exception.Message)"
    }

    if ($uri.Scheme -ne 'http') {
        throw "PCV_PREFIX_INVALID|The API prefix must use http.|Use a local http://127.0.0.1:<port>/ prefix."
    }

    if (-not $Prefix.EndsWith('/')) {
        throw 'PCV_PREFIX_INVALID|The API prefix must end with a slash.|HttpListener prefixes require a trailing slash.'
    }

    $hostName = $uri.DnsSafeHost.ToLowerInvariant()
    $allowedHosts = @('127.0.0.1', 'localhost', '::1')
    $isLoopback = $hostName -in $allowedHosts

    [ordered]@{
        prefix = $Prefix
        host = $hostName
        port = $uri.Port
        exposure = $(if ($isLoopback) { 'loopback' } else { 'lan' })
        is_loopback = $isLoopback
    }
}

function Resolve-PcvApiToken {
    param(
        [AllowNull()][string]$ApiToken,
        [AllowNull()][string]$ApiTokenFile,
        [AllowNull()][string]$ApiTokenProtectedFile
    )

    $sources = @()
    if (-not [string]::IsNullOrWhiteSpace($ApiToken)) {
        $sources += 'inline'
    }
    if (-not [string]::IsNullOrWhiteSpace($ApiTokenFile)) {
        $sources += 'file'
    }
    if (-not [string]::IsNullOrWhiteSpace($ApiTokenProtectedFile)) {
        $sources += 'protected_file'
    }

    if ($sources.Count -gt 1) {
        throw 'PCV_API_TOKEN_CONFLICT|Specify only one API token source.|Use -ApiToken for short-lived tests, -ApiTokenFile for legacy packaging, or -ApiTokenProtectedFile for product service packaging.'
    }

    if ($sources.Count -eq 0) {
        return [ordered]@{
            value = $null
            source = 'none'
            storage = 'none'
            path = $null
        }
    }

    if ($sources[0] -eq 'inline') {
        return [ordered]@{
            value = $ApiToken
            source = 'inline'
            storage = 'inline'
            path = $null
        }
    }

    if ($sources[0] -eq 'protected_file') {
        Import-PcvApiServiceTokenSupport
        $protectedToken = Read-PcvDesktopServiceProtectedTokenFile -Path $ApiTokenProtectedFile
        return [ordered]@{
            value = $protectedToken.token
            source = 'protected_file'
            storage = $protectedToken.storage
            path = $ApiTokenProtectedFile
        }
    }

    if (-not (Test-Path -LiteralPath $ApiTokenFile -PathType Leaf)) {
        throw "PCV_API_TOKEN_FILE_NOT_FOUND|The API token file was not found.|Create the token file before starting the listener: '$ApiTokenFile'."
    }

    $token = (Get-Content -LiteralPath $ApiTokenFile -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "PCV_API_TOKEN_FILE_EMPTY|The API token file is empty.|Write a non-empty bearer token into '$ApiTokenFile'."
    }

    [ordered]@{
        value = $token
        source = 'file'
        storage = 'external_token_file'
        path = $ApiTokenFile
    }
}

function Import-PcvApiServiceTokenSupport {
    $desktopRoot = Split-Path -Parent $PSScriptRoot
    $modulePath = Join-Path (Join-Path $desktopRoot 'service') 'PcvDesktopService.psm1'
    if (-not (Test-Path -LiteralPath $modulePath -PathType Leaf)) {
        throw "PCV_API_PROTECTED_TOKEN_SUPPORT_MISSING|Protected API token support module was not found.|Expected service module at '$modulePath'."
    }

    Import-Module $modulePath -Force
}

function Assert-PcvApiPrefix {
    param(
        [Parameter(Mandatory)][string]$Prefix,
        [switch]$AllowLan,
        [AllowNull()][string]$ApiToken
    )

    $info = Get-PcvApiPrefixInfo -Prefix $Prefix
    $authRequired = -not [string]::IsNullOrWhiteSpace($ApiToken)

    if (-not $info.is_loopback -and -not $AllowLan) {
        throw "PCV_PREFIX_NOT_LOOPBACK|The API prefix must stay on loopback unless LAN mode is explicitly enabled.|Rejected host '$($info.host)'."
    }

    if (-not $info.is_loopback -and -not $authRequired) {
        throw 'PCV_LAN_TOKEN_REQUIRED|LAN API mode requires a bearer token.|Pass -ApiToken with a non-empty token when -AllowLan is used.'
    }

    [ordered]@{
        prefix = $info.prefix
        host = $info.host
        port = $info.port
        exposure = $info.exposure
        is_loopback = $info.is_loopback
        auth_required = $authRequired
    }
}

function Assert-PcvLoopbackPrefix {
    param([Parameter(Mandatory)][string]$Prefix)

    [void](Assert-PcvApiPrefix -Prefix $Prefix)
    $true
}

function Write-PcvApiEvent {
    param(
        [AllowNull()][string]$Path,
        [Parameter(Mandatory)][string]$EventName,
        [AllowNull()]$Data
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return [ordered]@{ ok = $true; path = $null }
    }

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $record = [ordered]@{
        timestamp = (Get-PcvApiTimestamp)
        event = $EventName
        data = $(if ($null -eq $Data) { [ordered]@{} } else { $Data })
    }

    $line = $record | ConvertTo-Json -Depth 30 -Compress
    Add-Content -LiteralPath $Path -Value $line -Encoding UTF8

    [ordered]@{ ok = $true; path = $Path }
}

function New-PcvFirewallRuleCommand {
    param(
        [Parameter(Mandatory)][string]$Prefix,
        [Parameter(Mandatory)][string]$RuleName,
        [ValidateSet('Add', 'Delete')][string]$Action = 'Add',
        [ValidateSet('private', 'domain', 'public', 'any')][string]$Profile = 'private'
    )

    $info = Get-PcvApiPrefixInfo -Prefix $Prefix
    if ($info.is_loopback) {
        throw 'PCV_FIREWALL_LAN_REQUIRED|Windows Firewall rule management is only valid for LAN API prefixes.|Use -AllowLan with a non-loopback prefix before configuring a firewall rule.'
    }

    $arguments = @(
        'advfirewall',
        'firewall',
        $Action.ToLowerInvariant(),
        'rule',
        "name=$RuleName"
    )

    if ($Action -eq 'Add') {
        $arguments += @(
            'dir=in',
            'action=allow',
            'protocol=TCP',
            "localport=$($info.port)",
            "profile=$Profile",
            'enable=yes'
        )
    }
    else {
        $arguments += @(
            'protocol=TCP',
            "localport=$($info.port)"
        )
    }

    [ordered]@{
        file_name = 'netsh.exe'
        arguments = $arguments
        action = $Action.ToLowerInvariant()
        rule_name = $RuleName
        port = $info.port
        profile = $Profile
    }
}

function Invoke-PcvNativeProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $FileName
    foreach ($argument in $Arguments) {
        [void]$process.StartInfo.ArgumentList.Add($argument)
    }
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true

    [void]$process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    [ordered]@{
        exit_code = $process.ExitCode
        stdout = $stdout
        stderr = $stderr
    }
}

function Invoke-PcvFirewallRuleEnsure {
    param(
        [Parameter(Mandatory)][string]$Prefix,
        [string]$RuleName = 'PureCVisor Desktop Node API',
        [ValidateSet('private', 'domain', 'public', 'any')][string]$Profile = 'private',
        [scriptblock]$InvokeProcess
    )

    $commands = @(
        (New-PcvFirewallRuleCommand -Prefix $Prefix -RuleName $RuleName -Action Delete -Profile $Profile),
        (New-PcvFirewallRuleCommand -Prefix $Prefix -RuleName $RuleName -Action Add -Profile $Profile)
    )

    $results = @()
    foreach ($command in $commands) {
        if ($null -eq $InvokeProcess) {
            $processResult = Invoke-PcvNativeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }
        else {
            $processResult = & $InvokeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }

        $results += [ordered]@{
            action = $command.action
            exit_code = [int]$processResult.exit_code
            stdout = [string]$processResult.stdout
            stderr = [string]$processResult.stderr
        }

        if ($command.action -eq 'add' -and [int]$processResult.exit_code -ne 0) {
            return [ordered]@{
                ok = $false
                operation = 'firewall.ensure'
                rule_name = $RuleName
                port = $command.port
                results = $results
            }
        }
    }

    [ordered]@{
        ok = $true
        operation = 'firewall.ensure'
        rule_name = $RuleName
        port = $commands[1].port
        results = $results
    }
}

function Invoke-PcvHyperVHelper {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [AllowNull()]$Params,
        [string]$HelperScriptPath = (Get-PcvDefaultHyperVHelperPath),
        [ValidateRange(1, 600)][int]$TimeoutSec = 30
    )

    if (-not (Test-Path -LiteralPath $HelperScriptPath -PathType Leaf)) {
        return New-PcvHelperFailure `
            -Operation $Operation `
            -Code 'PCV_HELPER_NOT_FOUND' `
            -Message 'The Hyper-V helper script was not found.' `
            -Detail "Expected helper script at '$HelperScriptPath'." `
            -Retryable $false
    }

    $payload = [ordered]@{
        operation = $Operation
        params = $(if ($null -eq $Params) { [ordered]@{} } else { $Params })
    } | ConvertTo-Json -Depth 30 -Compress

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = 'pwsh'
    [void]$process.StartInfo.ArgumentList.Add('-NoProfile')
    [void]$process.StartInfo.ArgumentList.Add('-ExecutionPolicy')
    [void]$process.StartInfo.ArgumentList.Add('Bypass')
    [void]$process.StartInfo.ArgumentList.Add('-File')
    [void]$process.StartInfo.ArgumentList.Add($HelperScriptPath)
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardInput = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true

    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.Write($payload)
        $process.StandardInput.Close()

        if (-not $process.WaitForExit($TimeoutSec * 1000)) {
            try {
                $process.Kill($true)
            }
            catch {
                $process.Kill()
            }

            return New-PcvHelperFailure `
                -Operation $Operation `
                -Code 'PCV_HELPER_TIMEOUT' `
                -Message 'The Hyper-V helper timed out.' `
                -Detail "The helper did not complete within $TimeoutSec seconds." `
                -Retryable $true
        }

        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()

        if ($process.ExitCode -ne 0) {
            $detail = "Helper exited with exit code $($process.ExitCode)."
            if (-not [string]::IsNullOrWhiteSpace($stderr)) {
                $detail = "$detail stderr: $($stderr.Trim())"
            }

            return New-PcvHelperFailure `
                -Operation $Operation `
                -Code 'PCV_HELPER_EXIT_FAILED' `
                -Message 'The Hyper-V helper process failed.' `
                -Detail $detail `
                -Retryable $true
        }

        try {
            return ($stdout | ConvertFrom-Json -Depth 30)
        }
        catch {
            return New-PcvHelperFailure `
                -Operation $Operation `
                -Code 'PCV_HELPER_INVALID_JSON' `
                -Message 'The Hyper-V helper returned invalid JSON.' `
                -Detail $_.Exception.Message `
                -Retryable $false
        }
    }
    catch {
        return New-PcvHelperFailure `
            -Operation $Operation `
            -Code 'PCV_HELPER_EXCEPTION' `
            -Message 'The API daemon failed to run the Hyper-V helper.' `
            -Detail $_.Exception.Message `
            -Retryable $true
    }
    finally {
        $process.Dispose()
    }
}

function Get-PcvApiStatusForHelperResult {
    param([Parameter(Mandatory)]$HelperResult)

    if ($HelperResult.ok) {
        return 200
    }

    $code = [string]$HelperResult.error.code
    if ($code -match 'TIMEOUT') { return 504 }
    if ($code -match 'NOT_FOUND') { return 404 }
    if ($code -match 'INVALID|MISSING|ALREADY_EXISTS') { return 400 }
    if ($code -match 'HOST_NOT_READY|NOT_ENABLED|NOT_RUNNING|DEFAULT_SWITCH|ADMIN_REQUIRED') { return 409 }
    return 502
}

function Convert-PcvHelperResultToApiResponse {
    param([Parameter(Mandatory)]$HelperResult)

    New-PcvApiResponse `
        -Status (Get-PcvApiStatusForHelperResult -HelperResult $HelperResult) `
        -Body $HelperResult
}

function Get-PcvStaticContentType {
    param([Parameter(Mandatory)][string]$Path)

    switch ([System.IO.Path]::GetExtension($Path).ToLowerInvariant()) {
        '.html' { 'text/html; charset=utf-8'; break }
        '.htm' { 'text/html; charset=utf-8'; break }
        '.css' { 'text/css; charset=utf-8'; break }
        '.js' { 'application/javascript; charset=utf-8'; break }
        '.json' { 'application/json; charset=utf-8'; break }
        '.svg' { 'image/svg+xml; charset=utf-8'; break }
        '.png' { 'image/png'; break }
        '.ico' { 'image/x-icon'; break }
        default { 'application/octet-stream'; break }
    }
}

function Test-PcvPathInsideRoot {
    param(
        [Parameter(Mandatory)][string]$RootPath,
        [Parameter(Mandatory)][string]$Path
    )

    $rootFull = [System.IO.Path]::GetFullPath($RootPath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $pathFull = [System.IO.Path]::GetFullPath($Path).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $comparison = [System.StringComparison]::OrdinalIgnoreCase

    $pathFull.Equals($rootFull, $comparison) -or
        $pathFull.StartsWith("$rootFull$([System.IO.Path]::DirectorySeparatorChar)", $comparison) -or
        $pathFull.StartsWith("$rootFull$([System.IO.Path]::AltDirectorySeparatorChar)", $comparison)
}

function New-PcvStaticFileForbiddenResponse {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Detail
    )

    New-PcvApiFailureResponse `
        -Status 403 `
        -Operation 'static.file' `
        -Code 'PCV_STATIC_PATH_FORBIDDEN' `
        -Message "Static file path '$Path' is not allowed." `
        -Detail $Detail `
        -Retryable $false
}

function New-PcvStaticFileNotFoundResponse {
    param([Parameter(Mandatory)][string]$Path)

    New-PcvApiFailureResponse `
        -Status 404 `
        -Operation 'static.file' `
        -Code 'PCV_STATIC_FILE_NOT_FOUND' `
        -Message "Static file '$Path' was not found." `
        -Detail 'The requested path does not resolve to a file under the configured WebRootPath.' `
        -Retryable $false
}

function Resolve-PcvStaticFilePath {
    param(
        [Parameter(Mandatory)][string]$WebRootPath,
        [Parameter(Mandatory)][string]$Path
    )

    if (-not (Test-Path -LiteralPath $WebRootPath -PathType Container)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvStaticFileNotFoundResponse -Path $Path)
        }
    }

    $rootFull = [System.IO.Path]::GetFullPath($WebRootPath)
    $decodedPath = [System.Uri]::UnescapeDataString($Path)
    $relativePath = 'index.html'

    if (-not [string]::IsNullOrWhiteSpace($decodedPath) -and $decodedPath -ne '/') {
        $trimmedPath = $decodedPath.TrimStart('/', '\')
        $segments = @($trimmedPath -split '[\\/]' | Where-Object { $_ -ne '' })
        if ($segments | Where-Object { $_ -eq '..' -or $_ -match ':' }) {
            return [ordered]@{
                ok = $false
                response = (New-PcvStaticFileForbiddenResponse `
                    -Path $Path `
                    -Detail 'Static file paths cannot contain parent-directory or drive-qualified segments.')
            }
        }

        if ($segments.Count -gt 0) {
            $relativePath = $segments -join [System.IO.Path]::DirectorySeparatorChar
        }
    }

    $targetPath = [System.IO.Path]::GetFullPath((Join-Path $rootFull $relativePath))
    if (-not (Test-PcvPathInsideRoot -RootPath $rootFull -Path $targetPath)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvStaticFileForbiddenResponse `
                -Path $Path `
                -Detail 'The requested path resolves outside the configured WebRootPath.')
        }
    }

    if (Test-Path -LiteralPath $targetPath -PathType Container) {
        $targetPath = [System.IO.Path]::GetFullPath((Join-Path $targetPath 'index.html'))
        if (-not (Test-PcvPathInsideRoot -RootPath $rootFull -Path $targetPath)) {
            return [ordered]@{
                ok = $false
                response = (New-PcvStaticFileForbiddenResponse `
                    -Path $Path `
                    -Detail 'The directory index resolves outside the configured WebRootPath.')
            }
        }
    }

    if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvStaticFileNotFoundResponse -Path $Path)
        }
    }

    [ordered]@{
        ok = $true
        path = $targetPath
    }
}

function New-PcvStaticFileResponse {
    param([Parameter(Mandatory)][string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    $textExtensions = @('.html', '.htm', '.css', '.js', '.json', '.svg', '.txt')
    $body = $null
    if ($extension -in $textExtensions) {
        $body = [System.Text.Encoding]::UTF8.GetString($bytes)
    }

    [ordered]@{
        status = 200
        headers = [ordered]@{
            'Content-Type' = (Get-PcvStaticContentType -Path $Path)
        }
        body = $body
        body_bytes = $bytes
    }
}

function Get-PcvRequestHeader {
    param(
        [AllowNull()]$Headers,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $Headers) {
        return $null
    }

    if ($Headers -is [System.Collections.Specialized.NameValueCollection]) {
        return $Headers[$Name]
    }

    if ($Headers -is [System.Collections.IDictionary]) {
        foreach ($key in $Headers.Keys) {
            if ([string]::Equals([string]$key, $Name, [System.StringComparison]::OrdinalIgnoreCase)) {
                return [string]$Headers[$key]
            }
        }
    }

    $property = $Headers.PSObject.Properties |
        Where-Object { [string]::Equals($_.Name, $Name, [System.StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -First 1
    if ($null -ne $property) {
        return [string]$property.Value
    }

    $null
}

function Test-PcvBearerToken {
    param(
        [AllowNull()]$Headers,
        [AllowNull()][string]$ApiToken
    )

    if ([string]::IsNullOrWhiteSpace($ApiToken)) {
        return [ordered]@{ ok = $true }
    }

    $authorization = Get-PcvRequestHeader -Headers $Headers -Name 'Authorization'
    if ([string]::IsNullOrWhiteSpace($authorization) -or $authorization -notmatch '^Bearer\s+(.+)$') {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 401 `
                -Operation 'api.auth' `
                -Code 'PCV_AUTH_REQUIRED' `
                -Message 'Authorization bearer token is required.' `
                -Detail 'Pass an Authorization: Bearer <token> header when ApiToken is configured.' `
                -Retryable $false)
        }
    }

    $providedToken = $Matches[1].Trim()
    if ($providedToken -ne $ApiToken) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 403 `
                -Operation 'api.auth' `
                -Code 'PCV_AUTH_FORBIDDEN' `
                -Message 'Authorization bearer token was rejected.' `
                -Detail 'The provided bearer token does not match the configured ApiToken.' `
                -Retryable $false)
        }
    }

    [ordered]@{ ok = $true }
}

function Test-PcvStaticAssetRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$WebRootPath
    )

    $normalizedMethod = $Method.ToUpperInvariant()
    $pathOnly = ($Path -split '\?', 2)[0].TrimEnd('/')
    if ([string]::IsNullOrWhiteSpace($pathOnly)) {
        $pathOnly = '/'
    }

    $isApiPath = $pathOnly.Equals('/api', [System.StringComparison]::OrdinalIgnoreCase) -or
        $pathOnly.StartsWith('/api/', [System.StringComparison]::OrdinalIgnoreCase)

    [ordered]@{
        ok = ($normalizedMethod -eq 'GET' -and -not $isApiPath -and -not [string]::IsNullOrWhiteSpace($WebRootPath))
        path = $pathOnly
    }
}

function Invoke-PcvApiRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Body,
        [string]$HelperScriptPath = (Get-PcvDefaultHyperVHelperPath),
        [scriptblock]$InvokeHelper,
        [string]$JobStorePath,
        [string]$WebRootPath,
        [AllowNull()]$Headers,
        [string]$ApiToken,
        [string]$ApiTokenStorage = 'external_token_file',
        [ValidateSet('loopback', 'lan')][string]$CurrentExposure = 'loopback',
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [switch]$AllowUnauthenticatedStatic
    )

    $normalizedMethod = $Method.ToUpperInvariant()
    $staticRequest = Test-PcvStaticAssetRequest `
        -Method $normalizedMethod `
        -Path $Path `
        -WebRootPath $WebRootPath

    if ($AllowUnauthenticatedStatic -and $staticRequest.ok) {
        $staticFile = Resolve-PcvStaticFilePath -WebRootPath $WebRootPath -Path $staticRequest.path
        if (-not $staticFile.ok) {
            return $staticFile.response
        }

        return New-PcvStaticFileResponse -Path $staticFile.path
    }

    $auth = Test-PcvBearerToken -Headers $Headers -ApiToken $ApiToken
    if (-not $auth.ok) {
        return $auth.response
    }

    $pathOnly = ($Path -split '\?', 2)[0].TrimEnd('/')
    if ([string]::IsNullOrWhiteSpace($pathOnly)) {
        $pathOnly = '/'
    }

    $isCheckpointDeleteRoute = $normalizedMethod -eq 'DELETE' -and
        $pathOnly -match '^/api/v1/vms/([^/]*)/checkpoints/([^/]*)$'

    if ($normalizedMethod -notin @('GET', 'POST') -and -not $isCheckpointDeleteRoute) {
        return New-PcvApiFailureResponse `
            -Status 405 `
            -Operation 'api.route' `
            -Code 'PCV_METHOD_NOT_ALLOWED' `
            -Message "HTTP method '$normalizedMethod' is not allowed for this Phase 4 endpoint." `
            -Detail 'Phase 4 exposes GET read/job endpoints, POST job create/control endpoints, and DELETE checkpoint delete jobs.' `
            -Retryable $false
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -eq '/api/v1/vms') {
        $parse = ConvertFrom-PcvApiRequestJson -Body $Body -Operation 'vm.create'
        if (-not $parse.ok) {
            return $parse.response
        }

        $job = New-PcvApiJob -Operation 'vm.create' -Params $parse.value
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/vms/([^/]*)/checkpoints$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'checkpoint.create'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $parse = ConvertFrom-PcvApiRequestJson -Body $Body -Operation 'checkpoint.create'
        if (-not $parse.ok) {
            return $parse.response
        }

        $checkpointName = Get-PcvApiObjectProperty -Value $parse.value -Names @('checkpoint_name', 'name')
        if ([string]::IsNullOrWhiteSpace([string]$checkpointName)) {
            return New-PcvApiFailureResponse `
                -Status 400 `
                -Operation 'checkpoint.create' `
                -Code 'PCV_CHECKPOINT_NAME_REQUIRED' `
                -Message 'Checkpoint name is required.' `
                -Detail 'Pass a JSON body with name or checkpoint_name.' `
                -Retryable $false
        }

        $job = New-PcvApiJob `
            -Operation 'checkpoint.create' `
            -Params ([ordered]@{
                vm_name = $routeId.value
                checkpoint_name = [string]$checkpointName
            })
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/vms/([^/]*)/checkpoints/([^/]*)/restore$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'checkpoint.restore'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $checkpointId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[2] -Operation 'checkpoint.restore'
        if (-not $checkpointId.ok) {
            return $checkpointId.response
        }

        $job = New-PcvApiJob `
            -Operation 'checkpoint.restore' `
            -Params ([ordered]@{
                vm_name = $routeId.value
                checkpoint_name = $checkpointId.value
            })
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }

    if ($isCheckpointDeleteRoute) {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'checkpoint.delete'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $checkpointId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[2] -Operation 'checkpoint.delete'
        if (-not $checkpointId.ok) {
            return $checkpointId.response
        }

        $job = New-PcvApiJob `
            -Operation 'checkpoint.delete' `
            -Params ([ordered]@{
                vm_name = $routeId.value
                checkpoint_name = $checkpointId.value
            })
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/vms/([^/]*)/(start|shutdown|poweroff|restart)$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'job.create'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $lifecycleOperation = switch ($Matches[2]) {
            'start' { 'vm.start'; break }
            'shutdown' { 'vm.shutdown'; break }
            'poweroff' { 'vm.poweroff'; break }
            'restart' { 'vm.restart'; break }
        }

        $job = New-PcvApiJob `
            -Operation $lifecycleOperation `
            -Params ([ordered]@{ name = $routeId.value })
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/jobs/([^/]+)/cancel$') {
        $jobId = $Matches[1]
        $job = Get-PcvApiJob -JobId $jobId
        if ($null -eq $job) {
            return New-PcvApiFailureResponse `
                -Status 404 `
                -Operation 'job.cancel' `
                -Code 'PCV_JOB_NOT_FOUND' `
                -Message "Job '$jobId' was not found." `
                -Detail 'The job was not found in the current memory store or loaded persisted store.' `
                -Retryable $false
        }

        $cancel = Cancel-PcvApiJob -Job $job
        if (-not $cancel.ok) {
            return $cancel.response
        }

        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiResponse `
            -Status 200 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'job.cancel' `
                -Data (Convert-PcvJobToApiData -Job $cancel.job) `
                -ErrorObject $null)
    }

    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/jobs/([^/]+)/retry$') {
        $jobId = $Matches[1]
        $job = Get-PcvApiJob -JobId $jobId
        if ($null -eq $job) {
            return New-PcvApiFailureResponse `
                -Status 404 `
                -Operation 'job.retry' `
                -Code 'PCV_JOB_NOT_FOUND' `
                -Message "Job '$jobId' was not found." `
                -Detail 'The job was not found in the current memory store or loaded persisted store.' `
                -Retryable $false
        }

        $retry = Retry-PcvApiJob -Job $job
        if (-not $retry.ok) {
            return $retry.response
        }

        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiResponse `
            -Status 202 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'job.retry' `
                -Data (Convert-PcvJobToApiData -Job $retry.job) `
                -ErrorObject $null)
    }

    if ($pathOnly -eq '/api/v1/runtime/policy' -and $normalizedMethod -ne 'GET') {
        return New-PcvApiFailureResponse `
            -Status 405 `
            -Operation 'runtime.policy' `
            -Code 'PCV_METHOD_NOT_ALLOWED' `
            -Message "HTTP method '$normalizedMethod' is not allowed for the runtime policy route." `
            -Detail 'Use GET /api/v1/runtime/policy to read the Local API runtime policy.' `
            -Retryable $false
    }

    if ($normalizedMethod -eq 'POST') {
        return New-PcvApiFailureResponse `
            -Status 404 `
            -Operation 'api.route' `
            -Code 'PCV_ROUTE_NOT_FOUND' `
            -Message "No Phase 4 POST route matches '$Path'." `
            -Detail 'Available POST routes: POST /api/v1/vms, POST /api/v1/vms/{id}/start, POST /api/v1/vms/{id}/shutdown, POST /api/v1/vms/{id}/poweroff, POST /api/v1/vms/{id}/restart, POST /api/v1/vms/{id}/checkpoints, POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore, POST /api/v1/jobs/{job_id}/cancel, POST /api/v1/jobs/{job_id}/retry.' `
            -Retryable $false
    }

    if ($pathOnly -match '^/api/v1/vms/([^/]*)/checkpoints$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'checkpoint.list'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $params = [ordered]@{ vm_name = $routeId.value }
        if ($null -eq $InvokeHelper) {
            $helperResult = Invoke-PcvHyperVHelper `
                -Operation 'checkpoint.list' `
                -Params $params `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }
        else {
            $helperResult = & $InvokeHelper `
                -Operation 'checkpoint.list' `
                -Params $params `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }

        return Convert-PcvHelperResultToApiResponse -HelperResult $helperResult
    }

    if ($pathOnly -match '^/api/v1/vms/([^/]+)$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'vm.get'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        if ($null -eq $InvokeHelper) {
            $helperResult = Invoke-PcvHyperVHelper `
                -Operation 'vm.list' `
                -Params ([ordered]@{}) `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }
        else {
            $helperResult = & $InvokeHelper `
                -Operation 'vm.list' `
                -Params ([ordered]@{}) `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }

        if (-not $helperResult.ok) {
            return Convert-PcvHelperResultToApiResponse -HelperResult $helperResult
        }

        $vm = Find-PcvVmInInventoryData -Data $helperResult.data -VmId $routeId.value
        if ($null -eq $vm) {
            return New-PcvApiFailureResponse `
                -Status 404 `
                -Operation 'vm.get' `
                -Code 'PCV_VM_NOT_FOUND' `
                -Message "VM '$($routeId.value)' was not found." `
                -Detail 'The VM was not present in the current Hyper-V inventory response.' `
                -Retryable $false
        }

        return New-PcvApiResponse `
            -Status 200 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'vm.get' `
                -Data $vm `
                -ErrorObject $null)
    }

    if ($pathOnly -match '^/api/v1/jobs/([^/]+)$') {
        $jobId = $Matches[1]
        $job = Get-PcvApiJob -JobId $jobId
        if ($null -eq $job) {
            return New-PcvApiFailureResponse `
                -Status 404 `
                -Operation 'job.get' `
                -Code 'PCV_JOB_NOT_FOUND' `
                -Message "Job '$jobId' was not found." `
                -Detail 'The job was not found in the current memory store or loaded persisted store.' `
                -Retryable $false
        }

        return New-PcvApiResponse `
            -Status 200 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'job.get' `
                -Data (Convert-PcvJobToApiData -Job $job) `
                -ErrorObject $null)
    }

    $operation = switch ($pathOnly) {
        '/api/v1/runtime/policy' { 'runtime.policy'; break }
        '/api/v1/host/status' { 'host.status'; break }
        '/api/v1/network/inventory' { 'network.inventory'; break }
        '/api/v1/vms' { 'vm.list'; break }
        default { $null }
    }

    if ($null -eq $operation) {
        $isApiPath = $pathOnly.Equals('/api', [System.StringComparison]::OrdinalIgnoreCase) -or
            $pathOnly.StartsWith('/api/', [System.StringComparison]::OrdinalIgnoreCase)
        if ($normalizedMethod -eq 'GET' -and -not $isApiPath -and -not [string]::IsNullOrWhiteSpace($WebRootPath)) {
            $staticFile = Resolve-PcvStaticFilePath -WebRootPath $WebRootPath -Path $pathOnly
            if (-not $staticFile.ok) {
                return $staticFile.response
            }

            return New-PcvStaticFileResponse -Path $staticFile.path
        }

        return New-PcvApiFailureResponse `
            -Status 404 `
            -Operation 'api.route' `
            -Code 'PCV_ROUTE_NOT_FOUND' `
            -Message "No Phase 4 route matches '$Path'." `
            -Detail 'Available routes: GET /api/v1/runtime/policy, GET /api/v1/host/status, GET /api/v1/network/inventory, GET /api/v1/vms, GET /api/v1/vms/{id}, GET /api/v1/vms/{id}/checkpoints, GET /api/v1/jobs/{job_id}, POST /api/v1/vms, POST /api/v1/vms/{id}/start, POST /api/v1/vms/{id}/shutdown, POST /api/v1/vms/{id}/poweroff, POST /api/v1/vms/{id}/restart, POST /api/v1/vms/{id}/checkpoints, POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore, DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}, POST /api/v1/jobs/{job_id}/cancel, POST /api/v1/jobs/{job_id}/retry.' `
            -Retryable $false
    }

    if ($operation -eq 'runtime.policy') {
        return New-PcvApiResponse `
            -Status 200 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'runtime.policy' `
                -Data (Get-PcvApiRuntimePolicy -TokenStorage $ApiTokenStorage -CurrentExposure $CurrentExposure) `
                -ErrorObject $null)
    }

    if ($null -eq $InvokeHelper) {
        $helperResult = Invoke-PcvHyperVHelper `
            -Operation $operation `
            -Params ([ordered]@{}) `
            -HelperScriptPath $HelperScriptPath `
            -TimeoutSec $TimeoutSec
    }
    else {
        $helperResult = & $InvokeHelper `
            -Operation $operation `
            -Params ([ordered]@{}) `
            -HelperScriptPath $HelperScriptPath `
            -TimeoutSec $TimeoutSec
    }

    Convert-PcvHelperResultToApiResponse -HelperResult $helperResult
}

function Start-PcvDesktopApi {
    param(
        [string]$Prefix = 'http://127.0.0.1:7777/',
        [string]$HelperScriptPath = (Get-PcvDefaultHyperVHelperPath),
        [string]$JobStorePath,
        [string]$WebRootPath,
        [string]$ApiToken,
        [string]$ApiTokenFile,
        [string]$ApiTokenProtectedFile,
        [switch]$AllowLan,
        [string]$EventLogPath,
        [switch]$EnsureFirewallRule,
        [string]$FirewallRuleName = 'PureCVisor Desktop Node API',
        [ValidateSet('private', 'domain', 'public', 'any')][string]$FirewallProfile = 'private',
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [switch]$Once
    )

    $tokenResolution = Resolve-PcvApiToken `
        -ApiToken $ApiToken `
        -ApiTokenFile $ApiTokenFile `
        -ApiTokenProtectedFile $ApiTokenProtectedFile

    $prefixPolicy = Assert-PcvApiPrefix `
        -Prefix $Prefix `
        -AllowLan:$AllowLan `
        -ApiToken $tokenResolution.value

    if (-not [string]::IsNullOrWhiteSpace($JobStorePath)) {
        [void](Initialize-PcvApiJobStore -Path $JobStorePath)
    }

    if ($EnsureFirewallRule) {
        $firewallResult = Invoke-PcvFirewallRuleEnsure `
            -Prefix $Prefix `
            -RuleName $FirewallRuleName `
            -Profile $FirewallProfile

        [void](Write-PcvApiEvent `
            -Path $EventLogPath `
            -EventName 'api.firewall.ensure' `
            -Data ([ordered]@{
                rule_name = $FirewallRuleName
                profile = $FirewallProfile
                ok = $firewallResult.ok
                results = $firewallResult.results
            }))

        if (-not $firewallResult.ok) {
            throw "PCV_FIREWALL_RULE_FAILED|Windows Firewall rule management failed.|Rule '$FirewallRuleName' could not be ensured for prefix '$Prefix'."
        }
    }

    $listener = [System.Net.HttpListener]::new()
    [void]$listener.Prefixes.Add($Prefix)
    $listenerStarted = $false
    $listener.Start()
    $listenerStarted = $true

    try {
        [void](Write-PcvApiEvent `
            -Path $EventLogPath `
            -EventName 'api.listener.start' `
            -Data ([ordered]@{
                prefix = $Prefix
                exposure = $prefixPolicy.exposure
                auth_required = $prefixPolicy.auth_required
                auth_source = $tokenResolution.source
                worker_count = $WorkerCount
                web_root_enabled = (-not [string]::IsNullOrWhiteSpace($WebRootPath))
                firewall_rule = $(if ($EnsureFirewallRule) { $FirewallRuleName } else { $null })
            }))

        Write-Host "PureCVisor Desktop Node API listening on $Prefix"
        while ($listener.IsListening) {
            $context = $listener.GetContext()
            $request = $context.Request
            $requestBody = $null

            if ($request.HasEntityBody) {
                $reader = [System.IO.StreamReader]::new($request.InputStream, $request.ContentEncoding)
                try {
                    $requestBody = $reader.ReadToEnd()
                }
                finally {
                    $reader.Dispose()
                }
            }

            $requestHeaders = @{}
            foreach ($headerName in $request.Headers.AllKeys) {
                $requestHeaders[$headerName] = $request.Headers[$headerName]
            }

            $apiResponse = Invoke-PcvApiRequest `
                -Method $request.HttpMethod `
                -Path $request.Url.PathAndQuery `
                -Body $requestBody `
                -HelperScriptPath $HelperScriptPath `
                -JobStorePath $JobStorePath `
                -WebRootPath $WebRootPath `
                -Headers $requestHeaders `
                -ApiToken $tokenResolution.value `
                -ApiTokenStorage $tokenResolution.storage `
                -CurrentExposure $prefixPolicy.exposure `
                -TimeoutSec $TimeoutSec `
                -AllowUnauthenticatedStatic:$prefixPolicy.is_loopback

            if ($apiResponse.Contains('body_bytes') -and $null -ne $apiResponse.body_bytes) {
                $responseBytes = [byte[]]$apiResponse.body_bytes
            }
            else {
                $responseBody = ''
                if ($null -ne $apiResponse.body) {
                    $responseBody = [string]$apiResponse.body
                }

                $responseBytes = [System.Text.Encoding]::UTF8.GetBytes($responseBody)
            }

            $context.Response.StatusCode = [int]$apiResponse.status
            $context.Response.ContentType = $apiResponse.headers['Content-Type']
            $context.Response.ContentLength64 = $responseBytes.Length
            $context.Response.OutputStream.Write($responseBytes, 0, $responseBytes.Length)
            $context.Response.OutputStream.Close()

            [void](Invoke-PcvApiWorkerPoolTick `
                -WorkerCount $WorkerCount `
                -HelperScriptPath $HelperScriptPath `
                -JobStorePath $JobStorePath `
                -TimeoutSec $TimeoutSec)

            if ($Once) {
                break
            }
        }
    }
    finally {
        if ($listenerStarted) {
            [void](Write-PcvApiEvent `
                -Path $EventLogPath `
                -EventName 'api.listener.stop' `
                -Data ([ordered]@{
                    prefix = $Prefix
                    exposure = $prefixPolicy.exposure
                }))
        }

        $listener.Stop()
        $listener.Close()
    }
}

Export-ModuleMember -Function `
    Add-PcvApiJobToQueue, `
    Assert-PcvApiPrefix, `
    Assert-PcvLoopbackPrefix, `
    Clear-PcvApiJobStore, `
    ConvertTo-PcvApiJson, `
    Convert-PcvJobToApiData, `
    Get-PcvDefaultHyperVHelperPath, `
    Get-PcvApiJob, `
    Get-PcvApiJobQueueIds, `
    Get-PcvApiJobStoreSnapshot, `
    Get-PcvApiPrefixInfo, `
    Get-PcvRequestHeader, `
    Get-PcvApiRuntimePolicy, `
    Get-PcvStaticContentType, `
    Import-PcvApiServiceTokenSupport, `
    Initialize-PcvApiJobStore, `
    Invoke-PcvFirewallRuleEnsure, `
    Invoke-PcvApiRequest, `
    Invoke-PcvHyperVHelper, `
    Invoke-PcvApiWorkerTick, `
    Invoke-PcvApiWorkerPoolTick, `
    New-PcvFirewallRuleCommand, `
    New-PcvApiJob, `
    New-PcvApiError, `
    New-PcvApiResponse, `
    New-PcvStaticFileResponse, `
    Resolve-PcvApiToken, `
    Resolve-PcvStaticFilePath, `
    Save-PcvApiJobStore, `
    Start-PcvDesktopApi, `
    Test-PcvBearerToken, `
    Test-PcvStaticAssetRequest, `
    Test-PcvPathInsideRoot, `
    Write-PcvApiEvent

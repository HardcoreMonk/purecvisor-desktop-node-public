[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("job-store-04265-reader-compatibility-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$FrozenHostPath = 'artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe',
    [string]$CurrentWriterProjectPath = 'packaging/windows-desktop-node/tools/fixtures/PcvJobStoreFixtureWriter/PcvJobStoreFixtureWriter.csproj',
    [ValidateRange(5, 120)]
    [int]$StartupTimeoutSeconds = 30,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$expectedFrozenHostSha256 = '95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83'
$expectedFrozenHostProductVersion = '0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876'
$startedAt = [DateTimeOffset]::UtcNow
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..')).Path

function Resolve-PcvCompatibilityPath {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$BasePath
    )

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Write-PcvCompatibilityJson {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        [IO.Directory]::CreateDirectory($parent) | Out-Null
    }

    $json = $Value | ConvertTo-Json -Depth 32
    [IO.File]::WriteAllText($Path, $json + "`n", [Text.UTF8Encoding]::new($false))
}

function Get-PcvFileSha256 {
    param([Parameter(Mandatory)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function New-PcvCurrentWriterFixture {
    param(
        [Parameter(Mandatory)][ValidateSet(1, 2)][int]$SchemaVersion,
        [Parameter(Mandatory)][string]$ScenarioRoot,
        [Parameter(Mandatory)][string]$WriterProjectPath
    )

    $outputs = [ordered]@{}
    foreach ($mode in @('terminal', 'queued')) {
        $modeRoot = Join-Path $ScenarioRoot ("current-writer-" + $mode)
        [IO.Directory]::CreateDirectory($modeRoot) | Out-Null
        $storePath = Join-Path $modeRoot 'jobs.json'
        $logPath = Join-Path $modeRoot 'writer.log'
        $writerOutput = @(& dotnet run `
                -c Release `
                --project $WriterProjectPath `
                --no-launch-profile `
                -- $storePath $SchemaVersion $mode 2>&1)
        $writerExitCode = $LASTEXITCODE
        [IO.File]::WriteAllText(
            $logPath,
            (($writerOutput | ForEach-Object { [string]$_ }) -join [Environment]::NewLine),
            [Text.UTF8Encoding]::new($false))
        if ($writerExitCode -ne 0 -or -not (Test-Path -LiteralPath $storePath -PathType Leaf)) {
            throw "PCV_04265_READER_CURRENT_WRITER_FAILED|Schema v$SchemaVersion mode $mode writer exit code $writerExitCode."
        }

        $snapshot = Get-Content -Raw -LiteralPath $storePath | ConvertFrom-Json -Depth 32
        if ([int]$snapshot.version -ne $SchemaVersion) {
            throw "PCV_04265_READER_CURRENT_WRITER_SCHEMA_MISMATCH|Schema v$SchemaVersion mode $mode output changed the schema version."
        }
        $outputs[$mode] = [pscustomobject][ordered]@{
            path = $storePath
            log_path = $logPath
            sha256 = Get-PcvFileSha256 -Path $storePath
            snapshot = $snapshot
        }
    }

    $terminalJobs = @($outputs.terminal.snapshot.jobs | Sort-Object -Property @(
            @{ Expression = { [DateTimeOffset]$_.updated_at }; Descending = $true },
            @{ Expression = { [DateTimeOffset]$_.created_at }; Descending = $true }
        ))
    $queuedJobs = @($outputs.queued.snapshot.jobs)
    $expectedQueue = @($outputs.queued.snapshot.queue | ForEach-Object { [string]$_ })
    if ($terminalJobs.Count -ne 3 -or
        @($outputs.terminal.snapshot.queue).Count -ne 0 -or
        $queuedJobs.Count -ne 2 -or
        $expectedQueue.Count -ne 2) {
        throw "PCV_04265_READER_CURRENT_WRITER_SHAPE_INVALID|Schema v$SchemaVersion current writer fixture counts are invalid."
    }

    return [pscustomobject][ordered]@{
        schema_version = $SchemaVersion
        generated_by_current_writer = $true
        terminal = $outputs.terminal
        queued = $outputs.queued
        expected_jobs = $terminalJobs
        expected_queue_jobs = $queuedJobs
        expected_queue = $expectedQueue
    }
}

function Get-PcvFreeHighLoopbackPort {
    $firstCandidate = [Security.Cryptography.RandomNumberGenerator]::GetInt32(49152, 65536)
    foreach ($offset in 0..511) {
        $port = 49152 + (($firstCandidate - 49152 + $offset) % 16384)
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $port)
        try {
            $listener.Start()
            return $port
        }
        catch [Net.Sockets.SocketException] {
            continue
        }
        finally {
            $listener.Stop()
        }
    }

    throw 'PCV_04265_READER_HIGH_PORT_UNAVAILABLE|Could not allocate a free loopback port at or above 49152.'
}

function ConvertTo-PcvResponseText {
    param([AllowNull()]$Content)

    if ($null -eq $Content) {
        return ''
    }
    if ($Content -is [byte[]]) {
        return [Text.Encoding]::UTF8.GetString($Content)
    }
    return [string]$Content
}

function Assert-PcvNullableEqual {
    param(
        [AllowNull()]$Actual,
        [AllowNull()]$Expected,
        [Parameter(Mandatory)][string]$Field
    )

    if ($null -eq $Expected) {
        if ($null -ne $Actual) {
            throw "PCV_04265_READER_PROJECTION_MISMATCH|Expected null for $Field."
        }
        return
    }

    if ([string]$Actual -ne [string]$Expected) {
        throw "PCV_04265_READER_PROJECTION_MISMATCH|$Field expected '$Expected' but observed '$Actual'."
    }
}

function Assert-PcvTimestampEqual {
    param(
        [AllowNull()]$Actual,
        [AllowNull()]$Expected,
        [Parameter(Mandatory)][string]$Field
    )

    if ($null -eq $Expected) {
        Assert-PcvNullableEqual -Actual $Actual -Expected $null -Field $Field
        return
    }
    if ($null -eq $Actual) {
        throw "PCV_04265_READER_PROJECTION_MISMATCH|$Field expected '$Expected' but observed null."
    }

    try {
        $actualTimestamp = [DateTimeOffset]$Actual
        $expectedTimestamp = if ($Expected -is [DateTimeOffset]) {
            [DateTimeOffset]$Expected
        }
        elseif ($Expected -is [DateTime]) {
            [DateTimeOffset]([DateTime]$Expected)
        }
        else {
            [DateTimeOffset]::Parse(
                [string]$Expected,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::RoundtripKind)
        }
    }
    catch {
        throw "PCV_04265_READER_PROJECTION_MISMATCH|$Field did not contain a comparable timestamp."
    }

    if ($actualTimestamp.UtcTicks -ne $expectedTimestamp.UtcTicks) {
        throw "PCV_04265_READER_PROJECTION_MISMATCH|$Field expected '$Expected' but observed '$Actual'."
    }
}

function Assert-PcvJobProjection {
    param(
        [Parameter(Mandatory)]$Response,
        [Parameter(Mandatory)][object[]]$ExpectedJobs,
        [Parameter(Mandatory)][int]$SchemaVersion
    )

    if (-not [bool]$Response.ok -or [string]$Response.operation -ne 'job.list') {
        throw "PCV_04265_READER_RESPONSE_INVALID|Schema v$SchemaVersion did not return a successful job.list envelope."
    }

    $actualJobs = @($Response.data.jobs)
    if ([int]$Response.data.count -ne $ExpectedJobs.Count -or
        [int]$Response.data.returned -ne $ExpectedJobs.Count -or
        $actualJobs.Count -ne $ExpectedJobs.Count) {
        throw "PCV_04265_READER_JOB_COUNT_MISMATCH|Schema v$SchemaVersion expected $($ExpectedJobs.Count) projected jobs."
    }

    for ($index = 0; $index -lt $ExpectedJobs.Count; $index++) {
        $actual = $actualJobs[$index]
        $expected = $ExpectedJobs[$index]
        foreach ($field in @(
                'job_id',
                'operation',
                'status',
                'attempt',
                'correlation_id',
                'request_id',
                'retry_of'
            )) {
            Assert-PcvNullableEqual -Actual $actual.$field -Expected $expected.$field -Field "jobs[$index].$field"
        }
        Assert-PcvTimestampEqual -Actual $actual.created_at -Expected $expected.created_at -Field "jobs[$index].created_at"
        Assert-PcvTimestampEqual -Actual $actual.updated_at -Expected $expected.updated_at -Field "jobs[$index].updated_at"
        Assert-PcvTimestampEqual -Actual $actual.canceled_at -Expected $expected.canceled_at -Field "jobs[$index].canceled_at"

        Assert-PcvNullableEqual -Actual $actual.params.vm_id -Expected $expected.params.vm_id -Field "jobs[$index].params.vm_id"
        Assert-PcvNullableEqual -Actual $actual.params.desired_state -Expected $expected.params.desired_state -Field "jobs[$index].params.desired_state"

        if ($expected.status -eq 'succeeded') {
            Assert-PcvNullableEqual -Actual $actual.result.ok -Expected $expected.result.ok -Field "jobs[$index].result.ok"
            Assert-PcvNullableEqual -Actual $actual.result.operation -Expected $expected.result.operation -Field "jobs[$index].result.operation"
            Assert-PcvNullableEqual -Actual $actual.result.data.action -Expected $expected.result.data.action -Field "jobs[$index].result.data.action"
            Assert-PcvNullableEqual -Actual $actual.result.data.fixture -Expected $expected.result.data.fixture -Field "jobs[$index].result.data.fixture"
            Assert-PcvNullableEqual -Actual $actual.result.error -Expected $null -Field "jobs[$index].result.error"
            if ($null -ne $actual.error) {
                throw "PCV_04265_READER_PROJECTION_MISMATCH|jobs[$index].error must be null for succeeded."
            }
        }
        else {
            Assert-PcvNullableEqual -Actual $actual.error.code -Expected $expected.error.code -Field "jobs[$index].error.code"
            Assert-PcvNullableEqual -Actual $actual.error.message -Expected $expected.error.message -Field "jobs[$index].error.message"
            Assert-PcvNullableEqual -Actual $actual.error.detail -Expected $expected.error.detail -Field "jobs[$index].error.detail"
            Assert-PcvNullableEqual -Actual $actual.error.retryable -Expected $expected.error.retryable -Field "jobs[$index].error.retryable"
            if ($null -ne $actual.result) {
                throw "PCV_04265_READER_PROJECTION_MISMATCH|jobs[$index].result must be null for $($expected.status)."
            }
        }
    }

    return [pscustomobject][ordered]@{
        ok = $true
        schema_version = $SchemaVersion
        job_count = $actualJobs.Count
        job_ids = @($actualJobs | ForEach-Object { [string]$_.job_id })
        statuses = @($actualJobs | ForEach-Object { [string]$_.status })
    }
}

function Assert-PcvQueueProjection {
    param(
        [Parameter(Mandatory)]$Response,
        [Parameter(Mandatory)][string[]]$ExpectedQueue,
        [Parameter(Mandatory)][int]$SchemaVersion
    )

    if (-not [bool]$Response.ok -or [string]$Response.operation -ne 'job.list') {
        throw "PCV_04265_READER_QUEUE_RESPONSE_INVALID|Schema v$SchemaVersion did not return a successful queue probe job.list envelope."
    }
    if ($ExpectedQueue.Count -ne 2) {
        throw "PCV_04265_READER_QUEUE_EXPECTATION_INVALID|Schema v$SchemaVersion queue probe requires two FIFO entries."
    }

    $actualJobs = @($Response.data.jobs)
    $first = @($actualJobs | Where-Object { [string]$_.job_id -eq $ExpectedQueue[0] })
    $second = @($actualJobs | Where-Object { [string]$_.job_id -eq $ExpectedQueue[1] })
    if ($actualJobs.Count -ne 2 -or $first.Count -ne 1 -or $second.Count -ne 1) {
        throw "PCV_04265_READER_QUEUE_JOB_MISMATCH|Schema v$SchemaVersion did not project both current-writer queued jobs."
    }
    if ([string]$first[0].status -ne 'failed' -or
        [string]$first[0].error.code -ne 'PCV_JOB_STORE_SAVE_FAILED') {
        throw "PCV_04265_READER_QUEUE_FIFO_MISMATCH|Schema v$SchemaVersion did not attempt the first FIFO job before the read-only store write guard blocked dispatch."
    }

    $secondStatus = [string]$second[0].status
    $selectionMode = 'first-failed-second-queued'
    if ($secondStatus -eq 'failed') {
        if ([string]$second[0].error.code -ne 'PCV_JOB_STORE_SAVE_FAILED') {
            throw "PCV_04265_READER_QUEUE_FIFO_MISMATCH|Schema v$SchemaVersion second FIFO job failed for an unexpected reason."
        }

        $firstUpdatedAt = [DateTimeOffset]$first[0].updated_at
        $secondUpdatedAt = [DateTimeOffset]$second[0].updated_at
        if ($firstUpdatedAt.UtcTicks -ge $secondUpdatedAt.UtcTicks) {
            throw "PCV_04265_READER_QUEUE_FIFO_MISMATCH|Schema v$SchemaVersion did not preserve first-before-second FIFO attempt timestamps."
        }
        $selectionMode = 'both-failed-in-fifo-timestamp-order'
    }
    elseif ($secondStatus -ne 'queued') {
        throw "PCV_04265_READER_QUEUE_FIFO_MISMATCH|Schema v$SchemaVersion second FIFO job had unexpected status '$secondStatus'."
    }

    return [pscustomobject][ordered]@{
        ok = $true
        schema_version = $SchemaVersion
        job_count = $actualJobs.Count
        expected_queue = $ExpectedQueue
        selected_job_id = [string]$first[0].job_id
        selected_job_status = [string]$first[0].status
        selected_job_error_code = [string]$first[0].error.code
        remaining_job_id = [string]$second[0].job_id
        remaining_job_status = $secondStatus
        selection_mode = $selectionMode
        attempted_job_ids = if ($secondStatus -eq 'failed') { @([string]$first[0].job_id, [string]$second[0].job_id) } else { @([string]$first[0].job_id) }
        fifo_selection_observed = $true
        provider_dispatch_prevented_by_store_guard = $true
    }
}

function Invoke-PcvFrozenReaderPass {
    param(
        [Parameter(Mandatory)][string]$HostPath,
        [Parameter(Mandatory)][string]$ScenarioRoot,
        [Parameter(Mandatory)][string]$JobStorePath,
        [Parameter(Mandatory)][object[]]$ExpectedJobs,
        [string[]]$ExpectedQueue = @(),
        [Parameter(Mandatory)][int]$SchemaVersion,
        [Parameter(Mandatory)][string]$PassName,
        [Parameter(Mandatory)][int]$TimeoutSeconds,
        [switch]$QueueProbe
    )

    $beforeHash = Get-PcvFileSha256 -Path $JobStorePath
    $port = Get-PcvFreeHighLoopbackPort
    $baseUri = "http://127.0.0.1:$port/"
    $requestUri = $baseUri + 'api/v1/jobs'
    $stdoutPath = Join-Path $ScenarioRoot "$PassName.stdout.log"
    $stderrPath = Join-Path $ScenarioRoot "$PassName.stderr.log"
    $responsePath = Join-Path $ScenarioRoot "$PassName.response.json"
    $eventLogPath = Join-Path $ScenarioRoot "$PassName.events.jsonl"
    $diagnosticsRoot = Join-Path $ScenarioRoot "$PassName-diagnostics"

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $HostPath
    $startInfo.WorkingDirectory = $ScenarioRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'listen',
            '--prefix', $baseUri,
            '--job-store', $JobStorePath,
            '--event-log', $eventLogPath,
            '--diagnostics-root', $diagnosticsRoot
        )) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $response = $null
    $lastRequestError = $null
    $terminatedByRunner = $false
    $processStarted = $false
    $passStarted = [DateTimeOffset]::UtcNow
    $stdoutTask = $null
    $stderrTask = $null
    $storeReadLock = $null
    try {
        if ($QueueProbe) {
            $storeReadLock = [IO.FileStream]::new(
                $JobStorePath,
                [IO.FileMode]::Open,
                [IO.FileAccess]::Read,
                [IO.FileShare]::Read)
        }
        $processStarted = $process.Start()
        if (-not $processStarted) {
            throw 'PCV_04265_READER_PROCESS_START_FAILED|Frozen host process did not start.'
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()

        $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
        while ([DateTimeOffset]::UtcNow -lt $deadline) {
            if ($process.HasExited) {
                break
            }

            try {
                $candidate = Invoke-WebRequest `
                    -Uri $requestUri `
                    -Method Get `
                    -TimeoutSec 2 `
                    -SkipHttpErrorCheck `
                    -ErrorAction Stop
                if ([int]$candidate.StatusCode -gt 0) {
                    if (-not $QueueProbe) {
                        $response = $candidate
                        break
                    }

                    $candidateBody = ConvertTo-PcvResponseText -Content $candidate.Content
                    $candidateJson = $candidateBody | ConvertFrom-Json -Depth 32
                    $candidateJobs = @($candidateJson.data.jobs)
                    $firstCandidate = @($candidateJobs | Where-Object { [string]$_.job_id -eq $ExpectedQueue[0] })
                    $secondCandidate = @($candidateJobs | Where-Object { [string]$_.job_id -eq $ExpectedQueue[1] })
                    if ($firstCandidate.Count -eq 1 -and
                        $secondCandidate.Count -eq 1 -and
                        [string]$firstCandidate[0].status -eq 'failed' -and
                        [string]$firstCandidate[0].error.code -eq 'PCV_JOB_STORE_SAVE_FAILED') {
                        $secondStatus = [string]$secondCandidate[0].status
                        if ($secondStatus -eq 'queued') {
                            $response = $candidate
                            break
                        }
                        if ($secondStatus -eq 'failed' -and
                            [string]$secondCandidate[0].error.code -eq 'PCV_JOB_STORE_SAVE_FAILED' -and
                            ([DateTimeOffset]$firstCandidate[0].updated_at).UtcTicks -lt
                                ([DateTimeOffset]$secondCandidate[0].updated_at).UtcTicks) {
                            $response = $candidate
                            break
                        }
                    }
                }
            }
            catch {
                $lastRequestError = $_.Exception.Message
            }

            Start-Sleep -Milliseconds 100
        }
    }
    finally {
        if ($processStarted) {
            if (-not $process.HasExited) {
                $terminatedByRunner = $true
                $process.Kill($true)
            }
            if (-not $process.WaitForExit(10000)) {
                throw 'PCV_04265_READER_PROCESS_CLEANUP_FAILED|Frozen host did not exit within 10 seconds.'
            }
        }
        if ($null -ne $storeReadLock) {
            $storeReadLock.Dispose()
        }
    }

    $stdout = if ($null -ne $stdoutTask) { $stdoutTask.GetAwaiter().GetResult() } else { '' }
    $stderr = if ($null -ne $stderrTask) { $stderrTask.GetAwaiter().GetResult() } else { '' }
    [IO.File]::WriteAllText($stdoutPath, $stdout, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($stderrPath, $stderr, [Text.UTF8Encoding]::new($false))

    if ($null -eq $response) {
        $detail = if (-not [string]::IsNullOrWhiteSpace($stderr)) {
            $stderr.Trim()
        }
        elseif (-not [string]::IsNullOrWhiteSpace($lastRequestError)) {
            $lastRequestError
        }
        else {
            "Frozen host exit code $($process.ExitCode)."
        }
        throw "PCV_04265_READER_LISTENER_UNAVAILABLE|$detail"
    }
    if ([int]$response.StatusCode -ne 200) {
        throw "PCV_04265_READER_HTTP_FAILED|Expected HTTP 200 but observed $([int]$response.StatusCode)."
    }

    $bodyText = ConvertTo-PcvResponseText -Content $response.Content
    [IO.File]::WriteAllText($responsePath, $bodyText, [Text.UTF8Encoding]::new($false))
    $afterHash = Get-PcvFileSha256 -Path $JobStorePath
    if ($afterHash -ne $beforeHash) {
        throw "PCV_04265_READER_STORE_MUTATED|Schema v$SchemaVersion $PassName changed jobs.json."
    }
    $responseJson = $bodyText | ConvertFrom-Json -Depth 32
    $projection = if ($QueueProbe) {
        Assert-PcvQueueProjection `
            -Response $responseJson `
            -ExpectedQueue $ExpectedQueue `
            -SchemaVersion $SchemaVersion
    }
    else {
        Assert-PcvJobProjection `
            -Response $responseJson `
            -ExpectedJobs $ExpectedJobs `
            -SchemaVersion $SchemaVersion
    }

    return [pscustomobject][ordered]@{
        name = $PassName
        ok = $true
        schema_version = $SchemaVersion
        http_status = [int]$response.StatusCode
        content_type = [string]$response.Headers['Content-Type']
        listener_host = '127.0.0.1'
        listener_port = $port
        request_path = '/api/v1/jobs'
        pass_kind = if ($QueueProbe) { 'queue-fifo-readonly-probe' } else { 'terminal-reader' }
        job_store_sha256_before = $beforeHash
        job_store_sha256_after = $afterHash
        job_store_hash_unchanged = ($beforeHash -eq $afterHash)
        projection = $projection
        native_operation_requests = 0
        hyperv_routes_invoked = $false
        queue_probe = [bool]$QueueProbe
        process_terminated_by_runner = $terminatedByRunner
        process_exit_code = $process.ExitCode
        duration_ms = [int]([DateTimeOffset]::UtcNow - $passStarted).TotalMilliseconds
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        response_path = $responsePath
    }
}

function Invoke-PcvSchemaScenario {
    param(
        [Parameter(Mandatory)][string]$HostPath,
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)]$Fixture,
        [Parameter(Mandatory)][int]$TimeoutSeconds
    )

    $schemaVersion = [int]$Fixture.schema_version
    $scenarioRoot = Join-Path $Root "schema-v$schemaVersion"
    [IO.Directory]::CreateDirectory($scenarioRoot) | Out-Null
    $terminalStorePath = [string]$Fixture.terminal.path
    $queuedStorePath = [string]$Fixture.queued.path
    $terminalBackupPath = Join-Path $scenarioRoot 'terminal.jobs.json.frozen-input.backup'
    $queuedBackupPath = Join-Path $scenarioRoot 'queued.jobs.json.frozen-input.backup'
    [IO.File]::Copy($terminalStorePath, $terminalBackupPath, $false)
    [IO.File]::Copy($queuedStorePath, $queuedBackupPath, $false)

    $terminalSourceHash = Get-PcvFileSha256 -Path $terminalStorePath
    $terminalBackupHash = Get-PcvFileSha256 -Path $terminalBackupPath
    $queuedSourceHash = Get-PcvFileSha256 -Path $queuedStorePath
    $queuedBackupHash = Get-PcvFileSha256 -Path $queuedBackupPath
    if ($terminalSourceHash -ne $terminalBackupHash -or $queuedSourceHash -ne $queuedBackupHash) {
        throw "PCV_04265_READER_BACKUP_MISMATCH|Schema v$schemaVersion backup hash differs from current-writer source."
    }

    $firstPass = Invoke-PcvFrozenReaderPass `
        -HostPath $HostPath `
        -ScenarioRoot $scenarioRoot `
        -JobStorePath $terminalStorePath `
        -ExpectedJobs $Fixture.expected_jobs `
        -SchemaVersion $schemaVersion `
        -PassName 'terminal-initial-read' `
        -TimeoutSeconds $TimeoutSeconds

    [IO.File]::Copy($terminalBackupPath, $terminalStorePath, $true)
    $terminalRestoredHash = Get-PcvFileSha256 -Path $terminalStorePath
    if ($terminalRestoredHash -ne $terminalBackupHash) {
        throw "PCV_04265_READER_RESTORE_MISMATCH|Schema v$schemaVersion terminal backup restore did not reproduce current-writer output."
    }

    $secondPass = Invoke-PcvFrozenReaderPass `
        -HostPath $HostPath `
        -ScenarioRoot $scenarioRoot `
        -JobStorePath $terminalStorePath `
        -ExpectedJobs $Fixture.expected_jobs `
        -SchemaVersion $schemaVersion `
        -PassName 'terminal-restored-read' `
        -TimeoutSeconds $TimeoutSeconds
    $terminalFinalHash = Get-PcvFileSha256 -Path $terminalStorePath

    $queueFirstPass = Invoke-PcvFrozenReaderPass `
        -HostPath $HostPath `
        -ScenarioRoot $scenarioRoot `
        -JobStorePath $queuedStorePath `
        -ExpectedJobs $Fixture.expected_queue_jobs `
        -ExpectedQueue $Fixture.expected_queue `
        -SchemaVersion $schemaVersion `
        -PassName 'queue-initial-read' `
        -TimeoutSeconds $TimeoutSeconds `
        -QueueProbe

    [IO.File]::Copy($queuedBackupPath, $queuedStorePath, $true)
    $queuedRestoredHash = Get-PcvFileSha256 -Path $queuedStorePath
    if ($queuedRestoredHash -ne $queuedBackupHash) {
        throw "PCV_04265_READER_RESTORE_MISMATCH|Schema v$schemaVersion queue backup restore did not reproduce current-writer output."
    }

    $queueSecondPass = Invoke-PcvFrozenReaderPass `
        -HostPath $HostPath `
        -ScenarioRoot $scenarioRoot `
        -JobStorePath $queuedStorePath `
        -ExpectedJobs $Fixture.expected_queue_jobs `
        -ExpectedQueue $Fixture.expected_queue `
        -SchemaVersion $schemaVersion `
        -PassName 'queue-restored-read' `
        -TimeoutSeconds $TimeoutSeconds `
        -QueueProbe
    $queuedFinalHash = Get-PcvFileSha256 -Path $queuedStorePath
    if ($terminalFinalHash -ne $terminalSourceHash -or $queuedFinalHash -ne $queuedSourceHash) {
        throw "PCV_04265_READER_FINAL_HASH_MISMATCH|Schema v$schemaVersion final jobs.json differs from current-writer output."
    }

    $passes = @($firstPass, $secondPass, $queueFirstPass, $queueSecondPass)

    return [pscustomobject][ordered]@{
        schema_version = $schemaVersion
        ok = $true
        generated_by_current_writer = [bool]$Fixture.generated_by_current_writer
        terminal_only = $false
        queue_count = @($Fixture.expected_queue).Count
        job_count = @($Fixture.expected_jobs).Count
        expected_job_ids = @($Fixture.expected_jobs | ForEach-Object { [string]$_.job_id })
        expected_queue = @($Fixture.expected_queue)
        terminal_job_store_path = $terminalStorePath
        queue_job_store_path = $queuedStorePath
        terminal_backup_path = $terminalBackupPath
        queue_backup_path = $queuedBackupPath
        terminal_source_sha256 = $terminalSourceHash
        terminal_backup_sha256 = $terminalBackupHash
        terminal_restored_sha256 = $terminalRestoredHash
        terminal_final_sha256 = $terminalFinalHash
        queue_source_sha256 = $queuedSourceHash
        queue_backup_sha256 = $queuedBackupHash
        queue_restored_sha256 = $queuedRestoredHash
        queue_final_sha256 = $queuedFinalHash
        backup_restore_performed = $true
        jobs_json_hash_unchanged = ($terminalSourceHash -eq $terminalFinalHash -and $queuedSourceHash -eq $queuedFinalHash)
        queue_fifo_observed = (@($passes | Where-Object { $_.queue_probe -and $_.projection.fifo_selection_observed }).Count -eq 2)
        passes = $passes
    }
}

$artifactRootFull = Resolve-PcvCompatibilityPath -Path $ArtifactRoot -BasePath $repoRoot
$frozenHostFull = Resolve-PcvCompatibilityPath -Path $FrozenHostPath -BasePath $repoRoot
$currentWriterProjectFull = Resolve-PcvCompatibilityPath -Path $CurrentWriterProjectPath -BasePath $repoRoot
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$summary = $null

try {
    if (Test-Path -LiteralPath $artifactRootFull) {
        if (@(Get-ChildItem -LiteralPath $artifactRootFull -Force).Count -gt 0) {
            throw "PCV_04265_READER_ARTIFACT_ROOT_NOT_EMPTY|Refusing to mix compatibility evidence in '$artifactRootFull'."
        }
    }
    else {
        [IO.Directory]::CreateDirectory($artifactRootFull) | Out-Null
    }

    if (-not (Test-Path -LiteralPath $frozenHostFull -PathType Leaf)) {
        throw "PCV_04265_READER_FROZEN_HOST_MISSING|Frozen 0.42.65 host was not found at '$frozenHostFull'."
    }
    if (-not (Test-Path -LiteralPath $currentWriterProjectFull -PathType Leaf)) {
        throw "PCV_04265_READER_CURRENT_WRITER_MISSING|Current writer fixture project was not found at '$currentWriterProjectFull'."
    }

    $observedSha256 = Get-PcvFileSha256 -Path $frozenHostFull
    if ($observedSha256 -ne $expectedFrozenHostSha256) {
        throw "PCV_04265_READER_FROZEN_HOST_HASH_MISMATCH|Expected $expectedFrozenHostSha256 but observed $observedSha256."
    }

    $versionInfo = (Get-Item -LiteralPath $frozenHostFull).VersionInfo
    $observedProductVersion = [string]$versionInfo.ProductVersion
    if ($observedProductVersion -ne $expectedFrozenHostProductVersion) {
        throw "PCV_04265_READER_FROZEN_HOST_VERSION_MISMATCH|Expected '$expectedFrozenHostProductVersion' but observed '$observedProductVersion'."
    }

    $schemaVersions = @(1, 2)
    if ($DryRun) {
        $summary = [ordered]@{
            schema_version = 1
            scope = 'job-store-04265-reader-compatibility'
            ok = $true
            actual_execution = 'dry-run-pinned-binary-no-listener'
            artifact_root = $artifactRootFull
            frozen_host = [ordered]@{
                path = $frozenHostFull
                expected_sha256 = $expectedFrozenHostSha256
                observed_sha256 = $observedSha256
                expected_product_version = $expectedFrozenHostProductVersion
                observed_product_version = $observedProductVersion
                pin_verified = $true
            }
            fixture_plans = @($schemaVersions | ForEach-Object {
                    [ordered]@{
                        schema_version = $_
                        generated_by_current_writer = $true
                        terminal_job_count = 3
                        queue_count = 2
                        passes_planned = @(
                            'terminal-initial-read',
                            'terminal-restored-read',
                            'queue-initial-read',
                            'queue-restored-read')
                    }
                })
            current_writer = [ordered]@{
                project_path = $currentWriterProjectFull
                execution_planned = 'dotnet run Release per schema/mode'
                manual_snapshot_assembly = $false
            }
            backup_restore_planned = $true
            service_mutation_performed = $false
            admin_required = $false
            hyperv_routes_invoked = $false
            host_mutation_performed = $false
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
            started_at = $startedAt.ToString('o')
            completed_at = [DateTimeOffset]::UtcNow.ToString('o')
        }
    }
    else {
        $fixtures = $schemaVersions | ForEach-Object {
            $scenarioRoot = Join-Path $artifactRootFull "schema-v$_"
            [IO.Directory]::CreateDirectory($scenarioRoot) | Out-Null
            New-PcvCurrentWriterFixture `
                -SchemaVersion $_ `
                -ScenarioRoot $scenarioRoot `
                -WriterProjectPath $currentWriterProjectFull
        }
        $fixtures = @($fixtures)
        $scenarios = @($fixtures | ForEach-Object {
                Invoke-PcvSchemaScenario `
                    -HostPath $frozenHostFull `
                    -Root $artifactRootFull `
                    -Fixture $_ `
                    -TimeoutSeconds $StartupTimeoutSeconds
            })
        $summary = [ordered]@{
            schema_version = 1
            scope = 'job-store-04265-reader-compatibility'
            ok = (@($scenarios | Where-Object { -not $_.ok }).Count -eq 0)
            actual_execution = 'frozen-04265-binary-high-loopback-reader'
            artifact_root = $artifactRootFull
            frozen_host = [ordered]@{
                path = $frozenHostFull
                expected_sha256 = $expectedFrozenHostSha256
                observed_sha256 = $observedSha256
                expected_product_version = $expectedFrozenHostProductVersion
                observed_product_version = $observedProductVersion
                file_version = [string]$versionInfo.FileVersion
                pin_verified = $true
            }
            current_writer = [ordered]@{
                project_path = $currentWriterProjectFull
                project_sha256 = Get-PcvFileSha256 -Path $currentWriterProjectFull
                generated_fixture_count = 4
                manual_snapshot_assembly = $false
            }
            scenarios = $scenarios
            schema_versions = @(1, 2)
            pass_count = @($scenarios | ForEach-Object { $_.passes }).Count
            backup_restore_performed = (@($scenarios | Where-Object { -not $_.backup_restore_performed }).Count -eq 0)
            jobs_json_hash_unchanged = (@($scenarios | Where-Object { -not $_.jobs_json_hash_unchanged }).Count -eq 0)
            request_scope = 'GET /api/v1/jobs only'
            queue_probe_scope = 'read-only locked jobs.json; frozen start-save fails before provider dispatch'
            listener_scope = '127.0.0.1 high ephemeral ports (49152-65535)'
            native_operation_requests = 0
            service_mutation_performed = $false
            admin_required = $false
            hyperv_routes_invoked = $false
            host_mutation_performed = $false
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
            started_at = $startedAt.ToString('o')
            completed_at = [DateTimeOffset]::UtcNow.ToString('o')
        }
    }

    Write-PcvCompatibilityJson -Path $summaryPath -Value $summary
    Write-Output ($summary | ConvertTo-Json -Depth 32)
    if (-not [bool]$summary.ok) {
        exit 1
    }
    exit 0
}
catch {
    $failure = [ordered]@{
        schema_version = 1
        scope = 'job-store-04265-reader-compatibility'
        ok = $false
        actual_execution = if ($DryRun) { 'dry-run-failed' } else { 'frozen-reader-failed' }
        artifact_root = $artifactRootFull
        frozen_host_path = $frozenHostFull
        expected_frozen_host_sha256 = $expectedFrozenHostSha256
        expected_frozen_host_product_version = $expectedFrozenHostProductVersion
        service_mutation_performed = $false
        admin_required = $false
        hyperv_routes_invoked = $false
        host_mutation_performed = $false
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        error = [ordered]@{
            message = $_.Exception.Message
            category = [string]$_.CategoryInfo.Category
        }
        started_at = $startedAt.ToString('o')
        completed_at = [DateTimeOffset]::UtcNow.ToString('o')
    }

    if (Test-Path -LiteralPath $artifactRootFull -PathType Container) {
        Write-PcvCompatibilityJson -Path $summaryPath -Value $failure
    }
    Write-Output ($failure | ConvertTo-Json -Depth 16)
    exit 1
}

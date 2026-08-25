[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("installed-cli-qos-guest-smoke-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$TokenProtectedFile = 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json',
    [string]$IsoPath = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso',
    [string]$VmRoot = (Join-Path $env:TEMP 'pcv-installed-cli-qos-guest-smoke'),
    [string]$VmName = ('pcv-cli-qos-guest-' + ([guid]::NewGuid().ToString('N').Substring(0, 8))),
    [int]$JobTimeoutSeconds = 240,
    [int]$CommandTimeoutSeconds = 120,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
$vmRootFull = [System.IO.Path]::GetFullPath($VmRoot)
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
New-Item -ItemType Directory -Path $vmRootFull -Force | Out-Null

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$steps = New-Object System.Collections.Generic.List[object]
$started = Get-Date
$ok = $false
$errorRecord = $null
$cleanup = [ordered]@{
    attempted = $false
    pcvcli_delete_attempted = $false
    direct_hyperv_cleanup_attempted = $false
    vm_removed = $false
    vm_root_removed = $false
    error = $null
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [int]$Depth = 24
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
}

function ConvertTo-RedactedText {
    param([AllowNull()][string]$Text)

    if ($null -eq $Text) {
        return ''
    }

    $redacted = $Text -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]'
    $redacted = $redacted -replace '(?i)("?(access_token|refresh_token|authorization|password|protected_token)"?\s*[:=]\s*)"?[^,"\r\n}]+"?', '$1"[REDACTED]"'
    $redacted
}

function Assert-True {
    param(
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-JsonValue {
    param(
        [Parameter(Mandatory)][string]$Json,
        [Parameter(Mandatory)][string]$StepName
    )

    try {
        return $Json | ConvertFrom-Json -Depth 24
    }
    catch {
        throw "PCV_INSTALLED_CLI_QOS_GUEST_JSON_PARSE_FAILED|$StepName|$($_.Exception.Message)"
    }
}

function Save-Summary {
    $summary = [ordered]@{
        schema_version = 1
        ok = [bool]$ok
        installed_cli_qos_guest_smoke = if ($DryRun) { 'dry-run' } elseif ($ok) { 'pass' } else { 'failed' }
        actual_execution = if ($DryRun) { 'dry-run-no-mutation' } else { 'installed-pcvcli-hyperv-qos-guest-targeted-smoke' }
        artifact_root = $artifactRootFull
        product_root = $ProductRoot
        pcvcli_exe = $script:PcvCliExe
        token_source = 'default-protected-token-file-auto-discovery'
        token_protected_file_present = [bool](Test-Path -LiteralPath $TokenProtectedFile -PathType Leaf)
        token_value_observed = $false
        password_value_observed = $false
        iso_path = $IsoPath
        vm_name = $VmName
        vm_root = $vmRootFull
        commands = $steps.ToArray()
        cleanup = $cleanup
        host_mutation_performed = -not [bool]$DryRun
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        started_at = $started.ToString('o')
        completed_at = (Get-Date).ToString('o')
        error = $errorRecord
    }
    Write-JsonFile -Path $summaryPath -Value $summary
}

function Invoke-CapturedPcvCli {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowNonZero
    )

    $safeName = ($StepName -replace '[^A-Za-z0-9_.-]', '-').Trim('-')
    $rawStdoutPath = Join-Path $artifactRootFull "$safeName.stdout.raw.txt"
    $rawStderrPath = Join-Path $artifactRootFull "$safeName.stderr.raw.txt"
    $stdoutPath = Join-Path $artifactRootFull "$safeName.stdout.txt"
    $stderrPath = Join-Path $artifactRootFull "$safeName.stderr.txt"
    $startedAt = Get-Date

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $script:PcvCliExe
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.WorkingDirectory = (Get-Location).Path
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()

    if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
        try { $process.Kill($true) } catch { }
        throw "PCV_INSTALLED_CLI_QOS_GUEST_COMMAND_TIMEOUT|$StepName|timeout_seconds=$CommandTimeoutSeconds"
    }

    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    Set-Content -LiteralPath $rawStdoutPath -Value $stdout -Encoding UTF8
    Set-Content -LiteralPath $rawStderrPath -Value $stderr -Encoding UTF8

    $stdoutRedacted = ConvertTo-RedactedText -Text $stdout
    $stderrRedacted = ConvertTo-RedactedText -Text $stderr
    Set-Content -LiteralPath $stdoutPath -Value $stdoutRedacted -Encoding UTF8
    Set-Content -LiteralPath $stderrPath -Value $stderrRedacted -Encoding UTF8
    Remove-Item -LiteralPath $rawStdoutPath, $rawStderrPath -Force -ErrorAction SilentlyContinue

    $record = [ordered]@{
        step = $StepName
        command = 'pcvcli ' + ($Arguments -join ' ')
        exit_code = [int]$process.ExitCode
        stdout = $stdoutPath
        stderr = $stderrPath
        ok = [bool]($process.ExitCode -eq 0)
        started_at = $startedAt.ToString('o')
        completed_at = (Get-Date).ToString('o')
    }
    $steps.Add([pscustomobject]$record) | Out-Null
    Save-Summary

    if ($process.ExitCode -ne 0 -and -not $AllowNonZero) {
        throw "PCV_INSTALLED_CLI_QOS_GUEST_COMMAND_FAILED|$StepName|exit=$($process.ExitCode)|$stderrPath"
    }

    [pscustomobject]@{
        StepName = $StepName
        ExitCode = [int]$process.ExitCode
        Stdout = $stdoutRedacted
        Stderr = $stderrRedacted
        StdoutPath = $stdoutPath
        StderrPath = $stderrPath
        Json = if ([string]::IsNullOrWhiteSpace($stdoutRedacted)) { $null } else { Get-JsonValue -Json $stdoutRedacted -StepName $StepName }
    }
}

function Invoke-PcvCliJson {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $result = Invoke-CapturedPcvCli -StepName $StepName -Arguments (@('--json') + $Arguments)
    Assert-True -Condition ($null -ne $result.Json) -Message "PCV_INSTALLED_CLI_QOS_GUEST_EMPTY_JSON|$StepName"
    Assert-True -Condition ([bool]$result.Json.ok) -Message "PCV_INSTALLED_CLI_QOS_GUEST_NOT_OK|$StepName"
    $result
}

function Wait-PcvJobSucceeded {
    param(
        [Parameter(Mandatory)][string]$JobId,
        [Parameter(Mandatory)][string]$StepName
    )

    $deadline = (Get-Date).AddSeconds($JobTimeoutSeconds)
    do {
        Start-Sleep -Seconds 2
        $jobResult = Invoke-PcvCliJson -StepName "$StepName-job-get" -Arguments @('job', 'get', $JobId)
        $status = [string]$jobResult.Json.data.status
        if ($status -eq 'succeeded') {
            return $jobResult
        }
        if ($status -in @('failed', 'canceled')) {
            throw "PCV_INSTALLED_CLI_QOS_GUEST_JOB_FAILED|$StepName|job=$JobId|status=$status"
        }
    } while ((Get-Date) -lt $deadline)

    throw "PCV_INSTALLED_CLI_QOS_GUEST_JOB_TIMEOUT|$StepName|job=$JobId|timeout_seconds=$JobTimeoutSeconds"
}

function Invoke-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments
    $jobId = [string]$created.Json.data.job_id
    Assert-True -Condition (-not [string]::IsNullOrWhiteSpace($jobId)) -Message "PCV_INSTALLED_CLI_QOS_GUEST_JOB_ID_MISSING|$StepName"
    [pscustomobject]@{
        Created = $created
        Completed = Wait-PcvJobSucceeded -JobId $jobId -StepName $StepName
    }
}

try {
    $script:PcvCliExe = Join-Path $ProductRoot 'pcvcli.exe'
    if (-not (Test-Path -LiteralPath $script:PcvCliExe -PathType Leaf)) {
        $command = Get-Command pcvcli.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -eq $command -or -not (Test-Path -LiteralPath $command.Source -PathType Leaf)) {
            throw "PCV_INSTALLED_CLI_QOS_GUEST_EXE_MISSING|Installed pcvcli.exe was not found."
        }
        $script:PcvCliExe = $command.Source
    }

    if (-not (Test-Path -LiteralPath $TokenProtectedFile -PathType Leaf)) {
        throw "PCV_INSTALLED_CLI_QOS_GUEST_TOKEN_FILE_MISSING|Protected token file was not found.|$TokenProtectedFile"
    }

    if (-not (Test-Path -LiteralPath $IsoPath -PathType Leaf)) {
        throw "PCV_INSTALLED_CLI_QOS_GUEST_ISO_MISSING|ISO file was not found.|$IsoPath"
    }

    if ($DryRun) {
        $ok = $true
        return
    }

    $hostStatus = Invoke-PcvCliJson -StepName 'host-status' -Arguments @('host', 'status')
    Assert-True -Condition ([bool]$hostStatus.Json.data.hyperv.feature_enabled) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_HYPERV_FEATURE_DISABLED'
    Assert-True -Condition ([bool]$hostStatus.Json.data.hyperv.vmms_running) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_VMMS_NOT_RUNNING'

    $create = Invoke-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create',
        $VmName,
        '--iso', $IsoPath,
        '--cpu', '1',
        '--memory-mb', '1024',
        '--disk-gb', '8',
        '--vm-root', $vmRootFull,
        '--generation', '2'
    )
    Assert-True -Condition ([string]$create.Completed.Json.data.result.operation -eq 'vm.create') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_CREATE_RESULT_MISMATCH'

    $vmList = Invoke-PcvCliJson -StepName 'vm-list-after-create' -Arguments @('vm', 'list')
    $vmListContainsVm = [bool](@($vmList.Json.data) | Where-Object { $_.name -eq $VmName } | Select-Object -First 1)
    Assert-True -Condition $vmListContainsVm -Message 'PCV_INSTALLED_CLI_QOS_GUEST_LIST_MISSING_CREATED_VM'

    $vmGet = Invoke-PcvCliJson -StepName 'vm-get-after-create' -Arguments @('vm', 'get', $VmName)
    Assert-True -Condition ([string]$vmGet.Json.operation -eq 'vm.get') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_GET_OPERATION_MISMATCH'

    $limit = Invoke-PcvCliJob -StepName 'vm-limit' -Arguments @('vm', 'limit', $VmName, '--cpu', '1', '--memory-mb', '1024')
    Assert-True -Condition ([string]$limit.Completed.Json.data.result.data.action -eq 'limit') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_LIMIT_ACTION_MISMATCH'

    $blkio = Invoke-PcvCliJson -StepName 'vm-blkio-get' -Arguments @('vm', 'blkio-get', $VmName)
    Assert-True -Condition ([string]$blkio.Json.operation -eq 'vm.blkio-get') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_BLKIO_OPERATION_MISMATCH'
    Assert-True -Condition ([bool]($blkio.Json.data.storage_qos.linux_blkio_compatible -eq $false)) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_BLKIO_COMPAT_FLAG_MISMATCH'

    $bandwidth = Invoke-PcvCliJson -StepName 'vm-bandwidth' -Arguments @('vm', 'bandwidth', $VmName)
    Assert-True -Condition ([string]$bandwidth.Json.operation -eq 'vm.bandwidth') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_BANDWIDTH_OPERATION_MISMATCH'
    Assert-True -Condition ([bool]($bandwidth.Json.data.network_qos.linux_bandwidth_compatible -eq $false)) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_BANDWIDTH_COMPAT_FLAG_MISMATCH'

    $guestAgentStatus = Invoke-PcvCliJson -StepName 'vm-guest-agent-status' -Arguments @('vm', 'guest-agent-status', $VmName)
    Assert-True -Condition ([string]$guestAgentStatus.Json.operation -eq 'vm.guest-agent-status') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_AGENT_STATUS_OPERATION_MISMATCH'
    Assert-True -Condition ([bool]($guestAgentStatus.Json.data.guest_agent.qemu_guest_agent -eq $false)) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_AGENT_QEMU_FLAG_MISMATCH'

    $start = Invoke-PcvCliJob -StepName 'vm-start' -Arguments @('vm', 'start', $VmName)
    Assert-True -Condition ([string]$start.Completed.Json.data.result.data.action -eq 'start') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_START_ACTION_MISMATCH'

    $guestPing = Invoke-PcvCliJson -StepName 'vm-guest-ping' -Arguments @('vm', 'guest-ping', $VmName)
    Assert-True -Condition ([string]$guestPing.Json.operation -eq 'vm.guest-ping') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_PING_OPERATION_MISMATCH'
    Assert-True -Condition ([bool]($guestPing.Json.data.guest_ping.qemu_guest_agent -eq $false)) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_PING_QEMU_FLAG_MISMATCH'
    Assert-True -Condition ([bool]($guestPing.Json.data.guest_ping.guest_heartbeat_verified -eq $false)) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_HEARTBEAT_FLAG_MISMATCH'

    $poweroff = Invoke-PcvCliJob -StepName 'vm-poweroff' -Arguments @('vm', 'poweroff', $VmName)
    Assert-True -Condition ([string]$poweroff.Completed.Json.data.result.data.action -eq 'poweroff') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_POWEROFF_ACTION_MISMATCH'

    $delete = Invoke-PcvCliJob -StepName 'vm-delete' -Arguments @('vm', 'delete', $VmName, '--yes')
    $cleanup.pcvcli_delete_attempted = $true
    Assert-True -Condition ([string]$delete.Completed.Json.data.result.data.action -eq 'delete') -Message 'PCV_INSTALLED_CLI_QOS_GUEST_DELETE_ACTION_MISMATCH'
    $cleanup.vm_removed = $true

    $vmListAfterDelete = Invoke-PcvCliJson -StepName 'vm-list-after-delete' -Arguments @('vm', 'list')
    $vmStillPresent = [bool](@($vmListAfterDelete.Json.data) | Where-Object { $_.name -eq $VmName } | Select-Object -First 1)
    Assert-True -Condition (-not $vmStillPresent) -Message 'PCV_INSTALLED_CLI_QOS_GUEST_VM_STILL_PRESENT_AFTER_DELETE'

    $targetVmPath = Join-Path $vmRootFull $VmName
    $resolvedVmRoot = [System.IO.Path]::GetFullPath($vmRootFull)
    $resolvedTarget = [System.IO.Path]::GetFullPath($targetVmPath)
    if ($resolvedTarget.StartsWith($resolvedVmRoot, [System.StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTarget)) {
        Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
        $cleanup.vm_root_removed = $true
    }

    $ok = $true
}
catch {
    $errorRecord = [ordered]@{
        message = $_.Exception.Message
        category = [string]$_.CategoryInfo.Category
    }
    throw
}
finally {
    if (-not $DryRun) {
        $cleanup.attempted = $true
        try {
            $existingVm = Get-VM -Name $VmName -ErrorAction SilentlyContinue
            if ($null -ne $existingVm) {
                $cleanup.direct_hyperv_cleanup_attempted = $true
                if ($existingVm.State -ne 'Off') {
                    Stop-VM -Name $VmName -TurnOff -Force -ErrorAction SilentlyContinue
                }
                Remove-VM -Name $VmName -Force -ErrorAction Stop
                $cleanup.vm_removed = $true
            }

            $targetVmPath = Join-Path $vmRootFull $VmName
            $resolvedVmRoot = [System.IO.Path]::GetFullPath($vmRootFull)
            $resolvedTarget = [System.IO.Path]::GetFullPath($targetVmPath)
            if ($resolvedTarget.StartsWith($resolvedVmRoot, [System.StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTarget)) {
                Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
                $cleanup.vm_root_removed = $true
            }
        }
        catch {
            $cleanup.error = $_.Exception.Message
        }
    }

    Save-Summary
}

[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("windows-event-log-default-transition-installed-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$Version = '0.39.6-admin-smoke',
    [int]$BuildTimeoutSeconds = 900,
    [int]$MsiTimeoutSeconds = 900
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

$programData = [Environment]::GetEnvironmentVariable('ProgramData')
if ([string]::IsNullOrWhiteSpace($programData)) {
    $programData = 'C:\ProgramData'
}

$dataRoot = Join-Path $programData 'PureCVisor\desktop-node'
$protectedTokenPath = Join-Path $dataRoot 'api-token.dpapi.json'
$transitionEvidencePath = Join-Path $dataRoot 'eventlog-default-transition.json'
$eventSourceRegistryPath = 'HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\PureCVisor Desktop Node'
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$buildPath = Join-Path $artifactRootFull 'build.json'
$installPath = Join-Path $artifactRootFull 'install.json'
$installLogPath = Join-Path $artifactRootFull 'install.log'
$eventLogEvidenceCopyPath = Join-Path $artifactRootFull 'eventlog-default-transition.json'

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 900
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw "PCV_PROCESS_START_FAILED|Failed to start process.|$FileName"
    }

    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try {
            $process.Kill($true)
        }
        catch {
        }
        throw "PCV_PROCESS_TIMEOUT|Process timed out.|$FileName $($Arguments -join ' ')"
    }

    [pscustomobject][ordered]@{
        file_name = $FileName
        arguments = @($Arguments)
        exit_code = [int]$process.ExitCode
        stdout = $stdoutTask.GetAwaiter().GetResult()
        stderr = $stderrTask.GetAwaiter().GetResult()
        ok = [bool]($process.ExitCode -eq 0)
    }
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

function Get-ServiceInfo {
    Get-CimInstance Win32_Service -Filter "Name='PureCVisorDesktopNode'" -ErrorAction SilentlyContinue
}

function Assert-EventLogServicePath {
    param([Parameter(Mandatory)]$Service)

    $pathName = [string]$Service.PathName
    Assert-True -Condition ($pathName.Contains('DesktopNode.Host.exe', [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_PATH_MISMATCH|Service PathName is not owned by DesktopNode.Host.exe.'
    Assert-True -Condition ($pathName.Contains('--event-log-writer', [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_WRITER_MISMATCH|Service PathName does not declare an Event Log writer baseline.'
    Assert-True -Condition ($pathName.Contains('windows-event-log', [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_WRITER_MISMATCH|Service PathName does not use the Windows Event Log writer after default transition.'
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-ServiceInfo
        if ($null -ne $service -and [string]$service.State -eq $Expected) {
            return $true
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Read-ProtectedToken {
    param([Parameter(Mandatory)][string]$Path)

    Import-Module (Join-Path $repoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1') -Force
    $token = Read-PcvDesktopNodeProductProtectedTokenFile -Path $Path
    [string]$token.token
}

function Invoke-Api {
    param(
        [Parameter(Mandatory)][string]$Token,
        [string]$Path = '/api/v1/runtime/policy'
    )

    $requestArgs = @{
        Uri = 'http://127.0.0.1:7777' + $Path
        Headers = @{ Authorization = "Bearer $Token" }
        TimeoutSec = 15
        ErrorAction = 'Stop'
    }
    if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
        $requestArgs.UseBasicParsing = $true
    }
    $response = Invoke-WebRequest @requestArgs
    [pscustomobject][ordered]@{
        status_code = [int]$response.StatusCode
        body = ($response.Content | ConvertFrom-Json)
    }
}

$smokeStart = Get-Date
$summary = [ordered]@{
    ok = $false
    scope = 'windows-event-log-default-transition-installed'
    version = $Version
    artifact_root = $artifactRootFull
    actual_execution = 'installed-msi-local-system-custom-action'
    host_mutation_performed = $true
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    build = $null
    install = $null
    event_log_default_transition = $null
    event_log_provider = $null
    service = $null
    health = $null
    evidence_paths = [ordered]@{
        eventlog_default_transition = $eventLogEvidenceCopyPath
    }
}

try {
    $build = Invoke-CapturedProcess `
        -FileName 'pwsh' `
        -Arguments @(
            '-NoProfile',
            '-ExecutionPolicy',
            'Bypass',
            '-File',
            (Join-Path $repoRoot 'packaging/windows-desktop-node/installer/build.ps1'),
            '-Version',
            $Version,
            '-OutputRoot',
            $artifactRootFull,
            '-SigningMode',
            'AllowUnsignedDev'
        ) `
        -TimeoutSeconds $BuildTimeoutSeconds
    Write-JsonFile -Path $buildPath -Value $build
    Assert-True -Condition $build.ok -Message "PCV_EVENTLOG_DEFAULT_SMOKE_BUILD_FAILED|build.ps1 exited $($build.exit_code)."
    $buildOutput = $build.stdout | ConvertFrom-Json
    $summary.build = [ordered]@{
        msi_path = [string]$buildOutput.msi_path
        msi_sha256 = [string]$buildOutput.provenance.msi.sha256
        provenance_commit = [string]$buildOutput.provenance.git_commit
        signing_mode = [string]$buildOutput.provenance.signing_mode
    }

    $install = Invoke-CapturedProcess `
        -FileName 'msiexec.exe' `
        -Arguments @(
            '/i',
            [string]$buildOutput.msi_path,
            'REBOOT=ReallySuppress',
            'MSIRESTARTMANAGERCONTROL=Disable',
            '/qn',
            '/norestart',
            '/l*vx',
            $installLogPath
        ) `
        -TimeoutSeconds $MsiTimeoutSeconds
    Write-JsonFile -Path $installPath -Value $install
    Assert-True -Condition $install.ok -Message "PCV_EVENTLOG_DEFAULT_SMOKE_INSTALL_FAILED|msiexec exited $($install.exit_code)."
    $summary.install = [ordered]@{
        exit_code = $install.exit_code
        log_path = $installLogPath
    }

    Assert-True -Condition (Wait-ServiceState -Expected 'Running' -TimeoutSeconds 120) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_NOT_RUNNING|Service did not reach Running after install.'
    $installedService = Get-ServiceInfo
    Assert-True -Condition ($null -ne $installedService) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_NOT_FOUND|Service was not found after install.'
    Assert-EventLogServicePath -Service $installedService
    Assert-True -Condition (Test-Path -LiteralPath $transitionEvidencePath -PathType Leaf) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_TRANSITION_EVIDENCE_MISSING|eventlog-default-transition.json was not written.'
    Copy-Item -LiteralPath $transitionEvidencePath -Destination $eventLogEvidenceCopyPath -Force

    $transition = Get-Content -LiteralPath $transitionEvidencePath -Raw | ConvertFrom-Json
    $service = Get-ServiceInfo
    $registry = Get-ItemProperty -LiteralPath $eventSourceRegistryPath -ErrorAction Stop
    $events = @(Get-WinEvent -FilterHashtable @{
            LogName = 'Application'
            ProviderName = 'PureCVisor Desktop Node'
            Id = 39101
            StartTime = $smokeStart
        } -MaxEvents 5 -ErrorAction SilentlyContinue)
    $token = Read-ProtectedToken -Path $protectedTokenPath
    $health = Invoke-Api -Token $token
    $pathName = [string]$service.PathName

    $summary.event_log_default_transition = [ordered]@{
        ok = [bool]$transition.ok
        operation = [string]$transition.operation
        default_writer_status = [string]$transition.default_writer_status
        provider_repair_status = [string]$transition.provider_repair_status
        event_write_status = [string]$transition.event_write_status
        volume_guard_status = [string]$transition.volume_guard_status
        provider_remove_status = [string]$transition.provider_remove_status
        final_provider_status = [string]$transition.final_provider_status
        schema_version = [int]$transition.schema_version
        event_id = [int]$transition.event_id
        event_records_found = [int]$events.Count
        host_mutation_performed = [bool]$transition.host_mutation_performed
    }
    $summary.event_log_provider = [ordered]@{
        log_name = 'Application'
        source = 'PureCVisor Desktop Node'
        registry_present = $true
        event_message_file = [string]$registry.EventMessageFile
        final_state = if ([bool]$transition.event_log.exists) { 'present' } else { 'missing' }
        owned = [bool]$transition.event_log.owned
    }
    $summary.service = [ordered]@{
        name = [string]$service.Name
        state = [string]$service.State
        start_name = [string]$service.StartName
        path_name = $pathName
        uses_event_log_writer = $pathName.Contains('--event-log-writer', [System.StringComparison]::OrdinalIgnoreCase)
        uses_event_log_provider_source = $pathName.Contains('--event-log-provider-source', [System.StringComparison]::OrdinalIgnoreCase)
        uses_event_log_schema_version = $pathName.Contains('--event-log-schema-version', [System.StringComparison]::OrdinalIgnoreCase)
    }
    $summary.health = [ordered]@{
        runtime_policy_status_code = [int]$health.status_code
        token_value_observed = $false
    }

    Assert-True -Condition ([bool]$transition.ok) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_TRANSITION_NOT_OK|Transition descriptor did not report ok.'
    Assert-True -Condition ([string]$transition.operation -eq 'windows-event-log-default-transition') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_OPERATION_REQUIRED|Unexpected transition operation.'
    Assert-True -Condition ([string]$transition.default_writer_status -eq 'default-writer-pass') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_WRITER_REQUIRED|Default writer did not pass.'
    Assert-True -Condition ([string]$transition.provider_repair_status -eq 'provider-repair-pass') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_REPAIR_REQUIRED|Provider repair did not pass.'
    Assert-True -Condition ([string]$transition.event_write_status -eq 'write-query-pass') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_WRITE_REQUIRED|Event write did not pass.'
    Assert-True -Condition ([string]$transition.volume_guard_status -eq 'volume-guard-pass') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_VOLUME_REQUIRED|Volume guard did not pass.'
    Assert-True -Condition ([string]$transition.provider_remove_status -eq 'provider-remove-pass') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_REMOVE_REQUIRED|Provider remove did not pass.'
    Assert-True -Condition ([string]$transition.final_provider_status -eq 'provider-present') -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_FINAL_PROVIDER_REQUIRED|Final provider is not present.'
    Assert-True -Condition ([int]$transition.schema_version -eq 1) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SCHEMA_REQUIRED|Event schema version was not 1.'
    Assert-True -Condition ($events.Count -gt 0) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_EVENT_RECORD_REQUIRED|Event id 39101 was not found in Application log.'
    Assert-True -Condition ([bool]$summary.service.uses_event_log_writer) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_WRITER_ARG_REQUIRED|Service PathName does not include --event-log-writer.'
    Assert-True -Condition ([bool]$summary.service.uses_event_log_provider_source) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_PROVIDER_ARG_REQUIRED|Service PathName does not include --event-log-provider-source.'
    Assert-True -Condition ([bool]$summary.service.uses_event_log_schema_version) -Message 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_SCHEMA_ARG_REQUIRED|Service PathName does not include --event-log-schema-version.'
    Assert-True -Condition ([int]$health.status_code -eq 200) -Message "PCV_EVENTLOG_DEFAULT_SMOKE_HEALTH_FAILED|Runtime policy health returned $($health.status_code)."

    $summary.ok = $true
    Write-JsonFile -Path $summaryPath -Value $summary
    $summary | ConvertTo-Json -Depth 16
}
catch {
    $summary.error = [ordered]@{
        message = [string]$_.Exception.Message
        detail = [string]$_
    }
    Write-JsonFile -Path $summaryPath -Value $summary
    throw
}

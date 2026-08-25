param(
    [string]$Version = '0.26.5-admin-smoke',
    [string]$IsoPath = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso',
    [string]$ArtifactRoot,
    [string]$BatchEvidenceRoot,
    [int]$BuildTimeoutSeconds = 600,
    [int]$ServiceActionTimeoutSeconds = 240,
    [int]$MsiStepTimeoutSeconds = 900,
    [int]$JobTimeoutSeconds = 240,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRootCandidate = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path -LiteralPath (Join-Path $repoRootCandidate 'packaging\windows-desktop-node\installer\build.ps1'))) {
    $repoRootCandidate = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
}
$repoRoot = $repoRootCandidate
if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $repoRoot ("artifacts\routeparity-service-msi-hyperv-mutation-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
$ArtifactRoot = [System.IO.Path]::GetFullPath($ArtifactRoot)
if (-not [string]::IsNullOrWhiteSpace($BatchEvidenceRoot)) {
    $BatchEvidenceRoot = [System.IO.Path]::GetFullPath($BatchEvidenceRoot)
    New-Item -ItemType Directory -Path $BatchEvidenceRoot -Force | Out-Null
}
New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null
$progressPath = Join-Path $ArtifactRoot 'progress.json'

$serviceName = 'PureCVisorDesktopNode'
$defaultDataRoot = Join-Path $env:ProgramData 'PureCVisor\desktop-node'
$defaultProtectedToken = Join-Path $defaultDataRoot 'api-token.dpapi.json'
$summarySteps = New-Object System.Collections.Generic.List[object]
$bootTimeBefore = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime

function Write-JsonFile {
    param(
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] $Value,
        [int]$Depth = 20
    )

    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Save-Progress {
    Write-JsonFile -Path $progressPath -Value ([pscustomobject][ordered]@{
        schema_version = 1
        artifact_root = $ArtifactRoot
        version = $Version
        updated_at = (Get-Date).ToString('o')
        steps = $summarySteps.ToArray()
    })
}

function Add-Step {
    param(
        [Parameter(Mandatory)] [string]$Name,
        [bool]$Ok = $false,
        [string]$Path,
        [ValidateSet('started', 'completed', 'failed')]
        [string]$Status,
        [string]$ErrorMessage
    )

    if ([string]::IsNullOrWhiteSpace($Status)) {
        $Status = if ($Ok) { 'completed' } else { 'failed' }
    }

    $summarySteps.Add([pscustomobject][ordered]@{
        name = $Name
        status = $Status
        ok = $Ok
        path = $Path
        error = $ErrorMessage
        timestamp = (Get-Date).ToString('o')
    }) | Out-Null
    Save-Progress
}

function Start-Step {
    param(
        [Parameter(Mandatory)] [string]$Name,
        [string]$Path
    )

    $params = @{
        Name = $Name
        Ok = $false
        Status = 'started'
    }
    if (-not [string]::IsNullOrWhiteSpace($Path)) {
        $params.Path = $Path
    }
    Add-Step @params
}

function Get-RemainingPcvVms {
    @(Get-VM -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like 'pcv-spike-*' } |
        Select-Object Name, State, Path)
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)] [string]$FileName,
        [Parameter(Mandatory)] [string[]]$Arguments,
        [string]$WorkingDirectory = $repoRoot,
        [int]$TimeoutSeconds = 900
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $started = Get-Date
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
        if ($timedOut) {
            try {
                $process.Kill($true)
                [void]$process.WaitForExit(5000)
            }
            catch {
            }
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        $finished = Get-Date

        [pscustomobject][ordered]@{
            file_name = $FileName
            arguments = @($Arguments)
            exit_code = if ($timedOut) { -1 } else { $process.ExitCode }
            stdout = $stdout
            stderr = $stderr
            timed_out = $timedOut
            duration_ms = [int]($finished - $started).TotalMilliseconds
            ok = -not $timedOut -and $process.ExitCode -eq 0
        }
    }
    finally {
        $process.Dispose()
    }
}

function Get-ServiceInfo {
    $service = Get-CimInstance Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return $null
    }

    [pscustomobject][ordered]@{
        Name = $service.Name
        State = $service.State
        StartMode = $service.StartMode
        PathName = $service.PathName
        ProcessId = $service.ProcessId
    }
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)] [string]$Expected,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-ServiceInfo
        if ($Expected -eq 'Missing' -and $null -eq $service) {
            return $true
        }
        if ($null -ne $service -and $service.State -eq $Expected) {
            return $true
        }
        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Remove-DesktopNodeServiceIfPresent {
    $before = Get-ServiceInfo
    $commands = @()
    if ($null -ne $before) {
        $commands += Invoke-CapturedProcess -FileName 'sc.exe' -Arguments @('stop', $serviceName) -TimeoutSeconds 60
        [void](Wait-ServiceState -Expected 'Stopped' -TimeoutSeconds 45)
        $commands += Invoke-CapturedProcess -FileName 'sc.exe' -Arguments @('delete', $serviceName) -TimeoutSeconds 60
        [void](Wait-ServiceState -Expected 'Missing' -TimeoutSeconds 45)
    }

    [pscustomobject][ordered]@{
        before = $before
        commands = @($commands)
        after = Get-ServiceInfo
    }
}

function Initialize-ProtectedDataSupport {
    if ('System.Security.Cryptography.ProtectedData' -as [type]) {
        return
    }

    foreach ($assemblyName in @('System.Security.Cryptography.ProtectedData', 'System.Security')) {
        try {
            Add-Type -AssemblyName $assemblyName -ErrorAction Stop
            if ('System.Security.Cryptography.ProtectedData' -as [type]) {
                return
            }
        }
        catch {
        }
    }

    throw 'PCV_SMOKE_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support is unavailable.|System.Security.Cryptography.ProtectedData type was not found.'
}

function Get-ProtectedTokenEntropy {
    [System.Text.Encoding]::UTF8.GetBytes('PureCVisor Desktop Node API Token Store v1')
}

function Get-ObjectPropertyValue {
    param(
        [Parameter(Mandatory)] $InputObject,
        [Parameter(Mandatory)] [string]$Name
    )

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    $property.Value
}

function Read-ProtectedToken {
    param([Parameter(Mandatory)] [string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_NOT_FOUND|The protected API token file was not found.|Create the protected token file before starting the listener: '$Path'."
    }

    try {
        $json = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_INVALID|The protected API token file is not valid JSON.|$($_.Exception.Message)"
    }

    $schemaVersion = Get-ObjectPropertyValue -InputObject $json -Name 'schema_version'
    $storage = [string](Get-ObjectPropertyValue -InputObject $json -Name 'storage')
    $scope = [string](Get-ObjectPropertyValue -InputObject $json -Name 'scope')
    $protectedToken = [string](Get-ObjectPropertyValue -InputObject $json -Name 'protected_token')

    if ([int]$schemaVersion -ne 1 -or
        $storage -ne 'dpapi-local-machine' -or
        $scope -ne 'LocalMachine' -or
        [string]::IsNullOrWhiteSpace($protectedToken)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_INVALID|The protected API token file schema is invalid.|Expected schema_version 1, storage dpapi-local-machine, scope LocalMachine, and protected_token in '$Path'."
    }

    try {
        Initialize-ProtectedDataSupport
        $protectedBytes = [Convert]::FromBase64String($protectedToken)
        $tokenBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
            $protectedBytes,
            (Get-ProtectedTokenEntropy),
            [System.Security.Cryptography.DataProtectionScope]::LocalMachine
        )
        $token = [System.Text.Encoding]::UTF8.GetString($tokenBytes)
    }
    catch {
        throw "PCV_SERVICE_PROTECTED_TOKEN_UNPROTECT_FAILED|The protected service API token could not be read.|$($_.Exception.Message)"
    }

    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_EMPTY|The protected service API token resolved to an empty value.|Rotate the protected token file: '$Path'."
    }

    $token
}

function Get-MsiStepFailureClassification {
    param(
        [Parameter(Mandatory)] $MsiStepResult,
        [string]$LogPath
    )

    $stepName = [string](Get-ObjectPropertyValue -InputObject $MsiStepResult -Name 'name')
    $exitCode = [int](Get-ObjectPropertyValue -InputObject $MsiStepResult -Name 'exit_code')
    $logText = ''
    if (-not [string]::IsNullOrWhiteSpace($LogPath) -and (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
        try {
            $logText = Get-Content -Raw -LiteralPath $LogPath -ErrorAction Stop
        }
        catch {
            $logText = ''
        }
    }

    $matchedMarkers = @()
    foreach ($marker in @(
            'RepairInstalled returned actual error code -1073741510',
            'MsiSystemRebootPending = 1',
            'Restart Manager failed',
            'Restart Manager session ended unexpectedly',
            'RM session failed')) {
        if ($logText -match [regex]::Escape($marker)) {
            $matchedMarkers += $marker
        }
    }

    if ($stepName -eq 'repair' -and $exitCode -eq 1603 -and @($matchedMarkers).Count -gt 0) {
        return [pscustomobject][ordered]@{
            code = 'msi-repair-retryable-transient'
            retryable = $true
            recommendation = 'rerun-batch-step'
            reasons = @($matchedMarkers)
            log_path = $LogPath
        }
    }

    [pscustomobject][ordered]@{
        code = 'msi-step-hard-failure'
        retryable = $false
        recommendation = 'inspect-msi-log'
        reasons = @($matchedMarkers)
        log_path = $LogPath
    }
}

function Write-MsiLifecycleEvidence {
    param(
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] $Lifecycle
    )

    Write-JsonFile -Path $Path -Value ([pscustomobject]$Lifecycle)
}

if ($SelfTest) {
    Start-Step -Name 'capture-self-test'
    $script = @'
$chunk = 'x' * 4096
1..512 | ForEach-Object { [Console]::Out.Write($chunk) }
1..128 | ForEach-Object { [Console]::Error.Write($chunk) }
'@
    $captureSelfTestPath = Join-Path $ArtifactRoot 'capture-self-test.json'
    $result = Invoke-CapturedProcess -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', $script) -TimeoutSeconds 30
    $captureOk = [bool](
        $result.ok -and
        $result.stdout.Length -ge (4096 * 512) -and
        $result.stderr.Length -ge (4096 * 128))
    Write-JsonFile -Path $captureSelfTestPath -Value ([pscustomobject][ordered]@{
        ok = $captureOk
        result = $result
    })
    $captureSelfTestStatus = if ($captureOk) { 'completed' } else { 'failed' }
    Add-Step -Name 'capture-self-test' -Ok $captureOk -Path $captureSelfTestPath -Status $captureSelfTestStatus

    Start-Step -Name 'protected-token-self-test'
    $protectedTokenSelfTestPath = Join-Path $ArtifactRoot 'protected-token-self-test.json'
    $protectedTokenPath = Join-Path $ArtifactRoot 'protected-token-self-test.dpapi.json'
    $tokenSelfTestOk = $false
    $tokenSelfTest = [ordered]@{
        ok = $false
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        token_length = 0
        protected_file_removed = $false
        error = $null
    }
    try {
        $tokenValue = 'pcv-selftest-token-' + [guid]::NewGuid().ToString('N')
        Initialize-ProtectedDataSupport
        $protectedBytes = [System.Security.Cryptography.ProtectedData]::Protect(
            [System.Text.Encoding]::UTF8.GetBytes($tokenValue),
            (Get-ProtectedTokenEntropy),
            [System.Security.Cryptography.DataProtectionScope]::LocalMachine
        )
        Write-JsonFile -Path $protectedTokenPath -Value ([pscustomobject][ordered]@{
            schema_version = 1
            storage = 'dpapi-local-machine'
            scope = 'LocalMachine'
            created_at = (Get-Date).ToUniversalTime().ToString('o')
            token_sha256 = '[self-test-redacted]'
            protected_token = [Convert]::ToBase64String($protectedBytes)
        })

        $roundTrip = Read-ProtectedToken -Path $protectedTokenPath
        $tokenSelfTestOk = [bool]($roundTrip -eq $tokenValue)
        $tokenSelfTest['ok'] = $tokenSelfTestOk
        $tokenSelfTest['token_length'] = $roundTrip.Length
    }
    catch {
        $tokenSelfTest['error'] = [string]$_
    }
    finally {
        Remove-Item -LiteralPath $protectedTokenPath -Force -ErrorAction SilentlyContinue
        $tokenSelfTest['protected_file_removed'] = -not (Test-Path -LiteralPath $protectedTokenPath)
    }

    Write-JsonFile -Path $protectedTokenSelfTestPath -Value ([pscustomobject]$tokenSelfTest)
    $protectedTokenSelfTestOk = [bool]($tokenSelfTestOk -and [bool]$tokenSelfTest['protected_file_removed'])
    $protectedTokenSelfTestStatus = if ($protectedTokenSelfTestOk) { 'completed' } else { 'failed' }
    Add-Step -Name 'protected-token-self-test' -Ok $protectedTokenSelfTestOk -Path $protectedTokenSelfTestPath -Status $protectedTokenSelfTestStatus

    Start-Step -Name 'msi-classifier-self-test'
    $msiClassifierSelfTestPath = Join-Path $ArtifactRoot 'msi-classifier-self-test.json'
    $retryableLogPath = Join-Path $ArtifactRoot 'msi-classifier-retryable.log'
    $hardLogPath = Join-Path $ArtifactRoot 'msi-classifier-hard.log'
    Set-Content -LiteralPath $retryableLogPath -Value 'RepairInstalled returned actual error code -1073741510' -NoNewline
    Set-Content -LiteralPath $hardLogPath -Value 'generic fatal MSI failure' -NoNewline
    $retryable = Get-MsiStepFailureClassification `
        -MsiStepResult ([pscustomobject][ordered]@{ name = 'repair'; exit_code = 1603 }) `
        -LogPath $retryableLogPath
    $hard = Get-MsiStepFailureClassification `
        -MsiStepResult ([pscustomobject][ordered]@{ name = 'install'; exit_code = 1603 }) `
        -LogPath $hardLogPath
    $msiClassifierOk = [bool](
        $retryable.retryable -and
        -not $hard.retryable -and
        [string]$retryable.code -eq 'msi-repair-retryable-transient' -and
        [string]$retryable.recommendation -eq 'rerun-batch-step')
    Write-JsonFile -Path $msiClassifierSelfTestPath -Value ([pscustomobject][ordered]@{
        ok = $msiClassifierOk
        retryable = $retryable
        hard = $hard
    })
    $msiClassifierSelfTestStatus = if ($msiClassifierOk) { 'completed' } else { 'failed' }
    Add-Step -Name 'msi-classifier-self-test' -Ok $msiClassifierOk -Path $msiClassifierSelfTestPath -Status $msiClassifierSelfTestStatus

    $ok = [bool]($captureOk -and $protectedTokenSelfTestOk -and $msiClassifierOk)
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value ([pscustomobject][ordered]@{
        schema_version = 1
        ok = $ok
        artifact_root = $ArtifactRoot
        version = $Version
        self_test = $true
        steps = $summarySteps.ToArray()
    })
    if ($ok) {
        Write-Output $ArtifactRoot
        exit 0
    }

    exit 1
}

function Invoke-Api {
    param(
        [Parameter(Mandatory)] [string]$Method,
        [Parameter(Mandatory)] [string]$Path,
        [string]$Token,
        $Body
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers.Authorization = "Bearer $Token"
    }
    $uri = "http://127.0.0.1:7777$Path"
    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers -TimeoutSec 90
    }

    $json = $Body | ConvertTo-Json -Depth 12
    Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers -Body $json -ContentType 'application/json' -TimeoutSec 90
}

function Assert-True {
    param(
        [Parameter(Mandatory)] [bool]$Condition,
        [Parameter(Mandatory)] [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-JobAndWait {
    param(
        [Parameter(Mandatory)] [string]$Method,
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] [string]$Token,
        $Body,
        [int]$TimeoutSeconds = 180,
        [string[]]$AllowedStatuses = @('succeeded'),
        [string[]]$AllowedErrorCodes = @()
    )

    $created = Invoke-Api -Method $Method -Path $Path -Token $Token -Body $Body
    Assert-True -Condition ([bool]$created.ok) -Message "Job create failed for $Path."
    $jobId = [string]$created.data.job_id
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $completed = $null
    do {
        Start-Sleep -Seconds 1
        $completed = Invoke-Api -Method 'GET' -Path "/api/v1/jobs/$jobId" -Token $Token
        $status = [string]$completed.data.status
        if ($status -in @('succeeded', 'failed', 'canceled')) {
            break
        }
    } while ((Get-Date) -lt $deadline)

    Assert-True -Condition ($null -ne $completed) -Message "Job polling returned no result for $jobId."
    $completedStatus = [string]$completed.data.status
    $completedErrorCode = $null
    if ($null -ne $completed.data.error -and $completed.data.error.PSObject.Properties.Name -contains 'code') {
        $completedErrorCode = [string]$completed.data.error.code
    }
    $statusAllowed = $AllowedStatuses -contains $completedStatus
    $errorAllowed = $completedStatus -eq 'failed' -and $AllowedErrorCodes -contains $completedErrorCode
    Assert-True -Condition ($statusAllowed -or $errorAllowed) -Message "Job $jobId ended with status $completedStatus and error $completedErrorCode."

    [pscustomobject][ordered]@{
        created = $created
        completed = $completed
    }
}

function Test-WebRootUnavailable {
    try {
        Invoke-WebRequest -Uri 'http://127.0.0.1:7777/' -TimeoutSec 5 | Out-Null
        return $false
    }
    catch {
        return $true
    }
}

function Test-InstalledHealth {
    param([string]$TokenPath = $defaultProtectedToken)

    Assert-True -Condition (Wait-ServiceState -Expected 'Running' -TimeoutSeconds 90) -Message 'Service did not reach Running state.'
    $token = Read-ProtectedToken -Path $TokenPath
    $runtimePolicy = Invoke-Api -Method 'GET' -Path '/api/v1/runtime/policy' -Token $token
    $tokenStorage = $null
    if ($null -ne $runtimePolicy.data -and
        $runtimePolicy.data.PSObject.Properties.Name -contains 'auth' -and
        $null -ne $runtimePolicy.data.auth -and
        $runtimePolicy.data.auth.PSObject.Properties.Name -contains 'token_storage') {
        $tokenStorage = [string]$runtimePolicy.data.auth.token_storage
    }
    elseif ($null -ne $runtimePolicy.data -and
        $runtimePolicy.data.PSObject.Properties.Name -contains 'token_storage') {
        $tokenStorage = [string]$runtimePolicy.data.token_storage
    }
    Assert-True -Condition (-not [string]::IsNullOrWhiteSpace($tokenStorage)) -Message 'Runtime policy did not expose token storage.'
    $webRoot = Invoke-WebRequest -Uri 'http://127.0.0.1/' -TimeoutSec 30
    [pscustomobject][ordered]@{
        service = Get-ServiceInfo
        runtime_policy_ok = [bool]$runtimePolicy.ok
        token_storage = $tokenStorage
        web_root_status = [int]$webRoot.StatusCode
        token_length = $token.Length
    }
}

function Invoke-MsiStep {
    param(
        [Parameter(Mandatory)] [string]$Name,
        [Parameter(Mandatory)] [string]$Phase,
        [Parameter(Mandatory)] [string[]]$MsiArguments,
        [Parameter(Mandatory)] [string]$LogPath,
        [int[]]$SuccessExitCodes = @(0),
        [int[]]$ConditionalExitCodes = @(),
        [int]$TimeoutSeconds = 900
    )

    $arguments = @($MsiArguments) + @(
        'REBOOT=ReallySuppress',
        'MSIRESTARTMANAGERCONTROL=Disable',
        '/qn',
        '/norestart',
        '/l*vx',
        $LogPath
    )
    $result = Invoke-CapturedProcess -FileName 'msiexec.exe' -Arguments $arguments -TimeoutSeconds $TimeoutSeconds
    $allowed = $SuccessExitCodes -contains $result.exit_code
    $conditional = $ConditionalExitCodes -contains $result.exit_code
    [pscustomobject][ordered]@{
        name = $Name
        phase = $Phase
        arguments = @($arguments)
        log_path = $LogPath
        exit_code = $result.exit_code
        stdout = $result.stdout
        stderr = $result.stderr
        ok = [bool]($allowed -or $conditional)
        conditional = [bool]$conditional
        reboot_required = [bool]($result.exit_code -eq 3010)
        actual_reboot_initiated = [bool]($result.exit_code -eq 1641)
    }
}

function Remove-SmokeVm {
    param(
        [Parameter(Mandatory)] [string]$Name,
        [Parameter(Mandatory)] [string]$VmRoot
    )

    $removed = [ordered]@{
        name = $Name
        stopped = $false
        removed_vm = $false
        removed_path = $false
        skipped_reason = $null
    }
    if (-not $Name.StartsWith('pcv-spike-api-', [System.StringComparison]::OrdinalIgnoreCase)) {
        $removed.skipped_reason = 'name-prefix-mismatch'
        return [pscustomobject]$removed
    }

    $vm = Get-VM -Name $Name -ErrorAction SilentlyContinue
    if ($null -ne $vm) {
        if ($vm.State -ne 'Off') {
            Stop-VM -Name $Name -TurnOff -Force -ErrorAction SilentlyContinue
            $removed.stopped = $true
        }
        Remove-VM -Name $Name -Force -ErrorAction SilentlyContinue
        $removed.removed_vm = $true
    }

    $fullVmRoot = [System.IO.Path]::GetFullPath($VmRoot)
    $target = Join-Path $fullVmRoot $Name
    if ((Test-Path -LiteralPath $target) -and
        [System.IO.Path]::GetFullPath($target).StartsWith($fullVmRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $target -Recurse -Force
        $removed.removed_path = $true
    }

    [pscustomobject]$removed
}

try {
    if (-not (Test-Path -LiteralPath $IsoPath -PathType Leaf)) {
        throw "PCV_SMOKE_ISO_MISSING|ISO not found: $IsoPath"
    }

    Add-Step -Name 'initialize' -Ok $true

    $buildJsonPath = Join-Path $ArtifactRoot 'build-output.json'
    Start-Step -Name 'build-current-admin-smoke-msi' -Path $buildJsonPath
    $buildArgs = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        (Join-Path $repoRoot 'packaging\windows-desktop-node\installer\build.ps1'),
        '-Version',
        $Version,
        '-OutputRoot',
        $ArtifactRoot,
        '-SigningMode',
        'AllowUnsignedDev'
    )
    $buildProcess = Invoke-CapturedProcess -FileName 'pwsh' -Arguments $buildArgs -TimeoutSeconds $BuildTimeoutSeconds
    if (-not $buildProcess.ok) {
        Write-JsonFile -Path $buildJsonPath -Value $buildProcess
        throw "PCV_SMOKE_BUILD_FAILED|build.ps1 exited $($buildProcess.exit_code)."
    }
    $buildOutput = $buildProcess.stdout | ConvertFrom-Json
    Write-JsonFile -Path $buildJsonPath -Value $buildOutput
    Add-Step -Name 'build-current-admin-smoke-msi' -Ok $true -Path $buildJsonPath

    $msiPath = [string]$buildOutput.msi_path
    $payloadRoot = [string]$buildOutput.provenance.payload.root

    $serviceActionPath = Join-Path $ArtifactRoot 'service-action-smoke.json'
    Start-Step -Name 'service-action-smoke' -Path $serviceActionPath
    $serviceActionProductRoot = Join-Path $env:TEMP ("pcv-routeparity-service-action-product-" + [guid]::NewGuid().ToString('N'))
    $serviceActionDataRoot = Join-Path $env:ProgramData 'PureCVisor\desktop-node-routeparity-service-action'
    $serviceAction = [ordered]@{
        ok = $false
        product_root = $serviceActionProductRoot
        data_root = $serviceActionDataRoot
        preclean = $null
        configure = $null
        health = $null
        data_root_remove_blocked_while_service_exists = $null
        remove = $null
        remove_data_handoff = $null
        data_root_exists_after_handoff = $null
        data_root_remove = $null
        data_root_exists_after_data_root_remove = $null
        unrelated_path_exists_after_data_root_remove = $null
        cleanup = $null
    }
    try {
        $serviceAction.preclean = Remove-DesktopNodeServiceIfPresent
        New-Item -ItemType Directory -Path $serviceActionProductRoot -Force | Out-Null
        Copy-Item -Path (Join-Path $payloadRoot '*') -Destination $serviceActionProductRoot -Recurse -Force
        $hostExe = Join-Path $serviceActionProductRoot 'DesktopNode.Host.exe'
        $configure = Invoke-CapturedProcess -FileName $hostExe -Arguments @(
            'service-action', 'configure-installed',
            '--product-root', $serviceActionProductRoot,
            '--data-root', $serviceActionDataRoot,
            '--service-exe', $hostExe
        ) -TimeoutSeconds $ServiceActionTimeoutSeconds
        $serviceAction.configure = $configure
        Assert-True -Condition $configure.ok -Message 'service-action configure-installed failed.'
        $serviceAction.health = Test-InstalledHealth -TokenPath (Join-Path $serviceActionDataRoot 'api-token.dpapi.json')
        $diagnosticsPath = Join-Path $serviceActionDataRoot 'diagnostics'
        $unrelatedPath = Join-Path $serviceActionDataRoot 'service-host.log'
        New-Item -ItemType Directory -Path $diagnosticsPath -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $serviceActionDataRoot 'api-token.txt') -Value 'legacy-delete-me' -NoNewline
        Set-Content -LiteralPath (Join-Path $serviceActionDataRoot 'jobs.json') -Value '{}' -NoNewline
        Set-Content -LiteralPath (Join-Path $serviceActionDataRoot 'events.jsonl') -Value '{}' -NoNewline
        Set-Content -LiteralPath (Join-Path $serviceActionDataRoot 'install.jsonl') -Value '{}' -NoNewline
        Set-Content -LiteralPath (Join-Path $diagnosticsPath 'bundle.json') -Value '{}' -NoNewline
        Set-Content -LiteralPath $unrelatedPath -Value 'keep-me' -NoNewline
        $blocked = Invoke-CapturedProcess -FileName $hostExe -Arguments @(
            'service-action', 'data-root-remove',
            '--product-root', $serviceActionProductRoot,
            '--data-root', $serviceActionDataRoot,
            '--service-exe', $hostExe,
            '--remove-data'
        ) -TimeoutSeconds $ServiceActionTimeoutSeconds
        $serviceAction.data_root_remove_blocked_while_service_exists = $blocked
        Assert-True -Condition (-not $blocked.ok) -Message 'service-action data-root-remove unexpectedly succeeded while service still exists.'
        $blockedResult = $blocked.stdout | ConvertFrom-Json
        Assert-True -Condition ([string]$blockedResult.ErrorCode -eq 'PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS') -Message 'data-root-remove did not report service-exists guard.'
        $remove = Invoke-CapturedProcess -FileName $hostExe -Arguments @(
            'service-action', 'remove-installed',
            '--product-root', $serviceActionProductRoot,
            '--data-root', $serviceActionDataRoot,
            '--service-exe', $hostExe,
            '--remove-data'
        ) -TimeoutSeconds $ServiceActionTimeoutSeconds
        $serviceAction.remove = $remove
        Assert-True -Condition $remove.ok -Message 'service-action remove-installed failed.'
        Assert-True -Condition (Wait-ServiceState -Expected 'Missing' -TimeoutSeconds 60) -Message 'Service remained after service-action remove.'
        $removeResult = $remove.stdout | ConvertFrom-Json
        $serviceAction.remove_data_handoff = $removeResult.RemoveDataHandoff
        Assert-True -Condition ($null -ne $removeResult.RemoveDataHandoff) -Message 'remove-installed --remove-data did not return RemoveDataHandoff.'
        Assert-True -Condition ([string]$removeResult.RemoveDataHandoff.Operation -eq 'data-root-remove') -Message 'RemoveDataHandoff operation was not data-root-remove.'
        Assert-True -Condition (@($removeResult.RemovedPaths).Count -eq 0) -Message 'remove-installed --remove-data directly removed data-root paths.'
        $serviceAction.data_root_exists_after_handoff = Test-Path -LiteralPath $serviceActionDataRoot
        Assert-True -Condition $serviceAction.data_root_exists_after_handoff -Message 'data root was removed before the data-root-remove gate.'
        $dataRootRemove = Invoke-CapturedProcess -FileName $hostExe -Arguments @(
            'service-action', 'data-root-remove',
            '--product-root', $serviceActionProductRoot,
            '--data-root', $serviceActionDataRoot,
            '--service-exe', $hostExe,
            '--remove-data'
        ) -TimeoutSeconds $ServiceActionTimeoutSeconds
        $serviceAction.data_root_remove = $dataRootRemove
        Assert-True -Condition $dataRootRemove.ok -Message 'service-action data-root-remove failed after service removal.'
        $dataRootRemoveResult = $dataRootRemove.stdout | ConvertFrom-Json
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains (Join-Path $serviceActionDataRoot 'api-token.dpapi.json')) -Message 'data-root-remove did not remove protected token file.'
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains (Join-Path $serviceActionDataRoot 'api-token.txt')) -Message 'data-root-remove did not remove legacy token file.'
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains (Join-Path $serviceActionDataRoot 'jobs.json')) -Message 'data-root-remove did not remove job store.'
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains (Join-Path $serviceActionDataRoot 'events.jsonl')) -Message 'data-root-remove did not remove event log.'
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains (Join-Path $serviceActionDataRoot 'install.jsonl')) -Message 'data-root-remove did not remove install log.'
        Assert-True -Condition (@($dataRootRemoveResult.RemovedPaths) -contains $diagnosticsPath) -Message 'data-root-remove did not remove diagnostics.'
        Assert-True -Condition (-not (@($dataRootRemoveResult.RemovedPaths) -contains $unrelatedPath)) -Message 'data-root-remove removed a non-allowlisted path.'
        $serviceAction.data_root_exists_after_data_root_remove = Test-Path -LiteralPath $serviceActionDataRoot
        $serviceAction.unrelated_path_exists_after_data_root_remove = Test-Path -LiteralPath $unrelatedPath
        Assert-True -Condition $serviceAction.data_root_exists_after_data_root_remove -Message 'data-root-remove removed the data root directory.'
        Assert-True -Condition $serviceAction.unrelated_path_exists_after_data_root_remove -Message 'data-root-remove removed non-allowlisted service-host log.'
        $serviceAction.ok = $true
    }
    finally {
        $currentService = Get-ServiceInfo
        if ($null -ne $currentService -and
            -not [string]::IsNullOrWhiteSpace($currentService.PathName) -and
            $currentService.PathName.Contains($serviceActionProductRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            [void](Invoke-CapturedProcess -FileName 'sc.exe' -Arguments @('stop', $serviceName) -TimeoutSeconds 60)
            [void](Wait-ServiceState -Expected 'Stopped' -TimeoutSeconds 45)
            [void](Invoke-CapturedProcess -FileName 'sc.exe' -Arguments @('delete', $serviceName) -TimeoutSeconds 60)
            [void](Wait-ServiceState -Expected 'Missing' -TimeoutSeconds 45)
        }
        if (Test-Path -LiteralPath $serviceActionProductRoot) {
            Remove-Item -LiteralPath $serviceActionProductRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path -LiteralPath $serviceActionDataRoot) {
            Remove-Item -LiteralPath $serviceActionDataRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
        $serviceAction.cleanup = [pscustomobject][ordered]@{
            product_root_exists = Test-Path -LiteralPath $serviceActionProductRoot
            data_root_exists = Test-Path -LiteralPath $serviceActionDataRoot
            service = Get-ServiceInfo
        }
        Write-JsonFile -Path $serviceActionPath -Value ([pscustomobject]$serviceAction)
    }
    Add-Step -Name 'service-action-smoke' -Ok $serviceAction.ok -Path $serviceActionPath

    $lifecyclePath = Join-Path $ArtifactRoot 'msi-lifecycle-smoke.json'
    Start-Step -Name 'msi-lifecycle-smoke' -Path $lifecyclePath
    $lifecycleLogRoot = Join-Path $ArtifactRoot 'msi-logs'
    New-Item -ItemType Directory -Path $lifecycleLogRoot -Force | Out-Null
    $lifecycle = [ordered]@{
        ok = $false
        msi_path = $msiPath
        batch_evidence_root = $BatchEvidenceRoot
        steps = @()
        health = @()
        failure = $null
        failure_classification = $null
        boot_time_before = $bootTimeBefore
        boot_time_after = $null
        boot_time_unchanged = $false
    }
    $batchEvidenceRootProperty = @()
    if (-not [string]::IsNullOrWhiteSpace($BatchEvidenceRoot)) {
        $batchEvidenceRootProperty = @("BATCH_EVIDENCE_ROOT=$BatchEvidenceRoot")
    }
    $steps = @(
        @{ name = 'install'; phase = 'Install'; args = @('/i', $msiPath) + $batchEvidenceRootProperty; log = 'install.log'; success = @(0); conditional = @() },
        @{ name = 'repair'; phase = 'Repair'; args = @('/i', $msiPath, 'REINSTALL=ALL', 'REINSTALLMODE=vomus') + $batchEvidenceRootProperty; log = 'repair.log'; success = @(0); conditional = @(3010) },
        @{ name = 'uninstall-preserve'; phase = 'Uninstall'; args = @('/x', $msiPath); log = 'uninstall-preserve.log'; success = @(0); conditional = @() },
        @{ name = 'install-remove-data'; phase = 'InstallRemoveData'; args = @('/i', $msiPath) + $batchEvidenceRootProperty; log = 'install-remove-data.log'; success = @(0); conditional = @() },
        @{ name = 'uninstall-remove-data'; phase = 'UninstallRemoveData'; args = @('/x', $msiPath, 'REMOVE_DATA=1'); log = 'uninstall-remove-data.log'; success = @(0); conditional = @() },
        @{ name = 'final-restore-install'; phase = 'Install'; args = @('/i', $msiPath) + $batchEvidenceRootProperty; log = 'final-restore-install.log'; success = @(0); conditional = @() }
    )
    foreach ($step in $steps) {
        $stepLogPath = Join-Path $lifecycleLogRoot $step.log
        $result = Invoke-MsiStep `
            -Name $step.name `
            -Phase $step.phase `
            -MsiArguments $step.args `
            -LogPath $stepLogPath `
            -SuccessExitCodes $step.success `
            -ConditionalExitCodes $step.conditional `
            -TimeoutSeconds $MsiStepTimeoutSeconds
        $classification = $null
        if (-not $result.ok -or $result.actual_reboot_initiated) {
            $classification = Get-MsiStepFailureClassification -MsiStepResult $result -LogPath $stepLogPath
            $result | Add-Member -NotePropertyName failure_classification -NotePropertyValue $classification -Force
        }
        $lifecycle.steps += $result
        Write-MsiLifecycleEvidence -Path $lifecyclePath -Lifecycle $lifecycle
        if (-not $result.ok -or $result.actual_reboot_initiated) {
            $lifecycle.failure = [pscustomobject][ordered]@{
                step = $step.name
                exit_code = $result.exit_code
                actual_reboot_initiated = $result.actual_reboot_initiated
            }
            $lifecycle.failure_classification = $classification
            Write-MsiLifecycleEvidence -Path $lifecyclePath -Lifecycle $lifecycle
            throw "PCV_SMOKE_MSI_STEP_FAILED|$($step.name) exited $($result.exit_code).|classification=$($classification.code)|recommendation=$($classification.recommendation)"
        }

        if ($step.name -in @('install', 'repair', 'install-remove-data', 'final-restore-install')) {
            $lifecycle.health += [pscustomobject][ordered]@{
                step = $step.name
                result = Test-InstalledHealth
            }
        }
        if ($step.name -in @('uninstall-preserve', 'uninstall-remove-data')) {
            Assert-True -Condition (Wait-ServiceState -Expected 'Missing' -TimeoutSeconds 90) -Message "Service remained after $($step.name)."
            Assert-True -Condition (Test-WebRootUnavailable) -Message "Web root still answered after $($step.name)."
        }
        Write-MsiLifecycleEvidence -Path $lifecyclePath -Lifecycle $lifecycle
    }
    $lifecycle.boot_time_after = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $lifecycle.boot_time_unchanged = $lifecycle.boot_time_before -eq $lifecycle.boot_time_after
    Assert-True -Condition $lifecycle.boot_time_unchanged -Message 'Boot time changed during MSI lifecycle.'
    $finalLifecycleService = Get-ServiceInfo
    $lifecycle.service_path_has_batch_evidence_root = $null
    if (-not [string]::IsNullOrWhiteSpace($BatchEvidenceRoot)) {
        $pathName = if ($null -ne $finalLifecycleService) { [string]$finalLifecycleService.PathName } else { '' }
        $lifecycle.service_path_has_batch_evidence_root = [bool](
            -not [string]::IsNullOrWhiteSpace($pathName) -and
            $pathName.Contains('--batch-evidence-root', [System.StringComparison]::OrdinalIgnoreCase) -and
            $pathName.Contains($BatchEvidenceRoot, [System.StringComparison]::OrdinalIgnoreCase))
        Assert-True -Condition $lifecycle.service_path_has_batch_evidence_root -Message 'Final installed service path did not include the requested batch evidence root.'
    }
    $lifecycle.ok = $true
    Write-MsiLifecycleEvidence -Path $lifecyclePath -Lifecycle $lifecycle
    Add-Step -Name 'msi-lifecycle-smoke' -Ok $true -Path $lifecyclePath

    $hypervPath = Join-Path $ArtifactRoot 'hyperv-api-route-smoke.json'
    $vmName = 'pcv-spike-api-' + ([guid]::NewGuid().ToString('N').Substring(0, 8))
    $vmRoot = Join-Path $env:TEMP 'pcv-hyperv-api-smoke'
    New-Item -ItemType Directory -Path $vmRoot -Force | Out-Null
    Start-Step -Name 'installed-dotnet-host-hyperv-api-route-smoke' -Path $hypervPath
    $token = Read-ProtectedToken -Path $defaultProtectedToken
    $hyperv = [ordered]@{
        schema_version = 1
        name = 'installed-dotnet-host-hyperv-api-route-smoke'
        ok = $false
        iso = $IsoPath
        vm_name = $vmName
        unmanaged_vm_name = $null
        vm_root = $vmRoot
        host_status = $null
        network_inventory = $null
        vm_create = $null
        vm_list_contains_vm = $false
        vm_detail = $null
        vm_limit = $null
        vm_blkio_get = $null
        vm_bandwidth = $null
        vm_guest_agent_status = $null
        vm_guest_ping = $null
        vm_start = $null
        vm_restart = $null
        vm_shutdown_unavailable = $null
        checkpoint_create = $null
        checkpoint_list_contains_checkpoint = $false
        checkpoint_restore_precondition = 'vm.poweroff-before-restore'
        checkpoint_restore = $null
        checkpoint_delete = $null
        vm_poweroff = $null
        vm_delete = $null
        vm_delete_repeat = $null
        vm_list_absent_after_delete = $false
        unmanaged_vm_created = $false
        unmanaged_vm_delete_block = $null
        unmanaged_vm_still_exists_after_block = $false
        unmanaged_cleanup = $null
        cleanup = $null
        token_length = $token.Length
        token_sha256 = ([System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($token))).Replace('-', '').ToLowerInvariant())
    }
    try {
        $hyperv.host_status = Invoke-Api -Method 'GET' -Path '/api/v1/host/status' -Token $token
        Assert-True -Condition ([bool]$hyperv.host_status.ok) -Message 'host.status failed.'
        $hyperv.network_inventory = Invoke-Api -Method 'GET' -Path '/api/v1/network/inventory' -Token $token
        Assert-True -Condition ([bool]$hyperv.network_inventory.ok) -Message 'network.inventory failed.'
        $hyperv.vm_create = Invoke-JobAndWait -Method 'POST' -Path '/api/v1/vms' -Token $token -TimeoutSeconds $JobTimeoutSeconds -Body @{
            name = $vmName
            iso_path = $IsoPath
            cpu = 1
            memory_mb = 1024
            disk_gb = 8
            generation = 2
            vm_root = $vmRoot
        }
        $vmList = Invoke-Api -Method 'GET' -Path '/api/v1/vms' -Token $token
        $hyperv.vm_list_contains_vm = [bool](@($vmList.data) | Where-Object { $_.name -eq $vmName } | Select-Object -First 1)
        Assert-True -Condition $hyperv.vm_list_contains_vm -Message 'VM list did not contain created VM.'
        $hyperv.vm_detail = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName" -Token $token
        Assert-True -Condition ([bool]$hyperv.vm_detail.ok) -Message 'vm.get failed.'
        $hyperv.vm_limit = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/limit" -Token $token -TimeoutSeconds $JobTimeoutSeconds -Body @{
            cpu = 1
            memory_mb = 1024
        }
        Assert-True -Condition ([string]$hyperv.vm_limit.completed.data.result.data.action -eq 'limit') -Message 'vm.limit did not report limit action.'
        $hyperv.vm_blkio_get = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName/blkio" -Token $token
        Assert-True -Condition ([bool]$hyperv.vm_blkio_get.ok) -Message 'vm.blkio-get failed.'
        $hyperv.vm_bandwidth = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName/bandwidth" -Token $token
        Assert-True -Condition ([bool]$hyperv.vm_bandwidth.ok) -Message 'vm.bandwidth failed.'
        $hyperv.vm_guest_agent_status = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName/guest-agent/status" -Token $token
        Assert-True -Condition ([bool]$hyperv.vm_guest_agent_status.ok) -Message 'vm.guest-agent-status failed.'
        $hyperv.vm_start = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/start" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        $hyperv.vm_guest_ping = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName/guest-agent/ping" -Token $token
        Assert-True -Condition ([bool]$hyperv.vm_guest_ping.ok) -Message 'vm.guest-ping failed.'
        $hyperv.vm_restart = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/restart" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        $hyperv.vm_shutdown_unavailable = Invoke-JobAndWait `
            -Method 'POST' `
            -Path "/api/v1/vms/$vmName/shutdown" `
            -Token $token `
            -TimeoutSeconds $JobTimeoutSeconds `
            -AllowedStatuses @('failed') `
            -AllowedErrorCodes @('PCV_VM_SHUTDOWN_NOT_AVAILABLE')
        $hyperv.checkpoint_create = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/checkpoints" -Token $token -TimeoutSeconds $JobTimeoutSeconds -Body @{
            name = 'before-install'
        }
        $checkpointList = Invoke-Api -Method 'GET' -Path "/api/v1/vms/$vmName/checkpoints" -Token $token
        $hyperv.checkpoint_list_contains_checkpoint = [bool](@($checkpointList.data) | Where-Object { $_.name -eq 'before-install' } | Select-Object -First 1)
        Assert-True -Condition $hyperv.checkpoint_list_contains_checkpoint -Message 'Checkpoint list did not contain before-install.'
        $hyperv.vm_poweroff = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/poweroff" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        $hyperv.checkpoint_restore = Invoke-JobAndWait -Method 'POST' -Path "/api/v1/vms/$vmName/checkpoints/before-install/restore" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        Assert-True -Condition ([string]$hyperv.checkpoint_restore.completed.data.result.data.action -eq 'restore') -Message 'checkpoint.restore did not report restore action.'
        $hyperv.checkpoint_delete = Invoke-JobAndWait -Method 'DELETE' -Path "/api/v1/vms/$vmName/checkpoints/before-install" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        $hyperv.vm_delete = Invoke-JobAndWait -Method 'DELETE' -Path "/api/v1/vms/$vmName" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        Assert-True -Condition ([string]$hyperv.vm_delete.completed.data.result.data.action -eq 'delete') -Message 'vm.delete did not report delete action.'
        $hyperv.vm_delete_repeat = Invoke-JobAndWait -Method 'DELETE' -Path "/api/v1/vms/$vmName" -Token $token -TimeoutSeconds $JobTimeoutSeconds
        Assert-True -Condition ([string]$hyperv.vm_delete_repeat.completed.data.result.data.action -eq 'absent') -Message 'repeat vm.delete did not report absent action.'
        $vmListAfterDelete = Invoke-Api -Method 'GET' -Path '/api/v1/vms' -Token $token
        $hyperv.vm_list_absent_after_delete = -not [bool](@($vmListAfterDelete.data) | Where-Object { $_.name -eq $vmName } | Select-Object -First 1)
        Assert-True -Condition $hyperv.vm_list_absent_after_delete -Message 'VM list still contained deleted VM.'
        $foreignVmName = 'pcv-spike-api-foreign-' + ([guid]::NewGuid().ToString('N').Substring(0, 8))
        $hyperv.unmanaged_vm_name = $foreignVmName
        try {
            New-VM -Name $foreignVmName -Generation 2 -MemoryStartupBytes 512MB -NoVHD -Path $vmRoot | Out-Null
            $hyperv.unmanaged_vm_created = $true
            $hyperv.unmanaged_vm_delete_block = Invoke-JobAndWait `
                -Method 'DELETE' `
                -Path "/api/v1/vms/$foreignVmName" `
                -Token $token `
                -TimeoutSeconds $JobTimeoutSeconds `
                -AllowedStatuses @('failed') `
                -AllowedErrorCodes @('PCV_VM_NOT_MANAGED_BY_PURECVISOR')
            $hyperv.unmanaged_vm_still_exists_after_block = $null -ne (Get-VM -Name $foreignVmName -ErrorAction SilentlyContinue)
            Assert-True -Condition $hyperv.unmanaged_vm_still_exists_after_block -Message 'Unmanaged VM was removed despite delete guard.'
        }
        finally {
            $hyperv.unmanaged_cleanup = Remove-SmokeVm -Name $foreignVmName -VmRoot $vmRoot
        }
        $hyperv.ok = $true
    }
    finally {
        $hyperv.cleanup = Remove-SmokeVm -Name $vmName -VmRoot $vmRoot
        Write-JsonFile -Path $hypervPath -Value ([pscustomobject]$hyperv)
    }
    Add-Step -Name 'installed-dotnet-host-hyperv-api-route-smoke' -Ok $hyperv.ok -Path $hypervPath

    $bootTimeAfter = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $remainingVms = @(Get-RemainingPcvVms)
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        artifact_root = $ArtifactRoot
        version = $Version
        batch_evidence_root = $BatchEvidenceRoot
        service_path_has_batch_evidence_root = $lifecycle.service_path_has_batch_evidence_root
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = $bootTimeBefore -eq $bootTimeAfter
        final_service = Get-ServiceInfo
        remaining_pcv_vms = @($remainingVms)
        steps = $summarySteps.ToArray()
    }
    Add-Step -Name 'complete' -Ok $true
    $summary.steps = $summarySteps.ToArray()
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Output $ArtifactRoot
    exit 0
}
catch {
    $bootTimeAfter = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $false
        artifact_root = $ArtifactRoot
        version = $Version
        error = [string]$_
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = $bootTimeBefore -eq $bootTimeAfter
        final_service = Get-ServiceInfo
        remaining_pcv_vms = @(Get-RemainingPcvVms)
        steps = $summarySteps.ToArray()
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Error $_
    exit 1
}

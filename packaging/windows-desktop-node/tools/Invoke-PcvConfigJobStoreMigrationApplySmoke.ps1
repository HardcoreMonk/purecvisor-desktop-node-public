param(
    [string]$Version = '0.38.5-admin-smoke',
    [string]$ArtifactRoot,
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$DataRoot = (Join-Path $env:ProgramData 'PureCVisor\desktop-node'),
    [int]$BuildTimeoutSeconds = 600,
    [int]$MsiStepTimeoutSeconds = 900,
    [int]$ServiceActionTimeoutSeconds = 240,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRootCandidate = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
if (-not (Test-Path -LiteralPath (Join-Path $repoRootCandidate 'packaging\windows-desktop-node\installer\build.ps1'))) {
    $repoRootCandidate = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
}
$repoRoot = $repoRootCandidate

if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $repoRoot ("artifacts\config-jobstore-migration-apply-installed-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}

$ArtifactRoot = [System.IO.Path]::GetFullPath($ArtifactRoot)
$ProductRoot = [System.IO.Path]::GetFullPath($ProductRoot)
$DataRoot = [System.IO.Path]::GetFullPath($DataRoot)
New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null

$progressPath = Join-Path $ArtifactRoot 'progress.json'
$hostExe = Join-Path $ProductRoot 'DesktopNode.Host.exe'
$protectedTokenPath = Join-Path $DataRoot 'api-token.dpapi.json'
$manifestPath = Join-Path $ProductRoot 'product-manifest.json'
$jobStorePath = Join-Path $DataRoot 'jobs.json'
$serviceName = 'PureCVisorDesktopNode'
$smokeJobId = 'pcv-migration-smoke-v1'
$summarySteps = New-Object System.Collections.Generic.List[object]

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [int]$Depth = 32
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
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
        [Parameter(Mandatory)][string]$Name,
        [bool]$Ok = $false,
        [string]$Path,
        [string]$Status = $(if ($Ok) { 'completed' } else { 'failed' }),
        [string]$Note = ''
    )

    $summarySteps.Add([pscustomobject][ordered]@{
        name = $Name
        status = $Status
        ok = $Ok
        path = $Path
        note = $Note
        timestamp = (Get-Date).ToString('o')
    }) | Out-Null
    Save-Progress
}

function Start-Step {
    param(
        [Parameter(Mandatory)][string]$Name,
        [string]$Path
    )

    Add-Step -Name $Name -Ok $false -Status 'started' -Path $Path
}

function New-PlannedStep {
    param([Parameter(Mandatory)][string]$Name)

    [pscustomobject][ordered]@{
        name = $Name
        status = 'planned'
        ok = $true
        path = $null
        note = 'plan-only'
    }
}

function Test-IsAdministrator {
    try {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]::new($identity)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch {
        return $false
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

function Get-ObjectPropertyValue {
    param(
        [Parameter(Mandatory)]$InputObject,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $InputObject) {
        return $null
    }
    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    $property.Value
}

function Get-ServiceInfo {
    $service = Get-CimInstance Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return $null
    }

    [pscustomobject][ordered]@{
        name = $service.Name
        state = $service.State
        start_mode = $service.StartMode
        path_name = $service.PathName
        process_id = $service.ProcessId
    }
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-ServiceInfo
        if ($Expected -eq 'Missing' -and $null -eq $service) {
            return $true
        }
        if ($null -ne $service -and [string]$service.state -eq $Expected) {
            return $true
        }
        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
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

function Invoke-MsiStep {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string[]]$MsiArguments,
        [Parameter(Mandatory)][string]$LogPath,
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
    [pscustomobject][ordered]@{
        name = $Name
        arguments = @($arguments)
        log_path = $LogPath
        exit_code = $result.exit_code
        stdout = $result.stdout
        stderr = $result.stderr
        timed_out = $result.timed_out
        duration_ms = $result.duration_ms
        ok = [bool]$result.ok
        reboot_required = [bool]($result.exit_code -eq 3010)
        actual_reboot_initiated = [bool]($result.exit_code -eq 1641)
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
    throw 'PCV_MIGRATION_SMOKE_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support is unavailable.'
}

function Get-ProtectedTokenEntropy {
    [System.Text.Encoding]::UTF8.GetBytes('PureCVisor Desktop Node API Token Store v1')
}

function Read-ProtectedToken {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_MIGRATION_SMOKE_TOKEN_FILE_MISSING|Protected token file not found.|$Path"
    }

    $json = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    Initialize-ProtectedDataSupport
    $protectedBytes = [Convert]::FromBase64String([string]$json.protected_token)
    $tokenBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
        $protectedBytes,
        (Get-ProtectedTokenEntropy),
        [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
    $token = [System.Text.Encoding]::UTF8.GetString($tokenBytes)
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw 'PCV_MIGRATION_SMOKE_TOKEN_EMPTY|Protected token resolved to an empty value.'
    }
    $token
}

function Invoke-Api {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Token
    )

    Invoke-RestMethod `
        -Method $Method `
        -Uri "http://127.0.0.1:7777$Path" `
        -Headers @{ Authorization = "Bearer $Token" } `
        -TimeoutSec 30
}

function Invoke-ApiWithRetry {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Token,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $lastError = $null
    do {
        try {
            return Invoke-Api -Method $Method -Path $Path -Token $Token
        }
        catch {
            $lastError = [string]$_
            Start-Sleep -Seconds 1
        }
    } while ((Get-Date) -lt $deadline)

    throw "PCV_MIGRATION_SMOKE_API_PROBE_FAILED|API probe failed for $Method $Path.|$lastError"
}

function Get-FileSha256 {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Read-JsonObject {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Invoke-ServiceAction {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string]$Action,
        [string[]]$ExtraArguments = @(),
        [scriptblock]$BuildValidation,
        [scriptblock]$Validate
    )

    $path = Join-Path $ArtifactRoot "$StepName.json"
    $arguments = @(
        'service-action',
        $Action,
        '--product-root',
        $ProductRoot,
        '--data-root',
        $DataRoot,
        '--service-exe',
        $hostExe
    ) + @($ExtraArguments)
    $process = Invoke-CapturedProcess -FileName $hostExe -Arguments $arguments -WorkingDirectory $ProductRoot -TimeoutSeconds $ServiceActionTimeoutSeconds
    $parsed = $null
    if (-not [string]::IsNullOrWhiteSpace($process.stdout)) {
        $parsed = $process.stdout | ConvertFrom-Json
    }
    $validation = if ($null -ne $BuildValidation) {
        & $BuildValidation $process $parsed
    } else {
        $null
    }
    $ok = if ($null -ne $Validate) {
        [bool](& $Validate $process $parsed $validation)
    } else {
        [bool]($process.ok -and $null -ne $parsed -and [bool]$parsed.Ok)
    }

    Write-JsonFile -Path $path -Value ([pscustomobject][ordered]@{
        process = $process
        parsed = $parsed
        validation = $validation
        ok = $ok
    })
    Add-Step -Name $StepName -Ok $ok -Path $path
    Assert-True -Condition $ok -Message "PCV_MIGRATION_SMOKE_STEP_FAILED|Step '$StepName' failed."
    [pscustomobject][ordered]@{
        process = $process
        parsed = $parsed
        validation = $validation
        ok = $ok
        path = $path
    }
}

function New-CommandPlan {
    $serviceActionBase = @(
        '--product-root',
        $ProductRoot,
        '--data-root',
        $DataRoot,
        '--service-exe',
        $hostExe
    )
    [pscustomobject][ordered]@{
        build = [pscustomobject][ordered]@{
            file_name = 'pwsh'
            arguments = @(
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
        }
        install = [pscustomobject][ordered]@{
            file_name = 'msiexec.exe'
            arguments = @('/i', '<msi-from-build>', 'REBOOT=ReallySuppress', 'MSIRESTARTMANAGERCONTROL=Disable', '/qn', '/norestart')
        }
        stop_service = [pscustomobject][ordered]@{
            file_name = $hostExe
            arguments = @('service-action', 'stop') + $serviceActionBase
        }
        job_store_seed = [pscustomobject][ordered]@{
            path = $jobStorePath
            schema_version = 1
            setup_kind = 'direct-seed-while-service-stopped'
            job_id = $smokeJobId
        }
        config_migration = [pscustomobject][ordered]@{
            file_name = $hostExe
            arguments = @(
                'service-action',
                'config-migration-apply'
            ) + $serviceActionBase + @(
                '--migration-plan-id',
                'product-config-v1-to-v2',
                '--migration-plan-version',
                '1'
            )
        }
        job_store_migration = [pscustomobject][ordered]@{
            file_name = $hostExe
            arguments = @(
                'service-action',
                'job-store-migration-apply'
            ) + $serviceActionBase + @(
                '--migration-plan-id',
                'job-store-v1-to-v2',
                '--migration-plan-version',
                '1'
            )
        }
        start_service = [pscustomobject][ordered]@{
            file_name = $hostExe
            arguments = @('service-action', 'start') + $serviceActionBase
        }
        post_migration_api_read = [pscustomobject][ordered]@{
            auth = 'bearer-protected-token-file'
            token_path = $protectedTokenPath
            paths = @('/api/v1/runtime/policy', '/api/v1/jobs', "/api/v1/jobs/$smokeJobId")
        }
    }
}

function Backup-ExistingJobStore {
    $backupPath = Join-Path $ArtifactRoot 'job-store-before-seed.json'
    if (Test-Path -LiteralPath $jobStorePath -PathType Leaf) {
        Copy-Item -LiteralPath $jobStorePath -Destination $backupPath -Force
        return $backupPath
    }
    return $null
}

function Write-SmokeJobStoreV1 {
    $now = (Get-Date).ToUniversalTime().ToString('o')
    $jobStore = [ordered]@{
        version = 1
        saved_at = $now
        jobs = @(
            [ordered]@{
                job_id = $smokeJobId
                operation = 'migration.smoke'
                status = 'succeeded'
                params = [ordered]@{
                    source = 'installed-config-jobstore-migration-apply-smoke'
                }
                result = $null
                error = $null
                retry_of = $null
                request_id = 'migration-smoke'
                correlation_id = $smokeJobId
                attempt = 1
                canceled_at = $null
                created_at = $now
                updated_at = $now
            }
        )
        queue = @()
    }

    $backupPath = Backup-ExistingJobStore
    Write-JsonFile -Path $jobStorePath -Value ([pscustomobject]$jobStore)
    [pscustomobject][ordered]@{
        ok = $true
        job_store_path = $jobStorePath
        previous_job_store_backup = $backupPath
        seeded_schema_version = 1
        seeded_job_id = $smokeJobId
        service = Get-ServiceInfo
        service_stopped = [bool]((Get-ServiceInfo).state -eq 'Stopped')
    }
}

function New-ConfigMigrationValidation {
    param($Parsed)

    $descriptor = Get-ObjectPropertyValue -InputObject $Parsed -Name 'ConfigMigration'
    $manifest = Read-JsonObject -Path $manifestPath
    $backupPath = [string](Get-ObjectPropertyValue -InputObject $descriptor -Name 'BackupPath')
    $tempPath = [string](Get-ObjectPropertyValue -InputObject $descriptor -Name 'TempPath')
    [pscustomobject][ordered]@{
        manifest_path = $manifestPath
        manifest_exists = Test-Path -LiteralPath $manifestPath -PathType Leaf
        manifest_schema_version = if ($null -ne $manifest) { [int]$manifest.schema_version } else { $null }
        manifest_migration_plan_id = if ($null -ne $manifest -and $null -ne (Get-ObjectPropertyValue -InputObject $manifest -Name 'migration')) { [string]$manifest.migration.plan_id } else { $null }
        manifest_migration_plan_version = if ($null -ne $manifest -and $null -ne (Get-ObjectPropertyValue -InputObject $manifest -Name 'migration')) { [int]$manifest.migration.plan_version } else { $null }
        descriptor_mutation_performed = if ($null -ne $descriptor) { [bool]$descriptor.MutationPerformed } else { $false }
        descriptor_source_schema_version = if ($null -ne $descriptor) { [int]$descriptor.SourceSchemaVersion } else { $null }
        descriptor_target_schema_version = if ($null -ne $descriptor) { [int]$descriptor.TargetSchemaVersion } else { $null }
        backup_path = $backupPath
        backup_exists = -not [string]::IsNullOrWhiteSpace($backupPath) -and (Test-Path -LiteralPath $backupPath -PathType Leaf)
        temp_path = $tempPath
        temp_exists_after_apply = -not [string]::IsNullOrWhiteSpace($tempPath) -and (Test-Path -LiteralPath $tempPath -PathType Leaf)
    }
}

function New-JobStoreMigrationValidation {
    param($Parsed)

    $descriptor = Get-ObjectPropertyValue -InputObject $Parsed -Name 'JobStoreMigration'
    $jobStore = Read-JsonObject -Path $jobStorePath
    $backupPath = [string](Get-ObjectPropertyValue -InputObject $descriptor -Name 'BackupPath')
    $tempPath = [string](Get-ObjectPropertyValue -InputObject $descriptor -Name 'TempPath')
    [pscustomobject][ordered]@{
        job_store_path = $jobStorePath
        job_store_exists = Test-Path -LiteralPath $jobStorePath -PathType Leaf
        job_store_schema_version = if ($null -ne $jobStore) { [int]$jobStore.version } else { $null }
        job_store_migration_plan_id = if ($null -ne $jobStore -and $null -ne (Get-ObjectPropertyValue -InputObject $jobStore -Name 'migration')) { [string]$jobStore.migration.plan_id } else { $null }
        job_store_migration_plan_version = if ($null -ne $jobStore -and $null -ne (Get-ObjectPropertyValue -InputObject $jobStore -Name 'migration')) { [int]$jobStore.migration.plan_version } else { $null }
        job_count = if ($null -ne $jobStore) { @($jobStore.jobs).Count } else { 0 }
        seeded_job_present = if ($null -ne $jobStore) { [bool](@($jobStore.jobs) | Where-Object { $_.job_id -eq $smokeJobId } | Select-Object -First 1) } else { $false }
        descriptor_mutation_performed = if ($null -ne $descriptor) { [bool]$descriptor.MutationPerformed } else { $false }
        descriptor_source_schema_version = if ($null -ne $descriptor) { [int]$descriptor.SourceSchemaVersion } else { $null }
        descriptor_target_schema_version = if ($null -ne $descriptor) { [int]$descriptor.TargetSchemaVersion } else { $null }
        backup_path = $backupPath
        backup_exists = -not [string]::IsNullOrWhiteSpace($backupPath) -and (Test-Path -LiteralPath $backupPath -PathType Leaf)
        temp_path = $tempPath
        temp_exists_after_apply = -not [string]::IsNullOrWhiteSpace($tempPath) -and (Test-Path -LiteralPath $tempPath -PathType Leaf)
    }
}

function Invoke-PostMigrationApiRead {
    $path = Join-Path $ArtifactRoot 'post-migration-api-read.json'
    $token = Read-ProtectedToken -Path $protectedTokenPath
    $runtimePolicy = Invoke-ApiWithRetry -Method 'GET' -Path '/api/v1/runtime/policy' -Token $token
    $jobs = Invoke-ApiWithRetry -Method 'GET' -Path '/api/v1/jobs' -Token $token
    $job = Invoke-ApiWithRetry -Method 'GET' -Path "/api/v1/jobs/$smokeJobId" -Token $token
    $jobRows = @()
    if ($null -ne $jobs.data -and $null -ne (Get-ObjectPropertyValue -InputObject $jobs.data -Name 'jobs')) {
        $jobRows = @($jobs.data.jobs)
    }
    $seededJobListed = [bool]($jobRows | Where-Object { $_.job_id -eq $smokeJobId } | Select-Object -First 1)
    $ok = [bool](
        $runtimePolicy.ok -and
        $jobs.ok -and
        $job.ok -and
        [string]$job.data.job_id -eq $smokeJobId -and
        $seededJobListed)

    $evidence = [pscustomobject][ordered]@{
        ok = $ok
        runtime_policy_ok = [bool]$runtimePolicy.ok
        jobs_ok = [bool]$jobs.ok
        job_get_ok = [bool]$job.ok
        jobs_count = if ($null -ne $jobs.data) { [int]$jobs.data.count } else { $null }
        seeded_job_listed = $seededJobListed
        seeded_job_get = if ($null -ne $job.data) { $job.data } else { $null }
        token_length = $token.Length
        token_sha256 = ([System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($token))).Replace('-', '').ToLowerInvariant())
    }
    Write-JsonFile -Path $path -Value $evidence
    Add-Step -Name 'post-migration-api-read' -Ok $ok -Path $path
    Assert-True -Condition $ok -Message 'PCV_MIGRATION_SMOKE_API_READ_FAILED|Post-migration API read did not load the migrated job store.'
    $evidence
}

$plannedStepNames = @(
    'preflight',
    'build-current-admin-smoke-msi',
    'install-current-msi',
    'stop-installed-service-for-migration',
    'seed-installed-job-store-v1',
    'config-migration-apply-installed',
    'job-store-migration-apply-installed',
    'start-installed-service-after-migration',
    'post-migration-api-read',
    'final-state'
)

if ($PlanOnly) {
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        plan_only = $true
        actual_execution = 'not-run'
        mutates_host = $false
        host_mutation_performed = $false
        artifact_root = $ArtifactRoot
        version = $Version
        product_root = $ProductRoot
        data_root = $DataRoot
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        command_plan = New-CommandPlan
        steps = @($plannedStepNames | ForEach-Object { New-PlannedStep -Name $_ })
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-JsonFile -Path $progressPath -Value $summary
    Write-Output $ArtifactRoot
    exit 0
}

$bootTimeBefore = $null
$buildOutput = $null
$msiPath = $null
$msiSha256 = $null
$hostMutationPerformed = $false

try {
    $bootTimeBefore = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime

    $preflightPath = Join-Path $ArtifactRoot 'preflight.json'
    $preflight = [pscustomobject][ordered]@{
        admin = Test-IsAdministrator
        artifact_root = $ArtifactRoot
        version = $Version
        build_script = Join-Path $repoRoot 'packaging\windows-desktop-node\installer\build.ps1'
        build_script_exists = Test-Path -LiteralPath (Join-Path $repoRoot 'packaging\windows-desktop-node\installer\build.ps1') -PathType Leaf
        product_root = $ProductRoot
        data_root = $DataRoot
        host_exe = $hostExe
        service_before = Get-ServiceInfo
        boot_time_before = $bootTimeBefore
    }
    $preflightOk = [bool]($preflight.admin -and $preflight.build_script_exists)
    Write-JsonFile -Path $preflightPath -Value $preflight
    Add-Step -Name 'preflight' -Ok $preflightOk -Path $preflightPath
    Assert-True -Condition $preflightOk -Message 'PCV_MIGRATION_SMOKE_PREFLIGHT_FAILED|Admin rights and installer build script are required.'

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
        throw "PCV_MIGRATION_SMOKE_BUILD_FAILED|build.ps1 exited $($buildProcess.exit_code)."
    }
    $buildOutput = $buildProcess.stdout | ConvertFrom-Json
    Write-JsonFile -Path $buildJsonPath -Value $buildOutput
    $msiPath = [string]$buildOutput.msi_path
    $msiSha256 = Get-FileSha256 -Path $msiPath
    Add-Step -Name 'build-current-admin-smoke-msi' -Ok $true -Path $buildJsonPath

    $installPath = Join-Path $ArtifactRoot 'install-current-msi.json'
    Start-Step -Name 'install-current-msi' -Path $installPath
    $installLogRoot = Join-Path $ArtifactRoot 'msi-logs'
    New-Item -ItemType Directory -Path $installLogRoot -Force | Out-Null
    $hostMutationPerformed = $true
    $installResult = Invoke-MsiStep `
        -Name 'install-current-msi' `
        -MsiArguments @('/i', $msiPath) `
        -LogPath (Join-Path $installLogRoot 'install-current-msi.log') `
        -TimeoutSeconds $MsiStepTimeoutSeconds
    $installHealth = $null
    if ($installResult.ok) {
        Assert-True -Condition (Wait-ServiceState -Expected 'Running' -TimeoutSeconds 120) -Message 'Installed service did not reach Running state after MSI install.'
        $installHealth = [pscustomobject][ordered]@{
            service = Get-ServiceInfo
            host_exe_exists = Test-Path -LiteralPath $hostExe -PathType Leaf
            manifest_exists = Test-Path -LiteralPath $manifestPath -PathType Leaf
            token_exists = Test-Path -LiteralPath $protectedTokenPath -PathType Leaf
        }
    }
    Write-JsonFile -Path $installPath -Value ([pscustomobject][ordered]@{
        msi_path = $msiPath
        msi_sha256 = $msiSha256
        result = $installResult
        health = $installHealth
        ok = [bool]($installResult.ok -and $null -ne $installHealth -and [bool]$installHealth.host_exe_exists -and [bool]$installHealth.manifest_exists -and [bool]$installHealth.token_exists)
    })
    $installOk = [bool]($installResult.ok -and $null -ne $installHealth -and [bool]$installHealth.host_exe_exists -and [bool]$installHealth.manifest_exists -and [bool]$installHealth.token_exists)
    Add-Step -Name 'install-current-msi' -Ok $installOk -Path $installPath
    Assert-True -Condition $installOk -Message 'PCV_MIGRATION_SMOKE_INSTALL_FAILED|MSI install did not produce a healthy installed product.'

    Invoke-ServiceAction `
        -StepName 'stop-installed-service-for-migration' `
        -Action 'stop' `
        -Validate {
            param($Process, $Parsed, $Validation)
            $Process.ok -and $null -ne $Parsed -and [bool]$Parsed.Ok -and $null -ne $Parsed.Service -and [string]$Parsed.Service.Status -eq 'stopped'
        } | Out-Null
    Assert-True -Condition (Wait-ServiceState -Expected 'Stopped' -TimeoutSeconds 90) -Message 'Installed service did not remain stopped for migration.'

    $seedPath = Join-Path $ArtifactRoot 'seed-installed-job-store-v1.json'
    $seed = Write-SmokeJobStoreV1
    Write-JsonFile -Path $seedPath -Value $seed
    Add-Step -Name 'seed-installed-job-store-v1' -Ok ([bool]$seed.ok) -Path $seedPath
    Assert-True -Condition ([bool]$seed.ok -and [bool]$seed.service_stopped) -Message 'PCV_MIGRATION_SMOKE_JOB_STORE_SEED_FAILED|Job store v1 fixture setup failed.'

    Invoke-ServiceAction `
        -StepName 'config-migration-apply-installed' `
        -Action 'config-migration-apply' `
        -ExtraArguments @('--migration-plan-id', 'product-config-v1-to-v2', '--migration-plan-version', '1') `
        -BuildValidation {
            param($Process, $Parsed)
            New-ConfigMigrationValidation -Parsed $Parsed
        } `
        -Validate {
            param($Process, $Parsed, $Validation)
            $Process.ok -and
                $null -ne $Parsed -and
                [bool]$Parsed.Ok -and
                $null -ne $Validation -and
                [bool]$Validation.descriptor_mutation_performed -and
                [int]$Validation.descriptor_source_schema_version -eq 1 -and
                [int]$Validation.descriptor_target_schema_version -eq 2 -and
                [int]$Validation.manifest_schema_version -eq 2 -and
                [string]$Validation.manifest_migration_plan_id -eq 'product-config-v1-to-v2' -and
                [int]$Validation.manifest_migration_plan_version -eq 1 -and
                [bool]$Validation.backup_exists -and
                -not [bool]$Validation.temp_exists_after_apply
        } | Out-Null

    Invoke-ServiceAction `
        -StepName 'job-store-migration-apply-installed' `
        -Action 'job-store-migration-apply' `
        -ExtraArguments @('--migration-plan-id', 'job-store-v1-to-v2', '--migration-plan-version', '1') `
        -BuildValidation {
            param($Process, $Parsed)
            New-JobStoreMigrationValidation -Parsed $Parsed
        } `
        -Validate {
            param($Process, $Parsed, $Validation)
            $Process.ok -and
                $null -ne $Parsed -and
                [bool]$Parsed.Ok -and
                $null -ne $Validation -and
                [bool]$Validation.descriptor_mutation_performed -and
                [int]$Validation.descriptor_source_schema_version -eq 1 -and
                [int]$Validation.descriptor_target_schema_version -eq 2 -and
                [int]$Validation.job_store_schema_version -eq 2 -and
                [string]$Validation.job_store_migration_plan_id -eq 'job-store-v1-to-v2' -and
                [int]$Validation.job_store_migration_plan_version -eq 1 -and
                [bool]$Validation.seeded_job_present -and
                [bool]$Validation.backup_exists -and
                -not [bool]$Validation.temp_exists_after_apply
        } | Out-Null

    Invoke-ServiceAction `
        -StepName 'start-installed-service-after-migration' `
        -Action 'start' `
        -Validate {
            param($Process, $Parsed, $Validation)
            $Process.ok -and $null -ne $Parsed -and [bool]$Parsed.Ok -and $null -ne $Parsed.Service -and [string]$Parsed.Service.Status -eq 'running'
        } | Out-Null
    Assert-True -Condition (Wait-ServiceState -Expected 'Running' -TimeoutSeconds 120) -Message 'Installed service did not reach Running after migration.'

    $postApi = Invoke-PostMigrationApiRead

    $bootTimeAfter = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $finalStatePath = Join-Path $ArtifactRoot 'final-state.json'
    $finalManifest = Read-JsonObject -Path $manifestPath
    $finalJobStore = Read-JsonObject -Path $jobStorePath
    $finalState = [pscustomobject][ordered]@{
        ok = $true
        service = Get-ServiceInfo
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = $bootTimeBefore -eq $bootTimeAfter
        manifest_schema_version = if ($null -ne $finalManifest) { [int]$finalManifest.schema_version } else { $null }
        job_store_schema_version = if ($null -ne $finalJobStore) { [int]$finalJobStore.version } else { $null }
        post_migration_api_read_ok = [bool]$postApi.ok
    }
    Write-JsonFile -Path $finalStatePath -Value $finalState
    Add-Step -Name 'final-state' -Ok ([bool]$finalState.ok) -Path $finalStatePath

    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        plan_only = $false
        actual_execution = 'completed'
        mutates_host = $true
        host_mutation_performed = $true
        artifact_root = $ArtifactRoot
        version = $Version
        product_root = $ProductRoot
        data_root = $DataRoot
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        msi_path = $msiPath
        msi_sha256 = $msiSha256
        provenance_path = if ($null -ne $buildOutput) { [string]$buildOutput.provenance_path } else { $null }
        provenance_commit = if ($null -ne $buildOutput -and $null -ne $buildOutput.provenance) { [string]$buildOutput.provenance.git_commit } else { $null }
        signing_mode = if ($null -ne $buildOutput -and $null -ne $buildOutput.provenance) { [string]$buildOutput.provenance.signing_mode } else { $null }
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = $bootTimeBefore -eq $bootTimeAfter
        final_service = Get-ServiceInfo
        final_manifest_schema_version = if ($null -ne $finalManifest) { [int]$finalManifest.schema_version } else { $null }
        final_job_store_schema_version = if ($null -ne $finalJobStore) { [int]$finalJobStore.version } else { $null }
        command_plan = New-CommandPlan
        steps = $summarySteps.ToArray()
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Output $ArtifactRoot
    exit 0
}
catch {
    $bootTimeAfter = try { (Get-CimInstance Win32_OperatingSystem).LastBootUpTime } catch { $null }
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $false
        plan_only = $false
        actual_execution = 'failed'
        mutates_host = $true
        host_mutation_performed = $hostMutationPerformed
        artifact_root = $ArtifactRoot
        version = $Version
        product_root = $ProductRoot
        data_root = $DataRoot
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        error = [string]$_
        msi_path = $msiPath
        msi_sha256 = $msiSha256
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = if ($null -ne $bootTimeBefore -and $null -ne $bootTimeAfter) { $bootTimeBefore -eq $bootTimeAfter } else { $false }
        final_service = Get-ServiceInfo
        command_plan = New-CommandPlan
        steps = $summarySteps.ToArray()
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Error $_
    exit 1
}

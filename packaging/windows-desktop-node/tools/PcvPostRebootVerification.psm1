Set-StrictMode -Version Latest

function Resolve-PcvPostRebootRepoRoot {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $resolved = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
    $required = @(
        'AGENTS.md',
        'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1',
        'src/DesktopNode.Host/DesktopNode.Host.csproj'
    )

    foreach ($relative in $required) {
        $path = Join-Path $resolved $relative
        if (-not (Test-Path -LiteralPath $path)) {
            throw "PCV_POST_REBOOT_REPO_BOUNDARY|Repository boundary check failed.|Missing '$relative' under '$resolved'."
        }
    }

    $resolved
}

function Resolve-PcvPostRebootAbsolutePath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Test-PcvPostRebootSensitiveKey {
    param([AllowNull()][string]$Key)

    if ([string]::IsNullOrWhiteSpace($Key)) {
        return $false
    }

    $Key -match '(?i)(authorization|token|api_token|api_token_file|api_token_protected_file|protected_token|token_sha256|access_token|password|secret|private_key|pfx)'
}

function Get-PcvPostRebootBootTimeUtc {
    try {
        return (Get-CimInstance Win32_OperatingSystem -ErrorAction Stop).LastBootUpTime.ToUniversalTime().ToString('o')
    }
    catch {
        return ''
    }
}

function ConvertTo-PcvPostRebootRedactedText {
    param(
        [AllowNull()][string]$Text,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $Text) {
        return $null
    }

    $redacted = [string]$Text
    $redacted = [regex]::Replace(
        $redacted,
        '(?im)^(\s*Authorization\s*:\s*)([^\r\n]+)',
        {
            param($Match)
            $value = [string]$Match.Groups[2].Value
            if ($value -match '^(?i)Bearer\s+') {
                return $Match.Value
            }
            return $Match.Groups[1].Value + '[REDACTED]'
        }
    )
    $redacted = $redacted -replace '(?i)(Bearer)\s+[A-Za-z0-9._~+/=-]+', '$1 [REDACTED]'
    $redacted = [regex]::Replace(
        $redacted,
        '(?i)(?<!\S)(-(?:ApiToken|ApiTokenFile|ApiTokenProtectedFile|ProtectedToken|Token|Password|Secret|PrivateKey|Pfx|CertificatePath|CertificateThumbprint)[ \t]+)(?:"[^"\r\n]*"|''[^''\r\n]*''|[^ \t\r\n]+)',
        {
            param($Match)
            $Match.Groups[1].Value + '[REDACTED]'
        }
    )
    $redacted = [regex]::Replace(
        $redacted,
        '(?i)("(?:authorization|token|api_token|api_token_file|api_token_protected_file|protected_token|token_sha256|access_token|password|secret|private_key|pfx)"\s*:\s*)"[^"]*"',
        {
            param($Match)
            $Match.Groups[1].Value + '"[REDACTED]"'
        }
    )
    $redacted = [regex]::Replace(
        $redacted,
        '(?i)(\b(?:token|api_token|api_token_file|api_token_protected_file|protected_token|token_sha256|access_token|password|secret|private_key|pfx)\b\s*[:=]\s*)(?:"[^"]*"|''[^'']*''|[^\s,;}\]]+)',
        {
            param($Match)
            $Match.Groups[1].Value + '[REDACTED]'
        }
    )

    if ($null -ne $PathRedactions) {
        $paths = @($PathRedactions.Keys) | Sort-Object { ([string]$_).Length } -Descending
        foreach ($path in $paths) {
            if (-not [string]::IsNullOrEmpty([string]$path)) {
                $redacted = $redacted.Replace([string]$path, [string]$PathRedactions[$path])
            }
        }
    }

    $redacted
}

function ConvertTo-PcvPostRebootRedactedObject {
    param(
        [AllowNull()]$InputObject,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [string]) {
        return ConvertTo-PcvPostRebootRedactedText -Text $InputObject -PathRedactions $PathRedactions
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            if (Test-PcvPostRebootSensitiveKey -Key ([string]$key)) {
                $out[$key] = '[REDACTED]'
            }
            else {
                $out[$key] = ConvertTo-PcvPostRebootRedactedObject -InputObject $InputObject[$key] -PathRedactions $PathRedactions
            }
        }
        return $out
    }

    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            if (Test-PcvPostRebootSensitiveKey -Key $property.Name) {
                $out[$property.Name] = '[REDACTED]'
            }
            else {
                $out[$property.Name] = ConvertTo-PcvPostRebootRedactedObject -InputObject $property.Value -PathRedactions $PathRedactions
            }
        }
        return $out
    }

    if ($InputObject -is [System.Collections.IEnumerable]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvPostRebootRedactedObject -InputObject $item -PathRedactions $PathRedactions
        }
        return $items
    }

    $InputObject
}

function New-PcvPostRebootCommand {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 1800,
        [bool]$Required = $true,
        [bool]$AllowFailure = $false,
        [string]$SummaryPattern = ''
    )

    [ordered]@{
        id = $Id
        working_directory = $WorkingDirectory
        file_name = $FileName
        arguments = @($Arguments)
        timeout_seconds = $TimeoutSeconds
        required = $Required
        allow_failure = $AllowFailure
        summary_pattern = $SummaryPattern
    }
}

function Get-PcvPostRebootActiveProfiles {
    @('ProductStatus', 'PackagingRegression')
}

function Assert-PcvPostRebootActiveProfile {
    param([Parameter(Mandatory)][string]$Profile)

    if ($Profile -eq 'HyperVNonIntegration') {
        throw "PCV_POST_REBOOT_PROFILE_RETIRED|Post-reboot profile '$Profile' is no longer an active product verification profile.|Run Hyper-V component/archive baseline verification outside the post-reboot product profile."
    }

    if ($Profile -notin (Get-PcvPostRebootActiveProfiles)) {
        $allowed = (Get-PcvPostRebootActiveProfiles) -join ','
        throw "PCV_POST_REBOOT_PROFILE_UNKNOWN|Unknown post-reboot profile '$Profile'.|Allowed active profiles: $allowed."
    }
}

function New-PcvPostRebootCommandProfile {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    Assert-PcvPostRebootActiveProfile -Profile $Profile
    $root = Resolve-PcvPostRebootRepoRoot -RepoRoot $RepoRoot
    $commands = New-Object System.Collections.Generic.List[object]

    switch ($Profile) {
        'ProductStatus' {
            $commands.Add((New-PcvPostRebootCommand `
                -Id 'product-status' `
                -WorkingDirectory $root `
                -FileName 'pwsh' `
                -Arguments @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1', '-Action', 'Status') `
                -TimeoutSeconds 300))
            $commands.Add((New-PcvPostRebootCommand `
                -Id 'product-collect-diagnostics' `
                -WorkingDirectory $root `
                -FileName 'pwsh' `
                -Arguments @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1', '-Action', 'CollectDiagnostics') `
                -TimeoutSeconds 600))
        }
        'PackagingRegression' {
            $commands.Add((New-PcvPostRebootCommand `
                -Id 'packaging-product-tests' `
                -WorkingDirectory $root `
                -FileName 'pwsh' `
                -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed")))
            $commands.Add((New-PcvPostRebootCommand `
                -Id 'packaging-installer-tests' `
                -WorkingDirectory $root `
                -FileName 'pwsh' `
                -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed")))
            $commands.Add((New-PcvPostRebootCommand `
                -Id 'git-diff-check' `
                -WorkingDirectory $root `
                -FileName 'git' `
                -Arguments @('diff', '--check') `
                -TimeoutSeconds 300))
        }
    }

    [ordered]@{
        profile = $Profile
        repo_root = $root
        commands = @($commands.ToArray())
    }
}

function Copy-PcvPostRebootContinuationCommand {
    param(
        [Parameter(Mandatory)]$Command,
        [Parameter(Mandatory)][string]$Profile
    )

    [ordered]@{
        id = "continuation-$Profile-$($Command.id)"
        working_directory = [string]$Command.working_directory
        file_name = [string]$Command.file_name
        arguments = @([string[]]$Command.arguments)
        timeout_seconds = [int]$Command.timeout_seconds
        required = [bool]$Command.required
        allow_failure = [bool]$Command.allow_failure
        summary_pattern = [string]$Command.summary_pattern
    }
}

function New-PcvPostRebootContinuationPlan {
    param(
        [string[]]$Profiles = @(),
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $commands = New-Object System.Collections.Generic.List[object]
    $selectedProfiles = @($Profiles | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })

    foreach ($profile in $selectedProfiles) {
        $profileData = New-PcvPostRebootCommandProfile -Profile $profile -RepoRoot $RepoRoot
        foreach ($command in @($profileData.commands)) {
            $commands.Add((Copy-PcvPostRebootContinuationCommand -Command $command -Profile $profile))
        }
    }

    [ordered]@{
        run_on_success = $true
        profiles = @($selectedProfiles)
        commands = @($commands.ToArray())
    }
}

function New-PcvPostRebootVerificationState {
    param(
        [Parameter(Mandatory)][string]$PhaseId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$EvidenceDir,
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$TaskName,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup',
        [switch]$RequiresUserProfile,
        [switch]$RequiresNetworkDrive,
        [switch]$RequiresSigningMaterial,
        [string[]]$ContinuationProfiles = @()
    )

    if ($PrincipalMode -eq 'LocalSystemAtStartup' -and ($RequiresUserProfile -or $RequiresNetworkDrive -or $RequiresSigningMaterial)) {
        throw 'PCV_POST_REBOOT_PRINCIPAL_NOT_ALLOWED|LocalSystemAtStartup cannot use user profile, mapped network drive, or signing material resources.|Use CurrentUserAtLogOn with explicit opt-in.'
    }

    $profileData = New-PcvPostRebootCommandProfile -Profile $Profile -RepoRoot $RepoRoot
    $evidenceRoot = Resolve-PcvPostRebootAbsolutePath -Path $EvidenceDir
    $continuation = New-PcvPostRebootContinuationPlan -Profiles $ContinuationProfiles -RepoRoot $profileData.repo_root

    [ordered]@{
        schema_version = 1
        phase_id = $PhaseId
        task_name = $TaskName
        repo_root = $profileData.repo_root
        evidence_dir = $evidenceRoot
        created_at_utc = (Get-Date).ToUniversalTime().ToString('o')
        created_by_user = [Environment]::UserName
        machine_name = [Environment]::MachineName
        pre_reboot_boot_time_utc = Get-PcvPostRebootBootTimeUtc
        profile = $Profile
        commands = @($profileData.commands)
        continuation = $continuation
        redaction = [ordered]@{
            version = 1
            paths = [ordered]@{
                repo_root = '[REPO_ROOT]'
                evidence_dir = '[EVIDENCE_ROOT]'
            }
        }
        cleanup = [ordered]@{
            unregister_task = $true
        }
        principal = [ordered]@{
            mode = $PrincipalMode
            requires_user_profile = [bool]$RequiresUserProfile
            requires_network_drive = [bool]$RequiresNetworkDrive
            requires_signing_material = [bool]$RequiresSigningMaterial
        }
    }
}

function Write-PcvPostRebootJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$InputObject
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $InputObject | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function ConvertTo-PcvPostRebootArgumentText {
    param([Parameter(Mandatory)][string[]]$Arguments)

    (($Arguments | ForEach-Object {
        if ($_ -match '\s') {
            '"' + ($_ -replace '"', '\"') + '"'
        }
        else {
            $_
        }
    }) -join ' ')
}

function New-PcvPostRebootScheduledTaskPlan {
    param(
        [Parameter(Mandatory)][string]$TaskName,
        [Parameter(Mandatory)][string]$StateFile,
        [Parameter(Mandatory)][string]$RunnerScript,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup'
    )

    $runnerPath = (Resolve-Path -LiteralPath $RunnerScript -ErrorAction Stop).Path
    $statePath = Resolve-PcvPostRebootAbsolutePath -Path $StateFile
    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $runnerPath,
        '-StateFile',
        $statePath
    )

    $trigger = 'AtLogOn'
    $principal = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    if ($PrincipalMode -eq 'LocalSystemAtStartup') {
        $trigger = 'AtStartup'
        $principal = 'SYSTEM'
    }

    [ordered]@{
        task_name = $TaskName
        trigger = $trigger
        principal_mode = $PrincipalMode
        principal_user_id = $principal
        run_level = 'Highest'
        action_file_name = 'pwsh.exe'
        action_arguments = ConvertTo-PcvPostRebootArgumentText -Arguments $arguments
        state_file = $statePath
        runner_script = $runnerPath
    }
}

function Register-PcvPostRebootScheduledTask {
    param([Parameter(Mandatory)]$TaskPlan)

    $action = New-ScheduledTaskAction -Execute $TaskPlan.action_file_name -Argument $TaskPlan.action_arguments
    $trigger = if ($TaskPlan.trigger -eq 'AtStartup') {
        New-ScheduledTaskTrigger -AtStartup
    }
    else {
        New-ScheduledTaskTrigger -AtLogOn
    }
    $principal = New-ScheduledTaskPrincipal -UserId $TaskPlan.principal_user_id -RunLevel Highest
    Register-ScheduledTask -TaskName $TaskPlan.task_name -Action $action -Trigger $trigger -Principal $principal -Force | Out-Null

    [ordered]@{
        ok = $true
        task_name = $TaskPlan.task_name
    }
}

function Initialize-PcvPostRebootVerification {
    param(
        [Parameter(Mandatory)][string]$PhaseId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$EvidenceDir,
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$TaskName,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup',
        [switch]$RequiresUserProfile,
        [switch]$RequiresNetworkDrive,
        [switch]$RequiresSigningMaterial,
        [string[]]$ContinuationProfiles = @(),
        [switch]$DryRun,
        [scriptblock]$RegisterTask
    )

    $state = New-PcvPostRebootVerificationState `
        -PhaseId $PhaseId `
        -RepoRoot $RepoRoot `
        -EvidenceDir $EvidenceDir `
        -Profile $Profile `
        -TaskName $TaskName `
        -PrincipalMode $PrincipalMode `
        -RequiresUserProfile:$RequiresUserProfile `
        -RequiresNetworkDrive:$RequiresNetworkDrive `
        -RequiresSigningMaterial:$RequiresSigningMaterial `
        -ContinuationProfiles $ContinuationProfiles
    $evidenceRoot = [string]$state.evidence_dir
    New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
    $stateFile = Join-Path $evidenceRoot 'post-reboot-state.json'
    Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

    $runner = Join-Path $state.repo_root 'packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1'
    $taskPlan = New-PcvPostRebootScheduledTaskPlan -TaskName $TaskName -StateFile $stateFile -RunnerScript $runner -PrincipalMode $PrincipalMode

    $registration = $null
    if (-not $DryRun) {
        if ($null -eq $RegisterTask) {
            $RegisterTask = { param($TaskPlan) Register-PcvPostRebootScheduledTask -TaskPlan $TaskPlan }
        }
        $registration = & $RegisterTask -TaskPlan $taskPlan
    }

    [ordered]@{
        ok = $true
        dry_run = [bool]$DryRun
        state_file = $stateFile
        task_plan = $taskPlan
        registration = $registration
    }
}

function Read-PcvPostRebootState {
    param([Parameter(Mandatory)][string]$StateFile)

    if (-not (Test-Path -LiteralPath $StateFile -PathType Leaf)) {
        throw "PCV_POST_REBOOT_STATE_NOT_FOUND|Post-reboot state file was not found.|Path: '$StateFile'."
    }

    Get-Content -LiteralPath $StateFile -Raw | ConvertFrom-Json
}

function Invoke-PcvPostRebootNativeProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [int]$TimeoutSeconds = 900
    )

    $started = Get-Date
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $FileName
    $process.StartInfo.WorkingDirectory = $WorkingDirectory
    foreach ($argument in $Arguments) {
        [void]$process.StartInfo.ArgumentList.Add($argument)
    }
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true

    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
    if ($timedOut) {
        try {
            $process.Kill($true)
        }
        catch {
        }
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $finished = Get-Date

    [ordered]@{
        exit_code = $(if ($timedOut) { -1 } else { $process.ExitCode })
        stdout = $stdout
        stderr = $stderr
        timed_out = $timedOut
        duration_ms = [int]($finished - $started).TotalMilliseconds
    }
}

function Write-PcvPostRebootTextFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Text
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -LiteralPath $Path -Value ([string]$Text) -Encoding UTF8
}

function New-PcvPostRebootMarkdownSummary {
    param([Parameter(Mandatory)]$Result)

    $lines = @()
    $lines += '### Post-reboot verification'
    $lines += ''
    $lines += ('- phase: `{0}`' -f $Result.phase_id)
    $lines += ('- ok: `{0}`' -f $Result.ok)
    $lines += ('- started_at_utc: `{0}`' -f $Result.started_at_utc)
    $lines += ('- finished_at_utc: `{0}`' -f $Result.finished_at_utc)
    $lines += ('- windows_boot_time_utc: `{0}`' -f $Result.windows_boot_time_utc)
    $lines += ('- git_commit: `{0}`' -f $Result.git_commit)
    $lines += ('- git_status_summary: `{0}`' -f $Result.git_status_summary)
    foreach ($command in @($Result.commands)) {
        $lines += ('- command `{0}`: exit `{1}`, ok `{2}`, timed_out `{3}`, summary `{4}`' -f $command.id, $command.exit_code, $command.ok, $command.timed_out, $command.summary)
    }
    if ($null -ne $Result.continuation) {
        $lines += ('- continuation: skipped `{0}`, ok `{1}`, reason `{2}`' -f $Result.continuation.skipped, $Result.continuation.ok, $Result.continuation.reason)
        foreach ($command in @($Result.continuation.commands)) {
            $lines += ('- continuation command `{0}`: exit `{1}`, ok `{2}`, timed_out `{3}`, summary `{4}`' -f $command.id, $command.exit_code, $command.ok, $command.timed_out, $command.summary)
        }
    }

    $lines -join [Environment]::NewLine
}

function Invoke-PcvPostRebootVerification {
    param(
        [Parameter(Mandatory)][string]$StateFile,
        [scriptblock]$InvokeProcess,
        [scriptblock]$GetBootTimeUtc,
        [scriptblock]$GetGitCommit,
        [scriptblock]$GetGitStatusSummary,
        [scriptblock]$UnregisterTask
    )

    $state = Read-PcvPostRebootState -StateFile $StateFile
    $repo = Resolve-PcvPostRebootRepoRoot -RepoRoot ([string]$state.repo_root)
    $evidenceDir = Resolve-PcvPostRebootAbsolutePath -Path ([string]$state.evidence_dir)
    New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null

    if ($null -eq $UnregisterTask) {
        $UnregisterTask = {
            param([string]$TaskName)
            Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
            [ordered]@{ ok = $true }
        }
    }

    $pathRedactions = @{}
    $pathRedactions[$repo] = '[REPO_ROOT]'
    $pathRedactions[$evidenceDir] = '[EVIDENCE_ROOT]'

    $completeMarker = Join-Path $evidenceDir 'post-reboot-complete.json'
    if (Test-Path -LiteralPath $completeMarker -PathType Leaf) {
        $existing = Get-Content -LiteralPath $completeMarker -Raw | ConvertFrom-Json
        $cleanup = [ordered]@{
            ok = $true
            skipped = $true
            reason = 'already-completed-cleanup-disabled'
        }
        if ($state.cleanup.unregister_task) {
            try {
                $cleanup = & $UnregisterTask -TaskName ([string]$state.task_name)
            }
            catch {
                $cleanup = [ordered]@{
                    ok = $false
                    action = 'unregister-task'
                    error = $_.Exception.Message
                }
            }
        }
        $cleanup = ConvertTo-PcvPostRebootRedactedObject -InputObject $cleanup -PathRedactions $pathRedactions
        return [ordered]@{
            ok = [bool]$cleanup.ok
            already_completed = $true
            completed_at_utc = $existing.completed_at_utc
            cleanup = $cleanup
        }
    }

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            Invoke-PcvPostRebootNativeProcess -FileName $FileName -Arguments $Arguments -WorkingDirectory $WorkingDirectory -TimeoutSeconds $TimeoutSeconds
        }
    }
    if ($null -eq $GetBootTimeUtc) {
        $GetBootTimeUtc = { Get-PcvPostRebootBootTimeUtc }
    }
    if ($null -eq $GetGitCommit) {
        $GetGitCommit = { (& git -C $repo rev-parse --short HEAD 2>$null) -join "`n" }
    }
    if ($null -eq $GetGitStatusSummary) {
        $GetGitStatusSummary = { (& git -C $repo status --short 2>$null) -join '; ' }
    }
    $started = [DateTime]::UtcNow
    $commandResults = @()
    $overallOk = $true

    $invokeStateCommand = {
        param([Parameter(Mandatory)]$Command)

        $processResult = & $InvokeProcess `
            -FileName ([string]$Command.file_name) `
            -Arguments ([string[]]$Command.arguments) `
            -WorkingDirectory ([string]$Command.working_directory) `
            -TimeoutSeconds ([int]$Command.timeout_seconds)

        $stdoutPath = Join-Path $evidenceDir "post-reboot-stdout-$($Command.id).log"
        $stderrPath = Join-Path $evidenceDir "post-reboot-stderr-$($Command.id).log"
        Write-PcvPostRebootTextFile -Path $stdoutPath -Text (ConvertTo-PcvPostRebootRedactedText -Text ([string]$processResult.stdout) -PathRedactions $pathRedactions)
        Write-PcvPostRebootTextFile -Path $stderrPath -Text (ConvertTo-PcvPostRebootRedactedText -Text ([string]$processResult.stderr) -PathRedactions $pathRedactions)

        $commandOk = ([int]$processResult.exit_code -eq 0 -and -not [bool]$processResult.timed_out)

        $summary = ''
        if (-not [string]::IsNullOrWhiteSpace([string]$Command.summary_pattern)) {
            $summary = [string]$Command.summary_pattern
        }
        elseif (-not [string]::IsNullOrWhiteSpace([string]$processResult.stderr)) {
            $summary = ([string]$processResult.stderr).Trim()
        }
        elseif (-not [string]::IsNullOrWhiteSpace([string]$processResult.stdout)) {
            $summary = ([string]$processResult.stdout).Trim()
        }
        $summary = ConvertTo-PcvPostRebootRedactedText -Text $summary -PathRedactions $pathRedactions

        [ordered]@{
            result = [ordered]@{
                id = [string]$Command.id
                exit_code = [int]$processResult.exit_code
                duration_ms = [int]$processResult.duration_ms
                timed_out = [bool]$processResult.timed_out
                stdout_artifact = Split-Path -Leaf $stdoutPath
                stderr_artifact = Split-Path -Leaf $stderrPath
                summary = $summary
                ok = $commandOk
            }
            failed_required = (-not $commandOk -and [bool]$Command.required -and -not [bool]$Command.allow_failure)
        }
    }

    foreach ($command in @($state.commands)) {
        $commandOutcome = & $invokeStateCommand -Command $command
        $commandResults += $commandOutcome.result
        if ([bool]$commandOutcome.failed_required) {
            $overallOk = $false
        }
    }

    $verificationOk = $overallOk
    $continuationResult = [ordered]@{
        skipped = $true
        reason = 'not-configured'
        ok = $true
        profiles = @()
        commands = @()
    }
    $continuationCommands = @()
    $continuationProfiles = @()
    if (($state.PSObject.Properties.Name -contains 'continuation') -and $null -ne $state.continuation) {
        $continuationCommands = @($state.continuation.commands)
        $continuationProfiles = @($state.continuation.profiles)
    }

    if ($continuationCommands.Count -gt 0) {
        $continuationResults = @()
        $continuationOk = $true
        if ($verificationOk) {
            foreach ($command in $continuationCommands) {
                $commandOutcome = & $invokeStateCommand -Command $command
                $continuationResults += $commandOutcome.result
                if ([bool]$commandOutcome.failed_required) {
                    $continuationOk = $false
                    $overallOk = $false
                }
            }
            $continuationResult = [ordered]@{
                skipped = $false
                reason = ''
                ok = $continuationOk
                profiles = @($continuationProfiles)
                commands = @($continuationResults)
            }
        }
        else {
            $continuationResult = [ordered]@{
                skipped = $true
                reason = 'verification-failed'
                ok = $false
                profiles = @($continuationProfiles)
                commands = @()
            }
        }
    }

    $cleanup = [ordered]@{
        ok = $true
        skipped = $true
        reason = 'cleanup-pending'
    }
    $cleanup = ConvertTo-PcvPostRebootRedactedObject -InputObject $cleanup -PathRedactions $pathRedactions
    $result = [ordered]@{
        schema_version = 1
        phase_id = [string]$state.phase_id
        task_name = [string]$state.task_name
        started_at_utc = $started.ToString('o')
        finished_at_utc = [DateTime]::UtcNow.ToString('o')
        ok = $overallOk
        windows_boot_time_utc = [string](& $GetBootTimeUtc)
        powershell_version = $PSVersionTable.PSVersion.ToString()
        git_commit = [string](& $GetGitCommit)
        git_status_summary = ConvertTo-PcvPostRebootRedactedText -Text ([string](& $GetGitStatusSummary)) -PathRedactions $pathRedactions
        commands = @($commandResults)
        continuation = $continuationResult
        cleanup = $cleanup
    }

    Write-PcvPostRebootJsonFile -Path (Join-Path $evidenceDir 'post-reboot-result.json') -InputObject $result
    Write-PcvPostRebootTextFile -Path (Join-Path $evidenceDir 'post-reboot-summary.md') -Text (New-PcvPostRebootMarkdownSummary -Result $result)
    Write-PcvPostRebootJsonFile -Path $completeMarker -InputObject ([ordered]@{
            completed_at_utc = [DateTime]::UtcNow.ToString('o')
            cleanup_completed = $false
            cleanup_ok = $null
        })

    $cleanup = [ordered]@{
        ok = $true
        skipped = $true
        reason = 'cleanup-disabled'
    }
    if ($state.cleanup.unregister_task) {
        try {
            $cleanup = & $UnregisterTask -TaskName ([string]$state.task_name)
        }
        catch {
            $overallOk = $false
            $cleanup = [ordered]@{
                ok = $false
                action = 'unregister-task'
                error = $_.Exception.Message
            }
        }
    }
    $cleanup = ConvertTo-PcvPostRebootRedactedObject -InputObject $cleanup -PathRedactions $pathRedactions
    $result['finished_at_utc'] = [DateTime]::UtcNow.ToString('o')
    $result['ok'] = $overallOk
    $result['cleanup'] = $cleanup

    Write-PcvPostRebootJsonFile -Path (Join-Path $evidenceDir 'post-reboot-result.json') -InputObject $result
    Write-PcvPostRebootTextFile -Path (Join-Path $evidenceDir 'post-reboot-summary.md') -Text (New-PcvPostRebootMarkdownSummary -Result $result)
    Write-PcvPostRebootJsonFile -Path $completeMarker -InputObject ([ordered]@{
            completed_at_utc = [DateTime]::UtcNow.ToString('o')
            cleanup_completed = $true
            cleanup_ok = [bool]$cleanup.ok
        })

    $result
}

Export-ModuleMember -Function `
    ConvertTo-PcvPostRebootRedactedObject, `
    ConvertTo-PcvPostRebootRedactedText, `
    Get-PcvPostRebootBootTimeUtc, `
    Initialize-PcvPostRebootVerification, `
    Invoke-PcvPostRebootNativeProcess, `
    Invoke-PcvPostRebootVerification, `
    New-PcvPostRebootMarkdownSummary, `
    New-PcvPostRebootCommandProfile, `
    New-PcvPostRebootScheduledTaskPlan, `
    New-PcvPostRebootVerificationState, `
    Read-PcvPostRebootState, `
    Register-PcvPostRebootScheduledTask, `
    Resolve-PcvPostRebootAbsolutePath, `
    Resolve-PcvPostRebootRepoRoot, `
    Test-PcvPostRebootSensitiveKey, `
    Write-PcvPostRebootJsonFile, `
    Write-PcvPostRebootTextFile

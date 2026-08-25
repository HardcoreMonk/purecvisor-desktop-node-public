[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("internal-clean-host-install-update-rollback-smoke-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$BaseVhdPath = 'D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\image-cache\windows-server-2022-eval-vhd\20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd',
    [string]$BaselineMsiPath = 'D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\internal-enterprise-requiresigned-rc-msi-20260507-0387\PureCVisorDesktopNode-0.38.7-rc.1-windows-x64.msi',
    [string]$UpdatePackagePath = 'D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\msi-update-package-20260509-0391\PureCVisorDesktopNode-0.39.1-admin-smoke-update.zip',
    [string]$InternalRootCertificatePath = '',
    [string]$VmName = ("pcv-cleanhost-" + (Get-Date -Format 'yyyyMMdd-HHmmss')),
    [string]$VmRoot = '',
    [string]$VMSwitchName = 'Default Switch',
    [ValidateSet(1, 2)]
    [int]$VmGeneration = 1,
    [string]$GuestUser = 'Administrator',
    [string]$GuestPassword = 'PcvCleanHost!2026',
    [bool]$InjectUnattend = $true,
    [string]$BaselineVersion = '0.38.7-rc.1',
    [string]$TargetVersion = '0.39.1-admin-smoke',
    [string]$UpdateChannel = 'admin-smoke',
    [string]$TargetSigningMode = 'RequireSigned',
    [int]$BootTimeoutSeconds = 900,
    [switch]$InstallWindowsUpdates,
    [string]$WindowsUpdateTitlePattern = '^20\d{2}-\d{2} Cumulative Update for Microsoft server operating system version 21H2',
    [int]$WindowsUpdateTimeoutSeconds = 7200,
    [int]$WindowsUpdateRebootTimeoutSeconds = 1800,
    [int]$WindowsUpdateNoContactRecoverySeconds = 900,
    [switch]$DisableWindowsUpdateNoContactRecovery,
    [int]$ServiceTimeoutSeconds = 180,
    [switch]$RemoveVmOnSuccess,
    [switch]$RemoveVmOnFailure
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-PcvJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-PcvSha256 {
    param([Parameter(Mandatory)][string]$Path)

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Resolve-PcvFilePath {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required file is missing: $Path"
    }

    (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
}

function Test-PcvChildPath {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Parent
    )

    $trimChars = [char[]]@(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    )
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd($trimChars)
    $fullParent = [System.IO.Path]::GetFullPath($Parent).TrimEnd($trimChars)
    $parentWithSeparator = $fullParent + [System.IO.Path]::DirectorySeparatorChar

    $fullPath.Equals($fullParent, [System.StringComparison]::OrdinalIgnoreCase) -or
        $fullPath.StartsWith($parentWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)
}

function New-PcvCredential {
    param(
        [Parameter(Mandatory)][string]$UserName,
        [Parameter(Mandatory)][string]$Password
    )

    $secure = ConvertTo-SecureString -String $Password -AsPlainText -Force
    [System.Management.Automation.PSCredential]::new($UserName, $secure)
}

function Get-PcvVmRecoverySnapshot {
    param([Parameter(Mandatory)][string]$Name)

    $vm = Get-VM -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $vm) {
        return [ordered]@{
            exists = $false
            name = $Name
        }
    }

    [ordered]@{
        exists = $true
        name = [string]$vm.Name
        state = [string]$vm.State
        state_value = [int]$vm.State
        uptime_seconds = [int][Math]::Round($vm.Uptime.TotalSeconds)
        status = [string]$vm.Status
        heartbeat = [string]$vm.Heartbeat
        heartbeat_value = [int]$vm.Heartbeat
        cpu_usage = [int]$vm.CPUUsage
        memory_assigned = [int64]$vm.MemoryAssigned
    }
}

function Test-PcvNoContactIdleVm {
    param([AllowNull()]$Snapshot)

    if ($null -eq $Snapshot -or -not [bool]$Snapshot.exists) {
        return $false
    }

    $isRunning = [string]::Equals([string]$Snapshot.state, 'Running', [System.StringComparison]::OrdinalIgnoreCase) -or
        ([int]$Snapshot.state_value -eq 2)
    $isNoContact = [string]$Snapshot.heartbeat -match 'NoContact' -or
        ([int]$Snapshot.heartbeat_value -eq 2)
    $isIdle = [int]$Snapshot.cpu_usage -le 1

    $isRunning -and $isNoContact -and $isIdle
}

function Wait-PcvPowerShellDirect {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][pscredential]$Credential,
        [Parameter(Mandatory)][int]$TimeoutSeconds,
        [int]$ProbeIntervalSeconds = 10,
        [switch]$AllowNoContactRecovery,
        [int]$NoContactRecoveryIdleSeconds = 900
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $attempts = @()
    $recoveryActions = @()
    $noContactSince = $null
    $lastVmSnapshot = $null
    do {
        try {
            $probe = Invoke-Command -VMName $Name -Credential $Credential -ScriptBlock {
                [ordered]@{
                    ok = $true
                    computer_name = $env:COMPUTERNAME
                    user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
                    powershell = $PSVersionTable.PSVersion.ToString()
                    os = (Get-CimInstance Win32_OperatingSystem).Caption
                }
            } -ErrorAction Stop

            return [ordered]@{
                ok = $true
                attempts = $attempts.Count + 1
                probe = $probe
                automatic_recovery_performed = @($recoveryActions).Count -gt 0
                recovery_actions = @($recoveryActions)
                last_vm_status = $lastVmSnapshot
            }
        }
        catch {
            $now = Get-Date
            $lastVmSnapshot = Get-PcvVmRecoverySnapshot -Name $Name
            $isNoContactIdle = Test-PcvNoContactIdleVm -Snapshot $lastVmSnapshot
            if ($isNoContactIdle -and $null -eq $noContactSince) {
                $noContactSince = $now
            }
            elseif (-not $isNoContactIdle) {
                $noContactSince = $null
            }

            $noContactIdleSeconds = if ($null -eq $noContactSince) {
                0
            }
            else {
                [int][Math]::Round(($now - $noContactSince).TotalSeconds)
            }

            $attempt = [ordered]@{
                utc = (Get-Date).ToUniversalTime().ToString('o')
                error = $_.Exception.Message
                vm_status = $lastVmSnapshot
                no_contact_idle_seconds = $noContactIdleSeconds
            }

            if ($AllowNoContactRecovery -and
                @($recoveryActions).Count -eq 0 -and
                $isNoContactIdle -and
                $noContactIdleSeconds -ge $NoContactRecoveryIdleSeconds) {
                $before = $lastVmSnapshot
                Stop-VM -Name $Name -TurnOff -Force -ErrorAction SilentlyContinue
                Start-VM -Name $Name | Out-Null
                Start-Sleep -Seconds 5
                $after = Get-PcvVmRecoverySnapshot -Name $Name
                $recoveryAction = [ordered]@{
                    utc = (Get-Date).ToUniversalTime().ToString('o')
                    reason = 'post-windows-update-heartbeat-no-contact-cpu-idle'
                    no_contact_idle_seconds = $noContactIdleSeconds
                    threshold_seconds = $NoContactRecoveryIdleSeconds
                    action = 'Stop-VM -TurnOff -Force; Start-VM'
                    before = $before
                    after = $after
                }
                $recoveryActions += $recoveryAction
                $attempt['recovery_action'] = $recoveryAction
                $lastVmSnapshot = $after
                $noContactSince = $null
            }

            $attempts += $attempt
            Start-Sleep -Seconds $ProbeIntervalSeconds
        }
    } while ((Get-Date) -lt $deadline)

    [ordered]@{
        ok = $false
        attempts = $attempts.Count
        errors = @($attempts | Select-Object -Last 10)
        automatic_recovery_performed = @($recoveryActions).Count -gt 0
        recovery_actions = @($recoveryActions)
        last_vm_status = $lastVmSnapshot
    }
}

function Invoke-PcvGuestWindowsUpdatePreparation {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][pscredential]$Credential,
        [AllowNull()][string]$TitlePattern,
        [Parameter(Mandatory)][int]$UpdateTimeoutSeconds,
        [Parameter(Mandatory)][int]$RebootTimeoutSeconds,
        [Parameter(Mandatory)][bool]$EnableNoContactRecovery,
        [Parameter(Mandatory)][int]$NoContactRecoverySeconds
    )

    $result = [ordered]@{
        requested = $true
        ok = $false
        reboot_performed = $false
        no_contact_recovery_policy = [ordered]@{
            enabled = $EnableNoContactRecovery
            threshold_seconds = $NoContactRecoverySeconds
            reason = 'post-windows-update-heartbeat-no-contact-cpu-idle'
        }
    }

    $updateJob = $null
    try {
        $updateJob = Start-Job -ScriptBlock {
            param(
                [string]$VmName,
                [pscredential]$VmCredential,
                [AllowNull()][string]$UpdateTitlePattern
            )

            $session = New-PSSession -VMName $VmName -Credential $VmCredential -ErrorAction Stop
            try {
                Invoke-Command -Session $session -ArgumentList $UpdateTitlePattern -ScriptBlock {
                    param([AllowNull()][string]$UpdateTitlePattern)

                    Set-StrictMode -Version Latest
                    $ErrorActionPreference = 'Stop'

                    function Get-OsSnapshot {
                        $currentVersion = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
                        $os = Get-CimInstance Win32_OperatingSystem
                        [ordered]@{
                            caption = $os.Caption
                            version = $os.Version
                            build = $os.BuildNumber
                            ubr = $currentVersion.UBR
                            last_boot = $os.LastBootUpTime
                        }
                    }

                    $preUpdateOs = Get-OsSnapshot
                    $updateSession = New-Object -ComObject Microsoft.Update.Session
                    $searcher = $updateSession.CreateUpdateSearcher()
                    $search = $searcher.Search("IsInstalled=0 and Type='Software'")
                    $updates = New-Object -ComObject Microsoft.Update.UpdateColl
                    $titles = @()
                    $skippedTitles = @()
                    for ($index = 0; $index -lt $search.Updates.Count; $index++) {
                        $update = $search.Updates.Item($index)
                        $title = [string]$update.Title
                        if (-not [string]::IsNullOrWhiteSpace($UpdateTitlePattern) -and
                            $title -notmatch $UpdateTitlePattern) {
                            $skippedTitles += $title
                            continue
                        }

                        if (-not $update.EulaAccepted) {
                            $update.AcceptEula()
                        }

                        [void]$updates.Add($update)
                        $titles += $title
                    }

                    if ($updates.Count -eq 0) {
                        return [ordered]@{
                            ok = $true
                            pre_update_os = $preUpdateOs
                            update_count = 0
                            titles = @()
                            skipped_titles = $skippedTitles
                            title_pattern = $UpdateTitlePattern
                            download_result = $null
                            install_result = $null
                            reboot_required = $false
                            result_codes = @()
                            hresults = @()
                        }
                    }

                    $downloader = $updateSession.CreateUpdateDownloader()
                    $downloader.Updates = $updates
                    $downloadResult = $downloader.Download()

                    $installer = $updateSession.CreateUpdateInstaller()
                    $installer.Updates = $updates
                    $installResult = $installer.Install()

                    $resultCodes = @()
                    $hresults = @()
                    for ($index = 0; $index -lt $updates.Count; $index++) {
                        $updateResult = $installResult.GetUpdateResult($index)
                        $resultCodes += [int]$updateResult.ResultCode
                        $hresults += ('0x{0:X8}' -f $updateResult.HResult)
                    }

                    [ordered]@{
                        ok = (($downloadResult.ResultCode -eq 2) -and ($installResult.ResultCode -eq 2))
                        pre_update_os = $preUpdateOs
                        update_count = $updates.Count
                        titles = $titles
                        skipped_titles = $skippedTitles
                        title_pattern = $UpdateTitlePattern
                        download_result = [int]$downloadResult.ResultCode
                        install_result = [int]$installResult.ResultCode
                        reboot_required = [bool]$installResult.RebootRequired
                        result_codes = $resultCodes
                        hresults = $hresults
                    }
                }
            }
            finally {
                if ($null -ne $session) {
                    Remove-PSSession -Session $session -ErrorAction SilentlyContinue
                }
            }
        } -ArgumentList $Name, $Credential, $TitlePattern

        if (-not (Wait-Job -Job $updateJob -Timeout $UpdateTimeoutSeconds)) {
            Stop-Job -Job $updateJob -ErrorAction SilentlyContinue
            $result.ok = $false
            $result.blocker = "Windows Update preparation timed out after $UpdateTimeoutSeconds seconds."
            return $result
        }

        $updateResult = Receive-Job -Job $updateJob -ErrorAction Stop
        $result.update = $updateResult
        if (-not [bool]$updateResult.ok) {
            $result.ok = $false
            $result.blocker = 'Windows Update COM install did not complete successfully.'
            return $result
        }

        if ([bool]$updateResult.reboot_required) {
            $result.reboot_performed = $true
            Restart-VM -Name $Name -Force
            $postReboot = Wait-PcvPowerShellDirect `
                -Name $Name `
                -Credential $Credential `
                -TimeoutSeconds $RebootTimeoutSeconds `
                -AllowNoContactRecovery:$EnableNoContactRecovery `
                -NoContactRecoveryIdleSeconds $NoContactRecoverySeconds
            $result.post_reboot_powershell_direct = $postReboot
            if (-not [bool]$postReboot.ok) {
                $result.timeout_forced_restart_performed = $true
                $result.timeout_forced_restart_reason = 'post-reboot-powershell-direct-timeout'
                Stop-VM -Name $Name -TurnOff -Force -ErrorAction SilentlyContinue
                Start-VM -Name $Name | Out-Null
                $postReboot = Wait-PcvPowerShellDirect -Name $Name -Credential $Credential -TimeoutSeconds $RebootTimeoutSeconds
                $result.post_forced_restart_powershell_direct = $postReboot
                if (-not [bool]$postReboot.ok) {
                    $result.ok = $false
                    $result.blocker = 'PowerShell Direct did not return after Windows Update reboot.'
                    return $result
                }
            }
        }

        $session = New-PSSession -VMName $Name -Credential $Credential -ErrorAction Stop
        try {
            $result.post_update_os = Invoke-Command -Session $session -ScriptBlock {
                $currentVersion = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
                $os = Get-CimInstance Win32_OperatingSystem
                [ordered]@{
                    caption = $os.Caption
                    version = $os.Version
                    build = $os.BuildNumber
                    ubr = $currentVersion.UBR
                    last_boot = $os.LastBootUpTime
                }
            }
        }
        finally {
            if ($null -ne $session) {
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
            }
        }

        $result.ok = $true
        $result
    }
    catch {
        $result.ok = $false
        $result.blocker = $_.Exception.Message
        $result
    }
    finally {
        if ($null -ne $updateJob) {
            Remove-Job -Job $updateJob -Force -ErrorAction SilentlyContinue
        }
    }
}

function Stop-PcvVmIfPresent {
    param([Parameter(Mandatory)][string]$Name)

    $vm = Get-VM -Name $Name -ErrorAction SilentlyContinue
    if ($null -eq $vm) {
        return
    }

    if ($vm.State -ne 'Off') {
        Stop-VM -Name $Name -TurnOff -Force -ErrorAction SilentlyContinue
    }
}

function Get-PcvAvailableDriveLetter {
    $used = @(Get-Volume -ErrorAction SilentlyContinue |
        Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.DriveLetter) } |
        ForEach-Object { [string]$_.DriveLetter })

    foreach ($code in ([byte][char]'Z')..([byte][char]'D')) {
        $letter = [string][char]$code
        if ($used -notcontains $letter) {
            return $letter
        }
    }

    throw 'No available drive letter was found for offline guest customization.'
}

function Add-PcvGuestUnattend {
    param(
        [Parameter(Mandatory)][string]$VhdPath,
        [Parameter(Mandatory)][string]$Password
    )

    $mounted = $null
    try {
        $mounted = Mount-VHD -Path $VhdPath -Passthru -ErrorAction Stop
        $disk = $mounted | Get-Disk -ErrorAction Stop
        $windowsRoot = $null

        foreach ($partition in @(Get-Partition -DiskNumber $disk.Number -ErrorAction Stop)) {
            if ($partition.Type -eq 'Reserved') {
                continue
            }

            $volume = $partition | Get-Volume -ErrorAction SilentlyContinue
            $driveLetter = if ($null -ne $volume -and -not [string]::IsNullOrWhiteSpace([string]$volume.DriveLetter)) {
                [string]$volume.DriveLetter
            }
            else {
                $newLetter = Get-PcvAvailableDriveLetter
                Set-Partition -DiskNumber $disk.Number -PartitionNumber $partition.PartitionNumber -NewDriveLetter $newLetter -ErrorAction Stop
                $newLetter
            }

            $candidateRoot = "$driveLetter`:\"
            if (Test-Path -LiteralPath (Join-Path $candidateRoot 'Windows\System32\Config\SYSTEM') -PathType Leaf) {
                $windowsRoot = $candidateRoot
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($windowsRoot)) {
            throw 'Could not locate a Windows volume in the clean-host VHD.'
        }

        $pantherRoot = Join-Path $windowsRoot 'Windows\Panther'
        New-Item -ItemType Directory -Path $pantherRoot -Force | Out-Null

        $unattend = @"
<?xml version="1.0" encoding="utf-8"?>
<unattend xmlns="urn:schemas-microsoft-com:unattend">
  <settings pass="specialize">
    <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <ComputerName>*</ComputerName>
      <RegisteredOwner>PureCVisor</RegisteredOwner>
      <RegisteredOrganization>PureCVisor</RegisteredOrganization>
    </component>
  </settings>
  <settings pass="oobeSystem">
    <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <UserAccounts>
        <AdministratorPassword>
          <Value>$Password</Value>
          <PlainText>true</PlainText>
        </AdministratorPassword>
      </UserAccounts>
      <OOBE>
        <HideEULAPage>true</HideEULAPage>
        <HideLocalAccountScreen>true</HideLocalAccountScreen>
        <HideOEMRegistrationScreen>true</HideOEMRegistrationScreen>
        <HideOnlineAccountScreens>true</HideOnlineAccountScreens>
        <HideWirelessSetupInOOBE>true</HideWirelessSetupInOOBE>
        <ProtectYourPC>3</ProtectYourPC>
      </OOBE>
    </component>
  </settings>
</unattend>
"@

        $pantherUnattend = Join-Path $pantherRoot 'Unattend.xml'
        $rootUnattend = Join-Path $windowsRoot 'Unattend.xml'
        Set-Content -LiteralPath $pantherUnattend -Value $unattend -Encoding UTF8
        Set-Content -LiteralPath $rootUnattend -Value $unattend -Encoding UTF8

        [ordered]@{
            ok = $true
            windows_root = $windowsRoot
            panther_unattend_written = $true
            root_unattend_written = $true
            password_recorded = $false
        }
    }
    finally {
        if ($null -ne $mounted) {
            Dismount-VHD -Path $VhdPath -ErrorAction SilentlyContinue
        }
    }
}

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$hostPlanPath = Join-Path $artifactRootFull 'clean-host-runner-plan.json'
$guestOutputsRoot = Join-Path $artifactRootFull 'guest-outputs'
$vmRoot = if ([string]::IsNullOrWhiteSpace($VmRoot)) {
    Join-Path $env:ProgramData "PureCVisor\desktop-node\clean-host-vms\$VmName"
}
else {
    [System.IO.Path]::GetFullPath($VmRoot)
}
New-Item -ItemType Directory -Path $guestOutputsRoot -Force | Out-Null
New-Item -ItemType Directory -Path $vmRoot -Force | Out-Null

$summary = [ordered]@{
    ok = $false
    scope = 'internal-clean-host-install-update-rollback-smoke'
    actual_execution = 'hyper-v-dedicated-clean-host-installed-smoke'
    artifact_root = $artifactRootFull
    vm_name = $VmName
    host_mutation_performed = $false
    guest_product_mutation_performed = $false
    internal_clean_host_install_update_rollback_smoke = 'not-run'
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    update_channel = $UpdateChannel
    public_trusted_signing = 'out-of-scope'
    external_stable_publication = 'out-of-scope'
    winget_submission = 'out-of-scope'
    public_release = 'not-claimed'
    token_value_observed = $false
}

$vmCreated = $false
$diffVhdPath = Join-Path $vmRoot "$VmName.vhd"
$session = $null

try {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $isAdmin) {
        throw 'Administrator shell is required for Hyper-V clean-host smoke.'
    }

    foreach ($commandName in @('New-VM', 'Set-VM', 'Start-VM', 'Get-VM', 'New-VHD', 'Invoke-Command', 'Copy-Item')) {
        if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
            throw "Required command is unavailable: $commandName"
        }
    }
    if (-not [string]::IsNullOrWhiteSpace($VMSwitchName) -and
        -not (Get-Command Connect-VMNetworkAdapter -ErrorAction SilentlyContinue)) {
        throw 'Required command is unavailable: Connect-VMNetworkAdapter'
    }

    if (Get-VM -Name $VmName -ErrorAction SilentlyContinue) {
        throw "VM already exists: $VmName"
    }

    $baseVhdFull = Resolve-PcvFilePath -Path $BaseVhdPath
    $baselineMsiFull = Resolve-PcvFilePath -Path $BaselineMsiPath
    $updatePackageFull = Resolve-PcvFilePath -Path $UpdatePackagePath
    $internalRootCertificateFull = if ([string]::IsNullOrWhiteSpace($InternalRootCertificatePath)) {
        $null
    }
    else {
        Resolve-PcvFilePath -Path $InternalRootCertificatePath
    }
    $baselineMsiSha256 = Get-PcvSha256 -Path $baselineMsiFull
    $updatePackageSha256 = Get-PcvSha256 -Path $updatePackageFull

    $plan = [ordered]@{
        scope = 'internal-clean-host-install-update-rollback-smoke'
        vm_name = $VmName
        vm_generation = $VmGeneration
        vm_switch_name = $VMSwitchName
        base_vhd_path = $baseVhdFull
        differencing_vhd_path = $diffVhdPath
        baseline_msi_path = $baselineMsiFull
        baseline_msi_sha256 = $baselineMsiSha256
        update_package_path = $updatePackageFull
        update_package_sha256 = $updatePackageSha256
        internal_root_certificate_path = $internalRootCertificateFull
        baseline_version = $BaselineVersion
        target_version = $TargetVersion
        update_channel = $UpdateChannel
        target_signing_mode = $TargetSigningMode
        guest_user = $GuestUser
        guest_password_recorded = $false
        inject_unattend = $InjectUnattend
        install_windows_updates = [bool]$InstallWindowsUpdates
        windows_update_timeout_seconds = $WindowsUpdateTimeoutSeconds
        windows_update_reboot_timeout_seconds = $WindowsUpdateRebootTimeoutSeconds
        windows_update_no_contact_recovery_enabled = -not [bool]$DisableWindowsUpdateNoContactRecovery
        windows_update_no_contact_recovery_seconds = $WindowsUpdateNoContactRecoverySeconds
        windows_update_title_pattern = $WindowsUpdateTitlePattern
        transport = 'PowerShell Direct'
        removes_vm_on_success = [bool]$RemoveVmOnSuccess
        removes_vm_on_failure = [bool]$RemoveVmOnFailure
        required_steps = @(
            'create-differencing-vhd',
            'create-dedicated-hyper-v-vm',
            'connect-vm-network-if-configured',
            'wait-powershell-direct',
            'install-windows-updates-if-requested',
            'copy-internal-signed-msi-and-update-package',
            'install-baseline-internal-signed-msi',
            'verify-baseline-service-health',
            'generate-guest-internal-file-catalog',
            'update-via-internal-catalog',
            'verify-updated-service-health',
            'rollback-to-baseline',
            'verify-final-service-health',
            'copy-redacted-guest-evidence'
        )
    }
    Write-PcvJsonFile -Path $hostPlanPath -Value $plan

    $summary.host_mutation_performed = $true
    $summary.base_vhd_path = $baseVhdFull
    $summary.vm_switch_name = $VMSwitchName
    $summary.baseline_msi_sha256 = $baselineMsiSha256
    $summary.update_package_sha256 = $updatePackageSha256

    New-VHD -Path $diffVhdPath -ParentPath $baseVhdFull -Differencing | Out-Null
    if ($InjectUnattend) {
        $summary.unattend_injection = Add-PcvGuestUnattend -VhdPath $diffVhdPath -Password $GuestPassword
    }
    New-VM -Name $VmName -Generation $VmGeneration -MemoryStartupBytes 4GB -VHDPath $diffVhdPath -Path $vmRoot | Out-Null
    # An interrupted run keeps the VM when -RemoveVmOnFailure is off; the Hyper-V default
    # AutomaticStartAction=StartIfRunning would resurrect that orphan on the next host reboot.
    Set-VM -Name $VmName -AutomaticStartAction Nothing | Out-Null
    $summary.vm_automatic_start_action = 'Nothing'
    if (-not [string]::IsNullOrWhiteSpace($VMSwitchName)) {
        if (-not (Get-VMSwitch -Name $VMSwitchName -ErrorAction SilentlyContinue)) {
            throw "VM switch was not found: $VMSwitchName"
        }

        Connect-VMNetworkAdapter -VMName $VmName -SwitchName $VMSwitchName
    }
    Set-VMProcessor -VMName $VmName -Count 2 | Out-Null
    if ($VmGeneration -eq 2) {
        Set-VMFirmware -VMName $VmName -EnableSecureBoot On -SecureBootTemplate 'MicrosoftWindows' | Out-Null
    }
    $vmCreated = $true

    Start-VM -Name $VmName | Out-Null
    $credential = New-PcvCredential -UserName $GuestUser -Password $GuestPassword
    $psDirect = Wait-PcvPowerShellDirect -Name $VmName -Credential $credential -TimeoutSeconds $BootTimeoutSeconds
    $summary.powershell_direct = $psDirect
    if (-not [bool]$psDirect.ok) {
        $summary.internal_clean_host_install_update_rollback_smoke = 'blocked-by-guest-powershell-direct'
        $summary.blocker = 'PowerShell Direct did not become available for the dedicated VM before timeout.'
        Write-PcvJsonFile -Path $summaryPath -Value $summary
        $summary
        exit 1
    }

    if ($InstallWindowsUpdates) {
        $windowsUpdate = Invoke-PcvGuestWindowsUpdatePreparation `
            -Name $VmName `
            -Credential $credential `
            -TitlePattern $WindowsUpdateTitlePattern `
            -UpdateTimeoutSeconds $WindowsUpdateTimeoutSeconds `
            -RebootTimeoutSeconds $WindowsUpdateRebootTimeoutSeconds `
            -EnableNoContactRecovery (-not [bool]$DisableWindowsUpdateNoContactRecovery) `
            -NoContactRecoverySeconds $WindowsUpdateNoContactRecoverySeconds
        $summary.windows_update_preparation = $windowsUpdate
        if (-not [bool]$windowsUpdate.ok) {
            $summary.internal_clean_host_install_update_rollback_smoke = 'blocked-by-windows-update-preparation'
            $summary.blocker = if ($windowsUpdate.Contains('blocker')) {
                [string]$windowsUpdate.blocker
            }
            else {
                'Windows Update preparation did not complete successfully.'
            }
            Write-PcvJsonFile -Path $summaryPath -Value $summary
            $summary
            exit 1
        }
    }

    $session = New-PSSession -VMName $VmName -Credential $credential -ErrorAction Stop
    Invoke-Command -Session $session -ScriptBlock {
        Remove-Item -LiteralPath 'C:\Windows\Panther\Unattend.xml', 'C:\Unattend.xml' -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Path 'C:\PcvCleanHostSmoke' -Force | Out-Null
    } -ErrorAction Stop

    $guestBaselineMsi = 'C:\PcvCleanHostSmoke\baseline-internal-signed.msi'
    $guestUpdatePackage = 'C:\PcvCleanHostSmoke\target-update.zip'
    $guestCatalog = 'C:\PcvCleanHostSmoke\internal-update-catalog.json'
    $guestInternalRootCertificate = 'C:\PcvCleanHostSmoke\internal-root.cer'
    Copy-Item -ToSession $session -LiteralPath $baselineMsiFull -Destination $guestBaselineMsi -Force -ErrorAction Stop
    Copy-Item -ToSession $session -LiteralPath $updatePackageFull -Destination $guestUpdatePackage -Force -ErrorAction Stop
    if (-not [string]::IsNullOrWhiteSpace($internalRootCertificateFull)) {
        Copy-Item -ToSession $session -LiteralPath $internalRootCertificateFull -Destination $guestInternalRootCertificate -Force -ErrorAction Stop
    }

    $guestSummary = Invoke-Command -Session $session -ArgumentList @(
        $guestBaselineMsi,
        $guestUpdatePackage,
        $guestCatalog,
        $(if ([string]::IsNullOrWhiteSpace($internalRootCertificateFull)) { '' } else { $guestInternalRootCertificate }),
        $baselineMsiSha256,
        $updatePackageSha256,
        $BaselineVersion,
        $TargetVersion,
        $UpdateChannel,
        $TargetSigningMode,
        $ServiceTimeoutSeconds
    ) -ScriptBlock {
        param(
            [string]$BaselineMsi,
            [string]$UpdatePackage,
            [string]$CatalogPath,
            [string]$InternalRootCertificate,
            [string]$BaselineMsiSha256,
            [string]$UpdatePackageSha256,
            [string]$BaselineVersion,
            [string]$TargetVersion,
            [string]$UpdateChannel,
            [string]$TargetSigningMode,
            [int]$ServiceTimeoutSeconds
        )

        Set-StrictMode -Version Latest
        $ErrorActionPreference = 'Stop'

        function Write-GuestJsonFile {
            param(
                [Parameter(Mandatory)][string]$Path,
                [Parameter(Mandatory)]$Value
            )

            $parent = Split-Path -Parent $Path
            if (-not [string]::IsNullOrWhiteSpace($parent)) {
                New-Item -ItemType Directory -Path $parent -Force | Out-Null
            }
            $Value | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
        }

        function Get-GuestSha256 {
            param([Parameter(Mandatory)][string]$Path)

            (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
        }

        function Invoke-GuestProcess {
            param(
                [Parameter(Mandatory)][string]$FilePath,
                [string[]]$ArgumentList = @(),
                [string]$WorkingDirectory = 'C:\PcvCleanHostSmoke'
            )

            function Join-GuestArguments {
                param([string[]]$Arguments)

                @($Arguments | ForEach-Object {
                    $argument = [string]$_
                    if ($argument -match '[\s"]') {
                        '"' + ($argument -replace '"', '\"') + '"'
                    }
                    else {
                        $argument
                    }
                }) -join ' '
            }

            $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
            $startInfo.FileName = $FilePath
            $startInfo.Arguments = Join-GuestArguments -Arguments $ArgumentList
            $startInfo.UseShellExecute = $false
            $startInfo.RedirectStandardOutput = $true
            $startInfo.RedirectStandardError = $true
            if (-not [string]::IsNullOrWhiteSpace($WorkingDirectory)) {
                $startInfo.WorkingDirectory = $WorkingDirectory
            }

            $process = [System.Diagnostics.Process]::new()
            $process.StartInfo = $startInfo
            try {
                [void]$process.Start()
                $stdout = $process.StandardOutput.ReadToEnd()
                $stderr = $process.StandardError.ReadToEnd()
                $process.WaitForExit()
                [ordered]@{
                    exit_code = $process.ExitCode
                    stdout = $stdout
                    stderr = $stderr
                    arguments = $ArgumentList
                }
            }
            finally {
                $process.Dispose()
            }
        }

        function Wait-GuestServiceRunning {
            param(
                [Parameter(Mandatory)][string]$Name,
                [Parameter(Mandatory)][int]$TimeoutSeconds
            )

            $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
            do {
                $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
                if ($null -ne $service -and $service.Status -eq 'Running') {
                    return [ordered]@{
                        ok = $true
                        name = $service.Name
                        status = $service.Status.ToString()
                    }
                }
                Start-Sleep -Seconds 2
            } while ((Get-Date) -lt $deadline)

            $final = Get-Service -Name $Name -ErrorAction SilentlyContinue
            [ordered]@{
                ok = $false
                name = $Name
                status = if ($null -eq $final) { 'Missing' } else { $final.Status.ToString() }
            }
        }

        function Get-GuestServiceSnapshot {
            param([Parameter(Mandatory)][string]$Name)

            $service = Get-CimInstance Win32_Service -Filter "Name='$Name'" -ErrorAction SilentlyContinue
            if ($null -eq $service) {
                return [ordered]@{
                    name = $Name
                    state = 'Missing'
                    start_mode = $null
                    path_name = $null
                }
            }

            [ordered]@{
                name = $service.Name
                state = $service.State
                start_mode = $service.StartMode
                path_name = $service.PathName
            }
        }

        function Read-GuestManifest {
            param([string]$Path = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json')

            if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
                return [ordered]@{
                    exists = $false
                    path = $Path
                    version = $null
                }
            }

            $manifest = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
            $manifestProperties = @($manifest.PSObject.Properties.Name)
            $productProperties = if ($null -ne $manifest.product -and $manifest.product -isnot [string]) {
                @($manifest.product.PSObject.Properties.Name)
            }
            else {
                @()
            }
            $version = if ($manifestProperties -contains 'version') {
                [string]$manifest.version
            }
            elseif ($productProperties -contains 'version') {
                [string]$manifest.product.version
            }
            else {
                $null
            }
            $releaseChannel = if ($manifestProperties -contains 'release_channel') {
                [string]$manifest.release_channel
            }
            elseif ($productProperties -contains 'release_channel') {
                [string]$manifest.product.release_channel
            }
            else {
                $null
            }

            [ordered]@{
                exists = $true
                path = $Path
                version = $version
                release_channel = $releaseChannel
            }
        }

        function Invoke-GuestHttpCheck {
            param([Parameter(Mandatory)][string]$Uri)

            try {
                $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
                [ordered]@{
                    ok = $true
                    uri = $Uri
                    status_code = [int]$response.StatusCode
                    length = if ($null -ne $response.Content) { [int]$response.Content.Length } else { 0 }
                }
            }
            catch {
                [ordered]@{
                    ok = $false
                    uri = $Uri
                    error = $_.Exception.Message
                }
            }
        }

        function Convert-GuestProcessJson {
            param([Parameter(Mandatory)]$ProcessResult)

            try {
                if (-not [string]::IsNullOrWhiteSpace([string]$ProcessResult.stdout)) {
                    return ($ProcessResult.stdout | ConvertFrom-Json)
                }
            }
            catch {
                return [ordered]@{
                    ok = $false
                    parse_error = $_.Exception.Message
                }
            }

            [ordered]@{
                ok = $false
                parse_error = 'empty stdout'
            }
        }

        $outputsRoot = 'C:\PcvCleanHostSmoke\outputs'
        New-Item -ItemType Directory -Path $outputsRoot -Force | Out-Null
        $summaryPath = Join-Path $outputsRoot 'guest-summary.json'
        $installLogPath = Join-Path $outputsRoot 'baseline-msi-install.log'
        $updateStdoutPath = Join-Path $outputsRoot 'update-stdout.json'
        $rollbackStdoutPath = Join-Path $outputsRoot 'rollback-stdout.json'

        $summary = [ordered]@{
            ok = $false
            scope = 'internal-clean-host-install-update-rollback-smoke-guest'
            actual_execution = 'guest-msi-install-catalog-update-rollback'
            baseline_version = $BaselineVersion
            target_version = $TargetVersion
            update_channel = $UpdateChannel
            service_name = 'PureCVisorDesktopNode'
            token_value_observed = $false
            public_trusted_signing = 'out-of-scope'
            external_stable_publication = 'out-of-scope'
            public_release = 'not-claimed'
        }

        try {
            $baselineActualSha = Get-GuestSha256 -Path $BaselineMsi
            $updateActualSha = Get-GuestSha256 -Path $UpdatePackage
            if ($baselineActualSha -ne $BaselineMsiSha256) {
                throw "Baseline MSI SHA-256 mismatch: $baselineActualSha"
            }
            if ($updateActualSha -ne $UpdatePackageSha256) {
                throw "Update package SHA-256 mismatch: $updateActualSha"
            }

            $signature = Get-AuthenticodeSignature -LiteralPath $BaselineMsi
            $summary.baseline_msi_signature_before_root_import = [ordered]@{
                status = $signature.Status.ToString()
                signer_subject = if ($null -eq $signature.SignerCertificate) { $null } else { $signature.SignerCertificate.Subject }
                signer_thumbprint = if ($null -eq $signature.SignerCertificate) { $null } else { $signature.SignerCertificate.Thumbprint }
            }

            if (-not [string]::IsNullOrWhiteSpace($InternalRootCertificate) -and
                (Test-Path -LiteralPath $InternalRootCertificate -PathType Leaf)) {
                $importedRoot = Import-Certificate -FilePath $InternalRootCertificate -CertStoreLocation Cert:\LocalMachine\Root
                $summary.internal_root_certificate_import = [ordered]@{
                    imported = $true
                    subject = if ($null -eq $importedRoot) { $null } else { $importedRoot.Subject }
                    thumbprint = if ($null -eq $importedRoot) { $null } else { $importedRoot.Thumbprint }
                    store = 'Cert:\LocalMachine\Root'
                }
            }
            else {
                $summary.internal_root_certificate_import = [ordered]@{
                    imported = $false
                    store = 'Cert:\LocalMachine\Root'
                }
            }

            $signature = Get-AuthenticodeSignature -LiteralPath $BaselineMsi
            $summary.baseline_msi_signature = [ordered]@{
                status = $signature.Status.ToString()
                signer_subject = if ($null -eq $signature.SignerCertificate) { $null } else { $signature.SignerCertificate.Subject }
                signer_thumbprint = if ($null -eq $signature.SignerCertificate) { $null } else { $signature.SignerCertificate.Thumbprint }
            }

            $packageUri = 'file:///' + ($UpdatePackage -replace '\\', '/')
            $catalog = [ordered]@{
                schema_version = 1
                product = 'PureCVisor Desktop Node'
                generated_utc = (Get-Date).ToUniversalTime().ToString('o')
                publication = [ordered]@{
                    public_trusted_signing = 'out-of-scope'
                    external_stable_publication = 'out-of-scope'
                }
                channels = @(
                    [ordered]@{
                        name = $UpdateChannel
                        version = $TargetVersion
                        release_channel = $UpdateChannel
                        signing_mode = $TargetSigningMode
                        package_uri = $packageUri
                        sha256 = $UpdatePackageSha256
                    }
                )
            }
            Write-GuestJsonFile -Path $CatalogPath -Value $catalog
            $summary.internal_catalog = [ordered]@{
                path = $CatalogPath
                package_uri = $packageUri
                sha256 = $UpdatePackageSha256
            }

            $eventLogBefore = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\wevtutil.exe" -ArgumentList @('gl', 'Application')
            $eventLogPrepare = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\wevtutil.exe" -ArgumentList @('sl', 'Application', '/ms:33554432', '/rt:false')
            $eventLogAfter = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\wevtutil.exe" -ArgumentList @('gl', 'Application')
            $summary.event_log_volume_guard_preparation = [ordered]@{
                before_exit_code = $eventLogBefore.exit_code
                before = $eventLogBefore.stdout
                prepare_exit_code = $eventLogPrepare.exit_code
                prepare_stderr = $eventLogPrepare.stderr
                after_exit_code = $eventLogAfter.exit_code
                after = $eventLogAfter.stdout
            }
            if ($eventLogPrepare.exit_code -ne 0) {
                throw "Application Event Log volume guard preparation failed with exit code $($eventLogPrepare.exit_code)."
            }

            $install = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\msiexec.exe" -ArgumentList @(
                '/i',
                $BaselineMsi,
                '/qn',
                '/norestart',
                '/l*v',
                $installLogPath
            )
            $summary.install = $install
            if ($install.exit_code -ne 0 -and $install.exit_code -ne 3010) {
                throw "Baseline MSI install failed with exit code $($install.exit_code)."
            }

            $baselineServiceWait = Wait-GuestServiceRunning -Name 'PureCVisorDesktopNode' -TimeoutSeconds $ServiceTimeoutSeconds
            $summary.baseline_service_wait = $baselineServiceWait
            if (-not [bool]$baselineServiceWait.ok) {
                throw 'Baseline service did not reach Running.'
            }

            $summary.baseline_manifest = Read-GuestManifest
            $summary.baseline_service = Get-GuestServiceSnapshot -Name 'PureCVisorDesktopNode'
            $summary.baseline_web_console = Invoke-GuestHttpCheck -Uri 'http://127.0.0.1/'
            if ($summary.baseline_manifest.version -ne $BaselineVersion) {
                throw "Baseline manifest version mismatch: $($summary.baseline_manifest.version)"
            }
            if (-not [bool]$summary.baseline_web_console.ok -or [int]$summary.baseline_web_console.status_code -ne 200) {
                throw 'Baseline loopback Web Console did not return HTTP 200.'
            }

            $wrapper = 'C:\Program Files\PureCVisor\DesktopNode\Invoke-PcvDesktopNodeProduct.ps1'
            $downloadRoot = 'C:\ProgramData\PureCVisor\desktop-node\updates'
            $updateResult = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -ArgumentList @(
                '-NoProfile',
                '-ExecutionPolicy',
                'Bypass',
                '-File',
                $wrapper,
                '-Action',
                'Update',
                '-UpdateCatalogUri',
                ('file:///' + ($CatalogPath -replace '\\', '/')),
                '-UpdateChannel',
                $UpdateChannel,
                '-DownloadRoot',
                $downloadRoot
            )
            $updateResult.stdout | Set-Content -LiteralPath $updateStdoutPath -Encoding UTF8
            $summary.update = $updateResult
            $summary.update_result = Convert-GuestProcessJson -ProcessResult $updateResult
            if ($updateResult.exit_code -ne 0) {
                throw "Catalog update failed with exit code $($updateResult.exit_code)."
            }

            $updatedServiceWait = Wait-GuestServiceRunning -Name 'PureCVisorDesktopNode' -TimeoutSeconds $ServiceTimeoutSeconds
            $summary.updated_service_wait = $updatedServiceWait
            if (-not [bool]$updatedServiceWait.ok) {
                throw 'Updated service did not reach Running.'
            }

            $summary.updated_manifest = Read-GuestManifest
            $summary.updated_service = Get-GuestServiceSnapshot -Name 'PureCVisorDesktopNode'
            $summary.updated_web_console = Invoke-GuestHttpCheck -Uri 'http://127.0.0.1/'
            if ($summary.updated_manifest.version -ne $TargetVersion) {
                throw "Updated manifest version mismatch: $($summary.updated_manifest.version)"
            }
            if (-not [bool]$summary.updated_web_console.ok -or [int]$summary.updated_web_console.status_code -ne 200) {
                throw 'Updated loopback Web Console did not return HTTP 200.'
            }

            $rollbackResult = Invoke-GuestProcess -FilePath "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -ArgumentList @(
                '-NoProfile',
                '-ExecutionPolicy',
                'Bypass',
                '-File',
                $wrapper,
                '-Action',
                'Rollback'
            )
            $rollbackResult.stdout | Set-Content -LiteralPath $rollbackStdoutPath -Encoding UTF8
            $summary.rollback = $rollbackResult
            $summary.rollback_result = Convert-GuestProcessJson -ProcessResult $rollbackResult
            if ($rollbackResult.exit_code -ne 0) {
                throw "Rollback failed with exit code $($rollbackResult.exit_code)."
            }

            $finalServiceWait = Wait-GuestServiceRunning -Name 'PureCVisorDesktopNode' -TimeoutSeconds $ServiceTimeoutSeconds
            $summary.final_service_wait = $finalServiceWait
            if (-not [bool]$finalServiceWait.ok) {
                throw 'Final service did not reach Running after rollback.'
            }

            $summary.final_manifest = Read-GuestManifest
            $summary.final_service = Get-GuestServiceSnapshot -Name 'PureCVisorDesktopNode'
            $summary.final_web_console = Invoke-GuestHttpCheck -Uri 'http://127.0.0.1/'
            $summary.previous_root_exists_after_rollback = Test-Path -LiteralPath 'C:\Program Files\PureCVisor\DesktopNode.previous' -PathType Container
            $summary.failed_root_exists_after_rollback = Test-Path -LiteralPath 'C:\Program Files\PureCVisor\DesktopNode.failed' -PathType Container
            $failedManifest = 'C:\Program Files\PureCVisor\DesktopNode.failed\product-manifest.json'
            if (Test-Path -LiteralPath $failedManifest -PathType Leaf) {
                $summary.failed_root_manifest = Read-GuestManifest -Path $failedManifest
            }
            if ($summary.final_manifest.version -ne $BaselineVersion) {
                throw "Final manifest version mismatch: $($summary.final_manifest.version)"
            }
            if (-not [bool]$summary.final_web_console.ok -or [int]$summary.final_web_console.status_code -ne 200) {
                throw 'Final loopback Web Console did not return HTTP 200.'
            }

            $summary.ok = $true
            $summary.internal_clean_host_install_update_rollback_smoke = 'pass'
            Write-GuestJsonFile -Path $summaryPath -Value $summary
            $summary
        }
        catch {
            $extraLogs = [ordered]@{}
            foreach ($logPath in @(
                'C:\ProgramData\PureCVisor\desktop-node\install.jsonl',
                'C:\ProgramData\PureCVisor\desktop-node\eventlog-default-transition.json',
                'C:\ProgramData\PureCVisor\desktop-node\credential-manager-default-transition.json',
                'C:\ProgramData\PureCVisor\desktop-node\service-logs\desktop-node-host.log',
                'C:\ProgramData\PureCVisor\desktop-node\service-logs\desktop-node-host.err.log'
            )) {
                if (Test-Path -LiteralPath $logPath -PathType Leaf) {
                    $destination = Join-Path $outputsRoot (Split-Path -Leaf $logPath)
                    Copy-Item -LiteralPath $logPath -Destination $destination -Force -ErrorAction SilentlyContinue
                    $extraLogs[$logPath] = $destination
                }
            }

            $summary.ok = $false
            $summary.internal_clean_host_install_update_rollback_smoke = 'failed'
            $summary.error = [ordered]@{
                message = $_.Exception.Message
            }
            $summary.extra_logs = $extraLogs
            Write-GuestJsonFile -Path $summaryPath -Value $summary
            $summary
        }
    } -ErrorAction Stop

    $summary.guest_product_mutation_performed = $true
    $summary.guest_summary = $guestSummary
    Copy-Item -FromSession $session -LiteralPath 'C:\PcvCleanHostSmoke\outputs\*' -Destination $guestOutputsRoot -Recurse -Force -ErrorAction SilentlyContinue

    if (-not [bool]$guestSummary.ok) {
        $summary.ok = $false
        $summary.internal_clean_host_install_update_rollback_smoke = 'failed'
        $summary.blocker = if ($guestSummary.PSObject.Properties.Name -contains 'error') { [string]$guestSummary.error.message } else { 'guest smoke failed' }
        Write-PcvJsonFile -Path $summaryPath -Value $summary
        $summary
        exit 1
    }

    $summary.install_exit_code = [int]$guestSummary.install.exit_code
    $summary.update_exit_code = [int]$guestSummary.update.exit_code
    $summary.rollback_exit_code = [int]$guestSummary.rollback.exit_code
    $summary.baseline_manifest_version = [string]$guestSummary.baseline_manifest.version
    $summary.updated_manifest_version = [string]$guestSummary.updated_manifest.version
    $summary.final_manifest_version = [string]$guestSummary.final_manifest.version
    $summary.final_service = $guestSummary.final_service
    $summary.final_web_status_code = [int]$guestSummary.final_web_console.status_code
    $summary.failed_root_exists_after_rollback = [bool]$guestSummary.failed_root_exists_after_rollback
    if ($guestSummary.PSObject.Properties.Name -contains 'failed_root_manifest') {
        $summary.failed_root_manifest_version = [string]$guestSummary.failed_root_manifest.version
    }

    if ([bool]$guestSummary.ok) {
        $summary.ok = $true
        $summary.internal_clean_host_install_update_rollback_smoke = 'pass'
        $summary.blocker = 'none'
    }
    else {
        $summary.ok = $false
        $summary.internal_clean_host_install_update_rollback_smoke = 'failed'
        $summary.blocker = if ($guestSummary.PSObject.Properties.Name -contains 'error') { [string]$guestSummary.error.message } else { 'guest smoke failed' }
    }

    Write-PcvJsonFile -Path $summaryPath -Value $summary
    $summary
}
catch {
    $summary.ok = $false
    $summary.internal_clean_host_install_update_rollback_smoke = 'failed'
    $summary.error = [ordered]@{
        message = $_.Exception.Message
    }
    if (-not $summary.Contains('blocker')) {
        $summary.blocker = $_.Exception.Message
    }
    Write-PcvJsonFile -Path $summaryPath -Value $summary
    $summary
    exit 1
}
finally {
    if ($null -ne $session) {
        Remove-PSSession -Session $session -ErrorAction SilentlyContinue
    }

    $shouldRemoveVm = ($summary.ok -and $RemoveVmOnSuccess) -or ((-not $summary.ok) -and $RemoveVmOnFailure)
    if ($vmCreated -and $shouldRemoveVm) {
        Stop-PcvVmIfPresent -Name $VmName
        Remove-VM -Name $VmName -Force -ErrorAction SilentlyContinue
        if ((Test-Path -LiteralPath $diffVhdPath -PathType Leaf) -and (Test-PcvChildPath -Path $diffVhdPath -Parent $vmRoot)) {
            Remove-Item -LiteralPath $diffVhdPath -Force -ErrorAction SilentlyContinue
        }
    }
}

Set-StrictMode -Version Latest

function New-PcvError {
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

function New-PcvResponse {
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

function ConvertTo-PcvJson {
    param([Parameter(Mandatory, ValueFromPipeline)]$Value)
    $Value | ConvertTo-Json -Depth 20 -Compress
}

function Test-PcvAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

function Get-PcvWindowsEdition {
    param([AllowNull()][string]$ProductName)

    if ($ProductName -match 'Enterprise') { return 'Enterprise' }
    if ($ProductName -match 'Professional|Pro') { return 'Pro' }
    if ($ProductName -match 'Education') { return 'Education' }
    if ($ProductName -match 'Home') { return 'Home' }
    return 'Unknown'
}

function Test-PcvHyperVCmdletsAvailable {
    return [bool](Get-Command Get-VM -ErrorAction SilentlyContinue)
}

function Get-PcvHostStatus {
    $reasons = New-Object System.Collections.Generic.List[string]

    $caption = $null
    $version = $null
    $edition = 'Unknown'
    try {
        $computer = Get-ComputerInfo
        $caption = [string]$computer.WindowsProductName
        $version = [string]$computer.WindowsVersion
        $edition = Get-PcvWindowsEdition -ProductName $caption
    }
    catch {
        $reasons.Add('PCV_WINDOWS_INFO_UNKNOWN')
    }

    $isSupportedEdition = $edition -in @('Pro', 'Enterprise', 'Education')
    if (-not $isSupportedEdition) {
        $reasons.Add('PCV_WINDOWS_EDITION_UNSUPPORTED')
    }

    $featureEnabled = $false
    try {
        if (Test-PcvHyperVCmdletsAvailable) {
            $featureEnabled = $true
        }
        else {
            $feature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V
            $featureEnabled = ([string]$feature.State -eq 'Enabled')
        }
        if (-not $featureEnabled) {
            $reasons.Add('PCV_HYPERV_NOT_ENABLED')
        }
    }
    catch {
        $reasons.Add('PCV_HYPERV_FEATURE_UNKNOWN')
    }

    $vmmsRunning = $false
    try {
        $vmms = Get-Service -Name vmms
        $vmmsRunning = ([string]$vmms.Status -eq 'Running')
        if (-not $vmmsRunning) {
            $reasons.Add('PCV_VMMS_NOT_RUNNING')
        }
    }
    catch {
        $reasons.Add('PCV_VMMS_UNKNOWN')
    }

    $defaultSwitchPresent = $false
    try {
        $switches = @(Get-VMSwitch -ErrorAction Stop)
        $defaultSwitchPresent = [bool]($switches | Where-Object { $_.Name -eq 'Default Switch' } | Select-Object -First 1)
        if (-not $defaultSwitchPresent) {
            $reasons.Add('PCV_DEFAULT_SWITCH_MISSING')
        }
    }
    catch {
        $reasons.Add('PCV_DEFAULT_SWITCH_UNKNOWN')
    }

    $isAdmin = $false
    try {
        $isAdmin = Test-PcvAdmin
        if (-not $isAdmin) {
            $reasons.Add('PCV_ADMIN_REQUIRED')
        }
    }
    catch {
        $reasons.Add('PCV_ADMIN_UNKNOWN')
    }

    [ordered]@{
        supported = ($isSupportedEdition -and $featureEnabled -and $vmmsRunning -and $defaultSwitchPresent -and $isAdmin)
        reasons = @($reasons)
        windows = [ordered]@{
            caption = $caption
            version = $version
            edition = $edition
        }
        admin = [ordered]@{
            elevated = $isAdmin
        }
        hyperv = [ordered]@{
            feature_enabled = $featureEnabled
            vmms_running = $vmmsRunning
            default_switch_present = $defaultSwitchPresent
        }
    }
}

function Convert-PcvBytesToMiB {
    param([Parameter(Mandatory)][UInt64]$Bytes)
    [int][math]::Round($Bytes / 1MB)
}

function Convert-PcvBytesToGiB {
    param([Parameter(Mandatory)][UInt64]$Bytes)
    [int][math]::Round($Bytes / 1GB)
}

function Get-PcvObjectPropertyValue {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $Value) { return $null }
    $property = $Value.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }
    $property.Value
}

function Get-PcvVmState {
    param([Parameter(Mandatory)][string]$State)
    switch ($State.ToLowerInvariant()) {
        'running' { 'running' }
        'off' { 'stopped' }
        'paused' { 'paused' }
        'saved' { 'saved' }
        default { $State.ToLowerInvariant() }
    }
}

function Test-PcvManagedVm {
    param([AllowNull()][string]$Notes)
    if ([string]::IsNullOrWhiteSpace($Notes)) { return $false }
    return ($Notes -match 'managed-by=purecvisor-desktop-node')
}

function Get-PcvVmList {
    $vms = @(Get-VM -ErrorAction Stop)
    $result = New-Object System.Collections.Generic.List[object]

    foreach ($vm in $vms) {
        $memoryAssigned = Get-PcvObjectPropertyValue -Value $vm -Name 'MemoryAssigned'
        $assignedMb = $null
        if ($null -ne $memoryAssigned) {
            $assignedMb = Convert-PcvBytesToMiB -Bytes ([UInt64]$memoryAssigned)
        }

        $generationValue = Get-PcvObjectPropertyValue -Value $vm -Name 'Generation'
        $generation = $null
        if ($null -ne $generationValue) {
            $generation = [int]$generationValue
        }

        $disks = New-Object System.Collections.Generic.List[object]
        try {
            $diskDrives = @(Get-VMHardDiskDrive -VMName $vm.Name -ErrorAction SilentlyContinue)
        }
        catch {
            $diskDrives = @()
        }

        foreach ($drive in $diskDrives) {
            $sizeGb = $null
            try {
                $vhd = Get-VHD -Path $drive.Path -ErrorAction Stop
                $sizeGb = Convert-PcvBytesToGiB -Bytes ([UInt64]$vhd.Size)
            }
            catch {
                $sizeGb = $null
            }
            $disks.Add([pscustomobject][ordered]@{
                kind = 'vhdx'
                path = $drive.Path
                size_gb = $sizeGb
                attached = $true
            })
        }

        $networks = New-Object System.Collections.Generic.List[object]
        try {
            $networkAdapters = @(Get-VMNetworkAdapter -VMName $vm.Name -ErrorAction SilentlyContinue)
        }
        catch {
            $networkAdapters = @()
        }

        foreach ($adapter in $networkAdapters) {
            $networks.Add([pscustomobject][ordered]@{
                switch = $adapter.SwitchName
                mode = if ($adapter.SwitchName -eq 'Default Switch') { 'default-switch' } else { 'hyperv-switch' }
            })
        }

        try {
            $snapshots = @(Get-VMSnapshot -VMName $vm.Name -ErrorAction SilentlyContinue)
        }
        catch {
            $snapshots = @()
        }

        $result.Add([pscustomobject][ordered]@{
            id = $vm.Name
            name = $vm.Name
            platform = 'hyperv'
            guest_family = 'linux'
            state = Get-PcvVmState -State ([string]$vm.State)
            cpu = [ordered]@{
                count = [int]$vm.ProcessorCount
            }
            memory = [ordered]@{
                startup_mb = (Convert-PcvBytesToMiB -Bytes ([UInt64]$vm.MemoryStartup))
                assigned_mb = $assignedMb
                dynamic = $false
            }
            generation = $generation
            storage = @($disks.ToArray())
            network = @($networks.ToArray())
            checkpoints = [ordered]@{
                count = $snapshots.Count
            }
            console = [ordered]@{
                type = 'vmconnect'
                available_local = $true
            }
            managed_by_purecvisor = Test-PcvManagedVm -Notes ([string]$vm.Notes)
        })
    }

    @($result.ToArray())
}

function Get-PcvNetworkInventory {
    $switches = @(Get-VMSwitch -ErrorAction Stop)
    $switchList = New-Object System.Collections.Generic.List[object]

    foreach ($switch in $switches) {
        $allowManagementOs = Get-PcvObjectPropertyValue -Value $switch -Name 'AllowManagementOS'
        if ($null -ne $allowManagementOs) {
            $allowManagementOs = [bool]$allowManagementOs
        }

        $netAdapter = Get-PcvObjectPropertyValue -Value $switch -Name 'NetAdapterInterfaceDescription'
        if ($null -ne $netAdapter) {
            $netAdapter = [string]$netAdapter
        }

        $switchList.Add([pscustomobject][ordered]@{
            name = [string]$switch.Name
            type = ([string]$switch.SwitchType).ToLowerInvariant()
            is_default = ([string]$switch.Name -eq 'Default Switch')
            allow_management_os = $allowManagementOs
            net_adapter_interface_description = $netAdapter
        })
    }

    [ordered]@{
        source = 'hyperv'
        mutating = $false
        switches = @($switchList.ToArray())
    }
}

function Test-PcvVmName {
    param([AllowEmptyString()][AllowNull()][string]$Name)
    return ($Name -match '^[A-Za-z0-9][A-Za-z0-9._-]{0,62}$')
}

function New-PcvFailureResult {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    New-PcvResponse -Ok $false -Operation $Operation -Data $null -ErrorObject (
        New-PcvError -Code $Code -Message $Message -Detail $Detail -Retryable $Retryable
    )
}

function Test-PcvRequiredProperties {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory)][string[]]$Properties
    )

    if ($null -eq $Value) { return $false }
    foreach ($property in $Properties) {
        if (-not $Value.PSObject.Properties.Name.Contains($property)) {
            return $false
        }
    }
    return $true
}

function ConvertTo-PcvInt32 {
    param(
        [AllowNull()]$Value,
        [ref]$Parsed
    )

    $parsedValue = 0
    if (-not [int]::TryParse(([string]$Value), [ref]$parsedValue)) {
        return $false
    }

    $Parsed.Value = $parsedValue
    return $true
}

function Test-PcvVmNotFoundError {
    param([Parameter(Mandatory)][System.Management.Automation.ErrorRecord]$ErrorRecord)

    $category = $ErrorRecord.CategoryInfo.Category
    if ($category -eq [System.Management.Automation.ErrorCategory]::ObjectNotFound) {
        return $true
    }

    $message = [string]$ErrorRecord.Exception.Message
    if ($message -match 'not found|Cannot find|was not found|unable to find|찾을 수 없습니다') {
        return $true
    }

    return $false
}

function Test-PcvVmExists {
    param([Parameter(Mandatory)][string]$Name)

    try {
        Get-VM -Name $Name -ErrorAction Stop | Out-Null
        return [ordered]@{
            exists = $true
            error = $null
        }
    }
    catch {
        $message = $_.Exception.Message
        if (Test-PcvVmNotFoundError -ErrorRecord $_) {
            return [ordered]@{
                exists = $false
                error = $null
            }
        }

        return [ordered]@{
            exists = $false
            error = $message
        }
    }
}

function Get-PcvRequiredVm {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Operation
    )

    try {
        return Get-VM -Name $Name -ErrorAction Stop
    }
    catch {
        if (Test-PcvVmNotFoundError -ErrorRecord $_) {
            throw [System.InvalidOperationException]::new("PCV_VM_NOT_FOUND|VM '$Name' was not found.|Hyper-V Get-VM did not return a VM with this name.")
        }

        throw [System.InvalidOperationException]::new("PCV_VM_LOOKUP_FAILED|VM '$Name' lookup failed.|$($_.Exception.Message)")
    }
}

function New-PcvVmFromIso {
    param([AllowNull()]$Params)

    $operation = 'vm.create'
    if (-not (Test-PcvRequiredProperties -Value $Params -Properties @('name', 'iso_path', 'cpu', 'memory_mb', 'disk_gb'))) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_CREATE_PARAMS_INVALID' -Message 'VM create params are missing or invalid.' -Detail 'Provide name, iso_path, cpu, memory_mb, and disk_gb. Optional fields are vm_root and generation.' -Retryable $false
    }

    $name = [string]$Params.name
    $isoPath = [string]$Params.iso_path
    $cpu = 0
    $memoryMb = 0
    $diskGb = 0
    $generation = 2

    if (-not (ConvertTo-PcvInt32 -Value $Params.cpu -Parsed ([ref]$cpu)) -or
        -not (ConvertTo-PcvInt32 -Value $Params.memory_mb -Parsed ([ref]$memoryMb)) -or
        -not (ConvertTo-PcvInt32 -Value $Params.disk_gb -Parsed ([ref]$diskGb))) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_CREATE_PARAMS_INVALID' -Message 'VM create params are missing or invalid.' -Detail 'cpu, memory_mb, disk_gb, and generation must be numeric integer values.' -Retryable $false
    }

    $vmRoot = if ($Params.PSObject.Properties.Name.Contains('vm_root')) { [string]$Params.vm_root } else { 'D:\PureCVisor\VMs' }
    if ($Params.PSObject.Properties.Name.Contains('generation') -and -not (ConvertTo-PcvInt32 -Value $Params.generation -Parsed ([ref]$generation))) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_CREATE_PARAMS_INVALID' -Message 'VM create params are missing or invalid.' -Detail 'cpu, memory_mb, disk_gb, and generation must be numeric integer values.' -Retryable $false
    }

    if (-not (Test-PcvVmName -Name $name)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$name' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }
    if (-not (Test-Path -LiteralPath $isoPath -PathType Leaf)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_ISO_NOT_FOUND' -Message "ISO '$isoPath' was not found." -Detail 'Provide a local ISO file path visible to the Hyper-V host.' -Retryable $false
    }
    if ($cpu -lt 1 -or $cpu -gt 32) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_CPU_OUT_OF_RANGE' -Message "CPU count '$cpu' is outside the supported spike range." -Detail 'Use a CPU count from 1 through 32.' -Retryable $false
    }
    if ($memoryMb -lt 512 -or $memoryMb -gt 262144) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_MEMORY_OUT_OF_RANGE' -Message "Memory '$memoryMb' MB is outside the supported spike range." -Detail 'Use memory from 512 MB through 262144 MB.' -Retryable $false
    }
    if ($diskGb -lt 8 -or $diskGb -gt 4096) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_DISK_OUT_OF_RANGE' -Message "Disk '$diskGb' GB is outside the supported spike range." -Detail 'Use disk size from 8 GB through 4096 GB.' -Retryable $false
    }
    if ($generation -notin @(1, 2)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_GENERATION_INVALID' -Message "Generation '$generation' is invalid." -Detail 'Use Hyper-V generation 1 or 2.' -Retryable $false
    }

    $hostStatus = Get-PcvHostStatus
    if (-not $hostStatus.supported) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_HOST_NOT_READY' -Message 'The Hyper-V host is not ready for VM creation.' -Detail (($hostStatus.reasons -join ', ')) -Retryable $false
    }

    $vmLookup = Test-PcvVmExists -Name $name
    if ($vmLookup.error) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_LOOKUP_FAILED' -Message "VM '$name' lookup failed." -Detail $vmLookup.error -Retryable $true
    }
    if ($vmLookup.exists) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_ALREADY_EXISTS' -Message "VM '$name' already exists." -Detail 'Choose a different VM name or remove the existing Hyper-V VM.' -Retryable $false
    }

    $vmDir = Join-Path $vmRoot $name
    $vhdPath = Join-Path $vmDir 'disk0.vhdx'
    $steps = New-Object System.Collections.Generic.List[string]
    $vmDirPreExisting = Test-Path -LiteralPath $vmDir -PathType Container
    $vhdPreExisting = Test-Path -LiteralPath $vhdPath -PathType Leaf
    $vmDirCreated = $false
    $vhdCreated = $false
    $vmCreated = $false

    try {
        New-Item -ItemType Directory -Path $vmDir -Force -ErrorAction Stop | Out-Null
        $vmDirCreated = $true
        $steps.Add('Create VM folder')

        New-VHD -Path $vhdPath -SizeBytes ([Int64]$diskGb * 1GB) -Dynamic -ErrorAction Stop | Out-Null
        $vhdCreated = $true
        $steps.Add('Create VHDX')

        New-VM -Name $name -Generation $generation -MemoryStartupBytes ([Int64]$memoryMb * 1MB) -VHDPath $vhdPath -Path $vmDir -ErrorAction Stop | Out-Null
        $vmCreated = $true
        $steps.Add('Create Hyper-V VM')

        Set-VMProcessor -VMName $name -Count $cpu -ErrorAction Stop
        Set-VM -Name $name -Notes 'managed-by=purecvisor-desktop-node' -ErrorAction Stop
        $steps.Add('Set resources')

        Add-VMDvdDrive -VMName $name -Path $isoPath -ErrorAction Stop
        $steps.Add('Attach ISO')

        Connect-VMNetworkAdapter -VMName $name -SwitchName 'Default Switch' -ErrorAction Stop
        $steps.Add('Attach Default Switch')

        if ($generation -eq 2) {
            $dvd = Get-VMDvdDrive -VMName $name -ErrorAction Stop
            Set-VMFirmware -VMName $name -FirstBootDevice $dvd -ErrorAction Stop
            $steps.Add('Set boot order')
        }

        return New-PcvResponse -Ok $true -Operation $operation -Data ([ordered]@{
            name = $name
            vm_dir = $vmDir
            vhd_path = $vhdPath
            iso_path = $isoPath
            switch = 'Default Switch'
            generation = $generation
            steps = @($steps)
        }) -ErrorObject $null
    }
    catch {
        $detail = $_.Exception.Message
        if ($vmCreated) {
            try { Stop-VM -Name $name -TurnOff -Force -ErrorAction SilentlyContinue } catch { }
            try { Remove-VM -Name $name -Force -ErrorAction SilentlyContinue } catch { }
        }

        if ($vmDirCreated -and -not $vmDirPreExisting) {
            try { Remove-Item -LiteralPath $vmDir -Recurse -Force -ErrorAction SilentlyContinue } catch { }
        }
        elseif ($vhdCreated -and -not $vhdPreExisting) {
            try { Remove-Item -LiteralPath $vhdPath -Force -ErrorAction SilentlyContinue } catch { }
        }

        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_CREATE_FAILED' -Message "VM '$name' creation failed." -Detail $detail -Retryable $true
    }
}

function Invoke-PcvVmLifecycle {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][AllowNull()]$Params
    )

    if (-not (Test-PcvRequiredProperties -Value $Params -Properties @('name'))) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_LIFECYCLE_PARAMS_INVALID' -Message 'VM lifecycle params are missing or invalid.' -Detail 'Provide params.name for lifecycle operations.' -Retryable $false
    }

    $name = [string]$Params.name
    if (-not (Test-PcvVmName -Name $name)) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$name' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }

    try {
        Get-PcvRequiredVm -Name $name -Operation $Operation | Out-Null

        switch ($Operation) {
            'vm.start' {
                Start-VM -Name $name -ErrorAction Stop
                $action = 'start'
            }
            'vm.shutdown' {
                Stop-VM -Name $name -Shutdown:$true -ErrorAction Stop
                $action = 'shutdown'
            }
            'vm.poweroff' {
                Stop-VM -Name $name -TurnOff -Force -ErrorAction Stop
                $action = 'poweroff'
            }
            'vm.restart' {
                Restart-VM -Name $name -Force -ErrorAction Stop
                $action = 'restart'
            }
            default {
                return New-PcvFailureResult -Operation $Operation -Code 'PCV_OPERATION_NOT_ALLOWED' -Message "Operation '$Operation' is not a lifecycle operation." -Detail 'Use vm.start, vm.shutdown, vm.poweroff, or vm.restart.' -Retryable $false
            }
        }

        return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{
            name = $name
            action = $action
        }) -ErrorObject $null
    }
    catch {
        $parts = $_.Exception.Message -split '\|', 3
        if ($parts.Count -eq 3 -and $parts[0] -in @('PCV_VM_NOT_FOUND', 'PCV_VM_LOOKUP_FAILED')) {
            return New-PcvFailureResult -Operation $Operation -Code $parts[0] -Message $parts[1] -Detail $parts[2] -Retryable ($parts[0] -eq 'PCV_VM_LOOKUP_FAILED')
        }
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_LIFECYCLE_FAILED' -Message "Lifecycle operation '$Operation' failed for VM '$name'." -Detail $_.Exception.Message -Retryable $true
    }
}

function Invoke-PcvCheckpointOperation {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][AllowNull()]$Params
    )

    if (-not (Test-PcvRequiredProperties -Value $Params -Properties @('vm_name'))) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_PARAMS_INVALID' -Message 'Checkpoint params are missing or invalid.' -Detail 'Provide params.vm_name for checkpoint operations.' -Retryable $false
    }

    $vmName = [string]$Params.vm_name
    if (-not (Test-PcvVmName -Name $vmName)) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$vmName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }

    if ($Operation -in @('checkpoint.create', 'checkpoint.restore', 'checkpoint.delete') -and
        -not (Test-PcvRequiredProperties -Value $Params -Properties @('checkpoint_name'))) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_NAME_INVALID' -Message "Checkpoint name '' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }

    try {
        Get-PcvRequiredVm -Name $vmName -Operation $Operation | Out-Null

        switch ($Operation) {
            'checkpoint.list' {
                $snapshots = @(Get-VMSnapshot -VMName $vmName -ErrorAction Stop)
                $data = @($snapshots | ForEach-Object {
                    $createdAt = $null
                    if ($_.PSObject.Properties.Name.Contains('CreationTime') -and $null -ne $_.CreationTime) {
                        $createdAt = $_.CreationTime.ToString('o')
                    }

                    [ordered]@{
                        name = $_.Name
                        vm_name = $vmName
                        created_at = $createdAt
                    }
                })
                return New-PcvResponse -Ok $true -Operation $Operation -Data $data -ErrorObject $null
            }
            'checkpoint.create' {
                $checkpointName = [string]$Params.checkpoint_name
                if (-not (Test-PcvVmName -Name $checkpointName)) {
                    return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_NAME_INVALID' -Message "Checkpoint name '$checkpointName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
                }
                Checkpoint-VM -Name $vmName -SnapshotName $checkpointName -ErrorAction Stop | Out-Null
                $visible = $false
                for ($attempt = 1; $attempt -le 3; $attempt += 1) {
                    $snapshots = @(Get-VMSnapshot -VMName $vmName -ErrorAction Stop)
                    $visible = [bool]($snapshots | Where-Object { [string]$_.Name -eq $checkpointName } | Select-Object -First 1)
                    if ($visible) {
                        break
                    }
                    if ($attempt -lt 3) {
                        Start-Sleep -Milliseconds 250
                    }
                }
                if (-not $visible) {
                    throw [System.InvalidOperationException]::new("PCV_CHECKPOINT_NOT_VISIBLE|Checkpoint '$checkpointName' was not visible after creation.|Get-VMSnapshot did not return the created checkpoint name after Checkpoint-VM completed.")
                }
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName }) -ErrorObject $null
            }
            'checkpoint.restore' {
                $checkpointName = [string]$Params.checkpoint_name
                if (-not (Test-PcvVmName -Name $checkpointName)) {
                    return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_NAME_INVALID' -Message "Checkpoint name '$checkpointName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
                }
                Restore-VMSnapshot -VMName $vmName -Name $checkpointName -Confirm:$false -ErrorAction Stop
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName; action = 'restore' }) -ErrorObject $null
            }
            'checkpoint.delete' {
                $checkpointName = [string]$Params.checkpoint_name
                if (-not (Test-PcvVmName -Name $checkpointName)) {
                    return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_NAME_INVALID' -Message "Checkpoint name '$checkpointName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
                }
                Remove-VMSnapshot -VMName $vmName -Name $checkpointName -Confirm:$false -ErrorAction Stop
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName; action = 'delete' }) -ErrorObject $null
            }
            default {
                return New-PcvFailureResult -Operation $Operation -Code 'PCV_OPERATION_NOT_ALLOWED' -Message "Operation '$Operation' is not a checkpoint operation." -Detail 'Use checkpoint.list, checkpoint.create, checkpoint.restore, or checkpoint.delete.' -Retryable $false
            }
        }
    }
    catch {
        $parts = $_.Exception.Message -split '\|', 3
        if ($parts.Count -eq 3 -and $parts[0] -in @('PCV_VM_NOT_FOUND', 'PCV_VM_LOOKUP_FAILED')) {
            return New-PcvFailureResult -Operation $Operation -Code $parts[0] -Message $parts[1] -Detail $parts[2] -Retryable ($parts[0] -eq 'PCV_VM_LOOKUP_FAILED')
        }
        if ($parts.Count -eq 3 -and $parts[0] -eq 'PCV_CHECKPOINT_NOT_VISIBLE') {
            return New-PcvFailureResult -Operation $Operation -Code $parts[0] -Message $parts[1] -Detail $parts[2] -Retryable $true
        }
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_FAILED' -Message "Checkpoint operation '$Operation' failed for VM '$vmName'." -Detail $_.Exception.Message -Retryable $true
    }
}

function Invoke-PcvOperation {
    param([Parameter(Mandatory)]$Request)

    $operation = [string]$Request.operation
    $allowed = @(
        'host.status',
        'vm.list',
        'network.inventory',
        'vm.create',
        'vm.start',
        'vm.shutdown',
        'vm.poweroff',
        'vm.restart',
        'checkpoint.list',
        'checkpoint.create',
        'checkpoint.restore',
        'checkpoint.delete'
    )

    if ($allowed -notcontains $operation) {
        return New-PcvResponse -Ok $false -Operation $operation -Data $null -ErrorObject (
            New-PcvError `
                -Code 'PCV_OPERATION_NOT_ALLOWED' `
                -Message "Operation '$operation' is not allowed." `
                -Detail 'The runner only dispatches the fixed Desktop Node Hyper-V spike operation allowlist.' `
                -Retryable $false
        )
    }

    if ($operation -eq 'host.status') {
        return New-PcvResponse -Ok $true -Operation $operation -Data (Get-PcvHostStatus) -ErrorObject $null
    }

    if ($operation -eq 'vm.list') {
        try {
            return New-PcvResponse -Ok $true -Operation $operation -Data (Get-PcvVmList) -ErrorObject $null
        }
        catch {
            return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_LIST_FAILED' -Message 'VM inventory failed.' -Detail $_.Exception.Message -Retryable $true
        }
    }

    if ($operation -eq 'network.inventory') {
        try {
            return New-PcvResponse -Ok $true -Operation $operation -Data (Get-PcvNetworkInventory) -ErrorObject $null
        }
        catch {
            return New-PcvFailureResult -Operation $operation -Code 'PCV_NETWORK_INVENTORY_FAILED' -Message 'Network inventory failed.' -Detail $_.Exception.Message -Retryable $true
        }
    }

    if ($operation -eq 'vm.create') {
        $params = if ($Request.PSObject.Properties.Name.Contains('params')) { $Request.params } else { $null }
        return New-PcvVmFromIso -Params $params
    }

    $params = if ($Request.PSObject.Properties.Name.Contains('params')) { $Request.params } else { $null }

    if ($operation -in @('vm.start', 'vm.shutdown', 'vm.poweroff', 'vm.restart')) {
        return Invoke-PcvVmLifecycle -Operation $operation -Params $params
    }

    if ($operation -in @('checkpoint.list', 'checkpoint.create', 'checkpoint.restore', 'checkpoint.delete')) {
        return Invoke-PcvCheckpointOperation -Operation $operation -Params $params
    }

    return New-PcvResponse -Ok $false -Operation $operation -Data $null -ErrorObject (
        New-PcvError `
            -Code 'PCV_OPERATION_NOT_IMPLEMENTED' `
            -Message "Operation '$operation' is allowed but is not implemented in this task." `
            -Detail 'Implement the operation in the dedicated follow-up task before using it.' `
            -Retryable $false
    )
}

Export-ModuleMember -Function `
    New-PcvError, `
    New-PcvResponse, `
    ConvertTo-PcvJson, `
    Test-PcvAdmin, `
    Get-PcvWindowsEdition, `
    Test-PcvHyperVCmdletsAvailable, `
    Get-PcvHostStatus, `
    Convert-PcvBytesToMiB, `
    Convert-PcvBytesToGiB, `
    Get-PcvVmState, `
    Test-PcvManagedVm, `
    Get-PcvVmList, `
    Get-PcvNetworkInventory, `
    Test-PcvVmName, `
    New-PcvFailureResult, `
    Test-PcvRequiredProperties, `
    ConvertTo-PcvInt32, `
    Test-PcvVmNotFoundError, `
    Test-PcvVmExists, `
    Get-PcvRequiredVm, `
    New-PcvVmFromIso, `
    Invoke-PcvVmLifecycle, `
    Invoke-PcvCheckpointOperation, `
    Invoke-PcvOperation

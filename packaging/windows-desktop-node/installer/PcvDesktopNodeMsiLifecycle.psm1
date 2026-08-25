Set-StrictMode -Version Latest

function Resolve-PcvMsiLifecycleFullPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_MSI_LIFECYCLE_PATH_EMPTY|Path must not be empty.'
    }

    [System.IO.Path]::GetFullPath($Path)
}

function New-PcvMsiLifecycleStep {
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [ValidateSet('Install', 'Repair', 'Uninstall', 'InstallRemoveData', 'UninstallRemoveData')]
        [string]$Phase,

        [Parameter(Mandatory)]
        [string[]]$MsiArguments,

        [Parameter(Mandatory)]
        [string]$LogPath,

        [int[]]$SuccessExitCodes = @(0),

        [int[]]$ConditionalExitCodes = @()
    )

    $arguments = @($MsiArguments) + @(
        'REBOOT=ReallySuppress',
        'MSIRESTARTMANAGERCONTROL=Disable',
        '/qn',
        '/norestart',
        '/l*vx',
        $LogPath
    )

    [pscustomobject][ordered]@{
        name = $Name
        phase = $Phase
        file_path = 'msiexec.exe'
        arguments = @($arguments)
        success_exit_codes = @($SuccessExitCodes)
        conditional_exit_codes = @($ConditionalExitCodes)
        mutates_host = $true
    }
}

function New-PcvMsiLifecycleSmokePlan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$MsiPath,

        [Parameter(Mandatory)]
        [string]$LogDirectory
    )

    $fullMsiPath = Resolve-PcvMsiLifecycleFullPath -Path $MsiPath
    $fullLogDirectory = Resolve-PcvMsiLifecycleFullPath -Path $LogDirectory

    $steps = @(
        New-PcvMsiLifecycleStep `
            -Name 'install' `
            -Phase Install `
            -MsiArguments @('/i', $fullMsiPath) `
            -LogPath (Join-Path $fullLogDirectory 'install.log')

        New-PcvMsiLifecycleStep `
            -Name 'repair' `
            -Phase Repair `
            -MsiArguments @('/i', $fullMsiPath, 'REINSTALL=ALL', 'REINSTALLMODE=vomus') `
            -LogPath (Join-Path $fullLogDirectory 'repair.log') `
            -SuccessExitCodes @(0) `
            -ConditionalExitCodes @(3010)

        New-PcvMsiLifecycleStep `
            -Name 'uninstall-preserve' `
            -Phase Uninstall `
            -MsiArguments @('/x', $fullMsiPath) `
            -LogPath (Join-Path $fullLogDirectory 'uninstall-preserve.log')

        New-PcvMsiLifecycleStep `
            -Name 'install-remove-data' `
            -Phase InstallRemoveData `
            -MsiArguments @('/i', $fullMsiPath) `
            -LogPath (Join-Path $fullLogDirectory 'install-remove-data.log')

        New-PcvMsiLifecycleStep `
            -Name 'uninstall-remove-data' `
            -Phase UninstallRemoveData `
            -MsiArguments @('/x', $fullMsiPath, 'REMOVE_DATA=1') `
            -LogPath (Join-Path $fullLogDirectory 'uninstall-remove-data.log')
    )

    [pscustomobject][ordered]@{
        schema_version = 1
        msi_path = $fullMsiPath
        log_directory = $fullLogDirectory
        no_auto_reboot = [pscustomobject][ordered]@{
            enabled = $true
            reboot_property = 'ReallySuppress'
            restart_manager_control = 'Disable'
            norestart_argument = '/norestart'
            reboot_initiated_exit_code = 1641
        }
        repair_contract = [pscustomobject][ordered]@{
            command_shape = 'install-reinstall-properties'
            forced_file_overwrite = $false
            restart_suppression = @('REBOOT=ReallySuppress', 'MSIRESTARTMANAGERCONTROL=Disable', '/norestart')
        }
        steps = @($steps)
    }
}

function ConvertTo-PcvMsiLifecycleExitClassification {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Install', 'Repair', 'Uninstall', 'InstallRemoveData', 'UninstallRemoveData')]
        [string]$Phase,

        [Parameter(Mandatory)]
        [int]$ExitCode,

        [switch]$AssertionsPassed
    )

    if ($ExitCode -eq 0) {
        return [pscustomobject][ordered]@{
            ok = $true
            phase = $Phase
            exit_code = $ExitCode
            result = 'success'
            reboot_required = $false
            actual_reboot_initiated = $false
            requires_post_reboot_verification = $false
        }
    }

    if ($ExitCode -eq 1641) {
        return [pscustomobject][ordered]@{
            ok = $false
            phase = $Phase
            exit_code = $ExitCode
            result = 'reboot_initiated_failure'
            reboot_required = $true
            actual_reboot_initiated = $true
            requires_post_reboot_verification = $true
        }
    }

    if ($ExitCode -eq 3010 -and $Phase -eq 'Repair') {
        return [pscustomobject][ordered]@{
            ok = [bool]$AssertionsPassed
            phase = $Phase
            exit_code = $ExitCode
            result = if ($AssertionsPassed) { 'reboot_required_success' } else { 'reboot_required_pending_assertions' }
            reboot_required = $true
            actual_reboot_initiated = $false
            requires_post_reboot_verification = $true
        }
    }

    [pscustomobject][ordered]@{
        ok = $false
        phase = $Phase
        exit_code = $ExitCode
        result = 'unexpected_exit_code'
        reboot_required = $ExitCode -eq 3010
        actual_reboot_initiated = $false
        requires_post_reboot_verification = $ExitCode -eq 3010
    }
}

Export-ModuleMember -Function @(
    'New-PcvMsiLifecycleSmokePlan',
    'ConvertTo-PcvMsiLifecycleExitClassification'
)

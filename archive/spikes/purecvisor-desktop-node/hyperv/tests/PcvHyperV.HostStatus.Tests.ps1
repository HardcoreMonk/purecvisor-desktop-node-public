Describe 'Get-PcvHostStatus' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvHyperV.psm1'
        $script:CreatedGetVMSwitchPlaceholder = $false

        if (-not (Get-Command Get-VMSwitch -ErrorAction SilentlyContinue)) {
            Set-Item -Path Function:global:Get-VMSwitch -Value { }
            $script:CreatedGetVMSwitchPlaceholder = $true
        }
    }

    AfterAll {
        if ($script:CreatedGetVMSwitchPlaceholder) {
            Remove-Item -Path Function:global:Get-VMSwitch -ErrorAction SilentlyContinue
        }
    }

    BeforeEach {
        Import-Module $ModulePath -Force

        Mock Test-PcvAdmin { $true } -ModuleName PcvHyperV
        Mock Test-PcvHyperVCmdletsAvailable { $false } -ModuleName PcvHyperV
        Mock Get-ComputerInfo {
            [pscustomobject]@{
                WindowsProductName = 'Windows 11 Pro'
                WindowsVersion = '23H2'
                OsHardwareAbstractionLayer = '10.0.22631.1'
            }
        } -ModuleName PcvHyperV
        Mock Get-WindowsOptionalFeature {
            param([switch]$Online, [string]$FeatureName)

            [pscustomobject]@{
                FeatureName = 'Microsoft-Hyper-V'
                State = 'Enabled'
            }
        } -ModuleName PcvHyperV
        Mock Get-Service {
            param($Name)

            [pscustomobject]@{
                Name = 'vmms'
                Status = 'Running'
            }
        } -ModuleName PcvHyperV
        Mock Get-VMSwitch {
            @(
                [pscustomobject]@{ Name = 'Default Switch'; SwitchType = 'Internal' }
            )
        } -ModuleName PcvHyperV
    }

    It 'reports Windows, admin, Hyper-V, VMMS, and Default Switch state' {
        $status = Get-PcvHostStatus

        $status.windows.caption | Should -Be 'Windows 11 Pro'
        $status.windows.version | Should -Be '23H2'
        $status.windows.edition | Should -Be 'Pro'
        $status.admin.elevated | Should -BeTrue
        $status.hyperv.feature_enabled | Should -BeTrue
        $status.hyperv.vmms_running | Should -BeTrue
        $status.hyperv.default_switch_present | Should -BeTrue
    }

    It 'treats available Hyper-V cmdlets as enabled when optional feature status is unavailable' {
        Mock Test-PcvHyperVCmdletsAvailable { $true } -ModuleName PcvHyperV
        Mock Get-WindowsOptionalFeature {
            throw 'Get-WindowsOptionalFeature unavailable'
        } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.hyperv.feature_enabled | Should -BeTrue
        $status.reasons | Should -Not -Contain 'PCV_HYPERV_FEATURE_UNKNOWN'
        Should -Invoke Get-WindowsOptionalFeature -Times 0 -ModuleName PcvHyperV
    }

    It 'marks unsupported Windows Home as unsupported' {
        Mock Get-ComputerInfo {
            [pscustomobject]@{
                WindowsProductName = 'Windows 11 Home'
                WindowsVersion = '23H2'
                OsHardwareAbstractionLayer = '10.0.22631.1'
            }
        } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.windows.edition | Should -Be 'Home'
        $status.supported | Should -BeFalse
        $status.reasons | Should -Contain 'PCV_WINDOWS_EDITION_UNSUPPORTED'
    }

    It 'keeps working when Hyper-V cmdlets are unavailable' {
        Mock Get-WindowsOptionalFeature {
            param([switch]$Online, [string]$FeatureName)

            throw 'Get-WindowsOptionalFeature unavailable'
        } -ModuleName PcvHyperV
        Mock Get-VMSwitch { throw 'Get-VMSwitch unavailable' } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.hyperv.feature_enabled | Should -BeFalse
        $status.hyperv.default_switch_present | Should -BeFalse
        $status.reasons | Should -Contain 'PCV_HYPERV_FEATURE_UNKNOWN'
        $status.reasons | Should -Contain 'PCV_DEFAULT_SWITCH_UNKNOWN'
    }

    It 'keeps working when admin detection fails' {
        Mock Test-PcvAdmin { throw 'Admin detection unavailable' } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.admin.elevated | Should -BeFalse
        $status.supported | Should -BeFalse
        $status.reasons | Should -Contain 'PCV_ADMIN_UNKNOWN'
    }
}

BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:LifecycleModule = Join-Path $script:InstallerRoot 'PcvDesktopNodeMsiLifecycle.psm1'
    Import-Module $script:LifecycleModule -Force
}

Describe 'Desktop Node MSI lifecycle smoke contract' {
    It 'plans repair through explicit reinstall properties instead of /fa force-all shorthand' {
        $msi = Join-Path $TestDrive 'PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi'
        Set-Content -LiteralPath $msi -Value 'fake-msi' -NoNewline

        $plan = New-PcvMsiLifecycleSmokePlan `
            -MsiPath $msi `
            -LogDirectory (Join-Path $TestDrive 'logs')

        $repair = $plan.steps | Where-Object { $_.phase -eq 'Repair' } | Select-Object -First 1

        $repair | Should -Not -BeNullOrEmpty
        $repair.file_path | Should -Be 'msiexec.exe'
        $repair.arguments | Should -Contain '/i'
        $repair.arguments | Should -Not -Contain '/fa'
        $repair.arguments | Should -Contain 'REINSTALL=ALL'
        $repair.arguments | Should -Contain 'REINSTALLMODE=vomus'
        $repair.arguments | Should -Contain 'REBOOT=ReallySuppress'
        $repair.arguments | Should -Contain 'MSIRESTARTMANAGERCONTROL=Disable'
        $repair.arguments | Should -Contain '/norestart'
    }

    It 'suppresses reboot and Restart Manager actions for every mutating MSI step' {
        $msi = Join-Path $TestDrive 'PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi'
        Set-Content -LiteralPath $msi -Value 'fake-msi' -NoNewline

        $plan = New-PcvMsiLifecycleSmokePlan `
            -MsiPath $msi `
            -LogDirectory (Join-Path $TestDrive 'logs')

        $plan.steps.Count | Should -Be 5
        foreach ($step in $plan.steps) {
            $step.mutates_host | Should -BeTrue
            $step.arguments | Should -Contain 'REBOOT=ReallySuppress'
            $step.arguments | Should -Contain 'MSIRESTARTMANAGERCONTROL=Disable'
            $step.arguments | Should -Contain '/norestart'
            $step.arguments | Should -Contain '/l*vx'
        }

        $plan.no_auto_reboot.enabled | Should -BeTrue
        $plan.no_auto_reboot.restart_manager_control | Should -Be 'Disable'
        $plan.no_auto_reboot.reboot_property | Should -Be 'ReallySuppress'
        $plan.no_auto_reboot.reboot_initiated_exit_code | Should -Be 1641
    }

    It 'classifies repair 3010 as success only after preservation assertions pass' {
        $pending = ConvertTo-PcvMsiLifecycleExitClassification `
            -Phase Repair `
            -ExitCode 3010

        $pending.ok | Should -BeFalse
        $pending.result | Should -Be 'reboot_required_pending_assertions'
        $pending.reboot_required | Should -BeTrue
        $pending.actual_reboot_initiated | Should -BeFalse

        $accepted = ConvertTo-PcvMsiLifecycleExitClassification `
            -Phase Repair `
            -ExitCode 3010 `
            -AssertionsPassed

        $accepted.ok | Should -BeTrue
        $accepted.result | Should -Be 'reboot_required_success'
        $accepted.reboot_required | Should -BeTrue
        $accepted.requires_post_reboot_verification | Should -BeTrue
    }

    It 'keeps repair 3010 out of unconditional success exit codes' {
        $msi = Join-Path $TestDrive 'PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi'
        Set-Content -LiteralPath $msi -Value 'fake-msi' -NoNewline

        $plan = New-PcvMsiLifecycleSmokePlan `
            -MsiPath $msi `
            -LogDirectory (Join-Path $TestDrive 'logs')

        $repair = $plan.steps | Where-Object { $_.phase -eq 'Repair' } | Select-Object -First 1

        $repair.success_exit_codes | Should -Contain 0
        $repair.success_exit_codes | Should -Not -Contain 3010
        $repair.conditional_exit_codes | Should -Contain 3010
    }

    It 'classifies 1641 as reboot-initiated failure for every lifecycle phase' {
        foreach ($phase in @('Install', 'Repair', 'Uninstall', 'InstallRemoveData', 'UninstallRemoveData')) {
            $classification = ConvertTo-PcvMsiLifecycleExitClassification `
                -Phase $phase `
                -ExitCode 1641 `
                -AssertionsPassed

            $classification.ok | Should -BeFalse
            $classification.result | Should -Be 'reboot_initiated_failure'
            $classification.actual_reboot_initiated | Should -BeTrue
            $classification.requires_post_reboot_verification | Should -BeTrue
        }
    }
}

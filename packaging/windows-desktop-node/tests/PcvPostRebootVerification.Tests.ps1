Set-StrictMode -Version Latest

Describe 'PcvPostRebootVerification dry-run contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1'
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force
    }

    It 'builds the PackagingRegression profile without host mutation commands' {
        $profile = New-PcvPostRebootCommandProfile `
            -Profile PackagingRegression `
            -RepoRoot $script:RepoRoot

        $profile.profile | Should -Be 'PackagingRegression'
        $profile.commands.Count | Should -Be 3
        $profile.commands[0].id | Should -Be 'packaging-product-tests'
        $profile.commands[0].file_name | Should -Be 'pwsh'
        ($profile.commands[0].arguments -join ' ') | Should -Match 'Invoke-Pester'
        $profile.commands[1].id | Should -Be 'packaging-installer-tests'
        $profile.commands[2].id | Should -Be 'git-diff-check'
        ($profile.commands | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'keeps active post-reboot profiles product-owned without spike command paths' {
        foreach ($profileName in @('ProductStatus', 'PackagingRegression')) {
            $profile = New-PcvPostRebootCommandProfile `
                -Profile $profileName `
                -RepoRoot $script:RepoRoot

            ($profile.commands | ConvertTo-Json -Depth 12) | Should -Not -Match 'spikes[\\/]purecvisor-desktop-node'
        }
    }

    It 'retires the HyperVNonIntegration profile from active post-reboot verification' {
        {
            New-PcvPostRebootCommandProfile `
                -Profile HyperVNonIntegration `
                -RepoRoot $script:RepoRoot
        } | Should -Throw -ExpectedMessage '*PCV_POST_REBOOT_PROFILE_RETIRED*'
    }

    It 'builds a LocalSystemAtStartup state file contract for repo-local commands' {
        $evidenceDir = Join-Path $TestDrive 'evidence'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile PackagingRegression `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-test' `
            -PrincipalMode LocalSystemAtStartup

        $state.schema_version | Should -Be 1
        $state.phase_id | Should -Be 'phase23'
        $state.repo_root | Should -Be $script:RepoRoot
        $state.evidence_dir | Should -Be $evidenceDir
        $state.principal.mode | Should -Be 'LocalSystemAtStartup'
        $state.commands.Count | Should -Be 3
        $state.cleanup.unregister_task | Should -BeTrue
    }

    It 'rejects LocalSystemAtStartup when user profile resources are required' {
        {
            New-PcvPostRebootVerificationState `
                -PhaseId 'phase20' `
                -RepoRoot $script:RepoRoot `
                -EvidenceDir (Join-Path $TestDrive 'evidence-user-profile') `
                -Profile ProductStatus `
                -TaskName 'PureCVisorDesktopNode-PostRebootVerification-user-profile' `
                -PrincipalMode LocalSystemAtStartup `
                -RequiresUserProfile
        } | Should -Throw -ExpectedMessage '*PCV_POST_REBOOT_PRINCIPAL_NOT_ALLOWED*'
    }

    It 'redacts bearer tokens, secret keys, and known paths from text' {
        $text = @'
Authorization: Bearer abc.def.secret
{"api_token":"raw-token","nested":{"password":"pw"},"path":"C:\ProgramData\PureCVisor\desktop-node"}
api_token=plain-token
password: plain-password
-ApiToken cli-secret -ApiTokenProtectedFile C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json
D:\data\projects\codex-zone\purecvisor-desktop-node
'@
        $redacted = ConvertTo-PcvPostRebootRedactedText `
            -Text $text `
            -PathRedactions @{
                'C:\ProgramData\PureCVisor\desktop-node' = '[DATA_ROOT]'
                'D:\data\projects\codex-zone\purecvisor-desktop-node' = '[REPO_ROOT]'
            }

        $redacted | Should -Match 'Bearer \[REDACTED\]'
        $redacted | Should -Match '"api_token":"\[REDACTED\]"'
        $redacted | Should -Match '"password":"\[REDACTED\]"'
        $redacted | Should -Match 'api_token=\[REDACTED\]'
        $redacted | Should -Match 'password: \[REDACTED\]'
        $redacted | Should -Match '-ApiToken \[REDACTED\]'
        $redacted | Should -Match '-ApiTokenProtectedFile \[REDACTED\]'
        $redacted | Should -Match '\[DATA_ROOT\]'
        $redacted | Should -Match '\[REPO_ROOT\]'
        $redacted | Should -Not -Match 'abc\.def\.secret|raw-token|"pw"|plain-token|plain-password|cli-secret|api-token\.dpapi\.json|D:\\data\\projects'
    }

    It 'normalizes relative evidence paths before storing state and scheduled task arguments' {
        Push-Location $TestDrive
        try {
            $result = Initialize-PcvPostRebootVerification `
                -PhaseId 'phase23' `
                -RepoRoot $script:RepoRoot `
                -EvidenceDir 'relative-evidence' `
                -Profile ProductStatus `
                -TaskName 'PureCVisorDesktopNode-PostRebootVerification-relative' `
                -PrincipalMode LocalSystemAtStartup `
                -DryRun
        }
        finally {
            Pop-Location
        }

        [System.IO.Path]::IsPathRooted($result.state_file) | Should -BeTrue
        [System.IO.Path]::IsPathRooted($result.task_plan.state_file) | Should -BeTrue
        $result.task_plan.action_arguments | Should -Match ([regex]::Escape($result.state_file))
        $state = Get-Content -LiteralPath $result.state_file -Raw | ConvertFrom-Json
        [System.IO.Path]::IsPathRooted([string]$state.evidence_dir) | Should -BeTrue
    }

    It 'builds a LocalSystem AtStartup scheduled task plan' {
        $stateFile = Join-Path $TestDrive 'post-reboot-state.json'
        $runner = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1'

        $plan = New-PcvPostRebootScheduledTaskPlan `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-test' `
            -StateFile $stateFile `
            -RunnerScript $runner `
            -PrincipalMode LocalSystemAtStartup

        $plan.task_name | Should -Be 'PureCVisorDesktopNode-PostRebootVerification-test'
        $plan.trigger | Should -Be 'AtStartup'
        $plan.principal_user_id | Should -Be 'SYSTEM'
        $plan.run_level | Should -Be 'Highest'
        $plan.action_file_name | Should -Be 'pwsh.exe'
        $plan.action_arguments | Should -Match 'Invoke-PcvPostRebootVerification\.ps1'
        $plan.action_arguments | Should -Match '-StateFile'
    }

    It 'writes a state file without registering a task in dry-run mode' {
        $evidenceDir = Join-Path $TestDrive 'evidence-dry-run'
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        $registrations = [System.Collections.Generic.List[object]]::new()

        $result = Initialize-PcvPostRebootVerification `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-dry-run' `
            -PrincipalMode LocalSystemAtStartup `
            -DryRun `
            -RegisterTask {
                param($TaskPlan)
                $registrations.Add($TaskPlan)
                [ordered]@{ ok = $true }
            }

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeTrue
        $result.state_file | Should -Be $stateFile
        Test-Path -LiteralPath $stateFile | Should -BeTrue
        $registrations.Count | Should -Be 0
        $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
        $state.phase_id | Should -Be 'phase23'
        $state.commands.Count | Should -Be 2
    }

    It 'registers a post-reboot task only through the explicit registration path' {
        $evidenceDir = Join-Path $TestDrive 'evidence-register'
        $registrations = [System.Collections.Generic.List[object]]::new()

        $result = Initialize-PcvPostRebootVerification `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-register' `
            -PrincipalMode LocalSystemAtStartup `
            -ContinuationProfiles PackagingRegression `
            -RegisterTask {
                param($TaskPlan)
                $registrations.Add($TaskPlan)
                [ordered]@{ ok = $true; task_name = $TaskPlan.task_name }
            }

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeFalse
        $registrations.Count | Should -Be 1
        $registrations[0].trigger | Should -Be 'AtStartup'
        $registrations[0].action_arguments | Should -Match 'Invoke-PcvPostRebootVerification\.ps1'
        $registrations[0].action_arguments | Should -Not -Match 'Restart-Computer'

        $state = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-state.json') -Raw | ConvertFrom-Json
        $state.continuation.profiles | Should -Contain 'PackagingRegression'
    }

    It 'adds continuation profile commands to the state contract' {
        $evidenceDir = Join-Path $TestDrive 'evidence-continuation'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-continuation' `
            -PrincipalMode LocalSystemAtStartup `
            -ContinuationProfiles PackagingRegression

        $state.commands.Count | Should -Be 2
        $state.continuation.run_on_success | Should -BeTrue
        $state.continuation.profiles | Should -Contain 'PackagingRegression'
        $state.continuation.commands.Count | Should -Be 3
        $state.continuation.commands[0].id | Should -Be 'continuation-PackagingRegression-packaging-product-tests'
        ($state.continuation.commands | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'runs the pre-reboot entrypoint in dry-run mode without task registration' {
        $evidenceDir = Join-Path $TestDrive 'entrypoint-dry-run'
        $entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1'

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $entrypoint `
            -PhaseId phase23 `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -DryRun |
            ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.dry_run | Should -BeTrue
        $json.registration | Should -BeNullOrEmpty
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-state.json') | Should -BeTrue
    }

    It 'rejects automatic reboot requests at the entrypoint' {
        $evidenceDir = Join-Path $TestDrive 'entrypoint-reboot-disabled'
        $entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1'

        $output = & pwsh -NoProfile -ExecutionPolicy Bypass -File $entrypoint `
            -PhaseId phase23 `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -DryRun `
            -Reboot 2>&1

        $LASTEXITCODE | Should -Not -Be 0
        ($output -join "`n") | Should -Match 'PCV_POST_REBOOT_AUTO_REBOOT_DISABLED'
        ($output -join "`n") | Should -Not -Match 'Restart-Computer'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-state.json') | Should -BeFalse
    }

    It 'rejects retired HyperVNonIntegration profile at the entrypoint' {
        $evidenceDir = Join-Path $TestDrive 'entrypoint-retired-profile'
        $entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1'

        $output = & pwsh -NoProfile -ExecutionPolicy Bypass -File $entrypoint `
            -PhaseId phase21 `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile HyperVNonIntegration `
            -DryRun 2>&1

        $LASTEXITCODE | Should -Not -Be 0
        ($output -join "`n") | Should -Match 'PCV_POST_REBOOT_PROFILE_RETIRED'
        ($output -join "`n") | Should -Not -Match 'spikes[\\/]purecvisor-desktop-node/hyperv/tests'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-state.json') | Should -BeFalse
    }
}

Describe 'PcvPostRebootVerification runner evidence' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1'
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force
    }

    It 'runs commands, writes redacted artifacts, and unregisters the task' {
        $evidenceDir = Join-Path $TestDrive 'runner-evidence'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-runner' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $calls = [System.Collections.Generic.List[object]]::new()
        $unregistered = [System.Collections.Generic.List[string]]::new()
        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            $calls.Add([pscustomobject]@{
                file = $FileName
                args = $Arguments
                cwd = $WorkingDirectory
                timeout = $TimeoutSeconds
            })
            [ordered]@{
                exit_code = 0
                stdout = '{"Authorization":"Bearer abc.def.secret","api_token":"raw-token"}'
                stderr = ''
                timed_out = $false
                duration_ms = 25
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { ' M packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1' } `
            -UnregisterTask { param([string]$TaskName) $unregistered.Add($TaskName); [ordered]@{ ok = $true } }

        $result.ok | Should -BeTrue
        $calls.Count | Should -Be 2
        $unregistered | Should -Contain 'PureCVisorDesktopNode-PostRebootVerification-runner'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-summary.md') | Should -BeTrue
        $stdout = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-stdout-product-status.log') -Raw
        $stdout | Should -Match '"Authorization":"\[REDACTED\]"'
        $stdout | Should -Match '"api_token":"\[REDACTED\]"'
        $stdout | Should -Not -Match 'abc\.def\.secret|raw-token'
    }

    It 'runs continuation commands after post-reboot verification succeeds' {
        $evidenceDir = Join-Path $TestDrive 'runner-continuation'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-continuation' `
            -PrincipalMode LocalSystemAtStartup `
            -ContinuationProfiles PackagingRegression
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $calls = [System.Collections.Generic.List[object]]::new()
        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            $calls.Add([pscustomobject]@{
                file = $FileName
                args = $Arguments
                cwd = $WorkingDirectory
                timeout = $TimeoutSeconds
            })
            [ordered]@{
                exit_code = 0
                stdout = 'ok'
                stderr = ''
                timed_out = $false
                duration_ms = 25
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask { param([string]$TaskName) [ordered]@{ ok = $true } }

        $result.ok | Should -BeTrue
        $calls.Count | Should -Be 5
        $result.commands.Count | Should -Be 2
        $result.continuation.skipped | Should -BeFalse
        $result.continuation.commands.Count | Should -Be 3
        $result.continuation.commands[0].id | Should -Be 'continuation-PackagingRegression-packaging-product-tests'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-stdout-continuation-PackagingRegression-packaging-product-tests.log') | Should -BeTrue
    }

    It 'skips continuation commands when post-reboot verification fails' {
        $evidenceDir = Join-Path $TestDrive 'runner-continuation-skip'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-continuation-skip' `
            -PrincipalMode LocalSystemAtStartup `
            -ContinuationProfiles PackagingRegression
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $calls = [System.Collections.Generic.List[object]]::new()
        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            $calls.Add([pscustomobject]@{ file = $FileName; args = $Arguments })
            [ordered]@{
                exit_code = 1
                stdout = ''
                stderr = 'product status failed'
                timed_out = $false
                duration_ms = 10
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask { param([string]$TaskName) [ordered]@{ ok = $true } }

        $result.ok | Should -BeFalse
        $calls.Count | Should -Be 2
        $result.continuation.skipped | Should -BeTrue
        $result.continuation.reason | Should -Be 'verification-failed'
    }

    It 'marks required command failure as overall failure and still writes evidence' {
        $evidenceDir = Join-Path $TestDrive 'runner-failure'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile PackagingRegression `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-failure' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            [ordered]@{
                exit_code = 1
                stdout = ''
                stderr = 'Pester failed'
                timed_out = $false
                duration_ms = 10
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask { param([string]$TaskName) [ordered]@{ ok = $true } }

        $result.ok | Should -BeFalse
        $json = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json') -Raw | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.commands[0].exit_code | Should -Be 1
    }

    It 'does not rerun commands after completion and still unregisters the task' {
        $evidenceDir = Join-Path $TestDrive 'runner-already-complete'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-already-complete' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state
        Write-PcvPostRebootJsonFile `
            -Path (Join-Path $evidenceDir 'post-reboot-complete.json') `
            -InputObject ([ordered]@{ completed_at_utc = '2026-04-29T01:02:03.0000000Z' })

        $unregistered = [System.Collections.Generic.List[string]]::new()
        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess { throw 'commands should not run after completion marker exists' } `
            -UnregisterTask { param([string]$TaskName) $unregistered.Add($TaskName); [ordered]@{ ok = $true } }

        $result.ok | Should -BeTrue
        $result.already_completed | Should -BeTrue
        $null -eq $result.completed_at_utc | Should -BeFalse
        $result.cleanup.ok | Should -BeTrue
        $unregistered | Should -Contain 'PureCVisorDesktopNode-PostRebootVerification-already-complete'
    }

    It 'records cleanup failure without losing command evidence' {
        $evidenceDir = Join-Path $TestDrive 'runner-cleanup-failure'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-cleanup-failure' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            [ordered]@{
                exit_code = 0
                stdout = 'ok'
                stderr = ''
                timed_out = $false
                duration_ms = 10
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask { param([string]$TaskName) throw "missing task $TaskName" }

        $result.ok | Should -BeFalse
        $result.cleanup.ok | Should -BeFalse
        $result.cleanup.error | Should -Match 'missing task'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-stdout-product-status.log') | Should -BeTrue
    }

    It 'persists completion artifacts before unregistering the scheduled task' {
        $evidenceDir = Join-Path $TestDrive 'runner-cleanup-order'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-cleanup-order' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            [ordered]@{
                exit_code = 0
                stdout = 'ok'
                stderr = ''
                timed_out = $false
                duration_ms = 10
            }
        }

        $persistedArtifactChecks = [System.Collections.Generic.List[bool]]::new()
        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask {
                param([string]$TaskName)
                $persistedArtifactChecks.Add(
                    (Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json')) -and
                    (Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-summary.md')) -and
                    (Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-complete.json'))
                )
                [ordered]@{ ok = $true }
            }

        $result.ok | Should -BeTrue
        $persistedArtifactChecks.Count | Should -Be 1
        $persistedArtifactChecks[0] | Should -BeTrue
        $complete = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-complete.json') -Raw | ConvertFrom-Json
        $complete.cleanup_completed | Should -BeTrue
        $complete.cleanup_ok | Should -BeTrue
    }
}

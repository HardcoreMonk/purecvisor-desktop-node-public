BeforeAll {
    $script:SelectorModulePath = Join-Path $PSScriptRoot '../tools/PcvDevelopmentVerification.psm1'
    $script:RunnerModulePath = Join-Path $PSScriptRoot '../tools/PcvDevelopmentVerificationRunner.psm1'
    Import-Module $script:SelectorModulePath -Force
    if (Test-Path -LiteralPath $script:RunnerModulePath -PathType Leaf) {
        Import-Module $script:RunnerModulePath -Force
    }
}

Describe 'Invoke-PcvDevelopmentVerification' {
    It 'records selected, skipped and failed suites without hiding scope' {
        Get-Command Invoke-PcvDevelopmentVerification -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $calls = [System.Collections.Generic.List[string]]::new()
        $runner = {
            param($Suite, $FileName, $Arguments, $WorkingDirectory)
            $calls.Add($Suite)
            [pscustomobject]@{
                exit_code = if ($Suite -eq 'web-pester') { 7 } else { 0 }
                duration_ms = 4
                stdout = ''
                stderr = ''
            }
        }

        $result = Invoke-PcvDevelopmentVerification `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('web/src/app.ts') `
            -WorkingDirectory $TestDrive `
            -CommandRunner $runner

        $result.ok | Should -BeFalse
        $result.failed_suite | Should -Be 'web-pester'
        ($result.results | Where-Object suite -eq 'dotnet').status |
            Should -Be 'not-selected-by-scope'
        @($calls) | Should -Be @('web-npm', 'web-pester')
    }

    It 'plans selected suites without invoking the command runner' {
        Get-Command Invoke-PcvDevelopmentVerification -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $calls = 0
        $runner = {
            $script:calls++
            throw 'runner must not execute in PlanOnly'
        }

        $result = Invoke-PcvDevelopmentVerification `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('src/DesktopNode.Api/Program.cs') `
            -WorkingDirectory $TestDrive `
            -PlanOnly `
            -CommandRunner $runner

        $result.ok | Should -BeTrue
        ($result.results | Where-Object suite -eq 'dotnet').status | Should -Be 'planned'
        $calls | Should -Be 0
    }

    It 'defines only non-mutating development suite commands' {
        Get-Command Get-PcvDevelopmentVerificationSuiteCatalog -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $catalog = Get-PcvDevelopmentVerificationSuiteCatalog
        @($catalog.Keys) | Should -Be @(
            'dotnet',
            'web-npm',
            'packaging-pester',
            'installer-pester',
            'web-pester',
            'git-diff-check',
            'current-evidence-check'
        )
        $catalog['current-evidence-check'].file_name | Should -Be 'pwsh'
        @($catalog['current-evidence-check'].arguments) -join ' ' |
            Should -Match 'Update-PcvCurrentEvidenceDocs\.ps1.*-Check'
        ($catalog | ConvertTo-Json -Depth 8) |
            Should -Not -Match 'AllowHostMutation|msiexec|New-VM|Start-Service|package build'
    }
}

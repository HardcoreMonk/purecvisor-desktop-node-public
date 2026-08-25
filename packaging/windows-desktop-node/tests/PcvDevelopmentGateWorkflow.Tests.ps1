Set-StrictMode -Version Latest

Describe 'Development gates workflow contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:WorkflowPath = Join-Path $script:RepoRoot '.github\workflows\development-gates.yml'
    }

    It 'exists and covers the active non-mutating development gates' {
        $script:WorkflowPath | Should -Exist
        $workflow = Get-Content -Raw -LiteralPath $script:WorkflowPath

        $workflow | Should -Match 'name:\s*Development Gates'
        $workflow | Should -Match '(?m)^\s*pull_request:\s*$'
        $workflow | Should -Match '(?m)^\s*push:\s*$'
        $workflow | Should -Match '(?m)^\s*-\s*main\s*$'
        $workflow | Should -Not -Match "(?m)^\s*-\s*'codex/\*\*'\s*$"
        $workflow | Should -Match '(?m)^\s*workflow_dispatch:\s*$'
        $workflow | Should -Match '(?m)^permissions:\s*\r?\n\s*contents:\s*read\s*$'
        $workflow | Should -Match 'cancel-in-progress:\s*true'

        $workflow | Should -Match '(?m)^\s*dotnet-tests:\s*$'
        $workflow | Should -Match '(?m)^\s*web-tests:\s*$'
        $workflow | Should -Match '(?m)^\s*packaging-pester:\s*$'
        $workflow | Should -Match '(?m)^\s*installer-web-pester:\s*$'
        ([regex]::Matches($workflow, 'runs-on:\s*windows-latest')).Count | Should -Be 3
        ([regex]::Matches($workflow, 'runs-on:\s*ubuntu-latest')).Count | Should -Be 1
        ([regex]::Matches($workflow, 'timeout-minutes:\s*\d+')).Count | Should -Be 4

        $evidenceCheckIndex = $workflow.IndexOf('- name: Validate canonical current evidence')
        $packagingPesterInstallIndex = $workflow.IndexOf('- name: Install exact Pester')
        $evidenceCheckIndex | Should -BeGreaterThan -1
        $evidenceCheckIndex | Should -BeLessThan $packagingPesterInstallIndex
        $workflow | Should -Match 'Update-PcvCurrentEvidenceDocs\.ps1''\s+-Check'
        $workflow | Should -Match 'dotnet-version:\s*10\.0\.x'
        $workflow | Should -Match 'node-version:\s*24'
        ([regex]::Matches($workflow, 'RequiredVersion 5\.7\.1')).Count | Should -Be 2
        $workflow | Should -Match 'dotnet restore src/DesktopNode\.sln'
        $workflow | Should -Match 'dotnet test src/DesktopNode\.sln -c Release --no-restore'
        $workflow | Should -Match 'npm ci --prefix web'
        $workflow | Should -Match 'npm test --prefix web'
        $workflow | Should -Match 'npm run verify:parity --prefix web'
        $workflow | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
        $workflow | Should -Match "Invoke-Pester -Path @\('packaging/windows-desktop-node/installer/tests', 'web/tests'\)"

        $workflow | Should -Match 'Invoke-PcvDevelopmentVerification\.ps1'
        $workflow | Should -Match '(?s)-Lane\s+Full.*-ChangeTier\s+M.*-PlanOnly'
        $workflow | Should -Match 'Join-Path \$artifactRoot ''summary\.json'''
        $workflow | Should -Not -Match 'AllowHostMutation|Invoke-PcvAdminSmokePackage|Invoke-PcvFullAdminHostMutationGate'

        $workflow | Should -Not -Match 'msiexec|Invoke-PcvAdminSmokePackage|Invoke-PcvFullAdminHostMutationGate|Start-VM|New-VM'
        $workflow | Should -Not -Match '(New|Set|Start|Stop|Restart|Remove)-Service|sc(?:\.exe)?\s+(create|start|stop|delete)'
        $workflow | Should -Not -Match 'SignTool|Create-Release|gh\s+release|actions/(upload|download)-artifact|deploy'
    }
}

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

        $isActive = $workflow -match '(?m)^\s{2}dotnet:\s*$'
        $isShadow = -not $isActive -and $workflow.Contains('Run replacement dotnet', [StringComparison]::Ordinal)
        if ($isActive) {
            $workflow | Should -Match '(?m)^\s*dotnet:\s*$'
            $workflow | Should -Match '(?m)^\s*web:\s*$'
            $workflow | Should -Match '(?m)^\s*delivery:\s*$'
            $workflow | Should -Match '(?m)^\s*installer-policy:\s*$'
        }
        else {
            $workflow | Should -Match '(?m)^\s*dotnet-tests:\s*$'
            $workflow | Should -Match '(?m)^\s*web-tests:\s*$'
            $workflow | Should -Match '(?m)^\s*packaging-pester:\s*$'
            $workflow | Should -Match '(?m)^\s*installer-web-pester:\s*$'
        }
        ([regex]::Matches($workflow, 'runs-on:\s*windows-latest')).Count | Should -Be 3
        ([regex]::Matches($workflow, 'runs-on:\s*ubuntu-latest')).Count | Should -Be 1
        ([regex]::Matches($workflow, 'timeout-minutes:\s*\d+')).Count | Should -Be 4

        if ($isShadow) {
            foreach ($token in @(
                'Run legacy dotnet',
                'Run legacy web',
                'Run legacy packaging Pester',
                'Run legacy installer and Web Pester',
                'Run replacement dotnet',
                'Run replacement web',
                'Run replacement delivery',
                'Run replacement installer-policy',
                'name: legacy-packaging',
                'name: replacement-delivery',
                'path: artifacts/shadow/delivery/legacy',
                'path: artifacts/shadow/delivery/replacement',
                'actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd',
                'actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d',
                'actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903',
                'actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f')) {
                $workflow | Should -Match ([regex]::Escape($token))
            }
            ([regex]::Matches($workflow, 'RequiredVersion 5\.7\.1')).Count | Should -Be 2
            $workflow | Should -Match 'npm ci --prefix web'
            $workflow | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
            $workflow | Should -Match "Invoke-Pester -Path @\('packaging/windows-desktop-node/installer/tests', 'web/tests'\)"
            $workflow | Should -Not -Match 'AllowHostMutation|Invoke-PcvAdminSmokePackage|Invoke-PcvFullAdminHostMutationGate|Start-VM|New-VM'
            $workflow | Should -Not -Match '(New|Set|Start|Stop|Restart|Remove)-Service|sc(?:\.exe)?\s+(create|start|stop|delete)'
            $workflow | Should -Not -Match 'SignTool|Create-Release|gh\s+release|deploy'
            return
        }

        if ($isActive) {
            foreach ($token in @(
                'Run dotnet shard',
                'Run web shard',
                'Run delivery shard',
                'Run installer-policy shard',
                'actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd',
                'actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d',
                'actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903',
                'actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f')) {
                $workflow | Should -Match ([regex]::Escape($token))
            }
            $workflow | Should -Not -Match '(?i)Invoke-Pester|Install-Module\s+Pester|shell:\s*(?:pwsh|powershell)'
            $workflow | Should -Not -Match 'AllowHostMutation|Invoke-PcvAdminSmokePackage|Invoke-PcvFullAdminHostMutationGate|Start-VM|New-VM'
            return
        }

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

Set-StrictMode -Version Latest

Describe 'CI trigger contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
    }

    It 'runs Development Gates for pull requests, main pushes, and manual dispatch only' {
        $workflowPath = Join-Path $script:RepoRoot '.github\workflows\development-gates.yml'
        $workflowPath | Should -Exist
        $workflow = Get-Content -Raw -LiteralPath $workflowPath

        $workflow | Should -Match '(?m)^\s*pull_request:\s*$'
        $workflow | Should -Match '(?m)^\s*push:\s*$'
        $workflow | Should -Match '(?m)^\s*-\s*main\s*$'
        $workflow | Should -Match '(?m)^\s*workflow_dispatch:\s*$'
        $workflow | Should -Not -Match "(?m)^\s*-\s*'codex/\*\*'\s*$"
    }

    It 'runs Public Boundary Contract for pull requests, main pushes, and manual dispatch only' {
        $workflowPath = Join-Path $script:RepoRoot '.github\workflows\public-boundary.yml'
        $workflowPath | Should -Exist
        $workflow = Get-Content -Raw -LiteralPath $workflowPath

        $workflow | Should -Match '(?m)^\s*pull_request:\s*$'
        $workflow | Should -Match '(?m)^\s*push:\s*$'
        $workflow | Should -Match '(?m)^\s*-\s*main\s*$'
        $workflow | Should -Match '(?m)^\s*workflow_dispatch:\s*$'
        $workflow | Should -Not -Match "(?m)^\s*-\s*'codex/\*\*'\s*$"
    }
}

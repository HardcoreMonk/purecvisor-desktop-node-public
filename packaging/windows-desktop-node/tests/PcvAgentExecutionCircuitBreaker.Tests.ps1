Set-StrictMode -Version Latest

Describe 'agent execution circuit breaker contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ContractPath = Join-Path $script:RepoRoot 'config/agent-execution-circuit-breaker.json'
        $script:PolicyPath = Join-Path $script:RepoRoot 'docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md'
        $script:AgentsPath = Join-Path $script:RepoRoot 'AGENTS.md'
    }

    It 'pins the bounded execution limits in a machine-readable contract' {
        $script:ContractPath | Should -Exist
        $contract = Get-Content -Raw -LiteralPath $script:ContractPath | ConvertFrom-Json

        $contract.schema_version | Should -Be 1
        $contract.contract | Should -Be 'pcv-agent-execution-circuit-breaker-v1'
        $contract.default_checkpoint_count | Should -Be 1
        $contract.elapsed_minutes_limit | Should -Be 30
        $contract.tool_batch_limit | Should -Be 18
        $contract.review_pass_limit | Should -Be 1
        $contract.narrow_rereview_pass_limit | Should -Be 2
        ($contract.review_pass_limit + $contract.narrow_rereview_pass_limit) | Should -Be 3
        $contract.same_failure_limit | Should -Be 3
        $contract.progress_warning_percent | Should -Be 70
    }

    It 'makes AGENTS load the normative policy without editing generated evidence' {
        $script:PolicyPath | Should -Exist
        $agents = Get-Content -Raw -LiteralPath $script:AgentsPath
        $policy = Get-Content -Raw -LiteralPath $script:PolicyPath

        $agents | Should -Match ([regex]::Escape('docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md'))
        $agents | Should -Match ([regex]::Escape('config/agent-execution-circuit-breaker.json'))
        $agents | Should -Match '`vague_resume_policy`:\s*`one-bounded-checkpoint`'
        $agents | Should -Match '`out_of_scope_findings`:\s*`report-only`'
        $agents.IndexOf('## 에이전트 실행 회로 차단기 (필수)') |
            Should -BeGreaterThan $agents.IndexOf('<!-- END GENERATED CURRENT EVIDENCE -->')
        ([regex]::Matches($agents, '<!-- BEGIN GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1
        ([regex]::Matches($agents, '<!-- END GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1
        $agents | Should -Match ('(?m)^- ' + [regex]::Escape('기본 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토 2회다.') + '\r?$')
        $agents | Should -Match ([regex]::Escape('동일 원인 3회 실패'))

        $policy | Should -Match '30분'
        $policy | Should -Match '18회'
        $policy | Should -Match '동일 원인.*3회'
        $policy | Should -Not -Match '동일 원인.*2회'
        $policy | Should -Match '정규 리뷰 1회'
        $policy | Should -Match '제한 재검토 2회'
        $policy | Should -Match '21분'
        $policy | Should -Match '13번째'
        $policy | Should -Match '소급.*않는다'
    }

    It 'requires stop-only behavior after budget exhaustion and forbids adjacent native expansion' {
        $policy = Get-Content -Raw -LiteralPath $script:PolicyPath

        $policy | Should -Match '추가 patch.*금지'
        $policy | Should -Match '새 테스트.*금지'
        $policy | Should -Match '새 하위 에이전트.*금지'
        $policy | Should -Match 'Add-Type'
        $policy | Should -Match 'P/Invoke'
        $policy | Should -Match '별도 probe checkpoint'
        $policy | Should -Match '사용자의 명시적 승인'
        $policy | Should -Not -Match 'public_trusted_signing\s*[:=]\s*true'
        $policy | Should -Not -Match 'external_stable_publication\s*[:=]\s*true'
        $policy | Should -Not -Match 'host_mutation\s*[:=]\s*true'
    }
}

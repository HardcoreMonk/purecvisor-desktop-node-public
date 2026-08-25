# Agent Execution Circuit Breaker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persist and regression-test a repository-level bounded-execution contract that stops vague follow-up work from expanding indefinitely.

**Architecture:** `config/agent-execution-circuit-breaker.json` pins the numeric contract, `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` is its normative human-readable explanation, and a short mandatory block in root `AGENTS.md` makes every repository agent load it. One focused Pester file ratchets all three surfaces without touching product payload or operational evidence.

**Tech Stack:** Markdown, JSON, PowerShell 5.1/7, Pester 5

---

## File map

- Create `config/agent-execution-circuit-breaker.json`: machine-readable limits and contract ID only.
- Create `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`: normative start, progress, stop, review, and low-level-change rules.
- Modify `AGENTS.md`: short mandatory entrypoint immediately after the generated current-evidence block.
- Create `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`: regression checks for all three surfaces.

### Task 1: Add the failing policy contract test

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`

- [ ] **Step 1: Write the failing test**

Create the test with this complete content:

```powershell
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
        $contract.elapsed_minutes_limit | Should -Be 20
        $contract.tool_batch_limit | Should -Be 12
        $contract.review_pass_limit | Should -Be 1
        $contract.narrow_rereview_pass_limit | Should -Be 1
        $contract.same_failure_limit | Should -Be 2
        $contract.progress_warning_percent | Should -Be 70
    }

    It 'makes AGENTS load the normative policy without editing generated evidence' {
        $script:PolicyPath | Should -Exist
        $agents = Get-Content -Raw -LiteralPath $script:AgentsPath
        $policy = Get-Content -Raw -LiteralPath $script:PolicyPath

        $agents | Should -Match [regex]::Escape('docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md')
        $agents | Should -Match [regex]::Escape('config/agent-execution-circuit-breaker.json')
        $agents | Should -Match 'vague_resume_policy:\s*`one-bounded-checkpoint`'
        $agents | Should -Match 'out_of_scope_findings:\s*`report-only`'
        $agents.IndexOf('## 에이전트 실행 회로 차단기 (필수)') |
            Should -BeGreaterThan $agents.IndexOf('<!-- END GENERATED CURRENT EVIDENCE -->')
        ([regex]::Matches($agents, '<!-- BEGIN GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1
        ([regex]::Matches($agents, '<!-- END GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1

        $policy | Should -Match '20분'
        $policy | Should -Match '12회'
        $policy | Should -Match '동일 원인.*2회'
        $policy | Should -Match '정규 리뷰 1회'
        $policy | Should -Match '제한 재검토 1회'
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
```

- [ ] **Step 2: Run the test to verify RED**

Run:

```powershell
Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1' -Output Detailed
```

Expected: three failed tests. The first failure names missing `config/agent-execution-circuit-breaker.json`; the second names missing `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`; the third cannot read that same missing policy. A parser or discovery failure is not the expected RED and must be corrected before continuing.

### Task 2: Add the minimal contract and policy surfaces

**Files:**
- Create: `config/agent-execution-circuit-breaker.json`
- Create: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- Modify: `AGENTS.md` immediately after `<!-- END GENERATED CURRENT EVIDENCE -->`
- Test: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`

- [ ] **Step 1: Create the machine-readable contract**

Create `config/agent-execution-circuit-breaker.json` exactly as follows:

```json
{
  "schema_version": 1,
  "contract": "pcv-agent-execution-circuit-breaker-v1",
  "default_checkpoint_count": 1,
  "elapsed_minutes_limit": 20,
  "tool_batch_limit": 12,
  "review_pass_limit": 1,
  "narrow_rereview_pass_limit": 1,
  "same_failure_limit": 2,
  "progress_warning_percent": 70
}
```

- [ ] **Step 2: Create the normative policy**

Create `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` with this complete content:

```markdown
# 에이전트 실행 회로 차단기

- Contract: `pcv-agent-execution-circuit-breaker-v1`
- Machine contract: `config/agent-execution-circuit-breaker.json`
- Product payload change: `false`
- Host/VM/service/package mutation authorization: `false`

이 문서는 저장소 에이전트 후속 작업의 실행 한도와 종료 동작의 단일 진실이다. 사용자의
명시적 지시가 없는 `재개`, `계속`, `후속 작업`은 다음 bounded checkpoint 하나만 뜻한다.

## 시작 계약

write, 외부 mutation 또는 하위 에이전트 시작 전에 commentary에 다음을 공개한다.

- checkpoint: 기본 `1`
- elapsed budget: 기본 `20분`
- tool batch budget: 기본 `12회`
- review budget: 정규 리뷰 1회 + 수정된 줄만 보는 제한 재검토 1회
- allowed files 또는 bounded directories
- 완료를 증명할 정확한 검증 명령
- 범위 밖 발견은 `report-only`

도구 작업 묶음은 하나의 목적을 위한 단일 tool call이다. 호출을 불필요하게 쪼개 한도를
우회하지 않는다. 시간과 작업 묶음 중 먼저 도달한 한도가 우선한다.

## 진행 보고

예산 70% 또는 주요 checkpoint 종료 시 `elapsed/limit`, `tool batches/limit`, 완료 항목,
남은 항목, 범위 밖 발견 수와 다음 stop 지점을 수치로 보고한다. `진행 중`만으로는 충분하지
않다.

## 회로 개방 조건

다음 중 하나면 즉시 회로를 연다.

- 경과 20분 또는 도구 작업 묶음 12회
- 동일 원인의 실패를 2회 관측
- 승인 범위 밖 파일군 또는 새 설계가 필요
- 정규 리뷰 1회와 제한 재검토 1회를 사용
- 재검토에서 새 범주의 결함 발견
- 새 `Add-Type`, P/Invoke, native ACL/FileId/process-tree/installer handoff 변경 필요

저수준 Windows 변경은 현재 작업에 붙이지 않는다. 사용자가 승인한 별도 probe checkpoint에서
최소 RED/GREEN 호환성 probe를 먼저 수행하며, 동일 원인으로 두 번 실패하면 확장하지 않는다.

## 회로 개방 후 허용 행동

회로가 열린 뒤에는 다음만 허용한다.

1. 실행 중 명령과 하위 에이전트를 종료한다.
2. `git status`, diff와 이미 끝난 검증 결과를 읽는다.
3. 권한이 있으면 이미 green인 최소 checkpoint만 로컬 보존한다.
4. 완료, 미완료, blocker, 범위 밖 발견과 mutation 여부를 보고한다.

추가 patch는 금지한다. 새 테스트도 금지한다. 새 하위 에이전트와 전체 재감사도 금지한다.
예산 연장이나 다음 checkpoint는 사용자의 명시적 승인 뒤 새 시작 계약으로만 가능하다.

## 리뷰 경계

리뷰어는 고정 diff, 승인 기준과 허용 파일만 검사한다. 정규 리뷰 1회 후 종료한다. 제한
재검토 1회는 수정된 줄과 원래 blocker만 확인한다. 새 범주의 P0/P1은 심각도와 영향을
보고하지만 같은 checkpoint에서 수정하지 않는다. 실제 데이터 손실 또는 host mutation
위험이면 추가 작업 없이 중단하고 사용자에게 방향과 권한을 요청한다.
```

- [ ] **Step 3: Add the mandatory AGENTS entrypoint**

Insert this block immediately after `<!-- END GENERATED CURRENT EVIDENCE -->` in root `AGENTS.md`, leaving the generated block byte-for-byte unchanged:

```markdown
## 에이전트 실행 회로 차단기 (필수)

- 단일 진실: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- 기계 계약: `config/agent-execution-circuit-breaker.json`
- `vague_resume_policy`: `one-bounded-checkpoint`
- `out_of_scope_findings`: `report-only`
- 기본 한도는 20분, 도구 작업 묶음 12회, 정규 리뷰 1회와 제한 재검토 1회다.
- 먼저 도달한 한도 또는 동일 원인 2회 실패 시 추가 구현을 중단하고 stop protocol만 수행한다.
- 사용자의 명시적 승인 없이는 예산, 범위 또는 checkpoint를 연장하지 않는다.
```

- [ ] **Step 4: Run focused tests to verify GREEN**

Run:

```powershell
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1',
  'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1'
) -Output Detailed
```

Expected: `8` tests passed, `0` failed. The current-evidence test proves the generated block and its downstream history remain intact.

### Task 3: Cross-version verification and local commit

**Files:**
- Verify: `config/agent-execution-circuit-breaker.json`
- Verify: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- Verify: `AGENTS.md`
- Verify: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`

- [ ] **Step 1: Verify JSON and PowerShell parsing in PowerShell 7**

Run:

```powershell
$null = Get-Content -Raw -LiteralPath 'config/agent-execution-circuit-breaker.json' | ConvertFrom-Json
$tokens = $null
$errors = $null
[Management.Automation.Language.Parser]::ParseFile(
  (Resolve-Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1'),
  [ref]$tokens,
  [ref]$errors) | Out-Null
if (@($errors).Count -ne 0) { throw ($errors | Out-String) }
```

Expected: exit `0`, no output.

- [ ] **Step 2: Verify JSON and PowerShell parsing in Windows PowerShell 5.1**

Run:

```powershell
$parsePathVariable = 'PCV_AGENT_POLICY_PARSE_PATH'
$previousParsePath = [Environment]::GetEnvironmentVariable($parsePathVariable, 'Process')
try {
  [Environment]::SetEnvironmentVariable(
    $parsePathVariable,
    (Resolve-Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1').Path,
    'Process')
  $windowsPowerShellProbe = @'
$ErrorActionPreference = 'Stop'
$null = Get-Content -Raw -LiteralPath 'config/agent-execution-circuit-breaker.json' | ConvertFrom-Json
$testPath = [Environment]::GetEnvironmentVariable('PCV_AGENT_POLICY_PARSE_PATH', 'Process')
$testText = [IO.File]::ReadAllText($testPath, [Text.UTF8Encoding]::new($false, $true))
$tokens = $null
$errors = $null
[Management.Automation.Language.Parser]::ParseInput($testText, [ref]$tokens, [ref]$errors) | Out-Null
if (@($errors).Count -ne 0) { throw ($errors | Out-String) }
'@
  $encodedProbe = [Convert]::ToBase64String(
    [Text.Encoding]::Unicode.GetBytes($windowsPowerShellProbe))
  powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand $encodedProbe
  if ($LASTEXITCODE -ne 0) { throw "Windows PowerShell parser failed: $LASTEXITCODE" }
}
finally {
  [Environment]::SetEnvironmentVariable($parsePathVariable, $previousParsePath, 'Process')
}
```

Expected: exit `0`, no parser errors. Explicit UTF-8 decoding is required because Windows PowerShell
5.1 `Parser.ParseFile` treats a UTF-8 file without BOM as the active ANSI code page.

- [ ] **Step 3: Inspect only the approved diff**

Run:

```powershell
git diff --check
git status --short
git diff -- AGENTS.md config/agent-execution-circuit-breaker.json docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1
```

Expected: exactly the four approved implementation files are modified or untracked; no generated current-evidence content, product source, operational evidence, or 0.42.75 worktree file appears.

- [ ] **Step 4: Commit the policy checkpoint locally**

Run:

```powershell
git add -- AGENTS.md config/agent-execution-circuit-breaker.json docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1
git commit -m "chore: enforce agent execution circuit breaker"
git status --short
```

Expected: commit succeeds and worktree status is empty. Do not push or create a PR.

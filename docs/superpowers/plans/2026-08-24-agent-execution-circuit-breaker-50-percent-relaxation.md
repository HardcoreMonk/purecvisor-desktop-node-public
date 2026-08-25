# Agent Execution Circuit Breaker 50 Percent Relaxation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Increase the repository-wide agent execution stop budgets by 50% while preserving checkpoint, warning, scope, mutation, and stop-protocol safety boundaries.

**Architecture:** Keep `config/agent-execution-circuit-breaker.json` as the numeric single source of truth, mirror it in the normative policy and the mandatory `AGENTS.md` summary, and ratchet all three surfaces with the existing focused Pester contract test. Change values directly without adding a runtime multiplier or changing the v1 contract shape.

**Tech Stack:** JSON, Markdown, root `AGENTS.md`, PowerShell 5.1, Pester 5.7.1, Git

---

## Execution boundary

Design source:
`docs/superpowers/specs/2026-08-24-agent-execution-circuit-breaker-50-percent-relaxation-design.md`

Run this plan only in the dedicated worktree:
`D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\circuit-breaker-50-percent-relaxation`

The current contract remains authoritative throughout the implementation checkpoint. Use one checkpoint with
the existing limits of 20 minutes, 12 tool batches, one regular review, one narrow re-review, and two
same-cause failures. The new defaults become effective only for checkpoints started after the policy change
commit. Do not claim or consume the relaxed limits during the checkpoint that creates them.

No host, VM, service, package, CI, network, public-signing, publication, or product-payload mutation is
authorized. Findings outside the four implementation files are report-only.

## File map

- Modify: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`
  - Pins exact numeric values, total review capacity, normative text, generated marker integrity, and retained
    stop-only/native boundaries.
- Modify: `config/agent-execution-circuit-breaker.json`
  - Stores the effective machine-readable limits without changing schema or contract identifiers.
- Modify: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
  - Defines the new start budget, 70% warning calculation, open conditions, non-retroactivity, and review limits.
- Modify: `AGENTS.md`
  - Updates only the mandatory circuit-breaker summary below the generated evidence block.

Do not modify the approved design, this plan, the historical `2026-08-23` design/plan, generated current
evidence, product code, Web Wave B files, or any operational evidence.

### Task 1: Ratchet and apply the relaxed stop budgets

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`
- Modify: `config/agent-execution-circuit-breaker.json`
- Modify: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- Modify: `AGENTS.md`
- Test: `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`

- [ ] **Step 1: Confirm the isolated baseline**

Run from the worktree root with Windows PowerShell 5.1 as the direct execution shell:

```powershell
git status --short
Import-Module Pester -ErrorAction Stop
Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1' -Output Detailed
```

Expected:

- `git status --short` prints nothing.
- Pester discovers 3 tests and reports `Tests Passed: 3, Failed: 0`.
- No host or external mutation occurs.

If the worktree is dirty or Pester does not print a real aggregate, stop and report. A shell that merely echoes
the command is not a passing test run.

- [ ] **Step 2: Write the failing contract expectations first**

Use `apply_patch` to update only
`packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`.

Replace the numeric assertions in the first test with:

```powershell
$contract.schema_version | Should -Be 1
$contract.contract | Should -Be 'pcv-agent-execution-circuit-breaker-v1'
$contract.default_checkpoint_count | Should -Be 1
$contract.elapsed_minutes_limit | Should -Be 30
$contract.tool_batch_limit | Should -Be 18
$contract.review_pass_limit | Should -Be 1
$contract.narrow_rereview_pass_limit | Should -Be 2
$contract.same_failure_limit | Should -Be 3
$contract.progress_warning_percent | Should -Be 70
($contract.review_pass_limit + $contract.narrow_rereview_pass_limit) | Should -Be 3
```

In the second test, retain all generated-marker and policy-link assertions, replace the old policy value checks,
and add the exact `AGENTS.md`, warning, and non-retroactivity checks:

```powershell
$agents | Should -Match ([regex]::Escape('기본 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토 2회다.'))
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
```

Do not weaken or remove these existing checks:

```powershell
$agents | Should -Match '`vague_resume_policy`:\s*`one-bounded-checkpoint`'
$agents | Should -Match '`out_of_scope_findings`:\s*`report-only`'
([regex]::Matches($agents, '<!-- BEGIN GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1
([regex]::Matches($agents, '<!-- END GENERATED CURRENT EVIDENCE -->').Count) | Should -Be 1
$policy | Should -Match '추가 patch.*금지'
$policy | Should -Match '새 테스트.*금지'
$policy | Should -Match '새 하위 에이전트.*금지'
$policy | Should -Match 'Add-Type'
$policy | Should -Match 'P/Invoke'
$policy | Should -Match '별도 probe checkpoint'
$policy | Should -Match '사용자의 명시적 승인'
```

- [ ] **Step 3: Run the focused test and verify RED**

Run with Windows PowerShell 5.1 as the direct execution shell:

```powershell
Import-Module Pester -ErrorAction Stop
Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1' -Output Detailed
```

Expected: the first machine-contract test and the second documentation-contract test fail because the checked-in
values are still `20/12/1/1/2/70`; the retained stop-only test remains green. A discovery, parser, module, path,
or command-echo failure is not the expected RED and must be corrected before implementation. Do not edit any
production policy surface until this RED is observed.

- [ ] **Step 4: Apply the minimal machine contract**

Use `apply_patch` to replace the complete contents of
`config/agent-execution-circuit-breaker.json` with:

```json
{
  "schema_version": 1,
  "contract": "pcv-agent-execution-circuit-breaker-v1",
  "default_checkpoint_count": 1,
  "elapsed_minutes_limit": 30,
  "tool_batch_limit": 18,
  "review_pass_limit": 1,
  "narrow_rereview_pass_limit": 2,
  "same_failure_limit": 3,
  "progress_warning_percent": 70
}
```

Do not add a multiplier, policy revision field, override, or new contract version.

- [ ] **Step 5: Apply the normative policy text**

Use `apply_patch` to make the following exact value and behavior changes in
`docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` while retaining every unrelated paragraph.

The start contract bullets must read:

```markdown
- checkpoint: 기본 `1`
- elapsed budget: 기본 `30분`
- tool batch budget: 기본 `18회`
- review budget: 정규 리뷰 1회 + 수정된 줄과 기존 blocker만 보는 제한 재검토 2회
```

Immediately after the start bullets, add:

```markdown
새 기본값은 정책 변경 커밋 이후 시작하는 checkpoint부터 적용한다. 이미 시작한 checkpoint의
예산은 시작 commentary에서 공개한 값이 끝까지 유효하며 소급 확장하지 않는다.
```

Keep the existing 70% progress rule and add its deterministic integer interpretation:

```markdown
70% 경고 시점은 elapsed 21분이며, 정수 단위인 도구 작업 묶음은
`ceil(18 * 0.70)`에 따라 13번째 묶음을 완료한 시점이다.
```

The numeric circuit-open bullets must read:

```markdown
- 경과 30분 또는 도구 작업 묶음 18회
- 동일 원인의 실패를 3회 관측
- 정규 리뷰 1회와 제한 재검토 2회를 사용
```

Change the low-level Windows probe sentence to use three same-cause failures:

```markdown
최소 RED/GREEN 호환성 probe를 먼저 수행하며, 동일 원인으로 세 번 실패하면 확장하지 않는다.
```

Replace the first two review-boundary sentences with:

```markdown
리뷰어는 고정 diff, 승인 기준과 허용 파일만 검사한다. 정규 리뷰는 1회로 제한한다. 제한
재검토는 최대 2회이며 수정된 줄과 원래 blocker만 확인한다.
```

Retain the rule that a new defect category during re-review opens the circuit, and retain every stop-only,
explicit-approval, report-only, native expansion, and mutation-risk rule.

- [ ] **Step 6: Update only the mandatory `AGENTS.md` summary**

Use `apply_patch` outside the generated current evidence markers. Replace only these two sentences under
`## 에이전트 실행 회로 차단기 (필수)`:

```markdown
- 기본 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토 2회다.
- 먼저 도달한 한도 또는 동일 원인 3회 실패 시 추가 구현을 중단하고 stop protocol만 수행한다.
```

Do not modify anything between:

```text
<!-- BEGIN GENERATED CURRENT EVIDENCE -->
<!-- END GENERATED CURRENT EVIDENCE -->
```

- [ ] **Step 7: Run focused GREEN verification**

Run with Windows PowerShell 5.1 as the direct execution shell:

```powershell
Import-Module Pester -ErrorAction Stop
Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1' -Output Detailed
```

Expected: `Tests Passed: 3, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0`.

If any test remains red, fix only the directly corresponding value or wording. A second failure with the same
cause opens the current circuit under the still-active old contract; stop and report instead of expanding scope.

- [ ] **Step 8: Verify JSON, scope, formatting, and generated-block isolation**

Run from the worktree root:

```powershell
$contract = Get-Content -Raw -LiteralPath 'config/agent-execution-circuit-breaker.json' | ConvertFrom-Json
@(
    $contract.default_checkpoint_count,
    $contract.elapsed_minutes_limit,
    $contract.tool_batch_limit,
    $contract.review_pass_limit,
    $contract.narrow_rereview_pass_limit,
    $contract.same_failure_limit,
    $contract.progress_warning_percent
) -join '/'
git diff --check
git diff --name-only
git diff --unified=0 -- AGENTS.md
git status --short
```

Expected:

- Value projection is exactly `1/30/18/1/2/3/70`.
- `git diff --check` exits 0.
- `git diff --name-only` lists exactly the four implementation files from the file map.
- The zero-context `AGENTS.md` diff contains only the two mandatory-summary sentence replacements below the
  generated evidence block; no generated evidence line appears as an added or removed line.
- `git status --short` shows exactly four modified tracked files.

- [ ] **Step 9: Commit the focused policy change**

Stage exactly the four implementation files:

```powershell
git add -- `
  AGENTS.md `
  config/agent-execution-circuit-breaker.json `
  docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md `
  packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1
git diff --cached --name-only
git commit -m 'chore: relax agent execution circuit breaker limits'
```

Expected staged paths, with no others:

```text
AGENTS.md
config/agent-execution-circuit-breaker.json
docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md
packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1
```

- [ ] **Step 10: Run fresh post-commit verification**

Run the complete focused verification again after the commit:

```powershell
Import-Module Pester -ErrorAction Stop
Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1' -Output Detailed
$contract = Get-Content -Raw -LiteralPath 'config/agent-execution-circuit-breaker.json' | ConvertFrom-Json
@(
    $contract.default_checkpoint_count,
    $contract.elapsed_minutes_limit,
    $contract.tool_batch_limit,
    $contract.review_pass_limit,
    $contract.narrow_rereview_pass_limit,
    $contract.same_failure_limit,
    $contract.progress_warning_percent
) -join '/'
git show --check --stat --oneline HEAD
git status --short
```

Expected:

- Pester reports 3 passed and 0 failed.
- Projection is `1/30/18/1/2/3/70`.
- `git show --check` reports no whitespace errors and only the four implementation files.
- `git status --short` prints nothing.
- The completion report explicitly states that the relaxed limits apply only to subsequently started
  checkpoints and that no host or external mutation occurred.

## Review handoff

After Task 1 is green and committed, run the selected execution workflow's mandatory spec-compliance review,
then code-quality review. Reviewers must compare against the approved design and inspect only the fixed four-file
diff. Any new defect category found during narrow re-review opens the circuit and is report-only in that
checkpoint.

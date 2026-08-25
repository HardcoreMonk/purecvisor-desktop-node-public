# Batch 3-B Troubleshooting Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Batch 3-A의 Evidence view 이후, Web Console troubleshooting/incident surface가 `batch_evidence` degradation, failed jobs, summary signals를 운영자가 즉시 triage할 수 있는 read-only 형태로 정리한다.

**Architecture:** Batch 3-B는 UI polish slice다. 기존 `troubleshooting` view와 `renderIncidentCommand()`를 확장해 evidence status, degraded signal, failed/retryable jobs, diagnostics boundary를 한 화면에서 스캔 가능하게 만든다. API/Host 경계는 유지하고, UI는 기존 `ops.summary`, `jobs`, browser-tracked jobs만 읽는다.

**Tech Stack:** TypeScript-owned static Web Console, generated `web/app.js`, Pester static tests, Node/TypeScript verification.

---

## 실행 종료 정리

- 상태: 완료
- 구현 commit: `c3163e23fad504677aac5d55f07c8124b9fb4d56`
- 병합 PR: `#4` `Harden batch evidence summary degradation`
- Merge commit: `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`
- 종료 evidence: `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`
- 검증: `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js`, `git diff --check`
- 경계: evidence path input, token input, host mutation command button, 새 destructive action을 추가하지 않았다.

## Prerequisite

- Batch 3-A commit이 먼저 적용되어 있어야 한다.
- `web/src/served-app.ts`에 `getBatchEvidence()`, `renderEvidenceStatusBadge()`, `renderEvidenceDashboard()`가 존재해야 한다.
- `web/index.html`에 `Evidence` view와 `#evidence-panel`이 존재해야 한다.

## 경계

- API/Host/C# code는 수정하지 않는다.
- Evidence path input, token input, host mutation command button을 만들지 않는다.
- Troubleshooting text는 운영 상태/결과를 표시하는 수준으로 제한한다. 기능 설명이나 사용법 문구를 길게 넣지 않는다.
- Failed job action은 기존 cancel/retry handler만 사용한다. 새 destructive action을 추가하지 않는다.

## File Structure

- Modify: `web/src/served-app.ts`
  - `collectEvidenceIssues`, `renderTroubleshootingEvidence`, `renderFailedJobTriageRows` helpers 추가.
  - `renderTroubleshooting()`에 evidence degradation card와 diagnostics boundary card 추가.
  - `renderIncidentCommand()`에 evidence issue row와 retryable failed jobs count를 표시.
- Modify: `web/styles.css`
  - troubleshooting evidence issue row, compact triage list, boundary chips style 추가.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - troubleshooting evidence degradation/static guard 추가.
- Modify: `web/src/user-visible-fixtures.ts`
  - degraded fixture에 `batch_evidence.status="unavailable"`와 `PCV_BATCH_EVIDENCE_PARSE_FAILED` error 추가.
- Generated: `web/app.js`
  - `npm run build:served --prefix web`로 갱신.
- Generated/check artifacts: static parity files if verification reports drift.

## Task 1: Static Test Guard

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: failing troubleshooting evidence test를 추가한다**

Add after the Batch 3-A evidence dashboard test:

```powershell
It 'declares troubleshooting evidence degradation and failed job triage surfaces' {
    $index = Get-Content -LiteralPath (Join-Path $script:WebRoot 'index.html') -Raw
    $app = Get-Content -LiteralPath (Join-Path $script:WebRoot 'app.js') -Raw
    $servedSource = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'served-app.ts') -Raw
    $fixtures = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'user-visible-fixtures.ts') -Raw

    $index | Should -Match 'id="troubleshooting-panel"'
    ($app + $servedSource) | Should -Match 'collectEvidenceIssues'
    ($app + $servedSource) | Should -Match 'renderTroubleshootingEvidence'
    ($app + $servedSource) | Should -Match 'renderFailedJobTriageRows'
    ($app + $servedSource) | Should -Match 'batch-evidence'
    ($app + $servedSource + $fixtures) | Should -Match 'PCV_BATCH_EVIDENCE_PARSE_FAILED|PCV_BATCH_EVIDENCE_ROOT_MISSING'
    ($app + $servedSource) | Should -Match 'retryable'
    ($app + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule'
}
```

- [x] **Step 2: red를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

Expected:

```text
FAIL: collectEvidenceIssues / renderTroubleshootingEvidence not found
```

## Task 2: Evidence Issue Helpers

**Files:**
- Modify: `web/src/served-app.ts`

- [x] **Step 1: evidence issue collector를 추가한다**

Add near evidence helpers introduced in Batch 3-A:

```js
function collectEvidenceIssues() {
  const evidence = getBatchEvidence();
  if (!evidence) return [];
  const issues = [];
  const status = evidence.status || 'not_configured';
  if (status !== 'available' && status !== 'not_configured') {
    issues.push({
      code: `batch-evidence-${status}`,
      message: `Batch evidence ${status}`,
      detail: status,
      tone: status === 'unavailable' ? 'error' : 'warn'
    });
  }
  for (const error of asArray(evidence.errors)) {
    const issue = normalizeError(error);
    issues.push({
      code: issue.code || 'PCV_BATCH_EVIDENCE',
      message: issue.message || 'Batch evidence issue',
      detail: issue.detail || evidence.status || '',
      tone: issue.code === 'PCV_BATCH_EVIDENCE_PARSE_FAILED' ? 'error' : 'warn'
    });
  }
  return issues;
}
```

- [x] **Step 2: failed job triage helper를 추가한다**

Add near activity helpers:

```js
function renderFailedJobTriageRows() {
  const failed = buildActivityRows()
    .map(({ source, job }) => ({ source, job: normalizeJob(job) }))
    .filter(({ job }) => job.status === 'failed')
    .slice(0, 5);
  if (!failed.length) {
    return '<p class="muted">No failed jobs are visible.</p>';
  }

  return `<div class="triage-list">
    ${failed.map(({ source, job }) => {
      const retryable = Boolean(job.error?.retryable);
      return `<div class="triage-row">
        <div>
          <strong>${escapeHtml(job.operation || 'job')}</strong>
          <span class="muted">${escapeHtml(source)}</span>
        </div>
        <div>${stateBadge(job.status)} ${retryable ? '<span class="status-badge warn">retryable</span>' : ''}</div>
      </div>`;
    }).join('')}
  </div>`;
}
```

## Task 3: Troubleshooting Render

**Files:**
- Modify: `web/src/served-app.ts`

- [x] **Step 1: evidence troubleshooting renderer를 추가한다**

Add:

```js
function renderTroubleshootingEvidence() {
  const evidence = getBatchEvidence();
  const issues = collectEvidenceIssues();
  const latest = evidence?.latest || {};
  const release = latest.release || {};
  const issueHtml = issues.length
    ? `<div class="triage-list">${issues.map((issue) => `<div class="triage-row">
        <div>
          <strong>${escapeHtml(issue.code)}</strong>
          <span class="muted">${escapeHtml(issue.message)}</span>
        </div>
        <span class="status-badge ${escapeHtml(issue.tone)}">${escapeHtml(issue.detail || evidence?.status || 'issue')}</span>
      </div>`).join('')}</div>`
    : '<p class="muted">No batch evidence degradation is visible.</p>';

  return `<div class="troubleshooting-card evidence-troubleshooting">
    <span class="muted">Batch evidence</span>
    <strong>${escapeHtml(latest.batch_id || evidence?.status || 'not_configured')}</strong>
    ${renderEvidenceStatusBadge(evidence)}
    ${issueHtml}
    <div class="boundary-chip-row">
      <span>${escapeHtml(release.public_trusted_signing || 'excluded')}</span>
      <span>${escapeHtml(release.external_stable_publication || 'not-claimed')}</span>
    </div>
  </div>`;
}
```

- [x] **Step 2: `renderTroubleshooting()`에 evidence card를 통합한다**

In `renderTroubleshooting()`, after existing `cards` HTML or before it, include:

```js
const evidenceHtml = renderTroubleshootingEvidence();
...
els.troubleshootingPanel.innerHTML = `
  ${evidenceHtml}
  <div class="troubleshooting-grid">
    ${cards.map(([title, value, detail]) => `<div class="troubleshooting-card"><span class="muted">${escapeHtml(title)}</span><strong>${escapeHtml(value)}</strong><p>${escapeHtml(detail)}</p></div>`).join('')}
  </div>`;
```

Keep existing token/security card language that says token values are never rendered.

- [x] **Step 3: `renderIncidentCommand()`에 failed/evidence triage를 반영한다**

Add an evidence issue block near failed jobs:

```js
const evidenceIssues = collectEvidenceIssues();
const evidenceIssueHtml = evidenceIssues.length
  ? `<div class="incident-card">
      <span class="muted">Evidence issues</span>
      <strong>${escapeHtml(String(evidenceIssues.length))}</strong>
      <div class="triage-list">${evidenceIssues.map((issue) => `<div class="triage-row">
        <span>${escapeHtml(issue.code)}</span>
        <span class="status-badge ${escapeHtml(issue.tone)}">${escapeHtml(issue.detail || 'issue')}</span>
      </div>`).join('')}</div>
    </div>`
  : '';
```

Insert `evidenceIssueHtml` into the incident grid next to failed jobs. Use `renderFailedJobTriageRows()` for the failed jobs card body.

## Task 4: Styles And Fixtures

**Files:**
- Modify: `web/styles.css`
- Modify: `web/src/user-visible-fixtures.ts`

- [x] **Step 1: triage styles를 추가한다**

Add near troubleshooting styles:

```css
.triage-list {
  display: grid;
  gap: 8px;
  margin-top: 10px;
}

.triage-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 12px;
  align-items: center;
  border-top: 1px solid var(--border);
  padding-top: 8px;
  min-width: 0;
}

.triage-row strong,
.triage-row span {
  overflow-wrap: anywhere;
}

.boundary-chip-row {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 10px;
}

.boundary-chip-row span {
  border: 1px solid var(--border);
  border-radius: 999px;
  padding: 4px 8px;
  font-size: 12px;
}
```

In mobile media query, add:

```css
.triage-row { grid-template-columns: 1fr; }
```

- [x] **Step 2: degraded fixture를 추가한다**

In `web/src/user-visible-fixtures.ts`, add or update one non-healthy fixture with:

```ts
batch_evidence: {
  schema_version: 1,
  configured: true,
  status: "unavailable",
  artifact_root: "[BATCH_EVIDENCE_ROOT]",
  latest: null,
  errors: [
    {
      code: "PCV_BATCH_EVIDENCE_PARSE_FAILED",
      message: "Batch evidence JSON could not be parsed.",
      detail: "summary.json parse failed",
      retryable: false
    }
  ]
}
```

Keep fixture paths redacted and token-free.

## Task 5: Generate And Verify

**Files:**
- Generated: `web/app.js`
- Generated/check artifacts if parity script reports drift.

- [x] **Step 1: served asset를 갱신한다**

Run:

```powershell
npm run build:served --prefix web
```

Expected:

```text
served app.js written
```

- [x] **Step 2: web verification을 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

Expected:

```text
Pester web tests pass
served app.js is current
static parity verification passed
browser fixture verification passed
git diff --check exit 0
```

- [x] **Step 3: commit한다**

Run:

```powershell
git status -sb
git add web/src/served-app.ts web/styles.css web/src/user-visible-fixtures.ts web/app.js web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "Improve troubleshooting evidence triage"
git status -sb
```

Expected:

```text
Clean worktree after commit
```

## Self-Review

- Spec coverage: evidence degradation, failed job triage, summary signals, diagnostics boundary, token/path safety, and responsive constraints are covered.
- Placeholder scan: no deferred implementation placeholder remains.
- Type consistency: Batch 3-B reuses Batch 3-A helper names and the existing `batch_evidence` API field.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch3b-troubleshooting-polish.md`.

Two execution options:

1. **Subagent-Driven (recommended)** - one worker owns troubleshooting UI/tests, one reviewer checks product/ops fit.
2. **Inline Execution** - execute tasks in this session with checkpoints after test, UI, and verification.

# Batch 3-A Evidence Dashboard Surface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Web Console에서 `GET /api/v1/ops/summary`의 `batch_evidence`를 제품형 Evidence view와 dashboard badge로 표시한다.

**Architecture:** Batch 3-A는 Web Console read-only UI slice다. 새 API route를 만들지 않고 기존 `state.opsSummary.batch_evidence`만 읽으며, main dashboard에는 compact badge/card를 추가하고 dedicated `evidence` view에는 batch run, step attempts, GPU snapshot, release boundary, final host state를 표시한다. `not_configured`, `missing`, `unavailable`, `available` 상태를 모두 UI state로 렌더링한다.

**Tech Stack:** TypeScript-owned static Web Console, `web/src/served-app.ts`, generated `web/app.js`, Pester static tests, Node/TypeScript verification.

---

## 실행 종료 정리

- 상태: 완료
- 구현 commit: `c3163e23fad504677aac5d55f07c8124b9fb4d56`
- 병합 PR: `#4` `Harden batch evidence summary degradation`
- Merge commit: `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`
- 종료 evidence: `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`
- 검증: `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js`, `git diff --check`
- 경계: 새 API route, host mutation, evidence path input, token/protected token 렌더링을 추가하지 않았다.

## 경계

- API/Host/C# code는 수정하지 않는다.
- Hyper-V, MSI, service, firewall, Event Log, trust-store mutation은 실행하지 않는다.
- HTTP request에서 evidence path를 받는 UI를 만들지 않는다.
- Token value, protected token content, raw command stdout/stderr/arguments를 렌더링하지 않는다.
- Marketing/landing page를 만들지 않는다. 기존 ops cockpit layout에 dense operational surface를 추가한다.

## File Structure

- Modify: `web/index.html`
  - nav에 `Evidence` view link 추가.
  - `<section id="evidence" ...>`와 `#evidence-panel` mount point 추가.
- Modify: `web/src/served-app.ts`
  - `VALID_VIEWS`에 `evidence` 추가.
  - `getBatchEvidence`, `renderEvidenceStatusBadge`, `renderEvidenceDashboard`, small format helpers 추가.
  - 기존 `renderOpsCockpit()`에 latest evidence badge/card 추가.
  - `render()`에서 `renderEvidenceDashboard()` 호출.
- Modify: `web/styles.css`
  - evidence grid/table/status badge responsive style 추가.
  - mobile에서 nav/evidence table overflow가 layout을 깨지 않도록 constraints 추가.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - evidence view/nav/function/data field static guard 추가.
- Modify: `web/src/user-visible-fixtures.ts`
  - browser fixture가 `batch_evidence.available`, `batch_evidence.missing`, `batch_evidence.unavailable` state를 볼 수 있도록 fixture에 compact evidence data 추가.
- Generated: `web/app.js`
  - `npm run build:served --prefix web`로 갱신.
- Generated/check artifacts: static parity files if `npm run verify:parity --prefix web` reports drift.

## Task 1: Static Test Guard

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: failing static test를 추가한다**

Add this test after `declares the ops cockpit multi-view shell and summary route`:

```powershell
It 'declares the batch evidence dashboard surface' {
    $index = Get-Content -LiteralPath (Join-Path $script:WebRoot 'index.html') -Raw
    $app = Get-Content -LiteralPath (Join-Path $script:WebRoot 'app.js') -Raw
    $servedSource = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'served-app.ts') -Raw
    $apiTypes = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -Raw
    $fixtures = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'user-visible-fixtures.ts') -Raw

    $index | Should -Match 'data-view-link="evidence"'
    $index | Should -Match 'id="evidence"'
    $index | Should -Match 'id="evidence-panel"'
    ($app + $servedSource) | Should -Match 'renderEvidenceDashboard'
    ($app + $servedSource) | Should -Match 'renderEvidenceStatusBadge'
    ($app + $servedSource) | Should -Match 'batch_evidence'
    ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'gpu_snapshots'
    ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'route_msi_hyperv'
    ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'os_mutation'
    ($app + $servedSource + $fixtures) | Should -Match 'not_configured|missing|unavailable|available'
}
```

- [x] **Step 2: red를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

Expected:

```text
FAIL: data-view-link="evidence" / renderEvidenceDashboard not found
```

## Task 2: Evidence View Mount

**Files:**
- Modify: `web/index.html`

- [x] **Step 1: nav와 mount point를 추가한다**

Change the nav block to include Evidence between Activity and Troubleshooting:

```html
<a href="#dashboard" data-view-link="dashboard">Dashboard</a>
<a href="#vms" data-view-link="vms">Virtual Machines</a>
<a href="#jobs" data-view-link="jobs">Jobs</a>
<a href="#activity" data-view-link="activity">Activity</a>
<a href="#evidence" data-view-link="evidence">Evidence</a>
<a href="#troubleshooting" data-view-link="troubleshooting">Troubleshooting</a>
```

Add this section after `activity` and before `troubleshooting`:

```html
<section id="evidence" class="section app-view" data-view="evidence">
  <div id="view-evidence">
    <div>
      <p class="eyebrow">Batch Evidence</p>
      <h2>Supervisor Evidence</h2>
    </div>
  </div>
  <div id="evidence-panel" class="evidence-panel"></div>
</section>
```

- [x] **Step 2: generated asset는 아직 갱신하지 않고 index-only test 실패를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

Expected:

```text
FAIL: renderEvidenceDashboard not found
```

## Task 3: Served App Evidence Rendering

**Files:**
- Modify: `web/src/served-app.ts`

- [x] **Step 1: view registry와 element lookup을 확장한다**

Change:

```js
const VALID_VIEWS = new Set(['dashboard', 'vms', 'jobs', 'activity', 'troubleshooting']);
```

to:

```js
const VALID_VIEWS = new Set(['dashboard', 'vms', 'jobs', 'activity', 'evidence', 'troubleshooting']);
```

In `cacheElements()`, add:

```js
evidencePanel: byId('evidence-panel'),
```

- [x] **Step 2: evidence helper를 추가한다**

Add near summary helper functions:

```js
function getBatchEvidence() {
  return state.opsSummary?.batch_evidence || state.opsSummary?.batchEvidence || null;
}

function evidenceTone(status) {
  if (status === 'available' || status === 'not_configured') return 'ok';
  if (status === 'missing' || status === 'unavailable') return 'warn';
  return 'warn';
}

function renderEvidenceStatusBadge(evidence = getBatchEvidence()) {
  const status = evidence?.status || 'not_configured';
  const latest = evidence?.latest || {};
  const label = latest.batch_id || status;
  return `<span class="status-badge ${evidenceTone(status)}">${escapeHtml(label)}</span>`;
}

function evidenceValue(value, fallback = 'Unavailable') {
  return value === undefined || value === null || value === '' ? fallback : String(value);
}
```

- [x] **Step 3: dashboard card를 추가한다**

In `renderOpsCockpit()`, append an Evidence card to `cards`:

```js
const evidence = getBatchEvidence();
const evidenceLatest = evidence?.latest || {};
cards.push([
  'Latest evidence',
  evidenceLatest.status || evidence?.status || 'not_configured',
  evidenceLatest.batch_id || 'Batch evidence root is not configured.'
]);
```

Keep the existing compact card structure; do not create nested cards.

- [x] **Step 4: dedicated evidence view renderer를 추가한다**

Add:

```js
function renderEvidenceDashboard() {
  if (!els.evidencePanel) return;
  const evidence = getBatchEvidence();
  if (!evidence || evidence.configured === false) {
    els.evidencePanel.innerHTML = `
      <div class="evidence-empty">
        ${renderEvidenceStatusBadge(evidence)}
        <p class="muted">Batch evidence is not configured for this listener.</p>
      </div>`;
    return;
  }

  const latest = evidence.latest || {};
  const release = latest.release || {};
  const gpu = latest.gpu_snapshots || {};
  const route = latest.route_msi_hyperv || {};
  const os = latest.os_mutation || {};
  const host = latest.host_final_state || {};
  const steps = asArray(latest.steps);
  const errors = asArray(evidence.errors);

  const stepRows = steps.length
    ? steps.map((step) => `<tr>
        <td>${escapeHtml(evidenceValue(step.step_id, 'step'))}</td>
        <td>${stateBadge(step.ok ? 'succeeded' : 'failed')}</td>
        <td>${escapeHtml(evidenceValue(step.attempt_count, '0'))}</td>
        <td>${escapeHtml(evidenceValue(step.retry_count, '0'))}</td>
        <td>${escapeHtml(step.timed_out ? 'true' : 'false')}</td>
      </tr>`).join('')
    : '<tr><td colspan="5" class="muted">No step evidence is available.</td></tr>';

  const errorHtml = errors.length
    ? `<div class="activity-warning">${errors.map((error) => `<strong>${escapeHtml(error.code || 'PCV_BATCH_EVIDENCE')}</strong> ${escapeHtml(error.message || '')}`).join('<br>')}</div>`
    : '';

  els.evidencePanel.innerHTML = `
    ${errorHtml}
    <div class="evidence-header">
      <div>
        <span class="muted">Batch</span>
        <strong>${escapeHtml(evidenceValue(latest.batch_id, evidence.status))}</strong>
      </div>
      ${renderEvidenceStatusBadge(evidence)}
    </div>
    <div class="evidence-grid">
      <div class="evidence-metric"><span class="muted">Version</span><strong>${escapeHtml(evidenceValue(release.version))}</strong></div>
      <div class="evidence-metric"><span class="muted">Signing</span><strong>${escapeHtml(evidenceValue(release.signing_mode))}</strong></div>
      <div class="evidence-metric"><span class="muted">GPU snapshots</span><strong>${escapeHtml(evidenceValue(gpu.count, '0'))}</strong></div>
      <div class="evidence-metric"><span class="muted">Service</span><strong>${escapeHtml(evidenceValue(host.service_state))}</strong></div>
      <div class="evidence-metric"><span class="muted">Route/MSI</span><strong>${escapeHtml(route.ok ? 'Passed' : 'Unavailable')}</strong></div>
      <div class="evidence-metric"><span class="muted">OS gate</span><strong>${escapeHtml(os.ok ? 'Passed' : 'Unavailable')}</strong></div>
    </div>
    <div class="evidence-boundary">
      <span>${escapeHtml(evidenceValue(release.public_trusted_signing, 'excluded'))}</span>
      <span>${escapeHtml(evidenceValue(release.external_stable_publication, 'not-claimed'))}</span>
    </div>
    <div class="evidence-table-wrap">
      <table class="evidence-table">
        <thead><tr><th>Step</th><th>Status</th><th>Attempts</th><th>Retries</th><th>Timed out</th></tr></thead>
        <tbody>${stepRows}</tbody>
      </table>
    </div>`;
}
```

- [x] **Step 5: render cycle에 연결한다**

In `render()`, add before `renderTroubleshooting()`:

```js
renderEvidenceDashboard();
```

- [x] **Step 6: served source syntax를 확인한다**

Run:

```powershell
npm test --prefix web
```

Expected:

```text
FAIL until web/app.js is regenerated
```

## Task 4: Styles And Fixtures

**Files:**
- Modify: `web/styles.css`
- Modify: `web/src/user-visible-fixtures.ts`

- [x] **Step 1: evidence layout style을 추가한다**

Add near ops/activity styles:

```css
.evidence-panel {
  display: grid;
  gap: 16px;
}

.evidence-header,
.evidence-boundary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.evidence-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.evidence-metric {
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 12px;
  min-width: 0;
}

.evidence-metric strong {
  display: block;
  margin-top: 6px;
  overflow-wrap: anywhere;
}

.evidence-table-wrap {
  overflow-x: auto;
}

.evidence-table {
  width: 100%;
  min-width: 620px;
  border-collapse: collapse;
}

.evidence-table th,
.evidence-table td {
  border-bottom: 1px solid var(--border);
  padding: 10px 8px;
  text-align: left;
  vertical-align: top;
}
```

In mobile media query, add:

```css
.evidence-grid { grid-template-columns: 1fr; }
```

- [x] **Step 2: fixture에 evidence data를 추가한다**

In `web/src/user-visible-fixtures.ts`, add `opsSummary.batch_evidence` to at least one healthy fixture:

```ts
batch_evidence: {
  schema_version: 1,
  configured: true,
  status: "available",
  artifact_root: "[BATCH_EVIDENCE_ROOT]",
  latest: {
    batch_id: "full-admin-host-mutation-gate-20260506-001432-0380",
    ok: true,
    status: "completed",
    total_steps: 2,
    executed_steps: 2,
    steps: [
      { step_id: "service-msi-hyperv-admin-smoke", ok: true, exit_code: 0, timed_out: false, retry_count: 1, attempt_count: 1, final_attempt: 1, duration_ms: 120322 },
      { step_id: "os-mutation-gate", ok: true, exit_code: 0, timed_out: false, retry_count: 0, attempt_count: 1, final_attempt: 1, duration_ms: 10021 }
    ],
    gpu_snapshots: { present: true, count: 24, status_counts: { collected: 24 }, peak_adapter_mib: 3912.45, peak_process_mib: 1512.12 },
    release: { version: "0.38.0-admin-smoke", signing_mode: "AllowUnsignedDev", public_trusted_signing: "excluded", external_stable_publication: "not-claimed" },
    route_msi_hyperv: { ok: true, msi_lifecycle_ok: true, msi_lifecycle_step_count: 6 },
    os_mutation: { ok: true, firewall_rule_count: 0, eventlog_source_present: false },
    host_final_state: { service_state: "Running", firewall_rule_count: 0, eventlog_source_present: false, trust_root_present: true, trust_publisher_present: true }
  },
  errors: []
}
```

Do not put secrets or absolute local paths in fixtures.

## Task 5: Generate And Verify

**Files:**
- Generated: `web/app.js`
- Generated/check artifacts if parity script reports drift.

- [x] **Step 1: generated served asset를 갱신한다**

Run:

```powershell
npm run build:served --prefix web
```

Expected:

```text
served app.js written
```

- [x] **Step 2: static tests를 green으로 만든다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

Expected:

```text
Tests Passed
```

- [x] **Step 3: full web verification을 실행한다**

Run:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

Expected:

```text
served app.js is current
static parity verification passed
browser fixture verification passed
git diff --check exit 0
```

- [x] **Step 4: commit한다**

Run:

```powershell
git status -sb
git add web/index.html web/src/served-app.ts web/styles.css web/src/user-visible-fixtures.ts web/app.js web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "Add dashboard batch evidence view"
git status -sb
```

Expected:

```text
Clean worktree after commit
```

## Self-Review

- Spec coverage: dashboard evidence badge, dedicated evidence view, batch status, step attempts, retry count, GPU snapshots, release boundary, final host state, and degraded states are covered.
- Placeholder scan: no deferred implementation placeholder remains.
- Type consistency: UI reads the existing API field `batch_evidence`; nested fields match `web/src/api-types.ts`.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch3a-evidence-dashboard-surface.md`.

Two execution options:

1. **Subagent-Driven (recommended)** - one worker owns Web UI/tests, one reviewer checks visual/contract fit.
2. **Inline Execution** - execute tasks in this session with checkpoints after tests, UI, and verification.

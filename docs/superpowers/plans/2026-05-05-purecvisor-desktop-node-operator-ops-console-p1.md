# Operator Ops Console P1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** P1 Monitoring/Auth/Checkpoint Retention warning을 Web Console read-only 운영 신호로 추가한다.

**Architecture:** P1은 P0에서 추가된 runtime policy, server-side job list, host status, VM inventory state를 재사용한다. 새 Local API route나 OS mutation은 추가하지 않는다. Web Console은 service/API readiness, VMMS/Hyper-V, active/failed job backlog, token/auth policy, LAN exposure, checkpoint count warning을 표시한다.

**Tech Stack:** TypeScript-owned `web/src/served-app.ts`, generated `web/app.js`, static HTML/CSS, Node `vm` browser fixture, Web Pester static tests, Korean Markdown docs.

**구현 상태:** `main`에서 `afc831e Add read-only monitoring signals to web console`로 완료됐다. 2026-05-07에는 문서 상태 정리로 checkbox closure만 반영했다.

---

## File Structure

- Modify: `web/index.html`
  - Add `Monitoring` sidebar link and `#monitoring` section.
  - Add `#monitoring-panel` mount point.
- Modify: `web/src/served-app.ts`
  - Add read-only monitoring signal helpers.
  - Render service/API readiness, VMMS, job backlog, failed jobs, token/auth policy, LAN exposure, checkpoint count warning.
  - Do not render token values or host mutation commands.
- Generate: `web/app.js`
  - Built from `web/src/served-app.ts`.
- Modify: `web/styles.css`
  - Add compact monitoring card styles.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Add `monitoring-panel` fixture id and rendered output assertions.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add static assertions for monitoring mount/render helpers and no forbidden Linux/host mutation terms.
- Modify: `docs/USER_GUIDE.md`
  - Document Monitoring/Auth/Checkpoint warning usage.
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
  - Mark P1 read-only monitoring/auth/checkpoint warning implemented after verification.

## Task 1: Web Static RED

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: Add monitoring surface static test**

Add this Pester test after the P0 operator activity test:

```powershell
It 'declares read-only monitoring auth and checkpoint warning surfaces' {
    $index = Get-Content -LiteralPath $script:IndexPath -Raw
    $app = Get-Content -LiteralPath $script:AppPath -Raw

    $index | Should -Match 'id="monitoring"'
    $index | Should -Match 'id="monitoring-panel"'
    $app | Should -Match 'renderMonitoring'
    $app | Should -Match 'buildMonitoringSignals'
    $app | Should -Match 'checkpoint-warning'
    $app | Should -Match 'token-policy'
    $app | Should -Match 'lan-exposure'
}
```

- [x] **Step 2: Run RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected: FAIL because monitoring mount and render functions are absent.

## Task 2: Monitoring UI Implementation

**Files:**
- Modify: `web/index.html`
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js`
- Modify: `web/styles.css`

- [x] **Step 1: Add HTML mount**

Add sidebar link:

```html
<a href="#monitoring">Monitoring</a>
```

Add this section after `#activity`:

```html
<section id="monitoring" class="section">
  <div class="section-header">
    <div>
      <p class="eyebrow">Signals</p>
      <h2>Monitoring</h2>
    </div>
  </div>
  <div id="monitoring-panel" class="monitoring-panel"></div>
</section>
```

- [x] **Step 2: Add signal helpers**

Add these functions before `renderTroubleshooting()`:

```javascript
function countJobsByStatus(statuses) {
  const wanted = new Set(statuses.map((status) => String(status).toLowerCase()));
  return buildActivityRows().filter(({ job }) => wanted.has(String(job?.status || '').toLowerCase())).length;
}

function getVmCheckpointCount(vm) {
  const raw = vm?.checkpoints?.count ?? vm?.checkpoints_count ?? 0;
  const parsed = Number(raw);
  return Number.isFinite(parsed) ? parsed : 0;
}

function countVmCheckpointWarnings() {
  return asArray(state.vms).filter((vm) => getVmCheckpointCount(vm) >= 10).length;
}

function countSelectedOldCheckpointWarnings() {
  const cutoff = Date.now() - (14 * 24 * 60 * 60 * 1000);
  return asArray(state.selectedVmCheckpoints).filter((checkpoint) => {
    const stamp = Date.parse(checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created || '');
    return Number.isFinite(stamp) && stamp < cutoff;
  }).length;
}

function buildMonitoringSignals() {
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const vmmsRunning = readNested(host, ['hyperv', 'vmms_running']);
  const tokenStorage = readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage']) || 'unknown';
  const exposure = readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind']) || 'loopback';
  const activeJobs = countJobsByStatus(['queued', 'running']);
  const failedJobs = countJobsByStatus(['failed']);
  const checkpointWarnings = countVmCheckpointWarnings();
  const oldCheckpointWarnings = countSelectedOldCheckpointWarnings();

  return [
    { key: 'service-api', label: 'Service/API', value: state.connectionState === 'connected' ? 'Connected' : 'Not connected', tone: state.connectionState === 'connected' ? 'ok' : 'warn' },
    { key: 'vmms', label: 'VMMS', value: formatPolicyValue(vmmsRunning), tone: vmmsRunning === true ? 'ok' : 'warn' },
    { key: 'active-jobs', label: 'Active jobs', value: activeJobs, tone: activeJobs > 0 ? 'warn' : 'ok' },
    { key: 'failed-jobs', label: 'Failed jobs', value: failedJobs, tone: failedJobs > 0 ? 'error' : 'ok' },
    { key: 'checkpoint-warning', label: 'Checkpoint warnings', value: checkpointWarnings + oldCheckpointWarnings, tone: checkpointWarnings + oldCheckpointWarnings > 0 ? 'warn' : 'ok' },
    { key: 'token-policy', label: 'Token policy', value: tokenStorage, tone: tokenStorage === 'none' ? 'warn' : 'ok' },
    { key: 'lan-exposure', label: 'LAN exposure', value: exposure, tone: String(exposure).toLowerCase().includes('lan') ? 'warn' : 'ok' }
  ];
}
```

- [x] **Step 3: Add `renderMonitoring()`**

Add this function before `renderTroubleshooting()`:

```javascript
function renderMonitoring() {
  const signals = buildMonitoringSignals();
  els.monitoringPanel.innerHTML = `
    <div class="monitoring-grid">
      ${signals.map((signal) => `<div class="monitoring-card signal-${escapeHtml(signal.tone)}" data-signal="${escapeHtml(signal.key)}"><span class="muted">${escapeHtml(signal.label)}</span><strong>${escapeHtml(signal.value)}</strong></div>`).join('')}
    </div>`;
}
```

- [x] **Step 4: Wire render and DOM**

Add to `render()` after `renderActivity()`:

```javascript
renderMonitoring();
```

Add to `init()` element map:

```javascript
monitoringPanel: byId('monitoring-panel'),
```

- [x] **Step 5: Add CSS**

Append before the media query:

```css
.monitoring-panel {
  display: grid;
  gap: 8px;
}
.monitoring-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(140px, 1fr));
  gap: 10px;
}
.monitoring-card {
  border: 1px solid var(--line);
  border-radius: 8px;
  background: var(--panel-soft);
  padding: 10px;
}
.monitoring-card strong {
  display: block;
  margin-top: 4px;
  font-size: 18px;
}
.signal-ok { border-color: #b7e4c7; }
.signal-warn { border-color: #f7d58b; }
.signal-error { border-color: #f1b4ac; }
```

Add to the mobile media query:

```css
.monitoring-grid { grid-template-columns: 1fr; }
```

- [x] **Step 6: Regenerate served asset**

Run:

```powershell
npm run build:served --prefix web
```

Expected: `web/app.js` is regenerated.

## Task 3: Browser Fixture GREEN

**Files:**
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Add fixture id**

Add to `requiredIds`:

```javascript
"monitoring-panel",
```

- [x] **Step 2: Add rendered assertions**

Extend the required output list:

```javascript
"Monitoring",
"Token policy",
"Checkpoint warnings"
```

- [x] **Step 3: Run Web verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

Expected: all pass. No Local API listener, Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, or real OS mutation runs.

## Task 4: User Docs and Backlog

**Files:**
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
- Modify: `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p1.md`

- [x] **Step 1: Update user guide**

Add to Web Console direct range:

```markdown
- Monitoring: service/API, VMMS, job backlog, failed job, token policy, LAN exposure, checkpoint warning 표시
```

Add section after `## Troubleshooting Center`:

```markdown
## Monitoring

`Monitoring`은 read-only 운영 신호를 보여준다. Service/API 연결 상태, VMMS 상태, active/failed job 수, token storage policy, LAN exposure 상태, checkpoint warning을 표시한다.

Checkpoint warning은 VM inventory의 checkpoint count와 선택된 VM checkpoint creation time을 기준으로 한다. Retention delete나 keep latest N 같은 destructive checkpoint mutation은 이 화면에서 실행하지 않는다.
```

- [x] **Step 2: Update backlog status**

Add implemented row:

```markdown
| Monitoring/Auth/Checkpoint warning P1 | implemented | `<commit> Add read-only monitoring signals to web console` |
```

Add short note that P1 remains read-only and does not implement retention delete.

- [x] **Step 3: Mark plan status**

Change implementation status:

```markdown
**Implementation Status:** completed on `main` with `<commit> Add read-only monitoring signals to web console`.
```

## Task 5: Commit and Push

**Files:**
- All modified P1 files

- [x] **Step 1: Commit implementation**

Run:

```powershell
git add web/index.html web/src/served-app.ts web/app.js web/styles.css web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "Add read-only monitoring signals to web console"
```

- [x] **Step 2: Commit docs**

Run:

```powershell
git add docs/USER_GUIDE.md docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p1.md
git commit -m "Document operator ops console P1"
```

- [x] **Step 3: Push**

Run:

```powershell
git push
```

Expected: `origin/main` receives the P1 commits.

## Self-Review

- Spec coverage: P1 monitoring, token/auth policy visibility, LAN exposure visibility, failed/active job signal, and checkpoint count/age warning are covered.
- Scope check: This plan does not implement public metrics, Prometheus, RBAC, token rotation/revoke mutation, checkpoint retention delete, or external notification.
- Mutation boundary: The plan does not execute Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, Task Scheduler, reboot, update, rollback, config migration apply, or job-store migration apply.
- Placeholder scan: 이 plan에는 미확정 자리표시자가 없다.

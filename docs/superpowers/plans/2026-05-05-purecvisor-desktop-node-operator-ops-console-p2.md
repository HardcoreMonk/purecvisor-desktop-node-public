# Operator Ops Console P2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** P2 Web UI operator workflow polish와 quality gates를 구현한다.

**Architecture:** P2 첫 slice는 Web Console 안에서 끝나는 비변경 UX/quality gate 작업이다. VM search/filter, safer destructive confirmation, stale/static asset indicator, forbidden Linux runtime term guard를 추가한다. API request/job correlation id와 server-side activity retention hardening은 route-wide contract 변경이므로 별도 후속 plan으로 남긴다.

**Tech Stack:** TypeScript-owned `web/src/served-app.ts`, generated `web/app.js`, static HTML/CSS, Node `vm` browser fixture, Web Pester static tests, Korean Markdown docs.

**구현 상태:** `main`에서 `7de0057 Add P2 operator workflow polish`로 완료됐다. 2026-05-07에는 문서 상태 정리로 checkbox closure만 반영했다.

---

## File Structure

- Modify: `web/index.html`
  - Add `#vm-filter` search input.
  - Add `#asset-status` mount point.
  - Remove Linux-specific create dialog heading/default fixture text.
- Modify: `web/src/served-app.ts`
  - Add VM filter state and render filtering.
  - Add safer lifecycle confirmation copy for poweroff/restart.
  - Render static asset status.
- Generate: `web/app.js`
  - Built from `web/src/served-app.ts`.
- Modify: `web/styles.css`
  - Add compact filter/status styles.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Add required ids and rendered output assertions.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add P2 static tests for filter/status/confirmation and forbidden Linux runtime strings in index/source/rendered output.
- Modify: `docs/USER_GUIDE.md`
  - Document VM filter and asset status.
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
  - Mark P2 Web polish/quality gate slice implemented and API correlation follow-up still separate.

## Task 1: Web Static RED

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: Add P2 static test**

Add this Pester test after the P1 monitoring test:

```powershell
It 'declares P2 operator workflow polish and quality gates' {
    $index = Get-Content -LiteralPath $script:IndexPath -Raw
    $app = Get-Content -LiteralPath $script:AppPath -Raw

    $index | Should -Match 'id="vm-filter"'
    $index | Should -Match 'id="asset-status"'
    $app | Should -Match 'renderAssetStatus'
    $app | Should -Match 'buildVmLifecycleConfirmation'
    $app | Should -Match 'vmFilter'
    ($index + $app) | Should -Not -Match 'Create Linux VM|ubuntu-24\.04|journalctl|libvirt|purecvisorsd|ZFS|OVS|OVN'
}
```

- [x] **Step 2: Run RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected: FAIL because filter/status/lifecycle confirmation are absent and the existing create dialog still says `Create Linux VM`.

## Task 2: Web UI Polish Implementation

**Files:**
- Modify: `web/index.html`
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js`
- Modify: `web/styles.css`

- [x] **Step 1: Update HTML**

In the sidebar after the nav, add:

```html
<div id="asset-status" class="asset-status"></div>
```

In the VM section header, add:

```html
<label class="filter-control">
  Filter
  <input id="vm-filter" type="search" autocomplete="off" placeholder="Name, state, note">
</label>
```

Change the create dialog heading and defaults:

```html
<h2>Create VM</h2>
<label>Name<input name="name" required value="internal-lab-01"></label>
<label>ISO path<input name="iso_path" required value="D:\iso\installer.iso"></label>
```

- [x] **Step 2: Extend state and constants**

Add near the state object:

```javascript
const WEB_ASSET_LABEL = 'app.js';
```

Add to `state`:

```javascript
vmFilter: '',
```

- [x] **Step 3: Add filter helper**

Add before `renderVms()`:

```javascript
function matchesVmFilter(vm) {
  const query = state.vmFilter.trim().toLowerCase();
  if (!query) return true;
  const haystack = [
    getVmId(vm),
    getVmName(vm),
    vm?.state,
    vm?.status,
    vm?.notes,
    vm?.error?.message
  ].join(' ').toLowerCase();
  return haystack.includes(query);
}
```

Change `renderVms()`:

```javascript
const vms = asArray(state.vms).filter(matchesVmFilter);
if (vms.length === 0) {
  els.vmTable.innerHTML = state.vmFilter.trim()
    ? '<p class="muted">No VMs match the current filter.</p>'
    : '<p class="muted">No VMs returned by the Desktop Node API.</p>';
  return;
}
```

- [x] **Step 4: Add safer lifecycle confirmation**

Add near `buildVmDeleteConfirmation()`:

```javascript
function buildVmLifecycleConfirmation(vmId, action) {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `${action} VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a Hyper-V host mutation.',
    'The result will appear in Tracked Jobs and Operator Activity.'
  ].join('\n');
}
```

Change `queueVmLifecycle()`:

```javascript
if (destructive && !window.confirm(buildVmLifecycleConfirmation(vmId, action))) {
  return;
}
```

- [x] **Step 5: Add asset status render**

Add:

```javascript
function renderAssetStatus() {
  els.assetStatus.innerHTML = `<span class="muted">Asset</span><strong>${escapeHtml(WEB_ASSET_LABEL)}</strong>`;
}
```

Call it from `render()` after `renderConnectionState()`.

Add DOM map:

```javascript
assetStatus: byId('asset-status'),
vmFilter: byId('vm-filter'),
```

Add event binding:

```javascript
els.vmFilter.addEventListener('input', () => {
  state.vmFilter = els.vmFilter.value;
  render();
});
```

- [x] **Step 6: Add CSS**

Append:

```css
.asset-status {
  margin-top: 18px;
  border-top: 1px solid var(--line);
  padding-top: 12px;
  display: grid;
  gap: 2px;
}
.asset-status strong { font-size: 13px; }
.filter-control {
  display: grid;
  gap: 4px;
  color: var(--muted);
  font-size: 12px;
  min-width: min(260px, 100%);
}
```

- [x] **Step 7: Regenerate served asset**

Run:

```powershell
npm run build:served --prefix web
```

Expected: `web/app.js` is regenerated.

## Task 3: Fixture and Verification

**Files:**
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Add fixture ids**

Add to `requiredIds`:

```javascript
"asset-status",
"vm-filter",
```

- [x] **Step 2: Extend rendered assertions**

Add required output:

```javascript
"Asset",
"app.js"
```

Add forbidden rendered output:

```javascript
"Create Linux VM",
"ubuntu-24.04"
```

- [x] **Step 3: Run verification**

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

## Task 4: Docs and Commit

**Files:**
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
- Modify: `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p2.md`

- [x] **Step 1: Update user guide**

Add to Web Console direct range:

```markdown
- VM filter, safer destructive confirmations, Web asset status 표시
```

Add a short section:

```markdown
## Operator Workflow Polish

VM 목록의 filter는 name, id, state, note/error text를 기준으로 현재 inventory를 좁힌다. Power off와 Restart confirmation은 VM name/id/state와 queued job 결과 위치를 함께 보여준다. Sidebar의 asset status는 현재 served Web Console asset을 표시한다.
```

- [x] **Step 2: Update backlog**

Add implemented row:

```markdown
| Web UI Operator Workflow Polish / Quality Gate P2 | implemented | `<commit> Add P2 operator workflow polish` |
```

Add note that request/job correlation id remains a separate API hardening follow-up.

- [x] **Step 3: Mark plan status**

Change implementation status:

```markdown
**Implementation Status:** completed on `main` with `<commit> Add P2 operator workflow polish`.
```

- [x] **Step 4: Commit and push**

Run:

```powershell
git add web/index.html web/src/served-app.ts web/app.js web/styles.css web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "Add P2 operator workflow polish"
git add docs/USER_GUIDE.md docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p2.md
git commit -m "Document operator ops console P2"
git push
```

## Self-Review

- Spec coverage: VM filter, safer destructive confirmations, stale/static asset indicator, and forbidden Linux runtime term guard are covered.
- Scope check: API correlation id and server-side activity retention hardening remain separate route-wide follow-up work.
- Mutation boundary: This plan does not execute Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, Task Scheduler, reboot, update, rollback, config migration apply, or job-store migration apply.
- Placeholder scan: 이 plan에는 미확정 자리표시자가 없다.

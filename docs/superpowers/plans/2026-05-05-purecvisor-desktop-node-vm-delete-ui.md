# VM Delete UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a guarded VM delete action to the Desktop Node Web Console using the existing `DELETE /api/v1/vms/{id}` queued job route.

**Architecture:** The Web Console remains a static TypeScript-owned served asset. The client adds advisory delete gating and confirmation, then relies on the .NET API managed marker guard as the authoritative safety boundary. Delete jobs reuse the existing `Tracked Jobs` persistence and polling flow.

**Tech Stack:** TypeScript source in `web/src/served-app.ts`, generated `web/app.js`, Node browser fixture, Pester static asset tests, `docs/USER_GUIDE.md`.

**구현 상태:** `main`에서 `940999e Add VM delete action to web console`로 완료됐다. 2026-05-07에는 문서 상태 정리로 checkbox closure만 반영했으며, 이 closure에서 Hyper-V/service/MSI/firewall/trust-store/LAN/Event Log mutation은 실행하지 않았다.

---

## File Structure

- Modify: `web/src/served-app.ts`
  - Render a `Delete VM` action in the selected VM detail panel.
  - Add helpers for VM state classification and confirmation text.
  - Add `queueVmDelete(vmId)` that sends `DELETE /api/v1/vms/{id}` and tracks the returned job.
  - Wire `data-action="vm-delete"` in the detail panel click handler.
- Modify generated: `web/app.js`
  - Regenerated from `web/src/served-app.ts` with `npm run build:served --prefix web`.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add static tests for delete endpoint/action/confirmation guard.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Make the fixture VM non-running so the rendered delete button is present without triggering real mutation.
  - Assert rendered output contains `Delete VM`.
- Modify: `docs/USER_GUIDE.md`
  - Document the Web Console delete operation, managed marker guard, and job tracking.

## Task 1: Failing Web Static Tests

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: Add failing static tests**

Add these assertions in the lifecycle action test:

```powershell
$app | Should -Match 'data-action="vm-delete"'
$app | Should -Match 'PCV_VM_DELETE_RUNNING_BLOCKED'
$app | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
```

- [x] **Step 2: Run test to verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected: FAIL because `vm-delete` and delete-specific guard strings are not present yet.

## Task 2: Implement Delete UI

**Files:**
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js`

- [x] **Step 1: Add VM state helpers and delete confirmation**

Add these functions near `getVmName`:

```javascript
function getVmState(vm) {
  return String(vm?.state || vm?.status || '').trim();
}

function isRunningVmState(value) {
  return String(value || '').toLowerCase().includes('running');
}

function buildVmDeleteConfirmation(vmId, vm) {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `Delete VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a destructive Hyper-V host mutation.',
    'Only PureCVisor-managed VMs can be deleted; unmanaged VMs are blocked by PCV_VM_NOT_MANAGED_BY_PURECVISOR.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}
```

- [x] **Step 2: Render Delete VM action**

Add this button in `.lifecycle-actions` after `Restart`:

```html
<button class="danger-button" data-action="vm-delete" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Delete VM</button>
```

- [x] **Step 3: Add queueVmDelete**

Add this function after `queueVmLifecycle`:

```javascript
async function queueVmDelete(vmId) {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmState = getVmState(vm);
  if (isRunningVmState(vmState)) {
    throw normalizeError({
      code: 'PCV_VM_DELETE_RUNNING_BLOCKED',
      message: 'Power off the VM before deleting it.',
      detail: 'The Web Console blocks delete for running VMs. Use Power off first, then queue Delete VM again.'
    });
  }
  if (!window.confirm(buildVmDeleteConfirmation(vmId, vm))) {
    return;
  }

  state.actionPending = true;
  state.error = null;
  render();
  try {
    const job = await apiFetch(`/api/v1/vms/${encodeURIComponent(vmId)}`, { method: 'DELETE' });
    trackJob(job);
    state.connectionState = 'connected';
    await loadVms();
    await refreshSelectedVm();
    startPolling();
  } finally {
    state.actionPending = false;
  }
}
```

- [x] **Step 4: Wire click handler**

Update the `actionMap` click branch so `vm-delete` calls `queueVmDelete(button.dataset.vmId)`.

- [x] **Step 5: Regenerate served asset**

Run:

```powershell
npm run build:served --prefix web
```

Expected: command exits 0 and `web/app.js` is regenerated from `web/src/served-app.ts`.

## Task 3: Browser Fixture and User Guide

**Files:**
- Modify: `web/scripts/verify-browser-fixture.mjs`
- Modify: `docs/USER_GUIDE.md`

- [x] **Step 1: Update fixture VM state**

Change fixture VM state from `running` to `off`, and change the required rendered value list from `running` to `Delete VM`.

- [x] **Step 2: Update user guide**

In `docs/USER_GUIDE.md`, add `Delete VM` to the VM power/action table:

```markdown
| `Delete VM` | PureCVisor managed VM delete job을 queue한다. 실행 전 확인 dialog가 뜨며, running VM은 Web Console에서 먼저 `Power off`를 요구한다. |
```

Add a short paragraph after the table:

```markdown
VM delete는 destructive host mutation이다. Web Console은 running VM delete를 먼저 차단하고, API는 PureCVisor managed marker가 없는 VM을 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. Delete job 결과는 `Tracked Jobs`에서 확인한다.
```

## Task 4: Verification and Commit

**Files:**
- All modified files

- [x] **Step 1: Run focused tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

Expected: all pass. No Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, or real OS mutation runs.

- [x] **Step 2: Commit**

Run:

```powershell
git add web/src/served-app.ts web/app.js web/tests/PcvDesktopWeb.Static.Tests.ps1 web/scripts/verify-browser-fixture.mjs docs/USER_GUIDE.md docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md
git commit -m "Add VM delete action to web console"
```

Expected: one implementation commit on `feature/vm-delete-ui`.

## Self-Review

- Spec coverage: The plan covers the existing API route, queued job tracking, managed guard copy, running state advisory block, user guide update, and no Linux runtime import.
- Placeholder scan: This plan contains no TBD/TODO/fill-later placeholders.
- Type consistency: The plan uses existing `state.selectedVm`, `findCachedVm`, `trackJob`, `apiFetch`, `render`, `loadVms`, `refreshSelectedVm`, and `startPolling` names from `web/src/served-app.ts`.

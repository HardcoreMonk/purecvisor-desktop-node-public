// @ts-nocheck
function renderVmDetail() {
  const vm = state.selectedVm;
  if (!vm) {
    els.vmDetailTitle.textContent = 'No VM selected';
    state.selectedVmCheckpoints = [];
    state.selectedVmReadbacks = null;
    state.selectedVmQosControl = null;
    els.vmDetailContent.innerHTML = '<p class="muted">Select a VM row to inspect lifecycle controls and inventory details.</p>';
    return;
  }

  els.vmDetailTitle.textContent = getVmName(vm);
  const vmId = getVmId(vm);
  const canOperate = rbacAllows('operate');
  const canViewConsole = rbacAllows('console.view');
  const templateLocked = isTemplateLockedVm(vm);
  const actionDisabled = isVmActionPending(vmId) || !canOperate ? ' disabled' : '';
  const lockedMutationDisabled = isVmActionPending(vmId) || !canOperate || templateLocked ? ' disabled' : '';
  const checkpointRefreshDisabled = isCheckpointActionPending(vmId, 'create') || !canOperate ? ' disabled' : '';
  const checkpointMutationDisabled = isCheckpointActionPending(vmId, 'create') || !canOperate || templateLocked ? ' disabled' : '';
  const consoleDisabled = canViewConsole ? '' : ' disabled';
  const pendingVmAction = state.pendingVmActions[getVmActionKey(vmId)];
  const storage = flattenNamedList(vm.storage, ['path', 'size_gb', 'attached']);
  const network = flattenNamedList(vm.network, ['name', 'switch', 'mode']);
  const details = [
    ['State', vm.state || vm.status],
    ['ID', vm.id],
    ['CPU', vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count],
    ['Startup Memory MB', vm.memory?.startup_mb ?? vm.memory_mb],
    ['Assigned Memory MB', vm.memory?.assigned_mb ?? vm.memory_assigned_mb],
    ['Generation', vm.generation],
    ['Storage', storage],
    ['Network', network],
    ['Checkpoints', vm.checkpoints?.count ?? vm.checkpoints_count],
    ['Console', formatConsoleValue(vm.console)],
    ['Managed', vm.managed_by_purecvisor],
    ['Template', templateLocked ? 'locked' : 'no'],
    ['Notes', vm.error?.message || vm.notes]
  ];
  const templateLockButton = templateLocked
    ? `<button data-action="vm-template-unlock" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Unlock template</button>`
    : `<button data-action="vm-template-lock" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Lock template</button>`;

  els.vmDetailContent.innerHTML = `
    <div class="lifecycle-actions">
      <button data-action="vm-start" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Start</button>
      <button data-action="vm-shutdown" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Shutdown</button>
      <button class="danger-button" data-action="vm-poweroff" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Power off</button>
      <button class="danger-button" data-action="vm-restart" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Restart</button>
      <button data-action="vm-save" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Save</button>
      <button data-action="vm-resume-saved" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Resume saved</button>
      <button data-action="vm-pause" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Pause</button>
      <button data-action="vm-resume" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Resume</button>
      <button data-action="vm-eject" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Eject media</button>
      <button data-action="vm-delete-status" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Delete status</button>
      <button data-action="vm-manage" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Manage VM</button>
      ${templateLockButton}
      <button data-action="vm-clone" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Clone VM</button>
      <button class="danger-button" data-action="vm-delete" data-vm-id="${escapeHtml(vmId)}"${lockedMutationDisabled}>Delete VM</button>
      <button data-action="vm-console" data-vm-id="${escapeHtml(vmId)}"${consoleDisabled}>Console</button>
      ${pendingVmAction ? `<span class="muted">Pending action: ${escapeHtml(pendingVmAction)}</span>` : ''}
      ${templateLocked ? '<span class="muted">Template lock: start, clone, and unlock only.</span>' : ''}
      ${!canOperate ? '<span class="muted">RBAC: operate permission required for lifecycle actions.</span>' : ''}
    </div>
    <div class="vm-resource-grid">
      <form class="vm-resource-form" data-action="vm-attach" data-vm-id="${escapeHtml(vmId)}">
        <input name="iso_path" type="text" placeholder="ISO path" aria-label="ISO path"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Attach media</button>
      </form>
      <form class="vm-resource-form" data-action="vm-rename" data-vm-id="${escapeHtml(vmId)}">
        <input name="new_name" type="text" placeholder="New VM name" aria-label="New VM name"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Rename VM</button>
      </form>
      <form class="vm-resource-form" data-action="vm-network-connect" data-vm-id="${escapeHtml(vmId)}">
        <select name="switch" aria-label="Switch to connect"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
        <button type="submit"${lockedMutationDisabled}>Connect switch</button>
      </form>
      <form class="vm-resource-form" data-action="vm-device-add" data-vm-id="${escapeHtml(vmId)}">
        <select name="device" aria-label="Device kind"${lockedMutationDisabled}><option value="nic">Network adapter</option><option value="dvd">DVD drive</option></select>
        <select name="switch" aria-label="Network adapter switch"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
        <input name="iso_path" type="text" placeholder="DVD ISO path (optional)" aria-label="DVD ISO path"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Add device</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-memory" data-vm-id="${escapeHtml(vmId)}">
        <input name="memory_mb" type="number" min="512" max="262144" step="128" placeholder="Memory MB" aria-label="memory MB"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Set memory</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-vcpu" data-vm-id="${escapeHtml(vmId)}">
        <input name="cpu" type="number" min="1" max="32" step="1" placeholder="vCPU" aria-label="vCPU"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Set vCPU</button>
      </form>
      <form class="vm-resource-form" data-action="vm-disk-resize" data-vm-id="${escapeHtml(vmId)}">
        <input name="disk_gb" type="number" min="8" max="4096" step="1" placeholder="Disk GB" aria-label="disk GB"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Resize disk</button>
      </form>
      <form class="vm-resource-form" data-action="vm-clone" data-vm-id="${escapeHtml(vmId)}">
        <input name="name" autocomplete="off" placeholder="Target VM name" aria-label="clone target name"${actionDisabled}>
        <button type="submit" data-action="vm-clone"${actionDisabled}>Clone VM</button>
      </form>
    </div>
    <div class="details-grid detail-grid">
      ${details.map(([label, value]) => `<div class="kv"><span>${escapeHtml(label)}</span><strong>${escapeHtml(formatObjectValue(value))}</strong></div>`).join('')}
    </div>
    ${renderExportImportReadback(vm, vmId, actionDisabled)}
    ${renderVmQosGuestReadback(vmId)}
    ${renderVmQosDirectControl(vmId)}
    <div class="checkpoint-panel">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">Checkpoints</p>
          <h3>VM Checkpoints</h3>
        </div>
        <button data-action="checkpoint-refresh" data-vm-id="${escapeHtml(vmId)}"${checkpointRefreshDisabled}>Refresh checkpoints</button>
      </div>
      ${renderCheckpointScheduleReadback(vm, vmId, actionDisabled)}
      <form class="checkpoint-form" data-action="checkpoint-create" data-vm-id="${escapeHtml(vmId)}">
        <input name="checkpoint_name" autocomplete="off" placeholder="Checkpoint name" aria-label="checkpoint name"${checkpointMutationDisabled}>
        <button type="submit"${checkpointMutationDisabled}>Create checkpoint</button>
      </form>
      <div class="checkpoint-list">${renderCheckpointList(vmId)}</div>
    </div>`;
}

function renderExportImportReadback(vm, vmId, actionDisabled = '') {
  const managed = vm?.managed_by_purecvisor === true;
  const generation = Number(vm?.generation);
  const power = String(vm?.state || vm?.status || '').trim().toLowerCase();
  const off = power === 'off' || power === 'stopped';
  const securityPresent = vm?.security_features_present === true;
  const exportEligible = managed && generation === 2 && off && !securityPresent;
  const securityLabel = securityPresent ? 'present' : 'not reported';
  return `<div class="export-import-readback">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">Hyper-V export/import</p>
          <h3>Export / Import</h3>
        </div>
      </div>
      <div class="diagnostics-grid">
        <div class="diagnostics-fact"><span class="muted">export</span><strong>${escapeHtml(exportEligible ? 'eligible' : 'blocked')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">managed</span><strong>${escapeHtml(managed ? 'yes' : 'no')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">generation</span><strong>${escapeHtml(formatObjectValue(vm?.generation))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">power</span><strong>${escapeHtml(formatObjectValue(vm?.state || vm?.status))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">security features</span><strong>${escapeHtml(securityLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">import</span><strong>new identity</strong></div>
      </div>
      <form class="vm-resource-form export-import-form" data-action="vm-export" data-vm-id="${escapeHtml(vmId)}">
        <input name="directory" type="text" placeholder="Export directory" aria-label="Export directory"${actionDisabled}>
        <button type="submit" data-action="vm-export-preview"${actionDisabled}>Preview export</button>
        <button type="submit" data-action="vm-export-apply"${actionDisabled}>Export VM</button>
      </form>
      <form class="vm-resource-form export-import-form" data-action="vm-import" data-vm-id="${escapeHtml(vmId)}">
        <input name="name" type="text" placeholder="New VM name" aria-label="Import VM name"${actionDisabled}>
        <input name="directory" type="text" placeholder="Export package directory" aria-label="Import package directory"${actionDisabled}>
        <label><input name="has_vmcx" type="checkbox"${actionDisabled}> package has .vmcx</label>
        <button type="submit" data-action="vm-import-preview"${actionDisabled}>Preview import</button>
        <button type="submit" data-action="vm-import-apply"${actionDisabled}>Import VM</button>
      </form>
      ${renderExportImportPreview(vmId)}
      <div class="boundary-chip-row">
        <span>preview before export/import</span>
        <span>new VM identity on import</span>
        <span>no OVF</span>
        <span>no TPM key copy</span>
      </div>
    </div>`;
}

function renderCheckpointScheduleReadback(vm, vmId, actionDisabled = '') {
  const schedule = asObject(vm?.checkpoint_schedule);
  const enabled = schedule.enabled === true;
  const status = schedule.status || (enabled ? 'waiting' : 'disabled');
  const count = Number(vm?.checkpoints?.count ?? vm?.checkpoints_count ?? 0);
  const retention = schedule.retention_max;
  const capacityBlocked = enabled && retention != null && Number.isFinite(Number(retention)) && count >= Number(retention);
  const statusLabel = capacityBlocked ? 'blocked capacity' : formatObjectValue(status);
  return `<div class="checkpoint-schedule-readback">
      <div class="diagnostics-grid">
        <div class="diagnostics-fact"><span class="muted">schedule</span><strong>${escapeHtml(enabled ? 'enabled' : 'disabled')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">status</span><strong>${escapeHtml(statusLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">interval minutes</span><strong>${escapeHtml(formatObjectValue(schedule.interval_minutes))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">retention max</span><strong>${escapeHtml(formatObjectValue(retention))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">last enqueued</span><strong>${escapeHtml(formatObjectValue(schedule.last_enqueued_at))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">next due</span><strong>${escapeHtml(formatObjectValue(schedule.next_due_at))}</strong></div>
      </div>
      <form class="vm-resource-form checkpoint-schedule-form" data-action="checkpoint-schedule" data-vm-id="${escapeHtml(vmId)}">
        <input name="interval_minutes" type="number" min="1" step="1" placeholder="Interval minutes" aria-label="Checkpoint interval minutes"${actionDisabled}>
        <input name="retention_max" type="number" min="1" step="1" placeholder="Retention max" aria-label="Checkpoint retention max"${actionDisabled}>
        <button type="submit" data-action="checkpoint-schedule-preview"${actionDisabled}>Preview schedule</button>
        <button type="submit" data-action="checkpoint-schedule-set"${actionDisabled}>Save schedule</button>
        <button type="button" class="danger-button" data-action="checkpoint-schedule-clear" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Clear schedule</button>
      </form>
      ${renderCheckpointSchedulePreview(vmId)}
      <div class="boundary-chip-row">
        <span>preview before save</span>
        <span>no infinite retention</span>
      </div>
    </div>`;
}

function renderVmWorkbenchContext() {
  const vm = state.selectedVm;
  if (!vm) {
    els.vmWorkbenchContext.innerHTML = '<p class="muted">Select a VM to focus lifecycle controls, checkpoints, and related current activity.</p>';
    return;
  }

  const vmId = getVmId(vm);
  const vmName = getVmName(vm);
  const activity = buildActivityRows()
    .map(({ job }) => job)
    .find((job) => {
      const haystack = [job?.job_id, job?.operation, job?.request_id, job?.correlation_id, JSON.stringify(job?.result || {})].join(' ').toLowerCase();
      return haystack.includes(vmId.toLowerCase()) || haystack.includes(vmName.toLowerCase());
    });
  const activityHtml = activity
    ? `<span>${escapeHtml(activity.operation || 'job')}</span>${stateBadge(activity.status)}`
    : '<span class="muted">No related current activity visible.</span>';

  els.vmWorkbenchContext.innerHTML = `
    <div class="vm-context-card">
      <div>
        <span class="muted">Selected VM</span>
        <strong>${escapeHtml(vmName)}</strong>
      </div>
      <div>${stateBadge(vm.state || vm.status)}</div>
      <div class="vm-context-activity">
        <span class="muted">Related activity</span>
        ${activityHtml}
      </div>
    </div>`;
}


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
  const actionDisabled = isVmActionPending(vmId) || !canOperate ? ' disabled' : '';
  const checkpointDisabled = isCheckpointActionPending(vmId, 'create') || !canOperate ? ' disabled' : '';
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
    ['Notes', vm.error?.message || vm.notes]
  ];

  els.vmDetailContent.innerHTML = `
    <div class="lifecycle-actions">
      <button data-action="vm-start" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Start</button>
      <button data-action="vm-shutdown" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Shutdown</button>
      <button class="danger-button" data-action="vm-poweroff" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Power off</button>
      <button class="danger-button" data-action="vm-restart" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Restart</button>
      <button data-action="vm-save" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Save</button>
      <button data-action="vm-resume-saved" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Resume saved</button>
      <button data-action="vm-eject" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Eject media</button>
      <button data-action="vm-delete-status" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Delete status</button>
      <button data-action="vm-manage" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Manage VM</button>
      <button class="danger-button" data-action="vm-delete" data-vm-id="${escapeHtml(vmId)}"${actionDisabled}>Delete VM</button>
      <button data-action="vm-console" data-vm-id="${escapeHtml(vmId)}"${consoleDisabled}>Console</button>
      ${pendingVmAction ? `<span class="muted">Pending action: ${escapeHtml(pendingVmAction)}</span>` : ''}
      ${!canOperate ? '<span class="muted">RBAC: operate permission required for lifecycle actions.</span>' : ''}
    </div>
    <div class="vm-resource-grid">
      <form class="vm-resource-form" data-action="vm-attach" data-vm-id="${escapeHtml(vmId)}">
        <input name="iso_path" type="text" placeholder="ISO path" aria-label="ISO path"${actionDisabled}>
        <button type="submit"${actionDisabled}>Attach media</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-memory" data-vm-id="${escapeHtml(vmId)}">
        <input name="memory_mb" type="number" min="512" max="262144" step="128" placeholder="Memory MB" aria-label="memory MB"${actionDisabled}>
        <button type="submit"${actionDisabled}>Set memory</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-vcpu" data-vm-id="${escapeHtml(vmId)}">
        <input name="cpu" type="number" min="1" max="32" step="1" placeholder="vCPU" aria-label="vCPU"${actionDisabled}>
        <button type="submit"${actionDisabled}>Set vCPU</button>
      </form>
      <form class="vm-resource-form" data-action="vm-disk-resize" data-vm-id="${escapeHtml(vmId)}">
        <input name="disk_gb" type="number" min="8" max="4096" step="1" placeholder="Disk GB" aria-label="disk GB"${actionDisabled}>
        <button type="submit"${actionDisabled}>Resize disk</button>
      </form>
    </div>
    <div class="details-grid detail-grid">
      ${details.map(([label, value]) => `<div class="kv"><span>${escapeHtml(label)}</span><strong>${escapeHtml(formatObjectValue(value))}</strong></div>`).join('')}
    </div>
    ${renderVmQosGuestReadback(vmId)}
    ${renderVmQosDirectControl(vmId)}
    <div class="checkpoint-panel">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">Checkpoints</p>
          <h3>VM Checkpoints</h3>
        </div>
        <button data-action="checkpoint-refresh" data-vm-id="${escapeHtml(vmId)}"${checkpointDisabled}>Refresh checkpoints</button>
      </div>
      <form class="checkpoint-form" data-action="checkpoint-create" data-vm-id="${escapeHtml(vmId)}">
        <input name="checkpoint_name" autocomplete="off" placeholder="Checkpoint name" aria-label="checkpoint name"${checkpointDisabled}>
        <button type="submit"${checkpointDisabled}>Create checkpoint</button>
      </form>
      <div class="checkpoint-list">${renderCheckpointList(vmId)}</div>
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


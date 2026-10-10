// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): VM inventory, detail, QoS and lifecycle mutations (legacy render-inventory, render-vm-detail, render-qos, mutate).
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-inventory.ts ---
function matchesVmFilter(vm) {
  const query = state.vmFilter.trim().toLowerCase();
  const stateFilter = String(state.vmStateFilter || 'all').toLowerCase();
  const vmState = getVmState(vm).toLowerCase();
  if (stateFilter !== 'all' && !vmState.includes(stateFilter)) {
    return false;
  }
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

function getVmUpdatedValue(vm) {
  const stamp = Date.parse(vm?.updated_at || vm?.created_at || vm?.uptime || '');
  return Number.isFinite(stamp) ? stamp : 0;
}

function compareVms(left, right) {
  const sort = String(state.vmSort || 'name').toLowerCase();
  if (sort === 'state') {
    return getVmState(left).localeCompare(getVmState(right)) || getVmName(left).localeCompare(getVmName(right));
  }
  if (sort === 'updated') {
    return getVmUpdatedValue(right) - getVmUpdatedValue(left) || getVmName(left).localeCompare(getVmName(right));
  }
  return getVmName(left).localeCompare(getVmName(right));
}

function renderVms() {
  const vms = asArray(state.vms).filter(matchesVmFilter).sort(compareVms);
  if (vms.length === 0) {
    els.vmTable.innerHTML = state.vmFilter.trim() || state.vmStateFilter !== 'all'
      ? '<p class="muted">No VMs match the current filter.</p>'
      : '<p class="muted">No VMs returned by the Desktop Node API.</p>';
    return;
  }
  const rows = vms.map((vm) => {
    const vmId = getVmId(vm);
    const selected = vmId && vmId === state.selectedVmId ? ' class="selected-row"' : '';
    return `
    <tr${selected} data-vm-id="${pcvEscapeHtml(vmId)}">
      <td><button type="button" class="link-button" data-action="select-vm" data-vm-id="${pcvEscapeHtml(vmId)}">${pcvEscapeHtml(getVmName(vm))}</button></td>
      <td>${stateBadge(vm.state || vm.status)}</td>
      <td>${pcvEscapeHtml(vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count)}</td>
      <td>${pcvEscapeHtml(vm.memory?.startup_mb ?? vm.memory_mb ?? vm.memory ?? vm.memory_assigned_mb)}</td>
      <td>${pcvEscapeHtml(vm.generation)}</td>
      <td>${pcvEscapeHtml(vm.uptime || vm.updated_at || vm.created_at)}</td>
      <td>${pcvEscapeHtml(vm.error?.message || vm.notes || '-')}</td>
    </tr>`;
  }).join('');
  els.vmTable.innerHTML = `
    <table>
      <thead><tr><th>Name</th><th>State</th><th>CPU</th><th>Memory</th><th>Gen</th><th>Updated</th><th>Notes</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>`;
}

function getNetworkInventory() {
  if (Array.isArray(state.networkInventory)) {
    return { source: 'network.inventory', mutating: false, switches: state.networkInventory };
  }
  return asObject(state.networkInventory);
}

function getNetworkSwitches() {
  const inventory = getNetworkInventory();
  return asArray(inventory.switches || inventory.items || inventory.networks);
}

function formatNetworkBoolean(value) {
  if (value === true) return 'enabled';
  if (value === false) return 'disabled';
  return '-';
}

function renderNetworkFailureGuidance(error) {
  if (!error) return '';
  const code = error.code || 'PCV_NETWORK_INVENTORY_UNAVAILABLE';
  const parity = code === 'PCV_NATIVE_PARITY_INCOMPLETE' || String(error.detail || error.message || '').toLowerCase().includes('parity');
  const detail = parity
    ? 'native parity failure: helper fallback is intentionally excluded; check the C# native adapter evidence before retrying.'
    : error.detail || 'Network inventory read failed without mutating Hyper-V switches, IP configuration, or firewall rules.';
  return `<div class="activity-warning network-error-state">
    <strong>${pcvEscapeHtml(code)}</strong>
    <span>${pcvEscapeHtml(error.message || 'Network inventory unavailable.')}</span>
    <p>${pcvEscapeHtml(detail)}</p>
  </div>`;
}

function renderNetworkChangeReadback(switches) {
  const rows = asArray(switches);
  const productCount = rows.filter((item) => String(item?.name || '').toLowerCase().startsWith('pcv-')).length;
  const reservedCount = rows.filter((item) => {
    const name = String(item?.name || '').toLowerCase();
    return name === 'default switch' || name.startsWith('wsl');
  }).length;
  const externalCount = rows.filter((item) => String(item?.type || item?.switch_type || '').toLowerCase() === 'external').length;
  return `<div class="network-change-readback">
      <div class="diagnostics-grid">
        <div class="diagnostics-fact"><span class="muted">switch create</span><strong>service-action only</strong></div>
        <div class="diagnostics-fact"><span class="muted">product switches</span><strong>${pcvEscapeHtml(productCount)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">reserved</span><strong>${pcvEscapeHtml(reservedCount)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">external</span><strong>${pcvEscapeHtml(externalCount === 0 ? 'none' : 'present')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">vm.network.connect</span><strong>VM detail</strong></div>
        <div class="diagnostics-fact"><span class="muted">NAT/DHCP</span><strong>excluded</strong></div>
      </div>
      <div class="boundary-chip-row">
        <span>no switch create form</span>
        <span>no NAT editor</span>
        <span>no DHCP editor</span>
        <span>switch connect in VM detail</span>
      </div>
    </div>`;
}

function renderNetworkInventory() {
  if (!els.networkInventoryPanel) return;
  const inventory = getNetworkInventory();
  const allSwitches = getNetworkSwitches();
  const switches = filterRowsByQuery(allSwitches, state.networkFilter, (item) => [
    item?.name,
    item?.type || item?.switch_type,
    item?.net_adapter_interface_description || item?.adapter || item?.adapter_name,
    item?.is_default === true ? 'default' : ''
  ].join(' '));
  const source = inventory.source || 'network.inventory';
  const mutationMode = inventory.mutating === true ? 'mutating' : 'read-only';
  const defaultSwitchCount = switches.filter((item) => item?.is_default === true || String(item?.name || '').toLowerCase() === 'default switch').length;
  const errorHtml = renderNetworkFailureGuidance(state.networkError);
  const rows = switches.map((item) => {
    const isDefault = item?.is_default === true || String(item?.name || '').toLowerCase() === 'default switch';
    return `
      <tr>
        <td><strong>${pcvEscapeHtml(item?.name || '-')}</strong></td>
        <td>${pcvEscapeHtml(item?.type || item?.switch_type || '-')}</td>
        <td>${isDefault ? stateBadge('default') : '-'}</td>
        <td>${pcvEscapeHtml(formatNetworkBoolean(item?.allow_management_os))}</td>
        <td>${pcvEscapeHtml(item?.net_adapter_interface_description || item?.adapter || item?.adapter_name || '-')}</td>
      </tr>`;
  }).join('');

  els.networkInventoryPanel.innerHTML = `
    ${errorHtml}
    ${renderTableStateSummary('Switches', switches.length, allSwitches.length, state.networkFilter, 'read-only Hyper-V inventory')}
    ${renderNetworkChangeReadback(allSwitches)}
    <div class="network-summary-grid">
      <div class="network-summary-card"><span class="muted">Source</span><strong>${pcvEscapeHtml(source)}</strong></div>
      <div class="network-summary-card"><span class="muted">Mutation</span><strong>${pcvEscapeHtml(mutationMode)}</strong></div>
      <div class="network-summary-card"><span class="muted">Switches</span><strong>${pcvEscapeHtml(switches.length)}</strong></div>
      <div class="network-summary-card"><span class="muted">Default</span><strong>${pcvEscapeHtml(defaultSwitchCount)}</strong></div>
    </div>
    <div class="network-table-wrap">
      ${switches.length === 0
        ? '<p class="muted network-empty-state">No Hyper-V switches returned by the Desktop Node API. This read-only view does not create switches, assign IP addresses, or change firewall policy.</p>'
        : `<table class="network-table">
            <thead><tr><th>Name</th><th>Type</th><th>Default</th><th>Management OS</th><th>Adapter</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>`}
    </div>`;
}

// --- legacy render-vm-detail.ts ---
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
  const dvdMedia = Array.isArray(vm.dvd_media)
    ? (vm.dvd_media.length > 0 ? flattenNamedList(vm.dvd_media, ['path']) : 'none')
    : 'not reported';
  const details = [
    ['State', vm.state || vm.status],
    ['ID', vm.id],
    ['CPU', vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count],
    ['Startup Memory MB', vm.memory?.startup_mb ?? vm.memory_mb],
    ['Assigned Memory MB', vm.memory?.assigned_mb ?? vm.memory_assigned_mb],
    ['Generation', vm.generation],
    ['Storage', storage],
    ['DVD Media', dvdMedia],
    ['Network', network],
    ['Checkpoints', vm.checkpoints?.count ?? vm.checkpoints_count],
    ['Console', formatConsoleValue(vm.console)],
    ['Managed', vm.managed_by_purecvisor],
    ['Template', templateLocked ? 'locked' : 'no'],
    ['Notes', vm.error?.message || vm.notes]
  ];
  const templateLockButton = templateLocked
    ? `<button data-action="vm-template-unlock" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Unlock template</button>`
    : `<button data-action="vm-template-lock" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Lock template</button>`;

  els.vmDetailContent.innerHTML = `
    <div class="lifecycle-actions">
      <button data-action="vm-start" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Start</button>
      <button data-action="vm-shutdown" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Shutdown</button>
      <button class="danger-button" data-action="vm-poweroff" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Power off</button>
      <button class="danger-button" data-action="vm-restart" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Restart</button>
      <button data-action="vm-save" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Save</button>
      <button data-action="vm-resume-saved" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Resume saved</button>
      <button data-action="vm-pause" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Pause</button>
      <button data-action="vm-resume" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Resume</button>
      <button data-action="vm-eject" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Eject media</button>
      <button data-action="vm-delete-status" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Delete status</button>
      <button data-action="vm-manage" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Manage VM</button>
      ${templateLockButton}
      <button data-action="vm-clone" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Clone VM</button>
      <button class="danger-button" data-action="vm-delete" data-vm-id="${pcvEscapeHtml(vmId)}"${lockedMutationDisabled}>Delete VM</button>
      <button data-action="vm-console" data-vm-id="${pcvEscapeHtml(vmId)}"${consoleDisabled}>Console</button>
      ${pendingVmAction ? `<span class="muted">Pending action: ${pcvEscapeHtml(pendingVmAction)}</span>` : ''}
      ${templateLocked ? '<span class="muted">Template lock: start, clone, and unlock only.</span>' : ''}
      ${!canOperate ? '<span class="muted">RBAC: operate permission required for lifecycle actions.</span>' : ''}
    </div>
    <div class="vm-resource-grid">
      <form class="vm-resource-form" data-action="vm-attach" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="iso_path" type="text" placeholder="ISO path" aria-label="ISO path"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Attach media</button>
      </form>
      <form class="vm-resource-form" data-action="vm-rename" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="new_name" type="text" placeholder="New VM name" aria-label="New VM name"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Rename VM</button>
      </form>
      <form class="vm-resource-form" data-action="vm-network-connect" data-vm-id="${pcvEscapeHtml(vmId)}">
        <select name="switch" aria-label="Switch to connect"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
        <button type="submit"${lockedMutationDisabled}>Connect switch</button>
      </form>
      <form class="vm-resource-form" data-action="vm-device-add" data-vm-id="${pcvEscapeHtml(vmId)}">
        <select name="device" aria-label="Device kind"${lockedMutationDisabled}><option value="nic">Network adapter</option><option value="dvd">DVD drive</option></select>
        <select name="switch" aria-label="Network adapter switch"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
        <input name="iso_path" type="text" placeholder="DVD ISO path (optional)" aria-label="DVD ISO path"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Add device</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-memory" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="memory_mb" type="number" min="512" max="262144" step="128" placeholder="Memory MB" aria-label="memory MB"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Set memory</button>
      </form>
      <form class="vm-resource-form" data-action="vm-set-vcpu" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="cpu" type="number" min="1" max="32" step="1" placeholder="vCPU" aria-label="vCPU"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Set vCPU</button>
      </form>
      <form class="vm-resource-form" data-action="vm-disk-resize" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="disk_gb" type="number" min="8" max="4096" step="1" placeholder="Disk GB" aria-label="disk GB"${lockedMutationDisabled}>
        <button type="submit"${lockedMutationDisabled}>Resize disk</button>
      </form>
      <form class="vm-resource-form" data-action="vm-clone" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="name" autocomplete="off" placeholder="Target VM name" aria-label="clone target name"${actionDisabled}>
        <button type="submit" data-action="vm-clone"${actionDisabled}>Clone VM</button>
      </form>
    </div>
    <div class="details-grid detail-grid">
      ${details.map(([label, value]) => `<div class="kv"><span>${pcvEscapeHtml(label)}</span><strong>${pcvEscapeHtml(formatObjectValue(value))}</strong></div>`).join('')}
    </div>
    ${renderVmConsoleFrameCard(vm, vmId, canViewConsole)}
    ${renderExportImportReadback(vm, vmId, actionDisabled)}
    ${renderVmQosGuestReadback(vmId)}
    ${renderVmQosDirectControl(vmId)}
    <div class="checkpoint-panel">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">Checkpoints</p>
          <h3>VM Checkpoints</h3>
        </div>
        <button data-action="checkpoint-refresh" data-vm-id="${pcvEscapeHtml(vmId)}"${checkpointRefreshDisabled}>Refresh checkpoints</button>
      </div>
      ${renderCheckpointScheduleReadback(vm, vmId, actionDisabled)}
      <form class="checkpoint-form" data-action="checkpoint-create" data-vm-id="${pcvEscapeHtml(vmId)}">
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
        <div class="diagnostics-fact"><span class="muted">export</span><strong>${pcvEscapeHtml(exportEligible ? 'eligible' : 'blocked')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">managed</span><strong>${pcvEscapeHtml(managed ? 'yes' : 'no')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">generation</span><strong>${pcvEscapeHtml(formatObjectValue(vm?.generation))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">power</span><strong>${pcvEscapeHtml(formatObjectValue(vm?.state || vm?.status))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">security features</span><strong>${pcvEscapeHtml(securityLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">import</span><strong>new identity</strong></div>
      </div>
      <form class="vm-resource-form export-import-form" data-action="vm-export" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="directory" type="text" placeholder="Export directory" aria-label="Export directory"${actionDisabled}>
        <button type="submit" data-action="vm-export-preview"${actionDisabled}>Preview export</button>
        <button type="submit" data-action="vm-export-apply"${actionDisabled}>Export VM</button>
      </form>
      <form class="vm-resource-form export-import-form" data-action="vm-import" data-vm-id="${pcvEscapeHtml(vmId)}">
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
        <div class="diagnostics-fact"><span class="muted">schedule</span><strong>${pcvEscapeHtml(enabled ? 'enabled' : 'disabled')}</strong></div>
        <div class="diagnostics-fact"><span class="muted">status</span><strong>${pcvEscapeHtml(statusLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">interval minutes</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.interval_minutes))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">retention max</span><strong>${pcvEscapeHtml(formatObjectValue(retention))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">last enqueued</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.last_enqueued_at))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">next due</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.next_due_at))}</strong></div>
      </div>
      <form class="vm-resource-form checkpoint-schedule-form" data-action="checkpoint-schedule" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="interval_minutes" type="number" min="1" step="1" placeholder="Interval minutes" aria-label="Checkpoint interval minutes"${actionDisabled}>
        <input name="retention_max" type="number" min="1" step="1" placeholder="Retention max" aria-label="Checkpoint retention max"${actionDisabled}>
        <button type="submit" data-action="checkpoint-schedule-preview"${actionDisabled}>Preview schedule</button>
        <button type="submit" data-action="checkpoint-schedule-set"${actionDisabled}>Save schedule</button>
        <button type="button" class="danger-button" data-action="checkpoint-schedule-clear" data-vm-id="${pcvEscapeHtml(vmId)}"${actionDisabled}>Clear schedule</button>
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
    ? `<span>${pcvEscapeHtml(activity.operation || 'job')}</span>${stateBadge(activity.status)}`
    : '<span class="muted">No related current activity visible.</span>';

  els.vmWorkbenchContext.innerHTML = `
    <div class="vm-context-card">
      <div>
        <span class="muted">Selected VM</span>
        <strong>${pcvEscapeHtml(vmName)}</strong>
      </div>
      <div>${stateBadge(vm.state || vm.status)}</div>
      <div class="vm-context-activity">
        <span class="muted">Related activity</span>
        ${activityHtml}
      </div>
    </div>`;
}

// --- legacy render-qos.ts ---
function renderCheckpointList(vmId) {
  const checkpoints = asArray(state.selectedVmCheckpoints);
  const canOperate = rbacAllows('operate');
  const templateLocked = isTemplateLockedVm(state.selectedVm);
  if (checkpoints.length === 0) {
    return '<p class="muted">No checkpoints returned for this VM.</p>';
  }

  return checkpoints.map((checkpoint) => {
    const checkpointId = getCheckpointId(checkpoint);
    const checkpointDisabled = isCheckpointActionPending(vmId, checkpointId) || !canOperate || templateLocked ? ' disabled' : '';
    return `<div class="checkpoint-row">
      <div>
        <strong>${pcvEscapeHtml(getCheckpointName(checkpoint))}</strong>
        <div class="muted">${pcvEscapeHtml(formatCheckpointMeta(checkpoint))}</div>
      </div>
      <div class="checkpoint-actions">
        <button data-action="checkpoint-restore" data-vm-id="${pcvEscapeHtml(vmId)}" data-checkpoint-id="${pcvEscapeHtml(checkpointId)}"${checkpointDisabled}>Restore</button>
        <button class="danger-button" data-action="checkpoint-delete" data-vm-id="${pcvEscapeHtml(vmId)}" data-checkpoint-id="${pcvEscapeHtml(checkpointId)}"${checkpointDisabled}>Delete</button>
      </div>
    </div>`;
  }).join('');
}

function getSelectedVmReadbacks(vmId) {
  const readbacks = state.selectedVmReadbacks;
  return readbacks && readbacks.vm_id === vmId ? readbacks : null;
}

function readbackBucket(payload, bucketName) {
  const record = asObject(payload);
  return asObject(record[bucketName] || readNested(record, ['data', bucketName]));
}

function readbackFieldSummary(payload, bucketName, fields) {
  const bucket = readbackBucket(payload, bucketName);
  const parts = fields
    .map((field) => {
      const value = bucket[field];
      return value === undefined || value === null || value === '' ? '' : `${field}=${formatObjectValue(value)}`;
    })
    .filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : 'payload available';
}

function readbackErrorFor(readbacks, key) {
  return asArray(readbacks?.errors).find((error) => error.key === key) || null;
}

function renderReadbackCard(readbacks, key, title, bucketName, fields) {
  const payload = readbacks?.values?.[key];
  const error = readbackErrorFor(readbacks, key);
  const status = error ? 'degraded' : payload ? 'available' : readbacks?.loading ? 'loading' : 'pending';
  const body = error
    ? `${error.code}: ${error.message}`
    : payload
      ? readbackFieldSummary(payload, bucketName, fields)
      : readbacks?.loading
        ? 'Loading'
        : 'Not loaded';
  const detail = error?.detail || '';
  return `<div class="qos-readback-card">
    <span class="muted">${pcvEscapeHtml(title)}</span>
    ${stateBadge(status)}
    <strong>${pcvEscapeHtml(body)}</strong>
    ${detail ? `<p class="muted">${pcvEscapeHtml(detail)}</p>` : ''}
  </div>`;
}

function renderVmQosGuestReadback(vmId) {
  const readbacks = getSelectedVmReadbacks(vmId);
  const updated = readbacks?.updated_at ? new Date(readbacks.updated_at).toLocaleString() : '-';
  const loadingLabel = readbacks?.loading ? 'Loading' : 'Refresh';
  return `<section class="qos-readback-panel">
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">QoS / Guest Readback</p>
        <h3>Hyper-V readback surface</h3>
      </div>
      <button data-action="vm-qos-guest-refresh" data-vm-id="${pcvEscapeHtml(vmId)}"${readbacks?.loading ? ' disabled' : ''}>${pcvEscapeHtml(loadingLabel)}</button>
    </div>
    <div class="qos-readback-grid">
      ${renderReadbackCard(readbacks, 'blkio', 'blkio', 'storage_qos', ['linux_blkio_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'bandwidth', 'bandwidth', 'network_qos', ['linux_bandwidth_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'guest_agent', 'guest-agent-status', 'guest_agent', ['status', 'qemu_guest_agent', 'guest_exec_supported'])}
      ${renderReadbackCard(readbacks, 'guest_ping', 'guest-ping', 'guest_ping', ['reachable', 'guest_heartbeat_verified'])}
      ${renderReadbackCard(readbacks, 'memory_stats', 'memory-stats', 'memory', ['startup_mb', 'assigned_mb', 'dynamic'])}
      ${renderReadbackCard(readbacks, 'cpu_stats', 'cpu-stats', 'cpu', ['count'])}
    </div>
    <p class="muted">updated_at=${pcvEscapeHtml(updated)} / vm.limit remains CLI/API queued mutation</p>
  </section>`;
}

function getSelectedVmQosControl(vmId) {
  const control = state.selectedVmQosControl;
  return control && control.vm_id === vmId ? control : null;
}

function renderQosControlResult(control) {
  if (!control) {
    return '<p class="muted">No QoS direct-control preview or apply has been run for this VM in this browser session.</p>';
  }

  const status = control.error ? 'degraded' : control.loading ? 'loading' : control.mode || 'tracked';
  const detail = control.error
    ? `${control.error.code}: ${control.error.message}`
    : control.result
      ? formatObjectValue(control.result)
      : 'pending';
  const updated = control.updated_at ? new Date(control.updated_at).toLocaleString() : '-';
  return `<div class="qos-control-result">
    <span class="muted">${pcvEscapeHtml(control.kind || 'qos')} ${pcvEscapeHtml(control.mode || 'control')}</span>
    ${stateBadge(status)}
    <strong>${pcvEscapeHtml(detail)}</strong>
    <p class="muted">updated_at=${pcvEscapeHtml(updated)}</p>
  </div>`;
}

function renderVmQosDirectControl(vmId) {
  const canOperate = rbacAllows('operate');
  const canGuestExec = rbacAllows('guest.exec');
  const canGuestChannel = rbacAllows('guest.channel.configure');
  const templateLocked = isTemplateLockedVm(state.selectedVm);
  const actionDisabled = isVmActionPending(vmId) || !canOperate || templateLocked ? ' disabled' : '';
  const guestExecDisabled = isVmActionPending(vmId) || !canGuestExec || templateLocked ? ' disabled' : '';
  const guestChannelDisabled = isVmActionPending(vmId) || !canGuestChannel || templateLocked ? ' disabled' : '';
  const control = getSelectedVmQosControl(vmId);
  return `<section class="qos-control-panel">
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">QoS Direct Control</p>
        <h3>ADR-0008 preview and apply</h3>
      </div>
      <span class="status-badge ${canOperate ? 'ok' : 'warn'}">${canOperate ? 'operate' : 'operate required'}</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-qos-kind="storage" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>Disk<input name="disk" autocomplete="off" value="disk0"${actionDisabled}></label>
        <label>Maximum IOPS<input name="maximum_iops" type="number" min="0" step="1" value="120"${actionDisabled}></label>
        <label>Minimum IOPS<input name="minimum_iops" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-storage-preview"${actionDisabled}>Preview</button>
          <button type="submit" class="danger-button" data-action="vm-qos-storage-apply"${actionDisabled}>Apply</button>
        </div>
      </form>
      <form class="qos-control-form" data-qos-kind="network" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>Adapter<input name="adapter" autocomplete="off" value="adapter0"${actionDisabled}></label>
        <label>Maximum Kbps<input name="maximum_kbps" type="number" min="0" step="1" value="20480"${actionDisabled}></label>
        <label>Minimum Kbps<input name="minimum_kbps" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-network-preview"${actionDisabled}>Preview</button>
          <button type="submit" class="danger-button" data-action="vm-qos-network-apply"${actionDisabled}>Apply</button>
        </div>
      </form>
    </div>
    ${renderQosControlResult(control)}
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">Guest Execution Direct Control</p>
        <h3>ADR-0009 queued execution and channel lifecycle</h3>
      </div>
      <span class="status-badge ${canGuestExec && canGuestChannel ? 'ok' : 'warn'}">guest.exec + guest.channel.configure required</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-guest-execution-kind="exec" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>Credential reference<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestExecDisabled}></label>
        <label>Timeout seconds<input name="timeout_sec" type="number" min="1" max="600" step="1" value="60"${guestExecDisabled}></label>
        <label>Command<input name="command" autocomplete="off" placeholder="hostname"${guestExecDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-guest-exec-preview"${guestExecDisabled}>Preview exec</button>
          <button type="submit" class="danger-button" data-action="vm-guest-exec"${guestExecDisabled}>Queue exec</button>
        </div>
      </form>
      <form class="qos-control-form" data-guest-execution-kind="channel" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>Credential reference<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestChannelDisabled}></label>
        <label>Timeout seconds<input name="timeout_sec" type="number" min="1" max="600" step="1" value="30"${guestChannelDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="guest-agent-channel-preview"${guestChannelDisabled}>Preview channel</button>
          <button type="submit" data-action="guest-agent-ensure-channel" data-guest-channel-mode="verify"${guestChannelDisabled}>Verify channel</button>
          <button type="submit" class="danger-button" data-action="guest-agent-ensure-channel" data-guest-channel-mode="repair"${guestChannelDisabled}>Repair channel</button>
        </div>
      </form>
      <form class="qos-control-form" data-action="vm-guest-file" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>Host path<input name="host_path" autocomplete="off" placeholder="C:\\ProgramData\\PureCVisor\\desktop-node\\guest-files\\payload.iso"${guestExecDisabled}></label>
        <label>Guest path<input name="guest_path" autocomplete="off" placeholder="C:\\Users\\Public\\PureCVisor\\payload.iso"${guestExecDisabled}></label>
        <label>Credential reference<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestExecDisabled}></label>
        <label>Timeout seconds<input name="timeout_sec" type="number" min="1" max="600" step="1" value="60"${guestExecDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" class="danger-button" data-action="vm-guest-file"${guestExecDisabled}>Copy host file</button>
        </div>
      </form>
    </div>
    <p class="muted">Guest command output is reduced to audit digests; raw stdout/stderr and credential values are not rendered.</p>
    <p class="muted">Guest file copy is host-to-guest only, allowlisted, and does not use HGFS.</p>
    <p class="muted">Account/noVNC target config mutation remains ADR-0010 deferred.</p>
  </section>`;
}

// --- legacy mutate.ts ---
async function queueVmLifecycle(vmId, action) {
  requireRbac('operate', `VM ${action}`);
  const requiresConfirmation = action === 'poweroff' || action === 'restart' || action === 'save' || action === 'resume-saved';
  if (requiresConfirmation && !window.confirm(buildVmLifecycleConfirmation(vmId, action))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, action);
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmAction(vmId, action);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmAttach(vmId, isoPath) {
  requireRbac('operate', 'VM attach');
  const path = String(isoPath || '').trim();
  if (!path) {
    throw normalizeError({
      code: 'PCV_VM_ATTACH_ISO_REQUIRED',
      message: 'Enter an ISO path.',
      detail: 'iso_path is required before queueing vm.attach.'
    });
  }
  if (!window.confirm(buildVmAttachConfirmation(vmId, path))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'attach');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmAttach(vmId, path);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmResourceMutation(vmId, action, valueName, rawValue) {
  requireRbac('operate', `VM ${action}`);
  const value = Number.parseInt(String(rawValue ?? ''), 10);
  if (!Number.isFinite(value)) {
    throw normalizeError({
      code: 'PCV_VM_RESOURCE_VALUE_REQUIRED',
      message: 'Enter a numeric VM resource value.',
      detail: `${valueName} must be an integer before queueing ${action}.`
    });
  }

  state.actionPending = true;
  setVmActionPending(vmId, action);
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmResourceMutation(vmId, action, { [valueName]: value });
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

function readRequiredText(formData, name, errorCode, message) {
  const value = String(formData.get(name) || '').trim();
  if (!value) {
    throw normalizeError({
      code: errorCode,
      message,
      detail: `${name} is required before queueing this operation.`
    });
  }

  return value;
}

function readNonNegativeInt(formData, name) {
  const value = Number.parseInt(String(formData.get(name) ?? ''), 10);
  if (!Number.isFinite(value) || value < 0) {
    throw normalizeError({
      code: 'PCV_VM_QOS_VALUE_INVALID',
      message: 'Enter a non-negative QoS value.',
      detail: `${name} must be an integer greater than or equal to zero.`
    });
  }

  return value;
}

function readBoundedInt(formData, name, min, max, errorCode, message) {
  const value = Number.parseInt(String(formData.get(name) ?? ''), 10);
  if (!Number.isFinite(value) || value < min || value > max) {
    throw normalizeError({
      code: errorCode,
      message,
      detail: `${name} must be an integer between ${min} and ${max}.`
    });
  }

  return value;
}

function readVmQosPayload(kind, formData) {
  if (kind === 'storage') {
    return {
      disk: readRequiredText(formData, 'disk', 'PCV_VM_QOS_STORAGE_DISK_REQUIRED', 'Enter a storage disk before previewing or applying QoS.'),
      maximum_iops: readNonNegativeInt(formData, 'maximum_iops'),
      minimum_iops: readNonNegativeInt(formData, 'minimum_iops')
    };
  }

  return {
    adapter: readRequiredText(formData, 'adapter', 'PCV_VM_QOS_NETWORK_ADAPTER_REQUIRED', 'Enter a network adapter before previewing or applying QoS.'),
    maximum_kbps: readNonNegativeInt(formData, 'maximum_kbps'),
    minimum_kbps: readNonNegativeInt(formData, 'minimum_kbps')
  };
}

function readVmGuestExecPayload(formData) {
  return {
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED',
      'Enter a protected credential reference before queueing guest execution.'),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      'Enter a guest execution timeout between 1 and 600 seconds.'),
    command: [
      readRequiredText(
        formData,
        'command',
        'PCV_GUEST_EXEC_COMMAND_REQUIRED',
        'Enter a guest command before queueing guest execution.')
    ]
  };
}

function readVmGuestFilePayload(formData) {
  return {
    host_path: readRequiredText(
      formData,
      'host_path',
      'PCV_GUEST_FILE_PATH_NOT_ALLOWED',
      'Enter an allowlisted host file path before previewing or copying.'),
    guest_path: readRequiredText(
      formData,
      'guest_path',
      'PCV_GUEST_FILE_PATH_NOT_ALLOWED',
      'Enter an allowlisted guest file path before previewing or copying.'),
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_FILE_CREDENTIAL_REF_REQUIRED',
      'Enter a protected credential reference before previewing or copying a guest file.'),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      'Enter a guest file timeout between 1 and 600 seconds.'),
    direction: 'host-to-guest'
  };
}

function readVmGuestChannelPayload(formData, mode) {
  if (mode === 'repair') {
    return { yes: true };
  }

  return {
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED',
      'Enter a protected credential reference before verifying the guest channel.'),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      'Enter a guest channel timeout between 1 and 600 seconds.')
  };
}

function buildVmQosConfirmation(vmId, kind, payload) {
  const target = kind === 'storage' ? payload.disk : payload.adapter;
  const maximum = kind === 'storage' ? payload.maximum_iops : payload.maximum_kbps;
  const minimum = kind === 'storage' ? payload.minimum_iops : payload.minimum_kbps;
  return [
    `Apply ${kind} QoS policy to VM ${vmId}?`,
    `Target: ${target}`,
    `Maximum: ${maximum}`,
    `Minimum: ${minimum}`,
    'This queues a Hyper-V host mutation using the ADR-0008 policy contract.',
    'Use preview first when changing a non-reset value.'
  ].join('\n');
}

function buildVmGuestExecutionConfirmation(vmId, mode) {
  return [
    `Queue ${mode} for VM ${vmId}?`,
    'This operation uses a protected credential reference and writes guest-execution-audit-v1 evidence.',
    'Raw stdout/stderr and credential values are not rendered in the Web Console.'
  ].join('\n');
}

async function queueVmQosDirectControl(vmId, kind, mode, payload) {
  requireRbac('operate', `VM ${kind} QoS ${mode}`);
  const apply = mode === 'apply';
  if (apply && !window.confirm(buildVmQosConfirmation(vmId, kind, payload))) {
    return;
  }

  const actionKey = `qos-${kind}-${mode}`;
  state.actionPending = true;
  setVmActionPending(vmId, actionKey);
  state.error = null;
  state.selectedVmQosControl = {
    vm_id: vmId,
    kind,
    mode,
    loading: true,
    updated_at: '',
    result: null,
    error: null
  };
  render();
  try {
    const result = kind === 'storage'
      ? apply
        ? await desktopApi.applyVmQosStorage(vmId, payload)
        : await desktopApi.previewVmQosStorage(vmId, payload)
      : apply
        ? await desktopApi.applyVmQosNetwork(vmId, payload)
        : await desktopApi.previewVmQosNetwork(vmId, payload);

    if (apply) {
      pcvTrackJob(result);
      startPolling();
    }

    state.selectedVmQosControl = {
      vm_id: vmId,
      kind,
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result,
      error: null
    };
    state.connectionState = 'connected';
    state.shellMessage = `VM ${kind} QoS ${mode} completed for ${vmId}.`;
  } catch (error) {
    const normalized = normalizeError(error);
    state.error = normalized;
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind,
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result: null,
      error: normalized
    };
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmGuestExecutionControl(vmId, mode, payload) {
  const requiredPermission = mode === 'exec' ? 'guest.exec' : 'guest.channel.configure';
  requireRbac(requiredPermission, `VM guest ${mode}`);
  if ((mode === 'exec' || mode === 'repair') && !window.confirm(buildVmGuestExecutionConfirmation(vmId, mode))) {
    return;
  }

  const actionKey = `guest-${mode}`;
  state.actionPending = true;
  setVmActionPending(vmId, actionKey);
  state.error = null;
  state.selectedVmQosControl = {
    vm_id: vmId,
    kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
    mode,
    loading: true,
    updated_at: '',
    result: null,
    error: null
  };
  render();
  try {
    const result = mode === 'exec'
      ? await desktopApi.queueVmGuestExec(vmId, payload)
      : mode === 'verify'
        ? await desktopApi.verifyVmGuestChannel(vmId, payload)
        : await desktopApi.ensureVmGuestChannel(vmId, payload);

    pcvTrackJob(result);
    startPolling();
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result,
      error: null
    };
    state.connectionState = 'connected';
    state.shellMessage = `VM guest ${mode} queued for ${vmId}.`;
  } catch (error) {
    const normalized = normalizeError(error);
    state.error = normalized;
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result: null,
      error: normalized
    };
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function refreshVmDeleteStatus(vmId) {
  requireRbac('read', 'VM delete status');
  state.actionPending = true;
  setVmActionPending(vmId, 'delete-status');
  state.error = null;
  render();
  try {
    const status = await desktopApi.getVmDeleteStatus(vmId);
    state.shellMessage = `Delete status ${status?.name || vmId}: ${status?.status || 'unknown'}`;
    state.connectionState = 'connected';
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmManage(vmId) {
  requireRbac('operate', 'VM manage');
  const vm = state.selectedVm || findCachedVm(vmId);
  if (!window.confirm(buildVmManageConfirmation(vmId, vm))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'manage');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmManage(vmId, vmId);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmGuestFile(vmId, payload) {
  requireRbac('guest.exec', 'VM guest file');
  state.actionPending = true;
  setVmActionPending(vmId, 'guest-file');
  state.error = null;
  render();
  try {
    const preview = await desktopApi.previewVmGuestFile(vmId, payload);
    if (!window.confirm(buildVmGuestFileConfirmation(vmId, payload, preview))) {
      return;
    }

    const job = await desktopApi.queueVmGuestFile(vmId, payload);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmTemplateLock(vmId, locked) {
  requireRbac('operate', 'VM template lock');
  const vm = state.selectedVm || findCachedVm(vmId);
  if (!window.confirm(buildVmTemplateLockConfirmation(vmId, vm, locked))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, locked ? 'template-lock' : 'template-unlock');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmTemplateLock(vmId, vmId, locked);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmClone(vmId, rawName) {
  requireRbac('operate', 'VM clone');
  const name = String(rawName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_VM_CLONE_NAME_REQUIRED',
      message: 'Enter a target VM name.',
      detail: 'name is required before queueing vm.clone.'
    });
  }

  const vm = state.selectedVm || findCachedVm(vmId);
  const payload = { confirm_name: vmId, name };

  state.actionPending = true;
  setVmActionPending(vmId, 'clone');
  state.error = null;
  render();
  try {
    const preview = await desktopApi.previewVmClone(vmId, payload);
    if (!window.confirm(buildVmCloneConfirmation(vmId, vm, name, preview))) {
      return;
    }

    const job = await desktopApi.queueVmClone(vmId, payload);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmDelete(vmId) {
  requireRbac('operate', 'VM delete');
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
  setVmActionPending(vmId, 'delete');
  state.error = null;
  render();
  try {
    const job = await desktopApi.deleteVm(vmId);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    await loadVms();
    await refreshSelectedVm();
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueCheckpointCreate(vmId, checkpointName) {
  requireRbac('operate', 'checkpoint create');
  const name = String(checkpointName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_FORM_INVALID',
      message: 'Checkpoint name is required.',
      detail: 'Enter a checkpoint name before creating a checkpoint.'
    });
  }

  state.checkpointPending = true;
  setCheckpointActionPending(vmId, 'create', 'create');
  state.error = null;
  render();
  try {
    const job = await desktopApi.createCheckpoint(vmId, name);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, 'create');
  }
}

async function queueCheckpointRestore(vmId, checkpointId) {
  requireRbac('operate', 'checkpoint restore');
  if (!window.confirm(buildCheckpointRestoreConfirmation(vmId, checkpointId))) {
    return;
  }
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'restore');
  state.error = null;
  render();
  try {
    const job = await desktopApi.restoreCheckpoint(vmId, checkpointId);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}

async function queueCheckpointDelete(vmId, checkpointId) {
  requireRbac('operate', 'checkpoint delete');
  if (!window.confirm(buildCheckpointDeleteConfirmation(vmId, checkpointId))) {
    return;
  }
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'delete');
  state.error = null;
  render();
  try {
    const job = await desktopApi.deleteCheckpoint(vmId, checkpointId);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}

// exports
window.matchesVmFilter = matchesVmFilter;
window.getVmUpdatedValue = getVmUpdatedValue;
window.compareVms = compareVms;
window.renderVms = renderVms;
window.getNetworkInventory = getNetworkInventory;
window.getNetworkSwitches = getNetworkSwitches;
window.formatNetworkBoolean = formatNetworkBoolean;
window.renderNetworkFailureGuidance = renderNetworkFailureGuidance;
window.renderNetworkChangeReadback = renderNetworkChangeReadback;
window.renderNetworkInventory = renderNetworkInventory;
window.renderVmDetail = renderVmDetail;
window.renderExportImportReadback = renderExportImportReadback;
window.renderCheckpointScheduleReadback = renderCheckpointScheduleReadback;
window.renderVmWorkbenchContext = renderVmWorkbenchContext;
window.renderCheckpointList = renderCheckpointList;
window.getSelectedVmReadbacks = getSelectedVmReadbacks;
window.readbackBucket = readbackBucket;
window.readbackFieldSummary = readbackFieldSummary;
window.readbackErrorFor = readbackErrorFor;
window.renderReadbackCard = renderReadbackCard;
window.renderVmQosGuestReadback = renderVmQosGuestReadback;
window.getSelectedVmQosControl = getSelectedVmQosControl;
window.renderQosControlResult = renderQosControlResult;
window.renderVmQosDirectControl = renderVmQosDirectControl;
window.queueVmLifecycle = queueVmLifecycle;
window.queueVmAttach = queueVmAttach;
window.queueVmResourceMutation = queueVmResourceMutation;
window.readRequiredText = readRequiredText;
window.readNonNegativeInt = readNonNegativeInt;
window.readBoundedInt = readBoundedInt;
window.readVmQosPayload = readVmQosPayload;
window.readVmGuestExecPayload = readVmGuestExecPayload;
window.readVmGuestFilePayload = readVmGuestFilePayload;
window.readVmGuestChannelPayload = readVmGuestChannelPayload;
window.buildVmQosConfirmation = buildVmQosConfirmation;
window.buildVmGuestExecutionConfirmation = buildVmGuestExecutionConfirmation;
window.queueVmQosDirectControl = queueVmQosDirectControl;
window.queueVmGuestExecutionControl = queueVmGuestExecutionControl;
window.refreshVmDeleteStatus = refreshVmDeleteStatus;
window.queueVmManage = queueVmManage;
window.queueVmGuestFile = queueVmGuestFile;
window.queueVmTemplateLock = queueVmTemplateLock;
window.queueVmClone = queueVmClone;
window.queueVmDelete = queueVmDelete;
window.queueCheckpointCreate = queueCheckpointCreate;
window.queueCheckpointRestore = queueCheckpointRestore;
window.queueCheckpointDelete = queueCheckpointDelete;
PCV.vm = Object.assign(PCV.vm || {}, { matchesVmFilter, getVmUpdatedValue, compareVms, renderVms, getNetworkInventory, getNetworkSwitches, formatNetworkBoolean, renderNetworkFailureGuidance, renderNetworkChangeReadback, renderNetworkInventory, renderVmDetail, renderExportImportReadback, renderCheckpointScheduleReadback, renderVmWorkbenchContext, renderCheckpointList, getSelectedVmReadbacks, readbackBucket, readbackFieldSummary, readbackErrorFor, renderReadbackCard, renderVmQosGuestReadback, getSelectedVmQosControl, renderQosControlResult, renderVmQosDirectControl, queueVmLifecycle, queueVmAttach, queueVmResourceMutation, readRequiredText, readNonNegativeInt, readBoundedInt, readVmQosPayload, readVmGuestExecPayload, readVmGuestFilePayload, readVmGuestChannelPayload, buildVmQosConfirmation, buildVmGuestExecutionConfirmation, queueVmQosDirectControl, queueVmGuestExecutionControl, refreshVmDeleteStatus, queueVmManage, queueVmGuestFile, queueVmTemplateLock, queueVmClone, queueVmDelete, queueCheckpointCreate, queueCheckpointRestore, queueCheckpointDelete });
})(window.PCV);

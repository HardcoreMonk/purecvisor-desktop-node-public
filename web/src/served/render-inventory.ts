// @ts-nocheck
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
    <tr${selected} data-vm-id="${escapeHtml(vmId)}">
      <td><button type="button" class="link-button" data-action="select-vm" data-vm-id="${escapeHtml(vmId)}">${escapeHtml(getVmName(vm))}</button></td>
      <td>${stateBadge(vm.state || vm.status)}</td>
      <td>${escapeHtml(vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count)}</td>
      <td>${escapeHtml(vm.memory?.startup_mb ?? vm.memory_mb ?? vm.memory ?? vm.memory_assigned_mb)}</td>
      <td>${escapeHtml(vm.generation)}</td>
      <td>${escapeHtml(vm.uptime || vm.updated_at || vm.created_at)}</td>
      <td>${escapeHtml(vm.error?.message || vm.notes || '-')}</td>
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
    <strong>${escapeHtml(code)}</strong>
    <span>${escapeHtml(error.message || 'Network inventory unavailable.')}</span>
    <p>${escapeHtml(detail)}</p>
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
        <td><strong>${escapeHtml(item?.name || '-')}</strong></td>
        <td>${escapeHtml(item?.type || item?.switch_type || '-')}</td>
        <td>${isDefault ? stateBadge('default') : '-'}</td>
        <td>${escapeHtml(formatNetworkBoolean(item?.allow_management_os))}</td>
        <td>${escapeHtml(item?.net_adapter_interface_description || item?.adapter || item?.adapter_name || '-')}</td>
      </tr>`;
  }).join('');

  els.networkInventoryPanel.innerHTML = `
    ${errorHtml}
    ${renderTableStateSummary('Switches', switches.length, allSwitches.length, state.networkFilter, 'read-only Hyper-V inventory')}
    <div class="network-summary-grid">
      <div class="network-summary-card"><span class="muted">Source</span><strong>${escapeHtml(source)}</strong></div>
      <div class="network-summary-card"><span class="muted">Mutation</span><strong>${escapeHtml(mutationMode)}</strong></div>
      <div class="network-summary-card"><span class="muted">Switches</span><strong>${escapeHtml(switches.length)}</strong></div>
      <div class="network-summary-card"><span class="muted">Default</span><strong>${escapeHtml(defaultSwitchCount)}</strong></div>
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


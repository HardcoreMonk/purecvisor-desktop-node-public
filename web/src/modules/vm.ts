// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): VM inventory, detail, QoS and lifecycle mutations (legacy render-inventory, render-vm-detail, render-qos, mutate).
// Top-level declarations stay on window.PCV so the module keeps the shared-scope behaviour of the classic-script bundle.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-inventory.ts ---
function vmQueryText(vm) {
  return [
    getVmId(vm),
    getVmName(vm),
    vm?.state,
    vm?.status,
    vm?.notes,
    vm?.error?.message
  ].join(' ').toLowerCase();
}

function matchesVmQuery(vm) {
  const query = String(state.vmFilter || '').trim().toLowerCase();
  if (!query) return true;
  return vmQueryText(vm).includes(query);
}

function matchesVmStateChip(vm, stateFilter) {
  const filter = String(stateFilter || 'all').toLowerCase();
  if (!filter || filter === 'all') return true;
  if (filter === 'template') return isTemplateLockedVm(vm);
  if (filter === 'running') return vmPowerKind(vm) === 'running';
  if (filter === 'off') return vmPowerKind(vm) === 'off';
  if (filter === 'failed') return getVmState(vm).toLowerCase().includes('failed');
  return getVmState(vm).toLowerCase().includes(filter);
}

function matchesVmFilter(vm) {
  const stateFilter = String(state.vmStateFilter || 'all').toLowerCase();
  if (!matchesVmStateChip(vm, stateFilter)) return false;
  return matchesVmQuery(vm);
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

function vmScalarText(value) {
  if (value == null || value === '') return '-';
  return String(value);
}

function vmCpuText(vm) {
  return vmScalarText(vm?.cpu?.count ?? vm?.cpu ?? vm?.vcpu ?? vm?.processor_count);
}

function vmMemoryText(vm) {
  return vmScalarText(vm?.memory?.startup_mb ?? vm?.memory_mb ?? vm?.memory ?? vm?.memory_assigned_mb);
}

function vmListPrimaryButton(vm) {
  const rawId = getVmId(vm);
  const vmId = pcvEscapeHtml(rawId);
  const pending = isVmActionPending(rawId);
  const operate = typeof rbacAllows !== 'function' || rbacAllows('operate');
  const actionDisabled = pending || !operate ? ' disabled' : '';
  if (isTemplateLockedVm(vm)) {
    return `<button type="button" class="btn pcv-row-action" data-action="vm-row-clone" data-vm-id="${vmId}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.clone'))}</button>`;
  }
  const kind = vmPowerKind(vm);
  if (kind === 'running') {
    const consoleDisabled = typeof rbacAllows === 'function' && !rbacAllows('console.view') ? ' disabled' : '';
    return `<button type="button" class="btn pcv-row-action" data-action="vm-row-console" data-vm-id="${vmId}"${consoleDisabled}>${pcvEscapeHtml(t('vm.list.console'))}</button>`;
  }
  if (kind === 'off') {
    return `<button type="button" class="btn pcv-row-action" data-action="vm-row-start" data-vm-id="${vmId}"${actionDisabled}>${pcvEscapeHtml(t('vm.power.start'))}</button>`;
  }
  return '';
}

function paintVmChips(named) {
  const chips = document.querySelectorAll('#vm-state-chips [data-vm-chip]');
  if (!chips || !chips.length) return;
  const current = String(state.vmStateFilter || 'all').toLowerCase();
  const counts = {
    all: named.length,
    running: 0,
    off: 0,
    template: 0,
    failed: named.filter((vm) => matchesVmStateChip(vm, 'failed')).length
  };
  named.forEach((vm) => {
    const kind = vmPowerKind(vm);
    if (kind === 'running') counts.running += 1;
    if (kind === 'off') counts.off += 1;
    if (isTemplateLockedVm(vm)) counts.template += 1;
  });
  Array.prototype.forEach.call(chips, (chip) => {
    const key = chip.getAttribute('data-vm-chip');
    const count = chip.querySelector('[data-chip-count]');
    if (count && Object.prototype.hasOwnProperty.call(counts, key)) count.textContent = String(counts[key]);
    const on = key === current;
    chip.classList.toggle('is-active', on);
    chip.setAttribute('aria-pressed', on ? 'true' : 'false');
  });
}

function templateRowButton(vmId, action, label, disabled) {
  const button = PCV.uxlib.el('button', {
    type: 'button',
    class: 'btn pcv-row-action',
    'data-action': action,
    'data-vm-id': vmId
  }, label);
  button.disabled = !!disabled;
  return button;
}

function renderTemplates() {
  const host = document.getElementById('template-table');
  if (!host || !PCV.uxlib || !PCV.components) return;
  PCV.uxlib.clearEl(host);
  const locked = asArray(state.vms).filter((vm) => isTemplateLockedVm(vm)).sort(compareVms);
  if (locked.length === 0) {
    host.appendChild(PCV.components.emptyState({
      titleKey: 'vm.template.empty.title',
      bodyKey: 'vm.template.empty.body'
    }));
    return;
  }
  const pendingFor = (vm) => isVmActionPending(getVmId(vm));
  const operate = typeof rbacAllows !== 'function' || rbacAllows('operate');
  const head = PCV.uxlib.el('tr', {},
    PCV.uxlib.el('th', {}, t('vm.list.col.name')),
    PCV.uxlib.el('th', {}, t('vm.list.col.state')),
    PCV.uxlib.el('th', {}, t('vm.list.col.generation')),
    PCV.uxlib.el('th', {}, t('vm.list.col.cpu')),
    PCV.uxlib.el('th', {}, t('vm.list.col.memory')));
  const body = PCV.uxlib.el('tbody');
  locked.forEach((vm) => {
    const vmId = getVmId(vm);
    const disabled = pendingFor(vm) || !operate;
    const row = PCV.uxlib.el('tr', { 'data-vm-id': vmId },
      PCV.uxlib.el('td', {}, getVmName(vm)),
      PCV.uxlib.el('td', {}, getVmState(vm) || t('vm.state.unknown')),
      PCV.uxlib.el('td', {}, vmScalarText(vm?.generation)),
      PCV.uxlib.el('td', {}, vmCpuText(vm)),
      PCV.uxlib.el('td', {}, vmMemoryText(vm)),
      PCV.uxlib.el('td', {},
        templateRowButton(vmId, 'vm-row-clone', t('vm.menu.clone'), disabled),
        templateRowButton(vmId, 'vm-template-unlock', t('vm.menu.unlock'), disabled)));
    body.appendChild(row);
  });
  host.appendChild(PCV.uxlib.el('table', { class: 'pcv-vm-table' }, PCV.uxlib.el('thead', {}, head), body));
}

function renderVms() {
  const named = asArray(state.vms).filter(matchesVmQuery);
  paintVmChips(named);
  renderTemplates();
  if (!els.vmTable) return;
  const vms = named.filter((vm) => matchesVmStateChip(vm, state.vmStateFilter)).sort(compareVms);
  if (vms.length === 0) {
    const filtered = String(state.vmFilter || '').trim() || String(state.vmStateFilter || 'all') !== 'all';
    els.vmTable.innerHTML = `<p class="muted">${pcvEscapeHtml(t(filtered ? 'vm.list.empty_filter' : 'vm.list.empty'))}</p>`;
    return;
  }
  const rows = vms.map((vm) => {
    const vmId = getVmId(vm);
    const selected = vmId && vmId === state.selectedVmId ? ' class="selected-row"' : '';
    const templateBadge = isTemplateLockedVm(vm)
      ? `<span class="badge badge-warn">${pcvEscapeHtml(t('vm.badge.template'))}</span>`
      : '';
    return `
    <tr${selected} data-vm-id="${pcvEscapeHtml(vmId)}">
      <td>${pcvEscapeHtml(getVmName(vm))}</td>
      <td>${stateBadge(getVmState(vm) || vm.status)}</td>
      <td>${pcvEscapeHtml(vmScalarText(vm.generation))}</td>
      <td>${pcvEscapeHtml(vmCpuText(vm))}</td>
      <td>${pcvEscapeHtml(vmMemoryText(vm))}</td>
      <td>${templateBadge}</td>
      <td>${vmListPrimaryButton(vm)}</td>
    </tr>`;
  }).join('');
  els.vmTable.innerHTML = `
    <table class="pcv-vm-table">
      <thead><tr><th>${pcvEscapeHtml(t('vm.list.col.name'))}</th><th>${pcvEscapeHtml(t('vm.list.col.state'))}</th><th>${pcvEscapeHtml(t('vm.list.col.generation'))}</th><th>${pcvEscapeHtml(t('vm.list.col.cpu'))}</th><th>${pcvEscapeHtml(t('vm.list.col.memory'))}</th><th>${pcvEscapeHtml(t('vm.list.col.display'))}</th><th>${pcvEscapeHtml(t('vm.list.col.action'))}</th></tr></thead>
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
  if (value === true) return t('vm.net.enabled');
  if (value === false) return t('vm.net.disabled');
  return '-';
}

function renderNetworkFailureGuidance(error) {
  if (!error) return '';
  const code = error.code || 'PCV_NETWORK_INVENTORY_UNAVAILABLE';
  const parity = code === 'PCV_NATIVE_PARITY_INCOMPLETE' || String(error.detail || error.message || '').toLowerCase().includes('parity');
  const detail = parity
    ? t('vm.net.error_parity')
    : error.detail || t('vm.net.error_fallback');
  return `<div class="activity-warning network-error-state">
    <strong>${pcvEscapeHtml(code)}</strong>
    <span>${pcvEscapeHtml(error.message || t('vm.net.error_unavailable'))}</span>
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
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.switch_create'))}</span><strong>${pcvEscapeHtml(t('vm.net.service_only'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.product'))}</span><strong>${pcvEscapeHtml(productCount)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.reserved'))}</span><strong>${pcvEscapeHtml(reservedCount)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.external'))}</span><strong>${pcvEscapeHtml(externalCount === 0 ? t('vm.net.none') : t('vm.net.present'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.connect_label'))}</span><strong>${pcvEscapeHtml(t('vm.net.connect_value'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.net.nat'))}</span><strong>${pcvEscapeHtml(t('vm.net.excluded'))}</strong></div>
      </div>
      <div class="boundary-chip-row">
        <span>${pcvEscapeHtml(t('vm.net.chip_create'))}</span>
        <span>${pcvEscapeHtml(t('vm.net.chip_nat'))}</span>
        <span>${pcvEscapeHtml(t('vm.net.chip_dhcp'))}</span>
        <span>${pcvEscapeHtml(t('vm.net.chip_connect'))}</span>
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
  const mutationMode = inventory.mutating === true ? t('vm.net.mutating') : t('vm.net.readonly');
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
    ${renderTableStateSummary(t('vm.net.switches'), switches.length, allSwitches.length, state.networkFilter, t('vm.net.summary_note'))}
    ${renderNetworkChangeReadback(allSwitches)}
    <div class="network-summary-grid">
      <div class="network-summary-card"><span class="muted">${pcvEscapeHtml(t('vm.net.source'))}</span><strong>${pcvEscapeHtml(source)}</strong></div>
      <div class="network-summary-card"><span class="muted">${pcvEscapeHtml(t('vm.net.mutation'))}</span><strong>${pcvEscapeHtml(mutationMode)}</strong></div>
      <div class="network-summary-card"><span class="muted">${pcvEscapeHtml(t('vm.net.switches'))}</span><strong>${pcvEscapeHtml(switches.length)}</strong></div>
      <div class="network-summary-card"><span class="muted">${pcvEscapeHtml(t('vm.net.default'))}</span><strong>${pcvEscapeHtml(defaultSwitchCount)}</strong></div>
    </div>
    <div class="network-table-wrap">
      ${switches.length === 0
        ? `<p class="muted network-empty-state">${pcvEscapeHtml(t('vm.net.empty'))}</p>`
        : `<table class="network-table">
            <thead><tr><th>${pcvEscapeHtml(t('vm.net.col.name'))}</th><th>${pcvEscapeHtml(t('vm.net.col.type'))}</th><th>${pcvEscapeHtml(t('vm.net.col.default'))}</th><th>${pcvEscapeHtml(t('vm.net.col.mgmt'))}</th><th>${pcvEscapeHtml(t('vm.net.col.adapter'))}</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>`}
    </div>`;
}

// --- legacy render-vm-detail.ts ---
function vmPowerKind(vm) {
  const value = String(getVmState(vm) || '').toLowerCase();
  if (value.includes('running') || value === 'on') return 'running';
  if (value.includes('paused')) return 'paused';
  if (value.includes('saved')) return 'saved';
  return 'off';
}

function vmCheckpointCount(vm) {
  const raw = vm?.checkpoints?.count ?? vm?.checkpoints_count ?? (Array.isArray(vm?.checkpoints) ? vm.checkpoints.length : 0);
  const count = Number(raw);
  return Number.isFinite(count) ? count : 0;
}

function renderVmPowerButtons(vmId, kind, actionDisabled, lockedMutationDisabled) {
  const id = pcvEscapeHtml(vmId);
  if (kind === 'running') {
    return `<button type="button" data-action="vm-shutdown" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.shutdown'))}</button><button type="button" data-action="vm-restart" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.restart'))}</button>`;
  }
  if (kind === 'paused') {
    return `<button type="button" data-action="vm-resume" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.resume'))}</button>`;
  }
  if (kind === 'saved') {
    return `<button type="button" data-action="vm-resume-saved" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.resume_saved'))}</button>`;
  }
  return `<button type="button" data-action="vm-start" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.power.start'))}</button>`;
}

function clearNode(node) {
  if (!node) return;
  while (node.firstChild) node.removeChild(node.firstChild);
}

function sameVmToken(value, expected) {
  const left = String(value == null ? '' : value);
  const right = String(expected == null ? '' : expected);
  return left.length > 0 && right.length > 0 && left.toUpperCase() === right.toUpperCase();
}

function jobVmIdentityValues(job) {
  const params = job && job.params && typeof job.params === 'object' && !Array.isArray(job.params) ? job.params : null;
  if (!params) return [];
  return ['name', 'vm_name', 'source', 'new_name'].map((key) => params[key]).filter((value) => typeof value === 'string' || typeof value === 'number');
}

function collectVmJobs(vmId, vmName) {
  const seen = new Set();
  const rows = [];
  const pools = [];
  if (Array.isArray(state.serverJobs)) pools.push.apply(pools, state.serverJobs);
  if (Array.isArray(state.trackedJobs)) pools.push.apply(pools, state.trackedJobs);
  pools.forEach((job) => {
    if (!job || typeof job !== 'object') return;
    const hit = jobVmIdentityValues(job).some((value) => sameVmToken(value, vmId) || sameVmToken(value, vmName));
    if (!hit) return;
    const key = String(job.job_id || job.id || '');
    if (key) {
      if (seen.has(key)) return;
      seen.add(key);
    }
    rows.push(job);
  });
  return rows;
}

function fillVmJobs(container, vmId, recentOnly) {
  if (!container) return;
  clearNode(container);
  const jobs = collectVmJobs(vmId, getVmName(state.selectedVm));
  const list = recentOnly && jobs.length > 3 ? jobs.slice(jobs.length - 3) : jobs;
  if (!list.length) {
    if (PCV.components && typeof PCV.components.emptyState === 'function') {
      container.appendChild(PCV.components.emptyState({
        titleKey: 'vm.jobs.empty_title',
        bodyKey: 'vm.jobs.empty_body'
      }));
    } else {
      const title = document.createElement('p');
      title.textContent = t('vm.jobs.empty_title');
      const body = document.createElement('p');
      body.textContent = t('vm.jobs.empty_body');
      container.appendChild(title);
      container.appendChild(body);
    }
    return;
  }
  list.forEach((job) => {
    if (PCV.components && typeof PCV.components.renderJobProgress === 'function') {
      container.appendChild(PCV.components.renderJobProgress(job));
    }
  });
  if (recentOnly) {
    const more = document.createElement('button');
    more.type = 'button';
    more.setAttribute('data-action', 'vm-open-jobs-tab');
    more.textContent = t('vm.jobs.open_tab');
    container.appendChild(more);
  }
}

function nodeInside(ancestor, node) {
  let current = node;
  while (current) {
    if (current === ancestor) return true;
    current = current.parentNode;
  }
  return false;
}

function bindVmMoreMenu(root) {
  const toggle = root.querySelector('[data-action="vm-more-toggle"]');
  const menu = root.querySelector('[role="menu"]');
  if (!toggle || !menu) return;
  function enabledItems() {
    return Array.prototype.filter.call(menu.querySelectorAll('[role="menuitem"]'), (item) => !item.disabled);
  }
  function closeMenu() {
    menu.hidden = true;
    toggle.setAttribute('aria-expanded', 'false');
  }
  function openMenu() {
    menu.hidden = false;
    toggle.setAttribute('aria-expanded', 'true');
    const items = enabledItems();
    if (items[0] && typeof items[0].focus === 'function') items[0].focus();
  }
  toggle.addEventListener('click', (event) => {
    if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
    if (menu.hidden) openMenu();
    else closeMenu();
  });
  menu.addEventListener('keydown', (event) => {
    const items = enabledItems();
    if (event.key === 'Escape') {
      if (typeof event.preventDefault === 'function') event.preventDefault();
      closeMenu();
      if (typeof toggle.focus === 'function') toggle.focus();
      return;
    }
    if (!items.length) return;
    const current = items.indexOf(event.target);
    let next = -1;
    if (event.key === 'ArrowDown') next = current < 0 ? 0 : (current + 1) % items.length;
    else if (event.key === 'ArrowUp') next = current < 0 ? items.length - 1 : (current - 1 + items.length) % items.length;
    else if (event.key === 'Home') next = 0;
    else if (event.key === 'End') next = items.length - 1;
    else return;
    if (typeof event.preventDefault === 'function') event.preventDefault();
    if (items[next] && typeof items[next].focus === 'function') items[next].focus();
  });
  if (!window.__pcvVmMenuOutsideBound) {
    window.__pcvVmMenuOutsideBound = true;
    document.addEventListener('click', (event) => {
      const menus = document.querySelectorAll('.pcv-vm-menu');
      Array.prototype.forEach.call(menus, (openMenu) => {
        if (!openMenu || openMenu.hidden) return;
        const wrap = openMenu.parentNode;
        if (wrap && nodeInside(wrap, event.target)) return;
        openMenu.hidden = true;
        const opener = wrap && wrap.querySelector ? wrap.querySelector('[data-action="vm-more-toggle"]') : null;
        if (opener) opener.setAttribute('aria-expanded', 'false');
      });
    });
  }
}

function mountVmDetailTabs(root, vmId) {
  const host = root.querySelector('[data-pcv-vm-tabs]');
  if (!host || !PCV.components || typeof PCV.components.renderTabs !== 'function') return null;
  const tabs = [
    { id: 'summary', labelKey: 'vm.tab.summary' },
    { id: 'console', labelKey: 'vm.tab.console' },
    { id: 'checkpoints', labelKey: 'vm.tab.checkpoints' },
    { id: 'hardware', labelKey: 'vm.tab.hardware' },
    { id: 'guest', labelKey: 'vm.tab.guest' },
    { id: 'jobs', labelKey: 'vm.tab.jobs' }
  ];
  const api = PCV.components.renderTabs(host, tabs, window.__pcvVmDetailTab || '', (id) => {
    window.__pcvVmDetailTab = id;
  }, { rememberKey: 'pcv-vm-detail-tab' });
  const buttons = host.querySelectorAll('[role="tab"]');
  let active = tabs[0] ? tabs[0].id : '';
  Array.prototype.forEach.call(buttons, (button, index) => {
    if (button.getAttribute('aria-selected') === 'true' && tabs[index]) active = tabs[index].id;
  });
  window.__pcvVmDetailTab = active;
  tabs.forEach((tab, index) => {
    const source = root.querySelector(`[data-pcv-tab-source="${tab.id}"]`);
    if (source && api.panels[index]) api.panels[index].appendChild(source);
  });
  const jobsSource = root.querySelector('[data-pcv-tab-source="jobs"]');
  fillVmJobs(jobsSource || api.panels[5], vmId, false);
  const recent = root.querySelector('[data-pcv-recent-jobs]');
  if (recent) fillVmJobs(recent, vmId, true);
  root.querySelectorAll('[data-action="vm-open-jobs-tab"]').forEach((button) => {
    button.addEventListener('click', () => {
      window.__pcvVmDetailTab = 'jobs';
      api.select('jobs');
    });
  });
  const openConsole = root.querySelector('[data-action="vm-open-console-tab"]');
  if (openConsole) {
    openConsole.addEventListener('click', () => {
      if (openConsole.disabled) return;
      window.__pcvVmDetailTab = 'console';
      api.select('console');
    });
  }
  root.querySelectorAll('[data-action="vm-clone-open"]').forEach((button) => {
    button.addEventListener('click', (event) => {
      if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
      if (button.disabled) return;
      openVmCloneModal(button.getAttribute('data-vm-id') || vmId);
    });
  });
  root.querySelectorAll('[data-action="checkpoint-create-open"]').forEach((button) => {
    button.addEventListener('click', (event) => {
      if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
      if (button.disabled) return;
      openCheckpointCreateModal(button.getAttribute('data-vm-id') || vmId);
    });
  });
  fillCheckpointList(root, vmId);
  bindVmMoreMenu(root);
  if (typeof bindVmConsoleFrame === 'function') bindVmConsoleFrame();
  return api;
}

function currentVmNames() {
  const rows = Array.isArray(state.vms) ? state.vms : [];
  return rows.map((vm) => {
    try { return getVmName(vm); } catch (err) { return ''; }
  }).filter(Boolean);
}

function suggestCloneName(sourceName, names) {
  const taken = new Set((names || []).map((name) => String(name).toUpperCase()));
  const base = `${sourceName}-clone`;
  if (!taken.has(base.toUpperCase())) return base;
  let index = 2;
  let candidate = `${sourceName}-clone-${index}`;
  while (taken.has(candidate.toUpperCase())) {
    index += 1;
    candidate = `${sourceName}-clone-${index}`;
  }
  return candidate;
}

function cloneRequestPayload(vmId, name, vmRoot) {
  const payload = { confirm_name: vmId, name: String(name || '') };
  const root = String(vmRoot || '').trim();
  if (root) payload.vm_root = root;
  return payload;
}

function cloneFixText(code) {
  if (code === 'PCV_VM_CLONE_NAME_CONFLICT') return t('vm.clone.fix.conflict');
  if (code === 'PCV_VM_ALREADY_EXISTS') return t('vm.clone.fix.duplicate');
  if (code === 'PCV_VM_CLONE_NAME_REQUIRED') return t('vm.clone.fix.required');
  if (code === 'PCV_VM_NAME_INVALID' || code === 'PCV_VM_CLONE_NAME_INVALID') return t('vm.clone.fix.invalid');
  if (code === 'PCV_VM_CLONE_CONFIRMATION_MISMATCH') return t('vm.clone.fix.mismatch');
  return t('vm.clone.fix.generic');
}

function showClonePreview(panel, preview) {
  clearNode(panel);
  const bytes = preview && preview.planned_copy_bytes;
  const line = document.createElement('p');
  line.textContent = t('vm.clone.preview_ok', {
    bytes: bytes === null || bytes === undefined || bytes === '' ? '-' : String(bytes)
  });
  panel.appendChild(line);
}

function showClonePreviewError(panel, error) {
  clearNode(panel);
  const normalized = typeof normalizeError === 'function' ? normalizeError(error) : { code: '' };
  const line = document.createElement('p');
  line.textContent = t('vm.clone.preview_fail', {
    code: normalized.code || '',
    fix: cloneFixText(normalized.code)
  });
  panel.appendChild(line);
}

function cloneOpenLink(name) {
  const link = document.createElement('a');
  link.href = '#vms';
  link.textContent = t('vm.clone.open_new');
  link.addEventListener('click', (event) => {
    if (event && typeof event.preventDefault === 'function') event.preventDefault();
    openClonedVm(name);
  });
  return link;
}

function showCloneJob(host, job, name) {
  clearNode(host);
  if (!job || !PCV.components || typeof PCV.components.renderJobProgress !== 'function') return;
  host.appendChild(PCV.components.renderJobProgress(job));
  const phaseOf = (row) => String((row && (row.status || row.state)) || '').toLowerCase();
  if (phaseOf(job) === 'succeeded') host.appendChild(cloneOpenLink(name));
  const jobId = job.job_id || job.id;
  if (!jobId || typeof PCV.components.trackJob !== 'function') return;
  if (phaseOf(job) === 'succeeded' || phaseOf(job) === 'failed' || phaseOf(job) === 'canceled') return;
  PCV.components.trackJob(jobId, (next) => {
    clearNode(host);
    host.appendChild(PCV.components.renderJobProgress(next));
    if (phaseOf(next) === 'succeeded') host.appendChild(cloneOpenLink(name));
  });
}

async function openClonedVm(name) {
  const sameName = (vm) => String(getVmName(vm)).toUpperCase() === String(name || '').toUpperCase();
  const listed = (Array.isArray(state.vms) ? state.vms : []).find(sameName);
  state.selectedVmId = listed ? getVmId(listed) : name;
  if (typeof loadVms === 'function') {
    try { await loadVms(); } catch (err) { /* the open link still points at the chosen name */ }
  }
  const after = (Array.isArray(state.vms) ? state.vms : []).find(sameName);
  if (after) state.selectedVmId = getVmId(after);
  if (typeof refreshSelectedVm === 'function') await refreshSelectedVm();
  if (typeof render === 'function') render();
  if (PCV.modalCore && typeof PCV.modalCore.close === 'function') PCV.modalCore.close(true);
}

function openVmCloneModal(vmId) {
  const vm = (state.selectedVm && getVmId(state.selectedVm) === vmId) ? state.selectedVm : (findCachedVm(vmId) || state.selectedVm);
  const sourceName = getVmName(vm);
  const names = currentVmNames();
  const suggested = suggestCloneName(sourceName, names);
  const ux = PCV.uxlib;
  const components = PCV.components;
  const title = ux.el('h2', {}, t('vm.clone.title'));
  const subtitle = ux.el('p', { class: 'pcv-clone-subtitle' }, t('vm.clone.subtitle', {
    name: sourceName,
    lock: t(isTemplateLockedVm(vm) ? 'vm.clone.lock_on' : 'vm.clone.lock_off'),
    power: getVmState(vm) || t('vm.state.unknown')
  }));
  const nameInput = ux.el('input', {
    name: 'name',
    type: 'text',
    autocomplete: 'off',
    'aria-label': t('vm.clone.name_label')
  });
  nameInput.value = suggested;
  const form = ux.el('form', { class: 'pcv-clone-form' }, ux.el('label', {}, t('vm.clone.name_label'), nameInput));
  const vmRootInput = ux.el('input', {
    name: 'vm_root',
    type: 'text',
    autocomplete: 'off',
    'aria-label': t('vm.clone.vm_root')
  });
  const storage = ux.el('details', { class: 'pcv-clone-storage' },
    ux.el('summary', {}, t('vm.clone.storage_summary')),
    ux.el('label', {}, t('vm.clone.vm_root'), vmRootInput));
  const previewBtn = ux.el('button', { type: 'button', class: 'btn' }, t('vm.clone.preview'));
  const startBtn = ux.el('button', { type: 'button', class: 'btn' }, t('vm.clone.start'));
  const previewPanel = ux.el('div', { class: 'pcv-clone-preview', role: 'status' });
  const jobHost = ux.el('div', { class: 'pcv-clone-job' });
  let previewedName = '';
  function syncStart() {
    startBtn.disabled = !(previewedName && nameInput.value === previewedName);
  }
  startBtn.disabled = true;
  nameInput.addEventListener('input', () => {
    previewedName = '';
    syncStart();
  });
  components.bindValidation(form, {
    name: [
      components.validators.required,
      components.validators.hyperVName,
      components.validators.notSameAs(sourceName),
      components.validators.notIn(names)
    ]
  }, previewBtn);
  form.addEventListener('submit', (event) => {
    if (event && typeof event.preventDefault === 'function') event.preventDefault();
    if (!previewBtn.disabled && typeof previewBtn.click === 'function') previewBtn.click();
  });
  previewBtn.addEventListener('click', async () => {
    if (previewBtn.disabled) return;
    const name = nameInput.value;
    const payload = cloneRequestPayload(vmId, name, vmRootInput.value);
    try {
      const preview = await desktopApi.previewVmClone(vmId, payload);
      if (nameInput.value !== name) return;
      previewedName = name;
      syncStart();
      showClonePreview(previewPanel, preview);
    } catch (error) {
      previewedName = '';
      syncStart();
      showClonePreviewError(previewPanel, error);
    }
  });
  startBtn.addEventListener('click', async () => {
    if (startBtn.disabled || nameInput.value !== previewedName) return;
    const name = nameInput.value;
    startBtn.disabled = true;
    try {
      const job = await queueVmClone(vmId, name, vmRootInput.value);
      showCloneJob(jobHost, job, name);
    } catch (error) {
      showClonePreviewError(previewPanel, error);
      syncStart();
    }
  });
  const body = ux.el('div', { class: 'pcv-clone-modal' }, title, subtitle, form, storage, previewBtn, startBtn, previewPanel, jobHost);
  PCV.modalCore.open(body, { wide: true });
  return body;
}

function currentCheckpointNames() {
  return asArray(state.selectedVmCheckpoints).map((item) => {
    if (typeof getCheckpointName !== 'function') return '';
    try { return String(getCheckpointName(item) || ''); } catch (err) { return ''; }
  }).filter(Boolean);
}

function isCheckpointDisplayName(name) {
  if (name.length < 1 || name.length > 128 || name !== name.trim()) return false;
  for (let index = 0; index < name.length; index += 1) {
    const code = name.charCodeAt(index);
    const ch = name.charAt(index);
    if (code <= 0x1F || (code >= 0x7F && code <= 0x9F) || ch === '/' || ch === '\\') return false;
  }
  return true;
}

function checkpointNameIssue(value, names) {
  const name = value == null ? '' : String(value);
  if (name.trim() === '') return { code: 'PCV_CHECKPOINT_NAME_REQUIRED', messageKey: 'vm.checkpoint.name.required' };
  if (!isCheckpointDisplayName(name)) return { code: 'PCV_CHECKPOINT_NAME_INVALID', messageKey: 'vm.checkpoint.name.invalid' };
  const needle = name.toUpperCase();
  const list = Array.isArray(names) ? names : [];
  for (let index = 0; index < list.length; index += 1) {
    if (String(list[index]).toUpperCase() === needle) return { code: 'PCV_VM_ALREADY_EXISTS', messageKey: 'vm.checkpoint.name.duplicate' };
  }
  return null;
}

function checkpointNameRules(names) {
  return [(value) => checkpointNameIssue(value, names)];
}

function suggestCheckpointName(names, now) {
  const taken = new Set((names || []).map((name) => String(name).toUpperCase()));
  const date = now instanceof Date ? now : new Date();
  const pad = (part) => String(part).padStart(2, '0');
  const stamp = `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}-${pad(date.getHours())}${pad(date.getMinutes())}`;
  let candidate = `checkpoint-${stamp}`;
  if (!taken.has(candidate.toUpperCase())) return candidate;
  let index = 2;
  candidate = `checkpoint-${stamp}-${index}`;
  while (taken.has(candidate.toUpperCase())) {
    index += 1;
    candidate = `checkpoint-${stamp}-${index}`;
  }
  return candidate;
}

function sameSelectedVm(vmId) {
  return String(state.selectedVmId || '') === String(vmId || '');
}

function checkpointErrorDetail(error) {
  if (!error || typeof error !== 'object') return '';
  return String(error.message || error.detail || error.code || '');
}

async function refreshCheckpointSurface(vmId) {
  if (!sameSelectedVm(vmId)) return;
  try {
    if (typeof refreshSelectedVm === 'function') {
      await refreshSelectedVm();
    } else if (typeof desktopApi !== 'undefined' && desktopApi && typeof desktopApi.getVm === 'function') {
      const vm = await desktopApi.getVm(vmId);
      if (!sameSelectedVm(vmId)) return;
      state.selectedVm = vm;
    }
    if (!sameSelectedVm(vmId)) return;
    if (typeof desktopApi !== 'undefined' && desktopApi && typeof desktopApi.getVmCheckpoints === 'function') {
      const checkpoints = await desktopApi.getVmCheckpoints(vmId);
      if (!sameSelectedVm(vmId)) return;
      state.selectedVmCheckpoints = asArray(checkpoints);
      state.checkpointListError = null;
    } else if (sameSelectedVm(vmId)) {
      state.checkpointListError = null;
    }
  } catch (error) {
    if (!sameSelectedVm(vmId)) return;
    state.checkpointListError = typeof normalizeError === 'function' ? normalizeError(error) : error;
  }
  if (sameSelectedVm(vmId) && typeof render === 'function') render();
}

function watchCheckpointSurface(vmId, job) {
  const jobId = job && (job.job_id || job.id);
  const phase = String((job && (job.status || job.state)) || '').toLowerCase();
  if (typeof pcvTrackJob === 'function') pcvTrackJob(job);
  if (!jobId || phase === 'succeeded') {
    refreshCheckpointSurface(vmId);
    return;
  }
  if (phase === 'failed' || phase === 'canceled') return;
  if (PCV.components && typeof PCV.components.trackJob === 'function') {
    PCV.components.trackJob(jobId, (next) => {
      const nextPhase = String((next && (next.status || next.state)) || '').toLowerCase();
      if (nextPhase === 'succeeded') refreshCheckpointSurface(vmId);
    });
  }
}

function checkpointCreateBlocked(vmId) {
  const pending = typeof isCheckpointActionPending === 'function' && isCheckpointActionPending(vmId, 'create');
  const operate = typeof rbacAllows !== 'function' || rbacAllows('operate');
  const locked = typeof isTemplateLockedVm === 'function' && isTemplateLockedVm(state.selectedVm);
  return Boolean(pending || !operate || locked);
}

function checkpointLabel(checkpointId) {
  const rows = asArray(state.selectedVmCheckpoints);
  for (let index = 0; index < rows.length; index += 1) {
    if (typeof getCheckpointId === 'function' && getCheckpointId(rows[index]) === String(checkpointId || '')) {
      return typeof getCheckpointName === 'function' ? String(getCheckpointName(rows[index])) : String(checkpointId || '');
    }
  }
  return String(checkpointId || '');
}

function showCheckpointJob(host, job) {
  clearNode(host);
  if (!job || !PCV.components || typeof PCV.components.renderJobProgress !== 'function') return;
  host.appendChild(PCV.components.renderJobProgress(job));
  const phaseOf = (row) => String((row && (row.status || row.state)) || '').toLowerCase();
  const jobId = job.job_id || job.id;
  if (!jobId || typeof PCV.components.trackJob !== 'function') return;
  if (phaseOf(job) === 'succeeded' || phaseOf(job) === 'failed' || phaseOf(job) === 'canceled') return;
  PCV.components.trackJob(jobId, (next) => {
    clearNode(host);
    if (host.isConnected === false) return;
    host.appendChild(PCV.components.renderJobProgress(next));
  });
}

function openCheckpointCreateModal(vmId) {
  if (checkpointCreateBlocked(vmId)) return null;
  const names = currentCheckpointNames();
  const suggested = suggestCheckpointName(names);
  const ux = PCV.uxlib;
  const components = PCV.components;
  const title = ux.el('h2', {}, t('vm.checkpoint.create_title'));
  const nameInput = ux.el('input', {
    name: 'name',
    type: 'text',
    autocomplete: 'off',
    'aria-label': t('vm.checkpoint.name_label')
  });
  nameInput.value = suggested;
  const form = ux.el('form', { class: 'pcv-checkpoint-create-form' }, ux.el('label', {}, t('vm.checkpoint.name_label'), nameInput));
  const startBtn = ux.el('button', { type: 'button', class: 'btn' }, t('vm.checkpoint.create'));
  const jobHost = ux.el('div', { class: 'pcv-checkpoint-job' });
  components.bindValidation(form, { name: checkpointNameRules(names) }, startBtn);
  form.addEventListener('submit', (event) => {
    if (event && typeof event.preventDefault === 'function') event.preventDefault();
    if (!startBtn.disabled && typeof startBtn.click === 'function') startBtn.click();
  });
  startBtn.addEventListener('click', async () => {
    if (startBtn.disabled) return;
    const name = nameInput.value;
    startBtn.disabled = true;
    try {
      const job = await queueCheckpointCreate(vmId, name);
      showCheckpointJob(jobHost, job);
    } catch (error) {
      startBtn.disabled = false;
      const normalized = typeof normalizeError === 'function' ? normalizeError(error) : null;
      const line = document.createElement('p');
      line.textContent = normalized ? String(normalized.message || normalized.detail || normalized.code || '') : '';
      clearNode(jobHost);
      jobHost.appendChild(line);
    }
  });
  const body = ux.el('div', { class: 'pcv-checkpoint-create-modal' }, title, form, startBtn, jobHost);
  PCV.modalCore.open(body);
  return body;
}

function fillCheckpointList(root, vmId) {
  const host = root && typeof root.querySelector === 'function' ? root.querySelector('[data-pcv-checkpoint-list]') : null;
  if (!host || !PCV.components) return;
  if (state.checkpointListError) {
    clearNode(host);
    host.appendChild(PCV.components.loadError({
      titleKey: 'vm.checkpoint.load_error',
      detail: checkpointErrorDetail(state.checkpointListError),
      onRetry: () => { refreshCheckpointSurface(vmId); }
    }));
    return;
  }
  if (asArray(state.selectedVmCheckpoints).length === 0) {
    clearNode(host);
    host.appendChild(PCV.components.emptyState({
      titleKey: 'vm.checkpoint.empty.title',
      bodyKey: 'vm.checkpoint.empty.body',
      actionLabelKey: 'vm.checkpoint.empty.action',
      onAction: () => { openCheckpointCreateModal(vmId); }
    }));
  }
}

function renderVmDetail() {
  const vm = state.selectedVm;
  if (!vm) {
    els.vmDetailTitle.textContent = t('vm.detail.none_title');
    state.selectedVmCheckpoints = [];
    state.selectedVmReadbacks = null;
    state.selectedVmQosControl = null;
    els.vmDetailContent.innerHTML = `<p class="muted">${pcvEscapeHtml(t('vm.detail.none_body'))}</p>`;
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
    ? (vm.dvd_media.length > 0 ? flattenNamedList(vm.dvd_media, ['path']) : t('vm.detail.none'))
    : t('vm.detail.not_reported');
  const details = [
    [t('vm.detail.label.state'), vm.state || vm.status],
    [t('vm.detail.label.id'), vm.id],
    [t('vm.detail.label.cpu'), vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count],
    [t('vm.detail.label.memory_startup'), vm.memory?.startup_mb ?? vm.memory_mb],
    [t('vm.detail.label.memory_assigned'), vm.memory?.assigned_mb ?? vm.memory_assigned_mb],
    [t('vm.detail.label.generation'), vm.generation],
    [t('vm.detail.label.storage'), storage],
    [t('vm.detail.label.dvd'), dvdMedia],
    [t('vm.detail.label.network'), network],
    [t('vm.detail.label.checkpoints'), vm.checkpoints?.count ?? vm.checkpoints_count],
    [t('vm.detail.label.console'), formatConsoleValue(vm.console)],
    [t('vm.detail.label.managed'), vm.managed_by_purecvisor],
    [t('vm.detail.label.template'), templateLocked ? t('vm.detail.locked') : t('vm.detail.unlocked')],
    [t('vm.detail.label.notes'), vm.error?.message || vm.notes]
  ];
  const vmName = getVmName(vm);
  const deleteDisabled = isVmActionPending(vmId) || !canOperate || templateLocked ? ' disabled' : '';
  const id = pcvEscapeHtml(vmId);
  const templateMenuItem = templateLocked
    ? `<button type="button" role="menuitem" data-action="vm-template-unlock" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.unlock'))}</button>`
    : `<button type="button" role="menuitem" data-action="vm-template-lock" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.lock'))}</button>`;
  const templateCloneButton = templateLocked
    ? `<button type="button" class="btn" data-action="vm-clone-open" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.clone'))}</button>`
    : '';
  const stateText = getVmState(vm) || t('vm.state.unknown');

  els.vmDetailContent.innerHTML = `
    <header class="pcv-vm-header">
      <nav class="pcv-vm-crumb" aria-label="${pcvEscapeHtml(t('vm.crumb.label'))}">
        <span>${pcvEscapeHtml(t('vm.crumb.vms'))}</span>
        <span aria-hidden="true"> › </span>
        <span>${pcvEscapeHtml(vmName)}</span>
      </nav>
      <div class="pcv-vm-title-row">
        <h2 class="pcv-vm-name">${pcvEscapeHtml(vmName)}</h2>
        <div class="pcv-vm-badges">${stateBadge(stateText)}${templateLocked ? `<span class="badge badge-warn">${pcvEscapeHtml(t('vm.badge.template'))}</span>` : ''}</div>
      </div>
      <div class="pcv-vm-actions">
        ${renderVmPowerButtons(vmId, vmPowerKind(vm), actionDisabled, lockedMutationDisabled)}
        <button type="button" data-action="vm-open-console-tab" data-vm-id="${id}"${consoleDisabled}>${pcvEscapeHtml(t('vm.console.open'))}</button>
        ${templateCloneButton}
        <div class="pcv-vm-menu-wrap">
          <button type="button" data-action="vm-more-toggle" aria-haspopup="menu" aria-expanded="false" aria-controls="pcv-vm-more-menu">${pcvEscapeHtml(t('vm.menu.more'))}</button>
          <div id="pcv-vm-more-menu" class="pcv-vm-menu" role="menu" hidden>
            <button type="button" role="menuitem" data-action="vm-clone-open" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.clone'))}</button>
            ${templateMenuItem}
            <button type="button" role="menuitem" data-action="vm-export-focus" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.menu.export'))}</button>
            <button type="button" role="menuitem" data-action="vm-console" data-vm-id="${id}"${consoleDisabled}>${pcvEscapeHtml(t('vm.menu.vmconnect'))}</button>
            <button type="button" role="menuitem" class="pcv-menu-danger" data-action="vm-poweroff" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.menu.poweroff'))}</button>
            <button type="button" role="menuitem" class="pcv-menu-danger" data-action="vm-delete" data-vm-id="${id}"${deleteDisabled}>${pcvEscapeHtml(t('vm.menu.delete'))}</button>
          </div>
        </div>
      </div>
      ${pendingVmAction ? `<p class="muted">${pcvEscapeHtml(t('vm.pending', { action: String(pendingVmAction) }))}</p>` : ''}
      ${templateLocked ? `<p class="muted">${pcvEscapeHtml(t('vm.lock.note'))}</p>` : ''}
      ${!canOperate ? `<p class="muted">${pcvEscapeHtml(t('vm.rbac.operate'))}</p>` : ''}
    </header>
    <div data-pcv-vm-tabs></div>
    <div data-pcv-tab-source="summary">
      <div class="details-grid detail-grid">
        ${details.map(([label, value]) => `<div class="kv"><span>${pcvEscapeHtml(label)}</span><strong>${pcvEscapeHtml(formatObjectValue(value))}</strong></div>`).join('')}
      </div>
      ${renderVmQosGuestReadback(vmId)}
      <div class="pcv-vm-recent">
        <h3>${pcvEscapeHtml(t('vm.jobs.recent'))}</h3>
        <div data-pcv-recent-jobs></div>
      </div>
      ${renderExportImportReadback(vm, vmId, actionDisabled)}
      <div class="lifecycle-actions pcv-vm-other-actions">
        <h3>${pcvEscapeHtml(t('vm.summary.other'))}</h3>
        <button type="button" data-action="vm-save" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.save'))}</button>
        <button type="button" data-action="vm-pause" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.pause'))}</button>
        <button type="button" data-action="vm-eject" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.eject'))}</button>
        <button type="button" data-action="vm-manage" data-vm-id="${id}"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.power.manage'))}</button>
        <button type="button" data-action="vm-delete-status" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.power.delete_status'))}</button>
      </div>
    </div>
    <div data-pcv-tab-source="console">
      ${renderVmConsoleFrameCard(vm, vmId, canViewConsole)}
    </div>
    <div data-pcv-tab-source="checkpoints">
      <div class="checkpoint-panel">
        <div class="mini-section-header">
          <div>
            <p class="eyebrow">${pcvEscapeHtml(t('vm.checkpoint.eyebrow'))}</p>
            <h3>${pcvEscapeHtml(t('vm.checkpoint.title'))}</h3>
          </div>
          <button type="button" data-action="checkpoint-refresh" data-vm-id="${id}"${checkpointRefreshDisabled}>${pcvEscapeHtml(t('vm.checkpoint.refresh'))}</button>
          <button type="button" data-action="checkpoint-create-open" data-vm-id="${id}"${checkpointMutationDisabled}>${pcvEscapeHtml(t('vm.checkpoint.create'))}</button>
        </div>
        ${renderCheckpointScheduleReadback(vm, vmId, actionDisabled)}
        <div class="checkpoint-list" data-pcv-checkpoint-list>${renderCheckpointList(vmId)}</div>
      </div>
    </div>
    <div data-pcv-tab-source="hardware">
      <div class="vm-resource-grid">
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.iso'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.iso_help'))}</p>
          <form class="vm-resource-form" data-action="vm-attach" data-vm-id="${id}">
            <input name="iso_path" type="text" placeholder="${pcvEscapeHtml(t('vm.hw.iso_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.iso'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.iso_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.rename'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.rename_help'))}</p>
          <form class="vm-resource-form" data-action="vm-rename" data-vm-id="${id}">
            <input name="new_name" type="text" placeholder="${pcvEscapeHtml(t('vm.hw.rename_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.rename'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.rename_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.switch'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.switch_help'))}</p>
          <form class="vm-resource-form" data-action="vm-network-connect" data-vm-id="${id}">
            <select name="switch" aria-label="${pcvEscapeHtml(t('vm.hw.switch'))}"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.switch_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.device'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.device_help'))}</p>
          <form class="vm-resource-form" data-action="vm-device-add" data-vm-id="${id}">
            <select name="device" aria-label="${pcvEscapeHtml(t('vm.hw.device'))}"${lockedMutationDisabled}><option value="nic">${pcvEscapeHtml(t('vm.hw.device_nic'))}</option><option value="dvd">${pcvEscapeHtml(t('vm.hw.device_dvd'))}</option></select>
            <select name="switch" aria-label="${pcvEscapeHtml(t('vm.hw.switch'))}"${lockedMutationDisabled}>${renderSwitchOptions()}</select>
            <input name="iso_path" type="text" placeholder="${pcvEscapeHtml(t('vm.hw.dvd_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.dvd_placeholder'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.device_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.memory'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.memory_help'))}</p>
          <form class="vm-resource-form" data-action="vm-set-memory" data-vm-id="${id}">
            <input name="memory_mb" type="number" min="512" max="262144" step="128" placeholder="${pcvEscapeHtml(t('vm.hw.memory_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.memory'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.memory_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.vcpu'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.vcpu_help'))}</p>
          <form class="vm-resource-form" data-action="vm-set-vcpu" data-vm-id="${id}">
            <input name="cpu" type="number" min="1" max="32" step="1" placeholder="${pcvEscapeHtml(t('vm.hw.vcpu_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.vcpu'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.vcpu_apply'))}</button>
          </form>
        </div>
        <div class="pcv-hw-cell">
          <p class="pcv-hw-label">${pcvEscapeHtml(t('vm.hw.disk'))}</p>
          <p class="pcv-hw-help">${pcvEscapeHtml(t('vm.hw.disk_help'))}</p>
          <form class="vm-resource-form" data-action="vm-disk-resize" data-vm-id="${id}">
            <input name="disk_gb" type="number" min="8" max="4096" step="1" placeholder="${pcvEscapeHtml(t('vm.hw.disk_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.hw.disk'))}"${lockedMutationDisabled}>
            <button type="submit"${lockedMutationDisabled}>${pcvEscapeHtml(t('vm.hw.disk_apply'))}</button>
          </form>
        </div>
      </div>
    </div>
    <div data-pcv-tab-source="guest">
      ${renderVmQosDirectControl(vmId)}
    </div>
    <div data-pcv-tab-source="jobs"></div>`;
  mountVmDetailTabs(els.vmDetailContent, vmId);
}

function renderExportImportReadback(vm, vmId, actionDisabled = '') {
  const managed = vm?.managed_by_purecvisor === true;
  const generation = Number(vm?.generation);
  const power = String(vm?.state || vm?.status || '').trim().toLowerCase();
  const off = power === 'off' || power === 'stopped';
  const securityPresent = vm?.security_features_present === true;
  const exportEligible = managed && generation === 2 && off && !securityPresent;
  const securityLabel = securityPresent ? t('vm.export.security_present') : t('vm.export.security_absent');
  return `<div class="export-import-readback">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">${pcvEscapeHtml(t('vm.export.eyebrow'))}</p>
          <h3>${pcvEscapeHtml(t('vm.export.title'))}</h3>
        </div>
      </div>
      <div class="diagnostics-grid">
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.export'))}</span><strong>${pcvEscapeHtml(exportEligible ? t('vm.export.eligible') : t('vm.export.blocked'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.managed'))}</span><strong>${pcvEscapeHtml(managed ? t('vm.export.yes') : t('vm.export.no'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.generation'))}</span><strong>${pcvEscapeHtml(formatObjectValue(vm?.generation))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.power'))}</span><strong>${pcvEscapeHtml(formatObjectValue(vm?.state || vm?.status))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.security'))}</span><strong>${pcvEscapeHtml(securityLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.export.import'))}</span><strong>${pcvEscapeHtml(t('vm.export.import_value'))}</strong></div>
      </div>
      <form class="vm-resource-form export-import-form" data-action="vm-export" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="directory" type="text" placeholder="${pcvEscapeHtml(t('vm.export.dir_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.export.dir_placeholder'))}"${actionDisabled}>
        <button type="submit" data-action="vm-export-preview"${actionDisabled}>${pcvEscapeHtml(t('vm.export.preview'))}</button>
        <button type="submit" data-action="vm-export-apply"${actionDisabled}>${pcvEscapeHtml(t('vm.export.apply'))}</button>
      </form>
      <form class="vm-resource-form export-import-form" data-action="vm-import" data-vm-id="${pcvEscapeHtml(vmId)}">
        <input name="name" type="text" placeholder="${pcvEscapeHtml(t('vm.export.name_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.export.import_name'))}"${actionDisabled}>
        <input name="directory" type="text" placeholder="${pcvEscapeHtml(t('vm.export.package_placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.export.package_dir'))}"${actionDisabled}>
        <label><input name="has_vmcx" type="checkbox"${actionDisabled}> ${pcvEscapeHtml(t('vm.export.has_vmcx'))}</label>
        <button type="submit" data-action="vm-import-preview"${actionDisabled}>${pcvEscapeHtml(t('vm.export.preview_import'))}</button>
        <button type="submit" data-action="vm-import-apply"${actionDisabled}>${pcvEscapeHtml(t('vm.export.apply_import'))}</button>
      </form>
      ${renderExportImportPreview(vmId)}
      <div class="boundary-chip-row">
        <span>${pcvEscapeHtml(t('vm.export.chip_preview'))}</span>
        <span>${pcvEscapeHtml(t('vm.export.chip_identity'))}</span>
        <span>${pcvEscapeHtml(t('vm.export.chip_ovf'))}</span>
        <span>${pcvEscapeHtml(t('vm.export.chip_tpm'))}</span>
      </div>
    </div>`;
}

function checkpointScheduleStatusText(status) {
  const folded = String(status || '').toLowerCase();
  if (folded === 'disabled') return t('vm.checkpoint.schedule.off');
  if (folded === 'waiting') return t('vm.checkpoint.schedule.status.waiting');
  if (folded === 'due') return t('vm.checkpoint.schedule.status.due');
  return formatObjectValue(status);
}

function renderCheckpointScheduleReadback(vm, vmId, actionDisabled = '') {
  const schedule = asObject(vm?.checkpoint_schedule);
  const enabled = schedule.enabled === true;
  const status = schedule.status || (enabled ? 'waiting' : 'disabled');
  const count = Number(vm?.checkpoints?.count ?? vm?.checkpoints_count ?? 0);
  const retention = schedule.retention_max;
  const capacityBlocked = enabled && retention != null && Number.isFinite(Number(retention)) && count >= Number(retention);
  const statusLabel = capacityBlocked ? t('vm.checkpoint.schedule.status.blocked') : checkpointScheduleStatusText(status);
  const id = pcvEscapeHtml(vmId);
  const draft = state.checkpointScheduleDraft && String(state.checkpointScheduleDraft.vm_id) === String(vmId)
    ? state.checkpointScheduleDraft
    : null;
  const intervalValue = draft && draft.interval_minutes != null ? String(draft.interval_minutes) : '';
  const retentionValue = draft && draft.retention_max != null ? String(draft.retention_max) : '';
  return `<div class="checkpoint-schedule-readback">
      <div class="diagnostics-grid">
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.enabled'))}</span><strong>${pcvEscapeHtml(enabled ? t('vm.checkpoint.schedule.on') : t('vm.checkpoint.schedule.off'))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.status'))}</span><strong>${pcvEscapeHtml(statusLabel)}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.interval'))}</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.interval_minutes))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.retention'))}</span><strong>${pcvEscapeHtml(formatObjectValue(retention))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.last'))}</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.last_enqueued_at))}</strong></div>
        <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.next'))}</span><strong>${pcvEscapeHtml(formatObjectValue(schedule.next_due_at))}</strong></div>
      </div>
      <form class="vm-resource-form checkpoint-schedule-form" data-action="checkpoint-schedule" data-vm-id="${id}">
        <input name="interval_minutes" type="number" min="60" max="10080" step="1" value="${pcvEscapeHtml(intervalValue)}" placeholder="${pcvEscapeHtml(t('vm.checkpoint.schedule.help_interval'))}" aria-label="${pcvEscapeHtml(t('vm.checkpoint.schedule.interval'))}"${actionDisabled}>
        <input name="retention_max" type="number" min="1" max="32" step="1" value="${pcvEscapeHtml(retentionValue)}" placeholder="${pcvEscapeHtml(t('vm.checkpoint.schedule.help_retention'))}" aria-label="${pcvEscapeHtml(t('vm.checkpoint.schedule.retention'))}"${actionDisabled}>
        <button type="submit" data-action="checkpoint-schedule-preview"${actionDisabled}>${pcvEscapeHtml(t('vm.checkpoint.schedule.preview'))}</button>
        <button type="submit" data-action="checkpoint-schedule-set"${actionDisabled}>${pcvEscapeHtml(t('vm.checkpoint.schedule.save'))}</button>
        <button type="button" class="danger-button" data-action="checkpoint-schedule-clear" data-vm-id="${id}"${actionDisabled}>${pcvEscapeHtml(t('vm.checkpoint.schedule.clear'))}</button>
      </form>
      ${renderCheckpointSchedulePreview(vmId)}
      <div class="boundary-chip-row">
        <span>${pcvEscapeHtml(t('vm.checkpoint.schedule.chip_preview'))}</span>
        <span>${pcvEscapeHtml(t('vm.checkpoint.schedule.chip_retention'))}</span>
      </div>
    </div>`;
}

function renderVmWorkbenchContext() {
  const vm = state.selectedVm;
  if (!vm) {
    els.vmWorkbenchContext.innerHTML = `<p class="muted">${pcvEscapeHtml(t('vm.workbench.empty'))}</p>`;
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
    ? `<span>${pcvEscapeHtml(activity.operation || t('vm.workbench.job'))}</span>${stateBadge(activity.status)}`
    : `<span class="muted">${pcvEscapeHtml(t('vm.workbench.none'))}</span>`;

  els.vmWorkbenchContext.innerHTML = `
    <div class="vm-context-card">
      <div>
        <span class="muted">${pcvEscapeHtml(t('vm.workbench.selected'))}</span>
        <strong>${pcvEscapeHtml(vmName)}</strong>
      </div>
      <div>${stateBadge(vm.state || vm.status)}</div>
      <div class="vm-context-activity">
        <span class="muted">${pcvEscapeHtml(t('vm.workbench.activity'))}</span>
        ${activityHtml}
      </div>
    </div>`;
}

// --- legacy render-qos.ts ---
function renderCheckpointList(vmId) {
  if (state.checkpointListError) return '';
  const checkpoints = asArray(state.selectedVmCheckpoints);
  if (checkpoints.length === 0) return '';
  const canOperate = rbacAllows('operate');
  const templateLocked = isTemplateLockedVm(state.selectedVm);
  const rows = checkpoints.map((checkpoint) => {
    const checkpointId = getCheckpointId(checkpoint);
    const checkpointDisabled = isCheckpointActionPending(vmId, checkpointId) || !canOperate || templateLocked ? ' disabled' : '';
    const id = pcvEscapeHtml(vmId);
    const cp = pcvEscapeHtml(checkpointId);
    const created = checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created;
    return `<tr>
      <td>${pcvEscapeHtml(getCheckpointName(checkpoint))}</td>
      <td>${pcvEscapeHtml(formatObjectValue(created))}</td>
      <td class="pcv-checkpoint-actions">
        <button type="button" data-action="checkpoint-restore" data-vm-id="${id}" data-checkpoint-id="${cp}" title="${checkpointRestoreBlocked(state.selectedVm) ? pcvEscapeHtml(t('vm.checkpoint.restore.poweroff')) : ''}"${checkpointDisabled}>${pcvEscapeHtml(t('vm.checkpoint.restore'))}</button>
        <button type="button" class="danger-button" data-action="checkpoint-delete" data-vm-id="${id}" data-checkpoint-id="${cp}"${checkpointDisabled}>${pcvEscapeHtml(t('vm.checkpoint.delete'))}</button>
      </td>
    </tr>`;
  }).join('');
  return `<table class="pcv-checkpoint-table">
      <thead><tr>
        <th>${pcvEscapeHtml(t('vm.checkpoint.col.name'))}</th>
        <th>${pcvEscapeHtml(t('vm.checkpoint.col.created'))}</th>
        <th>${pcvEscapeHtml(t('vm.checkpoint.col.actions'))}</th>
      </tr></thead>
      <tbody>${rows}</tbody>
    </table>`;
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
  return parts.length > 0 ? parts.join(' / ') : t('vm.readback.payload');
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
        ? t('vm.readback.loading')
        : t('vm.readback.not_loaded');
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
  const loadingLabel = readbacks?.loading ? t('vm.readback.loading') : t('vm.readback.refresh');
  return `<section class="qos-readback-panel">
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">${pcvEscapeHtml(t('vm.readback.eyebrow'))}</p>
        <h3>${pcvEscapeHtml(t('vm.readback.title'))}</h3>
      </div>
      <button data-action="vm-qos-guest-refresh" data-vm-id="${pcvEscapeHtml(vmId)}"${readbacks?.loading ? ' disabled' : ''}>${pcvEscapeHtml(loadingLabel)}</button>
    </div>
    <div class="qos-readback-grid">
      ${renderReadbackCard(readbacks, 'blkio', t('vm.readback.blkio'), 'storage_qos', ['linux_blkio_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'bandwidth', t('vm.readback.bandwidth'), 'network_qos', ['linux_bandwidth_compatible', 'mutation_supported'])}
      ${renderReadbackCard(readbacks, 'guest_agent', t('vm.readback.guest_agent'), 'guest_agent', ['status', 'qemu_guest_agent', 'guest_exec_supported'])}
      ${renderReadbackCard(readbacks, 'guest_ping', t('vm.readback.guest_ping'), 'guest_ping', ['reachable', 'guest_heartbeat_verified'])}
      ${renderReadbackCard(readbacks, 'memory_stats', t('vm.readback.memory'), 'memory', ['startup_mb', 'assigned_mb', 'dynamic'])}
      ${renderReadbackCard(readbacks, 'cpu_stats', t('vm.readback.cpu'), 'cpu', ['count'])}
    </div>
    <p class="muted">${pcvEscapeHtml(t('vm.readback.note', { time: updated }))}</p>
  </section>`;
}

function getSelectedVmQosControl(vmId) {
  const control = state.selectedVmQosControl;
  return control && control.vm_id === vmId ? control : null;
}

function renderQosControlResult(control) {
  if (!control) {
    return `<p class="muted">${pcvEscapeHtml(t('vm.qosui.empty'))}</p>`;
  }

  const status = control.error ? 'degraded' : control.loading ? 'loading' : control.mode || 'tracked';
  const detail = control.error
    ? `${control.error.code}: ${control.error.message}`
    : control.result
      ? formatObjectValue(control.result)
      : t('vm.qosui.pending');
  const updated = control.updated_at ? new Date(control.updated_at).toLocaleString() : '-';
  return `<div class="qos-control-result">
    <span class="muted">${pcvEscapeHtml(control.kind || 'qos')} ${pcvEscapeHtml(control.mode || 'control')}</span>
    ${stateBadge(status)}
    <strong>${pcvEscapeHtml(detail)}</strong>
    <p class="muted">${pcvEscapeHtml(t('vm.qosui.updated', { time: updated }))}</p>
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
        <p class="eyebrow">${pcvEscapeHtml(t('vm.qosui.eyebrow'))}</p>
        <h3>${pcvEscapeHtml(t('vm.qosui.title'))}</h3>
      </div>
      <span class="status-badge ${canOperate ? 'ok' : 'warn'}">${pcvEscapeHtml(canOperate ? t('vm.qosui.operate') : t('vm.qosui.operate_required'))}</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-qos-kind="storage" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>${pcvEscapeHtml(t('vm.qosui.disk'))}<input name="disk" autocomplete="off" value="disk0"${actionDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.qosui.max_iops'))}<input name="maximum_iops" type="number" min="0" step="1" value="120"${actionDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.qosui.min_iops'))}<input name="minimum_iops" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-storage-preview"${actionDisabled}>${pcvEscapeHtml(t('vm.qosui.preview'))}</button>
          <button type="submit" class="danger-button" data-action="vm-qos-storage-apply"${actionDisabled}>${pcvEscapeHtml(t('vm.qosui.apply'))}</button>
        </div>
      </form>
      <form class="qos-control-form" data-qos-kind="network" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>${pcvEscapeHtml(t('vm.qosui.adapter'))}<input name="adapter" autocomplete="off" value="adapter0"${actionDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.qosui.max_kbps'))}<input name="maximum_kbps" type="number" min="0" step="1" value="20480"${actionDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.qosui.min_kbps'))}<input name="minimum_kbps" type="number" min="0" step="1" value="0"${actionDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-qos-network-preview"${actionDisabled}>${pcvEscapeHtml(t('vm.qosui.preview'))}</button>
          <button type="submit" class="danger-button" data-action="vm-qos-network-apply"${actionDisabled}>${pcvEscapeHtml(t('vm.qosui.apply'))}</button>
        </div>
      </form>
    </div>
    ${renderQosControlResult(control)}
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">${pcvEscapeHtml(t('vm.guestui.eyebrow'))}</p>
        <h3>${pcvEscapeHtml(t('vm.guestui.title'))}</h3>
      </div>
      <span class="status-badge ${canGuestExec && canGuestChannel ? 'ok' : 'warn'}">${pcvEscapeHtml(t('vm.guestui.badge'))}</span>
    </div>
    <div class="qos-control-grid">
      <form class="qos-control-form" data-guest-execution-kind="exec" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>${pcvEscapeHtml(t('vm.guestui.cred'))}<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestExecDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.timeout'))}<input name="timeout_sec" type="number" min="1" max="600" step="1" value="60"${guestExecDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.command'))}<input name="command" autocomplete="off" placeholder="hostname"${guestExecDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="vm-guest-exec-preview"${guestExecDisabled}>${pcvEscapeHtml(t('vm.guestui.preview_exec'))}</button>
          <button type="submit" class="danger-button" data-action="vm-guest-exec"${guestExecDisabled}>${pcvEscapeHtml(t('vm.guestui.queue_exec'))}</button>
        </div>
      </form>
      <form class="qos-control-form" data-guest-execution-kind="channel" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>${pcvEscapeHtml(t('vm.guestui.cred'))}<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestChannelDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.timeout'))}<input name="timeout_sec" type="number" min="1" max="600" step="1" value="30"${guestChannelDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" data-action="guest-agent-channel-preview"${guestChannelDisabled}>${pcvEscapeHtml(t('vm.guestui.preview_channel'))}</button>
          <button type="submit" data-action="guest-agent-ensure-channel" data-guest-channel-mode="verify"${guestChannelDisabled}>${pcvEscapeHtml(t('vm.guestui.verify'))}</button>
          <button type="submit" class="danger-button" data-action="guest-agent-ensure-channel" data-guest-channel-mode="repair"${guestChannelDisabled}>${pcvEscapeHtml(t('vm.guestui.repair'))}</button>
        </div>
      </form>
      <form class="qos-control-form" data-action="vm-guest-file" data-vm-id="${pcvEscapeHtml(vmId)}">
        <label>${pcvEscapeHtml(t('vm.guestui.host'))}<input name="host_path" autocomplete="off" placeholder="C:\\ProgramData\\PureCVisor\\desktop-node\\guest-files\\payload.iso"${guestExecDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.guest'))}<input name="guest_path" autocomplete="off" placeholder="C:\\Users\\Public\\PureCVisor\\payload.iso"${guestExecDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.cred'))}<input name="credential_ref" autocomplete="off" placeholder="wincred:target"${guestExecDisabled}></label>
        <label>${pcvEscapeHtml(t('vm.guestui.timeout'))}<input name="timeout_sec" type="number" min="1" max="600" step="1" value="60"${guestExecDisabled}></label>
        <div class="qos-control-actions">
          <button type="submit" class="danger-button" data-action="vm-guest-file"${guestExecDisabled}>${pcvEscapeHtml(t('vm.guestui.copy'))}</button>
        </div>
      </form>
    </div>
    <p class="muted">${pcvEscapeHtml(t('vm.guestui.note_output'))}</p>
    <p class="muted">${pcvEscapeHtml(t('vm.guestui.note_copy'))}</p>
    <p class="muted">${pcvEscapeHtml(t('vm.guestui.note_deferred'))}</p>
  </section>`;
}

// --- legacy mutate.ts ---
function vmActionName(action) {
  if (action === 'start') return t('vm.action.start');
  if (action === 'shutdown') return t('vm.action.shutdown');
  if (action === 'poweroff') return t('vm.action.poweroff');
  if (action === 'restart') return t('vm.action.restart');
  if (action === 'save') return t('vm.action.save');
  if (action === 'resume-saved') return t('vm.action.resume_saved');
  if (action === 'pause') return t('vm.action.pause');
  if (action === 'resume') return t('vm.action.resume');
  if (action === 'eject') return t('vm.action.eject');
  if (action === 'set-memory') return t('vm.action.set_memory');
  if (action === 'set-vcpu') return t('vm.action.set_vcpu');
  if (action === 'disk-resize') return t('vm.action.disk_resize');
  return String(action || '');
}

function vmModeName(mode) {
  if (mode === 'apply') return t('vm.mode.apply');
  if (mode === 'preview') return t('vm.mode.preview');
  if (mode === 'exec') return t('vm.mode.exec');
  if (mode === 'repair') return t('vm.mode.repair');
  if (mode === 'verify') return t('vm.mode.verify');
  if (mode === 'clear') return t('vm.mode.clear');
  if (mode === 'set') return t('vm.mode.set');
  return String(mode || '');
}

async function askVmConfirm(opts) {
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return false;
  return PCV.components.confirmDialog({
    title: opts.title,
    message: opts.message,
    confirmLabel: opts.confirmLabel,
    cancelLabel: t('ui.confirm.cancel'),
    danger: opts.danger === true
  });
}

async function queueVmLifecycle(vmId, action) {
  requireRbac('operate', vmActionName(action));
  if (action === 'poweroff' || action === 'restart' || action === 'save' || action === 'resume-saved') {
    const vm = state.selectedVm || findCachedVm(vmId);
    const name = getVmName(vm);
    const vmState = getVmState(vm) || t('vm.state.unknown');
    let title = t('vm.restart.title');
    let message = t('vm.restart.message', { name, state: vmState });
    let confirmLabel = t('vm.restart.confirm');
    let danger = false;
    if (action === 'poweroff') {
      title = t('vm.poweroff.title');
      message = t('vm.poweroff.message', { name, state: vmState });
      confirmLabel = t('vm.poweroff.confirm');
      danger = true;
    } else if (action === 'save') {
      title = t('vm.save.title');
      message = t('vm.save.message', { name, state: vmState });
      confirmLabel = t('vm.save.confirm');
    } else if (action === 'resume-saved') {
      title = t('vm.resume_saved.title');
      message = t('vm.resume_saved.message', { name, state: vmState });
      confirmLabel = t('vm.resume_saved.confirm');
    }
    const accepted = await PCV.components.confirmDialog({
      title,
      message,
      confirmLabel,
      cancelLabel: t('ui.confirm.cancel'),
      danger
    });
    if (!accepted) return;
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
  requireRbac('operate', t('vm.rbac.attach'));
  const path = String(isoPath || '').trim();
  if (!path) {
    throw normalizeError({
      code: 'PCV_VM_ATTACH_ISO_REQUIRED',
      message: t('vm.err.iso_required'),
      detail: t('vm.err.iso_required_detail')
    });
  }
  const vm = state.selectedVm || findCachedVm(vmId);
  const accepted = await askVmConfirm({
    title: t('vm.confirm.attach.title'),
    message: t('vm.confirm.attach.message', { name: getVmName(vm), path }),
    confirmLabel: t('vm.confirm.attach.confirm'),
    danger: false
  });
  if (!accepted) return;

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
  requireRbac('operate', vmActionName(action));
  const value = Number.parseInt(String(rawValue ?? ''), 10);
  if (!Number.isFinite(value)) {
    throw normalizeError({
      code: 'PCV_VM_RESOURCE_VALUE_REQUIRED',
      message: t('vm.err.resource_required'),
      detail: t('vm.err.resource_required_detail', { name: valueName, action: vmActionName(action) })
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
      detail: t('vm.err.field_required', { name })
    });
  }

  return value;
}

function readNonNegativeInt(formData, name) {
  const value = Number.parseInt(String(formData.get(name) ?? ''), 10);
  if (!Number.isFinite(value) || value < 0) {
    throw normalizeError({
      code: 'PCV_VM_QOS_VALUE_INVALID',
      message: t('vm.err.qos_nonneg'),
      detail: t('vm.err.qos_nonneg_detail', { name })
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
      detail: t('vm.err.bound_detail', { name, min: String(min), max: String(max) })
    });
  }

  return value;
}

function readVmQosPayload(kind, formData) {
  if (kind === 'storage') {
    return {
      disk: readRequiredText(formData, 'disk', 'PCV_VM_QOS_STORAGE_DISK_REQUIRED', t('vm.err.qos_disk')),
      maximum_iops: readNonNegativeInt(formData, 'maximum_iops'),
      minimum_iops: readNonNegativeInt(formData, 'minimum_iops')
    };
  }

  return {
    adapter: readRequiredText(formData, 'adapter', 'PCV_VM_QOS_NETWORK_ADAPTER_REQUIRED', t('vm.err.qos_adapter')),
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
      t('vm.err.guest_cred')),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      t('vm.err.guest_timeout')),
    command: [
      readRequiredText(
        formData,
        'command',
        'PCV_GUEST_EXEC_COMMAND_REQUIRED',
        t('vm.err.guest_command'))
    ]
  };
}

function readVmGuestFilePayload(formData) {
  return {
    host_path: readRequiredText(
      formData,
      'host_path',
      'PCV_GUEST_FILE_PATH_NOT_ALLOWED',
      t('vm.err.guest_file_host')),
    guest_path: readRequiredText(
      formData,
      'guest_path',
      'PCV_GUEST_FILE_PATH_NOT_ALLOWED',
      t('vm.err.guest_file_guest')),
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_FILE_CREDENTIAL_REF_REQUIRED',
      t('vm.err.guest_file_cred')),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      t('vm.err.guest_file_timeout')),
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
      t('vm.err.channel_cred')),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      t('vm.err.channel_timeout'))
  };
}

function buildVmQosConfirmation(vmId, kind, payload) {
  const vm = state.selectedVm || findCachedVm(vmId);
  const target = kind === 'storage' ? payload.disk : payload.adapter;
  const maximum = kind === 'storage' ? payload.maximum_iops : payload.maximum_kbps;
  const minimum = kind === 'storage' ? payload.minimum_iops : payload.minimum_kbps;
  return t('vm.confirm.qos.message', {
    name: getVmName(vm),
    kind: kind === 'storage' ? t('vm.qos.kind.storage') : t('vm.qos.kind.network'),
    target: String(target ?? ''),
    maximum: String(maximum ?? ''),
    minimum: String(minimum ?? '')
  });
}

function buildVmGuestExecutionConfirmation(vmId, mode) {
  const vm = state.selectedVm || findCachedVm(vmId);
  const name = getVmName(vm);
  return mode === 'repair'
    ? t('vm.confirm.guest_repair.message', { name })
    : t('vm.confirm.guest_exec.message', { name });
}

async function queueVmQosDirectControl(vmId, kind, mode, payload) {
  requireRbac('operate', t('vm.rbac.qos', {
    kind: kind === 'storage' ? t('vm.qos.kind.storage') : t('vm.qos.kind.network'),
    mode: vmModeName(mode)
  }));
  const apply = mode === 'apply';
  if (apply) {
    const accepted = await askVmConfirm({
      title: t('vm.confirm.qos.title'),
      message: buildVmQosConfirmation(vmId, kind, payload),
      confirmLabel: t('vm.confirm.qos.confirm'),
      danger: false
    });
    if (!accepted) return;
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
    state.shellMessage = t('vm.shell.qos_done', {
      name: vmId,
      kind: kind === 'storage' ? t('vm.qos.kind.storage') : t('vm.qos.kind.network'),
      mode: vmModeName(mode)
    });
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
  requireRbac(requiredPermission, t('vm.rbac.guest', { mode: vmModeName(mode) }));
  if (mode === 'exec' || mode === 'repair') {
    const repair = mode === 'repair';
    const accepted = await askVmConfirm({
      title: repair ? t('vm.confirm.guest_repair.title') : t('vm.confirm.guest_exec.title'),
      message: buildVmGuestExecutionConfirmation(vmId, mode),
      confirmLabel: repair ? t('vm.confirm.guest_repair.confirm') : t('vm.confirm.guest_exec.confirm'),
      danger: repair
    });
    if (!accepted) return;
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
    state.shellMessage = t('vm.shell.guest_queued', { name: vmId, mode: vmModeName(mode) });
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
  requireRbac('read', t('vm.rbac.delete_status'));
  state.actionPending = true;
  setVmActionPending(vmId, 'delete-status');
  state.error = null;
  render();
  try {
    const status = await desktopApi.getVmDeleteStatus(vmId);
    state.shellMessage = t('vm.shell.delete_status', {
      name: status?.name || vmId,
      status: status?.status || t('vm.state.unknown')
    });
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
  requireRbac('operate', t('vm.rbac.manage'));
  const vm = state.selectedVm || findCachedVm(vmId);
  const accepted = await askVmConfirm({
    title: t('vm.confirm.manage.title'),
    message: t('vm.confirm.manage.message', { name: getVmName(vm) }),
    confirmLabel: t('vm.confirm.manage.confirm'),
    danger: false
  });
  if (!accepted) return;

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
  requireRbac('guest.exec', t('vm.rbac.guest_file'));
  state.actionPending = true;
  setVmActionPending(vmId, 'guest-file');
  state.error = null;
  render();
  try {
    const preview = await desktopApi.previewVmGuestFile(vmId, payload);
    const sizeBytes = preview?.size_bytes;
    const size = sizeBytes === null || sizeBytes === undefined || sizeBytes === '' ? '-' : String(sizeBytes);
    const accepted = await askVmConfirm({
      title: t('vm.confirm.guest_file.title'),
      message: t('vm.confirm.guest_file.message', {
        host: String(payload?.host_path || ''),
        guest: String(payload?.guest_path || ''),
        size
      }),
      confirmLabel: t('vm.confirm.guest_file.confirm'),
      danger: true
    });
    if (!accepted) return;

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
  requireRbac('operate', t('vm.rbac.template_lock'));
  const vm = state.selectedVm || findCachedVm(vmId);
  if (locked) {
    const accepted = await PCV.components.confirmDialog({
      title: t('vm.lock.title'),
      message: t('vm.lock.message', {
        name: getVmName(vm),
        state: getVmState(vm) || t('vm.state.unknown')
      }),
      confirmLabel: t('vm.lock.confirm'),
      cancelLabel: t('ui.confirm.cancel')
    });
    if (!accepted) return;
  } else {
    const accepted = await PCV.components.confirmDialog({
      title: t('vm.unlock.title'),
      message: t('vm.unlock.message', { name: getVmName(vm) }),
      confirmLabel: t('vm.unlock.confirm'),
      cancelLabel: t('ui.confirm.cancel')
    });
    if (!accepted) return;
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

async function queueVmClone(vmId, rawName, rawRoot) {
  requireRbac('operate', t('vm.rbac.clone'));
  const name = String(rawName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_VM_CLONE_NAME_REQUIRED',
      message: t('vm.err.clone_name'),
      detail: t('vm.err.clone_name_detail')
    });
  }

  const payload = cloneRequestPayload(vmId, name, rawRoot);

  state.actionPending = true;
  setVmActionPending(vmId, 'clone');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmClone(vmId, payload);
    pcvTrackJob(job);
    state.connectionState = 'connected';
    startPolling();
    return job;
  } catch (error) {
    state.error = normalizeError(error);
    throw error;
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmDelete(vmId) {
  requireRbac('operate', t('vm.rbac.delete'));
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmState = getVmState(vm);
  if (isRunningVmState(vmState)) {
    throw normalizeError({
      code: 'PCV_VM_DELETE_RUNNING_BLOCKED',
      message: t('vm.err.delete_running'),
      detail: t('vm.err.delete_running_detail')
    });
  }
  const vmName = getVmName(vm);
  const accepted = await PCV.components.confirmDialog({
    title: t('vm.delete.title'),
    message: t('vm.delete.message', { name: vmName, checkpoints: String(vmCheckpointCount(vm)) }),
    confirmLabel: t('vm.delete.confirm_label'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: true,
    typedName: vmName
  });
  if (!accepted) return;

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
  requireRbac('operate', t('vm.rbac.checkpoint_create'));
  const name = checkpointName == null ? '' : String(checkpointName);
  const issue = checkpointNameIssue(name, currentCheckpointNames());
  if (issue) {
    throw normalizeError({
      code: issue.code,
      message: t(issue.messageKey)
    });
  }

  state.checkpointPending = true;
  setCheckpointActionPending(vmId, 'create', 'create');
  state.error = null;
  render();
  try {
    const job = await desktopApi.createCheckpoint(vmId, name);
    watchCheckpointSurface(vmId, job);
    state.connectionState = 'connected';
    if (typeof startPolling === 'function') startPolling();
    return job;
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, 'create');
  }
}

function checkpointRestoreBlocked(vm) {
  const power = String((typeof getVmState === 'function' ? getVmState(vm) : '') || '').trim().toLowerCase();
  return power !== '' && power !== 'off' && power !== 'stopped' && power !== 'saved';
}

async function queueCheckpointRestore(vmId, checkpointId) {
  requireRbac('operate', t('vm.rbac.checkpoint_restore'));
  const vm = state.selectedVm || (typeof findCachedVm === 'function' ? findCachedVm(vmId) : null);
  const blocked = checkpointRestoreBlocked(vm);
  const running = typeof isRunningVmState === 'function' && isRunningVmState(typeof getVmState === 'function' ? getVmState(vm) : '');
  const name = checkpointLabel(checkpointId);
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return;
  const accepted = await PCV.components.confirmDialog({
    title: t('vm.checkpoint.restore.title'),
    message: blocked
      ? (running ? t('vm.checkpoint.restore.running', { name }) : t('vm.checkpoint.restore.poweroff'))
      : t('vm.checkpoint.restore.message', { name }),
    confirmLabel: t('vm.checkpoint.restore'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: true
  });
  if (!accepted || blocked) return;
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'restore');
  state.error = null;
  render();
  try {
    const job = await desktopApi.restoreCheckpoint(vmId, checkpointId);
    watchCheckpointSurface(vmId, job);
    state.connectionState = 'connected';
    if (typeof startPolling === 'function') startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}

async function queueCheckpointDelete(vmId, checkpointId) {
  requireRbac('operate', t('vm.rbac.checkpoint_delete'));
  const name = checkpointLabel(checkpointId);
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return;
  const accepted = await PCV.components.confirmDialog({
    title: t('vm.checkpoint.delete.title'),
    message: t('vm.checkpoint.delete.message', { name }),
    confirmLabel: t('vm.checkpoint.delete'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: true
  });
  if (!accepted) return;
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'delete');
  state.error = null;
  render();
  try {
    const job = await desktopApi.deleteCheckpoint(vmId, checkpointId);
    watchCheckpointSurface(vmId, job);
    state.connectionState = 'connected';
    if (typeof startPolling === 'function') startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}

let createWizardBound = false;
let createWizardStep = 1;
let createWizardPhase = 'edit';
let createWizardForceClose = false;
let createWizardConfirming = false;
let createWizardRefresh = { 1: null, 2: null, 3: null };

function createField(name) {
  const form = document.getElementById('create-vm-form');
  if (!form || typeof form.querySelectorAll !== 'function') return null;
  const nodes = form.querySelectorAll('[name]');
  for (let index = 0; index < nodes.length; index += 1) {
    if (nodes[index].getAttribute('name') === name) return nodes[index];
  }
  return null;
}

function createFieldValue(name) {
  const field = createField(name);
  return field ? String(field.value == null ? '' : field.value) : '';
}

function setCreateField(name, value) {
  const field = createField(name);
  if (field) field.value = value;
}

function createIntRule(min, max, code, messageKey) {
  return function (value) {
    const text = String(value == null ? '' : value).trim();
    if (!/^\d+$/.test(text)) return { code: code, messageKey: messageKey };
    const number = Number(text);
    if (number < min || number > max) return { code: code, messageKey: messageKey };
    return null;
  };
}

function createIsoRule(value) {
  if (String(value || '').trim() === '') return { code: 'PCV_CREATE_PARAMS_INVALID', messageKey: 'vm.create.wizard.iso_required' };
  return null;
}

function createNameNotIn(value) {
  const validators = PCV.components && PCV.components.validators;
  if (!validators) return { code: 'PCV_VM_NAME_INVALID', messageKey: 'vm.name.create_invalid' };
  return validators.notIn(currentVmNames())(value);
}

function resetCreateWizardFields() {
  setCreateField('name', '');
  setCreateField('cpu', '2');
  setCreateField('memory_mb', '4096');
  setCreateField('disk_gb', '40');
  setCreateField('iso_path', '');
  setCreateField('vm_root', '');
}

function createWizardDirty() {
  return createFieldValue('name') !== ''
    || createFieldValue('cpu') !== '2'
    || createFieldValue('memory_mb') !== '4096'
    || createFieldValue('disk_gb') !== '40'
    || createFieldValue('iso_path') !== ''
    || createFieldValue('vm_root').trim() !== '';
}

function finishCreateWizardClose() {
  createWizardPhase = 'edit';
  createWizardStep = 1;
  resetCreateWizardFields();
}

async function confirmDiscardCreateWizard() {
  if (createWizardConfirming) return;
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return;
  createWizardConfirming = true;
  let accepted = false;
  try {
    accepted = await PCV.components.confirmDialog({
      title: t('vm.create.wizard.discard_title'),
      message: t('vm.create.wizard.discard_message'),
      confirmLabel: t('vm.create.wizard.discard_confirm'),
      cancelLabel: t('ui.confirm.cancel'),
      danger: false
    });
  } finally {
    createWizardConfirming = false;
  }
  if (!accepted) return;
  createWizardForceClose = true;
  const dialog = document.getElementById('create-vm-dialog');
  if (dialog && typeof dialog.close === 'function') dialog.close();
}

function renderCreateSummary() {
  const host = document.getElementById('create-vm-summary');
  if (!host) return;
  clearNode(host);
  const vmRoot = createFieldValue('vm_root').trim();
  const networkNotice = t('vm.create.wizard.network_notice');
  const rows = [
    ['name', t('vm.create.wizard.summary.name'), createFieldValue('name').trim()],
    ['generation', t('vm.create.wizard.summary.generation'), t('vm.create.wizard.generation_value')],
    ['cpu', t('vm.create.wizard.summary.cpu'), `${createFieldValue('cpu')} vCPU`],
    ['memory', t('vm.create.wizard.summary.memory'), `${createFieldValue('memory_mb')} MB`],
    ['disk', t('vm.create.wizard.summary.disk'), `${createFieldValue('disk_gb')} GB`],
    ['iso', t('vm.create.wizard.summary.iso'), createFieldValue('iso_path').trim()],
    ['network', t('vm.create.wizard.summary.network'), networkNotice],
    ['vm_root', t('vm.create.wizard.summary.vm_root'), vmRoot || t('vm.create.wizard.vm_root_default')]
  ];
  rows.forEach((row) => {
    const line = document.createElement('div');
    line.className = 'pcv-wizard-summary-row';
    line.setAttribute('data-summary', row[0]);
    const label = document.createElement('dt');
    label.textContent = row[1];
    const value = document.createElement('dd');
    value.textContent = row[2];
    line.appendChild(label);
    line.appendChild(value);
    host.appendChild(line);
  });
}

function syncCreateNext() {
  const next = document.getElementById('create-vm-next');
  if (!next) return;
  const refresh = createWizardRefresh[createWizardStep];
  if (createWizardStep >= 4 || typeof refresh !== 'function') {
    next.disabled = false;
    return;
  }
  next.disabled = !refresh();
}

function focusCreateStep(step) {
  const name = step === 1 ? 'name' : step === 2 ? 'cpu' : step === 3 ? 'iso_path' : '';
  const target = name ? createField(name) : document.getElementById('create-vm-submit');
  if (target && typeof target.focus === 'function') target.focus();
}

function showCreateStep(step) {
  createWizardStep = step;
  for (let index = 1; index <= 4; index += 1) {
    const panel = document.getElementById(`create-vm-step-${index}`);
    if (panel) panel.hidden = index !== step;
    const tab = document.getElementById(`create-vm-step-tab-${index}`);
    if (!tab) continue;
    tab.classList.toggle('is-current', index === step);
    tab.classList.toggle('is-complete', index < step);
    if (index === step) tab.setAttribute('aria-current', 'step');
    else tab.removeAttribute('aria-current');
    tab.tabIndex = index === step ? 0 : -1;
  }
  const back = document.getElementById('create-vm-back');
  const next = document.getElementById('create-vm-next');
  const submit = document.getElementById('create-vm-submit');
  if (back) back.hidden = step === 1;
  if (next) next.hidden = step === 4;
  if (submit) submit.hidden = step !== 4;
  if (step === 4) renderCreateSummary();
  syncCreateNext();
  focusCreateStep(step);
}

function goCreateBack() {
  if (createWizardPhase !== 'edit' || createWizardStep <= 1) return;
  showCreateStep(createWizardStep - 1);
}

function goCreateNext() {
  if (createWizardPhase !== 'edit' || createWizardStep >= 4) return;
  syncCreateNext();
  const next = document.getElementById('create-vm-next');
  if (!next || next.disabled) return;
  showCreateStep(createWizardStep + 1);
}

function setCreateWizardEditing(editing) {
  const panels = document.getElementById('create-vm-panels');
  const actions = document.getElementById('create-vm-actions');
  const steps = document.getElementById('create-vm-steps');
  const job = document.getElementById('create-vm-job');
  if (panels) panels.hidden = !editing;
  if (actions) actions.hidden = !editing;
  if (steps) steps.hidden = !editing;
  if (job) job.hidden = editing;
}

function createFailureFix(code) {
  const keys = {
    PCV_VM_NAME_INVALID: 'vm.create.wizard.fix.name',
    PCV_VM_ALREADY_EXISTS: 'vm.create.wizard.fix.duplicate',
    PCV_CPU_OUT_OF_RANGE: 'vm.create.wizard.fix.cpu',
    PCV_MEMORY_OUT_OF_RANGE: 'vm.create.wizard.fix.memory',
    PCV_DISK_OUT_OF_RANGE: 'vm.create.wizard.fix.disk',
    PCV_GENERATION_INVALID: 'vm.create.wizard.fix.generation',
    PCV_CREATE_PARAMS_INVALID: 'vm.create.wizard.fix.params'
  };
  return t(keys[code] || 'vm.create.wizard.fix.generic');
}

function createFixLine(code) {
  const line = document.createElement('p');
  line.className = 'pcv-wizard-fix';
  line.textContent = t('vm.create.wizard.failed', { code: code || '-', fix: createFailureFix(code) });
  return line;
}

function createJobPhase(job) {
  return String((job && (job.status || job.state)) || '').toLowerCase();
}

function createJobCode(job) {
  const error = job && job.error;
  if (error && typeof error === 'object' && error.code) return String(error.code);
  return String((job && (job.error_code || job.code)) || '');
}

function createOpenButton(name) {
  const button = document.createElement('button');
  button.type = 'button';
  button.className = 'btn';
  button.id = 'create-vm-open';
  button.textContent = t('vm.create.wizard.open_new');
  button.addEventListener('click', () => { openCreatedVm(name); });
  return button;
}

async function openCreatedVm(name) {
  await openClonedVm(name);
  createWizardForceClose = true;
  const dialog = document.getElementById('create-vm-dialog');
  if (dialog && dialog.open && typeof dialog.close === 'function') dialog.close();
}

function paintCreateJob(host, job, name) {
  clearNode(host);
  host.hidden = false;
  if (PCV.components && typeof PCV.components.renderJobProgress === 'function') {
    host.appendChild(PCV.components.renderJobProgress(job));
  }
  const phase = createJobPhase(job);
  if (phase === 'succeeded') host.appendChild(createOpenButton(name));
  if (phase === 'failed') host.appendChild(createFixLine(createJobCode(job)));
}

function showCreateJob(job, name) {
  createWizardPhase = 'job';
  setCreateWizardEditing(false);
  const host = document.getElementById('create-vm-job');
  if (!host) return;
  paintCreateJob(host, job, name);
  const phase = createJobPhase(job);
  const jobId = job && (job.job_id || job.id);
  if (!jobId || phase === 'succeeded' || phase === 'failed' || phase === 'canceled') return;
  if (!PCV.components || typeof PCV.components.trackJob !== 'function') return;
  PCV.components.trackJob(jobId, (next) => paintCreateJob(host, next, name));
}

function showCreateFailure(error) {
  createWizardPhase = 'job';
  setCreateWizardEditing(false);
  const host = document.getElementById('create-vm-job');
  if (!host) return;
  clearNode(host);
  host.hidden = false;
  const normalized = error || {};
  host.appendChild(createFixLine(String(normalized.code || '')));
  const detail = [normalized.message, normalized.detail].filter(Boolean).join(' ');
  if (!detail) return;
  const extra = document.createElement('p');
  extra.className = 'pcv-wizard-fix-detail';
  extra.textContent = detail;
  host.appendChild(extra);
}

function readCreateWizardPayload() {
  const payload = {
    name: createFieldValue('name').trim(),
    iso_path: createFieldValue('iso_path').trim(),
    cpu: Number(createFieldValue('cpu')),
    memory_mb: Number(createFieldValue('memory_mb')),
    disk_gb: Number(createFieldValue('disk_gb')),
    generation: 2
  };
  const vmRoot = createFieldValue('vm_root').trim();
  if (vmRoot) payload.vm_root = vmRoot;
  return payload;
}

function createWizardStepsOk() {
  for (let step = 1; step <= 3; step += 1) {
    const refresh = createWizardRefresh[step];
    if (typeof refresh !== 'function' || !refresh()) return false;
  }
  return true;
}

async function submitCreateVmWizard(event) {
  if (event && typeof event.preventDefault === 'function') event.preventDefault();
  if (createWizardPhase === 'job') return;
  if (createWizardStep < 4) {
    goCreateNext();
    return;
  }
  if (!createWizardStepsOk()) return;
  const payload = readCreateWizardPayload();
  try {
    if (typeof requireRbac === 'function') requireRbac('operate', t('vm.rbac.create'));
    const job = await desktopApi.createVm(payload);
    if (typeof pcvTrackJob === 'function') pcvTrackJob(job);
    showCreateJob(job, payload.name);
    if (typeof startPolling === 'function') startPolling();
  } catch (error) {
    const normalized = typeof normalizeError === 'function' ? normalizeError(error) : (error || {});
    showCreateFailure(normalized);
  }
}

function beginCreateWizard() {
  createWizardPhase = 'edit';
  resetCreateWizardFields();
  const job = document.getElementById('create-vm-job');
  if (job) {
    clearNode(job);
    job.hidden = true;
  }
  setCreateWizardEditing(true);
  showCreateStep(1);
}

function bindCreateVmWizard() {
  if (createWizardBound) return;
  const dialog = document.getElementById('create-vm-dialog');
  const form = document.getElementById('create-vm-form');
  if (!dialog || !form || !PCV.components || typeof PCV.components.bindValidation !== 'function') return;
  const validators = PCV.components.validators;
  createWizardRefresh = {
    1: PCV.components.bindValidation(form, {
      name: [validators.required, validators.vmCreateName, createNameNotIn]
    }, null),
    2: PCV.components.bindValidation(form, {
      cpu: [createIntRule(1, 32, 'PCV_CPU_OUT_OF_RANGE', 'vm.create.wizard.cpu_range')],
      memory_mb: [createIntRule(512, 262144, 'PCV_MEMORY_OUT_OF_RANGE', 'vm.create.wizard.memory_range')],
      disk_gb: [createIntRule(8, 4096, 'PCV_DISK_OUT_OF_RANGE', 'vm.create.wizard.disk_range')]
    }, null),
    3: PCV.components.bindValidation(form, {
      iso_path: [createIsoRule]
    }, null)
  };
  createWizardBound = true;
  form.addEventListener('input', syncCreateNext);
  form.addEventListener('change', syncCreateNext);
  form.addEventListener('submit', (event) => { submitCreateVmWizard(event); });
  const back = document.getElementById('create-vm-back');
  const next = document.getElementById('create-vm-next');
  if (back) back.addEventListener('click', goCreateBack);
  if (next) next.addEventListener('click', () => { if (!next.disabled) goCreateNext(); });
  const steps = document.getElementById('create-vm-steps');
  if (steps) {
    steps.setAttribute('aria-label', t('vm.create.wizard.steps'));
    steps.addEventListener('keydown', (event) => {
      if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft') return;
      if (event.preventDefault) event.preventDefault();
      if (event.key === 'ArrowLeft') goCreateBack();
      if (event.key === 'ArrowRight') goCreateNext();
    });
    steps.querySelectorAll('[data-step]').forEach((tab) => {
      tab.addEventListener('click', () => {
        const target = Number(tab.getAttribute('data-step'));
        if (!Number.isFinite(target) || target === createWizardStep) return;
        if (target < createWizardStep) showCreateStep(target);
      });
    });
  }
  const nativeShow = dialog.showModal.bind(dialog);
  const nativeClose = dialog.close.bind(dialog);
  dialog.showModal = function () {
    const wasOpen = dialog.open === true;
    const shown = nativeShow();
    if (wasOpen) return shown;
    return beginCreateWizard();
  };
  dialog.close = function () {
    if (createWizardForceClose) {
      createWizardForceClose = false;
      finishCreateWizardClose();
      return nativeClose();
    }
    if (createWizardPhase === 'edit' && createWizardDirty()) {
      confirmDiscardCreateWizard();
      return;
    }
    finishCreateWizardClose();
    return nativeClose();
  };
  dialog.addEventListener('cancel', (event) => {
    if (createWizardPhase !== 'edit' || !createWizardDirty()) return;
    if (event && typeof event.preventDefault === 'function') event.preventDefault();
    confirmDiscardCreateWizard();
  });
  if (typeof applyI18n === 'function') applyI18n();
}

function openCreateVmWizard() {
  bindCreateVmWizard();
  const dialog = document.getElementById('create-vm-dialog');
  if (!dialog || typeof dialog.showModal !== 'function') return undefined;
  return dialog.showModal();
}

function requestCloseCreateVmWizard() {
  const dialog = document.getElementById('create-vm-dialog');
  if (dialog && typeof dialog.close === 'function') dialog.close();
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
window.openVmCloneModal = openVmCloneModal;
window.suggestCloneName = suggestCloneName;
window.collectVmJobs = collectVmJobs;
window.queueVmDelete = queueVmDelete;
window.queueCheckpointCreate = queueCheckpointCreate;
window.queueCheckpointRestore = queueCheckpointRestore;
window.queueCheckpointDelete = queueCheckpointDelete;
window.openCheckpointCreateModal = openCheckpointCreateModal;
window.suggestCheckpointName = suggestCheckpointName;
window.refreshCheckpointSurface = refreshCheckpointSurface;
window.watchCheckpointSurface = watchCheckpointSurface;
window.bindCreateVmWizard = bindCreateVmWizard;
window.openCreateVmWizard = openCreateVmWizard;
window.requestCloseCreateVmWizard = requestCloseCreateVmWizard;
window.submitCreateVmWizard = submitCreateVmWizard;
window.readCreateWizardPayload = readCreateWizardPayload;
PCV.vm = Object.assign(PCV.vm || {}, { matchesVmFilter, getVmUpdatedValue, compareVms, renderVms, getNetworkInventory, getNetworkSwitches, formatNetworkBoolean, renderNetworkFailureGuidance, renderNetworkChangeReadback, renderNetworkInventory, renderVmDetail, renderExportImportReadback, renderCheckpointScheduleReadback, renderVmWorkbenchContext, renderCheckpointList, getSelectedVmReadbacks, readbackBucket, readbackFieldSummary, readbackErrorFor, renderReadbackCard, renderVmQosGuestReadback, getSelectedVmQosControl, renderQosControlResult, renderVmQosDirectControl, queueVmLifecycle, queueVmAttach, queueVmResourceMutation, readRequiredText, readNonNegativeInt, readBoundedInt, readVmQosPayload, readVmGuestExecPayload, readVmGuestFilePayload, readVmGuestChannelPayload, buildVmQosConfirmation, buildVmGuestExecutionConfirmation, queueVmQosDirectControl, queueVmGuestExecutionControl, refreshVmDeleteStatus, queueVmManage, queueVmGuestFile, queueVmTemplateLock, queueVmClone, queueVmDelete, queueCheckpointCreate, queueCheckpointRestore, queueCheckpointDelete, openVmCloneModal, suggestCloneName, collectVmJobs, openCheckpointCreateModal, suggestCheckpointName, refreshCheckpointSurface, watchCheckpointSurface, bindCreateVmWizard, openCreateVmWizard, requestCloseCreateVmWizard, submitCreateVmWizard, readCreateWizardPayload });
})(window.PCV);

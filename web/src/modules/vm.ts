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
  bindVmMoreMenu(root);
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
            <p class="eyebrow">Checkpoints</p>
            <h3>VM Checkpoints</h3>
          </div>
          <button data-action="checkpoint-refresh" data-vm-id="${id}"${checkpointRefreshDisabled}>Refresh checkpoints</button>
        </div>
        ${renderCheckpointScheduleReadback(vm, vmId, actionDisabled)}
        <form class="checkpoint-form" data-action="checkpoint-create" data-vm-id="${id}">
          <input name="checkpoint_name" autocomplete="off" placeholder="Checkpoint name" aria-label="checkpoint name"${checkpointMutationDisabled}>
          <button type="submit"${checkpointMutationDisabled}>Create checkpoint</button>
        </form>
        <div class="checkpoint-list">${renderCheckpointList(vmId)}</div>
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
  requireRbac('operate', 'VM clone');
  const name = String(rawName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_VM_CLONE_NAME_REQUIRED',
      message: 'Enter a target VM name.',
      detail: 'name is required before queueing vm.clone.'
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
window.openVmCloneModal = openVmCloneModal;
window.suggestCloneName = suggestCloneName;
window.collectVmJobs = collectVmJobs;
window.queueVmDelete = queueVmDelete;
window.queueCheckpointCreate = queueCheckpointCreate;
window.queueCheckpointRestore = queueCheckpointRestore;
window.queueCheckpointDelete = queueCheckpointDelete;
PCV.vm = Object.assign(PCV.vm || {}, { matchesVmFilter, getVmUpdatedValue, compareVms, renderVms, getNetworkInventory, getNetworkSwitches, formatNetworkBoolean, renderNetworkFailureGuidance, renderNetworkChangeReadback, renderNetworkInventory, renderVmDetail, renderExportImportReadback, renderCheckpointScheduleReadback, renderVmWorkbenchContext, renderCheckpointList, getSelectedVmReadbacks, readbackBucket, readbackFieldSummary, readbackErrorFor, renderReadbackCard, renderVmQosGuestReadback, getSelectedVmQosControl, renderQosControlResult, renderVmQosDirectControl, queueVmLifecycle, queueVmAttach, queueVmResourceMutation, readRequiredText, readNonNegativeInt, readBoundedInt, readVmQosPayload, readVmGuestExecPayload, readVmGuestFilePayload, readVmGuestChannelPayload, buildVmQosConfirmation, buildVmGuestExecutionConfirmation, queueVmQosDirectControl, queueVmGuestExecutionControl, refreshVmDeleteStatus, queueVmManage, queueVmGuestFile, queueVmTemplateLock, queueVmClone, queueVmDelete, queueCheckpointCreate, queueCheckpointRestore, queueCheckpointDelete, openVmCloneModal, suggestCloneName, collectVmJobs });
})(window.PCV);

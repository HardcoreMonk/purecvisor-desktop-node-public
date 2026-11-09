// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): VM list render, filter, state chips, sort, row primary action, and template list.
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

function vmSortDefaultDir(sort) {
  return String(sort || '').toLowerCase() === 'updated' ? 'desc' : 'asc';
}

function vmSortSign() {
  return state.vmSortDir === 'desc' ? -1 : 1;
}

function vmSortNumeric(vm, sort) {
  let raw;
  if (sort === 'generation') raw = vm ? vm.generation : undefined;
  else if (sort === 'cpu') {
    raw = vm && vm.cpu && typeof vm.cpu === 'object' ? vm.cpu.count : vm && vm.cpu;
    if (raw == null || raw === '') raw = vm ? (vm.vcpu != null ? vm.vcpu : vm.processor_count) : undefined;
  } else if (sort === 'memory') {
    raw = vm && vm.memory && typeof vm.memory === 'object' ? vm.memory.startup_mb : vm && vm.memory;
    if (raw == null || raw === '') raw = vm ? (vm.memory_mb != null ? vm.memory_mb : vm.memory_assigned_mb) : undefined;
  }
  if (raw == null || raw === '') return null;
  const number = Number(raw);
  return Number.isFinite(number) ? number : null;
}

function compareVms(left, right) {
  const sort = String(state.vmSort || 'name').toLowerCase();
  const dir = vmSortSign();
  const byName = getVmName(left).localeCompare(getVmName(right));
  if (sort === 'state') return dir * getVmState(left).localeCompare(getVmState(right)) || byName;
  if (sort === 'updated') return dir * (getVmUpdatedValue(left) - getVmUpdatedValue(right)) || byName;
  if (sort === 'generation' || sort === 'cpu' || sort === 'memory') {
    const a = vmSortNumeric(left, sort);
    const b = vmSortNumeric(right, sort);
    if (a == null || b == null) {
      if (a == null && b == null) return byName;
      return a == null ? 1 : -1;
    }
    return dir * (a - b) || byName;
  }
  if (sort === 'display') {
    const a = isTemplateLockedVm(left) ? 1 : 0;
    const b = isTemplateLockedVm(right) ? 1 : 0;
    return dir * (a - b) || byName;
  }
  return dir * getVmName(left).localeCompare(getVmName(right)) || byName;
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

function vmMoreMenuModel(vm) {
  const vmId = getVmId(vm);
  const pending = isVmActionPending(vmId);
  const canOperate = rbacAllows('operate');
  const canViewConsole = rbacAllows('console.view');
  const templateLocked = isTemplateLockedVm(vm);
  const actionDisabled = pending || !canOperate;
  const lockedMutationDisabled = pending || !canOperate || templateLocked;
  return {
    vmId,
    items: [
      { action: 'vm-clone-open', disabled: actionDisabled, label: t('vm.menu.clone'), danger: false },
      { action: templateLocked ? 'vm-template-unlock' : 'vm-template-lock', disabled: actionDisabled, label: t(templateLocked ? 'vm.menu.unlock' : 'vm.menu.lock'), danger: false },
      { action: 'vm-export-focus', disabled: actionDisabled, label: t('vm.menu.export'), danger: false },
      { action: 'vm-console', disabled: !canViewConsole, label: t('vm.menu.vmconnect'), danger: false },
      { action: 'vm-poweroff', disabled: lockedMutationDisabled, label: t('vm.menu.poweroff'), danger: true },
      { action: 'vm-delete', disabled: lockedMutationDisabled, label: t('vm.menu.delete'), danger: true }
    ]
  };
}

function vmMoreMenuItemsHtml(vm) {
  const model = vmMoreMenuModel(vm);
  const id = pcvEscapeHtml(model.vmId);
  const flag = {};
  const label = {};
  model.items.forEach((item) => {
    flag[item.action] = item.disabled ? ' disabled' : '';
    label[item.action] = item.label;
  });
  const templateItem = flag['vm-template-unlock'] !== undefined
    ? `<button type="button" role="menuitem" data-action="vm-template-unlock" data-vm-id="${id}"${flag['vm-template-unlock']}>${pcvEscapeHtml(label['vm-template-unlock'])}</button>`
    : `<button type="button" role="menuitem" data-action="vm-template-lock" data-vm-id="${id}"${flag['vm-template-lock']}>${pcvEscapeHtml(label['vm-template-lock'])}</button>`;
  return [
    `<button type="button" role="menuitem" data-action="vm-clone-open" data-vm-id="${id}"${flag['vm-clone-open']}>${pcvEscapeHtml(label['vm-clone-open'])}</button>`,
    templateItem,
    `<button type="button" role="menuitem" data-action="vm-export-focus" data-vm-id="${id}"${flag['vm-export-focus']}>${pcvEscapeHtml(label['vm-export-focus'])}</button>`,
    `<button type="button" role="menuitem" data-action="vm-console" data-vm-id="${id}"${flag['vm-console']}>${pcvEscapeHtml(label['vm-console'])}</button>`,
    `<button type="button" role="menuitem" class="pcv-menu-danger" data-action="vm-poweroff" data-vm-id="${id}"${flag['vm-poweroff']}>${pcvEscapeHtml(label['vm-poweroff'])}</button>`,
    `<button type="button" role="menuitem" class="pcv-menu-danger" data-action="vm-delete" data-vm-id="${id}"${flag['vm-delete']}>${pcvEscapeHtml(label['vm-delete'])}</button>`
  ].join('');
}

function vmListRowMenuButton(vm) {
  const vmId = pcvEscapeHtml(getVmId(vm));
  const label = pcvEscapeHtml(t('vm.list.row_menu', { name: getVmName(vm) }));
  return `<button type="button" class="btn pcv-row-menu" data-action="vm-row-menu" data-vm-id="${vmId}" aria-haspopup="menu" aria-expanded="false" aria-controls="pcv-vm-row-menu" aria-label="${label}">⋯</button>`;
}

let vmRowMenuOpener = null;
let vmRowMenuToggle = null;

function vmListEventElement(target) {
  let node = target;
  if (node && node.nodeType === 3) node = node.parentNode;
  return node && node.nodeType === 1 ? node : null;
}

function vmListRowFromTarget(target) {
  const node = vmListEventElement(target);
  if (!node || typeof node.closest !== 'function') return null;
  const row = node.closest('tr[data-vm-id]');
  if (!row || typeof row.closest !== 'function' || !row.closest('#vm-table')) return null;
  return row;
}

function vmRowMenuEnabledItems(menu) {
  return Array.prototype.filter.call(menu.querySelectorAll('[role="menuitem"]'), (item) => !item.disabled);
}

function closeVmRowMenu() {
  const menu = document.getElementById('pcv-vm-row-menu');
  if (menu) menu.hidden = true;
  if (vmRowMenuToggle && typeof vmRowMenuToggle.setAttribute === 'function') vmRowMenuToggle.setAttribute('aria-expanded', 'false');
  vmRowMenuToggle = null;
  vmRowMenuOpener = null;
}

function placeVmRowMenu(menu, opener, point) {
  let left = 0;
  let top = 0;
  if (point && Number.isFinite(point.x) && Number.isFinite(point.y)) {
    left = point.x;
    top = point.y;
  } else if (opener && typeof opener.getBoundingClientRect === 'function') {
    const rect = opener.getBoundingClientRect();
    left = rect.left || 0;
    top = rect.bottom || 0;
  }
  menu.style.position = 'fixed';
  menu.style.left = left + 'px';
  menu.style.top = top + 'px';
  menu.style.zIndex = '80';
}

function ensureVmRowMenu() {
  let menu = document.getElementById('pcv-vm-row-menu');
  if (menu) return menu;
  menu = document.createElement('div');
  menu.id = 'pcv-vm-row-menu';
  menu.className = 'pcv-vm-menu';
  menu.setAttribute('role', 'menu');
  menu.hidden = true;
  menu.addEventListener('keydown', onVmRowMenuKey);
  menu.addEventListener('click', onVmRowMenuClick);
  if (document.body) document.body.appendChild(menu);
  return menu;
}

function fillVmRowMenu(menu, vm) {
  while (menu.firstChild) menu.removeChild(menu.firstChild);
  const model = vmMoreMenuModel(vm);
  model.items.forEach((item) => {
    const button = document.createElement('button');
    button.setAttribute('type', 'button');
    button.setAttribute('role', 'menuitem');
    button.setAttribute('data-action', item.action);
    button.setAttribute('data-vm-id', model.vmId);
    if (item.danger) button.className = 'pcv-menu-danger';
    button.disabled = !!item.disabled;
    button.textContent = item.label;
    menu.appendChild(button);
  });
}

function openVmRowMenu(vm, opener, point) {
  if (!vm) return;
  const menu = ensureVmRowMenu();
  fillVmRowMenu(menu, vm);
  const row = opener && typeof opener.closest === 'function' ? opener.closest('tr[data-vm-id]') : null;
  const toggle = opener && opener.getAttribute && opener.getAttribute('data-action') === 'vm-row-menu'
    ? opener
    : (row && typeof row.querySelector === 'function' ? row.querySelector('[data-action="vm-row-menu"]') : null);
  if (vmRowMenuToggle && vmRowMenuToggle !== toggle && typeof vmRowMenuToggle.setAttribute === 'function') {
    vmRowMenuToggle.setAttribute('aria-expanded', 'false');
  }
  vmRowMenuToggle = toggle;
  vmRowMenuOpener = opener || toggle;
  if (toggle) toggle.setAttribute('aria-expanded', 'true');
  menu.hidden = false;
  placeVmRowMenu(menu, opener || toggle, point);
  const items = vmRowMenuEnabledItems(menu);
  if (items[0] && typeof items[0].focus === 'function') items[0].focus();
}

function onVmRowMenuButtonClick(event) {
  const node = vmListEventElement(event && event.target);
  const button = node && typeof node.closest === 'function' ? node.closest('[data-action="vm-row-menu"]') : null;
  if (!button) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  if (typeof event.stopPropagation === 'function') event.stopPropagation();
  const menu = document.getElementById('pcv-vm-row-menu');
  if (menu && !menu.hidden && vmRowMenuToggle === button) {
    closeVmRowMenu();
    if (typeof button.focus === 'function') button.focus();
    return;
  }
  const vmId = button.getAttribute('data-vm-id') || '';
  const vm = asArray(state.vms).find((item) => getVmId(item) === vmId);
  openVmRowMenu(vm, button, null);
}

function onVmRowMenuKey(event) {
  const menu = document.getElementById('pcv-vm-row-menu');
  if (!menu || menu.hidden || !event) return;
  if (event.key === 'Tab') {
    closeVmRowMenu();
    return;
  }
  const items = vmRowMenuEnabledItems(menu);
  if (event.key === 'Escape') {
    if (typeof event.preventDefault === 'function') event.preventDefault();
    const opener = vmRowMenuOpener;
    closeVmRowMenu();
    if (opener && typeof opener.focus === 'function') opener.focus();
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
}

function onVmRowMenuClick(event) {
  const node = vmListEventElement(event && event.target);
  const item = node && typeof node.closest === 'function' ? node.closest('[role="menuitem"]') : null;
  if (!item || item.disabled) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  if (typeof event.stopPropagation === 'function') event.stopPropagation();
  const action = item.getAttribute('data-action') || '';
  const vmId = item.getAttribute('data-vm-id') || '';
  closeVmRowMenu();
  state.selectedVmId = vmId;
  state.selectedVm = findCachedVm(vmId);
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = emptyVmReadbackState(vmId);
  state.selectedVmQosControl = null;
  state.connectionState = 'connected';
  if (typeof render === 'function') render();
  else renderVms();
  const panel = document.getElementById('vm-detail-panel');
  if (!panel || typeof panel.querySelectorAll !== 'function') return;
  const nodes = panel.querySelectorAll('button[data-action]');
  for (let index = 0; index < nodes.length; index += 1) {
    const candidate = nodes[index];
    if ((candidate.getAttribute('data-action') || '') !== action) continue;
    if ((candidate.getAttribute('data-vm-id') || '') !== vmId) continue;
    if (typeof candidate.click === 'function') candidate.click();
    return;
  }
}

function onVmRowContextMenu(event) {
  const row = vmListRowFromTarget(event && event.target);
  if (!row || !event) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  const vm = asArray(state.vms).find((item) => getVmId(item) === (row.getAttribute('data-vm-id') || ''));
  const prior = document.activeElement && row.contains(document.activeElement) ? document.activeElement : row.querySelector('[data-action="vm-row-menu"]');
  openVmRowMenu(vm, prior, { x: Number(event.clientX), y: Number(event.clientY) });
}

function onVmRowMenuContextKey(event) {
  if (!event) return;
  const openKey = event.key === 'ContextMenu' || (event.key === 'F10' && event.shiftKey);
  if (!openKey) return;
  const row = vmListRowFromTarget(event.target);
  if (!row) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  const vm = asArray(state.vms).find((item) => getVmId(item) === (row.getAttribute('data-vm-id') || ''));
  openVmRowMenu(vm, event.target, null);
}

function onVmRowMenuOutsideClick(event) {
  const menu = document.getElementById('pcv-vm-row-menu');
  if (!menu || menu.hidden) return;
  const node = vmListEventElement(event && event.target);
  if (node && menu.contains(node)) return;
  closeVmRowMenu();
}

function bindVmRowMenuButtons() {
  if (!els.vmTable || typeof els.vmTable.querySelectorAll !== 'function') return;
  const buttons = els.vmTable.querySelectorAll('[data-action="vm-row-menu"]');
  Array.prototype.forEach.call(buttons, (button) => {
    button.addEventListener('click', onVmRowMenuButtonClick);
  });
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

const VM_DETAIL_TABS = ['summary', 'console', 'checkpoints', 'hardware', 'guest', 'jobs'];
let vmRouteHold = false;
let vmDeepLinkPending = false;
let vmApplyToken = 0;
let syncingVmHash = false;

function syncVmListDetailMode() {
  const workspace = document.querySelector('.vm-workspace');
  if (!workspace || !workspace.classList) return;
  workspace.classList.toggle('is-detail', Boolean(state.selectedVmId));
}

function decodeVmRouteId(id) {
  const raw = String(id || '');
  if (!raw) return '';
  try { return decodeURIComponent(raw); } catch (_) { return raw; }
}

function normalizeVmTab(tab) {
  const key = String(tab || '').toLowerCase();
  return VM_DETAIL_TABS.indexOf(key) >= 0 ? key : 'summary';
}

function parseVmRoute(hash) {
  const raw = String(hash || '').replace(/^#\/?/, '');
  const parts = raw ? raw.split('/') : [];
  return {
    page: String(parts[0] || '').toLowerCase(),
    id: decodeVmRouteId(parts[1] || ''),
    tab: String(parts[2] || '').toLowerCase()
  };
}

function vmHashFor(id, tab) {
  return '#/vms/' + encodeURIComponent(id) + '/' + normalizeVmTab(tab);
}

function rememberVmRoute(hash) {
  const route = parseVmRoute(hash == null ? location.hash : hash);
  window.__pcvVmHashRoute = route;
  if (route.page === 'vms' && route.id) vmDeepLinkPending = true;
  return route;
}

function writeVmHash(hash, mode) {
  if (!hash || (location.hash || '') === hash) return;
  const url = String(location.pathname || '') + String(location.search || '') + hash;
  syncingVmHash = true;
  try {
    const method = mode === 'push' ? history.pushState : history.replaceState;
    if (typeof method === 'function') {
      try { method.call(history, null, '', url); } catch (_) { /* history may be a test stub */ }
    }
    if ((location.hash || '') !== hash) location.hash = hash;
  } finally {
    syncingVmHash = false;
  }
}

function currentVmDetailTab() {
  const explicit = String(window.__pcvVmDetailTab || '').toLowerCase();
  if (VM_DETAIL_TABS.indexOf(explicit) >= 0) return explicit;
  const route = parseVmRoute(location.hash);
  if (route.page === 'vms' && route.tab) return normalizeVmTab(route.tab);
  return 'summary';
}

function syncVmListHash() {
  if (vmRouteHold || syncingVmHash) return;
  if (state.activeView !== 'vms') return;
  const route = parseVmRoute(location.hash);
  if (!state.selectedVmId) {
    if (vmDeepLinkPending && route.page === 'vms' && route.id) return;
    if (route.page === 'vms' && route.id) writeVmHash('#/vms', 'replace');
    return;
  }
  vmDeepLinkPending = false;
  const desired = vmHashFor(state.selectedVmId, currentVmDetailTab());
  if ((location.hash || '') === desired) return;
  const sameVm = route.page === 'vms' && route.id && route.id === state.selectedVmId;
  writeVmHash(desired, sameVm ? 'replace' : 'push');
}

function clearVmSelection() {
  state.selectedVmId = '';
  state.selectedVm = null;
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = null;
  state.selectedVmQosControl = null;
}

function vmListFailure() {
  if (state.vmListError) return state.vmListError;
  return asArray(state.partialFailures).find((item) => item && item.operation === 'vm.list') || null;
}

function clearVmListFailure() {
  state.vmListError = null;
  if (Array.isArray(state.partialFailures)) {
    state.partialFailures = state.partialFailures.filter((item) => !item || item.operation !== 'vm.list');
  }
}

function mountVmListHost(node) {
  const host = els.vmTable;
  if (!host) return;
  if (typeof host.replaceChildren === 'function') {
    host.replaceChildren(node || undefined);
    if (!node || host.firstChild) return;
  }
  while (host.firstChild) host.removeChild(host.firstChild);
  if (node) host.appendChild(node);
}

function clearVmListFilter() {
  state.vmFilter = '';
  state.vmStateFilter = 'all';
  if (els.vmFilter) els.vmFilter.value = '';
  if (typeof render === 'function') render();
  else renderVms();
}

function retryVmListLoad() {
  const run = typeof loadVms === 'function' ? Promise.resolve(loadVms()) : Promise.resolve();
  run.then(() => {
    clearVmListFailure();
  }).catch((error) => {
    state.vmListError = typeof normalizeError === 'function' ? normalizeError(error) : { message: String(error && error.message || error || '') };
  }).then(() => {
    if (typeof render === 'function') render();
    else renderVms();
  });
}

function vmListEmptyAction(node) {
  if (!node || node.nodeType === 3) return null;
  const className = String(node.className || '');
  if (className.split(/\s+/).includes('pcv-empty-action')) return node;
  if (typeof node.querySelector === 'function') {
    const found = node.querySelector('.pcv-empty-action');
    if (found) return found;
  }
  const kids = node.children || node.childNodes || [];
  for (const child of kids) {
    const found = vmListEmptyAction(child);
    if (found) return found;
  }
  return null;
}

function renderVmListEmpty(filtered) {
  if (!PCV.components || typeof PCV.components.emptyState !== 'function') return;
  if (filtered) {
    mountVmListHost(PCV.components.emptyState({
      titleKey: 'vm.list.empty_filter',
      bodyKey: 'vm.list.empty_filter',
      actionLabelKey: 'vm.list.clear_filter',
      onAction: clearVmListFilter
    }));
    return;
  }
  const node = PCV.components.emptyState({
    titleKey: 'vm.list.empty',
    bodyKey: 'vm.list.empty_body',
    actionLabelKey: 'vm.list.create'
  });
  const action = vmListEmptyAction(node);
  if (action) action.setAttribute('data-shell-action', 'open-create-vm');
  mountVmListHost(node);
}

function renderVmListFailure(failure) {
  if (!PCV.components || typeof PCV.components.loadError !== 'function') return;
  mountVmListHost(PCV.components.loadError({
    titleKey: 'ui.load_error.title',
    detail: failure.message || failure.detail || '',
    onRetry: retryVmListLoad
  }));
}

async function applyVmDeepLink(id, tab) {
  const token = ++vmApplyToken;
  vmRouteHold = true;
  let failed = false;
  try {
    if (typeof loadVms === 'function') await loadVms();
  } catch (_) {
    failed = true;
  }
  if (token !== vmApplyToken) return;
  if (failed) {
    vmRouteHold = false;
    vmDeepLinkPending = false;
    if (typeof render === 'function') render();
    else renderVms();
    return;
  }
  const found = asArray(state.vms).find((vm) => getVmId(vm) === id || getVmName(vm) === id);
  if (!found) {
    clearVmSelection();
    vmRouteHold = false;
    vmDeepLinkPending = false;
    writeVmHash('#/vms', 'replace');
    if (PCV.components && typeof PCV.components.toast === 'function') {
      PCV.components.toast(t('vm.list.missing'), { kind: 'info' });
    }
    if (typeof render === 'function') render();
    else renderVms();
    return;
  }
  state.selectedVmId = getVmId(found);
  state.selectedVm = found;
  window.__pcvVmDetailTab = normalizeVmTab(tab);
  vmRouteHold = false;
  vmDeepLinkPending = false;
  writeVmHash(vmHashFor(state.selectedVmId, window.__pcvVmDetailTab), 'replace');
  if (typeof render === 'function') render();
  else renderVms();
}

function vmListTabIdFromButton(button) {
  if (!button) return '';
  const match = /^pcv-tab-(summary|console|checkpoints|hardware|guest|jobs)-/.exec(String(button.id || ''));
  return match ? match[1] : '';
}

function vmListOnDetailTabClick(event) {
  const target = event && event.target;
  if (!target || typeof target.closest !== 'function') return;
  const button = target.closest('[role="tab"]');
  if (!button || typeof button.closest !== 'function' || !button.closest('#vm-detail-panel')) return;
  const tab = vmListTabIdFromButton(button);
  if (!tab) return;
  window.__pcvVmDetailTab = tab;
  syncVmListHash();
}

function vmListOnDetailTabKey(event) {
  const key = event && event.key;
  if (key !== 'ArrowRight' && key !== 'ArrowLeft' && key !== 'ArrowDown' && key !== 'ArrowUp' && key !== 'Home' && key !== 'End') return;
  const target = event.target;
  if (!target || typeof target.closest !== 'function' || !target.closest('#vm-detail-panel')) return;
  const selected = document.querySelector('#vm-detail-panel [role="tab"][aria-selected="true"]');
  const tab = vmListTabIdFromButton(selected);
  if (!tab) return;
  window.__pcvVmDetailTab = tab;
  syncVmListHash();
}

function vmListOnHashChange() {
  if (vmRouteHold || syncingVmHash) return;
  const route = window.__pcvVmHashRoute || parseVmRoute(location.hash);
  if (route.page !== 'vms' || route.id || !state.selectedVmId) return;
  clearVmSelection();
  vmDeepLinkPending = false;
  if (typeof render === 'function') render();
  else renderVms();
}

// A periodic render rebuilds the rows. Keep an open row menu open on the rebuilt toggle, like the
// detail more menu does, and close it only when its VM is no longer listed.
function keepVmRowMenuAfterRender(vms) {
  const menu = document.getElementById('pcv-vm-row-menu');
  if (!menu || menu.hidden) return;
  const first = menu.querySelector('[role="menuitem"]');
  const vmId = first ? first.getAttribute('data-vm-id') || '' : '';
  const vm = asArray(vms).find((item) => getVmId(item) === vmId);
  const toggle = vm && els.vmTable && typeof els.vmTable.querySelectorAll === 'function'
    ? Array.prototype.find.call(els.vmTable.querySelectorAll('[data-action="vm-row-menu"]'), (button) => button.getAttribute('data-vm-id') === vmId)
    : null;
  if (!vm || !toggle) {
    closeVmRowMenu();
    return;
  }
  const active = document.activeElement;
  const focusAction = active && menu.contains(active) ? active.getAttribute('data-action') || '' : '';
  fillVmRowMenu(menu, vm);
  if (!vmRowMenuOpener || vmRowMenuOpener === vmRowMenuToggle || vmRowMenuOpener.isConnected === false) vmRowMenuOpener = toggle;
  vmRowMenuToggle = toggle;
  toggle.setAttribute('aria-expanded', 'true');
  if (!focusAction) return;
  const again = menu.querySelector(`[data-action="${focusAction}"]`);
  const target = again && !again.disabled ? again : vmRowMenuEnabledItems(menu)[0];
  if (target && typeof target.focus === 'function') target.focus();
}

function vmSortHeader(key, labelKey) {
  const sort = String(state.vmSort || 'name').toLowerCase();
  const active = sort === key;
  const dir = state.vmSortDir === 'desc' ? 'desc' : 'asc';
  const aria = active ? (dir === 'desc' ? 'descending' : 'ascending') : 'none';
  const mark = active ? (dir === 'desc' ? '▼' : '▲') : '';
  const symbol = mark ? ` <span aria-hidden="true">${mark}</span>` : '';
  return `<th aria-sort="${aria}"><button type="button" data-vm-sort="${key}">${pcvEscapeHtml(t(labelKey))}${symbol}</button></th>`;
}

function syncVmSortSelect() {
  if (!els.vmSort) return;
  const sort = String(state.vmSort || 'name');
  if (els.vmSort.value !== sort) els.vmSort.value = sort;
}

function onVmSortHeaderClick(event) {
  const node = vmListEventElement(event && event.target);
  const button = node && typeof node.closest === 'function' ? node.closest('button[data-vm-sort]') : null;
  if (!button) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  const key = button.getAttribute('data-vm-sort') || 'name';
  if (String(state.vmSort || 'name').toLowerCase() === key) state.vmSortDir = state.vmSortDir === 'desc' ? 'asc' : 'desc';
  else {
    state.vmSort = key;
    state.vmSortDir = vmSortDefaultDir(key);
  }
  syncVmSortSelect();
  if (typeof render === 'function') render();
  else renderVms();
}

function bindVmSortHeaderClicks() {
  if (!els.vmTable || els.vmTable.__pcvVmSortBound) return;
  els.vmTable.__pcvVmSortBound = true;
  els.vmTable.addEventListener('click', onVmSortHeaderClick);
}

function onVmSortSelectChange(event) {
  const target = event && event.target;
  if (!target || target.id !== 'vm-sort') return;
  const key = target.value || 'name';
  state.vmSort = key;
  state.vmSortDir = vmSortDefaultDir(key);
  if (typeof render === 'function') render();
  else renderVms();
}

function vmListRows() {
  const host = els.vmTable || document.getElementById('vm-table');
  if (!host || typeof host.querySelectorAll !== 'function') return [];
  return Array.prototype.filter.call(host.querySelectorAll('tbody tr[data-vm-id]'), (row) => !row.hidden);
}

function focusVmListRow(vmId) {
  const rows = vmListRows();
  for (let index = 0; index < rows.length; index += 1) {
    if ((rows[index].getAttribute('data-vm-id') || '') !== vmId) continue;
    if (typeof rows[index].focus === 'function') rows[index].focus();
    return;
  }
}

async function moveVmListSelection(vmId) {
  state.error = null;
  try {
    if (typeof loadVmDetail === 'function') await loadVmDetail(vmId);
    state.connectionState = 'connected';
  } catch (error) {
    state.error = typeof normalizeError === 'function' ? normalizeError(error) : error;
  }
  if (typeof render === 'function') render();
  else renderVms();
  focusVmListRow(vmId);
}

function onVmListJk(event) {
  if (!event || event.defaultPrevented || event.ctrlKey || event.altKey || event.metaKey) return;
  const key = String(event.key || '').toLowerCase();
  if (key !== 'j' && key !== 'k') return;
  const target = event.target;
  if (!target) return;
  const tag = String(target.tagName || '').toUpperCase();
  if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA' || target.isContentEditable) return;
  if (vmListEscapeTargetInOverlay(target)) return;
  const table = document.getElementById('vm-table');
  if (!table || typeof table.contains !== 'function' || !table.contains(target)) return;
  const rows = vmListRows();
  if (!rows.length) return;
  const ids = rows.map((row) => row.getAttribute('data-vm-id') || '');
  const current = ids.indexOf(state.selectedVmId || '');
  let next = -1;
  if (current < 0) next = key === 'j' ? 0 : ids.length - 1;
  else if (key === 'j') next = current + 1;
  else next = current - 1;
  if (next < 0 || next >= ids.length) return;
  if (typeof event.preventDefault === 'function') event.preventDefault();
  moveVmListSelection(ids[next]);
}

let vmBulkJobs = [];

function pruneVmChecked() {
  const known = new Set(asArray(state.vms).map((vm) => getVmId(vm)));
  state.vmChecked = asArray(state.vmChecked).map((id) => String(id || '')).filter((id) => id && known.has(id));
}

function vmCheckedSet() {
  return new Set(asArray(state.vmChecked).map((id) => String(id)));
}

function checkedVmRecords() {
  const byId = new Map(asArray(state.vms).map((vm) => [getVmId(vm), vm]));
  return asArray(state.vmChecked).map((id) => byId.get(String(id))).filter(Boolean);
}

function vmBulkSkipped(vm) {
  if (isTemplateLockedVm(vm)) return true;
  return typeof isVmActionPending === 'function' && isVmActionPending(getVmId(vm));
}

function vmCanBulkDelete(vm) {
  if (!vm || vmBulkSkipped(vm)) return false;
  const kind = typeof vmPowerKind === 'function' ? vmPowerKind(vm) : 'off';
  return kind !== 'running';
}

function vmBulkDeleteRecords(vmIds) {
  if (!Array.isArray(vmIds)) return checkedVmRecords();
  const byId = new Map(asArray(state.vms).map((vm) => [getVmId(vm), vm]));
  return vmIds.map((entry) => {
    if (entry && typeof entry === 'object') return entry;
    return byId.get(String(entry || '')) || null;
  }).filter(Boolean);
}

function vmBulkDeleteTargets(vmIds) {
  return vmBulkDeleteRecords(vmIds).filter(vmCanBulkDelete);
}

function vmBulkTargets(action) {
  return checkedVmRecords().filter((vm) => {
    if (vmBulkSkipped(vm)) return false;
    const kind = typeof vmPowerKind === 'function' ? vmPowerKind(vm) : 'off';
    if (action === 'start') return kind === 'off';
    if (action === 'shutdown') return kind === 'running';
    return false;
  });
}

function vmBulkBarHtml() {
  const checked = checkedVmRecords();
  if (!checked.length) return '';
  const skipped = checked.filter(vmBulkSkipped).length;
  const skipText = skipped > 0 ? `<span>${pcvEscapeHtml(t('vm.list.skipped', { count: String(skipped) }))}</span>` : '';
  const deleteTargets = checked.filter(vmCanBulkDelete);
  const deleteSkipped = checked.length - deleteTargets.length;
  const deleteSkipText = deleteSkipped > 0 ? `<span>${pcvEscapeHtml(t('vm.list.bulk_delete_skipped', { count: String(deleteSkipped) }))}</span>` : '';
  const noOperate = typeof rbacAllows === 'function' && !rbacAllows('operate');
  const locked = noOperate ? ' disabled' : '';
  const deleteLocked = (noOperate || deleteTargets.length === 0) ? ' disabled' : '';
  return `<div class="pcv-vm-bulk" data-vm-bulk>
    <span>${pcvEscapeHtml(t('vm.list.selected_count', { count: String(checked.length) }))}</span>
    ${skipText}
    ${deleteSkipText}
    <button type="button" data-vm-bulk-action="start"${locked}>${pcvEscapeHtml(t('vm.power.start'))}</button>
    <button type="button" data-vm-bulk-action="shutdown"${locked}>${pcvEscapeHtml(t('vm.power.shutdown'))}</button>
    <button type="button" class="pcv-menu-danger" data-vm-bulk-action="delete"${deleteLocked}>${pcvEscapeHtml(t('vm.list.bulk_delete'))}</button>
    <button type="button" data-vm-bulk-action="clear">${pcvEscapeHtml(t('vm.list.clear_checked'))}</button>
  </div>`;
}

function paintVmBulkProgress() {
  const host = els.vmTable && typeof els.vmTable.querySelector === 'function'
    ? els.vmTable.querySelector('[data-vm-bulk-progress]')
    : null;
  if (!host) return;
  while (host.firstChild) host.removeChild(host.firstChild);
  vmBulkJobs.forEach((row) => {
    const line = document.createElement('div');
    line.className = 'pcv-vm-bulk-line';
    line.setAttribute('data-vm-bulk-line', row.vmId);
    const name = document.createElement('span');
    name.textContent = row.name;
    line.appendChild(name);
    if (row.job && PCV.components && typeof PCV.components.renderJobProgress === 'function') {
      line.appendChild(PCV.components.renderJobProgress(row.job));
    }
    host.appendChild(line);
  });
}

function rememberVmBulkJob(vmId, name, job) {
  vmBulkJobs.push({ vmId, name, job, jobId: job && (job.job_id || job.id) || '' });
}

function watchVmBulkJob(vmId, job) {
  const jobId = job && (job.job_id || job.id);
  if (!jobId || !PCV.components || typeof PCV.components.trackJob !== 'function') return;
  PCV.components.trackJob(jobId, (next) => {
    const row = vmBulkJobs.find((item) => item.vmId === vmId && item.jobId === jobId);
    if (!row) return;
    row.job = next;
    paintVmBulkProgress();
  });
}

function syncVmCheckAllBox() {
  if (!els.vmTable || typeof els.vmTable.querySelector !== 'function') return;
  const box = els.vmTable.querySelector('input[data-vm-check="all"]');
  if (!box) return;
  const visible = vmListRows().map((row) => row.getAttribute('data-vm-id') || '').filter(Boolean);
  const selected = vmCheckedSet();
  const count = visible.filter((id) => selected.has(id)).length;
  box.checked = visible.length > 0 && count === visible.length;
  box.indeterminate = count > 0 && count < visible.length;
}

function onVmCheckClick(event) {
  const box = event && event.currentTarget;
  if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
  if (!box) return;
  if (box.getAttribute('data-vm-check') === 'all') {
    const visible = vmListRows().map((row) => row.getAttribute('data-vm-id') || '').filter(Boolean);
    const selected = vmCheckedSet();
    const allOn = visible.length > 0 && visible.every((id) => selected.has(id));
    if (allOn) state.vmChecked = asArray(state.vmChecked).map(String).filter((id) => visible.indexOf(id) < 0);
    else {
      const next = vmCheckedSet();
      visible.forEach((id) => next.add(id));
      state.vmChecked = Array.from(next);
    }
  } else {
    const id = box.getAttribute('data-vm-id') || '';
    const next = asArray(state.vmChecked).map(String).filter((item) => item !== id);
    if (box.checked && id) next.push(id);
    state.vmChecked = next;
  }
  if (typeof render === 'function') render();
  else renderVms();
}

async function runVmBulk(action) {
  if (typeof requireRbac === 'function') requireRbac('operate', action === 'shutdown' ? t('vm.power.shutdown') : t('vm.power.start'));
  const targets = vmBulkTargets(action);
  if (!targets.length) return;
  if (action === 'shutdown') {
    const names = targets.map((vm) => getVmName(vm)).join(', ');
    if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return;
    const accepted = await PCV.components.confirmDialog({
      title: t('vm.list.bulk_shutdown_title'),
      message: t('vm.list.bulk_shutdown_message', { names }),
      confirmLabel: t('vm.power.shutdown'),
      cancelLabel: t('ui.confirm.cancel'),
      danger: false
    });
    if (!accepted) return;
  }
  for (let index = 0; index < targets.length; index += 1) {
    const vm = targets[index];
    const vmId = getVmId(vm);
    state.actionPending = true;
    if (typeof setVmActionPending === 'function') setVmActionPending(vmId, action);
    state.error = null;
    if (typeof render === 'function') render();
    else renderVms();
    try {
      const job = await desktopApi.queueVmAction(vmId, action);
      if (typeof pcvTrackJob === 'function') pcvTrackJob(job);
      rememberVmBulkJob(vmId, getVmName(vm), job);
      watchVmBulkJob(vmId, job);
      state.connectionState = 'connected';
      if (typeof startPolling === 'function') startPolling();
    } catch (error) {
      state.error = typeof normalizeError === 'function' ? normalizeError(error) : error;
    } finally {
      if (typeof clearVmActionPending === 'function') clearVmActionPending(vmId);
      state.actionPending = false;
      if (typeof render === 'function') render();
      else renderVms();
    }
  }
}

function onVmBulkClick(event) {
  const button = event && event.currentTarget;
  if (event && typeof event.preventDefault === 'function') event.preventDefault();
  if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
  if (!button || button.disabled) return;
  const action = button.getAttribute('data-vm-bulk-action') || '';
  if (action === 'clear') {
    state.vmChecked = [];
    if (typeof render === 'function') render();
    else renderVms();
    return;
  }
  if (action === 'start' || action === 'shutdown') runVmBulk(action);
  if (action === 'delete') queueVmBulkDelete();
}

async function queueVmBulkDelete(vmIds) {
  if (typeof requireRbac === 'function') requireRbac('operate', t('vm.rbac.delete'));
  const targets = vmBulkDeleteTargets(vmIds);
  if (!targets.length) return;
  const names = targets.map((vm) => getVmName(vm)).join(', ');
  const count = String(targets.length);
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return;
  const accepted = await PCV.components.confirmDialog({
    title: t('vm.list.bulk_delete_title'),
    message: t('vm.list.bulk_delete_message', { names, count }),
    confirmLabel: t('vm.delete.confirm_label'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: true,
    typedName: count
  });
  if (!accepted) return;
  for (let index = 0; index < targets.length; index += 1) {
    const vm = targets[index];
    const vmId = getVmId(vm);
    state.actionPending = true;
    if (typeof setVmActionPending === 'function') setVmActionPending(vmId, 'delete');
    state.error = null;
    if (typeof render === 'function') render();
    else renderVms();
    try {
      const job = await desktopApi.deleteVm(vmId);
      if (typeof pcvTrackJob === 'function') pcvTrackJob(job);
      rememberVmBulkJob(vmId, getVmName(vm), job);
      watchVmBulkJob(vmId, job);
      state.connectionState = 'connected';
      if (typeof startPolling === 'function') startPolling();
    } catch (error) {
      state.error = typeof normalizeError === 'function' ? normalizeError(error) : error;
    } finally {
      if (typeof clearVmActionPending === 'function') clearVmActionPending(vmId);
      state.actionPending = false;
      if (typeof render === 'function') render();
      else renderVms();
    }
  }
}

function bindVmChecks() {
  if (!els.vmTable || typeof els.vmTable.querySelectorAll !== 'function') return;
  Array.prototype.forEach.call(els.vmTable.querySelectorAll('input[data-vm-check]'), (box) => {
    box.addEventListener('click', onVmCheckClick);
  });
  Array.prototype.forEach.call(els.vmTable.querySelectorAll('[data-vm-bulk-action]'), (button) => {
    button.addEventListener('click', onVmBulkClick);
  });
  syncVmCheckAllBox();
  paintVmBulkProgress();
}

function renderVms() {
  pruneVmChecked();
  syncVmListDetailMode();
  syncVmSortSelect();
  const named = asArray(state.vms).filter(matchesVmQuery);
  paintVmChips(named);
  renderTemplates();
  if (!els.vmTable) {
    closeVmRowMenu();
    syncVmListHash();
    return;
  }
  const failure = vmListFailure();
  if (failure) {
    closeVmRowMenu();
    renderVmListFailure(failure);
    syncVmListHash();
    return;
  }
  const vms = named.filter((vm) => matchesVmStateChip(vm, state.vmStateFilter)).sort(compareVms);
  if (vms.length === 0) {
    const filtered = String(state.vmFilter || '').trim() || String(state.vmStateFilter || 'all') !== 'all';
    closeVmRowMenu();
    renderVmListEmpty(filtered);
    syncVmListHash();
    return;
  }
  const rows = vms.map((vm) => {
    const vmId = getVmId(vm);
    const name = getVmName(vm);
    const selected = vmId && vmId === state.selectedVmId ? ' class="selected-row" aria-selected="true"' : '';
    const templateBadge = isTemplateLockedVm(vm)
      ? `<span class="badge badge-warn">${pcvEscapeHtml(t('vm.badge.template'))}</span>`
      : '';
    const checked = vmCheckedSet().has(vmId) ? ' checked' : '';
    return `
    <tr${selected} data-vm-id="${pcvEscapeHtml(vmId)}" tabindex="-1">
      <td><input type="checkbox" data-vm-check="row" data-vm-id="${pcvEscapeHtml(vmId)}" aria-label="${pcvEscapeHtml(t('vm.list.check', { name }))}"${checked}></td>
      <td title="${pcvEscapeHtml(name)}">${pcvEscapeHtml(name)}</td>
      <td>${stateBadge(getVmState(vm) || vm.status)}</td>
      <td>${pcvEscapeHtml(vmScalarText(vm.generation))}</td>
      <td>${pcvEscapeHtml(vmCpuText(vm))}</td>
      <td>${pcvEscapeHtml(vmMemoryText(vm))}</td>
      <td>${templateBadge}</td>
      <td>${vmListPrimaryButton(vm)}${vmListRowMenuButton(vm)}</td>
    </tr>`;
  }).join('');
  els.vmTable.innerHTML = `
    ${vmBulkBarHtml()}
    <div data-vm-bulk-progress></div>
    <table class="pcv-vm-table">
      <thead><tr><th><input type="checkbox" data-vm-check="all" aria-label="${pcvEscapeHtml(t('vm.list.check_all'))}"></th>${vmSortHeader('name', 'vm.list.col.name')}${vmSortHeader('state', 'vm.list.col.state')}${vmSortHeader('generation', 'vm.list.col.generation')}${vmSortHeader('cpu', 'vm.list.col.cpu')}${vmSortHeader('memory', 'vm.list.col.memory')}${vmSortHeader('display', 'vm.list.col.display')}<th>${pcvEscapeHtml(t('vm.list.col.action'))}</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>`;
  bindVmRowMenuButtons();
  bindVmSortHeaderClicks();
  bindVmChecks();
  keepVmRowMenuAfterRender(vms);
  syncVmListHash();
}

function vmListEscapeTargetInOverlay(target) {
  if (!target || typeof target.closest !== 'function') return false;
  return Boolean(target.closest('[role="menu"]') || target.closest('dialog') || target.closest('.pcv-confirm'));
}

function vmListEscapeClearsSelection(event) {
  if (!event || event.key !== 'Escape' || event.defaultPrevented) return;
  if (vmListEscapeTargetInOverlay(event.target)) return;
  let dialogOpen = false;
  if (typeof document.querySelectorAll === 'function') {
    document.querySelectorAll('dialog').forEach((dialog) => {
      if (dialog.open) dialogOpen = true;
    });
  }
  if (dialogOpen) return;
  if (window.cmdPaletteOpen || window.kbdHelpOpen) return;
  if (!state.selectedVmId) return;
  clearVmSelection();
  if (typeof render === 'function') render();
}

if (typeof document !== 'undefined' && document && typeof document.addEventListener === 'function') {
  document.addEventListener('keydown', onVmRowMenuContextKey);
  document.addEventListener('keydown', onVmListJk);
  document.addEventListener('change', onVmSortSelectChange);
  document.addEventListener('contextmenu', onVmRowContextMenu);
  document.addEventListener('click', onVmRowMenuOutsideClick);
  document.addEventListener('keydown', vmListEscapeClearsSelection);
  document.addEventListener('keydown', vmListOnDetailTabKey);
  document.addEventListener('click', vmListOnDetailTabClick);
}

rememberVmRoute(typeof location !== 'undefined' ? location.hash : '');
if (typeof window !== 'undefined' && window.addEventListener) {
  window.addEventListener('hashchange', () => { rememberVmRoute(location.hash); }, true);
  window.addEventListener('hashchange', vmListOnHashChange);
}
if (typeof window !== 'undefined' && typeof window.navigateToHash === 'function') {
  const previousNavigateToHash = window.navigateToHash;
  window.navigateToHash = function () {
    rememberVmRoute(location.hash);
    return previousNavigateToHash.apply(this, arguments);
  };
}
if (typeof window !== 'undefined' && typeof window.loadVms === 'function') {
  const previousLoadVms = window.loadVms;
  window.loadVms = function () {
    return Promise.resolve(previousLoadVms.apply(this, arguments)).then((result) => {
      clearVmListFailure();
      return result;
    }, (error) => {
      state.vmListError = typeof normalizeError === 'function' ? normalizeError(error) : { message: String(error && error.message || error || '') };
      throw error;
    });
  };
}
if (typeof window !== 'undefined') {
  const previousOpenResource = window.openResourceById;
  window.openResourceById = function (page, id) {
    if (String(page || '').toLowerCase() !== 'vms') {
      if (typeof previousOpenResource === 'function') return previousOpenResource.apply(this, arguments);
      return undefined;
    }
    const decoded = decodeVmRouteId(id);
    const saved = window.__pcvVmHashRoute;
    const tab = saved && saved.page === 'vms' && saved.id === decoded ? saved.tab : '';
    return applyVmDeepLink(decoded, tab);
  };
}

window.vmMoreMenuItemsHtml = vmMoreMenuItemsHtml;
window.closeVmRowMenu = closeVmRowMenu;
window.matchesVmFilter = matchesVmFilter;
window.getVmUpdatedValue = getVmUpdatedValue;
window.compareVms = compareVms;
window.renderVms = renderVms;
window.queueVmBulkDelete = queueVmBulkDelete;
PCV.vm = Object.assign(PCV.vm || {}, { matchesVmFilter, getVmUpdatedValue, compareVms, renderVms, queueVmBulkDelete });
})(window.PCV);

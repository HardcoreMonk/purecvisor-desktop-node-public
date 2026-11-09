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

function syncVmListDetailMode() {
  const workspace = document.querySelector('.vm-workspace');
  if (!workspace || !workspace.classList) return;
  workspace.classList.toggle('is-detail', Boolean(state.selectedVmId));
}

function renderVms() {
  syncVmListDetailMode();
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
    const selected = vmId && vmId === state.selectedVmId ? ' class="selected-row" aria-selected="true"' : '';
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
  if (state.commandPaletteOpen || window.cmdPaletteOpen || window.kbdHelpOpen) return;
  if (!state.selectedVmId) return;
  state.selectedVmId = '';
  state.selectedVm = null;
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = null;
  state.selectedVmQosControl = null;
  if (typeof render === 'function') render();
}

if (typeof document !== 'undefined' && document && typeof document.addEventListener === 'function') {
  document.addEventListener('keydown', vmListEscapeClearsSelection);
}

window.matchesVmFilter = matchesVmFilter;
window.getVmUpdatedValue = getVmUpdatedValue;
window.compareVms = compareVms;
window.renderVms = renderVms;
PCV.vm = Object.assign(PCV.vm || {}, { matchesVmFilter, getVmUpdatedValue, compareVms, renderVms });
})(window.PCV);

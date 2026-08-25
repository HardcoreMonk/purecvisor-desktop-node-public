// @ts-nocheck
function renderError() {
  if (!state.error) {
    els.alertRegion.innerHTML = '';
    return;
  }
  els.alertRegion.innerHTML = `<strong>${escapeHtml(state.error.code)}</strong> ${escapeHtml(state.error.message)}<div>${escapeHtml(state.error.detail)}</div>`;
}

function formatRelativeTime(timestampMs) {
  if (!timestampMs) return '—';
  const seconds = Math.max(0, Math.round((Date.now() - timestampMs) / 1000));
  if (seconds < 60) return `${seconds}s ago`;
  const minutes = Math.round(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  return `${Math.round(minutes / 60)}h ago`;
}

function hasRefreshedOperation(operation) {
  if (state.lastRefreshedAt === null) return false;
  const failures = state.partialFailures || [];
  // The Local API does not tag an auth rejection with the failing route: both 401
  // sites answer with operation 'api.auth' (DesktopNodeHostApplication.Json and
  // DesktopNodeApiAuthSessionHandler.AuthValidationFailure). A per-operation match
  // would therefore read every route as "succeeded" under a total 401 and re-expose
  // the fabricated values this gate exists to remove. An auth failure invalidates
  // every operation, including values loaded before the session expired.
  if (failures.some(isAuthError)) return false;
  return !failures.some((failure) => failure?.operation === operation);
}

function renderStatusBar() {
  if (els.statusConnection) {
    els.statusConnection.textContent = getStatusBarConnectionLabel();
  }
  if (els.statusHost) {
    const caption = hasRefreshedOperation('host.status')
      ? readNested(state.host, ['windows', 'caption'])
      : undefined;
    els.statusHost.textContent = caption ? String(caption) : '—';
  }
  if (els.statusUpdated) {
    els.statusUpdated.textContent = `Updated ${formatRelativeTime(state.lastRefreshedAt)}`;
  }
  if (els.statusVmCount) {
    if (hasRefreshedOperation('vm.list')) {
      const counts = getSummaryVmCounts();
      els.statusVmCount.textContent = `VM: ${counts.running}/${counts.total}`;
    } else {
      els.statusVmCount.textContent = 'VM: —';
    }
  }
  if (els.statusView) {
    els.statusView.textContent = state.activeView;
  }
}

function renderHeroChips() {
  if (els.heroWorkload) {
    if (hasRefreshedOperation('vm.list')) {
      const counts = getSummaryVmCounts();
      els.heroWorkload.textContent = `${counts.running}/${counts.total}`;
    } else {
      els.heroWorkload.textContent = '—';
    }
  }
  if (els.heroHostMode) {
    // getHostReadinessLabel() now gates itself, so calling it unconditionally
    // keeps this chip agreeing with the metric grid and ops cockpit when only
    // one of ops.summary / host.status came back.
    els.heroHostMode.textContent = String(getHostReadinessLabel());
  }
  if (els.heroAlerts) {
    els.heroAlerts.textContent = state.lastRefreshedAt === null
      ? '—'
      : String((state.partialFailures || []).length);
  }
}

const CONNECTION_STATE_LABELS = {
  idle: 'Idle',
  connected: 'Connected',
  degraded: 'Degraded',
  auth: 'Auth required',
  error: 'Error'
};

function getConnectionStateLabel() {
  return CONNECTION_STATE_LABELS[state.connectionState] || 'Idle';
}

// #connection-state and the footer read the same label map so they can never
// contradict each other (a partial failure showed 'Degraded' in the badge and
// 'Not connected' in the footer before). Spec §6 pins the footer wording for the
// pre-load and unauthenticated rows to 'Not connected'; those two states are not
// contradicted by the badge's 'Idle'/'Auth required', which say the same thing.
function getStatusBarConnectionLabel() {
  return state.connectionState === 'idle' || state.connectionState === 'auth'
    ? 'Not connected'
    : getConnectionStateLabel();
}

function renderConnectionState() {
  els.connectionState.className = `connection-state state-${state.connectionState}`;
  els.connectionState.textContent = getConnectionStateLabel();
}

function needsAuthGate() {
  return state.connectionState === 'auth'
    && !state.authAccessToken.trim()
    && !state.apiToken.trim();
}

function bindAlertRegionAuthGate() {
  if (!els.alertRegion || els.alertRegion.dataset.authGateSubmitBound === 'true') return;
  if (typeof els.alertRegion.addEventListener !== 'function') return;
  els.alertRegion.addEventListener('submit', async (event) => {
    const form = event.target?.closest?.('form#account-login-form');
    if (!form) return;
    await loginAccountFromForm(event);
  });
  els.alertRegion.dataset.authGateSubmitBound = 'true';
}

function renderAuthGate() {
  if (!els.alertRegion) return;
  if (!needsAuthGate()) {
    return;
  }
  const code = state.authError?.code || 'PCV_AUTH_REQUIRED';
  const message = state.authError?.message || 'Authorization bearer token is required.';
  const formHtml = state.activeView === 'troubleshooting'
    ? ''
    : `<form id="account-login-form" class="account-login-form" autocomplete="off">
        <label>Username<input id="account-username" name="username" type="text" autocomplete="username"></label>
        <label>Password<input id="account-password" name="password" type="password" autocomplete="current-password"></label>
        <button type="submit">Login</button>
      </form>`;
  const gateHtml = `<div class="diagnostics-result error" data-auth-gate="true">
    <span class="muted">Auth required</span>
    <strong>${escapeHtml(code)}</strong>
    <p>${escapeHtml(message)} Use the header api-token field or the account login form. Service tokens stay out of HTML.</p>
    ${formHtml}
  </div>`;
  const existing = els.alertRegion.innerHTML;
  els.alertRegion.innerHTML = existing ? `${gateHtml}${existing}` : gateHtml;
  bindAlertRegionAuthGate();
}

function applyUiPreferences() {
  if (document.documentElement) {
    document.documentElement.dataset.theme = state.theme;
    document.documentElement.lang = state.language;
  }
  if (els.themeSelect && els.themeSelect.value !== state.theme) {
    els.themeSelect.value = state.theme;
  }
  if (els.languageSelect && els.languageSelect.value !== state.language) {
    els.languageSelect.value = state.language;
  }
}

function renderAssetStatus() {
  els.assetStatus.innerHTML = `<span class="muted">Asset</span><strong>${escapeHtml(WEB_ASSET_LABEL)}</strong>`;
}

function getViewLabel(view) {
  return {
    dashboard: 'Dashboard',
    vms: 'VM Assets',
    network: 'Network',
    jobs: 'Jobs',
    activity: 'Activity',
    evidence: 'Evidence',
    troubleshooting: 'Troubleshooting'
  }[view] || 'Dashboard';
}

function buildCommandPaletteItems() {
  const viewItems = ['dashboard', 'vms', 'network', 'jobs', 'activity', 'evidence', 'troubleshooting']
    .map((view) => ({
      id: `view:${view}`,
      label: getViewLabel(view),
      detail: `Open ${getViewLabel(view)}`,
      tone: view === state.activeView ? 'ok' : 'info',
      view
    }));
  const commandItems = [
    { id: 'command:refresh', label: 'Refresh all', detail: 'Reload current Desktop Node API state.', tone: 'info', command: 'refresh' },
    { id: 'command:create-vm', label: 'Create VM', detail: 'Open the Windows-native VM create dialog.', tone: 'warn', command: 'open-create-vm' },
    { id: 'command:clear-browser-state', label: 'Clear browser session', detail: 'Clear browser token, selected VM, diagnostics, and tracked jobs.', tone: 'warn', command: 'clear-browser-state' }
  ];
  const vmItems = asArray(state.vms).slice(0, 20).map((vm) => ({
    id: `vm:${getVmId(vm)}`,
    label: getVmName(vm),
    detail: `VM ${getVmState(vm) || 'unknown'} / ${getVmId(vm)}`,
    tone: isRunningVmState(getVmState(vm)) ? 'ok' : 'info',
    vmId: getVmId(vm)
  }));
  const jobItems = buildActivityRows().slice(0, 20).map(({ source, job }) => ({
    id: `job:${job?.job_id || source}`,
    label: job?.operation || job?.job_id || 'job',
    detail: `${job?.status || 'unknown'} / ${job?.job_id || source}`,
    tone: String(job?.status || '').toLowerCase() === 'failed' ? 'error' : 'info',
    view: 'jobs'
  }));
  const routeItems = DESKTOP_NODE_ROUTE_COVERAGE.map((route) => ({
    id: `route:${route.id}`,
    label: route.id,
    detail: `${route.method} ${route.route}`,
    tone: route.mutating ? 'warn' : 'info',
    view: route.view === 'service' ? 'troubleshooting' : route.view
  }));
  return [...viewItems, ...commandItems, ...vmItems, ...jobItems, ...routeItems];
}

function getCommandPaletteMatches() {
  const query = state.commandQuery || state.globalSearch;
  return filterRowsByQuery(buildCommandPaletteItems(), query, (item) => [
    item.label,
    item.detail,
    item.id,
    item.view,
    item.command
  ].join(' ')).slice(0, 12);
}

function renderCommandPalette() {
  if (!els.commandPalette || !els.commandPaletteResults) return;
  const open = state.commandPaletteOpen || Boolean(state.globalSearch.trim());
  els.commandPalette.hidden = !open;
  if (els.commandPaletteInput && els.commandPaletteInput.value !== state.commandQuery) {
    els.commandPaletteInput.value = state.commandQuery;
  }
  if (els.globalSearchInput && els.globalSearchInput.value !== state.globalSearch) {
    els.globalSearchInput.value = state.globalSearch;
  }
  if (!open) {
    els.commandPaletteResults.innerHTML = '';
    return;
  }
  const items = getCommandPaletteMatches();
  els.commandPaletteResults.innerHTML = items.length
    ? items.map((item) => `<button type="button" data-command-id="${escapeHtml(item.id)}">
        <span class="status-badge ${escapeHtml(normalizeEventTone(item.tone))}">${escapeHtml(item.id.split(':')[0])}</span>
        <strong>${escapeHtml(item.label)}</strong>
        <span>${escapeHtml(item.detail)}</span>
      </button>`).join('')
    : '<p class="muted">No Windows Desktop Node command matches the current search.</p>';
}

function openCommandPalette(query = '') {
  state.commandPaletteOpen = true;
  state.commandQuery = query || state.commandQuery || state.globalSearch;
  render();
  if (els.commandPaletteInput && typeof els.commandPaletteInput.focus === 'function') {
    els.commandPaletteInput.focus();
  }
}

function closeCommandPalette() {
  state.commandPaletteOpen = false;
  state.commandQuery = '';
  state.globalSearch = '';
  render();
}

function handleCommandSearch(query) {
  state.globalSearch = query;
  state.commandQuery = query;
  state.commandPaletteOpen = Boolean(String(query || '').trim());
  render();
}

async function runCommandPaletteItem(commandId) {
  const item = buildCommandPaletteItems().find((entry) => entry.id === commandId);
  if (!item) return;
  state.commandPaletteOpen = false;
  state.commandQuery = '';
  state.globalSearch = '';
  if (item.vmId) {
    await selectVmFromShell(item.vmId);
    return;
  }
  if (item.command) {
    handleShellCommand(item.command);
    return;
  }
  if (item.view) {
    navigateToView(item.view);
  }
}

function renderWorkspaceTabs() {
  if (!els.workspaceTabbar) return;
  const views = ['dashboard', 'vms', 'network', 'jobs', 'activity', 'evidence', 'troubleshooting'];
  els.workspaceTabbar.innerHTML = views.map((view) => {
    const active = view === state.activeView;
    return `<a class="workspace-tab${active ? ' active' : ''}" role="tab" aria-selected="${active ? 'true' : 'false'}" href="#${escapeHtml(view)}" data-view-link="${escapeHtml(view)}">${escapeHtml(getViewLabel(view))}<button type="button" aria-label="${escapeHtml(getViewLabel(view))} tab pinned">x</button></a>`;
  }).join('');
}

function getAssetFilterText() {
  return String(els.assetSearchInput?.value || state.vmFilter || '').trim().toLowerCase();
}

function renderVmAssetList() {
  if (!els.vmAssetList) return;
  const filter = getAssetFilterText();
  const vms = asArray(state.vms)
    .filter((vm) => {
      if (!filter) return true;
      return [getVmName(vm), getVmId(vm), getVmState(vm), vm?.notes]
        .join(' ')
        .toLowerCase()
        .includes(filter);
    })
    .slice(0, 50);
  if (els.assetCount) {
    els.assetCount.textContent = String(vms.length);
  }
  if (vms.length === 0) {
    els.vmAssetList.innerHTML = '<p class="muted">No VM assets match the current filter.</p>';
    return;
  }
  els.vmAssetList.innerHTML = vms.map((vm) => {
    const vmId = getVmId(vm);
    const active = vmId && vmId === state.selectedVmId;
    const cpu = vm.cpu?.count ?? vm.cpu ?? vm.vcpu ?? vm.processor_count ?? '-';
    const memory = vm.memory?.startup_mb ?? vm.memory_mb ?? vm.memory ?? '-';
    const memoryLabel = Number.isFinite(Number(memory)) ? `${Math.round(Number(memory) / 1024)}G` : memory;
    const stateLabel = getVmState(vm) || '-';
    return `<button type="button" class="asset-row${active ? ' active' : ''}" data-action="select-asset-vm" data-vm-id="${escapeHtml(vmId)}">
      <span class="asset-check"></span>
      <span class="asset-star">☆</span>
      <span class="asset-health"></span>
      <strong>${escapeHtml(getVmName(vm))}</strong>
      <span>${escapeHtml(cpu)}</span>
      <span>${escapeHtml(memoryLabel)}</span>
      <span>${escapeHtml(stateLabel)}</span>
    </button>`;
  }).join('');
}

function renderActiveView() {
  document.querySelectorAll('.app-view').forEach((section) => {
    const active = section.dataset?.view === state.activeView || section.id === state.activeView;
    section.hidden = !active;
  });
  document.querySelectorAll('[data-view-link]').forEach((link) => {
    const active = link.dataset?.viewLink === state.activeView;
    link.className = active ? 'nav-active' : '';
    if (active) link.setAttribute('aria-current', 'page');
    else link.removeAttribute('aria-current');
  });
}

function render() {
  renderActiveView();
  applyUiPreferences();
  renderError();
  renderConnectionState();
  renderAuthGate();
  renderStatusBar();
  renderHeroChips();
  renderAssetStatus();
  renderCommandPalette();
  renderWorkspaceTabs();
  renderVmAssetList();
  renderOpsCockpit();
  renderMetrics();
  renderHost();
  renderVms();
  renderNetworkInventory();
  renderVmWorkbenchContext();
  renderVmDetail();
  renderJobs();
  renderDashboardActivity();
  renderActivity();
  renderEventCenter();
  renderEvidenceDashboard();
  renderMonitoring();
  renderIncidentCommand();
  renderAccountSession();
  renderConsolePanel();
  renderTokenRotation();
  renderDiagnosticsBundle();
  renderBetaFollowup();
  renderTroubleshooting();
  if (els.openCreateVm) {
    els.openCreateVm.disabled = !rbacAllows('operate');
  }
}


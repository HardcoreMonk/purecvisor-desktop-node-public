// @ts-nocheck
function getDiagnosticBundleId(bundle = state.diagnosticBundle) {
  return String(bundle?.bundle_id || bundle?.bundleId || '');
}

function renderDiagnosticBundleList() {
  const page = state.diagnosticBundlePage || {};
  const bundles = asArray(state.diagnosticBundles);
  const retention = page.retention || {};
  const nextOffset = page.next_offset;
  const hasNext = nextOffset !== null && nextOffset !== undefined && nextOffset !== '';
  const rows = bundles.length
    ? bundles.map((bundle) => `<div class="triage-row diagnostic-bundle-row">
        <div>
          <strong>${escapeHtml(getDiagnosticBundleId(bundle) || bundle.file_name || 'diagnostic bundle')}</strong>
          <span class="muted">${escapeHtml([bundle.created_at, `${bundle.size_bytes ?? 0} bytes`, bundle.redaction_status].filter(Boolean).join(' / '))}</span>
        </div>
        <span class="status-badge ok">${escapeHtml(bundle.archive_status || 'available')}</span>
      </div>`).join('')
    : '<div class="diagnostics-result"><span class="muted">Bundle list</span><strong>No retained bundles visible</strong><p>GET list route keeps pagination metadata separate from create/download.</p></div>';

  return `<div class="diagnostics-result diagnostics-list">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Retained bundles</span>
        <strong>${escapeHtml(page.returned ?? bundles.length)} / ${escapeHtml(page.count ?? bundles.length)}</strong>
      </div>
      <span class="status-badge neutral">next_offset=${escapeHtml(nextOffset ?? 'none')}</span>
    </div>
    <div class="boundary-chip-row">
      <span>max_bundle_count=${escapeHtml(retention.max_bundle_count ?? 'unavailable')}</span>
      <span>retention_days=${escapeHtml(retention.retention_days ?? 'unavailable')}</span>
      <span>limit=${escapeHtml(page.limit ?? 10)}</span>
    </div>
    <div class="triage-list">${rows}</div>
    <div class="diagnostics-actions">
      <button type="button" data-action="diagnostic-list-next" ${hasNext ? '' : 'disabled aria-disabled="true"'}>Load more bundles</button>
      <span class="muted">Read-only list pagination; no host mutation.</span>
    </div>
  </div>`;
}

function persistDiagnosticBundleDownload(download) {
  if (!download?.body || typeof Blob === 'undefined' || !window.URL?.createObjectURL) {
    return false;
  }
  const link = document.createElement('a');
  if (!link || typeof link.click !== 'function') {
    return false;
  }

  const blob = new Blob([download.body], { type: download.content_type || 'application/json' });
  const url = window.URL.createObjectURL(blob);
  link.href = url;
  link.download = download.file_name || `${download.bundle_id || 'pcv-diagnostic-bundle'}.bundle.json`;
  link.hidden = true;
  document.body?.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
  return true;
}

async function createDiagnosticBundleFromPanel() {
  requireRbac('diagnostics.create', 'diagnostic bundle create');
  state.pendingDiagnosticAction = 'creating';
  state.lastDiagnosticAction = 'create';
  state.diagnosticBundleError = null;
  render();
  try {
    const bundle = await desktopApi.createDiagnosticBundle({
      source: 'web-console',
      include: ['runtime_policy', 'ops_summary', 'job_summary'],
      requested_at: new Date().toISOString()
    });
    state.diagnosticBundle = bundle;
    state.diagnosticBundleDownload = null;
    state.connectionState = 'connected';
    await loadDiagnosticBundleList();
  } catch (error) {
    state.diagnosticBundleError = normalizeError(error);
    state.connectionState = isAuthError(state.diagnosticBundleError) ? 'auth' : 'degraded';
  } finally {
    state.pendingDiagnosticAction = '';
    render();
  }
}

async function downloadLatestDiagnosticBundle() {
  requireRbac('diagnostics.read', 'diagnostic bundle download');
  if (!getDiagnosticBundleId() && state.diagnosticBundles.length > 0) {
    state.diagnosticBundle = state.diagnosticBundles[0];
  }
  const bundleId = getDiagnosticBundleId();
  if (!bundleId) {
    state.diagnosticBundleError = normalizeError({
      code: 'PCV_DIAGNOSTIC_BUNDLE_NOT_SELECTED',
      message: 'Create a diagnostic bundle before downloading it.',
      detail: DESKTOP_NODE_API_ROUTES.diagnosticBundles
    });
    render();
    return;
  }

  state.pendingDiagnosticAction = 'downloading';
  state.lastDiagnosticAction = 'download';
  state.diagnosticBundleError = null;
  render();
  try {
    const download = await desktopApi.downloadDiagnosticBundle(bundleId);
    download.saved_in_browser = persistDiagnosticBundleDownload(download);
    state.diagnosticBundleDownload = download;
    state.connectionState = 'connected';
  } catch (error) {
    state.diagnosticBundleError = normalizeError(error);
    state.connectionState = isAuthError(state.diagnosticBundleError) ? 'auth' : 'degraded';
  } finally {
    state.pendingDiagnosticAction = '';
    render();
  }
}

function isLoopbackHostname(hostname) {
  const value = String(hostname || '').replace(/^\[|\]$/g, '').toLowerCase();
  return value === '127.0.0.1' || value === 'localhost' || value === '::1';
}

async function ensureLoopbackSession() {
  if (state.authAccessToken.trim() || state.apiToken.trim()) {
    return;
  }
  if (!isLoopbackHostname(window.location.hostname)) {
    return;
  }
  state.authPending = true;
  state.authError = null;
  try {
    const result = await desktopApi.createLoopbackSession();
    applyAccountSessionPayload(result);
  } catch (error) {
    state.authError = normalizeError(error);
    state.connectionState = 'auth';
  } finally {
    state.authPending = false;
  }
}

async function loginAccountFromForm(event) {
  event.preventDefault();
  const form = event.target.closest('form#account-login-form') || event.currentTarget;
  const data = new FormData(form);
  state.authPending = true;
  state.authError = null;
  render();
  try {
    const payload = {
      username: String(data.get('username') || '').trim(),
      password: String(data.get('password') || '')
    };
    const result = await desktopApi.loginAccount(payload);
    applyAccountSessionPayload(result);
    const passwordInput = byId('account-password');
    if (passwordInput) passwordInput.value = '';
    state.connectionState = 'connected';
    await refreshAll();
  } catch (error) {
    state.authError = normalizeError(error);
    state.connectionState = isAuthError(state.authError) ? 'auth' : 'degraded';
  } finally {
    state.authPending = false;
    render();
  }
}

async function refreshAccountSession(options = {}) {
  const silent = Boolean(options?.silent);
  if (!state.authRefreshToken) {
    state.authError = normalizeError({
      code: 'PCV_REFRESH_TOKEN_REQUIRED',
      message: 'Refresh token is not present in the browser session.',
      detail: 'Login again before refreshing the account JWT.'
    });
    if (!silent) render();
    return;
  }

  state.authPending = !silent;
  state.authError = null;
  if (!silent) render();
  try {
    const result = await desktopApi.refreshAccount({ refresh_token: state.authRefreshToken });
    applyAccountSessionPayload(result);
    state.connectionState = 'connected';
    if (!silent) await refreshAll();
  } catch (error) {
    state.authError = normalizeError(error);
    clearAccountSessionState();
    state.connectionState = 'auth';
  } finally {
    state.authPending = false;
    if (!silent) render();
  }
}

async function logoutAccount() {
  const refreshToken = state.authRefreshToken;
  state.authPending = true;
  state.authError = null;
  render();
  try {
    if (refreshToken) {
      await desktopApi.logoutAccount({ refresh_token: refreshToken });
    }
  } catch (error) {
    state.authError = normalizeError(error);
  } finally {
    clearAccountSessionState();
    state.authPending = false;
    state.tokenActionMessage = 'account JWT session cleared; service bearer token state was not changed.';
    await refreshAll();
    render();
  }
}

async function openSelectedConsole() {
  const vmId = state.selectedVmId;
  requireRbac('console.view', 'console view');
  if (!vmId) {
    throw normalizeError({
      code: 'PCV_CONSOLE_VM_REQUIRED',
      message: 'Select a VM before opening console handoff.',
      detail: 'The console route is scoped to /api/v1/vms/{id}/console.'
    });
  }

  state.consoleError = null;
  render();
  try {
    state.consoleSession = await desktopApi.getVmConsole(vmId);
  } catch (error) {
    state.consoleError = normalizeError(error);
  } finally {
    render();
  }
}

function clearBrowserToken() {
  state.apiToken = '';
  clearAccountSessionState();
  if (els.apiToken) els.apiToken.value = '';
  state.tokenActionMessage = 'browser token cleared; all views refresh; pending jobs are rechecked; token-required routes may show Auth required.';
  state.diagnosticBundleError = null;
  state.partialFailures = [];
  refreshAll();
}

function clearBrowserState() {
  state.apiToken = '';
  clearAccountSessionState();
  if (els.apiToken) els.apiToken.value = '';
  state.trackedJobs = [];
  state.selectedVmId = '';
  state.selectedVm = null;
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = null;
  state.diagnosticBundle = null;
  state.diagnosticBundles = [];
  state.diagnosticBundlePage = null;
  state.diagnosticBundleDownload = null;
  state.diagnosticBundleError = null;
  state.pendingDiagnosticAction = '';
  state.lastDiagnosticAction = '';
  state.tokenActionMessage = 'browser token cleared; all views refresh; pending jobs are rechecked; token-required routes may show Auth required.';
  state.error = null;
  try {
    window.localStorage.removeItem(JOB_HISTORY_KEY);
  } catch (_) {
    // Browser storage can be unavailable; the visible session still clears.
  }
  refreshAll();
}

function navigateToView(view) {
  setActiveView(view);
  window.location.hash = `#${state.activeView}`;
  render();
}

function handleShellCommand(command) {
  if (!command) return;
  if (command === 'refresh') {
    refreshAll();
    return;
  }
  if (command === 'open-create-vm') {
    navigateToView('vms');
    els.createDialog.showModal();
    return;
  }
  if (command === 'clear-browser-state') {
    clearBrowserState();
    return;
  }
  if (VALID_VIEWS.has(command)) {
    navigateToView(command);
  }
}


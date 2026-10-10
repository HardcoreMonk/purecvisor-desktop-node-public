// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): account session, diagnostics bundle and shell command actions (legacy actions).
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy actions.ts ---
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
          <strong>${pcvEscapeHtml(getDiagnosticBundleId(bundle) || bundle.file_name || 'diagnostic bundle')}</strong>
          <span class="muted">${pcvEscapeHtml([bundle.created_at, `${bundle.size_bytes ?? 0} bytes`, bundle.redaction_status].filter(Boolean).join(' / '))}</span>
        </div>
        <span class="status-badge ok">${pcvEscapeHtml(bundle.archive_status || 'available')}</span>
      </div>`).join('')
    : '<div class="diagnostics-result"><span class="muted">Bundle list</span><strong>No retained bundles visible</strong><p>GET list route keeps pagination metadata separate from create/download.</p></div>';

  return `<div class="diagnostics-result diagnostics-list">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Retained bundles</span>
        <strong>${pcvEscapeHtml(page.returned ?? bundles.length)} / ${pcvEscapeHtml(page.count ?? bundles.length)}</strong>
      </div>
      <span class="status-badge neutral">next_offset=${pcvEscapeHtml(nextOffset ?? 'none')}</span>
    </div>
    <div class="boundary-chip-row">
      <span>max_bundle_count=${pcvEscapeHtml(retention.max_bundle_count ?? 'unavailable')}</span>
      <span>retention_days=${pcvEscapeHtml(retention.retention_days ?? 'unavailable')}</span>
      <span>limit=${pcvEscapeHtml(page.limit ?? 10)}</span>
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

async function pcvEnsureLoopbackSession() {
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

async function createAccountFromForm(event) {
  event.preventDefault();
  const form = event.target.closest('form#account-create-form') || event.currentTarget;
  const data = new FormData(form);
  const username = String(data.get('username') || '').trim();
  const password = String(data.get('password') || '');
  const role = String(data.get('role') || 'admin').trim() || 'admin';
  const displayName = String(data.get('display_name') || '').trim();
  const bootstrap = isAccountBootstrapOpen();
  if (!window.confirm(buildAccountCreateConfirmation(username, role, bootstrap))) {
    return;
  }

  state.accountManagePending = true;
  state.accountDirectoryError = null;
  render();
  try {
    const payload = {
      username,
      password,
      role: bootstrap ? 'admin' : role,
      display_name: displayName || undefined
    };
    await desktopApi.createAccount(payload);
    const passwordInput = form.querySelector('input[name="password"]');
    if (passwordInput) passwordInput.value = '';
    if (bootstrap) {
      const result = await desktopApi.loginAccount({ username, password });
      applyAccountSessionPayload(result);
      state.connectionState = 'connected';
    }
    await refreshAll();
  } catch (error) {
    state.accountDirectoryError = normalizeError(error);
  } finally {
    state.accountManagePending = false;
    render();
  }
}

async function disableAccountFromButton(username) {
  const name = String(username || '').trim();
  if (!name) return;
  if (!window.confirm(buildAccountDisableConfirmation(name))) {
    return;
  }

  state.accountManagePending = true;
  state.accountDirectoryError = null;
  render();
  try {
    await desktopApi.disableAccount(name, { confirm_username: name });
    await refreshAll();
  } catch (error) {
    state.accountDirectoryError = normalizeError(error);
  } finally {
    state.accountManagePending = false;
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

// exports
window.getDiagnosticBundleId = getDiagnosticBundleId;
window.renderDiagnosticBundleList = renderDiagnosticBundleList;
window.persistDiagnosticBundleDownload = persistDiagnosticBundleDownload;
window.createDiagnosticBundleFromPanel = createDiagnosticBundleFromPanel;
window.downloadLatestDiagnosticBundle = downloadLatestDiagnosticBundle;
window.isLoopbackHostname = isLoopbackHostname;
window.pcvEnsureLoopbackSession = pcvEnsureLoopbackSession;
window.loginAccountFromForm = loginAccountFromForm;
window.refreshAccountSession = refreshAccountSession;
window.logoutAccount = logoutAccount;
window.createAccountFromForm = createAccountFromForm;
window.disableAccountFromButton = disableAccountFromButton;
window.openSelectedConsole = openSelectedConsole;
window.clearBrowserToken = clearBrowserToken;
window.clearBrowserState = clearBrowserState;
window.navigateToView = navigateToView;
window.handleShellCommand = handleShellCommand;
PCV.accounts = Object.assign(PCV.accounts || {}, { getDiagnosticBundleId, renderDiagnosticBundleList, persistDiagnosticBundleDownload, createDiagnosticBundleFromPanel, downloadLatestDiagnosticBundle, isLoopbackHostname, pcvEnsureLoopbackSession, loginAccountFromForm, refreshAccountSession, logoutAccount, createAccountFromForm, disableAccountFromButton, openSelectedConsole, clearBrowserToken, clearBrowserState, navigateToView, handleShellCommand });
})(window.PCV);

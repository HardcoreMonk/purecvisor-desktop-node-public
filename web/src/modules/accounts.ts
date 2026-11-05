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
          <strong>${pcvEscapeHtml(getDiagnosticBundleId(bundle) || bundle.file_name || t('account.diag.fallback'))}</strong>
          <span class="muted">${pcvEscapeHtml([bundle.created_at, t('account.diag.bytes', { size: bundle.size_bytes ?? 0 }), bundle.redaction_status].filter(Boolean).join(' / '))}</span>
        </div>
        <span class="status-badge ok">${pcvEscapeHtml(bundle.archive_status || t('account.diag.available'))}</span>
      </div>`).join('')
    : `<div class="diagnostics-result"><span class="muted">${pcvEscapeHtml(t('account.diag.empty_label'))}</span><strong>${pcvEscapeHtml(t('account.diag.empty_value'))}</strong><p>${pcvEscapeHtml(t('account.diag.empty_body'))}</p></div>`;

  return `<div class="diagnostics-result diagnostics-list">
    <div class="diagnostics-header">
      <div>
        <span class="muted">${pcvEscapeHtml(t('account.diag.retained'))}</span>
        <strong>${pcvEscapeHtml(page.returned ?? bundles.length)} / ${pcvEscapeHtml(page.count ?? bundles.length)}</strong>
      </div>
      <span class="status-badge neutral">next_offset=${pcvEscapeHtml(nextOffset ?? t('account.diag.next_none'))}</span>
    </div>
    <div class="boundary-chip-row">
      <span>max_bundle_count=${pcvEscapeHtml(retention.max_bundle_count ?? 'unavailable')}</span>
      <span>retention_days=${pcvEscapeHtml(retention.retention_days ?? 'unavailable')}</span>
      <span>limit=${pcvEscapeHtml(page.limit ?? 10)}</span>
    </div>
    <div class="triage-list">${rows}</div>
    <div class="diagnostics-actions">
      <button type="button" data-action="diagnostic-list-next" ${hasNext ? '' : 'disabled aria-disabled="true"'}>${pcvEscapeHtml(t('account.diag.more'))}</button>
      <span class="muted">${pcvEscapeHtml(t('account.diag.readonly'))}</span>
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
  requireRbac('diagnostics.create', t('account.rbac.diag_create'));
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
  requireRbac('diagnostics.read', t('account.rbac.diag_download'));
  if (!getDiagnosticBundleId() && state.diagnosticBundles.length > 0) {
    state.diagnosticBundle = state.diagnosticBundles[0];
  }
  const bundleId = getDiagnosticBundleId();
  if (!bundleId) {
    state.diagnosticBundleError = normalizeError({
      code: 'PCV_DIAGNOSTIC_BUNDLE_NOT_SELECTED',
      message: t('account.diag.required'),
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
      message: t('account.diag.refresh_missing'),
      detail: t('account.diag.refresh_detail')
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
    state.tokenActionMessage = t('account.diag.jwt_cleared');
    await refreshAll();
    render();
  }
}

const ACCOUNT_USERNAME = /^[A-Za-z][A-Za-z0-9._-]{2,31}$/;
const ACCOUNT_ROLES = ['viewer', 'operator', 'admin'];

function accountField(form, name) {
  if (!form || !form.querySelector) return null;
  return form.querySelector('[name="' + name + '"]');
}

function accountFieldValue(form, name) {
  const field = accountField(form, name);
  return field ? String(field.value || '') : '';
}

function accountRule(code, messageKey) {
  return { code: code, messageKey: messageKey };
}

function accountUsernameRule(value) {
  const name = String(value || '').trim();
  if (!ACCOUNT_USERNAME.test(name) || name.toLowerCase() === 'loopback-session') {
    return accountRule('PCV_ACCOUNT_USERNAME_INVALID', 'account.username.invalid');
  }
  return null;
}

function accountRoleRule(value) {
  const role = String(value || '').trim().toLowerCase();
  if (ACCOUNT_ROLES.indexOf(role) < 0) return accountRule('PCV_ACCOUNT_ROLE_INVALID', 'account.role.invalid');
  return null;
}

function accountPasswordRules(form) {
  return [function (value) {
    const password = String(value || '');
    if (password.length < 12 || password.trim() === '') {
      return accountRule('PCV_ACCOUNT_PASSWORD_INVALID', 'account.password.invalid');
    }
    const username = accountFieldValue(form, 'username').trim();
    if (username && password.toLowerCase() === username.toLowerCase()) {
      return accountRule('PCV_ACCOUNT_PASSWORD_INVALID', 'account.password.same_as_username');
    }
    return null;
  }];
}

function accountConfirmRules(form) {
  return [function (value) {
    if (String(value || '') !== accountFieldValue(form, 'password')) {
      return accountRule('PCV_ACCOUNT_PASSWORD_MISMATCH', 'account.password.mismatch');
    }
    return null;
  }];
}

function accountFormValid(form) {
  if (accountUsernameRule(accountFieldValue(form, 'username'))) return false;
  if (accountRoleRule(isAccountBootstrapOpen() ? 'admin' : accountFieldValue(form, 'role'))) return false;
  const passwordRules = accountPasswordRules(form);
  if (passwordRules[0](accountFieldValue(form, 'password'))) return false;
  const confirmRules = accountConfirmRules(form);
  if (confirmRules[0](accountFieldValue(form, 'password_confirm'))) return false;
  return true;
}

function touchAccountFields(form) {
  ['username', 'role', 'password', 'password_confirm'].forEach((name) => {
    const field = accountField(form, name);
    if (!field || typeof field.dispatchEvent !== 'function') return;
    const emitted = typeof Event === 'function' ? new Event('input') : { type: 'input' };
    field.dispatchEvent(emitted);
  });
}

function scrubAccountSecret(error, secret) {
  if (!error || !secret) return error;
  const wipe = (value) => typeof value === 'string' ? value.split(secret).join('') : value;
  return Object.assign({}, error, {
    message: wipe(error.message),
    detail: wipe(error.detail)
  });
}

function clearAccountSecretFields(scope) {
  const root = scope || document;
  if (!root || typeof root.querySelectorAll !== 'function') return;
  Array.prototype.forEach.call(root.querySelectorAll('input[type="password"]'), (input) => {
    if (!input || input.id === 'account-password' || input.id === 'login-pass') return;
    input.value = '';
    if (typeof input.setAttribute === 'function') input.setAttribute('value', '');
  });
}

function accountEnabledText(account) {
  return account?.enabled === false ? t('account.list.disabled') : t('account.list.enabled');
}

function bindAccountsPanel() {
  const panel = document.getElementById('accounts-panel');
  if (!panel || panel._pcvAccountsBound || typeof panel.addEventListener !== 'function') return;
  panel._pcvAccountsBound = true;
  panel.addEventListener('click', (event) => {
    const button = event.target && event.target.closest ? event.target.closest('button[data-action]') : null;
    if (!button || button.disabled) return;
    if (button.dataset.action === 'account-create-open') openAccountCreateModal();
    else if (button.dataset.action === 'account-disable') disableAccountFromButton(button.getAttribute('data-username'));
  });
}

function paintAccountsView() {
  const root = document.getElementById('accounts-panel');
  if (!root || !PCV.uxlib) return;
  PCV.uxlib.clearEl(root);
  bindAccountsPanel();
  if (typeof canManageAccounts === 'function' && !canManageAccounts()) {
    root.appendChild(PCV.uxlib.el('div', { class: 'pcv-account-denied', role: 'status' },
      PCV.uxlib.el('h2', {}, t('account.denied.title')),
      PCV.uxlib.el('p', {}, t('account.denied.body'))));
    return;
  }
  const error = state.accountDirectoryError;
  if (error) {
    const serverText = [error.message, error.detail].filter(Boolean).join(' ');
    const lastAdmin = error.code === 'PCV_ACCOUNT_LAST_ADMIN' ? t('account.error.last_admin') : '';
    root.appendChild(PCV.uxlib.el('div', { class: 'pcv-account-error', role: 'alert' },
      PCV.uxlib.el('strong', {}, String(error.code || '')),
      PCV.uxlib.el('p', {}, [lastAdmin, serverText].filter(Boolean).join(' '))));
  }
  const accounts = asArray(state.accountDirectory).filter((account) => account && account.username);
  const body = PCV.uxlib.el('tbody');
  if (!accounts.length) {
    body.appendChild(PCV.uxlib.el('tr', {}, PCV.uxlib.el('td', { colspan: '4' }, t('account.list.empty'))));
  } else {
    accounts.forEach((account) => {
      const username = String(account.username || '');
      const enabled = account.enabled !== false;
      const disable = PCV.uxlib.el('button', {
        type: 'button',
        class: 'btn',
        'data-action': 'account-disable',
        'data-username': username
      }, t('account.disable.action'));
      disable.disabled = !!state.accountManagePending || !enabled;
      body.appendChild(PCV.uxlib.el('tr', {},
        PCV.uxlib.el('td', {}, username),
        PCV.uxlib.el('td', {}, String(account.role || '-')),
        PCV.uxlib.el('td', {}, accountEnabledText(account)),
        PCV.uxlib.el('td', {}, disable)));
    });
  }
  const create = PCV.uxlib.el('button', { type: 'button', class: 'btn', 'data-action': 'account-create-open' }, t('account.create.open'));
  create.disabled = !!state.accountManagePending;
  root.appendChild(create);
  root.appendChild(PCV.uxlib.el('table', { class: 'pcv-vm-table account-directory-table' },
    PCV.uxlib.el('thead', {}, PCV.uxlib.el('tr', {},
      PCV.uxlib.el('th', {}, t('account.list.username')),
      PCV.uxlib.el('th', {}, t('account.list.role')),
      PCV.uxlib.el('th', {}, t('account.list.state')),
      PCV.uxlib.el('th', {}, ''))),
    body));
}

function openAccountCreateModal() {
  if (!PCV.uxlib || !PCV.components || !PCV.modalCore) return;
  if (typeof canManageAccounts === 'function' && !canManageAccounts()) return;
  const ux = PCV.uxlib;
  const bootstrap = isAccountBootstrapOpen();
  const username = ux.el('input', { name: 'username', type: 'text', autocomplete: 'off', 'aria-label': t('account.create.username') });
  const role = ux.el('select', { name: 'role', 'aria-label': t('account.create.role') },
    ux.el('option', { value: 'viewer' }, 'viewer'),
    ux.el('option', { value: 'operator' }, 'operator'),
    ux.el('option', { value: 'admin' }, 'admin'));
  role.value = bootstrap ? 'admin' : 'viewer';
  if (bootstrap) role.disabled = true;
  const password = ux.el('input', { name: 'password', type: 'password', autocomplete: 'new-password', 'aria-label': t('account.create.password') });
  const confirm = ux.el('input', { name: 'password_confirm', type: 'password', autocomplete: 'new-password', 'aria-label': t('account.create.password_confirm') });
  const submit = ux.el('button', { type: 'submit', class: 'btn' }, t('account.create.submit'));
  const form = ux.el('form', { id: 'account-create-form', class: 'account-create-form', autocomplete: 'off', novalidate: 'novalidate' },
    ux.el('label', {}, t('account.create.username'), username),
    ux.el('label', {}, t('account.create.role'), role),
    bootstrap ? ux.el('p', { class: 'muted' }, t('account.create.bootstrap_note')) : null,
    ux.el('label', {}, t('account.create.password'), password),
    ux.el('label', {}, t('account.create.password_confirm'), confirm),
    submit);
  form._pcvAccountRefresh = PCV.components.bindValidation(form, {
    username: [accountUsernameRule],
    role: [accountRoleRule],
    password: accountPasswordRules(form),
    password_confirm: accountConfirmRules(form)
  }, submit);
  form.addEventListener('submit', (event) => {
    createAccountFromForm(event);
  });
  PCV.modalCore.open([
    ux.el('h2', {}, t('account.create.title')),
    form
  ], { backdropDismiss: true });
}

async function createAccountFromForm(event) {
  if (event && typeof event.preventDefault === 'function') event.preventDefault();
  const form = (event && event.target && event.target.closest && event.target.closest('form')) || document.getElementById('account-create-form');
  if (!form) return;
  touchAccountFields(form);
  if (typeof form._pcvAccountRefresh === 'function' && !form._pcvAccountRefresh()) return;
  if (!accountFormValid(form)) return;
  const username = accountFieldValue(form, 'username').trim();
  const password = accountFieldValue(form, 'password');
  const role = isAccountBootstrapOpen() ? 'admin' : (accountFieldValue(form, 'role').trim().toLowerCase() || 'viewer');
  const accepted = await PCV.components.confirmDialog({
    title: t('account.create.confirm_title'),
    message: t('account.create.confirm_message', { username: username, role: role }),
    confirmLabel: t('account.create.confirm'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: false
  });
  if (!accepted) return;

  state.accountManagePending = true;
  state.accountDirectoryError = null;
  render();
  try {
    await desktopApi.createAccount({ username: username, password: password, role: role });
    if (isAccountBootstrapOpen()) {
      const result = await desktopApi.loginAccount({ username: username, password: password });
      applyAccountSessionPayload(result);
      state.connectionState = 'connected';
    }
    await refreshAll();
  } catch (error) {
    state.accountDirectoryError = scrubAccountSecret(normalizeError(error), password);
  } finally {
    clearAccountSecretFields(form);
    clearAccountSecretFields(document);
    state.accountManagePending = false;
    if (PCV.modalCore && typeof PCV.modalCore.close === 'function') PCV.modalCore.close(true);
    render();
    clearAccountSecretFields(document);
  }
}

async function disableAccountFromButton(username) {
  const name = String(username || '').trim();
  if (!name) return;
  const accepted = await PCV.components.confirmDialog({
    title: t('account.disable.confirm_title'),
    message: t('account.disable.confirm_message', { username: name }),
    confirmLabel: t('account.disable.confirm'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: true
  });
  if (!accepted) return;

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
  requireRbac('console.view', t('account.rbac.console'));
  if (!vmId) {
    throw normalizeError({
      code: 'PCV_CONSOLE_VM_REQUIRED',
      message: t('account.err.console_vm'),
      detail: t('account.err.console_vm_detail')
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
  state.tokenActionMessage = t('account.token.cleared');
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
  state.tokenActionMessage = t('account.token.cleared');
  state.error = null;
  try {
    window.localStorage.removeItem(JOB_HISTORY_KEY);
  } catch (_) {
    // Browser storage can be unavailable; the visible session still clears.
  }
  refreshAll();
}

function navigateToView(view) {
  if (typeof navigateTo === 'function') {
    navigateTo(view);
    return;
  }
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
window.openAccountCreateModal = openAccountCreateModal;
window.paintAccountsView = paintAccountsView;
window.openSelectedConsole = openSelectedConsole;
window.clearBrowserToken = clearBrowserToken;
window.clearBrowserState = clearBrowserState;
window.navigateToView = navigateToView;
window.handleShellCommand = handleShellCommand;
PCV.accounts = Object.assign(PCV.accounts || {}, { getDiagnosticBundleId, renderDiagnosticBundleList, persistDiagnosticBundleDownload, createDiagnosticBundleFromPanel, downloadLatestDiagnosticBundle, isLoopbackHostname, pcvEnsureLoopbackSession, loginAccountFromForm, refreshAccountSession, logoutAccount, createAccountFromForm, disableAccountFromButton, openSelectedConsole, clearBrowserToken, clearBrowserState, navigateToView, handleShellCommand });
})(window.PCV);

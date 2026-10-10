// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): dashboard panels, monitoring, incident command and the legacy render dispatcher (legacy render-ops, render-panels, render-monitoring, render-shell).
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-ops.ts ---
function getDiagnosticActionPolicy(error = state.diagnosticBundleError) {
  const status = Number(error?.status || 0);
  const code = String(error?.code || '');
  const message = String(error?.message || '').toLowerCase();
  const pending = Boolean(state.pendingDiagnosticAction);
  const bundleId = getDiagnosticBundleId();
  const guideByStatus: Record<number, string> = {
    401: '401 auth required: enter a browser token and retry.',
    403: '403 auth forbidden: verify the token source before retrying.',
    404: '404 PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED: installed listener does not expose the diagnostic bundle API.',
    500: '500 server failure: keep the error details for support triage.'
  };
  const timedOut = status === 408 || status === 504 || code === 'PCV_ROUTE_TIMEOUT' || message.includes('timeout');
  const unsupported = status === 404 || code === 'PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED';
  const auth = status === 401 || status === 403 || isAuthError(error);
  const retryable = Boolean(error?.retryable || timedOut || status >= 500 || auth);
  const guide = timedOut
    ? 'timeout: retry after the listener is responsive or after the Retry-After hint.'
    : guideByStatus[status] || (code ? `${code}: review problem-details and retry when safe.` : 'Ready for authenticated create/download.');

  return {
    createDisabled: pending || unsupported,
    downloadDisabled: pending || !bundleId || unsupported,
    retryVisible: Boolean(error && retryable && !unsupported),
    statusLabel: pending || (unsupported ? 'API unsupported' : auth ? 'Auth required' : error ? 'Problem details' : 'API ready'),
    guide
  };
}

function buildPartialRefreshError(failures) {
  return normalizeError({
    code: 'PCV_PARTIAL_REFRESH_DEGRADED',
    message: 'Some Desktop Node API panels failed while the console stayed online.',
    detail: failures.map(formatErrorSummary).join(' | '),
    retryable: failures.some((failure) => failure.retryable)
  });
}

function collectRefreshFailures(stepFailures) {
  const failures = [];
  for (const failure of asArray(stepFailures)) {
    const normalized = failure?.normalized ? failure : normalizeError(failure);
    normalized.operation = normalized.operation || failure?.label || 'web.refresh';
    failures.push(normalized);
  }
  for (const localFailure of [state.summaryError, state.networkError, state.activityError]) {
    if (localFailure) failures.push(localFailure);
  }

  const seen = new Set();
  return failures.filter((failure) => {
    const key = `${failure.operation}:${failure.code}:${failure.detail}`;
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

function getPriorityItems() {
  const items = [];
  const seen = new Set();
  const pushUnique = (item, key = `${item.label}:${item.detail}`) => {
    const normalizedKey = String(key).toLowerCase();
    if (seen.has(normalizedKey)) return;
    seen.add(normalizedKey);
    items.push(item);
  };
  const hostReadiness = String(getHostReadinessLabel()).toLowerCase();
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  if (state.summaryError) {
    pushUnique({ tone: 'warn', label: 'Ops summary unavailable', detail: formatErrorSummary(state.summaryError) }, state.summaryError.code);
  }
  for (const error of asArray(state.opsSummary?.errors)) {
    const issue = normalizeSummaryIssue(error);
    pushUnique({ tone: 'error', label: 'Ops summary degraded', detail: `${issue.code}: ${issue.message}` }, issue.code);
  }
  for (const signal of asArray(state.opsSummary?.signals)) {
    const key = summarySignalKey(signal);
    const tone = summarySignalTone(signal);
    if (!key && !tone) continue;
    if (tone === 'ok' || tone === 'ready' || tone === 'healthy') continue;
    if (key === 'summary-errors' && asArray(state.opsSummary?.errors).length > 0) continue;
    const issue = normalizeSummaryIssue(signal, key || 'PCV_OPS_SUMMARY_SIGNAL');
    const priorityTone = tone === 'error' || tone === 'critical' || tone === 'fail' || tone === 'failed' ? 'error' : 'warn';
    pushUnique({ tone: priorityTone, label: issue.message || 'Ops summary signal', detail: issue.detail || issue.code }, issue.code);
  }
  if (hostReadiness.includes('need') || hostReadiness.includes('fail') || hostReadiness.includes('error')) {
    pushUnique({ tone: 'error', label: 'Host readiness needs attention', detail: 'Check Hyper-V support, admin context, VMMS, and Default Switch state.' }, 'host-readiness');
  }
  if (jobCounts.failed > 0) {
    pushUnique({ tone: 'error', label: 'Failed jobs', detail: `${jobCounts.failed} failed job(s) need review.` }, 'failed-jobs');
  }
  if (vmCounts.checkpoint_warnings > 0) {
    pushUnique({ tone: 'warn', label: 'Checkpoint warnings', detail: `${vmCounts.checkpoint_warnings} VM checkpoint warning(s) need review.` }, 'checkpoint-warnings');
  }
  if (String(getRuntimeExposure()).toLowerCase().includes('lan')) {
    pushUnique({ tone: 'warn', label: 'LAN exposure', detail: 'Confirm explicit LAN approval and token source proof.' }, 'lan-exposure');
  }
  return items;
}

function renderMetrics() {
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  // getSummaryVmCounts() falls back to vms.length, so an unloaded inventory is
  // indistinguishable from a genuine zero. Job counts keep their number: they
  // also draw on browser-tracked jobs, which remain real without the server.
  const vmsLoaded = hasRefreshedOperation('vm.list');
  els.metricGrid.innerHTML = [
    ['Host', getHostReadinessLabel()],
    ['VMs', vmsLoaded ? vmCounts.total : '—'],
    ['Running', vmsLoaded ? vmCounts.running : '—'],
    ['Active Jobs', jobCounts.active]
  ].map(([label, value]) => `<div class="metric"><span class="muted">${pcvEscapeHtml(label)}</span><strong>${pcvEscapeHtml(value)}</strong></div>`).join('');
}

function renderHost() {
  const host = state.host || {};
  const entries = Object.entries(host).slice(0, 16);
  if (entries.length === 0) {
    els.hostDetails.innerHTML = '<p class="muted">Host status has not been loaded.</p>';
    return;
  }
  els.hostDetails.innerHTML = entries
    .map(([key, value]) => `<div class="kv"><span>${pcvEscapeHtml(key)}</span><strong>${pcvEscapeHtml(typeof value === 'object' ? JSON.stringify(value) : value)}</strong></div>`)
    .join('');
}

function renderOpsSummaryCard(card) {
  const [arrayLabel, arrayValue, arrayDetail] = Array.isArray(card) ? card : [];
  const label = card.label ?? arrayLabel;
  const valueHtml = card.valueHtml ?? pcvEscapeHtml(card.value ?? arrayValue);
  const detail = card.detail ?? arrayDetail;
  return `<div class="ops-summary-card"><span class="muted">${pcvEscapeHtml(label)}</span><strong>${valueHtml}</strong><p>${pcvEscapeHtml(detail)}</p></div>`;
}

function renderOpsCockpit() {
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  const evidence = getBatchEvidence();
  const vmsLoaded = hasRefreshedOperation('vm.list');
  const cards = [
    ['Host readiness', getHostReadinessLabel(), 'Hyper-V support, admin context, and service/API availability.'],
    ['VMs total/running', vmsLoaded ? `${vmCounts.total} / ${vmCounts.running}` : '—', 'Inventory count from ops summary with local inventory fallback.'],
    ['Checkpoint warnings', vmsLoaded ? vmCounts.checkpoint_warnings : '—', 'VMs with checkpoint warning signals from ops summary.'],
    ['Jobs active/failed', `${jobCounts.active} / ${jobCounts.failed}`, 'Current server and browser-visible job activity.'],
    ['Exposure / token', `${getRuntimeExposure()} / ${getTokenPolicyLabel()}`, 'Loopback-first listener policy and token storage posture.'],
    {
      label: 'Latest evidence',
      valueHtml: renderEvidenceStatusBadge(evidence),
      detail: evidenceDashboardDetail(evidence)
    },
    {
      label: 'Current evidence',
      valueHtml: renderCurrentEvidenceStatusBadge(),
      detail: currentEvidenceDashboardDetail()
    }
  ];
  els.opsSummaryPanel.innerHTML = `
    <div class="ops-summary-grid">
      ${cards.map(renderOpsSummaryCard).join('')}
    </div>`;

  const priorityItems = getPriorityItems();
  if (priorityItems.length === 0) {
    els.priorityPanel.innerHTML = '<div class="priority-empty">No high-priority warnings.</div>';
    return;
  }
  els.priorityPanel.innerHTML = priorityItems.map((item) => `
    <div class="priority-item priority-${pcvEscapeHtml(item.tone)}">
      <strong>${pcvEscapeHtml(item.label)}</strong>
      <span>${pcvEscapeHtml(item.detail)}</span>
    </div>`).join('');
}

// --- legacy render-panels.ts ---
function renderRuntimeApiRegistryBridge() {
  const bridge = getRuntimeApiRegistryBridge();
  const contractKey = bridge.contract_key || bridge.contractKey;
  const source = bridge.handler_registry_source || bridge.handlerRegistrySource || bridge.source;
  const anchor = bridge.documentation_anchor || bridge.documentationAnchor || bridge.anchor;
  const routeKeys = asArray(bridge.route_keys || bridge.routeKeys)
    .map((route) => String(route || '').trim())
    .filter(Boolean);
  const routeCount = routeKeys.length;
  const hasBridge = Boolean(contractKey);
  const routeLabel = routeCount > 0 ? `${routeCount} routes` : 'routes not reported';
  const routeDetailHtml = routeKeys.length
    ? `<ul class="diagnostics-route-list">${routeKeys.map((route) => `<li>${pcvEscapeHtml(route)}</li>`).join('')}</ul>`
    : '<p>Route detail not reported by ops summary.</p>';

  return `<div class="diagnostics-result runtime-api-registry-bridge">
    <span class="muted">Runtime/API registry bridge</span>
    <strong>${pcvEscapeHtml(contractKey || 'not reported')}</strong>
    <p>${pcvEscapeHtml([source || 'source not reported', routeLabel].join(' / '))}</p>
    <p>${pcvEscapeHtml(anchor || 'documentation anchor not reported')}</p>
    ${routeDetailHtml}
    <div class="boundary-chip-row">
      <span>${hasBridge ? 'ops summary direct expose' : 'ops summary bridge absent'}</span>
      <span>route detail metadata only</span>
    </div>
  </div>`;
}

function renderHostOpsLifecycleBucketTable() {
  const descriptor = getCurrentEvidenceHostOps();
  const buckets = asArray(descriptor.buckets)
    .map((bucket) => asObject(bucket))
    .filter((bucket) => bucket.bucket_key || bucket.bucketKey);
  const contractKey = descriptor.lifecycle_bucket_contract_key || descriptor.lifecycleBucketContractKey || 'bucket contract not reported';
  const mutationPerformed = descriptor.host_mutation_performed ?? descriptor.hostMutationPerformed;
  const mutationLabel = mutationPerformed === true
    ? 'Host mutation: reported by evidence'
    : 'Host mutation: not performed by diagnostics view';

  if (!buckets.length) {
    return `<div class="diagnostics-result hostops-lifecycle-buckets">
      <span class="muted">Host Ops lifecycle buckets</span>
      <strong>${pcvEscapeHtml(descriptor.status || 'not reported')}</strong>
      <p>${pcvEscapeHtml(contractKey)}</p>
      <div class="boundary-chip-row">
        <span>${pcvEscapeHtml(mutationLabel)}</span>
        <span>ops summary metadata only</span>
      </div>
    </div>`;
  }

  const rows = buckets.map((bucket) => {
    const operations = asArray(bucket.operations).map((operation) => String(operation || '').trim()).filter(Boolean);
    return `<tr>
      <td>${pcvEscapeHtml(bucket.bucket_key || bucket.bucketKey)}</td>
      <td>${pcvEscapeHtml(bucket.owner || '-')}</td>
      <td>${pcvEscapeHtml(bucket.mutation_boundary || bucket.mutationBoundary || '-')}</td>
      <td>${pcvEscapeHtml(operations.join(', ') || '-')}</td>
    </tr>`;
  }).join('');

  return `<div class="diagnostics-result hostops-lifecycle-buckets">
    <span class="muted">Host Ops lifecycle buckets</span>
    <strong>${pcvEscapeHtml(descriptor.contract_key || descriptor.contractKey || 'host-ops-lifecycle-descriptor-bridge-v1')}</strong>
    <p>${pcvEscapeHtml(contractKey)}</p>
    <div class="evidence-table-wrap diagnostics-hostops-table">
      <table class="evidence-table">
        <thead><tr><th>Bucket</th><th>Owner</th><th>Mutation boundary</th><th>Operations</th></tr></thead>
        <tbody>${rows}</tbody>
      </table>
    </div>
    <div class="boundary-chip-row">
      <span>${pcvEscapeHtml(mutationLabel)}</span>
      <span>service-action/Event Log/firewall/trust-store/Credential Manager/data-root separated</span>
    </div>
  </div>`;
}

function renderDiagnosticsBundle() {
  if (!els.diagnosticsPanel) return;
  const bundle = state.diagnosticBundle || {};
  const download = state.diagnosticBundleDownload || {};
  const bundleId = getDiagnosticBundleId(bundle);
  const pending = state.pendingDiagnosticAction;
  const policy = getDiagnosticActionPolicy();
  const canCreate = rbacAllows('diagnostics.create');
  const canDownload = rbacAllows('diagnostics.read');
  const createDisabled = policy.createDisabled || !canCreate ? 'disabled aria-disabled="true"' : '';
  const downloadDisabled = policy.downloadDisabled || !canDownload ? 'disabled aria-disabled="true"' : '';
  const facts = [
    ['Mode', 'API action'],
    ['Mutation', 'no host mutation'],
    ['Output root', DIAGNOSTIC_BUNDLE_ROOT],
    ['Route', DESKTOP_NODE_API_ROUTES.diagnosticBundles],
    ['List route', DESKTOP_NODE_API_ROUTES.diagnosticBundlesPage(10, 0)],
    ['CollectDiagnostics', 'server-side compatible archive'],
    ['Redaction', 'token values and Authorization headers redacted']
  ];
  const statusHtml = state.diagnosticBundleError
    ? `<div class="diagnostics-result error"><span class="muted">Last action</span><strong>${pcvEscapeHtml(state.diagnosticBundleError.code)}</strong><p>${pcvEscapeHtml(state.diagnosticBundleError.message)} ${pcvEscapeHtml(state.diagnosticBundleError.detail)}</p><p>${pcvEscapeHtml(policy.guide)}</p></div>`
    : bundleId
      ? `<div class="diagnostics-result"><span class="muted">Latest bundle</span><strong>${pcvEscapeHtml(bundleId)}</strong><p>${pcvEscapeHtml([
          bundle.created_at,
          bundle.download_status,
          bundle.redaction_status,
          bundle.retention_status
        ].filter(Boolean).join(' / ') || 'ready for authenticated download')}</p></div>`
      : `<div class="diagnostics-result"><span class="muted">Latest bundle</span><strong>Not created in this browser session</strong><p>Create uses POST ${pcvEscapeHtml(DESKTOP_NODE_API_ROUTES.diagnosticBundles)} and keeps the archive server-side.</p></div>`;
  const downloadHtml = download.bundle_id
    ? `<div class="diagnostics-result"><span class="muted">Last download</span><strong>${pcvEscapeHtml(download.file_name || download.bundle_id)}</strong><p>${pcvEscapeHtml(download.content_type || 'application/vnd.purecvisor.diagnostic-bundle+json')} / ${pcvEscapeHtml(download.size_bytes ?? 0)} bytes</p></div>`
    : '';

  els.diagnosticsPanel.innerHTML = `<div class="diagnostics-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Support artifact</span>
        <strong>Diagnostic Bundle</strong>
      </div>
      <span class="status-badge ${pending || state.diagnosticBundleError ? 'warn' : 'ok'}">${pcvEscapeHtml(pending || policy.statusLabel)}</span>
    </div>
    <div class="diagnostics-grid">
      ${facts.map(([label, value]) => `<div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(label)}</span><strong>${pcvEscapeHtml(value)}</strong></div>`).join('')}
    </div>
    <div class="diagnostics-actions">
      <button type="button" data-action="diagnostic-create" ${createDisabled}>Create bundle</button>
      <button type="button" data-action="diagnostic-download" data-bundle-id="${pcvEscapeHtml(bundleId)}" ${downloadDisabled}>Download latest</button>
      ${policy.retryVisible ? '<button type="button" data-action="diagnostic-retry">Retry action</button>' : ''}
      <span class="muted">Authenticated create/download only; command strings and token values are not rendered.</span>
    </div>
    ${statusHtml}
    ${renderRuntimeApiRegistryBridge()}
    ${renderHostOpsLifecycleBucketTable()}
    ${renderDiagnosticBundleList()}
    ${downloadHtml}
    <div class="boundary-chip-row">
      <span>token file content excluded</span>
      <span>protected token blobs omitted</span>
      <span>public signing not claimed</span>
      <span>external publication not claimed</span>
    </div>
  </div>`;
}

function renderTokenRotation() {
  if (!els.tokenRotationPanel) return;
  const policy = state.runtimePolicy || {};
  const storage = readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage']) || getTokenPolicyLabel();
  const exposure = readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind']) || getRuntimeExposure();
  const browserToken = state.apiToken.trim() ? 'browser token present' : 'browser token empty';
  const facts = [
    ['Mode', 'rotation handoff'],
    ['Service token', 'no service token mutation'],
    ['Protected token file', TOKEN_PROTECTED_FILE],
    ['Browser token', browserToken],
    ['Token-required route status', tokenRequiredRouteStatus()],
    ['Storage', formatPolicyValue(storage)],
    ['Exposure', formatPolicyValue(exposure)]
  ];

  els.tokenRotationPanel.innerHTML = `<div class="token-rotation-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Auth lifecycle</span>
        <strong>Token Rotation</strong>
      </div>
      <span class="status-badge warn">operator handoff</span>
    </div>
    <div class="diagnostics-grid">
      ${facts.map(([label, value]) => `<div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(label)}</span><strong>${pcvEscapeHtml(value)}</strong></div>`).join('')}
    </div>
    <div class="token-rotation-actions">
      <button type="button" data-action="clear-browser-token">Clear browser token</button>
      <span class="muted">Token values and Authorization headers are not rendered.</span>
    </div>
    ${state.tokenActionMessage ? `<div class="diagnostics-result"><span class="muted">Last browser action</span><strong>${pcvEscapeHtml(state.tokenActionMessage)}</strong></div>` : ''}
    <div class="boundary-chip-row">
      <span>revoke browser token only</span>
      <span>service token replacement operator-owned</span>
      <span>no host mutation</span>
    </div>
  </div>`;
}

function renderAccountSession() {
  if (!els.accountSessionPanel) return;
  const session = state.authSession || {};
  const role = getAccountRoleLabel();
  const permissions = getAccountPermissions();
  const signedIn = Boolean(state.authAccessToken && session.username);
  const status = state.authPending ? 'pending' : signedIn ? role : 'not signed in';
  const errorHtml = state.authError
    ? `<div class="diagnostics-result error"><span class="muted">Account auth</span><strong>${pcvEscapeHtml(state.authError.code)}</strong><p>${pcvEscapeHtml(state.authError.message)} ${pcvEscapeHtml(state.authError.detail)}</p></div>`
    : '';
  const injectLoginForm = !(needsAuthGate() && state.activeView !== 'troubleshooting');
  const loginFormHtml = injectLoginForm
    ? `<form id="account-login-form" class="account-login-form" autocomplete="off">
      <label>Username<input id="account-username" name="username" type="text" autocomplete="username" aria-label="account username"></label>
      <label>Password<input id="account-password" name="password" type="password" autocomplete="current-password" aria-label="account password"></label>
      <button type="submit"${state.authPending ? ' disabled' : ''}>Login</button>
    </form>`
    : '';

  els.accountSessionPanel.innerHTML = `<div class="token-rotation-card account-session-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Account</span>
        <strong>RBAC Session</strong>
      </div>
      <span class="status-badge ${signedIn ? 'ok' : 'warn'}">${pcvEscapeHtml(status)}</span>
    </div>
    ${loginFormHtml}
    <div class="diagnostics-grid">
      <div class="diagnostics-fact"><span class="muted">User</span><strong>${pcvEscapeHtml(session.username || '-')}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Role</span><strong>${pcvEscapeHtml(role)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">JWT access</span><strong>${pcvEscapeHtml(state.authAccessToken ? 'present' : 'empty')}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Refresh</span><strong>${pcvEscapeHtml(state.authRefreshToken ? 'present' : 'empty')}</strong></div>
    </div>
    <div class="token-rotation-actions">
      <button type="button" data-action="account-refresh"${state.authRefreshToken ? '' : ' disabled aria-disabled="true"'}>Refresh JWT</button>
      <button type="button" data-action="account-logout"${signedIn ? '' : ' disabled aria-disabled="true"'}>Logout account</button>
      <span class="muted">JWT and password values are kept out of the DOM after submit.</span>
    </div>
    <div class="boundary-chip-row">
      <span>permissions: ${pcvEscapeHtml(permissions.length ? permissions.join(', ') : 'none')}</span>
      <span>service bearer fallback preserved</span>
      <span>RBAC gates destructive actions</span>
    </div>
    ${errorHtml}
  </div>
  ${renderAccountDirectory()}`;
}

function renderAccountDirectory() {
  const bootstrap = isAccountBootstrapOpen();
  const canManage = canManageAccounts();
  const pending = state.accountManagePending || state.authPending;
  const disabledAttr = pending ? ' disabled' : '';
  const directoryError = state.accountDirectoryError
    ? `<div class="diagnostics-result error"><span class="muted">Accounts</span><strong>${pcvEscapeHtml(state.accountDirectoryError.code)}</strong><p>${pcvEscapeHtml(state.accountDirectoryError.message)} ${pcvEscapeHtml(state.accountDirectoryError.detail)}</p></div>`
    : '';

  if (bootstrap) {
    return `<div class="token-rotation-card account-directory-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Accounts</span>
        <strong>Create first admin</strong>
      </div>
      <span class="status-badge warn">no-default-account</span>
    </div>
    <form id="account-create-form" class="account-login-form" autocomplete="off">
      <label>Username<input name="username" type="text" autocomplete="off" aria-label="new account username"${disabledAttr}></label>
      <label>Password<input name="password" type="password" autocomplete="new-password" aria-label="new account password"${disabledAttr}></label>
      <label>Display name<input name="display_name" type="text" autocomplete="off" aria-label="new account display name"${disabledAttr}></label>
      <input type="hidden" name="role" value="admin">
      <button type="submit"${disabledAttr}>Create first admin</button>
    </form>
    <div class="boundary-chip-row">
      <span>loopback bootstrap only</span>
      <span>password stays out of the DOM after submit</span>
      <span>no default account</span>
    </div>
    ${directoryError}
  </div>`;
  }

  if (!canManage) {
    return '';
  }

  const accounts = asArray(state.accountDirectory);
  const rows = accounts.length
    ? accounts.map((account) => {
        const username = String(account?.username || '');
        const enabled = account?.enabled !== false;
        return `<tr>
        <td>${pcvEscapeHtml(username)}</td>
        <td>${pcvEscapeHtml(account?.role || '-')}</td>
        <td>${pcvEscapeHtml(enabled ? 'enabled' : 'disabled')}</td>
        <td><button type="button" class="danger-button" data-action="account-disable" data-username="${pcvEscapeHtml(username)}"${pending || !enabled ? ' disabled' : ''}>Disable</button></td>
      </tr>`;
      }).join('')
    : '<tr><td colspan="4">No accounts listed.</td></tr>';

  return `<div class="token-rotation-card account-directory-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Accounts</span>
        <strong>Create / disable</strong>
      </div>
      <span class="status-badge ok">${pcvEscapeHtml(state.accountDirectory?.bootstrap_state || 'accounts-configured')}</span>
    </div>
    <table class="data-table account-directory-table">
      <thead><tr><th>Username</th><th>Role</th><th>State</th><th></th></tr></thead>
      <tbody>${rows}</tbody>
    </table>
    <form id="account-create-form" class="account-login-form" autocomplete="off">
      <label>Username<input name="username" type="text" autocomplete="off" aria-label="new account username"${disabledAttr}></label>
      <label>Password<input name="password" type="password" autocomplete="new-password" aria-label="new account password"${disabledAttr}></label>
      <label>Role<select name="role" aria-label="new account role"${disabledAttr}>
        <option value="admin">admin</option>
        <option value="operator">operator</option>
        <option value="viewer">viewer</option>
      </select></label>
      <label>Display name<input name="display_name" type="text" autocomplete="off" aria-label="new account display name"${disabledAttr}></label>
      <button type="submit"${disabledAttr}>Create account</button>
    </form>
    <div class="boundary-chip-row">
      <span>account.manage required</span>
      <span>last enabled admin cannot be disabled</span>
      <span>password stays out of the DOM after submit</span>
    </div>
    ${directoryError}
  </div>`;
}

// --- legacy render-monitoring.ts ---
function readNested(value, path) {
  return path.reduce((current, key) => current && typeof current === 'object' ? current[key] : undefined, value);
}

function formatPolicyValue(value) {
  if (value === true) return 'enabled';
  if (value === false) return 'disabled';
  if (value === null || value === undefined || value === '') return '-';
  return value;
}

function countJobsByStatus(statuses) {
  const wanted = new Set(statuses.map((status) => String(status).toLowerCase()));
  return buildActivityRows().filter(({ job }) => wanted.has(String(job?.status || '').toLowerCase())).length;
}

function getVmCheckpointCount(vm) {
  const raw = vm?.checkpoints?.count ?? vm?.checkpoints_count ?? 0;
  const parsed = Number(raw);
  return Number.isFinite(parsed) ? parsed : 0;
}

function countVmCheckpointWarnings() {
  return asArray(state.vms).filter((vm) => getVmCheckpointCount(vm) >= 10).length;
}

function countSelectedOldCheckpointWarnings() {
  const cutoff = Date.now() - (14 * 24 * 60 * 60 * 1000);
  return asArray(state.selectedVmCheckpoints).filter((checkpoint) => {
    const stamp = Date.parse(checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created || '');
    return Number.isFinite(stamp) && stamp < cutoff;
  }).length;
}

function buildMonitoringSignals() {
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const summaryPolicy = state.opsSummary?.runtime_policy || state.opsSummary?.runtimePolicy || {};
  const vmmsRunning = readNested(host, ['hyperv', 'vmms_running']);
  const tokenStorage = readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage']) || 'unknown';
  const exposure = readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind']) || 'loopback';
  const hardening = readNested(policy, ['service', 'hardening']) || readNested(policy, ['hardening']) || readNested(summaryPolicy, ['service', 'hardening']) || readNested(summaryPolicy, ['hardening']) || {};
  const routeTimeout = readNested(hardening, ['route_timeout_seconds']) || readNested(policy, ['route_timeout_seconds']) || readNested(summaryPolicy, ['route_timeout_seconds']) || '-';
  const requestLimit = readNested(hardening, ['request_limit_per_minute']) || readNested(hardening, ['request_limit']) || readNested(policy, ['request_limit_per_minute']) || '-';
  const burstLimit = readNested(hardening, ['request_burst_limit']) || readNested(hardening, ['burst_limit']) || readNested(policy, ['request_burst_limit']) || '-';
  const retryAfter = readNested(hardening, ['retry_after_seconds']) || readNested(policy, ['retry_after_seconds']) || '-';
  const activeJobs = countJobsByStatus(['queued', 'running']);
  const failedJobs = countJobsByStatus(['failed']);
  const checkpointWarnings = countVmCheckpointWarnings();
  const oldCheckpointWarnings = countSelectedOldCheckpointWarnings();

  return [
    { key: 'service-api', label: 'Service/API', value: state.connectionState === 'connected' ? 'Connected' : 'Not connected', tone: state.connectionState === 'connected' ? 'ok' : 'warn' },
    { key: 'vmms', label: 'VMMS', value: formatPolicyValue(vmmsRunning), tone: vmmsRunning === true ? 'ok' : 'warn' },
    { key: 'active-jobs', label: 'Active jobs', value: activeJobs, tone: activeJobs > 0 ? 'warn' : 'ok' },
    { key: 'failed-jobs', label: 'Failed jobs', value: failedJobs, tone: failedJobs > 0 ? 'error' : 'ok' },
    { key: 'checkpoint-warning', label: 'Checkpoint warnings', value: checkpointWarnings + oldCheckpointWarnings, tone: checkpointWarnings + oldCheckpointWarnings > 0 ? 'warn' : 'ok' },
    { key: 'token-policy', label: 'Token policy', value: tokenStorage, tone: tokenStorage === 'none' ? 'warn' : 'ok' },
    { key: 'lan-exposure', label: 'LAN exposure', value: exposure, tone: String(exposure).toLowerCase().includes('lan') ? 'warn' : 'ok' },
    { key: 'route-timeout', label: 'Route timeout', value: routeTimeout === '-' ? '-' : `${routeTimeout}s`, tone: routeTimeout === '-' ? 'warn' : 'ok' },
    { key: 'request-limit', label: 'Request limit', value: requestLimit === '-' ? '-' : `${requestLimit}/min`, tone: requestLimit === '-' ? 'warn' : 'ok' },
    { key: 'burst-limit', label: 'Burst limit', value: burstLimit, tone: burstLimit === '-' ? 'warn' : 'ok' },
    { key: 'retry-after', label: 'Retry-After', value: retryAfter === '-' ? '-' : `${retryAfter}s`, tone: retryAfter === '-' ? 'warn' : 'ok' }
  ];
}

function evidenceStatusMessage(status) {
  return {
    not_configured: 'Batch evidence root is not configured.',
    missing: 'Configured evidence root has no readable batch summary.',
    degraded: 'Latest batch supervisor evidence is partial or malformed.',
    unavailable: 'Batch evidence summary could not be read.',
    available: 'Latest batch supervisor evidence is loaded.'
  }[status] || 'Batch evidence summary could not be read.';
}

function renderEvidenceDashboard() {
  if (!els.evidencePanel) return;
  const evidence = getBatchEvidence();
  const currentEvidence = getCurrentEvidenceRollup();
  const status = normalizeEvidenceStatus(evidence);
  const errors = asArray(evidence?.errors);

  if (!evidence || evidence.configured === false || status === 'not_configured') {
    els.evidencePanel.innerHTML = `
      <div class="evidence-empty">
        ${renderEvidenceStatusBadge(evidence)}
        ${renderCurrentEvidenceSummary(currentEvidence)}
        <p class="muted">${pcvEscapeHtml(evidenceStatusMessage('not_configured'))}</p>
      </div>`;
    return;
  }

  const latest = asObject(evidence.latest);
  const release = asObject(latest.release);
  const gpu = asObject(latest.gpu_snapshots);
  const route = asObject(latest.route_msi_hyperv);
  const os = asObject(latest.os_mutation);
  const host = asObject(latest.host_final_state);
  const steps = asArray(latest.steps);

  const stepRows = steps.length
    ? steps.map((step) => {
        const stepStatus = step?.ok === true ? 'succeeded' : step?.ok === false ? 'failed' : 'unknown';
        return `<tr>
        <td>${pcvEscapeHtml(evidenceValue(step?.step_id, 'step'))}</td>
        <td>${stateBadge(stepStatus)}</td>
        <td>${pcvEscapeHtml(evidenceValue(step?.attempt_count, '0'))}</td>
        <td>${pcvEscapeHtml(evidenceValue(step?.retry_count, '0'))}</td>
        <td>${pcvEscapeHtml(step?.timed_out === true ? 'true' : 'false')}</td>
      </tr>`;
      }).join('')
    : '<tr><td colspan="5" class="muted">No step evidence is available.</td></tr>';

  const errorHtml = errors.length
    ? `<div class="activity-warning">${errors.map((error) => {
        const issue = normalizeSummaryIssue(error, 'PCV_BATCH_EVIDENCE');
        const detail = issue.detail && issue.detail !== issue.message
          ? ` <span class="muted">${pcvEscapeHtml(issue.detail)}</span>`
          : '';
        return `<strong>${pcvEscapeHtml(issue.code)}</strong> ${pcvEscapeHtml(issue.message)}${detail}`;
      }).join('<br>')}</div>`
    : '';

  els.evidencePanel.innerHTML = `
    ${errorHtml}
    <div class="evidence-header">
      <div>
        <span class="muted">Batch</span>
        <strong>${pcvEscapeHtml(evidenceValue(latest.batch_id, evidenceStatusLabel(status)))}</strong>
        <p class="muted">${pcvEscapeHtml(evidenceStatusMessage(status))}</p>
      </div>
      ${renderEvidenceStatusBadge(evidence)}
    </div>
    <div class="evidence-grid">
      <div class="evidence-metric"><span class="muted">Version</span><strong>${pcvEscapeHtml(evidenceValue(release.version))}</strong></div>
      <div class="evidence-metric"><span class="muted">Signing</span><strong>${pcvEscapeHtml(evidenceValue(release.signing_mode))}</strong></div>
      <div class="evidence-metric"><span class="muted">GPU snapshots</span><strong>${pcvEscapeHtml(evidenceValue(gpu.count, '0'))}</strong></div>
      <div class="evidence-metric"><span class="muted">GPU peak MiB</span><strong>${pcvEscapeHtml(evidenceValue(gpu.peak_adapter_mib))}</strong></div>
      <div class="evidence-metric"><span class="muted">Service</span><strong>${pcvEscapeHtml(evidenceValue(host.service_state))}</strong></div>
      <div class="evidence-metric"><span class="muted">Route/MSI</span><strong>${pcvEscapeHtml(evidenceBooleanLabel(route.ok))}</strong></div>
      <div class="evidence-metric"><span class="muted">OS gate</span><strong>${pcvEscapeHtml(evidenceBooleanLabel(os.ok))}</strong></div>
      <div class="evidence-metric"><span class="muted">Firewall final</span><strong>${pcvEscapeHtml(evidenceValue(host.firewall_rule_count ?? os.firewall_rule_count, '0'))}</strong></div>
    </div>
    <div class="evidence-boundary">
      <span>Public signing: ${pcvEscapeHtml(evidenceValue(release.public_trusted_signing, 'excluded'))}</span>
      <span>External publication: ${pcvEscapeHtml(evidenceValue(release.external_stable_publication, 'not-claimed'))}</span>
    </div>
    ${renderCurrentEvidenceSummary(currentEvidence)}
    <div class="evidence-table-wrap">
      <table class="evidence-table">
        <thead><tr><th>Step</th><th>Status</th><th>Attempts</th><th>Retries</th><th>Timed out</th></tr></thead>
        <tbody>${stepRows}</tbody>
      </table>
    </div>`;
}

function renderCurrentEvidenceSummary(rollup = getCurrentEvidenceRollup()) {
  if (!rollup) {
    return '';
  }

  const publicBoundary = getCurrentEvidencePublicBoundary(rollup);
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const packagePair = getCurrentEvidencePackagePair(rollup);
  const nextPackagePair = getCurrentEvidenceNextPackagePair(rollup);
  const hostOps = getCurrentEvidenceHostOps(rollup);
  return `
    <div class="evidence-header current-evidence-rollup">
      <div>
        <span class="muted">Current evidence</span>
        <strong>${pcvEscapeHtml(rollup.contract_key || 'runtime-api-current-evidence-rollup-v1')}</strong>
        <p class="muted">${pcvEscapeHtml(currentEvidenceDashboardDetail())}</p>
      </div>
      ${renderCurrentEvidenceStatusBadge()}
    </div>
    <div class="evidence-grid current-evidence-grid">
      <div class="evidence-metric"><span class="muted">Public boundary</span><strong>${pcvEscapeHtml(evidenceValue(publicBoundary.run_id || publicBoundary.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Public boundary head</span><strong>${pcvEscapeHtml(evidenceValue(publicBoundary.head_sha))}</strong></div>
      <div class="evidence-metric"><span class="muted">Full admin</span><strong>${pcvEscapeHtml(evidenceValue(fullAdmin.version || fullAdmin.batch_id))}</strong></div>
      <div class="evidence-metric"><span class="muted">Manual admin</span><strong>${pcvEscapeHtml(evidenceValue(packagePair.package_pair || packagePair.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Manual admin next</span><strong>${pcvEscapeHtml(evidenceValue(nextPackagePair.package_pair || nextPackagePair.status))}</strong></div>
      <div class="evidence-metric"><span class="muted">Next decision</span><strong>${pcvEscapeHtml(evidenceValue(nextPackagePair.decision))}</strong></div>
      <div class="evidence-metric"><span class="muted">Descriptor</span><strong>${pcvEscapeHtml(evidenceValue(packagePair.current_card_descriptor_batch_id || packagePair.descriptor_batch_id))}</strong></div>
      <div class="evidence-metric"><span class="muted">Host Ops</span><strong>${pcvEscapeHtml(evidenceValue(hostOps.contract_key || hostOps.status))}</strong></div>
    </div>`;
}

function renderMonitoring() {
  const signals = buildMonitoringSignals();
  els.monitoringPanel.innerHTML = `
    <div class="monitoring-grid">
      ${signals.map((signal) => `<div class="monitoring-card signal-${pcvEscapeHtml(signal.tone)}" data-signal="${pcvEscapeHtml(signal.key)}"><span class="muted">${pcvEscapeHtml(signal.label)}</span><strong>${pcvEscapeHtml(signal.value)}</strong></div>`).join('')}
    </div>`;
}

function renderTroubleshootingEvidence() {
  const evidence = getBatchEvidence();
  const issues = collectEvidenceIssues();
  const latest = asObject(evidence?.latest);
  const release = asObject(latest.release);
  const issueHtml = issues.length
    ? `<div class="triage-list">${issues.map((issue) => `<div class="triage-row">
        <div>
          <strong>${pcvEscapeHtml(issue.code)}</strong>
          <span class="muted">${pcvEscapeHtml(issue.message)}</span>
        </div>
        <span class="status-badge ${pcvEscapeHtml(evidenceIssueTone(issue))}">${pcvEscapeHtml(issue.detail || normalizeEvidenceStatus(evidence))}</span>
      </div>`).join('')}</div>`
    : '<p class="muted">No batch evidence degradation is visible.</p>';

  return `<div class="troubleshooting-card batch-evidence-troubleshooting">
    <span class="muted">Batch evidence</span>
    <strong>${pcvEscapeHtml(latest.batch_id || normalizeEvidenceStatus(evidence))}</strong>
    ${renderEvidenceStatusBadge(evidence)}
    ${issueHtml}
    <div class="boundary-chip-row">
      <span>Public signing: ${pcvEscapeHtml(evidenceValue(release.public_trusted_signing, 'excluded'))}</span>
      <span>External publication: ${pcvEscapeHtml(evidenceValue(release.external_stable_publication, 'not-claimed'))}</span>
    </div>
  </div>`;
}

// --- legacy render-shell.ts ---
function renderError() {
  if (!state.error) {
    els.alertRegion.innerHTML = '';
    return;
  }
  els.alertRegion.innerHTML = `<strong>${pcvEscapeHtml(state.error.code)}</strong> ${pcvEscapeHtml(state.error.message)}<div>${pcvEscapeHtml(state.error.detail)}</div>`;
}

function pcvFormatRelativeTime(timestampMs) {
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
    els.statusUpdated.textContent = `Updated ${pcvFormatRelativeTime(state.lastRefreshedAt)}`;
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
    <strong>${pcvEscapeHtml(code)}</strong>
    <p>${pcvEscapeHtml(message)} Use the header api-token field or the account login form. Service tokens stay out of HTML.</p>
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
  els.assetStatus.innerHTML = `<span class="muted">Asset</span><strong>${pcvEscapeHtml(WEB_ASSET_LABEL)}</strong>`;
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
    ? items.map((item) => `<button type="button" data-command-id="${pcvEscapeHtml(item.id)}">
        <span class="status-badge ${pcvEscapeHtml(normalizeEventTone(item.tone))}">${pcvEscapeHtml(item.id.split(':')[0])}</span>
        <strong>${pcvEscapeHtml(item.label)}</strong>
        <span>${pcvEscapeHtml(item.detail)}</span>
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
    return `<a class="workspace-tab${active ? ' active' : ''}" role="tab" aria-selected="${active ? 'true' : 'false'}" href="#${pcvEscapeHtml(view)}" data-view-link="${pcvEscapeHtml(view)}">${pcvEscapeHtml(getViewLabel(view))}<button type="button" aria-label="${pcvEscapeHtml(getViewLabel(view))} tab pinned">x</button></a>`;
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
    return `<button type="button" class="asset-row${active ? ' active' : ''}" data-action="select-asset-vm" data-vm-id="${pcvEscapeHtml(vmId)}">
      <span class="asset-check"></span>
      <span class="asset-star">☆</span>
      <span class="asset-health"></span>
      <strong>${pcvEscapeHtml(getVmName(vm))}</strong>
      <span>${pcvEscapeHtml(cpu)}</span>
      <span>${pcvEscapeHtml(memoryLabel)}</span>
      <span>${pcvEscapeHtml(stateLabel)}</span>
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
  try { renderActiveView(); } catch (e) { if (window._DEBUG) console.warn('render:renderActiveView', e); }
  try { applyUiPreferences(); } catch (e) { if (window._DEBUG) console.warn('render:applyUiPreferences', e); }
  try { renderError(); } catch (e) { if (window._DEBUG) console.warn('render:renderError', e); }
  try { renderConnectionState(); } catch (e) { if (window._DEBUG) console.warn('render:renderConnectionState', e); }
  try { renderAuthGate(); } catch (e) { if (window._DEBUG) console.warn('render:renderAuthGate', e); }
  try { renderStatusBar(); } catch (e) { if (window._DEBUG) console.warn('render:renderStatusBar', e); }
  try { renderHeroChips(); } catch (e) { if (window._DEBUG) console.warn('render:renderHeroChips', e); }
  try { renderAssetStatus(); } catch (e) { if (window._DEBUG) console.warn('render:renderAssetStatus', e); }
  try { renderCommandPalette(); } catch (e) { if (window._DEBUG) console.warn('render:renderCommandPalette', e); }
  try { renderWorkspaceTabs(); } catch (e) { if (window._DEBUG) console.warn('render:renderWorkspaceTabs', e); }
  try { renderVmAssetList(); } catch (e) { if (window._DEBUG) console.warn('render:renderVmAssetList', e); }
  try { renderOpsCockpit(); } catch (e) { if (window._DEBUG) console.warn('render:renderOpsCockpit', e); }
  try { renderMetrics(); } catch (e) { if (window._DEBUG) console.warn('render:renderMetrics', e); }
  try { renderHost(); } catch (e) { if (window._DEBUG) console.warn('render:renderHost', e); }
  try { renderVms(); } catch (e) { if (window._DEBUG) console.warn('render:renderVms', e); }
  try { renderNetworkInventory(); } catch (e) { if (window._DEBUG) console.warn('render:renderNetworkInventory', e); }
  try { renderVmWorkbenchContext(); } catch (e) { if (window._DEBUG) console.warn('render:renderVmWorkbenchContext', e); }
  try { renderVmDetail(); } catch (e) { if (window._DEBUG) console.warn('render:renderVmDetail', e); }
  try { paintVmConsoleFrame(); } catch (e) { if (window._DEBUG) console.warn('render:paintVmConsoleFrame', e); }
  try { renderJobs(); } catch (e) { if (window._DEBUG) console.warn('render:renderJobs', e); }
  try { renderDashboardActivity(); } catch (e) { if (window._DEBUG) console.warn('render:renderDashboardActivity', e); }
  try { renderActivity(); } catch (e) { if (window._DEBUG) console.warn('render:renderActivity', e); }
  try { renderEventCenter(); } catch (e) { if (window._DEBUG) console.warn('render:renderEventCenter', e); }
  try { renderEvidenceDashboard(); } catch (e) { if (window._DEBUG) console.warn('render:renderEvidenceDashboard', e); }
  try { renderMonitoring(); } catch (e) { if (window._DEBUG) console.warn('render:renderMonitoring', e); }
  try { renderIncidentCommand(); } catch (e) { if (window._DEBUG) console.warn('render:renderIncidentCommand', e); }
  try { renderAccountSession(); } catch (e) { if (window._DEBUG) console.warn('render:renderAccountSession', e); }
  try { renderConsolePanel(); } catch (e) { if (window._DEBUG) console.warn('render:renderConsolePanel', e); }
  try { renderTokenRotation(); } catch (e) { if (window._DEBUG) console.warn('render:renderTokenRotation', e); }
  try { renderDiagnosticsBundle(); } catch (e) { if (window._DEBUG) console.warn('render:renderDiagnosticsBundle', e); }
  try { renderBetaFollowup(); } catch (e) { if (window._DEBUG) console.warn('render:renderBetaFollowup', e); }
  try { renderTroubleshooting(); } catch (e) { if (window._DEBUG) console.warn('render:renderTroubleshooting', e); }
  if (els.openCreateVm) {
    els.openCreateVm.disabled = !rbacAllows('operate');
  }
}

// exports
window.getDiagnosticActionPolicy = getDiagnosticActionPolicy;
window.buildPartialRefreshError = buildPartialRefreshError;
window.collectRefreshFailures = collectRefreshFailures;
window.getPriorityItems = getPriorityItems;
window.renderMetrics = renderMetrics;
window.renderHost = renderHost;
window.renderOpsSummaryCard = renderOpsSummaryCard;
window.renderOpsCockpit = renderOpsCockpit;
window.renderRuntimeApiRegistryBridge = renderRuntimeApiRegistryBridge;
window.renderHostOpsLifecycleBucketTable = renderHostOpsLifecycleBucketTable;
window.renderDiagnosticsBundle = renderDiagnosticsBundle;
window.renderTokenRotation = renderTokenRotation;
window.renderAccountSession = renderAccountSession;
window.renderAccountDirectory = renderAccountDirectory;
window.readNested = readNested;
window.formatPolicyValue = formatPolicyValue;
window.countJobsByStatus = countJobsByStatus;
window.getVmCheckpointCount = getVmCheckpointCount;
window.countVmCheckpointWarnings = countVmCheckpointWarnings;
window.countSelectedOldCheckpointWarnings = countSelectedOldCheckpointWarnings;
window.buildMonitoringSignals = buildMonitoringSignals;
window.evidenceStatusMessage = evidenceStatusMessage;
window.renderEvidenceDashboard = renderEvidenceDashboard;
window.renderCurrentEvidenceSummary = renderCurrentEvidenceSummary;
window.renderMonitoring = renderMonitoring;
window.renderTroubleshootingEvidence = renderTroubleshootingEvidence;
window.renderError = renderError;
window.pcvFormatRelativeTime = pcvFormatRelativeTime;
window.hasRefreshedOperation = hasRefreshedOperation;
window.renderStatusBar = renderStatusBar;
window.renderHeroChips = renderHeroChips;
window.getConnectionStateLabel = getConnectionStateLabel;
window.getStatusBarConnectionLabel = getStatusBarConnectionLabel;
window.renderConnectionState = renderConnectionState;
window.needsAuthGate = needsAuthGate;
window.bindAlertRegionAuthGate = bindAlertRegionAuthGate;
window.renderAuthGate = renderAuthGate;
window.applyUiPreferences = applyUiPreferences;
window.renderAssetStatus = renderAssetStatus;
window.getViewLabel = getViewLabel;
window.buildCommandPaletteItems = buildCommandPaletteItems;
window.getCommandPaletteMatches = getCommandPaletteMatches;
window.renderCommandPalette = renderCommandPalette;
window.openCommandPalette = openCommandPalette;
window.closeCommandPalette = closeCommandPalette;
window.handleCommandSearch = handleCommandSearch;
window.runCommandPaletteItem = runCommandPaletteItem;
window.renderWorkspaceTabs = renderWorkspaceTabs;
window.getAssetFilterText = getAssetFilterText;
window.renderVmAssetList = renderVmAssetList;
window.renderActiveView = renderActiveView;
window.render = render;
window.CONNECTION_STATE_LABELS = CONNECTION_STATE_LABELS;
PCV.monitor = Object.assign(PCV.monitor || {}, { getDiagnosticActionPolicy, buildPartialRefreshError, collectRefreshFailures, getPriorityItems, renderMetrics, renderHost, renderOpsSummaryCard, renderOpsCockpit, renderRuntimeApiRegistryBridge, renderHostOpsLifecycleBucketTable, renderDiagnosticsBundle, renderTokenRotation, renderAccountSession, renderAccountDirectory, readNested, formatPolicyValue, countJobsByStatus, getVmCheckpointCount, countVmCheckpointWarnings, countSelectedOldCheckpointWarnings, buildMonitoringSignals, evidenceStatusMessage, renderEvidenceDashboard, renderCurrentEvidenceSummary, renderMonitoring, renderTroubleshootingEvidence, renderError, pcvFormatRelativeTime, hasRefreshedOperation, renderStatusBar, renderHeroChips, getConnectionStateLabel, getStatusBarConnectionLabel, renderConnectionState, needsAuthGate, bindAlertRegionAuthGate, renderAuthGate, applyUiPreferences, renderAssetStatus, getViewLabel, buildCommandPaletteItems, getCommandPaletteMatches, renderCommandPalette, openCommandPalette, closeCommandPalette, handleCommandSearch, runCommandPaletteItem, renderWorkspaceTabs, getAssetFilterText, renderVmAssetList, renderActiveView, render, CONNECTION_STATE_LABELS });
})(window.PCV);

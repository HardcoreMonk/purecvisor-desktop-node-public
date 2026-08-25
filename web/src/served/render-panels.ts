// @ts-nocheck
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
    ? `<ul class="diagnostics-route-list">${routeKeys.map((route) => `<li>${escapeHtml(route)}</li>`).join('')}</ul>`
    : '<p>Route detail not reported by ops summary.</p>';

  return `<div class="diagnostics-result runtime-api-registry-bridge">
    <span class="muted">Runtime/API registry bridge</span>
    <strong>${escapeHtml(contractKey || 'not reported')}</strong>
    <p>${escapeHtml([source || 'source not reported', routeLabel].join(' / '))}</p>
    <p>${escapeHtml(anchor || 'documentation anchor not reported')}</p>
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
      <strong>${escapeHtml(descriptor.status || 'not reported')}</strong>
      <p>${escapeHtml(contractKey)}</p>
      <div class="boundary-chip-row">
        <span>${escapeHtml(mutationLabel)}</span>
        <span>ops summary metadata only</span>
      </div>
    </div>`;
  }

  const rows = buckets.map((bucket) => {
    const operations = asArray(bucket.operations).map((operation) => String(operation || '').trim()).filter(Boolean);
    return `<tr>
      <td>${escapeHtml(bucket.bucket_key || bucket.bucketKey)}</td>
      <td>${escapeHtml(bucket.owner || '-')}</td>
      <td>${escapeHtml(bucket.mutation_boundary || bucket.mutationBoundary || '-')}</td>
      <td>${escapeHtml(operations.join(', ') || '-')}</td>
    </tr>`;
  }).join('');

  return `<div class="diagnostics-result hostops-lifecycle-buckets">
    <span class="muted">Host Ops lifecycle buckets</span>
    <strong>${escapeHtml(descriptor.contract_key || descriptor.contractKey || 'host-ops-lifecycle-descriptor-bridge-v1')}</strong>
    <p>${escapeHtml(contractKey)}</p>
    <div class="evidence-table-wrap diagnostics-hostops-table">
      <table class="evidence-table">
        <thead><tr><th>Bucket</th><th>Owner</th><th>Mutation boundary</th><th>Operations</th></tr></thead>
        <tbody>${rows}</tbody>
      </table>
    </div>
    <div class="boundary-chip-row">
      <span>${escapeHtml(mutationLabel)}</span>
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
    ? `<div class="diagnostics-result error"><span class="muted">Last action</span><strong>${escapeHtml(state.diagnosticBundleError.code)}</strong><p>${escapeHtml(state.diagnosticBundleError.message)} ${escapeHtml(state.diagnosticBundleError.detail)}</p><p>${escapeHtml(policy.guide)}</p></div>`
    : bundleId
      ? `<div class="diagnostics-result"><span class="muted">Latest bundle</span><strong>${escapeHtml(bundleId)}</strong><p>${escapeHtml([
          bundle.created_at,
          bundle.download_status,
          bundle.redaction_status,
          bundle.retention_status
        ].filter(Boolean).join(' / ') || 'ready for authenticated download')}</p></div>`
      : `<div class="diagnostics-result"><span class="muted">Latest bundle</span><strong>Not created in this browser session</strong><p>Create uses POST ${escapeHtml(DESKTOP_NODE_API_ROUTES.diagnosticBundles)} and keeps the archive server-side.</p></div>`;
  const downloadHtml = download.bundle_id
    ? `<div class="diagnostics-result"><span class="muted">Last download</span><strong>${escapeHtml(download.file_name || download.bundle_id)}</strong><p>${escapeHtml(download.content_type || 'application/vnd.purecvisor.diagnostic-bundle+json')} / ${escapeHtml(download.size_bytes ?? 0)} bytes</p></div>`
    : '';

  els.diagnosticsPanel.innerHTML = `<div class="diagnostics-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Support artifact</span>
        <strong>Diagnostic Bundle</strong>
      </div>
      <span class="status-badge ${pending || state.diagnosticBundleError ? 'warn' : 'ok'}">${escapeHtml(pending || policy.statusLabel)}</span>
    </div>
    <div class="diagnostics-grid">
      ${facts.map(([label, value]) => `<div class="diagnostics-fact"><span class="muted">${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join('')}
    </div>
    <div class="diagnostics-actions">
      <button type="button" data-action="diagnostic-create" ${createDisabled}>Create bundle</button>
      <button type="button" data-action="diagnostic-download" data-bundle-id="${escapeHtml(bundleId)}" ${downloadDisabled}>Download latest</button>
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
      ${facts.map(([label, value]) => `<div class="diagnostics-fact"><span class="muted">${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join('')}
    </div>
    <div class="token-rotation-actions">
      <button type="button" data-action="clear-browser-token">Clear browser token</button>
      <span class="muted">Token values and Authorization headers are not rendered.</span>
    </div>
    ${state.tokenActionMessage ? `<div class="diagnostics-result"><span class="muted">Last browser action</span><strong>${escapeHtml(state.tokenActionMessage)}</strong></div>` : ''}
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
    ? `<div class="diagnostics-result error"><span class="muted">Account auth</span><strong>${escapeHtml(state.authError.code)}</strong><p>${escapeHtml(state.authError.message)} ${escapeHtml(state.authError.detail)}</p></div>`
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
      <span class="status-badge ${signedIn ? 'ok' : 'warn'}">${escapeHtml(status)}</span>
    </div>
    ${loginFormHtml}
    <div class="diagnostics-grid">
      <div class="diagnostics-fact"><span class="muted">User</span><strong>${escapeHtml(session.username || '-')}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Role</span><strong>${escapeHtml(role)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">JWT access</span><strong>${escapeHtml(state.authAccessToken ? 'present' : 'empty')}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Refresh</span><strong>${escapeHtml(state.authRefreshToken ? 'present' : 'empty')}</strong></div>
    </div>
    <div class="token-rotation-actions">
      <button type="button" data-action="account-refresh"${state.authRefreshToken ? '' : ' disabled aria-disabled="true"'}>Refresh JWT</button>
      <button type="button" data-action="account-logout"${signedIn ? '' : ' disabled aria-disabled="true"'}>Logout account</button>
      <span class="muted">JWT and password values are kept out of the DOM after submit.</span>
    </div>
    <div class="boundary-chip-row">
      <span>permissions: ${escapeHtml(permissions.length ? permissions.join(', ') : 'none')}</span>
      <span>service bearer fallback preserved</span>
      <span>RBAC gates destructive actions</span>
    </div>
    ${errorHtml}
  </div>`;
}


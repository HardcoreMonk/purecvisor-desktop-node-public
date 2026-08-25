// @ts-nocheck
function formatConsoleLabel(value, fallback = '-') {
  if (value === true) return 'enabled';
  if (value === false) return 'disabled';
  if (value === null || value === undefined || value === '') return fallback;
  return String(value).replace(/_/g, ' ');
}

function getConsoleAccessProjection(source, fallbackSource = {}) {
  const sourceObject = asObject(source);
  const fallbackObject = asObject(fallbackSource);
  const card = asObject(sourceObject.console_access);
  const fallbackCard = asObject(fallbackObject.console_access);
  const account = asObject(card.account || fallbackCard.account);
  const windowsConsole = asObject(
    card.windows_console ||
    sourceObject.windows_console ||
    sourceObject.console ||
    fallbackCard.windows_console ||
    fallbackObject.windows_console ||
    fallbackObject.console
  );
  const noVnc = asObject(
    card.novnc ||
    sourceObject.novnc ||
    fallbackCard.novnc ||
    fallbackObject.novnc
  );
  const noVncStatus = noVnc.status || (noVnc.enabled ? 'available' : 'not_configured');
  const noVncEnabled = noVnc.enabled === true || String(noVncStatus).toLowerCase() === 'available';
  const noVncPathOrReason =
    noVnc.websocket_path ||
    noVnc.path ||
    noVnc.reason ||
    (noVncEnabled ? 'noVNC bridge is configured.' : 'Windows VNC/WebSocket bridge is not configured.');
  const nextAction =
    card.next_action ||
    sourceObject.next_action ||
    fallbackCard.next_action ||
    fallbackObject.next_action ||
    (noVncEnabled
      ? 'Open the noVNC browser session for this VM, or use vmconnect from the host console.'
      : 'Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.');

  return {
    accountPermission:
      account.required_permission ||
      sourceObject.required_permission ||
      fallbackObject.required_permission ||
      'console.view',
    contract: card.contract || fallbackCard.contract || 'legacy-console-payload',
    windowsType: windowsConsole.type || 'vmconnect',
    windowsTransport:
      windowsConsole.transport ||
      windowsConsole.launch_mode ||
      windowsConsole.launch ||
      (windowsConsole.available === false ? 'unavailable' : 'local-handoff'),
    noVncStatus,
    noVncEnabled,
    noVncPathOrReason,
    nextAction
  };
}

function renderConsolePanel() {
  if (!els.accountConsolePanel) return;
  const capabilities = state.consoleCapabilities || {};
  const selectedVm = state.selectedVmId || '';
  const projection = getConsoleAccessProjection(state.consoleSession || capabilities, capabilities);
  const sessionProjection = state.consoleSession ? getConsoleAccessProjection(state.consoleSession, capabilities) : null;
  const currentRole = asObject(state.authSession).role || 'not signed in';
  const allowed = rbacAllows('console.view');
  const disabled = selectedVm && allowed ? '' : ' disabled aria-disabled="true"';
  const status = projection.noVncEnabled ? 'noVNC available' : `noVNC ${formatConsoleLabel(projection.noVncStatus)}`;
  const errorHtml = state.consoleError
    ? `<div class="diagnostics-result error"><span class="muted">Console</span><strong>${escapeHtml(state.consoleError.code)}</strong><p>${escapeHtml(state.consoleError.message)} ${escapeHtml(state.consoleError.detail)}</p></div>`
    : '';
  const sessionHtml = state.consoleSession
    ? `<div class="diagnostics-result"><span class="muted">Console session</span><strong>${escapeHtml(state.consoleSession.vm_id || selectedVm)}</strong><p>${escapeHtml(sessionProjection.windowsTransport)} / ${escapeHtml(formatConsoleLabel(sessionProjection.noVncStatus))} / ${escapeHtml(sessionProjection.noVncPathOrReason)}</p></div>`
    : '';

  els.accountConsolePanel.innerHTML = `<div class="diagnostics-card account-console-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Account/Console</span>
        <strong>noVNC / Hyper-V Console</strong>
      </div>
      <span class="status-badge ${projection.noVncEnabled ? 'ok' : 'warn'}">${escapeHtml(status)}</span>
    </div>
    <div class="diagnostics-grid">
      <div class="diagnostics-fact"><span class="muted">account permission</span><strong>${escapeHtml(projection.accountPermission)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">current role</span><strong>${escapeHtml(currentRole)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Windows console</span><strong>${escapeHtml(`${projection.windowsType} / ${projection.windowsTransport}`)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC status</span><strong>${escapeHtml(formatConsoleLabel(projection.noVncStatus))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC path/reason</span><strong>${escapeHtml(projection.noVncPathOrReason)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Selected VM</span><strong>${escapeHtml(selectedVm || '-')}</strong></div>
    </div>
    <div class="diagnostics-actions">
      <button type="button" data-action="console-open-selected"${disabled}>Open selected console</button>
      <span class="muted">${escapeHtml(projection.nextAction)}</span>
    </div>
    ${sessionHtml}
    ${errorHtml}
    <div class="boundary-chip-row">
      <span>no Linux console backend</span>
      <span>no host mutation</span>
      <span>contract: ${escapeHtml(projection.contract)}</span>
    </div>
  </div>`;
}

function getBetaFollowupItems() {
  const configuredItems = asArray(readNested(state.opsSummary || {}, ['beta_followup', 'items']));
  if (configuredItems.length) {
    return configuredItems.map((item) => ({
      label: item.label || item.name || 'follow-up',
      status: item.status || item.state || 'tracked',
      detail: item.detail || item.evidence || ''
    }));
  }

  return [
    {
      label: 'Installed listener QA automation',
      status: 'ready',
      detail: 'capture-installed-listener-qa.mjs covers real listener navigation, diagnostics create/download, screenshots, and token non-observation.'
    },
    {
      label: 'service token revoke handoff',
      status: 'operator-owned',
      detail: 'The browser clears only its saved token; protected service token rotation/revoke remains an elevated operator path.'
    },
    {
      label: 'diagnostic retention pagination',
      status: 'code-level applied',
      detail: 'Server-side bundle creation/download and retention are active; pagination remains tracked as a future list-route hardening item.'
    },
    {
      label: 'VM delete guarded',
      status: 'guarded UI',
      detail: 'Delete stays behind selection, managed-marker guard, running-state block, and API job tracking.'
    },
    {
      label: 'ops cockpit P0/P1/P2',
      status: 'surface-active',
      detail: 'Dashboard, activity, network, evidence, monitoring, and troubleshooting views are wired for beta validation.'
    },
    {
      label: 'public distribution bundle',
      status: 'local descriptor bundle',
      detail: 'Public distribution/operations expansion is tracked by non-mutating bundle evidence; public signing and external publication are not claimed.'
    },
    {
      label: 'Browser host boundary',
      status: 'host mutation not started from browser',
      detail: 'MSI, firewall, trust-store, LAN, signed build, updater, and rollback mutation remain outside this Web Console surface.'
    }
  ];
}

function renderBetaFollowup() {
  if (!els.betaFollowupPanel) return;

  const items = getBetaFollowupItems();
  els.betaFollowupPanel.innerHTML = `<div class="diagnostics-card beta-followup-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Beta readiness</span>
        <strong>Follow-up Status</strong>
      </div>
      <span class="status-badge ok">tracked</span>
    </div>
    <div class="triage-list">
      ${items.map((item) => `<div class="triage-row">
        <div>
          <strong>${escapeHtml(item.label)}</strong>
          <p>${escapeHtml(item.detail || 'Tracked for beta validation.')}</p>
        </div>
        <span class="status-badge ${String(item.status).toLowerCase().includes('not') || String(item.status).toLowerCase().includes('blocked') ? 'warn' : 'ok'}">${escapeHtml(item.status)}</span>
      </div>`).join('')}
    </div>
    <div class="boundary-chip-row">
      <span>host mutation not started from browser</span>
      <span>token values hidden</span>
      <span>public publication not claimed</span>
    </div>
  </div>`;
}

function renderTroubleshooting() {
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const cards = [
    ['Host readiness', host.supported === false ? 'Needs attention' : 'Ready', 'Check Hyper-V support, admin context, VMMS, and Default Switch state.'],
    ['VMMS', formatPolicyValue(readNested(host, ['hyperv', 'vmms_running'])), 'VM lifecycle requests require the Hyper-V management service to be available.'],
    ['Listener exposure', formatPolicyValue(readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind'])), 'Loopback is the default. LAN mode requires explicit approval and token source proof.'],
    ['Token storage', formatPolicyValue(readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage'])), 'Token values are never rendered in this console.'],
    ['Job store', formatPolicyValue(readNested(policy, ['job_runtime', 'state_store', 'persistence'])), 'Schema v2 migration stores load; newer unsupported schemas return PCV_JOB_STORE_SCHEMA_UNSUPPORTED without quarantine.'],
    ['Diagnostics boundary', 'read-only', 'Evidence path, token value, and host mutation command inputs are not rendered here.']
  ];
  const errors = [
    ['PCV_AUTH_REQUIRED', 'Token is missing or rejected.'],
    ['PCV_JOB_STORE_SCHEMA_UNSUPPORTED', 'Job store was written by a newer unsupported runtime. Stop and investigate before any migration apply.'],
    ['PCV_VM_NOT_MANAGED_BY_PURECVISOR', 'The API blocked destructive VM mutation before provider mutation.'],
    ['PCV_VM_SHUTDOWN_NOT_AVAILABLE', 'Guest shutdown integration is unavailable for the selected VM.']
  ];

  els.troubleshootingPanel.innerHTML = `
    ${renderTroubleshootingEvidence()}
    <div class="troubleshooting-grid">
      ${cards.map(([title, value, detail]) => `<div class="troubleshooting-card"><span class="muted">${escapeHtml(title)}</span><strong>${escapeHtml(value)}</strong><p>${escapeHtml(detail)}</p></div>`).join('')}
    </div>
    <div class="code-list">
      ${errors.map(([code, detail]) => `<div class="kv"><span>${escapeHtml(code)}</span><strong>${escapeHtml(detail)}</strong></div>`).join('')}
    </div>`;
}


// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): browser console frame, keyboard, checkpoint schedule, export/import and guest controls (legacy render-console, vm-detail-extensions).
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-console.ts ---
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
  const noVncReasonCode = noVnc.reason_code || noVncStatus;
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
    noVncReasonCode,
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
    ? `<div class="diagnostics-result error"><span class="muted">Console</span><strong>${pcvEscapeHtml(state.consoleError.code)}</strong><p>${pcvEscapeHtml(state.consoleError.message)} ${pcvEscapeHtml(state.consoleError.detail)}</p></div>`
    : '';
  const sessionHtml = state.consoleSession
    ? `<div class="diagnostics-result"><span class="muted">Console session</span><strong>${pcvEscapeHtml(state.consoleSession.vm_id || selectedVm)}</strong><p>${pcvEscapeHtml(sessionProjection.windowsTransport)} / ${pcvEscapeHtml(formatConsoleLabel(sessionProjection.noVncStatus))} / ${pcvEscapeHtml(sessionProjection.noVncPathOrReason)}</p></div>`
    : '';

  els.accountConsolePanel.innerHTML = `<div class="diagnostics-card account-console-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">Account/Console</span>
        <strong>noVNC / Hyper-V Console</strong>
      </div>
      <span class="status-badge ${projection.noVncEnabled ? 'ok' : 'warn'}">${pcvEscapeHtml(status)}</span>
    </div>
    <div class="diagnostics-grid">
      <div class="diagnostics-fact"><span class="muted">account permission</span><strong>${pcvEscapeHtml(projection.accountPermission)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">current role</span><strong>${pcvEscapeHtml(currentRole)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Windows console</span><strong>${pcvEscapeHtml(`${projection.windowsType} / ${projection.windowsTransport}`)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC enabled</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncEnabled))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC status</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncStatus))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC reason_code</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncReasonCode))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">noVNC path/reason</span><strong>${pcvEscapeHtml(projection.noVncPathOrReason)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">Selected VM</span><strong>${pcvEscapeHtml(selectedVm || '-')}</strong></div>
    </div>
    <div class="diagnostics-actions">
      <button type="button" data-action="console-open-selected"${disabled}>Open selected console</button>
      <span class="muted">${pcvEscapeHtml(projection.nextAction)}</span>
    </div>
    ${sessionHtml}
    ${errorHtml}
    <div class="boundary-chip-row">
      <span>no Linux console backend</span>
      <span>no host mutation</span>
      <span>no target save form</span>
      <span>CLI/API configure only</span>
      <span>contract: ${pcvEscapeHtml(projection.contract)}</span>
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
          <strong>${pcvEscapeHtml(item.label)}</strong>
          <p>${pcvEscapeHtml(item.detail || 'Tracked for beta validation.')}</p>
        </div>
        <span class="status-badge ${String(item.status).toLowerCase().includes('not') || String(item.status).toLowerCase().includes('blocked') ? 'warn' : 'ok'}">${pcvEscapeHtml(item.status)}</span>
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
      ${cards.map(([title, value, detail]) => `<div class="troubleshooting-card"><span class="muted">${pcvEscapeHtml(title)}</span><strong>${pcvEscapeHtml(value)}</strong><p>${pcvEscapeHtml(detail)}</p></div>`).join('')}
    </div>
    <div class="code-list">
      ${errors.map(([code, detail]) => `<div class="kv"><span>${pcvEscapeHtml(code)}</span><strong>${pcvEscapeHtml(detail)}</strong></div>`).join('')}
    </div>`;
}

// --- legacy vm-detail-extensions.ts ---
// VM detail 의 lifecycle action 표와 확장 form action 이다. served-app.ts 의 submit 분기가 처리하지 않은
// data-action 은 handleVmDetailExtensionSubmit 으로 온다.
const VM_LIFECYCLE_ACTIONS = {
  'vm-start': 'start',
  'vm-shutdown': 'shutdown',
  'vm-poweroff': 'poweroff',
  'vm-restart': 'restart',
  'vm-save': 'save',
  'vm-resume-saved': 'resume-saved',
  'vm-pause': 'pause',
  'vm-resume': 'resume',
  'vm-eject': 'eject'
};

async function queueVmRename(vmId, newName) {
  requireRbac('operate', 'VM rename');
  const target = String(newName || '').trim();
  if (!target) {
    throw normalizeError({
      code: 'PCV_VM_RENAME_TARGET_REQUIRED',
      message: 'Enter a new VM name.',
      detail: 'new_name is required before queueing vm.rename.'
    });
  }
  if (!window.confirm(`Rename VM '${vmId}' to '${target}'?\n\nThe VM keeps its ID and disks. Scripts that use the old name must change.`)) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'rename');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmRename(vmId, target);
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

const VM_DETAIL_EXTENSION_CLICK_ACTIONS = new Set(['checkpoint-schedule-clear', 'vm-console-frame-start', 'vm-console-frame-stop', 'vm-console-input-cad']);

function readCheckpointSchedulePayload(data) {
  const payload = {};
  for (const name of ['interval_minutes', 'retention_max']) {
    const raw = String(data.get(name) ?? '').trim();
    if (raw) {
      payload[name] = Number(raw);
    }
  }
  return payload;
}

function buildCheckpointScheduleConfirmation(vmId, mode, payload) {
  return mode === 'clear'
    ? `Clear the periodic checkpoint schedule for VM '${vmId}'?\n\nExisting checkpoints stay. Scheduled checkpoints stop until a new schedule is saved.`
    : `Save the periodic checkpoint schedule for VM '${vmId}'?\n\ninterval_minutes=${payload.interval_minutes ?? '-'} / retention_max=${payload.retention_max ?? '-'}`;
}

async function queueCheckpointScheduleControl(vmId, mode, payload) {
  requireRbac('operate', `Checkpoint schedule ${mode}`);
  if (mode !== 'preview' && !window.confirm(buildCheckpointScheduleConfirmation(vmId, mode, payload))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, `checkpoint-schedule-${mode}`);
  state.error = null;
  render();
  try {
    const result = mode === 'preview'
      ? await desktopApi.previewCheckpointSchedule(vmId, payload)
      : mode === 'set'
        ? await desktopApi.setCheckpointSchedule(vmId, payload)
        : await desktopApi.clearCheckpointSchedule(vmId);
    if (mode === 'preview') {
      state.checkpointSchedulePreview = { vm_id: vmId, updated_at: new Date().toISOString(), result };
    } else {
      pcvTrackJob(result);
      startPolling();
    }
    state.connectionState = 'connected';
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

function renderCheckpointSchedulePreview(vmId) {
  const preview = state.checkpointSchedulePreview;
  if (!preview || preview.vm_id !== vmId) {
    return '<p class="muted">Preview the schedule before saving it.</p>';
  }
  const updated = preview.updated_at ? new Date(preview.updated_at).toLocaleString() : '-';
  return `<p class="muted">schedule preview ${pcvEscapeHtml(updated)}: ${pcvEscapeHtml(formatObjectValue(preview.result))}</p>`;
}

function readExportImportPayload(kind, data) {
  const directory = String(data.get('directory') || '').trim();
  return kind === 'export'
    ? { directory }
    : { name: String(data.get('name') || '').trim(), directory, has_vmcx: data.get('has_vmcx') === 'on' };
}

function buildExportImportConfirmation(vmId, kind, payload) {
  return kind === 'export'
    ? `Export VM '${vmId}' to '${payload.directory}'?\n\nHyper-V writes an export package there. The VM itself does not change.`
    : `Import '${payload.directory}' as new VM '${payload.name}'?\n\nThe import gets a new VM identity and the managed marker. Disks are copied under the VM root.`;
}

async function queueVmExportImportControl(vmId, kind, mode, payload) {
  requireRbac('operate', `VM ${kind} ${mode}`);
  const apply = mode === 'apply';
  if (apply && !window.confirm(buildExportImportConfirmation(vmId, kind, payload))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, `${kind}-${mode}`);
  state.error = null;
  render();
  try {
    const result = kind === 'export'
      ? apply ? await desktopApi.exportVm(vmId, payload) : await desktopApi.previewVmExport(vmId, payload)
      : apply ? await desktopApi.importVm(payload) : await desktopApi.previewVmImport(payload);
    if (apply) {
      pcvTrackJob(result);
      startPolling();
    } else {
      state.vmExportImportPreview = { vm_id: vmId, kind, updated_at: new Date().toISOString(), result };
    }
    state.connectionState = 'connected';
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

function renderExportImportPreview(vmId) {
  const preview = state.vmExportImportPreview;
  if (!preview || preview.vm_id !== vmId) {
    return '<p class="muted">Preview an export or import before running it.</p>';
  }
  const updated = preview.updated_at ? new Date(preview.updated_at).toLocaleString() : '-';
  return `<p class="muted">${pcvEscapeHtml(preview.kind)} preview ${pcvEscapeHtml(updated)}: ${pcvEscapeHtml(formatObjectValue(preview.result))}</p>`;
}

function renderSwitchOptions() {
  const inventory = state.networkInventory || {};
  const switches = asArray(inventory.switches || inventory.items || inventory.networks)
    .map((item) => String(item?.name || '').trim())
    .filter(Boolean);
  return ['<option value="">Select switch</option>']
    .concat(switches.map((name) => `<option value="${pcvEscapeHtml(name)}">${pcvEscapeHtml(name)}</option>`))
    .join('');
}

function readVmNetworkChangePayload(kind, data) {
  const switchName = String(data.get('switch') || '').trim();
  if (kind === 'connect') {
    return { switch: switchName };
  }
  const device = String(data.get('device') || 'nic');
  const isoPath = String(data.get('iso_path') || '').trim();
  return device === 'dvd'
    ? (isoPath ? { device, iso_path: isoPath } : { device })
    : { device, switch: switchName };
}

function buildVmNetworkChangeConfirmation(vmId, kind, payload) {
  return kind === 'connect'
    ? `Connect VM '${vmId}' to switch '${payload.switch}'?\n\nThe first network adapter is retargeted. The VM must be Off.`
    : `Add a ${payload.device === 'dvd' ? 'DVD drive' : 'network adapter'} to VM '${vmId}'?\n\nThe VM must be Off. Each add creates another device.`;
}

async function queueVmNetworkChange(vmId, kind, payload) {
  requireRbac('operate', kind === 'connect' ? 'VM switch connect' : 'VM device add');
  if (!window.confirm(buildVmNetworkChangeConfirmation(vmId, kind, payload))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, kind === 'connect' ? 'network-connect' : 'device-add');
  state.error = null;
  render();
  try {
    const job = kind === 'connect'
      ? await desktopApi.connectVmNetwork(vmId, payload)
      : await desktopApi.addVmDevice(vmId, payload);
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

async function previewVmGuestExecutionControl(vmId, kind, payload) {
  requireRbac(kind === 'exec' ? 'guest.exec' : 'guest.channel.configure', `VM guest ${kind} preview`);
  const controlKind = kind === 'exec' ? 'guest-execution' : 'guest-channel';
  const control = (patch) => ({ vm_id: vmId, kind: controlKind, mode: 'preview', loading: false, updated_at: new Date().toISOString(), result: null, error: null, ...patch });
  state.actionPending = true;
  setVmActionPending(vmId, `guest-${kind}-preview`);
  state.error = null;
  state.selectedVmQosControl = control({ loading: true, updated_at: '' });
  render();
  try {
    const result = kind === 'exec'
      ? await desktopApi.previewVmGuestExec(vmId, payload)
      : await desktopApi.previewVmGuestChannel(vmId, payload);
    state.selectedVmQosControl = control({ result });
    state.connectionState = 'connected';
  } catch (error) {
    const normalized = normalizeError(error);
    state.error = normalized;
    state.selectedVmQosControl = control({ error: normalized });
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function handleVmGuestPreviewSubmit(guestForm, submitterAction, data) {
  const exec = submitterAction === 'vm-guest-exec-preview';
  await previewVmGuestExecutionControl(
    guestForm.dataset.vmId,
    exec ? 'exec' : 'channel',
    exec ? readVmGuestExecPayload(data) : readVmGuestChannelPayload(data, 'repair'));
}

async function handleVmDetailExtensionSubmit(form, data, submitterAction) {
  if (form?.dataset.action === 'vm-rename') {
    await queueVmRename(form.dataset.vmId, data.get('new_name'));
    form.reset();
  } else if (form?.dataset.action === 'vm-export' || form?.dataset.action === 'vm-import') {
    const kind = form.dataset.action === 'vm-export' ? 'export' : 'import';
    await queueVmExportImportControl(
      form.dataset.vmId,
      kind,
      String(submitterAction || '').endsWith('-apply') ? 'apply' : 'preview',
      readExportImportPayload(kind, data));
  } else if (form?.dataset.action === 'vm-network-connect' || form?.dataset.action === 'vm-device-add') {
    const kind = form.dataset.action === 'vm-network-connect' ? 'connect' : 'device';
    await queueVmNetworkChange(form.dataset.vmId, kind, readVmNetworkChangePayload(kind, data));
  } else if (form?.dataset.action === 'vm-console-text') {
    const text = String(data.get('text') || '');
    if (text) await sendVmConsoleKeyboard(form.dataset.vmId, { kind: 'text', text });
    form.reset();
  } else if (form?.dataset.action === 'checkpoint-schedule') {
    await queueCheckpointScheduleControl(
      form.dataset.vmId,
      submitterAction === 'checkpoint-schedule-set' ? 'set' : 'preview',
      readCheckpointSchedulePayload(data));
  }
}

async function handleVmDetailExtensionClick(button) {
  if (button.dataset.action === 'checkpoint-schedule-clear') {
    await queueCheckpointScheduleControl(button.dataset.vmId, 'clear', {});
  } else if (button.dataset.action === 'vm-console-frame-start') {
    startVmConsoleFrame(button.dataset.vmId);
  } else if (button.dataset.action === 'vm-console-frame-stop') {
    stopVmConsoleFrame();
    render();
  } else if (button.dataset.action === 'vm-console-input-cad') {
    await sendVmConsoleKeyboard(button.dataset.vmId, { kind: 'ctrl-alt-del' });
  }
}

// S1 browser console (design pcv-s1-browser-console-v1 §6): the frame route returns a zlib-compressed RGB565 frame;
// the browser inflates it with DecompressionStream('deflate') and paints it on a canvas. Polling repaints only the canvas.
const VM_CONSOLE_FRAME_SIZES = ['640x480', '800x600', '1024x768'];
const VM_CONSOLE_FRAME_FPS = [1, 2, 5];

function decodeRgb565ToRgba(bytes, width, height) {
  const rgba = new Uint8ClampedArray(width * height * 4);
  for (let pixel = 0; pixel < width * height; pixel += 1) {
    const value = bytes[pixel * 2] | (bytes[(pixel * 2) + 1] << 8);
    rgba[pixel * 4] = Math.round(((value >> 11) & 0x1f) * 255 / 31);
    rgba[(pixel * 4) + 1] = Math.round(((value >> 5) & 0x3f) * 255 / 63);
    rgba[(pixel * 4) + 2] = Math.round((value & 0x1f) * 255 / 31);
    rgba[(pixel * 4) + 3] = 255;
  }
  return rgba;
}

async function inflateConsoleFrame(base64) {
  const binary = atob(base64);
  const compressed = new Uint8Array(binary.length);
  for (let index = 0; index < binary.length; index += 1) compressed[index] = binary.charCodeAt(index);
  const stream = new Blob([compressed]).stream().pipeThrough(new DecompressionStream('deflate'));
  return new Uint8Array(await new Response(stream).arrayBuffer());
}

function getVmConsoleFrameControl(vmId) {
  const control = state.vmConsoleFrame;
  return control && control.vm_id === vmId ? control : null;
}

function renderVmConsoleFrameCard(vm, vmId, canViewConsole) {
  const control = getVmConsoleFrameControl(vmId);
  const running = control?.running === true;
  const size = control?.size || VM_CONSOLE_FRAME_SIZES[0];
  const fps = control?.fps || 2;
  const [width, height] = size.split('x');
  const disabled = canViewConsole ? '' : ' disabled';
  const vmState = String(vm.state || vm.status || '').toLowerCase();
  const status = control?.error
    ? `${control.error.code}: ${control.error.message}`
    : running
      ? `streaming ${size} at ${fps} fps`
      : vmState === 'running' ? 'paused' : 'VM is not running; start it to see its screen.';
  return `<div class="checkpoint-panel vm-console-frame-panel">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">Browser console</p>
          <h3>VM Screen</h3>
        </div>
        <div class="diagnostics-actions">
          <select data-console-frame="size" aria-label="console frame size"${disabled}>${VM_CONSOLE_FRAME_SIZES.map((item) => `<option value="${item}"${item === size ? ' selected' : ''}>${item}</option>`).join('')}</select>
          <select data-console-frame="fps" aria-label="console frame rate"${disabled}>${VM_CONSOLE_FRAME_FPS.map((item) => `<option value="${item}"${item === fps ? ' selected' : ''}>${item} fps</option>`).join('')}</select>
          <button data-action="vm-console-frame-start" data-vm-id="${pcvEscapeHtml(vmId)}"${running ? ' disabled' : disabled}>Start screen</button>
          <button data-action="vm-console-frame-stop" data-vm-id="${pcvEscapeHtml(vmId)}"${running ? '' : ' disabled'}>Pause screen</button>
        </div>
      </div>
      <canvas id="vm-console-frame-canvas" class="vm-console-frame-canvas" tabindex="0" width="${pcvEscapeHtml(width)}" height="${pcvEscapeHtml(height)}" style="width:100%;max-width:${pcvEscapeHtml(width)}px;background:#000"></canvas>
      <p id="vm-console-frame-status" class="muted">${pcvEscapeHtml(status)}</p>
      ${renderVmConsoleInputControls(vmId)}
      <p class="muted">Screen view needs console.view. vmconnect handoff stays available from the Console button.</p>
    </div>`;
}

function paintVmConsoleFrame() {
  ensureVmConsoleKeyBinding();
  const control = state.vmConsoleFrame;
  const canvas = document.getElementById('vm-console-frame-canvas');
  if (!control?.image || !canvas || canvas.width !== control.image.width || canvas.height !== control.image.height) return;
  canvas.getContext('2d')?.putImageData(control.image, 0, 0);
}

function setVmConsoleFrameStatus(text) {
  const element = document.getElementById('vm-console-frame-status');
  if (element) element.textContent = text;
}

async function pollVmConsoleFrame(vmId, generation) {
  const control = state.vmConsoleFrame;
  if (!control || control.vm_id !== vmId || control.generation !== generation || !control.running) return;
  if (document.hidden || getVmId(state.selectedVm || {}) !== vmId) {
    stopVmConsoleFrame();
    return;
  }
  try {
    const frame = await desktopApi.getVmConsoleFrame(vmId, control.size);
    const data = frame?.data || frame;
    const rgb565 = await inflateConsoleFrame(data.frame_base64);
    control.image = new ImageData(decodeRgb565ToRgba(rgb565, data.width, data.height), data.width, data.height);
    control.error = null;
    paintVmConsoleFrame();
    setVmConsoleFrameStatus(`streaming ${control.size} at ${control.fps} fps / ${new Date().toLocaleTimeString()}`);
  } catch (error) {
    const normalized = normalizeError(error);
    if (normalized.code !== 'PCV_CONSOLE_RATE_LIMITED') {
      control.error = normalized;
      control.running = false;
      render();
      return;
    }
  }
  control.timer = window.setTimeout(() => pollVmConsoleFrame(vmId, generation), Math.round(1000 / control.fps));
}

function startVmConsoleFrame(vmId) {
  requireRbac('console.view', 'VM screen');
  const panel = els.vmDetailPanel;
  const size = panel?.querySelector('select[data-console-frame="size"]')?.value || VM_CONSOLE_FRAME_SIZES[0];
  const fps = Number(panel?.querySelector('select[data-console-frame="fps"]')?.value || 2);
  stopVmConsoleFrame();
  const generation = Date.now();
  state.vmConsoleFrame = { vm_id: vmId, size, fps, running: true, generation, image: null, error: null, timer: null };
  render();
  pollVmConsoleFrame(vmId, generation);
}

function stopVmConsoleFrame() {
  const control = state.vmConsoleFrame;
  if (!control) return;
  if (control.timer) window.clearTimeout(control.timer);
  control.running = false;
  control.timer = null;
}

// S1 console input (design pcv-s1-browser-console-v1 §6): a focused screen canvas sends key press/release by Windows
// virtual-key code; Ctrl+Alt+Del and short text go through buttons. Without console.input the screen stays read-only.
const VM_CONSOLE_VIRTUAL_KEYS = Object.freeze({
  Backspace: 0x08, Tab: 0x09, Enter: 0x0d, NumpadEnter: 0x0d, ShiftLeft: 0x10, ShiftRight: 0x10, ControlLeft: 0x11,
  ControlRight: 0x11, AltLeft: 0x12, AltRight: 0x12, Pause: 0x13, CapsLock: 0x14, Escape: 0x1b, Space: 0x20, PageUp: 0x21,
  PageDown: 0x22, End: 0x23, Home: 0x24, ArrowLeft: 0x25, ArrowUp: 0x26, ArrowRight: 0x27, ArrowDown: 0x28, Insert: 0x2d,
  Delete: 0x2e, MetaLeft: 0x5b, MetaRight: 0x5c, Semicolon: 0xba, Equal: 0xbb, Comma: 0xbc, Minus: 0xbd, Period: 0xbe,
  Slash: 0xbf, Backquote: 0xc0, BracketLeft: 0xdb, Backslash: 0xdc, BracketRight: 0xdd, Quote: 0xde
});

function getVmConsoleVirtualKey(code) {
  if (/^Key[A-Z]$/.test(code)) return code.charCodeAt(3);
  if (/^Digit[0-9]$/.test(code)) return code.charCodeAt(5);
  if (/^Numpad[0-9]$/.test(code)) return 0x60 + Number(code.slice(6));
  if (/^F([1-9]|1[0-2])$/.test(code)) return 0x6f + Number(code.slice(1));
  return VM_CONSOLE_VIRTUAL_KEYS[code] || null;
}

async function sendVmConsoleKeyboard(vmId, payload) {
  requireRbac('console.input', 'VM console input');
  try {
    await desktopApi.sendVmConsoleInput(vmId, payload);
  } catch (error) {
    const normalized = normalizeError(error);
    if (normalized.code === 'PCV_CONSOLE_RATE_LIMITED') return;
    const control = getVmConsoleFrameControl(vmId);
    if (control) control.error = normalized;
    setVmConsoleFrameStatus(`${normalized.code}: ${normalized.message}`);
  }
}

function handleVmConsoleKeyEvent(event) {
  if (event.target?.id !== 'vm-console-frame-canvas' || !rbacAllows('console.input')) return;
  const keyCode = getVmConsoleVirtualKey(event.code);
  if (!keyCode) return;
  event.preventDefault();
  if (event.type === 'keydown' && event.repeat) return;
  const vmId = getVmId(state.selectedVm || {});
  if (!vmId) return;
  sendVmConsoleKeyboard(vmId, { kind: 'key', action: event.type === 'keydown' ? 'press' : 'release', key_code: keyCode });
}

function ensureVmConsoleKeyBinding() {
  if (state.vmConsoleKeysBound || !els.vmDetailPanel) return;
  els.vmDetailPanel.addEventListener('keydown', handleVmConsoleKeyEvent);
  els.vmDetailPanel.addEventListener('keyup', handleVmConsoleKeyEvent);
  state.vmConsoleKeysBound = true;
}

function renderVmConsoleInputControls(vmId) {
  if (!rbacAllows('console.input')) {
    return '<p class="muted">Keyboard input needs console.input; the screen stays read-only.</p>';
  }
  return `<form class="vm-resource-form" data-action="vm-console-text" data-vm-id="${pcvEscapeHtml(vmId)}">
        <p class="muted">Click the screen to type into the VM. Keys go as press and release; passwords are not recorded.</p>
        <input name="text" maxlength="256" autocomplete="off" placeholder="Text to type (printable ASCII)" aria-label="console text">
        <button type="submit" data-action="vm-console-text">Send text</button>
        <button type="button" data-action="vm-console-input-cad" data-vm-id="${pcvEscapeHtml(vmId)}">Ctrl+Alt+Del</button>
      </form>`;
}

// exports
window.formatConsoleLabel = formatConsoleLabel;
window.getConsoleAccessProjection = getConsoleAccessProjection;
window.renderConsolePanel = renderConsolePanel;
window.getBetaFollowupItems = getBetaFollowupItems;
window.renderBetaFollowup = renderBetaFollowup;
window.renderTroubleshooting = renderTroubleshooting;
window.queueVmRename = queueVmRename;
window.readCheckpointSchedulePayload = readCheckpointSchedulePayload;
window.buildCheckpointScheduleConfirmation = buildCheckpointScheduleConfirmation;
window.queueCheckpointScheduleControl = queueCheckpointScheduleControl;
window.renderCheckpointSchedulePreview = renderCheckpointSchedulePreview;
window.readExportImportPayload = readExportImportPayload;
window.buildExportImportConfirmation = buildExportImportConfirmation;
window.queueVmExportImportControl = queueVmExportImportControl;
window.renderExportImportPreview = renderExportImportPreview;
window.renderSwitchOptions = renderSwitchOptions;
window.readVmNetworkChangePayload = readVmNetworkChangePayload;
window.buildVmNetworkChangeConfirmation = buildVmNetworkChangeConfirmation;
window.queueVmNetworkChange = queueVmNetworkChange;
window.previewVmGuestExecutionControl = previewVmGuestExecutionControl;
window.handleVmGuestPreviewSubmit = handleVmGuestPreviewSubmit;
window.handleVmDetailExtensionSubmit = handleVmDetailExtensionSubmit;
window.handleVmDetailExtensionClick = handleVmDetailExtensionClick;
window.decodeRgb565ToRgba = decodeRgb565ToRgba;
window.inflateConsoleFrame = inflateConsoleFrame;
window.getVmConsoleFrameControl = getVmConsoleFrameControl;
window.renderVmConsoleFrameCard = renderVmConsoleFrameCard;
window.paintVmConsoleFrame = paintVmConsoleFrame;
window.setVmConsoleFrameStatus = setVmConsoleFrameStatus;
window.pollVmConsoleFrame = pollVmConsoleFrame;
window.startVmConsoleFrame = startVmConsoleFrame;
window.stopVmConsoleFrame = stopVmConsoleFrame;
window.getVmConsoleVirtualKey = getVmConsoleVirtualKey;
window.sendVmConsoleKeyboard = sendVmConsoleKeyboard;
window.handleVmConsoleKeyEvent = handleVmConsoleKeyEvent;
window.ensureVmConsoleKeyBinding = ensureVmConsoleKeyBinding;
window.renderVmConsoleInputControls = renderVmConsoleInputControls;
window.VM_LIFECYCLE_ACTIONS = VM_LIFECYCLE_ACTIONS;
window.VM_DETAIL_EXTENSION_CLICK_ACTIONS = VM_DETAIL_EXTENSION_CLICK_ACTIONS;
window.VM_CONSOLE_FRAME_SIZES = VM_CONSOLE_FRAME_SIZES;
window.VM_CONSOLE_FRAME_FPS = VM_CONSOLE_FRAME_FPS;
window.VM_CONSOLE_VIRTUAL_KEYS = VM_CONSOLE_VIRTUAL_KEYS;
PCV.vmConsole = Object.assign(PCV.vmConsole || {}, { formatConsoleLabel, getConsoleAccessProjection, renderConsolePanel, getBetaFollowupItems, renderBetaFollowup, renderTroubleshooting, queueVmRename, readCheckpointSchedulePayload, buildCheckpointScheduleConfirmation, queueCheckpointScheduleControl, renderCheckpointSchedulePreview, readExportImportPayload, buildExportImportConfirmation, queueVmExportImportControl, renderExportImportPreview, renderSwitchOptions, readVmNetworkChangePayload, buildVmNetworkChangeConfirmation, queueVmNetworkChange, previewVmGuestExecutionControl, handleVmGuestPreviewSubmit, handleVmDetailExtensionSubmit, handleVmDetailExtensionClick, decodeRgb565ToRgba, inflateConsoleFrame, getVmConsoleFrameControl, renderVmConsoleFrameCard, paintVmConsoleFrame, setVmConsoleFrameStatus, pollVmConsoleFrame, startVmConsoleFrame, stopVmConsoleFrame, getVmConsoleVirtualKey, sendVmConsoleKeyboard, handleVmConsoleKeyEvent, ensureVmConsoleKeyBinding, renderVmConsoleInputControls, VM_LIFECYCLE_ACTIONS, VM_DETAIL_EXTENSION_CLICK_ACTIONS, VM_CONSOLE_FRAME_SIZES, VM_CONSOLE_FRAME_FPS, VM_CONSOLE_VIRTUAL_KEYS });
})(window.PCV);

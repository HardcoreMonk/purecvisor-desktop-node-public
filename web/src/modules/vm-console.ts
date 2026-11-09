// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): browser console frame, keyboard, checkpoint schedule, export/import and guest controls (legacy render-console, vm-detail-extensions).
// Top-level declarations stay on window.PCV so the module keeps the shared-scope behaviour of the classic-script bundle.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-console.ts ---
function formatConsoleLabel(value, fallback = '-') {
  if (value === true) return t('vm.console.panel.enabled');
  if (value === false) return t('vm.console.panel.disabled');
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
    (noVncEnabled ? t('vm.console.panel.bridge_on') : t('vm.console.panel.bridge_off'));
  const nextAction =
    card.next_action ||
    sourceObject.next_action ||
    fallbackCard.next_action ||
    fallbackObject.next_action ||
    (noVncEnabled
      ? t('vm.console.panel.next_on')
      : t('vm.console.panel.next_off'));

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
  const currentRole = asObject(state.authSession).role || t('vm.console.panel.signed_out');
  const allowed = rbacAllows('console.view');
  const disabled = selectedVm && allowed ? '' : ' disabled aria-disabled="true"';
  const status = projection.noVncEnabled ? t('vm.console.panel.novnc_on') : t('vm.console.panel.novnc_off', { status: formatConsoleLabel(projection.noVncStatus) });
  const errorHtml = state.consoleError
    ? `<div class="diagnostics-result error"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.error'))}</span><strong>${pcvEscapeHtml(state.consoleError.code)}</strong><p>${pcvEscapeHtml(state.consoleError.message)} ${pcvEscapeHtml(state.consoleError.detail)}</p></div>`
    : '';
  const sessionHtml = state.consoleSession
    ? `<div class="diagnostics-result"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.session'))}</span><strong>${pcvEscapeHtml(state.consoleSession.vm_id || selectedVm)}</strong><p>${pcvEscapeHtml(sessionProjection.windowsTransport)} / ${pcvEscapeHtml(formatConsoleLabel(sessionProjection.noVncStatus))} / ${pcvEscapeHtml(sessionProjection.noVncPathOrReason)}</p></div>`
    : '';

  els.accountConsolePanel.innerHTML = `<div class="diagnostics-card account-console-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">${pcvEscapeHtml(t('vm.console.panel.account'))}</span>
        <strong>${pcvEscapeHtml(t('vm.console.panel.title'))}</strong>
      </div>
      <span class="status-badge ${projection.noVncEnabled ? 'ok' : 'warn'}">${pcvEscapeHtml(status)}</span>
    </div>
    <div class="diagnostics-grid">
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.permission'))}</span><strong>${pcvEscapeHtml(projection.accountPermission)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.role'))}</span><strong>${pcvEscapeHtml(currentRole)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.windows'))}</span><strong>${pcvEscapeHtml(`${projection.windowsType} / ${projection.windowsTransport}`)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.novnc_enabled'))}</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncEnabled))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.novnc_status'))}</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncStatus))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.reason_code'))}</span><strong>${pcvEscapeHtml(formatConsoleLabel(projection.noVncReasonCode))}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.path'))}</span><strong>${pcvEscapeHtml(projection.noVncPathOrReason)}</strong></div>
      <div class="diagnostics-fact"><span class="muted">${pcvEscapeHtml(t('vm.console.panel.selected'))}</span><strong>${pcvEscapeHtml(selectedVm || '-')}</strong></div>
    </div>
    <div class="diagnostics-actions">
      <button type="button" data-action="console-open-selected"${disabled}>${pcvEscapeHtml(t('vm.console.panel.open'))}</button>
      <span class="muted">${pcvEscapeHtml(projection.nextAction)}</span>
    </div>
    ${sessionHtml}
    ${errorHtml}
    <div class="boundary-chip-row">
      <span>${pcvEscapeHtml(t('vm.console.panel.chip_linux'))}</span>
      <span>${pcvEscapeHtml(t('vm.console.panel.chip_mutation'))}</span>
      <span>${pcvEscapeHtml(t('vm.console.panel.chip_target'))}</span>
      <span>${pcvEscapeHtml(t('vm.console.panel.chip_cli'))}</span>
      <span>${pcvEscapeHtml(t('vm.console.panel.chip_contract', { contract: projection.contract }))}</span>
    </div>
  </div>`;
}

function getBetaFollowupItems() {
  const configuredItems = asArray(readNested(state.opsSummary || {}, ['beta_followup', 'items']));
  if (configuredItems.length) {
    return configuredItems.map((item) => ({
      label: item.label || item.name || t('vm.beta.followup'),
      status: item.status || item.state || t('vm.beta.tracked'),
      detail: item.detail || item.evidence || ''
    }));
  }

  return [1, 2, 3, 4, 5, 6, 7].map((index) => ({
    label: t(`vm.beta.item${index}.label`),
    status: t(`vm.beta.item${index}.status`),
    detail: t(`vm.beta.item${index}.detail`)
  }));
}

function renderBetaFollowup() {
  if (!els.betaFollowupPanel) return;

  const items = getBetaFollowupItems();
  els.betaFollowupPanel.innerHTML = `<div class="diagnostics-card beta-followup-card">
    <div class="diagnostics-header">
      <div>
        <span class="muted">${pcvEscapeHtml(t('vm.beta.eyebrow'))}</span>
        <strong>${pcvEscapeHtml(t('vm.beta.title'))}</strong>
      </div>
      <span class="status-badge ok">${pcvEscapeHtml(t('vm.beta.tracked'))}</span>
    </div>
    <div class="triage-list">
      ${items.map((item) => `<div class="triage-row">
        <div>
          <strong>${pcvEscapeHtml(item.label)}</strong>
          <p>${pcvEscapeHtml(item.detail || t('vm.beta.fallback'))}</p>
        </div>
        <span class="status-badge ${String(item.status).toLowerCase().includes('not') || String(item.status).toLowerCase().includes('blocked') ? 'warn' : 'ok'}">${pcvEscapeHtml(item.status)}</span>
      </div>`).join('')}
    </div>
    <div class="boundary-chip-row">
      <span>${pcvEscapeHtml(t('vm.beta.chip_host'))}</span>
      <span>${pcvEscapeHtml(t('vm.beta.chip_token'))}</span>
      <span>${pcvEscapeHtml(t('vm.beta.chip_public'))}</span>
    </div>
  </div>`;
}

function renderTroubleshooting() {
  if (!els.troubleshootingPanel) return;
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const cards = [
    [t('vm.trouble.host'), host.supported === false ? t('vm.trouble.host_bad') : t('vm.trouble.host_ok'), t('vm.trouble.host_detail')],
    [t('vm.trouble.vmms'), formatPolicyValue(readNested(host, ['hyperv', 'vmms_running'])), t('vm.trouble.vmms_detail')],
    [t('vm.trouble.listener'), formatPolicyValue(readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind'])), t('vm.trouble.listener_detail')],
    [t('vm.trouble.token'), formatPolicyValue(readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage'])), t('vm.trouble.token_detail')],
    [t('vm.trouble.jobs'), formatPolicyValue(readNested(policy, ['job_runtime', 'state_store', 'persistence'])), t('vm.trouble.jobs_detail')],
    [t('vm.trouble.boundary'), t('vm.trouble.boundary_value'), t('vm.trouble.boundary_detail')]
  ];
  const errors = [
    ['PCV_AUTH_REQUIRED', t('vm.trouble.err_auth')],
    ['PCV_JOB_STORE_SCHEMA_UNSUPPORTED', t('vm.trouble.err_schema')],
    ['PCV_VM_NOT_MANAGED_BY_PURECVISOR', t('vm.trouble.err_managed')],
    ['PCV_VM_SHUTDOWN_NOT_AVAILABLE', t('vm.trouble.err_shutdown')]
  ];

  const evidenceHtml = renderTroubleshootingEvidence();
  const codeHtml = `<div class="code-list">
      ${errors.map(([code, detail]) => `<div class="kv"><span>${pcvEscapeHtml(code)}</span><strong>${pcvEscapeHtml(detail)}</strong></div>`).join('')}
    </div>`;
  const gridHtml = `<div class="troubleshooting-grid">
      ${cards.map(([title, value, detail]) => `<div class="troubleshooting-card"><span class="muted">${pcvEscapeHtml(title)}</span><strong>${pcvEscapeHtml(value)}</strong><p>${pcvEscapeHtml(detail)}</p></div>`).join('')}
    </div>`;
  const skipped = typeof pcvLegacyViewSkipped === 'function' && pcvLegacyViewSkipped('troubleshooting');
  els.troubleshootingPanel.innerHTML = skipped ? `${evidenceHtml}${codeHtml}` : `${evidenceHtml}${gridHtml}${codeHtml}`;
}

// --- legacy vm-detail-extensions.ts ---
// VM detail 의 lifecycle action 표와 확장 form action 이다. web/src/bootstrap.ts 의
// els.vmDetailPanel submit listener 가 처리하지 않은 data-action 은 handleVmDetailExtensionSubmit 으로 온다.
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

async function askConsoleConfirm(opts) {
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return false;
  return PCV.components.confirmDialog({
    title: opts.title,
    message: opts.message,
    confirmLabel: opts.confirmLabel,
    cancelLabel: t('ui.confirm.cancel'),
    danger: opts.danger === true
  });
}

async function queueVmRename(vmId, newName) {
  requireRbac('operate', t('vm.rbac.rename'));
  const target = String(newName || '').trim();
  if (!target) {
    throw normalizeError({
      code: 'PCV_VM_RENAME_TARGET_REQUIRED',
      message: t('vm.err.rename_required'),
      detail: t('vm.err.rename_required_detail')
    });
  }
  const accepted = await askConsoleConfirm({
    title: t('vm.confirm.rename.title'),
    message: t('vm.confirm.rename.message', { name: vmId, target }),
    confirmLabel: t('vm.confirm.rename.confirm'),
    danger: false
  });
  if (!accepted) return;

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

const VM_DETAIL_EXTENSION_CLICK_ACTIONS = new Set(['checkpoint-schedule-clear', 'vm-console-frame-start', 'vm-console-frame-stop', 'vm-console-input-cad', 'vm-console-fit', 'vm-console-actual', 'vm-console-fullscreen']);

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
  const interval = payload && payload.interval_minutes != null ? String(payload.interval_minutes) : '-';
  const retention = payload && payload.retention_max != null ? String(payload.retention_max) : '-';
  return mode === 'clear'
    ? t('vm.checkpoint.schedule.clear_message')
    : t('vm.checkpoint.schedule.save_message', { interval, retention });
}

async function confirmCheckpointScheduleChange(mode, payload) {
  if (!PCV.components || typeof PCV.components.confirmDialog !== 'function') return false;
  const clear = mode === 'clear';
  const interval = payload && payload.interval_minutes != null ? String(payload.interval_minutes) : '-';
  const retention = payload && payload.retention_max != null ? String(payload.retention_max) : '-';
  return PCV.components.confirmDialog({
    title: clear ? t('vm.checkpoint.schedule.clear_title') : t('vm.checkpoint.schedule.save_title'),
    message: clear
      ? t('vm.checkpoint.schedule.clear_message')
      : t('vm.checkpoint.schedule.save_message', { interval, retention }),
    confirmLabel: clear ? t('vm.checkpoint.schedule.clear') : t('vm.checkpoint.schedule.save'),
    cancelLabel: t('ui.confirm.cancel'),
    danger: false
  });
}

function rememberCheckpointScheduleDraft(vmId, mode, payload) {
  if (mode === 'clear') {
    if (state.checkpointScheduleDraft && String(state.checkpointScheduleDraft.vm_id) === String(vmId)) {
      state.checkpointScheduleDraft = null;
    }
    return;
  }
  if (!payload || typeof payload !== 'object') return;
  if (payload.interval_minutes == null && payload.retention_max == null) return;
  state.checkpointScheduleDraft = {
    vm_id: vmId,
    interval_minutes: payload.interval_minutes ?? null,
    retention_max: payload.retention_max ?? null
  };
}

async function queueCheckpointScheduleControl(vmId, mode, payload) {
  requireRbac('operate', t('vm.rbac.schedule', { mode: mode === 'clear' ? t('vm.mode.clear') : mode === 'set' ? t('vm.mode.set') : t('vm.mode.preview') }));
  rememberCheckpointScheduleDraft(vmId, mode, payload);
  if (mode !== 'preview') {
    const accepted = await confirmCheckpointScheduleChange(mode, payload);
    if (!accepted) return;
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
    } else if (typeof watchCheckpointSurface === 'function') {
      watchCheckpointSurface(vmId, result);
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

function checkpointSchedulePreviewSummary(result) {
  const record = result && typeof result === 'object' ? result : null;
  const minutes = record ? record.interval_minutes : null;
  const retention = record ? record.retention_max : null;
  if (minutes == null && retention == null) {
    return typeof formatObjectValue === 'function' ? formatObjectValue(result) : '';
  }
  return t('vm.checkpoint.schedule.preview_summary', {
    minutes: minutes == null ? '-' : String(minutes),
    retention: retention == null ? '-' : String(retention)
  });
}

function renderCheckpointSchedulePreview(vmId) {
  const preview = state.checkpointSchedulePreview;
  if (!preview || preview.vm_id !== vmId) {
    return `<p class="muted">${pcvEscapeHtml(t('vm.checkpoint.schedule.preview_empty'))}</p>`;
  }
  const updated = preview.updated_at ? new Date(preview.updated_at).toLocaleString() : '-';
  const body = checkpointSchedulePreviewSummary(preview.result);
  return `<p class="muted checkpoint-schedule-preview">${pcvEscapeHtml(t('vm.checkpoint.schedule.preview_ready', { time: updated, body }))}</p>`;
}

function readExportImportPayload(kind, data) {
  const directory = String(data.get('directory') || '').trim();
  return kind === 'export'
    ? { directory }
    : { name: String(data.get('name') || '').trim(), directory, has_vmcx: data.get('has_vmcx') === 'on' };
}

function buildExportImportConfirmation(vmId, kind, payload) {
  return kind === 'export'
    ? t('vm.confirm.export.message', { name: vmId, directory: String(payload?.directory || '') })
    : t('vm.confirm.import.message', { directory: String(payload?.directory || ''), target: String(payload?.name || '') });
}

async function queueVmExportImportControl(vmId, kind, mode, payload) {
  requireRbac('operate', t('vm.rbac.transfer', {
    kind: kind === 'export' ? t('vm.kind.export') : t('vm.kind.import'),
    mode: mode === 'apply' ? t('vm.mode.apply') : t('vm.mode.preview')
  }));
  const apply = mode === 'apply';
  if (apply) {
    const importing = kind === 'import';
    const accepted = await askConsoleConfirm({
      title: importing ? t('vm.confirm.import.title') : t('vm.confirm.export.title'),
      message: buildExportImportConfirmation(vmId, kind, payload),
      confirmLabel: importing ? t('vm.confirm.import.confirm') : t('vm.confirm.export.confirm'),
      danger: importing
    });
    if (!accepted) return;
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
    return `<p class="muted">${pcvEscapeHtml(t('vm.export.preview_empty'))}</p>`;
  }
  const updated = preview.updated_at ? new Date(preview.updated_at).toLocaleString() : '-';
  const kindLabel = preview.kind === 'import' ? t('vm.kind.import') : t('vm.kind.export');
  return `<p class="muted">${pcvEscapeHtml(t('vm.export.preview_ready', { kind: kindLabel, time: updated, body: formatObjectValue(preview.result) }))}</p>`;
}

function renderSwitchOptions() {
  const inventory = state.networkInventory || {};
  const switches = asArray(inventory.switches || inventory.items || inventory.networks)
    .map((item) => String(item?.name || '').trim())
    .filter(Boolean);
  return [`<option value="">${pcvEscapeHtml(t('vm.export.select_switch'))}</option>`]
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
  if (kind === 'connect') {
    return t('vm.confirm.net_connect.message', { name: vmId, switchName: String(payload?.switch || '') });
  }
  const device = payload && payload.device === 'dvd' ? t('vm.confirm.device.dvd') : t('vm.confirm.device.nic');
  return t('vm.confirm.device_add.message', { name: vmId, device });
}

async function queueVmNetworkChange(vmId, kind, payload) {
  requireRbac('operate', kind === 'connect' ? t('vm.rbac.switch_connect') : t('vm.rbac.device_add'));
  const connect = kind === 'connect';
  const accepted = await askConsoleConfirm({
    title: connect ? t('vm.confirm.net_connect.title') : t('vm.confirm.device_add.title'),
    message: buildVmNetworkChangeConfirmation(vmId, kind, payload),
    confirmLabel: connect ? t('vm.confirm.net_connect.confirm') : t('vm.confirm.device_add.confirm'),
    danger: connect
  });
  if (!accepted) return;

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
  requireRbac(kind === 'exec' ? 'guest.exec' : 'guest.channel.configure', kind === 'exec' ? t('vm.guestui.preview_exec') : t('vm.guestui.preview_channel'));
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
    if (button.disabled || !rbacAllows('console.input')) return;
    await sendVmConsoleKeyboard(button.dataset.vmId, { kind: 'ctrl-alt-del' });
  } else if (button.dataset.action === 'vm-console-fit') {
    writeVmConsolePref({ fit: 'fit' });
    applyVmConsoleDisplay();
  } else if (button.dataset.action === 'vm-console-actual') {
    writeVmConsolePref({ fit: 'actual' });
    applyVmConsoleDisplay();
  } else if (button.dataset.action === 'vm-console-fullscreen') {
    await requestVmConsoleFullscreen();
  }
}

// S1 browser console (design pcv-s1-browser-console-v1 §6): the frame route returns a zlib-compressed RGB565 frame;
// the browser inflates it with DecompressionStream('deflate') and paints it on a canvas. Polling repaints only the canvas.
const VM_CONSOLE_FRAME_SIZES = ['1024x768', '800x600', '640x480'];
const VM_CONSOLE_FRAME_FPS = [1, 2, 5];
const VM_CONSOLE_DEFAULT_SIZE = '1024x768';

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

function readVmConsolePref() {
  const pref = { fit: 'fit', size: VM_CONSOLE_DEFAULT_SIZE, fps: 2 };
  try {
    const storedFit = localStorage.getItem('pcv-console-fit');
    if (storedFit === 'fit' || storedFit === 'actual') pref.fit = storedFit;
    const storedSize = localStorage.getItem('pcv-console-size');
    if (VM_CONSOLE_FRAME_SIZES.includes(storedSize)) pref.size = storedSize;
    const storedFps = Number(localStorage.getItem('pcv-console-fps'));
    if (VM_CONSOLE_FRAME_FPS.includes(storedFps)) pref.fps = storedFps;
  } catch (err) {
    return { fit: 'fit', size: VM_CONSOLE_DEFAULT_SIZE, fps: 2 };
  }
  return pref;
}

function writeVmConsolePref(partial) {
  const next = partial || {};
  try {
    if (next.fit === 'fit' || next.fit === 'actual') localStorage.setItem('pcv-console-fit', next.fit);
    if (VM_CONSOLE_FRAME_SIZES.includes(next.size)) localStorage.setItem('pcv-console-size', next.size);
    const fps = Number(next.fps);
    if (VM_CONSOLE_FRAME_FPS.includes(fps)) localStorage.setItem('pcv-console-fps', String(fps));
  } catch (err) {
    return false;
  }
  return true;
}

function consoleDisplayScale(mode, frameWidth, frameHeight, containerWidth, viewportHeight) {
  const width = Number(frameWidth) || 0;
  const height = Number(frameHeight) || 0;
  const container = Number(containerWidth) || 0;
  const viewport = Number(viewportHeight) || 0;
  if (mode === 'actual') return 1;
  if (!(width > 0) || !(container > 0)) return 1;
  let scale = container / width;
  if (viewport > 0 && height > 0) scale = Math.min(scale, viewport / height);
  return scale;
}

function vmConsoleStageVisible() {
  if (document.hidden) return false;
  const canvas = document.getElementById('vm-console-frame-canvas');
  if (!canvas) return false;
  let node = canvas;
  while (node && node.nodeType !== 9) {
    if (node.hidden) return false;
    if (node.classList && typeof node.classList.contains === 'function' && node.classList.contains('hidden')) return false;
    node = node.parentNode;
  }
  return true;
}

function vmConsoleFrameStatusText(vm, control, running, size, fps) {
  if (control?.error) return `${control.error.code}: ${control.error.message}`;
  if (running) return t('vm.console.status.streaming', { size, fps });
  const vmState = String(vm?.state || vm?.status || '').toLowerCase();
  if (vmState === 'running') return t('vm.console.status.paused');
  return t('vm.console.status.not_running');
}

function consoleFullscreenSupported() {
  try {
    const root = document.documentElement;
    return Boolean(document.fullscreenEnabled && root && typeof root.requestFullscreen === 'function');
  } catch (err) {
    return false;
  }
}

function renderVmConsoleInputChip() {
  if (!rbacAllows('console.input')) {
    return `<span id="pcv-console-input-chip" class="pcv-console-chip" data-state="denied">${pcvEscapeHtml(t('vm.console.input.denied'))}</span>`;
  }
  return `<span id="pcv-console-input-chip" class="pcv-console-chip" data-state="idle">${pcvEscapeHtml(t('vm.console.input.idle'))}</span>`;
}

function applyVmConsoleDisplay() {
  const stage = document.getElementById('pcv-console-stage');
  const canvas = document.getElementById('vm-console-frame-canvas');
  const pref = readVmConsolePref();
  const fullscreen = Boolean(stage && document.fullscreenElement === stage);
  const mode = fullscreen ? 'fit' : pref.fit;
  const frameWidth = Number(canvas?.width) || 0;
  const frameHeight = Number(canvas?.height) || 0;
  const containerWidth = fullscreen
    ? (window.innerWidth || stage.clientWidth || stage.offsetWidth || 0)
    : (stage ? (stage.clientWidth || stage.offsetWidth || 0) : 0);
  const viewportHeight = window.innerHeight || 0;
  const scale = consoleDisplayScale(mode, frameWidth, frameHeight, containerWidth, viewportHeight);
  if (canvas && frameWidth > 0 && frameHeight > 0) {
    canvas.style.width = `${Math.max(1, Math.round(frameWidth * scale))}px`;
    canvas.style.height = `${Math.max(1, Math.round(frameHeight * scale))}px`;
  }
  if (stage && stage.classList && typeof stage.classList.toggle === 'function') {
    stage.classList.toggle('pcv-console-actual', mode === 'actual');
  }
  const fitButton = document.getElementById('pcv-console-fit');
  const actualButton = document.getElementById('pcv-console-actual');
  if (fitButton && typeof fitButton.setAttribute === 'function') fitButton.setAttribute('aria-pressed', mode === 'fit' ? 'true' : 'false');
  if (actualButton && typeof actualButton.setAttribute === 'function') actualButton.setAttribute('aria-pressed', mode === 'actual' ? 'true' : 'false');
  const control = state && state.vmConsoleFrame;
  if (control) control.displayScale = scale;
  const sizeText = frameWidth > 0 && frameHeight > 0 ? `${frameWidth}x${frameHeight}` : (control?.size || pref.size);
  const sizeEl = document.getElementById('pcv-console-size-label');
  if (sizeEl) sizeEl.textContent = t('vm.console.footer.size', { size: sizeText });
  const scaleEl = document.getElementById('pcv-console-scale');
  if (scaleEl) scaleEl.textContent = t('vm.console.footer.scale', { scale: Math.round(scale * 100) });
  const ageEl = document.getElementById('pcv-console-age');
  if (ageEl) {
    if (control?.updatedAt) {
      const seconds = Math.max(0, Math.round((Date.now() - control.updatedAt) / 1000));
      ageEl.textContent = t('vm.console.footer.age', { seconds });
    } else {
      ageEl.textContent = t('vm.console.footer.age_none');
    }
  }
  return scale;
}

async function requestVmConsoleFullscreen() {
  const stage = document.getElementById('pcv-console-stage');
  const button = document.getElementById('pcv-console-fullscreen');
  const note = document.getElementById('pcv-console-fullscreen-note');
  if (!stage || typeof stage.requestFullscreen !== 'function') {
    if (button) button.disabled = true;
    if (note) note.textContent = t('vm.console.fullscreen.denied');
    return;
  }
  try {
    await stage.requestFullscreen();
    applyVmConsoleDisplay();
  } catch (err) {
    if (button) button.disabled = true;
    if (note) note.textContent = t('vm.console.fullscreen.denied');
  }
}

function syncVmConsoleInputChip() {
  const chip = document.getElementById('pcv-console-input-chip');
  if (!chip) return;
  const canvas = document.getElementById('vm-console-frame-canvas');
  if (!rbacAllows('console.input')) {
    chip.dataset.state = 'denied';
    chip.textContent = t('vm.console.input.denied');
    return;
  }
  if (canvas && document.activeElement === canvas) {
    chip.dataset.state = 'active';
    chip.textContent = t('vm.console.input.active');
    return;
  }
  chip.dataset.state = 'idle';
  chip.textContent = t('vm.console.input.idle');
}

function onConsoleChromeClick(event) {
  const target = event && event.target;
  const button = target && typeof target.closest === 'function' ? target.closest('button[data-action]') : target;
  if (!button || !button.dataset) return;
  const action = button.dataset.action;
  if (action !== 'vm-console-fit' && action !== 'vm-console-actual' && action !== 'vm-console-fullscreen') return;
  if (event && typeof event.stopPropagation === 'function') event.stopPropagation();
  return handleVmDetailExtensionClick(button);
}

function onConsoleSelectChange(event) {
  const select = event && event.target;
  if (!select) return;
  const kind = select.dataset ? (select.dataset.consoleFrame || '') : '';
  const attr = !kind && typeof select.getAttribute === 'function' ? select.getAttribute('data-console-frame') : kind;
  const control = state.vmConsoleFrame;
  if (attr === 'size') {
    const size = VM_CONSOLE_FRAME_SIZES.includes(select.value) ? select.value : VM_CONSOLE_DEFAULT_SIZE;
    writeVmConsolePref({ size });
    if (control && control.running) control.size = size;
    const parts = size.split('x');
    const canvas = document.getElementById('vm-console-frame-canvas');
    if (canvas && parts.length === 2) {
      canvas.width = Number(parts[0]);
      canvas.height = Number(parts[1]);
    }
    applyVmConsoleDisplay();
  } else if (attr === 'fps') {
    const fps = Number(select.value);
    if (!VM_CONSOLE_FRAME_FPS.includes(fps)) return;
    writeVmConsolePref({ fps });
    if (control && control.running) control.fps = fps;
  }
}

function onVmConsoleVisibilityChange() {
  const control = state.vmConsoleFrame;
  if (!control?.running) return;
  if (!vmConsoleStageVisible()) return;
  if (control.timer) window.clearTimeout(control.timer);
  control.timer = null;
  pollVmConsoleFrame(control.vm_id, control.generation);
}

function bindVmConsoleFrame() {
  const root = document.getElementById('pcv-console-card');
  if (root && root.dataset && root.dataset.pcvBound !== '1') {
    root.dataset.pcvBound = '1';
    if (typeof root.addEventListener === 'function') {
      root.addEventListener('click', onConsoleChromeClick);
      root.addEventListener('change', onConsoleSelectChange);
    }
  }
  const canvas = document.getElementById('vm-console-frame-canvas');
  if (canvas && canvas.dataset && canvas.dataset.pcvBound !== '1') {
    canvas.dataset.pcvBound = '1';
    if (typeof canvas.addEventListener === 'function') {
      canvas.addEventListener('focus', syncVmConsoleInputChip);
      canvas.addEventListener('blur', syncVmConsoleInputChip);
    }
  }
  if (!window.__pcvConsoleVisibilityBound) {
    window.__pcvConsoleVisibilityBound = true;
    if (document && typeof document.addEventListener === 'function') {
      document.addEventListener('visibilitychange', onVmConsoleVisibilityChange);
      document.addEventListener('fullscreenchange', applyVmConsoleDisplay);
    }
    if (typeof window.addEventListener === 'function') window.addEventListener('resize', applyVmConsoleDisplay);
  }
  ensureVmConsoleKeyBinding();
  applyVmConsoleDisplay();
  syncVmConsoleInputChip();
}

function renderVmConsoleFrameCard(vm, vmId, canViewConsole) {
  const control = getVmConsoleFrameControl(vmId);
  const pref = readVmConsolePref();
  const running = control?.running === true;
  const size = control?.size || pref.size;
  const fps = control?.fps || pref.fps;
  const mode = pref.fit === 'actual' ? 'actual' : 'fit';
  const [width, height] = String(size).split('x');
  const disabled = canViewConsole ? '' : ' disabled';
  const fullscreenDisabled = (!canViewConsole || !consoleFullscreenSupported()) ? ' disabled' : '';
  const status = vmConsoleFrameStatusText(vm, control, running, size, fps);
  const stageClass = mode === 'actual' ? 'pcv-console-stage pcv-console-actual' : 'pcv-console-stage';
  const footerAge = control?.updatedAt
    ? t('vm.console.footer.age', { seconds: Math.max(0, Math.round((Date.now() - control.updatedAt) / 1000)) })
    : t('vm.console.footer.age_none');
  const sizeOptions = VM_CONSOLE_FRAME_SIZES.map((item) => `<option value="${item}"${item === size ? ' selected' : ''}>${pcvEscapeHtml(item)}</option>`).join('');
  const fpsOptions = VM_CONSOLE_FRAME_FPS.map((item) => `<option value="${item}"${Number(item) === Number(fps) ? ' selected' : ''}>${pcvEscapeHtml(t('vm.console.fps_option', { fps: item }))}</option>`).join('');
  return `<div id="pcv-console-card" class="checkpoint-panel vm-console-frame-panel pcv-console">
      <div class="mini-section-header">
        <div>
          <p class="eyebrow">${pcvEscapeHtml(t('vm.console.eyebrow'))}</p>
          <h3>${pcvEscapeHtml(t('vm.console.title'))}</h3>
        </div>
      </div>
      <div class="pcv-console-toolbar diagnostics-actions">
        <button type="button" id="pcv-console-fit" data-action="vm-console-fit" aria-pressed="${mode === 'fit' ? 'true' : 'false'}"${disabled}>${pcvEscapeHtml(t('vm.console.fit'))}</button>
        <button type="button" id="pcv-console-actual" data-action="vm-console-actual" aria-pressed="${mode === 'actual' ? 'true' : 'false'}"${disabled}>${pcvEscapeHtml(t('vm.console.actual'))}</button>
        <select data-console-frame="size" aria-label="${pcvEscapeHtml(t('vm.console.size'))}"${disabled}>${sizeOptions}</select>
        <select data-console-frame="fps" aria-label="${pcvEscapeHtml(t('vm.console.fps'))}"${disabled}>${fpsOptions}</select>
        <button type="button" data-action="vm-console-frame-start" data-vm-id="${pcvEscapeHtml(vmId)}"${running ? ' disabled' : disabled}>${pcvEscapeHtml(t('vm.console.start'))}</button>
        <button type="button" data-action="vm-console-frame-stop" data-vm-id="${pcvEscapeHtml(vmId)}"${running ? '' : ' disabled'}>${pcvEscapeHtml(t('vm.console.pause'))}</button>
        <button type="button" id="pcv-console-fullscreen" data-action="vm-console-fullscreen"${fullscreenDisabled}>${pcvEscapeHtml(t('vm.console.fullscreen'))}</button>
        <button type="button" id="pcv-console-cad" data-action="vm-console-input-cad" data-vm-id="${pcvEscapeHtml(vmId)}"${rbacAllows('console.input') ? '' : ' disabled'}>${pcvEscapeHtml(t('vm.console.cad'))}</button>
        ${renderVmConsoleInputChip()}
      </div>
      <div id="pcv-console-stage" class="${stageClass}">
        <canvas id="vm-console-frame-canvas" class="vm-console-frame-canvas pcv-console-frame-canvas" tabindex="0" width="${pcvEscapeHtml(width)}" height="${pcvEscapeHtml(height)}"></canvas>
      </div>
      <p id="pcv-console-fullscreen-note" class="muted"></p>
      <p id="vm-console-frame-status" class="muted">${pcvEscapeHtml(status)}</p>
      ${renderVmConsoleInputControls(vmId)}
      <p class="pcv-console-footer muted">
        <span id="pcv-console-size-label">${pcvEscapeHtml(t('vm.console.footer.size', { size }))}</span>
        <span id="pcv-console-scale">${pcvEscapeHtml(t('vm.console.footer.scale', { scale: 100 }))}</span>
        <span id="pcv-console-age">${pcvEscapeHtml(footerAge)}</span>
        <span>${pcvEscapeHtml(t('vm.console.release'))}</span>
      </p>
      <p class="muted">${canViewConsole ? '' : pcvEscapeHtml(t('vm.console.need_view'))}</p>
      <p><button type="button" data-action="vm-console" data-vm-id="${pcvEscapeHtml(vmId)}"${disabled}>${pcvEscapeHtml(t('vm.console.vmconnect'))}</button></p>
    </div>`;
}

function paintVmConsoleFrame() {
  ensureVmConsoleKeyBinding();
  const control = state.vmConsoleFrame;
  const canvas = document.getElementById('vm-console-frame-canvas');
  if (!control?.image || !canvas) return;
  if (canvas.width !== control.image.width || canvas.height !== control.image.height) {
    canvas.width = control.image.width;
    canvas.height = control.image.height;
  }
  canvas.getContext('2d')?.putImageData(control.image, 0, 0);
  applyVmConsoleDisplay();
}

function setVmConsoleFrameStatus(text) {
  const element = document.getElementById('vm-console-frame-status');
  if (element) element.textContent = text;
}

function vmConsolePollDelay(control) {
  const fps = Number(control?.fps) || 2;
  return Math.max(1, Math.round(1000 / fps));
}

function scheduleVmConsolePoll(vmId, generation) {
  const control = state.vmConsoleFrame;
  if (!control || control.vm_id !== vmId || control.generation !== generation || !control.running) return;
  if (control.timer) window.clearTimeout(control.timer);
  control.timer = window.setTimeout(() => pollVmConsoleFrame(vmId, generation), vmConsolePollDelay(control));
}

function selectedVmMatchesConsole(vmId) {
  try {
    return getVmId(state.selectedVm || {}) === vmId;
  } catch (err) {
    return false;
  }
}

async function pollVmConsoleFrame(vmId, generation) {
  const control = state.vmConsoleFrame;
  if (!control || control.vm_id !== vmId || control.generation !== generation || !control.running) return;
  if (!selectedVmMatchesConsole(vmId)) {
    stopVmConsoleFrame();
    return;
  }
  if (!vmConsoleStageVisible()) {
    scheduleVmConsolePoll(vmId, generation);
    return;
  }
  try {
    const frame = await desktopApi.getVmConsoleFrame(vmId, control.size);
    const data = frame?.data || frame;
    const rgb565 = await inflateConsoleFrame(data.frame_base64);
    control.image = new ImageData(decodeRgb565ToRgba(rgb565, data.width, data.height), data.width, data.height);
    control.error = null;
    control.updatedAt = Date.now();
    paintVmConsoleFrame();
    setVmConsoleFrameStatus(t('vm.console.status.streaming', { size: control.size, fps: control.fps }));
  } catch (error) {
    const normalized = normalizeError(error);
    if (normalized.code !== 'PCV_CONSOLE_RATE_LIMITED') {
      control.error = normalized;
      control.running = false;
      if (control.timer) window.clearTimeout(control.timer);
      control.timer = null;
      render();
      return;
    }
  }
  scheduleVmConsolePoll(vmId, generation);
}

function startVmConsoleFrame(vmId) {
  requireRbac('console.view', t('vm.rbac.screen'));
  const panel = els.vmDetailPanel;
  const pref = readVmConsolePref();
  const sizeValue = panel?.querySelector('select[data-console-frame="size"]')?.value || pref.size;
  const fpsValue = Number(panel?.querySelector('select[data-console-frame="fps"]')?.value || pref.fps);
  const size = VM_CONSOLE_FRAME_SIZES.includes(sizeValue) ? sizeValue : VM_CONSOLE_DEFAULT_SIZE;
  const fps = VM_CONSOLE_FRAME_FPS.includes(fpsValue) ? fpsValue : 2;
  writeVmConsolePref({ size, fps });
  stopVmConsoleFrame();
  const generation = Date.now();
  state.vmConsoleFrame = { vm_id: vmId, size, fps, running: true, generation, image: null, error: null, timer: null, updatedAt: null, displayScale: 1 };
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
  requireRbac('console.input', t('vm.rbac.input'));
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
    return `<p class="muted">${pcvEscapeHtml(t('vm.console.input.readonly'))}</p>`;
  }
  return `<form class="vm-resource-form" data-action="vm-console-text" data-vm-id="${pcvEscapeHtml(vmId)}">
        <p class="muted">${pcvEscapeHtml(t('vm.console.text.help'))}</p>
        <input name="text" maxlength="256" autocomplete="off" placeholder="${pcvEscapeHtml(t('vm.console.text.placeholder'))}" aria-label="${pcvEscapeHtml(t('vm.console.text.label'))}">
        <button type="submit" data-action="vm-console-text">${pcvEscapeHtml(t('vm.console.text.send'))}</button>
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
window.consoleDisplayScale = consoleDisplayScale;
window.readVmConsolePref = readVmConsolePref;
window.writeVmConsolePref = writeVmConsolePref;
window.bindVmConsoleFrame = bindVmConsoleFrame;
window.syncVmConsoleInputChip = syncVmConsoleInputChip;
window.applyVmConsoleDisplay = applyVmConsoleDisplay;
window.requestVmConsoleFullscreen = requestVmConsoleFullscreen;
window.vmConsoleStageVisible = vmConsoleStageVisible;
window.VM_LIFECYCLE_ACTIONS = VM_LIFECYCLE_ACTIONS;
window.VM_DETAIL_EXTENSION_CLICK_ACTIONS = VM_DETAIL_EXTENSION_CLICK_ACTIONS;
window.VM_CONSOLE_FRAME_SIZES = VM_CONSOLE_FRAME_SIZES;
window.VM_CONSOLE_FRAME_FPS = VM_CONSOLE_FRAME_FPS;
window.VM_CONSOLE_VIRTUAL_KEYS = VM_CONSOLE_VIRTUAL_KEYS;
PCV.vmConsole = Object.assign(PCV.vmConsole || {}, { formatConsoleLabel, getConsoleAccessProjection, renderConsolePanel, getBetaFollowupItems, renderBetaFollowup, renderTroubleshooting, queueVmRename, readCheckpointSchedulePayload, buildCheckpointScheduleConfirmation, queueCheckpointScheduleControl, renderCheckpointSchedulePreview, readExportImportPayload, buildExportImportConfirmation, queueVmExportImportControl, renderExportImportPreview, renderSwitchOptions, readVmNetworkChangePayload, buildVmNetworkChangeConfirmation, queueVmNetworkChange, previewVmGuestExecutionControl, handleVmGuestPreviewSubmit, handleVmDetailExtensionSubmit, handleVmDetailExtensionClick, decodeRgb565ToRgba, inflateConsoleFrame, getVmConsoleFrameControl, renderVmConsoleFrameCard, paintVmConsoleFrame, setVmConsoleFrameStatus, pollVmConsoleFrame, startVmConsoleFrame, stopVmConsoleFrame, getVmConsoleVirtualKey, sendVmConsoleKeyboard, handleVmConsoleKeyEvent, ensureVmConsoleKeyBinding, renderVmConsoleInputControls, consoleDisplayScale, readVmConsolePref, writeVmConsolePref, bindVmConsoleFrame, syncVmConsoleInputChip, applyVmConsoleDisplay, requestVmConsoleFullscreen, vmConsoleStageVisible, VM_LIFECYCLE_ACTIONS, VM_DETAIL_EXTENSION_CLICK_ACTIONS, VM_CONSOLE_FRAME_SIZES, VM_CONSOLE_FRAME_FPS, VM_CONSOLE_VIRTUAL_KEYS });
})(window.PCV);

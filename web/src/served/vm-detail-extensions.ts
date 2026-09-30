// @ts-nocheck
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
    trackJob(job);
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

const VM_DETAIL_EXTENSION_CLICK_ACTIONS = new Set(['checkpoint-schedule-clear']);

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
      trackJob(result);
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
  return `<p class="muted">schedule preview ${escapeHtml(updated)}: ${escapeHtml(formatObjectValue(preview.result))}</p>`;
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
      trackJob(result);
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
  return `<p class="muted">${escapeHtml(preview.kind)} preview ${escapeHtml(updated)}: ${escapeHtml(formatObjectValue(preview.result))}</p>`;
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
  }
}

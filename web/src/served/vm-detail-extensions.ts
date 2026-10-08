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

function renderSwitchOptions() {
  const inventory = state.networkInventory || {};
  const switches = asArray(inventory.switches || inventory.items || inventory.networks)
    .map((item) => String(item?.name || '').trim())
    .filter(Boolean);
  return ['<option value="">Select switch</option>']
    .concat(switches.map((name) => `<option value="${escapeHtml(name)}">${escapeHtml(name)}</option>`))
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
          <button data-action="vm-console-frame-start" data-vm-id="${escapeHtml(vmId)}"${running ? ' disabled' : disabled}>Start screen</button>
          <button data-action="vm-console-frame-stop" data-vm-id="${escapeHtml(vmId)}"${running ? '' : ' disabled'}>Pause screen</button>
        </div>
      </div>
      <canvas id="vm-console-frame-canvas" class="vm-console-frame-canvas" tabindex="0" width="${escapeHtml(width)}" height="${escapeHtml(height)}" style="width:100%;max-width:${escapeHtml(width)}px;background:#000"></canvas>
      <p id="vm-console-frame-status" class="muted">${escapeHtml(status)}</p>
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
  return `<form class="vm-resource-form" data-action="vm-console-text" data-vm-id="${escapeHtml(vmId)}">
        <p class="muted">Click the screen to type into the VM. Keys go as press and release; passwords are not recorded.</p>
        <input name="text" maxlength="256" autocomplete="off" placeholder="Text to type (printable ASCII)" aria-label="console text">
        <button type="submit" data-action="vm-console-text">Send text</button>
        <button type="button" data-action="vm-console-input-cad" data-vm-id="${escapeHtml(vmId)}">Ctrl+Alt+Del</button>
      </form>`;
}

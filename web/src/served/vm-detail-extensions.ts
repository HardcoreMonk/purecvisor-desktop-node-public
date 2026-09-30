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

async function handleVmDetailExtensionSubmit(form, data) {
  if (form?.dataset.action === 'vm-rename') {
    await queueVmRename(form.dataset.vmId, data.get('new_name'));
    form.reset();
  }
}

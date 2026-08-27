// @ts-nocheck
async function queueVmLifecycle(vmId, action) {
  requireRbac('operate', `VM ${action}`);
  const requiresConfirmation = action === 'poweroff' || action === 'restart' || action === 'save' || action === 'resume-saved';
  if (requiresConfirmation && !window.confirm(buildVmLifecycleConfirmation(vmId, action))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, action);
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmAction(vmId, action);
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

async function queueVmAttach(vmId, isoPath) {
  requireRbac('operate', 'VM attach');
  const path = String(isoPath || '').trim();
  if (!path) {
    throw normalizeError({
      code: 'PCV_VM_ATTACH_ISO_REQUIRED',
      message: 'Enter an ISO path.',
      detail: 'iso_path is required before queueing vm.attach.'
    });
  }
  if (!window.confirm(buildVmAttachConfirmation(vmId, path))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'attach');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmAttach(vmId, path);
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

async function queueVmResourceMutation(vmId, action, valueName, rawValue) {
  requireRbac('operate', `VM ${action}`);
  const value = Number.parseInt(String(rawValue ?? ''), 10);
  if (!Number.isFinite(value)) {
    throw normalizeError({
      code: 'PCV_VM_RESOURCE_VALUE_REQUIRED',
      message: 'Enter a numeric VM resource value.',
      detail: `${valueName} must be an integer before queueing ${action}.`
    });
  }

  state.actionPending = true;
  setVmActionPending(vmId, action);
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmResourceMutation(vmId, action, { [valueName]: value });
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

function readRequiredText(formData, name, errorCode, message) {
  const value = String(formData.get(name) || '').trim();
  if (!value) {
    throw normalizeError({
      code: errorCode,
      message,
      detail: `${name} is required before queueing this operation.`
    });
  }

  return value;
}

function readNonNegativeInt(formData, name) {
  const value = Number.parseInt(String(formData.get(name) ?? ''), 10);
  if (!Number.isFinite(value) || value < 0) {
    throw normalizeError({
      code: 'PCV_VM_QOS_VALUE_INVALID',
      message: 'Enter a non-negative QoS value.',
      detail: `${name} must be an integer greater than or equal to zero.`
    });
  }

  return value;
}

function readBoundedInt(formData, name, min, max, errorCode, message) {
  const value = Number.parseInt(String(formData.get(name) ?? ''), 10);
  if (!Number.isFinite(value) || value < min || value > max) {
    throw normalizeError({
      code: errorCode,
      message,
      detail: `${name} must be an integer between ${min} and ${max}.`
    });
  }

  return value;
}

function readVmQosPayload(kind, formData) {
  if (kind === 'storage') {
    return {
      disk: readRequiredText(formData, 'disk', 'PCV_VM_QOS_STORAGE_DISK_REQUIRED', 'Enter a storage disk before previewing or applying QoS.'),
      maximum_iops: readNonNegativeInt(formData, 'maximum_iops'),
      minimum_iops: readNonNegativeInt(formData, 'minimum_iops')
    };
  }

  return {
    adapter: readRequiredText(formData, 'adapter', 'PCV_VM_QOS_NETWORK_ADAPTER_REQUIRED', 'Enter a network adapter before previewing or applying QoS.'),
    maximum_kbps: readNonNegativeInt(formData, 'maximum_kbps'),
    minimum_kbps: readNonNegativeInt(formData, 'minimum_kbps')
  };
}

function readVmGuestExecPayload(formData) {
  return {
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED',
      'Enter a protected credential reference before queueing guest execution.'),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      'Enter a guest execution timeout between 1 and 600 seconds.'),
    command: [
      readRequiredText(
        formData,
        'command',
        'PCV_GUEST_EXEC_COMMAND_REQUIRED',
        'Enter a guest command before queueing guest execution.')
    ]
  };
}

function readVmGuestChannelPayload(formData, mode) {
  if (mode === 'repair') {
    return { yes: true };
  }

  return {
    credential_ref: readRequiredText(
      formData,
      'credential_ref',
      'PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED',
      'Enter a protected credential reference before verifying the guest channel.'),
    timeout_sec: readBoundedInt(
      formData,
      'timeout_sec',
      1,
      600,
      'PCV_GUEST_EXEC_TIMEOUT_INVALID',
      'Enter a guest channel timeout between 1 and 600 seconds.')
  };
}

function buildVmQosConfirmation(vmId, kind, payload) {
  const target = kind === 'storage' ? payload.disk : payload.adapter;
  const maximum = kind === 'storage' ? payload.maximum_iops : payload.maximum_kbps;
  const minimum = kind === 'storage' ? payload.minimum_iops : payload.minimum_kbps;
  return [
    `Apply ${kind} QoS policy to VM ${vmId}?`,
    `Target: ${target}`,
    `Maximum: ${maximum}`,
    `Minimum: ${minimum}`,
    'This queues a Hyper-V host mutation using the ADR-0008 policy contract.',
    'Use preview first when changing a non-reset value.'
  ].join('\n');
}

function buildVmGuestExecutionConfirmation(vmId, mode) {
  return [
    `Queue ${mode} for VM ${vmId}?`,
    'This operation uses a protected credential reference and writes guest-execution-audit-v1 evidence.',
    'Raw stdout/stderr and credential values are not rendered in the Web Console.'
  ].join('\n');
}

async function queueVmQosDirectControl(vmId, kind, mode, payload) {
  requireRbac('operate', `VM ${kind} QoS ${mode}`);
  const apply = mode === 'apply';
  if (apply && !window.confirm(buildVmQosConfirmation(vmId, kind, payload))) {
    return;
  }

  const actionKey = `qos-${kind}-${mode}`;
  state.actionPending = true;
  setVmActionPending(vmId, actionKey);
  state.error = null;
  state.selectedVmQosControl = {
    vm_id: vmId,
    kind,
    mode,
    loading: true,
    updated_at: '',
    result: null,
    error: null
  };
  render();
  try {
    const result = kind === 'storage'
      ? apply
        ? await desktopApi.applyVmQosStorage(vmId, payload)
        : await desktopApi.previewVmQosStorage(vmId, payload)
      : apply
        ? await desktopApi.applyVmQosNetwork(vmId, payload)
        : await desktopApi.previewVmQosNetwork(vmId, payload);

    if (apply) {
      trackJob(result);
      startPolling();
    }

    state.selectedVmQosControl = {
      vm_id: vmId,
      kind,
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result,
      error: null
    };
    state.connectionState = 'connected';
    state.shellMessage = `VM ${kind} QoS ${mode} completed for ${vmId}.`;
  } catch (error) {
    const normalized = normalizeError(error);
    state.error = normalized;
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind,
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result: null,
      error: normalized
    };
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmGuestExecutionControl(vmId, mode, payload) {
  const requiredPermission = mode === 'exec' ? 'guest.exec' : 'guest.channel.configure';
  requireRbac(requiredPermission, `VM guest ${mode}`);
  if ((mode === 'exec' || mode === 'repair') && !window.confirm(buildVmGuestExecutionConfirmation(vmId, mode))) {
    return;
  }

  const actionKey = `guest-${mode}`;
  state.actionPending = true;
  setVmActionPending(vmId, actionKey);
  state.error = null;
  state.selectedVmQosControl = {
    vm_id: vmId,
    kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
    mode,
    loading: true,
    updated_at: '',
    result: null,
    error: null
  };
  render();
  try {
    const result = mode === 'exec'
      ? await desktopApi.queueVmGuestExec(vmId, payload)
      : mode === 'verify'
        ? await desktopApi.verifyVmGuestChannel(vmId, payload)
        : await desktopApi.ensureVmGuestChannel(vmId, payload);

    trackJob(result);
    startPolling();
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result,
      error: null
    };
    state.connectionState = 'connected';
    state.shellMessage = `VM guest ${mode} queued for ${vmId}.`;
  } catch (error) {
    const normalized = normalizeError(error);
    state.error = normalized;
    state.selectedVmQosControl = {
      vm_id: vmId,
      kind: mode === 'exec' ? 'guest-execution' : 'guest-channel',
      mode,
      loading: false,
      updated_at: new Date().toISOString(),
      result: null,
      error: normalized
    };
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function refreshVmDeleteStatus(vmId) {
  requireRbac('read', 'VM delete status');
  state.actionPending = true;
  setVmActionPending(vmId, 'delete-status');
  state.error = null;
  render();
  try {
    const status = await desktopApi.getVmDeleteStatus(vmId);
    state.shellMessage = `Delete status ${status?.name || vmId}: ${status?.status || 'unknown'}`;
    state.connectionState = 'connected';
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueVmManage(vmId) {
  requireRbac('operate', 'VM manage');
  const vm = state.selectedVm || findCachedVm(vmId);
  if (!window.confirm(buildVmManageConfirmation(vmId, vm))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'manage');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmManage(vmId, vmId);
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

async function queueVmClone(vmId, rawName) {
  requireRbac('operate', 'VM clone');
  const name = String(rawName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_VM_CLONE_NAME_REQUIRED',
      message: 'Enter a target VM name.',
      detail: 'name is required before queueing vm.clone.'
    });
  }

  const vm = state.selectedVm || findCachedVm(vmId);
  const payload = { confirm_name: vmId, name };

  state.actionPending = true;
  setVmActionPending(vmId, 'clone');
  state.error = null;
  render();
  try {
    const preview = await desktopApi.previewVmClone(vmId, payload);
    if (!window.confirm(buildVmCloneConfirmation(vmId, vm, name, preview))) {
      return;
    }

    const job = await desktopApi.queueVmClone(vmId, payload);
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

async function queueVmDelete(vmId) {
  requireRbac('operate', 'VM delete');
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmState = getVmState(vm);
  if (isRunningVmState(vmState)) {
    throw normalizeError({
      code: 'PCV_VM_DELETE_RUNNING_BLOCKED',
      message: 'Power off the VM before deleting it.',
      detail: 'The Web Console blocks delete for running VMs. Use Power off first, then queue Delete VM again.'
    });
  }
  if (!window.confirm(buildVmDeleteConfirmation(vmId, vm))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'delete');
  state.error = null;
  render();
  try {
    const job = await desktopApi.deleteVm(vmId);
    trackJob(job);
    state.connectionState = 'connected';
    await loadVms();
    await refreshSelectedVm();
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}

async function queueCheckpointCreate(vmId, checkpointName) {
  requireRbac('operate', 'checkpoint create');
  const name = String(checkpointName || '').trim();
  if (!name) {
    throw normalizeError({
      code: 'PCV_FORM_INVALID',
      message: 'Checkpoint name is required.',
      detail: 'Enter a checkpoint name before creating a checkpoint.'
    });
  }

  state.checkpointPending = true;
  setCheckpointActionPending(vmId, 'create', 'create');
  state.error = null;
  render();
  try {
    const job = await desktopApi.createCheckpoint(vmId, name);
    trackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, 'create');
  }
}

async function queueCheckpointRestore(vmId, checkpointId) {
  requireRbac('operate', 'checkpoint restore');
  if (!window.confirm(buildCheckpointRestoreConfirmation(vmId, checkpointId))) {
    return;
  }
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'restore');
  state.error = null;
  render();
  try {
    const job = await desktopApi.restoreCheckpoint(vmId, checkpointId);
    trackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}

async function queueCheckpointDelete(vmId, checkpointId) {
  requireRbac('operate', 'checkpoint delete');
  if (!window.confirm(buildCheckpointDeleteConfirmation(vmId, checkpointId))) {
    return;
  }
  state.checkpointPending = true;
  setCheckpointActionPending(vmId, checkpointId, 'delete');
  state.error = null;
  render();
  try {
    const job = await desktopApi.deleteCheckpoint(vmId, checkpointId);
    trackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } finally {
    state.checkpointPending = false;
    clearCheckpointActionPending(vmId, checkpointId);
  }
}


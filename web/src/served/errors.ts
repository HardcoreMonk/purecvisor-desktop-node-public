function escapeHtml(value: unknown): string {
  return String(value ?? '-')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function asArray(value: any): any[] {
  if (Array.isArray(value)) return value;
  if (Array.isArray(value?.vms)) return value.vms;
  if (Array.isArray(value?.items)) return value.items;
  if (Array.isArray(value?.bundles)) return value.bundles;
  if (Array.isArray(value?.checkpoints)) return value.checkpoints;
  if (Array.isArray(value?.jobs)) return value.jobs;
  if (value && typeof value === 'object') {
    return Object.values(value).filter((item) => item && typeof item === 'object');
  }
  return [];
}

function getVmId(vm: any): string {
  return String(vm?.id || vm?.name || '');
}

function getVmName(vm: any): string {
  return String(vm?.name || vm?.id || 'Unknown VM');
}

function getVmState(vm: any): string {
  return String(vm?.state || vm?.status || '').trim();
}

function isRunningVmState(value: unknown): boolean {
  return String(value || '').toLowerCase().includes('running');
}

function buildVmManageConfirmation(vmId: string, vm: any): string {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `Manage VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'After success this VM will pass PureCVisor managed delete.',
    'Unmanaged delete refusal remains.',
    'This queues a Hyper-V Notes managed-marker mutation.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmCloneConfirmation(vmId: string, vm: any, targetName: string, preview: any): string {
  const sourceName = getVmName(vm);
  const plannedCopyBytes = preview?.planned_copy_bytes;
  const plannedCopyBytesText = plannedCopyBytes === null || plannedCopyBytes === undefined || plannedCopyBytes === ''
    ? '-'
    : String(plannedCopyBytes);
  return [
    `Clone VM ${sourceName} to ${targetName}?`,
    `Source: ${sourceName}`,
    `VM id: ${vmId}`,
    `Target name: ${targetName}`,
    `planned_copy_bytes: ${plannedCopyBytesText}`,
    '독립 VHDX를 복사한 새 managed VM을 만든다. 소스 VM은 변경하지 않는다.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmDeleteConfirmation(vmId: string, vm: any): string {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `Delete VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a destructive Hyper-V host mutation.',
    'Only PureCVisor-managed VMs can be deleted; unmanaged VMs are blocked by PCV_VM_NOT_MANAGED_BY_PURECVISOR.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmLifecycleConfirmation(vmId: string, action: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  const lines = [
    `${action} VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a Hyper-V host mutation.'
  ];
  if (action === 'save') {
    lines.push('Save is Hyper-V Saved, not pause.');
  }
  if (action === 'resume-saved') {
    lines.push('Resume saved is only valid when the current state is saved.');
  }

  lines.push('The result will appear in Tracked Jobs and Operator Activity.');
  return lines.join('\n');
}

function buildVmAttachConfirmation(vmId: string, isoPath: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  return [
    `Attach ISO to VM ${vmName}?`,
    `VM id: ${vmId}`,
    `ISO: ${isoPath}`,
    'This queues a Hyper-V DVD media mutation.',
    'The existing virtual DVD HostResource is replaced. No USB or new DVD device is created.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildCheckpointRestoreConfirmation(vmId: string, checkpointId: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  return [
    `Restore checkpoint ${checkpointId}?`,
    `VM: ${getVmName(vm)}`,
    `VM id: ${vmId}`,
    'This queues a Hyper-V checkpoint restore host mutation.',
    'Power-off-before-restore preconditions are enforced by the Local API and job result.',
    'The result will appear in Tracked Jobs and Operator Activity.'
  ].join('\n');
}

function buildCheckpointDeleteConfirmation(vmId: string, checkpointId: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  return [
    `Delete checkpoint ${checkpointId}?`,
    `VM: ${getVmName(vm)}`,
    `VM id: ${vmId}`,
    'This queues a destructive Hyper-V checkpoint host mutation.',
    'Only the selected checkpoint id is targeted.',
    'The result will appear in Tracked Jobs and Operator Activity.'
  ].join('\n');
}

function formatObjectValue(value: unknown): string {
  if (value === null || value === undefined || value === '') return '-';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

function formatErrorDetail(value: unknown): string {
  if (value === null || value === undefined || value === '') return '';
  if (typeof value !== 'object') return String(value);
  const parts = Object.entries(value as Record<string, unknown>)
    .filter(([, entry]) => entry !== null && entry !== undefined && entry !== '')
    .map(([key, entry]) => `${key}=${formatObjectValue(entry)}`);
  return parts.join(' / ') || JSON.stringify(value);
}

function flattenNamedList(value: unknown, keys: string[]): string {
  const items = asArray(value);
  if (items.length === 0) return '-';
  return items.map((item) => {
    if (!item || typeof item !== 'object') return String(item);
    const record = item as Record<string, unknown>;
    return keys
      .map((key) => record[key])
      .filter((part) => part !== null && part !== undefined && part !== '')
      .join(' / ');
  }).filter(Boolean).join(', ') || '-';
}

function formatConsoleValue(consoleInfo: any): unknown {
  if (!consoleInfo || typeof consoleInfo !== 'object') return consoleInfo;
  const parts = [];
  if (consoleInfo.type !== null && consoleInfo.type !== undefined && consoleInfo.type !== '') parts.push(consoleInfo.type);
  if (consoleInfo.available_local !== null && consoleInfo.available_local !== undefined && consoleInfo.available_local !== '') {
    parts.push(`local=${consoleInfo.available_local}`);
  }
  if (consoleInfo.mode !== null && consoleInfo.mode !== undefined && consoleInfo.mode !== '') parts.push(consoleInfo.mode);
  if (consoleInfo.available !== null && consoleInfo.available !== undefined && consoleInfo.available !== '') {
    parts.push(`available=${consoleInfo.available}`);
  }
  return parts.join(' / ') || consoleInfo;
}

function getCheckpointId(checkpoint: any): string {
  return String(checkpoint?.id || checkpoint?.name || checkpoint?.checkpoint_name || '');
}

function getCheckpointName(checkpoint: any): string {
  return String(checkpoint?.name || checkpoint?.checkpoint_name || checkpoint?.id || 'Unnamed checkpoint');
}

function formatCheckpointMeta(checkpoint: any): string {
  const parts = [
    checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created,
    checkpoint?.type,
    checkpoint?.notes
  ].filter((part) => part !== null && part !== undefined && part !== '');
  return parts.join(' / ') || '-';
}

function normalizeError(error: any): PcvNormalizedError {
  if (error?.normalized) return error;
  return {
    normalized: true,
    status: error?.status ?? 0,
    operation: error?.operation ?? 'web.request',
    code: error?.code ?? 'PCV_NETWORK_ERROR',
    message: error?.message ?? 'The Desktop Node API request failed.',
    detail: formatErrorDetail(error?.detail ?? error),
    retryable: Boolean(error?.retryable)
  };
}

function readResponseHeader(response: any, name: string): string | null {
  const headers = response?.headers;
  if (!headers) return null;
  if (typeof headers.get === 'function') {
    return headers.get(name);
  }

  return headers[name] ?? headers[name.toLowerCase()] ?? null;
}

function isProblemDetailsPayload(payload: any): boolean {
  return Boolean(
    payload &&
    typeof payload === 'object' &&
    !payload.error &&
    payload.code &&
    (payload.type || payload.title || payload.status)
  );
}

function normalizeProblemDetails(payload: any, response: Response): Partial<PcvNormalizedError> & Record<string, unknown> {
  const retryAfterSeconds = payload.retry_after_seconds ?? readResponseHeader(response, 'Retry-After');
  const metadata = [
    payload.request_id ? `request_id=${payload.request_id}` : null,
    retryAfterSeconds !== null && retryAfterSeconds !== undefined && retryAfterSeconds !== '' ? `retry_after_seconds=${retryAfterSeconds}` : null,
    payload.route_timeout_seconds !== null && payload.route_timeout_seconds !== undefined ? `route_timeout_seconds=${payload.route_timeout_seconds}` : null
  ].filter(Boolean).join(' / ');
  return {
    status: payload.status ?? response.status,
    operation: payload.operation,
    code: payload.code,
    message: payload.title || payload.message || 'The Desktop Node API request failed.',
    detail: [formatErrorDetail(payload.detail), metadata].filter(Boolean).join(' / '),
    retryable: payload.retryable
  };
}

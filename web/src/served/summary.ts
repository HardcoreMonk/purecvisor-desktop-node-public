// @ts-nocheck
function readSummaryValue(...paths) {
  const summary = state.opsSummary || {};
  for (const path of paths) {
    const value = readNested(summary, path);
    if (value !== null && value !== undefined && value !== '') return value;
  }
  return undefined;
}

function getSummaryVmCounts() {
  const vms = asArray(state.vms);
  const summaryCounts = state.opsSummary?.vm_counts || state.opsSummary?.vmCounts;
  const counts = summaryCounts || {};
  const total = Number(counts.total ?? counts.total_vms ?? vms.length);
  const runningFallback = vms.filter((vm) => isRunningVmState(vm.state || vm.status)).length;
  const running = Number(counts.running ?? counts.running_vms ?? runningFallback);
  const checkpointWarningsFallback = countVmCheckpointWarnings();
  const checkpointWarnings = Number(counts.checkpoint_warnings ?? counts.checkpointWarnings ?? checkpointWarningsFallback);
  return {
    total: Number.isFinite(total) ? total : vms.length,
    running: Number.isFinite(running) ? running : runningFallback,
    checkpoint_warnings: Number.isFinite(checkpointWarnings) ? checkpointWarnings : checkpointWarningsFallback
  };
}

function getSummaryJobCounts() {
  const rows = buildActivityRows();
  const summaryCounts = state.opsSummary?.job_counts || state.opsSummary?.jobCounts;
  if (summaryCounts) {
    const queued = Number(summaryCounts.queued ?? 0);
    const running = Number(summaryCounts.running ?? 0);
    const failed = Number(summaryCounts.failed ?? 0);
    return {
      active: (Number.isFinite(queued) ? queued : 0) + (Number.isFinite(running) ? running : 0),
      failed: Number.isFinite(failed) ? failed : 0
    };
  }
  const activeFallback = rows.filter(({ job }) => ['queued', 'running'].includes(String(job?.status || '').toLowerCase())).length;
  const failedFallback = rows.filter(({ job }) => String(job?.status || '').toLowerCase() === 'failed').length;
  return {
    active: activeFallback,
    failed: failedFallback
  };
}

function getRuntimeExposure() {
  return formatPolicyValue(
    readNested(state.opsSummary || {}, ['runtime_policy', 'network', 'current_exposure']) ||
    readNested(state.opsSummary || {}, ['runtime_policy', 'network', 'bind']) ||
    readNested(state.runtimePolicy || {}, ['network', 'current_exposure']) ||
    readNested(state.runtimePolicy || {}, ['network', 'bind']) ||
    'loopback'
  );
}

function getTokenPolicyLabel() {
  return formatPolicyValue(
    readNested(state.opsSummary || {}, ['runtime_policy', 'auth', 'token_storage']) ||
    readNested(state.opsSummary || {}, ['runtime_policy', 'token', 'storage']) ||
    readNested(state.runtimePolicy || {}, ['auth', 'token_storage']) ||
    readNested(state.runtimePolicy || {}, ['token', 'storage']) ||
    'unknown'
  );
}

// Readiness must never be inferred from absence. The old fallback keyed on
// `state.host?.supported === false`, which is also false when `state.host` is
// null, so an unauthenticated or unloaded console reported a healthy `Ready`.
// Every caller — metric grid, ops cockpit, hero chip, priority scan — depends
// on this helper, so the gate belongs here rather than at each call site.
function getHostReadinessLabel() {
  const summaryReadiness = hasRefreshedOperation('ops.summary')
    ? readSummaryValue(['host', 'readiness'], ['host', 'status'])
    : undefined;
  if (summaryReadiness) return formatPolicyValue(summaryReadiness);
  if (!hasRefreshedOperation('host.status') || !state.host) return '—';
  return formatPolicyValue(state.host.supported === false ? 'Needs attention' : 'Ready');
}

function normalizeSummaryIssue(issue, fallbackCode = 'PCV_OPS_SUMMARY_DEGRADED') {
  if (!issue || typeof issue !== 'object') {
    return {
      code: fallbackCode,
      message: String(issue || 'Ops summary returned a degraded signal.'),
      detail: ''
    };
  }
  return {
    code: issue.code || issue.key || issue.id || fallbackCode,
    message: issue.message || issue.label || issue.detail || 'Ops summary returned a degraded signal.',
    detail: issue.detail || issue.description || issue.operation || ''
  };
}

function summarySignalTone(signal) {
  return String(signal?.tone || signal?.status || signal?.severity || '').toLowerCase();
}

function summarySignalKey(signal) {
  return String(signal?.key || signal?.id || signal?.code || signal?.name || '').toLowerCase();
}

function formatErrorSummary(error) {
  return [
    `${error.code}: ${error.message}`,
    error.detail
  ].filter(Boolean).join(' / ');
}


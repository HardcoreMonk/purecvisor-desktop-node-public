// @ts-nocheck
function getDiagnosticActionPolicy(error = state.diagnosticBundleError) {
  const status = Number(error?.status || 0);
  const code = String(error?.code || '');
  const message = String(error?.message || '').toLowerCase();
  const pending = Boolean(state.pendingDiagnosticAction);
  const bundleId = getDiagnosticBundleId();
  const guideByStatus: Record<number, string> = {
    401: '401 auth required: enter a browser token and retry.',
    403: '403 auth forbidden: verify the token source before retrying.',
    404: '404 PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED: installed listener does not expose the diagnostic bundle API.',
    500: '500 server failure: keep the error details for support triage.'
  };
  const timedOut = status === 408 || status === 504 || code === 'PCV_ROUTE_TIMEOUT' || message.includes('timeout');
  const unsupported = status === 404 || code === 'PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED';
  const auth = status === 401 || status === 403 || isAuthError(error);
  const retryable = Boolean(error?.retryable || timedOut || status >= 500 || auth);
  const guide = timedOut
    ? 'timeout: retry after the listener is responsive or after the Retry-After hint.'
    : guideByStatus[status] || (code ? `${code}: review problem-details and retry when safe.` : 'Ready for authenticated create/download.');

  return {
    createDisabled: pending || unsupported,
    downloadDisabled: pending || !bundleId || unsupported,
    retryVisible: Boolean(error && retryable && !unsupported),
    statusLabel: pending || (unsupported ? 'API unsupported' : auth ? 'Auth required' : error ? 'Problem details' : 'API ready'),
    guide
  };
}

function buildPartialRefreshError(failures) {
  return normalizeError({
    code: 'PCV_PARTIAL_REFRESH_DEGRADED',
    message: 'Some Desktop Node API panels failed while the console stayed online.',
    detail: failures.map(formatErrorSummary).join(' | '),
    retryable: failures.some((failure) => failure.retryable)
  });
}

function collectRefreshFailures(stepFailures) {
  const failures = [];
  for (const failure of asArray(stepFailures)) {
    const normalized = failure?.normalized ? failure : normalizeError(failure);
    normalized.operation = normalized.operation || failure?.label || 'web.refresh';
    failures.push(normalized);
  }
  for (const localFailure of [state.summaryError, state.networkError, state.activityError]) {
    if (localFailure) failures.push(localFailure);
  }

  const seen = new Set();
  return failures.filter((failure) => {
    const key = `${failure.operation}:${failure.code}:${failure.detail}`;
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

function getPriorityItems() {
  const items = [];
  const seen = new Set();
  const pushUnique = (item, key = `${item.label}:${item.detail}`) => {
    const normalizedKey = String(key).toLowerCase();
    if (seen.has(normalizedKey)) return;
    seen.add(normalizedKey);
    items.push(item);
  };
  const hostReadiness = String(getHostReadinessLabel()).toLowerCase();
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  if (state.summaryError) {
    pushUnique({ tone: 'warn', label: 'Ops summary unavailable', detail: formatErrorSummary(state.summaryError) }, state.summaryError.code);
  }
  for (const error of asArray(state.opsSummary?.errors)) {
    const issue = normalizeSummaryIssue(error);
    pushUnique({ tone: 'error', label: 'Ops summary degraded', detail: `${issue.code}: ${issue.message}` }, issue.code);
  }
  for (const signal of asArray(state.opsSummary?.signals)) {
    const key = summarySignalKey(signal);
    const tone = summarySignalTone(signal);
    if (!key && !tone) continue;
    if (tone === 'ok' || tone === 'ready' || tone === 'healthy') continue;
    if (key === 'summary-errors' && asArray(state.opsSummary?.errors).length > 0) continue;
    const issue = normalizeSummaryIssue(signal, key || 'PCV_OPS_SUMMARY_SIGNAL');
    const priorityTone = tone === 'error' || tone === 'critical' || tone === 'fail' || tone === 'failed' ? 'error' : 'warn';
    pushUnique({ tone: priorityTone, label: issue.message || 'Ops summary signal', detail: issue.detail || issue.code }, issue.code);
  }
  if (hostReadiness.includes('need') || hostReadiness.includes('fail') || hostReadiness.includes('error')) {
    pushUnique({ tone: 'error', label: 'Host readiness needs attention', detail: 'Check Hyper-V support, admin context, VMMS, and Default Switch state.' }, 'host-readiness');
  }
  if (jobCounts.failed > 0) {
    pushUnique({ tone: 'error', label: 'Failed jobs', detail: `${jobCounts.failed} failed job(s) need review.` }, 'failed-jobs');
  }
  if (vmCounts.checkpoint_warnings > 0) {
    pushUnique({ tone: 'warn', label: 'Checkpoint warnings', detail: `${vmCounts.checkpoint_warnings} VM checkpoint warning(s) need review.` }, 'checkpoint-warnings');
  }
  if (String(getRuntimeExposure()).toLowerCase().includes('lan')) {
    pushUnique({ tone: 'warn', label: 'LAN exposure', detail: 'Confirm explicit LAN approval and token source proof.' }, 'lan-exposure');
  }
  return items;
}

function renderMetrics() {
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  // getSummaryVmCounts() falls back to vms.length, so an unloaded inventory is
  // indistinguishable from a genuine zero. Job counts keep their number: they
  // also draw on browser-tracked jobs, which remain real without the server.
  const vmsLoaded = hasRefreshedOperation('vm.list');
  els.metricGrid.innerHTML = [
    ['Host', getHostReadinessLabel()],
    ['VMs', vmsLoaded ? vmCounts.total : '—'],
    ['Running', vmsLoaded ? vmCounts.running : '—'],
    ['Active Jobs', jobCounts.active]
  ].map(([label, value]) => `<div class="metric"><span class="muted">${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join('');
}

function renderHost() {
  const host = state.host || {};
  const entries = Object.entries(host).slice(0, 16);
  if (entries.length === 0) {
    els.hostDetails.innerHTML = '<p class="muted">Host status has not been loaded.</p>';
    return;
  }
  els.hostDetails.innerHTML = entries
    .map(([key, value]) => `<div class="kv"><span>${escapeHtml(key)}</span><strong>${escapeHtml(typeof value === 'object' ? JSON.stringify(value) : value)}</strong></div>`)
    .join('');
}

function renderOpsSummaryCard(card) {
  const [arrayLabel, arrayValue, arrayDetail] = Array.isArray(card) ? card : [];
  const label = card.label ?? arrayLabel;
  const valueHtml = card.valueHtml ?? escapeHtml(card.value ?? arrayValue);
  const detail = card.detail ?? arrayDetail;
  return `<div class="ops-summary-card"><span class="muted">${escapeHtml(label)}</span><strong>${valueHtml}</strong><p>${escapeHtml(detail)}</p></div>`;
}

function renderOpsCockpit() {
  const vmCounts = getSummaryVmCounts();
  const jobCounts = getSummaryJobCounts();
  const evidence = getBatchEvidence();
  const vmsLoaded = hasRefreshedOperation('vm.list');
  const cards = [
    ['Host readiness', getHostReadinessLabel(), 'Hyper-V support, admin context, and service/API availability.'],
    ['VMs total/running', vmsLoaded ? `${vmCounts.total} / ${vmCounts.running}` : '—', 'Inventory count from ops summary with local inventory fallback.'],
    ['Checkpoint warnings', vmsLoaded ? vmCounts.checkpoint_warnings : '—', 'VMs with checkpoint warning signals from ops summary.'],
    ['Jobs active/failed', `${jobCounts.active} / ${jobCounts.failed}`, 'Current server and browser-visible job activity.'],
    ['Exposure / token', `${getRuntimeExposure()} / ${getTokenPolicyLabel()}`, 'Loopback-first listener policy and token storage posture.'],
    {
      label: 'Latest evidence',
      valueHtml: renderEvidenceStatusBadge(evidence),
      detail: evidenceDashboardDetail(evidence)
    },
    {
      label: 'Current evidence',
      valueHtml: renderCurrentEvidenceStatusBadge(),
      detail: currentEvidenceDashboardDetail()
    }
  ];
  els.opsSummaryPanel.innerHTML = `
    <div class="ops-summary-grid">
      ${cards.map(renderOpsSummaryCard).join('')}
    </div>`;

  const priorityItems = getPriorityItems();
  if (priorityItems.length === 0) {
    els.priorityPanel.innerHTML = '<div class="priority-empty">No high-priority warnings.</div>';
    return;
  }
  els.priorityPanel.innerHTML = priorityItems.map((item) => `
    <div class="priority-item priority-${escapeHtml(item.tone)}">
      <strong>${escapeHtml(item.label)}</strong>
      <span>${escapeHtml(item.detail)}</span>
    </div>`).join('');
}


// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): jobs, activity, event center and evidence views (legacy render-jobs, render-activity, evidence).
// Top-level declarations stay on window.PCV so the module keeps the shared-scope behaviour of the classic-script bundle.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy render-jobs.ts ---
function getFilteredJobRows() {
  const allRows = buildActivityRows();
  const statusFilter = String(state.jobStatusFilter || 'all').toLowerCase();
  const statusFiltered = statusFilter === 'all'
    ? allRows
    : allRows.filter(({ job }) => String(job?.status || '').toLowerCase() === statusFilter);
  const queryFiltered = filterRowsByQuery(statusFiltered, state.jobFilter, ({ source, job }) => [
    source,
    job?.job_id,
    job?.operation,
    job?.status,
    job?.request_id,
    job?.correlation_id,
    job?.error?.code,
    job?.error?.message
  ].join(' '));
  return {
    allRows,
    rows: sortRowsByKey(queryFiltered, state.jobSort, {
      updated: ({ job }) => Date.parse(job?.updated_at || job?.created_at || '') || 0,
      status: ({ job }) => job?.status || '',
      operation: ({ job }) => job?.operation || '',
      source: ({ source }) => source || ''
    })
  };
}

function getJobCancelScope(job) {
  const status = String(job?.status || '').toLowerCase();
  const operation = String(job?.operation || job?.action || '').toLowerCase();
  const isGuestExecution = operation.includes('guest.exec') ||
    operation.includes('guest-exec') ||
    operation.includes('guest execution');
  return status === 'running' && isGuestExecution ? 'running-guest-execution' : 'job';
}

function formatJobCancelLabel(job) {
  return getJobCancelScope(job) === 'running-guest-execution'
    ? t('copy.job.cancel_guest')
    : t('copy.job.cancel');
}

function canReconcileVmMutation(job) {
  const operation = String(job?.operation || '').toLowerCase();
  return String(job?.status || '').toLowerCase() === 'failed' &&
    ['vm.rename', 'vm.delete', 'checkpoint.create', 'checkpoint.restore', 'vm.create', 'vm.shutdown', 'vm.restart', 'vm.qos.storage.set', 'vm.qos.network.set'].includes(operation) &&
    String(job?.error?.code || '').toUpperCase() === 'PCV_JOB_INTERRUPTED';
}

// --- legacy render-activity.ts ---
function normalizeJob(job) {
  return job && typeof job === 'object' && !Array.isArray(job) ? job : {};
}

function renderFailedJobTriageRows() {
  const failed = buildActivityRows()
    .map(({ source, job }) => ({ source, job: normalizeJob(job) }))
    .filter(({ job }) => String(job.status || '').toLowerCase() === 'failed')
    .slice(0, 5);
  if (!failed.length) {
    return `<p class="muted">${pcvEscapeHtml(t('copy.incident.no_failed'))}</p>`;
  }

  return `<div class="triage-list">
    ${failed.map(({ source, job }) => {
      const retryable = Boolean(job.error?.retryable);
      return `<div class="triage-row">
        <div>
          <strong>${pcvEscapeHtml(job.operation || job.job_id || 'job')}</strong>
          <span class="muted">${pcvEscapeHtml(job.job_id || source)}</span>
          <span class="muted">${pcvEscapeHtml(formatJobDetail(job))}</span>
        </div>
        <div>${stateBadge(job.status)} ${retryable ? '<span class="status-badge warn">retryable</span>' : ''}</div>
      </div>`;
    }).join('')}
  </div>`;
}

function renderIncidentCommand() {
  const failedJobs = buildActivityRows()
    .map(({ job }) => normalizeJob(job))
    .filter((job) => String(job?.status || '').toLowerCase() === 'failed');
  const retryableFailedJobs = failedJobs.filter((job) => Boolean(job.error?.retryable)).length;
  const evidenceIssues = collectEvidenceIssues();
  const priorityItems = getPriorityItems();
  const evidenceIssueHtml = evidenceIssues.length
    ? `<div class="triage-list">${evidenceIssues.map((issue) => `<div class="triage-row">
        <span>${pcvEscapeHtml(issue.code)}</span>
        <span class="status-badge ${pcvEscapeHtml(evidenceIssueTone(issue))}">${pcvEscapeHtml(issue.detail || 'issue')}</span>
      </div>`).join('')}</div>`
    : `<p class="muted">${pcvEscapeHtml(t('copy.incident.no_evidence'))}</p>`;
  const priorityHtml = priorityItems.length === 0
    ? `<p class="muted">${pcvEscapeHtml(t('copy.incident.no_priority'))}</p>`
    : priorityItems.map((item) => `<div class="priority-item priority-${pcvEscapeHtml(item.tone)}"><strong>${pcvEscapeHtml(item.label)}</strong><span>${pcvEscapeHtml(item.detail)}</span></div>`).join('');

  els.incidentPanel.innerHTML = `
    <div class="incident-grid">
      <div class="incident-card">
        <span class="muted">${pcvEscapeHtml(t('copy.incident.failed_jobs'))}</span>
        <strong>${pcvEscapeHtml(failedJobs.length)} failed / ${pcvEscapeHtml(retryableFailedJobs)} retryable</strong>
        ${renderFailedJobTriageRows()}
      </div>
      <div class="incident-card">
        <span class="muted">${pcvEscapeHtml(t('copy.incident.evidence'))}</span>
        <strong>${pcvEscapeHtml(evidenceIssues.length)}</strong>
        ${evidenceIssueHtml}
      </div>
      <div class="incident-card">
        <span class="muted">${pcvEscapeHtml(t('copy.incident.priority'))}</span>
        ${priorityHtml}
      </div>
    </div>`;
}

function normalizeEventTone(tone) {
  const value = String(tone || '').toLowerCase();
  if (value === 'error' || value === 'critical') return 'error';
  if (value === 'warn' || value === 'warning') return 'warn';
  if (value === 'ok' || value === 'success') return 'ok';
  return 'info';
}

function buildEventCenterItems() {
  const items = [];
  const push = (tone, label, detail, source = 'web') => {
    if (!label && !detail) return;
    items.push({
      tone: normalizeEventTone(tone),
      label: label || detail,
      detail: detail || '',
      source
    });
  };

  for (const error of [state.error, state.summaryError, state.activityError, state.networkError, state.diagnosticBundleError, state.authError, state.consoleError]) {
    if (error) push('error', error.code, `${error.message}${error.detail ? ` / ${error.detail}` : ''}`, error.operation || 'problem-details');
  }
  for (const failure of state.partialFailures || []) {
    push('warn', failure.code, formatErrorSummary(failure), failure.operation || 'partial-refresh');
  }
  for (const issue of collectEvidenceIssues()) {
    push(evidenceIssueTone(issue), issue.code, issue.message || issue.detail, 'batch-evidence');
  }
  for (const item of getPriorityItems()) {
    push(item.tone, item.label, item.detail, 'priority');
  }
  const rows = buildActivityRows();
  const activeJobs = rows.filter(({ job }) => ['queued', 'running'].includes(String(job?.status || '').toLowerCase()));
  const failedJobs = rows.filter(({ job }) => String(job?.status || '').toLowerCase() === 'failed');
  if (activeJobs.length > 0) push('warn', t('copy.mon.active_jobs'), t('copy.mon.active_detail', { count: activeJobs.length }), 'jobs');
  if (failedJobs.length > 0) push('error', t('copy.mon.failed_jobs'), t('copy.mon.failed_detail', { count: failedJobs.length }), 'jobs');
  if (!state.apiToken.trim()) push('info', t('copy.mon.browser_token'), t('copy.events.token_detail'), 'session');

  return items.slice(0, 12);
}

function getJobTime(job) {
  return job?.updated_at || job?.created_at || job?.canceled_at || '-';
}

function formatJobDetail(job) {
  if (job?.error?.code) {
    return `${job.error.code}: ${job.error.message || t('copy.job.failed_short')}`;
  }
  if (job?.result?.operation) {
    return `result=${job.result.operation}`;
  }
  if (job?.retry_of) {
    return `retry of ${job.retry_of}`;
  }
  return `attempt=${job?.attempt || 1}`;
}

function formatCorrelationValue(job) {
  return job?.request_id || job?.correlation_id || '-';
}

function getActivityRowsForDashboard() {
  const recentActivity = asArray(state.opsSummary?.recent_activity);
  if (recentActivity.length > 0) {
    return recentActivity.map((job) => ({ source: 'summary', job })).slice(0, 5);
  }
  return buildActivityRows().slice(0, 5);
}

function buildActivityRows() {
  const serverJobs = asArray(state.serverJobs);
  const serverIds = new Set(serverJobs.map((job) => job.job_id).filter(Boolean));
  const rows = serverJobs.map((job) => ({ source: 'server', job }));
  for (const job of state.trackedJobs) {
    if (!serverIds.has(job.job_id)) {
      rows.push({ source: 'browser', job });
    }
  }
  return rows.slice(0, JOB_HISTORY_LIMIT);
}

function renderDashboardActivity() {
  const rows = getActivityRowsForDashboard();
  if (rows.length === 0) {
    els.dashboardActivityPanel.innerHTML = '<p class="muted">No recent operator activity has been loaded.</p>';
    return;
  }

  els.dashboardActivityPanel.innerHTML = `
    <div class="mini-section-header">
      <div>
        <p class="eyebrow">Recent Activity</p>
        <h3>Operator Activity</h3>
      </div>
    </div>
    <div class="dashboard-activity-list">
      ${rows.map(({ source, job }) => `<div class="dashboard-activity-row">
        <div>
          <strong>${pcvEscapeHtml(job.operation || job.action || 'job')}</strong>
          <div class="muted">${pcvEscapeHtml(job.job_id || job.correlation_id || job.request_id || '-')}</div>
        </div>
        <div>${stateBadge(job.status || job.state || 'unknown')}</div>
        <div class="muted">${pcvEscapeHtml(getJobTime(job))}</div>
        <div>${pcvEscapeHtml(formatJobDetail(job))}</div>
        <div><span class="badge">${pcvEscapeHtml(source)}</span></div>
      </div>`).join('')}
    </div>`;
}

function renderActivity() {
  if (!els.activityPanel || !PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
  const el = PCV.uxlib.el;
  PCV.uxlib.clearEl(els.activityPanel);
  els.activityPanel.appendChild(el('p', { class: 'activity-jobs-hint' }, t('copy.activity.jobs_hint')));
  els.activityPanel.appendChild(el('a', {
    href: '#/jobs',
    class: 'btn',
    'data-view-link': 'jobs'
  }, t('copy.activity.jobs_link')));
}

function renderActivityPageSummary() {
  const page = state.serverJobPage;
  if (!page || typeof page !== 'object') return '';
  const retention = page.retention || {};
  const nextOffset = page.next_offset === null || page.next_offset === undefined ? 'none' : page.next_offset;
  const nextButton = nextOffset === 'none'
    ? ''
    : `<button type="button" data-action="load-next-jobs" data-next-offset="${pcvEscapeHtml(nextOffset)}">${pcvEscapeHtml(t('copy.jobs.load_next'))}</button>`;
  return `<div class="activity-warning activity-page-summary">
    <strong>${pcvEscapeHtml(t('copy.jobs.pagination'))}</strong>
    ${pcvEscapeHtml(t('copy.list.shown', { shown: page.returned ?? asArray(page).length, total: page.count ?? asArray(state.serverJobs).length }))}
    / limit=${pcvEscapeHtml(page.limit ?? 50)}
    / offset=${pcvEscapeHtml(page.offset ?? 0)}
    / next_offset=${pcvEscapeHtml(nextOffset)}
    / retention max_terminal_jobs=${pcvEscapeHtml(retention.max_terminal_jobs ?? '-')}
    ${nextButton}
  </div>`;
}

// --- legacy evidence.ts ---
function stateBadge(value) {
  const text = String(value ?? 'unknown');
  const normalized = text.toLowerCase();
  const cls = normalized.includes('running') || normalized.includes('ready') || normalized === 'ok'
    ? 'badge-ok'
    : normalized.includes('fail') || normalized.includes('error') || normalized.includes('forbidden')
      ? 'badge-error'
      : 'badge-warn';
  return `<span class="badge ${cls}">${pcvEscapeHtml(text)}</span>`;
}

function asObject(value) {
  return value && typeof value === 'object' && !Array.isArray(value) ? value : {};
}

function getBatchEvidence() {
  const evidence = state.opsSummary?.batch_evidence || state.opsSummary?.batchEvidence;
  return evidence && typeof evidence === 'object' && !Array.isArray(evidence) ? evidence : null;
}

function getCurrentEvidenceRollup() {
  const rollup = state.opsSummary?.current_evidence || state.opsSummary?.currentEvidence;
  return rollup && typeof rollup === 'object' && !Array.isArray(rollup) ? rollup : null;
}

function getCurrentEvidenceFullAdmin(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['full_admin_host_mutation', 'latest']) || readNested(rollup || {}, ['fullAdminHostMutation', 'latest']));
}

function getCurrentEvidencePublicBoundary(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['public_boundary', 'latest_main_push']) || readNested(rollup || {}, ['publicBoundary', 'latestMainPush']));
}

function getCurrentEvidencePackagePair(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['manual_admin', 'latest_package_pair']) || readNested(rollup || {}, ['manualAdmin', 'latestPackagePair']));
}

function getCurrentEvidenceNextPackagePair(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['manual_admin', 'next_package_pair']) || readNested(rollup || {}, ['manualAdmin', 'nextPackagePair']));
}

function getCurrentEvidenceHostOps(rollup = getCurrentEvidenceRollup()) {
  return asObject(readNested(rollup || {}, ['host_ops', 'lifecycle_descriptor']) || readNested(rollup || {}, ['hostOps', 'lifecycleDescriptor']));
}

function getRuntimeApiRegistryBridge() {
  const summary = asObject(state.opsSummary);
  const installedRuntime = asObject(summary.installed_runtime || summary.installedRuntime);
  const diagnostics = asObject(installedRuntime.diagnostics);
  const bridge = diagnostics.runtime_api_registry_bridge || diagnostics.runtimeApiRegistryBridge;
  return asObject(bridge);
}

function normalizeEvidenceStatus(evidence = getBatchEvidence()) {
  const status = String(evidence?.status || (evidence ? 'unavailable' : 'not_configured')).toLowerCase();
  return ['not_configured', 'missing', 'unavailable', 'available', 'degraded'].includes(status) ? status : 'unavailable';
}

function evidenceTone(status) {
  if (status === 'available') return 'ok';
  if (status === 'not_configured') return 'neutral';
  return 'warn';
}

function evidenceStatusLabel(status) {
  return {
    not_configured: 'not configured',
    missing: 'missing',
    degraded: 'degraded',
    unavailable: 'unavailable',
    available: 'available'
  }[status] || 'unavailable';
}

function currentEvidenceTone(status) {
  if (['available', 'artifact-discovered', 'tracked-in-documentation', 'pass'].includes(status)) return 'ok';
  if (status === 'not_configured') return 'neutral';
  return 'warn';
}

function currentEvidenceStatusLabel(status) {
  return {
    not_configured: 'not configured',
    missing: 'missing',
    degraded: 'degraded',
    unavailable: 'unavailable',
    available: 'available',
    'artifact-discovered': 'artifact discovered',
    'tracked-in-documentation': 'tracked in documentation',
    pass: 'pass'
  }[status] || 'tracked';
}

function renderEvidenceStatusBadge(evidence = getBatchEvidence()) {
  const status = normalizeEvidenceStatus(evidence);
  const latest = asObject(evidence?.latest);
  const label = latest.batch_id || evidenceStatusLabel(status);
  return `<span class="status-badge evidence-${status} ${evidenceTone(status)}" title="${pcvEscapeHtml(evidenceStatusLabel(status))}">${pcvEscapeHtml(label)}</span>`;
}

function renderCurrentEvidenceStatusBadge() {
  const rollup = getCurrentEvidenceRollup();
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const status = String(fullAdmin.status || (rollup ? 'tracked' : 'not_configured')).toLowerCase();
  const normalized = status === 'tracked' ? 'tracked-in-documentation' : status;
  const label = fullAdmin.batch_id || fullAdmin.version || currentEvidenceStatusLabel(normalized);
  return `<span class="status-badge current-evidence ${currentEvidenceTone(normalized)}" title="${pcvEscapeHtml(currentEvidenceStatusLabel(normalized))}">${pcvEscapeHtml(label)}</span>`;
}

function evidenceValue(value, fallback) {
  const shown = fallback === undefined ? t('copy.evidence.unavailable') : fallback;
  return value === undefined || value === null || value === '' ? shown : String(value);
}

function evidenceBooleanLabel(value) {
  if (value === true) return t('copy.evidence.passed');
  if (value === false) return t('copy.evidence.failed');
  return t('copy.evidence.unavailable');
}

function evidenceDashboardDetail(evidence = getBatchEvidence()) {
  const status = normalizeEvidenceStatus(evidence);
  if (!evidence || evidence.configured === false || status === 'not_configured') {
    return t('copy.evidence.root_missing');
  }
  const latest = asObject(evidence.latest);
  const gpu = asObject(latest.gpu_snapshots);
  const route = asObject(latest.route_msi_hyperv);
  const os = asObject(latest.os_mutation);
  return [
    `run=${evidenceValue(latest.status || status, status)}`,
    `gpu=${evidenceValue(gpu.count, '0')}`,
    `route=${evidenceBooleanLabel(route.ok)}`,
    `os=${evidenceBooleanLabel(os.ok)}`
  ].join(' / ');
}

function currentEvidenceDashboardDetail() {
  const rollup = getCurrentEvidenceRollup();
  if (!rollup) {
    return t('copy.evidence.rollup_missing');
  }

  const publicBoundary = getCurrentEvidencePublicBoundary(rollup);
  const fullAdmin = getCurrentEvidenceFullAdmin(rollup);
  const packagePair = getCurrentEvidencePackagePair(rollup);
  const nextPackagePair = getCurrentEvidenceNextPackagePair(rollup);
  const hostOps = getCurrentEvidenceHostOps(rollup);
  return [
    `public=${evidenceValue(publicBoundary.run_id || publicBoundary.status, 'not configured')}`,
    `full=${evidenceValue(fullAdmin.batch_id || fullAdmin.status, 'not configured')}`,
    `manual=${evidenceValue(packagePair.package_pair || packagePair.status, 'not configured')}`,
    `next=${evidenceValue(nextPackagePair.package_pair || nextPackagePair.status, 'not configured')}`,
    `hostops=${evidenceValue(hostOps.contract_key || hostOps.status, 'not configured')}`
  ].join(' / ');
}

function evidenceIssueTone(issue) {
  const tone = String(issue?.tone || '').toLowerCase();
  return tone === 'error' ? 'error' : 'warn';
}

function collectEvidenceIssues() {
  const evidence = getBatchEvidence();
  if (!evidence) return [];
  const issues = [];
  const status = normalizeEvidenceStatus(evidence);
  if (status !== 'available' && status !== 'not_configured') {
    issues.push({
      code: `batch-evidence-${status}`,
      message: `Batch evidence ${status}`,
      detail: status,
      tone: status === 'unavailable' ? 'error' : 'warn'
    });
  }
  for (const error of asArray(evidence.errors)) {
    const issue = normalizeSummaryIssue(error, 'PCV_BATCH_EVIDENCE');
    issues.push({
      code: issue.code || 'PCV_BATCH_EVIDENCE',
      message: issue.message || t('copy.evidence.issue'),
      detail: issue.detail || status,
      tone: issue.code === 'PCV_BATCH_EVIDENCE_PARSE_FAILED' ? 'error' : 'warn'
    });
  }
  return issues;
}

// exports
window.getFilteredJobRows = getFilteredJobRows;
window.getJobCancelScope = getJobCancelScope;
window.formatJobCancelLabel = formatJobCancelLabel;
window.canReconcileVmMutation = canReconcileVmMutation;
window.normalizeJob = normalizeJob;
window.renderFailedJobTriageRows = renderFailedJobTriageRows;
window.renderIncidentCommand = renderIncidentCommand;
window.normalizeEventTone = normalizeEventTone;
window.buildEventCenterItems = buildEventCenterItems;
window.getJobTime = getJobTime;
window.formatJobDetail = formatJobDetail;
window.formatCorrelationValue = formatCorrelationValue;
window.getActivityRowsForDashboard = getActivityRowsForDashboard;
window.buildActivityRows = buildActivityRows;
window.renderDashboardActivity = renderDashboardActivity;
window.renderActivity = renderActivity;
window.renderActivityPageSummary = renderActivityPageSummary;
window.stateBadge = stateBadge;
window.asObject = asObject;
window.getBatchEvidence = getBatchEvidence;
window.getCurrentEvidenceRollup = getCurrentEvidenceRollup;
window.getCurrentEvidenceFullAdmin = getCurrentEvidenceFullAdmin;
window.getCurrentEvidencePublicBoundary = getCurrentEvidencePublicBoundary;
window.getCurrentEvidencePackagePair = getCurrentEvidencePackagePair;
window.getCurrentEvidenceNextPackagePair = getCurrentEvidenceNextPackagePair;
window.getCurrentEvidenceHostOps = getCurrentEvidenceHostOps;
window.getRuntimeApiRegistryBridge = getRuntimeApiRegistryBridge;
window.normalizeEvidenceStatus = normalizeEvidenceStatus;
window.evidenceTone = evidenceTone;
window.evidenceStatusLabel = evidenceStatusLabel;
window.currentEvidenceTone = currentEvidenceTone;
window.currentEvidenceStatusLabel = currentEvidenceStatusLabel;
window.renderEvidenceStatusBadge = renderEvidenceStatusBadge;
window.renderCurrentEvidenceStatusBadge = renderCurrentEvidenceStatusBadge;
window.evidenceValue = evidenceValue;
window.evidenceBooleanLabel = evidenceBooleanLabel;
window.evidenceDashboardDetail = evidenceDashboardDetail;
window.currentEvidenceDashboardDetail = currentEvidenceDashboardDetail;
window.evidenceIssueTone = evidenceIssueTone;
window.collectEvidenceIssues = collectEvidenceIssues;
PCV.ops = Object.assign(PCV.ops || {}, { getFilteredJobRows, getJobCancelScope, formatJobCancelLabel, canReconcileVmMutation, normalizeJob, renderFailedJobTriageRows, renderIncidentCommand, normalizeEventTone, buildEventCenterItems, getJobTime, formatJobDetail, formatCorrelationValue, getActivityRowsForDashboard, buildActivityRows, renderDashboardActivity, renderActivity, renderActivityPageSummary, stateBadge, asObject, getBatchEvidence, getCurrentEvidenceRollup, getCurrentEvidenceFullAdmin, getCurrentEvidencePublicBoundary, getCurrentEvidencePackagePair, getCurrentEvidenceNextPackagePair, getCurrentEvidenceHostOps, getRuntimeApiRegistryBridge, normalizeEvidenceStatus, evidenceTone, evidenceStatusLabel, currentEvidenceTone, currentEvidenceStatusLabel, renderEvidenceStatusBadge, renderCurrentEvidenceStatusBadge, evidenceValue, evidenceBooleanLabel, evidenceDashboardDetail, currentEvidenceDashboardDetail, evidenceIssueTone, collectEvidenceIssues });
})(window.PCV);

// @ts-nocheck
function normalizeJob(job) {
  return job && typeof job === 'object' && !Array.isArray(job) ? job : {};
}

function renderFailedJobTriageRows() {
  const failed = buildActivityRows()
    .map(({ source, job }) => ({ source, job: normalizeJob(job) }))
    .filter(({ job }) => String(job.status || '').toLowerCase() === 'failed')
    .slice(0, 5);
  if (!failed.length) {
    return '<p class="muted">No failed jobs are visible.</p>';
  }

  return `<div class="triage-list">
    ${failed.map(({ source, job }) => {
      const retryable = Boolean(job.error?.retryable);
      return `<div class="triage-row">
        <div>
          <strong>${escapeHtml(job.operation || job.job_id || 'job')}</strong>
          <span class="muted">${escapeHtml(job.job_id || source)}</span>
          <span class="muted">${escapeHtml(formatJobDetail(job))}</span>
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
        <span>${escapeHtml(issue.code)}</span>
        <span class="status-badge ${escapeHtml(evidenceIssueTone(issue))}">${escapeHtml(issue.detail || 'issue')}</span>
      </div>`).join('')}</div>`
    : '<p class="muted">No batch evidence degradation is visible.</p>';
  const priorityHtml = priorityItems.length === 0
    ? '<p class="muted">No high-priority warnings.</p>'
    : priorityItems.map((item) => `<div class="priority-item priority-${escapeHtml(item.tone)}"><strong>${escapeHtml(item.label)}</strong><span>${escapeHtml(item.detail)}</span></div>`).join('');

  els.incidentPanel.innerHTML = `
    <div class="incident-grid">
      <div class="incident-card">
        <span class="muted">Failed jobs</span>
        <strong>${escapeHtml(failedJobs.length)} failed / ${escapeHtml(retryableFailedJobs)} retryable</strong>
        ${renderFailedJobTriageRows()}
      </div>
      <div class="incident-card">
        <span class="muted">Evidence issues</span>
        <strong>${escapeHtml(evidenceIssues.length)}</strong>
        ${evidenceIssueHtml}
      </div>
      <div class="incident-card">
        <span class="muted">Priority items</span>
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
  if (activeJobs.length > 0) push('warn', 'Active jobs', `${activeJobs.length} queued/running job(s) are visible.`, 'jobs');
  if (failedJobs.length > 0) push('error', 'Failed jobs', `${failedJobs.length} failed job(s) need review.`, 'jobs');
  if (!state.apiToken.trim()) push('info', 'Browser token', 'token-required routes may show Auth required.', 'session');

  return items.slice(0, 12);
}

function renderEventCenter() {
  if (!els.eventCenterPanel) return;
  const items = buildEventCenterItems();
  const counts = items.reduce((acc, item) => {
    acc[item.tone] = (acc[item.tone] || 0) + 1;
    return acc;
  }, {});
  const severityLane = ['error', 'warn', 'info', 'ok']
    .map((tone) => `<span class="event-pill event-${tone}">${escapeHtml(tone)} ${escapeHtml(counts[tone] || 0)}</span>`)
    .join('');
  const rows = items.length
    ? items.map((item) => `<div class="event-row event-${escapeHtml(item.tone)}">
        <span class="status-badge ${escapeHtml(item.tone)}">${escapeHtml(item.source)}</span>
        <div><strong>${escapeHtml(item.label)}</strong><p>${escapeHtml(item.detail)}</p></div>
      </div>`).join('')
    : '<p class="muted">No operator events are visible.</p>';

  els.eventCenterPanel.innerHTML = `
    <div class="event-center-header">
      <div><p class="eyebrow">Event Center</p><h3>Severity lane</h3></div>
      <div class="event-severity-lane">${severityLane}</div>
    </div>
    <div class="event-center-list">${rows}</div>`;
}

function getJobTime(job) {
  return job?.updated_at || job?.created_at || job?.canceled_at || '-';
}

function formatJobDetail(job) {
  if (job?.error?.code) {
    return `${job.error.code}: ${job.error.message || 'Job failed'}`;
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
          <strong>${escapeHtml(job.operation || job.action || 'job')}</strong>
          <div class="muted">${escapeHtml(job.job_id || job.correlation_id || job.request_id || '-')}</div>
        </div>
        <div>${stateBadge(job.status || job.state || 'unknown')}</div>
        <div class="muted">${escapeHtml(getJobTime(job))}</div>
        <div>${escapeHtml(formatJobDetail(job))}</div>
        <div><span class="badge">${escapeHtml(source)}</span></div>
      </div>`).join('')}
    </div>`;
}

function renderActivity() {
  const rows = buildActivityRows();
  const degraded = state.activityError
    ? `<div class="activity-warning"><strong>${escapeHtml(state.activityError.code)}</strong> ${escapeHtml(state.activityError.message)}</div>`
    : '';
  const pageSummary = renderActivityPageSummary();

  if (rows.length === 0) {
    els.activityPanel.innerHTML = `${degraded}${pageSummary}<p class="muted">No server or browser job activity has been loaded.</p>`;
    return;
  }

  els.activityPanel.innerHTML = degraded + pageSummary + rows.map(({ source, job }) => {
    const status = String(job.status || 'unknown').toLowerCase();
    const canOperate = rbacAllows('operate');
    const actions = source === 'browser' ? [
      renderJobCancelButton(job, canOperate),
      status === 'failed' ? `<button data-action="retry-job" data-job-id="${escapeHtml(job.job_id)}"${canOperate ? '' : ' disabled'}>Retry</button>` : '',
      renderJobReconcileButton(job, canOperate)
    ].join('') : '';

    return `<div class="activity-row">
      <div>
        <strong>${escapeHtml(job.operation || 'job')}</strong>
        <div class="muted">${escapeHtml(job.job_id || '-')}</div>
        <div class="muted">${escapeHtml(formatCorrelationValue(job))}</div>
      </div>
      <div>${stateBadge(job.status)}</div>
      <div class="muted">${escapeHtml(getJobTime(job))}</div>
      <div>${escapeHtml(formatJobDetail(job))}</div>
      <div><span class="badge">${escapeHtml(source)}</span>${actions}</div>
    </div>`;
  }).join('');
}

function renderActivityPageSummary() {
  const page = state.serverJobPage;
  if (!page || typeof page !== 'object') return '';
  const retention = page.retention || {};
  const nextOffset = page.next_offset === null || page.next_offset === undefined ? 'none' : page.next_offset;
  const nextButton = nextOffset === 'none'
    ? ''
    : `<button type="button" data-action="load-next-jobs" data-next-offset="${escapeHtml(nextOffset)}">Load next jobs</button>`;
  return `<div class="activity-warning activity-page-summary">
    <strong>Pagination</strong>
    ${escapeHtml(page.returned ?? asArray(page).length)} shown of ${escapeHtml(page.count ?? asArray(state.serverJobs).length)}
    / limit=${escapeHtml(page.limit ?? 50)}
    / offset=${escapeHtml(page.offset ?? 0)}
    / next_offset=${escapeHtml(nextOffset)}
    / retention max_terminal_jobs=${escapeHtml(retention.max_terminal_jobs ?? '-')}
    ${nextButton}
  </div>`;
}


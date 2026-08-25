// @ts-nocheck
function renderJobEdgeSummary() {
  const page = state.serverJobPage || {};
  const rows = buildActivityRows();
  const active = rows.filter(({ job }) => ['queued', 'running'].includes(String(job?.status || '').toLowerCase())).length;
  const failed = rows.filter(({ job }) => String(job?.status || '').toLowerCase() === 'failed').length;
  const retained = page.retention?.max_terminal_jobs ?? 'unknown';
  const nextOffset = page.next_offset === null || page.next_offset === undefined ? 'none' : page.next_offset;
  return `<div class="activity-warning job-edge-summary">
    <strong>Job edge cases</strong>
    active running jobs=${escapeHtml(active)}
    / failed job retry=${escapeHtml(failed)}
    / retained terminal jobs=${escapeHtml(retained)}
    / next_offset=${escapeHtml(nextOffset)}
  </div>`;
}

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
    ? 'Cancel running guest exec'
    : 'Cancel';
}

function canReconcileVmMutation(job) {
  const operation = String(job?.operation || '').toLowerCase();
  return String(job?.status || '').toLowerCase() === 'failed' &&
    ['vm.rename', 'vm.delete', 'checkpoint.create', 'checkpoint.restore'].includes(operation) &&
    String(job?.error?.code || '').toUpperCase() === 'PCV_JOB_INTERRUPTED';
}

function renderJobReconcileButton(job, canOperate) {
  if (!canReconcileVmMutation(job)) return '';
  const operation = String(job?.operation || '').toLowerCase();
  const label = operation === 'vm.delete'
    ? 'Reconcile delete'
    : operation === 'checkpoint.create'
      ? 'Reconcile checkpoint'
      : operation === 'checkpoint.restore'
        ? 'Reconcile restore'
        : 'Reconcile rename';
  return `<button data-action="reconcile-job" data-job-id="${escapeHtml(job.job_id)}"${canOperate ? '' : ' disabled'}>${label}</button>`;
}

function renderJobCancelButton(job, canOperate) {
  const status = String(job?.status || 'unknown').toLowerCase();
  if (!['queued', 'running'].includes(status)) return '';
  const cancelScope = getJobCancelScope(job);
  const buttonClass = cancelScope === 'running-guest-execution' ? ' class="danger-button job-cancel-button"' : '';
  return `<button${buttonClass} data-action="cancel-job" data-job-id="${escapeHtml(job.job_id)}" data-job-cancel-scope="${escapeHtml(cancelScope)}"${canOperate ? '' : ' disabled'}>${escapeHtml(formatJobCancelLabel(job))}</button>`;
}

function renderJobs() {
  const { allRows, rows } = getFilteredJobRows();
  const canOperate = rbacAllows('operate');
  const summary = renderJobEdgeSummary();
  const tableSummary = renderTableStateSummary('Jobs', rows.length, allRows.length, state.jobFilter, `status=${state.jobStatusFilter || 'all'}`);
  if (rows.length === 0) {
    const filtered = Boolean(state.jobFilter.trim() || state.jobStatusFilter !== 'all');
    const emptyMessage = filtered
      ? 'No jobs match the current filter. Jobs created from this browser session or returned by GET /api/v1/jobs will appear here.'
      : 'No jobs on this page. Jobs created from this browser session or returned by GET /api/v1/jobs will appear here.';
    els.jobsPanel.innerHTML = `${summary}${tableSummary}<p class="muted">${escapeHtml(emptyMessage)}</p>`;
    return;
  }
  els.jobsPanel.innerHTML = summary + tableSummary + rows.map(({ source, job }) => {
    const status = String(job.status || 'unknown').toLowerCase();
    const actions = [
      renderJobCancelButton(job, canOperate),
      status === 'failed' ? `<button data-action="retry-job" data-job-id="${escapeHtml(job.job_id)}"${canOperate ? '' : ' disabled'}>Retry</button>` : '',
      renderJobReconcileButton(job, canOperate)
    ].join('');
    return `<div class="job-row"><div><strong>${escapeHtml(job.job_id)}</strong><div class="muted">${escapeHtml(job.operation || 'vm.create')}</div></div><div>${stateBadge(job.status)}</div><div><span class="badge">${escapeHtml(source)}</span>${actions}</div></div>`;
  }).join('');
}


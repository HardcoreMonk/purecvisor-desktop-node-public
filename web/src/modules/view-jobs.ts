// @ts-nocheck
// Desktop Node module (ADR-0018): jobs screen on the U4a view registry.
// Cancel, retry, and reconcile stay on the jobs-panel delegated click.
window.PCV = window.PCV || {};
(function (PCV) {
  var FACET = 'jobStatus';
  var syncingStatus = false;

  function statusValue() {
    return String(state.jobStatusFilter || 'all').toLowerCase();
  }

  function operationValue() {
    return String(state.jobOperationFilter || '');
  }

  function facetFor(value) {
    return value && value !== 'all' ? [value] : [];
  }

  function syncFacetFromState() {
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs) return;
    var cur = (fs.current() || {})[FACET] || [];
    var want = facetFor(statusValue());
    if (cur.length === want.length && String(cur[0] || '') === String(want[0] || '')) return;
    syncingStatus = true;
    try { fs.apply({ jobStatus: want }); }
    finally { syncingStatus = false; }
  }

  function syncStateFromFacet() {
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs) return;
    var cur = (fs.current() || {})[FACET] || [];
    var next = cur.length === 1 ? String(cur[0]).toLowerCase() : 'all';
    if (statusValue() === next) return;
    state.jobStatusFilter = next;
    var select = document.getElementById('job-status-filter');
    if (select) select.value = next;
  }

  function countStatus(rows, name) {
    return rows.filter(function (row) {
      return String(row.job && row.job.status || '').toLowerCase() === name;
    }).length;
  }

  function kindCell(job) {
    var el = PCV.uxlib.el;
    var correlation = typeof formatCorrelationValue === 'function'
      ? formatCorrelationValue(job)
      : String((job && (job.request_id || job.correlation_id)) || '-');
    return el('div', null,
      String((job && job.operation) || '-'),
      el('div', { class: 'muted' }, String((job && job.job_id) || '-')),
      el('div', { class: 'muted' }, String(correlation || '-')));
  }

  function jobTarget(job) {
    return String((job && (job.target || job.vm_name || job.vm_id || job.subject || job.name)) || '-');
  }

  function formatDuration(job) {
    var start = Date.parse((job && job.created_at) || '');
    var end = Date.parse((job && (job.finished_at || job.updated_at)) || '');
    if (!start || !end || end < start) return '-';
    var seconds = Math.round((end - start) / 1000);
    if (seconds < 60) return String(seconds) + 's';
    return String(Math.floor(seconds / 60)) + 'm ' + String(seconds % 60) + 's';
  }

  function errorSummary(job) {
    var error = job && job.error;
    if (!error) return '-';
    var code = error.code ? String(error.code) : '';
    var message = error.message ? String(error.message) : '';
    if (code && message) return code + ': ' + message;
    return code || message || '-';
  }

  function statusLabel(status) {
    var key = {
      queued: 'copy.jobs.status.queued',
      running: 'copy.jobs.status.running',
      failed: 'copy.jobs.status.failed',
      succeeded: 'copy.jobs.status.succeeded',
      canceled: 'copy.jobs.status.canceled'
    }[status];
    return key ? t(key) : (status || '-');
  }

  function statusTone(status) {
    if (status === 'succeeded') return 'ok';
    if (status === 'failed') return 'crit';
    if (status === 'running' || status === 'queued') return 'warn';
    return 'idle';
  }

  function reconcileLabel(job) {
    var operation = String(job && job.operation || '').toLowerCase();
    if (operation === 'vm.delete') return t('copy.job.reconcile_delete');
    if (operation === 'checkpoint.create') return t('copy.job.reconcile_checkpoint');
    if (operation === 'checkpoint.restore') return t('copy.job.reconcile_restore');
    if (operation === 'vm.create') return t('copy.job.reconcile_create');
    if (operation === 'vm.shutdown') return t('copy.job.reconcile_shutdown');
    if (operation === 'vm.restart') return t('copy.job.reconcile_restart');
    if (operation === 'vm.qos.storage.set') return t('copy.job.reconcile_storage');
    if (operation === 'vm.qos.network.set') return t('copy.job.reconcile_network');
    return t('copy.job.reconcile_rename');
  }

  function actionButtons(job) {
    var el = PCV.uxlib.el;
    var canOperate = typeof rbacAllows === 'function' ? rbacAllows('operate') : true;
    var status = String(job && job.status || '').toLowerCase();
    var nodes = [];
    if (status === 'queued' || status === 'running') {
      var scope = getJobCancelScope(job);
      var cancelAttrs = {
        type: 'button',
        'data-action': 'cancel-job',
        'data-job-id': job.job_id,
        'data-job-cancel-scope': scope
      };
      if (scope === 'running-guest-execution') cancelAttrs.class = 'danger-button job-cancel-button';
      if (!canOperate) cancelAttrs.disabled = '';
      nodes.push(el('button', cancelAttrs, formatJobCancelLabel(job)));
    }
    if (status === 'failed') {
      var retryAttrs = {
        type: 'button',
        'data-action': 'retry-job',
        'data-job-id': job.job_id
      };
      if (!canOperate) retryAttrs.disabled = '';
      nodes.push(el('button', retryAttrs, t('copy.job.retry')));
    }
    if (typeof canReconcileVmMutation === 'function' && canReconcileVmMutation(job)) {
      var reconcileAttrs = {
        type: 'button',
        'data-action': 'reconcile-job',
        'data-job-id': job.job_id
      };
      if (!canOperate) reconcileAttrs.disabled = '';
      nodes.push(el('button', reconcileAttrs, reconcileLabel(job)));
    }
    return el('div', { class: 'job-actions' }, nodes);
  }

  function edgeSummary(rows) {
    var el = PCV.uxlib.el;
    var page = state.serverJobPage || {};
    var active = countStatus(rows, 'queued') + countStatus(rows, 'running');
    var failed = countStatus(rows, 'failed');
    var retained = page.retention && page.retention.max_terminal_jobs != null
      ? page.retention.max_terminal_jobs
      : 'unknown';
    var nextOffset = page.next_offset === null || page.next_offset === undefined ? 'none' : page.next_offset;
    return el('div', { class: 'activity-warning job-edge-summary' },
      el('strong', null, t('copy.jobs.edge.title')),
      ' ' + t('copy.jobs.edge.active') + '=' + String(active),
      ' / ' + t('copy.jobs.edge.failed') + '=' + String(failed),
      ' / ' + t('copy.jobs.edge.retained') + '=' + String(retained),
      ' / ' + t('copy.jobs.pagination'),
      ' / max_terminal_jobs=' + String(retained),
      ' / next_offset=' + String(nextOffset));
  }

  function operationNames(rows) {
    var names = [];
    rows.forEach(function (row) {
      var name = String(row.job && row.job.operation || '');
      if (name && names.indexOf(name) === -1) names.push(name);
    });
    names.sort();
    var current = operationValue();
    if (current && names.indexOf(current) === -1) names.push(current);
    return names;
  }

  function nextOffset() {
    var page = state.serverJobPage;
    if (!page || typeof page !== 'object') return null;
    if (page.next_offset === null || page.next_offset === undefined || page.next_offset === '') return null;
    return page.next_offset;
  }

  function renderJobsView(mount, ctx) {
    if (!mount) return;
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    var el = PCV.uxlib.el;
    var clearEl = PCV.uxlib.clearEl;
    syncFacetFromState();
    var statusSelect = document.getElementById('job-status-filter');
    if (statusSelect && statusSelect.value !== statusValue()) statusSelect.value = statusValue();
    var packed = getFilteredJobRows();
    var allRows = packed.allRows;
    var rows = packed.rows;
    var operation = operationValue();
    if (operation) {
      rows = rows.filter(function (row) {
        return String(row.job && row.job.operation || '') === operation;
      });
    }
    clearEl(mount);
    var tools = el('div', { class: 'jobs-toolbar' });
    tools.appendChild(HN.filterBar([{
      key: FACET,
      label: t('copy.jobs.status_filter'),
      singleSelect: true,
      showAll: true,
      allLabel: t('copy.jobs.status.all'),
      allCount: allRows.length,
      options: [
        { value: 'queued', label: t('copy.jobs.status.queued'), count: countStatus(allRows, 'queued') },
        { value: 'running', label: t('copy.jobs.chip.running'), count: countStatus(allRows, 'running') },
        { value: 'succeeded', label: t('copy.jobs.status.succeeded'), count: countStatus(allRows, 'succeeded') },
        { value: 'failed', label: t('copy.jobs.status.failed'), count: countStatus(allRows, 'failed') },
        { value: 'canceled', label: t('copy.jobs.status.canceled'), count: countStatus(allRows, 'canceled') }
      ]
    }]));
    var opSelect = el('select', {
      id: 'job-operation-filter',
      'aria-label': t('copy.jobs.operation_filter')
    }, [el('option', { value: '' }, t('copy.jobs.operation.all'))].concat(operationNames(allRows).map(function (name) {
      return el('option', { value: name }, name);
    })));
    opSelect.value = operation;
    opSelect.addEventListener('change', function () {
      state.jobOperationFilter = opSelect.value || '';
      if (typeof render === 'function') render();
    });
    tools.appendChild(opSelect);
    mount.appendChild(tools);
    mount.appendChild(edgeSummary(allRows));
    var failed = !!state.activityError;
    var filtered = Boolean(String(state.jobFilter || '').trim() || statusValue() !== 'all' || operation);
    if (failed && rows.length === 0) {
      var detail = String(state.activityError.code || 'PCV_JOBS_UNAVAILABLE') + ' ' +
        String(state.activityError.message || '');
      mount.appendChild(PCV.components.loadError({
        titleKey: 'ui.load_error.title',
        detail: detail,
        onRetry: function () {
          if (typeof loadServerJobs !== 'function') return;
          Promise.resolve(loadServerJobs()).then(function () {
            if (typeof render === 'function') render();
          });
        }
      }));
    } else if (rows.length === 0) {
      var empty = PCV.components.emptyState({
        titleKey: filtered ? 'copy.jobs.empty_filter_title' : 'copy.jobs.empty_page_title',
        bodyKey: filtered ? 'copy.jobs.empty_filter' : 'copy.jobs.empty_page'
      });
      empty.classList.add(filtered ? 'jobs-empty-filter' : 'jobs-empty-page');
      mount.appendChild(empty);
    } else {
      if (failed) {
        mount.appendChild(PCV.components.loadError({
          titleKey: 'ui.load_error.title',
          detail: String(state.activityError.code || '') + ' ' + String(state.activityError.message || '')
        }));
      }
      var tableHost = el('div', { class: 'jobs-view-table' });
      mount.appendChild(tableHost);
      createDataTable(tableHost, { tableId: 'dt-jobs',
        searchable: false,
        exportable: true,
        headers: [
          { label: t('copy.jobs.col.received'), key: 'received' },
          { label: t('copy.jobs.col.kind'), key: 'kind' },
          { label: t('copy.jobs.col.target'), key: 'target' },
          { label: t('copy.jobs.col.status'), key: 'status' },
          { label: t('copy.jobs.col.duration'), key: 'duration' },
          { label: t('copy.jobs.col.error'), key: 'error' },
          { label: t('copy.jobs.col.actions'), key: 'actions', sortable: false }
        ],
        rows: rows.map(function (row) {
          var job = row.job || {};
          var status = String(job.status || '').toLowerCase();
          return [
            String(job.created_at || '-'),
            kindCell(job),
            jobTarget(job),
            HN.statusPill(statusTone(status), statusLabel(status)),
            formatDuration(job),
            errorSummary(job),
            actionButtons(job)
          ];
        })
      });
    }
    var offset = nextOffset();
    if (offset !== null) {
      mount.appendChild(el('button', {
        type: 'button',
        class: 'btn jobs-next-page',
        'data-action': 'load-next-jobs',
        'data-next-offset': String(offset),
        onClick: function () {
          var run = typeof loadNextJobPage === 'function' ? loadNextJobPage : null;
          if (!run) return;
          Promise.resolve(run()).then(function () {
            if (typeof render === 'function') render();
          });
        }
      }, t('copy.jobs.next_page')));
    }
  }

  if (PCV.ui && PCV.ui.filterState && typeof PCV.ui.filterState.subscribe === 'function') {
    PCV.ui.filterState.subscribe(function () {
      if (syncingStatus) return;
      var active = (PCV.nav && PCV.nav.activeView && PCV.nav.activeView()) ||
        (typeof state !== 'undefined' && state.activeView);
      if (active !== 'jobs') return;
      syncStateFromFacet();
      if (typeof render === 'function') render();
    });
  }

  if (typeof document !== 'undefined' && document.addEventListener) {
    document.addEventListener('change', function (event) {
      var target = event && event.target;
      if (!target || target.id !== 'job-status-filter') return;
      var next = String(target.value || 'all').toLowerCase();
      if (statusValue() === next) return;
      state.jobStatusFilter = next;
      var active = (PCV.nav && PCV.nav.activeView && PCV.nav.activeView()) ||
        (typeof state !== 'undefined' && state.activeView);
      if (active === 'jobs' && typeof render === 'function') render();
    });
  }

  PCV.viewJobs = { render: renderJobsView };
  PCV.views.register('jobs', { render: renderJobsView });
})(window.PCV);

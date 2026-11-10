// @ts-nocheck
// Desktop Node module (ADR-0018): dashboard page on the U4a view registry.
// KPI cards paint into #dashboard-kpi. Pinned panels under the mount stay on the legacy painters.
window.PCV = window.PCV || {};
(function (PCV) {
  var DAY_MS = 24 * 60 * 60 * 1000;

  function jobRows() {
    return typeof buildActivityRows === 'function' ? buildActivityRows() : [];
  }

  function vmTallies() {
    if (typeof hasRefreshedOperation === 'function' && !hasRefreshedOperation('vm.list')) return null;
    var vms = Array.isArray(state.vms) ? state.vms : [];
    var running = 0;
    var off = 0;
    var templates = 0;
    vms.forEach(function (vm) {
      if (typeof isTemplateLockedVm === 'function' && isTemplateLockedVm(vm)) {
        templates += 1;
        return;
      }
      var kind = typeof vmPowerKind === 'function' ? vmPowerKind(vm) : 'off';
      if (kind === 'running') running += 1;
      else if (kind === 'off') off += 1;
    });
    return { running: running, off: off, templates: templates };
  }

  function activeJobs(rows) {
    return rows.filter(function (row) {
      var status = String(row && row.job && row.job.status || '').toLowerCase();
      return status === 'queued' || status === 'running';
    }).length;
  }

  function failedWithin24h(rows, now) {
    var count = 0;
    rows.forEach(function (row) {
      var job = row && row.job;
      if (!job || String(job.status || '').toLowerCase() !== 'failed') return;
      var stamp = Date.parse(job.created_at || '');
      if (!Number.isFinite(stamp)) return;
      var age = now - stamp;
      if (age >= 0 && age <= DAY_MS) count += 1;
    });
    return count;
  }

  function hyperv() {
    var label = typeof getHostReadinessLabel === 'function' ? String(getHostReadinessLabel()) : '—';
    if (!label || label === '—' || label === '-') {
      return { state: 'unknown', value: '-', note: t('copy.dashboard.kpi.loading') };
    }
    if (typeof hostReadinessBlocked === 'function' && hostReadinessBlocked()) {
      return { state: 'not-ready', value: t('copy.dashboard.kpi.hv_not_ready'), note: label };
    }
    return { state: 'ready', value: t('copy.dashboard.kpi.hv_ready'), note: label };
  }

  function resolveMount(passed) {
    if (passed && typeof passed.hasAttribute === 'function' && passed.hasAttribute('data-view-mount')) return passed;
    var byId = document.getElementById('dashboard-kpi');
    if (byId && byId !== passed) return byId;
    return null;
  }

  function card(spec) {
    var el = PCV.uxlib.el;
    var tag = spec.href ? 'a' : 'div';
    var attrs = { class: 'dashboard-kpi-card', 'data-kpi': spec.kpi };
    if (spec.href) {
      attrs.href = spec.href;
      attrs['data-view-link'] = spec.view;
    }
    if (spec.hv) attrs['data-hv-state'] = spec.hv;
    return el(tag, attrs,
      el('span', { class: 'dashboard-kpi-label' }, spec.label),
      el('strong', { id: spec.valueId, class: 'dashboard-kpi-value' }, spec.value),
      el('span', { id: spec.noteId, class: 'dashboard-kpi-note', role: 'status' }, spec.note || ''));
  }

  function mirror(id, text) {
    var node = document.getElementById(id);
    if (node && node.textContent !== text) node.textContent = text;
  }

  function workloadRows() {
    if (typeof hasRefreshedOperation === 'function' && !hasRefreshedOperation('vm.list')) return null;
    var vms = Array.isArray(state.vms) ? state.vms.slice() : [];
    vms.sort(function (left, right) {
      var leftRank = (typeof vmPowerKind === 'function' ? vmPowerKind(left) : 'off') === 'running' ? 0 : 1;
      var rightRank = (typeof vmPowerKind === 'function' ? vmPowerKind(right) : 'off') === 'running' ? 0 : 1;
      if (leftRank !== rightRank) return leftRank - rightRank;
      var leftName = typeof getVmName === 'function' ? getVmName(left) : String(left && left.name || '');
      var rightName = typeof getVmName === 'function' ? getVmName(right) : String(right && right.name || '');
      return leftName.localeCompare(rightName);
    });
    return vms.slice(0, 8);
  }

  function powerPill(kind) {
    if (kind === 'running') return 's-ok';
    if (kind === 'paused' || kind === 'saved') return 's-warn';
    return 's-off';
  }

  function workloadScalar(value) {
    if (value == null || value === '' || typeof value === 'object') return '-';
    return String(value);
  }

  function workloadCpu(vm) {
    var cpu = vm && vm.cpu;
    var count = cpu && typeof cpu === 'object' ? cpu.count : cpu;
    if (count == null || count === '') count = vm ? (vm.vcpu != null ? vm.vcpu : vm.processor_count) : undefined;
    return workloadScalar(count);
  }

  function workloadMemory(vm) {
    var memory = vm && vm.memory;
    var startup = memory && typeof memory === 'object' ? memory.startup_mb : memory;
    if (startup == null || startup === '') startup = vm ? (vm.memory_mb != null ? vm.memory_mb : vm.memory_assigned_mb) : undefined;
    return workloadScalar(startup);
  }

  function openCreateVm() {
    var opener = document.getElementById('open-create-vm');
    if (opener && typeof opener.click === 'function') opener.click();
  }

  function renderWorkload() {
    var body = document.getElementById('dashboard-workload-body');
    if (!body) return;
    var el = PCV.uxlib.el;
    var rows = workloadRows();
    if (!rows) {
      PCV.uxlib.clearEl(body);
      body.appendChild(el('p', { role: 'status', 'data-workload-state': 'loading' }, t('copy.dashboard.kpi.loading')));
      return;
    }
    if (rows.length === 0) {
      PCV.uxlib.clearEl(body);
      body.appendChild(el('div', { class: 'empty-state', role: 'status', 'data-workload-state': 'empty' },
        el('div', { class: 'empty-title' }, t('copy.dashboard.workload.empty')),
        el('button', {
          type: 'button',
          class: 'btn btn-primary',
          'data-workload-create': '1',
          onclick: openCreateVm
        }, '+ ', el('span', null, t('copy.dashboard.create')))));
      return;
    }
    var tableRows = rows.map(function (vm) {
      var id = typeof getVmId === 'function' ? getVmId(vm) : String(vm && (vm.id || vm.name) || '');
      var name = typeof getVmName === 'function' ? getVmName(vm) : String(vm && vm.name || id);
      var kind = typeof vmPowerKind === 'function' ? vmPowerKind(vm) : 'off';
      var stateText = typeof getVmState === 'function' ? getVmState(vm) : '';
      if (!stateText) stateText = '-';
      var locked = typeof isTemplateLockedVm === 'function' && isTemplateLockedVm(vm);
      return [
        el('a', {
          href: '#/vms/' + encodeURIComponent(id) + '/summary',
          'data-workload-vm': id
        }, name),
        el('span', { class: 'status-badge ' + powerPill(kind), 'data-workload-power': kind }, stateText),
        workloadCpu(vm),
        workloadMemory(vm),
        locked ? t('copy.dashboard.workload.template_yes') : t('copy.dashboard.workload.template_no')
      ];
    });
    if (typeof createDataTable !== 'function') return;
    createDataTable(body, {
      tableId: 'dt-dashboard-workload',
      searchable: false,
      exportable: false,
      headers: [
        { key: 'name', label: t('copy.dashboard.workload.col.name'), sortable: false },
        { key: 'status', label: t('copy.dashboard.workload.col.status'), sortable: false },
        { key: 'cpu', label: t('copy.dashboard.workload.col.cpu'), sortable: false },
        { key: 'memory', label: t('copy.dashboard.workload.col.memory'), sortable: false },
        { key: 'template', label: t('copy.dashboard.workload.col.template'), sortable: false }
      ],
      rows: tableRows
    });
    var table = document.getElementById('dt-dashboard-workload-table');
    if (table) table.setAttribute('aria-labelledby', 'dashboard-workload-title');
  }

  function readStamp(raw) {
    if (!raw || typeof raw !== 'object') return NaN;
    return Date.parse(raw.created_at || raw.observed_at || raw.timestamp || raw.updated_at || '');
  }

  function incidentAction(raw, issue) {
    var recommended = raw && (raw.recommended_action || raw.recommendedAction || raw.recommendation);
    if (recommended) return String(recommended);
    var detail = issue && issue.detail ? String(issue.detail) : '';
    var title = issue && issue.message ? String(issue.message) : '';
    if (detail && detail !== title) return detail;
    return '';
  }

  function opsIncidents() {
    var summary = state.opsSummary;
    if (!summary || typeof summary !== 'object') return [];
    var items = [];
    var errors = Array.isArray(summary.errors) ? summary.errors : [];
    errors.forEach(function (error) {
      var issue = typeof normalizeSummaryIssue === 'function' ? normalizeSummaryIssue(error) : { message: '', detail: '' };
      items.push({
        kind: 'incident',
        at: readStamp(error),
        severity: 'error',
        title: issue.message || t('copy.mon.ops_degraded'),
        action: incidentAction(error, issue)
      });
    });
    var signals = Array.isArray(summary.signals) ? summary.signals : [];
    signals.forEach(function (signal) {
      var tone = typeof summarySignalTone === 'function' ? summarySignalTone(signal) : '';
      var key = typeof summarySignalKey === 'function' ? summarySignalKey(signal) : '';
      if (!key && !tone) return;
      if (tone === 'ok' || tone === 'ready' || tone === 'healthy') return;
      if (key === 'summary-errors' && errors.length > 0) return;
      var issue = typeof normalizeSummaryIssue === 'function'
        ? normalizeSummaryIssue(signal, key || 'PCV_OPS_SUMMARY_SIGNAL')
        : { message: '', detail: '' };
      var severity = (tone === 'error' || tone === 'critical' || tone === 'fail' || tone === 'failed') ? 'error' : 'warn';
      items.push({
        kind: 'incident',
        at: readStamp(signal),
        severity: severity,
        title: issue.message || t('copy.mon.ops_signal'),
        action: incidentAction(signal, issue)
      });
    });
    return items;
  }

  function alertsUnavailable() {
    if (state.summaryError) return true;
    if (typeof hasRefreshedOperation === 'function' && !hasRefreshedOperation('ops.summary')) return true;
    return false;
  }

  // 첫 갱신이 끝나기 전에는 실패가 아니라 읽는 중이다. summaryError가 있으면 실패가 우선이다.
  function alertsLoading() {
    return state.loading === true && state.lastRefreshedAt === null && !state.summaryError;
  }

  function collectAlerts(skipIncidents) {
    var now = Date.now();
    var items = [];
    jobRows().forEach(function (row) {
      var job = row && row.job;
      if (!job || String(job.status || '').toLowerCase() !== 'failed') return;
      var stamp = Date.parse(job.created_at || '');
      if (!Number.isFinite(stamp)) return;
      var age = now - stamp;
      if (age < 0 || age > DAY_MS) return;
      items.push({ kind: 'job', at: stamp, job: job });
    });
    if (!skipIncidents) opsIncidents().forEach(function (item) { items.push(item); });
    items.sort(function (left, right) {
      var leftOk = Number.isFinite(left.at);
      var rightOk = Number.isFinite(right.at);
      if (leftOk && rightOk && left.at !== right.at) return right.at - left.at;
      if (leftOk !== rightOk) return leftOk ? -1 : 1;
      return 0;
    });
    return items.slice(0, 8);
  }

  function severityLabel(severity) {
    return severity === 'error' ? t('copy.dashboard.alerts.severity_error') : t('copy.dashboard.alerts.severity_warn');
  }

  function renderAlertItem(item) {
    var el = PCV.uxlib.el;
    if (item.kind === 'job') {
      var job = item.job || {};
      var summary = typeof formatJobDetail === 'function' ? formatJobDetail(job) : '-';
      var target = job.target || job.vm_name || job.vm_id || job.resource_id || job.job_id || '-';
      var kind = job.operation || job.action || '-';
      return el('li', { class: 'dashboard-alert-item', 'data-alert-kind': 'job', 'data-alert-id': String(job.job_id || '') },
        el('span', { 'data-alert-field': 'kind' }, String(kind)),
        el('span', { class: 'dashboard-alert-meta', 'data-alert-field': 'target' }, String(target)),
        el('span', { class: 'dashboard-alert-meta', 'data-alert-field': 'time' }, String(job.created_at || '')),
        el('span', { class: 'dashboard-alert-meta', 'data-alert-field': 'error' }, String(summary)),
        el('a', { href: '#/jobs', 'data-alert-job': String(job.job_id || ''), 'data-view-link': 'jobs' }, t('copy.dashboard.jobs_link')));
    }
    var children = [
      el('span', {
        class: 'status-badge ' + (item.severity === 'error' ? 's-err' : 's-warn'),
        'data-alert-severity': item.severity
      }, severityLabel(item.severity)),
      el('span', { 'data-alert-field': 'title' }, item.title)
    ];
    if (item.action) children.push(el('span', { class: 'dashboard-alert-meta', 'data-alert-field': 'action' }, item.action));
    return el('li', { class: 'dashboard-alert-item', 'data-alert-kind': 'incident' }, children);
  }

  function renderAlerts() {
    var body = document.getElementById('dashboard-alerts-body');
    if (!body) return;
    var el = PCV.uxlib.el;
    PCV.uxlib.clearEl(body);
    if (alertsLoading()) {
      body.appendChild(el('p', { role: 'status', 'data-alerts-state': 'loading' }, t('copy.dashboard.alerts.loading')));
      return;
    }
    var unavailable = alertsUnavailable();
    var items = collectAlerts(unavailable);
    if (unavailable) {
      body.appendChild(el('p', { role: 'status', 'data-alerts-state': 'unavailable' }, t('copy.dashboard.alerts.unavailable')));
    }
    if (items.length === 0) {
      if (!unavailable) {
        body.appendChild(el('p', { role: 'status', 'data-alerts-state': 'empty' }, t('copy.dashboard.alerts.empty')));
      }
      return;
    }
    body.appendChild(el('ul', { class: 'dashboard-alert-list' }, items.map(renderAlertItem)));
  }

  function renderDashboard(mount, ctx) {
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    if (!PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
    var target = resolveMount(mount);
    if (!target) {
      renderWorkload();
      renderAlerts();
      return;
    }
    var tallies = vmTallies();
    var rows = jobRows();
    var now = Date.now();
    var host = hyperv();
    var unknown = t('copy.dashboard.kpi.loading');
    var specs = [
      {
        kpi: 'vm-running',
        href: '#/vms',
        view: 'vms',
        label: t('copy.dashboard.kpi.vm_running'),
        valueId: 'kpi-vm-running',
        noteId: 'kpi-vm-running-note',
        value: tallies ? String(tallies.running) : '-',
        note: tallies ? '' : unknown
      },
      {
        kpi: 'vm-off',
        href: '#/vms',
        view: 'vms',
        label: t('copy.dashboard.kpi.vm_off'),
        valueId: 'kpi-vm-off',
        noteId: 'kpi-vm-off-note',
        value: tallies ? String(tallies.off) : '-',
        note: tallies ? '' : unknown
      },
      {
        kpi: 'templates',
        href: '#/vms',
        view: 'vms',
        label: t('copy.dashboard.kpi.templates'),
        valueId: 'kpi-templates',
        noteId: 'kpi-templates-note',
        value: tallies ? String(tallies.templates) : '-',
        note: tallies ? '' : unknown
      },
      {
        kpi: 'jobs-active',
        href: '#/jobs',
        view: 'jobs',
        label: t('copy.dashboard.kpi.jobs_active'),
        valueId: 'kpi-jobs-active',
        noteId: 'kpi-jobs-active-note',
        value: String(activeJobs(rows)),
        note: ''
      },
      {
        kpi: 'jobs-failed-24h',
        href: '#/jobs',
        view: 'jobs',
        label: t('copy.dashboard.kpi.jobs_failed_24h'),
        valueId: 'kpi-jobs-failed-24h',
        noteId: 'kpi-jobs-failed-24h-note',
        value: String(failedWithin24h(rows, now)),
        note: ''
      },
      {
        kpi: 'hyperv',
        label: t('copy.dashboard.kpi.hyperv'),
        valueId: 'kpi-hyperv',
        noteId: 'kpi-hyperv-note',
        value: host.value,
        note: host.note,
        hv: host.state
      }
    ];
    PCV.uxlib.clearEl(target);
    specs.forEach(function (spec) { target.appendChild(card(spec)); });
    specs.forEach(function (spec) {
      mirror(spec.valueId, spec.value);
      mirror(spec.noteId, spec.note || '');
    });
    renderWorkload();
    renderAlerts();
  }

  PCV.viewDashboard = { render: renderDashboard };
  PCV.views.register('dashboard', { render: renderDashboard });
})(window.PCV);

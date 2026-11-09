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

  function renderDashboard(mount, ctx) {
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    var target = resolveMount(mount);
    if (!target || !PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
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
  }

  PCV.viewDashboard = { render: renderDashboard };
  PCV.views.register('dashboard', { render: renderDashboard });
})(window.PCV);

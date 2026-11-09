// @ts-nocheck
// Desktop Node module (ADR-0018): activity screen on the U4a view registry.
// The activity panel stays a jobs-screen link. Event rows paint into #event-center-panel.
window.PCV = window.PCV || {};
(function (PCV) {
  var FACET = 'eventSeverity';
  var TONES = ['error', 'warn', 'info', 'ok'];
  var syncingSeverity = false;

  function severityValue() {
    var value = String(state.eventSeverityFilter || 'all').toLowerCase();
    return TONES.indexOf(value) === -1 ? 'all' : value;
  }

  function facetFor(value) {
    return value && value !== 'all' ? [value] : [];
  }

  function syncFacetFromState() {
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs) return;
    var cur = (fs.current() || {})[FACET] || [];
    var want = facetFor(severityValue());
    if (cur.length === want.length && String(cur[0] || '') === String(want[0] || '')) return;
    syncingSeverity = true;
    try { fs.apply({ eventSeverity: want }); }
    finally { syncingSeverity = false; }
  }

  function syncStateFromFacet() {
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs) return;
    var cur = (fs.current() || {})[FACET] || [];
    var next = cur.length === 1 ? String(cur[0]).toLowerCase() : 'all';
    if (TONES.indexOf(next) === -1) next = 'all';
    if (severityValue() === next) return;
    state.eventSeverityFilter = next;
  }

  function toneLabel(tone) {
    var key = {
      error: 'copy.events.tone.error',
      warn: 'copy.events.tone.warn',
      info: 'copy.events.tone.info',
      ok: 'copy.events.tone.ok'
    }[tone];
    return key ? t(key) : (tone || '-');
  }

  function tonePill(tone) {
    var status = tone === 'error' ? 'crit' : tone === 'warn' ? 'warn' : tone === 'ok' ? 'ok' : 'idle';
    return HN.statusPill(status, toneLabel(tone));
  }

  function eventItems() {
    return typeof buildEventCenterItems === 'function' ? buildEventCenterItems() : [];
  }

  function countTone(items, tone) {
    return items.filter(function (item) { return item.tone === tone; }).length;
  }

  function summaryFailure(items) {
    var error = state.summaryError;
    if (!error) return false;
    if (!items.length) return true;
    var code = String(error.code || '');
    var source = String(error.operation || 'problem-details');
    return items.every(function (item) {
      return item && item.label === code && item.source === source;
    });
  }

  function messageText(item) {
    var label = item && item.label ? String(item.label) : '';
    var detail = item && item.detail ? String(item.detail) : '';
    if (label && detail && label !== detail) return label + ' ' + detail;
    return detail || label || '-';
  }

  function countCards(items) {
    var el = PCV.uxlib.el;
    return el('div', { class: 'event-severity-lane' }, TONES.map(function (tone) {
      return el('div', { class: 'event-count-card event-pill event-' + tone },
        el('span', null, toneLabel(tone)),
        el('strong', null, String(countTone(items, tone))));
    }));
  }

  function renderEventsView(mount, ctx) {
    if (!mount) return;
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    var el = PCV.uxlib.el;
    var clearEl = PCV.uxlib.clearEl;
    syncFacetFromState();
    var items = eventItems();
    var severity = severityValue();
    var rows = severity === 'all' ? items : items.filter(function (item) { return item.tone === severity; });
    var failed = summaryFailure(items);
    var filtered = severity !== 'all';
    clearEl(mount);
    mount.appendChild(countCards(items));
    var tools = el('div', { class: 'events-toolbar' });
    tools.appendChild(HN.filterBar([{
      key: FACET,
      label: t('copy.events.filter'),
      singleSelect: true,
      showAll: true,
      allLabel: t('copy.jobs.status.all'),
      allCount: items.length,
      options: TONES.map(function (tone) {
        return { value: tone, label: toneLabel(tone), count: countTone(items, tone) };
      })
    }]));
    mount.appendChild(tools);
    if (state.loading && items.length === 0) {
      mount.appendChild(el('div', { class: 'events-loading', role: 'status' }, t('copy.events.loading')));
      return;
    }
    if (failed && !filtered) {
      var error = state.summaryError || {};
      mount.appendChild(PCV.components.loadError({
        titleKey: 'ui.load_error.title',
        detail: String(error.code || 'PCV_OPS_SUMMARY_FAILED') + ' ' + String(error.message || '')
      }));
      return;
    }
    if (rows.length === 0) {
      var empty = PCV.components.emptyState({
        titleKey: filtered ? 'copy.events.empty_filter_title' : 'copy.events.empty_title',
        bodyKey: filtered ? 'copy.events.empty_filter' : 'copy.events.empty'
      });
      empty.classList.add(filtered ? 'events-empty-filter' : 'events-empty');
      mount.appendChild(empty);
      return;
    }
    var tableHost = el('div', { class: 'events-view-table' });
    mount.appendChild(tableHost);
    createDataTable(tableHost, { tableId: 'dt-events',
      searchable: false,
      exportable: true,
      headers: [
        { label: t('copy.events.col.time'), key: 'time' },
        { label: t('copy.events.col.severity'), key: 'severity' },
        { label: t('copy.events.col.source'), key: 'source' },
        { label: t('copy.events.col.message'), key: 'message' }
      ],
      rows: rows.map(function (item) {
        return ['-', tonePill(item.tone), String(item.source || '-'), messageText(item)];
      })
    });
  }

  if (PCV.ui && PCV.ui.filterState && typeof PCV.ui.filterState.subscribe === 'function') {
    PCV.ui.filterState.subscribe(function () {
      if (syncingSeverity) return;
      var active = (PCV.nav && PCV.nav.activeView && PCV.nav.activeView()) ||
        (typeof state !== 'undefined' && state.activeView);
      if (active !== 'activity') return;
      syncStateFromFacet();
      if (typeof render === 'function') render();
    });
  }

  PCV.viewEvents = { render: renderEventsView };
  PCV.views.register('activity', { render: renderEventsView });
})(window.PCV);

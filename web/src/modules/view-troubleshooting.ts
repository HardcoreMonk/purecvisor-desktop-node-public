// @ts-nocheck
// Desktop Node module (ADR-0018): troubleshooting page on the U4a view registry.
// Cards paint into #troubleshooting-cards. Pinned panels stay on the legacy painters.
window.PCV = window.PCV || {};
(function (PCV) {
  function el() {
    return PCV.uxlib.el;
  }

  function scalar(value) {
    if (value === null || value === undefined || value === '') return '';
    if (typeof value === 'object') {
      return typeof formatPolicyValue === 'function' ? String(formatPolicyValue(value) || '') : '';
    }
    return String(value);
  }

  function hypervState() {
    var label = typeof getHostReadinessLabel === 'function' ? String(getHostReadinessLabel()) : '—';
    if (!label || label === '—' || label === '-') {
      return { state: 'unknown', value: '-', action: t('copy.dashboard.kpi.loading') };
    }
    if (typeof hostReadinessBlocked === 'function' && hostReadinessBlocked()) {
      return { state: 'not-ready', value: label, action: t('copy.mon.host_check') };
    }
    return { state: 'ready', value: label, action: '' };
  }

  function serviceValue() {
    var host = state.host || {};
    var direct = scalar(host.service);
    if (direct) return direct;
    var vmms = host.hyperv && host.hyperv.vmms_running;
    var shown = scalar(vmms);
    if (shown) return shown;
    if (state.connectionState) return String(state.connectionState);
    return '-';
  }

  function listenerValue() {
    var host = state.host || {};
    var direct = scalar(host.listener);
    if (direct) return direct;
    var policy = state.runtimePolicy || {};
    var nested = typeof readNested === 'function'
      ? (readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind']))
      : null;
    var shown = scalar(nested);
    return shown || '-';
  }

  function sessionUser() {
    var session = state.authSession || {};
    return scalar(session.username) || '-';
  }

  function sessionRole() {
    if (typeof getAccountRoleLabel === 'function') return String(getAccountRoleLabel() || '-');
    var session = state.authSession || {};
    return scalar(session.role) || '-';
  }

  function onLogout() {
    if (typeof logoutAccount !== 'function') return;
    var result = logoutAccount();
    if (result && typeof result.catch === 'function') result.catch(function () {});
  }

  function clickPanelAction(action) {
    var panel = document.getElementById('diagnostics-panel');
    if (!panel || typeof panel.querySelector !== 'function') return;
    var real = panel.querySelector('[data-action="' + action + '"]');
    if (real && typeof real.click === 'function') real.click();
  }

  function resolveMount(passed) {
    if (passed && typeof passed.hasAttribute === 'function' && passed.hasAttribute('data-view-mount')) return passed;
    var byId = document.getElementById('troubleshooting-cards');
    if (byId && byId !== passed) return byId;
    return null;
  }

  function paintCards(target) {
    var node = el();
    var hv = hypervState();
    var policy = typeof getDiagnosticActionPolicy === 'function' ? getDiagnosticActionPolicy() : {};
    var canCreate = typeof rbacAllows !== 'function' || rbacAllows('diagnostics.create');
    var canDownload = typeof rbacAllows !== 'function' || rbacAllows('diagnostics.read');
    PCV.uxlib.clearEl(target);
    target.appendChild(node('section', { 'data-trouble-card': 'hyperv', 'data-hv-state': hv.state },
      node('h2', null, t('copy.trouble.card.hyperv')),
      node('strong', { 'data-hv-value': '1' }, hv.value),
      node('p', { 'data-hv-action': '1' }, hv.action)));
    target.appendChild(node('section', { 'data-trouble-card': 'service' },
      node('h2', null, t('copy.trouble.card.service')),
      node('span', null, t('copy.trouble.service')),
      node('strong', { 'data-trouble-service': '1' }, serviceValue()),
      node('span', null, t('copy.trouble.listener')),
      node('strong', { 'data-trouble-listener': '1' }, listenerValue())));
    target.appendChild(node('section', { 'data-trouble-card': 'session' },
      node('h2', null, t('copy.trouble.card.session')),
      node('span', null, t('copy.trouble.user')),
      node('strong', { 'data-trouble-user': '1' }, sessionUser()),
      node('span', null, t('copy.trouble.role')),
      node('strong', { 'data-trouble-role': '1' }, sessionRole()),
      node('button', { type: 'button', 'data-action': 'account-logout', 'data-trouble-logout': '1', onclick: onLogout }, t('copy.trouble.logout'))));
    target.appendChild(node('section', { 'data-trouble-card': 'bundle' },
      node('h2', null, t('copy.trouble.card.bundle')),
      node('button', {
        type: 'button',
        'data-action': 'diagnostic-create',
        'data-trouble-bundle': 'create',
        disabled: policy.createDisabled || !canCreate,
        onclick: function () { clickPanelAction('diagnostic-create'); }
      }, t('copy.mon.create_bundle')),
      node('button', {
        type: 'button',
        'data-action': 'diagnostic-download',
        'data-trouble-bundle': 'download',
        disabled: policy.downloadDisabled || !canDownload,
        onclick: function () { clickPanelAction('diagnostic-download'); }
      }, t('copy.trouble.download'))));
  }

  function bridgeModel() {
    var bridge = typeof getRuntimeApiRegistryBridge === 'function' ? getRuntimeApiRegistryBridge() : {};
    bridge = typeof asObject === 'function' ? asObject(bridge) : (bridge || {});
    var contractKey = bridge.contract_key || bridge.contractKey || '';
    var source = bridge.handler_registry_source || bridge.handlerRegistrySource || bridge.source || '';
    var anchor = bridge.documentation_anchor || bridge.documentationAnchor || bridge.anchor || '';
    var routeKeys = (typeof asArray === 'function' ? asArray(bridge.route_keys || bridge.routeKeys) : [])
      .map(function (route) { return String(route || '').trim(); })
      .filter(Boolean);
    return { contractKey: contractKey, source: source, anchor: anchor, routeKeys: routeKeys };
  }

  function bucketModel() {
    var descriptor = typeof getCurrentEvidenceHostOps === 'function' ? getCurrentEvidenceHostOps() : {};
    descriptor = typeof asObject === 'function' ? asObject(descriptor) : (descriptor || {});
    var buckets = (typeof asArray === 'function' ? asArray(descriptor.buckets) : [])
      .map(function (bucket) { return typeof asObject === 'function' ? asObject(bucket) : (bucket || {}); })
      .filter(function (bucket) { return bucket.bucket_key || bucket.bucketKey; });
    var contractKey = descriptor.lifecycle_bucket_contract_key || descriptor.lifecycleBucketContractKey || '';
    var mutationPerformed = descriptor.host_mutation_performed ?? descriptor.hostMutationPerformed;
    return {
      buckets: buckets,
      status: descriptor.status || '',
      titleKey: descriptor.contract_key || descriptor.contractKey || '',
      contractKey: contractKey,
      mutationLabel: mutationPerformed === true ? t('copy.mon.mutation_reported') : t('copy.mon.mutation_diagnostics')
    };
  }

  function paintAdvanced() {
    var body = document.getElementById('troubleshooting-advanced-body');
    if (!body || !PCV.uxlib || typeof PCV.uxlib.clearEl !== 'function') return;
    var node = el();
    var bridge = bridgeModel();
    var buckets = bucketModel();
    var routeLabel = bridge.routeKeys.length
      ? t('copy.trouble.bridge.routes', { count: bridge.routeKeys.length })
      : t('copy.trouble.bridge.routes_missing');
    PCV.uxlib.clearEl(body);
    body.appendChild(node('div', { 'data-trouble-bridge': '1', class: 'diagnostics-result runtime-api-registry-bridge' },
      node('span', { class: 'muted' }, t('copy.trouble.bridge.title')),
      node('strong', { 'data-trouble-bridge-key': '1' }, bridge.contractKey || t('copy.trouble.bridge.not_reported')),
      node('p', null, [bridge.source || t('copy.trouble.bridge.source_missing'), routeLabel].join(' / ')),
      node('p', null, bridge.anchor || t('copy.trouble.bridge.anchor_missing')),
      bridge.routeKeys.length
        ? node('ul', { class: 'diagnostics-route-list', 'data-trouble-routes': '1' }, bridge.routeKeys.map(function (route) {
          return node('li', null, route);
        }))
        : node('p', { 'data-trouble-routes': '0' }, t('copy.trouble.bridge.detail_missing')),
      node('div', { class: 'boundary-chip-row' },
        node('span', { 'data-trouble-chip': 'bridge' }, bridge.contractKey ? t('copy.trouble.bridge.expose') : t('copy.trouble.bridge.absent')),
        node('span', { 'data-trouble-chip': 'routes' }, t('copy.trouble.bridge.meta')))));
    var rows = buckets.buckets.map(function (bucket) {
      var operations = (typeof asArray === 'function' ? asArray(bucket.operations) : [])
        .map(function (operation) { return String(operation || '').trim(); })
        .filter(Boolean);
      return node('tr', { 'data-trouble-bucket': bucket.bucket_key || bucket.bucketKey },
        node('td', null, bucket.bucket_key || bucket.bucketKey),
        node('td', null, bucket.owner || '-'),
        node('td', null, bucket.mutation_boundary || bucket.mutationBoundary || '-'),
        node('td', null, operations.join(', ') || '-'));
    });
    body.appendChild(node('div', { 'data-trouble-hostops': '1', class: 'diagnostics-result hostops-lifecycle-buckets' },
      node('span', { class: 'muted' }, t('copy.trouble.hostops.title')),
      node('strong', null, buckets.buckets.length
        ? (buckets.titleKey || 'host-ops-lifecycle-descriptor-bridge-v1')
        : (buckets.status || t('copy.trouble.hostops.not_reported'))),
      node('p', { 'data-trouble-hostops-contract': '1' }, buckets.contractKey || t('copy.trouble.hostops.contract_missing')),
      node('div', { class: 'evidence-table-wrap diagnostics-hostops-table' },
        node('table', { 'data-trouble-buckets': '1', class: 'evidence-table' },
          node('thead', null, node('tr', null,
            node('th', null, t('copy.trouble.hostops.col.bucket')),
            node('th', null, t('copy.trouble.hostops.col.owner')),
            node('th', null, t('copy.trouble.hostops.col.boundary')),
            node('th', null, t('copy.trouble.hostops.col.ops')))),
          node('tbody', null, rows))),
      node('div', { class: 'boundary-chip-row' },
        node('span', { 'data-trouble-chip': 'mutation' }, buckets.mutationLabel),
        buckets.buckets.length
          ? node('span', { 'data-trouble-chip': 'separated' }, t('copy.trouble.hostops.separated'))
          : node('span', { 'data-trouble-chip': 'meta' }, t('copy.trouble.hostops.meta')))));
    body.appendChild(node('div', { class: 'boundary-chip-row', 'data-trouble-boundaries': '1' },
      node('span', { 'data-trouble-chip': 'token' }, t('copy.trouble.chip.token')),
      node('span', { 'data-trouble-chip': 'blobs' }, t('copy.trouble.chip.blobs')),
      node('span', { 'data-trouble-chip': 'signing' }, t('copy.trouble.chip.signing')),
      node('span', { 'data-trouble-chip': 'publication' }, t('copy.trouble.chip.publication'))));
  }

  function renderTroubleshootingView(mount, ctx) {
    if (ctx && typeof ctx.isCurrent === 'function' && !ctx.isCurrent()) return;
    if (!PCV.uxlib || typeof PCV.uxlib.el !== 'function' || typeof PCV.uxlib.clearEl !== 'function') return;
    var target = resolveMount(mount);
    if (target) paintCards(target);
    paintAdvanced();
  }

  PCV.viewTroubleshooting = { render: renderTroubleshootingView, paintAdvanced: paintAdvanced };
  PCV.views.register('troubleshooting', { render: renderTroubleshootingView });
})(window.PCV);

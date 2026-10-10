// @ts-nocheck
// Ported from purecvisor ui/modules/mobile.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1). Desktop Node changes: tabs are home, alerts and power (no self-healing);
// containers, Suricata/IPS, push toggles, silences and RPC calls are gone; VM data comes from PCV.api + EP.VM_LIST and
// alerts from the Desktop Node event center snapshot; power actions go through PCV.vm.
window.PCV = window.PCV || {};
(function (PCV) {
  'use strict';
  var MQ = '(max-width: 600px)';
  var SCREEN_ID = 'm-screen';
  function _t(ko, en) { return (typeof _L === 'function') ? _L(ko, en) : ko; }
  var TABS = [
    { id: 'home',   ko: '홈',   en: 'Home',   icon: 'ci-house-01' },
    { id: 'alerts', ko: '알림', en: 'Alerts', icon: 'ci-bell', badge: 'alerts' },
    { id: 'power',  ko: '전원', en: 'Power',  icon: 'ci-play' }
  ];
  var _state = { activeTab: 'home', badges: { alerts: 0 } };
  var MOBILE_SVG_NS = 'http://www.w3.org/2000/svg';
  function _mobileIcon(symbol) {
    var svg = document.createElementNS(MOBILE_SVG_NS, 'svg');
    svg.setAttribute('class', 'ci-icon mnav-icon');
    svg.setAttribute('aria-hidden', 'true');
    svg.setAttribute('focusable', 'false');
    var use = document.createElementNS(MOBILE_SVG_NS, 'use');
    use.setAttribute('href', 'vendor/coolicons/coolicons.svg#' + symbol);
    svg.appendChild(use);
    return svg;
  }
  function _pageHead(title, subtitle) {
    var el = PCV.uxlib.el;
    return el('div', { class: 'm-pagehead' },
      el('h1', { class: 'm-page-title' }, title),
      el('p', { class: 'm-page-subtitle' }, subtitle));
  }
  function vmStatus(state) { return state === 'running' ? 'ok' : (state === 'paused' ? 'warn' : 'idle'); }
  function sevStatus(sev) { return (sev === 'crit' || sev === 'critical') ? 'crit' : ((sev === 'warn' || sev === 'warning') ? 'warn' : 'idle'); }
  function _alertSev(a) { return (window.HN && HN.alertSeverity) ? HN.alertSeverity(a) : sevStatus(a && a.severity); }
  function _alertLabel(a) {
    var severity = _alertSev(a);
    return severity === 'crit' ? 'CRIT' : (severity === 'warn' ? 'WARN' : 'INFO');
  }
  function _alerts() {
    if (PCV.ops && typeof PCV.ops.alertList === 'function') return PCV.ops.alertList() || [];
    return window.alertList || [];
  }
  function _vms() {
    return (PCV.state && PCV.state.vmList) || window.vmList || [];
  }
  function isActive() { return document.body.classList.contains('mshell'); }
  function _buildNav() {
    var el = PCV.uxlib.el;
    var items = TABS.map(function (tab) {
      var badge = tab.badge
        ? el('span', { class: 'mnav-badge', 'data-badge': tab.badge, hidden: '' }, '0')
        : null;
      return el('button', {
        class: 'mnav-item' + (tab.id === _state.activeTab ? ' on' : ''),
        type: 'button', 'data-tab': tab.id, 'aria-label': _t(tab.ko, tab.en),
        onClick: function () { showTab(tab.id); }
      },
        _mobileIcon(tab.icon),
        el('span', { class: 'mnav-label' }, _t(tab.ko, tab.en)),
        badge);
    });
    return el.apply(null, ['nav', { class: 'mnav', 'aria-label': _t('모바일 탭', 'Mobile tabs') }].concat(items));
  }
  function _syncNavActive() {
    var items = document.querySelectorAll('.mnav .mnav-item');
    Array.prototype.forEach.call(items, function (it) {
      var on = it.getAttribute('data-tab') === _state.activeTab;
      it.classList.toggle('on', on);
      it.setAttribute('aria-current', on ? 'page' : 'false');
    });
  }
  function _tabFromHash() {
    var h = (location.hash || '').replace(/^#\/?/, '');
    if (!h) return null;
    if (/activity|alert/.test(h)) return 'alerts';
    if (/vms|jobs/.test(h)) return 'power';
    return null;
  }
  var _hashConsumed = false;
  function mount() {
    if (document.getElementById(SCREEN_ID)) { _syncNavActive(); return; }
    document.body.classList.add('mshell');
    var scr = PCV.uxlib.el('div', { id: SCREEN_ID, class: 'mscreen', role: 'main' });
    document.body.appendChild(scr);
    document.body.appendChild(_buildNav());
    if (!_hashConsumed) {
      _hashConsumed = true;
      var deep = _tabFromHash();
      if (deep) _state.activeTab = deep;
    }
    showTab(_state.activeTab);
    refreshBadges();
  }
  function unmount() {
    if (_deleteSelection) _deleteSelection.clear();
    document.body.classList.remove('mshell');
    var scr = document.getElementById(SCREEN_ID);
    if (scr) scr.remove();
    var nav = document.querySelector('.mnav');
    if (nav) nav.remove();
  }
  function sync() {
    var phone = window.matchMedia(MQ).matches;
    var authed = !!window.authToken && !document.body.classList.contains('login-active');
    if (phone && authed && !document.getElementById(SCREEN_ID)) mount();
    else if ((!phone || !authed) && document.getElementById(SCREEN_ID)) unmount();
  }
  function showTab(id) {
    if (!SCREENS[id]) return;
    _state.activeTab = id;
    _syncNavActive();
    var scr = document.getElementById(SCREEN_ID);
    if (!scr) return;
    PCV.uxlib.clearEl(scr);
    SCREENS[id]();
  }
  function setBadges(counts) {
    counts = counts || {};
    if (counts.alerts != null) _state.badges.alerts = counts.alerts;
    Object.keys(_state.badges).forEach(function (key) {
      var b = document.querySelector('.mnav .mnav-badge[data-badge="' + key + '"]');
      if (!b) return;
      var n = _state.badges[key] || 0;
      b.textContent = String(n);
      if (n > 0) b.removeAttribute('hidden'); else b.setAttribute('hidden', '');
    });
  }
  function _paint(node) {
    var scr = document.getElementById(SCREEN_ID);
    if (!scr) return;
    PCV.uxlib.clearEl(scr);
    scr.appendChild(node);
    if (window.currentUser && typeof applyRoleVisibility === 'function') {
      applyRoleVisibility(window.currentUser.role);
    }
  }
  function _empty(text) {
    return PCV.uxlib.el('div', { class: 'mscreen-body' },
      PCV.uxlib.el('div', { class: 'mscreen-empty' }, text));
  }
  function buildHome(d) {
    var el = PCV.uxlib.el;
    d = d || { vms: [], alerts: [], hostCpu: 0, hostMem: 0 };
    var vms = d.vms || [], alerts = d.alerts || [];
    var vmRun = vms.filter(function (v) { return v.state === 'running'; }).length;
    var critWarn = alerts.filter(function (a) { var s = _alertSev(a); return s === 'crit' || s === 'warn'; }).length;
    var bar = HN.statusBar([
      { count: vmRun, label: _t('VM 실행', 'VMs up'), status: vmRun === vms.length ? 'ok' : 'warn', sub: '/ ' + vms.length },
      { count: critWarn, label: _t('알림', 'Alerts'), status: critWarn > 0 ? 'crit' : 'ok', sub: _t('미해결', 'open') }
    ], { onNavigate: function (s) { if (s.label === _t('알림', 'Alerts')) showTab('alerts'); else showTab('power'); } });
    var cpu = Math.round(d.hostCpu || 0), mem = Math.round(d.hostMem || 0);
    var gauges = el('div', { class: 'sg grid-2' },
      HN.gauge({ value: cpu, warn: 80, crit: 95, unit: '%', label: 'CPU' }),
      HN.gauge({ value: mem, warn: 80, crit: 95, unit: '%', label: _t('메모리', 'Memory') }));
    var quick = el('div', { class: 'm-quick' },
      el('button', { class: 'btn', type: 'button', onClick: function () { showTab('power'); } }, _t('전원', 'Power')),
      el('button', { class: 'btn', type: 'button', onClick: function () { showTab('alerts'); } }, _t('알림', 'Alerts')),
      el('button', { class: 'btn btn-g', type: 'button', 'data-role': 'OPERATOR,ADMIN', onClick: function () { if (typeof window.showCreate === 'function') window.showCreate(); } }, '+ VM'));
    var recentItems = alerts.slice(-3).reverse().map(function (a) {
      var record = (a && typeof a === 'object' && !Array.isArray(a)) ? a : {};
      return el('div', { class: 'm-listcard' },
        HN.statusPill(_alertSev(record), _alertLabel(record)),
        el('span', { class: 'm-name' }, record.message || record.title || ''));
    });
    var recent = el('div', { class: 'm-recent' }, recentItems.length
      ? el.apply(null, ['div', { class: 'mscreen-body' }].concat(recentItems))
      : el('div', { class: 'm-hint' }, _t('최근 알림 없음', 'No recent alerts')));
    return el('div', { class: 'mscreen-body' },
      _pageHead(_t('운영 개요', 'Operations overview'),
        _t('Desktop Node 호스트 상태와 빠른 작업', 'Desktop Node status and quick actions')),
      bar, gauges,
      el('div', { class: 'mscreen-section-title' }, _t('빠른 작업', 'Quick actions')), quick,
      el('div', { class: 'mscreen-section-title' }, _t('최근 알림', 'Recent alerts')), recent);
  }
  function buildAlerts(d) {
    var el = PCV.uxlib.el;
    d = d || { alerts: [], filter: [] };
    var alerts = d.alerts || [], filter = d.filter || [];
    var counts = { crit: 0, warn: 0 };
    alerts.forEach(function (a) { var s = _alertSev(a); if (s === 'crit') counts.crit++; else if (s === 'warn') counts.warn++; });
    var bar = HN.filterBar([{ key: 'severity', options: [
      { value: 'crit', label: _t('심각', 'Critical'), count: counts.crit, sw: 'crit' },
      { value: 'warn', label: _t('경고', 'Warning'), count: counts.warn, sw: 'warn' }
    ] }]);
    var shown = filter.length
      ? alerts.filter(function (a) { return filter.indexOf(_alertSev(a)) !== -1; })
      : alerts;
    var rows = shown.slice().reverse().map(function (a) {
      var record = (a && typeof a === 'object' && !Array.isArray(a)) ? a : {};
      return el('div', { class: 'm-alert-row m-listcard' },
        HN.statusPill(_alertSev(record), _alertLabel(record)),
        el('div', { class: 'm-name m-copy' },
          el('div', null, record.message || record.title || ''),
          el('div', { class: 'm-hint', style: 'padding:0' }, (record.source || record.kind || '') + (record.time ? ' · ' + record.time : ''))));
    });
    var list = rows.length
      ? el.apply(null, ['div', { class: 'mscreen-body' }].concat(rows))
      : el('div', { class: 'm-hint' }, _t('알림 없음', 'No alerts'));
    return el('div', { class: 'mscreen-body' },
      _pageHead(_t('알림', 'Alerts'),
        _t('심각 ' + counts.crit + ' · 경고 ' + counts.warn,
          counts.crit + ' critical · ' + counts.warn + ' warning')),
      bar,
      el('div', { class: 'mscreen-section-title' }, _t('알림 이력', 'Alert history')), list);
  }
  var _deleteSelection = new Map();
  function buildPower(d) {
    var el = PCV.uxlib.el;
    d = d || { vms: [] };
    var vms = d.vms || [];
    _deleteSelection.forEach(function (subject, name) {
      if (!vms.some(function (v) { return v.name === name; })) _deleteSelection.delete(name);
    });
    var bulkDelete = el('button', { class: 'btn btn-r m-vm-bulk-delete', type: 'button',
      style: 'min-height:40px', 'data-role': 'OPERATOR,ADMIN',
      onClick: function () { if (PCV.vm && typeof PCV.vm.bulkDelete === 'function') PCV.vm.bulkDelete(Array.from(_deleteSelection.values())); } });
    function updateDeleteCount() {
      bulkDelete.disabled = !_deleteSelection.size;
      bulkDelete.textContent = _t('일괄 삭제', 'Bulk delete') + ' (' + _deleteSelection.size + ')';
    }
    updateDeleteCount();
    function pbtn(label, cls, fn) { return el('button', { class: 'btn ' + cls, type: 'button', 'data-role': 'OPERATOR,ADMIN', onClick: fn }, label); }
    var vmCards = vms.map(function (v) {
      var st = vmStatus(v.state);
      var actions;
      if (v.state === 'running') {
        actions = el('div', { class: 'm-actions' },
          pbtn(_t('종료', 'Shut down'), 'btn-r', function () { _vmPowerAction(v.name, 'shutdown'); }),
          pbtn(_t('일시정지', 'Pause'), '', function () { _vmPowerAction(v.name, 'pause'); }));
      } else if (v.state === 'paused') {
        actions = el('div', { class: 'm-actions' },
          pbtn(_t('재개', 'Resume'), 'btn-g', function () { _vmPowerAction(v.name, 'resume'); }),
          pbtn(_t('종료', 'Shut down'), 'btn-r', function () { _vmPowerAction(v.name, 'shutdown'); }));
      } else {
        actions = el('div', { class: 'm-actions' },
          pbtn(_t('시작', 'Start'), 'btn-g', function () { _vmPowerAction(v.name, 'start'); }));
      }
      var subject = { name: v.name, id: v.id || v.name };
      actions.appendChild(pbtn(_t('VM 삭제', 'Delete VM'), 'btn-r m-vm-delete', function () { if (PCV.vm && typeof PCV.vm.vmDel === 'function') PCV.vm.vmDel(subject); }));
      var row = el('div', { class: 'm-vm m-listcard' },
        el('label', { class: 'm-vm-select', 'data-role': 'OPERATOR,ADMIN' },
          el('input', { type: 'checkbox', checked: _deleteSelection.has(v.name) ? '' : null,
            'aria-label': _t('선택: ', 'Select ') + v.name,
            onChange: function (e) {
              if (e.target.checked) _deleteSelection.set(v.name, subject); else _deleteSelection.delete(v.name);
              updateDeleteCount();
            } })),
        HN.statusDot(st, st === 'ok' ? { glow: true } : null),
        el('span', { class: 'm-name' }, v.name),
        HN.statusPill(st, v.state || '?'),
        actions);
      var hint = el('div', { class: 'm-hint' }, v.state === 'running'
        ? _t('콘솔: 브라우저 화면 사용 가능 (데스크톱)', 'Console: browser screen available (desktop)')
        : _t('콘솔: VM 정지됨', 'Console: VM stopped'));
      return el('div', null, row, hint);
    });
    var vmSection = vmCards.length ? el.apply(null, ['div', { class: 'mscreen-body' }].concat(vmCards)) : el('div', { class: 'm-hint' }, _t('VM 없음', 'No VMs'));
    return el('div', { class: 'mscreen-body' },
      _pageHead(_t('전원 관리', 'Power controls'), 'VM ' + vms.length),
      el('div', { class: 'mscreen-section-title' }, 'VM'),
      el('div', { 'data-role': 'OPERATOR,ADMIN' }, bulkDelete,
        el('p', { class: 'm-hint' }, _t('체크박스로 삭제할 VM을 선택하세요.', 'Select VMs to delete using the checkboxes.'))), vmSection);
  }
  function _ready() {
    return !!(window.authToken && PCV.api && typeof PCV.api.fetchGet === 'function' && typeof EP !== 'undefined' && EP.VM_LIST);
  }
  async function _loadVms() {
    if (!_ready()) return _vms();
    var r = await PCV.api.fetchGet(EP.VM_LIST());
    var list = (typeof unwrapList === 'function') ? unwrapList(r) : ((r && r.data && (r.data.items || r.data.vms)) || []);
    return list.length ? list : _vms();
  }
  async function showHome() {
    var scr = document.getElementById(SCREEN_ID); if (!scr) return;
    if (!scr.querySelector('.mscreen-body') && typeof showSkeleton === 'function') showSkeleton(scr, 4);
    if (!window.authToken) { _paint(_empty(_t('로그인 후 이용 가능', 'Sign in to continue'))); return; }
    var vms = await _loadVms();
    var alerts = _alerts();
    var d = {
      vms: vms, alerts: alerts,
      hostCpu: (PCV.metrics && PCV.metrics.latest('host.cpu')) || 0,
      hostMem: (PCV.metrics && PCV.metrics.latest('host.mem')) || 0
    };
    setBadges({ alerts: alerts.filter(function (a) { var s = _alertSev(a); return s === 'crit' || s === 'warn'; }).length });
    if (_state.activeTab === 'home') _paint(buildHome(d));
  }
  async function showAlerts() {
    var scr = document.getElementById(SCREEN_ID); if (!scr) return;
    if (!scr.querySelector('.mscreen-body') && typeof showSkeleton === 'function') showSkeleton(scr, 4);
    if (!window.authToken) { _paint(_empty(_t('로그인 후 이용 가능', 'Sign in to continue'))); return; }
    _alertsData = { alerts: _alerts() };
    setBadges({ alerts: _alertsData.alerts.filter(function (a) { var s = _alertSev(a); return s === 'crit' || s === 'warn'; }).length });
    if (_state.activeTab === 'alerts') { _renderAlerts(); _ensureAlertsSub(); }
  }
  async function showPower() {
    var scr = document.getElementById(SCREEN_ID); if (!scr) return;
    if (!scr.querySelector('.mscreen-body') && typeof showSkeleton === 'function') showSkeleton(scr, 4);
    if (!window.authToken) { _paint(_empty(_t('로그인 후 이용 가능', 'Sign in to continue'))); return; }
    var vms = await _loadVms();
    if (_state.activeTab === 'power') _paint(buildPower({ vms: vms }));
  }
  var _alertsData = { alerts: [] };
  var _alertsSub = null;
  function _renderAlerts() {
    var fs = PCV.ui && PCV.ui.filterState;
    var filter = (fs && fs.current().severity) || [];
    _paint(buildAlerts({ alerts: _alertsData.alerts, filter: filter }));
  }
  function _ensureAlertsSub() {
    if (_alertsSub) return;
    var fs = PCV.ui && PCV.ui.filterState;
    if (!fs || !fs.subscribe) return;
    _alertsSub = fs.subscribe(function () { if (_state.activeTab === 'alerts') _renderAlerts(); });
  }
  async function _vmPowerAction(name, action) {
    var vms = _vms();
    var idx = -1;
    for (var i = 0; i < vms.length; i++) { if (vms[i].name === name) { idx = i; break; } }
    if (idx < 0) { toast(_t('VM 목록 동기화 필요', 'VM list out of sync'), false); return; }
    var target = { name: vms[idx].name, id: vms[idx].id || vms[idx].name };
    if (action === 'shutdown') {
      var ok = await customConfirm(_t('VM 종료', 'Shut down VM'), name + ' — ' + _t('종료하시겠습니까?', 'Shut down this VM?'));
      if (!ok) return;
    }
    if (PCV.vm && typeof PCV.vm.lifecycle === 'function') { PCV.vm.lifecycle(target, action); return; }
    if (typeof window.vmPower === 'function') window.vmPower(action, target);
  }
  var SCREENS = { home: showHome, alerts: showAlerts, power: showPower };
  var _mq = window.matchMedia(MQ);
  if (_mq.addEventListener) _mq.addEventListener('change', sync);
  else if (_mq.addListener) _mq.addListener(sync);
  var _rzT = null;
  window.addEventListener('resize', function () { if (_rzT) clearTimeout(_rzT); _rzT = setTimeout(sync, 150); });
  queueMicrotask(sync);
  if (typeof MutationObserver === 'function') {
    new MutationObserver(sync).observe(document.body, { attributes: true, attributeFilter: ['class'] });
  }
  function refreshActive() { if (SCREENS[_state.activeTab]) SCREENS[_state.activeTab](); }
  function refreshBadges() {
    if (!window.authToken) return;
    var alerts = _alerts().filter(function (a) { var s = _alertSev(a); return s === 'crit' || s === 'warn'; }).length;
    setBadges({ alerts: alerts });
  }
  function _wireLiveRefresh() {
    if (window._mLoadAllWrapped) return;
    if (typeof window.loadAll !== 'function') return;
    window._mLoadAllWrapped = true;
    var orig = window.loadAll;
    window.loadAll = function () {
      var r = orig.apply(this, arguments);
      if (isActive()) { refreshActive(); refreshBadges(); }
      return r;
    };
  }
  window.addEventListener('load', _wireLiveRefresh);
  PCV.mobile = {
    sync: sync, mount: mount, unmount: unmount, isActive: isActive,
    showTab: showTab, setBadges: setBadges,
    buildHome: buildHome, buildAlerts: buildAlerts, buildPower: buildPower,
    vmStatus: vmStatus, sevStatus: sevStatus,
    TABS: TABS,
    _wireLiveRefresh: _wireLiveRefresh,
    get activeTab() { return _state.activeTab; }
  };
})(window.PCV);

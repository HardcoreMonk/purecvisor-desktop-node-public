// @ts-nocheck
// Ported from purecvisor ui/modules/shell.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1). Desktop Node changes: the nav model lists the Desktop Node views only
// (dashboard, vms, network, jobs, activity, evidence, troubleshooting, helppage), the brand reads DESKTOP NODE, the
// status bar segments come from the Desktop Node snapshot (vms, jobs, alerts, host, evidence) and VM sub-tabs are gone.
window.PCV = window.PCV || {};
(function (PCV) {
var VM_TABS = [];
function L(ko, en) { return (typeof _L === 'function') ? _L(ko, en) : ko; }
var SHELL_SVG_NS = 'http://www.w3.org/2000/svg';
function shellIcon(symbol, cls) {
  var svg = document.createElementNS(SHELL_SVG_NS, 'svg');
  svg.setAttribute('class', 'ci-icon' + (cls ? ' ' + cls : ''));
  svg.setAttribute('aria-hidden', 'true');
  svg.setAttribute('focusable', 'false');
  var use = document.createElementNS(SHELL_SVG_NS, 'use');
  use.setAttribute('href', 'vendor/coolicons/coolicons.svg#' + symbol);
  svg.appendChild(use);
  return svg;
}
function NAV_SECTIONS() {
  return [
    { label: L('운영', 'Operations'), items: [
      { id: 'dashboard', key: 'nav.dashboard', ico: 'ci-house-01' } ] },
    { label: L('워크로드', 'Workloads'), items: [
      { id: 'vms', key: 'nav.vms', ico: 'ci-desktop', cnt: 'vms' },
      { id: 'templates', key: 'nav.templates', ico: 'ci-file-document' } ] },
    { label: L('인프라', 'Infrastructure'), items: [
      { id: 'network', key: 'nav.network', ico: 'ci-globe' } ] },
    { label: L('관제', 'Monitoring'), items: [
      { id: 'jobs', key: 'nav.jobs', ico: 'ci-refresh', dot: 'jobs' },
      { id: 'activity', key: 'nav.activity', ico: 'ci-warning', dot: 'alerts' },
      { id: 'evidence', key: 'nav.evidence', ico: 'ci-file-document', dot: 'evidence' } ] },
    { label: L('시스템', 'System'), items: [
      { id: 'accounts', key: 'nav.accounts', ico: 'ci-users' },
      { id: 'troubleshooting', key: 'nav.troubleshooting', ico: 'ci-settings' } ] },
    { label: L('도움말', 'Help'), items: [
      { id: 'helppage', key: 'nav.help', ico: 'ci-info' } ] }
  ];
}
function signedInAccountName() {
  var session = (typeof state !== 'undefined' && state && state.authSession) || null;
  var account = session && session.account;
  return String((session && session.username) || (account && account.username) || '').trim();
}
function passwordChangeEntryVisible() {
  if (typeof state === 'undefined' || !state) return false;
  if (!String(state.authAccessToken || '').trim()) return false;
  var name = signedInAccountName();
  if (!name || name.toLowerCase() === 'loopback-session') return false;
  return true;
}
function bindPasswordChangeEntry() {
  var root = document.documentElement;
  if (!root || root._pcvPasswordChangeBound) return;
  root._pcvPasswordChangeBound = true;
  document.addEventListener('click', function (event) {
    var target = event.target;
    var button = target && target.closest ? target.closest('[data-action="account-change-password"]') : null;
    if (!button || button.hidden || button.disabled) return;
    if (typeof openAccountChangeModal === 'function') openAccountChangeModal();
  });
}
function syncPasswordChangeEntry() {
  bindPasswordChangeEntry();
  var host = document.getElementById('us') || document.getElementById('shell-topbar');
  if (!host) return;
  var button = document.getElementById('account-change-password');
  if (!button) {
    button = document.createElement('button');
    button.type = 'button';
    button.className = 'btn';
    button.id = 'account-change-password';
    button.setAttribute('data-action', 'account-change-password');
    host.appendChild(button);
  }
  button.textContent = (typeof t === 'function') ? t('account.change.open') : '비밀번호 변경';
  button.hidden = !passwordChangeEntryVisible();
}
function syncAccountNav() {
  var node = document.querySelector('#shell-sidebar [data-nav="accounts"]');
  if (node) {
    var allowed = typeof canManageAccounts !== 'function' || canManageAccounts();
    node.hidden = !allowed;
  }
  syncPasswordChangeEntry();
}
var _snapshot = null;
function itemLabel(it) { return t(it.key); }
function navTarget(id) {
  return id;
}
function buildSidebar() {
  var el = PCV.uxlib.el;
  var root = document.getElementById('shell-sidebar');
  if (!root) return;
  PCV.uxlib.clearEl(root);
  root.appendChild(el('div', { class: 'shell-brand' },
    el('div', { class: 'shell-brand-mark', 'aria-hidden': 'true' }, 'P'),
    el('div', null,
      el('div', { class: 'shell-brand-name' }, 'PureCVisor'),
      el('div', { class: 'shell-brand-edi' }, 'DESKTOP NODE'))));
  var wrap = el('nav', { class: 'shell-navwrap', role: 'navigation', 'aria-label': t('copy.shell.nav') });
  NAV_SECTIONS().forEach(function (sec) {
    var box = el('div', { class: 'shell-navsec' });
    if (sec.label) box.appendChild(el('div', { class: 'shell-navlbl' }, sec.label));
    sec.items.forEach(function (it) {
      var attrs = {
        class: 'shell-navitem', 'data-nav': it.id, role: 'link', tabindex: '-1',
        onClick: function () {
          if (typeof navigateTo !== 'function') return;
          navigateTo(navTarget(it.id), {
            after: function () { if (typeof closeMobileSB === 'function') closeMobileSB(); }
          });
        }
      };
      if (it.role) attrs['data-role'] = it.role;
      var node = el('div', attrs,
        shellIcon(it.ico || 'ci-info', 'shell-ico'),
        el('span', { class: 'shell-nm' }, itemLabel(it)),
        it.cnt ? el('span', { class: 'shell-cnt', 'data-cnt': it.cnt }, '—') : null,
        it.dot ? el('span', { class: 'shell-dot', 'data-dot': it.dot, hidden: '' }) : null);
      box.appendChild(node);
    });
    wrap.appendChild(box);
  });
  wrap.addEventListener('keydown', function (e) {
    if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
    var allItems = Array.prototype.slice.call(wrap.querySelectorAll('.shell-navitem'));
    var items = allItems.filter(function (n) {
      return n.getClientRects().length > 0 && getComputedStyle(n).visibility === 'visible';
    });
    var idx = items.indexOf(document.activeElement);
    if (idx === -1) return;
    e.preventDefault();
    var n = items.length, d = e.key === 'ArrowDown' ? 1 : -1;
    var next = items[((idx + d) % n + n) % n];
    allItems.forEach(function (m) { m.setAttribute('tabindex', m === next ? '0' : '-1'); });
    next.focus();
  });
  var first = wrap.querySelector('.shell-navitem');
  if (first) first.setAttribute('tabindex', '0');
  root.appendChild(wrap);
  syncAccountNav();
}
function buildTopbar() {
  var el = PCV.uxlib.el;
  var root = document.getElementById('shell-topbar');
  if (!root) return;
  PCV.uxlib.clearEl(root);
  root.appendChild(el('div', { class: 'shell-crumb', id: 'shell-crumb' }, 'Desktop Node'));
  root.appendChild(el('div', {
    class: 'shell-search', role: 'button', tabindex: '0',
    onClick: function () { if (typeof toggleGlobalSearch === 'function') toggleGlobalSearch(); }
  },
    shellIcon('ci-search', 'shell-search-ico'),
    el('span', { class: 'shell-search-ph' }, L('검색', 'Search')),
    el('kbd', { 'aria-hidden': 'true' }, 'Ctrl+K')));
  var right = el('div', { class: 'shell-topbar-r' });
  right.appendChild(el('span', {
    id: 'shell-sync', class: 'shell-sync', role: 'status',
    'aria-live': 'polite', 'aria-atomic': 'true'
  }));
  root.appendChild(right);
}
function seg(status, count, label, sub, target, filter) {
  return { key: target, status: status, count: count, label: label, sub: sub,
           filter: filter || null, _target: target };
}
// Desktop Node snapshot: { vms: { run, total }, jobs: { active, failed }, alerts: { crit, warn, unack },
// host: { ok, mode }, evidence: { issues } } built by the bootstrap from the Local API readbacks.
function buildSegments(s) {
  var dash = '—';
  var vms = s && s.vms, jobs = s && s.jobs, al = s && s.alerts, host = s && s.host, ev = s && s.evidence;
  return [
    vms ? seg(vms.run < vms.total ? 'warn' : 'ok', vms.run, L('VM 실행', 'VMs up'),
              '/ ' + vms.total + ' · ' + (vms.total - vms.run) + L(' 중지', ' down'), navTarget('vms'))
        : seg('idle', dash, L('VM 실행', 'VMs up'), '', navTarget('vms')),
    jobs ? seg(jobs.failed > 0 ? 'crit' : (jobs.active > 0 ? 'warn' : 'ok'), jobs.active, L('작업', 'Jobs'),
               jobs.failed + L(' 실패', ' failed'), 'jobs')
         : seg('idle', dash, L('작업', 'Jobs'), '', 'jobs'),
    al ? seg(al.crit > 0 ? 'crit' : (al.warn > 0 ? 'warn' : 'ok'), al.crit, 'Critical',
             '+' + al.warn + ' warn · ' + al.unack + ' unack', 'activity',
             al.crit > 0 ? { severity: ['crit'] } : null)
       : seg('idle', dash, 'Critical', '', 'activity'),
    host ? seg(host.ok ? 'ok' : 'crit', host.mode || (host.ok ? 'ok' : 'blocked'), L('호스트', 'Host'),
               host.detail || '', 'dashboard')
         : seg('idle', dash, L('호스트', 'Host'), '', 'dashboard'),
    ev ? seg(ev.issues > 0 ? 'warn' : 'ok', ev.issues, L('증적', 'Evidence'),
             ev.issues > 0 ? L('확인 필요', 'needs review') : L('이상 없음', 'clean'), 'evidence')
       : seg('idle', dash, L('증적', 'Evidence'), '', 'evidence')
  ];
}
function finiteCount(value, fallback) {
  var n = Number(value);
  return isFinite(n) ? n : fallback;
}
function operatorKpiSnapshot() {
  var loaded = typeof state !== 'undefined' && state && state.lastRefreshedAt != null;
  var refreshed = typeof hasRefreshedOperation === 'function' ? hasRefreshedOperation : function () { return false; };
  var snap = {};
  if (loaded && refreshed('vm.list') && typeof getSummaryVmCounts === 'function') {
    var vmCounts = getSummaryVmCounts();
    snap.vms = { run: finiteCount(vmCounts.running, 0), total: finiteCount(vmCounts.total, 0) };
  }
  if (loaded && typeof getSummaryJobCounts === 'function') {
    var jobCounts = getSummaryJobCounts();
    snap.jobs = { active: finiteCount(jobCounts.active, 0), failed: finiteCount(jobCounts.failed, 0) };
  }
  if (loaded && typeof getPriorityItems === 'function') {
    var items = getPriorityItems() || [];
    var crit = 0;
    var warn = 0;
    for (var index = 0; index < items.length; index += 1) {
      if (items[index] && items[index].tone === 'error') crit += 1;
      else warn += 1;
    }
    snap.alerts = { crit: crit, warn: warn, unack: items.length };
  }
  if (typeof getHostReadinessLabel === 'function') {
    var mode = String(getHostReadinessLabel() || '');
    if (mode && mode !== '—' && mode !== '-') {
      var blocked = typeof hostReadinessBlocked === 'function' ? hostReadinessBlocked() : false;
      snap.host = { ok: !blocked, mode: mode, detail: '' };
    }
  }
  if (loaded && refreshed('ops.summary') && state.opsSummary && typeof collectEvidenceIssues === 'function') {
    snap.evidence = { issues: collectEvidenceIssues().length };
  }
  return snap;
}
function syncFromOperatorState() {
  update(operatorKpiSnapshot());
}
function buildStatusbar() {
  var root = document.getElementById('shell-statusbar');
  if (!root) return;
  var active = document.activeElement;
  var focusKey = active && root.contains(active) ? active.dataset.segKey : null;
  PCV.uxlib.clearEl(root);
  root.appendChild(HN.statusBar(buildSegments(_snapshot), {
    onNavigate: function (s) { if (typeof navigateTo === 'function') navigateTo(s._target); }
  }));
  if (focusKey) {
    var replacement = root.querySelector('[data-seg-key="' + focusKey + '"]');
    if (replacement) replacement.focus();
  }
}
function mount() {
  if (!document.getElementById('shell-sidebar')) {
    console.error('PCV.shell.mount: shell containers missing');
    return;
  }
  buildSidebar();
  buildTopbar();
  if (_snapshot) update(_snapshot); else buildStatusbar();
  if (window.currentUser && typeof applyRoleVisibility === 'function') {
    applyRoleVisibility(window.currentUser.role);
  }
  setActive(window.currentTab || 'dashboard');
}
function update(snapshot) {
  _snapshot = snapshot || _snapshot;
  var s = _snapshot || {};
  var vmCnt = document.querySelector('#shell-sidebar [data-cnt="vms"]');
  if (vmCnt) vmCnt.textContent = s.vms ? s.vms.run + '/' + s.vms.total : '—';
  function dot(name, on, tone) {
    var d = document.querySelector('#shell-sidebar [data-dot="' + name + '"]');
    if (!d) return;
    if (on) { d.removeAttribute('hidden'); d.className = 'shell-dot shell-dot-' + tone; }
    else d.setAttribute('hidden', '');
  }
  dot('jobs', !!(s.jobs && (s.jobs.failed > 0 || s.jobs.active > 0)), s.jobs && s.jobs.failed > 0 ? 'crit' : 'warn');
  dot('alerts', !!(s.alerts && s.alerts.crit > 0), 'crit');
  dot('evidence', !!(s.evidence && s.evidence.issues > 0), 'warn');
  syncAccountNav();
  buildStatusbar();
}
function crumbFor(tabId) {
  var effective = VM_TABS.indexOf(tabId) !== -1 ? 'vms' : tabId;
  var secLabel = null, itLabel = null;
  NAV_SECTIONS().forEach(function (sec) {
    sec.items.forEach(function (it) {
      if (it.id === effective) { secLabel = sec.label; itLabel = itemLabel(it); }
    });
  });
  return { section: secLabel, item: itLabel || tabId, effective: effective };
}
function setActive(tabId) {
  var c = crumbFor(tabId);
  document.querySelectorAll('#shell-sidebar .shell-navitem').forEach(function (n) {
    var on = n.dataset.nav === c.effective;
    n.classList.toggle('active', on);
    if (on) n.setAttribute('aria-current', 'page'); else n.removeAttribute('aria-current');
  });
  var el = PCV.uxlib.el;
  var crumb = document.getElementById('shell-crumb');
  if (crumb) {
    PCV.uxlib.clearEl(crumb);
    crumb.appendChild(el('span', null, 'Desktop Node'));
    if (c.section) {
      crumb.appendChild(el('span', { class: 'shell-crumb-sep' }, '/'));
      crumb.appendChild(el('span', null, c.section));
    }
    crumb.appendChild(el('span', { class: 'shell-crumb-sep' }, '/'));
    crumb.appendChild(el('b', null, c.item));
  }
}
PCV.shell = { mount: mount, update: update, setActive: setActive, NAV_SECTIONS: NAV_SECTIONS, buildSegments: buildSegments, syncAccountNav: syncAccountNav, syncFromOperatorState: syncFromOperatorState };
})(window.PCV);

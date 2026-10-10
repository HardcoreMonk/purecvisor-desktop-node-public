// @ts-nocheck
// Ported from purecvisor ui/modules/help.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1 §3). Desktop Node changes: the reference catalog lists the Local API routes,
// PCVCLI commands and Web Console views of the Windows Hyper-V product instead of the Linux RPC namespaces, the
// Swagger page (renderSwaggerApi, swJump, filterSwagger, swTry) is dropped because the Desktop Node ships no OpenAPI
// explorer, the docs button opens docs.html at the web root, and the in-page handlers are attached as listeners
// instead of inline onclick strings. _L(ko, en) comes from i18n.js.
window.PCV = window.PCV || {};
(function (PCV) {
  function L(ko, en) { return (typeof _L === 'function') ? _L(ko, en) : ko; }
  function searchLabel() { return (typeof t === 'function') ? t('search') : L('검색', 'Search'); }

  // [route, minimum role (0 viewer, 1 operator, 2 admin, -1 none), pcvcli command, Web Console view, ko, en]
  function catalog() {
    return [
      { id: 'host', ko: '호스트 · 런타임', en: 'Host & Runtime', rows: [
        ['GET /runtime/policy', 0, 'runtime policy', 'dashboard / troubleshooting', 'runtime/auth/job/native operation policy 확인', 'Runtime, auth, job and native operation policy'],
        ['GET /host/status', 0, 'host status', 'dashboard', 'host readiness (Hyper-V, VMMS, 관리자 권한)', 'Host readiness (Hyper-V, VMMS, elevation)'],
        ['GET /ops/summary', 0, '-', 'dashboard', 'Ops Cockpit 요약 (VM/job count, priority warning)', 'Ops Cockpit summary (VM/job counts, priority warnings)'],
        ['GET /network/inventory', 0, 'network inventory', 'network', 'Hyper-V switch inventory (read-only)', 'Hyper-V switch inventory (read-only)'],
        ['GET /console/capabilities', 0, '-', 'troubleshooting', 'vmconnect handoff와 optional noVNC bridge 상태', 'vmconnect handoff and optional noVNC bridge state']
      ] },
      { id: 'auth', ko: '인증 · 계정', en: 'Auth & Accounts', rows: [
        ['POST /auth/loopback-session', -1, '-', 'login', 'loopback 접속의 짧은 JWT 발급 (계정이 구성되면 409로 닫힘)', 'Short JWT for loopback callers (409 once accounts exist)'],
        ['POST /auth/login', -1, '-', 'login', 'username/password로 access/refresh JWT 발급', 'Issue access/refresh JWT from username/password'],
        ['POST /auth/refresh', -1, '-', '-', 'refresh token으로 JWT 회전', 'Rotate JWT with the refresh token'],
        ['POST /auth/logout', -1, '-', 'topbar', 'browser session clear와 refresh/session revoke handoff', 'Clear the browser session and revoke the refresh token'],
        ['GET /auth/session', 0, '-', 'topbar', '현재 account session 확인', 'Current account session'],
        ['GET /auth/rbac', 0, '-', 'troubleshooting', 'role/permission matrix 확인', 'Role/permission matrix'],
        ['GET /accounts', 2, 'account list', 'topbar', '계정 목록', 'List accounts'],
        ['POST /accounts', 2, 'account create', 'topbar', '계정 생성 (password는 env/prompt로만)', 'Create an account (password via env/prompt only)'],
        ['POST /accounts/{username}/disable', 2, 'account disable', 'topbar', '계정 비활성화', 'Disable an account']
      ] },
      { id: 'vm', ko: '가상 머신', en: 'Virtual Machines', rows: [
        ['GET /vms', 0, 'vm list', 'vms', 'VM 목록', 'List VMs'],
        ['GET /vms/{id}', 0, 'vm get', 'vms', 'VM 상세', 'VM detail'],
        ['POST /vms', 1, 'vm create', 'vms', 'VM 생성 job queue (Generation 2)', 'Queue VM create (Generation 2)'],
        ['POST /vms/{id}/start', 1, 'vm start', 'vms', 'VM start job queue', 'Queue VM start'],
        ['POST /vms/{id}/shutdown', 1, 'vm shutdown', 'vms', 'guest shutdown job queue', 'Queue guest shutdown'],
        ['POST /vms/{id}/poweroff', 1, 'vm poweroff', 'vms', '강제 전원 종료 job queue', 'Queue power off'],
        ['POST /vms/{id}/restart', 1, 'vm restart', 'vms', '재시작 job queue', 'Queue restart'],
        ['POST /vms/{id}/save', 1, 'vm save', 'vms', 'Hyper-V Saved 상태 저장 job queue', 'Queue save to the Hyper-V Saved state'],
        ['POST /vms/{id}/resume-saved', 1, 'vm resume-saved', 'vms', 'Saved 상태 재개 job queue', 'Queue resume from Saved'],
        ['POST /vms/{id}/manage', 1, 'vm manage', 'vms', 'existing Hyper-V VM managed marker opt-in', 'Opt an existing Hyper-V VM into the managed marker'],
        ['POST /vms/{id}/clone/preview', 0, 'vm clone --dry-run', 'vms', 'clone 미리보기 (planned_copy_bytes)', 'Clone preview (planned_copy_bytes)'],
        ['POST /vms/{id}/clone', 1, 'vm clone', 'vms', 'managed VM clone job queue', 'Queue managed VM clone'],
        ['DELETE /vms/{id}', 1, 'vm delete', 'vms', 'managed VM delete job queue (running VM 차단)', 'Queue managed VM delete (running VMs refused)'],
        ['GET /vms/{id}/delete-status', 0, 'vm delete-status', 'vms', 'delete 진행 상태', 'Delete progress'],
        ['POST /vms/{id}/attach', 1, 'vm attach', 'vms', 'Virtual DVD에 ISO attach job queue', 'Queue ISO attach to the virtual DVD'],
        ['POST /vms/{id}/eject', 1, 'vm eject', 'vms', 'ISO eject job queue', 'Queue ISO eject'],
        ['POST /vms/{id}/set-memory', 1, 'vm set-memory', 'vms', 'startup memory 변경 job queue', 'Queue startup memory change'],
        ['POST /vms/{id}/set-vcpu', 1, 'vm set-vcpu', 'vms', 'vCPU 수 변경 job queue', 'Queue vCPU count change'],
        ['POST /vms/{id}/disk-resize', 1, 'vm disk-resize', 'vms', 'VHDX 확장 job queue', 'Queue VHDX resize'],
        ['GET /vms/{id}/memory-stats', 0, '-', 'vms', '메모리 통계', 'Memory statistics'],
        ['GET /vms/{id}/cpu-stats', 0, '-', 'vms', 'CPU 통계', 'CPU statistics'],
        ['GET /vms/{id}/blkio', 0, '-', 'vms', 'storage QoS (IOPS) 조회', 'Storage QoS (IOPS) state'],
        ['POST /vms/{id}/qos/storage', 1, 'vm blkio-set', 'vms', 'storage QoS 설정 job queue (preview 먼저)', 'Queue storage QoS change (preview first)'],
        ['GET /vms/{id}/bandwidth', 0, '-', 'vms', 'network bandwidth 조회', 'Network bandwidth state'],
        ['POST /vms/{id}/qos/network', 1, 'vm bandwidth-set', 'vms', 'network bandwidth 설정 job queue (preview 먼저)', 'Queue network bandwidth change (preview first)'],
        ['POST /vms/{id}/network', 1, 'vm network connect', 'vms', 'NIC을 Hyper-V switch에 연결하는 job queue', 'Queue NIC connect to a Hyper-V switch'],
        ['POST /vms/{id}/devices', 1, '-', 'vms', '장치 변경 job queue', 'Queue device change'],
        ['POST /vms/{id}/export', 1, 'vm export', 'vms', 'VM export job queue (preview 먼저)', 'Queue VM export (preview first)'],
        ['POST /vms/import', 1, 'vm import', 'vms', 'VM import job queue (preview 먼저)', 'Queue VM import (preview first)'],
        ['POST /vms/{id}/guest/channel', 1, 'vm guest-agent-ensure-channel', 'vms', 'guest channel 보장/복구', 'Ensure or repair the guest channel'],
        ['POST /vms/{id}/guest/exec', 1, 'vm guest-exec', 'vms', 'guest 내부 명령 실행 (preview 먼저)', 'Run a command inside the guest (preview first)']
      ] },
      { id: 'checkpoint', ko: '체크포인트', en: 'Checkpoints', rows: [
        ['GET /vms/{id}/checkpoints', 0, '-', 'vms', 'checkpoint 목록', 'List checkpoints'],
        ['POST /vms/{id}/checkpoints', 1, 'vm checkpoint create', 'vms', 'checkpoint 생성 job queue', 'Queue checkpoint create'],
        ['POST /vms/{id}/checkpoints/{checkpoint_id}/restore', 1, 'vm checkpoint restore', 'vms', 'checkpoint restore job queue', 'Queue checkpoint restore'],
        ['DELETE /vms/{id}/checkpoints/{checkpoint_id}', 1, 'vm checkpoint delete', 'vms', 'checkpoint 삭제 job queue', 'Queue checkpoint delete'],
        ['POST /vms/{id}/checkpoints/schedule/preview', 0, 'vm checkpoint schedule preview', 'vms', 'schedule 미리보기', 'Schedule preview'],
        ['POST /vms/{id}/checkpoints/schedule', 1, 'vm checkpoint schedule set', 'vms', 'schedule 저장', 'Save the schedule'],
        ['POST /vms/{id}/checkpoints/schedule/clear', 1, 'vm checkpoint schedule clear', 'vms', 'schedule 제거', 'Clear the schedule']
      ] },
      { id: 'console', ko: '콘솔', en: 'Console', rows: [
        ['GET /vms/{id}/console', 0, 'vm console', 'vms', 'VM별 console session/handoff metadata', 'Per-VM console session/handoff metadata'],
        ['GET /vms/{id}/console/frame/{size}', 0, '-', 'vms', 'Browser console 화면 프레임 (console.view)', 'Browser console frame (console.view)'],
        ['POST /vms/{id}/console/input', 1, '-', 'vms', 'Browser console 키 입력 (console.input)', 'Browser console key input (console.input)'],
        ['POST /console/novnc-target/preview', 0, '-', '-', 'noVNC target 미리보기', 'noVNC target preview'],
        ['POST /console/novnc-target', 1, 'console novnc-target set', '-', 'noVNC target 설정 job queue (Web Console은 폼을 열지 않음)', 'Queue noVNC target set (no Web Console form)'],
        ['POST /console/novnc-target/clear', 1, 'console novnc-target clear', '-', 'noVNC target 제거 job queue', 'Queue noVNC target clear']
      ] },
      { id: 'job', ko: '작업', en: 'Jobs', rows: [
        ['GET /jobs', 0, 'job list', 'activity', 'server-side job snapshot (limit/offset)', 'Server-side job snapshot (limit/offset)'],
        ['GET /jobs/{job_id}', 0, 'job get', 'jobs', 'job 상태 확인', 'Job state'],
        ['POST /jobs/{job_id}/cancel', 1, 'job cancel', 'jobs', 'job 취소 요청', 'Request job cancel'],
        ['POST /jobs/{job_id}/retry', 1, 'job retry', 'jobs', 'retryable failed job 재시도', 'Retry a retryable failed job']
      ] },
      { id: 'diagnostics', ko: '진단', en: 'Diagnostics', rows: [
        ['GET /diagnostics/bundles', 0, '-', 'troubleshooting', 'diagnostic bundle 목록 (pagination, retention)', 'List diagnostic bundles (pagination, retention)'],
        ['POST /diagnostics/bundles', 1, 'diagnostics bundle create', 'troubleshooting', 'redaction을 적용한 bundle 생성', 'Create a redacted bundle'],
        ['GET /diagnostics/bundles/{bundle_id}/download', 1, '-', 'troubleshooting', 'bundle download', 'Download a bundle']
      ] }
    ];
  }

  function rbacBadge(n) {
    if (n === 2) return HN.statusPill('idle', 'ADMIN');
    if (n === 1) return HN.statusPill('idle', 'OPERATOR');
    if (n === 0) return HN.statusPill('idle', 'VIEWER');
    return HN.statusPill('idle', L('없음', 'NONE'));
  }

  function renderHelp(b) {
    var el = PCV.uxlib.el, frag = PCV.uxlib.frag, clearEl = PCV.uxlib.clearEl;
    var NS = catalog();
    var routeCount = NS.reduce(function (n, ns) { return n + ns.rows.filter(function (r) { return r[0] !== '-'; }).length; }, 0);
    var apiBase = String(window.API_BASE || '/api/v1');
    var featChips = el('div', { class: 'help-feat-chips' },
      el('span', { class: 'help-feat-chip' }, '🪟 ' + L('Windows Hyper-V', 'Windows Hyper-V')),
      el('span', { class: 'help-feat-chip' }, '🔒 ' + L('loopback 기본', 'Loopback by default')),
      el('span', { class: 'help-feat-chip' }, '🧾 ' + L('queued job', 'Queued jobs')),
      el('span', { class: 'help-feat-chip' }, '📸 ' + L('체크포인트', 'Checkpoints')));
    var banner = el('div', { class: 'help-banner' },
      el('div', { class: 'help-banner-main' },
        el('div', { class: 'help-banner-title' }, 'PureCVisor Desktop Node — ' + L('Local API 레퍼런스', 'Local API Reference')),
        el('div', { class: 'help-banner-sub' }, routeCount + ' route · PCVCLI · ' + L('네임스페이스별 접이식 카탈로그', 'collapsible per-namespace catalog')),
        featChips),
      el('a', { href: 'docs.html', target: '_blank', rel: 'noopener', class: 'help-guide-btn' }, '📖 ' + L('문서 홈', 'Open Docs')));
    var stats = el('div', { class: 'sg grid-3' },
      HN.card('⚙ ' + L('시스템', 'System'), [
        HN.row(L('Local API route', 'Local API routes'), String(routeCount)),
        HN.row(L('API base', 'API base'), el('code', null, apiBase)),
        HN.row(L('CLI 명령 안내', 'CLI reference'), el('code', null, 'pcvcli --help'))]),
      HN.card('🧭 ' + L('범위', 'Scope'), [
        HN.row(L('에디션', 'Edition'), L('Desktop Node (Windows Hyper-V)', 'Desktop Node (Windows Hyper-V)')),
        HN.row(L('기본 노출', 'Default exposure'), 'loopback-only'),
        HN.row(L('host mutation', 'Host mutation'), L('관리자 opt-in gate', 'admin opt-in gate'))]),
      HN.card('📖 ' + L('문서', 'Documentation'), [
        HN.row(L('운영 가이드', 'Operator guide'), el('a', { href: 'docs.html', target: '_blank', rel: 'noopener' }, 'docs.html')),
        HN.row(L('키보드 단축키', 'Keyboard shortcuts'), el('kbd', null, '?')),
        HN.row(L('환경설정', 'Preferences'), el('kbd', null, 'Ctrl+P'))]));
    var jump = el('div', { class: 'help-jump' }, NS.map(function (ns) {
      var chip = el('button', { type: 'button', class: 'help-chip' }, ns.id + ' ' + ns.rows.length);
      chip.addEventListener('click', function () { helpJump(ns.id); });
      return chip;
    }));
    var searchInput = el('input', { 'aria-label': searchLabel(), id: 'help-search', class: 'sb-search', placeholder: searchLabel(),
      style: 'max-width:600px;font-size:15px;padding:10px 14px' });
    searchInput.addEventListener('input', filterHelp);
    var searchBar = el('div', { class: 'mb-16' }, searchInput);
    var sections = NS.map(function (ns) {
      var collapsed = ns.id !== 'host';
      var head = el('h4', { class: 'color-accent help-ns-h' },
        el('span', { class: 'help-ns-id' }, ns.id),
        el('span', { class: 'help-ns-name' }, L(ns.ko, ns.en)),
        el('span', { class: 'help-ns-count' }, String(ns.rows.length)));
      var tbody = el('tbody', null, ns.rows.map(function (r) {
        return el('tr', { 'data-search': (r[0] + ' ' + r[2] + ' ' + r[3] + ' ' + r[4] + ' ' + r[5]).toLowerCase() },
          el('td', { class: 'help-m' }, r[0]),
          el('td', null, rbacBadge(r[1])),
          el('td', { class: 'help-cli' }, r[2] === '-' ? '-' : 'pcvcli ' + r[2]),
          el('td', null, r[3]),
          el('td', { class: 'color-muted' }, L(r[4], r[5])));
      }));
      var table = el('table', { class: 'help-tbl' },
        el('thead', null, el('tr', null,
          el('th', null, 'Route'), el('th', null, 'RBAC'), el('th', null, 'CLI'),
          el('th', null, 'Web Console'), el('th', null, L('설명', 'Description')))),
        tbody);
      var section = el('div', { class: 'hc help-ns' + (collapsed ? ' ns-collapsed' : ''), id: 'help-ns-' + ns.id, 'data-default-collapsed': collapsed ? '1' : '0' },
        head, table);
      head.addEventListener('click', function () { section.classList.toggle('ns-collapsed'); });
      return section;
    });
    var content = el('div', { id: 'help-content' }, sections);
    clearEl(b);
    b.appendChild(frag(HN.pagehead({ title: L('도움말 & 참조', 'Help & Reference') }), banner, stats, jump, searchBar, content));
  }
  window.renderHelp = renderHelp;

  function helpJump(id) {
    var e = document.getElementById('help-ns-' + id);
    if (!e) return;
    e.classList.remove('ns-collapsed');
    if (typeof e.scrollIntoView === 'function') e.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
  window.helpJump = helpJump;

  function filterHelp() {
    var input = document.getElementById('help-search');
    var q = input ? input.value.trim().toLowerCase() : '';
    document.querySelectorAll('#help-content .help-ns').forEach(function (sec) {
      if (!q) {
        sec.style.display = '';
        if (sec.dataset.defaultCollapsed === '1') sec.classList.add('ns-collapsed');
        else sec.classList.remove('ns-collapsed');
        sec.querySelectorAll('tr[data-search]').forEach(function (r) { r.style.display = ''; });
        return;
      }
      var any = false;
      sec.querySelectorAll('tr[data-search]').forEach(function (r) {
        var hit = r.dataset.search.indexOf(q) !== -1;
        r.style.display = hit ? '' : 'none';
        if (hit) any = true;
      });
      if (any) { sec.style.display = ''; sec.classList.remove('ns-collapsed'); }
      else { sec.style.display = 'none'; }
    });
  }
  window.filterHelp = filterHelp;

  var kbdHelpOpen = false;
  window.kbdHelpOpen = kbdHelpOpen;
  var kbdDialog = null;
  function toggleKbdHelp() {
    if (kbdHelpOpen) { closeKbdHelp(); return; }
    kbdHelpOpen = true; window.kbdHelpOpen = kbdHelpOpen;
    var shortcuts = [
      ['Ctrl+K', L('커맨드 팔레트', 'Command Palette')],
      ['Ctrl+N', L('새 VM', 'New VM'), 'operator'],
      ['Ctrl+P', L('환경설정', 'Preferences')],
      ['F11', L('전체 화면', 'Fullscreen')],
      ['Escape', L('대화상자 닫기', 'Close Dialog')],
      ['?', L('이 도움말', 'This Help')],
      ['Ctrl+Shift+F', L('통합 검색 (Ctrl+K와 동일)', 'Unified Search (same as Ctrl+K)')],
      ['Ctrl+B', L('사이드바 전환', 'Toggle Sidebar')]
    ];
    var el = PCV.uxlib.el;
    var box = el('div', { class: 'kbd-box' },
      el('h2', { class: 'kbd-title' }, L('키보드 단축키', 'Keyboard Shortcuts')),
      el('div', { class: 'kbd-grid' },
        shortcuts.filter(function (s) {
          return !s[2] || (typeof pcvRoleAllows === 'function' ? pcvRoleAllows(s[2]) : true);
        }).map(function (s) {
          return el('div', { class: 'kbd-row' },
            el('span', { class: 'kbd-key' }, s[0]),
            el('span', { class: 'kbd-desc' }, s[1]));
        })),
      el('div', { class: 'kbd-close' }, L('? 또는 Esc 키로 닫기', 'Press ? or Esc to close')));
    if (!PCV.modalCore || typeof PCV.modalCore.openBare !== 'function') {
      kbdHelpOpen = false; window.kbdHelpOpen = false;
      if (PCV.modal && typeof PCV.modal.show === 'function') PCV.modal.show({ title: L('키보드 단축키', 'Keyboard Shortcuts'), body: box, actions: [{ label: L('닫기', 'Close'), primary: true }] });
      return;
    }
    var mine = PCV.modalCore.openBare(box, {
      dialogClass: 'kbd-help',
      onClose: function () { if (kbdDialog !== mine) return; kbdHelpOpen = false; window.kbdHelpOpen = false; kbdDialog = null; }
    });
    kbdDialog = mine;
  }
  window.toggleKbdHelp = toggleKbdHelp;
  function closeKbdHelp() {
    if (kbdDialog) { var d = kbdDialog; kbdDialog = null; try { d.close(); } catch (e) { /* already closed */ } }
    kbdHelpOpen = false; window.kbdHelpOpen = false;
  }
  window.closeKbdHelp = closeKbdHelp;

  PCV.help = {
    render: renderHelp,
    renderHelp: renderHelp,
    helpJump: helpJump,
    filterHelp: filterHelp,
    toggleKbdHelp: toggleKbdHelp,
    closeKbdHelp: closeKbdHelp,
    catalog: catalog
  };
})(window.PCV);

// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §3): the preferences dialog the Single Edge
// topbar and command palette open through showPrefs() (Single Edge keeps it in ui/app.js, which the Desktop Node does
// not port). It carries the settings the legacy header form used to show: theme, language, Local API base and the
// optional browser bearer token, plus the Single Edge settings export/import.
window.PCV = window.PCV || {};
(function (PCV) {
  function L(ko, en) { return (typeof _L === 'function') ? _L(ko, en) : ko; }
  function legacyState() { return window.state && typeof window.state === 'object' ? window.state : null; }
  function currentTheme() { return document.documentElement.getAttribute('data-theme') || 'supanova'; }
  function applyTheme(value) {
    if (PCV.theme && typeof PCV.theme.change === 'function') PCV.theme.change(value);
    var st = legacyState();
    if (st) st.theme = value;
  }
  function applyLanguage(value) {
    if (typeof I18N !== 'undefined' && I18N.setLang(value)) {
      if (typeof applyI18n === 'function') applyI18n();
      if (PCV.shell && typeof PCV.shell.mount === 'function') { try { PCV.shell.mount(); } catch (e) { /* shell not mounted yet */ } }
    }
    var st = legacyState();
    if (st) st.language = value;
    if (typeof render === 'function') { try { render(); } catch (e) { /* legacy panels may be absent */ } }
  }
  function applyApiBase(value) {
    var base = String(value || '').trim().replace(/\/$/, '');
    if (!base) return false;
    window.API_BASE = base + '/api/v1';
    var st = legacyState();
    if (st) st.apiBaseUrl = base;
    return true;
  }
  function applyBrowserToken(value) {
    var token = String(value || '').trim();
    var st = legacyState();
    if (st) st.apiToken = token;
    if (token && !window.authToken) window.authToken = token;
  }
  function showPrefs() {
    var el = PCV.uxlib.el;
    var st = legacyState() || {};
    var themes = (PCV.theme && PCV.theme.PREVIEWS) || [];
    var themeSelect = el('select', { id: 'pref-theme', class: 'login-input', 'aria-label': L('테마', 'Theme') },
      themes.map(function (t) { return el('option', { value: t.id, selected: t.id === currentTheme() ? '' : null }, t.name); }));
    var langSelect = el('select', { id: 'pref-lang', class: 'login-input', 'aria-label': L('언어', 'Language') },
      el('option', { value: 'ko', selected: (typeof I18N !== 'undefined' && I18N.getLang() === 'ko') ? '' : null }, '한국어'),
      el('option', { value: 'en', selected: (typeof I18N !== 'undefined' && I18N.getLang() === 'en') ? '' : null }, 'English'));
    var apiInput = el('input', { id: 'pref-api-base', class: 'login-input', type: 'url', autocomplete: 'off',
      value: st.apiBaseUrl || (window.API_BASE || '').replace(/\/api\/v1$/, ''), 'aria-label': L('Local API 주소', 'Local API base') });
    var tokenInput = el('input', { id: 'pref-api-token', class: 'login-input', type: 'password', autocomplete: 'off',
      placeholder: L('선택: 서비스 bearer token', 'optional service bearer token'), 'aria-label': L('브라우저 token', 'Browser token') });
    var body = [
      el('div', { class: 'theme-editor-item' }, el('label', { for: 'pref-theme' }, L('테마', 'Theme')), themeSelect),
      el('div', { class: 'theme-editor-item' }, el('label', { for: 'pref-lang' }, L('언어', 'Language')), langSelect),
      el('div', { class: 'theme-editor-item' }, el('label', { for: 'pref-api-base' }, L('Local API 주소', 'Local API base')), apiInput),
      el('div', { class: 'theme-editor-item' }, el('label', { for: 'pref-api-token' }, L('브라우저 token', 'Browser token')), tokenInput),
      el('p', { class: 'text-xs color-muted', style: 'margin-top:8px' },
        L('token 값은 화면과 기록에 남지 않습니다. loopback 접속은 token 없이 세션이 열립니다.', 'Token values are never rendered or logged. Loopback sessions need no token.')),
      el('div', { style: 'display:flex;gap:8px;margin-top:12px;flex-wrap:wrap' },
        el('button', { class: 'btn', type: 'button', onClick: function () { if (typeof openThemeEditor === 'function') openThemeEditor(); } }, L('테마 편집기', 'Theme editor')),
        el('button', { class: 'btn', type: 'button', onClick: function () { if (typeof exportUiSettings === 'function') exportUiSettings(); } }, L('설정 내보내기', 'Export settings')),
        el('button', { class: 'btn', type: 'button', onClick: function () { if (typeof importUiSettings === 'function') importUiSettings(); } }, L('설정 가져오기', 'Import settings')),
        el('button', { class: 'btn btn-r', type: 'button', onClick: function () { if (typeof clearBrowserToken === 'function') clearBrowserToken(); if (typeof toast === 'function') toast(L('브라우저 token을 지웠습니다', 'Browser token cleared')); } }, L('token 지우기', 'Clear token')))
    ];
    PCV.modal.show({
      title: L('환경설정', 'Preferences'),
      body: body,
      width: 420,
      actions: [
        { label: L('취소', 'Cancel') },
        { label: L('적용', 'Apply'), primary: true, onClick: function () {
          applyTheme(themeSelect.value);
          applyLanguage(langSelect.value);
          var baseChanged = applyApiBase(apiInput.value);
          if (tokenInput.value) applyBrowserToken(tokenInput.value);
          if (baseChanged && typeof loadAll === 'function') loadAll();
          if (typeof toast === 'function') toast(L('환경설정 적용', 'Preferences applied'));
          return true;
        } }
      ]
    });
  }
  function showCreate() {
    var dialog = document.getElementById('create-vm-dialog');
    if (dialog && typeof dialog.showModal === 'function') { if (typeof navigateTo === 'function') navigateTo('vms'); dialog.showModal(); return; }
    if (typeof toast === 'function') toast(L('VM 생성 폼이 없습니다', 'VM create form is missing'), false);
  }
  PCV.prefs = { show: showPrefs, applyTheme: applyTheme, applyLanguage: applyLanguage, applyApiBase: applyApiBase, applyBrowserToken: applyBrowserToken, showCreate: showCreate };
  window.showPrefs = showPrefs;
  window.showCreate = showCreate;
})(window.PCV);

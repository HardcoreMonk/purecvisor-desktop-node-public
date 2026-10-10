// @ts-nocheck
// Ported from purecvisor ui/modules/api.js (Apache-2.0, same author) for the Desktop Node Web Console (ADR-0018,
// pcv-single-edge-frontend-structure-v1). Desktop Node changes: login uses /auth/login and the {ok, data} envelope,
// loopback hosts open a session through /auth/loopback-session, a rejected bearer token (401, or 403 with a PCV_AUTH_*
// code) is recovered once through the refresh token or a new loopback session before the request is retried (the stale
// token problem seen in the 2026-10-10 S3 demo), the WebSocket client and TOTP flows are gone (events are polled by the
// events module) and the session watch probes /auth/session for opaque tokens.
window.PCV = window.PCV || {};
(function(PCV) {
function _setAuthSurfaceActive(node, active) {
  if (!node) return;
  if (!active && document.activeElement && node.contains(document.activeElement) &&
      typeof document.activeElement.blur === 'function') {
    document.activeElement.blur();
  }
  node.inert = !active;
  if (active) node.removeAttribute('aria-hidden');
  else node.setAttribute('aria-hidden', 'true');
}
function pcvSetLoginVisible(visible) {
  var loginPage = document.getElementById('login-page');
  var app = document.getElementById('app');
  if (loginPage) loginPage.style.display = visible ? 'flex' : 'none';
  _setAuthSurfaceActive(loginPage, !!visible);
  _setAuthSurfaceActive(app, !visible);
  if (document.body) document.body.classList.toggle('login-active', !!visible);
  if (visible && typeof closeMobileSB === 'function') closeMobileSB();
}
window.pcvSetLoginVisible = pcvSetLoginVisible;
function isLoopbackHostname(hostname) {
  var value = String(hostname || '').replace(/^\[|\]$/g, '').toLowerCase();
  return value === '127.0.0.1' || value === 'localhost' || value === '::1';
}
function _storeSession(d, user) {
  window.authToken = d.access_token;
  sessionStorage.setItem('pcv_token', d.access_token);
  if (d.refresh_token) sessionStorage.setItem('pcv_refresh_token', d.refresh_token);
  else sessionStorage.removeItem('pcv_refresh_token');
  var session = d.session || {};
  var name = user || session.username || session.subject || 'user';
  sessionStorage.setItem('pcv_user', name);
  window.currentUser = {
    name: name,
    role: String(session.role || 'viewer'),
    permissions: Array.isArray(session.permissions) ? session.permissions.slice() : [],
    subject: session.subject || name,
    display_name: session.display_name || name
  };
  sessionStorage.setItem('pcv_session', JSON.stringify(window.currentUser));
  _syncLegacyState(d);
  return name;
}
// The legacy console parts (core/desktop-api modules) keep their own auth state; mirror the Single Edge session into it.
function _syncLegacyState(d) {
  var st = window.state;
  if (!st || typeof st !== 'object') return;
  st.authAccessToken = String((d && d.access_token) || '');
  st.authRefreshToken = String((d && d.refresh_token) || '');
  st.authSession = (d && d.session) || null;
  st.authError = null;
  if (typeof saveAccountSessionToStorage === 'function') { try { saveAccountSessionToStorage(); } catch (e) { /* storage unavailable */ } }
}
var _refreshInProgress = null;
async function _tryRefreshToken() {
  var refreshToken = sessionStorage.getItem('pcv_refresh_token');
  if (!refreshToken) return false;
  if (_refreshInProgress) return _refreshInProgress;
  _refreshInProgress = (async function() {
    try {
      var r = await _fetchWithTimeout(EP.AUTH_REFRESH(), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refresh_token: refreshToken })
      });
      if (!r.ok) return false;
      var d = unwrapData(await r.json());
      if (d && d.access_token) { _storeSession(d, sessionStorage.getItem('pcv_user')); return true; }
      return false;
    } catch (e) { return false; }
    finally { _refreshInProgress = null; }
  })();
  return _refreshInProgress;
}
var _loopbackInProgress = null;
async function _createLoopbackSession() {
  if (!isLoopbackHostname(window.location.hostname)) return false;
  if (_loopbackInProgress) return _loopbackInProgress;
  _loopbackInProgress = (async function() {
    try {
      var r = await _fetchWithTimeout(EP.AUTH_LOOPBACK_SESSION(), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: '{}'
      });
      if (!r.ok) return false;
      var d = unwrapData(await r.json());
      if (d && d.access_token) { _storeSession(d, 'loopback-session'); return true; }
      return false;
    } catch (e) { return false; }
    finally { _loopbackInProgress = null; }
  })();
  return _loopbackInProgress;
}
// Recover a rejected bearer token once: refresh token first, then a fresh loopback session on loopback hosts.
async function _recoverSession() {
  window.authToken = '';
  if (await _tryRefreshToken()) return true;
  sessionStorage.removeItem('pcv_token');
  sessionStorage.removeItem('pcv_refresh_token');
  return _createLoopbackSession();
}
function _redirectToLogin() {
  window.authToken = '';
  window.currentUser = null;
  sessionStorage.removeItem('pcv_token');
  sessionStorage.removeItem('pcv_refresh_token');
  sessionStorage.removeItem('pcv_user');
  sessionStorage.removeItem('pcv_session');
  _syncLegacyState(null);
  if (PCV.events && typeof PCV.events.stop === 'function') PCV.events.stop();
  pcvSetLoginVisible(true);
  var la = document.getElementById('la');
  if (la) la.classList.remove('hidden');
  var us = document.getElementById('us');
  if (us) { us.classList.add('hidden'); us.style.display = 'none'; }
}
var PCV_FETCH_TIMEOUT_MS = 12000;
function _fetchWithTimeout(url, opts) {
  var controller = new AbortController();
  var tid = setTimeout(function() { controller.abort(); }, PCV_FETCH_TIMEOUT_MS);
  var merged = Object.assign({}, opts, { signal: controller.signal });
  return fetch(url, merged).finally(function() { clearTimeout(tid); });
}
function _setAuthHeader(opts) {
  if (!opts.headers) opts.headers = {};
  if (typeof opts.headers.set === 'function') opts.headers.set('Authorization', 'Bearer ' + window.authToken);
  else opts.headers['Authorization'] = 'Bearer ' + window.authToken;
}
async function _isAuthRejection(r) {
  if (r.status === 401) return true;
  if (r.status !== 403) return false;
  try {
    var body = await r.clone().json();
    return /^PCV_AUTH_/.test(String(body && body.error && body.error.code || ''));
  } catch (e) { return false; }
}
async function _fetchWithRefresh(url, opts) {
  var r = await _fetchWithTimeout(url, opts);
  if (!opts._pcvRetried && window.authToken && await _isAuthRejection(r)) {
    if (await _recoverSession()) {
      opts._pcvRetried = true;
      _setAuthHeader(opts);
      return _fetchWithTimeout(url, opts);
    }
    _redirectToLogin();
    throw new Error('Session expired');
  }
  return r;
}
function _json(r) {
  if (!r.ok) return r.json().catch(() => ({ ok: false, error: { code: r.status, message: 'HTTP ' + r.status } }));
  return r.json();
}
function fetchGet(u) {
  return _fetchWithRefresh(u, { headers: { Authorization: 'Bearer ' + window.authToken } }).then(_json);
}
function fetchPost(u, b) {
  return _fetchWithRefresh(u, {
    method: 'POST',
    headers: { Authorization: 'Bearer ' + window.authToken, 'Content-Type': 'application/json' },
    body: JSON.stringify(b === undefined ? {} : b)
  }).then(_json);
}
function fetchDelete(u, body) {
  var opts = {
    method: 'DELETE',
    headers: { Authorization: 'Bearer ' + window.authToken }
  };
  if (body) { opts.headers['Content-Type'] = 'application/json'; opts.body = JSON.stringify(body); }
  return _fetchWithRefresh(u, opts).then(_json);
}
function fetchPut(u, b) {
  return _fetchWithRefresh(u, {
    method: 'PUT',
    headers: { Authorization: 'Bearer ' + window.authToken, 'Content-Type': 'application/json' },
    body: JSON.stringify(b)
  }).then(_json);
}
function _loginI18n(key, fallback) {
  var value = (typeof t === 'function') ? t(key) : key;
  return value && value !== key ? value : fallback;
}
async function _readLoginResponse(r) {
  var body = await r.text();
  if (!body) return {};
  try {
    return JSON.parse(body);
  } catch (e) {
    return {
      error: {
        code: r.status || 0,
        message: _loginI18n('login.bad_response', 'Login server returned an invalid response.')
      }
    };
  }
}
function _loginHttpDetail(status) {
  if (status === 401 || status === 403) {
    return _loginI18n('login.invalid_credentials', 'Invalid username or password.');
  }
  if (status === 429) {
    return _loginI18n('login.too_many', 'Too many login attempts. Try again later.');
  }
  if (status === 502 || status === 503 || status === 504) {
    return _loginI18n('login.service_unavailable', 'Login service is temporarily unavailable.');
  }
  if (status >= 500) {
    return _loginI18n('login.server_error', 'Login server could not process the request.');
  }
  return _loginI18n('login.bad_response', 'Login server returned an invalid response.');
}
function _loginFailureText(status) {
  var titleKey = (status === 401 || status === 403) ? 'login.failed' : 'login.error';
  var titleFallback = (status === 401 || status === 403) ? 'Login failed' : 'Connection error';
  var prefix = _loginI18n(titleKey, titleFallback);
  return prefix + ': ' + _loginHttpDetail(status || 0);
}
function _loginNetworkFailureText(e) {
  var prefix = _loginI18n('login.error', 'Connection error');
  var detail = e && e.name === 'AbortError'
    ? _loginI18n('login.timeout', 'Login request timed out.')
    : _loginI18n('login.network', 'Could not connect to the login server.');
  return prefix + ': ' + detail;
}
function _showSignedIn(name) {
  pcvSetLoginVisible(false);
  var la = document.getElementById('la');
  if (la) la.classList.add('hidden');
  var us = document.getElementById('us');
  if (us) { us.classList.remove('hidden'); us.style.display = 'flex'; }
  var usName = document.getElementById('us-name');
  if (usName) usName.textContent = name;
  if (typeof window.__pcvDismissSplash === 'function') window.__pcvDismissSplash();
}
function _finishLogin(user, d, opts) {
  opts = opts || {};
  var name = _storeSession(d, user);
  _showSignedIn(name);
  if (!opts.silent) {
    if (typeof window.toast === 'function') window.toast(typeof t === 'function' ? t('logged.in') + ': ' + name : 'Logged in: ' + name);
    if (typeof window.addEvt === 'function') window.addEvt('AUTH Login successful — user: ' + name + ', session issued');
  }
  if (typeof window.loadAll === 'function') window.loadAll();
  if (PCV.events && typeof PCV.events.start === 'function') PCV.events.start();
  startSessionWatch();
  if (typeof pcvPostLoginInit === 'function') pcvPostLoginInit();
}
var _loginPageInFlight = false;
function _setLoginPageBusy(busy) {
  var button = document.querySelector('#login-cred-actions .login-submit[type="submit"]');
  if (!button) return;
  button.disabled = !!busy;
  button.setAttribute('aria-busy', busy ? 'true' : 'false');
  var label = button.querySelector('.login-submit-label');
  if (label) {
    label.textContent = busy
      ? _loginI18n('login.loading', 'Signing in…')
      : _loginI18n('login', 'Login');
  }
}
async function doLoginPage() {
  const user = document.getElementById('login-user')?.value.trim();
  const pass = document.getElementById('login-pass')?.value;
  const errEl = document.getElementById('login-err');
  if (errEl) errEl.textContent = '';
  if (!user || !pass) { if (errEl) errEl.textContent = typeof t === 'function' ? t('login.required') : 'Required'; return; }
  if (_loginPageInFlight) return;
  _loginPageInFlight = true;
  _setLoginPageBusy(true);
  try {
    const r = await _fetchWithTimeout(EP.AUTH_LOGIN(), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: user, password: pass })
    });
    const d = unwrapData(await _readLoginResponse(r));
    if (r.ok && d && d.access_token) {
      _finishLogin(user, d);
    } else {
      if (errEl) errEl.textContent = _loginFailureText(r.status);
    }
  } catch (e) {
    if (errEl) errEl.textContent = _loginNetworkFailureText(e);
  } finally {
    _loginPageInFlight = false;
    _setLoginPageBusy(false);
  }
}
async function doLogin() { return doLoginPage(); }
async function ensureLoopbackSession() {
  if (window.authToken) return true;
  if (!(await _createLoopbackSession())) return false;
  _finishLogin('loopback-session', {
    access_token: window.authToken,
    refresh_token: sessionStorage.getItem('pcv_refresh_token') || '',
    session: window.currentUser
  }, { silent: true });
  return true;
}
async function doLogout() {
  var refreshToken = sessionStorage.getItem('pcv_refresh_token');
  try {
    if (refreshToken) {
      await _fetchWithTimeout(EP.AUTH_LOGOUT(), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refresh_token: refreshToken })
      });
    }
  } catch (e) { /* the browser session is cleared regardless */ }
  window.vmList = [];
  _redirectToLogin();
  var wsStatus = document.getElementById('ws-s');
  if (wsStatus && PCV.uxlib) PCV.uxlib.clearEl(wsStatus);
  if (typeof window.toast === 'function') window.toast(typeof t === 'function' ? t('logged.out') : 'Logged out');
  if (typeof window.addEvt === 'function') window.addEvt(typeof t === 'function' ? t('logged.out') : 'Logged out');
}
var _sessionCheckInterval = null;
var _sessionProbeTick = 0;
async function _probeSession() {
  if (!window.authToken) return;
  try {
    var r = await _fetchWithTimeout(EP.AUTH_SESSION(), { headers: { Authorization: 'Bearer ' + window.authToken } });
    if (await _isAuthRejection(r)) {
      if (await _recoverSession()) return;
      _redirectToLogin();
      toast(typeof _L === 'function' ? _L('세션 만료 — 다시 로그인하세요', 'Session expired — please login again') : 'Session expired', false);
    }
  } catch (e) { /* transient network failure; the next probe retries */ }
}
function startSessionWatch() {
  if (_sessionCheckInterval) clearInterval(_sessionCheckInterval);
  _sessionCheckInterval = setInterval(function() {
    var token = window.authToken;
    if (!token) return;
    _sessionProbeTick = (_sessionProbeTick + 1) % 2;
    if (_sessionProbeTick === 0) _probeSession();
    try {
      var payload = JSON.parse(atob(token.split('.')[1]));
      var exp = payload.exp * 1000;
      var remaining = exp - Date.now();
      if (remaining < 0) {
        clearInterval(_sessionCheckInterval);
        _redirectToLogin();
        toast(typeof _L === 'function' ? _L('세션 만료 — 다시 로그인하세요', 'Session expired — please login again') : 'Session expired', false);
      } else if (remaining < 300000) {
        var mins = Math.ceil(remaining / 60000);
        var el = document.getElementById('session-warn');
        if (!el) {
          el = document.createElement('div');
          el.id = 'session-warn';
          el.style.cssText = 'position:fixed;bottom:40px;left:50%;transform:translateX(-50%);background:var(--bg2);border:1px solid var(--yellow);border-radius:6px;padding:8px 14px;font-size:12px;z-index:9000';
          document.body.appendChild(el);
        }
        var lblExpires = typeof _L === 'function' ? _L('세션 만료까지', 'Session expires in') : 'Expires in';
        var lblMin = typeof _L === 'function' ? _L('분', 'min') : 'min';
        PCV.uxlib.clearEl(el);
        el.appendChild(PCV.uxlib.frag(
          PCV.uxlib.el('span', { class: 'color-yellow' }, '⚠'),
          ' ' + lblExpires + ' ' + mins + lblMin
        ));
        if (remaining < 120000) {
          _tryRefreshToken().then(function(ok) {
            if (ok) {
              var warn = document.getElementById('session-warn');
              if (warn) warn.remove();
              toast(typeof _L === 'function' ? _L('세션 자동 갱신됨', 'Session auto-renewed') : 'Session renewed');
            }
          }).catch(function(){});
        }
      } else {
        var warn = document.getElementById('session-warn');
        if (warn) warn.remove();
      }
    } catch (e) { /* opaque (non-JWT) token: the periodic /auth/session probe covers expiry */ }
  }, 30000);
}
function restoreSession() {
  window.authToken = sessionStorage.getItem('pcv_token') || '';
  try { window.currentUser = JSON.parse(sessionStorage.getItem('pcv_session') || 'null'); } catch (e) { window.currentUser = null; }
  if (!window.authToken) {
    ensureLoopbackSession().then(function (opened) {
      if (opened) return;
      pcvSetLoginVisible(true);
      var la = document.getElementById('la');
      if (la) la.classList.remove('hidden');
      if (typeof window.__pcvDismissSplash === 'function') window.__pcvDismissSplash();
    });
    return;
  }
  const savedUser = sessionStorage.getItem('pcv_user') || 'user';
  fetchGet(EP.AUTH_SESSION()).then(function (r) {
    if (r && r.error) throw new Error(r.error.message || 'session rejected');
    var session = unwrapData(r);
    _finishLogin(savedUser, {
      access_token: window.authToken,
      refresh_token: sessionStorage.getItem('pcv_refresh_token') || '',
      session: session && session.username ? session : window.currentUser
    }, { silent: true });
  }).catch(function () {
    sessionStorage.removeItem('pcv_token');
    sessionStorage.removeItem('pcv_user');
    sessionStorage.removeItem('pcv_session');
    window.authToken = '';
    restoreSession();
  });
}
var _apiActivityLog = [];
var _perfMetrics = {
  pageLoadTime: 0,
  firstContentfulPaint: 0,
  apiCallCount: 0,
  apiTotalTime: 0,
  avgApiTime: 0
};
window.addEventListener('load', function() {
  setTimeout(function() {
    if (window.performance && window.performance.timing) {
      var t = window.performance.timing;
      _perfMetrics.pageLoadTime = t.loadEventEnd - t.navigationStart;
    }
    if (window.performance && window.performance.getEntriesByType) {
      var paints = window.performance.getEntriesByType('paint');
      paints.forEach(function(p) {
        if (p.name === 'first-contentful-paint') _perfMetrics.firstContentfulPaint = Math.round(p.startTime);
      });
    }
  }, 100);
});
function unwrapData(r) {
  if (r == null) return r;
  if (r.data !== undefined) return r.data;
  if (r.result !== undefined) return r.result;
  return r;
}
function unwrapList(r) {
  if (Array.isArray(r)) return r;
  var d = unwrapData(r);
  if (Array.isArray(d)) return d;
  if (d && typeof d === 'object') {
    for (var key of ['vms', 'items', 'bundles', 'checkpoints', 'jobs', 'accounts', 'switches', 'adapters']) {
      if (Array.isArray(d[key])) return d[key];
    }
  }
  return [];
}
async function waitForJob(jobId, options) {
  var attempts = options && options.attempts || 5;
  var interval = options && options.interval !== undefined ? options.interval : 2000;
  var job = { job_id: jobId, status: 'queued' };
  for (var i = 0; i < attempts; i++) {
    if (i) await new Promise(resolve => setTimeout(resolve, interval));
    var response = await fetchGet(EP.JOB(jobId));
    if (response && response.error) throw new Error(response.error.message || 'Unable to read job result');
    job = unwrapData(response) || job;
    var status = String(job.status || '').toLowerCase();
    if (status === 'failed' || status === 'canceled' || status === 'cancelled')
      throw new Error(job.error_message || job.detail || job.error || ('Job ' + status));
    if (status === 'succeeded' || status === 'completed') return job;
  }
  return job;
}
var _pollingTimers = {};
function startAdaptivePolling(id, fn, intervalMs) {
  stopAdaptivePolling(id);
  var run = function() {
    if (document.hidden) return;
    fn();
  };
  run();
  _pollingTimers[id] = setInterval(run, intervalMs);
  var _vc = function() {
    if (!_pollingTimers[id]) { document.removeEventListener('visibilitychange', _vc); return; }
    if (!document.hidden) run();
  };
  if (!window._pollingListeners) window._pollingListeners = {};
  window._pollingListeners[id] = _vc;
  document.addEventListener('visibilitychange', _vc);
}
function stopAdaptivePolling(id) {
  if (_pollingTimers[id]) { clearInterval(_pollingTimers[id]); delete _pollingTimers[id]; }
  if (window._pollingListeners && window._pollingListeners[id]) {
    document.removeEventListener('visibilitychange', window._pollingListeners[id]);
    delete window._pollingListeners[id];
  }
}
var _origFetch = window.fetch;
window.fetch = function() {
  var url = arguments[0] || '';
  if (typeof url === 'string' && url.includes('/api/')) {
    _perfMetrics.apiCallCount++;
    var start = performance.now();
    return _origFetch.apply(this, arguments).then(function(r) {
      _perfMetrics.apiTotalTime += (performance.now() - start);
      _perfMetrics.avgApiTime = Math.round(_perfMetrics.apiTotalTime / _perfMetrics.apiCallCount);
      return r;
    });
  }
  return _origFetch.apply(this, arguments);
};
PCV.api = {
  fetchGet: fetchGet,
  fetchPost: fetchPost,
  fetchDelete: fetchDelete,
  fetchPut: fetchPut,
  _fetchWithTimeout: _fetchWithTimeout,
  unwrapData: unwrapData,
  unwrapList: unwrapList,
  waitForJob: waitForJob,
  doLoginPage: doLoginPage,
  doLogin: doLogin,
  doLogout: doLogout,
  ensureLoopbackSession: ensureLoopbackSession,
  isLoopbackHostname: isLoopbackHostname,
  restoreSession: restoreSession,
  startSessionWatch: startSessionWatch,
  startAdaptivePolling: startAdaptivePolling,
  stopAdaptivePolling: stopAdaptivePolling,
  currentUser: function () { return window.currentUser || null; },
  _tryRefreshToken: _tryRefreshToken,
  _recoverSession: _recoverSession,
  _redirectToLogin: _redirectToLogin,
  _apiActivityLog: _apiActivityLog,
  _perfMetrics: _perfMetrics
};
window.unwrapData = unwrapData;
window.unwrapList = unwrapList;
window.fetchGet = fetchGet;
window.fetchPost = fetchPost;
window.fetchDelete = fetchDelete;
window.fetchPut = fetchPut;
window._tryRefreshToken = _tryRefreshToken;
window._redirectToLogin = _redirectToLogin;
window.doLoginPage = doLoginPage;
window.doLogin = doLogin;
window.doLogout = doLogout;
window.ensureLoopbackSession = ensureLoopbackSession;
window.startSessionWatch = startSessionWatch;
window.restoreSession = restoreSession;
window.startAdaptivePolling = startAdaptivePolling;
window.stopAdaptivePolling = stopAdaptivePolling;
window._apiActivityLog = _apiActivityLog;
window._perfMetrics = _perfMetrics;
})(window.PCV);

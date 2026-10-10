// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): polling adapter in the place of the Single
// Edge WebSocket event stream (ui/modules/api.js connectWS). It carries the legacy web/src/served/job-polling.ts
// behaviour: tracked jobs persisted in the browser session, adaptive polling of /jobs/{id} while jobs are queued or
// running, a slow heartbeat otherwise, and subscribers that repaint their panels on each tick.
window.PCV = window.PCV || {};
(function (PCV) {
  var JOB_HISTORY_LIMIT = 50;
  var STORAGE_KEY = 'pcv_tracked_jobs';
  var ACTIVE_STATES = ['queued', 'running'];
  var FAST_MS = 2000, MAX_MS = 15000, HEARTBEAT_MS = 30000;
  var _tracked = _load();
  var _timer = null;
  var _delay = FAST_MS;
  var _lastOk = 0;
  var _lastError = null;
  var _subs = [];
  var _ticking = false;
  function _load() {
    try {
      var raw = sessionStorage.getItem(STORAGE_KEY);
      var parsed = raw ? JSON.parse(raw) : [];
      return Array.isArray(parsed) ? parsed.filter(function (job) { return job && job.job_id; }) : [];
    } catch (e) { return []; }
  }
  function _save() {
    try { sessionStorage.setItem(STORAGE_KEY, JSON.stringify(_tracked)); } catch (e) { /* storage unavailable */ }
  }
  function _status(job) { return String(job && job.status || '').toLowerCase(); }
  function trackJob(job) {
    if (!job || !job.job_id) return;
    var index = _tracked.findIndex(function (item) { return item.job_id === job.job_id; });
    if (index >= 0) _tracked[index] = job; else _tracked.unshift(job);
    _tracked = _tracked.slice(0, JOB_HISTORY_LIMIT);
    _save();
    _emit('job', job);
    if (ACTIVE_STATES.indexOf(_status(job)) !== -1) start();
  }
  function trackedJobs() { return _tracked.slice(); }
  function clearHistory() { _tracked = []; _save(); _emit('jobs', _tracked); }
  function hasActiveTrackedJobs() {
    return _tracked.some(function (job) { return ACTIVE_STATES.indexOf(_status(job)) !== -1; });
  }
  async function pollTrackedJobs() {
    var ids = _tracked.map(function (job) { return job.job_id; });
    for (var i = 0; i < ids.length; i++) {
      var r = await fetchGet(EP.JOB(ids[i]));
      if (r && r.error) throw new Error(r.error.message || ('job ' + ids[i] + ' unreadable'));
      var job = unwrapData(r);
      if (job && job.job_id) {
        var index = _tracked.findIndex(function (item) { return item.job_id === job.job_id; });
        if (index >= 0) _tracked[index] = job;
      }
    }
    _save();
  }
  function subscribe(fn) {
    if (typeof fn !== 'function') return function () {};
    _subs.push(fn);
    return function () { var i = _subs.indexOf(fn); if (i >= 0) _subs.splice(i, 1); };
  }
  function _emit(kind, payload) {
    _subs.slice().forEach(function (fn) { try { fn(kind, payload); } catch (e) { /* subscriber errors never stop polling */ } });
  }
  function _schedule() {
    if (_timer) clearTimeout(_timer);
    var wait = hasActiveTrackedJobs() ? _delay : HEARTBEAT_MS;
    _timer = setTimeout(_tick, wait);
  }
  async function _tick() {
    _timer = null;
    if (!window.authToken) { _timer = setTimeout(_tick, HEARTBEAT_MS); return; }
    if (document.hidden || _ticking) { _schedule(); return; }
    _ticking = true;
    try {
      await pollTrackedJobs();
      _lastOk = Date.now();
      _lastError = null;
      _delay = FAST_MS;
      _emit('jobs', _tracked);
      if (!hasActiveTrackedJobs()) _emit('heartbeat', { at: _lastOk });
    } catch (e) {
      _lastError = e;
      _delay = Math.min(Math.round(_delay * 1.5), MAX_MS);
      _emit('error', e);
    } finally {
      _ticking = false;
      _schedule();
    }
  }
  function start() {
    if (_timer) return;
    _delay = FAST_MS;
    _timer = setTimeout(_tick, 0);
  }
  function stop() {
    if (_timer) clearTimeout(_timer);
    _timer = null;
  }
  function isHealthy() {
    return !!window.authToken && _lastOk > 0 && (Date.now() - _lastOk) < (HEARTBEAT_MS * 2 + 5000);
  }
  function statusText() {
    if (!window.authToken) return typeof _L === 'function' ? _L('로그인 필요', 'login required') : 'login required';
    if (_lastError) return typeof _L === 'function' ? _L('폴링 오류', 'polling error') : 'polling error';
    return (typeof _L === 'function' ? _L('폴링', 'polling') : 'polling') + (hasActiveTrackedJobs() ? ' ' + (_delay / 1000) + 's' : '');
  }
  document.addEventListener('visibilitychange', function () { if (!document.hidden && window.authToken && !_timer) start(); });
  PCV.events = {
    start: start, stop: stop, trackJob: trackJob, trackedJobs: trackedJobs, clearHistory: clearHistory,
    hasActiveTrackedJobs: hasActiveTrackedJobs, pollTrackedJobs: pollTrackedJobs, subscribe: subscribe,
    isHealthy: isHealthy, statusText: statusText,
    lastOk: function () { return _lastOk; }, lastError: function () { return _lastError; },
    JOB_HISTORY_LIMIT: JOB_HISTORY_LIMIT
  };
  window.trackJob = trackJob;
})(window.PCV);

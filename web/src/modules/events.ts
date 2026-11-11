// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): job polling adapter in place of the Single Edge WebSocket stream, with a PCV.events facade.
// Top-level declarations stay on window.PCV so the module keeps the shared-scope behaviour of the classic-script bundle.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy job-polling.ts ---
// 자동 호출(init, 로그인 완료)은 이미 도는 첫 갱신을 취소하지 않고 같은 Promise를 기다린다.
// token 검사를 통과한 뒤 인증 값이 바뀌면 그 갱신은 옛 자격으로 끝나므로 새로 시작한다.
// force는 사용자가 누른 새로 고침, 연결 변경, token 지우기처럼 새 갱신을 시작한다.
let refreshFlight = null;
let refreshFlightAuth = null;

function refreshAuthValue() {
  return state.authAccessToken.trim() || state.apiToken.trim();
}

function refreshLocalErrors() {
  return {
    summaryError: state.summaryError,
    networkError: state.networkError,
    activityError: state.activityError
  };
}

function refreshLocalAuthStarted(before) {
  return [state.summaryError, state.networkError, state.activityError].some((failure, index) => {
    const previous = index === 0 ? before.summaryError : index === 1 ? before.networkError : before.activityError;
    return Boolean(failure && failure !== previous && isAuthError(failure));
  });
}

async function runRefreshStep(step, requestId, controller, stepFailures) {
  if (requestId !== state.refreshRequestId || controller.signal.aborted) return;
  try {
    await step.run();
  } catch (error) {
    const normalized = normalizeError(error);
    if (normalized.code === 'PCV_REQUEST_ABORTED') return;
    normalized.operation = normalized.operation || step.label;
    stepFailures.push(normalized);
    if (isAuthError(normalized)) controller.abort();
  }
}

async function runRefreshWave(wave, requestId, controller, stepFailures) {
  if (requestId !== state.refreshRequestId) return true;
  const before = refreshLocalErrors();
  await Promise.allSettled(wave.map((step) => runRefreshStep(step, requestId, controller, stepFailures)));
  if (requestId !== state.refreshRequestId) return true;
  if (stepFailures.some(isAuthError) || refreshLocalAuthStarted(before)) {
    if (!controller.signal.aborted) controller.abort();
    return true;
  }
  return false;
}

function publishRefreshResult(stepFailures) {
  const failures = collectRefreshFailures(stepFailures)
    .filter((failure) => failure.code !== 'PCV_REQUEST_ABORTED');
  state.lastRefreshedAt = Date.now();
  state.partialFailures = failures;
  if (failures.length > 0) {
    state.error = buildPartialRefreshError(failures);
    state.connectionState = failures.some(isAuthError) ? 'auth' : 'degraded';
  } else {
    state.error = null;
    state.connectionState = 'connected';
  }
}

async function runRefreshAll(requestId, controller, requestOptions) {
  state.loading = true;
  state.error = null;
  state.partialFailures = [];
  render();
  try {
    await pcvEnsureLoopbackSession();
    if (requestId !== state.refreshRequestId) return;
    const authValue = refreshAuthValue();
    if (!authValue) {
      state.connectionState = 'auth';
      state.lastRefreshedAt = Date.now();
      state.loading = false;
      state.refreshController = null;
      render();
      return;
    }
    if (requestId === state.refreshRequestId) refreshFlightAuth = authValue;

    const steps = [
      { label: 'ops.summary', run: () => loadOpsSummary(requestOptions) },
      { label: 'host.status', run: () => loadHost(requestOptions) },
      { label: 'vm.list', run: () => loadVms(requestOptions) },
      { label: 'network.inventory', run: () => loadNetworkInventory(requestOptions) },
      { label: 'runtime.policy', run: () => loadRuntimePolicy(requestOptions) },
      { label: 'auth.session', run: () => loadAccountSession(requestOptions) },
      { label: 'account.list', run: () => loadAccountDirectory(requestOptions) },
      { label: 'console.capabilities', run: () => loadConsoleCapabilities(requestOptions) },
      { label: 'job.list', run: () => loadServerJobs(requestOptions) },
      { label: 'diagnostic.bundle.list', run: () => loadDiagnosticBundleList(requestOptions) },
      { label: 'job.poll', run: () => pollTrackedJobs(requestOptions) },
      { label: 'vm.selected.refresh', run: () => refreshSelectedVm(requestOptions) }
    ];
    const stepFailures = [];
    const dashboardAuth = await runRefreshWave(steps.slice(0, 3), requestId, controller, stepFailures);
    if (requestId !== state.refreshRequestId) return;
    if (!dashboardAuth) {
      publishRefreshResult(stepFailures);
      render();
      await runRefreshWave(steps.slice(3), requestId, controller, stepFailures);
      if (requestId !== state.refreshRequestId) return;
    }
    publishRefreshResult(stepFailures);
  } catch (error) {
    if (requestId !== state.refreshRequestId) return;
    state.error = normalizeError(error);
    state.connectionState = isAuthError(state.error) ? 'auth' : 'error';
  } finally {
    if (requestId === state.refreshRequestId) {
      state.loading = false;
      state.refreshController = null;
      render();
    }
  }
}

async function refreshAll(options) {
  const force = Boolean(options && options.force === true);
  const authValue = refreshAuthValue();
  const sameAuth = refreshFlightAuth === null || refreshFlightAuth === authValue;
  if (!force && refreshFlight && sameAuth) return refreshFlight;
  if (state.refreshController) state.refreshController.abort();
  refreshFlightAuth = null;
  const requestId = state.refreshRequestId + 1;
  const controller = new AbortController();
  state.refreshRequestId = requestId;
  state.refreshController = controller;
  const flight = runRefreshAll(requestId, controller, { signal: controller.signal });
  refreshFlight = flight;
  return flight.finally(() => {
    if (refreshFlight === flight) {
      refreshFlight = null;
      refreshFlightAuth = null;
    }
  });
}

function pcvTrackJob(job) {
  if (!job?.job_id) return;
  const existingIndex = state.trackedJobs.findIndex((item) => item.job_id === job.job_id);
  if (existingIndex >= 0) state.trackedJobs[existingIndex] = job;
  else state.trackedJobs.unshift(job);
  state.trackedJobs = state.trackedJobs.slice(0, JOB_HISTORY_LIMIT);
  saveTrackedJobsToStorage();
  dropSucceededVmDeleteFromClientList(job);
}

// vm.delete가 succeeded이면 다음 vm.list가 실패해도 그 VM 행과 선택을 바로 뺀다.
function dropSucceededVmDeleteFromClientList(job) {
  if (!job || job.operation !== 'vm.delete') return;
  if (String(job.status || '').toLowerCase() !== 'succeeded') return;
  const params = job.params && typeof job.params === 'object' ? job.params : {};
  const target = String(params.name || params.vm_name || '').trim();
  if (!target) return;
  const rows = Array.isArray(state.vms) ? state.vms : [];
  const removed = [];
  state.vms = rows.filter((vm) => {
    const id = String(vm && vm.id || '');
    const name = String(vm && vm.name || '');
    if (id !== target && name !== target) return true;
    removed.push(vm);
    return false;
  });
  const selectedId = String(state.selectedVmId || '');
  const selected = state.selectedVm || null;
  const selectedName = selected ? String(selected.name || '') : '';
  const selectedObjectId = selected ? String(selected.id || '') : '';
  const removedSelected = removed.some((vm) => {
    const id = String(vm && vm.id || '');
    const name = String(vm && vm.name || '');
    return (id && (id === selectedId || id === selectedObjectId)) ||
      (name && (name === selectedId || name === selectedName));
  });
  if (selectedId === target || selectedName === target || selectedObjectId === target || removedSelected) {
    state.selectedVmId = '';
    state.selectedVm = null;
    state.selectedVmCheckpoints = [];
    state.selectedVmReadbacks = null;
    state.selectedVmQosControl = null;
  }
}

async function pollTrackedJobs(options = {}) {
  const ids = state.trackedJobs.map((job) => job.job_id).filter(Boolean);
  for (const jobId of ids) {
    const job = await desktopApi.getJob(jobId, options);
    pcvTrackJob(job);
  }
}

function hasActiveTrackedJobs() {
  return state.trackedJobs.some((job) => ['queued', 'running'].includes(String(job.status).toLowerCase()));
}

function resetJobPollDelay() {
  state.jobPollDelayMs = 2000;
}

function increaseJobPollDelay() {
  state.jobPollDelayMs = Math.min(Math.round(state.jobPollDelayMs * 1.5), 15000);
}

function clearPollingTimer() {
  if (!state.pollTimer) return;
  window.clearTimeout(state.pollTimer);
  state.pollTimer = null;
}

function scheduleNextPoll(delayMs = state.jobPollDelayMs) {
  clearPollingTimer();
  if (!hasActiveTrackedJobs()) return;
  state.pollTimer = window.setTimeout(runPollTick, delayMs);
}

async function runPollTick() {
  if (!hasActiveTrackedJobs()) {
    clearPollingTimer();
    return;
  }
  try {
    await pollTrackedJobs();
    await loadOpsSummary();
    await loadServerJobs();
    await loadVms();
    await refreshSelectedVm();
    resetJobPollDelay();
  } catch (error) {
    state.error = normalizeError(error);
    increaseJobPollDelay();
  } finally {
    render();
    scheduleNextPoll();
  }
}

function startPolling() {
  resetJobPollDelay();
  scheduleNextPoll(0);
}

async function cancelJob(jobId, cancelScope = 'job') {
  if (cancelScope === 'running-guest-execution') {
    requireRbac('operate', t('copy.rbac.guest_exec_cancel'));
  } else {
    requireRbac('operate', t('copy.rbac.job_cancel'));
  }
  const job = await desktopApi.cancelJob(jobId);
  pcvTrackJob(job);
  render();
}

async function retryJob(jobId) {
  requireRbac('operate', t('copy.rbac.job_retry'));
  const job = await desktopApi.retryJob(jobId);
  pcvTrackJob(job);
  startPolling();
  render();
}

async function reconcileJob(jobId) {
  requireRbac('operate', t('copy.rbac.job_reconcile'));
  const job = await desktopApi.reconcileJob(jobId);
  pcvTrackJob(job);
  render();
}

async function loadNextJobPage() {
  const page = state.serverJobPage || {};
  const nextOffset = page.next_offset;
  if (nextOffset === null || nextOffset === undefined || nextOffset === '') return;
  state.activityError = null;
  try {
    const nextPage = await desktopApi.listJobs(page.limit ?? 50, nextOffset);
    const currentJobs = asArray(state.serverJobs);
    const seen = new Set(currentJobs.map((job) => job?.job_id).filter(Boolean));
    const appended = asArray(nextPage).filter((job) => {
      if (!job?.job_id) return true;
      if (seen.has(job.job_id)) return false;
      seen.add(job.job_id);
      return true;
    });
    state.serverJobs = [...currentJobs, ...appended].slice(0, 200);
    state.serverJobPage = {
      ...nextPage,
      jobs: state.serverJobs,
      returned: state.serverJobs.length,
      offset: page.offset ?? 0
    };
  } catch (error) {
    state.activityError = normalizeError(error);
  }
}

// Single Edge facade (ui/modules/api.js connectWS replacement): polling instead of a WebSocket.
PCV.events = {
  start: function () { startPolling(); },
  stop: function () { clearPollingTimer(); },
  trackJob: pcvTrackJob,
  trackedJobs: function () { return state.trackedJobs.slice(); },
  hasActiveTrackedJobs: hasActiveTrackedJobs,
  pollTrackedJobs: pollTrackedJobs,
  subscribe: function () { return function () {}; },
  isHealthy: function () { return !!(state.lastRefreshedAt && (Date.now() - state.lastRefreshedAt) < 120000); },
  statusText: function () { return String(state.connectionState || 'idle'); },
  clearHistory: function () { if (typeof clearTrackedJobHistory === 'function') clearTrackedJobHistory(); }
};
window.loadAll = refreshAll;
// exports
window.refreshAll = refreshAll;
window.pcvTrackJob = pcvTrackJob;
window.pollTrackedJobs = pollTrackedJobs;
window.hasActiveTrackedJobs = hasActiveTrackedJobs;
window.resetJobPollDelay = resetJobPollDelay;
window.increaseJobPollDelay = increaseJobPollDelay;
window.clearPollingTimer = clearPollingTimer;
window.scheduleNextPoll = scheduleNextPoll;
window.runPollTick = runPollTick;
window.startPolling = startPolling;
window.cancelJob = cancelJob;
window.retryJob = retryJob;
window.reconcileJob = reconcileJob;
window.loadNextJobPage = loadNextJobPage;
PCV.events = Object.assign(PCV.events || {}, { refreshAll, pcvTrackJob, pollTrackedJobs, hasActiveTrackedJobs, resetJobPollDelay, increaseJobPollDelay, clearPollingTimer, scheduleNextPoll, runPollTick, startPolling, cancelJob, retryJob, reconcileJob, loadNextJobPage });
})(window.PCV);

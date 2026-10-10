// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): job polling adapter in place of the Single Edge WebSocket stream (legacy web/src/served/job-polling.ts) with a PCV.events facade.
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy job-polling.ts ---
async function refreshAll() {
  if (state.refreshController) {
    state.refreshController.abort();
  }
  const requestId = state.refreshRequestId + 1;
  const controller = new AbortController();
  state.refreshRequestId = requestId;
  state.refreshController = controller;
  const requestOptions = { signal: controller.signal };
  state.loading = true;
  state.error = null;
  state.partialFailures = [];
  render();
  try {
    await pcvEnsureLoopbackSession();
    if (requestId !== state.refreshRequestId) return;
    if (!state.authAccessToken.trim() && !state.apiToken.trim()) {
      state.connectionState = 'auth';
      state.lastRefreshedAt = Date.now();
      state.loading = false;
      state.refreshController = null;
      render();
      return;
    }

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
    for (const step of steps) {
      if (requestId !== state.refreshRequestId) return;
      try {
        await step.run();
      } catch (error) {
        const normalized = normalizeError(error);
        if (normalized.code === 'PCV_REQUEST_ABORTED') {
          continue;
        }
        normalized.operation = normalized.operation || step.label;
        stepFailures.push(normalized);
        if (isAuthError(normalized)) {
          controller.abort();
          break;
        }
      }
    }

    if (requestId !== state.refreshRequestId) return;
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

function readCreatePayload(form) {
  const data = new FormData(form);
  const payload = {
    name: String(data.get('name') || '').trim(),
    iso_path: String(data.get('iso_path') || '').trim(),
    cpu: Number(data.get('cpu')),
    memory_mb: Number(data.get('memory_mb')),
    disk_gb: Number(data.get('disk_gb')),
    vm_root: String(data.get('vm_root') || '').trim(),
    generation: Number(data.get('generation'))
  };
  if (!payload.name || !payload.iso_path || !payload.vm_root) {
    throw normalizeError({
      code: 'PCV_FORM_INVALID',
      message: 'Required fields are missing.',
      detail: 'Name, ISO path, and VM root are required.'
    });
  }
  return payload;
}

async function submitCreateVm(event) {
  event.preventDefault();
  state.error = null;
  try {
    requireRbac('operate', 'VM create');
    const payload = readCreatePayload(event.currentTarget);
    const job = await desktopApi.createVm(payload);
    pcvTrackJob(job);
    els.createDialog.close();
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  }
  render();
}

function pcvTrackJob(job) {
  if (!job?.job_id) return;
  const existingIndex = state.trackedJobs.findIndex((item) => item.job_id === job.job_id);
  if (existingIndex >= 0) state.trackedJobs[existingIndex] = job;
  else state.trackedJobs.unshift(job);
  state.trackedJobs = state.trackedJobs.slice(0, JOB_HISTORY_LIMIT);
  saveTrackedJobsToStorage();
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
    requireRbac('operate', 'running guest execution cancel');
  } else {
    requireRbac('operate', 'job cancel');
  }
  const job = await desktopApi.cancelJob(jobId);
  pcvTrackJob(job);
  render();
}

async function retryJob(jobId) {
  requireRbac('operate', 'job retry');
  const job = await desktopApi.retryJob(jobId);
  pcvTrackJob(job);
  startPolling();
  render();
}

async function reconcileJob(jobId) {
  requireRbac('operate', 'job reconciliation');
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
window.readCreatePayload = readCreatePayload;
window.submitCreateVm = submitCreateVm;
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
PCV.events = Object.assign(PCV.events || {}, { refreshAll, readCreatePayload, submitCreateVm, pcvTrackJob, pollTrackedJobs, hasActiveTrackedJobs, resetJobPollDelay, increaseJobPollDelay, clearPollingTimer, scheduleNextPoll, runPollTick, startPolling, cancelJob, retryJob, reconcileJob, loadNextJobPage });
})(window.PCV);

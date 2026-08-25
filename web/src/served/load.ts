// @ts-nocheck
async function loadHost(options = {}) {
  state.host = await desktopApi.getHostStatus(options);
}

async function loadVms(options = {}) {
  state.vms = await desktopApi.listVms(options);
  reconcileSelectedVm();
}

async function loadNetworkInventory(options = {}) {
  state.networkError = null;
  try {
    state.networkInventory = await desktopApi.getNetworkInventory(options);
  } catch (error) {
    state.networkError = normalizeError(error);
    state.networkInventory = null;
  }
}

async function loadRuntimePolicy(options = {}) {
  state.runtimePolicy = await desktopApi.getRuntimePolicy(options);
}

async function loadAccountSession(options = {}) {
  if (!state.authAccessToken.trim()) {
    state.authSession = null;
    state.authRbac = null;
    return;
  }

  state.authError = null;
  try {
    state.authSession = await desktopApi.getAccountSession(options);
    state.authRbac = await desktopApi.getAccountRbac(options);
    saveAccountSessionToStorage();
  } catch (error) {
    state.authError = normalizeError(error);
    if (state.authError.code === 'PCV_JWT_EXPIRED' && state.authRefreshToken) {
      await refreshAccountSession({ silent: true });
      if (!state.authAccessToken.trim()) return;
      state.authSession = await desktopApi.getAccountSession(options);
      state.authRbac = await desktopApi.getAccountRbac(options);
      return;
    }
    if (isAuthError(state.authError)) {
      state.authAccessToken = '';
      state.authSession = null;
      saveAccountSessionToStorage();
    }
  }
}

async function loadConsoleCapabilities(options = {}) {
  state.consoleError = null;
  try {
    state.consoleCapabilities = await desktopApi.getConsoleCapabilities(options);
  } catch (error) {
    state.consoleError = normalizeError(error);
    state.consoleCapabilities = null;
  }
}

async function loadOpsSummary(options = {}) {
  state.summaryError = null;
  try {
    state.opsSummary = await desktopApi.getOpsSummary(options);
  } catch (error) {
    state.summaryError = normalizeError(error);
    state.opsSummary = null;
  }
}

async function loadServerJobs(options = {}) {
  state.activityError = null;
  try {
    const page = await desktopApi.listJobs(50, 0, options);
    state.serverJobPage = page;
    state.serverJobs = asArray(page);
  } catch (error) {
    state.activityError = normalizeError(error);
    state.serverJobPage = null;
    state.serverJobs = [];
  }
}

async function loadDiagnosticBundleList(options = {}) {
  try {
    const page = await desktopApi.listDiagnosticBundles(10, 0, options);
    state.diagnosticBundleError = null;
    state.diagnosticBundlePage = page;
    state.diagnosticBundles = asArray(page);
    if (!getDiagnosticBundleId() && state.diagnosticBundles.length > 0) {
      state.diagnosticBundle = state.diagnosticBundles[0];
    }
  } catch (error) {
    const normalized = normalizeError(error);
    state.diagnosticBundlePage = null;
    state.diagnosticBundles = [];
    state.diagnosticBundleError = normalized;
  }
}

async function loadNextDiagnosticBundlePage() {
  const page = state.diagnosticBundlePage || {};
  const nextOffset = page.next_offset;
  if (nextOffset === null || nextOffset === undefined || nextOffset === '') return;
  state.pendingDiagnosticAction = 'listing';
  state.diagnosticBundleError = null;
  render();
  try {
    const nextPage = await desktopApi.listDiagnosticBundles(page.limit ?? 10, nextOffset);
    const currentBundles = asArray(state.diagnosticBundles);
    const seen = new Set(currentBundles.map((bundle) => getDiagnosticBundleId(bundle)).filter(Boolean));
    const appended = asArray(nextPage).filter((bundle) => {
      const bundleId = getDiagnosticBundleId(bundle);
      if (!bundleId) return true;
      if (seen.has(bundleId)) return false;
      seen.add(bundleId);
      return true;
    });
    state.diagnosticBundles = [...currentBundles, ...appended].slice(0, 100);
    state.diagnosticBundlePage = {
      ...nextPage,
      bundles: state.diagnosticBundles,
      returned: state.diagnosticBundles.length,
      offset: page.offset ?? 0
    };
  } catch (error) {
    state.diagnosticBundleError = normalizeError(error);
  } finally {
    state.pendingDiagnosticAction = '';
    render();
  }
}

function findCachedVm(vmId) {
  return asArray(state.vms).find((vm) => getVmId(vm) === vmId || getVmName(vm) === vmId) || null;
}

function reconcileSelectedVm() {
  if (!state.selectedVmId) return;
  const cached = findCachedVm(state.selectedVmId);
  if (cached) {
    state.selectedVm = cached;
    return;
  }

  state.error = normalizeError({
    code: 'PCV_SELECTED_VM_STALE',
    message: 'Selected VM is no longer present in the current inventory.',
    detail: 'The detail panel and checkpoint list were cleared after refresh so stale lifecycle controls cannot be used.'
  });
  state.selectedVmId = '';
  state.selectedVm = null;
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = null;
  state.selectedVmQosControl = null;
}

function emptyVmReadbackState(vmId, previous = null) {
  return {
    vm_id: vmId,
    loading: true,
    updated_at: previous?.vm_id === vmId ? previous.updated_at || '' : '',
    values: previous?.vm_id === vmId ? previous.values || {} : {},
    errors: []
  };
}

async function loadVmQosGuestReadbacks(vmId, options = {}) {
  if (!vmId) return;
  const silent = Boolean(options.silent);
  const { silent: _silent, ...requestOptions } = options;
  state.selectedVmReadbacks = emptyVmReadbackState(vmId, state.selectedVmReadbacks);
  if (!silent) render();

  const steps = [
    ['blkio', () => desktopApi.getVmBlkio(vmId, requestOptions)],
    ['bandwidth', () => desktopApi.getVmBandwidth(vmId, requestOptions)],
    ['guest_agent', () => desktopApi.getVmGuestAgentStatus(vmId, requestOptions)],
    ['guest_ping', () => desktopApi.getVmGuestAgentPing(vmId, requestOptions)]
  ];
  const results = await Promise.allSettled(steps.map(([, run]) => run()));
  if (state.selectedVmId !== vmId) return;

  const values = {};
  const errors = [];
  results.forEach((result, index) => {
    const key = steps[index][0];
    if (result.status === 'fulfilled') {
      values[key] = result.value;
      return;
    }

    errors.push({ key, ...normalizeError(result.reason) });
  });

  state.selectedVmReadbacks = {
    vm_id: vmId,
    loading: false,
    updated_at: new Date().toISOString(),
    values,
    errors
  };

  if (!silent) render();
}

async function loadVmDetail(vmId) {
  state.selectedVmId = vmId;
  state.selectedVm = findCachedVm(vmId);
  state.selectedVmCheckpoints = [];
  state.selectedVmReadbacks = emptyVmReadbackState(vmId);
  state.selectedVmQosControl = null;
  render();
  const vm = await desktopApi.getVm(vmId);
  if (state.selectedVmId === vmId) {
    state.selectedVm = vm;
  }
  await loadCheckpoints(vmId);
  await loadVmQosGuestReadbacks(vmId, { silent: true });
}

async function selectVmFromShell(vmId) {
  if (!vmId) return;
  setActiveView('vms');
  window.location.hash = '#vms';
  state.error = null;
  await loadVmDetail(vmId);
  state.connectionState = 'connected';
  render();
}

async function loadCheckpoints(vmId, options = {}) {
  const checkpoints = await desktopApi.getVmCheckpoints(vmId, options);
  if (state.selectedVmId === vmId) {
    state.selectedVmCheckpoints = asArray(checkpoints);
  }
}

async function refreshSelectedVm(options = {}) {
  if (!state.selectedVmId) return;
  const vmId = state.selectedVmId;
  try {
    const vm = await desktopApi.getVm(vmId, options);
    if (state.selectedVmId === vmId) {
      state.selectedVm = vm;
    }
    await loadCheckpoints(vmId, options);
    await loadVmQosGuestReadbacks(vmId, { ...options, silent: true });
  } catch (error) {
    const normalized = normalizeError(error);
    if (normalized.code === 'PCV_VM_NOT_FOUND' && state.selectedVmId === vmId) {
      state.selectedVmId = '';
      state.selectedVm = null;
      state.selectedVmCheckpoints = [];
      state.selectedVmReadbacks = null;
      state.selectedVmQosControl = null;
    }
    state.error = normalized;
  }
}


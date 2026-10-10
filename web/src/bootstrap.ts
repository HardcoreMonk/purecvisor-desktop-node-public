// @ts-nocheck
// Ported from purecvisor ui/app.js (Apache-2.0, same author): bootstrap position and window.PCV conventions.
// Desktop Node bootstrap for web/app.bundle.js (ADR-0018): the legacy entry part (web/src/served-app.ts: element
// bindings, event wiring, init) kept verbatim with the shell elements that index.next.html does not carry guarded,
// followed by the Single Edge startup (shell mount, session restore).
window.PCV = window.PCV || {};
window.PCV.config = Object.assign({}, window.PCV.config || {}, {
  EDITION: 'desktop-node',
  API_BASE: '/api/v1',
  BUNDLE: 'app.bundle.js'
});
(function (PCV) {
function bindEvents() {
  window.addEventListener('hashchange', () => {
    setActiveView(getHashView());
    render();
  });
  els.connectionForm?.addEventListener('submit', (event) => {
    event.preventDefault();
    state.apiBaseUrl = els.apiBaseUrl.value.trim() || window.location.origin;
    state.apiToken = els.apiToken.value.trim();
    refreshAll();
  });
  els.clearToken?.addEventListener('click', clearBrowserToken);
  els.themeSelect?.addEventListener('change', () => {
    state.theme = els.themeSelect.value || 'supanova';
    render();
  });
  els.languageSelect?.addEventListener('change', () => {
    state.language = els.languageSelect.value || 'ko';
    render();
  });
  els.globalSearchInput?.addEventListener('input', () => handleCommandSearch(els.globalSearchInput.value));
  els.openCommandPalette?.addEventListener('click', () => openCommandPalette(state.globalSearch));
  els.commandPaletteInput?.addEventListener('input', () => {
    state.commandQuery = els.commandPaletteInput.value;
    state.commandPaletteOpen = true;
    render();
  });
  els.commandPaletteResults?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-command-id]');
    if (!button) return;
    try {
      await runCommandPaletteItem(button.dataset.commandId);
    } catch (error) {
      state.error = normalizeError(error);
      render();
    }
  });
  els.accountSessionPanel?.addEventListener('submit', async (event) => {
    if (event.target.closest('form#account-login-form')) {
      await loginAccountFromForm(event);
      return;
    }
    if (event.target.closest('form#account-create-form')) {
      await createAccountFromForm(event);
    }
  });
  els.accountSessionPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    try {
      if (button.dataset.action === 'account-refresh') {
        await refreshAccountSession();
      } else if (button.dataset.action === 'account-logout') {
        await logoutAccount();
      } else if (button.dataset.action === 'account-disable') {
        await disableAccountFromButton(button.dataset.username);
      }
    } catch (error) {
      state.authError = normalizeError(error);
      render();
    }
  });
  els.accountConsolePanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action="console-open-selected"]');
    if (!button) return;
    try {
      await openSelectedConsole();
    } catch (error) {
      state.consoleError = normalizeError(error);
      render();
    }
  });
  els.tokenRotationPanel?.addEventListener('click', (event) => {
    const button = event.target.closest('button[data-action="clear-browser-token"]');
    if (!button) return;
    clearBrowserToken();
  });
  els.diagnosticsPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    state.error = null;
    try {
      if (button.dataset.action === 'diagnostic-create') {
        await createDiagnosticBundleFromPanel();
      } else if (button.dataset.action === 'diagnostic-download') {
        await downloadLatestDiagnosticBundle();
      } else if (button.dataset.action === 'diagnostic-list-next') {
        await loadNextDiagnosticBundlePage();
      } else if (button.dataset.action === 'diagnostic-retry') {
        if (state.lastDiagnosticAction === 'download' && getDiagnosticBundleId()) await downloadLatestDiagnosticBundle();
        else await createDiagnosticBundleFromPanel();
      }
    } catch (error) {
      state.diagnosticBundleError = normalizeError(error);
      render();
    }
  });
  els.refreshAll?.addEventListener('click', refreshAll);
  els.vmFilter?.addEventListener('input', () => {
    state.vmFilter = els.vmFilter.value;
    render();
  });
  els.vmStateFilter?.addEventListener('change', () => {
    state.vmStateFilter = els.vmStateFilter.value || 'all';
    render();
  });
  els.vmSort?.addEventListener('change', () => {
    state.vmSort = els.vmSort.value || 'name';
    render();
  });
  bindJobAndNetworkFilterEvents();
  els.assetSearchInput?.addEventListener('input', renderVmAssetList);
  document.addEventListener('keydown', (event) => {
    if ((event.ctrlKey || event.metaKey) && String(event.key || '').toLowerCase() === 'k') {
      event.preventDefault();
      openCommandPalette(state.globalSearch);
      return;
    }
    if (event.key === 'Escape' && state.commandPaletteOpen) {
      event.preventDefault();
      closeCommandPalette();
      return;
    }
    if (event.key === 'Enter' && state.commandPaletteOpen && document.activeElement === els.commandPaletteInput) {
      event.preventDefault();
      const first = getCommandPaletteMatches()[0];
      if (first) {
        runCommandPaletteItem(first.id).catch((error) => {
          state.error = normalizeError(error);
          render();
        });
      }
    }
  });
  els.vmAssetList?.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action="select-asset-vm"]');
    if (!button) return;
    try {
      await selectVmFromShell(button.dataset.vmId);
    } catch (error) {
      state.error = normalizeError(error);
      render();
    }
  });
  document.addEventListener('click', (event) => {
    const viewLink = event.target.closest('[data-view-link]');
    if (viewLink) {
      event.preventDefault();
      navigateToView(viewLink.dataset.viewLink);
      return;
    }
    const shellAction = event.target.closest('[data-shell-action]');
    if (shellAction) {
      event.preventDefault();
      handleShellCommand(shellAction.dataset.shellAction);
      return;
    }
    const menuCommand = event.target.closest('[data-menu-command]');
    if (menuCommand) {
      event.preventDefault();
      handleShellCommand(menuCommand.dataset.menuCommand);
    }
  });
  els.openCreateVm?.addEventListener('click', () => els.createDialog.showModal());
  els.closeCreateVm?.addEventListener('click', () => els.createDialog.close());
  els.createVmForm?.addEventListener('submit', submitCreateVm);
  els.vmTable?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action="select-vm"]');
    const row = event.target.closest('tr[data-vm-id]');
    const vmId = button?.dataset.vmId || row?.dataset.vmId;
    if (!vmId) return;
    state.error = null;
    try {
      await loadVmDetail(vmId);
      state.connectionState = 'connected';
    } catch (error) {
      state.error = normalizeError(error);
    }
    render();
  });
  els.closeVmDetail?.addEventListener('click', () => {
    state.selectedVmId = '';
    state.selectedVm = null;
    state.selectedVmCheckpoints = [];
    state.selectedVmReadbacks = null;
    state.selectedVmQosControl = null;
    render();
  });
  els.vmDetailPanel?.addEventListener('submit', async (event) => {
    const form = event.target.closest('form[data-action]');
    const submitterAction = event.submitter?.dataset?.action || '';
    const qosForm = event.target.closest('form[data-qos-kind]');
    const guestForm = event.target.closest('form[data-guest-execution-kind]');
    if (!form && !qosForm && !guestForm) return;
    event.preventDefault();
    state.error = null;
    try {
      const data = new FormData(form || qosForm || guestForm);
      if (submitterAction === 'vm-qos-storage-preview' || submitterAction === 'vm-qos-storage-apply') {
        await queueVmQosDirectControl(
          qosForm.dataset.vmId,
          'storage',
          submitterAction.endsWith('-apply') ? 'apply' : 'preview',
          readVmQosPayload('storage', data));
      } else if (submitterAction === 'vm-qos-network-preview' || submitterAction === 'vm-qos-network-apply') {
        await queueVmQosDirectControl(
          qosForm.dataset.vmId,
          'network',
          submitterAction.endsWith('-apply') ? 'apply' : 'preview',
          readVmQosPayload('network', data));
      } else if (submitterAction === 'vm-guest-exec-preview' || submitterAction === 'guest-agent-channel-preview') {
        await handleVmGuestPreviewSubmit(guestForm, submitterAction, data);
      } else if (submitterAction === 'vm-guest-exec') {
        await queueVmGuestExecutionControl(
          guestForm.dataset.vmId,
          'exec',
          readVmGuestExecPayload(data));
      } else if (submitterAction === 'guest-agent-ensure-channel') {
        const mode = event.submitter?.dataset?.guestChannelMode === 'repair' ? 'repair' : 'verify';
        await queueVmGuestExecutionControl(
          guestForm.dataset.vmId,
          mode,
          readVmGuestChannelPayload(data, mode));
      } else if (form.dataset.action === 'vm-guest-file') {
        await queueVmGuestFile(form.dataset.vmId, readVmGuestFilePayload(data));
      } else if (form.dataset.action === 'checkpoint-create') {
        await queueCheckpointCreate(form.dataset.vmId, data.get('checkpoint_name'));
        form.reset();
      } else if (form.dataset.action === 'vm-set-memory') {
        await queueVmResourceMutation(form.dataset.vmId, 'set-memory', 'memory_mb', data.get('memory_mb'));
        form.reset();
      } else if (form.dataset.action === 'vm-set-vcpu') {
        await queueVmResourceMutation(form.dataset.vmId, 'set-vcpu', 'cpu', data.get('cpu'));
        form.reset();
      } else if (form.dataset.action === 'vm-disk-resize') {
        await queueVmResourceMutation(form.dataset.vmId, 'disk-resize', 'disk_gb', data.get('disk_gb'));
        form.reset();
      } else if (form.dataset.action === 'vm-attach') {
        await queueVmAttach(form.dataset.vmId, data.get('iso_path'));
        form.reset();
      } else if (form.dataset.action === 'vm-clone') {
        await queueVmClone(form.dataset.vmId, data.get('name'));
        form.reset();
      } else {
        await handleVmDetailExtensionSubmit(form, data, submitterAction);
      }
    } catch (error) {
      state.error = normalizeError(error);
    }
    render();
  });
  els.vmDetailPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    // BL-0016: a submit button inside a form belongs to the panel submit handler above. Handling it here
    // re-renders the panel during the click, which detaches the form before the browser runs the submit
    // algorithm and cancels it ("Form submission canceled because the form is not connected").
    if (button.type === 'submit' && button.form) return;
    const action = VM_LIFECYCLE_ACTIONS[button.dataset.action];
    state.error = null;
    try {
      if (action) {
        await queueVmLifecycle(button.dataset.vmId, action);
      } else if (button.dataset.action === 'vm-delete-status') {
        await refreshVmDeleteStatus(button.dataset.vmId);
      } else if (button.dataset.action === 'vm-manage') {
        await queueVmManage(button.dataset.vmId);
      } else if (button.dataset.action === 'vm-template-lock') {
        await queueVmTemplateLock(button.dataset.vmId, true);
      } else if (button.dataset.action === 'vm-template-unlock') {
        await queueVmTemplateLock(button.dataset.vmId, false);
      } else if (button.dataset.action === 'vm-clone') {
        if (button.closest('form[data-action="vm-clone"]')) {
          return;
        }
        const cloneForm = els.vmDetailPanel.querySelector('form[data-action="vm-clone"]');
        await queueVmClone(button.dataset.vmId, cloneForm ? new FormData(cloneForm).get('name') : '');
      } else if (button.dataset.action === 'vm-delete') {
        await queueVmDelete(button.dataset.vmId);
      } else if (button.dataset.action === 'vm-console') {
        await openSelectedConsole();
      } else if (button.dataset.action === 'vm-qos-guest-refresh') {
        await loadVmQosGuestReadbacks(button.dataset.vmId);
      } else if (button.dataset.action === 'checkpoint-refresh') {
        await loadCheckpoints(button.dataset.vmId);
      } else if (button.dataset.action === 'checkpoint-restore') {
        await queueCheckpointRestore(button.dataset.vmId, button.dataset.checkpointId);
      } else if (button.dataset.action === 'checkpoint-delete') {
        await queueCheckpointDelete(button.dataset.vmId, button.dataset.checkpointId);
      } else if (VM_DETAIL_EXTENSION_CLICK_ACTIONS.has(button.dataset.action)) {
        await handleVmDetailExtensionClick(button);
      }
    } catch (error) {
      state.error = normalizeError(error);
    }
    render();
  });
  els.jobsPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    state.error = null;
    try {
      if (button.dataset.action === 'cancel-job') await cancelJob(button.dataset.jobId, button.dataset.jobCancelScope);
      if (button.dataset.action === 'retry-job') await retryJob(button.dataset.jobId);
      if (button.dataset.action === 'reconcile-job') await reconcileJob(button.dataset.jobId);
    } catch (error) {
      state.error = normalizeError(error);
      render();
    }
  });
  els.activityPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    state.error = null;
    try {
      let shouldReloadJobs = false;
      if (button.dataset.action === 'cancel-job') {
        await cancelJob(button.dataset.jobId, button.dataset.jobCancelScope);
        shouldReloadJobs = true;
      }
      if (button.dataset.action === 'retry-job') {
        await retryJob(button.dataset.jobId);
        shouldReloadJobs = true;
      }
      if (button.dataset.action === 'reconcile-job') {
        await reconcileJob(button.dataset.jobId);
        shouldReloadJobs = true;
      }
      if (button.dataset.action === 'load-next-jobs') await loadNextJobPage();
      if (shouldReloadJobs) await loadServerJobs();
    } catch (error) {
      state.error = normalizeError(error);
    }
    render();
  });
  els.clearJobHistory?.addEventListener('click', clearTrackedJobHistory);
}

function init() {
  Object.assign(els, {
    connectionForm: byId('connection-form'),
    apiBaseUrl: byId('api-base-url'),
    apiToken: byId('api-token'),
    themeSelect: byId('theme-select'),
    languageSelect: byId('language-select'),
    globalSearchInput: byId('global-search-input'),
    openCommandPalette: byId('open-command-palette'),
    commandPalette: byId('command-palette'),
    commandPaletteInput: byId('command-palette-input'),
    commandPaletteResults: byId('command-palette-results'),
    clearToken: byId('clear-token'),
    refreshAll: byId('refresh-all'),
    connectionState: byId('connection-state'),
    statusConnection: byId('status-connection'),
    statusHost: byId('status-host'),
    statusUpdated: byId('status-updated'),
    statusVmCount: byId('status-vm-count'),
    statusView: byId('status-view'),
    heroWorkload: byId('hero-workload'),
    heroHostMode: byId('hero-host-mode'),
    heroAlerts: byId('hero-alerts'),
    assetStatus: byId('asset-status'),
    assetCount: byId('asset-count'),
    assetSearchInput: byId('asset-search-input'),
    vmAssetList: byId('vm-asset-list'),
    workspaceTabbar: byId('workspace-tabbar'),
    alertRegion: byId('alert-region'),
    opsSummaryPanel: byId('ops-summary-panel'),
    priorityPanel: byId('priority-panel'),
    dashboardActivityPanel: byId('dashboard-activity-panel'),
    metricGrid: byId('metric-grid'),
    hostDetails: byId('host-details'),
    vmFilter: byId('vm-filter'),
    vmStateFilter: byId('vm-state-filter'),
    vmSort: byId('vm-sort'),
    jobFilter: byId('job-filter'),
    jobStatusFilter: byId('job-status-filter'),
    jobSort: byId('job-sort'),
    networkFilter: byId('network-filter'),
    vmTable: byId('vm-table'),
    networkInventoryPanel: byId('network-inventory-panel'),
    vmDetailPanel: byId('vm-detail-panel'),
    vmWorkbenchContext: byId('vm-workbench-context'),
    vmDetailTitle: byId('vm-detail-title'),
    vmDetailContent: byId('vm-detail-content'),
    closeVmDetail: byId('close-vm-detail'),
    jobsPanel: byId('jobs-panel'),
    clearJobHistory: byId('clear-job-history'),
    activityPanel: byId('activity-panel'),
    eventCenterPanel: byId('event-center-panel'),
    evidencePanel: byId('evidence-panel'),
    monitoringPanel: byId('monitoring-panel'),
    incidentPanel: byId('incident-panel'),
    accountSessionPanel: byId('account-session-panel'),
    accountConsolePanel: byId('account-console-panel'),
    accountPassword: byId('account-password'),
    tokenRotationPanel: byId('token-rotation-panel'),
    diagnosticsPanel: byId('diagnostics-panel'),
    betaFollowupPanel: byId('beta-followup-panel'),
    troubleshootingPanel: byId('troubleshooting-panel'),
    openCreateVm: byId('open-create-vm'),
    closeCreateVm: byId('close-create-vm'),
    createDialog: byId('create-vm-dialog'),
    createVmForm: byId('create-vm-form')
  });
  setActiveView(getHashView());
  state.trackedJobs = loadTrackedJobsFromStorage();
  loadAccountSessionFromStorage();
  if (els.apiBaseUrl) els.apiBaseUrl.value = state.apiBaseUrl;
  bindEvents();
  render();
  refreshAll();
}


window.bindEvents = bindEvents;
window.init = init;
function pcvBootstrap() {
  try { init(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:init', e); }
  if (PCV.shell && typeof PCV.shell.mount === 'function') { try { PCV.shell.mount(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:shell', e); } }
  if (typeof restoreSession === 'function' && document.getElementById('login-page')) restoreSession();
  if (typeof navigateToHash === 'function') { try { navigateToHash(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:hash', e); } }
}
PCV.bootstrap = pcvBootstrap;
document.addEventListener('DOMContentLoaded', pcvBootstrap);
})(window.PCV);

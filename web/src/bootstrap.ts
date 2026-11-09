// @ts-nocheck
// Ported from purecvisor ui/app.js (Apache-2.0, same author): bootstrap position and window.PCV conventions.
// Desktop Node bootstrap for web/app.bundle.js (ADR-0018): element bindings, event wiring, and init, with shell
// elements that index.html does not carry guarded, followed by the Single Edge startup (shell mount, session restore).
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
    state.theme = els.themeSelect.value || 'supanova-cyan';
    render();
  });
  els.languageSelect?.addEventListener('change', () => {
    state.language = els.languageSelect.value || 'ko';
    render();
  });
  els.openCommandPalette?.addEventListener('click', () => {
    if (typeof toggleGlobalSearch === 'function') toggleGlobalSearch();
  });
  els.accountSessionPanel?.addEventListener('submit', async (event) => {
    if (event.target.closest('form#account-create-form')) {
      await createAccountFromForm(event);
    }
  });
  els.accountSessionPanel?.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    try {
      if (button.dataset.action === 'open-login-overlay') {
        if (typeof pcvSetLoginVisible === 'function') pcvSetLoginVisible(true);
      } else if (button.dataset.action === 'account-refresh') {
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
  document.getElementById('vm-state-chips')?.addEventListener('click', (event) => {
    const chip = event.target.closest('[data-vm-chip]');
    if (!chip) return;
    state.vmStateFilter = chip.getAttribute('data-vm-chip') || 'all';
    render();
  });
  els.vmSort?.addEventListener('change', () => {
    state.vmSort = els.vmSort.value || 'name';
    render();
  });
  bindJobAndNetworkFilterEvents();
  document.addEventListener('keydown', (event) => {
    if ((event.ctrlKey || event.metaKey) && String(event.key || '').toLowerCase() === 'k') {
      event.preventDefault();
      if (typeof toggleGlobalSearch === 'function') toggleGlobalSearch();
      return;
    }
    if (event.key === 'Escape') {
      if (typeof closeCmdPalette === 'function') closeCmdPalette();
      if (typeof closeKbdHelp === 'function') closeKbdHelp();
      if (typeof closeMobileSB === 'function') closeMobileSB();
      const more = document.getElementById('pcv-vm-more-menu');
      if (more) more.hidden = true;
      const moreToggle = document.querySelector('[data-action="vm-more-toggle"]');
      if (moreToggle) moreToggle.setAttribute('aria-expanded', 'false');
      const ctx = document.getElementById('ctx');
      if (ctx && PCV.uxlib && typeof PCV.uxlib.clearEl === 'function') PCV.uxlib.clearEl(ctx);
      const menu = document.getElementById('pcv-ctx-menu');
      if (menu && menu.parentNode) menu.parentNode.removeChild(menu);
      return;
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
  if (typeof bindCreateVmWizard === 'function') bindCreateVmWizard();
  els.openCreateVm?.addEventListener('click', () => els.createDialog.showModal());
  els.closeCreateVm?.addEventListener('click', () => els.createDialog.close());
  els.vmTable?.addEventListener('click', async (event) => {
    const action = event.target.closest('button[data-action]');
    const row = event.target.closest('tr[data-vm-id]');
    if (action && action.dataset.action && action.dataset.action !== 'select-vm') {
      event.preventDefault();
      if (action.disabled) return;
      const actionVmId = action.dataset.vmId || row?.dataset.vmId;
      if (!actionVmId) return;
      state.error = null;
      try {
        if (action.dataset.action === 'vm-row-start') await queueVmLifecycle(actionVmId, 'start');
        else if (action.dataset.action === 'vm-row-console') {
          window.__pcvVmDetailTab = 'console';
          await loadVmDetail(actionVmId);
          state.connectionState = 'connected';
        } else if (action.dataset.action === 'vm-row-clone') openVmCloneModal(actionVmId);
      } catch (error) {
        state.error = normalizeError(error);
      }
      render();
      return;
    }
    const button = action && action.dataset.action === 'select-vm' ? action : null;
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
  els.templateTable?.addEventListener('click', async (event) => {
    const action = event.target.closest('button[data-action]');
    const row = event.target.closest('tr[data-vm-id]');
    if (action) {
      event.preventDefault();
      if (action.disabled) return;
      const actionVmId = action.dataset.vmId || row?.dataset.vmId;
      if (!actionVmId) return;
      state.error = null;
      try {
        if (action.dataset.action === 'vm-row-clone') openVmCloneModal(actionVmId);
        else if (action.dataset.action === 'vm-template-unlock') await queueVmTemplateLock(actionVmId, false);
      } catch (error) {
        state.error = normalizeError(error);
        render();
      }
      return;
    }
    const vmId = row?.dataset.vmId;
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
        await openVmCloneModal(form.dataset.vmId);
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
      } else if (button.dataset.action === 'vm-clone' || button.dataset.action === 'vm-clone-open') {
        await openVmCloneModal(button.dataset.vmId);
      } else if (button.dataset.action === 'vm-more-toggle') {
        // The menu toggle listener owns open and close. Re-rendering here would rebuild the menu shut.
      } else if (button.dataset.action === 'vm-open-console-tab') {
        window.__pcvVmDetailTab = 'console';
      } else if (button.dataset.action === 'vm-open-jobs-tab') {
        window.__pcvVmDetailTab = 'jobs';
      } else if (button.dataset.action === 'vm-export-focus') {
        window.__pcvVmDetailTab = 'summary';
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
    if (button.dataset.action === 'vm-more-toggle') return;
    render();
    if (button.dataset.action === 'vm-export-focus') {
      const exportDirectory = els.vmDetailPanel.querySelector('form[data-action="vm-export"] input[name="directory"]');
      if (exportDirectory && typeof exportDirectory.focus === 'function') exportDirectory.focus();
    }
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
    openCommandPalette: byId('open-command-palette'),
    clearToken: byId('clear-token'),
    refreshAll: byId('refresh-all'),
    heroWorkload: byId('hero-workload'),
    heroHostMode: byId('hero-host-mode'),
    heroAlerts: byId('hero-alerts'),
    alertRegion: byId('alert-region'),
    opsSummaryPanel: byId('ops-summary-panel'),
    priorityPanel: byId('priority-panel'),
    dashboardActivityPanel: byId('dashboard-activity-panel'),
    metricGrid: byId('metric-grid'),
    hostDetails: byId('host-details'),
    vmFilter: byId('vm-filter'),
    vmSort: byId('vm-sort'),
    jobFilter: byId('job-filter'),
    jobStatusFilter: byId('job-status-filter'),
    jobSort: byId('job-sort'),
    networkFilter: byId('network-filter'),
    vmTable: byId('vm-table'),
    templateTable: byId('template-table'),
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
// Single Edge service worker registration at the web root (updateViaCache none, SKIP_WAITING handshake, reload on
// controller change). Loopback http is a secure context; on plain-http LAN the browser rejects the registration and the
// console keeps working without the cache (pcv-single-edge-frontend-structure-v1 §3, Task 15).
function pcvRegisterServiceWorker() {
  if (typeof navigator !== 'object' || !navigator || !('serviceWorker' in navigator)) return;
  window.addEventListener('load', function () {
    navigator.serviceWorker.register('sw.js', { updateViaCache: 'none' }).then(function (reg) {
      window._swRegError = null;
      if (reg.waiting) reg.waiting.postMessage({ type: 'SKIP_WAITING' });
      reg.addEventListener('updatefound', function () {
        var nw = reg.installing;
        if (!nw) return;
        nw.addEventListener('statechange', function () {
          if (nw.state === 'installed' && navigator.serviceWorker.controller) nw.postMessage({ type: 'SKIP_WAITING' });
        });
      });
      reg.update().catch(function () { /* offline update check */ });
    }).catch(function (err) {
      window._swRegError = { name: err && err.name, message: err && err.message ? String(err.message) : String(err) };
    });
    navigator.serviceWorker.addEventListener('controllerchange', function () { window.location.reload(); });
  });
}
PCV.registerServiceWorker = pcvRegisterServiceWorker;
function pcvBootstrap() {
  try { init(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:init', e); }
  if (PCV.shell && typeof PCV.shell.mount === 'function') { try { PCV.shell.mount(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:shell', e); } }
  if (typeof restoreSession === 'function' && document.getElementById('login-page')) restoreSession();
  if (typeof navigateToHash === 'function') { try { navigateToHash(); } catch (e) { if (window._DEBUG) console.warn('bootstrap:hash', e); } }
  pcvRegisterServiceWorker();
}
PCV.bootstrap = pcvBootstrap;
document.addEventListener('DOMContentLoaded', pcvBootstrap);
})(window.PCV);

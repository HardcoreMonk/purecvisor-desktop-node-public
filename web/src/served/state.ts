const WEB_ASSET_LABEL = 'app.js';

function resolveInitialApiBaseUrl(): string {
  const configured = window.PCV_DESKTOP_NODE_CONFIG?.apiBaseUrl;
  return typeof configured === 'string' && configured.trim()
    ? configured.trim().replace(/\/$/, '')
    : window.location.origin;
}

const state: PcvState = {
  apiBaseUrl: resolveInitialApiBaseUrl(),
  apiToken: '',
  authAccessToken: '',
  authRefreshToken: '',
  authSession: null,
  authRbac: null,
  authError: null,
  authPending: false,
  activeView: 'dashboard',
  host: null,
  vms: [],
  networkInventory: null,
  networkError: null,
  vmFilter: '',
  vmStateFilter: 'all',
  vmSort: 'name',
  jobFilter: '',
  jobStatusFilter: 'all',
  jobSort: 'updated:desc',
  networkFilter: '',
  commandPaletteOpen: false,
  commandQuery: '',
  globalSearch: '',
  shellMessage: '',
  trackedJobs: [],
  runtimePolicy: null,
  opsSummary: null,
  serverJobs: [],
  serverJobPage: null,
  loading: false,
  error: null,
  summaryError: null,
  activityError: null,
  partialFailures: [],
  connectionState: 'idle',
  lastRefreshedAt: null,
  pollTimer: null,
  selectedVmId: '',
  selectedVm: null,
  selectedVmCheckpoints: [],
  selectedVmReadbacks: null,
  selectedVmQosControl: null,
  diagnosticBundle: null,
  diagnosticBundles: [],
  diagnosticBundlePage: null,
  diagnosticBundleDownload: null,
  diagnosticBundleError: null,
  consoleCapabilities: null,
  consoleSession: null,
  consoleError: null,
  pendingDiagnosticAction: '',
  lastDiagnosticAction: '',
  tokenActionMessage: '',
  actionPending: false,
  checkpointPending: false,
  pendingVmActions: {},
  pendingCheckpoints: {},
  refreshRequestId: 0,
  refreshController: null,
  jobPollDelayMs: 2000,
  theme: 'supanova',
  language: 'ko'
};

const JOB_HISTORY_KEY = 'pcvDesktopTrackedJobs.v1';
const ACCOUNT_SESSION_KEY = 'pcvDesktopAccountSession.v1';
const JOB_HISTORY_LIMIT = 50;
const DIAGNOSTIC_BUNDLE_ROOT = '%ProgramData%\\PureCVisor\\desktop-node\\diagnostics';
const TOKEN_PROTECTED_FILE = '%ProgramData%\\PureCVisor\\desktop-node\\api-token.dpapi.json';
const els: Record<string, HTMLElement | null> = {};

function byId(id: string): HTMLElement | null {
  return document.getElementById(id);
}

function getHashView(): PcvView {
  const value = String(window.location.hash || '').replace(/^#/, '').toLowerCase();
  return VALID_VIEWS.has(value) ? value as PcvView : 'dashboard';
}

function setActiveView(view: string): void {
  state.activeView = VALID_VIEWS.has(view) ? view as PcvView : 'dashboard';
}

function loadTrackedJobsFromStorage(): any[] {
  try {
    const raw = window.localStorage.getItem(JOB_HISTORY_KEY);
    if (!raw) return [];
    return asArray(JSON.parse(raw))
      .filter((job) => job && typeof job === 'object' && (job as Record<string, unknown>).job_id)
      .slice(0, JOB_HISTORY_LIMIT);
  } catch (_) {
    return [];
  }
}

function saveTrackedJobsToStorage(): void {
  try {
    const jobs = state.trackedJobs.slice(0, JOB_HISTORY_LIMIT);
    window.localStorage.setItem(JOB_HISTORY_KEY, JSON.stringify(jobs));
  } catch (_) {
    // Browser storage may be disabled; the in-memory session history still works.
  }
}

function clearTrackedJobHistory(): void {
  state.trackedJobs = [];
  try {
    window.localStorage.removeItem(JOB_HISTORY_KEY);
  } catch (_) {
    // Ignore localStorage errors; clearing memory state is still useful.
  }
  render();
}

function loadAccountSessionFromStorage(): void {
  try {
    const raw = window.sessionStorage.getItem(ACCOUNT_SESSION_KEY);
    if (!raw) return;
    const parsed = JSON.parse(raw);
    state.authAccessToken = String(parsed?.access_token || '');
    state.authRefreshToken = String(parsed?.refresh_token || '');
    state.authSession = parsed?.session || null;
  } catch (_) {
    state.authAccessToken = '';
    state.authRefreshToken = '';
    state.authSession = null;
  }
}

function saveAccountSessionToStorage(): void {
  try {
    if (!state.authAccessToken && !state.authRefreshToken) {
      window.sessionStorage.removeItem(ACCOUNT_SESSION_KEY);
      return;
    }
    window.sessionStorage.setItem(ACCOUNT_SESSION_KEY, JSON.stringify({
      access_token: state.authAccessToken,
      refresh_token: state.authRefreshToken,
      session: state.authSession
    }));
  } catch (_) {
    // Session storage can be unavailable; in-memory auth state remains active.
  }
}

function clearAccountSessionState(): void {
  state.authAccessToken = '';
  state.authRefreshToken = '';
  state.authSession = null;
  state.authRbac = null;
  state.authError = null;
  state.consoleSession = null;
  try {
    window.sessionStorage.removeItem(ACCOUNT_SESSION_KEY);
  } catch (_) {
    // Ignore sessionStorage errors; visible session state is still cleared.
  }
}

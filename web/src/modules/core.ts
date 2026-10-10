// @ts-nocheck
// Desktop Node module (ADR-0018, pcv-single-edge-frontend-structure-v1 §4): state, routes, errors, summary, table, rbac and the loaders of the legacy console (web/src/served types, state, routes, errors, summary, table, rbac, load).
// The legacy parts are kept verbatim inside one window.PCV module; their top-level declarations are exported to
// window so the parts keep the shared-scope behaviour of the legacy bundle until Task 16 retires web/src/served.
window.PCV = window.PCV || {};
(function (PCV) {
// --- legacy types.ts ---
type PcvView =
  | 'dashboard'
  | 'vms'
  | 'network'
  | 'jobs'
  | 'activity'
  | 'evidence'
  | 'troubleshooting';

type PcvConnectionState = 'idle' | 'connected' | 'degraded' | 'auth' | 'error';

interface PcvNormalizedError {
  normalized: true;
  status: number;
  operation: string;
  code: string;
  message: string;
  detail: string;
  retryable: boolean;
}

interface PcvRouteRegistry {
  opsSummary: string;
  runtimePolicy: string;
  hostStatus: string;
  networkInventory: string;
  vmList: string;
  jobList: string;
  diagnosticBundles: string;
  authLogin: string;
  authLoopbackSession: string;
  authRefresh: string;
  authLogout: string;
  authSession: string;
  authRbac: string;
  accounts: string;
  accountDisable(username: string): string;
  consoleCapabilities: string;
  jobsPage(limit?: number, offset?: number): string;
  diagnosticBundlesPage(limit?: number, offset?: number): string;
  vmDetail(vmId: string): string;
  vmBlkio(vmId: string): string;
  vmBandwidth(vmId: string): string;
  vmMemoryStats(vmId: string): string;
  vmCpuStats(vmId: string): string;
  vmQosStoragePreview(vmId: string): string;
  vmQosStorage(vmId: string): string;
  vmQosNetworkPreview(vmId: string): string;
  vmQosNetwork(vmId: string): string;
  vmGuestAgentStatus(vmId: string): string;
  vmGuestAgentPing(vmId: string): string;
  vmGuestExec(vmId: string): string;
  vmGuestFilePreview(vmId: string): string;
  vmGuestFile(vmId: string): string;
  vmGuestChannelVerify(vmId: string): string;
  vmGuestChannelEnsure(vmId: string): string;
  vmAction(vmId: string, action: string): string;
  vmClonePreview(vmId: string): string;
  vmCheckpoints(vmId: string): string;
  vmGuestExecPreview(vmId: string): string;
  vmGuestChannelPreview(vmId: string): string;
  vmNetwork(vmId: string): string;
  vmDevices(vmId: string): string;
  vmExportPreview(vmId: string): string;
  vmExport(vmId: string): string;
  vmImportPreview(): string;
  vmImport(): string;
  vmCheckpointSchedulePreview(vmId: string): string;
  vmCheckpointSchedule(vmId: string): string;
  vmCheckpointScheduleClear(vmId: string): string;
  checkpointDetail(vmId: string, checkpointId: string): string;
  checkpointAction(vmId: string, checkpointId: string, action: string): string;
  jobDetail(jobId: string): string;
  jobAction(jobId: string, action: string): string;
  diagnosticBundleDownload(bundleId: string): string;
  vmConsole(vmId: string): string;
  vmConsoleFrame(vmId: string, size: string): string;
  vmConsoleInput(vmId: string): string;
}

interface PcvRouteCoverageItem {
  id: string;
  featureId: string;
  method: string;
  route: string;
  view: PcvView | 'service';
  mutating: boolean;
  tokenRequired: boolean;
}

interface PcvDesktopApi {
  getHostStatus(options?: RequestInit): Promise<any>;
  listVms(options?: RequestInit): Promise<any>;
  getNetworkInventory(options?: RequestInit): Promise<any>;
  getRuntimePolicy(options?: RequestInit): Promise<any>;
  getOpsSummary(options?: RequestInit): Promise<any>;
  listJobs(limit?: number, offset?: number, options?: RequestInit): Promise<any>;
  getVm(vmId: string, options?: RequestInit): Promise<any>;
  getVmBlkio(vmId: string, options?: RequestInit): Promise<any>;
  getVmBandwidth(vmId: string, options?: RequestInit): Promise<any>;
  getVmMemoryStats(vmId: string, options?: RequestInit): Promise<any>;
  getVmCpuStats(vmId: string, options?: RequestInit): Promise<any>;
  previewVmQosStorage(vmId: string, payload: Record<string, unknown>): Promise<any>;
  applyVmQosStorage(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmQosNetwork(vmId: string, payload: Record<string, unknown>): Promise<any>;
  applyVmQosNetwork(vmId: string, payload: Record<string, unknown>): Promise<any>;
  getVmGuestAgentStatus(vmId: string, options?: RequestInit): Promise<any>;
  getVmGuestAgentPing(vmId: string, options?: RequestInit): Promise<any>;
  queueVmGuestExec(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmGuestFile(vmId: string, payload: Record<string, unknown>): Promise<any>;
  queueVmGuestFile(vmId: string, payload: Record<string, unknown>): Promise<any>;
  verifyVmGuestChannel(vmId: string, payload: Record<string, unknown>): Promise<any>;
  ensureVmGuestChannel(vmId: string, payload: Record<string, unknown>): Promise<any>;
  getVmDeleteStatus(vmId: string, options?: RequestInit): Promise<any>;
  getVmCheckpoints(vmId: string, options?: RequestInit): Promise<any>;
  previewVmGuestExec(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmGuestChannel(vmId: string, payload: Record<string, unknown>): Promise<any>;
  connectVmNetwork(vmId: string, payload: Record<string, unknown>): Promise<any>;
  addVmDevice(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmExport(vmId: string, payload: Record<string, unknown>): Promise<any>;
  exportVm(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmImport(payload: Record<string, unknown>): Promise<any>;
  importVm(payload: Record<string, unknown>): Promise<any>;
  previewCheckpointSchedule(vmId: string, payload: Record<string, unknown>): Promise<any>;
  setCheckpointSchedule(vmId: string, payload: Record<string, unknown>): Promise<any>;
  clearCheckpointSchedule(vmId: string): Promise<any>;
  queueVmAction(vmId: string, action: string): Promise<any>;
  queueVmAttach(vmId: string, isoPath: string): Promise<any>;
  queueVmRename(vmId: string, newName: string): Promise<any>;
  queueVmManage(vmId: string, confirmName: string): Promise<any>;
  queueVmTemplateLock(vmId: string, confirmName: string, locked: boolean): Promise<any>;
  previewVmClone(vmId: string, payload: Record<string, unknown>): Promise<any>;
  queueVmClone(vmId: string, payload: Record<string, unknown>): Promise<any>;
  queueVmResourceMutation(vmId: string, action: string, payload: Record<string, unknown>): Promise<any>;
  createVm(payload: Record<string, unknown>): Promise<any>;
  deleteVm(vmId: string): Promise<any>;
  createCheckpoint(vmId: string, name: string): Promise<any>;
  restoreCheckpoint(vmId: string, checkpointId: string): Promise<any>;
  deleteCheckpoint(vmId: string, checkpointId: string): Promise<any>;
  getJob(jobId: string, options?: RequestInit): Promise<any>;
  cancelJob(jobId: string): Promise<any>;
  retryJob(jobId: string): Promise<any>;
  reconcileJob(jobId: string): Promise<any>;
  listDiagnosticBundles(limit?: number, offset?: number, options?: RequestInit): Promise<any>;
  createDiagnosticBundle(payload?: Record<string, unknown>): Promise<any>;
  downloadDiagnosticBundle(bundleId: string, options?: RequestInit): Promise<any>;
  loginAccount(payload: Record<string, unknown>): Promise<any>;
  createLoopbackSession(): Promise<any>;
  refreshAccount(payload: Record<string, unknown>): Promise<any>;
  logoutAccount(payload?: Record<string, unknown>): Promise<any>;
  getAccountSession(options?: RequestInit): Promise<any>;
  getAccountRbac(options?: RequestInit): Promise<any>;
  listAccounts(options?: RequestInit): Promise<any>;
  createAccount(payload: Record<string, unknown>): Promise<any>;
  disableAccount(username: string, payload: Record<string, unknown>): Promise<any>;
  getConsoleCapabilities(options?: RequestInit): Promise<any>;
  getVmConsole(vmId: string, options?: RequestInit): Promise<any>;
  getVmConsoleFrame(vmId: string, size: string, options?: RequestInit): Promise<any>;
  sendVmConsoleInput(vmId: string, payload: Record<string, unknown>): Promise<any>;
}

interface Window {
  PCV_DESKTOP_NODE_CONFIG?: {
    apiBaseUrl?: string;
  };
}

interface PcvState {
  apiBaseUrl: string;
  apiToken: string;
  authAccessToken: string;
  authRefreshToken: string;
  authSession: any;
  authRbac: any;
  authError: PcvNormalizedError | null;
  authPending: boolean;
  accountDirectory: any;
  accountDirectoryError: PcvNormalizedError | null;
  accountManagePending: boolean;
  activeView: PcvView;
  host: any;
  vms: any[];
  networkInventory: any;
  networkError: PcvNormalizedError | null;
  vmFilter: string;
  vmStateFilter: string;
  vmSort: string;
  jobFilter: string;
  jobStatusFilter: string;
  jobSort: string;
  networkFilter: string;
  commandPaletteOpen: boolean;
  commandQuery: string;
  globalSearch: string;
  shellMessage: string;
  trackedJobs: any[];
  runtimePolicy: any;
  opsSummary: any;
  serverJobs: any[];
  serverJobPage: any;
  loading: boolean;
  error: PcvNormalizedError | null;
  summaryError: PcvNormalizedError | null;
  activityError: PcvNormalizedError | null;
  partialFailures: PcvNormalizedError[];
  connectionState: PcvConnectionState;
  pollTimer: number | null;
  selectedVmId: string;
  selectedVm: any;
  selectedVmCheckpoints: any[];
  selectedVmReadbacks: any;
  selectedVmQosControl: any;
  diagnosticBundle: any;
  diagnosticBundles: any[];
  diagnosticBundlePage: any;
  diagnosticBundleDownload: any;
  diagnosticBundleError: PcvNormalizedError | null;
  consoleCapabilities: any;
  consoleSession: any;
  consoleError: PcvNormalizedError | null;
  vmConsoleFrame: any;
  pendingDiagnosticAction: string;
  lastDiagnosticAction: string;
  tokenActionMessage: string;
  actionPending: boolean;
  checkpointPending: boolean;
  pendingVmActions: Record<string, string>;
  pendingCheckpoints: Record<string, string>;
  refreshRequestId: number;
  lastRefreshedAt: number | null;
  refreshController: AbortController | null;
  jobPollDelayMs: number;
  theme: string;
  language: string;
}

declare function render(): void;
declare function findCachedVm(vmId: string): any;

// --- legacy state.ts ---
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
  accountDirectory: null,
  accountDirectoryError: null,
  accountManagePending: false,
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
  vmConsoleFrame: null,
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

// --- legacy routes.ts ---
const VALID_VIEWS: ReadonlySet<string> = new Set(['dashboard', 'vms', 'network', 'jobs', 'activity', 'evidence', 'troubleshooting']);

const DESKTOP_NODE_API_ROUTES: Readonly<PcvRouteRegistry> = Object.freeze({
  opsSummary: '/api/v1/ops/summary',
  runtimePolicy: '/api/v1/runtime/policy',
  hostStatus: '/api/v1/host/status',
  networkInventory: '/api/v1/network/inventory',
  vmList: '/api/v1/vms',
  jobList: '/api/v1/jobs',
  diagnosticBundles: '/api/v1/diagnostics/bundles',
  authLogin: '/api/v1/auth/login',
  authLoopbackSession: '/api/v1/auth/loopback-session',
  authRefresh: '/api/v1/auth/refresh',
  authLogout: '/api/v1/auth/logout',
  authSession: '/api/v1/auth/session',
  authRbac: '/api/v1/auth/rbac',
  accounts: '/api/v1/accounts',
  accountDisable: (username: string) => `/api/v1/accounts/${encodeRouteSegment(username)}/disable`,
  consoleCapabilities: '/api/v1/console/capabilities',
  jobsPage: (limit = 50, offset = 0) => `/api/v1/jobs?limit=${encodeRouteQueryValue(limit)}&offset=${encodeRouteQueryValue(offset)}`,
  diagnosticBundlesPage: (limit = 10, offset = 0) => `/api/v1/diagnostics/bundles?limit=${encodeRouteQueryValue(limit)}&offset=${encodeRouteQueryValue(offset)}`,
  vmDetail: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}`,
  vmBlkio: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/blkio`,
  vmBandwidth: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/bandwidth`,
  vmMemoryStats: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/memory-stats`,
  vmCpuStats: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/cpu-stats`,
  vmQosStoragePreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/storage/preview`,
  vmQosStorage: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/storage`,
  vmQosNetworkPreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/network/preview`,
  vmQosNetwork: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/network`,
  vmGuestAgentStatus: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest-agent/status`,
  vmGuestAgentPing: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest-agent/ping`,
  vmGuestExec: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/exec`,
  vmGuestFilePreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/file/preview`,
  vmGuestFile: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/file`,
  vmGuestChannelVerify: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/channel/verify`,
  vmGuestChannelEnsure: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/channel`,
  vmAction: (vmId: string, action: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/${requireRouteAction(action, ['start', 'shutdown', 'poweroff', 'restart', 'save', 'resume-saved', 'pause', 'resume', 'rename', 'eject', 'attach', 'delete-status', 'set-memory', 'set-vcpu', 'disk-resize', 'manage', 'clone', 'template-lock'])}`,
  vmClonePreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/clone/preview`,
  vmCheckpoints: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints`,
  vmGuestExecPreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/exec/preview`,
  vmGuestChannelPreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/channel/preview`,
  vmNetwork: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/network`,
  vmDevices: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/devices`,
  vmExportPreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/export/preview`,
  vmExport: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/export`,
  vmImportPreview: () => '/api/v1/vms/import/preview',
  vmImport: () => '/api/v1/vms/import',
  vmCheckpointSchedulePreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/schedule/preview`,
  vmCheckpointSchedule: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/schedule`,
  vmCheckpointScheduleClear: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/schedule/clear`,
  checkpointDetail: (vmId: string, checkpointId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/${encodeRouteSegment(checkpointId)}`,
  checkpointAction: (vmId: string, checkpointId: string, action: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/${encodeRouteSegment(checkpointId)}/${requireRouteAction(action, ['restore'])}`,
  jobDetail: (jobId: string) => `/api/v1/jobs/${encodeRouteSegment(jobId)}`,
  jobAction: (jobId: string, action: string) => `/api/v1/jobs/${encodeRouteSegment(jobId)}/${requireRouteAction(action, ['cancel', 'retry', 'reconcile'])}`,
  diagnosticBundleDownload: (bundleId: string) => `/api/v1/diagnostics/bundles/${encodeRouteSegment(bundleId)}/download`,
  vmConsole: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/console`,
  vmConsoleFrame: (vmId: string, size: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/console/frame/${encodeRouteSegment(size)}`,
  vmConsoleInput: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/console/input`
});

const DESKTOP_NODE_ROUTE_COVERAGE: ReadonlyArray<PcvRouteCoverageItem> = Object.freeze([
  { id: 'ops.summary', featureId: 'pcv.ops.summary', method: 'GET', route: DESKTOP_NODE_API_ROUTES.opsSummary, view: 'dashboard', mutating: false, tokenRequired: true },
  { id: 'runtime.policy', featureId: 'pcv.runtime.policy', method: 'GET', route: DESKTOP_NODE_API_ROUTES.runtimePolicy, view: 'dashboard', mutating: false, tokenRequired: true },
  { id: 'host.status', featureId: 'pcv.host.status', method: 'GET', route: DESKTOP_NODE_API_ROUTES.hostStatus, view: 'dashboard', mutating: false, tokenRequired: true },
  { id: 'network.inventory', featureId: 'pcv.network.inventory', method: 'GET', route: DESKTOP_NODE_API_ROUTES.networkInventory, view: 'network', mutating: false, tokenRequired: true },
  { id: 'vm.list', featureId: 'pcv.vm.inventory', method: 'GET', route: DESKTOP_NODE_API_ROUTES.vmList, view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.create', featureId: 'pcv.vm.create', method: 'POST', route: DESKTOP_NODE_API_ROUTES.vmList, view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.detail', featureId: 'pcv.vm.inventory', method: 'GET', route: '/api/v1/vms/{vm_id}', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.blkio-get', featureId: 'pcv.vm.qos', method: 'GET', route: '/api/v1/vms/{vm_id}/blkio', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.memory-stats', featureId: 'pcv.vm.telemetry', method: 'GET', route: '/api/v1/vms/{vm_id}/memory-stats', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.cpu-stats', featureId: 'pcv.vm.telemetry', method: 'GET', route: '/api/v1/vms/{vm_id}/cpu-stats', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'checkpoint.schedule.preview', featureId: 'pcv.checkpoint.lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/checkpoints/schedule/preview', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'checkpoint.schedule.set', featureId: 'pcv.checkpoint.lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/checkpoints/schedule', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'checkpoint.schedule.clear', featureId: 'pcv.checkpoint.lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/checkpoints/schedule/clear', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.export.preview', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/{vm_id}/export/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.export', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/{vm_id}/export', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.import.preview', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/import/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.import', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/import', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.network.connect', featureId: 'pcv.network.inventory', method: 'POST', route: '/api/v1/vms/{vm_id}/network', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.device.add', featureId: 'pcv.network.inventory', method: 'POST', route: '/api/v1/vms/{vm_id}/devices', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.exec.preview', featureId: 'pcv.vm.guest-execution', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/exec/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest.channel.preview', featureId: 'pcv.vm.guest-channel', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/channel/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.bandwidth', featureId: 'pcv.vm.qos', method: 'GET', route: '/api/v1/vms/{vm_id}/bandwidth', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.storage.preview', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/storage/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.storage.set', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/storage', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.qos.network.preview', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/network/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.network.set', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/network', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest-agent-status', featureId: 'pcv.vm.guest-service-readback', method: 'GET', route: '/api/v1/vms/{vm_id}/guest-agent/status', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest-ping', featureId: 'pcv.vm.guest-service-readback', method: 'GET', route: '/api/v1/vms/{vm_id}/guest-agent/ping', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest.exec', featureId: 'pcv.vm.guest-execution', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/exec', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.file.preview', featureId: 'pcv.vm.guest-execution', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/file/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest.file', featureId: 'pcv.vm.guest-execution', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/file', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.channel.verify', featureId: 'pcv.vm.guest-channel', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/channel/verify', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.channel.ensure', featureId: 'pcv.vm.guest-channel', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/channel', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.lifecycle', featureId: 'pcv.vm.power-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/start|shutdown|poweroff|restart', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.save', featureId: 'pcv.vm.saved-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/save', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.resume-saved', featureId: 'pcv.vm.saved-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/resume-saved', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.pause', featureId: 'pcv.vm.pause-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/pause', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.resume', featureId: 'pcv.vm.pause-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/resume', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.rename', featureId: 'pcv.vm.rename', method: 'POST', route: '/api/v1/vms/{vm_id}/rename', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.media', featureId: 'pcv.vm.media-eject', method: 'POST', route: '/api/v1/vms/{vm_id}/eject', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.media.attach', featureId: 'pcv.vm.media-attach', method: 'POST', route: '/api/v1/vms/{vm_id}/attach', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.resource-mutation', featureId: 'pcv.vm.resource-limits', method: 'POST', route: '/api/v1/vms/{vm_id}/set-memory|set-vcpu|disk-resize', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.delete-status', featureId: 'pcv.vm.delete', method: 'GET', route: '/api/v1/vms/{vm_id}/delete-status', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.manage', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/{vm_id}/manage', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.clone.preview', featureId: 'pcv.vm.clone', method: 'POST', route: '/api/v1/vms/{vm_id}/clone/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.clone', featureId: 'pcv.vm.clone', method: 'POST', route: '/api/v1/vms/{vm_id}/clone', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.template.lock', featureId: 'pcv.vm.clone', method: 'POST', route: '/api/v1/vms/{vm_id}/template-lock', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.delete', featureId: 'pcv.vm.delete', method: 'DELETE', route: '/api/v1/vms/{vm_id}', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'checkpoint.list', featureId: 'pcv.checkpoint.lifecycle', method: 'GET', route: '/api/v1/vms/{vm_id}/checkpoints', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'checkpoint.create', featureId: 'pcv.checkpoint.lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/checkpoints', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'checkpoint.restore', featureId: 'pcv.checkpoint.restore', method: 'POST', route: '/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/restore', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'checkpoint.delete', featureId: 'pcv.checkpoint.lifecycle', method: 'DELETE', route: '/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'job.list', featureId: 'pcv.job.lifecycle', method: 'GET', route: DESKTOP_NODE_API_ROUTES.jobList, view: 'jobs', mutating: false, tokenRequired: true },
  { id: 'job.detail', featureId: 'pcv.job.lifecycle', method: 'GET', route: '/api/v1/jobs/{job_id}', view: 'jobs', mutating: false, tokenRequired: true },
  { id: 'job.cancel', featureId: 'pcv.job.lifecycle', method: 'POST', route: '/api/v1/jobs/{job_id}/cancel', view: 'jobs', mutating: false, tokenRequired: true },
  { id: 'job.retry', featureId: 'pcv.job.lifecycle', method: 'POST', route: '/api/v1/jobs/{job_id}/retry', view: 'jobs', mutating: true, tokenRequired: true },
  { id: 'job.reconcile', featureId: 'pcv.job.lifecycle', method: 'POST', route: '/api/v1/jobs/{job_id}/reconcile', view: 'jobs', mutating: true, tokenRequired: true },
  { id: 'diagnostic.bundle.list', featureId: 'pcv.diagnostics.bundle', method: 'GET', route: DESKTOP_NODE_API_ROUTES.diagnosticBundles, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'diagnostic.bundle.create', featureId: 'pcv.diagnostics.bundle', method: 'POST', route: DESKTOP_NODE_API_ROUTES.diagnosticBundles, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'diagnostic.bundle.download', featureId: 'pcv.diagnostics.bundle', method: 'GET', route: '/api/v1/diagnostics/bundles/{bundle_id}/download', view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'auth.login', featureId: 'pcv.account.session', method: 'POST', route: DESKTOP_NODE_API_ROUTES.authLogin, view: 'troubleshooting', mutating: false, tokenRequired: false },
  { id: 'auth.loopback-session', featureId: 'pcv.account.session', method: 'POST', route: DESKTOP_NODE_API_ROUTES.authLoopbackSession, view: 'troubleshooting', mutating: false, tokenRequired: false },
  { id: 'auth.refresh', featureId: 'pcv.account.session', method: 'POST', route: DESKTOP_NODE_API_ROUTES.authRefresh, view: 'troubleshooting', mutating: false, tokenRequired: false },
  { id: 'auth.logout', featureId: 'pcv.account.session', method: 'POST', route: DESKTOP_NODE_API_ROUTES.authLogout, view: 'troubleshooting', mutating: false, tokenRequired: false },
  { id: 'auth.session', featureId: 'pcv.account.session', method: 'GET', route: DESKTOP_NODE_API_ROUTES.authSession, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'auth.rbac', featureId: 'pcv.account.session', method: 'GET', route: DESKTOP_NODE_API_ROUTES.authRbac, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'account.list', featureId: 'pcv.account.session', method: 'GET', route: DESKTOP_NODE_API_ROUTES.accounts, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'account.create', featureId: 'pcv.account.session', method: 'POST', route: DESKTOP_NODE_API_ROUTES.accounts, view: 'troubleshooting', mutating: true, tokenRequired: true },
  { id: 'account.disable', featureId: 'pcv.account.session', method: 'POST', route: '/api/v1/accounts/{username}/disable', view: 'troubleshooting', mutating: true, tokenRequired: true },
  { id: 'console.capabilities', featureId: 'pcv.console.capabilities', method: 'GET', route: DESKTOP_NODE_API_ROUTES.consoleCapabilities, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'console.session', featureId: 'pcv.vm.console-handoff', method: 'GET', route: '/api/v1/vms/{vm_id}/console', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'console.frame', featureId: 'pcv.vm.browser-console', method: 'GET', route: '/api/v1/vms/{vm_id}/console/frame/{size}', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'console.input', featureId: 'pcv.vm.browser-console', method: 'POST', route: '/api/v1/vms/{vm_id}/console/input', view: 'vms', mutating: true, tokenRequired: true }
]);

function encodeRouteSegment(value: unknown): string {
  return encodeURIComponent(String(value ?? ''));
}

function encodeRouteQueryValue(value: unknown): string {
  return encodeURIComponent(String(value ?? ''));
}

function requireRouteAction(action: string, allowedActions: string[]): string {
  const candidate = String(action || '');
  if (allowedActions.includes(candidate)) {
    return candidate;
  }

  throw normalizeError({
    code: 'PCV_FRONTEND_ROUTE_ACTION_INVALID',
    message: 'The Web Console route action is not supported.',
    detail: candidate || 'empty'
  });
}

// --- legacy errors.ts ---
function pcvEscapeHtml(value: unknown): string {
  return String(value ?? '-')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function asArray(value: any): any[] {
  if (Array.isArray(value)) return value;
  if (Array.isArray(value?.vms)) return value.vms;
  if (Array.isArray(value?.items)) return value.items;
  if (Array.isArray(value?.bundles)) return value.bundles;
  if (Array.isArray(value?.checkpoints)) return value.checkpoints;
  if (Array.isArray(value?.jobs)) return value.jobs;
  if (Array.isArray(value?.accounts)) return value.accounts;
  if (value && typeof value === 'object') {
    return Object.values(value).filter((item) => item && typeof item === 'object');
  }
  return [];
}

function getVmId(vm: any): string {
  return String(vm?.id || vm?.name || '');
}

function getVmName(vm: any): string {
  return String(vm?.name || vm?.id || 'Unknown VM');
}

function getVmState(vm: any): string {
  return String(vm?.state || vm?.status || '').trim();
}

function isTemplateLockedVm(vm: any): boolean {
  return Boolean(vm?.template_lock);
}

function isRunningVmState(value: unknown): boolean {
  return String(value || '').toLowerCase().includes('running');
}

function buildVmManageConfirmation(vmId: string, vm: any): string {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `Manage VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'After success this VM will pass PureCVisor managed delete.',
    'Unmanaged delete refusal remains.',
    'This queues a Hyper-V Notes managed-marker mutation.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildAccountCreateConfirmation(username: string, role: string, bootstrap: boolean): string {
  return [
    bootstrap ? `Create the first admin account ${username}?` : `Create account ${username}?`,
    `Role: ${role}`,
    bootstrap
      ? 'After success loopback session is closed. Login with this admin account.'
      : 'The account is stored in accounts.json. Password is not shown again.',
    'No default password is generated.'
  ].join('\n');
}

function buildAccountDisableConfirmation(username: string): string {
  return [
    `Disable account ${username}?`,
    'The last enabled admin cannot be disabled.',
    'Login for this username will fail after success.',
    'This does not delete the account record.'
  ].join('\n');
}

function buildVmGuestFileConfirmation(vmId: string, payload: any, preview: any): string {
  const sizeBytes = preview?.size_bytes ?? payload?.size_bytes;
  const sizeText = sizeBytes === null || sizeBytes === undefined || sizeBytes === '' ? '-' : String(sizeBytes);
  return [
    `Copy host file into VM ${vmId}?`,
    `Host path: ${payload?.host_path || '-'}`,
    `Guest path: ${payload?.guest_path || '-'}`,
    `size_bytes: ${sizeText}`,
    'This queues a host-to-guest copy with a protected credential reference.',
    'HGFS shared folders are not used. The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmTemplateLockConfirmation(vmId: string, vm: any, locked: boolean): string {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    locked ? `Lock VM ${vmName} as a template?` : `Unlock template lock on VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    locked
      ? 'After success this managed VM allows start and clone only. Unmanaged lock is blocked by PCV_VM_NOT_MANAGED_BY_PURECVISOR.'
      : 'After success this VM is no longer a start/clone-only template.',
    'This queues a Hyper-V Notes template-lock mutation.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmCloneConfirmation(vmId: string, vm: any, targetName: string, preview: any): string {
  const sourceName = getVmName(vm);
  const plannedCopyBytes = preview?.planned_copy_bytes;
  const plannedCopyBytesText = plannedCopyBytes === null || plannedCopyBytes === undefined || plannedCopyBytes === ''
    ? '-'
    : String(plannedCopyBytes);
  return [
    `Clone VM ${sourceName} to ${targetName}?`,
    `Source: ${sourceName}`,
    `VM id: ${vmId}`,
    `Target name: ${targetName}`,
    `planned_copy_bytes: ${plannedCopyBytesText}`,
    '독립 VHDX를 복사한 새 managed VM을 만든다. 소스 VM은 변경하지 않는다.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmDeleteConfirmation(vmId: string, vm: any): string {
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  return [
    `Delete VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a destructive Hyper-V host mutation.',
    'Only PureCVisor-managed VMs can be deleted; unmanaged VMs are blocked by PCV_VM_NOT_MANAGED_BY_PURECVISOR.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildVmLifecycleConfirmation(vmId: string, action: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  const vmState = getVmState(vm) || 'unknown';
  const lines = [
    `${action} VM ${vmName}?`,
    `VM id: ${vmId}`,
    `Current state: ${vmState}`,
    'This queues a Hyper-V host mutation.'
  ];
  if (action === 'save') {
    lines.push('Save is Hyper-V Saved, not pause.');
  }
  if (action === 'resume-saved') {
    lines.push('Resume saved is only valid when the current state is saved.');
  }

  lines.push('The result will appear in Tracked Jobs and Operator Activity.');
  return lines.join('\n');
}

function buildVmAttachConfirmation(vmId: string, isoPath: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  return [
    `Attach ISO to VM ${vmName}?`,
    `VM id: ${vmId}`,
    `ISO: ${isoPath}`,
    'This queues a Hyper-V DVD media mutation.',
    'The existing virtual DVD HostResource is replaced. No USB or new DVD device is created.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}

function buildCheckpointRestoreConfirmation(vmId: string, checkpointId: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  return [
    `Restore checkpoint ${checkpointId}?`,
    `VM: ${getVmName(vm)}`,
    `VM id: ${vmId}`,
    'This queues a Hyper-V checkpoint restore host mutation.',
    'Power-off-before-restore preconditions are enforced by the Local API and job result.',
    'The result will appear in Tracked Jobs and Operator Activity.'
  ].join('\n');
}

function buildCheckpointDeleteConfirmation(vmId: string, checkpointId: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  return [
    `Delete checkpoint ${checkpointId}?`,
    `VM: ${getVmName(vm)}`,
    `VM id: ${vmId}`,
    'This queues a destructive Hyper-V checkpoint host mutation.',
    'Only the selected checkpoint id is targeted.',
    'The result will appear in Tracked Jobs and Operator Activity.'
  ].join('\n');
}

function formatObjectValue(value: unknown): string {
  if (value === null || value === undefined || value === '') return '-';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

function formatErrorDetail(value: unknown): string {
  if (value === null || value === undefined || value === '') return '';
  if (typeof value !== 'object') return String(value);
  const parts = Object.entries(value as Record<string, unknown>)
    .filter(([, entry]) => entry !== null && entry !== undefined && entry !== '')
    .map(([key, entry]) => `${key}=${formatObjectValue(entry)}`);
  return parts.join(' / ') || JSON.stringify(value);
}

function flattenNamedList(value: unknown, keys: string[]): string {
  const items = asArray(value);
  if (items.length === 0) return '-';
  return items.map((item) => {
    if (!item || typeof item !== 'object') return String(item);
    const record = item as Record<string, unknown>;
    return keys
      .map((key) => record[key])
      .filter((part) => part !== null && part !== undefined && part !== '')
      .join(' / ');
  }).filter(Boolean).join(', ') || '-';
}

function formatConsoleValue(consoleInfo: any): unknown {
  if (!consoleInfo || typeof consoleInfo !== 'object') return consoleInfo;
  const parts = [];
  if (consoleInfo.type !== null && consoleInfo.type !== undefined && consoleInfo.type !== '') parts.push(consoleInfo.type);
  if (consoleInfo.available_local !== null && consoleInfo.available_local !== undefined && consoleInfo.available_local !== '') {
    parts.push(`local=${consoleInfo.available_local}`);
  }
  if (consoleInfo.mode !== null && consoleInfo.mode !== undefined && consoleInfo.mode !== '') parts.push(consoleInfo.mode);
  if (consoleInfo.available !== null && consoleInfo.available !== undefined && consoleInfo.available !== '') {
    parts.push(`available=${consoleInfo.available}`);
  }
  return parts.join(' / ') || consoleInfo;
}

function getCheckpointId(checkpoint: any): string {
  return String(checkpoint?.id || checkpoint?.name || checkpoint?.checkpoint_name || '');
}

function getCheckpointName(checkpoint: any): string {
  return String(checkpoint?.name || checkpoint?.checkpoint_name || checkpoint?.id || 'Unnamed checkpoint');
}

function formatCheckpointMeta(checkpoint: any): string {
  const parts = [
    checkpoint?.created_at || checkpoint?.creation_time || checkpoint?.created,
    checkpoint?.type,
    checkpoint?.notes
  ].filter((part) => part !== null && part !== undefined && part !== '');
  return parts.join(' / ') || '-';
}

function normalizeError(error: any): PcvNormalizedError {
  if (error?.normalized) return error;
  return {
    normalized: true,
    status: error?.status ?? 0,
    operation: error?.operation ?? 'web.request',
    code: error?.code ?? 'PCV_NETWORK_ERROR',
    message: error?.message ?? 'The Desktop Node API request failed.',
    detail: formatErrorDetail(error?.detail ?? error),
    retryable: Boolean(error?.retryable)
  };
}

function readResponseHeader(response: any, name: string): string | null {
  const headers = response?.headers;
  if (!headers) return null;
  if (typeof headers.get === 'function') {
    return headers.get(name);
  }

  return headers[name] ?? headers[name.toLowerCase()] ?? null;
}

function isProblemDetailsPayload(payload: any): boolean {
  return Boolean(
    payload &&
    typeof payload === 'object' &&
    !payload.error &&
    payload.code &&
    (payload.type || payload.title || payload.status)
  );
}

function normalizeProblemDetails(payload: any, response: Response): Partial<PcvNormalizedError> & Record<string, unknown> {
  const retryAfterSeconds = payload.retry_after_seconds ?? readResponseHeader(response, 'Retry-After');
  const metadata = [
    payload.request_id ? `request_id=${payload.request_id}` : null,
    retryAfterSeconds !== null && retryAfterSeconds !== undefined && retryAfterSeconds !== '' ? `retry_after_seconds=${retryAfterSeconds}` : null,
    payload.route_timeout_seconds !== null && payload.route_timeout_seconds !== undefined ? `route_timeout_seconds=${payload.route_timeout_seconds}` : null
  ].filter(Boolean).join(' / ');
  return {
    status: payload.status ?? response.status,
    operation: payload.operation,
    code: payload.code,
    message: payload.title || payload.message || 'The Desktop Node API request failed.',
    detail: [formatErrorDetail(payload.detail), metadata].filter(Boolean).join(' / '),
    retryable: payload.retryable
  };
}

// --- legacy summary.ts ---
function readSummaryValue(...paths) {
  const summary = state.opsSummary || {};
  for (const path of paths) {
    const value = readNested(summary, path);
    if (value !== null && value !== undefined && value !== '') return value;
  }
  return undefined;
}

function getSummaryVmCounts() {
  const vms = asArray(state.vms);
  const summaryCounts = state.opsSummary?.vm_counts || state.opsSummary?.vmCounts;
  const counts = summaryCounts || {};
  const total = Number(counts.total ?? counts.total_vms ?? vms.length);
  const runningFallback = vms.filter((vm) => isRunningVmState(vm.state || vm.status)).length;
  const running = Number(counts.running ?? counts.running_vms ?? runningFallback);
  const checkpointWarningsFallback = countVmCheckpointWarnings();
  const checkpointWarnings = Number(counts.checkpoint_warnings ?? counts.checkpointWarnings ?? checkpointWarningsFallback);
  return {
    total: Number.isFinite(total) ? total : vms.length,
    running: Number.isFinite(running) ? running : runningFallback,
    checkpoint_warnings: Number.isFinite(checkpointWarnings) ? checkpointWarnings : checkpointWarningsFallback
  };
}

function getSummaryJobCounts() {
  const rows = buildActivityRows();
  const summaryCounts = state.opsSummary?.job_counts || state.opsSummary?.jobCounts;
  if (summaryCounts) {
    const queued = Number(summaryCounts.queued ?? 0);
    const running = Number(summaryCounts.running ?? 0);
    const failed = Number(summaryCounts.failed ?? 0);
    return {
      active: (Number.isFinite(queued) ? queued : 0) + (Number.isFinite(running) ? running : 0),
      failed: Number.isFinite(failed) ? failed : 0
    };
  }
  const activeFallback = rows.filter(({ job }) => ['queued', 'running'].includes(String(job?.status || '').toLowerCase())).length;
  const failedFallback = rows.filter(({ job }) => String(job?.status || '').toLowerCase() === 'failed').length;
  return {
    active: activeFallback,
    failed: failedFallback
  };
}

function getRuntimeExposure() {
  return formatPolicyValue(
    readNested(state.opsSummary || {}, ['runtime_policy', 'network', 'current_exposure']) ||
    readNested(state.opsSummary || {}, ['runtime_policy', 'network', 'bind']) ||
    readNested(state.runtimePolicy || {}, ['network', 'current_exposure']) ||
    readNested(state.runtimePolicy || {}, ['network', 'bind']) ||
    'loopback'
  );
}

function getTokenPolicyLabel() {
  return formatPolicyValue(
    readNested(state.opsSummary || {}, ['runtime_policy', 'auth', 'token_storage']) ||
    readNested(state.opsSummary || {}, ['runtime_policy', 'token', 'storage']) ||
    readNested(state.runtimePolicy || {}, ['auth', 'token_storage']) ||
    readNested(state.runtimePolicy || {}, ['token', 'storage']) ||
    'unknown'
  );
}

// Readiness must never be inferred from absence. The old fallback keyed on
// `state.host?.supported === false`, which is also false when `state.host` is
// null, so an unauthenticated or unloaded console reported a healthy `Ready`.
// Every caller — metric grid, ops cockpit, hero chip, priority scan — depends
// on this helper, so the gate belongs here rather than at each call site.
function getHostReadinessLabel() {
  const summaryReadiness = hasRefreshedOperation('ops.summary')
    ? readSummaryValue(['host', 'readiness'], ['host', 'status'])
    : undefined;
  if (summaryReadiness) return formatPolicyValue(summaryReadiness);
  if (!hasRefreshedOperation('host.status') || !state.host) return '—';
  return formatPolicyValue(state.host.supported === false ? 'Needs attention' : 'Ready');
}

function normalizeSummaryIssue(issue, fallbackCode = 'PCV_OPS_SUMMARY_DEGRADED') {
  if (!issue || typeof issue !== 'object') {
    return {
      code: fallbackCode,
      message: String(issue || 'Ops summary returned a degraded signal.'),
      detail: ''
    };
  }
  return {
    code: issue.code || issue.key || issue.id || fallbackCode,
    message: issue.message || issue.label || issue.detail || 'Ops summary returned a degraded signal.',
    detail: issue.detail || issue.description || issue.operation || ''
  };
}

function summarySignalTone(signal) {
  return String(signal?.tone || signal?.status || signal?.severity || '').toLowerCase();
}

function summarySignalKey(signal) {
  return String(signal?.key || signal?.id || signal?.code || signal?.name || '').toLowerCase();
}

function formatErrorSummary(error) {
  return [
    `${error.code}: ${error.message}`,
    error.detail
  ].filter(Boolean).join(' / ');
}

// --- legacy table.ts ---
function normalizeSearchText(value) {
  return String(value ?? '').trim().toLowerCase();
}

function collectRowText(row, keys = []) {
  if (typeof row === 'string' || typeof row === 'number' || typeof row === 'boolean') {
    return String(row);
  }
  if (!row || typeof row !== 'object') return '';
  const record = row;
  if (keys.length > 0) {
    return keys.map((key) => formatObjectValue(readNested(record, key.split('.')))).join(' ');
  }
  return Object.values(record).map(formatObjectValue).join(' ');
}

function filterRowsByQuery(rows, query, projector) {
  const normalized = normalizeSearchText(query);
  if (!normalized) return rows;
  return rows.filter((row) => normalizeSearchText(projector ? projector(row) : collectRowText(row)).includes(normalized));
}

function sortRowsByKey(rows, sort, selectors = {}) {
  const [rawKey, rawDirection] = String(sort || '').split(':');
  const key = rawKey || 'name';
  const direction = rawDirection === 'desc' ? -1 : 1;
  const selector = selectors[key] || ((row) => row?.[key]);
  return [...rows].sort((left, right) => {
    const leftValue = selector(left);
    const rightValue = selector(right);
    const leftNumber = Number(leftValue);
    const rightNumber = Number(rightValue);
    if (Number.isFinite(leftNumber) && Number.isFinite(rightNumber)) {
      return (leftNumber - rightNumber) * direction;
    }
    return String(leftValue ?? '').localeCompare(String(rightValue ?? ''), undefined, { numeric: true, sensitivity: 'base' }) * direction;
  });
}

function renderTableStateSummary(label, shown, total, query = '', extra = '') {
  const filterText = query ? ` / filter=${query}` : '';
  return `<div class="table-state-summary"><strong>${pcvEscapeHtml(label)}</strong> ${pcvEscapeHtml(shown)} shown of ${pcvEscapeHtml(total)}${pcvEscapeHtml(filterText)}${extra ? ` / ${pcvEscapeHtml(extra)}` : ''}</div>`;
}

function getVmActionKey(vmId) {
  return String(vmId || '');
}

function isVmActionPending(vmId, action = '') {
  const pending = state.pendingVmActions[getVmActionKey(vmId)];
  return Boolean(pending && (!action || pending === action));
}

function setVmActionPending(vmId, action) {
  state.pendingVmActions[getVmActionKey(vmId)] = action;
}

function clearVmActionPending(vmId) {
  delete state.pendingVmActions[getVmActionKey(vmId)];
}

function getCheckpointActionKey(vmId, checkpointId = 'create') {
  return `${vmId || ''}:${checkpointId || 'create'}`;
}

function isCheckpointActionPending(vmId, checkpointId = 'create') {
  return Boolean(state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)]);
}

function setCheckpointActionPending(vmId, checkpointId, action) {
  state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)] = action;
}

function clearCheckpointActionPending(vmId, checkpointId = 'create') {
  delete state.pendingCheckpoints[getCheckpointActionKey(vmId, checkpointId)];
}

function bindJobAndNetworkFilterEvents() {
  els.jobFilter?.addEventListener('input', () => {
    state.jobFilter = els.jobFilter.value;
    render();
  });
  els.jobStatusFilter?.addEventListener('change', () => {
    state.jobStatusFilter = els.jobStatusFilter.value || 'all';
    render();
  });
  els.jobSort?.addEventListener('change', () => {
    state.jobSort = els.jobSort.value || 'updated:desc';
    render();
  });
  els.networkFilter?.addEventListener('input', () => {
    state.networkFilter = els.networkFilter.value;
    render();
  });
}

// --- legacy rbac.ts ---
function isAuthError(error) {
  return ['PCV_AUTH_REQUIRED', 'PCV_AUTH_FORBIDDEN', 'PCV_LOGIN_FAILED', 'PCV_JWT_INVALID', 'PCV_JWT_EXPIRED', 'PCV_RBAC_FORBIDDEN'].includes(error?.code);
}

function tokenRequiredRouteStatus(error = state.error) {
  if (isAuthError(error)) {
    return 'token-required route rejected the browser token';
  }
  if (state.authAccessToken.trim()) {
    return `account JWT session active as ${getAccountRoleLabel()}`;
  }
  if (!state.apiToken.trim()) {
    return 'token-required routes may show Auth required';
  }
  return 'browser token present for token-required routes';
}

function getAccountRoleLabel() {
  return state.authSession?.role || state.authSession?.account?.role || 'unauthenticated';
}

function getAccountPermissions() {
  return asArray(state.authSession?.permissions || state.authSession?.account?.permissions)
    .map((permission) => String(permission || '').trim())
    .filter(Boolean);
}

function accountRbacModeEnabled() {
  const mode = String(readNested(state.runtimePolicy || {}, ['auth', 'mode']) || '').toLowerCase();
  const rbac = readNested(state.runtimePolicy || {}, ['auth', 'rbac']);
  return Boolean(state.authAccessToken || rbac === true || mode.includes('account'));
}

function rbacAllows(permission) {
  if (!accountRbacModeEnabled()) return true;
  const permissions = getAccountPermissions();
  return permissions.includes('*') || permissions.includes(permission);
}

function isAccountBootstrapOpen() {
  const mode = String(readNested(state.runtimePolicy || {}, ['auth', 'mode']) || '').toLowerCase();
  return mode.includes('not_configured') ||
    String(state.accountDirectory?.bootstrap_state || '') === 'no-default-account';
}

function canManageAccounts() {
  return rbacAllows('account.manage');
}

function requireRbac(permission, actionLabel = 'this action') {
  if (rbacAllows(permission)) return;
  throw normalizeError({
    code: 'PCV_RBAC_FORBIDDEN',
    message: `The current account role cannot use ${actionLabel}.`,
    detail: `Required permission: ${permission}. Current role: ${getAccountRoleLabel()}.`
  });
}

function applyAccountSessionPayload(payload) {
  state.authAccessToken = String(payload?.access_token || state.authAccessToken || '');
  state.authRefreshToken = String(payload?.refresh_token || state.authRefreshToken || '');
  state.authSession = payload?.session || state.authSession || null;
  state.authError = null;
  saveAccountSessionToStorage();
}

// --- legacy load.ts ---
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

async function loadAccountDirectory(options = {}) {
  if (!state.authAccessToken.trim() && !state.apiToken.trim()) {
    state.accountDirectory = null;
    state.accountDirectoryError = null;
    return;
  }

  try {
    state.accountDirectory = await desktopApi.listAccounts(options);
    state.accountDirectoryError = null;
  } catch (error) {
    state.accountDirectoryError = normalizeError(error);
    state.accountDirectory = null;
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
    ['guest_ping', () => desktopApi.getVmGuestAgentPing(vmId, requestOptions)],
    ['memory_stats', () => desktopApi.getVmMemoryStats(vmId, requestOptions)],
    ['cpu_stats', () => desktopApi.getVmCpuStats(vmId, requestOptions)]
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

// exports
window.resolveInitialApiBaseUrl = resolveInitialApiBaseUrl;
window.byId = byId;
window.getHashView = getHashView;
window.setActiveView = setActiveView;
window.loadTrackedJobsFromStorage = loadTrackedJobsFromStorage;
window.saveTrackedJobsToStorage = saveTrackedJobsToStorage;
window.clearTrackedJobHistory = clearTrackedJobHistory;
window.loadAccountSessionFromStorage = loadAccountSessionFromStorage;
window.saveAccountSessionToStorage = saveAccountSessionToStorage;
window.clearAccountSessionState = clearAccountSessionState;
window.WEB_ASSET_LABEL = WEB_ASSET_LABEL;
window.state = state;
window.JOB_HISTORY_KEY = JOB_HISTORY_KEY;
window.ACCOUNT_SESSION_KEY = ACCOUNT_SESSION_KEY;
window.JOB_HISTORY_LIMIT = JOB_HISTORY_LIMIT;
window.DIAGNOSTIC_BUNDLE_ROOT = DIAGNOSTIC_BUNDLE_ROOT;
window.TOKEN_PROTECTED_FILE = TOKEN_PROTECTED_FILE;
window.els = els;
window.encodeRouteSegment = encodeRouteSegment;
window.encodeRouteQueryValue = encodeRouteQueryValue;
window.requireRouteAction = requireRouteAction;
window.VALID_VIEWS = VALID_VIEWS;
window.DESKTOP_NODE_API_ROUTES = DESKTOP_NODE_API_ROUTES;
window.DESKTOP_NODE_ROUTE_COVERAGE = DESKTOP_NODE_ROUTE_COVERAGE;
window.pcvEscapeHtml = pcvEscapeHtml;
window.asArray = asArray;
window.getVmId = getVmId;
window.getVmName = getVmName;
window.getVmState = getVmState;
window.isTemplateLockedVm = isTemplateLockedVm;
window.isRunningVmState = isRunningVmState;
window.buildVmManageConfirmation = buildVmManageConfirmation;
window.buildAccountCreateConfirmation = buildAccountCreateConfirmation;
window.buildAccountDisableConfirmation = buildAccountDisableConfirmation;
window.buildVmGuestFileConfirmation = buildVmGuestFileConfirmation;
window.buildVmTemplateLockConfirmation = buildVmTemplateLockConfirmation;
window.buildVmCloneConfirmation = buildVmCloneConfirmation;
window.buildVmDeleteConfirmation = buildVmDeleteConfirmation;
window.buildVmLifecycleConfirmation = buildVmLifecycleConfirmation;
window.buildVmAttachConfirmation = buildVmAttachConfirmation;
window.buildCheckpointRestoreConfirmation = buildCheckpointRestoreConfirmation;
window.buildCheckpointDeleteConfirmation = buildCheckpointDeleteConfirmation;
window.formatObjectValue = formatObjectValue;
window.formatErrorDetail = formatErrorDetail;
window.flattenNamedList = flattenNamedList;
window.formatConsoleValue = formatConsoleValue;
window.getCheckpointId = getCheckpointId;
window.getCheckpointName = getCheckpointName;
window.formatCheckpointMeta = formatCheckpointMeta;
window.normalizeError = normalizeError;
window.readResponseHeader = readResponseHeader;
window.isProblemDetailsPayload = isProblemDetailsPayload;
window.normalizeProblemDetails = normalizeProblemDetails;
window.readSummaryValue = readSummaryValue;
window.getSummaryVmCounts = getSummaryVmCounts;
window.getSummaryJobCounts = getSummaryJobCounts;
window.getRuntimeExposure = getRuntimeExposure;
window.getTokenPolicyLabel = getTokenPolicyLabel;
window.getHostReadinessLabel = getHostReadinessLabel;
window.normalizeSummaryIssue = normalizeSummaryIssue;
window.summarySignalTone = summarySignalTone;
window.summarySignalKey = summarySignalKey;
window.formatErrorSummary = formatErrorSummary;
window.normalizeSearchText = normalizeSearchText;
window.collectRowText = collectRowText;
window.filterRowsByQuery = filterRowsByQuery;
window.sortRowsByKey = sortRowsByKey;
window.renderTableStateSummary = renderTableStateSummary;
window.getVmActionKey = getVmActionKey;
window.isVmActionPending = isVmActionPending;
window.setVmActionPending = setVmActionPending;
window.clearVmActionPending = clearVmActionPending;
window.getCheckpointActionKey = getCheckpointActionKey;
window.isCheckpointActionPending = isCheckpointActionPending;
window.setCheckpointActionPending = setCheckpointActionPending;
window.clearCheckpointActionPending = clearCheckpointActionPending;
window.bindJobAndNetworkFilterEvents = bindJobAndNetworkFilterEvents;
window.isAuthError = isAuthError;
window.tokenRequiredRouteStatus = tokenRequiredRouteStatus;
window.getAccountRoleLabel = getAccountRoleLabel;
window.getAccountPermissions = getAccountPermissions;
window.accountRbacModeEnabled = accountRbacModeEnabled;
window.rbacAllows = rbacAllows;
window.isAccountBootstrapOpen = isAccountBootstrapOpen;
window.canManageAccounts = canManageAccounts;
window.requireRbac = requireRbac;
window.applyAccountSessionPayload = applyAccountSessionPayload;
window.loadHost = loadHost;
window.loadVms = loadVms;
window.loadNetworkInventory = loadNetworkInventory;
window.loadRuntimePolicy = loadRuntimePolicy;
window.loadAccountSession = loadAccountSession;
window.loadAccountDirectory = loadAccountDirectory;
window.loadConsoleCapabilities = loadConsoleCapabilities;
window.loadOpsSummary = loadOpsSummary;
window.loadServerJobs = loadServerJobs;
window.loadDiagnosticBundleList = loadDiagnosticBundleList;
window.loadNextDiagnosticBundlePage = loadNextDiagnosticBundlePage;
window.findCachedVm = findCachedVm;
window.reconcileSelectedVm = reconcileSelectedVm;
window.emptyVmReadbackState = emptyVmReadbackState;
window.loadVmQosGuestReadbacks = loadVmQosGuestReadbacks;
window.loadVmDetail = loadVmDetail;
window.selectVmFromShell = selectVmFromShell;
window.loadCheckpoints = loadCheckpoints;
window.refreshSelectedVm = refreshSelectedVm;
PCV.core = Object.assign(PCV.core || {}, { resolveInitialApiBaseUrl, byId, getHashView, setActiveView, loadTrackedJobsFromStorage, saveTrackedJobsToStorage, clearTrackedJobHistory, loadAccountSessionFromStorage, saveAccountSessionToStorage, clearAccountSessionState, WEB_ASSET_LABEL, state, JOB_HISTORY_KEY, ACCOUNT_SESSION_KEY, JOB_HISTORY_LIMIT, DIAGNOSTIC_BUNDLE_ROOT, TOKEN_PROTECTED_FILE, els, encodeRouteSegment, encodeRouteQueryValue, requireRouteAction, VALID_VIEWS, DESKTOP_NODE_API_ROUTES, DESKTOP_NODE_ROUTE_COVERAGE, pcvEscapeHtml, asArray, getVmId, getVmName, getVmState, isTemplateLockedVm, isRunningVmState, buildVmManageConfirmation, buildAccountCreateConfirmation, buildAccountDisableConfirmation, buildVmGuestFileConfirmation, buildVmTemplateLockConfirmation, buildVmCloneConfirmation, buildVmDeleteConfirmation, buildVmLifecycleConfirmation, buildVmAttachConfirmation, buildCheckpointRestoreConfirmation, buildCheckpointDeleteConfirmation, formatObjectValue, formatErrorDetail, flattenNamedList, formatConsoleValue, getCheckpointId, getCheckpointName, formatCheckpointMeta, normalizeError, readResponseHeader, isProblemDetailsPayload, normalizeProblemDetails, readSummaryValue, getSummaryVmCounts, getSummaryJobCounts, getRuntimeExposure, getTokenPolicyLabel, getHostReadinessLabel, normalizeSummaryIssue, summarySignalTone, summarySignalKey, formatErrorSummary, normalizeSearchText, collectRowText, filterRowsByQuery, sortRowsByKey, renderTableStateSummary, getVmActionKey, isVmActionPending, setVmActionPending, clearVmActionPending, getCheckpointActionKey, isCheckpointActionPending, setCheckpointActionPending, clearCheckpointActionPending, bindJobAndNetworkFilterEvents, isAuthError, tokenRequiredRouteStatus, getAccountRoleLabel, getAccountPermissions, accountRbacModeEnabled, rbacAllows, isAccountBootstrapOpen, canManageAccounts, requireRbac, applyAccountSessionPayload, loadHost, loadVms, loadNetworkInventory, loadRuntimePolicy, loadAccountSession, loadAccountDirectory, loadConsoleCapabilities, loadOpsSummary, loadServerJobs, loadDiagnosticBundleList, loadNextDiagnosticBundlePage, findCachedVm, reconcileSelectedVm, emptyVmReadbackState, loadVmQosGuestReadbacks, loadVmDetail, selectVmFromShell, loadCheckpoints, refreshSelectedVm });
})(window.PCV);

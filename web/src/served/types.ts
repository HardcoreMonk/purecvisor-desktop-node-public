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
  consoleCapabilities: string;
  jobsPage(limit?: number, offset?: number): string;
  diagnosticBundlesPage(limit?: number, offset?: number): string;
  vmDetail(vmId: string): string;
  vmBlkio(vmId: string): string;
  vmBandwidth(vmId: string): string;
  vmQosStoragePreview(vmId: string): string;
  vmQosStorage(vmId: string): string;
  vmQosNetworkPreview(vmId: string): string;
  vmQosNetwork(vmId: string): string;
  vmGuestAgentStatus(vmId: string): string;
  vmGuestAgentPing(vmId: string): string;
  vmGuestExec(vmId: string): string;
  vmGuestChannelVerify(vmId: string): string;
  vmGuestChannelEnsure(vmId: string): string;
  vmAction(vmId: string, action: string): string;
  vmClonePreview(vmId: string): string;
  vmCheckpoints(vmId: string): string;
  checkpointDetail(vmId: string, checkpointId: string): string;
  checkpointAction(vmId: string, checkpointId: string, action: string): string;
  jobDetail(jobId: string): string;
  jobAction(jobId: string, action: string): string;
  diagnosticBundleDownload(bundleId: string): string;
  vmConsole(vmId: string): string;
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
  previewVmQosStorage(vmId: string, payload: Record<string, unknown>): Promise<any>;
  applyVmQosStorage(vmId: string, payload: Record<string, unknown>): Promise<any>;
  previewVmQosNetwork(vmId: string, payload: Record<string, unknown>): Promise<any>;
  applyVmQosNetwork(vmId: string, payload: Record<string, unknown>): Promise<any>;
  getVmGuestAgentStatus(vmId: string, options?: RequestInit): Promise<any>;
  getVmGuestAgentPing(vmId: string, options?: RequestInit): Promise<any>;
  queueVmGuestExec(vmId: string, payload: Record<string, unknown>): Promise<any>;
  verifyVmGuestChannel(vmId: string, payload: Record<string, unknown>): Promise<any>;
  ensureVmGuestChannel(vmId: string, payload: Record<string, unknown>): Promise<any>;
  getVmDeleteStatus(vmId: string, options?: RequestInit): Promise<any>;
  getVmCheckpoints(vmId: string, options?: RequestInit): Promise<any>;
  queueVmAction(vmId: string, action: string): Promise<any>;
  queueVmAttach(vmId: string, isoPath: string): Promise<any>;
  queueVmManage(vmId: string, confirmName: string): Promise<any>;
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
  getConsoleCapabilities(options?: RequestInit): Promise<any>;
  getVmConsole(vmId: string, options?: RequestInit): Promise<any>;
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

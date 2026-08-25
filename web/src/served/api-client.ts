function mergeRequestOptions(baseOptions: RequestInit, options: RequestInit = {}): RequestInit {
  const headers = new Headers(baseOptions.headers || {});
  const optionHeaders = new Headers(options.headers || {});
  optionHeaders.forEach((value, key) => headers.set(key, value));
  return { ...baseOptions, ...options, headers };
}

async function apiFetch(path: string, options: RequestInit = {}): Promise<any> {
  const response = await apiRequest(path, options);

  let payload: any;
  try {
    payload = await response.json();
  } catch (error) {
    const caught = error as Error;
    throw normalizeError({
      status: response.status,
      code: 'PCV_RESPONSE_INVALID',
      message: 'The API returned a malformed response.',
      detail: caught.message
    });
  }

  if (!response.ok || payload.ok === false) {
    throw normalizeApiResponseError(response, payload);
  }

  return unwrapApiEnvelope(payload);
}

function unwrapApiEnvelope(payload: any): any {
  if (!payload || typeof payload !== 'object') return payload;
  if (Object.prototype.hasOwnProperty.call(payload, 'data')) return payload.data;
  return payload;
}

function unwrapApiList(payload: any): any[] {
  return asArray(unwrapApiEnvelope(payload));
}

async function apiRequest(path: string, options: RequestInit = {}): Promise<Response> {
  const base = state.apiBaseUrl.replace(/\/$/, '');
  const requestOptions = options as RequestInit & { skipAuth?: boolean };
  const headers = new Headers(requestOptions.headers || {});
  headers.set('Accept', headers.get('Accept') || 'application/json');
  const accountToken = state.authAccessToken.trim();
  const serviceToken = state.apiToken.trim();
  if (!requestOptions.skipAuth && (accountToken || serviceToken)) {
    headers.set('Authorization', `Bearer ${accountToken || serviceToken}`);
  }
  if (requestOptions.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }
  const { skipAuth: _skipAuth, ...fetchOptions } = requestOptions;

  let response: Response;
  try {
    response = await fetch(`${base}${path}`, { ...fetchOptions, headers });
  } catch (error) {
    const caught = error as Error;
    throw normalizeError({
      code: caught.name === 'AbortError' ? 'PCV_REQUEST_ABORTED' : 'PCV_NETWORK_ERROR',
      message: caught.name === 'AbortError' ? 'The Web Console request was superseded.' : 'Network request failed.',
      detail: caught.message
    });
  }

  return response;
}

function normalizeApiResponseError(response: Response, payload: any): PcvNormalizedError {
  if (isProblemDetailsPayload(payload)) {
    return normalizeError(normalizeProblemDetails(payload, response));
  }

  const apiError = payload?.error || {};
  return normalizeError({
    status: response.status,
    operation: payload?.operation,
    code: apiError.code,
    message: apiError.message,
    detail: apiError.detail,
    retryable: apiError.retryable
  });
}

function readDownloadFileName(response: Response, bundleId: string): string {
  const disposition = readResponseHeader(response, 'Content-Disposition') || '';
  const match = disposition.match(/filename="?([^";]+)"?/i);
  return match?.[1] || `${bundleId}.bundle.json`;
}

async function apiDownload(path: string, bundleId: string, options: RequestInit = {}): Promise<any> {
  const response = await apiRequest(path, mergeRequestOptions({
    method: 'GET',
    headers: { Accept: 'application/vnd.purecvisor.diagnostic-bundle+json, application/json' }
  }, options));

  if (!response.ok) {
    let payload: any = null;
    try {
      payload = await response.json();
    } catch (_) {
      throw normalizeError({
        status: response.status,
        operation: 'diagnostic.bundle.download',
        code: 'PCV_DIAGNOSTIC_BUNDLE_DOWNLOAD_FAILED',
        message: 'Diagnostic bundle download failed.',
        detail: `HTTP ${response.status}`,
        retryable: false
      });
    }
    throw normalizeApiResponseError(response, payload);
  }

  const contentType = readResponseHeader(response, 'Content-Type') || 'application/vnd.purecvisor.diagnostic-bundle+json';
  const bundleHeader = readResponseHeader(response, 'X-PCV-Diagnostic-Bundle-Id') || bundleId;
  const fileName = readDownloadFileName(response, bundleHeader);
  let body = '';
  if (typeof response.text === 'function') {
    body = await response.text();
  }

  return {
    bundle_id: bundleHeader,
    content_type: contentType,
    file_name: fileName,
    size_bytes: body.length,
    body
  };
}

const desktopApi: Readonly<PcvDesktopApi> = Object.freeze({
  getHostStatus: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.hostStatus, options),
  listVms: async (options = {}) => unwrapApiList(await apiFetch(DESKTOP_NODE_API_ROUTES.vmList, options)),
  getNetworkInventory: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.networkInventory, options),
  getRuntimePolicy: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.runtimePolicy, options),
  getOpsSummary: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.opsSummary, options),
  listJobs: (limit = 50, offset = 0, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.jobsPage(limit, offset), options),
  getVm: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmDetail(vmId), options),
  getVmBlkio: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmBlkio(vmId), options),
  getVmBandwidth: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmBandwidth(vmId), options),
  previewVmQosStorage: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmQosStoragePreview(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  applyVmQosStorage: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmQosStorage(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  previewVmQosNetwork: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmQosNetworkPreview(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  applyVmQosNetwork: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmQosNetwork(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  getVmGuestAgentStatus: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmGuestAgentStatus(vmId), options),
  getVmGuestAgentPing: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmGuestAgentPing(vmId), options),
  queueVmGuestExec: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmGuestExec(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  verifyVmGuestChannel: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmGuestChannelVerify(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  ensureVmGuestChannel: (vmId: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmGuestChannelEnsure(vmId), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  getVmDeleteStatus: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, 'delete-status'), options),
  getVmCheckpoints: async (vmId: string, options = {}) => unwrapApiList(await apiFetch(DESKTOP_NODE_API_ROUTES.vmCheckpoints(vmId), options)),
  queueVmAction: (vmId: string, action: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, action), { method: 'POST' }),
  queueVmAttach: (vmId: string, isoPath: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, 'attach'), {
    method: 'POST',
    body: JSON.stringify({ iso_path: isoPath })
  }),
  queueVmManage: (vmId: string, confirmName: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, 'manage'), {
    method: 'POST',
    body: JSON.stringify({ confirm_name: confirmName })
  }),
  queueVmResourceMutation: (vmId: string, action: string, payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, action), {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  createVm: (payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.vmList, { method: 'POST', body: JSON.stringify(payload) }),
  deleteVm: (vmId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmDetail(vmId), { method: 'DELETE' }),
  createCheckpoint: (vmId: string, name: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmCheckpoints(vmId), {
    method: 'POST',
    body: JSON.stringify({ name })
  }),
  restoreCheckpoint: (vmId: string, checkpointId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.checkpointAction(vmId, checkpointId, 'restore'), { method: 'POST' }),
  deleteCheckpoint: (vmId: string, checkpointId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.checkpointDetail(vmId, checkpointId), { method: 'DELETE' }),
  getJob: (jobId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.jobDetail(jobId), options),
  cancelJob: (jobId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.jobAction(jobId, 'cancel'), { method: 'POST' }),
  retryJob: (jobId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.jobAction(jobId, 'retry'), { method: 'POST' }),
  reconcileJob: (jobId: string) => apiFetch(DESKTOP_NODE_API_ROUTES.jobAction(jobId, 'reconcile'), { method: 'POST' }),
  listDiagnosticBundles: (limit = 10, offset = 0, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.diagnosticBundlesPage(limit, offset), options),
  createDiagnosticBundle: (payload = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.diagnosticBundles, {
    method: 'POST',
    body: JSON.stringify(payload)
  }),
  downloadDiagnosticBundle: (bundleId: string, options = {}) => apiDownload(DESKTOP_NODE_API_ROUTES.diagnosticBundleDownload(bundleId), bundleId, options),
  loginAccount: (payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.authLogin, {
    method: 'POST',
    body: JSON.stringify(payload),
    skipAuth: true
  } as RequestInit & { skipAuth: boolean }),
  createLoopbackSession: () => apiFetch(DESKTOP_NODE_API_ROUTES.authLoopbackSession, {
    method: 'POST',
    skipAuth: true
  } as RequestInit & { skipAuth: boolean }),
  refreshAccount: (payload: Record<string, unknown>) => apiFetch(DESKTOP_NODE_API_ROUTES.authRefresh, {
    method: 'POST',
    body: JSON.stringify(payload),
    skipAuth: true
  } as RequestInit & { skipAuth: boolean }),
  logoutAccount: (payload = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.authLogout, {
    method: 'POST',
    body: JSON.stringify(payload),
    skipAuth: true
  } as RequestInit & { skipAuth: boolean }),
  getAccountSession: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.authSession, options),
  getAccountRbac: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.authRbac, options),
  getConsoleCapabilities: (options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.consoleCapabilities, options),
  getVmConsole: (vmId: string, options = {}) => apiFetch(DESKTOP_NODE_API_ROUTES.vmConsole(vmId), options)
});

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
  consoleCapabilities: '/api/v1/console/capabilities',
  jobsPage: (limit = 50, offset = 0) => `/api/v1/jobs?limit=${encodeRouteQueryValue(limit)}&offset=${encodeRouteQueryValue(offset)}`,
  diagnosticBundlesPage: (limit = 10, offset = 0) => `/api/v1/diagnostics/bundles?limit=${encodeRouteQueryValue(limit)}&offset=${encodeRouteQueryValue(offset)}`,
  vmDetail: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}`,
  vmBlkio: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/blkio`,
  vmBandwidth: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/bandwidth`,
  vmQosStoragePreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/storage/preview`,
  vmQosStorage: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/storage`,
  vmQosNetworkPreview: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/network/preview`,
  vmQosNetwork: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/qos/network`,
  vmGuestAgentStatus: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest-agent/status`,
  vmGuestAgentPing: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest-agent/ping`,
  vmGuestExec: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/exec`,
  vmGuestChannelVerify: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/channel/verify`,
  vmGuestChannelEnsure: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/guest/channel`,
  vmAction: (vmId: string, action: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/${requireRouteAction(action, ['start', 'shutdown', 'poweroff', 'restart', 'save', 'resume-saved', 'eject', 'attach', 'delete-status', 'set-memory', 'set-vcpu', 'disk-resize', 'manage'])}`,
  vmCheckpoints: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints`,
  checkpointDetail: (vmId: string, checkpointId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/${encodeRouteSegment(checkpointId)}`,
  checkpointAction: (vmId: string, checkpointId: string, action: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/checkpoints/${encodeRouteSegment(checkpointId)}/${requireRouteAction(action, ['restore'])}`,
  jobDetail: (jobId: string) => `/api/v1/jobs/${encodeRouteSegment(jobId)}`,
  jobAction: (jobId: string, action: string) => `/api/v1/jobs/${encodeRouteSegment(jobId)}/${requireRouteAction(action, ['cancel', 'retry', 'reconcile'])}`,
  diagnosticBundleDownload: (bundleId: string) => `/api/v1/diagnostics/bundles/${encodeRouteSegment(bundleId)}/download`,
  vmConsole: (vmId: string) => `/api/v1/vms/${encodeRouteSegment(vmId)}/console`
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
  { id: 'vm.bandwidth', featureId: 'pcv.vm.qos', method: 'GET', route: '/api/v1/vms/{vm_id}/bandwidth', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.storage.preview', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/storage/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.storage.set', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/storage', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.qos.network.preview', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/network/preview', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.qos.network.set', featureId: 'pcv.vm.qos', method: 'POST', route: '/api/v1/vms/{vm_id}/qos/network', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest-agent-status', featureId: 'pcv.vm.guest-service-readback', method: 'GET', route: '/api/v1/vms/{vm_id}/guest-agent/status', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest-ping', featureId: 'pcv.vm.guest-service-readback', method: 'GET', route: '/api/v1/vms/{vm_id}/guest-agent/ping', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.guest.exec', featureId: 'pcv.vm.guest-execution', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/exec', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.channel.verify', featureId: 'pcv.vm.guest-channel', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/channel/verify', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.guest.channel.ensure', featureId: 'pcv.vm.guest-channel', method: 'POST', route: '/api/v1/vms/{vm_id}/guest/channel', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.lifecycle', featureId: 'pcv.vm.power-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/start|shutdown|poweroff|restart', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.save', featureId: 'pcv.vm.saved-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/save', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.resume-saved', featureId: 'pcv.vm.saved-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/resume-saved', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.media', featureId: 'pcv.vm.media-eject', method: 'POST', route: '/api/v1/vms/{vm_id}/eject', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.media.attach', featureId: 'pcv.vm.media-attach', method: 'POST', route: '/api/v1/vms/{vm_id}/attach', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.resource-mutation', featureId: 'pcv.vm.resource-limits', method: 'POST', route: '/api/v1/vms/{vm_id}/set-memory|set-vcpu|disk-resize', view: 'vms', mutating: true, tokenRequired: true },
  { id: 'vm.delete-status', featureId: 'pcv.vm.delete', method: 'GET', route: '/api/v1/vms/{vm_id}/delete-status', view: 'vms', mutating: false, tokenRequired: true },
  { id: 'vm.manage', featureId: 'pcv.vm.managed-import', method: 'POST', route: '/api/v1/vms/{vm_id}/manage', view: 'vms', mutating: true, tokenRequired: true },
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
  { id: 'console.capabilities', featureId: 'pcv.console.capabilities', method: 'GET', route: DESKTOP_NODE_API_ROUTES.consoleCapabilities, view: 'troubleshooting', mutating: false, tokenRequired: true },
  { id: 'console.session', featureId: 'pcv.vm.console-handoff', method: 'GET', route: '/api/v1/vms/{vm_id}/console', view: 'vms', mutating: false, tokenRequired: true }
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

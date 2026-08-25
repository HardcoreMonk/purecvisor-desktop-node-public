import type {
  HostStatusResponse,
  JobListResponse,
  NetworkInventoryResponse,
  OpsSummaryResponse,
  RuntimePolicyResponse,
  VirtualMachineListResponse
} from "./api-types";
import { buildDashboardViewModel } from "./view-model";

export const phase25WebConsoleScaffold = {
  decisionCandidate: "static-asset-parity-scaffold-first",
  runtimeReplacement: "default",
  servedAsset: "app.js",
  servedTypeScriptEntry: "src/served-app.ts"
} as const;

export const localApiRoutes = {
  opsSummary: "/api/v1/ops/summary",
  runtimePolicy: "/api/v1/runtime/policy",
  hostStatus: "/api/v1/host/status",
  networkInventory: "/api/v1/network/inventory",
  vmList: "/api/v1/vms",
  vmDetail: "/api/v1/vms/{vm_id}",
  vmBlkio: "/api/v1/vms/{vm_id}/blkio",
  vmBandwidth: "/api/v1/vms/{vm_id}/bandwidth",
  vmGuestAgentStatus: "/api/v1/vms/{vm_id}/guest-agent/status",
  vmGuestAgentPing: "/api/v1/vms/{vm_id}/guest-agent/ping",
  vmAction: "/api/v1/vms/{vm_id}/{action}",
  vmCheckpoints: "/api/v1/vms/{vm_id}/checkpoints",
  checkpointDetail: "/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}",
  checkpointAction: "/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/{action}",
  jobList: "/api/v1/jobs",
  jobsPage: "/api/v1/jobs?limit={limit}&offset={offset}",
  jobDetail: "/api/v1/jobs/{job_id}",
  jobAction: "/api/v1/jobs/{job_id}/{action}",
  authLogin: "/api/v1/auth/login",
  authRefresh: "/api/v1/auth/refresh",
  authLogout: "/api/v1/auth/logout",
  authSession: "/api/v1/auth/session",
  authRbac: "/api/v1/auth/rbac",
  consoleCapabilities: "/api/v1/console/capabilities",
  vmConsole: "/api/v1/vms/{vm_id}/console"
} as const;

export type WebConsoleSnapshot = {
  opsSummary?: OpsSummaryResponse["data"] | null;
  runtimePolicy?: RuntimePolicyResponse["data"] | null;
  host?: HostStatusResponse["data"] | null;
  networkInventory?: NetworkInventoryResponse["data"] | null;
  vms?: VirtualMachineListResponse["data"] | null;
  jobs?: JobListResponse["data"] | null;
};

export function buildStaticParitySnapshot(snapshot: WebConsoleSnapshot) {
  const vms = snapshot.vms?.vms ?? snapshot.vms?.items ?? [];
  const jobs = snapshot.jobs?.jobs ?? snapshot.jobs?.items ?? [];

  return {
    routes: localApiRoutes,
    scaffold: phase25WebConsoleScaffold,
    opsSummary: snapshot.opsSummary ?? null,
    networkInventory: snapshot.networkInventory ?? null,
    dashboard: buildDashboardViewModel({
      host: snapshot.host,
      vms,
      jobs,
      runtimePolicy: snapshot.runtimePolicy
    })
  };
}

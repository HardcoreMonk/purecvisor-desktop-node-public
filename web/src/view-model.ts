import type {
  HostStatusResponse,
  JobRuntimePolicy,
  JobSummary,
  RuntimePolicyResponse,
  VirtualMachineSummary
} from "./api-types";

export type DashboardViewModel = {
  hostReady: boolean;
  vmCount: number;
  runningVmCount: number;
  activeJobCount: number;
  jobRuntimeOwner: string;
  helperBoundary: string;
  retryPolicy: string;
};

export function buildDashboardViewModel(input: {
  host?: HostStatusResponse["data"] | null;
  vms?: VirtualMachineSummary[] | null;
  jobs?: JobSummary[] | null;
  runtimePolicy?: RuntimePolicyResponse["data"] | null;
}): DashboardViewModel {
  const vms = input.vms ?? [];
  const jobs = input.jobs ?? [];
  const runtime = input.runtimePolicy?.job_runtime;

  return {
    hostReady: input.host?.supported !== false,
    vmCount: vms.length,
    runningVmCount: vms.filter((vm) => isRunning(vm.state ?? vm.status)).length,
    activeJobCount: jobs.filter((job) => isActiveJob(job.status)).length,
    jobRuntimeOwner: runtime?.owner ?? "local-api",
    helperBoundary: runtime?.dispatch.helper_boundary ?? "dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation",
    retryPolicy: describeRetryPolicy(runtime)
  };
}

function isRunning(value: unknown): boolean {
  return String(value ?? "").toLowerCase().includes("running");
}

function isActiveJob(status: unknown): boolean {
  const normalized = String(status ?? "").toLowerCase();
  return normalized === "queued" || normalized === "running";
}

function describeRetryPolicy(runtime: JobRuntimePolicy | undefined): string {
  const retry = runtime?.control.retry;
  if (!retry) {
    return "retry-policy-unknown";
  }

  if (retry.manual_only && retry.failed_error_retryable_only) {
    return `manual-retryable-failed-only:max-${retry.max_attempts}`;
  }

  return "custom-retry-policy";
}

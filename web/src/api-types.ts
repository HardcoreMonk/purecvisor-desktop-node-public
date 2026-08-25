export type ApiResult<TData> = {
  ok: boolean;
  operation: string;
  data?: TData;
  error?: ApiError | null;
};

export type ApiError = {
  code: string;
  message: string;
  detail?: string;
  retryable?: boolean;
};

export type RuntimePolicyResponse = ApiResult<{
  job_runtime: JobRuntimePolicy;
  network?: NetworkPolicy;
  token?: TokenPolicy;
}>;

export type JobRuntimePolicy = {
  contract_version: number;
  owner: "local-api";
  state_store: {
    backend: string;
    persistence: "json-file-snapshot";
    corrupt_store: string;
    unsupported_future_version: string;
  };
  dispatch: {
    mode: string;
    helper_boundary: "dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation";
    native_probe_operations?: string[];
    native_mutation_operations?: string[];
    mutation_dispatch?: string;
  };
  control: {
    cancel: {
      queued_only: boolean;
      running_interrupt: boolean;
    };
    retry: {
      manual_only: boolean;
      failed_error_retryable_only: boolean;
      max_attempts: number;
      creates_new_job: boolean;
    };
  };
  host_mutation: "native-read-routes-vm-create-lifecycle-delete-and-checkpoint-mutation";
  orchestration?: {
    primary: string;
    contract: string;
  };
  native_core?: {
    status: string;
    reason: string;
    revisit_when: string;
  };
  managed_core?: {
    candidate: "dotnet";
    status: string;
    host_replacement: "dotnet-windows-service-host";
  };
};

export type NetworkPolicy = {
  bind: string;
  allow_lan: boolean;
  static_assets_require_auth_on_non_loopback?: boolean;
};

export type TokenPolicy = {
  required: boolean;
  source?: string;
  storage?: string;
};

export type HostStatusResponse = ApiResult<Record<string, unknown>>;

export type NetworkInventoryResponse = ApiResult<{
  source?: string;
  mutating?: boolean;
  switches?: NetworkSwitchSummary[];
  items?: NetworkSwitchSummary[];
}>;

export type NetworkSwitchSummary = {
  name?: string;
  type?: string;
  switch_type?: string;
  is_default?: boolean;
  allow_management_os?: boolean;
  net_adapter_interface_description?: string;
  adapter?: string;
  adapter_name?: string;
};

export type BatchEvidenceSummary = {
  schema_version: number;
  configured: boolean;
  status: BatchEvidenceStatus;
  artifact_root?: string | null;
  latest?: BatchEvidenceRunSummary | null;
  errors?: ApiError[];
};

export type BatchEvidenceStatus = "not_configured" | "missing" | "available" | "degraded" | "unavailable" | string;

export type BatchEvidenceRunSummary = {
  batch_id?: string | null;
  ok?: boolean | null;
  status?: string | null;
  total_steps?: number | null;
  executed_steps?: number | null;
  steps?: BatchEvidenceStepSummary[];
  gpu_snapshots?: {
    status?: BatchEvidenceStatus;
    present?: boolean;
    count?: number;
    status_counts?: Record<string, number>;
    peak_adapter_mib?: number | null;
    peak_process_mib?: number | null;
  };
  release?: {
    status?: BatchEvidenceStatus;
    version?: string | null;
    git_commit?: string | null;
    msi_sha256?: string | null;
    signing_mode?: string | null;
    public_trusted_signing?: string | null;
    external_stable_publication?: string | null;
  };
  route_msi_hyperv?: BatchEvidenceRouteMsiHypervSummary;
  os_mutation?: BatchEvidenceOsMutationSummary;
  host_final_state?: Record<string, unknown>;
};

export type BatchEvidenceRouteMsiHypervSummary = {
  status?: BatchEvidenceStatus;
  ok?: boolean | null;
  version?: string | null;
  boot_time_unchanged?: boolean | null;
  msi_lifecycle_ok?: boolean | null;
  msi_lifecycle_step_count?: number | null;
};

export type BatchEvidenceOsMutationSummary = {
  status?: BatchEvidenceStatus;
  ok?: boolean | null;
  version?: string | null;
  boot_time_unchanged?: boolean | null;
  firewall_rule_count?: number | null;
  eventlog_source_present?: boolean | null;
};

export type BatchEvidenceStepSummary = {
  step_id?: string | null;
  ok?: boolean | null;
  exit_code?: number | null;
  timed_out?: boolean | null;
  retry_count?: number | null;
  attempt_count?: number | null;
  final_attempt?: number | null;
  duration_ms?: number | null;
};

export type CurrentEvidenceRollup = {
  schema_version?: number;
  contract_key?: string | null;
  source?: string | null;
  public_boundary?: {
    latest_main_push?: {
      status?: string | null;
      source?: string | null;
      evidence?: string | null;
      run_id?: string | null;
      job_id?: string | null;
      head_sha?: string | null;
      public_trusted_signing?: string | null;
      external_stable_publication?: string | null;
    };
  };
  full_admin_host_mutation?: {
    latest?: {
      status?: BatchEvidenceStatus | string | null;
      source?: string | null;
      batch_id?: string | null;
      evidence_status?: BatchEvidenceStatus | string | null;
      run_status?: string | null;
      version?: string | null;
      msi_sha256?: string | null;
      git_commit?: string | null;
      signing_mode?: string | null;
      service_state?: string | null;
      route_msi_hyperv_status?: string | null;
      os_mutation_status?: string | null;
      public_trusted_signing?: string | null;
      external_stable_publication?: string | null;
    };
  };
  host_ops?: {
    lifecycle_descriptor?: {
      status?: string | null;
      source?: string | null;
      contract_key?: string | null;
      lifecycle_bucket_contract_key?: string | null;
      schema_version?: number | null;
      host_mutation_performed?: boolean | null;
      buckets?: Array<{
        bucket_key?: string | null;
        operation_family?: string | null;
        owner?: string | null;
        mutation_boundary?: string | null;
        operations?: string[];
      }>;
    };
  };
  manual_admin?: {
    latest_package_pair?: {
      status?: string | null;
      source?: string | null;
      evidence?: string | null;
      package_pair?: string | null;
      baseline_version?: string | null;
      target_version?: string | null;
      current_card_descriptor_batch_id?: string | null;
      descriptor_batch_id?: string | null;
      descriptor_contract_key?: string | null;
      descriptor_generation_contract?: string | null;
      descriptor_overall_status?: string | null;
      descriptor_schema_version?: number | null;
      descriptor_source?: string | null;
      descriptor_summary?: string | null;
      missing_count?: number | null;
      not_pass_count?: number | null;
      runner_count?: number | null;
      public_trusted_signing?: string | null;
      external_stable_publication?: string | null;
    };
    next_package_pair?: {
      status?: string | null;
      source?: string | null;
      evidence?: string | null;
      decision?: string | null;
      package_pair?: string | null;
      baseline_version?: string | null;
      target_version?: string | null;
      host_mutation_performed?: boolean | null;
      public_trusted_signing?: string | null;
      external_stable_publication?: string | null;
    };
  };
};

export type OpsSummaryResponse = ApiResult<{
  host?: Record<string, unknown>;
  installed_runtime?: {
    version?: string | null;
    service_state?: string | null;
    diagnostics?: {
      runtime_api_registry_bridge?: {
        contract_key?: string | null;
        handler_registry_source?: string | null;
        documentation_anchor?: string | null;
        route_keys?: string[];
      };
    };
  };
  vm_counts?: {
    total?: number;
    running?: number;
    stopped?: number;
    failed?: number;
    checkpoint_warnings?: number;
  };
  job_counts?: {
    active?: number;
    queued?: number;
    running?: number;
    failed?: number;
    succeeded?: number;
    canceled?: number;
  };
  runtime_policy?: Record<string, unknown>;
  batch_evidence?: BatchEvidenceSummary;
  current_evidence?: CurrentEvidenceRollup;
  signals?: unknown[];
  recent_activity?: JobSummary[];
  errors?: ApiError[];
}>;

export type VirtualMachineListResponse = ApiResult<{
  vms?: VirtualMachineSummary[];
  items?: VirtualMachineSummary[];
}>;

export type VirtualMachineSummary = {
  id?: string;
  name?: string;
  state?: string;
  status?: string;
  generation?: number;
  cpu?: number | { count?: number };
  memory?: number | { startup_mb?: number };
  memory_mb?: number;
  uptime?: string;
  updated_at?: string;
  created_at?: string;
  notes?: string;
  error?: ApiError;
};

export type CheckpointSummary = {
  id?: string;
  name?: string;
  checkpoint_name?: string;
  created_at?: string;
  creation_time?: string;
  type?: string;
  notes?: string;
};

export type JobListResponse = ApiResult<{
  count?: number;
  default_limit?: number;
  jobs?: JobSummary[];
  items?: JobSummary[];
  limit?: number;
  max_limit?: number;
  next_offset?: number | null;
  offset?: number;
  retention?: {
    active_jobs_preserved?: boolean;
    max_terminal_jobs?: number;
    pruned_terminal_jobs?: number;
    terminal_statuses?: string[];
  };
  returned?: number;
}>;

export type JobSummary = {
  job_id: string;
  operation: string;
  status: "queued" | "running" | "succeeded" | "failed" | "canceled" | string;
  retry_of?: string | null;
  attempt?: number;
  result?: unknown;
  error?: ApiError | null;
};

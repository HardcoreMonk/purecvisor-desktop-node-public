import { buildStaticParitySnapshot, type WebConsoleSnapshot } from "./app";
import type { JobRuntimePolicy, NetworkInventoryResponse } from "./api-types";

const fixtureRuntimePolicy = {
  contract_version: 1,
  owner: "local-api",
  state_store: {
    backend: "dotnet-json-job-store",
    persistence: "json-file-snapshot",
    corrupt_store: "quarantine-and-start-empty",
    unsupported_future_version: "blocked-diagnostics-no-mutation"
  },
  dispatch: {
    mode: "bounded-synchronous-worker-tick",
    helper_boundary: "dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation",
    native_probe_operations: ["host.status", "network.inventory", "vm.list", "checkpoint.list"],
    native_mutation_operations: [
      "vm.create",
      "vm.start",
      "vm.shutdown",
      "vm.poweroff",
      "vm.restart",
      "vm.delete",
      "checkpoint.create",
      "checkpoint.restore",
      "checkpoint.delete"
    ],
    mutation_dispatch: "native-vm-create-lifecycle-delete-checkpoint-mutation"
  },
  control: {
    cancel: {
      queued_only: true,
      running_interrupt: false
    },
    retry: {
      manual_only: true,
      failed_error_retryable_only: true,
      max_attempts: 3,
      creates_new_job: true
    }
  },
  host_mutation: "native-read-routes-vm-create-lifecycle-delete-and-checkpoint-mutation",
  orchestration: {
    primary: "dotnet",
    contract: "dotnet-native-adapter-contract-tests-admin-smoke"
  },
  native_core: {
    status: "read-route-vm-create-lifecycle-and-checkpoint-mutation-started",
    reason: "host.status,network.inventory,vm.list,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete",
    revisit_when: "next-read-route-or-mutation-adapter-parity"
  },
  managed_core: {
    candidate: "dotnet",
    status: "service-host-default",
    host_replacement: "dotnet-windows-service-host"
  }
} satisfies JobRuntimePolicy;

const fixtureNetworkInventory = {
  source: "native-csharp",
  mutating: false,
  switches: [
    {
      name: "Default Switch",
      type: "Internal",
      is_default: true,
      allow_management_os: true,
      net_adapter_interface_description: "fixture-ethernet"
    },
    {
      name: "lab-external",
      type: "External",
      is_default: false,
      allow_management_os: false,
      net_adapter_interface_description: "Intel(R) Ethernet"
    }
  ]
} satisfies NonNullable<NetworkInventoryResponse["data"]>;

function buildFixture(snapshot: WebConsoleSnapshot) {
  return buildStaticParitySnapshot({
    runtimePolicy: {
      job_runtime: fixtureRuntimePolicy
    },
    ...snapshot
  });
}

export const webConsoleUserVisibleParityContract = {
  generatedBy: "src/user-visible-fixtures.ts",
  runtimeReplacement: "default",
  servedAsset: "app.js",
  servedTypeScriptEntry: "src/served-app.ts",
  replacesServedAsset: true,
  fixtureNames: [
    "emptyInventory",
    "runningVmAndJob",
    "degradedBatchEvidence",
    "unsupportedHost"
  ]
} as const;

export const webConsoleUserVisibleParityFixtures = {
  emptyInventory: buildFixture({
    host: { supported: true },
    vms: { vms: [] },
    jobs: { jobs: [] },
    opsSummary: {
      installed_runtime: {
        version: "0.42.21-admin-smoke",
        service_state: "Running",
        diagnostics: {
          runtime_api_registry_bridge: {
            contract_key: "runtime-api-diagnostics-ops-summary-registry-bridge-v2",
            handler_registry_source: "DesktopNodeApiRuntimeRoutes",
            documentation_anchor: "docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md#runtime-api-diagnostics-ops-summary",
            route_keys: [
              "GET /api/v1/diagnostics/bundles -> ListDiagnosticBundles [runtime-api-diagnostics-bundle-contract]",
              "GET /api/v1/diagnostics/bundles/{bundleId}/download -> DownloadDiagnosticBundle [runtime-api-diagnostics-bundle-contract]",
              "GET /api/v1/ops/summary -> OpsSummary [runtime-api-ops-summary-current-card]",
              "POST /api/v1/diagnostics/bundles -> CreateDiagnosticBundle [runtime-api-diagnostics-bundle-contract]"
            ]
          }
        }
      },
      batch_evidence: {
        schema_version: 1,
        configured: true,
        status: "missing",
        artifact_root: "[BATCH_EVIDENCE_ROOT]",
        latest: null,
        errors: [
          {
            code: "PCV_BATCH_EVIDENCE_MISSING",
            message: "No batch supervisor summary was found."
          }
        ]
      },
      current_evidence: {
        schema_version: 1,
        contract_key: "runtime-api-current-evidence-rollup-v1",
        source: "ops-summary",
        public_boundary: {
          latest_main_push: {
            status: "tracked-in-documentation",
            source: "docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md",
            evidence: "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md",
            run_id: "26578120570",
            job_id: "78303066840",
            head_sha: "7a7d5de822bdb058b04149eeeef0a7eb462828b5",
            public_trusted_signing: "not-claimed",
            external_stable_publication: "not-claimed"
          }
        },
        full_admin_host_mutation: {
          latest: {
            status: "missing",
            source: "batch_evidence"
          }
        },
        host_ops: {
          lifecycle_descriptor: {
            status: "contract-linked",
            source: "DesktopNode.Contracts.HostOpsLifecycleDescriptor",
            contract_key: "host-ops-lifecycle-descriptor-bridge-v1",
            lifecycle_bucket_contract_key: "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated",
            schema_version: 1,
            host_mutation_performed: false,
            buckets: [
              { bucket_key: "service-action", operation_family: "service-lifecycle", mutation_boundary: "windows-service-control-manager", owner: "DesktopNodeServiceLifecycleOps", operations: ["configure-installed", "repair-installed", "remove-installed"] },
              { bucket_key: "event-log", operation_family: "event-log", mutation_boundary: "windows-event-log-provider", owner: "DesktopNodeEventLogOps", operations: ["eventlog-repair", "eventlog-default-transition"] },
              { bucket_key: "firewall", operation_family: "firewall", mutation_boundary: "windows-firewall-rule", owner: "DesktopNodeFirewallOps", operations: ["firewall-enable", "firewall-remove"] },
              { bucket_key: "trust-store", operation_family: "trust-store", mutation_boundary: "windows-x509-store", owner: "DesktopNodeTrustStoreOps", operations: ["trust-store-install", "trust-store-remove"] },
              { bucket_key: "credential-manager", operation_family: "credential-manager", mutation_boundary: "windows-credential-manager", owner: "DesktopNodeCredentialManagerOps", operations: ["credential-manager-system-proof", "credential-manager-default-transition"] },
              { bucket_key: "data-root", operation_family: "data-root", mutation_boundary: "allowlisted-programdata-root", owner: "DesktopNodeDataRootLifecycleOps", operations: ["data-root-remove"] }
            ]
          }
        },
        manual_admin: {
          latest_package_pair: {
            status: "tracked-in-documentation",
            source: "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            package_pair: "0.42.22-admin-smoke -> 0.42.23-admin-smoke",
            current_card_descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04222-04223-closed",
            descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04222-04223-closed",
            descriptor_contract_key: "manual-admin-descriptor-generation-contract-v2",
            descriptor_generation_contract: "manual-admin-descriptor-generation-contract-v2",
            descriptor_overall_status: "pass",
            descriptor_schema_version: 2
          },
          next_package_pair: {
            status: "candidate-selected-awaiting-target",
            source: "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            evidence: "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md",
            decision: "opened-public-boundary-current-evidence-rollup-payload",
            package_pair: "0.42.56-admin-smoke -> 0.42.57-admin-smoke",
            baseline_version: "0.42.56-admin-smoke",
            target_version: "0.42.57-admin-smoke",
            host_mutation_performed: false,
            public_trusted_signing: "not-claimed",
            external_stable_publication: "not-claimed"
          }
        }
      }
    }
  }),
  runningVmAndJob: buildFixture({
    host: { supported: true },
    networkInventory: fixtureNetworkInventory,
    vms: {
      vms: [
        {
          name: "pcv-fixture-vm",
          state: "Running",
          generation: 2
        }
      ]
    },
    jobs: {
      jobs: [
        {
          job_id: "fixture-job-1",
          operation: "vm.create",
          status: "running"
        }
      ]
    },
    opsSummary: {
      batch_evidence: {
        schema_version: 1,
        configured: true,
        status: "available",
        artifact_root: "[BATCH_EVIDENCE_ROOT]",
        latest: {
          batch_id: "full-admin-host-mutation-gate-20260506-212527-0384",
          ok: true,
          status: "completed",
          total_steps: 2,
          executed_steps: 2,
          steps: [
            {
              step_id: "service-msi-hyperv-admin-smoke",
              ok: true,
              exit_code: 0,
              timed_out: false,
              retry_count: 1,
              attempt_count: 1,
              final_attempt: 1,
              duration_ms: 133356
            },
            {
              step_id: "os-mutation-gate",
              ok: true,
              exit_code: 0,
              timed_out: false,
              retry_count: 0,
              attempt_count: 1,
              final_attempt: 1,
              duration_ms: 11047
            }
          ],
          gpu_snapshots: {
            status: "available",
            present: true,
            count: 22,
            status_counts: { collected: 22 },
            peak_adapter_mib: 3306.84,
            peak_process_mib: 17382.52
          },
          release: {
            status: "available",
            version: "0.38.4-admin-smoke",
            signing_mode: "AllowUnsignedDev",
            public_trusted_signing: "excluded",
            external_stable_publication: "not-claimed"
          },
          route_msi_hyperv: {
            status: "available",
            ok: true,
            msi_lifecycle_ok: true,
            msi_lifecycle_step_count: 6
          },
          os_mutation: {
            status: "available",
            ok: true,
            firewall_rule_count: 0,
            eventlog_source_present: false
          },
          host_final_state: {
            service_state: "Running",
            firewall_rule_count: 0,
            eventlog_source_present: false,
            trust_root_present: true,
            trust_publisher_present: true
          }
        },
        errors: []
      },
      current_evidence: {
        schema_version: 1,
        contract_key: "runtime-api-current-evidence-rollup-v1",
        source: "ops-summary",
        public_boundary: {
          latest_main_push: {
            status: "artifact-discovered",
            source: "batch_evidence_artifact",
            evidence: "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md",
            run_id: "26578120570",
            job_id: "78303066840",
            head_sha: "7a7d5de822bdb058b04149eeeef0a7eb462828b5",
            public_trusted_signing: "not-claimed",
            external_stable_publication: "not-claimed"
          }
        },
        full_admin_host_mutation: {
          latest: {
            status: "available",
            source: "batch_evidence",
            batch_id: "full-admin-host-mutation-gate-20260506-212527-0384",
            version: "0.38.4-admin-smoke",
            msi_sha256: "7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7",
            git_commit: "6bbb39f0a3a271e4a1187ce7de2014e009977425",
            signing_mode: "AllowUnsignedDev",
            service_state: "Running",
            public_trusted_signing: "excluded",
            external_stable_publication: "not-claimed"
          }
        },
        host_ops: {
          lifecycle_descriptor: {
            status: "contract-linked",
            source: "DesktopNode.Contracts.HostOpsLifecycleDescriptor",
            contract_key: "host-ops-lifecycle-descriptor-bridge-v1",
            lifecycle_bucket_contract_key: "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated",
            schema_version: 1,
            host_mutation_performed: false,
            buckets: [
              { bucket_key: "service-action", operation_family: "service-lifecycle", mutation_boundary: "windows-service-control-manager", owner: "DesktopNodeServiceLifecycleOps", operations: ["configure-installed", "repair-installed", "remove-installed"] },
              { bucket_key: "event-log", operation_family: "event-log", mutation_boundary: "windows-event-log-provider", owner: "DesktopNodeEventLogOps", operations: ["eventlog-repair", "eventlog-default-transition"] },
              { bucket_key: "firewall", operation_family: "firewall", mutation_boundary: "windows-firewall-rule", owner: "DesktopNodeFirewallOps", operations: ["firewall-enable", "firewall-remove"] },
              { bucket_key: "trust-store", operation_family: "trust-store", mutation_boundary: "windows-x509-store", owner: "DesktopNodeTrustStoreOps", operations: ["trust-store-install", "trust-store-remove"] },
              { bucket_key: "credential-manager", operation_family: "credential-manager", mutation_boundary: "windows-credential-manager", owner: "DesktopNodeCredentialManagerOps", operations: ["credential-manager-system-proof", "credential-manager-default-transition"] },
              { bucket_key: "data-root", operation_family: "data-root", mutation_boundary: "allowlisted-programdata-root", owner: "DesktopNodeDataRootLifecycleOps", operations: ["data-root-remove"] }
            ]
          }
        },
        manual_admin: {
          latest_package_pair: {
            status: "artifact-discovered",
            source: "batch_evidence_artifact",
            evidence: "artifacts/manual-admin-campaign-20260516-04223-04224/manual-admin-campaign-descriptor/summary.json",
            package_pair: "0.42.23-admin-smoke -> 0.42.24-admin-smoke",
            baseline_version: "0.42.23-admin-smoke",
            target_version: "0.42.24-admin-smoke",
            current_card_descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04223-04224-closed",
            descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04223-04224-closed",
            descriptor_contract_key: "manual-admin-descriptor-generation-contract-v2",
            descriptor_generation_contract: "manual-admin-descriptor-generation-contract-v2",
            descriptor_overall_status: "pass",
            descriptor_schema_version: 2,
            descriptor_summary: "artifacts/manual-admin-campaign-20260516-04223-04224/manual-admin-campaign-descriptor/summary.json",
            public_trusted_signing: "not-claimed",
            external_stable_publication: "not-claimed"
          }
        }
      }
    }
  }),
  degradedBatchEvidence: buildFixture({
    host: { supported: true },
    vms: { vms: [] },
    jobs: { jobs: [] },
    opsSummary: {
      batch_evidence: {
        schema_version: 1,
        configured: true,
        status: "degraded",
        artifact_root: "[BATCH_EVIDENCE_ROOT]",
        latest: {
          batch_id: "full-admin-host-mutation-gate-20260506-212527-0384",
          ok: true,
          status: "completed",
          total_steps: 2,
          executed_steps: 2,
          steps: [
            {
              step_id: "service-msi-hyperv-admin-smoke",
              ok: true,
              exit_code: 0,
              timed_out: false,
              retry_count: 1,
              attempt_count: 1,
              final_attempt: 1,
              duration_ms: 133356
            },
            {
              step_id: "os-mutation-gate",
              ok: true,
              exit_code: 0,
              timed_out: false,
              retry_count: 0,
              attempt_count: 1,
              final_attempt: 1,
              duration_ms: 11047
            }
          ],
          gpu_snapshots: {
            status: "unavailable",
            present: true,
            count: 21,
            status_counts: { collected: 21 },
            peak_adapter_mib: 3306.84,
            peak_process_mib: 17382.52
          },
          release: {
            status: "available",
            version: "0.38.4-admin-smoke",
            signing_mode: "AllowUnsignedDev",
            public_trusted_signing: "excluded",
            external_stable_publication: "not-claimed"
          },
          route_msi_hyperv: {
            status: "available",
            ok: true,
            msi_lifecycle_ok: true,
            msi_lifecycle_step_count: 6
          },
          os_mutation: {
            status: "unavailable",
            ok: null,
            firewall_rule_count: null,
            eventlog_source_present: null
          },
          host_final_state: {
            service_state: "Running",
            firewall_rule_count: null,
            eventlog_source_present: null,
            trust_root_present: null,
            trust_publisher_present: null
          }
        },
        errors: [
          {
            code: "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
            message: "os_mutation evidence JSON could not be parsed.",
            detail: "[BATCH_EVIDENCE_ROOT]/os-mutation-gates-batch-profile-20260506-212527-0384/summary.json parse failed",
            retryable: false
          }
        ]
      }
    }
  }),
  unsupportedHost: buildFixture({
    host: { supported: false },
    vms: { vms: [] },
    jobs: { jobs: [] },
    opsSummary: {
      batch_evidence: {
        schema_version: 1,
        configured: true,
        status: "unavailable",
        artifact_root: "[BATCH_EVIDENCE_ROOT]",
        latest: null,
        errors: [
          {
            code: "PCV_BATCH_EVIDENCE_PARSE_FAILED",
            message: "Batch evidence JSON could not be parsed.",
            detail: "summary.json parse failed",
            retryable: false
          }
        ]
      }
    }
  })
} as const;

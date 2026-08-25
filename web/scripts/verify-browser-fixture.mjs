import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const webRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const appScript = readFileSync(join(webRoot, "app.js"), "utf8");
const indexText = readFileSync(join(webRoot, "index.html"), "utf8");

const requiredIds = [
  "connection-form",
  "api-base-url",
  "api-token",
  "clear-token",
  "refresh-all",
  "connection-state",
  "asset-status",
  "alert-region",
  "dashboard",
  "vms",
  "network",
  "jobs",
  "activity",
  "evidence",
  "troubleshooting",
  "view-dashboard",
  "view-vms",
  "view-network",
  "view-evidence",
  "view-troubleshooting",
  "ops-summary-panel",
  "priority-panel",
  "dashboard-activity-panel",
  "vm-workbench-context",
  "incident-panel",
  "token-rotation-panel",
  "diagnostics-panel",
  "beta-followup-panel",
  "metric-grid",
  "host-details",
  "vm-filter",
  "vm-state-filter",
  "vm-sort",
  "vm-table",
  "network-inventory-panel",
  "vm-detail-panel",
  "vm-detail-title",
  "vm-detail-content",
  "close-vm-detail",
  "jobs-panel",
  "clear-job-history",
  "activity-panel",
  "evidence-panel",
  "monitoring-panel",
  "troubleshooting-panel",
  "open-create-vm",
  "close-create-vm",
  "create-vm-dialog",
  "create-vm-form",
  "status-connection",
  "status-host",
  "status-updated",
  "status-vm-count",
  "status-view",
  "hero-workload",
  "hero-host-mode",
  "hero-alerts"
];

class FixtureElement {
  constructor(id) {
    this.id = id;
    this.className = "";
    this.dataset = {};
    this.innerHTML = "";
    this.hidden = false;
    this.listeners = new Map();
    this.open = false;
    this.textContent = "";
    this.value = "";
    this.attributes = {};
  }

  addEventListener(type, handler) {
    const handlers = this.listeners.get(type) ?? [];
    handlers.push(handler);
    this.listeners.set(type, handlers);
  }

  removeAttribute(name) {
    delete this.attributes[name];
    if (name === "hidden") this.hidden = false;
  }

  setAttribute(name, value) {
    this.attributes[name] = String(value);
    if (name === "hidden") this.hidden = true;
  }

  close() {
    this.open = false;
  }

  closest() {
    return null;
  }

  reset() {
    this.value = "";
  }

  showModal() {
    this.open = true;
  }
}

function createLocalStorage(initialEntries = []) {
  const values = new Map(initialEntries);

  return {
    getItem(key) {
      return values.has(key) ? values.get(key) : null;
    },
    removeItem(key) {
      values.delete(key);
    },
    setItem(key, value) {
      values.set(key, String(value));
    }
  };
}

class FixtureHeaders {
  constructor(init = {}) {
    this.values = new Map();
    if (init && typeof init.forEach === "function") {
      init.forEach((value, key) => this.set(key, value));
      return;
    }
    for (const [key, value] of Object.entries(init || {})) {
      this.set(key, value);
    }
  }

  set(key, value) {
    this.values.set(String(key).toLowerCase(), String(value));
  }

  get(key) {
    return this.values.get(String(key).toLowerCase()) ?? null;
  }

  has(key) {
    return this.values.has(String(key).toLowerCase());
  }

  forEach(callback) {
    for (const [key, value] of this.values.entries()) {
      callback(value, key, this);
    }
  }
}

function createDocument() {
  const elements = new Map();
  const listeners = new Map();

  function getElementById(id) {
    if (!elements.has(id)) {
      elements.set(id, new FixtureElement(id));
    }

    return elements.get(id);
  }

  for (const id of requiredIds) {
    getElementById(id);
  }

  const viewNames = ["dashboard", "vms", "network", "jobs", "activity", "evidence", "troubleshooting"];
  const viewLinks = viewNames.map((view) => {
    const element = new FixtureElement(`link-${view}`);
    element.dataset.viewLink = view;
    element.href = `#${view}`;
    elements.set(element.id, element);
    return element;
  });
  const appViews = viewNames.map((view) => {
    const element = getElementById(view);
    element.className = "section app-view";
    element.dataset.view = view;
    return element;
  });

  return {
    elements,
    addEventListener(type, handler) {
      const handlers = listeners.get(type) ?? [];
      handlers.push(handler);
      listeners.set(type, handlers);
    },
    createElement(tagName) {
      return new FixtureElement(tagName);
    },
    dispatch(type) {
      for (const handler of listeners.get(type) ?? []) {
        handler();
      }
    },
    getElementById,
    querySelectorAll(selector) {
      if (selector === "[data-view-link]") return viewLinks;
      if (selector === ".app-view") return appViews;
      return [];
    },
    viewLinks
  };
}

function ok(data, operation) {
  return {
    ok: true,
    status: 200,
    async json() {
      return { ok: true, operation, data };
    }
  };
}

const fixtureVm = {
  id: "pcv-browser-fixture",
  name: "pcv-browser-fixture",
  state: "off",
  cpu: { count: 2 },
  memory: { startup_mb: 4096 },
  generation: 2,
  checkpoints: { count: 0 },
  network: [{ name: "fixture-nic", switch: "Default Switch", mode: "nat" }],
  storage: [{ path: "D:\\PureCVisor\\VMs\\pcv-browser-fixture.vhdx", size_gb: 40 }],
  managed_by_purecvisor: true
};

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
};

function failResponse(status, operation, code, message) {
  return {
    ok: false,
    status,
    async json() {
      return {
        ok: false,
        operation,
        error: {
          code,
          message,
          detail: message,
          retryable: false
        }
      };
    }
  };
}

function problemDetailsResponse(status, operation, code, title, detail, extra = {}) {
  return {
    ok: false,
    status,
    headers: {
      get(name) {
        return String(name).toLowerCase() === "retry-after" && extra.retry_after_seconds
          ? String(extra.retry_after_seconds)
          : null;
      }
    },
    async json() {
      return {
        type: "about:blank",
        title,
        status,
        code,
        detail,
        operation,
        request_id: "req-problem-details-fixture",
        retryable: true,
        ...extra
      };
    }
  };
}

function buildBatchEvidenceFixture() {
  return {
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
        { step_id: "service-msi-hyperv-admin-smoke", ok: true, exit_code: 0, timed_out: false, retry_count: 1, attempt_count: 1 },
        { step_id: "os-mutation-gate", ok: true, exit_code: 0, timed_out: false, retry_count: 0, attempt_count: 1 }
      ],
      gpu_snapshots: { status: "available", present: true, count: 22, status_counts: { collected: 22 }, peak_adapter_mib: 3306.84, peak_process_mib: 17382.52 },
      release: {
        status: "available",
        version: "0.38.4-admin-smoke",
        signing_mode: "AllowUnsignedDev",
        public_trusted_signing: "excluded",
        external_stable_publication: "not-claimed"
      },
      route_msi_hyperv: { status: "available", ok: true, msi_lifecycle_ok: true, msi_lifecycle_step_count: 6 },
      os_mutation: { status: "available", ok: true, firewall_rule_count: 0, eventlog_source_present: false },
      host_final_state: { service_state: "Running", firewall_rule_count: 0, eventlog_source_present: false, trust_root_present: true, trust_publisher_present: true }
    },
    errors: []
  };
}

function buildCurrentEvidenceFixture(batchEvidence = buildBatchEvidenceFixture()) {
  const latest = batchEvidence.latest || {};
  const release = latest.release || {};
  const host = latest.host_final_state || {};
  return {
    schema_version: 1,
    contract_key: "runtime-api-current-evidence-rollup-v1",
    source: "ops-summary",
    public_boundary: {
      latest_main_push: {
        status: "tracked-in-documentation",
        source: "docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md",
        evidence: "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md",
        run_id: "25954744127",
        job_id: "76299282407",
        head_sha: "d11a096086326004f27facd9612c2296ded15a4b",
        public_trusted_signing: "not-claimed",
        external_stable_publication: "not-claimed"
      }
    },
    full_admin_host_mutation: {
      latest: {
        status: batchEvidence.status || "available",
        source: "batch_evidence",
        batch_id: latest.batch_id,
        evidence_status: batchEvidence.status || "available",
        run_status: latest.status,
        version: release.version,
        msi_sha256: release.msi_sha256 || "7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7",
        git_commit: release.git_commit || "6bbb39f0a3a271e4a1187ce7de2014e009977425",
        signing_mode: release.signing_mode,
        service_state: host.service_state,
        public_trusted_signing: release.public_trusted_signing || "excluded",
        external_stable_publication: release.external_stable_publication || "not-claimed"
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
        evidence: "docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md",
        package_pair: "0.42.22-admin-smoke -> 0.42.23-admin-smoke",
        baseline_version: "0.42.22-admin-smoke",
        target_version: "0.42.23-admin-smoke",
        current_card_descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04222-04223-closed",
        descriptor_batch_id: "manual-admin-campaign-descriptor-20260516-04222-04223-closed",
        descriptor_contract_key: "manual-admin-descriptor-generation-contract-v2",
        descriptor_generation_contract: "manual-admin-descriptor-generation-contract-v2",
        descriptor_overall_status: "pass",
        descriptor_schema_version: 2,
        descriptor_summary: "docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md",
        public_trusted_signing: "not-claimed",
        external_stable_publication: "not-claimed"
      },
      next_package_pair: {
        status: "candidate-selected-awaiting-target",
        source: "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
        evidence: "docs/ga-ready/evidence/post-04255-followup-execution-2026-05-28.md",
        decision: "not-opened-no-next-product-payload-target",
        package_pair: "0.42.55-admin-smoke -> next-admin-smoke-required",
        baseline_version: "0.42.55-admin-smoke",
        target_version: "next-admin-smoke-required",
        host_mutation_performed: false,
        public_trusted_signing: "not-claimed",
        external_stable_publication: "not-claimed"
      }
    }
  };
}

function buildDegradedBatchEvidenceFixture() {
  const evidence = buildBatchEvidenceFixture();
  evidence.status = "degraded";
  evidence.latest.gpu_snapshots.status = "unavailable";
  evidence.latest.gpu_snapshots.count = 21;
  evidence.latest.gpu_snapshots.status_counts = { collected: 21 };
  evidence.latest.os_mutation = { status: "unavailable", ok: null, firewall_rule_count: null, eventlog_source_present: null };
  evidence.errors = [
    {
      code: "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
      message: "os_mutation evidence JSON could not be parsed.",
      detail: "[BATCH_EVIDENCE_ROOT]/os-mutation-gates-batch-profile-20260506-212527-0384/summary.json parse failed",
      retryable: false
    }
  ];
  return evidence;
}

function buildJobListFixture() {
  return [
    {
      job_id: "job-browser-fixture",
      request_id: "req-browser-fixture",
      correlation_id: "job-browser-fixture",
      operation: "vm.create",
      status: "running",
      attempt: 1,
      created_at: "2026-05-05T00:00:00.0000000Z",
      updated_at: "2026-05-05T00:00:01.0000000Z",
      error: null,
      result: null
    }
  ];
}

// The real Local API does NOT tag an auth rejection with the per-route operation
// id. Both 401 sites collapse to a single envelope:
//   src/DesktopNode.Host/DesktopNodeHostApplication.cs Json(...) -> operation "api.auth"
//   src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs -> AuthValidationFailure("api.auth", ...)
// The fixture must reproduce that contract exactly, otherwise the unauthenticated
// guard below passes against a payload shape the product never emits.
const AUTH_FAILURE_OPERATION = "api.auth";

function createFixtureFetch({
  failOpsSummary = false,
  failOpsSummaryProblemDetails = false,
  failVmListProblemDetails = false,
  failNetworkProblemDetails = false,
  diagnosticFailure = null,
  degradedOpsSummary = false,
  failAllAuth = false,
  allowLoopbackSession,
  authExpiry = null,
  batchEvidence = buildBatchEvidenceFixture(),
  jobs = buildJobListFixture()
} = {}) {
  const fetchCalls = [];
  async function fixtureFetch(rawUrl, options = {}) {
  fetchCalls.push({ url: String(rawUrl), method: String(options.method || "GET").toUpperCase() });
  const url = new URL(String(rawUrl));
  const path = url.pathname;
  const method = String(options.method || "GET").toUpperCase();

  if (path === "/api/v1/auth/loopback-session" && method === "POST") {
    if (allowLoopbackSession === false) {
      return failResponse(409, "auth.loopback-session", "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
    }
    if (allowLoopbackSession === true) {
      return ok({
        token_type: "Bearer",
        grant_type: "loopback_session",
        access_token: "loopback-access-token",
        refresh_token: "loopback-refresh-token",
        expires_in: 900,
        refresh_expires_in: 28800,
        session: { username: "loopback-session", role: "operator", subject: "loopback-session" }
      }, "auth.loopback-session");
    }
  }

  if (failAllAuth || authExpiry?.expired === true) {
    return failResponse(401, AUTH_FAILURE_OPERATION, "PCV_AUTH_REQUIRED", "Authentication is required.");
  }

  if (path === "/api/v1/auth/loopback-session" && method === "POST") {
    return ok({
      token_type: "Bearer",
      grant_type: "loopback_session",
      access_token: "loopback-access-token",
      refresh_token: "loopback-refresh-token",
      expires_in: 900,
      refresh_expires_in: 28800,
      session: { username: "loopback-session", role: "operator", subject: "loopback-session" }
    }, "auth.loopback-session");
  }

  if (path === "/api/v1/runtime/policy") {
    return ok(
      {
        auth: {
          mode: "service-token-or-account-jwt",
          token_storage: "dpapi-local-machine",
          rbac: true,
          roles: ["viewer", "operator", "admin"],
          grant_types: ["password", "refresh_token"],
          session_storage: "browser-session"
        },
        console: {
          windows_console: { type: "vmconnect", available: true },
          novnc: { enabled: false, status: "not_configured" },
          host_mutation_performed: false
        },
        network: { current_exposure: "loopback", static_asset_auth: { non_loopback: "bearer-required" } },
        job_runtime: {
          state_store: { persistence: "json-file-snapshot" },
          dispatch: { helper_boundary: "dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation" }
        },
        service: {
          hardening: {
            route_timeout_seconds: 30,
            request_limit_per_minute: 120,
            request_burst_limit: 20,
            retry_after_seconds: 15
          }
        }
      },
      "runtime.policy"
    );
  }

  if (path === "/api/v1/auth/session" && method === "GET") {
    return ok(
      {
        username: "fixture-operator",
        role: "operator",
        permissions: ["read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view"],
        expires_at: "2026-05-10T00:30:00.0000000Z"
      },
      "auth.session"
    );
  }

  if (path === "/api/v1/auth/rbac" && method === "GET") {
    return ok(
      {
        roles: {
          viewer: ["read"],
          operator: ["read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view"],
          admin: ["*"]
        },
        permissions: ["read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view"]
      },
      "auth.rbac"
    );
  }

  if (path === "/api/v1/console/capabilities" && method === "GET") {
    return ok(
      {
        windows_console: {
          type: "vmconnect",
          available: true,
          launch: "operator-handoff",
          host_mutation_performed: false
        },
        novnc: {
          enabled: false,
          status: "not_configured",
          reason: "Windows VNC/WebSocket bridge is not configured."
        },
        console_access: {
          contract: "console-access-card.v1",
          account: {
            required_permission: "console.view",
            auth_surface: "service-token-or-account-jwt",
            token_output: "redacted"
          },
          windows_console: {
            available: true,
            type: "vmconnect",
            transport: "local-handoff"
          },
          novnc: {
            enabled: false,
            status: "not_configured",
            bridge_mode: "websocket-to-vnc-tcp",
            websocket_path_template: null,
            websocket_path: null,
            reason: "Windows VNC/WebSocket bridge is not configured."
          },
          status: "local-console-handoff-ready",
          next_action: "Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.",
          host_mutation_performed: false
        },
        required_permission: "console.view"
      },
      "console.capabilities"
    );
  }

  if (path === "/api/v1/ops/summary") {
    if (failOpsSummaryProblemDetails) {
      return problemDetailsResponse(
        429,
        "rate.limit",
        "PCV_RATE_LIMIT_EXCEEDED",
        "Too Many Requests",
        {
          message: "The Local API request limit was exceeded for the current client identity.",
          client: "fixture-client"
        },
        { retry_after_seconds: 7 }
      );
    }

    if (failOpsSummary) {
      return failResponse(503, "ops.summary", "PCV_OPS_SUMMARY_UNAVAILABLE", "Ops summary unavailable.");
    }

    const summary = {
      host: { readiness: "Ready", supported: true, vmms_running: true },
      vm_counts: { total: 1, running: 0, stopped: 1, checkpoint_warnings: 0 },
      job_counts: { queued: 1, running: 0, failed: 0, succeeded: 0, canceled: 0 },
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
      runtime_policy: {
        network: { current_exposure: "loopback-fixture-summary", bind: "loopback-bind-fallback" },
        auth: { token_storage: "fixture-token-source" }
      },
      signals: [],
      recent_activity: [
        {
          job_id: "job-summary-recent-fixture",
          request_id: "req-summary-recent-fixture",
          correlation_id: "corr-summary-recent-fixture",
          operation: "vm.restart",
          status: "succeeded",
          attempt: 1,
          created_at: "2026-05-05T00:00:02.0000000Z",
          updated_at: "2026-05-05T00:00:03.0000000Z",
          result: { operation: "vm.restart" }
        }
      ],
      batch_evidence: batchEvidence,
      current_evidence: buildCurrentEvidenceFixture(batchEvidence),
      errors: []
    };

    if (degradedOpsSummary) {
      summary.signals = [{ key: "summary-errors", tone: "error", label: "Summary errors", detail: "Native reads degraded." }];
      summary.errors = [{ code: "PCV_OPS_SUMMARY_NATIVE_READ_DEGRADED", message: "Native reads degraded." }];
    }

    return ok(summary, "ops.summary");
  }

  if (path === "/api/v1/host/status") {
    return ok(
      {
        supported: true,
        admin: { elevated: true },
        hyperv: { default_switch_present: true, feature_enabled: true, vmms_running: true },
        windows: { caption: "Windows browser fixture" }
      },
      "host.status"
    );
  }

  if (path === "/api/v1/network/inventory") {
    if (failNetworkProblemDetails) {
      return problemDetailsResponse(
        500,
        "network.inventory",
        "PCV_NATIVE_PARITY_INCOMPLETE",
        "Native parity failure",
        {
          message: "The native adapter returned incomplete network inventory parity.",
          route: "/api/v1/network/inventory"
        },
        { retry_after_seconds: 15 }
      );
    }

    return ok(fixtureNetworkInventory, "network.inventory");
  }

  if (path === "/api/v1/vms") {
    if (failVmListProblemDetails) {
      return problemDetailsResponse(
        504,
        "vm.list",
        "PCV_ROUTE_TIMEOUT",
        "Route Timeout",
        {
          message: "VM list read route exceeded the configured deadline.",
          route: "/api/v1/vms"
        },
        { retry_after_seconds: 15, route_timeout_seconds: 30 }
      );
    }

    return ok(
      [fixtureVm],
      "vm.list"
    );
  }

  if (path === "/api/v1/vms/pcv-browser-fixture") {
    return ok(fixtureVm, "vm.get");
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/blkio") {
    return ok(
      {
        name: "pcv-browser-fixture",
        state: "off",
        storage_qos: {
          contract: "hyperv-storage-inventory-readback-v1",
          linux_blkio_compatible: false,
          mutation_supported: false,
          disks: fixtureVm.storage
        }
      },
      "vm.blkio-get"
    );
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/bandwidth") {
    return ok(
      {
        name: "pcv-browser-fixture",
        state: "off",
        network_qos: {
          contract: "hyperv-network-inventory-readback-v1",
          linux_bandwidth_compatible: false,
          mutation_supported: false,
          adapters: fixtureVm.network
        }
      },
      "vm.bandwidth"
    );
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/guest-agent/status") {
    return ok(
      {
        name: "pcv-browser-fixture",
        state: "off",
        guest_agent: {
          contract: "hyperv-guest-service-status-readback-v1",
          status: "vm-not-running",
          qemu_guest_agent: false,
          guest_exec_supported: false
        }
      },
      "vm.guest-agent-status"
    );
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/guest-agent/ping") {
    return ok(
      {
        name: "pcv-browser-fixture",
        state: "off",
        guest_ping: {
          contract: "hyperv-guest-service-ping-readiness-v1",
          reachable: false,
          guest_heartbeat_verified: false
        }
      },
      "vm.guest-ping"
    );
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/checkpoints") {
    return ok([], "checkpoint.list");
  }

  if (path === "/api/v1/vms/pcv-browser-fixture/console") {
    return ok(
      {
        vm_id: "pcv-browser-fixture",
        console: {
          transport: "vmconnect-handoff",
          available: true,
          launch: "operator-handoff"
        },
        novnc: {
          enabled: true,
          status: "available",
          websocket_path: "/api/v1/console/novnc/pcv-browser-fixture",
          reason: "noVNC bridge is configured."
        },
        console_access: {
          contract: "console-access-card.v1",
          account: {
            required_permission: "console.view",
            auth_surface: "service-token-or-account-jwt",
            token_output: "redacted"
          },
          windows_console: {
            available: true,
            type: "vmconnect",
            transport: "local-handoff"
          },
          novnc: {
            enabled: true,
            status: "available",
            bridge_mode: "websocket-to-vnc-tcp",
            websocket_path_template: "/api/v1/console/novnc/{vm_id}",
            websocket_path: "/api/v1/console/novnc/pcv-browser-fixture",
            reason: "noVNC bridge is configured."
          },
          status: "browser-streaming-available",
          next_action: "Open the noVNC browser session for this VM, or use vmconnect from the host console.",
          host_mutation_performed: false
        },
        host_mutation_performed: false
      },
      "console.session"
    );
  }

  if (path === "/api/v1/jobs") {
    return ok(
      {
        count: jobs.length,
        default_limit: 50,
        jobs,
        limit: 50,
        max_limit: 200,
        next_offset: null,
        offset: 0,
        retention: {
          active_jobs_preserved: true,
          max_terminal_jobs: 500,
          pruned_terminal_jobs: 0,
          terminal_statuses: ["succeeded", "failed", "canceled"]
        },
        returned: jobs.length
      },
      "job.list"
    );
  }

  if (path === "/api/v1/jobs/job-browser-fixture") {
    return ok(
      {
        job_id: "job-browser-fixture",
        request_id: "req-browser-fixture",
        correlation_id: "job-browser-fixture",
        operation: "vm.create",
        status: "running"
      },
      "job.get"
    );
  }

  if (path === "/api/v1/diagnostics/bundles" && method === "GET") {
    return ok(
      {
        actual_execution: "code-level-api-read",
        archive_status: "listed",
        authz_status: "token-required-route-contract",
        bundles: [
          {
            archive_status: "available",
            bundle_id: "pcv-diag-20260509T010101Z-abcdef12",
            created_at: "2026-05-09T01:01:01.0000000Z",
            download_url: "/api/v1/diagnostics/bundles/pcv-diag-20260509T010101Z-abcdef12/download",
            file_name: "pcv-diag-20260509T010101Z-abcdef12.bundle.json",
            redaction_status: "applied",
            size_bytes: 256
          }
        ],
        count: 1,
        default_limit: 10,
        limit: Number(url.searchParams.get("limit") || 10),
        max_limit: 100,
        next_offset: null,
        offset: Number(url.searchParams.get("offset") || 0),
        retention: {
          max_bundle_count: 50,
          removed: [],
          retention_days: 14
        },
        retention_status: "applied",
        returned: 1
      },
      "diagnostic.bundle.list"
    );
  }

  if (path === "/api/v1/diagnostics/bundles" && method === "POST") {
    if (diagnosticFailure) {
      return problemDetailsResponse(
        diagnosticFailure.status,
        "diagnostic.bundle.create",
        diagnosticFailure.code,
        diagnosticFailure.title,
        diagnosticFailure.detail,
        diagnosticFailure.extra || {}
      );
    }

    return {
      ok: true,
      status: 201,
      headers: {
        get(name) {
          return String(name).toLowerCase() === "content-type" ? "application/json" : null;
        }
      },
      async json() {
        return {
          ok: true,
          operation: "diagnostic.bundle.create",
          request_id: "req-diagnostic-fixture",
          data: {
            actual_execution: "code-level-api-action",
            archive_status: "created",
            authz_status: "token-required-route-contract",
            bundle_id: "pcv-diag-20260509T010101Z-abcdef12",
            created_at: "2026-05-09T01:01:01.0000000Z",
            download_status: "served-by-download-route",
            download_url: "/api/v1/diagnostics/bundles/pcv-diag-20260509T010101Z-abcdef12/download",
            external_stable_publication: "not-claimed",
            host_mutation_performed: false,
            public_trusted_signing: "not-claimed",
            redaction_status: "applied",
            retention_status: "applied"
          }
        };
      }
    };
  }

  if (path === "/api/v1/diagnostics/bundles/pcv-diag-20260509T010101Z-abcdef12/download" && method === "GET") {
    return {
      ok: true,
      status: 200,
      headers: {
        get(name) {
          const key = String(name).toLowerCase();
          if (key === "content-type") return "application/vnd.purecvisor.diagnostic-bundle+json";
          if (key === "content-disposition") return "attachment; filename=\"pcv-diag-20260509T010101Z-abcdef12.bundle.json\"";
          if (key === "x-pcv-diagnostic-bundle-id") return "pcv-diag-20260509T010101Z-abcdef12";
          return null;
        }
      },
      async text() {
        return JSON.stringify({
          bundle_id: "pcv-diag-20260509T010101Z-abcdef12",
          redaction_status: "applied",
          token: "[REDACTED]"
        });
      }
    };
  }

  return {
    ok: false,
    status: 404,
    async json() {
      return {
        ok: false,
        operation: "fixture.fetch",
        error: {
          code: "PCV_FIXTURE_ROUTE_MISSING",
          message: `No fixture response for ${path}`,
          detail: path,
          retryable: false
        }
      };
    }
  };
  }
  fixtureFetch.fetchCalls = fetchCalls;
  return fixtureFetch;
}

function fail(message) {
  console.error(`browser fixture verification failed: ${message}`);
  process.exit(1);
}

function requireIncludes(text, value, label) {
  if (!text.includes(value)) {
    fail(`${label} is missing ${value}`);
  }
}

function requireNotIncludes(text, value, label) {
  if (text.includes(value)) {
    fail(`${label} must not include ${value}`);
  }
}

function requireEvidencePanel(document, values, label) {
  const html = document.getElementById("evidence-panel").innerHTML;
  for (const value of values) {
    requireIncludes(html, value, label);
  }
  return html;
}

async function flush() {
  for (let i = 0; i < 64; i += 1) {
    await Promise.resolve();
  }

  await new Promise((resolve) => setTimeout(resolve, 0));
}

async function runFixture(options = {}) {
  const initialTrackedJobs = options.initialTrackedJobs ?? [{
    job_id: "job-browser-fixture",
    request_id: "req-browser-fixture",
    correlation_id: "job-browser-fixture",
    operation: "vm.create",
    status: "queued"
  }];
  const localStorage = createLocalStorage(initialTrackedJobs === null ? [] : [
    [
      "pcvDesktopTrackedJobs.v1",
      JSON.stringify(initialTrackedJobs)
    ]
  ]);
  const initialAccountSession = Object.prototype.hasOwnProperty.call(options, "initialAccountSession")
    ? options.initialAccountSession
    : {
    access_token: "fixture-access-token",
    refresh_token: "fixture-refresh-token",
    session: {
      username: "fixture-operator",
      role: "operator",
      permissions: ["read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view"],
      expires_at: "2026-05-10T00:30:00.0000000Z"
    }
  };
  const sessionStorage = createLocalStorage(initialAccountSession === null ? [] : [
    [
      "pcvDesktopAccountSession.v1",
      JSON.stringify(initialAccountSession)
    ]
  ]);
  const document = createDocument();
  const windowListeners = new Map();
  const fixtureFetch = createFixtureFetch(options);
  const window = {
    AbortController,
    addEventListener(type, handler) {
      const handlers = windowListeners.get(type) ?? [];
      handlers.push(handler);
      windowListeners.set(type, handlers);
    },
    clearInterval() {},
    confirm: () => true,
    dispatch(type) {
      for (const handler of windowListeners.get(type) ?? []) {
        handler();
      }
    },
    fetch: fixtureFetch,
    Headers: FixtureHeaders,
    localStorage,
    sessionStorage,
    location: { origin: "http://127.0.0.1:7777", hash: "#dashboard", hostname: options.locationHostname || "127.0.0.1" },
    setInterval: () => 1
  };

  const context = {
    AbortController,
    FormData: class FixtureFormData {
      get() {
        return "";
      }
    },
    clearInterval: window.clearInterval,
    console,
    document,
    fetch: fixtureFetch,
    Headers: FixtureHeaders,
    localStorage,
    sessionStorage,
    setInterval: window.setInterval,
    window
  };

  vm.runInNewContext(appScript, context, { filename: "app.js" });
  document.dispatch("DOMContentLoaded");
  await flush();

  if (!options.skipVmSelect) {
    const vmTable = document.getElementById("vm-table");
    for (const handler of vmTable.listeners.get("click") ?? []) {
      await handler({
        target: {
          closest(selector) {
            if (selector === 'button[data-action="select-vm"]' || selector === "tr[data-vm-id]") {
              return { dataset: { vmId: "pcv-browser-fixture" } };
            }
            return null;
          }
        }
      });
    }
  }
  await flush();

  window.__pcvFetchCalls = fixtureFetch.fetchCalls;
  return { document, window, context, fetchCalls: fixtureFetch.fetchCalls };
}

const { document, window, context } = await runFixture();
if (typeof context.createDiagnosticBundleFromPanel !== "function" || typeof context.downloadLatestDiagnosticBundle !== "function") {
  fail("diagnostic bundle panel action functions must be available to the browser fixture");
}
if (typeof context.openSelectedConsole !== "function") {
  fail("console handoff action function must be available to the browser fixture");
}
await context.openSelectedConsole();
await context.createDiagnosticBundleFromPanel();
await context.downloadLatestDiagnosticBundle();
await flush();

const renderedText = [...document.elements.values()]
  .map((element) => `${element.textContent}\n${element.innerHTML}`)
  .join("\n");
const combinedText = `${indexText}\n${renderedText}`;

for (const value of ["Ops Cockpit", "Ready", "Network Inventory", "Default Switch", "fixture-ethernet", "read-only", "pcv-browser-fixture", "Delete VM", "QoS / Guest Readback", "linux_blkio_compatible=false", "linux_bandwidth_compatible=false", "qemu_guest_agent=false", "guest_heartbeat_verified=false", "job-browser-fixture", "req-browser-fixture", "Operator Activity", "Pagination", "next_offset", "max_terminal_jobs", "Job edge cases", "active running jobs", "failed job retry", "retained terminal jobs", "Monitoring", "Token policy", "Checkpoint warnings", "Route timeout", "30s", "Request limit", "120/min", "Burst limit", "20", "Retry-After", "15s", "Incident Command", "Failed jobs", "Token Rotation", "rotation handoff", "Clear browser token", "no service token mutation", "browser token empty", "Token-required route status", "%ProgramData%\\PureCVisor\\desktop-node\\api-token.dpapi.json", "Diagnostic Bundle", "CollectDiagnostics", "API action", "Create bundle", "Download latest", "Account/Console", "account permission", "current role", "Windows console", "noVNC status", "noVNC path/reason", "/api/v1/console/novnc/pcv-browser-fixture", "Open selected console", "Open the noVNC browser session for this VM, or use vmconnect from the host console.", "Current evidence", "runtime-api-current-evidence-rollup-v1", "0.42.22-admin-smoke -&gt; 0.42.23-admin-smoke", "Manual admin next", "0.42.55-admin-smoke -&gt; next-admin-smoke-required", "not-opened-no-next-product-payload-target", "Runtime/API registry bridge", "runtime-api-diagnostics-ops-summary-registry-bridge-v2", "DesktopNodeApiRuntimeRoutes", "4 routes", "GET /api/v1/ops/summary -&gt; OpsSummary", "GET /api/v1/diagnostics/bundles", "POST /api/v1/diagnostics/bundles -&gt; CreateDiagnosticBundle", "route detail metadata only", "Host Ops lifecycle buckets", "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated", "service-action", "DesktopNodeServiceLifecycleOps", "windows-service-control-manager", "credential-manager-system-proof", "allowlisted-programdata-root", "Host mutation: not performed by diagnostics view", "Retained bundles", "Load more bundles", "next_offset=none", "max_bundle_count=50", "pcv-diag-20260509T010101Z-abcdef12", "served-by-download-route", "application/vnd.purecvisor.diagnostic-bundle+json", "no host mutation", "token values", "Authorization headers", "%ProgramData%\\PureCVisor\\desktop-node\\diagnostics", "Installed listener QA automation", "service token revoke handoff", "diagnostic retention pagination", "VM delete guarded", "ops cockpit P0/P1/P2", "public distribution bundle", "host mutation not started from browser", "PCV_JOB_STORE_SCHEMA_UNSUPPORTED", "Asset", "app.js"]) {
  requireIncludes(combinedText, value, "browser fixture rendered output");
}

requireNotIncludes(document.getElementById("priority-panel").innerHTML, "High priority", "priority panel");
requireIncludes(document.getElementById("dashboard-activity-panel").innerHTML, "job-summary-recent-fixture", "dashboard activity panel");
requireIncludes(document.getElementById("dashboard-activity-panel").innerHTML, "vm.restart", "dashboard activity panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "Ready", "ops summary panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "Checkpoint warnings", "ops summary panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "loopback-fixture-summary", "ops summary panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "fixture-token-source", "ops summary panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "Latest evidence", "ops summary panel");
requireIncludes(document.getElementById("ops-summary-panel").innerHTML, "full-admin-host-mutation-gate-20260506-212527-0384", "ops summary panel");
requireIncludes(document.getElementById("incident-panel").innerHTML, "Failed jobs", "incident panel");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "Token Rotation", "token rotation panel");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "Clear browser token", "token rotation panel");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "no service token mutation", "token rotation panel");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "%ProgramData%\\PureCVisor\\desktop-node\\api-token.dpapi.json", "token rotation panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Diagnostic Bundle", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "CollectDiagnostics", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "API action", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Create bundle", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Download latest", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Runtime/API registry bridge", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "runtime-api-diagnostics-ops-summary-registry-bridge-v2", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "DesktopNodeApiRuntimeRoutes", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "4 routes", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "GET /api/v1/ops/summary -&gt; OpsSummary", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "route detail metadata only", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Host Ops lifecycle buckets", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "DesktopNodeCredentialManagerOps", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "credential-manager-system-proof", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "allowlisted-programdata-root", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "Host mutation: not performed by diagnostics view", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "pcv-diag-20260509T010101Z-abcdef12", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "served-by-download-route", "diagnostics panel");
requireIncludes(document.getElementById("diagnostics-panel").innerHTML, "no host mutation", "diagnostics panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "Installed listener QA automation", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "service token revoke handoff", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "diagnostic retention pagination", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "VM delete guarded", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "ops cockpit P0/P1/P2", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "public distribution bundle", "beta follow-up panel");
requireIncludes(document.getElementById("beta-followup-panel").innerHTML, "host mutation not started from browser", "beta follow-up panel");
await context.clearBrowserToken();
await flush();
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "browser token cleared", "token clear refresh");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "all views refresh", "token clear refresh");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "pending jobs are rechecked", "token clear refresh");
requireIncludes(document.getElementById("token-rotation-panel").innerHTML, "token-required routes may show Auth required", "token clear refresh");
requireEvidencePanel(document, [
  "full-admin-host-mutation-gate-20260506-212527-0384",
  "0.38.4-admin-smoke",
  "AllowUnsignedDev",
  "GPU snapshots",
  "22",
  "Route/MSI",
  "OS gate",
  "service-msi-hyperv-admin-smoke",
  "os-mutation-gate",
  "Public signing: excluded",
  "External publication: not-claimed",
  "Running"
], "evidence panel");

window.location.hash = "#vms";
window.dispatch("hashchange");
if (document.getElementById("vms").hidden) {
  fail("vms app view must be visible after hash navigation");
}
if (!document.getElementById("dashboard").hidden) {
  fail("dashboard app view must be hidden after vms hash navigation");
}
const vmNavLink = document.viewLinks.find((link) => link.dataset.viewLink === "vms");
if (vmNavLink.className !== "nav-active") {
  fail("vms nav link must be marked nav-active after hash navigation");
}
if (vmNavLink.attributes["aria-current"] !== "page") {
  fail("vms nav link must set aria-current page after hash navigation");
}

window.location.hash = "#network";
window.dispatch("hashchange");
if (document.getElementById("network").hidden) {
  fail("network app view must be visible after hash navigation");
}
if (!document.getElementById("vms").hidden) {
  fail("vms app view must be hidden after network hash navigation");
}
const networkNavLink = document.viewLinks.find((link) => link.dataset.viewLink === "network");
if (networkNavLink.className !== "nav-active") {
  fail("network nav link must be marked nav-active after hash navigation");
}
if (networkNavLink.attributes["aria-current"] !== "page") {
  fail("network nav link must set aria-current page after hash navigation");
}

window.location.hash = "#evidence";
window.dispatch("hashchange");
if (document.getElementById("evidence").hidden) {
  fail("evidence app view must be visible after hash navigation");
}
if (!document.getElementById("network").hidden) {
  fail("network app view must be hidden after evidence hash navigation");
}
const evidenceNavLink = document.viewLinks.find((link) => link.dataset.viewLink === "evidence");
if (evidenceNavLink.className !== "nav-active") {
  fail("evidence nav link must be marked nav-active after hash navigation");
}
if (evidenceNavLink.attributes["aria-current"] !== "page") {
  fail("evidence nav link must set aria-current page after hash navigation");
}

const degradedRun = await runFixture({ failOpsSummary: true });
const degradedText = [...degradedRun.document.elements.values()]
  .map((element) => `${element.textContent}\n${element.innerHTML}`)
  .join("\n");
requireIncludes(degradedRun.document.getElementById("priority-panel").innerHTML, "Ops summary unavailable", "degraded priority panel");
requireIncludes(degradedRun.document.getElementById("priority-panel").innerHTML, "PCV_OPS_SUMMARY_UNAVAILABLE", "degraded priority panel");
for (const value of ["Ready", "pcv-browser-fixture", "job-browser-fixture", "loopback", "Token policy"]) {
  requireIncludes(degradedText, value, "degraded browser fixture fallback output");
}
requireEvidencePanel(degradedRun.document, ["not configured", "Batch evidence root is not configured."], "not configured evidence panel");

const rateLimitRun = await runFixture({ failOpsSummaryProblemDetails: true });
const rateLimitPriority = rateLimitRun.document.getElementById("priority-panel").innerHTML;
requireIncludes(rateLimitPriority, "PCV_RATE_LIMIT_EXCEEDED", "problem-details priority panel");
requireIncludes(rateLimitPriority, "Too Many Requests", "problem-details priority panel");
requireIncludes(rateLimitPriority, "fixture-client", "problem-details priority panel");
requireIncludes(rateLimitPriority, "retry_after_seconds=7", "problem-details priority panel");
requireNotIncludes(rateLimitPriority, "PCV_NETWORK_ERROR", "problem-details priority panel");
requireNotIncludes(rateLimitPriority, "[object Object]", "problem-details priority panel");

const partialFailureRun = await runFixture({ failVmListProblemDetails: true, skipVmSelect: true });
const partialFailureAlert = partialFailureRun.document.getElementById("alert-region").innerHTML;
const partialFailureConnection = partialFailureRun.document.getElementById("connection-state").textContent;
requireIncludes(partialFailureAlert, "PCV_PARTIAL_REFRESH_DEGRADED", "partial failure alert");
requireIncludes(partialFailureAlert, "PCV_ROUTE_TIMEOUT", "partial failure alert");
requireIncludes(partialFailureAlert, "retry_after_seconds=15", "partial failure alert");
requireIncludes(partialFailureAlert, "route_timeout_seconds=30", "partial failure alert");
requireIncludes(partialFailureAlert, "req-problem-details-fixture", "partial failure alert");
requireIncludes(partialFailureConnection, "Degraded", "partial failure connection state");
requireIncludes(partialFailureRun.document.getElementById("host-details").innerHTML, "Windows", "partial failure host panel");
// The footer connection text and the #connection-state badge must never contradict
// each other. Before this assertion existed the badge read "Degraded" while the
// footer collapsed every non-connected state to "Not connected".
const partialFailureStatusConnection = partialFailureRun.document.getElementById("status-connection").textContent;
requireIncludes(partialFailureStatusConnection, "Degraded", "partial failure status bar connection");
if (partialFailureStatusConnection !== partialFailureConnection) {
  fail(`status bar connection "${partialFailureStatusConnection}" must match connection badge "${partialFailureConnection}"`);
}
// Partial failure shows only the routes that loaded: host.status succeeded, vm.list did not.
requireIncludes(partialFailureRun.document.getElementById("status-host").textContent, "Windows browser fixture", "partial failure status bar host");
requireIncludes(partialFailureRun.document.getElementById("status-vm-count").textContent, "VM: —", "partial failure status bar vm count");
requireIncludes(partialFailureRun.document.getElementById("hero-workload").textContent, "—", "partial failure hero workload");

const emptyJobsRun = await runFixture({ jobs: [], initialTrackedJobs: [] });
const emptyJobsHtml = emptyJobsRun.document.getElementById("jobs-panel").innerHTML;
requireIncludes(emptyJobsHtml, "No jobs on this page", "empty jobs panel");
requireIncludes(emptyJobsHtml, "retained terminal jobs", "empty jobs panel");
requireIncludes(emptyJobsHtml, "failed job retry", "empty jobs panel");

const networkFailureRun = await runFixture({ failNetworkProblemDetails: true });
const networkFailureHtml = networkFailureRun.document.getElementById("network-inventory-panel").innerHTML;
requireIncludes(networkFailureHtml, "PCV_NATIVE_PARITY_INCOMPLETE", "network native parity failure");
requireIncludes(networkFailureHtml, "native parity failure", "network native parity failure");
requireIncludes(networkFailureHtml, "helper fallback is intentionally excluded", "network native parity failure");

const diagnosticUnsupportedRun = await runFixture({
  diagnosticFailure: {
    status: 404,
    code: "PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED",
    title: "Diagnostic bundle API unsupported",
    detail: { message: "Installed listener does not expose diagnostic bundle API." }
  }
});
await diagnosticUnsupportedRun.context.createDiagnosticBundleFromPanel();
await flush();
const unsupportedDiagnosticsHtml = diagnosticUnsupportedRun.document.getElementById("diagnostics-panel").innerHTML;
requireIncludes(unsupportedDiagnosticsHtml, "PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED", "diagnostic unsupported panel");
requireIncludes(unsupportedDiagnosticsHtml, "404 PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED", "diagnostic unsupported panel");
requireIncludes(unsupportedDiagnosticsHtml, "API unsupported", "diagnostic unsupported panel");
requireNotIncludes(unsupportedDiagnosticsHtml, "diagnostic-retry", "diagnostic unsupported panel");

const diagnosticAuthRun = await runFixture({
  diagnosticFailure: {
    status: 401,
    code: "PCV_AUTH_REQUIRED",
    title: "Auth required",
    detail: { message: "Token is required." },
    extra: { retry_after_seconds: 5 }
  }
});
await diagnosticAuthRun.context.createDiagnosticBundleFromPanel();
await flush();
const authDiagnosticsHtml = diagnosticAuthRun.document.getElementById("diagnostics-panel").innerHTML;
requireIncludes(authDiagnosticsHtml, "401 auth required", "diagnostic auth panel");
requireIncludes(authDiagnosticsHtml, "Retry action", "diagnostic auth panel");

const diagnosticServerRun = await runFixture({
  diagnosticFailure: {
    status: 500,
    code: "PCV_DIAGNOSTIC_BUNDLE_SERVER_FAILED",
    title: "Diagnostic bundle server failed",
    detail: { message: "Server-side bundle generation failed." }
  }
});
await diagnosticServerRun.context.createDiagnosticBundleFromPanel();
await flush();
requireIncludes(diagnosticServerRun.document.getElementById("diagnostics-panel").innerHTML, "500 server failure", "diagnostic 500 panel");

const diagnosticTimeoutRun = await runFixture({
  diagnosticFailure: {
    status: 504,
    code: "PCV_ROUTE_TIMEOUT",
    title: "Route Timeout",
    detail: { message: "Diagnostic bundle route timeout." },
    extra: { retry_after_seconds: 15, route_timeout_seconds: 30 }
  }
});
await diagnosticTimeoutRun.context.createDiagnosticBundleFromPanel();
await flush();
requireIncludes(diagnosticTimeoutRun.document.getElementById("diagnostics-panel").innerHTML, "timeout: retry", "diagnostic timeout panel");

const degradedSuccessRun = await runFixture({ degradedOpsSummary: true });
const degradedSuccessPriority = degradedSuccessRun.document.getElementById("priority-panel").innerHTML;
requireIncludes(degradedSuccessPriority, "PCV_OPS_SUMMARY_NATIVE_READ_DEGRADED", "degraded successful priority panel");
requireIncludes(degradedSuccessPriority, "Native reads degraded.", "degraded successful priority panel");
requireNotIncludes(degradedSuccessPriority, "No high-priority warnings", "degraded successful priority panel");
requireIncludes(degradedSuccessRun.document.getElementById("dashboard-activity-panel").innerHTML, "job-summary-recent-fixture", "degraded successful dashboard activity panel");

const missingEvidenceRun = await runFixture({
  batchEvidence: {
    schema_version: 1,
    configured: true,
    status: "missing",
    latest: null,
    errors: [{ code: "PCV_BATCH_EVIDENCE_MISSING", message: "No batch supervisor summary was found." }]
  }
});
requireEvidencePanel(missingEvidenceRun.document, [
  "missing",
  "Configured evidence root has no readable batch summary.",
  "PCV_BATCH_EVIDENCE_MISSING",
  "No step evidence is available."
], "missing evidence panel");

const unavailableEvidenceRun = await runFixture({
  batchEvidence: {
    schema_version: 1,
    configured: true,
    status: "unavailable",
    latest: null,
    errors: [{ code: "PCV_BATCH_EVIDENCE_UNAVAILABLE", message: "Batch evidence summary could not be read." }]
  }
});
requireEvidencePanel(unavailableEvidenceRun.document, [
  "unavailable",
  "Batch evidence summary could not be read.",
  "PCV_BATCH_EVIDENCE_UNAVAILABLE",
  "No step evidence is available."
], "unavailable evidence panel");

const degradedEvidenceRun = await runFixture({
  batchEvidence: buildDegradedBatchEvidenceFixture()
});
const degradedEvidenceHtml = requireEvidencePanel(degradedEvidenceRun.document, [
  "degraded",
  "Latest batch supervisor evidence is partial or malformed.",
  "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
  "os_mutation evidence JSON could not be parsed.",
  "[BATCH_EVIDENCE_ROOT]/os-mutation-gates-batch-profile-20260506-212527-0384/summary.json parse failed"
], "degraded evidence panel");
for (const value of ["D:\\", "secret-token-value", "secret-bearer-value", "ApiTokenProtectedFile"]) {
  requireNotIncludes(degradedEvidenceHtml, value, "degraded evidence panel");
}

const malformedEvidenceRun = await runFixture({ batchEvidence: [] });
requireEvidencePanel(malformedEvidenceRun.document, ["not configured", "Batch evidence root is not configured."], "malformed evidence panel");

const escapedEvidenceRun = await runFixture({
  batchEvidence: {
    schema_version: 1,
    configured: true,
    status: "available",
    latest: {
      batch_id: "<img src=x onerror=alert(1)>",
      status: "completed",
      steps: [{ step_id: "<script>alert(1)</script>", ok: false, timed_out: true }],
      gpu_snapshots: { count: 1 },
      release: { version: "<b>0.0.0</b>", signing_mode: "AllowUnsignedDev" },
      route_msi_hyperv: { ok: false },
      os_mutation: { ok: false },
      host_final_state: { service_state: "<b>Running</b>" }
    },
    errors: [{ code: "PCV_BATCH_EVIDENCE_ESCAPE", message: "<b>must escape</b>" }]
  }
});
const escapedEvidenceHtml = requireEvidencePanel(escapedEvidenceRun.document, [
  "&lt;img src=x onerror=alert(1)&gt;",
  "&lt;script&gt;alert(1)&lt;/script&gt;",
  "&lt;b&gt;0.0.0&lt;/b&gt;",
  "&lt;b&gt;must escape&lt;/b&gt;"
], "escaped evidence panel");
for (const value of ["<img src=x onerror=alert(1)>", "<script>alert(1)</script>", "<b>must escape</b>"]) {
  requireNotIncludes(escapedEvidenceHtml, value, "escaped evidence panel");
}

const troubleshootingTriageRun = await runFixture({
  batchEvidence: {
    schema_version: 1,
    configured: true,
    status: "unavailable",
    latest: null,
    errors: [
      {
        code: "PCV_BATCH_EVIDENCE_PARSE_FAILED",
        message: "Batch evidence JSON could not be parsed.",
        detail: "summary.json parse failed <b>redacted</b>",
        retryable: false
      }
    ]
  },
  jobs: [
    {
      job_id: "job-failed-retryable-fixture",
      request_id: "req-failed-retryable-fixture",
      correlation_id: "corr-failed-retryable-fixture",
      operation: "vm.restart",
      status: "failed",
      attempt: 1,
      created_at: "2026-05-05T00:00:04.0000000Z",
      updated_at: "2026-05-05T00:00:05.0000000Z",
      error: {
        code: "PCV_JOB_RETRYABLE_FIXTURE",
        message: "Retryable fixture failure.",
        retryable: true
      },
      result: null
    }
  ]
});
const troubleshootingHtml = troubleshootingTriageRun.document.getElementById("troubleshooting-panel").innerHTML;
const incidentHtml = troubleshootingTriageRun.document.getElementById("incident-panel").innerHTML;
for (const value of [
  "Batch evidence",
  "PCV_BATCH_EVIDENCE_PARSE_FAILED",
  "Batch evidence JSON could not be parsed.",
  "summary.json parse failed &lt;b&gt;redacted&lt;/b&gt;",
  "Public signing: excluded",
  "External publication: not-claimed"
]) {
  requireIncludes(troubleshootingHtml, value, "troubleshooting evidence triage");
}
for (const value of [
  "1 failed / 1 retryable",
  "job-failed-retryable-fixture",
  "vm.restart",
  "PCV_JOB_RETRYABLE_FIXTURE",
  "retryable",
  "Evidence issues",
  "PCV_BATCH_EVIDENCE_PARSE_FAILED",
  "summary.json parse failed &lt;b&gt;redacted&lt;/b&gt;"
]) {
  requireIncludes(incidentHtml, value, "incident evidence and failed job triage");
}
for (const value of ["summary.json parse failed <b>redacted</b>", "<b>redacted</b>"]) {
  requireNotIncludes(`${troubleshootingHtml}\n${incidentHtml}`, value, "troubleshooting evidence triage");
}

for (const value of ["PCV_NETWORK_ERROR", "Network request failed", "PCV_FIXTURE_ROUTE_MISSING"]) {
  requireNotIncludes(renderedText, value, "browser fixture rendered output");
}

const guardedTerms = [
  ["Restart", "-", "Computer"],
  ["msi", "exec"],
  ["New", "-", "Net", "Firewall", "Rule"],
  ["Register", "-", "Event", "Source"],
  ["New", "-", "Event", "Log"],
  ["Register", "-", "Scheduled", "Task"],
  ["New", "-", "VM"],
  ["Remove", "-", "VM"]
].map((parts) => parts.join(""));
const tokenValuePattern = new RegExp(["Bearer", "\\s+[A-Za-z0-9._~+/=-]+"].join(""));

if (tokenValuePattern.test(renderedText)) {
  fail("rendered output must not contain auth secrets");
}

for (const term of guardedTerms) {
  requireNotIncludes(renderedText, term, "browser fixture rendered output");
}

for (const value of ["journalctl", "libvirt", "purecvisorsd"]) {
  requireNotIncludes(renderedText, value, "browser fixture rendered output");
}

for (const value of ["Create Linux VM", "ubuntu-24.04"]) {
  requireNotIncludes(combinedText, value, "browser fixture rendered output");
}

const unauthenticatedRun = await runFixture({ failAllAuth: true });
const unauthDoc = unauthenticatedRun.document;

const unauthFooter = [
  "status-connection",
  "status-host",
  "status-updated",
  "status-vm-count"
].map((id) => unauthDoc.getElementById(id).textContent).join(" | ");

requireIncludes(unauthFooter, "Not connected", "unauthenticated status bar");
requireNotIncludes(unauthFooter, "pcv-node-a", "unauthenticated status bar");
requireNotIncludes(unauthFooter, "VM: 3/3", "unauthenticated status bar");
requireIncludes(unauthFooter, "VM: —", "unauthenticated status bar");
// "Updated" is a "last refresh attempt" timestamp, not a "last successful load"
// timestamp (state.lastRefreshedAt is set on every completed refresh, success or
// failure, so hasRefreshedOperation() can tell "never synced" apart from "synced,
// and this specific field failed"). A completed-but-fully-failed refresh legitimately
// shows "Updated Xs ago" alongside "—" everywhere else: that is an honest report of
// when the attempt happened, not fabricated operational data, so it is intentionally
// not asserted to be "—" here.
requireIncludes(unauthFooter, "Updated ", "unauthenticated status bar");
requireIncludes(unauthFooter, "ago", "unauthenticated status bar");

// The Local API answers every 401 with operation "api.auth", so a per-route
// operation match cannot tell which route failed. An auth failure must invalidate
// every gated value; otherwise these read back as "VM: 0/0" and "Ready".
requireNotIncludes(unauthFooter, "VM: 0/0", "unauthenticated status bar");
requireIncludes(unauthDoc.getElementById("status-host").textContent, "—", "unauthenticated status bar");

const unauthHero = [
  "hero-workload",
  "hero-host-mode",
  "hero-alerts"
].map((id) => unauthDoc.getElementById(id).textContent).join(" | ");

requireNotIncludes(unauthHero, "4/5", "unauthenticated hero chips");
requireNotIncludes(unauthHero, "Ready", "unauthenticated hero chips");
requireNotIncludes(unauthHero, "0/0", "unauthenticated hero chips");
if (unauthDoc.getElementById("hero-workload").textContent !== "—") {
  fail(`unauthenticated hero workload must be — but was "${unauthDoc.getElementById("hero-workload").textContent}"`);
}
if (unauthDoc.getElementById("hero-host-mode").textContent !== "—") {
  fail(`unauthenticated hero host mode must be — but was "${unauthDoc.getElementById("hero-host-mode").textContent}"`);
}

// The Metrics panel and Ops Cockpit read the same host-readiness and VM-count
// helpers as the footer and hero chips. Under a dead API they must not report
// a healthy host or a definitive zero inventory either.
// Assertions name the exact card so they cannot be satisfied — or defeated — by
// an unrelated card that happens to hold the same digits. The Jobs card also
// renders an "N / N" pair, so a bare "0 / 0" check would be ambiguous.
const unauthMetrics = unauthDoc.getElementById("metric-grid").innerHTML;
requireNotIncludes(unauthMetrics, "Ready", "unauthenticated metric grid");
requireIncludes(unauthMetrics, '<span class="muted">Host</span><strong>—</strong>', "unauthenticated metric grid");
requireIncludes(unauthMetrics, '<span class="muted">VMs</span><strong>—</strong>', "unauthenticated metric grid");
requireIncludes(unauthMetrics, '<span class="muted">Running</span><strong>—</strong>', "unauthenticated metric grid");

const unauthCockpit = unauthDoc.getElementById("ops-summary-panel").innerHTML;
requireNotIncludes(unauthCockpit, "Ready", "unauthenticated ops cockpit");
requireIncludes(unauthCockpit, '<span class="muted">Host readiness</span><strong>—</strong>', "unauthenticated ops cockpit");
requireIncludes(unauthCockpit, '<span class="muted">VMs total/running</span><strong>—</strong>', "unauthenticated ops cockpit");
requireIncludes(unauthCockpit, '<span class="muted">Checkpoint warnings</span><strong>—</strong>', "unauthenticated ops cockpit");

// Stale case: the host and VM list load successfully at T1, then the session
// expires at T2. The load gate must stop presenting the T1 values as current.
const sessionExpiry = { expired: false };
const sessionExpiryRun = await runFixture({ authExpiry: sessionExpiry, skipVmSelect: true });
const loadedFooter = [
  "status-connection",
  "status-host",
  "status-vm-count"
].map((id) => sessionExpiryRun.document.getElementById(id).textContent).join(" | ");
requireIncludes(loadedFooter, "Connected", "authenticated status bar");
requireIncludes(loadedFooter, "Windows browser fixture", "authenticated status bar");
requireIncludes(loadedFooter, "VM: 0/1", "authenticated status bar");
requireIncludes(sessionExpiryRun.document.getElementById("hero-host-mode").textContent, "Ready", "authenticated hero chips");

sessionExpiry.expired = true;
await sessionExpiryRun.context.refreshAll();
await flush();
const staleFooter = [
  "status-connection",
  "status-host",
  "status-vm-count"
].map((id) => sessionExpiryRun.document.getElementById(id).textContent).join(" | ");
requireIncludes(staleFooter, "Not connected", "expired-session status bar");
requireNotIncludes(staleFooter, "Windows browser fixture", "expired-session status bar");
requireNotIncludes(staleFooter, "VM: 0/1", "expired-session status bar");
requireIncludes(staleFooter, "VM: —", "expired-session status bar");
if (sessionExpiryRun.document.getElementById("hero-host-mode").textContent !== "—") {
  fail(`expired-session hero host mode must be — but was "${sessionExpiryRun.document.getElementById("hero-host-mode").textContent}"`);
}
if (sessionExpiryRun.document.getElementById("hero-workload").textContent !== "—") {
  fail(`expired-session hero workload must be — but was "${sessionExpiryRun.document.getElementById("hero-workload").textContent}"`);
}

const noSessionRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1"
});
const noSessionCalls = noSessionRun.fetchCalls.filter((call) => !call.url.includes("/api/v1/auth/loopback-session"));
if (noSessionCalls.some((call) => call.url.includes("/api/v1/ops/summary") || call.url.includes("/api/v1/vms") || call.url.includes("/api/v1/host/status"))) {
  fail("refreshAll must not fan out protected routes without credentials");
}

const lanRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  // public-safety: synthetic-rfc1918
  locationHostname: "192.168.1.10"
});
if (lanRun.fetchCalls.some((call) => call.url.includes("/api/v1/auth/loopback-session"))) {
  fail("non-loopback hostname must not POST loopback-session");
}

const bootstrapRun = await runFixture({
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1",
  allowLoopbackSession: true
});
if (!bootstrapRun.fetchCalls.some((call) => call.url.includes("/api/v1/auth/loopback-session") && call.method === "POST")) {
  fail("loopback hostname with empty session must POST loopback-session");
}
if (!bootstrapRun.fetchCalls.some((call) => call.url.includes("/api/v1/ops/summary"))) {
  fail("successful loopback session must continue refreshAll");
}

const gateRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1"
});
const alertHtml = gateRun.document.getElementById("alert-region").innerHTML;
requireIncludes(alertHtml, "account-login-form", "auth gate");
requireIncludes(alertHtml, "api-token", "auth gate points at browser token");

console.log("browser fixture verification passed");

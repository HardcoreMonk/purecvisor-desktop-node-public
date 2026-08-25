import assert from "node:assert/strict";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError,
  createWebContractContext
} from "../contracts/web-contract-harness.mjs";
import * as staticContracts from "../contracts/web-static-contracts.mjs";

const repositoryRoot = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
  ".."
);

function readCanonical(relativePath) {
  return createWebContractContext({ repoRoot: repositoryRoot }).readText(relativePath);
}

function removeOnce(relativePath, needle) {
  const canonical = readCanonical(relativePath);
  assert.ok(canonical.includes(needle), `canonical needle missing: ${relativePath}`);
  return new Map([[relativePath, canonical.replace(needle, "")]]);
}

function removeAll(relativePath, needle) {
  const canonical = readCanonical(relativePath);
  assert.ok(canonical.includes(needle), `canonical needle missing: ${relativePath}`);
  return new Map([[relativePath, canonical.replaceAll(needle, "")]]);
}

function replaceExactlyOnce(relativePath, needle, replacement) {
  const canonical = readCanonical(relativePath);
  assert.equal(
    canonical.split(needle).length - 1,
    1,
    `canonical occurrence count mismatch: ${relativePath}`
  );
  return new Map([[relativePath, canonical.replace(needle, replacement)]]);
}

function transformJson(relativePath, transform) {
  const canonical = JSON.parse(readCanonical(relativePath));
  const transformed = transform(canonical);
  const projected = transformed === undefined ? canonical : transformed;
  return new Map([[relativePath, `${JSON.stringify(projected, null, 2)}\n`]]);
}

const assertionFailed = WEB_CONTRACT_ERROR_CODES.assertionFailed;
const ledgerPath = "config/desktop-node-feature-surface-ledger.json";
const schemaPath = "config/desktop-node-feature-surface-ledger.schema.json";
const batchPlanPath = "docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json";
const ownerFailed = WEB_CONTRACT_ERROR_CODES.ownerFailed;

const defectCases = [
  {
    name: "node syntax owner failure",
    id: "web.static.javascript-syntax",
    ownerIds: ["node-check"],
    processRunner: async () => ({ exitCode: 1, signal: null, timedOut: false, stdout: "", stderr: "injected node --check failure" }),
    expectedCode: ownerFailed
  },
  {
    name: "missing served asset check script",
    id: "web.static.served-typescript-output",
    caseVariantOverrides: () => transformJson("web/package.json", (packageJson) => {
      packageJson.scripts.test = packageJson.scripts.test.replace("check:served", "CHECK:SERVED");
    }),
    overrides: () => transformJson("web/package.json", (packageJson) => { delete packageJson.scripts["check:served"]; }),
    expectedCode: assertionFailed,
    expectedLabel: "served-typescript-output:check-served"
  },
  {
    name: "incorrect exact TypeScript test script",
    id: "web.static.typescript-scaffold",
    caseVariantOverrides: () => replaceExactlyOnce("web/index.html", "app.js", "APP.JS"),
    overrides: () => transformJson("web/package.json", (packageJson) => { packageJson.scripts.test = "npm run check:served"; }),
    expectedCode: assertionFailed,
    expectedLabel: "typescript-scaffold:test-script"
  },
  {
    name: "missing RuntimePolicyResponse contract mirror",
    id: "web.static.typescript-contract-mirror",
    caseVariantOverrides: () => replaceExactlyOnce("web/src/api-types.ts", "RuntimePolicyResponse", "runtimepolicyresponse"),
    overrides: () => removeOnce("web/src/api-types.ts", "RuntimePolicyResponse"),
    expectedCode: assertionFailed,
    expectedLabel: "typescript-contract-mirror:runtime-policy-response"
  },
  {
    name: "missing runtime policy parity route",
    id: "web.static.parity-manifest",
    caseVariantOverrides: () => replaceExactlyOnce("web/index.html", "/app.js", "/APP.JS"),
    collectionVariantOverrides: () => transformJson("web/generated/parity/static-asset-parity.manifest.json", (manifest) => {
      manifest.userVisibleFixtureNames = "emptyInventory runningVmAndJob unsupportedHost";
    }),
    overrides: () => transformJson("web/generated/parity/static-asset-parity.manifest.json", (manifest) => { delete manifest.localApiRoutes.runtimePolicy; }),
    expectedCode: assertionFailed,
    expectedLabel: "parity-manifest:runtime-policy-route"
  },
  {
    name: "missing empty inventory fixture",
    id: "web.static.user-visible-fixtures",
    caseVariantOverrides: () => new Map([["web/src/user-visible-fixtures.ts", readCanonical("web/src/user-visible-fixtures.ts").replaceAll("emptyInventory", "EMPTYINVENTORY")]]),
    overrides: () => removeAll("web/src/user-visible-fixtures.ts", "emptyInventory"),
    expectedCode: assertionFailed,
    expectedLabel: "user-visible-fixtures:empty-inventory"
  },
  {
    name: "missing parity verification script",
    id: "web.static.verifier-wiring",
    caseVariantOverrides: () => replaceExactlyOnce("web/scripts/build-served-asset.mjs", "typescript", "TYPESCRIPT"),
    overrides: () => transformJson("web/package.json", (packageJson) => { delete packageJson.scripts["verify:parity"]; }),
    expectedCode: assertionFailed,
    expectedLabel: "verifier-wiring:verify-parity-script"
  },
  {
    name: "mutating browser fixture manifest",
    id: "web.static.generated-parity-alignment",
    caseVariantOverrides: () => replaceExactlyOnce("web/index.html", "<script src=\"/app.js\" defer></script>", "<SCRIPT src=\"/APP.JS\" defer></SCRIPT>"),
    overrides: () => transformJson("web/generated/parity/static-asset-parity.manifest.json", (manifest) => { manifest.browserFixture.mutating = true; }),
    expectedCode: assertionFailed,
    expectedLabel: "generated-parity-alignment:browser-fixture-mutating"
  },
  {
    name: "literal bearer in TypeScript source",
    id: "web.static.secret-mutation-guard",
    caseVariantRejectOverrides: () => new Map([["web/src/app.ts", `${readCanonical("web/src/app.ts")}\nbearer abcdefghijklmnopqrstuvwxyz`]]),
    overrides: () => new Map([["web/src/app.ts", `${readCanonical("web/src/app.ts")}\nBearer abcdefghijklmnopqrstuvwxyz`]]),
    expectedCode: assertionFailed,
    expectedLabel: "secret-mutation-guard:no-literal-bearer"
  },
  {
    name: "fabricated VM count in shell",
    id: "web.static.no-fabricated-values",
    caseVariantRejectOverrides: () => new Map([["web/index.html", `${readCanonical("web/index.html")}\nvm: 3/3`]]),
    overrides: () => new Map([["web/index.html", `${readCanonical("web/index.html")}\nVM: 3/3`]]),
    expectedCode: assertionFailed,
    expectedLabel: "no-fabricated-values:literal-VM: 3/3"
  },
  {
    name: "missing schema key",
    id: "web.static.feature-surface-ledger",
    overrides: () => removeOnce(ledgerPath, "\"$schema\""),
    expectedCode: assertionFailed,
    expectedLabel: "feature-surface-ledger:schema-key"
  },
  {
    name: "missing app root",
    id: "web.static.root-assets",
    overrides: () => removeOnce("web/index.html", "id=\"app-root\""),
    expectedCode: assertionFailed,
    expectedLabel: "root-assets:app-root"
  },
  {
    name: "missing inline favicon",
    id: "web.static.inline-favicon",
    overrides: () => removeOnce("web/index.html", "<link rel=\"icon\""),
    expectedCode: assertionFailed,
    expectedLabel: "inline-favicon:link"
  },
  {
    name: "single edge path escape",
    id: "web.static.single-edge-isolation",
    overrides: () => new Map([["web/app.js", `${readCanonical("web/app.js")}\n../../ui/`]]),
    expectedCode: assertionFailed,
    expectedLabel: "single-edge-isolation:web/app.js"
  },
  {
    name: "missing design title",
    id: "web.static.design-boundary",
    overrides: () => removeOnce("web/DESIGN.md", "PureCVisor Desktop Node Web DESIGN.md"),
    expectedCode: assertionFailed,
    expectedLabel: "design-boundary:title"
  },
  {
    name: "missing accent token",
    id: "web.static.supanova-tokens",
    overrides: () => removeOnce("web/styles.css", "--accent: #22d3ee"),
    expectedCode: assertionFailed,
    expectedLabel: "supanova-tokens:accent"
  },
  {
    name: "missing visual shell port",
    id: "web.static.visual-shell",
    overrides: () => removeOnce("web/index.html", "data-ui-port=\"single-edge-visual-shell\""),
    expectedCode: assertionFailed,
    expectedLabel: "visual-shell:port"
  },
  {
    name: "missing menu bar",
    id: "web.static.workbench-frame",
    overrides: () => removeOnce("web/index.html", "class=\"menu-bar\""),
    expectedCode: assertionFailed,
    expectedLabel: "workbench-frame:index-menu-bar"
  },
  {
    name: "missing Ops Cockpit navigation label",
    id: "web.static.frontend-mockups",
    overrides: () => removeOnce("web/mockups/frontend-completion-samples.html", "Ops Cockpit"),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-mockups:ops-cockpit-navigation"
  },
  {
    name: "incorrect batch count",
    id: "web.static.frontend-batches",
    overrides: () => replaceExactlyOnce(
      batchPlanPath,
      "\"batch_count\": 5",
      "\"batch_count\": 4"
    ),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-batches:batch-count"
  },
  {
    name: "missing Phase 2H host status endpoint",
    id: "web.static.phase2h-endpoints",
    overrides: () => removeOnce("web/app.js", "/api/v1/host/status"),
    expectedCode: assertionFailed,
    expectedLabel: "phase2h-endpoints:host-status"
  },
  {
    name: "missing Local API route coverage registry",
    id: "web.static.local-api-registry",
    overrides: () => removeOnce("web/src/served/routes.ts", "DESKTOP_NODE_ROUTE_COVERAGE"),
    expectedCode: assertionFailed,
    expectedLabel: "local-api-registry:route-coverage-source"
  },
  {
    name: "missing VM QoS guest readback renderer",
    id: "web.static.qos-guest-readback",
    overrides: () => removeOnce("web/app.js", "renderVmQosGuestReadback"),
    expectedCode: assertionFailed,
    expectedLabel: "qos-guest-readback:render"
  },
  {
    name: "missing VM QoS storage preview route",
    id: "web.static.qos-guest-control",
    overrides: () => removeOnce("web/app.js", "vmQosStoragePreview"),
    expectedCode: assertionFailed,
    expectedLabel: "qos-guest-control:storage-preview-route"
  },
  {
    name: "missing running guest execution cancel label",
    id: "web.static.guest-exec-cancel",
    overrides: () => removeOnce("web/app.js", "Cancel running guest exec"),
    expectedCode: assertionFailed,
    expectedLabel: "guest-exec-cancel:label"
  },
  {
    name: "missing command palette",
    id: "web.static.search-event-table",
    overrides: () => removeOnce("web/index.html", "id=\"command-palette\""),
    expectedCode: assertionFailed,
    expectedLabel: "search-event-table:command-palette"
  },
  {
    name: "missing staged errors source wiring",
    id: "web.static.served-source-parts",
    overrides: () => removeOnce("web/scripts/build-served-asset.mjs", "src/served/errors.ts"),
    expectedCode: assertionFailed,
    expectedLabel: "served-source-parts:part-errors.ts"
  },
  {
    name: "missing optional bearer authorization header",
    id: "web.static.optional-bearer",
    overrides: () => removeOnce("web/app.js", "headers.set('Authorization'"),
    expectedCode: assertionFailed,
    expectedLabel: "optional-bearer:authorization"
  },
  {
    name: "missing account login form",
    id: "web.static.account-rbac-console",
    overrides: () => removeOnce("web/index.html", "id=\"account-login-form\""),
    expectedCode: assertionFailed,
    expectedLabel: "account-rbac-console:login-form"
  },
  {
    name: "missing listener API base script",
    id: "web.static.listener-api-base",
    overrides: () => removeOnce("web/index.html", "/pcv-config.js"),
    expectedCode: assertionFailed,
    expectedLabel: "listener-api-base:config-script"
  },
  {
    name: "missing VM create ISO payload field",
    id: "web.static.vm-create-payload",
    overrides: () => removeOnce("web/app.js", "iso_path: String(data.get('iso_path') || '').trim()"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-create-payload:iso-path:generated"
  },
  {
    name: "missing VM saved-state resume route",
    id: "web.static.vm-lifecycle-routes",
    overrides: () => removeOnce("web/app.js", "route: '/api/v1/vms/{vm_id}/resume-saved'"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-routes:resume-saved:generated"
  },
  {
    name: "missing VM detail panel mount",
    id: "web.static.vm-detail-mount",
    overrides: () => removeOnce("web/index.html", "id=\"vm-detail-panel\""),
    expectedCode: assertionFailed,
    expectedLabel: "vm-detail-mount:panel"
  },
  {
    name: "missing running VM delete guard",
    id: "web.static.vm-lifecycle-actions",
    overrides: () => removeOnce("web/app.js", "PCV_VM_DELETE_RUNNING_BLOCKED"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-actions:running-delete-guard:generated"
  },
  {
    name: "missing checkpoint restore action",
    id: "web.static.checkpoint-actions",
    overrides: () => removeOnce("web/app.js", "data-action=\"checkpoint-restore\""),
    expectedCode: assertionFailed,
    expectedLabel: "checkpoint-actions:restore:generated"
  },
  {
    name: "missing tracked browser job history key",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/app.js", "pcvDesktopTrackedJobs.v1"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:storage-key:generated"
  },
  {
    name: "missing job polling delay state",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/app.js", "jobPollDelayMs: 2000"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:poll-delay:generated"
  },
  {
    name: "missing shell refresh command",
    id: "web.static.shell-controls",
    overrides: () => removeOnce("web/index.html", "data-menu-command=\"refresh\""),
    expectedCode: assertionFailed,
    expectedLabel: "shell-controls:refresh-command"
  },
  {
    name: "missing activity troubleshooting panel",
    id: "web.static.activity-troubleshooting",
    overrides: () => removeOnce("web/index.html", "id=\"activity-panel\""),
    expectedCode: assertionFailed,
    expectedLabel: "activity-troubleshooting:activity-panel"
  },
  {
    name: "missing ops summary panel",
    id: "web.static.ops-cockpit",
    overrides: () => removeOnce("web/index.html", "id=\"ops-summary-panel\""),
    expectedCode: assertionFailed,
    expectedLabel: "ops-cockpit:ops-summary-panel"
  },
  {
    name: "missing evidence panel",
    id: "web.static.evidence-dashboard",
    overrides: () => removeOnce("web/index.html", "id=\"evidence-panel\""),
    expectedCode: assertionFailed,
    expectedLabel: "evidence-dashboard:evidence-panel"
  },
  {
    name: "missing evidence issue collector",
    id: "web.static.evidence-degradation",
    overrides: () => removeAll("web/app.js", "collectEvidenceIssues"),
    expectedCode: assertionFailed,
    expectedLabel: "evidence-degradation:collect-evidence-issues:generated"
  },
  {
    name: "missing diagnostic bundle list renderer",
    id: "web.static.diagnostic-bundle",
    overrides: () => removeAll("web/app.js", "renderDiagnosticBundleList"),
    expectedCode: assertionFailed,
    expectedLabel: "diagnostic-bundle:render-list:generated"
  },
  {
    name: "missing internal distribution boundary term",
    id: "web.static.operator-terms",
    overrides: () => removeOnce("docs/OPERATOR_SURFACE_TERMS.md", "배포 경계: 내부 사설망 전용"),
    expectedCode: assertionFailed,
    expectedLabel: "operator-terms:distribution-boundary"
  },
  {
    name: "missing selected VM stale edge case",
    id: "web.static.frontend-edge-cases",
    overrides: () => removeAll("web/app.js", "PCV_SELECTED_VM_STALE"),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-edge-cases:selected-vm-stale:generated"
  },
  {
    name: "missing token rotation panel",
    id: "web.static.token-rotation",
    overrides: () => removeOnce("web/index.html", 'id="token-rotation-panel"'),
    expectedCode: assertionFailed,
    expectedLabel: "token-rotation:panel"
  },
  {
    name: "missing beta follow-up panel",
    id: "web.static.beta-followup",
    overrides: () => removeOnce("web/index.html", 'id="beta-followup-panel"'),
    expectedCode: assertionFailed,
    expectedLabel: "beta-followup:panel"
  },
  {
    name: "missing monitoring panel",
    id: "web.static.monitoring",
    overrides: () => removeOnce("web/index.html", 'id="monitoring-panel"'),
    expectedCode: assertionFailed,
    expectedLabel: "monitoring:panel"
  },
  {
    name: "missing network inventory panel",
    id: "web.static.network-inventory",
    overrides: () => removeOnce("web/index.html", 'id="network-inventory-panel"'),
    expectedCode: assertionFailed,
    expectedLabel: "network-inventory:panel"
  },
  {
    name: "missing VM filter",
    id: "web.static.workflow-polish",
    overrides: () => removeOnce("web/index.html", 'id="vm-filter"'),
    expectedCode: assertionFailed,
    expectedLabel: "workflow-polish:vm-filter"
  },
  {
    name: "missing job orchestration authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/state.ts", "pendingVmActions: {},"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:pending-vm-actions:source"
  },
  {
    name: "missing shell controls authoritative source owner",
    id: "web.static.shell-controls",
    overrides: () => removeOnce("web/src/served/render-shell.ts", "function renderVmAssetList()"),
    expectedCode: assertionFailed,
    expectedLabel: "shell-controls:render-vm-asset-list:source"
  },
  {
    name: "missing VM create payload authoritative source owner",
    id: "web.static.vm-create-payload",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "function readCreatePayload(form)"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-create-payload:payload-reader:source"
  },
  {
    name: "missing VM lifecycle route authoritative source owner",
    id: "web.static.vm-lifecycle-routes",
    overrides: () => removeOnce("web/src/served/routes.ts", "vmAction: (vmId: string, action: string)"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-routes:vm-action:source"
  },
  {
    name: "missing VM detail state authoritative source owner",
    id: "web.static.vm-detail-mount",
    overrides: () => removeOnce("web/src/served/state.ts", "vmStateFilter: 'all',"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-detail-mount:state-filter:source"
  },
  {
    name: "missing VM detail binding authoritative source owner",
    id: "web.static.vm-detail-mount",
    overrides: () => removeOnce("web/src/served-app.ts", "els.vmStateFilter?.addEventListener"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-detail-mount:state-filter-binding:source"
  },
  {
    name: "missing VM detail sort authoritative source owner",
    id: "web.static.vm-detail-mount",
    overrides: () => removeOnce("web/src/served/render-inventory.ts", "function compareVms(left, right)"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-detail-mount:compare:source"
  },
  {
    name: "missing VM lifecycle control authoritative source owner",
    id: "web.static.vm-lifecycle-actions",
    overrides: () => removeOnce("web/src/served/render-vm-detail.ts", "data-action=\"vm-resume-saved\""),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-actions:resume-saved:source"
  },
  {
    name: "missing VM lifecycle delete guard authoritative source owner",
    id: "web.static.vm-lifecycle-actions",
    overrides: () => removeOnce("web/src/served/mutate.ts", "code: 'PCV_VM_DELETE_RUNNING_BLOCKED'"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-actions:running-delete-guard:source"
  },
  {
    name: "missing VM lifecycle refusal authoritative source owner",
    id: "web.static.vm-lifecycle-actions",
    overrides: () => removeOnce("web/src/served/errors.ts", "Unmanaged delete refusal remains."),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-actions:unmanaged-delete-refusal:source"
  },
  {
    name: "missing VM lifecycle manage client authoritative source owner",
    id: "web.static.vm-lifecycle-actions",
    overrides: () => removeOnce("web/src/served/api-client.ts", "body: JSON.stringify({ confirm_name: confirmName })"),
    expectedCode: assertionFailed,
    expectedLabel: "vm-lifecycle-actions:confirm-name:source"
  },
  {
    name: "missing checkpoint controls authoritative source owner",
    id: "web.static.checkpoint-actions",
    overrides: () => removeOnce("web/src/served/render-qos.ts", "data-action=\"checkpoint-restore\""),
    expectedCode: assertionFailed,
    expectedLabel: "checkpoint-actions:restore:source"
  },
  {
    name: "missing checkpoint binding authoritative source owner",
    id: "web.static.checkpoint-actions",
    overrides: () => removeOnce("web/src/served-app.ts", "button.dataset.action === 'checkpoint-restore'"),
    expectedCode: assertionFailed,
    expectedLabel: "checkpoint-actions:restore-binding:source"
  },
  {
    name: "missing checkpoint loader authoritative source owner",
    id: "web.static.checkpoint-actions",
    overrides: () => removeOnce("web/src/served/load.ts", "async function loadCheckpoints(vmId, options = {})"),
    expectedCode: assertionFailed,
    expectedLabel: "checkpoint-actions:load:source"
  },
  {
    name: "missing browser job history retention authoritative source owner",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/src/served/state.ts", "const JOB_HISTORY_LIMIT = 50;"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:limit:source"
  },
  {
    name: "missing browser job history load retention authoritative source owner",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/src/served/state.ts", ".slice(0, JOB_HISTORY_LIMIT);"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:load-retention:source"
  },
  {
    name: "missing browser job history save retention authoritative source owner",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/src/served/state.ts", "const jobs = state.trackedJobs.slice(0, JOB_HISTORY_LIMIT);"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:save-retention:source"
  },
  {
    name: "missing browser job tracking retention authoritative source owner",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "state.trackedJobs = state.trackedJobs.slice(0, JOB_HISTORY_LIMIT);"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:track-retention:source"
  },
  {
    name: "missing browser job history binding authoritative source owner",
    id: "web.static.browser-job-history",
    overrides: () => removeOnce("web/src/served-app.ts", "els.clearJobHistory.addEventListener('click', clearTrackedJobHistory);"),
    expectedCode: assertionFailed,
    expectedLabel: "browser-job-history:clear-binding:source"
  },
  {
    name: "missing job orchestration pending table authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/table.ts", "function isVmActionPending(vmId, action = '')"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:vm-action-pending:source"
  },
  {
    name: "missing job orchestration backoff authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "state.jobPollDelayMs = Math.min(Math.round(state.jobPollDelayMs * 1.5), 15000);"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:poll-backoff:source"
  },
  {
    name: "missing job orchestration cancel implementation authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "const job = await desktopApi.cancelJob(jobId);"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:cancel-call:source"
  },
  {
    name: "missing job orchestration retry implementation authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "const job = await desktopApi.retryJob(jobId);"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:retry-call:source"
  },
  {
    name: "missing job orchestration reconcile implementation authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/job-polling.ts", "const job = await desktopApi.reconcileJob(jobId);"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:reconcile-call:source"
  },
  {
    name: "missing job orchestration client authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/api-client.ts", "cancelJob: (jobId: string) =>"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:cancel-client:source"
  },
  {
    name: "missing job orchestration rendered controls authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/render-jobs.ts", "data-action=\"cancel-job\""),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:cancel-control:source"
  },
  {
    name: "missing job orchestration binding authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served-app.ts", "if (button.dataset.action === 'cancel-job') await cancelJob(button.dataset.jobId, button.dataset.jobCancelScope);"),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:cancel-binding:source"
  },
  {
    name: "missing job orchestration pagination authoritative source owner",
    id: "web.static.job-orchestration",
    overrides: () => removeOnce("web/src/served/render-activity.ts", "data-action=\"load-next-jobs\""),
    expectedCode: assertionFailed,
    expectedLabel: "job-orchestration:load-next-jobs:source"
  },
  {
    name: "missing Local API authoritative source owner",
    id: "web.static.local-api-registry",
    overrides: () => removeOnce("web/src/served/routes.ts", "DESKTOP_NODE_API_ROUTES"),
    expectedCode: assertionFailed,
    expectedLabel: "local-api-registry:api-routes:source"
  },
  {
    name: "missing QoS readback authoritative source owner",
    id: "web.static.qos-guest-readback",
    overrides: () => removeOnce("web/src/served/routes.ts", "vmBlkio"),
    expectedCode: assertionFailed,
    expectedLabel: "qos-guest-readback:vm-blkio:source"
  },
  {
    name: "missing QoS control authoritative source owner",
    id: "web.static.qos-guest-control",
    overrides: () => removeOnce("web/src/served/routes.ts", "vmQosStoragePreview"),
    expectedCode: assertionFailed,
    expectedLabel: "qos-guest-control:storage-preview-route-source"
  },
  {
    name: "missing guest cancel authoritative source owner",
    id: "web.static.guest-exec-cancel",
    overrides: () => removeOnce("web/src/served/render-jobs.ts", "Cancel running guest exec"),
    expectedCode: assertionFailed,
    expectedLabel: "guest-exec-cancel:label-source"
  },
  {
    name: "missing search authoritative source owner",
    id: "web.static.search-event-table",
    overrides: () => removeOnce("web/src/served/render-shell.ts", "buildCommandPaletteItems"),
    expectedCode: assertionFailed,
    expectedLabel: "search-event-table:build-command-palette:source"
  },
  {
    name: "missing account authoritative source owner",
    id: "web.static.account-rbac-console",
    overrides: () => removeOnce("web/src/served/routes.ts", "/api/v1/auth/login"),
    expectedCode: assertionFailed,
    expectedLabel: "account-rbac-console:login-route:source"
  },
  {
    name: "missing listener authoritative source owner",
    id: "web.static.listener-api-base",
    overrides: () => removeOnce("web/src/served/state.ts", "PCV_DESKTOP_NODE_CONFIG"),
    expectedCode: assertionFailed,
    expectedLabel: "listener-api-base:config-object:source"
  },
  {
    name: "null schema",
    id: "web.static.feature-surface-ledger",
    overrides: () => transformJson(schemaPath, () => null),
    expectedCode: assertionFailed,
    expectedLabel: "feature-surface-ledger:schema-object"
  },
  {
    name: "null plan",
    id: "web.static.frontend-batches",
    overrides: () => transformJson(batchPlanPath, () => null),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-batches:plan-object"
  },
  {
    name: "null batch",
    id: "web.static.frontend-batches",
    overrides: () => transformJson(batchPlanPath, (plan) => {
      plan.batches[0] = null;
    }),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-batches:batch-0:object"
  },
  {
    name: "null work item",
    id: "web.static.frontend-batches",
    overrides: () => transformJson(batchPlanPath, (plan) => {
      plan.batches[0].work_items[0] = null;
    }),
    expectedCode: assertionFailed,
    expectedLabel: "frontend-batches:batch-0:item-0:object"
  },
  {
    name: "overlapping present and excluded surfaces",
    id: "web.static.feature-surface-ledger",
    overrides: () => transformJson(ledgerPath, (ledger) => {
      ledger.features[0].routes[0].excluded_surfaces.push({
        surface: "web",
        reason: "controlled overlap defect"
      });
    }),
    expectedCode: assertionFailed,
    expectedLabel: "feature-surface-ledger:feature-0:route-0:surface-partition"
  },
  {
    name: "omitted target surface",
    id: "web.static.feature-surface-ledger",
    overrides: () => transformJson(ledgerPath, (ledger) => {
      ledger.features[0].routes[0].present_surfaces = ["api", "cli"];
    }),
    expectedCode: assertionFailed,
    expectedLabel: "feature-surface-ledger:feature-0:route-0:surface-partition"
  },
  {
    name: "missing present Web binding",
    id: "web.static.feature-surface-ledger",
    overrides: () => transformJson(ledgerPath, (ledger) => {
      delete ledger.features[0].routes[0].surface_bindings.web;
    }),
    expectedCode: assertionFailed,
    expectedLabel: "feature-surface-ledger:feature-0:route-0:surface-binding-membership"
  }
];

for (const defect of defectCases) {
  test(`controlled shell asset defect rejects: ${defect.id} (${defect.name})`, async () => {
    const verifier = staticContracts.WEB_STATIC_VERIFIERS?.[defect.id];
    assert.equal(typeof verifier, "function", `verifier missing: ${defect.id}`);

    await verifier(createWebContractContext({ repoRoot: repositoryRoot }).forContract(defect.id));

    if (defect.caseVariantOverrides) {
      await verifier(createWebContractContext({
        repoRoot: repositoryRoot,
        textOverrides: defect.caseVariantOverrides()
      }).forContract(defect.id));
    }
    if (defect.caseVariantRejectOverrides) {
      await assert.rejects(
        () => verifier(createWebContractContext({
          repoRoot: repositoryRoot,
          textOverrides: defect.caseVariantRejectOverrides()
        }).forContract(defect.id)),
        (error) => error instanceof WebContractError
          && error.contract_id === defect.id
          && error.code === assertionFailed
      );
    }
    if (defect.collectionVariantOverrides) {
      await assert.rejects(
        () => verifier(createWebContractContext({
          repoRoot: repositoryRoot,
          textOverrides: defect.collectionVariantOverrides()
        }).forContract(defect.id)),
        (error) => error instanceof WebContractError
          && error.contract_id === defect.id
          && error.code === assertionFailed
          && error.message === `${assertionFailed}|assertion=parity-manifest:fixture-names-array`
      );
    }

    const context = createWebContractContext({
      repoRoot: repositoryRoot,
      textOverrides: defect.overrides?.() ?? new Map(),
      processRunner: defect.processRunner
    }).forContract(defect.id);
    await assert.rejects(
      async () => {
        if (defect.ownerIds) {
          await context.runOwners(defect.ownerIds);
        }
        await verifier(context);
      },
      (error) => {
        assert.ok(error instanceof WebContractError);
        assert.equal(error.contract_id, defect.id);
        assert.equal(error.code, defect.expectedCode);
        if (defect.expectedLabel !== undefined) {
          assert.equal(error.message, `${defect.expectedCode}|assertion=${defect.expectedLabel}`);
        }
        return true;
      }
    );
  });
}

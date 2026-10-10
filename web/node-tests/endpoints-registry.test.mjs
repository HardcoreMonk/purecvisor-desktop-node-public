import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018: web/src/modules/endpoints.ts is the Single Edge EP surface shape carrying the Desktop Node Local API routes.
// Until Task 16 removes the legacy registry, every route fragment it builds must exist in web/src/served/routes.ts, and
// no Linux Single Edge route may appear.

const endpoints = fs.readFileSync(fileURLToPath(new URL("../src/modules/endpoints.ts", import.meta.url)), "utf8");
const legacyRoutes = fs.readFileSync(fileURLToPath(new URL("../src/served/routes.ts", import.meta.url)), "utf8");
const api = fs.readFileSync(fileURLToPath(new URL("../src/modules/api.ts", import.meta.url)), "utf8");
const events = fs.readFileSync(fileURLToPath(new URL("../src/modules/events.ts", import.meta.url)), "utf8");

// Comment lines describe what was removed; only code lines are checked for forbidden surfaces.
function codeOnly(text) {
  return text.split("\n").filter((line) => !line.trim().startsWith("//")).join("\n");
}

const keys = [...endpoints.matchAll(/^\s+([A-Z][A-Z0-9_]+):\s*function/gm)].map((match) => match[1]);

test("the Desktop Node endpoint surface lists the routes the console uses and nothing from the Linux Single Edge", () => {
  for (const key of ["OPS_SUMMARY", "RUNTIME_POLICY", "HOST_STATUS", "NETWORK_INVENTORY", "CONSOLE_CAPABILITIES", "VM_LIST", "VM_CREATE", "VM_DETAIL", "VM_ACTION", "VM_CHECKPOINTS", "CHECKPOINT", "CHECKPOINT_ACTION", "CHECKPOINT_SCHEDULE_PREVIEW", "CHECKPOINT_SCHEDULE", "CHECKPOINT_SCHEDULE_CLEAR", "VM_CONSOLE", "VM_CONSOLE_FRAME", "VM_CONSOLE_INPUT", "VM_GUEST_EXEC", "VM_EXPORT", "VM_IMPORT", "JOB_LIST", "JOBS_PAGE", "JOB", "JOB_ACTION", "DIAGNOSTIC_BUNDLES", "AUTH_LOGIN", "AUTH_LOOPBACK_SESSION", "AUTH_REFRESH", "AUTH_LOGOUT", "AUTH_SESSION", "AUTH_RBAC", "ACCOUNTS", "ACCOUNT_DISABLE", "AUTH_REGISTER", "AUTH_PASSWORD"]) {
    assert.ok(keys.includes(key), `missing endpoint ${key}`);
  }
  const code = codeOnly(endpoints);
  for (const forbidden of ["CTR_", "STORAGE_", "OVN_", "VPC_", "SURICATA_", "DPDK_", "SRIOV_", "PUSH_", "AUTH_TOTP", "RPC:", "ISCSI", "OVERLAY", "UPDATE_CHECK", "AUTH_TOKEN:"]) {
    assert.doesNotMatch(code, new RegExp("^\s+" + forbidden, "m"), `Linux Single Edge endpoint ${forbidden}`);
  }
  assert.match(code, /window\.PCV_DESKTOP_NODE_CONFIG/, "API_BASE comes from pcv-config.js");
});

test("every route fragment of the endpoint surface exists in the legacy route registry", () => {
  const fragments = [...codeOnly(endpoints).matchAll(/'(\/[a-z0-9\-/?=&]+)'/g)].map((match) => match[1]).filter((fragment) => fragment !== "/" && fragment !== "/api/v1");
  assert.ok(fragments.length > 40, `fragments ${fragments.length}`);
  const missing = fragments.filter((fragment) => !legacyRoutes.includes(fragment));
  assert.deepEqual(missing, [], "fragments absent from web/src/served/routes.ts");
  for (const action of ["'start', 'shutdown', 'poweroff', 'restart', 'save', 'resume-saved', 'pause', 'resume', 'rename', 'eject'", "'cancel', 'retry', 'reconcile'"]) {
    assert.ok(endpoints.includes(action) && legacyRoutes.includes(action), `action list ${action}`);
  }
});

test("api recovers a rejected bearer token once and the events module polls instead of a WebSocket", () => {
  const apiCode = codeOnly(api);
  assert.match(apiCode, /async function _recoverSession\(\)/);
  assert.match(apiCode, /_createLoopbackSession/);
  assert.match(apiCode, /\/\^PCV_AUTH_\//, "403 with a PCV_AUTH_* code counts as a rejected token");
  assert.match(apiCode, /opts\._pcvRetried = true;/, "retry happens once");
  assert.doesNotMatch(apiCode, /new WebSocket\(|function connectWS|_totp[A-Z]/, "no WebSocket or TOTP flow");
  assert.match(apiCode, /EP\.AUTH_LOGIN\(\)/);
  assert.match(apiCode, /EP\.AUTH_LOOPBACK_SESSION\(\)/);
  const eventsCode = codeOnly(events);
  assert.match(eventsCode, /PCV\.events = \{/);
  assert.match(eventsCode, /pcv_tracked_jobs/);
  assert.match(eventsCode, /isHealthy: isHealthy/);
  assert.doesNotMatch(eventsCode, /new WebSocket\(/);
});

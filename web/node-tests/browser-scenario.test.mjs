// Task 4a: the browser scenario runner plans S1-S3 without a browser, rejects bad arguments,
// refuses VM names outside the pcv-it- prefix, and keeps secrets out of the summary.
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { dirname, resolve } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import {
  PLAN_STEPS,
  assertScenarioProfile,
  assertScenarioVmName,
  classifyJobStatus,
  judgeShellReady,
  serializeScenarioSummary
} from "../scripts/run-browser-scenario.mjs";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const script = resolve(repoRoot, "web", "scripts", "run-browser-scenario.mjs");
const probeSecret = "probe-secret";
const probeToken = "probe-token";

function runScenario(args, env = {}) {
  return spawnSync(process.execPath, [script, ...args], {
    cwd: repoRoot,
    encoding: "utf8",
    windowsHide: true,
    env: { ...process.env, PCV_SCENARIO_PASSWORD: probeSecret, ...env }
  });
}

function stepNames(stdout) {
  return stdout.split(/\r?\n/).filter((line) => /^\d+\. /.test(line)).map((line) => line.replace(/^\d+\. /, ""));
}

test("rejects an unknown argument with a usage error", () => {
  const result = runScenario(["--scenario", "s1", "--bogus", "1"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /argument error: unknown --bogus/);
  assert.match(result.stderr, /usage: node scripts\/run-browser-scenario\.mjs /);
  assert.equal(result.stdout, "");
  assert.equal(result.stderr.includes(probeSecret), false);
});

test("rejects a bad scenario with a usage error and does not echo the value", () => {
  const result = runScenario(["--scenario", "nope"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /argument error: invalid --scenario/);
  assert.match(result.stderr, /usage: node scripts\/run-browser-scenario\.mjs /);
  assert.equal(result.stdout, "");
  assert.equal(result.stderr.includes("nope"), false);
  assert.equal(result.stderr.includes(probeSecret), false);
});

test("plan-only lists s1 steps and sends no request", () => {
  const result = runScenario(["--scenario", "s1", "--iso", "X"]);
  assert.equal(result.status, 0, result.stderr);
  assert.match(result.stdout, /browser scenario plan-only \(no request sent\)/);
  assert.match(result.stdout, /scenario=s1/);
  assert.match(result.stdout, /mode=plan-only/);
  assert.match(result.stdout, /out=artifacts\/browser-scenario-\d{8}\/s1-\d{6}/);
  assert.deepEqual(stepNames(result.stdout), PLAN_STEPS.s1);
  assert.equal(result.stdout.includes(probeSecret), false);
  assert.equal(result.stderr.includes(probeSecret), false);
});

test("plan-only lists s2 and s3 steps", () => {
  const s2 = runScenario(["--scenario", "s2", "--source", "pcv-it-source"]);
  const s3 = runScenario(["--scenario", "s3", "--vm", "pcv-it-x"]);
  assert.equal(s2.status, 0, s2.stderr);
  assert.equal(s3.status, 0, s3.stderr);
  assert.deepEqual(stepNames(s2.stdout), PLAN_STEPS.s2);
  assert.deepEqual(stepNames(s3.stdout), PLAN_STEPS.s3);
  assert.match(s2.stdout, /out=artifacts\/browser-scenario-\d{8}\/s2-\d{6}/);
  assert.match(s3.stdout, /out=artifacts\/browser-scenario-\d{8}\/s3-\d{6}/);
});

test("check plan names stay in the exported list and this test does not launch a browser", () => {
  assert.deepEqual(PLAN_STEPS.check, [
    "login",
    "version-badge",
    "vm-list",
    "create-wizard-fields",
    "vm-detail-power",
    "console-tab",
    "checkpoint-tab",
    "template-lock-clone"
  ]);
});

test("assertScenarioVmName accepts only the pcv-it- prefix", () => {
  assert.equal(assertScenarioVmName("pcv-it-x"), "pcv-it-x");
  assert.throws(() => assertScenarioVmName("pcv-guest-installed-04253-r1"), /pcv-it-/);
  assert.throws(() => assertScenarioVmName(""), /pcv-it-/);
});

test("summary serialization drops a fake password and token", () => {
  const body = serializeScenarioSummary({
    contract: "pcv-browser-scenario-v1",
    password: probeSecret,
    token: probeToken,
    access_token: probeToken,
    detail: `saw ${probeSecret} then ${probeToken}`,
    steps: [{ name: "login", detail: probeSecret, screenshot: "" }]
  }, [probeSecret, probeToken]);
  assert.equal(body.includes(probeSecret), false);
  assert.equal(body.includes(probeToken), false);
  assert.match(body, /\[redacted\]/);
  assert.match(body, /"contract":"pcv-browser-scenario-v1"/);
});

test("plan-only lists connect-switch, clone_seconds, and cleanup without a browser", () => {
  assert.ok(PLAN_STEPS.s1.includes("connect-switch"));
  assert.equal(PLAN_STEPS.s1.indexOf("connect-switch"), PLAN_STEPS.s1.indexOf("wait-create-job") + 1);
  assert.ok(PLAN_STEPS.s2.includes("record-clone-seconds"));
  const cleanup = runScenario(["--scenario", "s1", "--cleanup"]);
  assert.equal(cleanup.status, 0, cleanup.stderr);
  assert.match(cleanup.stdout, /browser scenario plan-only \(no request sent\)/);
  assert.deepEqual(stepNames(cleanup.stdout), PLAN_STEPS.cleanup);
  assert.equal(cleanup.stdout.includes(probeSecret), false);
});

test("shell ready treats degraded as ready and auth as a failure", () => {
  const ready = {
    loading: false,
    lastRefreshedAt: "2026-11-11T00:00:00Z",
    loginHidden: true,
    appVisible: true
  };
  const degraded = judgeShellReady({ ...ready, connectionState: "degraded" });
  assert.equal(degraded.ok, true);
  assert.equal(degraded.failed, false);
  assert.equal(degraded.connection, "degraded");
  const connected = judgeShellReady({ ...ready, connectionState: "connected" });
  assert.equal(connected.ok, true);
  const auth = judgeShellReady({ ...ready, connectionState: "auth" });
  assert.equal(auth.ok, false);
  assert.equal(auth.failed, true);
  assert.equal(auth.connection, "auth");
  const broken = judgeShellReady({ ...ready, connectionState: "error" });
  assert.equal(broken.failed, true);
  assert.equal(broken.ok, false);
  const loading = judgeShellReady({ ...ready, connectionState: "connected", loading: true });
  assert.equal(loading.ok, false);
  assert.equal(loading.failed, false);
  const waitingForLogin = judgeShellReady({ connectionState: "auth", loginHidden: false, appVisible: false, lastRefreshedAt: 1, loading: false });
  assert.equal(waitingForLogin.ok, false);
  assert.equal(waitingForLogin.failed, false);
});

test("job status splits in-flight, success, and failure", () => {
  for (const status of ["queued", "running", "pending", "accepted"]) {
    assert.equal(classifyJobStatus(status), "in-flight");
  }
  assert.equal(classifyJobStatus("succeeded"), "success");
  assert.equal(classifyJobStatus("failed"), "failed");
  assert.equal(classifyJobStatus("canceled"), "failed");
  assert.equal(classifyJobStatus(""), "failed");
});

test("profile paths must stay under artifacts", () => {
  const allowed = assertScenarioProfile("artifacts/browser-scenario-profile", repoRoot);
  assert.equal(allowed.endsWith("browser-scenario-profile"), true);
  assert.throws(() => assertScenarioProfile("../outside", repoRoot), /artifacts/);
  assert.throws(() => assertScenarioProfile("C:\\outside", repoRoot), /artifacts/);
});

test("execute rejects missing s1 arguments before a browser", () => {
  const missingIso = runScenario(["--execute", "--scenario", "s1", "--vm", "pcv-it-x"]);
  const missingVm = runScenario(["--execute", "--scenario", "s1", "--iso", "X"]);
  assert.equal(missingIso.status, 2);
  assert.equal(missingVm.status, 2);
  assert.match(missingIso.stderr, /argument error: missing value --iso/);
  assert.match(missingVm.stderr, /argument error: missing value --vm/);
  assert.equal(missingIso.stdout, "");
  assert.equal(missingVm.stdout, "");
  assert.equal(missingIso.stderr.includes(probeSecret), false);
  assert.equal(missingVm.stderr.includes("not implemented"), false);
});

test("execute rejects vm names outside the pcv-it- prefix", () => {
  const badVm = "pcv-guest-installed-04253-r1";
  const s1 = runScenario(["--execute", "--scenario", "s1", "--iso", "X", "--vm", badVm]);
  const s2 = runScenario(["--execute", "--scenario", "s2", "--source", "guest", "--clone", "pcv-it-clone"]);
  const s2Clone = runScenario(["--execute", "--scenario", "s2", "--source", "pcv-it-source", "--clone", "guest"]);
  const s3 = runScenario(["--execute", "--scenario", "s3", "--vm", "not-it"]);
  const cleanup = runScenario(["--execute", "--cleanup", "--scenario", "s1", "--vm", "bad,pcv-it-x"]);
  for (const result of [s1, s2, s2Clone, s3, cleanup]) {
    assert.equal(result.status, 2);
    assert.equal(result.stdout, "");
    assert.equal(result.stderr.includes(probeSecret), false);
    assert.equal(result.stderr.includes("not implemented"), false);
  }
  assert.match(s1.stderr, /argument error: invalid --vm/);
  assert.equal(s1.stderr.includes(badVm), false);
  assert.match(s2.stderr, /argument error: invalid --source/);
  assert.match(s2Clone.stderr, /argument error: invalid --clone/);
  assert.match(s3.stderr, /argument error: invalid --vm/);
  assert.match(cleanup.stderr, /argument error: invalid --vm/);
  assert.equal(cleanup.stderr.includes("bad,pcv-it-x"), false);
});

test("execute rejects a non-numeric argument and a profile outside artifacts", () => {
  const cpu = runScenario(["--execute", "--scenario", "s1", "--iso", "X", "--vm", "pcv-it-x", "--cpu", "nope"]);
  const interval = runScenario(["--execute", "--scenario", "s3", "--vm", "pcv-it-x", "--interval", "60m"]);
  const profile = runScenario(["--execute", "--scenario", "s1", "--iso", "X", "--vm", "pcv-it-x", "--profile", "../outside"]);
  const absolute = runScenario(["--execute", "--scenario", "s1", "--iso", "X", "--vm", "pcv-it-x", "--profile", "C:\\outside"]);
  for (const result of [cpu, interval, profile, absolute]) {
    assert.equal(result.status, 2);
    assert.equal(result.stdout, "");
    assert.equal(result.stderr.includes(probeSecret), false);
  }
  assert.match(cpu.stderr, /argument error: invalid --cpu/);
  assert.match(interval.stderr, /argument error: invalid --interval/);
  assert.match(profile.stderr, /argument error: invalid --profile/);
  assert.match(absolute.stderr, /argument error: invalid --profile/);
  assert.equal(profile.stderr.includes("../outside"), false);
  assert.equal(absolute.stderr.includes("C:\\outside"), false);
});

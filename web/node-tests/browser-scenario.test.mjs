// Task 4a: the browser scenario runner plans S1-S3 without a browser, rejects bad arguments,
// refuses VM names outside the pcv-it- prefix, and keeps secrets out of the summary.
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { dirname, resolve } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { PLAN_STEPS, assertScenarioVmName, serializeScenarioSummary } from "../scripts/run-browser-scenario.mjs";

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

test("execute is not implemented and exits 2 before a browser", () => {
  const result = runScenario(["--scenario", "s1", "--execute", "--iso", "X"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /browser scenario execute is not implemented/);
  assert.equal(result.stdout, "");
  assert.equal(result.stderr.includes(probeSecret), false);
});

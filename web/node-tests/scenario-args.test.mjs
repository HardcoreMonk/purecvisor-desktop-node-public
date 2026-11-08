// BL-0022: the S1, S2, and S3 scenario scripts read --name value and --name=value the same way, reject unknown
// arguments with a usage error, and default the out folder to the run date.
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { dirname, resolve } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { ScenarioArgumentError, formatRunDay, parseScenarioArgs } from "../scripts/scenario-args.mjs";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const spec = { values: ["api", "out"], booleans: ["execute"] };
const scenarios = [
  // A fixed --vm keeps the S1 plan stable; its default name carries the current second.
  {
    script: "run-s1-console-scenario.mjs",
    extra: ["--iso", "artifacts/scenario-args-probe.iso", "--vm", "pcv-it-s1-args-probe"],
    defaultOut: (day) => `artifacts/s1-console-scenario-${day}/pcv-it-s1-args-probe`
  },
  { script: "run-s2-clone-scenario.mjs", extra: [], defaultOut: (day) => `artifacts/s2-template-clone-${day}` },
  { script: "run-s3-checkpoint-scenario.mjs", extra: [], defaultOut: (day) => `artifacts/s3-checkpoint-${day}` }
];

function runScenario(script, args) {
  return spawnSync(process.execPath, [resolve(repoRoot, "web", "scripts", script), ...args], {
    cwd: repoRoot,
    encoding: "utf8",
    windowsHide: true
  });
}

function outLine(stdout) {
  const line = stdout.split(/\r?\n/).find((value) => value.startsWith("out="));
  assert.ok(line, "plan prints out=");
  return line.slice("out=".length);
}

function rejection(argv) {
  try {
    parseScenarioArgs(argv, spec);
  } catch (error) {
    assert.ok(error instanceof ScenarioArgumentError);
    return `${error.kind} ${error.argument}`;
  }
  return "accepted";
}

test("parses --name value and --name=value the same way", () => {
  const spaced = parseScenarioArgs(["--out", "artifacts/x", "--execute", "--api", "http://127.0.0.1:7777"], spec);
  const inline = parseScenarioArgs(["--out=artifacts/x", "--execute", "--api=http://127.0.0.1:7777"], spec);
  assert.deepEqual(spaced.values, { out: "artifacts/x", api: "http://127.0.0.1:7777" });
  assert.deepEqual(inline.values, spaced.values);
  assert.deepEqual([...spaced.flags], ["execute"]);
  assert.deepEqual([...inline.flags], ["execute"]);
});

test("rejects unknown, duplicate, positional, valueless, and password arguments", () => {
  assert.equal(rejection(["--bogus=1"]), "unknown bogus");
  assert.equal(rejection(["stray"]), "unknown stray");
  assert.equal(rejection(["--out", "a", "--out=b"]), "duplicate out");
  assert.equal(rejection(["--out"]), "missing value out");
  assert.equal(rejection(["--out", "--execute"]), "missing value out");
  assert.equal(rejection(["--out="]), "missing value out");
  assert.equal(rejection(["--execute=yes"]), "unexpected value execute");
  assert.equal(rejection(["--password=probe"]), "rejected password");
});

test("formats the local run date as yyyymmdd", () => {
  assert.equal(formatRunDay(new Date(2026, 10, 9, 23, 59)), "20261109");
  assert.equal(formatRunDay(new Date(2026, 0, 2)), "20260102");
});

for (const scenario of scenarios) {
  test(`${scenario.script} honours a spaced --out and defaults out to the run date`, () => {
    const spaced = runScenario(scenario.script, [...scenario.extra, "--out", "artifacts/scenario-args-probe"]);
    const inline = runScenario(scenario.script, [...scenario.extra, "--out=artifacts/scenario-args-probe"]);
    const defaulted = runScenario(scenario.script, scenario.extra);
    assert.equal(spaced.status, 0, spaced.stderr);
    assert.equal(inline.status, 0, inline.stderr);
    assert.equal(defaulted.status, 0, defaulted.stderr);
    assert.equal(outLine(spaced.stdout), "artifacts/scenario-args-probe");
    assert.equal(spaced.stdout, inline.stdout);
    assert.equal(outLine(defaulted.stdout), scenario.defaultOut(formatRunDay()));
  });

  test(`${scenario.script} rejects an unknown argument with a usage error before any plan`, () => {
    const result = runScenario(scenario.script, [...scenario.extra, "--bogus", "1"]);
    assert.equal(result.status, 2);
    assert.match(result.stderr, /argument error: unknown --bogus/);
    assert.match(result.stderr, new RegExp(`usage: node scripts/${scenario.script.replaceAll(".", "\\.")} `));
    assert.equal(result.stdout, "");
  });
}

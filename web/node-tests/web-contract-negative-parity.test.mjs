import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { EventEmitter } from "node:events";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import {
  authorizeNegativeParityFixtureCandidate,
  parseNodeTap,
  parsePesterSummary,
  runNegativeParity,
  runNegativeParityProcess
} from "../scripts/verify-web-contract-negative-parity.mjs";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const pesterSummary = JSON.stringify({
  total: 50,
  passed: 0,
  failed: 1,
  skipped: 0,
  not_run: 49,
  failure: "Expected app-root"
});
const nodeTap = [
  "TAP version 13",
  "# Subtest: web.static.root-assets",
  "not ok 1 - web.static.root-assets",
  "  ---",
  "  error: app-root missing",
  "  ...",
  "1..50",
  "# tests 50",
  "# pass 0",
  "# fail 1",
  "# skipped 49"
].join("\n");

function unsafe(detail) {
  assert.throws(
    () => parsePesterSummary(detail),
    /PCV_WEB_CONTRACT_FIXTURE_UNSAFE/
  );
}

function makeRunner({ pester = {}, node = {}, calls = [] } = {}) {
  return async (request) => {
    calls.push(request);
    if (request.fileName === process.execPath) {
      return {
        exitCode: 1,
        signal: null,
        timedOut: false,
        stdout: node.stdout ?? nodeTap,
        stderr: node.stderr ?? ""
      };
    }
    return {
      exitCode: 1,
      signal: null,
      timedOut: false,
      stdout: pester.stdout ?? pesterSummary,
      stderr: pester.stderr ?? ""
    };
  };
}

function generatedFixtureSnapshot() {
  return fs.readdirSync(os.tmpdir())
    .filter((name) => name.startsWith("pcv-web-contract-negative-"))
    .sort();
}

function alternateCaseForwardSlashPath(value) {
  const normalized = value.replaceAll("\\", "/");
  const drive = /^([A-Za-z]):/.exec(normalized);
  if (!drive) return normalized;
  const alternateDrive = drive[1] === drive[1].toLowerCase()
    ? drive[1].toUpperCase()
    : drive[1].toLowerCase();
  return `${alternateDrive}${normalized.slice(1)}`;
}

function createFakeProcessRuntime() {
  const child = new EventEmitter();
  child.stdout = new EventEmitter();
  child.stderr = new EventEmitter();
  child.kills = [];
  child.kill = (signal = undefined) => {
    child.kills.push(signal);
    return true;
  };
  const timers = [];
  return {
    child,
    timers,
    runtime: {
      spawnProcess: () => child,
      setTimer: (callback, delay) => {
        const timer = { callback, delay, cleared: false };
        timers.push(timer);
        return timer;
      },
      clearTimer: (timer) => { timer.cleared = true; }
    }
  };
}

test("parsePesterSummary normalizes the exact focused failure summary", () => {
  assert.deepEqual(parsePesterSummary(
    '{"total":50,"passed":0,"failed":1,"skipped":0,"not_run":49}'
  ), { total: 50, passed: 0, failed: 1, skipped: 0, notRun: 49 });
});

test("parseNodeTap normalizes the exact focused failure TAP summary", () => {
  assert.deepEqual(parseNodeTap(`
1..50
# tests 50
# pass 0
# fail 1
# skipped 49
`), { tests: 50, passed: 0, failed: 1, skipped: 49 });
});

test("parsePesterSummary fails closed for zero executed, malformed, and oversized output", () => {
  unsafe('{"total":50,"passed":0,"failed":0,"skipped":0,"not_run":50}');
  unsafe("not-json");
  unsafe("x".repeat(8193));
});

test("parseNodeTap fails closed for pass, two failures, malformed, and oversized output", () => {
  for (const output of [
    "1..50\n# tests 50\n# pass 1\n# fail 0\n# skipped 49",
    "1..50\n# tests 50\n# pass 0\n# fail 2\n# skipped 48",
    "not-tap",
    "x".repeat(8193)
  ]) {
    assert.throws(() => parseNodeTap(output), /PCV_WEB_CONTRACT_FIXTURE_UNSAFE/);
  }
});

test("runNegativeParityProcess preserves close during timeout grace and cleans listeners", async () => {
  const { child, timers, runtime } = createFakeProcessRuntime();
  const pending = runNegativeParityProcess({ fileName: "node", arguments: [], cwd: repositoryRoot, timeoutMs: 120000 }, runtime);
  timers[0].callback();
  assert.deepEqual(child.kills, [undefined]);
  assert.equal(timers.length, 2);
  child.emit("close", 1, null);
  const result = await pending;
  assert.equal(result.timedOut, true);
  assert.equal(result.exitCode, 1);
  assert.equal(timers.every((timer) => timer.cleared), true);
  assert.equal(child.listenerCount("close"), 0);
  assert.equal(child.listenerCount("error"), 0);
  assert.equal(child.stdout.listenerCount("data"), 0);
  assert.equal(child.stderr.listenerCount("data"), 0);
});

test("runNegativeParityProcess force-settles a nonclosing timed out child with bounded shared capture", async () => {
  const { child, timers, runtime } = createFakeProcessRuntime();
  const pending = runNegativeParityProcess({ fileName: "node", arguments: [], cwd: repositoryRoot, timeoutMs: 120000 }, runtime);
  child.stdout.emit("data", "x".repeat(8192));
  child.stderr.emit("data", "overflow");
  timers[0].callback();
  timers[1].callback();
  const result = await pending;
  assert.equal(result.timedOut, true);
  assert.equal(result.signal, "SIGKILL");
  assert.equal(result.overflow, true);
  assert.equal(result.stdout.length + result.stderr.length, 8192);
  assert.deepEqual(child.kills, [undefined, "SIGKILL"]);
  assert.equal(timers.every((timer) => timer.cleared), true);
  assert.equal(child.listenerCount("close"), 0);
  assert.equal(child.stdout.listenerCount("data"), 0);
  assert.equal(child.stderr.listenerCount("data"), 0);
});

test("runNegativeParityProcess clears a preinstalled grace timer when graceful kill closes synchronously", async () => {
  const { child, timers, runtime } = createFakeProcessRuntime();
  child.kill = (signal = undefined) => {
    child.kills.push(signal);
    if (signal === undefined) child.emit("close", 1, null);
    return true;
  };
  const pending = runNegativeParityProcess({ fileName: "node", arguments: [], cwd: repositoryRoot, timeoutMs: 120000 }, runtime);
  timers[0].callback();
  const result = await pending;
  assert.equal(result.exitCode, 1);
  assert.equal(timers.length, 2);
  assert.equal(timers.every((timer) => timer.cleared), true);
  assert.equal(child.listenerCount("close"), 0);
});

test("runNegativeParityProcess force-settles when grace timer installation throws", async () => {
  const { child, timers, runtime } = createFakeProcessRuntime();
  let timerCalls = 0;
  runtime.setTimer = (callback, delay) => {
    timerCalls += 1;
    if (timerCalls === 1) {
      const timer = { callback, delay, cleared: false };
      timers.push(timer);
      return timer;
    }
    throw new Error("grace timer unavailable");
  };
  const pending = runNegativeParityProcess({ fileName: "node", arguments: [], cwd: repositoryRoot, timeoutMs: 120000 }, runtime);
  timers[0].callback();
  const result = await pending;
  assert.equal(result.signal, "SIGKILL");
  assert.deepEqual(child.kills, ["SIGKILL"]);
  assert.equal(child.listenerCount("close"), 0);
});

test("runNegativeParityProcess kills and rejects when initial timeout timer installation throws", async () => {
  const { child, runtime } = createFakeProcessRuntime();
  runtime.setTimer = () => { throw new Error("timeout timer unavailable"); };
  await assert.rejects(
    runNegativeParityProcess({ fileName: "node", arguments: [], cwd: repositoryRoot, timeoutMs: 120000 }, runtime),
    /timeout timer unavailable/
  );
  assert.deepEqual(child.kills, ["SIGKILL"]);
  assert.equal(child.listenerCount("close"), 0);
  assert.equal(child.stdout.listenerCount("data"), 0);
});

test("runNegativeParity creates a contained marked fixture and invokes fixed child commands", async () => {
  const calls = [];
  const before = generatedFixtureSnapshot();
  const result = await runNegativeParity({ repositoryRoot, processRunner: makeRunner({ calls }) });
  assert.deepEqual(result, {
    pesterExecuted: 1,
    pesterFailed: 1,
    pesterNotRun: 49,
    nodeFailed: 1,
    nodeSkipped: 49,
    cleanup: "pass"
  });
  assert.equal(calls.length, 2);
  const [pester, node] = calls;
  assert.equal(pester.shell, false);
  assert.equal(pester.windowsHide, true);
  assert.equal(pester.stdin, "ignore");
  assert.equal(pester.timeoutMs, 120000);
  assert.deepEqual(pester.arguments.slice(0, 4), ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command"]);
  assert.equal(pester.arguments.length, 5);
  assert.match(pester.arguments[4], /FullNameFilter/);
  assert.match(pester.arguments[4], /\$failureMessage/);
  assert.equal(Object.hasOwn(pester.env, "PCV_WEB_NEGATIVE_PESTER_PATH"), true);
  assert.equal(node.fileName, process.execPath);
  assert.deepEqual(node.arguments, [
    "--test",
    "--test-reporter=tap",
    "node-tests/web-static-contracts.test.mjs"
  ]);
  assert.equal(node.cwd, path.join(repositoryRoot, "web"));
  assert.equal(node.shell, false);
  assert.equal(node.windowsHide, true);
  assert.equal(node.stdin, "ignore");
  assert.equal(node.timeoutMs, 120000);
  assert.equal(node.env.PCV_WEB_CONTRACT_FIXTURE_MODE, "negative-parity-v1");
  assert.match(node.env.PCV_WEB_CONTRACT_FIXTURE_ROOT, /^.+pcv-web-contract-negative-/i);
  assert.deepEqual(generatedFixtureSnapshot(), before);
});

test("runNegativeParity rejects a raw Node name-filter result that reports only one test", async () => {
  const nodeFilteredTap = nodeTap
    .replace("1..50", "1..1")
    .replace("# tests 50", "# tests 1")
    .replace("# skipped 49", "# skipped 0");
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: makeRunner({ node: { stdout: nodeFilteredTap } })
    }),
    /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|node_summary=unexpected/
  );
});

test("runNegativeParity observes an honest raw 50/0/1/49 Node TAP result", async () => {
  let rawNodeOutput = "";
  const result = await runNegativeParity({
    repositoryRoot,
    processRunner: async (request) => {
      if (request.fileName !== process.execPath) return makeRunner()({ fileName: "pwsh" });
      const childEnv = { ...request.env };
      delete childEnv.NODE_TEST_CONTEXT;
      const child = spawnSync(request.fileName, request.arguments, {
        cwd: request.cwd,
        env: childEnv,
        encoding: "utf8",
        shell: false,
        windowsHide: true,
        stdio: "pipe"
      });
      rawNodeOutput = `${child.stdout}${child.stderr}`;
      return {
        exitCode: child.status,
        signal: child.signal,
        timedOut: false,
        stdout: child.stdout,
        stderr: child.stderr
      };
    }
  });
  assert.deepEqual(parseNodeTap(rawNodeOutput), { tests: 50, passed: 0, failed: 1, skipped: 49 });
  assert.match(rawNodeOutput, /web\.static\.root-assets/);
  assert.match(rawNodeOutput, /app-root/);
  assert.equal(result.nodeFailed, 1);
  assert.equal(result.nodeSkipped, 49);
});

test("runNegativeParity rejects a wrong Pester failure label and redacts output", async () => {
  const before = generatedFixtureSnapshot();
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: makeRunner({ pester: { stdout: JSON.stringify({ total: 50, passed: 0, failed: 1, skipped: 0, not_run: 49, failure: "wrong cause password=hunter2" }) } })
    }),
    (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
      && !error.message.includes("hunter2")
      && error.message.includes("[REDACTED]")
  );
  assert.deepEqual(generatedFixtureSnapshot(), before);
});

test("runNegativeParity redacts the complete common secret and proxy-auth key vocabulary", async () => {
  for (const [input, value] of [
    ["client_secret=client-secret-value", "client-secret-value"],
    ["api_key=api-key-value", "api-key-value"],
    ["api-key=hyphen-key-value", "hyphen-key-value"],
    ["apikey=compact-key-value", "compact-key-value"],
    ["x-api-key=x-api-secret", "x-api-secret"],
    ["Proxy-Authorization: Digest proxy-secret", "proxy-secret"]
  ]) {
    await assert.rejects(
      runNegativeParity({ repositoryRoot, processRunner: makeRunner({ pester: { stdout: `not-json ${input}` } }) }),
      (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
        && !error.message.includes(value)
        && error.message.includes("[REDACTED]")
    );
  }
});

test("runNegativeParity fails closed when combined child output overflows 8192 characters", async () => {
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: async (request) => request.fileName === process.execPath
        ? makeRunner()({ fileName: process.execPath })
        : { exitCode: 1, signal: null, timedOut: false, overflow: true, stdout: pesterSummary, stderr: "x".repeat(8193) }
    }),
    /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|pester_output=overflow/
  );
});

test("runNegativeParity redacts malformed child output before reporting it", async () => {
  const secretPath = path.join(repositoryRoot, "private-token.txt");
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: makeRunner({ pester: { stdout: `not-json password=hunter2 ${secretPath}` } })
    }),
    (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
      && !error.message.includes("hunter2")
      && !error.message.includes(secretPath)
      && error.message.includes("[REDACTED]")
  );
});

test("runNegativeParity redacts Authorization headers and protected paths before the 8192-byte boundary", async () => {
  const fixturePath = path.join(os.tmpdir(), "pcv-web-contract-negative-ABC123");
  const alternateCaseRepositoryRoot = alternateCaseForwardSlashPath(repositoryRoot);
  const oversized = `${"x".repeat(8090)}Authorization: Basic basic-secret-token\r\nAuthorization: Bearer bearer-secret-token\r\nAuthorization: Digest digest-secret-token\r\n${repositoryRoot}\r\n${alternateCaseRepositoryRoot}\r\n${fixturePath}\r\n`;
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: makeRunner({ pester: { stdout: oversized } })
    }),
      (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
      && error.message.length <= 8192
      && !error.message.includes("Authorization: Basic")
      && !error.message.includes("basic-secret-token")
      && !error.message.includes("bearer-secret-token")
      && !error.message.includes("digest-secret-token")
      && !error.message.includes(repositoryRoot)
      && !error.message.includes(alternateCaseRepositoryRoot)
      && !error.message.includes(fixturePath)
      && error.message.includes("[REDACTED]")
  );
});

test("runNegativeParity redacts alternate-case forward-slash protected paths on Windows", async () => {
  const alternateCaseRepositoryRoot = alternateCaseForwardSlashPath(repositoryRoot);
  await assert.rejects(
    runNegativeParity({
      repositoryRoot,
      processRunner: makeRunner({ pester: { stdout: `not-json ${alternateCaseRepositoryRoot}` } })
    }),
    (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
      && !error.message.includes(alternateCaseRepositoryRoot)
      && error.message.includes("[REDACTED_PATH]")
  );
});

test("runNegativeParity rejects fixture roots outside temp", async () => {
  assert.throws(
    () => authorizeNegativeParityFixtureCandidate(repositoryRoot),
    /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|fixture_root=outside-temp/
  );
});

test("runNegativeParity rejects legacy fixture injection without touching a pre-existing matching directory", async () => {
  const existing = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-contract-negative-"));
  const sentinel = path.join(existing, "sentinel.txt");
  fs.writeFileSync(sentinel, "preserve\n", "utf8");
  try {
    await assert.rejects(
      runNegativeParity({ repositoryRoot, fixtureFactory: () => existing, processRunner: makeRunner() }),
      /PCV_WEB_CONTRACT_CONFIG_INVALID\|fixture_factory=unsupported/
    );
    assert.equal(fs.readFileSync(sentinel, "utf8"), "preserve\n");
  } finally {
    fs.rmSync(existing, { recursive: true, force: true });
  }
});

test("runNegativeParity cleans a marker-missing generated fixture without masking the work failure", async () => {
  let fixtureRoot;
  const before = generatedFixtureSnapshot();
  try {
    await assert.rejects(
      runNegativeParity({
        repositoryRoot,
        processRunner: makeRunner(),
        beforeChildren: ({ fixtureRoot: created }) => {
          fixtureRoot = created;
          fs.unlinkSync(path.join(created, ".pcv-web-contract-negative-v1"));
        }
      }),
      (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
        && error.message.includes("fixture_marker=missing")
        && !error.message.includes("cleanup=failed")
    );
    assert.ok(fixtureRoot);
    assert.equal(fs.existsSync(fixtureRoot), false);
  } finally {
    if (fixtureRoot) fs.rmSync(fixtureRoot, { recursive: true, force: true });
    assert.deepEqual(generatedFixtureSnapshot(), before);
  }
});

test("runNegativeParity rejects a replacement fixture before running children and does not delete it", async () => {
  let fixtureRoot;
  const calls = [];
  try {
    await assert.rejects(
      runNegativeParity({
        repositoryRoot,
        processRunner: makeRunner({ calls }),
        beforeChildren: ({ fixtureRoot: created }) => {
          fixtureRoot = created;
          fs.rmSync(created, { recursive: true, force: true });
          fs.mkdirSync(created);
          fs.writeFileSync(path.join(created, ".pcv-web-contract-negative-v1"), "replacement\n", "utf8");
        }
      }),
      (error) => error.code === "PCV_WEB_CONTRACT_FIXTURE_UNSAFE"
        && error.message.includes("cleanup=failed")
        && error.message.includes("identity-changed")
    );
    assert.equal(calls.length, 0);
    assert.equal(fs.existsSync(fixtureRoot), true);
  } finally {
    if (fixtureRoot) fs.rmSync(fixtureRoot, { recursive: true, force: true });
  }
});

test("runNegativeParity rejects a lexical temp junction before touching its target", async (t) => {
  const target = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-contract-negative-"));
  const link = path.join(os.tmpdir(), "pcv-web-contract-negative-ZYXWVU");
  try {
    try {
      fs.symlinkSync(target, link, "junction");
    } catch (error) {
      if (error?.code === "EPERM" || error?.code === "EACCES") {
        t.skip("junction creation unavailable on this host");
        return;
      }
      throw error;
    }
    assert.throws(
      () => authorizeNegativeParityFixtureCandidate(link),
      /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|fixture_root=reparse-invalid/
    );
    assert.equal(fs.lstatSync(link).isSymbolicLink(), true);
    assert.equal(fs.existsSync(target), true);
    assert.equal(fs.existsSync(path.join(target, ".pcv-web-contract-negative-v1")), false);
  } finally {
    fs.rmSync(link, { recursive: true, force: true });
    fs.rmSync(target, { recursive: true, force: true });
  }
});

test("runNegativeParity makes cleanup failure terminal", async () => {
  let tempRoot;
  try {
    await assert.rejects(
      runNegativeParity({
        repositoryRoot,
        processRunner: makeRunner(),
        beforeChildren: ({ fixtureRoot }) => { tempRoot = fixtureRoot; },
        cleanupFixture: () => { throw new Error("cannot remove"); }
      }),
      /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|cleanup=failed/
    );
  } finally {
    fs.rmSync(tempRoot, { recursive: true, force: true });
  }
});

test("runNegativeParity rejects a no-op cleanup callback that leaves its authorized fixture behind", async () => {
  let tempRoot;
  const before = generatedFixtureSnapshot();
  try {
    await assert.rejects(
      runNegativeParity({
        repositoryRoot,
        processRunner: makeRunner(),
        beforeChildren: ({ fixtureRoot }) => { tempRoot = fixtureRoot; },
        cleanupFixture: () => {}
      }),
      /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|cleanup=failed/
    );
    assert.equal(fs.existsSync(tempRoot), true);
  } finally {
    fs.rmSync(tempRoot, { recursive: true, force: true });
    assert.deepEqual(generatedFixtureSnapshot(), before);
  }
});

test("runNegativeParity treats a dangling cleanup link as a terminal cleanup failure", async (t) => {
  let tempRoot;
  const target = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-contract-negative-target-"));
  try {
    await assert.rejects(
      runNegativeParity({
        repositoryRoot,
        processRunner: makeRunner(),
        beforeChildren: ({ fixtureRoot }) => { tempRoot = fixtureRoot; },
        cleanupFixture: (authorizedRoot) => {
          fs.rmSync(authorizedRoot, { recursive: true, force: true });
          try {
            fs.symlinkSync(target, authorizedRoot, "junction");
          } catch (error) {
            if (error?.code === "EPERM" || error?.code === "EACCES") {
              t.skip("junction creation unavailable on this host");
              return;
            }
            throw error;
          }
          fs.rmSync(target, { recursive: true, force: true });
        }
      }),
      /PCV_WEB_CONTRACT_FIXTURE_UNSAFE\|cleanup=failed/
    );
    assert.equal(fs.lstatSync(tempRoot).isSymbolicLink(), true);
  } finally {
    fs.rmSync(tempRoot, { recursive: true, force: true });
    fs.rmSync(target, { recursive: true, force: true });
  }
});

test("runNegativeParity rejects unknown CLI-style arguments", async () => {
  await assert.rejects(
    runNegativeParity({ repositoryRoot, args: ["--unexpected"], processRunner: makeRunner() }),
    /PCV_WEB_CONTRACT_CONFIG_INVALID\|arguments=invalid/
  );
});

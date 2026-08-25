import { spawn } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const ERROR_FIXTURE_UNSAFE = "PCV_WEB_CONTRACT_FIXTURE_UNSAFE";
const ERROR_CONFIG_INVALID = "PCV_WEB_CONTRACT_CONFIG_INVALID";
const CAPTURE_LIMIT = 8192;
const TIMEOUT_MS = 120_000;
const TERMINATION_GRACE_MS = 1_000;
const FIXTURE_PREFIX = "pcv-web-contract-negative-";
const FIXTURE_MARKER = ".pcv-web-contract-negative-v1";
const PESTER_FULL_NAME = "PcvDesktopWeb static console assets.ships index, stylesheet, and script assets under the Desktop Node web root";
const PESTER_SCRIPT = `$fullName = '${PESTER_FULL_NAME}'
$pesterPath = $env:PCV_WEB_NEGATIVE_PESTER_PATH
if ([string]::IsNullOrWhiteSpace($pesterPath)) { exit 3 }
$result = Invoke-Pester -Path $pesterPath -FullNameFilter $fullName -PassThru -Output None
$failed = @($result.Tests | Where-Object Result -eq 'Failed')
$failureMessage = $failed[0].ErrorRecord.Exception.Message
if ($failureMessage.Length -gt 512) { $failureMessage = $failureMessage.Substring(0, 512) }
[ordered]@{
  total = $result.TotalCount
  passed = $result.PassedCount
  failed = $result.FailedCount
  skipped = $result.SkippedCount
  not_run = $result.NotRunCount
  failure = $failureMessage
} | ConvertTo-Json -Compress
if ($failed.Count -eq 1 -and $result.FailedCount -eq 1) { exit 1 }
exit 2`;
const REQUIRED_FILES = Object.freeze([
  "web/tests/PcvDesktopWeb.Static.Tests.ps1",
  "web/index.html",
  "web/styles.css",
  "web/app.js",
  "web/scripts/build-served-asset.mjs"
]);

export class WebNegativeParityError extends Error {
  constructor(code, detail) {
    super(`${code}|${detail}`);
    this.name = "WebNegativeParityError";
    this.code = code;
  }
}

function configInvalid(detail) {
  return new WebNegativeParityError(ERROR_CONFIG_INVALID, detail);
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function redact(value, roots = []) {
  let output = String(value)
    .replace(/(?:Authorization|Proxy-Authorization|X-Api-Key)\s*:\s*[^\r\n]*/gi, "Authorization: [REDACTED]")
    .replace(/Bearer\s+[A-Za-z0-9._~+/=-]+/gi, "Bearer [REDACTED]")
    .replace(/((?:password|access_token|refresh_token|api_token|client_secret|api[_-]?key|apikey|x[_-]?api[_-]?key)\s*[=:]\s*)[^\s"']+/gi, "$1[REDACTED]");
  for (const root of [...new Set([...roots, os.homedir()])].filter(Boolean).sort((a, b) => b.length - a.length)) {
    output = output.replace(new RegExp(escapeRegExp(root), process.platform === "win32" ? "gi" : "g"), "[REDACTED_PATH]");
    output = output.replace(
      new RegExp(escapeRegExp(root.replaceAll("\\", "/")), process.platform === "win32" ? "gi" : "g"),
      "[REDACTED_PATH]"
    );
  }
  return output;
}

function unsafe(detail, output = "", roots = []) {
  const prefix = `${ERROR_FIXTURE_UNSAFE}|${detail}`;
  const outputPrefix = ";output=";
  const available = Math.max(0, CAPTURE_LIMIT - prefix.length - outputPrefix.length);
  const suffix = output ? `${outputPrefix}${redact(output, roots).slice(0, available)}` : "";
  return new WebNegativeParityError(ERROR_FIXTURE_UNSAFE, `${detail}${suffix}`);
}

function isInside(parent, candidate) {
  const relative = path.relative(parent, candidate);
  return Boolean(relative) && !path.isAbsolute(relative) && relative !== ".." && !relative.startsWith(`..${path.sep}`);
}

function realDirectory(candidate, detail) {
  try {
    const real = fs.realpathSync(candidate);
    if (!fs.statSync(real).isDirectory()) throw new Error("not-directory");
    return real;
  } catch {
    throw unsafe(detail);
  }
}

function lstatOrMissing(candidate, detail) {
  try {
    return fs.lstatSync(candidate);
  } catch (error) {
    if (error?.code === "ENOENT") return undefined;
    throw unsafe(detail);
  }
}

function requireInteger(object, name) {
  if (!Number.isSafeInteger(object?.[name]) || object[name] < 0) {
    throw unsafe(`summary=${name}-invalid`);
  }
  return object[name];
}

export function parsePesterSummary(output) {
  if (typeof output !== "string" || output.length === 0 || output.length > CAPTURE_LIMIT) {
    throw unsafe("pester_output=invalid");
  }
  let parsed;
  try {
    parsed = JSON.parse(output.trim());
  } catch {
    throw unsafe("pester_output=unparseable", output);
  }
  if (parsed === null || Array.isArray(parsed) || typeof parsed !== "object") {
    throw unsafe("pester_output=unparseable", output);
  }
  const summary = {
    total: requireInteger(parsed, "total"),
    passed: requireInteger(parsed, "passed"),
    failed: requireInteger(parsed, "failed"),
    skipped: requireInteger(parsed, "skipped"),
    notRun: requireInteger(parsed, "not_run")
  };
  if (summary.total !== summary.passed + summary.failed + summary.skipped + summary.notRun) {
    throw unsafe("pester_summary=inconsistent", output);
  }
  if (summary.total - summary.skipped - summary.notRun === 0) {
    throw unsafe("pester_summary=zero-executed", output);
  }
  return summary;
}

function tapCount(output, name) {
  const matches = [...output.matchAll(new RegExp(`^# ${name} (\\d+)\\s*$`, "gm"))];
  if (matches.length !== 1) throw unsafe(`node_tap=${name}-invalid`, output);
  const value = Number(matches[0][1]);
  if (!Number.isSafeInteger(value) || value < 0) throw unsafe(`node_tap=${name}-invalid`, output);
  return value;
}

export function parseNodeTap(output) {
  if (typeof output !== "string" || output.length === 0 || output.length > CAPTURE_LIMIT) {
    throw unsafe("node_output=invalid");
  }
  const summary = {
    tests: tapCount(output, "tests"),
    passed: tapCount(output, "pass"),
    failed: tapCount(output, "fail"),
    skipped: tapCount(output, "skipped")
  };
  if (summary.tests !== summary.passed + summary.failed + summary.skipped) {
    throw unsafe("node_tap=inconsistent", output);
  }
  if (summary.passed !== 0 || summary.failed !== 1) {
    throw unsafe("node_tap=unexpected-failure-count", output);
  }
  return summary;
}

export function runNegativeParityProcess(request, runtime = {
  spawnProcess: spawn,
  setTimer: setTimeout,
  clearTimer: clearTimeout
}) {
  return new Promise((resolve, reject) => {
    let child;
    let timeoutTimer;
    let graceTimer;
    let settled = false;
    let setupFailed = false;
    let stdout = "";
    let stderr = "";
    let captured = 0;
    let overflow = false;
    const append = (stream, chunk) => {
      const value = String(chunk);
      const remaining = CAPTURE_LIMIT - captured;
      if (value.length > remaining) overflow = true;
      const accepted = value.slice(0, Math.max(0, remaining));
      captured += accepted.length;
      if (stream === "stdout") stdout += accepted;
      else stderr += accepted;
    };
    const onStdout = (chunk) => append("stdout", chunk);
    const onStderr = (chunk) => append("stderr", chunk);
    const destroy = (stream, listener) => {
      stream?.removeListener?.("data", listener);
      try { stream?.destroy?.(); } catch { /* cleanup remains bounded */ }
    };
    const cleanup = () => {
      if (timeoutTimer !== undefined) runtime.clearTimer(timeoutTimer);
      if (graceTimer !== undefined) runtime.clearTimer(graceTimer);
      child?.removeListener?.("error", onError);
      child?.removeListener?.("close", onClose);
      destroy(child?.stdout, onStdout);
      destroy(child?.stderr, onStderr);
    };
    const settle = (handler, value) => {
      if (settled) return;
      settled = true;
      cleanup();
      handler(value);
    };
    const result = (exitCode, signal, timedOut) => ({ exitCode, signal, timedOut, stdout, stderr, overflow });
    function onError(error) {
      if (!setupFailed) settle(reject, error);
    }
    function onClose(exitCode, signal) {
      if (!setupFailed) settle(resolve, result(exitCode, signal, timedOut));
    }
    let timedOut = false;
    const force = () => {
      if (settled) return;
      try { child.kill("SIGKILL"); } catch { /* bounded settlement still follows */ }
      settle(resolve, result(null, "SIGKILL", true));
    };
    const onTimeout = () => {
      if (settled) return;
      timedOut = true;
      try {
        // Install the bounded owner timer before a graceful kill can synchronously close.
        graceTimer = runtime.setTimer(force, TERMINATION_GRACE_MS);
      } catch {
        force();
        return;
      }
      try { child.kill(); } catch { /* the installed escalation timer still bounds the child */ }
    };
    try {
      child = runtime.spawnProcess(request.fileName, request.arguments, {
        cwd: request.cwd,
        env: request.env,
        shell: false,
        windowsHide: true,
        stdio: ["ignore", "pipe", "pipe"]
      });
      child.stdout?.on("data", onStdout);
      child.stderr?.on("data", onStderr);
      child.once("error", onError);
      child.once("close", onClose);
      timeoutTimer = runtime.setTimer(onTimeout, request.timeoutMs);
    } catch (error) {
      setupFailed = true;
      try { child?.kill?.("SIGKILL"); } catch { /* rejection below remains deterministic */ }
      settle(reject, error);
    }
  });
}

export function authorizeNegativeParityFixtureCandidate(root, tempRoot = realDirectory(os.tmpdir(), "temp_root=invalid")) {
  const lexicalPath = path.resolve(root);
  const lexical = lstatOrMissing(lexicalPath, "fixture_root=identity-unavailable");
  if (lexical === undefined) throw unsafe("fixture_root=realpath-failed");
  if (!lexical.isDirectory() || lexical.isSymbolicLink()) throw unsafe("fixture_root=reparse-invalid");
  const real = realDirectory(lexicalPath, "fixture_root=realpath-failed");
  if (!isInside(tempRoot, real)) throw unsafe("fixture_root=outside-temp");
  const basename = path.basename(real);
  const suffix = basename.slice(FIXTURE_PREFIX.length);
  if (!basename.startsWith(FIXTURE_PREFIX) || !/^[A-Za-z0-9]{6}$/.test(suffix)) {
    throw unsafe("fixture_root=generated-name-invalid");
  }
  let stat;
  try {
    stat = fs.statSync(real);
  } catch {
    throw unsafe("fixture_root=identity-unavailable");
  }
  if (!stat.isDirectory()) {
    throw unsafe("fixture_root=reparse-invalid");
  }
  return Object.freeze({
    tempRoot,
    basename,
    lexicalPath,
    realPath: real,
    lexicalDevice: lexical.dev,
    lexicalInode: lexical.ino,
    device: stat.dev,
    inode: stat.ino
  });
}

function validateAuthorizedFixture(authorization, { allowMissing = false } = {}) {
  if (authorization === null || typeof authorization !== "object") throw unsafe("fixture_root=authorization-invalid");
  const lexical = lstatOrMissing(authorization.lexicalPath, "fixture_root=identity-unavailable");
  if (lexical === undefined) {
    if (allowMissing) return false;
    throw unsafe("fixture_root=missing-before-cleanup");
  }
  if (!lexical.isDirectory() || lexical.isSymbolicLink()) throw unsafe("fixture_root=identity-changed");
  const real = realDirectory(authorization.lexicalPath, "fixture_root=realpath-failed");
  if (!isInside(authorization.tempRoot, real) || path.basename(real) !== authorization.basename) {
    throw unsafe("fixture_root=outside-temp");
  }
  const suffix = authorization.basename.slice(FIXTURE_PREFIX.length);
  if (!authorization.basename.startsWith(FIXTURE_PREFIX) || !/^[A-Za-z0-9]{6}$/.test(suffix)) {
    throw unsafe("fixture_root=generated-name-invalid");
  }
  let stat;
  try {
    stat = fs.statSync(real);
  } catch {
    throw unsafe("fixture_root=identity-unavailable");
  }
  if (
    real !== authorization.realPath || !stat.isDirectory()
    || lexical.dev !== authorization.lexicalDevice || lexical.ino !== authorization.lexicalInode
    || stat.dev !== authorization.device || stat.ino !== authorization.inode
  ) throw unsafe("fixture_root=identity-changed");
  return true;
}

function requireFixtureMarker(authorization) {
  validateAuthorizedFixture(authorization);
  if (!fs.existsSync(path.join(authorization.realPath, FIXTURE_MARKER))) {
    throw unsafe("fixture_marker=missing");
  }
}

function copyFixture(repositoryRoot, fixtureRoot) {
  for (const relativePath of REQUIRED_FILES) {
    const source = path.join(repositoryRoot, ...relativePath.split("/"));
    if (!fs.existsSync(source) || !fs.statSync(source).isFile()) throw unsafe("fixture_source=missing");
    const destination = path.join(fixtureRoot, ...relativePath.split("/"));
    fs.mkdirSync(path.dirname(destination), { recursive: true });
    fs.copyFileSync(source, destination);
  }
  const indexPath = path.join(fixtureRoot, "web", "index.html");
  const index = fs.readFileSync(indexPath, "utf8");
  const matches = [...index.matchAll(/id="app-root"/g)];
  if (matches.length !== 1) throw unsafe("fixture_defect=app-root-count-invalid");
  fs.writeFileSync(indexPath, index.replace("id=\"app-root\"", ""), "utf8");
}

function childSucceededAsExpected(result) {
  return result?.exitCode === 1 && result?.signal == null && result?.timedOut === false;
}

function childOutput(result) {
  return `${String(result?.stdout ?? "")}\n${String(result?.stderr ?? "")}`;
}

function verifyPester(result, roots) {
  const output = childOutput(result);
  if (result?.overflow === true) throw unsafe("pester_output=overflow", output, roots);
  if (!childSucceededAsExpected(result)) throw unsafe("pester_exit=unexpected", output, roots);
  let parsed;
  try {
    parsed = JSON.parse(String(result.stdout ?? "").trim());
  } catch {
    throw unsafe("pester_output=unparseable", output, roots);
  }
  const summary = parsePesterSummary(JSON.stringify({
    total: parsed.total,
    passed: parsed.passed,
    failed: parsed.failed,
    skipped: parsed.skipped,
    not_run: parsed.not_run
  }));
  if (
    summary.total !== 50 || summary.passed !== 0 || summary.failed !== 1
    || summary.skipped !== 0 || summary.notRun !== 49
  ) throw unsafe("pester_summary=unexpected", output, roots);
  if (typeof parsed.failure !== "string" || !parsed.failure.includes("app-root")) {
    throw unsafe("pester_failure=wrong-label", output, roots);
  }
  return summary;
}

function verifyNode(result, roots) {
  const output = childOutput(result);
  if (result?.overflow === true) throw unsafe("node_output=overflow", output, roots);
  if (!childSucceededAsExpected(result)) throw unsafe("node_exit=unexpected", output, roots);
  const summary = parseNodeTap(output);
  if (summary.tests !== 50 || summary.passed !== 0 || summary.failed !== 1 || summary.skipped !== 49) {
    throw unsafe("node_summary=unexpected", output, roots);
  }
  if (!output.includes("web.static.root-assets") || !output.includes("app-root")) {
    throw unsafe("node_failure=wrong-label", output, roots);
  }
  return summary;
}

export async function runNegativeParity({
  repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", ".."),
  args = [],
  processRunner = runNegativeParityProcess,
  fixtureFactory = undefined,
  beforeChildren = undefined,
  cleanupFixture = undefined
} = {}) {
  if (!Array.isArray(args) || args.length !== 0) throw configInvalid("arguments=invalid");
  if (typeof processRunner !== "function") throw configInvalid("process_runner=invalid");
  if (fixtureFactory !== undefined) throw configInvalid("fixture_factory=unsupported");
  if (beforeChildren !== undefined && typeof beforeChildren !== "function") throw configInvalid("before_children=invalid");
  if (cleanupFixture !== undefined && typeof cleanupFixture !== "function") throw configInvalid("cleanup_fixture=invalid");

  const sourceRoot = realDirectory(repositoryRoot, "repository_root=invalid");
  const tempRoot = realDirectory(os.tmpdir(), "temp_root=invalid");
  let fixtureAuthorization;
  let workError;
  try {
    const created = fs.mkdtempSync(path.join(tempRoot, FIXTURE_PREFIX));
    fixtureAuthorization = authorizeNegativeParityFixtureCandidate(created, tempRoot);
    fs.writeFileSync(path.join(fixtureAuthorization.realPath, FIXTURE_MARKER), "negative-parity-v1\n", "utf8");
    copyFixture(sourceRoot, fixtureAuthorization.realPath);
    requireFixtureMarker(fixtureAuthorization);
    if (beforeChildren !== undefined) beforeChildren({ fixtureRoot: fixtureAuthorization.realPath });
    requireFixtureMarker(fixtureAuthorization);

    const pesterPath = path.join(fixtureAuthorization.realPath, "web", "tests", "PcvDesktopWeb.Static.Tests.ps1");
    const pesterResult = await processRunner({
      fileName: process.platform === "win32" ? "pwsh.exe" : "pwsh",
      arguments: ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command", PESTER_SCRIPT],
      cwd: fixtureAuthorization.realPath,
      env: { ...process.env, PCV_WEB_NEGATIVE_PESTER_PATH: pesterPath },
      shell: false,
      windowsHide: true,
      stdin: "ignore",
      timeoutMs: TIMEOUT_MS
    });
    const pester = verifyPester(pesterResult, [sourceRoot, fixtureAuthorization.realPath]);

    const nodeResult = await processRunner({
      fileName: process.execPath,
      arguments: ["--test", "--test-reporter=tap", "node-tests/web-static-contracts.test.mjs"],
      cwd: path.join(sourceRoot, "web"),
      env: {
        ...process.env,
        PCV_WEB_CONTRACT_FIXTURE_MODE: "negative-parity-v1",
        PCV_WEB_CONTRACT_FIXTURE_ROOT: fixtureAuthorization.realPath
      },
      shell: false,
      windowsHide: true,
      stdin: "ignore",
      timeoutMs: TIMEOUT_MS
    });
    const node = verifyNode(nodeResult, [sourceRoot, fixtureAuthorization.realPath]);
    return {
      pesterExecuted: pester.total - pester.skipped - pester.notRun,
      pesterFailed: pester.failed,
      pesterNotRun: pester.notRun,
      nodeFailed: node.failed,
      nodeSkipped: node.skipped,
      cleanup: "pass"
    };
  } catch (error) {
    workError = error instanceof WebNegativeParityError
      ? error
      : unsafe("execution=failed", String(error?.message ?? error), [sourceRoot, fixtureAuthorization?.realPath]);
    throw workError;
  } finally {
    if (fixtureAuthorization !== undefined) {
      try {
        validateAuthorizedFixture(fixtureAuthorization);
        if (cleanupFixture === undefined) fs.rmSync(fixtureAuthorization.realPath, { recursive: true, force: false });
        else cleanupFixture(fixtureAuthorization.realPath);
        if (validateAuthorizedFixture(fixtureAuthorization, { allowMissing: true })) {
          throw unsafe("cleanup=target-remains");
        }
      } catch (cleanupError) {
        throw unsafe("cleanup=failed", String(cleanupError?.message ?? cleanupError), [sourceRoot, fixtureAuthorization.realPath]);
      }
    }
  }
}

async function main() {
  const result = await runNegativeParity({ args: process.argv.slice(2) });
  process.stdout.write(
    `Web negative parity PASS: defect=missing-app-root pester_executed=${result.pesterExecuted} pester_failed=${result.pesterFailed} pester_not_run=${result.pesterNotRun} node_failed=${result.nodeFailed} node_skipped=${result.nodeSkipped} cleanup=${result.cleanup}\n`
  );
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  main().catch((error) => {
    process.stderr.write(`${error?.message ?? ERROR_FIXTURE_UNSAFE}\n`);
    process.exitCode = 1;
  });
}

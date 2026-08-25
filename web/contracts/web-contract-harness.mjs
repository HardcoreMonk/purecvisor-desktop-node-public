import { spawn } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";

export const WEB_CONTRACT_ERROR_CODES = Object.freeze({
  configInvalid: "PCV_WEB_CONTRACT_CONFIG_INVALID",
  registryMismatch: "PCV_WEB_CONTRACT_REGISTRY_MISMATCH",
  fileMissing: "PCV_WEB_CONTRACT_FILE_MISSING",
  assertionFailed: "PCV_WEB_CONTRACT_ASSERTION_FAILED",
  ownerFailed: "PCV_WEB_CONTRACT_OWNER_FAILED",
  fixtureUnsafe: "PCV_WEB_CONTRACT_FIXTURE_UNSAFE",
  manifestInvalid: "PCV_VERIFICATION_MIGRATION_MANIFEST_INVALID"
});

const OWNER_COMMANDS = {
  "feature-surface": [["scripts/verify-feature-surface-parity.mjs"]],
  "typescript": [["node_modules/typescript/bin/tsc", "--noEmit", "-p", "tsconfig.json"]],
  "served-asset": [["scripts/build-served-asset.mjs", "--check"]],
  "frontend-batches": [["scripts/validate-frontend-completion-batches.mjs"]],
  "static-parity": [
    ["scripts/regenerate-static-parity.mjs", "--check"],
    ["scripts/verify-static-parity.mjs"]
  ],
  "browser-fixture": [["scripts/verify-browser-fixture.mjs"]],
  "node-check": [["--check", "app.js"]],
  "static-contract": []
};

const OWNER_TIMEOUT_MS = 120_000;
const OWNER_TERMINATION_GRACE_MS = 1_000;
const OWNER_CAPTURE_LIMIT = 16_384;
const OWNER_EXPOSED_OUTPUT_LIMIT = 8192;
const DEFAULT_OWNER_PROCESS_RUNTIME = Object.freeze({
  spawnProcess: spawn,
  setTimer: setTimeout,
  clearTimer: clearTimeout
});

export class WebContractError extends Error {
  constructor(code, detail, cause = undefined) {
    super(`${code}|${detail}`, { cause });
    this.name = "WebContractError";
    this.code = code;
  }
}

function configError(detail, cause = undefined) {
  return new WebContractError(WEB_CONTRACT_ERROR_CODES.configInvalid, detail, cause);
}

function sanitizedFilesystemCause(error) {
  const cause = new Error(typeof error?.code === "string" ? error.code : "filesystem_error");
  cause.name = "FilesystemError";
  cause.stack = `${cause.name}: ${cause.message}`;
  return cause;
}

function sanitizedJsonCause() {
  const cause = new SyntaxError("invalid_json");
  cause.stack = "SyntaxError: invalid_json";
  return cause;
}

function normalizeRelative(relativePath) {
  if (typeof relativePath !== "string" || relativePath.length === 0 || relativePath.includes("\0")) {
    throw configError("path=invalid");
  }

  const forward = relativePath.replaceAll("\\", "/");
  if (forward.startsWith("/") || /^[A-Za-z]:/.test(forward)) {
    throw configError("path=invalid");
  }
  if (forward.split("/").includes("..")) {
    throw configError("path=escape");
  }

  const normalized = path.posix.normalize(forward);
  if (!normalized || normalized === "." || normalized.startsWith("/") || normalized.includes("\0")) {
    throw configError("path=invalid");
  }
  return normalized;
}

function hasExactBoundary(root, candidate) {
  if (candidate === root) {
    return true;
  }
  const boundary = root.endsWith(path.sep) ? root : `${root}${path.sep}`;
  return candidate.startsWith(boundary);
}

function createLocation(repoRoot, relativePath) {
  const normalized = normalizeRelative(relativePath);
  const resolved = path.resolve(repoRoot, ...normalized.split("/"));
  if (!hasExactBoundary(repoRoot, resolved)) {
    throw configError("path=escape");
  }
  return { normalized, resolved };
}

function verifyExistingContainment(repoRoot, location) {
  if (!fs.existsSync(location.resolved)) {
    return false;
  }

  let real;
  try {
    real = fs.realpathSync(location.resolved);
  } catch (error) {
    throw configError(`file=unreadable:${location.normalized}`, sanitizedFilesystemCause(error));
  }
  if (!hasExactBoundary(repoRoot, real)) {
    throw configError("path=escape");
  }
  return true;
}

function copyNormalizedOverrides(textOverrides) {
  if (!(textOverrides instanceof Map)) {
    throw configError("text_overrides=invalid");
  }
  const copy = new Map();
  for (const [key, value] of textOverrides) {
    if (typeof key !== "string") {
      continue;
    }
    try {
      if (normalizeRelative(key) === key) {
        if (typeof value !== "string") {
          throw configError("text_overrides=invalid");
        }
        copy.set(key, value);
      }
    } catch (error) {
      if (error instanceof WebContractError && error.message.endsWith("text_overrides=invalid")) {
        throw error;
      }
    }
  }
  return copy;
}

function copyNormalizedMissingPaths(missingPaths) {
  if (!(missingPaths instanceof Set)) {
    throw configError("missing_paths=invalid");
  }
  const copy = new Set();
  for (const key of missingPaths) {
    if (typeof key !== "string") {
      continue;
    }
    try {
      if (normalizeRelative(key) === key) {
        copy.add(key);
      }
    } catch {
      // Invalid and non-normalized fixture keys are deliberately ignored.
    }
  }
  return copy;
}

function resolveRepositoryRoot(repoRoot) {
  if (typeof repoRoot !== "string" || !repoRoot || repoRoot.includes("\0")) {
    throw configError("repo_root=invalid");
  }
  try {
    const real = fs.realpathSync(path.resolve(repoRoot));
    if (!fs.statSync(real).isDirectory()) {
      throw new Error("not_directory");
    }
    return real;
  } catch (error) {
    throw configError("repo_root=invalid", sanitizedFilesystemCause(error));
  }
}

function resolveWebRoot(repoRoot) {
  const location = createLocation(repoRoot, "web");
  if (!verifyExistingContainment(repoRoot, location)) {
    throw configError("web_root=invalid");
  }
  try {
    const real = fs.realpathSync(location.resolved);
    if (!fs.statSync(real).isDirectory()) {
      throw new Error("not_directory");
    }
    return real;
  } catch (error) {
    throw configError("web_root=invalid", sanitizedFilesystemCause(error));
  }
}

function validateOwnerIds(ownerIds) {
  if (!Array.isArray(ownerIds)) {
    throw configError("owner_ids=invalid");
  }
  for (const ownerId of ownerIds) {
    if (typeof ownerId !== "string" || ownerId.length === 0) {
      throw configError("owner_ids=invalid");
    }
    if (!Object.hasOwn(OWNER_COMMANDS, ownerId)) {
      throw configError(`owner=unknown:${ownerId}`);
    }
  }
  return [...ownerIds];
}

function validateOwnerTarget(webRoot, commandArguments) {
  const target = commandArguments[0] === "--check"
    ? commandArguments[1]
    : commandArguments[0];
  const location = createLocation(webRoot, target);
  if (!verifyExistingContainment(webRoot, location)) {
    throw configError(`owner_target=missing:${location.normalized}`);
  }
  try {
    if (!fs.statSync(location.resolved).isFile()) {
      throw configError(`owner_target=invalid:${location.normalized}`);
    }
  } catch (error) {
    if (error instanceof WebContractError) {
      throw error;
    }
    throw configError(
      `owner_target=unreadable:${location.normalized}`,
      sanitizedFilesystemCause(error)
    );
  }
}

function appendBounded(current, chunk) {
  if (current.length >= OWNER_CAPTURE_LIMIT) {
    return current;
  }
  return current + String(chunk).slice(0, OWNER_CAPTURE_LIMIT - current.length);
}

function normalizeOwnerProcessRuntime(runtime) {
  if (runtime === null || typeof runtime !== "object") {
    throw configError("owner_process_runtime=invalid");
  }
  const { spawnProcess, setTimer, clearTimer } = runtime;
  if (
    typeof spawnProcess !== "function"
    || typeof setTimer !== "function"
    || typeof clearTimer !== "function"
  ) {
    throw configError("owner_process_runtime=invalid");
  }
  return Object.freeze({ spawnProcess, setTimer, clearTimer });
}

function spawnOwnerProcess(request, runtime) {
  return new Promise((resolve, reject) => {
    let child;
    let timeoutTimer;
    let forceTimer;
    let settled = false;
    let timedOut = false;
    let stdout = "";
    let stderr = "";

    function onStdoutData(chunk) {
      stdout = appendBounded(stdout, chunk);
    }

    function onStderrData(chunk) {
      stderr = appendBounded(stderr, chunk);
    }

    function destroyStream(stream, listener) {
      stream?.removeListener?.("data", listener);
      try {
        stream?.destroy?.();
      } catch {
        // Settlement must not depend on inherited or already-closed pipe handles.
      }
    }

    function cleanup() {
      if (timeoutTimer !== undefined) {
        runtime.clearTimer(timeoutTimer);
        timeoutTimer = undefined;
      }
      if (forceTimer !== undefined) {
        runtime.clearTimer(forceTimer);
        forceTimer = undefined;
      }
      child?.removeListener?.("error", onError);
      child?.removeListener?.("close", onClose);
      destroyStream(child?.stdout, onStdoutData);
      destroyStream(child?.stderr, onStderrData);
    }

    const settle = (handler, value) => {
      if (settled) {
        return;
      }
      settled = true;
      cleanup();
      handler(value);
    };

    function onError(error) {
      settle(reject, error);
    }

    function onClose(exitCode, signal) {
      settle(resolve, { exitCode, signal, timedOut, stdout, stderr });
    }

    function forceTerminateAndSettle() {
      if (settled) {
        return;
      }
      try {
        child.kill("SIGKILL");
      } catch {
        // The bounded timeout result still settles when termination cannot be confirmed.
      }
      settle(resolve, {
        exitCode: null,
        signal: "SIGKILL",
        timedOut: true,
        stdout,
        stderr
      });
    }

    function onTimeout() {
      if (settled) {
        return;
      }
      timedOut = true;
      try {
        forceTimer = runtime.setTimer(forceTerminateAndSettle, OWNER_TERMINATION_GRACE_MS);
      } catch (error) {
        forceTerminateAndSettle();
        return;
      }
      try {
        child.kill();
      } catch {
        // Forced termination and bounded settlement still run after the grace period.
      }
    }

    try {
      child = runtime.spawnProcess(request.fileName, request.arguments, {
        cwd: request.cwd,
        shell: false,
        windowsHide: true,
        stdio: ["ignore", "pipe", "pipe"]
      });
    } catch (error) {
      settle(reject, error);
      return;
    }

    child.stdout?.on("data", onStdoutData);
    child.stderr?.on("data", onStderrData);
    child.once("error", onError);
    child.once("close", onClose);

    try {
      timeoutTimer = runtime.setTimer(onTimeout, request.timeoutMs);
    } catch (error) {
      try {
        child.kill("SIGKILL");
      } catch {
        // Setup failure is reported after best-effort child termination.
      }
      settle(reject, error);
    }
  });
}

function redact(value) {
  return String(value)
    .replace(
      /((?:Proxy-Authorization|Authorization|X-Api-Key)\s*:\s*)[^\r\n]*/gi,
      "$1[REDACTED]"
    )
    .replace(/Bearer\s+[A-Za-z0-9._~+/=-]+/gi, "Bearer [REDACTED]")
    .replace(
      /((?:password|access_token|refresh_token|api_token|client_secret|api[_-]?key|apikey|x[_-]?api[_-]?key)\s*[=:]\s*)[^\s"']+/gi,
      "$1[REDACTED]"
    );
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function redactPaths(value, repoRoot) {
  const candidates = [...new Set([
    repoRoot,
    repoRoot.replaceAll("\\", "/"),
    os.homedir(),
    os.homedir().replaceAll("\\", "/")
  ])].filter(Boolean).sort((left, right) => right.length - left.length);
  let redacted = value;
  for (const candidate of candidates) {
    const flags = process.platform === "win32" ? "gi" : "g";
    redacted = redacted.replace(new RegExp(escapeRegExp(candidate), flags), "[REDACTED_PATH]");
  }
  return redacted;
}

function ownerOutput(result, repoRoot) {
  const combined = `stdout=${String(result?.stdout ?? "")}\nstderr=${String(result?.stderr ?? "")}`;
  return redactPaths(redact(combined), repoRoot).slice(0, OWNER_EXPOSED_OUTPUT_LIMIT);
}

function ownerFailure(ownerId, result, repoRoot, runnerError = undefined) {
  const projectedResult = runnerError === undefined
    ? result
    : { stdout: "", stderr: `runner_error=${String(runnerError?.message ?? runnerError)}` };
  const status = runnerError === undefined
    ? `exit_code=${String(result?.exitCode ?? "null")};signal=${String(result?.signal ?? "none")};timed_out=${result?.timedOut === true}`
    : "runner=rejected";
  const error = new WebContractError(
    WEB_CONTRACT_ERROR_CODES.ownerFailed,
    `owner=${ownerId};${status}|output=${ownerOutput(projectedResult, repoRoot)}`
  );
  error.stack = `${error.name}: ${error.message}`;
  return error;
}

function projectOwnerError(error, contractId) {
  const prefix = `${error.code}|`;
  const detail = error.message.startsWith(prefix)
    ? error.message.slice(prefix.length)
    : "owner=failed";
  const projected = new WebContractError(error.code, detail);
  if (contractId !== undefined) {
    projected.contract_id = contractId;
  }
  projected.stack = `${projected.name}: ${projected.message}`;
  return projected;
}

function validateContractId(contractId) {
  if (typeof contractId !== "string" || !/^[A-Za-z0-9][A-Za-z0-9._-]*$/.test(contractId)) {
    throw configError("contract_id=invalid");
  }
  return contractId;
}

function maskInactiveJavaScript(source) {
  const masked = source.split("");
  const mask = (index) => {
    masked[index] = " ";
  };

  let index = 0;
  while (index < source.length) {
    const character = source[index];
    const next = source[index + 1];
    if (character === "/" && next === "/") {
      mask(index);
      mask(index + 1);
      index += 2;
      while (index < source.length && source[index] !== "\r" && source[index] !== "\n") {
        mask(index);
        index += 1;
      }
      continue;
    }
    if (character === "/" && next === "*") {
      mask(index);
      mask(index + 1);
      index += 2;
      while (index < source.length) {
        const closesComment = source[index] === "*" && source[index + 1] === "/";
        mask(index);
        index += 1;
        if (closesComment) {
          mask(index);
          index += 1;
          break;
        }
      }
      continue;
    }
    if (character === '"' || character === "'" || character === "`") {
      const delimiter = character;
      mask(index);
      index += 1;
      while (index < source.length) {
        const stringCharacter = source[index];
        mask(index);
        index += 1;
        if (stringCharacter === "\\" && index < source.length) {
          mask(index);
          index += 1;
        } else if (stringCharacter === delimiter) {
          break;
        }
      }
      continue;
    }
    index += 1;
  }
  return masked.join("");
}

function parseServedSourceParts(source) {
  const maskedSource = maskInactiveJavaScript(source);
  const declarations = [...maskedSource.matchAll(
    /\bconst\s+servedSourceParts\s*=\s*\[([\s\S]*?)\]\s*;/g
  )];
  if (declarations.length === 0) {
    throw configError("served_source_parts=empty");
  }
  if (declarations.length > 1) {
    throw configError("served_source_parts=invalid");
  }

  const declaration = declarations[0];
  const bodyStart = declaration.index + declaration[0].indexOf("[") + 1;
  const body = source.slice(bodyStart, bodyStart + declaration[1].length);
  const literal = /(["'])([^"']*)\1/g;
  const parts = [];
  let previousEnd = 0;
  for (const match of body.matchAll(literal)) {
    const separator = body.slice(previousEnd, match.index);
    const validSeparator = parts.length === 0 ? /^\s*$/.test(separator) : /^\s*,\s*$/.test(separator);
    const validSourcePart = /^src\/served\/[A-Za-z0-9._-]+\.ts$/.test(match[2])
      || match[2] === "src/served-app.ts";
    if (!validSeparator || match[1] !== '"' || !validSourcePart) {
      throw configError("served_source_parts=invalid");
    }
    parts.push(match[2]);
    previousEnd = match.index + match[0].length;
  }
  if (parts.length === 0) {
    if (/^\s*$/.test(body)) {
      throw configError("served_source_parts=empty");
    }
    throw configError("served_source_parts=invalid");
  }
  if (!/^\s*,?\s*$/.test(body.slice(previousEnd))) {
    throw configError("served_source_parts=invalid");
  }

  const seen = new Set();
  for (const part of parts) {
    if (seen.has(part)) {
      throw configError(`served_source_parts=duplicate:${part}`);
    }
    seen.add(part);
  }
  const servedAppIndex = parts.indexOf("src/served-app.ts");
  if (parts.length < 2 || servedAppIndex !== parts.length - 1) {
    throw configError("served_source_parts=invalid");
  }
  return parts;
}

export function createWebContractContext({
  repoRoot,
  textOverrides = new Map(),
  missingPaths = new Set(),
  processRunner = undefined,
  ownerProcessRuntime = DEFAULT_OWNER_PROCESS_RUNTIME
}) {
  const realRepoRoot = resolveRepositoryRoot(repoRoot);
  const webRoot = resolveWebRoot(realRepoRoot);
  let resolvedProcessRunner = processRunner;
  if (resolvedProcessRunner === undefined) {
    const runtime = normalizeOwnerProcessRuntime(ownerProcessRuntime);
    resolvedProcessRunner = (request) => spawnOwnerProcess(request, runtime);
  }
  if (typeof resolvedProcessRunner !== "function") {
    throw configError("process_runner=invalid");
  }
  const overrides = copyNormalizedOverrides(textOverrides);
  const projectedMissingPaths = copyNormalizedMissingPaths(missingPaths);
  const textCache = new Map();
  const jsonCache = new Map();
  const ownerCache = new Map();
  const state = {
    repoRoot: realRepoRoot,
    webRoot,
    overrides,
    missingPaths: projectedMissingPaths,
    textCache,
    jsonCache,
    ownerCache,
    processRunner: resolvedProcessRunner
  };

  async function executeOwner(ownerId) {
    const commands = OWNER_COMMANDS[ownerId];
    for (const commandArguments of commands) {
      validateOwnerTarget(state.webRoot, commandArguments);
    }
    for (const commandArguments of commands) {
      let result;
      try {
        result = await state.processRunner({
          fileName: process.execPath,
          arguments: [...commandArguments],
          cwd: state.webRoot,
          shell: false,
          windowsHide: true,
          timeoutMs: OWNER_TIMEOUT_MS,
          stdin: "ignore"
        });
      } catch (error) {
        throw ownerFailure(ownerId, undefined, state.repoRoot, error);
      }
      const succeeded = result?.exitCode === 0
        && (result.signal === null || result.signal === undefined)
        && result.timedOut === false;
      if (!succeeded) {
        throw ownerFailure(ownerId, result, state.repoRoot);
      }
    }
  }

  function cachedOwner(ownerId) {
    if (!state.ownerCache.has(ownerId)) {
      const promise = Promise.resolve().then(() => executeOwner(ownerId));
      state.ownerCache.set(ownerId, promise);
    }
    return state.ownerCache.get(ownerId);
  }

  function buildContext(contractId = undefined) {
    function repoPath(relativePath) {
      const location = createLocation(state.repoRoot, relativePath);
      verifyExistingContainment(state.repoRoot, location);
      return location.resolved;
    }

    function missingError(normalized) {
      return new WebContractError(WEB_CONTRACT_ERROR_CODES.fileMissing, `path=${normalized}`);
    }

    function readText(relativePath) {
      const location = createLocation(state.repoRoot, relativePath);
      if (state.missingPaths.has(location.normalized)) {
        throw missingError(location.normalized);
      }
      verifyExistingContainment(state.repoRoot, location);
      if (state.textCache.has(location.normalized)) {
        return state.textCache.get(location.normalized);
      }
      if (state.overrides.has(location.normalized)) {
        const override = state.overrides.get(location.normalized);
        state.textCache.set(location.normalized, override);
        return override;
      }

      let source;
      try {
        source = fs.readFileSync(location.resolved, "utf8");
      } catch (error) {
        if (error?.code === "ENOENT" || error?.code === "ENOTDIR") {
          throw missingError(location.normalized);
        }
        throw configError(`file=unreadable:${location.normalized}`, sanitizedFilesystemCause(error));
      }
      state.textCache.set(location.normalized, source);
      return source;
    }

    function readJson(relativePath) {
      const normalized = normalizeRelative(relativePath);
      const location = createLocation(state.repoRoot, normalized);
      verifyExistingContainment(state.repoRoot, location);
      if (state.jsonCache.has(normalized)) {
        return state.jsonCache.get(normalized);
      }
      const source = readText(normalized);
      let parsed;
      try {
        parsed = JSON.parse(source);
      } catch {
        throw configError(`json=invalid:${normalized}`, sanitizedJsonCause());
      }
      state.jsonCache.set(normalized, parsed);
      return parsed;
    }

    function readCombined(relativePaths) {
      if (!Array.isArray(relativePaths)) {
        throw configError("paths=invalid");
      }
      return relativePaths.map((relativePath) => readText(relativePath)).join("\n");
    }

    function readServedSource() {
      const buildSource = readText("web/scripts/build-served-asset.mjs");
      const parts = parseServedSourceParts(buildSource);
      return readCombined(parts.map((part) => `web/${part}`));
    }

    function assertionFailure(label) {
      const detail = typeof label === "string" && label ? label : "invalid-label";
      const error = new WebContractError(
        WEB_CONTRACT_ERROR_CODES.assertionFailed,
        `assertion=${detail}`
      );
      if (contractId !== undefined) {
        error.contract_id = contractId;
      }
      throw error;
    }

    function assertExists(relativePath, label) {
      const location = createLocation(state.repoRoot, relativePath);
      if (state.missingPaths.has(location.normalized)) {
        assertionFailure(label);
      }
      const exists = verifyExistingContainment(state.repoRoot, location);
      if (!exists && state.overrides.has(location.normalized)) {
        return;
      }
      if (!exists) {
        assertionFailure(label);
      }
      if (state.textCache.has(location.normalized)) {
        return;
      }
      try {
        if (!fs.statSync(location.resolved).isFile()) {
          assertionFailure(label);
        }
      } catch (error) {
        if (error instanceof WebContractError) {
          throw error;
        }
        assertionFailure(label);
      }
    }

    function assertMatch(value, pattern, label) {
      if (!(pattern instanceof RegExp)) {
        throw configError("pattern=invalid");
      }
      if (!new RegExp(pattern.source, pattern.flags).test(String(value))) {
        assertionFailure(label);
      }
    }

    function assertNotMatch(value, pattern, label) {
      if (!(pattern instanceof RegExp)) {
        throw configError("pattern=invalid");
      }
      if (new RegExp(pattern.source, pattern.flags).test(String(value))) {
        assertionFailure(label);
      }
    }

    function assertEqual(actual, expected, label) {
      if (!Object.is(actual, expected)) {
        assertionFailure(label);
      }
    }

    function assertIncludes(values, expected, label) {
      if (typeof values?.includes !== "function" || !values.includes(expected)) {
        assertionFailure(label);
      }
    }

    function assertBefore(value, first, second, label) {
      const source = String(value);
      const firstIndex = source.indexOf(String(first));
      const secondIndex = source.indexOf(String(second));
      if (firstIndex < 0 || secondIndex < 0 || firstIndex >= secondIndex) {
        assertionFailure(label);
      }
    }

    function forContract(nextContractId) {
      return buildContext(validateContractId(nextContractId));
    }

    async function runOwners(ownerIds) {
      try {
        const validatedOwnerIds = validateOwnerIds(ownerIds);
        await Promise.all(validatedOwnerIds.map((ownerId) => cachedOwner(ownerId)));
      } catch (error) {
        if (error instanceof WebContractError) {
          throw projectOwnerError(error, contractId);
        }
        throw projectOwnerError(
          ownerFailure("unknown", undefined, state.repoRoot, error),
          contractId
        );
      }
    }

    return Object.freeze({
      repoPath,
      forContract,
      readText,
      readJson,
      readCombined,
      readServedSource,
      assertExists,
      assertMatch,
      assertNotMatch,
      assertEqual,
      assertIncludes,
      assertBefore,
      runOwners
    });
  }

  return buildContext();
}

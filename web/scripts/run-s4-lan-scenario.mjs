// S4 scenario (ADR-0017, design D5): read /pcv-config.js from the Web origin, log in with an account,
// then run the S1, S2, and S3 API steps with that JWT. Plan-only is the default and sends no request.
// --execute is the only mode that calls the host. The password comes from PCV_S4_PASSWORD.
// Access tokens, refresh tokens, and the password are kept in memory and never written.
import { createHash } from "node:crypto";
import { existsSync, mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { inflateSync } from "node:zlib";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const valueNames = new Set(["web", "user", "iso", "vm-root", "out", "phases", "clone-bound-ms", "interval", "retention"]);
const booleanNames = new Set(["execute", "skip-auto"]);
const forbiddenApiHosts = new Set(["+", "*", "0.0.0.0"]);
const loopbackHosts = new Set(["127.0.0.1", "localhost", "::1"]);
const refusedLoopbackCodes = new Set(["PCV_LOOPBACK_SESSION_NOT_LOOPBACK", "PCV_LOOPBACK_SESSION_DISABLED"]);
const vmNames = {
  console: "pcv-it-s4-console",
  source: "pcv-it-s4-source",
  clone: "pcv-it-s4-clone",
  checkpoint: "pcv-it-s4-checkpoint"
};
const checkpointName = "s4-cp1";

function argumentError(kind, name) {
  process.stderr.write(`argument error: ${kind} --${name}\n`);
  process.exit(2);
}

function formatDay(date) {
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}${month}${day}`;
}

function parseArgs(argv) {
  const values = {};
  const seen = new Set();
  for (let index = 0; index < argv.length; index += 1) {
    const token = argv[index];
    if (token === "--password" || token.startsWith("--password=")) argumentError("rejected", "password");
    if (!token.startsWith("--") || token === "--") argumentError("unknown", token.replace(/^-+/, "") || "argument");
    const equals = token.indexOf("=");
    const hasInline = equals !== -1;
    const name = hasInline ? token.slice(2, equals) : token.slice(2);
    if (!name || (!valueNames.has(name) && !booleanNames.has(name))) argumentError("unknown", name || "argument");
    if (seen.has(name)) argumentError("duplicate", name);
    seen.add(name);
    if (booleanNames.has(name)) {
      if (hasInline) argumentError("unexpected value", name);
      values[name] = true;
      continue;
    }
    let raw = hasInline ? token.slice(equals + 1) : "";
    if (!hasInline) {
      const next = argv[index + 1];
      if (next === undefined || next.startsWith("--")) argumentError("missing value", name);
      raw = next;
      index += 1;
    }
    if (raw === "") argumentError("missing value", name);
    values[name] = raw;
  }
  return values;
}

function readBoundedInt(name, raw, minimum, maximum) {
  if (!/^\d+$/.test(raw)) argumentError("invalid", name);
  const value = Number(raw);
  if (!Number.isSafeInteger(value) || value < minimum || value > maximum) argumentError("invalid", name);
  return value;
}

function hostnameOf(value) {
  const match = /^[a-z][a-z0-9+.-]*:\/\/([^/?#]+)/i.exec(value);
  if (!match) return "";
  let authority = match[1];
  const at = authority.lastIndexOf("@");
  if (at !== -1) authority = authority.slice(at + 1);
  if (authority.startsWith("[")) {
    const end = authority.indexOf("]");
    return end === -1 ? "" : authority.slice(1, end);
  }
  const colon = authority.lastIndexOf(":");
  if (colon !== -1 && /^\d+$/.test(authority.slice(colon + 1))) return authority.slice(0, colon);
  return authority;
}

function scrubPaths(text) {
  return String(text)
    .replace(/\b[A-Za-z]:[\\/]+Users[\\/]+\S+/g, "<redacted-path>")
    .replace(/(^|[\s"'=])\/Users\/\S+/g, "$1<redacted-path>");
}

function displayPath(target) {
  const rel = relative(repoRoot, resolve(target)).split("\\").join("/");
  if (rel === "" || rel === ".." || rel.startsWith("../")) return "<path-outside-repo>";
  return rel;
}

function parsePhases(raw) {
  if (raw === "none") return [];
  const parts = raw.split(",").map((part) => part.trim()).filter(Boolean);
  if (parts.length === 0 || parts.includes("none")) argumentError("invalid", "phases");
  const seen = new Set();
  for (const part of parts) {
    if (part !== "s1" && part !== "s2" && part !== "s3") argumentError("invalid", "phases");
    if (seen.has(part)) argumentError("duplicate", "phases");
    seen.add(part);
  }
  return ["s1", "s2", "s3"].filter((phase) => seen.has(phase));
}

function namesForPhases(phases) {
  const names = [];
  if (phases.includes("s1")) names.push(vmNames.console);
  if (phases.includes("s2")) names.push(vmNames.source, vmNames.clone);
  if (phases.includes("s3")) names.push(vmNames.checkpoint);
  return names;
}

function planLines(config) {
  const api = "<apiBaseUrl>";
  const isoText = config.iso ? scrubPaths(config.iso) : "<--iso required for --execute>";
  const vmRootText = displayPath(config.vmRoot);
  const lines = [
    ["preflight.web-root", `GET ${config.web}/ expect 200 text/html`],
    ["preflight.web-config", `GET ${config.web}/pcv-config.js expect apiBaseUrl host = web host`],
    ["preflight.loopback-session-refused", `POST ${api}/api/v1/auth/loopback-session expect not 200 and code PCV_LOOPBACK_SESSION_NOT_LOOPBACK or PCV_LOOPBACK_SESSION_DISABLED; discard token`],
    ["preflight.login", `POST ${api}/api/v1/auth/login expect 200 data.access_token`],
    ["preflight.auth-check", `GET ${api}/api/v1/vms expect 200`]
  ];
  if (config.phases.includes("s1")) {
    lines.push(
      ["s1.create", `POST ${api}/api/v1/vms name=${vmNames.console} generation=2 cpu=2 memory_mb=2048 disk_gb=32 iso=${isoText} vm_root=${vmRootText}; fail vm-already-present if the name exists`],
      ["s1.start", `POST ${api}/api/v1/vms/${vmNames.console}/start`],
      ["s1.screen-before", `GET ${api}/api/v1/vms/${vmNames.console}/console/frame/640x480 until the screen is not blank`],
      ["s1.input", `POST ${api}/api/v1/vms/${vmNames.console}/console/input kind=key action=type key_code=13`],
      ["s1.screen-after", `GET ${api}/api/v1/vms/${vmNames.console}/console/frame/640x480 until the frame hash changes`],
      ["s1.poweroff", `POST ${api}/api/v1/vms/${vmNames.console}/poweroff`],
      ["s1.delete", `DELETE ${api}/api/v1/vms/${vmNames.console}`]
    );
  }
  if (config.phases.includes("s2")) {
    lines.push(
      ["s2.create", `POST ${api}/api/v1/vms name=${vmNames.source} generation=2 cpu=1 memory_mb=512 disk_gb=8 iso=${isoText} vm_root=${vmRootText}; fail vm-already-present if the name exists`],
      ["s2.template-lock", `POST ${api}/api/v1/vms/${vmNames.source}/template-lock locked=true confirm_name=${vmNames.source}`],
      ["s2.clone-preview", `POST ${api}/api/v1/vms/${vmNames.source}/clone/preview name=${vmNames.clone} confirm_name=${vmNames.source} vm_root=${vmRootText}`],
      ["s2.clone", `POST ${api}/api/v1/vms/${vmNames.source}/clone name=${vmNames.clone} confirm_name=${vmNames.source} vm_root=${vmRootText}`],
      ["s2.bound", `wait until the clone job succeeds and elapsed_ms <= ${config.cloneBoundMs}`]
    );
  }
  if (config.phases.includes("s3")) {
    const autoBoundMs = (config.intervalMinutes * 60000) + 120000;
    lines.push(
      ["s3.create", `POST ${api}/api/v1/vms name=${vmNames.checkpoint} generation=2 cpu=1 memory_mb=512 disk_gb=8 iso=${isoText} vm_root=${vmRootText}; fail vm-already-present if the name exists`],
      ["s3.checkpoint-create", `POST ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints name=${checkpointName}`],
      ["s3.checkpoint-list", `GET ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints expect ${checkpointName}`],
      ["s3.checkpoint-restore", `POST ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints/${checkpointName}/restore`],
      ["s3.schedule-preview", `POST ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints/schedule/preview interval_minutes=${config.intervalMinutes} retention_max=${config.retentionMax}`],
      ["s3.schedule-set", `POST ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints/schedule interval_minutes=${config.intervalMinutes} retention_max=${config.retentionMax}`],
      ["s3.auto-checkpoint", `wait until the worker enqueues the first scheduled checkpoint (count >= 2) within ${autoBoundMs} ms; --skip-auto records pass-without-auto and does not wait`],
      ["s3.schedule-clear", `POST ${api}/api/v1/vms/${vmNames.checkpoint}/checkpoints/schedule/clear`]
    );
  }
  const cleanupNames = namesForPhases(config.phases);
  lines.push(["cleanup", `clear armed schedules; delete ${vmNames.clone} after a clone; template-lock locked=false then delete ${vmNames.source}; delete VMs this run created (${cleanupNames.join(", ") || "none"})`]);
  return lines;
}

function renderPlan(config) {
  const header = [
    "S4 LAN scenario plan-only (no request sent).",
    `web=${config.web}`,
    `out=${displayPath(config.outDir)}`,
    `vm-root=${displayPath(config.vmRoot)}`,
    `iso=${config.iso ? scrubPaths(config.iso) : "<--iso required for --execute>"}`,
    `phases=${config.phases.length ? config.phases.join(",") : "none"}`,
    `clone-bound-ms=${config.cloneBoundMs}`,
    `interval=${config.intervalMinutes}`,
    `retention=${config.retentionMax}`,
    `skip-auto=${config.skipAuto ? "on" : "off"}`
  ];
  const steps = planLines(config).map(([name, detail], index) => `${index + 1}.${name}: ${detail}`);
  return scrubPaths(`${header.concat(steps).join("\n")}\n`);
}

function loadConfig(argv) {
  const args = parseArgs(argv);
  const day = formatDay(new Date());
  const webRaw = args.web || "http://127.0.0.1";
  if (!hostnameOf(webRaw)) argumentError("invalid", "web");
  const outDir = resolve(args.out || join(repoRoot, "artifacts", `s4-lan-${day}`));
  const vmRoot = resolve(args["vm-root"] || join(repoRoot, "artifacts", `s4-lan-${day}`, "vms"));
  return {
    execute: args.execute === true,
    skipAuto: args["skip-auto"] === true,
    web: webRaw.replace(/\/+$/, ""),
    webHost: hostnameOf(webRaw),
    user: args.user || "",
    iso: args.iso || "",
    phases: parsePhases(args.phases || "s1,s2,s3"),
    cloneBoundMs: readBoundedInt("clone-bound-ms", args["clone-bound-ms"] || "60000", 1, Number.MAX_SAFE_INTEGER),
    intervalMinutes: readBoundedInt("interval", args.interval || "60", 60, 10080),
    retentionMax: readBoundedInt("retention", args.retention || "2", 1, 32),
    outDir,
    vmRoot
  };
}

function delay(ms) {
  return new Promise((done) => setTimeout(done, ms));
}

function rowsOf(payload) {
  const data = payload?.data;
  if (Array.isArray(data)) return data;
  if (Array.isArray(data?.vms)) return data.vms;
  if (Array.isArray(data?.checkpoints)) return data.checkpoints;
  return [];
}

function readApiBase(text) {
  const match = /apiBaseUrl"\s*:\s*"([^"]*)"/.exec(text);
  return match ? match[1] : "";
}

const config = loadConfig(process.argv.slice(2));
if (!config.execute) {
  process.stdout.write(renderPlan(config));
  process.exit(0);
}

if (!config.user) argumentError("missing value", "user");
const password = process.env.PCV_S4_PASSWORD || "";
if (!password) argumentError("missing value", "PCV_S4_PASSWORD");
if (config.phases.length > 0 && !config.iso) argumentError("missing value", "iso");
if (existsSync(join(config.outDir, "summary.json"))) argumentError("summary already exists", "out");

const secrets = new Set([password]);
const createdVms = [];
const cleanupErrors = [];
let token = "";
let apiBase = "";
let scheduleArmed = false;
let sourceLocked = false;
const hostKind = loopbackHosts.has(config.webHost.toLowerCase()) ? "loopback" : "lan";

function scrub(text) {
  let value = String(text ?? "");
  for (const secret of secrets) {
    if (secret) value = value.split(secret).join("<redacted>");
  }
  if (hostKind === "lan" && config.webHost) value = value.split(config.webHost).join("<host-lan-ip>");
  return scrubPaths(value);
}

function publicUrl(url) {
  if (!url) return url;
  return hostKind === "lan" ? scrub(url) : url;
}

function scrubValue(value) {
  if (typeof value === "string") return scrub(value);
  if (Array.isArray(value)) return value.map((item) => scrubValue(item));
  if (value && typeof value === "object") {
    const copy = {};
    for (const [key, item] of Object.entries(value)) copy[key] = scrubValue(item);
    return copy;
  }
  return value;
}

function collectSecrets(value) {
  if (!value || typeof value !== "object") return;
  for (const [key, item] of Object.entries(value)) {
    if ((key === "access_token" || key === "refresh_token" || key === "password") && typeof item === "string" && item) {
      secrets.add(item);
    } else if (item && typeof item === "object") collectSecrets(item);
  }
}

const summary = {
  contract: "pcv-s4-lan-scenario-v1",
  host_kind: hostKind,
  web: publicUrl(config.web),
  api: null,
  user: config.user,
  role: null,
  phases: {
    s1: { result: "skipped", steps: {} },
    s2: { result: "skipped", steps: {} },
    s3: { result: "skipped", steps: {} }
  },
  preflight: {},
  vms_created: [],
  vms_remaining: [],
  cleanup_error: null,
  started_at: new Date().toISOString(),
  finished_at: null,
  result: "fail"
};

function blankPhase(name) {
  return summary.phases[name];
}

async function webGet(path) {
  let response;
  try {
    response = await fetch(`${config.web}${path}`);
  } catch {
    throw Object.assign(new Error(`GET ${path} failed`), { step: path });
  }
  const text = await response.text();
  return { status: response.status, contentType: response.headers.get("content-type") || "", text };
}

async function api(method, path, body) {
  const headers = {};
  if (body !== undefined) headers["content-type"] = "application/json";
  if (token) headers.authorization = `Bearer ${token}`;
  let response;
  try {
    response = await fetch(`${apiBase}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body)
    });
  } catch {
    throw Object.assign(new Error(`${method} ${path} failed`), { step: path });
  }
  const text = await response.text();
  let payload = {};
  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = {};
    }
  }
  collectSecrets(payload);
  return { status: response.status, payload };
}

async function waitJob(result, label) {
  const jobId = result.payload?.data?.job_id || "";
  const code = result.payload?.error?.code || "";
  if (!jobId) throw Object.assign(new Error(`${label}: no job id (status ${result.status} ${code})`), { step: label, code });
  for (let attempt = 0; attempt < 120; attempt += 1) {
    const job = await api("GET", `/api/v1/jobs/${encodeURIComponent(jobId)}`);
    const status = job.payload?.data?.status || "";
    if (status && !["queued", "running", "pending", "accepted"].includes(status)) {
      if (status !== "succeeded") {
        const jobCode = job.payload?.data?.error?.code || job.payload?.error?.code || "";
        throw Object.assign(new Error(`${label}: job ${status} ${jobCode}`), { step: label, code: jobCode, status });
      }
      return { status, job_id: jobId };
    }
    await delay(1000);
  }
  throw Object.assign(new Error(`${label}: job timed out`), { step: label });
}

async function listVmNames() {
  const listed = await api("GET", "/api/v1/vms");
  if (listed.status !== 200) throw Object.assign(new Error(`vm-list status ${listed.status}`), { step: "vm-list", code: listed.payload?.error?.code || "" });
  return rowsOf(listed.payload).map((row) => String(row.name || row.id || "")).filter(Boolean);
}

async function createVm(name, spec) {
  const existing = await listVmNames();
  if (existing.includes(name)) throw Object.assign(new Error("vm-already-present"), { step: "create", code: "vm-already-present" });
  const created = await api("POST", "/api/v1/vms", {
    name,
    iso_path: resolve(config.iso),
    vm_root: resolve(config.vmRoot),
    generation: 2,
    ...spec
  });
  const job = await waitJob(created, "create");
  createdVms.push(name);
  return job;
}

async function deleteVm(name) {
  const existing = await listVmNames();
  if (!existing.includes(name)) return { status: "absent" };
  const deleted = await api("DELETE", `/api/v1/vms/${encodeURIComponent(name)}`);
  return waitJob(deleted, "delete");
}

async function readScreen(vm, label, previousHash) {
  for (let attempt = 0; attempt < 120; attempt += 1) {
    const frame = await api("GET", `/api/v1/vms/${encodeURIComponent(vm)}/console/frame/640x480`);
    const data = frame.payload?.data;
    if (frame.status === 200 && data?.frame_base64) {
      try {
        const raw = inflateSync(Buffer.from(data.frame_base64, "base64"));
        const hash = createHash("sha256").update(raw).digest("hex");
        const blank = raw.length === 0 || raw.every((byte) => byte === 0);
        if (!blank && hash !== previousHash) return { sha256: hash, attempts: attempt + 1 };
      } catch {
        // The next attempt replaces an incomplete frame.
      }
    }
    await delay(1000);
  }
  throw Object.assign(new Error(`${label}: screen`), { step: label });
}

async function checkpointNames(vm) {
  const listed = await api("GET", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints`);
  if (listed.status !== 200) {
    throw Object.assign(new Error(`checkpoint-list status ${listed.status}`), { step: "checkpoint-list", code: listed.payload?.error?.code || "" });
  }
  return rowsOf(listed.payload).map((row) => String(row.name || row.id || ""));
}

function failPhase(phase, error) {
  const step = error.step || "phase";
  const current = phase.steps[step] || { ok: false };
  phase.steps[step] = { ...current, ok: false, code: error.code || current.code || "fail" };
  phase.result = "fail";
}

async function preflight() {
  const root = await webGet("/");
  const rootOk = root.status === 200 && root.contentType.toLowerCase().includes("text/html");
  summary.preflight["web-root"] = { ok: rootOk, status: root.status };
  if (!rootOk) throw Object.assign(new Error("preflight.web-root"), { step: "web-root" });

  const configResponse = await webGet("/pcv-config.js");
  const apiHost = hostnameOf(readApiBase(configResponse.text));
  const hostOk = configResponse.status === 200
    && apiHost
    && apiHost.toLowerCase() === config.webHost.toLowerCase()
    && !forbiddenApiHosts.has(apiHost);
  summary.preflight["web-config"] = { ok: hostOk, status: configResponse.status, reason: hostOk ? "ok" : "api-base-host" };
  if (!hostOk) throw Object.assign(new Error("preflight.web-config"), { step: "web-config" });
  apiBase = readApiBase(configResponse.text).replace(/\/+$/, "");
  summary.api = publicUrl(apiBase);

  const loopback = await api("POST", "/api/v1/auth/loopback-session", {});
  const loopbackCode = loopback.payload?.error?.code || "";
  const refused = loopback.status !== 200 && refusedLoopbackCodes.has(loopbackCode);
  summary.preflight["loopback-session-refused"] = { ok: refused, status: loopback.status, code: loopbackCode };
  if (!refused) throw Object.assign(new Error("preflight.loopback-session-refused"), { step: "loopback-session-refused", code: loopbackCode });

  const login = await api("POST", "/api/v1/auth/login", { username: config.user, password });
  const access = typeof login.payload?.data?.access_token === "string" ? login.payload.data.access_token : "";
  const role = typeof login.payload?.data?.session?.role === "string" ? login.payload.data.session.role : "";
  if (access) secrets.add(access);
  const loginOk = login.status === 200 && access.length > 0;
  summary.preflight.login = { ok: loginOk, status: login.status, code: login.payload?.error?.code || "", role: loginOk ? role : null };
  if (!loginOk) throw Object.assign(new Error("preflight.login"), { step: "login", code: login.payload?.error?.code || "" });
  token = access;
  summary.role = role;

  const check = await api("GET", "/api/v1/vms");
  const checkOk = check.status === 200;
  summary.preflight["auth-check"] = { ok: checkOk, status: check.status, code: check.payload?.error?.code || "" };
  if (!checkOk) throw Object.assign(new Error("preflight.auth-check"), { step: "auth-check" });
}

async function runS1() {
  const phase = blankPhase("s1");
  const vm = vmNames.console;
  try {
    const created = await createVm(vm, { cpu: 2, memory_mb: 2048, disk_gb: 32 });
    phase.steps.create = { ok: true, status: created.status };
    phase.steps.start = { ok: true, status: (await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/start`, {}), "start")).status };
    const before = await readScreen(vm, "screen-before", "");
    phase.steps["screen-before"] = { ok: true, sha256: before.sha256, attempts: before.attempts };
    const sent = await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/console/input`, { kind: "key", action: "type", key_code: 13 });
    phase.steps.input = { ok: sent.status === 200, status: sent.status, code: sent.payload?.error?.code || "" };
    if (sent.status !== 200) throw Object.assign(new Error("input"), { step: "input", code: sent.payload?.error?.code || "" });
    const after = await readScreen(vm, "screen-after", before.sha256);
    phase.steps["screen-after"] = { ok: true, sha256: after.sha256, attempts: after.attempts };
    phase.steps.poweroff = { ok: true, status: (await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/poweroff`, {}), "poweroff")).status };
    const deleted = await deleteVm(vm);
    if (deleted.status === "absent") throw Object.assign(new Error("delete"), { step: "delete", code: "absent" });
    phase.steps.delete = { ok: true, status: deleted.status };
    phase.result = "pass";
  } catch (error) {
    failPhase(phase, error);
    throw error;
  }
}

async function runS2() {
  const phase = blankPhase("s2");
  try {
    const created = await createVm(vmNames.source, { cpu: 1, memory_mb: 512, disk_gb: 8 });
    phase.steps.create = { ok: true, status: created.status };
    const locked = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vmNames.source)}/template-lock`, {
      confirm_name: vmNames.source,
      locked: true
    }), "template-lock");
    sourceLocked = true;
    phase.steps["template-lock"] = { ok: true, status: locked.status };
    const preview = await api("POST", `/api/v1/vms/${encodeURIComponent(vmNames.source)}/clone/preview`, {
      confirm_name: vmNames.source,
      name: vmNames.clone,
      vm_root: resolve(config.vmRoot)
    });
    phase.steps["clone-preview"] = { ok: preview.status === 200, status: preview.status, code: preview.payload?.error?.code || "" };
    if (preview.status !== 200) throw Object.assign(new Error("clone-preview"), { step: "clone-preview", code: preview.payload?.error?.code || "" });
    const started = Date.now();
    createdVms.push(vmNames.clone);
    const cloned = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vmNames.source)}/clone`, {
      confirm_name: vmNames.source,
      name: vmNames.clone,
      vm_root: resolve(config.vmRoot)
    }), "clone");
    const elapsed = Date.now() - started;
    phase.steps.clone = { ok: true, status: cloned.status, elapsed_ms: elapsed };
    const within = elapsed <= config.cloneBoundMs;
    phase.steps.bound = { ok: within, elapsed_ms: elapsed, bound_ms: config.cloneBoundMs };
    if (!within) throw Object.assign(new Error("clone-bound"), { step: "bound" });
    phase.result = "pass";
  } catch (error) {
    failPhase(phase, error);
    throw error;
  }
}

async function runS3() {
  const phase = blankPhase("s3");
  const vm = vmNames.checkpoint;
  try {
    const created = await createVm(vm, { cpu: 1, memory_mb: 512, disk_gb: 8 });
    phase.steps.create = { ok: true, status: created.status };
    const checkpoint = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints`, { name: checkpointName }), "checkpoint-create");
    phase.steps["checkpoint-create"] = { ok: true, status: checkpoint.status };
    const names = await checkpointNames(vm);
    const listed = names.includes(checkpointName);
    phase.steps["checkpoint-list"] = { ok: listed, count: names.length };
    if (!listed) throw Object.assign(new Error("checkpoint-list"), { step: "checkpoint-list" });
    const restored = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/${encodeURIComponent(checkpointName)}/restore`, {}), "checkpoint-restore");
    phase.steps["checkpoint-restore"] = { ok: true, status: restored.status };
    const preview = await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule/preview`, {
      interval_minutes: config.intervalMinutes,
      retention_max: config.retentionMax
    });
    phase.steps["schedule-preview"] = { ok: preview.status === 200, status: preview.status, code: preview.payload?.error?.code || "" };
    if (preview.status !== 200) throw Object.assign(new Error("schedule-preview"), { step: "schedule-preview", code: preview.payload?.error?.code || "" });
    const scheduled = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule`, {
      interval_minutes: config.intervalMinutes,
      retention_max: config.retentionMax
    }), "schedule-set");
    scheduleArmed = true;
    phase.steps["schedule-set"] = { ok: true, status: scheduled.status };
    if (config.skipAuto) {
      phase.steps["auto-checkpoint"] = { ok: true, status: "skipped" };
      phase.result = "pass-without-auto";
    } else {
      const autoBoundMs = (config.intervalMinutes * 60000) + 120000;
      const started = Date.now();
      let count = names.length;
      let seen = false;
      while (Date.now() - started < autoBoundMs) {
        const nowNames = await checkpointNames(vm);
        count = nowNames.length;
        if (count >= 2) {
          phase.steps["auto-checkpoint"] = { ok: true, count, elapsed_ms: Date.now() - started };
          seen = true;
          break;
        }
        await delay(2000);
      }
      if (!seen) {
        phase.steps["auto-checkpoint"] = { ok: false, count };
        throw Object.assign(new Error("auto-checkpoint"), { step: "auto-checkpoint" });
      }
      phase.result = "pass";
    }
    const cleared = await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule/clear`), "schedule-clear");
    scheduleArmed = false;
    phase.steps["schedule-clear"] = { ok: true, status: cleared.status };
  } catch (error) {
    failPhase(phase, error);
    throw error;
  }
}

async function cleanup() {
  if (!token) return;
  if (scheduleArmed) {
    try {
      await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vmNames.checkpoint)}/checkpoints/schedule/clear`), "schedule-clear");
      scheduleArmed = false;
    } catch (error) {
      cleanupErrors.push(scrub(error.message));
    }
  }
  if (createdVms.includes(vmNames.clone)) {
    try {
      await deleteVm(vmNames.clone);
    } catch (error) {
      cleanupErrors.push(scrub(error.message));
    }
  }
  if (createdVms.includes(vmNames.source)) {
    let unlockFailed = false;
    if (sourceLocked) {
      try {
        await waitJob(await api("POST", `/api/v1/vms/${encodeURIComponent(vmNames.source)}/template-lock`, {
          confirm_name: vmNames.source,
          locked: false
        }), "template-unlock");
        sourceLocked = false;
      } catch (error) {
        unlockFailed = true;
        cleanupErrors.push(scrub(error.message));
      }
    }
    if (!unlockFailed) {
      try {
        await deleteVm(vmNames.source);
      } catch (error) {
        cleanupErrors.push(scrub(error.message));
      }
    }
  }
  for (const name of createdVms) {
    if (name === vmNames.clone || name === vmNames.source) continue;
    try {
      await deleteVm(name);
    } catch (error) {
      cleanupErrors.push(scrub(error.message));
    }
  }
  try {
    const live = new Set(await listVmNames());
    summary.vms_remaining = createdVms.filter((name) => live.has(name));
  } catch (error) {
    cleanupErrors.push(scrub(error.message));
    summary.vms_remaining = [...createdVms];
  }
}

function finish(exitCode) {
  summary.vms_created = [...createdVms];
  summary.cleanup_error = cleanupErrors.length ? cleanupErrors.join("; ") : null;
  summary.finished_at = new Date().toISOString();
  const okResult = summary.result === "pass" || summary.result === "pass-without-auto";
  const code = okResult && !summary.cleanup_error ? 0 : 1;
  try {
    mkdirSync(config.outDir, { recursive: true });
    writeFileSync(join(config.outDir, "summary.json"), `${JSON.stringify(scrubValue(summary), null, 2)}\n`);
  } catch {
    process.stderr.write("summary write failed\n");
    process.stdout.write(scrub(`result=${summary.result} s1=${summary.phases.s1.result} s2=${summary.phases.s2.result} s3=${summary.phases.s3.result}\n`));
    process.exit(1);
  }
  process.stdout.write(scrub(`result=${summary.result} s1=${summary.phases.s1.result} s2=${summary.phases.s2.result} s3=${summary.phases.s3.result}\n`));
  process.exit(exitCode ?? code);
}

const runners = { s1: runS1, s2: runS2, s3: runS3 };

try {
  await preflight();
  if (config.phases.length > 0) {
    const existing = await listVmNames();
    const clash = namesForPhases(config.phases).filter((name) => existing.includes(name));
    if (clash.length > 0) {
      const phase = blankPhase(config.phases[0]);
      phase.steps.create = { ok: false, code: "vm-already-present" };
      phase.result = "fail";
      throw Object.assign(new Error("vm-already-present"), { step: "create", code: "vm-already-present" });
    }
    for (const phase of config.phases) await runners[phase]();
  }
  const selectedPass = config.phases.every((phase) => summary.phases[phase].result === "pass" || summary.phases[phase].result === "pass-without-auto");
  if (!selectedPass) summary.result = "fail";
  else if (config.skipAuto) summary.result = "pass-without-auto";
  else summary.result = "pass";
} catch {
  summary.result = "fail";
} finally {
  try {
    await cleanup();
  } catch (error) {
    cleanupErrors.push(scrub(error.message));
  }
  finish();
}

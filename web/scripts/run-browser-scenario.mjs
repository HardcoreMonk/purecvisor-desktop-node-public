// Browser scenarios S1-S3. The default is plan-only and sends no request.
// --check logs in and looks for screens. --execute drives the same screens.
// Passwords stay in PCV_SCENARIO_PASSWORD and are never printed.
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, isAbsolute, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { launch } from "./browser-scenario-cdp.mjs";
import { displayRepoPath, formatRunDay, readScenarioArgs, usageText } from "./scenario-args.mjs";

const SPEC = {
  values: ["scenario", "web", "username", "iso", "vm", "source", "clone", "checkpoint", "interval", "retention", "cpu", "memory", "disk", "switch", "keys", "profile", "out"],
  booleans: ["execute", "plan-only", "check", "headed", "cleanup", "wait-schedule", "manual-login", "start"]
};
const SCENARIOS = new Set(["s1", "s2", "s3"]);
const SECRET_FIELDS = new Set(["password", "token", "access_token", "refresh_token", "authorization", "pcv_scenario_password"]);
const NUMBER_ARGS = ["cpu", "memory", "disk", "interval", "retention"];
const JOB_TIMEOUT_MS = 10 * 60 * 1000;
const SHELL_TIMEOUT_MS = 25000;
const MANUAL_LOGIN_TIMEOUT_MS = 10 * 60 * 1000;
const CONFIRM_WAIT_MS = 2500;

export const PLAN_STEPS = {
  s1: [
    "login",
    "open-create-wizard",
    "fill-create-fields",
    "submit-create",
    "accept-create-confirm",
    "wait-create-job",
    "connect-switch",
    "start-vm",
    "open-console",
    "wait-console-frame",
    "send-console-keys",
    "observe-console-change"
  ],
  s2: [
    "login",
    "select-source-vm",
    "power-off-source",
    "template-lock",
    "open-clone-form",
    "submit-clone",
    "wait-clone-job",
    "record-clone-seconds",
    "start-clone"
  ],
  s3: [
    "login",
    "select-vm",
    "create-checkpoint",
    "restore-checkpoint",
    "schedule-preview",
    "schedule-save",
    "wait-schedule",
    "schedule-clear"
  ],
  cleanup: [
    "login",
    "select-vm",
    "template-unlock",
    "power-off",
    "delete-vm",
    "confirm-absent"
  ],
  check: [
    "login",
    "version-badge",
    "vm-list",
    "create-wizard-fields",
    "vm-detail-power",
    "console-tab",
    "checkpoint-tab",
    "template-lock-clone"
  ]
};

// 4b calls this before every write. Names outside the integration-test prefix are refused.
export function assertScenarioVmName(name) {
  const value = String(name ?? "");
  if (!value.startsWith("pcv-it-")) throw new Error("scenario vm name must start with pcv-it-");
  return value;
}

// Profile reuse is limited to the repo artifacts tree so a run cannot point Edge at an arbitrary folder.
export function assertScenarioProfile(profile, repoRoot) {
  const root = resolve(repoRoot);
  const target = resolve(root, String(profile || ""));
  const rel = relative(root, target).split("\\").join("/");
  if (!rel || rel === ".." || rel.startsWith("../") || isAbsolute(rel) || !(rel === "artifacts" || rel.startsWith("artifacts/"))) {
    throw new Error("scenario profile must stay under artifacts/");
  }
  return target;
}

export function scenarioMode(flags) {
  if (flags.has("execute")) return "execute";
  if (flags.has("check")) return "check";
  return "plan-only";
}

// Operator accounts can sit on degraded because the account list returns 403.
// auth and error stop the wait. Anything else is not ready yet.
export function judgeShellReady(snapshot) {
  const connection = String(snapshot && snapshot.connectionState || "");
  const loginHidden = !!(snapshot && snapshot.loginHidden === true);
  const appVisible = !!(snapshot && snapshot.appVisible === true);
  const refreshed = !!(snapshot && snapshot.lastRefreshedAt);
  const loading = !!(snapshot && snapshot.loading);
  // Before a login the account-mode shell sits in "auth" with the login page shown; that is waiting, not failure.
  // auth or error only fails once the shell is shown and its refresh has finished.
  const failed = (connection === "auth" || connection === "error") && loginHidden && appVisible && refreshed && !loading;
  const readyConnection = connection === "connected" || connection === "degraded";
  const ok = !failed && loginHidden && appVisible && refreshed && !loading && readyConnection;
  return { ok, failed, connection };
}

// succeeded is the only success. Empty and unknown statuses are failures.
// The job waiter treats a blank status as still in flight until the timeout.
export function classifyJobStatus(status) {
  const value = String(status ?? "");
  if (value === "queued" || value === "running" || value === "pending" || value === "accepted") return "in-flight";
  if (value === "succeeded") return "success";
  return "failed";
}

export function serializeScenarioSummary(summary, secrets = []) {
  const banned = secrets.map((item) => String(item || "")).filter((item) => item.length > 0);
  return JSON.stringify(redactValue(summary, banned));
}

function redactValue(value, banned) {
  if (Array.isArray(value)) return value.map((item) => redactValue(item, banned));
  if (value && typeof value === "object") {
    const out = {};
    for (const [key, item] of Object.entries(value)) {
      if (SECRET_FIELDS.has(String(key).toLowerCase())) continue;
      out[key] = redactValue(item, banned);
    }
    return out;
  }
  if (typeof value === "string") {
    let text = value;
    for (const secret of banned) text = text.split(secret).join("[redacted]");
    return text;
  }
  return value;
}

function redactText(text, secrets) {
  let line = String(text ?? "");
  for (const secret of secrets) {
    if (secret) line = line.split(String(secret)).join("[redacted]");
  }
  return line;
}

function formatRunClock(date = new Date()) {
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");
  return `${hours}${minutes}${seconds}`;
}

function defaultOutDir(repoRoot, scenario, mode, date = new Date()) {
  const folder = mode === "check" ? "check" : scenario;
  return join(repoRoot, "artifacts", `browser-scenario-${formatRunDay(date)}`, `${folder}-${formatRunClock(date)}`);
}

function failUsage(message) {
  process.stderr.write(`${message}\n${usageText("run-browser-scenario.mjs", SPEC)}\n`);
  process.exit(2);
}

function isDirect() {
  const entry = process.argv[1];
  if (!entry) return false;
  return fileURLToPath(import.meta.url).toLowerCase() === resolve(entry).toLowerCase();
}

function sleep(ms) {
  return new Promise((resolveSleep) => setTimeout(resolveSleep, ms));
}

function scenarioSecrets() {
  const password = process.env.PCV_SCENARIO_PASSWORD || "";
  return password ? [password] : [];
}

function requireDigits(args) {
  for (const name of NUMBER_ARGS) {
    const raw = args.values[name];
    if (raw !== undefined && !/^\d+$/.test(String(raw))) failUsage(`argument error: invalid --${name}`);
  }
}

function guardVm(value, flagName) {
  try {
    return assertScenarioVmName(value);
  } catch {
    failUsage(`argument error: invalid --${flagName}`);
  }
  return "";
}

function validateExecute(args, scenario, cleanup) {
  if (cleanup) {
    const names = String(args.values.vm || "").split(",").map((item) => item.trim()).filter((item) => item.length > 0);
    if (names.length === 0) failUsage("argument error: missing value --vm");
    for (const name of names) guardVm(name, "vm");
    return;
  }
  if (scenario === "s1") {
    if (!args.values.iso) failUsage("argument error: missing value --iso");
    if (!args.values.vm) failUsage("argument error: missing value --vm");
    guardVm(args.values.vm, "vm");
  } else if (scenario === "s2") {
    if (!args.values.source) failUsage("argument error: missing value --source");
    if (!args.values.clone) failUsage("argument error: missing value --clone");
    guardVm(args.values.source, "source");
    guardVm(args.values.clone, "clone");
  } else if (scenario === "s3") {
    if (!args.values.vm) failUsage("argument error: missing value --vm");
    guardVm(args.values.vm, "vm");
  }
}

function integerArg(args, name, fallback) {
  const raw = args.values[name];
  return raw === undefined ? fallback : Number(raw);
}

const LOGIN_VISIBLE = `function () {
  const login = document.getElementById("login-page");
  const user = document.getElementById("login-user");
  if (!login || !user) return { ok: true, form: false };
  const display = getComputedStyle(login).display;
  const hidden = login.getAttribute("aria-hidden");
  return { ok: true, form: display !== "none" && hidden !== "true" };
}`;

const READ_SHELL = `function () {
  const login = document.getElementById("login-page");
  const app = document.getElementById("app");
  const display = login ? getComputedStyle(login).display : "none";
  const model = window.state || {};
  return {
    connectionState: model.connectionState || "",
    loading: !!model.loading,
    lastRefreshedAt: model.lastRefreshedAt || "",
    loginHidden: !login || display === "none",
    appVisible: !!(app && app.getAttribute("aria-hidden") !== "true")
  };
}`;

const VERSION_TEXT = `function () {
  const node = document.getElementById("version-badge");
  const text = node ? String(node.textContent || "").replace(/\\s+/g, " ").trim() : "";
  const hidden = !node || node.hasAttribute("hidden");
  return { ok: true, found: text.length > 0 && !hidden, text: text };
}`;

const PRESENT = `function (selector) {
  return { ok: true, found: !!document.querySelector(selector) };
}`;

const ENABLED = `function (selector) {
  const node = document.querySelector(selector);
  return { ok: !!(node && !node.disabled && !node.hidden) };
}`;

const READ_JOBS = `function () {
  const jobs = (window.state && Array.isArray(state.trackedJobs)) ? state.trackedJobs : [];
  return {
    ok: true,
    jobs: jobs.map(function (job) {
      return {
        job_id: String(job.job_id || job.id || ""),
        status: String(job.status || ""),
        operation: String(job.operation || "")
      };
    })
  };
}`;

const READ_VM = `function (wanted) {
  const list = (window.state && Array.isArray(state.vms)) ? state.vms : [];
  const vm = list.find(function (item) {
    const label = String(item.name || item.vm_name || item.display_name || "");
    const id = String(item.id || item.vm_id || "");
    return label === wanted || id === wanted;
  }) || null;
  const raw = vm ? String(vm.state || vm.status || "") : "";
  const lower = raw.toLowerCase();
  return {
    ok: true,
    found: !!vm,
    state: raw,
    running: lower.indexOf("running") !== -1,
    off: !!vm && (raw === "" || lower === "off" || lower === "stopped" || lower === "saved"),
    template: !!(vm && vm.template_lock)
  };
}`;

const WIZARD_FIELDS = [
  ["name", "#create-vm-form [name=\"name\"]"],
  ["iso", "#create-vm-form [name=\"iso_path\"]"],
  ["generation", "#create-vm-generation"],
  ["cpu", "#create-vm-form [name=\"cpu\"]"],
  ["memory", "#create-vm-form [name=\"memory_mb\"]"],
  ["disk", "#create-vm-form [name=\"disk_gb\"]"],
  ["switch", "#create-vm-network-notice"]
];

async function probe(browser, selector) {
  const result = await browser.callFunction(PRESENT, selector);
  return !!(result && result.found);
}

async function probeMap(browser, pairs) {
  const detail = {};
  for (const [name, selector] of pairs) detail[name] = await probe(browser, selector);
  return detail;
}

function stepResult(name, started, status, detail, screenshot) {
  return {
    name,
    status,
    ms: Date.now() - started,
    detail,
    screenshot: screenshot || ""
  };
}

async function shot(browser, outDir, repoRoot, fileName) {
  const filePath = join(outDir, fileName);
  await browser.screenshot(filePath);
  return displayRepoPath(repoRoot, filePath);
}

async function waitUntil(browser, fnSource, args, timeoutMs, intervalMs = 150) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    last = await browser.callFunction(fnSource, ...args);
    if (last && last.ok) return last;
    if (last && last.failed) {
      const error = new Error(last.detail || "wait failed");
      error.detail = last;
      throw error;
    }
    await sleep(intervalMs);
  }
  const error = new Error("wait timeout");
  error.detail = last;
  throw error;
}

async function waitForShell(browser, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    const snapshot = await browser.callFunction(READ_SHELL);
    last = judgeShellReady(snapshot || {});
    if (last.failed) throw new Error(`shell connection ${last.connection || "unknown"}`);
    if (last.ok) return last;
    await sleep(200);
  }
  throw new Error(`shell not ready ${last && last.connection ? last.connection : "timeout"}`);
}

// After navigation the page first shows a module splash. Wait until it settles on the login page or a ready shell.
async function waitForLoginOrShell(browser, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const visible = await browser.callFunction(LOGIN_VISIBLE);
    if (visible && visible.form) return visible;
    const snapshot = await browser.callFunction(READ_SHELL);
    if (judgeShellReady(snapshot || {}).ok) return { ok: true, form: false };
    await sleep(200);
  }
  throw new Error("login page or shell did not appear");
}

async function loginSession(browser, { username, manual }) {
  const visible = await waitForLoginOrShell(browser, SHELL_TIMEOUT_MS);
  if (visible && visible.form && !manual) {
    const password = process.env.PCV_SCENARIO_PASSWORD || "";
    if (!username || !password) throw new Error(username ? "password missing" : "username missing");
    await browser.fill("#login-user", username);
    await browser.fill("#login-pass", password);
    await browser.click('#login-cred-actions button.login-submit[type="submit"]');
  }
  await waitForShell(browser, manual ? MANUAL_LOGIN_TIMEOUT_MS : SHELL_TIMEOUT_MS);
  if (visible && visible.form) return manual ? "manual" : "account";
  return "loopback";
}

async function readJobs(browser) {
  const result = await browser.callFunction(READ_JOBS);
  return (result && result.jobs) || [];
}

async function snapshotJobIds(browser) {
  const ids = new Set();
  for (const job of await readJobs(browser)) if (job.job_id) ids.add(job.job_id);
  return ids;
}

function freshJob(jobs, previousIds) {
  return jobs.find((job) => job.job_id && !previousIds.has(job.job_id)) || null;
}

async function waitForNewJob(browser, previousIds, timeoutMs = JOB_TIMEOUT_MS) {
  const deadline = Date.now() + timeoutMs;
  let current = null;
  while (Date.now() < deadline) {
    current = freshJob(await readJobs(browser), previousIds) || current;
    if (current && current.job_id) {
      const kind = current.status ? classifyJobStatus(current.status) : "in-flight";
      if (kind === "success") return { job_id: current.job_id, status: current.status };
      if (kind === "failed") throw new Error(`job ${current.job_id} ${current.status || "failed"}`);
    }
    await sleep(400);
  }
  if (current && current.job_id) throw new Error(`job ${current.job_id} ${current.status || "pending"} timeout`);
  throw new Error("job was not tracked");
}

async function waitForOptionalJob(browser, previousIds, appearMs = 20000) {
  const deadline = Date.now() + appearMs;
  while (Date.now() < deadline) {
    const found = freshJob(await readJobs(browser), previousIds);
    if (found && found.job_id) return waitForNewJob(browser, previousIds, JOB_TIMEOUT_MS);
    await sleep(400);
  }
  return null;
}

async function acceptConfirm(browser, typedName, { required = false } = {}) {
  let appeared = null;
  try {
    appeared = await browser.waitFor(`function () {
      return { ok: !!document.querySelector('[data-pcv-confirm="accept"]') };
    }`, { timeoutMs: CONFIRM_WAIT_MS, intervalMs: 100 });
  } catch {
    appeared = null;
  }
  if (!appeared) {
    if (required) throw new Error("confirm dialog missing");
    return "confirm=absent";
  }
  const typed = await browser.callFunction(`function () {
    return { found: !!document.querySelector(".pcv-confirm-typed") };
  }`);
  if (typed && typed.found) {
    await browser.fill(".pcv-confirm-typed", String(typedName || ""));
    await browser.waitFor(`function () {
      const button = document.querySelector('[data-pcv-confirm="accept"]');
      return { ok: !!(button && !button.disabled) };
    }`, { timeoutMs: 5000 });
  }
  await browser.click('[data-pcv-confirm="accept"]');
  return "confirm=accepted";
}

async function readVm(browser, name) {
  return browser.callFunction(READ_VM, name);
}

async function openVmNav(browser) {
  await browser.click('#shell-sidebar .shell-navitem[data-nav="vms"]');
  await sleep(300);
}

async function selectVm(browser, name) {
  assertScenarioVmName(name);
  await openVmNav(browser);
  const result = await browser.callFunction(`function (wanted) {
    const rows = Array.from(document.querySelectorAll("#vm-table tbody tr[data-vm-id]"));
    const row = rows.find(function (item) {
      const id = item.getAttribute("data-vm-id") || "";
      const nameNode = item.querySelector(".pcv-vm-list-name");
      const label = nameNode ? nameNode.textContent.trim() : "";
      const cell = item.querySelector("td");
      const cellText = cell ? cell.textContent.trim() : "";
      return label === wanted || cellText === wanted || id === wanted;
    });
    if (!row) return { ok: false, reason: "missing" };
    const nameNode = row.querySelector(".pcv-vm-list-name");
    if (nameNode) nameNode.click();
    else row.click();
    return { ok: true, id: row.getAttribute("data-vm-id") || "" };
  }`, name);
  if (!result || result.ok !== true) throw new Error("vm not listed");
  await waitUntil(browser, `function (vmId) {
    const selected = !!(window.state && state.selectedVmId === vmId);
    const panel = document.querySelector("#vm-detail-panel [data-vm-id]");
    return { ok: selected && !!panel && panel.getAttribute("data-vm-id") === vmId };
  }`, [result.id], 20000, 250);
  return result.id || name;
}

// The VM id the Web Console renders on rows, detail buttons and forms (data-vm-id) for a scenario VM name.
async function vmIdFor(browser, name) {
  assertScenarioVmName(name);
  const result = await browser.callFunction(`function (wanted) {
    const row = Array.from(document.querySelectorAll("#vm-table tbody tr[data-vm-id]")).find(function (item) {
      const nameNode = item.querySelector(".pcv-vm-list-name");
      return nameNode && nameNode.textContent.trim() === wanted;
    });
    if (row) return { ok: true, id: row.getAttribute("data-vm-id") || "" };
    const list = (window.state && Array.isArray(state.vms)) ? state.vms : [];
    const vm = list.find(function (item) { return String(item.name || item.vm_name || "") === wanted; });
    return vm ? { ok: true, id: String(vm.id || vm.vm_id || "") } : { ok: false };
  }`, name);
  if (!result || result.ok !== true || !result.id) throw new Error("vm id not found");
  return result.id;
}

// Every write click on a VM goes through here. The element, or its closest [data-vm-id] owner, must carry the id
// of the named scenario VM. A slow detail render then can never send the action to another VM (a preserved VM).
async function clickOwned(browser, selector, name) {
  const vmId = await vmIdFor(browser, name);
  const result = await browser.callFunction(`function (selector, vmId) {
    const node = document.querySelector(selector);
    if (!node) return { ok: false, reason: "missing" };
    const owner = node.hasAttribute("data-vm-id") ? node : node.closest("[data-vm-id]");
    const ownerId = owner ? owner.getAttribute("data-vm-id") || "" : "";
    if (ownerId !== vmId) return { ok: false, reason: "owner-mismatch" };
    node.click();
    return { ok: true };
  }`, selector, vmId);
  if (!result || result.ok !== true) throw new Error(`refused ${selector}: ${result ? result.reason : "no result"}`);
}

async function clickRowMenu(browser, name, action) {
  assertScenarioVmName(name);
  await openVmNav(browser);
  const opened = await browser.callFunction(`function (wanted) {
    const rows = Array.from(document.querySelectorAll("#vm-table tbody tr[data-vm-id]"));
    const row = rows.find(function (item) {
      const id = item.getAttribute("data-vm-id") || "";
      const nameNode = item.querySelector(".pcv-vm-list-name");
      const label = nameNode ? nameNode.textContent.trim() : "";
      return label === wanted || id === wanted;
    });
    if (!row) return { ok: false, reason: "missing-row" };
    const button = row.querySelector("[data-action='vm-row-menu']");
    if (!button) return { ok: false, reason: "missing-menu" };
    button.click();
    return { ok: true };
  }`, name);
  if (!opened || opened.ok !== true) throw new Error(`row menu missed ${action}`);
  await browser.waitFor(`function () {
    const menu = document.getElementById("pcv-vm-row-menu");
    return { ok: !!(menu && !menu.hidden) };
  }`, { timeoutMs: 5000 });
  await clickOwned(browser, `#pcv-vm-row-menu [data-action="${action}"]`, name);
}

async function ensureOff(browser, name) {
  assertScenarioVmName(name);
  const info = await readVm(browser, name);
  if (!info || !info.found) throw new Error("vm not listed");
  if (!info.running) return "already-off";
  await selectVm(browser, name);
  const before = await snapshotJobIds(browser);
  await clickOwned(browser, "#vm-detail-panel [data-action='vm-shutdown']", name);
  const confirm = await acceptConfirm(browser);
  const job = await waitForNewJob(browser, before);
  await waitUntil(browser, `function (wanted) {
    const list = (window.state && Array.isArray(state.vms)) ? state.vms : [];
    const vm = list.find(function (item) {
      const label = String(item.name || item.vm_name || "");
      const id = String(item.id || item.vm_id || "");
      return label === wanted || id === wanted;
    });
    const raw = vm ? String(vm.state || vm.status || "") : "";
    const lower = raw.toLowerCase();
    const off = !!vm && (raw === "" || lower === "off" || lower === "stopped" || lower === "saved");
    return { ok: off, state: raw };
  }`, [name], JOB_TIMEOUT_MS, 500);
  return `${confirm} job_id=${job.job_id} status=${job.status}`;
}

async function startVm(browser, name) {
  assertScenarioVmName(name);
  await selectVm(browser, name);
  const before = await snapshotJobIds(browser);
  await clickOwned(browser, "#vm-detail-panel [data-action='vm-start']", name);
  const confirm = await acceptConfirm(browser);
  const job = await waitForNewJob(browser, before);
  return `${confirm} job_id=${job.job_id} status=${job.status}`;
}

function jobDetail(prefix, job) {
  return `${prefix} job_id=${job.job_id} status=${job.status}`;
}

async function openCheckpointTab(browser) {
  await browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-checkpoints-"]');
  await sleep(300);
}

async function sampleCanvas(browser) {
  return browser.callFunction(`function () {
    const canvas = document.getElementById("vm-console-frame-canvas");
    if (!canvas || typeof canvas.getContext !== "function") return { uniform: true, hash: "", pixels: 0 };
    const context = canvas.getContext("2d");
    if (!context) return { uniform: true, hash: "", pixels: 0 };
    const width = canvas.width || 0;
    const height = canvas.height || 0;
    if (!width || !height) return { uniform: true, hash: "", pixels: 0 };
    let data;
    try { data = context.getImageData(0, 0, width, height).data; }
    catch { return { uniform: true, hash: "", pixels: 0 }; }
    const step = Math.max(1, Math.floor((data.length / 4) / 4000));
    let first = "";
    let uniform = true;
    let hash = 0;
    let count = 0;
    for (let index = 0; index < data.length; index += step * 4) {
      const pixel = data[index] + "," + data[index + 1] + "," + data[index + 2] + "," + data[index + 3];
      if (!first) first = pixel;
      else if (pixel !== first) uniform = false;
      hash = (hash * 33 + data[index] + data[index + 1] * 3 + data[index + 2] * 7 + data[index + 3]) >>> 0;
      count += 1;
    }
    return { uniform: uniform || count === 0, hash: String(hash), pixels: count };
  }`);
}

function parseKeys(raw) {
  const text = String(raw || "Enter");
  if (!text.includes(",")) return [text];
  return text.split(",").map((item) => item.trim()).filter((item) => item.length > 0);
}

async function runCheck(options) {
  const { repoRoot, outDir, web, username, vm, headed } = options;
  const secrets = scenarioSecrets();
  mkdirSync(outDir, { recursive: true });
  const startedAt = new Date().toISOString();
  const steps = [];
  let installedVersion = "";
  let browser = null;
  let currentStep = "login";
  try {
    browser = await launch({ profileDir: join(outDir, "edge-profile"), headed });
    await browser.navigate(web);
    const loginStarted = Date.now();
    const how = await loginSession(browser, { username, manual: false });
    const screenshot = await shot(browser, outDir, repoRoot, "01-login.png");
    steps.push(stepResult("login", loginStarted, "pass", how, screenshot));

    currentStep = "version-badge";
    const versionStarted = Date.now();
    let version = { found: false, text: "" };
    const versionDeadline = Date.now() + 8000;
    while (Date.now() < versionDeadline) {
      version = await browser.callFunction(VERSION_TEXT);
      if (version && version.found) break;
      await sleep(200);
    }
    installedVersion = version && version.found ? String(version.text || "") : "";
    const versionShot = await shot(browser, outDir, repoRoot, "02-version.png");
    steps.push(stepResult("version-badge", versionStarted, version && version.found ? "pass" : "fail", installedVersion || "missing", versionShot));

    currentStep = "vm-list";
    const listStarted = Date.now();
    await browser.click('#shell-sidebar .shell-navitem[data-nav="vms"]');
    await sleep(400);
    const rows = await browser.callFunction(`function () {
      return { ok: true, count: document.querySelectorAll("#vm-table tbody tr[data-vm-id]").length };
    }`);
    const listShot = await shot(browser, outDir, repoRoot, "03-vm-list.png");
    steps.push(stepResult("vm-list", listStarted, rows && rows.count > 0 ? "pass" : "fail", `rows=${rows ? rows.count : 0}`, listShot));

    currentStep = "create-wizard-fields";
    const wizardStarted = Date.now();
    await browser.click("#open-create-vm");
    await browser.waitFor(`function () {
      const dialog = document.getElementById("create-vm-dialog");
      return { ok: !!(dialog && dialog.open) };
    }`, { timeoutMs: 5000 });
    const fields = await probeMap(browser, WIZARD_FIELDS);
    const wizardShot = await shot(browser, outDir, repoRoot, "04-create-wizard.png");
    await browser.click("#close-create-vm");
    const wizardOk = Object.values(fields).every(Boolean);
    steps.push(stepResult("create-wizard-fields", wizardStarted, wizardOk ? "pass" : "fail", fields, wizardShot));

    currentStep = "vm-detail-power";
    const powerStarted = Date.now();
    const picked = await browser.callFunction(`function (wanted) {
      const rows = Array.from(document.querySelectorAll("#vm-table tbody tr[data-vm-id]"));
      const row = rows.find(function (item) {
        const id = item.getAttribute("data-vm-id") || "";
        const nameNode = item.querySelector(".pcv-vm-list-name");
        const name = nameNode ? nameNode.textContent.trim() : "";
        if (!wanted) return true;
        return name === wanted || id === wanted;
      });
      if (!row) return { ok: false, reason: "missing" };
      const nameNode = row.querySelector(".pcv-vm-list-name");
      if (nameNode) nameNode.click();
      else row.click();
      return { ok: true, id: row.getAttribute("data-vm-id") || "" };
    }`, vm || "");
    let power = { start: false, shutdown: false };
    if (picked && picked.ok) {
      await browser.waitFor(`function () {
        const panel = document.querySelector("#vm-detail-panel [data-action='vm-start'], #vm-detail-panel [data-action='vm-shutdown']");
        return { ok: !!panel };
      }`, { timeoutMs: 15000 }).catch(() => null);
      power = {
        start: await probe(browser, "#vm-detail-panel [data-action='vm-start']"),
        shutdown: await probe(browser, "#vm-detail-panel [data-action='vm-shutdown']")
      };
    }
    const powerShot = await shot(browser, outDir, repoRoot, "05-vm-detail.png");
    steps.push(stepResult("vm-detail-power", powerStarted, power.start || power.shutdown ? "pass" : "fail", power, powerShot));

    currentStep = "console-tab";
    const consoleStarted = Date.now();
    if (picked && picked.ok) {
      await browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-console-"]');
      await sleep(300);
    }
    const consoleFields = await probeMap(browser, [
      ["canvas", "#vm-console-frame-canvas"],
      ["size", "select[data-console-frame='size']"],
      ["fps", "select[data-console-frame='fps']"],
      ["text", "form[data-action='vm-console-text']"],
      ["cad", "[data-action='vm-console-input-cad']"]
    ]);
    const consoleShot = await shot(browser, outDir, repoRoot, "06-console.png");
    const consoleOk = consoleFields.canvas && consoleFields.size && consoleFields.fps && consoleFields.text && consoleFields.cad;
    steps.push(stepResult("console-tab", consoleStarted, consoleOk ? "pass" : "fail", consoleFields, consoleShot));

    currentStep = "checkpoint-tab";
    const checkpointStarted = Date.now();
    if (picked && picked.ok) {
      await browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-checkpoints-"]');
      await sleep(300);
    }
    const checkpointFields = await probeMap(browser, [
      ["create", "[data-action='checkpoint-create-open']"],
      ["restore", "[data-action='checkpoint-restore']"],
      ["schedule", "form[data-action='checkpoint-schedule']"],
      ["preview", "[data-action='checkpoint-schedule-preview']"],
      ["save", "[data-action='checkpoint-schedule-set']"],
      ["clear", "[data-action='checkpoint-schedule-clear']"]
    ]);
    const checkpointShot = await shot(browser, outDir, repoRoot, "07-checkpoint.png");
    const checkpointOk = checkpointFields.create && checkpointFields.schedule && checkpointFields.preview && checkpointFields.save && checkpointFields.clear;
    steps.push(stepResult("checkpoint-tab", checkpointStarted, checkpointOk ? "pass" : "fail", checkpointFields, checkpointShot));

    currentStep = "template-lock-clone";
    const templateStarted = Date.now();
    await browser.click('#shell-sidebar .shell-navitem[data-nav="templates"]');
    await sleep(300);
    const templateFields = {
      table: await probe(browser, "#template-table"),
      unlock: await probe(browser, "#template-table [data-action='vm-template-unlock']"),
      clone: await probe(browser, "#template-table [data-action='vm-row-clone']"),
      form: false
    };
    if (templateFields.clone) {
      await browser.click("#template-table [data-action='vm-row-clone']");
      await sleep(300);
      templateFields.form = await probe(browser, "form.pcv-clone-form [name='name']");
      await browser.pressKey("Escape");
    }
    const templateShot = await shot(browser, outDir, repoRoot, "08-template.png");
    const templateOk = templateFields.table && templateFields.unlock && templateFields.clone && templateFields.form;
    steps.push(stepResult("template-lock-clone", templateStarted, templateOk ? "pass" : "fail", templateFields, templateShot));
  } catch (error) {
    const message = redactText(String(error && error.message || error || "check failed"), secrets);
    let screenshot = "";
    if (browser) {
      try { screenshot = await shot(browser, outDir, repoRoot, "99-error.png"); } catch { /* capture is best effort */ }
    }
    if (!steps.some((item) => item.name === currentStep && item.status === "fail")) {
      steps.push(stepResult(currentStep, Date.now(), "fail", message, screenshot));
    }
  } finally {
    if (browser) await browser.close();
  }
  const result = steps.length > 0 && steps.every((item) => item.status === "pass") ? "pass" : "fail";
  const summary = {
    contract: "pcv-browser-scenario-v1",
    scenario: options.scenario,
    mode: "check",
    web,
    installed_version: installedVersion,
    started_at: startedAt,
    finished_at: new Date().toISOString(),
    result,
    steps
  };
  writeFileSync(join(outDir, "summary.json"), serializeScenarioSummary(summary, secrets));
  process.stdout.write(`browser scenario check ${result}\nout=${displayRepoPath(repoRoot, outDir)}\n`);
  process.exit(result === "pass" ? 0 : 1);
}

async function runNamedStep(ctx, name, fn) {
  if (ctx.failed) return;
  const started = Date.now();
  const fileName = `${String(ctx.steps.length + 1).padStart(2, "0")}-${name}.png`;
  try {
    const detail = await fn();
    const screenshot = await shot(ctx.browser, ctx.outDir, ctx.repoRoot, fileName);
    ctx.steps.push(stepResult(name, started, "pass", detail == null ? "" : detail, screenshot));
  } catch (error) {
    let screenshot = "";
    try { screenshot = await shot(ctx.browser, ctx.outDir, ctx.repoRoot, fileName); } catch { /* capture is best effort */ }
    const message = redactText(String(error && error.message || error || "step failed"), ctx.secrets);
    ctx.steps.push(stepResult(name, started, "fail", message, screenshot));
    ctx.failed = true;
  }
}

async function runS1(ctx, args) {
  const vmName = assertScenarioVmName(args.values.vm);
  const iso = String(args.values.iso || "");
  const cpu = String(integerArg(args, "cpu", 2));
  const memory = String(integerArg(args, "memory", 4096));
  const disk = String(integerArg(args, "disk", 32));
  const switchName = args.values.switch || "Default Switch";
  const keys = parseKeys(args.values.keys || "Enter");
  await runNamedStep(ctx, "login", () => loginSession(ctx.browser, { username: args.values.username || "", manual: args.flags.has("manual-login") }));
  await runNamedStep(ctx, "open-create-wizard", async () => {
    await ctx.browser.click("#open-create-vm");
    await ctx.browser.waitFor(`function () {
      const dialog = document.getElementById("create-vm-dialog");
      return { ok: !!(dialog && dialog.open) };
    }`, { timeoutMs: 5000 });
    return "create-vm-dialog";
  });
  await runNamedStep(ctx, "fill-create-fields", async () => {
    assertScenarioVmName(vmName);
    await ctx.browser.fill('#create-vm-form [name="name"]', vmName);
    await waitUntil(ctx.browser, ENABLED, ["#create-vm-next"], 8000);
    await ctx.browser.click("#create-vm-next");
    await ctx.browser.fill('#create-vm-form [name="cpu"]', cpu);
    await ctx.browser.fill('#create-vm-form [name="memory_mb"]', memory);
    await ctx.browser.fill('#create-vm-form [name="disk_gb"]', disk);
    await waitUntil(ctx.browser, ENABLED, ["#create-vm-next"], 8000);
    await ctx.browser.click("#create-vm-next");
    await ctx.browser.fill('#create-vm-form [name="iso_path"]', iso);
    await waitUntil(ctx.browser, ENABLED, ["#create-vm-next"], 8000);
    await ctx.browser.click("#create-vm-next");
    await waitUntil(ctx.browser, ENABLED, ["#create-vm-submit"], 8000);
    return `name=${vmName} cpu=${cpu} memory=${memory} disk=${disk}`;
  });
  await runNamedStep(ctx, "submit-create", async () => {
    assertScenarioVmName(vmName);
    ctx.jobIds = await snapshotJobIds(ctx.browser);
    await ctx.browser.click("#create-vm-submit");
    return "submitted";
  });
  await runNamedStep(ctx, "accept-create-confirm", async () => acceptConfirm(ctx.browser, vmName));
  await runNamedStep(ctx, "wait-create-job", async () => {
    const job = await waitForNewJob(ctx.browser, ctx.jobIds || new Set());
    return jobDetail("create", job);
  });
  await runNamedStep(ctx, "connect-switch", async () => {
    assertScenarioVmName(vmName);
    await selectVm(ctx.browser, vmName);
    await ctx.browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-hardware-"]');
    await sleep(300);
    const nic = await ctx.browser.callFunction(`function (switchName) {
      const vm = window.state && state.selectedVm;
      const nics = vm && Array.isArray(vm.network) ? vm.network : [];
      const connected = nics.some(function (item) {
        return item && (item.switch === switchName || item.name === switchName);
      });
      return { connected: connected };
    }`, switchName);
    if (nic && nic.connected) return `already-connected ${switchName}`;
    const selected = await ctx.browser.callFunction(`function (switchName) {
      const select = document.querySelector('form[data-action="vm-network-connect"] select[name="switch"]');
      if (!select) return { ok: false, reason: "missing-select" };
      const option = Array.from(select.options).find(function (item) {
        return item.value === switchName || item.textContent.trim() === switchName;
      });
      if (!option) return { ok: false, reason: "missing-option" };
      select.value = option.value;
      select.dispatchEvent(new Event("input", { bubbles: true }));
      select.dispatchEvent(new Event("change", { bubbles: true }));
      return { ok: true };
    }`, switchName);
    if (!selected || selected.ok !== true) throw new Error("switch option missing");
    const before = await snapshotJobIds(ctx.browser);
    await clickOwned(ctx.browser, 'form[data-action="vm-network-connect"] button[type="submit"]', vmName);
    const confirm = await acceptConfirm(ctx.browser, "", { required: true });
    const job = await waitForNewJob(ctx.browser, before);
    return `${confirm} ${jobDetail("connect", job)}`;
  });
  await runNamedStep(ctx, "start-vm", async () => startVm(ctx.browser, vmName));
  await runNamedStep(ctx, "open-console", async () => {
    await ctx.browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-console-"]');
    await waitUntil(ctx.browser, PRESENT, ["#vm-console-frame-canvas"], 8000);
    return "console-tab";
  });
  await runNamedStep(ctx, "wait-console-frame", async () => {
    const deadline = Date.now() + 3 * 60 * 1000;
    let startedFrame = false;
    let sample = { uniform: true, hash: "" };
    while (Date.now() < deadline) {
      sample = await sampleCanvas(ctx.browser);
      if (sample && sample.uniform === false) break;
      if (!startedFrame) {
        startedFrame = true;
        await ctx.browser.callFunction(`function () {
          const button = document.querySelector("[data-action='vm-console-frame-start']");
          if (!button || button.disabled) return { ok: true, skipped: true };
          button.click();
          return { ok: true, clicked: true };
        }`);
      }
      await sleep(500);
    }
    if (!sample || sample.uniform !== false) throw new Error("console frame stayed one color");
    ctx.frameHash = String(sample.hash || "");
    return `hash=${ctx.frameHash}`;
  });
  await runNamedStep(ctx, "send-console-keys", async () => {
    await ctx.browser.callFunction(`function () {
      const canvas = document.getElementById("vm-console-frame-canvas");
      if (canvas && typeof canvas.focus === "function") canvas.focus();
      return { ok: !!canvas };
    }`);
    for (const key of keys) {
      await ctx.browser.callFunction(`function (key) {
        const canvas = document.getElementById("vm-console-frame-canvas");
        if (!canvas) return { ok: false };
        canvas.focus();
        const init = { key: key, code: key, bubbles: true, cancelable: true };
        canvas.dispatchEvent(new KeyboardEvent("keydown", init));
        canvas.dispatchEvent(new KeyboardEvent("keyup", init));
        return { ok: true };
      }`, key);
      await ctx.browser.pressKey(key);
    }
    return `keys=${keys.join(",")}`;
  });
  await runNamedStep(ctx, "observe-console-change", async () => {
    const before = String(ctx.frameHash || "");
    const deadline = Date.now() + 60 * 1000;
    let hash = before;
    while (Date.now() < deadline) {
      const sample = await sampleCanvas(ctx.browser);
      hash = String(sample && sample.hash || "");
      if (hash && hash !== before) return `hash=${before}->${hash}`;
      await sleep(500);
    }
    throw new Error(`console frame hash unchanged ${before}`);
  });
}

async function runS2(ctx, args) {
  const source = assertScenarioVmName(args.values.source);
  const clone = assertScenarioVmName(args.values.clone);
  await runNamedStep(ctx, "login", () => loginSession(ctx.browser, { username: args.values.username || "", manual: args.flags.has("manual-login") }));
  await runNamedStep(ctx, "select-source-vm", async () => selectVm(ctx.browser, source));
  await runNamedStep(ctx, "power-off-source", () => ensureOff(ctx.browser, source));
  await runNamedStep(ctx, "template-lock", async () => {
    const info = await readVm(ctx.browser, source);
    if (info && info.template) return "already-template";
    assertScenarioVmName(source);
    const before = await snapshotJobIds(ctx.browser);
    await clickRowMenu(ctx.browser, source, "vm-template-lock");
    const confirm = await acceptConfirm(ctx.browser, "", { required: true });
    const job = await waitForNewJob(ctx.browser, before);
    return `${confirm} ${jobDetail("template-lock", job)}`;
  });
  await runNamedStep(ctx, "open-clone-form", async () => {
    await ctx.browser.click('#shell-sidebar .shell-navitem[data-nav="templates"]');
    await sleep(300);
    const opened = await ctx.browser.callFunction(`function (wanted) {
      const rows = Array.from(document.querySelectorAll("#template-table tr[data-vm-id]"));
      const row = rows.find(function (item) {
        const cell = item.querySelector("td");
        const label = cell ? cell.textContent.trim() : "";
        return label === wanted || (item.getAttribute("data-vm-id") || "") === wanted;
      });
      if (!row) return { ok: false, reason: "missing" };
      const button = row.querySelector("[data-action='vm-row-clone']");
      if (!button) return { ok: false, reason: "missing-button" };
      button.click();
      return { ok: true };
    }`, source);
    if (!opened || opened.ok !== true) throw new Error("clone button missing");
    await waitUntil(ctx.browser, PRESENT, ["form.pcv-clone-form [name='name']"], 8000);
    return "clone-form";
  });
  await runNamedStep(ctx, "submit-clone", async () => {
    assertScenarioVmName(clone);
    await ctx.browser.fill('form.pcv-clone-form [name="name"]', clone);
    const preview = await ctx.browser.callFunction(`function () {
      const modal = document.querySelector(".pcv-clone-modal");
      if (!modal) return { ok: false, reason: "missing" };
      const button = modal.querySelector(":scope > button");
      if (!button || button.disabled) return { ok: false, reason: "disabled" };
      button.click();
      return { ok: true };
    }`);
    if (!preview || preview.ok !== true) throw new Error("clone preview unavailable");
    await ctx.browser.waitFor(`function () {
      const modal = document.querySelector(".pcv-clone-modal");
      if (!modal) return { ok: false };
      const buttons = Array.from(modal.querySelectorAll(":scope > button"));
      return { ok: !!(buttons[1] && !buttons[1].disabled) };
    }`, { timeoutMs: 60000 });
    ctx.jobIds = await snapshotJobIds(ctx.browser);
    ctx.cloneStarted = Date.now();
    const started = await ctx.browser.callFunction(`function () {
      const modal = document.querySelector(".pcv-clone-modal");
      if (!modal) return { ok: false };
      const buttons = Array.from(modal.querySelectorAll(":scope > button"));
      const button = buttons[1];
      if (!button || button.disabled) return { ok: false, reason: "disabled" };
      button.click();
      return { ok: true };
    }`);
    if (!started || started.ok !== true) throw new Error("clone start unavailable");
    return `clone=${clone}`;
  });
  await runNamedStep(ctx, "wait-clone-job", async () => {
    const confirm = await acceptConfirm(ctx.browser);
    const job = await waitForNewJob(ctx.browser, ctx.jobIds || new Set());
    const elapsed = Math.max(0, Date.now() - (ctx.cloneStarted || Date.now())) / 1000;
    ctx.summary.clone_seconds = Math.round(elapsed * 1000) / 1000;
    ctx.summary.clone_within_60s = ctx.summary.clone_seconds <= 60;
    ctx.cloneJob = job;
    return `${confirm} ${jobDetail("clone", job)}`;
  });
  await runNamedStep(ctx, "record-clone-seconds", async () => {
    const job = ctx.cloneJob || { job_id: "", status: "" };
    return `clone_seconds=${ctx.summary.clone_seconds} clone_within_60s=${ctx.summary.clone_within_60s} job_id=${job.job_id} status=${job.status}`;
  });
  await runNamedStep(ctx, "start-clone", async () => {
    if (!args.flags.has("start")) return "start not requested";
    return startVm(ctx.browser, clone);
  });
}

async function runS3(ctx, args) {
  const vmName = assertScenarioVmName(args.values.vm);
  const checkpoint = args.values.checkpoint || "s3-cp1";
  const interval = integerArg(args, "interval", 60);
  const retention = integerArg(args, "retention", 3);
  const waitSchedule = args.flags.has("wait-schedule");
  await runNamedStep(ctx, "login", () => loginSession(ctx.browser, { username: args.values.username || "", manual: args.flags.has("manual-login") }));
  await runNamedStep(ctx, "select-vm", async () => selectVm(ctx.browser, vmName));
  await runNamedStep(ctx, "create-checkpoint", async () => {
    assertScenarioVmName(vmName);
    await openCheckpointTab(ctx.browser);
    await clickOwned(ctx.browser, "#vm-detail-panel [data-action='checkpoint-create-open']", vmName);
    await waitUntil(ctx.browser, PRESENT, ["form.pcv-checkpoint-create-form input[name='name']"], 8000);
    await ctx.browser.fill("form.pcv-checkpoint-create-form input[name='name']", checkpoint);
    const before = await snapshotJobIds(ctx.browser);
    const clicked = await ctx.browser.callFunction(`function () {
      const modal = document.querySelector(".pcv-checkpoint-create-modal");
      if (!modal) return { ok: false, reason: "missing" };
      const button = modal.querySelector(":scope > button");
      if (!button || button.disabled) return { ok: false, reason: "disabled" };
      button.click();
      return { ok: true };
    }`);
    if (!clicked || clicked.ok !== true) throw new Error("checkpoint create unavailable");
    const confirm = await acceptConfirm(ctx.browser);
    const job = await waitForNewJob(ctx.browser, before);
    return `${confirm} ${jobDetail("checkpoint-create", job)}`;
  });
  await runNamedStep(ctx, "restore-checkpoint", async () => {
    assertScenarioVmName(vmName);
    const power = await ensureOff(ctx.browser, vmName);
    await selectVm(ctx.browser, vmName);
    await openCheckpointTab(ctx.browser);
    const before = await snapshotJobIds(ctx.browser);
    const ownerId = await vmIdFor(ctx.browser, vmName);
    const clicked = await ctx.browser.callFunction(`function (wanted, vmId) {
      const rows = Array.from(document.querySelectorAll(".pcv-checkpoint-table tbody tr"));
      const row = rows.find(function (item) {
        const cell = item.querySelector("td");
        return cell && cell.textContent.trim() === wanted;
      });
      if (!row) return { ok: false, reason: "missing" };
      const button = row.querySelector("[data-action='checkpoint-restore']");
      if (!button) return { ok: false, reason: "missing-button" };
      if ((button.getAttribute("data-vm-id") || "") !== vmId) return { ok: false, reason: "owner-mismatch" };
      button.click();
      return { ok: true };
    }`, checkpoint, ownerId);
    if (!clicked || clicked.ok !== true) throw new Error("checkpoint restore missing");
    const confirm = await acceptConfirm(ctx.browser, "", { required: true });
    const job = await waitForNewJob(ctx.browser, before);
    return `${power}; ${confirm} ${jobDetail("checkpoint-restore", job)}`;
  });
  await runNamedStep(ctx, "schedule-preview", async () => {
    await selectVm(ctx.browser, vmName);
    await openCheckpointTab(ctx.browser);
    await ctx.browser.fill('form[data-action="checkpoint-schedule"] [name="interval_minutes"]', String(interval));
    await ctx.browser.fill('form[data-action="checkpoint-schedule"] [name="retention_max"]', String(retention));
    await ctx.browser.click("[data-action='checkpoint-schedule-preview']");
    await waitUntil(ctx.browser, `function () {
      const preview = window.state && state.checkpointSchedulePreview;
      return { ok: !!(preview && preview.updated_at) };
    }`, [], 30000);
    return `interval=${interval} retention=${retention}`;
  });
  await runNamedStep(ctx, "schedule-save", async () => {
    assertScenarioVmName(vmName);
    const before = await snapshotJobIds(ctx.browser);
    await clickOwned(ctx.browser, "#vm-detail-panel [data-action='checkpoint-schedule-set']", vmName);
    const confirm = await acceptConfirm(ctx.browser, "", { required: true });
    const job = await waitForOptionalJob(ctx.browser, before);
    if (!job) return `${confirm} no tracked job`;
    return `${confirm} ${jobDetail("schedule-set", job)}`;
  });
  await runNamedStep(ctx, "wait-schedule", async () => {
    if (!waitSchedule) return "not requested";
    const timeoutMs = (interval + 10) * 60 * 1000;
    const deadline = Date.now() + timeoutMs;
    let found = false;
    while (Date.now() < deadline) {
      await selectVm(ctx.browser, vmName);
      await openCheckpointTab(ctx.browser);
      const names = await ctx.browser.callFunction(`function () {
        const fromRows = Array.from(document.querySelectorAll(".pcv-checkpoint-table tbody tr td:first-child")).map(function (cell) {
          return cell.textContent.trim();
        });
        const fromState = (window.state && Array.isArray(state.selectedVmCheckpoints)) ? state.selectedVmCheckpoints.map(function (item) {
          return String(item.name || item.checkpoint_name || item.id || "");
        }) : [];
        return { names: fromRows.concat(fromState) };
      }`);
      const list = (names && names.names) || [];
      if (list.some((item) => String(item).startsWith("pcv-schedule-"))) {
        found = true;
        break;
      }
      await sleep(5000);
    }
    if (!found) throw new Error("scheduled checkpoint did not appear");
    return "pcv-schedule- present";
  });
  await runNamedStep(ctx, "schedule-clear", async () => {
    if (!waitSchedule) {
      ctx.summary.schedule_left = true;
      return "schedule left in place";
    }
    assertScenarioVmName(vmName);
    await selectVm(ctx.browser, vmName);
    await openCheckpointTab(ctx.browser);
    const before = await snapshotJobIds(ctx.browser);
    await clickOwned(ctx.browser, "#vm-detail-panel [data-action='checkpoint-schedule-clear']", vmName);
    const confirm = await acceptConfirm(ctx.browser, "", { required: true });
    const job = await waitForOptionalJob(ctx.browser, before);
    if (!job) return `${confirm} no tracked job`;
    return `${confirm} ${jobDetail("schedule-clear", job)}`;
  });
}

async function runCleanup(ctx, args) {
  const names = String(args.values.vm || "").split(",").map((item) => item.trim()).filter((item) => item.length > 0);
  for (const name of names) assertScenarioVmName(name);
  await runNamedStep(ctx, "login", () => loginSession(ctx.browser, { username: args.values.username || "", manual: args.flags.has("manual-login") }));
  for (const name of names) {
    if (ctx.failed) return;
    const info = await readVm(ctx.browser, name);
    await runNamedStep(ctx, "select-vm", async () => {
      if (!info || !info.found) return `absent ${name}`;
      await selectVm(ctx.browser, name);
      return name;
    });
    await runNamedStep(ctx, "template-unlock", async () => {
      if (!info || !info.found) return "absent";
      if (!info.template) return "not-template";
      assertScenarioVmName(name);
      await ctx.browser.click('#shell-sidebar .shell-navitem[data-nav="templates"]');
      await sleep(300);
      const before = await snapshotJobIds(ctx.browser);
      const clicked = await ctx.browser.callFunction(`function (wanted) {
        const rows = Array.from(document.querySelectorAll("#template-table tr[data-vm-id]"));
        const row = rows.find(function (item) {
          const cell = item.querySelector("td");
          const label = cell ? cell.textContent.trim() : "";
          return label === wanted || (item.getAttribute("data-vm-id") || "") === wanted;
        });
        if (!row) return { ok: false, reason: "missing" };
        const button = row.querySelector("[data-action='vm-template-unlock']");
        if (!button) return { ok: false, reason: "missing-button" };
        button.click();
        return { ok: true };
      }`, name);
      if (!clicked || clicked.ok !== true) {
        await clickRowMenu(ctx.browser, name, "vm-template-unlock");
      }
      const confirm = await acceptConfirm(ctx.browser, "", { required: true });
      const job = await waitForNewJob(ctx.browser, before);
      return `${confirm} ${jobDetail("template-unlock", job)}`;
    });
    await runNamedStep(ctx, "power-off", async () => {
      if (!info || !info.found) return "absent";
      return ensureOff(ctx.browser, name);
    });
    await runNamedStep(ctx, "delete-vm", async () => {
      if (!info || !info.found) return "absent";
      assertScenarioVmName(name);
      const before = await snapshotJobIds(ctx.browser);
      await clickRowMenu(ctx.browser, name, "vm-delete");
      const confirm = await acceptConfirm(ctx.browser, name, { required: true });
      const job = await waitForNewJob(ctx.browser, before);
      return `${confirm} ${jobDetail("delete", job)}`;
    });
    await runNamedStep(ctx, "confirm-absent", async () => {
      await openVmNav(ctx.browser);
      const still = await ctx.browser.callFunction(`function (wanted) {
        const rows = Array.from(document.querySelectorAll("#vm-table tbody tr[data-vm-id], #template-table tr[data-vm-id]"));
        const found = rows.some(function (item) {
          const id = item.getAttribute("data-vm-id") || "";
          const nameNode = item.querySelector(".pcv-vm-list-name");
          const label = nameNode ? nameNode.textContent.trim() : "";
          const cell = item.querySelector("td");
          const cellText = cell ? cell.textContent.trim() : "";
          return label === wanted || cellText === wanted || id === wanted;
        });
        const list = (window.state && Array.isArray(state.vms)) ? state.vms : [];
        const inState = list.some(function (item) {
          return String(item.name || item.vm_name || "") === wanted || String(item.id || "") === wanted;
        });
        return { ok: !found && !inState, found: found || inState };
      }`, name);
      if (!still || still.ok !== true) throw new Error("vm still listed");
      return `absent ${name}`;
    });
  }
}

async function runExecute(options) {
  const { repoRoot, outDir, web, args, scenario, cleanup, profileDir, headed } = options;
  const secrets = scenarioSecrets();
  mkdirSync(outDir, { recursive: true });
  const ctx = {
    repoRoot,
    outDir,
    browser: null,
    steps: [],
    secrets,
    failed: false,
    summary: {}
  };
  const startedAt = new Date().toISOString();
  try {
    ctx.browser = await launch({ profileDir, headed });
    await ctx.browser.navigate(web);
    if (cleanup) await runCleanup(ctx, args);
    else if (scenario === "s1") await runS1(ctx, args);
    else if (scenario === "s2") await runS2(ctx, args);
    else if (scenario === "s3") await runS3(ctx, args);
  } catch (error) {
    const message = redactText(String(error && error.message || error || "execute failed"), secrets);
    if (!ctx.steps.some((item) => item.status === "fail")) {
      ctx.steps.push(stepResult("login", Date.now(), "fail", message, ""));
    }
    ctx.failed = true;
  } finally {
    if (ctx.browser) await ctx.browser.close();
  }
  const result = ctx.failed || ctx.steps.length === 0 || ctx.steps.some((item) => item.status !== "pass") ? "fail" : "pass";
  const summary = {
    contract: "pcv-browser-scenario-v1",
    scenario: cleanup ? "cleanup" : scenario,
    mode: "execute",
    web,
    profile: displayRepoPath(repoRoot, profileDir),
    started_at: startedAt,
    finished_at: new Date().toISOString(),
    result,
    ...ctx.summary,
    steps: ctx.steps
  };
  writeFileSync(join(outDir, "summary.json"), serializeScenarioSummary(summary, secrets));
  const lines = [`browser scenario execute ${result}`, `out=${displayRepoPath(repoRoot, outDir)}`];
  if (typeof summary.clone_seconds === "number") {
    lines.push(`clone_seconds=${summary.clone_seconds}`);
    lines.push(`clone_within_60s=${summary.clone_within_60s}`);
  }
  if (summary.schedule_left === true) lines.push("schedule_left=true");
  process.stdout.write(`${lines.join("\n")}\n`);
  process.exit(result === "pass" ? 0 : 1);
}

async function main() {
  const args = readScenarioArgs(process.argv.slice(2), "run-browser-scenario.mjs", SPEC);
  const cleanup = args.flags.has("cleanup");
  const scenario = args.values.scenario || "";
  if (scenario && !SCENARIOS.has(scenario)) failUsage("argument error: invalid --scenario");
  if (!scenario && !cleanup) failUsage("argument error: invalid --scenario");
  const mode = scenarioMode(args.flags);
  const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
  requireDigits(args);
  if (args.values.profile) {
    try {
      assertScenarioProfile(args.values.profile, repoRoot);
    } catch {
      failUsage("argument error: invalid --profile");
    }
  }
  const web = args.values.web || "http://127.0.0.1/";
  const outDir = resolve(args.values.out || defaultOutDir(repoRoot, scenario || "cleanup", mode));
  if (mode === "execute") {
    validateExecute(args, scenario, cleanup);
    const profileDir = args.values.profile ? assertScenarioProfile(args.values.profile, repoRoot) : join(outDir, "edge-profile");
    await runExecute({
      repoRoot,
      outDir,
      web,
      args,
      scenario,
      cleanup,
      profileDir,
      headed: args.flags.has("headed") || args.flags.has("manual-login")
    });
    return;
  }
  if (mode === "plan-only") {
    const names = cleanup ? PLAN_STEPS.cleanup : PLAN_STEPS[scenario];
    process.stdout.write(`browser scenario plan-only (no request sent)\nscenario=${scenario || "cleanup"}\nmode=plan-only\nout=${displayRepoPath(repoRoot, outDir)}\n`);
    names.forEach((name, index) => process.stdout.write(`${index + 1}. ${name}\n`));
    return;
  }
  await runCheck({
    repoRoot,
    outDir,
    web,
    scenario,
    username: args.values.username || "",
    vm: args.values.vm || "",
    headed: args.flags.has("headed")
  });
}

if (isDirect()) {
  main().catch((error) => {
    const line = redactText(String(error && error.message || error || "browser scenario failed"), scenarioSecrets());
    process.stderr.write(`browser scenario failed: ${line}\n`);
    process.exit(1);
  });
}

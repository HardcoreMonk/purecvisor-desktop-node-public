// Browser scenarios S1-S3. The default is plan-only and sends no request. --check logs in and looks for screens.
// --execute is left for task 4b. Passwords stay in PCV_SCENARIO_PASSWORD and are never printed.
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { launch } from "./browser-scenario-cdp.mjs";
import { displayRepoPath, formatRunDay, readScenarioArgs, usageText } from "./scenario-args.mjs";

const SPEC = {
  values: ["scenario", "web", "username", "iso", "vm", "source", "clone", "checkpoint", "interval", "retention", "out"],
  booleans: ["execute", "plan-only", "check", "headed", "cleanup", "wait-schedule"]
};
const SCENARIOS = new Set(["s1", "s2", "s3"]);
const SECRET_FIELDS = new Set(["password", "token", "access_token", "refresh_token", "authorization", "pcv_scenario_password"]);

export const PLAN_STEPS = {
  s1: [
    "login",
    "open-create-wizard",
    "fill-create-fields",
    "submit-create",
    "accept-create-confirm",
    "wait-create-job",
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

export function scenarioMode(flags) {
  if (flags.has("execute")) return "execute";
  if (flags.has("check")) return "check";
  return "plan-only";
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

const LOGIN_VISIBLE = `function () {
  const login = document.getElementById("login-page");
  const user = document.getElementById("login-user");
  if (!login || !user) return { ok: true, form: false };
  const display = getComputedStyle(login).display;
  const hidden = login.getAttribute("aria-hidden");
  return { ok: true, form: display !== "none" && hidden !== "true" };
}`;

const SHELL_READY = `function () {
  const login = document.getElementById("login-page");
  const app = document.getElementById("app");
  const display = login ? getComputedStyle(login).display : "";
  const appHidden = app ? app.getAttribute("aria-hidden") : null;
  const connected = !!(window.state && state.connectionState === "connected" && !state.loading);
  return { ok: display === "none" && appHidden !== "true" && connected };
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

async function runCheck(options) {
  const { repoRoot, outDir, web, username, vm, headed } = options;
  const password = process.env.PCV_SCENARIO_PASSWORD || "";
  const secrets = [password].filter(Boolean);
  mkdirSync(outDir, { recursive: true });
  const startedAt = new Date().toISOString();
  const steps = [];
  let installedVersion = "";
  let browser = null;
  try {
    browser = await launch({ profileDir: join(outDir, "edge-profile"), headed });
    await browser.navigate(web);
    const loginStarted = Date.now();
    const visible = await browser.callFunction(LOGIN_VISIBLE);
    if (visible && visible.form) {
      if (!username || !password) {
        const screenshot = await shot(browser, outDir, repoRoot, "01-login.png");
        steps.push(stepResult("login", loginStarted, "fail", username ? "password missing" : "username missing", screenshot));
      } else {
        await browser.fill("#login-user", username);
        await browser.fill("#login-pass", password);
        await browser.click('#login-cred-actions button.login-submit[type="submit"]');
        await browser.waitFor(SHELL_READY, { timeoutMs: 25000 });
        const screenshot = await shot(browser, outDir, repoRoot, "01-login.png");
        steps.push(stepResult("login", loginStarted, "pass", "account", screenshot));
      }
    } else {
      await browser.waitFor(SHELL_READY, { timeoutMs: 25000 });
      const screenshot = await shot(browser, outDir, repoRoot, "01-login.png");
      steps.push(stepResult("login", loginStarted, "pass", "loopback", screenshot));
    }
    if (steps[0] && steps[0].status === "pass") {
      const versionStarted = Date.now();
      let version = { found: false, text: "" };
      const versionDeadline = Date.now() + 8000;
      while (Date.now() < versionDeadline) {
        version = await browser.callFunction(VERSION_TEXT);
        if (version && version.found) break;
        await new Promise((resolve) => setTimeout(resolve, 200));
      }
      installedVersion = version && version.found ? String(version.text || "") : "";
      const screenshot = await shot(browser, outDir, repoRoot, "02-version.png");
      steps.push(stepResult("version-badge", versionStarted, version && version.found ? "pass" : "fail", installedVersion || "missing", screenshot));

      const listStarted = Date.now();
      await browser.click('#shell-sidebar .shell-navitem[data-nav="vms"]');
      await new Promise((resolve) => setTimeout(resolve, 400));
      const rows = await browser.callFunction(`function () {
        return { ok: true, count: document.querySelectorAll("#vm-table tbody tr[data-vm-id]").length };
      }`);
      const listShot = await shot(browser, outDir, repoRoot, "03-vm-list.png");
      steps.push(stepResult("vm-list", listStarted, rows && rows.count > 0 ? "pass" : "fail", `rows=${rows ? rows.count : 0}`, listShot));

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

      const consoleStarted = Date.now();
      if (picked && picked.ok) {
        await browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-console-"]');
        await new Promise((resolve) => setTimeout(resolve, 300));
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

      const checkpointStarted = Date.now();
      if (picked && picked.ok) {
        await browser.click('#vm-detail-panel [role="tab"][id^="pcv-tab-checkpoints-"]');
        await new Promise((resolve) => setTimeout(resolve, 300));
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

      const templateStarted = Date.now();
      await browser.click('#shell-sidebar .shell-navitem[data-nav="templates"]');
      await new Promise((resolve) => setTimeout(resolve, 300));
      const templateFields = {
        table: await probe(browser, "#template-table"),
        unlock: await probe(browser, "#template-table [data-action='vm-template-unlock']"),
        clone: await probe(browser, "#template-table [data-action='vm-row-clone']"),
        form: false
      };
      if (templateFields.clone) {
        await browser.click("#template-table [data-action='vm-row-clone']");
        await new Promise((resolve) => setTimeout(resolve, 300));
        templateFields.form = await probe(browser, "form.pcv-clone-form [name='name']");
        await browser.pressKey("Escape");
      }
      const templateShot = await shot(browser, outDir, repoRoot, "08-template.png");
      const templateOk = templateFields.table && templateFields.unlock && templateFields.clone && templateFields.form;
      steps.push(stepResult("template-lock-clone", templateStarted, templateOk ? "pass" : "fail", templateFields, templateShot));
    }
  } catch (error) {
    const failed = steps.some((item) => item.name === "login") ? steps : steps.concat([stepResult("login", Date.now(), "fail", "screen not ready", "")]);
    steps.splice(0, steps.length, ...failed);
    if (browser) {
      try { await shot(browser, outDir, repoRoot, "99-error.png"); } catch { /* capture is best effort */ }
    }
    const message = String(error && error.message || error || "check failed");
    if (steps.length > 0 && steps[steps.length - 1].status === "fail" && !steps[steps.length - 1].detail) {
      steps[steps.length - 1].detail = "screen not ready";
    } else if (!steps.some((item) => item.detail === "screen not ready")) {
      steps.push(stepResult("check", Date.now(), "fail", "screen not ready", ""));
    }
    void message;
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
  const body = serializeScenarioSummary(summary, secrets);
  writeFileSync(join(outDir, "summary.json"), body);
  process.stdout.write(`browser scenario check ${result}\nout=${displayRepoPath(repoRoot, outDir)}\n`);
  process.exit(result === "pass" ? 0 : 1);
}

async function main() {
  const args = readScenarioArgs(process.argv.slice(2), "run-browser-scenario.mjs", SPEC);
  const scenario = args.values.scenario || "";
  if (!SCENARIOS.has(scenario)) failUsage("argument error: invalid --scenario");
  const mode = scenarioMode(args.flags);
  const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
  const web = args.values.web || "http://127.0.0.1/";
  const outDir = resolve(args.values.out || defaultOutDir(repoRoot, scenario, mode));
  if (mode === "execute") {
    process.stderr.write("browser scenario execute is not implemented\n");
    process.exit(2);
  }
  if (mode === "plan-only") {
    const names = PLAN_STEPS[scenario];
    process.stdout.write(`browser scenario plan-only (no request sent)\nscenario=${scenario}\nmode=plan-only\nout=${displayRepoPath(repoRoot, outDir)}\n`);
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
    process.stderr.write("browser scenario failed\n");
    void error;
    process.exit(1);
  });
}

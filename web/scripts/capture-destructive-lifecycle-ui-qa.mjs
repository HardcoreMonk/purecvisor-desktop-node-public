import { createHash } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { spawn, spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const defaultChromePaths = [
  "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
  "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
  "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
  "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
];
const terminalJobStatuses = new Set(["succeeded", "failed", "canceled"]);
let currentApiBaseUrl = "http://127.0.0.1:7777/";
let currentApiToken = "";

function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const arg = process.argv.slice(2).find((value) => value.startsWith(prefix));
  return arg ? arg.slice(prefix.length) : fallback;
}

function sleep(ms) {
  return new Promise((resolveSleep) => setTimeout(resolveSleep, ms));
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function findChrome() {
  const explicit = getArg("chrome", process.env.PCV_BROWSER_QA_CHROME || "");
  if (explicit && existsSync(explicit)) return explicit;
  const found = defaultChromePaths.find((candidate) => existsSync(candidate));
  if (!found) {
    throw new Error("PCV_BROWSER_QA_CHROME_NOT_FOUND|Chrome or Edge executable was not found.");
  }
  return found;
}

class CdpClient {
  constructor(socket) {
    this.socket = socket;
    this.nextId = 1;
    this.pending = new Map();
    this.events = new Map();
    socket.addEventListener("message", (event) => this.handleMessage(event));
  }

  handleMessage(event) {
    const payload = JSON.parse(String(event.data));
    if (payload.id && this.pending.has(payload.id)) {
      const { resolve: resolvePending, reject } = this.pending.get(payload.id);
      this.pending.delete(payload.id);
      if (payload.error) reject(new Error(`${payload.error.code}|${payload.error.message}`));
      else resolvePending(payload.result || {});
      return;
    }

    const listeners = this.events.get(payload.method) || [];
    for (const listener of listeners) listener(payload.params || {});
  }

  send(method, params = {}) {
    const id = this.nextId;
    this.nextId += 1;
    return new Promise((resolveSend, reject) => {
      this.pending.set(id, { resolve: resolveSend, reject });
      this.socket.send(JSON.stringify({ id, method, params }));
    });
  }

  waitFor(method, timeoutMs = 15000) {
    return new Promise((resolveWait, reject) => {
      const timer = setTimeout(() => {
        reject(new Error(`PCV_BROWSER_QA_EVENT_TIMEOUT|Timed out waiting for ${method}.`));
      }, timeoutMs);
      const listener = (params) => {
        clearTimeout(timer);
        this.off(method, listener);
        resolveWait(params);
      };
      this.on(method, listener);
    });
  }

  on(method, listener) {
    const listeners = this.events.get(method) || [];
    listeners.push(listener);
    this.events.set(method, listeners);
  }

  off(method, listener) {
    const listeners = (this.events.get(method) || []).filter((entry) => entry !== listener);
    this.events.set(method, listeners);
  }

  close() {
    this.socket.close();
  }
}

async function connectCdp(webSocketDebuggerUrl) {
  const socket = new WebSocket(webSocketDebuggerUrl);
  await new Promise((resolveOpen, reject) => {
    socket.addEventListener("open", resolveOpen, { once: true });
    socket.addEventListener("error", reject, { once: true });
  });
  return new CdpClient(socket);
}

async function waitForFile(path, timeoutMs = 15000) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (existsSync(path)) return readFileSync(path, "utf8");
    await sleep(100);
  }
  throw new Error(`PCV_BROWSER_QA_DEVTOOLS_TIMEOUT|DevToolsActivePort was not created at ${path}.`);
}

async function evaluate(client, expression) {
  const result = await client.send("Runtime.evaluate", {
    expression,
    awaitPromise: true,
    returnByValue: true
  });
  if (result.exceptionDetails) {
    const detail = result.exceptionDetails.exception?.description || result.exceptionDetails.text || expression;
    throw new Error(`PCV_BROWSER_QA_EVALUATE_FAILED|${detail}`);
  }
  return result.result?.value;
}

async function waitForExpression(client, expression, timeoutMs = 15000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    last = await evaluate(client, expression);
    if (last) return last;
    await sleep(150);
  }
  throw new Error(`PCV_BROWSER_QA_WAIT_TIMEOUT|Expression did not become truthy.|${expression}|last=${JSON.stringify(last)}`);
}

async function screenshot(client, path) {
  const result = await client.send("Page.captureScreenshot", {
    format: "png",
    fromSurface: true,
    captureBeyondViewport: true
  });
  writeFileSync(path, Buffer.from(result.data, "base64"));
}

async function setViewport(client, width, height) {
  await client.send("Emulation.setDeviceMetricsOverride", {
    width,
    height,
    deviceScaleFactor: 1,
    mobile: width <= 520
  });
}

async function click(client, selector) {
  return evaluate(client, `(() => {
    const node = document.querySelector(${JSON.stringify(selector)});
    if (!node) return { clicked: false, found: false, disabled: false, text: '' };
    const disabled = Boolean(node.disabled || node.getAttribute('aria-disabled') === 'true');
    if (!disabled) node.click();
    return { clicked: !disabled, found: true, disabled, text: node.textContent.trim() };
  })()`);
}

async function requireClick(client, selector, label) {
  const result = await click(client, selector);
  if (!result?.clicked) {
    throw new Error(`PCV_BROWSER_QA_CLICK_FAILED|${label}|selector=${selector}|result=${JSON.stringify(result)}`);
  }
  return result;
}

async function typeInto(client, selector, value) {
  return evaluate(client, `(() => {
    const node = document.querySelector(${JSON.stringify(selector)});
    if (!node) return false;
    node.value = ${JSON.stringify(value)};
    node.dispatchEvent(new Event('input', { bubbles: true }));
    node.dispatchEvent(new Event('change', { bubbles: true }));
    return true;
  })()`);
}

async function navigateView(client, view) {
  await evaluate(client, `(() => {
    window.location.hash = ${JSON.stringify(`#${view}`)};
    window.dispatchEvent(new Event('hashchange'));
    return true;
  })()`);
  await waitForExpression(client, `document.querySelector(${JSON.stringify(`#${view}`)})?.hidden === false`, 10000);
}

async function browserApi(client, method, path, body = null) {
  void client;
  let lastPayload = null;
  for (let attempt = 1; attempt <= 8; attempt += 1) {
    const headers = { Accept: "application/json" };
    if (currentApiToken) headers.Authorization = `Bearer ${currentApiToken}`;
    const options = { method, headers };
    if (body !== null && body !== undefined) {
      headers["Content-Type"] = "application/json";
      options.body = JSON.stringify(body);
    }
    const response = await fetch(new URL(path, currentApiBaseUrl).href, options);
    let json = null;
    try {
      json = await response.json();
    } catch {
      json = null;
    }
    const payload = { ok: response.ok && json?.ok !== false, status: response.status, payload: json };
    if (payload.ok) return payload.payload?.data;
    lastPayload = payload;
    const retryAfter = Number(json?.retry_after_seconds || response.headers.get("Retry-After") || 0);
    const code = String(json?.error?.code || json?.code || "");
    const nativeInventoryTransient = response.status === 502 && /PCV_NATIVE_.*INCOMPLETE/.test(code);
    const retryable = response.status === 429 || response.status === 408 || response.status === 504 || nativeInventoryTransient || json?.retryable === true;
    if (retryable && attempt < 8) {
      await sleep(Math.max(retryAfter, 2) * 1000);
      continue;
    }
    break;
  }
  throw new Error(`PCV_BROWSER_QA_API_FAILED|${method} ${path}|status=${lastPayload?.status}|payload=${JSON.stringify(lastPayload?.payload)}`);
}

async function trackedJobIds(client) {
  return evaluate(client, `(() => state.trackedJobs.map((job) => job.job_id).filter(Boolean))()`);
}

async function latestUiError(client) {
  return evaluate(client, `(() => state.error ? {
    code: state.error.code || '',
    message: state.error.message || '',
    detail: state.error.detail || '',
    status: state.error.status || 0
  } : null)()`);
}

async function waitForNewTrackedJob(client, beforeIds, label, timeoutMs = 20000) {
  const seen = new Set(beforeIds || []);
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const jobs = await evaluate(client, `(() => state.trackedJobs.map((job) => ({
      job_id: job.job_id,
      operation: job.operation || '',
      status: job.status || ''
    })).filter((job) => job.job_id))()`);
    const fresh = jobs.find((job) => !seen.has(job.job_id));
    if (fresh) return fresh;
    const error = await latestUiError(client);
    if (error) {
      throw new Error(`PCV_BROWSER_QA_UI_ERROR|${label}|${JSON.stringify(error)}`);
    }
    await sleep(250);
  }
  throw new Error(`PCV_BROWSER_QA_JOB_NOT_TRACKED|${label}|before=${JSON.stringify([...seen])}`);
}

async function waitForJobTerminal(client, jobId, label, timeoutMs = 240000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    last = await browserApi(client, "GET", `/api/v1/jobs/${encodeURIComponent(jobId)}`);
    const status = String(last?.status || "").toLowerCase();
    if (terminalJobStatuses.has(status)) {
      if (status !== "succeeded") {
        throw new Error(`PCV_BROWSER_QA_JOB_FAILED|${label}|job=${jobId}|status=${status}|job=${JSON.stringify(last)}`);
      }
      return last;
    }
    await sleep(2000);
  }
  throw new Error(`PCV_BROWSER_QA_JOB_TIMEOUT|${label}|job=${jobId}|last=${JSON.stringify(last)}`);
}

function vmNameOf(vm) {
  return String(vm?.name || vm?.id || "");
}

function vmStateOf(vm) {
  return String(vm?.state || vm?.status || "");
}

async function findVm(client, vmName) {
  const vms = await browserApi(client, "GET", "/api/v1/vms");
  return (Array.isArray(vms) ? vms : []).find((vm) => vmNameOf(vm) === vmName || String(vm?.id || "") === vmName) || null;
}

async function waitForVm(client, vmName, predicate, label, timeoutMs = 120000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    last = await findVm(client, vmName);
    if (predicate(last)) return last;
    await sleep(1500);
  }
  throw new Error(`PCV_BROWSER_QA_VM_WAIT_TIMEOUT|${label}|last=${JSON.stringify(last)}`);
}

async function waitForVmAbsent(client, vmName, timeoutMs = 120000) {
  return waitForVm(client, vmName, (vm) => !vm, "vm.absent", timeoutMs);
}

async function listCheckpoints(client, vmName) {
  const checkpoints = await browserApi(client, "GET", `/api/v1/vms/${encodeURIComponent(vmName)}/checkpoints`);
  return Array.isArray(checkpoints) ? checkpoints : [];
}

function checkpointIdOf(checkpoint) {
  return String(checkpoint?.id || checkpoint?.checkpoint_id || checkpoint?.name || "");
}

function checkpointNameOf(checkpoint) {
  return String(checkpoint?.name || checkpoint?.id || checkpoint?.checkpoint_id || "");
}

async function waitForCheckpoint(client, vmName, checkpointName, expectedPresent, timeoutMs = 120000) {
  const deadline = Date.now() + timeoutMs;
  let last = [];
  while (Date.now() < deadline) {
    last = await listCheckpoints(client, vmName);
    const found = last.find((checkpoint) => checkpointNameOf(checkpoint) === checkpointName || checkpointIdOf(checkpoint) === checkpointName);
    if (expectedPresent && found) return found;
    if (!expectedPresent && !found) return null;
    await sleep(1500);
  }
  throw new Error(`PCV_BROWSER_QA_CHECKPOINT_WAIT_TIMEOUT|name=${checkpointName}|expectedPresent=${expectedPresent}|last=${JSON.stringify(last)}`);
}

async function refreshUi(client) {
  await evaluate(client, `(async () => {
    try { await loadVms(); } catch (error) { state.error = normalizeError(error); }
    try { if (state.selectedVmId) await refreshSelectedVm(); } catch (error) { state.error = normalizeError(error); }
    try { await loadServerJobs(); } catch (_) {}
    render();
    return true;
  })()`);
  await sleep(800);
}

async function selectVmInUi(client, vmName) {
  await navigateView(client, "vms");
  await refreshUi(client);
  await typeInto(client, "#vm-filter", vmName);
  await waitForExpression(client, `!![...document.querySelectorAll('#vm-table button[data-action="select-vm"]')].find((button) => button.textContent.trim() === ${JSON.stringify(vmName)} || button.dataset.vmId === ${JSON.stringify(vmName)})`, 30000);
  await evaluate(client, `(() => {
    const button = [...document.querySelectorAll('#vm-table button[data-action="select-vm"]')]
      .find((entry) => entry.textContent.trim() === ${JSON.stringify(vmName)} || entry.dataset.vmId === ${JSON.stringify(vmName)});
    button.click();
    return true;
  })()`);
  await waitForExpression(client, `document.querySelector('#vm-detail-title')?.textContent.trim() === ${JSON.stringify(vmName)}`, 30000);
}

async function captureState(client, name, outDir, screenshots, states, width = 1366, height = 900) {
  await setViewport(client, width, height);
  await sleep(350);
  const path = join(outDir, `${name}.png`);
  await screenshot(client, path);
  screenshots.push({ name, path, width, height, sha256: sha256(readFileSync(path)) });
  states.push({
    name,
    ...(await evaluate(client, `(() => ({
      href: location.href,
      visible_view: [...document.querySelectorAll('.app-view')].find((node) => !node.hidden)?.id || '',
      connection_state: document.querySelector('#connection-state')?.textContent || '',
      alert_text: document.querySelector('#alert-region')?.textContent || '',
      selected_vm_title: document.querySelector('#vm-detail-title')?.textContent || '',
      selected_vm_detail: document.querySelector('#vm-detail-content')?.textContent || '',
      checkpoint_text: document.querySelector('.checkpoint-panel')?.textContent || '',
      jobs_text: document.querySelector('#jobs-panel')?.textContent || ''
    }))()`))
  });
}

async function runUiJob(client, actions, label, selector, screenshotName, outDir, screenshots, states) {
  const before = await trackedJobIds(client);
  const clickResult = await requireClick(client, selector, label);
  const tracked = await waitForNewTrackedJob(client, before, label);
  const completed = await waitForJobTerminal(client, tracked.job_id, label);
  const confirms = await evaluate(client, "(() => window.__pcvConfirmMessages || [])()");
  actions.push({
    label,
    selector,
    click_text: clickResult.text,
    job_id: tracked.job_id,
    operation: tracked.operation || completed?.operation || "",
    status: completed?.status || "",
    confirm_count: Array.isArray(confirms) ? confirms.length : 0
  });
  await refreshUi(client);
  if (screenshotName) await captureState(client, screenshotName, outDir, screenshots, states);
  return completed;
}

async function terminateBrowser(browser) {
  if (!browser || browser.exitCode !== null) return;
  if (process.platform === "win32" && browser.pid) {
    spawnSync("taskkill.exe", ["/PID", String(browser.pid), "/T", "/F"], { stdio: "ignore" });
  } else {
    browser.kill();
  }
  const exited = await new Promise((resolveExit) => {
    const timer = setTimeout(() => resolveExit(false), 2000);
    browser.once("exit", () => {
      clearTimeout(timer);
      resolveExit(true);
    });
  });
  if (!exited) {
    browser.kill("SIGKILL");
    await sleep(500);
  }
}

async function removeProfileDirectory(profileDir) {
  const deadline = Date.now() + 5000;
  while (Date.now() < deadline) {
    try {
      rmSync(profileDir, { recursive: true, force: true });
      return true;
    } catch {
      await sleep(250);
    }
  }
  return false;
}

async function main() {
  const url = getArg("url", "http://127.0.0.1:7777/");
  const outDir = resolve(getArg("out", join(repoRoot, "artifacts", `web-console-destructive-lifecycle-ui-${new Date().toISOString().replace(/[-:]/g, "").replace(/\..+/, "Z")}`)));
  const token = process.env.PCV_BROWSER_QA_TOKEN || "";
  const isoPath = getArg("iso", "D:\\Downloads\\Rocky-10.1-x86_64-minimal.iso");
  const vmRoot = getArg("vm-root", join(process.env.TEMP || "C:\\Windows\\Temp", "pcv-hyperv-ui-smoke"));
  const vmName = getArg("vm-name", `pcv-spike-ui-${Date.now().toString(36)}`);
  const checkpointName = getArg("checkpoint-name", "ui-before-restore");
  const chromePath = findChrome();
  currentApiBaseUrl = url;
  currentApiToken = token;
  const profileDir = join(outDir, "chrome-profile");
  const screenshots = [];
  const states = [];
  const actions = [];
  const summary = {
    schema_version: 1,
    ok: false,
    url,
    artifact_root: outDir,
    vm_name: vmName,
    checkpoint_name: checkpointName,
    iso_path: isoPath,
    vm_root: vmRoot,
    host_mutation_performed: true,
    mutation_source: "installed-listener-web-console-ui",
    public_trusted_signing: "not-claimed",
    external_stable_publication: "not-claimed",
    token: {
      supplied: Boolean(token),
      value_observed: false
    },
    browser: {
      executable: chromePath,
      engine: "chrome-cdp-headless"
    },
    actions,
    screenshots,
    states,
    cleanup: {
      fallback_used: false,
      vm_absent_after_delete: false
    }
  };

  mkdirSync(outDir, { recursive: true });
  rmSync(profileDir, { recursive: true, force: true });
  mkdirSync(profileDir, { recursive: true });

  const chrome = spawn(chromePath, [
    "--headless=new",
    "--disable-gpu",
    "--disable-background-networking",
    "--disable-breakpad",
    "--disable-component-update",
    "--disable-default-apps",
    "--disable-extensions",
    "--disable-sync",
    "--metrics-recording-only",
    "--no-first-run",
    "--no-default-browser-check",
    "--remote-debugging-port=0",
    `--user-data-dir=${profileDir}`,
    "--window-size=1366,900",
    "about:blank"
  ], { stdio: "ignore" });

  let client = null;
  try {
    const activePort = await waitForFile(join(profileDir, "DevToolsActivePort"));
    const [port] = activePort.trim().split(/\r?\n/);
    const targetResponse = await fetch(`http://127.0.0.1:${port}/json/new?${encodeURIComponent(url)}`, { method: "PUT" });
    const target = await targetResponse.json();
    client = await connectCdp(target.webSocketDebuggerUrl);

    await client.send("Page.enable");
    await client.send("Runtime.enable");
    await client.send("DOM.enable");
    await client.send("Network.enable");
    await setViewport(client, 1366, 900);
    await client.waitFor("Page.loadEventFired", 20000).catch(() => null);
    await waitForExpression(client, "document.readyState === 'complete' && !!document.querySelector('#connection-form')", 20000);
    await evaluate(client, `(() => {
      window.__pcvConfirmMessages = [];
      window.confirm = (message) => {
        window.__pcvConfirmMessages.push(String(message || ''));
        return true;
      };
      return true;
    })()`);

    if (token) {
      await typeInto(client, "#api-token", token);
      await evaluate(client, "document.querySelector('#connection-form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))");
      await sleep(2500);
    }
    await captureState(client, "00-dashboard-before-create", outDir, screenshots, states);

    await requireClick(client, "#open-create-vm", "open-create-vm");
    await waitForExpression(client, "document.querySelector('#create-vm-dialog')?.open === true", 10000);
    await typeInto(client, '#create-vm-form input[name="name"]', vmName);
    await typeInto(client, '#create-vm-form input[name="iso_path"]', isoPath);
    await typeInto(client, '#create-vm-form input[name="vm_root"]', vmRoot);
    await typeInto(client, '#create-vm-form input[name="cpu"]', "1");
    await typeInto(client, '#create-vm-form input[name="memory_mb"]', "1024");
    await typeInto(client, '#create-vm-form input[name="disk_gb"]', "8");
    await typeInto(client, '#create-vm-form input[name="generation"]', "2");
    await runUiJob(client, actions, "vm.create", '#create-vm-form button[type="submit"]', null, outDir, screenshots, states);
    await waitForVm(client, vmName, Boolean, "vm.present-after-create", 120000);

    await selectVmInUi(client, vmName);
    await captureState(client, "01-vm-selected-after-create", outDir, screenshots, states);

    await runUiJob(client, actions, "vm.start", '#vm-detail-panel button[data-action="vm-start"]', "02-vm-start-job", outDir, screenshots, states);
    await waitForVm(client, vmName, (vm) => /running/i.test(vmStateOf(vm)), "vm.running-after-start", 120000);
    await selectVmInUi(client, vmName);

    await runUiJob(client, actions, "vm.restart", '#vm-detail-panel button[data-action="vm-restart"]', "03-vm-restart-job", outDir, screenshots, states);
    await waitForVm(client, vmName, (vm) => /running/i.test(vmStateOf(vm)), "vm.running-after-restart", 120000);
    await selectVmInUi(client, vmName);

    await typeInto(client, '.checkpoint-form input[name="checkpoint_name"]', checkpointName);
    await runUiJob(client, actions, "checkpoint.create", '.checkpoint-form button[type="submit"]', "04-checkpoint-created", outDir, screenshots, states);
    const checkpoint = await waitForCheckpoint(client, vmName, checkpointName, true, 120000);
    const checkpointId = checkpointIdOf(checkpoint);
    await refreshUi(client);

    await runUiJob(client, actions, "vm.poweroff", '#vm-detail-panel button[data-action="vm-poweroff"]', "05-vm-powered-off", outDir, screenshots, states);
    await waitForVm(client, vmName, (vm) => /(off|stopped)/i.test(vmStateOf(vm)), "vm.off-after-poweroff", 120000);
    await selectVmInUi(client, vmName);

    await waitForExpression(client, `!!document.querySelector('#vm-detail-panel button[data-action="checkpoint-restore"][data-checkpoint-id="${checkpointId.replace(/\\/g, "\\\\").replace(/"/g, '\\"')}"]') || !!document.querySelector('#vm-detail-panel button[data-action="checkpoint-restore"]')`, 30000);
    await runUiJob(client, actions, "checkpoint.restore", '#vm-detail-panel button[data-action="checkpoint-restore"]', "06-checkpoint-restored", outDir, screenshots, states);

    await runUiJob(client, actions, "checkpoint.delete", '#vm-detail-panel button[data-action="checkpoint-delete"]', "07-checkpoint-deleted", outDir, screenshots, states);
    await waitForCheckpoint(client, vmName, checkpointName, false, 120000);

    await runUiJob(client, actions, "vm.delete", '#vm-detail-panel button[data-action="vm-delete"]', "08-vm-delete-job", outDir, screenshots, states);
    await waitForVmAbsent(client, vmName, 120000);
    summary.cleanup.vm_absent_after_delete = true;

    await navigateView(client, "jobs");
    await refreshUi(client);
    await captureState(client, "09-jobs-after-delete", outDir, screenshots, states);
    summary.confirm_messages = await evaluate(client, "(() => window.__pcvConfirmMessages || [])()");
    summary.ok = true;
  } catch (error) {
    summary.error = {
      message: error.message,
      stack: error.stack
    };
    if (client) {
      try {
        const vm = await findVm(client, vmName);
        if (vm) {
          summary.cleanup.fallback_used = true;
          await browserApi(client, "POST", `/api/v1/vms/${encodeURIComponent(vmName)}/poweroff`).catch(() => null);
          await sleep(3000);
          await browserApi(client, "DELETE", `/api/v1/vms/${encodeURIComponent(vmName)}`).catch(() => null);
          await sleep(3000);
          summary.cleanup.vm_absent_after_delete = !(await findVm(client, vmName));
        }
      } catch (cleanupError) {
        summary.cleanup.error = cleanupError.message;
      }
    }
    throw error;
  } finally {
    writeFileSync(join(outDir, "summary.json"), JSON.stringify(summary, null, 2));
    console.log(JSON.stringify(summary, null, 2));
    if (client) {
      await client.send("Browser.close").catch(() => null);
      try {
        client.close();
      } catch {
        // Browser.close may already have closed the DevTools socket.
      }
    }
    await terminateBrowser(chrome);
    await removeProfileDirectory(profileDir);
  }
}

main().catch((error) => {
  console.error(error.stack || String(error));
  process.exit(1);
});

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

function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const arg = process.argv.slice(2).find((value) => value.startsWith(prefix));
  return arg ? arg.slice(prefix.length) : fallback;
}

function sleep(ms) {
  return new Promise((resolveSleep) => setTimeout(resolveSleep, ms));
}

function sha256(value) {
  return createHash("sha256").update(String(value)).digest("hex");
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
    const message = JSON.stringify({ id, method, params });
    return new Promise((resolveSend, reject) => {
      this.pending.set(id, { resolve: resolveSend, reject });
      this.socket.send(message);
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

async function waitForExpression(client, expression, timeoutMs = 15000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    const result = await evaluate(client, expression);
    last = result;
    if (result) return result;
    await sleep(150);
  }
  throw new Error(`PCV_BROWSER_QA_WAIT_TIMEOUT|Expression did not become truthy.|${expression}|last=${last}`);
}

async function evaluate(client, expression) {
  const result = await client.send("Runtime.evaluate", {
    expression,
    awaitPromise: true,
    returnByValue: true
  });
  if (result.exceptionDetails) {
    throw new Error(`PCV_BROWSER_QA_EVALUATE_FAILED|${result.exceptionDetails.text || expression}`);
  }
  return result.result?.value;
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
    if (!node) return false;
    node.click();
    return true;
  })()`);
}

async function navigateView(client, view) {
  return evaluate(client, `(() => {
    window.location.hash = ${JSON.stringify(`#${view}`)};
    window.dispatchEvent(new Event('hashchange'));
    return document.querySelector(${JSON.stringify(`#${view}`)})?.hidden === false;
  })()`);
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

async function collectState(client) {
  return evaluate(client, `(() => {
    const visibleView = [...document.querySelectorAll('.app-view')].find((node) => !node.hidden)?.id || '';
    const missingButtonLabels = [...document.querySelectorAll('button')]
      .filter((button) => !button.textContent.trim() && !button.getAttribute('aria-label'))
      .map((button) => button.id || button.dataset.action || button.outerHTML.slice(0, 80));
    const unlabeledInputs = [...document.querySelectorAll('input,select')]
      .filter((input) => !input.getAttribute('aria-label') && !input.closest('label') && !document.querySelector('label[for="' + input.id + '"]'))
      .map((input) => input.id || input.name);
    return {
      href: location.href,
      visible_view: visibleView,
      connection_state: document.querySelector('#connection-state')?.textContent || '',
      alert_text: document.querySelector('#alert-region')?.textContent || '',
      selected_vm_title: document.querySelector('#vm-detail-title')?.textContent || '',
      diagnostics_text: document.querySelector('#diagnostics-panel')?.textContent || '',
      jobs_text: document.querySelector('#jobs-panel')?.textContent || '',
      network_text: document.querySelector('#network-inventory-panel')?.textContent || '',
      missing_button_labels: missingButtonLabels,
      unlabeled_inputs: unlabeledInputs
    };
  })()`);
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
  let lastError = null;
  while (Date.now() < deadline) {
    try {
      rmSync(profileDir, { recursive: true, force: true });
      return true;
    } catch (error) {
      lastError = error;
      await sleep(250);
    }
  }
  console.error(`PCV_BROWSER_QA_PROFILE_CLEANUP_WARNING|${lastError?.message || "profile cleanup timed out"}`);
  return false;
}

async function main() {
  const url = getArg("url", "http://127.0.0.1:7777/");
  const outDir = resolve(getArg("out", join(repoRoot, "output", "playwright", `installed-listener-qa-${new Date().toISOString().replace(/[-:]/g, "").replace(/\..+/, "Z")}`)));
  const token = process.env.PCV_BROWSER_QA_TOKEN || "";
  const accountUsername = process.env.PCV_BROWSER_QA_ACCOUNT_USERNAME || "";
  const accountPassword = process.env.PCV_BROWSER_QA_ACCOUNT_PASSWORD || "";
  const chromePath = findChrome();
  const profileDir = join(outDir, "chrome-profile");
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
    "--window-size=2048,1152",
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
    await setViewport(client, 2048, 1152);
    await client.waitFor("Page.loadEventFired", 20000).catch(() => null);
    await waitForExpression(client, "document.readyState === 'complete' && !!document.querySelector('#connection-form')", 20000);

    if (accountUsername && accountPassword) {
      await navigateView(client, "troubleshooting");
      await sleep(500);
      await typeInto(client, "#account-username", accountUsername);
      await typeInto(client, "#account-password", accountPassword);
      await evaluate(client, "document.querySelector('#account-login-form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))");
      await waitForExpression(client, "Boolean(window.sessionStorage.getItem('pcvDesktopAccountSession.v1'))", 20000);
      await sleep(1500);
      await navigateView(client, "dashboard");
      await sleep(500);
    } else if (token) {
      await typeInto(client, "#api-token", token);
      await evaluate(client, "document.querySelector('#connection-form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))");
      await sleep(2500);
    }

    const states = [];
    const screenshots = [];
    async function capture(name, width = 2048, height = 1152) {
      await setViewport(client, width, height);
      await sleep(350);
      const path = join(outDir, `${name}.png`);
      await screenshot(client, path);
      screenshots.push({ name, path, width, height, sha256: sha256(readFileSync(path)) });
      states.push({ name, ...(await collectState(client)) });
    }

    await capture("dashboard-wide", 2048, 1152);
    await navigateView(client, "vms");
    await sleep(500);
    await typeInto(client, "#vm-filter", "pcv");
    await typeInto(client, "#vm-state-filter", "running");
    await typeInto(client, "#vm-sort", "updated");
    const vmSelected = await click(client, '#vm-table [data-action="select-vm"]');
    await sleep(1000);
    await capture("vm-detail", 1366, 900);

    await navigateView(client, "jobs");
    await sleep(500);
    await capture("jobs", 1366, 900);

    await navigateView(client, "network");
    await sleep(500);
    await capture("network", 1366, 900);

    await navigateView(client, "troubleshooting");
    await sleep(500);
    const diagnosticCreateClicked = await click(client, '[data-action="diagnostic-create"]');
    await sleep(2500);
    const diagnosticDownloadClicked = await click(client, '[data-action="diagnostic-download"]:not([disabled])');
    await sleep(1200);
    await capture("troubleshooting-diagnostics", 1366, 900);

    await navigateView(client, "dashboard");
    await capture("dashboard-1366", 1366, 768);
    await capture("dashboard-tablet", 900, 900);
    await capture("dashboard-mobile", 390, 860);

    const summary = {
      schema_version: 1,
      ok: true,
      url,
      artifact_root: outDir,
      browser: {
        executable: chromePath,
        engine: "chrome-cdp-headless"
      },
      token: {
        supplied: Boolean(token),
        value_observed: false
      },
      account: {
        login_supplied: Boolean(accountUsername && accountPassword),
        password_value_observed: false
      },
      actions: {
        dashboard_loaded: true,
        vm_filter_sort_exercised: true,
        vm_select_clicked: Boolean(vmSelected),
        jobs_view_clicked: true,
        network_view_clicked: true,
        troubleshooting_view_clicked: true,
        diagnostic_create_clicked: Boolean(diagnosticCreateClicked),
        diagnostic_download_clicked: Boolean(diagnosticDownloadClicked)
      },
      accessibility_probe: {
        missing_button_label_count: Math.max(...states.map((state) => state.missing_button_labels.length), 0),
        unlabeled_input_count: Math.max(...states.map((state) => state.unlabeled_inputs.length), 0)
      },
      screenshots,
      states
    };
    writeFileSync(join(outDir, "summary.json"), JSON.stringify(summary, null, 2));
    console.log(JSON.stringify(summary, null, 2));
  } finally {
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

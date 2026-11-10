// Edge CDP helper for the browser scenarios. The launch flags, DevToolsActivePort handshake, and process-tree
// shutdown follow web/scripts/verify-browser-ui-gates.mjs. This file does not start a browser on import.
import { spawn, spawnSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import path from "node:path";

const EDGE_PATHS = [
  "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
  "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
];
const LINUX_BROWSERS = ["google-chrome", "google-chrome-stable", "chromium", "chromium-browser", "microsoft-edge"];
const VIEWPORT = { width: 1440, height: 900 };

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function commandOnPath(name) {
  const locator = process.platform === "win32" ? "where.exe" : "which";
  const result = spawnSync(locator, [name], { encoding: "utf8" });
  if (result.status !== 0) return "";
  const line = String(result.stdout || "").split(/\r?\n/).map((item) => item.trim()).find(Boolean);
  return line && existsSync(line) ? line : "";
}

function findBrowser() {
  const explicit = process.env.PCV_BROWSER_PATH || "";
  if (explicit) {
    if (!existsSync(explicit)) throw new Error("PCV_BROWSER_PATH does not exist");
    return explicit;
  }
  const edge = EDGE_PATHS.find((candidate) => existsSync(candidate));
  if (edge) return edge;
  for (const name of LINUX_BROWSERS) {
    const found = commandOnPath(name);
    if (found) return found;
  }
  throw new Error("browser executable was not found");
}

async function waitForFile(filePath, timeoutMs = 15000) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (existsSync(filePath)) return readFileSync(filePath, "utf8");
    await sleep(100);
  }
  throw new Error("DevToolsActivePort was not created");
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
      const pending = this.pending.get(payload.id);
      this.pending.delete(payload.id);
      if (payload.error) pending.reject(new Error(`${payload.error.code}|${payload.error.message}`));
      else pending.resolve(payload.result || {});
      return;
    }
    const listeners = this.events.get(payload.method) || [];
    for (const listener of listeners) listener(payload.params || {});
  }

  send(method, params = {}) {
    const id = this.nextId;
    this.nextId += 1;
    const message = JSON.stringify({ id, method, params });
    return new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });
      this.socket.send(message);
    });
  }

  waitFor(method, timeoutMs = 15000) {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error(`event timeout ${method}`)), timeoutMs);
      const listener = (params) => {
        clearTimeout(timer);
        this.off(method, listener);
        resolve(params);
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
    this.events.set(method, (this.events.get(method) || []).filter((entry) => entry !== listener));
  }

  close() {
    try { this.socket.close(); } catch { /* socket already closed */ }
  }
}

async function connectCdp(webSocketDebuggerUrl) {
  const socket = new WebSocket(webSocketDebuggerUrl);
  await new Promise((resolve, reject) => {
    socket.addEventListener("open", resolve, { once: true });
    socket.addEventListener("error", reject, { once: true });
  });
  return new CdpClient(socket);
}

async function openTarget(port) {
  const endpoint = `http://127.0.0.1:${port}/json/new?${encodeURIComponent("about:blank")}`;
  let response = await fetch(endpoint, { method: "PUT" });
  if (!response.ok) response = await fetch(endpoint, { method: "POST" });
  if (response.ok) return response.json();
  const list = await fetch(`http://127.0.0.1:${port}/json/list`);
  const targets = await list.json();
  const page = Array.isArray(targets) ? targets.find((target) => target.type === "page" && target.webSocketDebuggerUrl) : null;
  if (!page) throw new Error(`cdp target failed ${response.status}`);
  return page;
}

function callArgument(value) {
  if (value === undefined) return { unserializableValue: "undefined" };
  return { value };
}

function readEvaluate(result) {
  if (result.exceptionDetails) {
    const text = result.exceptionDetails.text || result.exceptionDetails.exception?.description || "evaluate failed";
    throw new Error(String(text).slice(0, 500));
  }
  return result.result?.value;
}

function killProcessTree(child, profileDir) {
  if (process.platform === "win32") {
    if (child && child.pid != null) spawnSync("taskkill.exe", ["/PID", String(child.pid), "/T", "/F"], { stdio: "ignore" });
    if (profileDir) {
      const command = `$marker = ${JSON.stringify(profileDir)}; Get-CimInstance Win32_Process -Filter "Name = 'msedge.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($marker) } | ForEach-Object { & taskkill.exe /PID $_.ProcessId /T /F | Out-Null }`;
      spawnSync("powershell.exe", ["-NoProfile", "-Command", command], { stdio: "ignore" });
    }
    return;
  }
  if (child && child.pid != null && child.exitCode === null) {
    try { process.kill(-child.pid, "SIGKILL"); } catch { try { child.kill("SIGKILL"); } catch { /* already exited */ } }
  }
}

export class BrowserScenarioCdp {
  constructor(child, client, profileDir) {
    this.child = child;
    this.client = client;
    this.profileDir = profileDir;
    this.closed = false;
  }

  async navigate(url) {
    const loaded = this.client.waitFor("Page.loadEventFired", 20000);
    await this.client.send("Page.navigate", { url });
    await loaded.catch(() => null);
    const deadline = Date.now() + 20000;
    while (Date.now() < deadline) {
      const ready = await this.evaluate("document.readyState");
      if (ready === "complete" || ready === "interactive") return;
      await sleep(100);
    }
    throw new Error("navigate timeout");
  }

  async evaluate(expression) {
    const result = await this.client.send("Runtime.evaluate", {
      expression,
      awaitPromise: true,
      returnByValue: true
    });
    return readEvaluate(result);
  }

  // fnSource is a function declaration. Arguments travel as CDP values, never interpolated into the source.
  async callFunction(fnSource, ...args) {
    const owner = await this.client.send("Runtime.evaluate", {
      expression: "globalThis",
      returnByValue: false
    });
    if (owner.exceptionDetails) readEvaluate(owner);
    const objectId = owner.result?.objectId;
    if (!objectId) throw new Error("cdp global missing");
    try {
      const result = await this.client.send("Runtime.callFunctionOn", {
        objectId,
        functionDeclaration: fnSource,
        arguments: args.map(callArgument),
        awaitPromise: true,
        returnByValue: true
      });
      return readEvaluate(result);
    } finally {
      await this.client.send("Runtime.releaseObject", { objectId }).catch(() => null);
    }
  }

  async waitFor(predicateSource, { timeoutMs = 20000, intervalMs = 150 } = {}) {
    const deadline = Date.now() + timeoutMs;
    let last = null;
    while (Date.now() < deadline) {
      last = await this.callFunction(predicateSource);
      if (last && last.ok) return last;
      await sleep(intervalMs);
    }
    const error = new Error("wait timeout");
    error.detail = last;
    throw error;
  }

  async click(selector) {
    const result = await this.callFunction(`function (selector) {
      const node = document.querySelector(selector);
      if (!node) return { ok: false, reason: "missing" };
      node.click();
      return { ok: true };
    }`, selector);
    if (!result || result.ok !== true) {
      const error = new Error(`click missed ${selector}`);
      error.detail = result || null;
      throw error;
    }
    return result;
  }

  async fill(selector, value) {
    const result = await this.callFunction(`function (selector, value) {
      const node = document.querySelector(selector);
      if (!node) return { ok: false, reason: "missing" };
      node.focus();
      node.value = value;
      node.dispatchEvent(new Event("input", { bubbles: true }));
      node.dispatchEvent(new Event("change", { bubbles: true }));
      return { ok: true };
    }`, selector, value);
    if (!result || result.ok !== true) {
      const error = new Error(`fill missed ${selector}`);
      error.detail = result || null;
      throw error;
    }
    return result;
  }

  async pressKey(key) {
    const named = {
      Enter: { key: "Enter", code: "Enter", windowsVirtualKeyCode: 13, nativeVirtualKeyCode: 13 },
      Escape: { key: "Escape", code: "Escape", windowsVirtualKeyCode: 27, nativeVirtualKeyCode: 27 },
      Tab: { key: "Tab", code: "Tab", windowsVirtualKeyCode: 9, nativeVirtualKeyCode: 9 }
    };
    const event = named[key] || {
      key,
      code: key.length === 1 ? `Key${key.toUpperCase()}` : key,
      windowsVirtualKeyCode: key.length === 1 ? key.toUpperCase().charCodeAt(0) : 0,
      nativeVirtualKeyCode: key.length === 1 ? key.toUpperCase().charCodeAt(0) : 0
    };
    const down = { type: "keyDown", ...event };
    const up = { type: "keyUp", ...event };
    if (key.length === 1 && !named[key]) {
      down.text = key;
      up.text = key;
    }
    await this.client.send("Input.dispatchKeyEvent", down);
    await this.client.send("Input.dispatchKeyEvent", up);
  }

  async screenshot(filePath) {
    mkdirSync(path.dirname(filePath), { recursive: true });
    await this.client.send("Emulation.setDeviceMetricsOverride", {
      width: VIEWPORT.width,
      height: VIEWPORT.height,
      deviceScaleFactor: 1,
      mobile: false
    });
    await sleep(80);
    const result = await this.client.send("Page.captureScreenshot", {
      format: "png",
      fromSurface: true,
      captureBeyondViewport: false
    });
    writeFileSync(filePath, Buffer.from(result.data, "base64"));
  }

  async close() {
    if (this.closed) return;
    this.closed = true;
    try { this.client.close(); } catch { /* socket already closed */ }
    killProcessTree(this.child, this.profileDir);
  }
}

// profileDir is the caller-owned folder under artifacts/. headed opens a visible window; the default is headless.
export async function launch({ profileDir, headed = false } = {}) {
  if (!profileDir) throw new Error("profileDir is required");
  mkdirSync(profileDir, { recursive: true });
  const browserPath = findBrowser();
  const launchArgs = [
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
    "--disable-dev-shm-usage",
    "--lang=ko",
    "--remote-debugging-port=0",
    `--user-data-dir=${profileDir}`,
    `--window-size=${VIEWPORT.width},${VIEWPORT.height}`,
    "about:blank"
  ];
  if (!headed) launchArgs.unshift("--headless=new");
  if (process.platform === "linux") launchArgs.splice(launchArgs.indexOf("--disable-dev-shm-usage"), 0, "--no-sandbox");
  const child = spawn(browserPath, launchArgs, { stdio: ["ignore", "ignore", "ignore"], windowsHide: true });
  try {
    const activePort = await waitForFile(path.join(profileDir, "DevToolsActivePort"));
    const debugPort = activePort.trim().split(/\r?\n/)[0];
    const target = await openTarget(debugPort);
    const client = await connectCdp(target.webSocketDebuggerUrl);
    await client.send("Page.enable");
    await client.send("Runtime.enable");
    await client.send("Emulation.setDeviceMetricsOverride", {
      width: VIEWPORT.width,
      height: VIEWPORT.height,
      deviceScaleFactor: 1,
      mobile: false
    });
    return new BrowserScenarioCdp(child, client, profileDir);
  } catch (error) {
    killProcessTree(child, profileDir);
    throw error;
  }
}

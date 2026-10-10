import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// ADR-0018: web/app.bundle.js (i18n.js first, like index.html) must evaluate and boot in a minimal DOM shaped
// after index.html without throwing, expose the window.PCV namespaces of src/modules.json and build its endpoint
// surface from pcv-config.js. This is the offline stand-in for a browser load until the train ships the bundle.

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const indexHtml = fs.readFileSync(webRoot + "index.html", "utf8");
const ids = [...new Set([...indexHtml.matchAll(/ id="([^"]+)"/g)].map((match) => match[1]))];
const order = JSON.parse(fs.readFileSync(webRoot + "src/modules.json", "utf8"));

class Element {
  constructor(tag, id) {
    this.tagName = String(tag || "div").toUpperCase();
    this.id = id || "";
    this.className = "";
    this.dataset = {};
    this.attributes = {};
    this.children = [];
    this.style = {};
    this.hidden = false;
    this.inert = false;
    this.open = false;
    this.value = "";
    this.disabled = false;
    this.textContent = "";
    this.innerHTML = "";
    this.listeners = new Map();
    this.classList = {
      add: (...names) => { for (const name of names) if (!this._classes().includes(name)) this.className = (this.className + " " + name).trim(); },
      remove: (...names) => { this.className = this._classes().filter((name) => !names.includes(name)).join(" "); },
      toggle: (name, force) => { const on = force === undefined ? !this._classes().includes(name) : !!force; on ? this.classList.add(name) : this.classList.remove(name); return on; },
      contains: (name) => this._classes().includes(name)
    };
  }
  _classes() { return this.className.split(/\s+/).filter(Boolean); }
  get isConnected() { return true; }
  get parentNode() { return null; }
  get firstChild() { return this.children[0] || null; }
  get offsetWidth() { return 100; }
  addEventListener(type, handler) { const list = this.listeners.get(type) || []; list.push(handler); this.listeners.set(type, list); }
  removeEventListener() {}
  dispatchEvent() { return true; }
  appendChild(child) { this.children.push(child); return child; }
  insertBefore(child) { this.children.unshift(child); return child; }
  removeChild(child) { this.children = this.children.filter((item) => item !== child); return child; }
  remove() {}
  replaceChildren() { this.children = []; }
  setAttribute(name, value) { this.attributes[name] = String(value); if (name === "hidden") this.hidden = true; if (name === "id") this.id = String(value); if (name === "class") this.className = String(value); }
  getAttribute(name) { return Object.prototype.hasOwnProperty.call(this.attributes, name) ? this.attributes[name] : null; }
  removeAttribute(name) { delete this.attributes[name]; if (name === "hidden") this.hidden = false; }
  hasAttribute(name) { return Object.prototype.hasOwnProperty.call(this.attributes, name); }
  querySelector() { return null; }
  querySelectorAll() { return []; }
  closest() { return null; }
  contains() { return false; }
  focus() {}
  blur() {}
  click() {}
  reset() { this.value = ""; }
  showModal() { this.open = true; }
  close() { this.open = false; }
  getBoundingClientRect() { return { left: 0, top: 0, right: 100, bottom: 100, width: 100, height: 100 }; }
  getClientRects() { return [{}]; }
  scrollIntoView() {}
  getContext() { return { beginPath() {}, arc() {}, fill() {}, fillText() {}, fillRect() {}, putImageData() {}, createImageData: (w, h) => ({ data: new Uint8ClampedArray(w * h * 4) }) }; }
  toDataURL() { return "data:image/png;base64,"; }
}

function storage() {
  const values = new Map();
  return {
    getItem: (key) => (values.has(key) ? values.get(key) : null),
    setItem: (key, value) => { values.set(key, String(value)); },
    removeItem: (key) => { values.delete(key); },
    clear: () => values.clear(),
    key: (index) => [...values.keys()][index] ?? null,
    get length() { return values.size; }
  };
}

function createSandbox() {
  const byId = new Map(ids.map((id) => [id, new Element("div", id)]));
  // #cb sections of index.html (one per route, BL-0019): the nav and the legacy renderActiveView() toggle their hidden flag.
  const views = [...indexHtml.matchAll(/<section id="([^"]+)" class="section app-view" data-view="([^"]+)"([^>]*)>/g)].map((match) => {
    const element = byId.get(match[1]);
    element.tagName = "SECTION";
    element.className = "section app-view";
    element.dataset.view = match[2];
    element.hidden = /\bhidden\b/.test(match[3]);
    return element;
  });
  const cb = byId.get("cb");
  if (cb) {
    cb.querySelectorAll = (selector) => (selector === ".app-view" ? views.slice() : []);
    cb.querySelector = (selector) => {
      const match = /^\.app-view\[data-view="([^"]+)"\]$/.exec(selector);
      return match ? views.find((view) => view.dataset.view === match[1]) || null : null;
    };
  }
  const body = new Element("body");
  const documentElement = new Element("html");
  documentElement.lang = "ko";
  const headEl = new Element("head");
  const listeners = new Map();
  const document = {
    body, documentElement, head: headEl, hidden: false, readyState: "complete", title: "", activeElement: null,
    getElementById: (id) => byId.get(id) || null,
    createElement: (tag) => new Element(tag),
    createElementNS: (ns, tag) => new Element(tag),
    createTextNode: (text) => ({ nodeType: 3, textContent: String(text) }),
    createDocumentFragment: () => new Element("fragment"),
    querySelector: () => null,
    querySelectorAll: (selector) => (selector === ".app-view" ? views.slice() : []),
    addEventListener: (type, handler) => { const list = listeners.get(type) || []; list.push(handler); listeners.set(type, list); },
    removeEventListener: () => {},
    dispatch: (type) => { for (const handler of listeners.get(type) || []) handler({ type }); }
  };
  const fetchLog = [];
  const sandbox = {
    console: { log() {}, warn() {}, error() {}, info() {}, debug() {} },
    document,
    localStorage: storage(),
    sessionStorage: storage(),
    location: { hash: "", hostname: "127.0.0.1", pathname: "/index.html", search: "", origin: "http://127.0.0.1", href: "http://127.0.0.1/index.html", replace() {} },
    history: { replaceState() {}, pushState() {} },
    navigator: { userAgent: "node", language: "ko", onLine: true },
    performance: { now: () => Date.now(), timing: { loadEventEnd: 0, navigationStart: 0 }, getEntriesByType: () => [] },
    matchMedia: () => ({ matches: false, addEventListener() {}, addListener() {}, removeEventListener() {} }),
    MutationObserver: class { observe() {} disconnect() {} },
    ResizeObserver: class { observe() {} disconnect() {} },
    requestAnimationFrame: (fn) => setTimeout(fn, 0),
    cancelAnimationFrame: (id) => clearTimeout(id),
    setTimeout: (fn, ms, ...args) => { const id = setTimeout(fn, ms, ...args); if (id && id.unref) id.unref(); return id; },
    clearTimeout,
    setInterval: (fn, ms, ...args) => { const id = setInterval(fn, ms, ...args); if (id && id.unref) id.unref(); return id; },
    clearInterval, queueMicrotask,
    AbortController, URL, URLSearchParams, TextEncoder, TextDecoder, Headers, Request, Response, Blob, Event, Promise, Date, Math, JSON, Number, String, Object, Array, Map, Set, RegExp, Error, TypeError, Intl, atob, btoa, encodeURIComponent, decodeURIComponent, structuredClone,
    Node: Element,
    HTMLElement: Element,
    Chart: class { constructor() {} destroy() {} update() {} },
    PCV_DESKTOP_NODE_CONFIG: Object.freeze({ apiBaseUrl: "http://127.0.0.1:7777" }),
    fetch: (url, init) => {
      fetchLog.push({ url: String(url), method: (init && init.method) || "GET" });
      return Promise.resolve(new Response(JSON.stringify({ ok: false, error: { code: "PCV_AUTH_REQUIRED", message: "fixture" } }), { status: 401, headers: { "Content-Type": "application/json" } }));
    },
    fetchLog
  };
  sandbox.window = sandbox;
  sandbox.self = sandbox;
  sandbox.globalThis = sandbox;
  sandbox.addEventListener = (type, handler) => { const list = listeners.get("window:" + type) || []; list.push(handler); listeners.set("window:" + type, list); };
  sandbox.removeEventListener = () => {};
  sandbox.dispatchEvent = () => true;
  sandbox.__dispatchWindow = (type) => { for (const handler of listeners.get("window:" + type) || []) handler({ type }); };
  sandbox.confirm = () => true;
  sandbox.alert = () => {};
  sandbox.open = () => null;
  sandbox.getComputedStyle = () => ({ visibility: "visible", getPropertyValue: () => "" });
  vm.createContext(sandbox);
  return { sandbox, document };
}

test("i18n.js and app.bundle.js evaluate in the index.html DOM and expose every module namespace", () => {
  const { sandbox } = createSandbox();
  vm.runInContext(fs.readFileSync(webRoot + "i18n.js", "utf8"), sandbox, { filename: "i18n.js" });
  vm.runInContext(fs.readFileSync(webRoot + "app.bundle.js", "utf8"), sandbox, { filename: "app.bundle.js" });
  const PCV = sandbox.PCV;
  assert.ok(PCV, "window.PCV");
  assert.equal(typeof sandbox.I18N.t, "function");
  assert.equal(sandbox._L("가", "a"), "가");
  const namespaces = { endpoints: "endpoints", api: "api", events: "events", ui: "ui", uxlib: "uxlib", metrics: "metrics", charts: "charts", theme: "theme", "modal-core": "modalCore", modal: "modal", nav: "nav", shell: "shell", mobile: "mobile", prefs: "prefs", help: "help", core: "core", "desktop-api": "desktopApi", vm: "vm", "vm-console": "vmConsole", ops: "ops", monitor: "monitor", accounts: "accounts", "filter-state": null };
  for (const name of order.modules) {
    const ns = namespaces[name];
    if (ns === null) continue;
    assert.ok(PCV[ns] && typeof PCV[ns] === "object", `PCV.${ns} missing for module ${name}`);
  }
  assert.ok(PCV.ui.filterState, "filter-state registers PCV.ui.filterState");
  assert.equal(sandbox.EP.VM_LIST(), "http://127.0.0.1:7777/api/v1/vms", "endpoint surface uses the configured API base");
  assert.equal(sandbox.EP.CHECKPOINT_SCHEDULE("pcv it/s3"), "http://127.0.0.1:7777/api/v1/vms/pcv%20it%2Fs3/checkpoints/schedule");
  assert.throws(() => sandbox.EP.VM_ACTION("x", "format-disk"), /PCV_ROUTE_ACTION_INVALID/);
  assert.deepEqual(JSON.parse(JSON.stringify(PCV.shell.NAV_SECTIONS().flatMap((section) => section.items.map((item) => item.id)))), ["dashboard", "vms", "network", "jobs", "activity", "evidence", "troubleshooting", "helppage"]);
  assert.equal(typeof sandbox.render, "function", "legacy render dispatcher exported");
  assert.equal(typeof sandbox.pcvEscapeHtml, "function", "legacy escapeHtml renamed away from the Single Edge global");
  assert.equal(sandbox.escapeHtml(null), "", "Single Edge escapeHtml stays the global");
  assert.equal(sandbox.pcvEscapeHtml(null), "-", "legacy escapeHtml keeps its dash fallback");
  assert.equal(PCV.events.isHealthy(), false);
  assert.equal(typeof PCV.bootstrap, "function");
  for (const fn of ["startVmConsoleFrame", "stopVmConsoleFrame", "pollVmConsoleFrame", "sendVmConsoleKeyboard", "handleVmGuestPreviewSubmit", "queueVmGuestExecutionControl", "queueVmGuestFile", "queueCheckpointScheduleControl", "queueVmExportImportControl", "showPrefs", "showCreate", "showRegisterModal", "showChangePwModal", "doLoginPage", "doLogout", "loadAll"]) {
    assert.equal(typeof sandbox[fn], "function", `global ${fn}`);
  }
  assert.ok(PCV.prefs && typeof PCV.prefs.show === "function", "PCV.prefs");
  assert.ok(PCV.help && typeof PCV.help.render === "function" && PCV.help.catalog().length === 7, "PCV.help");
});

test("the bootstrap binds the index.html elements, renders every panel and starts a loopback session", async () => {
  const { sandbox, document } = createSandbox();
  vm.runInContext(fs.readFileSync(webRoot + "i18n.js", "utf8"), sandbox, { filename: "i18n.js" });
  vm.runInContext(fs.readFileSync(webRoot + "app.bundle.js", "utf8"), sandbox, { filename: "app.bundle.js" });
  sandbox._DEBUG = true;
  const warnings = [];
  sandbox.console.warn = (...args) => warnings.push(args.map(String).join(" "));
  document.dispatch("DOMContentLoaded");
  await new Promise((resolve) => setTimeout(resolve, 50));
  assert.ok(sandbox.els && sandbox.els.vmTable, "els bound to the index.html content ids");
  assert.equal(sandbox.els.connectionForm, null, "shell-only legacy elements are absent and tolerated");
  assert.match(sandbox.els.vmTable.innerHTML, /No VMs|muted/, "VM table rendered an empty state");
  assert.ok(sandbox.fetchLog.some((entry) => entry.url.endsWith("/api/v1/auth/loopback-session") && entry.method === "POST"), "loopback session requested on a loopback host");
  const fatal = warnings.filter((line) => /bootstrap:init|bootstrap:shell|render:/.test(line) && !/render:renderConnectionState|render:renderStatusBar|render:renderAssetStatus|render:renderWorkspaceTabs|render:renderVmAssetList|render:renderHeroChips|render:applyUiPreferences/.test(line));
  assert.deepEqual(fatal, [], "no unexpected bootstrap or render failures");
});

test("the Single Edge nav keeps the chosen section shown through sidebar, hash and the legacy re-render (BL-0019)", async () => {
  const { sandbox, document } = createSandbox();
  vm.runInContext(fs.readFileSync(webRoot + "i18n.js", "utf8"), sandbox, { filename: "i18n.js" });
  vm.runInContext(fs.readFileSync(webRoot + "app.bundle.js", "utf8"), sandbox, { filename: "app.bundle.js" });
  document.dispatch("DOMContentLoaded");
  await new Promise((resolve) => setTimeout(resolve, 50));
  const shown = () => document.querySelectorAll(".app-view").filter((section) => !section.hidden).map((section) => section.dataset.view);
  assert.equal(document.querySelectorAll(".app-view").length, 8, "seven legacy views plus the help section");
  const troubleshooting = document.getElementById("troubleshooting");
  const marker = { nodeType: 1, marker: "legacy-troubleshooting-panel" };
  troubleshooting.appendChild(marker);
  for (const view of ["dashboard", "vms", "network", "jobs", "activity", "evidence", "troubleshooting", "helppage"]) {
    sandbox.navigateTo(view);
    assert.deepEqual(shown(), [view], `${view} after the sidebar navigation`);
    sandbox.render();
    assert.deepEqual(shown(), [view], `${view} after the legacy re-render`);
    assert.equal(sandbox.PCV.nav.activeView(), view);
    if (view !== "helppage") assert.equal(sandbox.state.activeView, view, `legacy view state follows ${view}`);
  }
  assert.ok(document.getElementById("helppage").children.length > 0, "help rendered into its own section");
  assert.ok(troubleshooting.children.includes(marker), "help no longer clears the troubleshooting section");
  for (const [hash, view] of [["#/jobs", "jobs"], ["#network", "network"], ["#/vms/pcv-it-x", "vms"]]) {
    sandbox.location.hash = hash;
    sandbox.__dispatchWindow("hashchange");
    await new Promise((resolve) => setTimeout(resolve, 20));
    assert.deepEqual(shown(), [view], `${hash} shows ${view}`);
    assert.equal(sandbox.state.activeView, view, `${hash} legacy view state`);
  }
});

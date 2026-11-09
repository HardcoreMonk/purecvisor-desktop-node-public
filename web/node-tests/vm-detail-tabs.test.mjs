import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 3 (ui-redesign-20261105): VM detail tabs, header menu, clone modal, and confirm dialogs.
// The harness is a small DOM because these node tests do not start a browser.

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const vmSourcePath = fileURLToPath(new URL("../src/modules/vm.ts", import.meta.url));
const VOID_TAGS = new Set(["AREA", "BASE", "BR", "COL", "EMBED", "HR", "IMG", "INPUT", "LINK", "META", "SOURCE", "TRACK", "WBR"]);

class Node {
  constructor() {
    this.parentNode = null;
    this.childNodes = [];
    this.nodeType = 0;
  }
  get nextSibling() {
    if (!this.parentNode) return null;
    const index = this.parentNode.childNodes.indexOf(this);
    return index < 0 ? null : this.parentNode.childNodes[index + 1] || null;
  }
  get isConnected() {
    let current = this;
    while (current) {
      if (current.nodeType === 9) return true;
      current = current.parentNode;
    }
    return false;
  }
}

class TextNode extends Node {
  constructor(text) {
    super();
    this.nodeType = 3;
    this.textContent = String(text);
  }
}

class Element extends Node {
  constructor(tag) {
    super();
    this.nodeType = 1;
    this.tagName = String(tag || "div").toUpperCase();
    this.attributes = new Map();
    this.listeners = new Map();
    this.style = {};
    this._className = "";
    this._id = "";
    this._disabled = false;
    this._hidden = false;
    this._value = "";
    this._tabIndex = 0;
    this.open = false;
    this.classList = {
      add: (...names) => names.forEach((name) => {
        if (!this._classes().includes(name)) this._className = `${this._className} ${name}`.trim();
      }),
      remove: (...names) => { this._className = this._classes().filter((name) => !names.includes(name)).join(" "); },
      contains: (name) => this._classes().includes(name),
      toggle: (name, force) => {
        const on = force === undefined ? !this._classes().includes(name) : !!force;
        if (on) this.classList.add(name);
        else this.classList.remove(name);
        return on;
      }
    };
  }
  _classes() { return this._className.split(/\s+/).filter(Boolean); }
  get className() { return this._className; }
  set className(value) { this._className = String(value || ""); }
  get id() { return this._id; }
  set id(value) { this.setAttribute("id", value); }
  get disabled() { return this._disabled; }
  set disabled(value) {
    this._disabled = !!value;
    if (this._disabled) this.attributes.set("disabled", "");
    else this.attributes.delete("disabled");
  }
  get hidden() { return this._hidden; }
  set hidden(value) {
    this._hidden = !!value;
    if (this._hidden) this.attributes.set("hidden", "");
    else this.attributes.delete("hidden");
  }
  get value() { return this._value; }
  set value(value) { this._value = String(value); }
  get tabIndex() { return this._tabIndex; }
  set tabIndex(value) { this._tabIndex = Number(value); this.setAttribute("tabindex", String(value)); }
  get textContent() {
    if (this.childNodes.length === 0) return "";
    return this.childNodes.map((child) => (child.nodeType === 3 ? child.textContent : child.textContent || "")).join("");
  }
  set textContent(value) {
    this.childNodes = [];
    this.appendChild(new TextNode(value));
  }
  get innerHTML() { return ""; }
  set innerHTML(value) {
    this.childNodes = [];
    const frag = parseHtml(String(value ?? ""), this.ownerDocument);
    while (frag.childNodes.length) this.appendChild(frag.childNodes[0]);
  }
  get firstChild() { return this.childNodes[0] || null; }
  setAttribute(name, value) {
    const text = String(value);
    this.attributes.set(name, text);
    if (name === "id") this._id = text;
    if (name === "class") this._className = text;
    if (name === "disabled") this._disabled = true;
    if (name === "hidden") this._hidden = true;
    if (name === "value") this._value = text;
    if (name === "tabindex") this._tabIndex = Number(text);
  }
  getAttribute(name) { return this.attributes.has(name) ? this.attributes.get(name) : null; }
  hasAttribute(name) { return this.attributes.has(name); }
  removeAttribute(name) {
    this.attributes.delete(name);
    if (name === "disabled") this._disabled = false;
    if (name === "hidden") this._hidden = false;
  }
  addEventListener(type, handler) {
    const list = this.listeners.get(type) || [];
    list.push(handler);
    this.listeners.set(type, list);
  }
  removeEventListener() {}
  dispatchEvent(event) {
    if (!event.target) event.target = this;
    const chain = [];
    let current = this;
    while (current) {
      chain.push(current);
      current = current.parentNode;
    }
    for (const node of chain) {
      event.currentTarget = node;
      const list = node.listeners && node.listeners.get ? node.listeners.get(event.type) : null;
      for (const handler of [...(list || [])]) handler(event);
      if (event.cancelBubble) break;
    }
    return true;
  }
  appendChild(child) {
    if (child instanceof Fragment) {
      while (child.childNodes.length) this.appendChild(child.childNodes[0]);
      return child;
    }
    if (child.parentNode) child.parentNode.removeChild(child);
    child.parentNode = this;
    if (!child.ownerDocument && this.ownerDocument) child.ownerDocument = this.ownerDocument;
    this.childNodes.push(child);
    return child;
  }
  insertBefore(child, reference) {
    if (child.parentNode) child.parentNode.removeChild(child);
    child.parentNode = this;
    if (!child.ownerDocument && this.ownerDocument) child.ownerDocument = this.ownerDocument;
    const index = reference ? this.childNodes.indexOf(reference) : -1;
    if (index < 0) this.childNodes.push(child);
    else this.childNodes.splice(index, 0, child);
    return child;
  }
  removeChild(child) {
    this.childNodes = this.childNodes.filter((item) => item !== child);
    child.parentNode = null;
    return child;
  }
  remove() { if (this.parentNode) this.parentNode.removeChild(this); }
  focus() { if (!this.disabled && this.ownerDocument) this.ownerDocument.activeElement = this; }
  click() { this.dispatchEvent({ type: "click", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } }); }
  showModal() { this.open = true; }
  close() { this.open = false; this.dispatchEvent({ type: "close", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } }); }
  getBoundingClientRect() { return { left: 0, top: 0, right: 200, bottom: 120, width: 200, height: 120 }; }
  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
  querySelectorAll(selector) {
    const matchers = selector.split(",").map((part) => part.trim()).filter(Boolean);
    const found = [];
    walk(this, (element) => {
      if (element !== this && matchers.some((part) => matches(element, part))) found.push(element);
    });
    return found;
  }
}

class Fragment extends Node {
  constructor() {
    super();
    this.nodeType = 11;
  }
  appendChild(child) {
    if (child instanceof Fragment) {
      while (child.childNodes.length) this.appendChild(child.childNodes[0]);
      return child;
    }
    if (child.parentNode) child.parentNode.removeChild(child);
    child.parentNode = this;
    if (!child.ownerDocument && this.ownerDocument) child.ownerDocument = this.ownerDocument;
    this.childNodes.push(child);
    return child;
  }
  removeChild(child) {
    this.childNodes = this.childNodes.filter((item) => item !== child);
    child.parentNode = null;
    return child;
  }
}

function walk(node, visit) {
  for (const child of node.childNodes || []) {
    if (child.nodeType === 1) {
      visit(child);
      walk(child, visit);
    }
  }
}

function matches(element, selector) {
  let rest = selector;
  const tag = /^[A-Za-z][\w-]*/.exec(rest);
  if (tag) {
    if (element.tagName !== tag[0].toUpperCase()) return false;
    rest = rest.slice(tag[0].length);
  }
  while (rest) {
    if (rest[0] === ".") {
      const cls = /^\.([^\s#.[\]]+)/.exec(rest);
      if (!cls || !element.classList.contains(cls[1])) return false;
      rest = rest.slice(cls[0].length);
    } else if (rest[0] === "#") {
      const id = /^#([^\s#.[\]]+)/.exec(rest);
      if (!id || element.id !== id[1]) return false;
      rest = rest.slice(id[0].length);
    } else if (rest[0] === "[") {
      const attr = /^\[([^\]=]+)(?:="([^"]*)")?\]/.exec(rest);
      if (!attr || !element.hasAttribute(attr[1])) return false;
      if (attr[2] !== undefined && element.getAttribute(attr[1]) !== attr[2]) return false;
      rest = rest.slice(attr[0].length);
    } else return false;
  }
  return true;
}

function decodeEntities(text) {
  return String(text)
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&quot;/g, "\"")
    .replace(/&#39;/g, "'")
    .replace(/&amp;/g, "&");
}

function parseAttrs(element, raw) {
  const re = /([:@A-Za-z_][\w:.-]*)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s"'>]+)))?/g;
  let match;
  while ((match = re.exec(raw))) {
    const value = match[2] ?? match[3] ?? match[4] ?? "";
    element.setAttribute(match[1], decodeEntities(value));
  }
}

function parseHtml(html, owner) {
  const root = new Fragment();
  root.ownerDocument = owner;
  const stack = [root];
  const re = /<!--[\s\S]*?-->|<\/([A-Za-z][\w:-]*)>|<([A-Za-z][\w:-]*)([^>]*)>/g;
  let match;
  let cursor = 0;
  while ((match = re.exec(html))) {
    if (match.index > cursor) {
      const text = decodeEntities(html.slice(cursor, match.index));
      if (text) stack[stack.length - 1].appendChild(owner.createTextNode(text));
    }
    cursor = match.index + match[0].length;
    if (match[1]) {
      const name = match[1].toUpperCase();
      for (let index = stack.length - 1; index > 0; index -= 1) {
        if (stack[index].tagName === name) {
          stack.length = index;
          break;
        }
      }
      continue;
    }
    const element = owner.createElement(match[2]);
    parseAttrs(element, match[3] || "");
    stack[stack.length - 1].appendChild(element);
    const selfClosing = /\/\s*$/.test(match[3] || "") || VOID_TAGS.has(element.tagName);
    if (!selfClosing) stack.push(element);
  }
  if (cursor < html.length) {
    const text = decodeEntities(html.slice(cursor));
    if (text) stack[stack.length - 1].appendChild(owner.createTextNode(text));
  }
  return root;
}

function storage() {
  const values = new Map();
  return {
    getItem: (key) => (values.has(key) ? values.get(key) : null),
    setItem: (key, value) => { values.set(key, String(value)); },
    removeItem: (key) => { values.delete(key); },
    clear: () => values.clear()
  };
}

function createEnv() {
  const document = {
    nodeType: 9,
    activeElement: null,
    hidden: false,
    title: "",
    readyState: "complete",
    listeners: new Map(),
    createElement: (tag) => {
      const node = new Element(tag);
      node.ownerDocument = document;
      return node;
    },
    createElementNS: (_ns, tag) => {
      const node = new Element(tag);
      node.ownerDocument = document;
      return node;
    },
    createTextNode: (text) => {
      const node = new TextNode(text);
      node.ownerDocument = document;
      return node;
    },
    createDocumentFragment: () => {
      const node = new Fragment();
      node.ownerDocument = document;
      return node;
    },
    addEventListener(type, handler) {
      const list = this.listeners.get(type) || [];
      list.push(handler);
      this.listeners.set(type, list);
    },
    removeEventListener() {},
    querySelector(selector) { return this.documentElement.querySelector(selector); },
    querySelectorAll(selector) { return this.documentElement.querySelectorAll(selector); },
    getElementById(id) {
      if (this.documentElement.id === id) return this.documentElement;
      return walkFind(this.documentElement, (element) => element.id === id);
    }
  };
  const html = new Element("html");
  const body = new Element("body");
  html.ownerDocument = document;
  body.ownerDocument = document;
  html.parentNode = document;
  html.appendChild(body);
  document.documentElement = html;
  document.body = body;
  document.activeElement = body;
  const sandbox = {
    console: { log() {}, warn() {}, error() {}, info() {}, debug() {} },
    document,
    localStorage: storage(),
    sessionStorage: storage(),
    location: { hash: "", hostname: "127.0.0.1" },
    navigator: { language: "ko", userAgent: "node" },
    performance: { now: () => Date.now() },
    matchMedia: () => ({ matches: false, addEventListener() {}, removeEventListener() {} }),
    setTimeout,
    clearTimeout,
    queueMicrotask,
    Promise,
    Date,
    Math,
    JSON,
    Number,
    String,
    Object,
    Array,
    Map,
    Set,
    RegExp,
    Error,
    TypeError,
    Intl,
    Node,
    Element
  };
  sandbox.window = sandbox;
  sandbox.globalThis = sandbox;
  sandbox.addEventListener = () => {};
  sandbox.removeEventListener = () => {};
  vm.createContext(sandbox);
  for (const file of ["i18n.js", "src/modules/uxlib.ts", "src/modules/modal-core.ts", "src/modules/components.ts", "src/modules/vm.ts"]) {
    vm.runInContext(fs.readFileSync(webRoot + file, "utf8"), sandbox, { filename: file });
  }
  return sandbox;
}

function walkFind(node, predicate) {
  for (const child of node.childNodes || []) {
    if (child.nodeType !== 1) continue;
    if (predicate(child)) return child;
    const nested = walkFind(child, predicate);
    if (nested) return nested;
  }
  return null;
}

function fire(target, type, extra = {}) {
  target.dispatchEvent({
    type,
    ...extra,
    target,
    preventDefault() {},
    stopPropagation() { this.cancelBubble = true; }
  });
}

function install(sandbox, row, opts = {}) {
  const deny = new Set(opts.deny || []);
  sandbox.state = {
    selectedVm: row,
    selectedVmId: row.id,
    selectedVmCheckpoints: [],
    selectedVmReadbacks: null,
    selectedVmQosControl: null,
    pendingVmActions: {},
    serverJobs: opts.serverJobs || [],
    trackedJobs: opts.trackedJobs || [],
    vms: opts.vms || [row],
    error: null,
    actionPending: false
  };
  sandbox.els = {
    vmDetailTitle: sandbox.document.createElement("h2"),
    vmDetailContent: sandbox.document.createElement("div"),
    vmDetailPanel: sandbox.document.createElement("div")
  };
  sandbox.document.body.appendChild(sandbox.els.vmDetailPanel);
  sandbox.els.vmDetailPanel.appendChild(sandbox.els.vmDetailContent);
  sandbox.getVmId = (item) => String((item && (item.id || item.name)) || "");
  sandbox.getVmName = (item) => String((item && (item.name || item.id)) || "Unknown VM");
  sandbox.getVmState = (item) => String((item && (item.state || item.status)) || "").trim();
  sandbox.isTemplateLockedVm = (item) => Boolean(item && item.template_lock);
  sandbox.isRunningVmState = (value) => String(value || "").toLowerCase().includes("running");
  sandbox.rbacAllows = (permission) => !deny.has(permission);
  sandbox.isVmActionPending = () => Boolean(opts.pending);
  sandbox.isCheckpointActionPending = () => false;
  sandbox.getVmActionKey = (id) => String(id || "");
  sandbox.flattenNamedList = () => "";
  sandbox.formatConsoleValue = () => "";
  sandbox.formatObjectValue = (value) => (value == null ? "" : String(value));
  sandbox.pcvEscapeHtml = (value) => String(value ?? "").replace(/[&<>"']/g, (ch) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[ch]));
  sandbox.stateBadge = (value) => `<span class="badge badge-ok">${sandbox.pcvEscapeHtml(value)}</span>`;
  sandbox.renderSwitchOptions = () => "";
  sandbox.renderVmConsoleFrameCard = () => "<div class=\"vm-console-card\"></div>";
  sandbox.renderExportImportPreview = () => "";
  sandbox.renderCheckpointSchedulePreview = () => "";
  sandbox.asArray = (value) => (Array.isArray(value) ? value : []);
  sandbox.asObject = (value) => (value && typeof value === "object" && !Array.isArray(value) ? value : {});
  sandbox.readNested = () => ({});
  sandbox.requireRbac = () => {};
  sandbox.findCachedVm = (id) => (sandbox.state.vms || []).find((item) => item.id === id || item.name === id) || null;
  sandbox.setVmActionPending = () => {};
  sandbox.clearVmActionPending = () => {};
  sandbox.pcvTrackJob = () => {};
  sandbox.startPolling = () => {};
  sandbox.loadVms = async () => {};
  sandbox.refreshSelectedVm = async () => {};
  sandbox.render = () => {};
  sandbox.normalizeError = (error) => {
    if (error && error.normalized) return error;
    return { normalized: true, code: (error && error.code) || "", message: (error && error.message) || "", detail: (error && error.detail) || "" };
  };
  sandbox.desktopApi = Object.assign({
    previewVmClone: async () => ({ planned_copy_bytes: 1 }),
    queueVmClone: async () => ({ job_id: "job-1", status: "queued" }),
    getJob: async () => ({ job_id: "job-1", status: "succeeded" }),
    deleteVm: async () => ({ job_id: "job-del", status: "queued" })
  }, opts.desktopApi || {});
  sandbox.PCV.desktopApi = sandbox.desktopApi;
}

function buttonByAction(root, action) {
  return root.querySelector(`[data-action="${action}"]`);
}

function visiblePanels(root) {
  return root.querySelectorAll('[role="tabpanel"]').filter((panel) => !panel.hidden);
}

const alpha = { id: "id-1", name: "alpha", state: "Off", checkpoints: { count: 2 } };

test("vm detail shows six tabs and only the selected panel", () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.renderVmDetail();
  const root = sandbox.els.vmDetailContent;
  const tabs = root.querySelectorAll('[role="tab"]');
  assert.deepEqual(tabs.map((tab) => tab.textContent), ["요약", "콘솔", "체크포인트", "하드웨어", "게스트", "작업 기록"]);
  assert.equal(visiblePanels(root).length, 1);
  assert.equal(visiblePanels(root)[0].querySelector('[data-pcv-tab-source="summary"]') != null, true);
  const summary = root.querySelector('[data-pcv-tab-source="summary"]');
  const hardware = root.querySelector('[data-pcv-tab-source="hardware"]');
  const guest = root.querySelector('[data-pcv-tab-source="guest"]');
  assert.equal(summary.querySelector(".detail-grid") != null, true);
  assert.equal(summary.querySelector(".qos-readback-panel") != null, true);
  assert.equal(summary.querySelector('form[data-action="vm-export"]') != null, true);
  assert.equal(root.querySelector('[data-pcv-tab-source="console"]').querySelector(".vm-console-card") != null, true);
  assert.equal(root.querySelector('[data-pcv-tab-source="checkpoints"]').querySelector(".checkpoint-panel") != null, true);
  assert.equal(hardware.querySelector('form[data-action="vm-set-memory"]') != null, true);
  assert.equal(hardware.querySelector(".pcv-hw-help") != null, true);
  assert.equal(root.querySelector('form[data-action="vm-clone"]'), null);
  assert.equal(root.querySelector('[data-action="vm-clone"]'), null);
  assert.equal(guest.querySelector(".qos-control-panel") != null, true);
  assert.equal(summary.querySelector(".qos-control-panel"), null);
  assert.match(root.querySelector(".pcv-vm-crumb").textContent, /가상 머신/);
  assert.equal(root.querySelector(".pcv-vm-crumb").textContent.includes("alpha"), false);
  assert.match(root.querySelector(".pcv-vm-name").textContent, /alpha/);
  assert.match(root.querySelector(".pcv-vm-badges").textContent, /Off/);
  assert.match(root.querySelector(".pcv-vm-badges").innerHTML || root.querySelector(".badge").className, /badge/);
  tabs[3].click();
  const visible = visiblePanels(root);
  assert.equal(visible.length, 1);
  assert.equal(visible[0].querySelector('[data-pcv-tab-source="hardware"]') != null, true);
  assert.equal(root.querySelector("[data-pcv-recent-jobs]").querySelector(".pcv-empty") != null, true);
});

test("open console switches to the console tab", () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.renderVmDetail();
  const root = sandbox.els.vmDetailContent;
  buttonByAction(root, "vm-open-console-tab").click();
  const visible = visiblePanels(root);
  assert.equal(visible.length, 1);
  assert.equal(visible[0].querySelector('[data-pcv-tab-source="console"]') != null, true);
  assert.equal(root.querySelector('[role="tab"][aria-selected="true"]').textContent, "콘솔");
});

test("more menu items follow state, permission, and template lock", () => {
  const sandbox = createEnv();
  function menu(row, opts) {
    install(sandbox, row, opts);
    sandbox.renderVmDetail();
    const root = sandbox.els.vmDetailContent;
    const item = (action) => root.querySelectorAll(`[data-action="${action}"]`).find((node) => node.getAttribute("role") === "menuitem") || null;
    const headerClone = root.querySelectorAll('[data-action="vm-clone-open"]').find((node) => node.getAttribute("role") !== "menuitem") || null;
    return { root, item, headerClone };
  }
  const off = menu(alpha);
  assert.equal(off.item("vm-clone-open").disabled, false);
  assert.equal(off.item("vm-template-lock").disabled, false);
  assert.equal(off.item("vm-template-unlock"), null);
  assert.equal(off.item("vm-export-focus").disabled, false);
  assert.equal(off.item("vm-console").disabled, false);
  assert.equal(off.item("vm-poweroff").disabled, false);
  assert.equal(off.item("vm-delete").disabled, false);
  assert.equal(off.headerClone, null);
  assert.equal(buttonByAction(off.root, "vm-start").disabled, false);
  assert.match(off.item("vm-delete").className, /pcv-menu-danger/);
  assert.equal(off.root.querySelector('[aria-haspopup="menu"]').getAttribute("aria-haspopup"), "menu");

  const locked = menu({ ...alpha, state: "Running", template_lock: true });
  assert.equal(locked.item("vm-clone-open").disabled, false);
  assert.equal(locked.item("vm-template-unlock").disabled, false);
  assert.equal(locked.item("vm-template-lock"), null);
  assert.equal(locked.item("vm-export-focus").disabled, false);
  assert.equal(locked.item("vm-console").disabled, false);
  assert.equal(locked.item("vm-poweroff").disabled, true);
  assert.equal(locked.item("vm-delete").disabled, true);
  assert.equal(locked.headerClone.disabled, false);
  assert.equal(buttonByAction(locked.root, "vm-shutdown").disabled, true);
  assert.equal(buttonByAction(locked.root, "vm-start"), null);

  const blocked = menu(alpha, { deny: ["operate", "console.view"] });
  assert.equal(blocked.item("vm-clone-open").disabled, true);
  assert.equal(blocked.item("vm-template-lock").disabled, true);
  assert.equal(blocked.item("vm-export-focus").disabled, true);
  assert.equal(blocked.item("vm-console").disabled, true);
  assert.equal(blocked.item("vm-poweroff").disabled, true);
  assert.equal(blocked.item("vm-delete").disabled, true);
  assert.equal(buttonByAction(blocked.root, "vm-open-console-tab").disabled, true);

  const pending = menu({ ...alpha, state: "Running" }, { pending: true });
  assert.equal(pending.item("vm-clone-open").disabled, true);
  assert.equal(pending.item("vm-delete").disabled, true);
  assert.equal(buttonByAction(pending.root, "vm-shutdown").disabled, true);

  const running = menu({ ...alpha, state: "Running" });
  assert.equal(running.item("vm-delete").disabled, false);
  assert.equal(buttonByAction(running.root, "vm-shutdown").disabled, false);
});

test("more menu moves with arrows and closes on Escape and an outside click", () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.renderVmDetail();
  const root = sandbox.els.vmDetailContent;
  const toggle = root.querySelector('[data-action="vm-more-toggle"]');
  const menu = root.querySelector('[role="menu"]');
  assert.equal(menu.hidden, true);
  toggle.click();
  assert.equal(menu.hidden, false);
  assert.equal(toggle.getAttribute("aria-expanded"), "true");
  const items = menu.querySelectorAll('[role="menuitem"]').filter((item) => !item.disabled);
  assert.equal(sandbox.document.activeElement, items[0]);
  fire(items[0], "keydown", { key: "ArrowDown" });
  assert.equal(sandbox.document.activeElement, items[1]);
  fire(items[1], "keydown", { key: "Escape" });
  assert.equal(menu.hidden, true);
  assert.equal(sandbox.document.activeElement, toggle);
  toggle.click();
  assert.equal(menu.hidden, false);
  sandbox.document.body.click();
  assert.equal(menu.hidden, true);
  assert.equal(toggle.getAttribute("aria-expanded"), "false");
});

test("clone modal blocks invalid names and sends preview before clone", async () => {
  const sandbox = createEnv();
  const previews = [];
  const queued = [];
  let releasePreview = null;
  install(sandbox, alpha, {
    vms: [alpha, { id: "id-2", name: "beta", state: "Off" }],
    desktopApi: {
      previewVmClone: (id, payload) => {
        previews.push({ id, payload });
        return new Promise((resolve) => { releasePreview = resolve; });
      },
      queueVmClone: async (id, payload) => {
        queued.push({ id, payload });
        return { job_id: "job-1", status: "queued" };
      },
      getJob: async () => ({ job_id: "job-1", status: "succeeded" })
    }
  });
  assert.equal(sandbox.suggestCloneName("alpha", ["alpha", "alpha-clone"]), "alpha-clone-2");
  sandbox.openVmCloneModal("id-1");
  const modal = sandbox.document.body.querySelector(".pcv-clone-modal");
  const input = modal.querySelector('[name="name"]');
  const preview = [...modal.childNodes].find((node) => node.textContent === "미리보기");
  const start = [...modal.childNodes].find((node) => node.textContent === "복제 시작");
  assert.equal(modal.querySelector("h2").textContent, "VM 복제");
  assert.equal(input.value, "alpha-clone");
  assert.equal(start.disabled, true);
  for (const bad of ["   ", "bad/name", "alpha", "beta"]) {
    input.value = bad;
    fire(input, "input");
    assert.equal(preview.disabled, true, bad);
    preview.click();
    assert.equal(previews.length, 0, bad);
  }
  input.value = "alpha-clone";
  fire(input, "input");
  assert.equal(preview.disabled, false);
  assert.equal(start.disabled, true);
  preview.click();
  assert.equal(previews.length, 1);
  assert.equal(previews[0].id, "id-1");
  assert.equal(previews[0].payload.confirm_name, "id-1");
  assert.equal(previews[0].payload.name, "alpha-clone");
  assert.equal(previews[0].payload.vm_root, undefined);
  assert.equal(start.disabled, true);
  releasePreview({ planned_copy_bytes: 12 });
  await new Promise((resolve) => setTimeout(resolve, 0));
  assert.equal(start.disabled, false);
  assert.match(modal.querySelector(".pcv-clone-preview").textContent, /12/);
  start.click();
  await new Promise((resolve) => setTimeout(resolve, 20));
  assert.equal(queued.length, 1);
  assert.equal(queued[0].id, "id-1");
  assert.equal(queued[0].payload.confirm_name, "id-1");
  assert.equal(queued[0].payload.name, "alpha-clone");
  assert.equal(queued[0].payload.vm_root, undefined);
  assert.equal(modal.querySelector(".pcv-clone-job").querySelector("a").textContent, "새 VM 열기");
});

test("BL-0031 clone preview shows KB MB or GB instead of raw bytes", async () => {
  const sandbox = createEnv();
  let releasePreview = null;
  install(sandbox, alpha, {
    desktopApi: {
      previewVmClone: () => new Promise((resolve) => { releasePreview = resolve; })
    }
  });
  sandbox.openVmCloneModal("id-1");
  const modal = sandbox.document.body.querySelector(".pcv-clone-modal");
  const preview = [...modal.childNodes].find((node) => node.textContent === "미리보기");
  preview.click();
  releasePreview({ planned_copy_bytes: 1048576 });
  await new Promise((resolve) => setTimeout(resolve, 0));
  const text = modal.querySelector(".pcv-clone-preview").textContent;
  assert.match(text, /1\.0 MB/);
  assert.equal(text.includes("1048576"), false);
  assert.equal(text.includes("바이트"), false);
});

test("BL-0031 delete message omits the checkpoint clause when the count is zero", async () => {
  const sandbox = createEnv();
  install(sandbox, { id: "id-1", name: "alpha", state: "Off", checkpoints: { count: 0 } });
  const pending = sandbox.queueVmDelete("id-1");
  const dialog = sandbox.document.body.querySelector('[role="dialog"]');
  assert.match(dialog.textContent, /alpha/);
  assert.equal(dialog.textContent.includes("checkpoint"), false);
  assert.equal(dialog.textContent.includes("0개"), false);
  dialog.querySelector('[data-pcv-confirm="cancel"]').click();
  await pending;
});

test("changing the clone name disables start until preview succeeds again", async () => {
  const sandbox = createEnv();
  const queued = [];
  install(sandbox, alpha, {
    desktopApi: {
      previewVmClone: async () => ({ planned_copy_bytes: 4 }),
      queueVmClone: async (id, payload) => {
        queued.push({ id, payload });
        return { job_id: "job-2", status: "queued" };
      }
    }
  });
  sandbox.openVmCloneModal("id-1");
  const modal = sandbox.document.body.querySelector(".pcv-clone-modal");
  const input = modal.querySelector('[name="name"]');
  const preview = [...modal.childNodes].find((node) => node.textContent === "미리보기");
  const start = [...modal.childNodes].find((node) => node.textContent === "복제 시작");
  preview.click();
  await new Promise((resolve) => setTimeout(resolve, 0));
  assert.equal(start.disabled, false);
  input.value = "alpha-clone-2";
  fire(input, "input");
  assert.equal(start.disabled, true);
  start.click();
  await new Promise((resolve) => setTimeout(resolve, 0));
  assert.deepEqual(queued, []);
});

test("delete confirmation stays disabled until the typed name matches", async () => {
  const sandbox = createEnv();
  const deleted = [];
  install(sandbox, alpha, {
    desktopApi: {
      deleteVm: async (id) => {
        deleted.push(id);
        return { job_id: "job-del", status: "queued" };
      }
    }
  });
  const pending = sandbox.queueVmDelete("id-1");
  const dialog = sandbox.document.body.querySelector('[role="dialog"]');
  const accept = dialog.querySelector('[data-pcv-confirm="accept"]');
  const input = dialog.querySelector("input");
  assert.equal(dialog.classList.contains("pcv-confirm"), true);
  assert.match(dialog.textContent, /alpha/);
  assert.match(dialog.textContent, /2/);
  assert.equal(accept.disabled, true);
  input.value = "alpha ";
  fire(input, "input");
  assert.equal(accept.disabled, true);
  input.value = "alpha";
  fire(input, "input");
  assert.equal(accept.disabled, false);
  accept.click();
  await pending;
  assert.equal(deleted.length, 1);
  assert.equal(deleted[0], "id-1");
});

test("jobs tab lists only this VM", () => {
  const sandbox = createEnv();
  install(sandbox, alpha, {
    serverJobs: [
      { job_id: "job-a", status: "queued", params: { name: "alpha" } },
      { job_id: "job-b", status: "running", params: { name: "other" } }
    ],
    trackedJobs: [
      { job_id: "job-a", status: "queued", params: { name: "alpha" } },
      { job_id: "job-c", status: "failed", params: { vm_name: "alpha" } }
    ]
  });
  sandbox.renderVmDetail();
  const root = sandbox.els.vmDetailContent;
  const recent = root.querySelector("[data-pcv-recent-jobs]");
  const jobs = root.querySelector('[data-pcv-tab-source="jobs"]');
  assert.equal(recent.querySelectorAll(".pcv-job").length, 2);
  assert.equal(recent.querySelector('[data-action="vm-open-jobs-tab"]').textContent, "작업 기록 열기");
  assert.equal(jobs.querySelectorAll(".pcv-job").map((node) => node.getAttribute("data-phase")).join(","), "queued,failed");
  assert.equal(jobs.querySelectorAll(".pcv-job").some((node) => node.getAttribute("data-phase") === "running"), false);
});

test("header and more-menu confirms do not call window.confirm", () => {
  const source = fs.readFileSync(vmSourcePath, "utf8");
  function sliceFn(name) {
    const start = source.indexOf(`async function ${name}`);
    assert.notEqual(start, -1, name);
    const rest = source.slice(start + 1);
    const next = rest.search(/\nasync function |\n\/\/ exports/);
    return next < 0 ? source.slice(start) : source.slice(start, start + 1 + next);
  }
  ["queueVmLifecycle", "queueVmTemplateLock", "queueVmClone", "queueVmDelete", "queueCheckpointRestore", "queueCheckpointDelete"].forEach((name) => {
    assert.equal(sliceFn(name).includes("window.confirm"), false, name);
  });
  ["queueVmAttach", "queueVmQosDirectControl", "queueVmGuestExecutionControl", "queueVmManage", "queueVmGuestFile"].forEach((name) => {
    assert.equal(sliceFn(name).includes("window.confirm"), false, name);
  });
});

test("prefix VM names do not share jobs", () => {
  const sandbox = createEnv();
  const jobs = [
    { job_id: "job-short", status: "queued", params: { name: "PCV-IT-S4" } },
    { job_id: "job-long", status: "running", params: { name: "pcv-it-s4-console" } },
    { job_id: "job-long-cp", status: "succeeded", params: { vm_name: "pcv-it-s4-console", checkpoint_name: "pcv-it-s4" } },
    { job_id: "job-clone", status: "queued", params: { source: "pcv-it-s4", name: "other-vm" } },
    { job_id: "job-nested", status: "failed", params: { name: "pcv-it-s4-console", reconciliation: { expected_before: { name: "pcv-it-s4" } } } },
    { job_id: "job-blob", status: "queued", params: { name: "unrelated" }, result: { note: "pcv-it-s4" } }
  ];
  const shortVm = { id: "id-s", name: "pcv-it-s4", state: "Off" };
  install(sandbox, shortVm, { serverJobs: jobs });
  assert.equal(sandbox.collectVmJobs("id-s", "pcv-it-s4").map((job) => job.job_id).join(","), "job-short,job-clone");
  assert.equal(sandbox.collectVmJobs("id-l", "pcv-it-s4-console").map((job) => job.job_id).join(","), "job-long,job-long-cp,job-nested");
});

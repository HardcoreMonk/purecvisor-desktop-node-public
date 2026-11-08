import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 5 (ui-redesign-20261105): checkpoint tab, create modal, schedule confirm, and BL-0020 readback.
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
  for (const file of ["i18n.js", "src/modules/uxlib.ts", "src/modules/modal-core.ts", "src/modules/components.ts", "src/modules/vm.ts", "src/modules/vm-console.ts"]) {
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
    selectedVmReadbacks: null,
    selectedVmQosControl: null,
    pendingVmActions: {},
    serverJobs: opts.serverJobs || [],
    trackedJobs: opts.trackedJobs || [],
    vms: opts.vms || [row],
    error: null,
    actionPending: false,
    checkpointListError: opts.checkpointListError || null,
    checkpointSchedulePreview: null,
    selectedVmCheckpoints: opts.checkpoints || []
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
  sandbox.bindVmConsoleFrame = () => {};
  sandbox.renderExportImportPreview = () => "";
  sandbox.getCheckpointId = (checkpoint) => String((checkpoint && (checkpoint.id || checkpoint.name || checkpoint.checkpoint_name)) || "");
  sandbox.getCheckpointName = (checkpoint) => String((checkpoint && (checkpoint.name || checkpoint.checkpoint_name || checkpoint.id)) || "");
  sandbox.setCheckpointActionPending = () => {};
  sandbox.clearCheckpointActionPending = () => {};
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

const consoleSourcePath = fileURLToPath(new URL("../src/modules/vm-console.ts", import.meta.url));
const apiSourcePath = fileURLToPath(new URL("../src/modules/desktop-api.ts", import.meta.url));
const alpha = { id: "id-1", name: "alpha", state: "Off", checkpoints: { count: 0 } };

function flush() {
  return new Promise((resolve) => setTimeout(resolve, 30));
}

function sliceFn(source, name) {
  const start = source.indexOf(`async function ${name}`);
  assert.notEqual(start, -1, name);
  const rest = source.slice(start + 1);
  const next = rest.search(/\nasync function |\nfunction |\n\/\/ exports/);
  return next < 0 ? source.slice(start) : source.slice(start, start + 1 + next);
}

function checkpointPanel(sandbox) {
  sandbox.renderVmDetail();
  return sandbox.els.vmDetailContent.querySelector(".checkpoint-panel");
}

test("checkpoint list shows name, created time, restore, and delete", () => {
  const sandbox = createEnv();
  install(sandbox, alpha, {
    checkpoints: [{ name: "manual-1", created_at: "2026-11-05T08:00:00Z" }]
  });
  const panel = checkpointPanel(sandbox);
  const headers = panel.querySelectorAll("th").map((cell) => cell.textContent);
  assert.deepEqual(headers, ["이름", "만든 시각", "조작"]);
  assert.equal(panel.textContent.includes("manual-1"), true);
  assert.equal(panel.textContent.includes("2026-11-05T08:00:00Z"), true);
  assert.equal(panel.querySelector('[data-action="checkpoint-restore"]').textContent, "복원…");
  assert.equal(panel.querySelector('[data-action="checkpoint-delete"]').textContent, "삭제…");
  assert.equal(headers.includes("종류"), false);
});

test("create modal blocks empty, invalid, and duplicate names, then posts the accepted name", async () => {
  const sandbox = createEnv();
  install(sandbox, alpha, {
    checkpoints: [{ name: "Manual-1", created_at: "2026-11-05T08:00:00Z" }]
  });
  const calls = [];
  sandbox.desktopApi.createCheckpoint = async (vmId, name) => {
    calls.push({ vmId, name });
    return { job_id: "job-create", status: "queued" };
  };
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  sandbox.renderVmDetail();
  sandbox.openCheckpointCreateModal("id-1");
  const modal = sandbox.document.querySelector(".pcv-checkpoint-create-modal");
  const input = modal.querySelector("input");
  const button = modal.querySelector("button");
  assert.match(input.value, /^checkpoint-\d{8}-\d{4}(?:-\d+)?$/);
  assert.equal(input.value.includes("pcv-schedule"), false);
  assert.equal(sandbox.suggestCheckpointName(["checkpoint-20261105-1002", "checkpoint-20261105-1002-2"], new Date(2026, 10, 5, 10, 2, 0)), "checkpoint-20261105-1002-3");
  const rejected = ["", "   ", " bad", "a/b", "a\\b", "a".repeat(129), "a\u0007b", "manual-1"];
  for (const value of rejected) {
    input.value = value;
    fire(input, "input");
    assert.equal(button.disabled, true, value);
    button.click();
  }
  await flush();
  assert.equal(calls.length, 0);
  for (const value of [".", ".."]) {
    input.value = value;
    fire(input, "input");
    assert.equal(button.disabled, false, value);
  }
  input.value = "checkpoint-ok";
  fire(input, "input");
  assert.equal(button.disabled, false);
  button.click();
  await flush();
  assert.equal(calls.length, 1);
  assert.equal(calls[0].vmId, "id-1");
  assert.equal(calls[0].name, "checkpoint-ok");
  const job = modal.querySelector(".pcv-checkpoint-job");
  assert.equal(job.querySelector(".pcv-job") != null, true);
  const client = fs.readFileSync(apiSourcePath, "utf8");
  const at = client.indexOf("createCheckpoint:");
  assert.notEqual(at, -1);
  assert.equal(client.slice(at, at + 400).includes("JSON.stringify({ name })"), true);
});

test("restore and delete use a danger confirm without a typed name", async () => {
  const sandbox = createEnv();
  install(sandbox, { id: "id-1", name: "alpha", state: "Running" }, {
    checkpoints: [{ name: "manual-1", created_at: "2026-11-05T08:00:00Z" }]
  });
  const seen = [];
  sandbox.PCV.components.confirmDialog = async (opts) => {
    seen.push(opts);
    return true;
  };
  const restores = [];
  const deletes = [];
  sandbox.desktopApi.restoreCheckpoint = async (vmId, checkpointId) => {
    restores.push({ vmId, checkpointId });
    return { job_id: "job-restore", status: "queued" };
  };
  sandbox.desktopApi.deleteCheckpoint = async (vmId, checkpointId) => {
    deletes.push({ vmId, checkpointId });
    return { job_id: "job-delete", status: "queued" };
  };
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  await sandbox.queueCheckpointRestore("id-1", "manual-1");
  assert.equal(seen[0].danger, true);
  assert.equal(Object.prototype.hasOwnProperty.call(seen[0], "typedName"), false);
  assert.match(seen[0].message, /현재 상태/);
  assert.match(seen[0].message, /실행 중/);
  assert.equal(restores.length, 1);
  sandbox.state.selectedVm = { id: "id-1", name: "alpha", state: "Off" };
  sandbox.PCV.components.confirmDialog = async () => false;
  await sandbox.queueCheckpointRestore("id-1", "manual-1");
  assert.equal(restores.length, 1);
  sandbox.PCV.components.confirmDialog = async (opts) => {
    seen.push(opts);
    return true;
  };
  await sandbox.queueCheckpointDelete("id-1", "manual-1");
  const deleted = seen[seen.length - 1];
  assert.equal(deleted.danger, true);
  assert.equal(Object.prototype.hasOwnProperty.call(deleted, "typedName"), false);
  assert.equal(deletes.length, 1);
  sandbox.PCV.components.confirmDialog = async () => false;
  await sandbox.queueCheckpointDelete("id-1", "manual-1");
  assert.equal(deletes.length, 1);
});

test("checkpoint and schedule paths do not call window.confirm", async () => {
  const vmSource = fs.readFileSync(vmSourcePath, "utf8");
  const consoleSource = fs.readFileSync(consoleSourcePath, "utf8");
  for (const name of ["queueCheckpointCreate", "queueCheckpointRestore", "queueCheckpointDelete"]) {
    assert.equal(sliceFn(vmSource, name).includes("window.confirm"), false, name);
  }
  const schedule = sliceFn(consoleSource, "queueCheckpointScheduleControl");
  assert.equal(schedule.includes("window.confirm"), false);
  const sandbox = createEnv();
  install(sandbox, alpha);
  const seen = [];
  sandbox.PCV.components.confirmDialog = async (opts) => {
    seen.push(opts);
    return true;
  };
  const previews = [];
  const saves = [];
  sandbox.desktopApi.previewCheckpointSchedule = async (vmId, payload) => {
    previews.push({ vmId, payload });
    return { dry_run: true, interval_minutes: payload.interval_minutes, retention_max: payload.retention_max };
  };
  sandbox.desktopApi.setCheckpointSchedule = async (vmId, payload) => {
    saves.push({ vmId, payload });
    return { job_id: "job-set", status: "queued" };
  };
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  await sandbox.queueCheckpointScheduleControl("id-1", "preview", { interval_minutes: 60, retention_max: 3 });
  assert.equal(seen.length, 0);
  assert.equal(previews.length, 1);
  await sandbox.queueCheckpointScheduleControl("id-1", "set", { interval_minutes: 60, retention_max: 3 });
  assert.equal(seen.length, 1);
  assert.equal(seen[0].danger, false);
  assert.equal(saves.length, 1);
  sandbox.PCV.components.confirmDialog = async () => false;
  await sandbox.queueCheckpointScheduleControl("id-1", "clear", {});
  assert.equal(saves.length, 1);
});

test("BL-0032 preview keeps typed schedule inputs after the form re-renders", async () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.desktopApi.previewCheckpointSchedule = async (_vmId, payload) => ({
    dry_run: true,
    interval_minutes: payload.interval_minutes,
    retention_max: payload.retention_max
  });
  sandbox.render = () => { sandbox.renderVmDetail(); };
  sandbox.renderVmDetail();
  const form = () => sandbox.els.vmDetailContent.querySelector('form[data-action="checkpoint-schedule"]');
  form().querySelector('[name="interval_minutes"]').value = "60";
  form().querySelector('[name="retention_max"]').value = "1";
  await sandbox.queueCheckpointScheduleControl("id-1", "preview", { interval_minutes: 60, retention_max: 1 });
  assert.equal(form().querySelector('[name="interval_minutes"]').value, "60");
  assert.equal(form().querySelector('[name="retention_max"]').value, "1");
});

test("schedule save reads the VM detail again and the card shows the new schedule", async () => {
  const sandbox = createEnv();
  const row = {
    id: "id-1",
    name: "alpha",
    state: "Off",
    checkpoint_schedule: { enabled: false, interval_minutes: 1440, retention_max: 1, next_due_at: "DUE-OLD", status: "disabled" }
  };
  install(sandbox, row);
  sandbox.desktopApi.setCheckpointSchedule = async () => ({ job_id: "job-sched", status: "queued" });
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  sandbox.desktopApi.getVm = async () => ({
    id: "id-1",
    name: "alpha",
    state: "Off",
    checkpoint_schedule: {
      enabled: true,
      interval_minutes: 60,
      retention_max: 3,
      next_due_at: "DUE-NEW",
      last_enqueued_at: "ENQ-NEW",
      status: "waiting"
    }
  });
  sandbox.desktopApi.getVmCheckpoints = async () => [];
  sandbox.PCV.components.confirmDialog = async () => true;
  sandbox.render = () => { sandbox.renderVmDetail(); };
  await sandbox.queueCheckpointScheduleControl("id-1", "set", { interval_minutes: 60, retention_max: 3 });
  await flush();
  const card = sandbox.els.vmDetailContent.querySelector(".checkpoint-schedule-readback");
  assert.equal(card.textContent.includes("DUE-NEW"), true);
  assert.equal(card.textContent.includes("DUE-OLD"), false);
  assert.equal(card.textContent.includes("60"), true);
  assert.equal(card.textContent.includes("1440"), false);
  assert.equal(card.textContent.includes("ENQ-NEW"), true);
  assert.equal(card.textContent.includes("사용"), true);
  assert.equal(card.textContent.includes("꺼짐"), false);
  const facts = card.querySelectorAll("strong").map((node) => node.textContent);
  assert.equal(facts.includes("3"), true);
});

test("a finished create job refreshes the checkpoint list", async () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.desktopApi.createCheckpoint = async () => ({ job_id: "job-create", status: "queued" });
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  sandbox.desktopApi.getVmCheckpoints = async () => [{ name: "checkpoint-new", created_at: "2026-11-05T01:02:03Z" }];
  sandbox.render = () => { sandbox.renderVmDetail(); };
  await sandbox.queueCheckpointCreate("id-1", "checkpoint-new");
  await flush();
  const panel = sandbox.els.vmDetailContent.querySelector(".checkpoint-panel");
  assert.equal(panel.textContent.includes("checkpoint-new"), true);
  assert.equal(panel.textContent.includes("2026-11-05T01:02:03Z"), true);
});

test("a late checkpoint job does not overwrite another selected VM", async () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  let releaseGetVm;
  let markStarted;
  const started = new Promise((resolve) => { markStarted = resolve; });
  sandbox.desktopApi.setCheckpointSchedule = async () => ({ job_id: "job-late", status: "queued" });
  sandbox.desktopApi.getJob = async (id) => ({ job_id: id, status: "succeeded" });
  sandbox.desktopApi.getVm = () => {
    markStarted();
    return new Promise((resolve) => { releaseGetVm = resolve; });
  };
  let checkpointReads = 0;
  sandbox.desktopApi.getVmCheckpoints = async () => {
    checkpointReads += 1;
    return [{ name: "late-row", created_at: "late" }];
  };
  sandbox.PCV.components.confirmDialog = async () => true;
  sandbox.render = () => {};
  const pending = sandbox.queueCheckpointScheduleControl("id-1", "set", { interval_minutes: 60, retention_max: 3 });
  await started;
  const other = {
    id: "id-2",
    name: "beta",
    state: "Off",
    checkpoint_schedule: { enabled: false, interval_minutes: 9, retention_max: 9, next_due_at: "keep-me" }
  };
  sandbox.state.selectedVmId = "id-2";
  sandbox.state.selectedVm = other;
  sandbox.state.selectedVmCheckpoints = [{ name: "beta-cp", created_at: "beta-time" }];
  releaseGetVm({
    id: "id-1",
    name: "alpha",
    state: "Off",
    checkpoint_schedule: { enabled: true, interval_minutes: 60, retention_max: 3, next_due_at: "DUE-NEW" }
  });
  await pending;
  await flush();
  assert.equal(sandbox.state.selectedVm.name, "beta");
  assert.equal(sandbox.state.selectedVm.checkpoint_schedule.next_due_at, "keep-me");
  assert.equal(sandbox.state.selectedVm.checkpoint_schedule.interval_minutes, 9);
  assert.equal(sandbox.state.selectedVmCheckpoints[0].name, "beta-cp");
  assert.equal(checkpointReads, 0);
});

test("an empty checkpoint list and a failed read use different copy", async () => {
  const sandbox = createEnv();
  install(sandbox, alpha);
  sandbox.render = () => { sandbox.renderVmDetail(); };
  sandbox.renderVmDetail();
  const listOf = () => sandbox.els.vmDetailContent.querySelector("[data-pcv-checkpoint-list]");
  const empty = listOf().querySelector(".pcv-empty");
  assert.equal(empty.querySelector(".pcv-empty-title").textContent, "체크포인트가 없습니다");
  assert.equal(empty.querySelector(".pcv-empty-action").textContent, "첫 checkpoint 만들기");
  assert.equal(listOf().querySelector(".pcv-load-error"), null);
  sandbox.state.checkpointListError = { message: "boom" };
  sandbox.renderVmDetail();
  const failed = listOf().querySelector(".pcv-load-error");
  assert.equal(failed.querySelector(".pcv-load-error-title").textContent, "체크포인트 목록을 읽지 못했습니다");
  assert.equal(failed.querySelector(".pcv-load-error-retry").textContent, "다시 시도");
  assert.equal(listOf().querySelector(".pcv-empty"), null);
  sandbox.desktopApi.getVmCheckpoints = async () => {
    throw { code: "PCV_CHECKPOINT_LIST_FAILED", message: "still down" };
  };
  failed.querySelector(".pcv-load-error-retry").click();
  await flush();
  const again = listOf().querySelector(".pcv-load-error");
  assert.equal(again.querySelector(".pcv-load-error-title").textContent, "체크포인트 목록을 읽지 못했습니다");
  assert.equal(again.textContent.includes("still down"), true);
  sandbox.desktopApi.getVmCheckpoints = async () => [];
  again.querySelector(".pcv-load-error-retry").click();
  await flush();
  assert.equal(listOf().querySelector(".pcv-load-error"), null);
  assert.equal(listOf().querySelector(".pcv-empty-action").textContent, "첫 checkpoint 만들기");
});

import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 6 (ui-redesign-20261105): four-step VM create wizard inside #create-vm-dialog.
// The harness is a small DOM because these node tests do not start a browser.

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const indexHtml = fs.readFileSync(webRoot + "index.html", "utf8");
const VOID_TAGS = new Set(["AREA", "BASE", "BR", "COL", "EMBED", "HR", "IMG", "INPUT", "LINK", "META", "SOURCE", "TRACK", "WBR"]);
const EXAMPLE_NAME = "internal-lab-01";
const EXAMPLE_ISO = "D:\\iso\\installer.iso";
const EXAMPLE_ROOT = "D:\\PureCVisor\\VMs";

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
    this.dataset = {};
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
  get placeholder() { return this.getAttribute("placeholder") || ""; }
  set placeholder(value) { this.setAttribute("placeholder", value); }
  get tabIndex() { return this._tabIndex; }
  set tabIndex(value) { this._tabIndex = Number(value); this.setAttribute("tabindex", String(value)); }
  get children() { return this.childNodes.filter((child) => child.nodeType === 1); }
  get firstChild() { return this.childNodes[0] || null; }
  get textContent() {
    if (this.childNodes.length === 0) return "";
    return this.childNodes.map((child) => (child.nodeType === 3 ? child.textContent : child.textContent || "")).join("");
  }
  set textContent(value) {
    this.childNodes = [];
    this.appendChild(new TextNode(value));
  }
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
    if (name === "aria-current") { /* attribute only */ }
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
  focus() {
    if (this.disabled) return;
    if (this.ownerDocument) this.ownerDocument.activeElement = this;
  }
  click() {
    this.dispatchEvent({ type: "click", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  }
  showModal() { this.open = true; }
  close() {
    this.open = false;
    this.dispatchEvent({ type: "close", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  }
  getBoundingClientRect() { return { left: 0, top: 0, right: 200, bottom: 120, width: 200, height: 120 }; }
  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
  querySelectorAll(selector) {
    const groups = selector.split(",").map((part) => part.trim()).filter(Boolean);
    const found = [];
    walk(this, (element) => {
      if (element !== this && groups.some((group) => matchesComplex(element, group))) found.push(element);
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
    if (child.parentNode && typeof child.parentNode.removeChild === "function") child.parentNode.removeChild(child);
    child.parentNode = this;
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

function matchesComplex(element, selector) {
  const parts = selector.split(/\s+/).filter(Boolean);
  if (parts.length === 1) return matches(element, parts[0]);
  if (!matches(element, parts[parts.length - 1])) return false;
  let current = element.parentNode;
  for (let index = parts.length - 2; index >= 0; index -= 1) {
    while (current && current.nodeType === 1 && !matches(current, parts[index])) current = current.parentNode;
    if (!current || current.nodeType !== 1) return false;
    current = current.parentNode;
  }
  return true;
}

function decodeEntities(text) {
  return String(text)
    .replaceAll("&lt;", "<")
    .replaceAll("&gt;", ">")
    .replaceAll("&quot;", "\"")
    .replaceAll("&#39;", "'")
    .replaceAll("&amp;", "&");
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

function walkFind(node, predicate) {
  for (const child of node.childNodes || []) {
    if (child.nodeType !== 1) continue;
    if (predicate(child)) return child;
    const nested = walkFind(child, predicate);
    if (nested) return nested;
  }
  return null;
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
    createElementNS: (_ns, tag) => document.createElement(tag),
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

function fire(target, type, extra = {}) {
  target.dispatchEvent({
    type,
    ...extra,
    preventDefault() { this.defaultPrevented = true; },
    stopPropagation() { this.cancelBubble = true; }
  });
}

function flush() {
  return new Promise((resolve) => setTimeout(resolve, 20));
}

function field(sandbox, name) {
  return sandbox.document.getElementById("create-vm-form").querySelector(`[name="${name}"]`);
}

function summary(sandbox, key) {
  return sandbox.document.querySelector(`[data-summary="${key}"]`);
}

function install(sandbox, opts = {}) {
  sandbox.state = {
    vms: opts.vms || [],
    networkInventory: opts.networkInventory || null,
    selectedVmId: "",
    error: null
  };
  sandbox.getVmName = (item) => String((item && (item.name || item.id)) || "");
  sandbox.getVmId = (item) => String((item && (item.id || item.name)) || "");
  sandbox.asArray = (value) => (Array.isArray(value) ? value : []);
  sandbox.asObject = (value) => (value && typeof value === "object" && !Array.isArray(value) ? value : {});
  sandbox.requireRbac = () => {};
  sandbox.normalizeError = (error) => error || {};
  sandbox.pcvTrackJob = (job) => { sandbox.tracked = job; };
  sandbox.startPolling = () => { sandbox.polled = true; };
  sandbox.loadVms = async () => {
    if (opts.onLoadVms) await opts.onLoadVms(sandbox);
  };
  sandbox.refreshSelectedVm = async () => {};
  sandbox.render = () => { sandbox.rendered = true; };
  sandbox.loadNetworkInventory = async () => {
    if (typeof opts.loadNetworkInventory === "function") await opts.loadNetworkInventory(sandbox);
  };
  sandbox.desktopApi = {
    createVm: async (payload) => {
      sandbox.createdPayload = payload;
      if (opts.createError) throw opts.createError;
      return opts.job || { job_id: "job-create", status: "queued" };
    },
    getJob: async () => opts.polledJob || { job_id: "job-create", status: "succeeded" }
  };
  sandbox.PCV.desktopApi = sandbox.desktopApi;
}

function mount(sandbox) {
  const start = indexHtml.indexOf('<dialog id="create-vm-dialog">');
  const end = indexHtml.indexOf("</dialog>", start);
  assert.ok(start > 0 && end > start, "create dialog markup");
  const fragment = parseHtml(indexHtml.slice(start, end + "</dialog>".length), sandbox.document);
  while (fragment.childNodes.length) sandbox.document.body.appendChild(fragment.childNodes[0]);
}

async function openWizard(opts = {}) {
  const sandbox = createEnv();
  install(sandbox, opts);
  mount(sandbox);
  await sandbox.openCreateVmWizard();
  return sandbox;
}

async function reachStep(sandbox, step, values = {}) {
  const name = values.name || "lab01";
  field(sandbox, "name").value = name;
  fire(field(sandbox, "name"), "input");
  if (step < 2) return;
  sandbox.document.getElementById("create-vm-next").click();
  if (values.cpu != null) {
    field(sandbox, "cpu").value = String(values.cpu);
    fire(field(sandbox, "cpu"), "input");
  }
  if (step < 3) return;
  sandbox.document.getElementById("create-vm-next").click();
  field(sandbox, "iso_path").value = values.iso || "E:\\media\\setup.iso";
  fire(field(sandbox, "iso_path"), "input");
  if (values.vmRoot != null) {
    field(sandbox, "vm_root").value = values.vmRoot;
    fire(field(sandbox, "vm_root"), "input");
  }
  if (step < 4) return;
  sandbox.document.getElementById("create-vm-next").click();
}

test("wizard shows four steps, moves with next and arrow keys, and focuses the first field", async () => {
  const sandbox = await openWizard();
  const tabs = [1, 2, 3, 4].map((index) => sandbox.document.getElementById(`create-vm-step-tab-${index}`));
  assert.deepEqual(tabs.map((tab) => tab.textContent.replace(/\s+/g, "").trim()), ["1기본", "2자원", "3설치와네트워크", "4확인"]);
  assert.equal(tabs[0].classList.contains("is-current"), true);
  assert.equal(tabs[0].getAttribute("aria-current"), "step");
  assert.equal(sandbox.document.getElementById("create-vm-step-2").hidden, true);
  assert.equal(sandbox.document.activeElement, field(sandbox, "name"));
  assert.equal(sandbox.document.getElementById("create-vm-next").disabled, true);
  assert.equal(sandbox.document.getElementById("create-vm-next").tagName, "BUTTON");
  assert.equal(sandbox.document.getElementById("create-vm-back").tagName, "BUTTON");
  field(sandbox, "name").value = "lab01";
  fire(field(sandbox, "name"), "input");
  assert.equal(sandbox.document.getElementById("create-vm-next").disabled, false);
  fire(tabs[0], "keydown", { key: "ArrowRight" });
  assert.equal(sandbox.document.getElementById("create-vm-step-2").hidden, false);
  assert.equal(tabs[0].classList.contains("is-complete"), true);
  assert.equal(tabs[1].classList.contains("is-current"), true);
  assert.equal(sandbox.document.activeElement, field(sandbox, "cpu"));
  field(sandbox, "cpu").value = "99";
  fire(field(sandbox, "cpu"), "input");
  assert.equal(sandbox.document.getElementById("create-vm-next").disabled, true);
  field(sandbox, "cpu").value = "2";
  fire(field(sandbox, "cpu"), "input");
  assert.equal(sandbox.document.getElementById("create-vm-next").disabled, false);
});

test("next stays off when the create name breaks the rule or duplicates a VM", async () => {
  const sandbox = await openWizard({ vms: [{ id: "id-1", name: "Lab01" }] });
  const next = sandbox.document.getElementById("create-vm-next");
  field(sandbox, "name").value = "bad name";
  fire(field(sandbox, "name"), "input");
  assert.equal(next.disabled, true);
  field(sandbox, "name").value = "-leading";
  fire(field(sandbox, "name"), "input");
  assert.equal(next.disabled, true);
  field(sandbox, "name").value = "lab01";
  fire(field(sandbox, "name"), "input");
  assert.equal(next.disabled, true);
  field(sandbox, "name").value = "lab02";
  fire(field(sandbox, "name"), "input");
  assert.equal(next.disabled, false);
});

test("example name, ISO, and VM root are placeholders and not initial values", async () => {
  const sandbox = await openWizard();
  const dialog = indexHtml.slice(indexHtml.indexOf('<dialog id="create-vm-dialog">'), indexHtml.indexOf("</dialog>", indexHtml.indexOf('<dialog id="create-vm-dialog">')));
  assert.equal(dialog.includes(`value="${EXAMPLE_NAME}"`), false);
  assert.equal(dialog.includes(`value="${EXAMPLE_ISO}"`), false);
  assert.equal(dialog.includes(`value="${EXAMPLE_ROOT}"`), false);
  assert.equal(field(sandbox, "name").value, "");
  assert.equal(field(sandbox, "iso_path").value, "");
  assert.equal(field(sandbox, "vm_root").value, "");
  assert.equal(field(sandbox, "name").placeholder, EXAMPLE_NAME);
  assert.equal(field(sandbox, "iso_path").placeholder, EXAMPLE_ISO);
  assert.equal(field(sandbox, "vm_root").placeholder, EXAMPLE_ROOT);
  assert.equal(sandbox.document.querySelector('#create-vm-form [name="generation"]'), null);
  assert.equal(sandbox.document.getElementById("create-vm-generation").textContent, "Generation 2");
  assert.equal(sandbox.document.querySelector("#create-vm-step-1 .muted").textContent, "서버는 Hyper-V 세대 2만 받습니다.");
  assert.equal(field(sandbox, "cpu").value, "2");
  assert.equal(field(sandbox, "memory_mb").value, "4096");
  assert.equal(field(sandbox, "disk_gb").value, "40");
});

test("going back keeps the values entered on earlier steps", async () => {
  const sandbox = await openWizard();
  await reachStep(sandbox, 3, { name: "kept01", iso: "E:\\media\\setup.iso" });
  field(sandbox, "cpu").value = "4";
  fire(field(sandbox, "cpu"), "input");
  sandbox.document.getElementById("create-vm-back").click();
  assert.equal(field(sandbox, "cpu").value, "4");
  assert.equal(sandbox.document.getElementById("create-vm-step-2").hidden, false);
  sandbox.document.getElementById("create-vm-back").click();
  assert.equal(field(sandbox, "name").value, "kept01");
  sandbox.document.getElementById("create-vm-next").click();
  sandbox.document.getElementById("create-vm-next").click();
  assert.equal(field(sandbox, "iso_path").value, "E:\\media\\setup.iso");
  assert.equal(field(sandbox, "cpu").value, "4");
});

test("step 3 shows the default switch notice and has no switch input", async () => {
  let calls = 0;
  const sandbox = await openWizard({
    loadNetworkInventory: async () => { calls += 1; }
  });
  await reachStep(sandbox, 3, { iso: "E:\\media\\setup.iso" });
  const notice = "Default Switch에 연결됩니다. 만든 뒤 하드웨어 탭에서 바꿀 수 있습니다.";
  assert.equal(calls, 0);
  assert.equal(sandbox.document.getElementById("create-vm-network-notice").textContent, notice);
  assert.equal(sandbox.document.querySelector('#create-vm-form [name="switch_name"]'), null);
  assert.equal(sandbox.document.querySelector("#create-vm-form select"), null);
  assert.equal(sandbox.document.querySelector("#create-vm-step-3").textContent.includes("연결 안 함"), false);
});

test("confirm step summarizes the entered VM", async () => {
  const sandbox = await openWizard();
  await reachStep(sandbox, 4, {
    name: "lab01",
    iso: "E:\\media\\setup.iso",
    vmRoot: ""
  });
  const notice = "Default Switch에 연결됩니다. 만든 뒤 하드웨어 탭에서 바꿀 수 있습니다.";
  assert.equal(sandbox.document.getElementById("create-vm-step-4").hidden, false);
  assert.equal(summary(sandbox, "name").textContent.includes("lab01"), true);
  assert.equal(summary(sandbox, "generation").textContent.includes("Generation 2"), true);
  assert.equal(summary(sandbox, "cpu").textContent.includes("2 vCPU"), true);
  assert.equal(summary(sandbox, "memory").textContent.includes("4096 MB"), true);
  assert.equal(summary(sandbox, "disk").textContent.includes("40 GB"), true);
  assert.equal(summary(sandbox, "iso").textContent.includes("E:\\media\\setup.iso"), true);
  assert.equal(summary(sandbox, "network").textContent.includes(notice), true);
  assert.equal(summary(sandbox, "network").textContent.includes("LabSwitch"), false);
  assert.equal(summary(sandbox, "vm_root").textContent.includes("서버 기본 위치"), true);
  assert.equal(sandbox.document.getElementById("create-vm-submit").hidden, false);
  assert.equal(sandbox.document.getElementById("create-vm-submit").textContent, "만들기");
});

test("create posts server field names and units and omits a blank root and the switch", async () => {
  const sandbox = await openWizard();
  await reachStep(sandbox, 4, {
    name: "lab01",
    cpu: 4,
    iso: "E:\\media\\setup.iso"
  });
  field(sandbox, "memory_mb").value = "2048";
  field(sandbox, "disk_gb").value = "80";
  await sandbox.submitCreateVmWizard({ preventDefault() {} });
  const payload = JSON.parse(JSON.stringify(sandbox.createdPayload));
  assert.deepEqual(payload, {
    name: "lab01",
    iso_path: "E:\\media\\setup.iso",
    cpu: 4,
    memory_mb: 2048,
    disk_gb: 80,
    generation: 2
  });
  for (const key of ["cpu", "memory_mb", "disk_gb", "generation"]) {
    assert.equal(typeof payload[key], "number");
  }
  assert.equal(Object.hasOwn(payload, "vm_root"), false);
  assert.equal(Object.hasOwn(payload, "switch_name"), false);
  field(sandbox, "vm_root").value = "E:\\vm-store";
  const withRoot = JSON.parse(JSON.stringify(sandbox.readCreateWizardPayload()));
  assert.equal(withRoot.vm_root, "E:\\vm-store");
  assert.equal(Object.hasOwn(withRoot, "switch_name"), false);
});

test("after create the dialog shows job progress and opens the new VM", async () => {
  const sandbox = await openWizard({
    onLoadVms: async (box) => {
      box.state.vms = [{ id: "id-new", name: "lab01" }];
    }
  });
  await reachStep(sandbox, 4, { name: "lab01", iso: "E:\\media\\setup.iso" });
  await sandbox.submitCreateVmWizard({ preventDefault() {} });
  await flush();
  const job = sandbox.document.getElementById("create-vm-job");
  assert.equal(job.hidden, false);
  assert.equal(job.textContent.includes("완료"), true);
  assert.equal(sandbox.tracked.job_id, "job-create");
  assert.equal(sandbox.polled, true);
  assert.equal(sandbox.document.getElementById("create-vm-panels").hidden, true);
  const open = sandbox.document.getElementById("create-vm-open");
  assert.equal(open.textContent, "새 VM 열기");
  open.click();
  await flush();
  assert.equal(sandbox.state.selectedVmId, "id-new");
  assert.equal(sandbox.rendered, true);
});

test("a failed create shows the code and how to fix it", async () => {
  const sandbox = await openWizard({
    createError: {
      code: "PCV_VM_ALREADY_EXISTS",
      message: "VM already exists.",
      detail: "Choose another name."
    }
  });
  await reachStep(sandbox, 4, { name: "lab01", iso: "E:\\media\\setup.iso" });
  await sandbox.submitCreateVmWizard({ preventDefault() {} });
  const job = sandbox.document.getElementById("create-vm-job");
  assert.equal(job.textContent.includes("PCV_VM_ALREADY_EXISTS"), true);
  assert.equal(job.textContent.includes("목록에 없는 다른 이름을 입력하세요."), true);
  assert.equal(job.textContent.includes("Choose another name."), true);
  assert.equal(sandbox.document.getElementById("create-vm-open"), null);
});

test("closing while input is in progress asks once and discard drops the input", async () => {
  const sandbox = await openWizard();
  const dialog = sandbox.document.getElementById("create-vm-dialog");
  field(sandbox, "name").value = "lab01";
  fire(field(sandbox, "name"), "input");
  fire(dialog, "cancel", { key: "Escape" });
  const accept = sandbox.document.querySelector('[data-pcv-confirm="accept"]');
  const confirm = sandbox.document.querySelector(".pcv-confirm");
  assert.equal(dialog.open, true);
  assert.ok(accept);
  assert.equal(confirm.textContent.includes("입력을 버릴까요?"), true);
  assert.equal(accept.classList.contains("pcv-confirm-danger"), false);
  accept.click();
  await flush();
  assert.equal(dialog.open, false);
  await sandbox.openCreateVmWizard();
  assert.equal(field(sandbox, "name").value, "");
});

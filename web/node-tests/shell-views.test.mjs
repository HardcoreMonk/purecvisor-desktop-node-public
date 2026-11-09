import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 7a: sidebar groups, W1 chips and row actions, templates view, accounts screen.

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const SECRET = "S3cret-pass-1";

class Node {
  constructor() {
    this.parentNode = null;
    this.childNodes = [];
    this.nodeType = 0;
    this.ownerDocument = null;
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
    this._html = "";
    this.open = false;
    const self = this;
    this.dataset = new Proxy({}, {
      get(_target, prop) {
        return self.getAttribute("data-" + String(prop).replace(/[A-Z]/g, (ch) => "-" + ch.toLowerCase()));
      },
      set(_target, prop, value) {
        self.setAttribute("data-" + String(prop).replace(/[A-Z]/g, (ch) => "-" + ch.toLowerCase()), value);
        return true;
      }
    });
    this.classList = {
      add: (...names) => names.forEach((name) => {
        if (name && !this._classes().includes(name)) this._className = `${this._className} ${name}`.trim();
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
  get children() { return this.childNodes.filter((child) => child.nodeType === 1); }
  get firstChild() { return this.childNodes[0] || null; }
  get textContent() {
    if (this.childNodes.length === 0) return "";
    return this.childNodes.map((child) => (child.nodeType === 3 ? child.textContent : child.textContent || "")).join("");
  }
  set textContent(value) {
    this.childNodes = [];
    if (value != null && value !== "") this.appendChild(new TextNode(value));
  }
  get innerHTML() { return this._html; }
  set innerHTML(value) {
    this._html = String(value ?? "");
    this.childNodes = [];
    const parsed = parseHtml(this._html, this.ownerDocument);
    while (parsed.firstChild) this.appendChild(parsed.firstChild);
  }
  get elements() {
    const fields = this.querySelectorAll("input, select, textarea");
    return { namedItem: (name) => fields.find((field) => field.getAttribute("name") === name) || null };
  }
  setAttribute(name, value) {
    const text = String(value);
    this.attributes.set(name, text);
    if (name === "id") this._id = text;
    if (name === "class") this._className = text;
    if (name === "disabled") this._disabled = true;
    if (name === "hidden") this._hidden = true;
    if (name === "value") this._value = text;
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
    if (child.parentNode && typeof child.parentNode.removeChild === "function") child.parentNode.removeChild(child);
    child.parentNode = this;
    if (!child.ownerDocument && this.ownerDocument) child.ownerDocument = this.ownerDocument;
    this.childNodes.push(child);
    return child;
  }
  insertBefore(child, reference) {
    if (child.parentNode && typeof child.parentNode.removeChild === "function") child.parentNode.removeChild(child);
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
  replaceChildren(...nodes) {
    this.childNodes = [];
    for (const node of nodes) if (node) this.appendChild(node);
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
  getBoundingClientRect() { return { left: 0, top: 0, right: 200, bottom: 40, width: 200, height: 40 }; }
  getClientRects() { return this.hidden ? [] : [{}]; }
  getContext() { return { beginPath() {}, arc() {}, fill() {}, fillText() {} }; }
  toDataURL() { return "data:image/png;base64,"; }
  querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
  querySelectorAll(selector) {
    const groups = String(selector).split(",").map((part) => part.trim()).filter(Boolean);
    const found = [];
    walk(this, (element) => {
      if (element !== this && groups.some((group) => matchesComplex(element, group))) found.push(element);
    });
    return found;
  }
  closest(selector) {
    let node = this;
    while (node && node.nodeType === 1) {
      if (matches(node, selector)) return node;
      node = node.parentNode;
    }
    return null;
  }
  contains(node) {
    let current = node;
    while (current) {
      if (current === this) return true;
      current = current.parentNode;
    }
    return false;
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
  get firstChild() { return this.childNodes[0] || null; }
}

class DomEvent {
  constructor(type) { this.type = type; }
  preventDefault() { this.defaultPrevented = true; }
  stopPropagation() { this.cancelBubble = true; }
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
    element.setAttribute(match[1], decodeEntities(match[2] ?? match[3] ?? match[4] ?? ""));
  }
}

function parseHtml(html, owner) {
  const root = new Fragment();
  root.ownerDocument = owner;
  const stack = [root];
  const re = /<\/([A-Za-z][\w:-]*)>|<([A-Za-z][\w:-]*)([^>]*)>/g;
  let match;
  let cursor = 0;
  while ((match = re.exec(html))) {
    if (match.index > cursor) {
      const text = decodeEntities(html.slice(cursor, match.index));
      if (text.trim()) stack[stack.length - 1].appendChild(owner.createTextNode(text));
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
    const selfClosing = /\/\s*$/.test(match[3] || "") || ["INPUT", "BR", "IMG"].includes(element.tagName);
    if (!selfClosing) stack.push(element);
  }
  return root;
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

function storage() {
  const values = new Map();
  return {
    getItem: (key) => (values.has(key) ? values.get(key) : null),
    setItem: (key, value) => { values.set(key, String(value)); },
    removeItem: (key) => { values.delete(key); },
    clear: () => values.clear()
  };
}

function section(document, id) {
  const node = document.createElement("section");
  node.id = id;
  node.className = "section app-view";
  node.dataset.view = id;
  node.hidden = true;
  return node;
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
  const head = new Element("head");
  const body = new Element("body");
  html.ownerDocument = document;
  head.ownerDocument = document;
  body.ownerDocument = document;
  html.parentNode = document;
  html.appendChild(head);
  html.appendChild(body);
  document.documentElement = html;
  document.head = head;
  document.body = body;
  document.activeElement = body;
  const sidebar = document.createElement("aside");
  sidebar.id = "shell-sidebar";
  const topbar = document.createElement("div");
  topbar.id = "shell-topbar";
  const statusbar = document.createElement("div");
  statusbar.id = "shell-statusbar";
  const cb = document.createElement("div");
  cb.id = "cb";
  for (const id of ["dashboard", "vms", "templates", "network", "jobs", "activity", "evidence", "accounts", "troubleshooting", "helppage"]) {
    cb.appendChild(section(document, id));
  }
  const vmTable = document.createElement("div");
  vmTable.id = "vm-table";
  const workspace = document.createElement("div");
  workspace.className = "vm-workspace";
  const closeVmDetail = document.createElement("button");
  closeVmDetail.id = "close-vm-detail";
  closeVmDetail.type = "button";
  workspace.appendChild(vmTable);
  workspace.appendChild(closeVmDetail);
  const chips = document.createElement("div");
  chips.id = "vm-state-chips";
  for (const name of ["all", "running", "off", "template", "failed"]) {
    const button = document.createElement("button");
    button.setAttribute("data-vm-chip", name);
    const count = document.createElement("span");
    count.setAttribute("data-chip-count", name);
    count.textContent = "0";
    button.appendChild(count);
    chips.appendChild(button);
  }
  const templateTable = document.createElement("div");
  templateTable.id = "template-table";
  const accountsPanel = document.createElement("div");
  accountsPanel.id = "accounts-panel";
  const session = document.createElement("div");
  session.id = "account-session-panel";
  const detailTitle = document.createElement("h2");
  detailTitle.id = "vm-detail-title";
  const detailContent = document.createElement("div");
  detailContent.id = "vm-detail-content";
  body.appendChild(sidebar);
  body.appendChild(topbar);
  body.appendChild(statusbar);
  body.appendChild(cb);
  body.appendChild(detailTitle);
  body.appendChild(detailContent);
  document.getElementById("vms").appendChild(chips);
  document.getElementById("vms").appendChild(workspace);
  document.getElementById("templates").appendChild(templateTable);
  document.getElementById("accounts").appendChild(accountsPanel);
  document.getElementById("troubleshooting").appendChild(session);
  const listeners = new Map();
  const sandbox = {
    console: { log() {}, warn() {}, error() {}, info() {}, debug() {} },
    document,
    localStorage: storage(),
    sessionStorage: storage(),
    location: { hash: "", hostname: "127.0.0.1", pathname: "/", search: "", origin: "http://127.0.0.1", href: "http://127.0.0.1/", replace() {} },
    history: { replaceState() {}, pushState() {} },
    navigator: { language: "ko", userAgent: "node", onLine: true },
    performance: { now: () => Date.now() },
    matchMedia: () => ({ matches: false, addEventListener() {}, removeEventListener() {} }),
    getComputedStyle: () => ({ visibility: "visible", getPropertyValue: () => "" }),
    MutationObserver: class { observe() {} disconnect() {} },
    ResizeObserver: class { observe() {} disconnect() {} },
    requestAnimationFrame: (fn) => setTimeout(fn, 0),
    setTimeout,
    clearTimeout,
    setInterval: (fn, ms, ...args) => {
      const id = setInterval(fn, ms, ...args);
      if (id && id.unref) id.unref();
      return id;
    },
    clearInterval,
    queueMicrotask,
    Event: DomEvent,
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
    URL,
    URLSearchParams,
    TextEncoder,
    TextDecoder,
    Headers,
    Request,
    Response,
    AbortController,
    atob,
    btoa,
    encodeURIComponent,
    decodeURIComponent,
    Node,
    Element,
    HTMLElement: Element,
    fetch: () => Promise.resolve(new Response(JSON.stringify({ ok: false }), { status: 401, headers: { "Content-Type": "application/json" } }))
  };
  sandbox.window = sandbox;
  sandbox.self = sandbox;
  sandbox.globalThis = sandbox;
  sandbox.addEventListener = (type, handler) => {
    const list = listeners.get(type) || [];
    list.push(handler);
    listeners.set(type, list);
  };
  sandbox.removeEventListener = () => {};
  sandbox.dispatchEvent = () => true;
  vm.createContext(sandbox);
  for (const file of ["i18n.js", "app.bundle.js"]) {
    vm.runInContext(fs.readFileSync(webRoot + file, "utf8"), sandbox, { filename: file });
  }
  sandbox.els.vmTable = vmTable;
  sandbox.els.templateTable = templateTable;
  sandbox.els.vmDetailTitle = detailTitle;
  sandbox.els.vmDetailContent = detailContent;
  sandbox.els.closeVmDetail = closeVmDetail;
  sandbox.els.accountSessionPanel = session;
  sandbox.bindEvents();
  sandbox.PCV.shell.mount();
  return sandbox;
}

function flush() {
  return new Promise((resolve) => setTimeout(resolve, 20));
}

function shown(sandbox) {
  return sandbox.document.querySelectorAll(".app-view").filter((section) => !section.hidden).map((section) => section.dataset.view);
}

function resetSession(sandbox) {
  sandbox.state.authAccessToken = "";
  sandbox.state.authSession = null;
  sandbox.state.runtimePolicy = null;
  sandbox.state.accountDirectory = null;
  sandbox.state.accountDirectoryError = null;
  sandbox.state.accountManagePending = false;
  sandbox.state.vmFilter = "";
  sandbox.state.vmStateFilter = "all";
  sandbox.state.vms = [];
  sandbox.state.pendingVmActions = {};
  sandbox.refreshAll = async () => {};
}

function hostValue(value) {
  return JSON.parse(JSON.stringify(value));
}

function stubDesktopApi(sandbox, methods) {
  sandbox.desktopApi = Object.assign({}, sandbox.desktopApi, methods);
}

const env = createEnv();

test("accounts.ts has no window.confirm and the new sections are registered together", () => {
  const accounts = fs.readFileSync(webRoot + "src/modules/accounts.ts", "utf8");
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  assert.doesNotMatch(accounts, /window\.confirm/);
  assert.match(index, /<section id="templates" class="section app-view" data-view="templates">/);
  assert.match(index, /<section id="accounts" class="section app-view" data-view="accounts">/);
  assert.match(index, /data-view-link="accounts"/);
  assert.match(index, /data-i18n="account\.open"/);
  assert.equal(env.VALID_VIEWS.has("templates"), true);
  assert.equal(env.VALID_VIEWS.has("accounts"), true);
  assert.equal(env.VALID_VIEWS.has("helppage"), false);
});

test("sidebar groups keep the design order and the account item follows account.manage", () => {
  resetSession(env);
  const sections = env.PCV.shell.NAV_SECTIONS();
  assert.deepEqual(hostValue(sections.map((section) => section.label)), ["운영", "워크로드", "인프라", "관제", "시스템", "도움말"]);
  assert.deepEqual(hostValue(sections.map((section) => section.items.map((item) => item.id))), [
    ["dashboard"],
    ["vms", "templates"],
    ["network"],
    ["jobs", "activity", "evidence"],
    ["accounts", "troubleshooting"],
    ["helppage"]
  ]);
  assert.equal(sections[1].items[1].key, "nav.templates");
  assert.equal(env.t(sections[1].items[1].key), "템플릿");
  assert.equal(sections[4].items[0].key, "nav.accounts");
  assert.equal(env.t(sections[4].items[0].key), "계정");
  assert.equal(sections[4].items[1].key, "nav.troubleshooting");
  assert.equal(env.t(sections[4].items[1].key), "진단");
  env.PCV.shell.mount();
  const item = () => env.document.querySelector('#shell-sidebar [data-nav="accounts"]');
  assert.equal(item().hidden, false, "account mode off still shows the item because rbac allows every permission");
  env.state.authAccessToken = "jwt";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "op", role: "operator", permissions: ["operate"] };
  env.PCV.shell.syncAccountNav();
  assert.equal(item().hidden, true);
  env.navigateTo("accounts");
  assert.deepEqual(shown(env), ["accounts"]);
  assert.match(env.document.querySelector("#accounts-panel").textContent, /계정 권한이 없습니다/);
  env.state.authSession.permissions = ["account.manage"];
  env.PCV.shell.syncAccountNav();
  assert.equal(item().hidden, false);
  env.state.authSession.permissions = ["*"];
  env.PCV.shell.syncAccountNav();
  assert.equal(item().hidden, false);
});

test("BL-0031 the top KPI strip uses loaded VM host job alert and evidence counts", () => {
  resetSession(env);
  env.state.lastRefreshedAt = 1;
  env.state.partialFailures = [];
  env.state.vms = [
    { id: "a", name: "a", state: "Running" },
    { id: "b", name: "b", state: "Off" }
  ];
  env.state.host = { supported: true };
  env.state.opsSummary = { batch_evidence: { status: "available", configured: true, latest: {} } };
  env.state.trackedJobs = [];
  env.state.serverJobs = [];
  env.render();
  const big = [...env.document.querySelectorAll("#shell-statusbar .seg-big")].map((node) => node.textContent);
  assert.equal(big.includes("—"), false, big.join("|"));
  assert.equal(big[0], "1");
  assert.match(env.document.getElementById("shell-statusbar").textContent, /\/ 2/);
  env.state.lastRefreshedAt = null;
  env.state.vms = [];
  env.state.host = null;
  env.state.opsSummary = null;
  if (env.PCV.shell.syncFromOperatorState) env.PCV.shell.syncFromOperatorState();
});

test("one render paints the account list only on the accounts view", () => {
  resetSession(env);
  env.state.accountDirectory = [{ username: "ada", role: "operator", enabled: true }];
  env.render();
  const session = env.document.getElementById("account-session-panel");
  const accounts = env.document.getElementById("accounts-panel");
  assert.equal(session.querySelector(".account-directory-card"), null);
  assert.match(session.innerHTML, /data-action="open-login-overlay"/);
  assert.match(accounts.textContent, /ada/);
  assert.match(accounts.textContent, /operator/);
  assert.equal(accounts.querySelectorAll('[data-action="account-disable"]').length, 1);
  const source = fs.readFileSync(webRoot + "src/modules/accounts.ts", "utf8");
  assert.doesNotMatch(source, /window\.render\s*=/);
  assert.doesNotMatch(source, /renderWithAccountsView/);
  assert.doesNotMatch(source, /stripAccountDirectory/);
});

test("command palette view labels follow the sidebar keys and accounts follows account.manage", () => {
  resetSession(env);
  assert.equal(env.getViewLabel("templates"), env.t("nav.templates"));
  assert.equal(env.getViewLabel("accounts"), env.t("nav.accounts"));
  assert.equal(env.getViewLabel("vms"), "가상 머신");
  assert.equal(env.getViewLabel("activity"), "이벤트");
  const viewIds = () => env.buildCommandPaletteItems().filter((item) => String(item.id).startsWith("view:")).map((item) => String(item.id));
  assert.deepEqual(hostValue(viewIds()), [
    "view:dashboard", "view:vms", "view:templates", "view:network", "view:jobs",
    "view:activity", "view:evidence", "view:accounts", "view:troubleshooting"
  ]);
  const templates = env.buildCommandPaletteItems().find((item) => item.id === "view:templates");
  assert.equal(templates.label, "템플릿");
  assert.equal(templates.detail, "템플릿 열기");
  env.state.authAccessToken = "jwt";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "op", role: "operator", permissions: ["operate"] };
  assert.equal(viewIds().includes("view:accounts"), false);
  env.state.authSession.permissions = ["account.manage"];
  assert.equal(viewIds().includes("view:accounts"), true);
  env.state.authSession.permissions = ["*"];
  assert.equal(viewIds().includes("view:accounts"), true);
});

test("#/templates and #/accounts show only that section", () => {
  resetSession(env);
  for (const [hash, view] of [["#/templates", "templates"], ["#/accounts", "accounts"]]) {
    env.location.hash = hash;
    env.navigateToHash();
    assert.deepEqual(shown(env), [view], hash);
    assert.equal(env.PCV.nav.activeView(), view);
    assert.equal(env.state.activeView, view);
  }
});

test("VM chips count the name-filtered inventory and the pressed chip filters the table", () => {
  resetSession(env);
  env.state.vms = [
    { id: "run-1", name: "run-1", state: "Running", cpu: { count: 2 }, memory: { startup_mb: 2048 }, generation: 2 },
    { id: "off-1", name: "off-1", state: "Off", cpu: { count: 1 }, memory: { startup_mb: 1024 }, generation: 2 },
    { id: "lock-run", name: "lock-run", state: "Running", template_lock: true, cpu: { count: 4 }, memory: { startup_mb: 4096 }, generation: 2 },
    { id: "paused-1", name: "paused-1", state: "Paused", cpu: { count: 1 }, memory: { startup_mb: 512 }, generation: 1 }
  ];
  env.renderVms();
  const count = (name) => env.document.querySelector(`[data-chip-count="${name}"]`).textContent;
  assert.equal(count("all"), "4");
  assert.equal(count("running"), "2");
  assert.equal(count("off"), "1");
  assert.equal(count("template"), "1");
  assert.equal(count("failed"), "0");
  env.document.querySelector('[data-vm-chip="template"]').click();
  assert.equal(env.state.vmStateFilter, "template");
  assert.equal(env.document.querySelectorAll("#vm-table tr[data-vm-id]").length, 1);
  assert.equal(env.document.querySelector("#vm-table tr[data-vm-id]").getAttribute("data-vm-id"), "lock-run");
  env.state.vmFilter = "off";
  env.state.vmStateFilter = "all";
  env.renderVms();
  assert.equal(count("all"), "1");
  assert.equal(count("off"), "1");
  assert.equal(count("running"), "0");
  assert.equal(count("template"), "0");
  assert.equal(count("failed"), "0");
});

test("the Failed chip counts and filters VMs whose state text includes failed", () => {
  resetSession(env);
  env.state.vms = [
    { id: "run-1", name: "run-1", state: "Running", generation: 2 },
    { id: "fail-1", name: "fail-1", state: "Failed", generation: 2 },
    { id: "start-fail", name: "start-fail", status: "StartFailed", generation: 2 }
  ];
  env.renderVms();
  const count = (name) => env.document.querySelector(`[data-chip-count="${name}"]`).textContent;
  assert.equal(count("failed"), "2");
  assert.equal(count("all"), "3");
  assert.equal(count("running"), "1");
  env.document.querySelector('[data-vm-chip="failed"]').click();
  assert.equal(env.state.vmStateFilter, "failed");
  const rows = [...env.document.querySelectorAll("#vm-table tr[data-vm-id]")].map((row) => row.getAttribute("data-vm-id"));
  assert.deepEqual(hostValue(rows), ["fail-1", "start-fail"]);
  env.state.vms = [{ id: "run-1", name: "run-1", state: "Running" }];
  env.state.vmStateFilter = "all";
  env.renderVms();
  assert.equal(count("failed"), "0");
});

test("the VM row primary action follows template lock, then running, then off", () => {
  resetSession(env);
  env.state.vms = [
    { id: "run-1", name: "run-1", state: "Running", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
    { id: "off-1", name: "off-1", state: "Off", generation: 2, cpu: { count: 1 }, memory: { startup_mb: 1024 } },
    { id: "lock-run", name: "lock-run", state: "Running", template_lock: true, generation: 2, cpu: { count: 4 }, memory: { startup_mb: 4096 } },
    { id: "paused-1", name: "paused-1", state: "Paused", generation: 1, cpu: { count: 1 }, memory: { startup_mb: 512 } }
  ];
  env.renderVms();
  const action = (id) => env.document.querySelector(`#vm-table tr[data-vm-id="${id}"] .pcv-row-action`);
  assert.equal(action("run-1").getAttribute("data-action"), "vm-row-console");
  assert.equal(action("run-1").textContent, "콘솔");
  assert.equal(action("off-1").getAttribute("data-action"), "vm-row-start");
  assert.equal(action("off-1").textContent, "시작");
  assert.equal(action("lock-run").getAttribute("data-action"), "vm-row-clone");
  assert.equal(action("lock-run").textContent, "복제…");
  assert.match(env.document.querySelector('#vm-table tr[data-vm-id="lock-run"]').textContent, /템플릿/);
  assert.equal(action("paused-1"), null);
  assert.match(env.els.vmTable.innerHTML, /세대/);
  assert.match(env.els.vmTable.innerHTML, /vCPU/);
});

test("VM list detail mode opens only after a VM is chosen and close returns to the list", () => {
  resetSession(env);
  env.state.vms = [
    { id: "run-1", name: "run-1", state: "Running", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
    { id: "off-1", name: "off-1", state: "Off", generation: 2, cpu: { count: 1 }, memory: { startup_mb: 1024 } }
  ];
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.renderVms();
  const workspace = env.document.querySelector(".vm-workspace");
  assert.equal(workspace.classList.contains("is-detail"), false);
  env.state.selectedVmId = "off-1";
  env.renderVms();
  assert.equal(workspace.classList.contains("is-detail"), true);
  const chosen = env.document.querySelector('#vm-table tr[data-vm-id="off-1"]');
  assert.equal(chosen.getAttribute("aria-selected"), "true");
  assert.equal(chosen.classList.contains("selected-row"), true);
  assert.equal(env.document.querySelector('#vm-table tr[data-vm-id="run-1"]').hasAttribute("aria-selected"), false);
  const css = fs.readFileSync(webRoot + "style.css", "utf8");
  assert.match(css, /\.vm-workspace\.is-detail\s*\{[^}]*grid-template-columns:\s*260px minmax\(0,\s*1fr\)/);
  assert.match(css, /\.vm-workspace > \.vm-detail-panel\s*\{[^}]*display:\s*none/);
  assert.match(css, /\.vm-workspace\.is-detail > \.table-wrap\s*\{[^}]*display:\s*none/);
  env.document.getElementById("close-vm-detail").click();
  assert.equal(env.state.selectedVmId, "");
  assert.equal(workspace.classList.contains("is-detail"), false);
  env.state.selectedVmId = "off-1";
  env.renderVms();
  const keydown = (defaultPrevented) => {
    const event = {
      type: "keydown",
      key: "Escape",
      defaultPrevented,
      preventDefault() { this.defaultPrevented = true; },
      stopPropagation() { this.cancelBubble = true; }
    };
    env.document.body.dispatchEvent(event);
  };
  keydown(false);
  assert.equal(env.state.selectedVmId, "");
  assert.equal(workspace.classList.contains("is-detail"), false);
  env.state.selectedVmId = "off-1";
  env.renderVms();
  keydown(true);
  assert.equal(env.state.selectedVmId, "off-1");
  assert.equal(workspace.classList.contains("is-detail"), true);
});

test("VM hash opens a VM and tab, tab changes replace, and an unknown id returns to the list", async () => {
  resetSession(env);
  const calls = [];
  const previousPush = env.history.pushState;
  const previousReplace = env.history.replaceState;
  const previousApi = env.desktopApi;
  env.history.pushState = (_state, _title, url) => { calls.push({ mode: "push", url: String(url) }); };
  env.history.replaceState = (_state, _title, url) => { calls.push({ mode: "replace", url: String(url) }); };
  stubDesktopApi(env, { listVms: async () => env.state.vms.map((vm) => ({ ...vm })) });
  env.state.vmListError = null;
  env.state.partialFailures = [];
  env.state.vms = [{ id: "a b", name: "a b", state: "Off", generation: 2 }];
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.__pcvVmDetailTab = "";
  env.location.hash = "#/vms/" + encodeURIComponent("a b") + "/console";
  env.navigateToHash();
  await new Promise((resolve) => setTimeout(resolve, 250));
  assert.equal(env.state.selectedVmId, "a b");
  assert.equal(env.__pcvVmDetailTab, "console");
  assert.equal(env.location.hash, "#/vms/a%20b/console");
  assert.equal(calls.some((call) => call.mode === "push"), false);
  env.location.hash = "#/vms/" + encodeURIComponent("a b") + "/nope";
  env.__pcvVmHashRoute = { page: "vms", id: "a b", tab: "nope" };
  await env.openResourceById("vms", "a%20b");
  assert.equal(env.__pcvVmDetailTab, "summary");
  assert.equal(env.location.hash, "#/vms/a%20b/summary");
  calls.length = 0;
  env.location.hash = "#/vms";
  env.state.selectedVmId = "";
  env.renderVms();
  env.state.selectedVmId = "a b";
  env.__pcvVmDetailTab = "summary";
  env.renderVms();
  assert.equal(calls.filter((call) => call.mode === "push").length, 1);
  assert.equal(env.location.hash, "#/vms/a%20b/summary");
  const namedRow = [...env.document.querySelectorAll("#vm-table tr[data-vm-id]")].find((row) => row.getAttribute("data-vm-id") === "a b");
  assert.equal(namedRow.querySelector("td").getAttribute("title"), "a b");
  const panel = env.document.createElement("div");
  panel.id = "vm-detail-panel";
  const tab = env.document.createElement("button");
  tab.id = "pcv-tab-jobs-2";
  tab.setAttribute("role", "tab");
  panel.appendChild(tab);
  env.document.body.appendChild(panel);
  calls.length = 0;
  tab.click();
  assert.equal(env.__pcvVmDetailTab, "jobs");
  assert.equal(calls.length, 1);
  assert.equal(calls[0].mode, "replace");
  assert.equal(env.location.hash, "#/vms/a%20b/jobs");
  calls.length = 0;
  env.document.getElementById("close-vm-detail").click();
  assert.equal(env.state.selectedVmId, "");
  assert.equal(env.location.hash, "#/vms");
  assert.equal(calls.every((call) => call.mode === "replace"), true);
  await env.openResourceById("vms", "missing-vm");
  assert.equal(env.state.selectedVmId, "");
  assert.equal(env.location.hash, "#/vms");
  assert.match(env.document.querySelector(".pcv-toast-message").textContent, /요청한 VM이 목록에 없습니다/);
  const css = fs.readFileSync(webRoot + "style.css", "utf8");
  assert.match(css, /\.vm-workspace\.is-detail \.pcv-vm-table \.badge\s*\{[^}]*white-space:\s*nowrap/);
  assert.match(css, /\.vm-workspace\.is-detail \.pcv-vm-table td:nth-child\(1\)\s*\{[^}]*text-overflow:\s*ellipsis/);
  calls.length = 0;
  env.state.selectedVmId = "a b";
  env.state.selectedVm = env.state.vms[0];
  env.state.activeView = "dashboard";
  env.location.hash = "#/dashboard";
  env.render();
  assert.equal(env.location.hash, "#/dashboard");
  assert.equal(calls.length, 0);
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.state.activeView = "dashboard";
  env.history.pushState = previousPush;
  env.history.replaceState = previousReplace;
  env.desktopApi = previousApi;
  env.location.hash = "";
  env.__pcvVmDetailTab = "";
  env.state.vmListError = null;
  env.state.partialFailures = [];
  panel.remove();
});

test("VM list empty state offers create, a filter empty state clears the filter, and a read failure can be retried", async () => {
  resetSession(env);
  env.state.vmListError = null;
  env.state.partialFailures = [];
  env.state.vms = [];
  env.state.vmFilter = "";
  env.state.vmStateFilter = "all";
  env.state.selectedVmId = "";
  env.location.hash = "#/vms";
  env.renderVms();
  const create = env.document.querySelector("#vm-table .pcv-empty-action");
  assert.equal(create.getAttribute("data-shell-action"), "open-create-vm");
  assert.equal(create.textContent, "+ 새 VM");
  assert.equal(env.document.querySelector("#vm-table .pcv-empty-title").textContent, "표시할 VM이 없습니다.");
  assert.match(env.document.querySelector("#vm-table .pcv-empty-body").textContent, /이 노드에 표시할 VM이 없습니다/);
  env.state.vms = [{ id: "off-1", name: "off-1", state: "Off", generation: 2 }];
  env.state.vmFilter = "zzz";
  env.renderVms();
  assert.equal(env.document.querySelector("#vm-table .pcv-empty-title").textContent, "조건에 맞는 VM이 없습니다.");
  const clear = env.document.querySelector("#vm-table .pcv-empty-action");
  assert.equal(clear.textContent, "필터 지우기");
  assert.equal(clear.hasAttribute("data-shell-action"), false);
  clear.click();
  assert.equal(env.state.vmFilter, "");
  assert.equal(env.state.vmStateFilter, "all");
  assert.ok(env.document.querySelector('#vm-table tr[data-vm-id="off-1"]'));
  env.state.vmListError = null;
  env.state.partialFailures = [{ operation: "vm.list", message: "partial" }];
  env.renderVms();
  assert.match(env.document.querySelector("#vm-table .pcv-load-error-detail").textContent, /partial/);
  env.state.partialFailures = [];
  env.state.vmListError = { message: "boom" };
  env.renderVms();
  assert.match(env.document.querySelector("#vm-table .pcv-load-error-detail").textContent, /boom/);
  const previousApi = env.desktopApi;
  let calls = 0;
  stubDesktopApi(env, {
    listVms: async () => {
      calls += 1;
      return [{ id: "off-1", name: "off-1", state: "Off", generation: 2 }];
    }
  });
  env.document.querySelector("#vm-table .pcv-load-error-retry").click();
  await flush();
  assert.equal(calls, 1);
  assert.equal(env.state.vmListError, null);
  assert.ok(env.document.querySelector('#vm-table tr[data-vm-id="off-1"]'));
  env.desktopApi = previousApi;
  env.state.vmListError = null;
  env.state.partialFailures = [];
  env.location.hash = "";
});

test("the templates view lists only locked VMs and shows emptyState when none are locked", () => {
  resetSession(env);
  env.state.vms = [
    { id: "run-1", name: "run-1", state: "Running", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
    { id: "lock-1", name: "lock-1", state: "Off", template_lock: true, generation: 2, cpu: { count: 4 }, memory: { startup_mb: 4096 } }
  ];
  env.renderVms();
  const rows = env.document.querySelectorAll("#template-table tr[data-vm-id]");
  assert.equal(rows.length, 1);
  assert.equal(rows[0].getAttribute("data-vm-id"), "lock-1");
  const actions = [...rows[0].querySelectorAll(".pcv-row-action")].map((button) => button.getAttribute("data-action"));
  assert.deepEqual(hostValue(actions), ["vm-row-clone", "vm-template-unlock"]);
  env.state.vms = [{ id: "run-1", name: "run-1", state: "Running" }];
  env.renderVms();
  assert.equal(env.document.querySelectorAll("#template-table tr[data-vm-id]").length, 0);
  assert.equal(env.document.querySelector("#template-table .pcv-empty-title").textContent, "잠긴 템플릿이 없습니다");
  assert.match(env.document.querySelector("#template-table .pcv-empty-body").textContent, /VM 상세 → 더 보기 → 템플릿으로 잠금/);
});

test("account create validation does not call the API until username, password, and confirmation pass", async () => {
  resetSession(env);
  const calls = [];
  stubDesktopApi(env, {
    createAccount: async (body) => {
      calls.push(JSON.parse(JSON.stringify(body)));
      return { username: body.username };
    }
  });
  env.openAccountCreateModal();
  const form = env.document.getElementById("account-create-form");
  const set = (name, value) => { form.querySelector(`[name="${name}"]`).value = value; };
  const submit = async () => {
    form.dispatchEvent({ type: "submit", preventDefault() {}, stopPropagation() { this.cancelBubble = true; }, target: form });
    await flush();
  };
  set("username", "1bad");
  set("role", "viewer");
  set("password", SECRET);
  set("password_confirm", SECRET);
  await submit();
  set("username", "loopback-session");
  await submit();
  set("username", "ada");
  set("password", "short-pass");
  set("password_confirm", "short-pass");
  await submit();
  set("username", "longusername");
  set("password", "longusername");
  set("password_confirm", "longusername");
  await submit();
  set("password", SECRET);
  set("password_confirm", "S3cret-pass-2");
  await submit();
  assert.equal(calls.length, 0);
  assert.equal(env.document.querySelector(".pcv-confirm"), null);
});

test("a valid account create sends username, password, and role, then clears the password from the DOM", async () => {
  resetSession(env);
  const calls = [];
  stubDesktopApi(env, {
    createAccount: async (body) => {
      calls.push(JSON.parse(JSON.stringify(body)));
      return { username: body.username, role: body.role };
    }
  });
  env.openAccountCreateModal();
  const form = env.document.getElementById("account-create-form");
  form.querySelector('[name="username"]').value = "ada";
  form.querySelector('[name="role"]').value = "operator";
  form.querySelector('[name="password"]').value = SECRET;
  form.querySelector('[name="password_confirm"]').value = SECRET;
  form.dispatchEvent({ type: "submit", preventDefault() {}, stopPropagation() { this.cancelBubble = true; }, target: form });
  await flush();
  const accept = env.document.querySelector('[data-pcv-confirm="accept"]');
  assert.equal(accept.classList.contains("pcv-confirm-danger"), false);
  accept.click();
  await flush();
  assert.deepEqual(calls, [{ username: "ada", password: SECRET, role: "operator" }]);
  const leftovers = env.document.querySelectorAll('input[type="password"]').filter((input) => input.value === SECRET || input.getAttribute("value") === SECRET);
  assert.equal(leftovers.length, 0);
  assert.equal(env.document.body.textContent.includes(SECRET), false);
  assert.equal(JSON.stringify(env.state.accountDirectoryError || {}).includes(SECRET), false);
});

test("disable uses a danger confirmDialog and shows PCV_ACCOUNT_LAST_ADMIN", async () => {
  resetSession(env);
  env.state.accountDirectory = [{ username: "root", role: "admin", enabled: true }];
  stubDesktopApi(env, {
    disableAccount: async () => {
      throw { code: "PCV_ACCOUNT_LAST_ADMIN", status: 409, message: "The last enabled admin cannot be disabled.", detail: "Create another admin before disabling this account." };
    }
  });
  env.render();
  env.document.querySelector('[data-action="account-disable"]').click();
  await flush();
  const accept = env.document.querySelector(".pcv-confirm .pcv-confirm-danger");
  assert.ok(accept, "disable confirmation is danger");
  assert.equal(accept.getAttribute("data-pcv-confirm"), "accept");
  accept.click();
  await flush();
  assert.match(env.document.querySelector("#accounts-panel").textContent, /PCV_ACCOUNT_LAST_ADMIN/);
  assert.match(env.document.querySelector("#accounts-panel").textContent, /마지막 관리자 계정은 끌 수 없습니다/);
});

function passwordLeftovers() {
  return env.document.querySelectorAll('input[type="password"]').filter((input) => input.value === SECRET || input.getAttribute("value") === SECRET);
}

function submitForm(form) {
  form.dispatchEvent({ type: "submit", preventDefault() {}, stopPropagation() { this.cancelBubble = true; }, target: form });
  return flush();
}

test("password reset is an admin row action and password change follows the signed-in account", () => {
  resetSession(env);
  env.state.accountDirectory = [
    { username: "ada", role: "admin", enabled: true },
    { username: "op", role: "operator", enabled: false }
  ];
  env.render();
  const change = () => env.document.querySelector('[data-action="account-change-password"]');
  const panelChange = () => env.document.querySelector("#accounts-panel").querySelector('[data-action="account-change-password"]');
  assert.equal(panelChange(), null);
  assert.equal(change().hidden, true);
  assert.equal(env.document.querySelector("#account-session-panel").querySelector('[data-action="account-change-password"]'), null);
  assert.equal(env.document.querySelectorAll('[data-action="account-reset-password"]').length, 2);
  assert.equal(env.document.querySelector('[data-action="account-reset-password"][data-username="op"]').disabled, false);

  env.state.authAccessToken = "jwt";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "op", role: "operator", permissions: ["operate"] };
  env.render();
  assert.match(env.document.querySelector("#accounts-panel").textContent, /계정 권한이 없습니다/);
  assert.equal(env.document.querySelector('[data-action="account-reset-password"]'), null);
  assert.equal(panelChange(), null);
  assert.equal(change().hidden, false);
  assert.equal(change().textContent, "비밀번호 변경");
  assert.equal(change().parentNode.id, "shell-topbar");

  env.state.authSession = { username: "ada", role: "admin", permissions: ["account.manage"] };
  env.render();
  assert.equal(env.document.querySelectorAll('[data-action="account-reset-password"]').length, 2);
  assert.equal(panelChange(), null);
  assert.equal(change().hidden, false);
  assert.equal(change().textContent, "비밀번호 변경");
});

test("password reset modal shows the target, sends confirm_username, and clears the fields", async () => {
  resetSession(env);
  env.state.authAccessToken = "jwt";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "root", role: "admin", permissions: ["account.manage"] };
  env.state.accountDirectory = [{ username: "ada", role: "operator", enabled: true }];
  const calls = [];
  stubDesktopApi(env, {
    resetAccountPassword: async (username, body) => {
      calls.push({ username, body: JSON.parse(JSON.stringify(body)) });
      return { username, enabled: true };
    }
  });
  env.render();
  env.document.querySelector('[data-action="account-reset-password"]').click();
  const form = env.document.getElementById("account-reset-form");
  assert.match(form.textContent, /대상 계정: ada/);
  assert.match(form.textContent, /12자 이상/);
  form.querySelector('[name="password"]').value = "short";
  form.querySelector('[name="password_confirm"]').value = "short";
  await submitForm(form);
  assert.equal(calls.length, 0);
  form.querySelector('[name="password"]').value = SECRET;
  form.querySelector('[name="password_confirm"]').value = "S3cret-pass-2";
  await submitForm(form);
  assert.equal(calls.length, 0);
  form.querySelector('[name="password_confirm"]').value = SECRET;
  await submitForm(form);
  assert.deepEqual(calls, [{ username: "ada", body: { confirm_username: "ada", new_password: SECRET } }]);
  assert.equal(passwordLeftovers().length, 0);
  assert.equal(env.document.body.textContent.includes(SECRET), false);
  assert.equal(env.document.querySelector(".pcv-confirm-danger"), null);
});

test("resetting the signed-in account clears the session and asks for login", async () => {
  resetSession(env);
  env.state.authAccessToken = "jwt";
  env.state.authRefreshToken = "refresh";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "ada", role: "admin", permissions: ["account.manage"] };
  env.state.accountDirectory = [{ username: "ada", role: "admin", enabled: true }];
  stubDesktopApi(env, {
    resetAccountPassword: async () => ({ username: "ada", enabled: true })
  });
  env.render();
  env.document.querySelector('[data-action="account-reset-password"]').click();
  const form = env.document.getElementById("account-reset-form");
  form.querySelector('[name="password"]').value = SECRET;
  form.querySelector('[name="password_confirm"]').value = SECRET;
  await submitForm(form);
  assert.equal(env.state.authAccessToken, "");
  assert.equal(env.state.authRefreshToken, "");
  assert.equal(env.state.authSession, null);
  assert.match(env.state.tokenActionMessage, /다시 로그인/);
  assert.equal(passwordLeftovers().length, 0);
  assert.equal(env.document.body.textContent.includes(SECRET), false);
});

test("password change sends current and new passwords, then stores the returned session", async () => {
  resetSession(env);
  env.state.authAccessToken = "old-access";
  env.state.authRefreshToken = "old-refresh";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "ada", role: "admin", permissions: ["account.manage"] };
  env.state.accountDirectory = [{ username: "ada", role: "admin", enabled: true }];
  const calls = [];
  stubDesktopApi(env, {
    changeAccountPassword: async (body) => {
      calls.push(JSON.parse(JSON.stringify(body)));
      return {
        access_token: "new-access",
        refresh_token: "new-refresh",
        session: { username: "ada", role: "admin", permissions: ["account.manage"] }
      };
    }
  });
  env.render();
  env.document.querySelector('[data-action="account-change-password"]').click();
  const form = env.document.getElementById("account-change-form");
  form.querySelector('[name="password"]').value = SECRET;
  form.querySelector('[name="password_confirm"]').value = SECRET;
  await submitForm(form);
  assert.equal(calls.length, 0);
  form.querySelector('[name="current_password"]').value = SECRET;
  await submitForm(form);
  assert.deepEqual(calls, [{ current_password: SECRET, new_password: SECRET }]);
  assert.equal(env.state.authAccessToken, "new-access");
  assert.equal(env.state.authRefreshToken, "new-refresh");
  assert.equal(env.state.authSession.username, "ada");
  assert.equal(passwordLeftovers().length, 0);
  assert.equal(env.document.body.textContent.includes(SECRET), false);
  assert.equal(JSON.stringify(env.state.accountDirectoryError || {}).includes(SECRET), false);
});

test("operator session shows the shell password change entry and submits current and new passwords", async () => {
  resetSession(env);
  env.state.apiToken = "";
  env.state.authAccessToken = "old-access";
  env.state.authRefreshToken = "old-refresh";
  env.state.runtimePolicy = { auth: { rbac: true, mode: "account" } };
  env.state.authSession = { username: "op", role: "operator", permissions: ["operate"] };
  const calls = [];
  stubDesktopApi(env, {
    changeAccountPassword: async (body) => {
      calls.push(JSON.parse(JSON.stringify(body)));
      return {
        access_token: "new-access",
        refresh_token: "new-refresh",
        session: { username: "op", role: "operator", permissions: ["operate"] }
      };
    }
  });
  env.render();
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  assert.match(index, /id="us"[\s\S]*id="account-change-password"[\s\S]*doLogout\(\)/);
  const button = env.document.querySelector('[data-action="account-change-password"]');
  assert.equal(button.hidden, false);
  assert.equal(button.textContent, "비밀번호 변경");
  assert.equal(env.document.querySelector("#accounts-panel").querySelector('[data-action="account-change-password"]'), null);
  assert.equal(env.document.querySelector('#shell-sidebar [data-nav="accounts"]').hidden, true);
  button.click();
  const form = env.document.getElementById("account-change-form");
  assert.ok(form);
  form.querySelector('[name="current_password"]').value = SECRET;
  form.querySelector('[name="password"]').value = SECRET;
  form.querySelector('[name="password_confirm"]').value = SECRET;
  await submitForm(form);
  assert.deepEqual(calls, [{ current_password: SECRET, new_password: SECRET }]);
  assert.equal(env.state.authAccessToken, "new-access");
  assert.equal(env.state.authSession.username, "op");
  assert.equal(passwordLeftovers().length, 0);
  assert.equal(env.document.body.textContent.includes(SECRET), false);
});

test("service bearer and loopback sessions hide the password change entry", () => {
  resetSession(env);
  env.state.apiToken = "service-bearer-token";
  env.state.authAccessToken = "";
  env.state.authRefreshToken = "";
  env.state.authSession = null;
  env.render();
  assert.equal(env.document.querySelector('[data-action="account-change-password"]').hidden, true);

  env.state.apiToken = "";
  env.state.authAccessToken = "loopback-jwt";
  env.state.authSession = { username: "loopback-session", role: "admin", permissions: ["*"] };
  env.render();
  assert.equal(env.document.querySelector('[data-action="account-change-password"]').hidden, true);
  assert.equal(env.document.querySelector("#account-session-panel").querySelector('[data-action="account-change-password"]'), null);

  env.state.authSession = { username: "LOOPBACK-SESSION", role: "admin", permissions: ["*"] };
  env.render();
  assert.equal(env.document.querySelector('[data-action="account-change-password"]').hidden, true);
  env.state.apiToken = "";
  env.state.authAccessToken = "";
  env.state.authSession = null;
});

function bootVmDeleteList(nextJob) {
  const sandbox = createEnv();
  const listError = new Error("parity");
  listError.code = "PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE";
  sandbox.listCalls = 0;
  sandbox.loadOpsSummary = async () => {};
  sandbox.loadServerJobs = async () => {};
  sandbox.refreshSelectedVm = async () => {};
  sandbox.loadVms = async () => {
    sandbox.listCalls += 1;
    throw listError;
  };
  sandbox.desktopApi = {
    getJob: async () => nextJob,
    listVms: async () => { throw listError; }
  };
  sandbox.state.vms = [
    { id: "keep-id", name: "keep" },
    { id: "gone-id", name: "probe" }
  ];
  sandbox.state.selectedVmId = "gone-id";
  sandbox.state.selectedVm = { id: "gone-id", name: "probe" };
  sandbox.state.selectedVmCheckpoints = [{ name: "cp" }];
  sandbox.state.selectedVmReadbacks = { vm_id: "probe" };
  sandbox.state.selectedVmQosControl = { pending: true };
  sandbox.state.jobPollDelayMs = 2000;
  sandbox.state.pollTimer = null;
  sandbox.state.error = null;
  return sandbox;
}

test("BL-0034 drops the deleted VM row and selection when vm.delete succeeds and the next list fails", async () => {
  const succeeded = { job_id: "job-1", operation: "vm.delete", status: "succeeded", params: { name: "probe" } };
  const sandbox = bootVmDeleteList(succeeded);
  sandbox.state.trackedJobs = [{ job_id: "job-1", operation: "vm.delete", status: "queued", params: { name: "probe" } }];

  await sandbox.runPollTick();

  assert.equal(sandbox.listCalls, 1);
  assert.equal(sandbox.state.vms.map((row) => row.name).join(","), "keep");
  assert.equal(sandbox.state.selectedVmId, "");
  assert.equal(sandbox.state.selectedVm, null);
  assert.equal(sandbox.state.selectedVmCheckpoints.length, 0);
  assert.equal(sandbox.state.selectedVmReadbacks, null);
  assert.equal(sandbox.state.selectedVmQosControl, null);
  assert.equal(sandbox.state.error.code, "PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE");
});

test("BL-0034 keeps the VM row when the tracked vm.delete does not succeed", async () => {
  const failed = { job_id: "job-1", operation: "vm.delete", status: "failed", params: { name: "probe" } };
  const sandbox = bootVmDeleteList(failed);
  sandbox.state.trackedJobs = [{ job_id: "job-1", operation: "vm.delete", status: "queued", params: { name: "probe" } }];

  await sandbox.runPollTick();

  assert.equal(sandbox.state.vms.map((row) => row.name).join(","), "keep,probe");
  assert.equal(sandbox.state.selectedVmId, "gone-id");
  assert.equal(sandbox.state.selectedVm.name, "probe");
});

test("BL-0034 keeps the VM row when a different tracked job succeeds", async () => {
  const started = { job_id: "job-2", operation: "vm.start", status: "succeeded", params: { name: "probe" } };
  const sandbox = bootVmDeleteList(started);
  sandbox.state.trackedJobs = [{ job_id: "job-2", operation: "vm.start", status: "queued", params: { name: "probe" } }];

  await sandbox.runPollTick();

  assert.equal(sandbox.state.vms.map((row) => row.name).join(","), "keep,probe");
  assert.equal(sandbox.state.selectedVmId, "gone-id");
});

test("mobile power and delete call exported vm methods", () => {
  const mobile = fs.readFileSync(webRoot + "src/modules/mobile.ts", "utf8");
  const vmSource = fs.readFileSync(webRoot + "src/modules/vm.ts", "utf8");
  const exportLine = vmSource.slice(vmSource.lastIndexOf("PCV.vm = Object.assign"));
  const names = new Set();
  for (const match of mobile.matchAll(/PCV\.vm\.([A-Za-z0-9_]+)/g)) names.add(match[1]);
  assert.ok(names.size > 0);
  for (const name of names) assert.match(exportLine, new RegExp("\\b" + name + "\\b"));
  assert.equal(names.has("bulkDelete"), false);
  assert.equal(names.has("vmDel"), false);
  assert.equal(names.has("lifecycle"), false);
  assert.match(mobile, /queueVmLifecycle/);
  assert.match(mobile, /queueVmDelete/);
  assert.doesNotMatch(mobile, /m-vm-bulk-delete|m-vm-select/);
});

test("logout closes open modals", async () => {
  if (!env.document.getElementById("toasts")) {
    const toasts = env.document.createElement("div");
    toasts.id = "toasts";
    env.document.body.appendChild(toasts);
  }
  env.PCV.modalCore.open("one");
  env.PCV.modalCore.open("two");
  const openCount = () => [...env.document.querySelectorAll("dialog")].filter((dialog) => dialog.open).length;
  assert.equal(openCount(), 2);
  await env.doLogout();
  assert.equal(openCount(), 0);
});

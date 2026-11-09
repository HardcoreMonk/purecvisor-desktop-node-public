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
  const detailPanel = document.createElement("aside");
  detailPanel.id = "vm-detail-panel";
  detailPanel.appendChild(detailTitle);
  detailPanel.appendChild(detailContent);
  body.appendChild(detailPanel);
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
  sandbox.els.vmDetailPanel = detailPanel;
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
  assert.equal(namedRow.querySelector("td[title]").getAttribute("title"), "a b");
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

function vmRowMenuFixture() {
  resetSession(env);
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.state.connectionState = "idle";
  env.state.vms = [
    { id: "off-1", name: "off-1", state: "Off", generation: 2, cpu: { count: 1 }, memory: { startup_mb: 1024 } },
    { id: "lock-1", name: "lock-1", state: "Running", template_lock: true, generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } }
  ];
  env.renderVms();
}

function vmRowMenuButton(id) {
  return env.document.querySelector(`#vm-table tr[data-vm-id="${id}"] [data-action="vm-row-menu"]`);
}

function vmRowMenu() {
  return env.document.getElementById("pcv-vm-row-menu");
}

function vmRowMenuItems() {
  return [...vmRowMenu().querySelectorAll('[role="menuitem"]')];
}

function keyOn(node, key, extra) {
  const event = new env.Event("keydown");
  event.key = key;
  if (extra) Object.assign(event, extra);
  node.dispatchEvent(event);
  return event;
}

test("the VM row menu opens from the ellipsis without selecting the row", () => {
  vmRowMenuFixture();
  const button = vmRowMenuButton("off-1");
  assert.equal(button.getAttribute("aria-haspopup"), "menu");
  assert.equal(button.getAttribute("aria-expanded"), "false");
  assert.match(button.getAttribute("aria-label"), /off-1/);
  assert.match(button.textContent, /⋯/);
  button.click();
  const menu = vmRowMenu();
  assert.equal(env.state.selectedVmId, "");
  assert.equal(env.state.selectedVm, null);
  assert.equal(menu.hidden, false);
  assert.equal(menu.getAttribute("role"), "menu");
  assert.equal(button.getAttribute("aria-expanded"), "true");
  const items = vmRowMenuItems().filter((item) => !item.disabled);
  assert.equal(env.document.activeElement, items[0]);
  assert.equal(items[0].getAttribute("data-action"), "vm-clone-open");
  // A periodic render rebuilds the rows but keeps the open menu on the rebuilt toggle and item.
  env.render();
  assert.equal(vmRowMenu().hidden, false);
  assert.equal(vmRowMenuButton("off-1").getAttribute("aria-expanded"), "true");
  assert.equal(env.document.activeElement.getAttribute("data-action"), "vm-clone-open");
  assert.equal(env.state.selectedVmId, "");
  // The menu closes once its VM is no longer listed.
  env.state.vms = env.state.vms.filter((vm) => vm.id !== "off-1");
  env.render();
  assert.equal(vmRowMenu().hidden, true);
});

test("the VM row menu opens from contextmenu and Shift+F10 and moves with arrows", () => {
  vmRowMenuFixture();
  const row = env.document.querySelector('#vm-table tr[data-vm-id="off-1"]');
  const context = new env.Event("contextmenu");
  context.clientX = 15;
  context.clientY = 25;
  row.dispatchEvent(context);
  assert.equal(context.defaultPrevented, true);
  assert.equal(env.state.selectedVmId, "");
  const menu = vmRowMenu();
  assert.equal(menu.hidden, false);
  assert.equal(menu.style.left, "15px");
  assert.equal(menu.style.top, "25px");
  env.renderVms();
  const primary = env.document.querySelector('#vm-table tr[data-vm-id="off-1"] .pcv-row-action');
  primary.focus();
  const f10 = keyOn(primary, "F10", { shiftKey: true });
  assert.equal(f10.defaultPrevented, true);
  assert.equal(vmRowMenu().hidden, false);
  assert.equal(env.state.selectedVmId, "");
  const items = vmRowMenuItems().filter((item) => !item.disabled);
  assert.equal(env.document.activeElement, items[0]);
  const down = keyOn(items[0], "ArrowDown");
  assert.equal(down.defaultPrevented, true);
  assert.equal(env.document.activeElement, items[1]);
  const end = keyOn(env.document.activeElement, "End");
  assert.equal(end.defaultPrevented, true);
  assert.equal(env.document.activeElement, items[items.length - 1]);
  const home = keyOn(env.document.activeElement, "Home");
  assert.equal(home.defaultPrevented, true);
  assert.equal(env.document.activeElement, items[0]);
  const up = keyOn(items[0], "ArrowUp");
  assert.equal(up.defaultPrevented, true);
  assert.equal(env.document.activeElement, items[items.length - 1]);
  env.renderVms();
  const again = env.document.querySelector('#vm-table tr[data-vm-id="off-1"] .pcv-row-action');
  again.focus();
  keyOn(again, "ContextMenu");
  assert.equal(vmRowMenu().hidden, false);
  const tab = keyOn(env.document.activeElement, "Tab");
  assert.equal(vmRowMenu().hidden, true);
  assert.equal(tab.defaultPrevented, undefined);
});

test("Escape closes the VM row menu, restores focus, and keeps the selection", () => {
  vmRowMenuFixture();
  env.state.selectedVmId = "off-1";
  env.state.selectedVm = env.state.vms[0];
  env.renderVms();
  const button = vmRowMenuButton("off-1");
  button.focus();
  button.click();
  assert.equal(vmRowMenu().hidden, false);
  const escape = keyOn(env.document.activeElement, "Escape");
  assert.equal(escape.defaultPrevented, true);
  assert.equal(vmRowMenu().hidden, true);
  assert.equal(env.document.activeElement, button);
  assert.equal(env.state.selectedVmId, "off-1");
  assert.equal(button.getAttribute("aria-expanded"), "false");
});

test("a template-locked VM disables force poweroff and delete in the row menu", () => {
  vmRowMenuFixture();
  vmRowMenuButton("lock-1").click();
  const item = (action) => vmRowMenuItems().find((node) => node.getAttribute("data-action") === action);
  assert.equal(item("vm-poweroff").disabled, true);
  assert.equal(item("vm-delete").disabled, true);
  assert.equal(item("vm-clone-open").disabled, false);
  assert.equal(item("vm-template-unlock").disabled, false);
  assert.equal(item("vm-template-lock"), undefined);
  const before = env.state.selectedVmId;
  item("vm-poweroff").click();
  assert.equal(env.state.selectedVmId, before);
  assert.equal(vmRowMenu().hidden, false);
});

test("an enabled VM row menu item selects that VM and clicks the same detail action", () => {
  vmRowMenuFixture();
  const calls = [];
  const original = env.openVmCloneModal;
  env.openVmCloneModal = (id) => { calls.push(id); };
  try {
    vmRowMenuButton("off-1").click();
    const item = vmRowMenuItems().find((node) => node.getAttribute("data-action") === "vm-clone-open");
    item.click();
    assert.equal(env.state.selectedVmId, "off-1");
    assert.equal(env.state.selectedVm.id, "off-1");
    assert.equal(env.state.connectionState, "connected");
    assert.deepEqual(calls, ["off-1"]);
  } finally {
    env.openVmCloneModal = original;
  }
});

function vmSortIds() {
  return [...env.document.querySelectorAll("#vm-table tbody tr[data-vm-id]")].map((row) => row.getAttribute("data-vm-id"));
}

function vmSortFixture() {
  resetSession(env);
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.state.connectionState = "idle";
  env.state.vmFilter = "";
  env.state.vmStateFilter = "all";
  env.state.vmSort = "name";
  env.state.vmSortDir = "asc";
  env.state.vms = [
    { id: "mid", name: "mid", state: "Running", generation: 2, cpu: { count: 4 }, memory: { startup_mb: 512 }, updated_at: "2026-01-02T00:00:00Z" },
    { id: "low", name: "aaa", state: "Off", generation: 1, cpu: { count: 1 }, memory: { startup_mb: 4096 }, updated_at: "2026-01-03T00:00:00Z" },
    { id: "none", name: "zzz", state: "Off", updated_at: "2026-01-01T00:00:00Z" }
  ];
  if (!env.els.vmSort) {
    const select = env.document.createElement("select");
    select.id = "vm-sort";
    env.document.body.appendChild(select);
    env.els.vmSort = select;
  }
  env.renderVms();
}

test("column headers sort, toggle direction, set aria-sort, and mirror the select", () => {
  vmSortFixture();
  assert.deepEqual(vmSortIds(), ["low", "mid", "none"]);
  assert.equal(env.document.querySelector('#vm-table button[data-vm-sort="name"]').parentNode.getAttribute("aria-sort"), "ascending");
  assert.equal(env.document.querySelector('#vm-table button[data-vm-sort="memory"]').parentNode.getAttribute("aria-sort"), "none");
  const heads = [...env.document.querySelectorAll("#vm-table thead th")];
  assert.equal(heads[heads.length - 1].querySelector("button"), null);
  env.document.querySelector('#vm-table button[data-vm-sort="memory"]').click();
  assert.equal(env.state.selectedVmId, "");
  assert.equal(env.state.vmSort, "memory");
  assert.equal(env.state.vmSortDir, "asc");
  assert.equal(env.els.vmSort.value, "memory");
  assert.equal(env.document.querySelector('#vm-table button[data-vm-sort="memory"]').parentNode.getAttribute("aria-sort"), "ascending");
  assert.deepEqual(vmSortIds(), ["mid", "low", "none"]);
  env.document.querySelector('#vm-table button[data-vm-sort="memory"]').click();
  assert.equal(env.state.vmSortDir, "desc");
  assert.equal(env.document.querySelector('#vm-table button[data-vm-sort="memory"]').parentNode.getAttribute("aria-sort"), "descending");
  assert.deepEqual(vmSortIds(), ["low", "mid", "none"]);
  env.els.vmSort.value = "state";
  env.els.vmSort.dispatchEvent(new env.Event("change"));
  assert.equal(env.state.vmSort, "state");
  assert.equal(env.state.vmSortDir, "asc");
  assert.deepEqual(vmSortIds(), ["low", "none", "mid"]);
  env.state.vmSort = "name";
  env.state.vmSortDir = "asc";
  env.renderVms();
  assert.deepEqual(vmSortIds(), ["low", "mid", "none"]);
  env.state.vmSort = "state";
  env.state.vmSortDir = "asc";
  env.renderVms();
  assert.deepEqual(vmSortIds(), ["low", "none", "mid"]);
  env.state.vmSort = "updated";
  env.state.vmSortDir = "desc";
  env.renderVms();
  assert.deepEqual(vmSortIds(), ["low", "mid", "none"]);
});

test("j and k move the VM row selection and focus, and ignore a search field and Ctrl+j", async () => {
  vmSortFixture();
  const calls = [];
  env.loadVmDetail = async (id) => {
    calls.push(id);
    env.state.selectedVmId = id;
    env.state.selectedVm = env.state.vms.find((vm) => vm.id === id);
  };
  const rows = () => [...env.document.querySelectorAll("#vm-table tbody tr[data-vm-id]")];
  assert.equal(rows()[0].getAttribute("tabindex"), "-1");
  rows()[0].focus();
  keyOn(rows()[0], "j");
  await flush();
  assert.deepEqual(calls, ["low"]);
  assert.equal(env.state.selectedVmId, "low");
  assert.equal(env.state.connectionState, "connected");
  assert.equal(env.document.activeElement, rows()[0]);
  keyOn(env.document.activeElement, "j");
  await flush();
  assert.deepEqual(calls, ["low", "mid"]);
  assert.equal(env.state.selectedVmId, "mid");
  assert.equal(env.document.activeElement, rows()[1]);
  keyOn(env.document.activeElement, "k");
  await flush();
  assert.equal(env.state.selectedVmId, "low");
  assert.equal(env.document.activeElement, rows()[0]);
  keyOn(rows()[0], "k");
  await flush();
  assert.equal(env.state.selectedVmId, "low");
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  keyOn(rows()[0], "k");
  await flush();
  assert.equal(env.state.selectedVmId, "none");
  const before = calls.length;
  const input = env.document.createElement("input");
  input.id = "vm-filter";
  env.document.body.appendChild(input);
  input.focus();
  keyOn(input, "j");
  await flush();
  assert.equal(calls.length, before);
  assert.equal(env.state.selectedVmId, "none");
  rows()[0].focus();
  keyOn(rows()[0], "j", { ctrlKey: true });
  await flush();
  assert.equal(calls.length, before);
  assert.equal(env.state.selectedVmId, "none");
});

test("VM checkboxes select visible rows, keep detail closed, and bulk start or shut down", async () => {
  env.state.vms = [
    { id: "off-1", name: "off-1", state: "Off" },
    { id: "off-2", name: "off-2", state: "Off" },
    { id: "run-1", name: "run-1", state: "Running" },
    { id: "lock-1", name: "lock-1", state: "Off", template_lock: true }
  ];
  env.state.vmChecked = [];
  env.state.vmFilter = "";
  env.state.vmStateFilter = "all";
  env.state.vmSort = "name";
  env.state.vmSortDir = "asc";
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.state.authAccessToken = "";
  env.state.authSession = null;
  env.state.pendingVmActions = {};
  env.state.trackedJobs = [];
  const previousLoad = env.loadVmDetail;
  const previousPoll = env.startPolling;
  const previousApi = env.desktopApi;
  const previousTrack = env.PCV.components.trackJob;
  const detailCalls = [];
  const actionCalls = [];
  const trackedJobs = [];
  env.loadVmDetail = async (id) => { detailCalls.push(id); };
  env.startPolling = () => {};
  env.PCV.components.trackJob = (jobId, onUpdate) => {
    trackedJobs.push(jobId);
    if (typeof onUpdate === "function") onUpdate({ job_id: jobId, status: "running" });
    return () => {};
  };
  stubDesktopApi(env, {
    queueVmAction: async (id, action) => {
      actionCalls.push([id, action]);
      return { job_id: "job-" + action + "-" + id, status: "queued" };
    },
    getJob: async (id) => ({ job_id: id, status: "succeeded" })
  });
  env.renderVms();

  assert.equal(env.document.querySelector("[data-vm-bulk]"), null);
  assert.ok(env.document.querySelector('#vm-table button[data-vm-sort="name"]'));
  const rowBox = env.document.querySelector('input[data-vm-check="row"][data-vm-id="off-1"]');
  assert.equal(rowBox.getAttribute("aria-label"), "off-1 선택");
  rowBox.checked = true;
  rowBox.click();
  await flush();
  assert.deepEqual(detailCalls, []);
  assert.equal(env.state.selectedVmId, "");
  assert.deepEqual(env.state.vmChecked, ["off-1"]);
  assert.match(env.document.querySelector("[data-vm-bulk]").textContent, /선택 1개/);
  const allBox = env.document.querySelector('input[data-vm-check="all"]');
  assert.equal(allBox.getAttribute("aria-label"), "보이는 VM 모두 선택");
  assert.equal(allBox.indeterminate, true);
  assert.equal(allBox.checked, false);

  allBox.click();
  await flush();
  assert.deepEqual(detailCalls, []);
  assert.deepEqual([...env.state.vmChecked].sort(), ["lock-1", "off-1", "off-2", "run-1"]);
  const allAfter = env.document.querySelector('input[data-vm-check="all"]');
  assert.equal(allAfter.indeterminate, false);
  assert.equal(allAfter.checked, true);
  assert.match(env.document.querySelector("[data-vm-bulk]").textContent, /선택 4개/);
  assert.match(env.document.querySelector("[data-vm-bulk]").textContent, /건너뜀 1/);

  env.document.querySelector('[data-vm-bulk-action="start"]').click();
  await flush();
  assert.deepEqual(actionCalls, [["off-1", "start"], ["off-2", "start"]]);
  assert.equal(env.document.querySelectorAll("[data-vm-bulk-line]").length, 2);

  env.document.querySelector('[data-vm-bulk-action="shutdown"]').click();
  await flush();
  const message = env.document.querySelector(".pcv-confirm .pcv-confirm-message");
  assert.ok(message);
  assert.match(message.textContent, /run-1/);
  env.document.querySelector('.pcv-confirm [data-pcv-confirm="accept"]').click();
  await flush();
  assert.deepEqual(actionCalls, [["off-1", "start"], ["off-2", "start"], ["run-1", "shutdown"]]);
  assert.equal(env.document.querySelectorAll("[data-vm-bulk-line]").length, actionCalls.length);
  assert.equal(trackedJobs.length, actionCalls.length);

  env.state.authAccessToken = "jwt";
  env.state.authSession = { role: "viewer", permissions: ["read"] };
  env.renderVms();
  assert.equal(env.document.querySelector('[data-vm-bulk-action="start"]').disabled, true);
  assert.equal(env.document.querySelector('[data-vm-bulk-action="shutdown"]').disabled, true);
  assert.equal(env.document.querySelector('[data-vm-bulk-action="clear"]').disabled, false);
  env.document.querySelector('[data-vm-bulk-action="clear"]').click();
  await flush();
  assert.equal(env.state.vmChecked.length, 0);
  assert.equal(env.document.querySelector("[data-vm-bulk]"), null);

  env.loadVmDetail = previousLoad;
  env.startPolling = previousPoll;
  env.desktopApi = previousApi;
  env.PCV.components.trackJob = previousTrack;
  env.state.authAccessToken = "";
  env.state.authSession = null;
  env.state.pendingVmActions = {};
  env.state.trackedJobs = [];
  if (env.state.pollTimer) {
    clearTimeout(env.state.pollTimer);
    env.state.pollTimer = null;
  }
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

test("bulk delete confirms the target count, skips running and locked VMs, and uses one deleteVm each", async () => {
  env.state.vms = [
    { id: "off-1", name: "off-1", state: "Off" },
    { id: "off-2", name: "off-2", state: "Off" },
    { id: "run-1", name: "run-1", state: "Running" },
    { id: "lock-1", name: "lock-1", state: "Off", template_lock: true }
  ];
  env.state.vmChecked = ["off-1", "off-2", "run-1", "lock-1"];
  env.state.vmFilter = "";
  env.state.vmStateFilter = "all";
  env.state.vmSort = "name";
  env.state.vmSortDir = "asc";
  env.state.selectedVmId = "";
  env.state.selectedVm = null;
  env.state.authAccessToken = "";
  env.state.authSession = null;
  env.state.pendingVmActions = {};
  env.state.trackedJobs = [];
  const previousPoll = env.startPolling;
  const previousApi = env.desktopApi;
  const previousTrack = env.PCV.components.trackJob;
  const deleteCalls = [];
  env.startPolling = () => {};
  env.PCV.components.trackJob = (jobId, onUpdate) => {
    if (typeof onUpdate === "function") onUpdate({ job_id: jobId, status: "running" });
    return () => {};
  };
  stubDesktopApi(env, {
    deleteVm: async (id) => {
      deleteCalls.push(id);
      return { job_id: "job-delete-" + id, status: "queued" };
    },
    getJob: async (id) => ({ job_id: id, status: "succeeded" })
  });
  env.renderVms();
  const button = env.document.querySelector('[data-vm-bulk-action="delete"]');
  assert.equal(button.classList.contains("pcv-menu-danger"), true);
  assert.equal(button.disabled, false);
  assert.match(env.document.querySelector("[data-vm-bulk]").textContent, /삭제 건너뜀 2/);
  assert.match(env.document.querySelector("[data-vm-bulk]").textContent, /건너뜀 1/);

  button.click();
  await flush();
  const message = env.document.querySelector(".pcv-confirm-message");
  assert.match(message.textContent, /off-1/);
  assert.match(message.textContent, /off-2/);
  assert.doesNotMatch(message.textContent, /run-1/);
  assert.doesNotMatch(message.textContent, /lock-1/);
  const typed = env.document.querySelector(".pcv-confirm-typed");
  const accept = () => env.document.querySelector(".pcv-confirm-danger");
  assert.equal(accept().disabled, true);
  typed.value = "4";
  typed.dispatchEvent(new env.Event("input"));
  assert.equal(accept().disabled, true);
  typed.value = "2";
  typed.dispatchEvent(new env.Event("input"));
  assert.equal(accept().disabled, false);
  env.document.querySelector('[data-pcv-confirm="cancel"]').click();
  await flush();
  assert.deepEqual(deleteCalls, []);

  env.document.querySelector('[data-vm-bulk-action="delete"]').click();
  await flush();
  const typedAgain = env.document.querySelector(".pcv-confirm-typed");
  typedAgain.value = "2";
  typedAgain.dispatchEvent(new env.Event("input"));
  env.document.querySelector(".pcv-confirm-danger").click();
  await flush();
  assert.deepEqual(deleteCalls, ["off-1", "off-2"]);
  const lineIds = [...env.document.querySelectorAll("[data-vm-bulk-line]")].map((line) => line.getAttribute("data-vm-bulk-line"));
  assert.ok(lineIds.includes("off-1"));
  assert.ok(lineIds.includes("off-2"));

  env.state.authAccessToken = "jwt";
  env.state.authSession = { role: "viewer", permissions: ["read"] };
  env.renderVms();
  const denied = env.document.querySelector('[data-vm-bulk-action="delete"]');
  assert.equal(denied.disabled, true);
  denied.click();
  await flush();
  assert.deepEqual(deleteCalls, ["off-1", "off-2"]);

  env.state.authAccessToken = "";
  env.state.authSession = null;
  env.state.vmChecked = ["run-1", "lock-1"];
  env.renderVms();
  assert.equal(env.document.querySelector('[data-vm-bulk-action="delete"]').disabled, true);

  const directDelete = env.PCV.vm.queueVmBulkDelete([
    { id: "off-2", name: "off-2", state: "Off" },
    { id: "run-1", name: "run-1", state: "Running" },
    { id: "lock-1", name: "lock-1", state: "Off", template_lock: true }
  ]);
  await flush();
  const directTyped = env.document.querySelector(".pcv-confirm-typed");
  assert.equal(env.document.querySelector(".pcv-confirm-danger").disabled, true);
  directTyped.value = "3";
  directTyped.dispatchEvent(new env.Event("input"));
  assert.equal(env.document.querySelector(".pcv-confirm-danger").disabled, true);
  directTyped.value = "1";
  directTyped.dispatchEvent(new env.Event("input"));
  assert.equal(env.document.querySelector(".pcv-confirm-danger").disabled, false);
  env.document.querySelector(".pcv-confirm-danger").click();
  await directDelete;
  await flush();
  assert.deepEqual(deleteCalls, ["off-1", "off-2", "off-2"]);

  env.startPolling = previousPoll;
  env.desktopApi = previousApi;
  env.PCV.components.trackJob = previousTrack;
  env.state.authAccessToken = "";
  env.state.authSession = null;
  env.state.vmChecked = [];
  env.state.pendingVmActions = {};
  env.state.trackedJobs = [];
  if (env.state.pollTimer) {
    clearTimeout(env.state.pollTimer);
    env.state.pollTimer = null;
  }
  const leftover = env.document.querySelector('[data-pcv-confirm="cancel"]');
  if (leftover) leftover.click();
});

test("mobile power and delete call exported vm methods", () => {
  const mobile = fs.readFileSync(webRoot + "src/modules/mobile.ts", "utf8");
  const vmSource = fs.readFileSync(webRoot + "src/modules/vm.ts", "utf8");
  const listSource = fs.readFileSync(webRoot + "src/modules/vm-list.ts", "utf8");
  const exportLine = vmSource.slice(vmSource.lastIndexOf("PCV.vm = Object.assign"))
    + listSource.slice(listSource.lastIndexOf("PCV.vm = Object.assign"));
  const names = new Set();
  for (const match of mobile.matchAll(/PCV\.vm\.([A-Za-z0-9_]+)/g)) names.add(match[1]);
  assert.ok(names.size > 0);
  for (const name of names) assert.match(exportLine, new RegExp("\\b" + name + "\\b"));
  assert.equal(names.has("bulkDelete"), false);
  assert.equal(names.has("vmDel"), false);
  assert.equal(names.has("lifecycle"), false);
  assert.equal(names.has("queueVmBulkDelete"), true);
  assert.match(mobile, /queueVmLifecycle/);
  assert.match(mobile, /queueVmDelete/);
  assert.match(mobile, /PCV\.vm\.queueVmBulkDelete/);
  assert.match(mobile, /m-vm-bulk-delete/);
  assert.match(mobile, /m-vm-select/);
  assert.doesNotMatch(mobile, /PCV\.vm\.bulkDelete|customConfirm\([^)]*일괄 삭제/);
});

test("registered views render only the active screen and leave legacy screens alone", () => {
  const network = env.document.querySelector('.app-view[data-view="network"]');
  const jobs = env.document.querySelector('.app-view[data-view="jobs"]');
  const dashboard = env.document.querySelector('.app-view[data-view="dashboard"]');
  const marker = env.document.createElement("span");
  marker.id = "u4-keep";
  network.appendChild(marker);
  const keptDash = env.document.createElement("span");
  keptDash.id = "u4-dash-keep";
  dashboard.appendChild(keptDash);
  const mountPoint = env.document.createElement("div");
  mountPoint.setAttribute("data-view-mount", "");
  network.appendChild(mountPoint);
  const calls = { network: [], jobs: [] };
  let which = "";
  try {
    env.PCV.views.register("network", { render() { which = "first"; } });
    env.PCV.views.register("network", {
      render(mount, ctx) {
        which = "second";
        calls.network.push({ mount, ctx });
      }
    });
    env.PCV.views.register("jobs", {
      render(mount, ctx) { calls.jobs.push({ mount, ctx }); }
    });
    assert.equal(env.PCV.views.isRegistered("network"), true);
    assert.equal(typeof env.PCV.views.get("network").render, "function");
    assert.equal(env.pcvLegacyViewSkipped("network"), true);
    assert.equal(env.pcvLegacyViewSkipped("evidence"), false);

    env.navigateTo("network");
    assert.equal(which, "second");
    assert.equal(calls.network.length, 1);
    assert.equal(calls.jobs.length, 0);
    assert.equal(calls.network[0].mount, mountPoint);
    assert.equal(calls.network[0].ctx.reason, "navigate");
    assert.equal(typeof calls.network[0].ctx.generation, "number");
    assert.equal(calls.network[0].ctx.isCurrent(), true);
    assert.equal(network.querySelector("#u4-keep"), marker);
    assert.equal(network.hidden, false);

    env.render();
    assert.equal(calls.network.length, 2);
    assert.equal(calls.jobs.length, 0);
    assert.equal(calls.network[1].ctx.reason, "refresh");
    assert.equal(calls.network[1].ctx.isCurrent(), true);

    env.navigateTo("dashboard");
    assert.equal(calls.jobs.length, 0);
    assert.equal(calls.network[0].ctx.isCurrent(), false);
    assert.equal(dashboard.hidden, false);
    assert.equal(network.hidden, true);
    assert.equal(dashboard.querySelector("#u4-dash-keep"), keptDash);
    assert.equal(env.PCV.views.isRegistered("evidence"), false);

    env.PCV.views.unregister("jobs");
    network.hidden = false;
    jobs.hidden = false;
    env.renderActiveView();
    assert.equal(network.hidden, false);
    assert.equal(jobs.hidden, true);
    assert.equal(dashboard.hidden, false);

    env.PCV.views.register("network", {
      render() { throw new Error("boom"); }
    });
    env.navigateTo("network");
    const error = mountPoint.querySelector(".pcv-load-error");
    assert.ok(error);
    assert.match(error.textContent, /boom/);
    assert.equal(dashboard.querySelector(".pcv-load-error"), null);
    assert.equal(network.querySelector("#u4-keep"), marker);

    assert.throws(() => env.PCV.views.register("not-a-view", { render() {} }));
    assert.throws(() => env.PCV.views.register("jobs", {}));
    assert.throws(() => env.PCV.views.register("jobs", { render: "no" }));
    assert.equal(env.PCV.views.isRegistered("jobs"), false);
  } finally {
    env.PCV.views.unregister("network");
    env.PCV.views.unregister("jobs");
    if (marker.parentNode) marker.remove();
    if (keptDash.parentNode) keptDash.remove();
    if (mountPoint.parentNode) mountPoint.remove();
    const leftover = network.querySelector(".pcv-load-error");
    if (leftover) leftover.remove();
    env.navigateTo("dashboard");
  }
});

test("registered network render draws pagehead, type chips, connected VM counts, and separates empty from load errors", () => {
  const network = env.document.querySelector('.app-view[data-view="network"]');
  const jobs = env.document.querySelector('.app-view[data-view="jobs"]');
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  const head = env.document.createElement("div");
  head.className = "pagehead";
  const title = env.document.createElement("h1");
  title.className = "pagehead-title";
  title.setAttribute("data-i18n", "copy.network.title");
  title.textContent = "네트워크 인벤토리";
  const desc = env.document.createElement("p");
  desc.className = "pagehead-desc";
  desc.setAttribute("data-i18n", "copy.network.desc");
  desc.textContent = "Hyper-V 스위치 인벤토리입니다. 이 화면은 읽기 전용입니다.";
  head.appendChild(title);
  head.appendChild(desc);
  const filter = env.document.createElement("input");
  filter.id = "network-filter";
  filter.setAttribute("type", "search");
  const mount = env.document.createElement("div");
  mount.setAttribute("data-view-mount", "");
  network.appendChild(head);
  network.appendChild(filter);
  network.appendChild(mount);
  const saved = {
    vms: env.state.vms,
    networkInventory: env.state.networkInventory,
    networkError: env.state.networkError,
    networkFilter: env.state.networkFilter
  };
  const product = env.PCV.viewNetwork.render;
  const paints = [];
  env.PCV.views.register("network", {
    render(mountNode, ctx) {
      paints.push(ctx && ctx.reason);
      product(mountNode, ctx);
    }
  });
  const chip = (value) => mount.querySelector(`[data-facet="networkType"][data-val="${value}"]`);
  const rowCells = () => [...mount.querySelectorAll(".network-view-table tbody tr")].map((row) => (
    [...row.querySelectorAll("td")].map((cell) => cell.textContent)
  ));
  try {
    env.state.vms = [];
    env.state.networkInventory = null;
    env.state.networkError = null;
    env.state.networkFilter = "";
    env.PCV.ui.filterState.apply({ networkType: [] });
    env.navigateTo("network");
    assert.equal(paints[0], "navigate");
    assert.match(index, /data-i18n="copy\.network\.title"[^>]*>네트워크 인벤토리</);
    assert.match(index, /data-i18n="copy\.network\.desc"[^>]*>[^<]*읽기 전용/);
    assert.match(index, /<input id="network-filter" type="search"/);
    assert.equal(network.querySelector(".pagehead-title").textContent, "네트워크 인벤토리");
    assert.match(network.querySelector(".pagehead-desc").textContent, /읽기 전용/);
    assert.equal(mount.querySelector(".pagehead"), null);
    assert.equal(mount.querySelector("#network-filter"), null);
    assert.equal(network.querySelector("#network-filter"), filter);
    assert.ok(mount.querySelector(".network-loading .skeleton"));
    assert.equal(typeof env.renderNetworkInventory, "undefined");

    env.state.vms = [
      {
        name: "vm-a",
        network: [
          { name: "fixture-nic", switch: "Default Switch", mode: "nat" },
          { name: "fixture-nic-2", switch: "Default Switch", mode: "nat" }
        ]
      },
      { name: "vm-b", network: [{ name: "ext-nic", switch: "lab-external", mode: "external" }] }
    ];
    env.state.networkInventory = {
      switches: [
        { name: "Default Switch", type: "Internal", is_default: true, net_adapter_interface_description: "fixture-ethernet" },
        { name: "lab-external", type: "External", net_adapter_interface_description: "Intel(R) Ethernet" }
      ]
    };
    env.render();
    assert.equal(paints.at(-1), "refresh");
    assert.equal(network.querySelector(".pagehead-title").textContent, "네트워크 인벤토리");
    assert.match(network.querySelector(".pagehead-desc").textContent, /읽기 전용/);
    assert.equal(mount.querySelector(".pagehead"), null);
    assert.equal(network.querySelector("#network-filter"), filter);
    assert.equal(chip("__all__").querySelector(".chip-c").textContent, "2");
    assert.equal(chip("Internal").querySelector(".chip-c").textContent, "1");
    assert.equal(chip("External").querySelector(".chip-c").textContent, "1");
    assert.deepEqual(
      [...mount.querySelectorAll(".network-view-table thead th")].map((cell) => cell.textContent),
      ["이름", "종류", "연결 VM", "어댑터", "상태"]
    );
    const rows = Object.fromEntries(rowCells().map((cells) => [cells[0], cells]));
    assert.equal(rows["Default Switch"][2], "1");
    assert.match(rows["Default Switch"][3], /fixture-ethernet/);
    assert.match(rows["Default Switch"][4], /default/);
    assert.equal(rows["lab-external"][2], "1");
    const readback = mount.querySelector(".network-change-readback").textContent;
    assert.match(readback, /스위치 만들기 폼 없음/);
    assert.match(readback, /NAT 편집기 없음/);
    assert.match(readback, /읽기 전용/);
    assert.equal(jobs.querySelector(".pagehead"), null);

    chip("External").click();
    assert.deepEqual(rowCells().map((cells) => cells[0]), ["lab-external"]);

    env.PCV.ui.filterState.apply({ networkType: [] });
    env.state.networkFilter = "fixture-ethernet";
    env.render();
    assert.deepEqual(rowCells().map((cells) => cells[0]), ["Default Switch"]);
    env.state.networkFilter = "";

    env.PCV.ui.filterState.apply({ networkType: [] });
    env.state.networkInventory = { switches: [] };
    env.state.networkError = null;
    env.state.networkFilter = "";
    env.render();
    const emptyNode = mount.querySelector(".pcv-empty");
    assert.ok(emptyNode.classList.contains("network-empty-state"));
    const empty = emptyNode.textContent;
    assert.match(empty, /스위치가 없습니다/);
    assert.match(empty, /스위치를 만들거나/);
    assert.equal(mount.querySelector(".pcv-load-error"), null);

    env.state.networkInventory = null;
    env.state.networkError = {
      code: "PCV_NATIVE_PARITY_INCOMPLETE",
      message: "Native parity failed",
      detail: "parity"
    };
    env.render();
    const error = mount.querySelector(".pcv-load-error").textContent;
    assert.match(error, /PCV_NATIVE_PARITY_INCOMPLETE/);
    assert.match(error, /도우미 폴백은 의도적으로/);
    assert.equal(mount.querySelector(".pcv-empty"), null);
    assert.equal(error.includes("스위치가 없습니다"), false);
  } finally {
    env.state.vms = saved.vms;
    env.state.networkInventory = saved.networkInventory;
    env.state.networkError = saved.networkError;
    env.state.networkFilter = saved.networkFilter;
    env.navigateTo("dashboard");
    env.PCV.ui.filterState.apply({ networkType: [] });
    if (head.parentNode) head.remove();
    if (filter.parentNode) filter.remove();
    if (mount.parentNode) mount.remove();
    env.PCV.views.register("network", { render: product });
  }
});

test("registered jobs render draws chips, kind select, table columns, and does not repaint the legacy jobs list", async () => {
  const jobs = env.document.querySelector('.app-view[data-view="jobs"]');
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  const head = env.document.createElement("div");
  head.className = "pagehead";
  const title = env.document.createElement("h1");
  title.className = "pagehead-title";
  title.setAttribute("data-i18n", "copy.jobs.title");
  title.textContent = "작업";
  const desc = env.document.createElement("p");
  desc.className = "pagehead-desc";
  desc.setAttribute("data-i18n", "copy.jobs.desc");
  desc.textContent = "접수된 작업과 진행 상태입니다.";
  head.appendChild(title);
  head.appendChild(desc);
  const statusSelect = env.document.createElement("select");
  statusSelect.id = "job-status-filter";
  for (const value of ["all", "queued", "running", "succeeded", "failed", "canceled"]) {
    const option = env.document.createElement("option");
    option.value = value;
    option.textContent = value;
    statusSelect.appendChild(option);
  }
  const filter = env.document.createElement("input");
  filter.id = "job-filter";
  filter.setAttribute("type", "search");
  const mount = env.document.createElement("div");
  mount.id = "jobs-panel";
  mount.className = "jobs-panel";
  mount.setAttribute("data-view-mount", "");
  jobs.appendChild(head);
  jobs.appendChild(filter);
  jobs.appendChild(statusSelect);
  jobs.appendChild(mount);
  env.els.jobsPanel = mount;
  const saved = {
    serverJobs: env.state.serverJobs,
    trackedJobs: env.state.trackedJobs,
    serverJobPage: env.state.serverJobPage,
    jobStatusFilter: env.state.jobStatusFilter,
    jobFilter: env.state.jobFilter,
    jobSort: env.state.jobSort,
    jobOperationFilter: env.state.jobOperationFilter,
    activityError: env.state.activityError,
    authAccessToken: env.state.authAccessToken,
    authSession: env.state.authSession
  };
  const product = env.PCV.viewJobs.render;
  env.PCV.views.register("jobs", { render: product });
  const sample = [
    { job_id: "job-run", operation: "vm.create", status: "running", created_at: "2026-05-05T00:00:00.000Z", updated_at: "2026-05-05T00:00:05.000Z", target: "vm-run" },
    { job_id: "job-ok", operation: "vm.shutdown", status: "succeeded", created_at: "2026-05-05T00:00:00.000Z", updated_at: "2026-05-05T00:00:05.000Z", target: "vm-ok" },
    { job_id: "job-fail", operation: "vm.delete", status: "failed", created_at: "2026-05-05T00:00:00.000Z", updated_at: "2026-05-05T00:00:05.000Z", target: "vm-fail", error: { code: "PCV_JOB_FAILED", message: "boom" } },
    { job_id: "job-cancel", operation: "checkpoint.create", status: "canceled", created_at: "2026-05-05T00:00:00.000Z", updated_at: "2026-05-05T00:00:05.000Z", target: "vm-cancel" },
    { job_id: "job-queue", operation: "vm.restart", status: "queued", created_at: "2026-05-05T00:00:00.000Z", updated_at: "2026-05-05T00:00:05.000Z", target: "vm-queue" }
  ];
  const chip = (value) => mount.querySelector(`[data-facet="jobStatus"][data-val="${value}"]`);
  const previousNext = env.loadNextJobPage;
  try {
    env.state.serverJobs = sample.map((job) => ({ ...job }));
    env.state.trackedJobs = [];
    env.state.serverJobPage = { next_offset: null, limit: 50, offset: 0, retention: { max_terminal_jobs: 500 } };
    env.state.jobStatusFilter = "all";
    env.state.jobFilter = "";
    env.state.jobSort = "updated:desc";
    env.state.jobOperationFilter = "";
    env.state.activityError = null;
    env.state.authAccessToken = "";
    env.state.authSession = null;
    env.PCV.ui.filterState.apply({ jobStatus: [] });
    env.navigateTo("jobs");
    assert.match(index, /data-i18n="copy\.jobs\.title"[^>]*>작업</);
    assert.match(index, /data-i18n="copy\.jobs\.desc"[^>]*>[^<]*접수된 작업/);
    assert.match(index, /id="job-filter"/);
    assert.match(index, /id="job-status-filter"/);
    assert.match(index, /id="job-sort"/);
    assert.match(index, /id="clear-job-history"/);
    assert.match(index, /id="jobs-panel"[^>]*data-view-mount/);
    assert.equal(mount.querySelector(".pagehead"), null);
    assert.equal(mount.querySelector("#job-filter"), null);
    assert.equal(chip("__all__").querySelector(".chip-c").textContent, "5");
    assert.equal(chip("running").querySelector(".chip-c").textContent, "1");
    assert.equal(chip("running").textContent.includes("진행 중"), true);
    assert.equal(chip("succeeded").querySelector(".chip-c").textContent, "1");
    assert.equal(chip("failed").querySelector(".chip-c").textContent, "1");
    assert.equal(chip("canceled").querySelector(".chip-c").textContent, "1");
    assert.equal(mount.querySelector("#job-operation-filter").value, "");
    assert.deepEqual(
      [...mount.querySelectorAll(".jobs-view-table thead th")].map((cell) => cell.textContent),
      ["접수 시각", "종류", "대상", "상태", "걸린 시간", "오류 요약", "조작"]
    );
    assert.equal(mount.querySelector(".jobs-view-table .sb-search"), null);
    assert.equal([...mount.querySelectorAll("button")].some((button) => button.textContent === "CSV"), true);
    const runCancel = [...mount.querySelectorAll('[data-action="cancel-job"]')].find((button) => button.getAttribute("data-job-id") === "job-run");
    assert.equal(runCancel.getAttribute("data-job-cancel-scope"), "job");
    assert.equal(runCancel.disabled, false);
    assert.equal(mount.querySelector('[data-action="retry-job"]').getAttribute("data-job-id"), "job-fail");
    assert.equal(mount.querySelector('[data-action="retry-job"]').disabled, false);

    chip("failed").click();
    assert.equal(env.state.jobStatusFilter, "failed");
    assert.equal(statusSelect.value, "failed");
    assert.equal(chip("failed").classList.contains("on"), true);
    assert.equal(mount.querySelector('[data-action="retry-job"]').getAttribute("data-job-id"), "job-fail");
    assert.equal(mount.querySelector('[data-action="cancel-job"]'), null);

    statusSelect.value = "succeeded";
    statusSelect.dispatchEvent({ type: "change", target: statusSelect });
    assert.equal(env.state.jobStatusFilter, "succeeded");
    assert.equal(chip("succeeded").classList.contains("on"), true);
    assert.match(mount.textContent, /vm-ok/);

    const kind = mount.querySelector("#job-operation-filter");
    kind.value = "vm.delete";
    kind.dispatchEvent({ type: "change", target: kind });
    assert.equal(env.state.jobOperationFilter, "vm.delete");
    assert.equal(env.state.jobStatusFilter, "succeeded");
    assert.ok(mount.querySelector(".jobs-empty-filter"));
    assert.match(mount.textContent, /현재 필터와 맞는 작업이 없습니다/);

    env.state.authAccessToken = "jwt";
    env.state.authSession = { role: "viewer", permissions: ["read"] };
    env.state.jobStatusFilter = "all";
    env.state.jobOperationFilter = "";
    env.render();
    assert.equal(mount.querySelector('[data-action="cancel-job"]').disabled, true);
    assert.equal(mount.querySelector('[data-action="retry-job"]').disabled, true);
    env.state.authAccessToken = "";
    env.state.authSession = null;

    env.state.serverJobs = [];
    env.state.trackedJobs = [];
    env.state.jobStatusFilter = "all";
    env.state.jobFilter = "";
    env.state.jobOperationFilter = "";
    env.state.activityError = null;
    env.render();
    const emptyPage = mount.querySelector(".jobs-empty-page").textContent;
    assert.match(emptyPage, /이 페이지에 작업이 없습니다/);
    assert.equal(mount.querySelector(".pcv-load-error"), null);
    assert.equal(mount.querySelector(".jobs-empty-filter"), null);

    env.state.jobFilter = "zzzz-no-match";
    env.render();
    const emptyFilter = mount.querySelector(".jobs-empty-filter").textContent;
    assert.match(emptyFilter, /현재 필터와 맞는 작업이 없습니다/);
    assert.equal(emptyFilter.includes("이 페이지에 작업이 없습니다"), false);
    assert.equal(mount.querySelector(".pcv-load-error"), null);

    env.state.jobFilter = "";
    env.state.activityError = { code: "PCV_JOB_LIST_FAILED", message: "jobs down" };
    env.render();
    const errorText = mount.querySelector(".pcv-load-error").textContent;
    assert.match(errorText, /읽지 못했습니다/);
    assert.match(errorText, /PCV_JOB_LIST_FAILED/);
    assert.equal(mount.querySelector(".jobs-empty-page"), null);
    assert.equal(mount.querySelector(".jobs-empty-filter"), null);
    assert.equal(errorText.includes("이 페이지에 작업이 없습니다"), false);
    assert.equal(errorText.includes("현재 필터와 맞는 작업이 없습니다"), false);

    env.state.activityError = null;
    env.state.serverJobs = sample.map((job) => ({ ...job }));
    env.state.serverJobPage = { next_offset: 50, limit: 50, offset: 0, retention: { max_terminal_jobs: 500 } };
    env.render();
    const next = mount.querySelector('[data-action="load-next-jobs"]');
    assert.equal(next.getAttribute("data-next-offset"), "50");
    assert.match(next.textContent, /다음 페이지/);
    const seen = [];
    env.loadNextJobPage = () => {
      seen.push(env.state.serverJobPage.next_offset);
      return Promise.resolve();
    };
    next.click();
    await flush();
    assert.deepEqual(seen, [50]);

    const marker = env.document.createElement("p");
    marker.textContent = "legacy-marker";
    mount.appendChild(marker);
    env.renderJobs();
    assert.equal(mount.querySelector("p").textContent, "legacy-marker");
    assert.equal(mount.querySelector(".job-row"), null);
    env.PCV.views.unregister("jobs");
    env.state.serverJobs = [];
    env.state.trackedJobs = [];
    env.state.jobFilter = "";
    env.state.jobStatusFilter = "all";
    env.renderJobs();
    assert.match(mount.innerHTML, /No jobs on this page/);
    assert.equal(mount.textContent.includes("legacy-marker"), false);
  } finally {
    env.loadNextJobPage = previousNext;
    env.state.serverJobs = saved.serverJobs;
    env.state.trackedJobs = saved.trackedJobs;
    env.state.serverJobPage = saved.serverJobPage;
    env.state.jobStatusFilter = saved.jobStatusFilter;
    env.state.jobFilter = saved.jobFilter;
    env.state.jobSort = saved.jobSort;
    env.state.jobOperationFilter = saved.jobOperationFilter || "";
    env.state.activityError = saved.activityError;
    env.state.authAccessToken = saved.authAccessToken;
    env.state.authSession = saved.authSession;
    env.navigateTo("dashboard");
    env.PCV.ui.filterState.apply({ jobStatus: [] });
    if (head.parentNode) head.remove();
    if (filter.parentNode) filter.remove();
    if (statusSelect.parentNode) statusSelect.remove();
    if (mount.parentNode) mount.remove();
    env.PCV.views.register("jobs", { render: product });
  }
});

test("registered activity render draws severity cards, chips, table columns, and a jobs link", () => {
  const activity = env.document.querySelector('.app-view[data-view="activity"]');
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  const product = env.PCV.viewEvents.render;
  env.PCV.views.register("activity", { render: product });
  const mount = env.document.createElement("div");
  mount.id = "event-center-panel";
  mount.className = "event-center-panel";
  mount.setAttribute("data-view-mount", "");
  const panel = env.document.createElement("div");
  panel.id = "activity-panel";
  panel.className = "activity-panel";
  activity.appendChild(mount);
  activity.appendChild(panel);
  env.els.eventCenterPanel = mount;
  env.els.activityPanel = panel;
  const saved = {
    apiToken: env.state.apiToken,
    error: env.state.error,
    summaryError: env.state.summaryError,
    activityError: env.state.activityError,
    networkError: env.state.networkError,
    diagnosticBundleError: env.state.diagnosticBundleError,
    authError: env.state.authError,
    consoleError: env.state.consoleError,
    partialFailures: env.state.partialFailures,
    loading: env.state.loading,
    eventSeverityFilter: env.state.eventSeverityFilter,
    build: env.buildEventCenterItems
  };
  function chip(value) {
    return mount.querySelector(`[data-facet="eventSeverity"][data-val="${value}"]`);
  }
  function cardCount(tone) {
    return mount.querySelector(".event-count-card.event-" + tone).querySelector("strong").textContent;
  }
  try {
    env.state.apiToken = "present";
    env.state.error = { code: "PCV_EVT_ERROR", message: "disk failed", operation: "host.status" };
    env.state.summaryError = null;
    env.state.activityError = null;
    env.state.networkError = null;
    env.state.diagnosticBundleError = null;
    env.state.authError = null;
    env.state.consoleError = null;
    env.state.partialFailures = [{ code: "PCV_EVT_WARN", message: "slow link", operation: "vm.list" }];
    env.state.loading = false;
    env.state.eventSeverityFilter = "all";
    env.PCV.ui.filterState.apply({ eventSeverity: [] });
    env.navigateTo("activity");
    assert.match(index, /id="activity"/);
    assert.match(index, /id="activity-panel"/);
    assert.match(index, /id="event-center-panel"[^>]*data-view-mount/);
    assert.match(index, /data-i18n="copy\.activity\.title"[^>]*>이벤트</);
    assert.match(index, /data-i18n="copy\.activity\.desc"/);
    assert.ok(mount.querySelector(".event-severity-lane"));
    assert.equal(mount.querySelectorAll(".event-count-card").length, 4);
    assert.equal(cardCount("error"), chip("error").querySelector(".chip-c").textContent);
    assert.equal(cardCount("warn"), chip("warn").querySelector(".chip-c").textContent);
    assert.equal(chip("__all__").querySelector(".chip-c").textContent, String(
      ["error", "warn", "info", "ok"].reduce((sum, tone) => sum + Number(cardCount(tone)), 0)
    ));
    assert.equal(Number(cardCount("error")) >= 1, true);
    assert.equal(Number(cardCount("warn")) >= 1, true);
    assert.deepEqual(
      [...mount.querySelectorAll(".events-view-table thead th")].map((cell) => cell.textContent),
      ["시각", "심각도", "출처", "메시지"]
    );
    assert.equal(mount.querySelector(".events-view-table .sb-search"), null);
    assert.equal([...mount.querySelectorAll("button")].some((button) => button.textContent === "CSV"), true);
    assert.match(mount.textContent, /PCV_EVT_ERROR/);
    assert.match(mount.textContent, /PCV_EVT_WARN/);
    assert.match(panel.textContent, /작업 기록은 작업 화면에서 봅니다/);
    const jobsLink = panel.querySelector('a[href="#/jobs"]');
    assert.equal(jobsLink.getAttribute("data-view-link"), "jobs");
    assert.match(jobsLink.textContent, /작업 화면/);
    assert.equal(panel.querySelector('[data-action="cancel-job"]'), null);
    assert.equal(panel.querySelector(".activity-row"), null);

    env.buildEventCenterItems = () => [];
    env.state.loading = true;
    env.render();
    assert.match(mount.textContent, /이벤트를 읽고 있습니다/);
    assert.equal(mount.textContent.includes("보이는 운영 이벤트가 없습니다"), false);
    assert.equal(mount.textContent.includes("읽지 못했습니다"), false);

    env.state.loading = false;
    env.state.summaryError = { code: "PCV_OPS_SUMMARY_FAILED", message: "summary unavailable", operation: "ops.summary" };
    env.render();
    assert.match(mount.textContent, /읽지 못했습니다/);
    assert.match(mount.textContent, /PCV_OPS_SUMMARY_FAILED/);
    assert.equal(mount.textContent.includes("보이는 운영 이벤트가 없습니다"), false);
    assert.equal(mount.textContent.includes("이벤트를 읽고 있습니다"), false);

    env.state.summaryError = null;
    env.render();
    assert.match(mount.textContent, /보이는 운영 이벤트가 없습니다/);
    assert.equal(mount.textContent.includes("읽지 못했습니다"), false);
    assert.equal(mount.querySelector(".events-empty"), mount.querySelector(".pcv-empty"));
    assert.equal(mount.querySelector(".pcv-load-error"), null);

    const marker = env.document.createElement("p");
    marker.textContent = "legacy-marker";
    mount.appendChild(marker);
    env.renderEventCenter();
    assert.match(mount.textContent, /legacy-marker/);
    env.PCV.views.unregister("activity");
    env.renderEventCenter();
    assert.equal(mount.textContent.includes("legacy-marker"), false);
    assert.match(mount.innerHTML, /event-severity-lane/);
  } finally {
    env.buildEventCenterItems = saved.build;
    env.state.apiToken = saved.apiToken;
    env.state.error = saved.error;
    env.state.summaryError = saved.summaryError;
    env.state.activityError = saved.activityError;
    env.state.networkError = saved.networkError;
    env.state.diagnosticBundleError = saved.diagnosticBundleError;
    env.state.authError = saved.authError;
    env.state.consoleError = saved.consoleError;
    env.state.partialFailures = saved.partialFailures;
    env.state.loading = saved.loading;
    env.state.eventSeverityFilter = saved.eventSeverityFilter || "all";
    env.navigateTo("dashboard");
    env.PCV.ui.filterState.apply({ eventSeverity: [] });
    if (mount.parentNode) mount.remove();
    if (panel.parentNode) panel.remove();
    env.PCV.views.register("activity", { render: product });
  }
});

test("registered dashboard render draws six KPI cards, links, Hyper-V states, and drops the hero", () => {
  const dashboard = env.document.querySelector('.app-view[data-view="dashboard"]');
  const index = fs.readFileSync(webRoot + "index.html", "utf8");
  // The registry renders into the first [data-view-mount] of the section; index.html already ships #dashboard-kpi.
  let mount = dashboard.querySelector("[data-view-mount]");
  if (!mount) {
    mount = env.document.createElement("div");
    mount.id = "dashboard-kpi";
    mount.className = "dashboard-kpi";
    mount.setAttribute("data-view-mount", "");
    dashboard.appendChild(mount);
  }
  const product = env.PCV.viewDashboard.render;
  env.PCV.views.register("dashboard", { render: product });
  const saved = {
    vms: env.state.vms,
    serverJobs: env.state.serverJobs,
    trackedJobs: env.state.trackedJobs,
    lastRefreshedAt: env.state.lastRefreshedAt,
    partialFailures: env.state.partialFailures,
    host: env.state.host,
    opsSummary: env.state.opsSummary
  };
  function card(kpi) {
    // The test DOM does not match class selectors on <a>; read the mount children directly.
    return [...mount.children].find((node) => node.getAttribute("data-kpi") === kpi);
  }
  function value(kpi) {
    return card(kpi).querySelector(".dashboard-kpi-value").textContent;
  }
  const now = Date.now();
  try {
    env.state.lastRefreshedAt = now;
    env.state.partialFailures = [];
    env.state.opsSummary = null;
    env.state.host = { supported: true };
    env.state.vms = [
      { name: "run-1", state: "Running" },
      { name: "off-1", state: "Off" },
      { name: "off-2", state: "Off" },
      { name: "paused-1", state: "Paused" },
      { name: "tpl-1", state: "Off", template_lock: true }
    ];
    env.state.serverJobs = [
      { job_id: "j-run", status: "running", created_at: new Date(now - 1000).toISOString() },
      { job_id: "j-q", status: "queued", created_at: new Date(now - 2000).toISOString() },
      { job_id: "j-fail-new", status: "failed", created_at: new Date(now - 60 * 60 * 1000).toISOString() },
      { job_id: "j-fail-old", status: "failed", created_at: new Date(now - 48 * 60 * 60 * 1000).toISOString() },
      { job_id: "j-fail-blank", status: "failed", created_at: "" },
      { job_id: "j-ok", status: "succeeded", created_at: new Date(now - 1000).toISOString() }
    ];
    env.state.trackedJobs = [];
    env.navigateTo("dashboard");
    assert.equal(mount.children.length, 6);
    assert.equal(value("vm-running"), "1");
    assert.equal(value("vm-off"), "2");
    assert.equal(value("templates"), "1");
    assert.equal(value("jobs-active"), "2");
    assert.equal(value("jobs-failed-24h"), "1");
    assert.equal(card("hyperv").getAttribute("data-hv-state"), "ready");
    assert.equal(value("hyperv"), "준비");
    assert.match(card("hyperv").querySelector(".dashboard-kpi-note").textContent, /준비/);
    assert.equal(card("vm-running").getAttribute("href"), "#/vms");
    assert.equal(card("vm-off").getAttribute("data-view-link"), "vms");
    assert.equal(card("templates").getAttribute("href"), "#/vms");
    assert.equal(card("jobs-active").getAttribute("href"), "#/jobs");
    assert.equal(card("jobs-failed-24h").getAttribute("data-view-link"), "jobs");
    assert.equal(card("hyperv").tagName, "DIV");
    assert.equal(card("vm-running").querySelector(".dashboard-kpi-note").textContent.includes("읽는 중"), false);
    assert.match(index, /id="open-create-vm"/);
    assert.match(index, /data-i18n="copy\.dashboard\.title"[^>]*>운영 콕핏</);
    assert.match(index, /data-i18n="copy\.dashboard\.desc"/);
    assert.match(index, /id="dashboard-kpi"[^>]*data-view-mount/);
    assert.match(index, /id="ops-summary-panel"/);
    assert.match(index, /id="priority-panel"/);
    assert.match(index, /id="dashboard-activity-panel"/);
    assert.match(index, /id="metric-grid"/);
    assert.match(index, /id="host-details"/);
    assert.match(index, /id="monitoring"/);
    assert.match(index, /id="monitoring-panel"/);
    assert.doesNotMatch(index, /class="dashboard-hero"/);
    assert.doesNotMatch(index, /class="quick-action-grid"/);
    assert.doesNotMatch(index, /class="operator-memo"/);
    assert.doesNotMatch(index, /id="hero-workload"/);

    env.state.host = { supported: false };
    env.render();
    assert.equal(card("hyperv").getAttribute("data-hv-state"), "not-ready");
    assert.equal(value("hyperv"), "미준비");
    assert.match(card("hyperv").querySelector(".dashboard-kpi-note").textContent, /확인 필요/);

    env.state.host = { supported: true };
    env.state.opsSummary = { host: { readiness: "needs attention" } };
    env.render();
    assert.equal(card("hyperv").getAttribute("data-hv-state"), "not-ready");
    assert.match(card("hyperv").querySelector(".dashboard-kpi-note").textContent, /needs attention/);

    env.state.lastRefreshedAt = null;
    env.state.host = null;
    env.state.opsSummary = null;
    env.render();
    assert.equal(value("vm-running"), "-");
    assert.equal(value("vm-off"), "-");
    assert.equal(value("templates"), "-");
    assert.match(card("vm-running").querySelector(".dashboard-kpi-note").textContent, /읽는 중/);
    assert.equal(card("hyperv").getAttribute("data-hv-state"), "unknown");
    assert.equal(value("hyperv"), "-");
    assert.match(card("hyperv").querySelector(".dashboard-kpi-note").textContent, /읽는 중/);
    assert.equal(value("jobs-active"), "2");
    assert.equal(value("jobs-failed-24h"), "1");
  } finally {
    env.state.vms = saved.vms;
    env.state.serverJobs = saved.serverJobs;
    env.state.trackedJobs = saved.trackedJobs;
    env.state.lastRefreshedAt = saved.lastRefreshedAt;
    env.state.partialFailures = saved.partialFailures;
    env.state.host = saved.host;
    env.state.opsSummary = saved.opsSummary;
    if (mount.parentNode) mount.remove();
    env.navigateTo("dashboard");
    env.PCV.views.register("dashboard", { render: product });
  }
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

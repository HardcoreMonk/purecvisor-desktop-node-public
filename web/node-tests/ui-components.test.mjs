import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 2 (ui-redesign-20261105): PCV.components confirm, validators, tabs, empty/load error, and job polling.
// The harness is a small DOM because these node tests do not start a browser.

const webRoot = fileURLToPath(new URL("../", import.meta.url));

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
  get firstChild() { return this.childNodes[0] || null; }
  setAttribute(name, value) {
    const text = String(value);
    this.attributes.set(name, text);
    if (name === "id") this._id = text;
    if (name === "class") this._className = text;
    if (name === "disabled") this._disabled = true;
    if (name === "hidden") this._hidden = true;
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
    addEventListener() {},
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
  for (const file of ["i18n.js", "src/modules/uxlib.ts", "src/modules/modal-core.ts", "src/modules/components.ts"]) {
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

function key(target, name, extra = {}) {
  target.dispatchEvent({
    type: "keydown",
    key: name,
    shiftKey: !!extra.shiftKey,
    target,
    preventDefault() {},
    stopPropagation() { this.cancelBubble = true; }
  });
}

test("confirm dialog cancels on Escape, cycles focus, blocks danger backdrop clicks, and gates typed names", async () => {
  const sandbox = createEnv();
  const opener = sandbox.document.createElement("button");
  opener.textContent = "open";
  sandbox.document.body.appendChild(opener);
  opener.focus();
  const typed = sandbox.PCV.components.confirmDialog({
    title: "삭제",
    message: "VM을 지웁니다.",
    confirmLabel: "삭제",
    danger: true,
    typedName: "lab-vm"
  });
  const dialog = sandbox.document.querySelector("dialog");
  assert.equal(dialog.getAttribute("role"), "dialog");
  assert.equal(dialog.getAttribute("aria-modal"), "true");
  assert.ok(dialog.getAttribute("aria-labelledby"));
  assert.ok(dialog.getAttribute("aria-describedby"));
  const input = dialog.querySelector("input");
  const accept = dialog.querySelector('[data-pcv-confirm="accept"]');
  const cancel = dialog.querySelector('[data-pcv-confirm="cancel"]');
  assert.equal(sandbox.document.activeElement, input);
  assert.equal(accept.disabled, true);
  input.value = "lab-VM";
  input.dispatchEvent({ type: "input", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  assert.equal(accept.disabled, true, "typedName compare is ordinal");
  input.value = "lab-vm";
  input.dispatchEvent({ type: "input", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  assert.equal(accept.disabled, false);
  let dangerSettled = false;
  typed.then(() => { dangerSettled = true; });
  dialog.dispatchEvent({
    type: "click",
    target: dialog,
    clientX: -20,
    clientY: -20,
    preventDefault() {},
    stopPropagation() { this.cancelBubble = true; }
  });
  await Promise.resolve();
  assert.equal(dangerSettled, false);
  assert.equal(dialog.open, true);
  key(dialog, "Tab");
  assert.equal(sandbox.document.activeElement, cancel);
  key(dialog, "Tab", { shiftKey: true });
  assert.equal(sandbox.document.activeElement, input);
  key(dialog, "Escape");
  assert.equal(await typed, false);
  assert.equal(sandbox.document.activeElement, opener);

  const plain = sandbox.PCV.components.confirmDialog({ title: "확인", message: "계속", confirmLabel: "계속" });
  const plainDialog = sandbox.document.querySelector("dialog");
  const plainAccept = plainDialog.querySelector('[data-pcv-confirm="accept"]');
  const plainCancel = plainDialog.querySelector('[data-pcv-confirm="cancel"]');
  assert.equal(sandbox.document.activeElement, plainAccept);
  key(plainDialog, "Tab");
  assert.equal(sandbox.document.activeElement, plainCancel);
  key(plainDialog, "Tab");
  assert.equal(sandbox.document.activeElement, plainAccept);
  plainAccept.click();
  assert.equal(await plain, true);
});

test("name validators follow the Hyper-V and create rules, including reserved names and duplicate hints", () => {
  const sandbox = createEnv();
  const rules = sandbox.PCV.components.validators;
  const hyper = rules.hyperVName;
  const create = rules.vmCreateName;
  const cases = [
    ["", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    [" a", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a ", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a/b", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a\\b", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a\u0001b", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    [".", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["..", "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a".repeat(128), null, "PCV_VM_NAME_INVALID"],
    ["a".repeat(129), "PCV_VM_NAME_INVALID", "PCV_VM_NAME_INVALID"],
    ["a".repeat(63), null, null],
    ["a".repeat(64), null, "PCV_VM_NAME_INVALID"],
    ["-abc", null, "PCV_VM_NAME_INVALID"],
    ["a-b_c.1", null, null],
    ["a b", null, "PCV_VM_NAME_INVALID"],
    ["가", null, null],
    ["a\uFEFF", null, "PCV_VM_NAME_INVALID"],
    ["😀", null, "PCV_VM_NAME_INVALID"]
  ];
  for (const [value, hyperCode, createCode] of cases) {
    assert.equal(hyper(value)?.code || null, hyperCode, `hyperVName ${JSON.stringify(value)}`);
    assert.equal(create(value)?.code || null, createCode, `vmCreateName ${JSON.stringify(value)}`);
  }
  assert.equal(rules.required("  ")?.code, "PCV_VM_NAME_REQUIRED");
  assert.equal(rules.required("a"), null);
  assert.equal(rules.notSameAs("Vm")("Vm")?.code, "PCV_VM_CLONE_NAME_CONFLICT");
  assert.equal(rules.notSameAs("Vm")("vm"), null);
  assert.equal(rules.notIn(["Alpha", "beta"])("alpha")?.code, "PCV_VM_ALREADY_EXISTS");
  assert.equal(rules.notIn(["ALPHA"])("alpha")?.code, "PCV_VM_ALREADY_EXISTS");
  assert.equal(rules.notIn(["Alpha"])("gamma"), null);
  assert.equal(rules.notIn(["İ"])("i\u0307"), null);
  assert.equal(rules.notIn(["ß"])("SS")?.code, "PCV_VM_ALREADY_EXISTS");
});

test("bindValidation stays quiet until input or blur and enables submit only after every field passes", () => {
  const sandbox = createEnv();
  const form = sandbox.document.createElement("form");
  const input = sandbox.document.createElement("input");
  const other = sandbox.document.createElement("input");
  input.setAttribute("name", "name");
  other.setAttribute("name", "other");
  const submit = sandbox.document.createElement("button");
  form.appendChild(input);
  form.appendChild(other);
  sandbox.document.body.appendChild(form);
  const rules = sandbox.PCV.components.validators;
  sandbox.PCV.components.bindValidation(form, {
    name: [rules.required, rules.hyperVName],
    other: [rules.required]
  }, submit);
  assert.equal(submit.disabled, true);
  assert.equal(form.querySelector(".pcv-field-error"), null);
  assert.equal(input.getAttribute("aria-invalid"), null);
  assert.equal(other.getAttribute("aria-invalid"), null);
  input.value = " bad";
  input.dispatchEvent({ type: "input", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  assert.equal(submit.disabled, true);
  assert.equal(input.getAttribute("aria-invalid"), "true");
  assert.equal(other.getAttribute("aria-invalid"), null);
  const error = form.querySelector(".pcv-field-error");
  assert.equal(input.getAttribute("aria-describedby"), error.id);
  assert.match(error.textContent, /1~128/);
  other.dispatchEvent({ type: "blur", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  assert.equal(other.getAttribute("aria-invalid"), "true");
  assert.equal(form.querySelectorAll(".pcv-field-error").length, 2);
  input.value = "lab-vm";
  input.dispatchEvent({ type: "input", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  other.value = "ok";
  other.dispatchEvent({ type: "input", preventDefault() {}, stopPropagation() { this.cancelBubble = true; } });
  assert.equal(submit.disabled, false);
  assert.equal(input.getAttribute("aria-invalid"), "false");
  assert.equal(other.getAttribute("aria-invalid"), "false");
  assert.equal(error.hidden, true);
});

test("tabs move with arrows, Home, and End, and ignore a broken rememberKey store", () => {
  const sandbox = createEnv();
  const container = sandbox.document.createElement("div");
  sandbox.document.body.appendChild(container);
  const seen = [];
  const tabs = [{ id: "summary", labelKey: "ui.job.queued" }, { id: "console", labelKey: "ui.job.running" }, { id: "jobs", labelKey: "ui.job.succeeded" }];
  sandbox.PCV.components.renderTabs(container, tabs, "summary", (id) => seen.push(id), { rememberKey: "pcv-tab" });
  const buttons = container.querySelectorAll('[role="tab"]');
  const panels = container.querySelectorAll('[role="tabpanel"]');
  assert.equal(container.querySelector('[role="tablist"]').getAttribute("role"), "tablist");
  assert.equal(buttons.length, 3);
  assert.equal(panels.length, 3);
  assert.equal(buttons[0].getAttribute("aria-selected"), "true");
  buttons[0].focus();
  key(buttons[0], "ArrowRight");
  assert.equal(buttons[1].getAttribute("aria-selected"), "true");
  assert.equal(panels[1].hidden, false);
  assert.equal(panels[0].hidden, true);
  key(buttons[1], "End");
  assert.equal(buttons[2].getAttribute("aria-selected"), "true");
  key(buttons[2], "Home");
  assert.equal(buttons[0].getAttribute("aria-selected"), "true");
  assert.deepEqual(seen, ["console", "jobs", "summary"]);
  assert.equal(sandbox.localStorage.getItem("pcv-tab"), "summary");

  sandbox.localStorage.getItem = () => { throw new Error("blocked"); };
  sandbox.localStorage.setItem = () => { throw new Error("blocked"); };
  const again = sandbox.document.createElement("div");
  sandbox.document.body.appendChild(again);
  assert.doesNotThrow(() => sandbox.PCV.components.renderTabs(again, tabs, "console", () => {}, { rememberKey: "pcv-tab" }));
  assert.equal(again.querySelectorAll('[role="tab"]')[1].getAttribute("aria-selected"), "true");
});

test("tab click selects that panel", () => {
  const sandbox = createEnv();
  const container = sandbox.document.createElement("div");
  sandbox.document.body.appendChild(container);
  const seen = [];
  const tabs = [{ id: "summary", labelKey: "ui.job.queued" }, { id: "console", labelKey: "ui.job.running" }, { id: "jobs", labelKey: "ui.job.succeeded" }];
  sandbox.PCV.components.renderTabs(container, tabs, "summary", (id) => seen.push(id));
  const buttons = container.querySelectorAll('[role="tab"]');
  const panels = container.querySelectorAll('[role="tabpanel"]');
  buttons[2].click();
  assert.equal(buttons[2].getAttribute("aria-selected"), "true");
  assert.equal(buttons[0].getAttribute("aria-selected"), "false");
  assert.equal(panels[2].hidden, false);
  assert.equal(panels[0].hidden, true);
  assert.equal(panels[1].hidden, true);
  buttons[0].click();
  assert.equal(buttons[0].getAttribute("aria-selected"), "true");
  assert.equal(panels[0].hidden, false);
  assert.equal(panels[2].hidden, true);
  assert.deepEqual(seen, ["jobs", "summary"]);
});

test("empty state and load error use different copy and classes", () => {
  const sandbox = createEnv();
  let acted = 0;
  let retried = 0;
  const empty = sandbox.PCV.components.emptyState({
    titleKey: "ui.empty.none",
    bodyKey: "ui.empty.none_body",
    actionLabelKey: "ui.confirm.confirm",
    onAction: () => { acted += 1; }
  });
  const failed = sandbox.PCV.components.loadError({
    titleKey: "ui.load_error.title",
    detail: "PCV_VM_NOT_FOUND",
    onRetry: () => { retried += 1; }
  });
  assert.equal(empty.classList.contains("pcv-empty"), true);
  assert.equal(empty.classList.contains("pcv-load-error"), false);
  assert.equal(failed.classList.contains("pcv-load-error"), true);
  assert.equal(failed.getAttribute("role"), "alert");
  assert.equal(empty.getAttribute("role"), "status");
  assert.equal(empty.querySelector("h3").tagName, "H3");
  assert.equal(failed.querySelector("h3").tagName, "H3");
  assert.notEqual(empty.querySelector("h3").textContent, failed.querySelector("h3").textContent);
  const titled = sandbox.PCV.components.emptyState({ titleKey: "ui.empty.none", bodyKey: "ui.empty.none_body", headingLevel: 2 });
  const warned = sandbox.PCV.components.loadError({ titleKey: "ui.load_error.title", detail: "x", headingLevel: 4 });
  const fallback = sandbox.PCV.components.emptyState({ titleKey: "ui.empty.none", bodyKey: "ui.empty.none_body", headingLevel: 6 });
  assert.equal(titled.querySelector("h2").tagName, "H2");
  assert.equal(warned.querySelector("h4").tagName, "H4");
  assert.equal(fallback.querySelector("h3").tagName, "H3");
  assert.match(failed.textContent, /PCV_VM_NOT_FOUND/);
  assert.doesNotMatch(empty.textContent, /PCV_VM_NOT_FOUND/);
  empty.querySelector("button").click();
  failed.querySelector("button").click();
  assert.equal(acted, 1);
  assert.equal(retried, 1);
});

test("trackJob polls the desktop job detail route and stops after a terminal status", async () => {
  const sandbox = createEnv();
  const jobs = [
    { job_id: "job-1", status: "running", created_at: "2026-11-05T00:00:00.000Z", updated_at: "2026-11-05T00:00:02.000Z" },
    { job_id: "job-1", status: "succeeded", created_at: "2026-11-05T00:00:00.000Z", updated_at: "2026-11-05T00:00:04.000Z" }
  ];
  const calls = [];
  sandbox.PCV.desktopApi = { getJob: (id) => { calls.push(id); return Promise.resolve(jobs[Math.min(calls.length, jobs.length) - 1]); } };
  const timers = [];
  sandbox.setTimeout = (fn, delay) => {
    function entry() { fn(); }
    entry.delay = delay;
    timers.push(entry);
    return timers.length;
  };
  sandbox.clearTimeout = () => {};
  const seen = [];
  const stop = sandbox.PCV.components.trackJob("job-1", (job) => seen.push(job.status));
  await Promise.resolve();
  await Promise.resolve();
  assert.deepEqual(calls, ["job-1"]);
  assert.deepEqual(seen, ["running"]);
  assert.equal(timers.length, 1);
  assert.equal(timers[0].delay, 2000);
  const row = sandbox.PCV.components.renderJobProgress(jobs[0]);
  assert.equal(row.getAttribute("data-phase"), "running");
  assert.match(row.textContent, /실행 중/);
  timers[0]();
  await Promise.resolve();
  await Promise.resolve();
  assert.deepEqual(seen, ["running", "succeeded"]);
  assert.equal(timers.length, 1, "a completed job does not schedule another poll");
  const done = sandbox.PCV.components.renderJobProgress({
    status: "failed",
    created_at: "2026-11-05T00:00:00.000Z",
    updated_at: "2026-11-05T00:00:03.000Z",
    error: { code: "PCV_VM_ALREADY_EXISTS", message: "exists", detail: "pick another name" }
  });
  assert.equal(done.getAttribute("data-phase"), "failed");
  assert.match(done.textContent, /PCV_VM_ALREADY_EXISTS/);
  assert.match(done.textContent, /자세히/);
  assert.equal(done.querySelector(".pcv-job-detail").hidden, true);
  done.querySelector("button").click();
  assert.equal(done.querySelector(".pcv-job-detail").hidden, false);
  assert.match(done.querySelector(".pcv-job-detail").textContent, /pick another name/);
  stop();
});

function pointer(target, type) {
  target.dispatchEvent({
    type,
    target,
    preventDefault() {},
    stopPropagation() { this.cancelBubble = true; }
  });
}

test("toast can be closed, success and info hide after 5 seconds unless held, and error stays", () => {
  const sandbox = createEnv();
  const timers = [];
  sandbox.setTimeout = (fn, delay) => {
    const entry = { fn, delay, cleared: false };
    timers.push(entry);
    return timers.length;
  };
  sandbox.clearTimeout = (id) => {
    const entry = timers[id - 1];
    if (entry) entry.cleared = true;
  };
  const ok = sandbox.PCV.components.toast("saved", { kind: "success" });
  const close = ok.querySelector(".pcv-toast-close");
  assert.equal(close.getAttribute("aria-label"), "닫기");
  assert.equal(timers.length, 1);
  assert.equal(timers[0].delay, 5000);
  pointer(ok, "mouseenter");
  assert.equal(timers[0].cleared, true);
  assert.equal(ok.parentNode, sandbox.document.getElementById("toasts"));
  pointer(ok, "mouseleave");
  assert.equal(timers.length, 2);
  assert.equal(timers[1].delay <= 5000 && timers[1].delay > 0, true);
  timers[1].fn();
  assert.equal(ok.parentNode, null);

  const info = sandbox.PCV.components.toast("note", { kind: "info" });
  const infoTimer = timers.length;
  pointer(info, "focusin");
  assert.equal(timers[infoTimer - 1].cleared, true);
  pointer(info, "focusout");
  assert.equal(timers.length, infoTimer + 1);
  timers[timers.length - 1].fn();
  assert.equal(info.parentNode, null);

  const beforeError = timers.length;
  const err = sandbox.PCV.components.toast("boom", { kind: "error", detail: "PCV_VM_NOT_FOUND" });
  assert.equal(timers.length, beforeError);
  assert.match(err.textContent, /PCV_VM_NOT_FOUND/);
  err.querySelector(".pcv-toast-close").click();
  assert.equal(err.parentNode, null);
});

test("job progress shows canceled on its own and keeps an unknown status on data-phase", async () => {
  const sandbox = createEnv();
  const canceled = sandbox.PCV.components.renderJobProgress({
    status: "canceled",
    created_at: "2026-11-05T00:00:00.000Z",
    updated_at: "2026-11-05T00:00:05.000Z"
  });
  assert.equal(canceled.getAttribute("data-phase"), "canceled");
  assert.equal(canceled.classList.contains("pcv-job-canceled"), true);
  assert.match(canceled.textContent, /취소됨/);
  assert.equal(canceled.querySelector(".pcv-job-details"), null);
  const unknown = sandbox.PCV.components.renderJobProgress({ status: "paused" });
  assert.equal(unknown.getAttribute("data-phase"), "paused");
  assert.match(unknown.textContent, /접수/);
  assert.equal(unknown.classList.contains("pcv-job-queued"), true);
  const jobs = [
    { job_id: "job-2", status: "running" },
    { job_id: "job-2", status: "canceled" }
  ];
  const calls = [];
  sandbox.PCV.desktopApi = {
    getJob: (id) => {
      calls.push(id);
      return Promise.resolve(jobs[Math.min(calls.length, jobs.length) - 1]);
    }
  };
  const timers = [];
  sandbox.setTimeout = (fn, delay) => {
    timers.push({ fn, delay });
    return timers.length;
  };
  sandbox.clearTimeout = () => {};
  const seen = [];
  sandbox.PCV.components.trackJob("job-2", (job) => seen.push(job.status));
  await Promise.resolve();
  await Promise.resolve();
  assert.equal(timers.length, 1);
  assert.equal(timers[0].delay, 2000);
  timers[0].fn();
  await Promise.resolve();
  await Promise.resolve();
  assert.deepEqual(seen, ["running", "canceled"]);
  assert.equal(timers.length, 1);
});

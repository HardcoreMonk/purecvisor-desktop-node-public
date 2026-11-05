import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 4 (ui-redesign-20261105): console frame size, fit scale, fullscreen, input chip, and poll pause.
// The harness is a small DOM because these node tests do not start a browser.

const webRoot = fileURLToPath(new URL("../", import.meta.url));

function storage(options = {}) {
  const values = new Map();
  return {
    getItem: (key) => {
      if (options.throwOnRead) throw new Error("storage read blocked");
      return values.has(key) ? values.get(key) : null;
    },
    setItem: (key, value) => {
      if (options.throwOnWrite) throw new Error("storage write blocked");
      values.set(key, String(value));
    },
    removeItem: (key) => { values.delete(key); },
    clear: () => values.clear()
  };
}

function makeDocument() {
  const nodes = new Map();
  const document = {
    nodeType: 9,
    hidden: false,
    fullscreenEnabled: true,
    fullscreenElement: null,
    activeElement: { id: "body" },
    documentElement: {
      requestFullscreen() { return Promise.resolve(); }
    },
    listeners: {},
    addEventListener(type, fn) {
      (this.listeners[type] ||= []).push(fn);
    },
    getElementById(id) {
      return nodes.get(id) || null;
    },
    register(node) {
      if (node.id) nodes.set(node.id, node);
      return node;
    }
  };
  return document;
}

function makeEl(document, id, tag = "div") {
  const node = {
    id,
    tagName: tag,
    nodeType: 1,
    hidden: false,
    disabled: false,
    parentNode: null,
    value: "",
    style: {},
    dataset: {},
    textContent: "",
    width: 1024,
    height: 768,
    clientWidth: 512,
    offsetWidth: 512,
    classList: {
      _s: new Set(),
      contains(name) { return this._s.has(name); },
      add(name) { this._s.add(name); },
      remove(name) { this._s.delete(name); },
      toggle(name, force) {
        const on = force === undefined ? !this.contains(name) : Boolean(force);
        if (on) this.add(name);
        else this.remove(name);
        return on;
      }
    },
    listeners: {},
    addEventListener(type, fn) {
      (this.listeners[type] ||= []).push(fn);
    },
    setAttribute(name, value) {
      if (name === "aria-pressed") this.ariaPressed = value;
    },
    getAttribute(name) {
      return null;
    },
    focus() {
      document.activeElement = this;
      for (const fn of this.listeners.focus || []) fn({ target: this });
    },
    blur() {
      document.activeElement = { id: "body" };
      for (const fn of this.listeners.blur || []) fn({ target: this });
    },
    getContext() {
      return { putImageData() {} };
    },
    requestFullscreen() {
      document.fullscreenElement = this;
      return Promise.resolve();
    }
  };
  document.register(node);
  return node;
}

function createEnv(storageOptions) {
  const document = makeDocument();
  const sandbox = {
    console: { log() {}, warn() {}, error() {}, info() {}, debug() {} },
    document,
    localStorage: storage(storageOptions),
    sessionStorage: storage(),
    location: { hash: "", hostname: "127.0.0.1" },
    navigator: { language: "ko", userAgent: "node" },
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
    Intl
  };
  sandbox.window = sandbox;
  sandbox.globalThis = sandbox;
  sandbox.addEventListener = () => {};
  sandbox.removeEventListener = () => {};
  sandbox.state = { vmConsoleFrame: null, vmConsoleKeysBound: false, selectedVm: { id: "id-1" } };
  sandbox.els = { vmDetailPanel: null };
  sandbox.rbacAllows = () => true;
  sandbox.requireRbac = () => {};
  sandbox.pcvEscapeHtml = (value) => String(value ?? "");
  sandbox.getVmId = (vm) => (vm && (vm.id || vm.vm_id)) || "";
  sandbox.normalizeError = (error) => ({
    code: error && error.code ? error.code : "PCV_ERROR",
    message: error && error.message ? error.message : String(error)
  });
  sandbox.render = () => {};
  sandbox.desktopApi = {
    getVmConsoleFrame: async () => ({}),
    sendVmConsoleInput: async () => {}
  };
  vm.createContext(sandbox);
  for (const file of ["i18n.js", "src/modules/vm-console.ts"]) {
    vm.runInContext(fs.readFileSync(webRoot + file, "utf8"), sandbox, { filename: file });
  }
  return { sandbox, document };
}

test("default frame size is 1024x768 and the other two sizes stay selectable", () => {
  const { sandbox } = createEnv();
  const html = sandbox.renderVmConsoleFrameCard({ state: "running" }, "id-1", true);
  assert.match(html, /<option value="1024x768" selected>/);
  assert.match(html, /<option value="800x600">/);
  assert.match(html, /<option value="640x480">/);
  assert.match(html, /id="pcv-console-fit"[^>]*aria-pressed="true"/);
  assert.match(html, /화면 시작/);
  assert.match(html, /창에 맞춤/);
  assert.match(html, /화면 밖을 클릭하면 입력이 해제됩니다\./);
  assert.match(html, /data-action="vm-console"/);
  assert.equal(html.includes("background:#000"), false);
  assert.equal(html.includes("Start screen"), false);
  assert.equal(html.includes("Browser console"), false);
});

test("fit scale is 50 percent when a 1024 frame sits in a 512 container", () => {
  const { sandbox } = createEnv();
  assert.equal(sandbox.consoleDisplayScale("fit", 1024, 768, 512, 2000), 0.5);
  const shortViewport = sandbox.consoleDisplayScale("fit", 1024, 768, 512, 100);
  assert.ok(Math.abs(shortViewport - (100 / 768)) < 1e-12);
  assert.equal(sandbox.consoleDisplayScale("fit", 1024, 768, 0, 2000), 1);
});

test("actual size keeps the display scale at 100 percent", () => {
  const { sandbox } = createEnv();
  assert.equal(sandbox.consoleDisplayScale("actual", 1024, 768, 512, 2000), 1);
  assert.equal(sandbox.consoleDisplayScale("actual", 640, 480, 200, 100), 1);
});

test("fullscreen requests the console stage and a rejection disables the button", async () => {
  const { sandbox, document } = createEnv();
  const stage = makeEl(document, "pcv-console-stage");
  const button = makeEl(document, "pcv-console-fullscreen", "button");
  const note = makeEl(document, "pcv-console-fullscreen-note", "p");
  let calls = 0;
  stage.requestFullscreen = () => {
    calls += 1;
    document.fullscreenElement = stage;
    return Promise.resolve();
  };
  await sandbox.requestVmConsoleFullscreen();
  assert.equal(calls, 1);
  assert.equal(button.disabled, false);
  assert.equal(note.textContent, "");

  stage.requestFullscreen = () => {
    calls += 1;
    return Promise.reject(new Error("denied"));
  };
  await sandbox.requestVmConsoleFullscreen();
  assert.equal(calls, 2);
  assert.equal(button.disabled, true);
  assert.equal(note.textContent, "전체 화면을 열 수 없습니다.");

  document.fullscreenEnabled = false;
  const denied = sandbox.renderVmConsoleFrameCard({ state: "running" }, "id-1", true);
  assert.match(denied, /id="pcv-console-fullscreen"[^>]*disabled/);
  document.fullscreenEnabled = true;
  delete document.documentElement.requestFullscreen;
  const unsupported = sandbox.renderVmConsoleFrameCard({ state: "running" }, "id-1", true);
  assert.match(unsupported, /id="pcv-console-fullscreen"[^>]*disabled/);
});

test("Ctrl+Alt+Del sends kind ctrl-alt-del and the button is disabled without console.input", async () => {
  const { sandbox } = createEnv();
  const sent = [];
  sandbox.desktopApi.sendVmConsoleInput = async (vmId, payload) => {
    sent.push({ vmId, kind: payload && payload.kind });
  };
  await sandbox.handleVmDetailExtensionClick({ dataset: { action: "vm-console-input-cad", vmId: "id-1" } });
  assert.equal(sent.length, 1);
  assert.equal(sent[0].vmId, "id-1");
  assert.equal(sent[0].kind, "ctrl-alt-del");

  sandbox.rbacAllows = (permission) => permission !== "console.input";
  const html = sandbox.renderVmConsoleFrameCard({ state: "running" }, "id-1", true);
  assert.match(html, /data-action="vm-console-input-cad"[^>]*disabled/);
  assert.equal(html.includes("data-action=\"vm-console-text\""), false);
  assert.match(html, /입력 권한 없음/);
  const sentBefore = sent.length;
  await sandbox.handleVmDetailExtensionClick({ dataset: { action: "vm-console-input-cad", vmId: "id-1" }, disabled: true });
  assert.equal(sent.length, sentBefore);
});

test("input chip shows capturing, click-to-type, and no-permission states", () => {
  const { sandbox, document } = createEnv();
  const canvas = makeEl(document, "vm-console-frame-canvas", "canvas");
  const chip = makeEl(document, "pcv-console-input-chip", "span");
  makeEl(document, "pcv-console-card");
  sandbox.rbacAllows = () => false;
  sandbox.bindVmConsoleFrame();
  assert.equal(chip.dataset.state, "denied");
  assert.equal(chip.textContent, "입력 권한 없음");

  sandbox.rbacAllows = () => true;
  sandbox.syncVmConsoleInputChip();
  assert.equal(chip.dataset.state, "idle");
  assert.equal(chip.textContent, "콘솔을 클릭하면 입력");

  canvas.focus();
  assert.equal(document.activeElement, canvas);
  assert.equal(chip.dataset.state, "active");
  assert.equal(chip.textContent, "입력 중");

  canvas.blur();
  assert.equal(chip.dataset.state, "idle");
  assert.equal(chip.textContent, "콘솔을 클릭하면 입력");
});

test("frame polling pauses while the console tab is hidden and resumes when it shows", async () => {
  const { sandbox, document } = createEnv();
  const panel = makeEl(document, "console-panel");
  const stage = makeEl(document, "pcv-console-stage");
  const canvas = makeEl(document, "vm-console-frame-canvas", "canvas");
  panel.hidden = true;
  canvas.parentNode = stage;
  stage.parentNode = panel;
  panel.parentNode = document;
  let calls = 0;
  sandbox.desktopApi.getVmConsoleFrame = async () => {
    calls += 1;
    const error = new Error("limited");
    error.code = "PCV_CONSOLE_RATE_LIMITED";
    throw error;
  };
  sandbox.state.vmConsoleFrame = {
    vm_id: "id-1",
    size: "1024x768",
    fps: 1000,
    running: true,
    generation: 7,
    image: null,
    error: null,
    timer: null
  };
  try {
    await sandbox.pollVmConsoleFrame("id-1", 7);
    assert.equal(calls, 0);
    assert.equal(sandbox.state.vmConsoleFrame.running, true);
    assert.ok(sandbox.state.vmConsoleFrame.timer);
    panel.hidden = false;
    await new Promise((resolve) => setTimeout(resolve, 40));
    assert.ok(calls >= 1);
    assert.equal(sandbox.state.vmConsoleFrame.running, true);
  } finally {
    sandbox.stopVmConsoleFrame();
  }
});

test("frame polling pauses while the document is hidden and resumes when it is visible", async () => {
  const { sandbox, document } = createEnv();
  const stage = makeEl(document, "pcv-console-stage");
  const canvas = makeEl(document, "vm-console-frame-canvas", "canvas");
  canvas.parentNode = stage;
  stage.parentNode = document;
  document.hidden = true;
  let calls = 0;
  sandbox.desktopApi.getVmConsoleFrame = async () => {
    calls += 1;
    const error = new Error("limited");
    error.code = "PCV_CONSOLE_RATE_LIMITED";
    throw error;
  };
  sandbox.state.vmConsoleFrame = {
    vm_id: "id-1",
    size: "1024x768",
    fps: 1000,
    running: true,
    generation: 8,
    image: null,
    error: null,
    timer: null
  };
  try {
    await sandbox.pollVmConsoleFrame("id-1", 8);
    assert.equal(calls, 0);
    assert.equal(sandbox.state.vmConsoleFrame.running, true);
    assert.equal(sandbox.vmConsoleStageVisible(), false);
    document.hidden = false;
    await new Promise((resolve) => setTimeout(resolve, 40));
    assert.ok(calls >= 1);
    assert.equal(sandbox.vmConsoleStageVisible(), true);
  } finally {
    sandbox.stopVmConsoleFrame();
  }
});

test("fit, size, and fps choices are remembered", () => {
  const { sandbox } = createEnv();
  sandbox.writeVmConsolePref({ fit: "actual", size: "800x600", fps: 5 });
  const pref = sandbox.readVmConsolePref();
  assert.equal(pref.fit, "actual");
  assert.equal(pref.size, "800x600");
  assert.equal(pref.fps, 5);
  const html = sandbox.renderVmConsoleFrameCard({ state: "off" }, "vm-1", true);
  assert.match(html, /<option value="800x600" selected>/);
  assert.match(html, /<option value="5" selected>/);
  assert.match(html, /id="pcv-console-actual"[^>]*aria-pressed="true"/);
  sandbox.localStorage.setItem("pcv-console-size", "9999x9999");
  assert.equal(sandbox.readVmConsolePref().size, "1024x768");
});

test("storage read and write failures fall back without throwing", () => {
  const { sandbox } = createEnv();
  sandbox.localStorage = {
    getItem() { throw new Error("read blocked"); },
    setItem() { throw new Error("write blocked"); }
  };
  const pref = sandbox.readVmConsolePref();
  assert.equal(pref.fit, "fit");
  assert.equal(pref.size, "1024x768");
  assert.equal(pref.fps, 2);
  assert.equal(sandbox.writeVmConsolePref({ fit: "actual", size: "640x480", fps: 1 }), false);
  const html = sandbox.renderVmConsoleFrameCard({ state: "running" }, "id-1", true);
  assert.match(html, /<option value="1024x768" selected>/);
});

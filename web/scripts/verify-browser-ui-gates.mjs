// Browser UI gate: loopback static server, fixed mock API, Edge/Chrome over CDP, axe-core ratchet.
import { spawn, spawnSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync, readFileSync, rmSync, statSync, unlinkSync, writeFileSync } from "node:fs";
import http from "node:http";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
let staticRoot = path.join(repoRoot, "web");
const axePath = path.join(repoRoot, "web", "node_modules", "axe-core", "axe.min.js");
const baselinePath = path.join(repoRoot, "web", "config", "browser-ui-gates-baseline.json");
const BASELINE_CONTRACT = "pcv-browser-ui-gates-baseline-v2";
const BASELINE_TODO_REASON = "TODO: 눈으로 확인 뒤 이유를 적는다";
const BASELINE_UPDATE_HINT = "의도한 변경이면 `node web/scripts/verify-browser-ui-gates.mjs --write-baseline` 뒤 새 reason을 확인하라";
const VIEWPORT = { width: 1280, height: 720 };
const THEMES = ["supanova", "supanova-cyan"];
const DEMO_TOKEN = "pcv-demo-token";
const UNSERVED_DIRS = new Set(["src", "node_modules", "scripts", "tests", "node-tests", "contracts", "config", "generated"]);
const UNSERVED_FILES = new Set(["package.json", "package-lock.json", "tsconfig.json", "eslint.config.js"]);
const EDGE_PATHS = [
  "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
  "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
];
const LINUX_BROWSERS = ["google-chrome", "google-chrome-stable", "chromium", "chromium-browser", "microsoft-edge"];
const CONTENT_TYPES = {
  ".css": "text/css; charset=utf-8",
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".map": "application/json",
  ".mjs": "text/javascript; charset=utf-8",
  ".png": "image/png",
  ".svg": "image/svg+xml",
  ".webp": "image/webp",
  ".woff": "font/woff",
  ".woff2": "font/woff2",
  ".txt": "text/plain; charset=utf-8",
  ".md": "text/markdown; charset=utf-8",
  ".webmanifest": "application/manifest+json"
};

const SESSION = {
  username: "pcv-demo",
  role: "admin",
  permissions: ["*"],
  subject: "pcv-demo",
  display_name: "pcv-demo"
};
const VMS = [
  { id: "pcv-demo-running", name: "pcv-demo-running", state: "Running", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
  { id: "pcv-demo-off", name: "pcv-demo-off", state: "Off", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
  { id: "pcv-demo-failed", name: "pcv-demo-failed", state: "Failed", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 } },
  { id: "pcv-demo-template", name: "pcv-demo-template", state: "Off", generation: 2, cpu: { count: 2 }, memory: { startup_mb: 2048 }, template_lock: true }
];
const CHECKPOINT = { id: "cp-demo-1", name: "pcv-demo-baseline", created_at: "2026-11-01T00:00:00Z" };
const JOB = { job_id: "job-demo-1", status: "succeeded", operation: "mock" };
const DIFFERENCES = [
  "helppage는 사이드바에 있지만 VALID_VIEWS에는 없다. 클릭하면 그 section만 보이고 aria-current는 page다.",
  "사이드바 키보드는 ArrowUp/ArrowDown만 처리한다. Home/End는 없다.",
  "VM 상세 탭은 클릭과 ArrowRight/Left/Up/Down, Home, End를 처리한다.",
  "더 보기 메뉴는 ArrowUp/ArrowDown, Home, End, Escape(토글로 포커스 복귀)를 처리한다. ArrowLeft/Right는 없다.",
  "새 VM 마법사 단계는 ArrowLeft/ArrowRight만 처리한다. Home/End와 Tab 가두기는 없다.",
  "confirmDialog는 열릴 때 안쪽으로 포커스를 옮기고 Tab을 가두며 Escape로 닫고 opener로 되돌린다. VM 상세 클릭 처리는 그 다음 render()로 상세 DOM을 갈아 끼워 복원된 opener가 사라진다.",
  "modalCore 대화상자(복제, 비밀번호 재설정)는 Tab 가두기와 opener 복원을 구현하지 않는다. showModal이 안쪽 포커스를 주고 Escape는 닫는다.",
  "삭제는 VM 이름을 입력해야 확인이 켜진다. 체크포인트 복원 확인은 이름 입력이 없는 danger confirm이다.",
  "테마 이름은 supanova(기본, 라이트)와 supanova-cyan(다크)이다. supanova-hicontrast와 supanova-mockup도 있으나 이번 축은 두 테마만 검사한다.",
  "비밀번호 재설정은 #account-reset-form 대화상자다. 별도 route가 아니다.",
  "루프백 성공 표시 이름은 제품이 loopback-session으로 고정한다. 비밀번호 변경 버튼은 세션 username을 본다.",
  "첫 navigateTo 전에는 index.html이 helppage만 hidden으로 둔다. shell.mount의 setActive는 사이드바 표시만 바꾸고, section의 hidden/active는 _renderContentPaint가 칠한다. 게이트는 대시보드 항목을 클릭한 뒤에 배타 표시를 확인한다.",
  "복제 메뉴 항목은 더 보기 메뉴를 닫지 않는다. 대화상자를 닫은 뒤 토글을 다시 누르면 메뉴가 닫힌다. 게이트는 메뉴가 이미 열려 있으면 토글을 누르지 않는다.",
  "게이트는 테마 전환 transition을 끄고 멈춘 색만 검사한다. body와 button의 color 전환이 0.3초 동안 이전 테마 글자색을 새 배경 위에 남긴다."
];

const HELPERS = `
window.__pcvGate = {
  applyTheme: function (name) {
    document.documentElement.setAttribute('data-theme', name);
    try { localStorage.setItem('pcv-theme', name); } catch (err) {}
    if (window.state) window.state.theme = name;
    var select = document.getElementById('theme-select');
    if (select) select.value = name;
    if (!document.getElementById('pcv-gate-no-transition')) {
      var style = document.createElement('style');
      style.id = 'pcv-gate-no-transition';
      style.textContent = '*,*::before,*::after{transition:none !important;animation:none !important;}';
      document.head.appendChild(style);
    }
    return document.documentElement.getAttribute('data-theme');
  },
  loginReady: function () {
    var splash = document.getElementById('splash');
    var login = document.getElementById('login-page');
    var display = login ? getComputedStyle(login).display : '';
    var hidden = login ? login.getAttribute('aria-hidden') : null;
    return { ok: !splash && !!login && display !== 'none' && hidden !== 'true', splash: !!splash, display: display, hidden: hidden };
  },
  shellReady: function () {
    var app = document.getElementById('app');
    var login = document.getElementById('login-page');
    var loginDisplay = login ? getComputedStyle(login).display : '';
    var error = window.state && state.error ? { code: String(state.error.code || ''), message: String(state.error.message || '').slice(0, 180) } : null;
    var session = window.state && state.authSession;
    var ready = !!(window.state && state.connectionState === 'connected' && !state.loading && app && app.getAttribute('aria-hidden') !== 'true' && loginDisplay === 'none');
    return {
      ok: ready,
      connection: window.state && state.connectionState,
      loading: !!(window.state && state.loading),
      loginDisplay: loginDisplay,
      appHidden: app ? app.getAttribute('aria-hidden') : null,
      error: error,
      username: session ? String(session.username || '') : '',
      nav: document.querySelectorAll('#shell-sidebar .shell-navitem').length,
      userLabel: (document.getElementById('us-name') || {}).textContent || ''
    };
  },
  hideLogin: function () {
    if (typeof pcvSetLoginVisible === 'function') pcvSetLoginVisible(false);
    return true;
  },
  debug: function () {
    var session = window.state && state.authSession;
    var partial = window.state && state.partialFailures;
    return {
      connection: window.state && state.connectionState,
      loading: !!(window.state && state.loading),
      username: session ? String(session.username || '') : '',
      permissions: session && session.permissions ? session.permissions.slice(0, 8) : [],
      vms: window.state && state.vms ? state.vms.length : 0,
      error: window.state && state.error ? String(state.error.code || '') : '',
      partial: partial ? partial.map(function (item) { return String(item.code || item.operation || ''); }).slice(0, 8) : []
    };
  },
  secrets: function () {
    var text = document.body ? document.body.innerText : '';
    return {
      ok: text.indexOf(${JSON.stringify(DEMO_TOKEN)}) === -1 && !/[A-Za-z]:\\\\Users\\\\(?!Public\\\\)/.test(text) && !/\\b(?:10\\.\\d|192\\.168|172\\.(?:1[6-9]|2\\d|3[01]))\\./.test(text),
      token: text.indexOf(${JSON.stringify(DEMO_TOKEN)}) !== -1
    };
  },
  click: function (selector) {
    var node = document.querySelector(selector);
    if (!node) return { ok: false, reason: 'missing', selector: selector };
    node.click();
    return { ok: true };
  },
  focus: function (selector) {
    var node = document.querySelector(selector);
    if (!node || typeof node.focus !== 'function') return { ok: false, selector: selector };
    node.focus();
    return { ok: document.activeElement === node };
  },
  key: function (selector, keyName) {
    var node = document.querySelector(selector);
    if (!node) return { ok: false, selector: selector };
    node.dispatchEvent(new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true }));
    var active = document.activeElement;
    return {
      ok: true,
      activeNav: active && active.dataset ? active.dataset.nav || '' : '',
      activeRole: active ? active.getAttribute('role') || '' : '',
      activeId: active ? active.id || '' : ''
    };
  },
  setInput: function (selector, value) {
    var node = document.querySelector(selector);
    if (!node) return { ok: false, selector: selector };
    node.focus();
    node.value = value;
    node.dispatchEvent(new Event('input', { bubbles: true }));
    node.dispatchEvent(new Event('change', { bubbles: true }));
    return { ok: true, value: node.value };
  },
  viewState: function (id) {
    var sections = Array.prototype.slice.call(document.querySelectorAll('.app-view[data-view]'));
    var visible = sections.filter(function (section) { return !section.hidden && section.classList.contains('active'); }).map(function (section) { return section.dataset.view; });
    var nav = document.querySelector('#shell-sidebar .shell-navitem[data-nav="' + id + '"]');
    var current = Array.prototype.slice.call(document.querySelectorAll('#shell-sidebar .shell-navitem[aria-current="page"]')).map(function (node) { return node.dataset.nav; });
    var activeClass = !!(nav && nav.classList.contains('active'));
    var aria = nav ? nav.getAttribute('aria-current') : null;
    return {
      ok: visible.length === 1 && visible[0] === id && aria === 'page' && activeClass && current.length === 1 && current[0] === id,
      visible: visible,
      current: current,
      aria: aria,
      activeClass: activeClass
    };
  },
  failedChip: function () {
    var count = document.querySelector('#vm-state-chips [data-vm-chip="failed"] [data-chip-count]');
    var row = document.querySelector('#vm-table tr[data-vm-id="pcv-demo-failed"]');
    var text = count ? String(count.textContent || '').trim() : '';
    return { ok: text !== '' && text !== '0' && !!row, count: text, row: !!row };
  },
  detailReady: function () {
    var name = document.querySelector('.pcv-vm-name');
    var tabs = document.querySelectorAll('[data-pcv-vm-tabs] [role="tab"]');
    var text = name ? name.textContent : '';
    return { ok: text.indexOf('pcv-demo-off') !== -1 && tabs.length === 6, text: text, tabs: tabs.length };
  },
  tabState: function () {
    var tabs = Array.prototype.slice.call(document.querySelectorAll('[data-pcv-vm-tabs] [role="tab"]'));
    var panels = Array.prototype.slice.call(document.querySelectorAll('[data-pcv-vm-tabs] [role="tabpanel"]'));
    var selected = tabs.map(function (tab) { return tab.getAttribute('aria-selected'); });
    var hidden = panels.map(function (panel) { return panel.hidden === true; });
    var selectedIndex = selected.indexOf('true');
    var visiblePanels = hidden.filter(function (value) { return value === false; }).length;
    return {
      ok: selectedIndex >= 0 && visiblePanels === 1 && hidden[selectedIndex] === false && selected.filter(function (value) { return value === 'true'; }).length === 1,
      selectedIndex: selectedIndex,
      labels: tabs.map(function (tab) { return (tab.textContent || '').trim(); }),
      selected: selected,
      hidden: hidden
    };
  },
  clickTab: function (index) {
    var tabs = document.querySelectorAll('[data-pcv-vm-tabs] [role="tab"]');
    var tab = tabs[index];
    if (!tab) return { ok: false, index: index };
    tab.click();
    return { ok: true };
  },
  keyTab: function (keyName) {
    var tablist = document.querySelector('[data-pcv-vm-tabs] [role="tablist"]');
    var selected = tablist ? tablist.querySelector('[aria-selected="true"]') : null;
    if (!selected) return { ok: false };
    selected.focus();
    selected.dispatchEvent(new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true }));
    return { ok: true };
  },
  menuState: function () {
    var menu = document.getElementById('pcv-vm-more-menu');
    var toggle = document.querySelector('[data-action="vm-more-toggle"]');
    var active = document.activeElement;
    return {
      ok: !!(menu && toggle && menu.hidden === false && toggle.getAttribute('aria-expanded') === 'true'),
      hidden: menu ? menu.hidden : null,
      expanded: toggle ? toggle.getAttribute('aria-expanded') : null,
      focusRole: active ? active.getAttribute('role') || '' : '',
      focusInside: !!(menu && active && menu.contains(active))
    };
  },
  confirmState: function () {
    var dlg = document.querySelector('.pcv-confirm');
    if (!dlg) return { ok: false, open: false };
    var accept = dlg.querySelector('[data-pcv-confirm="accept"]');
    var active = document.activeElement;
    return {
      ok: dlg.open === true,
      open: dlg.open === true,
      focusInside: !!(active && dlg.contains(active)),
      acceptDisabled: !!(accept && accept.disabled),
      hasTyped: !!dlg.querySelector('input.pcv-confirm-typed')
    };
  },
  tabTrap: function (selector) {
    var dlg = document.querySelector(selector);
    if (!dlg) return { ok: false, reason: 'missing' };
    dlg.dispatchEvent(new KeyboardEvent('keydown', { key: 'Tab', bubbles: true, cancelable: true }));
    var after = document.activeElement;
    return { ok: !!(after && dlg.contains(after)), stayed: !!(after && dlg.contains(after)) };
  },
  focusInside: function (selector) {
    var node = document.querySelector(selector);
    var dlg = node && node.closest ? (node.closest('dialog') || node) : null;
    var active = document.activeElement;
    return { ok: !!(dlg && active && dlg.contains(active)), open: !!(dlg && dlg.open) };
  },
  stepState: function (step) {
    var panel = document.getElementById('create-vm-step-' + step);
    var tab = document.getElementById('create-vm-step-tab-' + step);
    var dialog = document.getElementById('create-vm-dialog');
    return {
      ok: !!(dialog && dialog.open && panel && panel.hidden === false && tab && tab.getAttribute('aria-current') === 'step'),
      open: !!(dialog && dialog.open),
      current: tab ? tab.getAttribute('aria-current') : null
    };
  },
  nextEnabled: function () {
    var next = document.getElementById('create-vm-next');
    return { ok: !!(next && next.disabled === false && next.hidden === false), disabled: next ? next.disabled : null };
  },
  restoreReady: function () {
    var button = document.querySelector('[data-action="checkpoint-restore"]');
    return { ok: !!(button && button.disabled !== true), disabled: button ? button.disabled === true : null };
  },
  wizardClosed: function () {
    var dialog = document.getElementById('create-vm-dialog');
    var confirm = document.querySelector('.pcv-confirm');
    return { ok: !!dialog && dialog.open !== true && !confirm, open: !!(dialog && dialog.open) };
  },
  passwordButton: function () {
    var button = document.getElementById('account-change-password');
    return { ok: !!(button && button.hidden !== true), present: !!button, hidden: button ? button.hidden === true : null };
  },
  resetOpen: function () {
    var form = document.getElementById('account-reset-form');
    var dlg = form && form.closest ? form.closest('dialog') : null;
    var active = document.activeElement;
    return { ok: !!(form && dlg && dlg.open && active && dlg.contains(active)), open: !!(dlg && dlg.open) };
  },
  resetClosed: function () {
    return { ok: !document.getElementById('account-reset-form') };
  },
  cloneOpen: function () {
    var body = document.querySelector('.pcv-clone-modal');
    var dlg = body && body.closest ? body.closest('dialog') : null;
    var active = document.activeElement;
    return { ok: !!(dlg && dlg.open && active && dlg.contains(active)), open: !!(dlg && dlg.open) };
  },
  cloneClosed: function () {
    return { ok: !document.querySelector('.pcv-clone-modal') };
  },
  activeMarker: function () {
    var active = document.activeElement;
    if (!active) return '';
    return active.getAttribute('data-action') || active.getAttribute('data-nav') || active.id || active.getAttribute('role') || active.tagName;
  },
  axe: async function () {
    if (typeof axe === 'undefined') return { error: 'axe-missing' };
    var context = document;
    var openDialogs = Array.prototype.slice.call(document.querySelectorAll('dialog[open]'));
    if (openDialogs.length) {
      context = openDialogs[openDialogs.length - 1];
      var active = document.activeElement;
      for (var d = 0; d < openDialogs.length; d++) {
        if (active && openDialogs[d].contains(active)) context = openDialogs[d];
      }
    }
    var result = await axe.run(context, {
      runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] },
      resultTypes: ['violations', 'incomplete']
    });
    function mapList(list) {
      return (list || []).map(function (item) {
        return {
          id: item.id,
          impact: item.impact || '',
          nodes: (item.nodes || []).map(function (node) {
            var checks = (node.any || []).concat(node.all || []).concat(node.none || []);
            var contrast = null;
            for (var i = 0; i < checks.length; i++) {
              var data = checks[i] && checks[i].data;
              if (data && (data.contrastRatio || data.fgColor || data.bgColor || data.messageKey)) {
                contrast = {
                  fg: data.fgColor || '',
                  bg: data.bgColor || '',
                  ratio: data.contrastRatio || null,
                  message: data.messageKey || ''
                };
                break;
              }
            }
            return {
              target: (node.target || []).join(' '),
              html: String(node.html || '').slice(0, 160),
              contrast: contrast
            };
          })
        };
      });
    }
    return {
      violations: mapList(result.violations),
      incomplete: mapList((result.incomplete || []).filter(function (item) { return item.id === 'color-contrast'; }))
    };
  }
};
`;

function getArg(name) {
  const argv = process.argv.slice(2);
  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === `--${name}`) return argv[index + 1] || "";
    if (arg.startsWith(`--${name}=`)) return arg.slice(name.length + 3);
  }
  return "";
}

function hasFlag(name) {
  return process.argv.slice(2).some((arg) => arg === `--${name}`);
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function withTimeout(promise, ms, label) {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(`timeout ${label}`)), ms);
    promise.then((value) => {
      clearTimeout(timer);
      resolve(value);
    }, (error) => {
      clearTimeout(timer);
      reject(error);
    });
  });
}

function stamp() {
  return new Date().toISOString().replace(/[-:]/g, "").replace(/\.\d{3}Z$/, "Z");
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

function killBrowsersUsingProfile(profileDir) {
  if (process.platform !== "win32" || !profileDir) return;
  const marker = path.basename(profileDir);
  if (!marker.startsWith("pcv-browser-ui-gates-")) return;
  const command = `$marker = ${JSON.stringify(marker)}; Get-CimInstance Win32_Process -Filter "Name = 'msedge.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($marker) } | ForEach-Object { & taskkill.exe /PID $_.ProcessId /T /F | Out-Null }`;
  spawnSync("powershell.exe", ["-NoProfile", "-Command", command], { stdio: "ignore" });
}

async function killBrowser(child, profileDir) {
  if (child && child.pid != null && child.exitCode === null) {
    if (process.platform === "win32") {
      spawnSync("taskkill.exe", ["/PID", String(child.pid), "/T", "/F"], { stdio: "ignore" });
    } else {
      try { process.kill(-child.pid, "SIGKILL"); } catch { try { child.kill("SIGKILL"); } catch { /* already exited */ } }
    }
    await new Promise((resolve) => {
      const timer = setTimeout(resolve, 2000);
      child.once("exit", () => {
        clearTimeout(timer);
        resolve();
      });
    });
    if (child.exitCode === null) {
      try { child.kill("SIGKILL"); } catch { /* already exited */ }
      await sleep(400);
    }
  }
  killBrowsersUsingProfile(profileDir);
}

async function removeDir(dir) {
  const deadline = Date.now() + 8000;
  while (Date.now() < deadline) {
    try {
      rmSync(dir, { recursive: true, force: true });
      if (!existsSync(dir)) return true;
    } catch {
      /* Windows can keep the profile lock for a moment after the browser exits. */
    }
    await sleep(250);
  }
  return !existsSync(dir);
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

async function waitForFile(filePath, timeoutMs = 15000) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (existsSync(filePath)) return readFileSync(filePath, "utf8");
    await sleep(100);
  }
  throw new Error("DevToolsActivePort was not created");
}

async function evaluate(client, expression) {
  const result = await withTimeout(client.send("Runtime.evaluate", {
    expression,
    awaitPromise: true,
    returnByValue: true
  }), 30000, "evaluate");
  if (result.exceptionDetails) {
    const text = result.exceptionDetails.text || result.exceptionDetails.exception?.description || "evaluate failed";
    throw new Error(String(text).slice(0, 500));
  }
  return result.result?.value;
}

async function call(client, method, args = []) {
  const expression = `window.__pcvGate.${method}(${args.map((arg) => JSON.stringify(arg)).join(",")})`;
  return evaluate(client, expression);
}

async function waitFor(client, method, args = [], timeoutMs = 20000) {
  const deadline = Date.now() + timeoutMs;
  let last = null;
  while (Date.now() < deadline) {
    last = await call(client, method, args);
    if (last && last.ok) return last;
    await sleep(150);
  }
  const error = new Error(`wait timeout ${method}`);
  error.detail = last;
  throw error;
}

async function settle(client, method, args = [], timeoutMs = 20000) {
  try {
    return await waitFor(client, method, args, timeoutMs);
  } catch (error) {
    return error.detail || { ok: false, error: String(error.message || error) };
  }
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

async function pressEscape(client) {
  const key = {
    key: "Escape",
    code: "Escape",
    windowsVirtualKeyCode: 27,
    nativeVirtualKeyCode: 27
  };
  await client.send("Input.dispatchKeyEvent", { type: "keyDown", ...key });
  await client.send("Input.dispatchKeyEvent", { type: "keyUp", ...key });
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    req.on("data", () => {});
    req.on("end", resolve);
    req.on("error", reject);
  });
}

function send(res, status, body, contentType) {
  res.writeHead(status, {
    "content-type": contentType,
    "cache-control": "no-store",
    "content-length": Buffer.byteLength(body)
  });
  res.end(body);
}

function sendJson(res, status, payload) {
  send(res, status, JSON.stringify(payload), "application/json; charset=utf-8");
}

function mockData(method, pathname) {
  if (pathname === "/api/v1/auth/loopback-session" && method === "POST") {
    return { access_token: DEMO_TOKEN, session: SESSION };
  }
  if (pathname === "/api/v1/auth/session" && method === "GET") return SESSION;
  if (pathname === "/api/v1/auth/rbac" && method === "GET") return { role: "admin", permissions: ["*"] };
  if (pathname === "/api/v1/auth/refresh" && method === "POST") return { access_token: DEMO_TOKEN, session: SESSION };
  if (pathname === "/api/v1/accounts" && method === "GET") {
    return {
      accounts: [
        { username: "pcv-demo", role: "admin", enabled: true },
        { username: "pcv-demo-operator", role: "operator", enabled: true }
      ]
    };
  }
  if (pathname === "/api/v1/host/status") {
    return {
      product: "PureCVisor Desktop Node",
      version: "0.42.96-admin-smoke",
      hypervisor: "Hyper-V",
      service_state: "Running",
      firewall_rule_count: 0
    };
  }
  if (pathname === "/api/v1/ops/summary") {
    return {
      recent_activity: [],
      signals: [],
      errors: [],
      batch_evidence: { status: "not-claimed" },
      current_evidence: {},
      runtime_policy: { version: "0.42.96-admin-smoke" }
    };
  }
  if (pathname === "/api/v1/runtime/policy") {
    return { version: "0.42.96-admin-smoke", auth: { mode: "account", rbac: true }, web: { loopback: true } };
  }
  if (pathname === "/api/v1/network/inventory") {
    return {
      source: "network.inventory",
      mutating: false,
      switches: [{ name: "Default Switch", type: "internal", is_default: true, allow_management_os: true }]
    };
  }
  if (pathname === "/api/v1/console/capabilities") return { vmconnect: true, novnc: false };
  if (pathname === "/api/v1/jobs") return { jobs: [], limit: 50, offset: 0, returned: 0 };
  if (pathname === "/api/v1/diagnostics/bundles") return { bundles: [], limit: 10, offset: 0, returned: 0 };
  if (pathname === "/api/v1/vms" && method === "GET") return { vms: VMS };
  const vmMatch = pathname.match(/^\/api\/v1\/vms\/([^/]+)(.*)$/);
  if (vmMatch) {
    const id = decodeURIComponent(vmMatch[1]);
    const rest = vmMatch[2] || "";
    const vm = VMS.find((item) => item.id === id) || null;
    if (rest === "" && method === "GET") return vm || {};
    if (rest === "/checkpoints" && method === "GET") {
      return { checkpoints: id === "pcv-demo-off" ? [CHECKPOINT] : [] };
    }
    if (method !== "GET") return JOB;
    return {};
  }
  if (pathname.startsWith("/api/v1/jobs/")) return JOB;
  if (method === "GET") return {};
  return JOB;
}

function isUnserved(pathname) {
  const name = pathname.replace(/^\/+/, "");
  if (!name) return false;
  const first = name.split("/")[0];
  return UNSERVED_DIRS.has(first) || UNSERVED_FILES.has(name);
}

function startServer() {
  const requests = [];
  const gate = { held: true };
  let releaseHold = () => {};
  const hold = new Promise((resolve) => {
    releaseHold = resolve;
  });
  const releaseAuth = () => {
    if (!gate.held) return;
    gate.held = false;
    releaseHold();
  };
  const server = http.createServer((req, res) => {
    handleRequest(req, res, requests, gate, hold).catch((error) => {
      if (!res.headersSent) send(res, 500, "mock error", "text/plain; charset=utf-8");
      else res.end();
      console.error(`mock ${error.message}`);
    });
  });
  return new Promise((resolve) => {
    server.listen(0, "127.0.0.1", () => {
      const address = server.address();
      resolve({ server, port: address.port, requests, releaseAuth });
    });
  });
}

async function handleRequest(req, res, requests, gate, hold) {
  const url = new URL(req.url || "/", "http://127.0.0.1");
  const pathname = decodeURIComponent(url.pathname);
  const method = req.method || "GET";
  if (pathname.includes("\0") || pathname.split("/").includes("..")) {
    send(res, 400, "bad path", "text/plain; charset=utf-8");
    return;
  }
  if (requests.length < 4000) requests.push({ method, path: pathname });
  if (pathname.startsWith("/api/")) {
    await readBody(req);
    if (pathname === "/api/v1/auth/loopback-session" && method === "POST" && gate.held) await hold;
    sendJson(res, 200, { ok: true, data: mockData(method, pathname) });
    return;
  }
  if (isUnserved(pathname)) {
    send(res, 404, "unserved", "text/plain; charset=utf-8");
    return;
  }
  if (pathname === "/pcv-config.js") {
    const origin = `http://127.0.0.1:${req.socket.localPort}`;
    send(res, 200, `window.PCV_DESKTOP_NODE_CONFIG=${JSON.stringify({ apiBaseUrl: origin })};`, "text/javascript; charset=utf-8");
    return;
  }
  const relative = pathname === "/" ? "index.html" : pathname.replace(/^\/+/, "");
  const filePath = path.resolve(staticRoot, ...relative.split("/"));
  if (filePath !== staticRoot && !filePath.startsWith(staticRoot + path.sep)) {
    send(res, 400, "bad path", "text/plain; charset=utf-8");
    return;
  }
  if (!existsSync(filePath) || !statSync(filePath).isFile()) {
    send(res, 404, "missing", "text/plain; charset=utf-8");
    return;
  }
  let body = readFileSync(filePath);
  let contentType = CONTENT_TYPES[path.extname(filePath).toLowerCase()] || "application/octet-stream";
  if (path.basename(filePath) === "index.html") {
    const html = body.toString("utf8");
    const marker = '<script src="app.bundle.js?v=single-edge-v1"></script>';
    if (!html.includes(marker)) throw new Error("index bundle marker missing");
    const stub = `<script>(function(){var fake={scope:'/',active:null,installing:null,waiting:null,addEventListener:function(){},removeEventListener:function(){},update:function(){return Promise.resolve();},unregister:function(){return Promise.resolve(true);}};if(navigator.serviceWorker){navigator.serviceWorker.register=function(){return Promise.resolve(fake);};}})();</script>`;
    body = Buffer.from(html.replace(marker, `${stub}\n${marker}`), "utf8");
    contentType = CONTENT_TYPES[".html"];
  }
  res.writeHead(200, {
    "content-type": contentType,
    "cache-control": "no-store",
    "content-length": body.length
  });
  res.end(body);
}

function baselineKey(entry) {
  return [entry.screen, entry.theme, entry.result || "violation", entry.rule, entry.target].join("\0");
}

function splitBaselineKey(value) {
  const text = String(value);
  const first = text.indexOf("|");
  const second = first >= 0 ? text.indexOf("|", first + 1) : -1;
  if (first <= 0 || second <= first) throw new Error("browser ui baseline key mismatch");
  return {
    screen: text.slice(0, first),
    theme: text.slice(first + 1, second),
    target: text.slice(second + 1)
  };
}

function reasonGroupKey(kind, rule, messageKey) {
  return [kind, rule, messageKey].join("\0");
}

function isTodoReason(reason) {
  return String(reason || "").startsWith("TODO");
}

function loadBaseline() {
  const parsed = JSON.parse(readFileSync(baselinePath, "utf8"));
  if (!parsed || typeof parsed !== "object") throw new Error("browser ui baseline contract mismatch");
  if (parsed.contract === "pcv-browser-ui-gates-baseline-v1" && Array.isArray(parsed.entries)) {
    return {
      contract: parsed.contract,
      entries: parsed.entries.map((entry) => ({
        screen: entry.screen,
        theme: entry.theme,
        result: entry.result || "violation",
        rule: entry.rule,
        target: entry.target,
        count: Number(entry.count || 1),
        reason: entry.reason || "",
        messageKey: undefined
      }))
    };
  }
  if (parsed.contract !== BASELINE_CONTRACT || !Array.isArray(parsed.groups)) {
    throw new Error("browser ui baseline contract mismatch");
  }
  const entries = [];
  const seen = new Set();
  for (const group of parsed.groups) {
    if (!group || typeof group.reason !== "string" || typeof group.messageKey !== "string" || !Array.isArray(group.keys)) {
      throw new Error("browser ui baseline contract mismatch");
    }
    group.keys.forEach((value, index) => {
      const parts = splitBaselineKey(value);
      const entry = {
        screen: parts.screen,
        theme: parts.theme,
        result: group.kind || "incomplete",
        rule: group.rule,
        target: parts.target,
        count: Array.isArray(group.counts) ? Number(group.counts[index] || 1) : 1,
        reason: group.reason,
        messageKey: group.messageKey
      };
      const key = baselineKey(entry);
      if (seen.has(key)) throw new Error("browser ui baseline duplicate key");
      seen.add(key);
      entries.push(entry);
    });
  }
  return { contract: parsed.contract, entries };
}

function compareBaseline(observed, entries) {
  const expected = new Map(entries.map((entry) => [baselineKey(entry), entry]));
  const seen = new Set();
  const problems = [];
  for (const item of observed) {
    const key = baselineKey(item);
    seen.add(key);
    const baseline = expected.get(key);
    if (!baseline) {
      problems.push({ kind: "unexpected", ...item });
      continue;
    }
    if (item.count > Number(baseline.count)) problems.push({ kind: "increased", baseline: baseline.count, ...item });
    if (item.count < Number(baseline.count)) problems.push({ kind: "stale", message: "기준선을 줄여라", baseline: baseline.count, ...item });
  }
  for (const entry of entries) {
    if (seen.has(baselineKey(entry))) continue;
    problems.push({ kind: "stale", message: "기준선을 줄여라", screen: entry.screen, theme: entry.theme, result: entry.result || "violation", rule: entry.rule, target: entry.target, count: 0, baseline: entry.count });
  }
  return problems;
}

function messageKeyOf(item) {
  return (item.contrast && item.contrast.message) || "";
}

function inheritReasons(incompleteItems, entries) {
  const reasonMap = new Map();
  for (const entry of entries) {
    if (entry.messageKey === undefined) continue;
    const key = reasonGroupKey(entry.result, entry.rule, entry.messageKey);
    if (!reasonMap.has(key)) reasonMap.set(key, entry.reason);
  }
  const votes = new Map();
  const legacy = new Map(entries.filter((entry) => entry.messageKey === undefined).map((entry) => [baselineKey(entry), entry]));
  for (const item of incompleteItems) {
    const old = legacy.get(baselineKey(item));
    if (!old) continue;
    const key = reasonGroupKey(item.result || "incomplete", item.rule, messageKeyOf(item));
    if (reasonMap.has(key)) continue;
    const tally = votes.get(key) || new Map();
    tally.set(old.reason, (tally.get(old.reason) || 0) + 1);
    votes.set(key, tally);
  }
  for (const [key, tally] of votes) {
    let best = "";
    let bestCount = -1;
    for (const [reason, count] of tally) {
      if (count > bestCount || (count === bestCount && String(reason) < best)) {
        best = reason;
        bestCount = count;
      }
    }
    reasonMap.set(key, best);
  }
  return reasonMap;
}

function buildBaselineGroups(incompleteItems, entries) {
  const reasonMap = inheritReasons(incompleteItems, entries);
  const groups = new Map();
  for (const item of incompleteItems) {
    const kind = item.result || "incomplete";
    const messageKey = messageKeyOf(item);
    const key = reasonGroupKey(kind, item.rule, messageKey);
    const reason = reasonMap.has(key) ? reasonMap.get(key) : BASELINE_TODO_REASON;
    let group = groups.get(key);
    if (!group) {
      group = { kind, rule: item.rule, messageKey, reason, keys: new Map() };
      groups.set(key, group);
    }
    const storedKey = `${item.screen}|${item.theme}|${item.target}`;
    const count = Number(item.count || 1);
    const previous = group.keys.get(storedKey);
    group.keys.set(storedKey, previous ? Math.max(previous, count) : count);
  }
  return [...groups.values()]
    .sort((a, b) => reasonGroupKey(a.kind, a.rule, a.messageKey).localeCompare(reasonGroupKey(b.kind, b.rule, b.messageKey)))
    .map((group) => {
      const keys = [...group.keys.keys()].sort((a, b) => (a < b ? -1 : a > b ? 1 : 0));
      const stored = { kind: group.kind, rule: group.rule, messageKey: group.messageKey, reason: group.reason, keys };
      const counts = keys.map((value) => group.keys.get(value));
      if (counts.some((count) => count !== 1)) stored.counts = counts;
      return stored;
    });
}

function writeBaseline(groups) {
  writeFileSync(baselinePath, `${JSON.stringify({ contract: BASELINE_CONTRACT, groups }, null, 2)}\n`);
  return groups.reduce((sum, group) => sum + group.keys.length, 0);
}

function addObserved(observed, screen, theme, items, resultKind) {
  const counts = new Map();
  for (const item of items || []) {
    const nodes = item.nodes && item.nodes.length ? item.nodes : [{ target: "(document)", html: "" }];
    for (const node of nodes) {
      const target = stableTarget(node.target || "(document)", node.html || "");
      const key = `${item.id}\0${target}`;
      const current = counts.get(key) || { rule: item.id, target, count: 0, impact: item.impact || "", html: node.html || "", contrast: node.contrast || null };
      current.count += 1;
      if (!current.html && node.html) current.html = node.html;
      if (!current.contrast && node.contrast) current.contrast = node.contrast;
      counts.set(key, current);
    }
  }
  for (const value of counts.values()) observed.push({ screen, theme, result: resultKind, ...value });
}

function stableTarget(target, html) {
  let value = String(target).replace(/#pcv-tab-(summary|console|checkpoints|hardware|guest|jobs)-\d+/g, "#pcv-tab-$1");
  if (value === ":root" && String(html).includes("\u25cf")) value = "#shell-sync > span";
  if (value.startsWith(":root > thead >") || value.startsWith(":root > tbody >")) {
    value = `#vm-table > .pcv-vm-table > ${value.slice(":root > ".length)}`;
  }
  return value;
}

function nodeCount(items) {
  return (items || []).reduce((sum, item) => sum + ((item.nodes || []).length || 1), 0);
}

function prepareScreenshots(dir) {
  mkdirSync(dir, { recursive: true });
  for (const name of readdirSync(dir)) {
    if (name.toLowerCase().endsWith(".png")) unlinkSync(path.join(dir, name));
  }
}

async function capture(client, filePath) {
  await client.send("Emulation.setDeviceMetricsOverride", {
    width: VIEWPORT.width,
    height: VIEWPORT.height,
    deviceScaleFactor: 1,
    mobile: false
  });
  await sleep(80);
  const result = await client.send("Page.captureScreenshot", {
    format: "png",
    fromSurface: true,
    captureBeyondViewport: false
  });
  writeFileSync(filePath, Buffer.from(result.data, "base64"));
}

function mutationsSince(requests, index) {
  return requests.slice(index).filter((entry) => entry.method !== "GET" && entry.method !== "HEAD" && entry.path.startsWith("/api/"));
}

async function main() {
  const started = Date.now();
  const screenshotArg = getArg("screenshots");
  const screenshotDir = screenshotArg ? path.resolve(repoRoot, screenshotArg) : "";
  const themeShots = hasFlag("theme-shots");
  const webRootArg = getArg("web-root");
  if (webRootArg) staticRoot = path.resolve(repoRoot, webRootArg);
  const runStamp = stamp();
  const artifactDir = path.join(repoRoot, "artifacts", "browser-ui-gates", runStamp);
  mkdirSync(artifactDir, { recursive: true });
  if (screenshotDir) prepareScreenshots(screenshotDir);
  const summary = {
    ok: false,
    contract: "pcv-browser-ui-gates-v1",
    elapsed_ms: 0,
    browser: "",
    viewport: VIEWPORT,
    screens: [],
    assertions: [],
    axe_observed: [],
    axe_incomplete: [],
    violation_count: 0,
    incomplete_count: 0,
    web_root: "",
    baseline: { entries: 0, unexpected: [], stale: [], increased: [] },
    requests: [],
    request_total: 0,
    differences: DIFFERENCES.slice(),
    notes: [],
    fatal: ""
  };
  const observed = [];
  const assertions = [];
  let server = null;
  let browser = null;
  let client = null;
  let mock = null;
  let profileDir = "";
  let profileRemoved = true;
  let baselineRejected = false;
  const check = (screen, name, ok, detail = null) => {
    assertions.push({ screen, name, ok: Boolean(ok), detail });
    return Boolean(ok);
  };
  try {
    const writeBaselineRequested = hasFlag("write-baseline");
    const loadedBaseline = loadBaseline();
    if (!writeBaselineRequested) {
      const todo = loadedBaseline.entries.find((entry) => isTodoReason(entry.reason));
      if (todo) throw new Error(`baseline reason TODO: ${todo.messageKey || todo.target}`);
    }
    if (!existsSync(axePath)) throw new Error("axe-core bundle missing");
    if (!existsSync(staticRoot)) throw new Error("web root missing");
    const axeSource = readFileSync(axePath, "utf8");
    const browserPath = findBrowser();
    summary.browser = path.basename(browserPath);
    summary.web_root = path.relative(repoRoot, staticRoot).replaceAll("\\", "/") || ".";
    if (screenshotDir) summary.screenshot_dir = path.relative(repoRoot, screenshotDir).replaceAll("\\", "/");
    mock = await startServer();
    server = mock.server;
    const appUrl = `http://127.0.0.1:${mock.port}/`;
    profileDir = path.join(os.tmpdir(), `pcv-browser-ui-gates-${runStamp}`);
    mkdirSync(profileDir, { recursive: true });
    profileRemoved = false;
    const launchArgs = [
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
      "--disable-dev-shm-usage",
      "--lang=ko",
      "--remote-debugging-port=0",
      `--user-data-dir=${profileDir}`,
      `--window-size=${VIEWPORT.width},${VIEWPORT.height}`,
      "about:blank"
    ];
    if (process.platform === "linux") launchArgs.splice(launchArgs.indexOf("--disable-dev-shm-usage"), 0, "--no-sandbox");
    browser = spawn(browserPath, launchArgs, { stdio: ["ignore", "ignore", "ignore"], windowsHide: true });
    const activePort = await waitForFile(path.join(profileDir, "DevToolsActivePort"));
    const debugPort = activePort.trim().split(/\r?\n/)[0];
    const target = await openTarget(debugPort);
    client = await connectCdp(target.webSocketDebuggerUrl);
    await client.send("Page.enable");
    await client.send("Runtime.enable");
    await client.send("Emulation.setDeviceMetricsOverride", {
      width: VIEWPORT.width,
      height: VIEWPORT.height,
      deviceScaleFactor: 1,
      mobile: false
    });
    const loaded = client.waitFor("Page.loadEventFired", 20000);
    await client.send("Page.navigate", { url: appUrl });
    await loaded.catch(() => null);
    await withTimeout(evaluate(client, "document.readyState === 'complete'"), 20000, "ready");
    await evaluate(client, HELPERS);
    const axeReady = await evaluate(client, `${axeSource}\n;typeof axe==='function'||typeof axe==='object'`);
    if (!axeReady) throw new Error("axe injection failed");

    const shot = async (id) => {
      if (!screenshotDir) return;
      await capture(client, path.join(screenshotDir, `${id}.png`));
    };
    const scan = async (id) => {
      const counts = {};
      const incompleteCounts = {};
      for (const theme of THEMES) {
        await call(client, "applyTheme", [theme]);
        let payload;
        try {
          payload = await call(client, "axe");
        } catch (error) {
          check(id, `axe-${theme}`, false, String(error.message || error));
          counts[theme] = -1;
          incompleteCounts[theme] = -1;
          continue;
        }
        if (!payload || payload.error) {
          check(id, `axe-${theme}`, false, payload && payload.error);
          counts[theme] = -1;
          incompleteCounts[theme] = -1;
          continue;
        }
        addObserved(observed, id, theme, payload.violations, "violation");
        addObserved(observed, id, theme, payload.incomplete, "incomplete");
        counts[theme] = nodeCount(payload.violations);
        incompleteCounts[theme] = nodeCount(payload.incomplete);
        check(id, `axe-${theme}`, true, counts[theme]);
        if (themeShots && screenshotDir) await capture(client, path.join(screenshotDir, `${id}--${theme}.png`));
      }
      await call(client, "applyTheme", ["supanova"]);
      summary.screens.push({ id, axe_violations: counts, axe_incomplete: incompleteCounts });
    };

    await waitFor(client, "loginReady", [], 15000);
    const loginSecrets = await call(client, "secrets");
    check("auth-login", "secrets", loginSecrets.ok, loginSecrets);
    await scan("auth-login");
    await shot("auth-login");
    mock.releaseAuth();
    let shell = null;
    try {
      shell = await waitFor(client, "shellReady", [], 25000);
    } catch (error) {
      shell = error.detail || null;
      const debug = await call(client, "debug").catch(() => null);
      if (debug && debug.connection === "connected") {
        await call(client, "hideLogin");
        summary.notes.push("auth succeeded and the login overlay was still up, so the harness called pcvSetLoginVisible(false)");
        shell = await waitFor(client, "shellReady", [], 10000);
      } else {
        summary.fatal = "shell did not connect";
        summary.notes.push(JSON.stringify({ shell, debug }));
        throw error;
      }
    }
    check("dashboard", "session-username", shell.username === "pcv-demo", { username: shell.username, userLabel: shell.userLabel });
    if (shell.userLabel && shell.userLabel !== "pcv-demo") {
      summary.notes.push(`top bar label is ${shell.userLabel}`);
    }
    const password = await call(client, "passwordButton");
    check("dashboard", "password-change-visible", password.ok, password);

    const openNav = async (id) => {
      const clicked = await call(client, "click", [`#shell-sidebar .shell-navitem[data-nav="${id}"]`]);
      if (!clicked.ok) {
        check(id, "sidebar-click", false, clicked);
        return false;
      }
      try {
        const view = await waitFor(client, "viewState", [id], 8000);
        check(id, "sidebar-view", view.ok, view);
        return true;
      } catch (error) {
        check(id, "sidebar-view", false, error.detail || error.message);
        return false;
      }
    };

    const ensureMenu = async () => {
      let menu = await call(client, "menuState");
      if (menu.ok) return menu;
      await call(client, "click", ['[data-action="vm-more-toggle"]']);
      return settle(client, "menuState");
    };

    if (await openNav("dashboard")) {
      await call(client, "focus", ['#shell-sidebar .shell-navitem[data-nav="dashboard"]']);
      const arrow = await call(client, "key", ["#shell-sidebar .shell-navwrap", "ArrowDown"]);
      const afterArrow = await call(client, "viewState", ["dashboard"]);
      check("dashboard", "nav-arrow-moves-focus", arrow.activeNav === "vms", arrow);
      check("dashboard", "nav-arrow-does-not-activate", afterArrow.ok, afterArrow);
      await scan("dashboard");
      await shot("dashboard");
    }

    if (await openNav("vms")) {
      const chips = await call(client, "failedChip");
      check("vms", "failed-chip", chips.ok, chips);
      await scan("vms");
      await shot("vms");
      const selected = await call(client, "click", ['#vm-table tr[data-vm-id="pcv-demo-off"] td']);
      check("vm-detail-summary", "select-off-vm", selected.ok, selected);
      if (selected.ok) {
        try {
          await waitFor(client, "detailReady", [], 15000);
          check("vm-detail-summary", "detail-open", true);
        } catch (error) {
          check("vm-detail-summary", "detail-open", false, error.detail || error.message);
        }
        const tabNames = ["summary", "console", "checkpoints", "hardware", "guest", "jobs"];
        const opened = summary.assertions.some((item) => item.screen === "vm-detail-summary" && item.name === "detail-open" && item.ok)
          || assertions.some((item) => item.name === "detail-open" && item.ok);
        if (opened) {
          await call(client, "clickTab", [0]);
          await settle(client, "tabState");
          let tabs = await call(client, "tabState");
          check("vm-detail-summary", "tab-click", tabs.ok && tabs.selectedIndex === 0, tabs);
          await scan("vm-detail-summary");
          await shot("vm-detail-summary");
          await call(client, "keyTab", ["ArrowRight"]);
          tabs = await call(client, "tabState");
          check("vm-detail-console", "tab-arrow-right", tabs.ok && tabs.selectedIndex === 1, tabs);
          await scan("vm-detail-console");
          await shot("vm-detail-console");
          await call(client, "keyTab", ["ArrowLeft"]);
          tabs = await call(client, "tabState");
          check("vm-detail-summary", "tab-arrow-left", tabs.ok && tabs.selectedIndex === 0, tabs);
          await call(client, "keyTab", ["ArrowDown"]);
          tabs = await call(client, "tabState");
          check("vm-detail-console", "tab-arrow-down", tabs.ok && tabs.selectedIndex === 1, tabs);
          await call(client, "keyTab", ["ArrowUp"]);
          tabs = await call(client, "tabState");
          check("vm-detail-summary", "tab-arrow-up", tabs.ok && tabs.selectedIndex === 0, tabs);
          await call(client, "keyTab", ["End"]);
          tabs = await call(client, "tabState");
          check("vm-detail-jobs", "tab-end", tabs.ok && tabs.selectedIndex === 5, tabs);
          await call(client, "keyTab", ["Home"]);
          tabs = await call(client, "tabState");
          check("vm-detail-summary", "tab-home", tabs.ok && tabs.selectedIndex === 0, tabs);
          for (let index = 2; index < tabNames.length; index += 1) {
            await call(client, "clickTab", [index]);
            tabs = await settle(client, "tabState");
            check(`vm-detail-${tabNames[index]}`, "tab-click", tabs.ok && tabs.selectedIndex === index, tabs);
            await scan(`vm-detail-${tabNames[index]}`);
            await shot(`vm-detail-${tabNames[index]}`);
          }
          await call(client, "clickTab", [0]);
          const menuClick = await call(client, "click", ['[data-action="vm-more-toggle"]']);
          check("vm-more-menu", "open", menuClick.ok, menuClick);
          const menu = await waitFor(client, "menuState").catch((error) => error.detail || { ok: false });
          check("vm-more-menu", "expanded", menu.ok && menu.focusInside, menu);
          await call(client, "key", ['#pcv-vm-more-menu [role="menuitem"]', "ArrowDown"]);
          const afterDown = await call(client, "menuState");
          check("vm-more-menu", "arrow-down", afterDown.focusInside, afterDown);
          await call(client, "key", ["#pcv-vm-more-menu", "End"]);
          await call(client, "key", ["#pcv-vm-more-menu", "Home"]);
          const afterHome = await call(client, "menuState");
          check("vm-more-menu", "home-end", afterHome.focusInside, afterHome);
          await scan("vm-more-menu");
          await shot("vm-more-menu");
          const beforeEscape = mock.requests.length;
          await call(client, "key", ["#pcv-vm-more-menu", "Escape"]);
          await sleep(100);
          const closedMenu = await call(client, "menuState");
          check("vm-more-menu", "escape", closedMenu.hidden === true && closedMenu.expanded === "false", closedMenu);
          check("vm-more-menu", "escape-no-mutation", mutationsSince(mock.requests, beforeEscape).length === 0, mutationsSince(mock.requests, beforeEscape));

          await ensureMenu();
          const cloneMarker = await call(client, "activeMarker");
          const beforeClone = mock.requests.length;
          await call(client, "click", ['[data-action="vm-clone-open"]']);
          const clone = await waitFor(client, "cloneOpen").catch((error) => error.detail || { ok: false });
          check("clone-modal", "focus-inside", clone.ok, clone);
          const cloneTab = await call(client, "tabTrap", [".pcv-clone-modal"]);
          summary.notes.push(`clone tab trap stayed=${cloneTab.stayed}`);
          check("clone-modal", "no-mutation-while-open", mutationsSince(mock.requests, beforeClone).length === 0, mutationsSince(mock.requests, beforeClone));
          await scan("clone-modal");
          await shot("clone-modal");
          await call(client, "focus", [".pcv-clone-modal"]);
          await pressEscape(client);
          await sleep(150);
          let cloneClosed = await call(client, "cloneClosed");
          if (!cloneClosed.ok) {
            await evaluate(client, "window.PCV&&PCV.modalCore&&PCV.modalCore.close(false)");
            cloneClosed = await call(client, "cloneClosed");
            check("clone-modal", "escape", false, cloneClosed);
          } else {
            check("clone-modal", "escape", true, { opener: cloneMarker, active: await call(client, "activeMarker") });
          }

          const deleteMenu = await ensureMenu();
          check("delete-confirm", "menu-open", deleteMenu.ok, deleteMenu);
          const beforeDelete = mock.requests.length;
          await call(client, "click", ['[data-action="vm-delete"]']);
          const deleteDialog = await waitFor(client, "confirmState").catch((error) => error.detail || { ok: false });
          check("delete-confirm", "focus-inside", deleteDialog.ok && deleteDialog.focusInside, deleteDialog);
          check("delete-confirm", "typed-name-disables-accept", deleteDialog.hasTyped && deleteDialog.acceptDisabled, deleteDialog);
          const deleteTab = await call(client, "tabTrap", [".pcv-confirm"]);
          check("delete-confirm", "tab-trap", deleteTab.ok, deleteTab);
          await call(client, "setInput", [".pcv-confirm-typed", "not-the-vm"]);
          const stillDisabled = await call(client, "confirmState");
          check("delete-confirm", "wrong-name-stays-disabled", stillDisabled.acceptDisabled === true, stillDisabled);
          await scan("delete-confirm");
          await shot("delete-confirm");
          await call(client, "key", [".pcv-confirm", "Escape"]);
          await sleep(200);
          const deleteClosed = await call(client, "confirmState");
          check("delete-confirm", "escape", deleteClosed.open === false, deleteClosed);
          check("delete-confirm", "escape-no-mutation", mutationsSince(mock.requests, beforeDelete).length === 0, mutationsSince(mock.requests, beforeDelete));
          summary.notes.push(`delete focus after escape: ${await call(client, "activeMarker")}`);

          await call(client, "clickTab", [2]);
          await settle(client, "tabState");
          await settle(client, "restoreReady", [], 8000);
          const beforeRestore = mock.requests.length;
          await call(client, "click", ['[data-action="checkpoint-restore"]']);
          const restoreDialog = await waitFor(client, "confirmState").catch((error) => error.detail || { ok: false });
          check("checkpoint-restore-confirm", "focus-inside", restoreDialog.ok && restoreDialog.focusInside, restoreDialog);
          check("checkpoint-restore-confirm", "no-typed-name", restoreDialog.hasTyped === false && restoreDialog.acceptDisabled === false, restoreDialog);
          const restoreTab = await call(client, "tabTrap", [".pcv-confirm"]);
          check("checkpoint-restore-confirm", "tab-trap", restoreTab.ok, restoreTab);
          await scan("checkpoint-restore-confirm");
          await shot("checkpoint-restore-confirm");
          await call(client, "click", ['[data-pcv-confirm="cancel"]']);
          await sleep(200);
          const restoreClosed = await call(client, "confirmState");
          check("checkpoint-restore-confirm", "cancel", restoreClosed.open === false, restoreClosed);
          check("checkpoint-restore-confirm", "cancel-no-mutation", mutationsSince(mock.requests, beforeRestore).length === 0, mutationsSince(mock.requests, beforeRestore));
        }
      }
    }

    const beforeWizard = mock.requests.length;
    await call(client, "click", ["#vm-list-create"]);
    const step1 = await waitFor(client, "stepState", [1]).catch((error) => error.detail || { ok: false });
    check("wizard-1", "open", step1.ok, step1);
    if (step1.ok) {
      await call(client, "setInput", ['#create-vm-form input[name="name"]', "pcv-demo-new"]);
      const next1 = await waitFor(client, "nextEnabled", [], 5000).catch((error) => error.detail || { ok: false });
      check("wizard-1", "next-enabled", next1.ok, next1);
      await scan("wizard-1");
      await shot("wizard-1");
      await call(client, "focus", ["#create-vm-step-tab-1"]);
      await call(client, "key", ["#create-vm-steps", "ArrowRight"]);
      const step2 = await waitFor(client, "stepState", [2]).catch((error) => error.detail || { ok: false });
      check("wizard-2", "arrow-right", step2.ok, step2);
      if (!step2.ok && next1.ok) {
        await call(client, "click", ["#create-vm-next"]);
      }
      const step2Ready = step2.ok ? step2 : await call(client, "stepState", [2]);
      check("wizard-2", "visible", step2Ready.ok, step2Ready);
      await scan("wizard-2");
      await shot("wizard-2");
      await call(client, "click", ["#create-vm-next"]);
      const step3 = await waitFor(client, "stepState", [3]).catch((error) => error.detail || { ok: false });
      check("wizard-3", "next", step3.ok, step3);
      await call(client, "setInput", ['#create-vm-form input[name="iso_path"]', "D:\\iso\\installer.iso"]);
      await scan("wizard-3");
      await shot("wizard-3");
      await call(client, "click", ["#create-vm-next"]);
      const step4 = await waitFor(client, "stepState", [4]).catch((error) => error.detail || { ok: false });
      check("wizard-4", "next", step4.ok, step4);
      await scan("wizard-4");
      await shot("wizard-4");
      await call(client, "focus", ["#create-vm-dialog"]);
      await pressEscape(client);
      await sleep(200);
      let discard = await call(client, "confirmState");
      if (!discard.open) {
        await call(client, "click", ["#close-create-vm"]);
        await sleep(200);
        discard = await call(client, "confirmState");
        summary.notes.push("wizard Escape did not open the discard confirm; the close button did");
      }
      check("wizard-4", "discard-confirm", discard.open === true, discard);
      if (discard.open) await call(client, "click", ['[data-pcv-confirm="accept"]']);
      const wizardClosed = await waitFor(client, "wizardClosed", [], 5000).catch((error) => error.detail || { ok: false });
      check("wizard-4", "closed", wizardClosed.ok, wizardClosed);
      const wizardMutations = mutationsSince(mock.requests, beforeWizard).filter((entry) => entry.method === "POST" && entry.path === "/api/v1/vms");
      check("wizard-4", "no-create-post", wizardMutations.length === 0, wizardMutations);
    }

    if (await openNav("templates")) {
      await scan("templates");
      await shot("templates");
    }
    if (await openNav("accounts")) {
      const resetButton = await call(client, "passwordButton");
      check("accounts", "password-change-visible", resetButton.ok, resetButton);
      await scan("accounts");
      await shot("accounts");
      const beforeReset = mock.requests.length;
      const resetClick = await call(client, "click", ['[data-action="account-reset-password"][data-username="pcv-demo-operator"]']);
      check("password-reset", "open-click", resetClick.ok, resetClick);
      const reset = await waitFor(client, "resetOpen").catch((error) => error.detail || { ok: false });
      check("password-reset", "focus-inside", reset.ok, reset);
      const resetTab = await call(client, "tabTrap", ["#account-reset-form"]);
      summary.notes.push(`password reset tab trap stayed=${resetTab.stayed}`);
      await scan("password-reset");
      await shot("password-reset");
      await call(client, "focus", ["#account-reset-form"]);
      await pressEscape(client);
      await sleep(150);
      let resetClosed = await call(client, "resetClosed");
      if (!resetClosed.ok) {
        await evaluate(client, "window.PCV&&PCV.modalCore&&PCV.modalCore.close(false)");
        resetClosed = await call(client, "resetClosed");
        check("password-reset", "escape", false, resetClosed);
      } else {
        check("password-reset", "escape", true);
      }
      const resetMutations = mutationsSince(mock.requests, beforeReset).filter((entry) => entry.path.includes("/password"));
      check("password-reset", "no-password-post", resetMutations.length === 0, resetMutations);
    }
    for (const view of ["network", "jobs", "activity", "evidence", "troubleshooting", "helppage"]) {
      if (await openNav(view)) {
        await scan(view);
        await shot(view);
      }
    }

    if (assertions.some((item) => item.name === "detail-open" && item.ok)) {
      await openNav("vms");
      await call(client, "click", ['#vm-table tr[data-vm-id="pcv-demo-off"] td']);
      await settle(client, "detailReady", [], 15000);
      await ensureMenu();
      const beforeAcceptDelete = mock.requests.length;
      await call(client, "click", ['[data-action="vm-delete"]']);
      await settle(client, "confirmState");
      await call(client, "setInput", [".pcv-confirm-typed", "pcv-demo-off"]);
      const armed = await call(client, "confirmState");
      check("delete-confirm", "typed-name-enables-accept", armed.acceptDisabled === false, armed);
      await call(client, "click", ['[data-pcv-confirm="accept"]']);
      await sleep(400);
      const deletes = mock.requests.slice(beforeAcceptDelete).filter((entry) => entry.method === "DELETE" && entry.path === "/api/v1/vms/pcv-demo-off");
      check("delete-confirm", "accept-deletes", deletes.length === 1, deletes);

      await call(client, "click", ['#vm-table tr[data-vm-id="pcv-demo-off"] td']);
      await settle(client, "detailReady", [], 15000);
      await call(client, "clickTab", [2]);
      await settle(client, "tabState");
      const restoreReady = await settle(client, "restoreReady", [], 8000);
      const beforeAcceptRestore = mock.requests.length;
      const restoreClick = await call(client, "click", ['[data-action="checkpoint-restore"]']);
      const restoreConfirm = await settle(client, "confirmState");
      const acceptClick = await call(client, "click", ['[data-pcv-confirm="accept"]']);
      await sleep(800);
      const restores = mock.requests.slice(beforeAcceptRestore).filter((entry) => entry.method === "POST" && entry.path === "/api/v1/vms/pcv-demo-off/checkpoints/cp-demo-1/restore");
      check("checkpoint-restore-confirm", "accept-restores", restores.length === 1, restores.length === 1 ? restores : { restores, restoreReady, restoreClick, restoreConfirm, acceptClick });
    }

    const finalSecrets = await call(client, "secrets");
    check("dashboard", "secrets-final", finalSecrets.ok, finalSecrets);
    const violationItems = observed.filter((item) => item.result === "violation");
    const incompleteItems = observed.filter((item) => item.result === "incomplete");
    const runFailed = assertions.some((item) => !item.ok);
    let problems = [];
    summary.violation_count = violationItems.reduce((sum, item) => sum + item.count, 0);
    summary.incomplete_count = incompleteItems.reduce((sum, item) => sum + item.count, 0);
    summary.baseline.entries = loadedBaseline.entries.length;
    if (writeBaselineRequested) {
      if (summary.violation_count > 0 || runFailed) {
        baselineRejected = true;
        console.error(summary.violation_count > 0 ? "violations remain; baseline not written" : "assertions failed; baseline not written");
      } else {
        const groups = buildBaselineGroups(incompleteItems, loadedBaseline.entries);
        const after = writeBaseline(groups);
        console.log(`기준선 ${loadedBaseline.entries.length}→${after}`);
        summary.baseline.entries = after;
      }
    } else {
      problems = compareBaseline(observed, loadedBaseline.entries);
    }
    summary.baseline.unexpected = problems.filter((item) => item.kind === "unexpected").map(redactFinding);
    summary.baseline.increased = problems.filter((item) => item.kind === "increased").map(redactFinding);
    summary.baseline.stale = problems.filter((item) => item.kind === "stale").map(redactFinding);
    summary.axe_observed = violationItems.map(redactFinding);
    summary.axe_incomplete = incompleteItems.map(redactFinding);
    if (problems.length) {
      for (const problem of problems.slice(0, 40)) {
        const shrink = problem.kind === "stale" ? " 기준선을 줄여라" : "";
        console.error(`axe ${problem.kind} ${problem.result || "violation"} ${problem.screen} ${problem.theme} ${problem.rule} ${problem.target} count=${problem.count}${shrink}`);
      }
      console.error(BASELINE_UPDATE_HINT);
    }
  } catch (error) {
    summary.fatal = String(error.message || error);
    if (error.detail) summary.notes.push(JSON.stringify(error.detail));
    console.error(summary.fatal);
  } finally {
    summary.assertions = assertions;
    const received = mock && Array.isArray(mock.requests) ? mock.requests : [];
    summary.requests = received.slice(0, 400).map((entry) => ({ method: entry.method, path: entry.path }));
    summary.request_total = received.length;
    summary.elapsed_ms = Date.now() - started;
    const failed = assertions.filter((item) => !item.ok);
    if (client) client.close();
    await killBrowser(browser, profileDir);
    if (server) await new Promise((resolve) => server.close(resolve));
    if (profileDir) profileRemoved = await removeDir(profileDir);
    const baselineFailed = summary.baseline.unexpected.length + summary.baseline.increased.length + summary.baseline.stale.length;
    summary.ok = !summary.fatal && failed.length === 0 && baselineFailed === 0 && !baselineRejected && profileRemoved !== false;
    if (!profileRemoved) summary.fatal = summary.fatal || "browser profile was not removed";
    writeFileSync(path.join(artifactDir, "summary.json"), `${JSON.stringify(summary, null, 2)}\n`);
    const line = `browser-ui-gates ok=${summary.ok} elapsed_ms=${summary.elapsed_ms} assertions=${assertions.length} failed=${failed.length} violations=${summary.violation_count} incomplete=${summary.incomplete_count} baseline=${summary.baseline.entries}`;
    console.log(line);
    if (!summary.ok) process.exitCode = 1;
  }
}

function redactFinding(finding) {
  const html = String(finding.html || "").includes(DEMO_TOKEN) ? "[redacted]" : finding.html || "";
  return {
    screen: finding.screen,
    theme: finding.theme,
    result: finding.result || "violation",
    rule: finding.rule,
    target: finding.target,
    count: finding.count,
    impact: finding.impact || "",
    html,
    contrast: finding.contrast || null
  };
}

main().catch((error) => {
  console.error(String(error && error.message || error));
  process.exitCode = 1;
});

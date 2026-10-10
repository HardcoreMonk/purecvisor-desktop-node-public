import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018 (Single Edge frontend structure): web/index.html is the Single Edge index.html structure (login page,
// app shell, icon defs, theme bootstrap, splash) carrying the Desktop Node content sections; it is served at /
// since campaign single-edge-frontend-structure-20261010 Task 16a. The legacy console stays at web/index.legacy.html
// (its own contracts) until a later campaign retires it. These checks keep the shell's shape.

const INDEX = fileURLToPath(new URL("../index.html", import.meta.url));
const html = fs.readFileSync(INDEX, "utf8");

test("head declares the Single Edge security and asset wiring for the Desktop Node", () => {
  assert.match(html, /<meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline';/);
  assert.match(html, /connect-src 'self' http:\/\/127\.0\.0\.1:\* http:\/\/localhost:\* http:\/\/\*:7777/);
  assert.match(html, /frame-src 'none'; object-src 'none'; base-uri 'self'; form-action 'self'/);
  assert.match(html, /<base href="\/">/);
  assert.match(html, /<link rel="stylesheet" href="vendor\/pretendard\/pretendard\.css\?v=/);
  assert.match(html, /<link rel="stylesheet" href="style\.css\?v=/);
  assert.match(html, /<link rel="manifest" href="manifest\.json">/);
  assert.match(html, /localStorage\.getItem\('pcv-theme'\)/, "theme bootstrap");
  assert.match(html, /var allowed = \['supanova', 'supanova-cyan', 'supanova-hicontrast', 'supanova-mockup'\]/);
  assert.match(html, /<title>PureCVisor Desktop Node/);
});

test("login page and app shell keep the Single Edge ids the ported modules bind to", () => {
  for (const id of ["splash", "splash-bar", "toasts", "ctx", "login-page", "login-user", "login-pass", "login-err", "lh-state", "lh-node", "lh-version", "lh-uptime", "lh-hyperv", "lh-disk", "app", "shell-sidebar", "shell-topbar-static", "shell-topbar", "version-badge", "ws-s", "notif-toolbar-badge", "la", "us", "us-name", "mobile-menu-btn", "shell-statusbar", "cb", "mobile-overlay", "ev-side", "evp", "ev-count", "ev-status"]) {
    assert.match(html, new RegExp(`id="${id}"`), `missing id ${id}`);
  }
  assert.match(html, /<div class="app" id="app" aria-hidden="true" inert>/);
  assert.match(html, /<body class="login-active">/);
  assert.match(html, /onsubmit="event\.preventDefault\(\);doLoginPage\(\);return false;"/);
  assert.doesNotMatch(html, /login-totp/, "TOTP is a Linux Single Edge feature and stays excluded");
  assert.equal((html.match(/<symbol id="pcv-i-/g) || []).length, 9, "icon defs");
});

test("content area carries every Desktop Node view and dialog", () => {
  for (const view of ["dashboard", "vms", "network", "jobs", "activity", "evidence", "troubleshooting"]) {
    assert.match(html, new RegExp(`<section id="${view}" class="section app-view" data-view="${view}">`), `view ${view}`);
  }
  for (const id of ["alert-region", "vm-detail-panel", "vm-table", "jobs-panel", "event-center-panel", "activity-panel", "evidence-panel", "account-login-form", "account-username", "account-password", "account-console-panel", "token-rotation-panel", "diagnostics-panel", "command-palette", "command-palette-input", "create-vm-dialog", "create-vm-form"]) {
    assert.match(html, new RegExp(`id="${id}"`), `missing id ${id}`);
  }
  const cbStart = html.indexOf('<div class="cb" id="cb"');
  const dashboard = html.indexOf('<section id="dashboard"');
  assert.ok(cbStart > 0 && dashboard > cbStart, "views sit inside the shell content box");
});

test("scripts load in the Single Edge order with the Desktop Node config first and nothing Linux-specific", () => {
  const order = ["/pcv-config.js", "vendor/chart.umd.min.js", "i18n.js", "app.bundle.js"].map((src) => html.indexOf(`<script src="${src}`));
  assert.ok(order.every((index) => index > 0), "every script tag present");
  assert.deepEqual([...order].sort((a, b) => a - b), order, "script order");
  assert.doesNotMatch(html, /qrcode\.js|novnc/, "qrcode and noVNC are not vendored");
  assert.doesNotMatch(html, /\bLXC\b|\bZFS\b|\bOVN\b|\bKVM\b|libvirt|purecvisorsd|journalctl/, "no Linux runtime surface");
  assert.doesNotMatch(html, /<script src="\/app\.js"/, "legacy bundle is not loaded by the new index");
  assert.match(html, /window\.__pcvDismissSplash = _pcvSplashRemove;/, "splash dismissal hook");
});

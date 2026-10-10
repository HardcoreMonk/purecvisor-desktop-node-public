import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018 (Single Edge frontend structure): every file under web/src/modules is a window.PCV IIFE module ported from
// the Single Edge ui/modules tree or written in that shape, listed in src/modules.json, and free of the Linux Single
// Edge surfaces (containers, ZFS storage, OVN, VPC, TOTP, self-healing, web push).

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const modulesDir = path.join(webRoot, "src", "modules");
const order = JSON.parse(fs.readFileSync(path.join(webRoot, "src", "modules.json"), "utf8"));
const moduleFiles = fs.readdirSync(modulesDir).filter((file) => file.endsWith(".ts")).sort();
const LINUX_IDS = /navigateTo\('(containers|storage|ovn|vpcs|selfhealing|mon-security|iscsi|dpdk|sriov|gpu|overlay|backup|topology)'\)/;

test("every module is listed in the order table and the order table lists only existing modules", () => {
  assert.deepEqual([...order.modules].sort(), moduleFiles.map((file) => file.slice(0, -3)).sort());
  assert.equal(order.contract, "pcv-web-module-order-v1");
  assert.equal(order.bootstrap, "src/bootstrap.ts");
});

test("modules follow the Single Edge window.PCV IIFE shape with an origin or Desktop Node header", () => {
  for (const file of moduleFiles) {
    const text = fs.readFileSync(path.join(modulesDir, file), "utf8");
    assert.match(text, /^\/\/ @ts-nocheck\n/, `${file}: ts-nocheck header`);
    assert.match(text, /^\/\/ (Ported from purecvisor ui\/modules\/[a-z0-9-]+\.js \(Apache-2\.0, same author\)|Desktop Node module)/m, `${file}: origin header`);
    assert.match(text, /window\.PCV = window\.PCV \|\| \{\};/, `${file}: PCV namespace`);
    assert.match(text, /\(function ?\(PCV\) \{[\s\S]*\}\)\(window\.PCV\);\s*$/, `${file}: IIFE closed over window.PCV`);
    assert.doesNotMatch(text, /^\s*(export|import)\s/m, `${file}: no ES module syntax`);
    assert.doesNotMatch(text, LINUX_IDS, `${file}: Linux Single Edge navigation target`);
    assert.doesNotMatch(text, /\/api\/v1\/(containers|storage\/zfs|ovn|vpcs|totp)/, `${file}: Linux Single Edge route`);
  }
});

test("the shell nav model and command palette expose the Desktop Node views only", () => {
  const shell = fs.readFileSync(path.join(modulesDir, "shell.ts"), "utf8");
  for (const view of ["dashboard", "vms", "network", "jobs", "activity", "evidence", "troubleshooting", "helppage"]) {
    assert.match(shell, new RegExp(`id: '${view}'`), `shell nav lists ${view}`);
  }
  assert.match(shell, /'DESKTOP NODE'/);
  assert.doesNotMatch(shell, /'containers'|'storage'|'ovn'|'vpcs'|'selfhealing'/);
  const nav = fs.readFileSync(path.join(modulesDir, "nav.ts"), "utf8");
  assert.match(nav, /window\.navigateTo = navigateTo;/);
  assert.match(nav, /\.app-view/, "nav toggles the static view sections instead of rebuilding #cb");
  assert.doesNotMatch(nav, /wsConnection/, "no WebSocket status");
  assert.match(nav, /EP\.RUNTIME_POLICY/, "version badge reads the runtime policy");
});

test("the generated bundle carries the module namespaces in order", () => {
  const bundle = fs.readFileSync(path.join(webRoot, "app.bundle.js"), "utf8");
  let last = -1;
  for (const name of order.modules) {
    const index = bundle.indexOf(`// --- src/modules/${name}.ts ---`);
    assert.ok(index > last, `bundle marker order for ${name}`);
    last = index;
  }
  for (const ns of ["PCV.ui = {", "PCV.uxlib = {", "PCV.ui.filterState = {", "PCV.metrics = {", "PCV.charts = {", "PCV.theme = {", "PCV.modalCore = modalCore;", "PCV.modal = Modal;", "PCV.nav = {", "PCV.shell = {", "PCV.mobile = {"]) {
    assert.ok(bundle.includes(ns), `bundle defines ${ns}`);
  }
});

import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018 (Single Edge frontend structure): the PWA trio web/manifest.json, web/sw.js and web/offline.html follows the
// Single Edge ui/ files at the Desktop Node web root. These checks keep the root scope, the precache list in step with
// the shipped payload, the cache-name bump wired to the build, and the Web Push surface out.

const webFile = (name) => fileURLToPath(new URL("../" + name, import.meta.url));
const read = (name) => fs.readFileSync(webFile(name), "utf8");
const manifest = JSON.parse(read("manifest.json"));
const sw = read("sw.js");
const offline = read("offline.html");
const bootstrap = read("src/bootstrap.ts");
const bundle = read("app.bundle.js");
const payload = JSON.parse(read("payload-manifest.json"));
const shipped = new Set([...payload.core, ...payload.files]);

test("manifest.json describes the Desktop Node console at the web root", () => {
  assert.equal(manifest.name, "PureCVisor Desktop Node");
  assert.equal(manifest.start_url, "/");
  assert.equal(manifest.scope, "/");
  assert.equal(manifest.display, "standalone");
  const sizes = new Set(manifest.icons.map((icon) => icon.sizes));
  assert.deepEqual([...sizes].sort(), ["192x192", "512x512"]);
  for (const icon of manifest.icons) {
    assert.match(icon.src, /^\/icon-(192|512)\.png$/, icon.src);
    assert.ok(shipped.has(icon.src.slice(1)), `${icon.src} shipped`);
  }
  const shortcutUrls = manifest.shortcuts.map((shortcut) => shortcut.url);
  assert.deepEqual(shortcutUrls, ["/#vms", "/#jobs", "/#troubleshooting", "/docs.html"]);
  assert.doesNotMatch(JSON.stringify(manifest), /\/ui\/|LXC|container|KVM/i);
});

test("sw.js precaches shipped root-scope files only and leaves the runtime config and the API alone", () => {
  assert.match(sw, /^const CACHE_NAME = 'pcv-ui-v[0-9a-f]{8}';$/m, "cache name stamped by the build");
  const listSource = sw.slice(sw.indexOf("const STATIC_ASSETS = ["), sw.indexOf("];", sw.indexOf("const STATIC_ASSETS = [")));
  const assets = [...listSource.matchAll(/'(\/[^']*)'/g)].map((m) => m[1]);
  assert.ok(assets.length >= 12, `precache entries: ${assets.length}`);
  for (const asset of assets) {
    if (asset === "/") continue;
    const file = asset.slice(1);
    assert.ok(fs.existsSync(webFile(file)), `${asset} exists`);
    assert.ok(shipped.has(file), `${asset} is in payload-manifest.json`);
  }
  for (const required of ["/", "/index.html", "/offline.html", "/style.css", "/app.bundle.js", "/i18n.js", "/manifest.json", "/docs.html"]) {
    assert.ok(assets.includes(required), `precache ${required}`);
  }
  assert.match(sw, /const NETWORK_ONLY = \['\/pcv-config\.js'\];/);
  assert.match(sw, /url\.pathname\.startsWith\('\/api\/'\)/);
  assert.match(sw, /url\.origin !== self\.location\.origin\) return;/);
  assert.match(sw, /type === 'SKIP_WAITING'/);
  assert.match(sw, /type === 'CLEAR_CACHE'/);
  assert.match(sw, /const OFFLINE_URL = '\/offline\.html';/);
  assert.doesNotMatch(sw, /addEventListener\('(push|pushsubscriptionchange|notificationclick)'/, "Web Push handlers stay out");
  assert.doesNotMatch(sw, /pushManager|showNotification|PENDING_CACHE/);
  const swCode = sw.split("\n").filter((line) => !line.trim().startsWith("//")).join("\n");
  assert.doesNotMatch(swCode, /\/ui\//, "Single Edge /ui/ prefix in code");
});

test("offline.html is self-contained and probes the console origin", () => {
  assert.match(offline, /<title>PureCVisor Desktop Node — Offline<\/title>/);
  assert.match(offline, /fetch\('\/pcv-config\.js', \{ cache: 'no-store' \}\)/);
  assert.match(offline, /location\.reload\(\)/);
  assert.doesNotMatch(offline, /https?:\/\//, "no external resources");
  assert.doesNotMatch(offline, /\/ui\//);
});

test("bootstrap registers the root-scope service worker the Single Edge way", () => {
  assert.match(bootstrap, /navigator\.serviceWorker\.register\('sw\.js', \{ updateViaCache: 'none' \}\)/);
  assert.match(bootstrap, /postMessage\(\{ type: 'SKIP_WAITING' \}\)/);
  assert.match(bootstrap, /addEventListener\('controllerchange'/);
  assert.match(bundle, /navigator\.serviceWorker\.register\('sw\.js'/, "registration is in the shipped bundle");
});

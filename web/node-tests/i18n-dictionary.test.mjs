import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018: web/i18n.js is the Single Edge I18N structure (ko/en dictionaries, t(), applyI18n(), toggle) served before
// app.bundle.js, plus the Desktop Node keys and the _L(ko, en) helper. The two dictionaries must stay symmetric.

const I18N_PATH = fileURLToPath(new URL("../i18n.js", import.meta.url));
const text = fs.readFileSync(I18N_PATH, "utf8");

function keysOf(block) {
  return [...block.matchAll(/^\s*'([^']+)':\s/gm)].map((match) => match[1]);
}

const koStart = text.indexOf("    ko: {");
const enStart = text.indexOf("    en: {");
const dataEnd = text.indexOf("  t(key, params) {");
assert.ok(koStart > 0 && enStart > koStart && dataEnd > enStart, "dictionary layout");
const koKeys = keysOf(text.slice(koStart, enStart));
const enKeys = keysOf(text.slice(enStart, dataEnd));

test("ko and en dictionaries carry the same keys", () => {
  assert.ok(koKeys.length > 200, `ko keys ${koKeys.length}`);
  assert.deepEqual([...new Set(koKeys)].sort(), [...new Set(enKeys)].sort());
  assert.equal(new Set(koKeys).size, koKeys.length, "duplicate ko keys");
});

test("Desktop Node additions are present in both languages and the helpers are exported", () => {
  const additions = ["product.name", "product.edition", "nav.vms", "nav.network", "nav.jobs", "nav.activity", "nav.evidence", "nav.troubleshooting", "nav.help", "login.loopback_note", "status.polling", "msg.update_procedure"];
  const koBlock = text.slice(text.indexOf("Object.assign(I18N._data.ko, {"), text.indexOf("Object.assign(I18N._data.en, {"));
  const enBlock = text.slice(text.indexOf("Object.assign(I18N._data.en, {"), text.indexOf("var _L = window._L"));
  for (const key of additions) {
    assert.match(koBlock, new RegExp(`'${key.replace(".", "\\.")}':`), `ko ${key}`);
    assert.match(enBlock, new RegExp(`'${key.replace(".", "\\.")}':`), `en ${key}`);
  }
  assert.match(text, /var _L = window\._L = function \(ko, en\) \{ return I18N\.getLang\(\) === 'en' \? en : ko; \};/);
  for (const exported of ["PCV.i18n = I18N;", "window.I18N = I18N;", "window.t = t;", "window.applyI18n = applyI18n;"]) {
    assert.ok(text.includes(exported), exported);
  }
  assert.match(text, /^\/\/ Ported from purecvisor ui\/i18n\.js \(Apache-2\.0, same author\)/);
});

test("keys used by the ported modules exist in the dictionary", () => {
  const modulesDir = fileURLToPath(new URL("../src/modules/", import.meta.url));
  const used = new Set();
  for (const file of fs.readdirSync(modulesDir)) {
    const source = fs.readFileSync(modulesDir + file, "utf8");
    for (const match of source.matchAll(/\b_?t\('([a-z0-9_.]+)'/g)) used.add(match[1]);
  }
  const known = new Set(koKeys);
  const missing = [...used].filter((key) => !known.has(key));
  assert.deepEqual(missing, [], "module keys missing from i18n.js");
});

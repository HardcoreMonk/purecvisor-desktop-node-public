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

// BL-0024 counts a capitalized 2–12 word English literal that is not already the English
// half of L()/_L(). Product names stay English. Screen copy uses a dictionary key.
const BL0024_FILES = ["monitor.ts", "core.ts", "ops.ts", "api.ts", "theme.ts", "nav.ts", "prefs.ts", "shell.ts", "desktop-api.ts", "events.ts"];
const BL0024_ALLOW = new Set([
  "SUPANOVA CYAN",
  "SUPANOVA HI-CONTRAST",
  "SUPANOVA MOCKUP",
  "PureCVisor Desktop Node",
  "DESKTOP NODE",
  "Desktop Node",
  "Local API 주소"
]);

function bl0024StripComments(source) {
  let out = "";
  let i = 0;
  while (i < source.length) {
    const c = source[i];
    const n = source[i + 1];
    if (c === "/" && n === "/") {
      while (i < source.length && source[i] !== "\n") { out += " "; i++; }
      continue;
    }
    if (c === "/" && n === "*") {
      i += 2;
      while (i < source.length && !(source[i] === "*" && source[i + 1] === "/")) {
        out += source[i] === "\n" ? "\n" : " ";
        i++;
      }
      out += "  ";
      i += 2;
      continue;
    }
    if (c === "'" || c === '"' || c === "`") {
      const q = c;
      out += c;
      i++;
      while (i < source.length) {
        out += source[i];
        if (source[i] === "\\") { i++; if (i < source.length) out += source[i]; i++; continue; }
        if (source[i] === q) { i++; break; }
        i++;
      }
      continue;
    }
    out += c;
    i++;
  }
  return out;
}

function bl0024Leftovers(source) {
  const hits = [];
  const re = /(['"`])([A-Z][^'"`\n]{2,240})\1/g;
  let match;
  while ((match = re.exec(source))) {
    const value = match[2].trim();
    if (value.includes("${") || BL0024_ALLOW.has(value)) continue;
    if (/^PCV_[A-Z0-9_]+$/.test(value)) continue;
    const words = value.replace(/[.,:;!?…]+$/g, "").split(/\s+/).filter(Boolean);
    if (words.length < 2 || words.length > 12) continue;
    if (!/^[A-Z][A-Za-z0-9'’/-]*$/.test(words[0])) continue;
    if (value.replace(/[^A-Za-z]/g, "").length < 4) continue;
    const before = source.slice(Math.max(0, match.index - 80), match.index);
    if (/(?:^|[^A-Za-z0-9_])_?L\(\s*(?:'[^']*'|"[^"]*"|`[^`]*`)\s*,\s*$/.test(before)) continue;
    if (/(?:^|[^A-Za-z0-9_])_?t\(\s*$/.test(before)) continue;
    const line = source.slice(0, match.index).split("\n").length;
    hits.push(`${line}: ${value}`);
  }
  return hits;
}

test("BL-0024 shell copy uses i18n keys and the Korean default", () => {
  const webRoot = fileURLToPath(new URL("../", import.meta.url));
  const html = fs.readFileSync(webRoot + "index.html", "utf8");
  for (const [key, korean, englishHeading] of [
    ["copy.dashboard.title", "운영 콕핏", "<h2>Ops Cockpit</h2>"],
    ["copy.dashboard.create", "새 VM", ">Create VM</button>"],
    ["copy.network.title", "네트워크 인벤토리", "<h2>Network Inventory</h2>"],
    ["copy.jobs.title", "작업", "<h2>Jobs</h2>"],
    ["copy.activity.title", "이벤트", "<h2>Operator Activity</h2>"],
    ["copy.evidence.title", "증적", "<h2>Evidence</h2>"],
    ["copy.trouble.title", "진단", "<h2>Diagnostics</h2>"],
    ["copy.monitoring.title", "모니터링", "<h2>Monitoring</h2>"]
  ]) {
    assert.match(html, new RegExp(`data-i18n="${key}"[^>]*>${korean}<`), key);
    assert.equal(html.includes(englishHeading), false, englishHeading);
  }
  assert.match(html, /data-i18n="login"[^>]*>로그인</);
  assert.match(html, /data-i18n="copy\.login\.user"[^>]*>계정 이름</);
  assert.match(html, /data-i18n="copy\.login\.password"[^>]*>비밀번호</);

  const core = fs.readFileSync(webRoot + "src/modules/core.ts", "utf8");
  assert.match(core, /t\('copy\.rbac\.denied'/);
  assert.match(core, /t\('copy\.list\.shown'/);
  assert.match(core, /t\('vm\.trouble\.host_ok'\)/);
  assert.match(core, /t\('vm\.trouble\.host_bad'\)/);
  assert.doesNotMatch(core, /The current account role cannot use/);
  assert.doesNotMatch(core, /shown of/);
  assert.doesNotMatch(core, /'Needs attention'/);
  assert.doesNotMatch(core, /'Ready'/);
  assert.doesNotMatch(core, /The result will appear in Tracked Jobs/);

  const events = fs.readFileSync(webRoot + "src/modules/events.ts", "utf8");
  assert.doesNotMatch(events, /'job cancel'|'job retry'|'job reconciliation'|'running guest execution cancel'/);

  const monitor = fs.readFileSync(webRoot + "src/modules/monitor.ts", "utf8");
  const ops = fs.readFileSync(webRoot + "src/modules/ops.ts", "utf8");
  const viewEvents = fs.readFileSync(webRoot + "src/modules/view-events.ts", "utf8");
  const theme = fs.readFileSync(webRoot + "src/modules/theme.ts", "utf8");
  for (const [source, key] of [
    [monitor, "copy.mon.bundle_kicker"],
    [monitor, "copy.mon.bundle_title"],
    [monitor, "copy.mon.create_bundle"],
    [monitor, "copy.mon.token_kicker"],
    [monitor, "copy.mon.token_title"],
    [monitor, "copy.mon.clear_browser_token"],
    [viewEvents, "copy.events.col.severity"],
    [viewEvents, "copy.events.filter"],
    [ops, "copy.jobs.pagination"],
    [ops, "copy.jobs.load_next"],
    [ops, "copy.job.cancel"],
    [theme, "copy.theme.import"],
    [theme, "copy.theme.cancel"]
  ]) {
    assert.match(source, new RegExp(`t\\('${key.replaceAll(".", "\\.")}'\\)`), key);
  }
  assert.doesNotMatch(monitor, />Support artifact<|>Diagnostic Bundle<|>Create bundle<|>Auth lifecycle<|>Token Rotation<|>Clear browser token</);
  assert.doesNotMatch(ops, />Event Center<|>Severity lane<|>Pagination<|>Load next jobs</);
  assert.doesNotMatch(ops, /: 'Cancel'/);
  assert.doesNotMatch(theme, /'Import'|'Cancel'/);
  const nav = fs.readFileSync(webRoot + "src/modules/nav.ts", "utf8");
  assert.match(nav, /role:\s*'combobox'/);
  assert.match(nav, /_L\('명령·객체·IP 검색', 'Search commands, objects, IPs'\)/);
  assert.doesNotMatch(html, /aria-label="Command palette"/);
  assert.doesNotMatch(html, />Command search</);
  assert.doesNotMatch(html, /placeholder="Search views, assets, jobs, diagnostics"/);

  const koBlock = text.slice(koStart, enStart);
  const enBlock = text.slice(enStart, dataEnd);
  for (const [key, korean, english] of [
    ["copy.mon.bundle_kicker", "지원 산출물", "Support artifact"],
    ["copy.mon.bundle_title", "진단 번들", "Diagnostic Bundle"],
    ["copy.mon.create_bundle", "번들 만들기", "Create bundle"],
    ["copy.mon.token_kicker", "인증 수명 주기", "Auth lifecycle"],
    ["copy.mon.token_title", "토큰 교체", "Token Rotation"],
    ["copy.mon.clear_browser_token", "브라우저 토큰 지우기", "Clear browser token"],
    ["copy.events.center", "이벤트 센터", "Event Center"],
    ["copy.events.severity", "심각도", "Severity lane"],
    ["copy.jobs.pagination", "페이지", "Pagination"],
    ["copy.jobs.load_next", "다음 작업 불러오기", "Load next jobs"],
    ["copy.palette.label", "명령 팔레트", "Command palette"],
    ["copy.palette.search", "명령 검색", "Command search"],
    ["copy.palette.placeholder", "화면, 자산, 작업, 진단 검색", "Search views, assets, jobs, diagnostics"],
    ["copy.theme.import", "가져오기", "Import"],
    ["copy.theme.cancel", "취소", "Cancel"],
    ["copy.job.cancel", "취소", "Cancel"],
    ["copy.login.user", "계정 이름", "Account name"],
    ["copy.login.password", "비밀번호", "Password"]
  ]) {
    assert.match(koBlock, new RegExp(`'${key.replaceAll(".", "\\.")}': '${korean}'`), `ko ${key}`);
    assert.match(enBlock, new RegExp(`'${key.replaceAll(".", "\\.")}': '${english}'`), `en ${key}`);
  }
  for (const key of ["copy.dashboard.title", "copy.rbac.denied", "copy.list.shown", "copy.api.malformed"]) {
    assert.match(koBlock, new RegExp(`'${key}':\\s*'[^']*[가-힣]`), `ko ${key}`);
    assert.match(enBlock, new RegExp(`'${key}':\\s*'`), `en ${key}`);
  }

  const leftovers = [];
  for (const file of BL0024_FILES) {
    const source = bl0024StripComments(fs.readFileSync(webRoot + "src/modules/" + file, "utf8"));
    for (const hit of bl0024Leftovers(source)) leftovers.push(`${file} ${hit}`);
  }
  assert.deepEqual(leftovers, []);
});

import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// ADR-0018 (Single Edge frontend structure): web/docs.html is the Single Edge docs portal (landing, search, reader)
// reading web/guide-content.md, and web/guide.html forwards old guide links to it. These checks keep the portal
// rooted at the Desktop Node web root and every link resolvable the way the portal script slugifies headings.

const webFile = (name) => fileURLToPath(new URL("../" + name, import.meta.url));
const docs = fs.readFileSync(webFile("docs.html"), "utf8");
const guide = fs.readFileSync(webFile("guide.html"), "utf8");
const content = fs.readFileSync(webFile("guide-content.md"), "utf8").replace(/\r\n/g, "\n");

// Mirrors cleanLine()/slugify() of the docs.html portal script.
function cleanLine(text) {
  return String(text || "").replace(/```/g, " ").replace(/[`*_>#|\[\]()]/g, " ").replace(/\s+/g, " ").trim();
}
function slugify(text) {
  return String(text || "").toLowerCase()
    .replace(/[`*\[\]()]/g, "")
    .replace(/[^a-z0-9가-힣\s-]/g, "")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-+|-+$/g, "");
}
function headings(markdown) {
  const counts = Object.create(null);
  const slugs = new Set();
  const chapters = [];
  for (const line of markdown.split("\n")) {
    const match = line.match(/^(#{1,6})\s+(.+)$/);
    if (!match) continue;
    const title = cleanLine(match[2]);
    const base = slugify(title);
    const seen = counts[base] || 0;
    counts[base] = seen + 1;
    slugs.add(seen ? `${base}-${seen + 1}` : base);
    if (match[1].length === 2) chapters.push(title);
  }
  return { slugs, chapters };
}
const { slugs, chapters } = headings(content);

test("docs.html keeps the Single Edge portal shell rooted at the Desktop Node web root", () => {
  assert.match(docs, /<base href="\/">/);
  assert.match(docs, /<title>PureCVisor Desktop Node Docs<\/title>/);
  assert.match(docs, /<link rel="stylesheet" href="vendor\/pretendard\/pretendard\.css\?v=/);
  for (const id of ["primary-nav", "doc-search-form", "doc-search-input", "doc-search-popover", "doc-search-status", "doc-search-results",
    "docs-theme-select", "docs-main", "docs-landing", "docs-landing-hero", "legacy-docs-nav-metadata", "guide-categories",
    "reader-mobile-toc", "reader-index-toggle", "reader-mobile-section", "docs-reader", "reader-index", "reader-chapter-nav",
    "reader-breadcrumb-chapter", "reader-state", "reader-content", "reader-pager", "reader-toc", "reader-overlay"]) {
    assert.match(docs, new RegExp(`id="${id}"`), `missing id ${id}`);
  }
  assert.match(docs, /PCV\.docsPortal = PCV\.docsPortal \|\| \{\};/);
  assert.match(docs, /fetch\('guide-content\.md', \{ cache: 'no-store' \}\)/);
  assert.equal((docs.match(/<div class="nav-group" data-nav-group>/g) || []).length, 4, "four navigation groups");
  assert.equal((docs.match(/class="docs-category"/g) || []).length, 4, "four chapter categories");
  assert.equal((docs.match(/class="chapter-card"/g) || []).length, chapters.length, "one chapter card per guide chapter");
  assert.doesNotMatch(docs, /\/ui\//, "Single Edge /ui/ prefix");
  assert.doesNotMatch(docs, /github\.com|purecvisor\.site/, "external links");
});

test("every portal link resolves to a heading of guide-content.md", () => {
  const hrefs = [...docs.matchAll(/href="docs\.html#([^"]+)"/g)].map((m) => m[1]);
  assert.ok(hrefs.length >= 20, `links: ${hrefs.length}`);
  for (const href of hrefs) assert.ok(slugs.has(href), `docs.html#${href}`);
  const routes = [...docs.matchAll(/'(category-[a-z]+|guide-categories|paths-title)': '([^']+)'/g)];
  assert.equal(routes.length, 6, "route map entries");
  for (const [, from, to] of routes) assert.ok(slugs.has(to), `${from} -> ${to}`);
  for (const id of ["category-start", "category-workloads", "category-operations", "category-interfaces"]) {
    assert.match(docs, new RegExp(`id="${id}"`), id);
  }
});

test("guide-content.md chapters are numbered the way the reader indexes them", () => {
  assert.match(content, /^# PureCVisor Desktop Node 운영 가이드$/m);
  assert.equal(chapters.length, 10);
  chapters.forEach((title, index) => assert.match(title, new RegExp(`^${index + 1}\\. `), title));
  const linuxSurface = /\b(KVM|libvirt|LXC|ZFS|OVS|OVN|systemd|pcvctl|virsh|qemu)\b/;
  for (const [name, text] of [["docs.html", docs], ["guide.html", guide], ["guide-content.md", content]]) {
    assert.doesNotMatch(text, linuxSurface, `${name} carries a Linux Single Edge term`);
    assert.doesNotMatch(text, /Bearer [A-Za-z0-9._-]{20,}/, `${name} carries a token-like value`);
  }
  assert.match(content, /^## 8\. 직접 API 호출$/m);
  assert.match(content, /\| `GET \/api\/v1\/runtime\/policy` \|/);
});

test("guide.html forwards to docs.html next to it with the hash", () => {
  assert.match(guide, /window\.location\.replace\('docs\.html' \+ window\.location\.hash\)/);
  assert.match(guide, /<a id="docs-link" href="docs\.html">/);
  assert.match(guide, /<meta name="robots" content="noindex">/);
  assert.doesNotMatch(guide, /\/ui\//);
});

// Ported from purecvisor scripts/domsafe_ratchet.py (Apache-2.0, same author) as a Node script (ADR-0018).
// Counts DOM HTML-injection sites (innerHTML/outerHTML assignment, insertAdjacentHTML, document.write) in the
// Web Console sources and compares the total with the recorded ceiling in config/domsafe-ratchet.json.
// Unlike the Single Edge visibility ratchet this one is a gate: a total above the ceiling fails, and a total
// below it fails too until the ceiling is tightened with --update (same rule as the packaging module size ratchet).
import { existsSync, readFileSync, readdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const configPath = join(webRoot, "config/domsafe-ratchet.json");
const PATTERN = /\.(innerHTML|outerHTML)\b|\.insertAdjacentHTML\s*\(|\bdocument\.write\s*\(/;
const CONTRACT = "pcv-web-domsafe-ratchet-v1";

function listSources() {
  const files = [];
  const bootstrap = join(webRoot, "src/bootstrap.ts");
  if (existsSync(bootstrap)) files.push("src/bootstrap.ts");
  const modulesDir = join(webRoot, "src/modules");
  if (existsSync(modulesDir)) {
    for (const file of readdirSync(modulesDir).sort()) {
      if (file.endsWith(".ts")) files.push(`src/modules/${file}`);
    }
  }
  // Legacy structure, removed in campaign single-edge-frontend-structure-20261010 Task 16.
  const servedDir = join(webRoot, "src/served");
  if (existsSync(servedDir)) {
    for (const file of readdirSync(servedDir).sort()) {
      if (file.endsWith(".ts")) files.push(`src/served/${file}`);
    }
  }
  if (existsSync(join(webRoot, "src/served-app.ts"))) files.push("src/served-app.ts");
  return files;
}

function scan(relativePath) {
  const hits = [];
  readFileSync(join(webRoot, relativePath), "utf8").split(/\r?\n/).forEach((line, index) => {
    if (PATTERN.test(line)) hits.push({ line: index + 1, code: line.trim().slice(0, 140) });
  });
  return hits;
}

const update = process.argv.includes("--update");
const verbose = process.argv.includes("--verbose");
const perFile = listSources().map((file) => ({ file, hits: scan(file) }));
const total = perFile.reduce((sum, entry) => sum + entry.hits.length, 0);

for (const entry of perFile) {
  if (entry.hits.length === 0) continue;
  console.log(`${entry.file}: ${entry.hits.length}`);
  if (verbose) for (const hit of entry.hits) console.log(`  L${hit.line}: ${hit.code}`);
}
console.log(`domsafe sites total: ${total}`);

if (update) {
  const config = {
    schema_version: 1,
    contract: CONTRACT,
    pattern: PATTERN.source,
    ceiling: total,
    recorded_on: new Date().toISOString().slice(0, 10),
    note: "Lower this only by removing HTML-injection sites; raise it only with a reviewed reason in the commit."
  };
  writeFileSync(configPath, `${JSON.stringify(config, null, 2)}\n`);
  console.log(`domsafe ceiling recorded: ${total}`);
  process.exit(0);
}

if (!existsSync(configPath)) {
  console.error("domsafe ratchet failed: config/domsafe-ratchet.json is missing; run node scripts/domsafe-ratchet.mjs --update");
  process.exit(1);
}
const config = JSON.parse(readFileSync(configPath, "utf8"));
if (config.contract !== CONTRACT || !Number.isInteger(config.ceiling)) {
  console.error(`domsafe ratchet failed: config must declare contract ${CONTRACT} and an integer ceiling`);
  process.exit(1);
}
if (total > config.ceiling) {
  console.error(`domsafe ratchet failed: ${total} sites exceed the ceiling ${config.ceiling}; remove the new injection site or raise the ceiling with a reviewed reason`);
  process.exit(1);
}
if (total < config.ceiling) {
  console.error(`domsafe ratchet failed: ${total} sites are below the ceiling ${config.ceiling}; tighten it with node scripts/domsafe-ratchet.mjs --update`);
  process.exit(1);
}
console.log(`domsafe ratchet ok: ${total} sites at the ceiling`);

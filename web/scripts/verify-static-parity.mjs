import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = dirname(dirname(fileURLToPath(import.meta.url)));

const paths = {
  packageJson: join(webRoot, "package.json"),
  index: join(webRoot, "index.html"),
  appSource: join(webRoot, "src", "app.ts"),
  fixtureSource: join(webRoot, "src", "user-visible-fixtures.ts"),
  servedSource: join(webRoot, "src", "served-app.ts"),
  // scripts/build-served-asset.mjs 의 servedSourceParts 와 같은 집합을 유지해야 한다. 여기서
  // 빠진 part 는 아래 requireIncludes 검사가 보지 못하므로, 그 part 로 옮겨진 심볼이 사라져도
  // parity 검사가 통과한다.
  servedSourceParts: [
    join(webRoot, "src", "served", "types.ts"),
    join(webRoot, "src", "served", "state.ts"),
    join(webRoot, "src", "served", "routes.ts"),
    join(webRoot, "src", "served", "errors.ts"),
    join(webRoot, "src", "served", "api-client.ts"),
    join(webRoot, "src", "served", "evidence.ts"),
    join(webRoot, "src", "served", "summary.ts"),
    join(webRoot, "src", "served", "table.ts"),
    join(webRoot, "src", "served", "rbac.ts"),
    join(webRoot, "src", "served", "render-ops.ts"),
    join(webRoot, "src", "served", "render-inventory.ts"),
    join(webRoot, "src", "served", "render-qos.ts"),
    join(webRoot, "src", "served", "render-vm-detail.ts"),
    join(webRoot, "src", "served", "render-jobs.ts"),
    join(webRoot, "src", "served", "render-monitoring.ts"),
    join(webRoot, "src", "served", "render-panels.ts"),
    join(webRoot, "src", "served", "render-console.ts"),
    join(webRoot, "src", "served", "render-activity.ts"),
    join(webRoot, "src", "served", "render-shell.ts"),
    join(webRoot, "src", "served", "load.ts"),
    join(webRoot, "src", "served", "mutate.ts"),
    join(webRoot, "src", "served", "job-polling.ts"),
    join(webRoot, "src", "served", "actions.ts")
  ],
  generatorSource: join(webRoot, "src", "generate-parity-manifest.ts"),
  servedBuilderScript: join(webRoot, "scripts", "build-served-asset.mjs"),
  regenerateScript: join(webRoot, "scripts", "regenerate-static-parity.mjs"),
  browserFixtureScript: join(webRoot, "scripts", "verify-browser-fixture.mjs"),
  manifest: join(webRoot, "generated", "parity", "static-asset-parity.manifest.json")
};

const expectedRoutes = {
  opsSummary: "/api/v1/ops/summary",
  runtimePolicy: "/api/v1/runtime/policy",
  hostStatus: "/api/v1/host/status",
  vmList: "/api/v1/vms",
  jobList: "/api/v1/jobs"
};

const expected = {
  generatedBy: "src/generate-parity-manifest.ts",
  regeneratedBy: "scripts/regenerate-static-parity.mjs",
  decisionCandidate: "static-asset-parity-scaffold-first",
  runtimeReplacement: "default",
  servedAsset: "app.js",
  indexScriptSrc: "/app.js",
  typeScriptEntry: "src/app.ts",
  servedTypeScriptEntry: "src/served-app.ts",
  userVisibleFixtureEntry: "src/user-visible-fixtures.ts",
  fixtureNames: ["emptyInventory", "runningVmAndJob", "unsupportedHost"],
  manifestOutput: "generated/parity/static-asset-parity.manifest.json",
  buildServedCommand: "npm run build:served",
  checkServedCommand: "npm run check:served",
  servedBuilderScript: "scripts/build-served-asset.mjs",
  generateCommand: "npm run generate:parity",
  checkCommand: "npm run verify:parity",
  browserFixtureScript: "scripts/verify-browser-fixture.mjs",
  browserFixtureCommand: "npm run browser:fixture",
  browserFixtureMode: "node-vm-minimal-dom"
};

function readText(path) {
  return readFileSync(path, "utf8");
}

function fail(message) {
  console.error(`static parity verification failed: ${message}`);
  process.exitCode = 1;
}

function requireEqual(actual, expectedValue, label) {
  if (actual !== expectedValue) {
    fail(`${label} expected ${JSON.stringify(expectedValue)} but found ${JSON.stringify(actual)}`);
  }
}

function requireIncludes(text, value, label) {
  if (!text.includes(value)) {
    fail(`${label} is missing ${value}`);
  }
}

function requireNotIncludes(text, value, label) {
  if (text.includes(value)) {
    fail(`${label} must not include ${value}`);
  }
}

function requireFile(path, label) {
  if (!existsSync(path)) {
    fail(`${label} is missing`);
    return false;
  }

  return true;
}

const packageJson = JSON.parse(readText(paths.packageJson));
const index = readText(paths.index);
const appSource = readText(paths.appSource);
const fixtureSource = readText(paths.fixtureSource);
const servedSource = [
  ...paths.servedSourceParts.map(readText),
  readText(paths.servedSource)
].join("\n");
const generatorSource = readText(paths.generatorSource);
const servedBuilderScript = readText(paths.servedBuilderScript);
const regenerateScript = readText(paths.regenerateScript);
const browserFixtureScript = requireFile(paths.browserFixtureScript, "scripts/verify-browser-fixture.mjs")
  ? readText(paths.browserFixtureScript)
  : "";
const manifestText = readText(paths.manifest);
const manifest = JSON.parse(manifestText);

requireEqual(packageJson.scripts?.["build:served"], "node scripts/build-served-asset.mjs --write", "package build:served script");
requireEqual(packageJson.scripts?.["check:served"], "node scripts/build-served-asset.mjs --check", "package check:served script");
requireIncludes(packageJson.scripts?.test, "npm run check:served", "package test script");
requireEqual(packageJson.scripts?.["generate:parity"], "node scripts/regenerate-static-parity.mjs --write", "package generate:parity script");
requireEqual(
  packageJson.scripts?.["verify:parity"],
  "npm run check:served && node scripts/regenerate-static-parity.mjs --check && node scripts/verify-static-parity.mjs && npm run browser:fixture",
  "package verify:parity script"
);
requireEqual(packageJson.scripts?.["browser:fixture"], "node scripts/verify-browser-fixture.mjs", "package browser:fixture script");

requireIncludes(index, '<script src="/app.js" defer></script>', "index.html");
requireNotIncludes(index, "src/app.ts", "index.html");
requireNotIncludes(index, "dist/", "index.html");

requireEqual(manifest.generatedBy, expected.generatedBy, "manifest.generatedBy");
requireEqual(manifest.regeneratedBy, expected.regeneratedBy, "manifest.regeneratedBy");
requireEqual(manifest.servedAsset, expected.servedAsset, "manifest.servedAsset");
requireEqual(manifest.replacesServedAsset, true, "manifest.replacesServedAsset");
requireEqual(manifest.indexScriptSrc, expected.indexScriptSrc, "manifest.indexScriptSrc");
requireEqual(manifest.typeScriptEntry, expected.typeScriptEntry, "manifest.typeScriptEntry");
requireEqual(manifest.servedTypeScriptEntry, expected.servedTypeScriptEntry, "manifest.servedTypeScriptEntry");
requireEqual(manifest.userVisibleFixtureEntry, expected.userVisibleFixtureEntry, "manifest.userVisibleFixtureEntry");
requireEqual(manifest.scaffold?.decisionCandidate, expected.decisionCandidate, "manifest.scaffold.decisionCandidate");
requireEqual(manifest.scaffold?.runtimeReplacement, expected.runtimeReplacement, "manifest.scaffold.runtimeReplacement");
requireEqual(manifest.scaffold?.servedAsset, expected.servedAsset, "manifest.scaffold.servedAsset");
requireEqual(manifest.scaffold?.servedTypeScriptEntry, expected.servedTypeScriptEntry, "manifest.scaffold.servedTypeScriptEntry");
requireEqual(manifest.regeneration?.source, expected.typeScriptEntry, "manifest.regeneration.source");
requireEqual(manifest.regeneration?.output, expected.manifestOutput, "manifest.regeneration.output");
requireEqual(manifest.regeneration?.writeCommand, expected.generateCommand, "manifest.regeneration.writeCommand");
requireEqual(manifest.regeneration?.checkCommand, expected.checkCommand, "manifest.regeneration.checkCommand");
requireEqual(manifest.regeneration?.replacesServedAsset, true, "manifest.regeneration.replacesServedAsset");
requireEqual(manifest.browserFixture?.script, expected.browserFixtureScript, "manifest.browserFixture.script");
requireEqual(manifest.browserFixture?.command, expected.browserFixtureCommand, "manifest.browserFixture.command");
requireEqual(manifest.browserFixture?.mode, expected.browserFixtureMode, "manifest.browserFixture.mode");
requireEqual(manifest.browserFixture?.mutating, false, "manifest.browserFixture.mutating");
requireEqual(manifest.browserFixture?.replacesServedAsset, true, "manifest.browserFixture.replacesServedAsset");

for (const [name, route] of Object.entries(expectedRoutes)) {
  requireEqual(manifest.localApiRoutes?.[name], route, `manifest.localApiRoutes.${name}`);
  requireIncludes(appSource, route, `src/app.ts ${name}`);
  requireIncludes(manifestText, route, `generated manifest ${name}`);
}

requireIncludes(generatorSource, expected.generatedBy, "src/generate-parity-manifest.ts");
requireIncludes(generatorSource, expected.regeneratedBy, "src/generate-parity-manifest.ts");
requireIncludes(generatorSource, expected.userVisibleFixtureEntry, "src/generate-parity-manifest.ts");
requireIncludes(generatorSource, "phase25WebConsoleScaffold", "src/generate-parity-manifest.ts");
requireIncludes(generatorSource, "localApiRoutes", "src/generate-parity-manifest.ts");
requireIncludes(generatorSource, "browserFixture", "src/generate-parity-manifest.ts");

requireIncludes(fixtureSource, "webConsoleUserVisibleParityFixtures", "src/user-visible-fixtures.ts");
requireIncludes(fixtureSource, "buildStaticParitySnapshot", "src/user-visible-fixtures.ts");
requireIncludes(fixtureSource, expected.runtimeReplacement, "src/user-visible-fixtures.ts");
requireIncludes(fixtureSource, expected.servedAsset, "src/user-visible-fixtures.ts");
requireIncludes(fixtureSource, expected.servedTypeScriptEntry, "src/user-visible-fixtures.ts");
requireNotIncludes(fixtureSource, "document.", "src/user-visible-fixtures.ts");
requireNotIncludes(fixtureSource, "window.", "src/user-visible-fixtures.ts");

for (const fixtureName of expected.fixtureNames) {
  requireIncludes(fixtureSource, fixtureName, "src/user-visible-fixtures.ts");
  requireIncludes(manifestText, fixtureName, "generated manifest");
}

requireIncludes(regenerateScript, "typescript", "scripts/regenerate-static-parity.mjs");
requireIncludes(regenerateScript, "phase25WebConsoleScaffold", "scripts/regenerate-static-parity.mjs");
requireIncludes(regenerateScript, "localApiRoutes", "scripts/regenerate-static-parity.mjs");
requireIncludes(regenerateScript, expected.browserFixtureCommand, "scripts/regenerate-static-parity.mjs");
requireIncludes(regenerateScript, "--write", "scripts/regenerate-static-parity.mjs");
requireIncludes(regenerateScript, "--check", "scripts/regenerate-static-parity.mjs");

requireIncludes(servedSource, "DOMContentLoaded", "src/served-app.ts");
requireIncludes(servedSource, "apiFetch", "src/served-app.ts");
requireIncludes(servedSource, "/api/v1/ops/summary", "src/served-app.ts");
requireIncludes(servedSource, "renderOpsCockpit", "src/served-app.ts");
requireIncludes(servedSource, "renderIncidentCommand", "src/served-app.ts");
requireIncludes(servedSource, "/api/v1/host/status", "src/served-app.ts");
requireIncludes(servedSource, "/api/v1/vms", "src/served-app.ts");
requireIncludes(servedBuilderScript, "typescript", "scripts/build-served-asset.mjs");
requireIncludes(servedBuilderScript, "src/served-app.ts", "scripts/build-served-asset.mjs");
requireIncludes(servedBuilderScript, "app.js", "scripts/build-served-asset.mjs");
requireIncludes(servedBuilderScript, "--write", "scripts/build-served-asset.mjs");
requireIncludes(servedBuilderScript, "--check", "scripts/build-served-asset.mjs");

for (const value of [expected.decisionCandidate, expected.runtimeReplacement, expected.servedAsset, expected.servedTypeScriptEntry]) {
  requireIncludes(appSource, value, "src/app.ts");
  requireIncludes(manifestText, value, "generated manifest");
}

for (const value of [expected.generatedBy, expected.indexScriptSrc, expected.typeScriptEntry, expected.servedTypeScriptEntry, expected.userVisibleFixtureEntry]) {
  requireIncludes(manifestText, value, "generated manifest");
}

for (const value of [expected.regeneratedBy, expected.manifestOutput, expected.generateCommand, expected.checkCommand]) {
  requireIncludes(manifestText, value, "generated manifest");
  requireIncludes(regenerateScript, value, "scripts/regenerate-static-parity.mjs");
}

for (const value of [expected.browserFixtureScript, expected.browserFixtureCommand, expected.browserFixtureMode]) {
  requireIncludes(manifestText, value, "generated manifest");
}

const guardedTerms = [
  ["Restart", "-", "Computer"],
  ["msi", "exec"],
  ["New", "-", "Net", "Firewall", "Rule"],
  ["Register", "-", "Event", "Source"],
  ["New", "-", "Event", "Log"],
  ["Register", "-", "Scheduled", "Task"],
  ["New", "-", "VM"],
  ["Remove", "-", "VM"]
].map((parts) => parts.join(""));

const scannedText = [
  JSON.stringify(packageJson),
  readText(fileURLToPath(import.meta.url)),
  appSource,
  servedSource,
  fixtureSource,
  generatorSource,
  servedBuilderScript,
  regenerateScript,
  manifestText,
  browserFixtureScript
].join("\n");

const tokenValuePattern = new RegExp(["Bearer", "\\s+[A-Za-z0-9._~+/=-]+"].join(""));

if (tokenValuePattern.test(scannedText)) {
  fail("scanned parity files must not contain auth secrets");
}

for (const term of guardedTerms) {
  if (scannedText.includes(term)) {
    fail(`scanned parity files must not contain ${term}`);
  }
}

if (process.exitCode) {
  process.exit();
}

console.log("static parity verification passed");

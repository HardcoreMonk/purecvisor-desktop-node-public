import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import {
  DISCOVERY_ROOTS,
  discoverLegacyPesterInventory,
  validateMigrationManifest
} from "../scripts/verify-verification-migration-manifest.mjs";
import { WEB_CONTRACT_ERROR_CODES, WebContractError } from "../contracts/web-contract-harness.mjs";
import { WEB_STATIC_CONTRACTS } from "../contracts/web-static-contracts.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");
const schemaPath = path.join(repoRoot, "config/development-verification-migration-manifest.schema.json");
const manifestPath = path.join(repoRoot, "config/development-verification-migration-manifest.json");

function schema() {
  return JSON.parse(fs.readFileSync(schemaPath, "utf8"));
}

function validManifest() {
  return JSON.parse(fs.readFileSync(manifestPath, "utf8"));
}

function input(manifest, options = {}) {
  return { manifest, schema: schema(), repoRoot, requireWebLocalPass: false, ...options };
}

function webEntry(manifest) {
  return manifest.entries.find((entry) => entry.domain === "web");
}

function invalid(error) {
  return error instanceof WebContractError
    && error.code === WEB_CONTRACT_ERROR_CODES.manifestInvalid
    && !error.message.includes(repoRoot);
}

function invalidDetail(detail) {
  return (error) => invalid(error)
    && error.message === `${WEB_CONTRACT_ERROR_CODES.manifestInvalid}|${detail}`;
}

function copyContainedFixtureFile(root, relativePath) {
  if (typeof relativePath !== "string"
      || !relativePath
      || relativePath.includes("\\")
      || relativePath.startsWith("/")
      || path.posix.normalize(relativePath) !== relativePath
      || relativePath.split("/").includes("..")) {
    throw new Error("fixture evidence path is unsafe");
  }

  const repoRootReal = fs.realpathSync(repoRoot);
  const fixtureRootReal = fs.realpathSync(root);
  const segments = relativePath.split("/");
  const source = path.resolve(repoRootReal, ...segments);
  const target = path.resolve(fixtureRootReal, ...segments);
  if (!source.startsWith(repoRootReal + path.sep)
      || !target.startsWith(fixtureRootReal + path.sep)
      || fs.lstatSync(source).isSymbolicLink()
      || !fs.statSync(source).isFile()
      || !fs.realpathSync(source).startsWith(repoRootReal + path.sep)) {
    throw new Error("fixture evidence source is unsafe");
  }

  fs.mkdirSync(path.dirname(target), { recursive: true });
  fs.copyFileSync(source, target);
}

function legacyRootsFixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-manifest-roots-"));
  try {
    for (const definition of DISCOVERY_ROOTS) {
      const source = path.join(repoRoot, ...definition.relativeRoot.split("/"));
      const target = path.join(root, ...definition.relativeRoot.split("/"));
      fs.mkdirSync(target, { recursive: true });
      for (const file of fs.readdirSync(source)) {
        if (file.endsWith(".Tests.ps1")) fs.copyFileSync(path.join(source, file), path.join(target, file));
      }
    }
    copyContainedFixtureFile(root, webEntry(validManifest()).local_parity.evidence);
    return root;
  } catch (error) {
    fs.rmSync(root, { recursive: true, force: true });
    throw error;
  }
}

function tempInput(root, manifest = validManifest()) {
  return { manifest, schema: schema(), repoRoot: root, requireWebLocalPass: false };
}

function webTestPath(root) {
  return path.join(root, "web/tests/PcvDesktopWeb.Static.Tests.ps1");
}

test("accepts the canonical local-pass fixture", () => {
  const result = validateMigrationManifest(input(validManifest()));
  assert.deepEqual(result.summary, {
    total: 62, packaging: 55, installer: 6, web: 1, missing: 0, duplicate: 0,
    web_status: "mapped", web_local: "pass", web_ci: "pending"
  });
});

test("accepts the unmodified canonical manifest in an isolated legacy-roots fixture", () => {
  const root = legacyRootsFixture();
  try {
    assert.doesNotThrow(() => validateMigrationManifest(tempInput(root)));
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("rejects top-level, entry, and nested additional properties", () => {
  for (const mutate of [
    (manifest) => { manifest.unexpected = true; },
    (manifest) => { manifest.entries[0].unexpected = true; },
    (manifest) => { manifest.entries[0].local_parity.unexpected = true; }
  ]) {
    const manifest = validManifest();
    mutate(manifest);
    assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
  }
});

test("rejects missing, duplicate, and wrong-count inventory paths", () => {
  const missing = validManifest();
  missing.entries.pop();
  assert.throws(() => validateMigrationManifest(input(missing)), invalid);
  const duplicate = validManifest();
  duplicate.entries[1].legacy_path = duplicate.entries[0].legacy_path;
  assert.throws(() => validateMigrationManifest(input(duplicate)), invalid);
  const wrongCount = validManifest();
  wrongCount.entries.at(-1).legacy_contract_count = 49;
  assert.throws(() => validateMigrationManifest(input(wrongCount)), invalid);
});

test("rejects incorrect inventory constants and unknown enums", () => {
  const constants = validManifest();
  constants.inventory.total = 61;
  assert.throws(() => validateMigrationManifest(input(constants)), invalid);
  const enumValue = validManifest();
  enumValue.entries[0].parity_status = "future";
  assert.throws(() => validateMigrationManifest(input(enumValue)), invalid);
});

test("rejects Web contract ID omission, reordering, and duplication", () => {
  for (const mutate of [
    (entry) => { entry.replacement_contract_ids.pop(); },
    (entry) => { [entry.replacement_contract_ids[0], entry.replacement_contract_ids[1]] = [entry.replacement_contract_ids[1], entry.replacement_contract_ids[0]]; },
    (entry) => { entry.replacement_contract_ids[1] = entry.replacement_contract_ids[0]; }
  ]) {
    const manifest = validManifest();
    mutate(webEntry(manifest));
    assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
  }
});

test("rejects a mapped non-Web row and non-Web replacement data", () => {
  const manifest = validManifest();
  manifest.entries[0].parity_status = "mapped";
  manifest.entries[0].replacement_owner = "web/node-tests/web-static-contracts.test.mjs";
  manifest.entries[0].replacement_contract_ids = ["web.static.root-assets"];
  assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
});

test("rejects pass without evidence and pending status with evidence", () => {
  const passed = validManifest();
  webEntry(passed).local_parity = { status: "pass", evidence: null };
  assert.throws(() => validateMigrationManifest(input(passed)), invalid);
  const pending = validManifest();
  webEntry(pending).local_parity = { status: "pending", evidence: "web/package.json" };
  assert.throws(() => validateMigrationManifest(input(pending)), invalid);
});

test("rejects CI pass, Web dual-run-pass, and every cutover in Wave B", () => {
  for (const mutate of [
    (entry) => { entry.ci_parity = { status: "pass", evidence: "web/package.json" }; },
    (entry) => { entry.parity_status = "dual-run-pass"; entry.local_parity = { status: "pass", evidence: "web/package.json" }; entry.ci_parity = { status: "pass", evidence: "web/package.json" }; },
    (entry) => { entry.parity_status = "cutover"; }
  ]) {
    const manifest = validManifest();
    mutate(webEntry(manifest));
    assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
  }
  const nonWebCutover = validManifest();
  nonWebCutover.entries[0].parity_status = "cutover";
  assert.throws(() => validateMigrationManifest(input(nonWebCutover)), invalid);
});

test("allows a contained existing Web local-pass evidence locator but require flag rejects pending or missing evidence", () => {
  const passed = validManifest();
  webEntry(passed).local_parity = { status: "pass", evidence: "web/package.json" };
  assert.doesNotThrow(() => validateMigrationManifest(input(passed)));
  assert.doesNotThrow(() => validateMigrationManifest(input(passed, { requireWebLocalPass: true })));
  const pending = validManifest();
  webEntry(pending).local_parity = { status: "pending", evidence: null };
  assert.throws(() => validateMigrationManifest(input(pending, { requireWebLocalPass: true })), invalid);
  const missing = validManifest();
  webEntry(missing).local_parity = { status: "pass", evidence: "docs/ga-ready/evidence/missing.md" };
  assert.throws(() => validateMigrationManifest(input(missing, { requireWebLocalPass: true })), invalid);
  const directory = validManifest();
  webEntry(directory).local_parity = { status: "pass", evidence: "docs" };
  assert.throws(() => validateMigrationManifest(input(directory, { requireWebLocalPass: true })), invalid);
});

test("rejects a published schema weakened below the strict manifest contract", () => {
  const weakened = schema();
  delete weakened.$defs.parity.required;
  assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
});

test("rejects tampered legacy path, count, and replacement ID schema constraints", () => {
  for (const mutate of [
    (value) => { delete value.$defs.entry.properties.legacy_path.pattern; },
    (value) => { delete value.$defs.entry.properties.legacy_contract_count.minimum; },
    (value) => { delete value.$defs.entry.properties.replacement_contract_ids.items.pattern; }
  ]) {
    const weakened = schema();
    mutate(weakened);
    assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
  }
});

test("rejects extra published schema properties at every object level", () => {
  for (const mutate of [
    (value) => { value.properties.extra = {}; },
    (value) => { value.$defs.inventory.properties.extra = {}; },
    (value) => { value.$defs.parity.properties.extra = {}; },
    (value) => { value.$defs.entry.properties.extra = {}; }
  ]) {
    const weakened = schema();
    mutate(weakened);
    assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
  }
});

test("rejects root schema semantic keywords outside the approved contract", () => {
  const weakened = schema();
  weakened.not = {};
  assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
});

test("rejects extra schema definitions outside the approved contract", () => {
  const weakened = schema();
  weakened.$defs.extra = {};
  assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
});

test("rejects nested schema semantic keywords outside the approved contract", () => {
  const weakened = schema();
  weakened.properties.contract.not = {};
  weakened.properties.entries.maxContains = 0;
  assert.throws(() => validateMigrationManifest({ manifest: validManifest(), schema: weakened, repoRoot, requireWebLocalPass: false }), invalid);
});

test("rejects same-count renamed and reordered Web legacy declarations", () => {
  const first = WEB_STATIC_CONTRACTS[0].legacyName;
  const second = WEB_STATIC_CONTRACTS[1].legacyName;
  for (const rewrite of [
    (source) => source.replace("It '" + first + "'", "It 'renamed but same count'"),
    (source) => source.replace("It '" + first + "'", "It '__temporary__'").replace("It '" + second + "'", "It '" + first + "'").replace("It '__temporary__'", "It '" + second + "'")
  ]) {
    const root = legacyRootsFixture();
    try {
      fs.writeFileSync(webTestPath(root), rewrite(fs.readFileSync(webTestPath(root), "utf8")), "utf8");
      assert.throws(() => validateMigrationManifest(tempInput(root)), invalidDetail("web=mapping"));
    } finally {
      fs.rmSync(root, { recursive: true, force: true });
    }
  }
});

test("projects ambiguous Web It parsing as a stable manifest error", () => {
  const root = legacyRootsFixture();
  try {
    const first = WEB_STATIC_CONTRACTS[0].legacyName;
    const source = fs.readFileSync(webTestPath(root), "utf8").replace("It '" + first + "'", "It \"" + first + " $value\"");
    fs.writeFileSync(webTestPath(root), source, "utf8");
    assert.throws(
      () => validateMigrationManifest(tempInput(root)),
      invalidDetail("web=legacy-parse")
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("rejects a discovery-matching manifest that drifts from the frozen Appendix B ledger", () => {
  const root = legacyRootsFixture();
  try {
    const manifest = validManifest();
    const original = manifest.entries[0];
    const source = path.join(root, ...original.legacy_path.split("/"));
    const replacement = "packaging/windows-desktop-node/tests/Replacement.Tests.ps1";
    fs.renameSync(source, path.join(root, ...replacement.split("/")));
    original.legacy_path = replacement;
    assert.throws(() => validateMigrationManifest(tempInput(root, manifest)), invalidDetail("entries=canonical-ledger"));
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("discovers case-insensitive Pester suffixes and rejects a direct extra file", () => {
  const root = legacyRootsFixture();
  try {
    const extra = path.join(root, "packaging/windows-desktop-node/tests/Extra.tests.ps1");
    fs.writeFileSync(extra, 'It "extra" { }', "utf8");
    assert.equal(discoverLegacyPesterInventory(root).some((entry) => entry.legacy_path.endsWith("Extra.tests.ps1")), true);
    assert.throws(() => validateMigrationManifest(tempInput(root)), invalidDetail("entries=inventory-mismatch"));
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("rejects NUL paths and normalizes every single backslash to forward slash", () => {
  const nul = validManifest();
  nul.entries[0].legacy_path += "\0";
  assert.throws(() => validateMigrationManifest(input(nul)), invalid);
  const roots = [{ domain: "packaging", relativeRoot: "packaging\\windows-desktop-node\\tests" }];
  const rows = discoverLegacyPesterInventory(repoRoot, roots);
  assert.equal(rows.every((row) => !row.legacy_path.includes("\\")), true);
});

test("CLI emits the exact canonical output and redacts unknown-argument failures", () => {
  const run = (args) => spawnSync(process.execPath, ["web/scripts/verify-verification-migration-manifest.mjs", ...args], {
    cwd: repoRoot, encoding: "utf8", shell: false, windowsHide: true, timeout: 30_000, stdio: ["ignore", "pipe", "pipe"]
  });
  const pass = run([]);
  assert.equal(pass.status, 0);
  assert.equal(pass.stdout, "Verification migration manifest PASS: total=62 packaging=55 installer=6 web=1 missing=0 duplicate=0 web_status=mapped web_local=pass web_ci=pending\n");
  assert.equal(pass.stderr, "");
  const invalidArgument = run(["--unknown"]);
  assert.equal(invalidArgument.status, 1);
  assert.equal(invalidArgument.stdout, "");
  assert.match(invalidArgument.stderr, new RegExp(WEB_CONTRACT_ERROR_CODES.manifestInvalid));
  assert.equal(invalidArgument.stderr.includes(repoRoot), false);
});

test("rejects escaping and absolute evidence locators", () => {
  for (const locator of ["../outside.md", "C:/outside.md", "/outside.md"]) {
    const manifest = validManifest();
    webEntry(manifest).local_parity = { status: "pass", evidence: locator };
    assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
  }
});

test("rejects malformed null, array, object, and scalar shapes", () => {
  for (const mutate of [
    (manifest) => { manifest.inventory = null; },
    (manifest) => { manifest.entries = {}; },
    (manifest) => { manifest.entries[0].replacement_contract_ids = null; },
    (manifest) => { manifest.entries[0].local_parity = []; },
    (manifest) => { manifest.entries[0].legacy_contract_count = "7"; }
  ]) {
    const manifest = validManifest();
    mutate(manifest);
    assert.throws(() => validateMigrationManifest(input(manifest)), invalid);
  }
});

test("uses the deliberately broad line-start It count", () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-manifest-count-"));
  try {
    const tests = path.join(root, "packaging/windows-desktop-node/tests");
    fs.mkdirSync(tests, { recursive: true });
    fs.writeFileSync(path.join(tests, "Sample.Tests.ps1"), 'Describe "x" {\n  It\n    -Skip "continued" { }\n  It "second" { }\n}', "utf8");
    const inventory = discoverLegacyPesterInventory(root, [{ domain: "packaging", relativeRoot: "packaging/windows-desktop-node/tests" }]);
    assert.equal(inventory[0].legacy_contract_count, 2);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("sorts discovery paths by ordinal code unit order", () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-manifest-order-"));
  try {
    const tests = path.join(root, "packaging/windows-desktop-node/tests");
    fs.mkdirSync(tests, { recursive: true });
    for (const file of ["a.Tests.ps1", "Z.Tests.ps1"]) fs.writeFileSync(path.join(tests, file), 'It "x" { }', "utf8");
    const paths = discoverLegacyPesterInventory(root, [{ domain: "packaging", relativeRoot: "packaging/windows-desktop-node/tests" }]).map((entry) => entry.legacy_path);
    assert.deepEqual(paths, ["packaging/windows-desktop-node/tests/Z.Tests.ps1", "packaging/windows-desktop-node/tests/a.Tests.ps1"]);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("rejects symlinked direct discovery files and redacts their absolute target", () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-manifest-link-"));
  const outside = path.join(os.tmpdir(), `pcv-manifest-outside-${Date.now()}.Tests.ps1`);
  try {
    const tests = path.join(root, "web/tests");
    fs.mkdirSync(tests, { recursive: true });
    fs.writeFileSync(outside, 'It "outside" { }', "utf8");
    fs.symlinkSync(outside, path.join(tests, "Escape.Tests.ps1"), "file");
    assert.throws(
      () => discoverLegacyPesterInventory(root, [{ domain: "web", relativeRoot: "web/tests" }]),
      (error) => invalid(error) && !error.message.includes(outside)
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { force: true });
  }
});

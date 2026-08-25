import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { WEB_STATIC_CONTRACT_METADATA } from "../contracts/web-static-contracts.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");
const packageJson = JSON.parse(fs.readFileSync(path.join(repoRoot, "web/package.json"), "utf8"));
const suiteCatalog = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-suites.json"), "utf8"));
const migrationManifest = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-migration-manifest.json"), "utf8"));

const EVIDENCE_RELATIVE_PATH = "docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md";
const INSTALLER_EVIDENCE_RELATIVE_PATH = "docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md";
const EVIDENCE_INPUT_HEAD = "20ba3b80c211cc6a29bc9ecaf7e9195911678f14";
const EXPECTED_MAPPING_BYTES = 5077;
const EXPECTED_MAPPING_HASH = "91c00cdf3ed8cd6a39ebb27131c629d1b54561f362f8099b2716c21a6c7a4d95";
const EXACT_EVIDENCE_LINES = [
  `evidence_input_head=${EVIDENCE_INPUT_HEAD}`,
  "input_dirty_state=clean",
  `mapping_bytes=${EXPECTED_MAPPING_BYTES}`,
  `mapping_sha256=${EXPECTED_MAPPING_HASH}`,
  "ci_parity_pass=false",
  "required_ci_pester_zero=false",
  "required_ci_nonadmin_powershell_zero=false",
  "cutover_completed=false",
  "host_mutation_performed=false",
  "msi_or_service_mutation=false",
  "actual_vm_tested=false",
  "public_trusted_signing=false",
  "external_stable_publication=false",
  "operational_current=0.42.74-admin-smoke"
];

const EXPECTED_TEST_SCRIPT = "npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches";
const EXPECTED_PARITY_SCRIPT = "npm run check:served && node scripts/regenerate-static-parity.mjs --check && node scripts/verify-static-parity.mjs && npm run browser:fixture";
const EXPECTED_SEPARATE_SCRIPTS = {
  "check:web-contract-registry": "node scripts/verify-web-contract-registry.mjs",
  "check:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --check && node scripts/verify-verification-migration-manifest.mjs --require-web-local-pass",
  "generate:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --write",
  "test:web-contracts": "npm run check:web-contract-registry && npm run check:verification-migration-manifest && node --test --test-reporter=spec node-tests/web-static-contracts.test.mjs",
  "verify:web-contract-negative-parity": "node scripts/verify-web-contract-negative-parity.mjs"
};

function assertExactMachineAssignments(lines, expectedLines) {
  const expectedKeys = new Set();
  for (const expected of expectedLines) {
    const separator = expected.indexOf("=");
    assert.notEqual(separator, -1, expected);
    const key = expected.slice(0, separator);
    assert.equal(expectedKeys.has(key), false, key);
    expectedKeys.add(key);
    assert.deepEqual(
      lines.filter((line) => line.startsWith(`${key}=`)),
      [expected],
      key
    );
  }
}

test("keeps the default Web test and parity commands unchanged", () => {
  assert.equal(packageJson.scripts.test, EXPECTED_TEST_SCRIPT);
  assert.equal(packageJson.scripts["verify:parity"], EXPECTED_PARITY_SCRIPT);
});

test("keeps the verification suite catalog at the Wave C non-cutover boundary", () => {
  const webParity = suiteCatalog.suites.find((suite) => suite.id === "web-parity");
  const deliveryContracts = suiteCatalog.suites.find((suite) => suite.id === "delivery-contracts");
  const installerContracts = suiteCatalog.suites.find((suite) => suite.id === "installer-contracts");
  const evidenceCheck = suiteCatalog.suites.find((suite) => suite.id === "evidence-check");
  assert.equal(suiteCatalog.activation_state, "plan-only-foundation");
  assert.equal(webParity?.migration_state, "wave-b-pending");
  assert.equal(deliveryContracts?.migration_state, "wave-d-pending");
  assert.equal(installerContracts?.migration_state, "mapped");
  assert.equal(evidenceCheck?.migration_state, "wave-d-pending");
});

test("exposes Web contract verification only through separate commands", () => {
  const actual = Object.fromEntries(
    Object.keys(EXPECTED_SEPARATE_SCRIPTS).map((name) => [name, packageJson.scripts[name]])
  );
  assert.deepEqual(actual, EXPECTED_SEPARATE_SCRIPTS);
});

test("promotes only Web and Installer migration rows to local parity", () => {
  const webEntries = migrationManifest.entries.filter((entry) => entry.domain === "web");
  assert.equal(webEntries.length, 1);

  const [webEntry] = webEntries;
  assert.equal(webEntry.parity_status, "mapped");
  assert.deepEqual(webEntry.local_parity, {
    status: "pass",
    evidence: EVIDENCE_RELATIVE_PATH
  });
  assert.deepEqual(webEntry.ci_parity, {
    status: "pending",
    evidence: null
  });

  const installerEntries = migrationManifest.entries.filter((entry) => entry.domain === "installer");
  assert.equal(installerEntries.length, 6);
  for (const entry of installerEntries) {
    assert.equal(entry.parity_status, "mapped", entry.legacy_path);
    assert.deepEqual(entry.local_parity, {
      status: "pass",
      evidence: INSTALLER_EVIDENCE_RELATIVE_PATH
    }, entry.legacy_path);
    assert.deepEqual(entry.ci_parity, { status: "pending", evidence: null }, entry.legacy_path);
  }

  const packagingEntries = migrationManifest.entries.filter((entry) => entry.domain === "packaging");
  assert.equal(packagingEntries.length, 55);
  for (const entry of packagingEntries) {
    assert.equal(entry.parity_status, "unmapped", entry.legacy_path);
    assert.deepEqual(entry.local_parity, { status: "pending", evidence: null }, entry.legacy_path);
    assert.deepEqual(entry.ci_parity, { status: "pending", evidence: null }, entry.legacy_path);
  }

  assert.equal(migrationManifest.entries.length, 62);
});

test("records the clean evidence input and exact non-cutover claims once", () => {
  const evidence = fs.readFileSync(path.join(repoRoot, EVIDENCE_RELATIVE_PATH), "utf8");
  const lines = evidence.split(/\r?\n/);

  assertExactMachineAssignments(lines, EXACT_EVIDENCE_LINES);
});

test("rejects a conflicting machine evidence assignment", () => {
  const expected = "ci_parity_pass=false";
  assert.throws(() => assertExactMachineAssignments(
    [expected, "ci_parity_pass=unexpected"],
    [expected]
  ));
});

test("rejects a duplicate expected machine evidence assignment", () => {
  const expected = "ci_parity_pass=false";
  assert.throws(() => assertExactMachineAssignments([expected, expected], [expected]));
});

test("binds the evidence to the exact registry-order mapping bytes", () => {
  const mapping = WEB_STATIC_CONTRACT_METADATA
    .map(({ legacyName, id }) => `${legacyName}\0${id}\n`)
    .join("");
  const bytes = Buffer.from(mapping, "utf8");
  const hash = crypto.createHash("sha256").update(bytes).digest("hex");

  assert.equal(WEB_STATIC_CONTRACT_METADATA.length, 50);
  assert.equal(bytes.length, EXPECTED_MAPPING_BYTES);
  assert.equal(hash, EXPECTED_MAPPING_HASH);
});

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
const PACKAGING_EVIDENCE_RELATIVE_PATH = "docs/ga-ready/evidence/pester-free-packaging-wave-d-2026-08-25.md";
const CUTOVER_EVIDENCE_RELATIVE_PATH = "docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md";
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
const EXPECTED_REQUIRED_SCRIPT = "npm test && npm run test:web-contracts && npm run verify:parity";
const EXPECTED_WEB_CONTRACTS_SCRIPT = "npm run check:web-contract-registry && npm run check:verification-migration-manifest && node --test --test-reporter=spec node-tests/web-contract-harness.test.mjs node-tests/web-static-contracts.test.mjs node-tests/web-static-contracts-negative.test.mjs node-tests/verification-migration-manifest.test.mjs node-tests/web-contract-negative-parity.test.mjs node-tests/web-verification-architecture-boundary.test.mjs";
const EXPECTED_SEPARATE_SCRIPTS = {
  "check:web-contract-registry": "node scripts/verify-web-contract-registry.mjs",
  "check:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --check && node scripts/verify-verification-migration-manifest.mjs --require-web-local-pass",
  "generate:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --write",
  "test:web-contracts": EXPECTED_WEB_CONTRACTS_SCRIPT,
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

test("composes the shell-free required Web command without transitional Pester parity", () => {
  assert.equal(packageJson.scripts.test, EXPECTED_TEST_SCRIPT);
  assert.equal(packageJson.scripts["verify:parity"], EXPECTED_PARITY_SCRIPT);
  assert.equal(packageJson.scripts["test:required"], EXPECTED_REQUIRED_SCRIPT);
  assert.equal(packageJson.scripts["test:required"].includes("verify:web-contract-negative-parity"), false);
});

test("keeps every catalog activation at its exact migration-state boundary", () => {
  const expectedByActivation = {
    "plan-only-foundation": [
      "native-existing", "native-existing", "wave-b-pending", "mapped", "mapped", "mapped", "wave-a-foundation"
    ],
    "shadow-ready": [
      "native-existing", "native-existing", "mapped", "mapped", "mapped", "mapped", "mapped"
    ],
    active: Array(7).fill("cutover")
  };
  assert.deepEqual(
    suiteCatalog.suites.map((suite) => suite.migration_state),
    expectedByActivation[suiteCatalog.activation_state]
  );
});

test("pins every Web contract command while retaining transitional parity for shadow only", () => {
  const actual = Object.fromEntries(
    Object.keys(EXPECTED_SEPARATE_SCRIPTS).map((name) => [name, packageJson.scripts[name]])
  );
  assert.deepEqual(actual, EXPECTED_SEPARATE_SCRIPTS);
});

test("records all Web, Installer, and Packaging rows at coherent local and CI parity", () => {
  const cutover = Object.hasOwn(migrationManifest, "cutover_locator");
  const expectedStatus = cutover ? "cutover" : "mapped";
  const expectedCi = cutover
    ? { status: "pass", evidence: CUTOVER_EVIDENCE_RELATIVE_PATH }
    : { status: "pending", evidence: null };
  const webEntries = migrationManifest.entries.filter((entry) => entry.domain === "web");
  assert.equal(webEntries.length, 1);

  const [webEntry] = webEntries;
  assert.equal(webEntry.parity_status, expectedStatus);
  assert.deepEqual(webEntry.local_parity, {
    status: "pass",
    evidence: EVIDENCE_RELATIVE_PATH
  });
  assert.deepEqual(webEntry.ci_parity, expectedCi);

  const installerEntries = migrationManifest.entries.filter((entry) => entry.domain === "installer");
  assert.equal(installerEntries.length, 6);
  for (const entry of installerEntries) {
    assert.equal(entry.parity_status, expectedStatus, entry.legacy_path);
    assert.deepEqual(entry.local_parity, {
      status: "pass",
      evidence: INSTALLER_EVIDENCE_RELATIVE_PATH
    }, entry.legacy_path);
    assert.deepEqual(entry.ci_parity, expectedCi, entry.legacy_path);
  }

  const packagingEntries = migrationManifest.entries.filter((entry) => entry.domain === "packaging");
  assert.equal(packagingEntries.length, 55);
  for (const entry of packagingEntries) {
    assert.equal(entry.parity_status, expectedStatus, entry.legacy_path);
    assert.deepEqual(entry.local_parity, {
      status: "pass",
      evidence: PACKAGING_EVIDENCE_RELATIVE_PATH
    }, entry.legacy_path);
    assert.deepEqual(entry.ci_parity, expectedCi, entry.legacy_path);
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

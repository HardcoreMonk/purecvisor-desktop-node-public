import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
import {
  buildMigrationManifest,
  buildMigrationManifestSchema,
  canonicalManifestJson,
  discoverLegacyContractInventory,
  discoverReplacementContractInventory,
  parseLegacyPesterContracts
} from "../scripts/regenerate-verification-migration-manifest.mjs";
import { validateMigrationManifest } from "../scripts/verify-verification-migration-manifest.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");
const manifestPath = path.join(repoRoot, "config/development-verification-migration-manifest.json");
const schemaPath = path.join(repoRoot, "config/development-verification-migration-manifest.schema.json");
const evidencePath = "docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md";
const invalid = (detail) => (error) =>
  error instanceof Error &&
  error.message.includes("PCV_VERIFICATION_MIGRATION_MANIFEST_INVALID") &&
  error.message.includes(detail);

function published() {
  return {
    manifest: JSON.parse(fs.readFileSync(manifestPath, "utf8")),
    schema: JSON.parse(fs.readFileSync(schemaPath, "utf8"))
  };
}

function clone(value) {
  return structuredClone(value);
}

function input(manifest, schema = published().schema) {
  return { manifest, schema, repoRoot, requireWebLocalPass: true };
}

function firstMappedContract(manifest) {
  return manifest.contracts.find((row) => row.replacement_contract_id !== null);
}

test("published strict v2 ledger validates with exact 62-file and 627-contract inventory", () => {
  const { manifest, schema } = published();

  const result = validateMigrationManifest(input(manifest, schema));

  assert.deepEqual(manifest.inventory, {
    files: { total: 62, packaging: 55, installer: 6, web: 1 },
    contracts: { total: 627, packaging: 528, installer: 49, web: 50 }
  });
  assert.equal(manifest.entries.length, 62);
  assert.equal(manifest.contracts.length, 627);
  assert.equal(result.summary.files_total, 62);
  assert.equal(result.summary.contracts_total, 627);
  assert.equal(result.summary.missing, 0);
  assert.equal(result.summary.duplicate, 0);
  assert.equal(result.summary.order_drift, 0);
});

test("generator discovers exact legacy and replacement inventories and is byte deterministic", () => {
  const { manifest } = published();
  const legacy = discoverLegacyContractInventory(repoRoot);
  const replacements = discoverReplacementContractInventory(repoRoot);
  const generated = buildMigrationManifest({ repoRoot, previousManifest: manifest });

  assert.equal(legacy.entries.length, 62);
  assert.equal(legacy.contracts.length, 627);
  assert.equal(legacy.contracts.filter((row) => row.domain === "packaging").length, 528);
  assert.equal(legacy.contracts.filter((row) => row.domain === "installer").length, 49);
  assert.equal(legacy.contracts.filter((row) => row.domain === "web").length, 50);
  assert.equal(new Set(replacements.map((row) => row.replacementContractId)).size, replacements.length);
  assert.equal(canonicalManifestJson(generated), fs.readFileSync(manifestPath, "utf8"));
});

test("generator advances a newly discovered complete v2 file mapping from unmapped to mapped", () => {
  const prior = clone(published().manifest);
  const legacyPath = "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1";
  const priorEntry = prior.entries.find((row) => row.legacy_path === legacyPath);
  priorEntry.parity_status = "unmapped";
  priorEntry.local_parity = { status: "pending", evidence: null };
  priorEntry.ci_parity = { status: "pending", evidence: null };
  for (const row of prior.contracts.filter((contract) => contract.legacy_path === legacyPath)) {
    row.replacement_owner = null;
    row.replacement_contract_id = null;
    row.parity_status = "unmapped";
    row.local_parity = { status: "pending", evidence: null };
    row.ci_parity = { status: "pending", evidence: null };
  }

  const generated = buildMigrationManifest({ repoRoot, previousManifest: prior });
  const entry = generated.entries.find((row) => row.legacy_path === legacyPath);
  const contracts = generated.contracts.filter((row) => row.legacy_path === legacyPath);

  assert.equal(entry.parity_status, "mapped");
  assert.deepEqual(entry.local_parity, { status: "pending", evidence: null });
  assert.deepEqual(entry.ci_parity, { status: "pending", evidence: null });
  assert.equal(contracts.length, 4);
  assert.equal(contracts.every((row) => row.parity_status === "mapped"), true);
  assert.equal(contracts.every((row) => row.local_parity.status === "pending"), true);
});

test("literal parser ignores comments and here-strings and rejects dynamic or malformed names", () => {
  const source = [
    "# It 'ignored' { }",
    "<# It 'also ignored' { } #>",
    "$text = @'",
    "It 'inside here string' { }",
    "'@",
    "Describe 'x' { It 'first' { }; It \"literal `$value\" { } }"
  ].join("\n");
  assert.deepEqual(parseLegacyPesterContracts(source), [
    { legacyOrdinal: 1, legacyName: "first" },
    { legacyOrdinal: 2, legacyName: "literal $value" }
  ]);
  for (const [candidate, detail] of [
    ["It \"dynamic $value\" { }", "dynamic-name"],
    ["It 'same' { }; It 'same' { }", "duplicate-name"],
    ["It\n  'continued' { }", "multiline-declaration"],
    ["It 'unterminated", "unmatched-quote"],
    ["<# unterminated", "unmatched-comment"],
    ["@'\nunterminated", "unmatched-here-string"]
  ]) {
    assert.throws(() => parseLegacyPesterContracts(candidate), invalid(detail));
  }
});

test("entries and contracts are unique, canonical, ordered, and aggregate-coherent", () => {
  const { manifest } = published();
  const fileKeys = manifest.entries.map((row) => row.legacy_path);
  const contractKeys = manifest.contracts.map((row) => `${row.legacy_path}\0${row.legacy_ordinal}`);
  const replacementIds = manifest.contracts
    .map((row) => row.replacement_contract_id)
    .filter((value) => value !== null);

  assert.equal(new Set(fileKeys).size, 62);
  assert.equal(new Set(contractKeys).size, 627);
  assert.equal(new Set(replacementIds).size, replacementIds.length);
  for (const entry of manifest.entries) {
    assert.equal(
      manifest.contracts.filter((row) => row.legacy_path === entry.legacy_path).length,
      entry.legacy_contract_count
    );
  }
});

test("Web remains exactly 50 mapped local-pass contracts with CI pending until cutover", () => {
  const { manifest } = published();
  const entry = manifest.entries.find((row) => row.domain === "web");
  const contracts = manifest.contracts.filter((row) => row.domain === "web");

  assert.equal(entry.parity_status, "mapped");
  assert.deepEqual(entry.local_parity, { status: "pass", evidence: evidencePath });
  assert.deepEqual(entry.ci_parity, { status: "pending", evidence: null });
  assert.equal(contracts.length, 50);
  assert.equal(contracts.every((row) => row.replacement_contract_id.startsWith("web.static.")), true);
  assert.equal(contracts.every((row) => row.local_parity.status === "pass"), true);
});

test("rejects one missing contract, duplicate ordinal, duplicate ID, and reordered name", () => {
  const baseline = published().manifest;
  const cases = [
    ["contracts=missing", (value) => value.contracts.pop()],
    ["contracts=duplicate-key", (value) => {
      value.contracts[1].legacy_path = value.contracts[0].legacy_path;
      value.contracts[1].legacy_ordinal = value.contracts[0].legacy_ordinal;
    }],
    ["contracts=duplicate-replacement", (value) => {
      const mapped = value.contracts.filter((row) => row.replacement_contract_id !== null);
      mapped[1].replacement_contract_id = mapped[0].replacement_contract_id;
    }],
    ["contracts=legacy-order", (value) => {
      const rows = value.contracts.filter((row) => row.legacy_path === value.contracts[0].legacy_path);
      [rows[0].legacy_name, rows[1].legacy_name] = [rows[1].legacy_name, rows[0].legacy_name];
    }]
  ];
  for (const [detail, mutate] of cases) {
    const candidate = clone(baseline);
    mutate(candidate);
    assert.throws(() => validateMigrationManifest(input(candidate)), invalid(detail));
  }
});

test("rejects wrong owner, mapping/state incoherence, pass without evidence, and unknown ID prefix", () => {
  const baseline = published().manifest;
  const cases = [
    ["replacement=owner", (value) => { firstMappedContract(value).replacement_owner = "wrong/owner"; }],
    ["state=mapped-null", (value) => {
      const row = firstMappedContract(value);
      row.replacement_owner = null;
      row.replacement_contract_id = null;
    }],
    ["state=unmapped-replacement", (value) => { firstMappedContract(value).parity_status = "unmapped"; }],
    ["parity=evidence", (value) => { firstMappedContract(value).local_parity = { status: "pass", evidence: null }; }],
    ["replacement=id", (value) => { firstMappedContract(value).replacement_contract_id = "unknown.prefix.contract"; }]
  ];
  for (const [detail, mutate] of cases) {
    const candidate = clone(baseline);
    mutate(candidate);
    assert.throws(() => validateMigrationManifest(input(candidate)), invalid(detail));
  }
});

test("rejects additional properties and every published schema weakening", () => {
  const baseline = published();
  const extra = clone(baseline.manifest);
  extra.contracts[0].extra = true;
  assert.throws(() => validateMigrationManifest(input(extra)), invalid("contract=shape"));

  for (const mutate of [
    (value) => { value.additionalProperties = true; },
    (value) => { value.required.pop(); },
    (value) => { delete value.$defs.contract.additionalProperties; },
    (value) => { value.$defs.contract.properties.replacement_contract_id.pattern = ".*"; },
    (value) => { value.$defs.entry.properties.legacy_contract_count.minimum = -1; }
  ]) {
    const schema = clone(baseline.schema);
    mutate(schema);
    assert.throws(
      () => validateMigrationManifest(input(baseline.manifest, schema)),
      invalid("schema=invalid")
    );
  }
});

test("generated schema is byte-equivalent to the strict published schema", () => {
  const expected = `${JSON.stringify(buildMigrationManifestSchema(), null, 2)}\n`;
  assert.equal(expected, fs.readFileSync(schemaPath, "utf8"));
});

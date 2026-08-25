import fs from "node:fs";
import path from "node:path";
import { isDeepStrictEqual } from "node:util";
import { fileURLToPath } from "node:url";
import { WEB_CONTRACT_ERROR_CODES, WebContractError } from "../contracts/web-contract-harness.mjs";
import {
  buildMigrationManifestSchema,
  discoverLegacyContractInventory,
  discoverReplacementContractInventory
} from "./regenerate-verification-migration-manifest.mjs";

const EXPECTED_INVENTORY = Object.freeze({
  files: Object.freeze({ total: 62, packaging: 55, installer: 6, web: 1 }),
  contracts: Object.freeze({ total: 627, packaging: 528, installer: 49, web: 50 })
});
const ROOT_KEYS = ["contract", "schema_version", "inventory", "entries", "contracts"];
const ENTRY_KEYS = ["legacy_path", "domain", "legacy_contract_count", "parity_status", "local_parity", "ci_parity"];
const CONTRACT_KEYS = ["legacy_path", "legacy_ordinal", "legacy_name", "domain", "replacement_owner", "replacement_contract_id", "parity_status", "local_parity", "ci_parity"];
const PARITY_KEYS = ["status", "evidence"];
const STATES = ["unmapped", "mapped", "dual-run-pass", "cutover"];
const DOMAINS = ["packaging", "installer", "web"];
const fail = (detail) => new WebContractError(WEB_CONTRACT_ERROR_CODES.manifestInvalid, detail);
const object = (value) => value !== null && typeof value === "object" && !Array.isArray(value) && Object.getPrototypeOf(value) === Object.prototype;
const keys = (value, expected) => object(value) && Object.keys(value).length === expected.length && expected.every((key) => Object.hasOwn(value, key));

function rootOf(repoRoot) {
  try {
    const root = fs.realpathSync(path.resolve(repoRoot));
    if (!fs.statSync(root).isDirectory()) throw new Error("not-directory");
    return root;
  } catch {
    throw fail("repo_root=invalid");
  }
}

function relative(value) {
  if (typeof value !== "string" || !value || value.includes("\0") || value.includes("\\")) throw fail("path=invalid");
  if (value.startsWith("/") || /^[A-Za-z]:/.test(value) || value.split("/").some((part) => !part || part === "." || part === "..")) throw fail("path=escape");
  return value;
}

function containedFile(root, value) {
  const normalized = relative(value);
  const candidate = path.resolve(root, ...normalized.split("/"));
  if (!candidate.startsWith(`${root}${path.sep}`)) throw fail("path=escape");
  try {
    const stat = fs.lstatSync(candidate);
    if (!stat.isFile() || stat.isSymbolicLink() || !fs.realpathSync(candidate).startsWith(`${root}${path.sep}`)) throw new Error("unsafe");
    return candidate;
  } catch {
    throw fail("path=unsafe");
  }
}

function validateParity(value, root) {
  if (!keys(value, PARITY_KEYS) || !["pending", "pass", "fail"].includes(value.status) || !(typeof value.evidence === "string" || value.evidence === null)) {
    throw fail("parity=shape");
  }
  if (value.status === "pending") {
    if (value.evidence !== null) throw fail("parity=pending-evidence");
    return;
  }
  if (typeof value.evidence !== "string" || !value.evidence) throw fail("parity=evidence");
  containedFile(root, value.evidence);
}

function validateState(row) {
  const mapped = row.replacement_owner !== null || row.replacement_contract_id !== null;
  if (row.parity_status === "unmapped") {
    if (mapped) throw fail("state=unmapped-replacement");
    if (row.local_parity.status !== "pending" || row.ci_parity.status !== "pending") throw fail("state=unmapped-parity");
    return;
  }
  if (!mapped || row.replacement_owner === null || row.replacement_contract_id === null) throw fail("state=mapped-null");
  if (row.parity_status === "mapped") {
    if (!["pending", "pass"].includes(row.local_parity.status) || row.ci_parity.status !== "pending") throw fail("state=mapped-parity");
  } else if (row.parity_status === "dual-run-pass" || row.parity_status === "cutover") {
    if (row.local_parity.status !== "pass" || row.ci_parity.status !== "pass") throw fail("state=advanced-parity");
  }
}

function validateEntryState(entry, children) {
  const mappedChildren = children.filter((row) => row.replacement_contract_id !== null);
  if (mappedChildren.length !== 0 && mappedChildren.length !== children.length) throw fail("entry=partial-mapping");
  if (entry.parity_status === "unmapped") {
    if (mappedChildren.length !== 0 || entry.local_parity.status !== "pending" || entry.ci_parity.status !== "pending") throw fail("entry=unmapped-state");
  } else {
    if (mappedChildren.length !== children.length) throw fail("entry=mapped-state");
  }
  for (const child of children) {
    if (child.parity_status !== entry.parity_status || !isDeepStrictEqual(child.local_parity, entry.local_parity) || !isDeepStrictEqual(child.ci_parity, entry.ci_parity)) {
      throw fail("entry=contract-coherence");
    }
  }
}

function idPrefixValid(domain, id) {
  if (typeof id !== "string") return false;
  if (domain === "web") return /^web\.static\.[a-z0-9]+(?:-[a-z0-9]+)*$/u.test(id);
  if (domain === "installer") return /^pcv\.installer\.[a-z0-9]+(?:-[a-z0-9]+)*\.[0-9]{3}$/u.test(id);
  return /^pcv\.delivery\.[a-z0-9]+(?:-[a-z0-9]+)*\.[0-9]{3}$/u.test(id);
}

export function validateMigrationManifest({
  manifest,
  schema,
  repoRoot,
  requireWebLocalPass = false,
  requireInstallerLocalPass = false,
  requirePackagingLocalPass = false,
  requireAllMapped = false,
  requireCutover = false
} = {}) {
  const root = rootOf(repoRoot);
  if (!isDeepStrictEqual(schema, buildMigrationManifestSchema())) throw fail("schema=invalid");
  if (!keys(manifest, ROOT_KEYS) || manifest.contract !== "pcv-development-verification-migration-manifest-v2" || manifest.schema_version !== 2 ||
      !isDeepStrictEqual(manifest.inventory, EXPECTED_INVENTORY) || !Array.isArray(manifest.entries) || !Array.isArray(manifest.contracts)) {
    throw fail("manifest=shape");
  }
  if (manifest.entries.length !== 62) throw fail("entries=count");
  if (manifest.contracts.length < 627) throw fail("contracts=missing");
  if (manifest.contracts.length !== 627) throw fail("contracts=count");
  for (const option of [requireWebLocalPass, requireInstallerLocalPass, requirePackagingLocalPass, requireAllMapped, requireCutover]) {
    if (typeof option !== "boolean") throw fail("options=invalid");
  }

  const legacy = discoverLegacyContractInventory(root);
  const replacements = discoverReplacementContractInventory(root);
  const replacementByKey = new Map(replacements.map((row) => [`${row.legacyPath}\0${row.legacyOrdinal}`, row]));
  const contractKeys = new Set();
  const replacementIds = new Set();

  for (let index = 0; index < manifest.contracts.length; index++) {
    const row = manifest.contracts[index];
    if (!keys(row, CONTRACT_KEYS) || typeof row.legacy_path !== "string" || !Number.isInteger(row.legacy_ordinal) || row.legacy_ordinal < 1 ||
        typeof row.legacy_name !== "string" || !row.legacy_name || !DOMAINS.includes(row.domain) ||
        !(typeof row.replacement_owner === "string" || row.replacement_owner === null) ||
        !(typeof row.replacement_contract_id === "string" || row.replacement_contract_id === null) || !STATES.includes(row.parity_status)) {
      throw fail("contract=shape");
    }
    relative(row.legacy_path);
    validateParity(row.local_parity, root);
    validateParity(row.ci_parity, root);
    validateState(row);

    const key = `${row.legacy_path}\0${row.legacy_ordinal}`;
    if (contractKeys.has(key)) throw fail("contracts=duplicate-key");
    contractKeys.add(key);
    if (row.replacement_contract_id !== null) {
      if (!idPrefixValid(row.domain, row.replacement_contract_id)) throw fail("replacement=id");
      if (replacementIds.has(row.replacement_contract_id)) throw fail("contracts=duplicate-replacement");
      replacementIds.add(row.replacement_contract_id);
    }

    const expected = legacy.contracts[index];
    if (!expected || expected.legacy_path !== row.legacy_path || expected.legacy_ordinal !== row.legacy_ordinal || expected.legacy_name !== row.legacy_name || expected.domain !== row.domain) {
      throw fail("contracts=legacy-order");
    }
    const replacement = replacementByKey.get(key) ?? null;
    if (replacement === null) {
      if (row.replacement_owner !== null || row.replacement_contract_id !== null) throw fail("replacement=orphan");
    } else {
      if (row.replacement_owner !== replacement.replacementOwner) throw fail("replacement=owner");
      if (row.replacement_contract_id !== replacement.replacementContractId) throw fail("replacement=id");
      if (row.legacy_name !== replacement.legacyName || row.domain !== replacement.domain) throw fail("replacement=legacy");
    }
  }

  const entryKeys = new Set();
  for (let index = 0; index < manifest.entries.length; index++) {
    const entry = manifest.entries[index];
    if (!keys(entry, ENTRY_KEYS) || typeof entry.legacy_path !== "string" || !DOMAINS.includes(entry.domain) ||
        !Number.isInteger(entry.legacy_contract_count) || entry.legacy_contract_count < 0 || !STATES.includes(entry.parity_status)) {
      throw fail("entry=shape");
    }
    relative(entry.legacy_path);
    validateParity(entry.local_parity, root);
    validateParity(entry.ci_parity, root);
    if (entryKeys.has(entry.legacy_path)) throw fail("entries=duplicate");
    entryKeys.add(entry.legacy_path);
    const expected = legacy.entries[index];
    if (!expected || expected.legacy_path !== entry.legacy_path || expected.domain !== entry.domain || expected.legacy_contract_count !== entry.legacy_contract_count) {
      throw fail("entries=legacy-order");
    }
    const children = manifest.contracts.filter((row) => row.legacy_path === entry.legacy_path);
    if (children.length !== entry.legacy_contract_count) throw fail("entry=contract-count");
    validateEntryState(entry, children);
  }

  const requireLocalPass = (domain, required) => {
    if (!required) return;
    const rows = manifest.contracts.filter((row) => row.domain === domain);
    if (!rows.length || rows.some((row) => row.replacement_contract_id === null || row.local_parity.status !== "pass")) throw fail(`${domain}=local-pass-required`);
  };
  requireLocalPass("web", requireWebLocalPass);
  requireLocalPass("installer", requireInstallerLocalPass);
  requireLocalPass("packaging", requirePackagingLocalPass);
  if (requireAllMapped && manifest.contracts.some((row) => row.replacement_contract_id === null)) throw fail("contracts=unmapped");
  if (requireCutover && manifest.contracts.some((row) => row.parity_status !== "cutover")) throw fail("contracts=cutover-required");

  const count = (domain, predicate) => manifest.contracts.filter((row) => row.domain === domain && predicate(row)).length;
  return {
    summary: {
      files_total: manifest.entries.length,
      contracts_total: manifest.contracts.length,
      web_mapped: count("web", (row) => row.replacement_contract_id !== null),
      web_local_pass: count("web", (row) => row.local_parity.status === "pass"),
      web_ci_pending: count("web", (row) => row.ci_parity.status === "pending"),
      installer_mapped: count("installer", (row) => row.replacement_contract_id !== null),
      installer_local_pass: count("installer", (row) => row.local_parity.status === "pass"),
      installer_ci_pending: count("installer", (row) => row.ci_parity.status === "pending"),
      packaging_mapped: count("packaging", (row) => row.replacement_contract_id !== null),
      packaging_local_pass: count("packaging", (row) => row.local_parity.status === "pass"),
      packaging_unmapped: count("packaging", (row) => row.replacement_contract_id === null),
      missing: 0,
      duplicate: 0,
      order_drift: 0
    }
  };
}

function runCli() {
  const allowed = new Set(["--require-web-local-pass", "--require-installer-local-pass", "--require-packaging-local-pass", "--require-all-mapped", "--require-cutover"]);
  const args = process.argv.slice(2);
  if (args.some((value) => !allowed.has(value)) || new Set(args).size !== args.length) throw fail("arguments=invalid");
  const scriptPath = fileURLToPath(import.meta.url);
  const repoRoot = path.resolve(path.dirname(scriptPath), "../..");
  const manifest = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-migration-manifest.json"), "utf8"));
  const schema = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-migration-manifest.schema.json"), "utf8"));
  const { summary } = validateMigrationManifest({
    manifest,
    schema,
    repoRoot,
    requireWebLocalPass: args.includes("--require-web-local-pass"),
    requireInstallerLocalPass: args.includes("--require-installer-local-pass"),
    requirePackagingLocalPass: args.includes("--require-packaging-local-pass"),
    requireAllMapped: args.includes("--require-all-mapped"),
    requireCutover: args.includes("--require-cutover")
  });
  process.stdout.write(
    `Verification migration manifest PASS: files_total=${summary.files_total} contracts_total=${summary.contracts_total} ` +
    `web_mapped=${summary.web_mapped} web_local_pass=${summary.web_local_pass} ` +
    `installer_mapped=${summary.installer_mapped} installer_local_pass=${summary.installer_local_pass} ` +
    `packaging_unmapped=${summary.packaging_unmapped} missing=0 duplicate=0 order_drift=0\n`
  );
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    runCli();
  } catch (error) {
    const message = error instanceof WebContractError ? error.message : `${WEB_CONTRACT_ERROR_CODES.manifestInvalid}|internal`;
    process.stderr.write(`${message}\n`);
    process.exitCode = 1;
  }
}

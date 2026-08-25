import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { WEB_CONTRACT_ERROR_CODES, WebContractError } from "../contracts/web-contract-harness.mjs";
import { WEB_STATIC_CONTRACTS, parseLegacyPesterTests } from "../contracts/web-static-contracts.mjs";

const inventory = Object.freeze({ total: 62, packaging: 55, installer: 6, web: 1 });
const CANONICAL_LEDGER = Object.freeze([["packaging","packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",7],["packaging","packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",11],["packaging","packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1",90],["packaging","packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",10],["packaging","packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",28],["packaging","packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",8],["packaging","packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1",2],["packaging","packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",5],["packaging","packaging/windows-desktop-node/tests/PcvCSharpArchitectureGapRegistry.Tests.ps1",10],["packaging","packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",12],["packaging","packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",16],["packaging","packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1",61],["packaging","packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",16],["packaging","packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",26],["packaging","packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1",1],["packaging","packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",9],["packaging","packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",20],["packaging","packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",7],["packaging","packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1",1],["packaging","packaging/windows-desktop-node/tests/PcvInstalledLoopbackBootstrapSmoke.Tests.ps1",1],["packaging","packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1",1],["packaging","packaging/windows-desktop-node/tests/PcvInternalHttpsTlsLifecycleSmoke.Tests.ps1",1],["packaging","packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",5],["packaging","packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",5],["packaging","packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",10],["packaging","packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",21],["packaging","packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",5],["packaging","packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",7],["packaging","packaging/windows-desktop-node/tests/PcvRunnerArtifactRootContract.Tests.ps1",3],["packaging","packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",5],["packaging","packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvStrictCollection.Tests.ps1",2],["packaging","packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",8],["packaging","packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1",4],["packaging","packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1",4],["packaging","packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1",4],["packaging","packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1",2],["packaging","packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",6],["packaging","packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",7],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",4],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",5],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1",21],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1",6],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1",10],["installer","packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1",3],["web","web/tests/PcvDesktopWeb.Static.Tests.ps1",50]].map((row) => Object.freeze(row)));
function deepFreeze(value) {
  if (value !== null && typeof value === "object") {
    for (const child of Object.values(value)) deepFreeze(child);
    Object.freeze(value);
  }
  return value;
}
function sameJson(left, right) {
  if (Object.is(left, right)) return true;
  if (Array.isArray(left) || Array.isArray(right)) return Array.isArray(left) && Array.isArray(right) && left.length === right.length && left.every((value, index) => sameJson(value, right[index]));
  if (left === null || right === null || typeof left !== "object" || typeof right !== "object") return false;
  const leftKeys = Object.keys(left).sort();
  const rightKeys = Object.keys(right).sort();
  return leftKeys.length === rightKeys.length && leftKeys.every((key, index) => key === rightKeys[index] && sameJson(left[key], right[key]));
}
const APPROVED_SCHEMA = deepFreeze(JSON.parse("{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"$id\":\"pcv-development-verification-migration-manifest-schema-v1\",\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"contract\",\"schema_version\",\"inventory\",\"entries\"],\"properties\":{\"contract\":{\"const\":\"pcv-development-verification-migration-manifest-v1\"},\"schema_version\":{\"const\":1},\"inventory\":{\"$ref\":\"#/$defs/inventory\"},\"entries\":{\"type\":\"array\",\"minItems\":62,\"maxItems\":62,\"items\":{\"$ref\":\"#/$defs/entry\"}}},\"$defs\":{\"inventory\":{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"total\",\"packaging\",\"installer\",\"web\"],\"properties\":{\"total\":{\"const\":62},\"packaging\":{\"const\":55},\"installer\":{\"const\":6},\"web\":{\"const\":1}}},\"parity\":{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"status\",\"evidence\"],\"properties\":{\"status\":{\"enum\":[\"pending\",\"pass\",\"fail\"]},\"evidence\":{\"type\":[\"string\",\"null\"]}}},\"entry\":{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"legacy_path\",\"domain\",\"legacy_contract_count\",\"replacement_owner\",\"replacement_contract_ids\",\"parity_status\",\"local_parity\",\"ci_parity\"],\"properties\":{\"legacy_path\":{\"type\":\"string\",\"pattern\":\"^[^\\\\/]+(?:/[^\\\\/]+)*\\\\.Tests\\\\.ps1$\"},\"domain\":{\"enum\":[\"packaging\",\"installer\",\"web\"]},\"legacy_contract_count\":{\"type\":\"integer\",\"minimum\":0},\"replacement_owner\":{\"type\":[\"string\",\"null\"]},\"replacement_contract_ids\":{\"type\":\"array\",\"uniqueItems\":true,\"items\":{\"type\":\"string\",\"pattern\":\"^web\\\\.static\\\\.[a-z0-9]+(?:-[a-z0-9]+)*$\"}},\"parity_status\":{\"enum\":[\"unmapped\",\"mapped\",\"dual-run-pass\",\"cutover\"]},\"local_parity\":{\"$ref\":\"#/$defs/parity\"},\"ci_parity\":{\"$ref\":\"#/$defs/parity\"}}}}}"));
const entryKeys = ["legacy_path", "domain", "legacy_contract_count", "replacement_owner", "replacement_contract_ids", "parity_status", "local_parity", "ci_parity"];
const parityKeys = ["status", "evidence"];
export const DISCOVERY_ROOTS = Object.freeze([
  Object.freeze({ domain: "packaging", relativeRoot: "packaging/windows-desktop-node/tests" }),
  Object.freeze({ domain: "installer", relativeRoot: "packaging/windows-desktop-node/installer/tests" }),
  Object.freeze({ domain: "web", relativeRoot: "web/tests" })
]);
const fail = (detail) => new WebContractError(WEB_CONTRACT_ERROR_CODES.manifestInvalid, detail);
const object = (value) => value !== null && typeof value === "object" && !Array.isArray(value) && Object.getPrototypeOf(value) === Object.prototype;
const keys = (value, expected) => object(value) && Object.keys(value).length === expected.length && expected.every((key) => Object.hasOwn(value, key));
function relative(value) {
  if (typeof value !== "string" || !value || value.includes("\0")) throw fail("path=invalid");
  const normalized = value.replaceAll("\\", "/");
  if (normalized.startsWith("/") || /^[A-Za-z]:/.test(normalized) || normalized.split("/").includes("..")) throw fail("path=escape");
  return normalized;
}
function rootOf(repoRoot) {
  try { const root = fs.realpathSync(path.resolve(repoRoot)); if (!fs.statSync(root).isDirectory()) throw new Error(); return root; } catch { throw fail("repo_root=invalid"); }
}
function contained(root, value) {
  const normalized = relative(value); const candidate = path.resolve(root, ...normalized.split("/"));
  if (!(candidate === root || candidate.startsWith(root + path.sep))) throw fail("path=escape");
  try { if (fs.lstatSync(candidate).isSymbolicLink() || !fs.realpathSync(candidate).startsWith(root + path.sep)) throw new Error(); return { candidate, normalized }; } catch { throw fail("path=unsafe"); }
}
function schemaValid(schema) {
  if (!sameJson(schema, APPROVED_SCHEMA)) return false;
  const strictObject = (value, required) => object(value) && value.type === "object" && value.additionalProperties === false && Array.isArray(value.required) && same(value.required, required) && keys(value.properties, required);
  const inventorySchema = schema?.$defs?.inventory;
  const paritySchema = schema?.$defs?.parity;
  const entrySchema = schema?.$defs?.entry;
  return object(schema) && schema.$schema === "https://json-schema.org/draft/2020-12/schema"
    && schema.$id === "pcv-development-verification-migration-manifest-schema-v1"
    && strictObject(schema, ["contract", "schema_version", "inventory", "entries"])
    && schema.properties.contract?.const === "pcv-development-verification-migration-manifest-v1"
    && schema.properties.schema_version?.const === 1
    && schema.properties.inventory?.$ref === "#/$defs/inventory"
    && schema.properties.entries?.type === "array" && schema.properties.entries?.minItems === 62
    && schema.properties.entries?.maxItems === 62 && schema.properties.entries?.items?.$ref === "#/$defs/entry"
    && strictObject(inventorySchema, ["total", "packaging", "installer", "web"])
    && inventorySchema.properties?.total?.const === 62 && inventorySchema.properties?.packaging?.const === 55
    && inventorySchema.properties?.installer?.const === 6 && inventorySchema.properties?.web?.const === 1
    && strictObject(paritySchema, parityKeys) && same(paritySchema.properties.status?.enum ?? [], ["pending", "pass", "fail"])
    && Array.isArray(paritySchema.properties.evidence?.type) && same(paritySchema.properties.evidence.type, ["string", "null"])
    && strictObject(entrySchema, entryKeys) && same(entrySchema.properties.domain?.enum ?? [], ["packaging", "installer", "web"])
    && entrySchema.properties.legacy_path?.type === "string"
    && entrySchema.properties.legacy_path?.pattern === "^[^\\/]+(?:/[^\\/]+)*\\.Tests\\.ps1$"
    && entrySchema.properties.legacy_contract_count?.type === "integer" && entrySchema.properties.legacy_contract_count?.minimum === 0
    && Array.isArray(entrySchema.properties.replacement_owner?.type) && same(entrySchema.properties.replacement_owner.type, ["string", "null"])
    && entrySchema.properties.replacement_contract_ids?.type === "array" && entrySchema.properties.replacement_contract_ids?.uniqueItems === true
    && entrySchema.properties.replacement_contract_ids?.items?.type === "string"
    && entrySchema.properties.replacement_contract_ids?.items?.pattern === "^web\\.static\\.[a-z0-9]+(?:-[a-z0-9]+)*$"
    && same(entrySchema.properties.parity_status?.enum ?? [], ["unmapped", "mapped", "dual-run-pass", "cutover"])
    && entrySchema.properties.local_parity?.$ref === "#/$defs/parity" && entrySchema.properties.ci_parity?.$ref === "#/$defs/parity";
}
function parity(value, root) {
  if (!keys(value, parityKeys) || !["pending", "pass", "fail"].includes(value.status) || !(typeof value.evidence === "string" || value.evidence === null)) throw fail("parity=shape");
  if (value.status === "pending" && value.evidence !== null) throw fail("parity=pending-evidence");
  if (value.status !== "pending") {
    if (typeof value.evidence !== "string" || !value.evidence) throw fail("parity=evidence");
    const evidence = contained(root, value.evidence);
    if (!fs.statSync(evidence.candidate).isFile()) throw fail("parity=evidence-file");
  }
}
export function discoverLegacyPesterInventory(repoRoot, roots = DISCOVERY_ROOTS) {
  const root = rootOf(repoRoot); if (!Array.isArray(roots) || !roots.length) throw fail("discovery_roots=invalid");
  const rows = [];
  for (const item of roots) {
    if (!object(item) || typeof item.domain !== "string" || typeof item.relativeRoot !== "string") throw fail("discovery_root=invalid");
    const directory = contained(root, item.relativeRoot);
    let children; try { children = fs.readdirSync(directory.candidate, { withFileTypes: true }); } catch { throw fail("discovery=unreadable"); }
    for (const child of children) {
      if (!child.name.toLowerCase().endsWith(".tests.ps1")) continue;
      const file = contained(root, path.posix.join(directory.normalized, child.name));
      let source; try { if (!fs.lstatSync(file.candidate).isFile()) throw new Error(); source = fs.readFileSync(file.candidate, "utf8"); } catch { throw fail("discovery=file"); }
      rows.push({ domain: item.domain, legacy_path: file.normalized, legacy_contract_count: (source.match(/^\s*It\b/gm) ?? []).length });
    }
  }
  const order = new Map(DISCOVERY_ROOTS.map((item, index) => [item.domain, index]));
  return rows.sort((a, b) => (order.get(a.domain) ?? 99) - (order.get(b.domain) ?? 99) || (a.legacy_path < b.legacy_path ? -1 : a.legacy_path > b.legacy_path ? 1 : 0));
}
const same = (a, b) => a.length === b.length && a.every((value, index) => value === b[index]);
const sameLedger = (a, b) => a.length === b.length && a.every((value, index) => Array.isArray(value) && Array.isArray(b[index]) && same(value, b[index]));
export function validateMigrationManifest({ manifest, schema, repoRoot, requireWebLocalPass = false } = {}) {
  const root = rootOf(repoRoot);
  if (!schemaValid(schema)) throw fail("schema=invalid");
  if (!keys(manifest, ["contract", "schema_version", "inventory", "entries"]) || manifest.contract !== "pcv-development-verification-migration-manifest-v1" || manifest.schema_version !== 1 || !keys(manifest.inventory, ["total", "packaging", "installer", "web"]) || !Object.entries(inventory).every(([key, value]) => manifest.inventory[key] === value) || !Array.isArray(manifest.entries) || manifest.entries.length !== 62 || typeof requireWebLocalPass !== "boolean") throw fail("manifest=shape");
  const seen = new Set();
  for (const entry of manifest.entries) {
    if (!keys(entry, entryKeys) || typeof entry.legacy_path !== "string" || !["packaging", "installer", "web"].includes(entry.domain) || !Number.isInteger(entry.legacy_contract_count) || entry.legacy_contract_count < 0 || !(typeof entry.replacement_owner === "string" || entry.replacement_owner === null) || !Array.isArray(entry.replacement_contract_ids) || !entry.replacement_contract_ids.every((id) => typeof id === "string" && /^web\.static\.[a-z0-9]+(?:-[a-z0-9]+)*$/.test(id)) || new Set(entry.replacement_contract_ids).size !== entry.replacement_contract_ids.length || !["unmapped", "mapped", "dual-run-pass", "cutover"].includes(entry.parity_status)) throw fail("entry=shape");
    relative(entry.legacy_path); parity(entry.local_parity, root); parity(entry.ci_parity, root);
    if (seen.has(entry.legacy_path)) throw fail("entries=duplicate"); seen.add(entry.legacy_path);
  }
  if (!sameLedger(manifest.entries.map((entry) => [entry.domain, entry.legacy_path, entry.legacy_contract_count]), CANONICAL_LEDGER)) throw fail("entries=canonical-ledger");
  const actual = discoverLegacyPesterInventory(root); const actualByPath = new Map(actual.map((row) => [row.legacy_path, row]));
  if (actual.length !== 62 || seen.size !== actualByPath.size || [...actualByPath].some(([name, row]) => { const entry = manifest.entries.find((item) => item.legacy_path === name); return !entry || entry.domain !== row.domain || entry.legacy_contract_count !== row.legacy_contract_count; })) throw fail("entries=inventory-mismatch");
  const web = manifest.entries.filter((entry) => entry.domain === "web");
  if (web.length !== 1) throw fail("web=count");
  const entry = web[0], expectedIds = WEB_STATIC_CONTRACTS.map((item) => item.id), source = fs.readFileSync(path.join(root, ...entry.legacy_path.split("/")), "utf8");
  let legacy;
  try {
    legacy = parseLegacyPesterTests(source);
  } catch {
    throw fail("web=legacy-parse");
  }
  if (legacy.length !== 50 || !same(legacy.map((item) => item.name), WEB_STATIC_CONTRACTS.map((item) => item.legacyName)) || entry.legacy_contract_count !== 50 || entry.parity_status !== "mapped" || entry.replacement_owner !== "web/node-tests/web-static-contracts.test.mjs" || !same(entry.replacement_contract_ids, expectedIds) || entry.ci_parity.status !== "pending" || entry.ci_parity.evidence !== null) throw fail("web=mapping");
  for (const row of manifest.entries) if (row.domain !== "web" && (row.parity_status !== "unmapped" || row.replacement_owner !== null || row.replacement_contract_ids.length || row.local_parity.status !== "pending" || row.local_parity.evidence !== null || row.ci_parity.status !== "pending" || row.ci_parity.evidence !== null)) throw fail("non_web=state");
  if (requireWebLocalPass && (entry.local_parity.status !== "pass" || typeof entry.local_parity.evidence !== "string")) throw fail("web=local-pass-required");
  return { summary: { ...inventory, missing: 0, duplicate: 0, web_status: entry.parity_status, web_local: entry.local_parity.status, web_ci: entry.ci_parity.status } };
}
function main() {
  const args = process.argv.slice(2); if (!args.every((arg) => arg === "--require-web-local-pass") || new Set(args).size !== args.length) throw fail("args=invalid");
  const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");
  const schema = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-migration-manifest.schema.json"), "utf8"));
  const manifest = JSON.parse(fs.readFileSync(path.join(repoRoot, "config/development-verification-migration-manifest.json"), "utf8"));
  const { summary } = validateMigrationManifest({ manifest, schema, repoRoot, requireWebLocalPass: args.includes("--require-web-local-pass") });
  console.log(`Verification migration manifest PASS: total=${summary.total} packaging=${summary.packaging} installer=${summary.installer} web=${summary.web} missing=${summary.missing} duplicate=${summary.duplicate} web_status=${summary.web_status} web_local=${summary.web_local} web_ci=${summary.web_ci}`);
}
if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) { try { main(); } catch (error) { console.error(error instanceof WebContractError ? error.message : `${WEB_CONTRACT_ERROR_CODES.manifestInvalid}|unexpected`); process.exitCode = 1; } }

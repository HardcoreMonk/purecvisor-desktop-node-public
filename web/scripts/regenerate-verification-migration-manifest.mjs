import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { WEB_CONTRACT_ERROR_CODES, WebContractError } from "../contracts/web-contract-harness.mjs";
import { WEB_STATIC_CONTRACTS } from "../contracts/web-static-contracts.mjs";

export const DISCOVERY_ROOTS = Object.freeze([
  Object.freeze({ domain: "packaging", relativeRoot: "packaging/windows-desktop-node/tests" }),
  Object.freeze({ domain: "installer", relativeRoot: "packaging/windows-desktop-node/installer/tests" }),
  Object.freeze({ domain: "web", relativeRoot: "web/tests" })
]);

const EXPECTED = Object.freeze({
  files: Object.freeze({ total: 62, packaging: 55, installer: 6, web: 1 }),
  contracts: Object.freeze({ total: 627, packaging: 528, installer: 49, web: 50 })
});
const WEB_PATH = "web/tests/PcvDesktopWeb.Static.Tests.ps1";
const WEB_OWNER = "web/node-tests/web-static-contracts.test.mjs";
const fail = (detail) => new WebContractError(WEB_CONTRACT_ERROR_CODES.manifestInvalid, detail);

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
  if (typeof value !== "string" || !value || value.includes("\0") || value.includes("\\")) {
    throw fail("path=invalid");
  }
  if (value.startsWith("/") || /^[A-Za-z]:/.test(value) || value.split("/").some((part) => !part || part === "." || part === "..")) {
    throw fail("path=escape");
  }
  return value;
}

function contained(root, value, expectedType = "file") {
  const normalized = relative(value);
  const candidate = path.resolve(root, ...normalized.split("/"));
  if (!candidate.startsWith(`${root}${path.sep}`)) throw fail("path=escape");
  try {
    const stat = fs.lstatSync(candidate);
    if (stat.isSymbolicLink()) throw new Error("symlink");
    if (expectedType === "file" && !stat.isFile()) throw new Error("not-file");
    if (expectedType === "directory" && !stat.isDirectory()) throw new Error("not-directory");
    const real = fs.realpathSync(candidate);
    if (!real.startsWith(`${root}${path.sep}`)) throw new Error("escape");
    return { candidate, normalized };
  } catch {
    throw fail("path=unsafe");
  }
}

function invalidParse(detail) {
  throw fail(detail);
}

function decodeBacktick(value) {
  return ({ "0": "\0", a: "\x07", b: "\b", e: "\x1b", f: "\f", n: "\n", r: "\r", t: "\t", v: "\v" })[value] ?? value;
}

function parseNameLiteral(line, cursor, rejectInterpolation) {
  const quote = line[cursor.index++];
  let value = "";
  while (cursor.index < line.length) {
    const current = line[cursor.index++];
    if (quote === "'" && current === "'") {
      if (line[cursor.index] === "'") {
        value += "'";
        cursor.index++;
        continue;
      }
      return value;
    }
    if (quote === '"' && current === "`") {
      if (cursor.index >= line.length) invalidParse(rejectInterpolation ? "multiline-declaration" : "unmatched-quote");
      value += decodeBacktick(line[cursor.index++]);
      continue;
    }
    if (quote === '"' && current === '"') return value;
    if (quote === '"' && current === "$" && rejectInterpolation) invalidParse("dynamic-name");
    value += current;
  }
  invalidParse("unmatched-quote");
}

function skipSourceString(line, cursor, quote, openingConsumed) {
  if (!openingConsumed) cursor.index++;
  while (cursor.index < line.length) {
    const current = line[cursor.index++];
    if (quote === "'" && current === "'") {
      if (line[cursor.index] === "'") {
        cursor.index++;
        continue;
      }
      return true;
    }
    if (quote === '"' && current === "`") {
      if (cursor.index < line.length) cursor.index++;
      continue;
    }
    if (quote === '"' && current === '"') return true;
  }
  return false;
}

function hereStringStart(line, index) {
  if (line[index] !== "@" || !["'", '"'].includes(line[index + 1])) return null;
  if ([...line.slice(index + 2)].some((value) => !/\s/u.test(value))) return null;
  return line[index + 1] === "'" ? "single-here" : "double-here";
}

function tokenStart(value) {
  return value === "_" || /[A-Za-z]/u.test(value);
}

function tokenPart(value) {
  return value === "_" || value === "-" || /[A-Za-z0-9]/u.test(value);
}

export function parseLegacyPesterContracts(source) {
  if (typeof source !== "string") throw fail("source=invalid");
  const lines = source.replaceAll("\r\n", "\n").replaceAll("\r", "\n").split("\n");
  const contracts = [];
  const names = new Set();
  let state = "none";

  for (const line of lines) {
    if (state === "single-here" || state === "double-here") {
      const terminator = state === "single-here" ? "'@" : '"@';
      const candidate = line.trimStart();
      const next = candidate[terminator.length];
      if (candidate.startsWith(terminator) && (candidate.length === terminator.length || /\s/u.test(next) || ["|", ";", ")", ","].includes(next))) {
        state = "none";
      }
      continue;
    }

    const cursor = { index: 0 };
    let statementPosition = true;
    if (state === "single-string" || state === "double-string") {
      const quote = state === "single-string" ? "'" : '"';
      statementPosition = false;
      if (!skipSourceString(line, cursor, quote, true)) continue;
      state = "none";
    }

    while (cursor.index < line.length) {
      if (state === "block-comment") {
        const end = line.indexOf("#>", cursor.index);
        if (end < 0) break;
        cursor.index = end + 2;
        state = "none";
        continue;
      }
      const current = line[cursor.index];
      if (/\s/u.test(current)) {
        cursor.index++;
        continue;
      }
      if (current === "#") break;
      if (current === "<" && line[cursor.index + 1] === "#") {
        state = "block-comment";
        cursor.index += 2;
        continue;
      }
      const here = hereStringStart(line, cursor.index);
      if (here) {
        state = here;
        break;
      }
      if (current === "'" || current === '"') {
        if (!skipSourceString(line, cursor, current, false)) {
          state = current === "'" ? "single-string" : "double-string";
          break;
        }
        statementPosition = false;
        continue;
      }
      if (["{", "}", ";"].includes(current)) {
        statementPosition = true;
        cursor.index++;
        continue;
      }
      if (tokenStart(current)) {
        const start = cursor.index++;
        while (cursor.index < line.length && tokenPart(line[cursor.index])) cursor.index++;
        const token = line.slice(start, cursor.index);
        if (statementPosition && token.toLowerCase() === "it") {
          while (cursor.index < line.length && /\s/u.test(line[cursor.index])) cursor.index++;
          if (cursor.index >= line.length || line[cursor.index] === "`") invalidParse("multiline-declaration");
          if (!["'", '"'].includes(line[cursor.index])) invalidParse("dynamic-name");
          const legacyName = parseNameLiteral(line, cursor, true);
          if (names.has(legacyName)) invalidParse("duplicate-name");
          names.add(legacyName);
          contracts.push({ legacyOrdinal: contracts.length + 1, legacyName });
        }
        statementPosition = false;
        continue;
      }
      statementPosition = false;
      cursor.index++;
    }
  }

  if (state === "block-comment") invalidParse("unmatched-comment");
  if (state === "single-here" || state === "double-here") invalidParse("unmatched-here-string");
  if (state === "single-string" || state === "double-string") invalidParse("unmatched-quote");
  return contracts;
}

export function discoverLegacyContractInventory(repoRoot) {
  const root = rootOf(repoRoot);
  const entries = [];
  const contracts = [];
  for (const discovery of DISCOVERY_ROOTS) {
    const directory = contained(root, discovery.relativeRoot, "directory");
    const children = fs.readdirSync(directory.candidate, { withFileTypes: true })
      .filter((item) => item.name.toLowerCase().endsWith(".tests.ps1"))
      .sort((left, right) => left.name < right.name ? -1 : left.name > right.name ? 1 : 0);
    for (const child of children) {
      if (!child.isFile() || child.isSymbolicLink()) throw fail("discovery=file");
      const legacyPath = `${directory.normalized}/${child.name}`;
      const source = fs.readFileSync(contained(root, legacyPath).candidate, "utf8");
      const parsed = parseLegacyPesterContracts(source);
      entries.push({ legacy_path: legacyPath, domain: discovery.domain, legacy_contract_count: parsed.length });
      for (const item of parsed) {
        contracts.push({
          legacy_path: legacyPath,
          legacy_ordinal: item.legacyOrdinal,
          legacy_name: item.legacyName,
          domain: discovery.domain
        });
      }
    }
  }
  const fileCounts = Object.fromEntries(["packaging", "installer", "web"].map((domain) => [domain, entries.filter((row) => row.domain === domain).length]));
  const contractCounts = Object.fromEntries(["packaging", "installer", "web"].map((domain) => [domain, contracts.filter((row) => row.domain === domain).length]));
  if (entries.length !== EXPECTED.files.total || contracts.length !== EXPECTED.contracts.total ||
      Object.entries(fileCounts).some(([domain, count]) => count !== EXPECTED.files[domain]) ||
      Object.entries(contractCounts).some(([domain, count]) => count !== EXPECTED.contracts[domain])) {
    throw fail("inventory=count");
  }
  return { entries, contracts };
}

function decodeCSharpString(value) {
  let output = "";
  for (let index = 0; index < value.length; index++) {
    const current = value[index];
    if (current !== "\\") {
      output += current;
      continue;
    }
    const escape = value[++index];
    if (escape === undefined) throw fail("replacement=string");
    const simple = { "0": "\0", a: "\x07", b: "\b", e: "\x1b", f: "\f", n: "\n", r: "\r", t: "\t", v: "\v", "\\": "\\", '"': '"', "'": "'" };
    if (Object.hasOwn(simple, escape)) {
      output += simple[escape];
      continue;
    }
    if (escape === "u" || escape === "U") {
      const length = escape === "u" ? 4 : 8;
      const digits = value.slice(index + 1, index + 1 + length);
      if (!new RegExp(`^[0-9A-Fa-f]{${length}}$`, "u").test(digits)) throw fail("replacement=string");
      output += String.fromCodePoint(Number.parseInt(digits, 16));
      index += length;
      continue;
    }
    throw fail("replacement=string");
  }
  return output;
}

function enumerateCSharpFiles(root, relativeDirectory) {
  const absolute = path.join(root, ...relativeDirectory.split("/"));
  if (!fs.existsSync(absolute)) return [];
  const rows = [];
  const visit = (directory, relativeBase) => {
    for (const child of fs.readdirSync(directory, { withFileTypes: true }).sort((a, b) => a.name.localeCompare(b.name, "en"))) {
      if (child.isSymbolicLink()) throw fail("replacement=symlink");
      const childRelative = `${relativeBase}/${child.name}`;
      const childAbsolute = path.join(directory, child.name);
      if (child.isDirectory()) visit(childAbsolute, childRelative);
      else if (child.isFile() && child.name.endsWith(".cs")) rows.push(childRelative);
    }
  };
  visit(absolute, relativeDirectory);
  return rows;
}

export function discoverReplacementContractInventory(repoRoot) {
  const root = rootOf(repoRoot);
  const replacements = WEB_STATIC_CONTRACTS.map((item, index) => ({
    legacyPath: WEB_PATH,
    legacyOrdinal: index + 1,
    legacyName: item.legacyName,
    replacementOwner: WEB_OWNER,
    replacementContractId: item.id,
    domain: "web"
  }));
  const attributePattern = /\[\s*PcvLegacyContract(?:Attribute)?\s*\(\s*"((?:\\.|[^"\\])*)"\s*,\s*"((?:\\.|[^"\\])*)"\s*,\s*(\d+)\s*,\s*"((?:\\.|[^"\\])*)"\s*\)\s*\]/gu;
  for (const owner of [
    ...enumerateCSharpFiles(root, "src/DesktopNode.Delivery.Tests/Installer"),
    ...enumerateCSharpFiles(root, "src/DesktopNode.Delivery.Tests/Delivery")
  ]) {
    const source = fs.readFileSync(contained(root, owner).candidate, "utf8");
    const markerCount = (source.match(/\[\s*PcvLegacyContract(?:Attribute)?\s*\(/gu) ?? []).length;
    let match;
    let parsedCount = 0;
    while ((match = attributePattern.exec(source)) !== null) {
      parsedCount++;
      const replacementContractId = decodeCSharpString(match[1]);
      const legacyPath = decodeCSharpString(match[2]);
      const legacyOrdinal = Number.parseInt(match[3], 10);
      const legacyName = decodeCSharpString(match[4]);
      const domain = replacementContractId.startsWith("pcv.installer.") ? "installer"
        : replacementContractId.startsWith("pcv.delivery.") ? "packaging" : null;
      if (!domain) throw fail("replacement=id");
      replacements.push({ legacyPath, legacyOrdinal, legacyName, replacementOwner: owner, replacementContractId, domain });
    }
    if (markerCount !== parsedCount) throw fail("replacement=attribute-parse");
  }
  const ids = new Set();
  const keys = new Set();
  for (const row of replacements) {
    const key = `${row.legacyPath}\0${row.legacyOrdinal}`;
    if (ids.has(row.replacementContractId)) throw fail("replacement=duplicate-id");
    if (keys.has(key)) throw fail("replacement=duplicate-key");
    ids.add(row.replacementContractId);
    keys.add(key);
  }
  return replacements;
}

function pending() {
  return { status: "pending", evidence: null };
}

function priorFile(previous, legacyPath) {
  return previous?.entries?.find((row) => row.legacy_path === legacyPath) ?? null;
}

function priorContract(previous, legacyPath, legacyOrdinal) {
  return previous?.schema_version === 2
    ? previous.contracts?.find((row) => row.legacy_path === legacyPath && row.legacy_ordinal === legacyOrdinal) ?? null
    : null;
}

function preserveParity(value) {
  return value && typeof value === "object"
    ? { status: value.status, evidence: value.evidence }
    : pending();
}

export function buildMigrationManifest({ repoRoot, previousManifest = null } = {}) {
  const legacy = discoverLegacyContractInventory(repoRoot);
  const replacements = discoverReplacementContractInventory(repoRoot);
  const replacementByKey = new Map(replacements.map((row) => [`${row.legacyPath}\0${row.legacyOrdinal}`, row]));
  const contracts = legacy.contracts.map((row) => {
    const replacement = replacementByKey.get(`${row.legacy_path}\0${row.legacy_ordinal}`) ?? null;
    if (replacement && (replacement.legacyName !== row.legacy_name || replacement.domain !== row.domain)) {
      throw fail("replacement=legacy-mismatch");
    }
    const prior = priorContract(previousManifest, row.legacy_path, row.legacy_ordinal);
    const v1Web = previousManifest?.schema_version === 1 && row.domain === "web" ? priorFile(previousManifest, row.legacy_path) : null;
    const sameReplacement = prior && replacement && prior.replacement_owner === replacement.replacementOwner && prior.replacement_contract_id === replacement.replacementContractId;
    const parityStatus = replacement ? (sameReplacement ? prior.parity_status : v1Web?.parity_status ?? "mapped") : "unmapped";
    return {
      legacy_path: row.legacy_path,
      legacy_ordinal: row.legacy_ordinal,
      legacy_name: row.legacy_name,
      domain: row.domain,
      replacement_owner: replacement?.replacementOwner ?? null,
      replacement_contract_id: replacement?.replacementContractId ?? null,
      parity_status: parityStatus,
      local_parity: replacement ? preserveParity(sameReplacement ? prior.local_parity : v1Web?.local_parity) : pending(),
      ci_parity: replacement ? preserveParity(sameReplacement ? prior.ci_parity : v1Web?.ci_parity) : pending()
    };
  });
  if (replacementByKey.size !== contracts.filter((row) => row.replacement_contract_id !== null).length) {
    throw fail("replacement=orphan");
  }
  const entries = legacy.entries.map((entry) => {
    const children = contracts.filter((row) => row.legacy_path === entry.legacy_path);
    const mapped = children.filter((row) => row.replacement_contract_id !== null).length;
    if (mapped !== 0 && mapped !== children.length) throw fail("replacement=partial-file");
    if (mapped !== 0) {
      const first = children[0];
      const sameState = children.every((row) =>
        row.parity_status === first.parity_status &&
        JSON.stringify(row.local_parity) === JSON.stringify(first.local_parity) &&
        JSON.stringify(row.ci_parity) === JSON.stringify(first.ci_parity));
      if (!sameState) throw fail("replacement=file-state");
    }
    const projection = mapped === 0 ? null : children[0];
    return {
      legacy_path: entry.legacy_path,
      domain: entry.domain,
      legacy_contract_count: entry.legacy_contract_count,
      parity_status: projection?.parity_status ?? "unmapped",
      local_parity: projection ? preserveParity(projection.local_parity) : pending(),
      ci_parity: projection ? preserveParity(projection.ci_parity) : pending()
    };
  });
  return {
    contract: "pcv-development-verification-migration-manifest-v2",
    schema_version: 2,
    inventory: {
      files: { ...EXPECTED.files },
      contracts: { ...EXPECTED.contracts }
    },
    entries,
    contracts
  };
}

export function buildMigrationManifestSchema() {
  const parity = {
    type: "object",
    additionalProperties: false,
    required: ["status", "evidence"],
    properties: {
      status: { enum: ["pending", "pass", "fail"] },
      evidence: { type: ["string", "null"] }
    }
  };
  const fileInventory = {
    type: "object",
    additionalProperties: false,
    required: ["total", "packaging", "installer", "web"],
    properties: {
      total: { const: 62 }, packaging: { const: 55 }, installer: { const: 6 }, web: { const: 1 }
    }
  };
  const contractInventory = {
    type: "object",
    additionalProperties: false,
    required: ["total", "packaging", "installer", "web"],
    properties: {
      total: { const: 627 }, packaging: { const: 528 }, installer: { const: 49 }, web: { const: 50 }
    }
  };
  const pathPattern = "^[^\\\\/]+(?:/[^\\\\/]+)*\\.Tests\\.ps1$";
  const ownerPattern = "^[^\\\\/]+(?:/[^\\\\/]+)*$";
  const idPattern = "^(?:web\\.static\\.[a-z0-9]+(?:-[a-z0-9]+)*|pcv\\.(?:installer|delivery)\\.[a-z0-9]+(?:-[a-z0-9]+)*\\.[0-9]{3})$";
  return {
    $schema: "https://json-schema.org/draft/2020-12/schema",
    $id: "pcv-development-verification-migration-manifest-schema-v2",
    type: "object",
    additionalProperties: false,
    required: ["contract", "schema_version", "inventory", "entries", "contracts"],
    properties: {
      contract: { const: "pcv-development-verification-migration-manifest-v2" },
      schema_version: { const: 2 },
      inventory: { $ref: "#/$defs/inventory" },
      entries: { type: "array", minItems: 62, maxItems: 62, items: { $ref: "#/$defs/entry" } },
      contracts: { type: "array", minItems: 627, maxItems: 627, items: { $ref: "#/$defs/contract" } }
    },
    $defs: {
      fileInventory,
      contractInventory,
      inventory: {
        type: "object",
        additionalProperties: false,
        required: ["files", "contracts"],
        properties: {
          files: { $ref: "#/$defs/fileInventory" },
          contracts: { $ref: "#/$defs/contractInventory" }
        }
      },
      parity,
      entry: {
        type: "object",
        additionalProperties: false,
        required: ["legacy_path", "domain", "legacy_contract_count", "parity_status", "local_parity", "ci_parity"],
        properties: {
          legacy_path: { type: "string", pattern: pathPattern },
          domain: { enum: ["packaging", "installer", "web"] },
          legacy_contract_count: { type: "integer", minimum: 0 },
          parity_status: { enum: ["unmapped", "mapped", "dual-run-pass", "cutover"] },
          local_parity: { $ref: "#/$defs/parity" },
          ci_parity: { $ref: "#/$defs/parity" }
        }
      },
      contract: {
        type: "object",
        additionalProperties: false,
        required: ["legacy_path", "legacy_ordinal", "legacy_name", "domain", "replacement_owner", "replacement_contract_id", "parity_status", "local_parity", "ci_parity"],
        properties: {
          legacy_path: { type: "string", pattern: pathPattern },
          legacy_ordinal: { type: "integer", minimum: 1 },
          legacy_name: { type: "string", minLength: 1 },
          domain: { enum: ["packaging", "installer", "web"] },
          replacement_owner: { type: ["string", "null"], pattern: ownerPattern },
          replacement_contract_id: { type: ["string", "null"], pattern: idPattern },
          parity_status: { enum: ["unmapped", "mapped", "dual-run-pass", "cutover"] },
          local_parity: { $ref: "#/$defs/parity" },
          ci_parity: { $ref: "#/$defs/parity" }
        }
      }
    }
  };
}

export function canonicalManifestJson(manifest) {
  return `${JSON.stringify(manifest, null, 2)}\n`;
}

function promoteLocal(manifest, { paths, domains, evidence }) {
  if (!paths.length && !domains.length) return;
  if (typeof evidence !== "string" || !evidence) throw fail("promotion=evidence");
  relative(evidence);
  const selectedEntries = manifest.entries.filter((entry) => paths.includes(entry.legacy_path) || domains.includes(entry.domain));
  if (!selectedEntries.length) throw fail("promotion=selection");
  for (const entry of selectedEntries) {
    if (entry.parity_status !== "mapped") throw fail("promotion=state");
    entry.local_parity = { status: "pass", evidence };
    for (const contract of manifest.contracts.filter((row) => row.legacy_path === entry.legacy_path)) {
      if (contract.parity_status !== "mapped") throw fail("promotion=state");
      contract.local_parity = { status: "pass", evidence };
    }
  }
}

function parseArguments(args) {
  const options = { mode: null, paths: [], domains: [], evidence: null };
  for (let index = 0; index < args.length; index++) {
    const argument = args[index];
    if (argument === "--write" || argument === "--check") {
      if (options.mode) throw fail("arguments=mode");
      options.mode = argument.slice(2);
    } else if (argument === "--promote-local-path") {
      options.paths.push(args[++index] ?? "");
    } else if (argument === "--promote-local-domain") {
      options.domains.push(args[++index] ?? "");
    } else if (argument === "--evidence") {
      options.evidence = args[++index] ?? "";
    } else {
      throw fail("arguments=unknown");
    }
  }
  if (!options.mode || options.domains.some((value) => !["packaging", "installer", "web"].includes(value))) throw fail("arguments=invalid");
  if ((options.paths.length || options.domains.length) && options.mode !== "write") throw fail("arguments=promotion-mode");
  return options;
}

function runCli() {
  const options = parseArguments(process.argv.slice(2));
  const scriptPath = fileURLToPath(import.meta.url);
  const repoRoot = path.resolve(path.dirname(scriptPath), "../..");
  const manifestPath = path.join(repoRoot, "config/development-verification-migration-manifest.json");
  const schemaPath = path.join(repoRoot, "config/development-verification-migration-manifest.schema.json");
  const previousManifest = fs.existsSync(manifestPath) ? JSON.parse(fs.readFileSync(manifestPath, "utf8")) : null;
  const manifest = buildMigrationManifest({ repoRoot, previousManifest });
  promoteLocal(manifest, options);
  const manifestText = canonicalManifestJson(manifest);
  const schemaText = `${JSON.stringify(buildMigrationManifestSchema(), null, 2)}\n`;
  if (options.mode === "write") {
    fs.writeFileSync(manifestPath, manifestText, "utf8");
    fs.writeFileSync(schemaPath, schemaText, "utf8");
  } else if (fs.readFileSync(manifestPath, "utf8") !== manifestText || fs.readFileSync(schemaPath, "utf8") !== schemaText) {
    throw fail("generated=drift");
  }
  process.stdout.write(`Verification migration manifest ${options.mode === "write" ? "WRITE" : "PASS"}: files=62 contracts=627\n`);
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

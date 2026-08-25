import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError
} from "../contracts/web-contract-harness.mjs";

const MISSING = "<missing>";

function registryMismatch(ordinal, legacyName, replacementId) {
  throw new WebContractError(
    WEB_CONTRACT_ERROR_CODES.registryMismatch,
    `ordinal=${ordinal}|legacy=${legacyName}|replacement=${replacementId}`
  );
}

async function verifyRegistry() {
  let contracts;
  try {
    contracts = await import("../contracts/web-static-contracts.mjs");
  } catch {
    registryMismatch(1, MISSING, MISSING);
  }

  const repositoryRoot = path.resolve(
    path.dirname(fileURLToPath(import.meta.url)),
    "..",
    ".."
  );
  let source;
  try {
    source = fs.readFileSync(
      path.join(repositoryRoot, "web", "tests", "PcvDesktopWeb.Static.Tests.ps1"),
      "utf8"
    );
  } catch {
    registryMismatch(1, MISSING, contracts.WEB_STATIC_CONTRACT_METADATA[0]?.id ?? MISSING);
  }

  let legacy;
  try {
    legacy = contracts.parseLegacyPesterTests(source);
  } catch {
    registryMismatch(1, MISSING, contracts.WEB_STATIC_CONTRACT_METADATA[0]?.id ?? MISSING);
  }

  const replacement = contracts.WEB_STATIC_CONTRACT_METADATA;
  const seenLegacyNames = new Set();
  const seenReplacementIds = new Set();
  const count = Math.max(legacy.length, replacement.length);

  for (let index = 0; index < count; index += 1) {
    const legacyName = legacy[index]?.name ?? MISSING;
    const replacementId = replacement[index]?.id ?? MISSING;
    if (
      legacyName === MISSING
      || replacementId === MISSING
      || replacement[index].legacyName !== legacyName
      || seenLegacyNames.has(legacyName)
      || seenReplacementIds.has(replacementId)
    ) {
      registryMismatch(index + 1, legacyName, replacementId);
    }
    seenLegacyNames.add(legacyName);
    seenReplacementIds.add(replacementId);
  }

  process.stdout.write(
    `Web contract registry PASS: legacy=${legacy.length} replacement=${replacement.length} missing=0 duplicate=0\n`
  );
}

try {
  await verifyRegistry();
} catch (error) {
  const message = error instanceof Error
    && error.code === WEB_CONTRACT_ERROR_CODES.registryMismatch
    ? error.message
    : `${WEB_CONTRACT_ERROR_CODES.registryMismatch}|ordinal=1|legacy=${MISSING}|replacement=${MISSING}`;
  process.stderr.write(`${message}\n`);
  process.exitCode = 1;
}

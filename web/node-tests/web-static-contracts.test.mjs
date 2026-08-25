import fs from "node:fs";
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { fileURLToPath, pathToFileURL } from "node:url";
import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError,
  createWebContractContext
} from "../contracts/web-contract-harness.mjs";
import { WEB_STATIC_CONTRACTS } from "../contracts/web-static-contracts.mjs";

const defaultRepositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");

function fixtureUnsafe(detail) {
  return new WebContractError(WEB_CONTRACT_ERROR_CODES.fixtureUnsafe, detail);
}

function assertFixtureGuardRegression() {
  const invalidRoot = path.join(os.tmpdir(), `pcv-web-contract-invalid-${process.pid}-${Date.now()}`);
  const importTarget = pathToFileURL(fileURLToPath(import.meta.url)).href;
  const child = spawnSync(process.execPath, ["--input-type=module", "--eval", "await import(process.argv[1])", importTarget], {
    encoding: "utf8",
    env: {
      ...process.env,
      PCV_WEB_CONTRACT_FIXTURE_MODE: "negative-parity-v1",
      PCV_WEB_CONTRACT_FIXTURE_ROOT: invalidRoot
    }
  });
  const output = `${child.stdout}${child.stderr}`;
  const normalizedOutput = output.replaceAll("\\", "/").toLowerCase();
  const normalizedInvalidRoot = invalidRoot.replaceAll("\\", "/").toLowerCase();
  assert.notEqual(child.status, 0);
  assert.equal(normalizedOutput.includes(normalizedInvalidRoot), false);
  assert.match(output, new RegExp(`${WEB_CONTRACT_ERROR_CODES.fixtureUnsafe}\\|fixture_root=realpath-failed`));

  const fixtureRoot = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-contract-case-"));
  try {
    fs.writeFileSync(path.join(fixtureRoot, ".pcv-web-contract-negative-v1"), "marker\n", "utf8");
    const alternateCaseRoot = process.platform === "win32"
      ? (() => {
        const drive = /^([A-Za-z]):[\\/]/.exec(fixtureRoot);
        assert.ok(drive, "fixture root must be a drive-qualified child on Windows");
        const alternateDrive = drive[1] === drive[1].toLowerCase()
          ? drive[1].toUpperCase()
          : drive[1].toLowerCase();
        return `${alternateDrive}${fixtureRoot.slice(1)}`;
      })()
      : fixtureRoot;
    if (process.platform === "win32") assert.notEqual(alternateCaseRoot, fixtureRoot);
    const alternateCaseChild = spawnSync(process.execPath, ["--input-type=module", "--eval", "await import(process.argv[1])", importTarget], {
      encoding: "utf8",
      env: {
        ...process.env,
        PCV_WEB_CONTRACT_FIXTURE_MODE: "negative-parity-v1",
        PCV_WEB_CONTRACT_FIXTURE_ROOT: alternateCaseRoot
      }
    });
    const alternateCaseOutput = `${alternateCaseChild.stdout}${alternateCaseChild.stderr}`;
    assert.notEqual(alternateCaseChild.status, 0);
    assert.equal(alternateCaseOutput.includes(WEB_CONTRACT_ERROR_CODES.fixtureUnsafe), false);
    assert.match(alternateCaseOutput, new RegExp(WEB_CONTRACT_ERROR_CODES.configInvalid));
  } finally {
    fs.rmSync(fixtureRoot, { recursive: true, force: true });
  }

  const contractsSource = fs.readFileSync(new URL("../contracts/web-static-contracts.mjs", import.meta.url), "utf8");
  assert.match(contractsSource, /metadataIds\.length !== 50 \|\| verifierIds\.length !== 50/);
}

function resolveRepositoryRoot() {
  const fixtureMode = process.env.PCV_WEB_CONTRACT_FIXTURE_MODE;
  const fixtureRoot = process.env.PCV_WEB_CONTRACT_FIXTURE_ROOT;
  if (fixtureMode === undefined && fixtureRoot === undefined) return defaultRepositoryRoot;
  if (fixtureMode !== "negative-parity-v1" || !fixtureRoot) throw fixtureUnsafe("fixture_config=invalid");
  let temporaryRoot;
  let resolvedFixtureRoot;
  try {
    temporaryRoot = fs.realpathSync(os.tmpdir());
    resolvedFixtureRoot = fs.realpathSync(fixtureRoot);
  } catch {
    throw fixtureUnsafe("fixture_root=realpath-failed");
  }
  const relative = path.relative(temporaryRoot, resolvedFixtureRoot);
  if (!relative || path.isAbsolute(relative) || relative === ".." || relative.startsWith(`..${path.sep}`)) {
    throw fixtureUnsafe("fixture_root=outside-temp");
  }
  try {
    if (!fs.existsSync(path.join(resolvedFixtureRoot, ".pcv-web-contract-negative-v1"))) {
      throw fixtureUnsafe("fixture_marker=missing");
    }
  } catch (error) {
    if (error instanceof WebContractError) throw error;
    throw fixtureUnsafe("fixture_marker=missing");
  }
  return resolvedFixtureRoot;
}

const sharedContext = createWebContractContext({ repoRoot: resolveRepositoryRoot() });
const negativeParityFixture = process.env.PCV_WEB_CONTRACT_FIXTURE_MODE === "negative-parity-v1";

for (const contract of WEB_STATIC_CONTRACTS) {
  test(contract.id, {
    concurrency: false,
    skip: negativeParityFixture && contract.id !== "web.static.root-assets"
  }, async () => {
    if (contract.id === "web.static.javascript-syntax") assertFixtureGuardRegression();
    const scopedContext = sharedContext.forContract(contract.id);
    await scopedContext.runOwners(contract.owners);
    await contract.verify(scopedContext);
  });
}

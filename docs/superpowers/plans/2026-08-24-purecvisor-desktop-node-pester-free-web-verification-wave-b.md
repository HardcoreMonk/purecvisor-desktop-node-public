# Pester-free Web Verification Wave B Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 기존 Web Pester 50개 정적 계약을 같은 순서의 고유 Node contract 50개로 이전하고, 같은 commit의 양성·음성 로컬 parity와 62파일 migration manifest를 증명하되 required CI 전환은 수행하지 않는다.

**Architecture:** `web-contract-harness.mjs`가 repository containment, cached file/JSON access, 정적 assertion과 shell-free owner process를 소유한다. `web-static-contracts.mjs`는 legacy `It` 순서와 일치하는 50개 metadata 및 verifier를 단일 registry로 구성하고 `node:test` projection이 contract ID를 테스트 이름으로 그대로 사용한다. 기존 feature/served/static/browser owner는 argument-array child process로 한 번씩만 실행하며, Pester는 로컬 dual-run과 통제된 음성 fixture 비교에서만 호출한다.

**Tech Stack:** Node.js 24, ECMAScript modules, Node built-in `node:test`/`node:assert`, TypeScript 5.9.3 existing toolchain, JSON Schema draft 2020-12 contract, Pester 5 legacy comparison, C#/.NET 10 unchanged verification baseline.

---

## Global constraints

- Source design: `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-web-verification-wave-b-design.md`
- Written-spec approval: `2026-08-24 user-approved`
- Implementation base: `75b540797d4b9f77457b8549c7bbcedfad739d1e`
- Main integration base: `[private-source-commit]`
- Execute only in the dedicated Wave B worktree. The main worktree's pre-existing user-owned
  documentation changes are not implementation inputs and must not be overwritten. Merge, push,
  stash deletion and main-worktree conflict resolution require a separate completion decision.
- Scope: Wave B Web replacement와 local parity only.
- 기존 `web/tests/PcvDesktopWeb.Static.Tests.ps1`, `.github/workflows/development-gates.yml`,
  `config/development-verification-suites.json`, schema와
  `docs/ga-ready/current-evidence.json`은 수정하지 않는다.
- 기존 `web/package.json`의 `scripts.test` 값은 byte-for-byte 유지한다. 신규 Node 계약은
  별도 `test:web-contracts` command로만 연다.
- required CI shadow/cutover, C# catalog activation, Installer 6파일과 Packaging 55파일의
  replacement는 Wave C~E가 소유한다.
- 새 `.ps1` 파일을 만들지 않는다. `pwsh`는 legacy positive run과 controlled negative parity
  comparison에만 argument-array 방식으로 사용한다.
- MSI, Service/SCM, firewall, Event Log, trust store, Credential Manager, Hyper-V, VM,
  Guest Execution과 package build를 호출하지 않는다.
- repository write는 구현 patch와 문서 patch만 허용한다. 검증 command는 OS temp negative
  fixture 외 repository content를 쓰지 않는다.
- contract/unit test는 skip, conditional omission과 network access를 사용하지 않는다.
- 각 task는 RED → GREEN → commit 순서를 지킨다.
- 문서는 한국어를 사용하고 contract, owner, error identifier는 원문을 유지한다.
- evidence의 operational current는 `0.42.74-admin-smoke`이며 승격하지 않는다.

## Execution status (2026-08-25 completion checkpoint)

- Final code checkpoint HEAD는 `e32db7689d80893544253ef2df27faea4f70d11e`다. Clean
  evidence-input HEAD는 `20ba3b80c211cc6a29bc9ecaf7e9195911678f14`, evidence commit은
  `67004d38195c5fc8af3ba69680e6a9d47e4c5a28`이다. 이후 evidence/manifest guard와 owner
  실패 출력 redaction을 추가 hardening했다.
- Task 1~13과 full completion audit를 완료했다. 이 완료 범위는 Wave B local parity이며 Wave E
  required CI dual-run과 cutover를 포함하지 않는다.
- Registry metadata와 verifier는 legacy Pester 이름·순서를 유지하는 `50/50`이고 positive Node
  projection과 legacy Pester는 각각 `50/50`이다. Migration manifest는 `62`행이며 Web 행만
  `mapped`/local pass/CI pending이고 나머지 `61`행은 `unmapped`/local pending/CI pending이다.
- Controlled missing-`app-root` negative parity의 raw Node TAP은 tests `50`, passed `0`, failed
  `1`, skipped `49`다. Current focused Node unit 집합은 passed `199`, failed `0`, skipped `0`이다.
- 기존 Web 명령과 legacy Pester `50/50`가 PASS했고 .NET Release solution은 passed `1451`,
  failed `0`, skipped `0`으로 baseline과 일치했다. 보호 경로 diff와 temp fixture 잔재는 없다.
- Local evidence는
  `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`에 게시했다. 기존 Web
  Pester, required workflow, C# catalog, protected paths와 current evidence는 변경하지 않았다.
- `ci_parity_pass=false`, `required_ci_pester_zero=false`,
  `required_ci_nonadmin_powershell_zero=false`, `cutover_completed=false`,
  `host_mutation_performed=false`, `msi_or_service_mutation=false`, `actual_vm_tested=false`,
  `public_trusted_signing=false`, `external_stable_publication=false`이고 operational current는
  `0.42.74-admin-smoke`로 유지한다.

## Baseline and completion boundary

- Node `v24.18.0`, npm `11.13.0`, TypeScript `5.9.3`.
- legacy Web Pester: 1파일, 1,207줄, `It` 50개.
- 전체 legacy inventory: Packaging 55파일/528 `It`, Installer 6파일/49 `It`,
  Web 1파일/50 `It`, 합계 62파일/627 `It`.
- existing `npm test --prefix web`: feature surface `52`, excluded `8`, served asset current,
  frontend batch `5/25`.
- existing `npm run verify:parity --prefix web`: served/static/browser fixture PASS.
- .NET Release baseline: `1451` passed, failed `0`, skipped `0`.
- Wave B 완료 시 Node projection `50/50`, legacy Pester `50/50`, controlled negative
  Pester/Node failure, migration inventory `62/62`와 Web `mapped/local pass/CI pending`를
  기록한다.
- `required_ci_pester_zero`, `required_ci_nonadmin_powershell_zero`,
  `ci_parity_pass`와 `cutover_completed`는 모두 `false`다.

## File map

| File | Responsibility |
| --- | --- |
| `web/contracts/web-contract-harness.mjs` | Error codes, root containment, cached file/JSON context, assertion helpers, owner allowlist/cache, bounded/redacted process result. |
| `web/contracts/web-static-contracts.mjs` | Exact 50 metadata, domain verifiers, final ordered registry. |
| `web/node-tests/web-contract-harness.test.mjs` | Containment, cache, assertion, owner argument/cache/redaction/timeout unit tests. |
| `web/node-tests/web-static-contracts-negative.test.mjs` | One deterministic Node-side defect per replacement contract. |
| `web/node-tests/web-static-contracts.test.mjs` | Exactly 50 positive `node:test` projections; no extra test cases. |
| `web/node-tests/verification-migration-manifest.test.mjs` | Strict shape, inventory, state and Web mapping negative tests. |
| `web/node-tests/web-contract-negative-parity.test.mjs` | TAP/Pester summary parser, fixture containment and cleanup unit tests. |
| `web/node-tests/web-verification-architecture-boundary.test.mjs` | Separate npm graph, catalog/CI non-cutover and exact false evidence boundary. |
| `web/scripts/verify-web-contract-registry.mjs` | Parse literal Pester `It` names and verify exact ordinal 50/50 mapping. |
| `web/scripts/verify-verification-migration-manifest.mjs` | Discover 62 files, count literal `It` blocks, validate manifest/schema/state. |
| `web/scripts/verify-web-contract-negative-parity.mjs` | OS-temp `app-root` defect, focused Pester and Node failure, cleanup. |
| `config/development-verification-migration-manifest.schema.json` | Strict `pcv-development-verification-migration-manifest-v1` schema. |
| `config/development-verification-migration-manifest.json` | 62-path migration ledger. |
| `web/package.json` | Four separate Wave B commands; existing `test` unchanged. |
| `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | Wave B local replacement and non-cutover policy. |
| `docs/DEVELOPER_INDEX.md` | Design, plan, manifest and evidence entrypoints. |
| `docs/ga-ready/EVIDENCE_INDEX.md` | Wave B code-level evidence locator. |
| `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md` | Commands, counts, duration, mapping hash and false claims. |

## Protected paths

The final audit must show an empty diff from the implementation base for:

```text
web/tests/PcvDesktopWeb.Static.Tests.ps1
.github/workflows/development-gates.yml
config/development-verification-suites.json
config/development-verification-suites.schema.json
docs/ga-ready/current-evidence.json
```

## Interface ledger

These names and shapes are fixed. Later tasks must not introduce aliases.

```javascript
export const WEB_CONTRACT_ERROR_CODES = Object.freeze({
  configInvalid: "PCV_WEB_CONTRACT_CONFIG_INVALID",
  registryMismatch: "PCV_WEB_CONTRACT_REGISTRY_MISMATCH",
  fileMissing: "PCV_WEB_CONTRACT_FILE_MISSING",
  assertionFailed: "PCV_WEB_CONTRACT_ASSERTION_FAILED",
  ownerFailed: "PCV_WEB_CONTRACT_OWNER_FAILED",
  fixtureUnsafe: "PCV_WEB_CONTRACT_FIXTURE_UNSAFE",
  manifestInvalid: "PCV_VERIFICATION_MIGRATION_MANIFEST_INVALID"
});

export class WebContractError extends Error {
  constructor(code, detail, cause = undefined) {}
}

export function createWebContractContext({
  repoRoot,
  textOverrides = new Map(),
  missingPaths = new Set(),
  processRunner = spawnOwnerProcess
}) {}

export const WEB_STATIC_CONTRACT_METADATA = Object.freeze([]);
export const WEB_STATIC_CONTRACTS = Object.freeze([]);
export function parseLegacyPesterTests(source) {}
export function validateMigrationManifest({ manifest, schema, repoRoot, requireWebLocalPass }) {}
```

Context methods are fixed:

```javascript
context.repoPath(relativePath)
context.forContract(contractId)
context.readText(relativePath)
context.readJson(relativePath)
context.readCombined(relativePaths)
context.readServedSource()
context.assertExists(relativePath, label)
context.assertMatch(value, pattern, label)
context.assertNotMatch(value, pattern, label)
context.assertEqual(actual, expected, label)
context.assertIncludes(values, expected, label)
context.assertBefore(value, first, second, label)
context.runOwners(ownerIds)
```

Owner IDs and direct entrypoints are fixed:

```javascript
const OWNER_COMMANDS = {
  "feature-surface": [["scripts/verify-feature-surface-parity.mjs"]],
  "typescript": [["node_modules/typescript/bin/tsc", "--noEmit", "-p", "tsconfig.json"]],
  "served-asset": [["scripts/build-served-asset.mjs", "--check"]],
  "frontend-batches": [["scripts/validate-frontend-completion-batches.mjs"]],
  "static-parity": [
    ["scripts/regenerate-static-parity.mjs", "--check"],
    ["scripts/verify-static-parity.mjs"]
  ],
  "browser-fixture": [["scripts/verify-browser-fixture.mjs"]],
  "node-check": [["--check", "app.js"]],
  "static-contract": []
};
```

Every non-empty command is invoked with `process.execPath`, an argument array, `cwd=webRoot`,
`shell=false`, `windowsHide=true` and a 120-second timeout. A shared context caches one Promise per
owner ID. Combined stdout/stderr is redacted before an 8 KiB cap.

---

### Task 1: Build the contained cached assertion context

**Files:**
- Create: `web/contracts/web-contract-harness.mjs`
- Create: `web/node-tests/web-contract-harness.test.mjs`

- [x] **Step 1: Write the failing context tests**

Create tests for contained reads, escape rejection, missing files, text/JSON cache identity,
override isolation and stable assertion errors:

```javascript
import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError,
  createWebContractContext
} from "../contracts/web-contract-harness.mjs";

function fixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-"));
  fs.mkdirSync(path.join(root, "web"), { recursive: true });
  fs.writeFileSync(path.join(root, "web", "sample.txt"), "alpha", "utf8");
  fs.writeFileSync(path.join(root, "web", "sample.json"), '{"value":1}', "utf8");
  return root;
}

test("context reads contained files once and honors in-memory overrides", () => {
  const root = fixture();
  try {
    const context = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web/sample.txt", "override"]])
    });
    assert.equal(context.readText("web/sample.txt"), "override");
    assert.strictEqual(context.readJson("web/sample.json"), context.readJson("web/sample.json"));
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("context rejects paths outside the repository", () => {
  const root = fixture();
  try {
    const context = createWebContractContext({ repoRoot: root });
    assert.throws(
      () => context.readText("../outside.txt"),
      (error) => error instanceof WebContractError
        && error.code === WEB_CONTRACT_ERROR_CODES.configInvalid
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});
```

- [x] **Step 2: Run the focused test and observe RED**

```text
node --test web/node-tests/web-contract-harness.test.mjs
```

Expected: nonzero; module `web/contracts/web-contract-harness.mjs` is absent.

- [x] **Step 3: Implement the minimal contained context**

Implement normalized forward-slash relative paths, `path.resolve` plus `fs.realpathSync`
containment with a separator boundary, Map caches and fixed assertion methods. Resolve the
repository root once. For every existing source path, reject a symbolic-link/reparse escape before
reading; wrap filesystem/JSON errors without exposing an absolute profile path:

```javascript
import fs from "node:fs";
import path from "node:path";

export class WebContractError extends Error {
  constructor(code, detail, cause = undefined) {
    super(`${code}|${detail}`, { cause });
    this.name = "WebContractError";
    this.code = code;
  }
}

function normalizeRelative(relativePath) {
  const value = String(relativePath).replaceAll("\\", "/");
  if (!value || value.startsWith("/") || /^[A-Za-z]:/.test(value)) {
    throw new WebContractError(WEB_CONTRACT_ERROR_CODES.configInvalid, "path=invalid");
  }
  return value;
}

function containedPath(repoRoot, relativePath) {
  const normalized = normalizeRelative(relativePath);
  const resolved = path.resolve(repoRoot, ...normalized.split("/"));
  if (resolved !== repoRoot && !resolved.startsWith(`${repoRoot}${path.sep}`)) {
    throw new WebContractError(WEB_CONTRACT_ERROR_CODES.configInvalid, "path=escape");
  }
  return { normalized, resolved };
}
```

`createWebContractContext` stores separate text/JSON caches, honors only normalized override keys,
projects missing overrides as `PCV_WEB_CONTRACT_FILE_MISSING`, and implements every method in the
interface ledger. `forContract(id)` returns a scoped view backed by the same file/owner caches and
adds a runtime `contract_id` field to every assertion/owner error. It rejects an invalid contract ID before
any read. `readServedSource()` extracts the exact
`"(src/served/[A-Za-z0-9._-]+\.ts)"` sequence from
`web/scripts/build-served-asset.mjs`, rejects empty/duplicate/escaping parts and concatenates them
in declared order.

- [x] **Step 4: Run the focused tests and observe GREEN**

Expected: all context tests pass, failed `0`, skipped `0`.

- [x] **Step 5: Commit**

```text
git add web/contracts/web-contract-harness.mjs web/node-tests/web-contract-harness.test.mjs
git commit -m "test: add contained Web contract harness"
```

### Task 2: Add shell-free cached owner execution

**Files:**
- Modify: `web/contracts/web-contract-harness.mjs`
- Modify: `web/node-tests/web-contract-harness.test.mjs`

- [x] **Step 1: Add failing owner adapter tests**

Use an injected runner and verify exact arguments, one invocation per owner, unknown-owner failure,
redaction, cap and timeout projection:

```javascript
test("owner execution uses direct Node arguments and caches by owner id", async () => {
  const calls = [];
  const context = createWebContractContext({
    repoRoot: fixtureRepository(),
    processRunner: async (request) => {
      calls.push(request);
      return { exitCode: 0, timedOut: false, stdout: "ok", stderr: "" };
    }
  });
  await context.runOwners(["served-asset", "served-asset"]);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].fileName, process.execPath);
  assert.deepEqual(calls[0].arguments, ["scripts/build-served-asset.mjs", "--check"]);
  assert.equal(calls[0].shell, false);
});
```

- [x] **Step 2: Run focused tests and observe RED**

Expected: `runOwners` or owner definitions are absent.

- [x] **Step 3: Implement owner definitions and runners**

Use the exact `OWNER_COMMANDS` ledger. For rows whose first argument is a script path, resolve and
contain that path under `webRoot` before passing the original relative argument. For `node-check`,
validate the second argument `app.js` and retain `--check` as the first Node option. Invoke every
row with `process.execPath`, `shell=false` and an argument array. The default runner uses `spawn`, bounded
buffers, a timer, `windowsHide=true` and no stdin. Redact these forms before exposing output:

```javascript
function redact(value) {
  return String(value)
    .replace(/Bearer\s+[A-Za-z0-9._~+/=-]+/gi, "Bearer [REDACTED]")
    .replace(
      /((?:password|access_token|refresh_token|api_token)\s*[=:]\s*)[^\s"']+/gi,
      "$1[REDACTED]"
    );
}
```

Store the owner Promise in the cache before awaiting it. Timeout, signal-only termination,
nonzero exit and unknown owner all throw `PCV_WEB_CONTRACT_OWNER_FAILED` or
`PCV_WEB_CONTRACT_CONFIG_INVALID`. Before the 8 KiB cap, also replace the resolved repository root
and `os.homedir()` in stdout/stderr with `[REDACTED_PATH]`; tests inject both Windows and
forward-slash forms.

- [x] **Step 4: Run harness tests and safe real-owner smoke**

```text
node --test web/node-tests/web-contract-harness.test.mjs
node web/scripts/verify-feature-surface-parity.mjs
node web/scripts/build-served-asset.mjs --check
node web/scripts/regenerate-static-parity.mjs --check
node web/scripts/verify-static-parity.mjs
node web/scripts/verify-browser-fixture.mjs
```

Expected: all exit `0`; repository diff remains empty.

- [x] **Step 5: Commit**

```text
git add web/contracts/web-contract-harness.mjs web/node-tests/web-contract-harness.test.mjs
git commit -m "test: add cached Web verification owners"
```

### Task 3: Lock the exact 50-row legacy metadata mapping

**Files:**
- Create: `web/contracts/web-static-contracts.mjs`
- Create: `web/scripts/verify-web-contract-registry.mjs`
- Modify: `web/node-tests/web-contract-harness.test.mjs`

- [x] **Step 1: Write failing metadata/parser tests**

The parser accepts only literal single- or double-quoted `It` declarations. It fails if the broad
`^\s*It\b` count differs from parsed literals:

```javascript
test("metadata matches the checked-in Pester source 50 for 50", () => {
  const source = fs.readFileSync(
    path.join(repositoryRoot(), "web/tests/PcvDesktopWeb.Static.Tests.ps1"),
    "utf8"
  );
  const legacy = parseLegacyPesterTests(source);
  assert.equal(legacy.length, 50);
  assert.deepEqual(
    WEB_STATIC_CONTRACT_METADATA.map((item) => item.legacyName),
    legacy.map((item) => item.name)
  );
  assert.equal(new Set(WEB_STATIC_CONTRACT_METADATA.map((item) => item.id)).size, 50);
});
```

- [x] **Step 2: Run focused tests and observe RED**

Expected: registry module or exports are absent.

- [x] **Step 3: Add metadata from the approved mapping ledger**

Create all 50 immutable rows using this exact shape:

```javascript
const m = (id, legacyName, domain, owners, legacyLines) =>
  Object.freeze({ id, legacyName, domain, owners: Object.freeze(owners), legacyLines });
```

The exact values are Appendix A. Validate `web.static.` kebab-case IDs, the four approved domains,
owner allowlist, legacy line locators, exact count/order and uniqueness.

- [x] **Step 4: Implement standalone registry verification**

`verify-web-contract-registry.mjs` imports metadata, reads the checked-in Pester file, compares every
ordinal and emits exactly:

```text
Web contract registry PASS: legacy=50 replacement=50 missing=0 duplicate=0
```

Any difference throws a message built as:

```javascript
`${WEB_CONTRACT_ERROR_CODES.registryMismatch}|ordinal=${ordinal}|legacy=${legacyName}|replacement=${replacementId}`
```

Do not include an absolute path.

- [x] **Step 5: Run tests and verifier**

```text
node --test web/node-tests/web-contract-harness.test.mjs
node web/scripts/verify-web-contract-registry.mjs
```

Expected: both exit `0`; registry exact `50/50`.

- [x] **Step 6: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/scripts/verify-web-contract-registry.mjs web/node-tests/web-contract-harness.test.mjs
git commit -m "test: map 50 legacy Web contracts"
```

## Assertion conversion ledger

Tasks 4-9 port the exact legacy line ranges, using this mechanical mapping:

| Legacy form | Node form |
| --- | --- |
| `Test-Path ... -PathType Leaf | Should -BeTrue` | `context.assertExists(path, label)` |
| `$value | Should -Match 'pattern'` | `context.assertMatch(value, /pattern/, label)` |
| `$value | Should -Not -Match 'pattern'` | `context.assertNotMatch(value, /pattern/, label)` |
| `$value | Should -Be expected` | `context.assertEqual(value, expected, label)` |
| `$array | Should -Contain expected` | `context.assertIncludes(array, expected, label)` |
| `IndexOf(first) | Should -BeLessThan IndexOf(second)` | `context.assertBefore(value, first, second, label)` |
| `[regex]::Escape(literal)` | Literal `includes`/`not-includes`, or `new RegExp(escapeRegex(literal), "i")` using a tested harness helper |
| Pester `foreach` assertion | The same ordered loop calling the mapped context assertion |

Regex flags must preserve Pester semantics: default text matching is case-insensitive, so translated
regexes use `i` unless the legacy contract explicitly depends on case. Multiline combined sources
retain their original concatenation order. No assertion is dropped merely because an existing
owner checks an adjacent behavior.

### Task 4: Port shell-assets contracts 1 through 10

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Create: `web/node-tests/web-static-contracts-negative.test.mjs`

- [x] **Step 1: Add failing canonical/defect cases for contracts 1-5**

The test helper reads the canonical text, creates one in-memory override and calls only the named
verifier. Existing owners are tested separately:

```javascript
function removeOnce(relativePath, needle) {
  const canonical = canonicalContext.readText(relativePath);
  assert.equal(canonical.split(needle).length - 1 >= 1, true);
  return new Map([[relativePath, canonical.replace(needle, "")]]);
}

const cases = [
  ["web.static.feature-surface-ledger", "config/desktop-node-feature-surface-ledger.json", '"$schema"'],
  ["web.static.root-assets", "web/index.html", 'id="app-root"'],
  ["web.static.inline-favicon", "web/index.html", '<link rel="icon"'],
  ["web.static.single-edge-isolation", "web/app.js", null],
  ["web.static.design-boundary", "web/DESIGN.md", "PureCVisor Desktop Node Web DESIGN.md"]
];
```

For `single-edge-isolation`, append `../../ui/` to the override instead of removing text. Every
canonical verifier must resolve; every defect must reject with the named contract ID and
`PCV_WEB_CONTRACT_ASSERTION_FAILED` or `PCV_WEB_CONTRACT_CONFIG_INVALID`.

```javascript
await verifier(canonicalContext.forContract(id));
await assert.rejects(
  verifier(defectContext.forContract(id)),
  (error) => error.code === expectedCode && error.message.includes(`contract_id=${id}`)
);
```

- [x] **Step 2: Run contracts 1-5 and observe RED**

```text
node --test --test-name-pattern "web.static.(feature-surface-ledger|root-assets|inline-favicon|single-edge-isolation|design-boundary)" web/node-tests/web-static-contracts-negative.test.mjs
```

Expected: missing verifier failures.

- [x] **Step 3: Port every assertion from legacy lines 43-102**

Translate all existence, match, no-match, package wiring and ordering assertions without weakening
regexes. The feature ledger verifier checks schema constants, exact allowed/required keys,
`target_surfaces` exact unique membership, nonempty features/routes, ID/route patterns, method enum,
nullable permission, unique present/excluded surfaces, mandatory API presence and Web/CLI binding
shapes for ledger, feature, route, excluded-surface and surface-binding objects before owner execution.

- [x] **Step 4: Re-run contracts 1-5 and observe GREEN**

Expected: five canonical checks and five controlled defects are observed by the test suite.

- [x] **Step 5: Add failing canonical/defect cases for contracts 6-10**

```javascript
const cases = [
  ["web.static.supanova-tokens", "web/styles.css", "--accent: #22d3ee"],
  ["web.static.visual-shell", "web/index.html", 'data-ui-port="single-edge-visual-shell"'],
  ["web.static.workbench-frame", "web/index.html", 'class="menu-bar"'],
  ["web.static.frontend-mockups", "web/mockups/frontend-completion-samples.html", "Ops Cockpit"],
  ["web.static.frontend-batches", "docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json", '"batch_count": 5']
];
```

The batch defect replaces the value with `4`; the other cases remove exactly one required literal.

- [x] **Step 6: Observe RED, port legacy lines 103-241, then observe GREEN**

Preserve every stylesheet token, visual shell/workbench selector, Linux/runtime exclusion, mockup
boundary, 5-batch/25-item JSON invariant, plan-doc marker, package wiring, secret guard and mutation
guard. Delegate the existing semantic batch validation to `frontend-batches` while retaining the
legacy-only document/package assertions.

- [x] **Step 7: Run the first ten contracts together**

```text
node --test --test-name-pattern "web.static.(feature-surface-ledger|root-assets|inline-favicon|single-edge-isolation|design-boundary|supanova-tokens|visual-shell|workbench-frame|frontend-mockups|frontend-batches)" web/node-tests/web-static-contracts-negative.test.mjs
```

Expected: selected canonical and defect cases pass; test failures `0`.

- [x] **Step 8: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs
git commit -m "test: port Web shell asset contracts"
```

### Task 5: Port route/action contracts 11 through 20

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Modify: `web/node-tests/web-static-contracts-negative.test.mjs`

- [x] **Step 1: Add defects for contracts 11-15 and observe RED**

```javascript
[
  ["web.static.phase2h-endpoints", "web/app.js", "/api/v1/host/status"],
  ["web.static.local-api-registry", "web/src/served/routes.ts", "DESKTOP_NODE_ROUTE_COVERAGE"],
  ["web.static.qos-guest-readback", "web/app.js", "renderVmQosGuestReadback"],
  ["web.static.qos-guest-control", "web/app.js", "vmQosStoragePreview"],
  ["web.static.guest-exec-cancel", "web/app.js", "Cancel running guest exec"]
]
```

Run a test-name pattern containing these five IDs. Expected: missing verifier failures.

- [x] **Step 2: Port legacy lines 242-345 and observe GREEN**

Port every endpoint, route registry, QoS/readback, Guest Execution, ADR-0009/0010, action label,
RBAC and forbidden novnc control assertion. Use `readServedSource()` so staged source parts and
`served-app.ts` have the same order as the existing build owner.

- [x] **Step 3: Add defects for contracts 16-20 and observe RED**

```javascript
[
  ["web.static.search-event-table", "web/index.html", 'id="command-palette"'],
  ["web.static.served-source-parts", "web/scripts/build-served-asset.mjs", "src/served/errors.ts"],
  ["web.static.optional-bearer", "web/app.js", "Authorization"],
  ["web.static.account-rbac-console", "web/index.html", 'id="account-login-form"'],
  ["web.static.listener-api-base", "web/index.html", "/pcv-config.js"]
]
```

- [x] **Step 4: Port legacy lines 346-449 and observe GREEN**

Preserve search/event/table forbidden runtime checks, exact staged part existence/wiring, optional
bearer surface without literal credentials, account/RBAC/JWT/console/noVNC checks and
`/pcv-config.js` before `/app.js` ordering.

- [x] **Step 5: Run registry plus all implemented cases**

```text
node web/scripts/verify-web-contract-registry.mjs
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs
```

Expected: metadata `50/50`; implemented cases pass, skipped `0` in unfiltered unit runs.

- [x] **Step 6: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs
git commit -m "test: port Web API route contracts"
```

### Task 6: Port route/action contracts 21 through 28

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Modify: `web/node-tests/web-static-contracts-negative.test.mjs`

- [x] **Step 1: Add defects for contracts 21-24**

```javascript
[
  ["web.static.vm-create-payload", "web/app.js", "iso_path"],
  ["web.static.vm-lifecycle-routes", "web/app.js", "resume-saved"],
  ["web.static.vm-detail-mount", "web/index.html", 'id="vm-detail-panel"'],
  ["web.static.vm-lifecycle-actions", "web/app.js", "PCV_VM_DELETE_RUNNING_BLOCKED"]
]
```

Run the four focused tests and observe missing verifier RED.

- [x] **Step 2: Port legacy lines 450-508 and observe GREEN**

Keep exact VM creation fields, lifecycle route/action list, panel mount IDs, destructive
confirmation, managed-VM refusal and saved-state controls.

- [x] **Step 3: Add defects for contracts 25-28**

```javascript
[
  ["web.static.checkpoint-actions", "web/app.js", "checkpoint-restore"],
  ["web.static.browser-job-history", "web/app.js", "pcvDesktopTrackedJobs.v1"],
  ["web.static.job-orchestration", "web/app.js", "jobPollDelayMs"],
  ["web.static.shell-controls", "web/index.html", 'data-menu-command="refresh"']
]
```

- [x] **Step 4: Port legacy lines 509-571 and observe GREEN**

Keep checkpoint CRUD UI, local job history, pending-state/polling/pagination and shell/view binding
assertions. Preserve forbidden `/auth/token` and `/ws/events` checks.

- [x] **Step 5: Run all route/action cases and existing owners**

```text
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs
node web/scripts/verify-feature-surface-parity.mjs
node web/scripts/build-served-asset.mjs --check
```

Expected: all exit `0`.

- [x] **Step 6: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs
git commit -m "test: port Web lifecycle action contracts"
```

### Task 7: Port operations/evidence contracts 29 through 35

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Modify: `web/node-tests/web-static-contracts-negative.test.mjs`

- [x] **Step 1: Add defects for contracts 29-32**

```javascript
[
  ["web.static.activity-troubleshooting", "web/index.html", 'id="activity-panel"'],
  ["web.static.ops-cockpit", "web/index.html", 'id="ops-summary-panel"'],
  ["web.static.evidence-dashboard", "web/index.html", 'id="evidence-panel"'],
  ["web.static.evidence-degradation", "web/app.js", "collectEvidenceIssues"]
]
```

Observe RED before adding these verifiers.

- [x] **Step 2: Port legacy lines 572-687 and observe GREEN**

Preserve route/pagination/correlation identifiers, ops cockpit views, batch/current evidence keys,
fixture statuses, public-boundary fixture values, degradation and failed-job triage. Browser fixture
owner remains cached and is not duplicated inside static verifier functions.

- [x] **Step 3: Add defects for contracts 33-35**

```javascript
[
  ["web.static.diagnostic-bundle", "web/app.js", "renderDiagnosticBundleList"],
  ["web.static.operator-terms", "docs/OPERATOR_SURFACE_TERMS.md", "배포 경계: 내부 사설망 전용"],
  ["web.static.frontend-edge-cases", "web/app.js", "PCV_SELECTED_VM_STALE"]
]
```

- [x] **Step 4: Port legacy lines 688-808 and observe GREEN**

Port every diagnostic create/list/download/retention/registry/Host Ops bucket assertion, all
PowerShell/host-mutation/credential exclusions, internal distribution terminology and every final
frontend error/a11y/responsive/installed-QA marker.

- [x] **Step 5: Run operations cases and browser fixture**

```text
node --test web/node-tests/web-static-contracts-negative.test.mjs
node web/scripts/verify-browser-fixture.mjs
```

Expected: all exit `0`; repository diff contains only intended source/test changes.

- [x] **Step 6: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs
git commit -m "test: port Web evidence contracts"
```

### Task 8: Port operations contracts 36 through 40

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Modify: `web/node-tests/web-static-contracts-negative.test.mjs`

- [x] **Step 1: Add all five defects and observe RED**

```javascript
[
  ["web.static.token-rotation", "web/index.html", 'id="token-rotation-panel"'],
  ["web.static.beta-followup", "web/index.html", 'id="beta-followup-panel"'],
  ["web.static.monitoring", "web/index.html", 'id="monitoring-panel"'],
  ["web.static.network-inventory", "web/index.html", 'id="network-inventory-panel"'],
  ["web.static.workflow-polish", "web/index.html", 'id="vm-filter"']
]
```

- [x] **Step 2: Port legacy lines 809-892 and observe GREEN**

Preserve token-rotation handoff/non-mutation, beta follow-up status, read-only monitoring,
read-only network inventory and workflow quality assertions. Keep every secret, host mutation and
Linux runtime exclusion regex.

- [x] **Step 3: Run all first 40 contract units and owners**

```text
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs
node web/scripts/verify-web-contract-registry.mjs
node web/scripts/verify-browser-fixture.mjs
```

Expected: all exit `0`.

- [x] **Step 4: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs
git commit -m "test: port Web operations contracts"
```

### Task 9: Port TypeScript/parity contracts 41 through 50 and project exactly 50 tests

**Files:**
- Modify: `web/contracts/web-static-contracts.mjs`
- Modify: `web/node-tests/web-static-contracts-negative.test.mjs`
- Create: `web/node-tests/web-static-contracts.test.mjs`

- [x] **Step 1: Add defects for contracts 41-45 and observe RED**

```javascript
[
  ["web.static.javascript-syntax", "owner:node-check", "forced nonzero"],
  ["web.static.served-typescript-output", "web/package.json", '"check:served"'],
  ["web.static.typescript-scaffold", "web/package.json", "npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches"],
  ["web.static.typescript-contract-mirror", "web/src/api-types.ts", "RuntimePolicyResponse"],
  ["web.static.parity-manifest", "web/generated/parity/static-asset-parity.manifest.json", '"runtimePolicy": "/api/v1/runtime/policy"']
]
```

The `node-check` defect uses an injected failed owner result. JSON defects remain valid JSON.

- [x] **Step 2: Port legacy lines 893-1002 and observe GREEN**

Use owners for JavaScript syntax, served build, TypeScript and static parity. Retain every package
script exact value, scaffold file, TypeScript mirror, manifest field/route/regeneration, index
script and no-secret/no-mutation assertion that is not already owned by those commands.

- [x] **Step 3: Add defects for contracts 46-50 and observe RED**

```javascript
[
  ["web.static.user-visible-fixtures", "web/src/user-visible-fixtures.ts", "emptyInventory"],
  ["web.static.verifier-wiring", "web/package.json", '"verify:parity"'],
  ["web.static.generated-parity-alignment", "web/generated/parity/static-asset-parity.manifest.json", '"mutating": false'],
  ["web.static.secret-mutation-guard", "web/src/app.ts", "Bearer abcdefghijklmnopqrstuvwxyz"],
  ["web.static.no-fabricated-values", "web/index.html", "VM: 3/3"]
]
```

For generated parity replace `false` with `true` only in `browserFixture.mutating`.

- [x] **Step 4: Port legacy lines 1003-1206 and observe GREEN**

Keep all fixture, verifier wiring, manifest alignment, combined scan, secret/mutation guard,
fabricated literal, ID-bound value and required binding assertions. Do not reduce the
`no-fabricated-values` location-space or value-space checks.

- [x] **Step 5: Bind metadata and verifier maps with exact-set validation**

`WEB_STATIC_CONTRACTS` is created only after both key sets are exact:

```javascript
const metadataIds = WEB_STATIC_CONTRACT_METADATA.map((item) => item.id);
const verifierIds = Object.keys(WEB_STATIC_VERIFIERS);
if (metadataIds.length !== 50
    || verifierIds.length !== 50
    || metadataIds.some((id, index) => id !== verifierIds[index])) {
  throw new WebContractError(
    WEB_CONTRACT_ERROR_CODES.registryMismatch,
    "metadata-verifier-set=not-exact"
  );
}

export const WEB_STATIC_CONTRACTS = Object.freeze(
  WEB_STATIC_CONTRACT_METADATA.map((metadata) =>
    Object.freeze({ ...metadata, verify: WEB_STATIC_VERIFIERS[metadata.id] }))
);
```

- [x] **Step 6: Create the exact 50-test positive projection**

The file has no describe blocks or helper tests:

```javascript
import path from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
import { createWebContractContext } from "../contracts/web-contract-harness.mjs";
import { WEB_STATIC_CONTRACTS } from "../contracts/web-static-contracts.mjs";

const defaultRepoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const repoRoot = process.env.PCV_WEB_CONTRACT_FIXTURE_ROOT ?? defaultRepoRoot;
const context = createWebContractContext({ repoRoot });

for (const contract of WEB_STATIC_CONTRACTS) {
  test(contract.id, { concurrency: false }, async () => {
    const scoped = context.forContract(contract.id);
    await scoped.runOwners(contract.owners);
    await contract.verify(scoped);
  });
}
```

The fixture override is accepted only when
`PCV_WEB_CONTRACT_FIXTURE_MODE=negative-parity-v1`, its real path is under `os.tmpdir()`, and the
marker `.pcv-web-contract-negative-v1` exists. Otherwise fail before reading it.

- [x] **Step 7: Run positive and negative Node suites**

```text
node --test --test-reporter=spec web/node-tests/web-static-contracts.test.mjs
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs
node web/scripts/verify-web-contract-registry.mjs
```

Expected: positive projection tests `50`, pass `50`, fail `0`, skipped `0`; all units pass.

- [x] **Step 8: Run unchanged npm owners**

```text
npm test --prefix web
npm run verify:parity --prefix web
```

Expected: both exit `0`; existing `scripts.test` string remains exact.

- [x] **Step 9: Commit**

```text
git add web/contracts/web-static-contracts.mjs web/node-tests/web-static-contracts-negative.test.mjs web/node-tests/web-static-contracts.test.mjs
git commit -m "test: complete 50 Web contract replacements"
```

### Task 10: Add the strict 62-file migration manifest

**Files:**
- Create: `config/development-verification-migration-manifest.schema.json`
- Create: `config/development-verification-migration-manifest.json`
- Create: `web/scripts/verify-verification-migration-manifest.mjs`
- Create: `web/node-tests/verification-migration-manifest.test.mjs`

- [x] **Step 1: Write failing strict-shape and state tests**

Create a valid in-memory fixture, then mutate one dimension per test:

```javascript
test("rejects an additional entry property", () => {
  const manifest = validManifest();
  manifest.entries[0].unexpected = true;
  assert.throws(
    () => validateMigrationManifest(input(manifest)),
    errorCode(WEB_CONTRACT_ERROR_CODES.manifestInvalid)
  );
});

test("rejects missing, duplicate and wrong-count inventory paths", () => {
  const missing = validManifest();
  missing.entries.pop();
  assert.throws(() => validateMigrationManifest(input(missing)));

  const duplicate = validManifest();
  duplicate.entries[1].legacy_path = duplicate.entries[0].legacy_path;
  assert.throws(() => validateMigrationManifest(input(duplicate)));

  const wrongCount = validManifest();
  wrongCount.entries.at(-1).legacy_contract_count = 49;
  assert.throws(() => validateMigrationManifest(input(wrongCount)));
});

test("rejects early Web dual-run or cutover", () => {
  for (const status of ["dual-run-pass", "cutover"]) {
    const manifest = validManifest();
    webEntry(manifest).parity_status = status;
    assert.throws(() => validateMigrationManifest(input(manifest)));
  }
});
```

Also cover unknown enum, Web ID omission, a non-Web mapped row, pass without evidence, CI pass in
Wave B, incorrect broad `It` count, escaping evidence locator and `--require-web-local-pass` while
local status is pending.

- [x] **Step 2: Run focused tests and observe RED**

```text
node --test web/node-tests/verification-migration-manifest.test.mjs
```

Expected: validator module is absent.

- [x] **Step 3: Create the strict schema**

The schema has `additionalProperties=false` at every object. Top-level and entry shape are exact:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "pcv-development-verification-migration-manifest-schema-v1",
  "type": "object",
  "additionalProperties": false,
  "required": ["contract", "schema_version", "inventory", "entries"],
  "properties": {
    "contract": {
      "const": "pcv-development-verification-migration-manifest-v1"
    },
    "schema_version": { "const": 1 },
    "inventory": { "$ref": "#/$defs/inventory" },
    "entries": {
      "type": "array",
      "minItems": 62,
      "maxItems": 62,
      "items": { "$ref": "#/$defs/entry" }
    }
  }
}
```

Define `inventory` with exact integer constants `total=62`, `packaging=55`, `installer=6`,
`web=1`. Define `entry` with the ten approved fields and nested exact
`status/evidence` objects. `replacement_owner` and evidence accept string or null;
`replacement_contract_ids` contains unique `web.static.` IDs;
`parity_status` is `unmapped|mapped|dual-run-pass|cutover`; local/CI status is
`pending|pass|fail`.

- [x] **Step 4: Implement discovery and semantic validation**

Discover only direct `*.Tests.ps1` files under the three approved roots. Normalize to forward
slashes, reject symlinks/reparse escapes, sort by domain order Packaging → Installer → Web and
then ordinal path. Count each file's Pester contracts with the broad line-start declaration:

```javascript
const broadCount = (source.match(/^\s*It\b/gm) ?? []).length;
```

This intentionally includes the two valid Packaging declarations that continue onto a following
`-Skip` line. Exact name/ordinal parsing is required only for the Web file through
`parseLegacyPesterTests`, where all 50 declarations are literal single-line names.

Validate the schema's published constants plus the same strict runtime shape. Compare discovered
paths/counts with the manifest exactly. Require:

```javascript
if (entry.domain === "web") {
  assert(entry.parity_status === "mapped");
  assert(entry.replacement_owner === "web/node-tests/web-static-contracts.test.mjs");
  assertDeepEqual(entry.replacement_contract_ids, WEB_STATIC_CONTRACTS.map((item) => item.id));
  assert(entry.ci_parity.status === "pending");
  assert(entry.ci_parity.evidence === null);
} else {
  assert(entry.parity_status === "unmapped");
  assert(entry.replacement_owner === null);
  assertDeepEqual(entry.replacement_contract_ids, []);
  assert(entry.local_parity.status === "pending");
  assert(entry.ci_parity.status === "pending");
}
```

`dual-run-pass` requires local/CI pass and both evidence locators but is forbidden for the Web row
in Wave B. `cutover` is always rejected. `--require-web-local-pass` additionally requires Web
local pass and an existing contained evidence file.

- [x] **Step 5: Create the canonical manifest from Appendix B**

Create one entry per Appendix B row. Initial implementation state is:

```json
{
  "legacy_path": "web/tests/PcvDesktopWeb.Static.Tests.ps1",
  "domain": "web",
  "legacy_contract_count": 50,
  "replacement_owner": "web/node-tests/web-static-contracts.test.mjs",
  "replacement_contract_ids": ["all 50 Appendix A IDs in order"],
  "parity_status": "mapped",
  "local_parity": { "status": "pending", "evidence": null },
  "ci_parity": { "status": "pending", "evidence": null }
}
```

All other 61 rows use their Appendix B count, null owner, empty IDs, `unmapped` and pending/null
local/CI. This intermediate pending state is not Wave B completion evidence.

- [x] **Step 6: Run unit and canonical validators**

```text
node --test web/node-tests/verification-migration-manifest.test.mjs
node web/scripts/verify-verification-migration-manifest.mjs
```

Expected:

```text
Verification migration manifest PASS: total=62 packaging=55 installer=6 web=1 missing=0 duplicate=0 web_status=mapped web_local=pending web_ci=pending
```

- [x] **Step 7: Commit**

```text
git add config/development-verification-migration-manifest.schema.json config/development-verification-migration-manifest.json web/scripts/verify-verification-migration-manifest.mjs web/node-tests/verification-migration-manifest.test.mjs
git commit -m "test: add verification migration manifest"
```

### Task 11: Prove controlled negative parity in OS temp

**Files:**
- Create: `web/scripts/verify-web-contract-negative-parity.mjs`
- Create: `web/node-tests/web-contract-negative-parity.test.mjs`
- Modify: `web/node-tests/web-static-contracts.test.mjs`

- [x] **Step 1: Write failing summary and containment tests**

Export pure parsers and an injectable `runNegativeParity` boundary. Unit tests require:

```javascript
assert.deepEqual(parsePesterSummary(
  '{"total":50,"passed":0,"failed":1,"skipped":0,"not_run":49}'
), { total: 50, passed: 0, failed: 1, skipped: 0, notRun: 49 });

assert.deepEqual(parseNodeTap(`
1..50
# tests 50
# pass 0
# fail 1
# skipped 49
`), { tests: 50, passed: 0, failed: 1, skipped: 49 });
```

Add tests that reject zero executed Pester tests, Node pass, two failures, wrong failure label,
fixture outside `os.tmpdir()`, marker absence and cleanup failure.

- [x] **Step 2: Run units and observe RED**

```text
node --test web/node-tests/web-contract-negative-parity.test.mjs
```

Expected: script module is absent.

- [x] **Step 3: Implement safe fixture creation**

Use `fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-contract-negative-"))`, realpath containment
and marker `.pcv-web-contract-negative-v1`. Copy only:

```text
web/tests/PcvDesktopWeb.Static.Tests.ps1
web/index.html
web/styles.css
web/app.js
web/scripts/build-served-asset.mjs
```

Preserve relative `web/tests` and `web/scripts` layout. Remove exactly one
`id="app-root"` occurrence; zero or multiple replacements fail with
`PCV_WEB_CONTRACT_FIXTURE_UNSAFE`.

- [x] **Step 4: Implement focused legacy Pester execution**

Spawn `pwsh` directly with:

```text
-NoLogo
-NoProfile
-NonInteractive
-Command
the literal comparison script shown below
```

Set child-only environment variable `PCV_WEB_NEGATIVE_PESTER_PATH` to the validated fixture
Pester path. Do not rely on a trailing `-Command` argument: the repository's current `pwsh`
invocation does not project that value into `$args[0]`.

The executable is the fixed basename `pwsh` (`pwsh.exe` on Windows), never user input. Use
`shell=false`, `windowsHide=true`, no stdin, a 120-second timeout and the same 8 KiB/redaction
boundary as owner execution.

The fixed comparison script runs:

```powershell
$fullName = 'PcvDesktopWeb static console assets.ships index, stylesheet, and script assets under the Desktop Node web root'
$pesterPath = $env:PCV_WEB_NEGATIVE_PESTER_PATH
if ([string]::IsNullOrWhiteSpace($pesterPath)) { exit 3 }
$result = Invoke-Pester -Path $pesterPath -FullNameFilter $fullName -PassThru -Output None
$failed = @($result.Tests | Where-Object Result -eq 'Failed')
[ordered]@{
  total = $result.TotalCount
  passed = $result.PassedCount
  failed = $result.FailedCount
  skipped = $result.SkippedCount
  not_run = $result.NotRunCount
  failure = $failed[0].ErrorRecord.Exception.Message
} | ConvertTo-Json -Compress
if ($failed.Count -eq 1 -and $result.FailedCount -eq 1) { exit 1 }
exit 2
```

Require child exit `1`, total `50`, passed `0`, failed `1`, skipped `0`, not-run `49`,
executed count `1` and failure text containing `app-root`. Exit `0`, exit `2`, all-not-run and
unparseable output are parity failures.

- [x] **Step 5: Implement focused Node execution**

Spawn:

```text
node --test --test-reporter=tap node-tests/web-static-contracts.test.mjs
```

Set only `PCV_WEB_CONTRACT_FIXTURE_MODE=negative-parity-v1` and set
`PCV_WEB_CONTRACT_FIXTURE_ROOT` to the validated GUID fixture root. The user-approved runtime
correction removes the obsolete `--test-name-pattern` approach: the current Node runtime reported
only one raw test and zero skips with that filter. Fixture mode instead registers all 50 contracts,
explicitly skips the 49 non-root contracts and executes the defective root contract without
synthesizing counts. Require child exit `1`, tests `50`, pass `0`, fail `1`, skipped `49` and output
containing both `web.static.root-assets` and `app-root`.

- [x] **Step 6: Enforce cleanup and stable PASS output**

Remove only the validated GUID temp root in `finally`. Cleanup failure is terminal. Success output:

```text
Web negative parity PASS: defect=missing-app-root pester_executed=1 pester_failed=1 pester_not_run=49 node_failed=1 node_skipped=49 cleanup=pass
```

- [x] **Step 7: Run unit and real controlled-negative tests**

```text
node --test web/node-tests/web-contract-negative-parity.test.mjs
node web/scripts/verify-web-contract-negative-parity.mjs
```

Expected: unit tests pass; real command exits `0` only after observing both expected child
nonzero results and successful cleanup.

- [x] **Step 8: Verify the original worktree was not changed**

```text
git status --short
git diff --check
```

Expected: only Task 11 source/test files are dirty before commit.
The exact scope is the two new files and the approved
`web/node-tests/web-static-contracts.test.mjs` fixture-projection modification.

- [x] **Step 9: Commit**

```text
git add web/scripts/verify-web-contract-negative-parity.mjs web/node-tests/web-contract-negative-parity.test.mjs web/node-tests/web-static-contracts.test.mjs
git commit -m "test: prove Web negative parity"
```

### Task 12: Wire separate npm commands and publish Wave B evidence

**Files:**
- Modify: `web/package.json`
- Modify: `config/development-verification-migration-manifest.json`
- Create: `web/node-tests/web-verification-architecture-boundary.test.mjs`
- Modify: `web/node-tests/verification-migration-manifest.test.mjs`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Create: `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`

- [x] **Step 1: Add a failing separate-command architecture test**

Create the architecture test with package/catalog assertions:

```javascript
assert.equal(
  packageJson.scripts.test,
  "npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches"
);
assert.equal(catalog.activation_state, "plan-only-foundation");
assert.equal(catalog.suites.find((item) => item.id === "web-parity").migration_state, "wave-b-pending");
```

Also assert that all four new script properties are absent so the focused test is genuinely RED.

- [x] **Step 2: Run the architecture test and observe RED**

```text
node --test web/node-tests/web-verification-architecture-boundary.test.mjs
```

Expected: the four separate package commands are missing.

- [x] **Step 3: Add four separate package scripts without modifying `scripts.test`**

Add exactly:

```json
"check:web-contract-registry": "node scripts/verify-web-contract-registry.mjs",
"check:verification-migration-manifest": "node scripts/verify-verification-migration-manifest.mjs --require-web-local-pass",
"test:web-contracts": "npm run check:web-contract-registry && npm run check:verification-migration-manifest && node --test --test-reporter=spec node-tests/web-static-contracts.test.mjs",
"verify:web-contract-negative-parity": "node scripts/verify-web-contract-negative-parity.mjs"
```

Do not insert any new command into the existing `test` or `verify:parity` string.

- [x] **Step 4: Prove package wiring GREEN and commit it**

Update the architecture assertions to require the exact four values, then run:

```text
node --test web/node-tests/web-verification-architecture-boundary.test.mjs
pwsh -NoProfile -NonInteractive -Command "$r=Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -PassThru -Output None; if($r.PassedCount -ne 50 -or $r.FailedCount -ne 0 -or $r.SkippedCount -ne 0){exit 1}"
```

Expected: architecture tests pass and legacy Pester remains `50/50`.

```text
git add web/package.json web/node-tests/web-verification-architecture-boundary.test.mjs
git commit -m "test: wire separate Web contract commands"
```

- [x] **Step 5: Record a clean code-input HEAD**

```text
git status --short
git rev-parse HEAD
```

Expected: status empty. Record this commit as `evidence-input HEAD`; it now includes all Wave B
runtime/test/package wiring but not the self-referencing evidence commit.

- [x] **Step 6: Run same-commit positive and negative parity**

Run the clean code input commands and retain exact counts/durations:

```text
pwsh -NoProfile -NonInteractive -Command "$r=Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -PassThru -Output None; [pscustomobject]@{total=$r.TotalCount;passed=$r.PassedCount;failed=$r.FailedCount;skipped=$r.SkippedCount;not_run=$r.NotRunCount;duration_ms=[math]::Round($r.Duration.TotalMilliseconds)}|ConvertTo-Json -Compress; if($r.FailedCount -ne 0 -or $r.PassedCount -ne 50){exit 1}"
node --test --test-reporter=spec web/node-tests/web-static-contracts.test.mjs
node web/scripts/verify-web-contract-negative-parity.mjs
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs web/node-tests/verification-migration-manifest.test.mjs web/node-tests/web-contract-negative-parity.test.mjs
npm test --prefix web
npm run verify:parity --prefix web
```

Expected: legacy `50/50`, replacement `50/50`, controlled negative parity PASS, all units and
existing npm owners PASS.

- [x] **Step 7: Extend architecture tests and observe evidence RED**

Add manifest/evidence assertions:

```javascript
assert.equal(webManifestEntry.parity_status, "mapped");
assert.equal(webManifestEntry.local_parity.status, "pass");
assert.equal(webManifestEntry.ci_parity.status, "pending");
```

The evidence test requires every exact line once:

```text
ci_parity_pass=false
required_ci_pester_zero=false
required_ci_nonadmin_powershell_zero=false
cutover_completed=false
host_mutation_performed=false
msi_or_service_mutation=false
actual_vm_tested=false
public_trusted_signing=false
external_stable_publication=false
operational_current=0.42.74-admin-smoke
```

Run the architecture test. Expected: failure because local status is pending and the evidence file
does not exist.

Because the canonical manifest itself changes from local pending to local pass in this evidence
slice, update `verification-migration-manifest.test.mjs` in the same approved scope: its canonical
summary and CLI expectation must become `web_local=pass`, while explicit pending and missing-evidence
fixtures continue to prove the `--require-web-local-pass` rejection boundary.

- [x] **Step 8: Create evidence and promote only local Web status**

Create the evidence document with `evidence-input HEAD`, `input_dirty_state=clean`, command lines, exact observed
counts/durations, negative defect/result/cleanup and mapping hash. The mapping hash is lowercase
SHA-256 over UTF-8 rows in registry order:

```javascript
`${legacyName}\0${id}\n`
```

Update only the Web manifest row:

```json
"parity_status": "mapped",
"local_parity": {
  "status": "pass",
  "evidence": "docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md"
},
"ci_parity": {
  "status": "pending",
  "evidence": null
}
```

The other 61 rows remain `unmapped/pending/pending`.

Update documentation entrypoints in the same evidence slice.

Insert `## 2026-08-24 Web verification Wave B local parity` immediately after each generated
current-evidence block or current Wave A section as appropriate:

- Policy: separate commands, Web `mapped`, local pass, CI pending, non-cutover boundary.
- Developer index: design, this plan, manifest/schema and evidence locators.
- Evidence index: local-only verdict and exact false claims.

Do not edit generated current-evidence blocks.

- [x] **Step 9: Run final package, architecture, manifest and legacy checks**

```text
npm run test:web-contracts --prefix web
npm run verify:web-contract-negative-parity --prefix web
node --test web/node-tests/web-verification-architecture-boundary.test.mjs
node web/scripts/verify-verification-migration-manifest.mjs --require-web-local-pass
pwsh -NoProfile -NonInteractive -Command "$r=Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -PassThru -Output None; if($r.PassedCount -ne 50 -or $r.FailedCount -ne 0 -or $r.SkippedCount -ne 0){exit 1}"
```

Expected: Node projection `50/50`; Web local pass; CI pending; architecture tests pass; legacy
Pester passed `50`, failed/skipped `0`.

- [x] **Step 10: Commit the evidence slice**

```text
git add config/development-verification-migration-manifest.json web/node-tests/web-verification-architecture-boundary.test.mjs web/node-tests/verification-migration-manifest.test.mjs docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md
git commit -m "docs: record Web verification Wave B parity"
```

### Task 13: Run the full completion audit

**Files:**
- Modify only if fresh observed evidence differs:
  `docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md`
- Explicitly approved current-facing status synchronization:
  `docs/GUIDE.md`, `docs/CODING_GUIDE.md`, `AGENTS.md`, `README.md`, `web/DESIGN.md` and this plan.

- [x] **Step 1: Run Node units and exact 50 projection**

```text
node --test web/node-tests/web-contract-harness.test.mjs web/node-tests/web-static-contracts-negative.test.mjs web/node-tests/verification-migration-manifest.test.mjs web/node-tests/web-contract-negative-parity.test.mjs web/node-tests/web-verification-architecture-boundary.test.mjs
npm run test:web-contracts --prefix web
npm run verify:web-contract-negative-parity --prefix web
```

Expected: unit failures `0`, skips `0`; positive projection exactly `50/50`; negative parity PASS.

- [x] **Step 2: Run unchanged Web and legacy gates**

```text
npm test --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -NonInteractive -Command "$r=Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -PassThru -Output None; $r|Select-Object Result,TotalCount,PassedCount,FailedCount,SkippedCount,NotRunCount,Duration|ConvertTo-Json -Compress; if($r.PassedCount -ne 50 -or $r.FailedCount -ne 0 -or $r.SkippedCount -ne 0 -or $r.NotRunCount -ne 0){exit 1}"
```

Expected: npm commands exit `0`; Pester `50/50`, failed/skipped/not-run `0`.

- [x] **Step 3: Run the complete .NET Release solution**

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

Expected baseline: passed `1451`, failed `0`, skipped `0`. If the count differs, report the exact
assembly counts and stop before claiming parity.

- [x] **Step 4: Validate manifest and document structure**

```text
node web/scripts/verify-web-contract-registry.mjs
node web/scripts/verify-verification-migration-manifest.mjs --require-web-local-pass
git diff --check
```

Expected: registry `50/50`, inventory `62/62`, missing/duplicate `0`, Web mapped/local pass/CI
pending and diff check exit `0`.

- [x] **Step 5: Prove protected paths are unchanged**

```text
git diff --name-only 75b540797d4b9f77457b8549c7bbcedfad739d1e -- web/tests/PcvDesktopWeb.Static.Tests.ps1 .github/workflows/development-gates.yml config/development-verification-suites.json config/development-verification-suites.schema.json docs/ga-ready/current-evidence.json
```

Expected: stdout empty.

- [x] **Step 6: Review the fixed implementation diff once**

Review only the diff from implementation base against the approved design and this plan. Check:

- all 50 legacy assertions have a Node owner or static verifier;
- every ID has one positive projection and one deterministic Node negative;
- child commands are direct Node arguments and cached;
- no repository write occurs during verification;
- manifest/evidence state cannot claim CI or cutover;
- no `.ps1`, workflow, catalog or current-evidence modification.

If the review finds a defect, fix only affected lines, run their focused RED/GREEN test and perform
one narrow re-review before repeating the full relevant gate.

- [x] **Step 7: Commit evidence-only corrections when required**

If fresh output changed an observed duration/count/hash, update only the evidence document, run
architecture/document checks and commit:

```text
git add docs/ga-ready/evidence/pester-free-web-verification-wave-b-2026-08-24.md
git commit -m "docs: finalize Web verification Wave B evidence"
```

If no value changed, do not create an empty commit.

Fresh audit는 clean evidence-input의 count/hash를 바꾸지 않았으므로 evidence-only correction은
필요하지 않았다. Current-facing checkpoint 문서는 별도 사용자 승인 범위에서 동기화했다.

- [x] **Step 8: Confirm clean final state**

```text
git status --short
git log -1 --oneline
```

Expected: status empty. This closes Wave B local parity only; Wave E required CI cutover remains
pending.

---

## Appendix A: Exact 50-contract metadata ledger

The `legacyName` column is copied byte-for-byte from the checked-in Pester source. Owner values
separated by `+` become an ordered array.

| # | ID | Lines | Domain | Owners | Exact `legacyName` |
| ---: | --- | ---: | --- | --- | --- |
| 1 | `web.static.feature-surface-ledger` | 43-54 | shell-assets | feature-surface | validates and wires the stable Feature ID surface ledger |
| 2 | `web.static.root-assets` | 55-66 | shell-assets | static-contract | ships index, stylesheet, and script assets under the Desktop Node web root |
| 3 | `web.static.inline-favicon` | 67-74 | shell-assets | static-contract | declares an inline favicon to avoid favicon.ico console noise |
| 4 | `web.static.single-edge-isolation` | 75-84 | shell-assets | static-contract | keeps the Desktop Node web console isolated from the Single Edge ui tree |
| 5 | `web.static.design-boundary` | 85-102 | shell-assets | static-contract | ships a Desktop Node web design contract without importing Single Edge runtime routes |
| 6 | `web.static.supanova-tokens` | 103-119 | shell-assets | static-contract | uses Desktop Node Supanova operation-console tokens in the active stylesheet |
| 7 | `web.static.visual-shell` | 120-142 | shell-assets | static-contract | ports the Single Edge visual shell into the active Desktop Node console without importing runtime routes |
| 8 | `web.static.workbench-frame` | 143-168 | shell-assets | static-contract | clones the Single console workbench frame while keeping Linux service surfaces excluded |
| 9 | `web.static.frontend-mockups` | 169-191 | shell-assets | static-contract | ships frontend completion mockup sample screens inside the Desktop Node web root |
| 10 | `web.static.frontend-batches` | 192-241 | shell-assets | frontend-batches | declares the 1-25 frontend completion work as five automatic staged batches |
| 11 | `web.static.phase2h-endpoints` | 242-252 | routes-actions | static-contract | declares the Phase 2H API endpoints used by the console |
| 12 | `web.static.local-api-registry` | 253-276 | routes-actions | feature-surface+static-contract | centralizes Local API access behind the Desktop Node frontend service registry |
| 13 | `web.static.qos-guest-readback` | 277-299 | routes-actions | static-contract | declares the Web VM QoS and guest readback operator surface as read-only routes |
| 14 | `web.static.qos-guest-control` | 300-332 | routes-actions | static-contract | opens Web VM QoS and ADR-0009 Guest Execution direct control routes with explicit operator controls |
| 15 | `web.static.guest-exec-cancel` | 333-345 | routes-actions | static-contract | exposes running guest execution cancel affordance on Web job rows |
| 16 | `web.static.search-event-table` | 346-373 | routes-actions | static-contract | adds Windows-local command palette, global search, event center, and table helpers from the Single Edge borrowing map |
| 17 | `web.static.served-source-parts` | 374-397 | routes-actions | served-asset+static-contract | splits frontend service logic into staged source parts before generating app.js |
| 18 | `web.static.optional-bearer` | 398-405 | routes-actions | static-contract | supports optional bearer token requests |
| 19 | `web.static.account-rbac-console` | 406-437 | routes-actions | static-contract | declares account RBAC JWT login, refresh, session, and console capability UX |
| 20 | `web.static.listener-api-base` | 438-449 | routes-actions | static-contract | loads listener-provided API base URL before the served app starts |
| 21 | `web.static.vm-create-payload` | 450-459 | routes-actions | static-contract | declares the VM create payload fields expected by POST /api/v1/vms |
| 22 | `web.static.vm-lifecycle-routes` | 460-474 | routes-actions | static-contract | declares the Phase 3B VM detail and lifecycle endpoints used by the console |
| 23 | `web.static.vm-detail-mount` | 475-487 | routes-actions | static-contract | ships a VM detail panel mount point |
| 24 | `web.static.vm-lifecycle-actions` | 488-508 | routes-actions | static-contract | declares lifecycle action handlers and destructive confirmation |
| 25 | `web.static.checkpoint-actions` | 509-518 | routes-actions | static-contract | declares checkpoint UI actions used by the console |
| 26 | `web.static.browser-job-history` | 519-530 | routes-actions | static-contract | persists tracked browser job history locally |
| 27 | `web.static.job-orchestration` | 531-547 | routes-actions | static-contract | hardens browser job orchestration with scoped pending state, polling backoff, and next-page loading |
| 28 | `web.static.shell-controls` | 548-571 | routes-actions | static-contract | binds the Single-style shell controls to Desktop Node view and asset state |
| 29 | `web.static.activity-troubleshooting` | 572-596 | operations-evidence | static-contract | declares operator activity and troubleshooting console surfaces |
| 30 | `web.static.ops-cockpit` | 597-635 | operations-evidence | static-contract+browser-fixture | declares the ops cockpit multi-view shell and summary route |
| 31 | `web.static.evidence-dashboard` | 636-671 | operations-evidence | static-contract+browser-fixture | declares the batch evidence dashboard surface |
| 32 | `web.static.evidence-degradation` | 672-687 | operations-evidence | static-contract+browser-fixture | declares troubleshooting evidence degradation and failed job triage surfaces |
| 33 | `web.static.diagnostic-bundle` | 688-732 | operations-evidence | static-contract+browser-fixture | declares diagnostic bundle API create and download UX without direct host mutation commands |
| 34 | `web.static.operator-terms` | 733-751 | operations-evidence | static-contract | keeps operator surface terms aligned with internal distribution boundary |
| 35 | `web.static.frontend-edge-cases` | 752-808 | operations-evidence | static-contract+browser-fixture | hardens final frontend service edge cases before installed-listener evidence |
| 36 | `web.static.token-rotation` | 809-824 | operations-evidence | static-contract+browser-fixture | declares token rotation operator UX without service token mutation |
| 37 | `web.static.beta-followup` | 825-842 | operations-evidence | static-contract+browser-fixture | declares a beta follow-up status surface without browser-started host mutation |
| 38 | `web.static.monitoring` | 843-859 | operations-evidence | static-contract+browser-fixture | declares read-only monitoring auth and checkpoint warning surfaces |
| 39 | `web.static.network-inventory` | 860-880 | operations-evidence | feature-surface+browser-fixture | declares a read-only network inventory view |
| 40 | `web.static.workflow-polish` | 881-892 | operations-evidence | static-contract | declares P2 operator workflow polish and quality gates |
| 41 | `web.static.javascript-syntax` | 893-897 | typescript-parity | node-check | passes JavaScript syntax validation |
| 42 | `web.static.served-typescript-output` | 898-915 | typescript-parity | served-asset+static-parity | treats the served app.js asset as TypeScript build output |
| 43 | `web.static.typescript-scaffold` | 916-933 | typescript-parity | typescript+static-contract | declares a Phase 25 TypeScript scaffold that owns the served app.js asset |
| 44 | `web.static.typescript-contract-mirror` | 934-956 | typescript-parity | typescript+static-contract | keeps TypeScript source as the Local API contract mirror and served app source |
| 45 | `web.static.parity-manifest` | 957-1002 | typescript-parity | static-parity | ships a generated TypeScript parity manifest for the served static asset |
| 46 | `web.static.user-visible-fixtures` | 1003-1024 | typescript-parity | static-parity | ships user-visible fixture parity snapshots for the TypeScript-owned app.js |
| 47 | `web.static.verifier-wiring` | 1025-1077 | typescript-parity | served-asset+static-parity+browser-fixture+frontend-batches | declares generated static parity and served asset verification scripts |
| 48 | `web.static.generated-parity-alignment` | 1078-1125 | typescript-parity | static-parity+browser-fixture | keeps generated static parity artifacts aligned with the TypeScript-owned served app.js route contract |
| 49 | `web.static.secret-mutation-guard` | 1126-1148 | typescript-parity | static-contract+static-parity | does not place secrets or host mutation command strings in parity scripts, TypeScript source, or generated output |
| 50 | `web.static.no-fabricated-values` | 1149-1206 | typescript-parity | static-contract+browser-fixture | keeps fabricated operational values out of the static console shell |

## Appendix B: Canonical 62-file inventory

The third column is the literal `It` count recorded in
`legacy_contract_count`. Paths are forward-slash normalized.

```text
packaging|packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1|7
packaging|packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1|11
packaging|packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1|90
packaging|packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1|10
packaging|packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1|28
packaging|packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1|8
packaging|packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1|2
packaging|packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1|5
packaging|packaging/windows-desktop-node/tests/PcvCSharpArchitectureGapRegistry.Tests.ps1|10
packaging|packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1|12
packaging|packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1|16
packaging|packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1|61
packaging|packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1|16
packaging|packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1|26
packaging|packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1|1
packaging|packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1|9
packaging|packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1|20
packaging|packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1|7
packaging|packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1|1
packaging|packaging/windows-desktop-node/tests/PcvInstalledLoopbackBootstrapSmoke.Tests.ps1|1
packaging|packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1|1
packaging|packaging/windows-desktop-node/tests/PcvInternalHttpsTlsLifecycleSmoke.Tests.ps1|1
packaging|packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1|5
packaging|packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1|5
packaging|packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1|10
packaging|packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1|21
packaging|packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1|5
packaging|packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1|7
packaging|packaging/windows-desktop-node/tests/PcvRunnerArtifactRootContract.Tests.ps1|3
packaging|packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1|5
packaging|packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvStrictCollection.Tests.ps1|2
packaging|packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1|8
packaging|packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1|4
packaging|packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1|4
packaging|packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1|4
packaging|packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1|2
packaging|packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1|6
packaging|packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1|7
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1|4
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1|5
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1|21
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1|6
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1|10
installer|packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1|3
web|web/tests/PcvDesktopWeb.Static.Tests.ps1|50
```

## Appendix C: Written-spec coverage

| Design requirement | Plan owner |
| --- | --- |
| Exact 50 IDs and ordinal names | Task 3, Appendix A |
| Domain registry and static assertion ownership | Tasks 4-9 |
| Existing owner reuse, cache and argument arrays | Task 2 |
| Positive Node projection exactly 50 | Task 9 |
| 62-row strict manifest and Web mapped state | Task 10, Appendix B |
| Controlled `app-root` Pester/Node failure parity | Task 11 |
| Separate npm command and unchanged `npm test` | Task 12 |
| Local positive/negative parity evidence | Tasks 12-13 |
| CI/Pester/PowerShell/cutover false boundary | Tasks 12-13 |
| Protected workflow/Pester/catalog/current evidence | Global constraints, Task 13 |
| No host/service/MSI/VM mutation | Global constraints, architecture evidence |
| Rollback by focused commits | Commit step in every task |

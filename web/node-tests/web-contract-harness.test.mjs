import assert from "node:assert/strict";
import { EventEmitter } from "node:events";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { PassThrough } from "node:stream";
import test from "node:test";
import { fileURLToPath } from "node:url";
import {
  WEB_CONTRACT_ERROR_CODES,
  WebContractError,
  createWebContractContext
} from "../contracts/web-contract-harness.mjs";
import {
  WEB_STATIC_CONTRACT_METADATA,
  parseLegacyPesterTests,
  validateWebStaticContractMetadata
} from "../contracts/web-static-contracts.mjs";

const actualRepoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");

function fixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-"));
  fs.mkdirSync(path.join(root, "web"), { recursive: true });
  fs.writeFileSync(path.join(root, "web", "sample.txt"), "alpha", "utf8");
  fs.writeFileSync(path.join(root, "web", "second.txt"), "beta", "utf8");
  fs.writeFileSync(path.join(root, "web", "sample.json"), '{"value":1}', "utf8");
  return root;
}

function fixtureRepository() {
  const root = fixture();
  for (const relativePath of [
    "web/scripts/verify-feature-surface-parity.mjs",
    "web/node_modules/typescript/bin/tsc",
    "web/scripts/build-served-asset.mjs",
    "web/scripts/validate-frontend-completion-batches.mjs",
    "web/scripts/regenerate-static-parity.mjs",
    "web/scripts/verify-static-parity.mjs",
    "web/scripts/verify-browser-fixture.mjs",
    "web/tsconfig.json",
    "web/app.js"
  ]) {
    const target = path.join(root, ...relativePath.split("/"));
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.writeFileSync(target, "// owner fixture\n", "utf8");
  }
  return root;
}

function withFixture(run) {
  const root = fixture();
  try {
    return run(root);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

async function withFixtureRepository(run) {
  const root = fixtureRepository();
  try {
    return await run(root);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

function expectWebError(action, code, detail) {
  let error;
  try {
    action();
  } catch (caught) {
    error = caught;
  }
  assert.ok(error instanceof WebContractError);
  assert.equal(error.name, "WebContractError");
  assert.equal(error.code, code);
  assert.equal(error.message, `${code}|${detail}`);
  return error;
}

async function expectWebErrorAsync(action, code) {
  let error;
  try {
    await action();
  } catch (caught) {
    error = caught;
  }
  assert.ok(error instanceof WebContractError);
  assert.equal(error.name, "WebContractError");
  assert.equal(error.code, code);
  assert.match(error.message, new RegExp(`^${code}\\|`));
  return error;
}

function writeServedFixture(root, parts, sourceByPart = new Map()) {
  fs.mkdirSync(path.join(root, "web", "scripts"), { recursive: true });
  fs.mkdirSync(path.join(root, "web", "src", "served"), { recursive: true });
  const declaration = [
    "const servedSourceParts = [",
    ...parts.map((part) => `  ${JSON.stringify(part)},`),
    "];"
  ].join("\n");
  fs.writeFileSync(
    path.join(root, "web", "scripts", "build-served-asset.mjs"),
    declaration,
    "utf8"
  );
  for (const [part, source] of sourceByPart) {
    const target = path.join(root, "web", ...part.split("/"));
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.writeFileSync(target, source, "utf8");
  }
}

function cloneWebStaticContractMetadata() {
  return WEB_STATIC_CONTRACT_METADATA.map((item) => ({
    ...item,
    owners: [...item.owners]
  }));
}

test("metadata matches the checked-in Pester source 50 for 50", () => {
  const source = fs.readFileSync(
    path.join(actualRepoRoot, "web/tests/PcvDesktopWeb.Static.Tests.ps1"),
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

test("legacy parser accepts only literal single- and double-quoted It declarations", () => {
  assert.deepEqual(
    parseLegacyPesterTests([
      "  It 'single quoted name' {",
      'It "double quoted name" {'
    ].join("\n")),
    [
      { name: "single quoted name", line: 1 },
      { name: "double quoted name", line: 2 }
    ]
  );

  for (const source of [
    "It $computedName {",
    "It ('parenthesized name') {",
    "It 'first' 'second' {",
    "It 'unterminated {"
  ]) {
    expectWebError(
      () => parseLegacyPesterTests(source),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "legacy_declarations=ambiguous"
    );
  }
});

test("legacy parser ignores block comments and here-strings across CRLF boundaries", () => {
  const source = [
    "$blockStart = 1 <#",
    "It 'block comment fake' {",
    "#> $blockEnd = 1",
    "<# It 'inline block fake' { #>",
    "$singleHere = @'",
    "It 'single here fake' {",
    "'@",
    '$doubleHere = @"',
    'It "double here fake" {',
    '"@',
    "It 'real single' {",
    'It "real double" {'
  ].join("\r\n");

  assert.deepEqual(parseLegacyPesterTests(source), [
    { name: "real single", line: 11 },
    { name: "real double", line: 12 }
  ]);
});

test("legacy parser keeps indented here-string pseudo-terminators as content", () => {
  const source = [
    "$singleHere = @'",
    "  '@",
    "It 'single here fake after indented marker' {",
    "'@",
    "It 'real after single here-string' {",
    '$doubleHere = @"',
    '  "@',
    'It "double here fake after indented marker" {',
    '"@',
    'It "real after double here-string" {'
  ].join("\r\n");

  assert.deepEqual(parseLegacyPesterTests(source), [
    { name: "real after single here-string", line: 5 },
    { name: "real after double here-string", line: 10 }
  ]);
});

test("legacy parser rejects expandable names but accepts escaped literal dollars", () => {
  for (const source of [
    'It "computed $name" {',
    'It "computed $($name)" {'
  ]) {
    expectWebError(
      () => parseLegacyPesterTests(source),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "legacy_declarations=ambiguous"
    );
  }

  assert.deepEqual(
    parseLegacyPesterTests('It "literal `$name and `$(value)" {'),
    [{ name: "literal $name and $(value)", line: 1 }]
  );
});

test("web static metadata rows and owner arrays are immutable", () => {
  assert.equal(Object.isFrozen(WEB_STATIC_CONTRACT_METADATA), true);
  for (const item of WEB_STATIC_CONTRACT_METADATA) {
    assert.equal(Object.isFrozen(item), true);
    assert.equal(Object.isFrozen(item.owners), true);
  }
  assert.throws(() => {
    WEB_STATIC_CONTRACT_METADATA[0].owners.push("node-check");
  }, TypeError);
});

test("web static metadata validation rejects shape, allowlist, uniqueness, and ordering defects", () => {
  assert.doesNotThrow(() => validateWebStaticContractMetadata(WEB_STATIC_CONTRACT_METADATA));

  const defects = [
    cloneWebStaticContractMetadata().slice(0, -1),
    Object.assign(cloneWebStaticContractMetadata(), { 0: { ...cloneWebStaticContractMetadata()[0], id: "web.static.Invalid" } }),
    Object.assign(cloneWebStaticContractMetadata(), { 1: { ...cloneWebStaticContractMetadata()[1], id: WEB_STATIC_CONTRACT_METADATA[0].id } }),
    Object.assign(cloneWebStaticContractMetadata(), { 1: { ...cloneWebStaticContractMetadata()[1], legacyName: WEB_STATIC_CONTRACT_METADATA[0].legacyName } }),
    Object.assign(cloneWebStaticContractMetadata(), { 0: { ...cloneWebStaticContractMetadata()[0], domain: "unsupported-domain" } }),
    Object.assign(cloneWebStaticContractMetadata(), { 0: { ...cloneWebStaticContractMetadata()[0], owners: ["unsupported-owner"] } }),
    Object.assign(cloneWebStaticContractMetadata(), { 0: { ...cloneWebStaticContractMetadata()[0], legacyLines: "54-43" } }),
    [cloneWebStaticContractMetadata()[1], cloneWebStaticContractMetadata()[0], ...cloneWebStaticContractMetadata().slice(2)],
    Object.assign(cloneWebStaticContractMetadata(), { 0: { ...cloneWebStaticContractMetadata()[0], id: "web.static.valid-but-not-canonical" } })
  ];

  for (const metadata of defects) {
    expectWebError(
      () => validateWebStaticContractMetadata(metadata),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "metadata=invalid"
    );
  }
});

test("context reads contained repository files and combines them in order", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    assert.equal(context.repoPath("web/sample.txt"), path.join(fs.realpathSync(root), "web", "sample.txt"));
    assert.equal(context.readText("web/sample.txt"), "alpha");
    assert.equal(context.readCombined(["web/sample.txt", "web/second.txt"]), "alpha\nbeta");
  });
});

test("context rejects empty, absolute, drive-qualified, NUL, and lexical parent paths", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    for (const relativePath of [
      "",
      ".",
      "/absolute.txt",
      "\\\\server\\share\\outside.txt",
      "C:/outside.txt",
      "C:outside.txt",
      "web/\0outside.txt"
    ]) {
      expectWebError(
        () => context.repoPath(relativePath),
        WEB_CONTRACT_ERROR_CODES.configInvalid,
        "path=invalid"
      );
    }
    for (const relativePath of ["../outside.txt", "web/../sample.txt"]) {
      expectWebError(
        () => context.readText(relativePath),
        WEB_CONTRACT_ERROR_CODES.configInvalid,
        "path=escape"
      );
    }
  });
});

test("context rejects an existing symbolic-link or reparse-point escape before reading", () => {
  const root = fixture();
  const outside = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-outside-"));
  try {
    fs.writeFileSync(path.join(outside, "secret.txt"), "secret", "utf8");
    fs.symlinkSync(outside, path.join(root, "web", "escape-link"), process.platform === "win32" ? "junction" : "dir");
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readText("web/escape-link/secret.txt"),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "path=escape"
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { recursive: true, force: true });
  }
});

test("missing disk files project a stable relative-path error", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    const error = expectWebError(
      () => context.readText("web/missing.txt"),
      WEB_CONTRACT_ERROR_CODES.fileMissing,
      "path=web/missing.txt"
    );
    assert.equal(error.message.includes(root), false);
  });
});

test("missingPaths wins over a text override", () => {
  withFixture((root) => {
    const context = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web/sample.txt", "override"]]),
      missingPaths: new Set(["web/sample.txt"])
    });
    expectWebError(
      () => context.readText("web/sample.txt"),
      WEB_CONTRACT_ERROR_CODES.fileMissing,
      "path=web/sample.txt"
    );
  });
});

test("text and parsed JSON use separate stable caches", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    assert.equal(context.readText("web/sample.txt"), "alpha");
    fs.writeFileSync(path.join(root, "web", "sample.txt"), "changed", "utf8");
    assert.equal(context.readText("web/sample.txt"), "alpha");

    const firstJson = context.readJson("web/sample.json");
    fs.writeFileSync(path.join(root, "web", "sample.json"), '{"value":2}', "utf8");
    const secondJson = context.readJson("web/sample.json");
    assert.strictEqual(firstJson, secondJson);
    assert.deepEqual(secondJson, { value: 1 });
    assert.equal(context.readText("web/sample.json"), '{"value":1}');
  });
});

test("readJson revalidates containment before returning a cached object", () => {
  const root = fixture();
  const outside = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-json-outside-"));
  const dataDirectory = path.join(root, "web", "json-data");
  try {
    fs.mkdirSync(dataDirectory, { recursive: true });
    fs.writeFileSync(path.join(dataDirectory, "cached.json"), '{"value":1}', "utf8");
    const context = createWebContractContext({ repoRoot: root });
    assert.deepEqual(context.readJson("web/json-data/cached.json"), { value: 1 });

    fs.rmSync(dataDirectory, { recursive: true, force: true });
    fs.writeFileSync(path.join(outside, "cached.json"), '{"value":2}', "utf8");
    fs.symlinkSync(
      outside,
      dataDirectory,
      process.platform === "win32" ? "junction" : "dir"
    );

    expectWebError(
      () => context.readJson("web/json-data/cached.json"),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "path=escape"
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { recursive: true, force: true });
  }
});

test("text overrides and caches are isolated between contexts", () => {
  withFixture((root) => {
    const firstOverrides = new Map([["web/sample.txt", "first"]]);
    const first = createWebContractContext({ repoRoot: root, textOverrides: firstOverrides });
    const second = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web/sample.txt", "second"]])
    });
    firstOverrides.set("web/sample.txt", "mutated-after-create");
    assert.equal(first.readText("web/sample.txt"), "first");
    assert.equal(second.readText("web/sample.txt"), "second");
  });
});

test("path inputs normalize to forward slashes while only normalized override and missing keys apply", () => {
  withFixture((root) => {
    const ignoredKeys = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web\\sample.txt", "ignored"]]),
      missingPaths: new Set(["web\\second.txt"])
    });
    assert.equal(ignoredKeys.readText("web\\.\\sample.txt"), "alpha");
    assert.equal(ignoredKeys.readText("web\\second.txt"), "beta");

    const normalizedKeys = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web/sample.txt", "normalized"]]),
      missingPaths: new Set(["web/second.txt"])
    });
    assert.equal(normalizedKeys.readText("web\\.\\sample.txt"), "normalized");
    expectWebError(
      () => normalizedKeys.readText("web\\second.txt"),
      WEB_CONTRACT_ERROR_CODES.fileMissing,
      "path=web/second.txt"
    );
  });
});

test("forContract shares parent text and JSON caches", () => {
  withFixture((root) => {
    const parent = createWebContractContext({ repoRoot: root });
    const cachedText = parent.readText("web/sample.txt");
    const cachedJson = parent.readJson("web/sample.json");
    const scoped = parent.forContract("web.static.cache-sharing");
    fs.writeFileSync(path.join(root, "web", "sample.txt"), "changed", "utf8");
    fs.writeFileSync(path.join(root, "web", "sample.json"), '{"value":2}', "utf8");
    assert.equal(scoped.readText("web/sample.txt"), cachedText);
    assert.strictEqual(scoped.readJson("web/sample.json"), cachedJson);
  });
});

test("forContract rejects unsafe IDs before any file read", () => {
  withFixture((root) => {
    const context = createWebContractContext({
      repoRoot: root,
      missingPaths: new Set(["web/sample.txt"])
    });
    for (const contractId of ["", "..", "unsafe/id", "unsafe\\id", "has space"]) {
      expectWebError(
        () => context.forContract(contractId),
        WEB_CONTRACT_ERROR_CODES.configInvalid,
        "contract_id=invalid"
      );
    }
  });
});

test("scoped assertion failures carry the runtime contract_id field", () => {
  withFixture((root) => {
    const scoped = createWebContractContext({ repoRoot: root }).forContract("web.static.example");
    const error = expectWebError(
      () => scoped.assertEqual("actual", "expected", "scoped-equality"),
      WEB_CONTRACT_ERROR_CODES.assertionFailed,
      "assertion=scoped-equality"
    );
    assert.equal(error.contract_id, "web.static.example");
  });
});

test("every assertion helper emits the stable label-based failure", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    const failures = [
      ["exists", () => context.assertExists("web/missing.txt", "exists")],
      ["match", () => context.assertMatch("alpha", /omega/, "match")],
      ["not-match", () => context.assertNotMatch("alpha", /alpha/, "not-match")],
      ["equal", () => context.assertEqual(1, 2, "equal")],
      ["includes", () => context.assertIncludes(["alpha"], "omega", "includes")],
      ["before", () => context.assertBefore("second then first", "first", "second", "before")]
    ];
    for (const [label, action] of failures) {
      const error = expectWebError(
        action,
        WEB_CONTRACT_ERROR_CODES.assertionFailed,
        `assertion=${label}`
      );
      assert.equal(Object.hasOwn(error, "contract_id"), false);
      assert.equal(error.message.includes(root), false);
    }
  });
});

test("every assertion helper accepts its successful case", () => {
  withFixture((root) => {
    const context = createWebContractContext({ repoRoot: root });
    assert.doesNotThrow(() => context.assertExists("web/sample.txt", "exists"));
    assert.doesNotThrow(() => context.assertMatch("alpha", /^alpha$/, "match"));
    assert.doesNotThrow(() => context.assertNotMatch("alpha", /omega/, "not-match"));
    assert.doesNotThrow(() => context.assertEqual("same", "same", "equal"));
    assert.doesNotThrow(() => context.assertIncludes(["alpha", "beta"], "beta", "includes"));
    assert.doesNotThrow(() => context.assertBefore("first then second", "first", "second", "before"));
  });
});

test("assertExists rejects an external reparse escape before honoring an override", () => {
  const root = fixture();
  const outside = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-exists-override-"));
  try {
    fs.writeFileSync(path.join(outside, "virtual.txt"), "outside", "utf8");
    fs.symlinkSync(
      outside,
      path.join(root, "web", "override-link"),
      process.platform === "win32" ? "junction" : "dir"
    );
    const context = createWebContractContext({
      repoRoot: root,
      textOverrides: new Map([["web/override-link/virtual.txt", "override"]])
    });
    expectWebError(
      () => context.assertExists("web/override-link/virtual.txt", "override-escape"),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "path=escape"
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { recursive: true, force: true });
  }
});

test("assertExists revalidates containment before honoring a text cache", () => {
  const root = fixture();
  const outside = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-harness-exists-cache-"));
  const dataDirectory = path.join(root, "web", "cached-exists");
  try {
    fs.mkdirSync(dataDirectory, { recursive: true });
    fs.writeFileSync(path.join(dataDirectory, "sample.txt"), "inside", "utf8");
    const context = createWebContractContext({ repoRoot: root });
    assert.equal(context.readText("web/cached-exists/sample.txt"), "inside");

    fs.rmSync(dataDirectory, { recursive: true, force: true });
    fs.writeFileSync(path.join(outside, "sample.txt"), "outside", "utf8");
    fs.symlinkSync(
      outside,
      dataDirectory,
      process.platform === "win32" ? "junction" : "dir"
    );

    expectWebError(
      () => context.assertExists("web/cached-exists/sample.txt", "cache-escape"),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "path=escape"
    );
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { recursive: true, force: true });
  }
});

test("filesystem failures retain only a stable sanitized cause", () => {
  const root = fixture();
  try {
    const error = expectWebError(
      () => createWebContractContext({ repoRoot: path.join(root, "missing-root") }),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "repo_root=invalid"
    );
    assert.ok(error.cause instanceof Error);
    assert.equal(error.cause.name, "FilesystemError");
    assert.equal(error.cause.message, "ENOENT");
    assert.equal(error.cause.stack, "FilesystemError: ENOENT");
    for (const exposedText of [error.message, error.cause.message, error.cause.stack]) {
      const normalizedText = exposedText.replaceAll("\\", "/");
      for (const sensitivePath of [root, actualRepoRoot, os.homedir()]) {
        assert.equal(
          normalizedText.includes(sensitivePath.replaceAll("\\", "/")),
          false
        );
      }
    }
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("invalid JSON retains only a stable sanitized cause", () => {
  withFixture((root) => {
    fs.writeFileSync(path.join(root, "web", "invalid.json"), `${root} is not JSON`, "utf8");
    const context = createWebContractContext({ repoRoot: root });
    const error = expectWebError(
      () => context.readJson("web/invalid.json"),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "json=invalid:web/invalid.json"
    );
    assert.ok(error.cause instanceof SyntaxError);
    assert.equal(error.cause.message, "invalid_json");
    assert.equal(error.cause.stack, "SyntaxError: invalid_json");
    for (const exposedText of [error.message, error.cause.message, error.cause.stack]) {
      assert.equal(exposedText.includes(root), false);
    }
  });
});

test("readServedSource concatenates the exact declared source-part order", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/second.ts", "src/served/first.ts", "src/served-app.ts"],
      new Map([
        ["src/served/first.ts", "first-source"],
        ["src/served/second.ts", "second-source"],
        ["src/served-app.ts", "app-source"]
      ])
    );
    const context = createWebContractContext({ repoRoot: root });
    assert.equal(context.readServedSource(), "second-source\nfirst-source\napp-source");
  });
});

test("readServedSource includes the actual repository terminal served app source", () => {
  const context = createWebContractContext({ repoRoot: actualRepoRoot });
  const combined = context.readServedSource();
  const servedApp = context.readText("web/src/served-app.ts");
  assert.notEqual(servedApp.length, 0);
  assert.equal(combined.slice(-servedApp.length), servedApp);
});

test("readServedSource ignores commented and quoted declaration decoys", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/real.ts", "src/served-app.ts"],
      new Map([
        ["src/served/line-decoy.ts", "line-decoy"],
        ["src/served/real.ts", "real-source"],
        ["src/served-app.ts", "app-source"]
      ])
    );
    const script = [
      '// const servedSourceParts = ["src/served/line-decoy.ts", "src/served-app.ts"];',
      '/* const servedSourceParts = ["src/served/block-decoy.ts", "src/served-app.ts"]; */',
      String.raw`const quotedDecoy = "escaped \" const servedSourceParts = [\"src/served/quoted-decoy.ts\", \"src/served-app.ts\"];";`,
      'const templateDecoy = `const servedSourceParts = ["src/served/template-decoy.ts", "src/served-app.ts"];`;',
      "const servedSourceParts = [",
      '  "src/served/real.ts",',
      '  "src/served-app.ts",',
      "];"
    ].join("\n");
    fs.writeFileSync(
      path.join(root, "web", "scripts", "build-served-asset.mjs"),
      script,
      "utf8"
    );
    const context = createWebContractContext({ repoRoot: root });
    assert.equal(context.readServedSource(), "real-source\napp-source");
  });
});

test("readServedSource rejects duplicate active declarations", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/first.ts", "src/served-app.ts"],
      new Map([
        ["src/served/first.ts", "first-source"],
        ["src/served/second.ts", "second-source"],
        ["src/served-app.ts", "app-source"]
      ])
    );
    const script = [
      'const servedSourceParts = ["src/served/first.ts", "src/served-app.ts"];',
      'const servedSourceParts = ["src/served/second.ts", "src/served-app.ts"];'
    ].join("\n");
    fs.writeFileSync(
      path.join(root, "web", "scripts", "build-served-asset.mjs"),
      script,
      "utf8"
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects a non-terminal served app source", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served-app.ts", "src/served/after.ts"],
      new Map([
        ["src/served-app.ts", "app-source"],
        ["src/served/after.ts", "after-source"]
      ])
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects staged parts without the terminal served app", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/a.ts"],
      new Map([["src/served/a.ts", "a-source"]])
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects a terminal served app without a staged part", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served-app.ts"],
      new Map([["src/served-app.ts", "app-source"]])
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects an empty declared source-part sequence", () => {
  withFixture((root) => {
    writeServedFixture(root, []);
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=empty"
    );
  });
});

test("readServedSource rejects duplicate declared source parts", () => {
  withFixture((root) => {
    writeServedFixture(root, ["src/served/duplicate.ts", "src/served/duplicate.ts"]);
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=duplicate:src/served/duplicate.ts"
    );
  });
});

test("readServedSource rejects a leading array hole", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/a.ts"],
      new Map([["src/served/a.ts", "a-source"]])
    );
    fs.writeFileSync(
      path.join(root, "web", "scripts", "build-served-asset.mjs"),
      'const servedSourceParts = [, "src/served/a.ts"];',
      "utf8"
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects a repeated array separator", () => {
  withFixture((root) => {
    writeServedFixture(
      root,
      ["src/served/a.ts", "src/served/b.ts"],
      new Map([
        ["src/served/a.ts", "a-source"],
        ["src/served/b.ts", "b-source"]
      ])
    );
    fs.writeFileSync(
      path.join(root, "web", "scripts", "build-served-asset.mjs"),
      'const servedSourceParts = ["src/served/a.ts",, "src/served/b.ts"];',
      "utf8"
    );
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("readServedSource rejects an escaping declared source part", () => {
  withFixture((root) => {
    writeServedFixture(root, ["src/served/valid.ts", "src/served/../escape.ts"]);
    const context = createWebContractContext({ repoRoot: root });
    expectWebError(
      () => context.readServedSource(),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "served_source_parts=invalid"
    );
  });
});

test("owner execution uses direct Node arguments and caches by owner id", async () => {
  await withFixtureRepository(async (root) => {
    const calls = [];
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: async (request) => {
        calls.push(request);
        return { exitCode: 0, signal: null, timedOut: false, stdout: "ok", stderr: "" };
      }
    });

    const result = await context.runOwners(["served-asset", "served-asset"]);
    for (const ownerId of [
      "feature-surface",
      "typescript",
      "frontend-batches",
      "static-parity",
      "browser-fixture",
      "node-check",
      "static-contract"
    ]) {
      await context.runOwners([ownerId, ownerId]);
    }
    await context.forContract("web.owner.cached").runOwners([
      "served-asset",
      "feature-surface",
      "typescript",
      "frontend-batches",
      "static-parity",
      "browser-fixture",
      "node-check",
      "static-contract"
    ]);

    assert.equal(result, undefined);
    assert.deepEqual(
      calls.map((call) => call.arguments),
      [
        ["scripts/build-served-asset.mjs", "--check"],
        ["scripts/verify-feature-surface-parity.mjs"],
        ["node_modules/typescript/bin/tsc", "--noEmit", "-p", "tsconfig.json"],
        ["scripts/validate-frontend-completion-batches.mjs"],
        ["scripts/regenerate-static-parity.mjs", "--check"],
        ["scripts/verify-static-parity.mjs"],
        ["scripts/verify-browser-fixture.mjs"],
        ["--check", "app.js"]
      ]
    );
    const expectedWebRoot = path.join(fs.realpathSync(root), "web");
    for (const call of calls) {
      assert.equal(call.fileName, process.execPath);
      assert.equal(call.cwd, expectedWebRoot);
      assert.equal(call.shell, false);
      assert.equal(call.windowsHide, true);
      assert.equal(call.timeoutMs, 120_000);
      assert.equal(call.stdin, "ignore");
    }
    assert.equal(Object.hasOwn(context, "stdout"), false);
    assert.equal(Object.hasOwn(context, "stderr"), false);
  });
});

test("genuinely concurrent owner calls share one cached Promise", async () => {
  await withFixtureRepository(async (root) => {
    const calls = [];
    let release;
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: (request) => {
        calls.push(request);
        return new Promise((resolve) => {
          release = resolve;
        });
      }
    });

    const first = context.runOwners(["browser-fixture"]);
    const second = context.forContract("web.owner.concurrent").runOwners(["browser-fixture"]);
    await Promise.resolve();
    assert.equal(calls.length, 1);
    release({ exitCode: 0, signal: null, timedOut: false, stdout: "one", stderr: "two" });
    await Promise.all([first, second]);
    assert.equal(calls.length, 1);
  });
});

test("failed owner Promises are cached and projected independently for scoped callers", async () => {
  await withFixtureRepository(async (root) => {
    const calls = [];
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: async (request) => {
        calls.push(request);
        return { exitCode: 9, signal: null, timedOut: false, stdout: "failed", stderr: "" };
      }
    });

    const first = await expectWebErrorAsync(
      () => context.forContract("web.owner.first").runOwners(["served-asset"]),
      WEB_CONTRACT_ERROR_CODES.ownerFailed
    );
    const second = await expectWebErrorAsync(
      () => context.forContract("web.owner.second").runOwners(["served-asset"]),
      WEB_CONTRACT_ERROR_CODES.ownerFailed
    );

    assert.equal(calls.length, 1);
    assert.notStrictEqual(first, second);
    assert.equal(first.contract_id, "web.owner.first");
    assert.equal(second.contract_id, "web.owner.second");
    assert.equal(first.contract_id, "web.owner.first");
  });
});

test("invalid owner configuration fails closed without invoking the runner", async () => {
  await withFixtureRepository(async (root) => {
    const calls = [];
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: async (request) => {
        calls.push(request);
        return { exitCode: 0, signal: null, timedOut: false, stdout: "", stderr: "" };
      }
    });

    for (const ownerIds of ["served-asset", null, {}, [42], [""], ["not-an-owner"]]) {
      await expectWebErrorAsync(
        () => context.runOwners(ownerIds),
        WEB_CONTRACT_ERROR_CODES.configInvalid
      );
    }
    assert.equal(calls.length, 0);
    expectWebError(
      () => createWebContractContext({ repoRoot: root, processRunner: null }),
      WEB_CONTRACT_ERROR_CODES.configInvalid,
      "process_runner=invalid"
    );
  });
});

test("owner targets must be real contained files before runner invocation", async () => {
  const root = fixtureRepository();
  const outside = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-web-owner-outside-"));
  try {
    fs.writeFileSync(path.join(outside, "build-served-asset.mjs"), "// outside\n", "utf8");
    fs.rmSync(path.join(root, "web", "scripts"), { recursive: true, force: true });
    fs.symlinkSync(
      outside,
      path.join(root, "web", "scripts"),
      process.platform === "win32" ? "junction" : "dir"
    );
    const calls = [];
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: async (request) => {
        calls.push(request);
        return { exitCode: 0, signal: null, timedOut: false, stdout: "", stderr: "" };
      }
    });
    const error = await expectWebErrorAsync(
      () => context.runOwners(["served-asset"]),
      WEB_CONTRACT_ERROR_CODES.configInvalid
    );
    assert.equal(error.message, `${WEB_CONTRACT_ERROR_CODES.configInvalid}|path=escape`);
    assert.equal(calls.length, 0);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
    fs.rmSync(outside, { recursive: true, force: true });
  }

  await withFixtureRepository(async (containedRoot) => {
    fs.rmSync(path.join(containedRoot, "web", "app.js"));
    const calls = [];
    const context = createWebContractContext({
      repoRoot: containedRoot,
      processRunner: async (request) => {
        calls.push(request);
        return { exitCode: 0, signal: null, timedOut: false, stdout: "", stderr: "" };
      }
    });
    await expectWebErrorAsync(
      () => context.runOwners(["node-check"]),
      WEB_CONTRACT_ERROR_CODES.configInvalid
    );
    assert.equal(calls.length, 0);
  });
});

test("all owner termination failures project the stable owner error", async () => {
  await withFixtureRepository(async (root) => {
    const scenarios = [
      {
        id: "nonzero",
        run: async () => ({
          exitCode: 7,
          signal: null,
          timedOut: false,
          stdout: "",
          stderr: "nonzero"
        })
      },
      {
        id: "signal",
        run: async () => ({
          exitCode: null,
          signal: "SIGTERM",
          timedOut: false,
          stdout: "",
          stderr: "signal"
        })
      },
      {
        id: "timeout",
        run: async () => ({
          exitCode: null,
          signal: "SIGTERM",
          timedOut: true,
          stdout: "",
          stderr: "timeout"
        })
      },
      {
        id: "rejection",
        run: async () => {
          throw new Error("runner rejected");
        }
      }
    ];

    for (const scenario of scenarios) {
      const scoped = createWebContractContext({
        repoRoot: root,
        processRunner: scenario.run
      }).forContract(`web.owner.${scenario.id}`);
      const error = await expectWebErrorAsync(
        () => scoped.runOwners(["feature-surface"]),
        WEB_CONTRACT_ERROR_CODES.ownerFailed
      );
      assert.equal(error.contract_id, `web.owner.${scenario.id}`);
    }
  });
});

test("owner failure output is redacted before its combined 8192 character cap", async () => {
  await withFixtureRepository(async (root) => {
    const nativeRoot = fs.realpathSync(root);
    const nativeHome = os.homedir();
    const secrets = [
      "bearer-secret.ABC+/=-",
      "password-secret",
      "access-secret",
      "refresh-secret",
      "api-secret",
      "basic-secret-token",
      "digest-secret-value",
      "proxy-secret-token",
      "header-api-secret"
    ];
    const stdout = [
      `Bearer ${secrets[0]}`,
      `password=${secrets[1]}`,
      `access_token: ${secrets[2]}`,
      `Authorization: Basic ${secrets[5]}`,
      `Proxy-Authorization: Basic ${secrets[7]}`,
      nativeRoot,
      nativeRoot.replaceAll("\\", "/")
    ].join("\n");
    const stderr = [
      `refresh_token = ${secrets[3]}`,
      `api_token=${secrets[4]}`,
      `Authorization: Digest username="operator", response="${secrets[6]}"`,
      `X-Api-Key: ${secrets[8]}`,
      nativeHome,
      nativeHome.replaceAll("\\", "/"),
      "x".repeat(20_000)
    ].join("\n");
    const context = createWebContractContext({
      repoRoot: root,
      processRunner: async () => ({
        exitCode: 1,
        signal: null,
        timedOut: false,
        stdout,
        stderr
      })
    });

    const error = await expectWebErrorAsync(
      () => context.runOwners(["typescript"]),
      WEB_CONTRACT_ERROR_CODES.ownerFailed
    );
    const outputMarker = "|output=";
    const markerIndex = error.message.indexOf(outputMarker);
    assert.notEqual(markerIndex, -1);
    const exposedOutput = error.message.slice(markerIndex + outputMarker.length);
    assert.ok(exposedOutput.length <= 8192);
    assert.match(exposedOutput, /Bearer \[REDACTED\]/);
    assert.match(exposedOutput, /password=\[REDACTED\]/);
    assert.match(exposedOutput, /access_token: \[REDACTED\]/);
    assert.match(exposedOutput, /refresh_token = \[REDACTED\]/);
    assert.match(exposedOutput, /api_token=\[REDACTED\]/);
    assert.match(exposedOutput, /Authorization: \[REDACTED\]/);
    assert.match(exposedOutput, /Proxy-Authorization: \[REDACTED\]/);
    assert.match(exposedOutput, /X-Api-Key: \[REDACTED\]/);
    assert.match(exposedOutput, /\[REDACTED_PATH\]/);
    for (const secret of secrets) {
      assert.equal(error.message.includes(secret), false);
    }
    const normalizedError = error.message.replaceAll("\\", "/");
    assert.equal(normalizedError.includes(nativeRoot.replaceAll("\\", "/")), false);
    assert.equal(normalizedError.includes(nativeHome.replaceAll("\\", "/")), false);
  });
});

test("default owner runner force-settles a non-closing child after timeout grace", async () => {
  await withFixtureRepository(async (root) => {
    const stdout = new PassThrough();
    const stderr = new PassThrough();
    const child = new EventEmitter();
    const kills = [];
    const spawnCalls = [];
    const timers = [];
    let runtimeBound = false;
    child.stdout = stdout;
    child.stderr = stderr;
    child.kill = (signal = "SIGTERM") => {
      kills.push(signal);
      return false;
    };

    const ownerProcessRuntime = {
      get spawnProcess() {
        runtimeBound = true;
        return (fileName, arguments_, options) => {
          spawnCalls.push({ fileName, arguments: arguments_, options });
          return child;
        };
      },
      setTimer(callback, delay) {
        const timer = { callback, delay, cleared: false };
        timers.push(timer);
        return timer;
      },
      clearTimer(timer) {
        timer.cleared = true;
      }
    };
    const context = createWebContractContext({ repoRoot: root, ownerProcessRuntime });

    assert.equal(runtimeBound, true, "the default runner must bind the injected runtime seam");
    const pending = context.runOwners(["served-asset"]);
    let settled = false;
    pending.then(
      () => { settled = true; },
      () => { settled = true; }
    );
    await Promise.resolve();

    assert.equal(spawnCalls.length, 1);
    assert.equal(spawnCalls[0].fileName, process.execPath);
    assert.deepEqual(spawnCalls[0].arguments, ["scripts/build-served-asset.mjs", "--check"]);
    assert.deepEqual(spawnCalls[0].options, {
      cwd: path.join(fs.realpathSync(root), "web"),
      shell: false,
      windowsHide: true,
      stdio: ["ignore", "pipe", "pipe"]
    });
    assert.equal(timers.length, 1);
    assert.equal(timers[0].delay, 120_000);
    stdout.write("captured stdout");
    stderr.write("captured stderr");

    timers[0].callback();
    assert.deepEqual(kills, ["SIGTERM"]);
    assert.equal(timers.length, 2);
    assert.equal(timers[1].delay, 1_000);
    assert.equal(settled, false);
    assert.equal(stdout.destroyed, false);
    assert.equal(stderr.destroyed, false);

    timers[1].callback();
    const error = await expectWebErrorAsync(
      () => pending,
      WEB_CONTRACT_ERROR_CODES.ownerFailed
    );
    assert.match(error.message, /timed_out=true/);
    assert.deepEqual(kills, ["SIGTERM", "SIGKILL"]);
    assert.equal(settled, true);
    assert.equal(stdout.destroyed, true);
    assert.equal(stderr.destroyed, true);
    assert.equal(stdout.listenerCount("data"), 0);
    assert.equal(stderr.listenerCount("data"), 0);
    assert.equal(child.listenerCount("error"), 0);
    assert.equal(child.listenerCount("close"), 0);
    assert.equal(timers.every((timer) => timer.cleared), true);

    child.emit("close", 0, null);
    assert.equal(settled, true);
  });
});

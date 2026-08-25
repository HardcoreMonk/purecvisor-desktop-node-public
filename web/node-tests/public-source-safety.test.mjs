import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const verifierUrl = new URL("../scripts/verify-public-source-safety.mjs", import.meta.url);
const verifierPath = fileURLToPath(verifierUrl);
const packageJson = JSON.parse(fs.readFileSync(new URL("../package.json", import.meta.url), "utf8"));
const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");

test("exposes the public source safety scanner API", async () => {
  assert.equal(fs.existsSync(verifierUrl), true, "public source safety verifier must exist");
  const verifier = await import(verifierUrl.href);
  assert.equal(typeof verifier.scanPublicSourceTree, "function");
});

function writeFixture(files) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-public-source-safety-"));
  for (const [relativePath, content] of Object.entries(files)) {
    const absolutePath = path.join(root, ...relativePath.split("/"));
    fs.mkdirSync(path.dirname(absolutePath), { recursive: true });
    fs.writeFileSync(absolutePath, content, "utf8");
  }
  return root;
}

async function scanFixture(files, options = {}) {
  const root = writeFixture(files);
  try {
    const verifier = await import(verifierUrl.href);
    return verifier.scanPublicSourceTree({
      repositoryRoot: root,
      trackedPaths: Object.keys(files),
      ...options
    });
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

function ruleIds(result) {
  return result.findings.map(({ ruleId }) => ruleId);
}

test("accepts public placeholders and marked synthetic network fixtures", async () => {
  const syntheticAddress = [192, 168, 56, 10].join(".");
  const result = await scanFixture({
    "docs/example.md": "Profile: C:/Users/Operator/project\nEndpoint: 192.0.2.10\n",
    "src/NetworkFixture.cs": `// public-safety: synthetic-rfc1918\nconst string TestAddress = "${syntheticAddress}";\n`
  });

  assert.deepEqual(result.findings, []);
});

test("accepts a public profile placeholder inside Markdown delimiters", async () => {
  const result = await scanFixture({
    "docs/example.md": "Use `C:/Users/Operator` or (C:\\Users\\Public) for examples.\n"
  });
  assert.deepEqual(result.findings, []);
});

test("rejects absolute personal profiles and configured operator identities", async () => {
  const privateIdentity = ["Private", "Operator"].join("");
  const privateProfile = ["C:", "Users", privateIdentity, "project"].join("\\");
  const result = await scanFixture({
    "docs/evidence.md": `Owner=${privateIdentity}\nPath=${privateProfile}\n`
  }, { forbiddenIdentities: [privateIdentity] });

  assert.deepEqual(ruleIds(result), [
    "identity.absolute-profile",
    "identity.forbidden-token"
  ]);
});

test("rejects observed private endpoints and unmarked synthetic literals", async () => {
  const privateAddress = [10, 42, 0, 7].join(".");
  const result = await scanFixture({
    "docs/ga-ready/evidence/host.md": `Observed endpoint: ${privateAddress}\n`,
    "src/NetworkFixture.cs": `const string Address = "${privateAddress}";\n`
  });

  assert.deepEqual(ruleIds(result), [
    "network.observed-private-endpoint",
    "network.synthetic-marker-missing"
  ]);
});

test("rejects credential URLs, private keys, personal email, and private hostnames", async () => {
  const credentialUrl = ["https://", "operator", ":", "credential", "@service.example"].join("");
  const keyBlock = ["-----BEGIN ", "PRIVATE KEY-----", "\ninvalid\n", "-----END ", "PRIVATE KEY-----"].join("");
  const personalEmail = ["person", "@", "workstation", ".internal"].join("");
  const privateHost = ["https://node", ".corp", "/status"].join("");
  const result = await scanFixture({
    "config/runtime.txt": `${credentialUrl}\n${keyBlock}\n${personalEmail}\n${privateHost}\n`
  });

  assert.deepEqual(ruleIds(result), [
    "identity.personal-email",
    "network.private-hostname",
    "secret.credential-url",
    "secret.private-key"
  ]);
});

test("accepts a synthetic local schema identifier but not a runtime endpoint", async () => {
  const schemaHost = ["contract", ".local"].join("");
  const result = await scanFixture({
    "config/example.schema.json": JSON.stringify({
      $id: `https://${schemaHost}/schemas/example-v1`,
      endpoint: `https://${schemaHost}/runtime`
    }, null, 2)
  });

  assert.deepEqual(ruleIds(result), ["network.private-hostname"]);
  assert.equal(result.findings[0].line, 3);
});

test("does not let a same-line schema identifier hide a private runtime endpoint", async () => {
  const schemaHost = ["contract", ".local"].join("");
  const result = await scanFixture({
    "config/minified.schema.json": JSON.stringify({
      $id: `https://${schemaHost}/schemas/example-v1`,
      endpoint: `https://${schemaHost}/runtime`
    })
  });

  assert.deepEqual(ruleIds(result), ["network.private-hostname"]);
});

test("allows the public no-reply author address but rejects other personal email", async () => {
  const personalEmail = ["person", "@", "mailhost", ".example"].join("");
  const result = await scanFixture({
    "docs/contacts.md": [
      "254846378+HardcoreMonk@users.noreply.github.com",
      personalEmail
    ].join("\n")
  });

  assert.deepEqual(ruleIds(result), ["identity.personal-email"]);
  assert.equal(result.findings[0].line, 2);
});

test("rejects the private archive provider identifier without rejecting the public target", async () => {
  const owner = "HardcoreMonk";
  const product = ["purecvisor", "desktop", "node"].join("-");
  const privateProvider = [owner, product].join("/");
  const publicProvider = `${privateProvider}-public`;
  const result = await scanFixture({
    "docs/providers.md": `archive=${privateProvider}\nauthority=${publicProvider}\n`
  });

  assert.deepEqual(ruleIds(result), ["provider.private-archive"]);
});

test("rejects nested Git metadata, path escapes, and unexpected credential archives", async () => {
  const result = await scanFixture({
    ".GiT/config": "[core]\n",
    "secrets/operator.pfx": "not-a-real-certificate\n"
  }, { additionalTrackedPaths: ["../outside.txt"] });

  assert.deepEqual(ruleIds(result), [
    "repository.binary-archive",
    "repository.nested-git",
    "repository.path-escape"
  ]);
});

test("rejects a tracked directory that can represent a Git submodule", async () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-public-source-safety-gitlink-"));
  try {
    fs.mkdirSync(path.join(root, "vendor", "module"), { recursive: true });
    const verifier = await import(verifierUrl.href);
    const result = verifier.scanPublicSourceTree({
      repositoryRoot: root,
      trackedPaths: ["vendor/module"]
    });
    assert.deepEqual(ruleIds(result), ["repository.gitlink-or-directory"]);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("requires rights, security, and authority boundary documents", async () => {
  const missing = await scanFixture({ "README.md": "Project\n" }, { requireBoundaryDocuments: true });
  assert.deepEqual(ruleIds(missing), [
    "boundary.authority-missing",
    "boundary.license-missing",
    "boundary.security-missing"
  ]);

  const valid = await scanFixture({
    "LICENSE": "All rights reserved. No permission is granted to reproduce, modify, redistribute, sublicense, or sell.\n",
    "SECURITY.md": "Report security issues through GitHub private vulnerability reporting.\n",
    "docs/PUBLIC_SOURCE_AUTHORITY.md": [
      "HardcoreMonk/purecvisor-desktop-node-public",
      "public_trusted_signing=false",
      "external_stable_publication=false"
    ].join("\n")
  }, { requireBoundaryDocuments: true });
  assert.deepEqual(valid.findings, []);
});

test("rejects incomplete boundary documents", async () => {
  const result = await scanFixture({
    "LICENSE": "Copyright only.\n",
    "SECURITY.md": "Contact support.\n",
    "docs/PUBLIC_SOURCE_AUTHORITY.md": "Authority not selected.\n"
  }, { requireBoundaryDocuments: true });

  assert.deepEqual(ruleIds(result), [
    "boundary.authority-invalid",
    "boundary.license-invalid",
    "boundary.security-invalid"
  ]);
});

test("rejects tracked symlinks and binary content without following them", async () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-public-source-safety-link-"));
  try {
    const target = path.join(root, "target");
    const link = path.join(root, "src", "linked");
    fs.mkdirSync(target, { recursive: true });
    fs.writeFileSync(path.join(target, "value.txt"), "safe\n", "utf8");
    fs.mkdirSync(path.dirname(link), { recursive: true });
    fs.symlinkSync(target, link, process.platform === "win32" ? "junction" : "dir");
    fs.writeFileSync(path.join(root, "binary.dat"), Buffer.from([0, 1, 2, 3]));

    const verifier = await import(verifierUrl.href);
    const result = verifier.scanPublicSourceTree({
      repositoryRoot: root,
      trackedPaths: ["src/linked", "binary.dat"]
    });
    assert.deepEqual(ruleIds(result), [
      "repository.binary-content",
      "repository.symlink"
    ]);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("accepts reviewed public media binary formats", async () => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "pcv-public-source-safety-media-"));
  const extensions = ["png", "jpg", "jpeg", "gif", "ico", "webp", "woff", "woff2"];
  try {
    const trackedPaths = extensions.map((extension) => `docs/assets/example.${extension}`);
    for (const relativePath of trackedPaths) {
      const absolutePath = path.join(root, ...relativePath.split("/"));
      fs.mkdirSync(path.dirname(absolutePath), { recursive: true });
      fs.writeFileSync(absolutePath, Buffer.from([0, 1, 2, 3]));
    }
    const verifier = await import(verifierUrl.href);
    const result = verifier.scanPublicSourceTree({ repositoryRoot: root, trackedPaths });
    assert.deepEqual(result.findings, []);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("provides shell-free tracked-file CLI verification", () => {
  const root = writeFixture({
    "LICENSE": "All rights reserved. No permission is granted to reproduce, modify, redistribute, sublicense, or sell.\n",
    "SECURITY.md": "Report security issues through GitHub private vulnerability reporting.\n",
    "docs/PUBLIC_SOURCE_AUTHORITY.md": [
      "HardcoreMonk/purecvisor-desktop-node-public",
      "public_trusted_signing=false",
      "external_stable_publication=false"
    ].join("\n"),
    "docs/safe.md": "Endpoint: 192.0.2.10\n"
  });
  try {
    const initialize = spawnSync("git", ["init", "--initial-branch=main"], {
      cwd: root,
      encoding: "utf8",
      shell: false,
      windowsHide: true
    });
    assert.equal(initialize.status, 0, initialize.stderr);
    const add = spawnSync("git", ["add", "--", "."], {
      cwd: root,
      encoding: "utf8",
      shell: false,
      windowsHide: true
    });
    assert.equal(add.status, 0, add.stderr);

    const safe = spawnSync(process.execPath, [verifierPath, "--root", root, "--require-boundaries"], {
      cwd: root,
      encoding: "utf8",
      shell: false,
      windowsHide: true
    });
    assert.equal(safe.status, 0, safe.stderr);
    const safeReport = JSON.parse(safe.stdout);
    assert.equal(safeReport.finding_count, 0);

    const privateAddress = [172, 20, 10, 8].join(".");
    fs.writeFileSync(path.join(root, "docs", "unsafe.md"), `Observed=${privateAddress}\n`, "utf8");
    const addUnsafe = spawnSync("git", ["add", "--", "docs/unsafe.md"], {
      cwd: root,
      encoding: "utf8",
      shell: false,
      windowsHide: true
    });
    assert.equal(addUnsafe.status, 0, addUnsafe.stderr);
    const unsafe = spawnSync(process.execPath, [verifierPath, "--root", root, "--require-boundaries"], {
      cwd: root,
      encoding: "utf8",
      shell: false,
      windowsHide: true
    });
    assert.equal(unsafe.status, 1, unsafe.stderr);
    assert.equal(unsafe.stdout.includes(privateAddress), false);
    assert.deepEqual(JSON.parse(unsafe.stdout).findings, [{
      line: 1,
      path: "docs/unsafe.md",
      rule_id: "network.observed-private-endpoint"
    }]);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
});

test("registers separate public-source safety commands", () => {
  assert.equal(
    packageJson.scripts["test:public-source-safety"],
    "node --test --test-reporter=spec node-tests/public-source-safety.test.mjs"
  );
  assert.equal(
    packageJson.scripts["verify:public-source-safety"],
    "node scripts/verify-public-source-safety.mjs --require-boundaries"
  );
});

test("the tracked repository satisfies the public authority boundary", async () => {
  const verifier = await import(verifierUrl.href);
  const result = verifier.scanPublicSourceTree({
    repositoryRoot,
    trackedPaths: verifier.listTrackedFiles(repositoryRoot),
    forbiddenIdentities: [process.env.USERNAME].filter(Boolean),
    requireBoundaryDocuments: true
  });

  assert.deepEqual(result.findings, []);
});

test("formats a canonical report without leaking matched values", async () => {
  const privateIdentity = ["Private", "Operator"].join("");
  const result = await scanFixture({
    "docs/evidence.md": `Owner=${privateIdentity}\n`
  }, { forbiddenIdentities: [privateIdentity] });
  const verifier = await import(verifierUrl.href);
  const report = verifier.formatSafetyReport(result);

  assert.equal(typeof report.report_sha256, "string");
  assert.equal(report.report_sha256.length, 64);
  assert.equal(JSON.stringify(report).includes(privateIdentity), false);
  assert.deepEqual(report.findings, [{
    line: 1,
    path: "docs/evidence.md",
    rule_id: "identity.forbidden-token"
  }]);
});

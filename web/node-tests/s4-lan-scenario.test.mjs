import assert from "node:assert/strict";
import { spawn, spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync } from "node:fs";
import http from "node:http";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

const script = fileURLToPath(new URL("../scripts/run-s4-lan-scenario.mjs", import.meta.url));
const probePassword = "s4-secret-probe";
const accessToken = "s4-fake-access-token-do-not-leak";
const refreshToken = "s4-fake-refresh-token-do-not-leak";
const loopbackToken = "s4-loopback-token-do-not-leak";

function run(args, env = process.env) {
  return spawnSync(process.execPath, [script, ...args], { encoding: "utf8", env, windowsHide: true });
}

function runLive(args, env) {
  return new Promise((resolve, reject) => {
    const child = spawn(process.execPath, [script, ...args], { env, windowsHide: true });
    let stdout = "";
    let stderr = "";
    child.stdout.setEncoding("utf8");
    child.stderr.setEncoding("utf8");
    child.stdout.on("data", (chunk) => { stdout += chunk; });
    child.stderr.on("data", (chunk) => { stderr += chunk; });
    child.on("error", reject);
    child.on("close", (status) => resolve({ status, stdout, stderr }));
  });
}

function listen(handler) {
  const requests = [];
  const server = http.createServer((request, response) => {
    const chunks = [];
    request.on("data", (chunk) => chunks.push(chunk));
    request.on("end", () => {
      requests.push((request.url || "").split("?")[0]);
      handler(request, response, Buffer.concat(chunks).toString("utf8"));
    });
  });
  return new Promise((resolve) => {
    server.listen(0, "127.0.0.1", () => {
      const { port } = server.address();
      resolve({ server, requests, base: `http://127.0.0.1:${port}` });
    });
  });
}

function closeServer(server) {
  return new Promise((resolve) => server.close(() => resolve()));
}

test("T1 plan-only without arguments prints the S4 steps and a repository out path", () => {
  const result = run([]);
  assert.equal(result.status, 0);
  assert.match(result.stdout, /^S4 LAN scenario plan-only \(no request sent\)\./);
  assert.match(result.stdout, /preflight\.web-root: GET /);
  assert.match(result.stdout, /preflight\.web-config: GET .+\/pcv-config\.js expect apiBaseUrl host = web host/);
  assert.match(result.stdout, /preflight\.loopback-session-refused: POST /);
  assert.match(result.stdout, /preflight\.login: POST /);
  assert.match(result.stdout, /preflight\.auth-check: GET /);
  assert.match(result.stdout, /s1\.create: POST .+ name=pcv-it-s4-console /);
  assert.match(result.stdout, /s1\.screen-before: GET .+\/console\/frame\/640x480 /);
  assert.match(result.stdout, /s1\.input: POST .+ key_code=13/);
  assert.match(result.stdout, /s1\.screen-after: GET /);
  assert.match(result.stdout, /s2\.template-lock: POST .+\/pcv-it-s4-source\/template-lock /);
  assert.match(result.stdout, /s2\.clone-preview: POST /);
  assert.match(result.stdout, /s2\.bound: wait until the clone job succeeds and elapsed_ms <= 60000/);
  assert.match(result.stdout, /s3\.checkpoint-create: POST .+ name=s4-cp1/);
  assert.match(result.stdout, /s3\.schedule-set: POST .+ interval_minutes=60 retention_max=2/);
  assert.match(result.stdout, /s3\.auto-checkpoint: .*within 3720000 ms/);
  assert.match(result.stdout, /cleanup: /);
  assert.match(result.stdout, /pcv-it-s4-console/);
  assert.match(result.stdout, /pcv-it-s4-source/);
  assert.match(result.stdout, /pcv-it-s4-clone/);
  assert.match(result.stdout, /pcv-it-s4-checkpoint/);
  assert.match(result.stdout, /<--iso required for --execute>/);
  assert.match(result.stdout, /artifacts[\\/]s4-lan-\d{8}/);
  assert.doesNotMatch(result.stdout, /Users[\\/]/);
  assert.doesNotMatch(result.stderr, /Users[\\/]/);
});

test("T2 --web=value and --web value print the same plan for the example host", () => {
  const inline = run(["--web=http://pcv-host.example"]);
  const spaced = run(["--web", "http://pcv-host.example"]);
  assert.equal(inline.status, 0);
  assert.equal(spaced.status, 0);
  assert.equal(inline.stdout, spaced.stdout);
  assert.match(inline.stdout, /http:\/\/pcv-host\.example/);
  assert.doesNotMatch(inline.stdout, /Users[\\/]/);
});

test("T3 an unknown argument is rejected before any plan", () => {
  const result = run(["--bogus=1"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /--bogus/);
  assert.doesNotMatch(result.stdout, /plan-only/);
  assert.equal(result.stdout, "");
});

test("T4 a value argument without a value is rejected", () => {
  const result = run(["--web"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /--web/);
  assert.equal(result.stdout, "");
});

test("T5 --password is rejected and its value is not written", () => {
  const result = run([`--password=${probePassword}`]);
  assert.equal(result.status, 2);
  assert.doesNotMatch(result.stdout, /s4-secret-probe/);
  assert.doesNotMatch(result.stderr, /s4-secret-probe/);
});

test("T6 --execute without --user or PCV_S4_PASSWORD sends no request", () => {
  const withPassword = { ...process.env, PCV_S4_PASSWORD: probePassword };
  const missingUser = run(["--execute", "--phases=none"], withPassword);
  assert.equal(missingUser.status, 2);
  assert.match(missingUser.stderr, /--user/);
  assert.equal(missingUser.stdout, "");
  assert.doesNotMatch(missingUser.stderr, /s4-secret-probe/);

  const env = { ...process.env };
  delete env.PCV_S4_PASSWORD;
  const missingPassword = run(["--execute", "--phases=none", "--user=s4-operator"], env);
  assert.equal(missingPassword.status, 2);
  assert.match(missingPassword.stderr, /PCV_S4_PASSWORD/);
  assert.equal(missingPassword.stdout, "");
});

test("T7 --phases=s4 is rejected", () => {
  const result = run(["--phases=s4"]);
  assert.equal(result.status, 2);
  assert.match(result.stderr, /--phases/);
  assert.equal(result.stdout, "");
});

test("T8 --execute --phases=none passes preflight against a fake host and keeps secrets out of the summary", async () => {
  const host = await listen((request, response) => {
    const url = request.url || "";
    if (url === "/") {
      response.writeHead(200, { "content-type": "text/html; charset=utf-8" });
      response.end("<!doctype html><title>pcv</title>");
      return;
    }
    if (url.startsWith("/pcv-config.js")) {
      response.writeHead(200, { "content-type": "application/javascript" });
      response.end(`window.PCV_DESKTOP_NODE_CONFIG = Object.freeze({"apiBaseUrl":"${host.base}"});\n`);
      return;
    }
    if (url.startsWith("/api/v1/auth/loopback-session")) {
      response.writeHead(409, { "content-type": "application/json" });
      response.end(JSON.stringify({ ok: false, error: { code: "PCV_LOOPBACK_SESSION_DISABLED" }, data: { access_token: loopbackToken } }));
      return;
    }
    if (url.startsWith("/api/v1/auth/login")) {
      response.writeHead(200, { "content-type": "application/json" });
      response.end(JSON.stringify({
        ok: true,
        data: {
          access_token: accessToken,
          refresh_token: refreshToken,
          session: { role: "operator", username: "s4-operator" }
        }
      }));
      return;
    }
    if (url.startsWith("/api/v1/vms")) {
      response.writeHead(200, { "content-type": "application/json" });
      response.end(JSON.stringify({ ok: true, data: [] }));
      return;
    }
    response.writeHead(404, { "content-type": "application/json" });
    response.end(JSON.stringify({ ok: false, error: { code: "PCV_TEST_UNEXPECTED" } }));
  });
  const out = mkdtempSync(join(tmpdir(), "pcv-s4-lan-"));
  try {
    const result = await runLive([
      "--execute",
      "--phases=none",
      `--web=${host.base}`,
      "--user=s4-operator",
      `--out=${out}`
    ], { ...process.env, PCV_S4_PASSWORD: probePassword });
    const summaryText = readFileSync(join(out, "summary.json"), "utf8");
    const summary = JSON.parse(summaryText);
    assert.equal(result.status, 0);
    assert.equal(summary.result, "pass");
    assert.equal(summary.contract, "pcv-s4-lan-scenario-v1");
    assert.equal(summary.preflight["loopback-session-refused"].status, 409);
    assert.equal(summary.preflight["loopback-session-refused"].code, "PCV_LOOPBACK_SESSION_DISABLED");
    assert.equal(summary.preflight.login.status, 200);
    assert.equal(summary.role, "operator");
    assert.match(result.stdout, /result=pass s1=skipped s2=skipped s3=skipped/);
    for (const secret of [probePassword, accessToken, refreshToken, loopbackToken]) {
      assert.equal(summaryText.includes(secret), false);
      assert.equal(result.stdout.includes(secret), false);
      assert.equal(result.stderr.includes(secret), false);
    }
  } finally {
    rmSync(out, { recursive: true, force: true });
    await closeServer(host.server);
  }
});

test("T9 a wildcard apiBaseUrl fails web-config before login", async () => {
  const host = await listen((request, response) => {
    const url = request.url || "";
    if (url === "/") {
      response.writeHead(200, { "content-type": "text/html; charset=utf-8" });
      response.end("<!doctype html><title>pcv</title>");
      return;
    }
    if (url.startsWith("/pcv-config.js")) {
      response.writeHead(200, { "content-type": "application/javascript" });
      response.end("window.PCV_DESKTOP_NODE_CONFIG = Object.freeze({\"apiBaseUrl\":\"http://+:7777\"});\n");
      return;
    }
    response.writeHead(200, { "content-type": "application/json" });
    response.end(JSON.stringify({ ok: true, data: { access_token: accessToken, session: { role: "operator" } } }));
  });
  const out = mkdtempSync(join(tmpdir(), "pcv-s4-lan-"));
  try {
    const result = await runLive([
      "--execute",
      "--phases=none",
      `--web=${host.base}`,
      "--user=s4-operator",
      `--out=${out}`
    ], { ...process.env, PCV_S4_PASSWORD: probePassword });
    const summary = JSON.parse(readFileSync(join(out, "summary.json"), "utf8"));
    assert.equal(result.status, 1);
    assert.equal(summary.preflight["web-config"].ok, false);
    assert.equal(host.requests.includes("/api/v1/auth/login"), false);
    assert.equal(result.stdout.includes(probePassword), false);
    assert.equal(result.stderr.includes(probePassword), false);
  } finally {
    rmSync(out, { recursive: true, force: true });
    await closeServer(host.server);
  }
});

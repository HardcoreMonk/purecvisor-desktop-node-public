// S2 scenario (ADR-0017): create a small managed Generation 2 VM, lock it as a template, clone it,
// and require the clone job to finish within 60 seconds. Then delete only the clone.
// --plan-only (the default) prints the steps without any request. --execute calls the same Local API
// routes the Web Console uses. The loopback session token stays in memory and is never printed.
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const match = process.argv.find((value) => value.startsWith(prefix));
  return match ? match.slice(prefix.length) : fallback;
}

const execute = process.argv.includes("--execute");
const api = getArg("api", "http://127.0.0.1:7777");
const source = getArg("source", "pcv-it-s2-source");
const target = getArg("target", "pcv-it-s2-clone");
const vmRoot = getArg("vm-root", "D:\\PureCVisor\\VMs");
const iso = getArg("iso", "D:\\Downloads\\ubuntu-26.04.1-live-server-amd64.iso");
const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const outDir = resolve(getArg("out", join(repoRoot, "artifacts", "s2-template-clone-20261009")));
const boundMs = 60000;

const steps = [
  ["create", `POST /api/v1/vms name=${source} generation=2 cpu=1 memory_mb=512 disk_gb=8 iso=${iso} vm_root=${vmRoot}`],
  ["template-lock", `POST /api/v1/vms/${source}/template-lock locked=true confirm_name=${source}`],
  ["preview", `POST /api/v1/vms/${source}/clone/preview name=${target} confirm_name=${source} vm_root=${vmRoot}`],
  ["clone", `POST /api/v1/vms/${source}/clone name=${target} confirm_name=${source} vm_root=${vmRoot}`],
  ["bound", `wait until the clone job succeeds and elapsed_ms <= ${boundMs}`],
  ["delete-clone", `DELETE /api/v1/vms/${target}`]
];

if (!execute) {
  process.stdout.write("S2 clone scenario plan-only (no request sent)\n");
  steps.forEach(([name, detail], index) => process.stdout.write(`${index + 1}. ${name}: ${detail}\n`));
  process.exit(0);
}

let token = "";
const summary = {
  contract: "pcv-s2-clone-scenario-v1",
  source,
  target,
  bound_ms: boundMs,
  started_at: new Date().toISOString(),
  steps: {}
};

async function call(method, path, body) {
  const response = await fetch(`${api}${path}`, {
    method,
    headers: { "content-type": "application/json", ...(token ? { authorization: `Bearer ${token}` } : {}) },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  const payload = await response.json().catch(() => ({}));
  return { status: response.status, payload };
}

async function waitJob(result, label) {
  const jobId = result.payload?.data?.job_id || "";
  if (!jobId) {
    throw new Error(`${label}: no job id (status ${result.status} ${result.payload?.error?.code || ""})`);
  }
  for (let attempt = 0; attempt < 90; attempt += 1) {
    const job = await call("GET", `/api/v1/jobs/${encodeURIComponent(jobId)}`);
    const status = job.payload?.data?.status || "";
    if (status && !["queued", "running", "pending", "accepted"].includes(status)) {
      summary.steps[label] = { status, elapsed_ms: summary.steps[label]?.elapsed_ms };
      if (status !== "succeeded") throw new Error(`${label}: job ${status} ${job.payload?.data?.error?.code || job.payload?.error?.code || ""}`);
      return;
    }
    await new Promise((done) => setTimeout(done, 1000));
  }
  throw new Error(`${label}: job timed out`);
}

async function vmExists(name) {
  const listed = await call("GET", "/api/v1/vms");
  const rows = listed.payload?.data || [];
  return rows.some((row) => (row.name || row.id) === name);
}

try {
  const session = await call("POST", "/api/v1/auth/loopback-session", {});
  token = session.payload?.data?.access_token || "";
  if (!token) throw new Error(`session status ${session.status}`);
  summary.steps.session = { status: session.status };

  if (!(await vmExists(source))) {
    const created = await call("POST", "/api/v1/vms", {
      name: source,
      iso_path: iso,
      vm_root: vmRoot,
      cpu: 1,
      memory_mb: 512,
      disk_gb: 8,
      generation: 2
    });
    await waitJob(created, "create");
  } else {
    summary.steps.create = { status: "already-present" };
  }

  const locked = await call("POST", `/api/v1/vms/${encodeURIComponent(source)}/template-lock`, {
    confirm_name: source,
    locked: true
  });
  await waitJob(locked, "template-lock");

  const preview = await call("POST", `/api/v1/vms/${encodeURIComponent(source)}/clone/preview`, {
    confirm_name: source,
    name: target,
    vm_root: vmRoot
  });
  summary.steps.preview = { status: preview.status, code: preview.payload?.error?.code || "" };
  if (preview.status !== 200) throw new Error(`preview status ${preview.status} ${preview.payload?.error?.code || ""}`);

  const started = Date.now();
  const cloned = await call("POST", `/api/v1/vms/${encodeURIComponent(source)}/clone`, {
    confirm_name: source,
    name: target,
    vm_root: vmRoot
  });
  await waitJob(cloned, "clone");
  const elapsed = Date.now() - started;
  summary.steps.clone.elapsed_ms = elapsed;
  summary.clone_elapsed_ms = elapsed;
  summary.within_bound = elapsed <= boundMs;
  if (elapsed > boundMs) throw new Error(`clone elapsed_ms ${elapsed} exceeds ${boundMs}`);
} catch (error) {
  summary.result = "fail";
  summary.error = String(error?.message || error);
} finally {
  if (token) {
    try {
      if (await vmExists(target)) {
        const deleted = await call("DELETE", `/api/v1/vms/${encodeURIComponent(target)}`);
        await waitJob(deleted, "delete-clone");
      }
    } catch (error) {
      summary.cleanup_error = String(error?.message || error);
    }
  }
  summary.finished_at = new Date().toISOString();
  if (!summary.result) summary.result = summary.within_bound ? "pass" : "fail";
  mkdirSync(outDir, { recursive: true });
  writeFileSync(join(outDir, "summary.json"), `${JSON.stringify(summary, null, 2)}\n`);
  process.stdout.write(`result=${summary.result} clone_elapsed_ms=${summary.clone_elapsed_ms ?? ""}\n`);
  process.exitCode = summary.result === "pass" && !summary.cleanup_error ? 0 : 1;
}

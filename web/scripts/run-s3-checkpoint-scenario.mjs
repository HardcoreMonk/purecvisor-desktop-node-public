// S3 scenario (ADR-0017): create a small managed Generation 2 VM, create a checkpoint, restore it, enable a
// checkpoint schedule and wait for the worker to enqueue an automatic checkpoint, then clear the schedule and
// delete the VM with its checkpoints. --plan-only (the default) prints the steps without any request.
// --execute calls the same Local API routes the Web Console uses. The loopback session token stays in memory
// and is never printed. Paths default to the repository artifacts folder so the summary carries no user paths.
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const match = process.argv.find((value) => value.startsWith(prefix));
  return match ? match.slice(prefix.length) : fallback;
}

const execute = process.argv.includes("--execute");
// --keep-vm leaves the VM (with its checkpoints) for the browser demo; --cleanup-only skips every step and only
// clears the schedule and deletes the VM. Both need --execute.
const keepVm = process.argv.includes("--keep-vm");
const cleanupOnly = process.argv.includes("--cleanup-only");
const api = getArg("api", "http://127.0.0.1:7777");
const vm = getArg("vm", "pcv-it-s3-source");
const checkpoint = getArg("checkpoint", "s3-cp1");
// CheckpointSchedulePolicy: interval 60..10080 minutes, retention 1..32. A schedule with no last_enqueued_at is due
// at once, so the first scheduled checkpoint appears within seconds and next_due_at moves one interval ahead.
const intervalMinutes = Number(getArg("interval", "60"));
const retentionMax = Number(getArg("retention", "2"));
const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const vmRoot = resolve(getArg("vm-root", join(repoRoot, "artifacts", "s3-checkpoint-20261010", "vms")));
const iso = resolve(getArg("iso", join(repoRoot, "artifacts", "smoke-media-20261003", "pcv-route-parity-smoke-20261003.iso")));
const outDir = resolve(getArg("out", join(repoRoot, "artifacts", "s3-checkpoint-20261010")));
const autoBoundMs = Number(getArg("auto-bound-ms", "120000"));

const steps = [
  ["create", `POST /api/v1/vms name=${vm} generation=2 cpu=1 memory_mb=512 disk_gb=8 iso=${iso} vm_root=${vmRoot}`],
  ["checkpoint-create", `POST /api/v1/vms/${vm}/checkpoints name=${checkpoint}`],
  ["checkpoint-list", `GET /api/v1/vms/${vm}/checkpoints expect ${checkpoint}`],
  ["checkpoint-restore", `POST /api/v1/vms/${vm}/checkpoints/${checkpoint}/restore`],
  ["schedule-preview", `POST /api/v1/vms/${vm}/checkpoints/schedule/preview interval_minutes=${intervalMinutes} retention_max=${retentionMax}`],
  ["schedule-set", `POST /api/v1/vms/${vm}/checkpoints/schedule interval_minutes=${intervalMinutes} retention_max=${retentionMax}`],
  ["auto-checkpoint", `wait until the worker enqueues the first scheduled checkpoint (due at once on enable, count >= 2) within ${autoBoundMs} ms`],
  ["schedule-clear", `POST /api/v1/vms/${vm}/checkpoints/schedule/clear`],
  ["delete-vm", `DELETE /api/v1/vms/${vm} (removes its checkpoints; skipped with --keep-vm, run alone with --cleanup-only)`]
];

if (!execute) {
  process.stdout.write("S3 checkpoint scenario plan-only (no request sent)\n");
  steps.forEach(([name, detail], index) => process.stdout.write(`${index + 1}. ${name}: ${detail}\n`));
  process.exit(0);
}

let token = "";
const summary = {
  contract: "pcv-s3-checkpoint-scenario-v1",
  vm,
  checkpoint,
  interval_minutes: intervalMinutes,
  retention_max: retentionMax,
  auto_bound_ms: autoBoundMs,
  keep_vm: keepVm,
  cleanup_only: cleanupOnly,
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
  for (let attempt = 0; attempt < 120; attempt += 1) {
    const job = await call("GET", `/api/v1/jobs/${encodeURIComponent(jobId)}`);
    const status = job.payload?.data?.status || "";
    if (status && !["queued", "running", "pending", "accepted"].includes(status)) {
      summary.steps[label] = { ...(summary.steps[label] || {}), status, job_id: jobId };
      if (status !== "succeeded") throw new Error(`${label}: job ${status} ${job.payload?.data?.error?.code || job.payload?.error?.code || ""}`);
      return;
    }
    await new Promise((done) => setTimeout(done, 1000));
  }
  throw new Error(`${label}: job timed out`);
}

function rowsOf(payload) {
  const data = payload?.data;
  if (Array.isArray(data)) return data;
  if (Array.isArray(data?.checkpoints)) return data.checkpoints;
  if (Array.isArray(data?.vms)) return data.vms;
  return [];
}

async function vmExists(name) {
  const listed = await call("GET", "/api/v1/vms");
  return rowsOf(listed.payload).some((row) => (row.name || row.id) === name);
}

async function checkpointNames() {
  const listed = await call("GET", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints`);
  if (listed.status !== 200) throw new Error(`checkpoint-list status ${listed.status} ${listed.payload?.error?.code || ""}`);
  return rowsOf(listed.payload).map((row) => row.name || row.id || "");
}

async function scheduleReadback() {
  const detail = await call("GET", `/api/v1/vms/${encodeURIComponent(vm)}`);
  return detail.payload?.data?.checkpoint_schedule || {};
}

let scheduleSet = false;

try {
  const session = await call("POST", "/api/v1/auth/loopback-session", {});
  token = session.payload?.data?.access_token || "";
  if (!token) throw new Error(`session status ${session.status}`);
  summary.steps.session = { status: session.status };

  if (cleanupOnly) {
    scheduleSet = (await scheduleReadback()).enabled === true;
    summary.within_bound = true;
    throw Object.assign(new Error("cleanup-only"), { cleanupOnly: true });
  }

  if (!(await vmExists(vm))) {
    const created = await call("POST", "/api/v1/vms", {
      name: vm,
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

  const createdCheckpoint = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints`, { name: checkpoint });
  await waitJob(createdCheckpoint, "checkpoint-create");

  const namesAfterCreate = await checkpointNames();
  summary.steps["checkpoint-list"] = { count: namesAfterCreate.length, names: namesAfterCreate };
  if (!namesAfterCreate.includes(checkpoint)) throw new Error(`checkpoint-list: ${checkpoint} missing`);

  const restored = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/${encodeURIComponent(checkpoint)}/restore`);
  await waitJob(restored, "checkpoint-restore");

  const preview = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule/preview`, {
    interval_minutes: intervalMinutes,
    retention_max: retentionMax
  });
  summary.steps["schedule-preview"] = { status: preview.status, code: preview.payload?.error?.code || "" };
  if (preview.status !== 200) throw new Error(`schedule-preview status ${preview.status} ${preview.payload?.error?.code || ""}`);

  const scheduled = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule`, {
    interval_minutes: intervalMinutes,
    retention_max: retentionMax
  });
  await waitJob(scheduled, "schedule-set");
  scheduleSet = true;
  summary.steps["schedule-set"].readback = await scheduleReadback();

  const started = Date.now();
  let autoCount = namesAfterCreate.length;
  while (Date.now() - started < autoBoundMs) {
    const names = await checkpointNames();
    autoCount = names.length;
    if (autoCount >= namesAfterCreate.length + 1) {
      summary.steps["auto-checkpoint"] = { count: autoCount, names, elapsed_ms: Date.now() - started, readback: await scheduleReadback() };
      break;
    }
    await new Promise((done) => setTimeout(done, 2000));
  }
  summary.auto_checkpoint_elapsed_ms = summary.steps["auto-checkpoint"]?.elapsed_ms ?? null;
  summary.within_bound = summary.auto_checkpoint_elapsed_ms !== null;
  if (!summary.within_bound) throw new Error(`auto-checkpoint: count stayed ${autoCount} within ${autoBoundMs} ms`);

  const cleared = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule/clear`);
  await waitJob(cleared, "schedule-clear");
  scheduleSet = false;
  summary.steps["schedule-clear"].readback = await scheduleReadback();
} catch (error) {
  if (!error?.cleanupOnly) {
    summary.result = "fail";
    summary.error = String(error?.message || error);
  }
} finally {
  if (token) {
    try {
      if (scheduleSet && (!keepVm || cleanupOnly)) {
        const cleared = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/checkpoints/schedule/clear`);
        await waitJob(cleared, "schedule-clear");
      }
      if ((!keepVm || cleanupOnly) && (await vmExists(vm))) {
        const deleted = await call("DELETE", `/api/v1/vms/${encodeURIComponent(vm)}`);
        await waitJob(deleted, "delete-vm");
      }
      summary.vm_remaining = await vmExists(vm);
    } catch (error) {
      summary.cleanup_error = String(error?.message || error);
    }
  }
  summary.finished_at = new Date().toISOString();
  const vmStateOk = keepVm && !cleanupOnly ? summary.vm_remaining === true : summary.vm_remaining === false;
  if (!summary.result) summary.result = summary.within_bound && vmStateOk ? "pass" : "fail";
  mkdirSync(outDir, { recursive: true });
  writeFileSync(join(outDir, "summary.json"), `${JSON.stringify(summary, null, 2)}\n`);
  process.stdout.write(`result=${summary.result} auto_checkpoint_elapsed_ms=${summary.auto_checkpoint_elapsed_ms ?? ""}\n`);
  process.exitCode = summary.result === "pass" && !summary.cleanup_error ? 0 : 1;
}

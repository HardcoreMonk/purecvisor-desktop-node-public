import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

const script = fileURLToPath(new URL("../scripts/run-s3-checkpoint-scenario.mjs", import.meta.url));

test("plan-only prints checkpoint create, restore, schedule and cleanup without executing", () => {
  const result = spawnSync(process.execPath, [script], { encoding: "utf8" });
  assert.equal(result.status, 0);
  assert.match(result.stdout, /plan-only \(no request sent\)/);
  assert.match(result.stdout, /checkpoint-create: POST \/api\/v1\/vms\/pcv-it-s3-source\/checkpoints name=s3-cp1/);
  assert.match(result.stdout, /checkpoint-restore: POST \/api\/v1\/vms\/pcv-it-s3-source\/checkpoints\/s3-cp1\/restore/);
  assert.match(result.stdout, /schedule-set: POST \/api\/v1\/vms\/pcv-it-s3-source\/checkpoints\/schedule interval_minutes=60 retention_max=2/);
  assert.match(result.stdout, /auto-checkpoint: .*due one interval after enable.*within 3720000 ms/);
  assert.match(result.stdout, /schedule-clear: POST \/api\/v1\/vms\/pcv-it-s3-source\/checkpoints\/schedule\/clear/);
  assert.match(result.stdout, /delete-vm: DELETE \/api\/v1\/vms\/pcv-it-s3-source/);
});

test("plan-only takes the vm, checkpoint and interval arguments and keeps paths inside the repository by default", () => {
  const result = spawnSync(process.execPath, [script, "--vm=pcv-it-s3-demo", "--checkpoint=demo-cp", "--interval=120", "--auto-bound-ms=30000"], { encoding: "utf8" });
  assert.equal(result.status, 0);
  assert.match(result.stdout, /pcv-it-s3-demo\/checkpoints name=demo-cp/);
  assert.match(result.stdout, /interval_minutes=120/);
  assert.match(result.stdout, /within 30000 ms/);
  assert.match(result.stdout, /artifacts[\\/]s3-checkpoint-20261010[\\/]vms/);
  assert.match(result.stdout, /artifacts[\\/]smoke-media-20261003[\\/]pcv-route-parity-smoke-20261003\.iso/);
  assert.doesNotMatch(result.stdout, /Users[\\/]/);
});

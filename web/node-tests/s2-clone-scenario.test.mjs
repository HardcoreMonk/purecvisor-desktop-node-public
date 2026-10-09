import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

const script = fileURLToPath(new URL("../scripts/run-s2-clone-scenario.mjs", import.meta.url));

test("plan-only prints the template lock, clone, and 60 second bound without executing", () => {
  const result = spawnSync(process.execPath, [script], { encoding: "utf8" });
  assert.equal(result.status, 0);
  assert.match(result.stdout, /plan-only \(no request sent\)/);
  assert.match(result.stdout, /template-lock: POST \/api\/v1\/vms\/pcv-it-s2-source\/template-lock/);
  assert.match(result.stdout, /preview: POST \/api\/v1\/vms\/pcv-it-s2-source\/clone\/preview/);
  assert.match(result.stdout, /clone: POST \/api\/v1\/vms\/pcv-it-s2-source\/clone /);
  assert.match(result.stdout, /elapsed_ms <= 60000/);
});

test("plan-only uses the approved source and clone names", () => {
  const result = spawnSync(process.execPath, [script, "--source=pcv-it-s2-source", "--target=pcv-it-s2-clone"], { encoding: "utf8" });
  assert.equal(result.status, 0);
  assert.match(result.stdout, /pcv-it-s2-source/);
  assert.match(result.stdout, /pcv-it-s2-clone/);
  assert.match(result.stdout, /delete-clone/);
});

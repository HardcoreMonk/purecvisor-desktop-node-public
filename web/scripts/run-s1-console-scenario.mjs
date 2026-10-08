// S1 scenario (ADR-0017, design pcv-s1-browser-console-v1): create a VM from an ISO, start it, read its screen through
// the console frame route, send keyboard input, read the screen again, then power off and delete the VM. It calls the
// same Local API routes the Web Console uses. --plan-only (the default) prints the steps without any request.
// --execute needs a separate host-mutation approval; the loopback session token stays in memory and is never printed.
import { createHash } from "node:crypto";
import { mkdirSync, writeFileSync } from "node:fs";
import { join, resolve } from "node:path";
import { inflateSync } from "node:zlib";

function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const match = process.argv.find((value) => value.startsWith(prefix));
  return match ? match.slice(prefix.length) : fallback;
}

const execute = process.argv.includes("--execute");
const api = getArg("api", "http://127.0.0.1:7777");
const iso = getArg("iso");
const vm = getArg("vm", `pcv-it-s1-${new Date().toISOString().replace(/[-:TZ.]/g, "").slice(0, 14)}`);
const outDir = resolve(getArg("out", join("..", "artifacts", "s1-console-scenario", vm)));
const keys = getArg("keys", "13").split(",").map((value) => Number(value)).filter((value) => value >= 1 && value <= 254);

const steps = [
  ["session", "POST /api/v1/auth/loopback-session"],
  ["create", `POST /api/v1/vms name=${vm} iso=${iso || "<--iso required>"} generation=2 cpu=2 memory_mb=2048 disk_gb=32`],
  ["start", `POST /api/v1/vms/${vm}/start`],
  ["screen-before", `GET /api/v1/vms/${vm}/console/frame/640x480 until the screen is not blank`],
  ["input", `POST /api/v1/vms/${vm}/console/input kind=key action=type key_code=${keys.join(",")}`],
  ["screen-after", `GET /api/v1/vms/${vm}/console/frame/640x480 until the screen changes`],
  ["poweroff", `POST /api/v1/vms/${vm}/poweroff`],
  ["delete", `DELETE /api/v1/vms/${vm}`]
];

if (!execute) {
  process.stdout.write(`S1 console scenario plan-only (no request sent)\nout=${outDir}\n`);
  steps.forEach(([name, detail], index) => process.stdout.write(`${index + 1}. ${name}: ${detail}\n`));
  process.exit(iso ? 0 : 2);
}

if (!iso) {
  process.stderr.write("--iso is required with --execute\n");
  process.exit(2);
}

let token = "";
const summary = { contract: "pcv-s1-console-scenario-v1", vm, api, started_at: new Date().toISOString(), steps: {} };

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
  const jobId = result.payload?.data?.job_id;
  if (!jobId) throw new Error(`${label}: no job id (status ${result.status} ${result.payload?.error?.code || ""})`);
  for (let attempt = 0; attempt < 180; attempt += 1) {
    const job = await call("GET", `/api/v1/jobs/${encodeURIComponent(jobId)}`);
    const status = job.payload?.data?.status;
    if (status && !["queued", "running", "pending", "accepted"].includes(status)) {
      summary.steps[label] = { job_id: jobId, status };
      if (status !== "succeeded") throw new Error(`${label}: job ${jobId} ${status}`);
      return;
    }
    await new Promise((done) => setTimeout(done, 2000));
  }
  throw new Error(`${label}: job ${jobId} timed out`);
}

function toBmp(rgb565, width, height) {
  const rowSize = Math.ceil((width * 3) / 4) * 4;
  const bmp = Buffer.alloc(54 + (rowSize * height));
  bmp.write("BM", 0);
  bmp.writeUInt32LE(bmp.length, 2);
  bmp.writeUInt32LE(54, 10);
  bmp.writeUInt32LE(40, 14);
  bmp.writeInt32LE(width, 18);
  bmp.writeInt32LE(-height, 22);
  bmp.writeUInt16LE(1, 26);
  bmp.writeUInt16LE(24, 28);
  for (let y = 0; y < height; y += 1) {
    for (let x = 0; x < width; x += 1) {
      const value = rgb565.readUInt16LE(((y * width) + x) * 2);
      const offset = 54 + (y * rowSize) + (x * 3);
      bmp[offset] = Math.round((value & 0x1f) * 255 / 31);
      bmp[offset + 1] = Math.round(((value >> 5) & 0x3f) * 255 / 63);
      bmp[offset + 2] = Math.round(((value >> 11) & 0x1f) * 255 / 31);
    }
  }
  return bmp;
}

async function readScreen(label, previousHash) {
  for (let attempt = 0; attempt < 120; attempt += 1) {
    const frame = await call("GET", `/api/v1/vms/${encodeURIComponent(vm)}/console/frame/640x480`);
    const data = frame.payload?.data;
    if (frame.status === 200 && data?.frame_base64) {
      const raw = inflateSync(Buffer.from(data.frame_base64, "base64"));
      const hash = createHash("sha256").update(raw).digest("hex");
      const blank = raw.every((byte) => byte === 0);
      if (!blank && hash !== previousHash) {
        writeFileSync(join(outDir, `${label}.bmp`), toBmp(raw, data.width, data.height));
        summary.steps[label] = { sha256: hash, captured_at: data.captured_at, attempts: attempt + 1 };
        return hash;
      }
    }
    await new Promise((done) => setTimeout(done, 1000));
  }
  throw new Error(`${label}: the screen did not ${previousHash ? "change" : "appear"}`);
}

mkdirSync(outDir, { recursive: true });
let created = false;
try {
  const session = await call("POST", "/api/v1/auth/loopback-session", {});
  token = session.payload?.data?.access_token || "";
  summary.steps.session = { status: session.status };
  if (!token) throw new Error(`session: status ${session.status}`);
  await waitJob(await call("POST", "/api/v1/vms", { name: vm, iso_path: resolve(iso), generation: 2, cpu: 2, memory_mb: 2048, disk_gb: 32 }), "create");
  created = true;
  await waitJob(await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/start`, {}), "start");
  const before = await readScreen("screen-before", "");
  for (const keyCode of keys) {
    const sent = await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/console/input`, { kind: "key", action: "type", key_code: keyCode });
    summary.steps[`input-${keyCode}`] = { status: sent.status };
    if (sent.status !== 200) throw new Error(`input ${keyCode}: status ${sent.status} ${sent.payload?.error?.code || ""}`);
  }
  await readScreen("screen-after", before);
  summary.result = "pass";
} catch (error) {
  summary.result = "fail";
  summary.error = String(error?.message || error);
} finally {
  if (created) {
    try {
      await waitJob(await call("POST", `/api/v1/vms/${encodeURIComponent(vm)}/poweroff`, {}), "poweroff");
    } catch (error) {
      summary.steps.poweroff_error = String(error?.message || error);
    }
    try {
      await waitJob(await call("DELETE", `/api/v1/vms/${encodeURIComponent(vm)}`), "delete");
    } catch (error) {
      summary.steps.delete_error = String(error?.message || error);
    }
  }
  summary.finished_at = new Date().toISOString();
  writeFileSync(join(outDir, "summary.json"), `${JSON.stringify(summary, null, 2)}\n`);
  process.stdout.write(`S1 console scenario ${summary.result}: ${join(outDir, "summary.json")}\n`);
  process.exitCode = summary.result === "pass" ? 0 : 1;
}

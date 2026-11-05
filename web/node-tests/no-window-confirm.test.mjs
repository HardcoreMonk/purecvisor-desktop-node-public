import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

// Task 7b: the new shell must not call window.confirm. The eight mutating
// operations ask PCV.components.confirmDialog and skip the API when cancelled.

const webRoot = fileURLToPath(new URL("../", import.meta.url));
const API_NAMES = [
  "queueVmAttach",
  "previewVmQosStorage",
  "applyVmQosStorage",
  "previewVmQosNetwork",
  "applyVmQosNetwork",
  "queueVmGuestExec",
  "verifyVmGuestChannel",
  "ensureVmGuestChannel",
  "queueVmManage",
  "previewVmGuestFile",
  "queueVmGuestFile",
  "queueVmRename",
  "previewVmExport",
  "exportVm",
  "previewVmImport",
  "importVm",
  "connectVmNetwork",
  "addVmDevice"
];

function walkTs(dir, out = []) {
  for (const name of fs.readdirSync(dir)) {
    const full = path.join(dir, name);
    if (fs.statSync(full).isDirectory()) walkTs(full, out);
    else if (name.endsWith(".ts")) out.push(full);
  }
  return out;
}

function loadShell() {
  const calls = [];
  const api = {};
  const desktopApi = {};
  for (const name of API_NAMES) {
    api[name] = [];
    desktopApi[name] = async (...args) => {
      api[name].push(args);
      if (name === "previewVmGuestFile") return { size_bytes: 12 };
      return { job_id: "job-1" };
    };
  }
  const sandbox = {
    console,
    localStorage: { getItem() { return null; }, setItem() {} },
    PCV: {},
    state: {
      selectedVm: { id: "vm-1", name: "alpha", state: "Off" },
      actionPending: false,
      pendingVmActions: {},
      error: null,
      shellMessage: ""
    },
    desktopApi,
    acceptConfirm: false,
    requireRbac() {},
    normalizeError(error) { return error; },
    render() {},
    setVmActionPending() {},
    clearVmActionPending() {},
    pcvTrackJob() {},
    startPolling() {},
    findCachedVm() { return sandbox.state.selectedVm; },
    getVmName(vm) { return (vm && (vm.name || vm.id)) || "alpha"; },
    getVmState(vm) { return (vm && (vm.state || vm.status)) || "Off"; },
    rbacAllows() { return true; }
  };
  sandbox.window = sandbox;
  sandbox.globalThis = sandbox;
  sandbox.PCV.components = {
    confirmDialog: async (opts) => {
      calls.push(opts);
      return sandbox.acceptConfirm === true;
    }
  };
  const context = vm.createContext(sandbox);
  vm.runInContext(fs.readFileSync(path.join(webRoot, "i18n.js"), "utf8"), context, { filename: "i18n.js" });
  vm.runInContext(fs.readFileSync(path.join(webRoot, "src/modules/vm.ts"), "utf8"), context, { filename: "vm.ts" });
  vm.runInContext(fs.readFileSync(path.join(webRoot, "src/modules/vm-console.ts"), "utf8"), context, { filename: "vm-console.ts" });
  return { sandbox, api, calls };
}

test("new shell modules and bootstrap do not call window.confirm", () => {
  const files = walkTs(path.join(webRoot, "src/modules"));
  files.push(path.join(webRoot, "src/bootstrap.ts"));
  const hits = [];
  for (const file of files) {
    if (fs.readFileSync(file, "utf8").includes("window.confirm(")) hits.push(path.relative(webRoot, file));
  }
  assert.deepEqual(hits, []);
});

test("ISO attach confirm is reversible and cancel does not queue", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmAttach("vm-1", "D:\\iso\\a.iso");
  assert.equal(calls.length, 1);
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /가상 DVD/);
  assert.equal(api.queueVmAttach.length, 0);
});

test("QoS apply confirm is not danger, preview skips it, and cancel does not apply", async () => {
  const { sandbox, api, calls } = loadShell();
  const payload = { disk: "disk0", maximum_iops: 10, minimum_iops: 0 };
  await sandbox.queueVmQosDirectControl("vm-1", "storage", "preview", payload);
  assert.equal(calls.length, 0);
  assert.equal(api.previewVmQosStorage.length, 1);
  assert.equal(api.applyVmQosStorage.length, 0);
  await sandbox.queueVmQosDirectControl("vm-1", "storage", "apply", payload);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /작업 목록/);
  assert.equal(api.applyVmQosStorage.length, 0);
});

test("guest exec is not danger, repair is danger, verify skips confirm, and cancel does not call the API", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmGuestExecutionControl("vm-1", "verify", {});
  assert.equal(calls.length, 0);
  assert.equal(api.verifyVmGuestChannel.length, 1);
  await sandbox.queueVmGuestExecutionControl("vm-1", "exec", { command: ["hostname"] });
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /감사/);
  assert.equal(api.queueVmGuestExec.length, 0);
  await sandbox.queueVmGuestExecutionControl("vm-1", "repair", { yes: true });
  assert.equal(calls[1].danger, true);
  assert.match(calls[1].message, /되돌리기/);
  assert.equal(api.ensureVmGuestChannel.length, 0);
});

test("manage confirm is not danger and cancel does not queue", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmManage("vm-1");
  assert.equal(calls.length, 1);
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /관리형 삭제/);
  assert.equal(api.queueVmManage.length, 0);
});

test("guest file confirm can overwrite, preview still runs, and cancel does not queue", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmGuestFile("vm-1", { host_path: "C:\\host\\a.bin", guest_path: "C:\\guest\\a.bin" });
  assert.equal(api.previewVmGuestFile.length, 1);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].danger, true);
  assert.match(calls[0].message, /덮어/);
  assert.match(calls[0].message, /12/);
  assert.equal(api.queueVmGuestFile.length, 0);
});

test("rename confirm keeps the disks and cancel does not queue", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmRename("vm-1", "beta");
  assert.equal(calls.length, 1);
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /디스크/);
  assert.equal(api.queueVmRename.length, 0);
});

test("export apply is not danger, import apply is danger, and preview skips confirm", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmExportImportControl("vm-1", "export", "preview", { directory: "D:\\out" });
  assert.equal(calls.length, 0);
  assert.equal(api.previewVmExport.length, 1);
  await sandbox.queueVmExportImportControl("vm-1", "export", "apply", { directory: "D:\\out" });
  assert.equal(calls[0].danger, false);
  assert.match(calls[0].message, /패키지/);
  assert.equal(api.exportVm.length, 0);
  await sandbox.queueVmExportImportControl("vm-1", "import", "apply", { directory: "D:\\out", name: "beta" });
  assert.equal(calls[1].danger, true);
  assert.match(calls[1].message, /삭제/);
  assert.equal(api.importVm.length, 0);
});

test("network connect can drop the link, device add does not, and cancel does not call the API", async () => {
  const { sandbox, api, calls } = loadShell();
  await sandbox.queueVmNetworkChange("vm-1", "connect", { switch: "Default Switch" });
  assert.equal(calls[0].danger, true);
  assert.match(calls[0].message, /끊길/);
  assert.equal(api.connectVmNetwork.length, 0);
  await sandbox.queueVmNetworkChange("vm-1", "add", { device: "nic", switch: "Default Switch" });
  assert.equal(calls[1].danger, false);
  assert.match(calls[1].message, /기존 어댑터/);
  assert.equal(api.addVmDevice.length, 0);
});

test("accepting a confirm sends the request and a missing dialog does not", async () => {
  const { sandbox, api } = loadShell();
  sandbox.acceptConfirm = true;
  await sandbox.queueVmRename("vm-1", "beta");
  assert.equal(api.queueVmRename.length, 1);
  assert.deepEqual(api.queueVmRename[0], ["vm-1", "beta"]);
  sandbox.PCV.components.confirmDialog = undefined;
  await sandbox.queueVmAttach("vm-1", "D:\\iso\\a.iso");
  assert.equal(api.queueVmAttach.length, 0);
});

test("new confirm sentences exist in both ko and en", () => {
  const { sandbox, calls } = loadShell();
  sandbox.I18N._lang = "en";
  return sandbox.queueVmAttach("vm-1", "D:\\iso\\a.iso").then(() => {
    assert.match(calls[0].message, /virtual DVD/);
    assert.equal(calls[0].danger, false);
  });
});

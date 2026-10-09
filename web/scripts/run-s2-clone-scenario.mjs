// S2 scenario (ADR-0017): lock a managed Generation 2 VM as a template, clone it, and require the clone
// job to finish within 60 seconds. It names the same Local API routes the Web Console uses.
// --plan-only (the default) prints the steps without any request. --execute stays closed until a
// separate host-mutation approval. The loopback session token is never printed.
function getArg(name, fallback = "") {
  const prefix = `--${name}=`;
  const match = process.argv.find((value) => value.startsWith(prefix));
  return match ? match.slice(prefix.length) : fallback;
}

const execute = process.argv.includes("--execute");
const source = getArg("source", "pcv-it-s2-source");
const target = getArg("target", "pcv-it-s2-clone");
const vmRoot = getArg("vm-root", "D:\\PureCVisor\\VMs");
const boundMs = 60000;

const steps = [
  ["source", `GET /api/v1/vms/${source} must be managed Generation 2 and Off, with no checkpoints`],
  ["template-lock", `POST /api/v1/vms/${source}/template-lock locked=true confirm_name=${source}`],
  ["preview", `POST /api/v1/vms/${source}/clone/preview name=${target} vm_root=${vmRoot}`],
  ["clone", `POST /api/v1/vms/${source}/clone name=${target} vm_root=${vmRoot}`],
  ["bound", `wait until the clone job succeeds and elapsed_ms <= ${boundMs}`]
];

if (execute) {
  process.stderr.write("S2 clone scenario --execute needs a separate host-mutation approval.\n");
  process.exit(2);
}

process.stdout.write("S2 clone scenario plan-only (no request sent)\n");
steps.forEach(([name, detail], index) => process.stdout.write(`${index + 1}. ${name}: ${detail}\n`));

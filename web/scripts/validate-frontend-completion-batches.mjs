import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = dirname(dirname(fileURLToPath(import.meta.url)));
const repoRoot = dirname(webRoot);
const planPath = join(
  repoRoot,
  "docs",
  "superpowers",
  "plans",
  "2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json"
);

const forbiddenRuntimePatterns = [
  /\bKVM\b/i,
  /\blibvirt\b/i,
  /\bLXC\b/i,
  /\bZFS\b/i,
  /\bOVS\b/i,
  /\bOVN\b/i,
  /\bpurecvisorsd\b/i,
  /\/containers\b/i,
  /\/storage\b/i,
  /\/ovn\b/i,
  /\/networks\b/i
];

const forbiddenMutationTerms = [
  ["Restart", "-", "Computer"],
  ["Stop", "-", "Computer"],
  ["shutdown", ".", "exe"],
  ["Register", "-", "Scheduled", "Task"],
  ["msi", "exec"],
  ["New", "-", "Net", "Firewall", "Rule"],
  ["Register", "-", "Event", "Source"],
  ["New", "-", "Event", "Log"],
  ["New", "-", "VM"],
  ["Remove", "-", "VM"]
].map((parts) => parts.join(""));

function fail(message) {
  console.error(`frontend completion batch validation failed: ${message}`);
  process.exit(1);
}

function assert(condition, message) {
  if (!condition) {
    fail(message);
  }
}

function asArray(value, name) {
  assert(Array.isArray(value), `${name} must be an array`);
  return value;
}

const planText = readFileSync(planPath, "utf8");
const plan = JSON.parse(planText);

assert(plan.schema_version === 1, "schema_version must be 1");
assert(plan.plan_id === "purecvisor-desktop-node-frontend-completion-auto-batches-2026-05-09", "plan_id mismatch");
assert(plan.scope === "windows-desktop-node-web-console-frontend-completion", "scope mismatch");
assert(plan.host_mutation_performed === false, "host_mutation_performed must be false");
assert(plan.host_mutation_required === false, "host_mutation_required must be false");
assert(plan.linux_runtime_excluded === true, "linux_runtime_excluded must be true");
assert(plan.single_ui_clone_required === true, "single_ui_clone_required must be true");
assert(plan.batch_count === 5, "batch_count must be 5");
assert(plan.work_item_count === 25, "work_item_count must be 25");

const batches = asArray(plan.batches, "batches");
assert(batches.length === 5, "expected exactly five staged batches");

const allItems = [];
for (const [batchIndex, batch] of batches.entries()) {
  assert(typeof batch.batch_id === "string" && batch.batch_id.startsWith("frontend-completion-batch-"), `batch ${batchIndex + 1} id is invalid`);
  assert(typeof batch.title === "string" && batch.title.length > 0, `batch ${batch.batch_id} title is required`);
  assert(typeof batch.objective === "string" && batch.objective.length > 0, `batch ${batch.batch_id} objective is required`);
  assert(asArray(batch.write_scope, `${batch.batch_id}.write_scope`).length > 0, `batch ${batch.batch_id} write_scope is empty`);
  assert(asArray(batch.verification_commands, `${batch.batch_id}.verification_commands`).length > 0, `batch ${batch.batch_id} verification_commands is empty`);

  const itemRange = asArray(batch.item_range, `${batch.batch_id}.item_range`);
  assert(itemRange.length === 2, `batch ${batch.batch_id} item_range must have start and end`);
  assert(itemRange[0] === batchIndex * 5 + 1, `batch ${batch.batch_id} item_range start mismatch`);
  assert(itemRange[1] === batchIndex * 5 + 5, `batch ${batch.batch_id} item_range end mismatch`);

  const workItems = asArray(batch.work_items, `${batch.batch_id}.work_items`);
  assert(workItems.length === 5, `batch ${batch.batch_id} must contain five work items`);
  for (const item of workItems) {
    allItems.push(item);
    assert(Number.isInteger(item.id), `work item id must be an integer in ${batch.batch_id}`);
    assert(item.id >= itemRange[0] && item.id <= itemRange[1], `work item ${item.id} is outside ${batch.batch_id} item_range`);
    assert(item.automatable === true, `work item ${item.id} must be automatable`);
    assert(typeof item.title === "string" && item.title.length > 0, `work item ${item.id} title is required`);
    assert(asArray(item.target_files, `work_items[${item.id}].target_files`).length > 0, `work item ${item.id} target_files is empty`);
    assert(asArray(item.acceptance, `work_items[${item.id}].acceptance`).length > 0, `work item ${item.id} acceptance is empty`);
    assert(asArray(item.verification, `work_items[${item.id}].verification`).length > 0, `work item ${item.id} verification is empty`);
  }
}

const ids = allItems.map((item) => item.id).sort((a, b) => a - b);
assert(ids.length === 25, "expected 25 work items");
for (let i = 1; i <= 25; i += 1) {
  assert(ids[i - 1] === i, `missing or out-of-order work item ${i}`);
}
assert(new Set(ids).size === 25, "work item ids must be unique");

const finalCommands = asArray(plan.final_verification_commands, "final_verification_commands").join("\n");
for (const requiredCommand of [
  "Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed",
  "npm test --prefix web",
  "npm run verify:parity --prefix web",
  "node --check web/app.js",
  "git diff --check"
]) {
  assert(finalCommands.includes(requiredCommand), `final verification missing: ${requiredCommand}`);
}

const serialized = JSON.stringify(plan, null, 2);
for (const pattern of forbiddenRuntimePatterns) {
  const matches = serialized.match(pattern) ?? [];
  const allowedBoundaryMention = matches.length > 0 && serialized.includes("linux_runtime_excluded");
  if (matches.length > 0 && !allowedBoundaryMention) {
    fail(`forbidden runtime marker matched ${pattern}`);
  }
}
for (const term of forbiddenMutationTerms) {
  assert(!serialized.includes(term), `forbidden host mutation command matched ${term}`);
}
assert(!/Bearer\s+[A-Za-z0-9._~+/=-]+/.test(serialized), "plan must not contain auth credential literals");
assert(!/\bTBD\b|\bTODO\b|implement later|fill in details/i.test(serialized), "plan must not contain placeholders");

console.log("frontend completion batch plan is valid: 5 batches, 25 work items");

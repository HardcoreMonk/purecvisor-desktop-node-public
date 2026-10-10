import assert from "node:assert/strict";
import fs from "node:fs";
import { test } from "node:test";
import { fileURLToPath } from "node:url";

// BL-0016 (2026-10-10 S3 browser demo on 0.42.93): the VM detail click delegate handled every button[data-action]
// and re-rendered the panel at the end, so a type="submit" button inside a form lost its form before the browser
// ran the submit algorithm ("Form submission canceled because the form is not connected"). The guard below must stay
// in the TypeScript source and in the generated web/app.js that the installed service serves.

const SOURCE = fileURLToPath(new URL("../src/served-app.ts", import.meta.url));
const SERVED = fileURLToPath(new URL("../app.js", import.meta.url));
const VM_DETAIL = fileURLToPath(new URL("../src/served/render-vm-detail.ts", import.meta.url));
const GUARD = "if (button.type === 'submit' && button.form)";

function clickHandlerBlock(text, label) {
  const start = text.indexOf("els.vmDetailPanel.addEventListener('click'");
  assert.notEqual(start, -1, `${label}: vmDetailPanel click handler not found`);
  const tryIndex = text.indexOf("try {", start);
  assert.notEqual(tryIndex, -1, `${label}: click handler try block not found`);
  return text.slice(start, tryIndex);
}

test("vm detail click handler returns before any work for submit buttons inside a form (source)", () => {
  const block = clickHandlerBlock(fs.readFileSync(SOURCE, "utf8"), "served-app.ts");
  assert.match(block, /const button = event\.target\.closest\('button\[data-action\]'\);/);
  assert.ok(block.includes(GUARD), "guard missing from the click handler head");
  assert.ok(block.indexOf(GUARD) < block.indexOf("state.error = null"), "guard must run before the handler touches state");
});

test("the generated web/app.js carries the same guard", () => {
  const block = clickHandlerBlock(fs.readFileSync(SERVED, "utf8"), "app.js");
  assert.ok(block.includes(GUARD), "guard missing from the served bundle; run npm run build:served");
});

test("checkpoint schedule preview and save stay submit buttons with data-action so the guard covers them", () => {
  const detail = fs.readFileSync(VM_DETAIL, "utf8");
  for (const action of ["checkpoint-schedule-preview", "checkpoint-schedule-set"]) {
    assert.match(detail, new RegExp(`<button type="submit" data-action="${action}"`), `${action} must be a submit button`);
  }
});

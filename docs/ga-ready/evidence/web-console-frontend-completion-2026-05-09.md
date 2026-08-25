# Web Console Frontend Completion Evidence - 2026-05-09

evidence_id: web-console-frontend-completion-2026-05-09
scope: windows-desktop-node-web-console-frontend-only
host_mutation_performed: false
linux_runtime_excluded: true
single_ui_clone_required: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

The five staged frontend batches for the Desktop Node Web Console were applied as a
static TypeScript-owned Web Console update. The implementation keeps the
`purecvisor-single` Supanova operation-console shell and workbench UX shape while
using only Windows Desktop Node Local API routes and browser-local state.

No MSI/service/Hyper-V/firewall/trust-store/LAN/Event Log/update/rollback/reboot
mutation was executed by this frontend batch work.

The frontend payload was later installed and checked against the real listener in
`docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`. The
requested host mutation run `20260509-122028-0391-frontend` is recorded in
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-frontend.md`;
the final installed listener screenshot/QA baseline is
`20260509-130105-0391-frontend-final2`.

## Batch Closure

| Batch | Scope | Result |
|---|---|---|
| 1 | Shell/session, UI mapping, menu, asset explorer | PASS |
| 2 | Dashboard, VM detail, lifecycle, checkpoint UX | PASS |
| 3 | Jobs/activity, network, diagnostics, token UX | PASS |
| 4 | Problem-details, service health, evidence, fixture/test hardening | PASS |
| 5 | Browser parity, CSS polish, responsive/a11y, final evidence | PASS |

## Frontend Changes

- `web/DESIGN.md` now records the Single UI clone mapping and Windows-only boundary.
- `web/index.html` exposes session controls, Live viewer state, logout/browser clear
  boundary, VM state filter, and VM sort controls.
- `web/src/served-app.ts` adds browser-state clearing, VM state filtering/sorting,
  diagnostic bundle create/download UX, and route hardening monitoring signals.
- Final frontend hardening covers diagnostic retry/unsupported status, stale selected
  VM recovery, checkpoint restore/delete confirmation copy, network native parity
  failure guidance, and job edge-case summary rows.
- Diagnostic bundle UX distinguishes unsupported API, auth failure, 404, 500, and
  timeout/problem-details states with retry/disabled button policy.
- Token UX clears browser-local token/session state, refreshes all views, and keeps
  token-required route copy visible after refresh.
- `web/src/served/api-client.ts` supports diagnostic bundle raw download metadata
  without rendering secrets or command strings.
- `web/scripts/verify-browser-fixture.mjs` covers diagnostic bundle create/download,
  hardening signals, and the updated shell.
- `web/scripts/capture-installed-listener-qa.mjs` captures installed-listener
  dashboard, VM detail, jobs, network, troubleshooting, and responsive screenshots.
- `web/tests/PcvDesktopWeb.Static.Tests.ps1` guards the 1-25 staged batch contract,
  Single UI mapping, diagnostic API UX, service hardening signals, and no direct host
  mutation command strings.

## Installed Listener QA

`docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md`
records the installed-listener screenshot hashes and summary values from
preserved artifact root
`artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b`.
The run drove the real listener at `http://127.0.0.1:7777/`, exercised view
navigation, VM filter/sort, jobs, network, troubleshooting, diagnostic create,
and diagnostic download. The token was supplied only to the browser session and
the evidence records `token_value_observed=false`.

The final smoke cleanup left no selectable VM in inventory, so QA did not create
a new VM for selection. The no-selection detail state and VM filter/sort controls
were verified without new destructive VM lifecycle actions from the browser.

The follow-up destructive lifecycle UI run is recorded separately in
`docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md` with
artifact root
`artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391`. That run
drove the installed listener Web Console buttons for VM create, start, restart,
checkpoint create, poweroff, checkpoint restore, checkpoint delete, and VM
delete, then verified no `pcv-spike-ui-*` VM remained.

## Verification

Fresh verification run in this workspace:

```text
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
PASS: 39 passed, 0 failed
Follow-up hardening rerun: PASS: 40 passed, 0 failed

npm test --prefix web
PASS: tsc --noEmit, check:served, check:frontend-batches

npm run verify:parity --prefix web
PASS: check:served, static parity manifest, static parity verification, browser fixture

node web/scripts/capture-installed-listener-qa.mjs --url=http://127.0.0.1:7777/ --out=artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b
PASS: dashboard/vm-detail/jobs/network/troubleshooting screenshots, diagnostic create/download, responsive probes, token_value_observed=false

node web/scripts/capture-destructive-lifecycle-ui-qa.mjs --url=http://127.0.0.1:7777/ --out=artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391
PASS: VM create/start/restart/poweroff/delete and checkpoint create/restore/delete through installed Web Console UI, token_value_observed=false, cleanup.vm_absent_after_delete=true

node --check web/app.js
PASS

git diff --check
PASS
```

Existing LF/CRLF working-copy warnings were informational only.

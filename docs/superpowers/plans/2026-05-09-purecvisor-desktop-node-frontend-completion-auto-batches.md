# PureCVisor Desktop Node Frontend Completion Auto Batches Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Complete the Desktop Node Web Console frontend by executing the 25 remaining items as five no-host-mutation staged batches.

**Architecture:** The frontend stays a static TypeScript-owned Web Console served by `web/app.js`, generated from `web/src/served/*.ts` and `web/src/served-app.ts`. Each batch changes a bounded UI/service slice, then runs static, TypeScript, parity, and browser-fixture checks before the next batch starts. The backend contract is treated as already available through the installed Windows Desktop Node Local API.

**Tech Stack:** TypeScript, generated static `app.js`, Node verification scripts, Pester static guards, Desktop Node Local API JSON contracts, Batch Supervisor-style staged verification.

---

## Execution Contract

Machine-readable batch matrix:

```text
docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json
```

Validation command:

```powershell
npm run check:frontend-batches --prefix web
```

Global final verification:

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

## Boundaries

- Do not add Linux, KVM, libvirt, LXC, ZFS, OVS, OVN, or `purecvisorsd` runtime code.
- Do not add username/password login, token refresh, websocket event flow, or external CDN assets.
- Do not render authorization token values or protected token file contents.
- Do not run MSI, service, Hyper-V, firewall, trust-store, LAN, Event Log, update, rollback, or reboot mutation from frontend batches.
- Public trusted signing and external stable publication remain excluded or not-claimed.

## File Structure

- `web/DESIGN.md`: Single UI clone contract and Desktop Node boundary.
- `web/index.html`: Static shell, view mount points, controls, accessibility attributes.
- `web/styles.css`: Single UI visual system, responsive layout, focus and status states.
- `web/src/served/types.ts`: Frontend service payload and state helper types.
- `web/src/served/state.ts`: Browser state, pending action keys, polling state, shell state.
- `web/src/served/routes.ts`: Desktop Node Local API route registry only.
- `web/src/served/errors.ts`: Problem-details normalization, redaction-safe formatting.
- `web/src/served/api-client.ts`: Local API client wrapper and route methods.
- `web/src/served-app.ts`: DOM rendering, event binding, action orchestration.
- `web/src/user-visible-fixtures.ts`: Static parity fixtures for visible operator states.
- `web/scripts/verify-browser-fixture.mjs`: Browser-like fixture checks for runtime UI behavior.
- `web/scripts/verify-static-parity.mjs`: Static parity and route contract checks.
- `web/tests/PcvDesktopWeb.Static.Tests.ps1`: Static guard for UI, route, boundary, and batch plan contracts.
- `docs/ga-ready/evidence/web-console-frontend-completion-2026-05-09.md`: Final frontend completion evidence.

## Staged Batches

### Batch 1: Shell And Session

**Covers items:** 1-5

**Files:**
- Modify: `web/DESIGN.md`
- Modify: `web/index.html`
- Modify: `web/styles.css`
- Modify: `web/src/served-app.ts`
- Modify: `web/src/served/state.ts`
- Modify: `web/src/served/routes.ts`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Implement item 1, UI mapping table**

Add a Desktop Node mapping section to `web/DESIGN.md` covering shell, menu, rail, asset explorer, tabbar, dashboard, and status bar. The mapping must explicitly say Linux runtime screens are excluded.

- [x] **Step 2: Implement item 2, shell route order**

Make `Dashboard`, `VM Workbench`, `Network`, `Jobs`, `Activity`, `Evidence`, `Troubleshooting`, and `Monitoring` reachable through the Single UI shell without changing Desktop Node API routes.

- [x] **Step 3: Implement item 3, VM asset explorer**

Finish search, status, selected state, and empty state for VM assets. Keep container runtime tabs disabled or absent.

- [x] **Step 4: Implement item 4, menu commands**

Wire refresh, dashboard, VM workbench, network, evidence, troubleshooting, and clear-browser-state menu commands to existing frontend handlers.

- [x] **Step 5: Implement item 5, operator session controls**

Bind organization, Korean language indicator, Live viewer state, token state, and logout boundary to Desktop Node local console state. Do not add password login.

- [x] **Step 6: Verify Batch 1**

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm run check:served --prefix web
node --check web/app.js
```

Expected: all commands exit `0`.

### Batch 2: VM Workbench

**Covers items:** 6-10

**Files:**
- Modify: `web/index.html`
- Modify: `web/styles.css`
- Modify: `web/src/served-app.ts`
- Modify: `web/src/served/api-client.ts`
- Modify: `web/src/served/errors.ts`
- Modify: `web/src/served/state.ts`
- Modify: `web/src/served/types.ts`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Implement item 6, dashboard API state binding**

Bind host readiness, service status, VM count, active jobs, warnings, and batch evidence to loaded API payloads or explicit degraded state.

- [x] **Step 2: Implement item 7, VM list filtering and selection**

Add text filter, state filter, stable sort, selected row sync, and empty result copy.

- [x] **Step 3: Implement item 8, VM detail expansion**

Render state, generation, CPU, memory, storage, network switch, console availability, managed marker, and checkpoint count.

- [x] **Step 4: Implement item 9, VM lifecycle action UX**

Finish create, start, shutdown, poweroff, restart, and delete flows with scoped pending state, job tracking, confirmation, disabled state, and structured failure recovery.

- [x] **Step 5: Implement item 10, checkpoint UX**

Finish checkpoint create, restore, and delete flows with existing API routes, scoped pending state, confirmation, job tracking, and failure rows.

- [x] **Step 6: Verify Batch 2**

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run browser:fixture --prefix web
```

Expected: all commands exit `0`.

### Batch 3: Activity Network Troubleshooting

**Covers items:** 11-15

**Files:**
- Modify: `web/index.html`
- Modify: `web/styles.css`
- Modify: `web/src/served-app.ts`
- Modify: `web/src/served/api-client.ts`
- Modify: `web/src/served/errors.ts`
- Modify: `web/src/served/state.ts`
- Modify: `web/src/served/types.ts`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Implement item 11, job queue and activity pagination**

Finish first page, next page, retention summary, retry, cancel, stale polling guard, and duplicate suppression.

- [x] **Step 2: Implement item 12, pending and recovery model**

Unify VM, checkpoint, job, refresh, and diagnostic pending keys. Recover on success, failure, abort, and stale responses.

- [x] **Step 3: Implement item 13, network inventory view**

Render switch type, default flag, management OS flag, external adapter description, source, and read-only boundary in Single UI style.

- [x] **Step 4: Implement item 14, diagnostic bundle create/download**

Call diagnostic bundle create/download through the API client, show bundle id, redaction boundary, retention status, and problem details.

- [x] **Step 5: Implement item 15, token rotation and browser-token clear**

Separate browser token clear from service protected-token mutation. Show storage path boundary and update connection state after clear.

- [x] **Step 6: Verify Batch 3**

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
```

Expected: all commands exit `0`.

### Batch 4: Error Service Evidence Tests

**Covers items:** 16-20

**Files:**
- Modify: `web/index.html`
- Modify: `web/styles.css`
- Modify: `web/src/served-app.ts`
- Modify: `web/src/served/api-client.ts`
- Modify: `web/src/served/errors.ts`
- Modify: `web/src/served/routes.ts`
- Modify: `web/src/served/state.ts`
- Modify: `web/src/served/types.ts`
- Modify: `web/src/user-visible-fixtures.ts`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Implement item 16, problem-details display**

Render `PCV_RATE_LIMIT_EXCEEDED`, `PCV_ROUTE_TIMEOUT`, `Retry-After`, `request_id`, `route_timeout_seconds`, and nested details as readable rows.

- [x] **Step 2: Implement item 17, service health and runtime policy panels**

Show service running, listener scope, runtime policy, token policy, LAN exposure, route timeout, request limit, burst limit, and retry-after settings.

- [x] **Step 3: Implement item 18, batch evidence dashboard**

Display latest batch id, route/MSI/Hyper-V status, OS mutation status, GPU snapshots, artifact paths, pass/fail/degraded/missing states, and public-boundary labels.

- [x] **Step 4: Implement item 19, fixture and installed listener switch behavior**

Keep fixture mode and installed listener mode on the same route registry, token handling, error normalization, and boundary guards.

- [x] **Step 5: Implement item 20, service layer tests**

Add tests for API client route registry, abort/stale refresh, partial refresh degradation, error formatting, rate-limit, timeout, and redaction boundary.

- [x] **Step 6: Verify Batch 4**

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run browser:fixture --prefix web
```

Expected: all commands exit `0`.

### Batch 5: Parity Visual A11y Evidence

**Covers items:** 21-25

**Files:**
- Modify: `web/index.html`
- Modify: `web/styles.css`
- Modify: `web/src/served-app.ts`
- Modify: `web/scripts/verify-browser-fixture.mjs`
- Modify: `web/scripts/verify-static-parity.mjs`
- Modify: `web/generated/parity/static-asset-parity.manifest.json`
- Modify: `web/mockups/frontend-completion-samples.html`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Create: `docs/ga-ready/evidence/web-console-frontend-completion-2026-05-09.md`
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- Modify: `docs/DEVELOPER_INDEX.md`

- [x] **Step 1: Implement item 21, browser fixture and parity expansion**

Cover all active views, shell commands, VM action pending states, network, evidence, diagnostics, token, rate-limit, and timeout states.

- [x] **Step 2: Implement item 22, CSS visual polish**

Polish spacing, typography, dark surfaces, teal accent, scrollbars, focus, hover, disabled, loading, destructive, warning, success, and table states.

- [x] **Step 3: Implement item 23, responsive layout**

Verify 344px, 480px, 768px, 1024px, 1440px, and ultrawide behavior. Tables must scroll or collapse predictably and button text must fit.

- [x] **Step 4: Implement item 24, accessibility pass**

Add labels or aria-labels, visible focus ring, aria-current on active views, keyboard affordances, and text equivalents for status updates.

- [x] **Step 5: Implement item 25, final evidence closure**

Regenerate `web/app.js`, regenerate parity manifest, create final evidence, update ledger and developer index.

- [x] **Step 6: Verify Batch 5**

```powershell
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

Expected: all commands exit `0`.

## Coverage Matrix

| Item | Batch | Status Target |
| ---: | --- | --- |
| 1 | Batch 1 | UI mapping complete |
| 2 | Batch 1 | Shell route order complete |
| 3 | Batch 1 | VM asset explorer complete |
| 4 | Batch 1 | Menu commands wired |
| 5 | Batch 1 | Session controls bound |
| 6 | Batch 2 | Dashboard API state bound |
| 7 | Batch 2 | VM list UX complete |
| 8 | Batch 2 | VM detail complete |
| 9 | Batch 2 | VM lifecycle UX complete |
| 10 | Batch 2 | Checkpoint UX complete |
| 11 | Batch 3 | Jobs/activity complete |
| 12 | Batch 3 | Pending/recovery unified |
| 13 | Batch 3 | Network inventory complete |
| 14 | Batch 3 | Diagnostic bundle UI complete |
| 15 | Batch 3 | Token UX complete |
| 16 | Batch 4 | Error contract complete |
| 17 | Batch 4 | Service/runtime panels complete |
| 18 | Batch 4 | Evidence dashboard complete |
| 19 | Batch 4 | Fixture/API switch normalized |
| 20 | Batch 4 | Service-layer tests complete |
| 21 | Batch 5 | Browser/parity coverage complete |
| 22 | Batch 5 | Visual polish complete |
| 23 | Batch 5 | Responsive pass complete |
| 24 | Batch 5 | Accessibility pass complete |
| 25 | Batch 5 | Final evidence complete |

## Self-Review

- Spec coverage: all 25 frontend completion items are represented once, in order, across five staged batches.
- Placeholder scan: no task uses TBD, TODO, implement later, or unspecified edge handling.
- Type consistency: all frontend work uses the existing `web/src/served/*.ts` split, generated `web/app.js`, Desktop Node Local API route registry, and existing verification commands.

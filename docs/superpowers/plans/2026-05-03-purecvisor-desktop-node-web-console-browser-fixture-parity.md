# Web Console Browser Fixture Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Served `app.js` Web Console asset가 fixture Local API responses로 dashboard/VM/job DOM을 렌더링하는지 검증하는 browser-level parity smoke를 추가한다.

**Architecture:** 새 dependency를 추가하지 않고 Node `vm`과 최소 DOM/browser fixture를 사용해 `index.html` mount IDs와 served `app.js`를 실행한다. 기존 `npm run verify:parity`에 browser fixture smoke를 연결하되, served asset 교체, Playwright 도입, dev server, 실제 Local API/Hyper-V/MSI/service mutation은 하지 않는다.

**Tech Stack:** Node.js built-ins, existing static `app.js`, PowerShell/Pester docs guard, TypeScript parity scripts.

---

> 현행화 메모: 이 문서는 browser fixture parity가 처음 추가된 당시 `spikes/purecvisor-desktop-node/web/**` 경로의 execution record를 보존한다. 후속 served asset/root migration slice 이후 현재 제품 Web Console source와 검증 owner는 repo-root `web/**`이며, current commands는 `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js`, `Invoke-Pester -Path 'web/tests'`다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료, 푸시 완료
- mutation 범위: static Web Console verification script와 docs만 수정한다.
- 관리자 opt-in: 이 plan에서는 실제 VM 생성/시작/중지/삭제, checkpoint 생성/삭제, service/MSI/firewall/Event Log/trust-store mutation을 실행하지 않는다.
- Browser tooling: 이번 slice는 lightweight fixture runner만 추가한다. Playwright는 후속 도구 후보로 남긴다.

## Files

- Modify: `spikes/purecvisor-desktop-node/web/package.json`
  - Add `browser:fixture` script.
  - Extend `verify:parity` to run static parity check and browser fixture smoke.
- Create: `spikes/purecvisor-desktop-node/web/scripts/verify-browser-fixture.mjs`
  - Minimal DOM/browser fixture runner that executes served `app.js`, stubs `fetch`, localStorage, timers, dialog methods, and inspects rendered DOM text.
- Modify: `spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs`
  - Require package hook and script existence.
  - Reject host mutation strings and bearer secrets in the new script.
- Modify: `spikes/purecvisor-desktop-node/web/src/generate-parity-manifest.ts`
  - Add `browserFixture` metadata while preserving `regeneration.checkCommand` as `npm run verify:parity`.
- Modify: `spikes/purecvisor-desktop-node/web/generated/parity/static-asset-parity.manifest.json`
  - Regenerate generated manifest with browser fixture metadata.
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Guard package hook, script path, manifest metadata, and mutation-string absence.
- Modify docs:
  - `README.md`, `docs/GUIDE.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`, `follower.md`, relevant Phase 25 docs.

## Tasks

### Task 1: RED - browser fixture verification contract

- [x] **Step 1: Write failing verification expectations**

Update `spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs` to expect:

```js
browserFixtureScript: "scripts/verify-browser-fixture.mjs",
browserFixtureCommand: "npm run browser:fixture"
```

Require:

```js
requireEqual(packageJson.scripts?.["browser:fixture"], "node scripts/verify-browser-fixture.mjs", "package browser:fixture script");
requireIncludes(regenerateScript, expected.browserFixtureCommand, "scripts/regenerate-static-parity.mjs");
requireIncludes(manifestText, expected.browserFixtureScript, "generated manifest");
```

Scan the new browser fixture script path once it exists.

Update `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1` to require `browser:fixture`, `scripts/verify-browser-fixture.mjs`, and manifest `browserFixture.script`.

- [x] **Step 2: Verify RED**

Run:

```powershell
npm run verify:parity --prefix spikes/purecvisor-desktop-node/web
```

Expected: FAIL because `browser:fixture`, browser fixture script, and manifest metadata are not implemented yet.

### Task 2: GREEN - lightweight browser fixture runner

- [x] **Step 1: Implement fixture script**

Create `spikes/purecvisor-desktop-node/web/scripts/verify-browser-fixture.mjs` with:

- Minimal `document.getElementById`, `document.addEventListener("DOMContentLoaded", ...)`, `Element.innerHTML`, `Element.textContent`, `Element.addEventListener`, `Element.closest`, `className`, `dataset`, `value`, `showModal`, `close`.
- `window.location.origin = "http://127.0.0.1:7777"`.
- `window.localStorage` in-memory stub.
- `window.setInterval` and `window.clearInterval` no-op safe stubs.
- `window.confirm` returns `true` but the fixture does not click mutation buttons.
- `fetch` fixture responses for:
  - `/api/v1/host/status`
  - `/api/v1/vms`
  - `/api/v1/jobs/<fixture-job>`
- Execute served `app.js` from disk with `vm.runInNewContext`.
- Fire DOMContentLoaded, wait for async refresh, then assert rendered text includes:
  - `Host Overview`
  - `Ready`
  - `pcv-browser-fixture`
  - `running`
  - `job-browser-fixture`
- Assert rendered text does not include raw bearer token, host mutation command strings, or network errors.

- [x] **Step 2: Hook package and manifest**

Update `package.json`:

```json
"browser:fixture": "node scripts/verify-browser-fixture.mjs",
"verify:parity": "node scripts/regenerate-static-parity.mjs --check && node scripts/verify-static-parity.mjs && npm run browser:fixture"
```

Update `src/generate-parity-manifest.ts` manifest metadata:

```ts
browserFixture: {
  script: "scripts/verify-browser-fixture.mjs",
  command: "npm run browser:fixture",
  mode: "node-vm-minimal-dom",
  mutating: false,
  replacesServedAsset: false
}
```

Regenerate manifest:

```powershell
npm run generate:parity --prefix spikes/purecvisor-desktop-node/web
```

- [x] **Step 3: Run focused GREEN**

Run:

```powershell
npm run verify:parity --prefix spikes/purecvisor-desktop-node/web
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
```

Expected: both pass.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Record that DOM/browser fixture comparison is now a code-level static Web Console verification slice. Keep Playwright and real browser/dev server evidence as follow-up candidates.

- [x] **Step 2: Run verification**

Run:

```powershell
npm test --prefix spikes/purecvisor-desktop-node/web
npm run generate:parity --prefix spikes/purecvisor-desktop-node/web
npm run verify:parity --prefix spikes/purecvisor-desktop-node/web
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass.

- [ ] **Step 3: Commit and push**

Run:

```powershell
git add spikes/purecvisor-desktop-node/web docs README.md follower.md
git commit -m "Add web console browser fixture parity"
git push
```

## Completion Evidence

- RED: `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web` failed as expected because `browser:fixture`, `browserFixture` manifest metadata, and `scripts/verify-browser-fixture.mjs` were missing.
- Focused GREEN: `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web` passed, including `npm run browser:fixture`; `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"` passed.
- Full verification: `npm test --prefix spikes/purecvisor-desktop-node/web`, `npm run generate:parity --prefix spikes/purecvisor-desktop-node/web`, `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web`, `node --check` for `app.js` and parity scripts, Web Pester, `dotnet test src\DesktopNode.sln`, root documentation Pester, and `git diff --check` passed.
- Commit: `3cd3761 Add web console browser fixture parity` pushed to `codex/native-host-status-adapter`.

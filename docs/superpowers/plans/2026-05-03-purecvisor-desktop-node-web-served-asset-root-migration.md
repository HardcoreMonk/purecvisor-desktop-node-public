# PureCVisor Desktop Node Web Served Asset Root Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** TypeScript Web Console build output을 실제 served `app.js` owner로 승격하고, Web Console 제품 경로를 repo-root `web/**`로 이동하는 첫 GA-ready migration slice를 만든다.

**Architecture:** 기존 Local API static route와 MSI payload contract는 `/app.js`, `/index.html`, `/styles.css` URL과 payload layout을 유지한다. 소스 owner만 `web/src/served-app.ts`와 npm build/check script로 바꾸고, packaging은 repo-root `web/**`에서 payload `web/**`로 staging한다. PowerShell helper 제거는 이 slice에서 실행하지 않고 route matrix의 `transition-helper` row closure 후속 slice로 남긴다.

**Tech Stack:** TypeScript 5.9, Node.js ESM scripts, npm package scripts, PowerShell/Pester static contract tests, existing .NET Host static serving.

---

## Scope

포함:

- `app.js`를 TypeScript source에서 재생성되는 served build output으로 전환
- parity manifest의 `replacesServedAsset`와 runtime replacement 상태 갱신
- Node `vm` browser fixture가 generated `app.js`를 실행한다는 검증 유지
- Web Console 제품 package를 repo-root `web/**`로 이동
- packaging payload staging과 문서 검증 command를 repo-root `web/**`로 갱신

제외:

- `spikes/purecvisor-desktop-node/api/**`, `hyperv/**`, `service/**`, `cli/**` 이동
- PowerShell Local API 또는 Hyper-V helper 제거
- Hyper-V/service/MSI/firewall/Event Log/trust store mutation 실행
- Playwright required dependency 도입
- ADR-0004 current decision 승격 또는 aggregate closure report 생성

## File Structure

- Move: `spikes/purecvisor-desktop-node/web/**` -> `web/**`
  - 제품 Web Console source, static assets, tests, scripts를 repo-root web package로 둔다.
- Create: `web/src/served-app.ts`
  - 기존 browser UI runtime을 TypeScript-owned served source로 둔다.
- Create: `web/scripts/build-served-asset.mjs`
  - `web/src/served-app.ts`를 transpile해서 `web/app.js`를 만들거나 committed output freshness를 검사한다.
- Modify: `web/package.json`
  - `build:served`, `check:served`, `test`, `verify:parity` script를 추가/갱신한다.
- Modify: `web/scripts/regenerate-static-parity.mjs`
  - manifest가 served source/build output 전환을 기록하게 한다.
- Modify: `web/scripts/verify-static-parity.mjs`
  - side-by-side scaffold가 아니라 served replacement contract를 검증한다.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - repo-root web path의 generated `app.js`를 실행한다.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - served asset replacement와 repo-root path를 검증한다.
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
  - web payload source를 repo-root `web/**`로 전환한다.
- Modify: docs and README entrypoints
  - Web Console verification command를 `--prefix web`과 `web/app.js`로 갱신한다.

## Tasks

### Task 1: RED served asset replacement guard

- [x] `web/tests/PcvDesktopWeb.Static.Tests.ps1`에서 manifest와 package script가 `replacesServedAsset = true`, `runtimeReplacement = default`, `servedTypeScriptEntry = src/served-app.ts`, `check:served`를 요구하도록 바꾼다.
- [x] `npm run verify:parity --prefix web` 또는 Web Pester subset을 실행해 기존 `replacesServedAsset = false` 상태에서 실패함을 확인한다.

### Task 2: Build output owner

- [x] `web/src/served-app.ts`를 추가하고 기존 served UI runtime을 이 파일이 소유하게 한다.
- [x] `web/scripts/build-served-asset.mjs`를 추가한다.
- [x] `web/package.json`에 `build:served`, `check:served`, `test`, `verify:parity`를 연결한다.
- [x] `npm run build:served --prefix web`로 `web/app.js`를 생성한다.
- [x] `npm run check:served --prefix web`가 committed output freshness를 확인하게 한다.

### Task 3: Parity and fixture contract

- [x] parity manifest generator와 verifier를 served replacement contract에 맞춘다.
- [x] `npm run generate:parity --prefix web`로 manifest를 갱신한다.
- [x] `npm run verify:parity --prefix web`가 typecheck, served freshness, static parity, browser fixture를 모두 통과하게 한다.

### Task 4: Repo-root web migration

- [x] `spikes/purecvisor-desktop-node/web/**`를 `web/**`로 이동한다.
- [x] packaging build가 repo-root `web/app.js`, `web/index.html`, `web/styles.css`를 payload `web/**`로 staging하게 한다.
- [x] docs와 verification policy command를 `web/**` 기준으로 갱신한다.
- [x] root documentation guard에서 Web Console entrypoint가 repo-root `web/**`로 discoverable한지 확인한다.

### Task 5: Verification

- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`
- [x] `npm test --prefix web`
- [x] `npm run generate:parity --prefix web`
- [x] `npm run verify:parity --prefix web`
- [x] `npm run browser:fixture --prefix web`
- [x] `node --check web/app.js`
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`
- [x] `dotnet test src/DesktopNode.sln`
- [x] `git diff --check`

## PowerShell Helper Follow-up

PowerShell helper 제거는 이번 web slice의 일부가 아니다. 다음 slice는 route matrix에서 `transition-helper`인 served current routes를 하나씩 닫아야 한다. 우선순위는 Tier 1 read-only fallback removal evidence, 그 다음 Tier 2 checkpoint/VM reversible mutation adapter, 마지막 Tier 3 VM create/delete와 product ops mutation이다.

## Execution Evidence

- RED: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1' -FullName '*served app.js asset as TypeScript build output*'` failed because `src/served-app.ts` was missing.
- GREEN before migration: `npm test --prefix spikes/purecvisor-desktop-node/web`, `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web`, and Web Pester passed after `src/served-app.ts`, `build-served-asset.mjs`, and manifest updates.
- Repo-root migration: `spikes/purecvisor-desktop-node/web/**` moved to `web/**`; packaging product wrapper and installer staging now source Web Console assets from repo-root `web/**`.
- Final verification:
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`: 20 passed.
  - `npm test --prefix web`: passed.
  - `npm run generate:parity --prefix web`: regenerated current manifest.
  - `npm run verify:parity --prefix web`: passed, including `check:served` and browser fixture.
  - `npm run browser:fixture --prefix web`: passed.
  - `node --check web/app.js`: passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 35 passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 100 passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"`: 21 passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 17 passed.
  - `dotnet test src/DesktopNode.sln`: 103 passed.
  - `git diff --check`: exit 0.

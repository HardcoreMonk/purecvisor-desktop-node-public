# PureCVisor Desktop Node Phase 25 TypeScript Web Console 경계/스캐폴드 설계

## 목적

이 문서는 Phase 25의 TypeScript Web Console 경계와 스캐폴드 기준을 정의한다.

Phase 25의 상위 후보인 `DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first` 안에서 Web Console만 별도 slice로 좁힌다. 목표는 현재 정적 Web Console 계약을 깨지 않고 TypeScript source/build 구조를 검토할 수 있는 최소 경계를 만드는 것이다.

이 slice는 GA 승격, stable release 발행, 제품 runtime 교체, Local API host 교체, service/MSI 실행 경로 변경을 의미하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`와 Windows Desktop Node 전용 저장소 경계를 유지한다.

## 결정 후보

```text
DESKTOP_NODE_PHASE25_TYPESCRIPT_WEB_CONSOLE_BOUNDARY_CANDIDATE: static-asset-parity-scaffold-first
```

이 결정 후보는 ADR이 아니다. 다음 조건을 만족할 때만 후속 ADR 또는 Phase 25 plan 반영을 검토한다.

- TypeScript source가 기존 Web Console 정적 asset과 같은 public behavior를 생성한다.
- Local API static serving 경로가 바뀌지 않는다.
- repo-root `web/app.js` served asset parity 검증이 유지된다.
- token 값은 source, fixture, log, command line, screenshot, generated artifact에 노출되지 않는다.
- 제품 실행 경로와 packaging wrapper는 기존 PowerShell/정적 asset 흐름을 유지한다.

## 포함 범위

- Web Console TypeScript source 후보 디렉터리 구조
- Local API response type 후보
- VM/job/runtime policy/diagnostics view model 후보
- TypeScript build output이 기존 정적 asset으로 떨어지는 contract
- 정적 asset parity 검증 후보
- Web Console test/validation command 후보 문서화

## 제외 범위

- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- C++23 native runtime 구현
- .NET Local API host 교체
- 제품 runtime replacement
- GA 승격 또는 stable release 발행
- 실제 Hyper-V VM 생성/삭제/변경
- service install/start/stop/delete
- MSI install/repair/uninstall 또는 `REMOVE_DATA=1`
- Windows Firewall 변경
- Event Log source 등록
- Task Scheduler 등록
- `Restart-Computer` 또는 post-reboot mutation
- LAN mode default 변경

## 경계 원칙

1. 정적 asset parity가 먼저다.
   - TypeScript scaffold는 사용자가 별도 dev server를 실행해야만 동작하는 구조가 아니어야 한다.
   - 최종 Web Console은 Local API가 기존처럼 정적 파일로 serve할 수 있어야 한다.
   - build output은 기존 파일명, route, cache 기대값을 바꿀 경우 별도 contract test를 먼저 가져야 한다.

2. 런타임 경로는 바꾸지 않는다.
   - 이 slice는 `app.js`를 즉시 대체하지 않는다.
   - 먼저 TypeScript source와 generated output의 비교 가능성을 만든다.
   - product wrapper, installer, WinSW, Local API listener policy는 변경하지 않는다.

3. API contract는 읽기 전용 view model로 시작한다.
   - `/api/v1/runtime/policy`, job API, diagnostics response shape를 TypeScript type 후보로 mirror한다.
   - Web Console type은 서버 contract의 source of truth가 아니다.
   - 서버 contract 변경 없이 client-side compile-time validation만 추가하는 방향을 우선한다.

4. token은 절대 값으로 다루지 않는다.
   - UI fixture는 token literal을 포함하지 않는다.
   - CLI 예시는 장기 token 값을 command line에 쓰지 않는다.
   - token source가 필요한 설명은 protected token file 또는 token file path를 우선한다.

5. Windows-only repo boundary를 유지한다.
   - Web Console은 Desktop Node Windows Local API consumer다.
   - Linux supervisor, KVM/libvirt, LXC, ZFS, OVS, OVN runtime 화면이나 adapter scaffold를 추가하지 않는다.

## 스캐폴드 후보

현재 구현은 side-by-side source 구조다.

```text
web/
  app.js
  index.html
  tests/
  generated/
    parity/
      static-asset-parity.manifest.json
  scripts/
    regenerate-static-parity.mjs
    verify-static-parity.mjs
    verify-browser-fixture.mjs
  src/
    api-types.ts
    view-model.ts
    app.ts
    user-visible-fixtures.ts
```

이 구조는 기존 정적 asset을 수정하지 않고 TypeScript source와 generated parity artifact를 비교 및 수동 재생성 가능하게 둔다. `npm run generate:parity`는 `src/app.ts`의 exported contract literal을 TypeScript AST로 읽어 `generated/parity/static-asset-parity.manifest.json`을 다시 쓰고, `npm run verify:parity`는 커밋된 manifest가 재생성 결과와 같은지 확인한 뒤 `npm run browser:fixture`를 실행한다. `src/user-visible-fixtures.ts`는 기존 view-model helper가 만드는 empty/running/unsupported dashboard snapshot을 side-by-side fixture로 고정하되, served `app.js`나 `index.html`에는 연결하지 않는다. `scripts/verify-browser-fixture.mjs`는 Node `vm` 최소 DOM, in-memory localStorage, fixture `fetch` 응답으로 served `app.js`의 initial dashboard/VM/job render를 검증하지만 실제 Local API, dev server, Playwright, Hyper-V/service/MSI mutation을 실행하지 않는다. 다음 항목은 여전히 후속 판단이다.

- TypeScript compiler output을 기존 `app.js`와 직접 교체할지, 별도 generated file로 둘지 결정한다.
- source map, minification, bundling 여부가 diagnostics와 supportability를 해치지 않는지 확인한다.
- generated asset regeneration을 release build step으로 승격할지, 현재처럼 수동 검증 flow로 유지할지 결정한다.

## 정적 asset parity 기준

TypeScript Web Console scaffold는 다음 parity를 유지해야 한다.

- 기존 `index.html`이 기대하는 script loading 방식이 유지된다.
- 기존 Local API static serving route에서 같은 화면이 열린다.
- VM 목록, job 상태, runtime policy, diagnostics 표시의 user-visible copy가 의도 없이 바뀌지 않는다.
- TypeScript fixture는 view-model helper의 user-visible state 요약을 고정하되 DOM, browser API, product serving path를 직접 바꾸지 않는다.
- Browser fixture는 served `app.js`가 fixture Local API 응답으로 dashboard, VM table, tracked job panel을 렌더링하는지만 확인한다.
- API error, unauthorized, empty state, loading state의 DOM marker가 기존 tests와 호환된다.
- token 값이나 Authorization header 값은 DOM, console log, fixture snapshot에 나타나지 않는다.
- build output이 추가되어도 service/MSI/Hyper-V/firewall/Event Log/Task Scheduler/reboot 작업을 실행하지 않는다.

## Validation 후보

문서만 변경할 때:

```powershell
git diff --check
```

TypeScript scaffold가 추가될 때:

```powershell
npm test --prefix web
npm run generate:parity --prefix web
npm run verify:parity --prefix web
node --check web/app.js
node --check web/scripts/regenerate-static-parity.mjs
node --check web/scripts/verify-static-parity.mjs
node --check web/scripts/verify-browser-fixture.mjs
npm run browser:fixture --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Local API response type fixture가 추가될 때:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Packaging static asset integration이 바뀔 때:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
git diff --check
```

관리자 권한 opt-in 검증은 이 slice의 기본 검증이 아니다. 실제 Hyper-V lifecycle, service mutation, MSI lifecycle, firewall, Event Log, Task Scheduler, reboot 검증은 기존 Phase 20/21/23 gate를 따른다.

## 완료 기준

이 design note 자체는 다음을 만족하면 완료다.

- TypeScript Web Console slice가 정적 asset parity-first로 제한되어 있다.
- source scaffold, generated parity manifest, parity verification script가 기존 served `app.js`를 교체하지 않는다고 명시되어 있다.
- generated parity manifest는 `generate:parity`로 수동 재생성할 수 있고 `verify:parity`에서 stale 여부를 확인한다고 명시되어 있다.
- user-visible fixture parity는 TypeScript source/test scaffold에만 존재하며 `app.js` replacement가 아니라고 명시되어 있다.
- browser fixture parity는 served `app.js` initial render smoke이며 Playwright/dev server/실제 Local API 실행을 요구하지 않는다고 명시되어 있다.
- Windows-only repository boundary가 명시되어 있다.
- GA 승격과 제품 runtime replacement가 아님을 명시한다.
- Linux/KVM/libvirt 계열 runtime을 제외한다.
- service/MSI/Hyper-V/firewall/Event Log/Task Scheduler/reboot mutation 금지를 명시한다.
- token 값 비노출 원칙을 포함한다.
- 후속 validation 후보를 영향 범위별로 문서화한다.

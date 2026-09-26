# 모듈 크기 라쳇 복구 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-09-20~09-25 기능 commit으로 `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json` 상한을 넘은 모듈 `9`종을 순수 이동으로 상한 이하로 되돌려, `DesktopNode.Delivery.Tests`의 `module-ratchet-exceeded` 실패 `50`건을 해소한다.

**Architecture:** 새 패턴을 발명하지 않는다. 이미 쓰이고 있는 같은 타입 partial 분해(`DesktopNodeHyperVNativeAdapter.Reads/Mutations/Guest/Shared`, `DesktopNodeHostServiceAction.Commands/ServiceConfig/Shared/Token`, `DesktopNodeJobRuntime.Commands/Shared`, `DesktopNodeHostApplication.NoVnc/Request/StaticAuth`)를 반복한다. 두 대형 handler는 `internal sealed class`에 `partial`만 추가하고 다른 선언은 바꾸지 않는다. partial 분해는 IL과 호출 그래프를 바꾸지 않으므로 기존 ownership guard와 충돌하지 않는다.

**Tech Stack:** C# / .NET 10, xUnit, TypeScript(served build), Pester 5(라인 수 gate)

## 현재 상태

기준: public 저장소 `feat/p1-9-account-crud` HEAD `d5d1a47` (2026-09-27). 라인 수는 verifier의 `CountLines`와 같은 규칙(`\n` 개수, 마지막 줄에 개행이 없으면 `+1`)으로 쟀다.

| 모듈 | 실측 | 상한 | 초과 |
| --- | ---: | ---: | ---: |
| `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs` | `2570` | `1139` | `+1431` |
| `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs` | `2004` | `989` | `+1015` |
| `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Mutations.cs` | `1171` | `925` | `+246` |
| `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs` | `731` | `643` | `+88` |
| `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` | `502` | `449` | `+53` |
| `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` | `596` | `563` | `+33` |
| `src/DesktopNode.Runtime/DesktopNodeJobRuntime.Persistence.cs` | `663` | `645` | `+18` |
| `src/DesktopNode.Host/DesktopNodeHostApplication.cs` | `214` | `201` | `+13` |
| `web/src/served-app.ts` | `441` | `429` | `+12` |

나머지 `5`종(`NativeAdapter.Reads`, `WmiVmCloneProvider`, `JobRuntime`, `BatchEvidenceSummaryReader`, `BatchEvidenceSummaryReader.Pathing`)은 상한과 같거나 아래다.

`DevelopmentPolicyContractVerifier.ValidateModuleSizeFixture`는 첫 초과 모듈에서 예외를 던지고, 이 검증을 공유하는 Delivery test `50`건이 모두 같은 메시지 `PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|module-ratchet-exceeded`로 실패한다. 따라서 `9`종을 모두 고치기 전에는 Delivery.Tests가 green이 되지 않는다. task 단위 진척은 아래 실측 스크립트로 확인한다.

## Global Constraints

- **순수 이동만 한다.** 공개 표면, 시그니처, 응답 본문, 오류 코드, 평가 순서를 바꾸지 않는다. 개선 아이디어는 이동과 같은 commit에 섞지 않는다.
- **라쳇은 내리기만 한다.** `ValidateModuleRatchet`은 상한을 올리면 `module-ratchet-widened`, 실측이 상한을 넘으면 `module-ratchet-exceeded`, 상한이 실측보다 `slack_lines`(`50`) 이상 위에 남으면 `module-ratchet-stale`로 실패한다. 이동 후 여유가 `50`줄 이상이면 그 모듈의 `max_lines`를 실측값으로 내린다. 해당 항목에 `decomposition_plan`으로 이 문서 경로를 적는다.
- **신규 파일은 `500`줄 미만으로 만든다.** `500`줄 이상 신규 partial은 생성 시점에 fixture 등록이 필요하지만, verifier가 `modules.Length != 14`를 고정하므로 등록은 verifier 변경을 동반한다. 이 계획은 fixture 모듈 수를 `14`로 유지한다.
- 신규 partial 파일의 using/namespace/타입 선언 머리는 원본 파일과 같게 쓴다.
- `web/src/served-app.ts`를 바꾸면 `npm run build:served --prefix web`으로 `web/app.js`를 다시 만들고 같은 commit에 넣는다.
- host mutation, 설치본 생성, operational anchor 승격을 하지 않는다. `public_trusted_signing`과 `external_stable_publication`은 범위 밖이며 주장하지 않는다.
- task 하나가 checkpoint 하나다(30분, tool batch 18회). 캠페인 러너(`pcv-campaign-runner-v1`)로 실행하며, commit은 `docs/ga-ready/active-campaign.json`의 `commit_policy`를 따라 task마다 하나씩 만든다.

## 실측 스크립트

```powershell
$r = Get-Content packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json -Raw | ConvertFrom-Json
foreach ($m in $r.modules) {
  $t = [IO.File]::ReadAllText($m.path)
  $n = ([regex]::Matches($t, "`n")).Count + $(if ($t.EndsWith("`n")) { 0 } else { 1 })
  '{0} actual={1} max={2} gap={3}' -f $m.path, $n, $m.max_lines, ($m.max_lines - $n)
}
```

`gap`이 음수면 초과, `50` 이상이면 상한을 내려야 한다. 신규 파일은 같은 규칙으로 `500` 미만인지 확인한다.

## Task 1: Host 모듈 (`+33`, `+13`)

**수정:** `DesktopNodeHostServiceAction.cs`, `DesktopNodeHostApplication.cs`, fixture
**생성:** `src/DesktopNode.Host/DesktopNodeHostServiceActionDescriptors.cs`, `src/DesktopNode.Host/DesktopNodeHostApplication.Lifetime.cs`

- [x] `DesktopNodeHostServiceAction.cs` 상단의 descriptor record 묶음(`DesktopNodeHostConfigMigrationSource`부터 `DesktopNodeHostCredentialManagerTransitionDescriptor`까지, 약 `140`줄)을 `DesktopNodeHostServiceActionDescriptors.cs`로 옮긴다. 기대 실측은 약 `455`줄이므로 상한 `563`을 실측값으로 내린다.
- [x] `DesktopNodeHostApplication.cs`의 `Dispose`(약 `40`줄)를 `DesktopNodeHostApplication.Lifetime.cs` partial로 옮긴다. 기대 실측은 약 `174`줄이고 여유가 `50` 미만이라 상한은 그대로 둔다.
- [x] `dotnet test src/DesktopNode.Host.Tests`, 실측 스크립트.

## Task 2: Runtime과 Api request processor (`+18`, `+53`)

**수정:** `DesktopNodeJobRuntime.Persistence.cs`, `DesktopNodeApiRequestProcessor.cs`, fixture
**생성:** `src/DesktopNode.Runtime/DesktopNodeJobRuntime.Load.cs`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.Worker.cs`

- [x] `DesktopNodeJobRuntime.Persistence.cs`의 `LoadUnsafe`(약 `158`줄)를 `DesktopNodeJobRuntime.Load.cs`로 옮긴다. 기대 실측은 약 `505`줄이므로 상한 `645`를 실측값으로 내린다.
- [x] `DesktopNodeApiRequestProcessor.cs`의 `RunWorkerLoopAsync`(약 `72`줄)를 `DesktopNodeApiRequestProcessor.Worker.cs`로 옮긴다. 기대 실측은 약 `430`줄이고 여유가 `50` 미만이다. 기존 `decomposition_plan`(2026-08-06)은 유지하고 `note`에 이 계획을 덧붙인다.
- [x] `dotnet test src/DesktopNode.Runtime.Tests`, `dotnet test src/DesktopNode.Api.Tests`, 실측 스크립트.

## Task 3: HyperV native adapter (`+246`, `+88`)

**수정:** `DesktopNodeHyperVNativeAdapter.Mutations.cs`, `DesktopNodeHyperVNativeAdapter.cs`, fixture
**생성:** `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.ResourceMutations.cs`, `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Construction.cs`

- [x] `Mutations.cs`의 `TryInvokeVmResourceMutation`(약 `290`줄)과 `ResourceMutationParamFailure`(약 `14`줄)를 `ResourceMutations.cs`로 옮긴다. 기대 실측은 약 `867`줄이므로 상한 `925`를 실측값으로 내린다.
- [x] `DesktopNodeHyperVNativeAdapter.cs`의 연속된 생성자 overload `4`개(현재 `L357`~`L508`, 약 `152`줄)를 `Construction.cs`로 옮긴다. 기대 실측은 약 `579`줄이므로 상한 `643`을 실측값으로 내린다.
- [x] `dotnet test src/DesktopNode.HyperV.Tests`, 실측 스크립트.

실행 기록(2026-09-27): 연속 생성자 `4`개 대신, primary 생성자(필드 대입과 dispatch table)는 core에 남기고 `: this(...)` delegating overload `19`개 전부를 `Construction.cs`로 옮겼다. core 실측 `315`줄, `Construction.cs` `424`줄, `Mutations.cs` `867`줄, `ResourceMutations.cs` `312`줄. HyperV.Tests `220/220`.

## Task 4: Web served app (`+12`)

**수정:** `web/src/served-app.ts`, `web/app.js`(생성물)
**생성:** `web/src/served/` 아래 binding 모듈 `1`개

- [x] `bindEvents`(`L13`~`L364`, 약 `352`줄)에서 한 도메인의 event binding 묶음(`12`줄 이상)을 `web/src/served/`의 새 모듈로 옮기고 `bindEvents`에서 호출한다. 기존 served 모듈의 import/export 형태를 따른다.
- [x] `npm run build:served --prefix web`, `npm test --prefix web`, `npm run verify:parity --prefix web`. `web/app.js` diff가 이동에 해당하는 부분뿐인지 확인한다.
- [x] 실측 스크립트. 여유가 `50` 이상이면 상한을 내린다.

실행 기록(2026-09-27): part는 ES module이 아니라 `build-served-asset.mjs`가 연결하는 한 스코프라서, 새 part를 만들지 않고(part 목록 두 곳 수정 회피) 목록 filter/sort binding `28`줄을 기존 `src/served/table.ts`의 `bindListFilterEvents()`로 옮겼다. `served-app.ts` `414`줄(상한 `429`), `table.ts` `109`줄. `npm test`, `verify:parity`, web Pester `50/50` 통과.

## Task 5: `DesktopNodeApiVmMutationRouteHandler` (`+1015`)

**수정:** `DesktopNodeApiVmMutationRouteHandler.cs`(`partial` 추가), fixture
**생성:** 모두 `src/DesktopNode.Api/` 아래.

| 파일 | 옮길 멤버 | 약 줄 수 |
| --- | --- | ---: |
| `DesktopNodeApiVmMutationRouteHandler.QueuedRoute.cs` | `HandleQueuedMutationRoute` | `470` |
| `DesktopNodeApiVmMutationRouteHandler.ExportImport.cs` | `HandleVmExport`, `HandleVmImport`, `HandleVmExportPreview`, `HandleVmImportPreview` | `285` |
| `DesktopNodeApiVmMutationRouteHandler.Devices.cs` | `HandleVmDeviceAdd`, `CountDvd`, `HandleVmNetworkConnect`, `SwitchExists` | `230` |
| `DesktopNodeApiVmMutationRouteHandler.GuestQueue.cs` | `QueueVmGuestExec`, `QueueVmGuestChannelVerify`, `QueueVmGuestChannelEnsure`, `QueueVmGuestFile`, `HandleGuest*` | `255` |

- [x] 파일 하나씩 옮기고 매번 build한다. `QueuedRoute.cs`는 `500`줄에 가까우므로 생성 직후 실측한다.
- [x] core 기대 실측은 약 `815`줄이므로 상한 `989`를 실측값으로 내린다.
- [x] `dotnet test src/DesktopNode.Api.Tests`(ownership guard 포함), 실측 스크립트.

실행 기록(2026-09-27): 표의 멤버에 각 도메인 전용 helper(`MapVmExportImportError`, `ReadDeviceQuantity`, `ReadGeneration`, `CountArray`, `CountDvdDrives`, `MapVmDeviceAddError`, `MapNetworkChangeError`, `HandleGuestFilePreviewRoute`, `TryReadGuestFileRequest`)를 함께 옮겼다. core `657`줄, `QueuedRoute` `466`, `ExportImport` `299`, `Devices` `295`, `GuestQueue` `323`. Api.Tests `408/410`이며 실패 `2`건(`ApiHandlerAdapterContractTests`)은 clean baseline `5ec4f23`에서도 재현되는 기존 실패다.

## Task 6: `DesktopNodeApiJobReconciliationHandler` (`+1431`)

**수정:** `DesktopNodeApiJobReconciliationHandler.cs`(`partial` 추가), fixture
**생성:** 모두 `src/DesktopNode.Api/` 아래.

| 파일 | 옮길 멤버 | 약 줄 수 |
| --- | --- | ---: |
| `DesktopNodeApiJobReconciliationHandler.VmReconcile.cs` | `ReconcileVmDeleteJob`, `ReconcileVmCreateJob`, `ReconcileVmShutdownJob`, `ReconcileVmRestartJob` | `430` |
| `DesktopNodeApiJobReconciliationHandler.VmCapture.cs` | `CaptureVmCreateBaseline`, `CaptureVmShutdownBaseline`, `CaptureVmRestartBaseline`, `CaptureVmRenameBaseline`, `CaptureVmDeleteBaseline` | `410` |
| `DesktopNodeApiJobReconciliationHandler.Qos.cs` | `ReconcileVmQosJob`, `CaptureVmQosBaseline`, `TryReadCapturedVmQosBaseline`, `QosTarget*` | `300` |
| `DesktopNodeApiJobReconciliationHandler.CheckpointReconcile.cs` | `ReconcileCheckpoint*`, `BuildCheckpoint*` | `305` |
| `DesktopNodeApiJobReconciliationHandler.CheckpointCapture.cs` | `CaptureCheckpoint*`, `TryReadCapturedCheckpoint*` | `240` |
| `DesktopNodeApiJobReconciliationHandler.BaselineReaders.cs` | `TryReadCapturedRenameBaseline`, `TryReadCapturedDeleteBaseline`, `TryReadCapturedVmShutdownBaseline`, `TryReadCapturedVmRestartBaseline`, `TryReadCapturedVmCreateBaseline` | `210` |

- [x] checkpoint 계열 두 파일을 합치면 `500`줄을 넘으므로 나눈 상태를 유지한다.
- [x] core 기대 실측은 약 `750`줄이므로 상한 `1139`를 실측값으로 내린다.
- [x] `dotnet test src/DesktopNode.Api.Tests`, 실측 스크립트.

실행 기록(2026-09-27): `Build*Parameters`(큐 등록 시점 baseline 캡처 진입점)와 fingerprint helper는 core에 남겼다. 각 도메인 전용 helper와 nested baseline record는 해당 파일로 옮겼다(QoS target/policy helper는 `Qos`, checkpoint helper와 `VmCheckpoint*Baseline`은 `CheckpointCapture`, 나머지 `Vm*Baseline` record는 `BaselineReaders`). core `674`줄, `VmReconcile` `429`, `VmCapture` `406`, `Qos` `380`, `CheckpointReconcile` `198`, `CheckpointCapture` `301`, `BaselineReaders` `236`. Api.Tests `408/410`(기존 baseline 실패 `2`건).

## Task 7: 종료 검증

- [ ] 실측 스크립트에서 `14`종 모두 `0 <= gap < 50`.
- [ ] `dotnet test src/DesktopNode.sln` 전체 통과. `DesktopNode.Delivery.Tests` 실패 `0`.
- [ ] `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`.
- [ ] Task 4를 포함하면 `npm run test:required --prefix web`.
- [ ] `git diff --check`, commit 후 AGENTS.md의 Required CI 네 shard.

## Nonclaims

- 이 문서는 계획이며 코드 변경을 포함하지 않는다.
- 줄 수는 HEAD `d5d1a47` 기준 추정치다. 각 task는 착수 시 실측으로 경계를 다시 확인한다.
- 설치본, operational anchor, current-evidence를 바꾸지 않는다.

# PureCVisor Desktop Node P0-3 Hyper-V Saved 설계

- Design-ID: `purecvisor-desktop-node-p0-hyperv-saved-v1`
- 작성일: `2026-08-14`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: service-plan-p0-saved-20260814`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P0-3, §8
- 구현 계획: `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` Slice C
- 운영 앵커: `0.42.73-admin-smoke`
- 선행 payload: `feat/service-plan-p0-attach` P0-1/P0-2 (catalog `57`)
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 Hyper-V **Saved** suspend/resume-from-saved를 기존 `pause`/`resume`과 **다른
operation**으로 연다. pause를 개명하지 않는다. 실제 VM Save mutation은 설치본/actual-VM
evidence가 소유하며 이 slice의 required 조건이 아니다.

## 1. 문제

04273은 `POST /api/v1/vms/{id}/pause`로 RequestedState `9`(Paused)만 큐한다. 호스트
재부팅을 넘는 Hyper-V Saved(`EnabledState` `32769`)는 제품 경로가 없다. inventory는 이미
`32769 → "saved"`를 매핑한다.

Workstation의 suspend와 같은 운영 공백이다. pause를 Saved로 바꾸면 실행 중 pause 계약이
깨진다.

## 2. 목표와 비목표

### 목표

- `POST /api/v1/vms/{vmId}/save`가 queued `vm.save` job을 만든다. native
  `RequestStateChange` RequestedState `6` (CIM Offline). EnabledState `32769`는
  inventory/`RequireSaved` 매핑이다.
- `POST /api/v1/vms/{vmId}/resume-saved`가 queued `vm.resume-saved` job을 만든다.
  RequestedState `2`. 현재 매핑 상태가 `saved`가 아니면 `PCV_VM_NOT_SAVED`.
- `vm.pause`는 RequestedState `9`, `vm.resume`은 RequestedState `2`(paused용)를 유지한다.
- Web `Save` / `Resume saved`와 `pcvcli vm save` / `pcvcli vm resume-saved`가 같은 route.

### 비목표

- pause/resume 개명, pause Web 버튼 추가 (지금 Web detail에 pause/resume이 없는 상태는
  이 slice가 고치지 않는다)
- Saved reconcile, checkpoint, hibernate 파일 경로 노출
- `0.42.74` 또는 package-pair 개방
- 이 code-level slice에서 실제 Hyper-V Saved mutation
- TUI, public trusted signing, 외부 publication

## 3. 계약

### 3.1 Route

| 항목 | save | resume-saved |
| --- | --- | --- |
| Method/path | `POST /api/v1/vms/{vmId}/save` | `POST /api/v1/vms/{vmId}/resume-saved` |
| OperationName | `QueueSaveVm` | `QueueResumeSavedVm` |
| Job operation | `vm.save` | `vm.resume-saved` |
| Family | `hyperv-vm` | `hyperv-vm` |
| Stance | `QueuedMutation` | `QueuedMutation` |
| Permission | `operate` | `operate` |

Catalog: `57` → `59`. QueuedMutation: `23` → `25`. Family count `13` 유지. ReadOnly `22`,
ProductOperation `12` 유지.

Enqueue body 없음. job params:

```json
{ "name": "<vm display name>" }
```

성공 job result data:

```json
{ "name": "<vm>", "action": "save" }
```

resume-saved의 `action`은 `"resume-saved"`.

`http-transport-contract-v1.json` `route_count`도 `59`로 맞춘다. P0-1이 `57`로 고정한
핀을 깨지 않기 위함이다.

### 3.2 Native

`IDesktopNodeHyperVVmPowerStateProvider.Invoke` 시그니처는 유지한다. 허용 operation에
`vm.save`와 `vm.resume-saved`를 추가한다.

```csharp
public const ushort SavedState = 6; // CIM RequestedState Offline
public const ushort SavedEnabledState = 32769; // Hyper-V EnabledState Saved
```

pause가 요청값 `9`와 결과 `32768`을 분리한 것과 같다. 04274 actual-VM에서
RequestedState `32769`는 ReturnValue `32775`였고 CIM `6`은 Saved로 성공했다.

| operation | RequestedState | 상수 |
| --- | ---: | --- |
| `vm.pause` | 9 | `PausedState` (불변) |
| `vm.resume` | 2 | `EnabledState` (불변) |
| `vm.save` | 6 | `SavedState` |
| `vm.resume-saved` | 2 | `EnabledState` |

`vm.resume`과 `vm.resume-saved`는 같은 RequestedState `2`를 쓰지만 **사전조건이 다르다**.
resume-saved만 현재 상태가 `saved`인지 검사한다. pause resume에 `PCV_VM_NOT_PAUSED`를
추가하지 않는다.

현재 상태 읽기: `RequestStateChange`에 쓰는 그 `Msvm_ComputerSystem`의 `EnabledState`를
inventory와 **같은 표**로 매핑한다 (`DesktopNodeHyperVWmiVmProvider.MapEnabledState`:
`2→running`, `9/32768→paused`, `6/32769→saved`). 매핑 결과가 `"saved"`가 아니면

- code `PCV_VM_NOT_SAVED`
- HTTP: job failed (enqueue는 `202`)
- retryable `false`
- 다음 행동: Saved 상태인 VM에만 `vm resume-saved`를 쓴다. paused VM은 `vm resume`.

`vm.save`는 추가 사전상태를 강제하지 않는다. WMI `InvalidState`는 기존
`PCV_VM_POWER_STATE_FAILED` / WMI method failure 경로다.

helper fallback 없음.

### 3.3 실패 코드

| 코드 | 단계 | 다음 행동 |
| --- | --- | --- |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름을 고친다 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM을 확인 |
| `PCV_VM_NOT_SAVED` | job failed | 상태가 `saved`일 때만 resume-saved |
| `PCV_VM_POWER_STATE_FAILED` | job failed, retryable 기존과 동일 | 진단 후 수동 재시도 |
| `PCV_OPERATION_NOT_ALLOWED` | job failed | save/resume-saved/pause/resume 등 허용 operation만 |

### 3.4 Catalog / policy

`vm.save`와 `vm.resume-saved`를 `vm.resume` 바로 뒤에 넣는다.

- Domain / dispatch: `VmLifecycle` / `vm-power-state-provider` / `VmPowerState`
- Wmi provider catalog power-state list
- RuntimePolicy `NativeMutationOperations`와 `NativeCore.Reason`
- `DesktopNodeApiHyperVOperationInvoker` allowlist
- API route catalog 위 표

pause/resume 행과 상수는 수정하지 않는다.

### 3.5 CLI / Web

- CLI: `pcvcli vm save <vm>`, `pcvcli vm resume-saved <vm>`. 하이픈 없는
  `vm resume saved`는 거부한다 (`vm resume`과 충돌). `--yes` 없음.
- interactive help: `vm save | Save VM to Hyper-V Saved state`,
  `vm resume-saved | Resume a VM from Hyper-V Saved state`
- Web: VM detail lifecycle 버튼 `Save`, `Resume saved`. confirmation은 VM 표시 이름과
  **현재 state**를 보여 주고, Save가 pause가 아님을 한 줄로 적는다. RBAC `operate`.
- `requireRouteAction` allowlist에 `save`, `resume-saved`를 추가한다.
- coverage: `vm.save` = `POST /api/v1/vms/{vm_id}/save`,
  `vm.resume-saved` = `POST /api/v1/vms/{vm_id}/resume-saved`.

기존 start/shutdown/poweroff/restart/eject 버튼과 pause API는 그대로 둔다.

### 3.6 검증

Code-level:

- `SavedState == 6` (RequestedState), `SavedEnabledState == 32769`, pause `9` / resume `2` 불변
- catalog `59`, QueuedMutation `25`, digest 갱신, transport `route_count` `59`
- native adapter InlineData `vm.save`/`vm.resume-saved`
- resume-saved: mapped state `saved`가 아니면 `PCV_VM_NOT_SAVED`; `saved`이면 RequestedState `2`
- API enqueue `202` `vm.save` / `vm.resume-saved`, helper fallback 없음
- CLI catalog, Web fixture/static, `node scripts/build-served-asset.mjs --write`

설치본/actual-VM Saved smoke는 SERVICE_PLAN 완료 조건이지만 **이 slice의 required가
아니다**. evidence에 `actual_vm_validation: not-run`을 명시한다.

## 4. pause와의 차이

| | pause / resume | save / resume-saved |
| --- | --- | --- |
| RequestedState | 9 / 2 | 6 / 2 |
| inventory | `paused` | `saved` |
| 호스트 재부팅 | 유지되지 않음 | Hyper-V Saved로 유지 |
| Web (이번 slice) | 버튼 추가 안 함 | `Save` / `Resume saved` |
| 사전조건 | 없음 (기존) | resume-saved만 `saved` |

## 5. 비주장

- operational current는 `0.42.73-admin-smoke` 유지
- host mutation performed `false`
- actual-VM Saved PASS를 이 evidence가 주장하지 않음
- public trusted signing / external stable publication `not-claimed`

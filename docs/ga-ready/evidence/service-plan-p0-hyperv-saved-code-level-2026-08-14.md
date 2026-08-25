# Service Plan P0-3 Hyper-V Saved code-level PASS (2026-08-14)

evidence_id: `service-plan-p0-hyperv-saved-code-level-2026-08-14`
slice_id: `service-plan-p0-hyperv-saved-code-level-2026-08-14`
result: `CODE_LEVEL_PASS`
Design-ID: `purecvisor-desktop-node-p0-hyperv-saved-v1`
approval_locator: `User-Approval: service-plan-p0-saved-20260814`
spec: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-hyperv-saved-design.md`
plan: `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md`
change_tier: `M`
verification_lane: `Full`
operational_current: `0.42.73-admin-smoke`
operational_current_changed: `false`
host_mutation_performed: `false`
package_build_performed: `false`
package_candidate_created: `false`
installed_product_changed: `false`
installed_smoke: `not-run`
actual_vm_validation: `not-run`
public_trusted_signing: `false`
external_stable_publication: `false`

## 판정

Hyper-V Saved suspend/resume-from-saved를 기존 `pause`/`resume`과 다른 operation으로
code-level 제품 경로에 열었다. Local API `POST /api/v1/vms/{vmId}/save`와
`POST /api/v1/vms/{vmId}/resume-saved`는 queued `vm.save` / `vm.resume-saved` job을
만든다. Web `Save` / `Resume saved`와 `pcvcli vm save` / `pcvcli vm resume-saved`가
같은 route를 쓴다. pause RequestedState `9`와 resume RequestedState `2`는 바꾸지
않았다. Web pause/resume 버튼은 추가하지 않았다.

이 기록은 Task 17 native/catalog/API와 Task 18 CLI/Web/docs 구현 검증을 묶은
code-level 범위다. 설치본 smoke, actual-VM Saved mutation, 다음 package campaign,
operational current 승격은 주장하지 않는다. SERVICE_PLAN 완료 조건의 actual-VM
Saved PASS는 이후 evidence가 소유한다.

## 계약

- Route: `POST /api/v1/vms/{vmId}/save` / `QueueSaveVm` / family `hyperv-vm` /
  `QueuedMutation`. `POST /api/v1/vms/{vmId}/resume-saved` / `QueueResumeSavedVm`.
  catalog count `59`. QueuedMutation count `25`.
- Job operation: `vm.save` / `vm.resume-saved`. enqueue `202` params `{ name }`.
- Native: `vm.save` RequestedState `SavedState` `32769`, action `"save"`.
  `vm.resume-saved`는 매핑 상태가 `"saved"`일 때만 RequestedState `EnabledState` `2`,
  아니면 `PCV_VM_NOT_SAVED`. helper fallback 없음.
- CLI: `pcvcli vm save <vm>`과 `pcvcli vm resume-saved <vm>`. `--yes` 없음.
  하이픈 없는 `vm resume saved` 두 단어는 거부한다.
- Web: VM detail lifecycle `Save` / `Resume saved`. confirmation은 VM 표시 이름과
  현재 state를 보여 주고, Save가 pause가 아님을 한 줄로 적는다. Resume saved는
  현재 state가 saved일 때만 유효하다고 안내한다. RBAC `operate`. coverage
  `vm.save` / `vm.resume-saved`.
- served route: `POST /api/v1/vms/{vm_id}/save`,
  `POST /api/v1/vms/{vm_id}/resume-saved`.

### 문제 코드

| code | 단계 | 의미 |
| --- | --- | --- |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름이 유효하지 않음 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM이 없음 |
| `PCV_VM_NOT_SAVED` | job failed | 상태가 `saved`가 아님. resume-saved만 |
| `PCV_VM_POWER_STATE_FAILED` | job failed, retryable 기존과 동일 | native power-state 실패 |
| `PCV_OPERATION_NOT_ALLOWED` | job failed | 허용 operation만 |

## 검증 결과

Task 17 native/catalog/API는 해당 커밋에서 관측한 결과다. Task 18은 CLI/Web/docs를
이 작업에서 다시 실행했다.

| 검증 | 결과 | 출처 |
| --- | --- | --- |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj` | PASS `149/149` | Task 17 |
| `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter FullyQualifiedName~RuntimePolicyContractTests` | PASS `13/13` | Task 17 |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests` | PASS `13/13` | Task 17 |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests` | PASS `155/155` | Task 17 |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~DesktopNodeHttpTransportContractTests` | PASS `10/10` | Task 17 |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj` | PASS `139/139` | Task 18 |
| `npm test --prefix web` | PASS (`tsc --noEmit`, `check:served`, `check:frontend-batches`) | Task 18 |
| `npm run verify:parity --prefix web` | PASS (`check:served`, static parity, `browser:fixture`) | Task 18 |
| `Invoke-Pester -Path web/tests -Output Detailed` | PASS `49/49` | Task 18 |
| `git diff --check` | PASS (출력 없음) | Task 18 |

## 의도적으로 남긴 항목

- 설치본 Hyper-V Saved smoke와 package/fullgate/manual-admin campaign은 이 slice의
  required 조건이 아니다.
- SERVICE_PLAN 완료 조건의 actual-VM Saved PASS는 `actual_vm_validation: not-run`으로
  남긴다. 이 evidence가 그 PASS를 대신하지 않는다.
- Web pause/resume 버튼은 기존처럼 없다. 이 slice가 고치지 않는다.
- Slice D managed import는 시작하지 않았다.

## Nonclaims

- 설치본 smoke와 actual-VM Saved mutation은 `not-run`이다. 실제 Hyper-V/VM mutation을
  실행하지 않았다.
- `package_candidate_created=false`. `0.42.74`와 `0.42.73 -> next` campaign을 열지
  않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- operational current는 `0.42.73-admin-smoke` 그대로다. `operational_current_changed=false`.
- pause를 Saved로 개명하지 않았고 pause RequestedState 상수를 바꾸지 않았다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작을 실행하지 않았다.

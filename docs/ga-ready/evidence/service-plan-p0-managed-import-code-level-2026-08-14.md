# Service Plan P0-4 managed import code-level PASS (2026-08-14)

evidence_id: `service-plan-p0-managed-import-code-level-2026-08-14`
slice_id: `service-plan-p0-managed-import-code-level-2026-08-14`
result: `CODE_LEVEL_PASS`
Design-ID: `purecvisor-desktop-node-p0-managed-import-v1`
approval_locator: `User-Approval: service-plan-p0-managed-import-20260814`
spec: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-managed-import-design.md`
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

이미 있는 Hyper-V VM에 managed marker를 opt-in으로 붙이는 운영자 경로를
code-level 제품 표면에 열었다. Local API `POST /api/v1/vms/{vmId}/manage`는
`{ "confirm_name": "<decoded vmId>" }`를 받아 queued `vm.manage` job을 만든다.
Web `Manage VM`과 `pcvcli vm manage <vm> --yes`가 같은 route를 쓴다.

이 기록은 Task 22 native/catalog/API와 Task 23 CLI/Web/docs 구현 검증을 묶은
code-level 범위다. 설치본 Notes mutation, actual-VM manage, 다음 package campaign,
operational current 승격은 주장하지 않는다. unmanaged delete 거절
`PCV_VM_NOT_MANAGED_BY_PURECVISOR`는 유지한다.

## 계약

- Route: `POST /api/v1/vms/{vmId}/manage` / `QueueManageVm` / family `hyperv-vm` /
  `QueuedMutation`. catalog count `60`. QueuedMutation count `26`.
- Job operation: `vm.manage`. enqueue `202` params `{ name }`.
- Native: Notes에 `managed-by=purecvisor-desktop-node`를 append. 이미 있으면
  WMI write 없이 `action=already-managed`. helper fallback 없음.
- CLI: `pcvcli vm manage <vm> --yes`. 인자는 정확히 4개. `--yes`가 없으면
  `PCV_CLI_CONFIRMATION_REQUIRED`. body `confirm_name`은 `<vm>` 인자 그대로.
- Web: VM detail lifecycle `Manage VM`. confirmation은 Hyper-V 표시 이름과
  성공 후 이 VM이 managed delete 가드를 통과한다는 점, unmanaged delete 거절은
  유지된다는 점을 보여 준다. POST `confirm_name`은 다른 lifecycle 버튼과 같은
  URL path identifier. RBAC `operate`. coverage `vm.manage`.
- served route: `POST /api/v1/vms/{vm_id}/manage`.

### 문제 코드

| code | 단계 | 의미 |
| --- | --- | --- |
| `PCV_CLI_CONFIRMATION_REQUIRED` | CLI enqueue 전 | `--yes` 없음 |
| `PCV_VM_MANAGE_CONFIRMATION_MISMATCH` | 400 enqueue | `confirm_name`이 디코드된 `{vmId}`와 Ordinal 불일치 |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름이 유효하지 않음 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM이 없음 |
| `PCV_VM_SETTINGS_NOT_FOUND` | job failed, retryable true | 설정 조회 실패 |
| `PCV_VM_NOT_MANAGED_BY_PURECVISOR` | job failed (delete) | manage 후에만 delete. 이 slice가 완화하지 않음 |

## 검증 결과

Task 22 native/catalog/API는 해당 커밋에서 관측한 결과다. Task 23은 CLI/Web/docs를
이 작업에서 다시 실행했다.

| 검증 | 결과 | 출처 |
| --- | --- | --- |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj` | PASS `156/156` | Task 22 |
| `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter FullyQualifiedName~RuntimePolicyContractTests` | PASS `13/13` | Task 22 |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests` | PASS `13/13` | Task 22 |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests` | PASS `164/164` | Task 22 |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~DesktopNodeHttpTransportContractTests` | PASS `10/10` | Task 22 |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj` | PASS `143/143` | Task 23 |
| `npm test --prefix web` | PASS (`tsc --noEmit`, `check:served`, `check:frontend-batches`) | Task 23 |
| `npm run verify:parity --prefix web` | PASS (`check:served`, static parity, `browser:fixture`) | Task 23 |
| `Invoke-Pester -Path web/tests -Output Detailed` | PASS `49/49` | Task 23 |
| `git diff --check` | PASS (출력 없음) | Task 23 |

## 의도적으로 남긴 항목

- 설치본 Hyper-V Notes mutation smoke와 package/fullgate/manual-admin campaign은
  이 slice의 required 조건이 아니다.
- SERVICE_PLAN 완료 조건의 설치본 evidence는 `installed_smoke: not-run`으로 남긴다.
  이 evidence가 그 PASS를 대신하지 않는다.
- P1 managed full clone은 시작하지 않았다.
- unmanaged delete 거절 문구와 `PCV_VM_NOT_MANAGED_BY_PURECVISOR` 가드는 유지한다.

## Nonclaims

- 설치본 smoke와 actual-VM Notes mutation은 `not-run`이다. 실제 Hyper-V/VM mutation을
  실행하지 않았다.
- `package_candidate_created=false`. `0.42.74`와 `0.42.73 -> next` campaign을 열지
  않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- operational current는 `0.42.73-admin-smoke` 그대로다. `operational_current_changed=false`.
- OVF/VHDX copy, full clone, export/import, unmanage를 열지 않았다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작을 실행하지 않았다.

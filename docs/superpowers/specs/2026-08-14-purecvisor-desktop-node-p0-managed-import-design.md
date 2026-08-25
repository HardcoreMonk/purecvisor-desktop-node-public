# PureCVisor Desktop Node P0-4 managed 승격 설계

- Design-ID: `purecvisor-desktop-node-p0-managed-import-v1`
- 작성일: `2026-08-14`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: service-plan-p0-managed-import-20260814`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P0-4, §8
- 구현 계획: `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` Slice D
- 운영 앵커: `0.42.73-admin-smoke`
- 선행 payload: `feat/service-plan-p0-attach` P0-1..P0-3 (catalog `59`)
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 이미 있는 Hyper-V VM에 **managed marker를 opt-in으로 붙이는** 경로만 연다.
OVF/VHDX copy/clone/export가 아니다. unmanaged delete 거절은 유지한다.

## 1. 문제

create는 Notes에 `managed-by=purecvisor-desktop-node`를 넣는다. delete는 이 marker가 없으면
`PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 막는다. 실험실에 먼저 만든 Hyper-V VM은 inventory에
보이지만 managed가 아니어서 제품 delete를 쓸 수 없다.

Workstation의 “기존 VM 들이기”에 해당하는 공백이다. 아무 VM이나 지우게 열면 안 된다.

## 2. 목표와 비목표

### 목표

- `POST /api/v1/vms/{vmId}/manage`가 `{ "confirm_name": "<display-name>" }`를 받아 queued
  `vm.manage` job을 만든다.
- native adapter가 Notes에 marker를 **append**한다. 기존 notes를 지우지 않는다.
- 이미 marker가 있으면 WMI write 없이 `action=already-managed`로 성공한다.
- 성공 후 `managed_by_purecvisor=true`이고 그 VM의 delete 가드가 통과한다.
- unmanaged VM delete는 계속 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`.
- Web와 `pcvcli vm manage --yes`가 같은 route를 쓴다.

### 비목표

- OVF/VHDX 복사, full clone (P1), export/import (P2)
- Notes 전체 재작성, marker 제거/unmanage
- 아무 이름이나 맞춰 주는 fuzzy confirm
- `0.42.74` 또는 package-pair 개방
- 실제 Hyper-V Notes mutation (code-level만)
- TUI, public trusted signing, 외부 publication

## 3. 계약

### 3.1 Route

| 항목 | 값 |
| --- | --- |
| Method/path | `POST /api/v1/vms/{vmId}/manage` |
| OperationName | `QueueManageVm` |
| Job operation | `vm.manage` |
| Family | `hyperv-vm` |
| Stance | `QueuedMutation` |
| Permission | `operate` |
| Catalog | `59` → `60` |
| QueuedMutation | `25` → `26` |

Body:

```json
{ "confirm_name": "lab-vm" }
```

`confirm_name`은 디코드된 `{vmId}`와 **Ordinal**로 같아야 한다. 공백/대소문자/다른 표시
이름은 `400 PCV_VM_MANAGE_CONFIRMATION_MISMATCH`이며 enqueue하지 않는다. 필드 없음·공백도
같은 코드다.

Enqueue `202` params:

```json
{ "name": "<decoded vmId>" }
```

성공 job result:

```json
{ "name": "<vm>", "action": "manage" }
```

이미 managed면 `"action": "already-managed"`.

`http-transport-contract-v1.json` `route_count`는 `60`.

### 3.2 Native

```csharp
public interface IDesktopNodeHyperVVmManageProvider
{
    DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVVmManageInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action);
```

`DesktopNodeHyperVWmiVmManageProvider`는 rename과 같이
`Msvm_VirtualSystemManagementService.ModifySystemSettings`를 쓴다.

1. `FindVm` / 현재 `Msvm_VirtualSystemSettingData` (rename의 Realized 제외 규칙).
2. Notes를 읽는다. `IsManagedVm`과 같은 판정:
   `Contains("managed-by=purecvisor-desktop-node", OrdinalIgnoreCase)`.
3. 이미 있으면 WMI write 없이 `action=already-managed`. 두 번째 marker 줄을 넣지 않는다.
4. 없으면 기존 Notes 문자열을 유지하고 끝에 marker를 append한다. 빈 Notes면 marker만
   넣는다. 기존 내용 앞에 덮어쓰지 않는다.
5. `ModifySystemSettings` 후 `action=manage`.

marker 상수와 inventory `IsManagedVm`은 같은 문자열을 쓴다. 표를 두 벌로 두지 않는다
(상수 `managed-by=purecvisor-desktop-node`를 공유).

없는 VM → `PCV_VM_NOT_FOUND`. helper fallback 없음.

### 3.3 Delete 가드

`TryInvokeVmDelete`의 unmanaged 거절은 그대로다. 이 slice는 그 분기를 지우거나
완화하지 않는다.

증명:

- unmanaged list row로 delete → 계속 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, delete provider
  호출 0.
- manage 성공 후 `managed_by_purecvisor=true`인 row로 delete → 가드를 통과하고 delete
  provider가 호출된다 (recording fake).

### 3.4 실패 코드

| 코드 | HTTP | 다음 행동 |
| --- | --- | --- |
| `PCV_VM_MANAGE_CONFIRMATION_MISMATCH` | 400 enqueue | 확인 이름을 VM 표시 이름과 똑같이 넣는다 |
| `PCV_VM_NAME_INVALID` | job failed | 표시 이름을 고친다 |
| `PCV_VM_NOT_FOUND` | job failed | 대상 VM을 확인 |
| `PCV_VM_SETTINGS_NOT_FOUND` | job failed, retryable true | 설정 조회 실패 진단 |
| `PCV_VM_NOT_MANAGED_BY_PURECVISOR` | job failed (delete) | manage 후에만 delete |

### 3.5 Catalog / policy

`vm.manage`를 `vm.rename` 근처에 넣는다. Domain `VmLifecycle`, provider
`vm-manage-provider`, handler `VmManage`. RuntimePolicy `NativeMutationOperations`와
Reason, invoker allowlist, Wmi catalog, ProviderSet 배선.

### 3.6 CLI / Web

- CLI: `pcvcli vm manage <vm> --yes`. `--yes` 없으면 delete와 같이
  `PCV_CLI_CONFIRMATION_REQUIRED` usage. body `confirm_name`은 `<vm>` 인자 그대로.
- Web: VM detail `Manage VM` 버튼. confirmation에 표시 이름과 “이후 이 VM은
  PureCVisor managed delete 가드를 통과한다. unmanaged delete 거절은 유지된다.”
  POST `{ confirm_name }`는 confirmation에 보여 준 그 이름. RBAC `operate`.
- coverage `vm.manage` = `POST /api/v1/vms/{vm_id}/manage`.
- `requireRouteAction`에 `manage` 추가.

### 3.7 검증

Code-level:

- catalog 60, QueuedMutation 26, digest, transport `route_count` 60
- native dispatch `vm.manage` / already-managed, Notes append helper (두 번째 marker 없음)
- API 400 mismatch, 202 manage, unmanaged delete still blocked, managed row delete proceeds
- CLI `--yes`, Web confirm/static, `build-served-asset.mjs --write`

설치본 Notes mutation과 package campaign은 이 slice의 required가 아니다.

## 4. 비주장

- operational current `0.42.73-admin-smoke` 유지
- host mutation performed `false`
- OVF/clone/export를 열지 않음
- public trusted signing / external stable publication `not-claimed`

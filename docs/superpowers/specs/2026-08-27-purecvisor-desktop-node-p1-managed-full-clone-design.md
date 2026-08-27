# PureCVisor Desktop Node P1-5 managed full clone 설계

- Design-ID: `purecvisor-desktop-node-p1-managed-full-clone-v1`
- 작성일: `2026-08-27`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: pcv-p1-managed-full-clone-20260827`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-5, §8
- 선행: P0-4 managed import PASS, operational current `0.42.75-admin-smoke`
- 선행 설계: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-managed-import-design.md`
- 구현 계획: `docs/superpowers/plans/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone.md`
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 **managed Gen2 VM의 독립 VHDX 전체 복사 + 새 managed VM** 경로만 연다.
linked clone, export/import, template lock, notes 복사는 열지 않는다. 이 문서 자체는
코드를 추가하지 않고 `0.42.76` package-pair를 열지 않는다.

## 1. 문제

P0-4로 기존 Hyper-V VM에 managed marker를 붙일 수 있다. lab에서 같은 디스크를 두 번째로
쓰려면 운영자가 Hyper-V Manager로 디스크를 복사하고 새 VM을 손으로 만든다. 제품 경로가
없다.

SERVICE_PLAN P1-5는 “새 VHDX 복사 + 새 marker + queued job”이다. 선행인 managed 정의는
04275에서 `pcv.vm.managed-import` pass다. Workstation식 GUI 즉시 클론, 차이 디스크,
OVF는 제품 경계 밖이다.

## 2. 목표와 비목표

### 목표

- `POST /api/v1/vms/{vmId}/clone/preview`가 복사 계획만 돌려 주고 파일을 만들지 않는다.
- `POST /api/v1/vms/{vmId}/clone`가 queued `vm.clone` job을 만든다.
- 소스는 managed, Generation 2, 전원 `Off`, checkpoint 0, 독립 VHDX만 허용한다.
- 대상은 `VmRoot/<target-name>/` 아래 새 디렉터리와 `diskN.vhdx` 복사본이다.
- 새 VM Notes는 `managed-by=purecvisor-desktop-node`만 넣는다. 소스 Notes를 복사하지 않는다.
- 새 NIC MAC을 만들고, 소스와 같은 Hyper-V switch에 연결한다. switch가 없으면 create와
  같이 `Default Switch`다.
- Web와 `pcvcli vm clone`이 같은 route를 쓴다. canonical operator id는
  `GET /api/v1/vms/{id}`가 받는 표시 이름이다.
- 실패 시 새로 만든 대상 디렉터리·VHDX·DefineSystem VM만 지운다. 소스 VM은 변경하지 않는다.

### 비목표

- linked clone, AVHDX, 차이 디스크 트리 (`docs/SERVICE_PLAN.md` §7.2)
- export/import, OVF, vTPM 키 복사 (P2)
- template lock (P1-7), inventory notes/시각 (P1-6)
- 소스 Running/Paused/Saved에서 디스크 복사
- 소스 checkpoint 트리 복사 또는 flatten
- Gen1, unmanaged 소스 클론
- ISO 파일 복사. DVD는 소스 ISO 경로가 호스트에 있으면 재연결하고, 없으면 빈 DVD다
- `0.42.76` package, fullgate, manual-admin pair, `current-evidence.json` write
- 이 문서가 실제 Hyper-V clone mutation을 실행하는 일
- TUI, public trusted signing, 외부 publication

## 3. 선택한 접근

세 후보:

1. **독립 VHDX `FileStream` 복사 후 create와 같은 `DefineSystem`** — 권장. create
   rollback 패턴을 재사용하고 linked clone을 들이지 않는다.
2. Hyper-V Export/Import — P2이며 vTPM/저장소 레이아웃이 제품 데이터 루트 밖으로 샌다.
3. differencing disk — SERVICE_PLAN이 지금 열지 않는다고 닫았다.

이 slice는 1만 구현한다. preview는 QoS preview처럼 **즉시 NativeProductOperation**이다.
복사 자체는 create처럼 **queued Native mutation**이다.

## 4. 계약

### 4.1 Route

현재 catalog `60`, NativeQueuedMutation `26`. 이 slice는 preview 1 + clone 1을 더해
catalog `62`, NativeQueuedMutation `27`이다. `http-transport-contract-v1.json`
`route_count`도 `62`다.

| 항목 | preview | clone |
| --- | --- | --- |
| Method/path | `POST /api/v1/vms/{vmId}/clone/preview` | `POST /api/v1/vms/{vmId}/clone` |
| OperationName | `PreviewCloneVm` | `QueueCloneVm` |
| Job operation | 없음 (즉시) | `vm.clone` |
| Feature ID | `pcv.vm.clone` | `pcv.vm.clone` |
| Family | `hyperv-vm` | `hyperv-vm` |
| Stance | `NativeProductOperation` | `QueuedMutation` |
| Permission | `operate` | `operate` |

Body (두 route 동일):

```json
{
  "confirm_name": "lab-vm",
  "name": "lab-vm-2"
}
```

`confirm_name`은 디코드된 `{vmId}`와 **Ordinal**로 같아야 한다. 공백/대소문자/다른 표시
이름은 `400 PCV_VM_CLONE_CONFIRMATION_MISMATCH`이며 preview도 enqueue도 하지 않는다.
`name`이 없거나 공백이면 `400 PCV_VM_CLONE_NAME_REQUIRED`. `name`이 소스 표시 이름과
Ordinal로 같으면 `400 PCV_VM_CLONE_NAME_CONFLICT`.

Enqueue `202` params:

```json
{
  "source": "<decoded vmId>",
  "name": "<target display name>"
}
```

preview `200` body:

```json
{
  "source": "lab-vm",
  "name": "lab-vm-2",
  "action": "preview",
  "generation": 2,
  "directory": "<VmRoot>\\lab-vm-2",
  "disk_count": 1,
  "planned_copy_bytes": 42949672960,
  "disks": [
    {
      "source": "<source-disk0.vhdx>",
      "target": "<VmRoot>\\lab-vm-2\\disk0.vhdx"
    }
  ]
}
```

`planned_copy_bytes`는 소스 VHDX **파일 길이의 합**이다. 가상 용량이 아니다.

성공 job result:

```json
{
  "source": "lab-vm",
  "name": "lab-vm-2",
  "action": "clone",
  "directory": "<VmRoot>\\lab-vm-2",
  "disks": ["<VmRoot>\\lab-vm-2\\disk0.vhdx"]
}
```

대상 inventory 행은 `managed_by_purecvisor=true`이고 `GET /api/v1/vms/lab-vm-2`가
표시 이름 `lab-vm-2`로 200을 준다.

### 4.2 Native

```csharp
public interface IDesktopNodeHyperVVmCloneProvider
{
    DesktopNodeHyperVVmClonePlan Preview(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);

    DesktopNodeHyperVVmCloneInfo Invoke(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVVmCloneRequest(
    string SourceName,
    string TargetName,
    string VmRoot);

public sealed record DesktopNodeHyperVVmClonePlan(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("generation")] int Generation,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("disk_count")] int DiskCount,
    [property: JsonPropertyName("planned_copy_bytes")] long PlannedCopyBytes,
    [property: JsonPropertyName("disks")] IReadOnlyList<DesktopNodeHyperVVmCloneDiskPlan> Disks);

public sealed record DesktopNodeHyperVVmCloneInfo(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("disks")] IReadOnlyList<string> Disks);
```

`DesktopNodeHyperVWmiVmCloneProvider` 순서:

1. `FindVm(source)`. 없으면 `PCV_VM_NOT_FOUND`.
2. `IsManagedVm`이 false면 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`. WMI write 0.
3. Generation이 2가 아니면 `PCV_VM_GENERATION_UNSUPPORTED`.
4. 전원 상태가 `Off`가 아니면 `PCV_VM_CLONE_SOURCE_NOT_OFF`. 다음 행동: 소스에
   `vm.poweroff` 또는 guest shutdown 후 Off를 확인한다. Saved/Paused/Running은 복사하지
   않는다.
5. checkpoint list가 1개 이상이면 `PCV_VM_CLONE_CHECKPOINTS_PRESENT`. flatten하지 않는다.
6. 각 디스크 HostResource가 `.vhdx`이고 parent/differencing/`.avhdx`가 아니면 통과.
   아니면 `PCV_VM_CLONE_DISK_NOT_INDEPENDENT`.
7. TPM, key protector, shielded 설정이 있으면
   `PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED`. 키 재료를 복사하지 않는다.
8. `FindVm(target)`가 있으면 `PCV_VM_ALREADY_EXISTS`.
9. preview는 여기까지 검사하고 계획 JSON만 반환한다. 디렉터리/VHDX/DefineSystem을
   만들지 않는다.
10. clone job: `Directory.CreateDirectory(VmRoot/target)`. 각 디스크를
    `disk0.vhdx`, `disk1.vhdx`, … 로 **토큰을 보는 스트림 복사**한다. `File.Copy` 한 방에
    맡기지 않는다. cancel이면 대상 디렉터리를 지우고 실패한다.
11. create와 같은 `DefineSystem`으로 Gen2 VM을 만들고, 복사한 VHDX를 SCSI에 붙인다.
    메모리/vCPU는 소스 realized settings를 복사한다. Notes는
    `DesktopNodeHyperVManagedNotes.Marker`만 넣는다.
12. NIC는 새 MAC, 소스 Ethernet connection의 switch. switch 조회 실패 시
    `Default Switch`.
13. DVD: 소스 ISO 경로 파일이 있으면 그 경로를 재연결한다. 없으면 빈 DVD. ISO 파일을
    대상 디렉터리로 복사하지 않는다.
14. Gen2 firmware는 create와 같은 UEFI template을 쓴다.
15. 10 이후 실패하면 대상 `DestroySystem` (정의된 경우)과 대상 디렉터리 삭제를 시도한다.
    소스 VM·소스 VHDX·소스 Notes는 그대로 둔다.

marker 상수는 create/manage/delete와 같은
`DesktopNodeHyperVManagedNotes.Marker`다.

### 4.3 실패 코드

| 코드 | HTTP | 다음 행동 |
| --- | --- | --- |
| `PCV_VM_CLONE_CONFIRMATION_MISMATCH` | 400 | 확인 이름을 소스 표시 이름과 똑같이 넣는다 |
| `PCV_VM_CLONE_NAME_REQUIRED` | 400 | 대상 표시 이름 `name`을 넣는다 |
| `PCV_VM_CLONE_NAME_CONFLICT` | 400 | 소스와 다른 대상 이름을 넣는다 |
| `PCV_VM_NOT_FOUND` | preview 실패 / job failed | 소스 표시 이름을 확인한다 |
| `PCV_VM_NOT_MANAGED_BY_PURECVISOR` | preview 실패 / job failed | 먼저 `vm.manage`로 marker를 붙인다 |
| `PCV_VM_GENERATION_UNSUPPORTED` | preview 실패 / job failed | Gen2 managed VM만 클론한다 |
| `PCV_VM_CLONE_SOURCE_NOT_OFF` | preview 실패 / job failed | 소스를 Off로 만든 뒤 다시 호출한다 |
| `PCV_VM_CLONE_CHECKPOINTS_PRESENT` | preview 실패 / job failed | checkpoint를 삭제한 뒤 다시 호출한다 |
| `PCV_VM_CLONE_DISK_NOT_INDEPENDENT` | preview 실패 / job failed | 독립 VHDX만 있는 소스를 고른다 |
| `PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED` | preview 실패 / job failed | TPM/shielded VM은 이 경로로 클론하지 않는다 |
| `PCV_VM_ALREADY_EXISTS` | preview 실패 / job failed | 다른 대상 이름을 고른다 |
| `PCV_VM_NAME_INVALID` | job failed | 대상 표시 이름을 고친다 |

helper fallback 없음. 내부 CLI `PCV_*`는 job/summary `error`에 보존한다.

### 4.4 Catalog / policy

`vm.clone.preview`와 `vm.clone`을 `vm.manage` 근처에 둔다. Domain `VmLifecycle`,
provider `vm-clone-provider`, handler `VmClone` / `VmClonePreview`.
RuntimePolicy `NativeMutationOperations`와 Reason, invoker allowlist, Wmi catalog,
ProviderSet 배선.

Feature surface ledger에 `pcv.vm.clone`을 새 feature로 추가한다. evidence ledger 승격은
Lane 3만 한다. 이 설계가 `current-evidence.json`을 쓰지 않는다.

### 4.5 CLI / Web

- CLI: `pcvcli vm clone <source> --name <target> --yes`. `--yes` 없으면
  `PCV_CLI_CONFIRMATION_REQUIRED`. body `confirm_name`은 `<source>` 인자 그대로.
  `--dry-run`은 preview route다. mutation enqueue가 아니다.
- Web: VM detail `Clone VM`. confirmation에 소스 표시 이름, 대상 이름 입력, “독립
  VHDX를 복사한 새 managed VM을 만든다. 소스 VM은 변경하지 않는다.” Preview로
  `planned_copy_bytes`를 보여 준 뒤 clone을 POST한다. RBAC `operate`.
- coverage `vm.clone.preview` = `POST /api/v1/vms/{vm_id}/clone/preview`,
  `vm.clone` = `POST /api/v1/vms/{vm_id}/clone`.
- `requireRouteAction`에 `clone`을 추가한다.

### 4.6 식별자

canonical operator id는 표시 이름이다. 소스 `{vmId}`, 대상 `name`, 이후
`vm get`/`vm delete`는 그 표시 이름이다. inventory `Id`를 Hyper-V GUID로 바꾸지 않는다.

### 4.7 검증

Code-level (Lane 1, 이 설계 승인 후 구현 계획):

- catalog 62, QueuedMutation 27, transport `route_count` 62
- preview: unmanaged / not-Off / checkpoint / differencing / TPM / name conflict를
  파일 write 0으로 거절
- clone: 독립 VHDX 복사, 새 marker, 소스 불변, 실패 rollback (대상만 삭제)
- API 400 mismatch/name, 200 preview, 202 clone
- CLI `--yes` / `--dry-run`, Web confirm/static, `build-served-asset.mjs --write`

설치본 clone mutation은 Lane 2다. 한 family, 한 artifact root. Full P0 자동 연쇄 없음.
`0.42.76` package/fullgate/manual-admin/current는 Lane 2 PASS 뒤 Lane 3만 연다.

## 5. 차선

| 차선 | 이 slice |
| --- | --- |
| Lane 0 | `ledger_current=0.42.75-admin-smoke`, 작업 권위 `source_head` |
| Lane 1 | 이 설계와 이후 RED/GREEN. host mutation 없음 |
| Lane 2 | 승인된 설치본 04275에서 clone family 한 프로브 |
| Lane 3 | clone actual-VM PASS 뒤에만 `0.42.75 -> 0.42.76` pair 검토 |

P1-6 notes, P1-7 template lock, GUID inventory, dual-hash campaign-tooling은 이 설계의
체크박스가 아니다.

## 6. 비주장

- operational current는 `0.42.75-admin-smoke`로 유지한다
- host mutation performed `false`
- linked clone / export / TPM clone을 열지 않는다
- public trusted signing / external stable publication `not-claimed`

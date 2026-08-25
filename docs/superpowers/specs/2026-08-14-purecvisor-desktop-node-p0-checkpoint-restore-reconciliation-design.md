# PureCVisor Desktop Node P0-2 checkpoint restore reconciliation 설계

- Design-ID: `purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-v1`
- 작성일: `2026-08-14`
- 문서 상태: `approved`
- 승인 locator: `User-Approval: service-plan-p0-restore-reconcile-20260814`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P0-2, §8
- 구현 계획: `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` Slice B
- 선행 create 계약: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-checkpoint-create-reconciliation.md`
- 운영 앵커: `0.42.73-admin-smoke`
- 선행 payload: `feat/service-plan-p0-attach` P0-1 (`POST /api/v1/vms/{vmId}/attach`, catalog `57`)
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

이 문서는 끊긴 `checkpoint.restore` job을 read-only `checkpoint.list`로만 판정한다. 새
HTTP route를 만들지 않고, list에 checkpoint가 있다고 자동 성공하지 않는다. create/delete/
rename reconcile family를 다시 열지 않는다.

## 1. 문제

Wave 2C는 `checkpoint.create`만 reconcile한다. 근거는 “이름이 list에 있다”가 생성
postcondition이기 때문이다. restore는 다르다. 대상 checkpoint는 restore 전에도 list에
있어야 하므로, presence만으로는 Apply가 끝났다고 말할 수 없다.

현재 `POST /api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore`는 queued `202`와
params `{ checkpoint_name, vm_name }`만 남긴다. durable baseline이 없다. 운영자가
`PCV_JOB_INTERRUPTED`를 보면 Web에 Reconcile 버튼이 없고, `job reconcile`은
`job-not-reconcilable`이다.

Hyper-V는 `Msvm_MostCurrentSnapshotInBranch`로 현재 스냅샷을 가리킨다. 이 값이 요청한
이름과 같고 단일 identity일 때만 restore postcondition이다.

## 2. 목표와 비목표

### 목표

- `checkpoint.list` JSON에 additive `is_current` (`true` / `false` / `null`)를 넣는다.
- restore enqueue가 read-only `checkpoint.list` baseline을
  `pcv-checkpoint-restore-reconciliation/v1`로 job params에 저장한다.
- 기존 `POST /api/v1/jobs/{jobId}/reconcile`가 끊긴 restore job을 받는다.
- `succeeded`는 요청 이름이 정확히 한 row이고 `is_current=true`일 때만.
- Web는 interrupted restore row에 `Reconcile restore`를 보여 준다. CLI는 기존
  `job reconcile` route를 유지한다.

### 비목표

- 새 reconcile HTTP route, 새 job status
- reconcile 경로에서 `checkpoint.restore` 재호출
- list에 이름이 있으면 성공으로 간주
- `instance_id`를 public JSON에 노출
- checkpoint.delete reconcile, periodic checkpoint, export/import
- `0.42.74` 또는 package-pair 개방
- 실제 Hyper-V restore mutation, 설치본 smoke
- TUI, public trusted signing, 외부 publication

## 3. 계약

### 3.1 Route

| 항목 | 값 |
| --- | --- |
| Restore enqueue | 기존 `POST /api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore` |
| Job operation | `checkpoint.restore` |
| Reconcile | 기존 `POST /api/v1/jobs/{jobId}/reconcile` / `ReconcileJob` |
| Reconcile permission | `operate` |
| Reconcile stance | `ProductOperation` |
| Catalog count | `57` 유지 (P0-1 이후, 새 route 없음) |

Enqueue는 계속 `202`다. baseline capture 실패는 enqueue를 막지 않고
`capture_status=unavailable`만 기록한다. create family와 같다.

### 3.2 `checkpoint.list` additive field

`DesktopNodeHyperVCheckpointInfo`:

```csharp
public sealed record DesktopNodeHyperVCheckpointInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("created_at")] string? CreatedAt,
    [property: JsonPropertyName("is_current")] bool? IsCurrent = null);
```

`is_current`는 항상 JSON 필드로 직렬화한다. `null`을 omit하지 않는다. restore
reconcile이 “현재를 모른다”와 “현재가 아니다”를 구분해야 하기 때문이다.

WMI:

- 기존 `Msvm_SnapshotOfVirtualSystem` list는 유지한다.
- 현재 스냅샷은 `vm.GetRelated("Msvm_VirtualSystemSettingData",
  "Msvm_MostCurrentSnapshotInBranch", ...)`다.
- 비교 키는 public JSON이 아니라 provider 내부 `InstanceID`다. `ElementName`만으로
  맞추면 동명이 있을 때 잘못된 row를 current로 표시한다.
- `MarkCurrent(rows, currentInstanceId)` helper가 한 row만 `true`, 나머지는 `false`로
  표시한다. public JSON에는 `instance_id`를 넣지 않는다.

읽기 실패 규칙:

| WMI/매핑 결과 | list HTTP | 각 row `is_current` |
| --- | --- | --- |
| current InstanceID 1개, list의 정확히 한 row와 일치 | 200, list 유지 | 그 row `true`, 나머지 `false` |
| current association 없음 / 조회 예외 / 0 또는 2+ 매칭 | 200, list 유지 | 전부 `null` |
| 기존 name/vm_name parity 실패 | 기존 `PCV_NATIVE_CHECKPOINT_LIST_PARITY_INCOMPLETE` | list를 숨기지 않는 현재 실패 유지 |

`is_current` 조회 실패는 list 전체를 실패로 바꾸지 않는다. create reconcile과 list
소비자는 name/vm_name만으로도 동작한다.

정확히 한 row만 `true`일 수 있다. 두 row가 `true`이면 구현 버그다. 그 경우
enqueue/reconcile은 `unavailable` / `409`로 fail-closed한다.

### 3.3 Durable enqueue baseline

`QueueRestoreVmCheckpoint`는 `BuildCheckpointRestoreParameters(vmName, checkpointName,
token)`를 호출한다. 기존 `{ checkpoint_name, vm_name }`에 `reconciliation`을 더한다.

```json
{
  "checkpoint_name": "requested",
  "vm_name": "lab-vm",
  "reconciliation": {
    "schema": "pcv-checkpoint-restore-reconciliation/v1",
    "capture_status": "captured",
    "before": { "current_name": "old", "vm_name": "lab-vm" },
    "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
  }
}
```

`capture_status=captured`가 되려면 다음이 **모두** 참이어야 한다.

1. scoped `checkpoint.list`가 성공한다.
2. 요청 이름+VM row가 정확히 1개다. (restore 대상이 enqueue 시점에 존재)
3. `is_current=true`인 row가 정확히 1개다. (현재 스냅샷 identity를 안다)
4. 그 current 이름이 요청 이름과 다르다. (아직 apply 전)

그 외는 `capture_status=unavailable`이다. `202`는 유지한다.

| enqueue 관측 | `capture_error_code` |
| --- | --- |
| `checkpoint.list` 실패 | provider code 또는 `PCV_CHECKPOINT_LIST_FAILED` |
| 요청 이름 0개 | `PCV_CHECKPOINT_NOT_FOUND` |
| 요청 이름 2개 이상 | `PCV_CHECKPOINT_IDENTITY_AMBIGUOUS` |
| current를 읽을 수 없음 (`is_current` 전부 `null` 또는 true가 0/2+) | `PCV_CHECKPOINT_CURRENT_UNAVAILABLE` |
| 요청 이름이 이미 current | `PCV_CHECKPOINT_ALREADY_CURRENT` |

이미 current인 restore를 `captured`로 남기면, 아무 mutation도 없이 끊긴 job이
reconcile에서 `succeeded`가 된다. 이는 “list에 있다 = 성공”과 같은 거짓 양성이다.

`before.current_name`은 current row의 `name`이다. fingerprint/id는 이 slice에서 만들지
않는다. 동명 current는 위 모호 규칙으로 `unavailable`이다.

### 3.4 Reconcile 분류

`HandleJobReconcile`은 다음이 모두 참일 때만 restore family로 들어간다.

- job status `failed`
- operation `checkpoint.restore`
- error `PCV_JOB_INTERRUPTED`
- metadata schema `pcv-checkpoint-restore-reconciliation/v1`
- `capture_status=captured`

그 외 restore job은 기존과 같이 `409 PCV_JOB_RECONCILIATION_REQUIRED`,
classification `job-not-reconcilable` 또는 `baseline-unavailable`이다. 에러 문구의
허용 family는 `vm.rename, vm.delete, checkpoint.create, or checkpoint.restore`로
늘린다.

reconcile은 provider `checkpoint.list`만 읽는다. `checkpoint.restore`를 호출하지 않는다.

| readback | classification | HTTP / job |
| --- | --- | --- |
| 요청 이름+VM row 정확히 1개이고 `is_current=true` | `postcondition-confirmed` | 200, job `succeeded`, result `action=reconciled` |
| 요청 이름 1개인데 `is_current=false` | `not-applied` | 409, job `failed` 유지 |
| 요청 이름 1개인데 `is_current=null` | `current-unavailable` | 409, job `failed` 유지 |
| 요청 이름 0개 | `not-applied` | 409, job `failed` 유지 |
| 요청 이름 2개 이상 | `ambiguous-duplicate-checkpoint-names` | 409, job `failed` 유지 |
| list 실패 | `readback-unavailable` | 409, job `failed` 유지 |
| captured baseline 없음 | `baseline-unavailable` | 409, job `failed` 유지 |

`is_current=false`인 presence는 성공이 아니다. 스냅샷은 원래 있었고, Apply가 끝났다고
말할 수 없다.

확인 결과 observation은 기존 `job-reconciled` / `job-reconciliation-required`다. 새
public job status를 만들지 않는다. `retryable`은 계속 `false`다.

성공 result:

```json
{
  "action": "reconciled",
  "operation": "checkpoint.restore",
  "reconciliation": {
    "schema": "pcv-checkpoint-restore-reconciliation/v1",
    "classification": "postcondition-confirmed",
    "before": { "current_name": "old", "vm_name": "lab-vm" },
    "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true },
    "observed": { "name": "requested", "vm_name": "lab-vm", "is_current": true }
  }
}
```

### 3.5 Native / handler 소유

- `DesktopNodeHyperVWmiCheckpointProvider.GetCheckpoints`가 current association을 읽고
  `MarkCurrent`를 적용한다.
- `HasCompleteCheckpointListParity`는 계속 name + vm_name만 본다. `is_current` 부재는
  parity 실패가 아니다.
- `DesktopNodeApiJobReconciliationHandler`가 `BuildCheckpointRestoreParameters`와
  restore reconcile 분기를 소유한다. create capture와 스키마 상수를 섞지 않는다.
- `DesktopNodeApiVmMutationRouteHandler`의 `QueueRestoreVmCheckpoint`만 새 builder를
  호출한다. restore worker / WMI Apply 경로는 바꾸지 않는다.

### 3.6 CLI / Web

- CLI: `pcvcli job reconcile <job_id>` 기존 route. 새 subcommand 없음. interactive help는
  `Reconcile an interrupted rename, delete, checkpoint create, or restore`.
- Web: `canReconcileVmMutation` allowlist에 `checkpoint.restore`를 추가한다. 버튼 라벨은
  `Reconcile restore`. RBAC `operate`. create 버튼 `Reconcile checkpoint`는 유지한다.
- Wave 2C fixture
  `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json`는
  2026-08-03 시점 기록으로 유지한다. `excluded_operations`의 `checkpoint.restore`를
  지우지 않는다. P0-2는 새 fixture
  `packaging/windows-desktop-node/tests/fixtures/service-plan-p0-checkpoint-restore-reconciliation.json`와
  짝 Pester를 추가한다.

### 3.7 검증

Code-level:

- `MarkCurrent` 단위 테스트: 한 row만 true, 모르는 current는 전부 null
- restore enqueue `capture_status=captured` / already-current·missing·ambiguous·list-fail은
  `unavailable`
- reconcile: current true → 200 `succeeded`; present but not current → 409 `not-applied`;
  duplicate names → 409; list fail → 409; 호출 횟수에 `checkpoint.restore` 없음
- job-not-reconcilable 문구에 restore 포함
- Web static/parity에 `Reconcile restore`
- 새 Pester fixture. Wave 2C Pester는 그대로 PASS
- catalog count `57`, snapshot digest 불변

설치본 restore smoke와 package campaign은 이 slice의 required 조건이 아니다.

## 4. 기존 family와의 차이

| | create (Wave 2C) | restore (P0-2) |
| --- | --- | --- |
| 성공 증거 | 요청 이름 row 정확히 1개 | 요청 이름 row 정확히 1개 **그리고** `is_current=true` |
| enqueue captured 조건 | 요청 이름 부재 | 요청 이름 1개, current 1개, current ≠ 요청 |
| presence only | 생성에는 충분 | **금지**. 409 `not-applied` |
| schema | `pcv-checkpoint-create-reconciliation/v1` | `pcv-checkpoint-restore-reconciliation/v1` |
| Web | `Reconcile checkpoint` | `Reconcile restore` |

create 구현을 restore에 복사한 뒤 이름만 바꾸면 이 설계를 위반한다.

## 5. 비주장

- operational current는 `0.42.73-admin-smoke` 유지
- host mutation performed `false`
- Hyper-V restore가 실제로 Apply됐는지는 설치본/actual-VM evidence가 소유
- public trusted signing / external stable publication `not-claimed`
- Hyper-V exactly-once, mixed-version writer 비주장 (ADR-0013)

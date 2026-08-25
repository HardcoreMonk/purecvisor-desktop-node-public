# Wave 2C `vm.delete` reconciliation specification

작성일: 2026-08-03
상태: `code_complete` (code-level only)
변경 등급: `L / Release`
운영 기준선: `0.42.65-admin-smoke`

## 목적과 범위

Wave 2B 결정표의 `vm.delete` 구현 게이트인 managed ownership, persisted before-state와
absent-versus-unknown terminal mapping을 닫는다. 기존 queued delete의 `202` 응답과 단일
mutation worker를 유지하면서, provider 호출이 terminal persistence 경계에서 끊긴 경우
operator가 읽기 전용 inventory로 삭제 후 상태를 판정할 수 있게 한다.

이 slice는 ASP.NET Core transport 전환, 새 job status, actual VM/Hyper-V host mutation,
설치본 package candidate 또는 promotion을 포함하지 않는다.

## Durable enqueue baseline

`DELETE /api/v1/vms/{vmId}`는 기존 `name` parameter를 유지하고, 추가로 다음 metadata를
job params에 저장한다.

```json
{
  "reconciliation": {
    "schema": "pcv-vm-delete-reconciliation/v1",
    "capture_status": "captured|unavailable",
    "before": "managed vm.list row or null",
    "before_fingerprint": "platform/guest/state/cpu/memory/generation/ownership",
    "expected_after": { "name": "vm_name", "state": "absent" }
  }
}
```

baseline capture는 `vm.list` 한 번을 호출하는 read-only preflight다. 정확히 하나의 row,
`managed_by_purecvisor=true` marker와 비어 있지 않은 stable VM `id`가 모두 있어야
`capture_status=captured`가 된다. provider readback 실패, 이름 없음/중복, unmanaged row 또는
identity 누락이면 enqueue 자체는 기존처럼 `202`로 유지하되 `capture_status=unavailable`을
기록한다.

## Additive reconcile API

기존 `POST /api/v1/jobs/{jobId}/reconcile`를 `vm.delete`에도 적용한다. route registry는
여전히 유일한 method/path source이며 route count는 55로 유지한다. permission은 `operate`,
mutation stance는 `ProductOperation`이다.

reconcile은 다음 조건에서만 provider `vm.list`를 읽는다.

- job status가 `failed`
- operation이 `vm.delete`
- error code가 `PCV_JOB_INTERRUPTED`
- durable baseline이 `captured`

provider mutation (`vm.delete`)은 이 endpoint에서 절대 호출하지 않는다. 기존
`vm.delete-status`는 job row의 latest status를 제공하므로, reconciliation은 별도 destructive
retry를 만들지 않고 현재 job evidence와 inventory readback을 함께 보존한다.

### 분류와 terminal mapping

| readback 판정 | 분류 | 응답/상태 | 의미 |
|---|---|---|---|
| 대상 이름 row 0개, captured managed before-state 존재 | `postcondition-confirmed` | HTTP 200, 기존 job `succeeded` | result action `reconciled`; duplicate delete 없음 |
| 같은 stable VM ID의 managed row가 남아 있음 | `not-applied` | HTTP 409, job `failed` 유지 | 삭제 side effect가 관측되지 않음; 기존 operator 절차로 결정 |
| 다른 ID의 managed row가 같은 이름으로 재생성됨 | `target-recreated-or-identity-changed` | HTTP 409, job `failed` 유지 | absence만으로 원래 VM 삭제를 주장하지 않음 |
| unmanaged row, 중복 이름 또는 readback 불명확 | collision/ambiguous classifications | HTTP 409, job `failed` 유지 | destructive retry 금지 |
| baseline 없음 또는 vm.list 실패 | `baseline-unavailable`/`readback-unavailable` | HTTP 409, job `failed` 유지 | `PCV_JOB_RECONCILIATION_REQUIRED`, manual action |

새 public job status는 도입하지 않는다. confirmed 결과는 기존 `succeeded`를 사용하고,
미확정 결과는 기존 `failed`를 유지한다. runtime은 `job-reconciled` 또는
`job-reconciliation-required` observation을 기록한다.

## Operator surface parity

- Web Console: `PCV_JOB_INTERRUPTED`인 failed `vm.delete` row에 RBAC `operate`로
  `Reconcile delete` 버튼을 표시한다.
- PCVCLI: `job reconcile <job_id>`가 동일한 POST route를 사용한다. API 409는 기존
  structured error transport로 non-zero exit와 recommended action을 표시한다.
- Retry는 `PCV_JOB_INTERRUPTED`의 기존 `retryable=false` 경계를 유지한다.

## 검증 및 경계

다음은 code-level 검증이다.

- Runtime: confirmed commit과 required/no-mutation observation tests
- API: managed baseline capture, absent confirmed postcondition, target-present required 409,
  route contract
- Web: TypeScript, served asset, static parity/browser fixture
- CLI: command catalog route parity (carry-forward)
- Pester: machine-readable Wave 2C fixture

실제 Hyper-V/VM mutation, installed smoke, package build, full-admin host mutation과 public
trusted signing은 실행하지 않는다. 결과는 `0.42.65-admin-smoke` carry-forward,
`package_candidate_created=false`, `promotion_not_triggered=true`로만 해석한다.

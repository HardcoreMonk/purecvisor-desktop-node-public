# Wave 2C `checkpoint.create` reconciliation specification

작성일: 2026-08-03
상태: `code_complete` (code-level only)
변경 등급: `L / Release`
운영 기준선: `0.42.65-admin-smoke`

## 목적과 범위

Wave 2B checkpoint family의 create conditional-postcondition gate를 닫는다. 기존 queued
checkpoint create의 `202` 응답과 단일 mutation worker를 유지하면서, provider 호출이
terminal persistence 경계에서 끊긴 경우 read-only `checkpoint.list`로 생성 후 상태를
판정할 수 있게 한다.

`checkpoint.restore`는 checkpoint presence만으로 VM data/state 복원을 증명할 수 없어 이
slice에서 계속 제외한다. ASP.NET Core transport 전환, 새 job status, actual VM/Hyper-V host
mutation, 설치본 package candidate 또는 promotion도 포함하지 않는다.

## Durable enqueue baseline

`POST /api/v1/vms/{vmId}/checkpoints`는 기존 `checkpoint_name`/`vm_name` parameters를
유지하고, 다음 metadata를 job params에 저장한다.

```json
{
  "reconciliation": {
    "schema": "pcv-checkpoint-create-reconciliation/v1",
    "capture_status": "captured|unavailable",
    "before": null,
    "expected_before": { "state": "absent", "name": "checkpoint", "vm_name": "vm" },
    "expected_after": { "state": "present", "name": "checkpoint", "vm_name": "vm" }
  }
}
```

baseline capture는 scoped VM에 대해 `checkpoint.list` 한 번을 호출하는 read-only preflight다.
요청한 checkpoint name이 정확히 부재할 때만 `capture_status=captured`가 된다. 이미 같은
이름이 있거나 duplicate row가 있으면 기존 `202` enqueue semantics는 유지하되
`capture_status=unavailable`을 기록한다. provider readback 실패도 동일하게 unavailable로
기록하며 explicit reconcile에서 fail-closed한다.

## Additive reconcile API

기존 `POST /api/v1/jobs/{jobId}/reconcile`를 `checkpoint.create`에도 적용한다. route
registry는 유일한 method/path source이며 route count는 55로 유지한다. permission은 `operate`,
mutation stance는 `ProductOperation`이다.

reconcile은 다음 조건에서만 provider `checkpoint.list`를 읽는다.

- job status가 `failed`
- operation이 `checkpoint.create`
- error code가 `PCV_JOB_INTERRUPTED`
- durable baseline이 `captured`이고 expected-before가 absent

provider mutation (`checkpoint.create`)은 이 endpoint에서 절대 호출하지 않는다.

### 분류와 terminal mapping

| readback 판정 | 분류 | 응답/상태 | 의미 |
|---|---|---|---|
| 동일 VM/이름 row가 정확히 1개 | `postcondition-confirmed` | HTTP 200, 기존 job `succeeded` | result action `reconciled`; duplicate create 없음 |
| 동일 VM/이름 row가 0개 | `not-applied` | HTTP 409, job `failed` 유지 | 생성 side effect가 관측되지 않음; 기존 operator 절차로 결정 |
| 동일 VM/이름 row가 복수 | `ambiguous-duplicate-checkpoint-names` | HTTP 409, job `failed` 유지 | checkpoint identity가 불명확하므로 재생성 금지 |
| baseline 없음 또는 checkpoint.list 실패 | `baseline-unavailable`/`readback-unavailable` | HTTP 409, job `failed` 유지 | `PCV_JOB_RECONCILIATION_REQUIRED`, manual action |

새 public job status는 도입하지 않는다. confirmed 결과는 기존 `succeeded`를 사용하고,
미확정 결과는 기존 `failed`를 유지한다. runtime은 `job-reconciled` 또는
`job-reconciliation-required` observation을 기록한다.

## Operator surface parity

- Web Console: `PCV_JOB_INTERRUPTED`인 failed `checkpoint.create` row에 RBAC `operate`로
  `Reconcile checkpoint` 버튼을 표시한다.
- PCVCLI: `job reconcile <job_id>`가 동일한 POST route를 사용한다. API 409는 기존
  structured error transport로 non-zero exit와 recommended action을 표시한다.
- Retry는 `PCV_JOB_INTERRUPTED`의 기존 `retryable=false` 경계를 유지한다.

## 검증 및 경계

다음은 code-level 검증이다.

- Runtime: confirmed commit과 required/no-mutation observation tests
- API: absent baseline capture, confirmed postcondition, not-applied/ambiguous 409, route contract
- Web: TypeScript, served asset, static parity/browser fixture
- CLI: command catalog route parity (carry-forward)
- Pester: machine-readable Wave 2C fixture

실제 Hyper-V/VM checkpoint mutation, restore, installed smoke, package build, full-admin host
mutation과 public trusted signing은 실행하지 않는다. 결과는 `0.42.65-admin-smoke`
carry-forward, `package_candidate_created=false`, `promotion_not_triggered=true`로만 해석한다.

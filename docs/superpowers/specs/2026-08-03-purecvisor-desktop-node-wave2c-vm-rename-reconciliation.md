# Wave 2C `vm.rename` reconciliation specification

작성일: 2026-08-03
상태: `code_complete` (code-level only)
변경 등급: `L / Release`
운영 기준선: `0.42.65-admin-smoke`

## 목적과 범위

Wave 2B 결정표에서 구현 승인된 단일 operation `vm.rename`의 interrupted-job 경계를
닫는다. 기존 queued rename의 `202` 응답과 단일 mutation worker를 유지하면서, provider
호출이 job terminal persistence 경계에서 끊긴 경우 operator가 읽기 전용 readback으로
postcondition을 확인할 수 있게 한다.

이 slice는 ASP.NET Core transport 전환, 새 job status, actual VM/Hyper-V host mutation,
설치본 package candidate 또는 promotion을 포함하지 않는다.

## Durable enqueue baseline

`POST /api/v1/vms/{vmId}/rename`는 기존 `name`/`new_name` parameters를 유지하고, 추가로
다음 metadata를 job params에 저장한다.

```json
{
  "reconciliation": {
    "schema": "pcv-vm-rename-reconciliation/v1",
    "capture_status": "captured|unavailable",
    "before": "captured vm.list row or null",
    "before_fingerprint": "platform/guest/state/cpu/memory/generation/ownership",
    "expected_after": { "name": "new_name" }
  }
}
```

baseline capture는 `vm.list` 한 번을 호출하는 read-only preflight다. provider readback이
실패하거나 old name이 0개/복수이면 enqueue 자체는 기존처럼 `202`로 유지하되
`capture_status=unavailable`을 기록한다. 이 경우 explicit reconcile은 자동 판단하지 않고
`409 PCV_JOB_RECONCILIATION_REQUIRED`로 fail-closed한다.

## Additive reconcile API

`POST /api/v1/jobs/{jobId}/reconcile`를 `jobs` route family의 `ReconcileJob`으로 추가한다.
permission은 `operate`, mutation stance는 `ProductOperation`이다. API route registry가
유일한 method/path source이며 route count는 54에서 55로 의도적으로 증가한다.

reconcile은 다음 조건에서만 provider `vm.list`를 읽는다.

- job status가 `failed`
- operation이 `vm.rename`
- error code가 `PCV_JOB_INTERRUPTED`
- durable baseline이 `captured`

provider mutation (`vm.rename`)은 이 endpoint에서 절대 호출하지 않는다.

### 분류와 terminal mapping

| readback 판정 | 분류 | 응답/상태 | 의미 |
|---|---|---|---|
| new row 1개, old row 0개, fingerprint 일치 | `postcondition-confirmed` | HTTP 200, 기존 job `succeeded` | result action `reconciled`; duplicate mutation 없음 |
| old row 1개, new row 0개, fingerprint 일치 | `not-applied` | HTTP 409, job `failed` 유지 | operator가 side effect 부재를 확인한 뒤 기존 절차로 결정 |
| old/new 동시 존재, target fingerprint 불일치, target 미관측 | ambiguous classifications | HTTP 409, job `failed` 유지 | 상태가 불명확하므로 자동 retry 금지 |
| baseline 없음 또는 vm.list 실패 | `baseline-unavailable`/`readback-unavailable` | HTTP 409, job `failed` 유지 | `PCV_JOB_RECONCILIATION_REQUIRED`, manual action |

새 public job status는 도입하지 않는다. confirmed 결과는 기존 `succeeded`를 사용하고,
미확정 결과는 기존 `failed`를 유지한다. runtime은 `job-reconciled` 또는
`job-reconciliation-required` observation을 기록하므로 Event Log sink, diagnostics,
ops-summary의 recent events에서 operator action이 추적된다.

## Operator surface parity

- Web Console: `PCV_JOB_INTERRUPTED`인 failed `vm.rename` row에 RBAC `operate`로
  `Reconcile rename` 버튼을 표시한다.
- PCVCLI: `job reconcile <job_id>`가 동일한 POST route를 사용한다. API 409는 기존
  structured error transport로 non-zero exit와 recommended action을 표시한다.
- Retry는 `PCV_JOB_INTERRUPTED`의 기존 `retryable=false` 경계를 유지한다.

## 검증 및 경계

다음은 code-level 검증이다.

- Runtime: confirmed commit과 required/no-mutation observation tests
- API: baseline capture, confirmed postcondition, not-applied 409, route contract
- Web: TypeScript, served asset, static parity/browser fixture
- CLI: command catalog route parity
- Pester: machine-readable Wave 2C fixture

실제 Hyper-V/VM mutation, installed smoke, package build, full-admin host mutation과 public
trusted signing은 실행하지 않는다. 이 문서의 결과는 `0.42.65-admin-smoke` carry-forward,
`package_candidate_created=false`, `promotion_not_triggered=true`로만 해석한다.

# Desktop Node P1-10 나머지 조건부 reconcile 설계

- Design-ID: `purecvisor-desktop-node-p1-remaining-reconcile-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-slice-4`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P1-10
- 선행: Wave 2C `vm.rename` / `vm.delete` / `checkpoint.create`, P0-2 `checkpoint.restore`
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

`POST /api/v1/jobs/{jobId}/reconcile`는 끊긴 mutation을 **다시 실행하지 않고** provider
readback으로 postcondition만 판정한다. 지금 받는 operation은 네 개다.

`vm.rename`, `vm.delete`, `checkpoint.create`, `checkpoint.restore`

SERVICE_PLAN P1-10은 남은 lab 공백을 **create / shutdown / restart / QoS** family별
slice로 연다. `PCV_JOB_INTERRUPTED` 뒤에 `job retry`로 같은 mutation을 자동 재제출하면
안 된다.

## 현재 계약

- Route는 기존 `job.reconcile` / `ReconcileJob` / permission `operate` / ProductOperation.
- 새 HTTP route 없음. catalog 68 유지.
- Feature는 `pcv.job.lifecycle`에 붙인다. 29번째 ID 없음.
- Enqueue는 `202`를 유지하고, baseline capture 실패는 `capture_status=unavailable`.
- Confirmed postcondition만 기존 job을 `succeeded`로 바꾼다. 모호하면 `409
  PCV_JOB_RECONCILIATION_REQUIRED`, job은 `failed` 유지.
- Runtime allowlist: `DesktopNodeJobRuntime.IsReconciliationSupportedOperation`.
- Handler 미해당 operation 문구: `Only a failed vm.rename, vm.delete, checkpoint.create, or checkpoint.restore job with PCV_JOB_INTERRUPTED can be reconciled.`
- Web `canReconcileVmMutation`과 CLI 문서 목록이 같은 네 operation이다.
- CLI 명령은 계속 `pcvcli job reconcile <job_id>`.

## 결정

- Family마다 enqueue가 read-only baseline을 `reconciliation` metadata로 남긴다.
- Reconcile 경로는 해당 native mutation을 호출하지 않는다. preview/retry도 아니다.
- 판정은 단일 identity일 때만 `postcondition-confirmed`. 0/2+, fingerprint 불일치, 부분 적용은 409.
- `not-applied`도 409다. 운영자가 상태를 본 뒤에 기존 절차로 결정한다. 자동 retry 금지.
- `vm.start` / `poweroff` / pause / Saved / attach / clone / guest-file / limit / disk-resize는 이 설계 밖이다.
- QoS는 storage와 network를 같은 P1-10 family로 두되 slice는 한 쌍으로 닫는다.

## Family 판정

### `vm.create` — schema `pcv-vm-create-reconciliation/v1`

Readback: `vm.list`.

| 분류 | 조건 |
| --- | --- |
| `postcondition-confirmed` | 요청 이름이 정확히 한 row, generation/ownership fingerprint가 expected_after와 같음 |
| `not-applied` | 요청 이름이 0개 |
| `ambiguous-*` | 동명 2+, fingerprint 불일치, unmanaged collision |
| `baseline-unavailable` | enqueue 때 이름이 이미 있었거나 capture 실패 |

Create가 VM은 만들었는데 managed marker 전에 끊기면 성공으로 치지 않는다.

### `vm.shutdown` — schema `pcv-vm-shutdown-reconciliation/v1`

Readback: `vm.list` 한 row (name + fingerprint).

| 분류 | 조건 |
| --- | --- |
| `postcondition-confirmed` | 동일 identity, state `Off` |
| `not-applied` | 동일 identity, state가 baseline과 같고 Running |
| `incomplete-power-state` | Saved/Paused 등 Off도 Running도 아님 |
| `identity-mismatch` | 이름 충돌 또는 fingerprint 불일치 |

Guest integration 실패로 Running이 남으면 `not-applied`다.

### `vm.restart` — schema `pcv-vm-restart-reconciliation/v1`

Readback: `vm.list`의 `state`와 `last_powered_on` (P1-6 `TimeOfLastStateChange`).

| 분류 | 조건 |
| --- | --- |
| `postcondition-confirmed` | 동일 identity, Running, `last_powered_on`이 baseline보다 이후 |
| `not-applied` | Running이고 `last_powered_on`이 baseline과 같음 |
| `incomplete-power-state` | Off (shutdown만 됨) |
| `timestamp-unavailable` | baseline 또는 readback에 `last_powered_on` 없음 |

Running만 보고 성공하지 않는다. 재시작 전에도 Running이다.

### QoS — schema `pcv-vm-qos-storage-reconciliation/v1` / `pcv-vm-qos-network-reconciliation/v1`

Operation: `vm.qos.storage.set`, `vm.qos.network.set`.
Readback: `vm.blkio-get` / `vm.bandwidth`의 대상 disk/adapter.

| 분류 | 조건 |
| --- | --- |
| `postcondition-confirmed` | 대상 장치의 max/min이 expected_after와 같음 |
| `not-applied` | before와 같음 |
| `partial-policy` | max/min 중 하나만 바뀜 |
| `target-missing` | disk/adapter가 없음 |

## Slice

### Slice 1 — `vm.create`

- Enqueue `BuildVmCreateParameters` + runtime allowlist + handler branch.
- Web `Reconcile create`. CLI 문서 operation 목록에 `vm.create`.
- shutdown/restart/QoS는 이 slice가 아니다.

### Slice 2 — `vm.shutdown`

- Enqueue baseline + handler. Web `Reconcile shutdown`.

### Slice 3 — `vm.restart`

- `last_powered_on` 비교. Web `Reconcile restart`.

### Slice 4 — QoS storage/network

- 두 queued set operation. Web `Reconcile storage QoS` / `Reconcile network QoS`.
- P1-10 닫힘. 다음 SERVICE_PLAN 항목은 P2다.

## 비목표

- 새 reconcile route, 새 job status
- reconcile에서 mutation 재호출, 자동 retry
- start/poweroff/pause/Saved/attach/clone/guest-file/limit/disk-resize
- current-evidence, package-pair, Lane 2 Hyper-V 복구
- 29번째 feature

## 검증

- Family마다 handler 단위 테스트: confirmed / not-applied / ambiguous / baseline-unavailable.
- Web/CLI는 기존 `job.reconcile` parity. catalog 68 유지.
- dirty-tree focused test. 설치본 interrupted-job smoke는 Lane 2이며 이 캠페인이 열지 않는다.

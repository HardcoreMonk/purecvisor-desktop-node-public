# PureCVisor Desktop Node Wave 2B operation reconciliation 결정표

## 결정 상태

- 결정 ID: `wave2b-operation-reconciliation-v1`
- 작성일: 2026-08-03
- 상태: `code_complete` / `promotion_not_triggered`
- 기준 제품: `0.42.65-admin-smoke`
- 구현 변경: 없음
- 제품 retry/recovery 동작 변경: 없음
- host/Hyper-V/actual-VM mutation: `false`
- public trusted signing / external stable publication: `false`

이 문서는 Wave 2A에서 `running` job을 자동 재개하지 않고 `PCV_JOB_INTERRUPTED`·`retryable=false`로
보존하는 현재 의미를 operation별로 해석한다. 결정표를 작성하는 동안 retry, reconciliation,
Hyper-V 상태 또는 새로운 public error/status를 추가하지 않는다. machine-readable 단일 진실은
`packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json`이다.

## 공통 결정

1. persisted `running` job은 현재처럼 failed projection으로 남기고 자동 retry하지 않는다.
2. readback이 postcondition을 보여도, persisted before-state 또는 고유 operation identity가 없으면
   그것을 자동 terminal-success로 승격하지 않는다.
3. readback deadline은 현재 route timeout 계약을 관찰 기준으로만 사용한다. 기본값은 30초,
   설정 범위는 1~3600초이며 Wave 2B가 worker용 새 timeout을 만들지 않는다. deadline 초과는
   `unknown`으로 취급하고 재실행하지 않는다.
4. 운영자는 job correlation/ID, provider readback, Event Log/diagnostics를 함께 확인한 뒤에만
   기존 수동 retry 또는 rollback 절차를 선택한다. 불확실한 external side effect는 반복하지 않는다.

## operation family 결정표

| family | operations | reconciliation 범위 | expected-before → expected-after 핵심 | readback 근거 | Wave 2B 자동 조치 |
|---|---|---|---|---|---|
| VM 생성 | `vm.create` | 조건부 postcondition | 이름/소유 artifact 부재 → managed VM, Gen2, CPU/메모리, VHD/Default Switch 존재 | `vm.list`, `vm.blkio-get`, `vm.bandwidth` | 금지. 부분 VHD/VM 생성 여부를 수동 판정 |
| VM 삭제 | `vm.delete` | before-state가 있을 때 조건부 | managed VM 존재 → inventory 부재 | `vm.list`, `vm.delete-status` | 금지. `action=absent`도 before-state 없는 자동 성공 근거로 사용하지 않음 |
| VM 이름 변경 | `vm.rename` | before-state가 있을 때 조건부 | old 존재/new 부재 → old 부재/new managed 존재 | `vm.list` | 금지. 양쪽 이름 또는 양쪽 부재는 수동 해결 |
| QoS | `vm.qos.storage.set`, `vm.qos.network.set` | 수동만 가능 | target과 기존 policy 확인 → 요청 policy 적용 | `vm.blkio-get`, `vm.bandwidth`는 policy 값을 공개하지 않음; terminal result의 apply evidence만 보조 | 금지. provider-specific policy readback 후 수동 조치 |
| Checkpoint | `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete` | create/delete 조건부, restore 수동 | create/delete 이름 존재성; restore는 VM 상태·데이터까지 일치해야 함 | `checkpoint.list`, `vm.list`, restore provider readback | 금지. restore는 checkpoint 존재만으로 성공 주장 금지 |
| 직접 VM 상태 | `vm.start`, `vm.poweroff`, `vm.pause`, `vm.resume` | before-state가 있을 때 조건부 | managed VM 상태 → running/stopped/paused 목표 상태 | `vm.list` | 금지. 현재 상태 일치만으로 이전 invocation의 완료를 주장하지 않음 |
| 모호한 VM 상태 | `vm.shutdown`, `vm.restart` | 수동만 가능 | guest shutdown/reset 요청 → guest acknowledgement와 전이 완료 | `vm.list` + guest/provider 상태 | 금지. shutdown/reset 재호출 금지 |
| resource/media | `vm.limit`, `vm.set-memory`, `vm.set-vcpu`, `vm.disk-resize`, `vm.eject` | field별 혼합 | resource 현재값 → 요청값; disk/media는 전용 post-state 필요 | `vm.list`, `vm.blkio-get`, provider-specific readback | 금지. 공개 readback에 필드가 없으면 unknown |
| Guest Execution | `vm.guest.exec`, `vm.guest.channel.verify`, `vm.guest.channel.ensure` | Wave 2B 제외 | credential/audit/timeout/RBAC → guest terminal effect | ADR-0009 및 guest audit/provider 계약 | 제외. 별도 설계·승인 전 자동 retry/reconcile 금지 |

표는 현재 22개 mutation operation을 9개 family로 빠짐없이 덮는다. `vm.delete`의 missing VM
`action=absent`와 checkpoint provider의 create/delete post-verify는 현재 adapter의 관찰 결과이지,
persisted-running job에 대한 자동 재실행 승인이 아니다.

## 2C로 넘기는 조건

Wave 2C는 이 표에서 승인된 operation 하나 또는 동일 의미 family만 별도 L/Release slice로 구현한다.
해당 slice는 최소한 다음을 추가로 증명해야 한다.

- persisted before-state 또는 고유 operation identity
- typed provider readback과 expected-before/after correlation
- external side effect가 불명확할 때의 terminal mapping과 수동 action
- 필요한 경우 `PCV_JOB_RECONCILIATION_REQUIRED` additive-contract review, Web/PCVCLI parity
- actual-VM pre/post/cleanup evidence; fake readback을 actual provider 성공으로 주장하지 않음

QoS는 현재 공개 readback이 정책 수치를 반환하지 않으므로 typed policy readback과 rollback evidence가
먼저 필요하다. Guest Execution은 ADR-0009의 credential, audit, redaction, timeout, RBAC와 provider
effect reconciliation을 별도 계획으로 유지한다.

## 비주장

- exactly-once Hyper-V 또는 guest side effect
- 자동 retry/reconcile, 새 job status/error code, schema v3/SQLite
- actual VM/host mutation, package promotion, public signing/publication

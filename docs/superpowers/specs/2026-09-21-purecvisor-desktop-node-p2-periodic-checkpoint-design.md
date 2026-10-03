# Desktop Node P2-12 주기 checkpoint 설계

- Design-ID: `purecvisor-desktop-node-p2-periodic-checkpoint-v1`
- 작성일: `2026-09-21`
- 문서 상태: `implemented-slice-5`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P2-12
- 선행: `pcv.checkpoint.lifecycle` list/create/delete, `pcv.checkpoint.restore` reconcile
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

운영자가 checkpoint를 수동으로만 만들면 긴 작업 전에 찍는 습관에 의존한다.
SERVICE_PLAN P2-12는 주기 checkpoint를 열기 **전에** retention과 용량 가드를 닫으라고
한다. Hyper-V AutoProtect처럼 상한이 없는 자동 체인은 디스크와 restore 모호성을 키운다.

이 설계는 새 Feature ID를 만들지 않는다. `pcv.checkpoint.lifecycle`에 붙인다.
P0 evidence 후보 4개와 catalog feature 28개는 그대로다.

## 현재 계약

- `POST /api/v1/vms/{vmId}/checkpoints` `checkpoint.create`, permission `operate`.
- `DELETE /api/v1/vms/{vmId}/checkpoints/{checkpointId}` `checkpoint.delete`.
- 주기 스케줄 route, 파일 persist, 백그라운드 tick worker는 없다.
- catalog routes 71, queued 31. 이 slice는 HTTP를 추가하지 않는다.

## 결정

- Feature는 `pcv.checkpoint.lifecycle`이다. 29번째 feature id를 만들지 않는다.
- permission은 기존 `operate`다. service bearer도 받는다. 새 RBAC 이름을 만들지 않는다.
- 스케줄은 VM마다 opt-in이다. 호스트 전역 기본 on, 무한 간격, retention 0/없음은 거절한다.
- 대상 VM은 managed여야 한다. unmanaged Hyper-V VM에 자동 checkpoint를 걸지 않는다.
- template-lock된 VM은 start/clone만 허용하므로 주기 create를 거절한다.
- 간격은 최소 60분, 최대 10080분(7일). 그 밖은 `PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`.
- retention 상한은 필수이며 1..32. 없거나 0 이하는 무한 AutoProtect로 보고
  `PCV_CHECKPOINT_SCHEDULE_RETENTION_REQUIRED`. 32 초과는
  `PCV_CHECKPOINT_SCHEDULE_RETENTION_INVALID`.
- 용량 가드: 현재 checkpoint 수가 retention 이상이면 **due create**를 거절한다.
  스케줄 set 자체는 허용한다(다음 tick이 막힌다). 남은 디스크가 10 GiB 미만이거나
  예상 checkpoint 크기가 남은 용량보다 크면 due create를 거절한다.
- due create가 통과하면 기존 `checkpoint.create` queued mutation만 쓴다. 새 Hyper-V
  자동 checkpoint API를 추가하지 않는다. 자동 retry는 금지. prune은 기존
  `checkpoint.delete`를 운영자가 또는 이후 slice의 명시 job으로만 한다.
- 정책 평가는 IO가 없다. 현재 개수와 디스크 값은 호출자가 request에 넣는다.
- Web은 이 설계에서 스케줄을 켜는 폼을 열지 않는다. 이후 slice의 readback만 검토한다.
- P2-13 export/import, P2-14 네트워크 변경, P2-15 NIC/DVD add, current-evidence write,
  package-pair, Lane 2 Hyper-V 복구는 이 설계 밖이다.

## 스키마 `pcv-checkpoint-schedule-v1`

```json
{
  "schema": "pcv-checkpoint-schedule-v1",
  "enabled": true,
  "vm_name": "lab-vm",
  "interval_minutes": 1440,
  "retention_max": 8
}
```

`enabled=false`이면 interval/retention은 없어도 된다. `enabled=true`이면 둘 다 필요하다.

## Slice

### Slice 1 — 정책 계약 (이번)

- `DesktopNode.Contracts` static `CheckpointSchedulePolicy`.
- `EvaluatePreview` / `EvaluateSet` / `EvaluateClear` / `EvaluateDueCreate`.
- HTTP, persist, CLI, Web, Host timer는 이 slice가 아니다.

### Slice 2 — preview HTTP/CLI

- `POST /api/v1/vms/{vmId}/checkpoints/schedule/preview`.
- `pcvcli vm checkpoint schedule preview <vm> --interval-minutes N --retention-max N`.
- catalog pin. persist와 tick worker는 이 slice가 아니다.

### Slice 3 — queued set/clear persist

- 파일 persist, in-process reload, 이전 파일 rollback, `--yes`.
- due create worker는 이 slice가 아니다.

### Slice 4 — due create worker와 Web readback

- 만기이면 기존 `checkpoint.create`만 enqueue. Web은 스케줄 상태 readback.
- 스케줄 on 토글 기본값, 무한 retention UI는 열지 않는다.

### Slice 5 — retention prune 명시 job

- due이고 checkpoint 수가 retention 이상이면 기존 `checkpoint.delete` queued job만 enqueue한다.
- 대상은 이름 접두사 `pcv-schedule-`인 checkpoint 중 가장 오래된 것 한 개다. 운영자가 만든 이름은 지우지 않는다.
- native delete를 직접 호출하지 않는다. 자동 retry 금지. 새 HTTP route를 추가하지 않는다.

## 거절 코드

`PCV_CHECKPOINT_SCHEDULE_FORBIDDEN`, `PCV_CHECKPOINT_SCHEDULE_VM_REQUIRED`,
`PCV_CHECKPOINT_SCHEDULE_NOT_MANAGED`, `PCV_CHECKPOINT_SCHEDULE_TEMPLATE_LOCKED`,
`PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`, `PCV_CHECKPOINT_SCHEDULE_RETENTION_REQUIRED`,
`PCV_CHECKPOINT_SCHEDULE_RETENTION_INVALID`, `PCV_CHECKPOINT_SCHEDULE_NOT_ENABLED`,
`PCV_CHECKPOINT_SCHEDULE_CAPACITY_EXCEEDED`.

Auth 실패는 host/interval 검사보다 먼저다.

## 검증

- Slice 1: `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj -c Release --filter FullyQualifiedName~CheckpointSchedulePolicyTests`
- Slice 2~3: focused API/CLI + catalog pin. dirty-tree는 four-shard PASS가 아니다.
- Slice 4: worker 단위 테스트. 설치본 actual-VM 주기는 Lane 2이며 이 캠페인이 열지 않는다.

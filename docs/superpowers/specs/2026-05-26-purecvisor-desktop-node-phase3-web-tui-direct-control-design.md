# Phase 3 Web/TUI Direct Control Design

date: `2026-05-26`
status: `approved-for-implementation`
scope: `operator-surface-productization`
upstream_policy: `docs/adr/0008-hyperv-qos-mutation-policy.md`
deferred_security: `docs/adr/0009-guest-execution-security-boundary.md`
deferred_novnc_config: `docs/adr/0010-account-novnc-target-config-security-policy-candidate.md`

## 목표

Phase 3는 이미 설치본 fullgate와 manual-admin package-pair로 닫힌 ADR-0008 Hyper-V QoS
mutation을 Web Console과 TUI 운영자 여정에 연결한다. 새 backend 의미를 추가하지 않고,
기존 Local API contract만 사용한다.

## 포함 범위

| Surface | 기능 | Contract |
| --- | --- | --- |
| Web Console | selected VM QoS storage/network preview form | `POST /api/v1/vms/{vm}/qos/storage/preview`, `POST /api/v1/vms/{vm}/qos/network/preview` |
| Web Console | selected VM QoS storage/network apply form | `POST /api/v1/vms/{vm}/qos/storage`, `POST /api/v1/vms/{vm}/qos/network` |
| TUI | selected VM QoS reset preview | preview route 2개를 호출하며 host mutation 없음 |
| TUI | selected VM QoS reset apply | 명시 confirmation 뒤 apply route 2개를 호출 |
| Evidence | code-level direct control evidence | Web/TUI tests와 generated asset parity |

## 제외 범위

| 후보 | Phase 3 결정 |
| --- | --- |
| `pcvcli vm guest-exec` | ADR-0009 boundary contract는 적용됐지만 credential/audit/redaction/timeout/cancel/RBAC 구현 payload 전까지 미구현 |
| `pcvcli vm guest-agent-ensure-channel` | ADR-0009 channel lifecycle 구현 payload 전까지 미구현 |
| Web/TUI guest command panel | ADR-0009 policy/API implementation과 confirmation/audit UX 전까지 미구현 |
| Account/noVNC target config mutation | ADR-0010이 닫힐 때까지 미구현 |
| Linux runtime object 계열 | Windows Desktop Node 제품 범위 밖 |

## UX 규칙

1. Preview는 apply route를 호출하지 않는다.
2. Apply는 명시 confirmation을 요구한다.
3. Web apply button은 `operate` permission 상태와 action pending 상태를 따른다.
4. TUI apply는 안전한 reset payload만 제공한다. 임의 수치 입력형 mutation은 TUI input
   contract를 별도 slice로 열기 전까지 제공하지 않는다.
5. Guest execution, guest channel, account/noVNC target config mutation은 화면에 보류
   상태로만 노출하고 실행 버튼을 추가하지 않는다.

## Payload

Storage preview/apply:

```json
{
  "disk": "disk0",
  "maximum_iops": 120,
  "minimum_iops": 0
}
```

Network preview/apply:

```json
{
  "adapter": "adapter0",
  "maximum_kbps": 20480,
  "minimum_kbps": 0
}
```

TUI reset payload는 storage/network 모두 `maximum=0`, `minimum=0`을 사용한다. 이 값은
ADR-0008 설치본 rollback smoke에서 사용한 reset semantics와 맞춘다.

## Package Chain

Web/TUI product payload가 바뀌므로 code-level PASS 뒤 `0.42.48-admin-smoke` package chain을
연다. 2026-05-26 기준 package build, full admin host mutation gate, installed Web/TUI/CLI
current-card smoke는 PASS했고, manual-admin package-pair closure는
`0.42.47-admin-smoke -> 0.42.48-admin-smoke` 후속 gate로 남긴다.

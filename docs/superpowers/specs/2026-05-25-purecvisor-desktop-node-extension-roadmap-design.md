# PureCVisor Desktop Node 확장 기능 로드맵 설계

## 목적

이 문서는 Windows Desktop Node가 Linux PureCVisor parity와 Operator Surface 확장을
어떤 순서와 경계로 검토할지 정리한다.

이번 산출물은 구현 slice가 아니다. `Guest Execution / Guest Channel`, Hyper-V QoS
mutation, Web/TUI direct control, account/noVNC Operator Surface, Linux Single Runtime
Object 계열을 하나의 phase map으로 정리하고, 각 phase가 구현으로 진입하기 전에 필요한
ADR, 보안 경계, evidence gate, release gate를 정의한다. 첫 구현 slice는 이 문서 승인 후
별도 `domain-architecture`, `grill-me`, `writing-plans` 단계에서 선택한다.

## 결정 마커

```text
DESKTOP_NODE_EXTENSION_ROADMAP_DECISION: phase-map-before-implementation-slice
desktop_node_extension_scope: windows-hyperv-product-boundary-first
first_implementation_slice: not-selected-by-this-spec
guest_exec_boundary: security-boundary-deferred
hyperv_qos_mutation_boundary: policy-adr-required
web_tui_direct_control_boundary: backend-policy-first
account_novnc_boundary: reproductize-and-reverify-on-next-operator-surface-payload
linux_single_runtime_object_boundary: out-of-product-scope-until-product-line-adr
public_release: not-claimed
```

## 현재 문서 경계

현재 Desktop Node의 PCVCLI/Operator Surface 경계는 다음 문서가 소유한다.

- `docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md`
- `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md`
- `docs/ga-ready/evidence/pcvcli-linux-command-coverage-matrix-2026-05-19-04232.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`
- `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04229.md`

이 문서들은 Windows Desktop Node가 Linux `pcvctl` command name을 그대로 복제하는 제품이
아니라, Windows Hyper-V Local API를 기준으로 안전하게 재해석 가능한 surface만 제품
claim으로 승격한다고 정리한다.

현재 supported 상태는 다음과 같다.

| 영역 | 현재 상태 | 유지할 판단 |
| --- | --- | --- |
| `vm limit` | Hyper-V resource mutation으로 지원 | Linux cgroup 호환은 주장하지 않는다. |
| `vm blkio-get` | Hyper-V storage readback으로 지원 | `blkio-set` mutation과 분리한다. |
| `vm bandwidth` | Hyper-V network readback으로 지원 | switch port bandwidth mutation과 분리한다. |
| `vm guest-agent-status`, `vm guest-ping` | Hyper-V Integration Services readback으로 지원 | qemu guest agent 또는 credentialless heartbeat claim은 하지 않는다. |
| Web/TUI QoS/guest surface | read-only panel로 지원 | direct mutation/control은 닫혀 있다. |
| account/noVNC | historical installed PASS | 다음 Operator Surface product payload 변경 때 최신 package 기준 재검증한다. |

현재 deferred/out-of-scope 상태는 다음과 같다.

| 영역 | 현재 상태 | 재검토 조건 |
| --- | --- | --- |
| `vm guest-agent-ensure-channel`, `vm guest-exec` | `security-boundary-deferred` | credential, audit, secret redaction, RBAC, timeout/cancel ADR 필요 |
| `vm blkio-set`, switch port bandwidth mutation | `hyperv-qos-mutation-policy` 후보 | rollback/readback, policy schema, host mutation evidence 필요 |
| Web/TUI direct control | ADR-0007에서 닫힘 | backend mutation policy가 먼저 닫혀야 함 |
| Linux Single Runtime Object 계열 | `out-of-product-scope` | 별도 제품 라인 ADR 필요 |

## Phase Map

### Phase 0. Extension Roadmap And ADR Backlog

Phase 0은 이 문서가 소유한다. 목적은 구현 없이 확장 후보를 제품 경계, 보안 경계,
evidence gate 기준으로 나누는 것이다.

산출물은 다음을 포함한다.

- 확장 후보별 현재 상태와 재검토 조건
- 필요한 ADR 목록
- implementation slice 선택 규칙
- release/evidence gate 공통 규칙

Phase 0은 host mutation, package build, public release를 수행하지 않는다.

### Phase 1. Account/noVNC Operator Surface Reproductization

Phase 1은 가장 낮은 위험의 첫 구현 후보다. 기존 account login, browser session,
target-backed noVNC streaming은 historical PASS evidence가 있으며, 다음 Operator Surface
payload 변경 때 최신 package 기준으로 다시 제품화/재검증하도록 ledger에 기록되어 있다.

목표는 account/noVNC를 새 기능처럼 확장하기보다, 최신 Desktop Node package에서 운영자 여정과
evidence contract를 다시 정렬하는 것이다.

예상 범위는 다음과 같다.

- Web Console account login/session/RBAC 흐름 재확인
- browser session smoke와 token/password redaction 재확인
- noVNC target-backed streaming 경계 재확인
- TUI/CLI가 console/noVNC handoff를 일관되게 표시하는지 재확인
- installed Web/TUI/CLI current-card와 account/noVNC smoke evidence 연결

Phase 1은 guest 내부 명령 실행, QoS mutation, Linux Single runtime object를 열지 않는다.
noVNC bridge는 explicit target host/port 구성 전까지 disabled 상태를 유지한다. LAN exposure는
기존 network exposure gate를 따른다.

Phase 1이 구현 slice로 선택되면 product payload 변경 여부에 따라 새 admin-smoke package,
full admin host mutation gate, manual-admin package-pair, installed current-card smoke가 필요하다.

### Phase 2. Hyper-V QoS Mutation Policy

Phase 2는 `vm blkio-set`과 switch port bandwidth mutation을 Windows Hyper-V 의미로 새로
정의하는 단계다. Linux cgroup/libvirt semantics를 그대로 이식하지 않는다.

핵심 설계 질문은 다음이다.

- Storage QoS를 VHDX, VM, host policy 중 어느 수준에서 적용할지
- network bandwidth를 VM network adapter, switch port, QoS policy 중 어느 수준에서 적용할지
- mutation 전후 readback을 어떤 payload로 고정할지
- 실패 또는 rollback 시 어떤 상태를 원복해야 하는지
- live VM과 stopped VM에서 허용 범위를 다르게 둘지

Phase 2의 제품 명령 후보는 다음이다.

```text
pcvcli vm blkio-set <vm> --read-bps N --write-bps N
pcvcli vm bandwidth <vm> --inbound-kbps N --outbound-kbps N
```

단, `bandwidth`는 현재 readback 명령으로 이미 존재한다. mutation을 같은 command에 옵션으로
붙일지, `bandwidth-set`을 별도 command로 둘지는 ADR에서 결정한다. 운영자 혼동을 줄이기 위해
초기 설계에서는 mutation command를 readback command와 명확히 분리하는 방향을 선호한다.

Phase 2의 필수 gate는 다음이다.

- `hyperv-qos-mutation-policy` ADR
- dry-run/preview payload
- queued job contract
- rollback/readback evidence
- actual VM mutation smoke
- full admin host mutation gate
- manual-admin package-pair closure

### Phase 3. Web/TUI Direct Control For Approved Mutations

Phase 3은 Web/TUI에서 direct mutation/control을 여는 단계다. 이 phase는 backend mutation
policy가 먼저 닫힌 기능만 표면에 노출한다.

초기 direct control 후보는 다음 순서로 제한한다.

1. 기존에 지원되는 `vm limit`
2. Phase 2에서 승인된 Hyper-V QoS mutation
3. Phase 4에서 승인된 guest command execution

Web/TUI는 destructive 또는 privileged mutation을 즉시 실행하지 않는다. 공통 UX는 다음을
따른다.

- selected VM context 고정
- preview/readback 먼저 표시
- mutation 입력값 validation
- confirmation
- queued job 생성
- job progress와 cancel 가능 여부 표시
- 완료 후 readback refresh
- 실패 시 structured error와 diagnostics link 제공

TUI direct control은 단축키 중심으로 열되, 처음에는 read-only detail panel에서 mutation
dialog로 진입하는 방식을 사용한다. Web direct control은 기존 운영 대시보드의 VM detail 흐름에
붙이며, 랜딩 페이지나 별도 marketing 화면을 만들지 않는다.

Phase 3의 필수 gate는 Web/TUI UI test, route contract test, installed current-card smoke,
actual VM smoke다. Guest credential 또는 secret이 포함되는 기능은 Phase 4 security boundary가
닫히기 전까지 Web/TUI에 노출하지 않는다.

### Phase 4. Guest Execution / Guest Channel Security Boundary

Phase 4는 가장 위험도가 높은 확장이다. `pcvcli vm guest-exec <vm> -- <command>`와
`pcvcli vm guest-agent-ensure-channel <vm>`는 단순 CLI command가 아니라 guest 내부 권한,
credential 처리, audit log, secret redaction, timeout/cancel, RBAC가 모두 연결되는 경계다.

Windows Desktop Node는 qemu guest agent channel을 그대로 제공하지 않는다. Hyper-V 제품
의미로는 다음 후보 중 하나를 선택해야 한다.

- Hyper-V PowerShell Direct 기반 guest command execution
- Hyper-V Guest Service Interface 기반 file copy/channel 준비
- explicit in-guest agent를 설치한 뒤 agent RPC로 실행

초기 설계에서는 PowerShell Direct를 기본 후보로 보되, credential/audit 경계가 닫히기 전까지
제품 route로 열지 않는다. in-guest agent 방식은 별도 agent lifecycle과 update/security model을
요구하므로 Phase 4 안에서도 별도 sub-ADR로 다룬다.

Phase 4의 필수 보안 결정은 다음이다.

- guest credential 입력/저장 금지 또는 저장 방식
- one-shot credential 사용 여부
- Windows Credential Manager 연계 여부
- account/RBAC capability 이름
- audit log schema
- command, args, stdout, stderr redaction 규칙
- max runtime, output limit, exit code payload
- cancel semantics
- concurrent execution limit
- allowlist/denylist policy
- installed LocalSystem service가 guest command를 대리 실행할 때의 권한 경계

Phase 4가 닫히기 전에는 Web/TUI guest command 실행 버튼을 만들지 않는다.

### Phase 5. Linux Single Runtime Object Product Line Review

Phase 5는 `nic`, `iso`, `storage`, `device`, `container`, `ovn`, `dpdk`, `sriov`,
`template`, `backup`, `alert`, `agent`, `batch`, `prometheus`, `webhook`, `security-group`,
`gpu`, `cloud` 같은 Linux Single Runtime Object 계열을 다룬다.

이 영역은 Windows Desktop Node Hyper-V Local API에 단순 command를 추가하는 문제가 아니다.
Linux PureCVisor의 KVM/libvirt/LXC/ZFS/OVS/OVN 계층을 Windows 제품에 새로 정의할지 결정해야
한다. 따라서 Phase 5는 Desktop Node 내부 기능 slice가 아니라 별도 제품 라인 검토로 둔다.

Phase 5의 초기 결론은 유지다.

```text
linux_single_runtime_object_boundary: out-of-product-scope-until-product-line-adr
```

이 경계를 변경하려면 다음이 필요하다.

- Linux Single Runtime Object를 Windows 제품에 들여올지에 대한 제품 ADR
- Hyper-V와 무관한 runtime provider 도입 여부
- data model과 API namespace 분리
- packaging/release/evidence chain 분리
- 기존 Desktop Node current evidence와의 claim 분리

## Cross-Cutting Gates

모든 phase는 다음 공통 gate를 따른다.

| Gate | 적용 대상 | 기준 |
| --- | --- | --- |
| ADR gate | Phase 2, 4, 5 | 기존 ADR-0007 결정을 바꾸거나 확장하는 경우 필수 |
| Runtime policy gate | CLI/API/Web/TUI에 새 operation을 노출하는 경우 | `runtime.policy`에 native/read/write/queued 여부를 명시 |
| RBAC gate | account/noVNC, Web/TUI direct control, guest-exec | capability 이름과 권한 모델을 문서화 |
| Audit gate | mutation, guest-exec, credential flow | command/result/actor/request id 기록, secret redaction |
| Secret redaction gate | credential, token, guest command output | token/password/secret literal이 artifact/UI/log에 노출되지 않아야 함 |
| Timeout/cancel gate | queued mutation, guest-exec | max runtime, cancel 가능 여부, terminal state를 명시 |
| Rollback/readback gate | QoS mutation, resource mutation | mutation 후 readback과 실패 시 원복 기준 필요 |
| Current-card gate | Operator Surface 변경 | installed Web/TUI/CLI current-card smoke 필요 |
| Host mutation gate | installed service/Hyper-V/OS mutation | full admin host mutation gate 필요 |
| Manual-admin package-pair gate | product payload 변경 | package build, update/rollback, clean-host, Burn, MSIX, descriptor closure 필요 |

## API와 Command 설계 원칙

새 command는 Linux command name을 차용할 수 있지만, payload에는 Windows Hyper-V 의미를
명확히 기록한다.

예시는 다음과 같다.

```json
{
  "operation": "vm.guest_exec",
  "provider": "hyperv-powershell-direct",
  "linux_qemu_guest_agent_compatible": false,
  "queued": true,
  "audit_required": true
}
```

CLI는 unsupported 상태를 조용히 성공으로 보이지 않는다. 아직 gate가 닫히지 않은 command는
명시적으로 `PCV_CLI_BACKEND_NOT_EXPOSED`, `PCV_CLI_SECURITY_BOUNDARY_REQUIRED`,
`PCV_CLI_MANUAL_ADMIN_GATE_REQUIRED` 같은 structured error를 반환한다.

Web/TUI는 backend가 read-only인 기능을 mutation처럼 보이게 만들지 않는다. 직접 제어가 열리는
순간에는 confirmation, job status, readback refresh가 한 흐름으로 제공되어야 한다.

## Evidence And Release Rules

이 roadmap의 어떤 phase도 public trusted signing, winget submission, public stable installer
URL, external stable publication을 주장하지 않는다. 현재 ADR-0006 internal private network
distribution boundary를 유지한다.

제품 payload가 바뀌는 phase는 다음 release chain을 요구한다.

1. code-level tests
2. package build
3. installed smoke
4. full admin host mutation gate
5. manual-admin package-pair closure
6. installed Web/TUI/CLI current-card
7. `CURRENT_EVIDENCE_LEDGER.md` 갱신
8. public-boundary CI guard

문서-only roadmap 변경은 host mutation과 package build를 요구하지 않는다.

## Implementation Slice Selection Rule

이 spec은 첫 구현 slice를 선택하지 않는다. 다음 단계에서 implementation slice를 고를 때는
아래 순서를 기본 추천으로 둔다.

1. Phase 1 Account/noVNC Operator Surface Reproductization
2. Phase 2 Hyper-V QoS Mutation Policy
3. Phase 3 Web/TUI Direct Control For Approved Mutations
4. Phase 4 Guest Execution / Guest Channel Security Boundary
5. Phase 5 Linux Single Runtime Object Product Line Review

기본 추천은 Phase 1이다. 기존 historical PASS evidence가 있고, 최신 payload 기준
재제품화/재검증으로 닫을 수 있어 가장 작은 blast radius로 신규 확장 규약을 검증할 수 있다.

Phase 2는 실제 host mutation과 rollback/readback 정책을 다루므로 Phase 1보다 무겁다.
Phase 4는 credential과 guest 내부 권한을 다루므로 보안 ADR 없이 구현하면 안 된다.
Phase 5는 Desktop Node 기능이 아니라 별도 제품 라인 판단이다.

## Non-Goals

- Linux KVM/libvirt/LXC/ZFS/OVS/OVN semantics를 Windows Desktop Node에 그대로 이식하지 않는다.
- `guest-exec`를 credential/audit/redaction ADR 없이 CLI에 노출하지 않는다.
- Web/TUI direct mutation을 backend policy보다 먼저 열지 않는다.
- readback command를 mutation 성공처럼 표시하지 않는다.
- public distribution claim을 추가하지 않는다.
- 이번 spec에서 package build, host mutation, manual-admin campaign을 실행하지 않는다.

## 검증 기준

이 문서는 설계 산출물이므로 검증 기준은 문서 정합성이다.

- 기존 ADR-0007 readback-first 결정을 뒤집지 않는다.
- `security-boundary-deferred`와 `hyperv-qos-mutation-policy` 후보를 별도 phase로 유지한다.
- account/noVNC는 historical PASS를 최신 payload 재검증 후보로만 다룬다.
- Linux Single Runtime Object 계열은 별도 제품 라인 ADR 전까지 out-of-product-scope로 둔다.
- 첫 implementation slice는 이 문서에서 선택하지 않는다.
- 모든 문장은 internal admin-smoke / ADR-0006 boundary 안에서 작성한다.

## 다음 단계

이 spec이 승인되면 lifecycle control plane의 다음 단계는 `domain-architecture`다.
`domain-architecture`에서는 Phase 1-5의 bounded context, provider boundary, API namespace,
evidence owner를 더 세밀하게 나눈다. 그 다음 `grill-me`, `plan-design-review`,
`superpowers:writing-plans`, `plan-eng-review` 순서로 첫 implementation slice를 확정한다.

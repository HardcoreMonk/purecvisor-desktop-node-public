# PureCVisor Desktop Node 확장 기능 Domain Architecture

## 목적

이 문서는 `2026-05-25-purecvisor-desktop-node-extension-roadmap-design.md` 승인 후속
`domain-architecture` 산출물이다. 목적은 확장 후보 5개를 bounded context, provider
boundary, API namespace, evidence owner로 나누고, 구현 전에 지켜야 할 의존 방향을 고정하는
것이다.

이 문서는 구현 계획이 아니다. 코드 변경, package build, host mutation, manual-admin campaign은
수행하지 않는다. 첫 implementation slice는 이 문서 이후 `grill-me`, `plan-design-review`,
`writing-plans`, `plan-eng-review`를 거친 뒤 선택한다.

## 결정 마커

```text
DESKTOP_NODE_EXTENSION_DOMAIN_ARCHITECTURE_DECISION: bounded-context-first
source_roadmap_spec: docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-roadmap-design.md
domain_architecture_scope: phase-1-to-5-owner-and-boundary-map
first_implementation_slice: not-selected-by-this-document
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 기준 코드와 문서

현재 경계는 아래 코드와 문서가 소유한다.

| 경계 | 현재 owner |
| --- | --- |
| Runtime policy contract | `src/DesktopNode.Contracts/RuntimePolicy.cs` |
| API route family/permission/mutation stance | `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`, `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs` |
| API request dispatch | `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs` |
| Runtime job state/cancel/retry | `src/DesktopNode.Runtime/JobStateTransitionPolicy.cs` |
| Hyper-V provider catalog | `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs`, `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs` |
| Hyper-V native adapter composition | `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs`, `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs` |
| CLI command routing | `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs` |
| Account/RBAC/JWT | `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` |
| Console/noVNC route handling | `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`, `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` |
| Host Ops lifecycle buckets | `src/DesktopNode.Contracts/HostOpsLifecycleDescriptor.cs`, `src/DesktopNode.Host/Ops/*` |
| Packaging/evidence gates | `packaging/windows-desktop-node/tools/*`, `docs/ga-ready/*` |

## Bounded Context Map

### Runtime/Core Context

Runtime/Core owns API route registration, auth/session boundary, runtime policy, job runtime, diagnostics,
request/correlation id propagation, and structured error shape.

Runtime/Core may know operation names and route families. It must not know WMI implementation details,
noVNC target internals, Windows Credential Manager mutation internals, or installer campaign details.

확장 기능에서 Runtime/Core의 책임은 다음이다.

- 새 operation이 read-only, product operation, queued mutation 중 무엇인지 고정
- required permission과 route family 정의
- job 생성, cancel/retry 가능 여부, terminal state 정의
- `runtime.policy`에 native/read/write/queued 상태 노출
- unsupported 또는 deferred command를 structured error로 반환

### Hyper-V Domain Context

Hyper-V Domain owns Windows Hyper-V provider boundary와 native adapter dispatch다. 현재
`DesktopNodeHyperVDomain.Catalog`는 `host`, `network`, `vm inventory`, `vm lifecycle`,
`checkpoint` domain을 제공한다.

확장 기능에서는 Hyper-V Domain을 다음처럼 확장 후보로 나눈다.

| 후보 domain | 상태 | provider boundary 후보 |
| --- | --- | --- |
| `vm-qos-readback` | existing readback | 기존 `vm-provider` |
| `vm-resource-mutation` | existing queued mutation | 기존 `vm-resource-mutation-provider` |
| `vm-storage-qos-mutation` | Phase 2 후보 | `vm-storage-qos-provider` |
| `vm-network-qos-mutation` | Phase 2 후보 | `vm-network-qos-provider` |
| `vm-guest-execution` | Phase 4 deferred | `vm-guest-execution-provider` 또는 별도 agent provider |

Hyper-V provider는 API/CLI/Web/TUI를 직접 참조하지 않는다. Provider는 typed request와
typed result만 다루며, auth/RBAC/audit/secret redaction은 provider 밖의 Runtime/Core와 보안
context가 소유한다.

### Operator Access Context

Operator Access owns account login, session, RBAC, console capability, browser session, noVNC handoff
and bridge boundary.

현재 account/noVNC는 historical PASS evidence가 있다. Phase 1에서는 기능 의미를 새로 넓히기보다
최신 package 기준으로 이 context를 다시 제품화하고 재검증한다.

Operator Access는 다음을 소유한다.

- account login/session/RBAC route
- `console.view`, `operate`, `job.control` 같은 permission mapping
- noVNC disabled-by-default boundary
- explicit target host/port gate
- token/password value redaction proof
- account/noVNC installed smoke evidence

Operator Access는 guest command execution credential을 소유하지 않는다. Guest credential은
Phase 4의 Guest Execution Security Context에서 별도로 결정한다.

### Operator Surfaces Context

Operator Surfaces owns Web Console, TUI, CLI presentation and command entry. 이 context는
Local API contract의 consumer다. Hyper-V provider, Host Ops native action, Windows Credential
Manager controller를 직접 호출하지 않는다.

Operator Surfaces는 다음 rule을 따른다.

- readback route는 read-only UI로 표시한다.
- mutation route는 preview, confirmation, queued job, status, readback refresh를 한 흐름으로 표시한다.
- direct control은 backend policy가 닫힌 operation만 표시한다.
- secret, token, password, guest credential을 화면이나 artifact에 노출하지 않는다.
- unsupported command는 성공처럼 표시하지 않는다.

Phase 3의 direct control은 이 context가 소유하지만, 승인된 mutation contract가 없으면 UI를 열 수 없다.

### Hyper-V QoS Policy Context

Hyper-V QoS Policy는 Phase 2에서 새로 열 수 있는 bounded context다. 이 context는 Linux
`blkio-set` 또는 libvirt/cgroup semantics를 그대로 구현하지 않는다. Windows Hyper-V에서 적용 가능한
storage/network throttling policy를 정의한다.

Hyper-V QoS Policy가 소유해야 할 결정은 다음이다.

- storage QoS 적용 단위
- network bandwidth 적용 단위
- live VM과 stopped VM 허용 범위
- dry-run/preview schema
- mutation request schema
- rollback/readback schema
- actual VM mutation smoke 기준

이 context는 Runtime Job Context를 통해서만 mutation을 수행한다. 즉시 실행 API나 UI direct call은
금지한다.

### Guest Execution Security Context

Guest Execution Security는 Phase 4에서만 열 수 있는 보안 bounded context다.

이 context는 다음을 먼저 결정해야 한다.

- execution backend 후보: PowerShell Direct, Hyper-V Guest Service Interface, explicit in-guest agent
- credential 입력 방식
- credential 저장 금지 또는 저장 방식
- Windows Credential Manager 연계 여부
- RBAC capability
- audit log schema
- command/args/stdout/stderr redaction
- timeout/cancel/output limit
- concurrent execution limit
- allowlist/denylist

Guest Execution Security가 닫히기 전까지 `pcvcli vm guest-exec`, `pcvcli vm guest-agent-ensure-channel`,
Web/TUI guest command control은 제품 route로 열지 않는다.

### Host Ops Context

Host Ops owns Windows service lifecycle, Event Log, firewall, trust store, Credential Manager,
data-root lifecycle. Host Ops는 VM lifecycle이나 guest command execution을 소유하지 않는다.

확장 기능에서 Host Ops가 관여할 수 있는 범위는 다음으로 제한한다.

- Credential Manager storage primitive 제공
- Event Log/audit sink 제공
- service-action lifecycle 제공
- diagnostics/evidence에 필요한 host state 제공

Host Ops가 guest command credential policy를 직접 결정하거나 Hyper-V QoS mutation을 실행하면
경계 위반이다.

### Packaging/Release/Evidence Context

Packaging/Release/Evidence owns package build, full admin host mutation, manual-admin package-pair,
installed current-card, public-boundary CI guard, evidence ledger update.

이 context는 제품 기능을 직접 구현하지 않는다. 대신 product payload 변경이 있을 때 어떤 release
chain이 필요한지 판정한다.

## API Namespace Map

아래 namespace는 domain architecture 관점의 후보다. implementation plan 전에는 route를 확정하지 않는다.

| 기능 | API namespace | 상태 | owner |
| --- | --- | --- | --- |
| account login/session/RBAC | `/api/v1/auth/*` | existing | Operator Access, Runtime/Core |
| console/noVNC capability | `/api/v1/console/*`, `/api/v1/vms/{vm}/console` | existing | Operator Access |
| QoS readback | `/api/v1/vms/{vm}/blkio`, `/api/v1/vms/{vm}/bandwidth` | existing read-only | Hyper-V Domain |
| guest service readback | `/api/v1/vms/{vm}/guest-agent/status`, `/api/v1/vms/{vm}/guest-agent/ping` | existing read-only | Hyper-V Domain |
| VM resource limit | `/api/v1/vms/{vm}/limit` | existing queued mutation | Runtime/Core, Hyper-V Domain |
| storage QoS mutation | `/api/v1/vms/{vm}/qos/storage` 또는 `/api/v1/vms/{vm}/blkio` POST | Phase 2 ADR 필요 | Hyper-V QoS Policy |
| network QoS mutation | `/api/v1/vms/{vm}/qos/network` 또는 `/api/v1/vms/{vm}/bandwidth` POST | Phase 2 ADR 필요 | Hyper-V QoS Policy |
| guest channel | `/api/v1/vms/{vm}/guest/channel` | Phase 4 deferred | Guest Execution Security |
| guest exec | `/api/v1/vms/{vm}/guest/exec` | Phase 4 deferred | Guest Execution Security |
| Linux Single objects | `/api/v1/linux-single/*` 또는 별도 API root | Phase 5 product-line ADR 필요 | 별도 제품 라인 |

Route naming은 ADR에서 확정한다. 같은 route에 GET/POST를 섞는 방식은 가능하지만 readback과 mutation의
운영자 혼동을 줄이는 쪽을 우선한다.

## Operation State Model

모든 operation은 다음 중 하나의 상태를 가진다.

| 상태 | 의미 | 예 |
| --- | --- | --- |
| `supported-read-only` | 상태 조회만 허용 | `vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status` |
| `supported-queued-mutation` | Runtime job으로 mutation 수행 | `vm.limit`, future QoS mutation |
| `supported-product-operation` | diagnostics/account처럼 product operation이지만 VM mutation은 아님 | diagnostics bundle create, auth/session |
| `deferred-security-boundary` | 보안 ADR 전까지 미지원 | `vm.guest-exec`, `vm.guest-agent-ensure-channel` |
| `deferred-policy-boundary` | 제품 policy ADR 전까지 미지원 | `vm.blkio-set`, switch bandwidth mutation |
| `out-of-product-scope` | Desktop Node 제품 경계 밖 | Linux Single Runtime Object 계열 |

CLI, API, Web, TUI는 이 상태를 다르게 해석하면 안 된다. CLI가 미지원으로 거절하는 operation을 Web/TUI가
우회해서 실행하는 것은 금지한다.

## 의존 방향

허용되는 의존 방향은 다음이다.

```text
CLI/TUI/Web
  -> Local API route contract
  -> Runtime/Core auth, permission, job runtime
  -> Hyper-V Domain provider boundary or Host Ops primitive
  -> Windows OS / Hyper-V

Packaging/Release/Evidence
  -> built product and artifact summaries
  -> docs/ga-ready ledger and matrices
```

금지되는 의존 방향은 다음이다.

- Web/TUI가 Hyper-V provider를 직접 호출
- CLI가 WMI/PowerShell Direct를 직접 호출
- Hyper-V provider가 account/RBAC/session을 직접 판단
- Host Ops가 VM lifecycle/QoS/guest-exec product semantics를 소유
- Guest Execution이 diagnostics redaction을 임의 문자열 치환으로만 처리
- Packaging tool이 product behavior를 runtime 대신 결정
- Linux Single Runtime Object가 Desktop Node current evidence claim 안으로 섞임

## Evidence Owner Map

| Phase | Evidence owner | 최소 evidence |
| --- | --- | --- |
| Phase 1 account/noVNC | Operator Access, Packaging/Release | installed account login, browser session, noVNC streaming, token/password redaction, current-card |
| Phase 2 QoS mutation | Hyper-V QoS Policy, Runtime/Core, Packaging/Release | ADR, route contract, job contract, actual VM mutation, rollback/readback, fullgate, manual-admin |
| Phase 3 Web/TUI direct control | Operator Surfaces, Runtime/Core | UI contract, keyboard/control flow, route contract, job progress, installed Web/TUI/CLI smoke |
| Phase 4 guest execution | Guest Execution Security, Runtime/Core, Host Ops primitive | security ADR, credential/audit/redaction tests, timeout/cancel tests, actual VM smoke, fullgate |
| Phase 5 Linux Single objects | Product Line ADR owner | product boundary ADR, namespace split, release/evidence separation |

## Phase별 Domain Readiness

| Phase | Domain readiness | 다음 단계 입력 |
| --- | --- | --- |
| Phase 1 | 기존 domain으로 충분 | grill-me에서 account/noVNC stale trigger와 최신 payload 조건 압박 |
| Phase 2 | 새 QoS Policy context 필요 | grill-me에서 storage/network policy, rollback, live/stopped VM 조건 압박 |
| Phase 3 | backend policy 이후 가능 | plan-design-review에서 Web/TUI direct control UX 압박 |
| Phase 4 | 새 Security context 필수 | grill-me에서 credential/audit/RBAC/redaction/timeout 압박 |
| Phase 5 | 별도 제품 라인 context 필수 | 현재 implementation plan 대상에서 제외 |

## 다음 Grill-Me 입력

다음 단계 `grill-me`는 아래 질문을 우선 압박해야 한다.

1. Phase 1 account/noVNC가 정말 첫 slice로 적합한가, 아니면 roadmap evidence만으로 충분한가?
2. Phase 2 QoS mutation은 storage와 network를 한 ADR로 묶을 수 있는가?
3. Web/TUI direct control은 `vm.limit`만 먼저 열어도 제품 가치가 있는가?
4. Guest Execution은 PowerShell Direct를 기본 후보로 둘 수 있는가, 아니면 in-guest agent ADR이 먼저 필요한가?
5. Linux Single Runtime Object 계열을 계속 out-of-product-scope로 둘 때 사용자에게 어떤 메시지를 보여야 하는가?

## Non-Goals

- 구현 순서와 작업량을 산정하지 않는다.
- route 이름을 최종 확정하지 않는다.
- guest credential 저장 방식을 확정하지 않는다.
- QoS storage/network policy를 확정하지 않는다.
- Web/TUI 화면 설계를 확정하지 않는다.
- package build, host mutation, manual-admin campaign을 실행하지 않는다.

## 검증 기준

이 문서는 다음을 만족해야 한다.

- Phase 1-5가 각각 하나 이상의 owner context를 가진다.
- deferred 항목을 supported처럼 표현하지 않는다.
- Linux Single Runtime Object 계열은 별도 제품 라인 전까지 out-of-product-scope로 유지한다.
- Operator Surface는 API consumer로만 남고 Hyper-V/Host Ops를 직접 호출하지 않는다.
- Runtime/Core, Hyper-V Domain, Host Ops, Packaging/Release의 기존 경계를 뒤집지 않는다.
- 다음 lifecycle 단계가 `grill-me`임을 명시한다.

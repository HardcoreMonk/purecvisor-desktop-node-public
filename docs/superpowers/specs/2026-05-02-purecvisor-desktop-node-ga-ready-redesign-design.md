# PureCVisor Desktop Node GA-ready 제품 재설계

작성 기준: 2026-05-02
현행화: 2026-05-05 ADR-0004 적용. 이 문서는 내부 전용 서비스 current decision의 supporting design으로 유지한다.

## 목적

이 문서는 Desktop Node를 내부 전용 GA-ready 제품 런타임으로 확정한 ADR-0004의 supporting design이다.

기존 Phase 25는 `.NET Host + .NET API/runtime 후보 + TypeScript Web Console 후보 + PowerShell Windows adapter` 전환을 시작했고, `DesktopNode.Host.exe`를 기본 service host와 listener owner로 교체했다. 후속 slice에서 Tier 1 read route, VM create/start/shutdown/poweroff/restart/delete, checkpoint create/restore/delete는 C# native adapter product path로 전환됐다. 2026-05-03 service status/start/stop code-level slice는 `DesktopNode.Host.exe service-action status|start|stop`에 native SCM controller와 ownership guard를 추가했다. `0.30.1-admin-smoke` installed destructive smoke는 VM delete managed/delete-repeat/unmanaged-guard evidence를 추가했다. 이후 GA-ready closure 작업에서 service install/repair/uninstall, update/rollback, data root, Event Log/firewall/trust store product ops의 PowerShell-backed current owner는 0개가 됐고, `spikes/**` 활성 제품 경로도 archive/read-only baseline으로 이동했다.

새 설계의 중심 사용자는 Windows 관리자다. 내부 개발/검증 운영자와 릴리스 담당자는 보조 persona다. 따라서 목표는 단순한 코드 이동이 아니라 설치, 수리, 삭제, 업데이트, 진단, 복구, 릴리스 신뢰 경계가 하나의 운영자 여정으로 이해되는 제품 구조를 만드는 것이다.

## 현재 결정

이 설계는 ADR-0004의 current decision과 같은 방향을 갖는다. 현재 적용 decision source는 `docs/ADR_INDEX.md`와 `docs/adr/0004-ga-ready-product-runtime-candidate.md`다.

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
```

`ga-ready-product-runtime`은 public trusted signing, external stable publication, 일반 사용자용 public release 실행을 뜻하지 않는다. 이 제품은 내부 전용 서비스이며, 외부 배포를 목표로 바꾸려면 별도 ADR이 필요하다.

## ADR 대체 범위와 승격 절차

ADR-0004 대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단이다.
`DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`와 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`는 별도 ADR이 바꾸기 전까지 현재 적용 결정으로 유지한다.

ADR-0004는 2026-05-05 aggregate closure와 같은 decision update 흐름에서 `적용 중`으로 바뀌었고, `docs/ADR_INDEX.md` 현재 적용 중인 ADR 표와 결정 마커도 함께 갱신됐다. 적용 후 `PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 ADR-0004 하나다.

이후 decision 변경 중단 조건:

- ADR 번호 충돌
- missing replacement closure report
- non-closed replacement closure report
- duplicate current product runtime promotion source
- missing preserved non-promotion current marker

## GA-ready 기준

GA-ready 기준은 다음 네 가지다.

1. 제품 runtime/request path에서 PowerShell helper dependency를 제거한다.
2. 제품 배포/운영 경로에서 PowerShell dependency를 제거한다.
3. Hyper-V 조작은 C# WMI/CIM adapter 중심으로 전환한다.
4. `spikes/**`는 활성 제품 경로에서 제거하거나 `archive/**`로 이동한다.

PowerShell은 설계 이력, archive baseline, 일회성 개발 도구로 남을 수 있지만 GA-ready 제품 behavior의 owner가 될 수 없다.

## Target Architecture

### `src/DesktopNode.Host`

제품 process owner다.

- Local API listener와 Windows Service entrypoint를 소유한다.
- install, repair, remove, status 같은 service-action을 소유한다.
- DPAPI protected token bootstrap과 ACL 적용을 소유한다.
- Web Console build output static serving을 소유한다.
- 제품 경로에서 PowerShell script를 호출하지 않는다.

### `src/DesktopNode.Api`

HTTP transport와 분리된 request processor다.

- route ownership, auth, request validation, JSON response contract를 소유한다.
- job dispatch와 route별 adapter 선택을 명시한다.
- diagnostics/runtime policy response contract를 소유한다.
- route별 current/target owner는 route promotion matrix와 일치해야 한다.

### `src/DesktopNode.Runtime`

순수 runtime policy와 job state machine을 소유한다.

- job state transition
- retry/cancel/recovery policy
- JSON job store schema와 migration
- no-auto-reboot classification metadata

### `src/DesktopNode.HyperV`

C# WMI/CIM 기반 Hyper-V adapter를 소유한다.

- Hyper-V read model과 mutation operation을 route별로 구현한다.
- PowerShell helper fallback은 read route product path에서 제거됐고, mutation route는 transition 기간에만 route별로 허용한다.
- GA-ready target에서는 제품 route fallback이 없어야 한다.
- WMI/CIM parity가 부족한 route는 GA-ready blocker로 남긴다.

### `web/**`

TypeScript Web Console app이다.

- 개발 구조는 독립 frontend app으로 둔다.
- 빌드 산출물은 Local API static serving과 MSI payload에 포함한다.
- 2026-05-03 Web Console served asset/root migration slice 이후 repo-root `web/src/served-app.ts`가 served `web/app.js`의 product owner다.
- DOM-level fixture와 browser interaction 검증을 갖춘다.

## Web Console Design Scope

GA-ready redesign은 새 Web Console 화면을 처음부터 재설계하지 않는다.
승인 시 목표 상태는 기존 Desktop Node 운영 콘솔 UX를 `web/**` 제품 owner와 TypeScript/package-owned verification으로 승격하는 것이다.

What already exists:

- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3a-web-console-design.md`는 Dashboard + VM Table 운영 콘솔, top bar, left nav, host status summary, VM table, recent jobs panel, create VM form, API token interaction을 정의한다.
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-p2-final-mockup-copy-package-design.md`는 Web Console normal, VM empty, job empty, unauthorized, Local API offline/error, API error, LAN mode warning 상태와 한국어/영어 copy를 정의한다.
- `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`는 TypeScript source, generated parity manifest, user-visible fixture, static asset parity 기준을 정의한다.

NOT in scope:

- 새 marketing/landing hero 또는 3-column feature grid 추가
- Single Edge Linux UI/API 통합
- 새로운 visual brand system 확정
- LAN mode default 변경
- token 발급, 저장소 암호화, multi-user login, RBAC UI
- WebSocket/event stream 기반 실시간 UI
- VMConnect 실행 UI
- 이 spec만으로 browser automation dependency 도입

## Web Console Information Architecture

GA-ready Web Console 첫 화면은 운영자가 3초 안에 node 상태와 action boundary를 판단할 수 있는 조밀한 운영 콘솔이어야 한다.

```text
Top bar
- Product name: PureCVisor Desktop Node
- Connection state: Connected / Token required / Service offline / LAN mode
- API base URL and token session controls
- Refresh action

Main priority 1: host and listener truth
- Host capability
- Service status
- Local API listener binding
- Version/channel
- Loopback/LAN policy

Main priority 2: operations surface
- VM inventory table
- Recent jobs panel
- Retry/cancel availability
- Explicit admin opt-in boundary for mutation actions

Main priority 3: support handoff
- Diagnostics evidence bundle reference
- Last verification or evidence timestamp
- Error code/message/job id fields
- internal-only service/public-release boundary note
```

No section should imply external publication, public trusted signing, or administrator mutation happened only because the console can display the state.

## Web Console Interaction States

후속 Web Console implementation이나 browser-level fixture 후보는 최소한 다음 user-visible states를 유지해야 한다.

| State | User sees | Source design |
|---|---|---|
| Normal dashboard | product title, connection badge, host card, VM table, job panel, diagnostics panel, non-GA note | P2 final mockup/copy package |
| VM inventory empty | empty VM copy and non-mutating create preparation CTA | P2 final mockup/copy package |
| Job empty | recent job empty copy without implying hidden failures | P2 final mockup/copy package |
| Unauthorized | API token required copy and session-only token action | P2 final mockup/copy package |
| Local API offline/error | service/listener troubleshooting copy with diagnostics handoff | P2 final mockup/copy package |
| API error | `PCV_*` code, message, job id, retry availability | P2 final mockup/copy package |
| LAN mode warning | explicit opt-in and token source warning | P2 final mockup/copy package |

State copy must not expose bearer token, API token, `Authorization` header value, protected token file content, private key, PFX password, or certificate secret material.

## Web Console Visual and Accessibility Guard

- Visual style remains a quiet Windows administrator console: dense, scan-friendly, low ornament, table-first where comparison matters.
- Cards are limited to repeated items and state summaries; the full page must not be wrapped in a decorative card.
- Avoid marketing hero layouts, decorative gradients, oversized slogans, generic feature grids, and visual claims that imply GA release.
- Keyboard navigation must reach token controls, refresh, VM table actions, job retry/cancel actions, and diagnostics links in a predictable order.
- Focus indicators must be visible without relying on hover.
- Interactive targets should be at least 44px on touch-capable layouts.
- Status badges must not rely on color alone; each status needs text.
- Error, unauthorized, empty, and LAN warning states need stable DOM markers for fixture verification.

### `packaging/windows-desktop-node/**`

MSI, release channel, provenance, signing, update package contract를 소유한다.

- PowerShell orchestration은 제거 대상이다.
- installer custom action은 installed product binary를 사용한다. WiX table로 표현 가능한 declarative install behavior는 별도 custom action으로 만들지 않는다.
- `REBOOT=ReallySuppress`, `/norestart`, no-auto-reboot evidence contract를 계속 유지한다.

### `archive/**`

`spikes/**` 이력과 legacy baseline을 보존하는 위치다.

- archive는 제품 runtime source가 아니다.
- archive test는 migration compatibility 확인에만 사용한다.
- 문서 링크는 migration map으로 이동 여부를 추적한다.

## Route Promotion Matrix

GA-ready 전환 단위는 route 또는 product operation이다.
상세 route contract는 `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`가 단일 진실이며, 이 설계 문서는 상위 field schema와 gate 의미만 둔다.
Route matrix는 Markdown contract이며 machine-readable JSON은 만들지 않는다.

필드 schema:

| Field | Required | Allowed values |
|---|---:|---|
| `route_surface` | yes | `current-route`, `future-route`, `product-operation` |
| `domain` | yes | non-empty string |
| `risk_tier` | yes | `tier1-read-only`, `tier2-reversible-mutation`, `tier3-destructive-or-persistent` |
| `current_owner` | yes | `dotnet-native`, `dotnet-request-processor-powershell-helper`, `powershell-helper`, `product-wrapper`, `dotnet-runtime`, `mixed-history`, `not-yet-defined`, `not-implemented` |
| `target_owner` | yes | `dotnet-host-adapter`, `dotnet-hyperv-adapter`, `dotnet-runtime`, `dotnet-job-store-migration-action`, `dotnet-service-action`, `dotnet-token-storage-action`, `dotnet-config-migration-action`, `dotnet-data-root-action`, `windows-native-package`, `windows-eventlog-action`, `windows-firewall-action`, `windows-trust-store-action` |
| `implementation_basis` | yes | `registry-wmi-service`, `wmi-cim`, `dotnet-runtime`, `product-config-migration-plan`, `job-store-migration-plan`, `windows-native-api`, `dpapi-local-machine-token-plan`, `windows-certificate-store-api`, `eventlog-registration-plan`, `firewall-rule-plan`, `data-root-lifecycle-plan`, `package-contract`, `approved-system-executable` |
| `fallback_policy` | yes | `none`, `test-only`, `transition-helper`, `blocked` |
| `promotion_state` | yes | `current-native`, `transition-helper`, `blocked`, `ga-ready-candidate` |
| `admin_smoke_required` | yes | `none`, `installed-non-mutating`, `explicit-admin-opt-in` |
| `release_gate` | yes | `none`, `release-approval-required` |
| `network_exposure_gate` | yes | `none`, `lan-exposure-approval-required` |

위험 등급은 다음과 같다.

- Tier 1 read-only: host/network/VM/checkpoint 조회
- Tier 2 reversible mutation: start, poweroff, checkpoint create/restore/delete, failed-job retry
- Tier 3 destructive or persistent mutation: VM create/remove, update/rollback, uninstall/remove-data, firewall/Event Log/trust 변경

State invariants:

- `promotion_state = current-native` allows only `fallback_policy = none` or `fallback_policy = test-only`.
- `promotion_state = transition-helper` requires `fallback_policy = transition-helper`.
- `promotion_state = blocked` requires `fallback_policy = blocked`.
- `promotion_state = ga-ready-candidate` allows only `fallback_policy = none` or `fallback_policy = test-only`.
- `risk_tier = tier1-read-only` allows only `admin_smoke_required = none` or `admin_smoke_required = installed-non-mutating`.
- `risk_tier = tier2-reversible-mutation` requires `admin_smoke_required = explicit-admin-opt-in`.
- `risk_tier = tier3-destructive-or-persistent` requires `admin_smoke_required = explicit-admin-opt-in`.

## Promotion Rules

Tier 1 route는 다음이 모두 충족되어야 promoted 상태가 된다.

- C# WMI/CIM adapter가 helper contract와 같은 public field를 반환한다.
- unsupported host, missing feature, access denied, not found error contract가 고정된다.
- xUnit contract test와 installed non-mutating route smoke가 통과한다.
- route matrix에서 transition fallback 제거 조건이 닫힌다.

Tier 2 route는 Tier 1 조건에 더해 다음이 필요하다.

- queued/running/succeeded/failed job state가 기존 public contract와 호환된다.
- cancel/retry/idempotency/timeout behavior가 테스트된다.
- 실패 중간 상태에서 cleanup 또는 safe recovery evidence가 있다.
- 관리자 opt-in smoke가 자동 reboot 없이 통과한다.

Tier 3 operation은 Tier 2 조건에 더해 다음이 필요하다.

- explicit admin opt-in smoke가 있다.
- no-auto-reboot evidence가 있다.
- rollback 또는 remove-data cleanup evidence가 있다.
- signing/channel/provenance policy와 충돌하지 않는다.
- diagnostics bundle이 변경 전후 상태와 cleanup 결과를 설명한다.

## Fallback Policy

Fallback policy는 세 단계로 구분한다.

- Current: 2026-05-03 read-route fallback removal slice 이후 Tier 1 read route는 native structured success/failure를 직접 반환한다.
- Transition: mutation route별 fallback 제거 gate를 둔다.
- GA-ready: 제품 route fallback이 없다. fallback이 필요한 route는 GA-ready blocker다.

2026-05-03 read-route fallback removal slice 이후 `network.inventory`, `vm.list`, VM detail, checkpoint list는 native structured success/failure를 직접 반환하며 product helper fallback을 사용하지 않는다. VM power-state/checkpoint/native lifecycle adapter slices 이후 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete도 native structured success/failure를 직접 반환한다. Native VM create product path는 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Transition fallback은 product operation row처럼 아직 current owner가 PowerShell-backed인 row에만 남는다.

## Aggregate GA-ready Gate

ADR-0004를 current decision으로 적용하기 전에는 route matrix와 supporting docs 기준으로 다음 aggregate gate가 닫혀야 했다.

- GA 범위의 `current-route`와 `product-operation` row는 `promotion_state = transition-helper` 또는 `promotion_state = blocked`가 0개여야 한다.
- `future-route` row는 GA 범위 제외 사유와 별도 implementation plan requirement를 명시해야 한다.
- 제품 runtime/request/admin execution path에는 PowerShell helper가 없어야 한다.
- 활성 제품 경로에는 `spikes/**`가 없어야 한다.
- repo migration preflight evidence와 verification ownership replacement evidence가 완료되어야 한다.
- `tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row는 explicit admin opt-in evidence가 완료되어야 하며, stale evidence는 aggregate GA-ready gate 충족에 사용할 수 없다.
- `release_gate = release-approval-required` row는 GA-ready 판정과 release execution을 분리하며, 별도 release approval 전에는 실행하지 않는다.

이 aggregate gate는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`에서 `aggregate_gate_status = closed`로 닫혔고, ADR-0004는 내부 전용 서비스 current decision으로 적용됐다.

## Active Product Path Classification

- `spikes/**` path가 runtime/service/API/CLI/Web Console execution, packaging input, installer input, static asset source, generated parity manifest, required verification command, CI/local verification command, or developer command documentation에 남아 있으면 active product path로 간주한다.
- `archive/spikes/**` reference는 historical/read-only baseline intent일 때만 허용하며 product execution, packaging, required verification source로 사용할 수 없다.
- `docs/**` command가 `spikes/**`를 required product path로 실행하도록 안내하면 active product path로 간주한다.
- Aggregate GA-ready gate closure에는 `spikes/**` active product path가 0개라는 repo migration preflight evidence와 docs command update evidence가 필요하다.

## Aggregate Gate Closure Report

ADR-0004 current decision 적용 근거는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md` Markdown closure report다.
후속 decision 변경도 같은 형식의 closure report가 `aggregate_gate_status = closed`일 때만 current decision을 바꿀 수 있다.

Closure report는 Markdown record이며 machine-readable JSON은 만들지 않는다.
필수 field는 `report_id`, `created_at`, `source_commit_sha`, `route_matrix_commit_sha`, `ga_scope_current_route_count`, `ga_scope_product_operation_count`, `future_route_exclusion_count`, `transition_helper_count`, `blocked_count`, `powershell_current_owner_count`, `powershell_fallback_count`, `active_spikes_path_count`, required evidence status fields, stale/waived/waiver-only counts, `aggregate_gate_status`다.
`aggregate_gate_status` allowed values는 `open`, `closed`, `blocked`다.

`aggregate_gate_status = closed`가 되려면 `transition_helper_count`, `blocked_count`, `powershell_current_owner_count`, `powershell_fallback_count`, `active_spikes_path_count`, `stale_evidence_count`, `waiver_only_gate_satisfaction_count`가 모두 `0`이어야 하며 required status field가 모두 `pass`여야 한다.
`aggregate_gate_status = blocked`는 GA-scope row가 blocked 상태로 남아 있거나, 금지된 PowerShell/product fallback/active spikes path/stale evidence/waiver-only gate satisfaction이 하나라도 있을 때 사용한다.
그 외 미실행 또는 미완료 상태는 `aggregate_gate_status = open`으로 둔다.

## Evidence Ledger

Evidence ledger 위치는 `docs/ga-ready/evidence/`다.
현재 ledger는 `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`이며, 2026-05-05 closure와 stable internal release/update/rollback evidence가 current decision supporting evidence를 닫는다.
Ledger는 Markdown evidence ledger이며 machine-readable JSON은 만들지 않는다.

각 evidence record는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Description |
|---|---:|---|
| `evidence_id` | yes | evidence record stable id |
| `route_or_operation` | yes | route matrix `Route/Operation` cell exact value |
| `route_surface` | yes | route matrix `route_surface` value |
| `risk_tier` | yes | route matrix `risk_tier` value |
| `current_owner` | yes | route matrix `current_owner` value |
| `commit_sha` | yes | implementation/evidence commit SHA |
| `artifact_or_package_version` | yes | artifact or package version under test |
| `target_owner` | yes | route matrix `target_owner` value |
| `implementation_basis` | yes | route matrix `implementation_basis` value |
| `fallback_policy` | yes | route matrix `fallback_policy` value |
| `promotion_state` | yes | route matrix `promotion_state` value |
| `admin_smoke_required` | yes | route matrix `admin_smoke_required` value |
| `release_gate` | yes | route matrix `release_gate` value |
| `network_exposure_gate` | yes | route matrix `network_exposure_gate` value |
| `runner_version` | yes | smoke/test runner version |
| `host_capability_snapshot` | yes | host capability snapshot reference or summary |
| `exact_command_mode` | yes | exact command mode used for the evidence |
| `result` | yes | `pass`, `fail`, `blocked`, `not-run` |
| `created_at` | yes | evidence creation timestamp |
| `stale_triggers` | yes | freshness triggers that would stale this record |
| `waiver_status` | yes | `none`, `requested`, `approved`, `rejected`, `expired` |

Evidence 기록 이후 current owner, target owner, implementation basis, fallback policy, promotion state, admin smoke requirement, release gate, network exposure gate, package contract, service host, installer custom action, route matrix gate가 변경되면 해당 evidence는 stale로 간주한다.
Stale evidence는 historical context로만 남기며 aggregate GA-ready gate 충족에 사용할 수 없다.
Waiver는 특정 stale evidence record를 제한적으로 대체하는 예외이며 aggregate GA-ready gate 자체를 통과시키는 용도가 아니다.
Waiver-only gate satisfaction is forbidden for `tier3-destructive-or-persistent`, `release_gate = release-approval-required`, trust-store, and firewall LAN exposure rows.

## Repo Migration

목표 layout은 다음과 같다.

```text
src/
  DesktopNode.Host/
  DesktopNode.Api/
  DesktopNode.Runtime/
  DesktopNode.Contracts/
  DesktopNode.HyperV/
  DesktopNode.Service/
web/
  package.json
  src/
  tests/
packaging/
  windows-desktop-node/
docs/
  adr/
  superpowers/
archive/
  spikes/
```

Migration rule:

- 제품 runtime source는 `src/**` 또는 `web/**`에 둔다.
- `spikes/**`는 historical/archive baseline으로 축소한다.
- 경로 이동은 behavior 변경과 분리한다.
- 문서 링크와 검증 command는 migration map에서 함께 갱신한다.
- `packaging/windows-desktop-node/**`의 product root/source root contract는 migration slice마다 검증한다.

초기 migration map은 active product target과 archive target을 분리해 작성한다.

| Current path | Active product target | Archive target | Migration condition |
|---|---|---|---|
| `spikes/purecvisor-desktop-node/api/**` | `src/DesktopNode.Api/**` | `archive/spikes/api/**` | route matrix owner가 .NET으로 이동한 뒤 archive |
| `spikes/purecvisor-desktop-node/hyperv/**` | `src/DesktopNode.HyperV/**` | `archive/spikes/hyperv/**` | C# WMI/CIM parity route가 promoted 된 뒤 archive |
| `spikes/purecvisor-desktop-node/service/**` | `src/DesktopNode.Service/**` | `archive/spikes/service/**` | product service-action이 PowerShell-free가 된 뒤 archive |
| `spikes/purecvisor-desktop-node/cli/**` | 없음 | `archive/spikes/cli/**` | GA-ready target에는 포함하지 않고, 제품 CLI가 필요하면 별도 `src/DesktopNode.Cli/**` 설계로 추가 |
| `spikes/purecvisor-desktop-node/web/**` | `web/**` | `archive/spikes/web/**` | 2026-05-03 served asset/root migration slice에서 active product target으로 이동됨 |
| `packaging/windows-desktop-node/**` | `packaging/windows-desktop-node/**` | 없음 | PowerShell orchestration 제거 slice별 갱신 |

## Verification Strategy

GA-ready 검증 체계는 제품 owner와 같은 방향으로 정리한다.

- .NET product path: xUnit과 installed smoke contract
- Web Console: npm, TypeScript typecheck, 후속 browser-level fixture 후보
- Packaging: installer/package contract tests와 signed/unsigned channel policy tests
- Operator smoke: no-auto-reboot, install/repair/uninstall/remove-data, update/rollback, diagnostics bundle, cleanup evidence
- Legacy baseline: Pester는 archive compatibility verification으로 축소
- Playwright는 후속 도구 후보이며 이 설계 승인만으로 required dependency가 되지 않는다.
- 첫 browser-level fixture 후보는 Web Console package가 소유하는 loopback fixture로 좁히며 static asset load, initial render, deterministic `GET /api/v1/runtime/policy` connection, optional bearer 401/200 handling, token/redaction 확인을 최소 범위로 둔다.
- Browser-level fixture 후보는 normal dashboard, VM empty, job empty, unauthorized, Local API offline/error, API error, LAN mode warning 상태의 user-visible copy와 stable DOM marker를 검증 대상으로 삼는다.
- Browser-level fixture 후보는 keyboard reachability, visible focus, status text fallback, token/redaction 확인을 UI trust boundary로 다룬다.
- Browser-level fixture 후보는 API route contract, route parity, Hyper-V, service/MSI/firewall/Event Log/trust store mutation, LAN exposure를 검증하지 않는다.

초기 verification ownership map은 다음 기준이다.

| Area | Current verification | Target verification | Transition rule |
|---|---|---|---|
| API contract | Pester + xUnit | xUnit + installed route smoke | route owner가 .NET이면 xUnit이 primary |
| Hyper-V read routes | xUnit + installed non-mutating smoke + Pester archive compatibility | xUnit adapter tests + installed non-mutating smoke | read-route fallback removal 이후 xUnit이 product path primary |
| Hyper-V mutation routes | Pester + admin opt-in | xUnit job tests + admin opt-in route smoke | destructive operation은 explicit opt-in 유지 |
| Web Console | Pester static tests + npm parity | npm + TypeScript + 후속 browser-level fixture 후보 | served build output 전환 전까지 parity 유지 |
| Packaging/MSI | Pester installer tests | package contract tests + installed lifecycle smoke | PowerShell 제거 slice마다 package tests 갱신 |
| Release/signing | Pester/build script checks | channel/provenance/signing contract + signed lifecycle evidence | public/internal trust model 구분 유지 |

## GA Gate와 Release Gate 분리

이 설계는 GA-ready product runtime 판단과 release execution을 분리한다.
GA-ready product runtime은 제품 runtime/ops/repo/test architecture가 GA 가능한 형태인지 판단한다.
Release execution은 selected trust model, signed stable MSI lifecycle, update/rollback compatibility, publication target, release notes를 별도 승인한다.

이 설계는 gate를 네 단계로 나눈다.

### Phase 26 gate

- ADR-0001 supersede 범위를 작성한다.
- route promotion matrix를 문서와 contract test로 고정한다.
- repo migration map을 작성한다.
- verification ownership map을 작성한다.
- 실제 route 구현 변경과 `spikes/**` 이동은 하지 않는다.

### Pre-GA gate

- 제품 runtime path가 PowerShell-free가 된다.
- 제품 배포/운영 path가 PowerShell-free가 된다.
- `spikes/**`가 archive/remove 상태가 된다.
- TypeScript Web Console build output이 Local API static serving 기본값이 된다.
- 제품 검증 primary가 xUnit/npm/browser-level fixture 후보/package contract로 이동한다.
- admin-smoke evidence는 no-auto-reboot와 cleanup을 계속 포함한다.

### GA-ready gate

- Aggregate gate closure report가 `aggregate_gate_status = closed`다.
- PowerShell helper, active `spikes/**`, stale evidence, waiver-only gate satisfaction이 aggregate closure에 남아 있지 않다.
- release-gated row는 pre-release evidence로 `blocked`를 해소할 수 있지만 release execution으로 간주하지 않는다.
- LAN exposure gated row는 pre-LAN evidence로 `blocked`를 해소할 수 있지만 firewall execution으로 간주하지 않는다.
- diagnostics/operator runbook이 제품 구조와 일치한다.

### Release execution gate

- signed stable MSI lifecycle evidence가 있다.
- 승인된 internal trust model이 명확하다. Public trusted signing은 내부 전용 서비스 scope 밖이다.
- update/rollback compatibility evidence가 있다.
- release notes, publication target, support boundary가 확정된다.
- stable publication, public trusted signing execution, external release, signed update/rollback 실행은 별도 release approval 전까지 금지한다.

## 첫 구현 Slice

첫 구현 slice는 설계/계약 정렬만 수행한다.

1. 이 설계 문서를 추가한다.
2. ADR-0001 supersede 범위를 정의한다.
3. route promotion matrix 초기 표를 문서화한다.
4. repo migration map 초기 표를 문서화한다.
5. verification ownership map 초기 표를 문서화한다.

첫 slice에서 하지 않는 일:

- PowerShell helper 제거
- route 구현 변경
- `spikes/**` 파일 이동
- 실제 evidence ledger 또는 aggregate closure report 파일 생성
- machine-readable JSON 생성
- stable release 발행
- MSI install/repair/uninstall 같은 관리자 mutation

첫 구현 plan은 route matrix를 먼저 문서와 테스트 contract로 고정한 뒤, 다음 route로 `vm.list` 같은 Tier 1 read-only route를 선택했다.

## 비목표

- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime은 대상이 아니다.
- C++23 runtime 전환은 이 설계의 대상이 아니다.
- external stable publication 실행은 이 설계의 대상이 아니다.
- Playwright 또는 다른 browser automation tool을 required dependency로 도입하는 일은 이 설계의 대상이 아니다.
- 기본 검증에서 Hyper-V mutation, service install/start/stop/delete, firewall/Event Log/trust store 변경을 실행하지 않는다.
- 현재 적용 ADR을 별도 closure/approval 없이 변경하지 않는다.

## 설계 검증 기준

이 문서 또는 관련 ADR만 변경하는 경우:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

후속 구현 plan이 route matrix contract test를 추가하면 해당 plan에서 `dotnet test src/DesktopNode.sln`과 관련 package/web 검증을 추가한다.

## 완료 기준

이 설계가 승인되면 다음 상태를 완료로 본다.

- `ga-ready-product-runtime` 목표가 문서화되어 있다.
- PowerShell-free product ops/runtime 목표가 명시되어 있다.
- route promotion matrix의 field schema, enum, state invariant, detailed matrix 분리 기준이 있다.
- `spikes/**` 제거/archive target과 migration rule이 있다.
- 제품 검증 primary를 xUnit/npm/browser-level fixture 후보/package contract로 옮기는 방향이 있다.
- Web Console은 기존 Phase 3A/P2/Phase 25 design basis를 재사용하고, normal/empty/error/unauthorized/LAN warning state와 visual/a11y guard를 가진다.
- Phase 26, Pre-GA, GA-ready, release execution gate가 분리되어 있다.
- ADR-0004 aggregate closure report와 ADR current decision procedure가 정의되어 있다.

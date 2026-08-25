# GA-ready Route 승격 매트릭스

이 문서는 Desktop Node API route와 product operation별 current owner, target owner, implementation basis, fallback policy, promotion state, GA-ready gate, release gate, network exposure gate를 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`과 `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`다. 이 matrix는 ADR-0004 적용 이후 내부 전용 서비스 제품 런타임의 route/product operation ownership contract다.

## Field Schema

이 섹션명과 field enum은 문서 guard와 evidence ledger가 그대로 참조하므로 영문 앵커를 유지한다.

| Field | Required | Allowed values |
|---|---:|---|
| `route` | yes | HTTP route path or product operation name |
| `route_surface` | yes | `current-route`, `future-route`, `product-operation` |
| `domain` | yes | `host-read`, `network-read`, `vm-read`, `vm-lifecycle`, `checkpoint-lifecycle`, `job-runtime`, `auth-runtime`, `console-access`, `product-ops`, `operating-system-ops` |
| `risk_tier` | yes | `tier1-read-only`, `tier2-reversible-mutation`, `tier3-destructive-or-persistent` |
| `current_owner` | yes | `dotnet-native`, `product-wrapper`, `dotnet-runtime`, `dotnet-host-listener`, `mixed-history`, `not-yet-defined`, `not-implemented` |
| `target_owner` | yes | `dotnet-host-adapter`, `dotnet-hyperv-adapter`, `dotnet-runtime`, `dotnet-host-listener`, `dotnet-job-store-migration-action`, `dotnet-service-action`, `dotnet-token-storage-action`, `dotnet-config-migration-action`, `dotnet-data-root-action`, `windows-native-package`, `windows-eventlog-action`, `windows-firewall-action`, `windows-trust-store-action` |
| `implementation_basis` | yes | `registry-wmi-service`, `wmi-cim`, `dotnet-runtime`, `websocket-to-vnc-tcp-bridge`, `product-config-migration-plan`, `job-store-migration-plan`, `windows-native-api`, `dpapi-local-machine-token-plan`, `windows-certificate-store-api`, `eventlog-registration-plan`, `firewall-rule-plan`, `data-root-lifecycle-plan`, `package-contract`, `approved-system-executable` |
| `fallback_policy` | yes | `none`, `test-only`, `transition-helper`, `blocked` |
| `promotion_state` | yes | `current-native`, `transition-helper`, `blocked`, `ga-ready-candidate` |
| `admin_smoke_required` | yes | `none`, `installed-non-mutating`, `explicit-admin-opt-in` |
| `ga_ready_gate` | yes | concise Korean gate text |
| `release_gate` | yes | `none`, `release-approval-required` |
| `network_exposure_gate` | yes | `none`, `lan-exposure-approval-required` |

## Fallback Policy

- `none`: product fallback을 사용하지 않는다.
- `test-only`: fixture 또는 injectable test fallback은 허용하지만 product fallback은 사용하지 않는다.
- `transition-helper`: historical pre-promotion marker only. ADR-0004 적용 이후 active product row에 다시 쓰면 gate blocker다.
- `blocked`: target owner parity가 생기기 전까지 route가 GA-ready blocker다.

## Promotion State

- `current-native`: 현재 product row가 product PowerShell fallback 없이 이미 구현돼 있다.
- `transition-helper`: historical pre-promotion marker only. ADR-0004 적용 이후 active product row에 다시 쓰면 gate blocker다.
- `blocked`: target owner implementation과 evidence가 있기 전까지 현재 product row를 승격할 수 없다.
- `ga-ready-candidate`: 현재 product row에 target owner evidence가 있으며 review 후 승격할 수 있다.

## State Invariants

- `promotion_state = current-native`는 `fallback_policy = none` 또는 `fallback_policy = test-only`만 허용한다.
- `promotion_state = transition-helper`는 `fallback_policy = transition-helper`를 요구한다.
- `promotion_state = blocked`는 `fallback_policy = blocked`를 요구한다.
- `promotion_state = ga-ready-candidate`는 `fallback_policy = none` 또는 `fallback_policy = test-only`만 허용한다.
- `risk_tier = tier1-read-only`는 `admin_smoke_required = none` 또는 `admin_smoke_required = installed-non-mutating`만 허용한다.
- `risk_tier = tier2-reversible-mutation`는 `admin_smoke_required = explicit-admin-opt-in`을 요구한다.
- `risk_tier = tier3-destructive-or-persistent`는 `admin_smoke_required = explicit-admin-opt-in`을 요구한다.

## Aggregate GA-ready Decision Gate

ADR-0004를 current decision으로 승격하기 전에는 route matrix와 supporting docs 기준으로 다음 aggregate gate가 닫혀야 했다.

- GA 범위의 `current-route`와 `product-operation` row는 `promotion_state = transition-helper` 또는 `promotion_state = blocked`가 0개여야 한다.
- `future-route` row는 GA 범위 제외 사유와 별도 implementation plan requirement를 명시해야 한다.
- 제품 runtime/request path에는 PowerShell helper가 없어야 한다.
- 활성 제품 경로에는 `spikes/**`가 없어야 한다.
- repo migration preflight evidence와 verification ownership replacement evidence가 완료되어야 한다.
- `tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row는 explicit admin opt-in evidence가 완료되어야 하며, Evidence Freshness Rule을 만족하지 않는 stale evidence는 aggregate GA-ready gate 충족에 사용할 수 없다.
- `release_gate = release-approval-required` row는 GA-ready 판정과 release execution을 분리하며, 별도 release approval 전에는 실행하지 않는다. 2026-05-05 internal stable release/update/rollback execution은 사용자 opt-in 범위의 내부 신뢰 evidence이며 public trusted signing 또는 외부 stable publication claim이 아니다.

이 aggregate gate는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`에서 closed로 닫혔고, ADR-0004는 내부 전용 서비스 current decision으로 적용됐다.

## GA Scope Classification Rule

- `route_surface = current-route`와 `route_surface = product-operation` row는 기본적으로 GA-scope다.
- `route_surface = future-route` row만 GA-scope에서 제외할 수 있으며, 제외 사유와 별도 implementation plan requirement를 기록해야 한다.
- `release_gate = release-approval-required`와 `network_exposure_gate = lan-exposure-approval-required`는 GA-scope 제외 사유가 아니며, execution approval 또는 exposure approval 분리만 의미한다.
- `current-route` 또는 `product-operation` row를 GA-scope 밖으로 빼려면 별도 ADR/task approval로 제품 범위를 줄여야 하며, 그 전에는 aggregate GA-ready gate closure로 계산할 수 없다.

## PowerShell-Free Product Path Closure Rule

- GA-scope `current-route` 또는 `product-operation` row는 product runtime/request/admin execution path에서 PowerShell helper를 사용하지 않아야 aggregate GA-ready gate closure로 계산할 수 있다.
- historical `current_owner = powershell-helper` 또는 `current_owner = dotnet-request-processor-powershell-helper` row는 ADR-0004 적용 이후 active product matrix allowed value가 아니다. 다시 등장하면 target owner evidence가 있더라도 aggregate GA-ready gate closure로 계산할 수 없다.
- `fallback_policy = transition-helper` row는 helper fallback 제거 evidence가 있기 전까지 aggregate GA-ready gate closure로 계산할 수 없다.
- `fallback_policy = test-only`는 fixture or injectable test fallback에만 허용하며 product execution path fallback으로 사용할 수 없다.

## Active Product Path Classification Rule

- `spikes/**` path가 runtime/service/API/CLI/Web Console execution, packaging input, installer input, static asset source, generated parity manifest, required verification command, CI/local verification command, or developer command documentation에 남아 있으면 active product path로 간주한다.
- `archive/spikes/**` reference는 historical/read-only baseline intent일 때만 허용하며 product execution, packaging, required verification source로 사용할 수 없다.
- `docs/**` command가 `spikes/**`를 required product path로 실행하도록 안내하면 active product path로 간주한다.
- README/AGENTS/DEVELOPER_INDEX/follower/PUBLIC_RELEASE_BOUNDARY의 component/archive entry point link는 product execution, packaging input, required verification command, CI/local command가 아니면 active product path로 계산하지 않는다.
- Aggregate GA-ready gate closure에는 `spikes/**` active product path가 0개라는 repo migration preflight evidence와 docs command update evidence가 필요하다.

## Aggregate Gate Closure Report Contract

Closure report 위치는 `docs/ga-ready/evidence/aggregate-gate-closure-<YYYY-MM-DD>.md`다.
현재 ADR-0004 적용 근거는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`다.
Closure report는 Markdown record이며 machine-readable JSON은 만들지 않는다.

각 closure report는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Allowed values | Description |
|---|---:|---|---|
| `report_id` | yes | non-empty string | closure report stable id |
| `created_at` | yes | ISO-8601 timestamp | report creation time |
| `source_commit_sha` | yes | full 40-char SHA or minimum 12-char abbreviated SHA | source tree under review |
| `route_matrix_commit_sha` | yes | full 40-char SHA or minimum 12-char abbreviated SHA | route matrix version used for counts |
| `ga_scope_current_route_count` | yes | integer >= 0 | GA-scope `current-route` row count |
| `ga_scope_product_operation_count` | yes | integer >= 0 | GA-scope `product-operation` row count |
| `future_route_exclusion_count` | yes | integer >= 0 | `future-route` rows excluded with reason and implementation plan requirement |
| `transition_helper_count` | yes | integer >= 0 | GA-scope rows with `promotion_state = transition-helper` |
| `blocked_count` | yes | integer >= 0 | GA-scope rows with `promotion_state = blocked` |
| `powershell_current_owner_count` | yes | integer >= 0 | GA-scope rows with PowerShell-backed current owner |
| `powershell_fallback_count` | yes | integer >= 0 | GA-scope rows with product execution `fallback_policy = transition-helper` |
| `active_spikes_path_count` | yes | integer >= 0 | active product path references under `spikes/**` |
| `component_archive_spikes_reference_count` | no | integer >= 0 | component/archive documentation references under `spikes/**`; not counted as active product path |
| `repo_migration_preflight_status` | yes | `pass`, `fail`, `blocked`, `not-run` | repo migration preflight evidence status |
| `docs_command_update_status` | yes | `pass`, `fail`, `blocked`, `not-run` | docs command update evidence status |
| `verification_ownership_replacement_status` | yes | `pass`, `fail`, `blocked`, `not-run` | replacement verification owner evidence status |
| `archive_readonly_rollback_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | archive read-only rollback/inventory evidence status |
| `tier2_admin_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | tier2 explicit admin opt-in evidence status |
| `tier3_admin_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | tier3 explicit admin opt-in evidence status |
| `release_gated_prerelease_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | release-gated pre-release evidence status |
| `lan_gated_preapproval_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | LAN-gated pre-approval evidence status |
| `stale_evidence_count` | yes | integer >= 0 | stale evidence records used by neither rerun nor approved limited waiver |
| `waived_evidence_count` | yes | integer >= 0 | approved limited waiver count |
| `waiver_only_gate_satisfaction_count` | yes | integer >= 0 | rows attempting to satisfy a gate by waiver alone |
| `aggregate_gate_status` | yes | `open`, `closed`, `blocked` | final aggregate gate state |

`aggregate_gate_status = closed`가 되려면 `transition_helper_count`, `blocked_count`, `powershell_current_owner_count`, `powershell_fallback_count`, `active_spikes_path_count`, `stale_evidence_count`, `waiver_only_gate_satisfaction_count`가 모두 `0`이어야 하며 required status field가 모두 `pass`여야 한다.
`aggregate_gate_status = blocked`는 GA-scope row가 blocked 상태로 남아 있거나, 금지된 PowerShell/product fallback/active product spikes path/stale evidence/waiver-only gate satisfaction이 하나라도 있을 때 사용한다.
그 외 미실행 또는 미완료 상태는 `aggregate_gate_status = open`으로 둔다.

## ADR Promotion Procedure Rule

- ADR-0004는 `aggregate_gate_status = closed` closure report를 근거로 current decision으로 적용됐다.
- 적용 diff는 ADR-0004 상태, `docs/ADR_INDEX.md` 현재 적용 중인 ADR 표, 결정 마커, 제안 중인 ADR 후보 섹션을 같은 diff에서 갱신해야 한다.
- 적용 후 `PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 ADR-0004 하나만 남아야 한다.
- ADR-0001의 `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`와 ADR-0003의 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`는 별도 ADR이 바꾸기 전까지 현재 적용 결정으로 보존해야 한다.
- Public trusted signing, external stable publication, 일반 사용자 public release를 내부 전용 서비스 scope에 포함하려면 별도 ADR이 필요하다.

## Evidence Freshness Rule

`tier2-reversible-mutation` 또는 `tier3-destructive-or-persistent` row의 explicit admin opt-in evidence는 다음 scope를 기록해야 aggregate GA-ready gate를 충족할 수 있다.

- commit SHA
- artifact/package version
- route/operation row id
- current owner
- target owner
- implementation basis
- fallback policy
- promotion state
- admin smoke requirement
- release gate
- network exposure gate
- runner version
- host capability snapshot
- exact command mode

Evidence 기록 이후 current owner, target owner, implementation basis, fallback policy, promotion state, admin smoke requirement, release gate, network exposure gate, package contract, service host, installer custom action, route matrix gate가 변경되면 해당 evidence는 stale로 간주한다.
Stale evidence는 historical context로만 남기며 aggregate GA-ready gate 충족에 사용할 수 없다.
Stale evidence는 rerun하거나 별도 approval waiver로만 다시 gate 충족 근거가 될 수 있다.

## Evidence Ledger Contract

Evidence ledger 위치는 `docs/ga-ready/evidence/`다.
현재 GA-ready evidence ledger는 `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`이며, 2026-05-05 closure와 stable internal release/update/rollback evidence가 current decision supporting evidence를 닫는다.
Ledger는 Markdown evidence ledger이며 machine-readable JSON은 만들지 않는다.

각 evidence record는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Description |
|---|---:|---|
| `evidence_id` | yes | evidence record stable id |
| `route_or_operation` | yes | route path or product operation name |
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
| `result` | yes | pass/fail/blocked result summary |
| `created_at` | yes | evidence creation timestamp |
| `stale_triggers` | yes | freshness triggers that would stale this record |
| `waiver_status` | yes | none/requested/approved and approval reference |

## Evidence Row Identity Rule

- `route_or_operation`은 route matrix의 `Route/Operation` cell과 정확히 일치해야 하며 evidence row identity로 사용한다.
- 같은 `route_or_operation` 값을 가진 duplicate matrix row는 허용하지 않는다.
- route path, operation name, route_surface, current_owner, target_owner, implementation_basis, fallback_policy, promotion_state, admin_smoke_required, release_gate, network_exposure_gate 중 하나가 바뀌면 기존 evidence는 stale로 간주한다.
- Evidence ledger는 rename 전후 row를 같은 evidence로 병합하지 않는다. Rename 후에는 새 `route_or_operation`에 대해 rerun evidence 또는 별도 approval waiver가 필요하다.

## Evidence Waiver Policy

Waiver는 aggregate GA-ready gate 자체를 통과시키는 용도가 아니다.
Waiver는 특정 stale evidence record를 제한적으로 대체하는 예외이며, waiver가 있더라도 row의 target owner, implementation basis, risk tier, release gate, network exposure gate는 낮출 수 없다.

각 waiver record는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Description |
|---|---:|---|
| `waiver_id` | yes | waiver stable id |
| `evidence_id` | yes | waived stale evidence record id |
| `scope` | yes | route/operation and condition covered by the waiver |
| `reason` | yes | reason rerun evidence is not available |
| `risk_acceptance_owner` | yes | named owner accepting residual risk |
| `expires_at` | yes | waiver expiry timestamp or milestone |
| `replacement_evidence_required` | yes | replacement evidence requirement |
| `approval_reference` | yes | ADR/task/review approval reference |

Waiver-only gate satisfaction is forbidden for `tier3-destructive-or-persistent`, `release_gate = release-approval-required`, trust-store, and firewall LAN exposure rows.
Those rows require rerun evidence, even if a waiver exists.

## Evidence Field Format and Enum Rule

Evidence ledger와 waiver record field는 다음 format과 enum rule을 따른다.

- `route_surface`, `risk_tier`, `current_owner`, `target_owner`, `implementation_basis`, `fallback_policy`, `promotion_state`, `admin_smoke_required`, `release_gate`, `network_exposure_gate`는 route matrix Field Schema enum을 그대로 재사용한다.
- `result` allowed values는 `pass`, `fail`, `blocked`, `not-run`이다.
- `waiver_status` allowed values는 `none`, `requested`, `approved`, `rejected`, `expired`다.
- `commit_sha`는 full 40-char SHA를 우선 사용하며, 최소 12-char abbreviated SHA를 허용한다.
- `created_at`과 `expires_at`은 ISO-8601 timestamp 또는 명시적 milestone reference만 허용한다.
- `scope`, `reason`, `host_capability_snapshot`, `approval_reference`는 자유 텍스트지만 비워둘 수 없다.

## Route Surface Invariants

- `route_surface = current-route`는 현재 구현된 Local API route에만 사용한다.
- `route_surface = future-route`는 현재 제품에 served route 또는 실행 product operation으로 존재하지 않는 future implementation exclusion row에만 사용한다.
- `route_surface = future-route` row는 반드시 `current_owner = not-implemented`, `fallback_policy = blocked`, `promotion_state = blocked`여야 한다.
- `route_surface = product-operation`은 HTTP API route가 아닌 product operation row에만 사용한다.

## Served Route Scope Rule

- `route_surface = current-route`는 실제 served Local API route만 의미한다.
- side-by-side contract-only route 후보는 실제 request processor 또는 PowerShell Local API available routes에 등록되기 전까지 matrix row로 추가하지 않는다.
- `GET /api/v1/jobs`는 현재 served route이며 pagination metadata와 terminal-job retention summary를 가진 read-only job list row로 표현한다.
- Job runtime read surface는 `GET /api/v1/jobs`와 `GET /api/v1/jobs/{job_id}` row로 표현한다.
- Contract mirror aggregate route 후보인 `POST /api/v1/vms/{vmId}/lifecycle/{action}`는 실제 served route가 아니므로 matrix row로 추가하지 않는다.
- VM lifecycle served surface는 현재 `POST /api/v1/vms/{id}/start`, `shutdown`, `poweroff`, `restart`, `DELETE /api/v1/vms/{id}` 개별 row로만 표현한다.

## Future Route Execution Guard

- `route_surface = future-route` row는 Phase 26 alignment slice에서 구현하거나 실제 Local API route/product operation으로 등록하지 않는다.
- `future-route` row를 `current-route` 또는 `product-operation`으로 변경하려면 별도 future implementation plan이 먼저 route/operation contract, not-found/idempotency contract, destructive cleanup proof, explicit admin opt-in evidence requirement를 정의해야 한다.
- `future-route` row는 위 evidence가 승인되기 전까지 `current_owner = not-implemented`, `fallback_policy = blocked`, `promotion_state = blocked`를 유지한다.

## Native-First Helper Fallback Rule

- `GET /api/v1/network/inventory`, `GET /api/v1/vms`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints` row는 현재 구현처럼 `current_owner = dotnet-native`로 기록한다.
- 2026-05-03 read-route helper fallback removal slice 이후 위 Tier 1 read rows는 product request path에서 PowerShell helper fallback을 사용하지 않는다.
- 위 row들의 `fallback_policy = none`과 `promotion_state = current-native`는 native adapter가 성공 또는 구조적 실패를 직접 반환한다는 뜻이다.
- `network.inventory` row는 topology parity가 불완전할 때 `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`를 반환하며 PowerShell helper로 재시도하지 않는다.
- `vm.list` row는 helper JSON contract의 VM identity/state, CPU/startup memory/generation/checkpoint count minimum summary parity, storage/network field parity를 native WMI/CIM path에서 보존한다. Empty provider result는 유효한 empty inventory success이고, incomplete parity는 native structured failure로 반환한다.
- `GET /api/v1/vms/{id}` row는 native `vm.list` result에서 detail을 찾는다. Native inventory에서 VM이 없으면 helper 재시도 없이 `PCV_VM_NOT_FOUND`를 반환하고, native inventory가 구조적 실패면 그 실패를 그대로 반환한다.
- `GET /api/v1/vms/{id}/checkpoints` row는 native VM inventory guard와 WMI snapshot association을 사용한다. Empty checkpoint list는 유효한 native success이며, VM inventory/checkpoint parity failure는 helper fallback 없이 native structured failure로 반환한다.
- `DELETE /api/v1/vms/{id}` row는 .NET request processor queue를 유지하되 job execution에서 C# WMI `DestroySystem` adapter를 직접 호출한다. Missing VM은 idempotent `action=absent` success로 반환하고, `managed-by=purecvisor-desktop-node` marker가 없는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. Evidence는 code-level xUnit과 `0.30.1-admin-smoke` installed destructive smoke다.
- 2026-05-03 checkpoint mutation native adapter slices 이후 `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`는 .NET request processor queue를 유지하되 job execution에서 C# WMI snapshot service adapter를 직접 호출한다.
- 2026-05-03 VM power-state native adapter slice 이후 `POST /api/v1/vms/{id}/start`와 `POST /api/v1/vms/{id}/poweroff`는 .NET request processor queue를 유지하되 job execution에서 C# WMI `Msvm_ComputerSystem.RequestStateChange` adapter를 직접 호출한다.
- 후속 VM create/shutdown/restart native adapter slice 이후 `POST /api/v1/vms`, `POST /api/v1/vms/{id}/shutdown`, `POST /api/v1/vms/{id}/restart`도 .NET request processor queue를 유지하되 job execution에서 C# WMI adapter를 직접 호출한다.
- VM create native product path는 이 slice에서 Hyper-V Generation 2만 지원한다. Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환하며 별도 generation 1 parity plan 없이는 GA gate 충족 근거로 계산하지 않는다.
- 위 current Hyper-V read/mutation rows의 `fallback_policy = none`과 `promotion_state = current-native`는 native adapter가 성공 또는 구조적 실패를 직접 반환하고 product request path에서 PowerShell helper로 재시도하지 않는다는 뜻이다.

## 현재 Code-level 경계 Evidence

2026-05-12 후속 split evidence는 `docs/ga-ready/evidence/runtime-host-hyperv-domain-followup-code-level-2026-05-12.md`와 `docs/ga-ready/evidence/runtime-hyperv-operator-followup-code-level-2026-05-12.md`가 소유한다.

- Runtime/Core route dispatch는 `DesktopNodeApiAuthSessionHandler`, `DesktopNodeApiJobRuntimeHandler`, `DesktopNodeApiDiagnosticsHandler`로 auth/session, jobs, diagnostics 계열을 먼저 분리한다.
- Runtime/Core 잔여 operator surface dispatch는 `DesktopNodeApiConsoleHandler`, `DesktopNodeApiOpsSummaryHandler`로 console과 ops-summary route family를 분리한다.
- Host Ops catalog는 `config-migration`, `job-store-migration`, `service-token`을 별도 owner로 고정한다. `service-token-rotation-revoke`는 더 이상 service lifecycle family로 분류하지 않는다.
- Hyper-V domain은 `DesktopNodeHyperVWmiProviderCatalog`로 `host-status-provider`, `switch-provider`, `vm-provider`, `checkpoint-provider`, `vm-create-provider`, `vm-power-state-provider`, `vm-delete-provider`, `checkpoint-mutation-provider`와 실제 implementation type 및 provider별 파일 경계를 연결한다.
- 이 evidence는 code-level boundary split만 주장하며 installed listener rerun, MSI apply, Hyper-V 실제 VM mutation, OS mutation, public trusted signing, external stable publication을 주장하지 않는다.

## Job Runtime Risk Inheritance Rule

- `POST /api/v1/jobs/{job_id}/retry` row는 retry state transition의 route owner를 나타낸다.
- retry로 다시 queued 되는 underlying operation은 원본 job operation의 `risk_tier`, `admin_smoke_required`, cleanup evidence를 상속한다.
- 원본 operation이 `tier2-reversible-mutation` 또는 `tier3-destructive-or-persistent`이면 retry 실행/검증은 기본 non-mutating verification에 포함하지 않고 `explicit-admin-opt-in` evidence에서만 다룬다.
- retry route는 원본 operation의 GA-ready gate, release gate, network exposure gate를 낮추거나 우회할 수 없다.

## Job Route Parameter Rule

- Job route path parameter는 `job_id`로 통일한다.
- `id`와 `jobId`는 code variable 또는 internal compatibility name으로만 다루며 route identity parameter로 사용하지 않는다.

## VM Route Parameter Rule

- VM route path parameter는 기존 served API 계약인 `id`를 유지한다.
- VM route `id`는 VM `id` 또는 `name` lookup key를 의미한다.
- `vmId`는 code variable 또는 internal compatibility name으로만 다루며 route identity parameter로 사용하지 않는다.
- `vm_id`로 바꾸는 것은 이 alignment slice 범위가 아니다. 별도 API route contract migration이 없으면 matrix row에 추가하지 않는다.

## Checkpoint Route Parameter Rule

- Checkpoint route path parameter는 `checkpoint_id`로 통일한다.
- `name`과 `checkpoint_name`은 request body/helper compatibility alias로만 다루며 route identity parameter로 사용하지 않는다.
- `name`/`checkpoint_name` request body alias는 checkpoint create에서 계속 허용하되 native adapter 내부 canonical param은 `checkpoint_name`이다.

## Current Owner Invariants

- `current_owner = not-yet-defined`은 더 이상 GA-scope row에 허용하지 않는다.
- `product config migration apply` row는 2026-05-06 이후 `DesktopNode.Host.exe service-action config-migration-apply` actual write/backup operation을 갖고, 2026-05-07 `0.38.6-admin-smoke` installed destructive admin smoke PASS 이후 `current-native`로 다룬다.
- `job store migration apply` row는 2026-05-06 이후 `DesktopNode.Host.exe service-action job-store-migration-apply` actual write/backup operation을 갖고, 2026-05-07 `0.38.6-admin-smoke` installed destructive admin smoke PASS 이후 `current-native`로 다룬다.
- `current_owner = not-implemented`는 `route_surface = future-route` row에만 허용한다.

## Current Owner Resolution Rule

- `not-yet-defined`은 이전 alignment plan에서만 허용했던 임시 계획 상태이며, current matrix에는 남기지 않는다.
- `product config migration apply`의 destructive write path는 current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, migration plan id/version, service stopped precondition, backup/rollback evidence 없이 실행할 수 없다. 현재 code-level action은 missing evidence를 config write 없이 blocked diagnostics로 반환하고, supported `product-config-v1-to-v2` plan/version 1에서만 write를 수행한다.

## Mixed History Resolution Rule

- `mixed-history`은 service product operation row에만 허용한다.
- `mixed-history` row는 wrapper, installer, service host 이력이 섞여 있음을 표시하는 임시 current owner 상태일 뿐이다.
- service product operation 구현 plan을 작성하기 전에는 actual current code path와 evidence source를 구체 owner로 해소하거나, 구현 범위에서 제외하고 `promotion_state = blocked` 유지 사유를 기록해야 한다.
- `mixed-history` 자체를 promotion evidence 또는 target owner로 간주하지 않는다.

## Target Owner Invariants

- `target_owner = dotnet-config-migration-action`은 `product config migration apply` row에만 허용한다.
- `product config migration apply` row는 반드시 `target_owner = dotnet-config-migration-action`이어야 한다.
- `target_owner = dotnet-job-store-migration-action`은 `job store migration apply` row에만 허용한다.
- `job store migration apply` row는 반드시 `target_owner = dotnet-job-store-migration-action`이어야 한다.
- `job store migration apply`에서 일반 runtime save/read는 migration mutation 근거가 아니며, migration mutation은 반드시 `dotnet-job-store-migration-action`이 소유한다.
- `target_owner = dotnet-token-storage-action`은 `protected token bootstrap` row에만 허용한다.
- `protected token bootstrap` row는 반드시 `target_owner = dotnet-token-storage-action`이어야 한다.
- `target_owner = dotnet-data-root-action`은 `data root remove` row에만 허용한다.
- `data root remove` row는 반드시 `target_owner = dotnet-data-root-action`이어야 한다.
- `target_owner = windows-native-package`는 `local payload update`, `rollback restore` row에만 허용한다.
- `local payload update`, `rollback restore` row는 반드시 `target_owner = windows-native-package`여야 한다.
- `target_owner = windows-eventlog-action`은 `Event Log source registration`, `Event Log source removal` row에만 허용한다.
- `Event Log source registration`, `Event Log source removal` row는 반드시 `target_owner = windows-eventlog-action`이어야 한다.
- `target_owner = windows-firewall-action`은 `firewall rule enable LAN exposure`, `firewall rule removal` row에만 허용한다.
- `firewall rule enable LAN exposure`, `firewall rule removal` row는 반드시 `target_owner = windows-firewall-action`이어야 한다.
- `target_owner = windows-trust-store-action`은 `trust store install`, `trust store removal` row에만 허용한다.
- `trust store install`, `trust store removal` row는 반드시 `target_owner = windows-trust-store-action`이어야 한다.

## Implementation Basis Invariants

- `implementation_basis = dpapi-local-machine-token-plan`은 `protected token bootstrap` row에만 허용한다.
- `protected token bootstrap` row는 반드시 `implementation_basis = dpapi-local-machine-token-plan`이어야 한다.
- `dpapi-local-machine-token-plan`은 raw token 비노출, token source inventory, single-source precondition, existing protected token no-overwrite, legacy token migration, legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required, protected token schema, ACL hardening, service command line protected file path only, command line token value forbidden, diagnostics redaction evidence 전용이다.
- `implementation_basis = product-config-migration-plan`은 `product config migration apply` row에만 허용한다.
- `product config migration apply` row는 반드시 `implementation_basis = product-config-migration-plan`이어야 한다.
- `product-config-migration-plan`은 current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, validation preflight descriptor required, backup path inside owned config backup root, atomic config replace, no data root mutation, no token mutation, no job store mutation, no service identity mutation, partial config migration forbidden evidence, rollback on migration failure, rollback result diagnostics, cleanup evidence, service-start preflight decision descriptor only, validation writes forbidden, explicit admin opt-in before config write 전용이다.
- `implementation_basis = job-store-migration-plan`은 `job store migration apply` row에만 허용한다.
- `job store migration apply` row는 반드시 `implementation_basis = job-store-migration-plan`이어야 한다.
- `job-store-migration-plan`은 current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, runtime writer stopped evidence, backup path inside owned job-store backup root, destructive rewrite disabled by default, atomic job store replace, no config mutation, no token mutation, no service identity mutation, partial job store migration forbidden evidence, rollback on migration failure, rollback result diagnostics, recovery evidence, explicit admin opt-in before job store write 전용이다.
- `job store migration apply`는 current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, migration plan id/version, runtime writer stopped evidence 없이 실행할 수 없으며, job store ownership/schema/migration plan/runtime-writer stopped evidence가 불명확하면 job store write 없이 blocked diagnostics를 반환한다.
- `implementation_basis = eventlog-registration-plan`은 `Event Log source registration`, `Event Log source removal` row에만 허용한다.
- `Event Log source registration`, `Event Log source removal` row는 반드시 `implementation_basis = eventlog-registration-plan`이어야 한다.
- `eventlog-registration-plan`은 exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, owned-source-only removal, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution 전용이며 MSI default action이 아니다.
- `implementation_basis = firewall-rule-plan`은 `firewall rule enable LAN exposure`, `firewall rule removal` row에만 허용한다.
- `firewall rule enable LAN exposure`, `firewall rule removal` row는 반드시 `implementation_basis = firewall-rule-plan`이어야 한다.
- `firewall-rule-plan`은 `windows-firewall-action`, LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, owned-rule-only removal, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution 전용이며 default install/repair/MSI action이 아니다.
- `implementation_basis = data-root-lifecycle-plan`은 `data root remove` row에만 허용한다.
- `data root remove` row는 반드시 `implementation_basis = data-root-lifecycle-plan`이어야 한다.
- `data-root-lifecycle-plan`은 `REMOVE_DATA=1`, remove-data handoff descriptor required, exact data root path allowlist, owned data root marker/evidence, service deleted/absent precondition, installed service blocks delete diagnostics, protected token delete only within owned data root, no product root mutation, no service mutation, locked-file abort before partial delete, delete manifest/journal evidence, post-delete absence evidence, no partial delete success evidence, diagnostics evidence 전용이다.
- `implementation_basis = package-contract`는 `local payload update`, `rollback restore` row에만 허용한다.
- `local payload update`, `rollback restore` row는 반드시 `implementation_basis = package-contract`여야 한다.
- `package-contract`는 ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation 전용이다.
- `package-contract`가 channel/version/update payload/root evidence와 일치하지 않으면 update/rollback은 activation 또는 restore 없이 blocked diagnostics만 반환한다.
- `implementation_basis = windows-certificate-store-api`는 `trust store install`, `trust store removal` row에만 허용한다.
- `trust store install`, `trust store removal` row는 반드시 `implementation_basis = windows-certificate-store-api`여야 한다.
- `windows-certificate-store-api`는 release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, owned certificate evidence, certificate store delete limited to owned certificate, thumbprint/store binding evidence, no service mutation, no firewall mutation, no eventlog mutation, post-install trust binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution 전용이다.

## Approved System Executable Rule

- 첫 slice에서는 `implementation_basis = approved-system-executable` row를 만들지 않는다.
- `approved-system-executable`은 schema enum 후보로만 남기며, 현재 matrix row에서는 사용 count가 반드시 0이어야 한다.
- 후속 slice에서 외부 executable 실행이 필요하면 해당 implementation plan이 ADR/task approval required, exact executable path and publisher/hash evidence, non-shell invocation only, argument schema with allowed flags/values, no user-controlled raw arguments, working directory fixed, environment variable allowlist, no token/secret on command line, no implicit reboot, timeout/exit-code contract, stdout/stderr redaction, dry-run/WhatIf where supported, no chained shell, admin opt-in, post-run evidence, examples are candidates only, not allowlist를 먼저 정의해야 한다.
- executable identity 또는 argument ownership이 불명확하면 implementation basis는 blocked로 유지한다.

## Release Gate

- `none`: GA-ready product runtime 판정만으로 해당 row의 promotion 가능 여부를 판단한다.
- `release-approval-required`: GA-ready evidence가 있어도 stable publication, public trusted signing, external release, signed update/rollback 실행은 별도 release approval 전까지 금지한다.

## Network Exposure Gate

- `none`: LAN exposure approval 없이 해당 row의 GA-ready promotion 판단이 가능하다.
- `lan-exposure-approval-required`: loopback-only 기본 정책, LAN mode opt-in, token source, firewall scope 변경을 별도 network exposure approval 전까지 금지한다.

## Auth and Exposure Boundary

- 내부 전용 GA-ready 제품 런타임의 기본 Local API auth mode는 `single_bearer_token`이며, account file이 `no-default-account` 상태이면 bearer token gate가 계속 authoritative하다.
- Account/RBAC/JWT route는 `accounts.json`와 `jwt-signing-key.txt`가 구성된 뒤에만 additive local auth surface로 동작한다. 기본 bootstrap은 계정을 만들지 않는다.
- RBAC enforcement는 Local API request processor가 소유한다. Web Console의 role/permission 표시는 operator hint이며 authorization의 단일 근거가 아니다.
- Console route는 Hyper-V `vmconnect` handoff capability를 기본으로 노출한다. noVNC/WebSocket bridge는 explicit `--novnc-target-host`/`--novnc-target-port` 구성 전까지 disabled이며, 구성된 경우 Windows Desktop Node listener가 WebSocket-to-VNC TCP bridge를 제공한다. Linux console backend를 가져오지 않는다.
- loopback static asset bypass는 Web Console bootstrap을 위한 `unauthenticated-static-only` 정책으로만 허용한다.
- non-loopback static assets require bearer auth. LAN mode에서 static asset과 API route는 같은 bearer token boundary 안에 있어야 한다.
- LAN mode requires `-AllowLan` and a token source. token source 없이 LAN prefix를 열 수 없으며 `PCV_LAN_TOKEN_REQUIRED` error contract를 유지한다.
- non-loopback prefix without explicit LAN opt-in은 `PCV_PREFIX_NOT_LOOPBACK` error contract를 유지한다.

## API Route Matrix

| Route/Operation | Route surface | Domain | Risk tier | Current owner | Target owner | Implementation basis | Fallback policy | Promotion state | Admin smoke required | GA-ready gate | Release gate | Network exposure gate |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/host/status` | `current-route` | `host-read` | `tier1-read-only` | `dotnet-native` | `dotnet-host-adapter` | `registry-wmi-service` | `test-only` | `current-native` | `installed-non-mutating` | OS, Hyper-V, VMMS, admin, default switch parity와 installed smoke | `none` | `none` |
| `GET /api/v1/network/inventory` | `current-route` | `network-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | switch type, `allow_management_os`, external adapter parity, native structured failure, no helper retry | `none` | `none` |
| `GET /api/v1/vms` | `current-route` | `vm-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | helper JSON contract와 VM identity/state, CPU/startup memory/generation/checkpoint count summary field parity, storage/network parity, native structured failure, no helper retry | `none` | `none` |
| `GET /api/v1/vms/{id}` | `current-route` | `vm-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | native `vm.list` result parity, detail field parity, missing VM error contract, no helper retry | `none` | `none` |
| `GET /api/v1/vms/{id}/checkpoints` | `current-route` | `checkpoint-lifecycle` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | native VM inventory guard, empty checkpoint list success, checkpoint list field parity, missing VM error contract, no helper retry | `none` | `none` |
| `POST /api/v1/vms/{id}/start` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Enabled=2)`, job lifecycle, idempotency, cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/shutdown` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `Msvm_ShutdownComponent.InitiateShutdown`, graceful shutdown semantics, shutdown integration unavailable structured failure, Windows Server 2022 Evaluation guest successful shutdown smoke, timeout, recovery evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/poweroff` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Disabled=3)`, safe stop semantics, timeout, cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/restart` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Reset=11)`, reset restart semantics, stop-start sequencing fallback forbidden, timeout, recovery evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/checkpoints` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `CreateSnapshot`, post-create rename/list visibility, duplicate-name error, display-name parity, installed mutation smoke cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `ApplySnapshot`, not-found error, timeout/recovery, `vm.poweroff-before-restore` minimum stable restore condition, `0.29.0-admin-smoke` installed restore mutation cleanup evidence | `none` | `none` |
| `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `DestroySnapshot`, post-delete absence, not-found error, display-name parity, installed mutation smoke cleanup evidence | `none` | `none` |
| `POST /api/v1/vms` | `current-route` | `vm-lifecycle` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | native Generation 2 create, Generation 1 invalid error contract, explicit admin smoke, cleanup, rollback, no-auto-reboot evidence | `none` | `none` |
| `DELETE /api/v1/vms/{id}` | `current-route` | `vm-lifecycle` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `DestroySystem`, managed marker guard, not-found/idempotency contract, `0.30.1-admin-smoke` managed delete `action=delete`, repeat `action=absent`, unmanaged guard block, cleanup/no-auto-reboot evidence | `none` | `none` |
| `GET /api/v1/runtime/policy` | `current-route` | `job-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | token storage, network exposure, `job_runtime` policy shape, secret 비노출 | `none` | `none` |
| `POST /api/v1/auth/login` | `current-route` | `auth-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | local account file and JWT signing key configured before use, no-default-account bootstrap, password/JWT redaction, bearer fallback while unconfigured | `none` | `none` |
| `POST /api/v1/auth/refresh` | `current-route` | `auth-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | refresh token rotation contract, JWT secret redaction, no installed account login smoke claim | `none` | `none` |
| `POST /api/v1/auth/logout` | `current-route` | `auth-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | refresh/session revoke handoff, browser session clear boundary, no host mutation | `none` | `none` |
| `GET /api/v1/auth/session` | `current-route` | `auth-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | current account session read, role/permission summary, bearer fallback while account auth is unconfigured | `none` | `none` |
| `GET /api/v1/auth/rbac` | `current-route` | `auth-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | viewer/operator/admin permission matrix, API-owned enforcement, Web display is advisory | `none` | `none` |
| `GET /api/v1/console/capabilities` | `current-route` | `console-access` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | Windows `vmconnect` capability, noVNC disabled until explicit target host/port, WebSocket-to-VNC TCP bridge code-level evidence, no Linux console backend import | `none` | `none` |
| `GET /api/v1/vms/{id}/console` | `current-route` | `console-access` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | selected VM console handoff or optional noVNC bridge metadata, `console.view` permission, no browser-started host mutation | `none` | `none` |
| `GET WebSocket /api/v1/console/novnc/{vm_id}` | `current-route` | `console-access` | `tier1-read-only` | `dotnet-host-listener` | `dotnet-host-listener` | `websocket-to-vnc-tcp-bridge` | `none` | `current-native` | `none` | opt-in noVNC bridge, explicit target host/port required, loopback target by default, bearer or account JWT with `console.view`, no Linux noVNC backend import, target-backed installed streaming smoke PASS in `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md` | `none` | `lan-exposure-approval-required` |
| `GET /api/v1/jobs` | `current-route` | `job-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | paged read-only job list, `limit`/`offset` metadata, terminal retention cap, active job preservation | `none` | `none` |
| `GET /api/v1/jobs/{job_id}` | `current-route` | `job-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | persisted state read, not-found error, recovery read tests | `none` | `none` |
| `POST /api/v1/jobs/{job_id}/cancel` | `current-route` | `job-runtime` | `tier2-reversible-mutation` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `explicit-admin-opt-in` | queued-only cancel state transition, not-cancelable error, persistence recovery tests | `none` | `none` |
| `POST /api/v1/jobs/{job_id}/retry` | `current-route` | `job-runtime` | `tier2-reversible-mutation` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `explicit-admin-opt-in` | failed retryable-only retry, attempt limit, `retry_of` lineage, admin opt-in for underlying mutation evidence | `none` | `none` |

## Product Ops Matrix

| Operation | Route surface | Domain | Risk tier | Current owner | Target owner | Implementation basis | Fallback policy | Promotion state | Admin smoke required | GA-ready gate | Release gate | Network exposure gate |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| service status | `product-operation` | `product-ops` | `tier1-read-only` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `installed-non-mutating` | `DesktopNode.Host.exe service-action status` code-level native SCM controller, service identity read, exact binary path ownership check, installed service-action-status-start-stop smoke evidence `artifacts/service-action-status-start-stop-20260504-002359` | `none` | `none` |
| service start | `product-operation` | `product-ops` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action start` code-level native SCM controller, owned service identity, exact SCM binary path/product root binding, foreign service blocks before mutation, missing-service diagnostics, no config mutation, no service delete, service started state, already-running idempotency, listener health after start, timeout/recovery, no-auto-reboot evidence, installed service-action-status-start-stop smoke evidence `artifacts/service-action-status-start-stop-20260504-002359` | `none` | `none` |
| service stop | `product-operation` | `product-ops` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action stop` code-level native SCM controller, owned service identity, exact SCM binary path/product root binding, foreign service blocks before mutation, missing-service diagnostics, no config mutation, no service delete, stop idempotency, already-stopped idempotency, stop wait timeout, stop wait timeout diagnostics, no-auto-reboot evidence, installed service-action-status-start-stop smoke evidence `artifacts/service-action-status-start-stop-20260504-002359` | `none` | `none` |
| service install create | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action configure-installed`/MSI stable install path, service name ownership, exact SCM binary path/product root binding, protected token path/listener args/service account/start mode/failure policy, idempotent install behavior, no-auto-reboot evidence, fresh stable internal evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| service configure update | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action repair-installed`/stable MSI repair path, owned service precondition, exact SCM binary path/product root binding, protected token path/listener args preservation, data preservation, no-auto-reboot evidence, fresh stable internal evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| protected token bootstrap | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-token-storage-action` | `dpapi-local-machine-token-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action configure-installed` creates/uses DPAPI LocalMachine protected token file, raw token not exposed on service command line or diagnostics, existing token no-overwrite, protected file path only in SCM command line, REMOVE_DATA final reinstall proof, fresh stable internal evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| service repair missing service recreation | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | native service action install/repair owner, service absent recreate contract, product root/manifest/protected-token ownership, exact SCM binary path binding, no product/data root deletion in repair path, no-auto-reboot evidence, stable MSI reinstall-after-absent and package tests refresh evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| service repair config drift correction | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | native service action repair/configure owner, owned service identity, exact SCM binary path/product root binding, protected token path/listener args correction, foreign service block/code-level tests, stable MSI repair evidence, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| service uninstall stop/delete | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | stable MSI uninstall preserve/remove-data stop-before-delete, service deletion confirmation, missing-service wait, no product/data direct mutation by service action, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| product root removal preserve-data | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | stable MSI uninstall preserve deletes current product payload, leaves ProgramData/token/data root intact, removes legacy WinSW root files on current MSI install, final active root has no legacy WinSW root files, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| service uninstall remove-data request | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `current-native` | `explicit-admin-opt-in` | stable MSI `REMOVE_DATA=1` request, service deleted/absent precondition, remove-installed handoff/data-root-remove sequence, no direct token/data delete before service absence, final reinstall proof, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| data root remove | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-data-root-action` | `data-root-lifecycle-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action data-root-remove --remove-data`, exact allowlist, service absent precondition, protected token/job/event/install/diagnostics delete proof, service log preservation, final reinstall proof, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| local payload update | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `windows-native-package` | `package-contract` | `none` | `current-native` | `explicit-admin-opt-in` | internal `RequireSigned` stable `0.35.3` payload update from stable `0.35.2`, schema v1 payload manifest/version match, staged source outside active root, binary payload activation, previous root slot, service stop/start health, no config/data/token/service identity mutation, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353`. Network source gate code-level evidence: `docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md`. Updater catalog/channel resolver code-level evidence: `docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md`. Product root filesystem rollback code-level evidence: `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`. Installer publication descriptor code-level evidence: `docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md`, descriptor records public/external publication not-claimed and Burn/MSIX/winget/catalog publication not executed. Latest installed destructive admin-smoke evidence: `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`, update `0.38.6-admin-smoke -> 0.38.8-admin-smoke`, health `200`, update journal `succeeded/health`, final rollback restored current root. | `release-approval-required` | `none` |
| rollback restore | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `windows-native-package` | `package-contract` | `none` | `current-native` | `explicit-admin-opt-in` | retained previous root stable `0.35.2` manifest/hash validation, rollback from `0.35.3`, failed root diagnostics preservation, post-rollback manifest/version/health, data/token preservation, no-auto-reboot evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353`. Latest installed destructive admin-smoke evidence: `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`, rollback restored `0.38.6-admin-smoke`, preserved `0.38.8-admin-smoke` as `DesktopNode.failed`, final service `Running`, boot time unchanged. | `release-approval-required` | `none` |
| product config schema validation | `product-operation` | `product-ops` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | schema v1 product manifest validation, update payload preflight validation before service stop/activation, dry-run config migration descriptor only, diagnostics redaction, no config write/backup/service mutation in validation, Pester/package tests and stable payload manifest evidence `artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353` | `none` | `none` |
| product config migration apply | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-config-migration-action` | `product-config-migration-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action config-migration-apply --migration-plan-id product-config-v1-to-v2 --migration-plan-version 1` actual apply path. Requires owned service identity, stopped service proof, owned product manifest, source schema v1, supported plan identity. Performs config backup under data-root owned backup root, same-directory temp write/replace to manifest schema v2, rollback diagnostics on failure, no job store/token/service identity/MSI/update/rollback/OS mutation, no implicit service stop/start. Code-level evidence: `ConfigMigrationApplyBacksUpAndAtomicallyRewritesSupportedManifestPlan`; product wrapper v2 manifest read compatibility evidence: `reads migrated product manifest schema v2 for update and rollback compatibility`. Historical blocked attempt: `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-preflight-blocked-2026-05-06.md`. Installed destructive admin smoke PASS evidence: `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`, artifact `artifacts/config-jobstore-migration-apply-installed-20260507-0386`, version `0.38.6-admin-smoke`, product manifest schema `1 -> 2`, backup exists, temp absent after apply, final service `Running`, boot time unchanged, post-migration API read ok. Public trusted signing/external stable publication excluded. | `none` | `none` |
| job store schema mismatch detection | `product-operation` | `product-ops` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | `read-only-or-blocked-with-diagnostics` schema mismatch behavior, schema mismatch returns blocked diagnostics, runtime read must not mutate jobs.json, no quarantine move/write, no migration write, migration handoff descriptor only, no migration execution, xUnit evidence `JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine` and runtime policy `blocked-diagnostics-no-mutation` contract | `none` | `none` |
| job store migration apply | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-job-store-migration-action` | `job-store-migration-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action job-store-migration-apply --migration-plan-id job-store-v1-to-v2 --migration-plan-version 1` actual apply path. Requires owned service identity, stopped service/runtime writer proof, owned `jobs.json`, source schema v1, supported plan identity. Performs job store backup under data-root owned backup root, same-directory temp write/replace to schema v2, recovery diagnostics on failure, no config/token/service identity/MSI/update/rollback/OS mutation, no implicit service stop/start. Runtime now loads schema v2 migration stores and continues to block newer unsupported schemas without quarantine. Code-level evidence: `JobStoreMigrationApplyBacksUpAndAtomicallyRewritesSupportedStorePlan`, `JobStoreVersion2MigrationStoreLoadsWithoutBlockedDiagnostics`, `JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine`. Historical blocked attempt: `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-preflight-blocked-2026-05-06.md`. Installed destructive admin smoke PASS evidence: `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`, artifact `artifacts/config-jobstore-migration-apply-installed-20260507-0386`, version `0.38.6-admin-smoke`, job store schema `1 -> 2`, seeded job preserved/listed, backup exists, temp absent after apply, final service `Running`, boot time unchanged, post-migration API read ok. Public trusted signing/external stable publication excluded. | `none` | `none` |
| Event Log source registration | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-eventlog-action` | `eventlog-registration-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action eventlog-register` code-level registry-backed event source action, explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, no MSI/default execution, no-auto-reboot evidence, xUnit evidence `EventLogRegisterUsesNativeRegistryControllerWithoutExternalCommands`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun` | `none` | `none` |
| Event Log source removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-eventlog-action` | `eventlog-registration-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action eventlog-remove` code-level registry-backed event source removal action, explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, exact log/source binding, owned-source-only removal, foreign-source conflict blocks, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution, no-auto-reboot evidence, xUnit evidence `EventLogRemoveDeletesOwnedSourceWithoutExternalCommands` and `EventLogRemoveTreatsMissingSourceAsIdempotentSuccess`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun` | `none` | `none` |
| firewall rule enable LAN exposure | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-firewall-action` | `firewall-rule-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action firewall-enable --allow-lan` code-level COM-backed Windows Firewall action, LAN exposure approval gate retained, explicit admin opt-in, loopback default preservation, exact rule name `PureCVisor Desktop Node Local API LAN`, exact direction inbound, exact protocol TCP, exact local port 7777, exact profile Private, exact remote address scope LocalSubnet, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence, xUnit evidence `FirewallEnableCreatesOwnedAllowRuleWithoutExternalCommands` and `FirewallEnableRejectsForeignExistingRuleBeforeMutation`, historical actual evidence `artifacts/os-mutation-gates-20260505-003459-0341/firewall-enable.json`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun`, LAN IP runtime policy evidence `http://[redacted-private-endpoint]:7777/`, follow-up missing-rule lookup xUnit evidence `FirewallRuleLookupTreatsComFileNotFoundAsMissingRule` | `none` | `lan-exposure-approval-required` |
| firewall rule removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-firewall-action` | `firewall-rule-plan` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action firewall-remove` code-level COM-backed Windows Firewall action, explicit admin opt-in, exact rule name `PureCVisor Desktop Node Local API LAN`, exact direction inbound, exact protocol TCP, exact local port 7777, exact profile Private, exact remote address scope LocalSubnet, owned rule evidence, owned-rule-only removal, foreign-rule conflict blocks, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence, xUnit evidence `FirewallRemoveDeletesOwnedRuleWithoutExternalCommands` and `FirewallRemoveTreatsMissingRuleAsIdempotentSuccess`, historical actual evidence `artifacts/os-mutation-gates-20260505-003459-0341/firewall-remove.json`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun`, follow-up missing-rule lookup xUnit evidence `FirewallRuleLookupTreatsComFileNotFoundAsMissingRule` | `none` | `none` |
| trust store install | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-trust-store-action` | `windows-certificate-store-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action trust-store-install --release-approved` code-level X509Store-backed certificate store action, release approval gate retained, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, no service mutation, no firewall mutation, no eventlog mutation, thumbprint/store binding evidence, post-install trust binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence, xUnit evidence `TrustStoreInstallImportsApprovedCertificatesWithoutExternalCommands`, `TrustStoreInstallRequiresReleaseApprovalBeforeMutation`, `TrustStoreInstallRejectsForeignCertificateBeforeMutation`, historical actual evidence `artifacts/os-mutation-gates-20260505-003459-0341/trust-store-install-existing.json`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun`, final restore evidence `trust-store-restore-existing.json` | `release-approval-required` | `none` |
| trust store removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `dotnet-native` | `windows-trust-store-action` | `windows-certificate-store-api` | `none` | `current-native` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action trust-store-remove --release-approved` code-level X509Store-backed certificate store action, release approval gate retained, explicit admin opt-in, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, owned certificate evidence, thumbprint/store binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence, xUnit evidence `TrustStoreRemoveDeletesOwnedCertificatesWithoutExternalCommands` and `TrustStoreRemoveTreatsMissingCertificatesAsIdempotentSuccess`, historical actual evidence `artifacts/os-mutation-gates-20260505-003459-0341/trust-store-remove-existing.json`, latest actual evidence `artifacts/os-mutation-gates-20260505-180434-0357-rerun`, final restore evidence `trust-store-restore-existing.json` | `release-approval-required` | `none` |

## Package Contract Execution Guard

- `local payload update`, `rollback restore`는 ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation이 모두 있어야 실행할 수 있다.
- `local payload update`는 package manifest/hash/root ownership/service stopped evidence 또는 ADR-0002 channel/version/update payload binding이 불명확하면 activation 없이 blocked diagnostics만 반환한다. `-SourceUri`와 `-UpdateCatalogUri` catalog gate는 activation 전에 package SHA-256/source-root preflight를 통과해야 하며, public trusted signing 또는 외부 stable publication claim을 만들지 않는다.
- `rollback restore`는 previous root/hash/ownership/service stopped evidence 또는 ADR-0002 channel/version/previous root slot binding이 불명확하면 restore 없이 blocked diagnostics만 반환한다.

## OS Mutation Execution Guard

- `Event Log source registration`, `Event Log source removal`, `firewall rule enable LAN exposure`, `firewall rule removal`, `trust store install`, `trust store removal`은 기본 install/repair/diagnostics/MSI 경로에서 실행하지 않는다.
- `Event Log source registration`은 `DesktopNode.Host.exe service-action eventlog-register` code-level registry-backed action이 소유한다. 실제 source 등록은 별도 explicit admin opt-in smoke에서만 실행하고, 기본 diagnostics/MSI 경로는 deferred policy와 host mutation 미수행 evidence만 기록한다.
- `Event Log source removal`은 `DesktopNode.Host.exe service-action eventlog-remove` code-level registry-backed action이 소유한다. 실제 source 제거는 별도 explicit admin opt-in smoke에서만 실행하고, 기본 diagnostics는 deferred policy와 host mutation 미수행 evidence만 기록한다.
- `Event Log source removal`은 source/channel ownership 또는 log/source binding이 불명확하면 registry delete 없이 blocked diagnostics만 반환한다.
- `firewall rule enable LAN exposure`는 `DesktopNode.Host.exe service-action firewall-enable --allow-lan` code-level COM-backed action이 소유한다. 실제 firewall rule enable은 `network_exposure_gate = lan-exposure-approval-required`, LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence가 모두 있어야 실행할 수 있다.
- `firewall rule enable LAN exposure`는 rule tuple/ownership/scope가 불명확하면 firewall write 없이 blocked diagnostics만 반환한다.
- `firewall rule removal`은 `DesktopNode.Host.exe service-action firewall-remove` code-level COM-backed action이 소유한다. 실제 firewall rule removal은 explicit admin opt-in, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, owned-rule-only removal, foreign-rule conflict blocks, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence가 모두 있어야 실행할 수 있으며 LAN exposure를 열지 않는다.
- `firewall rule removal`은 rule tuple/ownership/scope가 불명확하면 firewall delete 없이 blocked diagnostics만 반환한다.
- `trust store install`은 `DesktopNode.Host.exe service-action trust-store-install --release-approved` code-level X509Store-backed action이 소유한다. 실제 trust store install은 `release_gate = release-approval-required`, release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, no service mutation, no firewall mutation, no eventlog mutation, thumbprint/store binding evidence, post-install trust binding evidence가 모두 있어야 실행할 수 있다.
- `trust store install`은 artifact/identity/store ownership이 불명확하면 certificate store write 없이 blocked diagnostics만 반환한다.
- `trust store removal`은 `DesktopNode.Host.exe service-action trust-store-remove --release-approved` code-level X509Store-backed action이 소유한다. 실제 trust store removal은 `release_gate = release-approval-required`, release approval, explicit admin opt-in, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, owned certificate evidence, thumbprint/store binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence가 모두 있어야 실행할 수 있다.
- `trust store removal`은 certificate identity/store ownership이 불명확하면 certificate store delete 없이 blocked diagnostics만 반환한다.
- 여섯 row의 실행 evidence는 no-auto-reboot와 mutation 전후 diagnostics를 포함해야 한다.
- 2026-05-05 `0.34.1-admin-smoke`는 사용자 fast-mode 관리자 opt-in으로 current native firewall enable/removal, LAN IP exposure smoke, internal Root/TrustedPublisher trust-store install/removal/restore를 실행했다. 후속 `0.35.5-admin-smoke`는 `artifacts/os-mutation-gates-20260505-101659-0355-final`에서 Event Log register/remove, firewall enable/remove, LAN IP `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets, internal Root/TrustedPublisher trust-store install/removal/restore를 실행 당시 HEAD 기준으로 확인했다.
- 최신 `0.35.7-admin-smoke`는 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`에서 같은 gate를 현재 HEAD 기준으로 다시 확인했다. 최신 LAN smoke는 `http://[redacted-private-endpoint]:7777/` bearer runtime policy/Web assets `HTTP 200`, final firewall rule count `0`, Event Log source absent, internal trust cert present 상태를 기록한다. 이 실행들은 public trusted signing, stable publication, local payload update, rollback restore를 실행하지 않았으며, default install/repair/MSI 경로의 자동 OS mutation 허용으로 해석하지 않는다.

External system executable 실행은 `windows-native-api`에 포함하지 않는다. `sc.exe`, `msiexec.exe`, `netsh.exe`는 가능한 `approved-system-executable` 후보 예시일 뿐 allowlist가 아니다. 이 첫 slice에서는 `implementation_basis = approved-system-executable` matrix row가 0개여야 한다. 이후 slice에서 non-PowerShell executable이 필요하면 해당 operation은 `implementation_basis = approved-system-executable`을 사용하고, 먼저 ADR/task approval 필요 여부, 정확한 executable path와 publisher/hash evidence, non-shell invocation only, 허용 flag/value가 있는 argument schema, user-controlled raw argument 금지, 고정 working directory, environment variable allowlist, command line token/secret 금지, implicit reboot 금지, timeout/exit-code contract, stdout/stderr redaction, 지원되는 경우 dry-run/WhatIf, chained shell 금지, admin opt-in, post-run evidence를 정의해야 한다. 구현 plan의 예시는 후보일 뿐 allowlist가 아니다. Executable identity 또는 argument ownership이 불명확하면 implementation basis는 blocked로 유지한다.

## Promotion Rule

Tier 1 promoted 조건:

- C# WMI/CIM adapter가 helper contract와 같은 public field를 반환한다.
- unsupported host, missing feature, access denied, not found error contract가 고정된다.
- xUnit contract test와 installed non-mutating route smoke가 통과한다.
- transition fallback 제거 조건이 닫힌다.

Tier 2 promoted 조건:

- Tier 1 조건을 만족한다.
- queued/running/succeeded/failed job state가 기존 public contract와 호환된다.
- cancel/retry/idempotency/timeout behavior가 테스트된다.
- 실패 중간 상태에서 cleanup 또는 safe recovery evidence가 있다.
- 관리자 opt-in smoke가 자동 reboot 없이 통과한다.

Tier 3 promoted 조건:

- Tier 2 조건을 만족한다.
- explicit admin opt-in smoke가 있다.
- no-auto-reboot evidence가 있다.
- rollback 또는 remove-data cleanup evidence가 있다.
- signing/channel/provenance policy와 충돌하지 않는다.
- diagnostics bundle이 변경 전후 상태와 cleanup 결과를 설명한다.

Release gate 조건:

- `release_gate = none`은 release execution 승인 없이 GA-ready promotion 판단이 가능하다.
- `release_gate = release-approval-required`는 GA-ready promotion이 가능해도 stable publication, public trusted signing, external release, signed update/rollback 실행을 별도 release approval 전까지 금지한다.
- `release_gate = release-approval-required`는 `local payload update`, `rollback restore`, `trust store install`, `trust store removal` row에만 허용한다.
- `local payload update`, `rollback restore`, `trust store install`, `trust store removal` row는 반드시 `release_gate = release-approval-required`여야 한다.

Release-gated pre-release evidence boundary:

- `release_gate = release-approval-required` row는 ADR-0004 승격 전에 `blocked`를 해소할 수 있지만, 그 근거는 release execution이 아니라 pre-release evidence여야 한다.
- 허용 evidence는 package/trust contract validation, manifest/hash/provenance validation, dry-run planning, non-mutating ownership checks, rollback plan validation, redaction evidence, no-auto-reboot evidence다.
- 금지 evidence는 stable publication, public trusted signing execution, certificate store write/delete, external update/rollback activation이다.
- Release approval 전에는 이 row가 `ga-ready-candidate`가 될 수는 있어도 execution-approved가 될 수 없다.

Network exposure gate 조건:

- `network_exposure_gate = none`은 LAN exposure approval 없이 GA-ready promotion 판단이 가능하다.
- `network_exposure_gate = lan-exposure-approval-required`는 release approval과 별개로 loopback-only 기본 정책, LAN mode opt-in, token source, firewall scope 변경 approval이 필요하다.
- `network_exposure_gate = lan-exposure-approval-required`는 `firewall rule enable LAN exposure` row와 optional noVNC WebSocket bridge row에만 허용한다.
- `firewall rule enable LAN exposure` row와 `GET WebSocket /api/v1/console/novnc/{vm_id}` row는 반드시 `network_exposure_gate = lan-exposure-approval-required`여야 한다.
- `firewall rule removal` row는 반드시 `network_exposure_gate = none`이어야 한다.

LAN exposure pre-approval evidence boundary:

- `network_exposure_gate = lan-exposure-approval-required` row는 LAN exposure approval 전에 `blocked`를 해소할 수 있지만, 그 근거는 firewall execution이 아니라 pre-LAN evidence여야 한다.
- 허용 evidence는 rule tuple validation, loopback default preservation proof, token source proof, non-mutating firewall ownership checks, scope planning, conflict diagnostics, redaction evidence, no-auto-reboot evidence다.
- 금지 evidence는 firewall rule create/update/delete, non-loopback listener exposure, token source mutation, external network reachability proof다.
- LAN approval 전에는 이 row가 `ga-ready-candidate`가 될 수는 있어도 exposure-approved가 될 수 없다.

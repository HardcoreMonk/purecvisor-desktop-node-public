# ADR-0004: 내부 전용 GA-ready 제품 런타임

- 상태: 적용 중
- 날짜: 2026-05-05
- 결정 마커:
  - `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`
  - `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`
  - `DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime`
- 대체 범위: ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단

## 맥락

Desktop Node는 Phase 25에서 `DesktopNode.Host.exe`를 기본 service host와 listener owner로 교체했고, Tier 1 read route, VM create/start/shutdown/poweroff/restart/delete, checkpoint create/restore/delete를 C# native adapter product path로 전환했다. `0.30.1-admin-smoke` installed destructive smoke는 VM delete managed/delete-repeat/unmanaged-guard evidence를 추가했다. 이후 GA-ready closure 작업에서 product ops mutation의 PowerShell-backed current owner를 0개로 줄이고, active `spikes/**` 제품 경로도 제거했다.

GA-ready 제품 runtime이 되려면 Windows 관리자가 설치, 수리, 삭제, 업데이트, 진단, 복구를 하나의 제품 경계로 이해할 수 있어야 한다. 이 ADR의 현재 적용 목표 상태는 PowerShell-free product ops/runtime이다.

2026-05-05 aggregate gate closure는 GA 범위 current route/product operation에서 blocked row, PowerShell-backed current owner, product fallback, active `spikes/**` product path를 모두 0으로 재계산했고, internal stable release/update/rollback evidence도 pass로 기록했다. 사용자는 Desktop Node를 외부 공개 제품이 아니라 내부 전용 서비스로 확정했다.

이 ADR은 해당 evidence와 내부 전용 범위를 근거로 현재 적용 결정이 된다. ADR-0001의 독립 Windows 저장소 결정과 ADR-0003의 internal root/leaf `RequireSigned` signing trust model은 계속 적용 중이다.

## 결정

Desktop Node의 제품 런타임 결정을 `ga-ready-product-runtime`으로 바꾼다.

배포 범위는 내부 전용 서비스로 고정한다.

GA-ready 기준:

- 제품 runtime/request path에서 PowerShell helper dependency를 제거한다.
- 제품 배포/운영 경로에서 PowerShell dependency를 제거한다.
- Hyper-V 조작은 C# WMI/CIM adapter 중심으로 전환한다.
- `spikes/**`는 활성 제품 경로에서 제거하거나 `archive/**`로 이동한다.
- stable release/update/rollback evidence는 ADR-0003의 `InternalEnterprise` trust model 또는 `AllowUnsignedDev` admin-smoke 범위에서만 주장한다.

Public trusted signing, 외부 stable publication, 일반 사용자용 public release는 이 제품 범위에 포함하지 않는다. 이 항목들은 "나중에 수행할 gate"가 아니라 내부 전용 서비스 결정의 scope 밖이다. 향후 외부 배포를 목표로 바꾸려면 별도 ADR이 필요하다.

## GA gate와 release gate 분리

이 ADR이 적용되어도 public trusted signing, external stable publication, 일반 사용자용 public release는 실행하지 않는다.

- GA-ready product runtime: 제품 runtime/ops/repo/test architecture가 GA 가능한 형태인지 판단한다.
- Internal release execution: ADR-0003 `InternalEnterprise` trust model, signed stable MSI lifecycle, update/rollback compatibility, 내부 배포 release notes를 별도 승인한다.
- External release execution: 현재 scope 밖이며 별도 ADR 없이는 주장하지 않는다.

## Aggregate GA-ready Decision Gate

ADR-0004를 current decision으로 승격하기 전에는 다음 aggregate gate가 닫혀야 했다.

- GA 범위의 `current-route`와 `product-operation` row는 `promotion_state = transition-helper` 또는 `promotion_state = blocked`가 0개여야 한다.
- `future-route` row는 GA 범위 제외 사유와 별도 implementation plan requirement를 명시해야 한다.
- 제품 runtime/request path에는 PowerShell helper가 없어야 한다.
- 활성 제품 경로에는 `spikes/**`가 없어야 한다.
- repo migration preflight evidence와 verification ownership replacement evidence가 완료되어야 한다.
- `tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row는 explicit admin opt-in evidence가 완료되어야 하며, Evidence Freshness Rule을 만족하지 않는 stale evidence는 aggregate GA-ready gate 충족에 사용할 수 없다.
- `release_gate = release-approval-required` row는 GA-ready 판정과 release execution을 분리하며, 별도 release approval 전에는 실행하지 않는다.

이 gate는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`에서 `aggregate_gate_status = closed`로 닫혔다.

## Aggregate Gate Closure Report

ADR-0004 current decision 승격 근거는 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`다.

요약:

- `ga_scope_current_route_count: 18`
- `ga_scope_product_operation_count: 22`
- `future_route_exclusion_count: 2`
- `transition_helper_count: 0`
- `blocked_count: 0`
- `powershell_current_owner_count: 0`
- `powershell_fallback_count: 0`
- `active_spikes_path_count: 0`
- `stable_internal_release_update_rollback_status: pass`
- `public_trusted_signing: excluded`
- `external_stable_publication: not-claimed`
- `aggregate_gate_status: closed`

## ADR-0001 Replacement Scope

ADR-0004 대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단이다.
`DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`와 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`는 별도 ADR이 바꾸기 전까지 현재 적용 결정으로 유지한다.
ADR-0001은 독립 Windows 저장소와 phase 19 이력 문서로 계속 적용 중이며, 제품 런타임 승격 판단만 이 ADR이 대체한다.

## Current Decision Promotion Procedure

이 ADR 적용 diff는 ADR-0004 상태를 `적용 중`으로 바꾸고, `docs/ADR_INDEX.md` 현재 적용 중인 ADR 표와 결정 마커를 갱신하며, ADR-0004를 제안 중인 ADR 후보 섹션에서 제거한다.
승격 근거는 `aggregate_gate_status = closed` closure report다.
승격 후 `PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 ADR-0004 하나다.

## 근거

- 현재 Phase 25 route parity는 transition fallback과 GA blocker를 명확히 구분하지 않는다.
- `spikes/**` 활성 경로가 남아 있으면 제품 runtime source와 historical baseline이 섞인다.
- 제품 검증 primary가 Pester legacy suite에 머무르면 .NET Host/API/runtime owner와 검증 owner가 어긋난다.
- 승인 전에는 후보 ADR로 분리해 `keep-spike` 결정을 유지하면서 Phase 26 alignment를 준비했고, 승인 후에는 같은 evidence chain이 ADR-0004 current decision의 근거가 됐다.

## 영향 범위

- 포함 경로:
  - `src/DesktopNode.*`
  - `web/**`
  - `packaging/windows-desktop-node/**`
  - `docs/**`
  - `archive/**`
- 제외 경로:
  - Linux `purecvisorsd`
  - Linux Single Edge UI/API
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- 운영 또는 검증 영향:
  - 이 ADR 적용만으로 host mutation은 실행하지 않는다.
  - 실제 Hyper-V mutation, MSI lifecycle, firewall rule enable/removal, Event Log source registration/removal, trust store install/removal은 계속 explicit admin opt-in gate다.

## 대안

### ADR-0001 keep-spike 유지

선택하지 않는다. `keep-spike`는 Phase 19 시점에서는 안전했지만, aggregate gate closure와 internal stable release/update/rollback evidence 이후에는 현재 내부 전용 서비스 범위를 설명하지 못한다.

### Phase spec만 유지하고 ADR을 만들지 않음

선택하지 않는다. 제품 승격 목표와 공개 경계, installer/service/update/security policy를 바꾸려는 결정은 ADR로 보여야 한다.

### 외부 공개 release를 동시에 채택

선택하지 않는다. public trusted signing, external stable publication, 일반 사용자용 배포는 내부 전용 서비스 scope 밖이다.

## 검증 기준

문서/ADR 변경:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

후속 route 전환 plan은 `dotnet test src/DesktopNode.sln`과 installed non-mutating/admin opt-in smoke 기준을 route tier에 맞게 추가한다.

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`
- `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- `docs/ga-ready/REPO_MIGRATION_MAP.md`
- `docs/ga-ready/VERIFICATION_OWNERSHIP.md`

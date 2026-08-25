# Post-04218 런타임/도메인 개발 Slice - 2026-05-15

```text
evidence_id: post-04218-runtime-domain-slices-2026-05-15
result: CODE_LEVEL_PASS
source_contract_alignment: docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md
source_version_anchor: 0.42.18-admin-smoke
actual_execution: code-contract-regression
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
runtime_api_diagnostics_bridge: route-family-evidence-linked
hyperv_dispatch_catalog_contract: vm-checkpoint-network-fixed
host_ops_lifecycle_bucket_contract: service-eventlog-firewall-truststore-data-root-separated
packaging_release_next_trigger: product-payload-change-after-04218-fullgate
operator_surface_journey_alignment: web-console-tui-cli-current-card
public_boundary_preserved: adr-0005-closed-adr-0006-internal-only
```

이 evidence는 post-04218 문서 계약을 실제 repository code/test contract로 승격한
후속 slice다. 실제 host mutation, package build, clean-host run, public signing,
external stable publication은 수행하지 않았다.

## 1. Runtime/Core

`ApiHandlerAdapterContract`는 `RuntimeApiDiagnosticsBridge`를
`route-family-evidence-linked`로 노출하고, `RouteFamilies`에서 jobs,
ops-summary, diagnostics route family를 evidence bridge 단위로 묶는다.

- jobs: `runtime-api-job-runtime-contract`
- ops-summary: `runtime-api-ops-summary-current-card`
- diagnostics: `runtime-api-diagnostics-bundle-contract`

이 contract는 `GET /api/v1/jobs`, job cancel/retry, `GET /api/v1/ops/summary`,
diagnostic bundle create/list/download route가 같은 Runtime/Core 운영 증거로
해석되도록 고정한다.

## 2. Hyper-V Domain

`DesktopNodeHyperVAdapterDispatchCatalog.ContractKey`는
`vm-checkpoint-network-fixed`다. `OperationsForHandler`는 network, VM list,
VM power-state, checkpoint mutation handler가 어떤 native operation을 소유하는지
테스트 가능한 catalog로 노출한다.

이 slice는 `DesktopNodeHyperVDomain`의 provider boundary와 dispatch catalog의
handler boundary가 drift하면 `PCV_NATIVE_DISPATCH_PROVIDER_BOUNDARY_DRIFT` 계열
회귀로 잡히도록 한다.

## 3. Host Ops

`DesktopNodeHostOpsCatalog.LifecycleBucketContractKey`는
`service-eventlog-firewall-truststore-data-root-separated`다.
`RequiredLifecycleSmokeBuckets`는 service lifecycle, Event Log, firewall,
trust store, data-root를 독립 smoke bucket으로 고정한다.

## 4. Packaging/Release

`New-PcvManualAdminCampaignDescriptor.ps1`는 baseline이
`0.42.18-admin-smoke`이고 target이 지정된 next candidate일 때
`packaging_release_next_trigger=product-payload-change-after-04218-fullgate`와
`release_candidate` block을 summary/descriptor에 기록한다. 0.42.19 candidate를
열 수 있는 descriptor metadata만 추가했으며 installer, package build, update,
rollback, clean-host 실행은 하지 않는다.

## 5. Operator Surfaces

`docs/OPERATOR_SURFACE_TERMS.md`는 Current-card 여정을 추가했다. Web Console은
`batch_evidence.latest`, TUI는 `current-card=ops-summary`, CLI는
`pcvcli ops summary`를 같은 운영자 journey로 사용한다.

## 6. Public Boundary

이 slice는 ADR-0005 public distribution candidate를 재개하지 않는다. ADR-0006
internal-private-network-only 경계를 유지하며 public trusted signing, winget public
submission, external stable publication, public clean-host smoke를 주장하지 않는다.

## 검증

```powershell
dotnet test src/DesktopNode.sln --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1' -Output Detailed"
```

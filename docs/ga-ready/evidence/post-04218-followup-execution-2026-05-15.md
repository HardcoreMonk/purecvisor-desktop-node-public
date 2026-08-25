# Post-04218 후속 실행 Slice - 2026-05-15

```text
evidence_id: post-04218-followup-execution-2026-05-15
result: PACKAGE_BUILD_PASS_CODE_CONTRACT_PASS
source_contract_alignment: docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md
source_runtime_domain_slice: docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md
source_version_anchor: 0.42.18-admin-smoke
target_version: 0.42.19-admin-smoke
actual_execution: code-contract-regression-and-admin-smoke-package-build
package_build_performed: true
package_build_decision: executed-0.42.19-admin-smoke
package_build_artifact_root: artifacts/admin-smoke-package-20260515-04219
target_msi_sha256: 3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55
target_update_zip_sha256: not-built
package_payload_aggregate_sha256: 868de3e80cd6d05263b3dacef8083cab951c6192f6ec74b81b56955f6ea9c49f
package_provenance_commit: 2b7bd9ed702a785361ea5bbaa8a969280d400360
manual_admin_package_pair_campaign_decision: not-run-package-build-only
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
runtime_route_registry_source: ApiHandlerAdapterContract
hyperv_dispatch_model: handler-registry-delegate-map
host_ops_family_helpers: service-eventlog-firewall-truststore-data-root
operator_surface_snapshot_parity: web-console-tui-cli-current-card
public_boundary_guard: public-boundary-ci-required
public_boundary_preserved: adr-0005-closed-adr-0006-internal-only
```

이 evidence는 `post-04218` 후속 개발 slice 1-2-3-4-5-6을 실제 코드 계약과
`0.42.19-admin-smoke` package build로 닫은 기록이다. 실행 범위는 repository
contract regression과 MSI package build다. `0.42.18-admin-smoke -> 0.42.19-admin-smoke`
package-pair campaign, update ZIP build, clean-host run, full admin host mutation,
public trusted signing, external stable publication은 실행하거나 주장하지 않았다.

## 1. Runtime/API

Runtime route registry는 `ApiHandlerAdapterContract`를 단일 source로 사용한다.
`DesktopNodeApiRuntimeRoutes`가 adapter contract에서 method/template/operation을
읽어 runtime matcher를 만들고, request processor permission lookup도 같은 contract를
사용한다.

검증 포인트:

- `GET /api/v1/vms/{vmId}/console` permission은 `console.view`다.
- `POST /api/v1/jobs/{jobId}/cancel`은 contract matcher에서 `jobId`를 추출한다.
- diagnostics route는 job route family로 잘못 분류되지 않는다.

## 2. Hyper-V Domain

`DesktopNodeHyperVNativeAdapter`의 dispatch는 switch 표현식이 아니라
`handler-registry-delegate-map` registry에서 handler delegate를 찾는다.
`DesktopNodeHyperVAdapterDispatchCatalog.DispatchModel`은 이 모델을 contract key로
노출하고, adapter는 등록된 handler 목록을 테스트 surface로 제공한다.

## 3. Host Ops

Host Ops family helper는 request processor 밖에서 service, Event Log, firewall,
trust store, data-root lifecycle을 분리한다.

- `DesktopNodeServiceLifecycleOps`
- `DesktopNodeEventLogOps`
- `DesktopNodeFirewallOps`
- `DesktopNodeTrustStoreOps`
- `DesktopNodeDataRootLifecycleOps`

`DesktopNodeHostOpsCatalog.RequiresDataRoot`는 data-root가 필요한 installed lifecycle
operation을 catalog 단위로 판정한다.

## 4. Packaging/Release

`packaging/windows-desktop-node/installer/build.ps1`로
`0.42.19-admin-smoke` MSI package build를 실행했다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.19-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260515-04219 -SigningMode AllowUnsignedDev -WixPath C:\Users\Operator\.dotnet\tools\wix.exe
```

결과:

- MSI: `artifacts/admin-smoke-package-20260515-04219/PureCVisorDesktopNode-0.42.19-admin-smoke-windows-x64.msi`
- MSI SHA-256: `3677d69988828f94fd10a0b1fa3036a060e217211d5fb5b215c153eac55b9d55`
- provenance: `artifacts/admin-smoke-package-20260515-04219/PureCVisorDesktopNode-0.42.19-admin-smoke-windows-x64.provenance.json`
- provenance commit: `2b7bd9ed702a785361ea5bbaa8a969280d400360`
- payload aggregate SHA-256: `868de3e80cd6d05263b3dacef8083cab951c6192f6ec74b81b56955f6ea9c49f`
- signing mode: `AllowUnsignedDev`

이 build는 package build evidence다. Update ZIP과 package-pair campaign은 아직
열지 않았으므로 `target_update_zip_sha256=not-built`로 기록한다.

## 5. Operator Surfaces

Web Console, TUI, CLI는 `GET /api/v1/ops/summary` current-card snapshot parity를
같은 운영자 여정으로 취급한다. Web Console은 `batch_evidence.latest`, TUI는
`current-card=ops-summary`, CLI는 `pcvcli ops summary`를 같은 snapshot 출처로
사용한다.

## 6. Public Boundary CI Guard

`docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md`는 ADR-0005와 ADR-0006 public boundary를
Pester required guard로 승격한다. 이 guard는 `public-boundary-ci-required`이며,
public trusted signing, winget public submission, external stable publication,
public stable installer URL, clean-host public signed install/update/rollback smoke를
계속 `not-claimed` 또는 `out-of-scope`로 고정한다.

## 검증

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~HyperVDomainContractTests" --no-restore
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "HostOpsFamilyHelpersOwnLifecycleBucketsOutsideTheRequestProcessor|HostOpsCatalogPublishesPost04218LifecycleBucketContract" --no-restore
dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --filter RendererAndSharedTermsExposeThePost04218CurrentCardJourney --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

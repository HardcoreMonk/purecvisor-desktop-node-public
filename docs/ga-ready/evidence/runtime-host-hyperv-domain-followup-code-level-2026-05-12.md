# Runtime/Host Ops/Hyper-V 경계 재분리 Code-level Evidence - 2026-05-12

evidence_id: `runtime-host-hyperv-domain-followup-code-level-2026-05-12`
created_at: `2026-05-12T01:27:53.8938011+09:00`
source_branch: `codex/followup-1-6-phase2`
source_scope: code-level repository change
result: `pass`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.2-admin-smoke` 이후 Stabilize Then Split 후속 작업의 code-level
경계 재정렬을 기록한다. Installed MSI, service restart, Hyper-V VM mutation,
firewall/LAN/Event Log/trust-store/Credential Manager/TLS/token host mutation은 실행하지
않았다.

## 적용 범위

- Runtime/Core: `DesktopNodeApiRequestProcessor`가 auth/session, jobs, diagnostics route를 `DesktopNodeApiAuthSessionHandler`, `DesktopNodeApiJobRuntimeHandler`, `DesktopNodeApiDiagnosticsHandler` 경유로 dispatch한다.
- Host Ops: `config-migration-apply`, `job-store-migration-apply`, `service-token-rotation-revoke`를 각각 `DesktopNodeConfigMigrationOps`, `DesktopNodeJobStoreMigrationOps`, `DesktopNodeServiceTokenOps` owner로 분리하고 `DesktopNodeHostOpsCatalog`에 등록했다.
- Hyper-V: `DesktopNodeHyperVWmiProviderCatalog`를 추가해 `DesktopNodeHyperVDomain.Catalog`의 provider boundary와 실제 WMI/native provider implementation type을 연결했다.

## Code-level 증거

| 경계 | 현재 증거 |
|------|-----------|
| Runtime/Core auth/session | `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`, `DesktopNodeApiAuthSessionHandler` |
| Runtime/Core jobs | `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`, `DesktopNodeApiJobRuntimeHandler`, `HandleJobGet`, `HandleJobCancel`, `HandleJobRetry` |
| Runtime/Core diagnostics | `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`, `DesktopNodeApiDiagnosticsHandler` |
| Host Ops config migration | `src/DesktopNode.Host/Ops/DesktopNodeConfigMigrationOps.cs`, `ConfigMigrationOpsOwnsNativeConfigMigrationDelegation` |
| Host Ops job store migration | `src/DesktopNode.Host/Ops/DesktopNodeJobStoreMigrationOps.cs`, `JobStoreMigrationOpsOwnsNativeJobStoreMigrationDelegation` |
| Host Ops service token | `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs`, `ServiceTokenOpsOwnsNativeServiceTokenDelegation` |
| Hyper-V WMI provider boundary | `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs`, `HyperVWmiProviderCatalogCoversDomainProviderBoundaries` |

## 후속 연결

`docs/ga-ready/evidence/runtime-hyperv-operator-followup-code-level-2026-05-12.md`는 이 evidence의 다음 slice다. 해당 slice는 console/ops-summary handler dispatch, Hyper-V WMI provider 파일 분리, 다음 manual-admin descriptor, P1 historical evidence 한국어 재작성을 기록한다.

## 검증

- `dotnet restore src/DesktopNode.sln`: PASS
- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "ServiceActionPlansDeclareStableOperationFamilies|HostOpsCatalogDeclaresIndependentOperationOwners|ConfigMigrationOpsOwnsNativeConfigMigrationDelegation|JobStoreMigrationOpsOwnsNativeJobStoreMigrationDelegation|ServiceTokenOpsOwnsNativeServiceTokenDelegation|ServiceTokenRotationRevokePlanUsesNativeServiceActionWithoutExternalCommands" --no-restore`: PASS, 27 passed
- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ApiAccountAuthRequestProcessorTests|ApiDiagnosticBundleRequestProcessorTests|ApiHandlerAdapterContractTests|HyperVDomainContractTests|ApiHardeningRequestProcessorTests" --no-restore`: PASS, 32 passed

## 경계

- 이 evidence는 code-level split과 contract guard만 주장한다.
- Installed listener rerun, MSI apply, manual-admin campaign, Hyper-V 실제 VM mutation, OS mutation은 이 evidence에 포함하지 않는다.
- Public trusted signing, winget public submission, external stable publication, public clean-host signed smoke는 ADR-0006 기준 범위 밖이다.

## Stale 조건

다음 중 하나가 바뀌면 이 evidence는 current code-level split 근거로 stale 처리한다.

- `DesktopNodeApiRequestProcessor` route dispatch 순서 또는 `DesktopNodeApiRuntimeCoreHandlers` handler contract 변경
- `DesktopNodeHostServiceActionPlan`의 `OperationFamily`/native operation property 변경
- `DesktopNodeHostOpsCatalog` owner 이름 또는 operation 목록 변경
- `DesktopNodeHyperVDomain.Catalog` provider boundary 변경
- `DesktopNodeHyperVWmiProviderCatalog` provider interface/implementation type 변경
- Hyper-V WMI provider implementation 파일 경계 변경

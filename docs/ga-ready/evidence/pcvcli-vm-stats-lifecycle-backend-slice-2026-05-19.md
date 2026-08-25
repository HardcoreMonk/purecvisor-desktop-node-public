# PCVCLI VM stats/lifecycle backend slice 2026-05-19

evidence_id: `pcvcli-vm-stats-lifecycle-backend-slice-2026-05-19`
result: `PASS_CODE_LEVEL_PRODUCT_PAYLOAD_CHANGED`
source_branch: `codex/vm-stats-lifecycle-backend-slice`
base_installed_anchor: `0.42.34-admin-smoke`
next_package_decision: `0.42.35-admin-smoke-required-after-merge`
host_mutation_performed: `false`
package_build_performed: `false`
public_release: `not-claimed`

이 slice는 Linux `pcvctl` 호환 command 중 Windows Desktop Node Hyper-V backend에
대응되는 항목을 code-level product route로 승격했다. 설치본 package, full admin
host mutation gate, manual-admin package-pair campaign은 아직 실행하지 않았다.
제품 payload가 바뀌었으므로 merge 이후 새 `0.42.35-admin-smoke` package chain에서
installed evidence를 닫아야 한다.

## 승격한 명령

| 명령 | Local API route | backend contract |
| --- | --- | --- |
| `pcvcli vm memory-stats <vm>` | `GET /api/v1/vms/{vm}/memory-stats` | Hyper-V VM inventory summary 기반 read-only metrics |
| `pcvcli vm cpu-stats <vm>` | `GET /api/v1/vms/{vm}/cpu-stats` | Hyper-V VM inventory summary 기반 read-only metrics |
| `pcvcli vm pause <vm>` | `POST /api/v1/vms/{vm}/pause` | queued mutation, `Msvm_ComputerSystem.RequestStateChange` |
| `pcvcli vm resume <vm>` | `POST /api/v1/vms/{vm}/resume` | queued mutation, `Msvm_ComputerSystem.RequestStateChange` |
| `pcvcli vm rename <vm> <new_name>` | `POST /api/v1/vms/{vm}/rename` | queued mutation, `Msvm_VirtualSystemManagementService.ModifySystemSettings` |

## MANUAL-ADMIN gate 분리

`vm set-memory`, `vm set-vcpu`, `vm disk-resize`는 일반 backend gap이 아니라
`vm-resource-mutation` MANUAL-ADMIN gate 후보로 분리했다. 현재 CLI는 직접 호출 시
`PCV_CLI_MANUAL_ADMIN_GATE_REQUIRED`를 반환한다. 제품 route 노출 전에는 live/offline
mutation policy, validation, rollback/error contract, installed host mutation
evidence, package/fullgate/manual-admin package-pair evidence가 필요하다.

## 검증

```powershell
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter "RoutesCommandsToLocalApiRequests|RejectsLinuxPcvCtlCommandsThatNeedDesktopNodeBackendSlice|RejectsMutationHeavyLinuxPcvCtlCommandsBehindManualAdminGate|RoutesEveryDesktopNodeHyperVRuntimeOperationThroughPcvCli|HelpListsAvailableCommandsAsSingleCommandRows|HelpUsesLinuxStyleCyberPaletteAndCommandRows|BuildsVmRenameBodyFromLinuxPcvCtlShape|DocumentsBackendCommandGapSliceForLinuxPcvCtlCandidates"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "DefaultContractMapsPhase25RouteCandidates|RuntimeRouteRegistryPublishesNativeQueuedMutationMatchers|HyperVDomainCatalogOwnsCurrentReadAndMutationOperations|HyperVWmiProviderCatalogCoversDomainProviderBoundaries|HyperVWmiProviderSetCreatesDefaultBoundaryMapFromCatalog|HyperVWmiProviderCatalogPublishesProviderSetFactoryCallSites|HyperVWmiHelperCatalogPublishesCommonProviderBoundary|HyperVAdapterDispatchCatalogCoversDomainProviderBoundaries|HyperVAdapterDispatchCatalogPublishesPost04218HandlerContract|HyperVNativeAdapterPublishesDelegateRegistryDispatchModel|MutationRoutesQueueJobsWithoutInvokingExternalFallback|VmRenameRouteQueuesJobWithOldAndNewNameWithoutExternalFallback|QueuedVmPowerStateWorkerDispatchesToNativeAdapterWithoutExternalFallback|QueuedVmRenameWorkerDispatchesToNativeAdapterWithoutExternalFallback|NativeVmPowerStateAdapterMapsProviderResult|NativeVmRenameAdapterMapsProviderResult|WmiVmPowerStateProviderUsesRequestStateChangeConstants"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter "RuntimePolicySerializesPhase24JobRuntimeContract|RuntimePolicyDeclaresNativeReadRouteStart|RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary"
dotnet test src\DesktopNode.sln --no-restore
git diff --check
```

결과는 모두 PASS다. Solution test 기준 통과 수는 Contracts `7`, CLI `87`, Runtime
`17`, Service `11`, TUI `120`, Host `148`, API `199`다.

## 다음 package gate 판단

이번 변경은 API route, runtime policy, Hyper-V adapter, CLI command catalog/help,
문서 evidence가 모두 바뀐 product payload change다. 따라서 다음 순서가 필요하다.

1. main merge 후 `0.42.35-admin-smoke` package build
2. 새 package 기준 full admin host mutation gate
3. `0.42.34 -> 0.42.35` package-pair descriptor/readiness/manual-admin campaign
4. 설치본 `pcvcli host status`, `pcvcli --json vm list`, `pcvcli vm memory-stats/cpu-stats`, `pcvcli vm pause/resume/rename` smoke
5. installed Web/TUI/CLI current-card 갱신

이 evidence는 internal admin-smoke/code-level 범위이며 public trusted signing 또는
external stable publication을 주장하지 않는다.

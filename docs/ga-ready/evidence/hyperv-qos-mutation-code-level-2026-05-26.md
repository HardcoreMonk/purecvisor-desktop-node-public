# Hyper-V QoS Mutation Code-level Evidence 2026-05-26

evidence_id: `hyperv-qos-mutation-code-level-2026-05-26`
result: `PASS_CODE_LEVEL`
scope: `phase2-hyperv-qos-mutation-api-cli-native-source-payload`
adr: `docs/adr/0008-hyperv-qos-mutation-policy.md`
planning_evidence: `docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md`
preview_contract: `hyperv-qos-mutation-preview.v1`
apply_evidence_contract: `hyperv-qos-mutation-apply-evidence.v1`
api_routes: `POST /api/v1/vms/{vm}/qos/storage/preview`, `POST /api/v1/vms/{vm}/qos/storage`, `POST /api/v1/vms/{vm}/qos/network/preview`, `POST /api/v1/vms/{vm}/qos/network`
native_operations: `vm.qos.storage.preview`, `vm.qos.network.preview`, `vm.qos.storage.set`, `vm.qos.network.set`
cli_commands: `pcvcli vm blkio-set`, `pcvcli vm bandwidth-set`
host_mutation_code_path: `implemented-wmi-storage-iops-and-network-port-bandwidth`
host_mutation_performed: `false`
package_build_performed: `superseded-by-0.42.47-installed-evidence`
installed_actual_vm_smoke: `superseded-by-0.42.47-installed-evidence`
full_admin_host_mutation_gate: `superseded-by-0.42.47-installed-evidence`
manual_admin_package_pair: `pending`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결론

Phase 2 Hyper-V QoS mutation의 첫 code-level slice를 구현했다. Source payload는
contract DTO, Local API preview/apply route, queued job dispatch, Hyper-V native adapter
operation ownership, WMI apply code path, PCVCLI dry-run/apply UX, Runtime Policy 노출을
포함한다.

이 evidence 자체는 설치본/package/admin gate closure가 아니다. 후속
`docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md`에서 실제 VM
storage/network QoS mutation, rollback restore, full admin host mutation gate를 PASS로
승격했다. Manual-admin package-pair closure는 계속 후속 gate로 남긴다.

## 구현 범위

| 범위 | 상태 | 근거 |
| --- | --- | --- |
| Contract DTO | `implemented` | `DesktopNode.Contracts.HyperVQosMutationContract` |
| Local API preview | `implemented` | `/api/v1/vms/{vm}/qos/storage/preview`, `/api/v1/vms/{vm}/qos/network/preview` |
| Local API queued apply | `implemented` | `/api/v1/vms/{vm}/qos/storage`, `/api/v1/vms/{vm}/qos/network` |
| Hyper-V domain ownership | `implemented` | `DesktopNodeHyperVDomain`, `DesktopNodeHyperVAdapterDispatchCatalog` |
| Native WMI apply code path | `implemented-source` | storage IOPS, network port bandwidth setting mutation path |
| PCVCLI UX | `implemented` | `vm blkio-set ... --dry-run|--yes`, `vm bandwidth-set ... --dry-run|--yes` |
| Runtime Policy | `implemented` | native probe/mutation operation set 갱신 |
| Web/TUI direct control | `deferred` | Phase 3 backend-policy-first |
| Installed actual VM smoke | `promoted-by-0.42.47` | `docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md` |

## 검증

```powershell
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter HyperVQosMutationContractTests
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "QosPreviewRoutesReturnDryRunContractWithoutQueuingJob|QosApplyRoutesQueueJobsWithRollbackDescriptorInputs|QueuedQosMutationWorkerDispatchesToNativeAdapterWithRollbackEvidence|DefaultContractMapsPhase25RouteCandidates|DefaultContractGroupsRuntimeCoreRoutesByFamily|RuntimeRouteRegistryPublishesNativeQueuedMutationMatchers"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "HyperVDomainCatalogOwnsCurrentReadAndMutationOperations|HyperVDomainClassifiesProviderBoundary|HyperVAdapterDispatchCatalogCoversDomainProviderBoundaries|HyperVAdapterDispatchCatalogPublishesPost04218HandlerContract"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "NativeVmQosPreviewAdapterReturnsDryRunContract|NativeVmQosMutationAdapterPassesRequestToResourceProvider"
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter "BuildsQosMutationPreviewRequests|BuildsQosMutationApplyRequests|RequiresDryRunOrExplicitYesForQosMutation|RoutesEveryDesktopNodeHyperVRuntimeOperationThroughPcvCli|UsageShowsPcvCliCommandName|HelpListsAvailableCommandsAsSingleCommandRows|CompletesKnownInteractiveCommandPrefixes"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter RuntimePolicyContractTests
```

현재 기록 시점의 위 focused test 결과는 PASS다.

## 후속 Gate

1. `0.42.45-admin-smoke -> 0.42.47-admin-smoke` manual-admin package-pair closure.
2. installed Web/TUI/CLI current-card evidence 갱신.

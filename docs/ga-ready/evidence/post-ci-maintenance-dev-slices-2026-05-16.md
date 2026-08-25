# Post-CI Maintenance 개발 Slice Code-level Evidence

evidence_id: post-ci-maintenance-dev-slices-2026-05-16
result: CODE_LEVEL_PASS
actual_execution: code-contract-regression
source_version_anchor: 0.42.20-admin-smoke
next_product_payload_candidate: 0.42.21-admin-smoke
source_full_admin_gate: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md
source_public_boundary_main_push: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md
public_boundary_main_push_run_id: 25934411998
public_boundary_main_push_job_id: 76236050409
public_boundary_checkout_action_version: actions/checkout@v6.0.2
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
hyperv_provider_callsite_guard: hyperv-wmi-provider-callsite-drift-guard-v1
host_ops_reason_code_contract: host-ops-dryrun-mutation-reason-code-v1
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
package_build_decision: candidate-selected-awaiting-package-build
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 public-boundary CI maintenance 이후 승인된 `1-2-3-4-5-6` 개발
slice를 code-level contract로 닫는다. 실제 package build, clean-host campaign,
full admin host mutation, public trusted signing, external stable publication은
실행하지 않았다. 코드 변경은 `0.42.20-admin-smoke` 이후 다음 product payload 후보를
`0.42.21-admin-smoke`로 선택하는 근거다.

## 고정한 계약

```text
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
hyperv_provider_callsite_guard: hyperv-wmi-provider-callsite-drift-guard-v1
host_ops_reason_code_contract: host-ops-dryrun-mutation-reason-code-v1
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
next_product_payload_candidate: 0.42.21-admin-smoke
```

- Runtime/API: `ApiHandlerAdapterContract.RuntimeEvidenceContract`가 diagnostics와
  ops summary route를 `DesktopNodeApiRuntimeRoutes` handler registry key 및
  `runtime-core-boundary-baseline-2026-05-11.md` 문서 anchor에 연결한다.
- Hyper-V domain: `DesktopNodeHyperVWmiProviderCatalog`가 각 WMI provider boundary의
  `DesktopNodeHyperVProviderSet.CreateDefaultWmi` factory call-site와 provider set
  property를 공개해 call-site drift를 테스트로 잡는다.
- Host Ops: `DesktopNodeHostOpsCatalog`가 mutation boundary별 dry-run evidence reason
  code와 actual mutation reason code를 `HOST_OPS_*` 형식으로 정규화한다.
- Packaging/Release: `New-PcvManualAdminCampaignDescriptor.ps1`가
  `manual-admin-descriptor-generation-contract-v2`, required code contracts, next product
  payload candidate status를 descriptor와 summary에 기록한다.

## Public Boundary 최신 Anchor

`public-boundary.yml`은 PR #135 merge 뒤 `main` head
`3933231e6e2abf3a398dfcc3fdc999b3df38dac6` 기준 run `25934411998` / job
`76236050409`에서 PASS했다. Checkout step은 `actions/checkout@v6.0.2`로 실행됐고,
Node.js 20 deprecation warning 문자열은 관찰되지 않았다.

## 검증

```text
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~HyperVDomainContractTests" /p:UseSharedCompilation=false
result: pass
tests: 26 passed, 0 failed

dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~HostOpsCatalog" /p:UseSharedCompilation=false
result: pass
tests: 6 passed, 0 failed

pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1' -Output Detailed"
result: pass
tests: 5 passed, 0 failed

pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
result: pass
tests: 53 passed, 0 failed

dotnet test src\DesktopNode.sln --no-restore /p:UseSharedCompilation=false
result: pass
tests: 516 passed, 0 failed

pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
result: pass
tests: 345 passed, 0 failed
```

## 경계

이 evidence는 repository code/test contract만 닫는다. 설치본 service state, Hyper-V VM,
firewall, trust store, Credential Manager, Event Log, MSI install/update/rollback,
clean-host VM은 변경하지 않는다. Public trusted signing, trusted timestamp, external
stable publication, winget submission, public stable installer URL은 ADR-0006 기준
out-of-scope이며 이 evidence에서 주장하지 않는다.

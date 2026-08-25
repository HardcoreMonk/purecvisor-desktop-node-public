# Runtime/Hyper-V/Operator 후속 Code-level Evidence - 2026-05-12

evidence_id: `runtime-hyperv-operator-followup-code-level-2026-05-12`
status: `code-level-pass`
actual_execution: `repo-code-and-doc-boundary-followup`
host_mutation_performed: `false`
installed_listener_rerun: `not-run`
msi_apply: `not-run`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `runtime-host-hyperv-domain-followup-code-level-2026-05-12` 이후 1-2-3-4-5 후속 작업의 code-level 경계를 기록한다. 실제 service install, MSI apply, Hyper-V VM mutation, firewall, Event Log, trust-store mutation은 실행하지 않았다.

## 코드 경계

- Hyper-V WMI provider는 `DesktopNodeHyperVWmiProviders.cs` monolith에서 provider boundary별 파일로 분리됐다.
- `DesktopNodeHyperVWmiProviderCatalog`의 provider boundary와 implementation type 계약은 유지한다.
- Runtime/Core request processor의 잔여 `console` route와 `ops-summary` route dispatch는 `DesktopNodeApiConsoleHandler`, `DesktopNodeApiOpsSummaryHandler`로 분리했다.
- Auth/session, jobs, diagnostics handler split은 기존 `DesktopNodeApiRuntimeCoreHandlers.cs` 경계 안에서 유지한다.

## 문서 경계

- 다음 manual-admin campaign descriptor는 이번 code-level split을 반영해 새 package build가 필요하다고 기록한다.
- P1 historical evidence 중 `internal-clean-host-install-update-rollback-smoke-2026-05-10-0417`와 `msix-package-lifecycle-smoke-2026-05-10-0416`의 운영자 본문을 한국어로 재작성했다.
- Artifact id, SHA-256, version token, command, public boundary token은 원문 값을 보존했다.

## Host Mutation 판단

이번 변경은 source code와 문서 경계 split이다. 새 MSI/update package를 아직 빌드하지 않았고 installed baseline도 바뀌지 않았으므로, 이 evidence에서 host mutation rerun은 필요 조건이 아니다. 다음 manual-admin campaign은 새 version/package input이 확정된 뒤 elevated operator opt-in으로 실행한다.

## 검증

- `dotnet restore src/DesktopNode.sln`: PASS
- `dotnet test src/DesktopNode.sln --no-restore`: PASS
- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ApiAccountAuthRequestProcessorTests|ApiHandlerAdapterContractTests|ApiRuntimePolicyRequestProcessorTests|HyperVDomainContractTests" --no-restore`: PASS, 145 tests
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: PASS, 309 tests
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: PASS, 49 tests
- `git diff --check`: PASS

## 경계

이 evidence는 code-level split과 문서 재작성 evidence다. Installed listener rerun, MSI apply, clean-host install/update/rollback, public trusted signing, winget submission, external stable publication, public GA release를 주장하지 않는다.

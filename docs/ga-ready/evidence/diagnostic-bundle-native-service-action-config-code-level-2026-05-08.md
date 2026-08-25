# Diagnostic Bundle Native Service-Action Config Code-Level Evidence - 2026-05-08

evidence_id: diagnostic-bundle-native-service-action-config-code-level-2026-05-08
scope: diagnostic-bundle-installed-service-listener-config-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Host/DesktopNodeHostServiceAction.cs
runner_version: DesktopNodeHostServiceActionTests.cs
actual_execution: code-level-native-service-action-test
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_host_listener_execution: code-level-host-listener
diagnostic_bundle_installed_listener_execution: not-run
diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator
diagnostic_bundle_request_id_propagation: code-level-host-header
diagnostic_bundle_service_action_config: code-level-applied
server_config_mutation: code-level-product-and-native-service-plan-applied
installed_listener_blocker: closed-by-0.39.0-installed-listener-rerun

## 요약

이 evidence는 ADR-0005 diagnostic bundle installed listener gate의 선행 config gap을 code-level로 닫는다. `DesktopNode.Host.exe service-action configure-installed|repair-installed`가 만드는 native SCM `BinaryPathName`이 product wrapper plan과 같은 listener 인자 집합을 포함하도록 맞췄다.

새 native service-action config는 다음 값을 `DesktopNode.Host.exe listen` command line에 포함한다.

- `--diagnostics-root "%ProgramData%\PureCVisor\desktop-node\diagnostics"`
- `--api-token-protected-file "%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json"`
- `--route-timeout-seconds 30`
- `--request-limit-per-minute 120`
- `--request-burst-limit 20`
- `--retry-after-seconds 15`

0.38.9 full admin host mutation evidence는 Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store PASS로 유지한다. 다만 해당 artifact의 final SCM `PathName`은 `--diagnostics-root`와 hardening arguments를 아직 포함하지 않으므로, 이 문서 단독으로는 installed diagnostic bundle listener PASS를 주장하지 않는다.

후속 사용자 관리자 opt-in rerun인 `0.39.0-admin-smoke`는 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`와 `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`에서 새 native service-action config가 설치 listener에 반영되고 실제 protected-token create/download route가 통과했음을 확인했다. 따라서 current matrix의 `diagnostic_bundle_installed_listener_execution`은 `installed-listener-pass`, blocker는 `none`이다.

## 검증

TDD RED:

```powershell
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~ConfigureInstalledUsesNativeServiceControllerWithoutExternalCommands"
```

초기 실패:

- `Assert.Contains() Failure`
- missing substring: `--diagnostics-root`

GREEN:

```powershell
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~ConfigureInstalledUsesNativeServiceControllerWithoutExternalCommands"
```

결과:

- Focused host service-action test: PASS, 1 test.

## 범위 제외

- Installed Windows service config mutation
- MSI install/repair/uninstall lifecycle
- Installed diagnostic bundle create/download smoke
- Firewall/trust-store/LAN/Event Log mutation
- Public trusted signing
- External stable publication

이 evidence 자체는 code-level native service-action config guard다. Host mutation은 수행하지 않았고 public distribution claim도 하지 않는다. Installed listener PASS evidence는 별도 `0.39.0-admin-smoke` rerun 문서가 소유한다.

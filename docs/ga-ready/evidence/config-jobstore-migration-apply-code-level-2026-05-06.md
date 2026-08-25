# Config/Job Store Migration Apply Code-Level Evidence - 2026-05-06

이 문서는 `product config migration apply`와 `job store migration apply`가 plan-only/future-route 상태에서 code-level actual apply product operation으로 전환된 증거를 기록한다. 이 증거는 temp directory와 fake Windows service controller 기반 단위 검증이며, installed destructive admin smoke 또는 public trusted signing evidence가 아니다.

## Scope

- Product config apply action: `DesktopNode.Host.exe service-action config-migration-apply --migration-plan-id product-config-v1-to-v2 --migration-plan-version 1`
- Job store apply action: `DesktopNode.Host.exe service-action job-store-migration-apply --migration-plan-id job-store-v1-to-v2 --migration-plan-version 1`
- Runtime compatibility: `jobs.json` schema v2 load support, schema v99 blocked/no-mutation 유지
- Wrapper compatibility: migrated product manifest schema v2 read support

## Code-Level Result

- Product config apply는 owned service identity, stopped service proof, owned product manifest, source schema v1, supported plan identity를 요구한다.
- Supported config apply는 data-root owned backup root에 `product-manifest.json`을 백업하고, same-directory temp file을 쓴 뒤 manifest schema v2로 replace한다.
- Config apply 실패 descriptor는 rollback attempted/succeeded, original restored, partial config present 상태를 포함한다.
- Job store apply는 owned service identity, stopped service/runtime writer proof, owned `jobs.json`, source schema v1, supported plan identity를 요구한다.
- Supported job store apply는 data-root owned backup root에 `jobs.json`을 백업하고, same-directory temp file을 쓴 뒤 job store schema v2로 replace한다.
- Job store apply 실패 descriptor는 rollback/recovery 상태를 포함한다.
- 두 action 모두 implicit service stop/start, token mutation, service identity mutation, MSI/update/rollback, Hyper-V/firewall/trust-store/LAN/Event Log mutation을 수행하지 않는다.

## Verification

Code-level verification commands:

```powershell
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobStoreVersion2MigrationStoreLoadsWithoutBlockedDiagnostics"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Verified behaviors:

- `ConfigMigrationApplyBacksUpAndAtomicallyRewritesSupportedManifestPlan`
- `JobStoreMigrationApplyBacksUpAndAtomicallyRewritesSupportedStorePlan`
- `JobStoreVersion2MigrationStoreLoadsWithoutBlockedDiagnostics`
- `JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine`
- `reads migrated product manifest schema v2 for update and rollback compatibility`

## Remaining Gate

Route matrix rows are now `product-operation` / `dotnet-native` / `ga-ready-candidate` at code level. Because both rows are `tier3-destructive-or-persistent`, installed destructive admin smoke remains required before they can be used as current-native GA closure evidence.

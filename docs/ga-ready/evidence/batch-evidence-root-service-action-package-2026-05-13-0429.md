# Batch Evidence Root Service Action Package - 2026-05-13 0.42.9

evidence_id: `batch-evidence-root-service-action-package-2026-05-13-0429`
result: `PASS`
product_version: `0.42.9-admin-smoke`
host_mutation_performed: `false`
artifact_root: `artifacts/admin-smoke-package-20260513-0429`
msi: `PureCVisorDesktopNode-0.42.9-admin-smoke-windows-x64.msi`
msi_sha256: `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`
provenance_commit: `f0620f2e18ae25de8751333684cb74b5051dcdc6`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.9-admin-smoke` product payload package build는 PASS다. 이 build는
`EventLogDefaultTransitionRepair`/`EventLogDefaultTransition` deferred custom action에
`--eventlog-default-transition-timeout-seconds 60` timeout guard를 포함한다.

이 evidence는 package build와 productized installer surface evidence다. 실제 full
admin host mutation PASS claim은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md`가
소유한다.

## Artifact

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260513-0429` |
| MSI SHA-256 | `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb` |
| provenance | `artifacts/admin-smoke-package-20260513-0429/PureCVisorDesktopNode-0.42.9-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260513-0429/PureCVisorDesktopNode-0.42.9-admin-smoke-windows-x64.publication.json` |
| service host SHA-256 | `33f6186fdc39018ee2b6ce04a9934c5667ce3531687dcfae04b013a45e019655` |
| CLI SHA-256 | `17c6625463c68e8bf3629014bc4e7c92df167fa07500c27b2de78ecf43d8f941` |
| TUI SHA-256 | `ecd17e8fd01b3b4b0f5a0123e5c3722f84e0a9ca8f3d296c13f17aa1458194d0` |

## 포함된 변경

- `DesktopNode.Host.exe service-action eventlog-default-transition`는
  `--eventlog-default-transition-timeout-seconds`를 parse한다.
- timeout 기본값은 `60`초이고 허용 범위는 `1..600`초다.
- timeout 발생 시 native descriptor는 `provider-repair-timeout`,
  `write-timeout`, `timeout_guard_status=timed-out`을 기록한다.
- 성공 시 native descriptor는 `timeout_seconds`와
  `timeout_guard_status=completed-within-timeout`을 기록한다.
- MSI configure/repair deferred custom action은 timeout guard 값을 명시적으로
  전달한다.
- 기존 MSI `BATCH_EVIDENCE_ROOT` property와
  `service-action configure-installed|repair-installed --batch-evidence-root`
  productization은 이 package에도 보존된다.

## 검증

- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~EventLogDefaultTransitionFailsFastWhenTimeoutExpires|FullyQualifiedName~DesktopNodeHostOptionsTests|FullyQualifiedName~EventLogDefaultTransitionRepairsRemovesRestoresWritesSchemaAndChecksVolumeGuard|FullyQualifiedName~EventLogDefaultTransitionWritesRedactedDescriptorWhenDataRootIsProvided"`:
  PASS, 25/25.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1' -Output Detailed"`:
  PASS, 9/9.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.9-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260513-0429 -SigningMode AllowUnsignedDev -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`:
  PASS.

## Release Boundary

이 evidence는 internal/admin-smoke package build evidence다. Public trusted signing,
external stable publication, public update channel availability, winget submission은
claim하지 않는다.

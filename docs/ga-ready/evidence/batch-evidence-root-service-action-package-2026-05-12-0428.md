# Batch Evidence Root Service-action Package Evidence - 2026-05-12 0428

evidence_id: `batch-evidence-root-service-action-package-2026-05-12-0428`
status: `package-build-pass`
actual_execution: `code-level-service-action-and-admin-smoke-package-build`
host_mutation_performed: `false`
installed_listener_rerun: `not-run`
msi_apply: `not-run`
product_version: `0.42.8-admin-smoke`
package_root: `artifacts/admin-smoke-package-20260512-0428`
msi: `PureCVisorDesktopNode-0.42.8-admin-smoke-windows-x64.msi`
msi_sha256: `4fd1ef5229dc09d925ca9aaf95708a7925a4f99713b22b7be66aca466daef521`
provenance_commit: `5f04e3f09df0e963cee66e531be77e8d51374b58`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `--batch-evidence-root`를 service-action 설치/복구 경로로 제품화한
code-level 변경과 그 변경을 포함한 `0.42.8-admin-smoke` MSI package build를
기록한다. 실제 MSI install/repair, service restart, Hyper-V, firewall, Event Log,
trust-store mutation은 실행하지 않았다.

## 구현 범위

- `DesktopNode.Host.exe service-action configure-installed|repair-installed`가
  `--batch-evidence-root <path>`를 parse하고 `DesktopNode.Host.exe listen`
  `BinaryPathName`에 기록한다.
- `repair-installed`는 explicit `--batch-evidence-root`가 없으면 현재 SCM
  `PathName`의 기존 `--batch-evidence-root` 값을 보존하고, explicit 값이 있으면
  새 값으로 override한다.
- MSI custom action은 public property `BATCH_EVIDENCE_ROOT`가 있을 때
  configure/repair action에 `--batch-evidence-root "[BATCH_EVIDENCE_ROOT]"`를
  전달할 수 있다.
- Product wrapper plan/entrypoint는 `-BatchEvidenceRoot`를 받아 product service
  host arguments와 `service.config.batch_evidence_root`에 기록한다.

## Package Build

```text
version: 0.42.8-admin-smoke
artifact_root: artifacts/admin-smoke-package-20260512-0428
msi_sha256: 4fd1ef5229dc09d925ca9aaf95708a7925a4f99713b22b7be66aca466daef521
provenance_commit: 5f04e3f09df0e963cee66e531be77e8d51374b58
payload_aggregate_sha256: 759b56bf2d816060901edc42de67fd95096fdfe858339ccef51a95016205ef80
service_host_sha256: b7cd066b56cacd5ab14f137a23fd41c8df62290eb1975094fb2ecb2eab1bca92
product_wrapper_sha256: da28034482d641f9c78414289e9f55b9d392de6e475575ac360b0ba15dfb2fbb
wix_version: 5.0.2+aa65968c
```

`0.42.8-admin-smoke`는 이전 product payload package build다. 다만 이 evidence는
full admin host mutation PASS가 아니므로 최신 full host mutation claim은 계속
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`
가 소유한다.

## 0.42.7 Package-pair Triage

`0.42.7-admin-smoke` 단독 manual-admin package-pair campaign은 실행하지 않는다.
이 변경이 `0.42.8-admin-smoke` payload를 새로 만들었으므로, package-pair campaign이
필요하면 다음 후보는 `0.42.7-admin-smoke -> 0.42.8-admin-smoke`다.

현재 닫힌 package-pair PASS claim은 계속
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`의
`0.42.5-admin-smoke -> 0.42.6-admin-smoke`가 소유한다.

## Zone Wiki

Canonical zone wiki path `/data/projects/codex-zone/wiki/index.md`,
project-mgmt wiki path `/data/projects/codex-zone/codex-project-mgmt/wiki/index.md`,
project-local `wiki/index.md`는 이 workspace에 없었다. 따라서 별도 wiki 파일은
수정하지 않고, canonical 기록은 이 evidence와 GA-ready index에 둔다.

## 검증

- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostOptionsTests|FullyQualifiedName~DesktopNodeHostServiceActionTests"`: PASS, 112 tests
- `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1" -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }'`: PASS, 25 tests
- `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1" -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }'`: PASS, 9 tests
- `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.8-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260512-0428 -SigningMode AllowUnsignedDev -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: PASS

## 경계

이 evidence는 internal/admin-smoke package build와 code-level service configuration
contract evidence다. Public trusted signing, external stable publication, winget
submission, public stable installer URL, public clean-host signed install/update/rollback
smoke를 주장하지 않는다.

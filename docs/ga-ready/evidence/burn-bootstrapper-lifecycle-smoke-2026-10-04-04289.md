# Burn bootstrapper lifecycle smoke `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-04-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20261004-04289`
preupdate_root: `artifacts/burn-bootstrapper-lifecycle-20261004-04289-preupdate`
bundle: `artifacts/burn-bootstrapper-lifecycle-20261004-04289/PureCVisorDesktopNode-0.42.89-admin-smoke-bootstrapper.exe`
bundle_sha256: `b014e4ed77fe40bfa1ea1a1b81d9a7006766d98755ecade3a2e2e8c11d8957d5`
bundle_bytes: `57235279`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `afceef503555269dade8e5cbaaf4c6518fbe115fa78a033429b2201d94438862`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.89` pair의 Burn bucket이다. update/rollback 뒤 설치본은 baseline `0.42.88-admin-smoke`였다. Burn prestate는 target과 같아야 하므로 제품 Update로 `0.42.89-admin-smoke`에 맞췄다(`preupdate_root/pre-update.json`, exit `0`, `ok=true`). 15초를 기다린 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `32`초).

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20261004-04289
-TargetMsiPath artifacts/admin-smoke-package-20261004-04289/PureCVisorDesktopNode-0.42.89-admin-smoke-windows-x64.msi
-TargetVersion 0.42.89-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute
```

| 단계 | exit |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0` |
| `/repair /quiet /norestart` | `0` |
| `/uninstall /quiet /norestart` | `0` |
| target MSI 복구 | `0` |

lifecycle summary는 `ok=true`, `status=PASS`, `restoration_status=PASS`다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{0E27390F-5A08-4F76-90FD-7694B41672A4}` `0.42.89` (1개) |
| product manifest | `0.42.89-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.89-admin-smoke+b463903153a6ffe75b438e673c231e78d4c99909` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.88-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

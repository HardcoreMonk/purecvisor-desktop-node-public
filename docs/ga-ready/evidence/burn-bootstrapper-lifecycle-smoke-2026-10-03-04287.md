# Burn bootstrapper lifecycle smoke `0.42.87-admin-smoke` (2026-10-03)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-03-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20261003-04287`
bundle: `artifacts/burn-bootstrapper-lifecycle-20261003-04287/PureCVisorDesktopNode-0.42.87-admin-smoke-bootstrapper.exe`
bundle_sha256: `7cdafadcfba70ed0913e4a4be21eaf80710f112978e7989e255c532f9a5d35ce`
bundle_bytes: `57246191`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `45a802c432fb479b0efa1599062ec9814cf73ae220575971b69d1656c7bc0bb6`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

update/rollback 뒤 설치본은 baseline `0.42.86-admin-smoke`였다. Burn prestate는 target과 같아야 하므로 제품 Update로 `0.42.87-admin-smoke`에 맞췄다(`pre-update.json`, exit `0`, `ok=true`). 서비스가 파일을 놓도록 15초를 기다린 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `43`초). 이번에는 repair `3010`이 나오지 않았다.

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20261003-04287
-TargetMsiPath artifacts/admin-smoke-package-20261003-04287/PureCVisorDesktopNode-0.42.87-admin-smoke-windows-x64.msi
-TargetVersion 0.42.87-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute
```

| 단계 | exit |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0` |
| `/repair /quiet /norestart` | `0` |
| `/uninstall /quiet /norestart` | `0` |
| target MSI 복구 | `0` |

lifecycle summary는 `ok=true`, `status=PASS`, `restoration_status=PASS`다. install log에서 `RemoveExistingProducts`가 실행되어 `0.42.86` MSI 제품을 major upgrade로 지웠다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{5EF95755-746A-494F-A160-ECCE7AAC2C80}` `0.42.87` (1개) |
| product manifest | `0.42.87-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.87-admin-smoke+8d940dab1e2aae1ad1e4cac5def45eb26a4343ae` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- 여기서 일어난 major upgrade는 다른 version(`0.42.86 → 0.42.87`)이다. 같은 version 재설치(A)는 Task 4 fullgate가 실증한다.
- operational current는 `0.42.86-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

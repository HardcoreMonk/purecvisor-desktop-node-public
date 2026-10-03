# Burn bootstrapper lifecycle smoke `0.42.88-admin-smoke` (2026-10-03)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-03-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20261003-04288`
preupdate_root: `artifacts/burn-bootstrapper-lifecycle-20261003-04288-preupdate`
bundle: `artifacts/burn-bootstrapper-lifecycle-20261003-04288/PureCVisorDesktopNode-0.42.88-admin-smoke-bootstrapper.exe`
bundle_sha256: `b67df01805c89b6cae4b43e302756261f10bfecf269bf3dc056d938176fa0ac8`
bundle_bytes: `57271789`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `d543946b5b003e59c5a9b507bd1d8d37a0f1827fe3bb7fd51ae72410b097d2fd`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

update/rollback 뒤 설치본은 baseline `0.42.87-admin-smoke`였다. Burn prestate는 target과 같아야 하므로 제품 Update로 `0.42.88-admin-smoke`에 맞췄다(`preupdate_root/pre-update.json`, exit `0`, `ok=true`). 15초를 기다린 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `31`초).

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20261003-04288
-TargetMsiPath artifacts/admin-smoke-package-20261003-04288/PureCVisorDesktopNode-0.42.88-admin-smoke-windows-x64.msi
-TargetVersion 0.42.88-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{5B28DA82-3E76-4FE6-A11C-97B38A18F487}` `0.42.88` (1개) |
| product manifest | `0.42.88-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.88-admin-smoke+ff62e596a949202d905699504cd12ce3c646dc0c` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.87-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

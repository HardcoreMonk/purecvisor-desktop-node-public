# Burn bootstrapper lifecycle smoke `0.42.86-admin-smoke` (2026-10-02)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-02-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20261002-04286`
bundle: `artifacts/burn-bootstrapper-lifecycle-20261002-04286/PureCVisorDesktopNode-0.42.86-admin-smoke-bootstrapper.exe`
bundle_sha256: `db5adc8a07010beeb3c93d86214ab08b5d86954798a61ff3fd4da441ab21d452`
bundle_bytes: `57220305`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `d2296bca7d4a829252497476a29f6b135873c503c1b025d824ae8c91ba96f5a3`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

update/rollback 직후 설치본은 baseline `0.42.85-admin-smoke`였다. Burn prestate는 target과 같아야 하므로 제품 Update로 `0.42.86-admin-smoke`에 맞췄다(`pre-update.json`, exit `0`, `ok=true`). 서비스가 파일을 놓은 뒤 15초를 기다리고 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다.

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20261002-04286
-TargetMsiPath artifacts/admin-smoke-package-20261002-04286/PureCVisorDesktopNode-0.42.86-admin-smoke-windows-x64.msi
-TargetVersion 0.42.86-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{EE323D7C-2343-4110-958A-0453E5D1D39A}` `0.42.86` (1개) |
| product manifest | `0.42.86-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.84-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Burn bootstrapper lifecycle smoke `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-06-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261006-04290-04291/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261006-04290-04291/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.91-admin-smoke-bootstrapper.exe`
bundle_sha256: `274e3beb4c97e5b3aec5cf164f3966fc8ea530dd391538ee8c9c87026882b84c`
bundle_bytes: `57234171`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `6fe05db41920ed837138299641038b7d8a9ecfce94f9396544fe3a9563e39f24`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.91` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.91-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `39`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261006-04290-04291/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261006-04291/PureCVisorDesktopNode-0.42.91-admin-smoke-windows-x64.msi
-TargetVersion 0.42.91-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -InstalledManifestPath <installed product-manifest.json> -ServiceName PureCVisorDesktopNode -Execute
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
| ARP `PureCVisor Desktop Node` | `{AC7292AD-3E1D-4512-BF63-996F831EF90B}` `0.42.91` (1개) |
| product manifest | `0.42.91-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.91-admin-smoke+59cd8b6cc1d95c9b4f3a566354361b3efd6076c5` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.90-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

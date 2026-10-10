# Burn bootstrapper lifecycle smoke `0.42.96-admin-smoke` (2026-10-11)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-11-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261011-04295-04296/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.96-admin-smoke-bootstrapper.exe`
bundle_sha256: `e6958074fb461d868b6908bbcdaf6d799699ff661109ed9e55a1be8797cfce68`
bundle_bytes: `62145193`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `44cc923a3f5c886fe077a5638782101154c17114bc1c8c4d10e14d36b930a3b2`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.96` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.96-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `36`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261011-04295-04296/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261011-04296/PureCVisorDesktopNode-0.42.96-admin-smoke-windows-x64.msi
-TargetVersion 0.42.96-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{EBCDF75B-F5A7-49D1-92BE-84F442F67E99}` `0.42.96` (1개) |
| product manifest | `0.42.96-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.93-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

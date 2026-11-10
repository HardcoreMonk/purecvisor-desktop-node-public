# Burn bootstrapper lifecycle smoke `0.43.4-admin-smoke` (2026-11-11)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-11-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04303-04304/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261111-04303-04304/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.43.4-admin-smoke-bootstrapper.exe`
bundle_sha256: `603e622a591047e38e1a6565aa15585e5a7fcbe393729899c21161066b357c65`
bundle_bytes: `62193345`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `84685e9e076d136d6543ea873a486f1d9eef357dfff49092079e1c83c9aa09fb`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.4` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.43.4-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `37`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261111-04303-04304/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261111-04304/PureCVisorDesktopNode-0.43.4-admin-smoke-windows-x64.msi
-TargetVersion 0.43.4-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{BF2AD1AA-4798-4D28-A433-172DBEC6D9A8}` `0.43.4` (1개) |
| product manifest | `0.43.4-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.4-admin-smoke+54a817c4ed0a8ab14056405310a998571c99d578` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.3-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

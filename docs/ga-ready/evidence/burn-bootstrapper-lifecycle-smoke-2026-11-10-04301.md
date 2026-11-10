# Burn bootstrapper lifecycle smoke `0.43.1-admin-smoke` (2026-11-10)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-10-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04300-04301/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261110-04300-04301/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.43.1-admin-smoke-bootstrapper.exe`
bundle_sha256: `cf09282a4a4d8a547dee74bf29d3f40acca0ba67f375e6a2848c0ff402e4e7f2`
bundle_bytes: `62201947`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `f71370d199d41a97c608838d107f02a35e1f2aa69c2dc42e8dd23b69e3742f5e`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.0-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.1` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.43.1-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `43`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261110-04300-04301/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261110-04301/PureCVisorDesktopNode-0.43.1-admin-smoke-windows-x64.msi
-TargetVersion 0.43.1-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{31E528BD-FAE4-4C29-AA38-78E968E1D1AA}` `0.43.1` (1개) |
| product manifest | `0.43.1-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.1-admin-smoke+0be6561e0a22b5163a7214e9de9b5ebb889778cc` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.0-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

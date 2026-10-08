# Burn bootstrapper lifecycle smoke `0.42.93-admin-smoke` (2026-10-08)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-08-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261008-04292-04293/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261008-04292-04293/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.93-admin-smoke-bootstrapper.exe`
bundle_sha256: `45c9c9934e1bed963666425465647d1529b5c6b04c204fdcad2d52eb635b289e`
bundle_bytes: `57247787`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `78599762170ffb6f4cf8cc3b2e00aceb7136284e13b5c1af013c95e285beecd6`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.93` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.93-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `35`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261008-04292-04293/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261008-04293/PureCVisorDesktopNode-0.42.93-admin-smoke-windows-x64.msi
-TargetVersion 0.42.93-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{867BA47E-0553-4B74-8E46-8C803008F61C}` `0.42.93` (1개) |
| product manifest | `0.42.93-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.93-admin-smoke+41421d8bf3272dbdbb8dcf384bcf1ff3f329726a` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.92-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Burn bootstrapper lifecycle smoke `0.43.7-admin-smoke` (2026-11-11)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-11-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.43.7-admin-smoke-bootstrapper.exe`
bundle_sha256: `bdf5f0ec3b33aa381f60643c87d172a804761e7589799c639399b54d334460c3`
bundle_bytes: `62214461`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `36c85f450af9e89130587c0f10a78e7f20be53045d01fa6f68b0afacdc8678d4`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.7` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.43.7-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `50`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261111-04306-04307-r2/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261111-04307/PureCVisorDesktopNode-0.43.7-admin-smoke-windows-x64.msi
-TargetVersion 0.43.7-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{1C067086-B542-4932-93AC-F05B6DF50D48}` `0.43.7` (1개) |
| product manifest | `0.43.7-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.7-admin-smoke+74b956256c585c032536a117281dadc128d9794a` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

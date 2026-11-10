# Burn bootstrapper lifecycle smoke `0.43.2-admin-smoke` (2026-11-10)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-10-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04301-04302/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261110-04301-04302/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.43.2-admin-smoke-bootstrapper.exe`
bundle_sha256: `06c71dfd8b307385b5f0c7b9170e808de711c81e5b996f7c7062719ad18fb5ff`
bundle_bytes: `62169701`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `dabec7560aeef2c455263ac6bfb5f7c573e763817b021ddcf197a325af5c5bcd`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.2` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.43.2-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `50`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261110-04301-04302/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261110-04302/PureCVisorDesktopNode-0.43.2-admin-smoke-windows-x64.msi
-TargetVersion 0.43.2-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{F5F4DAE1-D579-4070-B290-F7D5C2DCCE93}` `0.43.2` (1개) |
| product manifest | `0.43.2-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.2-admin-smoke+b45e8215db21ecd952da38d4c0ebbdbc194a4d89` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.1-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

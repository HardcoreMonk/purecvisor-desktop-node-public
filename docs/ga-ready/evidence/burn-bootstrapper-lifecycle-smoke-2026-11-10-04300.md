# Burn bootstrapper lifecycle smoke `0.43.0-admin-smoke` (2026-11-10)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-10-04300`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04299-04300/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261110-04299-04300/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.43.0-admin-smoke-bootstrapper.exe`
bundle_sha256: `a31c4187b051005bea508e604ac8e9fab61b877ee3723828625f6d4673ccd594`
bundle_bytes: `62200281`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `36f525b93ceaa95c7e89419cab8fb0cc31b5d0038aff861c218e597fafbc278d`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.0` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.43.0-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `49`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261110-04299-04300/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261110-04300/PureCVisorDesktopNode-0.43.0-admin-smoke-windows-x64.msi
-TargetVersion 0.43.0-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{C5E6067A-783A-4468-90A1-D8AB232814B7}` `0.43.0` (1개) |
| product manifest | `0.43.0-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.0-admin-smoke+87eb923659ff55d24cf30674b27dd32b19be8c13` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.99-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

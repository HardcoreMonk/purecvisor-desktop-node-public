# Burn bootstrapper lifecycle smoke `0.42.90-admin-smoke` (2026-10-05)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-10-05-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261005-04289-04290/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261005-04289-04290/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.90-admin-smoke-bootstrapper.exe`
bundle_sha256: `3647d54e7c96d60a4fed2993941b472b64ebed0f3f40d1b86a454d0ac3dbde48`
bundle_bytes: `57250135`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `7f197bc7c23a31036e3b16c2151472001aaac9ae706262533535bef1dcd69f62`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.90` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.90-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `49`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261005-04289-04290/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261005-04290/PureCVisorDesktopNode-0.42.90-admin-smoke-windows-x64.msi
-TargetVersion 0.42.90-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{8AE1F6ED-2174-4484-A57B-E60378BB1F72}` `0.42.90` (1개) |
| product manifest | `0.42.90-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.90-admin-smoke+0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.89-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

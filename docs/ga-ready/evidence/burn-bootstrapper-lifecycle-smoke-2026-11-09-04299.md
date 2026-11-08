# Burn bootstrapper lifecycle smoke `0.42.99-admin-smoke` (2026-11-09)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-09-04299`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261109-04298-04299/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261109-04298-04299/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.99-admin-smoke-bootstrapper.exe`
bundle_sha256: `9301343cbe22d86b54989b03dfa7f0cfd50ab83477a51f9ac367dc7976c51c59`
bundle_bytes: `62166985`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `990fca275802640d1a3acf58a2d52c869955696041b4c7d7978b5a3736132eed`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.99` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.99-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `39`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261109-04298-04299/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261109-04299/PureCVisorDesktopNode-0.42.99-admin-smoke-windows-x64.msi
-TargetVersion 0.42.99-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{004AC8CC-50E9-4EBE-A041-BC6C6986C351}` `0.42.99` (1개) |
| product manifest | `0.42.99-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.99-admin-smoke+4931d6d59c45a0775f443aa87ea5fe9208467203` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.98-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

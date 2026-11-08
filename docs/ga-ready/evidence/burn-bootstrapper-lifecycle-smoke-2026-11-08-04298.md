# Burn bootstrapper lifecycle smoke `0.42.98-admin-smoke` (2026-11-08)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-11-08-04298`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261108-04296-04298/burn-bootstrapper-lifecycle`
bundle: `artifacts/manual-admin-campaign-20261108-04296-04298/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.98-admin-smoke-bootstrapper.exe`
bundle_sha256: `190a88d70d8c228e4e1602f5bb990e085bcf10fcdb87b94de59b0ebaa2523e77`
bundle_bytes: `62161849`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `6b85b6225258185a95332404b2a4020b0ddf038b666c7633537544b0c1f8f11f`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.98` pair orchestrator의 Burn bucket이다. 직전 update/rollback bucket이 target `0.42.98-admin-smoke`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `56`초).

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261108-04296-04298/burn-bootstrapper-lifecycle
-TargetMsiPath artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-windows-x64.msi
-TargetVersion 0.42.98-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
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
| ARP `PureCVisor Desktop Node` | `{625D101D-B310-4820-AA39-1AB4352C8E05}` `0.42.98` (1개) |
| product manifest | `0.42.98-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.98-admin-smoke+54cb61ef3128df7d57191aefd0f9a684b8de2f8f` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.96-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

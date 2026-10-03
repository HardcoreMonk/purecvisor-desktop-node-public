# Burn bootstrapper lifecycle smoke `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-09-30-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20260930-04284`
bundle: `artifacts/burn-bootstrapper-lifecycle-20260930-04284/PureCVisorDesktopNode-0.42.84-admin-smoke-bootstrapper.exe`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
summary_sha256: `d8c5e78b130879a25e018984d022ea77f5d578a3e93d98de67b147e4151886af`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

먼저 설치본을 제품 Update(`Invoke-PcvDesktopNodeProduct.ps1 -Action Update`)로 `0.42.84-admin-smoke`에 맞췄다. 결과는 `pre-update.json`, `ok=true`, `0.42.83 → 0.42.84`다. 그 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`가 WiX Burn bundle을 만들고 install, repair, remove를 실행했다. 실행 시간은 2026-09-30 `06:13:13Z`부터 `06:13:45Z`까지다. 마지막으로 lifecycle 복구가 target MSI `0.42.84`를 다시 설치했다.

```text
-ArtifactRoot artifacts/burn-bootstrapper-lifecycle-20260930-04284
-TargetMsiPath artifacts/admin-smoke-package-20260930-04284/PureCVisorDesktopNode-0.42.84-admin-smoke-windows-x64.msi
-TargetVersion 0.42.84-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute
```

| 단계 | exit / 상태 |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0`, manifest `0.42.84`, Running/Automatic |
| `/repair /quiet /norestart` | `0`, manifest `0.42.84`, Running/Automatic |
| `/uninstall /quiet /norestart` | `0`, product root, service, manifest 모두 없음 |
| target MSI 복구 | `0` |

lifecycle summary는 `ok=true`, `status=PASS`, `restoration_status=PASS`다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{F50C37FD-D2CB-465D-83B8-1693DE0B6772}` `0.42.84` (1개) |
| product manifest | `0.42.84-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |

0.42.83 run과 같이 target(`0.42.84`)을 설치된 채로 둔다. 다음 task(MSIX, fullgate, current-card)가 target 설치본을 쓰기 때문이다.

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| MSIX build/install/update/remove | `not-run` |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.83-admin-smoke` |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.83-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Burn install/repair/remove `0.42.83-admin-smoke`

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-09-29-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20260929-04283`
bundle: `artifacts/burn-bootstrapper-lifecycle-20260929-04283/lifecycle/PureCVisorDesktopNode-0.42.83-admin-smoke-bootstrapper.exe`
bundle_sha256: `47e391bfe79d639b1a9dd851503949def8d1e69ad31482b0cbae34ae1cb5ea12`
summary_sha256: `63ec3d76858927626998ce52e25cc5730b24a9adba66f92b724965234dda01c8`
target_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1`
runner_sha256: `d9841416184e8e17c8aaacd15cac4592c61948e8d94cd9242eadce8ae9216808`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner를 `f3ac8db`에서 저장소에 들여왔다. 0.42.78 run 때 미추적 worktree에서 실행한 파일과 SHA가 같다.

먼저 설치본을 제품 Update(`Invoke-PcvDesktopNodeProduct.ps1 -Action Update`)로 `0.42.83-admin-smoke`에 맞췄다. 결과는 `pre-update.json`, `ok=true`, `0.42.78 → 0.42.83`이다. 그 뒤 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`가 WiX Burn bundle을 만들고 install, repair, remove를 실행했다. 실행 시간은 2026-09-29 `14:10:26Z`부터 `14:11:10Z`까지다. 마지막으로 lifecycle 복구가 target MSI `0.42.83`을 다시 설치하고 `RepairInstalled`를 실행했다.

```text
-TargetMsiPath artifacts/admin-smoke-package-20260929-04283/PureCVisorDesktopNode-0.42.83-admin-smoke-windows-x64.msi
-TargetVersion 0.42.83-admin-smoke -WixPath <user>/.dotnet/tools/wix.exe
-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Execute
```

| 단계 | exit / 상태 |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0`, manifest `0.42.83`, Running |
| `/repair /quiet /norestart` | `0`, manifest `0.42.83`, Running |
| `/uninstall /quiet /norestart` | `0`, product root, service, manifest 모두 없음 |
| target MSI 복구 | `0`, `RepairInstalled` 통과 |

lifecycle summary는 `ok=true`, `status=PASS`, `restoration_status=PASS`다. 최종 service는 `Running` / `Automatic`, manifest는 `0.42.83-admin-smoke`다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{D74A4692-388C-40CD-81F7-036E3CA61840}` `0.42.83` (1개) |
| product manifest | `0.42.83-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |

0.42.78 run은 끝에 baseline MSI를 다시 설치해 호스트를 되돌렸다. 이번에는 `0.42.83`을 설치된 채로 둔다. 다음 task(MSIX, fullgate, current-card)가 target 설치본을 쓰기 때문이다.

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| MSIX build/install/update/remove | `not-run` |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.78-admin-smoke` |

## Nonclaims

- Burn bundle은 내부 smoke용이며 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.78-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Burn install/repair/remove `0.42.78-admin-smoke`

evidence_id: `burn-bootstrapper-lifecycle-smoke-2026-09-25-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/burn-bootstrapper-lifecycle-20260925-04278`
bundle: `artifacts/burn-bootstrapper-lifecycle-20260925-04278/lifecycle/PureCVisorDesktopNode-0.42.78-admin-smoke-bootstrapper.exe`
bundle_sha256: `9be9126c8e5d12f54dad06587effc405b0c8d965c66a88538e03dfaac5c34401`
target_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
baseline_restore_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
wix_version: `5.0.2+aa65968c`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

호스트 product manifest를 filesystem update로 `0.42.78-admin-smoke`에 맞춘 뒤
`Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`가 WiX Burn bundle을 만들고
install, repair, remove를 실행했다. lifecycle 복구는 대상 MSI `0.42.78`을 다시
설치했다. 그 다음 그 제품을 제거하고 기준 MSI `0.42.77-admin-smoke`를 설치해
호스트를 되돌렸다.

| 단계 | exit |
| --- | --- |
| bundle build | `0` |
| `/install /quiet /norestart` | `0` |
| `/repair /quiet /norestart` | `0` |
| `/uninstall /quiet /norestart` | `0` |
| 대상 MSI 복구 | `0` |
| 기준 MSI 제거 후 재설치 | `0` |

lifecycle summary는 `ok=true`, `status=PASS`, 최종 서비스 `Running` /
`Automatic`, manifest `0.42.78-admin-smoke`였다. 기준 복구 후 호스트는
`0.42.77-admin-smoke`다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.77-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.77-admin-smoke+04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6` |
| MSI DisplayVersion | `0.42.77` |
| MSI product code | `{A0E5684C-2191-4003-9DDE-64EE845339ED}` |
| service | `PureCVisorDesktopNode` Running |
| VM | `pcv-cleanhost-20260910-r2-04274-04275` Saved, `pcv-guest-installed-04253-r1` Off |

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| MSIX build/install/update/remove | `not-run` |
| installed runtime ops summary | `not-run` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.77-admin-smoke -> 0.42.78-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 이 기록은 내부 AllowUnsignedDev Burn lifecycle만 증명한다.
- operational current를 `0.42.78-admin-smoke`로 올리지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

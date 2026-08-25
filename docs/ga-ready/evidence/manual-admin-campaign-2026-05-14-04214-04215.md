# MANUAL-ADMIN 캠페인 2026-05-14 04214->04215

```text
evidence_id: manual-admin-campaign-2026-05-14-04214-04215
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.14-admin-smoke
target_version: 0.42.15-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.14-admin-smoke -> 0.42.15-admin-smoke` package-pair를
manual-admin 운영 단위로 닫는다. Installed update/rollback, Windows Update 포함
dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX
build/install/update/remove, installed runtime ops summary capture, manual-admin
descriptor generation이 모두 PASS다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260514-04214-selectorfix` |
| baseline MSI | `PureCVisorDesktopNode-0.42.14-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb` |
| target package root | `artifacts/admin-smoke-package-20260514-04215-clean` |
| target MSI | `PureCVisorDesktopNode-0.42.15-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7` |
| target provenance commit | `8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff` |
| target payload aggregate SHA-256 | `9318522dbf926746a758547f30cdfb9c6b528cbc2744052f65219330a691aab1` |
| service host SHA-256 | `f8ce7de453e8e753bd68b78373215a24917d9c7cbf0900a40a857a50f7435670` |
| CLI SHA-256 | `39f6c68278ce52ab6c0a3138f4232bcc97735834ca8b3544fdb5f63b21dfc40b` |
| TUI SHA-256 | `a06d3f52d29dbbeb7056957ab667bf0a35648a883846d3becf2501b02bf7ae06` |
| signing mode | `AllowUnsignedDev` |
| update package | `artifacts/manual-admin-campaign-20260514-04214-04215/lifecycle/PureCVisorDesktopNode-0.42.15-admin-smoke-update.zip` |
| update package SHA-256 | `06f5879431bac90da6da09f243c1e91c6bb875358779e4cedc98a9a3860dad6b` |

## Campaign Descriptor 산출물

- Descriptor root:
  `artifacts/manual-admin-campaign-20260514-04214-04215/manual-admin-campaign-descriptor-supervised`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`,
  `not_pass_count=0`
- Batch manifest:
  `artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04214-04215/manifest.json`
- Batch summary: `ok=true`, `status=completed`, `executed_steps=1`

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260514-04214-04215/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260514-04214-04215/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update-nocontact-recovery` | `artifacts/manual-admin-campaign-20260514-04214-04215/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260514-04214-04215/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260514-04214-04215/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260514-04214-04215/installed-runtime-ops-summary/summary.json` |

## Clean-host 핵심 결과

- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.14-admin-smoke`
- Updated manifest: `0.42.15-admin-smoke`
- Final manifest: `0.42.14-admin-smoke`
- Install/update/rollback exit code: `0` / `0` / `0`
- Final service: `Running`
- Final Web Console: HTTP `200`
- Blocker: `none`
- Windows Update: KB `5087545`, `update_count=1`, reboot performed
- Windows Update NoContact recovery:
  `automatic_recovery_performed=true`, `recovery_actions=1`,
  `threshold_seconds=900`

## Installed Runtime Ops Summary

`0.42.15-admin-smoke` target MSI를 설치한 뒤 product wrapper
`RepairInstalled -BatchEvidenceRoot artifacts`를 실행하고 installed `pcvcli.exe
--protected-token-file ... --format json ops summary`를 캡처했다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260514-04214-04215/installed-runtime-ops-summary`
- Installed manifest: `0.42.15-admin-smoke`
- `batch_evidence.status`: `available`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- `installed_runtime.evidence_anchor`:
  `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
- Service PathName `--batch-evidence-root`: `true`

이 installed runtime capture는 descriptor batch가 최신 `batch-runs` 항목인 상태에서도
selector guard가 descriptor를 operational latest로 선택하지 않는다는 회귀 조건을
다시 확인한다. 이후 `0.42.15-admin-smoke` full admin host mutation gate가 별도로
PASS하면서 current-card 최신 anchor는 04215 full gate로 승격됐다.

## 판정

`0.42.14 -> 0.42.15` package-pair는 installed update/rollback, clean-host,
Burn, MSIX, installed runtime summary, descriptor 기준으로 PASS다. 이 evidence는
internal/admin-smoke host mutation evidence이며 public trusted signing, winget,
external stable publication, public clean-host release claim은 추가하지 않는다.

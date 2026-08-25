# MANUAL-ADMIN 캠페인 2026-05-14 04212->04213

```text
evidence_id: manual-admin-campaign-2026-05-14-04212-04213
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.12-admin-smoke
target_version: 0.42.13-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair를
manual-admin 운영 단위로 닫는다. Installed update/rollback, Windows Update 포함
dedicated clean-host install/update/rollback, Burn install/repair/remove, MSIX
build/install/update/remove, installed runtime ops summary capture, manual-admin
descriptor generation이 모두 PASS다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260513-04212` |
| baseline MSI | `PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e` |
| target package root | `artifacts/admin-smoke-package-20260514-04213` |
| target MSI | `PureCVisorDesktopNode-0.42.13-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a` |
| target provenance commit | `a28bb808386f206c9dbf7dcaeee232eacb648434` |
| signing mode | `AllowUnsignedDev` |
| update package | `artifacts/manual-admin-campaign-20260514-04212-04213/lifecycle/PureCVisorDesktopNode-0.42.13-admin-smoke-update.zip` |
| update package SHA-256 | `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9f700b83059a8c9` |

## Campaign Descriptor 산출물

- Descriptor root:
  `artifacts/manual-admin-campaign-20260514-04212-04213/manual-admin-campaign-descriptor-supervised`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`,
  `not_pass_count=0`
- Batch manifest:
  `artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04212-04213/manifest.json`
- Batch summary: `ok=true`, `status=completed`, `executed_steps=1`

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260514-04212-04213/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260514-04212-04213/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update-nocontact-recovery` | `artifacts/manual-admin-campaign-20260514-04212-04213/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260514-04212-04213/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260514-04212-04213/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260514-04212-04213/installed-runtime-ops-summary/summary.json` |

## Clean-host 핵심 결과

- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.12-admin-smoke`
- Updated manifest: `0.42.13-admin-smoke`
- Final manifest: `0.42.12-admin-smoke`
- Install/update/rollback exit code: `0` / `0` / `0`
- Final service: `Running`
- Final Web Console: HTTP `200`
- Blocker: `none`
- Windows Update NoContact recovery:
  `automatic_recovery_performed=true`, `recovery_actions=1`,
  `threshold_seconds=900`

## Installed Current-card 후속

Descriptor batch 생성 후 canonical `artifacts` root에서 최신 `batch-runs` 항목이
`manual-admin-campaign-descriptor-20260514-04212-04213`가 되면서, 기존 설치본
current-card가 descriptor summary를 운영 evidence처럼 선택하는 회귀를 발견했다.
이 회귀는
`docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`
에서 source guard, `0.42.14-admin-smoke` package, 설치본 Web Console current-card
smoke로 닫았다.

## 판정

`0.42.12 -> 0.42.13` package-pair는 installed update/rollback, clean-host,
Burn, MSIX, installed runtime summary, descriptor 기준으로 PASS다. 이 evidence는
internal/admin-smoke host mutation evidence이며 public trusted signing, winget,
external stable publication, public clean-host release claim은 추가하지 않는다.

# MANUAL-ADMIN 캠페인 2026-05-14 04211->04212

```text
evidence_id: manual-admin-campaign-2026-05-14-04211-04212
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.11-admin-smoke
target_version: 0.42.12-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.11-admin-smoke -> 0.42.12-admin-smoke` package-pair를
manual-admin 운영 단위로 닫는다. Installed update/rollback, wrapper repair
`BatchEvidenceRoot` 재설정, Windows Update 포함 dedicated clean-host
install/update/rollback, Burn install/repair/remove, MSIX build/install/update/remove,
installed runtime ops summary capture, manual-admin descriptor generation이 모두 PASS다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260513-04211` |
| baseline MSI | `PureCVisorDesktopNode-0.42.11-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1` |
| target package root | `artifacts/admin-smoke-package-20260513-04212` |
| target MSI | `PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e` |
| target provenance commit | `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3` |
| signing mode | `AllowUnsignedDev` |
| update package | `artifacts/manual-admin-campaign-20260514-04211-04212/lifecycle/PureCVisorDesktopNode-0.42.12-admin-smoke-update.zip` |
| update package SHA-256 | `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04` |

## Campaign Descriptor 산출물

- Descriptor root:
  `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`,
  `not_pass_count=0`
- Batch manifest:
  `artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04211-04212/manifest.json`
- Batch summary: `ok=true`, `status=completed`, `executed_steps=1`

## 설치본 Update/Rollback

Installed-host product update/rollback은 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260514-04211-04212/lifecycle/product-update-rollback`
- Update PASS:
  `02-update.json`
- Wrapper repair PASS:
  `04-wrapper-repair-installed-batch-root.json`
- Rollback PASS:
  `05-rollback.json`
- Direct native repair after rollback PASS:
  `07-direct-native-repair-after-rollback.json`
- Summary:
  `summary.json`

Update action은 `Update`, rollback action은 `Rollback`이며 둘 다 `ok=true`다.
Wrapper repair 이후 service `PathName`은 campaign root를 포함했고, rollback 이후
direct native repair는 canonical `artifacts` root를 다시 적용했다.

## Dedicated Clean-host 검증

Dedicated clean-host install/update/rollback은 Windows Update 적용 후 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260514-04211-04212/clean-host-updated-os`
- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.11-admin-smoke`
- Updated manifest: `0.42.12-admin-smoke`
- Final manifest: `0.42.11-admin-smoke`
- Install/update/rollback exit code: `0` / `0` / `0`
- Final Web Console: HTTP `200`
- Blocker: `none`
- Windows Update preparation: `ok=true`, `reboot_performed=true`
- Applied OS update: `KB5087545`, post-update UBR `5139`

Windows Update reboot 이후 VM heartbeat가 `NoContact`이고 CPU가 장시간 idle 상태로
멈춰 수동 power cycle을 수행했다. 이 recovery는
`artifacts/manual-admin-campaign-20260514-04211-04212/clean-host-updated-os/manual-forced-restart-after-windows-update-hang.json`
에 기록했으며, 이후 PowerShell Direct가 회복되어 guest package-pair smoke가 PASS로
완료됐다. VM은 성공 후 제거됐다.

## Burn Bootstrapper 검증

Burn bootstrapper lifecycle은 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260514-04211-04212/burn-bootstrapper-lifecycle`
- Bundle version: `0.42.12.0`
- Bundle SHA-256:
  `24c96796baa8e890f96394fd6971232fa73e00e0c99395260028a86618378958`
- Install exit code: `0`
- Repair exit code: `0`
- Remove exit code: `0`
- Baseline restore exit code: `0`
- Final restore manifest: `0.42.11-admin-smoke`

## MSIX Lifecycle 검증

MSIX internal package lifecycle은 PASS다.

- Artifact root: `artifacts/msix-package-lifecycle-smoke-20260514-04211-04212`
- Result: `build-install-update-remove-pass-internal-smoke`
- v1 package SHA-256:
  `3e1d0f154f082c4db4aefa8c6ac3810f6248350ba0b7fea3f7f6f4ad4639c76c`
- v2 package SHA-256:
  `93e0b0f60120e45fb35c13011f37c27e2a5eb7dffcdf60883b8adc0d7f4e945e`
- Final MSIX package/service absence: PASS
- Existing MSI service final state: `Running`

이 MSIX evidence는 internal smoke이며 public trusted signing 또는 external stable
publication evidence가 아니다.

## 설치본 Runtime Ops Summary

Installed runtime ops summary capture는 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260514-04211-04212/installed-runtime-ops-summary`
- Installed manifest: `0.42.12-admin-smoke`
- Service state: `Running`
- Service path: `--batch-evidence-root` 포함
- `batch_evidence.status`: `available`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260513-04212`
- `latest.release.version`: `0.42.12-admin-smoke`
- `latest.release.msi_sha256`:
  `74735f98bb7afbaa46127eddb200a3de6e5a954b240d7a65578072960368e233`
- `installed_runtime.evidence_anchor`: `full-admin-host-mutation-gate-20260513-04212`
- `PCV_AUTH_REQUIRED` boundary 유지

캠페인 종료 시 설치본은 target `0.42.12-admin-smoke`로 복구했고, product wrapper
`RepairInstalled -BatchEvidenceRoot`를 다시 실행해 current-card root를 canonical
`artifacts`로 맞췄다.

## 판정

`0.42.11 -> 0.42.12` package-pair는 installed update/rollback, wrapper repair,
dedicated clean-host, Burn, MSIX, installed runtime summary, descriptor 기준으로 PASS다.

이 evidence는 internal/admin-smoke host mutation evidence다. Public trusted signing,
winget, external stable publication, public clean-host release claim은 추가하지 않는다.

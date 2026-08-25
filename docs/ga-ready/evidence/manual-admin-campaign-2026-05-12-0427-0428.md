# MANUAL-ADMIN 캠페인 2026-05-12 0427→0428

```text
evidence_id: manual-admin-campaign-2026-05-12-0427-0428
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.7-admin-smoke
target_version: 0.42.8-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.7-admin-smoke -> 0.42.8-admin-smoke` package-pair를
manual-admin 운영 단위로 재검증한다. Installed update/rollback, dedicated
clean-host install/update/rollback, Burn install/repair/remove, MSIX
build/install/update/remove, installed runtime ops summary capture, manual-admin
descriptor generation을 모두 PASS로 닫았다.

이 문서는 internal/admin-smoke 운영 evidence다. Public trusted signing, winget,
external stable publication, public clean-host release claim은 추가하지 않는다.

## 패키지 입력

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260512-0427` |
| baseline MSI | `PureCVisorDesktopNode-0.42.7-admin-smoke-windows-x64.msi` |
| baseline MSI SHA-256 | `256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9` |
| target package root | `artifacts/admin-smoke-package-20260512-0428-postmerge` |
| target MSI | `PureCVisorDesktopNode-0.42.8-admin-smoke-windows-x64.msi` |
| target MSI SHA-256 | `e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687` |
| target provenance commit | `5397e580c98a34e8b7beb5b9773d1d857025315b` |
| signing mode | `AllowUnsignedDev` |
| update package | `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/PureCVisorDesktopNode-0.42.8-admin-smoke-update.zip` |
| update package SHA-256 | `f8bb7900687c1a19eafc57266adbd388c826b15b4926808beac8ac0e79871ccc` |

## Campaign Descriptor

- Descriptor root:
  `artifacts/manual-admin-campaign-20260512-0427-0428/manual-admin-campaign-descriptor`
- Summary: `overall_status=pass`, `runner_count=6`, `missing_count=0`,
  `not_pass_count=0`
- Batch manifest:
  `artifacts/batch-runs/manual-admin-campaign-descriptor-20260512-0427-0428/manifest.json`
- Batch summary: `ok=true`, `status=completed`, `executed_steps=1`

## Installed Update/Rollback

Installed-host product update/rollback은 PASS다.

- Baseline snapshot:
  `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/product-update-rollback/baseline-0427-installed-snapshot.json`
- Update PASS:
  `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/product-update-rollback/update-0427-to-0428-summary.json`
- Updated snapshot:
  `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/product-update-rollback/updated-0428-installed-snapshot.json`
- Rollback PASS:
  `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/product-update-rollback/rollback-0428-to-0427-summary.json`
- Final rollback snapshot:
  `artifacts/manual-admin-campaign-20260512-0427-0428/lifecycle/product-update-rollback/rollback-0427-installed-snapshot.json`

Update action은 `Update`, rollback action은 `Rollback`이며 둘 다 `ok=true`다.
Rollback 직후 final installed manifest는 `0.42.7-admin-smoke`, service는 `Running`이다.

## Dedicated Clean-host

Dedicated clean-host install/update/rollback은 PASS다.

- Artifact root: `artifacts/manual-admin-campaign-20260512-0427-0428/clean-host`
- Result: `internal_clean_host_install_update_rollback_smoke=pass`
- Baseline manifest: `0.42.7-admin-smoke`
- Updated manifest: `0.42.8-admin-smoke`
- Final manifest: `0.42.7-admin-smoke`
- Install/update/rollback exit code: `0` / `0` / `0`
- Final Web Console: HTTP `200`
- Blocker: `none`

Windows Update는 clean-host guest에서 요청/적용했고, reboot 후 PowerShell Direct
재접속으로 install/update/rollback을 완료했다.

## Burn Bootstrapper

Burn bootstrapper lifecycle은 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260512-0427-0428/burn-bootstrapper-lifecycle`
- Bundle:
  `PureCVisorDesktopNode-0.42.8-admin-smoke-bootstrapper.exe`
- Bundle SHA-256:
  `46e67ece65f4c0af5a4929f7858e6920a44996e5ae29b3d933b6c68b5b5599e8`
- Install exit code: `0`
- Repair exit code: `0`
- Remove exit code: `0`
- Baseline restore exit code: `0`
- Blocker: `none`

Target `0.42.8-admin-smoke` repair path는 `BATCH_EVIDENCE_ROOT` 전달을 확인했다.
다만 baseline `0.42.7-admin-smoke` restore는 해당 제품화 이전 MSI이므로 service
`PathName`에서 `--batch-evidence-root`가 사라질 수 있다. 따라서 restore나 full gate
이후에는 `0.42.8-admin-smoke`의 `service-action repair-installed
--batch-evidence-root <artifacts-parent-root>`를 authoritative 운영 절차로 다시
적용한다.

## MSIX Lifecycle

MSIX internal package lifecycle은 PASS다.

- Artifact root: `artifacts/msix-package-lifecycle-smoke-20260512-0427-0428`
- Summary: `summary.json`
- Result: `build-install-update-remove-pass-internal-smoke`
- Final MSIX package/service absence: PASS

이 MSIX evidence는 internal Root/leaf signing smoke이며 public trusted signing 또는
external stable publication evidence가 아니다.

## Installed Runtime Ops Summary

Installed runtime ops summary capture는 PASS다.

- Artifact root:
  `artifacts/manual-admin-campaign-20260512-0427-0428/installed-runtime-ops-summary`
- Command surface: installed `pcvcli.exe --protected-token-file ... --json ops summary`
- Installed manifest: `0.42.7-admin-smoke`
- Service state: `Running`
- Service path: `--batch-evidence-root` 포함
- Batch evidence root:
  `D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts`
- `batch_evidence.status`: `available`
- `latest.batch_id`: `full-admin-host-mutation-gate-20260512-181309-0427`
- Errors: `0`

이 capture는 package-pair campaign 시점의 latest full gate가 0427임을 보여준다.
후속 0428 full gate와 post-gate current-card는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md`가
소유한다.

## 판정

`0.42.7 -> 0.42.8` package-pair는 installed update/rollback, dedicated clean-host,
Burn, MSIX, installed runtime summary, descriptor 기준으로 PASS다.

Manual-admin 운영 절차에서 `BATCH_EVIDENCE_ROOT`는 개별 batch directory가 아니라
batch child evidence를 함께 읽을 수 있는 artifacts parent root로 설정한다. Baseline
restore 또는 full gate가 service path를 재작성한 뒤에는 target `0.42.8-admin-smoke`
`service-action repair-installed --batch-evidence-root`로 current-card path를 다시
고정한다.

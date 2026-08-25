# MANUAL-ADMIN 캠페인 2026-05-15 04215->04216

```text
evidence_id: manual-admin-campaign-2026-05-15-04215-04216
scope: manual-admin-package-pair-clean-host-burn-msix-installed-runtime
result: PASS
baseline_version: 0.42.15-admin-smoke
target_version: 0.42.16-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
public_release: not-claimed
```

이 evidence는 `0.42.15-admin-smoke -> 0.42.16-admin-smoke` 내부 MANUAL-ADMIN
package-pair campaign을 닫는다. Readiness, installed update/rollback, dedicated
clean-host with Windows Update NoContact recovery, Burn install/repair/remove, MSIX
build/install/update/remove, installed runtime ops summary, descriptor generation이 모두
PASS다. 이 기록은 내부 사설망 admin-smoke 증거이며 public trusted signing 또는 외부
stable publication evidence가 아니다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| baseline package root | `artifacts/admin-smoke-package-20260514-04215-clean` |
| baseline MSI SHA-256 | `80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7` |
| target package root | `artifacts/admin-smoke-package-20260515-04216` |
| target MSI SHA-256 | `8b67c774f5d986c90749f494cc2084626d5bdf63904d3f9dd26b9b5daadde366` |
| target package provenance commit | `29197ca7e269d2be9a8fe3f645c004819738838f` |
| payload aggregate SHA-256 | `79e48f2f0b98e72d43bd0b02a3a530df62956a9a9802596109a027da6d89f950` |
| product wrapper SHA-256 | `5ba0708413d863e356b166a69ab8e4ae43f26d9609d65b7a3b9cce13f6344c33` |
| service host SHA-256 | `ed3c987f0d2675ddde53f07ed9305af55b4b885e11cba0ef6140211551e8bb4d` |
| CLI SHA-256 | `1c8113d56103145acfd35d0936594ddbb234a450a2924ddc58aea7a8b006264b` |
| TUI SHA-256 | `5d7bd7121920da145a7f6e8e81a13829e651fb7f504e9976bb9bf55f0b5f1d55` |
| signing mode | `AllowUnsignedDev` |
| update ZIP | `artifacts/manual-admin-campaign-20260515-04215-04216/lifecycle/PureCVisorDesktopNode-0.42.16-admin-smoke-update.zip` |
| update ZIP SHA-256 | `acd5209aa73cb14ffc655122b5905f45c87a9b9c610dd2f15307a61de7a966ab` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260515-04215-04216/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260515-04215-04216/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update-nocontact-recovery` | `artifacts/manual-admin-campaign-20260515-04215-04216/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260515-04215-04216/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260515-04215-04216/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260515-04215-04216/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260515-04215-04216/manual-admin-campaign-descriptor-supervised/summary.json` |

## Lifecycle Observations

- Installed product update/rollback summary: `ok=true`; manifest version은 update 후
  `0.42.16-admin-smoke`, rollback 후 `0.42.15-admin-smoke`로 복귀했다.
- Clean-host summary: install/update/rollback exit code는 모두 `0`; baseline
  `0.42.15-admin-smoke`, updated `0.42.16-admin-smoke`, final
  `0.42.15-admin-smoke`; final service는 `Running`, Web Console은 HTTP `200`,
  blocker는 `none`이었다.
- Clean-host Windows Update: KB `5087545`, `update_count=1`, reboot 수행,
  `automatic_recovery_performed=true`, `recovery_actions=1`,
  `WindowsUpdateNoContactRecoverySeconds=900`, post-update UBR `5139`.
- Burn lifecycle: bundle
  `artifacts/manual-admin-campaign-20260515-04215-04216/burn-bootstrapper-lifecycle/PureCVisorDesktopNode-0.42.16-admin-smoke-bootstrapper.exe`,
  bundle SHA-256 `fd045b7f50763c5cd9e1787eb24f9253f63bf40cde3d3cb2de9afea3687a577a`,
  build/install/repair/remove/restore-baseline checks가 모두 `true`였다.
- MSIX lifecycle: v1 MSIX SHA-256
  `520e12031cef120f4801c65a8bc0022ef863961c0effce1609d828722a6cee0c`,
  v2 MSIX SHA-256
  `1edf5a2b4233759328cea5f258b1be17d9c1dff5f0b63febce3fe8f866b8daf5`,
  internal cert thumbprint `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`.
- Installed runtime ops summary: target installed manifest는
  `0.42.16-admin-smoke`, service `Running`, Web Console HTTP `200`, runtime policy
  unauthenticated boundary `401`, service path는 `--batch-evidence-root`를 포함했다.

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260515-04215-04216` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04215-04216/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04215-04216/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260515-04215-04216/manual-admin-campaign-descriptor-supervised/summary.json` |
| batch status | `completed` |
| batch ok | `true` |
| executed steps | `1` |
| descriptor overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |

## Decision

`0.42.15-admin-smoke -> 0.42.16-admin-smoke` package-pair는 PASS로 닫는다.
Operational latest current-card anchor는 별도 full admin host mutation gate evidence
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04216-hostmutation.md`가
소유한다.

# MANUAL-ADMIN 캠페인 2026-05-15 04216->04218

```text
evidence_id: manual-admin-campaign-2026-05-15-04216-04218
result: PASS
baseline_version: 0.42.16-admin-smoke
target_version: 0.42.18-admin-smoke
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.16-admin-smoke -> 0.42.18-admin-smoke` 내부 MANUAL-ADMIN
package-pair campaign을 닫는다. `0.42.17-admin-smoke`는 package build와 일부
lifecycle bucket이 통과했지만 dedicated clean-host update에서
`PCV_PRODUCT_UPDATE_START_FAILED` / `sc.exe start` exit `1053`으로 실패했다.
원인은 clean Server 2022 guest에 .NET 10 runtime이 없는데 target payload가
framework-dependent로 publish된 회귀였고, commit
`9121d1f5e7fa83d803c484a44698d4fc8e825c19`가 installer payload publish를 다시
self-contained로 고정했다. 따라서 `0.42.17-admin-smoke`는 diagnostic/failure
history로만 보존하고, current package-pair claim은 `0.42.18-admin-smoke`가
소유한다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260515-04216-04218` |
| baseline package root | `artifacts/admin-smoke-package-20260515-04216` |
| baseline version | `0.42.16-admin-smoke` |
| baseline MSI SHA-256 | `8b67c774f5d986c90749f494cc2084626d5bdf63904d3f9dd26b9b5daadde366` |
| target package root | `artifacts/admin-smoke-package-20260515-04218` |
| target version | `0.42.18-admin-smoke` |
| target MSI SHA-256 | `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af` |
| target payload aggregate SHA-256 | `156497b8a21a2bada24bc5af7f9ea73c60a3a4b0b3cdd0674f0086eed321077b` |
| target wrapper SHA-256 | `a56174fc5653435af98ac75aeb3356336825bf4cebd1da17122a626b8beae80b` |
| target host SHA-256 | `140a7b9bb0db3885bf3d63b3dec30f1e41abb04588f66325f7704be4c005e497` |
| target CLI SHA-256 | `8c4442d2f7841414f0da994e74beefaf837cfe3fa3f6e0af1233e9318bcebf42` |
| target TUI SHA-256 | `bb130aeac8e383ea7ff039c8fb3a2c62d7f62f4436845443b68752ce071eadc9` |
| provenance commit | `9121d1f5e7fa83d803c484a44698d4fc8e825c19` |
| signing mode | `AllowUnsignedDev` |
| update ZIP SHA-256 | `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260515-04216-04218/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update-nocontact-recovery` | `artifacts/manual-admin-campaign-20260515-04216-04218/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260515-04216-04218/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260515-04216-04218/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260515-04216-04218/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json` |

## 관찰

- Installed update/rollback은 baseline manifest `0.42.16-admin-smoke`, update 후
  `0.42.18-admin-smoke`, rollback 후 `0.42.16-admin-smoke`를 확인했다.
- Clean-host는 Windows Server 2022 cumulative update `KB5087545` 적용 후 heartbeat
  `NoContact` + CPU idle 상태에서 recovery policy를 한 번 실행했고
  `automatic_recovery_performed=true`, `recovery_actions=1`을 남겼다. Guest smoke는
  install/update/rollback exit `0`, final Web Console HTTP `200`, final manifest
  `0.42.16-admin-smoke`로 닫혔다.
- Burn bundle SHA-256은
  `efaadc1286ee8385847e766ef9702e33d10269be94ad567e70763921e8c9b1de`다.
- MSIX lifecycle은 `0.42.16.0 -> 0.42.18.0` build/install/update/remove를 통과했다.
  v1 SHA-256은 `12e6207efa5dd62ebc95b9b2f6204777f2da961c3c1e9e6eb66d4b2afd0f899e`,
  v2 SHA-256은 `11f91820252f3a8607d844c2e648ae970a87ed2ca74d48861de7e06cc887448d`다.
- Installed runtime ops summary는 installed manifest `0.42.18-admin-smoke`, service
  `Running`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, unauthenticated
  runtime policy `401`, absolute batch evidence root를 확인했다.

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260515-04216-04218` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04216-04218/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04216-04218/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

## 결정

`0.42.16-admin-smoke -> 0.42.18-admin-smoke` package-pair는 PASS로 닫는다.
이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted
signing, public stable URL, winget submission, external stable publication을 주장하지
않는다. 최신 operational current-card anchor와 full admin host mutation PASS는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`가
소유한다.

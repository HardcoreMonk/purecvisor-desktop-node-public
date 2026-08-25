# MANUAL-ADMIN 캠페인 2026-05-16 04224->04225

```text
evidence_id: manual-admin-campaign-2026-05-16-04224-04225
result: PASS
package_pair: 0.42.24-admin-smoke -> 0.42.25-admin-smoke
baseline_version: 0.42.24-admin-smoke
target_version: 0.42.25-admin-smoke
host_mutation_performed: true
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
descriptor_batch_id: manual-admin-campaign-descriptor-20260516-04224-04225-closed
descriptor_missing_count: 0
descriptor_not_pass_count: 0
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.24-admin-smoke -> 0.42.25-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign을 닫는다. Installed update/rollback,
Windows Update 포함 dedicated clean-host install/update/rollback, Burn bootstrapper
lifecycle, MSIX build/install/update/remove, installed runtime ops summary, descriptor
generation을 모두 PASS로 확인했다.

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260516-04224-04225` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04224` |
| baseline MSI SHA-256 | `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225` |
| target MSI SHA-256 | `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b` |
| target payload aggregate SHA-256 | `3ad6856606ab71fddef89adf2c59e17d7c68ee257723444922431e0e0070a6cb` |
| provenance commit | `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1` |
| update ZIP SHA-256 | `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585` |
| Burn bundle SHA-256 | `b3be16eb9c4dcf4ea44e334b9c2eab8f008245d452c3f88e90d8d45d1f921315` |
| MSIX v1/v2 SHA-256 | `2976a5531d994a6ff832a6c0e5a5a951bfab5b881c33f1b957a22eed34390b43` / `c7b0b95795982bb23d0710be8b0ff125eeefa04b29f989d912f56db8f1bd6434` |
| signing mode | `AllowUnsignedDev` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260516-04224-04225/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260516-04224-04225/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260516-04224-04225/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260516-04224-04225/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260516-04224-04225/installed-runtime-ops-summary/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-campaign-descriptor-supervised/summary.json` |

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260516-04224-04225-closed` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04224-04225-closed/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260516-04224-04225-closed/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-campaign-descriptor-supervised/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

Installed update/rollback은 baseline manifest `0.42.24-admin-smoke`, update 후
`0.42.25-admin-smoke`, rollback 후 `0.42.24-admin-smoke`, final current
`0.42.25-admin-smoke`를 확인했다. Clean-host는 blocker `none`, final rollback
`0.42.24-admin-smoke`, Web Console HTTP `200`으로 닫혔다. Installed runtime ops
summary는 latest batch `full-admin-host-mutation-gate-20260516-04225`,
`runtime-api-current-evidence-rollup-v1`, registry bridge route detail count `4`를
확인했다.

이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing
또는 외부 stable publication evidence가 아니다.

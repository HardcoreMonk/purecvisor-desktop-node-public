# MANUAL-ADMIN 캠페인 2026-05-17 04225->04226

```text
evidence_id: manual-admin-campaign-2026-05-17-04225-04226
result: PASS
package_pair: 0.42.25-admin-smoke -> 0.42.26-admin-smoke
baseline_version: 0.42.25-admin-smoke
target_version: 0.42.26-admin-smoke
host_mutation_performed: true
manual_admin_descriptor_generation_contract: manual-admin-descriptor-generation-contract-v2
descriptor_batch_id: manual-admin-campaign-descriptor-20260517-04225-04226-closed
descriptor_missing_count: 0
descriptor_not_pass_count: 0
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign을 닫는다. Installed update/rollback,
Windows Update 포함 dedicated clean-host install/update/rollback, Burn bootstrapper
lifecycle, MSIX build/install/update/remove, installed runtime ops summary, descriptor
generation을 모두 PASS로 확인했다.

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260517-04225-04226` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04225` |
| baseline MSI SHA-256 | `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226` |
| target MSI SHA-256 | `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7` |
| target payload aggregate SHA-256 | `82061f08020b2af871e903b3fb2a82f5b9a78a4711192fcafc0284afea61fdc3` |
| provenance commit | `d6500c01c972cbc7ca1e290e51120181ceea1501` |
| update ZIP SHA-256 | `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4` |
| Burn bundle SHA-256 | `db171cfc5f93db7c428d83b9131f38fa86cc3cfc0b08fabbe9c6d527f8ba63dc` |
| MSIX v1/v2 SHA-256 | `4bf992ae37ba5e88f860b87bb9c92842a1ac07378f02f1f672d60d2b1ddac5b3` / `57dcd8418dcc39f09fd8f27f3111d2d5ab709a9267f31164eb4119071c98ffa7` |
| signing mode | `AllowUnsignedDev` |

## PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/lifecycle/product-update-rollback/summary.json` |
| installed update summary | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/lifecycle/product-update-rollback/update-04225-to-04226-summary.json` |
| installed rollback summary | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/lifecycle/product-update-rollback/rollback-04226-to-04225-summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260517-04225-04226/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260517-04225-04226/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/installed-runtime-ops-summary/summary.json` |
| installed current-card recheck after descriptor closure | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/installed-runtime-ops-summary/current-card-recheck-after-docs/summary.json` |
| descriptor generation | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json` |

## Descriptor

| 항목 | 값 |
| --- | --- |
| descriptor batch id | `manual-admin-campaign-descriptor-20260517-04225-04226-closed` |
| batch manifest | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260517-04225-04226-closed/manifest.json` |
| batch summary | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260517-04225-04226-closed/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json` |
| overall status | `pass` |
| runner count | `6` |
| missing count | `0` |
| not pass count | `0` |
| host mutation by descriptor batch | `false` |

Installed update/rollback은 baseline manifest `0.42.25-admin-smoke`, update 후
`0.42.26-admin-smoke`, rollback 후 `0.42.25-admin-smoke`, final current
`0.42.26-admin-smoke`를 확인했다. Clean-host는 Windows Update를 적용한 dedicated
VM에서 baseline `0.42.25-admin-smoke`, updated `0.42.26-admin-smoke`, final rollback
`0.42.25-admin-smoke`, blocker `none`, Web Console HTTP `200`으로 닫혔다.

Installed runtime ops summary는 installed manifest `0.42.26-admin-smoke`, service
`Running`, Web Console HTTP `200`, `/pcv-config.js` HTTP `200`, unauthenticated
runtime policy `401` / `PCV_AUTH_REQUIRED`, latest batch
`full-admin-host-mutation-gate-20260516-04226`,
`runtime-api-current-evidence-rollup-v1`, registry bridge route detail count `4`를
확인했다.

Descriptor closure 이후 installed current-card recheck는
`pcvcli --protected-token-file <redacted> --json ops summary`로 다시 수행했다.
`artifacts/manual-admin-campaign-20260517-04225-04226/installed-runtime-ops-summary/current-card-recheck-after-docs/summary.json`는
`ok=true`, `batch_evidence_status=available`,
`latest_batch_id=full-admin-host-mutation-gate-20260516-04226`,
`current_evidence_contract=runtime-api-current-evidence-rollup-v1`,
`runtime_api_registry_bridge_contract=runtime-api-diagnostics-ops-summary-registry-bridge-v2`,
route count `4`, latest manual-admin package-pair
`0.42.25-admin-smoke -> 0.42.26-admin-smoke`를 기록한다.

이 evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing
또는 외부 stable publication evidence가 아니다.

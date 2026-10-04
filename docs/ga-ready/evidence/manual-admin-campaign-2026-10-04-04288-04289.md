# Manual-admin campaign consume `0.42.88-admin-smoke` -> `0.42.89-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-04-04288-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261004-04288-04289`
artifact_root: `artifacts/manual-admin-campaign-20261004-04288-04289`
consume_manifest: `artifacts/manual-admin-campaign-20261004-04288-04289/consume-manifest.json`
consume_manifest_sha256: `52062a3b8c53b38f02c3758e63e7d0be935a22a1df448a900c971378d8756887`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261004-04288-04289-consume`
descriptor_path: `artifacts/manual-admin-campaign-20261004-04288-04289/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `a9135272795e30c005e7232c852f408f208d436a141da4968349e0a0287084b1`
descriptor_summary_sha256: `4a7dbf238fbc843a76c65eb433f091772a477b9ae166d89f96f9d262c3693578`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20261004-04288-04289`
baseline_version: `0.42.88-admin-smoke`
target_version: `0.42.89-admin-smoke`
release_train: `0.42.89-admin-smoke`
approved_target_msi_sha256: `e4574861a06537aacf41f16e75138e9f8cc9c5d4c8f0df7d7e977bd58e0b7391`
update_package_sha256: `b852641609601f77cccf015969e3135212f35562e5dc694dd690fb59ca505f71`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-10-04 원본 descriptor(`manual-admin-campaign-descriptor-20261004-04288-04289`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다. descriptor 도구는 bucket summary 경로를 명시적으로 받는다. 그래서 consume root 안의 경로를 넘겼다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.89-admin-smoke`로 썼다.

## Descriptor consume

| 필드 | 값 |
| --- | --- |
| `ok` | `true` |
| `overall_status` | `pass` |
| `runner_count` | `6` |
| `missing_count` | `0` |
| `not_pass_count` | `0` |
| `plan_only` | `true` |
| `actual_execution` | `not-run` |
| `host_mutation_performed` | `false` |

## PASS Bucket (source)

| Bucket | 결과 | Source root | Consume path |
| --- | --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20261004-04288-04289` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20261004-04288-04289` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20261004-04288-04289` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20261004-04289` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20261004-04288-04289` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20261004-04288` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-04에 host mutation을 수행했다. 이 consume은 그 summary JSON `7`개만 복사했다(`bulky_binaries_not_copied=true`).

pair target은 clean package MSI `e4574861…`다. fullgate operational MSI(`fe5677ff…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

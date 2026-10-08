# Manual-admin campaign consume `0.42.92-admin-smoke` -> `0.42.93-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-08-04292-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261008-04292-04293`
artifact_root: `artifacts/manual-admin-campaign-20261008-04292-04293`
consume_manifest: `artifacts/manual-admin-campaign-20261008-04292-04293/consume-manifest.json`
consume_manifest_sha256: `e3ceb9d282c6d60a534d15cfa769fcfb47c5c35d5586e85c623ed3b4981c276f`
descriptor_batch_id: `manual-admin-campaign-20261008-04292-04293-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261008-04292-04293/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `4a86e474d30cedb46da21de7d532c82e371ad303c5634753cf940d3e114adb91`
descriptor_summary_sha256: `7fb71b4723717a1a87f4bf29bb20821e7cb66a6ac12da55d678362e7d9b6be9a`
source_descriptor_batch_id: `manual-admin-campaign-20261008-04292-04293-closed`
baseline_version: `0.42.92-admin-smoke`
target_version: `0.42.93-admin-smoke`
release_train: `0.42.93-admin-smoke`
approved_target_msi_sha256: `13d7f0d476828f865b0d4aca7331a2dcd1a10a8dbe157b9d6a93dc2217f9fb49`
update_package_sha256: `97e4ec6a6376fea970cd2f084ec4352d062bb00cb42e2fdfc0f08d1cb7dfd181`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261008-04292-04293-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-10-08 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.93-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261008-04292-04293` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-08 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `13d7f0d4…`다. fullgate operational MSI(`5d5a7c7c…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

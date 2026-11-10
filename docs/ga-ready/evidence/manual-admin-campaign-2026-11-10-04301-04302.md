# Manual-admin campaign consume `0.43.1-admin-smoke` -> `0.43.2-admin-smoke`

evidence_id: `manual-admin-campaign-2026-11-10-04301-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261110-04301-04302`
artifact_root: `artifacts/manual-admin-campaign-20261110-04301-04302`
consume_manifest: `artifacts/manual-admin-campaign-20261110-04301-04302/consume-manifest.json`
consume_manifest_sha256: `ce3ec65c87e3322643e7e2af833398850978f3a62e06c8843a6b0d5d4ea7ed7f`
descriptor_batch_id: `manual-admin-campaign-20261110-04301-04302-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261110-04301-04302/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `2e1ec0f15139d0f7b874b59e4193d3acf65718ca2e56b00902fbcb1c7183ce7d`
descriptor_summary_sha256: `2fc1ce203cb19648d33dad95ca6e4740d80751a0088fa9f2116402fd7077327e`
source_descriptor_batch_id: `manual-admin-campaign-20261110-04301-04302-closed`
baseline_version: `0.43.1-admin-smoke`
target_version: `0.43.2-admin-smoke`
release_train: `0.43.2-admin-smoke`
approved_target_msi_sha256: `fa6afbce618568a95bb941c34ded38c188d7a8c221bfc26810d3ac14111b1c0c`
update_package_sha256: `c9854f1c22dafc98b4f04e7f2e0af3d926d10021453b5f957a7da509f66b61b4`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261110-04301-04302-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다. baseline package는 train `0.43.1` Task 1의 `artifacts/admin-smoke-package-20261110-04301`이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-11-10 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.43.2-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261110-04301-04302` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-11-10 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `fa6afbce…`다. fullgate operational MSI(`82e78f2b…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

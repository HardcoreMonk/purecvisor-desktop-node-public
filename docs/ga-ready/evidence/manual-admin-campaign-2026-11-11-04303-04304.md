# Manual-admin campaign consume `0.43.3-admin-smoke` -> `0.43.4-admin-smoke`

evidence_id: `manual-admin-campaign-2026-11-11-04303-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261111-04303-04304`
artifact_root: `artifacts/manual-admin-campaign-20261111-04303-04304`
consume_manifest: `artifacts/manual-admin-campaign-20261111-04303-04304/consume-manifest.json`
consume_manifest_sha256: `732692a63a5b8ad5120fc94bf7e92a4c04f2e03dffc3d0027883d73cef6ae94a`
descriptor_batch_id: `manual-admin-campaign-20261111-04303-04304-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04303-04304/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `574b22c1200ad59e19dda450bf1dd6fefe4dd52dbe4e16f3c17999a374addb1d`
descriptor_summary_sha256: `56e1ca96ba7c900526dbee9cd5166392a24b3f7f3bbfa8ee3fd723a9393c4fe5`
source_descriptor_batch_id: `manual-admin-campaign-20261111-04303-04304-closed`
baseline_version: `0.43.3-admin-smoke`
target_version: `0.43.4-admin-smoke`
release_train: `0.43.4-admin-smoke`
approved_target_msi_sha256: `c24ef8317c838d659834b7e77446625d9377e5774f9057fedc813318a739fccb`
update_package_sha256: `92630abe393ea54fa3acb0c8a775e03d18fc8c27d0aeb1c4d8b1bc81f3cdb2cc`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261111-04303-04304-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다. baseline package는 train `0.43.3` Task 1의 `artifacts/admin-smoke-package-20261111-04303`이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-11-11 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.43.4-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261111-04303-04304` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-11-11 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `c24ef831…`다. fullgate operational MSI(`c9f87149…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

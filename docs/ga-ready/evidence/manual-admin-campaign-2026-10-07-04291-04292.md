# Manual-admin campaign consume `0.42.91-admin-smoke` -> `0.42.92-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-07-04291-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261007-04291-04292`
artifact_root: `artifacts/manual-admin-campaign-20261007-04291-04292`
consume_manifest: `artifacts/manual-admin-campaign-20261007-04291-04292/consume-manifest.json`
consume_manifest_sha256: `1594f32f4463eaf1b24e87a4c626346d121353b025c325ea1bb4e859a8f0e0a9`
descriptor_batch_id: `manual-admin-campaign-20261007-04291-04292-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261007-04291-04292/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `3604ac35fdeea61633883525abb31df8cbd30827c6442b5897f19d6c3f105082`
descriptor_summary_sha256: `e09aa8f0c5e452aaa109f0d0e61bf91e2db39f321f9db792d65c83f3f506bc9a`
source_descriptor_batch_id: `manual-admin-campaign-20261007-04291-04292-closed`
baseline_version: `0.42.91-admin-smoke`
target_version: `0.42.92-admin-smoke`
release_train: `0.42.92-admin-smoke`
approved_target_msi_sha256: `dc79fdd166f0882a31e726e43909e0ab5b3dda09142de162c72dfa9821b104ec`
update_package_sha256: `fcaf13dc3118371eab79013f203e5da2ba1e078401e6b2b331801a334ad57102`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261007-04291-04292-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-10-08 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.92-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261007-04291-04292` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-07 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `dc79fdd1…`다. fullgate operational MSI(`67b257a3…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

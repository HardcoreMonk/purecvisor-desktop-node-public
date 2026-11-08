# Manual-admin campaign consume `0.42.96-admin-smoke` -> `0.42.98-admin-smoke`

evidence_id: `manual-admin-campaign-2026-11-08-04296-04298`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261108-04296-04298`
artifact_root: `artifacts/manual-admin-campaign-20261108-04296-04298`
consume_manifest: `artifacts/manual-admin-campaign-20261108-04296-04298/consume-manifest.json`
consume_manifest_sha256: `8e8b25bdede9a5832d76dc8a3118706196501cc475b04904e5816ce449a1d3bc`
descriptor_batch_id: `manual-admin-campaign-20261108-04296-04298-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261108-04296-04298/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `a90c8b36ee302ecbafd75c6443b753746160e3b035287b9777de0790a985dd57`
descriptor_summary_sha256: `ad07d2dc96f2c5c459223c6fea01cc07b91a430cb42ed88c2aea17f4cc3a8ac5`
source_descriptor_batch_id: `manual-admin-campaign-20261108-04296-04298-closed`
baseline_version: `0.42.96-admin-smoke`
target_version: `0.42.98-admin-smoke`
release_train: `0.42.98-admin-smoke`
approved_target_msi_sha256: `9a26d4cee9c4360cb8a04ce520b57de4d2e99c3e7b64997d8b4567e7c04733b0`
update_package_sha256: `c18bc3de426cc031cff4acc939038bd045513e4e401c8b61dbd3adb371777494`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261108-04296-04298-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다. baseline package는 재클론으로 원래 artifact가 없어 `0.42.96` payload source `87a7deb`에서 다시 빌드한 `artifacts/admin-smoke-package-20261108-04296r`이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-11-08 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.98-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261108-04296-04298` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-11-08 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `9a26d4ce…`다. fullgate operational MSI(`0a42b89f…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

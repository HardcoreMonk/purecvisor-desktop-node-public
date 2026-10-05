# Manual-admin campaign consume `0.42.89-admin-smoke` -> `0.42.90-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-05-04289-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261005-04289-04290`
artifact_root: `artifacts/manual-admin-campaign-20261005-04289-04290`
consume_manifest: `artifacts/manual-admin-campaign-20261005-04289-04290/consume-manifest.json`
consume_manifest_sha256: `72ddcfe37d762c714fe334cf8e5bad033374dcb08b24f080572a70690a882e0a`
descriptor_batch_id: `manual-admin-campaign-20261005-04289-04290-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261005-04289-04290/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `9ab32dc3cc895c8cdfbe6192416f3a122c57429f63df3dae15d0e89699e29581`
descriptor_summary_sha256: `8e3e135b1782d3ed1b77904e92d693a7a523f301c772a6b2d67d6a13c3c653f4`
source_descriptor_batch_id: `manual-admin-campaign-20261005-04289-04290-closed`
baseline_version: `0.42.89-admin-smoke`
target_version: `0.42.90-admin-smoke`
release_train: `0.42.90-admin-smoke`
approved_target_msi_sha256: `54277baafea5be820572c082a874b75c23f55ca3fab0c374cd2b6ca357012146`
update_package_sha256: `1c91de5c16ae73f624deb3802d25a42cf0c66053b126714c5b2208075e1a3abb`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261005-04289-04290-closed`를 만들었다. 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.90-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261005-04289-04290` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-05 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `54277baa…`다. fullgate operational MSI(`ac367ea4…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

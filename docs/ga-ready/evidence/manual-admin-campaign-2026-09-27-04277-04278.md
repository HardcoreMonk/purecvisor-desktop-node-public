# Manual-admin campaign descriptor consume 2026-09-27 0.42.77 -> 0.42.78

evidence_id: `manual-admin-campaign-2026-09-27-04277-04278`
result: `PASS`
scope: `manual-admin-package-pair-descriptor-consume`
baseline_version: `0.42.77-admin-smoke`
target_version: `0.42.78-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260927-04277-04278`
consume_manifest: `artifacts/manual-admin-campaign-20260927-04277-04278/consume-manifest.json`
consume_manifest_sha256: `67c6e58d07d1df6b3890ff0b3453a4687c5e1adab9cce7f1fb6fc4f1cfd090e6`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260927-04277-04278`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20260925-04277-04278`
descriptor_summary: `artifacts/manual-admin-campaign-20260927-04277-04278/manual-admin-campaign-descriptor/summary.json`
descriptor_summary_sha256: `e0347afdd6c628dbe477a1b1267f3cc25d4f9c4d15d1196fe0433a2c4bd5b95a`
descriptor_path: `artifacts/manual-admin-campaign-20260927-04277-04278/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `e6c024994e1fbc22893b198ea143dfbf9cb22423eb7e66fe6314a4a0ea381519`
baseline_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
target_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
update_zip_sha256: `6cb31a6b75d58a10a35a4aba5dd6d78e249f5c72f2ac8ad4a41601585939a821`
operational_fullgate_msi_not_used: `true`
copy_mode: `json-evidence-only`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 이미 PASS한 여섯 bucket summary의 JSON을 0.42.77 consume과 같은 single-root
배치로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`가 `overall_status=pass`를 낸
consume 기록이다. 2026-09-25 descriptor(`manual-admin-campaign-descriptor-20260925-04277-04278`)는
여섯 원본 root를 직접 읽었고, 이 consume는 같은 입력을 한 root로 묶었다. consume checkpoint
자체는 MSI/서비스/Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가
`docs/ga-ready/current-evidence.json` current를 `0.42.78-admin-smoke`로 쓴다.

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

## PASS Bucket (historical source)

| Bucket | 결과 | Source root | Consume path |
| --- | --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20260925-04277-04278` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20260925-04277-04278` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20260925-04277-04278` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20260925-04278/lifecycle` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260925-04277-04278` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20260925-04277` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-09-25에 host mutation을 수행했다. 이 consume는 그 JSON evidence
`18`개만 복사한다. update 원본의 `update.json`/`rollback.json`은 0.42.77 배치와 같은 이름
`update-summary.json`/`rollback-summary.json`으로 두었다. Burn/MSIX 대용량 패키지 바이너리는
source root에 남긴다.

pair target은 clean package MSI `c3390c1e…`다. operational fullgate MSI `0856d07e…` /
provenance `0de176f`는 이 pair의 설치 대상이 아니다.

## Nonclaims

- 2026-09-27 Lane 3가 `docs/ga-ready/current-evidence.json`을 `0.42.78-admin-smoke`로
  승격한다. 이 consume 문서는 그 pair descriptor의 입력을 소유한다.
- 이 consume에서 호스트 설치본을 바꾸는 product Update는 실행하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

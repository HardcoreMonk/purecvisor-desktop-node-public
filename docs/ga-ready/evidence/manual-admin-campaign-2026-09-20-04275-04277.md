# Manual-admin campaign descriptor consume 2026-09-20 0.42.75 -> 0.42.77

evidence_id: `manual-admin-campaign-2026-09-20-04275-04277`
result: `PASS`
scope: `manual-admin-package-pair-descriptor-consume`
baseline_version: `0.42.75-admin-smoke`
target_version: `0.42.77-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260920-04275-04277`
consume_manifest: `artifacts/manual-admin-campaign-20260920-04275-04277/consume-manifest.json`
consume_manifest_sha256: `2061933485820b09fe245d03a951d210e09a63db35a53bc2915f65887a7e36b4`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260920-04275-04277`
descriptor_summary: `artifacts/manual-admin-campaign-20260920-04275-04277/manual-admin-campaign-descriptor/summary.json`
descriptor_summary_sha256: `d1058204a3ed84f3d755fc65230866b389dd24976fa29df4b3f5be511fb8f2d2`
descriptor_path: `artifacts/manual-admin-campaign-20260920-04275-04277/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `005cf2ed6e41062e02c7b80852ff63e7cf8cee764fa99109723bb4b6211685c6`
baseline_msi_sha256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`
target_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
update_zip_sha256: `18c90d56c6ee43612a02b6294a9fceb25a2f9ef3ccc7c29e52c36c1a9ee1eafd`
operational_fullgate_msi_not_used: `true`
copy_mode: `json-evidence-only`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 이미 PASS한 여섯 bucket summary를 한 artifact root로 모은 뒤
`New-PcvManualAdminCampaignDescriptor -PlanOnly`가 `overall_status=pass`를 낸 consume
기록이다. consume checkpoint 자체는 MSI/서비스/Hyper-V mutation을 실행하지 않았다.
같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를
`0.42.77-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/ma-04275-04277-fss-r2` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/ma-04275-04277-fss-r2` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/ma-04275-04277-fss-r2` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/ma-04275-04277-lane2-burn-20260906` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/ma-04275-04277-lane2-burn-20260906` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/ma-04275-04277-lane2-burn-20260906` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-08-31~2026-09-06에 host mutation을 수행했다. 이 consume는 그
JSON evidence만 복사한다. Burn/MSIX 대용량 패키지 바이너리는 source root에 남긴다.

pair target은 clean package MSI `d03eedaf…`다. operational fullgate MSI `d4ebba77…` /
provenance `9f051b5`는 이 pair의 설치 대상이 아니다.

## 설치본 시제

consume 시점 이 호스트 `installed_current`는 `0.42.75-admin-smoke`다. 09-06 ops-summary는
당시 설치본을 `0.42.77-admin-smoke`로 기록했다. 두 값은 서로 다른 시각의 관측이다.

## Nonclaims

- 2026-09-20 Lane 3가 `docs/ga-ready/current-evidence.json`을 `0.42.77-admin-smoke`로
  승격했다. 이 consume 문서는 그 pair descriptor의 입력을 소유한다.
- 이 호스트를 `0.42.77-admin-smoke`로 다시 맞추는 product Update는 실행하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- 04277 current-card 재캡처는 이 consume 범위가 아니다.

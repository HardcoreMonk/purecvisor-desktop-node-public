# Manual-admin campaign consume `0.42.86-admin-smoke` -> `0.42.87-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-03-04286-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261003-04286-04287`
artifact_root: `artifacts/manual-admin-campaign-20261003-04286-04287`
consume_manifest: `artifacts/manual-admin-campaign-20261003-04286-04287/consume-manifest.json`
consume_manifest_sha256: `6f0d55842dde2dc1e7449255ad896adafb8cb06dd5dbf4b959284139128399f4`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261003-04286-04287-consume`
descriptor_path: `artifacts/manual-admin-campaign-20261003-04286-04287/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `27ba0d02c52c602a7fca988e877b962635922203c12f282758ebf94ba2a6c9ee`
descriptor_summary_sha256: `c32ce557f59655961d63a21c43e5f3910b2a9d070e02ee7381eccfbb1c70b728`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20261003-04286-04287`
baseline_version: `0.42.86-admin-smoke`
target_version: `0.42.87-admin-smoke`
approved_target_msi_sha256: `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1`
update_package_sha256: `28dc8af80d334dd8e3bec8acaf826a8d9033e5c66ffddf553956955fb07d2de3`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-10-03 원본 descriptor(`manual-admin-campaign-descriptor-20261003-04286-04287`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다. descriptor 도구는 bucket summary 경로를 명시적으로 받는다. 그래서 consume root 안의 경로를 넘겼다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.87-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20261003-04286-04287` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20261003-04286-04287` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04286-04287` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20261003-04287` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20261003-04286-04287` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20261003-04286` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-03에 host mutation을 수행했다. 이 consume은 그 summary JSON `7`개만 복사했다. MSI, bundle, MSIX 같은 큰 바이너리는 복사하지 않았다(`bulky_binaries_not_copied=true`).

pair target은 clean package MSI `a0041c9f…`다. fullgate operational MSI(`f339ab45…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

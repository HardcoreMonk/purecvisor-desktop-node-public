# Manual-admin campaign consume `0.42.87-admin-smoke` -> `0.42.88-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-03-04287-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261003-04287-04288`
artifact_root: `artifacts/manual-admin-campaign-20261003-04287-04288`
consume_manifest: `artifacts/manual-admin-campaign-20261003-04287-04288/consume-manifest.json`
consume_manifest_sha256: `897faf7dca12da19e9aee7144f52e1ee1e7477fe2024759001b4ae3041481b63`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261003-04287-04288-consume`
descriptor_path: `artifacts/manual-admin-campaign-20261003-04287-04288/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `f7adface19ce7fa3062c1ba055ae56c18fcd27d4d8896e4a238c4a29d93e542f`
descriptor_summary_sha256: `34c0a27b9bba16aebd571120ce9d52e97a03fe44a263cbb977cf3680202944da`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20261003-04287-04288`
baseline_version: `0.42.87-admin-smoke`
target_version: `0.42.88-admin-smoke`
approved_target_msi_sha256: `81ef85273cb9bbf9d813d4c3cce40f88c22e5d596c40906dab8766e4cab79b64`
update_package_sha256: `7051e735aff0466925ec1912103dce57dad42f6f67c452cfca4026de244cb751`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-10-03 원본 descriptor(`manual-admin-campaign-descriptor-20261003-04287-04288`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다. descriptor 도구는 bucket summary 경로를 명시적으로 받는다. 그래서 consume root 안의 경로를 넘겼다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.88-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20261003-04287-04288` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20261003-04287-04288` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04287-04288` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20261003-04288` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20261003-04287-04288` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20261003-04287` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-03에 host mutation을 수행했다. 이 consume은 그 summary JSON `7`개만 복사했다(`bulky_binaries_not_copied=true`).

pair target은 clean package MSI `81ef8527…`다. fullgate operational MSI(`32b35113…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

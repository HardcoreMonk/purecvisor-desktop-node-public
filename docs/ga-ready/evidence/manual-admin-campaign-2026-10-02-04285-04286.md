# Manual-admin campaign consume `0.42.85-admin-smoke` -> `0.42.86-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-02-04285-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261002-04285-04286`
artifact_root: `artifacts/manual-admin-campaign-20261002-04285-04286`
consume_manifest: `artifacts/manual-admin-campaign-20261002-04285-04286/consume-manifest.json`
consume_manifest_sha256: `8bdd2022a47535870d980dce362c27a670b1cc7758af5f7164d30d59d38041f1`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261002-04285-04286-consume`
descriptor_path: `artifacts/manual-admin-campaign-20261002-04285-04286/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `b4e2d1c9c5a29593c035699a5992a52eb22148014ad957fdc6353e08eadd09e4`
descriptor_summary_sha256: `fa48fa03f166c39c46697f1c2c6e1ddd30aeab23273d2913e4d4d397ccccc7c6`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20261002-04285-04286`
baseline_version: `0.42.85-admin-smoke`
target_version: `0.42.86-admin-smoke`
approved_target_msi_sha256: `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`
update_package_sha256: `5255453cd66336f5cf94ffc7489712ffdcf6f17ff14d93f92fd5af2c56f9a357`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-10-02 원본 descriptor(`manual-admin-campaign-descriptor-20261002-04285-04286`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다. descriptor 도구는 bucket summary 경로를 명시적으로 받는다. 그래서 consume root 안의 경로를 넘겼다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.86-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20261002-04285-04286` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20261002-04285-04286` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20261002-04285-04286` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20261002-04286` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20261002-04285-04286` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20261002-04285` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-02에 host mutation을 수행했다. 이 consume은 그 summary JSON `7`개만 복사했다. MSI, bundle, MSIX 같은 큰 바이너리는 복사하지 않았다(`bulky_binaries_not_copied=true`).

pair target은 clean package MSI `8edb19ce…`다. fullgate operational MSI(`85387f31…`)는 쓰지 않았다(`operational_fullgate_msi_not_used=true`). `0.42.85-admin-smoke`는 이 pair의 baseline이며 `fb95de1`을 포함하지 않는다. `0.42.85`를 operational current로 적지 않는다.

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

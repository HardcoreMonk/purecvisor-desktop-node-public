# Manual-admin campaign consume `0.42.78-admin-smoke` -> `0.42.83-admin-smoke`

evidence_id: `manual-admin-campaign-2026-09-29-04278-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20260929-04278-04283`
artifact_root: `artifacts/manual-admin-campaign-20260929-04278-04283`
consume_manifest: `artifacts/manual-admin-campaign-20260929-04278-04283/consume-manifest.json`
consume_manifest_sha256: `d69c07026fa550eac052523dc74a717d7ed9e478d5522b4f316ab3edc30234a5`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260929-04278-04283-consume`
descriptor_path: `artifacts/manual-admin-campaign-20260929-04278-04283/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `238e81f91fc7920d244280f7c474adf528627ff5f20011cbec9aef4b759a96e2`
descriptor_summary_sha256: `2df1d20caf311c31855e1cee8075f0ac1d4ef3ff4297065066394e6056730bdd`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20260929-04278-04283`
baseline_version: `0.42.78-admin-smoke`
target_version: `0.42.83-admin-smoke`
approved_target_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
update_package_sha256: `e9da97140ffe54cd609694474dfc6687ced26097222df2a65b397ee3605f2365`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 0.42.78 consume과 같은 single-root 배치로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-09-29 원본 descriptor(`manual-admin-campaign-descriptor-20260929-04278-04283`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.83-admin-smoke`로 쓴다.

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
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20260929-04278-04283` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20260929-04278-04283` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20260929-04278-04283` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20260929-04283/lifecycle` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260929-04278-04283` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20260929-04278` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-09-29에 host mutation을 수행했다. 이 consume은 그 JSON evidence `19`개만 복사했다. MSI, bundle, MSIX 같은 큰 바이너리는 복사하지 않았다(`bulky_binaries_not_copied=true`).

같은 root의 `baseline-install/`에는 Task 2a의 baseline 설치 로그가 있다. consume 입력은 아니다.

pair target은 clean package MSI `52d7cfd5…`다. fullgate operational MSI(`b7e26bfb…`)는 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

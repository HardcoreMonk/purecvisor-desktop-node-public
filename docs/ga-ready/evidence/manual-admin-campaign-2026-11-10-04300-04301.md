# Manual-admin campaign consume `0.43.0-admin-smoke` -> `0.43.1-admin-smoke`

evidence_id: `manual-admin-campaign-2026-11-10-04300-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261110-04300-04301`
artifact_root: `artifacts/manual-admin-campaign-20261110-04300-04301`
consume_manifest: `artifacts/manual-admin-campaign-20261110-04300-04301/consume-manifest.json`
consume_manifest_sha256: `5e7051cd5769a187102ac7b3aed794155edd8552ec99633b268d7fe2e4779d0b`
descriptor_batch_id: `manual-admin-campaign-20261110-04300-04301-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261110-04300-04301/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `a3d3aabefb6965e226b5422c07473b9df02dca36f6dcdb4cc9bf24a11ae28e3b`
descriptor_summary_sha256: `76eef5fc18c229ba16c4f74f998152e3d088c1923bdce3d819ae747b5c7922a6`
source_descriptor_batch_id: `manual-admin-campaign-20261110-04300-04301-closed`
baseline_version: `0.43.0-admin-smoke`
target_version: `0.43.1-admin-smoke`
release_train: `0.43.1-admin-smoke`
approved_target_msi_sha256: `37c3163fa024703c3e285d6c8f2343422437b9db933e78a08df4acce88b120f1`
update_package_sha256: `a504d2304544b1bf9ba468d5256d5e51951be9ae5fd7e56a45ed437f2e7c077e`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261110-04300-04301-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다. baseline package는 train `0.43.0` Task 1의 `artifacts/admin-smoke-package-20261110-04300`이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-11-10 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.43.1-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261110-04300-04301` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-11-10 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `37c3163f…`다. fullgate operational MSI(`f4f3f30d…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Manual-admin campaign consume `0.42.95-admin-smoke` -> `0.42.96-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-11-04295-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261011-04295-04296`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296`
consume_manifest: `artifacts/manual-admin-campaign-20261011-04295-04296/consume-manifest.json`
consume_manifest_sha256: `2ba14da45a21339e681ee63907b8eb3738fa0300ef953fa98676adde86321549`
descriptor_batch_id: `manual-admin-campaign-20261011-04295-04296-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261011-04295-04296/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `d96d9ec10b2e0f33bce263452e80bd2a573ad59825c4eab9b9c83727cb8a60a7`
descriptor_summary_sha256: `b0603bd71eb3f7cc5d8669c3d5b83370219e6c87dc9157a9371b2ba72f8c8007`
source_descriptor_batch_id: `manual-admin-campaign-20261011-04295-04296-closed`
baseline_version: `0.42.95-admin-smoke`
target_version: `0.42.96-admin-smoke`
release_train: `0.42.96-admin-smoke`
approved_target_msi_sha256: `326b867a161ffa5038f4b3cecd8405ca0999cff00875c4a9fb8f5ac92f3cdeed`
update_package_sha256: `e4324e2420724dc4383b142debb600465dee84d415243183f138e602530c6a94`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261011-04295-04296-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-10-11 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.96-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261011-04295-04296` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-11 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `326b867a…`다. fullgate operational MSI(`37bfc1ff…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

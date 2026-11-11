# Manual-admin campaign consume `0.43.6-admin-smoke` -> `0.43.7-admin-smoke`

evidence_id: `manual-admin-campaign-2026-11-12-04306-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261111-04306-04307-r2`
artifact_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2`
consume_manifest: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/consume-manifest.json`
consume_manifest_sha256: `866a4dd36f7dc4f35b1d18cdee4f34fbfced4a7e46e5411491269e578f67f7ea`
descriptor_batch_id: `manual-admin-campaign-20261111-04306-04307-r2-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `ac229c81fd78b79659368a9d39b34cf4509badc577e09da17e555975c24a49c3`
descriptor_summary_sha256: `d1cb9a2cb65880c79ca51a7d72e589c725636e6f1d13068dae3b5782756e9787`
source_descriptor_batch_id: `manual-admin-campaign-20261111-04306-04307-r2-closed`
baseline_version: `0.43.6-admin-smoke`
target_version: `0.43.7-admin-smoke`
release_train: `0.43.7-admin-smoke`
approved_target_msi_sha256: `a808cda4d54eae4caf012a8d7b012663b6caa46f683741599150dae7e0223a31`
update_package_sha256: `fcc5182ac1a2e44b8ea09a5e462f1b7c6ec2b2cba4a04c9fb10b43774dbafe7d`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.7-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261111-04306-04307-r2-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다. baseline package는 정차한 train `0.43.6` Task 1의 `artifacts/admin-smoke-package-20261111-04306`이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 2026-11-12 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.43.7-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261111-04306-04307-r2` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-11-11 orchestrator `-Execute` 재실행(`-r2`)에서 host mutation을 수행했다. 첫 실행 `manual-admin-campaign-20261111-04306-04307`은 clean-host guest의 Windows Update 다운로드 실패(`0x80246007`)로 FAIL이었고 consume하지 않았다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `a808cda4…`다. fullgate operational MSI(`e6a89367…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

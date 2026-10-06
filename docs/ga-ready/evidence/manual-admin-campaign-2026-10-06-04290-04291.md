# Manual-admin campaign consume `0.42.90-admin-smoke` -> `0.42.91-admin-smoke`

evidence_id: `manual-admin-campaign-2026-10-06-04290-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20261006-04290-04291`
artifact_root: `artifacts/manual-admin-campaign-20261006-04290-04291`
consume_manifest: `artifacts/manual-admin-campaign-20261006-04290-04291/consume-manifest.json`
consume_manifest_sha256: `e6c7db3153376e2f741883e16a3c008d01034138a2e6a19f2ecea0e0d5a58b03`
descriptor_batch_id: `manual-admin-campaign-20261006-04290-04291-closed`
descriptor_path: `artifacts/manual-admin-campaign-20261006-04290-04291/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `6f4fc0de291a7b8c3d9dd255ca355b4a3c0d44e333dc8ccf2eb0539a42cb0866`
descriptor_summary_sha256: `5f1e059b5037c06bc72afaff188591fb479519d8b01ca371d0fdfbcb78611e29`
source_descriptor_batch_id: `manual-admin-campaign-20261006-04290-04291-closed`
baseline_version: `0.42.90-admin-smoke`
target_version: `0.42.91-admin-smoke`
release_train: `0.42.91-admin-smoke`
approved_target_msi_sha256: `bdef7609de3667298325d162641578a85e191ed31075cc6238e8e0f79fbfc12f`
update_package_sha256: `f31ae34ca27932892fc6aad9255122b88bc8fa15a4518bab63e6a4eca6a43568`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 한 root로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

pair orchestrator `Invoke-PcvManualAdminPackagePairCampaign.ps1`가 여섯 bucket을 처음부터 이 root에 쓰고, 닫을 때 같은 root로 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행해 `manual-admin-campaign-20261006-04290-04291-closed`를 만들었다(`DEVELOPMENT_PROCEDURE.md` §10). 따로 모으는 단계가 없으므로 source descriptor와 consume descriptor가 같다. consume manifest는 그 descriptor가 읽은 summary JSON `7`개를 Lane 3에서 SHA-256과 함께 적은 것이다.

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.91-admin-smoke`로 썼다.

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
| readiness | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20261006-04290-04291` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-10-06 orchestrator `-Execute`에서 host mutation을 수행했다. 이 consume은 파일을 복사하지 않았다(같은 root, `bulky_binaries_not_copied=true`).

pair target은 clean package MSI `bdef7609…`다. fullgate operational MSI(`46dddccb…`)는 pair에 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Manual-admin campaign consume `0.42.83-admin-smoke` -> `0.42.84-admin-smoke`

evidence_id: `manual-admin-campaign-2026-09-30-04283-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
campaign_id: `manual-admin-campaign-20260930-04283-04284`
artifact_root: `artifacts/manual-admin-campaign-20260930-04283-04284`
consume_manifest: `artifacts/manual-admin-campaign-20260930-04283-04284/consume-manifest.json`
consume_manifest_sha256: `4553917d93f8c13406bf5ecdacef4138a8301d5365818971321f36b33785afae`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260930-04283-04284-consume`
descriptor_path: `artifacts/manual-admin-campaign-20260930-04283-04284/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `d6fafd2f00d2659c9f08a2173cd659b923be8e3b246c55061aecbb021d4986ed`
descriptor_summary_sha256: `904ea8510825d51a1c0b15c5a0164152811906061e44da6597fd754592983db6`
source_descriptor_batch_id: `manual-admin-campaign-descriptor-20260930-04283-04284`
baseline_version: `0.42.83-admin-smoke`
target_version: `0.42.84-admin-smoke`
approved_target_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
update_package_sha256: `09b22e1c1d7a186e955e65309f46690a8a794be0f419800e09a574f5470edb90`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 consume 기록이다. 이미 PASS한 여섯 bucket summary의 JSON을 0.42.83 consume과 같은 single-root 배치로 모은 뒤 `New-PcvManualAdminCampaignDescriptor -PlanOnly`를 실행했고, 결과는 `overall_status=pass`다.

2026-09-30 원본 descriptor(`manual-admin-campaign-descriptor-20260930-04283-04284`)는 여섯 원본 root를 직접 읽었다. 이 consume은 같은 입력을 한 root로 묶었다. 같은 날짜라 batch id에 `-consume`을 붙여 구분했다. descriptor 도구는 bucket summary 경로를 명시적으로 받는다. 그래서 consume root 안의 경로를 넘겼다(`-CampaignArtifactRoot`는 기록용이다).

consume checkpoint 자체는 MSI, 서비스, Hyper-V mutation을 실행하지 않았다. 같은 날짜 Lane 3가 `docs/ga-ready/current-evidence.json` current를 `0.42.84-admin-smoke`로 쓴다.

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
| readiness | `PASS` | `artifacts/manual-admin-rebaseline-readiness-20260930-04283-04284` | `manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/product-update-rollback-20260930-04283-04284` | `lifecycle/product-update-rollback/update-summary.json`, `rollback-summary.json` |
| dedicated clean-host Windows Update | `PASS` | `artifacts/internal-clean-host-install-update-rollback-smoke-20260930-04283-04284` | `clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/burn-bootstrapper-lifecycle-20260930-04284` | `burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260930-04283-04284-r2` | `msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/installed-runtime-ops-summary-20260930-04283` | `installed-runtime-ops-summary/summary.json` |

원본 bucket 실행은 2026-09-30에 host mutation을 수행했다. 이 consume은 그 JSON evidence `16`개만 복사했다. MSI, bundle, MSIX 같은 큰 바이너리는 복사하지 않았다(`bulky_binaries_not_copied=true`).

pair target은 clean package MSI `12a582ef…`다. fullgate operational MSI(`f9e1e341…`)는 쓰지 않았다(`operational_fullgate_msi_not_used=true`).

## Nonclaims

- 이 consume은 host mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

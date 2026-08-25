# Installed operator surface current-card 2026-05-17 0.42.27

evidence_id: `installed-operator-surface-current-card-2026-05-17-04227`
result: `PASS`
scope: `installed-web-tui-cli-current-card`
artifact_root: `artifacts/installed-operator-surface-current-card-20260517-04227`
summary: `artifacts/installed-operator-surface-current-card-20260517-04227/summary.json`
manual_admin_recheck_summary: `artifacts/manual-admin-campaign-20260517-04226-04227/installed-runtime-ops-summary/current-card-recheck-after-docs/summary.json`
version: `0.42.27-admin-smoke`
manifest_version: `0.42.27-admin-smoke`
latest_batch_id: `full-admin-host-mutation-gate-20260517-04227`
latest_batch_status: `available`
latest_release_msi_sha256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
latest_release_provenance_commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
runtime_api_current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
runtime_api_registry_bridge_route_count: `4`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
manual_admin_package_pair: `0.42.26-admin-smoke -> 0.42.27-admin-smoke`
manual_admin_current_card_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04226-04227-closed`
manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-campaign-descriptor/summary.json`
manual_admin_update_zip_sha256: `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`
manual_admin_descriptor_overall_status: `pass`
manual_admin_descriptor_missing_count: `0`
manual_admin_descriptor_not_pass_count: `0`
host_mutation_performed: `true`
distribution_decision: `internal-private-network-only`
adr: `ADR-0006`
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 설치된 `0.42.27-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
current-card를 보는지 재확인한 결과다. Descriptor closure 이후 summary를
`artifacts/manual-admin-campaign-20260517-04226-04227/installed-runtime-ops-summary/current-card-recheck-after-docs/summary.json`에도
복사해 manual-admin campaign closure와 연결했다.

## Surface Smoke

| 항목 | 결과 |
| --- | --- |
| service state | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401`, `PCV_AUTH_REQUIRED` |
| CLI ops summary | `ok=true` |
| TUI runtime smoke | `pass` |
| token/password 노출 | `false` |

## Current-card Contract

Installed current-card는 latest batch
`full-admin-host-mutation-gate-20260517-04227`, installed runtime
`0.42.27-admin-smoke`, Runtime/API current evidence
`runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다.

Host Ops lifecycle descriptor bridge도 current-card에 연결됐다. Contract key는
`host-ops-lifecycle-descriptor-bridge-v1`이고 lifecycle bucket contract는
`service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`,
bucket count는 `6`이다.

Manual-admin current-card는 package-pair
`0.42.26-admin-smoke -> 0.42.27-admin-smoke`, descriptor
`manual-admin-campaign-descriptor-20260517-04226-04227-closed`,
`missing_count=0`, `not_pass_count=0`을 표시한다.

이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable publication,
외부 stable publication evidence가 아니다.

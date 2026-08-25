# Installed operator surface current-card 2026-05-17 0.42.28

evidence_id: `installed-operator-surface-current-card-2026-05-17-04228`
result: `PASS`
scope: `installed-web-tui-cli-current-card`
artifact_root: `artifacts/installed-operator-surface-current-card-20260517-04228`
summary: `artifacts/installed-operator-surface-current-card-20260517-04228/summary.json`
version: `0.42.28-admin-smoke`
manifest_version: `0.42.28-admin-smoke`
latest_batch_id: `full-admin-host-mutation-gate-20260517-04228`
latest_batch_status: `available`
latest_release_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
clean_package_msi_sha256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
latest_release_provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
runtime_api_current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
runtime_api_registry_bridge_route_count: `4`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
host_mutation_performed: `true`
distribution_decision: `internal-private-network-only`
adr: `ADR-0006`
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 설치된 `0.42.28-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
ops summary current-card 출처를 읽고, full-gate batch evidence root가
`full-admin-host-mutation-gate-20260517-04228`을 latest로 표시하는지 재확인한 결과다.
설치 서비스에는 `RepairInstalled -BatchEvidenceRoot artifacts`를 적용해 현재 worktree
evidence root를 service PathName에 연결했다.

## Surface Smoke

| 항목 | 결과 |
| --- | --- |
| service state | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401`, `PCV_AUTH_REQUIRED` |
| CLI ops summary | `ok=true`, latest batch `full-admin-host-mutation-gate-20260517-04228` |
| TUI runtime smoke | `pass`, `pcvtui --smoke-once runtime` |
| token/password 노출 | `false` |

## Current-card Contract

Installed current-card는 latest batch
`full-admin-host-mutation-gate-20260517-04228`, installed runtime
`0.42.28-admin-smoke`, Runtime/API current evidence
`runtime-api-current-evidence-rollup-v1`, Runtime/API registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다. Full-gate MSI SHA-256은
`223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`, clean package
MSI SHA-256은 `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`,
provenance commit은 `b9676f6dc37d667ae0d60367e9f4e576a27e3864`다.

Host Ops lifecycle descriptor bridge도 current-card에 연결됐다. Contract key는
`host-ops-lifecycle-descriptor-bridge-v1`이고 lifecycle bucket contract는
`service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`,
bucket count는 `6`이다. 이번 Operator Surface product payload는 같은 bucket data를
Web diagnostics panel의 table로 렌더링하며 UI contract key를
`host-ops-web-diagnostics-bucket-table-v1`로 기록한다.

이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable publication,
외부 stable publication evidence가 아니다.

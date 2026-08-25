# Installed operator surface current-card 2026-05-17 0.42.29

evidence_id: `installed-operator-surface-current-card-2026-05-17-04229`
result: `PASS`
scope: `installed-web-tui-cli-current-card`
artifact_root: `artifacts/installed-operator-surface-current-card-20260517-04229`
summary: `artifacts/installed-operator-surface-current-card-20260517-04229/summary.json`
version: `0.42.29-admin-smoke`
manual_admin_package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
manual_admin_update_zip_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
distribution_decision: `ADR-0006 internal-private-network-only`
manifest_version: `0.42.29-admin-smoke`
latest_batch_id: `full-admin-host-mutation-gate-20260517-04229`
batch_evidence_status: `available`
latest_release_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
clean_package_msi_sha256: `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77`
latest_release_provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
runtime_current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
runtime_api_registry_bridge_route_count: `4`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
tui_smoke: `pass`
cli_ops_summary: `pass`
web_console: `pass`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 설치된 `0.42.29-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
operational current-card를 표시하는지 재확인한 결과다. Web/TUI/CLI current-card는
`full-admin-host-mutation-gate-20260517-04229`을 latest로 표시했고 Runtime/API current
evidence contract와 Host Ops lifecycle bucket table contract도 같은 summary에 노출됐다.

## 확인 결과

| 항목 | 결과 |
| --- | --- |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| unauth runtime policy | HTTP `401`, expected auth boundary |
| CLI ops summary | `ok=true`, latest batch `full-admin-host-mutation-gate-20260517-04229` |
| TUI helper | `installed_tui_operator_smoke=pass` |
| Current evidence contract | `runtime-api-current-evidence-rollup-v1` |
| Runtime bridge | `runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4` |
| Host Ops bucket table | `host-ops-web-diagnostics-bucket-table-v1`, bucket count `6` |

Latest release MSI SHA-256은
`2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`, clean package
MSI SHA-256은 `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77`,
provenance commit은 `d306712ad671c8a00d5c560765b8952e24a07502`다.

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

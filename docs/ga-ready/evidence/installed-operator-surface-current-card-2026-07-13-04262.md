# Installed operator surface current-card 2026-07-13 0.42.62

evidence_id: `installed-operator-surface-current-card-2026-07-13-04262`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.62-admin-smoke`
installed_manifest_version: `0.42.62-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260713-04262`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260713-04262/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260713-04262`
clean_package_msi_sha256: `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`
operational_fullgate_msi_sha256: `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`
clean_package_payload_aggregate_sha256: `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`
operational_fullgate_payload_aggregate_sha256: `ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`
provenance_commit: `7f71f0a518c5b592f233373522d36b5401c3f1df`
cli_exit_zero_count: `5`
tui_exit_zero_count: `2`
web_http_200_count: `3`
service_state: `Running/Automatic`
token_value_observed: `false`
password_value_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-fullgate`
latest_manual_admin_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Installed current-card

- `Default Switch`: `internal`, `allow_management_os=true`
- `WSL (Hyper-V firewall)`: `internal`, `allow_management_os=true`
- CLI: `ops summary` JSON/table, `host status`, `vm list`, `network inventory` 모두 exit `0`
- TUI: `--smoke-once runtime`, `--smoke-once job` 모두 exit `0`
- Web: `/`, `/pcv-config.js`, `/app.js` 모두 HTTP `200`
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`
- Dynamic command stdout/stderr secret scan: token/password value 모두 관찰되지 않음

Current-card는 full-gate 이후 read-only smoke다. 별도 manual-admin campaign을 실행하지 않았으며
최신 closed package-pair는 계속 `0.42.58-admin-smoke -> 0.42.59-admin-smoke`다.

이 evidence는 installed internal admin-smoke operator surface 증거이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.

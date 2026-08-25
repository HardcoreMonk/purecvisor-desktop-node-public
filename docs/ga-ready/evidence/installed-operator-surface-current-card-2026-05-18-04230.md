# Installed operator surface current-card 2026-05-18 0.42.30

evidence_id: `installed-operator-surface-current-card-2026-05-18-04230`
result: `PASS`
scope: `installed-web-tui-cli-current-card`
adr: `ADR-0006`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260518-04230-r2`
summary: `artifacts/installed-operator-surface-current-card-20260518-04230-r2/summary.json`
version: `0.42.30-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04230.md`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-18-04229-04230.md`
package_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
manifest_version: `0.42.30-admin-smoke`
service_state: `Running`
msi_sha256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
payload_aggregate_sha256: `0fddc06c7ced0239ea04a89fd90cc0c152a64688904e0f58b97c3fcd5368a28c`
provenance_commit: `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
runtime_policy_unauthenticated_status_code: `401`
runtime_policy_boundary_status: `expected-auth-boundary`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_source: `default-protected-token-file-auto-discovery`
protected_token_file_exists: `true`
token_value_observed: `false`
password_value_observed: `false`
batch_evidence_status: `available`
batch_evidence_configured: `true`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260518-04230`
current_manual_admin_package_pair: `0.42.29-admin-smoke -> 0.42.30-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260518-04229-04230-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.30-admin-smoke` full admin host mutation gate와
`0.42.29-admin-smoke -> 0.42.30-admin-smoke` manual-admin package-pair closure 이후,
설치된 Web/TUI/CLI current-card가 같은 batch/descriptor를 operator surface에 노출하는지
재확인한 결과다.

## 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.30-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running` |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| unauth runtime policy | HTTP `401`, expected auth boundary |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0` |
| TUI runtime smoke | `pcvtui --smoke-once --no-color runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260518-04230` |
| manual-admin current pair | `0.42.29-admin-smoke -> 0.42.30-admin-smoke` |
| descriptor closure | `manual-admin-campaign-descriptor-20260518-04229-04230-closed`, `missing_count=0`, `not_pass_count=0` |

## Token/PATH 확인

`pcvcli.exe`와 `pcvtui.exe`는 새 shell의 machine `PATH`에서
`C:\Program Files\PureCVisor\DesktopNode\...`로 resolve됐다. Smoke 명령은
`--token`, `--token-file`, `--token-env`, `--protected-token-file` 없이 실행했고,
기본 protected token file auto discovery를 사용했다. Bearer token, password,
refresh token, JWT signing key 값은 stdout/stderr 또는 summary에 기록하지 않았다.

## Current-card 경계

이번 current-card는 full admin host mutation batch root가 service `PathName`에 연결된
설치본 기준이다. 따라서 `pcvcli --json ops summary`의 `batch_evidence.status`는
`available`이고, Host Ops lifecycle bucket count `6`, runtime route count `4`,
manual-admin descriptor closure id가 같은 payload에 함께 노출된다.

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

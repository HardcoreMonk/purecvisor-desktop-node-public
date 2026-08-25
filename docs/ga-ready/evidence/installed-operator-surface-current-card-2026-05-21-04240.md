# 설치본 operator surface current-card 2026-05-21 0.42.40

evidence_id: `installed-operator-surface-current-card-2026-05-21-04240`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04240-package-chain-closure`
adr: `ADR-0007`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260521-04240`
summary: `artifacts/installed-operator-surface-current-card-20260521-04240/summary.json`
version: `0.42.40-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`
manual_admin_previous_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md`
manifest_version: `0.42.40-admin-smoke`
service_state: `Running`
service_path_has_batch_evidence_root: `true`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
package_msi_sha256: `4979a3a60f96b8e8dbcda41bd722c33909c2faf39bc4cf88b8a79fb89e9628e8`
full_gate_msi_sha256: `eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`
payload_aggregate_sha256: `0c5e566f49bd4ef5c78249b3439a4441462a3c6b54433985be4b9badb9618666`
full_gate_payload_aggregate_sha256: `cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5`
provenance_commit: `adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
api_policy_unauth_boundary_status_code: `401`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
token_source: `default-protected-token-file-auto-discovery`
batch_evidence_status: `available`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260521-04240`
current_full_admin_host_mutation: `0.42.40-admin-smoke`
current_manual_admin_package_pair: `0.42.39-admin-smoke -> 0.42.40-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260521-04239-04240-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `false`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 설치본 `0.42.40-admin-smoke`에서 Web/TUI/CLI current-card를 재확인한
기록이다. `0.42.40` full admin host mutation gate와 manual-admin package-pair closure가
ops summary/current evidence rollup에 현재 anchor로 노출된다.

## Current-card 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.40-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running`, `--batch-evidence-root` present |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| API unauth boundary | `/api/v1/runtime/policy` HTTP `401`, `PCV_AUTH_REQUIRED` 경계 |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list JSON | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0`, `batch_evidence.status=available` |
| TUI runtime smoke | `pcvtui --smoke-once runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260521-04240` |
| manual-admin current pair | `0.42.39-admin-smoke -> 0.42.40-admin-smoke` |

## Ops summary 확인

`artifacts/installed-operator-surface-current-card-20260521-04240/pcvcli-json-ops-summary.stdout.txt`는
`batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260521-04240`,
`current_evidence.full_admin_host_mutation.latest.version=0.42.40-admin-smoke`,
`manual_admin.latest_package_pair.package_pair=0.42.39-admin-smoke -> 0.42.40-admin-smoke`,
`manual_admin.latest_package_pair.descriptor_batch_id=manual-admin-campaign-descriptor-20260521-04239-04240-closed`를
노출했다. Host Ops lifecycle descriptor는 bucket count `6`과
`host-ops-lifecycle-descriptor-bridge-v1` contract를 유지한다.

## Token/PATH 확인

Smoke 명령은 token 인자를 직접 전달하지 않고 default token discovery를 사용했다.
`pcvcli.exe`, `pcvtui.exe`는 `C:\Program Files\PureCVisor\DesktopNode`에서 전역 PATH로
해결됐다. Bearer token, password, refresh token, JWT signing key 값은 stdout/stderr 또는
summary에 기록하지 않았다.

## 경계

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

# 설치본 operator surface current-card 2026-05-20 0.42.39

evidence_id: `installed-operator-surface-current-card-2026-05-20-04239`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-pcvcli-hyperv-qos-guest-service-slice`
adr: `ADR-0007`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260520-04239-final`
summary: `artifacts/installed-operator-surface-current-card-20260520-04239-final/summary.json`
version: `0.42.39-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04239.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`
manual_admin_previous_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04237-04238.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04239-hostmutation.md`
manifest_version: `0.42.39-admin-smoke`
service_state: `Running`
service_path_has_batch_evidence_root: `true`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
package_msi_sha256: `b6fac120b145b5d0a8bf48a955037593756613d5bbe355bae96de59da4f0d805`
full_gate_msi_sha256: `8ccf24a0a304b82dfcb0039c92149806539cf74977014bc3468c589e4ddf624f`
payload_aggregate_sha256: `359aee4c862fb4efc35a1dd631c92219e62e87adf7e96c8134d687fe38c7dede`
full_gate_payload_aggregate_sha256: `cd2d820c66e6f28df8a740207c7182ab744d5d984fc3bfc6a009a35da95c0869`
provenance_commit: `6fd931baf3de77435d0d11b92424cf6657ea4515`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
token_source: `default-protected-token-file-auto-discovery`
batch_evidence_status: `available`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260520-04239`
current_full_admin_host_mutation: `0.42.39-admin-smoke`
current_manual_admin_package_pair: `0.42.38-admin-smoke -> 0.42.39-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04238-04239-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `true`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 설치본 `0.42.39-admin-smoke`에서 Web/TUI/CLI current-card를 다시 확인한
기록이다. `0.42.39` full admin host mutation gate는 current full-gate anchor로 표시되고,
manual-admin package-pair도 Windows Update clean-host closure 이후
`0.42.38-admin-smoke -> 0.42.39-admin-smoke`로 표시된다.

## Current-card 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.39-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running`, `--batch-evidence-root` present |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list JSON | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0`, `batch_evidence.status=available` |
| TUI runtime smoke | `pcvtui --smoke-once runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260520-04239` |
| manual-admin current pair | `0.42.38-admin-smoke -> 0.42.39-admin-smoke` |

## Ops summary 확인

`artifacts/installed-operator-surface-current-card-20260520-04239-final/pcvcli-json-ops-summary.stdout.txt`는
`batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260520-04239`,
`current_evidence.full_admin_host_mutation.latest.version=0.42.39-admin-smoke`,
`manual_admin.latest_package_pair.package_pair=0.42.38-admin-smoke -> 0.42.39-admin-smoke`,
`manual_admin.latest_package_pair.descriptor_batch_id=manual-admin-campaign-descriptor-20260520-04238-04239-closed`를
노출했다. Host Ops lifecycle descriptor는 bucket count `6`과
`host-ops-lifecycle-descriptor-bridge-v1` contract를 유지한다.

## Token/PATH 확인

Smoke 명령은 token 인자를 직접 전달하지 않고 default token discovery를 사용했다.
Bearer token, password, refresh token, JWT signing key 값은 stdout/stderr 또는 summary에
기록하지 않았다.

## 경계

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

# 설치본 operator surface current-card 2026-05-20 0.42.38

evidence_id: `installed-operator-surface-current-card-2026-05-20-04238`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-vm-media-resource-mutation-slice`
adr: `ADR-0006`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260520-04238-closure-r4`
summary: `artifacts/installed-operator-surface-current-card-20260520-04238-closure-r4/summary.json`
version: `0.42.38-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04238.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04237-04238.md`
manual_admin_previous_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04238-hostmutation.md`
manifest_version: `0.42.38-admin-smoke`
service_state: `Running`
service_path_has_batch_evidence_root: `true`
package_msi_sha256: `2ae739cee46780b01d1c3873d8186c30761243df578ecf7ab1e9d66a19f572b4`
full_gate_msi_sha256: `b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f`
payload_aggregate_sha256: `40ec6157c99dffaf29bf9d0dcd1c513ba99fee77c21bb883976aa03eb3b73ca7`
full_gate_payload_aggregate_sha256: `ab5cb6404e8f482ad3ecb32b087cb7e5020aceca595adb0fa01e3aa26d2317b8`
provenance_commit: `3c49b9a010c57e4a8637cb32ed17cd432dd0cd6f`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
batch_evidence_status: `available`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260520-04238`
current_full_admin_host_mutation: `0.42.38-admin-smoke`
current_manual_admin_package_pair: `0.42.37-admin-smoke -> 0.42.38-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04237-04238-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
latest_manual_admin_candidate_package_pair: `0.42.37-admin-smoke -> 0.42.38-admin-smoke`
latest_manual_admin_candidate_status: `pass-closed`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 설치본 `0.42.38-admin-smoke`에서 Web/TUI/CLI current-card를 다시 확인한
기록이다. `0.42.38` full admin host mutation gate는 current full-gate anchor로 표시되고,
manual-admin package-pair도 Windows Update clean-host rerun closure 이후
`0.42.37-admin-smoke -> 0.42.38-admin-smoke`로 표시된다.

## Current-card 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.38-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running`, `--batch-evidence-root` present |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list JSON | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0`, `batch_evidence.status=available` |
| TUI runtime smoke | `pcvtui --smoke-once --no-color runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260520-04238` |
| manual-admin current pair | `0.42.37-admin-smoke -> 0.42.38-admin-smoke` |
| manual-admin latest candidate | `0.42.37-admin-smoke -> 0.42.38-admin-smoke`, `pass-closed` |

## Ops summary 확인

`artifacts/installed-operator-surface-current-card-20260520-04238-closure-r4/pcvcli-ops-summary-json.txt`는
`batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260520-04238`,
`current_evidence.full_admin_host_mutation.latest.version=0.42.38-admin-smoke`,
`release.msi_sha256=b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f`,
`manual_admin.latest_package_pair.package_pair=0.42.37-admin-smoke -> 0.42.38-admin-smoke`를
노출했다. Host Ops lifecycle descriptor는 bucket count `6`과
`host-ops-lifecycle-descriptor-bridge-v1` contract를 유지한다.

## Token/PATH 확인

Smoke 명령은 token 인자를 직접 전달하지 않고 default token discovery를 사용했다.
Bearer token, password, refresh token, JWT signing key 값은 stdout/stderr 또는 summary에
기록하지 않았다.

## 경계

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

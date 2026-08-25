# Installed operator surface current-card 2026-05-19 0.42.34

evidence_id: `installed-operator-surface-current-card-2026-05-19-04234`
result: `PASS`
scope: `installed-web-tui-cli-current-card-fullgate-closure-smoke`
adr: `ADR-0006`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260519-04234`
summary: `artifacts/installed-operator-surface-current-card-20260519-04234/summary.json`
version: `0.42.34-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`
manifest_version: `0.42.34-admin-smoke`
service_state: `Running`
service_path_has_batch_evidence_root: `true`
msi_sha256: `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`
payload_aggregate_sha256: `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`
provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
runtime_policy_unauthenticated_status_code: `401`
runtime_policy_boundary_status: `expected-auth-boundary`
cli_interactive_linux_palette: `pass`
cli_interactive_utf8_prompt: `pass`
cli_interactive_no_color: `pass`
cli_host_status: `pass`
cli_vm_list_table: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
machine_path_contains_install_dir: `true`
current_process_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_source: `default-protected-token-file-auto-discovery`
protected_token_file_exists: `true`
token_value_observed: `false`
password_value_observed: `false`
batch_evidence_status: `available`
batch_evidence_configured: `true`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260519-04234`
current_full_admin_host_mutation: `0.42.34-admin-smoke`
current_manual_admin_package_pair: `0.42.32-admin-smoke -> 0.42.34-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260519-04232-04234-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `true`
full_admin_host_mutation_campaign_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.34-admin-smoke` full admin host mutation gate와
`0.42.32-admin-smoke -> 0.42.34-admin-smoke` manual-admin package-pair closure 이후,
설치본 Web/TUI/CLI가 자동 token discovery와 Linux-style PCVCLI interactive shell을
실제 product root에서 실행하는지 확인한 current-card smoke다.

## 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.34-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running` |
| Service PathName | `--batch-evidence-root "D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts"` 포함 |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| unauth runtime policy | HTTP `401`, expected auth boundary |
| CLI interactive neon | `pcvcli --interactive`, exit `0`, `38;5;33`/`38;5;198`/`38;5;51`/`38;5;46`, prompt `❯`, 한 줄 command row 확인 |
| CLI interactive no-color | `pcvcli --interactive --no-color`, exit `0`, ANSI 없음, prompt `(pcv) >`, `vm create | Create a new VM` 확인 |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list table | `pcvcli vm list`, exit `0`, 현재 host VM count `0`, `No VMs found.` |
| CLI VM list JSON | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0`, `batch_evidence.status=available` |
| TUI runtime smoke | `pcvtui --smoke-once --no-color runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260519-04234` |
| manual-admin current pair | `0.42.32-admin-smoke -> 0.42.34-admin-smoke` |
| descriptor closure | `manual-admin-campaign-descriptor-20260519-04232-04234-closed`, `missing_count=0`, `not_pass_count=0` |

## Token/PATH 확인

Machine `PATH`와 현재 smoke process PATH에서 `C:\Program Files\PureCVisor\DesktopNode`가
확인됐고, `pcvcli.exe`와 `pcvtui.exe`는 설치된 product root에서 resolve됐다.

Smoke 명령은 `--token`, `--token-file`, `--token-env`, `--protected-token-file` 없이
실행했고, 기본 protected token file auto discovery를 사용했다. Bearer token,
password, refresh token, JWT signing key 값은 stdout/stderr 또는 summary에 기록하지
않았다.

## Batch evidence root repair

직접 MSI install/update 후 service `PathName`에서 `--batch-evidence-root`가 빠지는 것을
확인했다. 설치본 current-card smoke 전에
`packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action RepairInstalled`
를 실행해 batch evidence root를 `artifacts`로 재연결했고, service health와
`pcvcli --json ops summary`에서 `batch_evidence.status=available`을 확인했다.

## Current-card 경계

이번 smoke의 설치본 package version, ops summary `installed_runtime.version`,
`batch_evidence.latest.release.version`, manual-admin latest package-pair는 모두
`0.42.34-admin-smoke` closure 기준으로 정렬됐다. PCVCLI Linux palette/UTF-8 product
payload도 이 설치본 current-card에서 같이 검증했다.

이 evidence는 internal admin-smoke 설치본 current-card smoke다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

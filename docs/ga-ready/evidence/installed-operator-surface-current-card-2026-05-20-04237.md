# Installed operator surface current-card 2026-05-20 0.42.37

evidence_id: `installed-operator-surface-current-card-2026-05-20-04237`
result: `PASS`
scope: `installed-web-tui-cli-current-card-and-actual-vm-lifecycle-smoke`
adr: `ADR-0006`
distribution_scope: `internal-private-network-only`
artifact_root: `artifacts/installed-operator-surface-current-card-20260520-04237`
summary: `artifacts/installed-operator-surface-current-card-20260520-04237/summary.json`
version: `0.42.37-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04235-hostmutation.md`
manifest_version: `0.42.37-admin-smoke`
service_state: `Running`
service_path_has_batch_evidence_root: `true`
msi_sha256: `05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e`
payload_aggregate_sha256: `1e2487bfe474daad624a3ef67837a278ab5d25a71c654f8b7c18c95e3cc94e9e`
provenance_commit: `9bed10099e1455717c89c8b2cc7481251705d609`
signing_mode: `AllowUnsignedDev`
web_console: `pass`
web_console_status_code: `200`
pcv_config_status_code: `200`
runtime_policy_unauthenticated_status_code: `401`
cli_interactive_linux_palette: `pass`
cli_interactive_no_color: `pass`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
actual_vm_lifecycle_smoke: `pass`
actual_vm_lifecycle_artifact: `artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`
batch_evidence_status: `available`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260520-04235`
current_full_admin_host_mutation: `0.42.35-admin-smoke`
current_manual_admin_package_pair: `0.42.34-admin-smoke -> 0.42.35-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04234-04235-closed`
current_manual_admin_descriptor_missing_count: `0`
current_manual_admin_descriptor_not_pass_count: `0`
current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
runtime_api_registry_bridge_route_count: `4`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 설치본 `0.42.37-admin-smoke`에서 Web/TUI/CLI current-card와 실제 Hyper-V VM
lifecycle smoke를 함께 닫은 기록이다. `0.42.35-admin-smoke` full gate/manual-admin closure를
current operational anchor로 유지하면서, Hyper-V pause fast-follow fix가 설치본 CLI에서 실제로
동작하는지 확인했다.

## Current-card 확인 결과

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.37-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running` |
| Web `/` | HTTP `200` |
| Web `/pcv-config.js` | HTTP `200` |
| unauth runtime policy | HTTP `401`, expected auth boundary |
| CLI interactive neon | `pcvcli --interactive`, exit `0`, Linux-style 256-color palette와 한 줄 command row 확인 |
| CLI interactive no-color | `pcvcli --interactive --no-color`, exit `0`, ANSI 없음, `(pcv) >`, `vm create | Create a new VM` 확인 |
| CLI host status | `pcvcli host status`, exit `0` |
| CLI VM list JSON | `pcvcli --json vm list`, exit `0`, JSON `ok=true`, VM count `0` |
| CLI ops summary | `pcvcli --json ops summary`, exit `0`, `batch_evidence.status=available` |
| TUI runtime smoke | `pcvtui --smoke-once --no-color runtime`, exit `0` |
| batch evidence | `available`, latest batch `full-admin-host-mutation-gate-20260520-04235` |
| manual-admin current pair | `0.42.34-admin-smoke -> 0.42.35-admin-smoke` |
| descriptor closure | `manual-admin-campaign-descriptor-20260520-04234-04235-closed`, `missing_count=0`, `not_pass_count=0` |

## 실제 VM lifecycle smoke

`artifacts/installed-cli-vm-lifecycle-smoke-20260520-04237/summary.json`는 설치본 전역
`pcvcli.exe`로 실제 Hyper-V VM `pcv-cli-04237-*`를 생성하고 다음 명령을 모두 PASS로
기록했다.

| 명령 묶음 | 결과 |
| --- | --- |
| `pcvcli --json host status` | `PASS` |
| `pcvcli --json vm list` | `PASS` |
| `pcvcli --json vm create ... --generation 2` | `PASS`, job succeeded |
| `pcvcli --json vm start` | `PASS`, job succeeded |
| `pcvcli --json vm memory-stats` / `cpu-stats` | `PASS` |
| `pcvcli --json vm pause` | `PASS`, job succeeded |
| `pcvcli --json vm get` after pause | `PASS`, paused inventory mapping 확인 |
| `pcvcli --json vm resume` | `PASS`, job succeeded |
| `pcvcli --json vm rename` | `PASS`, job succeeded |
| cleanup `poweroff/delete` | `PASS` |

0.42.35 설치본에서는 `vm.pause`가 WMI `32775`로 실패했고, 0.42.36 중간 빌드에서는
pause job은 성공했지만 paused inventory가 `EnabledState=9`를 `unknown`으로 매핑해
`PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE`가 발생했다. 0.42.37은 두 결함을 모두
닫은 설치본 evidence다.

## Token/PATH 확인

Machine `PATH`와 smoke process PATH에서 `C:\Program Files\PureCVisor\DesktopNode`가 확인됐고,
`pcvcli.exe`와 `pcvtui.exe`는 설치된 product root에서 resolve됐다. Smoke 명령은 token
인자를 직접 전달하지 않고 default token discovery를 사용했다. Bearer token, password,
refresh token, JWT signing key 값은 stdout/stderr 또는 summary에 기록하지 않았다.

## 경계

이 evidence는 internal admin-smoke 설치본 current-card 및 실제 VM lifecycle smoke다.
Public trusted signing, public stable installer URL, winget submission, 외부 stable
publication은 주장하지 않는다.

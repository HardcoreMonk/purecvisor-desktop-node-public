# Installed Operator Surface current-card 2026-05-22 0.42.41

evidence_id: `installed-operator-surface-current-card-2026-05-22-04241`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04241-package-chain-closure`
version: `0.42.41-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260522-04241`
summary: `artifacts/installed-operator-surface-current-card-20260522-04241/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md`
actual_vm_row_projection_evidence: `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`
installed_manifest_version: `0.42.41-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260522-04241`
current_manual_admin_package_pair: `0.42.40-admin-smoke -> 0.42.41-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260522-04240-04241-closed`
installed_tui_actual_vm_row_projection: `pass`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
web_index: `pass`
web_config: `pass`
api_runtime_policy_boundary: `401-or-200-pass`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_value_observed: `false`
password_value_observed: `false`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 current-card는 `0.42.41-admin-smoke` 설치본에서 Web/TUI/CLI 운영자 surface가 같은
current evidence rollup을 보고 있음을 재확인한 기록이다. Web `/`, `/pcv-config.js`,
unauthenticated runtime policy boundary, `pcvcli host status`, `pcvcli --json vm list`,
`pcvcli --json ops summary`, `pcvtui --smoke-once runtime`이 모두 PASS했다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.41-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto`, `--batch-evidence-root` configured |
| command resolution | `pcvcli.exe`, `pcvtui.exe` both resolve from `C:\Program Files\PureCVisor\DesktopNode` |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns auth boundary `401` |
| ops summary full gate | `full-admin-host-mutation-gate-20260522-04241` |
| ops summary manual-admin | `0.42.40-admin-smoke -> 0.42.41-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260522-04240-04241-closed` |
| installed TUI row projection | `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`, PASS |

`artifacts/installed-operator-surface-current-card-20260522-04241/pcvcli-json-ops-summary.stdout.txt`는
`batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260522-04241`,
`current_evidence.full_admin_host_mutation.latest.version=0.42.41-admin-smoke`,
`manual_admin.latest_package_pair.package_pair=0.42.40-admin-smoke -> 0.42.41-admin-smoke`,
`manual_admin.latest_package_pair.descriptor_batch_id=manual-admin-campaign-descriptor-20260522-04240-04241-closed`를
노출한다.

## 경계

이 evidence는 설치본 current-card smoke다. Public trusted signing, public stable installer
URL, winget submission, 외부 stable publication은 주장하지 않는다.

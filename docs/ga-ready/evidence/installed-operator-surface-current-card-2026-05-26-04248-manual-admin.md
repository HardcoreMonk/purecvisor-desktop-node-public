# Installed Operator Surface current-card 2026-05-26 0.42.48 manual-admin closure

evidence_id: `installed-operator-surface-current-card-2026-05-26-04248-manual-admin`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04248-manual-admin-closure`
version: `0.42.48-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin`
summary: `artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`
installed_manifest_version: `0.42.48-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260526-04248`
current_manual_admin_package_pair: `0.42.47-admin-smoke -> 0.42.48-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_source: `default-protected-token-file-auto-discovery`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
web_index: `pass`
web_config: `pass`
api_runtime_policy_boundary: `401-pass`
token_value_observed: `false`
password_value_observed: `false`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 current-card는 `0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair
closure 이후 설치본 Web/TUI/CLI 운영자 surface가 같은 current evidence rollup을 보고
있음을 확인한 기록이다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.48-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto` |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns `401` auth boundary |
| CLI | `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary` exit `0` |
| TUI | `pcvtui --smoke-once runtime` exit `0` |
| ops summary full gate | `full-admin-host-mutation-gate-20260526-04248`, `batch_evidence.status=available` |
| ops summary release | `0.42.48-admin-smoke`, MSI SHA-256 `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf` |
| ops summary manual-admin | `0.42.47-admin-smoke -> 0.42.48-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed` |

`summary.json`는 `ok=true`, `latest_batch_id=full-admin-host-mutation-gate-20260526-04248`,
`current_card_version=0.42.48-admin-smoke`,
`manual_admin_package_pair=0.42.47-admin-smoke -> 0.42.48-admin-smoke`,
`manual_admin_missing_count=0`, `manual_admin_not_pass_count=0`를 기록한다.

## 경계

이 evidence는 설치본 current-card smoke다. Host mutation은 이 smoke 자체가 아니라 선행
full admin gate에서 수행됐다. Public trusted signing, public stable installer URL, winget
submission, 외부 stable publication은 주장하지 않는다.

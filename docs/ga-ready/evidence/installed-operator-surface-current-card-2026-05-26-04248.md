# Installed Operator Surface current-card 2026-05-26 0.42.48

evidence_id: `installed-operator-surface-current-card-2026-05-26-04248`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04248-fullgate-pre-manual-admin-closure`
version: `0.42.48-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260526-04248`
summary: `artifacts/installed-operator-surface-current-card-20260526-04248/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`
installed_manifest_version: `0.42.48-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260526-04248`
current_manual_admin_package_pair: `0.42.45-admin-smoke -> 0.42.47-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04245-04247-closed`
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

이 current-card는 `0.42.48-admin-smoke` 설치본에서 Web/TUI/CLI 운영자 surface가 같은
current evidence rollup을 보고 있음을 확인한 기록이다. Phase 3 Web/TUI QoS direct control
payload는 full admin host mutation gate와 설치본 smoke까지 통과했고, manual-admin
package-pair closure는 최신 닫힌 `0.42.45-admin-smoke -> 0.42.47-admin-smoke` anchor를
계속 참조한다.

2026-05-26 후속 manual-admin campaign이 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`로
닫히면서, 최신 installed current-card claim은
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`가
소유한다. 이 파일은 pre-closure current-card predecessor로 보존한다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.48-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto`, `--batch-evidence-root` configured |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns auth boundary `401` / `PCV_AUTH_REQUIRED` |
| CLI | `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary` exit `0` |
| TUI | `pcvtui --smoke-once runtime` exit `0` |
| ops summary full gate | `full-admin-host-mutation-gate-20260526-04248`, `batch_evidence.status=available` |
| ops summary release | `0.42.48-admin-smoke`, MSI SHA-256 `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf` |
| ops summary manual-admin | `0.42.45-admin-smoke -> 0.42.47-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260526-04245-04247-closed` |

`artifacts/installed-operator-surface-current-card-20260526-04248/summary.json`는
`ops_summary_projection.latest_batch_id=full-admin-host-mutation-gate-20260526-04248`,
`batch_evidence_status=available`, `current_card_version=0.42.48-admin-smoke`,
`manual_admin_package_pair=0.42.45-admin-smoke -> 0.42.47-admin-smoke`,
`manual_admin_missing_count=0`, `manual_admin_not_pass_count=0`를 기록한다.

## 경계

이 evidence는 설치본 current-card smoke다. Host mutation은 이 smoke 자체가 아니라
선행 full admin gate에서 수행됐다. `0.42.47-admin-smoke -> 0.42.48-admin-smoke`
manual-admin package-pair campaign은 후속 evidence에서 닫혔고, 이 파일은 그 전 상태를
보존한다. Public trusted signing, public stable installer URL, winget submission, 외부
stable publication은 주장하지 않는다.

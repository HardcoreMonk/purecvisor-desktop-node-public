# Installed Operator Surface current-card 2026-05-26 0.42.47

evidence_id: `installed-operator-surface-current-card-2026-05-26-04247`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04247-fullgate-and-package-pair-closure`
version: `0.42.47-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260526-04247`
summary: `artifacts/installed-operator-surface-current-card-20260526-04247/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04247.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04247-hostmutation.md`
installed_qos_mutation_smoke: `artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary.json`
installed_manifest_version: `0.42.47-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260526-04247`
current_manual_admin_package_pair: `0.42.45-admin-smoke -> 0.42.47-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04245-04247-closed`
product_repair_batch_root: `pass`
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

이 current-card는 `0.42.47-admin-smoke` 설치본에서 Web/TUI/CLI 운영자 surface가 같은
current evidence rollup을 보고 있음을 재확인한 기록이다. Hyper-V QoS mutation 실제 VM
smoke와 04245→04247 manual-admin package-pair closure를 같은 current anchor로 연결한다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.47-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto`, `--batch-evidence-root` configured |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns auth boundary `401` / `PCV_AUTH_REQUIRED` |
| CLI | `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary` exit `0` |
| TUI | `pcvtui --smoke-once runtime` exit `0` |
| Hyper-V QoS mutation | installed actual VM smoke PASS, `artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary.json` |
| account/noVNC | 04245 account login/browser smoke와 target-backed noVNC streaming smoke를 historical product surface PASS로 유지 |
| ops summary full gate | `full-admin-host-mutation-gate-20260526-04247`, `batch_evidence.status=available` |
| ops summary manual-admin | `0.42.45-admin-smoke -> 0.42.47-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260526-04245-04247-closed` |

`artifacts/installed-operator-surface-current-card-20260526-04247/summary.json`는
`ops.data.current_evidence.full_admin_host_mutation.latest.batch_id=full-admin-host-mutation-gate-20260526-04247`,
`current_card_version=0.42.47-admin-smoke`,
`manual_admin.latest_package_pair.package_pair=0.42.45-admin-smoke -> 0.42.47-admin-smoke`,
`manual_admin.latest_package_pair.descriptor_batch_id=manual-admin-campaign-descriptor-20260526-04245-04247-closed`를
노출한다.

## 경계

이 evidence는 설치본 current-card smoke다. Public trusted signing, public stable installer
URL, winget submission, 외부 stable publication은 주장하지 않는다.

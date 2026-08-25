# Installed Operator Surface current-card 2026-05-25 0.42.44 r2

evidence_id: `installed-operator-surface-current-card-2026-05-25-04244-r2`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-04244-fullgate-and-package-pair-closure`
version: `0.42.44-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260525-04244-r2`
summary: `artifacts/installed-operator-surface-current-card-20260525-04244-r2/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04244.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-25-04243-04244.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-25-04244-hostmutation.md`
installed_manifest_version: `0.42.44-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260525-04244-r2`
current_manual_admin_package_pair: `0.42.43-admin-smoke -> 0.42.44-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260525-04243-04244-closed`
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

이 current-card는 `0.42.44-admin-smoke` 설치본에서 Web/TUI/CLI 운영자 surface가 같은
current evidence rollup을 보고 있음을 재확인한 기록이다. 최초 확인 때 설치된 service
`PathName`에 `--batch-evidence-root`가 없어 ops summary의 `batch_evidence.status`가
`not_configured`였고, product wrapper `RepairInstalled -BatchEvidenceRoot artifacts`로
동일 service path를 보정한 뒤 PASS로 닫았다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.44-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto`, `--batch-evidence-root` configured |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns auth boundary `401` / `PCV_AUTH_REQUIRED` |
| CLI | `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary` exit `0` |
| TUI | `pcvtui --smoke-once runtime` exit `0` |
| ops summary full gate | `full-admin-host-mutation-gate-20260525-04244-r2`, `batch_evidence.status=available` |
| ops summary manual-admin | `0.42.43-admin-smoke -> 0.42.44-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260525-04243-04244-closed` |

`artifacts/installed-operator-surface-current-card-20260525-04244-r2/pcvcli-ops-summary.json`는
`batch_evidence.latest.batch_id=full-admin-host-mutation-gate-20260525-04244-r2`,
`current_evidence.full_admin_host_mutation.latest.version=0.42.44-admin-smoke`,
`manual_admin.latest_package_pair.package_pair=0.42.43-admin-smoke -> 0.42.44-admin-smoke`,
`manual_admin.latest_package_pair.descriptor_batch_id=manual-admin-campaign-descriptor-20260525-04243-04244-closed`를
노출한다.

## 경계

이 evidence는 설치본 current-card smoke다. Public trusted signing, public stable installer
URL, winget submission, 외부 stable publication은 주장하지 않는다.

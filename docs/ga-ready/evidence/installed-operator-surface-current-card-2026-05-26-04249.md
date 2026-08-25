# Installed Operator Surface current-card 2026-05-26 0.42.49

evidence_id: `installed-operator-surface-current-card-2026-05-26-04249`
result: `PASS`
scope: `installed-web-tui-cli-current-card-guest-execution-boundary`
version: `0.42.49-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260526-04249`
summary: `artifacts/installed-operator-surface-current-card-20260526-04249/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md`
manual_admin_latest_closed_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`
installed_manifest_version: `0.42.49-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260526-04249`
current_manual_admin_package_pair: `0.42.47-admin-smoke -> 0.42.48-admin-smoke`
current_manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_source: `default-protected-token-file-auto-discovery`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
cli_runtime_policy: `pass`
tui_smoke_runtime: `pass`
web_index: `pass`
web_config: `pass`
guest_execution_policy: `pass-disabled-boundary`
guest_exec_preview_disabled: `403-PCV_GUEST_EXEC_DISABLED`
token_value_observed: `false`
password_value_observed: `false`
secret_value_observed: `false`
credential_ref_observed: `false`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 current-card는 `0.42.49-admin-smoke` 설치본 Web/TUI/CLI 운영자 surface가 같은 current
evidence rollup을 보고 있고, Guest Execution boundary가 설치본에서 disabled 상태로 안전하게
노출됨을 확인한 기록이다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.49-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running` |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| CLI | `pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`, `pcvcli --json runtime policy` exit `0` |
| TUI | `pcvtui --smoke-once runtime --no-color` exit `0` |
| ops summary full gate | `full-admin-host-mutation-gate-20260526-04249`, `batch_evidence.status=completed` |
| ops summary release | `0.42.49-admin-smoke`, MSI SHA-256 `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32` |
| ops summary manual-admin | `0.42.47-admin-smoke -> 0.42.48-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed` |
| guest execution policy | `enabled=false`, `guest-execution-audit-v1`, `guest-execution-redaction-v1` |
| guest exec preview | HTTP `403`, error `PCV_GUEST_EXEC_DISABLED`, secret/ref echo 없음 |

`summary.json`는 `ok=true`, `latest_batch_id=full-admin-host-mutation-gate-20260526-04249`,
`current_card_version=0.42.49-admin-smoke`, `guest_execution_policy.enabled=false`,
`guest_exec_preview_disabled.error_code=PCV_GUEST_EXEC_DISABLED`,
`secret_value_observed=false`, `credential_ref_observed=false`를 기록한다.

## 경계

이 evidence는 설치본 current-card smoke다. Host mutation은 이 smoke 자체가 아니라 선행
full admin gate에서 수행됐다. 최신 closed manual-admin package-pair는 아직
`0.42.47-admin-smoke -> 0.42.48-admin-smoke`다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

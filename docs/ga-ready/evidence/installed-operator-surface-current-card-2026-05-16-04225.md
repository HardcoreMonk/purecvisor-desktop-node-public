# Installed Operator Surface Current Card 0.42.25 PASS

```text
evidence_id: installed-operator-surface-current-card-2026-05-16-04225
result: PASS
scope: installed-web-tui-cli-current-card
artifact_root: artifacts/installed-operator-surface-current-card-20260516-04225
summary: artifacts/installed-operator-surface-current-card-20260516-04225/summary.json
version: 0.42.25-admin-smoke
manifest_version: 0.42.25-admin-smoke
service_state: Running
web_console_status_code: 200
pcv_config_status_code: 200
runtime_policy_unauthenticated_status_code: 401
runtime_policy_unauthenticated_error_code: PCV_AUTH_REQUIRED
runtime_policy_boundary_status: expected-auth-boundary
cli_ops_summary_ok: true
tui_operator_smoke: pass
latest_batch_id: full-admin-host-mutation-gate-20260516-04225
latest_batch_status: available
runtime_api_current_evidence_contract: runtime-api-current-evidence-rollup-v1
latest_release_msi_sha256: e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b
latest_release_provenance_commit: 4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_source: DesktopNodeApiRuntimeRoutes
runtime_api_registry_bridge_route_count: 4
manual_admin_package_pair: 0.42.24-admin-smoke -> 0.42.25-admin-smoke
manual_admin_package_pair_status: artifact-discovered
public_boundary_run_id: 25959505688
public_boundary_job_id: 76312299500
public_boundary_head_sha: 4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1
distribution_decision: internal-private-network-only
adr: ADR-0006
token_source: protected-token-file
service_token_storage: windows-credential-manager
token_value_observed: false
password_value_observed: false
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 설치된 `0.42.25-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
current-card anchor를 읽는지 확인한다. 실행 전 installed service `PathName`에
`RepairInstalled -BatchEvidenceRoot`를 절대 `artifacts` root로 적용했고, 이후
operator surface를 캡처했다.

- Web Console은 `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`,
  installed `app.js`의 `Current evidence` / `current_evidence` surface를 확인했다.
- Web API `http://127.0.0.1:7777/api/v1/runtime/policy`는 unauthenticated 요청에
  HTTP `401` / `PCV_AUTH_REQUIRED` boundary를 반환했다.
- CLI는 protected token file로 `ops summary`를 호출했고,
  `runtime-api-current-evidence-rollup-v1`, latest batch
  `full-admin-host-mutation-gate-20260516-04225`, installed runtime
  `0.42.25-admin-smoke`, Runtime/API registry bridge route detail 4개를 확인했다.
- TUI는 installed `pcvtui.exe --smoke-once runtime`으로 runtime surface를 PASS로
  닫았고 token/password value는 artifact에 기록하지 않았다.
- Current-card artifact `summary.json`이 operational Batch Supervisor run으로
  오인되지 않도록 source reader에는 `batch_id`만 있는 capture summary를 latest 후보에서
  제외하는 guard를 추가했다. 설치본 캡처 당시에는 full-gate summary timestamp를
  갱신해 `batch_evidence.status=available`을 확인했다.

이 current-card evidence는 ADR-0006 `internal-private-network-only` 범위의 internal
admin-smoke operator-surface evidence다. Public trusted signing 또는 external stable
publication evidence가 아니며, ADR 변경 없이는 public distribution evidence로 승격하지 않는다.

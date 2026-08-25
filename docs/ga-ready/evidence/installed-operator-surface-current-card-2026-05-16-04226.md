# Installed Operator Surface Current Card 0.42.26 PASS

```text
evidence_id: installed-operator-surface-current-card-2026-05-16-04226
result: PASS
scope: installed-web-tui-cli-current-card
artifact_root: artifacts/installed-operator-surface-current-card-20260516-04226
summary: artifacts/installed-operator-surface-current-card-20260516-04226/summary.json
version: 0.42.26-admin-smoke
manifest_version: 0.42.26-admin-smoke
service_state: Running
web_console_status_code: 200
pcv_config_status_code: 200
runtime_policy_unauthenticated_status_code: 401
runtime_policy_unauthenticated_error_code: PCV_AUTH_REQUIRED
runtime_policy_boundary_status: expected-auth-boundary
cli_ops_summary_ok: true
tui_operator_smoke: pass
latest_batch_id: full-admin-host-mutation-gate-20260516-04226
latest_batch_status: available
runtime_api_current_evidence_contract: runtime-api-current-evidence-rollup-v1
latest_release_msi_sha256: f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7
latest_release_provenance_commit: d6500c01c972cbc7ca1e290e51120181ceea1501
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_route_count: 4
manual_admin_package_pair: 0.42.24-admin-smoke -> 0.42.25-admin-smoke
manual_admin_package_pair_status: artifact-discovered
public_boundary_run_id: 25961834812
public_boundary_job_id: 76318357776
public_boundary_head_sha: d6500c01c972cbc7ca1e290e51120181ceea1501
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

이 evidence는 설치된 `0.42.26-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
current-card anchor를 읽는지 확인한다. 실행 전 installed service `PathName`에
`RepairInstalled -BatchEvidenceRoot`를 절대 `artifacts` root로 적용했고, 이후
operator surface를 캡처했다.

- Web Console은 `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`,
  installed `app.js`의 `Current evidence` / `current_evidence` surface를 확인했다.
- Web API `http://127.0.0.1:7777/api/v1/runtime/policy`는 unauthenticated 요청에
  HTTP `401` / `PCV_AUTH_REQUIRED` boundary를 반환했다.
- CLI는 protected token file로 `ops summary`를 호출했고,
  `runtime-api-current-evidence-rollup-v1`, latest batch
  `full-admin-host-mutation-gate-20260516-04226`, installed runtime
  `0.42.26-admin-smoke`, Runtime/API registry bridge route detail 4개를 확인했다.
- TUI는 installed `pcvtui.exe --smoke-once runtime`으로 runtime surface를 PASS로
  닫았고 token/password value는 artifact에 기록하지 않았다.
- Current-card source reader는 `manual-admin-campaign-descriptor-*` candidate batch를
  operational latest 후보에서 제외하고, 최신 full admin host mutation batch를
  `batch_evidence.latest`로 표시했다.

이 current-card evidence는 ADR-0006 `internal-private-network-only` 범위의 internal
admin-smoke operator-surface evidence다. Public trusted signing 또는 external stable
publication evidence가 아니며, ADR 변경 없이는 public distribution evidence로 승격하지 않는다.

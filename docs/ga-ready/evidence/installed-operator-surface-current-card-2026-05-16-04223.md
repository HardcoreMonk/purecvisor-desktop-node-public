# Installed Operator Surface Current Card 0.42.23 PASS

```text
evidence_id: installed-operator-surface-current-card-2026-05-16-04223
result: PASS
scope: installed-web-tui-cli-current-card
artifact_root: artifacts/installed-operator-surface-current-card-20260516-04223
summary: artifacts/installed-operator-surface-current-card-20260516-04223/summary.json
version: 0.42.23-admin-smoke
manifest_version: 0.42.23-admin-smoke
service_state: Running
web_console_status_code: 200
pcv_config_status_code: 200
runtime_policy_unauthenticated_status_code: 401
runtime_policy_boundary_status: expected-auth-boundary
cli_ops_summary_ok: true
tui_operator_smoke: pass
latest_batch_id: full-admin-host-mutation-gate-20260516-04223
latest_batch_status: available
latest_release_msi_sha256: ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932
closed_package_msi_sha256: 2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406
latest_release_provenance_commit: d11a096086326004f27facd9612c2296ded15a4b
closed_package_provenance_commit: 676b4177b10dc80209969066857bab6008ff2473
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_source: DesktopNodeApiRuntimeRoutes
runtime_api_registry_bridge_route_count: 4
token_source: protected-token-file
service_token_storage: windows-credential-manager
token_value_observed: false
password_value_observed: false
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 설치된 `0.42.23-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
current-card anchor를 읽는지 확인한다. 실행 전 installed service `PathName`에
`RepairInstalled -BatchEvidenceRoot`를 적용해 이번 worktree의 `artifacts` root를
연결했고, 이후 operator surface를 캡처했다.

- Web Console은 `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`을
  반환했다.
- Web API `http://127.0.0.1:7777/api/v1/runtime/policy`는 unauthenticated 요청에
  HTTP `401` / `PCV_AUTH_REQUIRED` boundary를 반환했다.
- CLI는 protected token file로 `ops summary`를 호출했고, latest batch
  `full-admin-host-mutation-gate-20260516-04223`와 Runtime/API registry bridge
  route detail 4개를 확인했다.
- TUI는 installed `pcvtui.exe --smoke-once runtime`으로 `api=reachable`,
  `RUNTIME TABLE` frame, `redaction active` 상태를 확인했다.
- CLI/TUI command line은 protected token file만 참조했고 token-like value는 artifact에
  기록하지 않았다. 설치본 service token storage는 Windows Credential Manager다.

이 current-card evidence는 internal admin-smoke operator-surface evidence다. Public
trusted signing 또는 external stable publication evidence가 아니다.

# Installed Operator Surface Current Card 0.42.22 PASS

```text
evidence_id: installed-operator-surface-current-card-2026-05-16-04222
result: PASS
scope: installed-web-tui-cli-current-card
artifact_root: artifacts/installed-operator-surface-current-card-20260516-04222
summary: artifacts/installed-operator-surface-current-card-20260516-04222/summary.json
version: 0.42.22-admin-smoke
manifest_version: 0.42.22-admin-smoke
service_state: Running
web_console_status_code: 200
pcv_config_status_code: 200
runtime_policy_unauthenticated_status_code: 401
runtime_policy_boundary_status: expected-auth-boundary
cli_ops_summary_ok: true
tui_operator_smoke: pass
latest_batch_id: full-admin-host-mutation-gate-20260516-04222
latest_batch_status: available
latest_release_msi_sha256: 35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c
clean_package_msi_sha256: 68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3
latest_release_provenance_commit: 8a38995cc25a888f64473e9a2869740949ad6b24
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_source: DesktopNodeApiRuntimeRoutes
runtime_api_registry_bridge_route_count: 4
token_source: protected-token-file
token_value_observed: false
password_value_observed: false
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 설치된 `0.42.22-admin-smoke` 기준으로 Web Console, TUI, CLI가 같은
current-card anchor를 읽는지 확인한다. 실행은 read-only operator smoke이며, 사전
보정으로 installed service `PathName`의 `--batch-evidence-root`를 canonical
`artifacts` root로 맞춰 route/OS child evidence가 containment 안에서 읽히도록 했다.

- Web Console은 `http://127.0.0.1/` HTTP `200`, `/pcv-config.js` HTTP `200`을
  반환했다.
- Web API `http://127.0.0.1:7777/api/v1/runtime/policy`는 unauthenticated 요청에
  HTTP `401` / `PCV_AUTH_REQUIRED` boundary를 반환했다.
- CLI는 protected token file로 `ops summary`를 호출했고, latest batch
  `full-admin-host-mutation-gate-20260516-04222`와 Runtime/API registry bridge
  route detail 4개를 확인했다.
- TUI는 installed `pcvtui.exe --smoke-once runtime`으로 `api=reachable`,
  `RUNTIME TABLE` frame을 확인했고 stdout/stderr는 redacted artifact로만 남겼다.

이 current-card evidence는 internal admin-smoke operator-surface evidence다. Public
trusted signing 또는 external stable publication evidence가 아니다.

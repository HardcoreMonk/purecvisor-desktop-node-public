# Installed account/noVNC Operator Surface smoke 2026-05-17 0.42.28

evidence_id: `installed-account-novnc-operator-surface-smoke-2026-05-17-04228`
result: `PASS`
scope: `installed-account-browser-and-target-backed-novnc-smoke`
version: `0.42.28-admin-smoke`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260517-04228`
full_gate_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
clean_package_msi_sha256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
installed_account_login_smoke: `artifacts/installed-account-login-smoke-20260517-04228/summary.json`
installed_browser_live_smoke: `artifacts/installed-account-login-browser-live-smoke-20260517-04228`
target_backed_novnc_smoke: `artifacts/target-backed-novnc-installed-streaming-smoke-20260517-04228/summary.json`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Host Ops Web diagnostics bucket table product payload 변경 이후 설치본
account/RBAC/browser 경로와 target-backed noVNC WebSocket 경로를 다시 확인한 기록이다.
기준 full-gate batch는 `full-admin-host-mutation-gate-20260517-04228`이고 Operator
Surface table contract는 `host-ops-web-diagnostics-bucket-table-v1`이다.

## 확인 결과

| 항목 | 결과 |
| --- | --- |
| installed account login | `pass`, login/session/rbac/console-capabilities HTTP `200` |
| browser live smoke | `pass`, screenshot `8`, diagnostic create/download clicked |
| account/JWT restore | `restore_status=restored`, service restart after restore |
| token/password 노출 | `false` |
| target-backed noVNC | `pass`, `/api/v1/console/novnc/{vm_id}` |
| noVNC frame echo | SHA-256 `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106` matched |
| service PathName restore | `true`, final service `Running` |

이 smoke는 설치본 계정 파일과 service PathName을 일시적으로 변경하므로 host mutation
evidence다. 변경은 summary 기준으로 복구됐으며 public trusted signing 또는 외부 stable
publication evidence가 아니다.

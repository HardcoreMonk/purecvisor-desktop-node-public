# Installed Account/noVNC Operator Surface Smoke 2026-05-28 0.42.57

evidence_id: `installed-account-novnc-operator-surface-smoke-2026-05-28-04257`
scope: `installed-account-login-browser-and-target-backed-novnc-rerun`
status: `pass`
installed_version: `0.42.57-admin-smoke`
installed_account_login_smoke_artifact_root: `artifacts/installed-account-login-smoke-20260528-04257`
target_backed_novnc_artifact_root: `artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04257`
host_mutation_performed: `true-service-config-temporary-restored`
token/password/refresh-token observed: `false/false/false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행 요약

0.42.57 설치본에서 account/RBAC/JWT login, browser QA, target-backed noVNC WebSocket
streaming을 재검증했다. 두 smoke 모두 설치본 service를 대상으로 실행됐고, account/JWT 파일과
service PathName은 실행 후 복구됐다.

| Smoke | 결과 | Artifact |
| --- | --- | --- |
| account login + browser QA | `PASS` | `artifacts/installed-account-login-smoke-20260528-04257/summary.json` |
| target-backed noVNC streaming | `PASS` | `artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04257/summary.json` |

## Account login

| 항목 | 값 |
| --- | --- |
| login/session/rbac/console status | `200/200/200/200` |
| runtime auth mode | `account_rbac_jwt` |
| account file restore | `restored` |
| jwt signing key restore | `restored` |
| service restart after restore | `restarted-after-restore` |
| browser QA status | `pass` |
| browser screenshot count | `8` |
| missing button labels | `0` |
| unlabeled inputs | `0` |
| token observed | `false` |
| password observed | `false` |
| refresh token observed | `false` |

## noVNC streaming

| 항목 | 값 |
| --- | --- |
| websocket path | `/api/v1/console/novnc/{vm_id}` |
| vm id | `pcv-novnc-04257` |
| target host | `127.0.0.1` |
| target frame length | `49` |
| target frame sha256 | `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106` |
| echoed frame sha256 | `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106` |
| service path restored | `true` |
| final service status | `Running` |
| token observed | `false` |
| password observed | `false` |

## 경계

이 evidence는 installed internal admin-smoke 재검증이다. Service PathName과 account/JWT
파일은 smoke 중 임시 변경 후 복구됐다. public trusted signing 또는 외부 stable publication
evidence가 아니다.

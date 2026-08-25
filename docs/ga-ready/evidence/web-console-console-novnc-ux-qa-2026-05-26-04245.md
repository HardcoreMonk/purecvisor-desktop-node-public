# Web Console Console/noVNC UX QA 증거

evidence_id: `web-console-console-novnc-ux-qa-2026-05-26-04245`
result: `PASS`
scope: `phase1-account-novnc-web-console-ux-qa`
version_anchor: `0.42.45-admin-smoke`
phase: `Phase 1 Account/noVNC Operator Surface`
new_host_mutation_performed: `false`
installed_smoke_host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Phase 1 Console Access Card productization 이후 Web Console의
Account/Console card, selected VM console handoff, noVNC path/reason 표시, token/password
redaction을 code-level fixture와 설치본 smoke artifact로 함께 확인한다.

## 확인한 항목

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| Web browser fixture | `PASS` | `Push-Location web; npm run browser:fixture; Pop-Location` |
| selected VM console card | `PASS` | fixture VM `pcv-browser-fixture`, route `/api/v1/vms/pcv-browser-fixture/console` |
| noVNC path/reason | `PASS` | `/api/v1/console/novnc/pcv-browser-fixture`, `noVNC bridge is configured.` |
| open handoff | `PASS` | `Open selected console` button and next action copy rendered |
| installed account login/browser smoke | `PASS` | `artifacts/installed-account-login-smoke-20260526-04245/summary.json` |
| installed browser screenshots | `PASS` | `artifacts/web-console-account-login-browser-smoke-20260526-04245/summary.json`, screenshot count `8` |
| installed accessibility probe | `PASS` | `missing_button_label_count=0`, `unlabeled_input_count=0` |
| target-backed noVNC streaming | `PASS` | `artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json` |
| secret redaction | `PASS` | token/password/refresh-token observed `false` in installed browser/account/noVNC summaries |

## 설치본 smoke 해석

Installed browser smoke는 실제 설치본 listener `http://127.0.0.1/`에서 account login,
Dashboard, Jobs, Network, Troubleshooting, diagnostics create/download, responsive
screenshots를 PASS했다. 해당 host에는 선택 가능한 실제 VM이 없어서
`vm_select_clicked=false`였고, selected VM console/noVNC path는 fixture에서 별도로
검증했다.

Target-backed noVNC streaming smoke는 installed service의
`/api/v1/console/novnc/{vm_id}` WebSocket path가 loopback target으로 frame을 relay하고,
service path를 원복했으며 token/password 값을 노출하지 않았음을 기록한다.

## 경계

이번 QA는 Web UX와 evidence 정합성을 확인한다. noVNC target host/port 설정 mutation,
account/RBAC/JWT schema 변경, service token mutation, Guest Exec, Hyper-V QoS mutation,
Web/TUI direct mutation control은 열지 않는다. Public trusted signing 또는 외부 stable
publication evidence도 주장하지 않는다.

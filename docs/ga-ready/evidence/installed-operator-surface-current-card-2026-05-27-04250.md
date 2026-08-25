# Installed operator surface current-card 2026-05-27 0.42.50

evidence_id: `installed-operator-surface-current-card-2026-05-27-04250`
result: `PASS`
scope: `installed-web-tui-cli-current-card-guest-execution-preview`
version: `0.42.50-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260527-04250`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260527-04250/summary.json`
full_admin_host_mutation_anchor: `full-admin-host-mutation-gate-20260527-04250`
host_mutation_performed: `false-smoke-after-fullgate`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| `pcvcli host status` | exit `0`, `ok=True` |
| `pcvcli --json vm list` | exit `0`, empty inventory allowed |
| `pcvcli --json runtime policy` | `guest_execution.preview_enabled=true`, `execute_enabled=false` |
| `pcvcli --json vm guest-agent-ensure-channel alpha --dry-run` | `guest-channel-preview.v1`, host mutation 없음 |
| `pcvcli --json vm guest-exec alpha --dry-run --credential-ref ...` | `guest-execution-preview.v1`, execution queued 없음, redaction applied |
| Secret echo guard | `super-secret-value`, `PureCVisor/guest/admin` 원문 미노출 |
| `pcvtui --smoke-once runtime --no-color` | exit `0`, API reachable |
| Web root/config | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |

## 경계

이 smoke는 preview contract가 설치본에서 노출됨을 확인한다. Guest execution 실제 실행,
channel verify/repair, Web/TUI direct control은 아직 제품 기능으로 열지 않는다.

# Installed operator surface current-card 2026-05-27 0.42.53

evidence_id: `installed-operator-surface-current-card-2026-05-27-04253`
result: `PASS`
scope: `installed-web-tui-cli-current-card-guest-execution-provider-direct-control`
version: `0.42.53-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260527-04253`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260527-04253/summary.json`
full_admin_host_mutation_anchor: `full-admin-host-mutation-gate-20260527-04253`
host_mutation_performed: `false-smoke-after-fullgate`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| `pcvcli host status` | exit `0`, `ok=True` |
| `pcvcli --json vm list` | exit `0`, empty inventory allowed |
| `pcvcli --json runtime policy` | `guest_execution.enabled=true`, `execute_enabled=true`, channel verify/repair true |
| Runtime dispatch | `vm.guest.exec`, `vm.guest.channel.verify`, `vm.guest.channel.ensure` 포함 |
| `pcvcli --json vm guest-agent-ensure-channel pcv-smoke --dry-run` | `guest-channel-preview.v1`, verify/repair enabled, host mutation 없음 |
| `pcvcli --json vm guest-exec pcv-smoke --dry-run --credential-ref ...` | `guest-execution-preview.v1`, execute enabled, execution queued 없음 |
| Secret echo guard | `PureCVisor/guest/admin` 원문 미노출 |
| `pcvtui --smoke-once runtime --no-color` | exit `0`, API reachable |
| Web root/config | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |

## 경계

이 smoke는 설치본 Web/TUI/CLI current-card와 dry-run preview를 확인한다. 실제 VM 내부
명령 실행은 유효한 Windows guest credential reference가 필요하므로 다음 실제 guest smoke에서
별도 evidence로 닫는다.


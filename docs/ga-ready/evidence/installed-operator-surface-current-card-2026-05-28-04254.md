# Installed operator surface current-card 2026-05-28 0.42.54

evidence_id: `installed-operator-surface-current-card-2026-05-28-04254`
result: `PASS`
scope: `installed-web-tui-cli-current-card-running-guest-cancel`
version: `0.42.54-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260528-04254`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260528-04254/summary.json`
package_artifact_root: `artifacts/admin-smoke-package-20260528-04254`
install_update_artifact_root: `artifacts/installed-update-20260528-04253-04254`
host_mutation_performed: `false-smoke-after-msi-update`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.54-admin-smoke` |
| `pcvcli host status` | exit `0`, `ok=True` |
| `pcvcli --json vm list` | exit `0`, `pcv-guest-installed-04253-r1 guest_family=windows` |
| `pcvcli --json vm get pcv-guest-installed-04253-r1` | `guest_family=windows`, `state=running` |
| Runtime policy | `running_interrupt=true`, `queued_only=false` |
| Runtime dispatch | `vm.guest.exec`, `vm.guest.channel.verify`, `vm.guest.channel.ensure` 포함 |
| `pcvcli --json vm guest-agent-ensure-channel ... --dry-run` | `guest-channel-preview.v1`, verify/repair enabled |
| `pcvcli --json vm guest-exec ... --dry-run --credential-ref ...` | `guest-execution-preview.v1`, execute enabled, execution queued 없음 |
| Running cancel installed smoke | `job-b06eb90e549a481bbf4003399b5604f8`, final `canceled` |
| `pcvtui --smoke-once runtime --no-color` | API reachable |
| Web root/config | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |

## 경계

이 smoke는 설치본 Web/TUI/CLI current-card와 actual running cancel result를 같은 installed
anchor로 묶는다. 최초 smoke 시점에는 0.42.53에서 0.42.54 MSI update path로 올라간 설치본을
검증했고, 이후 `full-admin-host-mutation-gate-20260528-04254`에서 같은 0.42.54 line을
operational fullgate anchor로 승격했다.

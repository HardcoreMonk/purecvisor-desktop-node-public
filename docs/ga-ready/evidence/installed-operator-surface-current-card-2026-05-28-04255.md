# Installed operator surface current-card 2026-05-28 0.42.55

evidence_id: `installed-operator-surface-current-card-2026-05-28-04255`
result: `PASS`
scope: `installed-web-tui-cli-current-card-running-cancel-affordance-and-actual-guest-exec`
version: `0.42.55-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260528-04255`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260528-04255/summary.json`
package_artifact_root: `artifacts/admin-smoke-package-20260528-04255`
fullgate_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04255`
host_mutation_performed: `false-smoke-after-fullgate`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.55-admin-smoke` |
| Clean package MSI | `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627` |
| Operational fullgate MSI | `cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb` |
| Runtime policy | `guest_execution.enabled=true`, `execute_enabled=true`, `running_interrupt=true`, `queued_only=false` |
| Runtime dispatch | `vm.guest.exec`, `vm.guest.channel.verify`, `vm.guest.channel.ensure` 포함 |
| `pcvcli --json vm list` | exit `0`, `pcv-guest-installed-04253-r1 guest_family=windows` |
| `pcvcli --json vm get pcv-guest-installed-04253-r1` | `guest_family=windows`, `state=running` |
| `pcvcli --json vm guest-exec ... --dry-run --credential-ref ...` | `guest-execution-preview.v1`, execute enabled, execution queued 없음 |
| Actual credentialed guest exec | `job-0e05ae5a574d49a5822237337c1e9ad3`, final `succeeded` |
| Channel verify | `job-92e44ca99cde460b9e34567168dbb7cd`, final `succeeded` |
| Web root/config | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| Web running cancel affordance | `Cancel running guest exec`, `data-job-cancel-scope=running-guest-execution` installed |
| `pcvtui --smoke-once runtime --no-color` | `pass-runtime-reachable` |
| `pcvtui --smoke-once job --no-color` | `pass-job-list-reachable` |
| Secret guard | token/password value observed `false` |

## 경계

이 smoke는 Web/TUI running guest execution cancel affordance를 code-level evidence에서 설치본
current-card로 승격한다. 실제 credentialed guest execution은 persistent Windows VHD target을 재사용했고,
새 VM 생성/삭제 mutation은 수행하지 않았다. Public trusted signing과 external stable publication은
계속 주장하지 않는다.

# Installed operator surface current-card 2026-05-28 0.42.56

evidence_id: `installed-operator-surface-current-card-2026-05-28-04256`
result: `PASS`
scope: `installed-web-tui-cli-current-card-manual-admin-next-package-pair`
version: `0.42.56-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260528-04256`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260528-04256/summary.json`
package_artifact_root: `artifacts/admin-smoke-package-20260528-04256`
fullgate_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04256`
manual_admin_campaign_artifact_root: `artifacts/manual-admin-campaign-20260528-04255-04256`
host_mutation_performed: `false-smoke-after-fullgate-and-manual-admin`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.56-admin-smoke` |
| Clean package MSI | `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae` |
| Operational fullgate MSI | `085792312b3bba3ba241882156212b40f936748b08a0ad56ae4a877b24759dec` |
| `pcvcli --json ops summary` | exit `0`, `next_package_pair` present |
| `pcvcli ops summary` | `current.manual_admin_next_package_pair` present |
| `pcvcli --json host status` | exit `0` |
| `pcvcli --json vm list` | exit `0` |
| `pcvtui --smoke-once runtime --no-color` | exit `0`, `MANUAL ADMIN NEXT`, `current-card=ops-summary` |
| `pcvtui --smoke-once job --no-color` | exit `0` |
| Web root/config/app | `/` HTTP `200`, `/pcv-config.js` HTTP `200`, `/app.js` HTTP `200` |
| Web current-card | `Manual admin next`, `Next decision` present |
| Secret guard | token/password value observed `false` |

## 경계

이 smoke는 Runtime/API ops summary의 manual-admin next package-pair projection을 Web/TUI/CLI
설치본 current-card까지 승격했음을 확인한다. Public trusted signing과 external stable
publication은 계속 주장하지 않는다.

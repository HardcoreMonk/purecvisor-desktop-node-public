# Installed operator surface current-card 2026-05-28 0.42.57

evidence_id: `installed-operator-surface-current-card-2026-05-28-04257`
result: `PASS`
scope: `installed-web-tui-cli-current-card-public-boundary-current-evidence`
version: `0.42.57-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260528-04257`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260528-04257/summary.json`
package_artifact_root: `artifacts/admin-smoke-package-20260528-04257`
fullgate_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04257`
manual_admin_campaign_artifact_root: `artifacts/manual-admin-campaign-20260528-04256-04257`
host_mutation_performed: `false-smoke-after-fullgate-and-manual-admin`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 확인 항목

| 항목 | 결과 |
| --- | --- |
| Installed manifest | `0.42.57-admin-smoke` |
| Clean package MSI | `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9` |
| Operational fullgate MSI | `809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e` |
| `pcvcli --json ops summary` | exit `0`, latest batch `full-admin-host-mutation-gate-20260528-04257`, public-boundary run/head present |
| `pcvcli ops summary` | `current.public_boundary_main_push`, `current.public_boundary_head_sha` present |
| `pcvcli --json host status` | exit `0` |
| `pcvcli --json vm list` | exit `0` |
| `pcvtui --smoke-once runtime --no-color` | exit `0`, `PUBLIC BOUNDARY CURRENT`, `current-card=ops-summary` |
| `pcvtui --smoke-once job --no-color` | exit `0` |
| Web root/config/app | `/` HTTP `200`, `/pcv-config.js` HTTP `200`, `/app.js` HTTP `200` |
| Web current-card renderer | `Public boundary head` metric renderer present; runtime data source는 ops summary API |
| Secret guard | token/password value observed `false` |

## 경계

이 smoke는 Runtime/API ops summary의 public-boundary current evidence projection을 Web/TUI/CLI
설치본 current-card까지 승격했음을 확인한다. Public trusted signing과 external stable
publication은 계속 주장하지 않는다.

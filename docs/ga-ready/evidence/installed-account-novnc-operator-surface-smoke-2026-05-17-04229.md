# Installed account/noVNC Operator Surface smoke 2026-05-17 0.42.29

evidence_id: `installed-account-novnc-operator-surface-smoke-2026-05-17-04229`
result: `PASS`
scope: `installed-account-browser-novnc-operator-surface`
version: `0.42.29-admin-smoke`
manual_admin_package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
manual_admin_update_zip_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260517-04229`
full_gate_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
clean_package_msi_sha256: `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77`
provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
installed_account_login_smoke: `artifacts/installed-account-login-smoke-20260517-04229/summary.json`
installed_browser_live_smoke: `artifacts/installed-account-login-browser-live-smoke-20260517-04229`
target_backed_novnc_smoke: `artifacts/target-backed-novnc-installed-streaming-smoke-20260517-04229/summary.json`
novnc_path_template: `/api/v1/console/novnc/{vm_id}`
novnc_vm_name: `pcv-novnc-smoke`
novnc_frame_sha256: `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
host_mutation_performed: `true`
token_value_observed: `false`
password_value_observed: `false`
refresh_token_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.29-admin-smoke` 설치본에서 account/RBAC/browser smoke와 target-backed
noVNC streaming smoke를 재확인한 결과다. 기준 full-gate batch는
`full-admin-host-mutation-gate-20260517-04229`이고 Operator Surface payload 변경 이후
account/noVNC 경로가 계속 동작하는지 확인했다.

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| account login | `pass` | login/session/RBAC/console capabilities `200`, runtime auth mode `account_rbac_jwt` |
| browser live QA | `pass` | screenshot count `8`, diagnostic create/download clicked `true`, missing button labels `0`, unlabeled inputs `0` |
| restore | `pass` | account/token state restored, service restart after restore |
| noVNC target-backed streaming | `pass` | path `/api/v1/console/novnc/{vm_id}`, VM `pcv-novnc-smoke`, frame SHA echoed and matched |
| final service | `pass` | service `Running`, service path restored `true` |

Token, password, refresh token value는 summary와 browser capture에 노출하지 않았다.
이 evidence는 internal admin-smoke 범위다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.

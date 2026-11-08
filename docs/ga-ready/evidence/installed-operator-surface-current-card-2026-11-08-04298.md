# Installed operator surface current-card 2026-11-08 `0.42.98`

evidence_id: `installed-operator-surface-current-card-2026-11-08-04298`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.98-admin-smoke`
installed_manifest_version: `0.42.98-admin-smoke`
installed_product_version: `0.42.98-admin-smoke+2dfe2ac7e1af85a51b676127f83d264a0b57d840`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261108-04298`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261108-04298/summary.json`
summary_sha256: `86a9cf60ce544851e0a44ad7d12ecffb57bb54ff8daa7fb459a4e2ca37325197`
capture_script: `artifacts/installed-operator-surface-current-card-20261108-04298/capture-current-card.ps1`
capture_script_sha256: `0a534c064515bc0a5f3ed802a32c527742097b0827eb99927265e24853d818fe`
fullgate_batch: `full-admin-host-mutation-gate-20261108-04298`
clean_package_msi_sha256: `9a26d4cee9c4360cb8a04ce520b57de4d2e99c3e7b64997d8b4567e7c04733b0`
operational_fullgate_msi_sha256: `0a42b89f4ababb0066e4d2a326a8019ae1a2780ee079b11c4809c999d0d93680`
clean_package_payload_aggregate_sha256: `b8caacbba9855907c656ccd761a01052783d5a79f1518d8290e3672688eeee5c`
operational_fullgate_payload_aggregate_sha256: `963f4f746b0fc9d1c80209861bc2a6317ff08cb5028b344acd589685bbb67444`
provenance_commit: `2dfe2ac7e1af85a51b676127f83d264a0b57d840`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.98`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.96-admin-smoke -> 0.42.98-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261108-04296-04298-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status`, `runtime policy`, `network inventory` | JSON ok, stderr empty, exit `0` (`3/3`) | `PASS` |
| Web `/`, `/pcv-config.js` | HTTP `200` (`2/2`), token-free | `PASS` |
| service | `Running/Automatic/LocalSystem`, credential manager target, raw/protected token flag 없음 | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

캡처 시각은 `2026-11-08T07:11:09.9864029Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261108-04298`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`501b41f7…`/`bfab42fd…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

캡처 결과는 `installed_non_promoted_candidate`였다. 2026-11-08 Lane 3가 operational current를 `0.42.98-admin-smoke`로 썼다.
- public trusted signing과 external stable publication을 주장하지 않는다.

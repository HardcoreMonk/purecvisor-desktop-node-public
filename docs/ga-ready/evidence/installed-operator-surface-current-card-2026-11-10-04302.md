# Installed operator surface current-card 2026-11-10 `0.43.2`

evidence_id: `installed-operator-surface-current-card-2026-11-10-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.2-admin-smoke`
installed_manifest_version: `0.43.2-admin-smoke`
installed_product_version: `0.43.2-admin-smoke+2f19778abb4b948bdaa9f8a4b196166cad90c653`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261110-04302`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261110-04302/summary.json`
summary_sha256: `09d4dafcc04db821eeb4faba38763fa18e68faf1d377394762cbc30b18e46bb4`
capture_script: `artifacts/installed-operator-surface-current-card-20261110-04302/capture-current-card.ps1`
capture_script_sha256: `fcd50e16f7aae65d8fe31d955c223e59f17e5ae6d7d1a1d7eac86548b53fd00b`
fullgate_batch: `full-admin-host-mutation-gate-20261110-04302`
clean_package_msi_sha256: `fa6afbce618568a95bb941c34ded38c188d7a8c221bfc26810d3ac14111b1c0c`
operational_fullgate_msi_sha256: `82e78f2b5247847d72e7cc4b95d04736a842f987bb4ef24c25a67561517228d5`
clean_package_payload_aggregate_sha256: `35ff7e2ea42152980b60adf8b63b8cade7cdff1aa5100820fe0c8b5e02d2d226`
operational_fullgate_payload_aggregate_sha256: `71b94bc9852b059c98c5a11f361dcbe9ee7bdd88448cd5c272d4b2d458e7b162`
provenance_commit: `2f19778abb4b948bdaa9f8a4b196166cad90c653`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.2`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.1-admin-smoke -> 0.43.2-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261110-04301-04302-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.2-admin-smoke`
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

캡처 시각은 `2026-11-10T11:52:42.2440027Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261110-04302`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`6f61420e…`/`8555fb9a…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

캡처 결과는 `installed_non_promoted_candidate`였다. 2026-11-10 Lane 3가 operational current를 `0.43.2-admin-smoke`로 썼다.
- public trusted signing과 external stable publication을 주장하지 않는다.

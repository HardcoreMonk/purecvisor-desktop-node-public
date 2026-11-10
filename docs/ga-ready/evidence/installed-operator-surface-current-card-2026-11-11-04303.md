# Installed operator surface current-card 2026-11-11 `0.43.3`

evidence_id: `installed-operator-surface-current-card-2026-11-11-04303`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.3-admin-smoke`
installed_manifest_version: `0.43.3-admin-smoke`
installed_product_version: `0.43.3-admin-smoke+db1a7626b7654264d8fa5059685027d064098445`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261111-04303`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261111-04303/summary.json`
summary_sha256: `17e50ad3f1165cdc820938ba422b6e0a19820332d3623f8201c221a42af7eba9`
capture_script: `artifacts/installed-operator-surface-current-card-20261111-04303/capture-current-card.ps1`
capture_script_sha256: `b2b0bb9134e1f76451449bdad0c00c7975a9350b58e85e1d58decef2415f1d9e`
fullgate_batch: `full-admin-host-mutation-gate-20261111-04303`
clean_package_msi_sha256: `66546521fb0fea092de8d0fd390153cc337108d9c17d5622015b748d901de060`
operational_fullgate_msi_sha256: `13f716ee18fbbcee0e0322de6cd12895762fb665863a1e7b437ff4bd292d0b59`
clean_package_payload_aggregate_sha256: `ef0c85dba321261bb6900caedc49fa95b1f58f488cd4aae3740115c5d159cd09`
operational_fullgate_payload_aggregate_sha256: `0ea79bc712613d9fd5ce52e0571ef23cd721c8e821fdd9882598858ba3f1f018`
provenance_commit: `db1a7626b7654264d8fa5059685027d064098445`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.3`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.2-admin-smoke -> 0.43.3-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261111-04302-04303-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.3-admin-smoke`
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

캡처 시각은 `2026-11-10T15:54:53.2045679Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261111-04303`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`6135e9a0…`/`e590f87f…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

캡처 결과는 `installed_non_promoted_candidate`였다. 2026-11-11 Lane 3가 operational current를 `0.43.3-admin-smoke`로 썼다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Installed operator surface current-card 2026-10-11 `0.42.96`

evidence_id: `installed-operator-surface-current-card-2026-10-11-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.96-admin-smoke`
installed_manifest_version: `0.42.96-admin-smoke`
installed_product_version: `0.42.96-admin-smoke+9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261011-04296`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261011-04296/summary.json`
summary_sha256: `1adee05d40d6ab0c17e937d2c20e137d2b8bb556b87c222520999a08e08803c8`
capture_script: `artifacts/installed-operator-surface-current-card-20261011-04296/capture-current-card.ps1`
capture_script_sha256: `0dc50f648b4ee3af6654370a5878539556094aaf62eb251bd75bdc65c34ed3f2`
fullgate_batch: `full-admin-host-mutation-gate-20261011-04296`
clean_package_msi_sha256: `326b867a161ffa5038f4b3cecd8405ca0999cff00875c4a9fb8f5ac92f3cdeed`
operational_fullgate_msi_sha256: `37bfc1ffd045950e3c4bfa1fefd4db6a96a0490288dc647fecc6cb1e1137d299`
clean_package_payload_aggregate_sha256: `b76c53108e93b37f6d8ca9236f51a1aee5ea252f67c9ffc1ca68883983449104`
operational_fullgate_payload_aggregate_sha256: `8dc6df5754616748d83b5728cd5f38b0a5788c47e44d1d8f46e31fca4dfe4cd5`
provenance_commit: `9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.96`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.95-admin-smoke -> 0.42.96-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261011-04295-04296-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status`, `runtime policy`, `network inventory` | JSON ok, stderr empty, exit `0` (`3/3`) | `PASS` |
| Web `/`, `/pcv-config.js` | HTTP `200` (`2/2`), token-free | `PASS` |
| service | `Running/Automatic/LocalSystem`, credential manager target, raw/protected token flag 없음 | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

캡처 시각은 `2026-10-10T17:04:14.3244922Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261011-04296`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`b52629d9…`/`7ad43a47…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.93-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

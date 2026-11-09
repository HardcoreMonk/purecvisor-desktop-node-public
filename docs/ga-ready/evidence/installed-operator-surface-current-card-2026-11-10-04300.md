# Installed operator surface current-card 2026-11-10 `0.43.0`

evidence_id: `installed-operator-surface-current-card-2026-11-10-04300`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.0-admin-smoke`
installed_manifest_version: `0.43.0-admin-smoke`
installed_product_version: `0.43.0-admin-smoke+1f646ee010daa175cbe82ff70ad79def5335ba15`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261110-04300`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261110-04300/summary.json`
summary_sha256: `d8c6fe8b8c0c68c1624f48daa23814bff61ba045f523edd0ad3f0ee139ddc4d3`
capture_script: `artifacts/installed-operator-surface-current-card-20261110-04300/capture-current-card.ps1`
capture_script_sha256: `0f7334d7cacc9e64a19f0f1baab0df901650c95e8d81fd9394cb0811dd1afd30`
fullgate_batch: `full-admin-host-mutation-gate-20261110-04300`
clean_package_msi_sha256: `1eb5d98424ba72c6cb42b7ef5d9c7bbe3778c29ab49a0ae28494bb0b9e1997ec`
operational_fullgate_msi_sha256: `32d7ec56c3bce2ef78fa28e8db775b8ee0a83458d3ade91037dbf9c0060f6bd8`
clean_package_payload_aggregate_sha256: `5a1c8d806932f4abbf90bf6e588e3e2e71bce33b65b496f595d757ccf9873202`
operational_fullgate_payload_aggregate_sha256: `0bb34c5c907d6c677aaa909d0742e283f58a3b9f1a3a2a81f6b0afe602fcb438`
provenance_commit: `1f646ee010daa175cbe82ff70ad79def5335ba15`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.0`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.99-admin-smoke -> 0.43.0-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261110-04299-04300-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.99-admin-smoke`
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

캡처 시각은 `2026-11-09T22:14:20.5612851Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261110-04300`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`5f0b34a3…`/`9b8d20cf…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.99-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

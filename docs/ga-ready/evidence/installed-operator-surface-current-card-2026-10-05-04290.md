# Installed operator surface current-card 2026-10-05 `0.42.90`

evidence_id: `installed-operator-surface-current-card-2026-10-05-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.90-admin-smoke`
installed_manifest_version: `0.42.90-admin-smoke`
installed_product_version: `0.42.90-admin-smoke+648139df9f03d37b3e3e036e995f701c5d7c32a3`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261005-04290`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261005-04290/summary.json`
summary_sha256: `24618dd593cd0a4ee17bf81580b978150954a43b56c725a7f533b5191b51ffa1`
capture_script: `artifacts/installed-operator-surface-current-card-20261005-04290/capture-current-card.ps1`
capture_script_sha256: `4910dec181fe658f78f5004ae863ecec6cd219a1abe057259422e406a996957a`
fullgate_batch: `full-admin-host-mutation-gate-20261005-04290`
clean_package_msi_sha256: `54277baafea5be820572c082a874b75c23f55ca3fab0c374cd2b6ca357012146`
operational_fullgate_msi_sha256: `ac367ea4c244aa574967963a822fe8c38e35407b371c7d6ae1caf299a690d153`
clean_package_payload_aggregate_sha256: `e6cec1d059367fc0003f74c417230d24e4d49d35c99dda428c3f40c9ca085fed`
operational_fullgate_payload_aggregate_sha256: `9031c5605255521f7600f06e0fecbfb57876e7b33a8b5f17139138b6115af58c`
provenance_commit: `648139df9f03d37b3e3e036e995f701c5d7c32a3`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.90`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.89-admin-smoke -> 0.42.90-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261005-04289-04290-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.89-admin-smoke`
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

캡처 시각은 `2026-10-05T12:28:26.4710589Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261005-04290`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`b4884616…`/`39a1d1d4…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

3b-r2 current-card 스크립트에서 root, evidence id, fullgate batch, 기대 SHA 상수, `provenance_commit`, 설치 manifest version을 바꿔 실행했다. 첫 캡처는 `provenance_commit` 상수를 바꾸지 않아 버리고 다시 캡처했다. 이 스크립트는 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.89-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

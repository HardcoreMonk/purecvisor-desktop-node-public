# Installed operator surface current-card 2026-10-06 `0.42.91`

evidence_id: `installed-operator-surface-current-card-2026-10-06-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.91-admin-smoke`
installed_manifest_version: `0.42.91-admin-smoke`
installed_product_version: `0.42.91-admin-smoke+990a4b2713f6d51dca416b476308a0bf92296155`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261006-04291`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261006-04291/summary.json`
summary_sha256: `0b49c3966e05e258a69056314baedcda8766680814db24b6701459ecb8266334`
capture_script: `artifacts/installed-operator-surface-current-card-20261006-04291/capture-current-card.ps1`
capture_script_sha256: `09d1740dcc29d20147b101f35c05049e2f6ef68488f85caf0018681f088a2918`
fullgate_batch: `full-admin-host-mutation-gate-20261006-04291`
clean_package_msi_sha256: `bdef7609de3667298325d162641578a85e191ed31075cc6238e8e0f79fbfc12f`
operational_fullgate_msi_sha256: `46dddccb669f75cf761ae2b52d9b2139c9df3f7f48e1c64a20f5027293cbb85b`
clean_package_payload_aggregate_sha256: `f4503dfcbde2b708c7cd0bc25ad89b69ab454935be3f92ac327a159c2f5c5cac`
operational_fullgate_payload_aggregate_sha256: `c79dff87d7ec3f44e3b701d9918745cccdd74ae9d7c550ea829512e76a4003c0`
provenance_commit: `990a4b2713f6d51dca416b476308a0bf92296155`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.91`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.90-admin-smoke -> 0.42.91-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261006-04290-04291-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.91-admin-smoke`
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

캡처 시각은 `2026-10-06T11:01:36.9737155Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261006-04291`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`a935701e…`/`2c249f0d…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

0.42.90 current-card 스크립트에서 `DEVELOPMENT_PROCEDURE.md` §10 목록대로 root, evidence id, fullgate batch, 기대 SHA 상수, `provenance_commit`, 설치 manifest version, `canonical_current_evidence`를 바꿔 root 밖에서 실행했다. 이 스크립트는 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.90-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

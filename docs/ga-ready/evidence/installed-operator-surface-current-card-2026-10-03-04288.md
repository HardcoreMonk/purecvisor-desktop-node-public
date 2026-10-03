# Installed operator surface current-card 2026-10-03 `0.42.88`

evidence_id: `installed-operator-surface-current-card-2026-10-03-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.88-admin-smoke`
installed_manifest_version: `0.42.88-admin-smoke`
installed_product_version: `0.42.88-admin-smoke+47ff198de86d77d25aa90fa095a294254c7ffef6`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261003-04288`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261003-04288/summary.json`
summary_sha256: `f910b7524249b2ed2c8ebf75d3ce05b8837e52de9b22c4e125349b4882ffbd1f`
capture_script: `artifacts/installed-operator-surface-current-card-20261003-04288/capture-current-card.ps1`
capture_script_sha256: `244510a9bd83e5d045b9d50f6d11adaa99a525ebaf2d828914a007058c5faa67`
fullgate_batch: `full-admin-host-mutation-gate-20261003-04288`
clean_package_msi_sha256: `81ef85273cb9bbf9d813d4c3cce40f88c22e5d596c40906dab8766e4cab79b64`
operational_fullgate_msi_sha256: `32b35113e00ffbe59cd503d1b83ae5b3b62028bd70a5fcddd2100de6e03cd2c6`
clean_package_payload_aggregate_sha256: `4377cc2519ab7e014c049697ecd5414891bc71a278b748bd670a8a3f0f870af6`
operational_fullgate_payload_aggregate_sha256: `163ece95759af368a5fdccf957d3791af3d6301224be434169c347719e1cd9af`
provenance_commit: `47ff198de86d77d25aa90fa095a294254c7ffef6`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.88`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.87-admin-smoke -> 0.42.88-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20261003-04287-04288`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.87-admin-smoke`
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

캡처 시각은 `2026-10-03T12:01:06.7407363Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261003-04288`가 마지막에 다시 설치한 operational MSI이고 ARP product code는 `{3F60088B-3F8D-483F-8B98-0B56DE01A012}` 하나다. 설치본 Host/CLI SHA-256(`1df96c7f…`/`763db7e2…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 테스트 VM은 없고 보존 VM 하나만 남았다. secret 형태 문자열은 관측되지 않았다.

0.42.87 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수 네 개, `canonical_current_evidence`를 바꿔 artifact root 밖에서 실행했고, 재현용으로 root에 복사했다. 이 스크립트는 읽기만 한다.

## Nonclaims

- 결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.87-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

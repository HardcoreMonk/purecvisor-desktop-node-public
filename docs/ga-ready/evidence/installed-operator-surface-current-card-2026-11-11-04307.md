# Installed operator surface current-card 2026-11-11 `0.43.7`

evidence_id: `installed-operator-surface-current-card-2026-11-11-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.7-admin-smoke`
installed_manifest_version: `0.43.7-admin-smoke`
installed_product_version: `0.43.7-admin-smoke+38981398807685e96fcdb8777527573fa97542cc`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261111-04307`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261111-04307/summary.json`
summary_sha256: `afc97278eec4678b12c5f2f0a7c698547628405bcecdf6068a5b114b61f78805`
capture_script: `artifacts/installed-operator-surface-current-card-20261111-04307/capture-current-card.ps1`
capture_script_sha256: `3ecfd687c662481be162265c3c23778980ad1ce11447a59594ad6a349e711829`
fullgate_batch: `full-admin-host-mutation-gate-20261111-04307`
clean_package_msi_sha256: `a808cda4d54eae4caf012a8d7b012663b6caa46f683741599150dae7e0223a31`
operational_fullgate_msi_sha256: `e6a893674bd50a12b2a619fab7e50bcb3fc29ab7b000dc6eb845cca59fb766ea`
clean_package_payload_aggregate_sha256: `33e39bd2191fc825d62815f296b9e11b4a2b2f052125665e77d427d504a4f7ef`
operational_fullgate_payload_aggregate_sha256: `a2dba459e72f1bc2263707c3855ad5c1b6ab5e7c0d4f67db9780224c2586cbf2`
provenance_commit: `38981398807685e96fcdb8777527573fa97542cc`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.7`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.6-admin-smoke -> 0.43.7-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261111-04306-04307-r2-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.7-admin-smoke`
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

캡처 시각은 `2026-11-11T13:32:01.3253760Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261111-04307`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`a9a72baa…`/`f6e851fc…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 이번 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

캡처 결과는 `installed_non_promoted_candidate`였다. 2026-11-12 Lane 3가 operational current를 `0.43.7-admin-smoke`로 썼다.
- public trusted signing과 external stable publication을 주장하지 않는다.

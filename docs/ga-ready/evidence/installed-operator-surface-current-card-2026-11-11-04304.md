# Installed operator surface current-card 2026-11-11 `0.43.4`

evidence_id: `installed-operator-surface-current-card-2026-11-11-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.4-admin-smoke`
installed_manifest_version: `0.43.4-admin-smoke`
installed_product_version: `0.43.4-admin-smoke+7b929ecbb618f483a0aa918643ef66c117aedcc5`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261111-04304`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261111-04304/summary.json`
summary_sha256: `bbbe6d8795353c384cfec466b96d688095b2166adb402780e9475f24596540d0`
capture_script: `artifacts/installed-operator-surface-current-card-20261111-04304/capture-current-card.ps1`
capture_script_sha256: `e1fb30025a2e9dbcc464dc37b54c22dbaadfd71fac963a9a8c82149800fcd83a`
fullgate_batch: `full-admin-host-mutation-gate-20261111-04304`
clean_package_msi_sha256: `c24ef8317c838d659834b7e77446625d9377e5774f9057fedc813318a739fccb`
operational_fullgate_msi_sha256: `c9f87149464776ac04cd53c0c2b82979f712ab093f907fad858d40a5986093d5`
clean_package_payload_aggregate_sha256: `29ba7ece25684f58b11c2e1ba4122569047494a814ef1a4ab49dee7c8959fb3d`
operational_fullgate_payload_aggregate_sha256: `307491b625c7ac6ec8537912ce1074cb7064aeeb163205b162a1c8769c1e9f02`
provenance_commit: `7b929ecbb618f483a0aa918643ef66c117aedcc5`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.4`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.3-admin-smoke -> 0.43.4-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261111-04303-04304-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.4-admin-smoke`
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

캡처 시각은 `2026-11-10T18:50:24.6130472Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261111-04304`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`f73f7e6a…`/`52c8a28f…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

캡처 결과는 `installed_non_promoted_candidate`였다. 2026-11-11 Lane 3가 operational current를 `0.43.4-admin-smoke`로 썼다.
- public trusted signing과 external stable publication을 주장하지 않는다.

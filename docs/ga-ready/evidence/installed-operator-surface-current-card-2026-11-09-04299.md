# Installed operator surface current-card 2026-11-09 `0.42.99`

evidence_id: `installed-operator-surface-current-card-2026-11-09-04299`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.99-admin-smoke`
installed_manifest_version: `0.42.99-admin-smoke`
installed_product_version: `0.42.99-admin-smoke+a95987b094f9865c1f344043afba48b810ecc8f5`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261109-04299`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261109-04299/summary.json`
summary_sha256: `0620149871e072955e4be6bcdd517b09e8f012d7c9dd52cf1086f0d019b9805f`
capture_script: `artifacts/installed-operator-surface-current-card-20261109-04299/capture-current-card.ps1`
capture_script_sha256: `aa0f343d2af7f5b4b7a37108d646503b37d43d6dc26f6124c542922829b1d412`
fullgate_batch: `full-admin-host-mutation-gate-20261109-04299`
clean_package_msi_sha256: `7946f464245edf9f7ab84685add805fa7656274a8511525fade9a2c0c6842af0`
operational_fullgate_msi_sha256: `7c78f89eb53016870d8bf9841e5f7d67a6740b113f880285aa52a59c33dca650`
clean_package_payload_aggregate_sha256: `e03dcb32faa37db01b8c4900151e0b51b7859fcbb3bb31e1f3f6e69099ea9c57`
operational_fullgate_payload_aggregate_sha256: `7c91f091746a3467f34ef0ba8af5a2ebf1667ce40ef54eddafcbfb85f3ed7831`
provenance_commit: `a95987b094f9865c1f344043afba48b810ecc8f5`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.99`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.98-admin-smoke -> 0.42.99-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261109-04298-04299-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.98-admin-smoke`
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

캡처 시각은 `2026-11-08T19:45:20.7934676Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261109-04299`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`a98ad16b…`/`c7531573…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.98-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Installed operator surface current-card 2026-10-10 `0.42.95`

evidence_id: `installed-operator-surface-current-card-2026-10-10-04295`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.95-admin-smoke`
installed_manifest_version: `0.42.95-admin-smoke`
installed_product_version: `0.42.95-admin-smoke+b9898cf7a125e4dadbc4b29ae235f46d8c914a01`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261010-04295`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261010-04295/summary.json`
summary_sha256: `207d679f9f707c565210997f2a0f123ce519e62975d25b5222e1131ac2298d17`
capture_script: `artifacts/installed-operator-surface-current-card-20261010-04295/capture-current-card.ps1`
capture_script_sha256: `dbe3b5b949f821ce06011abb4a5e55f11a2af518eb805ed168d32f6244932dcc`
fullgate_batch: `full-admin-host-mutation-gate-20261010-04295`
clean_package_msi_sha256: `50680d3c757596e8fce85b3cbb41a7228e8a1ced244678d620fe912f371fab24`
operational_fullgate_msi_sha256: `c1acc538b988e6a7be73722340330e2faca8ecda46b2c234d888a661bd467b52`
clean_package_payload_aggregate_sha256: `53a58bded4c5eec6f6e430fd4dfd8b69f904b4773c7f45841e224cc3c830381f`
operational_fullgate_payload_aggregate_sha256: `132cb887cb9ca98187d8a75b652424a32af072b9cf8c2b43ad6d9db8e9522f4b`
provenance_commit: `b9898cf7a125e4dadbc4b29ae235f46d8c914a01`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.95`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.94-admin-smoke -> 0.42.95-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261010-04294-04295-closed`
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

캡처 시각은 `2026-10-10T14:32:20.0086525Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261010-04295`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`a397b758…`/`fb5392ec…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.93-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

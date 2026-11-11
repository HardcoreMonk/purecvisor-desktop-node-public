# Installed operator surface current-card 2026-11-11 `0.43.6`

evidence_id: `installed-operator-surface-current-card-2026-11-11-04306`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.6-admin-smoke`
installed_manifest_version: `0.43.6-admin-smoke`
installed_product_version: `0.43.6-admin-smoke+bfb30c11d747a89f9d41f4a963d52a279f5ba231`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261111-04306`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261111-04306/summary.json`
summary_sha256: `cbc8920cd8e98db696a081200fe9d04c7aee8be385a83b0ec375390f6d970012`
capture_script: `artifacts/installed-operator-surface-current-card-20261111-04306/capture-current-card.ps1`
capture_script_sha256: `dc9133a49fe1767166f6f01fa9b9db9e8a482d5434e314479d0f9ed040eeeeb6`
fullgate_batch: `full-admin-host-mutation-gate-20261111-04306`
clean_package_msi_sha256: `59106d65ad70f57ab4cb3d9bd5b5f7cf6dde9f333af76d66ce44d9ba00dd98bd`
operational_fullgate_msi_sha256: `8c87b46a3127f60c126af91f7ab4d0425c1a7fc2f233e9917280d427be5415ab`
clean_package_payload_aggregate_sha256: `7960b364a05ba83c92ab206a78858fb321adb03a785c62807e7301d9d5089119`
operational_fullgate_payload_aggregate_sha256: `070b3a12a2c528d9a051eda9f1ec83373d093030ec88ca8067846146bd796662`
provenance_commit: `bfb30c11d747a89f9d41f4a963d52a279f5ba231`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.6`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.5-admin-smoke -> 0.43.6-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261111-04305-04306-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.4-admin-smoke`
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

캡처 시각은 `2026-11-11T10:15:47.8453207Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261111-04306`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`111fb379…`/`a21f826c…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

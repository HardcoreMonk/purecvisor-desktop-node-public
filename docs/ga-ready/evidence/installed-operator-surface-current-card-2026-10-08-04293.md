# Installed operator surface current-card 2026-10-08 `0.42.93`

evidence_id: `installed-operator-surface-current-card-2026-10-08-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.93-admin-smoke`
installed_manifest_version: `0.42.93-admin-smoke`
installed_product_version: `0.42.93-admin-smoke+818d00f113b3d3eaf1573652cd4c2d86a551834d`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261008-04293`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261008-04293/summary.json`
summary_sha256: `6ff8617b7ae0f190f152c19b08e84759ae426a75a6b3039e9cd450201ae8754e`
capture_script: `artifacts/installed-operator-surface-current-card-20261008-04293/capture-current-card.ps1`
capture_script_sha256: `730b2e5c0c29556fc0325a17a02d0eba760d8c86a4c76e4769f2ce07a572261f`
fullgate_batch: `full-admin-host-mutation-gate-20261008-04293`
clean_package_msi_sha256: `13d7f0d476828f865b0d4aca7331a2dcd1a10a8dbe157b9d6a93dc2217f9fb49`
operational_fullgate_msi_sha256: `5d5a7c7c6086c591bae0b2609c8e44f5ae94e84e3fd70584f7b3f994456864dd`
clean_package_payload_aggregate_sha256: `2a071cd2c6298e289b940a37ef124a1c3d3502fd9a476033174251884a0f2cf7`
operational_fullgate_payload_aggregate_sha256: `df1a8cacade8db1ccf644a4a9a83852f2cc731c3d50231389758f6b78bf0d29e`
provenance_commit: `818d00f113b3d3eaf1573652cd4c2d86a551834d`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.93`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.92-admin-smoke -> 0.42.93-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261008-04292-04293-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.92-admin-smoke`
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

캡처 시각은 `2026-10-08T06:28:56.0543928Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261008-04293`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`9f537217…`/`df02f453…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. root 밖에서 실행했고 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.92-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

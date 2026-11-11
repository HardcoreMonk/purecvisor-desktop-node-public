# Installed operator surface current-card 2026-11-11 `0.43.5`

evidence_id: `installed-operator-surface-current-card-2026-11-11-04305`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.5-admin-smoke`
installed_manifest_version: `0.43.5-admin-smoke`
installed_product_version: `0.43.5-admin-smoke+8173777e208d43fd1097af53f26b2213fdd98b07`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261111-04305`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261111-04305/summary.json`
summary_sha256: `d5e0794743aaa955361532ac405ea5457df4032da7eceddaa20af1c099024925`
capture_script: `artifacts/installed-operator-surface-current-card-20261111-04305/capture-current-card.ps1`
capture_script_sha256: `ddae5ec621e21b571f88d7b289c63df7faaf09e7e928cb5681dca7af0c2d1678`
fullgate_batch: `full-admin-host-mutation-gate-20261111-04305`
clean_package_msi_sha256: `5f4699097d267ef3bddda081d0351f5f14ae286803d1447ce1eeeba5ef727d8d`
operational_fullgate_msi_sha256: `dd77d8713337b791e763a7bfb48be085f6576cbe642e3233a63c99d34ce38c11`
clean_package_payload_aggregate_sha256: `b5151f72c9ebfa5c7cb462627ec4bb3964dac79bad8bfe34c0647c2a565b1f8d`
operational_fullgate_payload_aggregate_sha256: `e45b2e74954af951cabfaa30147d75f620819cc137da64cb63476beb0e686da3`
provenance_commit: `8173777e208d43fd1097af53f26b2213fdd98b07`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.5`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.4-admin-smoke -> 0.43.5-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261111-04304-04305-closed`
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

캡처 시각은 `2026-11-11T07:06:39.3908363Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261111-04305`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`705b2e9e…`/`b6fccfc8…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

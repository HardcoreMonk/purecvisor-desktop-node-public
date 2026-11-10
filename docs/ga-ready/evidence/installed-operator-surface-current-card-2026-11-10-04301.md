# Installed operator surface current-card 2026-11-10 `0.43.1`

evidence_id: `installed-operator-surface-current-card-2026-11-10-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.1-admin-smoke`
installed_manifest_version: `0.43.1-admin-smoke`
installed_product_version: `0.43.1-admin-smoke+143a55b57b42fd059908776bad285783518a1887`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261110-04301`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261110-04301/summary.json`
summary_sha256: `b55202c60a28faa83657a7198c7bebab4560dfafe21d60b06cb419eaa74e6b44`
capture_script: `artifacts/installed-operator-surface-current-card-20261110-04301/capture-current-card.ps1`
capture_script_sha256: `37361625a1cb6c6d84b42acd4f42fde6e11359a152ac6a99c215ada5d8321926`
fullgate_batch: `full-admin-host-mutation-gate-20261110-04301`
clean_package_msi_sha256: `37c3163fa024703c3e285d6c8f2343422437b9db933e78a08df4acce88b120f1`
operational_fullgate_msi_sha256: `f4f3f30d8761a0e855c8496f521f188e6954e11790dc4f48b8f6a651d07a333a`
clean_package_payload_aggregate_sha256: `97b42b7495666c0674486980e5c097c53f2cd49d238737c83f87f1d2ec4d79e4`
operational_fullgate_payload_aggregate_sha256: `4c4506982afdef420d02a01b0cfa02b375e1fb0588eb098a7c4b00d95b374c3c`
provenance_commit: `143a55b57b42fd059908776bad285783518a1887`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.43.1`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.43.0-admin-smoke -> 0.43.1-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261110-04300-04301-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.43.0-admin-smoke`
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

캡처 시각은 `2026-11-10T08:02:54.4125869Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261110-04301`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`31873211…`/`113f23a8…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 렌더된 fullgate facts에서 만들었다. 저장소 root(card root 밖)에서 실행했고, 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.43.0-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

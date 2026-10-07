# Installed operator surface current-card 2026-10-07 `0.42.92`

evidence_id: `installed-operator-surface-current-card-2026-10-07-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.92-admin-smoke`
installed_manifest_version: `0.42.92-admin-smoke`
installed_product_version: `0.42.92-admin-smoke+b51b8cf804288121abcd8ea3724714cd7f9fc9f6`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261007-04292`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261007-04292/summary.json`
summary_sha256: `213591080df8b706b7fdccfb7bdd20ff84034630e298209d4cd88ac686c3b8c6`
capture_script: `artifacts/installed-operator-surface-current-card-20261007-04292/capture-current-card.ps1`
capture_script_sha256: `c1285063ac7ec3796ae24e0dc130fab05337db96a4caa3ed776d9a688b857d0d`
fullgate_batch: `full-admin-host-mutation-gate-20261007-04292`
clean_package_msi_sha256: `dc79fdd166f0882a31e726e43909e0ab5b3dda09142de162c72dfa9821b104ec`
operational_fullgate_msi_sha256: `67b257a3bce6b31936458aed5cc5cf99926731ca8320e5bbdfaeee3a88685080`
clean_package_payload_aggregate_sha256: `efbbbb16756d88a4c5e3d4b5ca746ff03db94b6fd41c99f91734741479153cff`
operational_fullgate_payload_aggregate_sha256: `dda8eb7f4bb535aca7aa8a8b411c6f5cf00311972d7cc388d1cbb1000a3c44d1`
provenance_commit: `b51b8cf804288121abcd8ea3724714cd7f9fc9f6`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.92`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.91-admin-smoke -> 0.42.92-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-20261007-04291-04292-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.92-admin-smoke`
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

캡처 시각은 `2026-10-07T15:07:41.0686745Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261007-04292`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`494d567b…`/`36d93aae…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `0`개다. secret 형태 문자열은 관측되지 않았다.

capture 스크립트는 `pcvverify train-host-inputs --kind current-card`가 train facts에서 렌더했다. fullgate 문서가 current-card summary를 요구해 순환하므로, 스크립트가 읽는 fullgate 값 다섯 개를 fullgate artifact에서 계산해 facts에 잠시 넣고 렌더한 뒤 되돌렸다(나중에 렌더한 fullgate 값과 같음, backlog `BL-0004`). root 밖에서 실행했고 읽기만 한다.

## Nonclaims

결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.91-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

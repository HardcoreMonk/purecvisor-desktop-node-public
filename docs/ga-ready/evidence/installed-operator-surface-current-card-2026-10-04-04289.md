# Installed operator surface current-card 2026-10-04 `0.42.89`

evidence_id: `installed-operator-surface-current-card-2026-10-04-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.89-admin-smoke`
installed_manifest_version: `0.42.89-admin-smoke`
installed_product_version: `0.42.89-admin-smoke+a780928ee41f6064cbeebec754eca647dfff42f1`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261004-04289`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261004-04289/summary.json`
summary_sha256: `a3d66257351aa7669d585f34a32935b72bfc38caf423058d7b8700021313c29a`
capture_script: `artifacts/installed-operator-surface-current-card-20261004-04289/capture-current-card.ps1`
capture_script_sha256: `5a8f3542e2421b82540dd5a17a6c59dcec9449ef595746e944aa3e1224bb7508`
fullgate_batch: `full-admin-host-mutation-gate-20261004-04289`
clean_package_msi_sha256: `e4574861a06537aacf41f16e75138e9f8cc9c5d4c8f0df7d7e977bd58e0b7391`
operational_fullgate_msi_sha256: `fe5677ff46e1bf23acf3638afd01b4f9a0814bd5bd39f8c81e62b1992fa843a2`
clean_package_payload_aggregate_sha256: `01bbfae597ecad5bf214fa68252c2e1099bfea877850cc716b9a41c3c53e76ca`
operational_fullgate_payload_aggregate_sha256: `e4f9387cf15e3be31816cf9db844fd9c50a50e5217aa1f37143d18bbc6ea388f`
provenance_commit: `a780928ee41f6064cbeebec754eca647dfff42f1`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.89`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.88-admin-smoke -> 0.42.89-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20261004-04288-04289`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.88-admin-smoke`
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

캡처 시각은 `2026-10-04T02:28:41.5909003Z`다. 설치본은 fullgate `full-admin-host-mutation-gate-20261004-04289`가 마지막에 다시 설치한 operational MSI이고 ARP product code는 `{CD234EF2-AF9A-4E62-9873-3680A14CD5C1}` 하나다. 설치본 Host/CLI SHA-256(`76043450…`/`de6deffd…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 테스트 VM은 없고 보존 VM 하나만 남았다. secret 형태 문자열은 관측되지 않았다.

0.42.88 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수 네 개, `canonical_current_evidence`를 바꿔 artifact root 밖에서 실행했고, 재현용으로 root에 복사했다. 이 스크립트는 읽기만 한다.

## Nonclaims

- 결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.88-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

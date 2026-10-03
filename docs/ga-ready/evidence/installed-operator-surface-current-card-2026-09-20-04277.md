# Installed operator surface current-card 2026-09-20 `0.42.77` (clean payload host)

evidence_id: `installed-operator-surface-current-card-2026-09-20-04277`
result: `FAIL`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.77-admin-smoke`
installed_manifest_version: `0.42.77-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260920-04277`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260920-04277/summary.json`
summary_sha256: `b8e637b39d0f97f57e0408ae15f86ec4a72df366ab0e248467e092853beed168`
fail_reason: `PCV_NETWORK_INVENTORY_FAILED`
host_supported: `false`
host_reasons: `PCV_DEFAULT_SWITCH_UNKNOWN`
default_switch_present: `false`
clean_package_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
installed_host_sha256: `d5588f0311be7ec8ef5daae352600e2f54a052c9191b2fedfe5b6bb154556902`
installed_cli_sha256: `51e924c490b54a55195e9d675174dcfbcbcb3eccff758e596d6dfb2cb77f36f3`
clean_payload_host_match: `true`
clean_payload_cli_match: `true`
operational_payload_host_match: `false`
provenance_commit: `04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6`
cli_exit_zero_count: `2`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
promotion_ledger_status: `fail-not-current`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status` | JSON ok, `supported=false`, `PCV_DEFAULT_SWITCH_UNKNOWN`, exit `0` | `PASS` (readback) |
| CLI `runtime policy` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `network inventory` | `PCV_NETWORK_INVENTORY_FAILED` | `FAIL` |
| Web `/` | HTTP `200` | `PASS` |
| Web `/pcv-config.js` | HTTP `200`, token-free | `PASS` |
| service | `Running/Automatic/LocalSystem` | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

설치본 Host/CLI hash는 clean package payload `04b3c9f` / `d5588f03…` / `51e924c4…`와
일치한다. 2026-08-30 current-card의 operational fullgate hash `810cccc9…`와는 다르다.
ARP `DisplayVersion`은 `0.42.75`로 남는다. product Update라서 MSI ProductVersion을
바꾸지 않았다.

## FAIL 원인

이 호스트에서 `Get-VMSwitch`가 `일반 오류입니다`로 실패하고 `default_switch_present=false`다.
CLI `network inventory`는 같은 Hyper-V 조회 실패를 `PCV_NETWORK_INVENTORY_FAILED`로 반환한다.
이 FAIL는 clean payload hash 불일치가 아니다.

## Nonclaims

- Lane 2 FAIL 프로브이며 `docs/ga-ready/current-evidence.json`을 바꾸지 않았다.
- FAIL는 `actual_vm_tested=pass` 또는 promotion eligibility 입력이 아니다.
- leftover VM `pcv-guest-installed-04253-r1`, `pcv-cleanhost-20260910-r2-04274-04275`는
  이 카드가 만들지 않았고 지우지 않았다. report-only다.
- Default Switch 복구/생성은 이 checkpoint 범위가 아니다.
- public trusted signing 또는 external stable publication evidence가 아니다.

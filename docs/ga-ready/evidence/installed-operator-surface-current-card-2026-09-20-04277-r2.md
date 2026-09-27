# Installed operator surface current-card 2026-09-20 `0.42.77` r2 (clean payload host)

evidence_id: `installed-operator-surface-current-card-2026-09-20-04277-r2`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
lane: `2`
working_authority: `installed_current`
version: `0.42.77-admin-smoke`
installed_manifest_version: `0.42.77-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260920-04277-r2`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260920-04277-r2/summary.json`
summary_sha256: `61c69a4db77dc771249715fef4a83b9b98fca61930068b3cbd6ff250991d8bb9`
predecessor_fail: `installed-operator-surface-current-card-2026-09-20-04277`
default_switch_recovery: `default-switch-recovery-2026-09-20-04277`
clean_package_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
installed_host_sha256: `d5588f0311be7ec8ef5daae352600e2f54a052c9191b2fedfe5b6bb154556902`
installed_cli_sha256: `51e924c490b54a55195e9d675174dcfbcbcb3eccff758e596d6dfb2cb77f36f3`
clean_payload_host_match: `true`
clean_payload_cli_match: `true`
operational_payload_host_match: `false`
operational_payload_cli_match: `false`
provenance_commit: `04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
promotion_ledger_status: `installed-non-promoted-candidate`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status` | JSON ok, `supported=true`, `default_switch_present=true`, `reasons=[]`, exit `0` | `PASS` |
| CLI `runtime policy` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `network inventory` | JSON ok, `Default Switch` `internal` `is_default=true` `allow_management_os=true`, exit `0` | `PASS` |
| Web `/` | HTTP `200` | `PASS` |
| Web `/pcv-config.js` | HTTP `200`, token-free `apiBaseUrl=http://127.0.0.1:7777` | `PASS` |
| service | `Running/Automatic/LocalSystem`, credential-target | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

직전 같은 날 FAIL 카드 `installed-operator-surface-current-card-2026-09-20-04277`는
`PCV_NETWORK_INVENTORY_FAILED` / `PCV_DEFAULT_SWITCH_UNKNOWN`이었다. Default Switch 복구
`default-switch-recovery-2026-09-20-04277` 이후 이 r2 recapture가 같은 설치본(clean
payload)을 다시 읽었다.

설치본 Host/CLI hash는 clean package payload `04b3c9f` / `d5588f03…` / `51e924c4…`와
일치한다. ledger에 남은 2026-08-30 current-card의 operational fullgate hash
`810cccc9…` / `6cbfa2df…`와는 다르다. ARP `DisplayVersion`은 `0.42.75`로 남는다.
product Update라서 MSI ProductVersion을 바꾸지 않았다.

## 승격 경계

이 카드는 Lane 2 설치본 recapture다. `docs/ga-ready/current-evidence.json`의
`installed_evidence`는 계속
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-30-04277.md`다.
이 recapture가 current-evidence를 바꾸지 않았고, operational fullgate identity와 clean
payload identity를 섞지 않는다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- leftover VM `pcv-cleanhost-20260910-r2-04274-04275` (Saved),
  `pcv-guest-installed-04253-r1` (Off)는 이 카드가 만들지 않았고 지우지 않았다.
  report-only다.
- ARP DisplayVersion `0.42.75`는 msiexec 없이 남은 값이며 이 recapture가 고치지 않는다.
- public trusted signing 또는 external stable publication evidence가 아니다.

# Installed operator surface current-card 2026-08-30 `0.42.77`

evidence_id: `installed-operator-surface-current-card-2026-08-30-04277`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.77-admin-smoke`
installed_manifest_version: `0.42.77-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260830-04277`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260830-04277/summary.json`
summary_sha256: `6c576475be532b8b0c0ca8d0d47078d48f27f57584a61e78cf58f5858b7b08bf`
fullgate_batch: `full-admin-host-mutation-gate-20260830-04277`
clean_package_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
operational_fullgate_msi_sha256: `d4ebba77adcd7af92275509a65809c926f5bc6fb6bf8f61c49a610943998000f`
clean_package_payload_aggregate_sha256: `370a267f7c9fdec1d89c9a1890af4941c688d25b9cad634d45de3774b5e4b99c`
operational_fullgate_payload_aggregate_sha256: `d16e498a3d14ed67e361bef26a26feb87839490425e2101453f28742839d84a1`
provenance_commit: `9f051b5a9cca80634e8ad7c4d15267a414c79d66`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
promotion_ledger_status: `not-promoted`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `runtime policy` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `network inventory` | JSON ok, stderr empty, exit `0` | `PASS` |
| Web `/` | HTTP `200` | `PASS` |
| Web `/pcv-config.js` | HTTP `200`, token-free `apiBaseUrl` | `PASS` |
| service | `Running/Automatic/LocalSystem` | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

Service argv는 credential-manager target을 사용하고 raw/protected token flag는
사용하지 않는다. 설치본 Host/CLI hash는 operational fullgate payload와 일치하고,
`04b3c9f` clean package payload와는 다르다. provenance는 `9f051b5`다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `d03eedaf…` PASS, current 아님 |
| fullgate | 2 steps, exit `0`, attempt `1` |
| clone actual-VM | `service-plan-p1-clone-actual-vm-2026-08-29-04277-r2` PASS |
| manual-admin pair | `0.42.75 -> 0.42.77` not-opened |
| cleanup | `pcv-spike-*` 잔여 `0` |

## 승격 경계

이 current-card의 promotion 판정은 `not-promoted`다. Canonical current-evidence는
`0.42.75-admin-smoke`로 유지한다. Lane 3 ledger update와 package-pair는 별도 승인이
필요하다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- 호스트 leftover `pcv-guest-installed-04253-r1`는 이 카드가 만들지 않았고 지우지
  않았다. report-only다.

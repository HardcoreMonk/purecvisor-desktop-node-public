# Installed operator surface current-card 2026-08-27 `0.42.75`

evidence_id: `installed-operator-surface-current-card-2026-08-27-04275`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.75-admin-smoke`
installed_manifest_version: `0.42.75-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260827-04275-r2`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260827-04275-r2/summary.json`
summary_sha256: `3c0378fc0046e328b5637e5872d349920b01bd53a671567fa947e643538f6ce6`
fullgate_batch: `full-admin-host-mutation-gate-20260821-04275`
clean_package_msi_sha256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`
operational_fullgate_msi_sha256: `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b`
clean_package_payload_aggregate_sha256: `3c33a35b21eb9cdd2b24156cc98afe2268f82f3ca32c7dd6a03882a262afdd2c`
operational_fullgate_payload_aggregate_sha256: `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7`
provenance_commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.74-admin-smoke -> 0.42.75-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260827-04274-04275`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `true`
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
사용하지 않는다. 설치본 CLI/Host hash는 clean package payload와 일치한다.
04274 operational fullgate MSI와는 별도 빌드다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `3d3ee255…` PASS |
| fullgate | 2 steps, exit `0`, attempt `1` |
| manual-admin | `0.42.74 -> 0.42.75` campaign `PASS` |
| actual-VM functional | `PASS` |
| actual-VM P0 | `PASS` (SavedOnly r2, Full r4, clean-target SavedOnly) |
| token rotation | 04272 R4 carry-forward. token payload 변경 없음 |
| cleanup | `pcv-p0-*`, `pcv-fc-cf-*`, `pcv-spike-*` 잔여 `0` |

## 승격 경계

Package/fullgate/manual-admin/actual-VM functional/P0/current-card 체인이 모두 `PASS`다.
token R4는 token payload 변경이 없어 04272 final evidence를 carry-forward한다. 이
current-card의 promotion 판정은 `promoted-current`다. Canonical current-evidence와
생성 인덱스 동기화는 같은 Lane 3 ledger update가 소유한다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- token rotation installed PASS는 연결된 04272 R4 final evidence가 소유한다.

# Installed operator surface current-card 2026-09-30 `0.42.84`

evidence_id: `installed-operator-surface-current-card-2026-09-30-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.84-admin-smoke`
installed_manifest_version: `0.42.84-admin-smoke`
installed_product_version: `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260930-04284-r2`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260930-04284-r2/summary.json`
summary_sha256: `4b5130ece499b7f11e62836a7aa0b88cf584cb631259afef7bd55684768e82f3`
capture_script: `artifacts/installed-operator-surface-current-card-20260930-04284-r2/capture-current-card.ps1`
capture_script_sha256: `b13e99d8f1b6cdcb4213a3307b0aa78d9060fb9617f658f64235519cdb60ecec`
fullgate_batch: `full-admin-host-mutation-gate-20260930-04284`
clean_package_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
operational_fullgate_msi_sha256: `f9e1e341a3541d91feb63723e91bb4956be02d75fbad41dda3344b8f0e6a5482`
clean_package_payload_aggregate_sha256: `9d8c92c5646c0d8ab7e7516602169295dddf66b89ed73679156a15709090d7f5`
operational_fullgate_payload_aggregate_sha256: `77481bdbd86f6bce78dd47375022d0fc71f6a9a26cf26d3b26b46df691de4ae7`
provenance_commit: `ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
arp_entry_count: `1`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.83-admin-smoke -> 0.42.84-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260930-04283-04284`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status` | JSON ok, operation `host.status`, stderr empty, exit `0` | `PASS` |
| CLI `runtime policy` | JSON ok, operation `runtime.policy`, stderr empty, exit `0` | `PASS` |
| CLI `network inventory` | JSON ok, operation `network.inventory`, stderr empty, exit `0` | `PASS` |
| Web `/` | HTTP `200` | `PASS` |
| Web `/pcv-config.js` | HTTP `200`, token-free `apiBaseUrl` | `PASS` |
| service | `Running/Automatic/LocalSystem` | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하거나 기록하지 않았다. CLI 출력과 Web 본문에 token 형태 문자열은 없었다.

service argv는 `--api-token-credential-target`으로 credential manager target을 쓴다. raw token이나 protected token 파일 flag는 없다.

설치본 Host/CLI hash는 fullgate operational payload와 같다(`ec0f479e…` / `60d8922b…`). clean package payload와는 다르다. provenance는 `ee90e0e`다.

## 캡처 방식

0.42.83 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수를 바꿔 캡처했다. 재현할 수 있도록 스크립트를 artifact root에 함께 두었다. 이 스크립트는 읽기만 하며 설치본, 서비스, VM을 바꾸지 않았다.

첫 캡처(`artifacts/installed-operator-surface-current-card-20260930-04284`)도 판정은 `pass`였다. 하지만 스크립트에 남은 0.42.83 기대 SHA 상수와 canonical 값 때문에 summary 일부가 틀렸다. 그래서 상수를 고쳐 `-r2`로 다시 캡처했다. 표면 관측값은 두 실행이 같았다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `12a582ef…` PASS (`admin-smoke-package-2026-09-30-04284`) |
| pair | `0.42.83 -> 0.42.84` descriptor PASS (`manual-admin-campaign-descriptor-2026-09-30-04283-04284`) |
| fullgate | 2 steps, exit `0`, attempt `1` (`full-admin-host-mutation-gate-2026-09-30-04284-hostmutation`) |
| 새 기능 actual-VM | PASS (`lane2-development-completion-actual-vm-2026-09-30-04284`) |
| token | 04272 R4 carry-forward. token payload는 04272 이후 변경 없음 |

## Nonclaims

- 캡처는 Lane 3 전에 했다(artifact summary는 `not-promoted`). 2026-09-30 Lane 3가 이 카드를 `0.42.84-admin-smoke` operational current의 installed current-card로 승격했다(`promoted-current`).
- public trusted signing과 external stable publication을 주장하지 않는다.

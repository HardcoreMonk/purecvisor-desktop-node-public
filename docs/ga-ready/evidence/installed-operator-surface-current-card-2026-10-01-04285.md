# Installed operator surface current-card 2026-10-01 `0.42.85`

evidence_id: `installed-operator-surface-current-card-2026-10-01-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
evidence_status: `installed-non-promoted-candidate`
version: `0.42.85-admin-smoke`
installed_manifest_version: `0.42.85-admin-smoke`
installed_product_version: `0.42.85-admin-smoke+62a0a1e32e3a8686eedc6e0a3da1406f6c0f3e80`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261001-04285`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261001-04285/summary.json`
summary_sha256: `17b7f148d75fd2bf4ba17936569af0b984860f590011ae03cff31f52717b6490`
capture_script: `artifacts/installed-operator-surface-current-card-20261001-04285/capture-current-card.ps1`
capture_script_sha256: `87a1e1052bd5d426986b0cb97d7d57dce2d4f9bd079d14f6bc1bcebb875e7ba8`
fullgate_batch: `full-admin-host-mutation-gate-20261001-04285`
clean_package_msi_sha256: `cba74683e6ae9f7e85e5f625f8e9b9f221ae27c8ffacf5912ce2861f1ef82828`
operational_fullgate_msi_sha256: `22d2f99ee390df51cfd6657c04a38eb46b3216574f3bd7f44dd2c34905d2d6ed`
clean_package_payload_aggregate_sha256: `796faba68eeeb1864f7f877debe8b134d43fb1a4c9ce026891b83fe6f61cb54b`
operational_fullgate_payload_aggregate_sha256: `53a3d18eac780bb4a742ec06a1cae8f51c7ff166399dc45ef218d5950eb720ce`
provenance_commit: `62a0a1e32e3a8686eedc6e0a3da1406f6c0f3e80`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
arp_entry_count: `1`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.84-admin-smoke -> 0.42.85-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20261001-04284-04285`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
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

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하지도 기록하지도 않았다. CLI 출력과 Web 본문에도 token 형태 문자열은 없었다.

service argv는 `--api-token-credential-target`으로 credential manager target을 가리킨다. raw token flag나 protected token 파일 flag는 없다.

설치본 Host/CLI hash는 fullgate operational payload와 같다(`76b7410b…` / `9476779d…`). clean package payload와는 다르다. provenance는 `62a0a1e`다.

## 캡처 방식

0.42.84 current-card r2 스크립트에서 다음 값만 바꿔 캡처했다:

- version, 경로, batch id
- provenance
- 기대 SHA 상수
- canonical 값(`0.42.84-admin-smoke`)

바꾼 뒤 남은 `04284` 경로 문자열은 `0`개다. 재현할 수 있도록 스크립트를 artifact root에 함께 두었다. 이 스크립트는 읽기만 하며 설치본, 서비스, VM을 바꾸지 않았다. 캡처 시각은 `2026-09-30T15:52:11Z`다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `cba74683…` PASS (`admin-smoke-package-2026-10-01-04285`) |
| pair | `0.42.84 -> 0.42.85` descriptor PASS (`manual-admin-campaign-descriptor-2026-10-01-04284-04285`) |
| fullgate | 2 steps, exit `0`, attempt `1` (`full-admin-host-mutation-gate-2026-10-01-04285-hostmutation`) |
| token | 04272 R4 carry-forward. token payload는 04272 이후 바뀌지 않았다 |

## Nonclaims

- 이 카드는 `installed-non-promoted-candidate`다. operational current는 `0.42.84-admin-smoke`로 유지된다. Lane 3 승격은 이 campaign의 승인 밖이다.
- public trusted signing과 external stable publication을 주장하지 않는다.

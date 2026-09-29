# Installed operator surface current-card 2026-09-29 `0.42.83`

evidence_id: `installed-operator-surface-current-card-2026-09-29-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.83-admin-smoke`
installed_manifest_version: `0.42.83-admin-smoke`
installed_product_version: `0.42.83-admin-smoke+68462481dee72049a1c0918f201efa2e1c387160`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260929-04283`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260929-04283/summary.json`
summary_sha256: `15d8e072ee38a03c51a4562d8ae8c8b3f7545d6bf1d995db14d0e79cd9788d4f`
capture_script: `artifacts/installed-operator-surface-current-card-20260929-04283/capture-current-card.ps1`
capture_script_sha256: `4cab16b3120b521a84865fef5367433691c4bdef62191791e320e1d205e56b5f`
fullgate_batch: `full-admin-host-mutation-gate-20260929-04283`
clean_package_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
operational_fullgate_msi_sha256: `b7e26bfb466dc671e3651dace50eb34deabc8c433fbb507d120f91e04f4651a7`
clean_package_payload_aggregate_sha256: `76d9ae9cfcc13fbffb999ad0b447d8d24ac31ec31fd14df20a59bbe681ca8e15`
operational_fullgate_payload_aggregate_sha256: `773eb918591361a978ef7a3c83176472387a544ae4ee1bbdd8d77ab5a456edc5`
provenance_commit: `68462481dee72049a1c0918f201efa2e1c387160`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
arp_entry_count: `1`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.78-admin-smoke -> 0.42.83-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260929-04278-04283`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
canonical_current_evidence: `0.42.78-admin-smoke`
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

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하거나 기록하지 않았다. CLI 출력과 Web 본문에 token 형태 문자열은 없었다.

service argv는 `--api-token-credential-target`으로 credential manager target을 쓴다. raw token이나 protected token 파일 flag는 없다.

설치본 Host/CLI hash는 fullgate operational payload와 같다(`22e4ed83…` / `2a4e1649…`). clean package payload와는 다르다. provenance는 `6846248`이다.

## 캡처 방식

0.42.78 current-card처럼 저장소 runner 없이 임시 스크립트로 캡처했다. 재현할 수 있도록 스크립트를 artifact root에 함께 두었다. 이 스크립트는 읽기만 하며 설치본, 서비스, VM을 바꾸지 않았다.

첫 실행은 service flag 판정 정규식이 `--api-token`을 `--api-token-credential-target`의 앞부분으로 잘못 잡아 `status=fail`이었다. 정규식을 flag 전체 일치로 고친 뒤 다시 캡처했다. 표면 관측값은 두 실행이 같았다. 첫 실행 artifact는 남기지 않았다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `52d7cfd5…` PASS (`admin-smoke-package-2026-09-29-04283`) |
| pair | `0.42.78 -> 0.42.83` descriptor PASS (`manual-admin-campaign-descriptor-2026-09-29-04278-04283`) |
| fullgate | 2 steps, exit `0`, attempt `1` (`full-admin-host-mutation-gate-2026-09-29-04283-hostmutation`) |
| token | 04272 R4 carry-forward. token payload는 04272 이후 변경 없음 |

## Nonclaims

- 이 current-card는 Lane 3 전의 캡처다(`promotion_ledger_status=not-promoted`). current-evidence는 Task 5에서 쓴다.
- public trusted signing과 external stable publication을 주장하지 않는다.

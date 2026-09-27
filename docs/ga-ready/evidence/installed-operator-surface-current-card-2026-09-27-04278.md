# Installed operator surface current-card 2026-09-27 `0.42.78`

evidence_id: `installed-operator-surface-current-card-2026-09-27-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
installed_manifest_version: `0.42.78-admin-smoke`
installed_product_version: `0.42.78-admin-smoke+0de176f12cbfe2bc8f842396159e3c81dfbd3b9b`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260927-04278-r2`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260927-04278-r2/summary.json`
summary_sha256: `eeaf91db0efddd64b915ee21ed4ba00b893f6e480418ad95617a8eec3790ead0`
fullgate_batch: `full-admin-host-mutation-gate-20260927-04278-r2`
clean_package_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
operational_fullgate_msi_sha256: `0856d07ee7576a1cd44ca18061e2c9351ddef95271adca0b7b0b319a02d278b6`
clean_package_payload_aggregate_sha256: `999f7106d6c63594f9e13d0d17dcfa97364f5b40ea4b86e28342776ef7b16ac1`
operational_fullgate_payload_aggregate_sha256: `2dfabb939317e4fd6a29b557d98a2d0f35a4197201b8173698737ff8f68ea4bb`
provenance_commit: `0de176f12cbfe2bc8f842396159e3c81dfbd3b9b`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
arp_entry_count: `1`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.77-admin-smoke -> 0.42.78-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260927-04277-04278`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
canonical_current_evidence: `0.42.78-admin-smoke`
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
사용하지 않는다. 설치본 Host/CLI hash는 r2 operational fullgate payload와 일치하고,
`e098e0a` clean package payload와는 다르다. provenance는 `0de176f`다.

## 첫 캡처와의 관계

같은 날 첫 캡처(`artifacts/installed-operator-surface-current-card-20260927-04278`)는
CLI/Web/service 표면이 모두 통과했지만, 설치본 Host/CLI가 `e098e0a` build
(`f2314360…`/`9222ef93…`)라 identity 검사에서 `status=fail`이었다. 잔여 ProductCode를
정리하고 r2 gate를 실행한 뒤 다시 캡처한 이 카드가 그 결과를 대체한다. 첫 캡처
artifact는 덮어쓰지 않고 보존한다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `c3390c1e…` PASS (`admin-smoke-package-2026-09-25-04278`) |
| fullgate | r2 2 steps, exit `0`, attempt `1` (`full-admin-host-mutation-gate-2026-09-27-04278-r2-hostmutation`) |
| manual-admin pair | `0.42.77 -> 0.42.78` consume descriptor `20260927` PASS (원본 `20260925`, plan-only) |
| functional | `functional-correctness-actual-host-validation-2026-09-27-04278-carryforward` (`0.42.75` PASS carry-forward) |
| cleanup | `pcv-spike-*` 잔여 `0` |

## 승격 경계

2026-09-27 Lane 3가 이 current-card를 `promoted-current`로 승격했다. Canonical
current-evidence는 `0.42.78-admin-smoke`다. token 관련 source(`src/*Token*`, `src/*Credential*`
비테스트 파일 `6`개)는 `0.42.77` 승격(`a842ede`) 뒤로 바뀌지 않아 token rotation evidence를
carry-forward한다. pair target은 clean MSI `c3390c1e…`이며
operational fullgate MSI `0856d07e…`와는 다른 identity다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- 호스트 leftover `pcv-guest-installed-04253-r1`는 이 카드가 만들지 않았고 지우지
  않았다. report-only다.

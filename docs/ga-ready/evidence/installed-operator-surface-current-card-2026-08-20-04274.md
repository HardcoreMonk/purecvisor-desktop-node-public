# Installed operator surface current-card 2026-08-20 `0.42.74`

evidence_id: `installed-operator-surface-current-card-2026-08-20-04274`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.74-admin-smoke`
installed_manifest_version: `0.42.74-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260820-04274`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260820-04274/summary.json`
summary_sha256: `531fc614da5edb0e11994b021383491ccb8830115d59fb211c6c330f5b25f8c8`
fullgate_batch: `full-admin-host-mutation-gate-20260820-04274`
clean_package_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
operational_fullgate_msi_sha256: `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`
clean_package_payload_aggregate_sha256: `c55cd17d14fed521252e6fee1bf08c828410339b23172fadb01dbd19f7d2578e`
operational_fullgate_payload_aggregate_sha256: `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`
provenance_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.73-admin-smoke -> 0.42.74-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260820-04273-04274-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
canonical_current_evidence: `0.42.74-admin-smoke`
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
사용하지 않는다. 이 캡처는 fullgate 직후 설치본 read-only smoke다.

runtime policy의 native mutation operations는 SERVICE_PLAN P0
`vm.attach` / `vm.save` / `vm.resume-saved` / `vm.manage`를 포함한다. network
inventory는 `Default Switch`를 `internal` / `allow_management_os=true`로
반환했다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `f4d0fcb7…` PASS |
| fullgate | 2 steps, exit `0`, attempt `1` |
| manual-admin | `0.42.73 -> 0.42.74` campaign `PASS` |
| actual-VM functional | `PASS` |
| actual-VM P0 | `FAIL` (`vm.save` WMI `32775`, 열린 결함) |
| token rotation | 04272 R4 carry-forward. token payload 변경 없음 |
| cleanup | `pcv-cleanhost-*`, `pcv-fc-cf-*`, `pcv-spike-*` 잔여 `0` |

## 승격 경계

Package/fullgate/manual-admin/actual-VM functional/current-card 체인이 모두 `PASS`다.
token R4는 token payload 변경이 없어 04272 final evidence를 carry-forward한다. SERVICE_PLAN
P0 `vm.save` actual-VM FAIL는 열린 결함이며 이 승격이 고치지 않는다. 이 current-card의
promotion 판정은 `promoted-current`다. Canonical current-evidence와 생성 인덱스 동기화는
같은 승격 변경의 ledger update가 소유하며, 이 evidence 문서 자체가 canonical anchor를
직접 생성하지는 않는다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- token rotation installed PASS는 연결된 04272 R4 final evidence가 소유한다.

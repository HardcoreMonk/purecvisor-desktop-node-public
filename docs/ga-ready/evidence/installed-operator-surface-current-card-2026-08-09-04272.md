# Installed operator surface current-card 2026-08-09 `0.42.72`

evidence_id: `installed-operator-surface-current-card-2026-08-09-04272`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.72-admin-smoke`
installed_manifest_version: `0.42.72-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260809-04272`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260809-04272/summary.json`
summary_sha256: `02304c8f93d122d21310ba7549356e7d12decbfc95342e799850d3929cf3f05a`
fullgate_batch: `full-admin-host-mutation-gate-20260809-04272`
clean_package_msi_sha256: `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`
operational_fullgate_msi_sha256: `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`
clean_package_payload_aggregate_sha256: `39475ad14a9bbd48ecf41c24bac5e42b391535783276cd5ed4d960af276962f0`
operational_fullgate_payload_aggregate_sha256: `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`
provenance_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.71-admin-smoke -> 0.42.72-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260809-04271-04272-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
promotion_ledger_status: `promoted-current`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

| surface | readback | result |
| --- | --- | --- |
| CLI `host status` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `runtime policy` | JSON ok, stderr empty, exit `0` | `PASS` |
| CLI `network inventory` | JSON ok, stderr empty, exit `0` | `PASS` |
| Web `/` | HTTP `200` | `PASS` |
| Web `/pcv-config.js` | HTTP `200` | `PASS` |
| service | `Running/Automatic/LocalSystem` | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

Service argv는 credential-manager target을 정확히 1회 사용하고 protected/raw token flag는
사용하지 않는다. Active package는 operational product code
`{56BB5D44-2CE1-4FD6-81A4-DDD4B6468998}` 하나이며 installed payload aggregate가
operational anchor `deb40a67c…`와 일치한다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| fullgate | 2 steps, exit `0`, attempt `1` |
| manual-admin | 6 buckets, `6/6 PASS` |
| actual-VM functional | 10 steps, QoS/shrink/expand/cleanup `PASS` |
| token rotation | R4 read-only reconciliation `PASS`, current-eligible |
| cleanup | `pcv-cleanhost-*`, `pcv-fc-cf-*`, `pcv-spike-*` 잔여 `0` |

## 승격 경계

R4 token final evidence가 prior verifier false-negative를 read-only로 재검증해 `PASS`로
닫았다. Package/fullgate/manual-admin/actual-VM/current-card/token final 체인이 모두
`PASS`이므로 이 current-card의 promotion 판정은 `promoted-current`다. Canonical
current-evidence와 생성 인덱스 동기화는 같은 승격 변경의 ledger update가 소유하며, 이
evidence 문서 자체가 canonical anchor를 직접 생성하지는 않는다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- token rotation installed PASS는 연결된 R4 final evidence가 소유한다.

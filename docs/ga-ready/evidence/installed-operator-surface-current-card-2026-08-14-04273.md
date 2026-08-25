# Installed operator surface current-card 2026-08-14 `0.42.73`

evidence_id: `installed-operator-surface-current-card-2026-08-14-04273`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.73-admin-smoke`
installed_manifest_version: `0.42.73-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260814-04273`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260814-04273/summary.json`
summary_sha256: `44a91426579c6fb486e6b99cca2321ba4fd8cd547d16797017e0baa6c9d0da14`
fullgate_batch: `full-admin-host-mutation-gate-20260814-04273`
clean_package_msi_sha256: `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
operational_fullgate_msi_sha256: `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`
clean_package_payload_aggregate_sha256: `bbe2bfde532260eab7bd80de13e4e13350ae6553e4ef6a4037faa6e650359660`
operational_fullgate_payload_aggregate_sha256: `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`
provenance_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.72-admin-smoke -> 0.42.73-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260814-04272-04273-closed`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
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

Service argv는 credential-manager target을 사용하고 raw/protected token flag는
사용하지 않는다. 이 캡처는 fullgate 직후 설치본 read-only smoke다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| fullgate | 2 steps, exit `0`, attempt `1` |
| manual-admin | 6 buckets, `6/6 PASS` |
| actual-VM functional | 10 steps, QoS/shrink/expand/cleanup `PASS` |
| token rotation | 04272 R4 carry-forward. token payload 변경 없음 |
| cleanup | `pcv-cleanhost-*`, `pcv-fc-cf-*`, `pcv-spike-*` 잔여 `0` |

## 승격 경계

Package/fullgate/manual-admin/actual-VM/current-card 체인이 모두 `PASS`다. token R4는
token payload 변경이 없어 04272 final evidence를 carry-forward한다. 이 current-card의
promotion 판정은 `promoted-current`다. Canonical current-evidence와 생성 인덱스 동기화는
같은 승격 변경의 ledger update가 소유하며, 이 evidence 문서 자체가 canonical anchor를
직접 생성하지는 않는다.

## Nonclaims

- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다.
- public trusted signing 또는 external stable publication evidence가 아니다.
- token rotation installed PASS는 연결된 04272 R4 final evidence가 소유한다.

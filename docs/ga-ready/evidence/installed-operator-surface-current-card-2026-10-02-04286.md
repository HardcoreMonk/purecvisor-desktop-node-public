# Installed operator surface current-card 2026-10-02 `0.42.86`

evidence_id: `installed-operator-surface-current-card-2026-10-02-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.86-admin-smoke`
installed_manifest_version: `0.42.86-admin-smoke`
installed_product_version: `0.42.86-admin-smoke+b807803f778e29c206f1bb2ba8277d2a1136198f`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261002-04286`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261002-04286/summary.json`
summary_sha256: `4f5d802cfa5793331259d9cd7780d2419084297915fc841560cf01fb61aa43b2`
capture_script: `artifacts/installed-operator-surface-current-card-20261002-04286/capture-current-card.ps1`
capture_script_sha256: `12e4591394e0b118f56350bd47e29910b4c8101fff8330d588fecfa4351df748`
fullgate_batch: `full-admin-host-mutation-gate-20261002-04286`
clean_package_msi_sha256: `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`
operational_fullgate_msi_sha256: `85387f31b6892c5be37ec25b22f64d5a655f323e870e29812e65b40ad6ed8f3e`
clean_package_payload_aggregate_sha256: `afa5cb95c4268f7118c52c554e3061b738c01cd3d4a3b4c75af04d094ee61484`
operational_fullgate_payload_aggregate_sha256: `bba7e10c0970e580bb5d5f176aa93e942961aa9c89c5e446da8de66208f9187d`
provenance_commit: `b807803f778e29c206f1bb2ba8277d2a1136198f`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.86`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.85-admin-smoke -> 0.42.86-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20261002-04285-04286`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_r4_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
token_rotation_r4_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `promoted-current`
canonical_current_evidence: `0.42.86-admin-smoke`
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

캡처 시각은 `2026-10-02T06:32:48.9426951Z`다. 이 시각의 설치본은 fullgate `full-admin-host-mutation-gate-20261002-04286`가 마지막에 다시 설치한 operational MSI다. ARP product code는 `{1994F4DF-A62D-497F-83FF-39FEB994FA8D}`다.

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하거나 기록하지 않았다. CLI 출력과 Web 본문에 token 형태 문자열은 없었다.

service argv는 `--api-token-credential-target`으로 credential manager target을 쓴다. raw token이나 protected token 파일 flag는 없다. summary의 service state는 `Running/Auto`이고, CIM StartMode `Auto`는 Automatic이다.

설치본 Host SHA-256은 `d886c632b8585968f4bd0526400c20216621c86895e88c118dd9d76345eba0ae`, CLI SHA-256은 `93134bb8872a09b9100a82f20d81728c8a7c70794b032f5f81976ada4450f717`이다. 둘 다 fullgate operational payload와 같다. clean package payload(`55a1c1a8…` / `fd01b161…`)와는 다르다. provenance는 `b807803`이다.

남아 있는 VM은 보존 대상 `pcv-guest-installed-04253-r1` 하나이고 State는 Off다. `pcv-spike-*` 테스트 VM은 없다. 이 캡처는 설치본, 서비스, VM을 바꾸지 않았다.

## 캡처 방식

0.42.85 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수를 바꿔 캡처했다. 재현할 수 있도록 스크립트를 artifact root에 함께 두었다. 이 스크립트는 읽기만 한다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `8edb19ce…`, source `1c488b6`, media fix `fb95de1` (`admin-smoke-package-2026-10-02-04286`) |
| pair | `0.42.85 -> 0.42.86` descriptor PASS (`manual-admin-campaign-descriptor-2026-10-02-04285-04286`) |
| fullgate | 2 steps, exit `0` (`full-admin-host-mutation-gate-2026-10-02-04286-hostmutation`) |
| media probe | 설치 당시 clean package에서 PASS (`lane2-vm-media-eject-attach-2026-10-02-04286`). 이 카드의 설치본 hash와는 다르다 |
| token | 04272 R4 carry-forward. token payload는 04272 이후 변경 없음 |

## Nonclaims

- 캡처는 Lane 3 전에 했다(artifact summary는 `not-promoted`). 2026-10-02 Lane 3가 이 카드를 `0.42.86-admin-smoke` operational current의 installed current-card로 승격했다(`promoted-current`). `0.42.85-admin-smoke`는 `fb95de1`을 포함하지 않으며 operational current가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.

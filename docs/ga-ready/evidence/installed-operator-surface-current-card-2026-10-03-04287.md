# Installed operator surface current-card 2026-10-03 `0.42.87`

evidence_id: `installed-operator-surface-current-card-2026-10-03-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.87-admin-smoke`
installed_manifest_version: `0.42.87-admin-smoke`
installed_product_version: `0.42.87-admin-smoke+8ade930587941e24451f31a352fe2a412841f001`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20261003-04287`
artifact_summary: `artifacts/installed-operator-surface-current-card-20261003-04287/summary.json`
summary_sha256: `866330f3a79de66c82ccee7a3c8cc721d00a53dbea31c64cae477f84ef52678d`
capture_script: `artifacts/installed-operator-surface-current-card-20261003-04287/capture-current-card.ps1`
capture_script_sha256: `7bc9c4084237671ab8625e75c9d42da320870f7954aa12f40a3dbad773e544d2`
fullgate_batch: `full-admin-host-mutation-gate-20261003-04287`
clean_package_msi_sha256: `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1`
operational_fullgate_msi_sha256: `f339ab45a54b31db0bdfda4e9349abae5bf443229a6158cac092d617243a6817`
clean_package_payload_aggregate_sha256: `7c3393887f4cce4a923ba932b716b9f0878d2e9924c6ece64188913fcb0b42a2`
operational_fullgate_payload_aggregate_sha256: `526acd9fa445d3c776ccbd8cb0b4303762ce60114bf902808d40d68c5895cd2c`
provenance_commit: `8ade930587941e24451f31a352fe2a412841f001`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Auto`
service_start_name: `LocalSystem`
service_uses_credential_manager: `true`
service_has_raw_or_protected_token_flag: `false`
arp_entry_count: `1`
arp_display_version: `0.42.87`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false`
latest_manual_admin_package_pair: `0.42.86-admin-smoke -> 0.42.87-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20261003-04286-04287`
token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
token_rotation_status: `carry-forward-no-token-payload-change-after-04272`
promotion_ledger_status: `not-promoted`
installed_status: `installed_non_promoted_candidate`
canonical_current_evidence: `0.42.86-admin-smoke`
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
| Web `/pcv-config.js` | HTTP `200`, token-free | `PASS` |
| service | `Running/Automatic/LocalSystem` | `PASS` |
| TUI | `pcvtui.exe` absent | expected |

캡처 시각은 `2026-10-03T07:32:12.6286532Z`다. 이 시각의 설치본은 fullgate `full-admin-host-mutation-gate-20261003-04287`가 마지막에 다시 설치한 operational MSI다. ARP product code는 `{F1D32C79-B11D-4C26-BD82-0E48A44CCC58}` 하나다.

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하거나 기록하지 않았고 secret 형태 문자열은 관측되지 않았다. service argv는 credential manager target을 쓰고 raw token이나 protected token 파일 flag가 없다.

설치본 Host SHA-256은 `c97c62bd6fee575749175a1e6823b05a3ccd7d6da8e5f0d6df3ef7acf07c1efc`, CLI SHA-256은 `5374c24921ba5311482982c8411309fe6ab7c842a00bf217c42ac9ed2a04a96a`이다. 둘 다 fullgate operational payload와 같고 clean package payload와는 다르다. provenance는 `8ade930`(product payload는 `8d940da`와 같음)이다.

남아 있는 VM은 보존 대상 `pcv-guest-installed-04253-r1` 하나이고 테스트 VM은 없다. 이 캡처는 설치본, 서비스, VM을 바꾸지 않았다.

## 캡처 방식

0.42.86 current-card 스크립트에서 version, 경로, batch id, provenance, 기대 SHA 상수 네 개, `canonical_current_evidence`를 바꿔 캡처했다. 스크립트는 artifact root 밖에서 실행한 뒤 재현용으로 root에 복사했다. 이 스크립트는 읽기만 한다.

## 연결 evidence

| 평면 | readback |
| --- | --- |
| package | clean MSI `a0041c9f…`, source `8d940da` (`admin-smoke-package-2026-10-03-04287`) |
| pair | `0.42.86 -> 0.42.87` descriptor PASS (`manual-admin-campaign-descriptor-2026-10-03-04286-04287`) |
| fullgate | 2 steps, exit `0`, 같은 version major upgrade 실증 (`full-admin-host-mutation-gate-2026-10-03-04287-hostmutation`) |
| token | 04272 R4 carry-forward. token payload는 04272 이후 변경 없음 |

## Nonclaims

- 결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `0.42.86-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.

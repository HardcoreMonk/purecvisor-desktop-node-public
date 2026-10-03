# Full admin host mutation gate `0.42.87-admin-smoke` (2026-10-03)

evidence_id: `full-admin-host-mutation-gate-2026-10-03-04287-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.87-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261003-04287`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261003-04287`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261003-04287.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261003-04287`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261003-04287`
batch_summary_sha256: `fe9addbc8f330186ac0a5a8058f846bc72d8dac7aec0ca5521944400eed9006f`
routeparity_summary_sha256: `1b6d88371df3b01e6d44e1e5c9e9b118fa82dbf38fff7a66b6e313bfa0599a62`
os_summary_sha256: `bd33a989bd4fde1bc10560fd4e43efc99483b4fd0e1946d38ef74e2a1f29723f`
operational_fullgate_msi_sha256: `f339ab45a54b31db0bdfda4e9349abae5bf443229a6158cac092d617243a6817`
operational_fullgate_payload_aggregate_sha256: `526acd9fa445d3c776ccbd8cb0b4303762ce60114bf902808d40d68c5895cd2c`
service_host_sha256: `c97c62bd6fee575749175a1e6823b05a3ccd7d6da8e5f0d6df3ef7acf07c1efc`
cli_sha256: `5374c24921ba5311482982c8411309fe6ab7c842a00bf217c42ac9ed2a04a96a`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.87-admin-smoke+8ade930587941e24451f31a352fe2a412841f001`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
provenance_commit: `8ade930587941e24451f31a352fe2a412841f001`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태 (설치본을 비우지 않음)

이전 gate들은 같은 version ProductCode가 겹치지 않도록 시작 전에 설치본을 `msiexec /x`로 비웠다. 이번에는 같은 version 재설치 설계 권고 A(`AllowSameVersionUpgrades`)를 실증하려고 비우지 않았다. Burn이 복구한 clean package `0.42.87`이 설치된 채로 gate를 시작했다.

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{5EF95755-746A-494F-A160-ECCE7AAC2C80}` `0.42.87` (1개, clean package) |
| `DesktopNode.Host.exe` ProductVersion | `0.42.87-admin-smoke+8d940dab1e2aae1ad1e4cac5def45eb26a4343ae` |
| VM | `pcv-guest-installed-04253-r1` Off |
| `PureCVisor` firewall rule | `0` |

ISO: 이전 gate가 쓴 `D:\Downloads\ubuntu-26.04-*.iso`가 이 호스트에 더는 없다. route smoke는 ISO를 Gen2 VM의 DVD로 붙여 create/start/restart/shutdown route만 확인하고 OS 부팅을 요구하지 않는다. 그래서 개인 파일 대신 README 하나만 든 ISO 9660 이미지(`45056` byte, SHA-256 `43ea387d…`, Windows `StorageType=ISO`)를 `artifacts/smoke-media-20261003/`에 만들어 썼다.

## 실행 결과

gate는 PR #28 branch의 clean HEAD `8ade930`(product payload는 `8d940da`와 같음)에서 빌드했다. 0.42.86 manifest에서 version, batch id, 경로, ISO만 바꿨다. 먼저 supervisor `-DryRun -AllowHostMutation`으로 두 step이 계획되는지 확인했다(`dry_run=true`, `ok=true`). 실행은 `2026-10-03T07:25:42Z`부터 약 212초다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `200.054s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.079s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. route-parity summary, MSI lifecycle, OS mutation summary가 `ok=true`다. boot time은 바뀌지 않았고 남은 `pcv-*` VM은 없다.

| phase | exit |
| --- | ---: |
| Install (같은 version major upgrade) | `0` |
| Repair | `0` |
| Uninstall (preserve) | `0` |
| InstallRemoveData | `0` |
| UninstallRemoveData (`REMOVE_DATA=1`) | `0` |
| final restore Install | `0` |

## 같은 version 재설치(A) 실증

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{5EF95755-…}` 기록, `same_version_upgrade_expected=true`, 차단 없음 |
| install log | `FindRelatedProducts`가 `WIX_UPGRADE_DETECTED`=`{5EF95755-…}`를 설정하고 `RemoveExistingProducts`가 실행됨 |
| 네 MSI log의 `another client exists` | `0`건 (install, uninstall-preserve, uninstall-remove-data, final-restore-install) |
| 네 MSI log의 `Won't Overwrite` | `0`건 |
| 사후 build commit 검사 | `ok=true`, 설치본 `8ade930…` == gate build `8ade930…` |
| 사후 같은 version ARP 검사 | `ok=true`, `{F1D32C79-B11D-4C26-BD82-0E48A44CCC58}` 1개 |

2026-09-26~27 `0.42.78`에서 같은 version 재실행이 ARP 항목을 쌓고 첫 build 바이너리를 남겼던 문제(`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-09-27-04278-hostmutation.md`)가 이 MSI에서는 일어나지 않았다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{F1D32C79-B11D-4C26-BD82-0E48A44CCC58}` `0.42.87` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`c97c62bd…` / `5374c249…`) |
| 설치본 Host ProductVersion | `0.42.87-admin-smoke+8ade930…` |
| product manifest | `0.42.87-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |
| protected token 파일 | 있음 |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.86-admin-smoke`다. 이 gate는 current를 바꾸지 않는다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다. guest OS 동작은 이 gate가 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.

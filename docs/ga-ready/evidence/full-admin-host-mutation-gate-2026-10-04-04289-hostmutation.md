# Full admin host mutation gate `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `full-admin-host-mutation-gate-2026-10-04-04289-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.89-admin-smoke`
release_train: `0.42.89-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261004-04289`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261004-04289`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261004-04289.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261004-04289`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261004-04289`
batch_summary_sha256: `0b8df9a83d6a6fa2d286fd058ab25b97d73261cabb6e95705cda8265c5cabd3b`
routeparity_summary_sha256: `beb119c895ab73026d2903649b846fdbc430324fdad81e7724eef7b31cab128d`
os_summary_sha256: `63882facccd38259558bf28c0b6d694547a6e5321361593b7c4e4544335d7dcd`
operational_fullgate_msi_sha256: `fe5677ff46e1bf23acf3638afd01b4f9a0814bd5bd39f8c81e62b1992fa843a2`
operational_fullgate_payload_aggregate_sha256: `e4f9387cf15e3be31816cf9db844fd9c50a50e5217aa1f37143d18bbc6ea388f`
service_host_sha256: `760434509f2303fdb533cb34d773d2d4ea0cbd8a36bfa0805edecc1f85e6a5e7`
cli_sha256: `de6deffddd28928a8c8458b03c3aa42541b16e8acd0dee3f6070131585837f58`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.89-admin-smoke+a780928ee41f6064cbeebec754eca647dfff42f1`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `a780928ee41f6064cbeebec754eca647dfff42f1`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

Burn이 복구한 clean package `0.42.89`(`{0E27390F-5A08-4F76-90FD-7694B41672A4}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. 보존 VM Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `train/04289-20261004`의 clean HEAD `a780928`에서 빌드했다(product source는 `d6711f3`와 같고, 그 뒤 commit은 문서뿐이다). 0.42.88 manifest에서 version, batch id, 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-04T02:21:48Z`부터 약 329초 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `315.954s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.137s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{0E27390F-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{0E27390F-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `a780928` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{CD234EF2-AF9A-4E62-9873-3680A14CD5C1}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.89`로 VM `pcv-spike-api-0113cbfb`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-0113cbfb` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{CD234EF2-AF9A-4E62-9873-3680A14CD5C1}` `0.42.89` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`76043450…` / `de6deffd…`) |
| 설치본 Host ProductVersion | `0.42.89-admin-smoke+a780928…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.88-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

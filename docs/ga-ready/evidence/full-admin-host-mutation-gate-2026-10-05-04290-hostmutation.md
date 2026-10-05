# Full admin host mutation gate `0.42.90-admin-smoke` (2026-10-05)

evidence_id: `full-admin-host-mutation-gate-2026-10-05-04290-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.90-admin-smoke`
release_train: `0.42.90-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261005-04290`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261005-04290`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261005-04290.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261005-04290`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261005-04290`
batch_summary_sha256: `ea8bd609fc96bb56fa526583726ac2991c983ac62918f174f9010e17d11f2aac`
routeparity_summary_sha256: `15f43d1fe6ee5c71e92d617832b0d49518b39890109661769f93ab3b4d5e21e1`
os_summary_sha256: `ce20683d915be0e3e1f633c2341be7c66b71b0fdf0e18ca61b4d5c50533a6928`
operational_fullgate_msi_sha256: `ac367ea4c244aa574967963a822fe8c38e35407b371c7d6ae1caf299a690d153`
operational_fullgate_payload_aggregate_sha256: `9031c5605255521f7600f06e0fecbfb57876e7b33a8b5f17139138b6115af58c`
service_host_sha256: `b4884616b66fad292ec888b2e7fbb4eb11608e6a52dd7e2bc784293a64fec937`
cli_sha256: `39a1d1d410cca0b70f780d4c99f2f26aaca5df9a46fe9927f2c9c83dc96da436`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.90-admin-smoke+648139df9f03d37b3e3e036e995f701c5d7c32a3`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `648139df9f03d37b3e3e036e995f701c5d7c32a3`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.90`(`{8AE1F6ED-2174-4484-A57B-E60378BB1F72}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `train/04290-20261005`의 clean HEAD `648139d`에서 빌드했다(product source는 `a66a8cd`와 같고, 그 뒤 commit은 문서뿐이다). 3b-r2 manifest에서 version, batch id, 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-05T12:16:36Z`부터 약 573초 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `561.124s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.114s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{8AE1F6ED-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{8AE1F6ED-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `648139d` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{C6B0372F-7659-458E-AED3-A34334ACBDDF}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.90`로 VM `pcv-spike-api-e55b8761`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-e55b8761` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{C6B0372F-7659-458E-AED3-A34334ACBDDF}` `0.42.90` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`b4884616…` / `39a1d1d4…`) |
| 설치본 Host ProductVersion | `0.42.90-admin-smoke+648139d…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.89-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

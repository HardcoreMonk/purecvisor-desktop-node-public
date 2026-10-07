# Full admin host mutation gate `0.42.92-admin-smoke` (2026-10-07)

evidence_id: `full-admin-host-mutation-gate-2026-10-07-04292-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.92-admin-smoke`
release_train: `0.42.92-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261007-04292`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261007-04292`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261007-04292.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261007-04292`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261007-04292`
batch_summary_sha256: `47d51282097cebb9aec8293b2e0fdfcab98770dbbb12494c5cf8a08ed7ca7666`
routeparity_summary_sha256: `62f52e052b3dda1add907237924b79f25cad2037ba8c7d91676b250259c0383f`
os_summary_sha256: `b32554dd616117df2e41e6b4fe313a88aeffec95c858f5fd3aa2c063b37ac099`
operational_fullgate_msi_sha256: `67b257a3bce6b31936458aed5cc5cf99926731ca8320e5bbdfaeee3a88685080`
operational_fullgate_payload_aggregate_sha256: `dda8eb7f4bb535aca7aa8a8b411c6f5cf00311972d7cc388d1cbb1000a3c44d1`
service_host_sha256: `494d567b6ead5bf7444bb1c4af04a50f7fef56bad2fbd5921a5876b8523cbc98`
cli_sha256: `36d93aaeb21aead75a8064566f647337ee8c07ce7bbc9a6acc6831bff3d519b9`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.92-admin-smoke+b51b8cf804288121abcd8ea3724714cd7f9fc9f6`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `b51b8cf804288121abcd8ea3724714cd7f9fc9f6`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.92`(`{0FDC43BE-E4C0-4D10-A574-781A9F060D4C}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `train/04292-20261007`의 clean HEAD `b51b8cf`에서 빌드했다(product source는 payload `ad8b5c2`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 만들었다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-07T15:01:01Z`부터 약 236초 실행했다. `os-mutation-gate`는 2026-10-07 사용자 추가 승인 `1` 뒤 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `224.051s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.120s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{0FDC43BE-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{0FDC43BE-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `b51b8cf` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{7D82F2ED-575E-41B2-89E8-BE2B51976E4C}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.92`로 VM `pcv-spike-api-92a349b0`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-92a349b0` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{7D82F2ED-575E-41B2-89E8-BE2B51976E4C}` `0.42.92` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`494d567b…` / `36d93aae…`) |
| 설치본 Host ProductVersion | `0.42.92-admin-smoke+b51b8cf…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.91-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

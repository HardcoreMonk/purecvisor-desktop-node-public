# Full admin host mutation gate `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `full-admin-host-mutation-gate-2026-10-06-04291-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.91-admin-smoke`
release_train: `0.42.91-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261006-04291`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261006-04291`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261006-04291.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261006-04291`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261006-04291`
batch_summary_sha256: `6255be3e439eee797de161133bd7b2c0f7783d7503b169cf101fc3f6e5177fac`
routeparity_summary_sha256: `78b28fd07c98a17e2c57060d5366e00e4035dbee312992a5ca7f8220322798ef`
os_summary_sha256: `4c74e0f2e6bf1dac1811d2b9e520488af9740886ec000e41e5e4eaff0ef301b0`
operational_fullgate_msi_sha256: `46dddccb669f75cf761ae2b52d9b2139c9df3f7f48e1c64a20f5027293cbb85b`
operational_fullgate_payload_aggregate_sha256: `c79dff87d7ec3f44e3b701d9918745cccdd74ae9d7c550ea829512e76a4003c0`
service_host_sha256: `a935701e623ef938576c71559c7e86d63acc647ff608e5dd183516397e65775f`
cli_sha256: `2c249f0dd483de1a2954ba4bb2660bf175a93c4e965ef6684290098070cb99e6`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.91-admin-smoke+990a4b2713f6d51dca416b476308a0bf92296155`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `990a4b2713f6d51dca416b476308a0bf92296155`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.91`(`{AC7292AD-3E1D-4512-BF63-996F831EF90B}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `train/04291-20261006`의 clean HEAD `990a4b2`에서 빌드했다(product source는 `05f42a2`와 같고, 그 뒤 commit은 문서뿐이다). 0.42.90 manifest에서 version, batch id, 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-06T10:47:05Z`부터 약 815초 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `802.759s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.146s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{AC7292AD-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{AC7292AD-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `990a4b2` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{ED64B13A-742B-422C-9142-DED650CB856B}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.91`로 VM `pcv-spike-api-e05831c2`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-e05831c2` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{ED64B13A-742B-422C-9142-DED650CB856B}` `0.42.91` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`a935701e…` / `2c249f0d…`) |
| 설치본 Host ProductVersion | `0.42.91-admin-smoke+990a4b2…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.90-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

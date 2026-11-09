# Full admin host mutation gate `0.43.0-admin-smoke` (2026-11-10)

evidence_id: `full-admin-host-mutation-gate-2026-11-10-04300-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.0-admin-smoke`
release_train: `0.43.0-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261110-04300`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261110-04300`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261110-04300.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261110-04300`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261110-04300`
batch_summary_sha256: `b1a441cfb6414475565c881ad1bfccb00cc4ba0e69781db8d6a056d4e3e38f24`
routeparity_summary_sha256: `8b4533673288614293b860379c7d548aff3a7adbdf2ee4eb2ae27521124f36ab`
os_summary_sha256: `f5469a13019b3c47895a642bf29d3a664e45868b92e439f94c74ff2ceadf5ddc`
operational_fullgate_msi_sha256: `32d7ec56c3bce2ef78fa28e8db775b8ee0a83458d3ade91037dbf9c0060f6bd8`
operational_fullgate_payload_aggregate_sha256: `0bb34c5c907d6c677aaa909d0742e283f58a3b9f1a3a2a81f6b0afe602fcb438`
service_host_sha256: `5f0b34a3236a586df85568d4fca684cf93ecd0c6ef3235ba322c4dd5910e4fe9`
cli_sha256: `9b8d20cfbdba5edc261ff76bc06b033042ee4335223f4abcdcb42f06df8e8e20`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.43.0-admin-smoke+1f646ee010daa175cbe82ff70ad79def5335ba15`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `1f646ee010daa175cbe82ff70ad79def5335ba15`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.43.0`(`{C5E6067A-783A-4468-90A1-D8AB232814B7}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04300-20261110`의 HEAD `1f646ee`에서 빌드했다(product source는 payload `6355e6ba`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.43.0-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 기본 게이트웨이가 있는 호스트 사설 IPv4로 실행 경계에서 만들어 환경 변수로만 넘겼다. 설치본은 pair 뒤의 `0.43.0`(ARP 1개)였고, 계정은 Task 2에서 data root 밖에 보관했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `735.316s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.146s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{C5E6067A-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{C5E6067A-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `1f646ee` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{C948BEA2-65D7-4047-81F3-FF4830E4BFC3}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.43.0`로 VM `pcv-spike-api-e157b200`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-e157b200` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{C948BEA2-65D7-4047-81F3-FF4830E4BFC3}` `0.43.0` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`5f0b34a3…` / `9b8d20cf…`) |
| 설치본 Host ProductVersion | `0.43.0-admin-smoke+1f646ee…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template)는 그대로 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.99-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

# Full admin host mutation gate `0.43.6-admin-smoke` (2026-11-11)

evidence_id: `full-admin-host-mutation-gate-2026-11-11-04306-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.6-admin-smoke`
release_train: `0.43.6-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261111-04306`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261111-04306`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04306.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261111-04306`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261111-04306`
batch_summary_sha256: `e1271a943ace887d8201133002bf2273f95dcae11f9b446c8416d856f1ae74bb`
routeparity_summary_sha256: `2db3a787d6be735dbf0adadc303cb45800e2a393ea76a1292b516e88b7c9c3fa`
os_summary_sha256: `876aeb053ef04f3714579690c03947459af24ae114e8ae67799654f0e2b41d3c`
operational_fullgate_msi_sha256: `8c87b46a3127f60c126af91f7ab4d0425c1a7fc2f233e9917280d427be5415ab`
operational_fullgate_payload_aggregate_sha256: `070b3a12a2c528d9a051eda9f1ec83373d093030ec88ca8067846146bd796662`
service_host_sha256: `111fb3795538eb2d8a004f3485b1278a796c2105a0c84ce170fc16bf5cc9fec2`
cli_sha256: `a21f826c9dd111cc0b4b09da7a4d163a60cc7b0f067a43d5cdcfdbc3c6444d63`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.43.6-admin-smoke+bfb30c11d747a89f9d41f4a963d52a279f5ba231`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `bfb30c11d747a89f9d41f4a963d52a279f5ba231`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.43.6`(`{DA8E27F5-B817-4510-898A-DB9EF5304DCD}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04306-20261111`의 HEAD `bfb30c1`에서 실행했다(product source는 payload `ee821cbd`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.43.6-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 직전 train manifest의 값을 이 호스트의 Up 어댑터 주소인지 확인한 뒤 실행 경계에서 환경 변수로만 넘겼다. 설치본은 pair 직후 `0.43.6`(ARP 1개)였고, 계정은 Task 2에서 data root 밖에 보관했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `382.776s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.120s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{DA8E27F5-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{DA8E27F5-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `bfb30c1` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{9BDBAE31-2AA7-4643-BCC1-FACB39CD5861}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.43.6`로 VM `pcv-spike-api-754448a5`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-754448a5` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{9BDBAE31-2AA7-4643-BCC1-FACB39CD5861}` `0.43.6` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`111fb379…` / `a21f826c…`) |
| 설치본 Host ProductVersion | `0.43.6-admin-smoke+bfb30c1…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template), `pcv-it-s1r-ubuntu` Running(직전 S1 시도 VM)로 그대로다 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.4-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

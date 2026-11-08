# Full admin host mutation gate `0.42.98-admin-smoke` (2026-11-08)

evidence_id: `full-admin-host-mutation-gate-2026-11-08-04298-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.98-admin-smoke`
release_train: `0.42.98-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261108-04298`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261108-04298`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261108-04298.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261108-04298`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261108-04298`
batch_summary_sha256: `32117065307a2ec0767a782a563196c65e59fbf4464331512c35a76e4070031e`
routeparity_summary_sha256: `539d9d6e04d1b204493864be6f22e91692bef5e8980ab6672f2d246a000f2f67`
os_summary_sha256: `0bfc0358dd443ffab5b89aef745fec2587560ce3a9e3102660ea643218ea498e`
operational_fullgate_msi_sha256: `0a42b89f4ababb0066e4d2a326a8019ae1a2780ee079b11c4809c999d0d93680`
operational_fullgate_payload_aggregate_sha256: `963f4f746b0fc9d1c80209861bc2a6317ff08cb5028b344acd589685bbb67444`
service_host_sha256: `501b41f7304fca9831f1fbde9b6b34edb81aeadf623eb1ff61e85b0c420d6cc0`
cli_sha256: `bfab42fdc3ae0389afd69ec7aa403c2e846438ff138c1a378bd76f7fbd74506a`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.42.98-admin-smoke+2dfe2ac7e1af85a51b676127f83d264a0b57d840`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `2dfe2ac7e1af85a51b676127f83d264a0b57d840`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.98`(`{625D101D-B310-4820-AA39-1AB4352C8E05}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04298-20261108`의 HEAD `2dfe2ac`에서 빌드했다(product source는 payload `8e4ebe2`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.42.98-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 S4 상태 보관 때 저장소 밖에 백업한 service 명령줄에서 읽어 환경 변수로만 넘겼다(재클론으로 직전 manifest artifact가 없음). 설치본은 pair 뒤의 `0.42.98`(ARP 1개)이었다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `358.767s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.102s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{625D101D-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{625D101D-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `2dfe2ac` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{5673340D-9C34-4CD0-99F0-28A5DDC60B57}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.98`로 VM `pcv-spike-api-303644f4`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-303644f4` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{5673340D-9C34-4CD0-99F0-28A5DDC60B57}` `0.42.98` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`501b41f7…` / `bfab42fd…`) |
| 설치본 Host ProductVersion | `0.42.98-admin-smoke+2dfe2ac…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template), `pcv-it-s4-console` Running(S4 시연 VM)은 그대로 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.96-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

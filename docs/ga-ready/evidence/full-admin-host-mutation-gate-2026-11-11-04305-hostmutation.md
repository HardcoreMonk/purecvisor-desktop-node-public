# Full admin host mutation gate `0.43.5-admin-smoke` (2026-11-11)

evidence_id: `full-admin-host-mutation-gate-2026-11-11-04305-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.5-admin-smoke`
release_train: `0.43.5-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261111-04305`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261111-04305`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04305.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261111-04305`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261111-04305`
batch_summary_sha256: `5d424dc04cc503b935eb5a9fef472d5fee548ad9b40716d0acff47d56866a995`
routeparity_summary_sha256: `59c92c946bf2cadb24f5e8d792390edd54be99c6f670f8622416b4b55fb639c5`
os_summary_sha256: `3e7b5f60ff935878783d454dc56553d07cf9b6cbc34165f0369e8549dc200754`
operational_fullgate_msi_sha256: `dd77d8713337b791e763a7bfb48be085f6576cbe642e3233a63c99d34ce38c11`
operational_fullgate_payload_aggregate_sha256: `e45b2e74954af951cabfaa30147d75f620819cc137da64cb63476beb0e686da3`
service_host_sha256: `705b2e9eebe7fce90cc132caedef9b99fe547a3ff8e39543537f74fc43185494`
cli_sha256: `b6fccfc886254e5d3e8feb0dbc3beb49b41194df24d57532f7d8ddc6ec28fe97`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.43.5-admin-smoke+8173777e208d43fd1097af53f26b2213fdd98b07`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `8173777e208d43fd1097af53f26b2213fdd98b07`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.43.5`(`{E5963E51-A9BD-489E-A4C9-C6C370922F9C}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04305-20261111`의 HEAD `8173777`에서 실행했다(product source는 payload `91009557`과 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.43.5-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 직전 train manifest의 값을 이 호스트의 Up 어댑터 주소인지 확인한 뒤 실행 경계에서 환경 변수로만 넘겼다. 설치본은 pair 직후 `0.43.5`(ARP 1개)였고, 계정은 Task 2에서 data root 밖에 보관했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `389.151s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.086s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{E5963E51-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{E5963E51-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `8173777` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{BF412A3E-4EE6-4A2E-B18F-0AA6FE426522}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.43.5`로 VM `pcv-spike-api-6a9553ea`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-6a9553ea` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{BF412A3E-4EE6-4A2E-B18F-0AA6FE426522}` `0.43.5` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`705b2e9e…` / `b6fccfc8…`) |
| 설치본 Host ProductVersion | `0.43.5-admin-smoke+8173777…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template), `pcv-it-s1r-ubuntu` Running(직전 campaign S1 시도 VM)로 그대로다 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.4-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

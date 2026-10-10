# Full admin host mutation gate `0.42.95-admin-smoke` (2026-10-10)

evidence_id: `full-admin-host-mutation-gate-2026-10-10-04295-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.95-admin-smoke`
release_train: `0.42.95-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261010-04295`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261010-04295`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261010-04295.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261010-04295`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261010-04295`
batch_summary_sha256: `c3c0d7d8ca1b8446a3ba94609618f8647bd3ae08d6a75a44e2aa1f88a0e64bbc`
routeparity_summary_sha256: `42904b3ff36c3316a4832e8dedaf27fe34380b4b68d3545337aa44c788a9a918`
os_summary_sha256: `4582e64df50f4bb883831d3ff876838193c5e86843e8d9d2fddf768045f401c6`
operational_fullgate_msi_sha256: `c1acc538b988e6a7be73722340330e2faca8ecda46b2c234d888a661bd467b52`
operational_fullgate_payload_aggregate_sha256: `132cb887cb9ca98187d8a75b652424a32af072b9cf8c2b43ad6d9db8e9522f4b`
service_host_sha256: `a397b758b02be33e2260d1e06e4a930bb28eb9e52ccbc94d449b4b76291bf600`
cli_sha256: `fb5392ec356bfdfa7e7f72cc3c45300f69be3afcb55c7b271b7dc6ac31331c85`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.95-admin-smoke+b9898cf7a125e4dadbc4b29ae235f46d8c914a01`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `b9898cf7a125e4dadbc4b29ae235f46d8c914a01`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.95`(`{25BE6564-240F-4ECE-94DC-26998143C40C}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04295-20261010`의 HEAD `b9898cf`에서 빌드했다(product source는 payload `f84d46a`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`가 `docs/ga-ready/trains/0.42.95-admin-smoke.host-inputs.json`에서 만들었고 LAN prefix는 직전 manifest에서 읽어 환경 변수로만 넘겼다. 설치본은 pair가 남긴 `0.42.95-admin-smoke+b401d94`(ARP 1개)에서 시작했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `279.174s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.099s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{25BE6564-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{25BE6564-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `b9898cf` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{F5203439-7074-4F2C-8F20-0D9571420514}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.95`로 VM `pcv-spike-api-d9091bf9`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-d9091bf9` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{F5203439-7074-4F2C-8F20-0D9571420514}` `0.42.95` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`a397b758…` / `fb5392ec…`) |
| 설치본 Host ProductVersion | `0.42.95-admin-smoke+b9898cf…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template)만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.93-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

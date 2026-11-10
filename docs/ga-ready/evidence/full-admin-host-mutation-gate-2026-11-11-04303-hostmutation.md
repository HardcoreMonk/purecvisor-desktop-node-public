# Full admin host mutation gate `0.43.3-admin-smoke` (2026-11-11)

evidence_id: `full-admin-host-mutation-gate-2026-11-11-04303-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.3-admin-smoke`
release_train: `0.43.3-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261111-04303`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261111-04303`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04303.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261111-04303`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261111-04303`
batch_summary_sha256: `36e086ac727ce499dd1717acbe67d36bc48a604bec29bc8f81300b62539a9b18`
routeparity_summary_sha256: `1ef3b0ff6fdf8f36b07af94a73f6989099642307ac04fee30244e114531090b3`
os_summary_sha256: `7d4c2c160f97d677bbcc2845620d9c37755e2722f8950fb0fb59821b656f0a00`
operational_fullgate_msi_sha256: `13f716ee18fbbcee0e0322de6cd12895762fb665863a1e7b437ff4bd292d0b59`
operational_fullgate_payload_aggregate_sha256: `0ea79bc712613d9fd5ce52e0571ef23cd721c8e821fdd9882598858ba3f1f018`
service_host_sha256: `6135e9a0d13286f9d0a4608a4507fd06f6a6927f235f9f18929c82a34e04144d`
cli_sha256: `e590f87f95be448e8c187fac336f99c5a268c18167c16cbee688302e222363a0`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.43.3-admin-smoke+db1a7626b7654264d8fa5059685027d064098445`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `db1a7626b7654264d8fa5059685027d064098445`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.43.3`(`{F52E5B60-1B4E-4183-A000-D98A8B576277}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04303-20261111`의 HEAD `db1a762`에서 빌드했다(product source는 payload `69675e0c`과 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.43.3-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 기본 게이트웨이가 있는 호스트 사설 IPv4로 실행 경계에서 만들어 환경 변수로만 넘겼다. 설치본은 pair 뒤의 `0.43.3`(ARP 1개)였고, 계정은 Task 2에서 data root 밖에 보관했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `395.461s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.100s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{F52E5B60-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{F52E5B60-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `db1a762` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{07427BC2-EEE3-4B8C-8DAA-8D958F883539}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.43.3`로 VM `pcv-spike-api-f21dfc44`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-f21dfc44` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{07427BC2-EEE3-4B8C-8DAA-8D958F883539}` `0.43.3` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`6135e9a0…` / `e590f87f…`) |
| 설치본 Host ProductVersion | `0.43.3-admin-smoke+db1a762…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template)는 그대로 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.2-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

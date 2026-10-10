# Full admin host mutation gate `0.42.96-admin-smoke` (2026-10-11)

evidence_id: `full-admin-host-mutation-gate-2026-10-11-04296-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.96-admin-smoke`
release_train: `0.42.96-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261011-04296`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261011-04296`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261011-04296.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261011-04296`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261011-04296`
batch_summary_sha256: `fe5bd7b8e96527fd7187ea27d99bee58bf05beea0a63cc023df0adb79b44f843`
routeparity_summary_sha256: `9023bcc0ad4359b3a6e7466c48ae6370e0e666f6ec509d6a8f9033ed573cec33`
os_summary_sha256: `fc04b02f976cabb5589da45e26f854a2c36329e840d2abb515631fbe462141cb`
operational_fullgate_msi_sha256: `37bfc1ffd045950e3c4bfa1fefd4db6a96a0490288dc647fecc6cb1e1137d299`
operational_fullgate_payload_aggregate_sha256: `8dc6df5754616748d83b5728cd5f38b0a5788c47e44d1d8f46e31fca4dfe4cd5`
service_host_sha256: `b52629d9febe1ab42b80b167cec8a4df8f203f90ff486c531f1161bfe071cb78`
cli_sha256: `7ad43a47423b6de5750e524eff854d162067745a22b044cb238944d07a5cfa6c`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.96-admin-smoke+9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.96`(`{EBCDF75B-F5A7-49D1-92BE-84F442F67E99}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04296-20261011`의 HEAD `9ac8eb3`에서 빌드했다(product source는 payload와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`가 `docs/ga-ready/trains/0.42.96-admin-smoke.host-inputs.json`에서 만들었고 LAN prefix는 직전 manifest에서 읽어 환경 변수로만 넘겼다. 설치본은 pair가 남긴 clean `0.42.96`(ARP 1개)에서 시작했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `309.723s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.095s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{EBCDF75B-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{EBCDF75B-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `9ac8eb3` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{E85C21A8-7701-4658-B963-A9F52DF04292}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.96`로 VM `pcv-spike-api-1c89406b`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-1c89406b` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{E85C21A8-7701-4658-B963-A9F52DF04292}` `0.42.96` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`b52629d9…` / `7ad43a47…`) |
| 설치본 Host ProductVersion | `0.42.96-admin-smoke+9ac8eb3…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template)만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.93-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

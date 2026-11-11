# Full admin host mutation gate `0.43.7-admin-smoke` (2026-11-11)

evidence_id: `full-admin-host-mutation-gate-2026-11-11-04307-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.7-admin-smoke`
release_train: `0.43.7-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261111-04307`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261111-04307`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261111-04307.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261111-04307`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261111-04307`
batch_summary_sha256: `0c1d41aa37351ad751aea37d77cf2607ee1db8d7ff70ea515f7934bf8cdcf32a`
routeparity_summary_sha256: `6a59ed2bdeef5a32e8dc6b21c3df3b50c8ec68daffb044bfebf311311ba3c0ac`
os_summary_sha256: `b09d5e8e532ea5dac72198644d915e1f1115ae3235a165fe972f2f9fbae67eb6`
operational_fullgate_msi_sha256: `e6a893674bd50a12b2a619fab7e50bcb3fc29ab7b000dc6eb845cca59fb766ea`
operational_fullgate_payload_aggregate_sha256: `a2dba459e72f1bc2263707c3855ad5c1b6ab5e7c0d4f67db9780224c2586cbf2`
service_host_sha256: `a9a72baa4494300007188d64b1107c39b8135e7ea32278a70435a23b7021ca90`
cli_sha256: `f6e851fc9b0a3c5c9ba22183ef8bf888ca79844b73dd1c22ae3487c6be117fd8`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
installed_product_version: `0.43.7-admin-smoke+38981398807685e96fcdb8777527573fa97542cc`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `38981398807685e96fcdb8777527573fa97542cc`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.43.7`(`{1C067086-B542-4932-93AC-F05B6DF50D48}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/train-04307-20261111`의 HEAD `3898139`에서 빌드했다(product source는 payload `bf5c214`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 `docs/ga-ready/trains/0.43.7-admin-smoke.host-inputs.json`에서 만들었다. LAN prefix는 직전 train manifest의 값이 이 호스트의 Up 어댑터 주소임을 확인한 뒤 실행 경계에서 환경 변수로만 넘겼다. 설치본은 pair 끝의 `0.43.7`(ARP 1개)였고, 계정은 Task 2에서 data root 밖에 보관했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `394.949s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.120s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{1C067086-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{1C067086-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `3898139` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{028F1BBE-CCCE-41BE-A859-8AE7CFD9D7A6}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.43.7`로 VM `pcv-spike-api-42f116f4`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-42f116f4` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{028F1BBE-CCCE-41BE-A859-8AE7CFD9D7A6}` `0.43.7` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`a9a72baa…` / `f6e851fc…`) |
| 설치본 Host ProductVersion | `0.43.7-admin-smoke+3898139…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(S2 template), `pcv-it-s1r-ubuntu` Running(S1 시도 VM)는 그대로다 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.43.4-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

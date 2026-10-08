# Full admin host mutation gate `0.42.93-admin-smoke` (2026-10-08)

evidence_id: `full-admin-host-mutation-gate-2026-10-08-04293-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.93-admin-smoke`
release_train: `0.42.93-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261008-04293`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261008-04293`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261008-04293.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261008-04293`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261008-04293`
batch_summary_sha256: `31eb1374348447a1783472aa7b0fd9ab61b948f559c664e35413d390c6a5fb52`
routeparity_summary_sha256: `1e1f678de48e8af9007102c2dd8505a79a534b86ad3a4dd35006b61291192fd0`
os_summary_sha256: `46319a01ba3d46617256efa7d54284a3011b91f68f84fd1c4dd4db1ad0692d61`
operational_fullgate_msi_sha256: `5d5a7c7c6086c591bae0b2609c8e44f5ae94e84e3fd70584f7b3f994456864dd`
operational_fullgate_payload_aggregate_sha256: `df1a8cacade8db1ccf644a4a9a83852f2cc731c3d50231389758f6b78bf0d29e`
service_host_sha256: `9f537217caf49818ce254ae419ebbad57f8ce578ed78c27a77f704ebf9348b49`
cli_sha256: `df02f453118c6f59102d3b563de86ec166f71dd9ef1190e4601aea6b2425ad76`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.93-admin-smoke+818d00f113b3d3eaf1573652cd4c2d86a551834d`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `818d00f113b3d3eaf1573652cd4c2d86a551834d`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

pair가 남긴 clean package `0.42.93`(`{867BA47E-0553-4B74-8E46-8C803008F61C}`, ARP 1개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM `pcv-guest-installed-04253-r1` Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane1/completion-20261008`의 HEAD `818d00f`에서 빌드했다(product source는 payload `56e7cd0`와 같고, 그 뒤 commit은 문서뿐이다). manifest는 `pcvverify train-host-inputs --kind fullgate-manifest`로 만들었다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-08T06:20:25Z`부터 약 268초 실행했다. `os-mutation-gate`는 이 campaign의 승인 범위에 들어 있어 함께 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `255.511s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.079s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{867BA47E-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{867BA47E-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `818d00f` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{70A3822D-89D0-4E0D-A60B-892CE8CDC776}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.93`로 VM `pcv-spike-api-95e7522e`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-95e7522e` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{70A3822D-89D0-4E0D-A60B-892CE8CDC776}` `0.42.93` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`9f537217…` / `df02f453…`) |
| 설치본 Host ProductVersion | `0.42.93-admin-smoke+818d00f…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.92-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

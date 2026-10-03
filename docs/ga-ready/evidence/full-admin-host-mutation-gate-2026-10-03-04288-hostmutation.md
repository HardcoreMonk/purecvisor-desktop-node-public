# Full admin host mutation gate `0.42.88-admin-smoke` (2026-10-03)

evidence_id: `full-admin-host-mutation-gate-2026-10-03-04288-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.88-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261003-04288`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261003-04288`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261003-04288.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261003-04288`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261003-04288`
batch_summary_sha256: `21bb35d9713eb69bd012c774ebe0f2a50ff6d1c35c4129dea033703aa6a7e688`
routeparity_summary_sha256: `caf7b3cf86ccde528bfdfd082512babd233d456f63fdb85f117a7c09ea3eddb4`
os_summary_sha256: `a242073a58d2846b2b97a806ad6ab37faedc3e40b01894d963a75df5145ff997`
operational_fullgate_msi_sha256: `32b35113e00ffbe59cd503d1b83ae5b3b62028bd70a5fcddd2100de6e03cd2c6`
operational_fullgate_payload_aggregate_sha256: `163ece95759af368a5fdccf957d3791af3d6301224be434169c347719e1cd9af`
service_host_sha256: `1df96c7f67bffaca4b6f9462256bad858c9ad3e3e94228ce7c43466ff34fe97a`
cli_sha256: `763db7e2dabf73d76f1228dd0282c5d40dc0e232c5c4d0cfcce9e8ffcd27cadd`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.88-admin-smoke+47ff198de86d77d25aa90fa095a294254c7ffef6`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_before: `1`
arp_entry_count_after: `1`
same_version_major_upgrade_observed: `true`
managed_delete_storage_cleanup_observed: `true`
provenance_commit: `47ff198de86d77d25aa90fa095a294254c7ffef6`
signing_mode: `AllowUnsignedDev`
iso_path: `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 상태

Burn이 복구한 clean package `0.42.88`(`{5B28DA82-3E76-4FE6-A11C-97B38A18F487}`, ARP 1개)이 설치된 채로 시작했다. 0.42.87부터 들어간 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. 보존 VM Off, `PureCVisor` firewall rule `0`.

## 실행 결과

gate는 branch `lane2/04288-package-pair-20261003`의 clean HEAD `47ff198`(product payload는 `ed4a4fa`와 같음)에서 빌드했다. 0.42.87 manifest에서 version, batch id, 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation` 확인 뒤 `2026-10-03T11:54:39Z`부터 약 273초 실행했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `260.796s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.073s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. MSI lifecycle 6 phase(Install, Repair, Uninstall preserve, InstallRemoveData, UninstallRemoveData, final restore Install)가 모두 exit `0`이고 boot time은 바뀌지 않았다.

| 관측 | 값 |
| --- | --- |
| `same-version-preflight` | 잔여 `{5B28DA82-…}` 기록, `same_version_upgrade_expected=true` |
| install log | `WIX_UPGRADE_DETECTED`=`{5B28DA82-…}` |
| 네 MSI log의 `another client exists` / `Won't Overwrite` | `0` / `0` |
| 사후 build commit 검사 | `ok=true`, `47ff198` == gate build |
| 사후 같은 version ARP 검사 | `ok=true`, `{3F60088B-3F8D-483F-8B98-0B56DE01A012}` 1개 |

## managed delete 디스크 정리 (gate 안 관측)

route smoke는 설치본 `0.42.88`로 VM `pcv-spike-api-61792db8`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.

| 필드 | 값 |
| --- | --- |
| `configuration_root` | `<user-temp>\pcv-hyperv-api-smoke\pcv-spike-api-61792db8` |
| `removed_files` | `disk0.vhdx` |
| `removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `retained` | 없음 |

smoke 자신의 cleanup은 지울 경로를 찾지 못했다(`removed_path=false`). `pcv-hyperv-api-smoke` 디렉터리는 비어 있다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{3F60088B-3F8D-483F-8B98-0B56DE01A012}` `0.42.88` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`1df96c7f…` / `763db7e2…`) |
| 설치본 Host ProductVersion | `0.42.88-admin-smoke+47ff198…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

## Nonclaims

- 내부 admin smoke다. MSI는 서명하지 않았다(`AllowUnsignedDev`).
- operational current는 `0.42.87-admin-smoke`다.
- route smoke VM은 OS를 부팅하지 않는 ISO로 만들었다.
- public trusted signing과 external stable publication을 주장하지 않는다.

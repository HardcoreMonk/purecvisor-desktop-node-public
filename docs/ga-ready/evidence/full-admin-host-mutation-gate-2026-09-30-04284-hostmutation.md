# Full admin host mutation gate `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `full-admin-host-mutation-gate-2026-09-30-04284-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.84-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260930-04284`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260930-04284`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20260930-04284.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260930-04284`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260930-04284`
preclean_root: `artifacts/fullgate-preclean-20260930-04284`
batch_summary_sha256: `fe07198cdf8ca5d297492fe6dd6c677e94bac1e06fd075ad037b1f8a8e9d48a4`
routeparity_summary_sha256: `35254fe3887da17ecf183a985389cd152ac284ea30e64911c7a748d27b8b80b0`
os_summary_sha256: `8bd55aa3cd5d49232cc0b78adffcc85bd66b0a89895bc6ecaa637f22fb18b772`
operational_fullgate_msi_sha256: `f9e1e341a3541d91feb63723e91bb4956be02d75fbad41dda3344b8f0e6a5482`
operational_fullgate_payload_aggregate_sha256: `77481bdbd86f6bce78dd47375022d0fc71f6a9a26cf26d3b26b46df691de4ae7`
service_host_sha256: `ec0f479e31dfd0415a927634653e299a3103122ae99082b7efa54c247711fb6d`
cli_sha256: `60d8922be4fa36ab95dde73c6be09c3ad3b739d523c52cac13aca97b50b74805`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_after: `1`
provenance_commit: `ee90e0ea2c042e21cd0bf0346440ae2db41dfe43`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 정리

같은 version의 ProductCode가 겹치지 않게 하려고 gate 전에 설치본을 비웠다. 그 전에 0.42.84를 다시 빌드한 적이 없어서 잔여 ProductCode는 없었다(ARP `0.42.84` 1개).

Burn task가 설치한 clean package `0.42.84`(`{F50C37FD-D2CB-465D-83B8-1693DE0B6772}`)를 데이터를 보존하는 `msiexec /x`로 제거했다. exit는 `0`이고 `REMOVE_DATA`는 쓰지 않았다. 제거 뒤 ARP 항목 `0`, service 없음, product root 없음을 확인했다. data root 최상위 파일 `11`개는 제거 전후 같은 목록이다. 첫 제거 시도는 PowerShell 인자 구문 오류로 `msiexec`가 시작되지 않았고, 호스트는 바뀌지 않았다.

## 실행 결과

gate HEAD는 `ee90e0e`(clean tree)다. 실행 시간은 2026-09-30 `06:18:13Z`부터 `06:21:52Z`까지다. manifest는 0.42.83 manifest에서 version, batch id, artifact 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation`으로 두 step이 계획되는 것을 먼저 확인했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `206.664s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.118s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`다. route-parity summary, `msi-lifecycle-smoke`, `hyperv-api-route-smoke`, OS mutation summary 모두 `ok=true`다.

MSI lifecycle(route-parity 안) 결과:

| phase | exit |
| --- | ---: |
| Install | `0` |
| Repair (`REINSTALL=ALL`) | `0` |
| Uninstall (preserve) | `0` |
| InstallRemoveData | `0` |
| UninstallRemoveData (`REMOVE_DATA=1`) | `0` |
| final restore Install | `0` |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{EE05403C-768A-4186-85CB-5A37D1AEF4B9}` `0.42.84` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`ec0f479e…` / `60d8922b…`) |
| 설치본 Host ProductVersion | `0.42.84-admin-smoke+ee90e0e…` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

data root 최상위 파일은 `11`개에서 `10`개가 됐다. 빠진 것은 `install.jsonl`이다. 이 파일은 gate의 `uninstall-remove-data`(`REMOVE_DATA=1`)가 설계대로 지우는 목록에 들어 있다. 0.42.83 gate와 같은 결과다.

gate의 operational MSI(`f9e1e341…`)는 pair target clean MSI(`12a582ef…`)와 identity가 다르다. 0.42.83과 같은 구조다.

## Nonclaims

- operational current는 `0.42.83-admin-smoke`다. 이번 campaign은 Lane 3 승격을 하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.

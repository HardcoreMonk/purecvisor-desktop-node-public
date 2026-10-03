# Full admin host mutation gate `0.42.86-admin-smoke` (2026-10-02)

evidence_id: `full-admin-host-mutation-gate-2026-10-02-04286-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.86-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261002-04286`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261002-04286`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261002-04286.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261002-04286`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261002-04286`
preclean_root: `artifacts/fullgate-preclean-20261002-04286`
batch_summary_sha256: `5b832dc0122e9ae11b61cc36c31892deae692f934eb58742f1110b823f11a902`
routeparity_summary_sha256: `a2b60a7e5eed2e76c7bb28d01c4b3ad98a4a0600b10fc8893894a2d3a82e194e`
os_summary_sha256: `11f0d6c5eb96405c55f737d6fa9a34b42305ca471866301c79e332e046d0a826`
operational_fullgate_msi_sha256: `85387f31b6892c5be37ec25b22f64d5a655f323e870e29812e65b40ad6ed8f3e`
operational_fullgate_payload_aggregate_sha256: `bba7e10c0970e580bb5d5f176aa93e942961aa9c89c5e446da8de66208f9187d`
service_host_sha256: `d886c632b8585968f4bd0526400c20216621c86895e88c118dd9d76345eba0ae`
cli_sha256: `93134bb8872a09b9100a82f20d81728c8a7c70794b032f5f81976ada4450f717`
product_wrapper_sha256: `086d491283f170558899cbce5e640c17e774186ed83b86d39a791ce4a7f4c1d5`
installed_product_version: `0.42.86-admin-smoke+b807803f778e29c206f1bb2ba8277d2a1136198f`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_after: `1`
provenance_commit: `b807803f778e29c206f1bb2ba8277d2a1136198f`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 정리

같은 version의 ProductCode가 겹치지 않도록 gate 전에 설치본을 비웠다. Burn이 복구한 clean package `0.42.86`(`{EE323D7C-2343-4110-958A-0453E5D1D39A}`)를 `msiexec /x`로 제거했다. 데이터는 보존했다(exit `0`, `REMOVE_DATA` 미사용). 제거 뒤 확인 결과:

- ARP 항목 `0`
- service 없음
- product root 없음
- protected token 파일은 남음
- 보존 VM `pcv-guest-installed-04253-r1`은 Off

제거 로그는 `preclean_root/uninstall-preserve.log`에 있다.

## 실행 결과

gate는 clean worktree `origin/main` `b807803`에서 빌드했다. 먼저 supervisor `-DryRun -AllowHostMutation`으로 두 step이 계획되는지 확인했다(`dry_run=true`, `ok=true`). 실행은 `2026-10-02T06:25:15Z`부터 약 249초다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `236.876s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.071s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`, `failed_step_id` 없음이다. route-parity summary, MSI lifecycle, OS mutation summary가 `ok=true`다. boot time은 바뀌지 않았고 남은 `pcv-*` VM은 없다.

route-parity 안의 MSI lifecycle 결과:

| phase | exit |
| --- | ---: |
| Install | `0` |
| Repair | `0` |
| Uninstall (preserve) | `0` |
| InstallRemoveData | `0` |
| UninstallRemoveData (`REMOVE_DATA=1`) | `0` |
| final restore Install | `0` |

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{1994F4DF-A62D-497F-83FF-39FEB994FA8D}` `0.42.86` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`d886c632…` / `93134bb8…`) |
| 설치본 Host ProductVersion | `0.42.86-admin-smoke+b807803…` |
| product manifest | `0.42.86-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

data root 최상위 항목은 `20`개에서 `18`개가 됐다. 빠진 두 파일은 로그다.

- `install.jsonl`
- `events.jsonl`

둘 다 gate의 `UninstallRemoveData` 뒤로 다시 생기지 않았다. 계정, token, 서명 키, job store는 그대로다.

gate의 operational MSI(`85387f31…`)와 pair target clean MSI(`8edb19ce…`)는 identity가 다르다. clean package provenance는 `1c488b6`이고, 이 gate provenance는 `b807803`이다. `fb95de1`은 두 commit의 조상이다.

## Nonclaims

- operational current는 `0.42.84-admin-smoke`다. 이 gate는 Lane 3 승격을 하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.

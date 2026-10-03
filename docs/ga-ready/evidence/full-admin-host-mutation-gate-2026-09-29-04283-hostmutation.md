# Full admin host mutation gate `0.42.83-admin-smoke` (2026-09-29)

evidence_id: `full-admin-host-mutation-gate-2026-09-29-04283-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.83-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260929-04283`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260929-04283`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20260929-04283.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260929-04283`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260929-04283`
preclean_root: `artifacts/fullgate-preclean-20260929-04283`
batch_summary_sha256: `9d857880ea3cfedbf77c37ec45c37c2734c5ed87c20448262601f9219fb105a8`
routeparity_summary_sha256: `fe81d616dd89807304e981308c5e3e12579e66062c6864c1c6e14d2b45ff5bbe`
os_summary_sha256: `6551a71f90d3e08802b64f62a0642f08177f612412fcf6c5c8ee7c66e0f8b748`
operational_fullgate_msi_sha256: `b7e26bfb466dc671e3651dace50eb34deabc8c433fbb507d120f91e04f4651a7`
operational_fullgate_payload_aggregate_sha256: `773eb918591361a978ef7a3c83176472387a544ae4ee1bbdd8d77ab5a456edc5`
service_host_sha256: `22e4ed8335cc174628697e9a1b57cfee0c1f541432d2676635bf8621c389b9bd`
cli_sha256: `2a4e1649c6f71564772b0ac00bcd92943d1f481373cc52c8476499fd69d1ee64`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
installed_product_version: `0.42.83-admin-smoke+68462481dee72049a1c0918f201efa2e1c387160`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_after: `1`
provenance_commit: `68462481dee72049a1c0918f201efa2e1c387160`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 정리

같은 version의 ProductCode가 겹치지 않게 하려고 gate 전에 설치본을 비웠다. 0.42.78 r2에서 같은 version 재실행이 잔여 ProductCode를 남겨 설치본 교체와 제거를 막은 적이 있다.

Burn task가 설치한 clean package `0.42.83`(`{D74A4692-388C-40CD-81F7-036E3CA61840}`)을 데이터를 보존하는 `msiexec /x`로 제거했다. exit는 `0`이고 `REMOVE_DATA`는 쓰지 않았다. 제거 뒤 ARP 항목 `0`, service 없음, product root 없음을 확인했다. data root 파일 `11`개는 제거 전후 같은 목록이다.

## 실행 결과

gate HEAD는 `6846248`(clean tree)이다. 실행 시간은 2026-09-29 `14:23:15Z`부터 `14:27:06Z`까지다. manifest는 0.42.78 r2 manifest에서 version, batch id, artifact 경로만 바꿨다. supervisor `-DryRun -AllowHostMutation`으로 두 step이 계획되는 것을 먼저 확인했다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `218.403s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.086s` |

batch는 `ok=true`, `status=completed`, `executed_steps=2`다. route-parity summary, `msi-lifecycle-smoke`, `hyperv-api-route-smoke`, OS mutation summary 모두 `ok=true`다.

MSI lifecycle(route-parity 안) 결과:

| step | phase | exit |
| --- | --- | ---: |
| `install` | Install | `0` |
| `repair` | Repair (`REINSTALL=ALL`) | `0` |
| `uninstall-preserve` | Uninstall | `0` |
| `install-remove-data` | InstallRemoveData | `0` |
| `uninstall-remove-data` | UninstallRemoveData (`REMOVE_DATA=1`) | `0` |
| `final-restore-install` | Install | `0` |

OS mutation gate 결과는 artifact root에 있다. 대상은 event log 등록·제거, firewall enable·remove, trust store install·remove·restore, LAN listener, 서비스 실행 중 config migration apply다.

## 최종 상태

| 항목 | 값 |
| --- | --- |
| ARP `PureCVisor Desktop Node` | `{A531920C-1242-4178-8FA9-2ACB7CC4866A}` `0.42.83` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`22e4ed83…` / `2a4e1649…`) |
| 설치본 Host ProductVersion | `0.42.83-admin-smoke+6846248…` |
| service | Running / Auto |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

data root 파일은 `11`개에서 `10`개가 됐다. `install.jsonl`이 없다. 이것은 gate의 `uninstall-remove-data`(`REMOVE_DATA=1`)가 설계대로 지우는 목록(`DesktopNodeHostServiceAction.cs`: `jobs.json.commit-pending`, `events.jsonl`, `install.jsonl`, `diagnostics`)에 든 파일이다. token, 계정, 서명 키 파일은 남아 있다.

gate의 operational MSI(`b7e26bfb…`)는 pair target clean MSI(`52d7cfd5…`)와 identity가 다르다. 0.42.78과 같은 구조다.

## Nonclaims

- operational current는 `0.42.78-admin-smoke`다. current-evidence는 Task 5에서 쓴다.
- public trusted signing과 external stable publication을 주장하지 않는다.

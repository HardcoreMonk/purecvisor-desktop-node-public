# Full admin host mutation gate `0.42.85-admin-smoke` (2026-10-01)

evidence_id: `full-admin-host-mutation-gate-2026-10-01-04285-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.85-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20261001-04285`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20261001-04285`
batch_manifest: `artifacts/batch-manifests/full-admin-host-mutation-gate-20261001-04285.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20261001-04285`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20261001-04285`
preclean_root: `artifacts/fullgate-preclean-20261001-04285`
batch_summary_sha256: `292303c14fcf4fa90076692b0f2ff1dd2a4f957b8c532e9b23824d158fdc1101`
routeparity_summary_sha256: `ab585d7799d3cb22450452d68a99120610da8794ce30defe44cc145f88f4e36c`
os_summary_sha256: `a4f65a2a0cc6d3fa2f207b1c27ba6c742067906fbc2ecd1b495f2efd616a2b5e`
operational_fullgate_msi_sha256: `22d2f99ee390df51cfd6657c04a38eb46b3216574f3bd7f44dd2c34905d2d6ed`
operational_fullgate_payload_aggregate_sha256: `53a3d18eac780bb4a742ec06a1cae8f51c7ff166399dc45ef218d5950eb720ce`
service_host_sha256: `76b7410b0cf88d7c5bef56944a07cf6d7ff14a922dd8c8b5d4ceaa870d8011d1`
cli_sha256: `9476779d07429646f75e9bd30a55adf78f95283d7ec0d7891198386bb82d25e4`
product_wrapper_sha256: `086d491283f170558899cbce5e640c17e774186ed83b86d39a791ce4a7f4c1d5`
installed_product_version: `0.42.85-admin-smoke+62a0a1e32e3a8686eedc6e0a3da1406f6c0f3e80`
installed_payload_matches_fullgate_build: `true`
arp_entry_count_after: `1`
provenance_commit: `62a0a1e32e3a8686eedc6e0a3da1406f6c0f3e80`
signing_mode: `AllowUnsignedDev`
iso_path: `D:/Downloads/ubuntu-26.04-live-server-amd64.iso`
lan_prefix: `http://[redacted-private-endpoint]:7777/`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## 사전 정리

같은 version의 ProductCode가 겹치지 않도록 gate 전에 설치본을 비웠다. 0.42.85는 이번 pair에서 처음 빌드했으므로 남은 ProductCode는 없었다(ARP `0.42.85` 1개).

Burn task가 복구한 clean package `0.42.85`(`{B23EB9BF-F5DA-43BD-9BA4-E827F766387F}`)를 `msiexec /x`로 제거했다. 데이터는 보존했다(exit `0`, `REMOVE_DATA` 미사용). 제거 뒤 확인 결과:

- ARP 항목 `0`
- service 없음
- product root 없음
- data root 최상위 파일 `12`개가 제거 전후 같은 목록

제거 로그는 `preclean_root/uninstall-preserve.log`에 있다.

## 실행 결과

gate HEAD는 `62a0a1e`(clean tree)다. 실행은 2026-09-30 `15:44:51Z`부터 `15:48:35Z`까지다. manifest는 0.42.84 manifest에서 version, batch id, artifact 경로만 바꿨다(남은 `04284` 문자열 `0`개). 먼저 supervisor `-DryRun -AllowHostMutation`으로 두 step이 계획되는지 확인했다. supervisor 출력은 `preclean_root/batch-run.log`에 있다.

| step | result | exit | attempts | duration |
| --- | --- | ---: | ---: | ---: |
| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `211.849s` |
| `os-mutation-gate` | `PASS` | `0` | `1` | `11.064s` |

batch 결과는 `ok=true`, `status=completed`, `executed_steps=2`다. 다음 summary가 모두 `ok=true`다:

- route-parity summary
- `msi-lifecycle-smoke`
- `hyperv-api-route-smoke`
- OS mutation summary

route-parity summary 기준으로 boot time은 바뀌지 않았고 남은 `pcv-*` VM은 없다.

route-parity 안의 MSI lifecycle 결과:

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
| ARP `PureCVisor Desktop Node` | `{E599C138-69C9-4DF7-8528-5290E3E845D0}` `0.42.85` (1개) |
| 설치본 Host / CLI SHA-256 | gate build와 같음 (`76b7410b…` / `9476779d…`) |
| 설치본 Host ProductVersion | `0.42.85-admin-smoke+62a0a1e…` |
| product manifest | `0.42.85-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | `pcv-guest-installed-04253-r1` Off만 남음 |
| `PureCVisor` firewall rule | `0` |

data root 최상위 파일은 `12`개에서 `10`개가 됐다. 빠진 두 파일은 모두 로그 파일이다:

- `install.jsonl`: MSI custom action 로그
- `events.jsonl`: 서비스 `--event-log` JSON-lines 경로

두 파일 모두 gate의 `UninstallRemoveData`(`REMOVE_DATA=1`) 뒤로 다시 생기지 않았다. 계정, token, 서명 키, job store, transition 파일 `10`개는 그대로다. 0.42.84 gate의 사전 목록에는 `events.jsonl`이 없었다. 그래서 그때는 `install.jsonl` 하나만 빠졌다.

gate의 operational MSI(`22d2f99e…`)와 pair target clean MSI(`cba74683…`)는 identity가 다르다. 0.42.84와 같은 구조다.

## Nonclaims

- operational current는 `0.42.84-admin-smoke`다. 이번 campaign은 Lane 3 승격을 하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.

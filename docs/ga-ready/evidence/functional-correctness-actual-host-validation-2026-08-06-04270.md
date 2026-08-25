# Functional correctness actual-host validation `0.42.70-admin-smoke` (2026-08-06)

evidence_id: `functional-correctness-actual-host-validation-2026-08-06-04270`
result: `PARTIAL_PASS_WITH_CARRY_FORWARD`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.70-admin-smoke`
fullgate_batch: `full-admin-host-mutation-gate-20260806-04270`
provenance_commit: `e91389880febdfb3c1ba430f97c84c2f7e006591`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 `0.42.70` anchor의 functional evidence 소유자다. **이 버전에서 실제로 재실행한 것과
이전 버전에서 이월한 것을 분리해서 기록한다.** 이월 항목을 재실행한 것처럼 읽어서는 안 된다.

## 이 버전에서 실제로 실행한 것

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| 설치본 Hyper-V API route smoke | `PASS` | `full-admin-host-mutation-gate-20260806-04270`의 `installed-dotnet-host-hyperv-api-route-smoke` |
| 실제 VM 생성/checkpoint/삭제 | `PASS` | `pcv-spike-api-c8b5decc`, `vm_list_absent_after_delete=true` |
| unmanaged VM delete guard | `PASS` | `pcv-spike-api-foreign-57e0dec4`, `unmanaged_vm_still_exists_after_block=true` |
| Gen2 boot order (FC-13) | `PASS` | smoke VM `generation=2`가 `ubuntu-26.04-live-server-amd64.iso`로 설치본 경로에서 기동 |
| Gen2 Secure Boot 템플릿 (FC-13) | `PASS` | 동일 Gen2 VM 기동 경로 |
| service/MSI lifecycle | `PASS` | 같은 gate의 `service-action-smoke`, `msi-lifecycle-smoke` |
| OS mutation gate | `PASS` | 방화벽 `0`, Event Log source 없음, `boot_time_unchanged=true` |
| 설치본 CLI/Web surface | `PASS` | CLI `3/3` exit `0`, Web `2/2` HTTP `200` |

## 이 버전에서 재실행하지 않고 이월한 것

아래 항목은 `0.42.70` gate에서 실행되지 않았다. gate artifact
`hyperv-api-route-smoke.json`에 `disk_resize`/`shrink`/`expand` 관측이 `0`건이고, `qos` 문자열은
`storage_qos`/`network_qos` **readback contract 필드**로만 등장하며 QoS 변환 mutation 실행
기록이 아니다.

| 이월 항목 | 마지막 실측 버전 | 소유 evidence |
| --- | --- | --- |
| QoS `2048 Kbps -> 2,048,000 bps` 변환 | `0.42.65` | `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md` |
| disk shrink guard (`PCV_VM_DISK_SHRINK_NOT_SUPPORTED`) | `0.42.65` | 동일 |
| disk `10 -> 11 GiB` expansion | `0.42.65` | 동일 |

`0.42.69` anchor도 이 세 항목을 이월로 기록했다. `0.42.70`에서도 재검증하지 않았으므로 이월
상태가 이어진다. 이 항목들은 provider/control payload가 바뀔 때 재실행 대상이다.

> **추가 (2026-08-06 후속 작업에서 닫힘):** 이 snapshot은 그대로 두고 상태 변화만 덧붙인다.
> 위 세 이월 항목은 같은 날 후속 작업에서 같은 `0.42.70` 설치본에 대해 실제로 재실행돼 모두
> `PASS`했다. 소유 evidence는
> `docs/ga-ready/evidence/functional-correctness-carry-forward-revalidation-2026-08-06-04270.md`,
> artifact는 `artifacts/functional-correctness-carryforward-20260806-04270/summary.json`,
> runner는 `packaging/windows-desktop-node/tools/Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`다.
> 위 표는 이 문서가 작성된 시점의 기록이며, 이 줄 이후로 세 항목은 더 이상 이월이 아니다.

FC-13의 근본 원인 분석과 실호스트 관측 상세는
`docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md` 부록 A가
소유한다.

## Nonclaims

- 이월 항목을 `0.42.70`에서 재검증했다고 주장하지 않는다.
- 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.

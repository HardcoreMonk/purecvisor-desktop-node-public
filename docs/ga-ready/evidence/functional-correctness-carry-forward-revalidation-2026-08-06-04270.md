# Functional correctness 이월 3항목 재검증 `0.42.70-admin-smoke` (2026-08-06)

evidence_id: `functional-correctness-carry-forward-revalidation-2026-08-06-04270`
result: `PASS`
evidence_scope: `internal-admin-smoke-actual-hyperv-vm`
version: `0.42.70-admin-smoke`
artifact: `artifacts/functional-correctness-carryforward-20260806-04270/summary.json`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`
validation_vm: `pcv-fc-cf-5e6f4823`
host_mutation_performed: `true`
validation_vm_cleanup: `PASS`
validation_root_cleanup: `PASS`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 문서는 `0.42.69`와 `0.42.70` anchor가 모두 `0.42.65` 이월로 기록했던 세 항목을 `0.42.70`
설치본에서 실제로 재실행한 결과를 소유한다.
`docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-06-04270.md`의
"재실행하지 않고 이월한 것" 표는 이 evidence로 닫힌다.

## 실측 결과

| 검증 항목 | 요청 | 실제 결과 | 판정 |
| --- | --- | --- | --- |
| Network QoS maximum 변환 | `2048 Kbps` | WMI `MaximumBandwidth=2,048,000 bps` | `PASS` |
| Disk shrink guard | `10 GiB -> 9 GiB` | job `failed`, `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`, 크기 `10,737,418,240` bytes 유지 | `PASS` |
| Disk expansion | `10 GiB -> 11 GiB` | job `succeeded`, 크기 `11,811,160,064` bytes | `PASS` |
| Cleanup | 검증 VM·임시 root 제거 | 남은 validation VM/root `0/0` | `PASS` |

QoS job은 `job-735252e6493b4cc9a03695b28a7c3247`이고 target은 `adapter0`, 요청은 maximum
`2048 Kbps` / minimum `0 Kbps`다. 관측값은 job evidence가 아니라 cleanup 직전
`Get-VMNetworkAdapter`의 `BandwidthSetting.MaximumBandwidth`를 직접 읽은 `2048000`이며, 기대값
`2048 * 1000`과 정확히 일치한다.

Disk shrink job `job-21d9c28237a1440e9f4fcdeb100f4d95`는 의도한 product problem code
`PCV_VM_DISK_SHRINK_NOT_SUPPORTED`로 실패했고 VHDX 크기는 변하지 않았다. 이어진 expansion job
`job-8b3b3ee0f5804a3b8228e25ec93c2751`은 성공했고 `11,811,160,064` bytes가 적용됐다. 크기는
job 보고값이 아니라 `Get-VHD`의 `Size`로 확인했다.

VHDX 경로는
`C:\Users\Operator\AppData\Local\Temp\pcv-functional-correctness-carryforward\pcv-fc-cf-5e6f4823\disk0.vhdx`였고,
검증 VM과 전용 임시 root는 실행 종료 시 모두 제거됐다. keep policy 자산
`pcv-guest-installed-04253-r1`은 건드리지 않았고 실행 후 checkpoint `1`개와 함께 그대로 있다.

## 04265 대비 달라진 것

`0.42.65` 실측은 전용 runner 없이 손으로 수행됐고 그래서 이후 버전에서 재현되지 않은 채 이월만
반복됐다. 이번에는 같은 절차를 `Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`로 고정해
다음 anchor가 같은 명령 한 줄로 재실행할 수 있게 했다.

`0.42.65`가 기록한 Default Switch non-zero reservation 호스트 제약(minimum `1024 Kbps` 요청이
`0x80070057`로 거부되는 건)은 이번 실행 범위에 넣지 않았다. minimum `0 Kbps`만 요청했으므로 그
제약은 이 evidence가 재확인하거나 반박하지 않는다.

## Nonclaims

- 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication을 주장하지 않는다.
- Default Switch non-zero bandwidth reservation 제약은 재검증하지 않았다.
- installed update/rollback compensation은 이 evidence의 PASS 범위가 아니다.

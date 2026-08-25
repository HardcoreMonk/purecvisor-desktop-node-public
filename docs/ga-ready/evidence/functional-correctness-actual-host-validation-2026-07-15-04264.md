# Functional correctness actual-host validation 2026-07-15 0.42.64

evidence_id: `functional-correctness-actual-host-validation-2026-07-15-04264`
result: `PASS_WITH_DOCUMENTED_HOST_LIMITATION`
evidence_scope: `internal-admin-smoke-actual-hyperv-vm`
version: `0.42.64-admin-smoke`
qos_artifact: `artifacts/functional-correctness-qos-actual-vm-20260715-04264/summary.json`
disk_artifact: `artifacts/functional-correctness-disk-actual-vm-20260715-04264-r2/summary.json`
host_mutation_performed: `true`
validation_vm_cleanup: `PASS`
validation_root_cleanup: `PASS`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실측 결과

| 검증 항목 | 요청 | 실제 결과 | 판정 |
| --- | --- | --- | --- |
| Network QoS maximum 변환 | `2048 Kbps` | WMI `Limit=2,048,000 bps` | `PASS` |
| Network QoS minimum 0 | `0 Kbps` | 적용 job `succeeded`, 적용 evidence `0 Kbps` | `PASS` |
| Disk shrink guard | `10 GiB -> 9 GiB` | `PCV_VM_DISK_SHRINK_NOT_SUPPORTED`, 크기 `10,737,418,240` bytes 유지 | `PASS` |
| Disk expansion | `10 GiB -> 11 GiB` | job `succeeded`, 크기 `11,811,160,064` bytes | `PASS` |
| Cleanup | 검증 VM·임시 root 제거 | 남은 validation VM/root `0/0` | `PASS` |

QoS 성공 job은 `job-ac5adf2a7cbf426abefd1e74f71c4a7b`다. Job evidence의 applied policy는
maximum `2048 Kbps`, minimum `0 Kbps`였고 cleanup 직전 `Get-VMNetworkAdapter`의 scalar
`BandwidthSetting`으로 `2,048,000 bps`를 직접 확인했다. 설치본 `pcvcli vm bandwidth`는 현재
numeric applied QoS를 투영하지 않는 inventory readback contract이므로 job evidence와 raw WMI
scalar가 이 실측의 authoritative readback이다.

Disk shrink job `job-3f4e2651649443188f5a9f08389f4cbd`는 의도한 product problem code로
실패했고 VHDX 크기는 변하지 않았다. 이어서 expansion job
`job-e9a8760c1c504a63bd358b73c8842485`는 성공했으며 11 GiB가 적용됐다.

## 확인된 호스트 제약

Hyper-V `Default Switch`에서 minimum `1024 Kbps`를 함께 요청한 선행 probe
`job-d6a35b346f314f9f8254c35f10f788a4`는 native `0x80070057` /
`PCV_HYPERV_WMI_JOB_FAILED`로 거부됐다. 이는 maximum Kbps→bps 변환 실패가 아니라 Default
Switch가 non-zero bandwidth Reservation을 지원하지 않는 호스트 제약으로 분류한다. 현재 API는
이 조합을 queue 단계에서 선제 차단하지 않으므로 후속 UX/problem-code 개선 후보로 남긴다.

이 evidence는 실제 Hyper-V VM과 VHDX를 생성·변경한 뒤 모두 제거한 internal admin-smoke다.
Installed update/rollback compensation, public trusted signing 또는 외부 stable publication은 이
evidence의 PASS 범위가 아니다.

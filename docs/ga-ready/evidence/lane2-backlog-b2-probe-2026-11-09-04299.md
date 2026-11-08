# Lane 2 backlog B2 수정 확인 `0.42.99-admin-smoke` (2026-11-09)

evidence_id: `lane2-backlog-b2-probe-2026-11-09-04299`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.99-admin-smoke`
installed_product_version: `0.42.99-admin-smoke+a95987b094f9865c1f344043afba48b810ecc8f5`
installed_host_sha256: `a98ad16bbec388b4d34cb168f63dcdcc0abbce753ce7c1da9721b35fdbaf8435`
installed_cli_sha256: `c7531573e8a4664992aaebf2a667b3a8241c67e53a9221a59919350e176d282b`
train_carriage: `PR #116` (`5f6c1f0c`)
backlog: `BL-0034`, `BL-0033`
host_mutation_performed: `true` (ADR-0016 `pcv-it-probe-list-1109b` 생성·시작·전원 끄기·checkpoint·삭제)
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

train `0.42.99-admin-smoke` 적재 PR #116(backlog B2)의 Lane 2 probe다. 기준은
`docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-train-04299.md` Task 6이다.

설치본은 fullgate 뒤의 `0.42.99`이고 loopback 전용이다. 호출은 PCVCLI(service bearer, `--format json`)로 했다. 쓴 VM은
ADR-0016 일회용 `pcv-it-probe-list-1109b` 하나다(Generation 2, 1 vCPU, 512 MB, 8 GB, smoke ISO
`artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`, VM root `artifacts/` 아래).

- BL-0034: 지워지는 VM 한 행의 요약 필드가 잠깐 비면 VM 목록 전체가
  `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE`로 실패했다. 2026-11-09 재현(설치본 `0.42.98`)에서는 삭제 3회 모두
  1회씩 실패했다. 수정은 요약 공백이면 400 ms 뒤 한 번 다시 읽는 것이다.
- BL-0033: 켜진 VM의 checkpoint 복원이 원시 `PCV_HYPERV_WMI_METHOD_FAILED`로 실패했다. 수정은 꺼지거나 저장된 상태가
  아니면 job을 만들기 전에 `409` `PCV_CHECKPOINT_RESTORE_VM_RUNNING`으로 거절하는 것이다.

## 결과

실행: 2026-11-09 04:46:49~04:48:03 KST, 일회용 스크립트(세션 작업 폴더, 저장소 밖).

### BL-0034 삭제 중 VM 목록

재현과 같은 방법이다. job마다 진행 중과 끝난 직후 `vm list`를 이어 읽었다.

| 단계 | job | 소요(ms) | 목록 읽기 | 성공 | 실패 |
| --- | --- | --- | --- | --- | --- |
| cycle 1 생성 | succeeded | 8,157 | 9 | 9 | 0 |
| cycle 1 삭제 | succeeded | 6,858 | 8 | 8 | 0 |
| cycle 2 생성 | succeeded | 8,152 | 9 | 9 | 0 |
| cycle 2 삭제 | succeeded | 6,719 | 8 | 8 | 0 |
| cycle 3 생성 | succeeded | 8,098 | 9 | 9 | 0 |
| cycle 3 삭제 | succeeded | 6,851 | 8 | 8 | 0 |
| cycle 4 삭제(checkpoint 있음) | succeeded | 7,132 | 8 | 8 | 0 |

삭제 4회 동안 읽은 목록 32회와 생성 3회 동안 읽은 27회가 모두 성공했다.
`PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE`는 0회다. 재현 때는 삭제마다 1회였다.

### BL-0033 켜진 VM 복원 거절

| 순서 | 동작 | 결과 |
| --- | --- | --- |
| 1 | `vm create` | job succeeded |
| 2 | `vm checkpoint create --name pcv-probe-cp` (꺼진 상태) | job succeeded |
| 3 | `vm start` | job succeeded, VM Running |
| 4 | `vm checkpoint restore` (켜진 상태) | 거절 `PCV_CHECKPOINT_RESTORE_VM_RUNNING`, job 없음 |
| 5 | `vm poweroff` | job succeeded |
| 6 | `vm checkpoint restore` (꺼진 상태) | job 생성, succeeded |
| 7 | `vm delete --yes` | job succeeded(위 cycle 4) |

4번은 job을 만들기 전에 거절됐다. 원시 WMI 오류가 아니라 전용 코드로 거절된 것이다. HTTP `409`는
`ApiRuntimePolicyRequestProcessorTests.CheckpointRestorePower`가 계약으로 본다. PCVCLI JSON 출력에는 상태 숫자가 없다.

5번 직후 같은 PowerShell 프로세스의 `Get-VM`은 `Running`을 읽었다. 같은 프로세스 안 Hyper-V cmdlet의 캐시 값이다.
6번에서 API가 꺼진 상태로 보고 복원 job을 받아 성공했다.

## 끝 상태

- `pcv-it-probe-*` VM 0개(Hyper-V와 CIM 모두). 빈 VM root 폴더 `artifacts/pcv-it-probe-list-1109b`는 지웠다.
- 보존 VM `pcv-guest-installed-04253-r1` Off, S2 template `pcv-it-s2-source` Off(그대로).
- service Running/Automatic, Web `http://127.0.0.1/` 200, PureCVisor 방화벽 규칙 0.

## Nonclaims

- operational current는 바뀌지 않았다(`0.42.98-admin-smoke`).
- Web Console 화면 쪽 BL-0034(삭제 job 성공 시 행 제거)와 BL-0032·0027·0031·0026·0024는 `web` 시험과 browser UI
  gate가 본다. 이 probe는 설치본 API·CLI만 본다.
- public trusted signing과 external stable publication을 주장하지 않는다.

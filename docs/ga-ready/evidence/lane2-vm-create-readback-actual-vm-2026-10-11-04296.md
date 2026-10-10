# Lane 2 probe: `vm.create` inventory readback on the installed 0.42.96 (2026-10-11)

- 종류: release train `0.42.96-admin-smoke` 적재 queue 78(변경 `1b04ff46`, BL-0014)의 Lane 2 probe. campaign `train-04296-20261011` Task 5. 승격 근거가 아니고 `current-evidence.json`에 쓰지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 확인한 것: managed Generation 2 VM create job이 `succeeded`한 직후 `GET /api/v1/vms`와 `pcvcli --json vm list`가 `PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE` 없이 새 VM을 돌려주는지(BL-0014의 inventory readback 대기와 EnabledState 전이 어휘).

## 설치본

- `0.42.96-admin-smoke+9ac8eb3`(train 0.42.96 fullgate 뒤), service Running/Automatic, Web 200. 보존 VM `pcv-guest-installed-04253-r1`과 `pcv-it-s2-source`는 Off 그대로.

## 실행

- 스크립트: scratch `train04296-probe-create.mjs`(Local API loopback 세션, token은 메모리에만). VM `pcv-it-probe-create-1011`: Generation 2, CPU 1, memory 512 MB, disk 8 GB, ISO는 smoke ISO, VM root `artifacts/pcv-it-probe-create-1011`. VM을 켜지 않았다.
- 시작 `2026-10-10T17:04:37.383Z`, 끝 `2026-10-10T17:04:46.460Z`.

| 단계 | 결과 |
| --- | --- |
| list before | status 200, 2개, error 없음 |
| create | status 202, job `job-ab2bf677ab5e4bf08f2931bcb410883b` succeeded (2.6초) |
| list after create (API) | status 200, error 없음, job 완료 뒤 690 ms, 3개, probe row state `stopped` |
| list after create (pcvcli --json) | parsed true, 3개, probe row state `stopped`, 오류 코드 없음 |
| detail | status 200, state `stopped`, generation 2, managed null |
| delete | status 202, job `job-a6d040b402f3445abe2c9d431b51dd73` succeeded (2.1초) |
| after delete | VM present false, VM root exists true, inventory 2개 |

## 판정: PASS

- create 직후 inventory가 거절되지 않고 새 VM이 off 어휘로 보였다(BL-0014 수정 확인). delete 뒤 VM과 폴더가 없다.
- summary는 `artifacts/lane2-vm-create-readback-20261011/summary.json`(git 밖).

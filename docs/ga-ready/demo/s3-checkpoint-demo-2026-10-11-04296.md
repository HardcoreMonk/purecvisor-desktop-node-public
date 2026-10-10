# S3 checkpoint 시연 (2026-10-11)

- 시나리오: `S3`. checkpoint 생성·복원과 예약 실행을 브라우저에서 한다.
- 기준: `config/project-completion-criteria.json`의 S3. ADR-0017. 직전 시연 `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-10.md`(설치본 `0.42.93`, 브라우저 예약 저장 FAIL, BL-0016)의 재시연이다.
- 이 기록은 이 호스트 설치본 `0.42.96-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. S4는 주장하지 않는다.
- 진행 상태: 끝났다. 브라우저 생성·복원·예약 미리보기·예약 저장은 campaign `train-04296-20261011` Task 7, 예약 실행 확인·
  브라우저 schedule clear·정리는 Task 9(2026-10-11 KST, 2026-10-10 UTC 18:05~18:14)다.

## 설치본

- 설치본 `0.42.96-admin-smoke+9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`(train 0.42.96 fullgate build), package 모드
  `AllowUnsignedDev`(signing trust model `LocalTest`). 시연은 설치본을 바꾸지 않았고 Rollback도 없다.
- service `PureCVisorDesktopNode` Running(Automatic), Web `http://127.0.0.1/` HTTP 200(Single Edge 셸), Local API
  `http://127.0.0.1:7777`(loopback). 보존 VM `pcv-guest-installed-04253-r1` Off, template VM `pcv-it-s2-source` Off.

## 실행

- VM만 Local API로 만들었다: `pcv-it-s3-source`(managed Generation 2, vCPU 1, memory 512 MB, disk 8 GB, VM root
  `artifacts/s3-checkpoint-20261011/vms`, DVD는 기존 smoke ISO, Off). create job `job-536ab38b015e4b268e09c2496f5d1ec4` `succeeded`,
  checkpoint 0개. 그 뒤 모든 단계는 Playwright(Chromium)로 새 셸의 VM 화면에서 폼과 버튼을 눌러 했다.
- 브라우저 checkpoint 생성 `s3-cp1`(Checkpoint name 입력 → Create checkpoint): job `job-d0b4ed886701414b9a8a6f3c7e9a11dd`
  `succeeded`, 목록에 `s3-cp1`.
- 브라우저 복원 `s3-cp1`(Restore → 확인 대화상자 수락): job `job-fa66031c0a624307ba1db4b488f5c7b5` `succeeded`.
- 브라우저 예약 미리보기(interval 60, retention 3 → Preview schedule 클릭): `POST /api/v1/vms/pcv-it-s3-source/checkpoints/schedule/preview`
  HTTP 200, dry run, `interval_minutes=60`, `retention_max=3`.
- 브라우저 예약 저장(같은 값 → **Save schedule 버튼 클릭** → 확인 대화상자 수락): job `job-e60738a6ca0846aca8eaeb69311932b4`
  `checkpoint.schedule.set` `succeeded`. 폼 submit이 취소되지 않았다(BL-0016 수정 확인).
- readback(API와 화면): `enabled / waiting / interval 60 / retention 3 / last enqueued 2026-10-10T17:08:33Z / next due
  2026-10-10T18:08:33Z`, checkpoint 1개.
- 관찰(report-only, backlog BL-0020): schedule set job이 끝나도 VM 상세 readback이 바로 갱신되지 않았다. VM을 다시 선택해
  detail을 다시 읽은 뒤 다음 렌더에서 `enabled`로 바뀌었다. 저장 값은 처음부터 맞았다(API readback).
- 예약 실행(service worker): `next_due_at` 18:08:33Z 뒤 18:08:35Z에 `pcv-schedule-20261010T180835Z`가 생겼다(checkpoint 시각
  18:08:38Z). readback `last_enqueued_at 2026-10-10T18:08:35Z`, `next_due_at 2026-10-10T19:08:35Z`. 확인은 Local API를 1분마다 읽기만
  하는 대기 스크립트로 했다(18:09:15Z에 발견).
- `node web/scripts/run-s3-checkpoint-scenario.mjs --execute --verify-only --expect-count=2`: `result=pass`, checkpoint 2개
  (`s3-cp1`, `pcv-schedule-20261010T180835Z`). 이 실행은 `--out`·`--vm-root`를 공백으로 띄운 형식으로 넘겼는데 스크립트는
  `--name=value`만 읽어 이를 무시했다. 그래서 summary가 기본 폴더 `artifacts/s3-checkpoint-20261010/summary.json`에 써져 2026-10-10
  시연의 로컬 summary(git 밖)를 덮었다. 이번 내용은 `artifacts/s3-checkpoint-20261011/verify-only-summary.json`으로 복사했다.
  2026-10-10 시연 기록 문서는 바뀌지 않았다(backlog BL-0022).
- 브라우저 readback(새 셸의 VM 화면): schedule `enabled / waiting / interval 60 / retention 3`, last enqueued 18:08:35Z, next due
  19:08:35Z, Checkpoints 목록에 `s3-cp1`과 `pcv-schedule-20261010T180835Z`.
- 브라우저 schedule clear(**Clear schedule 버튼 클릭** → 확인 대화상자 수락): job `job-0706d0ebb8cb4718b9adadcf6e43433c`
  `checkpoint.schedule.clear` `succeeded`(18:12:00Z). API readback `enabled=false`, `status=disabled`. 화면은 job 뒤에도 `enabled`로
  남았고, VM을 다시 선택한 뒤 `disabled`로 바뀌었다(BL-0020과 같은 현상). clear는 기존 checkpoint를 지우지 않아 2개가 남았다.

## 화면 캡처

캡처는 `docs/ga-ready/demo/s3-checkpoint-20261011/` 아래 PNG다. 1280×800으로 찍어 500 KB 이하 규칙에 맞춰 960×600으로 축소했다.
loopback 주소만 보이고 사용자 홈 경로·LAN IP·호스트명·token은 없다.

- `01-checkpoint-created.png`: 폼으로 만든 `s3-cp1`이 VM Checkpoints 목록에 보인다.
- `02-schedule-preview.png`: interval 60, retention 3으로 Preview schedule을 누른 화면.
- `03-schedule-saved.png`: Save schedule 뒤 readback `enabled / waiting / interval 60 / retention 3`, last enqueued, next due.
- `04-scheduled-run.png`: 예약 실행 뒤 readback(last enqueued 18:08:35Z, next due 19:08:35Z)과 목록의 `s3-cp1`,
  `pcv-schedule-20261010T180835Z`.
- `05-schedule-cleared.png`: Clear schedule 뒤 VM을 다시 선택한 화면. schedule `disabled`, checkpoint 2개는 남아 있다.

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |
| 확인 방법 | |

확인자 칸이 비어 있으면 `pcvverify completion`이 passed로 세더라도 이 기록은 "에이전트 자가 보고"다(2026-10-09 감사 §10).

## 정리

- `node web/scripts/run-s3-checkpoint-scenario.mjs --execute --cleanup-only --out=artifacts/s3-checkpoint-20261011
  --vm-root=artifacts/s3-checkpoint-20261011/vms`: delete job `job-e5ba3fb5210a4c26b118c0a0349ea92b` `succeeded`, `vm_remaining=false`,
  `result=pass`(18:13:42Z). `pcv-it-s3-source`와 checkpoint 2개를 지웠다.
- VM 목록은 `pcv-guest-installed-04253-r1`과 `pcv-it-s2-source`(둘 다 Off)만 남았고 `pcv-it-s3-*` VM은 0개다. VM 폴더
  `artifacts/s3-checkpoint-20261011/vms/pcv-it-s3-source`는 없고, 빈 상위 폴더 `vms`는 지웠다.
- 설치본은 바꾸지 않았다(Rollback 없음). service `Running/Automatic`, Web HTTP 200, ARP의 PureCVisor 항목은 `0.42.96` 하나다.

## 판정

- PASS. S3의 세 동작(checkpoint 생성, 복원, 예약 실행)을 설치본 `0.42.96-admin-smoke`의 새 셸 브라우저에서 했다. 예약 저장과
  clear도 폼 버튼 클릭으로 했고(BL-0016 수정 확인), service worker가 next due 2초 뒤 예약 checkpoint를 만들었다.
- 남은 관찰은 schedule set·clear 뒤 VM 상세 readback이 바로 바뀌지 않는 점이다(BL-0020, report-only). 저장 값과 API readback은
  맞았다.
- 이 기록은 criteria S3를 닫지 않는다. `config/project-completion-criteria.json`의 S3는 `open` 그대로이고, 닫기는 train 뒤 S4
  campaign 첫 PR에서 한다(campaign `train-04296-20261011` 다음 승인 3). `pcvverify completion`은 돌리지 않았고 완료를 주장하지 않는다.

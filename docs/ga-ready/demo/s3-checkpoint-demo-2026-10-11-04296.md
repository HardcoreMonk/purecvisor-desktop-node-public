# S3 checkpoint 시연 (2026-10-11)

- 시나리오: `S3`. checkpoint 생성·복원과 예약 실행을 브라우저에서 한다.
- 기준: `config/project-completion-criteria.json`의 S3. ADR-0017. 직전 시연 `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-10.md`(설치본 `0.42.93`, 브라우저 예약 저장 FAIL, BL-0016)의 재시연이다.
- 이 기록은 이 호스트 설치본 `0.42.96-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. S4는 주장하지 않는다.
- 진행 상태: 브라우저 생성·복원·예약 미리보기·예약 저장까지 끝났다(campaign `train-04296-20261011` Task 7). 예약 실행 확인과
  정리(Task 9)는 `next_due_at` 뒤에 이 기록을 이어 쓴다.

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

## 화면 캡처

캡처는 `docs/ga-ready/demo/s3-checkpoint-20261011/` 아래 PNG다. 1280×800으로 찍어 500 KB 이하 규칙에 맞춰 960×600으로 축소했다.
loopback 주소만 보이고 사용자 홈 경로·LAN IP·호스트명·token은 없다.

- `01-checkpoint-created.png`: 폼으로 만든 `s3-cp1`이 VM Checkpoints 목록에 보인다.
- `02-schedule-preview.png`: interval 60, retention 3으로 Preview schedule을 누른 화면.
- `03-schedule-saved.png`: Save schedule 뒤 readback `enabled / waiting / interval 60 / retention 3`, last enqueued, next due.

## 판정

- (Task 9에서 예약 실행 확인과 정리 뒤 적는다.)

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |

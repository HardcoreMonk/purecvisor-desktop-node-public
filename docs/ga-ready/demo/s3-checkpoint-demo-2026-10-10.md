# S3 checkpoint 시연 (2026-10-10)

- 시나리오: `S3`. checkpoint 생성·복원과 예약 실행을 브라우저에서 한다.
- 기준: `config/project-completion-criteria.json`의 S3. ADR-0017.
- 이 기록은 이 호스트 설치본 `0.42.93-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. S4는 주장하지 않는다.
- 브라우저 예약 저장은 설치본 결함(BL-0016)으로 실패했고 criteria S3는 `open` 그대로다(2026-10-10 2차 결정 4). 판정은 아래
  "판정" 절과 같다.

## 설치본

- 설치본 version `0.42.93-admin-smoke`, provenance commit `818d00f1`(`docs/ga-ready/current-evidence.json`), package 모드
  `AllowUnsignedDev`(signing trust model `LocalTest`, `docs/ga-ready/evidence/admin-smoke-package-2026-10-08-04293.md`).
  시연은 설치본을 바꾸지 않았고 Rollback도 없다.
- service `PureCVisorDesktopNode` Running(Automatic), Web `http://127.0.0.1/` HTTP 200, Local API `http://127.0.0.1:7777`
  (loopback). 보존 VM `pcv-guest-installed-04253-r1` Off, template VM `pcv-it-s2-source` Off.

## 실행

- 명령: `node web/scripts/run-s3-checkpoint-scenario.mjs --execute`(VM 생성, checkpoint `s3-cp1`, 복원, schedule set까지),
  브라우저는 Playwright로 Web Console VM 상세 폼, 예약 실행 뒤 `--execute --verify-only --expect-count=3`, 끝에
  `--execute --cleanup-only`.
- 시연 VM `pcv-it-s3-source`: managed Generation 2, vCPU 1, memory 512 MB, disk 8 GB(저장소 `artifacts/` 아래), DVD는 기존
  smoke ISO, Off.
- 브라우저 checkpoint 생성 `s3-ui-cp`: job `succeeded`.
- 브라우저 복원 `s3-cp1`: job `succeeded`(Related activity에 restore succeeded).
- 브라우저 schedule clear: job `succeeded`(시연 중 두 번, 둘 다 `type="button"` 경로).
- 브라우저 schedule preview/save: **FAIL**. `Preview schedule`/`Save schedule` 클릭은 4회 모두 "Form submission canceled
  because the form is not connected"로 취소되어 요청이 나가지 않았다. 원인은 VM 상세 click 위임 핸들러가 모든
  `button[data-action]`에 대해 끝에서 `render()`를 불러 click 처리 중 form이 detach되는 것이다(BL-0016, 수정 commit
  `01c6833`, 설치본에는 아직 없음).
- schedule set(submit 핸들러 직접 호출): 같은 폼에 `form.requestSubmit(Save)`로 submit 핸들러에 들어가면 confirm →
  `POST /api/v1/vms/pcv-it-s3-source/checkpoints/schedule` → job `succeeded`. readback `enabled / interval 60 / retention 3 /
  last enqueued 2026-10-10T04:24:15Z / next due 2026-10-10T05:24:15Z`.
- 예약 실행: worker가 `next_due_at` 뒤 8초인 2026-10-10T05:24:23Z에 `pcv-schedule-20261010T052423Z`를 만들었다.
  `--execute --verify-only --expect-count=3` `result=pass`. readback `enabled / blocked capacity(count 3 ≥ retention 3) /
  last enqueued 05:24:23Z / next due 06:24:23Z`.
- 결과: `artifacts/s3-checkpoint-20261010/summary.json` `result=pass`(verify-only, cleanup-only). 첫 `--execute` 판은
  "schedule이 enable 즉시 due"라는 가정이 틀려 auto-checkpoint 단계에서 FAIL이었고, 그 뒤 스크립트를 `--skip-auto`,
  `--verify-only`, `--cleanup-only`로 나눴다(plan Task 1·2 실행 기록).
- 브라우저 loopback session은 시연 중 두 번 만료됐다(403 `PCV_AUTH_FORBIDDEN`). stale token이 남아 있으면
  `ensureLoopbackSession`이 세션을 다시 만들지 않아 token을 비우고 다시 만들었다(report-only).

## 화면 캡처

캡처는 `docs/ga-ready/demo/s3-checkpoint-20261010/` 아래 PNG다. 파일당 500 KB 이하이고 loopback 주소만 보인다.

- `01-checkpoints-panel-schedule-armed.png`: VM 상세 Checkpoints 패널. API로 만든 `s3-cp1`과 schedule readback.
- `02-checkpoint-created-from-browser.png`: 폼으로 만든 `s3-ui-cp`가 목록에 보인다.
- `03-checkpoint-restored-from-browser.png`: `s3-cp1` Restore 뒤 패널.
- `03b-related-activity-restore-succeeded.png`: Related activity의 restore job `succeeded`.
- `04-schedule-cleared-from-browser.png`: Clear schedule 뒤 readback `disabled`.
- `05-schedule-saved-readback.png`: submit 핸들러 직접 호출로 저장한 뒤 readback `enabled`, next due.
- `06-scheduled-checkpoint-readback.png`: 예약 실행 뒤 checkpoint 3개와 readback `blocked capacity`, last enqueued, next due.
- `07-schedule-cleared-after-auto.png`: 예약 실행 뒤 UI Clear schedule, readback `disabled`.

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |
| 확인 방법 | |

확인자 칸이 비어 있으면 `pcvverify completion`이 passed로 세더라도 이 기록은 "에이전트 자가 보고"다(2026-10-09 감사 §10).

## 정리

- `--execute --cleanup-only` `result=pass`. `pcv-it-s3-source`와 checkpoint 3개 삭제, `pcv-it-s3-*` VM 0개,
  `artifacts/s3-checkpoint-20261010/vms` 잔여 파일 없음. Rollback 없음(설치본 변경 없음). 보존 VM
  `pcv-guest-installed-04253-r1` Off, template `pcv-it-s2-source` Off.

## 판정

- 브라우저 생성·복원·clear 통과, 예약 실행 통과(설정은 submit 핸들러 직접 호출), 예약 저장 폼 클릭 FAIL(BL-0016).
  S3는 `open` 그대로이고, 브라우저 예약 저장 재시연은 BL-0016이 들어간 설치본(train 0.42.94) 뒤 S4 campaign에서 한다.
- `pcvverify completion`: 돌리지 않음(S3가 `open`이라 판정 대상이 아니다). 완료를 주장하지 않는다.

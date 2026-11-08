# Web Console 재설계 설치본 시연 (2026-11-08)

- 시나리오: 해당 없음. release train `0.42.98-admin-smoke` 적재 queue 97~103·105~108의 Lane 2 probe다. 대상은 Web Console 재설계,
  옛 콘솔 퇴역, 브라우저 품질 게이트, 이전 버전 제품 Update 호환 파일이다. 기준은
  `docs/superpowers/plans/2026-11-08-purecvisor-desktop-node-train-04298.md` Task 5다.
- 기준: ADR-0018 Single Edge 셸. 직전 셸 시연은 `docs/ga-ready/demo/web-console-shell-demo-2026-10-11.md`(설치본 `0.42.96`)다.
- 이 기록은 이 호스트 설치본 `0.42.98-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. 다른 시나리오는 주장하지 않는다.
- 판정: **PASS**. 여덟 단계가 모두 통과했고, 끝 상태에 `pcv-it-probe-*` VM은 0개다.

## 설치본

- 설치본 `0.42.98-admin-smoke+2dfe2ac`(train 0.42.98 fullgate build). package 모드는 `AllowUnsignedDev` / `LocalTest`이고,
  시연 뒤 Rollback은 하지 않았다.
- service Running/Automatic, Web `http://127.0.0.1/` 200, loopback 전용(PureCVisor 방화벽 규칙 0개), 계정 없음(loopback 세션).
- 보존 VM: `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off(template), `pcv-it-s4-console` Running. 시연 전후 같다.

## 실행

- 명령: `node probe-web-console-04298.mjs`. 세션 작업 폴더의 일회용 스크립트이고 저장소에는 넣지 않았다.
  `web/scripts/verify-browser-ui-gates.mjs`와 같은 방식으로 시스템 Edge를 CDP로 몰고, viewport는 1600×1000이다. 새 npm 의존성은 없다.
  destructive 동작(정지, 복원, 복제, 삭제)은 대화상자나 행의 이름이 `pcv-it-probe-`로 시작할 때만 누른다. 보존 VM 이름이 보이면 중단하고,
  실패해도 `finally`에서 자기 VM과 폴더를 지운다. token, cookie, 세션 값은 출력하거나 저장하지 않는다.
- 구현과 반복 실행은 grok(Herdr agent)이 했고, Claude가 결과와 캡처를 검토했다.
- 마지막 실행: 2026-11-08 17:26:57부터 17:33:07 KST까지(6분 10초), 결과 `ok=true`. 실행 중에 `running`/`queued`로 기록된 job 다섯 개는 끝난 뒤
  Local API `GET /api/v1/jobs/<id>`로 다시 읽었고 모두 `succeeded`였다.

| 단계 | 확인 | 결과 |
| --- | --- | --- |
| 1 셸 | 사이드바 10 화면(대시보드, 가상 머신, 템플릿, 네트워크, 작업, 이벤트, 증적, 계정, 진단, 도움말)을 한 번씩 전환. 계정 메뉴는 loopback 세션에서 숨김이고 화면 전환은 된다. VM 목록 상태 칩 `all`·`running`·`off`·`template`·`failed`와 행 상태 배지 | PASS (10.6초) |
| 2 옛 주소 | `/index.legacy.html` 404(본문 없음). `/app.js` 200(246 byte), `/styles.css` 200(249 byte), 둘 다 주석만 담고 `Retired legacy Web Console placeholder` 문구가 있다(PR #108) | PASS |
| 3 새 VM 마법사 | 4단계(기본, 자원, 설치와 네트워크, 확인)로 `pcv-it-probe-wizard-1108`을 만들었다. Generation 2, 1 vCPU, 512 MB, disk 8 GB, smoke ISO `artifacts/smoke-media-20261108/pcv-route-parity-smoke-20261108.iso`, 저장 위치 `artifacts/pcv-it-probe-wizard-1108`, 네트워크 Default Switch. `vm.create` succeeded 뒤 `vm.start` succeeded | PASS (45.2초) |
| 4 콘솔 탭 | "창에 맞춤" 눌림 상태(`aria-pressed=true`), "전체 화면" 버튼 사용 가능, 입력 상태 칩 `idle` "콘솔을 클릭하면 입력" | PASS (0.8초) |
| 5 체크포인트 탭 | 켜진 VM에서 `checkpoint.create` succeeded. 스크립트가 VM을 끈 뒤(`vm.poweroff`) 복원 확인 모달을 승인해 `checkpoint.restore` succeeded(켜진 VM에서는 실패한다, BL-0033). 예약은 미리보기 뒤 저장(간격 60분, 보존 1개)했고 reload 없이 바로 보였다(주소 그대로). 미리보기가 입력을 비워서 저장 전에 값을 다시 넣었다(BL-0032). 예약 끄기 뒤 사용·상태·마지막 결과·다음 실행이 "꺼짐", "꺼짐", "-", "-"로 바뀌었다 | PASS (138.6초) |
| 6 복제 모달 | 더 보기 메뉴의 복제 모달은 첫 입력 칸이 이름이다(순서: 이름, VM 루트). 원본을 끄고 체크포인트를 지운 뒤 `pcv-it-probe-clone-1108`로 `vm.clone` succeeded(13초) | PASS (88.8초) |
| 7 삭제 | 이름 확인 모달에 VM 이름을 입력해 `pcv-it-probe-clone-1108`, `pcv-it-probe-wizard-1108` 순으로 지웠다. `vm.delete` 두 건 succeeded | PASS (71.6초) |
| 8 API 확인 | Local API VM 목록 200, `pcv-it-probe-*` 0개 | PASS |

## 화면 캡처

캡처는 `docs/ga-ready/demo/web-console-redesign-20261108/` 아래 PNG다. 1600×1000 CSS 픽셀로 찍었고, 500 KB가 넘는 여덟 장은 저장소
규칙에 맞춰 줄였다(01·08·11·12는 1120×700, 02~06은 1280×800, 07은 960×600, 나머지는 원본). 스크립트는 찍기 전마다 화면의 호스트명, Windows 사용자 이름, 사용자 홈 경로, 사설 IPv4를 `<host>`, `<user>`,
`<home>`, `<host-lan-ip>`로 바꿨다. 바꾼 곳은 07~16에서 장마다 1곳이고 01~06은 0곳이다. Claude가 캡처를 열어 보고 사용자 홈 경로,
LAN IP, 호스트명, token이 보이지 않는 것을 확인했다. 화면의 `D:\data\projects\codex-zone\...`는 저장소 위치이고 사용자 홈 경로가 아니다.

- `01-shell.png`: Single Edge 셸, 가상 머신 화면, 상태 칩(전체 3, 실행 중 1, 꺼짐 2, 템플릿 1, 실패 0)과 상태 배지.
- `02-wizard-basics.png` ~ `05-wizard-confirm.png`: 새 VM 마법사 네 단계와 확인 요약.
- `06-wizard-result.png`: 만들기 뒤 작업 결과.
- `07-vm-started.png`: 켜진 `pcv-it-probe-wizard-1108`과 상세 패널(요약 탭).
- `08-console.png`: 콘솔 탭(창에 맞춤, 실제 크기, 전체 화면, 입력 상태 칩).
- `09-checkpoint-created.png`, `10-checkpoint-restore.png`: 체크포인트 생성과 복원 확인 모달.
- `11-checkpoint-schedule.png`, `12-checkpoint-schedule-cleared.png`: 예약 저장 직후와 예약 끈 뒤.
- `13-clone-modal.png`, `14-clone-result.png`: 복제 모달(이름 첫 칸)과 완료 결과.
- `15-delete-clone.png`, `16-delete-wizard.png`: 삭제 이름 확인 모달.

## 관찰 (report-only)

여덟 단계는 모두 통과했지만, 스크립트가 피해 간 동작이 셋 있다. 셋 다 이 train 적재가 만든 회귀가 아니다. 예약 form은
`bb54287`부터, 복원과 VM 목록은 Hyper-V 쪽 동작이고, 이번 적재에는 `src/DesktopNode.HyperV` 변경이 없다. backlog에 적었고 분류는
사용자 결정이다.

- `BL-0032`: 예약 미리보기를 누르면 화면이 form을 다시 그려 간격·보존 입력이 비워진다. 미리보기 뒤 바로 저장을 누르면
  `PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`로 거절된다.
- `BL-0033`: 켜진 VM에서 복원을 승인하면 `checkpoint.restore`가 원시 `PCV_HYPERV_WMI_METHOD_FAILED`로 실패한다. 화면에 끄라는 안내가 없다.
- `BL-0034`: 반복 실행 중 VM 목록 갱신이 `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE`로 실패하면 지운 VM 행과 작업 상태가 화면에 남았다.
  최종 실행의 판정은 서버 작업 상태와 API 이름 목록으로 했다.

화면 다듬기 거리는 `BL-0031`에 적었다.

- 체크포인트 탭의 마지막 결과·다음 실행 칸과 체크포인트 목록의 만든 시각이 `2026-11-08T08:29:29.5052714+00:00` 같은 원시 ISO 문자열이다. 좁은
  칸에서 여러 줄로 꺾인다.
- 체크포인트 예약 미리보기가 원시 JSON(`{"action":"preview","dry_run":true,...}`)을 그대로 보여 주고, 패널 오른쪽 밖으로 넘친다.
- 상세 패널이 좁아서 체크포인트 행의 "복원…", "삭제…" 버튼 글자가 세로로 꺾인다. 상세 패널을 아래로 스크롤하면 왼쪽 목록 자리가 빈다.
- 복제 모달에서 "새 VM 이름" 레이블이 입력 칸에 겹치고, VM 루트 칸이 잘려 보이며, "미리보기"와 "복제 시작" 버튼이 붙어 있다. 미리보기가
  복사 크기를 `4194304바이트`로 보여 준다.
- 상단 KPI 줄(VM 실행, 작업, Critical, 호스트, 증적)이 VM 화면에서 모두 "—"다.
- 삭제 모달 문구가 체크포인트가 없을 때도 "checkpoint 0개도 함께 지워집니다"다.

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |
| 확인 방법 | |

확인자 칸이 비어 있으면 이 기록은 "에이전트 자가 보고"다(2026-10-09 감사 §10).

## 정리

- 만든 VM `pcv-it-probe-wizard-1108`, `pcv-it-probe-clone-1108`은 Web Console에서 지웠고, `artifacts/` 아래 두 폴더도 지웠다. 끝 상태
  `Get-VM`에 `pcv-it-probe-*`는 0개다.
- 보존 VM 세 개는 시연 전과 같다. service Running/Automatic, Web 200, PureCVisor 방화벽 규칙 0개다. Rollback은 없다.

## 판정

- Task 5 probe PASS. `pcvverify completion`은 돌리지 않았고 완료를 주장하지 않는다. operational current는 `0.42.96-admin-smoke` 그대로다.

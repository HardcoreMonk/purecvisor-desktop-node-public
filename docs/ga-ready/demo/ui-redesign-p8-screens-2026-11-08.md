# UI redesign P8 화면 기록 (2026-11-08)

- 시나리오: 해당 없음. 이 기록은 새 셸 브라우저 품질 게이트의 기본 테마 화면을 남긴다.
- 기준: `web/scripts/verify-browser-ui-gates.mjs`, axe-core 4.14.0, 태그 `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`.
- mock API 고정 데이터로 찍은 것이고 설치본 시연이 아니다. 설치본 반영과 S1~S3 재시연은 release train 뒤다.
- 운영 current는 `0.42.96-admin-smoke` 그대로다. 이 기록은 public trusted signing이나 외부 stable publication을 주장하지 않는다. S4는 주장하지 않는다.
- 판정: 화면 26장. 테마 `supanova`와 `supanova-cyan` 모두 axe 위반 0, `incomplete` `color-contrast`는 기준선 1893건과 같다.
- 기본 테마는 Part 2 `:root` 팔레트 그대로라 어둡게 그려진다. `supanova`의 강조 버튼 글자만 `#0a0f1a`로 두어 `#ffffff` / `#22d3ee` 위반이 다시 나오지 않게 했다.

## 실행

- 명령: `node web/scripts/verify-browser-ui-gates.mjs --screenshots docs/ga-ready/demo/ui-redesign-p8-20261108`
- 기준선 갱신: `node web/scripts/verify-browser-ui-gates.mjs --write-baseline`. 기본은 합집합이다. 이번 실행의 incomplete key를 기존 key에 더하고, 같은 key의 count는 줄이지 않는다. reason은 `(kind, rule, messageKey)`로 이어받는다. 이번 실행에 없는 key는 남긴다. `--prune`을 같이 주면 그 key를 지운다. 출력은 `기준선 N→M 추가 A 제거 B`다. 위반이 있거나 단언이 실패하면 파일을 쓰지 않는다. 새 messageKey의 reason은 `TODO: 눈으로 확인 뒤 이유를 적는다`이고, TODO가 있으면 일반 실행이 실패한다. 기준선에 없는 새 incomplete는 실패다. 기준선에만 있는 항목은 경고로 출력하고 summary에 개수를 남기며 실패하지 않는다. 마지막 줄에 `platform`과 브라우저 이름을 쓴다.
- 브라우저: `msedge.exe`. 헤드리스, viewport 1280×720, `deviceScaleFactor` 1, `--lang=ko`.
- 결과: `ok=true`, `elapsed_ms=22487`, 단언 121, 실패 0, 위반 0, `incomplete` 1893, 기준선 1893. 산출 `artifacts/browser-ui-gates/20261107T182238Z/summary.json`.
- 캡처는 각 화면의 두 테마 검사 뒤에 기본 테마 `supanova`로 되돌린 다음 한 장이다.
- 저장 PNG는 같은 1280×720 캡처를 768×432로 줄이고 64색으로 양자화했다. 레이아웃을 다시 흐르지 않았다. 합계가 3 MB 아래로 맞추기 위한 축소다.
- `origin/main`의 `web`과 같은 8화면을 두 테마로 비교했다. 비교 PNG는 `artifacts/browser-ui-compare/`에만 있다. `supanova-cyan`은 dashboard, vms, vm-detail-summary, vm-detail-checkpoints, wizard-1, accounts가 픽셀 차이 0이다. clone-modal 0.1313%, delete-confirm 0.0666%는 대화상자 채움과 글자 가장자리다. `supanova` 차이는 강조 버튼 글자색이다. dashboard 0.0573%, vms 0.1319%, vm-detail-summary 0.1332%, vm-detail-checkpoints 0.1332%, wizard-1 0.1277%, accounts 0.2818%, clone-modal 0.4047%, delete-confirm 0.3140%.
- VM 이름은 `pcv-demo-*`다. 삭제 확인 입력칸의 `not-the-vm`은 이름이 다르면 삭제가 켜지지 않는지 보는 값이다. 마법사 3단계 ISO 칸의 `D:\iso\installer.iso`는 제품 placeholder를 칸에 넣은 것이고, 이 호스트의 경로가 아니다. 화면과 이 기록에 사용자 홈 경로, LAN 사설 IP, 호스트명, token은 없다.

## 화면 캡처

캡처 디렉터리는 `docs/ga-ready/demo/ui-redesign-p8-20261108/`다. 파일 26개, 합계 2,677,902바이트.

- `auth-login.png`: 로그인 오버레이. 계정 이름, 비밀번호, 로그인, 첫 계정 생성.
- `dashboard.png`: 사이드바와 운영 대시보드. 상단 바 이름은 `loopback-session`.
- `vms.png`: 가상 머신 목록. `pcv-demo-running`, `pcv-demo-off`, `pcv-demo-failed`, `pcv-demo-template`.
- `vm-detail-summary.png`: `pcv-demo-off` 상세 요약 탭.
- `vm-detail-console.png`: 같은 VM의 콘솔 탭.
- `vm-detail-checkpoints.png`: 체크포인트 탭. `pcv-demo-baseline`.
- `vm-detail-hardware.png`: 하드웨어 탭.
- `vm-detail-guest.png`: 게스트 탭.
- `vm-detail-jobs.png`: VM 작업 탭.
- `vm-more-menu.png`: 더 보기 메뉴가 열린 상태.
- `clone-modal.png`: 복제 대화상자.
- `delete-confirm.png`: 삭제 확인. 입력값이 VM 이름과 달라 삭제가 꺼져 있다.
- `checkpoint-restore-confirm.png`: 체크포인트 복원 확인. 이름 입력칸은 없다.
- `wizard-1.png`: 새 VM 마법사 1단계. 이름 `pcv-demo-new`.
- `wizard-2.png`: 2단계 자원.
- `wizard-3.png`: 3단계 설치와 네트워크. ISO 칸은 제품 placeholder다.
- `wizard-4.png`: 4단계 확인.
- `templates.png`: 템플릿.
- `accounts.png`: 계정. `pcv-demo`, `pcv-demo-operator`.
- `password-reset.png`: `#account-reset-form` 비밀번호 재설정.
- `network.png`: 네트워크 인벤토리. Default Switch.
- `jobs.png`: 작업.
- `activity.png`: 이벤트 센터.
- `evidence.png`: 증적.
- `troubleshooting.png`: 진단.
- `helppage.png`: 도움말.

같은 실행에서 위 26화면의 axe 위반 수는 테마마다 0이다. `incomplete` `color-contrast` 합은 1893이고 기준선과 같다.

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |

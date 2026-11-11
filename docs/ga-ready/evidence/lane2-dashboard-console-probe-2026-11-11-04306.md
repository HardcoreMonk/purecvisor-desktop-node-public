# Lane 2 probe: 설치본 대시보드 첫 로드와 브라우저 콘솔 입력 (train 0.43.6, 2026-11-11, FAIL)

- evidence id: `lane2-dashboard-console-probe-2026-11-11-04306`
- train: `0.43.6-admin-smoke`(campaign `train-04306-20261111`, plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04306.md` Task 6)
- 대상: queue 152(대시보드 첫 로드), 153(콘솔 입력 순서), 157(콘솔 route 전역 lock 밖, loopback 세션 하나). 승인 1의 probe 범위다.
- 설치본: `0.43.6-admin-smoke+bfb30c1`(fullgate `full-admin-host-mutation-gate-20261111-04306` 뒤). loopback 전용, 계정은 Task 2에서 보관 중(loopback 모드)이다.
- host mutation: 콘솔 입력은 `pcv-it-s1r-ubuntu`(Ubuntu 설치 프로그램 사용자 정보 화면)에 보냈다. 새 VM은 만들지 않았다. 비밀번호와 token은 기록하지 않았다.
- 시각: 2026-11-11 19:16~19:25 KST
- 판정: **FAIL**. 두 가지 모두 크게 나아졌지만 통과 조건을 채우지 못했다. train을 정차한다.

## 내주는 파일

`app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload와 같다. `sw.js` CACHE_NAME `pcv-ui-v0256f88d`도 payload commit `ee821cbd` 소스와 같다.

## 브라우저 콘솔 입력 (queue 153, 157)

| 항목 | train 0.43.5 | train 0.43.6 |
| --- | --- | --- |
| `console/input` 요청 시간 중앙값 | 2,429ms | 168ms(n=202), 169ms(n=84) |
| p95 | 2,666ms | 174ms, 177ms |
| 처리 | 165초에 55건, 나머지는 큐에서 사라짐 | 202건 모두 처리(입력 큐가 끝까지 비워짐) |
| 2xx가 아닌 응답 | 0 | 30건(첫 실행), 0건(둘째 실행) |
| 20자 두 칸 입력 일치 | 판단 못 함 | **불일치**(비밀번호 칸 약 26자, 확인 칸 비어 있음) |

- 첫 실행(Backspace 30번, 20자, 아래 칸 이동, 20자, 텍스트 폼): 약 1분 동안 입력 요청 146건과 콘솔 frame 요청이 겹쳤다.
  - 30건이 거절됐다. 그 안에 확인 칸으로 내려가는 키와 글자 일부가 있었다. 그래서 두 번째 20자도 비밀번호 칸에 들어가고 일부 글자가 빠졌다.
- 둘째 실행(Backspace 40번, 1분에 약 100건): 84건 모두 성공했다.
- 원인: Local API의 전역 요청 한도(클라이언트당 분당 120건 + burst, `DesktopNodeApiRequestThrottle`)가 콘솔 frame·input 요청도 센다.
  - 콘솔 frame 요청만 해도 1분에 수십 건이다. 거기에 키 하나에 요청이 2건 붙는다.
  - 요청 시간이 빨라진 #157 뒤에는 사람이 비밀번호를 두 번 치는 정도로도 한도를 넘는다. 0.43.5에서는 요청이 느려 한도에 닿지 않았다.
- 텍스트 보내기 폼은 같은 실행의 뒤쪽이라 판정하지 않았다.

## 대시보드 첫 로드 (queue 152, 157, loopback 모드)

Playwright로 1440×900과 768×1024에서 새 context를 세 번씩 열고 15초 동안 보았다.

| 화면 | 회 | loopback 세션 POST | `ops/summary` 요청 | 그중 취소 | `ops/summary` `200` | 목록(VM 3개) | 첫 갱신 끝 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1440 | 1 | 1 | 2 | 1 | 6.5초 | 8.5초 | 9.6초 |
| 1440 | 2 | 1 | 2 | 1 | 6.3초 | 8.5초 | 9.3초 |
| 1440 | 3 | 1 | 2 | 1 | 6.3초 | 8.6초 | 9.4초 |
| 768 | 1 | 1 | 2 | 1 | 6.4초 | 8.5초 | 9.3초 |
| 768 | 2 | 1 | 2 | 1 | 6.4초 | 8.6초 | 9.4초 |
| 768 | 3 | 1 | 2 | 1 | 6.4초 | 8.5초 | 9.6초 |

- 비교: train 0.43.5는 loopback POST 2회 이상, `ops/summary` 3회·취소 2회, 목록 약 12.1초였다. train 0.43.4 목록은 9.2~10.2초였다.
  - loopback 세션 중복은 고쳐졌다.
  - 그러나 `ops/summary` 취소 1회가 남아 통과 조건(1회, 취소 0회)을 채우지 못했다.
- CDP initiator trace(`artifacts/lane2-04306-probe-20261111/trace-loopback/`)
  - 첫 갱신이 118ms에 `ops/summary`를 보냈다.
  - 479ms에 두 번째 갱신(`runRefreshAll`)이 같은 요청을 다시 보내며 첫 요청을 취소했다. 두 번째 갱신을 시작시킨 호출은 비동기 경계 너머라 스택에 보이지 않는다.
  - loopback 모드의 `ops/summary`는 5.8초 걸렸다(서버 전역 lock 안의 다른 요청과 줄을 선다).
- 회귀 없음: version `v0.43.6-admin-smoke`, HTTP 4xx 0, console error 0, "운영 요약을 읽지 못했습니다" 0

## Nonclaims

- operational current는 `0.43.4-admin-smoke` 그대로다. 이 FAIL은 current에 쓰지 않는다.
- 디자인 감사나 접근성 판정을 새로 하지 않았다.

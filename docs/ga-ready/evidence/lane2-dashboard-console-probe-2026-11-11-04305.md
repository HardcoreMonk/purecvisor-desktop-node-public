# Lane 2 probe: 설치본 대시보드 첫 로드와 브라우저 콘솔 입력 (train 0.43.5, 2026-11-11, FAIL)

- evidence id: `lane2-dashboard-console-probe-2026-11-11-04305`
- train: `0.43.5-admin-smoke`(campaign `train-04305-20261111`, plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04305.md` Task 6)
- 대상: queue 152(대시보드 첫 로드)와 153(브라우저 콘솔 입력, BL-0046·BL-0047). 승인 1의 probe 범위다.
- 설치본: `0.43.5-admin-smoke+8173777`(fullgate `full-admin-host-mutation-gate-20261111-04305` 뒤). loopback 전용이고 계정은 Task 2에서 보관 중이다(loopback 세션).
- host mutation: 콘솔 입력은 `pcv-it-s1r-ubuntu`(Ubuntu 설치 프로그램 사용자 정보 화면)에 보냈다. 새 VM은 만들지 않았다. 비밀번호와 token은 기록하지 않았다.
- 시각: 2026-11-11 16:07~16:16 KST
- 판정: **FAIL**. 두 변경 모두 설치본에서 목표한 개선을 보이지 않았다. train을 정차한다.

## 내주는 파일

`http://127.0.0.1/`이 내주는 `app.bundle.js`, `index.html`, `style.css`, `sw.js`, `i18n.js`의 SHA-256이 Task 1 package payload와 같다. `sw.js` CACHE_NAME `pcv-ui-vb668651a`도 payload commit `91009557` 소스와 같다.

## 대시보드 첫 로드 (queue 152)

Playwright로 1440×900과 768×1024에서 새 browser context를 세 번씩 열고 `#/dashboard`를 15초 동안 보았다.

| 화면 | 회 | `ops/summary` 요청 | 그중 취소 | `ops/summary` `200` | 첫 갱신 끝(`loading=false`) | "운영 요약을 읽지 못했습니다" |
| --- | --- | --- | --- | --- | --- | --- |
| 1440 | 1 | 3 | 2 | 10.3초 | 13.2초 | 없음 |
| 1440 | 2 | 3 | 2 | 10.0초 | 12.9초 | 없음 |
| 1440 | 3 | 3 | 2 | 10.1초 | 12.9초 | 없음 |
| 768 | 1 | 3 | 2 | 10.0초 | 12.9초 | 없음 |
| 768 | 2 | 3 | 2 | 10.0초 | 12.9초 | 없음 |
| 768 | 3 | 3 | 2 | 10.1초 | 13.2초 | 없음 |

- 30초 시간선(1440, 새 context 하나)
  - 0.3초: 읽는 중 문구가 보인다(BL-0044 유지).
  - 10.8초: VM 3개가 화면 상태에 들어온다.
  - 12.1초: 첫 갱신 결과가 그려지고 경고 카드가 목록이 된다(`connected`).
  - 12.9초: 나머지 단계가 끝난다.
- 기준은 train `0.43.4` probe다. 그때 목록은 9.2~10.2초에 그려졌고 `ops/summary` 취소는 2~3회였다. 이번에는 목록이 약 2초 늦고, 취소 2회가 남았다. node 지연 mock 시험(3,884ms → 1,622ms)의 개선이 설치본 흐름에서는 나타나지 않았다.
- 회귀 없음
  - 셸 version `v0.43.5-admin-smoke`
  - HTTP 4xx 0, console error 0
  - "운영 요약을 읽지 못했습니다" 한 번도 없음
  - 읽는 중 문구는 0.3~0.4초부터 보임

## 브라우저 콘솔 입력 (queue 153)

`pcv-it-s1r-ubuntu`의 콘솔 탭에서 사용자 정보 화면을 썼다. 입력은 Web Console 키 처리기(콘솔 canvas의 keydown/keyup)와 텍스트 보내기 폼으로 보냈다. Web Console의 `console/input` 요청 시간은 브라우저 안에서 쟀다.

| 항목 | 값 |
| --- | --- |
| `console/input` 요청 시간(n=55) | 중앙값 2,429ms, p95 2,666ms, 최대 6,848ms, 2xx 아닌 응답 0 |
| 기준(`0.43.4`) | 요청당 약 2초 |
| 사람이 치는 속도(키 사이 약 0.3초)로 Backspace 60번과 20자 두 번을 보냈을 때 | 165초 동안 요청 55건만 처리됐다. 나머지는 Web Console 입력 큐에 남아 있다가 브라우저를 닫을 때 사라졌다 |
| 20자 두 칸 입력 일치 | 판단할 수 없음. 64초 뒤 캡처에서 확인 칸은 아직 지워지는 중이었다 |
| 텍스트 보내기 폼(`pcv`, `pcvs1r`) | 판단할 수 없음. 앞선 키 입력 큐 뒤에 있었다 |

- #153은 같은 VM 입력을 도착 순서대로 한 번에 하나씩 처리하게 했다. 그래서 순서는 지켜지지만, 요청 하나가 약 2.4초라 사람이 치는 속도를 따라가지 못한다.
- 원인 후보: #153 리뷰가 report-only로 남긴 adapter의 키마다 `GetVms()` 조회. 키보드 경로 캐시만으로는 요청 시간이 줄지 않았다.

## 도구 관찰 (report-only)

- 시나리오 CDP 도우미(PR #153 수정 포함)로 실행할 때마다 Edge 8~13개가 종료 뒤에도 남았다. 실행마다 profile 표식으로 끄고 확인했다(끝에 0개).
- 저장소 밖 콘솔 도우미가 loopback 모드에서 로그인 화면을 먼저 보고 계정 로그인을 한 번 시도했다(그 계정은 보관 중이라 실패했을 수 있다). 판정 시간을 늘려 고쳤다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke` 그대로다. 이 FAIL은 current에 쓰지 않는다.
- 디자인 감사나 접근성 판정을 새로 하지 않았다.

# Lane 2 probe: 설치본 Web Console의 backlog counts 수정 (train 0.43.1, 2026-11-10)

- evidence id: `lane2-backlog-counts-probe-2026-11-10-04301`
- train: `0.43.1-admin-smoke`(campaign `train-04301-20261110`, plan `docs/superpowers/plans/2026-11-10-purecvisor-desktop-node-train-04301.md` Task 6)
- 대상: queue 134(backlog counts BL-0036~BL-0041). 승인 원문의 `probe(Web Console 화면)` 범위다.
- 설치본: `0.43.1-admin-smoke+143a55b`(fullgate `full-admin-host-mutation-gate-20261110-04301` 뒤), loopback 전용, 계정 없음(Task 2 보관 중, loopback 세션 `200`)
- host mutation: 없음. VM을 만들거나 바꾸지 않았다. 보존 VM은 읽기만 했다. 브라우저 `localStorage`의 테마 값은 확인 뒤 원래 값(`supanova-cyan`)으로 되돌렸다.
- 시각: 2026-11-10 17:03~17:20 KST

## 내주는 파일

`http://127.0.0.1/`이 내주는 파일 다섯 개의 SHA-256이 Task 1 package payload(`artifacts/admin-smoke-package-20261110-04301/payload/web/`)와 payload commit `c685bd51`의 소스와 같다.

| 파일 | SHA-256 앞 12자리 | 결과 |
| --- | --- | --- |
| `app.bundle.js` | `17d86f7e13d9` | 같음 |
| `index.html` | `af6605014be1` | 같음 |
| `style.css` | `c857953a12c9` | 같음 |
| `sw.js` | `696519495b2e` | 같음 |
| `i18n.js` | `9c1f0e50d103` | 같음 |

`sw.js` CACHE_NAME은 `pcv-ui-v6c9f22e6`로 소스와 같다.

## 수정 여섯 건

Playwright 브라우저로 1440×900과 768×1024에서 확인했다. 캡처 28장은 `artifacts/lane2-backlog-counts-probe-20261110-04301/`에 있다(커밋하지 않음).

| 항목 | 확인 방법 | 결과 |
| --- | --- | --- |
| BL-0040 첫 방문 401 | 새 browser context(저장소 비어 있음)로 `#/dashboard`를 열고 `/api/v1/` 요청 순서와 응답을 기록 | 1440과 768 모두 `POST /api/v1/auth/loopback-session`이 `GET /api/v1/runtime/policy`보다 먼저다. policy 요청은 Bearer token을 가진다. HTTP 4xx·5xx 0, console error 0 |
| BL-0036 셸 검색 버튼 | 768 폭에서 `.shell-search`의 접근성 트리 | `button "검색"`. 이름이 정확히 "검색"인 button 1개 |
| BL-0041 진단 서비스 값 | 진단 카드 4개의 글자 줄 | 서비스 값 "실행 중". 값이 `true`·`false`인 줄 0 |
| BL-0037 좁은 VM 목록 | 1440에서 S2 template을 `#/vms/<vm>/summary`로 고른 뒤 목록 | `.vm-workspace.is-detail`, 목록 2행 모두 이름 전체(말줄임 없음), 상태 점과 화면 읽기용 상태 `stopped`, S2 template 행에만 템플릿 배지. 3열과 7열 숨김. 768에서는 상세만 보이는 기존 배치다 |
| BL-0039 고대비·mockup 테마 | 저장값을 바꾸고 다시 읽은 뒤 `:root` 값 | 아래 표 |
| BL-0038 삭제 확인 gate | 해당 없음 | browser gate 시험만 바꿨으므로 설치본 확인 대상이 아니다 |

BL-0039 테마 값(1440, 저장값을 바꾼 뒤 페이지 다시 읽기):

| 저장값 | 적용된 `data-theme` | 다시 쓴 저장값 | `--bg` | `--fg` | `--accent` |
| --- | --- | --- | --- | --- | --- |
| `supanova-cyan` | `supanova-cyan` | `supanova-cyan` | `#0a0f1a` | `#e0f0ff` | `#22d3ee` |
| `supanova-hicontrast` | `supanova-hicontrast` | `supanova-hicontrast` | `#030405` | `#ffffff` | `#facc15` |
| `supanova-mockup` | `supanova-cyan` | `supanova-cyan` | `#0a0f1a` | `#e0f0ff` | `#22d3ee` |
| `contrast` | `supanova-hicontrast` | `supanova-hicontrast` | `#030405` | `#ffffff` | `#facc15` |

body 배경과 글자색도 고대비에서 `rgb(3, 4, 5)`, `rgb(255, 255, 255)`로 바뀐다.

## 나머지 화면

| 화면 | 1440 | 768 | 확인한 것 |
| --- | --- | --- | --- |
| 대시보드 | 보임 | 보임 | KPI 카드 6개 |
| 가상 머신 | 보임 | 보임 | VM 2개(보존 VM, S2 template) |
| VM 상세 | 보임 | 보임 | deep link로 상세 패널 |
| 네트워크, 작업, 이벤트, evidence, 계정, 템플릿, 도움말 | 보임 | 보임 | 등록표·옛 `render()` 화면이 그려짐 |
| 진단 | 보임 | 보임 | 카드 4개 |
| 명령 팔레트 | 열림 | 열림 | `Ctrl+K`, 항목 15개 |

- 모든 화면에서 옛 `#command-palette`가 없고, 가로 스크롤이 없다.
- 화면을 도는 동안 console error 0, HTTP 4xx·5xx 응답 0이다.

## 확인 중 고친 probe 실수

- 처음 테마 확인은 같은 URL(`#/dashboard`)로 `goto`해서 페이지가 다시 읽히지 않았다(hash만 같은 문서 이동). 그래서 저장값이 적용되지 않은 것처럼 보였다. `reload()`로 다시 확인한 값이 위 표다. 제품 문제는 아니다.

## 범위 밖 발견 (report-only)

- version 표시(`#version-badge`)가 설치본에서 보이지 않는다. `updateVersionBadge()`는 `GET /api/v1/runtime/policy`의 `version`·`product_version`·`service_version`을 읽지만, 이 응답의 `data`에는 `runtime_core`, `job_runtime`, `auth`, `network`, `console`, `guest_execution`만 있다. BL-0040 수정 전에는 이 요청이 401로 끝나 같은 결과였다. 이번 train이 만든 회귀가 아니다. backlog `BL-0042`.
- 첫 방문에서 `POST /api/v1/auth/loopback-session`이 2~4번, `GET /api/v1/runtime/policy`가 3~4번 나간다. 모두 성공했고 오류는 없다. 기록만 한다.

## Nonclaims

- 디자인 감사나 접근성 판정을 새로 하지 않았다(Web gate가 한다).
- operational current는 이 probe 때 `0.43.0-admin-smoke`다.

# Lane 2 probe: 설치본 Web Console 화면 (train 0.43.0, 2026-11-10)

- evidence id: `lane2-ui-parity-probe-2026-11-10-04300`
- train: `0.43.0-admin-smoke`(campaign `train-04300-20261110`, plan `docs/superpowers/plans/2026-11-10-purecvisor-desktop-node-train-04300.md` Task 6)
- 대상: queue 123~130(ADR-0020 UI 개편 U1~U8). 승인 원문의 `probe(Web Console 화면)` 범위다.
- 설치본: `0.43.0-admin-smoke+1f646ee`(fullgate `full-admin-host-mutation-gate-20261110-04300` 뒤), loopback 전용, 계정 없음(Task 2 보관 중, loopback 세션 `200`)
- host mutation: 없음. VM을 만들거나 바꾸지 않았다. 보존 VM은 읽기만 했다.
- 시각: 2026-11-10 07:15~07:25 KST

## 내주는 파일

`http://127.0.0.1/`이 내주는 파일 다섯 개의 SHA-256이 Task 1 package payload(`artifacts/admin-smoke-package-20261110-04300/payload/web/`)와 payload commit `6355e6ba`의 소스와 같다.

| 파일 | SHA-256 앞 12자리 | 결과 |
| --- | --- | --- |
| `app.bundle.js` | `4b0cd22755ca` | 같음 |
| `index.html` | `1642a06e5b8d` | 같음 |
| `style.css` | `1c17a3ba3963` | 같음 |
| `sw.js` | `c239f8295b5a` | 같음 |
| `i18n.js` | `30ed366e37a6` | 같음 |

`sw.js` CACHE_NAME은 `pcv-ui-va5dc518c`로 소스와 같다.

## 화면

Playwright 브라우저로 1440×900과 768×1024에서 화면 열 개, VM 상세, 명령 팔레트를 열었다. 캡처 24장은 `artifacts/lane2-ui-parity-probe-20261110-04300/`에 있다(커밋하지 않음).

| 화면 | 1440 | 768 | 확인한 것 |
| --- | --- | --- | --- |
| 대시보드 | 보임 | 보임 | KPI 카드 6개 모두 `dashboard-kpi-card` class 유지(U8 수정), pagehead `+ 새 VM` 버튼 모양 유지, 워크로드 표 VM 2행 |
| 가상 머신 | 보임 | 보임 | VM 2개(보존 VM, S2 template), 목록·상세 배치 |
| VM 상세 | 보임 | 보임 | `#/vms/<vm>/summary` deep link로 상세(이름, 시작·콘솔·더 보기, 탭 6개, 요약 값) |
| 네트워크 | 보임 | 보임 | 등록표 mount 3개 |
| 작업 | 보임 | 보임 | 등록표 mount 3개 |
| 이벤트 | 보임 | 보임 | 등록표 mount 3개 |
| 진단 | 보임 | 보임 | 카드 4개, 각 카드 `display: grid`(라벨·값 분리, U8 수정) |
| evidence | 보임 | 보임 | 등록표 mount 3개 |
| 계정 | 보임 | 보임 | 등록표 mount 1개(계정 없는 loopback 상태) |
| 템플릿 | 보임 | 보임 | 옛 `render()` 경로 |
| 도움말 | 보임 | 보임 | 옛 `render()` 경로 |
| 명령 팔레트 | 열림 | 열림 | `Ctrl+K`, `#cmd-input` `role=combobox`, 항목 15개 |

- 모든 화면에서 옛 `#command-palette`가 없고, 가로 스크롤이 없다.
- 화면을 도는 동안 console error 0, HTTP 4xx·5xx 응답 0이다.

## 범위 밖 발견 (report-only)

- 새 브라우저의 첫 방문에서 `GET /api/v1/runtime/policy`가 한 번 `401`을 받고 console error로 남는다. 그 뒤 화면은 정상이고, 같은 브라우저의 다시 읽기에서는 401이 없다. backlog `BL-0040`.
- 진단의 "서비스와 수신기" 카드가 서비스 값을 `true`로 그대로 보인다. backlog `BL-0041`.

## Nonclaims

- 디자인 감사나 접근성 판정을 새로 하지 않았다(Web gate가 한다).
- operational current는 이 probe 때 `0.42.99-admin-smoke`다.

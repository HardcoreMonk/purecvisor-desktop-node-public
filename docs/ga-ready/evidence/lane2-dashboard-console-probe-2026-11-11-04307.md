# Lane 2 probe: 대시보드 첫 로드와 브라우저 콘솔 키 입력 (train 0.43.7, 2026-11-11)

- evidence id: `lane2-dashboard-console-probe-2026-11-11-04307`
- campaign: `train-04307-20261111`(plan `docs/superpowers/plans/2026-11-11-purecvisor-desktop-node-train-04307.md` Task 6)
- 대상: queue 152, 153, 157, 159(train `0.43.7-admin-smoke` 적재 전부)
- 설치본: train fullgate build `0.43.7-admin-smoke+3898139`(product source는 payload `bf5c214`와 같다). 계정은 Task 2에서 보관 중이라 loopback 모드다.
- 시각: 2026-11-11 22:34~22:37 KST

## 내주는 파일

| 파일 | 설치본이 내주는 파일 == Task 1 package payload |
| --- | --- |
| `app.bundle.js` | 같음 |
| `index.html` | 같음 |
| `style.css` | 같음 |
| `sw.js` | 같음. CACHE_NAME `pcv-ui-v3b8eda95`는 payload commit 소스와 같다 |
| `i18n.js` | 같음 |

## 대시보드 첫 로드 (loopback 모드, 새 context 여섯 번)

| 화면 | 회 | loopback 세션 POST | 문서 로드 | `ops/summary` 요청 | 취소 | `ops/summary` `200` | 목록(VM 3개) | 첫 갱신 끝 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1440 | 1 | 1 | 1 | 1 | 0 | 3.2초 | 5.7초 | 6.8초 |
| 1440 | 2 | 1 | 1 | 1 | 0 | 2.7초 | 5.2초 | 6.0초 |
| 1440 | 3 | 1 | 1 | 1 | 0 | 2.8초 | 5.2초 | 6.0초 |
| 768 | 1 | 1 | 1 | 1 | 0 | 2.9초 | 5.3초 | 6.1초 |
| 768 | 2 | 1 | 1 | 1 | 0 | 2.8초 | 5.2초 | 6.3초 |
| 768 | 3 | 1 | 1 | 1 | 0 | 2.7초 | 4.9초 | 6.0초 |

- 판정: **통과**. 매번 `ops/summary` 1회, 취소 0회, 문서 로드 1회(첫 방문 reload 없음)다.
- 목록이 그려지는 시각 비교(train probe 기준): `0.43.4` 9.2~10.2초, `0.43.5` 약 12.1초, `0.43.6` 8.5초, 이번 4.9~5.7초(dev probe 4.3~5.1초)
- 회귀 없음: version `v0.43.7-admin-smoke`, HTTP 4xx 0, console error 0, "운영 요약을 읽지 못했습니다" 0

## 브라우저 콘솔 키 입력 (`pcv-it-s1r-ubuntu`, Ubuntu 설치 프로그램 사용자 정보 화면)

Web Console 콘솔 canvas의 키 처리기로 사람이 치는 속도로 보냈다. 입력 큐가 빈 뒤(1.6초) 판정했다.

| 항목 | 값 | 판정 |
| --- | --- | --- |
| `console/input` 요청 | 188건, 중앙값 176ms, p95 189ms, 최대 405ms | 통과(기준 0.5초 이하, `0.43.5` 2,429ms) |
| 거절된 요청(2xx 아님) | 0건 | 통과(`0.43.6` 30건) |
| 같은 새 20자를 두 비밀번호 칸에(Backspace 50번 포함) | 두 칸 모두 20자, "Passwords do not match" 없음 | 통과 |

- 텍스트 보내기 폼은 승인 문장대로 판정에서 뺐다(BL-0047, 같은 campaign Task 18).
- 비밀번호는 실행 경계에서 만들었고 기록하거나 저장하지 않았다. 캡처는 `artifacts/lane2-04307-probe-20261111/`(커밋하지 않음)에 있다.

## 끝 상태

- 설치본 `0.43.7-admin-smoke+3898139`, service Running/Automatic, Web `200`, loopback 모드(계정은 Task 7에서 복원)
- `pcv-it-s1r-ubuntu` Running(사용자 정보 화면, 두 비밀번호 칸 일치), 보존 VM 둘 Off
- 남은 probe Edge 0개

## Nonclaims

- 이 probe는 설치본이 #152·#153·#157·#159를 내주는지 본다. 디자인 감사나 접근성 판정을 새로 하지 않았다.
- `current-evidence.json`을 쓰지 않았다(Lane 3에서 쓴다).

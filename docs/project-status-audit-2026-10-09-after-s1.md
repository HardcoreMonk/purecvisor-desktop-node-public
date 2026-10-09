# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-10-09, S1 통과 뒤)

- 기준: `main` `9b6c0e2`(PR #76 merge). operational current `0.42.93-admin-smoke`.
- 판정: `pcvverify completion`(완료 정의 v3) 읽기 전용. `complete=false met=3/7 gaps=4 head=9b6c0e28b5c66741f4a6df64bbc34d0d4d2559ed`.
- 직전 같은 날 snapshot: `docs/project-status-audit-2026-10-09.md`(`main` `6b76274`, `met=1/7`, 시연 시작 전).

## 1. 결론

S1은 이 호스트 dev probe에서 시연됐고 criteria는 `passed`다. 완료는 아니다. 남은 판정 갭은 S2, S3, S4와 C5의 `ubuntu-26-runner` 기한(2026-10-19)이다. C1의 Development Gates와 C5의 두 CI workflow는 이 `main` SHA에서 success다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | Development Gates success |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족 | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 template 복제 | 미착수 | 시연 기록 없음 |
| S3 checkpoint를 브라우저에서 | 미착수 | 시연 기록 없음 |
| S4 LAN에서 S1~S3 | 미착수 | 시연 기록 없음 |
| C5 CI와 기한 위험 | 미충족 | CI는 success. `ubuntu-26-runner`가 2026-10-19까지 열림 |
| C6 영구 범위 밖 | 충족 | 5개 항목 |

운영 위생(판정 밖): C2·C3·C4 충족. C7은 `BL-0011`, `BL-0012`가 `undecided`다.

## 3. 이 호스트

- 설치본 manifest `0.42.93-admin-smoke`. S1 dev probe `0.42.94-admin-smoke`는 제품 Rollback으로 이 version에 돌아왔다. 승격 근거가 아니다.
- service `PureCVisorDesktopNode` Running/Automatic. Web `http://127.0.0.1/` HTTP 200.
- `pcv-it-` VM 0개. 보존 VM `pcv-guest-installed-04253-r1` Off.

## 4. 남은 일

| 항목 | 다음 행동 |
| --- | --- |
| campaign `s1-installed-20261008` Task 11 | 2026-10-19 이후 C5 runner 확인 |
| S2, S3, S4 | 시나리오별 campaign 승인 |
| `BL-0011`, `BL-0012` | 사용자 분류 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`일 때만 적는다.
- public trusted signing과 external stable publication을 주장하지 않는다.

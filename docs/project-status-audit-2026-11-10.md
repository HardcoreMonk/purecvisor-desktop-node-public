# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-11-10, UI 개편 train 승격)

- 기준: `main` `a8e2b57`(PR #131 merge). operational current는 `0.43.0-admin-smoke`다.
- 판정: clean `main`에서 `pcvverify completion`(완료 정의 v3, ADR-0017)을 실행했다.
  - 결과: `complete=true met=7/7 gaps=0 head=a8e2b57d0e2ed08843a1437c632bf8627429a956`
  - 결과 파일: 로컬 `artifacts/completion/20261110/result.json`(gitignore). 입력 CI 목록은 같은 폴더의 `main-runs.json`이다.
- 직전 감사: `docs/project-status-audit-2026-11-09.md`(`main` `67d3bbe`, `met=7/7`, 운영 위생 네 조건 충족).

## 1. 결론

완료 정의 v3의 일곱 조건이 계속 충족되고, 판정 갭은 0이다. ADR-0020 Web Console UI 개편(U1~U8, PR #123~#130)을 release train
`0.43.0`으로 실어 operational current로 올렸다. 운영 위생 C2(current와 train)는 충족한다. C7(backlog 분류)은 `undecided` 6건이
남아 충족하지 않는다. 그 분류는 사용자 결정이다.

이 결론은 판정 결과를 옮긴 것이다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | `main` `a8e2b57` Development Gates success(run `42713120467`) |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족 | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 설치된 VM을 template으로 잠그고 복제 | 충족 | `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md` |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 충족 | `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md` |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 충족 | `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md` |
| C5 CI와 기한 위험 | 충족 | Development Gates와 Public Boundary Contract(run `42713120027`) success. 열린 기한 위험 0 |
| C6 영구 범위 밖 | 충족 | `permanent_out_of_scope` 5개 항목 |

운영 위생(판정 밖):

| 조건 | 상태 | 값 |
| --- | --- | --- |
| C2 current와 train | 충족 | current `0.43.0-admin-smoke` = operational current, train `promoted`, queue 0, 미완료 train 0 |
| C3 feature qualification | 충족 | candidate 4개 모두 pass, `promotion_eligible=true` |
| C4 SERVICE_PLAN 항목 | 충족 | 15개 모두 닫힘 |
| C7 backlog 분류 | 미충족 | `open_counts=0`, `undecided=6`(BL-0036~0041) |

## 3. 이 기간에 한 일 (2026-11-09 → 2026-11-10)

- UI 개편(`ui-parity-20261109`, ADR-0020): 단계 U1~U8을 PR #123~#130으로 merge했다.
  - 동작 결함 9건과 명령 팔레트 하나
  - VM 목록·상세 골격과 행 메뉴·일괄 작업
  - 화면별 렌더 등록표와 네트워크·작업·이벤트·대시보드·진단·evidence·계정 화면
  - 옛 렌더러 퇴역
  - 감사 61건은 고침 41, 그대로 둠·이미 같음 8, 넣지 않음 7, API 설계로 넘김 5로 처리했다.
- release train `0.43.0`(`train-04300-20261110`, 단일 PR #131):
  - version은 승인 문장의 `0.42.100` 대신 `0.43.0`이다. train 도구의 다섯 자리 꼬리표가 세 자리 patch를 받지 못해서이며, 사용자가 "0.43.0으로 출발 (권장)"으로 정했다.
  - package, pair 여섯 bucket(`0.42.99 -> 0.43.0`), fullgate 2/2, current-card가 PASS했다.
  - 설치본 Web Console 화면 probe
    - 내주는 Web 파일 5개가 payload, 소스와 같다.
    - 화면 12종이 1440·768에서 그려졌다. 옛 팔레트는 없고 console error는 0이다.
  - fullgate `REMOVE_DATA` 동안 계정 파일을 data root 밖에 보관했다가 되돌렸다.
  - Lane 3를 같은 PR에 담았다. `main-push-payload` evidence가 payload `6355e6ba`의 main push run을 인용하고, path check에서 바뀐 pin은 허용 pin 6개뿐이다.
- merge 규칙: 2026-11-09 사용자 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`으로 PR #123~#131을 merge했다.

## 4. 이 호스트

- 설치본: `0.43.0-admin-smoke+1f646ee`(operational current와 같음, ARP 1개). service `PureCVisorDesktopNode`는 Running/Automatic이다.
- 네트워크: loopback 전용이다. Web `http://127.0.0.1/` 200, PureCVisor 방화벽 규칙 0개.
- 계정 3개: `pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled. 계정이 있어 loopback 세션은 409로 거절된다.
- VM: 보존 VM `pcv-guest-installed-04253-r1` Off, S2 template `pcv-it-s2-source` Off. 그 밖의 `pcv-it-` VM은 0개다.
- train 계정 백업(백업 `accounts.json`, fullgate가 만든 빈 저장소, ACL 기록)은 저장소 밖 보호 위치에 남아 있다. 지울지는 사용자에게 묻는다.

## 5. 남은 일

| 항목 | 다음 행동 |
| --- | --- |
| C7 backlog 분류 | BL-0036(768 폭 검색 버튼 이름), BL-0037(좁은 VM 목록 이름 잘림), BL-0038(삭제 확인 gate flake), BL-0039(고대비·mockup 테마 핵심 색), BL-0040(첫 방문 `runtime/policy` 401), BL-0041(진단 서비스 값 `true`)을 counts 또는 out-of-scope로 정한다 |
| train 계정 백업 | 사용자 승인 뒤 지운다 |
| 다음 release train | queue에 product payload 변경이 생기면 출발 조건(DEVELOPMENT_PROCEDURE §10)대로 |
| completion autopilot | 정책 `status=paused`(ADR-0017) 그대로다. 재개 여부는 사용자 결정이다 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`의 결과로만 적는다.
- 이 문서는 operational current를 바꾸지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 내부 사설망 전용 배포(ADR-0006) 범위다.

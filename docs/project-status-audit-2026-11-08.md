# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-11-08, 완료 판정)

- 기준: `main` `2845ec4`(PR #112 merge). operational current `0.42.98-admin-smoke`.
- 판정: `pcvverify completion`(완료 정의 v3, ADR-0017), clean `main`에서 실행했다.
  `complete=true met=7/7 gaps=0 head=2845ec4cbd1c339e0021ba6a06ab6ad065e3a198`. 결과 파일은 로컬
  `artifacts/completion/20261108/result.json`(gitignore)이고, 입력 CI 목록은 같은 폴더의 `main-runs.json`이다.
- 직전 감사: `docs/project-status-audit-2026-10-09-after-s1.md`(`main` `9b6c0e2`, `met=3/7`).

## 1. 결론

완료 정의 v3의 일곱 조건이 모두 충족됐다. 판정 갭은 0이다. 2026-10-09 뒤 남았던 S2, S3, S4와 C5 기한 위험이 모두 닫혔다.
S4는 2026-11-08 설치본 `0.42.98`에서 판정했다.

이 결론은 판정 결과를 옮긴 것이다. 운영 위생(판정 밖) C7은 backlog `undecided` 18건으로 미충족이다. 이 행들의 분류는 사용자 결정이다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | `main` `2845ec4` Development Gates success(run `42485082138`) |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족 | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 설치된 VM을 template으로 잠그고 복제 | 충족 | `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md` |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 충족 | `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md` |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 충족 | `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md`(사용자 보고, LAN 시나리오 스크립트 `pass-without-auto`, 22:10 예약 checkpoint 관측, 끝 상태 loopback). 근거 구성과 한계는 그 기록의 판정 절에 있다 |
| C5 CI와 기한 위험 | 충족 | Development Gates와 Public Boundary Contract(run `42485082091`) success. `ubuntu-26-runner` 위험은 PR #111로 닫혔다(첫 `ubuntu-26.04` `main` run `41955196527`, `41955196532`). 열린 기한 위험 0 |
| C6 영구 범위 밖 | 충족 | `permanent_out_of_scope` 5개 항목 |

운영 위생(판정 밖):

| 조건 | 상태 | 값 |
| --- | --- | --- |
| C2 current와 train | 충족 | current `0.42.98-admin-smoke` = operational current, train `promoted`, queue 0, 미완료 train 0 |
| C3 feature qualification | 충족 | candidate 4개 모두 pass, `promotion_eligible=true` |
| C4 SERVICE_PLAN 항목 | 충족 | 15개 모두 닫힘 |
| C7 backlog 분류 | 미충족 | `undecided` 18건: BL-0011, 0012, 0013, 0015, 0018, 0022, 0024~0035 |

## 3. 이 기간에 한 일 (2026-10-09 → 2026-11-08)

- S2(2026-10-09, PR #77), S3(2026-10-11, 설치본 `0.42.96`) 시연과 criteria `passed`.
- ADR-0018 Single Edge 셸(train `0.42.96`)과 ADR-0019 Web Console 재설계(campaign `ui-redesign-20261105`)가 설치본에 들어갔다.
  - 재설계 내용: VM 상세 탭, 4단계 마법사, 이름이 첫 칸인 복제 모달, 앱 안 확인 창, 콘솔 맞춤·전체 화면, 체크포인트 예약 즉시 반영, 템플릿·계정 화면
  - 함께 들어간 것: 계정 비밀번호 재설정·변경(PR #104), `configure-installed` LAN 옵션과 실행 중 service 재시작(PR #94, #95)
- release train:
  - `0.42.97`은 BL-0029로 정차했다. `0.42.96` 설치본의 제품 Update가 옛 콘솔 파일을 요구했다.
  - PR #108 뒤 `0.42.98`이 두 PR(#109 evidence, #110 Lane 3)로 operational current가 됐다.
- C5: Ubuntu 26 runner 확인으로 기한 위험을 닫았다(PR #111).
- S4: LAN 계정 시연을 설치본 `0.42.98`에서 판정했다(PR #112). 시나리오 스크립트의 access token 미갱신 결함은 `BL-0035`다.

## 4. 이 호스트

- 설치본 `0.42.98-admin-smoke+2dfe2ac`(operational current와 같음, ARP 1개). service `PureCVisorDesktopNode` Running/Automatic.
- loopback 전용이다(S4 뒤 복귀). Web `http://127.0.0.1/` 200. PureCVisor 방화벽 규칙 0개.
- 계정 3개: `pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled. loopback 세션은 계정이 있어 409로 거절한다.
- VM: 보존 VM `pcv-guest-installed-04253-r1` Off(부모 VHD 경로 문제 BL-0028), S2 template `pcv-it-s2-source` Off. 그 밖의 `pcv-it-` VM은 0개다.
- S4 계정 백업은 저장소 밖 보호된 위치에 남아 있다. 지울지는 사용자 결정이다.

## 5. 남은 일

| 항목 | 다음 행동 |
| --- | --- |
| C7 backlog 18건 | 사용자 분류(counts 또는 out-of-scope). BL-0029는 PR #108로 고쳐졌으니 닫을지 정한다 |
| BL-0031~BL-0035 | Web Console 다듬기, 예약 미리보기 입력 초기화, 켜진 VM 복원 실패 안내, 목록 parity 실패 뒤 남는 행, S4 스크립트 token 갱신. 분류 뒤 Lane 1 수정 후보 |
| 다음 release train | queue에 web payload 변경이 생기면 출발 조건(DEVELOPMENT_PROCEDURE §10)대로 |
| completion autopilot | 정책 `status=paused`(ADR-0017) 그대로다. 완료 뒤 재개 여부는 사용자 결정이다 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`의 결과로만 적는다.
- operational current를 바꾸지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 내부 사설망 전용 배포(ADR-0006) 범위다.

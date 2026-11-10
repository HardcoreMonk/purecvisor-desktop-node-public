# PureCVisor Desktop Node 프로젝트 현황 감사 (2026-11-11 r2, BL-0044 train 승격)

- 기준: `main` `9d583e6`(PR #147 merge). operational current는 `0.43.4-admin-smoke`다.
- 판정: clean `main`에서 `pcvverify completion`(완료 정의 v3, ADR-0017)을 실행했다.
  - 결과: `complete=true met=7/7 gaps=0 head=9d583e6611ff7a03cca14c1328d6ad34db0eb890`
  - 결과 파일: 로컬 `artifacts/completion/20261111-r4/result.json`(gitignore). 입력 CI 목록은 같은 폴더의 `main-runs.json`이다.
- 직전 감사: `docs/project-status-audit-2026-11-11.md`(`main` `6844699`, `met=7/7`, C7 `undecided=1`). 같은 날 두 번째 감사라 이름 끝에 `-r2`를 붙였다.

## 1. 결론

판정상 완료 정의 v3의 일곱 조건이 계속 충족되고, 판정 갭은 0이다. BL-0044 수정(PR #146)을 release train `0.43.4`로 실어 operational current로 올렸다. 설치본 대시보드 "경고" 카드는 첫 갱신 중에 "운영 요약을 읽고 있습니다."를 보이고, 끝나면 목록이 된다. 운영 위생 C2·C3·C4·C7이 모두 충족한다.

이 결론은 판정 결과를 옮긴 것이다. 같은 날 사용자 요청으로 한 제로베이스 감사(아래 §5)는 이 판정이 시나리오 상태 문자열과 시연 기록 파일의 존재만 본다는 점을 지적했다(`src/DesktopNode.Verification/Completion/ProjectCompletionEvaluator.cs:139-155`). 판정 충족이 현행 설치본에서 S1~S4를 다시 시연했다는 뜻은 아니다.

## 2. 조건

| 조건 | 상태 | 근거 |
| --- | --- | --- |
| C1 제품 런타임 GA-ready CI | 충족 | `main` `9d583e6` Development Gates success(run `42882044894`) |
| S1 브라우저에서 ISO로 VM 생성 → OS 설치 → 네트워크 확인 | 충족(판정) | `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md` |
| S2 설치된 VM을 template으로 잠그고 복제 | 충족(판정) | `docs/ga-ready/demo/s2-template-clone-demo-2026-10-09.md` |
| S3 checkpoint 생성·복원·예약을 브라우저에서 | 충족(판정) | `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md` |
| S4 LAN의 다른 PC에서 계정으로 S1~S3 | 충족(판정) | `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md` |
| C5 CI와 기한 위험 | 충족 | Development Gates와 Public Boundary Contract(run `42882045120`) success. 열린 기한 위험 0 |
| C6 영구 범위 밖 | 충족 | `permanent_out_of_scope` 5개 항목 |

운영 위생(판정 밖):

| 조건 | 상태 | 값 |
| --- | --- | --- |
| C2 current와 train | 충족 | current `0.43.4-admin-smoke` = operational current, train `promoted`, queue 0, 미완료 train 0 |
| C3 feature qualification | 충족 | candidate 4개 모두 pass, `promotion_eligible=true` |
| C4 SERVICE_PLAN 항목 | 충족 | 15개 모두 닫힘 |
| C7 backlog 분류 | 충족 | `open_counts=0`, `undecided=0` |

## 3. 이 기간에 한 일 (2026-11-11 → 2026-11-11 r2)

- train `0.43.3` 계정 백업 삭제와 BL-0044 `counts` 분류(사용자 결정 `"1, 2 문구 수정"`, PR #145).
- BL-0044 수정(`bl0044-alerts-loading-20261111`, PR #146): 첫 갱신이 진행 중이고 아직 끝난 적이 없으면 "경고" 카드가 `data-alerts-state="loading"`과 새 문구 "운영 요약을 읽고 있습니다."를 보인다.
- release train `0.43.4`(`train-04304-20261111`, 단일 PR #147):
  - package, pair 여섯 bucket(`0.43.3 -> 0.43.4`), fullgate 2/2, current-card가 PASS했다. pair clean-host bucket은 guest 재부팅 무응답을 runner 복구 정책으로 넘겨 1,222초가 걸렸다.
  - 설치본 probe: 내주는 Web 파일 5개가 payload와 같다. 1440·768에서 새 context 세 번씩 모두 첫 갱신 중 읽는 중 문구가 보였고, "운영 요약을 읽지 못했습니다"는 한 번도 없었다.
  - Lane 3를 같은 PR에 담았다. path check에서 바뀐 pin은 허용 pin 6개뿐이다.

## 4. 이 호스트

- 설치본: `0.43.4-admin-smoke+7b929ec`(operational current와 같음, ARP 1개). service `PureCVisorDesktopNode`는 Running/Automatic이다.
- 네트워크: loopback 전용이다. Web `http://127.0.0.1/` 200, PureCVisor 방화벽 규칙 0개.
- 계정 3개: `pcv-admin` disabled, `pcv-admin2` admin enabled, `pcv-s4-operator` disabled. 계정이 있어 loopback 세션은 409로 거절된다.
- VM: 보존 VM `pcv-guest-installed-04253-r1` Off, S2 template `pcv-it-s2-source` Off. 그 밖의 `pcv-it-` VM은 0개다.
- train 계정 백업은 저장소 밖 보호 위치에 남아 있다. 지울지는 사용자에게 묻는다.

## 5. 제로베이스 감사(2026-11-11)와 남은 일

사용자 요청으로 읽기 전용 제로베이스 감사를 했다. 요지는 다음과 같다.

- 2026-10-08~11-08에는 브라우저 VM 생성, 콘솔 입력, template 복제, checkpoint와 예약, LAN 계정 로그인이 코드와 설치본에 들어왔다.
- 11-08 이후 제품 변경은 Web Console 재작성과 설치본 probe가 찾은 표시 결함 수정이었다. 새 Hyper-V 기능은 없었다.
  - train `0.43.1`~`0.43.4`는 각각 표시 수정 하나만 실었다.
  - 2026-10-14 이후 merge가 아닌 commit 202개 중 127개가 문서만 바꿨다. 테스트를 뺀 `src/` 변경은 +764줄이다.
- 현행 0.43.x Web Console로 실제 VM을 만들거나 바꾸는 흐름, 브라우저 콘솔 OS 설치, OS가 설치된 VM의 복제는 다시 시연하지 않았다.

| 항목 | 다음 행동 |
| --- | --- |
| train 계정 백업 | 사용자 승인 뒤 지운다 |
| 현행 설치본 S1~S3 브라우저 재증명과 시나리오 스크립트 | 감사 권장안. 사용자 승인 문장으로 연다 |
| release train 묶음 출발 기준 | 표시 수정 하나마다 train을 보내지 않도록 절차를 정한다. 사용자 승인 문장으로 연다 |
| 대시보드 첫 로드 약 10초 | 원인(`ops/summary` 5~7초, 첫 갱신 취소 경로)을 고친다. 사용자 승인 문장으로 연다 |
| 다음 시나리오 단계(S5~) | 사용자 결정 |
| completion autopilot | 정책 `status=paused`(ADR-0017) 그대로다 |

## Nonclaims

- 이 감사는 판정 결과를 옮긴 것이다. 완료는 `pcvverify completion` exit `0`의 결과로만 적는다. 판정이 정적 조건이라는 한계는 §1과 §5에 적었다.
- 이 문서는 operational current를 바꾸지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
- 내부 사설망 전용 배포(ADR-0006) 범위다.

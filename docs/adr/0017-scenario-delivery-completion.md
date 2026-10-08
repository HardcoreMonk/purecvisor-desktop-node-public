# ADR-0017: 시나리오 기준 완료 정의와 절차 축소

상태: 제안
일자: 2026-10-08

## 결정 마커

```text
DESKTOP_NODE_COMPLETION_DEFINITION: scenario-based-v3 (proposed)
DESKTOP_NODE_FEATURE_PR_PATH: lane1-required-ci-installed-smoke (proposed)
DESKTOP_NODE_RELEASE_TRAIN_TRIGGER: scenario-stage-complete (proposed)
DESKTOP_NODE_COMPLETION_AUTOPILOT: paused-until-v3 (proposed)
```

## 맥락

2026-10-08 사용자 진단은 "단계별로 구현되는 서비스 기능이 없는 제자리 걷기"다. 저장소 기록이 같은 결론을 보인다.

- 2026-09-08~10-08 public commit(merge 제외)은 `docs` 274, `feat` 69, `fix` 25, `test` 27개다. 10-03 이후 `feat`는
  대부분 검증·train 도구(`pcvverify completion`, `train-facts`, Lane 3 spec, evidence 템플릿)이고, 사용자 기능 묶음은
  09-30 Web 폼(checkpoint 일정, export/import, 스위치 연결, guest 실행)이 마지막이다.
- public 저장소에서 추가된 줄은 docs와 evidence가 약 18.8만, 제품 C#이 약 6.0만, 시험이 약 6.9만이다. evidence 파일은
  697개, plan/spec은 297개다.
- 완료 정의 v2(`pcv-project-completion-definition-v2`)의 C1~C7 중 6개가 CI·증거·train·backlog 상태다. 기능을 다루는
  C4도 "사용자가 할 수 있나"가 아니라 "evidence가 있나"를 본다. 그래서 completion autopilot은 증거의 빈칸을 메우는
  campaign을 계속 열고, 판정이 `met=6/7`이어도 시연할 새 기능이 없다.
- 제품 버그 수정 두 건(train `0.42.93`)을 내보내는 데 package, pair, fullgate, current-card, probe 2개, Lane 3, ship,
  판정까지 12개 task를 돌았다.
- 핵심 사용자 흐름이 끊겨 있다. Web Console의 콘솔은 `vmconnect` handoff뿐이고 noVNC는 기본 `not_configured`라,
  브라우저에서 만든 VM의 화면을 브라우저에서 볼 수 없다.

## 결정 (제안)

### 1. 완료 정의 v3: 시연 가능한 사용자 시나리오

| 단계 | 시나리오 | 완료 조건 |
| --- | --- | --- |
| S1 | 브라우저에서 ISO로 VM을 만들고, 브라우저 콘솔로 OS를 설치하고, 네트워크 연결을 확인한다 | 설치본에서 S1 시나리오 스크립트 1회 통과와 시연 기록 |
| S2 | 설치한 VM을 template으로 만들고 복제해 새 VM을 1분 안에 받는다 | 같은 형식 |
| S3 | checkpoint 생성·복원과 예약 실행을 브라우저에서 한다 | 같은 형식 |
| S4 | LAN의 다른 PC에서 계정으로 로그인해 S1~S3을 쓴다 | 같은 형식, LAN 노출은 ADR-0006·ADR-0010 경계 안 |

- 프로젝트 완료는 S1~S4 통과, `main` Required CI green, 제품 런타임 GA-ready 유지(v2 C1)다.
- v2의 C2(train 상태), C3(ledger stage), C7(backlog 분류)은 완료 조건이 아니라 운영 위생으로 내린다.
- 시연 기록은 짧은 markdown 한 장(시나리오, 설치본 version, 스크립트 결과, 화면 캡처 경로)이고 evidence 문서를 대신한다.
- 영구 범위 밖(v2 §3)은 그대로다.

### 2. 절차 축소

1. 평소 기능 PR은 Lane 1 + Required CI + 설치본 smoke(해당 단계 시나리오 스크립트) 하나로 merge한다. package, pair,
   fullgate, Lane 3를 PR마다 돌리지 않는다.
2. release train(pair, fullgate, Lane 3 승격)은 시나리오 단계가 끝날 때만 출발한다.
3. 기능 PR 하나에 plan 또는 evidence 문서는 1개 이하다.
4. 동기화 부담(Lane 3 spec SHA pin, feature ledger 행, `CURRENT_EVIDENCE_LEDGER.md`·`EVIDENCE_INDEX.md` 절, current-card)은
   채택 뒤 항목마다 자동화나 폐지를 정한다.
5. completion autopilot(`pcv-completion-autopilot-v1`)은 v3 판정이 구현될 때까지 멈춘다.
6. 진척 지표는 주마다 "새로 시연 가능해진 시나리오 수"와 `feat`:`docs` commit 비율이다.

## 채택 때 바뀌는 계약

- `config/project-completion-criteria.json`, `src/DesktopNode.Verification` `completion` 명령(v3 판정)
- `config/completion-autopilot-policy.json`(정지 또는 v3 갭 종류)
- `docs/DEVELOPMENT_PROCEDURE.md` §10 train 출발 조건, `AGENTS.md` 현재 기준 절
- ADR-0015 feature ledger의 승격 역할(시나리오 단계와의 관계)
- private 저장소 `pcv-campaign`, `pcv-campaign-open`, `pcv-goal` skill
- 완료 정의 v2 설계 `docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-definition-v2-design.md`는
  역사 기록으로 낮춘다

이 제안 단계에서는 위 계약을 바꾸지 않는다. 채택은 사용자 승인으로 하고, 계약 변경은 채택 뒤 Lane 1 task로 나눈다.

## 결과

- 진척을 증거 상태가 아니라 사용자가 시연할 수 있는 기능으로 잰다.
- 기능 하나를 내보내는 고정비가 PR 하나와 설치본 smoke로 준다. train은 단계마다 한 번이다.
- 대신 PR마다 설치본 승격 증거가 쌓이지 않는다. operational current는 단계 train에서만 바뀐다.

## Nonclaims

- 이 ADR은 제안이다. 완료 정의 v2, `pcvverify completion`, autopilot 정책은 채택 전까지 그대로다.
- S1의 브라우저 콘솔 구현 방식은 이 ADR이 정하지 않는다(campaign `scenario-pivot-20261008` spike).
- public trusted signing과 external stable publication을 주장하지 않는다.

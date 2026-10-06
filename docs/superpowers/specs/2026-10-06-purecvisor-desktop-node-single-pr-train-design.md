# Release train 단일 PR 설계

- Design-ID: `pcv-single-pr-train-v1`
- 작성일: `2026-10-06`
- 문서 상태: `proposed` (2026-10-06 사용자 승인 `1,2,3,4`의 4, 설계만. 구현은 별도 승인)
- 변경 등급: M (train 절차. 제품 동작 변경 없음)
- host/VM/service/package mutation: `false`
- public trusted signing / external stable publication: `false`

## 1. 문제

train 하나가 PR 두 개를 만든다. train PR(package·pair·fullgate·current-card·probe evidence)을 먼저 merge하고, 그 merge의
main push CI run을 Lane 3 PR이 evidence(`public-boundary-ci-main-push-<date>-<tag>-pr<N>-postmerge-pass`)로 인용한 뒤
`current-evidence.json`을 쓴다.

| train | train PR (생성→merge, UTC) | Lane 3 준비부터 merge까지 | 두 번째 종료 검증 |
| --- | --- | --- | --- |
| 0.42.90 | #49 12:36→12:40 | 21:40→21:56 KST, 약 `16`분 | 21:46:37→21:53:38, `7`분 |
| 0.42.91 | #54 12:00→12:02 | 21:02→21:21 KST, 약 `19`분 | 21:07:13→21:13:33, `6`분 20초 |

두 번째 PR은 종료 검증 한 번, CI 한 바퀴, main push run 대기, merge를 더한다. train마다 약 `10`분이다. 2026-10-06 점검
F3에서 측정했다(commit 시각과 PR 시각 두 출처).

## 2. 순서가 둘로 나뉜 이유

- Lane 3 main push evidence가 인용하는 run은 train PR merge 뒤의 `push` run이다. 같은 PR에 Lane 3를 넣으면 merge 전에 그 PR의
  merge 뒤 run을 인용해야 한다. 순환이다.
- 이 evidence는 문서만이 아니다. 설치본 Ops Summary의 `public_boundary.latest_main_push`가 batch evidence root의
  `public-boundary-ci-main-push-*/summary.json`(scope `public-boundary-ci-required-main-push`)을 읽는다.
- main push evidence의 내용은 "train PR이 `src`, `web/src`, `config`, `.github`를 바꾸지 않았다"와 "payload commit이 main에서
  도달한다"다. train PR은 evidence와 절차 파일만 더한다.

## 3. 설계

### 3.1 인용할 main push run을 payload commit의 run으로 바꾼다

- 출발 때 고정하는 `main` HEAD(carriage의 마지막 merge commit)가 package의 payload source다. 이 commit에는 merge 때 돈
  main push run이 이미 있다.
- Lane 3 main push evidence는 이 run을 인용한다. scope는 `public-boundary-ci-required-main-push` 그대로, 이름은
  `public-boundary-ci-main-push-<date>-<tag>-payload-pass`다. Ops Summary reader는 바꾸지 않는다.
- 같은 evidence에 기계 확인을 하나 더한다. `git diff --name-only <payload commit>..<PR head>`에 `src/`, `web/src/`, `config/`,
  `.github/`가 없어야 한다. 있으면 Lane 3를 쓰지 않고 멈춘다.

### 3.2 PR 하나

- train branch 하나에 출발, package, pair, fullgate, current-card, probe, Lane 3 commit을 쌓고 PR 하나로 merge한다.
- 종료 검증은 Lane 3 commit 뒤 한 번이다.
- merge 전 PR CI(`pull_request`, merge ref)가 green이어야 한다. branch는 merge 때 `main`과 같은 base여야 한다. 그 사이 다른
  PR이 merge되면 rebase하고 PR CI를 다시 기다린다. 그래야 PR CI가 본 tree와 merge 뒤 `main` tree가 같다.

### 3.3 merge 뒤 확인

- merge 뒤 main push run을 기다린다. green이면 끝난다. run id는 다음 train의 출발 기록에 적는다(새 evidence 문서 없음).
- red면 그 merge를 revert하는 PR을 만든다. operational current는 직전 값으로 돌아가고, 원인은 Lane 1로 고친 뒤 다음
  version으로 다시 출발한다(train 설계 §4.6과 같다). revert 자체도 push/PR/merge 승인 범위 안에서 한다.

## 4. 바꿀 곳 (구현 때)

| 대상 | 변경 |
| --- | --- |
| `docs/DEVELOPMENT_PROCEDURE.md` §10 | task 6(pair evidence PR)과 8(Lane 3 PR)을 마지막 task 하나로 합친다. 출발 조건에 "고정할 `main` HEAD의 push run green"을 더한다 |
| `pcvverify train-facts`·`train-evidence` `main-push` 문서 | 입력을 payload commit run으로, 제목·scope 문장과 경로 확인 행을 바꾼다. golden을 새로 만든다 |
| `pcvverify lane3-spec` | main push evidence id 형식과 `DOCUMENTATION_INDEX` 공개 소스 권위 줄(HEAD는 payload commit) |
| train 설계 §4.7 승인 문구 | "push, PR, green CI 뒤 merge"에 "post-merge red면 revert PR"을 더한다 |
| `pcv-ship` skill | train branch는 merge 전 base 일치 확인 |

## 5. 위험과 대안

| 위험 | 대응 |
| --- | --- |
| PR CI는 green인데 merge 뒤 main이 red | base 일치 조건으로 같은 tree를 보장한다. 남는 원인(runner 차이, 시간 의존 시험)은 revert 규칙이 막는다 |
| 인용 run이 merge 뒤 run보다 오래됨 | payload는 그 commit과 같다는 경로 확인이 증거다. merge 뒤 run은 3.3에서 확인한다 |
| 다른 PR이 끼면 rebase와 CI 재대기 | train은 짧다(호스트 실행 약 `20`분). 그 사이 merge를 피하는 운영 규칙으로 충분하다 |

대안과 기각 이유:

- Lane 3 PR의 로컬 종료 검증만 생략한다: Lane 3가 고치는 `packaging/.../fixtures/lane3-promotion-docs-spec-*.json`은 Required
  CI에 없는 manual-admin Pester가 확인한다. 생략하면 그 확인이 빠진다.
- main push evidence를 다음 train으로 미룬다: Ops Summary가 승격 시점의 main push를 보여 주지 못한다.

## 6. 효과

- train마다 PR `1`개, 종료 검증 `1`회, CI 한 바퀴, main push 대기가 줄어 약 `10`분이 준다.
- evidence 문서 수는 같다(main push 문서 `1`개가 인용 run만 바뀐다).

## 7. Nonclaims

- 설계만이다. 절차, 도구, `current-evidence.json`, 호스트를 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.

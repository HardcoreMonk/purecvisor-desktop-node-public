# PureCVisor Desktop Node 효율적 개발 절차 설계

**Date:** 2026-08-29
**Status:** Implemented
**Implementation plan:** `docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md`
**Repository authority:** `HardcoreMonk/purecvisor-desktop-node-public`
**Contract:** `pcv-efficient-development-procedure-v1`

## 1. 목적

이 설계는 사람 개발자와 Codex/에이전트가 같은 절차를 사용하도록 기존 개발 정책을 하나의
실행 흐름으로 연결한다. 목표는 새 정책을 중복 정의하는 것이 아니라 다음 판단을 짧고
일관되게 만드는 것이다.

- 지금 작업의 소스 및 operational 권위가 무엇인지
- 변경이 `S`, `M`, `L` 중 어느 등급인지
- 현재 어떤 lane에서 무엇까지 수행할 수 있는지
- 어떤 검증이 focused 결과이고 어떤 검증이 clean-HEAD 통합 PASS인지
- 설치, Hyper-V, current evidence, Git/PR에 각각 어떤 승인이 필요한지
- 실패나 예산 소진 시 어디에서 중단하고 무엇을 보고해야 하는지

최종 사용자 진입점은 `docs/DEVELOPMENT_PROCEDURE.md`로 둔다. 이 문서는 규범 원본을
복제하지 않는 얇은 실행 안내서이며, 분류·검증·회로 차단기·Lane 정책의 단일 진실은 기존
문서와 기계 계약에 계속 둔다.

## 2. 현재 근거와 제약

절차는 다음 기존 계약을 변경하지 않고 조합한다.

- `docs/PUBLIC_SOURCE_AUTHORITY.md`: 공개 권위 저장소와 공개 배포 경계
- `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`: `S/M/L` 최소 변경 등급
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: `Fast/Full/Release` 및 clean-HEAD 검증
- `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`: checkpoint, 예산, 중단 규칙
- `config/agent-execution-circuit-breaker.json`: 기계 판독 회로 차단기 계약
- `config/development-verification-suites.json`: 검증 suite와 네 required shard
- `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md`:
  Lane 0~3 의미와 승격 금지 조건

활성 제품 표면은 Web Console과 PCVCLI다. TUI는 현재 절차의 기능 검증 대상이 아니다.
소스 공개와 trusted binary release는 별개이며, 이 절차 자체는 trusted signing 또는 외부
stable publication 권한을 만들지 않는다.

## 3. 검토한 대안

### 3.1 기존 문서 연결형

기존 문서에 링크만 추가한다. 변경량은 작지만 작업자가 여러 문서를 오가며 lane, 변경 등급,
승인 경계를 재조합해야 하므로 반복 작업 효율이 낮다.

### 3.2 실행형 통합 SOP

하나의 얇은 절차 문서에 진입 판단, lane 흐름, 명령 묶음, 승인표, 종료 보고 형식을 제공하고
세부 규칙은 규범 원본으로 연결한다. 검색 비용을 줄이면서 정책 중복과 drift를 피할 수 있다.

### 3.3 자동화 우선형

diff 기반 분류부터 검증과 증거 생성을 하나의 실행기로 묶는다. 반복성은 높지만 Git, 설치,
Hyper-V, operational promotion의 서로 다른 승인 경계를 잘못 결합할 위험이 있고 초기 구현
비용도 크다.

### 3.4 결정

`3.2 실행형 통합 SOP`를 채택한다. 먼저 문서와 기존 검증 진입점을 일관되게 연결하고,
반복 과정에서 측정된 병목만 후속 자동화 후보로 분리한다.

## 4. 전체 실행 모델

```text
Lane 0: 권위·범위·예산 고정
  -> S/M/L 변경 분류
  -> Lane 1: 설계·TDD·repo-local 검증
  -> Git commit 승인
  -> clean committed HEAD 네 shard 검증
  -> Git push/PR 승인
  -> [필요하고 승인된 경우] Lane 2: 설치본·Actual VM probe
  -> [모든 PASS와 별도 승인] Lane 3: operational promotion
  -> 상태·잔여 작업·mutation·current-write 보고
```

한 checkpoint는 하나의 lane만 소유한다. Lane 전환은 새 checkpoint 선언 없이 묵시적으로
수행하지 않는다. `작업 재개`처럼 범위가 불명확한 입력은 회로 차단기의
`one-bounded-checkpoint` 정책을 따른다.

## 5. Lane 0: 빠른 권위 및 작업 카드

모든 작업은 구현 전에 다음 값을 기록한다.

| 필드 | 의미 |
| --- | --- |
| `repository_authority` | 권위 저장소 식별자 |
| `origin_main` | 원격 통합 기준 SHA |
| `local_head` | 현재 로컬 HEAD SHA |
| `working_branch` | 실제 작업 브랜치 |
| `working_tree_state` | clean 또는 dirty |
| `ledger_current` | current evidence가 선언하는 operational 버전 |
| `installed_current` | 현재 호스트 설치본 버전 |
| `source_head` | 이번 코드 판단 기준 SHA |
| `change_intent` | 한 문장 변경 목적 |
| `lane` / `tier` | 현재 lane과 `S/M/L` 등급 |
| `budget` | 시간, 도구 묶음, 리뷰 한도 |
| `approval_profile` | 이번 checkpoint에서 유효한 승인 범위 |

`origin_main`, `local_head`, `source_head`는 서로 같다고 가정하지 않는다. 특히 로컬 `main`이
원격보다 앞서 있으면 해당 커밋은 remote-integrated 상태가 아니라 로컬 후보 상태로 기록한다.

## 6. 변경 분류와 검증 lane

변경 파일 경로를 기준으로 `Resolve-PcvDevelopmentChangeTier`가 계산한 최소 등급을 사용한다.
호출자가 더 낮은 등급을 요청해도 자동 최소 등급을 낮추지 않는다.

| 변경 등급 | 대표 범위 | 최소 검증 |
| --- | --- | --- |
| `S` | 단일 내부 모듈의 비계약 구현·테스트 | `Fast` |
| `M` | API/CLI/Web 계약, 일반 패키징, 교차 모듈 | `Full` |
| `L` | 보안, installer lifecycle, host mutation, current evidence, 공개 배포 | `Release` |

분류되지 않은 경로는 근거 없이 `L`로 표시하지 않되, 검증 lane은 `Full`로 올린다. 분류 결과는
`requested_change_tier`, `change_tier`, `tier_reasons`, `requested_lane`, `effective_lane`을
보존한다. `Release`는 비변경 preflight이며 mutation 권한이 아니다.

## 7. Lane 1: 기본 개발 루프

Lane 1은 모든 코드 변경의 기본값이다.

1. 변경의 단일 owner와 영향 계약을 지정한다.
2. 실패하는 focused test 또는 재현 가능한 계약 검사를 먼저 만든다.
3. 최소 구현으로 통과시킨다.
4. focused test를 재실행하고 필요한 범위만 리팩터링한다.
5. dirty-tree 준비 검증을 수행한다.
6. diff와 생성 산출물의 의도치 않은 변경을 검토한다.
7. Git commit 승인을 받은 뒤 clean committed HEAD 통합 검증을 수행한다.

owner 이동과 동작 변경을 같은 변경에 섞지 않는다. API 오류, 인증, cancellation/lifetime,
HTTP/TLS, WMI 의미가 바뀌면 낮은 등급의 국소 변경으로 취급하지 않는다.

### 7.1 dirty-tree 준비 검증

표준 준비 검증은 다음 순서를 사용한다.

```powershell
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

개발 중에는 변경 owner의 focused test를 우선 실행한다. dirty tree에서 얻은 focused 결과를
whole-solution 또는 clean-HEAD policy PASS로 표시하지 않는다.

### 7.2 clean-HEAD 통합 검증

Git commit 승인을 받아 clean committed HEAD를 만든 뒤
`DesktopNode.Verification` 진입점으로 다음 required shard를 모두 실행한다.

- `dotnet`
- `web`
- `delivery`
- `installer-policy`

`--plan-only`는 suite 선택이 올바른지만 확인하므로 PASS 근거가 아니다. `.ps1` changed path는
검증 선택 데이터일 뿐 해당 PowerShell 파일 실행 권한을 뜻하지 않는다.

## 8. 승인 경계

승인은 다음처럼 분리한다.

| 작업 | 기본 처리 |
| --- | --- |
| 읽기, 분석, repo-local focused test | 선언된 Lane 0/1 범위에서 자동 진행 |
| 소스 파일 변경 | 승인된 설계와 checkpoint 범위에서 진행 |
| Git commit | 별도 Git 승인 |
| push 및 PR 생성/갱신 | 별도 Git/PR 승인 |
| package candidate 생성 | package checkpoint 승인 |
| 설치 및 Hyper-V mutation | Lane 2 승인 또는 유효한 명시적 standing profile |
| current evidence 쓰기 | Lane 3 별도 승인 |
| trusted signing 및 외부 publication | 별도 공개 배포 승인 |

사용자가 선언한 설치·Hyper-V 자동 승인 profile은 명시된 저장소, 호스트, 기능군과 checkpoint
범위에서만 Lane 2 진입 조건을 충족한다. 이 profile은 다음 권한을 포함하지 않는다.

- 시간·도구 예산 또는 checkpoint 자동 연장
- 실패 후 다른 기능군이나 Full mode로 자동 확대
- current evidence 갱신
- trusted signing 또는 외부 publication

## 9. Lane 2: 설치본 및 Actual VM probe

Lane 2는 운영 승격이 아니라 제한된 검증 lane이다.

- checkpoint당 기능군 하나
- artifact root 하나
- VM root 하나
- 실행 전 설치본 버전과 대상 provider를 기록
- pre-state, mutation, readback, cleanup, failure evidence를 분리
- `DryRun`을 actual PASS로 표시하지 않음
- `SavedOnly` 이후 `Full` 실행은 새 checkpoint 필요
- 최초 하위 `PCV_*` 오류 코드를 요약에 보존

Lane 2 FAIL은 `actual_vm_tested=pass` 또는 promotion eligibility를 만들 수 없고 current evidence를
수정할 수 없다. 성공하더라도 결과는 installed non-promoted candidate 상태다.

## 10. Lane 3: operational promotion

Lane 3는 모든 필수 근거가 PASS이고 별도 승인이 있을 때만 열린다.

필수 입력은 대상 변경의 성격에 따라 package build, full admin host mutation, manual-admin pair,
installed operator surface current-card, feature qualification summary다. current evidence에는 PASS
근거만 기록한다. 실패나 부분 완료 결과는 historical/probe evidence로 보존하고 current로
승격하지 않는다.

Lane 3 완료는 public trusted signing 또는 external stable publication 완료를 의미하지 않는다.

## 11. 상태 모델

절차와 보고서는 다음 상태를 혼용하지 않는다.

1. `code_complete`
2. `code_ready_operational_pending`
3. `package_candidate`
4. `installed_non_promoted_candidate`
5. `operational_current`
6. `promotion_complete`

각 상태 전이는 이전 상태의 PASS 근거와 해당 승인 식별자를 요구한다. 소스 코드 완료를 설치본
검증 완료 또는 operational current로 축약하지 않는다.

## 12. 중단 및 실패 처리

기본 Lane 1 checkpoint 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토
2회다. 다음 중 먼저 발생한 조건에서 구현을 멈추고 stop protocol을 수행한다.

- checkpoint 시간 또는 도구 예산 소진
- 동일 원인 3회 실패
- 승인 범위를 넘어서는 mutation 필요
- 권위 또는 대상 경로를 안전하게 확정할 수 없음

중단 시 추가 구현 대신 다음을 수행한다.

1. 실행 중인 변경을 안전한 상태로 정리한다.
2. 최초 하위 오류와 재현 조건을 보존한다.
3. 수행된 mutation과 cleanup 결과를 기록한다.
4. 완료, 잔여, 범위 외 발견을 분리한다.
5. 추가 권한 또는 새 checkpoint가 필요한 이유를 보고한다.

범위 밖 발견은 report-only이며 현재 checkpoint 구현으로 흡수하지 않는다.

## 13. 종료 보고 계약

모든 checkpoint 종료 보고에는 최소 다음 필드를 포함한다.

```text
lane:
repository_authority:
origin_main:
source_head:
ledger_current:
installed_current:
change_tier / effective_lane:
budget_used:
completed:
verification:
remaining:
out_of_scope:
host_or_vm_mutation_performed:
current_evidence_written:
next_approval_required:
```

성공 보고는 실행한 검증과 실행하지 않은 검증을 함께 명시한다. `code complete`, `package
candidate`, `operational current`를 하나의 `완료` 표현으로 합치지 않는다.

## 14. 구현 구조

승인된 설계를 다음 문서 변경으로 구현한다.

1. `docs/DEVELOPMENT_PROCEDURE.md` 생성
   - 2분 진입 절차
   - 변경 분류표
   - Lane 0~3 실행 체크리스트
   - 검증 명령
   - 승인표
   - stop/report 템플릿
2. `docs/DEVELOPER_INDEX.md`에 기본 개발 진입점으로 연결
3. 필요한 경우 기존 lane 절차 문서에서 새 실행 진입점으로 역링크
4. 문서 링크와 명령이 현재 기계 계약과 일치하는지 focused 검증

기존 규범 원본의 숫자, 필드 또는 승인 의미를 새 문서에 독립적으로 재정의하지 않는다.
불일치가 발견되면 새 안내서에서 숨기지 않고 원본 계약 수정 작업으로 분리한다.

## 15. 수용 기준

다음을 모두 만족하면 절차 문서 구현이 완료된다.

- 새 개발자가 한 문서에서 시작 lane과 최소 검증을 결정할 수 있다.
- 사람과 에이전트가 같은 작업 카드와 종료 보고 형식을 사용한다.
- `S/M/L`과 `Fast/Full/Release`의 관계가 기존 resolver 계약과 일치한다.
- dirty focused 결과와 clean-HEAD 네 shard PASS가 구분된다.
- Git, package, 설치·Hyper-V, current evidence, publication 승인이 분리된다.
- standing 설치·Hyper-V 승인이 Lane 3나 예산 확대로 전이되지 않는다.
- Lane 2 FAIL이 current evidence를 갱신할 수 없음을 명확히 한다.
- 문서 링크, 예제 경로, 명령이 focused 검증을 통과한다.
- public trusted signing 및 external stable publication 주장을 새로 만들지 않는다.

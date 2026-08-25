# 에이전트 실행 회로 차단기 설계

- Design-ID: `pcv-agent-execution-circuit-breaker-v1`
- 작성일: `2026-08-23`
- 문서 상태: `proposed-for-written-spec-review`
- 설계 승인 근거: 사용자 요청 `재발 방지책 적용`
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`
- public trusted signing: `false`
- external stable publication: `false`

## 1. 문제와 사고 경계

후속 작업이 하나의 bounded checkpoint로 끝나지 않고, 계속 새 안전 이슈를 찾는 리뷰와
인접 P1 수정으로 확장됐다. 진행 보고는 `진행 중`이라는 상태만 전달하고 경과 시간, 사용한
도구 작업 묶음, 남은 범위와 강제 종료 시점을 보여주지 못했다. 사용자가 직접 중단하기 전까지
에이전트가 스스로 종료하지 못한 것이 핵심 실패다.

일반적인 C# 구현 속도와 별개로, PowerShell에서 동적 컴파일하는 C#/Win32 interop까지
후속 수정 범위에 포함되면서 설계 변경과 검증 비용이 커졌다. 새 위험을 발견할 때마다 모두
수정하는 방식은 안전 감사에는 적합할 수 있지만, 사용자가 요청한 bounded follow-up 실행에는
적합하지 않다.

이 설계는 에이전트의 선의나 기억에만 의존하지 않는다. 저장소 지침, 기계 판독 가능한 정책,
회귀 테스트를 함께 두어 후속 세션에서도 동일한 제한이 보이도록 한다. 다만 현재 제공된
에이전트 도구에는 wall-clock 기준으로 세션을 외부에서 강제 종료하는 API가 없으므로, 저장소
내 통제는 절대적인 process kill이 아니라 에이전트가 더 이상 write/review/test 작업을 시작하지
못하게 하는 필수 stop contract다.

## 2. 목표와 비목표

### 목표

- 모호한 `재개`, `계속`, `후속 작업` 요청을 기본적으로 다음 checkpoint 하나로 제한한다.
- 시작 전에 시간, 도구 작업 묶음, 리뷰 횟수, 동일 실패 횟수와 수정 범위를 공개한다.
- 한도에 도달하면 추가 구현 대신 실행 중 작업 정리와 상태 보고만 허용한다.
- 리뷰가 새 범주의 작업을 무한히 생성하지 못하게 한다.
- C#/Win32, ACL, process lifecycle 같은 저수준 변경을 별도 probe 없이 인접 수정으로 확장하지
  못하게 한다.
- 정책의 필수 수치와 `AGENTS.md` 연결이 사라지면 Pester가 실패하게 한다.

### 비목표

- Codex/ChatGPT 클라이언트 자체의 wall-clock hard kill 구현
- 제품 runtime, installer, VM 또는 운영 evidence 변경
- 이미 중단된 0.42.75 promotion closure 작업 재개
- 모든 작업에 동일한 절대 시간을 강제하는 범용 프로젝트 관리 시스템
- 사용자가 명시적으로 승인한 장시간 build/test/monitor 작업을 임의로 축소

## 3. 검토한 접근

### A. 대화상 약속만 추가

에이전트가 매번 짧게 끝내겠다고 약속하는 방식이다. 저장소에 남지 않고 새 세션에서 사라지며,
정책 누락도 테스트할 수 없다. 이번 사고와 같은 실패를 막지 못하므로 채택하지 않는다.

### B. 저장소 정책 + 기계 판독 계약 + 회귀 테스트

`AGENTS.md`에 필수 동작을 노출하고, 별도 정책 문서에 정확한 한도와 stop protocol을 둔다.
동일 값을 기계 판독 가능한 JSON 계약으로 고정하고 Pester가 문서 연결과 값을 검증한다.
현재 저장소 안에서 구현 가능한 가장 강한 통제이므로 채택한다.

### C. 클라이언트 외부 watchdog

wall-clock 초과 시 세션 자체를 종료하는 가장 강한 방식이다. 하지만 현재 저장소와 제공된
도구만으로 Codex 클라이언트 프로세스를 안전하게 종료할 권한이나 API가 없다. 지원되는
클라이언트 기능이 제공될 때 별도 작업으로 검토하며, 존재하지 않는 hard-kill 기능을 현재
대책으로 주장하지 않는다.

## 4. 선택한 구조

### 4.1 단일 진실

새 문서 `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`가 실행 규칙의 사람이 읽는 단일 진실이다.
루트 `AGENTS.md`는 파일 상단의 generated current evidence 블록을 건드리지 않고, 그 바로 뒤에
짧은 필수 지침과 단일 진실 링크를 둔다.

`config/agent-execution-circuit-breaker.json`은 다음 기본값을 기계 판독 가능한 형태로 고정한다.

```json
{
  "schema_version": 1,
  "contract": "pcv-agent-execution-circuit-breaker-v1",
  "default_checkpoint_count": 1,
  "elapsed_minutes_limit": 20,
  "tool_batch_limit": 12,
  "review_pass_limit": 1,
  "narrow_rereview_pass_limit": 1,
  "same_failure_limit": 2,
  "progress_warning_percent": 70
}
```

시간과 도구 작업 묶음 중 먼저 도달한 한도가 stop을 발생시킨다. `tool batch`는 하나의 목적을
위해 묶어 실행한 단일 도구 호출이다. 병렬 검색 여러 개를 한 호출에서 수행해도 한 묶음이며,
호출을 잘게 쪼개 한도를 우회해서는 안 된다.

### 4.2 시작 계약

파일 write, 외부 mutation 또는 하위 에이전트 시작 전에 commentary로 다음 항목을 공개한다.

```text
checkpoint: 1
elapsed budget: 20 minutes
tool batch budget: 12
review budget: 1 review + 1 narrow rereview
allowed files: <exact paths or bounded directories>
completion evidence: <exact commands>
out-of-scope findings: report only
```

읽기 전용 탐색은 설계에 필요한 최소 범위로 수행한다. 탐색 결과 수정 파일군이나 완료 조건을
늘려야 하면 구현을 시작하지 않고 사용자 승인을 요청한다.

### 4.3 진행 보고

70% 한도에 도달하거나 주요 checkpoint가 끝날 때 다음 수치를 보고한다.

```text
elapsed: 14/20 minutes
tool batches: 8/12
completed: 3/4
new out-of-scope findings: 2, queued without fixes
next stop: current checkpoint completion or first exhausted budget
```

단순한 `진행 중` 보고는 이 계약을 충족하지 않는다.

### 4.4 강제 종료 조건

다음 중 하나가 발생하면 회로가 열린다.

- 경과 시간 20분 도달
- 도구 작업 묶음 12회 도달
- 같은 원인의 실패를 두 번 관측
- 최초 승인 범위 밖 파일군 또는 새 설계가 필요
- C#/Win32, native ACL, process tree, installer handoff 같은 저수준 경계를 새로 변경해야 함
- 정규 리뷰 1회와 수정된 줄만 보는 재검토 1회가 끝남
- 재검토에서 새 범주의 P0/P1이 발견됨

회로가 열린 뒤 허용되는 행동은 다음뿐이다.

1. 실행 중인 명령과 하위 에이전트를 종료하거나 더 이상 새 작업을 주지 않는다.
2. `git status`, 현재 diff, 이미 끝난 검증 결과를 읽는다.
3. 사용자 승인 범위 안에서 이미 green인 최소 단위를 로컬 checkpoint로 보존한다. 커밋 권한이
   없으면 커밋하지 않는다.
4. 완료, 미완료, blocker, 새 발견 사항과 mutation 여부를 보고한다.

추가 patch, 새 테스트, 전체 재감사 또는 다른 reviewer 생성은 금지한다. 예산 연장은 사용자의
명시적 승인으로만 가능하며, 승인 시 새 checkpoint와 새 예산을 공개한다.

### 4.5 리뷰 경계

리뷰어는 고정된 diff, 승인 기준, 허용 파일만 받는다. 정규 리뷰가 끝나면 reviewer를 종료한다.
P0는 현재 checkpoint의 완료 조건을 직접 깨는 경우에만 수정한다. P1/P2와 인접 개선은
follow-up 목록에 기록하고 자동 수정하지 않는다.

재검토는 수정된 줄과 원래 P0 판정만 확인한다. 재검토 중 발견한 새 범주의 문제는 심각도와
영향을 보고하되 같은 checkpoint에서 구현하지 않는다. 이 규칙은 `안전`을 이유로 무제한
확장하는 것도 금지한다. 실제 host mutation이나 데이터 손실 위험이면 더 보수적으로 즉시
중단하고 사용자에게 권한과 방향을 요청한다.

### 4.6 저수준 C#/Windows 변경 경계

기존 작업 중 새 `Add-Type`, P/Invoke, `NtCreateFile`, ACL/security descriptor, FileId 또는 process
tree 제어가 필요해지면 현재 checkpoint에서 직접 구현하지 않는다. 별도 승인을 받은 probe
checkpoint에서 최소 호환성 테스트 하나를 먼저 RED/GREEN으로 닫아야 한다. probe가 두 번
실패하면 더 큰 helper나 framework로 확장하지 않고 종료한다.

## 5. 검증 설계

새 `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`는 최소한
다음을 검증한다.

- JSON 계약 schema와 contract ID가 정확하다.
- 기본값이 checkpoint `1`, 20분, 12 tool batches, review `1 + 1`, 동일 실패 `2`, 경고 `70%`다.
- `AGENTS.md`가 정책 문서와 JSON 계약을 링크하고 `vague resume = one checkpoint` 및
  `out-of-scope findings = report only`를 필수 문구로 가진다.
- 정책 문서가 모든 stop 조건과 stop 이후 허용 행동을 포함한다.
- 정책 어디에도 현재 제품 version/evidence를 승격하거나 host mutation을 허용하는 표현이 없다.

TDD 순서는 정책 테스트를 먼저 추가하고 예상한 missing-file/missing-contract 실패를 확인한 뒤,
JSON, 정책 문서, `AGENTS.md`를 최소 변경해 통과시키는 것이다. 해당 Pester 파일과 PowerShell
5.1/7 parser 검증만 실행한다. 전체 제품 테스트는 제품 payload를 바꾸지 않는 이 checkpoint의
완료 조건이 아니다.

## 6. 적용 순서와 종료점

1. 이 설계 문서를 사용자 검토로 확정한다.
2. `writing-plans`로 파일별 RED/GREEN 구현 계획을 작성한다.
3. 정책 회귀 테스트를 추가하고 RED를 확인한다.
4. JSON 계약, 정책 문서, `AGENTS.md`를 최소 변경한다.
5. 대상 Pester와 PowerShell 5.1/7 parser를 fresh run한다.
6. 로컬 정책 commit 하나를 만들고 즉시 종료한다.

0.42.75 promotion closure worktree, package artifact, evidence 문서, 서비스, VM, installer에는
손대지 않는다. push와 PR도 별도 사용자 승인 없이는 수행하지 않는다.


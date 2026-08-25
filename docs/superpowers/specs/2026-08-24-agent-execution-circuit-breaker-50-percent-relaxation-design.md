# 에이전트 실행 회로 차단기 50% 완화 설계

- Design-ID: `pcv-agent-execution-circuit-breaker-50-percent-relaxation-v1`
- 설계 승인일: `2026-08-24`
- 기반 계약: `pcv-agent-execution-circuit-breaker-v1`
- Product payload change: `false`
- Host/VM/service/package mutation authorization: `false`

## 1. 목적

현재 회로 차단기는 짧은 후속 작업이 반복 중단되는 것을 막기 위해 시간, 도구 작업 묶음,
리뷰, 동일 원인 실패 횟수를 제한한다. 이 설계는 범위와 안전 경계를 유지하면서 실제 중단
한도 총량을 정확히 50% 늘린다. 일시적인 대화 승인에 의존하지 않고 기계 계약, 규범 문서,
저장소 에이전트 지침과 계약 테스트가 같은 값을 소유하게 한다.

## 2. 결정

배율을 실행 시 계산하거나 세션별 override로 보관하지 않는다. 현재 값을 기계 계약에 직접
기록하고 모든 규범 표면이 그 값을 그대로 참조한다. 필드의 구조와 의미는 바뀌지 않으므로
`schema_version=1`과 `contract=pcv-agent-execution-circuit-breaker-v1`은 유지한다.

50% 완화는 실제 중단 용량에만 적용한다.

| 계약 필드 | 현재 | 변경 | 근거 |
|---|---:|---:|---|
| `default_checkpoint_count` | 1 | 1 | 모호한 재개를 제한하는 구조 규칙이므로 유지 |
| `elapsed_minutes_limit` | 20 | 30 | 시간 예산 1.5배 |
| `tool_batch_limit` | 12 | 18 | 도구 작업 묶음 예산 1.5배 |
| `review_pass_limit` | 1 | 1 | 최초 전체 리뷰는 한 번만 수행 |
| `narrow_rereview_pass_limit` | 1 | 2 | 총 리뷰 예산을 2회에서 3회로 확대 |
| `same_failure_limit` | 2 | 3 | 동일 원인 실패 허용량 1.5배 |
| `progress_warning_percent` | 70 | 70 | 조기 경고는 중단 용량이 아니므로 유지 |

리뷰 예산은 정규 리뷰 1회와 수정된 줄 및 기존 blocker만 보는 제한 재검토 2회로 구성한다.
따라서 총 리뷰 예산은 2회에서 3회가 되어 정확히 50% 증가한다. 제한 재검토에서 새 범주의
결함이 발견되면 남은 재검토 횟수와 관계없이 기존처럼 회로를 연다.

## 3. 적용 시점과 실행 흐름

새 기본값은 정책 변경 커밋 이후 시작하는 checkpoint부터 적용한다. 이미 시작한 checkpoint의
예산은 시작 commentary에서 공개한 값이 끝까지 유효하며 소급 확장하지 않는다.

새 checkpoint는 시작할 때 다음을 공개한다.

- checkpoint 1개
- elapsed budget 30분
- tool batch budget 18회
- 정규 리뷰 1회와 제한 재검토 2회
- 동일 원인 실패 3회 제한
- 허용 파일 또는 bounded directory와 정확한 검증 명령
- 범위 밖 발견은 `report-only`

진행 경고는 계속 70%에서 발생한다. 시간 기준은 21분이며, 정수 단위인 도구 작업 묶음은
`ceil(18 * 0.70)`에 따라 13번째 묶음을 완료한 시점에 경고한다. 시간, 도구 묶음, 동일 원인
실패 또는 리뷰 한도 중 먼저 도달한 조건이 중단을 결정한다.

## 4. 유지되는 안전 경계

다음 규칙은 완화하지 않는다.

- 모호한 `재개`, `계속`, `후속 작업`은 bounded checkpoint 하나만 연다.
- 사용자의 명시적 승인 없이 예산, 범위 또는 checkpoint를 연장하지 않는다.
- 승인 범위 밖 파일군 또는 새 설계가 필요하면 중단한다.
- 범위 밖 발견은 보고만 하고 같은 checkpoint에서 수정하지 않는다.
- 새 `Add-Type`, P/Invoke, native ACL/FileId, process-tree 또는 installer handoff는 별도 probe
  checkpoint가 필요하다.
- 실제 데이터 손실 또는 host mutation 위험이 있으면 추가 작업 없이 중단한다.
- 회로 개방 뒤에는 stop protocol만 허용하며 추가 patch, 새 테스트, 새 하위 에이전트 또는
  전체 재감사를 금지한다.

명시적 사용자 승인은 이후에도 특정 checkpoint의 예산이나 범위를 별도로 조정할 수 있지만,
그 승인은 저장소 기본값을 자동으로 다시 변경하지 않는다.

## 5. 변경 표면

구현은 다음 기존 표면만 변경한다.

1. `config/agent-execution-circuit-breaker.json`
   - 변경된 수치의 기계 판독 단일 진실
2. `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
   - 시작 계약, 진행 경고, 회로 개방 조건과 리뷰 경계의 규범 설명
3. `AGENTS.md`
   - generated current evidence 블록 밖의 필수 요약만 갱신
4. `packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`
   - 기계 계약과 두 문서 표면의 정확한 값 및 유지 경계 검증

기존 `2026-08-23` 설계와 구현 계획은 당시 기준을 보존하는 historical record이므로 수정하지
않는다. 제품 코드, Web Wave B 작업, 운영 evidence, 패키징 도구와 PowerShell 자산의 구조 변경은
이 작업 범위가 아니다.

## 6. 검증 설계

기존 focused Pester 계약 테스트를 TDD guard로 사용한다. 새 PowerShell 파일이나 실행 도구는
추가하지 않는다.

1. 테스트 기대값을 먼저 `30`, `18`, `1`, `2`, `3`, `70`으로 변경한다.
2. 현재 계약과 문서에 대해 실행하여 새 기대값 때문에 실패하는 RED를 확인한다.
3. 기계 계약, 규범 문서와 `AGENTS.md` 요약을 최소 변경한다.
4. 같은 focused 테스트를 다시 실행하여 3개 테스트가 모두 통과하는 GREEN을 확인한다.
5. JSON parse, `git diff --check`, 변경 파일 allowlist와 clean status를 별도로 확인한다.

테스트는 다음 관계도 고정한다.

- `review_pass_limit + narrow_rereview_pass_limit = 3`
- checkpoint와 진행 경고 값은 각각 `1`, `70`으로 유지
- 정책 문서에 30분, 18회, 동일 원인 3회, 정규 리뷰 1회, 제한 재검토 2회가 존재
- `AGENTS.md` generated evidence marker는 각각 하나이며 generated block 내용은 변경하지 않음
- stop-only 규칙, native 확장 경계와 명시적 승인 요구가 보존됨
- public trusted signing, external stable publication 또는 host mutation 성공을 새로 주장하지 않음

실행기 문제로 테스트가 명령 문자열만 반환하는 경우 성공으로 간주하지 않는다. Windows
PowerShell 5.1을 실행 셸로 직접 지정하고 실제 Pester 집계와 종료 코드를 확인한다.

RED가 새 기대값과 기존 값의 차이 때문에 실패하지 않거나 GREEN에 실패가 남으면 정책 변경을
완료로 주장하지 않는다. 추가 파일로 우회하지 않고 현재 diff와 실패 원인을 보고한 뒤 해당
checkpoint의 stop protocol을 따른다.

## 7. 수용 기준

- 기계 계약에 `elapsed_minutes_limit=30`, `tool_batch_limit=18`,
  `review_pass_limit=1`, `narrow_rereview_pass_limit=2`, `same_failure_limit=3`,
  `progress_warning_percent=70`이 정확히 기록된다.
- 총 리뷰 예산은 3회이며 추가 1회는 제한 재검토에만 배정된다.
- checkpoint 1개와 70% 조기 경고가 유지된다.
- 네 기존 변경 표면 외 파일은 구현 diff에 포함되지 않는다.
- focused Pester 결과가 `Passed: 3, Failed: 0`이다.
- JSON parse와 `git diff --check`가 성공하고 작업 트리가 clean하다.
- host, VM, service, package, CI 또는 네트워크 mutation이 실행되지 않는다.

## 8. 비목표

- 모호한 재개 정책을 두 checkpoint 이상으로 확대하지 않는다.
- 진행 경고를 늦추거나 제거하지 않는다.
- 범위, 파괴 작업 또는 저수준 Windows 안전 경계를 완화하지 않는다.
- 기존 Pester guard를 Node로 이전하거나 전체 PowerShell 제거 작업을 포함하지 않는다.
- 제품 버전, package candidate, promotion eligibility 또는 public release claim을 변경하지 않는다.

# 에이전트 실행 회로 차단기

- Contract: `pcv-agent-execution-circuit-breaker-v1`
- Machine contract: `config/agent-execution-circuit-breaker.json`
- Product payload change: `false`
- Host/VM/service/package mutation authorization: `false`

이 문서는 저장소 에이전트 후속 작업의 실행 한도와 종료 동작의 단일 진실이다. 사용자의
명시적 지시가 없는 `재개`, `계속`, `후속 작업`은 다음 bounded checkpoint 하나만 뜻한다.
이 정책 변경 commit 이후 새로 시작한 checkpoint에만 새 기본값을 적용한다. 이미 시작하면서
공개한 예산은 소급하여 확장하지 않는다.

## 시작 계약

write, 외부 mutation 또는 하위 에이전트 시작 전에 commentary에 다음을 공개한다.

- checkpoint: 기본 `1`
- elapsed budget: 기본 `30분`
- tool batch budget: 기본 `18회`
- review budget: 정규 리뷰 1회 + 수정된 줄과 원래 blocker만 보는 제한 재검토 최대 2회
- allowed files 또는 bounded directories
- 완료를 증명할 정확한 검증 명령
- 범위 밖 발견은 `report-only`

도구 작업 묶음은 하나의 목적을 위한 단일 tool call이다. 호출을 불필요하게 쪼개 한도를
우회하지 않는다. 시간과 작업 묶음 중 먼저 도달한 한도가 우선한다.

## 작업 차선

모호한 `재개`는 다음 한 checkpoint이며, 그 checkpoint는 정확히 한 차선에 속한다.

| 차선 | elapsed | tool batch | review | mutation |
| --- | ---: | ---: | --- | --- |
| Lane 0 권위 읽기 | 10분 | 6 | 0 | false |
| Lane 1 계약 | 30분 | 18 | 정규 1 + 제한 재검토 2 | false |
| Lane 2 설치본 프로브 | 45분 | 12 | 정규 1 | 사용자 명시 opt-in |
| Lane 3 승격 | 30분 | 12 | 정규 1 + 제한 재검토 2 | current-evidence만. host mutation 별도 |

Lane 1 경고는 21분과 13번째 묶음이다. Lane 2 경고는 32분과 9번째 묶음이다.
차선을 바꾸면 새 시작 계약을 공개한다. 같은 checkpoint에서 예산을 소급 확장하지 않는다.
Lane 2 `overall_verdict=FAIL` summary는 `actual_vm_tested=pass` 입력이 될 수 없다.
에이전트 종료 보고는 `lane=`, `working_authority=`, `current_evidence_written=false|true`를 포함한다.
`current_evidence_written=true`는 Lane 3가 아니면 즉시 회로를 연다.

## 진행 보고

예산 70% 또는 주요 checkpoint 종료 시 `elapsed/limit`, `tool batches/limit`, 완료 항목,
남은 항목, 범위 밖 발견 수와 다음 stop 지점을 수치로 보고한다. `진행 중`만으로는 충분하지
않다. 경과 시간 경고는 21분(30분의 70%)이고, 도구 작업 묶음 경고는
`ceil(18 * 0.70) = 13`에 따라 13번째 묶음을 완료한 뒤 보고한다.

## 회로 개방 조건

다음 중 하나면 즉시 회로를 연다.

- 경과 30분 또는 도구 작업 묶음 18회
- 동일 원인의 실패를 3회 관측
- 승인 범위 밖 파일군 또는 새 설계가 필요
- 정규 리뷰 1회와 제한 재검토 2회를 모두 사용
- 재검토에서 새 범주의 결함 발견
- 새 `Add-Type`, P/Invoke, native ACL/FileId/process-tree/installer handoff 변경 필요

저수준 Windows 변경은 현재 작업에 붙이지 않는다. 사용자가 승인한 별도 probe checkpoint에서
최소 RED/GREEN 호환성 probe를 먼저 수행하며, 동일 원인으로 세 번 실패하면 확장하지 않는다.

## 회로 개방 후 허용 행동

회로가 열린 뒤에는 다음만 허용한다.

1. 실행 중 명령과 하위 에이전트를 종료한다.
2. `git status`, diff와 이미 끝난 검증 결과를 읽는다.
3. 권한이 있으면 이미 green인 최소 checkpoint만 로컬 보존한다.
4. 완료, 미완료, blocker, 범위 밖 발견과 mutation 여부를 보고한다.

추가 patch는 금지한다. 새 테스트도 금지한다. 새 하위 에이전트와 전체 재감사도 금지한다.
예산 연장이나 다음 checkpoint는 사용자의 명시적 승인 뒤 새 시작 계약으로만 가능하다.

## 리뷰 경계

리뷰어는 고정 diff, 승인 기준과 허용 파일만 검사한다. 정규 리뷰는 1회다. 제한 재검토는
최대 2회이며 수정된 줄과 원래 blocker만 확인한다. 재검토에서 새 범주의 결함을 발견하면
여전히 즉시 회로를 연다. 새 범주의 P0/P1은 심각도와 영향을 보고하지만 같은 checkpoint에서
수정하지 않는다. 실제 데이터 손실 또는 host mutation 위험이면 추가 작업 없이 중단하고
사용자에게 방향과 권한을 요청한다.

# Desktop Node 캠페인 연속 실행 절차

- Design-ID: `pcv-campaign-continuity-procedure-v1`
- 작성일: `2026-09-20`
- 문서 상태: `implemented-in-docs`
- 대상: 공개 저장소 `purecvisor-desktop-node-public`의 에이전트·인간 개발 절차
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`
- public trusted signing: `false`
- external stable publication: `false`

> 실행 라우터는 계속 `docs/DEVELOPMENT_PROCEDURE.md`다. Lane 의미와 FAIL≠current 금지는
> `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md`가
> 소유한다. 이 문서는 **연속 실행이 끊기는 문제**만 고친다.

## 1. 문제

2026-08-27 차선 분리는 권한 혼동과 FAIL→current를 막았다. 그 대가로 개발이 한 checkpoint마다
멈춘다.

1. `vague_resume_policy=one-bounded-checkpoint`는 `다음 단계`를 새 Lane 0/2 프로브로 읽는다.
2. 차선을 바꿀 때마다 사용자 재승인을 요구해서, 이미 승인한 align 작업도 조각난다.
3. 범위 밖 발견(Default Switch 부재, leftover VM, ARP DisplayVersion)이 다음 일이 된다.
4. 기획 스킬의 승인 게이트가 같은 작업을 한 번 더 멈춘다.

04277 ledger 승격 → 설치 Update → `다음 단계`가 current-card 재캡처로 새어 Hyper-V
`PCV_NETWORK_INVENTORY_FAILED`에서 멈춘 것이 이 패턴이다. 사용자는 연속 제품 개발을 원했다.

## 2. 결정

### 2.1 연속 단위는 campaign이다

`docs/ga-ready/active-campaign.json`이 열린 작업의 `intent`, `allowed_lanes`, `next_step`을
소유한다. checkpoint는 한 차선의 예산·권한 경계로 남는다.

기계 계약:

- `campaign_resume_policy`: `continue-open-campaign`
- `vague_resume_policy`: `one-bounded-checkpoint` (campaign이 닫혀 있을 때만)

### 2.2 이미 받은 승인은 되묻지 않는다

사용자가 campaign을 열었거나 같은 세션에서 lane을 명시했으면, 허용된 `next_step`은 재승인
없이 실행한다. 차선 변경 시 새 시작 계약은 공개한다. Git commit, push/PR, host mutation,
current-evidence write는 기존 승인 표를 유지한다. campaign은 없는 승인을 만들지 않는다.

### 2.3 범위 밖 발견은 next_step이 아니다

`out_of_scope_findings=report-only`를 강화한다. leftover VM, ARP DisplayVersion, Default
Switch 부재, 기획 스킬 추가 질문은 campaign `next_step`에 적혀 있거나 사용자가 이름을
부르지 않으면 다음 checkpoint가 되지 않는다.

### 2.4 기획 게이트

campaign이 이미 범위를 준 작업에 별도 설계 승인 대기를 붙이지 않는다. 설계는 같은 턴에서
공개하고 첫 구현 slice를 시작한다. 새 제품 payload나 새 하위 시스템이 필요하면
`new-design-required`로 campaign을 멈추고 그때만 설계 문서를 연다.

### 2.5 기본 제품 개발 campaign

intent `lane1-continuous-development`:

- `working_authority=source_head`
- `allowed_lanes=0,1`
- `mutation_allowed=false`
- `current_write_allowed=false`
- ledger/install/HEAD 불일치는 상태이며 Lane 1을 멈추지 않는다.

04277 align(ledger write + 설치 Update)은 이 설계의 predecessor다. 그 호스트의 current-card
FAIL은 align의 다음 일이 아니다.

## 3. 유지하는 금지

- FAIL 프로브는 current를 쓰지 못한다.
- 한 checkpoint는 한 차선만 소유한다.
- 세 권위를 서로 추정하지 않는다.
- 회로 차단기 예산 소급 확장 금지, `Add-Type`/`P/Invoke` 인접 확장 금지.

## 4. 파일

| 파일 | 역할 |
| --- | --- |
| `docs/ga-ready/active-campaign.json` | 열린 campaign 상태 |
| `docs/DEVELOPMENT_PROCEDURE.md` §1.4, §9 | 실행 라우터 |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | 재개 규칙 |
| `config/agent-execution-circuit-breaker.json` | `campaign_resume_policy`, `vague_resume_policy` |
| `AGENTS.md` 작업 원칙 | campaign 한 줄. generated current 블록은 건드리지 않음 |

## 5. 비목표

- 회로 차단기 제거
- Lane 2/3 자동 연쇄를 기본값으로 열기
- FAIL current-card를 승격
- Default Switch 복구, leftover VM 삭제, ARP msiexec
- public trusted signing / 외부 publication

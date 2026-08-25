# PureCVisor Desktop Node Luna Completion Control Design

- 날짜: `2026-08-03`
- 상태: `approved-design`
- 범위: `csharp-architecture-remaining-waves`
- predecessor: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- current source head at design approval: `4041d1d59ae6b01119bf3887b7d94bdd89415d35`
- operational anchor observed at design approval: `0.42.65-admin-smoke`
- installed non-promoted candidate observed at design approval: `0.42.68-admin-smoke`
- installed candidate evidence: `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`
- active design amendments: `luna-control-bootstrap-v1`, `luna-max-routing-v1`
- amendment approval locators: `User-Approval: luna-control-bootstrap-ultra-20260803`,
  `User-Approval: luna-max-availability-20260803`
- observed available Luna label: `Luna Max (gpt-5/6-luna, max)`
- canonical Luna execution model / reasoning: `gpt-5.6-luna` / `max`
- distribution boundary: `internal-private-network-only`
- public trusted signing: `false`
- external stable publication: `false`

위 두 version은 설계 승인 시점 snapshot이다. 현재 운영 anchor의 authoritative 값과 승격 주장은
`docs/ga-ready/current-evidence.json` 및 그 파일이 가리키는 GA evidence가 소유하고, 설치 후보
사실은 위 candidate evidence가 소유한다. 이 설계의 복사본은 승격 근거가 아니다.

## 1. 목적

현재 C# 구조 개선과 ASP.NET Core 전환 계획은 1,461행과 361개 raw checkbox를 한 문서에서
관리한다. 완료 이력, 조건부 작업, 구현 상태, package 승격, 관리자 승인과 7일 관찰 gate가
같은 목록에 섞여 있어 다음 문제가 발생한다.

- 이미 `code_complete`인 Wave와 아직 열려 있는 형식적 checkbox가 함께 남는다.
- 작은 문서 작업과 package-pair, actual-VM, 7일 관찰이 같은 checkbox 가중치를 갖는다.
- 구현 상태와 운영 승격 상태가 한 진행률 숫자로 오해될 수 있다.
- 장시간 실행 중 context가 압축되면 다음 작업, 허용 파일과 선행조건을 다시 추론해야 한다.
- 관리자 승인이나 위험 등급 상승이 발견됐을 때 안전한 모델 인계 형식이 없다.

이 설계는 기존 계획을 역사적 predecessor로 보존하고, 남은 개발을 모델이 저장소 상태만으로
재개할 수 있는 successor control plane으로 재구성한다. 목표는 체크박스 숫자를 높이는 것이
아니라 내부 사설망용 Desktop Node의 정의된 100% 완료 조건을 증거와 함께 닫는 것이다.

## 2. 확정 결정

1. 수용 기준을 통과한 새 successor가 현재 단일 실행 기준이 된다.
2. 활성화 시 기존 2026-08-02 계획은 완료 이력과 당시 미완료 상태를 보존하는 historical predecessor가 된다.
3. 실행 구조는 얇은 통합 제어 계획, 원자 작업 카드, 기계 판정 상태 원장으로 나눈다.
4. `GPT-5.6-Luna` max는 S/M 변경, `GPT-5.6-Sol`은 L/Release 변경을 담당한다.
5. 내부 사설망 제품 완료를 100% 경계로 유지한다.
6. Public signing, Winget, 외부 stable publication은 완료 조건에 추가하지 않는다.
7. 설치, 서비스, HTTP binding/TLS, Hyper-V와 update/rollback은 사용자 명시 승인 없이 실행하지 않는다.
8. 구현 상태와 승격 상태를 독립된 두 축으로 유지한다.
9. 작업 카드 하나는 한 종류의 변경과 한 commit만 소유한다.
10. 계획과 상태 자체도 schema와 자동 guard로 검증한다.

## 3. 범위

### 포함

- Wave 5A request lifetime, cancellation, task supervision과 service health 완료
- Wave 3 Host Ops 실제 owner 심화
- Wave 4 Hyper-V registry, adapter와 WMI seam 개선
- Wave 7 analyzer, coverage, evidence reader와 문서 정리
- ADR-0014와 ASP.NET Core server compatibility spike
- ASP.NET Core API, static Web, noVNC와 Windows Service lifetime 전환
- `legacy_default -> aspnet_opt_in -> aspnet_default_legacy_retained -> aspnet_only` rollout
- package, installed current-card, 필요한 actual-VM, update/rollback과 관찰 evidence
- 기존 계획의 current/historical 연결과 최종 DoD 정합성

### 제외

- C++ 또는 다른 언어로의 전환
- TypeScript Web Console을 Razor, MVC, Blazor 또는 C# client UI로 교체
- ASP.NET Identity 또는 Entity Framework 도입
- JSON job store의 즉시 SQLite 전환
- worker 병렬 mutation 실행
- public trusted signing, public stable installer URL, Winget submission과 외부 publication
- 일반 사용자 대상 public release
- 관리자 승인 없는 host 또는 Hyper-V mutation 자동 실행

## 4. 불변조건

- Active operator surface는 Web Console과 PCVCLI다.
- TypeScript source, npm build와 browser runtime을 유지한다.
- API route 55개, Hyper-V operation 34개, Host service-action 22개 계약을 승인 없이 변경하지 않는다.
- `System.Net.HttpListener`와 ASP.NET Core transport는 같은 제품 port에서 동시에 실행하지 않는다.
- uncertain mutation을 다른 transport 또는 backend로 자동 재실행하지 않는다.
- Hyper-V mutation worker는 항상 single consumer다.
- Operational anchor는 항상 activation 시 재검증된 `docs/ga-ready/current-evidence.json` 4-tuple이
  소유한다. 설계 승인 snapshot `0.42.65-admin-smoke`를 미래 불변값으로 고정하지 않는다.
- 설계 승인 snapshot의 `0.42.68-admin-smoke`는 read-only 설치 smoke가 있는 미승격 후보이며
  명시적 승격 evidence 없이는 current anchor가 아니다.
- `public_trusted_signing=false`, `external_stable_publication=false`를 유지한다.
- 작업 모델이나 context가 바뀌어도 Git commit graph, 검증된 상태 원장, 작업 카드와 successor
  projection 순서가 재개 근거다.

## 5. 제어 문서 아키텍처

```text
Luna Completion Control
├─ stable design
│  └─ docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-luna-completion-control-design.md
├─ successor controller (current after activation)
│  └─ docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md
├─ atomic task cards
│  └─ docs/superpowers/plans/luna-completion/<CARD-ID>-<slug>.md
├─ machine state
│  ├─ docs/superpowers/plans/luna-completion/execution-state.json
│  └─ docs/superpowers/plans/luna-completion/execution-state.schema.json
└─ validation
   ├─ packaging/windows-desktop-node/tools/Test-PcvLunaExecutionPlan.ps1
   └─ packaging/windows-desktop-node/tests/PcvLunaExecutionPlan.Tests.ps1
```

### 단일 진실 소유권

| 정보 | 단일 owner |
|---|---|
| 범위, 모델 정책, 상태 전이, 승인 경계, 100% 완료의 규범 | stable design |
| Wave 순서, 의존성, required card/observation 집합, 완료 projection과 current task 선택 정책 | successor |
| 파일 범위, 테스트, 구현·rollback·commit 계약 | task card |
| mutable current task pointer, 현재 task 상태, exact HEAD, evidence와 관찰 상태 | execution state |
| 실제 검증 결과와 제품 승격 주장 | `docs/ga-ready/evidence/**` |
| current operational anchor | `docs/ga-ready/current-evidence.json`과 참조 evidence |
| installed non-promoted candidate 사실 | 해당 installed candidate evidence |
| 현재 개발 진입점 | `docs/DEVELOPER_INDEX.md` |

같은 사실을 여러 문서에서 수동으로 반복하지 않는다. Successor의 current task 표시는
execution state의 `current_task_id`를 projection하며, card ID와 상태 요약 외에 카드 본문이나
evidence 결과를 복사하지 않는다.

### 활성화 경계

이 문서는 `approved-design` 상태이며 successor를 아직 활성화하지 않는다. 활성화는 두 단계다.

1. Materialization LC card series는 successor, state, schema와 guard를 추가하되 predecessor를
   current로 유지한다. 마지막 control-only materialization anchor commit을 대상으로 post-commit
   guard와 required CI를 PASS한다.
2. Activation commit은 검증된 materialization anchor commit과 CI run을 참조하고
   `docs/DEVELOPER_INDEX.md`, predecessor의 Superseded note와 state activation pointer만 원자적으로
   바꾼다. 또한 작성 시점 designated current branch의 exact HEAD를 `activation_base_head`로
   기록한다. Branch protection은 최신 base 반영과 merge-group required CI 재실행을 강제한다.

그 전까지 2026-08-02 predecessor가 current execution plan이다. Activation commit이 CI 또는
merge 전에 있는 branch에서는 successor를 current로 주장하지 않는다.
Materialization anchor 검증과 activation 직전에 각각 `docs/ga-ready/current-evidence.json`을 다시 읽어
version, operational MSI SHA-256, payload SHA-256, provenance commit과 두 public claim을 state
snapshot과 비교한다. 하나라도 다르면 activation을 차단하고 LC rebaseline materialization을 다시
수행한다.

Designated branch HEAD가 `activation_base_head`에서 바뀌면 기존 activation CI는 무효이며 rebase,
snapshot 비교와 required CI를 다시 수행한다. Effective current 판정은 다음 조건을 모두 요구한다.

```text
activation commit is reachable from designated current branch
AND activation required/merge-group CI passed against latest activation_base_head
AND post-merge guard passed for the exact merged commit
AND post-merge current-evidence 4-tuple/public claims equal the activation snapshot
```

### 일회성 control bootstrap amendment

`Design-Amendment: luna-control-bootstrap-v1`은 아직 card와 state가 없는 최초 control plane을 만들기
위해 다음 예외를 한 번만 허용한다. 이 예외는 successor의 범위나 제품 구현 권한을 넓히지 않는다.

1. `LC-001` 시작 전에 base `Plan-ID`, active `Plan-Revision`과 그 revision의 `User-Approval` locator를
   Git graph에서 각각 유일하게 해석한다. Revision commit은 base plan commit의 descendant이고 셋 모두
   latest designated `main`의 ancestor여야 한다. Squash/rebase로 locator가 사라졌거나 중복되면
   `blocked/bootstrap-authority-invalid`로 중단한다.
2. Amendment와 active revision이 commit-preserving merge로 `main`에 들어간 뒤, 사용자가 exact fresh
   `main` HEAD를 기준으로 별도 materialization 승인을 제공해야 한다. Design/revision 승인이나 이
   amendment의 승인을 materialization 승인으로 자동 승계하지 않는다. 승인 locator는
   `User-Approval: luna-control-materialization-<approved-base-12>-<yyyymmdd>` 형식으로 repository에서
   유일해야 한다. Initial state의 `bootstrap_provenance.materialization_approval`에는 locator, exact 40자
   `approved_base_head`, durable `approval_source_ref`, 승인 시각과 `control-only-lc-001-through-lc-023`
   scope를 기록한다. `approved_base_head`는 승인 시 latest `main`, `LC-001.start_head`와 LC-001 result의
   first parent가 모두 같아야 하며 result commit의 ancestor여야 한다.
3. Fresh materialization branch에서 변경 전 `git rev-parse HEAD`를 exact 40자
   `LC-001.start_head`로 기록한다. Mutable branch name이나 plan commit hash를 대신 쓰지 않는다.
4. `LC-001` result transaction만 자신의 card, 나머지 LC control-card 정의, state schema, initial state,
   canonical graph/template source와 schema valid/invalid fixture/test를 함께 생성할 수 있다. Initial state는
   resolved plan authority와 17개 weekly forecast를 포함하고 `program_status=design_approved`로 시작한다.
   `LC-001`은 result locator로 완료를 표시하고 `LC-002`~`LC-028` 각각의 state record를 `pending`으로
   seed한다. Missing, duplicate 또는 extra LC state record가 있으면 bootstrap validation은 실패한다.
5. 이 transaction은 product source, Web source, GA evidence, current-evidence pointer, predecessor current
   pointer, package/service/HTTP/Hyper-V state를 변경할 수 없고 host mutation을 수행할 수 없다.
6. `LC-001` result commit에는 `Card-ID: LC-001`을 두되 자기 commit hash를 같은 commit 내용에 쓰지
   않는다. 이후 모든 card는 실행 전에 card와 state record가 존재해야 하며 bootstrap 예외를 재사용할
   수 없다.
7. 모든 required future card와 guard가 materialize되고 `LC-023` inactive anchor가 PASS한 result에서만
   `program_status=materialized_inactive`로 전환한다. 그 전에는 predecessor가 current다.

Validator는 `bootstrap`, `materialized_inactive`, `active` validation phase를 구분하고 amendment locator,
amendment approval, revision approval와 별도 materialization approval locator, approved-base ancestry 및
금지 path를 서로 구분해 강제한다. `LC-001`~`LC-023` 전체는 product/Web source, GA/current evidence,
current pointer와 predecessor를 바꿀 수 없고 package/service/HTTP/Hyper-V mutation을 수행할 수 없다.
각 result와 ledger event에서 이 forbidden-path/host-mutation guard를 다시 적용한다. 모델이 unavailable하면 bootstrap도
`blocked/model_unavailable`로 멈추며 Sol 또는 다른 모델로 자동 대체하지 않는다.
Bootstrap amendment 직후 successor v1의 S/M `medium|high` 표기를 v2 `ultra`로 정렬했던 이력은
보존한다. 아래 Luna Max amendment가 적용된 뒤에는 v2 `ultra` projection이 stale이다. S/M `max`, weekly
projection과 active approval locator를 정렬한 successor Plan-Revision v3가 latest `main` ancestor가 될
때까지 `LC-001`은 `blocked/active-revision-stale`다.

### Luna Max availability amendment

사용자가 실제 선택 가능한 Luna가 `Luna Max (gpt-5/6-luna, max)`뿐임을 재확인했다. 저장소와 실행
schema의 canonical model ID는 기존 dotted identifier `gpt-5.6-luna`를 유지하고 `gpt-5/6-luna`는 UI
display alias로만 기록한다. Slash alias를 card의 `execution_model` 값으로 쓰지 않는다.

`Design-Amendment: luna-max-routing-v1`은 다음을 규범으로 추가한다.

1. S와 M card의 `execution_model`은 `gpt-5.6-luna`, `reasoning`은 정확히 `max`다. S/Fast와 M/Full
   verification lane은 바꾸지 않는다.
2. L/Release card는 계속 `gpt-5.6-sol`과 `high|ultra`를 사용한다. Luna Max availability를 이유로 L을
   Luna에 배정하거나 change tier를 낮추지 않는다.
3. Validator는 model-specific reasoning 조합을 강제한다. Luna에 `medium|high|ultra`, Sol에 `max`가
   나오면 `PCV_LUNA_PLAN_MODEL_REASONING_INVALID`로 실패한다.
4. `LC-001` transaction 시작 전 runtime selector 관찰값의 `selected_model_label`,
   `resolved_execution_model`과 durable evidence ref를 기록한다. `gpt-5/6-luna`가 정확히
   `gpt-5.6-luna`로 resolve됨을 확인할 수 없거나 다른 execution ID로 resolve되면 파일, state 또는
   result commit을 만들지 않고 `blocked/model_identifier_unresolved`로 중단한다. 사용자에게 보이는
   display label만으로 이 mapping을 증명한 것으로 간주하지 않는다.
5. Luna Max가 callable하지 않으면 `blocked/model_unavailable`이며 Sol/Terra 또는 다른 모델로 자동
   대체하지 않는다.
6. 기존 materialization approval
   `User-Approval: luna-control-materialization-dbac0ae5abd8-20260803`은 v2/ultra 및 당시 exact main에
   결합돼 실제 LC transaction에 사용되지 않은 stale approval이다. Max amendment, successor v3와 파생
   문서가 merge된 뒤 새 exact `main` SHA를 대상으로 별도 materialization 승인을 다시 받는다.

Amendment commit은 `Design-Amendment: luna-max-routing-v1`과
`User-Approval: luna-max-availability-20260803` locator를 함께 가지며 stable design 외 제품/GA/current
pointer 또는 host state를 바꾸지 않는다.

## 6. 원자 작업 카드 계약

모든 카드에는 다음 필드가 있어야 한다.

```yaml
card_id: LC-000
wave: control
title: example
change_tier: S | M | L
execution_model: gpt-5.6-luna | gpt-5.6-sol
reasoning: max | high | ultra
verification_lane: Fast | Full | Release
dependencies: []
start_head_ref: execution-state.json#/tasks/LC-000/start_head
allowed_paths: []
forbidden_paths: []
invariants: []
behavior_change: false
package_candidate: false
host_mutation_required: false
hyperv_mutation_required: false
approval_checkpoints: []
```

본문은 다음 순서로 작성한다.

1. 결과와 사용자 영향
2. 선행조건과 state가 소유한 시작 HEAD reference
3. 허용 파일과 금지 파일
4. 보존할 route, JSON, CLI, Web, Hyper-V 계약
5. RED 또는 characterization 재현
6. 최소 구현 단계
7. focused 검증과 effective Fast/Full/Release 명령
8. evidence와 문서 갱신 범위
9. rollback과 중단 조건
10. 단일 commit 메시지
11. `code_complete` 또는 operational pending 판정 조건

카드는 기본적으로 production 파일 1~5개를 변경한다. 이를 넘거나 두 개 이상의 behavior
contract를 바꾸면 분할한다. 구조상 분할할 수 없으면 실행 전에 change tier와 모델 배정을
재검토한다. 한 카드는 production/test와 그 변경에 직접 귀속되는 문서·최종 상태를 하나의
atomic result commit으로 닫는다. 별도 운영 실행이나 GA evidence가 필요하면 `OP` card로 분리하고
구현 card에 섞지 않는다. 모든 result commit에는 유일한 `Card-ID: <CARD-ID>` trailer를 둔다.
Lease, approval, handoff, block과 observation 진행은 control plane만 변경하는 durable
ledger-event commit으로 기록하며 card result commit으로 세지 않는다. Ledger-event commit은
production/test/task-card 파일을 바꿀 수 없다.
Pending state의 `start_head`는 `null`일 수 있지만, `ready` 전이 ledger-event commit에서 exact 40자
base commit으로 설정해야 한다. Card는 mutable hash를 복사하지 않고 stable `start_head_ref`만 둔다.

### 카드 ID namespace

| Prefix | 소유 범위 |
|---|---|
| `LC` | completion control과 baseline |
| `LT` | Wave 5A lifetime, cancellation과 supervision |
| `HO` | Wave 3 Host Ops |
| `HV` | Wave 4 Hyper-V |
| `QG` | Wave 7 quality와 evidence |
| `AC` | Wave 6 ASP.NET Core |
| `OP` | package, installed, actual-VM과 observation |
| `FC` | final closure |

## 7. 모델 라우팅

| 등급 | 실행 모델 | Reasoning | 필수 검증 | 대표 작업 |
|---|---|---|---|---|
| S | `gpt-5.6-luna` | `max` | focused + Fast | 문서, fixture, 국소 test |
| M | `gpt-5.6-luna` | `max` | Full | owner 이동, 비파괴 API/Web 계약 |
| L | `gpt-5.6-sol` | `high` 또는 `ultra` | Release + 요구 evidence | 동시성, 보안, transport, installer |

다음 조건은 입력 등급과 관계없이 L/Release와 Sol로 승격한다.

- auth, account, RBAC, JWT, token 또는 TLS 의미 변경
- request lifetime, cancellation, persistence 또는 reconciliation 의미 변경
- service, port, URL ACL, SSL binding, firewall 또는 Event Log 변경
- package, install, repair, update, rollback 또는 uninstall 변경
- WMI provider 의미나 Hyper-V/actual-VM mutation 변경
- operational anchor 또는 current evidence 변경

Luna는 위험 등급을 낮출 수 없다. 실행 중 승격 조건을 발견하면 현재 diff를 삭제하거나
숨기지 않고 `handoff_required`로 전환한다. 인계에는 exact HEAD, changed paths, diff summary,
실행한 tests, 실패와 미해결 결정을 포함한다.

인계받은 Sol은 같은 card의 effective tier/model/lane을 L/Sol/Release로 고치되 최초 계획값과
승격 사유를 state의 `routing_history`에 보존한다. 이 기록이 없으면 validator가 Luna→Sol 변경을
거부한다. 모델 자체가 unavailable하거나 rate limit 상태인 경우 자동 대체하지 않고
`blocked`와 `model_unavailable`을 기록해 사용자에게 모델 예외 승인을 요청한다. L/Release card는
구현 주체와 별도의 review 또는 required CI가 `review_status=pass`여야 완료할 수 있다.

Public signing, Winget 또는 external publication 경계 변경은 등급 승격 대상이 아니라
`blocked_out_of_scope` 사유로 중단한다. 별도 ADR, 별도 program과 사용자 명시 승인이 없으면 이
completion program 안에서 수행하지 않는다.

## 8. 상태 원장

상태 원장은 카드의 실행, 구현, 승격, 검증과 승인을 별도 축으로 저장하고 관찰을 독립 record로
관리한다.

### 프로그램 상태

- `design_approved`
- `materialized_inactive`
- `active`
- `control_recovery_blocked`
- `complete`

활성화 후 control-plane 장애가 발생해도 predecessor를 다시 current로 만들지 않고
`control_recovery_blocked`에서 마지막 검증 snapshot을 복원한다.
Activation commit의 선언 상태는 `active`지만 effective current 판정은 required CI PASS와
designated current branch reachability, latest-base validation, post-merge guard와 evidence snapshot
일치를 모두 요구한다.

### 실행 상태

- `pending`
- `ready`
- `stale`
- `in_progress`
- `handoff_required`
- `approval_required`
- `blocked`
- `completed`

`stale`은 production 변경 전에 HEAD 또는 dependency drift가 확인된 카드에만 사용한다.
카드의 시작 조건을 다시 검토하고 `start_head`와 dependency를 갱신한 뒤 `pending`으로 되돌린다.

### 구현 상태

- `not_started`
- `code_ready_operational_pending`
- `code_complete`
- `closed-not-adopted`

`closed-not-adopted`는 inherited Wave 5B에만 허용한다.

### 승격 상태

- `promotion_not_triggered`
- `promotion_pending`
- `promotion_complete`

### 검증 결과

- `not_run`
- `pass`
- `fail`

실행 상태 `completed`는 검증 결과가 `pass`일 때만 허용한다. `fail`은 현재 카드를 유지하며
다음 카드 실행을 허용하지 않는다.

### 검토 상태

- `not_required`
- `pending`
- `pass`
- `fail`

L/Release card에는 `not_required`를 사용할 수 없다.

### 승인 상태

- `not_required`
- `required`
- `approved`
- `executed`
- `blocked`

이 값은 실행 상태와 별도인 checkpoint substate다. Host mutation card가 실행 상태
`approval_required`에 들어가면 승인 상태는 `required`여야 한다.

### 관찰 상태

- `not_started`
- `running`
- `restarted`
- `passed`
- `failed`

`restarted`는 현재 attempt를 PASS 계산에서 제외하고 그 4-tuple, 기간과 무효화 사유를 history에
보존한 뒤 새 target 4-tuple을 고정해 새 7일 attempt를 시작한다. 새 tuple은 이전과 같을 수도,
payload 교체로 달라질 수도 있다.

### 필수 top-level state

```json
{
  "schema_version": 1,
  "program_id": "purecvisor-desktop-node-luna-completion-20260803",
  "program_status": "materialized_inactive",
  "predecessor": "docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md",
  "ledger_base_head": "4041d1d59ae6b01119bf3887b7d94bdd89415d35",
  "activation": {
    "validated_materialization_commit": null,
    "materialization_ci_ref": null,
    "activation_base_head": null,
    "activation_commit_locator": "Program-Activation: purecvisor-desktop-node-luna-completion-20260803",
    "designated_current_branch": "main"
  },
  "operational_snapshot": {
    "evidence_ref": "docs/ga-ready/current-evidence.json",
    "version": "0.42.65-admin-smoke",
    "operational_msi_sha256": "9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30",
    "payload_sha256": "5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28",
    "provenance_commit": "4855947fe0199cedc978e8b40ffb45e96ced6876"
  },
  "claims_snapshot": {
    "public_trusted_signing": false,
    "external_stable_publication": false
  },
  "observed_installed_candidate": "0.42.68-admin-smoke",
  "observed_installed_candidate_evidence_ref": "docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md",
  "current_task_id": null,
  "lease": null,
  "tasks": {
    "LC-001": {
      "card_id": "LC-001",
      "card_ref": "docs/superpowers/plans/luna-completion/LC-001-control-activation.md",
      "execution_status": "pending",
      "implementation_status": "not_started",
      "promotion_status": "promotion_not_triggered",
      "verification_result": "not_run",
      "review_status": "not_required",
      "actual_execution_model": null,
      "actual_reasoning": null,
      "actual_verification_lane": null,
      "execution_task_ref": null,
      "routing_history": [],
      "start_head": null,
      "result_commit_locator": "Card-ID: LC-001",
      "test_results": [],
      "evidence_refs": [],
      "approval_records": [],
      "observation_refs": [],
      "review_ref": null,
      "block_reason": null,
      "unblock_reason": null,
      "failure_fingerprint": null,
      "attempt_count": 0
    }
  },
  "observations": {
    "OBS-LIFETIME-LEGACY-TRACKED": {
      "required": true,
      "state": "not_started",
      "version": null,
      "operational_msi_sha256": null,
      "payload_sha256": null,
      "provenance_commit": null,
      "workspace_source_head": null,
      "current_attempt_id": null,
      "started_at": null,
      "ended_at": null,
      "p0_p1_regression_count": null,
      "attempts": [],
      "restart_count": 0
    },
    "OBS-ASPNET-DEFAULT": {
      "required": true,
      "state": "not_started",
      "version": null,
      "operational_msi_sha256": null,
      "payload_sha256": null,
      "provenance_commit": null,
      "workspace_source_head": null,
      "current_attempt_id": null,
      "started_at": null,
      "ended_at": null,
      "p0_p1_regression_count": null,
      "attempts": [],
      "restart_count": 0
    },
    "OBS-ASPNET-ONLY": {
      "required": true,
      "state": "not_started",
      "version": null,
      "operational_msi_sha256": null,
      "payload_sha256": null,
      "provenance_commit": null,
      "workspace_source_head": null,
      "current_attempt_id": null,
      "started_at": null,
      "ended_at": null,
      "p0_p1_regression_count": null,
      "attempts": [],
      "restart_count": 0
    }
  }
}
```

`ledger_base_head`와 card `start_head`는 state가 담긴 commit 자신의 hash가 아니라 해당 card의
production 변경을 시작한 base commit이다. `result_commit` hash를 같은 commit 안에 써서
자기참조하지 않는다.
대신 card의 유일한 `Card-ID` trailer를 `result_commit_locator`로 저장하고, post-commit guard가 Git
graph에서 정확한 result commit을 해석한다. `start_head`는 result commit의 ancestor여야 하며,
사이에 있는 commit은 `Ledger-Event: <event-id>` trailer를 갖고 `execution-state.json`만 변경해야
한다. Exact result hash는 post-commit evidence가 소유한다. Card의 `start_head_ref`는 이 state
field를 가리키며 hash를 중복 소유하지 않는다.

동시에 `in_progress`인 card와 active lease는 전체 program에서 최대 하나다. 관찰 record는
`running`인 채로 card 실행과 병행할 수 있지만, P2를 포함한 card commit은 직렬화한다. 같은 파일
또는 authoritative test를 수정하는 두 card를 동시에 실행하지 않는다. Intermediate lease와
`in_progress|handoff_required|approval_required|blocked`, approval과 observation 변경은 각각
durable ledger-event commit으로 보존한다. 완료되는 production/test 변경은 card당 하나의 result
commit만 만든다.

## 9. 상태 전이

```text
pending -> ready
ready|in_progress -> stale -> pending
ready -> in_progress -> completed
in_progress -> approval_required -> in_progress
in_progress -> handoff_required -> in_progress
pending|ready|in_progress|approval_required|handoff_required -> blocked -> pending
```

- 선행 카드가 모두 완료되고 start HEAD가 맞아야 `ready`가 된다.
- `stale`은 시작 조건을 다시 고친 뒤 `pending`으로만 되돌아간다.
- `in_progress -> stale`은 production/test diff가 0건일 때만 허용한다. Diff가 이미 있으면
  `handoff_required` 또는 명시적 replan을 사용한다.
- 작업 시작 시 current task lease와 exact HEAD를 함께 기록한다.
- 검증 실패 상태에서는 다음 카드로 이동하지 않는다.
- 같은 원인의 실패가 세 번 반복되면 `blocked`로 전환한다.
- `approval_denied`는 `approval_required -> blocked`, `model_unavailable`은
  `ready|handoff_required -> blocked`, `blocked_out_of_scope`는
  `pending|ready|in_progress|approval_required|handoff_required -> blocked`, 동일 실패 3회는
  `in_progress -> blocked`에서만 허용한다.
- `blocked`는 외부 조건 변경, 새 사용자 결정 또는 검증된 replan을 `unblock_reason`으로 기록한 뒤
  `pending`으로만 되돌아간다. 단순 재시도는 해제 사유가 아니다.
- `completed`는 `verification_result=pass`를 요구하는 실행 상태이며 code/promotion 완료를 자동으로
  의미하지 않는다.
- `code_complete`에는 필수 test, lane summary와 evidence reference가 필요하다.
- `promotion_complete`에는 package와 해당 card가 요구한 operational evidence가 필요하다.

나머지 축의 합법 전이는 다음과 같다.

```text
program:
  design_approved -> materialized_inactive -> active -> complete
  active -> control_recovery_blocked -> active

implementation:
  not_started -> code_ready_operational_pending -> code_complete
  not_started -> code_complete
  not_started -> closed-not-adopted  # inherited Wave 5B only

promotion:
  promotion_not_triggered -> promotion_pending -> promotion_complete
  promotion_pending -> promotion_not_triggered  # candidate 폐기 evidence가 있을 때만

verification:
  not_run -> pass
  not_run -> fail -> pass

review:
  not_required  # S/M only
  pending -> pass|fail  # L/Release
  fail -> pending  # 새 review 대상 commit이 있을 때만

observation:
  not_started -> running -> passed|failed
  running -> restarted -> running
```

## 10. 실행 구간과 의존성

```text
P0 control and rebaseline
  -> P1 Wave 5A safety completion
     -> P1-Ops tracked lifetime installed validation and 7-day observation
         -> P3 ASP.NET Core server decision and exclusive seam
            -> P4 ASP.NET Core parity and opt-in readiness
               -> P5 ASP.NET Core default package and 7-day observation
                  -> P6 HttpListener removal package and 7-day observation
                     -> P7 final acceptance

P1-Ops observation window
  -> P2 Wave 3 Host Ops
  -> P2 Wave 4 Hyper-V seam
  -> P2 Wave 7 quality and evidence
  -> non-product ADR-0014/server spike

P2 workstreams must all close before P7. They are eligible while the observation runs, but task cards
remain globally serialized.
```

### P0. Completion control과 rebaseline

- successor, task card, state schema와 validation guard를 추가한다.
- 현재 HEAD, branch, test count, coverage fixture, route/operation/action manifest를 재확인한다.
- Wave 0/1/2와 Wave 5B 완료는 inherited scope로만 기록한다.
- stale와 conditional checkbox를 실행 작업으로 잘못 가져오지 않는다.
- 기존 untracked 사용자 문서를 수정하거나 stage하지 않는다.

### P1. Wave 5A 안전성 완료

Sol L 카드로 다음을 분리한다.

- GET timeout 이후 late commit 0건
- request cancellation과 durable job lifetime 분리
- listener/worker fault의 Windows Service health 전파
- noVNC와 HTTP child task shutdown drain
- accepted context task 생성 전 admission과 bounded waiting
- disconnect-before/after-commit 정확성

### P1-Ops. Tracked lifetime 승격

- installed serialized/admission load
- service stop/start와 in-flight drain
- account/session/RBAC, target-backed noVNC와 diagnostics
- queued mutation, cancel과 recovery
- closed package-pair와 정확한 rollback
- `OBS-LIFETIME-LEGACY-TRACKED`에서 payload hash 고정 상태의 7일 P0/P1 회귀 0건
- 조건 충족 후 legacy request lifetime mode 제거

### P2. 관찰과 병행하는 구조·품질 작업

- Host Ops family별 callback-free owner 이동
- Hyper-V canonical registry와 read provider seam
- WMI mutation 의미 변경은 Sol L 카드로 분리
- evidence reader owner 분리
- analyzer와 warning-as-error 정책
- `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json`에 current
  source snapshot, SDK/collector version, test count와 project별 line/branch coverage를 재수집하고
  0.0%p ratchet 적용
- current/historical evidence link 정리

P2는 P1-Ops 관찰 payload를 변경하지 않는 card로 진행한다. Workspace source HEAD가 바뀌어도
현재 설치된 `observed_payload_hash`가 같으면 관찰은 계속되며, P2 변경을 포함한 새 package를
설치하는 순간 기존 기간을 인정하지 않고 `restarted`로 판정한다.

### P3. ASP.NET Core server 결정과 exclusive seam

- ADR-0014에서 HTTP.sys 또는 Kestrel 하나를 product server로 선택한다.
- non-product dynamic loopback spike로 binding, WebSocket, TLS owner와 service lifetime을 확인한다.
- exclusive transport selector를 추가한다.
- existing application owner와 single mutation worker를 공유한다.

### P4. ASP.NET Core parity와 opt-in readiness

- 55-route, static/CORS/body/auth/noVNC exact parity를 검증한다.
- TestServer와 선택 server actual loopback socket을 모두 검증한다.
- TypeScript build output만 static asset으로 제공한다.
- legacy 기본값을 유지한 opt-in publish/MSI preflight와 rollback 명령을 검증한다.

### P5. ASP.NET Core 기본값 전환

- package build와 hash/provenance를 고정한다.
- 별도 승인으로 installed current-card와 필요한 actual-VM/admin smoke를 실행한다.
- 직전 legacy package와 install/update/rollback pair를 닫는다.
- `OBS-ASPNET-DEFAULT`에서 ASP.NET Core default, legacy retained 상태의 7일 P0/P1 회귀 0건을
  기록한다.

### P6. HttpListener 제거

- legacy `DesktopNodeHostApplication(HttpListener)` production reachability를 제거한다.
- 제거를 새 product payload candidate로 취급한다.
- clean install, update, repair, uninstall과 직전 package rollback을 검증한다.
- 별도 closed package-pair와 `OBS-ASPNET-ONLY` 제거 후 7일 P0/P1 회귀 0건을 기록한다.
- 최종 `promotion_complete`는 이 legacy-removal package에 귀속한다.

### P7. 최종 closure

- 모든 Wave와 card 상태를 재계산한다.
- 전체 .NET/Web/Pester, coverage, analyzer, architecture와 diff guard를 실행한다.
- installed/current-card/actual-VM/rollback evidence와 final hashes를 연결한다.
- predecessor의 최종 수용 28개를 `FA-001`~`FA-028`, Definition of Done 12개를
  `DOD-001`~`DOD-012`로 mapping해 총 40개를 기계 판정한다.
- successor를 complete로 전환하고 남은 항목을 별도 backlog 또는 ADR 후보로 이동한다.

## 11. 관리자 승인 checkpoint

승인은 네 category로 분리한다. `package_service`만 build 전과 install 전의 두 stage를 가진다.

| Checkpoint | 범위 |
|---|---|
| `package_service/build` | source HEAD, version, build recipe와 internal LocalTest/AllowUnsignedDev signing mode 고정 |
| `package_service/install` | 생성된 artifact hash 확인 후 install과 service reconfiguration |
| `http_binding_tls` | URL ACL, SSL binding, certificate, product port smoke |
| `hyperv_actual_vm` | Hyper-V와 actual-VM mutation |
| `lifecycle_rollback` | update, rollback, clean-host와 manual-admin package-pair |

한 card는 `approval_checkpoints` 배열로 여러 checkpoint를 요구할 수 있으며 각 항목은 독립된
approval record와 상태를 가진다. 각 승인 요청은 다음을 고정한다.

- exact HEAD와 version
- build stage의 recipe, signing mode, expected artifact path와 host install/service mutation 0건 경계
- install stage의 package, payload와 provenance hash
- 실행할 정확한 명령과 host/VM target
- 허용 mutation과 금지 mutation
- PlanOnly/Release preflight 결과
- rollback 명령과 expected final state
- 수집할 evidence 위치

이 program의 package approval은 `public_trusted_signing=true`를 허용하거나 증명하지 않는다.

승인이 필요한 mutation의 composite 전이는 다음 순서만 허용한다.

```text
(in_progress, required)
  -> (approval_required, required)
  -> (approval_required, approved)
  -> (in_progress, approved)
  -> exact approved host mutation command
  -> (in_progress, executed)
  -> verification/evidence
  -> completed
```

Approval이 `required`인 채로 `in_progress`에 복귀하거나 `approved` 전에 mutation을 실행할 수 없다.
`executed`는 승인된 command, target과 hash가 실제 실행된 뒤에만 기록한다. 여러 checkpoint가 있는
card는 다음 mutation에 해당하는 모든 approval record가 `approved`여야 한다. 어느 단계에서든
거부되면 approval과 execution을 `blocked`로 기록한다. Git push, PR Ready와 merge 승인은 이
checkpoint와 별도이며 하나의 승인이 다른 종류의 host mutation 권한으로 확장되지 않는다.

## 12. 관찰 gate

각 관찰은 다음을 기록한다.

- observation ID와 대상 card/package
- `required` 여부와 state
- exact version, MSI SHA-256, payload aggregate SHA-256와 provenance commit
- `observed_payload_hash`와 별도 `workspace_source_head`
- current attempt ID, 이전 attempt별 4-tuple·기간·무효화 사유 history와 restart count
- 시작·종료 시각과 최소 7일 충족 여부
- P0/P1 transport 또는 lifetime regression count
- service restart, recovery와 rollback event
- 관찰 중 product payload 변경 여부

관찰 중 product payload hash가 바뀌면 해당 관찰을 `restarted`로 기록하고 7일을 다시 시작한다.
문서만 바뀌고 product payload aggregate가 같으면 관찰 기간을 유지한다. 관찰 기간은 다음
작업을 기다리는 idle 시간이 아니며, 대상 payload를 바꾸지 않는 P2 작업과 non-product spike를
관찰과 시간상 병행한다. Card 실행 자체는 전역 직렬화 규칙을 유지한다.

각 required observation은 version, operational MSI SHA-256, payload aggregate SHA-256와
provenance commit의 4-tuple을 소유한다. 특히 최종 current evidence, installed current-card,
rollback evidence와 `OBS-ASPNET-ONLY`의 4-tuple은 모두 같아야 한다. 다른 payload를 설치하면
관찰을 조기 종료하거나 carry-forward하지 않고 `restarted`로 새 7일을 측정한다.
Restart ledger-event는 기존 attempt를 history에 먼저 append한 뒤 새 `current_attempt_id`, 4-tuple과
시작 시각을 원자적으로 기록해야 한다.

Successor의 required observation 집합은 정확히 다음 세 ID를 포함한다.

- `OBS-LIFETIME-LEGACY-TRACKED`
- `OBS-ASPNET-DEFAULT`
- `OBS-ASPNET-ONLY`

## 13. 계획 자동 검증

`Test-PcvLunaExecutionPlan.ps1`와 Pester guard는 다음을 검사한다.

- task card ID 중복 0건
- 모든 dependency와 card path 존재
- dependency graph cycle 0건
- 동시에 `in_progress`인 card 최대 1개
- tier, model, reasoning과 verification lane 조합 일치
- L escalation path의 Luna 배정 0건
- planned/actual model, reasoning, execution task ref와 routing history 존재
- L/Release card의 independent review 또는 required CI `review_status=pass`
- 모든 state transition의 합법성
- current task start HEAD와 lease 시작 시 Git HEAD 일치
- post-commit `Card-ID` locator 유일성, start HEAD ancestor 관계와 중간 ledger-only commit path
- `Ledger-Event` ID 유일성, `execution-state.json` 외 변경 0건
- `completed` card의 `verification_result=pass`
- 완료 card의 required test/evidence/commit 누락 0건
- 승인 없는 mutation 실행 상태 0건
- promotion evidence 없는 `promotion_complete` 0건
- package/current-card/rollback/required observation 4-tuple 불일치 0건
- observation restart count, current attempt와 immutable attempt history 정합성
- required observation `state=passed`, current attempt 최소 7일과 P0/P1 회귀 0건
- successor, state, card와 DEVELOPER_INDEX current link 일치
- predecessor superseded note와 successor backlink 존재
- activation이 검증된 materialization anchor/CI와 latest designated branch HEAD를 참조하는지
- merge-group CI와 exact merged commit post-merge guard가 PASS했는지
- activation 직전 current-evidence 4-tuple/public claim과 operational snapshot 불일치 0건
- final closure 시 current/historical evidence index 불일치 0건

이 guard는 문서 control plane 추가 시 RED/GREEN으로 구현하고 이후 Full/Release lane에 포함한다.

## 14. 검증 전략

### Control plane

- JSON schema valid/invalid fixture
- dependency missing/cycle fixture
- duplicate ID와 multiple in-progress fixture
- tier/model mismatch와 escalation fixture
- illegal state transition fixture
- approval/evidence 없이 완료 상태를 설정하는 negative fixture
- observation payload drift/restart fixture
- Git commit locator/ancestor, ledger-only path와 ledger 자기참조 방지 fixture
- materialization/activation commit과 CI/merge 경계 fixture

### 각 작업 카드

- behavior change는 RED test를 먼저 고정한다.
- body move는 characterization과 ownership guard를 먼저 둔다.
- S는 focused + Fast, M은 Full, L은 Release를 실행한다.
- `PlanOnly`는 suite selection 확인이며 완료 evidence로 사용하지 않는다.
- test count, skip, coverage, commit과 source snapshot을 evidence에 기록한다.

### 최종 검증

- `dotnet test src/DesktopNode.sln -c Release`
- npm type/static/parity/browser fixture
- packaging, installer와 Web Pester
- touched project line/branch coverage 0.0%p ratchet
- analyzer warning 0와 compiled architecture guard
- `git diff --check`
- final state/schema/control-plane guard

## 15. 실패와 복구

| 조건 | 처리 |
|---|---|
| HEAD 또는 dependency drift | 코드 변경 전 `stale`로 중단하고 rebase/replan 요청 |
| focused/Full/Release 실패 | 현재 card 유지, 다음 card 진행 금지 |
| 위험 경계 발견 | diff를 보존하고 `handoff_required`로 Sol 인계 |
| Luna unavailable/rate limit | `blocked/model_unavailable`, 자동 모델 대체 금지 |
| public signing/publication 범위 요청 | `blocked/blocked_out_of_scope`, 별도 program 요구 |
| 승인 미획득 | `approval_required`, mutation 실행 금지 |
| 설치 또는 운영 실패 | rollback, 실패 evidence 보존, promotion 금지 |
| 동일 `failure_fingerprint` 원인 3회 실패 | `blocked`, reproduction과 가설·로그 기록 |
| observation 중 payload drift | observation restart |
| state/schema 불일치 | state 변경과 완료 주장 거부 |

자동 fallback, request replay, uncertain mutation 재실행과 다른 server/backend 자동 전환은 금지한다.
Rollback은 검증된 직전 package, transactional rollback 또는 revert candidate만 사용한다.

## 16. 100% 완료 판정

프로그램은 다음 조건이 모두 참일 때만 완료한다.

```text
all required cards have execution_status=completed and verification_result=pass
AND every required L/Release card has review_status=pass
AND Wave 0 through Wave 7 completion conditions PASS
AND every required code Wave except inherited Wave 5B is code_complete
AND inherited Wave 5B is closed-not-adopted
AND Wave 6 is code_complete + promotion_complete
AND http_transport_rollout is aspnet_only
AND legacy HttpListener production reachability is zero
AND full test suite has skip zero
AND the versioned C# quality baseline records source/SDK/collector and touched-project line and branch coverage regression is 0.0 percentage points
AND package/current-card/required actual-VM/rollback evidence PASS
AND OBS-LIFETIME-LEGACY-TRACKED, OBS-ASPNET-DEFAULT and OBS-ASPNET-ONLY each have state=passed
AND each required observation current attempt spans at least seven full days with p0_p1_regression_count=0
AND no restarted, failed or historical attempt contributes duration to the current attempt
AND the final current evidence, installed current-card, rollback evidence and OBS-ASPNET-ONLY identify the same version/MSI/payload/provenance 4-tuple
AND FA-001 through FA-028 and DOD-001 through DOD-012 all PASS
AND Git, documentation and evidence indexes agree
```

이 완료는 internal/private-network product completion이다. 완료 후에도 다음을 유지한다.

- Web Console과 PCVCLI만 active
- TypeScript Web Console 유지
- C++ transition 없음
- public trusted signing `false`
- external stable publication `false`

## 17. Predecessor 승계

구 계획의 미완료 checkbox를 새 계획에서 완료로 위조하지 않는다.

1. Activation commit에서만 구 계획 상단에 successor path와 historical 보존 이유를 담은
   `Superseded note`를 추가한다.
2. 완료된 Wave 0/1/2와 Wave 5B는 successor의 inherited completed scope에 evidence link로 기록한다.
3. 실제 남은 작업만 task card로 만든다.
4. `docs/DEVELOPER_INDEX.md`의 현재 실행 계획 링크를 successor로 바꾼다.
5. `docs/ga-ready/EVIDENCE_INDEX.md`는 L/Release evidence가 실제 추가될 때만 갱신한다.
6. Activation-time operational snapshot과 설계 시점 installed non-promotion `0.42.68` 후보를
   혼동하지 않는다.

## 18. Rollout

1. Control-plane schema와 guard를 RED/GREEN으로 구현한다.
2. Materialization LC card series에서 successor, cards와 inherited state를 원자 단위로 추가한다.
3. 마지막 materialization anchor commit의 post-commit guard와 required CI를 PASS한다.
4. 별도 activation commit으로 current pointer와 predecessor note를 전환하고 latest-base
   merge-group CI와 exact post-merge guard PASS 뒤 effective current로 판정한다.
5. P0 rebaseline card를 실행해 current test/coverage/manifest를 갱신한다.
6. P1 Wave 5A critical path부터 task card execution을 시작한다.
7. 각 observation window에서 P2와 허용된 spike를 관찰과 병행하되 card는 직렬 실행한다.
8. P3~P6 transport rollout을 package별로 분리한다.
9. P7에서 100% formula를 자동 판정한다.

Activation 전 control plane에 문제가 있으면 candidate materialization을 고치고 predecessor를
current로 유지한다. Activation 후에는 Superseded predecessor를 다시 current로 만들지 않는다.
Successor를 `control_recovery_blocked`로 전환하고 마지막 검증 state snapshot을 ledger-event로
복원한 뒤 LC repair card로 복구한다. 이미 완료된 code/evidence result commit은 폐기하지 않는다.
Program deactivation이 필요하면 별도 design과 사용자 명시 승인을 요구한다.

## 19. 검토한 대안

### 단일 거대 successor Markdown

파일 수는 적지만 기존 계획과 같은 크기·상태 drift 문제가 반복된다. 채택하지 않는다.

### Wave별 독립 계획과 얇은 roadmap

경계는 명확하지만 문서 수와 전체 상태 집계가 늘고, 작업 재개를 위해 여러 계획을 다시 읽어야 한다.
채택하지 않는다.

### 얇은 controller + task cards + state ledger

안정된 결정, 실행 계약과 현재 상태를 분리해 모델 교체와 context 압축 후에도 deterministic하게
재개할 수 있다. 이 설계로 채택한다.

## 20. 설계 구현 수용 기준

- successor가 current execution controller로 연결된다.
- predecessor가 historical record로 보존된다.
- 실제 남은 작업에 대응하는 atomic task card가 존재한다.
- state와 schema가 모든 card와 dependency를 표현한다.
- Luna/Sol routing과 자동 L escalation이 guard로 강제된다.
- 관리자 승인과 Git/PR 승인이 분리된다.
- observation payload drift와 기간 재시작이 기계 판정된다.
- control-plane Pester와 documentation guard가 PASS한다.
- host mutation은 수행하지 않는다.
- public signing 또는 external publication claim은 변경하지 않는다.

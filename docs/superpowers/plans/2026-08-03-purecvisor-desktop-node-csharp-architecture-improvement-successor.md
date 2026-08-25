# PureCVisor Desktop Node C# Architecture Completion Successor Plan

> **For agentic workers:** 이 문서가 처음 commit되는 시점에는 검토용 비활성 successor다. 이후
> lifecycle은 이 정적 문구가 아니라 state의 program/activation projection이 소유한다. 사용자의 별도 materialization
> 승인 전에는 `LC-001`을 시작하지 않는다. 승인 후에도 한 번에 한 카드만 실행하고, 각 카드가
> 지정한 모델·검증 lane·허용 경로·commit trailer를 지킨다. 제품 코드 실행은 materialization과
> activation이 유효하게 끝난 뒤에만 허용한다.

- 날짜: `2026-08-03`
- 문서 상태: `controller-definition`; mutable program/effective-current 상태는 state-owned
- program status pointer: `docs/superpowers/plans/luna-completion/execution-state.json#/program_status`
- current task pointer: `docs/superpowers/plans/luna-completion/execution-state.json#/current_task_id`
- plan ID: `purecvisor-desktop-node-luna-successor-20260803`
- plan commit locator: `Plan-ID: purecvisor-desktop-node-luna-successor-20260803`
- active plan revision: `weekly-service-delivery-v3`
- plan revision locator: `Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3`
- revision approval locator: `User-Approval: weekly-service-delivery-v3-luna-max-20260803`
- stable design: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-luna-completion-control-design.md`
- stable design amendment locators: `Design-Amendment: luna-control-bootstrap-v1`,
  `Design-Amendment: luna-max-routing-v1`
- stable design amendment approvals: `User-Approval: luna-control-bootstrap-ultra-20260803`,
  `User-Approval: luna-max-availability-20260803`
- available Luna selector / canonical execution pair: `Luna Max (gpt-5/6-luna, max)` /
  `gpt-5.6-luna` + `max`
- current predecessor: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- authoring base HEAD: `15a6119f6d388b84e455a79a2664a78276774b35`
- authoring branch: `codex/aspnet-core-wave6-transition`
- v3 revision authoring base HEAD: `c973cfe14aae6027ab1c5f934ec0228e8b291b71`
- v3 revision authoring branch: `codex/luna-max-successor-v3`
- designated current branch: `main`
- distribution boundary: `internal-private-network-only`
- public trusted signing: `false`
- external stable publication: `false`

`authoring base HEAD`는 이 계획을 작성한 snapshot이지 future card의 시작 HEAD가 아니다. 각 카드의
`start_head`는 `ready` 전이 시점의 exact 40자 commit으로 상태 원장에 기록한다.
`v3 revision authoring base HEAD`도 v3/derived merge 전 revision 작성 snapshot이며 materialization 승인
base가 아니다. Materialization base는 두 merge와 exact post-merge CI 뒤 받는 새 승인만 소유한다.

**Goal:** 이미 완료된 Wave 0/1/2와 기각 종결된 Wave 5B를 다시 수행하지 않고, 남은 Wave 5A,
Wave 3/4/7, ASP.NET Core 전환, 세 단계 package 승격과 관찰을 원자 카드로 실행해 내부 사설망용
제품 완료 조건을 100% 닫는다.

**Architecture:** 승인된 stable design을 규범 owner로 유지하고, 이 문서는 P0~P7 의존성과 required
card/observation 집합과 주간 delivery projection 정책을 소유한다. 각 원자 작업의 파일·테스트·rollback은
materialization 때 생성할 task card가 소유하고, mutable 작업·주간 상태는 JSON ledger가 소유한다.
`DesktopNodeApiRequestProcessor`와 application owner는 유지한 채 transport만 `HttpListener`에서
ASP.NET Core로 단계적으로 교체한다.

**Tech Stack:** C# / .NET 10 Windows, ASP.NET Core 10, TypeScript Web Console, xUnit,
`System.Text.Json`, `System.Management`, Windows Service, PowerShell 7, Pester 5.7.1, WiX/MSI,
GitHub Actions.

---

## 1. 현재 판정과 권위

이 문서가 commit돼도 즉시 current plan이 되지 않는다. Effective activation 전까지 current execution
plan은 predecessor이며 다음 파일을 이번 계획 작성 commit에서 수정하지 않는다.

- `docs/DEVELOPER_INDEX.md`
- predecessor 상단 상태와 본문
- `docs/ga-ready/current-evidence.json`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- package, service, port, firewall, certificate와 Hyper-V 상태

작성 시점 operational snapshot은 다음과 같다.

| 필드 | 관찰값 | 권위 |
|---|---|---|
| version | `0.42.65-admin-smoke` | `docs/ga-ready/current-evidence.json` |
| operational MSI SHA-256 | `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30` | 동일 |
| payload SHA-256 | `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28` | 동일 |
| provenance commit | `4855947fe0199cedc978e8b40ffb45e96ced6876` | 동일 |
| installed non-promoted candidate | `0.42.68-admin-smoke` | `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md` |
| active surfaces | Web Console, PCVCLI | current evidence |
| TUI | absent | current evidence |
| public claims | `false` / `false` | current evidence |

이 값은 snapshot이다. LC materialization anchor와 activation 직전에 authoritative JSON을 다시 읽고,
version/operational-MSI/payload/provenance와 public claim이 다르면 snapshot을 정직하게 갱신한 새 materialization
anchor가 필요하다. `0.42.68` 설치 사실은 operational promotion이 아니다.

### 1.1 작성 시점 activation blocker

다음은 v3 revision 시점의 관찰값이며 LC activation audit에서 다시 확인한다.

- PR #176은 merge commit `1c4655336d1972cc0795507e3880cefaa1348385`로 병합됐고 base `Plan-ID`와
  당시 v1 `Plan-Revision` locator ancestry 및 exact merged-main CI run `30813789833` /
  `30813789840`이 PASS했다.
- Bootstrap/Luna-ultra amendment는 original commit
  `49fb197ae5ae85bb30f1b7821ec5694274f9663c`를 보존한 merge commit
  `d93051fbcfa34e83f46e752624306cd94c2ff3c9`로 `main`에 들어갔으며 exact merged-main CI run
  `30814689579` / `30814689665`가 PASS했다.
- v2 weekly revision은 original commit `91750336328b0b213a8c843305882a592d3b67c0`를 보존한
  PR #178 merge commit `28290aec51dd04f95c9ec48a15cd1bc8fc3e139e`로 들어갔고 exact merged-main
  CI run `30815667557` / `30815667608`이 PASS했다. v2 derived projection은 PR #179 merge commit
  `dbac0ae5abd86d2fbddafeb51ed8d6991a58d5b1`와 run `30816609076` / `30816608570`에서 PASS했다.
- Luna Max availability amendment는 original commit `6fb8a3e946391b70b2ca968189e7e7fa824f6863`을
  보존한 PR #180 merge commit `c973cfe14aae6027ab1c5f934ec0228e8b291b71`로 들어갔으며 exact
  merged-main CI run `30820050609` / `30820050083`이 PASS했다.
- `.github/workflows/development-gates.yml`과 `.github/workflows/public-boundary.yml`에
  `merge_group` trigger가 없다.
- `gh pr checks 176 --required`는 required check가 없다고 보고했다.
- Private repository의 현재 GitHub plan에서 branch protection/ruleset API는 HTTP 403이며 강제 여부를
  증명할 수 없다.

따라서 materialization 준비는 가능하지만 stable design이 요구한 latest-base/merge-group/required
checks가 증명되기 전 activation은 `blocked`다. 일반 PR-head green CI는 이를 대신하지 않는다.
GitHub 기능을 확보할 수 없다면 자동 완화하지 않고 stable design amendment와 사용자 명시 승인을
먼저 받는다.

### 1.2 WSD v3 revision 판정

- v2에서 `WSD-B001`은 §5.2를 canonical owner로 유지하고 §19 SW-01 initial-state seed를 동일한
  `LC-027`, `LC-028`, `LT-001`~`LT-005` 집합과 lifetime goal로 정렬해 resolved됐다. v3는 17개
  weekly row, dependency와 acceptance를 변경하지 않고 model projection만 정렬한다.
- `WSD-B002`의 bootstrap-contract subgate는 bootstrap amendment로 resolved됐다. Control-only
  materialization은 `LC-024` audit까지 pending이고 activation/attestation은 `LC-026` PASS까지 pending이므로
  WSD-B002 전체를 closed로 표시하지 않는다.
- S/M card는 Luna Max amendment에 따라 `gpt-5.6-luna`/`max`, S/Fast와 M/Full을 사용한다. L/Release는
  `gpt-5.6-sol` high 또는 ultra를 유지한다. Luna가 callable하지 않으면 자동 대체 없이
  `blocked/model_unavailable`이다.
- `LC-001` 전 `gpt-5/6-luna` selector가 canonical `gpt-5.6-luna` execution ID로 resolve됨을 durable
  evidence로 확인한다. 확인 불가 또는 불일치는 `blocked/model_identifier_unresolved`다.
- `User-Approval: luna-control-materialization-dbac0ae5abd8-20260803`은 v2/ultra exact main에 결합됐으나
  LC transaction에 사용되지 않은 stale approval이다. 이 v3 revision과 Max derived projection이 latest
  `main` ancestor가 된 뒤 exact fresh-main materialization 승인을 별도로 다시 받아야 `LC-001`을 시작할 수 있다.

## 2. 범위와 불변조건

### 포함

- Wave 5A async lifetime, cancellation, admission, task supervision과 service health
- Wave 3 Host Ops callback-free family owner
- Wave 4 Hyper-V canonical registry, adapter/domain owner와 fake 가능한 WMI seam
- Wave 7 evidence reader, historical scaffold, analyzer, architecture와 coverage ratchet
- ADR-0014, 선택 server, exclusive transport seam과 ASP.NET Core API/static/noVNC/service parity
- `legacy_default -> aspnet_opt_in -> aspnet_default_legacy_retained -> aspnet_only`
- internal package, installed current-card, 필요한 actual-VM, lifecycle rollback과 세 번의 7일 관찰
- `FA-001`~`FA-028`, `DOD-001`~`DOD-012`의 기계 판정

### 제외

- C++ 전환과 Linux/KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- TypeScript를 Razor, MVC, Blazor 또는 C# browser client로 교체
- ASP.NET Identity, Entity Framework, IIS/IIS Express
- worker 병렬 mutation과 uncertain request의 transport/backend 자동 replay
- public trusted signing, Winget, 외부 stable publication과 일반 사용자 public release
- 명시 승인 없는 install, service, HTTP/TLS, firewall, trust-store 또는 Hyper-V mutation

### 제품 불변조건

- Web Console과 PCVCLI만 active operator surface이며 TUI는 absent다.
- TypeScript source, npm build와 browser runtime을 유지한다.
- activation-time baseline이 확인한 API route, Hyper-V operation과 Host Ops action 계약을 승인 없는
  behavior change로 바꾸지 않는다. 설계 snapshot은 각각 55/34/22지만 LC-027이 실제 HEAD를 재측정한다.
- 같은 process와 product port에서 legacy와 ASP.NET Core transport를 동시에 실행하지 않는다.
- mutation queue consumer는 항상 하나다.
- durable commit된 job을 client disconnect로 취소하지 않는다.
- public claim 두 값은 계속 `false`다.

## 3. 제어 평면과 bootstrap

```text
stable design
  -> this inactive successor
     -> LC control cards, schema, validator and state
        -> materialization-only PR and exact merged anchor
           -> activation-only PR
              -> latest-base + merge-group + exact post-merge attestation
                 -> P0 rebaseline
                    -> one globally serialized product/ops card at a time
```

### 3.1 한 번만 허용하는 bootstrap 규칙

Ledger와 card 파일은 아직 없지만 bootstrap 계약 자체는 PR #177의 stable-design amendment로
승인·병합됐다. 이 successor는 stable design의 규범 owner 권한을 대신하지 않고 다음 규칙을 투영한다.
이 v3 revision이 `main` ancestor가 아니면 `LC-001`은 `blocked/active-revision-stale`, exact fresh-main
materialization 승인이 없으면 `blocked/materialization-approval-missing`이다.

1. 유일한 base `Plan-ID`, active v3 `Plan-Revision`, revision approval, bootstrap/Max amendment와 각
   amendment approval locator를 Git graph에서 찾는다. Revision은 base의 descendant이고 모두 승인 시
   latest `main`의 ancestor여야 한다. Squash로 어느 locator라도 소실됐으면 materialization을 시작하지 않는다.
2. Stable design 형식의 별도 materialization approval locator가 승인 대상 exact 40자 fresh-main SHA에
   결합돼야 한다. Fresh materialization branch의 변경 전 `git rev-parse HEAD`, approved base와
   `LC-001.start_head`를 동일하게 사용한다. Plan commit hash를 start HEAD로 복사하지 않는다.
3. Runtime selector 관찰에서 `selected_model_label="Luna Max (gpt-5/6-luna, max)"`,
   `selector_model_alias="gpt-5/6-luna"`, `resolved_execution_model="gpt-5.6-luna"`와
   `model_selection_resolution_evidence_ref`를 먼저 고정한다. 확인하지 못하거나 ID가 다르면 파일, state와
   result commit을 만들지 않고 `blocked/model_identifier_unresolved`, callable하지 않으면
   `blocked/model_unavailable`로 중단한다.
4. `LC-001` result commit은 자신의 card와 나머지 LC control-card 정의, schema, initial state와
   schema test를 함께 만들 수 있다. Initial state는 plan authority/weekly forecast를 포함하고 fresh
   materialization base를 `start_head`로 두며
   `LC-001` result는 locator로 완료 표시하며 나머지 LC task를 pending으로 seed한다. 제품 source,
   GA evidence와 current pointer는 변경할 수 없다.
5. result commit에는 `Card-ID: LC-001`을 둔다. 자기 commit hash는 같은 commit 내용에 쓰지 않는다.
6. 이후 모든 card는 실행 전에 card 파일과 state record가 존재해야 하며 이 예외를 재사용할 수 없다.

Initial state의 `program_status`는 `design_approved`다. 모든 required future card가 materialize되고
LC-023 anchor가 PASS한 result에서만 `materialized_inactive`로 전환한다.

Merged amendment commits `49fb197ae5ae85bb30f1b7821ec5694274f9663c`과
`6fb8a3e946391b70b2ca968189e7e7fa824f6863`은 각각 `Design-Amendment: luna-control-bootstrap-v1`,
`Design-Amendment: luna-max-routing-v1` locator를 가지며 제품/GA evidence/current pointer를 바꾸지 않았다.
Validator는 두 amendment, revision과 각각의 approval reference 및 별도 materialization approval을
구분하고 bootstrap을 암묵적으로 허용하지 않는다. `bootstrap`, `materialized_inactive`,
`active` 세 validation phase를 명시적으로 구분하며 materialization anchor부터 required
card/path/dependency 집합 전체를 강제한다. LC-001~LC-023은 stable design의 control-only forbidden-path와
host-mutation 금지를 모두 적용한다.

### 3.2 task-card metadata 형식

외부 YAML module과 자유형 regex parser를 도입하지 않는다. Card front matter는 `---` 사이의 top-level
`key: JSON-literal` subset만 허용한다.

- 문자열은 JSON처럼 큰따옴표로 감싼다.
- boolean은 `true|false`, 배열은 JSON 배열을 사용한다.
- nested map, anchor, alias, tag, multiline scalar와 duplicate key를 금지한다.
- 필수 key는 stable design의 card contract와 동일하다.

예시:

```yaml
---
card_id: "LT-001"
wave: "wave5a"
title: "processor async timeout and late-commit guard"
change_tier: "L"
execution_model: "gpt-5.6-sol"
reasoning: "high"
verification_lane: "Release"
dependencies: ["LC-028"]
start_head_ref: "execution-state.json#/tasks/LT-001/start_head"
allowed_paths: ["src/DesktopNode.Api", "src/DesktopNode.Api.Tests"]
forbidden_paths: ["docs/ga-ready/current-evidence.json"]
invariants: ["single mutation worker", "no late commit after timeout"]
approval_checkpoints: []
behavior_change: true
package_candidate: false
host_mutation_required: false
hyperv_mutation_required: false
---
```

## 4. 실행 프로토콜

### 4.1 모델과 검증 lane

| 등급 | 모델 | reasoning | 최소 lane |
|---|---|---|---|
| S | `gpt-5.6-luna` | `max` | Fast |
| M | `gpt-5.6-luna` | `max` | Full |
| L | `gpt-5.6-sol` | `high` 또는 `ultra` | Release |

Validator는 Luna의 `medium|high|ultra`와 Sol의 `max`를
`PCV_LUNA_PLAN_MODEL_REASONING_INVALID`로 차단한다.

Auth/RBAC/JWT/TLS, lifetime/cancellation/persistence, service/port/package/install, WMI/Hyper-V mutation,
operational current evidence에 닿으면 L/Sol/Release로 자동 승격한다. Luna가 이 경계를 발견하면 diff와
test 결과를 보존한 채 `handoff_required`로 전환한다. 자동 모델 대체와 위험 등급 하향은 금지한다.

### 4.2 직렬 실행과 observation 병행

- 전체 program에서 `in_progress` card와 active lease는 최대 하나다.
- observation의 `state=running`은 task `in_progress`가 아니므로 P2 card와 시간상 병행할 수 있다.
- observation start card는 tuple/start time을 기록한 뒤 즉시 `completed/pass`로 닫는다.
- 중간 sample과 restart는 `execution-state.json`만 바꾸는 `Ledger-Event` commit이다.
- observation close card가 7일과 회귀 0건을 판정한다.
- P2 source/doc commit만으로 설치된 payload hash가 바뀌지 않으면 관찰을 유지한다. 새 payload를
  설치하면 기존 attempt를 history에 보존하고 처음부터 다시 시작한다.

`current_task_id`는 state의 projection이며 다음 결정 순서를 사용한다.

1. Active lease 또는 `in_progress|handoff_required|approval_required|blocked` card가 있으면 그대로 둔다.
2. Required observation close가 elapsed 조건을 충족하면 새 구조 작업보다 해당 close card를 우선한다.
3. 그렇지 않으면 dependency가 모두 completed/pass이고 eligibility predicate가 true인 pending card만
   후보로 삼는다.
4. Critical path는 `LC -> LT/OP-LT -> LT-012 -> AC/OP-AC -> AC-501/OP-5xx -> AC-6xx/OP-6xx -> FC`
   순이다.
5. `OBS-LIFETIME-LEGACY-TRACKED`가 running이고 close가 아직 불가능할 때만 P2를
   `HO -> HV code/대응 OP-HV -> QG -> AC-301` 순서로 고른다.
6. Critical path의 다음 card가 `OP-004` 때문에 eligible하지 않으면 `HV-101..108`, `HV-110..113`을
   canonical dependency 순서로 완료한 뒤 eligible해진 `OP-004`를 즉시 선택하고 critical path를 재개한다.
7. 그 밖에 critical path의 다음 card가 incomplete P2/`AC-301` dependency 때문에 eligible하지 않을 때만
   위 P2 순서에서 가장 앞선 eligible card를 고른다. 두 fallback 모두 막힌 critical path를 해소할 뿐 weekly
   pool이 우선순위를 override하는 규칙이 아니다.
8. 같은 group에서는 numeric card ID 오름차순을 tie-breaker로 사용한다.
9. 선택 결과와 제외된 더 높은 우선순위 card의 eligibility 사유를 ledger event에 기록한다.

Blocked card를 건너뛰어 다른 product card를 임의 실행하지 않는다. Observation이 running인 사실만으로
lease나 current task를 점유하지 않는다.

### 4.3 commit과 state

- result commit: 유일한 `Card-ID: <ID>` trailer
- ledger-only commit: 유일한 `Ledger-Event: <event-id>` trailer, state 외 변경 0건
- activation commit: `Card-ID: LC-025`와
  `Program-Activation: purecvisor-desktop-node-luna-completion-20260803`
- plan commit: `Plan-ID: purecvisor-desktop-node-luna-successor-20260803`
- approved weekly projection revision: 유일한
  `Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3`

`start_head`와 `ledger_base_head`는 state를 포함한 commit 자신의 hash가 아니다. Post-commit guard가
locator를 exact commit으로 해석하고 ancestor와 중간 ledger-only path를 검사한다.

### 4.4 승인 category

| Category | 승인 경계 |
|---|---|
| `package_service/build` | exact HEAD/version/recipe/signing/output를 고정하고 host mutation 0건으로 build |
| `package_service/install` | 생성된 exact artifact hash를 확인한 뒤 install/service 변경 |
| `http_binding_tls` | URL ACL, SSL binding, certificate와 product port |
| `hyperv_actual_vm` | actual VM 및 Hyper-V mutation |
| `lifecycle_rollback` | update, rollback, repair, uninstall, clean-host와 package-pair |

Build 승인과 install 승인은 합치지 않는다. 여러 category가 필요한 card는 각 record가 승인돼야 해당
mutation을 실행한다. Git push/PR/merge 승인은 host mutation 승인이 아니다.

## 5. 전체 의존성

```text
P0 materialize -> activate -> rebaseline
  -> P1 LT-001..LT-011 -> tracked-default operational chain
     -> OBS-LIFETIME-LEGACY-TRACKED running
        -> P2 HO / HV / QG and non-product AC-301, globally serialized
     -> observation close -> LT-012 legacy lifetime removal
        -> P3 exclusive ASP.NET seam
           -> P4 exact parity and opt-in package
              -> P5 ASP.NET default + OBS-ASPNET-DEFAULT
                 -> P6 HttpListener removal + OBS-ASPNET-ONLY
                    -> P7 FA/DOD/final tuple closure
```

P2와 P3/P4의 non-product 준비는 관찰 기간을 활용하지만 모든 required P2 card는 P7 전에 닫혀야 한다.

### 5.1 Canonical required-card DAG

아래 JSON이 required card ID와 direct dependency의 단일 owner다. 다른 표의 `의존성`/`선행` cell과
range·설명은 transitive 또는 semantic prerequisite를 줄인 비규범 요약일 뿐 direct-edge projection이
아니며 state/card dependency를 생성하지 않는다. `LC-022` dependency conformance는 generated card/state와
이 JSON만 비교하고 사람이 읽는 요약 cell의 문자열 동등성을 요구하지 않는다. Validator는 dependency object key set을
정렬한 값이 state task set과 정확히 같고 count가 `158`인지 검사한다. Card ID pattern은
`^(?:LC|LT|HO|HV|QG|AC|FC|OP|OP-(?:LT|HV|AC))-[0-9]{3}$`다.

```json
{
  "contract": "luna-successor-required-dag-v1",
  "required_card_count": 158,
  "dependencies": {
    "LC-001": [],
    "LC-002": ["LC-001"],
    "LC-003": ["LC-002"],
    "LC-004": ["LC-003"],
    "LC-005": ["LC-004"],
    "LC-006": ["LC-005"],
    "LC-007": ["LC-006"],
    "LC-008": ["LC-007"],
    "LC-009": ["LC-008"],
    "LC-010": ["LC-009"],
    "LC-011": ["LC-010"],
    "LC-012": ["LC-011"],
    "LC-013": ["LC-012"],
    "LC-014": ["LC-013"],
    "LC-015": ["LC-014"],
    "LC-016": ["LC-015"],
    "LC-017": ["LC-016"],
    "LC-018": ["LC-017"],
    "LC-019": ["LC-018"],
    "LC-020": ["LC-019"],
    "LC-021": ["LC-020"],
    "LC-022": ["LC-021"],
    "LC-023": ["LC-022"],
    "LC-024": ["LC-023"],
    "LC-025": ["LC-024"],
    "LC-026": ["LC-025"],
    "LC-027": ["LC-026"],
    "LC-028": ["LC-027"],

    "LT-001": ["LC-028"],
    "LT-002": ["LT-001"],
    "LT-003": ["LT-002"],
    "LT-004": ["LT-003"],
    "LT-005": ["LT-003"],
    "LT-006": ["LT-003"],
    "LT-007": ["LT-004", "LT-005", "LT-006"],
    "LT-008": ["LT-004"],
    "LT-009": ["LT-004", "LT-006", "LT-007"],
    "LT-010": ["LT-001", "LT-002", "LT-003", "LT-004", "LT-005", "LT-006", "LT-007", "LT-008", "LT-009"],
    "LT-011": ["LT-010"],

    "OP-001": ["LC-028"],
    "OP-002": ["OP-001"],
    "OP-003": ["OP-001"],
    "OP-004": ["HV-113", "OP-001"],
    "OP-005": ["OP-002"],
    "OP-LT-101": ["LT-011", "OP-001", "OP-003"],
    "OP-LT-102": ["OP-LT-101", "OP-003"],
    "OP-LT-103": ["OP-LT-102"],
    "OP-LT-104": ["OP-LT-103", "OP-005"],
    "OP-LT-105": ["OP-LT-104"],
    "OP-LT-106": ["OP-LT-105"],
    "LT-012": ["OP-LT-106"],

    "HO-100": ["OP-LT-105"],
    "HO-101": ["HO-100"],
    "HO-102": ["HO-101"],
    "HO-103": ["HO-102"],
    "HO-104": ["HO-103"],
    "HO-105": ["HO-104"],
    "HO-106": ["HO-105"],
    "HO-107": ["HO-106"],
    "HO-108": ["HO-107"],
    "HO-109": ["HO-108"],
    "HO-110": ["HO-101", "HO-102", "HO-103", "HO-104", "HO-105", "HO-106", "HO-107", "HO-108", "HO-109"],

    "HV-101": ["OP-LT-105"],
    "HV-102": ["HV-101"],
    "HV-103": ["HV-102"],
    "HV-104": ["HV-103"],
    "HV-105": ["HV-104"],
    "HV-106": ["HV-105"],
    "HV-107": ["HV-106"],
    "HV-108": ["HV-107"],
    "HV-110": ["HV-108"],
    "HV-111": ["HV-110"],
    "HV-112": ["HV-111"],
    "HV-113": ["HV-112"],
    "HV-120": ["HV-113", "OP-004"],
    "OP-HV-120": ["HV-120", "OP-004"],
    "HV-121": ["OP-HV-120"],
    "OP-HV-121": ["HV-121", "OP-004"],
    "HV-122": ["OP-HV-121"],
    "OP-HV-122": ["HV-122", "OP-004"],
    "HV-123": ["OP-HV-122"],
    "OP-HV-123": ["HV-123", "OP-004"],
    "HV-124": ["OP-HV-123"],
    "OP-HV-124": ["HV-124", "OP-004"],
    "HV-125": ["OP-HV-124"],
    "OP-HV-125": ["HV-125", "OP-004"],
    "HV-126": ["OP-HV-125"],
    "OP-HV-126": ["HV-126", "OP-004"],
    "HV-127": ["OP-HV-126"],
    "OP-HV-127": ["HV-127", "OP-004"],
    "HV-128": ["OP-HV-127"],
    "OP-HV-128": ["HV-128", "OP-004"],

    "QG-101": ["OP-LT-105"],
    "QG-102": ["QG-101"],
    "QG-103": ["QG-102"],
    "QG-104": ["QG-103"],
    "QG-105": ["QG-104"],
    "QG-106": ["QG-105"],
    "QG-107": ["QG-106"],
    "QG-108": ["QG-107"],
    "QG-109": ["QG-108"],
    "QG-110": ["QG-109"],
    "QG-111": ["HO-110", "HV-128", "QG-110"],
    "QG-112": ["QG-111"],
    "QG-113": ["QG-110"],
    "QG-114": ["QG-112", "QG-113"],
    "QG-115": ["QG-114"],

    "AC-301": ["OP-LT-105"],
    "AC-302": ["LT-007", "OP-LT-106"],
    "OP-AC-302": ["AC-302", "OP-001"],
    "OP-AC-303": ["OP-AC-302", "OP-003", "OP-005"],
    "AC-303": ["OP-AC-303"],
    "OP-AC-304": ["AC-303", "OP-001"],
    "OP-AC-305": ["OP-AC-304", "OP-003", "OP-005"],
    "AC-307": ["AC-301", "OP-LT-106"],
    "AC-304": ["LT-012", "AC-307", "OP-AC-305"],
    "AC-305": ["AC-304"],
    "AC-306": ["AC-305"],
    "AC-401": ["AC-306"],
    "AC-402": ["AC-401"],
    "AC-403": ["AC-402"],
    "AC-404": ["AC-402"],
    "AC-405": ["AC-402", "OP-AC-303"],
    "AC-406": ["AC-403", "AC-405"],
    "AC-407": ["AC-404", "AC-406"],
    "AC-408": ["AC-407"],
    "AC-409": ["AC-408"],
    "AC-410": ["AC-409", "OP-001"],
    "AC-411": ["AC-410", "OP-003"],
    "OP-AC-411": ["AC-411", "OP-001"],
    "OP-AC-412": ["OP-AC-411"],
    "OP-AC-413": ["OP-AC-412", "OP-003", "OP-005"],

    "AC-501": ["OP-AC-413"],
    "OP-501": ["AC-501", "OP-001"],
    "OP-502": ["OP-501", "OP-003"],
    "OP-503": ["OP-502", "AC-307"],
    "OP-504": ["OP-503", "OP-004"],
    "OP-505": ["OP-504", "OP-003", "OP-005"],
    "OP-506": ["OP-505"],
    "OP-507": ["OP-506"],

    "AC-600": ["OP-507"],
    "AC-601": ["AC-600"],
    "AC-602": ["AC-601"],
    "AC-603": ["AC-602"],
    "OP-601": ["AC-603", "OP-001", "HO-110", "OP-HV-128", "QG-115"],
    "OP-602": ["OP-601", "OP-003"],
    "OP-603": ["OP-602", "AC-307"],
    "OP-604": ["OP-603", "OP-004"],
    "OP-605": ["OP-604", "OP-003", "OP-005"],
    "OP-606": ["OP-605"],
    "OP-607": ["OP-606"],

    "FC-001": ["LT-012", "HO-110", "OP-HV-128", "QG-115", "AC-603"],
    "FC-002": ["FC-001"],
    "FC-003": ["FC-002", "OP-607"],
    "FC-004": ["FC-003"],
    "FC-005": ["FC-004"],
    "FC-006": ["FC-005"],
    "FC-007": ["FC-006"]
  },
  "eligibility_predicates": {
    "LC-001": ["bootstrap amendment locator and user approval exist", "base Plan-ID and active Plan-Revision with revision approval are unique main ancestors", "HEAD equals recorded fresh materialization base"],
    "LC-023": ["predecessor remains current", "base Plan-ID and active Plan-Revision ancestry pass", "materialization PR uses MERGE strategy", "program minimum CI set passes"],
    "LC-024": ["LC-023 locator resolves in final merged main history", "original Card-ID commits remain ancestors"],
    "LC-025": ["activation_base_head equals latest main", "user activation approval exists", "merge queue and minimum required checks are enforceable"],
    "LC-026": ["activation PR is merged with MERGE strategy", "activation merge-group and post-merge CI pass", "operational snapshot and public claims match"],
    "LC-027": ["attestation PR is merged and validator-derived effective current is true"],
    "HO-100": ["OBS-LIFETIME-LEGACY-TRACKED is running or passed"],
    "HV-101": ["OBS-LIFETIME-LEGACY-TRACKED is running or passed"],
    "QG-101": ["OBS-LIFETIME-LEGACY-TRACKED is running or passed"],
    "AC-301": ["OBS-LIFETIME-LEGACY-TRACKED is running or passed"],
    "OP-LT-106": ["current observation attempt spans at least seven full days", "required samples are complete"],
    "OP-507": ["current observation attempt spans at least seven full days", "required samples are complete"],
    "OP-607": ["current observation attempt spans at least seven full days", "required samples are complete"],
    "AC-307": ["ADR-0014 selected server decision is immutable"],
    "AC-408": ["selected-server effective path and command are frozen"],
    "AC-409": ["selected-server effective path and command are frozen"],
    "AC-410": ["selected-server effective path and command are frozen"],
    "AC-411": ["selected-server effective path and command are frozen"],
    "OP-601": ["all required P2 result and operational evidence commits are ancestors of package source HEAD"],
    "FC-005": ["pre-promotion tuple and 37 pre-promotable FA/DOD rows pass", "only DOD-008, DOD-009 and DOD-011 await atomic promotion"],
    "FC-007": ["post-promotion current/index tuple equals the pre-promotion tuple"]
  }
}
```

모든 OP, OP-LT, OP-HV, OP-AC와 observation start/close card는 별도 표기가 없더라도
`L / gpt-5.6-sol / ultra / Release`다. Read-only evidence projection만 수행하는 FC/LC 예외는 해당 표의
명시 등급을 따른다.

### 5.2 1주 단위 서비스 개발 목표 projection

`Weekly Service Delivery Projection`은 canonical DAG를 달력에 투영한 rolling forecast다. 실행 권한,
dependency, `current_task_id`, card 완료 또는 100% 판정을 대신하지 않는다. 주간 목표를 맞추려고
dependency가 닫히지 않은 card를 시작하거나 blocked card를 건너뛸 수 없다.

Control bootstrap/materialization/activation인 `LC-001`~`LC-026`은 기간을 약속할 수 없는 선행 gate다.
`SW-01`은 validator-derived effective current를 만든 `LC-026`이 PASS한 뒤 처음 도래하는
`Asia/Seoul` 월요일 00:00에 시작한다. 각 주는 정확히 7일의 `[starts_at, ends_at_exclusive)` 구간이고
state에는 두 경계를 UTC로 기록한다. `LC-027`/`LC-028` 재기준화부터 service delivery week에 포함한다.
Weekly anchor를 기다리느라 eligible card를 멈추지 않는다. LC-026 PASS와 SW-01 시작 사이에 완료된
card는 pre-anchor result로 기록하고 첫 forecast revision에서 pool에서 제거한다.

각 week record는 최소 다음 field를 가진다.

- `week_id`, `sequence`, `starts_at_utc`, `ends_at_exclusive_utc`
- `goal`, `forecast_card_pool`, exact `planned_card_ids`
- `entry_conditions`, `exit_conditions`, `approval_categories`, `observation_ids`
- `planning_base_head`, `selection_simulation_ref`
- `available_capacity_hours`, card별 `estimated_effort_hours`, `planned_effort_hours`
- `carryover_from`, `carryover_blockers`, `result_refs`, append-only `events`, `derived_status`

`derived_status` label은 `forecast|committed|in_progress|met|partial|blocked|superseded` 중 하나지만 독립
상태 머신이 아니다. Immutable forecast/commitment/blocker/review/superseding ledger event, current time과
canonical card/evidence 상태에서 validator가 결정적으로 파생한다. Future row는 forecast이며 현재 주와
바로 다음 주에만 commitment event를 추가할 수 있다. Commitment 전 available capacity와 card별 독립
effort estimate를 기록하고 planned effort가 capacity를 넘으면 안 된다. `S|M|L`은 검증 위험 등급이지
일정 추정치가 아니므로 effort로 변환하지 않는다.

`planned_card_ids`는 canonical 158-card set의 정확한 subset이고 card 실행 시점에는 원래 dependency와
eligibility가 모두 PASS해야 한다. Commitment의 ordered `planned_card_ids`는 planning base state에서
각 선행 card가 성공한다고 가정해 기존 §4.2 selector를 반복 적용한 deterministic prefix여야 하고 simulation
artifact를 참조한다. Forecast pool은 더 넓을 수 있지만 weekly projection이 selector 순서를 override할
수 없다. Week 종료 때 planned card가 모두 completed/pass이고 exit condition과
필요 review/evidence가 닫힌 review event가 있을 때만 `met`로 파생된다. 미완료는 card 실패로 가장하지
않고 review/blocker event에서 `partial` 또는 `blocked` 사유를 기록한다. 다음 주 forecast revision은
미완료 card를 `carryover_from`으로 명시한 뒤 먼저 계획한다. 완료 card는 재계획하지 않고 과거 row/event는
수정하지 않는다. Carry-over로 같은 card가 여러 historical week에 나타날 때는 이전 row와 locator가
정확히 연결돼야 한다.

아래 17주는 blocker·관찰 restart가 없을 때의 baseline forecast pool이다. 한 row 전체가 자동 weekly
commitment는 아니며 capacity와 current-task selection simulation을 확인한 뒤 exact planned subset으로
축소한다. Carry-over가 생기면 이후 주의 날짜는 유지하되 기존 forecast를 수정하지 않고 superseding
forecast revision event를 append한다. 이 표를 완료 증거로 사용하지 않는다.

| 주차 | 서비스 개발 목표 | Baseline forecast card pool | 주간 exit target | 운영/승인 초점 |
|---|---|---|---|---|
| SW-01 | effective-current 재기준화와 lifetime foundation | `LC-027..028`, `LT-001..005` | HEAD/route/operation/action/quality 재측정, async lifetime/cancel/admission/noVNC foundation PASS | control attestation 유지; host mutation 없음 |
| SW-02 | tracked request lifetime code 완결 | `LT-006..011` | service fault/drain/operator/parity와 tracked default PASS | L/Release review |
| SW-03 | 운영 runner, legacy-tracked 설치본 승격과 관찰 시작 | `OP-001..003`, `OP-005`, `OP-LT-101..105` | exact runner/build/install/actual-VM/lifecycle/current-card PASS 후 `OBS-LIFETIME-LEGACY-TRACKED` running | `package_service/build`, `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-04 | 첫 관찰 coverage와 Host Ops owner 이동 | `HO-100..110` | 관찰 sample cadence 유지, 9 family/22 action callback-free owner와 focused/Full PASS | product payload 설치 금지 |
| SW-05 | 첫 관찰 종료, legacy lifetime 제거와 security seam | `OP-LT-106`, `LT-012`, `AC-301..303`, `AC-307`, `OP-AC-302..305` | 관찰 ≥7일/P0·P1 0, legacy lifetime reachability 0, server 결정과 noVNC/static installed hardening PASS | `package_service/build`, `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-06 | exclusive ASP.NET seam과 핵심 pipeline parity | `AC-304..306`, `AC-401..406` | exclusive selector, API adapter/body/static/noVNC/admission/service lifetime parity PASS | legacy default 유지 |
| SW-07 | 55-route/실제 server parity와 opt-in rollback | `AC-407..411`, `OP-AC-411..413`, `AC-501` | actual server/CLI/publish parity, opt-in install/rollback PASS, ASP.NET default source ready | `package_service/build`, `package_service/install`, `http_binding_tls`, `lifecycle_rollback` |
| SW-08 | ASP.NET default build/install/TLS preflight | `OP-501..503` | default candidate, installed current-card와 selected-server TLS/service restore PASS | `package_service/build`, `package_service/install`, `http_binding_tls` |
| SW-09 | Hyper-V canonical registry와 read seam | `HV-101..108`, `HV-110..113`, `OP-004` | canonical projection, fakeable executor/read providers와 mutation-disabled actual-VM runner PASS | host mutation 없음 |
| SW-10 | ASP.NET default actual-VM/lifecycle와 관찰 시작 | `OP-504..506` | targeted actual-VM과 lifecycle/target restore PASS 후 `OBS-ASPNET-DEFAULT` running | `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-11 | ASP.NET default 안정성 관찰 종료 | `OP-507` | ≥7일, 최소 8 sample, 간격 ≤26시간, P0·P1 0건으로 observation PASS | payload 변경 금지 |
| SW-12 | HttpListener production reachability 제거 | `AC-600..603` | legacy fixture 동결, selector/implementation/package branch reachability 0과 compiled guard PASS | product package는 아직 만들지 않음 |
| SW-13 | Hyper-V WMI mutation seam 실제 검증 | `HV-120..128`, `OP-HV-120..128` | 각 named operation/group의 fake matrix와 actual-VM pre/post/readback/cleanup PASS | card별 `hyperv_actual_vm` |
| SW-14 | Quality/evidence closure | `QG-101..115` | evidence owner, architecture/coverage ratchet와 historical link cleanup PASS | current anchor/index 승격 금지 |
| SW-15 | final package 승격과 ASP.NET-only 관찰 시작 | `OP-601..606` | final build/install/TLS/actual-VM/lifecycle PASS 후 `OBS-ASPNET-ONLY` running | `package_service/build`, `package_service/install`, `http_binding_tls`, `hyperv_actual_vm`, `lifecycle_rollback` |
| SW-16 | final package 안정성 관찰과 품질 closure | `FC-001..002` | observation sample cadence 유지, final quality ratchet과 full Release lane PASS | product payload 변경 금지 |
| SW-17 | final observation, 원자 승격과 완료 attestation | `OP-607`, `FC-003..007` | observation PASS, FA 28/DOD 12 전부 PASS, atomic promotion/post-merge attestation 뒤 program complete | current evidence 승격 승인 |

Range 표기는 같은 prefix의 양 끝을 포함한다. `LC-001..026` pre-gate와 SW-01~17 pool을 확장한 합집합은
canonical 158-card set과 정확히 같고 duplicate/missing/extra가 0이어야 한다. 이 equality는 baseline
coverage만 검증하며 week placement가 dependency나 readiness를 바꾸지는 않는다.
State의 `approval_categories`는 committed card metadata의 exact category union으로 생성한다. 표의
운영/승인 초점은 요약일 뿐 원래 card가 요구하는 category를 제거하거나 대신할 수 없다.
SW-17 종료 시 program이 complete가 아니면 forced PASS나 deadline failure로 처리하지 않고 SW-18 이후
7일 row를 append해 carry-over를 계속한다. 17주는 초기 forecast horizon이지 완료 보증이나 상한이 아니다.

세 observation은 dependency상 순차이며 각각 독립된 7×24시간과 최소 8개 sample을 요구하므로 재시작이
없어도 관찰 시간만 최소 21일이다. 주간 경계가 observation start와 일치하지 않아도 시간을 반올림하거나
carry-forward하지 않는다. Payload drift, sample 누락, 26시간 초과 gap 또는 P0/P1 회귀가 발생하면 해당
attempt를 restart하고 append-only event에서 그 영향을 받은 current/future week의
`partial|blocked|superseded` 파생 근거를 남긴다.

주간 projection은 승인도 승계하지 않는다. `package_service/build`, `package_service/install`,
`http_binding_tls`, `hyperv_actual_vm`, `lifecycle_rollback`은 원래 card의 exact command/hash/host별로 각각
승인받는다. 주요 forecast blocker는 bootstrap/activation/merge-queue 증명, `OP-005` external lifecycle
runner, baseline mismatch, TCP 7777 소유권, clean-host/VM availability와 artifact tuple 불일치다.

## 6. P0 Completion Control 카드

| ID | 등급/모델/lane | 결과 | 의존성 |
|---|---|---|---|
| LC-001 | M/Luna/Full | strict schema, plan authority/weekly projection initial state, valid/invalid fixtures와 LC bootstrap card pack; approved design와 plan/revision locator가 precondition | 없음 |
| LC-002 | M/Luna/Full | deterministic validator loader와 `PCV_LUNA_PLAN_*` 오류 | LC-001 |
| LC-003 | M/Luna/Full | task/state/lease/start-head/3-failure guard와 weekly append-only history/derived consistency guard | LC-002 |
| LC-004 | M/Luna/Full | dependency DAG, ID, tier/model/lane, review routing guard | LC-003 |
| LC-005 | M/Luna/Full | temp Git fixture 기반 Card-ID/Ledger-Event/ancestor guard | LC-004 |
| LC-006 | L/Sol/Release | approval composite transition, promotion evidence, public-boundary guard | LC-005 |
| LC-007 | M/Luna/Full | required observation, restart history, 7일과 4-tuple guard | LC-006 |
| LC-008 | L/Sol/Release | materialization/activation/latest-base/merge/post-merge/current projection guard | LC-007 |
| LC-009 | M/Luna/Full | 두 workflow `merge_group` trigger와 Luna guard wiring | LC-008 |
| LC-010 | M/Luna/Full | authoritative current operational snapshot 재검증과 inherited-state 정규화 | LC-009 |
| LC-011 | M/Luna/Full | inherited Wave 0/1/2, Wave 5B와 FA/DOD ID mapping | LC-010 |
| LC-012 | S/Luna/Fast | LT card pack materialization | LC-011 |
| LC-013 | S/Luna/Fast | tracked-lifetime OP card pack materialization | LC-012 |
| LC-014 | S/Luna/Fast | HO card pack materialization | LC-013 |
| LC-015 | S/Luna/Fast | HV card pack materialization | LC-014 |
| LC-016 | S/Luna/Fast | OP-HV card pack materialization | LC-015 |
| LC-017 | S/Luna/Fast | QG card pack materialization | LC-016 |
| LC-018 | S/Luna/Fast | AC P3 card pack materialization | LC-017 |
| LC-019 | S/Luna/Fast | AC P4 card pack materialization | LC-018 |
| LC-020 | S/Luna/Fast | AC/OP P5-P6 card pack materialization | LC-019 |
| LC-021 | S/Luna/Fast | cross-cutting OP/FC card pack materialization | LC-020 |
| LC-022 | M/Luna/Full | required-set/path/dependency/acceptance와 weekly projection conformance | LC-021 |
| LC-023 | M/Luna/Full | plan revision approval/ancestry, inactive materialization anchor locator, pre-merge guard와 fresh PR CI | LC-022 |
| LC-024 | L/Sol/Release | merged LC-023 exact SHA/CI를 기록하고 latest-main/required-check/merge-queue capability audit | LC-023 |
| LC-025 | L/Sol/Release | 별도 사용자 승인 뒤 activation-only current pointer commit | LC-024 |
| LC-026 | M/Luna/Full | merged LC-025의 exact SHA, merge-group CI, post-merge guard와 effective current attestation | LC-025 |
| LC-027 | M/Luna/Full | effective-current precondition 뒤 HEAD/branch/test와 55-route/34-op/22-action rebaseline | LC-026 |
| LC-028 | M/Luna/Full | SDK/collector/test/coverage fixture rebaseline과 0.0%p ratchet | LC-027 |

`LC-012`~`LC-021`은 이 문서의 catalog ID와 제목을
`docs/superpowers/plans/luna-completion/<CARD-ID>-<kebab-slug>.md`로 materialize하고 같은 commit에서
state task record를 갱신한다. 이 최초 plan commit의 canonical DAG는 immutable owner이므로 pack card가
successor required set이나 dependency를 수정하지 않는다. 변경이 필요하면 사용자 승인과 유일한
`Plan-Revision: <id>` locator를 가진 별도 revision commit 뒤 state의 plan revision pointer를 전환한다.
State가 아직 없는 pre-materialization revision은 locator와 사용자 approval locator만 commit하고
`LC-001`이 base plan/revision locator를 exact ancestor commit으로 해석해 initial state에 seed한다.
State materialization 뒤 revision부터는 검증된 revision commit 후 별도 ledger-only pointer 전환을
요구한다. Plan 본문에 revision commit 자신의 SHA를 자기참조로 쓰지 않는다.
Pack card 자체는 task-doc/state만 변경하며
제품 구현을 선행하지 않는다. 각 future card는 stable design의 11개 본문 항목, exact allowed/forbidden
paths, focused RED/GREEN, effective lane, rollback, evidence와 단일 commit subject를 모두 가져야 한다.
`LC-022`에서는 missing/duplicate/extra card, dangling path/dependency와 cycle이 모두 0이어야 한다.

`LC-001`은 amendment가 허용한 generated bootstrap transaction으로만 실행한다. 순서는 schema와
valid/invalid fixture → plan/revision locator를 해석한 fresh-base initial state와 17개 forecast week seed →
canonical graph에서 LC-001~028 card 문서 생성 → schema-only Pester → 단일 result commit이다. 수작업으로
28개 card나 weekly record를 서로 다르게 복사하지 않고
동일 template/graph source에서 생성한다. 이 control-only 예외는 production-file 1~5개 권고를
확장하지 않으며 LC-002부터 일반 카드 규칙을 적용한다.

### 6.1 P0 구현 파일과 RED/GREEN

`LC-001`~`LC-008`의 공통 production-independent 파일은 다음과 같다.

- Create: `docs/superpowers/plans/luna-completion/execution-state.schema.json`
- Create: `docs/superpowers/plans/luna-completion/execution-state.json`
- Create: `packaging/windows-desktop-node/tools/Test-PcvLunaExecutionPlan.ps1`
- Create: `packaging/windows-desktop-node/tests/PcvLunaExecutionPlan.Tests.ps1`
- Create: `docs/superpowers/plans/luna-completion/LC-001-*.md`부터 모든 LC control card

LC-001의 GREEN은 schema valid/invalid focused Pester만 사용한다. 아래 validator invocation은 LC-002가
tool을 만든 뒤부터 사용한다. 전체 RED fixture는 최소한 schema invalid/valid, duplicate ID, missing dependency, cycle, multiple
`in_progress`, illegal transition, tier/model mismatch, duplicate locator, non-ancestor start, non-ledger
intermediate commit, 승인 전 mutation, evidence 없는 promotion, observation payload drift carry-forward,
7일 미달 PASS, current link mismatch, activation CI 누락, weekly duration이 7일이 아닌 row, week ID/window
중복·overlap, canonical set 밖 card, dependency를 거스른 commitment, capacity 초과, carry-over backlink
누락, current-task selection prefix가 아닌 commitment, historical event mutation, hand-written derived
label mismatch와 invalid superseding revision을 포함한다. GREEN은 다음 명령이 모두
성공해야 한다.

```powershell
$plan = 'docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md'
$state = 'docs/superpowers/plans/luna-completion/execution-state.json'
$schema = 'docs/superpowers/plans/luna-completion/execution-state.schema.json'

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Test-PcvLunaExecutionPlan.ps1 `
  -PlanPath $plan -StatePath $state -SchemaPath $schema

Invoke-Pester `
  -Path packaging/windows-desktop-node/tests/PcvLunaExecutionPlan.Tests.ps1 `
  -PassThru -Output Detailed
```

`LC-009`는 두 workflow에 `merge_group:`을 추가하고 Luna guard를 Development Gates와
`public-boundary-ci-required` 양쪽이 놓치지 않게 연결한다. Git graph guard가 실행되는 checkout은
`fetch-depth: 0` 또는 검증 대상 refs의 명시 fetch를 사용한다. Shallow clone에서 locator/ancestor
검사가 거짓 PASS하지 않는 negative fixture를 둔다. `LC-023`은 merge 전에 다음을 모두 요구한다.

- predecessor가 계속 current인 `materialized_inactive` validation
- base `Plan-ID`, active `Plan-Revision`, 두 locator의 commit-preserving ancestry와 사용자 revision approval
- `Update-PcvCurrentEvidenceDocs.ps1 -Check`
- full control-plane Pester와 Full lane PASS
- unique materialization anchor locator와 fresh PR CI run
- dirty overlap 0건; 아래 사용자 untracked 파일은 수정·삭제·stage하지 않고 그대로 보존

보존 파일:

- `docs/functional-correctness-verification-2026-07-15-results.md`
- `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`

`LC-024`는 fresh main에서 LC-023 locator를 exact final merged commit으로 해석한다. PR head,
merge-group temporary head와 final merged commit은 서로 다른 SHA일 수 있으므로 각각의 CI ref와 함께
분리해 기록하고 같은 PR/merge-queue entry에 속함을 검증한다. Program minimum required set은
`dotnet-tests`, `web-tests`, `packaging-pester`, `installer-web-pester`, `luna-execution-plan`,
`public-boundary-ci-required`이며 추가 required check는 허용한다. Set이 비어 있거나 minimum 중 하나가
없거나 merge-group head에 minimum CI가 없거나 merge queue/ruleset 강제를 증명할 수 없으면
`LC-025`는 시작하지 않는다. `LC-025`가 바꿀 수 있는 범위는 activation pointer를 포함한 state,
`docs/DEVELOPER_INDEX.md`, predecessor의 Superseded note뿐이다. Workflow나 제품 파일을 activation
commit에 섞지 않는다.

`LC-026`은 activation commit 자신의 hash를 본문에 자기참조하지 않는다. Activation locator를 exact
merged SHA로 해석하고 merge-group CI, exact merged commit post-merge guard, operational snapshot과
public claim equality를 별도 attestation-only PR의 control evidence/state에 기록한다. LC-026 result
commit 자체도 commit-preserving 방식으로 designated branch에 merge되고 required CI를 통과한 뒤에만
validator가 effective current를 파생할 수 있다. `effective_current`를 mutable boolean으로 미리 쓰지
않는다.

`LC-027`의 authoritative 재측정 대상은 다음과 같다.

| 계약 | source | focused test |
|---|---|---|
| API routes | `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` | `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` |
| Hyper-V operations | `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs` | `src/DesktopNode.HyperV.Tests/HyperVDomainContractTests.cs` |
| Host Ops families/actions | `src/DesktopNode.Host/Ops/DesktopNodeHostOpsCatalog.cs` | `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs` |

`LC-028`은 stale `611` 또는 문서의 `816`을 복사하지 않고
`packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json`을 실제 capture로
재생성한다. Source snapshot, SDK, collector, test count, skip, project별 line/branch를 함께 고정한다.

### 6.2 LC-011 inherited evidence allowlist

LC-011은 predecessor checkbox를 다시 해석하지 않고 다음 exact tracked refs만 inherited scope 입력으로
사용한다.

- Wave 0: `docs/ga-ready/evidence/csharp-architecture-baseline-2026-08-02.md`,
  `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md`
- Wave 1A: `docs/ga-ready/evidence/csharp-architecture-wave1a-job-runtime-owner-2026-08-02.md`
- Wave 1B: `docs/ga-ready/evidence/csharp-architecture-wave1b-diagnostics-owner-2026-08-02.md`
- Wave 1C: `docs/ga-ready/evidence/csharp-architecture-wave1c-auth-owner-2026-08-02.md`
- Wave 1D: `docs/ga-ready/evidence/csharp-architecture-wave1d-ops-dispatch-owner-2026-08-02.md`
- Wave 2A: `docs/ga-ready/evidence/csharp-architecture-wave2a-job-create-preack-durability-2026-08-02.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2a-physical-job-store-durability-2026-08-02.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`
- Wave 2 installed checkpoint: `docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`
- Wave 2B: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2b-operation-reconciliation-decision.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2b-operation-reconciliation-decision-2026-08-03.md`,
  `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json`
- Wave 2C rename: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-rename-reconciliation.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2c-vm-rename-reconciliation-2026-08-03.md`,
  `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-rename-reconciliation.json`
- Wave 2C delete: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-delete-reconciliation.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2c-vm-delete-reconciliation-2026-08-03.md`,
  `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-delete-reconciliation.json`
- Wave 2C checkpoint: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-checkpoint-create-reconciliation.md`,
  `docs/ga-ready/evidence/csharp-architecture-wave2c-checkpoint-create-reconciliation-2026-08-03.md`,
  `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json`
- Wave 5B: `docs/adr/0012-api-read-concurrency-policy.md`의 serialized 결정과 `closed-not-adopted`

Missing ref, evidence 내부 result가 PASS가 아니거나 current source와 compiled guard가 충돌하면 inherited
complete를 부여하지 않는다. `0.42.68` installed checkpoint는 non-promotion fact로만 연결한다.

## 7. P1 Wave 5A와 tracked lifetime 승격

경로 약어는 표에서만 사용한다: `API=src/DesktopNode.Api`, `HOST=src/DesktopNode.Host`,
`RUNTIME=src/DesktopNode.Runtime`, `PKG=packaging/windows-desktop-node`.

| ID | 등급/실행 | 핵심 파일 | RED와 완료 조건 | 의존성 |
|---|---|---|---|---|
| LT-001 | L/Sol-high/Release | `API/DesktopNodeApiRequestProcessor.cs`, runtime dependencies와 API hardening tests | timeout 뒤 late commit/겹침을 재현하고 end-to-end async task·exception 관찰로 0건 | LC-028 |
| LT-002 | L/Sol-ultra/Release | processor, `RUNTIME/DesktopNodeJobRuntime*.cs`, durability tests | disconnect-before-commit enqueue 0; after durable commit job 1/dispatch 1 | LT-001 |
| LT-003 | L/Sol-high/Release | 신규 `HOST/DesktopNodeRequestLifetimeCoordinator.cs`, Host application과 tests | service/timeout/disconnect token과 serialization owner를 transport 밖 한 곳에 두고 mixed concurrent fixture에서 모든 request-processor 진입 max concurrency=1 | LT-002 |
| LT-004 | L/Sol-high/Release | Host application, `DesktopNodeRequestAdmission.cs`와 tests | active 32/waiting 64 초과를 body read와 per-context task 생성 전에 503/Retry-After | LT-003 |
| LT-005 | L/Sol-ultra/Release | Host application, HTTP/noVNC contract tests | noVNC `WhenAny` 뒤 peer cancel, 두 예외 관찰, half-close/stop handle 0 | LT-003 |
| LT-006 | L/Sol-ultra/Release | Host application, `DesktopNodeWindowsService.cs`, 신규 service tests | listener/worker fault 뒤 `Running but dead` 0 | LT-003 |
| LT-007 | L/Sol-ultra/Release | lifetime coordinator, Host application/service | admission close 뒤 request-body read, native read, diagnostic download, noVNC를 각각 failure-injection하며 drain→worker reconcile→dispose; 경로별 10초 초과 health failure | LT-004..LT-006 |
| LT-008 | L/Sol-high/Release | `web/src/served-app.ts`, CLI application, Web/CLI fixtures | overload 503/code/header 표시와 stable CLI non-zero exit parity | LT-004 |
| LT-009 | L/Sol-high/Release | Host runtime policy/ops summary와 tests | active/waiting/reject/oldest age/heartbeat/current-job/store latency·failure 관측 계약 | LT-004, LT-006, LT-007 |
| LT-010 | M/Luna-max/Full | versioned transport fixture, Host transport tests | legacy/tracked route/JSON/status/header/static/noVNC 차이 0 | LT-001..LT-009 |
| LT-011 | L/Sol-ultra/Release | Host options, product plan/module와 tests | `tracked_async_serialized` default-switch만 수행하고 legacy branch는 유지 | LT-010 |
| LT-012 | L/Sol-ultra/Release | Host options, `Program.cs`, product scripts/tests | 관찰 후 legacy lifetime enum/argv/default/reachability 0 | OP-LT-106 |

P1 전체는 job state, rate limiter와 auth revoke state의 독립 synchronization owner를 유지하고,
cancellation callback을 job-state lock 보유 중 실행하지 않는다. 포화 상태에서도 static/Web-only,
OPTIONS, noVNC와 auth pre-gate의 기존 우선순위를 latest transport manifest와 동일하게 유지하며 mutation
worker max concurrency는 항상 1이다. `tracked_async_serialized`에서는 read/mutation 구분 없이 모든
request-processor entry의 observed max concurrency도 1이며 LT-003 focused RED/GREEN과 LT-010 parity
fixture가 이 값을 직접 검사한다.

### 7.1 Tracked lifetime 운영 카드

| ID | 승인 | 결과 | 의존성 |
|---|---|---|---|
| OP-LT-101 | `package_service/build` | exact HEAD/version/LocalTest+AllowUnsignedDev package와 hashes | LT-011, OP-001 |
| OP-LT-102 | `package_service/install` | installed load/drain/account/current-card와 동일-host 10회 start 및 sustained-load legacy transport performance baseline | OP-LT-101, OP-003 |
| OP-LT-103 | `package_service/install`, `hyperv_actual_vm` | service-path-changing target-backed noVNC, queued mutation/cancel/recovery와 cleanup | OP-LT-102 |
| OP-LT-104 | `lifecycle_rollback`, `package_service/install` | exact install/update/rollback pair를 닫고 target을 다시 설치해 current-card/operational payload hash equality 확인 | OP-LT-103, OP-005 |
| OP-LT-105 | 없음 | `OBS-LIFETIME-LEGACY-TRACKED` tuple/start attempt 기록 후 card 완료 | OP-LT-104 |
| OP-LT-106 | 없음 | elapsed precondition에서 current attempt 7일 이상, P0/P1 0건을 검사해 observation close | OP-LT-105 |

첫 lifecycle baseline은 현재 host 상태를 가장하지 않는다. Exact `0.42.68` artifact를 검증된 legacy
baseline으로 예약하거나 dedicated clean host/checkpoint에서 operational `0.42.65`를 사용한다. 현재
host를 단순 downshift해 PASS evidence를 만들 수 없다. Readiness mismatch는 honest `blocked`다.
`OP-LT-102` 결과는 workload, warm-up, sample 수, latency/error/admission 지표와 측정 host를 포함한
immutable `legacy_performance_baseline_ref`를 state에 기록하며 `OP-502`와 `OP-602`가 같은 계약으로
비교한다.

## 8. P2 Wave 3 Host Ops

`HO-100`은 9 family/22 action characterization을 고정한다. `HO-101`~`HO-109`는 strict
behavior-preserving body move로 계획된 M/Luna/Full 카드다. 보안, SCM, firewall, trust-store,
credential 또는 실제 mutation 의미 차이가 발견되면 해당 diff를 보존하고 L/Sol/Release와 별도
OP card로 재계획한다.

| ID | owner | 선행 카드 | 완료 조건 |
|---|---|---|---|
| HO-100 | `DesktopNodeHostOpsCatalog` characterization | OP-LT-105; observation record running 또는 passed | 9 family/22 action/mutation boundary snapshot |
| HO-101 | config migration | HO-100 | callback 0, plan/result parity, focused owner test |
| HO-102 | job-store migration | HO-101 | migration/rollback diagnostics parity |
| HO-103 | Event Log | HO-102 | redaction/source lifecycle parity |
| HO-104 | firewall | HO-103 | approval/no-fallback/rollback plan parity |
| HO-105 | trust store | HO-104 | store target/thumbprint/rollback parity |
| HO-106 | credential manager | HO-105 | credential redaction/cleanup parity |
| HO-107 | service token | HO-106 | rotate/revoke/result parity |
| HO-108 | data-root lifecycle | HO-107 | containment/backup/rollback parity |
| HO-109 | service lifecycle | HO-108 | SCM path/plan/exit contract parity |
| HO-110 | façade closure | HO-101..HO-109 | `ExecuteNative*ForOps` callback 0, façade는 catalog/dispatch/result만 소유 |

각 owner의 primary source는 `src/DesktopNode.Host/Ops/*.cs`, façade는
`src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`, target test는
`src/DesktopNode.Host.Tests/Ops/*Tests.cs`다. `DesktopNodeHostServiceActionTests.cs`에는 catalog
completeness, dispatch와 public result만 남긴다.

## 9. P2 Wave 4 Hyper-V

### 9.1 Registry, adapter와 read seam

| ID | 등급 | 결과 | 의존성 |
|---|---|---|---|
| HV-101 | M/Luna/Full | 신규 canonical 34-operation registry; observation record는 running 또는 passed | OP-LT-105 |
| HV-102 | M/Luna/Full | Dispatch/WMI/API projection을 canonical registry로 전환 | HV-101 |
| HV-103 | M/Luna/Full | `DesktopNodeHyperVProviderSet` product composition 경로 단일화 | HV-102 |
| HV-104 | M/Luna/Full | read handler owner 이동, adapter dispatch-only | HV-103 |
| HV-105 | M/Luna/Full | VM lifecycle handler owner 이동 | HV-104 |
| HV-106 | M/Luna/Full | checkpoint handler owner 이동 | HV-105 |
| HV-107 | M/Luna/Full | resource mutation handler owner 이동 | HV-106 |
| HV-108 | M/Luna/Full | guest execution handler owner 이동 | HV-107 |
| HV-110 | M/Luna/Full | fake 가능한 WMI query/invoke/job/cancel/dispose executor | HV-108 |
| HV-111 | M/Luna/Full | switch read provider seam | HV-110 |
| HV-112 | M/Luna/Full | VM inventory read provider seam | HV-111 |
| HV-113 | M/Luna/Full | checkpoint read provider seam | HV-112 |

Primary source는 `src/DesktopNode.HyperV/DesktopNodeHyperV*.cs`, tests는
`src/DesktopNode.HyperV.Tests/**`와 API native-candidate contract다. Operation ID, contract key,
telemetry, `PCV_*` error와 single worker 의미를 바꾸지 않는다.

### 9.2 WMI mutation seam과 actual-VM

| Code card | Operation | Ops card | 공통 완료 조건 |
|---|---|---|---|
| HV-120 | VM create | OP-HV-120 | fake failure matrix + approved pre/post/cleanup |
| HV-121 | VM delete | OP-HV-121 | managed guard/error + approved pre/post/cleanup |
| HV-122 | power start/stop/pause/resume | OP-HV-122 | job wait/timeout/cancel + lifecycle readback |
| HV-123 | checkpoint create/restore/delete | OP-HV-123 | mapping/error + checkpoint cleanup |
| HV-124 | `vm.rename` | OP-HV-124 | exact readback와 원래 이름 복원 |
| HV-125 | media eject | OP-HV-125 | controller/media error와 원래 상태 복원 |
| HV-126 | memory/vCPU compute | OP-HV-126 | compute pre/post와 원래 memory/vCPU 복원 |
| HV-127 | virtual disk | OP-HV-127 | shrink guard, disposable-VM expansion과 VM cleanup; shrink rollback 주장 금지 |
| HV-128 | QoS | OP-HV-128 | Kbps→bps unit, pre/post와 원래 QoS 복원 |

각 `HV-12x`는 L/Sol-high/Release, 각 `OP-HV-12x`는 L/Sol-ultra/Release이며
`hyperv_actual_vm` 승인을 별도로 요구한다. Code card는 fake/Release PASS 뒤
`code_ready_operational_pending`으로 닫을 수 있고, 대응 OP card가 PASS해야 `code_complete`다.
OP-HV-120~128은 OP-004가 생성한
`packaging/windows-desktop-node/tools/Invoke-PcvHyperVWmiProviderActualVmParity.ps1`과 대응 Pester의
exact named operation 또는 operation-group selector를 사용하며 PlanOnly summary의 non-Hyper-V mutation
capability가 모두 false여야 한다. Canonical DAG의 direct dependency를 사용하고 각 card는 위 표의 대응
scope만 실행한다. `power`, `checkpoint`, `compute` group은 구성 operation 모두에 개별 pre/post/readback과
cleanup assertion을 남겨야 하며 하나라도 빠지거나 실패하면 group card 전체가 실패한다.
Actual-VM runner는 provider 종류, source commit, pre/post, cleanup, boot/service final state와 fake/actual
suite를 구분한다. 서로 다른 rollback 의미를 가진 rename/media/compute/disk/QoS mutation을 한 카드로 합치지
않는다.

## 10. P2 Wave 7 Quality와 Evidence

| ID | 등급 | 결과 | 의존성 |
|---|---|---|---|
| QG-101 | M/Luna/Full | evidence root/reparse/containment owner; observation record는 running 또는 passed | OP-LT-105 |
| QG-102 | M/Luna/Full | artifact discovery/latest selection owner와 malformed-child degraded contract | QG-101 |
| QG-103 | M/Luna/Full | manual-admin evidence projector | QG-102 |
| QG-104 | M/Luna/Full | public-boundary evidence projector | QG-103 |
| QG-105 | M/Luna/Full | operational/current 4-tuple projector | QG-104 |
| QG-106 | M/Luna/Full | ServiceHostCandidate invariant 이전/처분 | QG-105 |
| QG-107 | M/Luna/Full | ServiceLifecycleAdapter invariant 이전/처분 | QG-106 |
| QG-108 | M/Luna/Full | orphan Service project/reference 제거 | QG-107 |
| QG-109 | M/Luna/Full | ApiHostCandidate를 authoritative route owner로 대체 | QG-108 |
| QG-110 | M/Luna/Full | shared nullable/analyzer/warnings policy | QG-109 |
| QG-111 | M/Luna/Full | machine-readable architecture rules와 compiled guard | HO-110, HV-128, QG-110 |
| QG-112 | M/Luna/Full | production/test LOC와 test-count hotspot ratchet | QG-111 |
| QG-113 | M/Luna/Full | deterministic source/SDK/collector/TRX/Cobertura capture | QG-110 |
| QG-114 | M/Luna/Full | current quality baseline과 line/branch 0.0%p ratchet | QG-112, QG-113 |
| QG-115 | S/Luna/Fast | historical evidence link와 dangling backlink만 정리; current anchor/index 변경 금지 | QG-101..QG-114 |

`BatchEvidenceSummaryReader`의 path policy, discovery와 schema projection을 서로 다른 카드로 유지한다.
Removed test invariant는 `csharp-architecture-test-migration.json`에서 살아 있는 owner test로 연결한다.
Current anchor/index 승격은 FC-005만 소유한다. QG-115에서 그 경계를 발견하면 승격해 계속하지 않고
forbidden-path 위반으로 중단한다.

## 11. P3 ASP.NET Core server 결정과 exclusive seam

| ID | 등급/실행 | 결과 | 의존성 |
|---|---|---|---|
| AC-301 | M/Luna-max/Full | HTTP.sys/Kestrel dynamic-loopback compatibility spike와 ADR-0014에서 product server 하나 선택; lifetime observation 중 실행 가능 | OP-LT-105 |
| AC-302 | L/Sol-ultra/Release | legacy noVNC allowed/invalid/missing Origin 보안 계약 | LT-007, OP-LT-106 |
| OP-AC-302 | L/Sol-ultra/Release, `package_service/build` | noVNC hardening 전용 non-promoted package와 exact hashes | AC-302, OP-LT-106, OP-001 |
| OP-AC-303 | L/Sol-ultra/Release, `package_service/install`, `hyperv_actual_vm`, `lifecycle_rollback` | target-backed installed noVNC fragmented frame/half-close/stop leak, exact rollback와 target restore | OP-AC-302, OP-005 |
| AC-303 | L/Sol-ultra/Release | legacy static web-root/reparse containment hardening | OP-AC-303 |
| OP-AC-304 | L/Sol-ultra/Release, `package_service/build` | static hardening 전용 non-promoted package와 exact hashes | AC-303 |
| OP-AC-305 | L/Sol-ultra/Release, `package_service/install`, `lifecycle_rollback` | installed containment, packaged/served SHA-256 parity, exact rollback와 target restore | OP-AC-304, OP-005 |
| AC-307 | L/Sol-high/Release | ADR-selected server의 HTTP/TLS lifecycle tooling: HTTP.sys existing runner 검증 또는 Kestrel private-key ACL runner RED/GREEN | AC-301, OP-LT-106 |
| AC-304 | L/Sol-ultra/Release | exclusive transport selector/options와 rollout state, legacy default | LT-012, AC-307, OP-AC-305 |
| AC-305 | L/Sol-high/Release | shared application composition과 processor/store/auth/rate-limit/single-worker instance 각 1개 | AC-304 |
| AC-306 | L/Sol-ultra/Release | 최초 product-selectable ASP.NET Core bootstrap, explicit isolated start만 허용 | AC-305 |

`AC-301` spike는 임시 data root, fixture processor와 동적 loopback port만 사용하며 설치 서비스, 실제
job store와 mutation route를 호출하지 않는다. HTTP.sys가 명시 gate를 통과하면 1차 권장안으로
선택하고, 실패한 경우에만 Kestrel의 TLS/private-key/package 차이를 ADR에 기록한다.
두 server는 동일 self-contained publish 설정으로 package size, cold start와 boot-to-listener p95를
측정한다. AC-301만 lifetime observation 중 허용하고 selected-server packaging/TLS를 바꾸는 AC-307은
OP-LT-106 뒤에 실행한다.

`AC-304` selector는 process 시작 전에 transport 하나만 고른다. `--urls`, `ASPNETCORE_URLS`,
`HTTP_PORTS`, `HTTPS_PORTS`, appsettings endpoint와 wildcard `*|+`가 제품 bind 정책을 덮어쓸 수
없다. 실패 요청을 다른 transport로 replay하는 fallback도 없다.

## 12. P4 ASP.NET Core parity와 opt-in readiness

| ID | 등급/실행 | primary owner와 완료 조건 | 의존성 |
|---|---|---|---|
| AC-401 | L/Sol-high/Release | `DesktopNodeApiTransportAdapter`: decode/query/raw body와 exact response passthrough | AC-306 |
| AC-402 | L/Sol-ultra/Release | endpoint branch order, CORS, auth와 admission 우선순위 | AC-401 |
| AC-403 | L/Sol-ultra/Release | known/chunked streaming body cap와 product-owned 413 | AC-402 |
| AC-404 | L/Sol-high/Release | TypeScript build-output static serving, GET-only/no SPA fallback/hash/cache parity | AC-402 |
| AC-405 | L/Sol-ultra/Release | ASP.NET noVNC auth/Origin/subprotocol/frame/close/drain parity | AC-402, OP-AC-303 |
| AC-406 | L/Sol-ultra/Release | admission/cancellation/Windows Service fault와 10초 drain | AC-403, AC-405 |
| AC-407 | L/Sol-high/Release | 55-route와 error/auth/diagnostics exact dual-transport parity | AC-401..AC-406 |
| AC-408 | L/Sol-ultra/Release | TestServer와 선택 server의 실제 dynamic loopback/two-bind/partial cleanup | AC-407 |
| AC-409 | L/Sol-ultra/Release | self-contained publish inventory; IIS/Node/runtime prerequisite 0 | AC-408 |
| AC-410 | L/Sol-ultra/Release | opt-in MSI payload와 이전 SCM `PathName` byte-for-byte rollback plan | AC-409, OP-001 |
| AC-411 | L/Sol-high/Release | OP-003 generic current-card를 selected server/configured-effective transport까지 확장하고 ASP.NET installed smoke Pester RED/GREEN | AC-410, OP-003 |
| OP-AC-411 | `package_service/build` | legacy-default/ASP.NET opt-in exact package candidate | AC-411 |
| OP-AC-412 | `package_service/install`, `http_binding_tls` | isolated opt-in installed Web/API/noVNC/TLS/current-card | OP-AC-411 |
| OP-AC-413 | `lifecycle_rollback`, `package_service/install` | legacy default와 exact SCM/path/package rollback rehearsal 뒤 opt-in target restore/current-card | OP-AC-412, OP-005 |

신규 production owner 후보는 다음 경계를 유지한다.

- `src/DesktopNode.Host/AspNetCore/DesktopNodeAspNetCoreApplication.cs`
- `src/DesktopNode.Host/AspNetCore/DesktopNodeApiTransportAdapter.cs`
- `src/DesktopNode.Host/AspNetCore/DesktopNodeTransportPolicy.cs`
- `src/DesktopNode.Host/AspNetCore/DesktopNodeNoVncEndpoint.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeAspNetCoreApplicationTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHttpTransportParityTests.cs`

정확한 type 이름은 task-card materialization 때 owner 응집도를 높이기 위해 한 번 조정할 수 있지만
route catalog, concurrency policy 또는 application state를 transport별로 복제할 수 없다.

AC-307과 AC-408~411 task card는 materialization 시
`execution-state.json#/decisions/adr0014_selected_server`를 immutable `decision_ref`로 가진다. Card body는
HTTP.sys와 Kestrel 각각의 exact allowed path, focused test, TLS/rollback command와 acceptance branch를
둘 다 정의한다. AC-301 result가 decision과 evidence를 state에 고정한 뒤 ready ledger event가 선택된
한 branch의 `effective_allowed_paths`와 command hash를 freeze한다. Validator는 실제 diff/command가 그
branch만 사용했는지 검사한다. Decision 전에는 이 카드들이 pending이며 generic union path로 실행할
수 없다.

작성 시점에는 다음 도구가 존재하지 않으므로 `AC-411` 이전에 existing command로 주장하지 않는다.

- `packaging/windows-desktop-node/tools/Invoke-PcvAspNetCoreTransportInstalledSmoke.ps1`
- `packaging/windows-desktop-node/tests/PcvAspNetCoreTransportInstalledSmoke.Tests.ps1`

## 13. Cross-cutting 운영 도구

| ID | 등급/실행 | 결과 | 선행 |
|---|---|---|---|
| OP-001 | L/Sol-high/Release | route/full-gate runner가 승인된 exact operational MSI와 expected operational-MSI/payload/provenance를 입력받고 mismatch hard-stop | LC-028 |
| OP-002 | L/Sol-high/Release | lifecycle descriptor가 external Burn/MSIX/installed rollback summary 부재를 honest blocked로 처리 | OP-001 |
| OP-003 | L/Sol-high/Release | generic installed Web/CLI/service/operator-surface current-card와 immutable observation sample runner/Pester | OP-001 |
| OP-004 | L/Sol-high/Release | targeted actual-VM runner/Pester/PlanOnly; service/package/HTTP/OS mutation capability는 false | HV-113, OP-001 |
| OP-005 | L/Sol-ultra/Release | external Burn/MSIX lifecycle runner owner/version/hash/schema/exact command acquisition과 provenance validation | OP-002 |

현재 `Invoke-PcvRouteParityMutationSmoke.ps1`는 내부에서 MSI를 다시 build하므로 build 승인 artifact와
install 승인 artifact의 동일성을 보장하지 못한다. `OP-001`은 최소
`-CandidateMsiPath`, `-ExpectedMsiSha256`, `-ExpectedPayloadSha256`,
`-ExpectedProvenanceCommit` 또는 동등한 candidate artifact-root 계약을 RED/GREEN으로 추가한다.
승인된 artifact를 받은 실행 경로에서는 내부 rebuild를 금지한다.

현재 저장소에는 executable Burn/MSIX lifecycle runner가 없다. `OP-002`는 임의 JSON 합성을 허용하지
않으며, provenance가 있는 외부 승인 runner summary가 없으면 lifecycle card와 descriptor를
`blocked-by-missing-evidence`로 유지한다. Clean-host runner의 실제 parameter는
`-UpdatePackagePath`이며 stale `-TargetUpdatePackagePath`를 사용하지 않는다.
`OP-005`가 승인된 runner의 owner/version/hash/schema/exact command를 확보하지 못하면 관련 lifecycle
카드와 program은 blocked 상태로 남는다. 저장소 내부의 합성 summary나 다른 runner 결과로 대체하지 않는다.

## 14. P5 ASP.NET Core default, legacy retained

| ID | 승인/등급 | 결과 | 의존성 |
|---|---|---|---|
| AC-501 | L/Sol-ultra/Release | source/package default를 ASP.NET Core로 바꾸고 legacy selector 유지 | OP-AC-413 |
| OP-501 | `package_service/build` | default candidate exact 4-tuple | AC-501, OP-002 |
| OP-502 | `package_service/install`, `http_binding_tls` | 승인된 endpoints로 service Running/Automatic, configured=effective ASP.NET, Web/API/CLI/current-card | OP-501, OP-003 |
| OP-503 | `package_service/install`, `http_binding_tls` | AC-307 selected-server runner로 certificate/binding/service restore | OP-502 |
| OP-504 | `hyperv_actual_vm` | service/package/HTTP/OS mutation이 false인 targeted runner로 touched actual-VM, queued mutation/cancel/recovery, cleanup 0 | OP-503 |
| OP-505 | `lifecycle_rollback`, `package_service/install` | legacy→ASP.NET-default clean/update/repair/rollback pair를 닫고 target 재설치/current-card/operational payload equality | OP-504, OP-005 |
| OP-506 | 없음 | `OBS-ASPNET-DEFAULT` tuple/start attempt 기록 후 card 완료 | OP-505 |
| OP-507 | 없음 | elapsed precondition에서 current attempt 7일 이상, P0/P1 0건으로 close | OP-506 |

`OP-502`는 Web `/`와 `/pcv-config.js`, API auth/runtime, PCVCLI host/runtime/ops-summary, account/RBAC,
diagnostics, noVNC와 service start/stop을 검사한다. `OP-503`과 `OP-504`의 승인은 서로 대체하지 않는다.
이 단계 rollout은 `aspnet_default_legacy_retained`이며 아직 `promotion_complete`가 아니다.

OP-502는 동일 host 10회 start/stop, reboot 후 `Running/Automatic`, boot-to-listener p95가 legacy 대비
`max(10%, 1초)` 이내인지와 승인된 sustained-load error/latency/admission budget을 검사한다.
`service-action` exit 0/1/2와 structured error를 확인하고 configured/effective mismatch, unknown transport,
selected-server process 부재를 current-card failure로 처리한다. OP-505 clean-host는 사전 설치된
.NET/ASP.NET runtime 없이 self-contained host가 시작되는지도 검증한다.

`OP-504`/`OP-604`는 runner PlanOnly summary에서 service/package/HTTP/OS mutation이 모두 false인
targeted route만 허용한다. 현재 FullAdmin profile처럼 MSI lifecycle과 OS mutation을 함께 수행하는
runner를 사용하려면 이 카드를 조건부로 넓히지 않는다. Stable design에 필요한 추가 approval category와
exact composite command를 amendment하고 별도 OP card를 materialize할 때까지 `blocked`다.

## 15. P6 HttpListener 제거와 final package

| ID | 승인/등급 | 결과 | 의존성 |
|---|---|---|---|
| AC-600 | M/Luna-max/Full | legacy characterization을 versioned historical fixture로 동결 | OP-507 |
| AC-601 | L/Sol-ultra/Release | legacy selector와 runtime construction reachability 제거 | AC-600 |
| AC-602 | L/Sol-ultra/Release | unreachable `HttpListener` implementation과 packaging branch 제거 | AC-601 |
| AC-603 | L/Sol-high/Release | `aspnet_only`, legacy production reachability 0 compiled guard | AC-602 |
| OP-601 | `package_service/build` | 모든 required P2 complete를 precondition으로 한 legacy-removal final candidate exact 4-tuple | AC-603 |
| OP-602 | `package_service/install`, `http_binding_tls` | 승인된 endpoints로 clean/current-card/service/Web/API/CLI on final package | OP-601, OP-003 |
| OP-603 | `package_service/install`, `http_binding_tls` | AC-307 selected-server runner로 final TLS/service restore/orphan 0 | OP-602 |
| OP-604 | `hyperv_actual_vm` | service/package/HTTP/OS mutation이 false인 targeted runner로 noVNC/jobs/touched operation pre/post/cleanup | OP-603 |
| OP-605 | `lifecycle_rollback`, `package_service/install` | clean/update/repair/uninstall/legacy-retained rollback pair를 닫고 final target 재설치/current-card/operational payload equality | OP-604, OP-005 |
| OP-606 | 없음 | `OBS-ASPNET-ONLY` tuple/start attempt 기록 후 card 완료 | OP-605 |
| OP-607 | 없음 | elapsed precondition에서 7일/P0-P1 0건 close; final promotion candidate tuple 고정 | OP-606 |

Legacy characterization은 historical fixture로 남기되 production project reachability는 0이어야 한다.
제거 후 rollback은 검증된 직전 legacy-retained package 또는 revert candidate만 사용한다. Transport
selector나 request-level 자동 fallback을 다시 추가하지 않는다.

OP-602는 final package에서 동일한 10회 start/stop/reboot, performance budget, service-action exit와
negative current-card cases를 반복한다. OP-605 clean-host도 외부 .NET/ASP.NET runtime prerequisite가
없음을 확인한다.

## 16. P7 Final Closure 카드

| ID | 등급/실행 | 결과 | 의존성 |
|---|---|---|---|
| FC-001 | M/Luna-max/Full | final quality capture, TRX skip/count/migration, line/branch ratchet | LT-012, HO-110, OP-HV-128, QG-115, AC-603 |
| FC-002 | L/Sol-high/Release | full .NET/Web/Pester/installer/Release lane와 architecture guard | FC-001 |
| FC-003 | L/Sol-high/Release | OP-607 뒤 final candidate/observation/rollback/state/public-claim pre-promotion join | FC-002, OP-607 |
| FC-004 | M/Luna-max/Full | exact FA-001~028/DOD-001~012 pre-promotion projection; post-promotion 전용 3개만 pending | FC-003 |
| FC-005 | L/Sol-ultra/Release | 37개 PASS와 3개 expected proof를 확인하고 package/current evidence/index/3개 row를 한 commit으로 원자 승격 | FC-004 |
| FC-006 | L/Sol-high/Release | exact merged promotion commit의 current/index/40-row/4-tuple post-merge attestation | FC-005 |
| FC-007 | L/Sol-high/Release | attestation PASS 뒤 program `complete` | FC-006 |

각 FA/DOD row는 최소 다음 field를 가진다.

```json
{
  "id": "FA-001",
  "status": "pass",
  "owner_card": "FC-004",
  "evidence_refs": [],
  "test_refs": [],
  "owner_result_locator": "Card-ID: FC-004",
  "verified_source_commits": [],
  "verified_at": "<UTC timestamp>",
  "failure_reason": null
}
```

Missing, duplicate, extra 또는 `pass`가 아닌 항목이 하나라도 있으면 closure를 금지한다.
FC-004 시점에는 promotion 자체를 요구하는 `DOD-008`, `DOD-009`, current/index equality를 요구하는
`DOD-011`만 `pending_post_promotion`을 허용한다. 나머지 FA 28개와 DOD 9개는 모두 PASS여야 한다.
FC-005는 proposed current tree에서 세 row의 exact evidence를 함께 적용해 CI가 40/40 PASS를 검사한
단일 promotion commit이다. FC-006 post-merge attestation이 실패하면 program은 complete가 아니며 해당
promotion commit을 검증된 revert로 되돌리고 failure evidence를 보존한다.

### 16.1 Final Acceptance exact mapping

| ID | 판정 |
|---|---|
| FA-001 | API processor는 route façade이며 job store/queue file I/O를 직접 소유하지 않음 |
| FA-002 | Host Ops owner가 giant façade implementation으로 callback하지 않음 |
| FA-003 | Hyper-V operation이 canonical single source에서 projection됨 |
| FA-004 | ASP.NET composition root가 HTTP/noVNC/worker child와 fault를 추적함 |
| FA-005 | ASP.NET only이며 legacy `HttpListener` production reachability 0 |
| FA-006 | TypeScript source/build/browser 유지, ASP.NET static은 build output만 serve |
| FA-007 | active project마다 production caller 또는 명시된 독립 제품 계약 존재 |
| FA-008 | durable enqueue 저장 실패 job은 worker에서 실행되지 않음 |
| FA-009 | memory/disk divergence가 recovery contract로 처리됨 |
| FA-010 | uncertain external side effect 자동 중복 실행 없음 |
| FA-011 | timeout/cancel/service stop 이후 late commit 0 |
| FA-012 | durable committed job은 disconnect로 취소되지 않고 rollback 중 uncertain replay 없음 |
| FA-013 | single mutation worker 유지 |
| FA-014 | full .NET skip 0, count delta와 removed-test migration 기록 |
| FA-015 | Hyper-V provider 주요 실패 branch direct test |
| FA-016 | Host Ops family별 독립 test |
| FA-017 | job save/restart/reconciliation fault injection PASS |
| FA-018 | route/static/CORS/body/auth/noVNC/Wave 5 exact cross-transport parity |
| FA-019 | TestServer와 selected-server loopback 둘 다 PASS |
| FA-020 | candidate Web/CLI/current-card와 required actual-VM/admin evidence PASS |
| FA-021 | candidate 없는 변경은 activation-time anchor carry-forward와 stale trigger를 `promotion_not_triggered`로 기록 |
| FA-022 | Web Console과 PCVCLI만 active |
| FA-023 | IIS/Razor/MVC/Blazor/Identity/Node runtime dependency 없음 |
| FA-024 | canonical API/Web, LAN opt-in과 explicit host/IP binding 유지 |
| FA-025 | TUI source/test/package absent |
| FA-026 | Linux runtime/generic PowerShell fallback 없음 |
| FA-027 | public trusted signing/external publication `false` |
| FA-028 | 승인 없는 host mutation 0 |

FA-003과 FA-018의 count는 LC-027 authoritative rebaseline 값을 사용한다. Predecessor의 하드코딩된
`0.42.65 carry-forward` 문구는 FA-021에서 activation-time operational snapshot으로 일반화한다.

### 16.2 Definition of Done exact mapping

| ID | 판정 |
|---|---|
| DOD-001 | Wave 0~7 완료 조건 |
| DOD-002 | behavior change commit과 body-move commit 분리 |
| DOD-003 | solution Release PASS |
| DOD-004 | TRX skip/count/test migration PASS |
| DOD-005 | touched project line/branch 0.0%p ratchet PASS |
| DOD-006 | effective Full/Release lane PASS |
| DOD-007 | required Wave code_complete, Wave 5B만 closed-not-adopted |
| DOD-008 | Wave 6 code+promotion, aspnet_only, ADR-0014/installed/observation/rollback 종료 |
| DOD-009 | candidate wave만 승인된 promotion; 나머지는 promotion_not_triggered |
| DOD-010 | `git diff --check` PASS |
| DOD-011 | current/historical evidence와 index 일치 |
| DOD-012 | deferred item은 backlog 또는 ADR 후보로 이동 |

## 17. 운영 카드 공통 절차

### 17.1 Package build

Build approval request는 exact source HEAD, version, output root, recipe, expected payload inputs와
`AllowUnsignedDev + LocalTest`를 고정한다. 먼저 같은 명령의 `-DryRun`을 검증하고 승인 후
`-DryRun`만 제거한다.

Build 직전 `git status --porcelain --untracked-files=all`에서 `src/**`, `web/**`,
`packaging/windows-desktop-node/**`와 build에 영향을 주는 root props/config의 tracked 또는 untracked
변경은 0건이어야 한다. 승인된 두 사용자 docs처럼 payload 밖 untracked file은 명시 allowlist로만
제외한다. Exact `HEAD^{tree}`, deterministic source snapshot과 effective publish/web/package input hash를
approval record에 고정하며 불일치 시 OP-001이 build를 중단한다. 따라서 uncommitted source를 MSI에
포함하면서 provenance만 HEAD로 기록할 수 없다.

```powershell
$version = '<approved-version>'
$outputRoot = '<approved-absolute-artifact-root>'

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version $version `
  -OutputRoot $outputRoot `
  -SigningMode AllowUnsignedDev `
  -SigningTrustModel LocalTest `
  -DryRun

# 별도 package_service/build 승인 후 exact command에서 -DryRun만 제거한다.
```

Build card는 clean MSI SHA-256, operational candidate MSI SHA-256, payload aggregate SHA-256,
provenance commit과 source HEAD를 기록한다. 이 mode는 public signing evidence가 아니다.

### 17.2 Install, full gate와 current-card

Install approval은 build card의 exact hashes와 OP-001 hardened runner를 사용한다. Runner가 MSI를
다시 만들거나 expected hash가 다르면 실행 전에 중단한다. 승인 record는 exact elevation, host,
port, VM/ISO, artifact root, 명령과 rollback을 포함한다.

Installed gate는 최소 다음을 분리해 기록한다.

- package install/repair/uninstall exit와 SCM `PathName`
- service `Running/Automatic`과 configured/effective transport
- Web/API port split, auth boundary, OPTIONS와 PCVCLI exit
- account/session/RBAC, diagnostics와 target-backed noVNC
- queued mutation/cancel/recovery와 single worker
- Event Log redaction, boot configuration unchanged와 orphan process/port/rule 0
- actual VM pre/post/cleanup과 remaining PCV VM 0

Port 7777 owner가 불명확하거나 HNS/WinNAT/WSL reservation과 product listener를 구분할 수 없으면
bind를 강탈하지 않고 `blocked`로 멈춘다.

### 17.3 HTTP/TLS

`http_binding_tls` card는 ADR-0014 selected server와 exact command를 함께 고정한다. HTTP.sys를
선택한 경우 explicit prefix/certificate/thumbprint/URL ACL/SSL binding과
`Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1` 계열을 사용한다. Kestrel을 선택한 경우 AC-307이 만든
dedicated certificate/private-key ACL lifecycle runner만 사용하고 HTTP.sys `netsh sslcert` final state를
억지로 요구하지 않는다. Wildcard와 승인되지 않은 LAN bind는 두 server 모두 금지한다. 어느 경로든
certificate access, service configuration, firewall와 HTTP restore가 selected-server 계약대로 복구돼야
PASS다. Runner가 `sc.exe config` 또는 service stop/start를 수행하면 같은 exact command에
`package_service/install` 승인도 필요하다.

### 17.4 Lifecycle rollback

먼저 `New-PcvManualAdminRebaselineReadiness.ps1 -PlanOnly`으로 exact baseline/target과 host reservation을
확인한다. Baseline mismatch면 host를 억지로 내리지 않고 중단한다. Clean-host 실행은 실제 parameter인
`-UpdatePackagePath`를 사용한다. 모든 source summary가 실제 PASS일 때만
`New-PcvManualAdminCampaignDescriptor.ps1 -PlanOnly`과 actual descriptor 생성을 진행한다.
`missing_count=0`, `not_pass_count=0`이 아니면 closed package-pair가 아니다.

Lifecycle campaign을 dedicated host/checkpoint에서 실행한 경우에도 다음 observation start 전에 designated
observation host에 exact target을 다시 설치하고 operational MSI/payload/provenance를 rehash한 뒤 generic
current-card를 PASS해야 한다. Lifecycle host의 PASS나 target restore 기록만으로 observation host의
installed tuple을 추론하지 않는다.

## 18. Observation 계약

Required set은 정확히 다음 세 ID다.

- `OBS-LIFETIME-LEGACY-TRACKED`
- `OBS-ASPNET-DEFAULT`
- `OBS-ASPNET-ONLY`

Start card는 version, `operational_msi_sha256`, `payload_sha256`, provenance commit, observed payload hash,
workspace source HEAD, attempt ID와 UTC start를 고정하고 즉시 닫는다. 중간 sample은 장시간 sleep하는
terminal process가 아니라 자동 재개 또는 짧은 ledger event로 기록한다.

Executable sampler owner는 `OP-003`이 만드는
`packaging/windows-desktop-node/tools/Invoke-PcvInstalledObservationSample.ps1`과 대응 Pester다. 각 start
card는 승인된 runner hash/version, exact one-shot command, cadence owner와 다음 예정 UTC를 attempt에
고정한다. Runner는 mutation을 수행하지 않고 sample마다 별도 immutable artifact와 ledger event를 만든다.
최소 schema는 `sample_id`, `attempt_id`, `sampled_at_utc`, `previous_sample_id`, `gap_seconds`, installed
4-tuple, service/transport/listener/current-card 결과, P0/P1 incident query window/count, package/recovery event,
source artifact refs와 overall status다. Close card는 runner schema와 artifact 존재를 다시 검증한다.

각 current attempt는 start와 close를 포함해 최소 8개 sample을 가져야 하며 인접 sample 간격은
26시간을 넘을 수 없다. 매 sample은 다음 source를 고정한다.

- service status/start type, configured/effective transport와 process identity
- listener port owner, Web/API/PCVCLI focused smoke
- installed `operational_msi_sha256`, payload aggregate SHA-256와 provenance rehash/readback
- health counters, admission/fault state와 P0/P1 incident/Event Log query 범위
- service restart, recovery, rollback 또는 package event 유무

Incident/service/package event가 있으면 정기 cadence와 별도의 즉시 sample을 남긴다. Required sample
누락, 26시간 초과 gap 또는 evidence source 누락은 회귀 0건으로 간주하지 않고
`failed/insufficient-observation-coverage`다. 새 attempt를 시작해야 하며 누락 기간을 carry하지 않는다.

Payload가 바뀌면 다음 원자 순서를 지킨다.

1. 기존 attempt의 tuple, 기간과 무효화 사유를 immutable history에 append한다.
2. state를 `restarted`로 기록한다.
3. 새 attempt ID, tuple과 start time을 고정한다.
4. 새 attempt만 7일 계산에 사용한다.

Close card는 현재 attempt가 7×24시간 이상이고 P0/P1 regression 0건인지 확인한다. Historical,
failed 또는 restarted attempt 시간을 합산하지 않는다. 최종 current evidence, installed current-card,
rollback evidence와 `OBS-ASPNET-ONLY`는 동일한
version/`operational_msi_sha256`/`payload_sha256`/provenance 4-tuple이어야 한다. Clean MSI hash는 별도
package fact이며 observation tuple의 operational MSI를 대신하지 않는다.

## 19. State schema의 activation·weekly 구현 필드

Stable design의 activation 조건을 durable하게 판정하기 위해 schema에는 최소 다음 구현 필드를 둔다.

```json
{
  "materialization_pr_head": null,
  "materialization_merge_group_head": null,
  "materialization_merge_group_ci_ref": null,
  "materialization_merged_commit": null,
  "materialization_post_merge_ci_ref": null,
  "activation_base_head": null,
  "activation_pr_head": null,
  "activation_merge_group_head": null,
  "activation_merge_group_ci_ref": null,
  "activation_commit_locator": "Program-Activation: purecvisor-desktop-node-luna-completion-20260803",
  "activation_merged_commit": null,
  "activation_post_merge_ci_ref": null,
  "attestation_pr_head": null,
  "attestation_merged_commit": null,
  "attestation_post_merge_ci_ref": null,
  "post_merge_operational_snapshot": null,
  "designated_current_branch": "main"
}
```

Plan authority와 mutable weekly projection은 같은 state에서 다음 최소 shape를 가진다.

```json
{
  "plan_authority": {
    "base_plan_locator": "Plan-ID: purecvisor-desktop-node-luna-successor-20260803",
    "base_plan_commit": null,
    "active_revision_locator": "Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3",
    "active_revision_commit": null,
    "approval_locator": "User-Approval: weekly-service-delivery-v3-luna-max-20260803"
  },
  "weekly_service_delivery": {
    "contract": "weekly-service-delivery-projection-v1",
    "timezone": "Asia/Seoul",
    "anchor_rule": "first Monday 00:00 Asia/Seoul strictly after LC-026 PASS",
    "anchor_start_utc": null,
    "current_week_id": null,
    "weeks": [
      {
        "week_id": "SW-01",
        "sequence": 1,
        "starts_at_utc": null,
        "ends_at_exclusive_utc": null,
        "goal": "effective-current rebaseline and lifetime foundation",
        "forecast_card_pool": ["LC-027", "LC-028", "LT-001", "LT-002", "LT-003", "LT-004", "LT-005"],
        "planned_card_ids": [],
        "entry_conditions": ["LC-026 completed/pass"],
        "exit_conditions": [],
        "approval_categories": [],
        "observation_ids": [],
        "planning_base_head": null,
        "selection_simulation_ref": null,
        "available_capacity_hours": null,
        "estimated_effort_hours": {},
        "planned_effort_hours": null,
        "carryover_from": [],
        "carryover_blockers": [],
        "result_refs": [],
        "events": [],
        "derived_status": "forecast"
      }
    ]
  }
}
```

위 commit `null`은 bootstrap 입력 shape에서만 허용한다. `LC-001`은 locator를 Git graph에서 해석해
두 필드를 exact 40자 ancestor commit으로 채우고 SW-01~SW-17 forecast row를 seed한다.
`LC-026` PASS 뒤 첫 weekly-anchor ledger event가 anchor와 모든 7일 window를 계산한다. 이후 weekly
commitment/replan/review는 state에 append하는 `Ledger-Event`이며 historical row/event의 기존 값은
불변이다. Cached `derived_status`가 event/time/card projection과 다르면 validation failure다.

`activation_base_head`는 feature HEAD가 아니라 activation PR 작성 직전 materialization merge를 포함한
최신 `main` HEAD다. `program_status=active` 선언과 validator가 파생하는 effective-current 판정을
구분한다. Merge-group temporary head와 final merged commit의 equality를 요구하지 않는다. 각각의
minimum required CI, PR/queue mapping, commit-preserving ancestry, attestation PR post-merge guard와
snapshot equality가 모두 없으면 effective current가 아니다.

## 20. 검증 명령

각 task card가 focused RED/GREEN을 구체화하고 effective changed-path를 사용한다. 공통 최종 명령은
다음과 같다.

```powershell
dotnet build src/DesktopNode.sln -c Release -warnaserror
dotnet test src/DesktopNode.sln -c Release

npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web

foreach ($pesterPath in @(
  'packaging/windows-desktop-node/tests',
  'packaging/windows-desktop-node/installer/tests',
  'web/tests'
)) {
  $result = Invoke-Pester -Path $pesterPath -PassThru -Output Detailed
  if ($result.FailedCount -gt 0) { throw "Pester failed: $pesterPath" }
}

$changedPaths = @(& git diff --name-only origin/main...HEAD)
if ($LASTEXITCODE -ne 0 -or $changedPaths.Count -eq 0) {
  throw 'actual changed-path list is required'
}

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath $changedPaths `
  -ArtifactRoot artifacts/development-verification-luna-completion-final

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 `
  -Check

git diff --check
```

Quality capture와 ratchet은 다음 owner를 사용한다.

```powershell
$qualityRoot = 'artifacts/csharp-architecture-quality-final'

& packaging/windows-desktop-node/tools/Invoke-PcvDotNetQualityCapture.ps1 `
  -SolutionPath src/DesktopNode.sln `
  -ArtifactRoot $qualityRoot

& packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1 `
  -ResultsRoot "$qualityRoot/test-results" `
  -BaselinePath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json `
  -MigrationManifestPath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json
```

`PlanOnly`와 dry-run은 suite selection과 mutation plan 검증일 뿐 code/installed/operational PASS evidence가
아니다. S는 focused+Fast, M은 focused+Full, L은 focused+Release와 independent review/required CI를
모두 요구한다.

## 21. Hard stop과 rollback

| 조건 | 처리 |
|---|---|
| HEAD/dependency/latest-main drift | production 변경 전에 `stale`, start HEAD 재기준화 |
| required checks/merge-group/ruleset 증명 불가 | activation `blocked`; ordinary PR CI로 대체 금지 |
| focused/Full/Release 실패 | 현재 card 유지, 다음 card 금지 |
| Luna가 L 경계 발견 | `handoff_required`, Sol로 routing history와 함께 인계 |
| exact 승인 command/target/hash 불일치 | mutation 전 중단, 재승인 |
| non-admin, ISO/topology/port owner 불명확 | operational card `blocked` |
| operational MSI/payload/provenance mismatch | install/promotion 금지 |
| service/binding/boot/VM cleanup 복원 실패 | rollback 후 실패 evidence 보존 |
| package-pair baseline mismatch | host downshift 금지, dedicated baseline 재예약 |
| Burn/MSIX external summary 부재 | lifecycle/descriptor blocked, JSON 합성 금지 |
| observation payload drift | history 보존 후 새 7일 attempt |
| P0/P1 regression | observation failed, promotion 금지 |
| 동일 failure fingerprint 3회 | `blocked`, 새 가설/결정 없이는 retry 금지 |
| public signing/publication 요구 또는 claim true | `blocked_out_of_scope` |
| state/schema/index/4-tuple 불일치 | 완료 주장 거부 |

Activation 전 control-plane failure는 predecessor를 current로 유지한 채 candidate를 고친다. Activation
후에는 predecessor를 재활성화하지 않고 `control_recovery_blocked`에서 마지막 검증 state를 복구한다.
Program deactivation은 별도 design과 사용자 승인이 필요하다.

## 22. PR 경계와 rollout

1. PR #176은 approved design, base `Plan-ID`와 v1 revision을 original commit ancestry가 남는
   merge commit으로 `main`에 반영했고 exact merged-main CI를 PASS했다. 이 PR을 activation 근거로
   재사용하지 않는다.
2. PR #177은 `Design-Amendment: luna-control-bootstrap-v1`과 그 사용자 승인 locator를 보존하는
   amendment-only merge 및 exact merged-main CI를 PASS했다. 같은 amendment를 다시 만들지 않는다.
3. PR #178은 v2 revision original commit과 locator를 보존하는 merge 및 exact merged-main CI를 PASS했고,
   PR #179는 v2 weekly spec/Developer Index/Coding Guide projection을 별도 merge로 정렬했다. 둘은 v3의
   historical predecessor이며 active revision으로 재사용하지 않는다.
4. PR #180은 `Design-Amendment: luna-max-routing-v1`과 그 사용자 승인 locator를 보존하는
   amendment-only merge 및 exact merged-main CI를 PASS했다.
5. 이 v3 revision은 successor 문서 하나만 바꾸는 별도 commit/PR에서
   `Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v3`와
   `User-Approval: weekly-service-delivery-v3-luna-max-20260803`을 보존하는 `MERGE` 및 exact
   merged-main CI를 통과해야 한다.
6. v3 merge 뒤 weekly spec, Developer Index와 Coding Guide만 별도 derived projection PR에서 Max routing과
   v3 source locator로 정렬하고 `MERGE` 및 exact merged-main CI를 통과한다.
7. derived merge와 post-merge CI를 확인한 exact fresh `main` HEAD를 사용자에게 제시하고 stable design의
   형식으로 새 control-only materialization 승인을 받는다. v3 revision 승인 또는 사용되지 않은
   `User-Approval: luna-control-materialization-dbac0ae5abd8-20260803`을 재사용하지 않는다.
8. 승인된 fresh `main`에서 materialization-only branch/PR을 만들고 LC-001~LC-023을 실행한다. Product code,
   current pointer와 host mutation은 포함하지 않는다. Merge method는 `MERGE`이며 `SQUASH|REBASE`를
   LC-023 pre-merge guard가 차단한다.
9. Fresh main의 LC-024 audit-only PR이 final materialization merge에서 original Card-ID
   commits/trailers와 parent ancestry가 보존됐음을 재검증하고 PR head, merge-group head, final merged
   commit과 각 CI ref를 고정한다.
10. 다시 fresh `main`에서 activation-only PR을 만들고 LC-024 PASS, 사용자 승인과 LC-025만 반영한다.
    이 PR도 `MERGE`만 허용한다.
11. Activation merge 뒤 fresh main에서 LC-026 attestation-only PR을 열어 activation PR head,
    merge-group head, final merged commit, minimum CI와 snapshot equality를 기록한다. 이 PR도 `MERGE`와
    post-merge minimum CI를 통과한 뒤에만 validator-derived effective current가 true다.
12. LC-027/028 rebaseline 뒤 P1 critical path를 시작한다.
13. 각 7일 observation이 running인 동안 product payload를 설치하지 않는 P2와 non-product spike를
    전역 직렬 카드로 진행한다.
14. P3~P6을 rollout 단계별 package와 rollback 경계로 수행한다.
15. P7에서 machine acceptance, final current evidence와 program closure를 수행한다.

이 v3 revision과 derived projection 단계에서는 package build/install, service/port 변경과 Hyper-V
mutation을 수행하지 않는다.

`weekly-service-delivery-v3` revision commit은 이 successor 문서만 바꾼다. 아직 존재하지 않는 state/schema,
LC/task card, stable design, workflow, product source, current pointer와 GA evidence를 만들거나 고치지 않는다.
사용자 소유 untracked 문서 두 개도 수정·삭제·stage하지 않는다. Weekly state materialization은 기존
bootstrap/Max amendment와 별도 fresh-main materialization 승인이 모두 끝난 뒤 `LC-001`이 수행한다.

어느 control PR에서도 merge commit을 사용할 수 없거나 squash/rebase가 강제되면 locator/ancestor 모델을
조용히 완화하지 않는다. Stable design amendment와 새 Git evidence model 승인 전까지 activation은
`blocked`다.

## 23. 100% 완료 판정

```text
all required cards execution_status=completed and verification_result=pass
AND every required L/Release card review_status=pass
AND Wave 0 through Wave 7 completion conditions pass
AND all required code waves are code_complete
AND inherited Wave 5B alone is closed-not-adopted
AND Wave 6 is code_complete and promotion_complete
AND http_transport_rollout=aspnet_only
AND legacy HttpListener production reachability=0
AND full .NET/Web/Pester suite skip=0
AND current source/SDK/collector/test/coverage baseline is exact
AND touched-project line and branch regression=0.0 percentage points
AND required package/current-card/actual-VM/rollback evidence pass
AND all three required observations have state=passed
AND each current attempt spans at least seven full days with P0/P1=0
AND restarted/failed/history duration contributes zero to current attempts
AND final current evidence, installed current-card, rollback evidence and OBS-ASPNET-ONLY share one version/operational-MSI/payload/provenance 4-tuple
AND FA-001..FA-028 all pass
AND DOD-001..DOD-012 all pass
AND Git, documentation and evidence indexes agree
AND public_trusted_signing=false
AND external_stable_publication=false
```

이 100%는 internal/private-network product completion이다. Public signing, Winget과 external stable
publication을 완료 분모에 넣지 않는다. Weekly `met|partial|blocked` 수나 17주 baseline 준수율도 완료
분모 또는 card PASS 증거가 아니다.

## 24. 이 문서 승인 후 첫 행동

이 문서의 review approval은 materialization 착수 승인과 동일하지 않다. 다음 사용자의 명시 승인
전에는 repository pointer나 control-plane 파일을 더 만들지 않는다. 다음 단계의 첫 행동은 이 v3
revision을 commit-preserving merge로 `main`에 반영하고 exact post-merge CI를 확인한 뒤 세 derived 문서를
별도 projection merge로 정렬하는 것이다. 그 exact fresh-main SHA를 제시해 새 materialization approval
locator를 받은 뒤, base `Plan-ID`, active v3 `Plan-Revision`, revision approval, bootstrap/Max amendment와
각 amendment approval locator가 각각 유일한 ancestor인지 확인한다. Selector-to-execution-ID mapping,
approved base HEAD, `LC-001.start_head`와 result first parent가 같다는 조건까지 고정한 다음에만 `LC-001`
strict schema RED test를 작성한다.

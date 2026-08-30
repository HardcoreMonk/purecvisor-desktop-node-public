# PureCVisor Desktop Node 차선 분리 개발 절차 설계

- Design-ID: `pcv-lane-separated-development-procedure-v1`
- 작성일: `2026-08-27`
- 문서 상태: `approved-for-implementation`
- 승인 locator:
  - `User-Approval: pcv-dev-procedure-lane-model-20260827`
  - `User-Approval: pcv-dev-procedure-doc-plus-min-guards-20260827`
  - `User-Approval: pcv-dev-procedure-lanes-0-3-20260827`
  - `User-Approval: pcv-dev-procedure-operating-rules-20260827`
- 대상: 공개 저장소 `purecvisor-desktop-node-public`의 에이전트·인간 개발 절차
- 제품 payload 변경: 이 문서 자체는 `false`. 구현 슬라이스 2의 runner/식별자 가드만 예외로 연다.
- host/VM/service/package mutation: 이 문서 자체는 `false`
- public trusted signing: `false`
- external stable publication: `false`

> 작업 시작 체크리스트, 검증 명령, 승인표와 종료 보고 형식은
> `docs/DEVELOPMENT_PROCEDURE.md`에서 시작한다. 이 설계는 Lane 의미와 금지 조건의 canonical
> owner로 유지된다.

## 1. 목적

코드 검증, 설치본 한 경로 프로브, operational current 승격이 한 세션에서 섞이지 않게 한다.
한 차선이 다른 차선의 예산·권한·증거를 먹지 않는다.

이 절차가 줄이는 것은 세 가지를 한 덩어리로 처리하는 시간이다.

- 결함 하나를 계약으로 닫는 시간
- 설치본에서 그 결함이 실제로 도는지 확인하는 시간
- FAIL 프로브를 current로 오인해 문서를 고치는 시간

정확성 계약은 유지한다. ADR-0015의 `operational_current`와 `feature_qualification` 분리,
Required CI 네 shard, 회로 차단기의 stop contract는 폐기하지 않는다.

## 2. 현재 문제

2026-08-27 SavedOnly 실행과 직전 재점검에서 다음이 한 절차로 붙어 있었다.

1. **권위 충돌.** ledger current는 `0.42.74-admin-smoke`인데 호스트 설치본은
   `0.42.75-admin-smoke`였다. 에이전트가 문서 current를 설치 권위로 읽으면 잘못된 Version으로
   runner를 띄우거나, 설치본 프로브를 current 승격으로 착각한다.
2. **식별자 불일치.** `vm.save` mutation은 Hyper-V GUID와 표시 이름을 모두 찾는다.
   `GET /api/v1/vms/{id}`는 inventory 행만 보고, inventory `Id`/`Name`은 WMI `ElementName`이다.
   SavedOnly는 GUID로 `vm get`을 호출해 exit `1`이 났고, 제품 `vm.save` job은 `succeeded`였다.
   04274의 WMI `32775`와 다른 결함인데 같은 `actual_vm_tested=fail` 주머니에 들어갈 수 있다.
3. **실패 코드 붕괴.** runner는 내부 `PCV_VM_NOT_FOUND` 또는 inventory incomplete를
   `PCV_P0_COMMAND_FAILED`로만 남겼다. 다음 checkpoint가 원인을 다시 추측해야 한다.
4. **캠페인이 fail-fast를 먹는다.** 04275 설계 Stage 0~7과 dual-hash Stage 1이, 이미 설치된
   0.42.75에서 Saved 왕복 한 경로를 보기 전에 도구 작업을 늘린다.
5. **회로 차단기 단위가 차선과 다르다.** 기본 30분/도구 묶음 18은 Lane 1(코드)에는 맞다.
   Lane 2(실제 VM create/start/save)는 같은 숫자로 재면 정상 대기도 회로 개방으로 보인다.
   반대로 Lane 1에서 host mutation을 같은 checkpoint에 붙이면 30분이 부족해 범위가 늘어난다.

이 다섯은 구현 속도 문제가 아니라 **차선이 없어서** 생긴다.

## 3. 결정

### 3.1 네 차선만 인정한다

| 차선 | 이름 | 하는 일 | 하지 않는 일 |
| --- | --- | --- | --- |
| Lane 0 | 권위 읽기 | ledger current, 설치본 manifest, `HEAD`를 읽고 이번 checkpoint의 작업 권위를 고른다 | mutation, 문서 승격, “아마 04275일 것” 추정 |
| Lane 1 | 계약 | RED/GREEN, focused Required CI shard, 식별자·실패코드 계약 | Hyper-V VM, MSI, service, current-evidence write |
| Lane 2 | 설치본 프로브 | 관리자 opt-in, 한 family, 한 artifact root | Full P0 자동 연쇄, package-pair, current-evidence write |
| Lane 3 | 승격 | package/fullgate/manual-admin/current-evidence/feature ledger를 PASS 증거로만 연결 | Lane 2 FAIL를 pass로 재해석, 설치본만 보고 current를 올림 |

기본 진행은 `0 → 1 → (승인 시) 2 → (승인 시) 3`이다. Lane을 건너뛰면 명시적 사용자 승인이
있어야 한다. `재개`/`계속`은 회로 차단기대로 **다음 한 checkpoint**이며, 그 checkpoint는
위 네 차선 중 정확히 하나에 속해야 한다.

### 3.2 이번 checkpoint의 작업 권위를 이름으로 고른다

Lane 0가 기록하는 값은 세 개다.

- `ledger_current`: `docs/ga-ready/current-evidence.json`의 `current.version`
- `installed_current`: 설치본 `product-manifest.json`의 `version`
- `source_head`: 이 checkout의 `git rev-parse HEAD`

세 값이 달라도 된다. 다른 것이 오류가 아니라 **상태**다. 오류는 고르지 않고 Lane 2/3을
시작하는 것이다.

규칙:

- Lane 1의 작업 권위는 `source_head`다. 설치본과 ledger가 달라도 코드를 고칠 수 있다.
- Lane 2의 작업 권위는 `installed_current`다. `-Version`은 설치본 manifest와 대소문자 포함
  완전 일치해야 한다. ledger가 낮거나 높아도 프로브는 설치본을 검증한다.
- Lane 3의 작업 권위는 `ledger_current`를 **무엇으로 바꿀지**다. Lane 2 PASS와 package
  규칙이 있을 때만 연다.

2026-08-27 호스트는 `installed_current=0.42.75-admin-smoke`,
`ledger_current=0.42.74-admin-smoke`다. SavedOnly는 Lane 2이고 설치본 04275를 검증한 것이며
ledger를 04275로 올리지 않았다. 이 해석을 절차의 정본으로 둔다.

### 3.3 식별자는 mutate와 readback이 같다

canonical operator id는 **`GET /api/v1/vms/{id}`가 받는 문자열**이다. Web Console과 PCVCLI
readback, Lane 2 runner의 `vm get`/`vm delete`는 그 문자열을 쓴다.

mutation 경로가 Hyper-V GUID도 받더라도, 프로브가 GUID로 get하고 표시 이름으로 list하는
혼합은 Lane 1 FAIL다. 제품 inventory `Id`를 GUID로 통일하는 변경은 이 설계의 필수 범위가
아니다. 그 변경은 별도 제품 payload slice다.

2026-08-27 관측: inventory `Id`/`Name`은 WMI `ElementName`이다. 따라서 현행 canonical
operator id는 VM 표시 이름이다. P0 runner는 create에 쓴 표시 이름으로 get/delete해야 한다.

### 3.4 Lane마다 시작 계약과 예산이 다르다

회로 차단기의 stop contract는 유지한다. 예산을 **같은 checkpoint에서 조용히 늘리지 않는다.**
차선을 바꾸면 **새 시작 계약을 공개**한다.

| 차선 | elapsed | tool batch | review | mutation |
| --- | ---: | ---: | --- | --- |
| Lane 0 | 10분 | 6 | 0 | false |
| Lane 1 | 30분 | 18 | 정규 1 + 제한 재검토 2 | false |
| Lane 2 | 45분 | 12 | 정규 1, 재검토 0 | 사용자 명시 opt-in만 |
| Lane 3 | 30분 | 12 | 정규 1 + 제한 재검토 2 | 문서/ledger만. host mutation은 별도 승인된 campaign worker |

Lane 2의 45분/12는 VM create 대기와 로그 읽기를 포함한다. 한도에 닿으면 추가 프로브 없이
summary와 잔여 VM 여부만 보고하고 회로를 연다. 동일 원인 3회 FAIL는 모든 차선에 그대로다.

Lane 1에서 설치본 mutation이 필요해지면 그 checkpoint를 닫고 Lane 2 승인을 받는다.
Lane 2 PASS 뒤에 Lane 3을 자동으로 시작하지 않는다.

### 3.5 FAIL 프로브는 current를 쓰지 못한다

Lane 2 `overall_verdict=FAIL`인 summary는 `actual_vm_tested=pass`의 입력이 될 수 없다.
`current-evidence.json` write, generated current 블록 갱신, feature ledger verdict를 pass로
바꾸는 일은 Lane 3만 한다.

부분 성공은 기록한다. 예: `queued_jobs.vm-save.status=succeeded`이면서
`slice_verdicts.saved_lifecycle=FAIL`이면, 제품 save mutation과 runner readback을 한
blocker로 합치지 않는다. 다음 Lane 1 checkpoint는 readback/식별자만 고친다.

### 3.6 최소 자동 가드만 이 설계의 코드 범위다

캠페인 상태기계(`pcvverify campaign-tooling`, dual-hash manifest, Stage 0~7 worker)는
후속 설계다. 이 설계가 코드로 고정하는 가드는 두 종류뿐이다.

1. **식별자 계약.** mutate에 쓰는 id로 get이 되어야 한다. runner는 canonical operator id를
   `vm get`/`vm delete`에 쓴다. 내부 CLI 실패 코드는 summary `error`에 보존한다.
2. **승격 거부 계약.** FAIL 프로브 summary 또는 `actual_vm_tested=fail` observation으로
   current-evidence를 pass/eligible로 쓰지 못한다. 기존
   `pcv-feature-promotion-decision-v1`와 04274 P0 fail fixture를 유지하고, Lane 2 FAIL
   summary를 pass 입력으로 넣는 경로가 있으면 그 경로를 RED로 막는다.

## 4. 차선별 작업 흐름

### 4.1 Lane 0 — 권위 읽기

읽기만 한다. 기본 출력은 네 줄이다.

```text
ledger_current=<version>
installed_current=<version-or-absent>
source_head=<sha>
working_authority=<ledger_current|installed_current|source_head>
```

설치본이 없으면 `installed_current=absent`이고 Lane 2는 열지 않는다. Hyper-V `Get-VM`는
Lane 0의 기본이 아니다. VM 존재 확인은 Lane 2 preflight다.

### 4.2 Lane 1 — 계약

TDD. 호스트 mutation 없음. pre-commit은 기존 규칙을 따른다.

```text
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

영향 범위의 focused test만 이 checkpoint의 완료 증명이다. clean HEAD의 네 shard는 커밋 후
별도 Lane 1 checkpoint이거나 CI다.

식별자 가드의 RED는 이 차선에서 먼저 실패해야 한다. GUID로 get하고 표시 이름으로 list하는
runner/테스트는 Lane 2에 가기 전에 막는다.

### 4.3 Lane 2 — 설치본 프로브

필수 입력:

- `-Version` = `installed_current`
- `-Mode` 한 값. 기본 Saved lifecycle 결함이면 `SavedOnly`
- 전용 `ArtifactRoot`, 전용 `VmRoot`(볼륨 루트 아래 세그먼트 2개 이상)
- canonical operator id로 get/delete
- 관리자 PowerShell 7

필수 출력: `summary.json`. `ok`, `overall_verdict`, slice verdict, queued job status,
Hyper-V/product readback, `error`(내부 `PCV_*` 포함), `cleanup.verdict`,
`host_mutation_performed`, `secret_observed`.

`DryRun`은 host mutation이 없고, 설치본 CLI/Hyper-V를 호출하지 않는다. DryRun PASS를
Lane 2 PASS로 계산하지 않는다.

`Mode=Full`은 SavedOnly PASS 뒤 **새** Lane 2 checkpoint다. 같은 artifact root, 같은 VM
이름을 재사용하지 않는다.

### 4.4 Lane 3 — 승격

입력이 모두 PASS일 때만 연다.

- Lane 2 해당 family의 `overall_verdict=PASS`와 `cleanup.verdict=PASS`
- 열려는 current의 package/fullgate/current-card 규칙
- feature ledger required stage
- 사용자 명시 승인

04275는 이 설계를 충족하기 전에 operational current가 아니다. 설치본이 04275여도
ledger는 04274로 남을 수 있다. 그 상태는 Lane 0가 보고하는 정상적인 skew다.

## 5. 최소 가드 파일 지도

구현 계획은 후속 writing-plans가 연다. 이 설계가 고정하는 파일 경계는 다음이다.

| 파일 | 가드 |
| --- | --- |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | 차선별 시작 계약·예산. 모호한 재개는 한 차선 한 checkpoint |
| `config/agent-execution-circuit-breaker.json` | Lane 0/1/2/3 수치. 기본값 키는 Lane 1과 호환 |
| `AGENTS.md` 작업 원칙 | 네 차선 표와 “FAIL 프로브는 current를 못 씀” 한 줄. generated current 블록은 건드리지 않음 |
| `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1` | `vm get`/`vm delete`에 canonical operator id(현행: 표시 이름). `error`에 내부 `PCV_*` 보존 |
| `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs` | 위 runner 토큰. GUID-only get 금지 |
| HyperV 또는 Api focused test | `GET /api/v1/vms/{id}` lookup이 inventory canonical id를 쓴다는 계약. 현행은 표시 이름 |
| 기존 feature promotion tests/fixtures | FAIL observation으로 `promotion_eligible=true`가 되지 않음 |

새 `pcvverify` command, 새 campaign manifest schema, dual-hash Git blob reader는 이 표에
없다.

## 6. 오류와 보고

Lane 2 summary `error`는 `Get-SafeFailureCode`가 잡은 **첫 번째 원인 코드**를 우선한다.
CLI `vm get`이 404/`PCV_VM_NOT_FOUND`이면 summary `error`는 `PCV_VM_NOT_FOUND`다.
`PCV_P0_COMMAND_FAILED`는 내부 코드가 없을 때만 쓴다.

에이전트 종료 보고는 회로 차단기 형식을 유지하되 차선을 앞에 둔다.

```text
lane=<0|1|2|3>
working_authority=<...>
elapsed/limit
tool batches/limit
done
remaining
out-of-scope count
next stop
host_mutation_performed
current_evidence_written=false|true
```

`current_evidence_written=true`는 Lane 3가 아니면 즉시 회로 개방 조건이다.

범위 밖 발견은 report-only다. 예: 빈 VM parent directory, Public Boundary Pester residue,
04275 dual-hash worktree dirty.

## 7. 비목표

- 04275 package rebuild, fullgate 재실행, manual-admin six bucket을 이 설계가 실행하지 않는다
- inventory `Id`를 Hyper-V GUID로 바꾸는 제품 payload는 별도 설계다
- `pcvverify campaign-tooling` dual-hash Stage 1을 이 설계가 완료하지 않는다
- P1 clone/notes, P2 noVNC, TUI 복원, public signing, 외부 publication
- 회로 차단기를 제거하거나, 사용자가 승인한 예산을 소급 확장하는 일
- archive/spikes Pester를 Required CI로 되돌리는 일

## 8. 성공 기준

절차 구현이 끝난 뒤 다음이 동시에 참이어야 한다.

1. Lane 0가 ledger/installed/HEAD skew를 한 번에 보고하고, 고르지 않은 권위로 Lane 2/3을
   시작하지 않는다.
2. GUID-only `vm get` runner는 Lane 1 테스트에서 FAIL한다.
3. 표시 이름 canonical id로 get/delete하는 runner는 Lane 1 PASS다.
4. Lane 2 FAIL summary를 입력해도 feature promotion은 `promotion_eligible=false`를 유지하고
   current-evidence write 경로가 없다.
5. 에이전트가 Lane 1 checkpoint에서 Hyper-V VM을 만들지 않는다.
6. 이 설계 문서 작성만으로 operational current, MSI hash, public signing 주장이 바뀌지 않는다.

## 9. 구현 슬라이스 초안

writing-plans가 이 순서를 상세 작업으로 분해한다. 이 문서가 순서를 고정한다.

1. **문서 차선.** 회로 차단기, JSON 수치, `AGENTS.md` 작업 원칙. 테스트는 기존 회로 차단기
   문서 계약이 새 키를 읽도록만 확장한다.
2. **식별자 가드.** runner get/delete id + Delivery contract test + API/HyperV lookup 계약.
   설치본 mutation 없음.
3. **실패 코드 보존.** summary `error`가 내부 `PCV_*`를 유지. DryRun/adapter 테스트만.
4. **승격 거부.** FAIL 프로브/fail observation으로 current pass write가 열리지 않는 테스트.
   이미 닫혀 있으면 새 코드 없이 계약을 고정한다.
5. **(별도 승인)** Lane 2 SavedOnly 재실행. 슬라이스 2 PASS 뒤에만. current write 없음.

슬라이스 5는 이 설계의 구현 완료 조건이 아니다. Lane 1 가드가 먼저 닫혀야 한다.

## 10. 관련 문서

| 문서 | 역할 |
| --- | --- |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | stop contract 단일 진실. 이 설계가 차선 예산을 더한다 |
| `config/agent-execution-circuit-breaker.json` | 기계 수치 |
| `docs/ga-ready/current-evidence.json` | operational current와 feature qualification |
| `docs/adr/0015-feature-evidence-promotion-policy.md` | current와 기능 승격 분리 |
| `docs/superpowers/specs/2026-08-21-purecvisor-desktop-node-04275-promotion-closure-design.md` | 04275 campaign. 이 절차의 Lane 3 후속 |
| `docs/SERVICE_PLAN.md` | P0 기능 기획. 절차가 아니라 제품 범위 |
| `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | Required CI 네 shard |

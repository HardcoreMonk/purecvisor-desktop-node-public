# PureCVisor Desktop Node Stabilize Then Split 재설계 PRD

작성 기준: 2026-05-11
상태: 설계 승인됨. Implementation plan 작성 전 사용자 리뷰 대기 대상.

## 목적

이 PRD는 `purecvisor-desktop-node`를 새 제품으로 다시 만드는 문서가 아니다.
현재 `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime` 상태와 내부
사설망 전용 배포 경계를 제품 기준선으로 고정한 뒤, 구현 가능한 재설계
workstream으로 나누기 위한 문서다.

성공 기준은 이 문서를 읽고 바로 다음 implementation plan으로 쪼갤 수 있는
수준이다. 대상 plan은 `runtime-core-boundary-plan`,
`hyperv-domain-split-plan`, `host-ops-domain-split-plan`,
`packaging-release-control-plane-plan`,
`operator-surfaces-alignment-plan`이다.

## Current Baseline

현재 제품 기준선은 다음 결정을 유지한다.

- `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`
- `DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service`
- `DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime`
- `DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION: internal-private-network-only`

배포와 운영 경계는 ADR-0006을 따른다. Public trusted signing, winget public
submission, external stable publication, public stable installer URL, clean-host
public signed install/update/rollback smoke는 scope 밖이다. 기존 public
distribution 후보와 blocked evidence는 historical 또는 closed-not-adopted
자료로 보존한다.

현재 기본 surface는 Web Console `http://127.0.0.1/`, Web API
`http://127.0.0.1:7777/api/v1/...` 분리다. `/pcv-config.js`가 browser API
origin을 주입하고, LAN mode는 명시 opt-in과 token source가 있을 때만 허용한다.

현재 auth 기준선은 no-default-account bootstrap, bearer token fallback,
account/RBAC/JWT path 공존이다. Account가 구성되지 않은 상태에서는 기존
bearer token gate가 authoritative하다.

현재 evidence는 삭제하지 않는다. 재설계 중에는 evidence를 `current`,
`historical`, `supporting`, `closed-not-adopted`로 재분류할 수 있지만,
증거 자체를 대량 삭제하거나 의미를 바꾸지 않는다.

Host mutation smoke는 기본 개발 loop가 아니다. 실제 Hyper-V VM 생성/삭제,
service install/start/stop/delete, Windows Firewall, Event Log, trust store,
Task Scheduler, MSI install/repair/uninstall, update/rollback은 계속 명시적
관리자 opt-in gate에서만 실행한다.

## Problem Statement

현재 저장소는 제품화 evidence를 많이 축적했지만, 구현과 문서 경계가 다시
접혀야 하는 상태다.

첫째, target architecture와 실제 solution 구조가 어긋난다. 기존 GA-ready
설계는 `src/DesktopNode.HyperV` 같은 Hyper-V domain 분리를 목표로 하지만,
실제 Hyper-V read/mutation 구현은 `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
안에 크게 모여 있다.

둘째, runtime route, auth/session, job runtime, diagnostics, ops summary가
`src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`에 집중되어 있다. API
request processor는 transport-independent contract owner로 남아야 하지만,
route family와 domain logic 경계가 더 명확해야 한다.

셋째, Windows host mutation이 `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
에 넓게 집중되어 있다. Service lifecycle, Event Log, firewall, trust store,
Credential Manager, data-root, config/job-store migration은 각각 작은 operation
family로 나뉘어야 한다.

넷째, packaging/release/evidence 문서가 README와 AGENTS까지 확장되어 진입점과
ledger의 역할이 섞였다. README와 AGENTS는 최신 증거 전체 목록이 아니라
현재 결정, 주요 진입점, 검증 기준을 안내하는 문서로 작아져야 한다.

다섯째, Web Console, TUI, CLI, user guide가 같은 운영자 여정을 설명하도록
용어와 흐름을 정렬해야 한다. 설치본 기준 login, VM operation, diagnostics,
update/rollback, evidence handoff가 표면마다 다르게 말하면 운영자가 제품을
하나의 도구로 이해하기 어렵다.

## Target Workstreams

### Runtime/Core

`Runtime/Core`는 HTTP route contract, auth/session/RBAC, job state machine,
diagnostics bundle/list/download, ops summary의 공통 응답 형식을 소유한다.

소유 파일과 모듈:

- `src/DesktopNode.Api/**`
- `src/DesktopNode.Runtime/**`
- `src/DesktopNode.Contracts/**`
- 관련 테스트: `src/DesktopNode.Api.Tests/**`, `src/DesktopNode.Runtime.Tests/**`,
  `src/DesktopNode.Contracts.Tests/**`

소유하지 않는 것:

- Hyper-V WMI/CIM 세부 구현
- Windows service, firewall, Event Log, trust store, Credential Manager mutation
- MSI build/update/rollback orchestration
- Web/TUI/CLI presentation copy

Implementation slices:

- Route dispatch table을 route family별로 분리한다.
- Auth/session/RBAC boundary를 API request processor에서 별도 service로 분리한다.
- Job runtime contract와 job store serialization boundary를 정리한다.
- Diagnostics bundle create/list/download route family를 분리한다.
- Ops summary contract를 route processing과 evidence reading boundary로 나눈다.

Verification gates:

- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj`
- `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj`
- `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj`
- 변경 범위가 넓으면 `dotnet test src/DesktopNode.sln`

### HyperV

`HyperV`는 독립 domain으로 분리한다. 목표는 현재
`DesktopNodeHyperVNativeAdapter.cs`를 read model, VM lifecycle, checkpoint
lifecycle, console handoff, WMI provider abstraction으로 나누는 것이다.

소유 파일과 모듈:

- 현재: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
- 목표: `src/DesktopNode.HyperV/**` 또는 같은 책임을 갖는 독립 domain module
- 관련 테스트: Hyper-V adapter xUnit, installed smoke evidence linkage

소유하지 않는 것:

- HTTP route parsing과 auth enforcement
- Web/TUI/CLI presentation
- Packaging build and installer lifecycle
- Public distribution reopening

Implementation slices:

- Hyper-V DTO/read model을 API response assembly에서 분리한다.
- VM inventory/detail read provider를 별도 file 또는 project로 이동한다.
- VM lifecycle mutation provider를 create/start/shutdown/poweroff/restart/delete로
  나눈다.
- Checkpoint list/create/restore/delete provider를 별도 boundary로 나눈다.
- WMI scope/query/resource modification helper를 provider abstraction으로 모은다.
- `PCV_*` structured failure contract를 이동 전후 동일하게 고정한다.

Verification gates:

- Hyper-V adapter xUnit
- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj`
- behavior-preserving split 이후 existing installed smoke evidence를 current
  baseline과 연결
- 실제 host mutation은 별도 explicit admin opt-in plan에서만 실행

### Host Ops

`Host Ops`는 Windows host mutation의 단일 경계다. `service-action` 아래의
SCM, Event Log, firewall, trust store, Credential Manager, data-root lifecycle,
config/job-store migration apply를 각각 작은 operation family로 분리한다.

소유 파일과 모듈:

- 현재: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- 관련 controllers: `src/DesktopNode.Host/DesktopNodeWindows*.cs`
- 관련 테스트: `src/DesktopNode.Host.Tests/**`

소유하지 않는 것:

- API runtime route behavior
- Hyper-V VM route implementation
- Packaging build script의 artifact assembly
- Web/TUI/CLI copy

Implementation slices:

- Service lifecycle operation family를 configure/repair/remove/start/stop/status로
  분리한다.
- Event Log operation family를 register/remove/repair/write-test/volume-guard/default
  transition으로 분리한다.
- Firewall operation family를 enable/remove와 LAN approval guard로 분리한다.
- Trust store operation family를 install/remove와 certificate spec validation으로
  분리한다.
- Credential Manager operation family를 proof/default transition/service token
  boundary로 분리한다.
- Data-root lifecycle과 config/job-store migration apply를 service lifecycle에서
  분리한다.

Verification gates:

- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
- service plan에 닿으면 packaging product plan Pester
- host mutation behavior 변경은 explicit admin opt-in smoke evidence plan 필요

### Packaging/Release

`Packaging/Release`는 설치, update/rollback, 내부 배포 artifact contract를
소유한다. 기존 PowerShell packaging wrapper를 즉시 제거하는 것이 아니라, 어떤
contract가 .NET product binary로 이동해야 하는지와 어떤 runner가 packaging-only로
남는지를 나눈다.

소유 파일과 모듈:

- `packaging/windows-desktop-node/**`
- `packaging/windows-desktop-node/installer/**`
- `docs/ga-ready/**`
- `docs/adr/0002-release-version-policy.md`
- `docs/adr/0003-internal-trusted-signing-policy.md`
- `docs/adr/0006-internal-private-network-distribution.md`

소유하지 않는 것:

- Hyper-V WMI behavior
- API route implementation details
- Operator UI layout details
- Public distribution reopening

Implementation slices:

- MSI payload contract와 product manifest file contract를 정리한다.
- Update/rollback contract와 transaction journal evidence boundary를 정리한다.
- Internal distribution matrix와 public closed-not-adopted matrix를 분리한다.
- Publication descriptor와 artifact evidence schema를 작게 만든다.
- README/AGENTS가 evidence ledger를 복제하지 않도록 evidence index entrypoint를
  만든다.

Verification gates:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`
- `git diff --check`
- signed build 또는 installed lifecycle smoke는 명시 opt-in evidence plan에서만 실행

### Operator Surfaces

`Operator Surfaces`는 Web Console, TUI, CLI, user docs를 하나의 Windows
관리자 여정으로 정렬한다. 설치본 기준 login, VM operation, diagnostics,
update/rollback, evidence handoff가 같은 용어와 boundary를 사용해야 한다.

소유 파일과 모듈:

- `web/**`
- `src/DesktopNode.Tui/**`
- `src/DesktopNode.Cli/**`
- `docs/USER_GUIDE.md`
- `docs/CLI_COMMAND_USAGE.md`
- `docs/USER_FEATURE_USAGE_SPEC.md`

소유하지 않는 것:

- Hyper-V provider internals
- Windows host mutation implementation
- Packaging artifact generation internals
- Public release boundary 변경

Implementation slices:

- Web/TUI/CLI command and screen vocabulary를 맞춘다.
- Login/session/RBAC journey를 표면별로 같은 개념으로 정리한다.
- VM lifecycle action labels, destructive guard, managed VM boundary를 맞춘다.
- Diagnostics bundle create/list/download, redaction, evidence handoff copy를 맞춘다.
- Update/rollback과 internal distribution copy를 user guide와 CLI/TUI/Web surface에
  같은 방식으로 반영한다.

Verification gates:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`
- `npm test --prefix web`
- `npm run verify:parity --prefix web`
- `npm run browser:fixture --prefix web`
- `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj`
- `dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj`

## Stabilization Gates

각 workstream은 분리 작업 전에 자기 기준선을 고정한다.

- `Runtime/Core`: 현재 route list, auth/session/RBAC behavior, job retention, diagnostics
  route contract, ops summary response contract를 snapshot으로 남긴다.
- `HyperV`: 현재 route별 read/mutation behavior, WMI provider boundary, structured
  failure code, installed smoke evidence link를 snapshot으로 남긴다.
- `Host Ops`: 현재 `service-action` 목록, mutation precondition, redaction, rollback
  diagnostics, explicit admin opt-in boundary를 snapshot으로 남긴다.
- `Packaging/Release`: 현재 MSI payload, product manifest, update/rollback, internal
  distribution matrix, publication descriptor, evidence artifact contract를 snapshot으로
  남긴다.
- `Operator Surfaces`: 현재 Web/TUI/CLI/user guide의 주요 operator journey와 용어를
  snapshot으로 남긴다.

첫 implementation wave는 behavior-preserving split이다. Behavior change가 필요한
경우 같은 PR 안에서 섞지 않고 별도 ADR 또는 follow-up implementation plan으로
분리한다.

## Migration Rules

1. Baseline freeze를 먼저 수행한다.
2. Contract tests before movement를 적용한다.
3. 기능 변경 없는 파일/모듈 분리를 첫 wave로 둔다.
4. Evidence는 삭제하지 않고 current, historical, supporting, closed-not-adopted로
   재분류한다.
5. README와 AGENTS는 evidence ledger가 아니라 entrypoint로 축소한다.
6. Host mutation은 계속 explicit admin opt-in evidence plan에서만 실행한다.
7. Public trusted signing, winget, external publication scope는 별도 ADR 없이는
   재개하지 않는다.
8. Workstream 간 write set은 implementation plan에서 분리해 충돌을 줄인다.
9. Archive와 historical spike reference는 product execution path로 되돌리지 않는다.
10. Operator UX alignment는 product scope 변경이 아니라 용어와 journey 정렬로
    제한한다.

## Plan Handoff

다음 단계는 `superpowers:writing-plans`로 implementation plan을 작성하는 것이다.
Plan은 한 번에 전체를 구현하지 않고 다음 다섯 개로 나눈다.

- `runtime-core-boundary-plan`
- `hyperv-domain-split-plan`
- `host-ops-domain-split-plan`
- `packaging-release-control-plane-plan`
- `operator-surfaces-alignment-plan`

각 plan은 다음 항목을 포함해야 한다.

- Baseline snapshot task
- Contract test or guard task
- Behavior-preserving split task
- Verification command
- Evidence/document update requirement
- Admin opt-in이 필요한지 여부
- 다른 workstream과 겹치는 file ownership

## Acceptance Criteria

이 PRD는 다음 조건을 만족하면 implementation plan handoff 준비가 끝난다.

- 5개 workstream의 goal, owned files/modules, non-goals, implementation slices,
  verification gates가 명확하다.
- Current baseline과 out-of-scope가 기존 ADR 결정과 충돌하지 않는다.
- Hyper-V, Host Ops, Runtime/Core의 첫 wave가 behavior-preserving split으로
  정의되어 있다.
- Packaging/Release가 public release reopening이 아니라 internal control plane
  cleanup으로 정의되어 있다.
- Operator Surfaces가 새 제품 scope가 아니라 Web/TUI/CLI/docs journey alignment로
  정의되어 있다.
- Evidence 삭제, public distribution 재개, host mutation 자동 실행이 모두 금지되어 있다.
- 이 문서에서 바로 다섯 개 implementation plan을 작성할 수 있다.

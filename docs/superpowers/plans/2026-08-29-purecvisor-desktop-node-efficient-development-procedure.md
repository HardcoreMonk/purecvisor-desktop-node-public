# PureCVisor Desktop Node 효율적 개발 절차 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (- [ ]) syntax for tracking.

**Goal:** 기존 Lane, 변경 등급, 검증, 승인, 중단 정책을 한 문서에서 실행할 수 있는 사람·에이전트 공통 개발 SOP와 회귀 가드를 제공한다.

**Architecture:** docs/DEVELOPMENT_PROCEDURE.md는 기존 규범 문서를 대체하지 않는 얇은 실행 라우터다. DesktopNode.Delivery.Tests의 문서 계약 테스트가 canonical owner 연결, Lane 순서, 승인 분리, 상태 분리와 인덱스 역링크를 고정한다.

**Tech Stack:** Markdown, C# 14, .NET 10, xUnit 2.9.3, DesktopNode.Verification, Node.js 24

---

## 실행 경계

- Design: docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md
- Lane: Lane 1 contract
- Change tier: M
- Verification lane: Full
- Product payload change: false
- Host, VM, service, installer mutation: false
- Current evidence write: false
- Trusted signing or external publication: false
- Git commit, push, PR은 각각 별도 승인 후에만 실행한다.

이 계획은 문서, 문서 계약 테스트와 navigation만 다룬다. package candidate, 설치본 probe,
Hyper-V mutation 또는 Lane 3 promotion을 열지 않는다.

## 파일 구조

| 파일 | 책임 |
| --- | --- |
| docs/DEVELOPMENT_PROCEDURE.md | 사람과 에이전트가 함께 쓰는 단일 실행 진입점 |
| src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs | 실행 절차의 canonical link, Lane, 승인, 상태 및 navigation 회귀 가드 |
| docs/DEVELOPER_INDEX.md | 개발 상황별 첫 진입점 연결 |
| docs/DOCUMENTATION_INDEX.md | 핵심 문서, 구현 계획, 설계 명세 카탈로그 연결 |
| docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md | Lane 의미의 canonical 설계에서 실행 SOP로 역링크 |
| docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md | 구현 완료 상태와 구현 계획 연결 |

### Task 1: 문서 계약 회귀 테스트를 RED로 고정

**Files:**
- Create: src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs
- Test: src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj

- [x] **Step 1: 다음 테스트 파일을 생성한다**

~~~csharp
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class EfficientDevelopmentProcedureDocumentationTests
{
    private static readonly RepositoryContractContext Repository =
        RepositoryContractContext.Find();

    [Fact]
    public void ProcedureRoutesToCanonicalPolicyOwners()
    {
        var procedure = Repository.ReadUtf8Text("docs/DEVELOPMENT_PROCEDURE.md");

        Assert.Contains(
            "pcv-efficient-development-procedure-v1",
            procedure,
            StringComparison.Ordinal);
        foreach (var canonicalPath in new[]
        {
            "docs/PUBLIC_SOURCE_AUTHORITY.md",
            "docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md",
            "docs/DEVELOPMENT_VERIFICATION_POLICY.md",
            "docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md",
            "config/agent-execution-circuit-breaker.json",
            "config/development-verification-suites.json",
        })
        {
            Assert.Contains(canonicalPath, procedure, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ProcedureKeepsLaneApprovalAndStateBoundariesSeparate()
    {
        var procedure = Repository.ReadUtf8Text("docs/DEVELOPMENT_PROCEDURE.md");

        AssertOrdered(
            procedure,
            "## 1. 2분 시작",
            "## 2. 변경 등급 결정",
            "## 3. Lane 1 개발",
            "## 4. Git과 clean-HEAD 검증",
            "## 5. Lane 2 설치본·Actual VM probe",
            "## 6. Lane 3 operational promotion",
            "## 7. 중단 조건",
            "## 8. 종료 보고");
        Assert.Contains("설치·Hyper-V standing approval", procedure, StringComparison.Ordinal);
        Assert.Contains("Lane 3 권한으로 확대되지 않는다", procedure, StringComparison.Ordinal);
        Assert.Contains("package checkpoint 승인", procedure, StringComparison.Ordinal);
        Assert.Contains("Git commit 승인", procedure, StringComparison.Ordinal);
        Assert.Contains("push/PR 승인", procedure, StringComparison.Ordinal);
        Assert.Contains("public_trusted_signing=false", procedure, StringComparison.Ordinal);
        Assert.Contains("external_stable_publication=false", procedure, StringComparison.Ordinal);
        foreach (var state in new[]
        {
            "code_complete",
            "code_ready_operational_pending",
            "package_candidate",
            "installed_non_promoted_candidate",
            "operational_current",
            "promotion_complete",
        })
        {
            Assert.Contains(state, procedure, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DeveloperNavigationAndLaneDesignPointToTheProcedure()
    {
        var developerIndex = Repository.ReadUtf8Text("docs/DEVELOPER_INDEX.md");
        var documentationIndex = Repository.ReadUtf8Text("docs/DOCUMENTATION_INDEX.md");
        var laneDesign = Repository.ReadUtf8Text(
            "docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md");
        var procedureDesign = Repository.ReadUtf8Text(
            "docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md");

        Assert.Contains(
            "| 개발 작업 시작·분류·Lane·승인 절차 | `docs/DEVELOPMENT_PROCEDURE.md` |",
            developerIndex,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "| 개발 가속 고정 기준 확인 | `docs/DEVELOPMENT_VERIFICATION_POLICY.md` |",
            developerIndex,
            StringComparison.Ordinal);
        AssertOrdered(
            documentationIndex,
            "- 기여자 온보딩: `docs/DEVELOPMENT_PROCEDURE.md`, `docs/DEVELOPER_INDEX.md`, `docs/CODING_GUIDE.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`",
            "## 핵심 문서",
            "- [docs/DEVELOPMENT_PROCEDURE.md](DEVELOPMENT_PROCEDURE.md)",
            "## 구현 계획",
            "- [docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md](superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md)",
            "## 설계 명세",
            "- [docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md](superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md)");
        AssertOrdered(
            laneDesign,
            "> 작업 시작 체크리스트, 검증 명령, 승인표와 종료 보고 형식은",
            "> `docs/DEVELOPMENT_PROCEDURE.md`에서 시작한다. 이 설계는 Lane 의미와 금지 조건의 canonical",
            "> owner로 유지된다.",
            "## 1. 목적");
        Assert.Contains("**Status:** Implemented", procedureDesign, StringComparison.Ordinal);
        Assert.Contains(
            "**Implementation plan:** `docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md`",
            procedureDesign,
            StringComparison.Ordinal);
        Assert.Contains(
            "**Contract:** `pcv-efficient-development-procedure-v1`",
            procedureDesign,
            StringComparison.Ordinal);
    }

    private static void AssertOrdered(string source, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            Assert.True(index >= offset, $"Missing or out-of-order procedure token: {token}");
            offset = index + token.Length;
        }
    }
}
~~~

- [x] **Step 2: focused test를 실행해 문서 부재 실패를 확인한다**

Run:

~~~powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter "FullyQualifiedName~EfficientDevelopmentProcedureDocumentationTests" --nologo
~~~

Expected: FAIL. 첫 테스트가 docs/DEVELOPMENT_PROCEDURE.md 부재로
PCV_DELIVERY_PATH_INVALID|missing을 보고해야 한다. 컴파일 오류가 나면 테스트 코드부터 수정하고
문서 구현으로 넘어가지 않는다.

### Task 2: 단일 개발 실행 진입점 작성

**Files:**
- Create: docs/DEVELOPMENT_PROCEDURE.md
- Test: src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs

- [x] **Step 1: 다음 내용으로 실행 SOP를 생성한다**

~~~markdown
# Desktop Node 개발 실행 절차

- Contract: pcv-efficient-development-procedure-v1
- Audience: human developers and Codex/agentic workers
- Procedure operator-surface scope: Web Console and PCVCLI; TUI steps are intentionally absent.
- Authority granted by this document: product/host mutation=false; public_trusted_signing=false; external_stable_publication=false.
- Current operational state lookup: docs/ga-ready/current-evidence.json.

이 문서는 개발 작업을 시작할 때 사용하는 실행 라우터다. 세부 규칙이나 현재 operational
수치를 독립적으로 소유하지 않는다. 충돌하면 다음 canonical owner를 우선한다.

| 판단 | Canonical owner |
| --- | --- |
| 공개 소스 권위 | docs/PUBLIC_SOURCE_AUTHORITY.md |
| 변경 S/M/L 등급 | docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md |
| Fast/Full/Release 검증 | docs/DEVELOPMENT_VERIFICATION_POLICY.md |
| checkpoint와 중단 규칙 | docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md |
| 회로 차단기 기계 계약 | config/agent-execution-circuit-breaker.json |
| suite와 required shard | config/development-verification-suites.json |
| Lane 0~3 의미 | docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md |

기본 흐름은 다음과 같다.

    Lane 0 권위 확인
      -> S/M/L 변경 분류
      -> Lane 1 설계·TDD·repo-local 검증
      -> Git commit 승인
      -> clean committed HEAD 네 shard 검증
      -> Git push/PR 승인
      -> 필요한 경우 Lane 2 설치본 probe
      -> 모든 PASS와 별도 승인 후 Lane 3 promotion
      -> 상태와 잔여 작업 보고

한 checkpoint는 정확히 하나의 lane만 소유한다.

## 1. 2분 시작

### 1.1 저장소와 Git 상태를 읽는다

    git status --short --branch
    git rev-parse HEAD
    git rev-parse origin/main

권위 operational 상태 읽기 예:

    $ledger = Get-Content -Raw -LiteralPath 'docs/ga-ready/current-evidence.json' | ConvertFrom-Json
    $ledger_current = $ledger.current.version
    $manifestPath = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
    if (Test-Path -LiteralPath $manifestPath) {
        $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
        $installed_current = $manifest.version
    } else {
        $installed_current = 'absent'
    }

읽기 또는 파싱 오류는 버전으로 추정하거나 조용히 변환하지 않고 오류로 기록한다. 명령을
실행하지 않은 경우 해당 값은 `not-read`로 기록한다.

권위 저장소는 HardcoreMonk/purecvisor-desktop-node-public이다. origin/main은 로컬 tracking
ref이므로 원격 freshness가 필요한 작업에서는 승인된 fetch 결과와 함께 기록한다. local HEAD가
origin/main보다 앞서 있어도 remote-integrated로 표현하지 않는다.

### 1.2 세 권위를 분리한다

- ledger_current: docs/ga-ready/current-evidence.json이 선언하는 operational 버전
- installed_current: 실제 호스트 product-manifest.json의 설치 버전
- source_head: 이번 코드 판단에 쓰는 checkout SHA

세 값이 다른 것은 상태다. 하나를 다른 값으로 추정하는 것이 오류다.

### 1.3 작업 카드를 작성한다

    repository_authority:
    origin_main:
    local_head:
    working_branch:
    working_tree_state:
    ledger_current:
    installed_current:
    source_head:
    change_intent:
    lane:
    requested_change_tier:
    effective_change_tier:
    effective_verification_lane:
    time_budget:
    tool_batch_budget:
    approval_profile:
    approval_locator:
    approval_repository:
    approval_host:
    approval_feature_family:
    approval_checkpoint:
    artifact_root:
    mutation_allowed:
    current_write_allowed:

기본 Lane 1 예산은 회로 차단기 canonical owner에서 읽는다. 예산이나 checkpoint 확대는
사용자의 명시적 승인 없이는 수행하지 않는다.

## 2. 변경 등급 결정

변경할 실제 경로를 먼저 나열한 뒤 자동 최소 등급을 적용한다.

| 등급 | 대표 범위 | 최소 검증 |
| --- | --- | --- |
| S | 단일 내부 모듈의 비계약 구현·테스트 | Fast |
| M | API/CLI/Web 계약, 일반 패키징, 교차 모듈 | Full |
| L | 보안, installer lifecycle, host mutation, current evidence, 공개 배포 | Release |

분류되지 않은 경로는 근거 없이 L로 부르지 않되 검증 lane은 Full로 올린다. 호출자가 요청한
등급보다 Resolve-PcvDevelopmentChangeTier가 계산한 등급이 높으면 계산 결과를 사용한다.
Release는 비변경 preflight이며 설치나 host mutation 권한이 아니다.

분류 projection 예:

    dotnet run --project src/DesktopNode.Verification -c Release -- verify --lane Fast --change-tier S --changed-path src/DesktopNode.Core/InternalHelper.cs --artifact-root artifacts/development-verification-fast --plan-only

이 --plan-only 사용은 변경 등급과 suite 선택을 확인하기 위한 것이다. 실제 검증 PASS로
보고하지 않는다.

## 3. Lane 1 개발

Lane 1은 모든 소스와 문서 변경의 기본값이다.

1. 변경 이유와 단일 owner를 한 문장으로 고정한다.
2. 실패하는 focused test 또는 계약 재현을 먼저 만든다.
3. 최소 구현으로 GREEN을 만든다.
4. focused test를 다시 실행한다.
5. 필요한 범위만 리팩터링한다.
6. diff에서 생성물, 비밀, 무관 변경을 확인한다.

owner 이동과 동작 변경은 한 변경에 섞지 않는다. API 오류, 인증, cancellation/lifetime,
HTTP/TLS, WMI 의미 변경은 국소 S 변경으로 낮추지 않는다.

dirty tree 준비 검증:

    dotnet restore src/DesktopNode.sln
    dotnet build src/DesktopNode.sln -c Release --no-restore
    npm ci --prefix web
    npm run test:required --prefix web
    git diff --check

focused test 결과를 whole-solution PASS 또는 clean-HEAD policy PASS로 표현하지 않는다.
Lane 1에서는 MSI 설치, service, firewall, trust store, Event Log, Hyper-V VM 및 current evidence
mutation을 수행하지 않는다.

## 4. Git과 clean-HEAD 검증

소스 변경 승인, Git commit 승인, push/PR 승인은 서로 다르다. 각 승인을 합쳐 추정하지 않는다.
commit 승인 후 clean committed HEAD에서 다음을 확인한다.

| 작업 | 필요한 승인 |
| --- | --- |
| 읽기, 분석, repo-local focused test | 선언된 Lane 0/1 범위 |
| 승인된 설계에 따른 소스 편집 | 현재 source checkpoint |
| Git commit | Git commit 승인 |
| push 또는 PR 생성·갱신 | push/PR 승인 |
| package candidate 생성 | package checkpoint 승인 |
| 설치 및 Hyper-V mutation | Lane 2 승인 또는 유효한 standing approval |
| current evidence 쓰기 | Lane 3 별도 승인 |
| trusted signing 및 외부 publication | 공개 배포 별도 승인 |

    git status --short

Expected: 출력 없음.

그 다음 required shard 네 개를 모두 실행한다.

    dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/local-dotnet --shard dotnet
    dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/local-web --shard web
    dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/local-delivery --shard delivery
    dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/local-installer-policy --shard installer-policy

위 .ps1 값은 changed-path 선택 데이터이며 PowerShell 실행 요청이 아니다. --plan-only는 suite
선택 확인일 뿐 PASS 근거가 아니다. 네 shard PASS 뒤에도 push와 PR은 별도 승인을 요구한다.

## 5. Lane 2 설치본·Actual VM probe

Lane 2는 operational promotion이 아니라 제한된 설치본 검증이다.

진입 조건:

- 기능군 하나
- artifact root 하나
- VM root 하나
- 설치본 버전과 provider 기록
- 관리자 opt-in 또는 현재 checkpoint에 유효한 설치·Hyper-V standing approval

standing approval의 모든 값(approval_locator, approval_repository, approval_host,
approval_feature_family, approval_checkpoint)은 현재 저장소·호스트·기능군·checkpoint와 각각
존재하고 일치해야 유효하다. 값이 없거나 하나라도 불일치하면 mutation_allowed=false이며
명시적인 Lane 2 승인이 필요하다. standing approval은 선언된 저장소, 호스트, 기능군과 checkpoint에만 적용된다. 예산 연장,
다른 기능군, SavedOnly 이후 Full mode, current evidence 쓰기 또는 Lane 3 권한으로 확대되지 않는다.

pre-state, mutation, readback, cleanup, failure를 분리해 기록한다. DryRun은 actual PASS가 아니다.
SavedOnly에서 Full로 넓히려면 새 checkpoint가 필요하다. 최초 하위 PCV_* 오류를 보존한다.

Lane 2 FAIL은 actual_vm_tested=pass, promotion eligibility 또는 current evidence 갱신의 근거가
될 수 없다. PASS하더라도 상태는 installed_non_promoted_candidate다.

## 6. Lane 3 operational promotion

Lane 3는 대상 변경에 필요한 package, full admin host mutation, manual-admin pair, installed
operator surface current-card와 feature qualification이 모두 PASS이고 별도 승인이 있을 때만
연다.

- current evidence에는 PASS 근거만 기록한다.
- probe FAIL이나 부분 완료는 historical/probe evidence로 보존한다.
- 설치본 버전만 보고 ledger current를 올리지 않는다.
- promotion 완료를 trusted signing이나 외부 stable publication으로 표현하지 않는다.

## 7. 중단 조건

다음 중 먼저 도달한 조건에서 구현을 중단하고 stop protocol만 수행한다.

- canonical checkpoint 시간 또는 도구 작업 예산 소진
- 동일 원인 3회 실패
- 승인 밖 mutation 필요
- 권위 저장소, 대상 경로 또는 cleanup 경계를 안전하게 확정할 수 없음

중단 순서:

1. 실행 중 자원을 안전하게 cleanup한다.
2. 최초 하위 PCV_* 오류와 재현 조건을 보존한다.
3. 수행된 mutation과 cleanup 결과를 기록한다.
4. 완료, 잔여, 범위 밖 발견을 나눈다.
5. 새 권한 또는 새 checkpoint가 필요한 이유를 보고한다.

범위 밖 발견은 report-only다.

상태 이름은 다음 순서를 유지한다.

1. code_complete
2. code_ready_operational_pending
3. package_candidate
4. installed_non_promoted_candidate
5. operational_current
6. promotion_complete

변경은 적용 가능한 가장 낮은 상태에 머물 수 있으며 모든 상태를 반드시 순회할 필요는 없다.
각 상태 전이는 이전 상태의 PASS evidence와 approval locator를 요구한다. 하위 상태를
건너뛰거나 code_complete를 operational_current로 축약하지 않는다.

## 8. 종료 보고

    lane:
    repository_authority:
    origin_main:
    local_head:
    source_head:
    ledger_current:
    installed_current:
    requested_change_tier:
    effective_change_tier:
    effective_verification_lane:
    budget_used:
    completed:
    verification_run:
    verification_not_run:
    remaining:
    out_of_scope:
    host_or_vm_mutation_performed:
    cleanup_result:
    current_evidence_written:
    resulting_state:
    next_approval_required:

성공 보고도 실행하지 않은 검증을 명시한다. current evidence를 쓰지 않았다면 false로 기록하고,
package나 설치본 상태를 operational current로 표현하지 않는다.
~~~

- [x] **Step 2: canonical owner와 Lane 경계 테스트 두 개를 실행한다**

Run:

~~~powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter "FullyQualifiedName~EfficientDevelopmentProcedureDocumentationTests.ProcedureRoutesToCanonicalPolicyOwners|FullyQualifiedName~EfficientDevelopmentProcedureDocumentationTests.ProcedureKeepsLaneApprovalAndStateBoundariesSeparate" --nologo
~~~

Expected: PASS 2, FAIL 0.

### Task 3: 개발 navigation과 기존 Lane 설계 연결

**Files:**
- Modify: docs/DEVELOPER_INDEX.md:475-496
- Modify: docs/DOCUMENTATION_INDEX.md:14-50, 730-738, 818-823
- Modify: docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md:15-17
- Modify: docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md:3-7
- Test: src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs

- [x] **Step 1: DEVELOPER_INDEX의 중복 가속 행을 실행 진입점으로 바꾼다**

docs/DEVELOPER_INDEX.md의 다음 행:

~~~markdown
| 개발 가속 고정 기준 확인 | docs/DEVELOPMENT_VERIFICATION_POLICY.md |
~~~

을 다음 행으로 교체한다.

~~~markdown
| 개발 작업 시작·분류·Lane·승인 절차 | docs/DEVELOPMENT_PROCEDURE.md |
~~~

바로 위의 검증 기준 행은 그대로 둔다.

- [x] **Step 2: DOCUMENTATION_INDEX에 procedure, plan, spec을 연결한다**

관례적 진입점 매핑의 기여자 온보딩 행을 다음으로 교체한다.

~~~markdown
- 기여자 온보딩: docs/DEVELOPMENT_PROCEDURE.md, docs/DEVELOPER_INDEX.md, docs/CODING_GUIDE.md, docs/DEVELOPMENT_VERIFICATION_POLICY.md
~~~

핵심 문서 목록에서 DEVELOPMENT_CHANGE_CLASSIFICATION과 DEVELOPMENT_VERIFICATION_POLICY 사이에
다음 행을 추가한다.

~~~markdown
- [docs/DEVELOPMENT_PROCEDURE.md](DEVELOPMENT_PROCEDURE.md)
~~~

구현 계획 목록의 2026-08-27 항목들 뒤에 다음 행을 추가한다.

~~~markdown
- [docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md](superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md)
~~~

설계 명세 목록의 2026-08-27 항목들 뒤에 다음 행을 추가한다.

~~~markdown
- [docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md](superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md)
~~~

- [x] **Step 3: Lane canonical 설계에 실행 진입점을 역링크한다**

docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md의
metadata 다음, 1. 목적 앞에 다음 문단을 추가한다.

~~~markdown
> 작업 시작 체크리스트, 검증 명령, 승인표와 종료 보고 형식은
> docs/DEVELOPMENT_PROCEDURE.md에서 시작한다. 이 설계는 Lane 의미와 금지 조건의 canonical
> owner로 유지된다.
~~~

- [x] **Step 4: 2026-08-29 설계 문서의 metadata를 구현 상태로 전환한다**

설계 문서 상단의 Status를 구현 완료로 바꾸고, Proposed contract를 Contract로 전환하며,
구현 계획을 다음 정확한 세 행으로 기록한다.

~~~markdown
**Status:** Implemented
**Implementation plan:** `docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md`
**Contract:** `pcv-efficient-development-procedure-v1`
~~~

- [x] **Step 5: 전체 focused 문서 계약 테스트를 실행한다**

Run:

~~~powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter "FullyQualifiedName~EfficientDevelopmentProcedureDocumentationTests" --nologo
~~~

Expected: PASS 3, FAIL 0.

### Task 4: Lane 1 준비 검증과 clean-HEAD Full 검증

**Files:**
- Verify only: 이번 계획에서 생성·수정한 파일 전체

- [x] **Step 1: 솔루션을 복원하고 Release로 빌드한다**

Run:

~~~powershell
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
~~~

Expected: restore 성공, build exit 0, compilation error 0.

- [x] **Step 2: Web required 검증을 실행한다**

Run:

~~~powershell
npm ci --prefix web
npm run test:required --prefix web
~~~

Expected: 두 명령 모두 exit 0.

- [x] **Step 3: focused test와 public source safety를 재실행한다**

Run:

~~~powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-build --no-restore --filter "FullyQualifiedName~EfficientDevelopmentProcedureDocumentationTests" --nologo
npm run test:public-source-safety --prefix web
npm run verify:public-source-safety --prefix web
git diff --check
~~~

Expected: focused PASS 3, Node 명령 exit 0, git diff check 출력 없음.

이 시점의 public-source-safety 결과는 `tracked baseline PASS; four untracked candidate files not covered by the repository script`다. candidate-wide 공식 safety 검증은 stage 뒤, commit 전의 필수 gate로 다시 실행한다.

Delivery baseline FAIL: PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|module-ratchet-exceeded; DesktopNodeApiVmMutationRouteHandler.cs 989 > 970; observed before this implementation; out-of-scope; user approved scoped continuation; never report as Full Delivery or unqualified Full-lane PASS.

- [x] **Step 4: 변경 범위를 검토한다**

Run:

~~~powershell
git status --short
git diff -- docs/DEVELOPMENT_PROCEDURE.md docs/DEVELOPER_INDEX.md docs/DOCUMENTATION_INDEX.md docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs
git diff --no-index -- NUL docs/DEVELOPMENT_PROCEDURE.md
git diff --no-index -- NUL docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md
git diff --no-index -- NUL docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md
git diff --no-index -- NUL src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs
~~~

Expected: 위 일곱 파일만 이번 절차 변경으로 표시된다. 기존 사용자 변경이 보이면 포함하거나
되돌리지 않고 분리해 보고한다. tracked file의 `git diff`는 exit 0을 기대한다. 새 파일의
각 `git diff --no-index -- NUL <file>`은 내용이 있으면 exit 1이 정상이며, 모든 added line을
검토한다. 다른 exit code는 실패다.

- [x] **Step 5: 별도 Git 승인을 받은 뒤 정확히 일곱 파일을 stage한다**

Run:

~~~powershell
git add docs/DEVELOPMENT_PROCEDURE.md docs/DEVELOPER_INDEX.md docs/DOCUMENTATION_INDEX.md docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs
~~~

Expected: 별도 Git 승인 후에만 정확히 일곱 파일을 stage한다. 다른 파일을 stage하지 않는다.

- [x] **Step 6: stage된 후보 전체를 fail-closed로 검토하고 candidate-wide safety를 실행한다**

Run:

~~~powershell
git diff --cached --check
git diff --cached -- docs/DEVELOPMENT_PROCEDURE.md docs/DEVELOPER_INDEX.md docs/DOCUMENTATION_INDEX.md docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md docs/superpowers/specs/2026-08-29-purecvisor-desktop-node-efficient-development-procedure-design.md docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-efficient-development-procedure.md src/DesktopNode.Delivery.Tests/Delivery/Verification/EfficientDevelopmentProcedureDocumentationTests.cs
git diff --cached --name-only
npm run test:public-source-safety --prefix web
npm run verify:public-source-safety --prefix web
~~~

Expected: cached diff check 출력 없음, cached diff에는 정확히 일곱 파일만 보이고
`git diff --cached --name-only`도 정확히 일곱 경로만 출력한다. stage하면 네 새 파일이
`git ls-files`에 포함되므로 여기의 public-source-safety가 candidate-wide 공식 gate다. 하나라도
실패하면 commit하지 말고 실패를 보고한다. destructive reset을 처방하거나 실행하지 않는다.

- [x] **Step 7: 모든 stage 검토가 PASS한 뒤에만 commit한다**

Run:

~~~powershell
git commit -m "docs: add unified development procedure"
~~~

Expected: commit exit 0. 승인 없이는 이 step을 실행하지 않는다.

- [ ] **Step 8: clean committed HEAD를 확인한다**

Run:

~~~powershell
git status --short
~~~

Expected: 출력 없음.

- [ ] **Step 9: required shard 네 개를 모두 실행한다**

Run:

~~~powershell
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-procedure-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/development-procedure-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/development-procedure-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/development-procedure-installer-policy --shard installer-policy
~~~

Expected: 각 summary의 ok=true, failed_suite=null, process exit 0.

### Task 5: 종료 판정과 인계

**Files:**
- Read only: Git 상태와 artifacts/development-procedure-* summary

- [ ] **Step 1: 결과를 상태 모델로 판정한다**

이 변경은 문서와 문서 계약 테스트만 변경하므로 네 shard PASS 후 결과 상태는
code_complete다. package_candidate, installed_non_promoted_candidate, operational_current 또는
promotion_complete로 올리지 않는다.

- [ ] **Step 2: 종료 보고를 작성한다**

다음 값을 실제 결과로 채워 사용자에게 보고한다.

~~~text
lane: Lane 1
repository_authority: HardcoreMonk/purecvisor-desktop-node-public
origin_main: 실행 시 측정 SHA
local_head: 실행 시 측정 SHA
source_head: 검증한 commit SHA
ledger_current: 실행 시 current-evidence 값
installed_current: not-read
requested_change_tier: M
effective_change_tier: M 또는 resolver 실측값
effective_verification_lane: Full
budget_used: 실제 시간과 도구 묶음
completed: SOP, navigation, regression guard
verification_run: focused, Web required, public source safety, four required shards
verification_not_run: Lane 2, Lane 3, package, host/VM mutation
remaining: push/PR 또는 없음
out_of_scope: 별도 발견
host_or_vm_mutation_performed: false
cleanup_result: no host/VM resources created
current_evidence_written: false
resulting_state: code_complete
next_approval_required: push/PR 승인 또는 없음
~~~

- [ ] **Step 3: push 또는 PR이 요청되면 별도 승인을 확인한다**

Git push와 PR 생성은 이 계획의 코드·문서 구현 승인에 포함되지 않는다. 사용자가 명시적으로
요청한 경우에만 보호 브랜치 정책과 exact four contexts를 확인하며 진행한다.

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

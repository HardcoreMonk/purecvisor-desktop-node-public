using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

// The Lane 3 promotion document tools run from PowerShell and their behavior suites live in
// packaging/windows-desktop-node/manual-admin-tests (outside the frozen Pester inventory). Required
// CI does not run PowerShell, so this class pins the source contracts those suites rely on.
[Trait("Category", "Delivery")]
public sealed class PcvLane3PromotionDocsToolsContractTests
{
    private const string ToolRoot = "packaging/windows-desktop-node/tools/";
    private const string DescriptorChainTool = ToolRoot + "Update-PcvManualAdminDescriptorChain.ps1";
    private const string LedgerRowsTool = ToolRoot + "Update-PcvCurrentEvidenceLedgerRows.ps1";
    private const string IndexSectionsTool = ToolRoot + "New-PcvPromotionIndexSections.ps1";
    private const string SpecPinsTool = ToolRoot + "Update-PcvContractSpecPins.ps1";
    private const string OrchestratorTool = ToolRoot + "Invoke-PcvLane3PromotionDocs.ps1";
    private const string ExampleSpec =
        "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04278.json";

    public static TheoryData<string> Tools =>
    [
        DescriptorChainTool,
        LedgerRowsTool,
        IndexSectionsTool,
        SpecPinsTool,
        OrchestratorTool,
    ];

    [Theory]
    [MemberData(nameof(Tools))]
    public void EveryToolHasDryRunApplyAndCheckModesAndDoesNotTouchTheHost(string tool)
    {
        var source = Source(tool);

        RequireTokens(
            source,
            "Set-StrictMode -Version Latest",
            "[switch]$Apply",
            "[switch]$Check",
            "apply-and-check-are-exclusive",
            "schema_version = 1",
            "'dry-run'");
        foreach (var forbidden in new[]
        {
            "msiexec", "Set-Service", "Start-Service", "Stop-Service", "New-VM", "Remove-VM",
            "Restart-Computer", "NetFirewallRule", "Invoke-WebRequest", "git push",
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void SpecDrivenToolsReadSpecsWithJsonDocumentAndNameTheirContracts()
    {
        foreach (var (tool, contract) in new[]
        {
            (DescriptorChainTool, "pcv-manual-admin-descriptor-chain-rotation-v1"),
            (LedgerRowsTool, "pcv-current-evidence-ledger-rows-rotation-v1"),
            (IndexSectionsTool, "pcv-promotion-index-sections-v1"),
            (OrchestratorTool, "pcv-lane3-promotion-docs-v1"),
        })
        {
            RequireTokens(Source(tool), "[System.Text.Json.JsonDocument]::Parse(", $"'{contract}'");
        }
    }

    [Fact]
    public void LedgerRowsRotateOnlyTheAnchorTableAndRefuseASecondApplication()
    {
        RequireTokens(
            Source(LedgerRowsTool),
            "'| ledger key | current 상태 | Evidence | 운영 규칙 |'",
            "predecessor after $PromotedTag promotion",
            "'^`artifacts/batch-runs/([^`/]+)`$'",
            "predecessor-rule-required",
            "current-row-count:",
            "already-rotated",
            "PCV_LEDGER_ROWS_STALE|");
    }

    [Fact]
    public void IndexSectionsTakeValuesFromCurrentEvidenceAndRefuseDisagreement()
    {
        var source = Source(IndexSectionsTool);

        RequireTokens(
            source,
            "'current.version'",
            "'manual_admin.latest_closed_descriptor'",
            "'not-eligible'",
            "not-current:",
            "same-as-current",
            "previous-heading-mismatch:",
            "missing-closure-line",
            "' predecessor promotion'",
            "'# Desktop Node 증거 인덱스'",
            "PCV_PROMOTION_INDEX_STALE|");
        AssertOrdered(source, "operational current promotion", "descriptor consume");
    }

    [Fact]
    public void SpecPinsHashTextWithoutBomAndKeepStructuredTransitionExemptions()
    {
        RequireTokens(
            Source(SpecPinsTool),
            "'pcv-*-contract-spec-v1.json'",
            "const string SpecPath =",
            "ExpectedSpecSha256 =",
            "SpecPath,",
            "StructuredTransitionSources = new",
            "exempt_stale",
            "[System.Text.UTF8Encoding]::new($false, $true)",
            "spec-sha-literal-count:",
            "PCV_SPEC_PINS_STALE|");
    }

    [Fact]
    public void OrchestratorRunsTheStepsInPromotionOrderAfterADryRunPreflight()
    {
        var source = Source(OrchestratorTool);

        AssertOrdered(
            source,
            "name = 'current_evidence_docs'",
            "name = 'descriptor_chain'",
            "name = 'ledger_head'",
            "'-DescriptorPath', 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'",
            "name = 'ledger_rows'",
            "name = 'index_sections'",
            "name = 'spec_pins'");
        RequireTokens(source, "GetRawText()", "preflight.", "PCV_[A-Z_]+_STALE");
        AssertOrdered(source, "-Mode 'dry-run'", "foreach ($step in $script:PcvLane3Steps)");
    }

    [Fact]
    public void ProcedureDocumentPointsToTheOrchestratorAndTheExampleSpec()
    {
        var repository = RepositoryContractContext.Find();
        var procedure = repository.ReadUtf8Text("docs/DEVELOPMENT_PROCEDURE.md");
        var example = repository.ReadUtf8Text(ExampleSpec);

        RequireTokens(procedure, OrchestratorTool, ExampleSpec);
        RequireTokens(
            example,
            "\"contract\": \"pcv-lane3-promotion-docs-v1\"",
            "\"descriptor_chain\":",
            "\"ledger_head\":",
            "\"ledger_rows\":",
            "\"index_sections\":");
    }

    private static string Source(string path) =>
        RepositoryContractContext.Find().ReadUtf8Text(path);

    private static void RequireTokens(string source, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            Assert.Contains(token, source, StringComparison.Ordinal);
        }
    }

    private static void AssertOrdered(string source, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Missing or out-of-order source token: {token}");
            offset = index + token.Length;
        }
    }
}

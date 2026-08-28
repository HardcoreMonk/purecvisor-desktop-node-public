using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP1CloneActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1";

    [Fact]
    public void PublishesValidatedInputsAndStrictDryRunBoundary()
    {
        var source = Source();
        RequireTokens(
            source,
            "[Parameter(Mandatory)]",
            "[string]$Version",
            "$ArtifactRoot",
            "$ProductRoot",
            "$IsoPath",
            "$VmRoot",
            "$SourceVm",
            "$TargetVm",
            "$JobTimeoutSeconds",
            "$CommandTimeoutSeconds",
            "$DryRun",
            "$RuntimeAdapter",
            "$SummaryWriter",
            "Invoke-RuntimeOperation",
            "PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH",
            "PCV_P1_CLONE_VM_NAME_INVALID",
            "dry-run-no-installed-cli-or-hyperv",
            "artifact_root_resolved",
            "vm_root_resolved");
        Assert.DoesNotContain("[ValidateSet('SavedOnly', 'Full')]", source, StringComparison.Ordinal);
        AssertOrdered(source, "if ($DryRun.IsPresent)", "Assert-InstalledProduct");
    }

    [Fact]
    public void PinsCloneFamilySliceOrderAndOperatorIds()
    {
        var source = Source();
        RequireTokens(
            source,
            "'source_create'",
            "'preview_mismatch'",
            "'preview_ok'",
            "'clone_ok'",
            "'cleanup'",
            "'vm', 'create'",
            "'--disk-gb', '8'",
            "'vm', 'clone'",
            "'--dry-run'",
            "'--yes'",
            "PCV_CLI_CONFIRMATION_REQUIRED",
            "'vm', 'get', $OperatorId",
            "'vm', 'delete', $record.name",
            "'vm', 'clone', $SourceVm",
            "'vm', 'clone', $SourceVm, '--name', $TargetVm, '--dry-run', '--vm-root', $vmRootFull",
            "'vm', 'clone', $SourceVm, '--name', $TargetVm, '--yes', '--vm-root', $vmRootFull",
            "Assert-SlicePassed",
            "Invoke-TrackedSlice",
            "Test-PcvProductOff");
        AssertOrdered(
            source,
            "'source_create'",
            "'preview_mismatch'",
            "'preview_ok'",
            "'clone_ok'",
            "'cleanup'");
        Assert.DoesNotContain("'vm', 'get', $Id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'delete', $record.id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'clone', $record.id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'--disk-gb', '1'", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm-start'", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm-save'", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PinsSummaryAtomicityAndDoesNotWriteCurrentEvidence()
    {
        var source = Source();
        RequireTokens(
            source,
            "installed_cli_sha256",
            "queued_jobs",
            "cleanup",
            "host_mutation_performed",
            "secret_observed",
            "overall_verdict",
            "PCV_P1_CLONE_SUMMARY_WRITE_FAILED",
            "summary.json.tmp",
            "Move-Item -LiteralPath",
            "Get-CliProblemCode");
        Assert.DoesNotContain("docs/ga-ready/current-evidence.json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Update-PcvCurrentEvidence", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-VM -Name", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(
            new Regex(@"(?i)bearer\s+[A-Za-z0-9._~+/\-]+=*", RegexOptions.CultureInvariant),
            source);
    }

    private static string Source() =>
        RepositoryContractContext.Find().ReadUtf8Text(RunnerPath);

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

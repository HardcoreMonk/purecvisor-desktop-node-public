using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP0ActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1";

    [Fact]
    public void PublishesValidatedInputsAndStrictDryRunBoundary()
    {
        var source = Source();

        RequireTokens(
            source,
            "[Parameter(Mandatory)]",
            "[string]$Version",
            "[ValidateSet('SavedOnly', 'Full')]",
            "$ArtifactRoot",
            "$ProductRoot",
            "$IsoPath",
            "$VmRoot",
            "$ManagedVm",
            "$ForeignVm",
            "$CheckpointName",
            "$JobTimeoutSeconds",
            "$CommandTimeoutSeconds",
            "$DryRun",
            "PCV_P0_INSTALLED_VERSION_MISMATCH",
            "PCV_P0_VM_NAME_INVALID",
            "PCV_P0_VM_ALREADY_EXISTS",
            "dry-run-no-installed-cli-or-hyperv",
            "artifact_root_resolved",
            "vm_root_resolved");
        AssertOrdered(source, "if ($DryRun.IsPresent)", "Assert-InstalledProduct");
    }

    [Fact]
    public void PinsSavedLifecycleAndFullSlicePostconditions()
    {
        var source = Source();

        RequireTokens(
            source,
            "'vm-create'",
            "'vm-start'",
            "'vm-save'",
            "'vm-resume-saved'",
            "'Saved'",
            "'saved'",
            "'Paused'",
            "'Running'",
            "'media_attach'",
            "'checkpoint_restore'",
            "'saved_lifecycle'",
            "'managed_import'",
            "HostResource",
            "is_current",
            "PCV_VM_NOT_MANAGED_BY_PURECVISOR",
            "managed-by=purecvisor-desktop-node",
            "Assert-SlicePassed",
            "PCV_P0_STATE_MISMATCH",
            "PCV_P0_SERVICE_LOST");
        AssertOrdered(
            source,
            "'saved_lifecycle'",
            "'media_attach'",
            "'checkpoint_restore'",
            "'managed_import'");
    }

    [Fact]
    public void PinsSummaryAtomicityFailureSemanticsAndSecretRedaction()
    {
        var source = Source();

        RequireTokens(
            source,
            "installed_cli_sha256",
            "managed_vm_id",
            "foreign_vm_id",
            "queued_jobs",
            "hyperv_state_after_save",
            "product_state_after_save",
            "cleanup",
            "host_mutation_performed",
            "secret_observed",
            "started_at",
            "completed_at",
            "overall_verdict",
            "PCV_P0_SUMMARY_WRITE_FAILED",
            "summary.json.tmp",
            "Move-Item -LiteralPath");
        Assert.DoesNotMatch(
            new Regex(@"(?i)bearer\s+[A-Za-z0-9._~+/\-]+=*", RegexOptions.CultureInvariant),
            source);
    }

    [Fact]
    public void PinsExactRecordedIdentityAndDedicatedRootCleanup()
    {
        var source = Source();

        RequireTokens(
            source,
            "Get-VM -Id",
            "Remove-VM -VM $current",
            "Assert-ValidatedChildPath",
            "PCV_P0_CLEANUP_ID_MISMATCH",
            "PCV_P0_CLEANUP_ROOT_INVALID",
            "native_fallback_used",
            "same_name_different_id_blocked");
        Assert.DoesNotContain("Remove-VM -Name", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(
            new Regex(@"(?is)Get-VM\s*\|\s*Where-Object.*?Remove-VM", RegexOptions.CultureInvariant),
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

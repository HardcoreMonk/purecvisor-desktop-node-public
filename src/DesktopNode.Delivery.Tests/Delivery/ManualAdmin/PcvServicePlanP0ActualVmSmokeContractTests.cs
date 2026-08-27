using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Delivery.Evidence;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP0ActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1";

    private const string AdapterPath =
        "packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1";

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
            "$RuntimeAdapter",
            "$SummaryWriter",
            "Invoke-RuntimeOperation",
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
            "Invoke-TrackedSlice",
            "slice_verdicts[$Slice] = 'FAIL'",
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
            "PCV_P0_SECRET_OBSERVED",
            "Test-SecretMaterial",
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
            "PCV_P0_VM_ROOT_ALREADY_EXISTS",
            "PCV_P0_CLEANUP_IDENTITY_DRIFT",
            "native_fallback_used",
            "same_name_different_id_blocked",
            "#requires -Version 7.0",
            "Assert-PcvPathAbsent",
            "root_owned_by_run",
            "Get-ValidatedCleanupVm");
        RequireTokens(
            source,
            "initial_status",
            "polling_status",
            "timed_out",
            "'invoke-cli'",
            "$DeferTerminalSummaryWrite",
            "observed_id",
            "observed_path",
            "New-VmOwnershipRecord",
            "Set-VmAuthoritativeIdentity",
            "productStateAfterResume -ne 'running'");
        Assert.DoesNotContain("Remove-VM -Name", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(
            new Regex(@"(?is)Get-VM\s*\|\s*Where-Object.*?Remove-VM", RegexOptions.CultureInvariant),
            source);
    }

    [Fact]
    public void PinsCanonicalOperatorIdForProductGetAndDelete()
    {
        var source = Source();
        RequireTokens(
            source,
            "Get-ProductVmState",
            "-OperatorId",
            "$ManagedVm",
            "'vm', 'get', $OperatorId",
            "'vm', 'delete', $record.name");
        Assert.DoesNotContain(
            "'vm', 'get', $Id",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Get-ProductVmState -Id $Record.id",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "'vm', 'delete', $record.id",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void PinsInnerProblemCodeAheadOfGenericCommandFailure()
    {
        var source = Source();
        RequireTokens(
            source,
            "function Invoke-PcvCliJson",
            "error",
            "code",
            "stderr",
            "code=",
            "PCV_P0_COMMAND_FAILED");
        AssertOrdered(
            source,
            "function Invoke-PcvCliJson",
            "$cliErrorCode",
            "throw");
        Assert.Contains("PCV_P0_COMMAND_FAILED", source, StringComparison.Ordinal);
        Assert.Contains("Get-CliProblemCode", source, StringComparison.Ordinal);
        Assert.Contains("-Stderr", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Lane2RunnerDoesNotWriteCurrentEvidence()
    {
        var source = Source();
        Assert.DoesNotContain(
            "docs/ga-ready/current-evidence.json",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Update-PcvCurrentEvidence",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Lane2FailObservationDoesNotMakePromotionEligible()
    {
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 4);
    }

    [Fact]
    public void AdapterDoesNotKeepRetiredProductVmStateShortcut()
    {
        var adapter = RepositoryContractContext.Find().ReadUtf8Text(AdapterPath);
        Assert.DoesNotContain("'product-vm-state'", adapter, StringComparison.Ordinal);
        Assert.DoesNotContain("product-vm-state", Source(), StringComparison.Ordinal);
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

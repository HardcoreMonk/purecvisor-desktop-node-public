using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP2OffVmActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1";

    [Fact]
    public void PublishesValidatedInputsFamilySelectionAndStrictDryRunBoundary()
    {
        var source = Source();
        RequireTokens(
            source,
            "[Parameter(Mandatory)]",
            "[string]$Version",
            "[ValidateSet('device-add', 'checkpoint-schedule', 'export-import', 'network-connect')]",
            "$NetSwitchName",
            "[string]$Family",
            "$ArtifactRoot",
            "$ProductRoot",
            "$IsoPath",
            "$VmRoot",
            "$VmName",
            "$ImportVmName",
            "$SwitchName",
            "$JobTimeoutSeconds",
            "$CommandTimeoutSeconds",
            "$DryRun",
            "$RuntimeAdapter",
            "$SummaryWriter",
            "Invoke-RuntimeOperation",
            "PCV_P2_OFFVM_INSTALLED_VERSION_MISMATCH",
            "PCV_P2_OFFVM_VM_NAME_INVALID",
            "PCV_P2_OFFVM_SWITCH_NOT_FOUND",
            "dry-run-no-installed-cli-or-hyperv",
            "artifact_root_resolved",
            "vm_root_resolved",
            "nonclaims");
        AssertOrdered(source, "if ($DryRun.IsPresent)", "Assert-InstalledProduct");
    }

    [Fact]
    public void PinsFamilySliceOrderAndDisplayNameOperatorIds()
    {
        var source = Source();
        RequireTokens(
            source,
            "'vm', 'create'",
            "'--disk-gb', '8'",
            "'vm', 'device', 'add', $VmName, '--kind', 'nic', '--switch', $SwitchName)",
            "'vm', 'device', 'add', $VmName, '--kind', 'nic', '--switch', $SwitchName, '--yes'",
            "'vm', 'device', 'add', $VmName, '--kind', 'dvd', '--yes'",
            "'vm', 'checkpoint', 'schedule', 'preview', $VmName",
            "'vm', 'checkpoint', 'schedule', 'set', $VmName",
            "'vm', 'checkpoint', 'schedule', 'clear', $VmName, '--yes'",
            "'vm', 'get', $VmName",
            "'vm', 'delete', $record.name, '--yes'",
            "PCV_CLI_CONFIRMATION_REQUIRED",
            "PCV_VM_DEVICE_LIMIT",
            "PCV_VM_DEVICE_ALREADY_PRESENT",
            "PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID",
            "Assert-SlicePassed",
            "Invoke-TrackedSlice",
            "Test-PcvProductOff");
        AssertOrdered(
            source,
            "'device-add' = @(",
            "'source_create'",
            "'nic_confirm_required'",
            "'nic_add'",
            "'nic_limit'",
            "'dvd_guard'",
            "'cleanup'");
        AssertOrdered(
            source,
            "'checkpoint-schedule' = @(",
            "'source_create'",
            "'schedule_preview'",
            "'schedule_interval_invalid'",
            "'schedule_set'",
            "'schedule_clear'",
            "'cleanup'");
        AssertOrdered(
            source,
            "'export-import' = @(",
            "'source_create'",
            "'export_confirm_required'",
            "'export_path_not_allowed'",
            "'export_preview'",
            "'export'",
            "'import_preview'",
            "'import'",
            "'cleanup'");
        Assert.DoesNotContain("'vm', 'delete', $record.id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'get', $Id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'start'", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'--iso-path'", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PinsExportImportAllowlistRootAndImportBeforeSourceCleanup()
    {
        var source = Source();
        RequireTokens(
            source,
            "'vm', 'export', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull)",
            "'vm', 'export', 'preview', $VmName, '--directory', $outsideDirFull, '--allowed-root', $exportRootFull)",
            "'vm', 'export', 'preview', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull)",
            "'vm', 'export', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull, '--yes')",
            "'vm', 'import', 'preview', '--name', $ImportVmName, '--directory', $exportDirFull,",
            "'vm', 'import', '--name', $ImportVmName, '--directory', $exportDirFull,",
            "'vm', 'get', $ImportVmName",
            "PCV_VM_EXPORT_PATH_NOT_ALLOWED",
            "New-VmOwnershipRecord -Kind 'import' -Name $ImportVmName -ExpectedRoot $exportRootFull",
            "'vm-delete-import'",
            "vmgs_count");
        AssertOrdered(
            source,
            "$ordered = @($script:VmRecords | Where-Object { $_.kind -eq 'import' })",
            "@($script:VmRecords | Where-Object { $_.kind -ne 'import' })");
        Assert.DoesNotContain("Import-VM", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Export-VM", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PinsNetworkConnectRunOwnedPrivateSwitchLifecycle()
    {
        var source = Source();
        RequireTokens(
            source,
            "'service-action', $Action, '--product-root', $summary.product_root_resolved,",
            "'--service-exe', $script:PcvHost, '--switch-name', $NetSwitchName)",
            "'--switch-type', 'private'",
            "'vm', 'network', 'connect', $VmName, '--switch', $NetSwitchName)",
            "'vm', 'network', 'connect', $VmName, '--switch', $MissingSwitchName, '--yes')",
            "'vm', 'network', 'connect', $VmName, '--switch', $NetSwitchName, '--yes')",
            "PCV_NETWORK_SWITCH_NOT_FOUND",
            "PCV_P2_OFFVM_SWITCH_ALREADY_EXISTS",
            "PCV_P2_OFFVM_CLEANUP_SWITCH_ID_MISMATCH",
            "Msvm_VirtualEthernetSwitch");
        AssertOrdered(
            source,
            "'network-connect' = @(",
            "'source_create'",
            "'switch_create'",
            "'connect_confirm_required'",
            "'connect_switch_missing'",
            "'connect'",
            "'cleanup'");
        AssertOrdered(
            source,
            "function Invoke-ExactCleanup",
            "'vm', 'delete'",
            "Invoke-SwitchCleanup -CleanupErrors $cleanupErrors");
        Assert.DoesNotContain("New-VMSwitch", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-VMSwitch", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Connect-VMNetworkAdapter", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("'--switch-type', 'internal'", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'--switch-type', 'external'", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadsHyperVDevicesFromWmiInsteadOfProcessCachedCmdlets()
    {
        var source = Source();
        RequireTokens(
            source,
            "root\\virtualization\\v2",
            "Msvm_SyntheticEthernetPortSettingData",
            "Msvm_EthernetPortAllocationSettingData",
            "Microsoft:Hyper-V:Synthetic DVD Drive",
            "Msvm_SnapshotOfVirtualSystem",
            "hyperv-wmi-root-virtualization-v2");
        Assert.DoesNotContain("Get-VMNetworkAdapter", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Get-VMDvdDrive", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Get-VMSnapshot", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PinsSummaryAtomicityCleanupAndDoesNotWriteCurrentEvidence()
    {
        var source = Source();
        RequireTokens(
            source,
            "installed_cli_sha256",
            "queued_jobs",
            "schedule_cleared",
            "host_mutation_performed",
            "secret_observed",
            "overall_verdict",
            "PCV_P2_OFFVM_SUMMARY_WRITE_FAILED",
            "PCV_P2_OFFVM_CLEANUP_SCHEDULE_NOT_CLEARED",
            "summary.json.tmp",
            "Move-Item -LiteralPath",
            "Get-CliProblemCode");
        AssertOrdered(source, "function Invoke-ExactCleanup", "Invoke-ScheduleCleanup", "'vm', 'delete'");
        Assert.DoesNotContain("docs/ga-ready/current-evidence.json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Update-PcvCurrentEvidence", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-VM", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Add-VMNetworkAdapter", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Add-VMDvdDrive", source, StringComparison.OrdinalIgnoreCase);
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

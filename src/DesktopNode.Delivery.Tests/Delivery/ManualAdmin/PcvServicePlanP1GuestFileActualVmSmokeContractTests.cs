using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP1GuestFileActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1";

    [Fact]
    public void PublishesValidatedInputsPersistentTargetGuardAndDryRunBoundary()
    {
        var source = Source();
        RequireTokens(
            source,
            "[string]$Version",
            "$VmName = 'pcv-guest-installed-04253-r1'",
            "$CredentialRef",
            "$HostFilesRoot",
            "$GuestPrefix",
            "$DryRun",
            "$RuntimeAdapter",
            "persistent_policy=keep-until-next-evidence-cycle",
            "PCV_P1_GUESTFILE_VM_NOT_PERSISTENT_TARGET",
            "PCV_P1_GUESTFILE_INSTALLED_VERSION_MISMATCH",
            "PCV_P1_GUESTFILE_CREDENTIAL_REF_INVALID",
            "'^(dpapi|wincred):\\S+$'",
            "dry-run-no-installed-cli-or-hyperv");
        AssertOrdered(source, "if ($DryRun.IsPresent)", "function Assert-InstalledProduct");
    }

    [Fact]
    public void PinsSliceOrderRejectionsBeforeCopyAndGuestExecOnlyGuestAccess()
    {
        var source = Source();
        AssertOrdered(
            source,
            "'preflight', 'vm_start', 'channel_verify', 'guest_prefix_probe', 'host_staging',",
            "'preview_path_not_allowed', 'preview_size_limit', 'preview_ok', 'copy_confirm_required',",
            "'copy', 'guest_readback', 'cleanup')");
        RequireTokens(
            source,
            "'vm', 'start', $VmName",
            "'vm', 'guest-agent-ensure-channel', $VmName, '--verify', '--credential-ref', $CredentialRef,",
            "'vm', 'guest-exec', $VmName, '--credential-ref', $CredentialRef, '--timeout-sec', \"$GuestTimeoutSeconds\",",
            "'--', 'powershell.exe', '-NoProfile', '-NonInteractive', '-Command', $Script",
            "'vm', 'guest-file', $VmName, '--host-path', $payloadPath, '--guest-path', $guestFilePath,",
            "PCV_GUEST_FILE_PATH_NOT_ALLOWED",
            "PCV_GUEST_FILE_SIZE_LIMIT",
            "Get-FileHash -Algorithm SHA256 -LiteralPath",
            "hash_matches_payload",
            "guest_prefix_present_before");
        Assert.DoesNotContain("Invoke-Command -VMName", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Copy-VMFile", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-PSSession", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-VM", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-VM", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Set-VM", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-Item -ItemType Directory -Path $GuestPrefix", source, StringComparison.Ordinal);
    }

    [Fact]
    public void RestoresPowerThroughProductCommandsAndDoesNotWriteCurrentEvidence()
    {
        var source = Source();
        RequireTokens(
            source,
            "'vm', 'shutdown', $VmName",
            "'vm', 'poweroff', $VmName",
            "PCV_P1_GUESTFILE_CLEANUP_POWER_NOT_RESTORED",
            "PCV_P1_GUESTFILE_CLEANUP_GUEST_FILE_REMAINS",
            "PCV_P1_GUESTFILE_SUMMARY_WRITE_FAILED",
            "summary.json.tmp");
        AssertOrdered(source, "function Invoke-Cleanup", "'guest-file-remove'", "'vm', 'shutdown', $VmName");
        Assert.DoesNotContain("docs/ga-ready/current-evidence.json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ConvertTo-SecureString", source, StringComparison.OrdinalIgnoreCase);
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

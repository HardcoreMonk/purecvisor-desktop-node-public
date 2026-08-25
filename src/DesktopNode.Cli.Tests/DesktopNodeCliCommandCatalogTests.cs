using System.Text.Json;
using DesktopNode.Cli;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliCommandCatalogTests
{
    [Theory]
    [InlineData("host status", "GET", "/api/v1/host/status")]
    [InlineData("runtime policy", "GET", "/api/v1/runtime/policy")]
    [InlineData("ops summary", "GET", "/api/v1/ops/summary")]
    [InlineData("network inventory", "GET", "/api/v1/network/inventory")]
    [InlineData("network list", "GET", "/api/v1/network/inventory")]
    [InlineData("vm list", "GET", "/api/v1/vms")]
    [InlineData("vm get ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01")]
    [InlineData("vm start ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/start")]
    [InlineData("vm stop ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/poweroff")]
    [InlineData("vm shutdown ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/shutdown")]
    [InlineData("vm guest-shutdown ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/shutdown")]
    [InlineData("vm poweroff ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/poweroff")]
    [InlineData("vm restart ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/restart")]
    [InlineData("vm pause ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/pause")]
    [InlineData("vm resume ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/resume")]
    [InlineData("vm save ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/save")]
    [InlineData("vm resume-saved ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/resume-saved")]
    [InlineData("vm console ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/console")]
    [InlineData("vm vnc ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/console")]
    [InlineData("vm memory-stats ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/memory-stats")]
    [InlineData("vm cpu-stats ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/cpu-stats")]
    [InlineData("vm guest-agent-status ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/guest-agent/status")]
    [InlineData("vm guest-ping ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/guest-agent/ping")]
    [InlineData("vm blkio-get ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/blkio")]
    [InlineData("vm bandwidth ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/bandwidth")]
    [InlineData("vm eject ubuntu-lab-01", "POST", "/api/v1/vms/ubuntu-lab-01/eject")]
    [InlineData("vm attach ubuntu-lab-01 --iso D:\\isos\\ubuntu.iso", "POST", "/api/v1/vms/ubuntu-lab-01/attach")]
    [InlineData("vm delete-status ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/delete-status")]
    [InlineData("vm checkpoint list ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/checkpoints")]
    [InlineData("vm snapshot list ubuntu-lab-01", "GET", "/api/v1/vms/ubuntu-lab-01/checkpoints")]
    [InlineData("vm checkpoint restore ubuntu-lab-01 before-upgrade", "POST", "/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade/restore")]
    [InlineData("vm checkpoint delete ubuntu-lab-01 before-upgrade", "DELETE", "/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade")]
    [InlineData("job get job-123", "GET", "/api/v1/jobs/job-123")]
    [InlineData("job cancel job-123", "POST", "/api/v1/jobs/job-123/cancel")]
    [InlineData("job retry job-123", "POST", "/api/v1/jobs/job-123/retry")]
    [InlineData("job reconcile job-123", "POST", "/api/v1/jobs/job-123/reconcile")]
    [InlineData("diagnostics bundle list", "GET", "/api/v1/diagnostics/bundles")]
    [InlineData("diagnostics bundle create", "POST", "/api/v1/diagnostics/bundles")]
    public void RoutesCommandsToLocalApiRequests(string commandLine, string method, string path)
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine));

        Assert.Equal(method, request.Method);
        Assert.Equal(path, request.Path);
        if (path.EndsWith("/attach", StringComparison.Ordinal))
        {
            Assert.NotNull(request.Body);
            return;
        }

        Assert.Null(request.Body);
    }

    [Fact]
    public void RejectsTwoWordResumeSavedAsUsageError()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["vm", "resume", "saved", "ubuntu-lab-01"]));

        Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
        Assert.Contains("vm resume <vm>", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsUnsupportedDiagnosticBundleListOption()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["diagnostics", "bundle", "list", "--limti", "25"]));

        Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
        Assert.Equal("PCV_CLI_USAGE|Unsupported diagnostics bundle list option --limti.", error.Message);
    }

    [Theory]
    [InlineData("snapshot list ubuntu-lab-01")]
    [InlineData("snapshot create ubuntu-lab-01 --name before-upgrade")]
    [InlineData("snapshot rollback ubuntu-lab-01 before-upgrade")]
    [InlineData("snapshot delete ubuntu-lab-01 before-upgrade")]
    public void RejectsRemovedTopLevelSnapshotCommands(string commandLine)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine)));

        Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
        Assert.Contains("Unknown command group 'snapshot'", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("pcvcli snapshot list|create|rollback|delete", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EscapesQuotedRouteSegments()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "get", "ubuntu lab"]);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu%20lab", request.Path);
    }

    [Fact]
    public void BuildsVmCreateBodyFromLinuxPcvCtlShape()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "create",
            "ubuntu-lab-01",
            "--vcpu",
            "2",
            "--memory_mb",
            "4096",
            "--disk_size_gb",
            "40",
            "--iso_path",
            "D:\\isos\\ubuntu.iso",
            "--image_dir",
            "D:\\PureCVisor\\VMs"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.Equal("ubuntu-lab-01", root.GetProperty("name").GetString());
        Assert.Equal("D:\\isos\\ubuntu.iso", root.GetProperty("iso_path").GetString());
        Assert.Equal(2, root.GetProperty("cpu").GetInt32());
        Assert.Equal(4096, root.GetProperty("memory_mb").GetInt32());
        Assert.Equal(40, root.GetProperty("disk_gb").GetInt32());
        Assert.Equal("D:\\PureCVisor\\VMs", root.GetProperty("vm_root").GetString());
    }

    [Fact]
    public void BuildsVmCreateBodyFromNamedOptions()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "create",
            "--name",
            "ubuntu-lab-01",
            "--iso",
            "D:\\isos\\ubuntu.iso",
            "--cpu",
            "2",
            "--memory-mb",
            "4096",
            "--disk-gb",
            "40",
            "--vm-root",
            "D:\\PureCVisor\\VMs",
            "--generation",
            "2"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.Equal("ubuntu-lab-01", root.GetProperty("name").GetString());
        Assert.Equal("D:\\isos\\ubuntu.iso", root.GetProperty("iso_path").GetString());
        Assert.Equal(2, root.GetProperty("cpu").GetInt32());
        Assert.Equal(4096, root.GetProperty("memory_mb").GetInt32());
        Assert.Equal(40, root.GetProperty("disk_gb").GetInt32());
        Assert.Equal("D:\\PureCVisor\\VMs", root.GetProperty("vm_root").GetString());
        Assert.Equal(2, root.GetProperty("generation").GetInt32());
    }

    [Fact]
    public void VmAttachSendsIsoPathBody()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "attach", "ubuntu-lab-01", "--iso", @"D:\isos\ubuntu.iso"]);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal(@"D:\isos\ubuntu.iso", document.RootElement.GetProperty("iso_path").GetString());
    }

    [Fact]
    public void VmAttachAcceptsIsoPathAlias()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "attach", "ubuntu-lab-01", "--iso_path", @"D:\isos\ubuntu.iso"]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/attach", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal(@"D:\isos\ubuntu.iso", document.RootElement.GetProperty("iso_path").GetString());
    }

    [Fact]
    public void BuildsCheckpointCreateBodyFromNameOption()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "checkpoint",
            "create",
            "ubuntu-lab-01",
            "--name",
            "before-upgrade"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/checkpoints", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal("before-upgrade", document.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public void BuildsVmRenameBodyFromLinuxPcvCtlShape()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "rename",
            "ubuntu-lab-01",
            "ubuntu-lab-02"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/rename", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal("ubuntu-lab-02", document.RootElement.GetProperty("new_name").GetString());
    }

    [Fact]
    public void RequiresExplicitYesForVmDelete()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["vm", "delete", "ubuntu-lab-01"]));

        Assert.Contains("PCV_CLI_CONFIRMATION_REQUIRED", error.Message);
    }

    [Fact]
    public void RequiresExplicitYesForVmManage()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["vm", "manage", "ubuntu-lab-01"]));

        Assert.Contains("PCV_CLI_CONFIRMATION_REQUIRED", error.Message, StringComparison.Ordinal);
        Assert.Contains("Use: vm manage <vm> --yes.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutesVmManageWhenExplicitlyConfirmed()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "manage", "ubuntu-lab-01", "--yes"]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/manage", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal("ubuntu-lab-01", document.RootElement.GetProperty("confirm_name").GetString());
    }

    [Fact]
    public void UsesVmManageArgumentVerbatimAsConfirmName()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "manage", "ubuntu lab", "--yes"]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu%20lab/manage", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal("ubuntu lab", document.RootElement.GetProperty("confirm_name").GetString());
    }

    [Theory]
    [InlineData("vm guest-agent-ensure-channel ubuntu-lab-01")]
    public void RejectsGuestExecutionApplyShapeUntilSecurityBoundaryOpens(string commandLine)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine)));

        Assert.Contains("PCV_CLI_CONFIRMATION_REQUIRED", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("pcvcli-backend-command-gap-slice", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildsGuestChannelVerifyAndRepairQueuedRequests()
    {
        var verify = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "guest-agent-ensure-channel",
            "ubuntu-lab-01",
            "--verify",
            "--credential-ref",
            "wincred:PureCVisor/guest/admin"
        ]);
        var repair = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "guest-agent-ensure-channel",
            "ubuntu-lab-01",
            "--repair",
            "--yes"
        ]);

        Assert.Equal("POST", verify.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/guest/channel/verify", verify.Path);
        using (var verifyDocument = JsonDocument.Parse(verify.Body!))
        {
            var root = verifyDocument.RootElement;
            Assert.Equal("wincred:PureCVisor/guest/admin", root.GetProperty("credential_ref").GetString());
            Assert.Equal("verify", root.GetProperty("mode").GetString());
        }

        Assert.Equal("POST", repair.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/guest/channel", repair.Path);
        using var repairDocument = JsonDocument.Parse(repair.Body!);
        Assert.True(repairDocument.RootElement.GetProperty("yes").GetBoolean());
        Assert.Equal("repair", repairDocument.RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public void BuildsGuestChannelPreviewRequest()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "guest-agent-ensure-channel",
            "ubuntu-lab-01",
            "--dry-run"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/guest/channel/preview", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.True(document.RootElement.GetProperty("dry_run").GetBoolean());
        Assert.Equal("dry-run", document.RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public void BuildsGuestExecPreviewRequestWithoutRawSecretOptions()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "guest-exec",
            "ubuntu-lab-01",
            "--dry-run",
            "--credential-ref",
            "wincred:PureCVisor/guest/admin",
            "--timeout-sec",
            "10",
            "--",
            "powershell",
            "Get-Process"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/guest/exec/preview", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.True(root.GetProperty("dry_run").GetBoolean());
        Assert.Equal("dry-run", root.GetProperty("mode").GetString());
        Assert.Equal("wincred:PureCVisor/guest/admin", root.GetProperty("credential_ref").GetString());
        Assert.Equal(10, root.GetProperty("timeout_sec").GetInt32());
        Assert.Equal(
            new[] { "powershell", "Get-Process" },
            root.GetProperty("command").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    [Fact]
    public void BuildsGuestExecQueuedRequestWithCredentialRefAndTimeout()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "guest-exec",
            "ubuntu-lab-01",
            "--credential-ref",
            "wincred:PureCVisor/guest/admin",
            "--timeout-sec",
            "45",
            "--",
            "powershell",
            "hostname"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/guest/exec", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.Equal("wincred:PureCVisor/guest/admin", root.GetProperty("credential_ref").GetString());
        Assert.Equal(45, root.GetProperty("timeout_sec").GetInt32());
        Assert.Equal(
            new[] { "powershell", "hostname" },
            root.GetProperty("command").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    [Fact]
    public void RejectsGuestExecRawSecretOptions()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest([
                "vm",
                "guest-exec",
                "ubuntu-lab-01",
                "--dry-run",
                "--password",
                "super-secret",
                "--",
                "hostname"
            ]));

        Assert.Contains("PCV_CLI_CREDENTIAL_REF_REQUIRED", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("vm blkio-set ubuntu-lab-01 --disk disk0 --maximum-iops 1200 --minimum-iops 100 --dry-run", "/api/v1/vms/ubuntu-lab-01/qos/storage/preview", "disk", "disk0", "maximum_iops", 1200, "minimum_iops", 100)]
    [InlineData("vm bandwidth-set ubuntu-lab-01 --adapter adapter0 --maximum-kbps 2048 --minimum-kbps 256 --dry-run", "/api/v1/vms/ubuntu-lab-01/qos/network/preview", "adapter", "adapter0", "maximum_kbps", 2048, "minimum_kbps", 256)]
    public void BuildsQosMutationPreviewRequests(
        string commandLine,
        string expectedPath,
        string targetProperty,
        string targetValue,
        string maximumProperty,
        int maximumValue,
        string minimumProperty,
        int minimumValue)
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine));

        Assert.Equal("POST", request.Method);
        Assert.Equal(expectedPath, request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.Equal(targetValue, root.GetProperty(targetProperty).GetString());
        Assert.Equal(maximumValue, root.GetProperty(maximumProperty).GetInt32());
        Assert.Equal(minimumValue, root.GetProperty(minimumProperty).GetInt32());
        Assert.True(root.GetProperty("dry_run").GetBoolean());
    }

    [Theory]
    [InlineData("vm blkio-set ubuntu-lab-01 --disk disk0 --maximum-iops 1200 --yes", "/api/v1/vms/ubuntu-lab-01/qos/storage", "disk", "disk0", "maximum_iops", 1200)]
    [InlineData("vm bandwidth-set ubuntu-lab-01 --adapter adapter0 --maximum-kbps 2048 --yes", "/api/v1/vms/ubuntu-lab-01/qos/network", "adapter", "adapter0", "maximum_kbps", 2048)]
    public void BuildsQosMutationApplyRequests(
        string commandLine,
        string expectedPath,
        string targetProperty,
        string targetValue,
        string maximumProperty,
        int maximumValue)
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine));

        Assert.Equal("POST", request.Method);
        Assert.Equal(expectedPath, request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        var root = document.RootElement;
        Assert.Equal(targetValue, root.GetProperty(targetProperty).GetString());
        Assert.Equal(maximumValue, root.GetProperty(maximumProperty).GetInt32());
        Assert.False(root.TryGetProperty("dry_run", out _));
    }

    [Theory]
    [InlineData("vm blkio-set ubuntu-lab-01 --disk disk0 --maximum-iops 1200")]
    [InlineData("vm bandwidth-set ubuntu-lab-01 --adapter adapter0 --maximum-kbps 2048")]
    public void RequiresDryRunOrExplicitYesForQosMutation(string commandLine)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine)));

        Assert.Contains("PCV_CLI_CONFIRMATION_REQUIRED", error.Message, StringComparison.Ordinal);
        Assert.Contains("--dry-run", error.Message, StringComparison.Ordinal);
        Assert.Contains("--yes", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("vm blkio-set ubuntu-lab-01 --disk disk0 --maximum-iops -1 --dry-run", "PCV_VM_QOS_STORAGE_RANGE_INVALID")]
    [InlineData("vm bandwidth-set ubuntu-lab-01 --adapter adapter0 --maximum-kbps 2048 --minimum-kbps 4096 --yes", "PCV_VM_QOS_NETWORK_RANGE_INVALID")]
    public void RejectsQosMutationInvalidRangesWithoutGlobalUsage(string commandLine, string expectedCode)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine)));

        Assert.Contains(expectedCode, error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Usage:", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("vm set-memory ubuntu-lab-01 4096", "/api/v1/vms/ubuntu-lab-01/set-memory", "memory_mb", 4096)]
    [InlineData("vm set-vcpu ubuntu-lab-01 4", "/api/v1/vms/ubuntu-lab-01/set-vcpu", "cpu", 4)]
    [InlineData("vm disk-resize ubuntu-lab-01 80", "/api/v1/vms/ubuntu-lab-01/disk-resize", "disk_gb", 80)]
    public void BuildsVmResourceMutationBodies(string commandLine, string expectedPath, string expectedProperty, int expectedValue)
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(Split(commandLine));

        Assert.Equal("POST", request.Method);
        Assert.Equal(expectedPath, request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal(expectedValue, document.RootElement.GetProperty(expectedProperty).GetInt32());
    }

    [Fact]
    public void BuildsVmLimitBodyFromHyperVQosScopeLock()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "vm",
            "limit",
            "ubuntu-lab-01",
            "--cpu",
            "4",
            "--memory-mb",
            "8192"
        ]);

        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01/limit", request.Path);
        using var document = JsonDocument.Parse(request.Body!);
        Assert.Equal(4, document.RootElement.GetProperty("cpu").GetInt32());
        Assert.Equal(8192, document.RootElement.GetProperty("memory_mb").GetInt32());
    }

    [Fact]
    public void RoutesVmDeleteWhenExplicitlyConfirmed()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest(["vm", "delete", "ubuntu-lab-01", "--yes"]);

        Assert.Equal("DELETE", request.Method);
        Assert.Equal("/api/v1/vms/ubuntu-lab-01", request.Path);
    }

    [Fact]
    public void BuildsJobListPaginationQuery()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "job",
            "list",
            "--limit",
            "25",
            "--offset",
            "50"
        ]);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/api/v1/jobs?limit=25&offset=50", request.Path);
    }

    [Fact]
    public void BuildsDiagnosticBundleListPaginationQuery()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "diagnostics",
            "bundle",
            "list",
            "--limit",
            "25",
            "--offset",
            "50"
        ]);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/api/v1/diagnostics/bundles?limit=25&offset=50", request.Path);
    }

    [Theory]
    [InlineData("--limit", "many")]
    [InlineData("--offset", "later")]
    public void RejectsNonIntegerDiagnosticBundlePagination(string option, string value)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["diagnostics", "bundle", "list", option, value]));

        Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
        Assert.Equal($"PCV_CLI_USAGE|Option {option} must be an integer.", error.Message);
    }

    [Fact]
    public void CapturesDiagnosticsDownloadOutputPath()
    {
        var request = DesktopNodeCliCommandCatalog.CreateRequest([
            "diagnostics",
            "bundle",
            "download",
            "pcv-diag-20260509T010101Z-abc",
            "--output",
            "D:\\evidence\\bundle.json"
        ]);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/api/v1/diagnostics/bundles/pcv-diag-20260509T010101Z-abc/download", request.Path);
        Assert.Equal("D:\\evidence\\bundle.json", request.OutputPath);
    }

    [Fact]
    public void RoutesEveryDeclaredCliSurfaceBindingThroughPcvCli()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(
            FindRepoFile("config/desktop-node-feature-surface-ledger.json")));
        var presentCount = 0;
        var excludedCount = 0;

        foreach (var feature in document.RootElement.GetProperty("features").EnumerateArray())
        {
            var featureId = feature.GetProperty("feature_id").GetString()!;
            foreach (var route in feature.GetProperty("routes").EnumerateArray())
            {
                var operationId = route.GetProperty("operation_id").GetString()!;
                var method = route.GetProperty("method").GetString()!;
                var routeTemplate = route.GetProperty("route_template").GetString()!;
                var cliPresent = route.GetProperty("present_surfaces")
                    .EnumerateArray()
                    .Any(item => item.GetString() == "cli");
                if (!cliPresent)
                {
                    excludedCount += 1;
                    continue;
                }

                presentCount += 1;
                Assert.True(
                    route.TryGetProperty("surface_bindings", out var surfaceBindings),
                    $"Missing CLI binding: feature_id={featureId} operation_id={operationId} method={method} route={routeTemplate}");
                Assert.True(
                    surfaceBindings.TryGetProperty("cli", out var cliBinding),
                    $"Missing CLI binding: feature_id={featureId} operation_id={operationId} method={method} route={routeTemplate}");
                var command = cliBinding.GetProperty("command")
                    .EnumerateArray()
                    .Select(item => item.GetString()!)
                    .ToArray();
                var request = DesktopNodeCliCommandCatalog.CreateRequest(command);
                var actualPath = request.Path.Split('?', 2)[0];

                Assert.Equal(method, request.Method);
                Assert.True(
                    MatchesRouteTemplate(routeTemplate, actualPath),
                    $"CLI route mismatch: feature_id={featureId} operation_id={operationId} expected={routeTemplate} actual={actualPath}");
            }
        }

        Assert.Equal(53, presentCount);
        Assert.Equal(7, excludedCount);
    }

    [Fact]
    public void UsageShowsPcvCliCommandName()
    {
        var usage = DesktopNodeCliCommandCatalog.GetUsage();

        Assert.Contains("pcvcli [--api URL]", usage);
        Assert.Contains("%ProgramData%\\PureCVisor\\desktop-node\\api-token.dpapi.json", usage);
        Assert.Contains("pcvcli vm limit <vm> --cpu N [--memory-mb MB]", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm blkio-set <vm> --disk DISK --maximum-iops N [--minimum-iops N] --dry-run|--yes", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm bandwidth-set <vm> --adapter ADAPTER --maximum-kbps N [--minimum-kbps N] --dry-run|--yes", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm blkio-get|bandwidth|guest-agent-status|guest-ping", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm set-memory|set-vcpu|disk-resize", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm attach <vm> --iso <path>", usage, StringComparison.Ordinal);
        Assert.Contains("pause|resume|save|resume-saved", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm manage <vm> --yes", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm eject|delete-status", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli diagnostics bundle list [--limit N] [--offset N]", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli diagnostics bundle create", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli diagnostics bundle download <bundle_id> --output <path>", usage, StringComparison.Ordinal);
        Assert.DoesNotContain("pcvcli snapshot list|create|rollback|delete", usage, StringComparison.Ordinal);
        Assert.DoesNotContain("pcvcli console capabilities", usage, StringComparison.Ordinal);
        Assert.DoesNotContain("  pcv [--api URL]", usage);
    }

    [Fact]
    public void KeepsGlobalConsoleCapabilitiesOutOfCliCatalog()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliCommandCatalog.CreateRequest(["console", "capabilities"]));

        Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
        Assert.Contains("Unknown command group 'console'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UsageAndSharedTermsDescribeVmDeleteConfirmation()
    {
        var terms = File.ReadAllText(FindRepoFile("docs/OPERATOR_SURFACE_TERMS.md"));
        var usage = DesktopNodeCliCommandCatalog.GetUsage();
        var error = Assert.Throws<ArgumentException>(() => DesktopNodeCliCommandCatalog.CreateRequest(["vm", "delete", "demo"]));

        Assert.Contains("VM delete 확인", terms, StringComparison.Ordinal);
        Assert.Contains("Web Console과 CLI는 destructive VM delete 전에 명시 확인을 요구한다", terms, StringComparison.Ordinal);
        Assert.Contains("Checkpoint mutation", terms, StringComparison.Ordinal);
        Assert.Contains("CLI checkpoint command는 API job으로 라우팅되는 명시 subcommand다", terms, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm manage <vm> --yes", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm delete <vm> --yes", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli vm checkpoint list|create|restore|delete", usage, StringComparison.Ordinal);
        Assert.Contains("VM delete requires explicit confirmation", error.Message, StringComparison.Ordinal);
        Assert.Contains("vm delete <vm> --yes", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("public release", usage + error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesRouteTemplate(string template, string actualPath)
    {
        var expected = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actual = actualPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return expected.Length == actual.Length && expected.Zip(actual).All(pair =>
            (pair.First.StartsWith('{') && pair.First.EndsWith('}')) ||
            string.Equals(pair.First, pair.Second, StringComparison.Ordinal));
    }

    private static string[] Split(string commandLine)
    {
        return commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static string FindRepoFile(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, normalized);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Could not locate repository file.", relativePath);
    }
}

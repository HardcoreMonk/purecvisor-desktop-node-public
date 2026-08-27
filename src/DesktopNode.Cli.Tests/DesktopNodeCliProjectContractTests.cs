using System.Text.Json;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliProjectContractTests
{
    [Fact]
    public void PublishesPcvCliCommandName()
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DesktopNode.Cli", "DesktopNode.Cli.csproj"));
        var project = File.ReadAllText(projectPath);

        Assert.Contains("<AssemblyName>pcvcli</AssemblyName>", project, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentsLinuxCompatibilityPromotionState()
    {
        var usage = File.ReadAllText(FindRepoFile("docs/CLI_COMMAND_USAGE.md"));
        var slice = File.ReadAllText(FindRepoFile("docs/ga-ready/evidence/pcvcli-backend-command-gap-slice-2026-05-19.md"));

        Assert.Contains("pcvcli-backend-command-gap-slice-2026-05-19", usage, StringComparison.Ordinal);
        Assert.Contains("0.42.38-admin-smoke", usage, StringComparison.Ordinal);
        Assert.Contains("제품 범위 밖으로 유지", usage, StringComparison.Ordinal);
        Assert.Contains("vm eject/delete-status", usage, StringComparison.Ordinal);
        Assert.Contains("vm set-memory/set-vcpu/disk-resize", usage, StringComparison.Ordinal);
        Assert.Contains("vm rename/pause/resume", slice, StringComparison.Ordinal);
        Assert.Contains("code-level queued mutation promoted", slice, StringComparison.Ordinal);
        Assert.Contains("vm set-memory/set-vcpu/disk-resize", slice, StringComparison.Ordinal);
        Assert.Contains("manual-admin-gate-required", slice, StringComparison.Ordinal);
        Assert.Contains("PCV_CLI_BACKEND_NOT_EXPOSED", slice, StringComparison.Ordinal);
        Assert.Contains("PCV_CLI_MANUAL_ADMIN_GATE_REQUIRED", slice, StringComparison.Ordinal);
        Assert.Contains("다음 backend slice", slice, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentsImplementedAdvancedCliCommandShapesAndSafetyBoundaries()
    {
        var usage = File.ReadAllText(FindRepoFile("docs/CLI_COMMAND_USAGE.md"));

        string[] commandShapes =
        [
            "pcvcli job reconcile <job_id>",
            "pcvcli vm guest-agent-ensure-channel <vm> --dry-run",
            "pcvcli vm guest-agent-ensure-channel <vm> --verify --credential-ref <ref> [--timeout-sec <n>]",
            "pcvcli vm guest-agent-ensure-channel <vm> --repair --yes",
            "pcvcli vm guest-exec <vm> --dry-run [--credential-ref <ref>] [--timeout-sec <n>] -- <command...>",
            "pcvcli vm guest-exec <vm> --credential-ref <ref> [--timeout-sec <n>] -- <command...>",
            "pcvcli vm set-memory <vm> <memory_mb>",
            "pcvcli vm set-vcpu <vm> <vcpu_count>",
            "pcvcli vm disk-resize <vm> <disk_gb>",
            "pcvcli vm save <vm>",
            "pcvcli vm resume-saved <vm>",
            "pcvcli vm manage <vm> --yes",
            "pcvcli vm clone <source> --name <target> --yes",
            "pcvcli vm clone <source> --name <target> --dry-run",
            "pcvcli vm eject <vm>",
            "pcvcli vm delete-status <vm>"
        ];

        foreach (var commandShape in commandShapes)
        {
            Assert.Contains(commandShape, usage, StringComparison.Ordinal);
        }

        Assert.Contains("PCV_JOB_RECONCILIATION_REQUIRED", usage, StringComparison.Ordinal);
        Assert.Contains("mutation을 중복 제출하지 않고", usage, StringComparison.Ordinal);
        Assert.Contains("PCV_CLI_CREDENTIAL_REF_REQUIRED", usage, StringComparison.Ordinal);
        Assert.Contains("secret-bearing command option", usage, StringComparison.Ordinal);
        Assert.Contains("PCV_VM_DISK_SHRINK_NOT_SUPPORTED", usage, StringComparison.Ordinal);
        Assert.Contains("현재 virtual disk보다 작은 값을 요청하면", usage, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentsDiagnosticsListCliAndApiOnlyConsoleCapabilities()
    {
        var usage = File.ReadAllText(FindRepoFile("docs/CLI_COMMAND_USAGE.md"));
        var featureUsage = File.ReadAllText(FindRepoFile("docs/USER_FEATURE_USAGE_SPEC.md"));
        var userGuide = File.ReadAllText(FindRepoFile("docs/USER_GUIDE.md"));
        var readme = File.ReadAllText(FindRepoFile("src/DesktopNode.Cli/README.md"));

        Assert.Contains("pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]", usage, StringComparison.Ordinal);
        Assert.Contains("GET /api/v1/diagnostics/bundles?limit=<n>&offset=<n>", usage, StringComparison.Ordinal);
        Assert.Contains("diagnostics.read", usage, StringComparison.Ordinal);
        Assert.Contains("기본 retention은", usage, StringComparison.Ordinal);
        Assert.Contains("14일 또는 최대 50개", usage, StringComparison.Ordinal);
        Assert.Contains("PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE", usage, StringComparison.Ordinal);
        Assert.Contains("$created.data.bundle_id", usage, StringComparison.Ordinal);
        Assert.DoesNotContain("$created.bundle_id", usage, StringComparison.Ordinal);
        Assert.DoesNotContain("현재 `pcvcli diagnostics bundle list` command는 없으며", usage, StringComparison.Ordinal);

        Assert.Contains("GET /api/v1/console/capabilities", usage, StringComparison.Ordinal);
        Assert.Contains("API/Web Console 전용", usage, StringComparison.Ordinal);
        Assert.Contains("console-access-card.v1", usage, StringComparison.Ordinal);
        Assert.Contains("console.view", usage, StringComparison.Ordinal);
        Assert.Contains("pcvcli --json vm console ubuntu-lab-01", usage, StringComparison.Ordinal);
        Assert.Contains("GUI를 자동 실행하지 않고", usage, StringComparison.Ordinal);

        Assert.Contains("| Diagnostics list | [ `pcv.diagnostics.bundle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-diagnostics-bundle) | Troubleshooting bundle 목록/pagination | `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]` | `GET /diagnostics/bundles?limit=&offset=` |", featureUsage, StringComparison.Ordinal);
        Assert.Contains("| Console capability discovery | [ `pcv.console.capabilities` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-console-capabilities) | Console/Troubleshooting capability card | API/Web Console 전용 | `GET /console/capabilities` |", featureUsage, StringComparison.Ordinal);
        Assert.Contains("| VM console/noVNC handoff | [ `pcv.vm.console-handoff` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-console-handoff) | 선택 VM Console panel | `pcvcli vm console/vnc <vm>` | `GET /vms/{id}/console` |", featureUsage, StringComparison.Ordinal);

        Assert.Contains("pcvcli --json diagnostics bundle list --limit 10 --offset 0", userGuide, StringComparison.Ordinal);
        Assert.Contains("전역 capability discovery는 API/Web Console 전용", userGuide, StringComparison.Ordinal);

        Assert.Contains("pcvcli diagnostics bundle list [--limit N] [--offset N]", readme, StringComparison.Ordinal);
        Assert.Contains("API/Web Console 전용 discovery card", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentsEveryStableFeatureIdAndSurfaceLedgerLink()
    {
        using var surfaceLedger = JsonDocument.Parse(File.ReadAllText(
            FindRepoFile("config/desktop-node-feature-surface-ledger.json")));
        var implementationLedger = File.ReadAllText(
            FindRepoFile("docs/FEATURE_IMPLEMENTATION_LEDGER.md"));
        var featureUsage = File.ReadAllText(
            FindRepoFile("docs/USER_FEATURE_USAGE_SPEC.md"));
        var featureCount = 0;
        var routeCount = 0;

        foreach (var feature in surfaceLedger.RootElement.GetProperty("features").EnumerateArray())
        {
            featureCount += 1;
            var featureId = feature.GetProperty("feature_id").GetString()!;
            var anchor = featureId.Replace('.', '-');
            Assert.Contains($"<a id=\"{anchor}\"></a>", implementationLedger, StringComparison.Ordinal);
            Assert.Contains(
                $"[ `{featureId}` ](FEATURE_IMPLEMENTATION_LEDGER.md#{anchor})",
                featureUsage,
                StringComparison.Ordinal);

            foreach (var route in feature.GetProperty("routes").EnumerateArray())
            {
                routeCount += 1;
                var operationId = route.GetProperty("operation_id").GetString()!;
                var method = route.GetProperty("method").GetString()!;
                var routeTemplate = route.GetProperty("route_template").GetString()!;
                Assert.Contains(operationId, implementationLedger, StringComparison.Ordinal);
                Assert.Contains($"{method} {routeTemplate}", implementationLedger, StringComparison.Ordinal);
            }
        }

        Assert.Equal(28, featureCount);
        Assert.Equal(62, routeCount);
        string[] stageLabels =
        [
            "code_tested",
            "packaged",
            "installed_tested",
            "actual_vm_tested",
            "manual_admin_tested",
            "not-assessed"
        ];
        foreach (var stageLabel in stageLabels)
        {
            Assert.Contains(stageLabel, implementationLedger, StringComparison.Ordinal);
        }

        Assert.Contains(
            "pcv.vm.saved-lifecycle/actual_vm_tested/fail",
            implementationLedger,
            StringComparison.Ordinal);
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

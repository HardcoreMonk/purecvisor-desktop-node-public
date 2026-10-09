using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.IntegrationTests;

// ADR-0016 guard and cleanup. The constructor refuses to start (so every test fails) unless the standing approval, the
// privileges and the host switch state are in place and no earlier integration run left its VMs behind. Every VM the
// tests create uses this run's prefix (pcv-it-<run id>-) and stores its disks under artifacts/hyperv-integration/<run id>/.
// Other pcv-it- VMs (scenario demos keep pcv-it-s2-source as an Off template) are not leftovers: they are only read, and
// the before/after name comparison in Dispose still proves this run touched nothing but its own VMs.
public sealed class HyperVIntegrationFixture : IDisposable
{
    internal const string ApprovalVariable = "PCV_HYPERV_INTEGRATION_APPROVAL";
    internal const string IsoVariable = "PCV_HYPERV_INTEGRATION_ISO";
    internal const string NamePrefix = "pcv-it-";
    private const string DefaultIso = "artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso";
    private const string HyperVAdministratorsSid = "S-1-5-32-578";
    private static readonly Regex RunVmName = new("^pcv-it-[0-9]{14}-", RegexOptions.CultureInvariant);

    private readonly DesktopNodeHyperVNativeAdapter adapter;
    private readonly IReadOnlyList<string> namesBefore;
    private readonly string repositoryRoot;

    public HyperVIntegrationFixture()
    {
        repositoryRoot = FindRepositoryRoot();
        RunId = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        RunPrefix = $"{NamePrefix}{RunId}-";
        VmRoot = Path.Combine(repositoryRoot, "artifacts", "hyperv-integration", RunId);
        IsoPath = Path.Combine(
            repositoryRoot,
            (Environment.GetEnvironmentVariable(IsoVariable) ?? DefaultIso).Replace('/', Path.DirectorySeparatorChar));

        RequireApproval();
        RequirePrivileges();
        RequireDefaultSwitch();
        if (!File.Exists(IsoPath))
        {
            throw new InvalidOperationException($"Integration ISO not found: {IsoPath}");
        }

        adapter = DesktopNodeHyperVNativeAdapter.CreateDefault();
        namesBefore = VmNames();
        var leftovers = namesBefore.Where(name => RunVmName.IsMatch(name)).ToList();
        if (leftovers.Count > 0)
        {
            throw new InvalidOperationException(
                "An earlier integration run left VMs behind; clean them up first: " + string.Join(", ", leftovers));
        }

        Directory.CreateDirectory(VmRoot);
    }

    public string RunId { get; }

    public string RunPrefix { get; }

    public string VmRoot { get; }

    public string IsoPath { get; }

    public string VmName(string suffix) => RunPrefix + suffix;

    public DesktopNodeHyperVOperationResult Invoke(string operation, object parameters)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(parameters));
        if (!adapter.TryInvoke(operation, document.RootElement.Clone(), CancellationToken.None, out var result))
        {
            throw new InvalidOperationException($"The native adapter did not handle {operation}.");
        }

        return result;
    }

    public JsonObject? FindVm(string name) =>
        VmRows().FirstOrDefault(row => row["name"]?.GetValue<string>() == name);

    public void DeleteVm(string name)
    {
        if (FindVm(name) is not null)
        {
            Invoke("vm.delete", new { name });
        }
    }

    public void Dispose()
    {
        var problems = new List<string>();
        foreach (var name in VmNames().Where(name => name.StartsWith(RunPrefix, StringComparison.Ordinal)))
        {
            var deleted = Invoke("vm.delete", new { name });
            if (!deleted.Ok)
            {
                problems.Add($"delete {name}: {deleted.Error?.Code}");
            }
        }

        if (Directory.Exists(VmRoot))
        {
            Directory.Delete(VmRoot, recursive: true);
        }

        var namesAfter = VmNames();
        if (!namesAfter.Order(StringComparer.Ordinal).SequenceEqual(namesBefore.Order(StringComparer.Ordinal)))
        {
            problems.Add("VM names differ before and after the run: " + string.Join(", ", namesAfter.Except(namesBefore)));
        }

        File.WriteAllText(
            Path.Combine(repositoryRoot, "artifacts", "hyperv-integration", RunId + ".summary.json"),
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = "pcv-hyperv-integration-run-v1",
                ["run_id"] = RunId,
                ["vm_names_before"] = namesBefore,
                ["vm_names_after"] = namesAfter,
                ["storage_root_removed"] = !Directory.Exists(VmRoot),
                ["cleanup_problems"] = problems,
                ["promotion_evidence"] = false
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        if (problems.Count > 0)
        {
            throw new InvalidOperationException("Integration cleanup failed: " + string.Join("; ", problems));
        }
    }

    private List<JsonObject> VmRows()
    {
        var result = Invoke("vm.list", new { });
        if (!result.Ok || result.Data is not { } data)
        {
            throw new InvalidOperationException($"vm.list failed: {result.Error?.Code}");
        }

        var node = JsonNode.Parse(data.GetRawText());
        var rows = node as JsonArray ?? node?["vms"] as JsonArray ?? [];
        return rows.OfType<JsonObject>().ToList();
    }

    private IReadOnlyList<string> VmNames() =>
        VmRows().Select(row => row["name"]?.GetValue<string>() ?? string.Empty).ToList();

    private void RequireApproval()
    {
        var approval = Environment.GetEnvironmentVariable(ApprovalVariable);
        if (string.IsNullOrWhiteSpace(approval) || approval.Length < 20)
        {
            throw new InvalidOperationException(
                $"{ApprovalVariable} must name the ADR-0016 standing approval phrase from the active campaign.");
        }

        var campaign = JsonNode.Parse(File.ReadAllText(
            Path.Combine(repositoryRoot, "docs", "ga-ready", "active-campaign.json")));
        var locator = campaign?["approval_locator"]?.GetValue<string>() ?? string.Empty;
        if (!locator.Contains(approval, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{ApprovalVariable} is not part of the active campaign approval_locator.");
        }
    }

    private static void RequirePrivileges()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var hyperVAdministrator = identity.Groups?.Any(group => group.Value == HyperVAdministratorsSid) == true;
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator) && !hyperVAdministrator)
        {
            throw new InvalidOperationException("The integration tier needs an elevated administrator or Hyper-V Administrators.");
        }
    }

    private static void RequireDefaultSwitch()
    {
        using var searcher = new ManagementObjectSearcher(
            new ManagementScope(@"root\virtualization\v2"),
            new ObjectQuery("SELECT ElementName FROM Msvm_VirtualEthernetSwitch"));
        using var switches = searcher.Get();
        var names = switches.Cast<ManagementBaseObject>().Select(item => item["ElementName"] as string).ToList();
        if (!names.Contains("Default Switch"))
        {
            throw new InvalidOperationException("environment: Default Switch is missing; fix the host before running.");
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }
}

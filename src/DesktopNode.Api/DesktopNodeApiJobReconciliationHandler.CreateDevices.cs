using System.Text.Json;

namespace DesktopNode.Api;

// Design pcv-vm-create-reconcile-devices-v1: a create interrupted after DefineSystem leaves a managed VM of the right
// generation without its devices, so the postcondition also needs the disk, ISO and switch the create provider always
// attaches. The expected devices come from the job parameters, which keeps the v1 baseline and covers older jobs.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    internal const string CreateSwitchName = "Default Switch";

    internal static SortedDictionary<string, object?> ExpectedCreateDevices(JsonElement parameters, string vmName)
    {
        var isoPath = DesktopNodeApiJsonReader.ReadString(parameters, "iso_path");
        return new SortedDictionary<string, object?>
        {
            ["disk"] = ExpectedCreateDiskPath(parameters, vmName),
            ["iso"] = string.IsNullOrWhiteSpace(isoPath) ? null : NormalizePath(isoPath),
            ["switch"] = CreateSwitchName
        };
    }

    internal static IReadOnlyList<string> MissingCreateDevices(JsonElement parameters, string vmName, JsonElement observed)
    {
        var missing = new List<string>();
        var disk = ExpectedCreateDiskPath(parameters, vmName);
        if (!Entries(observed, "storage").Any(entry =>
                string.Equals(DesktopNodeApiJsonReader.ReadString(entry, "kind"), "vhdx", StringComparison.OrdinalIgnoreCase) &&
                DesktopNodeApiJsonReader.ReadElement(entry, "attached") is { ValueKind: JsonValueKind.True } &&
                SamePath(DesktopNodeApiJsonReader.ReadString(entry, "path"), disk)))
        {
            missing.Add("disk");
        }

        var isoPath = DesktopNodeApiJsonReader.ReadString(parameters, "iso_path");
        if (string.IsNullOrWhiteSpace(isoPath) ||
            !Entries(observed, "dvd_media").Any(entry => SamePath(DesktopNodeApiJsonReader.ReadString(entry, "path"), isoPath)))
        {
            missing.Add("iso");
        }

        if (!Entries(observed, "network").Any(entry =>
                string.Equals(DesktopNodeApiJsonReader.ReadString(entry, "switch"), CreateSwitchName, StringComparison.OrdinalIgnoreCase)))
        {
            missing.Add("switch");
        }

        return missing;
    }

    internal static string IncompleteCreateHint(string vmName, IReadOnlyList<string> missingDevices) =>
        $"VM '{vmName}' was registered but the create stopped before attaching: {string.Join(", ", missingDevices)}. " +
        "Delete it with the managed vm.delete and run vm.create again with the same name.";

    private static string ExpectedCreateDiskPath(JsonElement parameters, string vmName)
    {
        var vmRoot = DesktopNodeApiJsonReader.ReadString(parameters, "vm_root");
        return NormalizePath(Path.Combine(
            string.IsNullOrWhiteSpace(vmRoot) ? DesktopNode.HyperV.DesktopNodeHyperVVmImportRequest.DefaultVmRoot : vmRoot,
            vmName,
            "disk0.vhdx"));
    }

    private static IEnumerable<JsonElement> Entries(JsonElement observed, string name) =>
        DesktopNodeApiJsonReader.ReadElement(observed, name) is { ValueKind: JsonValueKind.Array } array
            ? array.EnumerateArray().Where(entry => entry.ValueKind == JsonValueKind.Object)
            : [];

    private static bool SamePath(string? observed, string expected) =>
        !string.IsNullOrWhiteSpace(observed) &&
        string.Equals(NormalizePath(observed), NormalizePath(expected), StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path).TrimEnd('\\', '/');
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return path.Trim();
        }
    }
}

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private static void ThrowIfNativeCanceled(CancellationToken cancellationToken, string operation)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return;
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NATIVE_OPERATION_CANCELED",
            "The native Hyper-V operation was canceled before it completed.",
            $"Cancellation was requested while operation '{operation}' was waiting for Hyper-V/WMI.",
            true);
    }

    private static DesktopNodeHyperVOperationResult CanceledResult(string operation)
    {
        return DesktopNodeHyperVOperationResult.Failure(
            operation,
            "PCV_NATIVE_OPERATION_CANCELED",
            "The native Hyper-V operation was canceled before it completed.",
            $"Cancellation was requested while operation '{operation}' was waiting for Hyper-V/WMI.",
            true,
            "Check route or job status and retry only if the operation is safe to repeat.");
    }

    private static bool HasCompleteSwitchTopology(IReadOnlyList<DesktopNodeHyperVSwitchInfo> switches)
    {
        return switches.All(static item =>
        {
            if (string.IsNullOrWhiteSpace(item.Type) ||
                string.Equals(item.Type, "unknown", StringComparison.OrdinalIgnoreCase) ||
                item.AllowManagementOs is null)
            {
                return false;
            }

            return !string.Equals(item.Type, "external", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(item.NetAdapterInterfaceDescription);
        });
    }

    private static bool HasCompleteVmIdentityState(IReadOnlyList<DesktopNodeHyperVVmInfo> vms)
    {
        return vms.All(static item =>
            !string.IsNullOrWhiteSpace(item.Id) &&
            !string.IsNullOrWhiteSpace(item.Name) &&
            !string.IsNullOrWhiteSpace(item.State) &&
            !string.Equals(item.State, "unknown", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasCompleteVmIdentity(IReadOnlyList<DesktopNodeHyperVVmInfo> vms)
    {
        return vms.All(static item =>
            !string.IsNullOrWhiteSpace(item.Id) &&
            !string.IsNullOrWhiteSpace(item.Name));
    }

    private static bool HasCompleteVmSummaryParity(IReadOnlyList<DesktopNodeHyperVVmInfo> vms)
    {
        return vms.All(static item =>
            !string.IsNullOrWhiteSpace(item.Platform) &&
            !string.IsNullOrWhiteSpace(item.GuestFamily) &&
            item.Cpu is not null &&
            item.Cpu.Count.HasValue &&
            item.Memory is not null &&
            item.Memory.StartupMb.HasValue &&
            item.Generation.HasValue &&
            item.Checkpoints is not null &&
            item.Checkpoints.Count.HasValue &&
            item.Console is not null &&
            !string.IsNullOrWhiteSpace(item.Console.Type));
    }

    private static bool HasCompleteCheckpointListParity(IReadOnlyList<DesktopNodeHyperVCheckpointInfo> checkpoints)
    {
        return checkpoints.All(static item =>
            !string.IsNullOrWhiteSpace(item.Name) &&
            !string.IsNullOrWhiteSpace(item.VmName));
    }

    private static bool IsValidHyperVName(string name)
    {
        if (name.Length is < 1 or > 128 || !string.Equals(name, name.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        return name.All(static item => !char.IsControl(item) && item is not '/' and not '\\');
    }

    private static bool IsValidVmCreateName(string name)
    {
        return name.Length is >= 1 and <= 63 &&
            char.IsLetterOrDigit(name[0]) &&
            name.All(static item => char.IsLetterOrDigit(item) || item is '.' or '_' or '-');
    }

    private static DesktopNodeHyperVVmInfo? FindVm(IReadOnlyList<DesktopNodeHyperVVmInfo> vms, string vmName)
    {
        return vms.FirstOrDefault(item =>
            string.Equals(item.Id, vmName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.Name, vmName, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool GetBooleanProperty(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.True;
    }

    private static IReadOnlyList<string> ReadStringList(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var items = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                items.Add(item.GetString() ?? string.Empty);
            }
        }

        return items;
    }

    private static IReadOnlyDictionary<string, string> ReadStringDictionary(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string>();
        }

        var items = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in value.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.String)
            {
                items[item.Name] = item.Value.GetString() ?? string.Empty;
            }
        }

        return items;
    }

    private static bool TryGetStringProperty(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetInt32Property(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetInt32(out value);
        }

        return property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private sealed record DesktopNodeHyperVNetworkInventoryData(
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("mutating")] bool Mutating,
        [property: JsonPropertyName("switches")] IReadOnlyList<DesktopNodeHyperVSwitchInfo> Switches);
}

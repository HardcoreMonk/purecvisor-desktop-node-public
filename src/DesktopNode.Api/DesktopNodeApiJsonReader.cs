using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

// JsonElement 읽기 전담: DesktopNodeApiRequestProcessor에서 추출되어,
// 라우트 소유자가 프로세서를 거치지 않고 요청/프로바이더 페이로드를 읽을 수 있게 한다.
internal static class DesktopNodeApiJsonReader
{
    internal static JsonElement? FindVm(JsonElement? data, string vmId)
    {
        if (data is null)
        {
            return null;
        }

        foreach (var vm in EnumerateVmList(data.Value))
        {
            if (MatchesVmId(vm, vmId))
            {
                return vm.Clone();
            }
        }

        return null;
    }

    internal static JsonElement? ReadNestedElement(
        JsonElement element,
        string parentName,
        string childName)
    {
        var parent = ReadElement(element, parentName);
        return parent is null ? null : ReadElement(parent.Value, childName);
    }

    internal static IEnumerable<JsonElement> EnumerateVmList(JsonElement data)
    {
        if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "vms", "items", "data" })
            {
                if (data.TryGetProperty(propertyName, out var nested))
                {
                    foreach (var item in EnumerateVmList(nested))
                    {
                        yield return item;
                    }

                    yield break;
                }
            }

            yield return data;
        }
    }

    internal static IEnumerable<JsonElement> EnumerateCheckpointList(JsonElement data)
    {
        if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "checkpoints", "items", "data" })
            {
                if (data.TryGetProperty(propertyName, out var nested))
                {
                    foreach (var item in EnumerateCheckpointList(nested))
                    {
                        yield return item;
                    }

                    yield break;
                }
            }
        }
    }

    internal static bool MatchesVmId(JsonElement vm, string vmId)
    {
        foreach (var propertyName in new[] { "id", "name" })
        {
            if (vm.ValueKind == JsonValueKind.Object &&
                vm.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                string.Equals(value.GetString(), vmId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static string? GetStringProperty(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    internal static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    internal static int? ReadInt(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    internal static bool ReadBool(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.True;
    }

    internal static IReadOnlyList<string> ReadStringList(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value))
        {
            return [];
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var item = value.GetString();
            return string.IsNullOrWhiteSpace(item) ? [] : [item];
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var items = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
            {
                items.Add(item.GetString()!);
            }
        }

        return items;
    }

    internal static IReadOnlyDictionary<string, string> ReadStringDictionary(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Object)
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        var items = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            items[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : property.Value.ToString();
        }

        return items;
    }

    internal static JsonElement? ReadElement(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind != JsonValueKind.Null
            ? value.Clone()
            : null;
    }

    internal static DesktopNodeHyperVOperationResult? ReadOperationResult(JsonElement element, string name)
    {
        var value = ReadElement(element, name);
        return value is null
            ? null
            : value.Value.Deserialize<DesktopNodeHyperVOperationResult>(RuntimePolicyContract.JsonOptions);
    }

    internal static DesktopNodeApiError? ReadApiError(JsonElement element, string name)
    {
        var value = ReadElement(element, name);
        return value is null
            ? null
            : value.Value.Deserialize<DesktopNodeApiError>(RuntimePolicyContract.JsonOptions);
    }
}

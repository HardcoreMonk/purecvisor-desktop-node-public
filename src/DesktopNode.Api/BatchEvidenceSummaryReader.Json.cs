using System.Text.RegularExpressions;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed partial class BatchEvidenceSummaryReader
{
    private static decimal? ReadPeakMib(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var values) || values.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        decimal? peak = null;
        foreach (var value in values.EnumerateArray())
        {
            peak = Max(peak, ReadDecimal(value, "mib"));
        }

        return peak;
    }

    private static decimal? Max(decimal? left, decimal? right)
    {
        if (left is null)
        {
            return right;
        }

        if (right is null)
        {
            return left;
        }

        return Math.Max(left.Value, right.Value);
    }

    private static string? ReadString(JsonElement? element, string name)
    {
        if (element is null ||
            element.Value.ValueKind != JsonValueKind.Object ||
            !element.Value.TryGetProperty(name, out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string? ReadString(JsonElement element, string name) => ReadString((JsonElement?)element, name);

    private static string? ReadString(JsonElement? element, string parentName, string childName)
    {
        var parent = ReadElement(element, parentName);
        return ReadString(parent, childName);
    }

    private static bool? ReadBool(JsonElement? element, string name)
    {
        if (element is null ||
            element.Value.ValueKind != JsonValueKind.Object ||
            !element.Value.TryGetProperty(name, out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool? ReadBool(JsonElement element, string name) => ReadBool((JsonElement?)element, name);

    private static int? ReadInt(JsonElement? element, string name)
    {
        return element is not null &&
            element.Value.ValueKind == JsonValueKind.Object &&
            element.Value.TryGetProperty(name, out var value) &&
            value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    private static int? ReadInt(JsonElement element, string name) => ReadInt((JsonElement?)element, name);

    private static decimal? ReadDecimal(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.TryGetDecimal(out var parsed)
            ? parsed
            : null;
    }

    private static JsonElement? ReadElement(JsonElement? element, string name)
    {
        return element is not null &&
            element.Value.ValueKind == JsonValueKind.Object &&
            element.Value.TryGetProperty(name, out var value) &&
            value.ValueKind != JsonValueKind.Null
            ? value.Clone()
            : null;
    }

    private static int? ReadArrayLength(JsonElement? element, string name)
    {
        return element is not null &&
            element.Value.ValueKind == JsonValueKind.Object &&
            element.Value.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : null;
    }

    private static JsonElement JsonFromObject(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, RuntimePolicyContract.JsonOptions));
        return document.RootElement.Clone();
    }
}

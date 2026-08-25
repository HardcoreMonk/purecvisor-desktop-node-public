using System.Text.Json;

namespace DesktopNode.Delivery.Tests.Infrastructure;

internal sealed class JsonContract : IDisposable
{
    private readonly JsonDocument document;
    private readonly string owner;

    private JsonContract(string owner, JsonDocument document)
    {
        this.owner = DeliveryContractError.RequireOwner(owner);
        this.document = document;
    }

    internal JsonElement Root => document.RootElement;

    internal static JsonContract Parse(string owner, string source)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        try
        {
            var document = JsonDocument.Parse(
                source,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 128,
                });
            try
            {
                ValidateUniqueKeys(document.RootElement, normalizedOwner);
                return new JsonContract(normalizedOwner, document);
            }
            catch
            {
                document.Dispose();
                throw;
            }
        }
        catch (JsonException error)
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "json-type", error);
        }
    }

    internal JsonElement RequireObject(JsonElement parent, string propertyName) =>
        RequireProperty(parent, propertyName, JsonValueKind.Object);

    internal JsonElement RequireArray(JsonElement parent, string propertyName) =>
        RequireProperty(parent, propertyName, JsonValueKind.Array);

    internal string RequireString(JsonElement parent, string propertyName) =>
        RequireProperty(parent, propertyName, JsonValueKind.String).GetString()
        ?? throw DeliveryContractError.Invalid(owner, "json-type");

    internal bool RequireBoolean(JsonElement parent, string propertyName) =>
        RequireProperty(parent, propertyName, JsonValueKind.True, JsonValueKind.False).GetBoolean();

    internal int RequireInteger(JsonElement parent, string propertyName)
    {
        var value = RequireProperty(parent, propertyName, JsonValueKind.Number);
        return value.TryGetInt32(out var result)
            ? result
            : throw DeliveryContractError.Invalid(owner, "json-type");
    }

    internal JsonElement RequireProperty(
        JsonElement parent,
        string propertyName,
        params JsonValueKind[] allowedKinds)
    {
        if (parent.ValueKind != JsonValueKind.Object ||
            string.IsNullOrEmpty(propertyName) ||
            !parent.TryGetProperty(propertyName, out var value) ||
            !allowedKinds.Contains(value.ValueKind))
        {
            throw DeliveryContractError.Invalid(owner, "json-type");
        }

        return value;
    }

    internal void RequireExactProperties(JsonElement value, params string[] expected)
    {
        if (value.ValueKind != JsonValueKind.Object ||
            !value.EnumerateObject().Select(property => property.Name).SequenceEqual(expected, StringComparer.Ordinal))
        {
            throw DeliveryContractError.Invalid(owner, "json-type");
        }
    }

    public void Dispose() => document.Dispose();

    private static void ValidateUniqueKeys(JsonElement element, string owner)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw DeliveryContractError.Invalid(owner, "duplicate-json-key");
                }

                ValidateUniqueKeys(property.Value, owner);
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ValidateUniqueKeys(item, owner);
            }
        }
    }
}

internal static class DeliveryContractError
{
    private const string ErrorCode = "PCV_DELIVERY_CONTRACT_INVALID";

    internal static string RequireOwner(string owner)
    {
        if (string.IsNullOrWhiteSpace(owner) ||
            Path.IsPathRooted(owner) ||
            owner.Contains('\\') ||
            owner.Contains('\0') ||
            owner.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new InvalidDataException($"{ErrorCode}|owner|path-containment");
        }

        return owner;
    }

    internal static InvalidDataException Invalid(
        string owner,
        string detail,
        Exception? inner = null) =>
        new($"{ErrorCode}|{RequireOwner(owner)}|{detail}", inner);
}

namespace DesktopNode.Delivery.Tests.Contracts;

internal static class LegacyContractId
{
    private const string ErrorCode = "PCV_DELIVERY_CONTRACT_ID_INVALID";
    private const string TestSuffix = ".Tests.ps1";

    internal static string Create(string domain, string legacyPath, int ordinal)
    {
        if (domain is not ("installer" or "delivery"))
        {
            throw Invalid("domain");
        }

        if (ordinal is < 1 or > 999)
        {
            throw Invalid("ordinal");
        }

        if (string.IsNullOrWhiteSpace(legacyPath) ||
            legacyPath.Contains('\\') ||
            legacyPath.Contains('\0') ||
            legacyPath.StartsWith('/') ||
            legacyPath.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw Invalid("path");
        }

        var fileName = legacyPath.Split('/')[^1];
        if (!fileName.EndsWith(TestSuffix, StringComparison.Ordinal) ||
            fileName.Length <= TestSuffix.Length + 3)
        {
            throw Invalid("filename");
        }

        var stem = fileName[..^TestSuffix.Length];
        if (!stem.StartsWith("Pcv", StringComparison.Ordinal))
        {
            throw Invalid("prefix");
        }

        stem = stem[3..];
        var slug = BuildSlug(stem);
        if (slug.Length == 0)
        {
            throw Invalid("slug");
        }

        return $"pcv.{domain}.{slug}.{ordinal.ToString("000", System.Globalization.CultureInfo.InvariantCulture)}";
    }

    private static string BuildSlug(string stem)
    {
        var slug = new System.Text.StringBuilder(stem.Length + 8);
        char? previousSource = null;
        foreach (var value in stem)
        {
            if (value is '.' or '_' or '-' || char.IsWhiteSpace(value))
            {
                AppendSeparator(slug);
                previousSource = value;
                continue;
            }

            if (!char.IsLetterOrDigit(value))
            {
                throw Invalid("character");
            }

            if (char.IsUpper(value) &&
                previousSource is { } previous &&
                (char.IsLower(previous) || char.IsDigit(previous)))
            {
                AppendSeparator(slug);
            }

            slug.Append(char.ToLowerInvariant(value));
            previousSource = value;
        }

        return slug.ToString().Trim('-');
    }

    private static void AppendSeparator(System.Text.StringBuilder value)
    {
        if (value.Length > 0 && value[^1] != '-')
        {
            value.Append('-');
        }
    }

    private static ArgumentException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");
}

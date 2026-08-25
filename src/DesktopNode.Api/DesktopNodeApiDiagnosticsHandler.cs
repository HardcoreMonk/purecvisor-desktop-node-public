using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

public sealed record DesktopNodeDiagnosticBundleOptions(
    string? DiagnosticsRoot = null,
    int RetentionDays = 14,
    int MaxBundleCount = 50,
    Func<DateTimeOffset>? Clock = null)
{
    public bool Enabled => !string.IsNullOrWhiteSpace(DiagnosticsRoot);

    public DateTimeOffset Now() => Clock?.Invoke() ?? DateTimeOffset.UtcNow;
}

internal sealed class DesktopNodeApiDiagnosticsHandler
{
    private const int DefaultListLimit = 10;
    private const int MaxListLimit = 100;
    private const string BundleFileSuffix = ".bundle.json";
    private const string BundleIdPattern = "^pcv-diag-[0-9]{8}T[0-9]{6}Z-[a-f0-9]{8}$";
    private readonly DesktopNodeDiagnosticBundleOptions options;
    private readonly DesktopNodeJobRuntime? jobRuntime;

    public DesktopNodeApiDiagnosticsHandler(
        DesktopNodeDiagnosticBundleOptions? options = null,
        DesktopNodeJobRuntime? jobRuntime = null)
    {
        this.options = options ?? new DesktopNodeDiagnosticBundleOptions();
        this.jobRuntime = jobRuntime;
    }

    public string? DiagnosticsRoot => options.DiagnosticsRoot;

    public DesktopNodeApiResponse? TryHandle(
        DesktopNodeApiRequest request,
        string method,
        string normalizedPath)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "ListDiagnosticBundles", out _))
        {
            return ListBundles(request.Path);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "CreateDiagnosticBundle", out _))
        {
            return CreateBundle(request);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "DownloadDiagnosticBundle", out var downloadMatch))
        {
            return DownloadBundle(downloadMatch.Parameters["bundleId"]);
        }

        return null;
    }

    private DesktopNodeApiResponse CreateBundle(DesktopNodeApiRequest request)
    {
        if (!options.Enabled)
        {
            return DesktopNodeApiResponseFactory.Failure(
                409,
                "diagnostic.bundle.create",
                "PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED",
                "Diagnostic bundle generation is not configured.",
                "Configure a diagnostics root before using the server-side diagnostic bundle API action.",
                false);
        }

        var diagnosticsRoot = Path.GetFullPath(options.DiagnosticsRoot!);
        Directory.CreateDirectory(diagnosticsRoot);

        var now = options.Now().ToUniversalTime();
        var bundleId = $"pcv-diag-{now:yyyyMMddTHHmmssZ}-{Guid.NewGuid():N}"[..34];
        var archivePath = Path.Combine(diagnosticsRoot, bundleId + BundleFileSuffix);
        var requestId = request.RequestId!;
        var createdAt = now.ToString("o");
        var archivePayload = new SortedDictionary<string, object?>
        {
            ["actual_execution"] = "code-level-api-action",
            ["archive_status"] = "created",
            ["authz_status"] = "token-required-route-contract",
            ["bundle_id"] = bundleId,
            ["client_identity"] = string.IsNullOrWhiteSpace(request.ClientIdentity) ? "unknown" : request.ClientIdentity,
            ["created_at"] = createdAt,
            ["download_status"] = "served-by-download-route",
            ["download_url"] = $"/api/v1/diagnostics/bundles/{bundleId}/download",
            ["external_stable_publication"] = "not-claimed",
            ["host_mutation_performed"] = false,
            ["job_store_recovery"] = BuildJobStoreRecoveryPayload(),
            ["public_trusted_signing"] = "not-claimed",
            ["redaction_status"] = "applied",
            ["request_body_redacted"] = RedactRequestBody(request.Body),
            ["request_id"] = requestId,
            ["retention_status"] = "applied",
            ["scope"] = "diagnostic-bundle-server-code-level"
        };

        File.WriteAllText(archivePath, DesktopNodeApiResponseFactory.SerializeResponsePayload(archivePayload));
        var retention = ApplyRetention(diagnosticsRoot, now);
        var data = new SortedDictionary<string, object?>(archivePayload)
        {
            ["archive_path"] = archivePath,
            ["retention"] = retention
        };

        return DesktopNodeApiResponseFactory.Json(201, DesktopNodeApiResponseFactory.Body(true, "diagnostic.bundle.create", DesktopNodeApiResponseFactory.JsonFromObject(data), null));
    }

    private object BuildJobStoreRecoveryPayload()
    {
        var snapshot = jobRuntime?.Snapshot();
        if (snapshot is null)
        {
            return new SortedDictionary<string, object?>
            {
                ["error_code"] = null,
                ["mutation_blocked"] = false,
                ["recent_events"] = Array.Empty<object>(),
                ["status"] = "not_available"
            };
        }

        return new SortedDictionary<string, object?>
        {
            ["error_code"] = snapshot.StoreHealth.ErrorCode,
            ["mutation_blocked"] = snapshot.StoreHealth.MutationBlocked,
            ["recent_events"] = snapshot.StoreHealth.RecentEvents,
            ["status"] = snapshot.StoreHealth.Status
        };
    }

    private DesktopNodeApiResponse ListBundles(string rawPath)
    {
        if (!options.Enabled)
        {
            return DesktopNodeApiResponseFactory.Failure(
                409,
                "diagnostic.bundle.list",
                "PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED",
                "Diagnostic bundle listing is not configured.",
                "Configure a diagnostics root before using the server-side diagnostic bundle list route.",
                false);
        }

        var page = ParseListPage(rawPath);
        if (!page.Ok)
        {
            return page.Response!;
        }

        var diagnosticsRoot = Path.GetFullPath(options.DiagnosticsRoot!);
        Directory.CreateDirectory(diagnosticsRoot);
        var now = options.Now().ToUniversalTime();
        var retention = ApplyRetention(diagnosticsRoot, now);
        var allBundles = Directory.GetFiles(diagnosticsRoot, "pcv-diag-*" + BundleFileSuffix)
            .Select(path => BuildListRow(new FileInfo(path)))
            .Where(row => row is not null)
            .Select(row => row!)
            .OrderByDescending(row => row.BundleId, StringComparer.Ordinal)
            .ToArray();
        var pageRows = allBundles
            .Skip(page.Offset)
            .Take(page.Limit)
            .Select(row => row.ToPayload())
            .ToArray();
        var nextOffset = page.Offset + pageRows.Length < allBundles.Length
            ? page.Offset + pageRows.Length
            : (int?)null;

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "diagnostic.bundle.list", DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["actual_execution"] = "code-level-api-read",
            ["archive_status"] = "listed",
            ["authz_status"] = "token-required-route-contract",
            ["bundles"] = pageRows,
            ["count"] = allBundles.Length,
            ["default_limit"] = DefaultListLimit,
            ["external_stable_publication"] = "not-claimed",
            ["host_mutation_performed"] = false,
            ["limit"] = page.Limit,
            ["max_limit"] = MaxListLimit,
            ["next_offset"] = nextOffset,
            ["offset"] = page.Offset,
            ["public_trusted_signing"] = "not-claimed",
            ["retention"] = retention,
            ["retention_status"] = "applied",
            ["returned"] = pageRows.Length,
            ["scope"] = "diagnostic-bundle-list-pagination-retention"
        }), null));
    }

    private DesktopNodeApiResponse DownloadBundle(string encodedBundleId)
    {
        if (!options.Enabled)
        {
            return DesktopNodeApiResponseFactory.Failure(
                409,
                "diagnostic.bundle.download",
                "PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED",
                "Diagnostic bundle downloads are not configured.",
                "Configure a diagnostics root before using the server-side diagnostic bundle download route.",
                false);
        }

        var bundleId = Uri.UnescapeDataString(encodedBundleId);
        if (!Regex.IsMatch(bundleId, BundleIdPattern, RegexOptions.CultureInvariant))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.download",
                "PCV_DIAGNOSTIC_BUNDLE_ID_INVALID",
                "The diagnostic bundle id is invalid.",
                "Use the bundle id returned by the diagnostic bundle create route.",
                false);
        }

        var diagnosticsRoot = Path.GetFullPath(options.DiagnosticsRoot!);
        var archivePath = Path.GetFullPath(Path.Combine(diagnosticsRoot, bundleId + BundleFileSuffix));
        var rootWithSeparator = diagnosticsRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!archivePath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.download",
                "PCV_DIAGNOSTIC_BUNDLE_ID_INVALID",
                "The diagnostic bundle id is invalid.",
                "Use the bundle id returned by the diagnostic bundle create route.",
                false);
        }

        if (!File.Exists(archivePath))
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                "diagnostic.bundle.download",
                "PCV_DIAGNOSTIC_BUNDLE_NOT_FOUND",
                $"Diagnostic bundle '{bundleId}' was not found.",
                "Create a new diagnostic bundle before requesting a download.",
                false);
        }

        return new DesktopNodeApiResponse(
            200,
            "application/vnd.purecvisor.diagnostic-bundle+json",
            File.ReadAllText(archivePath),
            new SortedDictionary<string, string>
            {
                ["Content-Disposition"] = $"attachment; filename=\"{bundleId}.bundle.json\"",
                ["X-PCV-Diagnostic-Bundle-Id"] = bundleId
            });
    }

    private object ApplyRetention(string diagnosticsRoot, DateTimeOffset now)
    {
        var retentionDays = Math.Max(1, options.RetentionDays);
        var maxBundleCount = Math.Max(1, options.MaxBundleCount);
        var cutoff = now.AddDays(-retentionDays);
        var bundles = Directory.GetFiles(diagnosticsRoot, "pcv-diag-*" + BundleFileSuffix)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.Name, StringComparer.Ordinal)
            .ToArray();
        var removed = new List<string>();

        foreach (var bundle in bundles)
        {
            var removeByAge = bundle.LastWriteTimeUtc < cutoff.UtcDateTime;
            var removeByCount = Array.IndexOf(bundles, bundle) >= maxBundleCount;
            if (!removeByAge && !removeByCount)
            {
                continue;
            }

            File.Delete(bundle.FullName);
            removed.Add(bundle.Name);
        }

        return new SortedDictionary<string, object?>
        {
            ["cutoff_utc"] = cutoff.ToString("o"),
            ["max_bundle_count"] = maxBundleCount,
            ["removed"] = removed,
            ["retention_days"] = retentionDays
        };
    }

    private static DiagnosticBundleListRow? BuildListRow(FileInfo file)
    {
        if (!file.Name.EndsWith(BundleFileSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var bundleId = file.Name[..^BundleFileSuffix.Length];
        if (!Regex.IsMatch(bundleId, BundleIdPattern, RegexOptions.CultureInvariant))
        {
            return null;
        }

        return new DiagnosticBundleListRow(
            BundleId: bundleId,
            FileName: file.Name,
            CreatedAt: ParseCreatedAt(bundleId, file.LastWriteTimeUtc),
            LastWriteTimeUtc: new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero).ToString("o"),
            SizeBytes: file.Length);
    }

    private static string ParseCreatedAt(string bundleId, DateTime lastWriteTimeUtc)
    {
        var match = Regex.Match(bundleId, "^pcv-diag-([0-9]{8}T[0-9]{6}Z)-", RegexOptions.CultureInvariant);
        if (match.Success &&
            DateTimeOffset.TryParseExact(
                match.Groups[1].Value,
                "yyyyMMdd'T'HHmmss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var createdAt))
        {
            return createdAt.ToString("o");
        }

        return new DateTimeOffset(lastWriteTimeUtc, TimeSpan.Zero).ToString("o");
    }

    private static DiagnosticBundleListPage ParseListPage(string rawPath)
    {
        var limitValue = QueryValue(rawPath, "limit");
        var offsetValue = QueryValue(rawPath, "offset");

        if (!TryParseInt(limitValue, DefaultListLimit, out var limit))
        {
            return DiagnosticBundleListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.list",
                "PCV_DIAGNOSTIC_BUNDLE_LIST_PAGE_INVALID",
                "The diagnostic bundle list pagination query is invalid.",
                "Use integer limit and offset query values.",
                false));
        }

        if (limit < 1 || limit > MaxListLimit)
        {
            return DiagnosticBundleListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.list",
                "PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE",
                $"The diagnostic bundle list limit must be between 1 and {MaxListLimit}.",
                $"Requested limit '{limit}' is outside the supported range.",
                false));
        }

        if (!TryParseInt(offsetValue, 0, out var offset))
        {
            return DiagnosticBundleListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.list",
                "PCV_DIAGNOSTIC_BUNDLE_LIST_PAGE_INVALID",
                "The diagnostic bundle list pagination query is invalid.",
                "Use integer limit and offset query values.",
                false));
        }

        if (offset < 0)
        {
            return DiagnosticBundleListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "diagnostic.bundle.list",
                "PCV_DIAGNOSTIC_BUNDLE_LIST_OFFSET_OUT_OF_RANGE",
                "The diagnostic bundle list offset must be zero or greater.",
                $"Requested offset '{offset}' is outside the supported range.",
                false));
        }

        return DiagnosticBundleListPage.Success(limit, offset);
    }

    private static JsonElement RedactRequestBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["present"] = false
            });
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return DesktopNodeApiResponseFactory.JsonFromObject(RedactJsonValue(document.RootElement, propertyName: null) ?? new SortedDictionary<string, object?>());
        }
        catch (JsonException)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["raw"] = "[REDACTED_INVALID_JSON]"
            });
        }
    }

    private static object? RedactJsonValue(JsonElement element, string? propertyName)
    {
        if (IsSensitiveProperty(propertyName))
        {
            return "[REDACTED]";
        }

        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => RedactJsonValue(property.Value, property.Name),
                StringComparer.Ordinal),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(item => RedactJsonValue(item, propertyName: null))
                .ToArray(),
            JsonValueKind.String => RedactString(element.GetString() ?? string.Empty),
            JsonValueKind.Number => element.TryGetInt64(out var integer) ? integer : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool IsSensitiveProperty(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        return propertyName.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Equals("authorization", StringComparison.OrdinalIgnoreCase);
    }

    private static string RedactString(string value)
    {
        return Regex.Replace(
            value,
            "Bearer\\s+[^\\s\\\"']+",
            "Bearer [REDACTED]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool TryParseInt(string? value, int defaultValue, out int parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = defaultValue;
            return true;
        }

        if (int.TryParse(value, out parsed))
        {
            return true;
        }

        parsed = defaultValue;
        return false;
    }

    private static string? QueryValue(string rawPath, string name)
    {
        var parts = rawPath.Split('?', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
        {
            return null;
        }

        foreach (var pair in parts[1].Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var keyValue = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(keyValue[0].Replace("+", " "));
            if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return keyValue.Length == 2
                ? Uri.UnescapeDataString(keyValue[1].Replace("+", " "))
                : string.Empty;
        }

        return null;
    }

    private sealed record DiagnosticBundleListPage(
        bool Ok,
        int Limit,
        int Offset,
        DesktopNodeApiResponse? Response)
    {
        public static DiagnosticBundleListPage Success(int limit, int offset) => new(true, limit, offset, null);

        public static DiagnosticBundleListPage Fail(DesktopNodeApiResponse response) => new(false, 0, 0, response);
    }

    private sealed record DiagnosticBundleListRow(
        string BundleId,
        string FileName,
        string CreatedAt,
        string LastWriteTimeUtc,
        long SizeBytes)
    {
        public SortedDictionary<string, object?> ToPayload() => new()
        {
            ["archive_status"] = "available",
            ["bundle_id"] = BundleId,
            ["created_at"] = CreatedAt,
            ["download_url"] = $"/api/v1/diagnostics/bundles/{BundleId}/download",
            ["file_name"] = FileName,
            ["last_write_time_utc"] = LastWriteTimeUtc,
            ["redaction_status"] = "applied",
            ["size_bytes"] = SizeBytes
        };
    }
}

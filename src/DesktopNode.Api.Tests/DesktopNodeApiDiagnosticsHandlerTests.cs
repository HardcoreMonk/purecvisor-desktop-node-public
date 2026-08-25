using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeApiDiagnosticsHandlerTests
{
    [Fact]
    public void TryHandleReturnsNullForNonDiagnosticsRoute()
    {
        var handler = new DesktopNodeApiDiagnosticsHandler();

        var response = handler.TryHandle(
            new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", RequestId: "req-not-diagnostics"),
            "GET",
            "/api/v1/runtime/policy");

        Assert.Null(response);
    }

    [Theory]
    [InlineData("GET", "/api/v1/diagnostics/bundles", "diagnostic.bundle.list")]
    [InlineData("POST", "/api/v1/diagnostics/bundles", "diagnostic.bundle.create")]
    [InlineData("GET", "/api/v1/diagnostics/bundles/pcv-diag-20260509T020000Z-dddd4444/download", "diagnostic.bundle.download")]
    public void UnconfiguredOwnerReturnsExistingRootError(
        string method,
        string path,
        string operation)
    {
        var handler = new DesktopNodeApiDiagnosticsHandler();

        var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
            new DesktopNodeApiRequest(method, path, RequestId: "req-unconfigured"),
            method,
            path));

        AssertFailure(response, 409, operation, "PCV_DIAGNOSTIC_BUNDLE_ROOT_NOT_CONFIGURED");
    }

    [Theory]
    [InlineData("limit=invalid", "PCV_DIAGNOSTIC_BUNDLE_LIST_PAGE_INVALID")]
    [InlineData("limit=0", "PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE")]
    [InlineData("limit=101", "PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE")]
    [InlineData("offset=invalid", "PCV_DIAGNOSTIC_BUNDLE_LIST_PAGE_INVALID")]
    [InlineData("offset=-1", "PCV_DIAGNOSTIC_BUNDLE_LIST_OFFSET_OUT_OF_RANGE")]
    public void ListOwnerPreservesPaginationErrors(string query, string errorCode)
    {
        var root = UniqueRoot("pagination");
        try
        {
            var handler = CreateHandler(root);
            var rawPath = "/api/v1/diagnostics/bundles?" + query;

            var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
                new DesktopNodeApiRequest("GET", rawPath, RequestId: "req-page-error"),
                "GET",
                "/api/v1/diagnostics/bundles"));

            AssertFailure(response, 400, "diagnostic.bundle.list", errorCode);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Theory]
    [InlineData("invalid", 400, "PCV_DIAGNOSTIC_BUNDLE_ID_INVALID")]
    [InlineData("%252e%252e%255csecret", 400, "PCV_DIAGNOSTIC_BUNDLE_ID_INVALID")]
    [InlineData("pcv-diag-20260509T020000Z-dddd4444", 404, "PCV_DIAGNOSTIC_BUNDLE_NOT_FOUND")]
    public void DownloadOwnerPreservesIdAndMissingBundleErrors(
        string bundleId,
        int statusCode,
        string errorCode)
    {
        var root = UniqueRoot("download-error");
        try
        {
            var handler = CreateHandler(root);
            var path = $"/api/v1/diagnostics/bundles/{bundleId}/download";

            var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
                new DesktopNodeApiRequest("GET", path, RequestId: "req-download-error"),
                "GET",
                path));

            AssertFailure(response, statusCode, "diagnostic.bundle.download", errorCode);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void CreateOwnerRedactsNestedSensitivePropertiesAndBearerStrings()
    {
        var root = UniqueRoot("redaction-recursive");
        try
        {
            var handler = CreateHandler(root);
            var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
                new DesktopNodeApiRequest(
                    "POST",
                    "/api/v1/diagnostics/bundles",
                    """
                    {"nested":{"password":"secret-value","note":"Bearer bearer-value","safe":"visible"}}
                    """,
                    RequestId: "req-redaction-recursive"),
                "POST",
                "/api/v1/diagnostics/bundles"));

            Assert.Equal(201, response.StatusCode);
            using var responseDocument = JsonDocument.Parse(response.Body);
            var archivePath = responseDocument.RootElement.GetProperty("data").GetProperty("archive_path").GetString();
            using var archiveDocument = JsonDocument.Parse(File.ReadAllText(archivePath!));
            var nested = archiveDocument.RootElement.GetProperty("request_body_redacted").GetProperty("nested");
            Assert.Equal("[REDACTED]", nested.GetProperty("password").GetString());
            Assert.Equal("Bearer [REDACTED]", nested.GetProperty("note").GetString());
            Assert.Equal("visible", nested.GetProperty("safe").GetString());
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Theory]
    [InlineData(null, "present", "false")]
    [InlineData("not-json", "raw", "[REDACTED_INVALID_JSON]")]
    public void CreateOwnerRedactsAbsentAndMalformedRequestBodies(
        string? requestBody,
        string redactionProperty,
        string expectedValue)
    {
        var root = UniqueRoot("redaction");
        try
        {
            var handler = CreateHandler(root);

            var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
                new DesktopNodeApiRequest(
                    "POST",
                    "/api/v1/diagnostics/bundles",
                    requestBody,
                    RequestId: "req-redaction"),
                "POST",
                "/api/v1/diagnostics/bundles"));

            Assert.Equal(201, response.StatusCode);
            using var responseDocument = JsonDocument.Parse(response.Body);
            var archivePath = responseDocument.RootElement.GetProperty("data").GetProperty("archive_path").GetString();
            using var archiveDocument = JsonDocument.Parse(File.ReadAllText(archivePath!));
            var redactedBody = archiveDocument.RootElement.GetProperty("request_body_redacted");
            var redactedValue = redactedBody.GetProperty(redactionProperty);
            var actualValue = redactedValue.ValueKind == JsonValueKind.String
                ? redactedValue.GetString()
                : redactedValue.GetRawText();
            Assert.Equal(expectedValue, actualValue);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void ListOwnerPrunesBundlesByAgeAndIgnoresMalformedBundleNames()
    {
        var root = UniqueRoot("retention-age");
        var now = DateTimeOffset.Parse("2026-05-09T02:15:00Z");
        var expiredBundleId = "pcv-diag-20260401T020000Z-aaaa1111";
        var retainedBundleId = "pcv-diag-20260509T020000Z-bbbb2222";
        try
        {
            Directory.CreateDirectory(root);
            WriteBundle(root, expiredBundleId, now.AddDays(-30));
            WriteBundle(root, retainedBundleId, now.AddMinutes(-15));
            File.WriteAllText(Path.Combine(root, "pcv-diag-malformed.bundle.json"), "{}");
            var handler = new DesktopNodeApiDiagnosticsHandler(new DesktopNodeDiagnosticBundleOptions(
                DiagnosticsRoot: root,
                RetentionDays: 14,
                MaxBundleCount: 50,
                Clock: () => now));

            var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
                new DesktopNodeApiRequest("GET", "/api/v1/diagnostics/bundles", RequestId: "req-retention-age"),
                "GET",
                "/api/v1/diagnostics/bundles"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(1, data.GetProperty("count").GetInt32());
            Assert.Equal(retainedBundleId, data.GetProperty("bundles")[0].GetProperty("bundle_id").GetString());
            Assert.Contains(
                expiredBundleId + ".bundle.json",
                data.GetProperty("retention").GetProperty("removed").EnumerateArray().Select(item => item.GetString()));
            Assert.False(File.Exists(Path.Combine(root, expiredBundleId + ".bundle.json")));
            Assert.True(File.Exists(Path.Combine(root, "pcv-diag-malformed.bundle.json")));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    private static DesktopNodeApiDiagnosticsHandler CreateHandler(string root)
    {
        return new DesktopNodeApiDiagnosticsHandler(new DesktopNodeDiagnosticBundleOptions(
            DiagnosticsRoot: root,
            Clock: () => DateTimeOffset.Parse("2026-05-09T02:15:00Z")));
    }

    private static void AssertFailure(
        DesktopNodeApiResponse response,
        int statusCode,
        string operation,
        string errorCode)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(operation, document.RootElement.GetProperty("operation").GetString());
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(errorCode, error.GetProperty("code").GetString());
        Assert.False(error.GetProperty("retryable").GetBoolean());
    }

    private static string UniqueRoot(string scenario)
    {
        return Path.Combine(Path.GetTempPath(), $"pcv-diag-owner-{scenario}-{Guid.NewGuid():N}");
    }

    private static void WriteBundle(string root, string bundleId, DateTimeOffset lastWriteTime)
    {
        var path = Path.Combine(root, bundleId + ".bundle.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new { bundle_id = bundleId }));
        File.SetLastWriteTimeUtc(path, lastWriteTime.UtcDateTime);
    }

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}

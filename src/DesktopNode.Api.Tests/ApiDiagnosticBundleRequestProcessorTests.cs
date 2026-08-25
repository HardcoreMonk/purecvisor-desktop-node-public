using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiDiagnosticBundleRequestProcessorTests
{
    [Fact]
    public void DiagnosticBundleListReturnsPaginatedRowsAfterRetention()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-diag-list-api-test-" + Guid.NewGuid().ToString("N"));
        var now = DateTimeOffset.Parse("2026-05-09T02:15:00Z");
        var newestBundleId = "pcv-diag-20260509T020000Z-dddd4444";
        var secondBundleId = "pcv-diag-20260509T015000Z-cccc3333";
        var thirdBundleId = "pcv-diag-20260509T014000Z-bbbb2222";
        var prunedBundleId = "pcv-diag-20260509T013000Z-aaaa1111";

        try
        {
            Directory.CreateDirectory(root);
            WriteBundle(root, prunedBundleId, now.AddMinutes(-45));
            WriteBundle(root, thirdBundleId, now.AddMinutes(-35));
            WriteBundle(root, secondBundleId, now.AddMinutes(-25));
            WriteBundle(root, newestBundleId, now.AddMinutes(-15));

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                diagnosticBundleOptions: new DesktopNodeDiagnosticBundleOptions(
                    DiagnosticsRoot: root,
                    RetentionDays: 14,
                    MaxBundleCount: 3,
                    Clock: () => now));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/diagnostics/bundles?limit=2&offset=0",
                RequestId: "req-diag-list",
                ClientIdentity: "operator-a"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("application/json", response.ContentType);

            using var document = JsonDocument.Parse(response.Body);
            var rootElement = document.RootElement;
            Assert.True(rootElement.GetProperty("ok").GetBoolean());
            Assert.Equal("diagnostic.bundle.list", rootElement.GetProperty("operation").GetString());
            Assert.Equal("req-diag-list", rootElement.GetProperty("request_id").GetString());

            var data = rootElement.GetProperty("data");
            Assert.Equal("code-level-api-read", data.GetProperty("actual_execution").GetString());
            Assert.Equal("listed", data.GetProperty("archive_status").GetString());
            Assert.Equal("token-required-route-contract", data.GetProperty("authz_status").GetString());
            Assert.Equal("applied", data.GetProperty("retention_status").GetString());
            Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
            Assert.Equal("not-claimed", data.GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", data.GetProperty("external_stable_publication").GetString());
            Assert.Equal(3, data.GetProperty("count").GetInt32());
            Assert.Equal(2, data.GetProperty("limit").GetInt32());
            Assert.Equal(0, data.GetProperty("offset").GetInt32());
            Assert.Equal(2, data.GetProperty("returned").GetInt32());
            Assert.Equal(2, data.GetProperty("next_offset").GetInt32());

            var bundles = data.GetProperty("bundles").EnumerateArray().ToArray();
            Assert.Equal(newestBundleId, bundles[0].GetProperty("bundle_id").GetString());
            Assert.Equal(secondBundleId, bundles[1].GetProperty("bundle_id").GetString());
            Assert.Equal($"/api/v1/diagnostics/bundles/{newestBundleId}/download", bundles[0].GetProperty("download_url").GetString());
            Assert.True(bundles[0].GetProperty("size_bytes").GetInt64() > 0);

            var retention = data.GetProperty("retention");
            Assert.Equal(3, retention.GetProperty("max_bundle_count").GetInt32());
            Assert.Contains(prunedBundleId + ".bundle.json", retention.GetProperty("removed").EnumerateArray().Select(item => item.GetString()));
            Assert.False(File.Exists(Path.Combine(root, prunedBundleId + ".bundle.json")));

            var nextPage = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/diagnostics/bundles?limit=2&offset=2",
                RequestId: "req-diag-list-2",
                ClientIdentity: "operator-a"));

            using var nextDocument = JsonDocument.Parse(nextPage.Body);
            var nextData = nextDocument.RootElement.GetProperty("data");
            Assert.Equal(1, nextData.GetProperty("returned").GetInt32());
            Assert.Equal(JsonValueKind.Null, nextData.GetProperty("next_offset").ValueKind);
            Assert.Equal(thirdBundleId, nextData.GetProperty("bundles")[0].GetProperty("bundle_id").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void DiagnosticBundleCreateWritesRedactedDownloadableArtifactAndAppliesRetention()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-diag-api-test-" + Guid.NewGuid().ToString("N"));
        var now = DateTimeOffset.Parse("2026-05-08T06:30:00Z");

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "pcv-diag-20260508T050000Z-old.bundle.json"), "{}");
            File.WriteAllText(Path.Combine(root, "pcv-diag-20260508T051000Z-newer.bundle.json"), "{}");

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                diagnosticBundleOptions: new DesktopNodeDiagnosticBundleOptions(
                    DiagnosticsRoot: root,
                    RetentionDays: 14,
                    MaxBundleCount: 2,
                    Clock: () => now));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/diagnostics/bundles",
                """
                {"include":["runtime_policy"],"token":"super-secret","headers":{"Authorization":"Bearer super-secret"}}
                """,
                RequestId: "req-diag-create",
                ClientIdentity: "operator-a"));

            Assert.Equal(201, response.StatusCode);
            Assert.Equal("application/json", response.ContentType);

            using var document = JsonDocument.Parse(response.Body);
            var rootElement = document.RootElement;
            Assert.True(rootElement.GetProperty("ok").GetBoolean());
            Assert.Equal("diagnostic.bundle.create", rootElement.GetProperty("operation").GetString());
            Assert.Equal("req-diag-create", rootElement.GetProperty("request_id").GetString());

            var data = rootElement.GetProperty("data");
            var bundleId = data.GetProperty("bundle_id").GetString();
            Assert.False(string.IsNullOrWhiteSpace(bundleId));
            Assert.Equal("code-level-api-action", data.GetProperty("actual_execution").GetString());
            Assert.Equal("created", data.GetProperty("archive_status").GetString());
            Assert.Equal("served-by-download-route", data.GetProperty("download_status").GetString());
            Assert.Equal("applied", data.GetProperty("redaction_status").GetString());
            Assert.Equal("token-required-route-contract", data.GetProperty("authz_status").GetString());
            Assert.Equal("applied", data.GetProperty("retention_status").GetString());
            Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
            Assert.Equal("not-claimed", data.GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", data.GetProperty("external_stable_publication").GetString());

            var archivePath = data.GetProperty("archive_path").GetString();
            Assert.True(File.Exists(archivePath));
            Assert.Equal($"/api/v1/diagnostics/bundles/{bundleId}/download", data.GetProperty("download_url").GetString());

            var archiveText = File.ReadAllText(archivePath!);
            Assert.DoesNotContain("super-secret", archiveText, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", archiveText, StringComparison.Ordinal);
            Assert.Contains("req-diag-create", archiveText, StringComparison.Ordinal);

            var files = Directory.GetFiles(root, "*.bundle.json").Select(Path.GetFileName).Order().ToArray();
            Assert.Equal(2, files.Length);
            Assert.DoesNotContain("pcv-diag-20260508T050000Z-old.bundle.json", files);

            var download = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                $"/api/v1/diagnostics/bundles/{bundleId}/download",
                RequestId: "req-diag-download",
                ClientIdentity: "operator-a"));

            Assert.Equal(200, download.StatusCode);
            Assert.Equal("application/vnd.purecvisor.diagnostic-bundle+json", download.ContentType);
            Assert.Equal(archiveText, download.Body);
            Assert.NotNull(download.Headers);
            Assert.Equal(bundleId, download.Headers!["X-PCV-Diagnostic-Bundle-Id"]);
            Assert.Contains(bundleId!, download.Headers!["Content-Disposition"], StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void DiagnosticBundleIncludesRedactedBlockedJobStoreRecoveryState()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-diag-job-store-recovery-" + Guid.NewGuid().ToString("N"));
        var jobStorePath = Path.Combine(root, "sensitive-job-store-name.json");
        var diagnosticsRoot = Path.Combine(root, "diagnostics");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                diagnosticBundleOptions: new DesktopNodeDiagnosticBundleOptions(
                    DiagnosticsRoot: diagnosticsRoot,
                    Clock: () => DateTimeOffset.Parse("2026-08-02T00:00:00Z")));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/diagnostics/bundles",
                "{}",
                RequestId: "req-diag-job-store"));

            Assert.Equal(201, response.StatusCode);
            using var responseDocument = JsonDocument.Parse(response.Body);
            var archivePath = responseDocument.RootElement
                .GetProperty("data")
                .GetProperty("archive_path")
                .GetString()!;
            using var archiveDocument = JsonDocument.Parse(File.ReadAllText(archivePath));
            var recovery = archiveDocument.RootElement.GetProperty("job_store_recovery");
            Assert.Equal("blocked", recovery.GetProperty("status").GetString());
            Assert.True(recovery.GetProperty("mutation_blocked").GetBoolean());
            Assert.Equal(
                "PCV_JOB_STORE_SCHEMA_UNSUPPORTED",
                recovery.GetProperty("error_code").GetString());
            Assert.Equal(
                "load-blocked",
                Assert.Single(recovery.GetProperty("recent_events").EnumerateArray())
                    .GetProperty("event")
                    .GetString());
            var archiveText = File.ReadAllText(archivePath);
            Assert.DoesNotContain(jobStorePath, archiveText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("params", archiveText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exception", archiveText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void WriteBundle(string root, string bundleId, DateTimeOffset lastWriteTime)
    {
        var path = Path.Combine(root, bundleId + ".bundle.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new { bundle_id = bundleId, redaction_status = "applied" }));
        File.SetLastWriteTimeUtc(path, lastWriteTime.UtcDateTime);
    }
}

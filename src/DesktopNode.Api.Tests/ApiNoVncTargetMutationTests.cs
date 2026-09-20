using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiNoVncTargetMutationTests
{
    [Fact]
    public void SetQueuesJobWritesFileReloadsCapabilitiesAndSkipsNativeAdapter()
    {
        using var root = new TempNoVncRoot();
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target",
            """{"host":"127.0.0.1","port":5900}""",
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Empty(nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("console.novnc-target.set", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Empty(nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("set", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
        Assert.Equal(
            DesktopNodeNoVncTargetStore.AuditSchema,
            tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("audit").GetProperty("contract").GetString());

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.Equal(NoVncTargetPolicy.Schema, file.RootElement.GetProperty("schema").GetString());
        Assert.True(file.RootElement.GetProperty("enabled").GetBoolean());
        Assert.Equal("127.0.0.1", file.RootElement.GetProperty("host").GetString());
        Assert.Equal(5900, file.RootElement.GetProperty("port").GetInt32());

        var capabilities = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/console/capabilities"));
        Assert.Equal(200, capabilities.StatusCode);
        using var capabilitiesDocument = JsonDocument.Parse(capabilities.Body);
        var data = capabilitiesDocument.RootElement.GetProperty("data");
        var novnc = data.GetProperty("novnc");
        Assert.True(novnc.GetProperty("enabled").GetBoolean());
        Assert.Equal("available", novnc.GetProperty("status").GetString());
        Assert.False(novnc.TryGetProperty("host", out _));
        Assert.False(novnc.TryGetProperty("port", out _));
        var card = data.GetProperty("console_access").GetProperty("novnc");
        Assert.True(card.GetProperty("enabled").GetBoolean());
        Assert.Equal("available", card.GetProperty("status").GetString());
        Assert.Equal("available", card.GetProperty("reason_code").GetString());
        Assert.False(card.TryGetProperty("host", out _));
        Assert.False(card.TryGetProperty("port", out _));
        Assert.False(card.TryGetProperty("allow_lan_target", out _));
    }

    [Fact]
    public void ClearWritesDisabledFileSoPathNameCannotResurrect()
    {
        using var root = new TempNoVncRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-novnc-target-v1",
          "enabled": true,
          "host": "127.0.0.1",
          "port": 5900
        }
        """);
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/clear",
            ServiceBearerAccepted: true));
        Assert.Equal(202, queued.StatusCode);

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Empty(nativeCalls);
        Assert.Equal("clear", tick.Job!.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.False(file.RootElement.GetProperty("enabled").GetBoolean());
        Assert.True(File.Exists(root.FilePath));

        var capabilities = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/console/capabilities"));
        using var capabilitiesDocument = JsonDocument.Parse(capabilities.Body);
        Assert.False(capabilitiesDocument.RootElement.GetProperty("data").GetProperty("novnc").GetProperty("enabled").GetBoolean());

        var bridge = DesktopNodeNoVncTargetStore.ResolveBridgeTarget(root.FilePath, "127.0.0.1", 5900);
        Assert.False(bridge.Enabled);
    }

    [Fact]
    public void SetPreservesExtraJsonAndRejectsMissingConfigurePermission()
    {
        using var root = new TempNoVncRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-novnc-target-v1",
          "enabled": false,
          "lab_tag": "keep-me"
        }
        """);
        var processor = CreateProcessor(root.FilePath);

        var forbidden = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target",
            """{"host":"127.0.0.1","port":5900}"""));
        Assert.Equal(403, forbidden.StatusCode);
        Assert.Contains(NoVncTargetProblemCodes.ConfigureForbidden, forbidden.Body, StringComparison.Ordinal);
        Assert.Contains("console.novnc-target.set", forbidden.Body, StringComparison.Ordinal);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target",
            """{"host":"127.0.0.1","port":5900}""",
            ServiceBearerAccepted: true));
        Assert.Equal(202, queued.StatusCode);
        Assert.True(processor.ProcessOneQueuedJob().Processed);

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.Equal("keep-me", file.RootElement.GetProperty("lab_tag").GetString());
        Assert.True(file.RootElement.GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public void SetRejectsIncompletePairAndDoesNotWriteFile()
    {
        using var root = new TempNoVncRoot();
        var processor = CreateProcessor(root.FilePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target",
            """{"host":"127.0.0.1"}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(NoVncTargetProblemCodes.Incomplete, response.Body, StringComparison.Ordinal);
        Assert.False(File.Exists(root.FilePath));
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    [Fact]
    public void FileOverlayWinsOverPathNameAndMissingFileIsNotConfirmedClear()
    {
        using var root = new TempNoVncRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-novnc-target-v1",
          "enabled": true,
          "host": "127.0.0.1",
          "port": 5901
        }
        """);

        var fromFile = DesktopNodeNoVncTargetStore.ResolveBridgeTarget(root.FilePath, "192.168.1.20", 5900);
        Assert.True(fromFile.Enabled);
        Assert.Equal("127.0.0.1", fromFile.Host);
        Assert.Equal(5901, fromFile.Port);

        var missing = DesktopNodeNoVncTargetStore.ResolveBridgeTarget(
            Path.Combine(root.Directory, "absent.json"),
            "127.0.0.1",
            5900);
        Assert.True(missing.Enabled);
        Assert.Equal("127.0.0.1", missing.Host);
        Assert.Equal(5900, missing.Port);

        var processor = CreateProcessor(root.FilePath);
        var capabilities = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/console/capabilities"));
        using var document = JsonDocument.Parse(capabilities.Body);
        Assert.True(document.RootElement.GetProperty("data").GetProperty("novnc").GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public void WriteFailureRestoresPreviousFileBytes()
    {
        using var root = new TempNoVncRoot();
        var store = new DesktopNodeNoVncTargetStore(new DesktopNodeConsoleOptions(NoVncTargetFilePath: root.FilePath));
        var first = store.Apply(
            "console.novnc-target.set",
            JsonSerializer.SerializeToElement(new { host = "127.0.0.1", port = 5900 }));
        Assert.True(first.Ok);
        var previous = File.ReadAllText(root.FilePath);
        Directory.CreateDirectory(root.FilePath + ".tmp");

        var second = store.Apply(
            "console.novnc-target.set",
            JsonSerializer.SerializeToElement(new { host = "127.0.0.1", port = 5901 }));

        Assert.False(second.Ok);
        Assert.Equal("PCV_NOVNC_TARGET_STORE_WRITE_FAILED", second.Error!.Code);
        Assert.Equal(previous, File.ReadAllText(root.FilePath));
        using var file = JsonDocument.Parse(previous);
        Assert.Equal(5900, file.RootElement.GetProperty("port").GetInt32());
    }

    [Fact]
    public void InterruptedSetReconcileConfirmsFileWithoutRewritingMutation()
    {
        using var root = new TempNoVncRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-novnc-target-v1",
          "enabled": true,
          "host": "127.0.0.1",
          "port": 5900,
          "allow_lan_target": false
        }
        """);
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedNoVncJobStoreJson("console.novnc-target.set", enabledAfter: true, portAfter: 5900));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);
        var before = File.ReadAllText(root.FilePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-novnc-reconcile/reconcile"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(nativeCalls);
        Assert.Equal(before, File.ReadAllText(root.FilePath));
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("succeeded", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal(
            "postcondition-confirmed",
            document.RootElement.GetProperty("data").GetProperty("result").GetProperty("reconciliation").GetProperty("classification").GetString());
    }

    [Fact]
    public void InterruptedSetReconcileRequiresManualActionWhenFileStillMatchesBefore()
    {
        using var root = new TempNoVncRoot();
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedNoVncJobStoreJson("console.novnc-target.set", enabledAfter: true, portAfter: 5900));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-novnc-reconcile/reconcile"));

        Assert.Equal(409, response.StatusCode);
        Assert.Empty(nativeCalls);
        Assert.False(File.Exists(root.FilePath));
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.Contains("not-applied", document.RootElement.GetProperty("error").GetProperty("detail").GetString(), StringComparison.Ordinal);
        Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    [Fact]
    public void InterruptedClearReconcileConfirmsDisabledFileWithoutNativeInvoke()
    {
        using var root = new TempNoVncRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-novnc-target-v1",
          "enabled": false
        }
        """);
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedNoVncJobStoreJson("console.novnc-target.clear", enabledAfter: false, portAfter: null));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-novnc-reconcile/reconcile"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("succeeded", document.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        string filePath,
        List<string>? nativeCalls = null,
        string? jobStorePath = null)
    {
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNoVncNativeAdapter(nativeCalls ?? []),
            jobStorePath: jobStorePath,
            consoleOptions: new DesktopNodeConsoleOptions(NoVncTargetFilePath: filePath));
    }

    private static string InterruptedNoVncJobStoreJson(string operation, bool enabledAfter, int? portAfter)
    {
        var expectedPort = portAfter is null ? "null" : portAfter.Value.ToString();
        return $$"""
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-novnc-reconcile",
              "operation": "{{operation}}",
              "status": "failed",
              "params": {
                "host": "127.0.0.1",
                "port": 5900,
                "reconciliation": {
                  "schema": "pcv-novnc-target-reconciliation/v1",
                  "capture_status": "captured",
                  "before": { "allow_lan_target": false, "enabled": false, "file_present": false, "host": null, "port": null, "reason": null },
                  "expected_after": { "allow_lan_target": false, "enabled": {{enabledAfter.ToString().ToLowerInvariant()}}, "file_present": true, "host": {{(enabledAfter ? "\"127.0.0.1\"" : "null")}}, "port": {{expectedPort}}, "reason": null }
                }
              },
              "result": null,
              "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
              "retry_of": null,
              "request_id": "req-novnc-reconcile",
              "correlation_id": "corr-novnc-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-09-21T00:00:00.0000000Z",
              "updated_at": "2026-09-21T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private sealed class RecordingNoVncNativeAdapter(List<string> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(operation);
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "noVNC target mutations must not invoke Hyper-V.",
                false);
            return false;
        }
    }

    private sealed class TempNoVncRoot : IDisposable
    {
        public string Directory { get; }
        public string FilePath { get; }

        public TempNoVncRoot()
        {
            Directory = Path.Combine(Path.GetTempPath(), "pcv-novnc-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
            FilePath = Path.Combine(Directory, "novnc-target.json");
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Directory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiJobStoreGoldenCharacterizationTests
{
    private const string FixtureName = "job-store-characterization-v1.json";
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    [Fact]
    public void V1AndV2GoldenLoadProduceEquivalentJobProjection()
    {
        using var fixture = LoadFixture();
        using var sandbox = new TestSandbox();
        var scenario = fixture.RootElement.GetProperty("load_and_fifo");
        var expected = scenario.GetProperty("expected_load_projection");
        var actualByVersion = new List<JsonElement>();

        foreach (var version in new[] { 1, 2 })
        {
            var storePath = sandbox.StorePath($"v{version}");
            WriteScenarioStore(storePath, scenario.GetProperty("store"), version);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: storePath);

            var actual = ProjectJobList(processor);

            AssertGolden(expected, actual);
            actualByVersion.Add(actual);
        }

        AssertGolden(actualByVersion[0], actualByVersion[1]);
    }

    [Fact]
    public void PersistedRunningRecoveryMatchesGoldenRewriteAndExcludesRecoveredJobFromQueue()
    {
        using var fixture = LoadFixture();
        using var sandbox = new TestSandbox();
        var scenario = fixture.RootElement.GetProperty("running_recovery");
        var storePath = sandbox.StorePath("running-recovery");
        WriteScenarioStore(storePath, scenario.GetProperty("store"), version: 1);
        var calls = new List<NativeCall>();

        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeAdapter(calls),
            jobStorePath: storePath);

        var actualRewrite = ProjectRunningRecoveryRewrite(
            storePath,
            originalUpdatedAt: scenario
                .GetProperty("store")
                .GetProperty("jobs")[0]
                .GetProperty("updated_at")
                .GetString()!);
        var firstTick = processor.ProcessOneQueuedJob();
        var secondTick = processor.ProcessOneQueuedJob();
        var actualDispatch = ProjectDispatch([firstTick], calls);
        var expected = scenario.GetProperty("expected_rewrite_projection");

        AssertGolden(expected, MergeDispatch(actualRewrite, actualDispatch));
        Assert.True(firstTick.Processed);
        Assert.False(secondTick.Processed);
    }

    [Fact]
    public void PersistedQueueDispatchesGoldenJobsInStoredFifoOrder()
    {
        using var fixture = LoadFixture();
        using var sandbox = new TestSandbox();
        var scenario = fixture.RootElement.GetProperty("load_and_fifo");
        var storePath = sandbox.StorePath("fifo");
        WriteScenarioStore(storePath, scenario.GetProperty("store"), version: 1);
        var calls = new List<NativeCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeAdapter(calls),
            jobStorePath: storePath);

        var ticks = Enumerable.Range(0, 3)
            .Select(_ => processor.ProcessOneQueuedJob())
            .ToArray();
        var exhausted = processor.ProcessOneQueuedJob();
        var actual = ProjectDispatch(ticks, calls);

        Assert.All(ticks, tick => Assert.True(tick.Processed));
        Assert.False(exhausted.Processed);
        AssertGolden(scenario.GetProperty("expected_dispatch"), actual);
    }

    [Fact]
    public void TerminalRetentionWriterMatchesGoldenShapeAndPreservesActiveJob()
    {
        using var fixture = LoadFixture();
        using var sandbox = new TestSandbox();
        var scenario = fixture.RootElement.GetProperty("retention");
        var storePath = sandbox.StorePath("retention");
        WriteRetentionStore(storePath, scenario);

        var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: storePath);
        var listResponse = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=200&offset=0"));
        var actual = ProjectRetentionRewrite(storePath, listResponse);

        AssertGolden(scenario.GetProperty("expected_writer_projection"), actual);
    }

    private static JsonDocument LoadFixture()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "JobStore", FixtureName);
        Assert.True(File.Exists(path), $"Tracked job-store fixture was not copied to '{path}'.");
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static void WriteScenarioStore(string storePath, JsonElement storeElement, int version)
    {
        var store = JsonNode.Parse(storeElement.GetRawText())?.AsObject()
            ?? throw new InvalidOperationException("The tracked store scenario must be a JSON object.");
        store["version"] = version;
        if (version == 2)
        {
            store["migration"] = new JsonObject
            {
                ["plan_id"] = "job-store-v1-to-v2",
                ["plan_version"] = 1,
                ["source_schema_version"] = 1,
                ["target_schema_version"] = 2,
                ["applied_at"] = "2026-07-01T00:00:00.0000000Z"
            };
        }

        File.WriteAllText(storePath, store.ToJsonString(IndentedJson));
    }

    private static void WriteRetentionStore(string storePath, JsonElement scenario)
    {
        var baseTime = DateTimeOffset.Parse(
            scenario.GetProperty("base_time").GetString()!,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
        var terminalCount = scenario.GetProperty("terminal_job_count").GetInt32();
        var terminalStatuses = scenario.GetProperty("terminal_statuses")
            .EnumerateArray()
            .Select(status => status.GetString()!)
            .ToArray();
        var jobs = new List<object?>(terminalCount + 1);

        for (var index = 0; index < terminalCount; index++)
        {
            var timestamp = baseTime.AddMinutes(index).ToString("O", CultureInfo.InvariantCulture);
            var status = terminalStatuses[index % terminalStatuses.Length];
            jobs.Add(new SortedDictionary<string, object?>
            {
                ["attempt"] = 1,
                ["canceled_at"] = status == "canceled" ? timestamp : null,
                ["created_at"] = timestamp,
                ["error"] = status == "failed"
                    ? PersistedError("PCV_TEST_FAILURE")
                    : status == "canceled"
                        ? PersistedError("PCV_JOB_CANCELED")
                        : null,
                ["job_id"] = $"terminal-{index:000}",
                ["operation"] = "vm.start",
                ["params"] = new SortedDictionary<string, object?> { ["name"] = $"terminal-{index:000}" },
                ["result"] = null,
                ["status"] = status,
                ["updated_at"] = timestamp
            });
        }

        jobs.Add(scenario.GetProperty("active_job").Clone());
        var snapshot = new SortedDictionary<string, object?>
        {
            ["jobs"] = jobs,
            ["queue"] = new[] { scenario.GetProperty("active_job").GetProperty("job_id").GetString()! },
            ["saved_at"] = baseTime.ToString("O", CultureInfo.InvariantCulture),
            ["version"] = scenario.GetProperty("store_version").GetInt32()
        };
        File.WriteAllText(storePath, JsonSerializer.Serialize(snapshot, IndentedJson));
    }

    private static SortedDictionary<string, object?> PersistedError(string code)
    {
        return new SortedDictionary<string, object?>
        {
            ["code"] = code,
            ["detail"] = "Golden fixture terminal state.",
            ["message"] = "Golden fixture terminal state.",
            ["retryable"] = false
        };
    }

    private static JsonElement ProjectJobList(DesktopNodeApiRequestProcessor processor)
    {
        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=200&offset=0"));
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        return ToElement(new SortedDictionary<string, object?>
        {
            ["count"] = data.GetProperty("count").GetInt32(),
            ["jobs"] = data.GetProperty("jobs").Clone()
        });
    }

    private static JsonElement ProjectRunningRecoveryRewrite(string storePath, string originalUpdatedAt)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(storePath));
        var root = document.RootElement;
        var jobs = root.GetProperty("jobs");
        var recovered = jobs.EnumerateArray().Single(job => job.GetProperty("job_id").GetString() == "job-running");
        var error = recovered.GetProperty("error");

        return ToElement(new SortedDictionary<string, object?>
        {
            ["version"] = root.GetProperty("version").GetInt32(),
            ["root_fields"] = PropertyNames(root),
            ["job_fields"] = PropertyNames(recovered),
            ["job_count"] = jobs.GetArrayLength(),
            ["queue"] = root.GetProperty("queue").Clone(),
            ["saved_at_is_timestamp"] = IsTimestamp(root.GetProperty("saved_at")),
            ["recovered_job"] = new SortedDictionary<string, object?>
            {
                ["job_id"] = recovered.GetProperty("job_id").GetString(),
                ["operation"] = recovered.GetProperty("operation").GetString(),
                ["status"] = recovered.GetProperty("status").GetString(),
                ["error"] = new SortedDictionary<string, object?>
                {
                    ["fields"] = PropertyNames(error),
                    ["code"] = error.GetProperty("code").GetString(),
                    ["retryable"] = error.GetProperty("retryable").GetBoolean()
                },
                ["result_is_null"] = recovered.GetProperty("result").ValueKind == JsonValueKind.Null,
                ["created_at"] = recovered.GetProperty("created_at").GetString(),
                ["updated_at_changed"] = recovered.GetProperty("updated_at").GetString() != originalUpdatedAt
            }
        });
    }

    private static JsonElement MergeDispatch(JsonElement rewriteProjection, JsonElement dispatch)
    {
        var merged = JsonNode.Parse(rewriteProjection.GetRawText())?.AsObject()
            ?? throw new InvalidOperationException("The recovery projection must be a JSON object.");
        merged["dispatch"] = JsonNode.Parse(dispatch.GetRawText());
        return JsonSerializer.SerializeToElement(merged);
    }

    private static JsonElement ProjectDispatch(
        IReadOnlyList<DesktopNodeApiWorkerTickResult> ticks,
        IReadOnlyList<NativeCall> calls)
    {
        Assert.Equal(ticks.Count, calls.Count);
        var projection = new List<SortedDictionary<string, object?>>(ticks.Count);
        for (var index = 0; index < ticks.Count; index++)
        {
            Assert.NotNull(ticks[index].Job);
            var job = ticks[index].Job!.Value;
            projection.Add(new SortedDictionary<string, object?>
            {
                ["job_id"] = job.GetProperty("job_id").GetString(),
                ["operation"] = calls[index].Operation,
                ["name"] = calls[index].Name
            });
        }

        return ToElement(projection);
    }

    private static JsonElement ProjectRetentionRewrite(string storePath, DesktopNodeApiResponse listResponse)
    {
        Assert.Equal(200, listResponse.StatusCode);
        using var responseDocument = JsonDocument.Parse(listResponse.Body);
        var data = responseDocument.RootElement.GetProperty("data");
        var retention = data.GetProperty("retention");
        using var storeDocument = JsonDocument.Parse(File.ReadAllText(storePath));
        var root = storeDocument.RootElement;
        var jobs = root.GetProperty("jobs").EnumerateArray().ToArray();
        var terminalJobs = jobs
            .Where(job => IsTerminalStatus(job.GetProperty("status").GetString()))
            .OrderBy(job => job.GetProperty("updated_at").GetString(), StringComparer.Ordinal)
            .ToArray();
        var activeJobs = jobs
            .Where(job => !IsTerminalStatus(job.GetProperty("status").GetString()))
            .ToArray();
        var active = Assert.Single(activeJobs);
        var retainedIds = jobs
            .Select(job => job.GetProperty("job_id").GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        return ToElement(new SortedDictionary<string, object?>
        {
            ["version"] = root.GetProperty("version").GetInt32(),
            ["root_fields"] = PropertyNames(root),
            ["job_fields"] = PropertyNames(jobs[0]),
            ["job_count"] = jobs.Length,
            ["terminal_job_count"] = terminalJobs.Length,
            ["active_job_count"] = activeJobs.Length,
            ["queue"] = root.GetProperty("queue").Clone(),
            ["saved_at_is_timestamp"] = IsTimestamp(root.GetProperty("saved_at")),
            ["oldest_retained_terminal_id"] = terminalJobs[0].GetProperty("job_id").GetString(),
            ["newest_retained_terminal_id"] = terminalJobs[^1].GetProperty("job_id").GetString(),
            ["pruned_terminal_ids"] = new[] { "terminal-000", "terminal-001", "terminal-002" }
                .Where(jobId => !retainedIds.Contains(jobId))
                .ToArray(),
            ["active_job"] = new SortedDictionary<string, object?>
            {
                ["job_id"] = active.GetProperty("job_id").GetString(),
                ["operation"] = active.GetProperty("operation").GetString(),
                ["status"] = active.GetProperty("status").GetString(),
                ["correlation_id"] = active.GetProperty("correlation_id").GetString()
            },
            ["api"] = new SortedDictionary<string, object?>
            {
                ["count"] = data.GetProperty("count").GetInt32(),
                ["returned"] = data.GetProperty("returned").GetInt32(),
                ["max_terminal_jobs"] = retention.GetProperty("max_terminal_jobs").GetInt32(),
                ["pruned_terminal_jobs"] = retention.GetProperty("pruned_terminal_jobs").GetInt32(),
                ["active_jobs_preserved"] = retention.GetProperty("active_jobs_preserved").GetBoolean()
            }
        });
    }

    private static string[] PropertyNames(JsonElement element)
    {
        return element.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsTimestamp(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(
                element.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out _);
    }

    private static bool IsTerminalStatus(string? status)
    {
        return status is "succeeded" or "failed" or "canceled";
    }

    private static JsonElement ToElement(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    private static void AssertGolden(JsonElement expected, JsonElement actual)
    {
        var expectedNode = JsonNode.Parse(expected.GetRawText());
        var actualNode = JsonNode.Parse(actual.GetRawText());
        Assert.True(
            JsonNode.DeepEquals(expectedNode, actualNode),
            $"Golden JSON mismatch.{Environment.NewLine}Expected: {expected.GetRawText()}{Environment.NewLine}Actual: {actual.GetRawText()}");
    }

    private sealed record NativeCall(string Operation, string Name);

    private sealed class RecordingNativeAdapter(IList<NativeCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            var name = parameters.GetProperty("name").GetString()!;
            calls.Add(new NativeCall(operation, name));
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: ToElement(new SortedDictionary<string, object?> { ["name"] = name }),
                Error: null);
            return true;
        }
    }

    private sealed class TestSandbox : IDisposable
    {
        private readonly string root = Path.Combine(
            Path.GetTempPath(),
            "pcv-api-jobstore-golden-" + Guid.NewGuid().ToString("N"));

        public TestSandbox()
        {
            Directory.CreateDirectory(root);
        }

        public string StorePath(string name)
        {
            var directory = Path.Combine(root, name);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "jobs.json");
        }

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}

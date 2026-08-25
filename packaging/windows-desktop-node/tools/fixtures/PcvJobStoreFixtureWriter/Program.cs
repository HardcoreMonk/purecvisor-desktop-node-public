using System.Text.Json;
using DesktopNode.Runtime;

if (args.Length != 3 ||
    !int.TryParse(args[1], out var schemaVersion) ||
    schemaVersion is not (1 or 2) ||
    args[2] is not ("terminal" or "queued"))
{
    Console.Error.WriteLine("usage: PcvJobStoreFixtureWriter <store-path> <schema-version:1|2> <terminal|queued>");
    return 2;
}

var storePath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(storePath)!);
if (File.Exists(storePath) || File.Exists(storePath + ".commit-pending"))
{
    Console.Error.WriteLine("PCV_FIXTURE_WRITER_TARGET_EXISTS|Refusing to overwrite an existing fixture target.");
    return 3;
}

if (schemaVersion == 2)
{
    var seed = JsonSerializer.Serialize(new SortedDictionary<string, object?>
    {
        ["jobs"] = Array.Empty<object>(),
        ["queue"] = Array.Empty<string>(),
        ["saved_at"] = "2026-08-02T00:00:00.0000000+00:00",
        ["version"] = 2
    });
    File.WriteAllText(storePath, seed);
}

var runtime = DesktopNodeJobRuntime.CreateDefault(storePath);

if (args[2] == "terminal")
{
    var succeeded = Create(runtime, schemaVersion, "succeeded");
    var succeededStart = runtime.TryStartNext(() => { })
        ?? throw new InvalidOperationException("The succeeded fixture job did not start.");
    runtime.Complete(
        succeededStart,
        new DesktopNodeJobExecutionOutcome(
            true,
            JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["data"] = new SortedDictionary<string, object?>
                {
                    ["action"] = "compatibility-read",
                    ["fixture"] = succeeded.JobId
                },
                ["error"] = null,
                ["ok"] = true,
                ["operation"] = "vm.poweroff"
            }),
            null));

    var failed = Create(runtime, schemaVersion, "failed");
    var failedStart = runtime.TryStartNext(() => { })
        ?? throw new InvalidOperationException("The failed fixture job did not start.");
    var failure = new JobError(
        "PCV_COMPAT_FAILURE",
        "Compatibility fixture failed as expected.",
        "The current writer recorded the frozen-reader failure fixture.",
        false);
    runtime.Complete(
        failedStart,
        new DesktopNodeJobExecutionOutcome(
            false,
            JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["data"] = null,
                ["error"] = failure,
                ["ok"] = false,
                ["operation"] = "vm.poweroff"
            }),
            failure));

    var canceled = Create(runtime, schemaVersion, "canceled");
    var cancelResult = runtime.Cancel(canceled.JobId);
    if (cancelResult.Outcome != DesktopNodeJobCommandOutcome.Canceled)
    {
        throw new InvalidOperationException("The queued cancellation fixture did not become terminal.");
    }
}
else
{
    Create(runtime, schemaVersion, "queue-first");
    Create(runtime, schemaVersion, "queue-second");
}

var snapshot = runtime.Snapshot();
if (snapshot.SchemaVersion != schemaVersion || snapshot.LoadBlock is not null)
{
    Console.Error.WriteLine("PCV_FIXTURE_WRITER_RUNTIME_INVALID|The current runtime did not preserve the requested schema.");
    return 5;
}

var expectedQueue = args[2] == "queued"
    ? new[] { $"pcv-current-v{schemaVersion}-queue-first", $"pcv-current-v{schemaVersion}-queue-second" }
    : Array.Empty<string>();
if (!snapshot.Queue.SequenceEqual(expectedQueue, StringComparer.Ordinal))
{
    Console.Error.WriteLine("PCV_FIXTURE_WRITER_QUEUE_INVALID|The current runtime did not persist the expected FIFO queue.");
    return 6;
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    ok = true,
    schema_version = snapshot.SchemaVersion,
    mode = args[2],
    job_count = snapshot.Jobs.Count,
    queue = snapshot.Queue,
    store_path = storePath
}));
return 0;

static DesktopNodeJobSnapshot Create(DesktopNodeJobRuntime runtime, int schemaVersion, string label)
{
    var jobId = $"pcv-current-v{schemaVersion}-{label}";
    return runtime.Create(
        new DesktopNodeJobCreateCommand(
            "vm.poweroff",
            JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["desired_state"] = label,
                ["vm_id"] = $"pcv-compat-v{schemaVersion}"
            }),
            JobId: jobId),
        new DesktopNodeJobRequestContext(
            $"req-{jobId}",
            $"corr-{jobId}"));
}

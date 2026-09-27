using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

public sealed class DesktopNodeCheckpointScheduleStore
{
    public const string FileSchema = "pcv-checkpoint-schedule-file-v1";
    public const string ReconciliationSchema = "pcv-checkpoint-schedule-reconciliation/v1";
    public const string AuditSchema = "pcv-checkpoint-schedule-audit/v1";

    private static readonly JsonSerializerOptions FileJsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly object gate = new();
    private readonly string? filePath;
    private readonly Dictionary<string, Snapshot> current = new(StringComparer.OrdinalIgnoreCase);

    public DesktopNodeCheckpointScheduleStore(string? filePath)
    {
        this.filePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath;
        LoadFromDiskUnlocked();
    }

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "PureCVisor",
        "desktop-node",
        "checkpoint-schedules.json");

    public JsonElement CaptureReconciliation(string operation, CheckpointScheduleEvaluation evaluation)
    {
        lock (gate)
        {
            var vmName = evaluation.VmName ?? string.Empty;
            current.TryGetValue(vmName, out var before);
            var expected = string.Equals(operation, "checkpoint.schedule.clear", StringComparison.Ordinal)
                ? new Snapshot(false, vmName, null, null, FilePresent: true)
                : new Snapshot(true, vmName, evaluation.IntervalMinutes, evaluation.RetentionMax, FilePresent: true);
            return SerializeObject(new SortedDictionary<string, object?>
            {
                ["schema"] = ReconciliationSchema,
                ["capture_status"] = "captured",
                ["vm_name"] = vmName,
                ["before"] = ToPayload(before with { VmName = vmName }),
                ["expected_after"] = ToPayload(expected)
            });
        }
    }

    public bool MatchesExpected(string vmName, JsonElement expectedAfter)
    {
        lock (gate)
        {
            return SnapshotEquals(GetUnlocked(vmName), ReadSnapshotPayload(expectedAfter));
        }
    }

    public bool MatchesBefore(string vmName, JsonElement before)
    {
        lock (gate)
        {
            return SnapshotEquals(GetUnlocked(vmName), ReadSnapshotPayload(before));
        }
    }

    public bool TryReadCurrent(string vmName, out JsonElement payload)
    {
        lock (gate)
        {
            payload = SerializeObject(ToPayload(GetUnlocked(vmName)));
            return true;
        }
    }

    public IReadOnlyList<CheckpointScheduleState> List()
    {
        lock (gate)
        {
            return current.Values.Select(ToState).ToArray();
        }
    }

    public JsonElement BuildReadback(string vmName, DateTimeOffset now)
    {
        lock (gate)
        {
            return SerializeObject(ToReadback(GetUnlocked(vmName), now));
        }
    }

    public void MarkEnqueued(string vmName, DateTimeOffset now)
    {
        lock (gate)
        {
            var before = GetUnlocked(vmName);
            current[vmName] = before with
            {
                LastEnqueuedAt = now.ToUniversalTime(),
                FilePresent = filePath is not null || before.FilePresent
            };
            if (filePath is not null)
            {
                WriteUnlocked();
            }
        }
    }

    public static bool IsDue(CheckpointScheduleState state, DateTimeOffset now)
    {
        return state.Enabled &&
            state.IntervalMinutes is { } interval &&
            interval > 0 &&
            (state.LastEnqueuedAt is not { } last || now >= last.AddMinutes(interval));
    }

    public DesktopNodeHyperVOperationResult Apply(string operation, JsonElement parameters)
    {
        lock (gate)
        {
            var vmName = ReadString(parameters, "vm_name") ?? string.Empty;
            var isClear = string.Equals(operation, "checkpoint.schedule.clear", StringComparison.Ordinal);
            var before = GetUnlocked(vmName);
            var next = isClear
                ? new Snapshot(false, vmName, null, null, FilePresent: true, LastEnqueuedAt: null)
                : new Snapshot(
                    true,
                    vmName,
                    ReadInt(parameters, "interval_minutes"),
                    ReadInt(parameters, "retention_max"),
                    FilePresent: true,
                    LastEnqueuedAt: DateTimeOffset.UtcNow);

            byte[]? previousBytes = null;
            try
            {
                if (filePath is not null && File.Exists(filePath))
                {
                    previousBytes = File.ReadAllBytes(filePath);
                }

                current[vmName] = next;
                if (filePath is not null)
                {
                    WriteUnlocked();
                }
            }
            catch (Exception exception)
            {
                current[vmName] = before;
                if (filePath is not null && previousBytes is not null)
                {
                    try
                    {
                        File.WriteAllBytes(filePath, previousBytes);
                    }
                    catch (IOException)
                    {
                    }
                }

                return DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_CHECKPOINT_SCHEDULE_STORE_WRITE_FAILED",
                    "The checkpoint schedule file could not be written.",
                    exception.Message,
                    false);
            }

            var data = SerializeObject(new SortedDictionary<string, object?>
            {
                ["action"] = isClear ? CheckpointSchedulePolicy.ActionClear : CheckpointSchedulePolicy.ActionSet,
                ["enabled"] = next.Enabled,
                ["interval_minutes"] = next.IntervalMinutes,
                ["retention_max"] = next.RetentionMax,
                ["vm_name"] = next.VmName,
                ["reload_result"] = "applied",
                ["audit"] = new SortedDictionary<string, object?>
                {
                    ["contract"] = AuditSchema,
                    ["previous"] = ToPayload(before),
                    ["proposed"] = ToPayload(next),
                    ["reload_result"] = "applied"
                }
            });
            return new DesktopNodeHyperVOperationResult(true, operation, data, null);
        }
    }

    private Snapshot GetUnlocked(string vmName)
    {
        return current.TryGetValue(vmName, out var snapshot)
            ? snapshot
            : new Snapshot(false, vmName, null, null, FilePresent: false);
    }

    private void LoadFromDiskUnlocked()
    {
        if (filePath is null || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            if (!document.RootElement.TryGetProperty("schedules", out var schedules) ||
                schedules.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var property in schedules.EnumerateObject())
            {
                current[property.Name] = ReadSnapshotPayload(property.Value) with
                {
                    VmName = property.Name,
                    FilePresent = true
                };
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
        }
    }

    private void WriteUnlocked()
    {
        var path = filePath!;
        JsonObject document;
        if (File.Exists(path))
        {
            document = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
        }
        else
        {
            document = new JsonObject();
        }

        document["schema"] = FileSchema;
        var schedules = document["schedules"] as JsonObject ?? new JsonObject();
        foreach (var pair in current)
        {
            var entry = schedules[pair.Key] as JsonObject ?? new JsonObject();
            entry["schema"] = CheckpointSchedulePolicy.Schema;
            entry["enabled"] = pair.Value.Enabled;
            entry["vm_name"] = pair.Value.VmName;
            entry["interval_minutes"] = pair.Value.IntervalMinutes is { } interval
                ? JsonValue.Create(interval)
                : null;
            entry["retention_max"] = pair.Value.RetentionMax is { } retention
                ? JsonValue.Create(retention)
                : null;
            entry["last_enqueued_at"] = pair.Value.LastEnqueuedAt is { } stamp
                ? JsonValue.Create(stamp.ToUniversalTime().ToString("o"))
                : null;
            schedules[pair.Key] = entry;
        }

        document["schedules"] = schedules;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, document.ToJsonString(FileJsonOptions), new UTF8Encoding(false));
        File.Move(tempPath, path, overwrite: true);
    }

    private static Snapshot ReadSnapshotPayload(JsonElement payload)
    {
        int? interval = null;
        int? retention = null;
        DateTimeOffset? lastEnqueued = null;
        if (payload.ValueKind == JsonValueKind.Object)
        {
            if (payload.TryGetProperty("interval_minutes", out var intervalValue) &&
                intervalValue.ValueKind == JsonValueKind.Number &&
                intervalValue.TryGetInt32(out var parsedInterval))
            {
                interval = parsedInterval;
            }

            if (payload.TryGetProperty("retention_max", out var retentionValue) &&
                retentionValue.ValueKind == JsonValueKind.Number &&
                retentionValue.TryGetInt32(out var parsedRetention))
            {
                retention = parsedRetention;
            }

            if (payload.TryGetProperty("last_enqueued_at", out var lastValue) &&
                lastValue.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(lastValue.GetString(), out var parsedLast))
            {
                lastEnqueued = parsedLast.ToUniversalTime();
            }
        }

        return new Snapshot(
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("enabled", out var enabled) &&
            enabled.ValueKind == JsonValueKind.True,
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("vm_name", out var name) &&
            name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null,
            interval,
            retention,
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("file_present", out var present) &&
            present.ValueKind == JsonValueKind.True,
            lastEnqueued);
    }

    private static bool SnapshotEquals(Snapshot left, Snapshot right)
    {
        return left.Enabled == right.Enabled &&
            left.FilePresent == right.FilePresent &&
            left.IntervalMinutes == right.IntervalMinutes &&
            left.RetentionMax == right.RetentionMax &&
            string.Equals(left.VmName, right.VmName, StringComparison.OrdinalIgnoreCase);
    }

    private static SortedDictionary<string, object?> ToPayload(Snapshot snapshot)
    {
        return new SortedDictionary<string, object?>
        {
            ["enabled"] = snapshot.Enabled,
            ["file_present"] = snapshot.FilePresent,
            ["interval_minutes"] = snapshot.IntervalMinutes,
            ["retention_max"] = snapshot.RetentionMax,
            ["vm_name"] = snapshot.VmName
        };
    }

    private static CheckpointScheduleState ToState(Snapshot snapshot)
    {
        return new CheckpointScheduleState(
            snapshot.VmName ?? string.Empty,
            snapshot.Enabled,
            snapshot.FilePresent,
            snapshot.IntervalMinutes,
            snapshot.RetentionMax,
            snapshot.LastEnqueuedAt);
    }

    private static SortedDictionary<string, object?> ToReadback(Snapshot snapshot, DateTimeOffset now)
    {
        DateTimeOffset? nextDue = null;
        var status = "disabled";
        if (snapshot.Enabled && snapshot.IntervalMinutes is { } interval && interval > 0)
        {
            nextDue = snapshot.LastEnqueuedAt is { } last
                ? last.AddMinutes(interval)
                : now;
            status = now >= nextDue.Value ? "due" : "waiting";
        }

        return new SortedDictionary<string, object?>
        {
            ["configure_via"] = "cli-api",
            ["enabled"] = snapshot.Enabled,
            ["interval_minutes"] = snapshot.IntervalMinutes,
            ["last_enqueued_at"] = snapshot.LastEnqueuedAt?.ToUniversalTime().ToString("o"),
            ["next_due_at"] = nextDue?.ToUniversalTime().ToString("o"),
            ["retention_max"] = snapshot.RetentionMax,
            ["schema"] = CheckpointSchedulePolicy.Schema,
            ["status"] = status,
            ["vm_name"] = snapshot.VmName
        };
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    private static JsonElement SerializeObject(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    private readonly record struct Snapshot(
        bool Enabled,
        string? VmName,
        int? IntervalMinutes,
        int? RetentionMax,
        bool FilePresent,
        DateTimeOffset? LastEnqueuedAt = null);
}

public sealed record CheckpointScheduleState(
    string VmName,
    bool Enabled,
    bool FilePresent,
    int? IntervalMinutes,
    int? RetentionMax,
    DateTimeOffset? LastEnqueuedAt);

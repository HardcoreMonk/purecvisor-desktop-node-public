using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

public sealed class DesktopNodeNoVncTargetStore
{
    public const string ReconciliationSchema = "pcv-novnc-target-reconciliation/v1";
    public const string AuditSchema = "pcv-novnc-target-audit/v1";

    private static readonly JsonSerializerOptions FileJsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly object gate = new();
    private readonly string? filePath;
    private readonly string? webSocketPath;
    private DesktopNodeConsoleOptions live;
    private Snapshot current;

    public DesktopNodeNoVncTargetStore(DesktopNodeConsoleOptions initial)
    {
        live = initial;
        filePath = string.IsNullOrWhiteSpace(initial.NoVncTargetFilePath) ? null : initial.NoVncTargetFilePath;
        webSocketPath = initial.NoVncWebSocketPath;
        if (ReadSnapshotFromDisk() is { } fromFile)
        {
            ApplyLiveUnlocked(fromFile);
        }
        else
        {
            current = new Snapshot(initial.NoVncEnabled, null, null, false, null, FilePresent: false);
        }
    }

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "PureCVisor",
        "desktop-node",
        "novnc-target.json");

    public DesktopNodeConsoleOptions Options
    {
        get
        {
            lock (gate)
            {
                return live;
            }
        }
    }

    public static (bool Enabled, string? Host, int? Port) ResolveBridgeTarget(
        string? configuredFilePath,
        string? pathNameHost,
        int? pathNamePort)
    {
        var path = string.IsNullOrWhiteSpace(configuredFilePath) ? DefaultFilePath : configuredFilePath;
        if (TryReadFile(path) is { } snapshot)
        {
            return (snapshot.Enabled && !string.IsNullOrWhiteSpace(snapshot.Host) && snapshot.Port is not null, snapshot.Host, snapshot.Port);
        }

        var pathNameEnabled = !string.IsNullOrWhiteSpace(pathNameHost) && pathNamePort.HasValue;
        return (pathNameEnabled, pathNameHost, pathNamePort);
    }

    public JsonElement CaptureReconciliation(string operation, NoVncTargetEvaluation evaluation)
    {
        lock (gate)
        {
            var expected = string.Equals(operation, "console.novnc-target.clear", StringComparison.Ordinal)
                ? new Snapshot(false, null, null, false, null, FilePresent: true)
                : new Snapshot(true, evaluation.Host, evaluation.Port, evaluation.AllowLanTarget, evaluation.Reason, FilePresent: true);
            return SerializeObject(new SortedDictionary<string, object?>
            {
                ["schema"] = ReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = ToPayload(current),
                ["expected_after"] = ToPayload(expected)
            });
        }
    }

    public bool TryReadCurrent(out JsonElement payload)
    {
        lock (gate)
        {
            payload = SerializeObject(ToPayload(current));
            return true;
        }
    }

    public DesktopNodeHyperVOperationResult Apply(string operation, JsonElement parameters)
    {
        lock (gate)
        {
            var isClear = string.Equals(operation, "console.novnc-target.clear", StringComparison.Ordinal);
            var before = current;
            var next = isClear
                ? new Snapshot(false, null, null, false, null, FilePresent: true)
                : new Snapshot(
                    true,
                    ReadString(parameters, "host"),
                    ReadInt(parameters, "port"),
                    ReadBool(parameters, "allow_lan_target"),
                    ReadString(parameters, "reason"),
                    FilePresent: true);

            byte[]? previousBytes = null;
            try
            {
                if (filePath is not null && File.Exists(filePath))
                {
                    previousBytes = File.ReadAllBytes(filePath);
                }

                if (filePath is not null)
                {
                    WriteSnapshotUnlocked(next);
                }

                ApplyLiveUnlocked(next);
            }
            catch (Exception exception)
            {
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
                    "PCV_NOVNC_TARGET_STORE_WRITE_FAILED",
                    "The noVNC target file could not be written.",
                    exception.Message,
                    false);
            }

            var data = SerializeObject(new SortedDictionary<string, object?>
            {
                ["action"] = isClear ? NoVncTargetPolicy.ActionClear : NoVncTargetPolicy.ActionSet,
                ["enabled"] = next.Enabled,
                ["host"] = next.Host,
                ["port"] = next.Port,
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

    public bool MatchesExpected(JsonElement expectedAfter)
    {
        lock (gate)
        {
            return SnapshotEquals(current, ReadSnapshotPayload(expectedAfter));
        }
    }

    public bool MatchesBefore(JsonElement before)
    {
        lock (gate)
        {
            return SnapshotEquals(current, ReadSnapshotPayload(before));
        }
    }

    private void WriteSnapshotUnlocked(Snapshot snapshot)
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

        document["schema"] = NoVncTargetPolicy.Schema;
        document["enabled"] = snapshot.Enabled;
        document["host"] = snapshot.Host;
        document["port"] = snapshot.Port;
        document["allow_lan_target"] = snapshot.AllowLanTarget;
        document["reason"] = snapshot.Reason;

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, document.ToJsonString(FileJsonOptions), new UTF8Encoding(false));
        File.Move(tempPath, path, overwrite: true);
    }

    private Snapshot? ReadSnapshotFromDisk()
    {
        return filePath is null ? null : TryReadFile(filePath);
    }

    private static Snapshot? TryReadFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var enabled = root.TryGetProperty("enabled", out var enabledValue) && enabledValue.ValueKind == JsonValueKind.True;
            int? port = null;
            if (root.TryGetProperty("port", out var portValue) &&
                portValue.ValueKind == JsonValueKind.Number &&
                portValue.TryGetInt32(out var parsedPort))
            {
                port = parsedPort;
            }

            return new Snapshot(
                enabled,
                ReadString(root, "host"),
                port,
                root.TryGetProperty("allow_lan_target", out var lan) && lan.ValueKind == JsonValueKind.True,
                ReadString(root, "reason"),
                FilePresent: true);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void ApplyLiveUnlocked(Snapshot snapshot)
    {
        current = snapshot;
        var enabled = snapshot.Enabled && !string.IsNullOrWhiteSpace(snapshot.Host) && snapshot.Port is not null;
        live = live with
        {
            NoVncEnabled = enabled,
            NoVncBridgeMode = enabled ? "websocket-to-vnc-tcp" : "disabled",
            NoVncWebSocketPath = webSocketPath ?? live.NoVncWebSocketPath
        };
    }

    private static SortedDictionary<string, object?> ToPayload(Snapshot snapshot)
    {
        return new SortedDictionary<string, object?>
        {
            ["allow_lan_target"] = snapshot.AllowLanTarget,
            ["enabled"] = snapshot.Enabled,
            ["file_present"] = snapshot.FilePresent,
            ["host"] = snapshot.Host,
            ["port"] = snapshot.Port,
            ["reason"] = snapshot.Reason
        };
    }

    private static Snapshot ReadSnapshotPayload(JsonElement payload)
    {
        int? port = null;
        if (payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("port", out var portValue) &&
            portValue.ValueKind == JsonValueKind.Number &&
            portValue.TryGetInt32(out var parsed))
        {
            port = parsed;
        }

        return new Snapshot(
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("enabled", out var enabled) &&
            enabled.ValueKind == JsonValueKind.True,
            ReadString(payload, "host"),
            port,
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("allow_lan_target", out var lan) &&
            lan.ValueKind == JsonValueKind.True,
            ReadString(payload, "reason"),
            payload.ValueKind == JsonValueKind.Object &&
            payload.TryGetProperty("file_present", out var present) &&
            present.ValueKind == JsonValueKind.True);
    }

    private static bool SnapshotEquals(Snapshot left, Snapshot right)
    {
        return left.Enabled == right.Enabled &&
            left.FilePresent == right.FilePresent &&
            left.Port == right.Port &&
            string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase);
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

    private static bool ReadBool(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.True;
    }

    private static JsonElement SerializeObject(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    private readonly record struct Snapshot(
        bool Enabled,
        string? Host,
        int? Port,
        bool AllowLanTarget,
        string? Reason,
        bool FilePresent);
}

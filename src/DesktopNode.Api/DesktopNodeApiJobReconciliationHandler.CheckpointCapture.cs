using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private JsonElement CaptureCheckpointCreateBaseline(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var expectedBefore = new SortedDictionary<string, object?>
        {
            ["state"] = "absent",
            ["name"] = checkpointName,
            ["vm_name"] = vmName
        };
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["state"] = "present",
            ["name"] = checkpointName,
            ["vm_name"] = vmName
        };

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke(
                "checkpoint.list",
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
                readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = CheckpointCreateReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED",
                    ["before"] = null,
                    ["expected_before"] = expectedBefore,
                    ["expected_after"] = expectedAfter
                });
            }

            var matching = DesktopNodeApiJsonReader.EnumerateCheckpointList(readback.Data.Value)
                .Where(checkpoint =>
                    string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "name"), checkpointName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matching.Length != 0)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = CheckpointCreateReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matching.Length == 1
                        ? "PCV_CHECKPOINT_ALREADY_EXISTS"
                        : "PCV_CHECKPOINT_IDENTITY_AMBIGUOUS",
                    ["before"] = matching.Length == 1 ? matching[0] : null,
                    ["expected_before"] = expectedBefore,
                    ["expected_after"] = expectedAfter
                });
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = CheckpointCreateReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = null,
                ["expected_before"] = expectedBefore,
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = CheckpointCreateReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_CHECKPOINT_LIST_FAILED",
                ["before"] = null,
                ["expected_before"] = expectedBefore,
                ["expected_after"] = expectedAfter
            });
        }
    }

    private JsonElement CaptureCheckpointRestoreBaseline(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var expectedAfter = CheckpointRestoreExpectedAfter(vmName, checkpointName);

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke(
                "checkpoint.list",
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
                readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return UnavailableCheckpointRestoreBaseline(
                    expectedAfter,
                    readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED");
            }

            var matching = MatchingCheckpoints(readback.Data.Value, vmName, checkpointName);
            if (matching.Length == 0)
            {
                return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_NOT_FOUND");
            }

            if (matching.Length != 1)
            {
                return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_IDENTITY_AMBIGUOUS");
            }

            var currentTrue = CurrentTrueCheckpoints(readback.Data.Value);
            if (currentTrue.Length != 1)
            {
                return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_CURRENT_UNAVAILABLE");
            }

            var currentName = DesktopNodeApiJsonReader.GetStringProperty(currentTrue[0], "name");
            if (string.IsNullOrWhiteSpace(currentName))
            {
                return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_CURRENT_UNAVAILABLE");
            }

            if (string.Equals(currentName, checkpointName, StringComparison.OrdinalIgnoreCase))
            {
                return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_ALREADY_CURRENT");
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = CheckpointRestoreReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = new SortedDictionary<string, object?>
                {
                    ["current_name"] = currentName,
                    ["vm_name"] = vmName
                },
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return UnavailableCheckpointRestoreBaseline(expectedAfter, "PCV_CHECKPOINT_LIST_FAILED");
        }
    }

    private static JsonElement UnavailableCheckpointRestoreBaseline(
        SortedDictionary<string, object?> expectedAfter,
        string captureErrorCode)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = CheckpointRestoreReconciliationSchema,
            ["capture_status"] = "unavailable",
            ["capture_error_code"] = captureErrorCode,
            ["before"] = null,
            ["expected_after"] = expectedAfter
        });
    }

    private static SortedDictionary<string, object?> CheckpointRestoreExpectedAfter(string vmName, string checkpointName)
    {
        return new SortedDictionary<string, object?>
        {
            ["current_name"] = checkpointName,
            ["vm_name"] = vmName,
            ["is_current"] = true
        };
    }

    private static JsonElement[] MatchingCheckpoints(JsonElement data, string vmName, string checkpointName)
    {
        return DesktopNodeApiJsonReader.EnumerateCheckpointList(data)
            .Where(checkpoint =>
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "name"), checkpointName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static JsonElement[] CurrentTrueCheckpoints(JsonElement data)
    {
        return DesktopNodeApiJsonReader.EnumerateCheckpointList(data)
            .Where(checkpoint => ReadIsCurrent(checkpoint) == true)
            .ToArray();
    }

    private static bool? ReadIsCurrent(JsonElement checkpoint)
    {
        if (checkpoint.ValueKind != JsonValueKind.Object ||
            !checkpoint.TryGetProperty("is_current", out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool TryReadCapturedCheckpointCreateBaseline(
        JsonElement? metadata,
        string vmName,
        string checkpointName,
        out VmCheckpointCreateBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), CheckpointCreateReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) ||
            DesktopNodeApiJsonReader.ReadElement(value, "before") is not null)
        {
            return false;
        }

        var expectedBefore = DesktopNodeApiJsonReader.ReadElement(value, "expected_before");
        if (expectedBefore is null ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedBefore.Value, "state"), "absent", StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedBefore.Value, "name"), checkpointName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedBefore.Value, "vm_name"), vmName, StringComparison.Ordinal))
        {
            return false;
        }

        baseline = new VmCheckpointCreateBaseline(
            CheckpointCreateReconciliationSchema,
            null);
        return true;
    }

    private static bool TryReadCapturedCheckpointRestoreBaseline(
        JsonElement? metadata,
        string vmName,
        string checkpointName,
        out VmCheckpointRestoreBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), CheckpointRestoreReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (before is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            expectedAfter.Value.ValueKind != JsonValueKind.Object ||
            string.IsNullOrWhiteSpace(DesktopNodeApiJsonReader.ReadString(before.Value, "current_name")) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(before.Value, "vm_name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "current_name"), checkpointName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "vm_name"), vmName, StringComparison.Ordinal) ||
            !expectedAfter.Value.TryGetProperty("is_current", out var isCurrent) ||
            isCurrent.ValueKind != JsonValueKind.True)
        {
            return false;
        }

        baseline = new VmCheckpointRestoreBaseline(
            CheckpointRestoreReconciliationSchema,
            before.Value.Clone(),
            expectedAfter.Value.Clone());
        return true;
    }

    private sealed record VmCheckpointCreateBaseline(
        string Schema,
        JsonElement? Before);

    private sealed record VmCheckpointRestoreBaseline(
        string Schema,
        JsonElement Before,
        JsonElement ExpectedAfter);
}

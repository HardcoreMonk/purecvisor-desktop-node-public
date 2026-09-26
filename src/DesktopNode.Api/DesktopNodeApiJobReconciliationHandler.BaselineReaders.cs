using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private static bool TryReadCapturedRenameBaseline(
        JsonElement? metadata,
        out VmRenameBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmRenameReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        if (before is null || beforeFingerprint is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        baseline = new VmRenameBaseline(
            VmRenameReconciliationSchema,
            before.Value.Clone(),
            beforeFingerprint.Value.Clone());
        return true;
    }

    private static bool TryReadCapturedDeleteBaseline(
        JsonElement? metadata,
        out VmDeleteBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmDeleteReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        if (before is null || beforeFingerprint is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object ||
            string.IsNullOrWhiteSpace(DesktopNodeApiJsonReader.ReadString(before.Value, "id")) ||
            !IsManagedVm(before.Value))
        {
            return false;
        }

        baseline = new VmDeleteBaseline(
            VmDeleteReconciliationSchema,
            before.Value.Clone(),
            beforeFingerprint.Value.Clone());
        return true;
    }

    private static bool TryReadCapturedVmShutdownBaseline(
        JsonElement? metadata,
        string vmName,
        out VmShutdownBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmShutdownReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (before is null ||
            beforeFingerprint is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(NormalizePowerState(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "state")), "off", StringComparison.Ordinal))
        {
            return false;
        }

        baseline = new VmShutdownBaseline(
            VmShutdownReconciliationSchema,
            vmName,
            before.Value.Clone(),
            beforeFingerprint.Value.Clone(),
            NormalizePowerState(DesktopNodeApiJsonReader.GetStringProperty(before.Value, "state")),
            expectedAfter.Value.Clone());
        return true;
    }

    private static bool TryReadCapturedVmRestartBaseline(
        JsonElement? metadata,
        string vmName,
        out VmRestartBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmRestartReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (before is null ||
            beforeFingerprint is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(NormalizePowerState(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "state")), "running", StringComparison.Ordinal))
        {
            return false;
        }

        baseline = new VmRestartBaseline(
            VmRestartReconciliationSchema,
            vmName,
            before.Value.Clone(),
            beforeFingerprint.Value.Clone(),
            DesktopNodeApiJsonReader.GetStringProperty(before.Value, "last_powered_on"),
            expectedAfter.Value.Clone());
        return true;
    }

    private static bool TryReadCapturedVmCreateBaseline(
        JsonElement? metadata,
        string vmName,
        out VmCreateBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmCreateReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) ||
            DesktopNodeApiJsonReader.ReadElement(value, "before") is not null)
        {
            return false;
        }

        var expectedBefore = DesktopNodeApiJsonReader.ReadElement(value, "expected_before");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (expectedBefore is null ||
            expectedAfter is null ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedBefore.Value, "state"), "absent", StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedBefore.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "state"), "present", StringComparison.Ordinal))
        {
            return false;
        }

        if (!expectedAfter.Value.TryGetProperty("generation", out var generationElement) ||
            !generationElement.TryGetInt32(out var generation) ||
            expectedAfter.Value.TryGetProperty("managed_by_purecvisor", out var managed) is false ||
            managed.ValueKind != JsonValueKind.True)
        {
            return false;
        }

        baseline = new VmCreateBaseline(VmCreateReconciliationSchema, vmName, generation, expectedAfter.Value.Clone());
        return true;
    }

    private sealed record VmRenameBaseline(
        string Schema,
        JsonElement Before,
        JsonElement BeforeFingerprint);

    private sealed record VmDeleteBaseline(
        string Schema,
        JsonElement Before,
        JsonElement BeforeFingerprint);

    private sealed record VmCreateBaseline(
        string Schema,
        string Name,
        int Generation,
        JsonElement ExpectedAfter);

    private sealed record VmShutdownBaseline(
        string Schema,
        string Name,
        JsonElement Before,
        JsonElement BeforeFingerprint,
        string BeforeState,
        JsonElement ExpectedAfter);

    private sealed record VmRestartBaseline(
        string Schema,
        string Name,
        JsonElement Before,
        JsonElement BeforeFingerprint,
        string? LastPoweredOn,
        JsonElement ExpectedAfter);
}

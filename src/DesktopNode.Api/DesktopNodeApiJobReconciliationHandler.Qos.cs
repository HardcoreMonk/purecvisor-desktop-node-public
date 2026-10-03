using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private DesktopNodeApiResponse ReconcileVmQosJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var isStorage = string.Equals(job.Operation, "vm.qos.storage.set", StringComparison.Ordinal);
        var operation = isStorage ? "vm.qos.storage.set" : "vm.qos.network.set";
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var targetProperty = isStorage ? "disk" : "adapter";
        var target = DesktopNodeApiJsonReader.ReadString(job.Parameters, targetProperty);
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            string.IsNullOrWhiteSpace(target) ||
            !TryReadCapturedVmQosBaseline(metadata, operation, vmName, target, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    $"The durable {operation} baseline was not captured or is not structurally valid.",
                    operation));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke(
            baseline.ReadOperation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["vm_name"] = vmName
            }),
            readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? (isStorage ? "PCV_VM_BLKIO_GET_FAILED" : "PCV_VM_BANDWIDTH_FAILED");
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider {baseline.ReadOperation} readback failed with {providerCode}; no mutation was attempted.",
                    operation));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = FindQosTargets(readback.Data.Value, target, isStorage);
        if (matching.Length != 1)
        {
            var classification = matching.Length == 0 ? "target-missing" : "ambiguous-duplicate-names";
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    classification,
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        classification,
                        $"Provider {baseline.ReadOperation} readback did not prove a unique QoS target.",
                        operation))));
        }

        var observedPolicy = ReadQosPolicy(matching[0], isStorage);
        if (QosPolicyMatches(observedPolicy, baseline.ExpectedMaximum, baseline.ExpectedMinimum, requireMinimum: baseline.HasExpectedMinimum))
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = operation,
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_after"] = baseline.ExpectedAfter,
                    ["observed"] = matching[0]
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    true,
                    "postcondition-confirmed",
                    result)));
        }

        var classificationUnconfirmed = QosPolicyMatches(observedPolicy, baseline.BeforeMaximum, baseline.BeforeMinimum, requireMinimum: true)
            ? "not-applied"
            : "partial-policy";
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classificationUnconfirmed,
                null,
                ReconciliationRequiredError(
                    jobId,
                    classificationUnconfirmed,
                    $"Provider {baseline.ReadOperation} readback did not prove the captured QoS policy postcondition.",
                    operation))));
    }

    private JsonElement CaptureVmQosBaseline(
        string operation,
        string vmName,
        string targetProperty,
        string target,
        string requiredValueProperty,
        int requiredValue,
        string optionalValueProperty,
        int? optionalValue,
        CancellationToken cancellationToken)
    {
        var isStorage = string.Equals(operation, "vm.qos.storage.set", StringComparison.Ordinal);
        var schema = isStorage ? VmQosStorageReconciliationSchema : VmQosNetworkReconciliationSchema;
        var readOperation = isStorage ? "vm.blkio-get" : "vm.bandwidth";
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            [targetProperty] = target,
            [requiredValueProperty] = requiredValue
        };
        if (optionalValue is not null)
        {
            expectedAfter[optionalValueProperty] = optionalValue.Value;
        }

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke(
                readOperation,
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["vm_name"] = vmName
                }),
                readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = schema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? (isStorage ? "PCV_VM_BLKIO_GET_FAILED" : "PCV_VM_BANDWIDTH_FAILED"),
                    ["before"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var matches = FindQosTargets(readback.Data.Value, target, isStorage);
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = schema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0
                        ? (isStorage ? "PCV_VM_QOS_STORAGE_TARGET_NOT_FOUND" : "PCV_VM_QOS_NETWORK_TARGET_NOT_FOUND")
                        : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var policy = ReadQosPolicy(matches[0], isStorage);
            var before = new SortedDictionary<string, object?>
            {
                ["name"] = vmName,
                [targetProperty] = target,
                [requiredValueProperty] = policy.Maximum,
                [optionalValueProperty] = policy.Minimum
            };
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = schema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = schema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = isStorage ? "PCV_VM_BLKIO_GET_FAILED" : "PCV_VM_BANDWIDTH_FAILED",
                ["before"] = null,
                ["expected_after"] = expectedAfter
            });
        }
    }

    private static JsonElement[] FindQosTargets(JsonElement data, string target, bool isStorage)
    {
        return EnumerateQosDevices(data, isStorage)
            .Select((device, index) => (device, index))
            .Where(item => QosTargetMatches(item.device, target, isStorage, item.index))
            .Select(item => item.device)
            .ToArray();
    }

    private static IEnumerable<JsonElement> EnumerateQosDevices(JsonElement data, bool isStorage)
    {
        var bucketName = isStorage ? "storage_qos" : "network_qos";
        var listName = isStorage ? "disks" : "adapters";
        var bucket = DesktopNodeApiJsonReader.ReadElement(data, bucketName) ?? data;
        var list = DesktopNodeApiJsonReader.ReadElement(bucket, listName);
        if (list is null)
        {
            yield break;
        }

        if (list.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in list.Value.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (list.Value.ValueKind == JsonValueKind.Object)
        {
            yield return list.Value;
        }
    }

    private static bool QosTargetMatches(JsonElement device, string target, bool isStorage, int index)
    {
        if (string.Equals(DesktopNodeApiJsonReader.GetStringProperty(device, isStorage ? "disk" : "adapter"), target, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(DesktopNodeApiJsonReader.GetStringProperty(device, "path"), target, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(DesktopNodeApiJsonReader.GetStringProperty(device, "name"), target, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(DesktopNodeApiJsonReader.GetStringProperty(device, "switch"), target, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var fileName = FileName(DesktopNodeApiJsonReader.GetStringProperty(device, "path"));
        if (string.Equals(fileName, target, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (isStorage)
        {
            return string.Equals(target, "disk" + index.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        var suffix = index.ToString(CultureInfo.InvariantCulture);
        return string.Equals(target, "adapter" + suffix, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(target, "nic" + suffix, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(target, "eth" + suffix, StringComparison.OrdinalIgnoreCase);
    }

    private static string? FileName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var slash = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
        return slash >= 0 ? path[(slash + 1)..] : path;
    }

    private static QosPolicyValues ReadQosPolicy(JsonElement device, bool isStorage)
    {
        var maxProperty = isStorage ? "maximum_iops" : "maximum_kbps";
        var minProperty = isStorage ? "minimum_iops" : "minimum_kbps";
        var source = DesktopNodeApiJsonReader.ReadElement(device, "policy") ?? device;
        return new QosPolicyValues(
            DesktopNodeApiJsonReader.ReadInt(source, maxProperty),
            DesktopNodeApiJsonReader.ReadInt(source, minProperty));
    }

    private static bool QosPolicyMatches(QosPolicyValues observed, int? expectedMaximum, int? expectedMinimum, bool requireMinimum)
    {
        if (observed.Maximum != expectedMaximum)
        {
            return false;
        }

        return !requireMinimum || observed.Minimum == expectedMinimum;
    }

    private readonly record struct QosPolicyValues(int? Maximum, int? Minimum);

    private static bool TryReadCapturedVmQosBaseline(
        JsonElement? metadata,
        string operation,
        string vmName,
        string target,
        out VmQosBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var isStorage = string.Equals(operation, "vm.qos.storage.set", StringComparison.Ordinal);
        var schema = isStorage ? VmQosStorageReconciliationSchema : VmQosNetworkReconciliationSchema;
        var targetProperty = isStorage ? "disk" : "adapter";
        var maxProperty = isStorage ? "maximum_iops" : "maximum_kbps";
        var minProperty = isStorage ? "minimum_iops" : "minimum_kbps";
        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), schema, StringComparison.Ordinal) ||
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
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, targetProperty), target, StringComparison.Ordinal))
        {
            return false;
        }

        var expectedMaximum = DesktopNodeApiJsonReader.ReadInt(expectedAfter.Value, maxProperty);
        if (expectedMaximum is null)
        {
            return false;
        }

        var expectedMinimum = DesktopNodeApiJsonReader.ReadInt(expectedAfter.Value, minProperty);
        var hasExpectedMinimum = expectedAfter.Value.TryGetProperty(minProperty, out _);
        baseline = new VmQosBaseline(
            schema,
            operation,
            isStorage ? "vm.blkio-get" : "vm.bandwidth",
            vmName,
            target,
            before.Value.Clone(),
            expectedAfter.Value.Clone(),
            DesktopNodeApiJsonReader.ReadInt(before.Value, maxProperty),
            DesktopNodeApiJsonReader.ReadInt(before.Value, minProperty),
            expectedMaximum,
            expectedMinimum,
            hasExpectedMinimum);
        return true;
    }

    private sealed record VmQosBaseline(
        string Schema,
        string Operation,
        string ReadOperation,
        string Name,
        string Target,
        JsonElement Before,
        JsonElement ExpectedAfter,
        int? BeforeMaximum,
        int? BeforeMinimum,
        int? ExpectedMaximum,
        int? ExpectedMinimum,
        bool HasExpectedMinimum);
}

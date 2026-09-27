using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 조정 경로와 baseline 캡처는 같은 스키마 상수와 fingerprint 를
// 공유하므로 한 소유자가 갖는다. Build*Parameters 는 큐 등록 시점에 provider readback 으로
// baseline 을 캡처하므로 mutation 경로가 이 소유자를 소비한다 - 방향은 한쪽뿐이다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly DesktopNodeApiHardeningOptions hardeningOptions;
    private readonly DesktopNodeNoVncTargetStore noVncTargetStore;
    private readonly DesktopNodeCheckpointScheduleStore checkpointScheduleStore;

    public DesktopNodeApiJobReconciliationHandler(
        DesktopNodeJobRuntime jobRuntime,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiHardeningOptions hardeningOptions,
        DesktopNodeNoVncTargetStore noVncTargetStore,
        DesktopNodeCheckpointScheduleStore checkpointScheduleStore)
    {
        this.jobRuntime = jobRuntime;
        this.operationInvoker = operationInvoker;
        this.hardeningOptions = hardeningOptions;
        this.noVncTargetStore = noVncTargetStore;
        this.checkpointScheduleStore = checkpointScheduleStore;
    }

    public DesktopNodeApiResponse? TryHandle(string method, string normalizedPath, CancellationToken cancellationToken)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "ReconcileJob", out var match))
        {
            return HandleJobReconcile(match.Parameters["jobId"], cancellationToken);
        }

        return null;
    }

    private const string VmRenameReconciliationSchema = "pcv-vm-rename-reconciliation/v1";
    private const string VmDeleteReconciliationSchema = "pcv-vm-delete-reconciliation/v1";
    private const string CheckpointCreateReconciliationSchema = "pcv-checkpoint-create-reconciliation/v1";
    private const string CheckpointRestoreReconciliationSchema = "pcv-checkpoint-restore-reconciliation/v1";
    private const string VmCreateReconciliationSchema = "pcv-vm-create-reconciliation/v1";
    private const string VmShutdownReconciliationSchema = "pcv-vm-shutdown-reconciliation/v1";
    private const string VmRestartReconciliationSchema = "pcv-vm-restart-reconciliation/v1";
    private const string VmQosStorageReconciliationSchema = "pcv-vm-qos-storage-reconciliation/v1";
    private const string VmQosNetworkReconciliationSchema = "pcv-vm-qos-network-reconciliation/v1";

    private DesktopNodeApiResponse HandleJobReconcile(
        string jobId,
        CancellationToken cancellationToken)
    {
        var current = jobRuntime.Get(jobId);
        if (current.Outcome == DesktopNodeJobCommandOutcome.NotFound)
        {
            return DesktopNodeApiResponseFactory.Json(404, DesktopNodeApiResponseFactory.Body(false, "job.reconcile", null, DesktopNodeApiErrorMapping.ToApiError(current.Error)));
        }

        if (current.Job is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                409,
                "job.reconcile",
                "PCV_JOB_RECONCILIATION_REQUIRED",
                "The job cannot be reconciled.",
                "The job runtime returned no current snapshot for the requested reconciliation.",
                false,
                "Inspect the job store and diagnostics before submitting another mutation.");
        }

        var job = current.Job;
        if (string.Equals(job.Operation, "vm.delete", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileVmDeleteJob(job, cancellationToken);
        }

        if (string.Equals(job.Operation, "checkpoint.create", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileCheckpointCreateJob(job, cancellationToken);
        }

        if (string.Equals(job.Operation, "checkpoint.restore", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileCheckpointRestoreJob(job, cancellationToken);
        }

        if (string.Equals(job.Operation, "vm.create", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileVmCreateJob(job, cancellationToken);
        }

        if (string.Equals(job.Operation, "vm.shutdown", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileVmShutdownJob(job, cancellationToken);
        }

        if (string.Equals(job.Operation, "vm.restart", StringComparison.Ordinal) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileVmRestartJob(job, cancellationToken);
        }

        if ((string.Equals(job.Operation, "vm.qos.storage.set", StringComparison.Ordinal) ||
                string.Equals(job.Operation, "vm.qos.network.set", StringComparison.Ordinal)) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileVmQosJob(job, cancellationToken);
        }

        if ((string.Equals(job.Operation, "console.novnc-target.set", StringComparison.Ordinal) ||
                string.Equals(job.Operation, "console.novnc-target.clear", StringComparison.Ordinal)) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileNoVncTargetJob(job);
        }

        if ((string.Equals(job.Operation, "checkpoint.schedule.set", StringComparison.Ordinal) ||
                string.Equals(job.Operation, "checkpoint.schedule.clear", StringComparison.Ordinal)) &&
            string.Equals(job.Status, "failed", StringComparison.Ordinal) &&
            string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            return ReconcileCheckpointScheduleJob(job);
        }

        if (!string.Equals(job.Operation, "vm.rename", StringComparison.Ordinal) ||
            !string.Equals(job.Status, "failed", StringComparison.Ordinal) ||
            !string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "job-not-reconcilable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "job-not-reconcilable",
                    "Only a failed vm.rename, vm.delete, checkpoint.create, checkpoint.restore, vm.create, vm.shutdown, vm.restart, vm.qos.storage.set, vm.qos.network.set, console.novnc-target.set, console.novnc-target.clear, checkpoint.schedule.set, or checkpoint.schedule.clear job with PCV_JOB_INTERRUPTED can be reconciled.",
                    job.Operation));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var oldName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var newName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "new_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName) ||
            !TryReadCapturedRenameBaseline(metadata, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.rename baseline was not captured or is not structurally valid."));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted."));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matchingOld = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), oldName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var matchingNew = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), newName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var targetMatchesBaseline = matchingNew.Length == 1 &&
            RenameFingerprintMatches(baseline.BeforeFingerprint, matchingNew[0]);
        var oldMatchesBaseline = matchingOld.Length == 1 &&
            RenameFingerprintMatches(baseline.BeforeFingerprint, matchingOld[0]);

        if (targetMatchesBaseline && matchingOld.Length == 0)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "vm.rename",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName },
                    ["observed"] = matchingNew[0]
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    true,
                    "postcondition-confirmed",
                    result)));
        }

        var classification = oldMatchesBaseline && matchingNew.Length == 0
            ? "not-applied"
            : matchingOld.Length > 0 && matchingNew.Length > 0
                ? "ambiguous-both-names-present"
                : matchingNew.Length == 1
                    ? "target-fingerprint-mismatch"
                    : "expected-target-not-observed";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider readback did not prove a unique renamed VM with the captured pre-state fingerprint."));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileNoVncTargetJob(DesktopNodeJobSnapshot job)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (metadata is null ||
            metadata.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(metadata.Value, "schema"), DesktopNodeNoVncTargetStore.ReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(metadata.Value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    "baseline-unavailable",
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        "baseline-unavailable",
                        $"The durable {operation} baseline was not captured or is not structurally valid.",
                        operation))));
        }

        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(metadata.Value, "expected_after");
        var before = DesktopNodeApiJsonReader.ReadElement(metadata.Value, "before");
        if (expectedAfter is null || before is null)
        {
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    "baseline-unavailable",
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        "baseline-unavailable",
                        $"The durable {operation} baseline was not captured or is not structurally valid.",
                        operation))));
        }

        if (noVncTargetStore.MatchesExpected(expectedAfter.Value))
        {
            noVncTargetStore.TryReadCurrent(out var observed);
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = operation,
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = DesktopNodeNoVncTargetStore.ReconciliationSchema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = before.Value,
                    ["expected_after"] = expectedAfter.Value,
                    ["observed"] = observed
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
        }

        var classification = noVncTargetStore.MatchesBefore(before.Value) ? "not-applied" : "partial-policy";
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(
                    jobId,
                    classification,
                    $"The noVNC target file did not prove the captured {operation} postcondition.",
                    operation))));
    }

    private DesktopNodeApiResponse ReconcileCheckpointScheduleJob(DesktopNodeJobSnapshot job)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name") ?? string.Empty;
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (metadata is null ||
            metadata.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(metadata.Value, "schema"), DesktopNodeCheckpointScheduleStore.ReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(metadata.Value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    "baseline-unavailable",
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        "baseline-unavailable",
                        $"The durable {operation} baseline was not captured or is not structurally valid.",
                        operation))));
        }

        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(metadata.Value, "expected_after");
        var before = DesktopNodeApiJsonReader.ReadElement(metadata.Value, "before");
        if (expectedAfter is null || before is null)
        {
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    "baseline-unavailable",
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        "baseline-unavailable",
                        $"The durable {operation} baseline was not captured or is not structurally valid.",
                        operation))));
        }

        if (checkpointScheduleStore.MatchesExpected(vmName, expectedAfter.Value))
        {
            checkpointScheduleStore.TryReadCurrent(vmName, out var observed);
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = operation,
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = DesktopNodeCheckpointScheduleStore.ReconciliationSchema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = before.Value,
                    ["expected_after"] = expectedAfter.Value,
                    ["observed"] = observed
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
        }

        var classification = checkpointScheduleStore.MatchesBefore(vmName, before.Value) ? "not-applied" : "partial-policy";
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(
                    jobId,
                    classification,
                    $"The checkpoint schedule file did not prove the captured {operation} postcondition.",
                    operation))));
    }

    private DesktopNodeApiResponse RenderReconciliationResult(DesktopNodeJobReconciliationResult result)
    {
        return result.Outcome switch
        {
            DesktopNodeJobReconciliationOutcome.NotFound => DesktopNodeApiResponseFactory.Json(404, DesktopNodeApiResponseFactory.Body(false, "job.reconcile", null, DesktopNodeApiErrorMapping.ToApiError(result.Error))),
            DesktopNodeJobReconciliationOutcome.Reconciled => DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "job.reconcile", DesktopNodeApiResponseFactory.JobData(result.Job!), null)),
            _ => DesktopNodeApiResponseFactory.Json(409, DesktopNodeApiResponseFactory.Body(false, "job.reconcile", result.Job is null ? null : DesktopNodeApiResponseFactory.JobData(result.Job), DesktopNodeApiErrorMapping.ToApiError(result.Error)))
        };
    }

    private static DesktopNodeJobRuntimeError ReconciliationRequiredError(
        string jobId,
        string classification,
        string detail,
        string? operation = null)
    {
        var mutation = operation switch
        {
            "vm.delete" => "delete",
            "vm.create" => "create",
            "vm.shutdown" => "shutdown",
            "vm.restart" => "restart",
            "vm.qos.storage.set" => "storage QoS",
            "vm.qos.network.set" => "network QoS",
            "console.novnc-target.set" => "noVNC target",
            "console.novnc-target.clear" => "noVNC clear",
            "checkpoint.schedule.set" => "checkpoint schedule",
            "checkpoint.schedule.clear" => "checkpoint schedule clear",
            "checkpoint.create" => "checkpoint create",
            "checkpoint.restore" => "checkpoint restore",
            _ => "rename"
        };
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_RECONCILIATION_REQUIRED",
            $"Job '{jobId}' requires operator reconciliation.",
            $"{detail} Classification: {classification}.",
            false,
            $"Inspect provider readback and Event Log/diagnostics, confirm whether the {mutation} applied, and do not submit a duplicate mutation until the side effect is known.");
    }

    public JsonElement BuildVmRenameParameters(
        string oldName,
        string newName,
        CancellationToken cancellationToken)
    {
        var reconciliation = CaptureVmRenameBaseline(oldName, newName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = oldName,
            ["new_name"] = newName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildVmDeleteParameters(
        string vmName,
        CancellationToken cancellationToken)
    {
        var reconciliation = CaptureVmDeleteBaseline(vmName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildCheckpointCreateParameters(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var reconciliation = CaptureCheckpointCreateBaseline(vmName, checkpointName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["checkpoint_name"] = checkpointName,
            ["vm_name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildCheckpointRestoreParameters(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var reconciliation = CaptureCheckpointRestoreBaseline(vmName, checkpointName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["checkpoint_name"] = checkpointName,
            ["vm_name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildVmCreateParameters(JsonElement body, CancellationToken cancellationToken)
    {
        var name = DesktopNodeApiJsonReader.GetStringProperty(body, "name") ?? string.Empty;
        var generation = 2;
        if (body.ValueKind == JsonValueKind.Object &&
            body.TryGetProperty("generation", out var generationElement) &&
            generationElement.TryGetInt32(out var parsedGeneration))
        {
            generation = parsedGeneration;
        }

        var payload = body.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(body.GetRawText()) as JsonObject ?? new JsonObject()
            : new JsonObject();
        payload["reconciliation"] = JsonNode.Parse(
            CaptureVmCreateBaseline(name, generation, cancellationToken).GetRawText());
        using var document = JsonDocument.Parse(payload.ToJsonString());
        return document.RootElement.Clone();
    }

    public JsonElement BuildVmShutdownParameters(string vmName, CancellationToken cancellationToken)
    {
        var reconciliation = CaptureVmShutdownBaseline(vmName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildVmRestartParameters(string vmName, CancellationToken cancellationToken)
    {
        var reconciliation = CaptureVmRestartBaseline(vmName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    public JsonElement BuildVmQosParameters(
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
        var reconciliation = CaptureVmQosBaseline(
            operation,
            vmName,
            targetProperty,
            target,
            requiredValueProperty,
            requiredValue,
            optionalValueProperty,
            optionalValue,
            cancellationToken);
        var parameters = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            [targetProperty] = target,
            [requiredValueProperty] = requiredValue,
            ["rollback_descriptor_required"] = true,
            ["readback_after_apply_required"] = true,
            ["reconciliation"] = reconciliation
        };
        if (optionalValue is not null)
        {
            parameters[optionalValueProperty] = optionalValue.Value;
        }

        return DesktopNodeApiResponseFactory.JsonFromObject(parameters);
    }

    private static JsonElement BuildVmRenameFingerprint(JsonElement vm)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["platform"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "platform"),
            ["guest_family"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "guest_family"),
            ["state"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "state"),
            ["cpu_count"] = DesktopNodeApiJsonReader.ReadNestedElement(vm, "cpu", "count"),
            ["startup_memory_mb"] = DesktopNodeApiJsonReader.ReadNestedElement(vm, "memory", "startup_mb"),
            ["generation"] = DesktopNodeApiJsonReader.ReadElement(vm, "generation"),
            ["managed_by_purecvisor"] = DesktopNodeApiJsonReader.ReadElement(vm, "managed_by_purecvisor")
        });
    }

    private static JsonElement BuildVmShutdownIdentityFingerprint(JsonElement vm)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["platform"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "platform"),
            ["guest_family"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "guest_family"),
            ["cpu_count"] = DesktopNodeApiJsonReader.ReadNestedElement(vm, "cpu", "count"),
            ["startup_memory_mb"] = DesktopNodeApiJsonReader.ReadNestedElement(vm, "memory", "startup_mb"),
            ["generation"] = DesktopNodeApiJsonReader.ReadElement(vm, "generation"),
            ["managed_by_purecvisor"] = DesktopNodeApiJsonReader.ReadElement(vm, "managed_by_purecvisor")
        });
    }

    private static bool ShutdownIdentityMatches(JsonElement beforeFingerprint, JsonElement observed)
    {
        if (beforeFingerprint.ValueKind != JsonValueKind.Object || observed.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return JsonNode.DeepEquals(
            JsonNode.Parse(beforeFingerprint.GetRawText()),
            JsonNode.Parse(BuildVmShutdownIdentityFingerprint(observed).GetRawText()));
    }

    private static string NormalizePowerState(string? state)
    {
        var value = (state ?? string.Empty).Trim().ToLowerInvariant();
        if (value is "off" or "stopped")
        {
            return "off";
        }

        if (value.Contains("running", StringComparison.Ordinal))
        {
            return "running";
        }

        return value;
    }

    private static bool TryReadTimestamp(string? value, out DateTimeOffset timestamp)
    {
        timestamp = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out timestamp);
    }

    private static JsonElement BuildVmDeleteFingerprint(JsonElement vm)
    {
        return BuildVmRenameFingerprint(vm);
    }

    private static bool IsManagedVm(JsonElement vm)
    {
        var marker = DesktopNodeApiJsonReader.ReadElement(vm, "managed_by_purecvisor");
        return marker is not null && marker.Value.ValueKind == JsonValueKind.True;
    }

    private static bool CreateFingerprintMatches(VmCreateBaseline baseline, JsonElement observed)
    {
        if (!IsManagedVm(observed))
        {
            return false;
        }

        var generation = DesktopNodeApiJsonReader.ReadElement(observed, "generation");
        return generation is not null &&
            generation.Value.ValueKind == JsonValueKind.Number &&
            generation.Value.TryGetInt32(out var observedGeneration) &&
            observedGeneration == baseline.Generation;
    }

    private static bool RenameFingerprintMatches(JsonElement beforeFingerprint, JsonElement observed)
    {
        if (beforeFingerprint.ValueKind != JsonValueKind.Object || observed.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return JsonNode.DeepEquals(
            JsonNode.Parse(beforeFingerprint.GetRawText()),
            JsonNode.Parse(BuildVmRenameFingerprint(observed).GetRawText()));
    }
}

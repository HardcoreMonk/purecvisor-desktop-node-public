using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 조정 경로와 baseline 캡처는 같은 스키마 상수와 fingerprint 를
// 공유하므로 한 소유자가 갖는다. Build*Parameters 는 큐 등록 시점에 provider readback 으로
// baseline 을 캡처하므로 mutation 경로가 이 소유자를 소비한다 - 방향은 한쪽뿐이다.
internal sealed class DesktopNodeApiJobReconciliationHandler
{
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly DesktopNodeApiHardeningOptions hardeningOptions;

    public DesktopNodeApiJobReconciliationHandler(
        DesktopNodeJobRuntime jobRuntime,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiHardeningOptions hardeningOptions)
    {
        this.jobRuntime = jobRuntime;
        this.operationInvoker = operationInvoker;
        this.hardeningOptions = hardeningOptions;
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
                    "Only a failed vm.rename, vm.delete, checkpoint.create, or checkpoint.restore job with PCV_JOB_INTERRUPTED can be reconciled.",
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

    private DesktopNodeApiResponse ReconcileVmDeleteJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedDeleteBaseline(metadata, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.delete baseline was not captured or is not structurally valid.",
                    "vm.delete"));
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
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.",
                    "vm.delete"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matching.Length == 0)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "vm.delete",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    },
                    ["observed"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    true,
                    "postcondition-confirmed",
                    result)));
        }

        var beforeId = DesktopNodeApiJsonReader.ReadString(baseline.Before, "id");
        var classification = matching.Length > 1
            ? "ambiguous-multiple-targets"
            : string.Equals(beforeId, DesktopNodeApiJsonReader.GetStringProperty(matching[0], "id"), StringComparison.Ordinal) &&
                IsManagedVm(matching[0])
                ? "not-applied"
                : IsManagedVm(matching[0])
                    ? "target-recreated-or-identity-changed"
                    : "target-name-collision-unmanaged";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider readback still contains the delete target or an ambiguous name collision; absence was not proven.",
                "vm.delete"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileCheckpointCreateJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
        var checkpointName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            string.IsNullOrWhiteSpace(checkpointName) ||
            !TryReadCapturedCheckpointCreateBaseline(metadata, vmName, checkpointName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable checkpoint.create baseline was not captured or is not structurally valid.",
                    "checkpoint.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke(
            "checkpoint.list",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
            readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider checkpoint.list readback failed with {providerCode}; no mutation was attempted.",
                    "checkpoint.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateCheckpointList(readback.Data.Value)
            .Where(checkpoint =>
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "name"), checkpointName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matching.Length == 1)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "checkpoint.create",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_before"] = new SortedDictionary<string, object?>
                    {
                        ["state"] = "absent",
                        ["name"] = checkpointName,
                        ["vm_name"] = vmName
                    },
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["state"] = "present",
                        ["name"] = checkpointName,
                        ["vm_name"] = vmName
                    },
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

        var classification = matching.Length == 0
            ? "not-applied"
            : "ambiguous-duplicate-checkpoint-names";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider checkpoint.list readback did not prove exactly one checkpoint with the captured absent pre-state.",
                "checkpoint.create"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileCheckpointRestoreJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
        var checkpointName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            string.IsNullOrWhiteSpace(checkpointName) ||
            !TryReadCapturedCheckpointRestoreBaseline(metadata, vmName, checkpointName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable checkpoint.restore baseline was not captured or is not structurally valid.",
                    "checkpoint.restore"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke(
            "checkpoint.list",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
            readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider checkpoint.list readback failed with {providerCode}; no mutation was attempted.",
                    "checkpoint.restore"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = MatchingCheckpoints(readback.Data.Value, vmName, checkpointName);
        var currentTrue = CurrentTrueCheckpoints(readback.Data.Value);
        if (matching.Length == 1 && currentTrue.Length == 1 && ReadIsCurrent(matching[0]) == true)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "checkpoint.restore",
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

        var classification = matching.Length == 0
            ? "not-applied"
            : matching.Length > 1
                ? "ambiguous-duplicate-checkpoint-names"
                : ReadIsCurrent(matching[0]) == false
                    ? "not-applied"
                    : "current-unavailable";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider checkpoint.list readback did not prove the requested checkpoint is uniquely current.",
                "checkpoint.restore"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
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

    private JsonElement CaptureVmRenameBaseline(
        string oldName,
        string newName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRenameReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), oldName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRenameReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
                });
            }

            var before = matches[0].Clone();
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRenameReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmRenameFingerprint(before),
                ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRenameReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
            });
        }
    }

    private JsonElement CaptureVmDeleteBaseline(
        string vmName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            var before = matches[0].Clone();
            if (!IsManagedVm(before))
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = "PCV_VM_NOT_MANAGED_BY_PURECVISOR",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(DesktopNodeApiJsonReader.ReadString(before, "id")))
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = "PCV_VM_IDENTITY_UNAVAILABLE",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmDeleteReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmDeleteFingerprint(before),
                ["expected_after"] = new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "absent"
                }
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmDeleteReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "absent"
                }
            });
        }
    }

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

    private static JsonElement BuildVmDeleteFingerprint(JsonElement vm)
    {
        return BuildVmRenameFingerprint(vm);
    }

    private static bool IsManagedVm(JsonElement vm)
    {
        var marker = DesktopNodeApiJsonReader.ReadElement(vm, "managed_by_purecvisor");
        return marker is not null && marker.Value.ValueKind == JsonValueKind.True;
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

    private sealed record VmRenameBaseline(
        string Schema,
        JsonElement Before,
        JsonElement BeforeFingerprint);

    private sealed record VmDeleteBaseline(
        string Schema,
        JsonElement Before,
        JsonElement BeforeFingerprint);

    private sealed record VmCheckpointCreateBaseline(
        string Schema,
        JsonElement? Before);

    private sealed record VmCheckpointRestoreBaseline(
        string Schema,
        JsonElement Before,
        JsonElement ExpectedAfter);
}

using System.Text.Json.Serialization;

namespace DesktopNode.Contracts;

public static class HyperVQosMutationContract
{
    public const string PreviewContract = "hyperv-qos-mutation-preview.v1";
    public const string ApplyEvidenceContract = "hyperv-qos-mutation-apply-evidence.v1";

    public static HyperVQosMutationPreview CreateStoragePreview(
        string vmName,
        string disk,
        int maximumIops,
        int? minimumIops,
        string requestId,
        string actor)
    {
        return new HyperVQosMutationPreview(
            Contract: PreviewContract,
            Mode: "dry-run",
            Provider: "hyperv",
            RequestId: requestId,
            Actor: actor,
            Vm: new HyperVQosMutationVm(vmName),
            Storage: new HyperVQosStoragePreview(
                TargetDisk: disk,
                CurrentPolicy: "readback-required",
                ProposedPolicy: new HyperVQosStoragePolicy(minimumIops, maximumIops),
                Supported: true,
                UnsupportedReason: null),
            Network: null,
            Validation: HyperVQosMutationValidation.DryRun(),
            RollbackPlan: new HyperVQosRollbackPlan(true, "vm.qos.storage.rollback"),
            ReadbackRoutes: ["GET /api/v1/vms/{vm}/blkio"]);
    }

    public static HyperVQosMutationPreview CreateNetworkPreview(
        string vmName,
        string adapter,
        int maximumKbps,
        int? minimumKbps,
        string requestId,
        string actor)
    {
        return new HyperVQosMutationPreview(
            Contract: PreviewContract,
            Mode: "dry-run",
            Provider: "hyperv",
            RequestId: requestId,
            Actor: actor,
            Vm: new HyperVQosMutationVm(vmName),
            Storage: null,
            Network: new HyperVQosNetworkPreview(
                Adapter: adapter,
                CurrentPolicy: "readback-required",
                ProposedPolicy: new HyperVQosNetworkPolicy(minimumKbps, maximumKbps),
                Supported: true,
                UnsupportedReason: null),
            Validation: HyperVQosMutationValidation.DryRun(),
            RollbackPlan: new HyperVQosRollbackPlan(true, "vm.qos.network.rollback"),
            ReadbackRoutes: ["GET /api/v1/vms/{vm}/bandwidth"]);
    }

    public static HyperVQosApplyEvidence CreateApplyEvidence(
        string operation,
        string vmName,
        string target,
        string previousPolicy,
        string appliedPolicy,
        string rollbackOperation,
        string requestId)
    {
        return new HyperVQosApplyEvidence(
            Contract: ApplyEvidenceContract,
            Operation: operation,
            Vm: new HyperVQosMutationVm(vmName),
            Target: target,
            PreviousPolicy: previousPolicy,
            AppliedPolicy: appliedPolicy,
            RollbackPlan: new HyperVQosRollbackPlan(true, rollbackOperation),
            Audit: new HyperVQosMutationAudit(
                Actor: "local-api-operator",
                RequestId: requestId,
                ArgsRedacted: true));
    }
}

public sealed record HyperVQosMutationPreview(
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("actor")] string Actor,
    [property: JsonPropertyName("vm")] HyperVQosMutationVm Vm,
    [property: JsonPropertyName("storage")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    HyperVQosStoragePreview? Storage,
    [property: JsonPropertyName("network")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    HyperVQosNetworkPreview? Network,
    [property: JsonPropertyName("validation")] HyperVQosMutationValidation Validation,
    [property: JsonPropertyName("rollback_plan")] HyperVQosRollbackPlan RollbackPlan,
    [property: JsonPropertyName("readback_routes")] IReadOnlyList<string> ReadbackRoutes);

public sealed record HyperVQosMutationVm(
    [property: JsonPropertyName("name")] string Name);

public sealed record HyperVQosStoragePreview(
    [property: JsonPropertyName("target_disk")] string TargetDisk,
    [property: JsonPropertyName("current_policy")] string CurrentPolicy,
    [property: JsonPropertyName("proposed_policy")] HyperVQosStoragePolicy ProposedPolicy,
    [property: JsonPropertyName("supported")] bool Supported,
    [property: JsonPropertyName("unsupported_reason")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? UnsupportedReason);

public sealed record HyperVQosStoragePolicy(
    [property: JsonPropertyName("minimum_iops")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? MinimumIops,
    [property: JsonPropertyName("maximum_iops")] int MaximumIops);

public sealed record HyperVQosNetworkPreview(
    [property: JsonPropertyName("adapter")] string Adapter,
    [property: JsonPropertyName("current_policy")] string CurrentPolicy,
    [property: JsonPropertyName("proposed_policy")] HyperVQosNetworkPolicy ProposedPolicy,
    [property: JsonPropertyName("supported")] bool Supported,
    [property: JsonPropertyName("unsupported_reason")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? UnsupportedReason);

public sealed record HyperVQosNetworkPolicy(
    [property: JsonPropertyName("minimum_kbps")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? MinimumKbps,
    [property: JsonPropertyName("maximum_kbps")] int MaximumKbps);

public sealed record HyperVQosMutationValidation(
    [property: JsonPropertyName("requires_admin")] bool RequiresAdmin,
    [property: JsonPropertyName("live_vm_allowed")] bool LiveVmAllowed,
    [property: JsonPropertyName("restart_required")] bool RestartRequired,
    [property: JsonPropertyName("host_mutation_performed")] bool HostMutationPerformed)
{
    public static HyperVQosMutationValidation DryRun()
    {
        return new HyperVQosMutationValidation(
            RequiresAdmin: true,
            LiveVmAllowed: true,
            RestartRequired: false,
            HostMutationPerformed: false);
    }
}

public sealed record HyperVQosRollbackPlan(
    [property: JsonPropertyName("previous_policy_captured")] bool PreviousPolicyCaptured,
    [property: JsonPropertyName("rollback_operation")] string RollbackOperation);

public sealed record HyperVQosApplyEvidence(
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("vm")] HyperVQosMutationVm Vm,
    [property: JsonPropertyName("target")] string Target,
    [property: JsonPropertyName("previous_policy")] string PreviousPolicy,
    [property: JsonPropertyName("applied_policy")] string AppliedPolicy,
    [property: JsonPropertyName("rollback_plan")] HyperVQosRollbackPlan RollbackPlan,
    [property: JsonPropertyName("audit")] HyperVQosMutationAudit Audit);

public sealed record HyperVQosMutationAudit(
    [property: JsonPropertyName("actor")] string Actor,
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("args_redacted")] bool ArgsRedacted);

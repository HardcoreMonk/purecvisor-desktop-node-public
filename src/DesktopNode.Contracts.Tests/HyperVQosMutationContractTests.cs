using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class HyperVQosMutationContractTests
{
    [Fact]
    public void HyperVQosMutationPreviewSerializesDryRunRollbackAndReadbackContract()
    {
        var preview = HyperVQosMutationContract.CreateStoragePreview(
            vmName: "lab vm",
            disk: "disk0",
            maximumIops: 1200,
            minimumIops: 100,
            requestId: "req-test",
            actor: "local-api-operator");

        var json = JsonSerializer.Serialize(preview, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("hyperv-qos-mutation-preview.v1", root.GetProperty("contract").GetString());
        Assert.Equal("dry-run", root.GetProperty("mode").GetString());
        Assert.Equal("hyperv", root.GetProperty("provider").GetString());
        Assert.Equal("req-test", root.GetProperty("request_id").GetString());
        Assert.Equal("local-api-operator", root.GetProperty("actor").GetString());
        Assert.Equal("lab vm", root.GetProperty("vm").GetProperty("name").GetString());
        Assert.Equal("disk0", root.GetProperty("storage").GetProperty("target_disk").GetString());
        Assert.Equal(1200, root.GetProperty("storage").GetProperty("proposed_policy").GetProperty("maximum_iops").GetInt32());
        Assert.Equal(100, root.GetProperty("storage").GetProperty("proposed_policy").GetProperty("minimum_iops").GetInt32());
        Assert.True(root.GetProperty("validation").GetProperty("requires_admin").GetBoolean());
        Assert.False(root.GetProperty("validation").GetProperty("host_mutation_performed").GetBoolean());
        Assert.Equal("vm.qos.storage.rollback", root.GetProperty("rollback_plan").GetProperty("rollback_operation").GetString());
        Assert.Contains(
            root.GetProperty("readback_routes").EnumerateArray().Select(item => item.GetString()),
            route => route == "GET /api/v1/vms/{vm}/blkio");
    }

    [Fact]
    public void HyperVQosMutationApplyEvidenceRedactsArgumentsAndCapturesRollbackDescriptor()
    {
        var evidence = HyperVQosMutationContract.CreateApplyEvidence(
            operation: "vm.qos.network.set",
            vmName: "lab vm",
            target: "eth0",
            previousPolicy: "unset",
            appliedPolicy: "maximum_kbps=2048",
            rollbackOperation: "vm.qos.network.rollback",
            requestId: "req-apply");

        var json = JsonSerializer.Serialize(evidence, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("hyperv-qos-mutation-apply-evidence.v1", root.GetProperty("contract").GetString());
        Assert.Equal("vm.qos.network.set", root.GetProperty("operation").GetString());
        Assert.Equal("lab vm", root.GetProperty("vm").GetProperty("name").GetString());
        Assert.Equal("eth0", root.GetProperty("target").GetString());
        Assert.Equal("unset", root.GetProperty("previous_policy").GetString());
        Assert.Equal("maximum_kbps=2048", root.GetProperty("applied_policy").GetString());
        Assert.Equal("vm.qos.network.rollback", root.GetProperty("rollback_plan").GetProperty("rollback_operation").GetString());
        Assert.Equal("req-apply", root.GetProperty("audit").GetProperty("request_id").GetString());
        Assert.True(root.GetProperty("audit").GetProperty("args_redacted").GetBoolean());
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
    }
}

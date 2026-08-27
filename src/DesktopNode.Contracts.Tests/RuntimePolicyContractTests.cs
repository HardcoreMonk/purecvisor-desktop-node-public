using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class RuntimePolicyContractTests
{
    [Fact]
    public void RuntimePolicySerializesPhase24JobRuntimeContract()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var jobRuntime = root.GetProperty("data").GetProperty("job_runtime");

        Assert.Equal("runtime.policy", root.GetProperty("operation").GetString());
        Assert.Equal(1, jobRuntime.GetProperty("contract_version").GetInt32());
        Assert.Equal("local-api", jobRuntime.GetProperty("owner").GetString());
        Assert.Equal("json-file-snapshot", jobRuntime.GetProperty("state_store").GetProperty("persistence").GetString());
        Assert.Equal("blocked-diagnostics-no-mutation", jobRuntime.GetProperty("state_store").GetProperty("unsupported_future_version").GetString());
        Assert.Equal("dotnet-native-read-vm-qos-preview-guestservice-guest-execution-create-lifecycle-media-resource-delete-checkpoint-mutation", jobRuntime.GetProperty("dispatch").GetProperty("helper_boundary").GetString());
        Assert.False(jobRuntime.GetProperty("control").GetProperty("cancel").GetProperty("queued_only").GetBoolean());
        Assert.True(jobRuntime.GetProperty("control").GetProperty("cancel").GetProperty("running_interrupt").GetBoolean());
        Assert.Equal(
            RuntimePolicyContract.RunningInterruptOperations,
            jobRuntime
                .GetProperty("control")
                .GetProperty("cancel")
                .GetProperty("running_interrupt_operations")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());
        Assert.True(jobRuntime.GetProperty("control").GetProperty("retry").GetProperty("failed_error_retryable_only").GetBoolean());
        Assert.Equal("native-read-routes-vm-qos-preview-guestservice-guest-execution-create-lifecycle-media-resource-delete-checkpoint-and-qos-mutation", jobRuntime.GetProperty("host_mutation").GetString());
        Assert.Equal("dotnet", jobRuntime.GetProperty("orchestration").GetProperty("primary").GetString());
        Assert.Equal("dotnet-native-adapter-contract-tests-admin-smoke", jobRuntime.GetProperty("orchestration").GetProperty("contract").GetString());
        Assert.Equal("single_bearer_token", root.GetProperty("data").GetProperty("auth").GetProperty("mode").GetString());
    }

    [Theory]
    [InlineData("vm.guest.exec", true)]
    [InlineData("vm.guest.channel.verify", true)]
    [InlineData("vm.guest.channel.ensure", false)]
    [InlineData("vm.start", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RuntimePolicyUsesOneRunningInterruptOperationContract(string? operation, bool expected)
    {
        Assert.Equal(expected, RuntimePolicyContract.IsRunningInterruptOperation(operation));

        var cancel = RuntimePolicyContract.CreateDefault().Data.JobRuntime.Control.Cancel;
        Assert.Equal(RuntimePolicyContract.RunningInterruptOperations, cancel.RunningInterruptOperations);
        Assert.Equal(cancel.RunningInterruptOperations.Count > 0, cancel.RunningInterrupt);
    }

    [Fact]
    public void RuntimePolicyDeclaresNativeReadRouteStart()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var nativeCore = document.RootElement
            .GetProperty("data")
            .GetProperty("job_runtime")
            .GetProperty("native_core");

        Assert.Equal("read-route-vm-qos-preview-guestservice-guest-execution-resource-checkpoint-and-qos-mutation-started", nativeCore.GetProperty("status").GetString());
        Assert.Equal("host.status,network.inventory,vm.list,vm.memory-stats,vm.cpu-stats,vm.blkio-get,vm.bandwidth,vm.qos.storage.preview,vm.qos.network.preview,vm.guest-agent-status,vm.guest-ping,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.pause,vm.resume,vm.save,vm.resume-saved,vm.rename,vm.manage,vm.clone.preview,vm.clone,vm.eject,vm.attach,vm.limit,vm.qos.storage.set,vm.qos.network.set,vm.guest.exec,vm.guest.channel.verify,vm.guest.channel.ensure,vm.set-memory,vm.set-vcpu,vm.disk-resize,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete", nativeCore.GetProperty("reason").GetString());
    }

    [Fact]
    public void RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var dispatch = document.RootElement
            .GetProperty("data")
            .GetProperty("job_runtime")
            .GetProperty("dispatch");

        Assert.Equal(
            new[] { "host.status", "network.inventory", "vm.list", "vm.memory-stats", "vm.cpu-stats", "vm.blkio-get", "vm.bandwidth", "vm.qos.storage.preview", "vm.qos.network.preview", "vm.guest-agent-status", "vm.guest-ping", "checkpoint.list" },
            dispatch.GetProperty("native_probe_operations").EnumerateArray().Select(item => item.GetString()).ToArray());
        Assert.Equal(
            new[] { "vm.create", "vm.start", "vm.shutdown", "vm.poweroff", "vm.restart", "vm.pause", "vm.resume", "vm.save", "vm.resume-saved", "vm.rename", "vm.manage", "vm.clone.preview", "vm.clone", "vm.eject", "vm.attach", "vm.limit", "vm.qos.storage.set", "vm.qos.network.set", "vm.guest.exec", "vm.guest.channel.verify", "vm.guest.channel.ensure", "vm.set-memory", "vm.set-vcpu", "vm.disk-resize", "vm.delete", "checkpoint.create", "checkpoint.restore", "checkpoint.delete" },
            dispatch.GetProperty("native_mutation_operations").EnumerateArray().Select(item => item.GetString()).ToArray());
        Assert.Equal("native-vm-qos-guest-execution-and-mutation-create-lifecycle-media-resource-delete-checkpoint-mutation", dispatch.GetProperty("mutation_dispatch").GetString());
    }

    [Fact]
    public void RuntimePolicyDeclaresDotNetAsDefaultServiceHost()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var managedCore = document.RootElement
            .GetProperty("data")
            .GetProperty("job_runtime")
            .GetProperty("managed_core");

        Assert.Equal("dotnet", managedCore.GetProperty("candidate").GetString());
        Assert.Equal("service-host-default", managedCore.GetProperty("status").GetString());
        Assert.Equal("dotnet-windows-service-host", managedCore.GetProperty("host_replacement").GetString());
    }

    [Fact]
    public void RuntimePolicyCanReportTokenStorageAndExposure()
    {
        var policy = RuntimePolicyContract.CreateDefault(
            tokenStorage: "dpapi-local-machine",
            currentExposure: "lan");

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal("dpapi-local-machine", data.GetProperty("auth").GetProperty("token_storage").GetString());
        Assert.Equal("lan", data.GetProperty("network").GetProperty("current_exposure").GetString());
        Assert.Equal("unauthenticated-static-only", data.GetProperty("network").GetProperty("static_asset_auth").GetProperty("loopback").GetString());
    }

    [Fact]
    public void RuntimePolicyDeclaresRuntimeCoreApiAuthJobAndDiagnosticsContract()
    {
        var policy = RuntimePolicyContract.CreateDefault(
            tokenStorage: "credential-manager",
            authPolicy: new RuntimePolicyAuthPolicy(
                Mode: "account_session_jwt",
                MultiUser: true,
                Rbac: true,
                TokenStorage: "credential-manager",
                Roles: ["admin", "operator", "viewer"],
                GrantTypes: ["password", "refresh_token"],
                SessionStorage: "jwt-refresh-session-store",
                AccessTokenTtlSeconds: 900,
                RefreshTokenTtlSeconds: 604800));

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var runtimeCore = document.RootElement.GetProperty("data").GetProperty("runtime_core");

        Assert.Equal(1, runtimeCore.GetProperty("contract_version").GetInt32());
        Assert.Equal("DesktopNode.Api", runtimeCore.GetProperty("owner").GetString());

        var apiRoutes = runtimeCore.GetProperty("api_routes");
        Assert.Contains("/api/v1/auth/session", apiRoutes.GetProperty("auth_session").EnumerateArray().Select(item => item.GetString()));
        Assert.Contains("/api/v1/jobs/{jobId}/retry", apiRoutes.GetProperty("job_runtime").EnumerateArray().Select(item => item.GetString()));
        Assert.Contains("/api/v1/diagnostics/bundles/{bundleId}/download", apiRoutes.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString()));

        var authSession = runtimeCore.GetProperty("auth_session");
        Assert.Equal("DesktopNode.Api.AccountAuth", authSession.GetProperty("owner").GetString());
        Assert.Equal("account_session_jwt", authSession.GetProperty("mode").GetString());
        Assert.Equal("credential-manager", authSession.GetProperty("token_storage").GetString());
        Assert.Equal("jwt-refresh-session-store", authSession.GetProperty("session_storage").GetString());

        var jobRuntime = runtimeCore.GetProperty("job_runtime");
        Assert.Equal("DesktopNode.Runtime", jobRuntime.GetProperty("owner").GetString());
        Assert.Equal("json-file-snapshot", jobRuntime.GetProperty("state_store").GetString());
        Assert.Equal("native-adapter-queued-mutation-boundary", jobRuntime.GetProperty("mutation_boundary").GetString());

        var diagnostics = runtimeCore.GetProperty("diagnostics");
        Assert.Equal("DesktopNode.Api.Diagnostics", diagnostics.GetProperty("owner").GetString());
        Assert.Equal("diagnostic-bundle-redaction-required", diagnostics.GetProperty("redaction").GetString());
        Assert.Equal("configured-retention-with-pagination", diagnostics.GetProperty("retention").GetString());
    }

    [Fact]
    public void RuntimePolicyDeclaresGuestExecutionProviderBoundary()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var guestExecution = document.RootElement.GetProperty("data").GetProperty("guest_execution");

        Assert.True(guestExecution.GetProperty("enabled").GetBoolean());
        Assert.True(guestExecution.GetProperty("preview_enabled").GetBoolean());
        Assert.True(guestExecution.GetProperty("execute_enabled").GetBoolean());
        Assert.True(guestExecution.GetProperty("channel_preview_enabled").GetBoolean());
        Assert.True(guestExecution.GetProperty("channel_verify_enabled").GetBoolean());
        Assert.True(guestExecution.GetProperty("channel_repair_enabled").GetBoolean());
        Assert.Equal("provider-verify-repair-queued-execution-enabled", guestExecution.GetProperty("status").GetString());
        Assert.Equal("docs/adr/0009-guest-execution-security-boundary.md", guestExecution.GetProperty("adr").GetString());
        Assert.Equal("credential-ref-only", guestExecution.GetProperty("credential_policy").GetString());
        Assert.Equal("guest-execution-audit-v1", guestExecution.GetProperty("audit_schema").GetString());
        Assert.Equal("guest-execution-redaction-v1", guestExecution.GetProperty("redaction_policy").GetString());
        Assert.Equal(60, guestExecution.GetProperty("timeout").GetProperty("default_seconds").GetInt32());
        Assert.Equal(600, guestExecution.GetProperty("timeout").GetProperty("max_seconds").GetInt32());
        Assert.Equal("queued-and-running-guest-execution-cancel-with-provider-token-interrupt", guestExecution.GetProperty("timeout").GetProperty("cancel").GetString());
        Assert.Equal(
            new[] { "operate", "guest.exec", "guest.channel.configure", "job.cancel" },
            guestExecution.GetProperty("required_capabilities").EnumerateArray().Select(item => item.GetString()).ToArray());
        Assert.Contains(
            "PCV_GUEST_EXEC_DISABLED",
            guestExecution.GetProperty("problem_codes").EnumerateArray().Select(item => item.GetString()));
        Assert.Contains(
            "PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED",
            guestExecution.GetProperty("problem_codes").EnumerateArray().Select(item => item.GetString()));
        Assert.Contains(
            "POST /api/v1/vms/{vmId}/guest/exec",
            guestExecution.GetProperty("routes").EnumerateArray().Select(item => item.GetString()));
    }
}

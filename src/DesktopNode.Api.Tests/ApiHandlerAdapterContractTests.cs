using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiHandlerAdapterContractTests
{
    [Fact]
    public void DefaultContractMatchesFeatureSurfaceLedger()
    {
        using var document = LoadRepoJson("config/desktop-node-feature-surface-ledger.json");
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-feature-surface-ledger-v1", root.GetProperty("contract").GetString());
        Assert.Equal(
            ["api", "cli", "web"],
            root.GetProperty("target_surfaces").EnumerateArray().Select(item => item.GetString()!).ToArray());

        var featureIds = new HashSet<string>(StringComparer.Ordinal);
        var ledgerRoutes = new Dictionary<
            (string Method, string RouteTemplate),
            (string FeatureId, string OperationId, string? RequiredPermission)>();

        foreach (var feature in root.GetProperty("features").EnumerateArray())
        {
            var featureId = feature.GetProperty("feature_id").GetString()!;
            Assert.Matches("^pcv\\.[a-z0-9._-]+$", featureId);
            Assert.True(featureIds.Add(featureId), $"Duplicate Feature ID: {featureId}");
            Assert.False(string.IsNullOrWhiteSpace(feature.GetProperty("title").GetString()));

            var featureRoutes = feature.GetProperty("routes").EnumerateArray().ToArray();
            Assert.NotEmpty(featureRoutes);
            foreach (var route in featureRoutes)
            {
                var method = route.GetProperty("method").GetString()!;
                var routeTemplate = route.GetProperty("route_template").GetString()!;
                var operationId = route.GetProperty("operation_id").GetString()!;
                var permissionElement = route.GetProperty("required_permission");
                var requiredPermission = permissionElement.ValueKind == JsonValueKind.Null
                    ? null
                    : permissionElement.GetString();
                var presentSurfaces = route.GetProperty("present_surfaces")
                    .EnumerateArray()
                    .Select(item => item.GetString()!)
                    .ToHashSet(StringComparer.Ordinal);
                var exclusions = route.GetProperty("excluded_surfaces").EnumerateArray().ToArray();

                Assert.Matches("^[a-z0-9.-]+$", operationId);
                Assert.Contains("api", presentSurfaces);
                foreach (var optionalSurface in new[] { "cli", "web" })
                {
                    var matchingExclusions = exclusions
                        .Where(item => item.GetProperty("surface").GetString() == optionalSurface)
                        .ToArray();
                    Assert.True(
                        presentSurfaces.Contains(optionalSurface) ^ matchingExclusions.Length == 1,
                        $"Surface decision must be exactly one of present/excluded: {method} {routeTemplate} {optionalSurface}");
                    if (matchingExclusions.Length == 1)
                    {
                        Assert.False(string.IsNullOrWhiteSpace(matchingExclusions[0].GetProperty("reason").GetString()));
                    }
                }

                Assert.True(
                    ledgerRoutes.TryAdd(
                        (method, routeTemplate),
                        (featureId, operationId, requiredPermission)),
                    $"Duplicate route ownership: {method} {routeTemplate}");
            }
        }

        var contract = ApiHandlerAdapterContract.CreateDefault();
        Assert.Equal(27, featureIds.Count);
        Assert.Equal(60, ledgerRoutes.Count);
        Assert.Equal(60, contract.Routes.Count);
        foreach (var route in contract.Routes)
        {
            Assert.Matches("^pcv\\.[a-z0-9._-]+$", route.FeatureId);
            Assert.Matches("^[a-z0-9.-]+$", route.OperationId);
            Assert.True(
                ledgerRoutes.TryGetValue((route.Method, route.RouteTemplate), out var ledgerRoute),
                $"Missing surface-ledger route: {route.Method} {route.RouteTemplate}");
            Assert.Equal(route.FeatureId, ledgerRoute.FeatureId);
            Assert.Equal(route.OperationId, ledgerRoute.OperationId);
            Assert.Equal(route.RequiredPermission, ledgerRoute.RequiredPermission);
        }
    }

    [Fact]
    public void EvidenceCandidateFeaturesAreKnownSurfaceFeatures()
    {
        using var surfaceDocument = LoadRepoJson("config/desktop-node-feature-surface-ledger.json");
        using var evidenceDocument = LoadRepoJson("config/desktop-node-feature-evidence-ledger.json");
        var surfaceFeatureIds = surfaceDocument.RootElement.GetProperty("features")
            .EnumerateArray()
            .Select(item => item.GetProperty("feature_id").GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        var evidenceFeatureIds = evidenceDocument.RootElement.GetProperty("features")
            .EnumerateArray()
            .Select(item => item.GetProperty("feature_id").GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(4, evidenceFeatureIds.Count);
        Assert.Contains("pcv.vm.saved-lifecycle", evidenceFeatureIds);
        Assert.True(evidenceFeatureIds.IsSubsetOf(surfaceFeatureIds));
    }

    [Fact]
    public void DefaultContractMapsPhase25RouteCandidates()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));

        Assert.Equal(60, contract.Routes.Count);
        Assert.Equal(60, routes.Count);

        AssertRoute(routes[("GET", "/api/v1/runtime/policy")], "GET", "RuntimePolicy", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/host/status")], "GET", "HostStatus", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms")], "GET", "ListVms", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/jobs")], "GET", "ListJobs", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/jobs/{jobId}")], "GET", "GetJob", MutationStance.ReadOnly);
        AssertRoute(routes[("POST", "/api/v1/jobs/{jobId}/cancel")], "POST", "CancelJob", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/jobs/{jobId}/retry")], "POST", "RetryJob", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/jobs/{jobId}/reconcile")], "POST", "ReconcileJob", MutationStance.ProductOperation);
        AssertRoute(routes[("GET", "/api/v1/ops/summary")], "GET", "OpsSummary", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/diagnostics/bundles")], "GET", "ListDiagnosticBundles", MutationStance.ReadOnly);
        AssertRoute(routes[("POST", "/api/v1/diagnostics/bundles")], "POST", "CreateDiagnosticBundle", MutationStance.ProductOperation);
        AssertRoute(routes[("GET", "/api/v1/diagnostics/bundles/{bundleId}/download")], "GET", "DownloadDiagnosticBundle", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/network/inventory")], "GET", "NetworkInventory", MutationStance.ReadOnly);
        AssertRoute(routes[("POST", "/api/v1/auth/login")], "POST", "LoginAccount", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/auth/loopback-session")], "POST", "CreateLoopbackSession", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/auth/refresh")], "POST", "RefreshAccount", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/auth/logout")], "POST", "LogoutAccount", MutationStance.ProductOperation);
        AssertRoute(routes[("GET", "/api/v1/auth/session")], "GET", "GetAccountSession", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/auth/rbac")], "GET", "GetAccountRbac", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/console/capabilities")], "GET", "GetConsoleCapabilities", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}")], "GET", "GetVm", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/console")], "GET", "GetVmConsoleSession", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/memory-stats")], "GET", "GetVmMemoryStats", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/cpu-stats")], "GET", "GetVmCpuStats", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/blkio")], "GET", "GetVmBlockIoPolicy", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/bandwidth")], "GET", "GetVmBandwidthPolicy", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/guest-agent/status")], "GET", "GetVmGuestAgentStatus", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/guest-agent/ping")], "GET", "PingVmGuestAgent", MutationStance.ReadOnly);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/guest/exec/preview")], "POST", "PreviewVmGuestExec", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/guest/exec")], "POST", "QueueVmGuestExec", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/guest/channel/preview")], "POST", "PreviewVmGuestChannel", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/guest/channel/verify")], "POST", "QueueVerifyVmGuestChannel", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/guest/channel")], "POST", "QueueEnsureVmGuestChannel", MutationStance.QueuedMutation);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/delete-status")], "GET", "GetVmDeleteStatus", MutationStance.ReadOnly);
        AssertRoute(routes[("GET", "/api/v1/vms/{vmId}/checkpoints")], "GET", "ListVmCheckpoints", MutationStance.ReadOnly);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/qos/storage/preview")], "POST", "PreviewVmStorageQos", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/qos/network/preview")], "POST", "PreviewVmNetworkQos", MutationStance.ProductOperation);
        AssertRoute(routes[("POST", "/api/v1/vms")], "POST", "QueueCreateVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/checkpoints")], "POST", "QueueCreateVmCheckpoint", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore")], "POST", "QueueRestoreVmCheckpoint", MutationStance.QueuedMutation);
        AssertRoute(routes[("DELETE", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}")], "DELETE", "QueueDeleteVmCheckpoint", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/start")], "POST", "QueueStartVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/shutdown")], "POST", "QueueShutdownVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/poweroff")], "POST", "QueuePowerOffVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/restart")], "POST", "QueueRestartVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/pause")], "POST", "QueuePauseVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/resume")], "POST", "QueueResumeVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/save")], "POST", "QueueSaveVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/resume-saved")], "POST", "QueueResumeSavedVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/rename")], "POST", "QueueRenameVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/manage")], "POST", "QueueManageVm", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/eject")], "POST", "QueueEjectVmMedia", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/attach")], "POST", "QueueAttachVmMedia", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/limit")], "POST", "QueueSetVmLimit", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/qos/storage")], "POST", "QueueSetVmStorageQos", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/qos/network")], "POST", "QueueSetVmNetworkQos", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/set-memory")], "POST", "QueueSetVmMemory", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/set-vcpu")], "POST", "QueueSetVmVcpu", MutationStance.QueuedMutation);
        AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/disk-resize")], "POST", "QueueResizeVmDisk", MutationStance.QueuedMutation);
        AssertRoute(routes[("DELETE", "/api/v1/vms/{vmId}")], "DELETE", "QueueDeleteVm", MutationStance.QueuedMutation);
        Assert.DoesNotContain(contract.Routes, route => route.RouteTemplate == "/api/v1/vms/{vmId}/lifecycle/{action}");
        Assert.DoesNotContain(contract.Routes, route => route.RouteTemplate.Contains("/evidence", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DefaultContractPinsCompleteRoutePermissionAndMutationSnapshot()
    {
        var routes = ApiHandlerAdapterContract.CreateDefault().Routes;
        var canonicalProjection = routes
            .OrderBy(route => route.Method, StringComparer.Ordinal)
            .ThenBy(route => route.RouteTemplate, StringComparer.Ordinal)
            .Select(route => string.Join(
                '|',
                route.Method,
                route.RouteTemplate,
                route.RouteFamily,
                route.RequiredPermission ?? string.Empty,
                route.MutationStance,
                route.AuthPolicy,
                route.DefaultOwner,
                route.OperationName));
        var snapshot = string.Join("\n", canonicalProjection);
        var digest = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(snapshot)))
            .ToLowerInvariant();

        Assert.Equal("dbb52dd93a265632ebcd302c3d2618012fe10af7721b0a44d9489585554cea85", digest);
        Assert.Equal(22, routes.Count(route => route.MutationStance == MutationStance.ReadOnly));
        Assert.Equal(12, routes.Count(route => route.MutationStance == MutationStance.ProductOperation));
        Assert.Equal(26, routes.Count(route => route.MutationStance == MutationStance.QueuedMutation));
        Assert.Equal(13, routes.Select(route => route.RouteFamily).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void DefaultContractSeparatesReadOnlyAndQueuedMutationRoutes()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();

        Assert.All(
            contract.Routes.Where(route => route.MutationStance == MutationStance.ReadOnly),
            route => Assert.StartsWith("Read", route.AuthPolicy, StringComparison.Ordinal));

        var queuedMutations = contract.Routes.Where(route => route.MutationStance == MutationStance.QueuedMutation).ToArray();
        Assert.All(
            queuedMutations,
            route =>
            {
                Assert.Equal("TokenRequired", route.AuthPolicy);
                Assert.StartsWith("Queue", route.OperationName, StringComparison.Ordinal);
            });
        Assert.All(
            queuedMutations,
            route => Assert.Equal("dotnet-native-adapter", route.DefaultOwner));

        var productOperations = contract.Routes.Where(route => route.MutationStance == MutationStance.ProductOperation).ToArray();
        Assert.All(
            productOperations,
            route =>
            {
                if (route.RouteTemplate.StartsWith("/api/v1/auth/", StringComparison.Ordinal))
                {
                    Assert.Equal("NoBearerTokenRequired", route.AuthPolicy);
                }
                else
                {
                    Assert.Equal("TokenRequired", route.AuthPolicy);
                }

                if (route.RouteFamily is "hyperv-vm-qos")
                {
                    Assert.Equal("dotnet-native-adapter", route.DefaultOwner);
                }
                else
                {
                    Assert.Equal("dotnet-runtime", route.DefaultOwner);
                }
            });

        var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));
        Assert.Equal("NoBearerTokenRequired", routes[("POST", "/api/v1/auth/loopback-session")].AuthPolicy);
        Assert.Null(routes[("POST", "/api/v1/auth/loopback-session")].RequiredPermission);
        Assert.Equal("auth", routes[("POST", "/api/v1/auth/loopback-session")].RouteFamily);
    }

    [Fact]
    public void DefaultContractKeepsDotNetProductOwners()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));

        Assert.Equal("dotnet-runtime", contract.DefaultOwner);
        Assert.Equal("product-default-dotnet-runtime", contract.AdapterStance);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/runtime/policy")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/jobs")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/ops/summary")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/diagnostics/bundles")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/diagnostics/bundles")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/diagnostics/bundles/{bundleId}/download")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/auth/login")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/auth/refresh")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/auth/logout")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/auth/session")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/auth/rbac")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/console/capabilities")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/vms/{vmId}/console")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("GET", "/api/v1/vms/{vmId}/delete-status")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/vms/{vmId}/guest/exec/preview")].DefaultOwner);
        Assert.Equal(
            "dotnet-runtime",
            routes[("POST", "/api/v1/vms/{vmId}/guest/channel/preview")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/guest/exec")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/guest/channel/verify")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/guest/channel")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/network/inventory")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/host/status")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/vms")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/vms/{vmId}")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/vms/{vmId}/checkpoints")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/vms/{vmId}/guest-agent/status")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("GET", "/api/v1/vms/{vmId}/blkio")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/checkpoints")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("DELETE", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/start")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/shutdown")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/poweroff")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("POST", "/api/v1/vms/{vmId}/restart")].DefaultOwner);
        Assert.Equal(
            "dotnet-native-adapter",
            routes[("DELETE", "/api/v1/vms/{vmId}")].DefaultOwner);
        Assert.All(
            contract.Routes.Where(route => route.MutationStance == MutationStance.QueuedMutation),
            route => Assert.Equal("dotnet-native-adapter", route.DefaultOwner));

        var serialized = JsonSerializer.Serialize(contract);
        Assert.DoesNotContain("powershell", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("helper", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DefaultContractDoesNotContainForbiddenHostMutationCommandStrings()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var serialized = JsonSerializer.Serialize(contract);
        var forbiddenCommands = new[]
        {
            "Install-Service",
            "Start-Service",
            "Stop-Service",
            "DeleteService",
            "msiexec",
            "New-NetFirewallRule",
            "Remove-NetFirewallRule",
            "New-EventLog",
            "CreateEventSource",
            "Register-ScheduledTask",
            "Unregister-ScheduledTask",
            "New-VM",
            "Remove-VM",
            "Start-VM",
            "Stop-VM",
            "Checkpoint-VM"
        };

        foreach (var forbiddenCommand in forbiddenCommands)
        {
            Assert.DoesNotContain(forbiddenCommand, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void HyperVRouteFamiliesStayNativeOwnedDuringDomainSplit()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var hyperVRoutes = contract.Routes
            .Where(route => route.RouteFamily.StartsWith("hyperv-", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(hyperVRoutes);
        Assert.All(hyperVRoutes, route => Assert.Equal("dotnet-native-adapter", route.DefaultOwner));
        Assert.All(hyperVRoutes, route => Assert.DoesNotContain("powershell", route.DefaultOwner, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DefaultContractGroupsRuntimeCoreRoutesByFamily()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));

        Assert.Equal("runtime-policy", routes[("GET", "/api/v1/runtime/policy")].RouteFamily);
        Assert.Equal("jobs", routes[("GET", "/api/v1/jobs")].RouteFamily);
        Assert.Equal("jobs", routes[("GET", "/api/v1/jobs/{jobId}")].RouteFamily);
        Assert.Equal("jobs", routes[("POST", "/api/v1/jobs/{jobId}/cancel")].RouteFamily);
        Assert.Equal("jobs", routes[("POST", "/api/v1/jobs/{jobId}/retry")].RouteFamily);
        Assert.Equal("jobs", routes[("POST", "/api/v1/jobs/{jobId}/reconcile")].RouteFamily);
        Assert.Equal("ops-summary", routes[("GET", "/api/v1/ops/summary")].RouteFamily);
        Assert.Equal("diagnostics", routes[("GET", "/api/v1/diagnostics/bundles")].RouteFamily);
        Assert.Equal("diagnostics", routes[("POST", "/api/v1/diagnostics/bundles")].RouteFamily);
        Assert.Equal("diagnostics", routes[("GET", "/api/v1/diagnostics/bundles/{bundleId}/download")].RouteFamily);
        Assert.Equal("auth", routes[("POST", "/api/v1/auth/login")].RouteFamily);
        Assert.Equal("auth", routes[("POST", "/api/v1/auth/refresh")].RouteFamily);
        Assert.Equal("auth", routes[("POST", "/api/v1/auth/logout")].RouteFamily);
        Assert.Equal("auth", routes[("GET", "/api/v1/auth/session")].RouteFamily);
        Assert.Equal("auth", routes[("GET", "/api/v1/auth/rbac")].RouteFamily);
        Assert.Equal("console", routes[("GET", "/api/v1/console/capabilities")].RouteFamily);
        Assert.Equal("console", routes[("GET", "/api/v1/vms/{vmId}/console")].RouteFamily);

        Assert.Equal("hyperv-host", routes[("GET", "/api/v1/host/status")].RouteFamily);
        Assert.Equal("hyperv-network", routes[("GET", "/api/v1/network/inventory")].RouteFamily);
        Assert.Equal("hyperv-vm", routes[("GET", "/api/v1/vms")].RouteFamily);
        Assert.Equal("hyperv-vm-qos", routes[("GET", "/api/v1/vms/{vmId}/blkio")].RouteFamily);
        Assert.Equal("hyperv-vm-qos", routes[("GET", "/api/v1/vms/{vmId}/bandwidth")].RouteFamily);
        Assert.Equal("hyperv-vm-qos", routes[("POST", "/api/v1/vms/{vmId}/qos/storage/preview")].RouteFamily);
        Assert.Equal("hyperv-vm-qos", routes[("POST", "/api/v1/vms/{vmId}/qos/storage")].RouteFamily);
        Assert.Equal("hyperv-guest-service", routes[("GET", "/api/v1/vms/{vmId}/guest-agent/status")].RouteFamily);
        Assert.Equal("hyperv-guest-service", routes[("GET", "/api/v1/vms/{vmId}/guest-agent/ping")].RouteFamily);
        Assert.Equal("guest-execution", routes[("POST", "/api/v1/vms/{vmId}/guest/exec/preview")].RouteFamily);
        Assert.Equal("guest-execution", routes[("POST", "/api/v1/vms/{vmId}/guest/exec")].RouteFamily);
        Assert.Equal("guest-execution", routes[("POST", "/api/v1/vms/{vmId}/guest/channel/preview")].RouteFamily);
        Assert.Equal("guest-execution", routes[("POST", "/api/v1/vms/{vmId}/guest/channel/verify")].RouteFamily);
        Assert.Equal("guest-execution", routes[("POST", "/api/v1/vms/{vmId}/guest/channel")].RouteFamily);
        Assert.Equal("jobs", routes[("GET", "/api/v1/vms/{vmId}/delete-status")].RouteFamily);
        Assert.Equal("hyperv-vm", routes[("DELETE", "/api/v1/vms/{vmId}")].RouteFamily);
        Assert.Equal("hyperv-checkpoint", routes[("GET", "/api/v1/vms/{vmId}/checkpoints")].RouteFamily);
    }

    [Fact]
    public void DefaultContractPublishesRbacPermissionForRuntimeCoreRoutes()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));

        Assert.Null(routes[("GET", "/api/v1/runtime/policy")].RequiredPermission);
        Assert.Equal("read", routes[("GET", "/api/v1/jobs")].RequiredPermission);
        Assert.Equal("read", routes[("GET", "/api/v1/jobs/{jobId}")].RequiredPermission);
        Assert.Equal("operate", routes[("POST", "/api/v1/jobs/{jobId}/cancel")].RequiredPermission);
        Assert.Equal("operate", routes[("POST", "/api/v1/jobs/{jobId}/retry")].RequiredPermission);
        Assert.Equal("operate", routes[("POST", "/api/v1/jobs/{jobId}/reconcile")].RequiredPermission);
        Assert.Equal("read", routes[("GET", "/api/v1/ops/summary")].RequiredPermission);
        Assert.Equal("diagnostics.read", routes[("GET", "/api/v1/diagnostics/bundles")].RequiredPermission);
        Assert.Equal("diagnostics.create", routes[("POST", "/api/v1/diagnostics/bundles")].RequiredPermission);
        Assert.Equal("diagnostics.read", routes[("GET", "/api/v1/diagnostics/bundles/{bundleId}/download")].RequiredPermission);
        Assert.Equal("guest.exec", routes[("POST", "/api/v1/vms/{vmId}/guest/exec/preview")].RequiredPermission);
        Assert.Equal("guest.exec", routes[("POST", "/api/v1/vms/{vmId}/guest/exec")].RequiredPermission);
        Assert.Equal("guest.channel.configure", routes[("POST", "/api/v1/vms/{vmId}/guest/channel/preview")].RequiredPermission);
        Assert.Equal("guest.channel.configure", routes[("POST", "/api/v1/vms/{vmId}/guest/channel/verify")].RequiredPermission);
        Assert.Equal("guest.channel.configure", routes[("POST", "/api/v1/vms/{vmId}/guest/channel")].RequiredPermission);
    }

    [Fact]
    public void DefaultContractPublishesPost04218RuntimeEvidenceBridgeFamilies()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var families = contract.RouteFamilies.ToDictionary(family => family.RouteFamily, StringComparer.Ordinal);

        Assert.Equal("route-family-evidence-linked", contract.RuntimeApiDiagnosticsBridge);
        Assert.Equal("runtime-api-job-runtime-contract", families["jobs"].EvidenceBridge);
        Assert.Equal("runtime-api-ops-summary-current-card", families["ops-summary"].EvidenceBridge);
        Assert.Equal("runtime-api-diagnostics-bundle-contract", families["diagnostics"].EvidenceBridge);

        Assert.Equal("dotnet-runtime", families["jobs"].DefaultOwner);
        Assert.Equal("dotnet-runtime", families["ops-summary"].DefaultOwner);
        Assert.Equal("dotnet-runtime", families["diagnostics"].DefaultOwner);

        Assert.Contains("GET /api/v1/jobs", families["jobs"].Routes);
        Assert.Contains("POST /api/v1/jobs/{jobId}/cancel", families["jobs"].Routes);
        Assert.Contains("GET /api/v1/ops/summary", families["ops-summary"].Routes);
        Assert.Contains("GET /api/v1/diagnostics/bundles", families["diagnostics"].Routes);
        Assert.Contains("POST /api/v1/diagnostics/bundles", families["diagnostics"].Routes);
        Assert.Contains("GET /api/v1/diagnostics/bundles/{bundleId}/download", families["diagnostics"].Routes);

        Assert.DoesNotContain(families.Values, family => family.EvidenceBridge.Contains("public", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(families.Values, family => family.Routes.Any(route => route.Contains("msiexec", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void RuntimeEvidenceContractPinsDiagnosticsAndOpsSummaryRoutes()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var runtimeEvidence = contract.RuntimeEvidenceContract;
        var families = runtimeEvidence.Families.ToDictionary(family => family.RouteFamily, StringComparer.Ordinal);

        Assert.Equal("runtime-api-diagnostics-ops-summary-contract-v1", runtimeEvidence.ContractKey);
        Assert.Equal(["diagnostics", "ops-summary"], families.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());

        Assert.Equal("dotnet-runtime", families["ops-summary"].DefaultOwner);
        Assert.Equal("runtime-api-ops-summary-current-card", families["ops-summary"].EvidenceBridge);
        Assert.Equal(["GET /api/v1/ops/summary"], families["ops-summary"].ReadRoutes);
        Assert.Empty(families["ops-summary"].ProductOperationRoutes);
        Assert.Equal(["read"], families["ops-summary"].RequiredPermissions);

        Assert.Equal("dotnet-runtime", families["diagnostics"].DefaultOwner);
        Assert.Equal("runtime-api-diagnostics-bundle-contract", families["diagnostics"].EvidenceBridge);
        Assert.Equal(
            [
                "GET /api/v1/diagnostics/bundles",
                "GET /api/v1/diagnostics/bundles/{bundleId}/download"
            ],
            families["diagnostics"].ReadRoutes);
        Assert.Equal(["POST /api/v1/diagnostics/bundles"], families["diagnostics"].ProductOperationRoutes);
        Assert.Equal(["diagnostics.create", "diagnostics.read"], families["diagnostics"].RequiredPermissions);

        var serialized = JsonSerializer.Serialize(runtimeEvidence);
        Assert.DoesNotContain("QueuedMutation", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("msiexec", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RuntimeEvidenceContractLinksDocumentationAndHandlerRegistryRoutes()
    {
        var runtimeEvidence = ApiHandlerAdapterContract.CreateDefault().RuntimeEvidenceContract;

        Assert.Equal("runtime-api-diagnostics-ops-summary-registry-bridge-v2", runtimeEvidence.RegistryBridgeContractKey);
        Assert.Equal("DesktopNodeApiRuntimeRoutes", runtimeEvidence.HandlerRegistrySource);
        Assert.Equal(
            "docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md#runtime-api-diagnostics-ops-summary",
            runtimeEvidence.DocumentationAnchor);
        Assert.Equal(
            [
                "GET /api/v1/diagnostics/bundles -> ListDiagnosticBundles [runtime-api-diagnostics-bundle-contract]",
                "GET /api/v1/diagnostics/bundles/{bundleId}/download -> DownloadDiagnosticBundle [runtime-api-diagnostics-bundle-contract]",
                "GET /api/v1/ops/summary -> OpsSummary [runtime-api-ops-summary-current-card]",
                "POST /api/v1/diagnostics/bundles -> CreateDiagnosticBundle [runtime-api-diagnostics-bundle-contract]"
            ],
            runtimeEvidence.HandlerRegistryRouteKeys);

        var registeredEvidenceRoutes = DesktopNodeApiRuntimeRoutes.RouteRegistrations
            .Where(route => route.RouteFamily is "diagnostics" or "ops-summary")
            .Select(route => runtimeEvidence.FormatHandlerRegistryRouteKey(route))
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(runtimeEvidence.HandlerRegistryRouteKeys, registeredEvidenceRoutes);
    }

    [Fact]
    public void RuntimeRouteRegistryIsGeneratedFromTheAdapterContract()
    {
        var contract = ApiHandlerAdapterContract.CreateDefault();
        var expectedRuntimeRoutes = contract.Routes
            .Where(route => route.DefaultOwner == "dotnet-runtime")
            .Select(route => $"{route.Method} {route.RouteTemplate} {route.RouteFamily} {route.OperationName}")
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();

        var registeredRuntimeRoutes = DesktopNodeApiRuntimeRoutes.RouteRegistrations
            .Select(route => $"{route.Method} {route.RouteTemplate} {route.RouteFamily} {route.OperationName}")
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedRuntimeRoutes, registeredRuntimeRoutes);
        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchOperation("POST", "/api/v1/jobs/job-123/cancel", "CancelJob", out var cancelMatch));
        Assert.Equal("job-123", cancelMatch.Parameters["jobId"]);
        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchOperation("GET", "/api/v1/vms/demo/console", "GetVmConsoleSession", out var consoleMatch));
        Assert.Equal("demo", consoleMatch.Parameters["vmId"]);
        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchOperation("GET", "/api/v1/vms/demo/delete-status", "GetVmDeleteStatus", out var deleteStatusMatch));
        Assert.Equal("demo", deleteStatusMatch.Parameters["vmId"]);
        Assert.Equal("console.view", DesktopNodeApiRuntimeRoutes.RequiredPermissionFor("GET", "/api/v1/vms/demo/console"));
        Assert.Equal("read", DesktopNodeApiRuntimeRoutes.RequiredPermissionFor("GET", "/api/v1/vms/demo/delete-status"));
        Assert.False(DesktopNodeApiRuntimeRoutes.IsJobRoute("GET", "/api/v1/diagnostics/bundles"));
    }

    [Fact]
    public void RuntimeRouteRegistryPublishesNativeQueuedMutationMatchers()
    {
        var queuedRoutes = ApiHandlerAdapterContract.CreateDefault().Routes
            .Where(route => route.MutationStance == MutationStance.QueuedMutation)
            .Select(route => $"{route.Method} {route.RouteTemplate}")
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            queuedRoutes,
            DesktopNodeApiRuntimeRoutes.QueuedMutationRouteRegistrations
                .Select(route => $"{route.Method} {route.RouteTemplate}")
                .OrderBy(route => route, StringComparer.Ordinal)
                .ToArray());

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/start", out var startMatch));
        Assert.Equal("QueueStartVm", startMatch.Route.OperationName);
        Assert.Equal("lab vm", startMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/disk-resize", out var diskResizeMatch));
        Assert.Equal("QueueResizeVmDisk", diskResizeMatch.Route.OperationName);
        Assert.Equal("lab vm", diskResizeMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/limit", out var limitMatch));
        Assert.Equal("QueueSetVmLimit", limitMatch.Route.OperationName);
        Assert.Equal("lab vm", limitMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/qos/storage", out var storageQosMatch));
        Assert.Equal("QueueSetVmStorageQos", storageQosMatch.Route.OperationName);
        Assert.Equal("lab vm", storageQosMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/manage", out var manageMatch));
        Assert.Equal("QueueManageVm", manageMatch.Route.OperationName);
        Assert.Equal("lab vm", manageMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchContract("GET", "/api/v1/vms/lab%20vm/guest-agent/status", out var guestStatusMatch));
        Assert.Equal("GetVmGuestAgentStatus", guestStatusMatch.Route.OperationName);
        Assert.Equal("lab vm", guestStatusMatch.Parameters["vmId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation("DELETE", "/api/v1/vms/lab%20vm/checkpoints/before%20upgrade", out var deleteCheckpointMatch));
        Assert.Equal("QueueDeleteVmCheckpoint", deleteCheckpointMatch.Route.OperationName);
        Assert.Equal("lab vm", deleteCheckpointMatch.Parameters["vmId"]);
        Assert.Equal("before upgrade", deleteCheckpointMatch.Parameters["checkpointId"]);

        Assert.True(DesktopNodeApiRuntimeRoutes.IsQueuedMutationRoute("POST", "/api/v1/vms/lab%20vm/checkpoints"));
        Assert.True(DesktopNodeApiRuntimeRoutes.UsesJobStore("DELETE", "/api/v1/vms/lab%20vm"));
        Assert.False(DesktopNodeApiRuntimeRoutes.IsQueuedMutationRoute("GET", "/api/v1/vms/lab%20vm"));
    }

    private static JsonDocument LoadRepoJson(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, normalized);
            if (File.Exists(candidate))
            {
                return JsonDocument.Parse(File.ReadAllText(candidate));
            }
        }

        throw new FileNotFoundException("Could not locate repository JSON file.", relativePath);
    }

    private static void AssertRoute(
        ApiHandlerRouteContract route,
        string method,
        string operationName,
        MutationStance mutationStance)
    {
        Assert.Equal(method, route.Method);
        Assert.Equal(operationName, route.OperationName);
        Assert.Equal(mutationStance, route.MutationStance);
    }
}

using System.Text;
using System.Text.Json;
using DesktopNode.Host;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host.Tests;
public sealed partial class DesktopNodeHostServiceActionTests
{
    private static readonly DesktopNodeHostOptions NativeActionOptions = new()
    {
        Mode = DesktopNodeHostMode.ServiceAction,
        ServiceAction = "status",
        ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
        ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
    };

    [Theory]
    [InlineData("status", "service-lifecycle")]
    [InlineData("start", "service-lifecycle")]
    [InlineData("stop", "service-lifecycle")]
    [InlineData("configure-installed", "service-lifecycle")]
    [InlineData("repair-installed", "service-lifecycle")]
    [InlineData("remove-installed", "service-lifecycle")]
    [InlineData("data-root-remove", "data-root")]
    [InlineData("config-migration-apply", "config-migration")]
    [InlineData("job-store-migration-apply", "job-store-migration")]
    [InlineData("service-token-rotation-revoke", "service-token")]
    [InlineData("credential-manager-system-proof", "credential-manager")]
    [InlineData("credential-manager-default-transition", "credential-manager")]
    [InlineData("eventlog-register", "event-log")]
    [InlineData("eventlog-remove", "event-log")]
    [InlineData("eventlog-repair", "event-log")]
    [InlineData("eventlog-write-test", "event-log")]
    [InlineData("eventlog-volume-guard", "event-log")]
    [InlineData("eventlog-default-transition", "event-log")]
    [InlineData("firewall-enable", "firewall")]
    [InlineData("firewall-remove", "firewall")]
    [InlineData("trust-store-install", "trust-store")]
    [InlineData("trust-store-remove", "trust-store")]
    public void ServiceActionPlansDeclareStableOperationFamilies(string action, string family)
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(OperationFamilyOptions(action));

        Assert.Equal(family, plan.OperationFamily);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void ServiceLifecycleOpsOwnsNativeServiceFamilyDelegation()
    {
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "status",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            DryRun = true
        };
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);

        var result = DesktopNodeServiceLifecycleOps.Execute(
            options,
            plan,
            new FakeWindowsServiceController(),
            new FakeWindowsCredentialManagerController(),
            new RecordingFileAclHardener());

        Assert.True(result.Ok);
        Assert.Equal("service-lifecycle", result.Plan.OperationFamily);
    }

    [Fact]
    public void HostOpsCatalogDeclaresIndependentOperationOwners()
    {
        var catalog = DesktopNodeHostOpsCatalog.Entries.ToDictionary(entry => entry.OperationFamily);

        Assert.Equal(9, catalog.Count);
        Assert.Equal(22, catalog.Values.Sum(entry => entry.Operations.Count));
        Assert.Equal(
            22,
            catalog.Values
                .SelectMany(entry => entry.Operations)
                .Distinct(StringComparer.Ordinal)
                .Count());

        Assert.Equal("DesktopNodeServiceLifecycleOps", catalog["service-lifecycle"].Owner);
        Assert.Equal("DesktopNodeDataRootLifecycleOps", catalog["data-root"].Owner);
        Assert.Equal("DesktopNodeConfigMigrationOps", catalog["config-migration"].Owner);
        Assert.Equal("DesktopNodeJobStoreMigrationOps", catalog["job-store-migration"].Owner);
        Assert.Equal("DesktopNodeServiceTokenOps", catalog["service-token"].Owner);
        Assert.Equal("DesktopNodeEventLogOps", catalog["event-log"].Owner);
        Assert.Equal("DesktopNodeFirewallOps", catalog["firewall"].Owner);
        Assert.Equal("DesktopNodeTrustStoreOps", catalog["trust-store"].Owner);
        Assert.Equal("DesktopNodeCredentialManagerOps", catalog["credential-manager"].Owner);
        Assert.Contains("data-root-remove", catalog["data-root"].Operations);
        Assert.Contains("config-migration-apply", catalog["config-migration"].Operations);
        Assert.Contains("job-store-migration-apply", catalog["job-store-migration"].Operations);
        Assert.Contains("service-token-rotation-revoke", catalog["service-token"].Operations);
        Assert.DoesNotContain("data-root-remove", catalog["service-lifecycle"].Operations);
        Assert.DoesNotContain("service-token-rotation-revoke", catalog["service-lifecycle"].Operations);
    }

    [Fact]
    public void HostOpsCatalogOwnsEveryNativeServiceActionPlanFamily()
    {
        foreach (var entry in DesktopNodeHostOpsCatalog.Entries)
        {
            foreach (var operation in entry.Operations)
            {
                Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation(operation, out var operationEntry));
                Assert.Equal(entry.OperationFamily, operationEntry.OperationFamily);
                Assert.Equal(entry.Owner, operationEntry.Owner);

                var plan = DesktopNodeHostServiceAction.CreatePlan(OperationFamilyOptions(operation));

                Assert.Equal(entry.OperationFamily, plan.OperationFamily);
                Assert.Empty(plan.Commands);
            }
        }
    }

    [Fact]
    public void HostOpsCatalogSeparatesServiceActionEventLogFirewallAndTrustStoreDomains()
    {
        var catalog = DesktopNodeHostOpsCatalog.Entries.ToDictionary(entry => entry.OperationFamily);

        Assert.Contains("repair-installed", catalog["service-lifecycle"].Operations);
        Assert.DoesNotContain("eventlog-repair", catalog["service-lifecycle"].Operations);
        Assert.Contains("eventlog-repair", catalog["event-log"].Operations);
        Assert.Contains("firewall-enable", catalog["firewall"].Operations);
        Assert.Contains("trust-store-install", catalog["trust-store"].Operations);

        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("repair-installed", out var serviceAction));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("eventlog-repair", out var eventLog));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("firewall-enable", out var firewall));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("trust-store-install", out var trustStore));

        Assert.Equal("service-lifecycle", serviceAction.OperationFamily);
        Assert.Equal("event-log", eventLog.OperationFamily);
        Assert.Equal("firewall", firewall.OperationFamily);
        Assert.Equal("trust-store", trustStore.OperationFamily);
    }

    [Fact]
    public void HostOpsCatalogPublishesSeparatedMutationBoundaries()
    {
        var catalog = DesktopNodeHostOpsCatalog.Entries.ToDictionary(entry => entry.OperationFamily);

        Assert.Equal("windows-service-control-manager", catalog["service-lifecycle"].MutationBoundary);
        Assert.Equal("windows-event-log-provider", catalog["event-log"].MutationBoundary);
        Assert.Equal("windows-firewall-rule", catalog["firewall"].MutationBoundary);
        Assert.Equal("windows-x509-store", catalog["trust-store"].MutationBoundary);
        Assert.Equal("allowlisted-programdata-root", catalog["data-root"].MutationBoundary);
        Assert.Equal("windows-credential-manager", catalog["credential-manager"].MutationBoundary);

        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("repair-installed", out var serviceLifecycle));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("firewall-enable", out var firewall));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("trust-store-install", out var trustStore));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("credential-manager-default-transition", out var credentialManager));
        Assert.True(DesktopNodeHostOpsCatalog.TryGetOperation("data-root-remove", out var dataRoot));

        Assert.Equal("windows-service-control-manager", serviceLifecycle.MutationBoundary);
        Assert.Equal("windows-firewall-rule", firewall.MutationBoundary);
        Assert.Equal("windows-x509-store", trustStore.MutationBoundary);
        Assert.Equal("windows-credential-manager", credentialManager.MutationBoundary);
        Assert.Equal("allowlisted-programdata-root", dataRoot.MutationBoundary);
        Assert.Equal("windows-firewall-rule", DesktopNodeHostOpsCatalog.MutationBoundaryForOperation("firewall-enable"));
        Assert.Equal("windows-credential-manager", DesktopNodeHostOpsCatalog.MutationBoundaryForOperation("credential-manager-system-proof"));
        Assert.Null(DesktopNodeHostOpsCatalog.MutationBoundaryForOperation("unknown-operation"));
    }

    [Fact]
    public void HostOpsCatalogPublishesDryRunAndMutationEvidenceReasonCodes()
    {
        foreach (var entry in DesktopNodeHostOpsCatalog.Entries)
        {
            Assert.StartsWith("HOST_OPS_DRY_RUN_", entry.DryRunEvidenceReasonCode, StringComparison.Ordinal);
            Assert.StartsWith("HOST_OPS_MUTATION_", entry.MutationEvidenceReasonCode, StringComparison.Ordinal);
            Assert.DoesNotContain(" ", entry.DryRunEvidenceReasonCode, StringComparison.Ordinal);
            Assert.DoesNotContain("-", entry.DryRunEvidenceReasonCode, StringComparison.Ordinal);
            Assert.DoesNotContain(" ", entry.MutationEvidenceReasonCode, StringComparison.Ordinal);
            Assert.DoesNotContain("-", entry.MutationEvidenceReasonCode, StringComparison.Ordinal);

            foreach (var operation in entry.Operations)
            {
                Assert.Equal(entry.DryRunEvidenceReasonCode, DesktopNodeHostOpsCatalog.DryRunEvidenceReasonForOperation(operation));
                Assert.Equal(entry.MutationEvidenceReasonCode, DesktopNodeHostOpsCatalog.MutationEvidenceReasonForOperation(operation));
            }
        }

        Assert.Equal("HOST_OPS_DRY_RUN_WINDOWS_FIREWALL_RULE", DesktopNodeHostOpsCatalog.DryRunEvidenceReasonForOperation("firewall-enable"));
        Assert.Equal("HOST_OPS_MUTATION_WINDOWS_CREDENTIAL_MANAGER", DesktopNodeHostOpsCatalog.MutationEvidenceReasonForOperation("credential-manager-system-proof"));
        Assert.Equal("HOST_OPS_DRY_RUN_ALLOWLISTED_PROGRAMDATA_ROOT", DesktopNodeHostOpsCatalog.DryRunEvidenceReasonForOperation("data-root-remove"));
        Assert.Null(DesktopNodeHostOpsCatalog.DryRunEvidenceReasonForOperation("unknown-operation"));
        Assert.Null(DesktopNodeHostOpsCatalog.MutationEvidenceReasonForOperation("unknown-operation"));
    }

    [Fact]
    public void HostOpsCatalogPublishesPost04218LifecycleBucketContract()
    {
        Assert.Equal(
            "service-eventlog-firewall-truststore-data-root-separated",
            DesktopNodeHostOpsCatalog.LifecycleBucketContractKey);
        Assert.Equal(
            [
                "service-lifecycle",
                "event-log",
                "firewall",
                "trust-store",
                "data-root"
            ],
            DesktopNodeHostOpsCatalog.RequiredLifecycleSmokeBuckets);

        foreach (var bucket in DesktopNodeHostOpsCatalog.RequiredLifecycleSmokeBuckets)
        {
            Assert.True(DesktopNodeHostOpsCatalog.TryGetEntry(bucket, out var entry));
            Assert.NotEmpty(entry.Operations);
        }

        Assert.True(DesktopNodeHostOpsCatalog.OperationBelongsTo("status", "service-lifecycle"));
        Assert.True(DesktopNodeHostOpsCatalog.OperationBelongsTo("eventlog-default-transition", "event-log"));
        Assert.True(DesktopNodeHostOpsCatalog.OperationBelongsTo("firewall-enable", "firewall"));
        Assert.True(DesktopNodeHostOpsCatalog.OperationBelongsTo("trust-store-install", "trust-store"));
        Assert.True(DesktopNodeHostOpsCatalog.OperationBelongsTo("data-root-remove", "data-root"));
    }

    [Fact]
    public void HostOpsCatalogPublishesCurrentEvidenceLifecycleDescriptorBridge()
    {
        var descriptor = DesktopNodeHostOpsCatalog.CreateLifecycleDescriptor();

        Assert.Equal("host-ops-lifecycle-descriptor-bridge-v1", descriptor.ContractKey);
        Assert.Equal(
            "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated",
            descriptor.LifecycleBucketContractKey);
        Assert.Equal(
            [
                "service-action",
                "event-log",
                "firewall",
                "trust-store",
                "credential-manager",
                "data-root"
            ],
            descriptor.Buckets.Select(bucket => bucket.BucketKey).ToArray());

        foreach (var bucket in descriptor.Buckets)
        {
            Assert.True(DesktopNodeHostOpsCatalog.TryGetEntry(bucket.OperationFamily, out var entry));
            Assert.Equal(entry.Owner, bucket.Owner);
            Assert.Equal(entry.MutationBoundary, bucket.MutationBoundary);
            Assert.Equal(entry.Operations, bucket.Operations);
        }
    }

    [Fact]
    public void HostOpsFamilyHelpersOwnLifecycleBucketsOutsideTheRequestProcessor()
    {
        Assert.Equal("service-lifecycle", DesktopNodeServiceLifecycleOps.OperationFamily);
        Assert.Equal("event-log", DesktopNodeEventLogOps.OperationFamily);
        Assert.Equal("firewall", DesktopNodeFirewallOps.OperationFamily);
        Assert.Equal("trust-store", DesktopNodeTrustStoreOps.OperationFamily);
        Assert.Equal("data-root", DesktopNodeDataRootLifecycleOps.OperationFamily);

        Assert.True(DesktopNodeServiceLifecycleOps.Owns("repair-installed"));
        Assert.True(DesktopNodeEventLogOps.Owns("eventlog-default-transition"));
        Assert.True(DesktopNodeFirewallOps.Owns("firewall-enable"));
        Assert.True(DesktopNodeTrustStoreOps.Owns("trust-store-install"));
        Assert.True(DesktopNodeDataRootLifecycleOps.Owns("data-root-remove"));
        Assert.False(DesktopNodeServiceLifecycleOps.Owns("data-root-remove"));

        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("repair-installed"));
        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("data-root-remove"));
        Assert.False(DesktopNodeHostOpsCatalog.RequiresDataRoot("status"));
    }

    [Fact]
    public void HostOpsFamilyHelpersOwnConfigJobTokenAndCredentialBuckets()
    {
        Assert.Equal("config-migration", DesktopNodeConfigMigrationOps.OperationFamily);
        Assert.Equal("job-store-migration", DesktopNodeJobStoreMigrationOps.OperationFamily);
        Assert.Equal("service-token", DesktopNodeServiceTokenOps.OperationFamily);
        Assert.Equal("credential-manager", DesktopNodeCredentialManagerOps.OperationFamily);

        Assert.True(DesktopNodeConfigMigrationOps.Owns("config-migration-apply"));
        Assert.True(DesktopNodeJobStoreMigrationOps.Owns("job-store-migration-apply"));
        Assert.True(DesktopNodeServiceTokenOps.Owns("service-token-rotation-revoke"));
        Assert.True(DesktopNodeCredentialManagerOps.Owns("credential-manager-system-proof"));
        Assert.True(DesktopNodeCredentialManagerOps.Owns("credential-manager-default-transition"));
        Assert.False(DesktopNodeServiceTokenOps.Owns("credential-manager-default-transition"));

        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("config-migration-apply"));
        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("job-store-migration-apply"));
        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("service-token-rotation-revoke"));
        Assert.True(DesktopNodeHostOpsCatalog.RequiresDataRoot("credential-manager-default-transition"));
    }

    [Fact]
    public void ConfigMigrationOpsOwnsNativeConfigMigrationDelegation()
    {
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "config-migration-apply",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            DryRun = true
        };
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);

        var result = DesktopNodeConfigMigrationOps.Execute(
            options,
            plan,
            new FakeWindowsServiceController());

        Assert.True(result.Ok);
        Assert.Equal("config-migration", result.Plan.OperationFamily);
        Assert.Equal("config-migration-apply", result.Plan.NativeConfigMigrationOperation);
        Assert.Null(result.Plan.NativeServiceOperation);
    }

    [Fact]
    public void JobStoreMigrationOpsOwnsNativeJobStoreMigrationDelegation()
    {
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "job-store-migration-apply",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            DryRun = true
        };
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);

        var result = DesktopNodeJobStoreMigrationOps.Execute(
            options,
            plan,
            new FakeWindowsServiceController());

        Assert.True(result.Ok);
        Assert.Equal("job-store-migration", result.Plan.OperationFamily);
        Assert.Equal("job-store-migration-apply", result.Plan.NativeJobStoreMigrationOperation);
        Assert.Null(result.Plan.NativeServiceOperation);
    }

    [Fact]
    public void ServiceTokenOpsOwnsNativeServiceTokenDelegation()
    {
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "service-token-rotation-revoke",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            DryRun = true
        };
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);

        var result = DesktopNodeServiceTokenOps.Execute(
            options,
            plan,
            new FakeWindowsServiceController(),
            new RecordingFileAclHardener());

        Assert.True(result.Ok);
        Assert.Equal("service-token", result.Plan.OperationFamily);
        Assert.Equal("service-token-rotation-revoke", result.Plan.NativeServiceTokenOperation);
        Assert.Null(result.Plan.NativeServiceOperation);
    }

    [Fact]
    public void DataRootLifecycleOpsOwnsNativeDataRootDelegation()
    {
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "data-root-remove",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            RemoveData = true,
            DryRun = true
        };
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);

        var result = DesktopNodeDataRootLifecycleOps.Execute(
            options,
            plan,
            new FakeWindowsServiceController());

        Assert.True(result.Ok);
        Assert.Equal("data-root", result.Plan.OperationFamily);
        Assert.Null(result.Plan.NativeServiceOperation);
        Assert.Equal("data-root-remove", result.Plan.NativeDataRootLifecycleOperation);
    }

    [Fact]
    public void ConfigureInstalledPlanUsesNativeServiceActionWithoutScmCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "configure-installed",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("dotnet-windows-service", plan.ServiceMode);
        Assert.Equal("DesktopNode.Host.exe", Path.GetFileName(plan.ServiceExecutablePath));
        Assert.Equal("configure-installed", plan.NativeServiceOperation);
        Assert.Contains("--max-request-body-bytes", plan.ServiceBinaryPathName);
        Assert.Contains("1048576", plan.ServiceBinaryPathName);
        Assert.DoesNotContain("--controlled-route-timeout-probe-delay-ms", plan.ServiceBinaryPathName);
        Assert.Empty(plan.Commands);
        Assert.Contains("C:\\ProgramData\\PureCVisor\\desktop-node\\accounts.json", plan.RemoveDataPaths);
        Assert.Contains("C:\\ProgramData\\PureCVisor\\desktop-node\\jwt-signing-key.txt", plan.RemoveDataPaths);
    }

    [Fact]
    public void ConfigureInstalledPlanCanOptIntoControlledRouteTimeoutProbe()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "configure-installed",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            RouteTimeoutSeconds = 1,
            ControlledRouteTimeoutProbeDelayMilliseconds = 2_500
        });

        Assert.Contains("--route-timeout-seconds 1", plan.ServiceBinaryPathName);
        Assert.Contains("--controlled-route-timeout-probe-delay-ms 2500", plan.ServiceBinaryPathName);
    }

    [Fact]
    public void RepairInstalledPlanUsesNativeServiceActionWithoutScmCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "repair-installed",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("repair-installed", plan.NativeServiceOperation);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public async Task RepairInstalledSkipsServiceMutationWhenRunningServiceAlreadyMatches()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-repair-noop-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var options = NativeActionOptions.WithAction("repair-installed").WithDataRoot(dataRoot);
            var plan = DesktopNodeHostServiceAction.CreatePlan(options);
            var hardener = new RecordingFileAclHardener();
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: plan.ServiceBinaryPathName,
                    Win32ExitCode: 0)
            };

            var result = await ExecuteWithHardenerAsync(options, controller, hardener);

            Assert.True(result.Ok);
            Assert.Equal(["query"], controller.Calls);
            Assert.Empty(controller.Configurations);
            Assert.True(File.Exists(result.PreparedTokenPath));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void RemoveInstalledPlanUsesNativeServiceActionWithoutScmCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "remove-installed",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("remove-installed", plan.NativeServiceOperation);
        Assert.Empty(plan.Commands);
        Assert.Contains(
            Path.Combine("C:\\ProgramData\\PureCVisor\\desktop-node", "jobs.json.commit-pending"),
            plan.RemoveDataPaths);
        Assert.Contains(
            Path.Combine("C:\\ProgramData\\PureCVisor\\desktop-node", "jobs.json.tmp"),
            plan.RemoveDataPaths);
    }

    [Fact]
    public void DataRootRemovePlanUsesNativeDataRootLifecycleActionWithoutScmCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "data-root-remove",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            RemoveData = true
        });

        Assert.Null(plan.NativeServiceOperation);
        Assert.Equal("data-root-remove", plan.NativeDataRootLifecycleOperation);
        Assert.Empty(plan.Commands);
        Assert.Contains("C:\\ProgramData\\PureCVisor\\desktop-node\\api-token.dpapi.json", plan.RemoveDataPaths);
    }

    [Fact]
    public void ConfigMigrationApplyPlanIsAcceptedWithoutExternalCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "config-migration-apply",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("dotnet-windows-service", plan.ServiceMode);
        Assert.Equal("config-migration-apply", plan.NativeConfigMigrationOperation);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public async Task ConfigMigrationApplyBlocksMissingManifestWithoutCreatingBackup()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "config-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe
                },
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_CONFIG_MIGRATION_PRECONDITION_MISSING", result.ErrorCode);
            Assert.Empty(result.Commands);
            Assert.Equal(["query"], controller.Calls);
            Assert.False(Directory.Exists(Path.Combine(dataRoot, "backups")));
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConfigMigrationApplyBlocksInvalidManifestBeforePlanEvaluation()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        File.WriteAllText(Path.Combine(productRoot, "product-manifest.json"), "{}", Encoding.UTF8);
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "config-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "product-config-v1-to-v2",
                    MigrationPlanVersion = 1
                },
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_CONFIG_MIGRATION_PRECONDITION_MISSING", result.ErrorCode);
            Assert.Empty(result.Commands);
            Assert.Equal(["query"], controller.Calls);
            Assert.False(Directory.Exists(Path.Combine(dataRoot, "backups")));
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConfigMigrationApplyBlocksRunningServiceWithoutImplicitStop()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        File.WriteAllText(
            Path.Combine(productRoot, "product-manifest.json"),
            """
            {
              "schema_version": 1,
              "product": "PureCVisor Desktop Node",
              "version": "0.35.6"
            }
            """,
            Encoding.UTF8);
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "running",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "config-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "product-config-v1-to-v2",
                    MigrationPlanVersion = 1
                },
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_CONFIG_MIGRATION_SERVICE_RUNNING", result.ErrorCode);
            Assert.Equal(["query"], controller.Calls);
            Assert.False(Directory.Exists(Path.Combine(dataRoot, "backups")));
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConfigMigrationApplyReturnsNoMutationDescriptorForUnsupportedPlan()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        WriteProductManifest(productRoot, "0.35.6");
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "config-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "unknown-plan",
                    MigrationPlanVersion = 99
                },
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_CONFIG_MIGRATION_PLAN_UNSUPPORTED", result.ErrorCode);
            Assert.NotNull(result.ConfigMigration);
            Assert.Equal("product.config.migration.apply", result.ConfigMigration.Operation);
            Assert.False(result.ConfigMigration.MutationPlanned);
            Assert.False(result.ConfigMigration.MutationPerformed);
            Assert.True(result.ConfigMigration.ServiceStopped);
            Assert.Equal("unknown-plan", result.ConfigMigration.MigrationPlanId);
            Assert.Equal(99, result.ConfigMigration.MigrationPlanVersion);
            var source = Assert.Single(result.ConfigMigration.ConfigSources);
            Assert.Equal("product-manifest", source.Name);
            Assert.Equal(Path.Combine(productRoot, "product-manifest.json"), source.Path);
            Assert.True(source.Owned);
            Assert.Equal(1, source.SchemaVersion);
            Assert.Equal("0.35.6", source.Version);
            Assert.Equal(["query"], controller.Calls);
            Assert.False(Directory.Exists(Path.Combine(dataRoot, "backups")));
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConfigMigrationApplyBacksUpAndAtomicallyRewritesSupportedManifestPlan()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-config-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        WriteProductManifest(productRoot, "0.38.4");
        var manifestPath = Path.Combine(productRoot, "product-manifest.json");
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "config-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "product-config-v1-to-v2",
                    MigrationPlanVersion = 1
                },
                controller);

            Assert.True(result.Ok);
            Assert.Null(result.ErrorCode);
            Assert.NotNull(result.ConfigMigration);
            Assert.True(result.ConfigMigration.Ok);
            Assert.True(result.ConfigMigration.MutationPlanned);
            Assert.True(result.ConfigMigration.MutationPerformed);
            Assert.Equal(1, result.ConfigMigration.SourceSchemaVersion);
            Assert.Equal(2, result.ConfigMigration.TargetSchemaVersion);
            Assert.False(result.ConfigMigration.RollbackAttempted);
            Assert.True(File.Exists(result.ConfigMigration.BackupPath));
            Assert.False(File.Exists(result.ConfigMigration.TempPath));
            Assert.Equal(["query"], controller.Calls);

            using var backup = JsonDocument.Parse(File.ReadAllText(result.ConfigMigration.BackupPath!));
            Assert.Equal(1, backup.RootElement.GetProperty("schema_version").GetInt32());
            Assert.Equal("0.38.4", backup.RootElement.GetProperty("version").GetString());

            using var migrated = JsonDocument.Parse(File.ReadAllText(manifestPath));
            Assert.Equal(2, migrated.RootElement.GetProperty("schema_version").GetInt32());
            Assert.Equal("PureCVisor Desktop Node", migrated.RootElement.GetProperty("product").GetString());
            Assert.Equal("0.38.4", migrated.RootElement.GetProperty("version").GetString());
            var migration = migrated.RootElement.GetProperty("migration");
            Assert.Equal("product-config-v1-to-v2", migration.GetProperty("plan_id").GetString());
            Assert.Equal(1, migration.GetProperty("source_schema_version").GetInt32());
            Assert.Equal(2, migration.GetProperty("target_schema_version").GetInt32());
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task JobStoreMigrationApplyBacksUpAndAtomicallyRewritesSupportedStorePlan()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-job-store-migration-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-job-store-migration-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        var jobStorePath = Path.Combine(dataRoot, "jobs.json");
        File.WriteAllText(
            jobStorePath,
            """
            {"version":1,"saved_at":"2026-05-06T00:00:00Z","jobs":[{"job_id":"job-1","status":"queued"}],"queue":["job-1"]}
            """,
            Encoding.UTF8);
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "job-store-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "job-store-v1-to-v2",
                    MigrationPlanVersion = 1
                },
                controller);

            Assert.True(result.Ok);
            Assert.Null(result.ErrorCode);
            Assert.NotNull(result.JobStoreMigration);
            Assert.True(result.JobStoreMigration.Ok);
            Assert.True(result.JobStoreMigration.MutationPlanned);
            Assert.True(result.JobStoreMigration.MutationPerformed);
            Assert.Equal(1, result.JobStoreMigration.SourceSchemaVersion);
            Assert.Equal(2, result.JobStoreMigration.TargetSchemaVersion);
            Assert.Equal(1, result.JobStoreMigration.JobCount);
            Assert.Equal(1, result.JobStoreMigration.QueueCount);
            Assert.False(result.JobStoreMigration.RollbackAttempted);
            Assert.True(File.Exists(result.JobStoreMigration.BackupPath));
            Assert.False(File.Exists(result.JobStoreMigration.TempPath));
            Assert.Equal(["query"], controller.Calls);

            using var backup = JsonDocument.Parse(File.ReadAllText(result.JobStoreMigration.BackupPath!));
            Assert.Equal(1, backup.RootElement.GetProperty("version").GetInt32());

            using var migrated = JsonDocument.Parse(File.ReadAllText(jobStorePath));
            Assert.Equal(2, migrated.RootElement.GetProperty("version").GetInt32());
            Assert.Equal("job-1", migrated.RootElement.GetProperty("jobs")[0].GetProperty("job_id").GetString());
            Assert.Equal("job-1", migrated.RootElement.GetProperty("queue")[0].GetString());
            var migration = migrated.RootElement.GetProperty("migration");
            Assert.Equal("job-store-v1-to-v2", migration.GetProperty("plan_id").GetString());
            Assert.Equal(1, migration.GetProperty("source_schema_version").GetInt32());
            Assert.Equal(2, migration.GetProperty("target_schema_version").GetInt32());
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task JobStoreMigrationBlocksUnresolvedPendingCommitBeforeBackupOrRewrite()
    {
        var productRoot = Path.Combine(Path.GetTempPath(), "pcv-host-job-store-pending-product-" + Guid.NewGuid().ToString("N"));
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-job-store-pending-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(dataRoot);
        var jobStorePath = Path.Combine(dataRoot, "jobs.json");
        var pendingPath = jobStorePath + ".commit-pending";
        var original = """{"version":1,"saved_at":"2026-08-02T00:00:00Z","jobs":[],"queue":[]}""";
        File.WriteAllText(jobStorePath, original, Encoding.UTF8);
        File.WriteAllText(pendingPath, "pending-guard", Encoding.UTF8);
        var serviceExe = Path.Combine(productRoot, "DesktopNode.Host.exe");
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: $"\"{serviceExe}\" listen",
                Win32ExitCode: 0)
        };

        try
        {
            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.ServiceAction,
                    ServiceAction = "job-store-migration-apply",
                    ProductRoot = productRoot,
                    DataRoot = dataRoot,
                    ServiceExecutablePath = serviceExe,
                    MigrationPlanId = "job-store-v1-to-v2",
                    MigrationPlanVersion = 1
                },
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_JOB_STORE_PENDING_COMMIT_UNRESOLVED", result.ErrorCode);
            Assert.Equal(original, File.ReadAllText(jobStorePath, Encoding.UTF8));
            Assert.True(File.Exists(pendingPath));
            Assert.False(Directory.Exists(Path.Combine(dataRoot, "backups")));
            Assert.Equal(["query"], controller.Calls);
        }
        finally
        {
            Directory.Delete(productRoot, recursive: true);
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public void EventLogRegisterPlanUsesNativeEventLogActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "eventlog-register",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("eventlog-register", plan.NativeEventLogOperation);
        Assert.Equal("PureCVisor Desktop Node", plan.EventLogSourceName);
        Assert.Equal("Application", plan.EventLogName);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void EventLogRemovePlanUsesNativeEventLogActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "eventlog-remove",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("eventlog-remove", plan.NativeEventLogOperation);
        Assert.Equal("PureCVisor Desktop Node", plan.EventLogSourceName);
        Assert.Equal("Application", plan.EventLogName);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void EventLogHardeningPlansUseNativeEventLogActionsWithoutPowerShellCommands()
    {
        foreach (var action in new[] { "eventlog-repair", "eventlog-write-test", "eventlog-volume-guard", "eventlog-default-transition" })
        {
            var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.ServiceAction,
                ServiceAction = action,
                ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
                ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
            });

            Assert.Equal(action, plan.NativeEventLogOperation);
            Assert.Equal("PureCVisor Desktop Node", plan.EventLogSourceName);
            Assert.Equal("Application", plan.EventLogName);
            Assert.Empty(plan.Commands);
        }
    }

    [Fact]
    public void CredentialManagerSystemProofPlanUsesNativeServiceActionWithoutExternalCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "credential-manager-system-proof",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("credential-manager-system-proof", plan.NativeCredentialManagerOperation);
        Assert.Equal("PureCVisor/PureCVisorDesktopNode/api-token", plan.CredentialTarget);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void CredentialManagerDefaultTransitionPlanUsesNativeCredentialManagerActionWithoutExternalCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(NativeActionOptions
            .WithAction("credential-manager-default-transition")
            .WithDataRoot("C:\\ProgramData\\PureCVisor\\desktop-node"));

        Assert.Null(plan.NativeServiceOperation);
        Assert.Equal("credential-manager-default-transition", plan.NativeCredentialManagerOperation);
        Assert.Equal("PureCVisor/PureCVisorDesktopNode/api-token", plan.CredentialTarget);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void FirewallEnablePlanUsesNativeFirewallActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "firewall-enable",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            AllowLan = true
        });

        Assert.Equal("firewall-enable", plan.NativeFirewallOperation);
        Assert.Equal("PureCVisor Desktop Node Local API LAN", plan.FirewallRule?.RuleName);
        Assert.Equal("inbound", plan.FirewallRule?.Direction);
        Assert.Equal("TCP", plan.FirewallRule?.Protocol);
        Assert.Equal(7777, plan.FirewallRule?.LocalPort);
        Assert.Equal("Private", plan.FirewallRule?.Profile);
        Assert.Equal("LocalSubnet", plan.FirewallRule?.RemoteAddress);
        Assert.True(plan.LanExposureApproved);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void FirewallRemovePlanUsesNativeFirewallActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "firewall-remove",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        });

        Assert.Equal("firewall-remove", plan.NativeFirewallOperation);
        Assert.Equal("PureCVisor Desktop Node Local API LAN", plan.FirewallRule?.RuleName);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void FirewallRuleLookupTreatsComFileNotFoundAsMissingRule()
    {
        var error = new FileNotFoundException("Fast cache data was not found.");

        Assert.True(DesktopNodeWindowsFirewallController.IsMissingRuleLookupFailure(error));
    }

    [Fact]
    public void TrustStoreInstallPlanUsesNativeCertificateStoreActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "trust-store-install",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            ReleaseApproved = true,
            TrustRootCertificatePath = "C:\\certs\\PureCVisor-Internal-CodeSigning-Root.cer",
            TrustRootCertificateThumbprint = "00112233445566778899AABBCCDDEEFF00112233",
            TrustPublisherCertificatePath = "C:\\certs\\PureCVisor-DesktopNode-Internal-CodeSigning.cer",
            TrustPublisherCertificateThumbprint = "AABBCCDDEEFF00112233445566778899AABBCCDD"
        });

        Assert.Equal("trust-store-install", plan.NativeTrustStoreOperation);
        Assert.True(plan.ReleaseApproved);
        Assert.Equal(2, plan.TrustStoreCertificates.Count);
        Assert.Contains(plan.TrustStoreCertificates, certificate =>
            certificate.StoreName == "Root" &&
            certificate.StoreLocation == "LocalMachine" &&
            certificate.ExpectedSubject == "CN=PureCVisor Internal Code Signing Root CA" &&
            certificate.CertificatePath == "C:\\certs\\PureCVisor-Internal-CodeSigning-Root.cer");
        Assert.Contains(plan.TrustStoreCertificates, certificate =>
            certificate.StoreName == "TrustedPublisher" &&
            certificate.StoreLocation == "LocalMachine" &&
            certificate.ExpectedSubject == "CN=PureCVisor Desktop Node Internal Code Signing" &&
            certificate.CertificatePath == "C:\\certs\\PureCVisor-DesktopNode-Internal-CodeSigning.cer");
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void TrustStoreRemovePlanUsesNativeCertificateStoreActionWithoutPowerShellCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = "trust-store-remove",
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            ReleaseApproved = true,
            TrustRootCertificateThumbprint = "00112233445566778899AABBCCDDEEFF00112233",
            TrustPublisherCertificateThumbprint = "AABBCCDDEEFF00112233445566778899AABBCCDD"
        });

        Assert.Equal("trust-store-remove", plan.NativeTrustStoreOperation);
        Assert.True(plan.ReleaseApproved);
        Assert.Equal(2, plan.TrustStoreCertificates.Count);
        Assert.All(plan.TrustStoreCertificates, certificate => Assert.Null(certificate.CertificatePath));
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public void EnsureProtectedTokenFileCreatesDpapiTokenWithoutRawTokenMaterial()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-action-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var path = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);

            Assert.Equal(Path.GetFullPath(path), Assert.Single(hardener.Paths));
            var text = File.ReadAllText(path);
            Assert.Contains("\"storage\": \"dpapi-local-machine\"", text);
            Assert.DoesNotContain("Bearer", text, StringComparison.OrdinalIgnoreCase);

            var token = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = path
            });
            Assert.Equal("protected_file", token.Source);
            Assert.Equal("dpapi-local-machine", token.Storage);
            Assert.False(string.IsNullOrWhiteSpace(token.Value));
            Assert.DoesNotContain(token.Value!, text);
        }
        finally
        {
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public void EnsureAccountAuthBootstrapFilesHardensBothCreatedFiles()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-account-bootstrap-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();

            DesktopNodeHostServiceAction.EnsureAccountAuthBootstrapFiles(dataRoot, hardener);

            var accountPath = Path.Combine(dataRoot, "accounts.json");
            var signingKeyPath = Path.Combine(dataRoot, "jwt-signing-key.txt");
            Assert.Equal(
                new[] { Path.GetFullPath(accountPath), Path.GetFullPath(signingKeyPath) }.Order(),
                hardener.Paths.Order());
            using var accounts = JsonDocument.Parse(File.ReadAllText(accountPath));
            Assert.Equal("no-default-account", accounts.RootElement.GetProperty("bootstrap_state").GetString());
            Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(signingKeyPath)));
        }
        finally
        {
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public void ServiceTokenRotationRevokePlanUsesNativeServiceActionWithoutExternalCommands()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(NativeActionOptions
            .WithAction("service-token-rotation-revoke")
            .WithDataRoot("C:\\ProgramData\\PureCVisor\\desktop-node"));

        Assert.Null(plan.NativeServiceOperation);
        Assert.Equal("service-token-rotation-revoke", plan.NativeServiceTokenOperation);
        Assert.Empty(plan.Commands);
        Assert.Contains(plan.RemoveDataPaths, path => path.EndsWith("api-token.dpapi.json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ServiceTokenRotationRevokeReplacesProtectedTokenFileRestartsServiceAndWritesRedactedAudit()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-token-rotate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var tokenPath = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);
            var originalText = File.ReadAllText(tokenPath);
            var originalToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = tokenPath
            });
            var serviceBinaryPath = "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen --api-token-protected-file \"" + tokenPath + "\"";

            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: serviceBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: serviceBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: serviceBinaryPath,
                    Win32ExitCode: 0)
            };

            hardener.Paths.Clear();
            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions.WithAction("service-token-rotation-revoke").WithDataRoot(dataRoot),
                controller,
                hardener);

            // This test has failed intermittently and a bare Assert.True discarded the reason:
            // the result already carries the structured error the rotation produced, and the
            // rotation converts IOException/CryptographicException into Ok=false rather than
            // throwing. Surface it so the next reproduction names its own cause.
            Assert.True(
                result.Ok,
                $"rotation reported Ok=false. error_code={result.ErrorCode ?? "<null>"}; " +
                $"error_message={result.ErrorMessage ?? "<null>"}; " +
                $"service_token_mutation={result.ServiceTokenRotation?.ServiceTokenMutation ?? "<null>"}; " +
                $"atomic_replace_status={result.ServiceTokenRotation?.AtomicReplaceStatus ?? "<null>"}; " +
                $"backup_write_status={result.ServiceTokenRotation?.BackupWriteStatus ?? "<null>"}; " +
                $"service_reload_status={result.ServiceTokenRotation?.ServiceReloadStatus ?? "<null>"}; " +
                $"old_token_sha256={result.ServiceTokenRotation?.OldTokenSha256 ?? "<null>"}; " +
                $"new_token_sha256={result.ServiceTokenRotation?.NewTokenSha256 ?? "<null>"}");
            Assert.Equal("service-token-rotation-revoke", result.Action);
            Assert.Equal(tokenPath, result.PreparedTokenPath);
            Assert.Equal("running", result.Service?.Status);
            Assert.True(result.ServiceOwnerVerified);
            Assert.Equal(["query", "stop", "start"], controller.Calls);
            Assert.Equal(Path.GetFullPath(tokenPath), Assert.Single(hardener.Paths));

            var replacementText = File.ReadAllText(tokenPath);
            var replacementToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = tokenPath
            });
            Assert.NotEqual(originalText, replacementText);
            Assert.NotEqual(originalToken.Value, replacementToken.Value);
            Assert.DoesNotContain(originalToken.Value!, replacementText, StringComparison.Ordinal);
            Assert.DoesNotContain(replacementToken.Value!, replacementText, StringComparison.Ordinal);

            var serialized = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            Assert.Contains("\"service_token_mutation\":\"performed\"", serialized);
            Assert.Contains("\"new_token_value_created\":true", serialized);
            Assert.Contains("\"token_value_observed\":false", serialized);
            Assert.Contains("\"old_token_rejection_status\":\"old-token-rejected-after-reload\"", serialized);
            Assert.Contains("\"token_rotation_audit_status\":\"written\"", serialized);
            Assert.DoesNotContain(originalToken.Value!, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(replacementToken.Value!, serialized, StringComparison.Ordinal);

            var backupRoot = Path.Combine(dataRoot, "backups", "service-token-rotation");
            Assert.True(Directory.Exists(backupRoot));
            Assert.NotEmpty(Directory.GetFiles(backupRoot, "*.dpapi.json"));
            var auditPath = Path.Combine(dataRoot, "service-token-rotation.audit.jsonl");
            Assert.True(File.Exists(auditPath));
            var auditText = File.ReadAllText(auditPath);
            Assert.Contains("service-token-rotation-revoke", auditText, StringComparison.Ordinal);
            Assert.Contains("token_rotation_audit_status", auditText, StringComparison.Ordinal);
            Assert.DoesNotContain(originalToken.Value!, auditText, StringComparison.Ordinal);
            Assert.DoesNotContain(replacementToken.Value!, auditText, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ServiceTokenRotationRejectsCredentialManagerTokenSourceBeforeMutation()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-token-source-mismatch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var tokenPath = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);
            var originalText = File.ReadAllText(tokenPath);
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen --api-token-credential-target \"PureCVisor/PureCVisorDesktopNode/api-token\"",
                    Win32ExitCode: 0)
            };

            hardener.Paths.Clear();
            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions.WithAction("service-token-rotation-revoke").WithDataRoot(dataRoot),
                controller,
                hardener);

            Assert.False(result.Ok);
            Assert.Equal("PCV_HOST_SERVICE_TOKEN_SOURCE_MISMATCH", result.ErrorCode);
            Assert.True(result.ServiceOwnerVerified);
            Assert.Equal(["query"], controller.Calls);
            Assert.Equal(originalText, File.ReadAllText(tokenPath));
            Assert.Empty(hardener.Paths);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void ProtectedTokenBootstrapDoesNotInvokeExternalAclExecutable()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "DesktopNode.Host",
            "DesktopNodeHostServiceAction.cs"));
        var source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("icacls.exe", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StatusUsesNativeServiceControllerWithoutExternalCommands()
    {
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "running",
                BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(NativeActionOptions, controller);

        Assert.True(result.Ok);
        Assert.Equal("status", result.Action);
        Assert.Empty(result.Plan.Commands);
        Assert.Empty(result.Commands);
        Assert.Equal("running", result.Service?.Status);
        Assert.True(result.ServiceOwnerVerified);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task ConfigureInstalledUsesNativeServiceControllerWithoutExternalCommands()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-configure-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060),
                ConfiguredSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0)
            };

            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions.WithAction("configure-installed").WithDataRoot(dataRoot),
                controller,
                hardener);

            Assert.True(result.Ok);
            Assert.Equal("running", result.Service?.Status);
            Assert.Empty(result.Commands);
            Assert.Equal(["query", "configure", "start"], controller.Calls);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains("--prefix \"http://127.0.0.1:7777/\"", binaryPath);
            Assert.Contains("--web-prefix \"http://127.0.0.1:80/\"", binaryPath);
            Assert.Contains("--web-root", binaryPath);
            Assert.Contains("--diagnostics-root", binaryPath);
            Assert.Contains($"\"{Path.Combine(dataRoot, "diagnostics")}\"", binaryPath);
            Assert.Contains("--api-token-protected-file", binaryPath);
            Assert.Contains($"\"{Path.Combine(dataRoot, "api-token.dpapi.json")}\"", binaryPath);
            Assert.Contains("--account-file", binaryPath);
            Assert.Contains($"\"{Path.Combine(dataRoot, "accounts.json")}\"", binaryPath);
            Assert.Contains("--jwt-signing-key-file", binaryPath);
            Assert.Contains($"\"{Path.Combine(dataRoot, "jwt-signing-key.txt")}\"", binaryPath);
            Assert.Contains("--route-timeout-seconds 30", binaryPath);
            Assert.Contains("--request-limit-per-minute 120", binaryPath);
            Assert.Contains("--request-burst-limit 20", binaryPath);
            Assert.Contains("--retry-after-seconds 15", binaryPath);
            Assert.Contains("--max-request-body-bytes", binaryPath);
            Assert.Contains("1048576", binaryPath);
            Assert.True(File.Exists(result.PreparedTokenPath));
            Assert.True(File.Exists(Path.Combine(dataRoot, "accounts.json")));
            Assert.True(File.Exists(Path.Combine(dataRoot, "jwt-signing-key.txt")));
            Assert.Equal(
                new[]
                {
                    Path.Combine(dataRoot, "api-token.dpapi.json"),
                    Path.Combine(dataRoot, "accounts.json"),
                    Path.Combine(dataRoot, "jwt-signing-key.txt")
                }.Select(Path.GetFullPath).Order(),
                hardener.Paths.Order());
            Assert.Contains(
                "\"storage\": \"dpapi-local-machine\"",
                File.ReadAllText(Path.Combine(dataRoot, "api-token.dpapi.json")),
                StringComparison.Ordinal);
            using var accounts = JsonDocument.Parse(File.ReadAllText(Path.Combine(dataRoot, "accounts.json")));
            Assert.Equal("no-default-account", accounts.RootElement.GetProperty("bootstrap_state").GetString());
            Assert.Equal(0, accounts.RootElement.GetProperty("accounts").GetArrayLength());
            Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(Path.Combine(dataRoot, "jwt-signing-key.txt"))));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ConfigureInstalledWritesBatchEvidenceRootToNativeServiceBinaryPath()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-configure-batch-" + Guid.NewGuid().ToString("N"));
        var batchEvidenceRoot = Path.Combine(dataRoot, "batch-evidence");
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060),
                ConfiguredSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0)
            };

            var options = new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.ServiceAction,
                ServiceAction = "configure-installed",
                ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
                ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                DataRoot = dataRoot,
                BatchEvidenceRootPath = batchEvidenceRoot
            };

            var result = await ExecuteWithHardenerAsync(options, controller, hardener);

            Assert.True(result.Ok);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains("--batch-evidence-root", binaryPath);
            Assert.Contains($"\"{batchEvidenceRoot}\"", binaryPath);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RepairInstalledPreservesCredentialManagerTokenSourceWhenCurrentServiceUsesCredentialTarget()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-repair-credential-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var currentBinaryPath =
                "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen " +
                "--api-token-credential-target \"PureCVisor/Custom/api-token\"";
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0)
            };

            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions.WithAction("repair-installed").WithDataRoot(dataRoot),
                controller,
                hardener);

            Assert.True(result.Ok);
            Assert.Equal(["query", "stop", "configure", "start"], controller.Calls);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains("--api-token-credential-target", binaryPath);
            Assert.Contains("\"PureCVisor/Custom/api-token\"", binaryPath);
            Assert.DoesNotContain("--api-token-protected-file", binaryPath);
            Assert.Contains("--max-request-body-bytes", binaryPath);
            Assert.Contains("1048576", binaryPath);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RepairInstalledPreservesBatchEvidenceRootFromCurrentServiceBinaryPath()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-repair-batch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var batchEvidenceRoot = "D:\\PureCVisorEvidence\\batch-runs";
            var currentBinaryPath =
                "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen " +
                $"--batch-evidence-root \"{batchEvidenceRoot}\"";
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0)
            };

            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions.WithAction("repair-installed").WithDataRoot(dataRoot),
                controller,
                hardener);

            Assert.True(result.Ok);
            Assert.Equal(["query", "stop", "configure", "start"], controller.Calls);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains("--batch-evidence-root", binaryPath);
            Assert.Contains($"\"{batchEvidenceRoot}\"", binaryPath);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RepairInstalledOverridesCurrentBatchEvidenceRootWhenOptionIsExplicit()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-repair-batch-override-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var previousBatchEvidenceRoot = "D:\\PureCVisorEvidence\\old-batches";
            var nextBatchEvidenceRoot = "D:\\PureCVisorEvidence\\next-batches";
            var currentBinaryPath =
                "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen " +
                $"--batch-evidence-root \"{previousBatchEvidenceRoot}\"";
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0)
            };
            var options = new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.ServiceAction,
                ServiceAction = "repair-installed",
                ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
                ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                DataRoot = dataRoot,
                BatchEvidenceRootPath = nextBatchEvidenceRoot
            };

            var result = await ExecuteWithHardenerAsync(options, controller, hardener);

            Assert.True(result.Ok);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains($"\"{nextBatchEvidenceRoot}\"", binaryPath);
            Assert.DoesNotContain(previousBatchEvidenceRoot, binaryPath);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task StartUsesNativeServiceControllerAfterOwnershipCheck()
    {
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0),
            MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "running",
                BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(NativeActionOptions.WithAction("start"), controller);

        Assert.True(result.Ok);
        Assert.Equal("running", result.Service?.Status);
        Assert.True(result.ServiceOwnerVerified);
        Assert.Equal(["query", "start"], controller.Calls);
    }

    [Fact]
    public async Task StopUsesNativeServiceControllerAfterOwnershipCheck()
    {
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "running",
                BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0),
            MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(NativeActionOptions.WithAction("stop"), controller);

        Assert.True(result.Ok);
        Assert.Equal("stopped", result.Service?.Status);
        Assert.True(result.ServiceOwnerVerified);
        Assert.Equal(["query", "stop"], controller.Calls);
    }

    [Fact]
    public async Task StartRejectsForeignServiceBeforeMutation()
    {
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: true,
                Status: "stopped",
                BinaryPathName: "\"C:\\Other\\DesktopNode.Host.exe\" listen",
                Win32ExitCode: 0)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(NativeActionOptions.WithAction("start"), controller);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_SERVICE_OWNERSHIP_MISMATCH", result.ErrorCode);
        Assert.False(result.ServiceOwnerVerified);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task RemoveInstalledRemoveDataReturnsHandoffWithoutDirectDataMutation()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-remove-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        var tokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        File.WriteAllText(tokenPath, "delete-me");
        try
        {
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                DeletedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060)
            };

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions.WithAction("remove-installed").WithDataRoot(dataRoot, removeData: true),
                controller);

            Assert.True(result.Ok);
            Assert.False(result.Service?.Exists);
            Assert.Equal(["query", "stop", "delete"], controller.Calls);
            Assert.Empty(result.RemovedPaths);
            Assert.NotNull(result.RemoveDataHandoff);
            Assert.Equal("data-root-remove", result.RemoveDataHandoff.Operation);
            Assert.Equal(dataRoot, result.RemoveDataHandoff.DataRoot);
            Assert.Contains(tokenPath, result.RemoveDataHandoff.Paths);
            Assert.True(File.Exists(tokenPath));
            Assert.True(Directory.Exists(dataRoot));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RemoveInstalledPreserveDataBlocksUnresolvedPendingCommitAfterStop()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-remove-pending-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        var pendingPath = Path.Combine(dataRoot, "jobs.json.commit-pending");
        File.WriteAllText(pendingPath, "pending-guard");
        try
        {
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                MutatedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0),
                DeletedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060)
            };

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions.WithAction("remove-installed").WithDataRoot(dataRoot),
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_JOB_STORE_PENDING_COMMIT_UNRESOLVED", result.ErrorCode);
            Assert.Equal(["query", "stop"], controller.Calls);
            Assert.Equal("stopped", result.Service?.Status);
            Assert.True(File.Exists(pendingPath));
        }
        finally
        {
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task StopRejectsMissingServiceBeforeMutation()
    {
        var controller = new FakeWindowsServiceController
        {
            Snapshot = new DesktopNodeWindowsServiceSnapshot(
                ServiceName: "PureCVisorDesktopNode",
                Exists: false,
                Status: "missing",
                BinaryPathName: null,
                Win32ExitCode: 1060)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(NativeActionOptions.WithAction("stop"), controller);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_SERVICE_NOT_FOUND", result.ErrorCode);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task DataRootRemoveRequiresExplicitRemoveData()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-data-root-remove-required-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        var tokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        await File.WriteAllTextAsync(tokenPath, "keep-me");
        try
        {
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060)
            };

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions.WithAction("data-root-remove").WithDataRoot(dataRoot),
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_HOST_REMOVE_DATA_REQUIRED", result.ErrorCode);
            Assert.Equal(["query"], controller.Calls);
            Assert.True(File.Exists(tokenPath));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DataRootRemoveBlocksWhenServiceStillExists()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-data-root-service-exists-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        var tokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        await File.WriteAllTextAsync(tokenPath, "keep-me");
        try
        {
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen",
                    Win32ExitCode: 0)
            };

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions.WithAction("data-root-remove").WithDataRoot(dataRoot, removeData: true),
                controller);

            Assert.False(result.Ok);
            Assert.Equal("PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS", result.ErrorCode);
            Assert.Equal(["query"], controller.Calls);
            Assert.True(File.Exists(tokenPath));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DataRootRemoveDeletesOnlyAllowlistedPathsAfterServiceAbsent()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-data-root-remove-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        var legacyTokenPath = Path.Combine(dataRoot, "api-token.txt");
        var protectedTokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        var jobStorePath = Path.Combine(dataRoot, "jobs.json");
        var legacyJobTempPath = Path.Combine(dataRoot, "jobs.json.tmp");
        var pendingCommitPath = Path.Combine(dataRoot, "jobs.json.commit-pending");
        var jobTempPath = Path.Combine(dataRoot, "jobs.json.tmp." + Guid.NewGuid().ToString("N"));
        var pendingTempPath = Path.Combine(dataRoot, "jobs.json.commit-pending.tmp." + Guid.NewGuid().ToString("N"));
        var unrelatedTempPath = Path.Combine(dataRoot, "jobs.json.tmp.not-owned");
        var eventLogPath = Path.Combine(dataRoot, "events.jsonl");
        var installLogPath = Path.Combine(dataRoot, "install.jsonl");
        var diagnosticsPath = Path.Combine(dataRoot, "diagnostics");
        var unrelatedPath = Path.Combine(dataRoot, "service-host.log");
        Directory.CreateDirectory(diagnosticsPath);
        Directory.CreateDirectory(legacyJobTempPath);
        await File.WriteAllTextAsync(legacyTokenPath, "delete-me");
        await File.WriteAllTextAsync(protectedTokenPath, "delete-me");
        await File.WriteAllTextAsync(jobStorePath, "delete-me");
        await File.WriteAllTextAsync(Path.Combine(legacyJobTempPath, "stale.json"), "delete-me");
        await File.WriteAllTextAsync(pendingCommitPath, "delete-me");
        await File.WriteAllTextAsync(jobTempPath, "delete-me");
        await File.WriteAllTextAsync(pendingTempPath, "delete-me");
        await File.WriteAllTextAsync(unrelatedTempPath, "keep-me");
        await File.WriteAllTextAsync(eventLogPath, "delete-me");
        await File.WriteAllTextAsync(installLogPath, "delete-me");
        await File.WriteAllTextAsync(Path.Combine(diagnosticsPath, "bundle.json"), "delete-me");
        await File.WriteAllTextAsync(unrelatedPath, "keep-me");
        try
        {
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: false,
                    Status: "missing",
                    BinaryPathName: null,
                    Win32ExitCode: 1060)
            };

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions.WithAction("data-root-remove").WithDataRoot(dataRoot, removeData: true),
                controller);

            Assert.True(result.Ok);
            Assert.Equal(["query"], controller.Calls);
            Assert.False(result.Service?.Exists);
            Assert.Null(result.RemoveDataHandoff);
            Assert.Contains(legacyTokenPath, result.RemovedPaths);
            Assert.Contains(protectedTokenPath, result.RemovedPaths);
            Assert.Contains(jobStorePath, result.RemovedPaths);
            Assert.Contains(legacyJobTempPath, result.RemovedPaths);
            Assert.Contains(pendingCommitPath, result.RemovedPaths);
            Assert.Contains(jobTempPath, result.RemovedPaths);
            Assert.Contains(pendingTempPath, result.RemovedPaths);
            Assert.Contains(eventLogPath, result.RemovedPaths);
            Assert.Contains(installLogPath, result.RemovedPaths);
            Assert.Contains(diagnosticsPath, result.RemovedPaths);
            Assert.False(File.Exists(legacyTokenPath));
            Assert.False(File.Exists(protectedTokenPath));
            Assert.False(File.Exists(jobStorePath));
            Assert.False(Directory.Exists(legacyJobTempPath));
            Assert.False(File.Exists(pendingCommitPath));
            Assert.False(File.Exists(jobTempPath));
            Assert.False(File.Exists(pendingTempPath));
            Assert.False(File.Exists(eventLogPath));
            Assert.False(File.Exists(installLogPath));
            Assert.False(Directory.Exists(diagnosticsPath));
            Assert.True(File.Exists(unrelatedPath));
            Assert.True(File.Exists(unrelatedTempPath));
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task EventLogRegisterUsesNativeRegistryControllerWithoutExternalCommands()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: false,
                EventMessageFile: null,
                Owned: false),
            MutatedSnapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-register"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-register", result.Action);
        Assert.Empty(result.Commands);
        Assert.True(result.EventLog?.Exists);
        Assert.True(result.EventLog?.Owned);
        Assert.Equal(["query", "register"], controller.Calls);
    }

    [Fact]
    public async Task EventLogRegisterRejectsForeignExistingSourceBeforeMutation()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Other\\Foreign.exe",
                Owned: false)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-register"),
            serviceController: null,
            eventLogController: controller);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_EVENTLOG_SOURCE_OWNERSHIP_MISMATCH", result.ErrorCode);
        Assert.False(result.EventLog?.Owned);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task EventLogRemoveDeletesOwnedSourceWithoutExternalCommands()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true),
            MutatedSnapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: false,
                EventMessageFile: null,
                Owned: false)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-remove"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.False(result.EventLog?.Exists);
        Assert.Equal(["query", "remove"], controller.Calls);
    }

    [Fact]
    public async Task EventLogRemoveTreatsMissingSourceAsIdempotentSuccess()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: false,
                EventMessageFile: null,
                Owned: false)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-remove"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.False(result.EventLog?.Exists);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task EventLogRepairRewritesOwnedProviderAndRecordsHardeningDescriptor()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true),
            MutatedSnapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-repair"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-repair", result.Action);
        Assert.Empty(result.Commands);
        Assert.Equal("provider-repair-pass", result.EventLogHardening?.ProviderRepairStatus);
        Assert.Equal("not-run", result.EventLogHardening?.EventWriteStatus);
        Assert.Equal("not-run", result.EventLogHardening?.VolumeGuardStatus);
        Assert.Equal(["query", "register"], controller.Calls);
    }

    [Fact]
    public async Task EventLogWriteTestRequiresOwnedProviderAndRecordsEventId()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true),
            WriteStatus = "write-query-pass"
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-write-test"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-write-test", result.Action);
        Assert.Equal("not-run", result.EventLogHardening?.ProviderRepairStatus);
        Assert.Equal("write-query-pass", result.EventLogHardening?.EventWriteStatus);
        Assert.Equal(39100, result.EventLogHardening?.EventId);
        Assert.Equal(["query", "write-test"], controller.Calls);
    }

    [Fact]
    public async Task EventLogVolumeGuardQueriesBoundedRetentionPolicy()
    {
        var controller = new FakeWindowsEventLogController
        {
            Snapshot = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true),
            VolumePolicy = new DesktopNodeWindowsEventLogVolumePolicySnapshot(
                LogName: "Application",
                MaximumSizeBytes: 20 * 1024 * 1024,
                RetentionPolicy: "overwrite-as-needed",
                VolumeGuarded: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-volume-guard"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-volume-guard", result.Action);
        Assert.Equal("volume-guard-pass", result.EventLogHardening?.VolumeGuardStatus);
        Assert.True(result.EventLogHardening?.VolumePolicy?.VolumeGuarded);
        Assert.Equal(["query", "volume-guard"], controller.Calls);
    }

    [Fact]
    public async Task EventLogDefaultTransitionRepairsRemovesRestoresWritesSchemaAndChecksVolumeGuard()
    {
        var providerPresent = new DesktopNodeWindowsEventLogSnapshot(
            LogName: "Application",
            SourceName: "PureCVisor Desktop Node",
            Exists: true,
            EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            Owned: true);
        var providerAbsent = new DesktopNodeWindowsEventLogSnapshot(
            LogName: "Application",
            SourceName: "PureCVisor Desktop Node",
            Exists: false,
            EventMessageFile: null,
            Owned: false);
        var controller = new ScriptedWindowsEventLogController(
            querySnapshot: providerPresent,
            registerSnapshots: [providerPresent, providerPresent],
            removeSnapshots: [providerAbsent],
            volumePolicy: new DesktopNodeWindowsEventLogVolumePolicySnapshot(
                LogName: "Application",
                MaximumSizeBytes: 20 * 1024 * 1024,
                RetentionPolicy: "overwrite-as-needed",
                VolumeGuarded: true));

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("eventlog-default-transition"),
            serviceController: null,
            eventLogController: controller);

        Assert.True(result.Ok);
        Assert.Equal("eventlog-default-transition", result.Action);
        Assert.Empty(result.Commands);
        Assert.True(result.EventLog?.Exists);
        Assert.True(result.EventLog?.Owned);
        Assert.Equal("windows-event-log-default-transition", result.EventLogHardening?.Operation);
        Assert.Equal("provider-repair-pass", result.EventLogHardening?.ProviderRepairStatus);
        Assert.Equal("write-query-pass", result.EventLogHardening?.EventWriteStatus);
        Assert.Equal("volume-guard-pass", result.EventLogHardening?.VolumeGuardStatus);
        Assert.Equal(39101, result.EventLogHardening?.EventId);
        Assert.True(result.EventLogHardening?.HostMutationPerformed);
        Assert.Equal("default-writer-pass", DescriptorValue(result.EventLogHardening!, "DefaultWriterStatus"));
        Assert.Equal("provider-remove-pass", DescriptorValue(result.EventLogHardening!, "ProviderRemoveStatus"));
        Assert.Equal("provider-present", DescriptorValue(result.EventLogHardening!, "FinalProviderStatus"));
        Assert.Equal(1, DescriptorValue(result.EventLogHardening!, "SchemaVersion"));
        Assert.Equal(60, DescriptorValue(result.EventLogHardening!, "TimeoutSeconds"));
        Assert.Equal("completed-within-timeout", DescriptorValue(result.EventLogHardening!, "TimeoutGuardStatus"));
        Assert.Equal(["query", "register", "write-test", "volume-guard", "remove", "register"], controller.Calls);
        Assert.Single(controller.Messages);
        Assert.Contains("\"schema_version\":1", controller.Messages[0]);
        Assert.Contains("windows-event-log-default-writer", controller.Messages[0]);
    }

    [Fact]
    public async Task EventLogDefaultTransitionWritesRedactedDescriptorWhenDataRootIsProvided()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-eventlog-transition-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var providerPresent = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: true,
                EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
                Owned: true);
            var providerAbsent = new DesktopNodeWindowsEventLogSnapshot(
                LogName: "Application",
                SourceName: "PureCVisor Desktop Node",
                Exists: false,
                EventMessageFile: null,
                Owned: false);
            var controller = new ScriptedWindowsEventLogController(
                querySnapshot: providerPresent,
                registerSnapshots: [providerPresent, providerPresent],
                removeSnapshots: [providerAbsent],
                volumePolicy: new DesktopNodeWindowsEventLogVolumePolicySnapshot(
                    LogName: "Application",
                    MaximumSizeBytes: 20 * 1024 * 1024,
                    RetentionPolicy: "overwrite-as-needed",
                    VolumeGuarded: true));

            var result = await DesktopNodeHostServiceAction.ExecuteAsync(
                NativeActionOptions
                    .WithAction("eventlog-default-transition")
                    .WithDataRoot(dataRoot),
                serviceController: null,
                eventLogController: controller);

            Assert.True(result.Ok);
            var evidencePath = Path.Combine(dataRoot, "eventlog-default-transition.json");
            Assert.True(File.Exists(evidencePath));
            using var document = JsonDocument.Parse(File.ReadAllText(evidencePath));
            var root = document.RootElement;
            Assert.True(root.GetProperty("ok").GetBoolean());
            Assert.Equal("windows-event-log-default-transition", root.GetProperty("operation").GetString());
            Assert.Equal("default-writer-pass", root.GetProperty("default_writer_status").GetString());
            Assert.Equal("provider-remove-pass", root.GetProperty("provider_remove_status").GetString());
            Assert.Equal("provider-present", root.GetProperty("final_provider_status").GetString());
            Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
            Assert.Equal(60, root.GetProperty("timeout_seconds").GetInt32());
            Assert.Equal("completed-within-timeout", root.GetProperty("timeout_guard_status").GetString());
            Assert.False(root.GetRawText().Contains("Bearer ", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(dataRoot, recursive: true);
        }
    }

    [Fact]
    public async Task EventLogDefaultTransitionFailsFastWhenTimeoutExpires()
    {
        var providerPresent = new DesktopNodeWindowsEventLogSnapshot(
            LogName: "Application",
            SourceName: "PureCVisor Desktop Node",
            Exists: true,
            EventMessageFile: "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            Owned: true);
        var controller = new BlockingEventLogWriteController(providerPresent);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions
                .WithAction("eventlog-default-transition")
                .WithEventLogDefaultTransitionTimeoutSeconds(1),
            serviceController: null,
            eventLogController: controller);
        stopwatch.Stop();

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_EVENTLOG_DEFAULT_TRANSITION_TIMEOUT", result.ErrorCode);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3));
        Assert.Equal("timed-out", DescriptorValue(result.EventLogHardening!, "TimeoutGuardStatus"));
        Assert.Equal(1, DescriptorValue(result.EventLogHardening!, "TimeoutSeconds"));
        Assert.Equal("provider-repair-timeout", result.EventLogHardening?.ProviderRepairStatus);
        Assert.Equal("write-timeout", result.EventLogHardening?.EventWriteStatus);
        Assert.Equal("not-run", result.EventLogHardening?.VolumeGuardStatus);
        Assert.Equal("not-run", DescriptorValue(result.EventLogHardening!, "ProviderRemoveStatus"));
        Assert.Equal("unknown-after-timeout", DescriptorValue(result.EventLogHardening!, "FinalProviderStatus"));
        Assert.Equal(["query", "register", "write-test"], controller.Calls);
    }

    [Fact]
    public async Task CredentialManagerSystemProofRecordsSystemIdentityWithoutTokenValue()
    {
        var controller = new FakeWindowsCredentialManagerController
        {
            Proof = new DesktopNodeWindowsCredentialManagerProofSnapshot(
                Identity: "NT AUTHORITY\\SYSTEM",
                CredentialTarget: "PureCVisor/PureCVisorDesktopNode/api-token",
                CredentialWriteStatus: "pass",
                CredentialReadStatus: "pass",
                CredentialDeleteStatus: "pass",
                TokenValueObserved: false,
                NewTokenValueCreated: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("credential-manager-system-proof"),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: null,
            credentialManagerController: controller);

        Assert.True(result.Ok);
        Assert.Equal("credential-manager-system-proof", result.Action);
        Assert.NotNull(result.CredentialManagerProof);
        Assert.Equal("system-context-proof-pass", result.CredentialManagerProof.ProofStatus);
        Assert.Equal("NT AUTHORITY\\SYSTEM", result.CredentialManagerProof.Identity);
        Assert.Equal("pass", result.CredentialManagerProof.CredentialWriteStatus);
        Assert.Equal("pass", result.CredentialManagerProof.CredentialReadStatus);
        Assert.Equal("pass", result.CredentialManagerProof.CredentialDeleteStatus);
        Assert.False(result.CredentialManagerProof.TokenValueObserved);
        Assert.Equal(["write-read-delete-proof"], controller.Calls);
    }

    [Fact]
    public async Task CredentialManagerDefaultTransitionMigratesProtectedTokenReconfiguresServiceReloadsAndWritesRollbackDiagnostics()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-credential-transition-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var protectedTokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
            var batchEvidenceRoot = "D:\\PureCVisorEvidence\\batch-runs";
            var previousBinaryPath = "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen " +
                "--api-token-protected-file \"" + protectedTokenPath + "\" " +
                "--batch-evidence-root \"" + batchEvidenceRoot + "\"";
            var serviceController = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: previousBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: previousBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen --api-token-credential-target \"PureCVisor/PureCVisorDesktopNode/api-token\"",
                    Win32ExitCode: 0)
            };
            var credentialController = new FakeWindowsCredentialManagerController
            {
                Proof = new DesktopNodeWindowsCredentialManagerProofSnapshot(
                    Identity: "NT AUTHORITY\\SYSTEM",
                    CredentialTarget: "PureCVisor/PureCVisorDesktopNode/api-token/system-proof",
                    CredentialWriteStatus: "pass",
                    CredentialReadStatus: "pass",
                    CredentialDeleteStatus: "pass",
                    TokenValueObserved: false,
                    NewTokenValueCreated: true)
            };

            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions
                    .WithAction("credential-manager-default-transition")
                    .WithDataRoot(dataRoot),
                serviceController,
                hardener,
                credentialController);

            var protectedToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = protectedTokenPath
            });

            Assert.True(result.Ok);
            Assert.Equal(Path.GetFullPath(protectedTokenPath), Assert.Single(hardener.Paths));
            Assert.Equal("credential-manager-default-transition", result.Action);
            Assert.Equal(["query", "stop", "configure", "start"], serviceController.Calls);
            Assert.Equal(["write-read-delete-proof", "write-token", "read-token"], credentialController.Calls);
            Assert.Single(serviceController.Configurations);
            var binaryPath = serviceController.Configurations[0].BinaryPathName;
            Assert.Contains("--api-token-credential-target", binaryPath);
            Assert.Contains("\"PureCVisor/PureCVisorDesktopNode/api-token\"", binaryPath);
            Assert.DoesNotContain("--api-token-protected-file", binaryPath);
            Assert.Contains("--batch-evidence-root", binaryPath);
            Assert.Contains($"\"{batchEvidenceRoot}\"", binaryPath);
            Assert.Equal(protectedToken.Value, credentialController.WrittenTokens["PureCVisor/PureCVisorDesktopNode/api-token"]);
            Assert.Equal("protected-file-to-credential-manager", result.CredentialManagerTransition?.TokenSourceMigration);
            Assert.Equal("restarted", result.CredentialManagerTransition?.ServiceReloadStatus);
            Assert.Equal("protected-file-source-rejected-after-reload", result.CredentialManagerTransition?.OldSourceRejectionStatus);
            Assert.Equal("written", result.CredentialManagerTransition?.RollbackDiagnosticsStatus);
            Assert.False(result.CredentialManagerTransition?.TokenValueObserved);
            Assert.True(File.Exists(result.CredentialManagerTransition?.RollbackDiagnosticsPath));
            var diagnosticsText = File.ReadAllText(result.CredentialManagerTransition!.RollbackDiagnosticsPath);
            Assert.Contains("previous_binary_path", diagnosticsText, StringComparison.Ordinal);
            Assert.DoesNotContain(protectedToken.Value!, diagnosticsText, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CredentialManagerDefaultTransitionTreatsExistingCredentialManagerSourceAsIdempotent()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-credential-existing-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            var protectedTokenPath = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);
            var protectedToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = protectedTokenPath
            });
            const string credentialTarget = "PureCVisor/PureCVisorDesktopNode/api-token";
            var currentBinaryPath = "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen --api-token-credential-target \"" + credentialTarget + "\"";
            var serviceController = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0)
            };
            var credentialController = new FakeWindowsCredentialManagerController();
            credentialController.WrittenTokens[credentialTarget] = protectedToken.Value!;

            hardener.Paths.Clear();
            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions
                    .WithAction("credential-manager-default-transition")
                    .WithDataRoot(dataRoot),
                serviceController,
                hardener,
                credentialController);

            Assert.True(result.Ok);
            Assert.True(result.ServiceOwnerVerified);
            Assert.Equal(["query"], serviceController.Calls);
            Assert.Equal(["read-token"], credentialController.Calls);
            Assert.Equal("already-credential-manager", result.CredentialManagerTransition?.TokenSourceMigration);
            Assert.Equal("not-required", result.CredentialManagerTransition?.ServiceReloadStatus);
            Assert.Equal("already-credential-manager", result.CredentialManagerTransition?.OldSourceRejectionStatus);
            Assert.Equal("written", result.CredentialManagerTransition?.RollbackDiagnosticsStatus);
            Assert.False(result.CredentialManagerTransition?.HostMutationPerformed);
            Assert.Equal(currentBinaryPath, result.CredentialManagerTransition?.PreviousBinaryPath);
            Assert.Equal(currentBinaryPath, result.CredentialManagerTransition?.NextBinaryPath);
            Assert.Empty(hardener.Paths);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CredentialManagerDefaultTransitionRejectsCredentialManagerTokenSourceBeforeMutation()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-credential-source-mismatch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var hardener = new RecordingFileAclHardener();
            _ = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);
            var serviceController = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen --api-token-credential-target \"PureCVisor/PureCVisorDesktopNode/api-token\"",
                    Win32ExitCode: 0)
            };
            var credentialController = new FakeWindowsCredentialManagerController();

            hardener.Paths.Clear();
            var result = await ExecuteWithHardenerAsync(
                NativeActionOptions
                    .WithAction("credential-manager-default-transition")
                    .WithDataRoot(dataRoot),
                serviceController,
                hardener,
                credentialController);

            Assert.False(result.Ok);
            Assert.Equal("PCV_HOST_CREDENTIAL_MANAGER_TOKEN_SOURCE_MISMATCH", result.ErrorCode);
            Assert.True(result.ServiceOwnerVerified);
            Assert.Equal(["query"], serviceController.Calls);
            Assert.Equal(["read-token"], credentialController.Calls);
            Assert.Empty(hardener.Paths);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task FirewallEnableRequiresLanApprovalBeforeMutation()
    {
        var controller = new FakeWindowsFirewallController();

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("firewall-enable"),
            serviceController: null,
            eventLogController: null,
            firewallController: controller,
            trustStoreController: null);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_FIREWALL_LAN_APPROVAL_REQUIRED", result.ErrorCode);
        Assert.Empty(result.Commands);
        Assert.Equal([], controller.Calls);
    }

    [Fact]
    public async Task FirewallEnableCreatesOwnedAllowRuleWithoutExternalCommands()
    {
        var controller = new FakeWindowsFirewallController
        {
            Snapshot = DesktopNodeWindowsFirewallRuleSnapshot.Missing(
                "PureCVisor Desktop Node Local API LAN",
                "inbound",
                "TCP",
                7777,
                "Private",
                "LocalSubnet"),
            MutatedSnapshot = new DesktopNodeWindowsFirewallRuleSnapshot(
                RuleName: "PureCVisor Desktop Node Local API LAN",
                Exists: true,
                Enabled: true,
                Direction: "inbound",
                Protocol: "TCP",
                LocalPort: 7777,
                Profile: "Private",
                RemoteAddress: "LocalSubnet",
                Description: "PureCVisor Desktop Node managed firewall rule",
                Owned: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("firewall-enable").WithAllowLan(),
            serviceController: null,
            eventLogController: null,
            firewallController: controller,
            trustStoreController: null);

        Assert.True(result.Ok);
        Assert.Equal("firewall-enable", result.Action);
        Assert.Empty(result.Commands);
        Assert.True(result.FirewallRule?.Exists);
        Assert.True(result.FirewallRule?.Enabled);
        Assert.True(result.FirewallRule?.Owned);
        Assert.Equal(["query", "enable"], controller.Calls);
    }

    [Fact]
    public async Task FirewallEnableRejectsForeignExistingRuleBeforeMutation()
    {
        var controller = new FakeWindowsFirewallController
        {
            Snapshot = new DesktopNodeWindowsFirewallRuleSnapshot(
                RuleName: "PureCVisor Desktop Node Local API LAN",
                Exists: true,
                Enabled: true,
                Direction: "inbound",
                Protocol: "TCP",
                LocalPort: 7777,
                Profile: "Private",
                RemoteAddress: "*",
                Description: "foreign rule",
                Owned: false)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("firewall-enable").WithAllowLan(),
            serviceController: null,
            eventLogController: null,
            firewallController: controller,
            trustStoreController: null);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_FIREWALL_RULE_OWNERSHIP_MISMATCH", result.ErrorCode);
        Assert.False(result.FirewallRule?.Owned);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task FirewallRemoveDeletesOwnedRuleWithoutExternalCommands()
    {
        var controller = new FakeWindowsFirewallController
        {
            Snapshot = new DesktopNodeWindowsFirewallRuleSnapshot(
                RuleName: "PureCVisor Desktop Node Local API LAN",
                Exists: true,
                Enabled: true,
                Direction: "inbound",
                Protocol: "TCP",
                LocalPort: 7777,
                Profile: "Private",
                RemoteAddress: "LocalSubnet",
                Description: "PureCVisor Desktop Node managed firewall rule",
                Owned: true),
            MutatedSnapshot = DesktopNodeWindowsFirewallRuleSnapshot.Missing(
                "PureCVisor Desktop Node Local API LAN",
                "inbound",
                "TCP",
                7777,
                "Private",
                "LocalSubnet")
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("firewall-remove"),
            serviceController: null,
            eventLogController: null,
            firewallController: controller,
            trustStoreController: null);

        Assert.True(result.Ok);
        Assert.Equal("firewall-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.False(result.FirewallRule?.Exists);
        Assert.Equal(["query", "remove"], controller.Calls);
    }

    [Fact]
    public async Task FirewallRemoveTreatsMissingRuleAsIdempotentSuccess()
    {
        var controller = new FakeWindowsFirewallController
        {
            Snapshot = DesktopNodeWindowsFirewallRuleSnapshot.Missing(
                "PureCVisor Desktop Node Local API LAN",
                "inbound",
                "TCP",
                7777,
                "Private",
                "LocalSubnet")
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("firewall-remove"),
            serviceController: null,
            eventLogController: null,
            firewallController: controller,
            trustStoreController: null);

        Assert.True(result.Ok);
        Assert.Equal("firewall-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.False(result.FirewallRule?.Exists);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task TrustStoreInstallRequiresReleaseApprovalBeforeMutation()
    {
        var controller = new FakeWindowsTrustStoreController();

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("trust-store-install").WithTrustStoreCertificateInputs(),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: controller);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_TRUST_STORE_RELEASE_APPROVAL_REQUIRED", result.ErrorCode);
        Assert.Empty(result.Commands);
        Assert.Equal([], controller.Calls);
    }

    [Fact]
    public async Task TrustStoreInstallImportsApprovedCertificatesWithoutExternalCommands()
    {
        var controller = new FakeWindowsTrustStoreController
        {
            Snapshots = MissingTrustStoreSnapshots(),
            MutatedSnapshots = OwnedTrustStoreSnapshots(exists: true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions
                .WithAction("trust-store-install")
                .WithReleaseApproved()
                .WithTrustStoreCertificateInputs(),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: controller);

        Assert.True(result.Ok);
        Assert.Equal("trust-store-install", result.Action);
        Assert.Empty(result.Commands);
        Assert.Equal(2, result.TrustStoreCertificates?.Count);
        Assert.All(result.TrustStoreCertificates!, certificate =>
        {
            Assert.True(certificate.Exists);
            Assert.True(certificate.Owned);
        });
        Assert.Equal(["query", "install"], controller.Calls);
    }

    [Fact]
    public async Task TrustStoreInstallRejectsForeignCertificateBeforeMutation()
    {
        var controller = new FakeWindowsTrustStoreController
        {
            Snapshots =
            [
                new DesktopNodeWindowsTrustStoreCertificateSnapshot(
                    StoreName: "Root",
                    StoreLocation: "LocalMachine",
                    Thumbprint: "00112233445566778899AABBCCDDEEFF00112233",
                    Exists: true,
                    Subject: "CN=Foreign Root",
                    Issuer: "CN=Foreign Root",
                    SerialNumber: "10",
                    Owned: false),
                MissingTrustStoreSnapshots()[1]
            ]
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions
                .WithAction("trust-store-install")
                .WithReleaseApproved()
                .WithTrustStoreCertificateInputs(),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: controller);

        Assert.False(result.Ok);
        Assert.Equal("PCV_HOST_TRUST_STORE_CERTIFICATE_OWNERSHIP_MISMATCH", result.ErrorCode);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task TrustStoreRemoveDeletesOwnedCertificatesWithoutExternalCommands()
    {
        var controller = new FakeWindowsTrustStoreController
        {
            Snapshots = OwnedTrustStoreSnapshots(exists: true),
            MutatedSnapshots = OwnedTrustStoreSnapshots(exists: false)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions
                .WithAction("trust-store-remove")
                .WithReleaseApproved()
                .WithTrustStoreCertificateInputs(certificatePaths: false),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: controller);

        Assert.True(result.Ok);
        Assert.Equal("trust-store-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.All(result.TrustStoreCertificates!, certificate => Assert.False(certificate.Exists));
        Assert.Equal(["query", "remove"], controller.Calls);
    }

    [Fact]
    public async Task TrustStoreRemoveTreatsMissingCertificatesAsIdempotentSuccess()
    {
        var controller = new FakeWindowsTrustStoreController
        {
            Snapshots = MissingTrustStoreSnapshots()
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions
                .WithAction("trust-store-remove")
                .WithReleaseApproved()
                .WithTrustStoreCertificateInputs(certificatePaths: false),
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: controller);

        Assert.True(result.Ok);
        Assert.Equal("trust-store-remove", result.Action);
        Assert.Empty(result.Commands);
        Assert.All(result.TrustStoreCertificates!, certificate => Assert.False(certificate.Exists));
        Assert.Equal(["query"], controller.Calls);
    }

    private static IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> MissingTrustStoreSnapshots()
    {
        return OwnedTrustStoreSnapshots(exists: false);
    }

    private static void WriteProductManifest(string productRoot, string version)
    {
        File.WriteAllText(
            Path.Combine(productRoot, "product-manifest.json"),
            $$"""
            {
              "schema_version": 1,
              "product": "PureCVisor Desktop Node",
              "version": "{{version}}"
            }
            """,
            Encoding.UTF8);
    }

    private static Task<DesktopNodeHostServiceActionResult> ExecuteWithHardenerAsync(
        DesktopNodeHostOptions options,
        IDesktopNodeWindowsServiceController? serviceController,
        IDesktopNodeHostFileAclHardener fileAclHardener,
        IDesktopNodeWindowsCredentialManagerController? credentialManagerController = null)
    {
        return DesktopNodeHostServiceAction.ExecuteAsync(
            options,
            serviceController,
            eventLogController: null,
            firewallController: null,
            trustStoreController: null,
            credentialManagerController: credentialManagerController,
            fileAclHardener: fileAclHardener,
            cancellationToken: CancellationToken.None);
    }

}

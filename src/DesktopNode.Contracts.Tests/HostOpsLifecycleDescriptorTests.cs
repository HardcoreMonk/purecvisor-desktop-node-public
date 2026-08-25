using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class HostOpsLifecycleDescriptorTests
{
    [Fact]
    public void HostOpsLifecycleDescriptorPublishesCurrentEvidenceBridgeBuckets()
    {
        var descriptor = HostOpsLifecycleDescriptor.CreateDefault();

        Assert.Equal(1, descriptor.SchemaVersion);
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

        var serviceAction = descriptor.Buckets.Single(bucket => bucket.BucketKey == "service-action");
        Assert.Equal("service-lifecycle", serviceAction.OperationFamily);
        Assert.Equal("windows-service-control-manager", serviceAction.MutationBoundary);
        Assert.Contains("configure-installed", serviceAction.Operations);
        Assert.Contains("repair-installed", serviceAction.Operations);
        Assert.Contains("remove-installed", serviceAction.Operations);

        var credentialManager = descriptor.Buckets.Single(bucket => bucket.BucketKey == "credential-manager");
        Assert.Equal("credential-manager", credentialManager.OperationFamily);
        Assert.Equal("windows-credential-manager", credentialManager.MutationBoundary);
        Assert.Contains("credential-manager-system-proof", credentialManager.Operations);
        Assert.Contains("credential-manager-default-transition", credentialManager.Operations);
    }
}

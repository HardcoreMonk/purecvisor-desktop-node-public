namespace DesktopNode.Contracts;

public sealed record HostOpsLifecycleDescriptor(
    int SchemaVersion,
    string ContractKey,
    string LifecycleBucketContractKey,
    IReadOnlyList<HostOpsLifecycleBucketDescriptor> Buckets)
{
    public const int CurrentSchemaVersion = 1;
    public const string CurrentContractKey = "host-ops-lifecycle-descriptor-bridge-v1";
    public const string CurrentLifecycleBucketContractKey =
        "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated";
    public const string SourceName = "DesktopNode.Contracts.HostOpsLifecycleDescriptor";

    public static HostOpsLifecycleDescriptor CreateDefault()
    {
        return Create(
        [
            new(
                "service-action",
                "service-lifecycle",
                "DesktopNodeServiceLifecycleOps",
                "windows-service-control-manager",
                [
                    "status",
                    "start",
                    "stop",
                    "configure-installed",
                    "repair-installed",
                    "remove-installed"
                ]),
            new(
                "event-log",
                "event-log",
                "DesktopNodeEventLogOps",
                "windows-event-log-provider",
                [
                    "eventlog-register",
                    "eventlog-remove",
                    "eventlog-repair",
                    "eventlog-write-test",
                    "eventlog-volume-guard",
                    "eventlog-default-transition"
                ]),
            new(
                "firewall",
                "firewall",
                "DesktopNodeFirewallOps",
                "windows-firewall-rule",
                [
                    "firewall-enable",
                    "firewall-remove"
                ]),
            new(
                "trust-store",
                "trust-store",
                "DesktopNodeTrustStoreOps",
                "windows-x509-store",
                [
                    "trust-store-install",
                    "trust-store-remove"
                ]),
            new(
                "credential-manager",
                "credential-manager",
                "DesktopNodeCredentialManagerOps",
                "windows-credential-manager",
                [
                    "credential-manager-system-proof",
                    "credential-manager-default-transition"
                ]),
            new(
                "data-root",
                "data-root",
                "DesktopNodeDataRootLifecycleOps",
                "allowlisted-programdata-root",
                [
                    "data-root-remove"
                ])
        ]);
    }

    public static HostOpsLifecycleDescriptor Create(IReadOnlyList<HostOpsLifecycleBucketDescriptor> buckets)
    {
        return new HostOpsLifecycleDescriptor(
            CurrentSchemaVersion,
            CurrentContractKey,
            CurrentLifecycleBucketContractKey,
            buckets);
    }
}

public sealed record HostOpsLifecycleBucketDescriptor(
    string BucketKey,
    string OperationFamily,
    string Owner,
    string MutationBoundary,
    IReadOnlyList<string> Operations);

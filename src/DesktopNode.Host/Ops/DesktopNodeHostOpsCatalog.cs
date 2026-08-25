using DesktopNode.Contracts;

namespace DesktopNode.Host.Ops;

internal sealed record DesktopNodeHostOpsCatalogEntry(
    string OperationFamily,
    string Owner,
    string MutationBoundary,
    IReadOnlyList<string> Operations)
{
    public string DryRunEvidenceReasonCode => FormatReasonCode("HOST_OPS_DRY_RUN", MutationBoundary);

    public string MutationEvidenceReasonCode => FormatReasonCode("HOST_OPS_MUTATION", MutationBoundary);

    private static string FormatReasonCode(string prefix, string mutationBoundary)
    {
        return $"{prefix}_{mutationBoundary.Replace('-', '_').ToUpperInvariant()}";
    }
}

internal static class DesktopNodeHostOpsCatalog
{
    public const string LifecycleBucketContractKey = "service-eventlog-firewall-truststore-data-root-separated";

    public static IReadOnlyList<string> RequiredLifecycleSmokeBuckets { get; } =
    [
        "service-lifecycle",
        "event-log",
        "firewall",
        "trust-store",
        "data-root"
    ];

    private static readonly HashSet<string> DataRootRequiredOperations =
    [
        "configure-installed",
        "repair-installed",
        "remove-installed",
        "data-root-remove",
        "service-token-rotation-revoke",
        "credential-manager-default-transition",
        "config-migration-apply",
        "job-store-migration-apply"
    ];

    public static IReadOnlyList<DesktopNodeHostOpsCatalogEntry> Entries { get; } =
    [
        new(
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
        new("data-root", "DesktopNodeDataRootLifecycleOps", "allowlisted-programdata-root", ["data-root-remove"]),
        new("config-migration", "DesktopNodeConfigMigrationOps", "allowlisted-programdata-config", ["config-migration-apply"]),
        new("job-store-migration", "DesktopNodeJobStoreMigrationOps", "allowlisted-programdata-job-store", ["job-store-migration-apply"]),
        new("service-token", "DesktopNodeServiceTokenOps", "protected-service-token-file", ["service-token-rotation-revoke"]),
        new(
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
        new("firewall", "DesktopNodeFirewallOps", "windows-firewall-rule", ["firewall-enable", "firewall-remove"]),
        new("trust-store", "DesktopNodeTrustStoreOps", "windows-x509-store", ["trust-store-install", "trust-store-remove"]),
        new(
            "credential-manager",
            "DesktopNodeCredentialManagerOps",
            "windows-credential-manager",
            [
                "credential-manager-system-proof",
                "credential-manager-default-transition"
            ])
    ];

    public static bool TryGetEntry(string operationFamily, out DesktopNodeHostOpsCatalogEntry entry)
    {
        foreach (var candidate in Entries)
        {
            if (string.Equals(candidate.OperationFamily, operationFamily, StringComparison.Ordinal))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null!;
        return false;
    }

    public static bool TryGetOperation(string operation, out DesktopNodeHostOpsCatalogEntry entry)
    {
        foreach (var candidate in Entries)
        {
            if (candidate.Operations.Contains(operation, StringComparer.Ordinal))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null!;
        return false;
    }

    public static bool OperationBelongsTo(string? operation, string operationFamily)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            TryGetOperation(operation, out var entry) &&
            string.Equals(entry.OperationFamily, operationFamily, StringComparison.Ordinal);
    }

    public static string? MutationBoundaryForOperation(string? operation)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            TryGetOperation(operation, out var entry)
                ? entry.MutationBoundary
                : null;
    }

    public static string? DryRunEvidenceReasonForOperation(string? operation)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            TryGetOperation(operation, out var entry)
                ? entry.DryRunEvidenceReasonCode
                : null;
    }

    public static string? MutationEvidenceReasonForOperation(string? operation)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            TryGetOperation(operation, out var entry)
                ? entry.MutationEvidenceReasonCode
                : null;
    }

    public static bool RequiresDataRoot(string? operation)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            DataRootRequiredOperations.Contains(operation);
    }

    public static HostOpsLifecycleDescriptor CreateLifecycleDescriptor()
    {
        var buckets = HostOpsLifecycleDescriptor.CreateDefault().Buckets
            .Select(template =>
            {
                if (!TryGetEntry(template.OperationFamily, out var entry))
                {
                    return template;
                }

                return template with
                {
                    Owner = entry.Owner,
                    MutationBoundary = entry.MutationBoundary,
                    Operations = entry.Operations
                };
            })
            .ToArray();

        return HostOpsLifecycleDescriptor.Create(buckets);
    }
}

namespace DesktopNode.Api;

public sealed record ApiHostCandidateContract(
    IReadOnlyList<string> PublicRouteCandidates,
    string HostReplacementStance,
    string RuntimeReplacementStance,
    string Owner,
    string DefaultOwner)
{
    public static ApiHostCandidateContract CreateDefault()
    {
        return new ApiHostCandidateContract(
            PublicRouteCandidates:
            [
                "/api/v1/runtime/policy",
                "/api/v1/host/status",
                "/api/v1/vms",
                "/api/v1/jobs",
                "/api/v1/jobs/{jobId}",
                "/api/v1/jobs/{jobId}/cancel",
                "/api/v1/jobs/{jobId}/retry",
                "/api/v1/jobs/{jobId}/reconcile",
                "/api/v1/ops/summary",
                "/api/v1/diagnostics/bundles",
                "/api/v1/diagnostics/bundles/{bundleId}/download"
            ],
            HostReplacementStance: "default",
            RuntimeReplacementStance: "default",
            Owner: "local-api",
            DefaultOwner: "dotnet-runtime");
    }
}

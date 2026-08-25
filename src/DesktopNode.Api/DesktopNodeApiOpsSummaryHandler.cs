using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed record DesktopNodeApiOpsSummarySnapshot(
    DesktopNodeHyperVOperationResult HostResult,
    DesktopNodeHyperVOperationResult VmResult,
    IReadOnlyList<JsonElement> JobRows,
    RuntimePolicyData RuntimePolicy,
    JsonElement BatchEvidence,
    string? DiagnosticsRoot,
    DesktopNodeJobStoreHealthSnapshot? JobStoreHealth = null,
    DesktopNodeFeatureQualificationSnapshot? FeatureQualification = null);

internal interface IDesktopNodeApiOpsSummaryQuery
{
    DesktopNodeApiOpsSummarySnapshot Read(CancellationToken cancellationToken);
}

internal sealed class DesktopNodeApiOpsSummaryQuery : IDesktopNodeApiOpsSummaryQuery
{
    private readonly IDesktopNodeHyperVNativeAdapter nativeAdapter;
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly DesktopNodeApiAuthSessionHandler authSessionHandler;
    private readonly BatchEvidenceSummaryReader batchEvidenceReader;
    private readonly string tokenStorage;
    private readonly string currentExposure;
    private readonly DesktopNodeConsoleOptions consoleOptions;
    private readonly string? diagnosticsRoot;
    private readonly DesktopNodeFeatureQualificationSnapshot featureQualification;

    public DesktopNodeApiOpsSummaryQuery(
        IDesktopNodeHyperVNativeAdapter nativeAdapter,
        DesktopNodeJobRuntime jobRuntime,
        DesktopNodeApiAuthSessionHandler authSessionHandler,
        string tokenStorage,
        string currentExposure,
        DesktopNodeConsoleOptions consoleOptions,
        string? batchEvidenceRoot,
        string? diagnosticsRoot,
        DesktopNodeFeatureQualificationSnapshot? featureQualification = null)
    {
        this.nativeAdapter = nativeAdapter;
        this.jobRuntime = jobRuntime;
        this.authSessionHandler = authSessionHandler;
        this.tokenStorage = tokenStorage;
        this.currentExposure = currentExposure;
        this.consoleOptions = consoleOptions;
        batchEvidenceReader = new BatchEvidenceSummaryReader(batchEvidenceRoot);
        this.diagnosticsRoot = diagnosticsRoot;
        this.featureQualification = featureQualification ?? DesktopNodeFeatureQualificationSnapshot.Unavailable();
    }

    public DesktopNodeApiOpsSummarySnapshot Read(CancellationToken cancellationToken)
    {
        var jobRuntimeSnapshot = jobRuntime.Snapshot();
        var jobRows = jobRuntimeSnapshot.Jobs
            .OrderByDescending(job => job.UpdatedAt)
            .ThenByDescending(job => job.CreatedAt)
            .Select(DesktopNodeApiResponseFactory.JobData)
            .ToArray();

        var runtimePolicy = RuntimePolicyContract.CreateDefault(
            tokenStorage,
            currentExposure,
            authSessionHandler.CreateRuntimePolicy(tokenStorage),
            CreateConsoleRuntimePolicy()).Data;

        var hostResult = Invoke("host.status", cancellationToken);
        var vmResult = Invoke("vm.list", cancellationToken);

        return new DesktopNodeApiOpsSummarySnapshot(
            hostResult,
            vmResult,
            jobRows,
            runtimePolicy,
            batchEvidenceReader.Read(),
            diagnosticsRoot,
            jobRuntimeSnapshot.StoreHealth,
            featureQualification);
    }

    private DesktopNodeHyperVOperationResult Invoke(
        string operation,
        CancellationToken cancellationToken)
    {
        nativeAdapter.TryInvoke(operation, DesktopNodeApiResponseFactory.EmptyObject(), cancellationToken, out var result);
        return result;
    }

    private RuntimePolicyConsolePolicy CreateConsoleRuntimePolicy()
    {
        return new RuntimePolicyConsolePolicy(
            Mode: consoleOptions.Enabled ? "windows-hyperv-console-handoff" : "disabled",
            WindowsConsole: "vmconnect",
            NoVnc: consoleOptions.NoVncEnabled ? "available" : "not_configured",
            Transport: consoleOptions.NoVncEnabled ? "websocket-vnc-bridge" : "local-handoff");
    }

}

internal sealed class DesktopNodeApiOpsSummaryHandler
{
    private readonly IDesktopNodeApiOpsSummaryQuery query;

    public DesktopNodeApiOpsSummaryHandler(IDesktopNodeApiOpsSummaryQuery query)
    {
        this.query = query;
    }

    public DesktopNodeApiResponse? TryHandle(
        string method,
        string normalizedPath,
        CancellationToken cancellationToken = default)
    {
        if (!DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "OpsSummary", out _))
        {
            return null;
        }

        var snapshot = query.Read(cancellationToken);
        var data = DesktopNodeApiOpsSummaryBuilder.Build(
            snapshot.HostResult,
            snapshot.VmResult,
            snapshot.JobRows,
            snapshot.RuntimePolicy,
            snapshot.BatchEvidence,
            snapshot.DiagnosticsRoot,
            snapshot.JobStoreHealth,
            snapshot.FeatureQualification);

        return DesktopNodeApiResponseFactory.Json(
            200,
            DesktopNodeApiResponseFactory.Body(true, "ops.summary", data, null));
    }
}

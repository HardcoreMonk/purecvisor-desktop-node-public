using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

public sealed partial class DesktopNodeApiRequestProcessor
{
    public static DesktopNodeApiRequestProcessor CreateDefault(
        string tokenStorage = "none",
        string currentExposure = "loopback",
        IDesktopNodeHyperVNativeAdapter? nativeAdapter = null,
        string? jobStorePath = null,
        string? batchEvidenceRoot = null,
        DesktopNodeApiHardeningOptions? hardeningOptions = null,
        DesktopNodeDiagnosticBundleOptions? diagnosticBundleOptions = null,
        DesktopNodeAccountAuthOptions? accountAuthOptions = null,
        DesktopNodeConsoleOptions? consoleOptions = null,
        IDesktopNodeJobRuntimeEventSink? jobRuntimeEventSink = null,
        string? currentEvidencePath = null)
    {
        var runtimeDependencies = new DesktopNodeApiRuntimeDependencies(
            JobRuntime: DesktopNodeJobRuntime.CreateDefault(jobStorePath, jobRuntimeEventSink));
        return CreateWithDependencies(
            runtimeDependencies,
            tokenStorage,
            currentExposure,
            nativeAdapter,
            batchEvidenceRoot,
            hardeningOptions,
            diagnosticBundleOptions,
            accountAuthOptions,
            consoleOptions,
            currentEvidencePath: currentEvidencePath);
    }

    internal static DesktopNodeApiRequestProcessor CreateWithDependencies(
        DesktopNodeApiRuntimeDependencies runtimeDependencies,
        string tokenStorage = "none",
        string currentExposure = "loopback",
        IDesktopNodeHyperVNativeAdapter? nativeAdapter = null,
        string? batchEvidenceRoot = null,
        DesktopNodeApiHardeningOptions? hardeningOptions = null,
        DesktopNodeDiagnosticBundleOptions? diagnosticBundleOptions = null,
        DesktopNodeAccountAuthOptions? accountAuthOptions = null,
        DesktopNodeConsoleOptions? consoleOptions = null,
        string? currentEvidencePath = null)
    {
        return new DesktopNodeApiRequestProcessor(
            tokenStorage,
            currentExposure,
            nativeAdapter ?? DesktopNodeHyperVNativeAdapter.CreateDefault(),
            runtimeDependencies,
            batchEvidenceRoot,
            hardeningOptions,
            diagnosticBundleOptions,
            accountAuthOptions,
            consoleOptions,
            currentEvidencePath: currentEvidencePath);
    }
}

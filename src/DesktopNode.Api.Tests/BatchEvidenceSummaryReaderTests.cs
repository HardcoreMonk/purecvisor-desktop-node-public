using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

// The current directory is process-global, so this legacy containment probe must be serialized.
// Wave 7 removal: delete this collection and CWD scope after the reader gains an explicit filesystem/path seam.
[CollectionDefinition("Batch evidence CWD isolation", DisableParallelization = true)]
public sealed class BatchEvidenceCwdIsolationCollection
{
}

[Collection("Batch evidence CWD isolation")]
public sealed class BatchEvidenceSummaryReaderTests : IDisposable
{
    private readonly string sandboxRoot;
    private readonly string evidenceRoot;
    private readonly string workingDirectory;
    private readonly string previousWorkingDirectory;
    private bool disposed;

    public BatchEvidenceSummaryReaderTests()
    {
        sandboxRoot = Path.Combine(
            Path.GetTempPath(),
            "pcv-batch-evidence-cwd-sandbox-" + Guid.NewGuid().ToString("N"));
        evidenceRoot = Path.Combine(sandboxRoot, "evidence");
        workingDirectory = Path.Combine(sandboxRoot, "working-directory");
        previousWorkingDirectory = Directory.GetCurrentDirectory();

        Directory.CreateDirectory(evidenceRoot);
        Directory.CreateDirectory(workingDirectory);
        Directory.SetCurrentDirectory(workingDirectory);
    }

    [Fact]
    public void RelativeChildEvidenceIsIgnoredWithoutConfiguredChildRoot()
    {
        var batchRun = Path.Combine(evidenceRoot, "run");
        Directory.CreateDirectory(batchRun);
        File.WriteAllText(Path.Combine(batchRun, "summary.json"), """
        {"schema_version":1,"ok":true,"status":"completed","batch_id":"run","total_steps":1,"executed_steps":1,"results":[{"step_id":"service-msi-hyperv-admin-smoke","ok":true}]}
        """);
        File.WriteAllText(Path.Combine(workingDirectory, "summary.json"), """
        {"schema_version":1,"ok":true,"version":"cwd-version-that-must-not-be-read","final_service":{"state":"CwdService"}}
        """);

        var evidence = new BatchEvidenceSummaryReader(evidenceRoot).Read();

        var latest = evidence.GetProperty("latest");
        Assert.Null(latest.GetProperty("release").GetProperty("version").GetString());
        Assert.Null(latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
        Assert.DoesNotContain("cwd-version-that-must-not-be-read", evidence.GetRawText(), StringComparison.Ordinal);
        Assert.DoesNotContain("CwdService", evidence.GetRawText(), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Directory.SetCurrentDirectory(previousWorkingDirectory);
        if (Directory.Exists(sandboxRoot))
        {
            Directory.Delete(sandboxRoot, recursive: true);
        }

        disposed = true;
    }
}

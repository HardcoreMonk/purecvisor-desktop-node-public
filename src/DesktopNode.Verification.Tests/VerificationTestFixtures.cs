using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

internal static class VerificationCatalogFixture
{
    internal static IReadOnlyList<string> AllowedExecutables { get; } = Array.AsReadOnly([
        "dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"
    ]);

    internal static string RepositoryRoot { get; } = FindRepositoryRoot();

    internal static JsonObject Canonical => JsonNode.Parse(
        CanonicalText)!
        .AsObject();

    internal static string CanonicalText => File.ReadAllText(
        Path.Combine(RepositoryRoot, "config", "development-verification-suites.json"));

    internal static VerificationCatalog LoadCanonical()
    {
        var fileSystem = new PhysicalVerificationFileSystem();
        return new VerificationCatalogLoader(fileSystem).Load(
            Path.Combine(RepositoryRoot, "config", "development-verification-suites.json"),
            Path.Combine(RepositoryRoot, "config", "development-verification-suites.schema.json"));
    }

    internal static VerificationCatalog SevenProcessSuites(int maxParallelism = 4) =>
        CreateProcessCatalog(7, maxParallelism, overallTimeoutSeconds: 30);

    internal static VerificationCatalog OneProcessSuite() =>
        CreateProcessCatalog(1, maxParallelism: 1, overallTimeoutSeconds: 30);

    private static VerificationCatalog CreateProcessCatalog(
        int count,
        int maxParallelism,
        int overallTimeoutSeconds)
    {
        var suites = Enumerable.Range(1, count)
            .Select(index => new SuiteDefinition(
                $"suite-{index}",
                "csharp",
                $"migration-{index}",
                "process",
                "dotnet",
                Array.AsReadOnly(new[] { "--version" }),
                null,
                10))
            .ToArray();

        return new VerificationCatalog(
            1,
            "test-contract",
            "plan-only-foundation",
            maxParallelism,
            overallTimeoutSeconds,
            Array.AsReadOnly(AllowedExecutables.ToArray()),
            Array.AsReadOnly(suites),
            Array.Empty<ShardDefinition>());
    }

    internal static MutatedCatalog LoadMutated(Action<JsonObject> mutate)
    {
        var catalog = Canonical;
        mutate(catalog);
        var contents = catalog.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

        return LoadRawCatalog(contents);
    }

    internal static MutatedCatalog LoadRawCatalog(string contents)
    {
        var temporaryRoot = Path.Combine(Path.GetTempPath(), $"pcv-verification-catalog-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(temporaryRoot);
            var catalogPath = Path.Combine(temporaryRoot, "development-verification-suites.json");
            var schemaPath = Path.Combine(temporaryRoot, "development-verification-suites.schema.json");
            File.WriteAllText(catalogPath, contents);
            File.Copy(
                Path.Combine(RepositoryRoot, "config", "development-verification-suites.schema.json"),
                schemaPath);

            var fileSystem = new PhysicalVerificationFileSystem();
            return new MutatedCatalog(
                temporaryRoot,
                () => new VerificationCatalogLoader(fileSystem).Load(catalogPath, schemaPath));
        }
        catch
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }

            throw;
        }
    }

    internal static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.Verification.Tests", "DesktopNode.Verification.Tests.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    internal sealed class MutatedCatalog(string temporaryRoot, Func<VerificationCatalog> load) : IDisposable
    {
        internal VerificationCatalog Load() => load();

        public void Dispose()
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
    }
}

internal static class VerificationPlanFixture
{
    internal static VerificationPlan Full(bool planOnly) =>
        ForCatalog(VerificationCatalogFixture.LoadCanonical(), planOnly);

    internal static VerificationPlan ForCatalog(VerificationCatalog catalog, bool planOnly)
    {
        var suites = Array.AsReadOnly(catalog.Suites.ToArray());
        var request = new VerificationRequest(
            VerificationLane.Full,
            ChangeTier.M,
            Array.AsReadOnly(new[] { "src/DesktopNode.Verification/VerificationExecutor.cs" }),
            "artifacts/test-run",
            Array.Empty<string>(),
            null,
            planOnly);

        return new VerificationPlan(
            request,
            VerificationLane.Full,
            ChangeTier.M,
            Array.AsReadOnly(new[] { "development-verification-boundary" }),
            null,
            ExecutionScope.Lane,
            null,
            false,
            suites);
    }
}

internal static class VerificationReportFixture
{
    internal static VerificationExecutionReport Planned(IReadOnlyList<SuiteDefinition> suites)
    {
        var rows = suites.Select(suite => new SuiteExecutionRecord(
            suite.Id,
            SuiteStatus.Planned,
            suite.MigrationState,
            null,
            0,
            false,
            false,
            null,
            null,
            null,
            null)).ToArray();

        return new VerificationExecutionReport(0, Array.AsReadOnly(rows));
    }
}

internal static class VerificationSummaryFixture
{
    internal static VerificationSummary Success()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: true);
        var timestamp = DateTimeOffset.Parse("2026-08-24T00:00:00Z");
        return VerificationSummaryFactory.Create(
            plan,
            catalog,
            VerificationReportFixture.Planned(plan.Suites),
            timestamp,
            timestamp);
    }
}

internal sealed class RecordingVerificationFileSystem(
    bool failMove = false,
    Exception? writeException = null,
    Exception? moveException = null,
    Exception? fileExistsException = null,
    Exception? deleteException = null) : IVerificationFileSystem
{
    private readonly Dictionary<string, string> files = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> operations = [];

    internal IReadOnlyList<string> Operations => operations;
    internal string TempPath { get; private set; } = string.Empty;

    public string ReadAllText(string path) => files[path];

    public void CreateDirectory(string path) => operations.Add("create-directory");

    public Task WriteAllTextAsync(
        string path,
        string contents,
        CancellationToken cancellationToken)
    {
        operations.Add("write-temp");
        TempPath = path;
        files[path] = contents;
        if (writeException is not null)
        {
            return Task.FromException(writeException);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public bool FileExists(string path)
    {
        if (fileExistsException is not null)
        {
            throw fileExistsException;
        }

        return files.ContainsKey(path);
    }

    public void MoveFile(string source, string destination, bool overwrite)
    {
        operations.Add(overwrite ? "move-overwrite" : "move");
        if (moveException is not null)
        {
            throw moveException;
        }

        if (failMove)
        {
            throw new IOException("Injected move failure.");
        }

        var contents = files[source];
        files.Remove(source);
        if (!overwrite && files.ContainsKey(destination))
        {
            throw new IOException("Destination already exists.");
        }

        files[destination] = contents;
    }

    public void DeleteFile(string path)
    {
        operations.Add("delete-temp");
        if (deleteException is not null)
        {
            throw deleteException;
        }

        files.Remove(path);
    }
}

internal sealed class RecordingProcessRunner(
    ProcessExecutionResult? result = null,
    TimeSpan? delay = null,
    Task? asyncGate = null,
    bool ignoreCancellation = false,
    bool failIfCalled = false,
    Func<ProcessInvocation, CancellationToken, Task<ProcessExecutionResult>>? handler = null)
    : IProcessRunner
{
    private readonly ProcessExecutionResult configuredResult = result ??
        new ProcessExecutionResult(0, 1, false, false, "", "", new string('0', 64));
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> completionIds = new();
    private int callCount;
    private int currentConcurrency;
    private int maximumConcurrency;

    internal int CallCount => Volatile.Read(ref callCount);
    internal int MaximumConcurrency => Volatile.Read(ref maximumConcurrency);
    internal IReadOnlyList<string> CompletionIds => completionIds.ToArray();

    public async Task<ProcessExecutionResult> RunAsync(
        ProcessInvocation invocation,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref callCount);
        if (failIfCalled)
        {
            throw new InvalidOperationException("Process runner must not be called.");
        }

        var current = Interlocked.Increment(ref currentConcurrency);
        UpdateMaximum(current);

        try
        {
            if (!ignoreCancellation)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (asyncGate is not null)
            {
                if (ignoreCancellation)
                {
                    await asyncGate;
                }
                else
                {
                    await asyncGate.WaitAsync(cancellationToken);
                }
            }

            ProcessExecutionResult completed;
            if (handler is not null)
            {
                completed = await handler(invocation, cancellationToken);
            }
            else
            {
                if (delay is { } configuredDelay)
                {
                    await Task.Delay(configuredDelay, ignoreCancellation ? CancellationToken.None : cancellationToken);
                }

                completed = configuredResult;
            }

            completionIds.Enqueue(invocation.SuiteId);
            return completed;
        }
        finally
        {
            Interlocked.Decrement(ref currentConcurrency);
        }
    }

    private void UpdateMaximum(int current)
    {
        while (true)
        {
            var observed = Volatile.Read(ref maximumConcurrency);
            if (observed >= current ||
                Interlocked.CompareExchange(ref maximumConcurrency, current, observed) == observed)
            {
                return;
            }
        }
    }
}

internal sealed class RecordingManagedSuiteRunner(
    SuiteExecutionRecord? result = null,
    bool failIfCalled = false,
    Task? asyncGate = null,
    bool ignoreCancellation = false,
    Func<SuiteDefinition, CancellationToken, Task<SuiteExecutionRecord>>? handler = null)
    : IManagedSuiteRunner
{
    private int callCount;

    internal int CallCount => Volatile.Read(ref callCount);

    public async Task<SuiteExecutionRecord> RunAsync(
        SuiteDefinition suite,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref callCount);
        if (failIfCalled)
        {
            throw new InvalidOperationException("Managed runner must not be called.");
        }

        if (!ignoreCancellation)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (asyncGate is not null)
        {
            if (ignoreCancellation)
            {
                await asyncGate;
            }
            else
            {
                await asyncGate.WaitAsync(cancellationToken);
            }
        }

        if (handler is not null)
        {
            return await handler(suite, cancellationToken);
        }

        return result ?? new SuiteExecutionRecord(
            suite.Id,
            SuiteStatus.Missing,
            suite.MigrationState,
            null,
            0,
            false,
            false,
            null,
            null,
            null,
            VerificationErrorCodes.ParityUnmapped);
    }
}

internal static class VerificationRequestFixture
{
    internal static VerificationRequest Create(
        string lane,
        string tier,
        IReadOnlyList<string> paths) =>
        new(
            Enum.Parse<VerificationLane>(lane, ignoreCase: true),
            Enum.Parse<ChangeTier>(tier, ignoreCase: true),
            Array.AsReadOnly(paths.ToArray()),
            "artifacts/test-run",
            Array.Empty<string>(),
            null,
            PlanOnly: true);
}

internal sealed class FixedVerificationClock : IVerificationClock
{
    private readonly DateTimeOffset[] values;
    private int index = -1;

    private FixedVerificationClock(DateTimeOffset[] values)
    {
        this.values = values;
    }

    internal static FixedVerificationClock At(params DateTimeOffset[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one clock value is required.", nameof(values));
        }

        return new FixedVerificationClock(values.ToArray());
    }

    public DateTimeOffset UtcNow
    {
        get
        {
            var next = Interlocked.Increment(ref index);
            return values[Math.Min(next, values.Length - 1)];
        }
    }
}

internal sealed class ApplicationRepositoryFixture : IDisposable
{
    private readonly string ownedParent;

    private ApplicationRepositoryFixture(string ownedParent, string root, string userProfile)
    {
        this.ownedParent = ownedParent;
        Root = root;
        UserProfile = userProfile;
    }

    internal string Root { get; }
    internal string UserProfile { get; }
    internal string CatalogPath => Path.Combine(Root, "config", "development-verification-suites.json");
    internal string SchemaPath => Path.Combine(Root, "config", "development-verification-suites.schema.json");
    internal string SolutionPath => Path.Combine(Root, "src", "DesktopNode.sln");
    internal string ActivationState =>
        JsonNode.Parse(File.ReadAllText(CatalogPath))!["activation_state"]!.GetValue<string>();

    internal static ApplicationRepositoryFixture Create()
    {
        var ownedParent = Path.Combine(
            Path.GetTempPath(),
            $"pcv-verification-application-{Guid.NewGuid():N}");
        var root = Path.Combine(ownedParent, "repository");
        var config = Directory.CreateDirectory(Path.Combine(root, "config")).FullName;
        var source = Directory.CreateDirectory(Path.Combine(root, "src")).FullName;
        Directory.CreateDirectory(Path.Combine(root, "artifacts"));
        var userProfile = Directory.CreateDirectory(Path.Combine(ownedParent, "user-profile")).FullName;

        File.Copy(
            Path.Combine(VerificationCatalogFixture.RepositoryRoot, "config", "development-verification-suites.json"),
            Path.Combine(config, "development-verification-suites.json"));
        File.Copy(
            Path.Combine(VerificationCatalogFixture.RepositoryRoot, "config", "development-verification-suites.schema.json"),
            Path.Combine(config, "development-verification-suites.schema.json"));
        File.WriteAllText(Path.Combine(source, "DesktopNode.sln"), string.Empty);

        return new ApplicationRepositoryFixture(ownedParent, root, userProfile);
    }

    internal string ArtifactPath(string name) => Path.Combine(Root, "artifacts", name);

    internal void SetActivationState(string activationState)
    {
        var catalog = JsonNode.Parse(File.ReadAllText(CatalogPath))!.AsObject();
        catalog["activation_state"] = activationState;
        File.WriteAllText(
            CatalogPath,
            catalog.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    internal VerificationApplication CreateApplication(
        RecordingProcessRunner processRunner,
        RecordingManagedSuiteRunner managedSuiteRunner,
        IVerificationClock? clock = null,
        IVerificationFileSystem? fileSystem = null,
        Func<string>? currentDirectory = null) =>
        new(
            processRunner,
            managedSuiteRunner,
            fileSystem ?? new PhysicalVerificationFileSystem(),
            clock ?? FixedVerificationClock.At(DateTimeOffset.Parse("2026-08-24T00:00:00Z")),
            currentDirectory ?? (() => Root),
            () => null,
            () => UserProfile);

    public void Dispose()
    {
        if (Directory.Exists(ownedParent))
        {
            Directory.Delete(ownedParent, recursive: true);
        }
    }
}

internal sealed class FaultInjectingVerificationFileSystem(Exception writeException) : IVerificationFileSystem
{
    private readonly PhysicalVerificationFileSystem inner = new();
    private int writeCallCount;

    internal int WriteCallCount => Volatile.Read(ref writeCallCount);

    public string ReadAllText(string path) => inner.ReadAllText(path);
    public void CreateDirectory(string path) => inner.CreateDirectory(path);

    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref writeCallCount);
        return Task.FromException(writeException);
    }

    public bool FileExists(string path) => inner.FileExists(path);
    public void MoveFile(string source, string destination, bool overwrite) =>
        inner.MoveFile(source, destination, overwrite);
    public void DeleteFile(string path) => inner.DeleteFile(path);
}

internal sealed class FirstWriteCancellationVerificationFileSystem(
    CancellationToken exceptionToken,
    CancellationTokenSource? cancellationSource = null) : IVerificationFileSystem
{
    private readonly PhysicalVerificationFileSystem inner = new();
    private readonly List<CancellationToken> writeTokens = [];
    private int writeCallCount;

    internal int WriteCallCount => Volatile.Read(ref writeCallCount);
    internal IReadOnlyList<CancellationToken> WriteTokens => writeTokens.ToArray();

    public string ReadAllText(string path) => inner.ReadAllText(path);
    public void CreateDirectory(string path) => inner.CreateDirectory(path);

    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken)
    {
        writeTokens.Add(cancellationToken);
        if (Interlocked.Increment(ref writeCallCount) == 1)
        {
            cancellationSource?.Cancel();
            return Task.FromException(new OperationCanceledException(exceptionToken));
        }

        return inner.WriteAllTextAsync(path, contents, cancellationToken);
    }

    public bool FileExists(string path) => inner.FileExists(path);
    public void MoveFile(string source, string destination, bool overwrite) =>
        inner.MoveFile(source, destination, overwrite);
    public void DeleteFile(string path) => inner.DeleteFile(path);
}

internal sealed class ThrowingVerificationClock(
    int throwFromRead,
    Exception exception,
    DateTimeOffset value) : IVerificationClock
{
    private int readCount;

    public DateTimeOffset UtcNow
    {
        get
        {
            if (Interlocked.Increment(ref readCount) >= throwFromRead)
            {
                throw exception;
            }

            return value;
        }
    }
}

internal sealed class FirstReadFailureVerificationFileSystem(Exception readException) : IVerificationFileSystem
{
    private readonly PhysicalVerificationFileSystem inner = new();
    private int readCount;

    public string ReadAllText(string path)
    {
        if (Interlocked.Increment(ref readCount) == 1)
        {
            throw readException;
        }

        return inner.ReadAllText(path);
    }

    public void CreateDirectory(string path) => inner.CreateDirectory(path);
    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken) =>
        inner.WriteAllTextAsync(path, contents, cancellationToken);
    public bool FileExists(string path) => inner.FileExists(path);
    public void MoveFile(string source, string destination, bool overwrite) =>
        inner.MoveFile(source, destination, overwrite);
    public void DeleteFile(string path) => inner.DeleteFile(path);
}

internal sealed class RecoveryMoveFailureVerificationFileSystem(Exception moveException) : IVerificationFileSystem
{
    private readonly PhysicalVerificationFileSystem inner = new();
    private int moveCallCount;
    private int deleteCallCount;

    internal int MoveCallCount => Volatile.Read(ref moveCallCount);
    internal int DeleteCallCount => Volatile.Read(ref deleteCallCount);

    public string ReadAllText(string path) => inner.ReadAllText(path);
    public void CreateDirectory(string path) => inner.CreateDirectory(path);
    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken) =>
        inner.WriteAllTextAsync(path, contents, cancellationToken);
    public bool FileExists(string path) => inner.FileExists(path);

    public void MoveFile(string source, string destination, bool overwrite)
    {
        if (Interlocked.Increment(ref moveCallCount) == 2)
        {
            throw moveException;
        }

        inner.MoveFile(source, destination, overwrite);
    }

    public void DeleteFile(string path)
    {
        Interlocked.Increment(ref deleteCallCount);
        inner.DeleteFile(path);
    }
}

internal sealed class ThrowingLineTextWriter(Exception exception) : StringWriter
{
    private int callCount;

    internal int CallCount => Volatile.Read(ref callCount);

    public override Task WriteLineAsync(string? value)
    {
        Interlocked.Increment(ref callCount);
        return Task.FromException(exception);
    }
}

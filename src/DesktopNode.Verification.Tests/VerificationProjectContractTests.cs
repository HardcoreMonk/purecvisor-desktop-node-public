using System.Xml.Linq;

namespace DesktopNode.Verification.Tests;

[Collection("Console contract tests")]
public sealed class VerificationProjectContractTests
{
    private static readonly SemaphoreSlim ConsoleErrorLock = new(1, 1);

    [Fact]
    public void ProductionProjectHasExpectedExecutableConfiguration()
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(root, "src", "DesktopNode.Verification", "DesktopNode.Verification.csproj");
        var project = XDocument.Load(projectPath);

        AssertSingleProperty(project, "OutputType", "Exe");
        AssertSingleProperty(project, "TargetFramework", "net10.0");
        AssertSingleProperty(project, "AssemblyName", "pcvverify");
        var internalsVisibleTo = project.Root?.Elements("ItemGroup").Elements("InternalsVisibleTo")
            .Where(element => element.Attribute("Include") is not null)
            .ToList() ?? [];
        Assert.Single(internalsVisibleTo);
        Assert.Equal("DesktopNode.Verification.Tests", internalsVisibleTo[0].Attribute("Include")?.Value);
        Assert.Empty(project.Root?.Elements("ItemGroup").Elements("ProjectReference") ?? []);
        var productionPackages = project.Root?.Elements("ItemGroup").Elements("PackageReference")
            .Where(element => element.Attribute("Include") is not null)
            .ToList() ?? [];
        var yamlDotNet = Assert.Single(productionPackages);
        Assert.Equal("YamlDotNet", yamlDotNet.Attribute("Include")?.Value);
        Assert.Equal("18.1.0", yamlDotNet.Attribute("Version")?.Value);
        Assert.Equal(2, yamlDotNet.Attributes().Count());

        var expectedPackages = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["coverlet.collector"] = "6.0.4",
            ["Microsoft.NET.Test.Sdk"] = "17.14.1",
            ["xunit"] = "2.9.3",
            ["xunit.runner.visualstudio"] = "3.1.4",
        };

        var testProjectPath = Path.Combine(root, "src", "DesktopNode.Verification.Tests", "DesktopNode.Verification.Tests.csproj");
        var testProject = XDocument.Load(testProjectPath);
        var packageReferences = testProject.Root?.Elements("ItemGroup").Elements("PackageReference")
            .Where(element => element.Attribute("Include") is not null)
            .ToList() ?? [];
        Assert.Equal(expectedPackages.Count, packageReferences.Count);
        foreach (var package in expectedPackages)
        {
            var matches = packageReferences.Where(element => element.Attribute("Include")?.Value == package.Key).ToList();
            Assert.Single(matches);
            Assert.Equal(package.Value, matches[0].Attribute("Version")?.Value);
        }
    }

    [Fact]
    public async Task SolutionContainsProductionAndTestProjectsExactlyOnce()
    {
        var root = FindRepositoryRoot();
        var solution = File.ReadAllText(Path.Combine(root, "src", "DesktopNode.sln"));

        Assert.Equal(1, CountOccurrences(solution, "DesktopNode.Verification\\DesktopNode.Verification.csproj"));
        Assert.Equal(1, CountOccurrences(solution, "DesktopNode.Verification.Tests\\DesktopNode.Verification.Tests.csproj"));
        Assert.Equal(1, CountOccurrences(solution, "DesktopNode.Delivery.Tests\\DesktopNode.Delivery.Tests.csproj"));

        await ConsoleErrorLock.WaitAsync();
        var originalError = Console.Error;
        using var capturedError = new StringWriter();
        try
        {
            Console.SetError(capturedError);
            var exitCode = await DesktopNode.Verification.Program.Main([]);

            Assert.Equal(2, exitCode);
            using var document = System.Text.Json.JsonDocument.Parse(capturedError.ToString());
            var error = document.RootElement;
            Assert.Equal(2, error.GetProperty("schema_version").GetInt32());
            Assert.Equal("pcv-development-verification-summary-v2", error.GetProperty("contract").GetString());
            Assert.False(error.GetProperty("ok").GetBoolean());
            Assert.Equal(VerificationErrorCodes.ConfigInvalid, error.GetProperty("error_code").GetString());
            Assert.Equal("cli:unknown-command", error.GetProperty("error_detail").GetString());
            Assert.DoesNotContain("Exception", capturedError.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain("credential", capturedError.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("token", capturedError.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetError(originalError);
            ConsoleErrorLock.Release();
        }
    }

    private static void AssertSingleProperty(XDocument project, string propertyName, string expectedValue)
    {
        var properties = project.Root?.Elements("PropertyGroup").Elements(propertyName).ToList() ?? [];
        Assert.Single(properties);
        Assert.Equal(expectedValue, properties[0].Value);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("PCV_VERIFY_CONFIG_INVALID|repository-root-not-found");
    }

    private static int CountOccurrences(string value, string search)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(search, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += search.Length;
        }

        return count;
    }
}

public sealed class ConsoleCancellationBridgeTests
{
    [Fact]
    public void CallbackCanWaitForDisposeWithoutDeadlockAndLaterSignalsAreNoOps()
    {
        var bridge = new ConsoleCancellationBridge();
        var disposeCompletedBeforeCallbackReturned = false;
        using var registration = bridge.Token.Register(() =>
        {
            var dispose = Task.Run(bridge.Dispose);
            disposeCompletedBeforeCallbackReturned = dispose.Wait(TimeSpan.FromSeconds(2));
        });

        bridge.Signal();
        var laterSignalException = Record.Exception(bridge.Signal);
        var repeatedDisposeException = Record.Exception(bridge.Dispose);

        Assert.True(disposeCompletedBeforeCallbackReturned);
        Assert.Null(laterSignalException);
        Assert.Null(repeatedDisposeException);
    }

    [Fact]
    public void SignalCancelsTokenWithoutThrowingWhenRegisteredCallbackThrows()
    {
        using var bridge = new ConsoleCancellationBridge();
        using var registration = bridge.Token.Register(
            () => throw new InvalidOperationException("TOP_SECRET_CANCEL_CALLBACK"));

        var exception = Record.Exception(bridge.Signal);

        Assert.Null(exception);
        Assert.True(bridge.Token.IsCancellationRequested);
    }

    [Fact]
    public void SignalPropagatesFatalCallbackWrappedByAggregateException()
    {
        using var bridge = new ConsoleCancellationBridge();
        using var registration = bridge.Token.Register(
            () => throw new OutOfMemoryException("fatal-callback"));

        var exception = Assert.Throws<AggregateException>(bridge.Signal);

        Assert.Contains(exception.Flatten().InnerExceptions, inner => inner is OutOfMemoryException);
    }

    [Fact]
    public async Task ConcurrentSignalAndDisposeRepeatsWithoutExceptionOrDeadlock()
    {
        for (var iteration = 0; iteration < 64; iteration++)
        {
            var bridge = new ConsoleCancellationBridge();
            var token = bridge.Token;
            var signals = Enumerable.Range(0, 8)
                .Select(_ => Task.Run(() =>
                {
                    for (var signal = 0; signal < 32; signal++)
                    {
                        bridge.Signal();
                    }
                }))
                .ToArray();
            var dispose = Task.Run(() => bridge.Dispose());

            await Task.WhenAll(signals.Append(dispose)).WaitAsync(TimeSpan.FromSeconds(5));
            bridge.Dispose();
            Assert.True(token.IsCancellationRequested || !token.CanBeCanceled || dispose.IsCompletedSuccessfully);
        }
    }
}

[CollectionDefinition("Console contract tests", DisableParallelization = true)]
public sealed class ConsoleContractCollection
{
}

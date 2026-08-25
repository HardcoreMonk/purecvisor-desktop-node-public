using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationCatalogTests
{
    [Fact]
    public void CanonicalCatalogHasSevenSuitesAndFourDisjointShards()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();

        Assert.Equal("pcv-development-verification-suite-catalog-v1", catalog.Contract);
        Assert.Contains(catalog.ActivationState, new[] { "plan-only-foundation", "shadow-ready", "active" });
        Assert.Equal(4, catalog.MaxParallelism);
        Assert.Equal([
            "dotnet", "web-typecheck", "web-parity", "delivery-contracts",
            "installer-contracts", "evidence-check", "policy-boundaries"
        ], catalog.Suites.Select(suite => suite.Id));
        Assert.Equal(["dotnet", "web", "delivery", "installer-policy"], catalog.Shards.Select(shard => shard.Id));
        Assert.Equal(7, catalog.Shards.SelectMany(shard => shard.SuiteIds).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(VerificationCatalogFixture.AllowedExecutables, catalog.AllowedExecutables);

        (string Id, string[] SuiteIds)[] expectedShards =
        [
            ("dotnet", ["dotnet"]),
            ("web", ["web-typecheck", "web-parity"]),
            ("delivery", ["delivery-contracts", "evidence-check"]),
            ("installer-policy", ["installer-contracts", "policy-boundaries"])
        ];
        for (var index = 0; index < expectedShards.Length; index++)
        {
            Assert.Equal(expectedShards[index].Id, catalog.Shards[index].Id);
            Assert.Equal(expectedShards[index].SuiteIds, catalog.Shards[index].SuiteIds);
        }

        var expectedMigrationStates = catalog.ActivationState switch
        {
            "plan-only-foundation" => new[]
            {
                "native-existing", "native-existing", "wave-b-pending", "mapped", "mapped", "mapped", "wave-a-foundation"
            },
            "shadow-ready" => new[]
            {
                "native-existing", "native-existing", "mapped", "mapped", "mapped", "mapped", "mapped"
            },
            "active" => Enumerable.Repeat("cutover", 7).ToArray(),
            _ => throw new InvalidOperationException("Unexpected canonical activation state.")
        };

        (string Id, string Owner, string Kind, string? FileName,
            string[] Arguments, string? ManagedHandler, int TimeoutSeconds)[] expectedSuites =
        [
            ("dotnet", "csharp", "process", "dotnet",
                ["test", "src/DesktopNode.sln", "-c", "Release", "--no-build", "--no-restore", "--nologo"], null, 900),
            ("web-typecheck", "node", "process", "npm",
                ["run", "test:required", "--prefix", "web"], null, 600),
            ("web-parity", "node", "process", "npm",
                ["run", "check:verification-migration-manifest", "--prefix", "web"], null, 600),
            ("delivery-contracts", "csharp", "process", "dotnet",
                ["test", "src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj", "-c", "Release", "--no-build", "--no-restore", "--filter", "Category=Delivery", "--nologo"], null, 900),
            ("installer-contracts", "csharp", "process", "dotnet",
                ["test", "src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj", "-c", "Release", "--no-build", "--no-restore", "--filter", "Category=Installer", "--nologo"], null, 900),
            ("evidence-check", "csharp", "managed", null,
                [], "current-evidence-check", 300),
            ("policy-boundaries", "csharp", "managed", null,
                [], "policy-boundaries", 300)
        ];
        for (var index = 0; index < expectedSuites.Length; index++)
        {
            var expected = expectedSuites[index];
            var actual = catalog.Suites[index];
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Owner, actual.Owner);
            Assert.Equal(expectedMigrationStates[index], actual.MigrationState);
            Assert.Equal(expected.Kind, actual.ExecutorKind);
            Assert.Equal(expected.FileName, actual.FileName);
            Assert.Equal(expected.Arguments, actual.Arguments);
            Assert.Equal(expected.ManagedHandler, actual.ManagedHandler);
            Assert.Equal(expected.TimeoutSeconds, actual.TimeoutSeconds);
        }
    }

    [Theory]
    [InlineData("shadow-ready")]
    [InlineData("active")]
    public void CatalogAcceptsSupportedExecutionActivationState(string activationState)
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["activation_state"] = activationState);

        Assert.Equal(activationState, mutated.Load().ActivationState);
    }

    [Fact]
    public void CatalogAcceptsCutoverMigrationState()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["suites"]![0]!["migration_state"] = "cutover");

        Assert.Equal("cutover", mutated.Load().Suites[0].MigrationState);
    }

    [Fact]
    public void CatalogRejectsRetiredWaveDPendingState()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["suites"]![3]!["migration_state"] = "wave-d-pending");

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("suite-definition:delivery-contracts=invalid", exception.Detail);
    }

    [Theory]
    [InlineData(@"C:\untrusted\dotnet.exe")]
    [InlineData("/tmp/dotnet")]
    [InlineData(@"..\dotnet.exe")]
    [InlineData("tools/dotnet")]
    public void CatalogRejectsPathQualifiedExecutable(string fileName)
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["suites"]![0]!["executor"]!["file_name"] = fileName);

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("catalog-command-forbidden:dotnet", exception.Detail);
    }

    [Fact]
    public void CatalogStoresAcceptedExecutableAsNormalizedBasename()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["suites"]![0]!["executor"]!["file_name"] = "DOTNET.EXE");

        var catalog = mutated.Load();

        Assert.Equal("dotnet.exe", catalog.Suites[0].FileName);
    }

    [Fact]
    public void CatalogRejectsDuplicateRootPropertyBeforeLastValueCanWin()
    {
        var rawCatalog = VerificationCatalogFixture.CanonicalText.Replace(
            "\"contract\": \"pcv-development-verification-suite-catalog-v1\"",
            "\"contract\": \"private-wrong-contract\",\n  \"contract\": \"pcv-development-verification-suite-catalog-v1\"",
            StringComparison.Ordinal);
        using var catalog = VerificationCatalogFixture.LoadRawCatalog(rawCatalog);

        var exception = Assert.Throws<VerificationException>(() => catalog.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("catalog-json=duplicate-property", exception.Detail);
    }

    [Fact]
    public void CatalogRejectsDuplicateNestedExecutorProperty()
    {
        var rawCatalog = VerificationCatalogFixture.CanonicalText.Replace(
            "\"file_name\": \"dotnet\"",
            "\"file_name\": \"node\",\n        \"file_name\": \"dotnet\"",
            StringComparison.Ordinal);
        using var catalog = VerificationCatalogFixture.LoadRawCatalog(rawCatalog);

        var exception = Assert.Throws<VerificationException>(() => catalog.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("catalog-json=duplicate-property", exception.Detail);
    }

    [Fact]
    public void MissingCatalogReadUsesStableDetailWithoutPathLeak()
    {
        var privatePath = Path.Combine(Path.GetTempPath(), $"private-customer-{Guid.NewGuid():N}.json");
        var schemaPath = Path.Combine(
            VerificationCatalogFixture.RepositoryRoot,
            "config",
            "development-verification-suites.schema.json");

        var exception = Assert.Throws<VerificationException>(() =>
            new VerificationCatalogLoader(new PhysicalVerificationFileSystem()).Load(privatePath, schemaPath));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("catalog-json=read-failed", exception.Detail);
        Assert.False(exception.Message.Contains(privatePath, StringComparison.Ordinal));
    }

    [Fact]
    public void AccessDeniedCatalogReadUsesStableDetailWithoutMessageLeak()
    {
        const string privatePath = @"D:\private-customer\catalog.json";
        const string privateMessage = "access denied for private-customer";
        const string schemaPath = "schema.json";
        var schemaContents = File.ReadAllText(Path.Combine(
            VerificationCatalogFixture.RepositoryRoot,
            "config",
            "development-verification-suites.schema.json"));
        var fileSystem = new AccessDeniedCatalogFileSystem(
            privatePath,
            schemaPath,
            schemaContents,
            privateMessage);

        var exception = Assert.Throws<VerificationException>(() =>
            new VerificationCatalogLoader(fileSystem).Load(privatePath, schemaPath));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("catalog-json=read-failed", exception.Detail);
        Assert.False(exception.Message.Contains(privatePath, StringComparison.Ordinal));
        Assert.False(exception.Message.Contains(privateMessage, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("pwsh", "-NoProfile", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    [InlineData("dotnet", "Invoke-Pester", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    [InlineData("msiexec", "/i", "PCV_VERIFY_CONFIG_INVALID")]
    [InlineData("dotnet", "AllowHostMutation", "PCV_VERIFY_CONFIG_INVALID")]
    public void CatalogRejectsForbiddenExecutableOrArgument(string fileName, string argument, string code)
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
        {
            var executor = root["suites"]![0]!["executor"]!.AsObject();
            executor["file_name"] = fileName;
            executor["arguments"] = new JsonArray(argument);
        });

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(code, exception.Code);
    }

    [Fact]
    public void CatalogRejectsDuplicateShardUnionMember()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
        {
            root["shards"]![1]!["suite_ids"] = new JsonArray("web-typecheck", "web-parity", "dotnet");
        });

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("shard-union", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogRejectsIncompleteShardUnionWithoutDuplicateOrUnknownMember()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
        {
            root["shards"]![1]!["suite_ids"] = new JsonArray("web-parity");
        });

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("shard-union", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogRejectsUnknownJsonProperty()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root => root["unexpected"] = true);

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("catalog-json", exception.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("schema_version", 2)]
    [InlineData("contract", "wrong-contract")]
    [InlineData("activation_state", "unknown")]
    [InlineData("max_parallelism", 3)]
    [InlineData("overall_timeout_seconds", 0)]
    public void CatalogRejectsInvalidHeader(string propertyName, object value)
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root[propertyName] = value switch
            {
                int integer => JsonValue.Create(integer),
                string text => JsonValue.Create(text),
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            });

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("catalog-header", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogRejectsExecutableAllowlistMismatch()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["allowed_executables"]![7] = "cmd.exe");

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("executable-allowlist", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogAcceptsReorderedExactExecutableAllowlist()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["allowed_executables"] = new JsonArray(
                "git.exe", "git", "npm.cmd", "npm", "node.exe", "node", "dotnet.exe", "dotnet"));

        var catalog = mutated.Load();

        Assert.Equal(VerificationCatalogFixture.AllowedExecutables.Order(), catalog.AllowedExecutables.Order());
    }

    [Fact]
    public void CatalogRejectsDuplicateExecutableAllowlistEntry()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["allowed_executables"]![7] = "dotnet");

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("executable-allowlist", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogRejectsInvalidExecutorUnion()
    {
        using var mutated = VerificationCatalogFixture.LoadMutated(root =>
            root["suites"]![0]!["executor"]!["managed_handler"] = "policy-boundaries");

        var exception = Assert.Throws<VerificationException>(() => mutated.Load());

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Contains("suite-definition:dotnet", exception.Detail, StringComparison.Ordinal);
    }

    private sealed class AccessDeniedCatalogFileSystem(
        string privatePath,
        string schemaPath,
        string schemaContents,
        string privateMessage) : IVerificationFileSystem
    {
        public string ReadAllText(string path) => path == privatePath
            ? throw new UnauthorizedAccessException(privateMessage)
            : path == schemaPath
                ? schemaContents
                : throw new FileNotFoundException("unexpected test path");

        public bool FileExists(string path) => true;
        public void CreateDirectory(string path) => throw new NotSupportedException();
        public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void MoveFile(string source, string destination, bool overwrite) => throw new NotSupportedException();
        public void DeleteFile(string path) => throw new NotSupportedException();
    }
}

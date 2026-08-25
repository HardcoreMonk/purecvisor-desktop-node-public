using DesktopNode.Delivery.Tests.Infrastructure;
using System.Text.Json;

namespace DesktopNode.Delivery.Tests.Contracts;

[Trait("Category", "VerificationInfrastructure")]
public sealed class LegacyPesterContractParserTests
{
    [Fact]
    public void ParsesSingleAndDoubleQuotedLiteralNamesAndEscapes()
    {
        const string source =
            "Describe 'fixture' {\n" +
            "  It 'it''s exact' { }\n" +
            "  It \"escaped `\"quote`\" and ``tick\" { }\n" +
            "}";

        var contracts = LegacyPesterContractParser.Parse("fixtures/Literal.Tests.ps1", source);

        Assert.Equal(
            [
                new LegacyPesterContract(1, "it's exact"),
                new LegacyPesterContract(2, "escaped \"quote\" and `tick"),
            ],
            contracts);
    }

    [Fact]
    public void IgnoresCommentsStringsHereStringsAndNonCommandTokens()
    {
        const string source =
            "# It 'line comment' { }\n" +
            "<# It 'block comment' { } #>\n" +
            "$single = 'It ''single string'' { }'\n" +
            "$double = \"It 'double string' { }\"\n" +
            "$singleHere = @'\nIt 'single here' { }\n'@\n" +
            "$doubleHere = @\"\nIt 'double here' { }\n\"@\n" +
            "Itinerary 'not a command'\n" +
            "Describe 'fixture' { It 'observed contract' { } }\n";

        var contracts = LegacyPesterContractParser.Parse("fixtures/States.Tests.ps1", source);

        Assert.Equal([new LegacyPesterContract(1, "observed contract")], contracts);
    }

    [Fact]
    public void AcceptsWindowsNewlinesAndAnEscapedDollarInALiteralName()
    {
        const string source = "Describe 'fixture' {\r\n  It \"literal `$value\" { }\r\n}";

        var contracts = LegacyPesterContractParser.Parse("fixtures/Windows.Tests.ps1", source);

        Assert.Equal([new LegacyPesterContract(1, "literal $value")], contracts);
    }

    [Theory]
    [InlineData("It \"dynamic $value\" { }", "dynamic-name")]
    [InlineData("It $name { }", "dynamic-name")]
    [InlineData("It\n  'continued' { }", "multiline-declaration")]
    [InlineData("It 'duplicate' { }; It 'duplicate' { }", "duplicate-name")]
    [InlineData("It 'unterminated", "unmatched-quote")]
    [InlineData("<# unterminated", "unmatched-comment")]
    [InlineData("@'\nunterminated", "unmatched-here-string")]
    public void RejectsAmbiguousOrMalformedDeclarations(string source, string detail)
    {
        var error = Assert.Throws<InvalidDataException>(
            () => LegacyPesterContractParser.Parse("fixtures/Invalid.Tests.ps1", source));

        Assert.Equal($"{LegacyPesterContractParser.ErrorCode}|{detail}", error.Message);
        Assert.DoesNotContain(Path.GetFullPath("."), error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(
        "installer",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
        1,
        "pcv.installer.desktop-node-installer-internal-trust.001")]
    [InlineData(
        "delivery",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        7,
        "pcv.delivery.04273-promotion-evidence.007")]
    public void CreatesDeterministicContractIds(
        string domain,
        string path,
        int ordinal,
        string expected)
    {
        Assert.Equal(expected, LegacyContractId.Create(domain, path, ordinal));
    }

    [Fact]
    public void ParsesTheExactRepositoryInstallerAndPackagingInventory()
    {
        var repository = RepositoryContractContext.Find();
        var installerPaths = Directory
            .EnumerateFiles(
                Path.Combine(FindRepositoryRoot(), "packaging", "windows-desktop-node", "installer", "tests"),
                "*.Tests.ps1",
                SearchOption.TopDirectoryOnly)
            .Select(ToRepositoryRelativePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var packagingPaths = Directory
            .EnumerateFiles(
                Path.Combine(FindRepositoryRoot(), "packaging", "windows-desktop-node", "tests"),
                "*.Tests.ps1",
                SearchOption.TopDirectoryOnly)
            .Select(ToRepositoryRelativePath)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var installer = installerPaths.Sum(path => CountContracts(repository, path));
        var packaging = packagingPaths.Sum(path => CountContracts(repository, path));

        Assert.Equal(6, installerPaths.Length);
        Assert.Equal(55, packagingPaths.Length);
        Assert.Equal(49, installer);
        Assert.Equal(528, packaging);
        Assert.Equal(577, installer + packaging);
    }

    [Theory]
    [InlineData("../outside.md")]
    [InlineData("src\\DesktopNode.sln")]
    [InlineData("C:/outside.md")]
    [InlineData("src/../DesktopNode.sln")]
    public void RepositoryContextRejectsUncontainedOrNonCanonicalPaths(string path)
    {
        var repository = RepositoryContractContext.Find();

        var error = Assert.Throws<InvalidDataException>(() => repository.ReadUtf8Text(path));

        Assert.StartsWith("PCV_DELIVERY_PATH_INVALID|", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(FindRepositoryRoot(), error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RepositoryContextRejectsNulWithoutDisclosingTheRoot()
    {
        var repository = RepositoryContractContext.Find();

        var error = Assert.Throws<InvalidDataException>(
            () => repository.ReadUtf8Text("src/\0DesktopNode.sln"));

        Assert.Equal("PCV_DELIVERY_PATH_INVALID|format", error.Message);
        Assert.DoesNotContain(FindRepositoryRoot(), error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RepositoryContextLoadsJsonAndXmlWithoutAWriteOrProcessSurface()
    {
        var repository = RepositoryContractContext.Find();

        using var json = repository.LoadJson("config/development-verification-migration-manifest.json");
        var project = repository.LoadXml(
            "src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj");

        Assert.Equal(JsonValueKind.Object, json.RootElement.ValueKind);
        Assert.Equal("Project", project.Root?.Name.LocalName);
        var callableNames = typeof(RepositoryContractContext)
            .GetMethods(System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic)
            .Where(method => method.DeclaringType == typeof(RepositoryContractContext))
            .Select(method => method.Name)
            .ToArray();
        Assert.DoesNotContain(callableNames, name =>
            name.Contains("Write", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Process", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Execute", StringComparison.OrdinalIgnoreCase));
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

        throw new InvalidOperationException("PCV_DELIVERY_CONFIG_INVALID|repository-root-not-found");
    }

    private static string ToRepositoryRelativePath(string path) =>
        Path.GetRelativePath(FindRepositoryRoot(), path).Replace('\\', '/');

    private static int CountContracts(RepositoryContractContext repository, string path)
    {
        try
        {
            return LegacyPesterContractParser.Parse(path, repository.ReadUtf8Text(path)).Count;
        }
        catch (InvalidDataException error)
        {
            throw new Xunit.Sdk.XunitException($"{path}: {error.Message}");
        }
    }
}

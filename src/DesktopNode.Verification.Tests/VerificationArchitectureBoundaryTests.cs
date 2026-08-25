using System.Text.Json;
using System.Xml.Linq;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationArchitectureBoundaryTests
{
    [Fact]
    public void ProductionProjectHasNoProductOrPowerShellDependency()
    {
        var root = FindRepositoryRoot();
        var project = XDocument.Load(Path.Combine(
            root, "src", "DesktopNode.Verification", "DesktopNode.Verification.csproj"));

        Assert.Empty(VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project));
    }

    [Fact]
    public void ProductionCompileSourcesConservativelyContainNoProductWmiOrInstallerTokens()
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(
            root, "src", "DesktopNode.Verification", "DesktopNode.Verification.csproj");
        var source = string.Join("\n", VerificationArchitectureBoundaryValidator
            .GetCompileSourceFiles(projectPath, root)
            .Select(File.ReadAllText));

        Assert.DoesNotContain("System.Management", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Management.Infrastructure", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WindowsInstaller", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogContainsNoDecodedPowerShellAndRemainsPlanOnlyFoundation()
    {
        var text = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "config", "development-verification-suites.json"));
        using var document = JsonDocument.Parse(text);

        _ = VerificationCatalogFixture.LoadCanonical();
        Assert.Equal("plan-only-foundation", document.RootElement.GetProperty("activation_state").GetString());
        Assert.Empty(VerificationArchitectureBoundaryValidator.FindForbiddenCatalogStrings(
            document.RootElement,
            text));
    }

    [Fact]
    public void EvidenceRefusesCutoverMutationAndPromotionClaims()
    {
        var evidence = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "docs", "ga-ready", "evidence",
            "pester-free-csharp-verification-wave-a-foundation-2026-08-24.md"));

        Assert.Empty(VerificationArchitectureBoundaryValidator.FindEvidenceClaimViolations(evidence));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("PCV_VERIFY_CONFIG_INVALID|repository-root-not-found");
    }
}

public sealed class VerificationArchitectureBoundarySyntheticTests
{
    [Fact]
    public void CustomRootSdkFailsClosedByStaticProjectAllowlist()
    {
        string[] unsupportedProjects =
        [
            "<Project Sdk=\"Contoso.Build.Sdk\" />",
            "<Project Sdk=\"Microsoft.NET.Sdk;Contoso.Build.Sdk\" />",
            """
            <Project Sdk="$(ProjectSdk)">
              <PropertyGroup><ProjectSdk>Microsoft.NET.Sdk</ProjectSdk></PropertyGroup>
            </Project>
            """,
        ];

        foreach (var project in unsupportedProjects)
        {
            using var fixture = new SyntheticArchitectureProject();
            fixture.WriteProject(project);

            AssertStaticProjectRejected(() =>
                _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
        }
    }

    [Fact]
    public void DynamicCompileTargetFailsClosedByStaticProjectAllowlist()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteOutsideSource();
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <Target BeforeTargets="CoreCompile">
                <CreateItem Include="..\Outside.cs">
                  <Output TaskParameter="Include" ItemName="Compile" />
                </CreateItem>
              </Target>
            </Project>
            """);

        AssertStaticProjectRejected(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
    }

    [Fact]
    public void UsingTaskFailsClosedByStaticProjectAllowlist()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <UsingTask TaskName="PcvSyntheticTask" AssemblyFile="PcvSyntheticTask.dll" />
            </Project>
            """);

        AssertStaticProjectRejected(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
    }

    [Fact]
    public void AncestorImplicitBuildInputFailsClosedByStaticProjectAllowlist()
    {
        foreach (var implicitInputName in new[] { "Directory.Build.props", "Directory.Build.targets" })
        {
            using var fixture = new SyntheticArchitectureProject();
            fixture.WriteOutsideSource();
            File.WriteAllText(Path.Combine(fixture.OwnedRoot, implicitInputName), """
                <Project>
                  <ItemGroup><Compile Include="..\Outside.cs" /></ItemGroup>
                </Project>
                """);
            fixture.WriteProject("<Project Sdk=\"Microsoft.NET.Sdk\" />");

            AssertStaticProjectRejected(() =>
                _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(
                    fixture.ProjectPath,
                    fixture.ProjectRoot));
        }
    }

    [Fact]
    public void AncestorDirectoryPackagesPropsFailsClosedByAutomaticBuildInputGuard()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteOutsideSource();
        File.WriteAllText(Path.Combine(fixture.OwnedRoot, "Directory.Packages.props"), """
            <Project>
              <ItemGroup><Compile Include="..\Outside.cs" /></ItemGroup>
            </Project>
            """);
        fixture.WriteProject("<Project Sdk=\"Microsoft.NET.Sdk\" />");

        AssertStaticProjectRejected(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(
                fixture.ProjectPath,
                fixture.ProjectRoot));
    }

    [Fact]
    public void MixedCaseDependencyItemFailsClosedWithoutEvaluatedGraph()
    {
        var project = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <pRoJeCtReFeReNcE Include="..\DesktopNode.Runtime\DesktopNode.Runtime.csproj" />
              </ItemGroup>
            </Project>
            """);

        var violations = VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project);

        Assert.Contains(violations, violation => violation.StartsWith(
            "unsupported-unevaluated-build-graph:",
            StringComparison.Ordinal));
    }

    [Fact]
    public void PropertyIndirectedPackageReferenceFailsClosedWithoutEvaluatedGraph()
    {
        var project = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><DependencyName>Microsoft.PowerShell.SDK</DependencyName></PropertyGroup>
              <ItemGroup><PackageReference Include="$(DependencyName)" /></ItemGroup>
            </Project>
            """);

        var violations = VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project);

        Assert.Contains(violations, violation => violation.StartsWith(
            "unsupported-unevaluated-build-graph:",
            StringComparison.Ordinal));
    }

    [Fact]
    public void ExactYamlDotNetPackageIsTheOnlySupportedProductionDependency()
    {
        var project = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup><PackageReference Include="YamlDotNet" Version="18.1.0" /></ItemGroup>
            </Project>
            """);

        Assert.Empty(VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project));
    }

    [Theory]
    [InlineData("YamlDotNet", "18.0.0")]
    [InlineData("YamlDotNet", "$(YamlVersion)")]
    [InlineData("Contoso.Yaml", "18.1.0")]
    public void AnyOtherProductionPackageDeclarationIsRejected(string package, string version)
    {
        var project = XDocument.Parse($"""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup><PackageReference Include="{package}" Version="{version}" /></ItemGroup>
            </Project>
            """);

        Assert.Contains(
            VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project),
            violation => violation.StartsWith("unsupported-unevaluated-build-graph:", StringComparison.Ordinal));
    }

    [Fact]
    public void ExplicitImportedPropsAddingCompileFailsClosedWithoutEvaluatedGraph()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteOutsideSource();
        File.WriteAllText(Path.Combine(fixture.ProjectRoot, "Imported.props"), """
            <Project>
              <ItemGroup><Compile Include="..\Outside.cs" Link="Outside.cs" /></ItemGroup>
            </Project>
            """);
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <Import Project="Imported.props" />
            </Project>
            """);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));

        Assert.True(
            exception.Message.Contains(
                "unsupported-unevaluated-build-graph-import",
                StringComparison.Ordinal),
            exception.Message);
    }

    [Fact]
    public void NamespacedProjectReferenceIsRejected()
    {
        var project = XDocument.Parse("""
            <Project xmlns="urn:pcv:test">
              <ItemGroup>
                <ProjectReference Include="..\DesktopNode.Runtime\DesktopNode.Runtime.csproj" />
              </ItemGroup>
            </Project>
            """);

        var violations = VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project);

        Assert.Contains(violations, violation => violation.Contains("ProjectReference", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("<Reference Include=\"System.Management.Automation\" />")]
    [InlineData("<Import Project=\"build/check.ps1\" />")]
    [InlineData("<PackageReference Update=\"Microsoft.PowerShell.SDK\" />")]
    public void NamespacedReferenceOrImportPowerShellDependencyIsRejected(string item)
    {
        var project = XDocument.Parse($"""
            <Project xmlns="urn:pcv:test">
              <ItemGroup>{item}</ItemGroup>
            </Project>
            """);

        Assert.NotEmpty(VerificationArchitectureBoundaryValidator.FindProjectDependencyViolations(project));
    }

    [Fact]
    public void ExternalLinkedCompileOutsideProjectRootIsRejected()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteOutsideSource();
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
              <ItemGroup><Compile Include="..\Outside.cs" Link="Outside.cs" /></ItemGroup>
            </Project>
            """);

        Assert.Throws<InvalidOperationException>(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
    }

    [Fact]
    public void RootedCompileIncludeIsRejected()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteOutsideSource();
        fixture.WriteProject(new XDocument(
            new XElement("Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup",
                    new XElement("EnableDefaultCompileItems", "false")),
                new XElement("ItemGroup",
                    new XElement("Compile", new XAttribute("Include", fixture.OutsideSourcePath))))));

        Assert.Throws<InvalidOperationException>(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
    }

    [Fact]
    public void EscapingCompileLinkIsRejected()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteSource("Inside.cs");
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
              <ItemGroup><Compile Include="Inside.cs" Link="..\Outside.cs" /></ItemGroup>
            </Project>
            """);

        Assert.Throws<InvalidOperationException>(() =>
            _ = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath));
    }

    [Fact]
    public void DefaultCompileScanExcludesBinAndObj()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteProject("<Project Sdk=\"Microsoft.NET.Sdk\" />");
        fixture.WriteSource("Source.cs");
        fixture.WriteSource("bin/Generated.cs");
        fixture.WriteSource("obj/Generated.cs");

        var relative = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath)
            .Select(path => Path.GetRelativePath(fixture.ProjectRoot, path).Replace('\\', '/'));

        Assert.Equal(["Source.cs"], relative);
    }

    [Fact]
    public void DisabledDefaultsUseOnlySortedInRootExplicitCompileItems()
    {
        using var fixture = new SyntheticArchitectureProject();
        fixture.WriteSource("A.cs");
        fixture.WriteSource("B.cs");
        fixture.WriteSource("Ignored.cs");
        fixture.WriteProject("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
              <ItemGroup>
                <Compile Include="B.cs" Link="Linked/B.cs" />
                <Compile Include="A.cs" />
              </ItemGroup>
            </Project>
            """);

        var relative = VerificationArchitectureBoundaryValidator.GetCompileSourceFiles(fixture.ProjectPath)
            .Select(path => Path.GetRelativePath(fixture.ProjectRoot, path).Replace('\\', '/'));

        Assert.Equal(["A.cs", "B.cs"], relative);
    }

    [Theory]
    [InlineData(FileAttributes.ReparsePoint)]
    [InlineData(FileAttributes.Directory | FileAttributes.ReparsePoint)]
    public void ReparseCompileFileOrDirectoryIsRejected(FileAttributes attributes)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-boundary-root"));
        var candidate = Path.Combine(root, "Candidate.cs");

        Assert.Throws<InvalidOperationException>(() =>
            VerificationArchitectureBoundaryValidator.RejectUnsafeSourcePath(root, candidate, attributes));
    }

    [Fact]
    public void DecodedCatalogPowerShellEscapeIsRejected()
    {
        const string text = """
            {"activation_state":"plan-only-foundation","arguments":["power\u0073hell"]}
            """;
        using var document = JsonDocument.Parse(text);

        Assert.NotEmpty(VerificationArchitectureBoundaryValidator.FindForbiddenCatalogStrings(
            document.RootElement,
            text));
    }

    [Fact]
    public void EvidenceKeyPrefixDoesNotSatisfyCanonicalFalseClaim()
    {
        var evidence = ValidEvidence().Replace(
            "- `host_mutation_performed=false`",
            "- `not_host_mutation_performed=false`",
            StringComparison.Ordinal);

        Assert.NotEmpty(VerificationArchitectureBoundaryValidator.FindEvidenceClaimViolations(evidence));
    }

    [Fact]
    public void DuplicateCanonicalFalseClaimIsRejected()
    {
        var evidence = ValidEvidence() + "\n- `host_mutation_performed=false`";

        Assert.NotEmpty(VerificationArchitectureBoundaryValidator.FindEvidenceClaimViolations(evidence));
    }

    [Fact]
    public void TrueClaimIsRejectedEvenWithCanonicalFalseClaim()
    {
        var evidence = ValidEvidence() + "\nobserved host_mutation_performed=true";

        Assert.NotEmpty(VerificationArchitectureBoundaryValidator.FindEvidenceClaimViolations(evidence));
    }

    private static string ValidEvidence() => string.Join(
        "\n",
        VerificationArchitectureBoundaryValidator.RequiredFalseClaimKeys
            .Select(key => $"- `{key}=false`"));

    private static void AssertStaticProjectRejected(Action action)
    {
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.True(
            exception.Message.Contains(
                "unsupported-unevaluated-build-graph",
                StringComparison.Ordinal),
            exception.Message);
    }
}

internal static class VerificationArchitectureBoundaryValidator
{
    private const string SupportedProjectSdk = "Microsoft.NET.Sdk";

    private static readonly HashSet<string> SupportedPropertyNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "OutputType",
        "TargetFramework",
        "AssemblyName",
        "ImplicitUsings",
        "Nullable",
        "EnableDefaultCompileItems",
    };

    private static readonly HashSet<string> AutomaticBuildInputFileNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "Directory.Build.props",
        "Directory.Build.targets",
        "Directory.Packages.props",
    };

    private static readonly string[] ForbiddenDependencyTokens =
    [
        "System.Management.Automation",
        "Microsoft.PowerShell",
        ".ps1",
        "PowerShell",
    ];

    private static readonly string[] ForbiddenCatalogTokens =
    [
        "pwsh",
        "powershell",
        "Invoke-Pester",
        ".ps1",
    ];

    internal static readonly IReadOnlyList<string> RequiredFalseClaimKeys =
    [
        "host_mutation_performed",
        "msi_or_service_mutation",
        "actual_vm_tested",
        "required_ci_pester_zero",
        "required_ci_nonadmin_powershell_zero",
        "cutover_completed",
        "public_trusted_signing",
        "external_stable_publication",
    ];

    internal static IReadOnlyList<string> FindProjectDependencyViolations(XDocument project)
    {
        var violations = new List<string>();
        foreach (var dependency in project.Descendants().Where(element =>
            NameEquals(element.Name, "ProjectReference") ||
            NameEquals(element.Name, "PackageReference") ||
            NameEquals(element.Name, "Reference") ||
            NameEquals(element.Name, "Import")))
        {
            if (NameEquals(dependency.Name, "PackageReference") &&
                string.Equals(dependency.Attribute("Include")?.Value, "YamlDotNet", StringComparison.Ordinal) &&
                string.Equals(dependency.Attribute("Version")?.Value, "18.1.0", StringComparison.Ordinal) &&
                dependency.Attributes().Count() == 2 &&
                !dependency.HasElements &&
                string.IsNullOrWhiteSpace(dependency.Value))
            {
                continue;
            }

            violations.Add(
                $"unsupported-unevaluated-build-graph:{dependency.Name.LocalName}:" +
                dependency.ToString(SaveOptions.DisableFormatting));
        }

        var serialized = project.ToString(SaveOptions.DisableFormatting);
        foreach (var forbidden in ForbiddenDependencyTokens.Where(token =>
            serialized.Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            violations.Add($"forbidden-dependency-token:{forbidden}");
        }

        return violations;
    }

    internal static IReadOnlyList<string> GetCompileSourceFiles(
        string projectPath,
        string? repositoryRoot = null)
    {
        var fullProjectPath = Path.GetFullPath(projectPath);
        if (!File.Exists(fullProjectPath))
        {
            throw InvalidSourcePath("project-file-missing", fullProjectPath);
        }

        var projectRoot = Path.GetDirectoryName(fullProjectPath) ??
            throw InvalidSourcePath("project-root-missing", fullProjectPath);
        projectRoot = Path.GetFullPath(projectRoot);
        var buildInputRoot = Path.GetFullPath(repositoryRoot ?? projectRoot);
        if (!Directory.Exists(buildInputRoot))
        {
            throw InvalidSourcePath("build-input-root-missing", buildInputRoot);
        }

        if (!IsWithinOrEqual(buildInputRoot, projectRoot))
        {
            throw InvalidSourcePath("project-outside-build-input-root", projectRoot);
        }

        RejectUnsafeSourcePath(buildInputRoot, projectRoot, File.GetAttributes(projectRoot));
        RejectUnsafeSourcePath(projectRoot, projectRoot, File.GetAttributes(projectRoot));
        RejectUnsafeSourcePath(projectRoot, fullProjectPath, File.GetAttributes(fullProjectPath));
        RejectImplicitBuildInputs(projectRoot);

        var project = XDocument.Load(fullProjectPath);
        RejectUnsupportedUnevaluatedBuildGraph(project);
        var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (DefaultCompileItemsAreEnabled(project))
        {
            AddDefaultCompileSources(projectRoot, sources);
        }

        foreach (var compile in project.Descendants()
            .Where(element => NameEquals(element.Name, "Compile")))
        {
            var includes = compile.Attributes()
                .Where(attribute => NameEquals(attribute.Name, "Include"))
                .Select(attribute => attribute.Value)
                .ToArray();
            if (includes.Length != 1)
            {
                throw InvalidSourcePath(
                    "unsupported-unevaluated-build-graph-compile-include",
                    compile.ToString(SaveOptions.DisableFormatting));
            }

            var include = includes[0];
            var sourcePath = ResolveProjectRelativePath(projectRoot, include, "Compile Include");
            if (!File.Exists(sourcePath))
            {
                throw InvalidSourcePath("explicit-compile-missing", sourcePath);
            }

            RejectExistingPathChainReparsePoints(projectRoot, sourcePath);
            sources.Add(sourcePath);

            var links = compile.Attributes()
                .Where(attribute => NameEquals(attribute.Name, "Link"))
                .Select(attribute => attribute.Value)
                .Concat(compile.Elements()
                    .Where(element => NameEquals(element.Name, "Link"))
                    .Select(element => element.Value));
            foreach (var link in links)
            {
                _ = ResolveProjectRelativePath(projectRoot, link, "Compile Link");
            }
        }

        return sources
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    internal static void RejectUnsafeSourcePath(
        string projectRoot,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRoot = Path.GetFullPath(projectRoot);
        var fullCandidate = Path.GetFullPath(candidatePath);
        if (!IsWithinOrEqual(fullRoot, fullCandidate))
        {
            throw InvalidSourcePath("outside-project-root", fullCandidate);
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw InvalidSourcePath("reparse-point", fullCandidate);
        }
    }

    internal static IReadOnlyList<string> FindForbiddenCatalogStrings(
        JsonElement root,
        string rawText)
    {
        _ = rawText;
        var violations = new List<string>();
        InspectDecodedJson(root, "$", violations);
        return violations;
    }

    internal static IReadOnlyList<string> FindEvidenceClaimViolations(string evidence)
    {
        var violations = new List<string>();
        var lines = evidence.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');

        foreach (var key in RequiredFalseClaimKeys)
        {
            var canonical = $"- `{key}=false`";
            var count = lines.Count(line => string.Equals(line, canonical, StringComparison.Ordinal));
            if (count != 1)
            {
                violations.Add($"canonical-count:{key}:{count}");
            }

            if (evidence.Contains($"{key}=true", StringComparison.Ordinal))
            {
                violations.Add($"true-claim:{key}");
            }
        }

        return violations;
    }

    private static bool DefaultCompileItemsAreEnabled(XDocument project)
    {
        var values = project.Descendants()
            .Where(element => NameEquals(element.Name, "EnableDefaultCompileItems"))
            .Select(element => element.Value.Trim())
            .ToArray();
        if (values.Any(value =>
            !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "PCV_VERIFY_CONFIG_INVALID|unsupported-enable-default-compile-items");
        }

        return values.Length == 0 ||
            !string.Equals(values[^1], "false", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddDefaultCompileSources(string projectRoot, ISet<string> sources)
    {
        var pending = new Stack<string>();
        pending.Push(projectRoot);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            RejectUnsafeSourcePath(projectRoot, directory, File.GetAttributes(directory));

            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal))
            {
                RejectUnsafeSourcePath(projectRoot, file, File.GetAttributes(file));
                sources.Add(Path.GetFullPath(file));
            }

            foreach (var child in Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                .OrderByDescending(path => path, StringComparer.Ordinal))
            {
                var name = Path.GetFileName(child);
                if (string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                RejectUnsafeSourcePath(projectRoot, child, File.GetAttributes(child));
                pending.Push(child);
            }
        }
    }

    private static string ResolveProjectRelativePath(
        string projectRoot,
        string itemPath,
        string label)
    {
        if (string.IsNullOrWhiteSpace(itemPath) ||
            Path.IsPathRooted(itemPath) ||
            itemPath.Contains("$(", StringComparison.Ordinal) ||
            itemPath.Contains("%(", StringComparison.Ordinal) ||
            itemPath.Contains("@(", StringComparison.Ordinal) ||
            itemPath.IndexOfAny(['*', '?', ';']) >= 0)
        {
            throw InvalidSourcePath($"unsafe-{label.ToLowerInvariant().Replace(' ', '-')}", itemPath);
        }

        var candidate = Path.GetFullPath(Path.Combine(projectRoot, itemPath));
        RejectUnsafeSourcePath(projectRoot, candidate, FileAttributes.Normal);
        return candidate;
    }

    private static void RejectExistingPathChainReparsePoints(string projectRoot, string sourcePath)
    {
        var sourceDirectory = Path.GetDirectoryName(sourcePath) ??
            throw InvalidSourcePath("source-directory-missing", sourcePath);
        var relativeDirectory = Path.GetRelativePath(projectRoot, sourceDirectory);
        var current = projectRoot;
        RejectUnsafeSourcePath(projectRoot, current, File.GetAttributes(current));
        if (relativeDirectory != ".")
        {
            foreach (var segment in relativeDirectory.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries))
            {
                current = Path.Combine(current, segment);
                if (!Directory.Exists(current))
                {
                    throw InvalidSourcePath("source-directory-missing", current);
                }

                RejectUnsafeSourcePath(projectRoot, current, File.GetAttributes(current));
            }
        }

        RejectUnsafeSourcePath(projectRoot, sourcePath, File.GetAttributes(sourcePath));
    }

    private static bool IsWithinOrEqual(string projectRoot, string candidatePath)
    {
        if (string.Equals(projectRoot, candidatePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var prefix = Path.TrimEndingDirectorySeparator(projectRoot) + Path.DirectorySeparatorChar;
        return candidatePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static void RejectImplicitBuildInputs(string projectRoot)
    {
        var current = projectRoot;
        while (true)
        {
            var currentAttributes = File.GetAttributes(current);
            if ((currentAttributes & FileAttributes.ReparsePoint) != 0)
            {
                throw InvalidSourcePath(
                    "unsupported-unevaluated-build-graph-ancestor-reparse-point",
                    current);
            }

            foreach (var path in Directory.EnumerateFiles(current, "*", SearchOption.TopDirectoryOnly)
                .Where(path => AutomaticBuildInputFileNames.Contains(Path.GetFileName(path))))
            {
                throw InvalidSourcePath(
                    "unsupported-unevaluated-build-graph-implicit-import",
                    Path.GetFullPath(path));
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (parent is null)
            {
                return;
            }

            current = Path.GetFullPath(parent);
        }
    }

    private static void RejectUnsupportedUnevaluatedBuildGraph(XDocument project)
    {
        var root = project.Root;
        if (root is null || !IsUnqualifiedName(root.Name, "Project"))
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-root",
                root?.ToString(SaveOptions.DisableFormatting) ?? "<missing>");
        }

        RejectUnsupportedContainerNodes(root, "root");
        var rootAttributes = root.Attributes().ToArray();
        if (rootAttributes.Length != 1 ||
            !IsUnqualifiedName(rootAttributes[0].Name, "Sdk") ||
            !string.Equals(rootAttributes[0].Value, SupportedProjectSdk, StringComparison.Ordinal))
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-sdk",
                root.ToString(SaveOptions.DisableFormatting));
        }

        foreach (var group in root.Elements())
        {
            if (IsUnqualifiedName(group.Name, "Import"))
            {
                throw InvalidSourcePath(
                    "unsupported-unevaluated-build-graph-import",
                    group.ToString(SaveOptions.DisableFormatting));
            }

            if (IsUnqualifiedName(group.Name, "PropertyGroup"))
            {
                ValidateStaticPropertyGroup(group);
                continue;
            }

            if (IsUnqualifiedName(group.Name, "ItemGroup"))
            {
                ValidateStaticItemGroup(group);
                continue;
            }

            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-root-child",
                group.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static void ValidateStaticPropertyGroup(XElement group)
    {
        if (group.HasAttributes)
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-property-group-attribute",
                group.ToString(SaveOptions.DisableFormatting));
        }

        RejectUnsupportedContainerNodes(group, "property-group");
        var observed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in group.Elements())
        {
            if (property.Name.NamespaceName.Length != 0 ||
                !SupportedPropertyNames.Contains(property.Name.LocalName) ||
                !observed.Add(property.Name.LocalName))
            {
                throw InvalidSourcePath(
                    "unsupported-unevaluated-build-graph-static-property",
                    property.ToString(SaveOptions.DisableFormatting));
            }

            RejectStaticLeafShape(property, "property");
            RejectNonLiteralBuildValue(property.Value, "property", property);
        }
    }

    private static void ValidateStaticItemGroup(XElement group)
    {
        if (group.HasAttributes)
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-item-group-attribute",
                group.ToString(SaveOptions.DisableFormatting));
        }

        RejectUnsupportedContainerNodes(group, "item-group");
        foreach (var item in group.Elements())
        {
            if (IsUnqualifiedName(item.Name, "Compile"))
            {
                ValidateStaticCompileItem(item);
                continue;
            }

            if (IsUnqualifiedName(item.Name, "InternalsVisibleTo"))
            {
                ValidateStaticInternalsVisibleToItem(item);
                continue;
            }

            if (IsUnqualifiedName(item.Name, "PackageReference") &&
                string.Equals(item.Attribute("Include")?.Value, "YamlDotNet", StringComparison.Ordinal) &&
                string.Equals(item.Attribute("Version")?.Value, "18.1.0", StringComparison.Ordinal) &&
                item.Attributes().Count() == 2 &&
                !item.HasElements &&
                string.IsNullOrWhiteSpace(item.Value))
            {
                continue;
            }

            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-item",
                item.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static void ValidateStaticCompileItem(XElement compile)
    {
        RejectUnsupportedContainerNodes(compile, "compile");
        var includeAttributes = compile.Attributes()
            .Where(attribute => IsUnqualifiedName(attribute.Name, "Include"))
            .ToArray();
        var linkAttributes = compile.Attributes()
            .Where(attribute => IsUnqualifiedName(attribute.Name, "Link"))
            .ToArray();
        var linkElements = compile.Elements()
            .Where(element => IsUnqualifiedName(element.Name, "Link"))
            .ToArray();
        if (includeAttributes.Length != 1 ||
            linkAttributes.Length + linkElements.Length > 1 ||
            compile.Attributes().Count() != includeAttributes.Length + linkAttributes.Length ||
            compile.Elements().Count() != linkElements.Length)
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-compile-shape",
                compile.ToString(SaveOptions.DisableFormatting));
        }

        RejectNonLiteralBuildValue(includeAttributes[0].Value, "compile-include", compile);
        foreach (var link in linkAttributes.Select(attribute => attribute.Value))
        {
            RejectNonLiteralBuildValue(link, "compile-link", compile);
        }

        foreach (var link in linkElements)
        {
            RejectStaticLeafShape(link, "compile-link");
            RejectNonLiteralBuildValue(link.Value, "compile-link", link);
        }
    }

    private static void ValidateStaticInternalsVisibleToItem(XElement item)
    {
        var includeAttributes = item.Attributes()
            .Where(attribute => IsUnqualifiedName(attribute.Name, "Include"))
            .ToArray();
        if (includeAttributes.Length != 1 || item.Attributes().Count() != 1 ||
            item.HasElements || item.Nodes().Any())
        {
            throw InvalidSourcePath(
                "unsupported-unevaluated-build-graph-static-internals-visible-to-shape",
                item.ToString(SaveOptions.DisableFormatting));
        }

        RejectNonLiteralBuildValue(
            includeAttributes[0].Value,
            "internals-visible-to-include",
            item);
    }

    private static void RejectStaticLeafShape(XElement element, string label)
    {
        if (element.HasAttributes || element.HasElements ||
            element.Nodes().Any(node => node.GetType() != typeof(XText)))
        {
            throw InvalidSourcePath(
                $"unsupported-unevaluated-build-graph-static-{label}-shape",
                element.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static void RejectUnsupportedContainerNodes(XElement element, string label)
    {
        var unsupported = element.Nodes().FirstOrDefault(node =>
            node is not XElement &&
            node is not XComment &&
            (node is not XText text || !string.IsNullOrWhiteSpace(text.Value)));
        if (unsupported is not null)
        {
            throw InvalidSourcePath(
                $"unsupported-unevaluated-build-graph-static-{label}-node",
                unsupported.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static void RejectNonLiteralBuildValue(
        string value,
        string label,
        XElement owner)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Contains("$(", StringComparison.Ordinal) ||
            value.Contains("%(", StringComparison.Ordinal) ||
            value.Contains("@(", StringComparison.Ordinal) ||
            value.IndexOfAny(['*', '?', ';']) >= 0)
        {
            throw InvalidSourcePath(
                $"unsupported-unevaluated-build-graph-static-{label}-literal",
                owner.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static bool IsUnqualifiedName(XName name, string expected) =>
        name.NamespaceName.Length == 0 && NameEquals(name, expected);

    private static bool NameEquals(XName name, string expected) =>
        string.Equals(name.LocalName, expected, StringComparison.OrdinalIgnoreCase);

    private static void InspectDecodedJson(
        JsonElement element,
        string path,
        ICollection<string> violations)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    InspectDecodedString(property.Name, $"{path}.<name>", violations);
                    InspectDecodedJson(property.Value, $"{path}.{property.Name}", violations);
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    InspectDecodedJson(item, $"{path}[{index}]", violations);
                    index++;
                }
                break;
            case JsonValueKind.String:
                InspectDecodedString(element.GetString() ?? string.Empty, path, violations);
                break;
        }
    }

    private static void InspectDecodedString(
        string value,
        string path,
        ICollection<string> violations)
    {
        var forbidden = ForbiddenCatalogTokens.FirstOrDefault(token =>
            value.Contains(token, StringComparison.OrdinalIgnoreCase));
        if (forbidden is not null)
        {
            violations.Add($"{path}:{forbidden}");
        }
    }

    private static InvalidOperationException InvalidSourcePath(string reason, string path) =>
        new($"PCV_VERIFY_CONFIG_INVALID|compile-source:{reason}:{path}");
}

internal sealed class SyntheticArchitectureProject : IDisposable
{
    private readonly string ownedRoot = Path.Combine(
        Path.GetTempPath(),
        $"pcv-verification-architecture-{Guid.NewGuid():N}");

    internal SyntheticArchitectureProject()
    {
        ProjectRoot = Directory.CreateDirectory(Path.Combine(ownedRoot, "project")).FullName;
    }

    internal string ProjectRoot { get; }
    internal string OwnedRoot => ownedRoot;
    internal string ProjectPath => Path.Combine(ProjectRoot, "Synthetic.csproj");
    internal string OutsideSourcePath => Path.Combine(ownedRoot, "Outside.cs");

    internal void WriteProject(string contents) => File.WriteAllText(ProjectPath, contents);
    internal void WriteProject(XDocument document) =>
        WriteProject(document.ToString(SaveOptions.DisableFormatting));
    internal void WriteOutsideSource() => File.WriteAllText(OutsideSourcePath, "internal sealed class Outside;");

    internal void WriteSource(string relativePath)
    {
        var path = Path.Combine(ProjectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "internal sealed class Source;");
    }

    public void Dispose()
    {
        if (Directory.Exists(ownedRoot))
        {
            Directory.Delete(ownedRoot, recursive: true);
        }
    }
}

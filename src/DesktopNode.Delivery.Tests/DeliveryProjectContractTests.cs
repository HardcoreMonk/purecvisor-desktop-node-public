using System.Xml.Linq;

namespace DesktopNode.Delivery.Tests;

[Trait("Category", "VerificationInfrastructure")]
public sealed class DeliveryProjectContractTests
{
    [Fact]
    public void ProjectIsAnIsolatedNet10XunitTestAssembly()
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(
            root,
            "src",
            "DesktopNode.Delivery.Tests",
            "DesktopNode.Delivery.Tests.csproj");
        var project = XDocument.Load(projectPath);

        AssertSingleProperty(project, "TargetFramework", "net10.0");
        Assert.Empty(project.Root?.Elements("PropertyGroup").Elements("OutputType") ?? []);
        Assert.Empty(project.Root?.Elements("ItemGroup").Elements("ProjectReference") ?? []);

        var expectedPackages = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["coverlet.collector"] = "6.0.4",
            ["Microsoft.NET.Test.Sdk"] = "17.14.1",
            ["xunit"] = "2.9.3",
            ["xunit.runner.visualstudio"] = "3.1.4",
        };
        var packages = project.Root?.Elements("ItemGroup").Elements("PackageReference")
            .Where(element => element.Attribute("Include") is not null)
            .ToList() ?? [];

        Assert.Equal(expectedPackages.Count, packages.Count);
        foreach (var expected in expectedPackages)
        {
            var matches = packages
                .Where(element => element.Attribute("Include")?.Value == expected.Key)
                .ToList();
            Assert.Single(matches);
            Assert.Equal(expected.Value, matches[0].Attribute("Version")?.Value);
        }
    }

    [Fact]
    public void SolutionContainsDeliveryProjectExactlyOnce()
    {
        var root = FindRepositoryRoot();
        var solution = File.ReadAllText(Path.Combine(root, "src", "DesktopNode.sln"));

        Assert.Equal(
            1,
            CountOccurrences(
                solution,
                "DesktopNode.Delivery.Tests\\DesktopNode.Delivery.Tests.csproj"));
    }

    private static void AssertSingleProperty(
        XDocument project,
        string propertyName,
        string expectedValue)
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

        throw new InvalidOperationException("PCV_DELIVERY_CONFIG_INVALID|repository-root-not-found");
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

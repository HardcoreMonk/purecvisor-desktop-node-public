using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

// ADR-0016: the Hyper-V adapter integration tier creates real VMs, so it must stay out of the solution, the Required CI
// suites and the workflows, and must keep its approval guard.
public sealed class HyperVIntegrationTierContractTests
{
    private const string ProjectName = "DesktopNode.HyperV.IntegrationTests";
    private static readonly string Root = RepositoryContractContext.Find().RootPath;

    [Fact]
    public void IntegrationProjectStaysOutOfTheSolutionAndRequiredCi()
    {
        Assert.True(File.Exists(Full($"src/{ProjectName}/{ProjectName}.csproj")));
        Assert.DoesNotContain(ProjectName, File.ReadAllText(Full("src/DesktopNode.sln")), StringComparison.Ordinal);
        Assert.DoesNotContain(ProjectName, File.ReadAllText(Full("config/development-verification-suites.json")), StringComparison.Ordinal);
        foreach (var workflow in Directory.GetFiles(Full(".github/workflows"), "*.yml"))
        {
            Assert.DoesNotContain(ProjectName, File.ReadAllText(workflow), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void IntegrationFixtureKeepsTheAdr0016Guard()
    {
        var fixture = File.ReadAllText(Full($"src/{ProjectName}/HyperVIntegrationFixture.cs"));

        Assert.Contains("PCV_HYPERV_INTEGRATION_APPROVAL", fixture, StringComparison.Ordinal);
        Assert.Contains("approval_locator", fixture, StringComparison.Ordinal);
        Assert.Contains("NamePrefix = \"pcv-it-\"", fixture, StringComparison.Ordinal);
        Assert.Contains("\"artifacts\", \"hyperv-integration\"", fixture, StringComparison.Ordinal);
        Assert.Contains("RequirePrivileges();", fixture, StringComparison.Ordinal);
        Assert.Contains("RequireDefaultSwitch();", fixture, StringComparison.Ordinal);
    }

    private static string Full(string relativePath) =>
        Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
}

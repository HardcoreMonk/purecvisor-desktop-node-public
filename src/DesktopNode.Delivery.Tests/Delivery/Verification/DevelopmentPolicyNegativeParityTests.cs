using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "VerificationInfrastructure")]
public sealed class DevelopmentPolicyNegativeParityTests
{
    private static readonly RepositoryContractContext Repository =
        RepositoryContractContext.Find();

    [Fact]
    public void RejectsAWeakenedWorkflowCancellationPolicy()
    {
        var workflow = Repository.ReadUtf8Text(".github/workflows/development-gates.yml")
            .Replace("cancel-in-progress: true", "cancel-in-progress: false", StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateWorkflowText(workflow));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|workflow-required",
            error.Message);
    }

    [Fact]
    public void AcceptsTheShellFreeActiveWorkflowIdentity()
    {
        DevelopmentPolicyContractVerifier.ValidateWorkflowText(ActiveWorkflow());
    }

    [Fact]
    public void RejectsPesterReintroducedIntoTheActiveWorkflow()
    {
        var workflow = ActiveWorkflow().Replace(
            "Run web shard",
            "Run web shard\n        run: Invoke-Pester",
            StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateWorkflowText(workflow));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|workflow-active-shell",
            error.Message);
    }

    [Fact]
    public void RejectsAForbiddenSuiteExecutable()
    {
        var suites = CanonicalSuites();
        suites[0] = suites[0] with { FileName = "pwsh" };

        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateSuiteCatalog(suites, AllowedExecutables()));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|suite-executable-forbidden",
            error.Message);
    }

    [Fact]
    public void RejectsAWidenedModuleThreshold()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateModuleRatchet(
                actualLines: 500,
                proposedMaximum: 600,
                recordedMaximum: 550,
                slackLines: 50));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|module-ratchet-widened",
            error.Message);
    }

    [Fact]
    public void RejectsAnUnknownQualityToolVersion()
    {
        var versions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["dotnet_sdk_version"] = "10.0.100",
            ["coverage_collector_version"] = "6.0.4",
            ["unknown_tool_version"] = "1.0.0",
        };

        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateToolVersions(versions));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|quality-tool-version",
            error.Message);
    }

    [Fact]
    public void RejectsADuplicateSuiteIdentity()
    {
        var suites = CanonicalSuites();
        suites[1] = suites[0];

        var error = Assert.Throws<InvalidDataException>(() =>
            DevelopmentPolicyContractVerifier.ValidateSuiteCatalog(suites, AllowedExecutables()));

        Assert.Equal(
            "PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|suite-identity",
            error.Message);
    }

    private static DevelopmentSuiteContract[] CanonicalSuites() =>
    [
        new("dotnet", "process", "dotnet", 900),
        new("web-typecheck", "process", "npm", 600),
        new("web-parity", "process", "npm", 600),
        new("delivery-contracts", "process", "dotnet", 900),
        new("installer-contracts", "process", "dotnet", 900),
        new("evidence-check", "managed", null, 300),
        new("policy-boundaries", "managed", null, 300),
    ];

    private static string[] AllowedExecutables() =>
    [
        "dotnet",
        "dotnet.exe",
        "node",
        "node.exe",
        "npm",
        "npm.cmd",
        "git",
        "git.exe",
    ];

    private static string ActiveWorkflow() =>
        """
        name: Development Gates
        on:
          pull_request:
          push:
          workflow_dispatch:
        permissions:
          contents: read
        concurrency:
          cancel-in-progress: true
        jobs:
          dotnet:
            runs-on: windows-latest
            timeout-minutes: 15
            steps:
              - name: dotnet-version: 10.0.x
              - name: Run dotnet shard
          web:
            runs-on: ubuntu-latest
            timeout-minutes: 15
            steps:
              - name: node-version: 24
              - name: Run web shard
          delivery:
            runs-on: windows-latest
            timeout-minutes: 15
            steps:
              - name: Run delivery shard
          installer-policy:
            runs-on: windows-latest
            timeout-minutes: 15
            steps:
              - name: Run installer and policy shard
        """;
}

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "VerificationInfrastructure")]
public sealed class OrchestrationNegativeParityTests
{
    [Fact]
    public void RejectsTwoTerminalRowsForOneStep()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            OrchestrationContractVerifier.ValidateTerminalRows(
            [
                new("build", "started"),
                new("build", "failed"),
                new("build", "completed"),
            ]));

        Assert.Equal(
            "PCV_DELIVERY_ORCHESTRATION_INVALID|terminal-cardinality",
            error.Message);
    }

    [Fact]
    public void RejectsATimeoutOverflow()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            OrchestrationContractVerifier.ValidateTimeoutPolicy(
                timeoutSeconds: 3601,
                retryCount: 1,
                maximumTimeoutSeconds: 3600));

        Assert.Equal("PCV_DELIVERY_ORCHESTRATION_INVALID|timeout-policy", error.Message);
    }

    [Fact]
    public void RejectsAnEscapingArtifactPath()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            OrchestrationContractVerifier.ValidateArtifactPath(
                @"C:\repo\artifacts",
                @"C:\repo\outside\summary.json"));

        Assert.Equal("PCV_DELIVERY_ORCHESTRATION_INVALID|artifact-path", error.Message);
    }

    [Fact]
    public void RejectsADuplicateCiTrigger()
    {
        const string workflow = """
            on:
              pull_request:
              push:
                branches:
                  - main
              workflow_dispatch:
              workflow_dispatch:
            """;

        var error = Assert.Throws<InvalidDataException>(() =>
            OrchestrationContractVerifier.ValidateCiTriggers(workflow));

        Assert.Equal(
            "PCV_DELIVERY_ORCHESTRATION_INVALID|ci-trigger-cardinality",
            error.Message);
    }

    [Fact]
    public void RejectsAMutationEnabledPlan()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            OrchestrationContractVerifier.ValidatePlanDescriptor(
                new OrchestrationPlanContract(
                    PlanOnly: true,
                    HostMutationPerformed: false,
                    MutatesHost: true,
                    Operations: ["inspect"])));

        Assert.Equal("PCV_DELIVERY_ORCHESTRATION_INVALID|plan-boundary", error.Message);
    }
}

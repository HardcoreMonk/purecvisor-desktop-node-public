namespace DesktopNode.Service;

public sealed record ServiceLifecycleAdapterAction(
    string Name,
    string Stance,
    string TokenSourceStance,
    bool RequiresExplicitOptIn);

public sealed record ServiceLifecycleAdapterContract(
    string Owner,
    string DefaultOwner,
    string DefaultActionStance,
    string CandidateStance,
    string TokenSourceStance,
    IReadOnlyList<ServiceLifecycleAdapterAction> Actions)
{
    public static ServiceLifecycleAdapterContract CreateDefault()
    {
        return new ServiceLifecycleAdapterContract(
            Owner: "dotnet-service-action-runner",
            DefaultOwner: "dotnet-windows-service-host",
            DefaultActionStance: "scm-dotnet-host",
            CandidateStance: "default-product-path",
            TokenSourceStance: "protected-token-file-preferred",
            Actions:
            [
                CreateDelegatedAction("install"),
                CreateDelegatedAction("configure"),
                CreateDelegatedAction("repair"),
                CreateDelegatedAction("start", requiresExplicitOptIn: true),
                CreateDelegatedAction("stop", requiresExplicitOptIn: true),
                CreateDelegatedAction("status"),
                CreateDelegatedAction("remove"),
                new ServiceLifecycleAdapterAction(
                    Name: "remove-data",
                    Stance: "scm-dotnet-host",
                    TokenSourceStance: "protected-token-file-required",
                    RequiresExplicitOptIn: true)
            ]);
    }

    private static ServiceLifecycleAdapterAction CreateDelegatedAction(string name, bool requiresExplicitOptIn = false)
    {
        return new ServiceLifecycleAdapterAction(
            Name: name,
            Stance: "scm-dotnet-host",
            TokenSourceStance: "protected-token-file-preferred",
            RequiresExplicitOptIn: requiresExplicitOptIn);
    }
}

namespace DesktopNode.Service;

public sealed record ServiceHostCandidateContract(
    string ServiceHostReplacementStance,
    string ProductServiceReplacementStance,
    string Owner,
    string DefaultOwner,
    string DefaultProductWrapperPath,
    IReadOnlyList<string> AllowedLaunchModes,
    string TokenSourceStance)
{
    public static ServiceHostCandidateContract CreateDefault()
    {
        return new ServiceHostCandidateContract(
            ServiceHostReplacementStance: "default",
            ProductServiceReplacementStance: "replaces-winsw",
            Owner: "dotnet-windows-service-host",
            DefaultOwner: "dotnet-windows-service-host",
            DefaultProductWrapperPath: "packaging/windows-desktop-node",
            AllowedLaunchModes:
            [
                "console-listen",
                "windows-service"
            ],
            TokenSourceStance: "protected-token-file-preferred");
    }
}

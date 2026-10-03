using DesktopNode.HyperV;

namespace DesktopNode.Host;

public sealed record DesktopNodeHyperVSwitchMutationSnapshot(
    string Name,
    bool Exists,
    string? Type,
    int AttachedVmCount,
    bool ProductOwned,
    bool AllowManagementOs);

public interface IDesktopNodeHyperVSwitchController
{
    DesktopNodeHyperVSwitchMutationSnapshot Query(string switchName, CancellationToken cancellationToken = default);

    DesktopNodeHyperVSwitchMutationSnapshot Create(
        string switchName,
        string switchType,
        bool allowManagementOs,
        CancellationToken cancellationToken = default);

    DesktopNodeHyperVSwitchMutationSnapshot Remove(string switchName, CancellationToken cancellationToken = default);
}

internal sealed class DesktopNodeHyperVSwitchController : IDesktopNodeHyperVSwitchController
{
    private readonly IDesktopNodeHyperVSwitchMutationProvider provider;

    public DesktopNodeHyperVSwitchController()
        : this(new DesktopNodeHyperVWmiSwitchMutationProvider())
    {
    }

    public DesktopNodeHyperVSwitchController(IDesktopNodeHyperVSwitchMutationProvider provider)
    {
        this.provider = provider;
    }

    public DesktopNodeHyperVSwitchMutationSnapshot Query(string switchName, CancellationToken cancellationToken = default)
    {
        return Map(provider.Query(switchName, cancellationToken));
    }

    public DesktopNodeHyperVSwitchMutationSnapshot Create(
        string switchName,
        string switchType,
        bool allowManagementOs,
        CancellationToken cancellationToken = default)
    {
        return Map(provider.Create(switchName, switchType, allowManagementOs, cancellationToken));
    }

    public DesktopNodeHyperVSwitchMutationSnapshot Remove(string switchName, CancellationToken cancellationToken = default)
    {
        return Map(provider.Remove(switchName, cancellationToken));
    }

    private static DesktopNodeHyperVSwitchMutationSnapshot Map(DesktopNodeHyperVSwitchMutationInfo info)
    {
        return new DesktopNodeHyperVSwitchMutationSnapshot(
            info.Name,
            info.Exists,
            info.Type,
            info.AttachedVmCount,
            info.ProductOwned,
            info.AllowManagementOs);
    }
}

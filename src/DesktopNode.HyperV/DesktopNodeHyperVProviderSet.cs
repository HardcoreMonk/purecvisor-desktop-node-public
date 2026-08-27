namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVProviderSet
{
    public DesktopNodeHyperVProviderSet(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmManageProvider vmManageProvider,
        IDesktopNodeHyperVVmCloneProvider vmCloneProvider,
        IDesktopNodeHyperVVmMediaProvider vmMediaProvider,
        IDesktopNodeHyperVVmResourceMutationProvider vmResourceMutationProvider,
        IDesktopNodeHyperVGuestExecutionProvider guestExecutionProvider)
    {
        ArgumentNullException.ThrowIfNull(switchProvider);
        ArgumentNullException.ThrowIfNull(hostStatusProvider);
        ArgumentNullException.ThrowIfNull(vmProvider);
        ArgumentNullException.ThrowIfNull(checkpointProvider);
        ArgumentNullException.ThrowIfNull(checkpointMutationProvider);
        ArgumentNullException.ThrowIfNull(vmPowerStateProvider);
        ArgumentNullException.ThrowIfNull(vmCreateProvider);
        ArgumentNullException.ThrowIfNull(vmDeleteProvider);
        ArgumentNullException.ThrowIfNull(vmRenameProvider);
        ArgumentNullException.ThrowIfNull(vmManageProvider);
        ArgumentNullException.ThrowIfNull(vmCloneProvider);
        ArgumentNullException.ThrowIfNull(vmMediaProvider);
        ArgumentNullException.ThrowIfNull(vmResourceMutationProvider);
        ArgumentNullException.ThrowIfNull(guestExecutionProvider);

        SwitchProvider = switchProvider;
        HostStatusProvider = hostStatusProvider;
        VmProvider = vmProvider;
        CheckpointProvider = checkpointProvider;
        CheckpointMutationProvider = checkpointMutationProvider;
        VmPowerStateProvider = vmPowerStateProvider;
        VmCreateProvider = vmCreateProvider;
        VmDeleteProvider = vmDeleteProvider;
        VmRenameProvider = vmRenameProvider;
        VmManageProvider = vmManageProvider;
        VmCloneProvider = vmCloneProvider;
        VmMediaProvider = vmMediaProvider;
        VmResourceMutationProvider = vmResourceMutationProvider;
        GuestExecutionProvider = guestExecutionProvider;
    }

    public IDesktopNodeHyperVSwitchProvider SwitchProvider { get; }

    public IDesktopNodeHyperVHostStatusProvider HostStatusProvider { get; }

    public IDesktopNodeHyperVVmProvider VmProvider { get; }

    public IDesktopNodeHyperVCheckpointProvider CheckpointProvider { get; }

    public IDesktopNodeHyperVCheckpointMutationProvider CheckpointMutationProvider { get; }

    public IDesktopNodeHyperVVmPowerStateProvider VmPowerStateProvider { get; }

    public IDesktopNodeHyperVVmCreateProvider VmCreateProvider { get; }

    public IDesktopNodeHyperVVmDeleteProvider VmDeleteProvider { get; }

    public IDesktopNodeHyperVVmRenameProvider VmRenameProvider { get; }

    public IDesktopNodeHyperVVmManageProvider VmManageProvider { get; }

    public IDesktopNodeHyperVVmCloneProvider VmCloneProvider { get; }

    public IDesktopNodeHyperVVmMediaProvider VmMediaProvider { get; }

    public IDesktopNodeHyperVVmResourceMutationProvider VmResourceMutationProvider { get; }

    public IDesktopNodeHyperVGuestExecutionProvider GuestExecutionProvider { get; }

    public static DesktopNodeHyperVProviderSet CreateDefaultWmi()
    {
        var switchProvider = new DesktopNodeHyperVWmiSwitchProvider();
        return new DesktopNodeHyperVProviderSet(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            new DesktopNodeHyperVWmiVmProvider(),
            new DesktopNodeHyperVWmiCheckpointProvider(),
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider(),
            new DesktopNodeHyperVWmiVmManageProvider(),
            new DesktopNodeHyperVWmiVmCloneProvider(),
            new DesktopNodeHyperVWmiVmMediaProvider(),
            new DesktopNodeHyperVWmiVmResourceMutationProvider(),
            new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider());
    }

    public IReadOnlyDictionary<string, object> ToProviderBoundaryMap()
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["host-status-provider"] = HostStatusProvider,
            ["switch-provider"] = SwitchProvider,
            ["vm-provider"] = VmProvider,
            ["checkpoint-provider"] = CheckpointProvider,
            ["vm-create-provider"] = VmCreateProvider,
            ["vm-power-state-provider"] = VmPowerStateProvider,
            ["vm-delete-provider"] = VmDeleteProvider,
            ["vm-rename-provider"] = VmRenameProvider,
            ["vm-manage-provider"] = VmManageProvider,
            ["vm-clone-provider"] = VmCloneProvider,
            ["vm-media-provider"] = VmMediaProvider,
            ["vm-resource-mutation-provider"] = VmResourceMutationProvider,
            ["guest-execution-provider"] = GuestExecutionProvider,
            ["checkpoint-mutation-provider"] = CheckpointMutationProvider,
        };
    }
}

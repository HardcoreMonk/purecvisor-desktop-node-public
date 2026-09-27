using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    public DesktopNodeHyperVNativeAdapter(DesktopNodeHyperVProviderSet providerSet)
        : this(
            RequireProviderSet(providerSet).SwitchProvider,
            RequireProviderSet(providerSet).HostStatusProvider,
            RequireProviderSet(providerSet).VmProvider,
            RequireProviderSet(providerSet).CheckpointProvider,
            RequireProviderSet(providerSet).CheckpointMutationProvider,
            RequireProviderSet(providerSet).VmPowerStateProvider,
            RequireProviderSet(providerSet).VmCreateProvider,
            RequireProviderSet(providerSet).VmDeleteProvider,
            RequireProviderSet(providerSet).VmRenameProvider,
            RequireProviderSet(providerSet).VmManageProvider,
            RequireProviderSet(providerSet).VmCloneProvider,
            RequireProviderSet(providerSet).VmMediaProvider,
            RequireProviderSet(providerSet).VmResourceMutationProvider,
            RequireProviderSet(providerSet).GuestExecutionProvider,
            RequireProviderSet(providerSet).VmExportProvider,
            RequireProviderSet(providerSet).VmImportProvider,
            RequireProviderSet(providerSet).VmNetworkConnectProvider)
    {
    }

    public DesktopNodeHyperVNativeAdapter(IDesktopNodeHyperVSwitchProvider switchProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            new DesktopNodeHyperVWmiVmProvider(),
            new DesktopNodeHyperVWmiCheckpointProvider(),
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            new DesktopNodeHyperVWmiCheckpointProvider(),
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider)
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmManageProvider vmManageProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            vmManageProvider)
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmManageProvider vmManageProvider,
        IDesktopNodeHyperVVmCloneProvider vmCloneProvider)
        : this(
            switchProvider,
            new DesktopNodeHyperVNativeHostStatusProvider(switchProvider),
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            vmManageProvider,
            vmCloneProvider)
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            new DesktopNodeHyperVWmiVmProvider(),
            new DesktopNodeHyperVWmiCheckpointProvider(),
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            new DesktopNodeHyperVWmiCheckpointProvider(),
            new DesktopNodeHyperVWmiCheckpointMutationProvider(),
            new DesktopNodeHyperVWmiVmPowerStateProvider(),
            new DesktopNodeHyperVWmiVmCreateProvider(),
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            new DesktopNodeHyperVWmiVmDeleteProvider(),
            new DesktopNodeHyperVWmiVmRenameProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            new DesktopNodeHyperVWmiVmManageProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmManageProvider vmManageProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            vmManageProvider,
            new DesktopNodeHyperVWmiVmCloneProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
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
        IDesktopNodeHyperVVmCloneProvider vmCloneProvider)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            vmManageProvider,
            vmCloneProvider,
            new DesktopNodeHyperVWmiVmMediaProvider(),
            new DesktopNodeHyperVWmiVmResourceMutationProvider(),
            new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider())
    {
    }

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmMediaProvider vmMediaProvider,
        IDesktopNodeHyperVVmResourceMutationProvider vmResourceMutationProvider,
        IDesktopNodeHyperVGuestExecutionProvider? guestExecutionProvider = null)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            new DesktopNodeHyperVWmiVmManageProvider(),
            new DesktopNodeHyperVWmiVmCloneProvider(),
            vmMediaProvider,
            vmResourceMutationProvider,
            guestExecutionProvider)
    {
    }

    public DesktopNodeHyperVNativeAdapter(
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
        IDesktopNodeHyperVVmMediaProvider vmMediaProvider,
        IDesktopNodeHyperVVmResourceMutationProvider vmResourceMutationProvider,
        IDesktopNodeHyperVGuestExecutionProvider? guestExecutionProvider = null)
        : this(
            switchProvider,
            hostStatusProvider,
            vmProvider,
            checkpointProvider,
            checkpointMutationProvider,
            vmPowerStateProvider,
            vmCreateProvider,
            vmDeleteProvider,
            vmRenameProvider,
            vmManageProvider,
            new DesktopNodeHyperVWmiVmCloneProvider(),
            vmMediaProvider,
            vmResourceMutationProvider,
            guestExecutionProvider)
    {
    }
}

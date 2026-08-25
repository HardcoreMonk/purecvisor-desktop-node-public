using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVWmiProviderTests
{
    [Fact]
    public void WmiVmProviderQueryAvoidsPowerShellOnlyNotesProjection()
    {
        Assert.Contains("Msvm_ComputerSystem", DesktopNodeHyperVWmiVmProvider.CimQuery);
        Assert.DoesNotContain("Notes", DesktopNodeHyperVWmiVmProvider.CimQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WmiVmProviderQueryAvoidsLocalizedCaptionFilter()
    {
        Assert.StartsWith("SELECT * FROM Msvm_ComputerSystem", DesktopNodeHyperVWmiVmProvider.CimQuery, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Caption", DesktopNodeHyperVWmiVmProvider.CimQuery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Description = 'Microsoft Virtual Machine'", DesktopNodeHyperVWmiVmProvider.CimQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WmiVmProviderMapsSummaryFieldsForNativeParity()
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 2,
            ProcessorCount: 2,
            StartupMemoryQuantity: 4096,
            StartupMemoryQuantityUnits: "byte*2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 1,
            Notes: "managed-by=purecvisor-desktop-node"));

        Assert.Equal("alpha", vm.Id);
        Assert.Equal("alpha", vm.Name);
        Assert.Equal("running", vm.State);
        Assert.Equal(2, vm.Cpu.Count);
        Assert.Equal(4096, vm.Memory.StartupMb);
        Assert.Null(vm.Memory.AssignedMb);
        Assert.False(vm.Memory.Dynamic);
        Assert.Equal(2, vm.Generation);
        Assert.Equal(1, vm.Checkpoints.Count);
        Assert.Equal("vmconnect", vm.Console.Type);
        Assert.True(vm.Console.AvailableLocal);
        Assert.True(vm.ManagedByPurecvisor);
    }

    [Theory]
    [InlineData("guest_family=windows", "windows")]
    [InlineData("guest-family: windows", "windows")]
    [InlineData("os=Microsoft Windows Server 2022 Datacenter Evaluation", "windows")]
    [InlineData("guest_family=linux", "linux")]
    [InlineData("guest_family=linux\ncomment=previously Windows imported", "linux")]
    [InlineData(null, "unknown")]
    public void WmiVmProviderMapsGuestFamilyFromExplicitNotesHint(string? notes, string expectedFamily)
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 2,
            ProcessorCount: 2,
            StartupMemoryQuantity: 4096,
            StartupMemoryQuantityUnits: "byte*2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 1,
            Notes: notes));

        Assert.Equal(expectedFamily, vm.GuestFamily);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(32768)]
    public void WmiVmProviderMapsPausedEnabledStatesForNativeParity(int enabledState)
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: enabledState,
            ProcessorCount: 1,
            StartupMemoryQuantity: 1024,
            StartupMemoryQuantityUnits: "byte*2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 0,
            Notes: null));

        Assert.Equal("paused", vm.State);
    }

    [Fact]
    public void WmiVmProviderMapsSavedEnabledStateForNativeParity()
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 32769,
            ProcessorCount: 1,
            StartupMemoryQuantity: 1024,
            StartupMemoryQuantityUnits: "byte*2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 0,
            Notes: null));

        Assert.Equal("saved", vm.State);
        Assert.Equal("saved", DesktopNodeHyperVWmiCommon.MapEnabledState(32769));
        Assert.Equal("saved", DesktopNodeHyperVWmiCommon.MapEnabledState(6));
    }

    [Fact]
    public void WmiVmProviderDeclinesUnknownSummaryUnits()
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 3,
            ProcessorCount: 1,
            StartupMemoryQuantity: 1024,
            StartupMemoryQuantityUnits: "unknown-unit",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 0,
            Notes: null));

        Assert.Equal("stopped", vm.State);
        Assert.Equal(1, vm.Cpu.Count);
        Assert.Null(vm.Memory.StartupMb);
        Assert.False(vm.ManagedByPurecvisor);
    }

    [Fact]
    public void WmiVmProviderMapsSpacedMemoryUnitsForNativeParity()
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 3,
            ProcessorCount: 1,
            StartupMemoryQuantity: 1024,
            StartupMemoryQuantityUnits: "byte * 2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 0,
            Notes: null));

        Assert.Equal(1024, vm.Memory.StartupMb);
    }

    [Fact]
    public void WmiVmProviderMapsStorageAndNetworkFieldsForNativeParity()
    {
        var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
            Id: "alpha",
            Name: "alpha",
            EnabledState: 2,
            ProcessorCount: 2,
            StartupMemoryQuantity: 4096,
            StartupMemoryQuantityUnits: "byte*2^20",
            GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
            CheckpointCount: 1,
            Notes: "managed-by=purecvisor-desktop-node",
            Storage:
            [
                new DesktopNodeHyperVWmiVmStorageSummary("D:\\VMs\\alpha\\disk0.vhdx", Attached: true)
            ],
            Network:
            [
                new DesktopNodeHyperVWmiVmNetworkSummary("Default Switch")
            ]));

        var disk = Assert.Single(vm.Storage);
        Assert.Equal("vhdx", disk.Kind);
        Assert.Equal("D:\\VMs\\alpha\\disk0.vhdx", disk.Path);
        Assert.Null(disk.SizeGb);
        Assert.True(disk.Attached);

        var network = Assert.Single(vm.Network);
        Assert.Equal("Default Switch", network.Switch);
        Assert.Equal("default-switch", network.Mode);
    }

    [Fact]
    public void WmiVmProviderUsesCurrentSettingAndResourceAssociations()
    {
        Assert.Equal("Msvm_VirtualSystemSettingData", DesktopNodeHyperVWmiVmProvider.VirtualSystemSettingClass);
        Assert.Equal("Msvm_SettingsDefineState", DesktopNodeHyperVWmiVmProvider.SettingsDefineStateAssociationClass);
        Assert.Equal("Msvm_VirtualSystemSettingDataComponent", DesktopNodeHyperVWmiVmProvider.SettingDataComponentAssociationClass);
        Assert.Equal("Msvm_ProcessorSettingData", DesktopNodeHyperVWmiVmProvider.ProcessorSettingClass);
        Assert.Equal("Msvm_MemorySettingData", DesktopNodeHyperVWmiVmProvider.MemorySettingClass);
        Assert.Equal("Msvm_SnapshotOfVirtualSystem", DesktopNodeHyperVWmiVmProvider.SnapshotAssociationClass);
    }

    [Fact]
    public void WmiVmProviderUsesStorageAndNetworkAssociationClasses()
    {
        Assert.Equal("Msvm_StorageAllocationSettingData", DesktopNodeHyperVWmiVmProvider.StorageSettingClass);
        Assert.Equal("Msvm_EthernetPortAllocationSettingData", DesktopNodeHyperVWmiVmProvider.EthernetPortAllocationSettingClass);
    }

    [Fact]
    public void WmiCheckpointProviderUsesSnapshotAssociationClass()
    {
        Assert.Equal("Msvm_SnapshotOfVirtualSystem", DesktopNodeHyperVWmiCheckpointProvider.SnapshotAssociationClass);
        Assert.Equal("Msvm_MostCurrentSnapshotInBranch", DesktopNodeHyperVWmiCheckpointProvider.CurrentSnapshotAssociationClass);
    }

    [Fact]
    public void WmiCheckpointProviderVmQueryAvoidsLocalizedCaptionFilter()
    {
        var query = DesktopNodeHyperVWmiCheckpointProvider.VmQuery;

        Assert.StartsWith("SELECT * FROM Msvm_ComputerSystem", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Caption", query, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Description = 'Microsoft Virtual Machine'", query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WmiSwitchProviderUsesFullProjectionForAssociationTraversal()
    {
        Assert.Equal(
            "SELECT * FROM Msvm_VirtualEthernetSwitch",
            DesktopNodeHyperVWmiSwitchProvider.SwitchQuery);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WmiSwitchProviderRejectsMissingAssociationTraversalPath(string? objectPath)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => DesktopNodeHyperVWmiSwitchProvider.EnsureAssociationTraversalPath(objectPath));

        Assert.Contains("association traversal", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WmiSwitchProviderAcceptsCompleteAssociationTraversalPath()
    {
        DesktopNodeHyperVWmiSwitchProvider.EnsureAssociationTraversalPath(
            @"\\HOST\root\virtualization\v2:Msvm_VirtualEthernetSwitch.Name=""switch-id""");
    }

    [Fact]
    public void WmiSwitchProviderMapsDefaultSwitchAsCompleteInternalTopology()
    {
        var info = DesktopNodeHyperVWmiSwitchProvider.MapSwitch("Default Switch");

        Assert.Equal("Default Switch", info.Name);
        Assert.Equal("internal", info.Type);
        Assert.True(info.IsDefault);
        Assert.True(info.AllowManagementOs);
        Assert.Null(info.NetAdapterInterfaceDescription);
    }

    [Fact]
    public void WmiSwitchProviderMapsProvenInternalSwitchWithoutNameHeuristic()
    {
        var info = DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
            "WSL (Hyper-V firewall)",
            hasInternalManagementPort: true);

        Assert.Equal("WSL (Hyper-V firewall)", info.Name);
        Assert.Equal("internal", info.Type);
        Assert.False(info.IsDefault);
        Assert.True(info.AllowManagementOs);
        Assert.Null(info.NetAdapterInterfaceDescription);
    }

    [Fact]
    public void WmiSwitchProviderDoesNotClassifyExternallyBoundSwitchAsInternal()
    {
        var info = DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
            "corp-uplink",
            hasInternalManagementPort: true,
            hasExternalBinding: true);

        Assert.Equal("corp-uplink", info.Name);
        Assert.Equal("unknown", info.Type);
        Assert.False(info.IsDefault);
        Assert.Null(info.AllowManagementOs);
        Assert.Null(info.NetAdapterInterfaceDescription);
    }

    [Fact]
    public void WmiVmPowerStateProviderUsesRequestStateChangeConstants()
    {
        Assert.Equal("RequestStateChange", DesktopNodeHyperVWmiVmPowerStateProvider.RequestStateChangeMethod);
        Assert.Equal("Msvm_ShutdownComponent", DesktopNodeHyperVWmiVmPowerStateProvider.ShutdownComponentClass);
        Assert.Equal("InitiateShutdown", DesktopNodeHyperVWmiVmPowerStateProvider.InitiateShutdownMethod);
        Assert.Equal(2, DesktopNodeHyperVWmiVmPowerStateProvider.EnabledState);
        Assert.Equal(3, DesktopNodeHyperVWmiVmPowerStateProvider.DisabledState);
        Assert.Equal(9, DesktopNodeHyperVWmiVmPowerStateProvider.PausedState);
        Assert.Equal(11, DesktopNodeHyperVWmiVmPowerStateProvider.ResetState);
        Assert.Equal(6, DesktopNodeHyperVWmiVmPowerStateProvider.SavedState);
        Assert.Equal(32769, DesktopNodeHyperVWmiVmPowerStateProvider.SavedEnabledState);
        Assert.Equal((uint)32768, DesktopNodeHyperVWmiVmPowerStateProvider.Failed);
    }

    [Fact]
    public void WmiVmPowerStateProviderRequireSavedAcceptsMappedSavedState()
    {
        Assert.Equal("saved", DesktopNodeHyperVWmiCommon.MapEnabledState(32769));
        Assert.Equal("saved", DesktopNodeHyperVWmiCommon.MapEnabledState(6));
        DesktopNodeHyperVWmiVmPowerStateProvider.RequireSaved(32769, "alpha");
        DesktopNodeHyperVWmiVmPowerStateProvider.RequireSaved(6, "alpha");
        DesktopNodeHyperVWmiVmPowerStateProvider.RequireSaved(DesktopNodeHyperVWmiVmPowerStateProvider.SavedState, "alpha");
        DesktopNodeHyperVWmiVmPowerStateProvider.RequireSaved(DesktopNodeHyperVWmiVmPowerStateProvider.SavedEnabledState, "alpha");
    }

    [Fact]
    public void WmiVmPowerStateProviderRequireSavedRejectsPausedState()
    {
        Assert.Equal("paused", DesktopNodeHyperVWmiCommon.MapEnabledState(9));
        var ex = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmPowerStateProvider.RequireSaved(9, "alpha"));

        Assert.Equal("PCV_VM_NOT_SAVED", ex.Code);
        Assert.False(ex.Retryable);
    }

    [Fact]
    public void WmiVmCreateProviderUsesObservedGenerationTwoResourceShape()
    {
        Assert.Equal("Msvm_SyntheticEthernetPortSettingData", DesktopNodeHyperVWmiVmCreateProvider.SyntheticEthernetPortSettingClass);
        Assert.Equal("Msvm_EthernetPortAllocationSettingData", DesktopNodeHyperVWmiVmCreateProvider.EthernetPortAllocationSettingClass);
        Assert.Equal("Microsoft:Hyper-V:Synthetic SCSI Controller", DesktopNodeHyperVWmiVmCreateProvider.SyntheticScsiControllerSubtype);
        Assert.Equal("Microsoft:Hyper-V:Synthetic Ethernet Port", DesktopNodeHyperVWmiVmCreateProvider.SyntheticEthernetPortSubtype);
        Assert.Equal("Microsoft:Hyper-V:Ethernet Connection", DesktopNodeHyperVWmiVmCreateProvider.EthernetConnectionSubtype);
        Assert.Equal(6, DesktopNodeHyperVWmiVmCreateProvider.SyntheticScsiControllerResourceType);
        Assert.Equal(10, DesktopNodeHyperVWmiVmCreateProvider.SyntheticEthernetPortResourceType);
        Assert.Equal(33, DesktopNodeHyperVWmiVmCreateProvider.EthernetConnectionResourceType);
    }

    [Fact]
    public void WmiVmDeleteProviderUsesDestroySystem()
    {
        Assert.Equal("Msvm_VirtualSystemManagementService", DesktopNodeHyperVWmiVmDeleteProvider.VirtualSystemManagementServiceClass);
        Assert.Equal("DestroySystem", DesktopNodeHyperVWmiVmDeleteProvider.DestroySystemMethod);
    }

    [Fact]
    public void WmiCheckpointMutationProviderUsesApplySnapshotForRestore()
    {
        Assert.Equal("ApplySnapshot", DesktopNodeHyperVWmiCheckpointMutationProvider.ApplySnapshotMethod);
    }

}

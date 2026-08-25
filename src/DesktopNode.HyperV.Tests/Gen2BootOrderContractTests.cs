using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

// FC-13. The 2026-08-05 audit found the create provider setting no boot order at all, so an ISO
// booted only if Hyper-V's unspecified default happened to favour it. 45ba267e made the product
// own the ordering but left the contract unlocked; these tests lock it.
public sealed class Gen2BootOrderContractTests
{
    // Shape measured on a freshly created Gen2 VM: network, then disk, then DVD.
    private const string Nic = @"VMBusPipe\{mac}\Msvm_SyntheticEthernetPort";
    private const string Disk = @"VMBusPipe\{disk}\Msvm_StorageAllocationSettingData\Scsi(0,0)";
    private const string Dvd = @"VMBusPipe\{dvd}\Msvm_StorageAllocationSettingData\Scsi(0,1)";

    [Fact]
    public void DvdMovesToTheFrontOfTheHyperVDefaultOrder()
    {
        var reordered = DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            new[] { Nic, Disk, Dvd }, Dvd);

        Assert.NotNull(reordered);
        Assert.Equal(new[] { Dvd, Nic, Disk }, reordered);
    }

    [Fact]
    public void EveryNonDvdEntryKeepsItsRelativeOrder()
    {
        // A stable sort matters here: reordering the firmware entries the caller did not ask about
        // would change PXE-versus-disk fallback behaviour as a side effect of attaching an ISO.
        var second = Disk.Replace("Scsi(0,0)", "Scsi(0,2)");

        var reordered = DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            new[] { Nic, Disk, second, Dvd }, Dvd);

        Assert.Equal(new[] { Dvd, Nic, Disk, second }, reordered);
    }

    [Fact]
    public void NoWriteHappensWhenTheDvdAlreadyBootsFirst()
    {
        // null means "leave the firmware settings alone", which keeps ConfigureGen2Firmware from
        // issuing a ModifySystemSettings call that would change nothing.
        Assert.Null(DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            new[] { Dvd, Nic, Disk }, Dvd));
    }

    [Fact]
    public void NoWriteHappensWhenTheVmHasNoDvdEntry()
    {
        Assert.Null(DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            new[] { Nic, Disk }, null));
    }

    [Fact]
    public void EmptyOrderIsLeftAlone()
    {
        Assert.Null(DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            Array.Empty<string>(), Dvd));
    }

    [Fact]
    public void DvdMatchIsCaseInsensitiveLikeTheWmiComparison()
    {
        var reordered = DesktopNodeHyperVWmiVmCreateProvider.OrderBootSourcesDvdFirst(
            new[] { Nic, Disk, Dvd }, Dvd.ToUpperInvariant());

        Assert.Equal(new[] { Dvd, Nic, Disk }, reordered);
    }
}

using DesktopNode.Contracts;

namespace DesktopNode.HyperV.IntegrationTests;

// 9402774: the inventory reports an Off VM as "stopped" while route fakes used "off", so export and network connect
// rejected every real Off VM. This reads the real vocabulary back from Hyper-V.
public sealed class VmPowerStateVocabularyTests(HyperVIntegrationFixture fixture) : IClassFixture<HyperVIntegrationFixture>
{
    [Fact]
    public void CreatedOffVmReadsAsStoppedAndPassesTheSharedOffCheck()
    {
        var name = fixture.VmName("off");
        try
        {
            var created = fixture.Invoke("vm.create", new
            {
                name,
                iso_path = fixture.IsoPath,
                cpu = 1,
                memory_mb = 1024,
                disk_gb = 8,
                vm_root = fixture.VmRoot,
                generation = 2
            });
            Assert.True(created.Ok, $"{created.Error?.Code}: {created.Error?.Message}");

            var row = fixture.FindVm(name);
            Assert.NotNull(row);
            var state = row["state"]?.GetValue<string>();
            Assert.Equal(VmPowerStates.InventoryStopped, state);
            Assert.True(VmPowerStates.IsOff(state));
            Assert.True(row["managed_by_purecvisor"]?.GetValue<bool>());
        }
        finally
        {
            fixture.DeleteVm(name);
        }
    }
}

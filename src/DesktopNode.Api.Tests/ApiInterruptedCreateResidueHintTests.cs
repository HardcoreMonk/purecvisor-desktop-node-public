using System.Text.Json;

namespace DesktopNode.Api.Tests;

// Design pcv-interrupted-create-residue-v1 §2.4: a not-applied vm.create reconcile tells the operator where residue
// may sit and that the next create of the same name recovers marked residue; reconcile itself stays readback-only.
public sealed class ApiInterruptedCreateResidueHintTests
{
    [Fact]
    public void HintNamesTheJobVmRoot()
    {
        using var parameters = JsonDocument.Parse("""{"name":"pcv-probe-rc","vm_root":"E:\\Lab\\VMs"}""");

        var hint = DesktopNodeApiJobReconciliationHandler.InterruptedCreateResidueHint(parameters.RootElement, "pcv-probe-rc");

        Assert.Contains(@"E:\Lab\VMs\pcv-probe-rc", hint, StringComparison.Ordinal);
        Assert.Contains("PureCVisor create marker", hint, StringComparison.Ordinal);
    }

    [Fact]
    public void HintFallsBackToTheDefaultVmRoot()
    {
        using var parameters = JsonDocument.Parse("""{"name":"pcv-probe-rc"}""");

        var hint = DesktopNodeApiJobReconciliationHandler.InterruptedCreateResidueHint(parameters.RootElement, "pcv-probe-rc");

        Assert.Contains(@"D:\PureCVisor\VMs\pcv-probe-rc", hint, StringComparison.Ordinal);
    }
}

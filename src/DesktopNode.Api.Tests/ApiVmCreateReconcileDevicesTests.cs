using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Api.Tests;

// Design pcv-vm-create-reconcile-devices-v1: vm.create reconcile confirms the postcondition only when the disk, ISO and
// switch the create provider always attaches are present; a registered VM without them stays a fingerprint mismatch.
public sealed class ApiVmCreateReconcileDevicesTests
{
    private const string Parameters = """{"name":"lab-vm","iso_path":"D:\\isos\\lab.iso"}""";

    [Fact]
    public void CompleteCreateHasNoMissingDevices()
    {
        Assert.Empty(Missing(Parameters, CompleteRow()));
    }

    [Theory]
    [InlineData("storage", null, "disk")]
    [InlineData("storage", """[{"kind":"vhdx","path":"D:\\PureCVisor\\VMs\\lab-vm\\disk0.vhdx","attached":false}]""", "disk")]
    [InlineData("storage", """[{"kind":"vhdx","path":"D:\\PureCVisor\\VMs\\other\\disk0.vhdx","attached":true}]""", "disk")]
    [InlineData("dvd_media", null, "iso")]
    [InlineData("dvd_media", """[{"path":"D:\\isos\\other.iso"}]""", "iso")]
    [InlineData("network", null, "switch")]
    [InlineData("network", """[{"switch":"External","mode":"external"}]""", "switch")]
    public void EachMissingDeviceIsReported(string property, string? value, string device)
    {
        var row = CompleteRow();
        if (value is null)
        {
            row.Remove(property);
        }
        else
        {
            row[property] = JsonNode.Parse(value);
        }

        Assert.Equal([device], Missing(Parameters, row));
    }

    [Fact]
    public void RegisteredVmWithoutDevicesMissesAllThree()
    {
        var row = CompleteRow();
        row.Remove("storage");
        row.Remove("dvd_media");
        row.Remove("network");

        Assert.Equal(["disk", "iso", "switch"], Missing(Parameters, row));
    }

    [Fact]
    public void PathsFollowTheJobVmRootAndIgnoreCase()
    {
        var row = CompleteRow();
        row["storage"] = JsonNode.Parse("""[{"kind":"VHDX","path":"e:\\lab\\vms\\LAB-VM\\DISK0.VHDX","attached":true}]""");
        row["dvd_media"] = JsonNode.Parse("""[{"path":"d:\\ISOS\\LAB.iso"}]""");

        Assert.Empty(Missing("""{"name":"lab-vm","iso_path":"D:\\isos\\lab.iso","vm_root":"E:\\Lab\\VMs"}""", row));
        Assert.Equal(["disk"], Missing(Parameters, row));
    }

    [Fact]
    public void JobWithoutIsoPathCannotProveTheIso()
    {
        Assert.Equal(["iso"], Missing("""{"name":"lab-vm"}""", CompleteRow()));
    }

    [Fact]
    public void HintNamesTheMissingDevicesAndTheRecovery()
    {
        var hint = DesktopNodeApiJobReconciliationHandler.IncompleteCreateHint("lab-vm", ["disk", "switch"]);

        Assert.Contains("'lab-vm'", hint, StringComparison.Ordinal);
        Assert.Contains("disk, switch", hint, StringComparison.Ordinal);
        Assert.Contains("vm.delete", hint, StringComparison.Ordinal);
    }

    private static JsonObject CompleteRow() => JsonNode.Parse("""
        {"name":"lab-vm","generation":2,"managed_by_purecvisor":true,
         "storage":[{"kind":"vhdx","path":"D:\\PureCVisor\\VMs\\lab-vm\\disk0.vhdx","attached":true}],
         "dvd_media":[{"path":"D:\\isos\\lab.iso"}],
         "network":[{"switch":"Default Switch","mode":"default-switch"}]}
        """)!.AsObject();

    private static IReadOnlyList<string> Missing(string parameters, JsonObject row)
    {
        using var parameterDocument = JsonDocument.Parse(parameters);
        using var rowDocument = JsonDocument.Parse(row.ToJsonString());
        return DesktopNodeApiJobReconciliationHandler.MissingCreateDevices(parameterDocument.RootElement, "lab-vm", rowDocument.RootElement);
    }
}

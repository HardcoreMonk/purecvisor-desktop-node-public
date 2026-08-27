using System.Text.Json;
using DesktopNode.Api;
using Xunit;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeApiJsonReaderVmLookupTests
{
    [Fact]
    public void FindVmMatchesDisplayNameAndRejectsUnmappedGuid()
    {
        using var document = JsonDocument.Parse(
            """[{"id":"pcv-p0-04275-behavior-managed","name":"pcv-p0-04275-behavior-managed"}]""");
        var data = document.RootElement;
        var byName = DesktopNodeApiJsonReader.FindVm(data, "pcv-p0-04275-behavior-managed");
        var byGuid = DesktopNodeApiJsonReader.FindVm(data, "b153fd4f-8adc-4835-8f72-750fe0649d19");
        Assert.NotNull(byName);
        Assert.Null(byGuid);
    }
}

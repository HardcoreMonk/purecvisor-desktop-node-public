using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;

namespace DesktopNode.Api.Tests;

public sealed class ApiNoVncTargetPreviewTests
{
    [Fact]
    public void PreviewAcceptsLoopbackTargetWithServiceBearerWithoutMutatingHost()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/preview",
            """{"host":"127.0.0.1","port":5900}""",
            ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("console.novnc-target.preview", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("preview", data.GetProperty("action").GetString());
        Assert.True(data.GetProperty("dry_run").GetBoolean());
        Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
        Assert.True(data.GetProperty("loopback").GetBoolean());
        Assert.Equal("127.0.0.1", data.GetProperty("host").GetString());
        Assert.Equal(5900, data.GetProperty("port").GetInt32());
        Assert.Equal(NoVncTargetPolicy.Schema, data.GetProperty("schema").GetString());
    }

    [Fact]
    public void PreviewRejectsMissingConfigurePermission()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/preview",
            """{"host":"127.0.0.1","port":5900}"""));

        Assert.Equal(403, response.StatusCode);
        Assert.Contains(NoVncTargetProblemCodes.ConfigureForbidden, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRejectsIncompleteHostPortPair()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/preview",
            """{"host":"127.0.0.1"}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(NoVncTargetProblemCodes.Incomplete, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRejectsNonLoopbackWithoutLanGates()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/preview",
            """{"host":"192.168.1.20","port":5901}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(NoVncTargetProblemCodes.NotLoopback, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewAcceptsLanTargetWhenListenerAndRequestGatesArePresent()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            consoleOptions: new DesktopNodeConsoleOptions(AllowLan: true));
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/console/novnc-target/preview",
            """{"host":"192.168.1.20","port":5901,"allow_lan_target":true,"reason":"lab streaming"}""",
            ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("loopback").GetBoolean());
        Assert.True(data.GetProperty("allow_lan_target").GetBoolean());
        Assert.True(data.GetProperty("dry_run").GetBoolean());
        Assert.Equal("192.168.1.20", data.GetProperty("host").GetString());
        Assert.Equal("lab streaming", data.GetProperty("reason").GetString());
    }
}

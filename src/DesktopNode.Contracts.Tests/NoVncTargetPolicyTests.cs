using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class NoVncTargetPolicyTests
{
    [Fact]
    public void PreviewAcceptsLoopbackTargetWithConsoleConfigure()
    {
        var result = NoVncTargetPolicy.EvaluatePreview(LoopbackRequest());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("127.0.0.1", result.Host);
        Assert.Equal(5900, result.Port);
        Assert.True(result.Loopback);
        Assert.Equal(NoVncTargetPolicy.ActionPreview, result.Action);
    }

    [Fact]
    public void SetAcceptsLocalhostAndIpv6LoopbackViaServiceBearer()
    {
        var localhost = NoVncTargetPolicy.EvaluateSet(LoopbackRequest() with
        {
            Host = "LocalHost",
            Auth = new NoVncTargetAuthContext(HasServiceBearer: true)
        });
        var ipv6 = NoVncTargetPolicy.EvaluateSet(LoopbackRequest() with
        {
            Host = "::1",
            Auth = new NoVncTargetAuthContext(HasServiceBearer: true)
        });

        Assert.True(localhost.Ok);
        Assert.True(localhost.Loopback);
        Assert.Equal(NoVncTargetPolicy.ActionSet, localhost.Action);
        Assert.True(ipv6.Ok);
        Assert.True(ipv6.Loopback);
        Assert.Equal("::1", ipv6.Host);
    }

    [Fact]
    public void SetAcceptsLanTargetWhenListenerAndRequestGatesAndReasonArePresent()
    {
        var result = NoVncTargetPolicy.EvaluateSet(LanRequest());

        Assert.True(result.Ok);
        Assert.False(result.Loopback);
        Assert.True(result.AllowLanTarget);
        Assert.Equal("lab streaming", result.Reason);
        Assert.Equal("192.168.1.20", result.Host);
        Assert.Equal(5901, result.Port);
    }

    [Fact]
    public void ClearAcceptsConfigureWithoutHostOrPort()
    {
        var result = NoVncTargetPolicy.EvaluateClear(new NoVncTargetRequest(
            Host: null,
            Port: null,
            Auth: new NoVncTargetAuthContext(HasConsoleConfigure: true)));

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.Host);
        Assert.Null(result.Port);
        Assert.Equal(NoVncTargetPolicy.ActionClear, result.Action);
    }

    [Theory]
    [InlineData("preview")]
    [InlineData("set")]
    [InlineData("clear")]
    public void RejectsMissingConfigurePermissionBeforeHostValidation(string operation)
    {
        var request = new NoVncTargetRequest(
            Host: null,
            Port: null,
            Auth: new NoVncTargetAuthContext());
        var result = Evaluate(operation, request);

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.ConfigureForbidden, result.ErrorCode);
    }

    [Theory]
    [InlineData("127.0.0.1", null)]
    [InlineData(null, 5900)]
    [InlineData(" ", 5900)]
    public void SetRejectsIncompleteHostPortPair(string? host, int? port)
    {
        var result = NoVncTargetPolicy.EvaluateSet(LoopbackRequest() with { Host = host, Port = port });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.Incomplete, result.ErrorCode);
    }

    [Fact]
    public void PreviewRejectsMissingHostAndPortAsHostRequired()
    {
        var result = NoVncTargetPolicy.EvaluatePreview(LoopbackRequest() with { Host = "  ", Port = null });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.HostRequired, result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void SetRejectsPortOutsideInclusiveRange(int port)
    {
        var result = NoVncTargetPolicy.EvaluateSet(LoopbackRequest() with { Port = port });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.PortInvalid, result.ErrorCode);
    }

    [Fact]
    public void SetRejectsNonLoopbackWithoutAllowLanTarget()
    {
        var result = NoVncTargetPolicy.EvaluateSet(LanRequest() with { AllowLanTarget = false });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.NotLoopback, result.ErrorCode);
    }

    [Fact]
    public void SetRejectsNonLoopbackWhenListenerLanIsOff()
    {
        var result = NoVncTargetPolicy.EvaluateSet(LanRequest() with { ListenerAllowLan = false });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.LanGateRequired, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetRejectsNonLoopbackWithoutReason(string? reason)
    {
        var result = NoVncTargetPolicy.EvaluateSet(LanRequest() with { Reason = reason });

        Assert.False(result.Ok);
        Assert.Equal(NoVncTargetProblemCodes.ReasonRequired, result.ErrorCode);
    }

    [Fact]
    public void IsLoopbackHostMatchesHostListenContract()
    {
        Assert.True(NoVncTargetPolicy.IsLoopbackHost("127.0.0.1"));
        Assert.True(NoVncTargetPolicy.IsLoopbackHost("localhost"));
        Assert.True(NoVncTargetPolicy.IsLoopbackHost("::1"));
        Assert.False(NoVncTargetPolicy.IsLoopbackHost("192.168.1.20"));
        Assert.False(NoVncTargetPolicy.IsLoopbackHost("127.0.0.2"));
    }

    private static NoVncTargetEvaluation Evaluate(string operation, NoVncTargetRequest request)
    {
        return operation switch
        {
            "preview" => NoVncTargetPolicy.EvaluatePreview(request),
            "set" => NoVncTargetPolicy.EvaluateSet(request),
            _ => NoVncTargetPolicy.EvaluateClear(request)
        };
    }

    private static NoVncTargetRequest LoopbackRequest()
    {
        return new NoVncTargetRequest(
            Host: "127.0.0.1",
            Port: 5900,
            Auth: new NoVncTargetAuthContext(HasConsoleConfigure: true));
    }

    private static NoVncTargetRequest LanRequest()
    {
        return new NoVncTargetRequest(
            Host: "192.168.1.20",
            Port: 5901,
            Auth: new NoVncTargetAuthContext(HasConsoleConfigure: true),
            AllowLanTarget: true,
            ListenerAllowLan: true,
            Reason: "  lab streaming  ");
    }
}

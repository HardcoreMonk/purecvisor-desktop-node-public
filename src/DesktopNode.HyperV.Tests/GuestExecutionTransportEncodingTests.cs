using System.Diagnostics;
using System.Text;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class GuestExecutionTransportEncodingTests
{
    // Characters chosen to span several legacy OEM code pages so the round-trip cannot
    // pass by accident on a host whose console page happens to cover one script.
    private const string NonAsciiSample = "café 한글 日本語 Ж Ω ß";

    [Fact]
    public void BridgeStartInfoPinsUtf8OnEveryRedirectedStream()
    {
        var startInfo = DesktopNodeHyperVPowerShellDirectTransport.BuildBridgeStartInfo("$null");

        Assert.True(startInfo.RedirectStandardInput);
        Assert.True(startInfo.RedirectStandardOutput);
        Assert.True(startInfo.RedirectStandardError);

        // A null encoding means .NET falls back to the console code page, which on Windows
        // is an OEM page rather than UTF-8.
        Assert.NotNull(startInfo.StandardInputEncoding);
        Assert.NotNull(startInfo.StandardOutputEncoding);
        Assert.NotNull(startInfo.StandardErrorEncoding);

        Assert.Equal(Encoding.UTF8.CodePage, startInfo.StandardInputEncoding!.CodePage);
        Assert.Equal(Encoding.UTF8.CodePage, startInfo.StandardOutputEncoding!.CodePage);
        Assert.Equal(Encoding.UTF8.CodePage, startInfo.StandardErrorEncoding!.CodePage);

        // A byte-order mark on stdin would corrupt the first JSON character the bridge reads.
        Assert.Empty(startInfo.StandardInputEncoding.GetPreamble());
    }

    [Fact]
    public void BridgeStartInfoEncodesTheScriptAsUtf16Base64()
    {
        const string script = "Write-Output 'x'";

        var startInfo = DesktopNodeHyperVPowerShellDirectTransport.BuildBridgeStartInfo(script);

        var encoded = startInfo.Arguments.Split("-EncodedCommand ", StringSplitOptions.None)[1];
        Assert.Equal(script, Encoding.Unicode.GetString(Convert.FromBase64String(encoded)));
    }

    [Fact]
    public void NonAsciiPayloadSurvivesTheRoundTripThroughPowerShell()
    {
        // Exercises the real transport contract without a Hyper-V guest: the payload the
        // provider writes to stdin must come back byte-identical on stdout. FC-12(b) is the
        // guest-side half of this same property. The echo body goes through the same
        // WrapWithUtf8Streams the production bridge uses, so this covers the shipped setup
        // rather than a parallel one.
        var echoScript = DesktopNodeHyperVPowerShellDirectTransport.WrapWithUtf8Streams(
            "$pcvOut.Write($pcvIn.ReadToEnd())");

        var startInfo = DesktopNodeHyperVPowerShellDirectTransport.BuildBridgeStartInfo(echoScript);

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());
        process.StandardInput.Write(NonAsciiSample);
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromSeconds(60)));

        Assert.Equal(NonAsciiSample, stdout);
    }
}

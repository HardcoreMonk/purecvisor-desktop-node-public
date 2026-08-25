using System.Text;
using DesktopNode.Cli;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliInteractiveShellTests
{
    [Fact]
    public async Task HelpPrintsBannerAndCommandTableWithoutCallingApi()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));
        var input = new Queue<string?>(["help", "exit"]);
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var exitCode = await DesktopNodeCliInteractiveShell.RunAsync(
            [],
            transport,
            () => input.Dequeue(),
            value => stdout.Append(value),
            value => stderr.Append(value),
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.False(transport.Called);
        Assert.Contains("___  ", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("NEURAL LINK ESTABLISHED", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("Type 'help' for commands", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("(pcv)", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("❯", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("Usage: pcvcli [FLAGS] <object> <action> [args...]", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("Available Commands:", stdout.ToString(), StringComparison.Ordinal);
        Assert.Contains("vm", stdout.ToString(), StringComparison.Ordinal);
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public void HelpUsesLinuxStyleCyberPaletteAndCommandRows()
    {
        var help = DesktopNodeCliInteractiveShell.GetHelp();

        Assert.Contains("\u001b[38;5;33m", help, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;198m", help, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;51m", help, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;226mvm\u001b[0m create |", help, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;240mCreate a new VM\u001b[0m", help, StringComparison.Ordinal);
    }

    [Fact]
    public void ProgramConfiguresUtf8OutputForLinuxStylePromptGlyph()
    {
        var previousOutputEncoding = Console.OutputEncoding;
        try
        {
            Program.ConfigureConsoleEncoding();
            var promptGlyph = "❯";
            var encoded = Console.OutputEncoding.GetBytes(promptGlyph);

            Assert.Equal(Encoding.UTF8.WebName, Console.OutputEncoding.WebName);
            Assert.Equal(promptGlyph, Console.OutputEncoding.GetString(encoded));
        }
        finally
        {
            Console.OutputEncoding = previousOutputEncoding;
        }
    }

    [Fact]
    public void HelpListsAvailableCommandsAsSingleCommandRows()
    {
        var help = DesktopNodeCliInteractiveShell.GetHelp(noColor: true);
        var lines = help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm create | Create a new VM", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm start | Start a VM", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm rename | Rename a VM", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm pause | Pause a VM", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm resume | Resume a VM", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm save | Save VM to Hyper-V Saved state", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm resume-saved | Resume a VM from Hyper-V Saved state", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm manage | Promote an existing VM to PureCVisor managed", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm attach | Attach ISO media to the virtual DVD", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm eject | Eject ISO media from the virtual DVD", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm memory-stats | Show VM memory stats", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm cpu-stats | Show VM CPU stats", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm blkio-set | Set Hyper-V storage IOPS policy", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm bandwidth-set | Set Hyper-V network bandwidth policy", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "vm checkpoint restore | Restore a VM checkpoint", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "job reconcile | Reconcile an interrupted rename, delete, checkpoint create, or restore", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "diagnostics bundle list | List diagnostic bundles", StringComparison.Ordinal));
        Assert.Contains(lines, line => string.Equals(line.Trim(), "diagnostics bundle download | Download an evidence bundle", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Trim().StartsWith("snapshot ", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("list|get|create", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("start|stop|shutdown", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("bundle create|download", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DispatchesInteractiveCommandThroughExistingApiClient()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true,\"status\":\"ready\"}"));
        var input = new Queue<string?>(["--json host status", "quit"]);
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var exitCode = await DesktopNodeCliInteractiveShell.RunAsync(
            [],
            transport,
            () => input.Dequeue(),
            value => stdout.Append(value),
            value => stderr.Append(value),
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.True(transport.Called);
        Assert.Equal("GET", transport.Request!.Method);
        Assert.Equal("/api/v1/host/status", transport.Request.Path);
        Assert.Contains("\"status\":\"ready\"", stdout.ToString(), StringComparison.Ordinal);
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public async Task TokenizesQuotedInteractiveRouteSegments()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));
        var input = new Queue<string?>(["vm get \"ubuntu lab\"", "exit"]);
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var exitCode = await DesktopNodeCliInteractiveShell.RunAsync(
            ["--no-color"],
            transport,
            () => input.Dequeue(),
            value => stdout.Append(value),
            value => stderr.Append(value),
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Equal("/api/v1/vms/ubuntu%20lab", transport.Request!.Path);
        Assert.DoesNotContain("\u001b[", stdout.ToString(), StringComparison.Ordinal);
        Assert.Empty(stderr.ToString());
    }

    [Fact]
    public async Task InteractiveCommandUsageErrorsDoNotAppendGlobalUsageBlock()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));
        var input = new Queue<string?>(["vm get", "exit"]);
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var exitCode = await DesktopNodeCliInteractiveShell.RunAsync(
            [],
            transport,
            () => input.Dequeue(),
            value => stdout.Append(value),
            value => stderr.Append(value),
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        var error = stderr.ToString();
        Assert.Equal(0, exitCode);
        Assert.False(transport.Called);
        Assert.Contains("PCV_CLI_USAGE|Use: vm get <vm>.", error, StringComparison.Ordinal);
        Assert.DoesNotContain("Usage:", error, StringComparison.Ordinal);
        Assert.DoesNotContain("pcvcli [--api URL]", error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("host", "host status")]
    [InlineData("network l", "network list")]
    [InlineData("vm bandwidth-s", "vm bandwidth-set ")]
    [InlineData("vm att", "vm attach ")]
    [InlineData("vm eje", "vm eject ")]
    [InlineData("vm sav", "vm save ")]
    [InlineData("vm resume-s", "vm resume-saved ")]
    [InlineData("vm mana", "vm manage ")]
    [InlineData("diagnostics bundle l", "diagnostics bundle list")]
    [InlineData("snapshot roll", "snapshot roll")]
    [InlineData("unknown", "unknown")]
    public void CompletesKnownInteractiveCommandPrefixes(string prefix, string expected)
    {
        Assert.Equal(expected, DesktopNodeCliInteractiveShell.Complete(prefix));
    }

    private static string MissingDefaultProtectedTokenPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "pcv-cli-default-token-tests",
            Guid.NewGuid().ToString("N"),
            "api-token.dpapi.json");
    }

    private sealed class RecordingTransport(DesktopNodeCliTransportResponse response) : IDesktopNodeCliTransport
    {
        public bool Called { get; private set; }

        public DesktopNodeCliRequest? Request { get; private set; }

        public Task<DesktopNodeCliTransportResponse> SendAsync(
            DesktopNodeCliRequest request,
            DesktopNodeCliOptions options,
            string? bearerToken,
            CancellationToken cancellationToken)
        {
            Called = true;
            Request = request;
            return Task.FromResult(response);
        }
    }
}

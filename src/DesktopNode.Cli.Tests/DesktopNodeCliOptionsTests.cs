using DesktopNode.Cli;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliOptionsTests
{
    [Fact]
    public void ParsesGlobalOptionsAndLeavesCommandArguments()
    {
        var options = DesktopNodeCliOptions.Parse([
            "--api",
            "http://127.0.0.1:7777",
            "--format",
            "json",
            "--token",
            "inline-token",
            "host",
            "status"
        ]);

        Assert.Equal("http://127.0.0.1:7777", options.ApiBaseUrl);
        Assert.Equal(DesktopNodeCliOutputFormat.Json, options.Format);
        Assert.Equal("inline-token", options.Token);
        Assert.Equal(["host", "status"], options.CommandArguments);
    }

    [Fact]
    public void SupportsEqualsStyleOptions()
    {
        var options = DesktopNodeCliOptions.Parse([
            "--api=http://localhost:7777",
            "--format=csv",
            "--token-env=PCV_TOKEN",
            "job",
            "list"
        ]);

        Assert.Equal("http://localhost:7777", options.ApiBaseUrl);
        Assert.Equal(DesktopNodeCliOutputFormat.Csv, options.Format);
        Assert.Equal("PCV_TOKEN", options.TokenEnv);
        Assert.Equal(["job", "list"], options.CommandArguments);
    }

    [Fact]
    public void JsonShortcutSelectsJsonFormat()
    {
        var options = DesktopNodeCliOptions.Parse(["--json", "runtime", "policy"]);

        Assert.Equal(DesktopNodeCliOutputFormat.Json, options.Format);
        Assert.Equal(["runtime", "policy"], options.CommandArguments);
    }

    [Fact]
    public void ShortHelpSelectsHelpMode()
    {
        var options = DesktopNodeCliOptions.Parse(["-h"]);

        Assert.True(options.ShowHelp);
        Assert.Empty(options.CommandArguments);
    }

    [Fact]
    public void ShortVerboseSelectsVerboseMode()
    {
        var options = DesktopNodeCliOptions.Parse(["-v", "host", "status"]);

        Assert.True(options.Verbose);
        Assert.Equal(["host", "status"], options.CommandArguments);
    }
}

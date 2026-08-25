using System.Diagnostics;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

// FC-12(b)'s guest half was recorded as an encoding question. It was not. The transport joined
// argv into one string and re-parsed it as a script in the guest, so an argument containing a
// space was split and an argument containing PowerShell syntax was executed. These tests lock
// the contract the CLI already documents - `vm guest-exec <vm> -- <command>` passes argv - and
// run it for real without needing a Hyper-V guest, because argv fidelity is decided in the
// bridge before the guest is contacted.
public sealed class GuestExecutionArgvFidelityTests
{
    private static void AssertArgv(string expected, params string[] argv)
    {
        var (stdout, stderr) = InvokeArgv(argv);
        Assert.Equal(expected, stdout);
        Assert.False(
            stderr.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
            stderr.Contains("ErrorRecord", StringComparison.OrdinalIgnoreCase),
            $"argv invocation reported an error on stderr: {stderr}");
    }

    // Runs the shipped argv invocation against a command array, going through the same
    // WrapWithUtf8Streams/BuildBridgeStartInfo the provider uses so this covers the shipped
    // setup rather than a parallel one.
    private static (string Stdout, string Stderr) InvokeArgv(params string[] argv)
    {
        var literal = string.Join(
            ", ",
            argv.Select(static value => "'" + value.Replace("'", "''") + "'"));

        var script = DesktopNodeHyperVPowerShellDirectTransport.WrapWithUtf8Streams($$"""
        $pcvArgv = [string[]]@({{literal}})
        $pcvInvoke = {
        {{DesktopNodeHyperVPowerShellDirectTransport.GuestArgvInvocation}}
        }
        $pcvOut.Write((& $pcvInvoke $pcvArgv | Out-String -Width 4096))
        """);

        using var process = new Process
        {
            StartInfo = DesktopNodeHyperVPowerShellDirectTransport.BuildBridgeStartInfo(script)
        };
        Assert.True(process.Start());
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEnd();
        // Not asserted empty: powershell.exe writes a CLIXML progress record ("preparing modules
        // for first use") to stderr on a cold start, which is noise rather than a failure. It is
        // carried into the assertion message instead so a real guest-side error stays visible.
        var stderr = process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromSeconds(60)));

        return (stdout.ReplaceLineEndings("\n").TrimEnd('\n'), stderr);
    }

    [Fact]
    public void ArgumentContainingSpacesStaysOneArgument()
    {
        // The join produced three separate outputs here.
        AssertArgv("a b c", "Write-Output", "a b c");
    }

    [Fact]
    public void ArgumentContainingASubexpressionIsNotEvaluated()
    {
        // The join evaluated this to "2".
        AssertArgv("$(1+1)", "Write-Output", "$(1+1)");
    }

    [Fact]
    public void ArgumentContainingAStatementSeparatorDoesNotStartASecondStatement()
    {
        // The join ran the tail as its own statement and emitted "x" then "INJECTED".
        AssertArgv(
            "x; Write-Output INJECTED",
            "Write-Output",
            "x; Write-Output INJECTED");
    }

    [Fact]
    public void NonAsciiArgumentSurvivesAsASingleArgument()
    {
        // The exact sample FC-12(b) used. Under the join its spaces split it into separate
        // output lines, which is why the observed byte count matched neither UTF-8 nor OEM.
        const string sample = "café 한글 日本語 Ж Ω ß";

        AssertArgv(sample, "Write-Output", sample);
    }

    [Fact]
    public void SingleElementArgvInvokesTheCommandWithoutArguments()
    {
        // Guards the descending-range trap: $argv[1..0] on a one-element array yields
        // @($argv[1], $argv[0]), which would pass the command to itself.
        // hostname's casing is the OS's to choose, so this one compares case-insensitively
        // rather than going through AssertArgv.
        var (stdout, _) = InvokeArgv("hostname");
        Assert.Equal(Environment.MachineName, stdout, ignoreCase: true);
    }

    [Fact]
    public void BridgeScriptDoesNotConcatenateArgvIntoAScript()
    {
        // A regression guard on the shipped bridge text itself: reintroducing the join would
        // restore both the splitting and the execution, and the tests above only cover the
        // invocation constant.
        var sourcePath = Path.Combine(
            FindRepoRoot(),
            "src",
            "DesktopNode.HyperV",
            "DesktopNodeHyperVPowerShellDirectGuestExecutionProvider.cs");
        Assert.True(File.Exists(sourcePath), sourcePath);

        // Comment lines are excluded on purpose: the fix's own comment quotes the removed
        // expression so a future reader knows what this replaced, and a naive whole-file scan
        // would match that prose and fail for the wrong reason.
        var code = File.ReadAllLines(sourcePath)
            .Where(static line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            .ToArray();
        var codeText = string.Join("\n", code);

        Assert.DoesNotContain("-join ' '", codeText, StringComparison.Ordinal);
        Assert.DoesNotContain("[scriptblock]::Create", codeText, StringComparison.Ordinal);
        Assert.Contains("-ArgumentList (, $pcvArgv)", codeText, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}

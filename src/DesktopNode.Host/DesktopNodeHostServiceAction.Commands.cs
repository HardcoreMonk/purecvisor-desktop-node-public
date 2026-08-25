using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DesktopNode.Host;

public static partial class DesktopNodeHostServiceAction
{
    private static async Task<DesktopNodeHostCommandResult> InvokeCommandAsync(
        DesktopNodeHostCommand command,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command.FileName,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in command.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {command.FileName}.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        return new DesktopNodeHostCommandResult(
            FileName: command.FileName,
            Arguments: command.Arguments,
            ExitCode: process.ExitCode,
            Stdout: stdout,
            Stderr: stderr,
            Ok: IsAllowed(command, process.ExitCode));
    }

    private static async Task<DesktopNodeHostCommandResult> WaitForServiceStoppedAsync(
        string serviceName,
        CancellationToken cancellationToken)
    {
        DesktopNodeHostCommandResult lastResult = new(
            "sc.exe",
            ["query", serviceName],
            ExitCode: 1,
            Stdout: string.Empty,
            Stderr: string.Empty,
            Ok: false);

        for (var attempt = 0; attempt < 30; attempt++)
        {
            lastResult = await InvokeCommandAsync(
                new DesktopNodeHostCommand("sc.exe", ["query", serviceName]),
                cancellationToken).ConfigureAwait(false);
            if (IsStoppedOrMissingQuery(lastResult))
            {
                return lastResult with { Ok = true };
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }

        return lastResult with { Ok = false };
    }

    private static bool IsStopCommand(DesktopNodeHostCommand command)
    {
        return command.Arguments.Count > 0 &&
            string.Equals(command.Arguments[0], "stop", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStoppedOrMissingQuery(DesktopNodeHostCommandResult result)
    {
        if (result.ExitCode == 1060)
        {
            return true;
        }

        return result.ExitCode == 0 &&
            (result.Stdout.Contains("STOPPED", StringComparison.OrdinalIgnoreCase) ||
                System.Text.RegularExpressions.Regex.IsMatch(result.Stdout, @":\s*1\s", System.Text.RegularExpressions.RegexOptions.CultureInvariant));
    }

    private static bool IsAllowed(DesktopNodeHostCommand command, int exitCode)
    {
        if (exitCode == 0)
        {
            return true;
        }

        var action = command.Arguments.Count > 0 ? command.Arguments[0].ToLowerInvariant() : string.Empty;
        return action switch
        {
            "create" => exitCode == 1073,
            "stop" => exitCode is 1060 or 1062,
            "delete" => exitCode == 1060,
            _ => false
        };
    }
}

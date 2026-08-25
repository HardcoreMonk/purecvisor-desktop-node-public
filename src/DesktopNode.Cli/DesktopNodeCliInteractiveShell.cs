using System.Text;

namespace DesktopNode.Cli;

public static class DesktopNodeCliInteractiveShell
{
    private const string Reset = "\u001b[0m";
    private const string Cyan = "\u001b[38;5;51m";
    private const string Pink = "\u001b[38;5;198m";
    private const string Yellow = "\u001b[38;5;226m";
    private const string Green = "\u001b[38;5;46m";
    private const string Blue = "\u001b[38;5;33m";
    private const string Dim = "\u001b[38;5;240m";
    private const string Bold = "\u001b[1m";

    private static readonly string[] ExitCommands = ["exit", "quit"];
    private static readonly string[] HelpCommands = ["help", "?"];
    private static readonly string[] CompletionCandidates =
    [
        "help",
        "exit",
        "quit",
        "host status",
        "runtime policy",
        "ops summary",
        "network inventory",
        "network list",
        "vm list",
        "vm get ",
        "vm create ",
        "vm start ",
        "vm stop ",
        "vm shutdown ",
        "vm guest-shutdown ",
        "vm poweroff ",
        "vm restart ",
        "vm rename ",
        "vm pause ",
        "vm resume ",
        "vm save ",
        "vm resume-saved ",
        "vm manage ",
        "vm attach ",
        "vm eject ",
        "vm console ",
        "vm vnc ",
        "vm memory-stats ",
        "vm cpu-stats ",
        "vm limit ",
        "vm blkio-set ",
        "vm blkio-get ",
        "vm bandwidth ",
        "vm bandwidth-set ",
        "vm guest-agent-status ",
        "vm guest-ping ",
        "vm delete ",
        "vm checkpoint list ",
        "vm checkpoint create ",
        "vm checkpoint restore ",
        "vm checkpoint delete ",
        "vm snapshot list ",
        "vm snapshot create ",
        "vm snapshot rollback ",
        "vm snapshot delete ",
        "job list",
        "job get ",
        "job cancel ",
        "job retry ",
        "job reconcile ",
        "diagnostics bundle list",
        "diagnostics bundle create",
        "diagnostics bundle download "
    ];

    private static readonly HelpCommand[] AvailableCommands =
    [
        new("host status", "Host readiness and Hyper-V status"),
        new("runtime policy", "Runtime/auth/job/native operation policy"),
        new("ops summary", "Operator summary"),
        new("network inventory", "Hyper-V switch inventory"),
        new("network list", "Hyper-V switch inventory"),
        new("vm list", "List all VMs"),
        new("vm get", "Get VM details"),
        new("vm create", "Create a new VM"),
        new("vm start", "Start a VM"),
        new("vm stop", "Stop a VM"),
        new("vm shutdown", "Gracefully shut down a VM"),
        new("vm guest-shutdown", "Gracefully shut down a VM"),
        new("vm poweroff", "Power off a VM"),
        new("vm restart", "Restart a VM"),
        new("vm rename", "Rename a VM"),
        new("vm pause", "Pause a VM"),
        new("vm resume", "Resume a VM"),
        new("vm save", "Save VM to Hyper-V Saved state"),
        new("vm resume-saved", "Resume a VM from Hyper-V Saved state"),
        new("vm manage", "Promote an existing VM to PureCVisor managed"),
        new("vm attach", "Attach ISO media to the virtual DVD"),
        new("vm eject", "Eject ISO media from the virtual DVD"),
        new("vm console", "Get console handoff details"),
        new("vm vnc", "Get noVNC session details"),
        new("vm memory-stats", "Show VM memory stats"),
        new("vm cpu-stats", "Show VM CPU stats"),
        new("vm limit", "Set Hyper-V CPU/MEM limits"),
        new("vm blkio-set", "Set Hyper-V storage IOPS policy"),
        new("vm blkio-get", "Show Hyper-V storage policy readback"),
        new("vm bandwidth", "Show Hyper-V network policy readback"),
        new("vm bandwidth-set", "Set Hyper-V network bandwidth policy"),
        new("vm guest-agent-status", "Show Hyper-V guest service status"),
        new("vm guest-ping", "Check Hyper-V guest service readiness"),
        new("vm delete", "Delete a VM"),
        new("vm checkpoint list", "List VM checkpoints"),
        new("vm checkpoint create", "Create a VM checkpoint"),
        new("vm checkpoint restore", "Restore a VM checkpoint"),
        new("vm checkpoint delete", "Delete a VM checkpoint"),
        new("vm snapshot list", "List VM checkpoints"),
        new("vm snapshot create", "Create a VM checkpoint"),
        new("vm snapshot rollback", "Restore a VM checkpoint"),
        new("vm snapshot delete", "Delete a VM checkpoint"),
        new("job list", "List queued jobs"),
        new("job get", "Get queued job details"),
        new("job cancel", "Cancel a queued job"),
        new("job retry", "Retry a failed job"),
        new("job reconcile", "Reconcile an interrupted rename, delete, checkpoint create, or restore"),
        new("diagnostics bundle list", "List diagnostic bundles"),
        new("diagnostics bundle create", "Create an evidence bundle"),
        new("diagnostics bundle download", "Download an evidence bundle")
    ];

    private const string Banner = """
     ___  _   _  ___  ___  ___  _ _  _  ___  ___  ___
    | . \| | | || . \| __>|  _>| | || |/ __>/ . \| . \
    |  /| |_| ||   /| _> | <__| V || |\__ \| | ||   /
    |_|  \___/ |_|_\<___>\___/ \_/ |_|<___/\___/|_|_\
                [ NEURAL LINK ESTABLISHED ]
    """;

    public static async Task<int> RunAsync(
        IReadOnlyList<string> startupArgs,
        IDesktopNodeCliTransport transport,
        Func<string?> readLine,
        Action<string> writeOutput,
        Action<string> writeError,
        Func<string, string?>? environment = null,
        string? defaultProtectedTokenFilePath = null,
        CancellationToken cancellationToken = default)
    {
        var shellArgs = RemoveInteractiveFlags(startupArgs);
        var noColor = shellArgs.Any(arg => string.Equals(arg, "--no-color", StringComparison.OrdinalIgnoreCase)) ||
            string.Equals(environment?.Invoke("NO_COLOR"), "1", StringComparison.OrdinalIgnoreCase);

        writeOutput(GetBanner(noColor));
        writeOutput("Type 'help' for commands | 'exit' to quit | Tab to complete" + Environment.NewLine + Environment.NewLine);

        while (!cancellationToken.IsCancellationRequested)
        {
            writeOutput(GetPrompt(noColor));
            var line = readLine();
            if (line is null)
            {
                return 0;
            }

            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (ExitCommands.Any(command => string.Equals(command, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                return 0;
            }

            if (HelpCommands.Any(command => string.Equals(command, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                writeOutput(GetHelp(noColor));
                continue;
            }

            try
            {
                var commandArgs = SplitCommandLine(trimmed);
                var args = shellArgs.Concat(commandArgs).ToArray();
                var result = await DesktopNodeCliApplication.RunAsync(
                        args,
                        transport,
                        environment: environment,
                        defaultProtectedTokenFilePath: defaultProtectedTokenFilePath,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(result.StandardOutput))
                {
                    writeOutput(result.StandardOutput);
                }

                if (!string.IsNullOrEmpty(result.StandardError))
                {
                    writeError(result.StandardError);
                }
            }
            catch (ArgumentException ex)
            {
                writeError(ex.Message + Environment.NewLine);
            }
        }

        return 0;
    }

    public static string GetBanner(bool noColor = false)
    {
        if (noColor)
        {
            return Banner + Environment.NewLine;
        }

        return string.Join(Environment.NewLine, [
            Bold + Blue + " ___  " + Pink + "_   _  ___  ___  " + Blue + "___  _ _  " + Pink + "_  ___  ___  ___ ",
            Blue + "| . \\" + Pink + "| | | || . \\| __>" + Blue + "|  _>| | |" + Pink + "| |/ __>/ . \\| . \\",
            Blue + "|  /" + Pink + "| |_| ||   /| _> " + Blue + "| <__| V |" + Pink + "| |\\__ \\| | ||   /",
            Blue + "|_|  " + Pink + "\\___/ |_|_\\<___>" + Blue + "\\___/ \\_/ " + Pink + "|_|<___/\\___/|_|_\\",
            Cyan + "            [ NEURAL LINK ESTABLISHED ]            " + Reset,
            string.Empty
        ]) + Environment.NewLine;
    }

    public static string? ReadLineWithCompletion()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine();
        }

        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return buffer.ToString();
                case ConsoleKey.Backspace:
                    if (buffer.Length > 0)
                    {
                        buffer.Length--;
                        Console.Write("\b \b");
                    }

                    break;
                case ConsoleKey.Tab:
                    var completion = Complete(buffer.ToString());
                    if (completion.Length > buffer.Length)
                    {
                        var suffix = completion[buffer.Length..];
                        buffer.Append(suffix);
                        Console.Write(suffix);
                    }

                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }

                    break;
            }
        }
    }

    public static string GetHelp(bool noColor = false)
    {
        var heading = noColor ? "Available Commands:" : Cyan + "Available Commands:" + Reset;
        var usage = noColor
            ? "Usage: pcvcli [FLAGS] <object> <action> [args...]"
            : Yellow + "Usage: pcvcli [FLAGS] <object> <action> [args...]" + Reset;
        var globalFlags = noColor ? "Global Flags:" : Bold + "Global Flags:" + Reset;
        var separator = noColor ? "------------------------------------------------------------" : Dim + "------------------------------------------------------------" + Reset;

        return string.Join(Environment.NewLine, [
            GetBanner(noColor).TrimEnd(),
            string.Empty,
            usage,
            string.Empty,
            globalFlags,
            "  --api <url>                       Local API URL",
            "  --format table|json|plain|csv     Output format",
            "  --json / --plain / --csv          Output shortcuts",
            "  --token / --token-file / --token-env / --protected-token-file",
            "  --no-color                        Disable ANSI color",
            "  --verbose / -v                    Print request metadata",
            "  --interactive / -i                Enter REPL mode",
            "  --help / -h                       Print help",
            string.Empty,
            heading,
            separator,
            .. AvailableCommands.Select(command => CommandRow(command.Command, command.Description, noColor)),
            string.Empty
        ]) + Environment.NewLine;
    }

    internal static IReadOnlyList<string> SplitCommandLine(string commandLine)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        char? quote = null;

        for (var index = 0; index < commandLine.Length; index++)
        {
            var character = commandLine[index];
            if (quote is not null)
            {
                if (character == '\\' && index + 1 < commandLine.Length && commandLine[index + 1] == quote)
                {
                    current.Append(commandLine[++index]);
                    continue;
                }

                if (character == quote)
                {
                    quote = null;
                    continue;
                }

                current.Append(character);
                continue;
            }

            if (character is '"' or '\'')
            {
                quote = character;
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                FlushCurrent(args, current);
                continue;
            }

            current.Append(character);
        }

        if (quote is not null)
        {
            throw new ArgumentException("PCV_CLI_USAGE|Unclosed quote in interactive command.");
        }

        FlushCurrent(args, current);
        return args;
    }

    internal static string Complete(string commandPrefix)
    {
        var matches = CompletionCandidates
            .Where(candidate => candidate.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matches.Length == 0)
        {
            return commandPrefix;
        }

        if (matches.Length == 1)
        {
            return matches[0];
        }

        var commonPrefix = matches[0];
        foreach (var match in matches.Skip(1))
        {
            commonPrefix = CommonPrefix(commonPrefix, match);
        }

        return commonPrefix.Length > commandPrefix.Length ? commonPrefix : commandPrefix;
    }

    private static string[] RemoveInteractiveFlags(IReadOnlyList<string> startupArgs)
    {
        return startupArgs
            .Where(arg =>
                !string.Equals(arg, "--interactive", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(arg, "-i", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string GetPrompt(bool noColor)
    {
        return noColor ? "(pcv) > " : Cyan + "(pcv)" + Reset + " " + Green + "❯" + Reset + " ";
    }

    private static string CommandRow(string command, string description, bool noColor)
    {
        if (noColor)
        {
            return $"  {command} | {description}";
        }

        var spaceIndex = command.IndexOf(' ', StringComparison.Ordinal);
        var objectName = spaceIndex < 0 ? command : command[..spaceIndex];
        var action = spaceIndex < 0 ? string.Empty : command[spaceIndex..];

        return $"  {Yellow}{objectName}{Reset}{action} | {Dim}{description}{Reset}";
    }

    private static void FlushCurrent(ICollection<string> args, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        args.Add(current.ToString());
        current.Clear();
    }

    private static string CommonPrefix(string left, string right)
    {
        var length = Math.Min(left.Length, right.Length);
        var index = 0;
        while (index < length && char.ToUpperInvariant(left[index]) == char.ToUpperInvariant(right[index]))
        {
            index++;
        }

        return left[..index];
    }

    private sealed record HelpCommand(string Command, string Description);
}

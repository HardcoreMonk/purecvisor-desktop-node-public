namespace DesktopNode.Cli;

public sealed record DesktopNodeCliOptions(
    string ApiBaseUrl,
    DesktopNodeCliOutputFormat Format,
    bool NoColor,
    bool Verbose,
    bool ShowHelp,
    string? Token,
    string? TokenFile,
    string? TokenEnv,
    string? ProtectedTokenFile,
    IReadOnlyList<string> CommandArguments)
{
    public static DesktopNodeCliOptions Parse(IReadOnlyList<string> args)
    {
        var apiBaseUrl = "http://127.0.0.1:7777";
        var format = DesktopNodeCliOutputFormat.Table;
        var noColor = false;
        var verbose = false;
        var showHelp = false;
        string? token = null;
        string? tokenFile = null;
        string? tokenEnv = null;
        string? protectedTokenFile = null;
        var command = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            var raw = args[index];
            if (string.Equals(raw, "-h", StringComparison.OrdinalIgnoreCase) && command.Count == 0)
            {
                showHelp = true;
                continue;
            }

            if (string.Equals(raw, "-v", StringComparison.OrdinalIgnoreCase) && command.Count == 0)
            {
                verbose = true;
                continue;
            }

            if (!raw.StartsWith("--", StringComparison.Ordinal) || command.Count > 0)
            {
                command.Add(raw);
                continue;
            }

            var (name, inlineValue) = SplitOption(raw);
            switch (name)
            {
                case "--api":
                    apiBaseUrl = RequiredValue(args, ref index, name, inlineValue);
                    break;
                case "--format":
                    format = ParseFormat(RequiredValue(args, ref index, name, inlineValue));
                    break;
                case "--json":
                    EnsureNoInlineValue(name, inlineValue);
                    format = DesktopNodeCliOutputFormat.Json;
                    break;
                case "--plain":
                    EnsureNoInlineValue(name, inlineValue);
                    format = DesktopNodeCliOutputFormat.Plain;
                    break;
                case "--csv":
                    EnsureNoInlineValue(name, inlineValue);
                    format = DesktopNodeCliOutputFormat.Csv;
                    break;
                case "--no-color":
                    EnsureNoInlineValue(name, inlineValue);
                    noColor = true;
                    break;
                case "--verbose":
                    EnsureNoInlineValue(name, inlineValue);
                    verbose = true;
                    break;
                case "--help":
                    EnsureNoInlineValue(name, inlineValue);
                    showHelp = true;
                    break;
                case "--token":
                    token = RequiredValue(args, ref index, name, inlineValue);
                    break;
                case "--token-file":
                    tokenFile = RequiredValue(args, ref index, name, inlineValue);
                    break;
                case "--token-env":
                    tokenEnv = RequiredValue(args, ref index, name, inlineValue);
                    break;
                case "--protected-token-file":
                    protectedTokenFile = RequiredValue(args, ref index, name, inlineValue);
                    break;
                default:
                    command.Add(raw);
                    break;
            }
        }

        return new DesktopNodeCliOptions(
            apiBaseUrl,
            format,
            noColor,
            verbose,
            showHelp,
            token,
            tokenFile,
            tokenEnv,
            protectedTokenFile,
            command);
    }

    private static DesktopNodeCliOutputFormat ParseFormat(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "table" => DesktopNodeCliOutputFormat.Table,
            "json" => DesktopNodeCliOutputFormat.Json,
            "plain" => DesktopNodeCliOutputFormat.Plain,
            "csv" => DesktopNodeCliOutputFormat.Csv,
            _ => throw Usage($"Unsupported output format '{value}'.")
        };
    }

    private static (string Name, string? InlineValue) SplitOption(string raw)
    {
        var equalsIndex = raw.IndexOf('=', StringComparison.Ordinal);
        if (equalsIndex < 0)
        {
            return (raw, null);
        }

        return (raw[..equalsIndex], raw[(equalsIndex + 1)..]);
    }

    private static string RequiredValue(IReadOnlyList<string> args, ref int index, string name, string? inlineValue)
    {
        if (inlineValue is not null)
        {
            if (string.IsNullOrWhiteSpace(inlineValue))
            {
                throw Usage($"Missing value for {name}.");
            }

            return inlineValue;
        }

        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw Usage($"Missing value for {name}.");
        }

        return args[++index];
    }

    private static void EnsureNoInlineValue(string name, string? inlineValue)
    {
        if (inlineValue is not null)
        {
            throw Usage($"{name} does not accept a value.");
        }
    }

    private static ArgumentException Usage(string message)
    {
        return new ArgumentException("PCV_CLI_USAGE|" + message);
    }
}

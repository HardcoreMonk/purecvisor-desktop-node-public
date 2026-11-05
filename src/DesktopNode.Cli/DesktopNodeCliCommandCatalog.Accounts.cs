using System.Text.Json;

namespace DesktopNode.Cli;

public static partial class DesktopNodeCliCommandCatalog
{
    private static DesktopNodeCliRequest AccountResetPassword(IReadOnlyList<string> args)
    {
        const string usage = "account reset-password NAME --password-env VAR|--password-stdin --yes";
        if (args.Count < 3 || args[2].StartsWith("--", StringComparison.Ordinal))
        {
            throw Usage("Use: " + usage + ".");
        }

        RejectUnexpectedPositionals(args.Skip(3).ToArray());
        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: true);
        RejectRawPasswordOptions(parsed.Options);
        if (!HasFlag(parsed.Options, "--yes"))
        {
            throw new ArgumentException(
                "PCV_CLI_CONFIRMATION_REQUIRED|" +
                "Account password reset requires explicit confirmation.|" +
                "Use: pcvcli " + usage + ".");
        }

        var username = args[2];
        var password = ReadPasswordChoice(parsed.Options);
        var body = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["confirm_username"] = username,
            ["new_password"] = password ?? string.Empty
        };
        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/accounts/{Segment(username)}/password",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest AccountChangePassword(IReadOnlyList<string> args)
    {
        const string usage = "account change-password --current-password-env VAR --password-env VAR";
        RejectUnexpectedPositionals(args.Skip(2).ToArray());
        var parsed = ParseOptions(args.Skip(2).ToArray(), allowFlags: true);
        RejectRawPasswordOptions(parsed.Options);
        foreach (var key in parsed.Options.Keys)
        {
            if (!Is(key, "--current-password-env") && !Is(key, "--password-env"))
            {
                throw Usage("Use: " + usage + ".");
            }
        }

        var currentEnv = Required(parsed.Options, "--current-password-env");
        var passwordEnv = Required(parsed.Options, "--password-env");
        var body = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["current_password"] = Environment.GetEnvironmentVariable(currentEnv) ?? string.Empty,
            ["new_password"] = Environment.GetEnvironmentVariable(passwordEnv) ?? string.Empty
        };
        return new DesktopNodeCliRequest(
            "POST",
            "/api/v1/auth/password",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static string? ReadPasswordChoice(IReadOnlyDictionary<string, string?> options)
    {
        var passwordEnv = FirstOption(options, "--password-env");
        var passwordStdin = HasFlag(options, "--password-stdin");
        if (!string.IsNullOrWhiteSpace(passwordEnv) == passwordStdin)
        {
            throw Usage("Use exactly one of --password-env VAR or --password-stdin.");
        }

        if (passwordStdin)
        {
            return Console.IsInputRedirected ? Console.In.ReadLine() : null;
        }

        return Environment.GetEnvironmentVariable(passwordEnv!);
    }

    private static void RejectUnexpectedPositionals(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            var name = args[index];
            if (!name.StartsWith("--", StringComparison.Ordinal))
            {
                throw Usage("Unexpected positional argument.");
            }

            if (index + 1 < args.Count && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                index++;
            }
        }
    }

    private static void RejectRawPasswordOptions(IReadOnlyDictionary<string, string?> options)
    {
        foreach (var key in options.Keys)
        {
            var name = key.Trim();
            var equals = name.IndexOf('=', StringComparison.Ordinal);
            if (equals >= 0)
            {
                name = name[..equals];
            }

            if (Is(name, "--password") || Is(name, "--current-password") || Is(name, "--new-password"))
            {
                throw Usage("Passwords are not accepted as arguments. Use --password-env or --password-stdin.");
            }
        }
    }
}

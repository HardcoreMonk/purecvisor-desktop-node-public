using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Cli;

// With --json the CLI also writes errors as one JSON envelope on stdout so automation can read them.
// stderr keeps the human-readable lines (code=..., Next action: ...) and the exit code is unchanged.
public static class DesktopNodeCliErrorJson
{
    public const string Operation = "pcvcli";

    private static readonly Regex CodePrefix = new("^(PCV_[A-Z0-9_]+)\\|(.*)$", RegexOptions.CultureInvariant | RegexOptions.Singleline);

    public static bool Requested(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(args[index], "--format", StringComparison.Ordinal) &&
                index + 1 < args.Count &&
                string.Equals(args[index + 1], "json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static string FromResponse(DesktopNodeCliTransportResponse response)
    {
        var body = response.Body?.Trim() ?? string.Empty;
        try
        {
            if (JsonNode.Parse(body) is JsonObject root)
            {
                if (root["error"] is JsonObject)
                {
                    root["ok"] ??= false;
                    return root.ToJsonString();
                }

                if (root["code"] is not null)
                {
                    return Envelope(root);
                }
            }
        }
        catch (JsonException)
        {
        }

        return Create(
            $"PCV_CLI_HTTP_{response.StatusCode}",
            body.Length == 0 ? $"HTTP {response.StatusCode}" : body);
    }

    public static string FromMessage(string message, string fallbackCode)
    {
        var text = message.Trim();
        var match = CodePrefix.Match(text);
        return match.Success
            ? Create(match.Groups[1].Value, match.Groups[2].Value)
            : Create(fallbackCode, text);
    }

    private static string Create(string code, string message) =>
        Envelope(new JsonObject
        {
            ["code"] = code,
            ["message"] = message,
            ["retryable"] = false
        });

    private static string Envelope(JsonObject error) =>
        new JsonObject
        {
            ["ok"] = false,
            ["operation"] = Operation,
            ["error"] = error
        }.ToJsonString();
}

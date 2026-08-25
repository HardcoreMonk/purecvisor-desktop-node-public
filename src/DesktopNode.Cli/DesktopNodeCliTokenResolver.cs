using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Cli;

public static class DesktopNodeCliTokenResolver
{
    internal static readonly byte[] ProtectionEntropy = Encoding.UTF8.GetBytes("PureCVisor Desktop Node API Token Store v1");

    public static string? Resolve(
        DesktopNodeCliOptions options,
        Func<string, string?>? environment = null,
        string? defaultProtectedTokenFilePath = null)
    {
        var sources = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            sources.Add("--token");
        }

        if (!string.IsNullOrWhiteSpace(options.TokenFile))
        {
            sources.Add("--token-file");
        }

        if (!string.IsNullOrWhiteSpace(options.TokenEnv))
        {
            sources.Add("--token-env");
        }

        if (!string.IsNullOrWhiteSpace(options.ProtectedTokenFile))
        {
            sources.Add("--protected-token-file");
        }

        if (sources.Count > 1)
        {
            throw new ArgumentException("PCV_CLI_TOKEN_SOURCE_CONFLICT|Choose exactly one token source: " + string.Join(", ", sources) + ".");
        }

        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            return options.Token;
        }

        if (!string.IsNullOrWhiteSpace(options.TokenEnv))
        {
            var readEnvironment = environment ?? Environment.GetEnvironmentVariable;
            var value = readEnvironment(options.TokenEnv);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("PCV_CLI_TOKEN_ENV_EMPTY|Token environment variable is empty: " + options.TokenEnv + ".");
            }

            return value.Trim();
        }

        if (!string.IsNullOrWhiteSpace(options.TokenFile))
        {
            return ReadPlainTokenFile(options.TokenFile);
        }

        if (!string.IsNullOrWhiteSpace(options.ProtectedTokenFile))
        {
            return ReadProtectedTokenFile(options.ProtectedTokenFile);
        }

        var defaultPath = ResolveDefaultProtectedTokenFilePath(defaultProtectedTokenFilePath);
        if (!string.IsNullOrWhiteSpace(defaultPath) && File.Exists(defaultPath))
        {
            return ReadProtectedTokenFile(defaultPath);
        }

        return null;
    }

    private static string? ResolveDefaultProtectedTokenFilePath(string? overridePath)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return overridePath;
        }

        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        return string.IsNullOrWhiteSpace(programData)
            ? null
            : Path.Combine(programData, "PureCVisor", "desktop-node", "api-token.dpapi.json");
    }

    private static string ReadPlainTokenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException("PCV_CLI_TOKEN_FILE_NOT_FOUND|Token file was not found.");
        }

        var value = File.ReadAllText(path).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PCV_CLI_TOKEN_FILE_EMPTY|Token file is empty.");
        }

        return value;
    }

    private static string ReadProtectedTokenFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("PCV_CLI_PROTECTED_TOKEN_FILE_NOT_FOUND|Protected token file was not found.");
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            var storage = RequiredString(root, "storage");
            var scope = RequiredString(root, "scope");
            if (!string.Equals(storage, "dpapi-local-machine", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(scope, "LocalMachine", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("PCV_CLI_PROTECTED_TOKEN_UNSUPPORTED|Protected token file uses an unsupported storage scope.");
            }

            var protectedToken = Convert.FromBase64String(RequiredString(root, "protected_token"));
            var clearBytes = ProtectedData.Unprotect(protectedToken, ProtectionEntropy, DataProtectionScope.LocalMachine);
            var value = Encoding.UTF8.GetString(clearBytes).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("PCV_CLI_PROTECTED_TOKEN_EMPTY|Protected token value is empty.");
            }

            return value;
        }
        catch (Exception error) when (
            error is UnauthorizedAccessException or
            SecurityException or
            JsonException or
            FormatException or
            CryptographicException)
        {
            throw NormalizeProtectedTokenReadException(error);
        }
    }

    internal static ArgumentException NormalizeProtectedTokenReadException(Exception error)
    {
        return error switch
        {
            UnauthorizedAccessException or SecurityException => new ArgumentException(
                "PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED|Protected token file access was denied.",
                error),
            JsonException or FormatException => new ArgumentException(
                "PCV_CLI_PROTECTED_TOKEN_INVALID|Protected token file is invalid.",
                error),
            CryptographicException => new ArgumentException(
                "PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED|Protected token file could not be decrypted.",
                error),
            _ => throw new ArgumentException("Unsupported protected token read failure.", nameof(error))
        };
    }

    private static string RequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException("PCV_CLI_PROTECTED_TOKEN_INVALID|Protected token file is missing " + propertyName + ".");
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PCV_CLI_PROTECTED_TOKEN_INVALID|Protected token file has an empty " + propertyName + ".");
        }

        return value;
    }
}

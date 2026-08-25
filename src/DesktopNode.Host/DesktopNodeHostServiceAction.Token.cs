using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Host;

public static partial class DesktopNodeHostServiceAction
{
    private static string? EnsureResultTokenPath(DesktopNodeHostOptions options)
    {
        if (options.ServiceAction is not ("configure-installed" or "repair-installed"))
        {
            return null;
        }

        return Path.Combine(options.DataRoot!, "api-token.dpapi.json");
    }

    public static string EnsureProtectedTokenFile(string dataRoot)
    {
        return EnsureProtectedTokenFile(dataRoot, DesktopNodeHostFileAclHardener.Instance);
    }

    internal static string EnsureProtectedTokenFile(
        string dataRoot,
        IDesktopNodeHostFileAclHardener fileAclHardener)
    {
        ArgumentNullException.ThrowIfNull(fileAclHardener);
        Directory.CreateDirectory(dataRoot);
        var path = Path.Combine(dataRoot, "api-token.dpapi.json");
        if (File.Exists(path))
        {
            _ = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = path
            });
            return path;
        }

        var token = CreateToken();
        _ = WriteProtectedTokenFile(path, token, DateTimeOffset.UtcNow, fileAclHardener);
        return path;
    }

    public static void EnsureAccountAuthBootstrapFiles(string dataRoot)
    {
        EnsureAccountAuthBootstrapFiles(dataRoot, DesktopNodeHostFileAclHardener.Instance);
    }

    internal static void EnsureAccountAuthBootstrapFiles(
        string dataRoot,
        IDesktopNodeHostFileAclHardener fileAclHardener)
    {
        ArgumentNullException.ThrowIfNull(fileAclHardener);
        Directory.CreateDirectory(dataRoot);
        var accountFile = Path.Combine(dataRoot, "accounts.json");
        var signingKeyFile = Path.Combine(dataRoot, "jwt-signing-key.txt");

        if (!File.Exists(accountFile))
        {
            var record = new SortedDictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["issuer"] = "purecvisor-desktop-node",
                ["audience"] = "desktop-node-local-api",
                ["accounts"] = Array.Empty<object>(),
                ["bootstrap_state"] = "no-default-account"
            };
            File.WriteAllText(accountFile, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
            fileAclHardener.Harden(accountFile);
        }

        if (!File.Exists(signingKeyFile))
        {
            File.WriteAllText(signingKeyFile, CreateToken(), Encoding.UTF8);
            fileAclHardener.Harden(signingKeyFile);
        }
    }

    // internal: DesktopNodeServiceTokenOps' rotation/revoke path writes the replacement token
    // file through this helper (Ops -> ServiceAction), keeping the six-member public token
    // surface's implementation on this type per the Task 9 caution.
    internal static string WriteProtectedTokenFile(
        string path,
        string token,
        DateTimeOffset createdAt,
        IDesktopNodeHostFileAclHardener? fileAclHardener)
    {
        var protectedToken = Convert.ToBase64String(DesktopNodeHostTokenResolver.ProtectForLocalMachine(token));
        var tokenSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        var record = new SortedDictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["storage"] = "dpapi-local-machine",
            ["scope"] = "LocalMachine",
            ["created_at"] = createdAt.ToString("o"),
            ["token_sha256"] = tokenSha256,
            ["protected_token"] = protectedToken
        };
        File.WriteAllText(path, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
        fileAclHardener?.Harden(path);

        return tokenSha256;
    }

    internal static string? ReadProtectedTokenSha256(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.TryGetProperty("token_sha256", out var tokenSha256Element)
            ? tokenSha256Element.GetString()
            : null;
    }

    internal static string CreateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Host;

public sealed record DesktopNodeHostResolvedToken(
    string? Value,
    string Source,
    string Storage,
    string? Path,
    string? CredentialTarget = null);

public static class DesktopNodeHostTokenResolver
{
    private static readonly byte[] ProtectedTokenEntropy =
        Encoding.UTF8.GetBytes("PureCVisor Desktop Node API Token Store v1");

    public static DesktopNodeHostResolvedToken Resolve(
        DesktopNodeHostOptions options,
        IDesktopNodeWindowsCredentialManagerController? credentialManagerController = null)
    {
        var hasTokenFile = !string.IsNullOrWhiteSpace(options.ApiTokenFile);
        var hasProtectedTokenFile = !string.IsNullOrWhiteSpace(options.ApiTokenProtectedFile);
        var hasCredentialTarget = !string.IsNullOrWhiteSpace(options.ApiTokenCredentialTarget);

        var sourceCount = (hasTokenFile ? 1 : 0) + (hasProtectedTokenFile ? 1 : 0) + (hasCredentialTarget ? 1 : 0);
        if (sourceCount > 1)
        {
            throw new ArgumentException("PCV_API_TOKEN_CONFLICT|Specify only one API token source.|Use --api-token-credential-target for service mode.");
        }

        if (sourceCount == 0)
        {
            return new DesktopNodeHostResolvedToken(null, "none", "none", null);
        }

        if (hasCredentialTarget)
        {
            return ResolveCredentialManagerTarget(
                options.ApiTokenCredentialTarget!,
                credentialManagerController ?? new DesktopNodeWindowsCredentialManagerController());
        }

        if (hasProtectedTokenFile)
        {
            return ResolveProtectedTokenFile(options.ApiTokenProtectedFile!);
        }

        return ResolveTokenFile(options.ApiTokenFile!);
    }

    public static byte[] ProtectForLocalMachine(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("PCV_SERVICE_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token.");
        }

        return ProtectedData.Protect(
            Encoding.UTF8.GetBytes(token),
            ProtectedTokenEntropy,
            DataProtectionScope.LocalMachine);
    }

    private static DesktopNodeHostResolvedToken ResolveTokenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"PCV_API_TOKEN_FILE_NOT_FOUND|The API token file was not found.|Create the token file before starting the listener: '{path}'.", path);
        }

        var token = File.ReadAllText(path).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"PCV_API_TOKEN_FILE_EMPTY|The API token file is empty.|Write a non-empty bearer token into '{path}'.");
        }

        return new DesktopNodeHostResolvedToken(token, "file", "external_token_file", path);
    }

    private static DesktopNodeHostResolvedToken ResolveProtectedTokenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"PCV_SERVICE_PROTECTED_TOKEN_FILE_NOT_FOUND|The protected API token file was not found.|Create the protected token file before starting the listener: '{path}'.", path);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var schemaVersion = root.TryGetProperty("schema_version", out var schema)
            ? schema.GetInt32()
            : 0;
        var storage = root.TryGetProperty("storage", out var storageElement)
            ? storageElement.GetString()
            : null;
        var scope = root.TryGetProperty("scope", out var scopeElement)
            ? scopeElement.GetString()
            : null;
        var protectedToken = root.TryGetProperty("protected_token", out var protectedTokenElement)
            ? protectedTokenElement.GetString()
            : null;

        if (schemaVersion != 1 ||
            storage != "dpapi-local-machine" ||
            scope != "LocalMachine" ||
            string.IsNullOrWhiteSpace(protectedToken))
        {
            throw new InvalidOperationException($"PCV_SERVICE_PROTECTED_TOKEN_FILE_INVALID|The protected API token file schema is invalid.|Expected schema_version 1, storage dpapi-local-machine, scope LocalMachine, and protected_token in '{path}'.");
        }

        var token = UnprotectLocalMachine(protectedToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"PCV_SERVICE_PROTECTED_TOKEN_EMPTY|The protected service API token resolved to an empty value.|Rotate the protected token file: '{path}'.");
        }

        return new DesktopNodeHostResolvedToken(token, "protected_file", "dpapi-local-machine", path);
    }

    private static DesktopNodeHostResolvedToken ResolveCredentialManagerTarget(
        string credentialTarget,
        IDesktopNodeWindowsCredentialManagerController credentialManagerController)
    {
        var token = credentialManagerController.ReadToken(credentialTarget);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"PCV_SERVICE_CREDENTIAL_MANAGER_TOKEN_EMPTY|The Windows Credential Manager token resolved to an empty value.|Rotate the credential target: '{credentialTarget}'.");
        }

        return new DesktopNodeHostResolvedToken(
            token,
            "credential_manager",
            "windows-credential-manager",
            null,
            credentialTarget);
    }

    private static string UnprotectLocalMachine(string protectedToken)
    {
        try
        {
            var tokenBytes = ProtectedData.Unprotect(
                Convert.FromBase64String(protectedToken),
                ProtectedTokenEntropy,
                DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(tokenBytes);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            throw new InvalidOperationException($"PCV_SERVICE_PROTECTED_TOKEN_UNPROTECT_FAILED|The protected service API token could not be read.|{ex.Message}", ex);
        }
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DesktopNode.Cli;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliTokenResolverTests
{
    [Fact]
    public void NormalizesProtectedTokenAccessDeniedWithoutPathOrSid()
    {
        var error = new UnauthorizedAccessException(
            @"Access denied: C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json S-1-5-21-1234");

        var normalized = DesktopNodeCliTokenResolver.NormalizeProtectedTokenReadException(error);

        Assert.Equal(
            "PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED|Protected token file access was denied.",
            normalized.Message);
        Assert.DoesNotContain("C:\\ProgramData", normalized.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("S-1-5-21", normalized.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("base64")]
    public void RejectsMalformedProtectedTokenPayloadWithStableCode(string kind)
    {
        var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "api-token.dpapi.json");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(
                path,
                kind == "json"
                    ? "{not-json"
                    : "{\"storage\":\"dpapi-local-machine\",\"scope\":\"LocalMachine\",\"protected_token\":\"%%%\"}");

            var options = DesktopNodeCliOptions.Parse(["--protected-token-file", path, "host", "status"]);
            var error = Assert.Throws<ArgumentException>(() =>
                DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null));

            Assert.StartsWith("PCV_CLI_PROTECTED_TOKEN_INVALID|", error.Message);
            Assert.DoesNotContain(path, error.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void NormalizesDpapiFailureWithStableCode()
    {
        var normalized = DesktopNodeCliTokenResolver.NormalizeProtectedTokenReadException(
            new CryptographicException("machine-specific DPAPI diagnostic"));

        Assert.Equal(
            "PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED|Protected token file could not be decrypted.",
            normalized.Message);
        Assert.DoesNotContain("machine-specific", normalized.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolvesInlineToken()
    {
        var options = DesktopNodeCliOptions.Parse(["--token", "inline-secret", "host", "status"]);

        var token = DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null);

        Assert.Equal("inline-secret", token);
    }

    [Fact]
    public void ResolvesEnvironmentToken()
    {
        var options = DesktopNodeCliOptions.Parse(["--token-env", "PCV_TEST_TOKEN", "host", "status"]);

        var token = DesktopNodeCliTokenResolver.Resolve(options, environment: name => name == "PCV_TEST_TOKEN" ? "env-secret" : null);

        Assert.Equal("env-secret", token);
    }

    [Fact]
    public void ResolvesPlainTokenFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "token.txt");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, "file-secret\r\n");

        try
        {
            var options = DesktopNodeCliOptions.Parse(["--token-file", path, "host", "status"]);

            var token = DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null);

            Assert.Equal("file-secret", token);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResolvesProtectedTokenFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "protected-token.json");
        Directory.CreateDirectory(directory);
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes("protected-secret"),
            DesktopNodeCliTokenResolver.ProtectionEntropy,
            DataProtectionScope.LocalMachine);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schema_version = 1,
            storage = "dpapi-local-machine",
            scope = "LocalMachine",
            protected_token = Convert.ToBase64String(encrypted)
        }));

        try
        {
            var options = DesktopNodeCliOptions.Parse(["--protected-token-file", path, "host", "status"]);

            var token = DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null);

            Assert.Equal("protected-secret", token);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResolvesDefaultProtectedTokenFileWhenNoTokenSourceIsSpecified()
    {
        var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "api-token.dpapi.json");
        Directory.CreateDirectory(directory);
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes("default-protected-secret"),
            DesktopNodeCliTokenResolver.ProtectionEntropy,
            DataProtectionScope.LocalMachine);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schema_version = 1,
            storage = "dpapi-local-machine",
            scope = "LocalMachine",
            protected_token = Convert.ToBase64String(encrypted)
        }));

        try
        {
            var options = DesktopNodeCliOptions.Parse(["host", "status"]);

            var token = DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null, defaultProtectedTokenFilePath: path);

            Assert.Equal("default-protected-secret", token);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void IgnoresMissingDefaultProtectedTokenFileWhenNoTokenSourceIsSpecified()
    {
        var options = DesktopNodeCliOptions.Parse(["host", "status"]);
        var missingPath = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"), "api-token.dpapi.json");

        var token = DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null, defaultProtectedTokenFilePath: missingPath);

        Assert.Null(token);
    }

    [Fact]
    public void RejectsMultipleTokenSources()
    {
        var options = DesktopNodeCliOptions.Parse(["--token", "one", "--token-env", "PCV_TOKEN", "host", "status"]);

        var error = Assert.Throws<ArgumentException>(() => DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null));

        Assert.Contains("PCV_CLI_TOKEN_SOURCE_CONFLICT", error.Message);
    }
}

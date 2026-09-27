using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;

namespace DesktopNode.Api.Tests;

public sealed class ApiAccountMutationRequestProcessorTests
{
    [Fact]
    public void LoopbackSessionCanCreateFirstAdminAndList()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();

        var loopback = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));
        Assert.Equal(200, loopback.StatusCode);
        var token = ReadAccessToken(loopback.Body);

        var created = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts",
            """{"username":"lab-admin","password":"correct-horse","role":"admin","display_name":"Lab Admin"}""",
            Authorization: "Bearer " + token,
            RemoteIsLoopback: true));
        Assert.Equal(200, created.StatusCode);
        Assert.DoesNotContain("correct-horse", created.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("password_hash", created.Body, StringComparison.Ordinal);
        using var createdDocument = JsonDocument.Parse(created.Body);
        Assert.Equal("lab-admin", createdDocument.RootElement.GetProperty("data").GetProperty("username").GetString());
        Assert.Equal(
            AccountMutationContract.BootstrapAccountsConfigured,
            createdDocument.RootElement.GetProperty("data").GetProperty("bootstrap_state").GetString());

        var list = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/accounts",
            Authorization: "Bearer " + token,
            RemoteIsLoopback: true));
        Assert.Equal(409, list.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.BootstrapNotAvailable, list.Body, StringComparison.Ordinal);

        var login = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            """{"username":"lab-admin","password":"correct-horse"}"""));
        Assert.Equal(200, login.StatusCode);
        var adminToken = ReadAccessToken(login.Body);

        var listed = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/accounts",
            Authorization: "Bearer " + adminToken));
        Assert.Equal(200, listed.StatusCode);
        using var listedDocument = JsonDocument.Parse(listed.Body);
        var accounts = listedDocument.RootElement.GetProperty("data").GetProperty("accounts");
        Assert.Equal(1, accounts.GetArrayLength());
        Assert.False(accounts[0].TryGetProperty("password_hash", out _));
    }

    [Fact]
    public void ServiceBearerCanBootstrapAndDisableOperator()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();

        var created = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts",
            """{"username":"lab-admin","password":"correct-horse","role":"admin"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(200, created.StatusCode);

        var operatorCreated = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts",
            """{"username":"lab-operator","password":"correct-horse","role":"operator"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(200, operatorCreated.StatusCode);

        var disabled = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/lab-operator/disable",
            """{"confirm_username":"lab-operator"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(200, disabled.StatusCode);
        using var document = JsonDocument.Parse(disabled.Body);
        Assert.Equal(AccountMutationContract.ActionDisable, document.RootElement.GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void DisableWithoutAuthIsForbiddenWhenConfigured()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        Assert.Equal(200, processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts",
            """{"username":"lab-admin","password":"correct-horse","role":"admin"}""",
            ServiceBearerAccepted: true)).StatusCode);

        var disabled = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/lab-admin/disable",
            """{"confirm_username":"lab-admin"}"""));
        Assert.Equal(403, disabled.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.ManageForbidden, disabled.Body, StringComparison.Ordinal);
    }

    private static string ReadAccessToken(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("data").GetProperty("access_token").GetString()!;
    }

    private sealed class TempAccountStore : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "pcv-account-http-" + Guid.NewGuid().ToString("N"));
        public string AccountFile => Path.Combine(Root, "accounts.json");
        public string SigningKeyFile => Path.Combine(Root, "jwt-signing-key.txt");

        public DesktopNodeApiRequestProcessor CreateProcessor()
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(AccountFile, """{"schema_version":1,"issuer":"purecvisor-desktop-node","audience":"desktop-node-local-api","accounts":[],"bootstrap_state":"no-default-account"}""");
            File.WriteAllText(SigningKeyFile, "test-signing-key-material");
            return DesktopNodeApiRequestProcessor.CreateDefault(
                accountAuthOptions: DesktopNodeAccountAuthOptions.FromFiles(AccountFile, SigningKeyFile));
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}

using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeAccountAuthStoreTests
{
    [Fact]
    public void BootstrapCreatePersistsFirstAdminAndEnablesLogin()
    {
        using var store = new TempAccountStore();
        var hardened = new List<string>();
        var service = store.CreateService(harden: hardened.Add);

        var created = service.CreateAccount(
            "lab-admin",
            "correct-horse",
            "admin",
            BootstrapAuth(),
            "Lab Admin");

        Assert.True(created.Ok);
        Assert.True(service.Ready);
        Assert.False(service.CanIssueLoopbackSession);
        Assert.Equal(store.AccountFile, Assert.Single(hardened));

        using var file = JsonDocument.Parse(File.ReadAllText(store.AccountFile));
        var root = file.RootElement;
        Assert.Equal(AccountMutationContract.BootstrapAccountsConfigured, root.GetProperty("bootstrap_state").GetString());
        Assert.Equal("keep-me", root.GetProperty("lab_note").GetString());
        var account = Assert.Single(root.GetProperty("accounts").EnumerateArray());
        Assert.Equal("lab-admin", account.GetProperty("username").GetString());
        Assert.Equal("admin", account.GetProperty("role").GetString());
        Assert.True(account.GetProperty("enabled").GetBoolean());
        Assert.DoesNotContain("correct-horse", File.ReadAllText(store.AccountFile), StringComparison.Ordinal);
        Assert.StartsWith("pbkdf2-sha256$", account.GetProperty("password_hash").GetString());

        var login = service.Login(Body("""{"username":"lab-admin","password":"correct-horse"}"""));
        Assert.True(login.Ok);
        var loopback = service.CreateLoopbackSession(remoteIsLoopback: true);
        Assert.False(loopback.Ok);
        Assert.Equal("PCV_LOOPBACK_SESSION_DISABLED", loopback.Error!.Code);
    }

    [Fact]
    public void CreateWithoutAccountFilePathIsRejected()
    {
        var service = new DesktopNodeAccountAuthService(new DesktopNodeAccountAuthOptions(
            Enabled: true,
            SigningKey: "test-signing-key-material"));

        var result = service.CreateAccount("lab-admin", "correct-horse", "admin", BootstrapAuth());
        Assert.False(result.Ok);
        Assert.Equal("PCV_ACCOUNT_AUTH_CONFIG_INCOMPLETE", result.Error!.Code);
    }

    [Fact]
    public void DisablePersistsAndRejectsLoginWithoutLeakingAccountPresence()
    {
        using var store = new TempAccountStore();
        var now = DateTimeOffset.Parse("2026-09-20T12:00:00Z");
        var service = store.CreateService(clock: () => now);

        Assert.True(service.CreateAccount("lab-admin", "correct-horse", "admin", BootstrapAuth()).Ok);
        Assert.True(service.CreateAccount(
            "lab-operator",
            "correct-horse",
            "operator",
            new AccountMutationAuthContext(HasAccountManage: true)).Ok);

        var disable = service.DisableAccount(
            "lab-operator",
            "lab-operator",
            new AccountMutationAuthContext(HasAccountManage: true));
        Assert.True(disable.Ok);

        using var file = JsonDocument.Parse(File.ReadAllText(store.AccountFile));
        var operatorAccount = file.RootElement.GetProperty("accounts").EnumerateArray()
            .Single(item => item.GetProperty("username").GetString() == "lab-operator");
        Assert.False(operatorAccount.GetProperty("enabled").GetBoolean());
        Assert.Equal(now, operatorAccount.GetProperty("disabled_at").GetDateTimeOffset());

        var login = service.Login(Body("""{"username":"lab-operator","password":"correct-horse"}"""));
        Assert.False(login.Ok);
        Assert.Equal("PCV_LOGIN_FAILED", login.Error!.Code);
    }

    [Fact]
    public void DisableLastAdminLeavesFileUnchanged()
    {
        using var store = new TempAccountStore();
        var service = store.CreateService();
        Assert.True(service.CreateAccount("lab-admin", "correct-horse", "admin", BootstrapAuth()).Ok);
        var before = File.ReadAllText(store.AccountFile);

        var disable = service.DisableAccount(
            "lab-admin",
            "lab-admin",
            new AccountMutationAuthContext(HasAccountManage: true));

        Assert.False(disable.Ok);
        Assert.Equal(AccountMutationProblemCodes.LastAdmin, disable.Error!.Code);
        Assert.Equal(before, File.ReadAllText(store.AccountFile));
        Assert.True(service.Login(Body("""{"username":"lab-admin","password":"correct-horse"}""")).Ok);
    }

    [Fact]
    public void DisabledAccountRefreshIsNotFound()
    {
        using var store = new TempAccountStore();
        var service = store.CreateService();
        Assert.True(service.CreateAccount("lab-admin", "correct-horse", "admin", BootstrapAuth()).Ok);
        Assert.True(service.CreateAccount(
            "lab-operator",
            "correct-horse",
            "operator",
            new AccountMutationAuthContext(HasAccountManage: true)).Ok);

        var login = service.Login(Body("""{"username":"lab-operator","password":"correct-horse"}"""));
        Assert.True(login.Ok);
        using var loginDocument = JsonDocument.Parse(JsonSerializer.Serialize(login.Data));
        var refreshToken = loginDocument.RootElement.GetProperty("refresh_token").GetString();

        Assert.True(service.DisableAccount(
            "lab-operator",
            "lab-operator",
            new AccountMutationAuthContext(HasAccountManage: true)).Ok);

        var refresh = service.Refresh(Body($"{{\"refresh_token\":\"{refreshToken}\"}}"));
        Assert.False(refresh.Ok);
        Assert.Equal("PCV_REFRESH_ACCOUNT_NOT_FOUND", refresh.Error!.Code);
    }

    [Fact]
    public void DisableIsIdempotentWhenAlreadyDisabled()
    {
        using var store = new TempAccountStore();
        var service = store.CreateService();
        Assert.True(service.CreateAccount("lab-admin", "correct-horse", "admin", BootstrapAuth()).Ok);
        Assert.True(service.CreateAccount(
            "lab-operator",
            "correct-horse",
            "operator",
            new AccountMutationAuthContext(HasAccountManage: true)).Ok);
        Assert.True(service.DisableAccount(
            "lab-operator",
            "lab-operator",
            new AccountMutationAuthContext(HasAccountManage: true)).Ok);

        var again = service.DisableAccount(
            "lab-operator",
            "LAB-OPERATOR",
            new AccountMutationAuthContext(HasAccountManage: true));
        Assert.True(again.Ok);
        using var data = JsonDocument.Parse(JsonSerializer.Serialize(again.Data));
        Assert.Equal(AccountMutationContract.ActionAlreadyDisabled, data.RootElement.GetProperty("action").GetString());
    }

    private static AccountMutationAuthContext BootstrapAuth()
    {
        return new AccountMutationAuthContext(RemoteIsLoopback: true, IsLoopbackSession: true);
    }

    private static JsonElement Body(string json)
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private sealed class TempAccountStore : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "pcv-account-store-" + Guid.NewGuid().ToString("N"));
        public string AccountFile => Path.Combine(Root, "accounts.json");
        public string SigningKeyFile => Path.Combine(Root, "jwt-signing-key.txt");

        public DesktopNodeAccountAuthService CreateService(
            Action<string>? harden = null,
            Func<DateTimeOffset>? clock = null)
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(
                AccountFile,
                """
                {
                  "schema_version": 1,
                  "issuer": "purecvisor-desktop-node",
                  "audience": "desktop-node-local-api",
                  "accounts": [],
                  "bootstrap_state": "no-default-account",
                  "lab_note": "keep-me"
                }
                """);
            File.WriteAllText(SigningKeyFile, "test-signing-key-material");
            var options = DesktopNodeAccountAuthOptions.FromFiles(AccountFile, SigningKeyFile) with
            {
                HardenAccountFile = harden,
                Clock = clock
            };
            return new DesktopNodeAccountAuthService(options);
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

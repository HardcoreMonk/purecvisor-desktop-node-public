using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Api;
using DesktopNode.Contracts;

namespace DesktopNode.Api.Tests;

public sealed class ApiAccountPasswordRequestProcessorTests
{
    private const string CurrentPassword = "correct-horse";
    private const string NextPassword = "correct-horse-next";
    private const string LongUsername = "lab-admin-one";

    [Fact]
    public void PasswordPolicyFailureReturnsInvalid()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, LongUsername, CurrentPassword, "admin");

        var tooShort = Reset(processor, LongUsername, LongUsername, "short-pass", serviceBearer: true);
        Assert.Equal(400, tooShort.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordInvalid, tooShort.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("short-pass", tooShort.Body, StringComparison.Ordinal);

        var sameName = Reset(processor, LongUsername, LongUsername, LongUsername, serviceBearer: true);
        Assert.Equal(400, sameName.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordInvalid, sameName.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(LongUsername, sameName.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("operator")]
    [InlineData("viewer")]
    public void OperatorAndViewerCannotResetPassword(string role)
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        CreateAccount(processor, "lab-member", CurrentPassword, role);
        var token = Login(processor, "lab-member", CurrentPassword).Access;

        var reset = Reset(processor, "lab-admin", "lab-admin", NextPassword, authorization: token);
        Assert.Equal(403, reset.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.ManageForbidden, reset.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(NextPassword, reset.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ServiceBearerCanResetPassword()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");

        var reset = Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true);
        Assert.Equal(200, reset.StatusCode);
        Assert.DoesNotContain(NextPassword, reset.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("password_hash", reset.Body, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(reset.Body);
        Assert.Equal("lab-admin", document.RootElement.GetProperty("data").GetProperty("username").GetString());
        Assert.True(document.RootElement.GetProperty("data").GetProperty("enabled").GetBoolean());
        Assert.False(document.RootElement.GetProperty("data").TryGetProperty("access_token", out _));
    }

    [Fact]
    public void LoopbackSessionCannotResetOrChangePassword()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        var loopback = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));
        Assert.Equal(200, loopback.StatusCode);
        var loopbackToken = ReadToken(loopback.Body, "access_token");
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin", authorization: loopbackToken, remoteIsLoopback: true);

        var reset = Reset(processor, "lab-admin", "lab-admin", NextPassword, authorization: loopbackToken, remoteIsLoopback: true);
        Assert.Equal(409, reset.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.BootstrapNotAvailable, reset.Body, StringComparison.Ordinal);

        var change = Change(processor, CurrentPassword, NextPassword, authorization: loopbackToken);
        Assert.Equal(403, change.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordChangeForbidden, change.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(NextPassword, change.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ServiceBearerCannotChangePassword()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");

        var change = Change(processor, CurrentPassword, NextPassword, serviceBearer: true);
        Assert.Equal(403, change.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordChangeForbidden, change.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentPassword, change.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfirmUsernameMismatchReturns400()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");

        var mismatch = Reset(processor, "lab-admin", "other-admin", NextPassword, serviceBearer: true);
        Assert.Equal(400, mismatch.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.ConfirmationMismatch, mismatch.Body, StringComparison.Ordinal);

        var missing = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/lab-admin/password",
            $$"""{"new_password":"{{NextPassword}}"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(400, missing.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.ConfirmationMismatch, missing.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(NextPassword, missing.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ChangePasswordRejectsCurrentPasswordMismatchBeforePolicy()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var token = Login(processor, "lab-admin", CurrentPassword).Access;

        var mismatch = Change(processor, "wrong-current-value", "short", authorization: token);
        Assert.Equal(401, mismatch.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordMismatch, mismatch.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("wrong-current-value", mismatch.Body, StringComparison.Ordinal);

        var invalidNext = Change(processor, CurrentPassword, "short", authorization: token);
        Assert.Equal(400, invalidNext.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.PasswordInvalid, invalidNext.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ResetAndChangeInvalidatePreviousAccessAndRefreshTokens()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var first = Login(processor, "lab-admin", CurrentPassword);
        Assert.Equal(200, Session(processor, first.Access).StatusCode);

        var reset = Reset(processor, "lab-admin", "lab-admin", NextPassword, authorization: first.Access);
        Assert.Equal(200, reset.StatusCode);
        AssertStale(Session(processor, first.Access));
        AssertStale(Refresh(processor, first.Refresh));

        var second = Login(processor, "lab-admin", NextPassword);
        Assert.Equal(200, Session(processor, second.Access).StatusCode);
        var changed = Change(processor, NextPassword, CurrentPassword, authorization: second.Access);
        Assert.Equal(200, changed.StatusCode);
        Assert.DoesNotContain(CurrentPassword, changed.Body, StringComparison.Ordinal);
        var rotated = LoginTokens(changed.Body);
        Assert.Equal(200, Session(processor, rotated.Access).StatusCode);
        AssertStale(Session(processor, second.Access));
        AssertStale(Refresh(processor, second.Refresh));
    }

    [Fact]
    public void TokenWithoutCredentialVersionClaimWorksWhileVersionIsZero()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var issued = Login(processor, "lab-admin", CurrentPassword);
        var stripped = StripCredentialVersionClaim(issued.Access);

        Assert.Equal(200, Session(processor, stripped).StatusCode);

        Assert.Equal(200, Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true).StatusCode);
        AssertStale(Session(processor, stripped));
        AssertStale(Session(processor, issued.Access));
    }

    [Fact]
    public void AccountFileKeepsHashAndOmitsPlaintext()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        Assert.Equal(200, Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true).StatusCode);

        var file = File.ReadAllText(store.AccountFile);
        Assert.DoesNotContain(NextPassword, file, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentPassword, file, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(file);
        var account = document.RootElement.GetProperty("accounts").EnumerateArray().Single();
        var hash = account.GetProperty("password_hash").GetString()!;
        Assert.StartsWith("pbkdf2-sha256$100000$", hash, StringComparison.Ordinal);
        Assert.Equal(4, hash.Split('$').Length);
        Assert.Equal(1, account.GetProperty("credential_version").GetInt32());
        Assert.True(account.GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public void LastAdminCanBeResetWithoutChangingEnabled()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");

        var reset = Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true);
        Assert.Equal(200, reset.StatusCode);
        Assert.DoesNotContain(AccountMutationProblemCodes.LastAdmin, reset.Body, StringComparison.Ordinal);

        var listed = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/accounts",
            ServiceBearerAccepted: true));
        Assert.Equal(200, listed.StatusCode);
        using var document = JsonDocument.Parse(listed.Body);
        var account = document.RootElement.GetProperty("data").GetProperty("accounts")[0];
        Assert.True(account.GetProperty("enabled").GetBoolean());
        Assert.False(account.TryGetProperty("password_hash", out _));
        Assert.False(account.TryGetProperty("credential_version", out _));
    }

    [Fact]
    public void DisabledAccountResetStaysDisabled()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        CreateAccount(processor, "lab-operator", CurrentPassword, "operator");
        var disabled = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/lab-operator/disable",
            """{"confirm_username":"lab-operator"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(200, disabled.StatusCode);

        var reset = Reset(processor, "lab-operator", "lab-operator", NextPassword, serviceBearer: true);
        Assert.Equal(200, reset.StatusCode);
        using var resetDocument = JsonDocument.Parse(reset.Body);
        Assert.False(resetDocument.RootElement.GetProperty("data").GetProperty("enabled").GetBoolean());

        using var file = JsonDocument.Parse(File.ReadAllText(store.AccountFile));
        var account = file.RootElement.GetProperty("accounts").EnumerateArray()
            .Single(item => item.GetProperty("username").GetString() == "lab-operator");
        Assert.False(account.GetProperty("enabled").GetBoolean());
        Assert.True(account.TryGetProperty("disabled_at", out _));
    }

    [Fact]
    public void AuditRecordOmitsPasswordValue()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var admin = Login(processor, "lab-admin", CurrentPassword);
        Assert.Equal(200, Reset(processor, "lab-admin", "lab-admin", NextPassword, authorization: admin.Access).StatusCode);

        var auditPath = Path.Combine(store.Root, DesktopNodeAccountAuthService.AccountMutationAuditFileName);
        var line = File.ReadAllLines(auditPath).Single();
        Assert.DoesNotContain(NextPassword, line, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentPassword, line, StringComparison.Ordinal);
        Assert.DoesNotContain("new_password", line, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(line);
        Assert.Equal("account.password.reset", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("lab-admin", document.RootElement.GetProperty("actor").GetString());
        Assert.Equal("lab-admin", document.RootElement.GetProperty("target").GetString());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("at").GetString()));
        Assert.Equal(4, document.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public void ServiceBearerResetAuditUsesServiceActor()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        Assert.Equal(200, Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true).StatusCode);

        var line = File.ReadAllLines(Path.Combine(store.Root, DesktopNodeAccountAuthService.AccountMutationAuditFileName)).Single();
        using var document = JsonDocument.Parse(line);
        Assert.Equal("service-bearer", document.RootElement.GetProperty("actor").GetString());
        Assert.DoesNotContain(NextPassword, line, StringComparison.Ordinal);
    }

    [Fact]
    public void CredentialInvalidationSurvivesNewInstance()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var issued = Login(processor, "lab-admin", CurrentPassword);
        Assert.Equal(200, Reset(processor, "lab-admin", "lab-admin", NextPassword, serviceBearer: true).StatusCode);

        var restarted = store.OpenProcessor();
        AssertStale(Session(restarted, issued.Access));
        AssertStale(Refresh(restarted, issued.Refresh));
        Assert.Equal(200, Session(restarted, Login(restarted, "lab-admin", NextPassword).Access).StatusCode);
    }

    [Fact]
    public void DisableInvalidatesPreviousTokensWithoutBumpingAnAlreadyDisabledAccount()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        CreateAccount(processor, "lab-operator", CurrentPassword, "operator");
        var issued = Login(processor, "lab-operator", CurrentPassword);
        Assert.Equal(200, Session(processor, issued.Access).StatusCode);
        Assert.Equal(0, ReadCredentialVersion(store.AccountFile, "lab-operator"));

        var disabled = Disable(processor, "lab-operator");
        Assert.Equal(200, disabled.StatusCode);
        Assert.DoesNotContain(CurrentPassword, disabled.Body, StringComparison.Ordinal);
        using (var disabledDocument = JsonDocument.Parse(disabled.Body))
        {
            Assert.Equal(AccountMutationContract.ActionDisable, disabledDocument.RootElement.GetProperty("data").GetProperty("action").GetString());
        }

        Assert.Equal(1, ReadCredentialVersion(store.AccountFile, "lab-operator"));
        Assert.Equal(0, ReadCredentialVersion(store.AccountFile, "lab-admin"));
        AssertStale(Session(processor, issued.Access));
        AssertStale(Refresh(processor, issued.Refresh));

        var afterDisable = File.ReadAllText(store.AccountFile);
        var again = Disable(processor, "lab-operator");
        Assert.Equal(200, again.StatusCode);
        using (var againDocument = JsonDocument.Parse(again.Body))
        {
            Assert.Equal(
                AccountMutationContract.ActionAlreadyDisabled,
                againDocument.RootElement.GetProperty("data").GetProperty("action").GetString());
        }

        Assert.Equal(afterDisable, File.ReadAllText(store.AccountFile));
        Assert.Equal(1, ReadCredentialVersion(store.AccountFile, "lab-operator"));
        Assert.False(File.Exists(Path.Combine(store.Root, DesktopNodeAccountAuthService.AccountMutationAuditFileName)));

        var restarted = store.OpenProcessor();
        AssertStale(Session(restarted, issued.Access));
        AssertStale(Refresh(restarted, issued.Refresh));

        var beforeLastAdmin = File.ReadAllText(store.AccountFile);
        var lastAdmin = Disable(restarted, "lab-admin");
        Assert.Equal(409, lastAdmin.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.LastAdmin, lastAdmin.Body, StringComparison.Ordinal);
        Assert.Equal(beforeLastAdmin, File.ReadAllText(store.AccountFile));
        Assert.Equal(0, ReadCredentialVersion(store.AccountFile, "lab-admin"));
    }

    [Fact]
    public void MissingAccountReturnsNotFound()
    {
        using var store = new TempAccountStore();
        var processor = store.CreateProcessor();
        CreateAccount(processor, "lab-admin", CurrentPassword, "admin");
        var token = Login(processor, "lab-admin", CurrentPassword).Access;

        var reset = Reset(processor, "missing-user", "missing-user", NextPassword, serviceBearer: true);
        Assert.Equal(404, reset.StatusCode);
        Assert.Contains(AccountMutationProblemCodes.NotFound, reset.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(NextPassword, reset.Body, StringComparison.Ordinal);

        var session = Session(processor, token);
        Assert.Equal(200, session.StatusCode);
    }

    private static void CreateAccount(
        DesktopNodeApiRequestProcessor processor,
        string username,
        string password,
        string role,
        string? authorization = null,
        bool remoteIsLoopback = false)
    {
        var created = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts",
            $$"""{"username":"{{username}}","password":"{{password}}","role":"{{role}}"}""",
            Authorization: authorization is null ? null : "Bearer " + authorization,
            ServiceBearerAccepted: authorization is null,
            RemoteIsLoopback: remoteIsLoopback));
        Assert.Equal(200, created.StatusCode);
    }

    private static IssuedTokens Login(DesktopNodeApiRequestProcessor processor, string username, string password)
    {
        var login = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            $$"""{"username":"{{username}}","password":"{{password}}"}"""));
        Assert.Equal(200, login.StatusCode);
        Assert.DoesNotContain(password, login.Body, StringComparison.Ordinal);
        return LoginTokens(login.Body);
    }

    private static DesktopNodeApiResponse Reset(
        DesktopNodeApiRequestProcessor processor,
        string username,
        string confirm,
        string password,
        bool serviceBearer = false,
        string? authorization = null,
        bool remoteIsLoopback = false)
    {
        return processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/" + username + "/password",
            $$"""{"confirm_username":"{{confirm}}","new_password":"{{password}}"}""",
            Authorization: authorization is null ? null : "Bearer " + authorization,
            ServiceBearerAccepted: serviceBearer,
            RemoteIsLoopback: remoteIsLoopback));
    }

    private static DesktopNodeApiResponse Change(
        DesktopNodeApiRequestProcessor processor,
        string currentPassword,
        string newPassword,
        bool serviceBearer = false,
        string? authorization = null)
    {
        return processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/password",
            $$"""{"current_password":"{{currentPassword}}","new_password":"{{newPassword}}"}""",
            Authorization: authorization is null ? null : "Bearer " + authorization,
            ServiceBearerAccepted: serviceBearer));
    }

    private static DesktopNodeApiResponse Session(DesktopNodeApiRequestProcessor processor, string accessToken)
    {
        return processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            Authorization: "Bearer " + accessToken));
    }

    private static DesktopNodeApiResponse Disable(DesktopNodeApiRequestProcessor processor, string username)
    {
        return processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/accounts/" + username + "/disable",
            $$"""{"confirm_username":"{{username}}"}""",
            ServiceBearerAccepted: true));
    }

    private static int ReadCredentialVersion(string accountFile, string username)
    {
        using var file = JsonDocument.Parse(File.ReadAllText(accountFile));
        var account = file.RootElement.GetProperty("accounts").EnumerateArray()
            .Single(item => item.GetProperty("username").GetString() == username);
        return account.TryGetProperty("credential_version", out var version) ? version.GetInt32() : 0;
    }

    private static DesktopNodeApiResponse Refresh(DesktopNodeApiRequestProcessor processor, string refreshToken)
    {
        return processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            $$"""{"refresh_token":"{{refreshToken}}"}"""));
    }

    private static void AssertStale(DesktopNodeApiResponse response)
    {
        Assert.Equal(401, response.StatusCode);
        Assert.Contains("PCV_TOKEN_CREDENTIAL_STALE", response.Body, StringComparison.Ordinal);
    }

    private static IssuedTokens LoginTokens(string body)
    {
        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");
        return new IssuedTokens(
            data.GetProperty("access_token").GetString()!,
            data.GetProperty("refresh_token").GetString()!);
    }

    private static string ReadToken(string body, string name)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("data").GetProperty(name).GetString()!;
    }

    private static string StripCredentialVersionClaim(string token)
    {
        var parts = token.Split('.');
        var json = Encoding.UTF8.GetString(DesktopNodeAccountPassword.Base64UrlDecode(parts[1]));
        var stripped = Regex.Replace(json, ",\"cv\":\\d+", "");
        if (string.Equals(stripped, json, StringComparison.Ordinal))
        {
            stripped = Regex.Replace(json, "\"cv\":\\d+,", "");
        }

        Assert.NotEqual(json, stripped);
        var payloadSegment = DesktopNodeAccountPassword.Base64UrlEncode(Encoding.UTF8.GetBytes(stripped));
        var signingInput = parts[0] + "." + payloadSegment;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-signing-key-material"));
        var signature = DesktopNodeAccountPassword.Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(signingInput)));
        return signingInput + "." + signature;
    }

    private readonly record struct IssuedTokens(string Access, string Refresh);

    private sealed class TempAccountStore : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "pcv-account-password-" + Guid.NewGuid().ToString("N"));
        public string AccountFile => Path.Combine(Root, "accounts.json");
        public string SigningKeyFile => Path.Combine(Root, "jwt-signing-key.txt");

        public DesktopNodeApiRequestProcessor CreateProcessor()
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(AccountFile, """{"schema_version":1,"issuer":"purecvisor-desktop-node","audience":"desktop-node-local-api","accounts":[],"bootstrap_state":"no-default-account"}""");
            File.WriteAllText(SigningKeyFile, "test-signing-key-material");
            return OpenProcessor();
        }

        public DesktopNodeApiRequestProcessor OpenProcessor()
        {
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

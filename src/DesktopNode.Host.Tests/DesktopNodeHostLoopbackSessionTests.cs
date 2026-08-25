using System.Net;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostLoopbackSessionTests
{
    private const string ServiceTokenValue = "loopback-session-service-secret";

    [Fact]
    public async Task LoopbackSessionThenRuntimePolicySucceedsWithoutServiceToken()
    {
        using var host = await StartNotConfiguredHostAsync();
        using var client = new HttpClient();
        using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
        var issuedBody = await issue.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
        using var document = JsonDocument.Parse(issuedBody);
        var access = document.RootElement.GetProperty("data").GetProperty("access_token").GetString();

        using var policyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        policyRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + access);
        using var policy = await client.SendAsync(policyRequest);
        Assert.Equal(HttpStatusCode.OK, policy.StatusCode);
    }

    [Fact]
    public async Task LoopbackSessionDoesNotChangeEmptyAccountsFile()
    {
        var (host, accountFile) = await StartNotConfiguredHostWithAccountFileAsync();
        using (host)
        {
            using var client = new HttpClient();
            using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
            Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
            using var accounts = JsonDocument.Parse(await File.ReadAllTextAsync(accountFile));
            Assert.Equal("no-default-account", accounts.RootElement.GetProperty("bootstrap_state").GetString());
            Assert.Equal(0, accounts.RootElement.GetProperty("accounts").GetArrayLength());
        }
    }

    [Fact]
    public async Task ReadyHostDisablesLoopbackSession()
    {
        using var host = await StartReadyHostAsync();
        using var client = new HttpClient();
        using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
        var body = await issue.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Conflict, issue.StatusCode);
        Assert.Contains("PCV_LOOPBACK_SESSION_DISABLED", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WebConfigScriptDoesNotContainTokens()
    {
        var webRoot = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(webRoot, "index.html"), "<html></html>");
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            WebPrefix = "http://127.0.0.1:0/",
            WebRootPath = webRoot,
            ApiTokenFile = WriteServiceTokenFile(),
            AccountAuthOptions = NotConfiguredOptions()
        });
        using var client = new HttpClient();
        var script = await client.GetStringAsync(new Uri(host.WebBaseUri, "/pcv-config.js"));
        Assert.Contains("apiBaseUrl", script, StringComparison.Ordinal);
        Assert.DoesNotContain("access_token", script, StringComparison.Ordinal);
        Assert.DoesNotContain("protected_token", script, StringComparison.Ordinal);
        Assert.DoesNotContain(ServiceTokenValue, script, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadyHostRejectsPreviousLoopbackAccessToken()
    {
        using var issuer = await StartNotConfiguredHostAsync();
        using var client = new HttpClient();
        using var issue = await client.PostAsync(new Uri(issuer.BaseUri, "/api/v1/auth/loopback-session"), null);
        using var document = JsonDocument.Parse(await issue.Content.ReadAsStringAsync());
        var access = document.RootElement.GetProperty("data").GetProperty("access_token").GetString();
        issuer.Dispose();

        using var ready = await StartReadyHostAsync();
        using var policyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(ready.BaseUri, "/api/v1/runtime/policy"));
        policyRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + access);
        using var policy = await client.SendAsync(policyRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, policy.StatusCode);
        Assert.Contains("PCV_LOOPBACK_SESSION_DISABLED", await policy.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ForwardedForHeaderDoesNotBypassLoopbackCheckOnIssuePath()
    {
        using var host = await StartNotConfiguredHostAsync();
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/loopback-session"));
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.9");
        using var issue = await client.SendAsync(request);
        // Real remote is still 127.0.0.1, so issue stays 200. The header must not cause 403.
        Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
    }

    [Fact]
    public async Task LoginWithoutBearerOnNotConfiguredHostReturnsAccountNotConfigured()
    {
        using var host = await StartNotConfiguredHostAsync();
        using var client = new HttpClient();
        using var login = await client.PostAsync(
            new Uri(host.BaseUri, "/api/v1/auth/login"),
            new StringContent(
                """{"username":"operator","password":"correct horse battery staple"}""",
                System.Text.Encoding.UTF8,
                "application/json"));
        var body = await login.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Conflict, login.StatusCode);
        Assert.Contains("PCV_ACCOUNT_AUTH_NOT_CONFIGURED", body, StringComparison.Ordinal);
        Assert.DoesNotContain("PCV_AUTH_REQUIRED", body, StringComparison.Ordinal);
    }

    private static async Task<DesktopNodeHostApplication> StartNotConfiguredHostAsync()
    {
        return await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            ApiTokenFile = WriteServiceTokenFile(),
            AccountAuthOptions = NotConfiguredOptions()
        });
    }

    private static async Task<(DesktopNodeHostApplication Host, string AccountFile)> StartNotConfiguredHostWithAccountFileAsync()
    {
        var accountFile = Path.Combine(Path.GetTempPath(), "pcv-host-loopback-accounts-" + Guid.NewGuid().ToString("N") + ".json");
        var signingKeyFile = Path.Combine(Path.GetTempPath(), "pcv-host-loopback-jwt-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(
            accountFile,
            """
            {
              "schema_version": 1,
              "issuer": "pcv-test",
              "audience": "pcv-local-api",
              "accounts": [],
              "bootstrap_state": "no-default-account"
            }
            """);
        await File.WriteAllTextAsync(signingKeyFile, SyntheticAuthMaterial.Value);

        var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            ApiTokenFile = WriteServiceTokenFile(),
            AccountFilePath = accountFile,
            JwtSigningKeyFilePath = signingKeyFile
        });
        return (host, accountFile);
    }

    private static async Task<DesktopNodeHostApplication> StartReadyHostAsync()
    {
        return await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            ApiTokenFile = WriteServiceTokenFile(),
            AccountAuthOptions = ReadyOptions()
        });
    }

    private static DesktopNodeAccountAuthOptions NotConfiguredOptions()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts: []);
    }

    private static DesktopNodeAccountAuthOptions ReadyOptions()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    "operator",
                    "operator",
                    DesktopNodeAccountPassword.HashPassword("correct horse battery staple", "pcv-test-salt"),
                    "operator",
                    "operator user")
            ]);
    }

    private static string WriteServiceTokenFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-host-loopback-token-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, ServiceTokenValue);
        return path;
    }
}

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostApplicationTests
{
    [Fact]
    public async Task RuntimePolicyEndpointReturnsManagedCoreContract()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        });

        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("runtime.policy", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("dotnet", document.RootElement.GetProperty("data").GetProperty("job_runtime").GetProperty("managed_core").GetProperty("candidate").GetString());
    }

    [Fact]
    public async Task FailedSecondListenerBindCleansUpFirstListener()
    {
        var apiPort = GetFreeLoopbackPort();
        var webPort = GetFreeLoopbackPort();
        var apiPrefix = $"http://127.0.0.1:{apiPort}/";
        var webPrefix = $"http://127.0.0.1:{webPort}/";
        using var occupiedWebListener = new HttpListener();
        occupiedWebListener.Prefixes.Add(webPrefix);
        occupiedWebListener.Start();

        await Assert.ThrowsAsync<HttpListenerException>(() => DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = apiPrefix,
            WebPrefix = webPrefix
        }));

        using var apiProbe = new HttpListener();
        apiProbe.Prefixes.Add(apiPrefix);
        apiProbe.Start();
    }

    [Fact]
    public async Task TrackedAsyncAdmissionRejectsBeforeCapacityIsAvailable()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            RequestLifetimeMode = DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized,
            RequestAdmissionActiveLimit = 1,
            RequestAdmissionWaitingLimit = 0,
            // Keep the first lease active long enough for a busy CI runner to
            // observe it and issue the rejection request before it is released.
            ControlledRouteTimeoutProbeDelayMilliseconds = 10_000
        });

        using var client = new HttpClient();
        var first = client.GetAsync(
            new Uri(host.BaseUri, "/api/v1/runtime/route-timeout-probe"),
            HttpCompletionOption.ResponseHeadersRead);

        var admissionDeadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (host.RequestAdmissionActiveCount < 1 && DateTimeOffset.UtcNow < admissionDeadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal(1, host.RequestAdmissionActiveCount);

        using var rejected = await client.GetAsync(
            new Uri(host.BaseUri, "/api/v1/runtime/policy"),
            HttpCompletionOption.ResponseHeadersRead);
        var rejectedBody = await rejected.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, rejected.StatusCode);
        Assert.Equal("15", rejected.Headers.RetryAfter?.Delta?.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        using (var rejectedDocument = JsonDocument.Parse(rejectedBody))
        {
            Assert.Equal(
                "PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED",
                rejectedDocument.RootElement.GetProperty("code").GetString());
        }

        using var firstResponse = await first;
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
    }

    [Fact]
    public async Task HostStartsBackgroundWorkerForQueuedJobs()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        });

        using var client = new HttpClient();
        using var create = await client.PostAsync(
            new Uri(host.BaseUri, "/api/v1/vms/pcv-invalid%5Cname/start"),
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Accepted, create.StatusCode);
        using var createdDocument = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var jobId = createdDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        string? status = null;
        string? errorCode = null;
        do
        {
            using var get = await client.GetAsync(new Uri(host.BaseUri, $"/api/v1/jobs/{jobId}"));
            using var getDocument = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
            var data = getDocument.RootElement.GetProperty("data");
            status = data.GetProperty("status").GetString();
            if (status is "succeeded" or "failed")
            {
                errorCode = data.GetProperty("error").GetProperty("code").GetString();
                break;
            }

            await Task.Delay(100);
        } while (DateTimeOffset.UtcNow < deadline);

        Assert.Equal("failed", status);
        Assert.Equal("PCV_VM_NAME_INVALID", errorCode);
    }

    [Fact]
    public async Task UnknownJobCommandsReturnConsistentNotFoundContract()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        });
        using var client = new HttpClient();

        foreach (var (method, path) in new[]
        {
            (HttpMethod.Get, "/api/v1/jobs/job-missing"),
            (HttpMethod.Post, "/api/v1/jobs/job-missing/cancel"),
            (HttpMethod.Post, "/api/v1/jobs/job-missing/retry")
        })
        {
            using var request = new HttpRequestMessage(method, new Uri(host.BaseUri, path));
            using var response = await client.SendAsync(request);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("PCV_JOB_NOT_FOUND", document.RootElement.GetProperty("error").GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task OpsSummaryIncludesBatchEvidenceFromConfiguredRoot()
    {
        var evidenceRoot = Path.Combine(Path.GetTempPath(), "pcv-host-batch-evidence-" + Guid.NewGuid().ToString("N"));
        var batchRun = Path.Combine(evidenceRoot, "host-option-batch-run");
        var routeRoot = Path.Combine(evidenceRoot, "routeparity-service-msi-hyperv-host-option");
        var osRoot = Path.Combine(evidenceRoot, "os-mutation-gates-host-option");
        Directory.CreateDirectory(batchRun);
        Directory.CreateDirectory(routeRoot);
        Directory.CreateDirectory(osRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "host-option-batch-run",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            await File.WriteAllTextAsync(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"status":"collected","adapter_memory":[{"mib":1}],"process_memory":[{"mib":1}]}
            """);
            await File.WriteAllTextAsync(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","boot_time_unchanged":true,"final_service":{"state":"Running"}}
            """);
            await File.WriteAllTextAsync(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.38.4-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.38.4-admin-smoke"},"git_commit":"6bbb39f0a3a271e4a1187ce7de2014e009977425","msi":{"sha256":"7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7"},"signing_mode":"AllowUnsignedDev"}
            """);
            await File.WriteAllTextAsync(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true}]}
            """);
            await File.WriteAllTextAsync(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                BatchEvidenceRootPath = evidenceRoot
            });

            using var client = new HttpClient();
            using var response = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/ops/summary"));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            Assert.True(evidence.GetProperty("configured").GetBoolean());
            Assert.Equal("available", evidence.GetProperty("status").GetString());
            Assert.Equal("host-option-batch-run", evidence.GetProperty("latest").GetProperty("batch_id").GetString());
        }
        finally
        {
            Directory.Delete(evidenceRoot, recursive: true);
        }
    }

    [Fact]
    public async Task StaticRootServesIndexHtmlFromWebRoot()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>desktop node</html>");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot
            });

            using var client = new HttpClient();
            using var response = await client.GetAsync(host.BaseUri);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("desktop node", body);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SeparateWebPrefixServesStaticAwayFromApiPort()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>separate desktop node</html>");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebPrefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot
            });

            Assert.NotEqual(host.BaseUri.Port, host.WebBaseUri.Port);

            using var client = new HttpClient();
            using var staticResponse = await client.GetAsync(host.WebBaseUri);
            var staticBody = await staticResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, staticResponse.StatusCode);
            Assert.Contains("separate desktop node", staticBody);

            using var apiRootResponse = await client.GetAsync(host.BaseUri);
            Assert.Equal(HttpStatusCode.NotFound, apiRootResponse.StatusCode);

            using var webApiResponse = await client.GetAsync(new Uri(host.WebBaseUri, "/api/v1/runtime/policy"));
            var webApiBody = await webApiResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, webApiResponse.StatusCode);
            Assert.Contains("PCV_API_ROUTE_ON_WEB_PORT", webApiBody);
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SeparateWebPrefixPublishesApiBaseConfigScript()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>config desktop node</html>");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebPrefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot
            });

            using var client = new HttpClient();
            using var response = await client.GetAsync(new Uri(host.WebBaseUri, "/pcv-config.js"));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/javascript", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("PCV_DESKTOP_NODE_CONFIG", body);
            Assert.Contains(host.BaseUri.GetLeftPart(UriPartial.Authority), body);
            Assert.DoesNotContain(host.WebBaseUri.GetLeftPart(UriPartial.Authority), body);
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SeparateWebPrefixAllowsCorsFromConfiguredWebOrigin()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(webRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>cors desktop node</html>");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebPrefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot
            });

            using var client = new HttpClient();
            var webOrigin = host.WebBaseUri.GetLeftPart(UriPartial.Authority);
            using var preflight = new HttpRequestMessage(HttpMethod.Options, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            preflight.Headers.TryAddWithoutValidation("Origin", webOrigin);
            preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
            preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "Authorization, Content-Type");
            using var preflightResponse = await client.SendAsync(preflight);

            Assert.Equal(HttpStatusCode.NoContent, preflightResponse.StatusCode);
            Assert.True(preflightResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var originValues));
            Assert.Equal(webOrigin, originValues.Single());
            Assert.True(preflightResponse.Headers.TryGetValues("Access-Control-Allow-Headers", out var headerValues));
            Assert.Contains("Authorization", headerValues.Single());

            using var apiRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            apiRequest.Headers.TryAddWithoutValidation("Origin", webOrigin);
            using var apiResponse = await client.SendAsync(apiRequest);

            Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
            Assert.True(apiResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var apiOriginValues));
            Assert.Equal(webOrigin, apiOriginValues.Single());
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ApiRouteRequiresBearerTokenWhenTokenFileIsConfigured()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-secret\r\n");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath
            });

            using var client = new HttpClient();
            using var missing = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);

            using var wrongRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            wrongRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer wrong");
            using var wrong = await client.SendAsync(wrongRequest);
            Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);

            using var correctRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            correctRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer host-secret");
            using var correct = await client.SendAsync(correctRequest);
            var body = await correct.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, correct.StatusCode);
            using var document = JsonDocument.Parse(body);
            Assert.Equal("external_token_file", document.RootElement.GetProperty("data").GetProperty("auth").GetProperty("token_storage").GetString());
            Assert.DoesNotContain("host-secret", body);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task AccountAuthBootstrapPathsRequireServiceBearerUntilAccountsAreConfigured()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        var accountFile = Path.Combine(Path.GetTempPath(), "pcv-host-accounts-" + Guid.NewGuid().ToString("N") + ".json");
        var signingKeyFile = Path.Combine(Path.GetTempPath(), "pcv-host-jwt-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-secret\r\n");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                AccountFilePath = accountFile,
                JwtSigningKeyFilePath = signingKeyFile
            });

            using var client = new HttpClient();
            using var missing = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer host-secret");
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("account_rbac_jwt_not_configured", body);
            Assert.DoesNotContain("host-secret", body, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(tokenPath);
            if (File.Exists(accountFile)) File.Delete(accountFile);
            if (File.Exists(signingKeyFile)) File.Delete(signingKeyFile);
        }
    }

    [Fact]
    public async Task AccountLoginBypassesServiceBearerGateAndJwtSessionPassesThrough()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-secret\r\n");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                AccountAuthOptions = new DesktopNodeAccountAuthOptions(
                    Enabled: true,
                    Issuer: "pcv-host-test",
                    Audience: "pcv-local-api",
                    SigningKey: SyntheticAuthMaterial.Value,
                    Accounts:
                    [
                        new DesktopNodeAccountUser(
                            Id: "operator",
                            Username: "operator",
                            PasswordHash: DesktopNodeAccountPassword.HashPassword("operator-password", "pcv-host-test-salt"),
                            Role: "operator",
                            DisplayName: "Operator")
                    ])
            });

            using var client = new HttpClient();
            using var login = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/login"),
                new StringContent("""{"username":"operator","password":"operator-password"}""", System.Text.Encoding.UTF8, "application/json"));
            var loginBody = await login.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            using var loginDocument = JsonDocument.Parse(loginBody);
            var accessToken = loginDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString();

            using var sessionRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/auth/session"));
            sessionRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
            using var session = await client.SendAsync(sessionRequest);
            var sessionBody = await session.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, session.StatusCode);
            Assert.Contains("auth.session", sessionBody);
            Assert.DoesNotContain("operator-password", loginBody + sessionBody, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task AccountJwtRefreshLogoutAndRbacFlowPassesThroughHostListener()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-secret\r\n");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                AccountAuthOptions = new DesktopNodeAccountAuthOptions(
                    Enabled: true,
                    Issuer: "pcv-host-lifecycle-test",
                    Audience: "pcv-local-api",
                    SigningKey: SyntheticAuthMaterial.Value,
                    Accounts:
                    [
                        new DesktopNodeAccountUser(
                            Id: "operator",
                            Username: "operator",
                            PasswordHash: DesktopNodeAccountPassword.HashPassword("operator-password", "pcv-host-lifecycle-salt"),
                            Role: "operator",
                            DisplayName: "Operator")
                    ])
            });

            using var client = new HttpClient();
            using var missingLogin = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/login"));
            using var missingLoginResponse = await client.SendAsync(missingLogin);
            Assert.Equal(HttpStatusCode.BadRequest, missingLoginResponse.StatusCode);

            using var login = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/login"),
                new StringContent(
                    """{"username":"operator","password":"operator-password"}""",
                    Encoding.UTF8,
                    "application/json"));
            var loginBody = await login.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            using var loginDocument = JsonDocument.Parse(loginBody);
            var initialAccessToken = loginDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString()!;
            var initialRefreshToken = loginDocument.RootElement.GetProperty("data").GetProperty("refresh_token").GetString()!;

            using var rbacRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/auth/rbac"));
            rbacRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {initialAccessToken}");
            using var rbac = await client.SendAsync(rbacRequest);
            var rbacBody = await rbac.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, rbac.StatusCode);
            Assert.Contains("account.manage", rbacBody, StringComparison.Ordinal);

            using var refresh = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/refresh"),
                new StringContent(
                    JsonSerializer.Serialize(new { refresh_token = initialRefreshToken }),
                    Encoding.UTF8,
                    "application/json"));
            var refreshBody = await refresh.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
            using var refreshDocument = JsonDocument.Parse(refreshBody);
            var rotatedAccessToken = refreshDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString()!;
            var rotatedRefreshToken = refreshDocument.RootElement.GetProperty("data").GetProperty("refresh_token").GetString()!;

            using var replay = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/refresh"),
                new StringContent(
                    JsonSerializer.Serialize(new { refresh_token = initialRefreshToken }),
                    Encoding.UTF8,
                    "application/json"));
            var replayBody = await replay.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
            Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", replayBody, StringComparison.Ordinal);
            Assert.DoesNotContain(initialRefreshToken, replayBody, StringComparison.Ordinal);

            using var guestRequest = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(host.BaseUri, "/api/v1/vms/alpha/guest/exec/preview"))
            {
                Content = new StringContent(
                    """{"command":["powershell","--password=host-secret-value"]}""",
                    Encoding.UTF8,
                    "application/json")
            };
            guestRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {rotatedAccessToken}");
            using var guest = await client.SendAsync(guestRequest);
            var guestBody = await guest.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Forbidden, guest.StatusCode);
            Assert.Contains("PCV_GUEST_EXEC_PERMISSION_DENIED", guestBody, StringComparison.Ordinal);
            Assert.DoesNotContain("host-secret-value", guestBody, StringComparison.Ordinal);

            using var logout = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/logout"),
                new StringContent(
                    JsonSerializer.Serialize(new { refresh_token = rotatedRefreshToken }),
                    Encoding.UTF8,
                    "application/json"));
            var logoutBody = await logout.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
            Assert.Contains("\"refresh_token_revoked\":true", logoutBody, StringComparison.Ordinal);
            Assert.DoesNotContain(rotatedRefreshToken, logoutBody, StringComparison.Ordinal);

            using var revoked = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/auth/refresh"),
                new StringContent(
                    JsonSerializer.Serialize(new { refresh_token = rotatedRefreshToken }),
                    Encoding.UTF8,
                    "application/json"));
            var revokedBody = await revoked.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Unauthorized, revoked.StatusCode);
            Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", revokedBody, StringComparison.Ordinal);

            using var sessionRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/auth/session"));
            sessionRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {rotatedAccessToken}");
            using var session = await client.SendAsync(sessionRequest);
            Assert.Equal(HttpStatusCode.OK, session.StatusCode);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task NoVncBridgeProxiesWebSocketFramesToLoopbackTcpTarget()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var targetPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        var echoTask = Task.Run(async () =>
        {
            using var tcp = await listener.AcceptTcpClientAsync();
            await using var stream = tcp.GetStream();
            var buffer = new byte[16];
            var read = await stream.ReadAsync(buffer);
            await stream.WriteAsync(buffer.AsMemory(0, read));
        });

        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-secret");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                NoVncTargetHost = "127.0.0.1",
                NoVncTargetPort = targetPort,
                NoVncWebSocketPath = "/api/v1/console/novnc/{vm_id}"
            });

            using var webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", "Bearer host-secret");
            var bridgeUri = new UriBuilder(host.BaseUri)
            {
                Scheme = "ws",
                Path = "/api/v1/console/novnc/alpha"
            }.Uri;

            await webSocket.ConnectAsync(bridgeUri, CancellationToken.None);
            var payload = Encoding.ASCII.GetBytes("RFB");
            await webSocket.SendAsync(payload, WebSocketMessageType.Binary, true, CancellationToken.None);
            var receiveBuffer = new byte[16];
            var received = await webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

            Assert.Equal(WebSocketMessageType.Binary, received.MessageType);
            Assert.Equal("RFB", Encoding.ASCII.GetString(receiveBuffer, 0, received.Count));
            await echoTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            listener.Stop();
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task StaticRootKeepsLoopbackUnauthenticatedBypassWhenTokenFileIsConfigured()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        Directory.CreateDirectory(webRoot);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>loopback static</html>");
            await File.WriteAllTextAsync(tokenPath, "host-secret");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot,
                ApiTokenFile = tokenPath
            });

            using var client = new HttpClient();
            using var response = await client.GetAsync(host.BaseUri);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("loopback static", body);
        }
        finally
        {
            File.Delete(tokenPath);
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ProtectedTokenFileCanBeUsedForBearerAuth()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".dpapi.json");
        var protectedToken = Convert.ToBase64String(DesktopNodeHostTokenResolver.ProtectForLocalMachine("protected-host-secret"));
        var tokenJson = $$"""
        {
          "schema_version": 1,
          "storage": "dpapi-local-machine",
          "scope": "LocalMachine",
          "protected_token": "{{protectedToken}}"
        }
        """;
        await File.WriteAllTextAsync(tokenPath, tokenJson);
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenProtectedFile = tokenPath
            });

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer protected-host-secret");
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(body);
            Assert.Equal("dpapi-local-machine", document.RootElement.GetProperty("data").GetProperty("auth").GetProperty("token_storage").GetString());
            Assert.DoesNotContain("protected-host-secret", body);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task DiagnosticBundleRoutesWorkThroughTokenProtectedHostListener()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-token-" + Guid.NewGuid().ToString("N") + ".txt");
        var diagnosticsRoot = Path.Combine(Path.GetTempPath(), "pcv-host-diag-" + Guid.NewGuid().ToString("N"));
        await File.WriteAllTextAsync(tokenPath, "host-diag-secret");
        Directory.CreateDirectory(diagnosticsRoot);

        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                DiagnosticsRootPath = diagnosticsRoot
            });

            using var client = new HttpClient();
            using var missingAuth = await client.PostAsync(
                new Uri(host.BaseUri, "/api/v1/diagnostics/bundles"),
                new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, missingAuth.StatusCode);

            using var createRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/diagnostics/bundles"))
            {
                Content = new StringContent(
                    """{"token":"super-secret","headers":{"Authorization":"Bearer super-secret"}}""",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            createRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer host-diag-secret");
            createRequest.Headers.TryAddWithoutValidation("X-PCV-Request-Id", "listener-diag-create");

            using var create = await client.SendAsync(createRequest);
            var createBody = await create.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            Assert.Equal("application/json", create.Content.Headers.ContentType?.MediaType);
            Assert.DoesNotContain("host-diag-secret", createBody, StringComparison.Ordinal);
            Assert.DoesNotContain("super-secret", createBody, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(createBody);
            Assert.Equal("listener-diag-create", document.RootElement.GetProperty("request_id").GetString());
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("code-level-api-action", data.GetProperty("actual_execution").GetString());
            Assert.Equal("token-required-route-contract", data.GetProperty("authz_status").GetString());
            var bundleId = data.GetProperty("bundle_id").GetString();
            Assert.False(string.IsNullOrWhiteSpace(bundleId));

            var archive = Directory.GetFiles(diagnosticsRoot, "*.bundle.json").Single();
            var archiveText = await File.ReadAllTextAsync(archive);
            Assert.Contains("listener-diag-create", archiveText, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", archiveText, StringComparison.Ordinal);
            Assert.DoesNotContain("super-secret", archiveText, StringComparison.Ordinal);

            using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, $"/api/v1/diagnostics/bundles/{bundleId}/download"));
            downloadRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer host-diag-secret");
            using var download = await client.SendAsync(downloadRequest);
            var downloadBody = await download.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, download.StatusCode);
            Assert.Equal("application/vnd.purecvisor.diagnostic-bundle+json", download.Content.Headers.ContentType?.MediaType);
            Assert.Equal(archiveText, downloadBody);
            Assert.True(download.Headers.TryGetValues("X-PCV-Diagnostic-Bundle-Id", out var values));
            Assert.Equal(bundleId, values.Single());
        }
        finally
        {
            File.Delete(tokenPath);
            if (Directory.Exists(diagnosticsRoot))
            {
                Directory.Delete(diagnosticsRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ApiRequestRejectsKnownLengthBodyAboveConfiguredCap()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            MaxRequestBodyBytes = DesktopNodeHostOptions.MinimumMaxRequestBodyBytes
        });

        using var client = new HttpClient();
        using var content = new StringContent("{\"payload\":\"" + new string('x', DesktopNodeHostOptions.MinimumMaxRequestBodyBytes) + "\"}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/login"), content);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("PCV_REQUEST_BODY_TOO_LARGE", body);
        Assert.Contains("recommended_action", body);

        using var health = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }

    [Fact]
    public async Task ApiRequestBodyCapRunsBeforeAccountAuthNotConfiguredWhenBearerTokenIsConfigured()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), "pcv-host-body-cap-token-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tokenPath, "host-body-cap-secret");
        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                MaxRequestBodyBytes = DesktopNodeHostOptions.MinimumMaxRequestBodyBytes
            });

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/login"));
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer host-body-cap-secret");
            request.Content = new StringContent(
                "{\"username\":\"pcv-installed-hardening-smoke\",\"password\":\"" + new string('x', DesktopNodeHostOptions.MinimumMaxRequestBodyBytes) + "\"}",
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("PCV_REQUEST_BODY_TOO_LARGE", body);
            Assert.DoesNotContain("PCV_ACCOUNT_AUTH_NOT_CONFIGURED", body);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task ApiRequestRejectsUnknownLengthBodyWhenBoundedReadExceedsCap()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            MaxRequestBodyBytes = DesktopNodeHostOptions.MinimumMaxRequestBodyBytes
        });

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/login"));
        request.Content = new PushStreamLikeContent("{\"payload\":\"" + new string('x', DesktopNodeHostOptions.MinimumMaxRequestBodyBytes) + "\"}");

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Contains("PCV_REQUEST_BODY_TOO_LARGE", body);
    }

    [Fact]
    public async Task ApiRequestClampsProgrammaticBodyCapToDocumentedMinimum()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            MaxRequestBodyBytes = 16
        });

        using var client = new HttpClient();
        using var content = new StringContent("{\"payload\":\"this body is below the documented minimum cap\"}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/login"), content);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain("PCV_REQUEST_BODY_TOO_LARGE", body);
    }

    [Fact]
    public async Task PostVmCreateForwardsRequestBodyIntoDotNetApiProcessor()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        });

        using var client = new HttpClient();
        using var response = await client.PostAsync(
            new Uri(host.BaseUri, "/api/v1/vms"),
            new StringContent(
                """{"name":"body-forwarded","iso_path":"D:\\iso\\rocky.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}""",
                System.Text.Encoding.UTF8,
                "application/json"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("vm.create", document.RootElement.GetProperty("data").GetProperty("operation").GetString());
        Assert.Equal("body-forwarded", document.RootElement.GetProperty("data").GetProperty("params").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ApiRouteReturnsRetryAfterProblemDetailsWhenRequestLimitIsExceeded()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            RequestLimitPerMinute = 1,
            RequestBurstLimit = 0,
            RetryAfterSeconds = 6
        });

        using var client = new HttpClient();
        using var first = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        using var second = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        var body = await second.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal((HttpStatusCode)429, second.StatusCode);
        Assert.Equal(6, (int?)second.Headers.RetryAfter?.Delta?.TotalSeconds);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(body);
        Assert.Equal("PCV_RATE_LIMIT_EXCEEDED", document.RootElement.GetProperty("code").GetString());
        Assert.Equal(429, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(6, document.RootElement.GetProperty("retry_after_seconds").GetInt32());
    }

    [Fact]
    public async Task ControlledRouteTimeoutProbeReturnsGatewayTimeoutWhenOptedIn()
    {
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            RouteTimeoutSeconds = 1,
            RetryAfterSeconds = 5,
            ControlledRouteTimeoutProbeDelayMilliseconds = 1_500
        });

        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/route-timeout-probe"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(5, (int?)response.Headers.RetryAfter?.Delta?.TotalSeconds);

        using var document = JsonDocument.Parse(body);
        Assert.Equal("PCV_ROUTE_TIMEOUT", document.RootElement.GetProperty("code").GetString());
        Assert.Equal("route.timeout", document.RootElement.GetProperty("operation").GetString());
    }

    private static int GetFreeLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed class PushStreamLikeContent(string body) : HttpContent
    {
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            await stream.WriteAsync(bytes);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}

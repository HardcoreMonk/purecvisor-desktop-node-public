using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHttpTransportContractTests
{
    private const string ServiceToken = "transport-service-secret";

    [Fact]
    public void FixtureConnectsHostCharacterizationToAuthoritativeRouteManifest()
    {
        using var fixture = LoadFixture();
        var root = fixture.RootElement;
        var manifestFixture = root.GetProperty("authoritative_route_manifest");
        var manifest = ApiHandlerAdapterContract.CreateDefault();
        var routeKeys = manifest.Routes
            .Select(route => $"{route.Method} {route.RouteTemplate}")
            .ToArray();

        Assert.Equal("http-transport-contract-v1", root.GetProperty("contract_key").GetString());
        Assert.Equal("System.Net.HttpListener", root.GetProperty("transport").GetString());
        Assert.Equal(manifestFixture.GetProperty("route_count").GetInt32(), manifest.Routes.Count);
        Assert.Equal(manifest.Routes.Count, routeKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            manifestFixture.GetProperty("runtime_registry_source").GetString(),
            manifest.RuntimeEvidenceContract.HandlerRegistrySource);
        Assert.Equal(
            "representative-transport-cases-not-route-catalog-duplication",
            manifestFixture.GetProperty("host_integration_scope").GetString());
        Assert.Equal(
            "discarded-before-DesktopNodeApiRequestProcessor",
            root.GetProperty("path_projection").GetProperty("query_forwarding").GetString());

        foreach (var representative in manifestFixture.GetProperty("representative_route_keys").EnumerateArray())
        {
            Assert.Contains(representative.GetString(), routeKeys);
        }

        Assert.Equal(
            [
                "web-port-api-rejection",
                "loopback-static-get-bypass",
                "web-port-non-static-rejection",
                "api-options-before-novnc-and-general-auth",
                "novnc-specific-auth",
                "general-service-token-auth",
                "static-get-after-general-auth",
                "bounded-body-read",
                "api-route-dispatch"
            ],
            root.GetProperty("request_precedence").EnumerateArray().Select(value => value.GetString()!).ToArray());

        var contractHeaders = root.GetProperty("response_headers").GetProperty("contract_headers")
            .EnumerateArray().Select(value => value.GetString()!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var transportHeaders = root.GetProperty("response_headers").GetProperty("transport_owned_non_contract_allowlist")
            .EnumerateArray().Select(value => value.GetString()!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Empty(contractHeaders.Intersect(transportHeaders, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RawTargetProjectionAndRepresentative404And405ResponsesMatchFixture()
    {
        using var fixture = LoadFixture();
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        });

        foreach (var testCase in fixture.RootElement.GetProperty("raw_target_cases").EnumerateArray())
        {
            var target = Expand(testCase.GetProperty("raw_target").GetString()!, host.BaseUri);
            var projectedUri = testCase.GetProperty("raw_target_kind").GetString() == "absolute-form"
                ? new Uri(target, UriKind.Absolute)
                : new Uri(host.BaseUri, target);
            Assert.Equal(testCase.GetProperty("absolute_path").GetString(), projectedUri.AbsolutePath);
            var normalizedPath = projectedUri.AbsolutePath.TrimEnd('/');
            Assert.Equal(
                testCase.GetProperty("normalized_path").GetString(),
                string.IsNullOrWhiteSpace(normalizedPath) ? "/" : normalizedPath);
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-PCV-Request-Id"] = testCase.GetProperty("request_id").GetString()!
            };
            var observed = await SendRawHttpAsync(
                host.BaseUri,
                testCase.GetProperty("method").GetString()!,
                target,
                headers);

            AssertExpectedResponse(fixture.RootElement, testCase, observed, host.BaseUri, host.WebBaseUri);
        }
    }

    [Fact]
    public async Task SeparatePortsAndGetOnlyStaticSurfaceMatchFixture()
    {
        using var fixture = LoadFixture();
        var webRoot = UniqueTempPath("pcv-http-transport-web");
        var tokenPath = UniqueTempPath("pcv-http-transport-token", ".txt");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>transport fixture</html>");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);

        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebPrefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot,
                ApiTokenFile = tokenPath
            });

            Assert.NotEqual(host.BaseUri.Port, host.WebBaseUri.Port);
            foreach (var testCase in fixture.RootElement.GetProperty("static_cases").EnumerateArray())
            {
                var listener = testCase.GetProperty("listener").GetString() == "web"
                    ? host.WebBaseUri
                    : host.BaseUri;
                var observed = await SendRawHttpAsync(
                    listener,
                    testCase.GetProperty("method").GetString()!,
                    testCase.GetProperty("raw_target").GetString()!);

                AssertExpectedResponse(fixture.RootElement, testCase, observed, host.BaseUri, host.WebBaseUri);
            }
        }
        finally
        {
            File.Delete(tokenPath);
            if (Directory.Exists(webRoot))
            {
                Directory.Delete(webRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task OptionsAndCorsRunBeforeServiceTokenAuthentication()
    {
        using var fixture = LoadFixture();
        var cors = fixture.RootElement.GetProperty("cors");
        var webRoot = UniqueTempPath("pcv-http-transport-cors-web");
        var tokenPath = UniqueTempPath("pcv-http-transport-cors-token", ".txt");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), "<html>cors</html>");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);

        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                WebPrefix = "http://127.0.0.1:0/",
                WebRootPath = webRoot,
                ApiTokenFile = tokenPath
            });
            var webOrigin = host.WebBaseUri.GetLeftPart(UriPartial.Authority);
            var requestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Origin"] = Expand(cors.GetProperty("request_origin").GetString()!, host.BaseUri, host.WebBaseUri),
                ["Access-Control-Request-Method"] = cors.GetProperty("requested_method").GetString()!,
                ["Access-Control-Request-Headers"] = cors.GetProperty("requested_headers").GetString()!
            };
            var observed = await SendRawHttpAsync(
                host.BaseUri,
                cors.GetProperty("method").GetString()!,
                cors.GetProperty("raw_target").GetString()!,
                requestHeaders);

            Assert.True(cors.GetProperty("bypasses_service_token").GetBoolean());
            AssertExpectedResponse(fixture.RootElement, cors, observed, host.BaseUri, host.WebBaseUri);
            foreach (var expectedHeader in cors.GetProperty("expected_headers").EnumerateObject())
            {
                Assert.Equal(
                    Expand(expectedHeader.Value.GetString()!, host.BaseUri, host.WebBaseUri),
                    observed.Headers[expectedHeader.Name]);
            }

            var authFailure = await SendRawHttpAsync(
                host.BaseUri,
                "GET",
                "/api/v1/runtime/policy",
                new Dictionary<string, string> { ["Origin"] = webOrigin });
            Assert.Equal(401, authFailure.StatusCode);
            Assert.Equal(webOrigin, authFailure.Headers["Access-Control-Allow-Origin"]);
            AssertHeadersAreContractedOrTransportOwned(fixture.RootElement, authFailure.Headers.Keys);
        }
        finally
        {
            File.Delete(tokenPath);
            if (Directory.Exists(webRoot))
            {
                Directory.Delete(webRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GeneralServiceTokenAuthenticationMatchesFixture()
    {
        using var fixture = LoadFixture();
        var tokenPath = UniqueTempPath("pcv-http-transport-service-token", ".txt");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);

        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath
            });

            using var client = new HttpClient();
            foreach (var testCase in fixture.RootElement.GetProperty("host_service_token_cases").EnumerateArray())
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
                request.Headers.TryAddWithoutValidation("X-PCV-Request-Id", testCase.GetProperty("request_id").GetString());
                if (testCase.GetProperty("authorization").ValueKind == JsonValueKind.String)
                {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        Expand(testCase.GetProperty("authorization").GetString()!, host.BaseUri));
                }

                using var response = await client.SendAsync(request);
                var observed = await ObserveAsync(response);
                AssertExpectedResponse(fixture.RootElement, testCase, observed, host.BaseUri, host.WebBaseUri);
            }
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task AccountReadyBootstrapAndAccountBearerPrecedenceMatchFixture()
    {
        using var fixture = LoadFixture();
        var tokenPath = UniqueTempPath("pcv-http-transport-account-token", ".txt");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);

        try
        {
            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                AccountAuthOptions = CreateAccountAuthOptions()
            });

            using var client = new HttpClient();
            string? accountAccessToken = null;
            foreach (var testCase in fixture.RootElement.GetProperty("account_ready_cases").EnumerateArray())
            {
                var method = new HttpMethod(testCase.GetProperty("method").GetString()!);
                using var request = new HttpRequestMessage(
                    method,
                    new Uri(host.BaseUri, testCase.GetProperty("raw_target").GetString()!));
                request.Headers.TryAddWithoutValidation("X-PCV-Request-Id", testCase.GetProperty("request_id").GetString());
                if (testCase.GetProperty("authorization").ValueKind == JsonValueKind.String)
                {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        Expand(
                            testCase.GetProperty("authorization").GetString()!,
                            host.BaseUri,
                            host.WebBaseUri,
                            accountAccessToken));
                }

                if (testCase.GetProperty("body").ValueKind == JsonValueKind.String)
                {
                    request.Content = new StringContent(
                        testCase.GetProperty("body").GetString()!,
                        Encoding.UTF8,
                        "application/json");
                }

                using var response = await client.SendAsync(request);
                var observed = await ObserveAsync(response);
                AssertExpectedResponse(fixture.RootElement, testCase, observed, host.BaseUri, host.WebBaseUri);

                if (testCase.TryGetProperty("captures_access_token", out var captures) && captures.GetBoolean())
                {
                    using var document = JsonDocument.Parse(observed.Body);
                    accountAccessToken = document.RootElement.GetProperty("data").GetProperty("access_token").GetString();
                    Assert.False(string.IsNullOrWhiteSpace(accountAccessToken));
                }
            }
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task KnownAndUnknownLengthBodyCapsAndAuthOrderingMatchFixture()
    {
        using var fixture = LoadFixture();
        var bodyCap = fixture.RootElement.GetProperty("body_cap");
        var maxBytes = bodyCap.GetProperty("max_request_body_bytes").GetInt32();
        Assert.Equal(DesktopNodeHostOptions.MinimumMaxRequestBodyBytes, maxBytes);
        var oversizedBody = "{\"payload\":\"" + new string('x', maxBytes) + "\"}";
        Assert.True(Encoding.UTF8.GetByteCount(oversizedBody) >= bodyCap.GetProperty("oversized_body_bytes_minimum").GetInt32());

        foreach (var testCase in bodyCap.GetProperty("cases").EnumerateArray())
        {
            var tokenPath = UniqueTempPath("pcv-http-transport-cap-token", ".txt");
            await File.WriteAllTextAsync(tokenPath, ServiceToken);
            try
            {
                var accountReady = testCase.GetProperty("account_ready").GetBoolean();
                using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
                {
                    Mode = DesktopNodeHostMode.Listen,
                    Prefix = "http://127.0.0.1:0/",
                    ApiTokenFile = tokenPath,
                    MaxRequestBodyBytes = maxBytes,
                    AccountAuthOptions = accountReady ? CreateAccountAuthOptions() : null
                });
                using var client = new HttpClient();
                var rawTarget = testCase.TryGetProperty("raw_target", out var rawTargetElement) &&
                    rawTargetElement.ValueKind == JsonValueKind.String
                    ? rawTargetElement.GetString()!
                    : "/api/v1/auth/login";
                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, rawTarget));
                request.Headers.TryAddWithoutValidation("X-PCV-Request-Id", testCase.GetProperty("request_id").GetString());
                if (testCase.GetProperty("authorization").ValueKind == JsonValueKind.String)
                {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        Expand(testCase.GetProperty("authorization").GetString()!, host.BaseUri));
                }

                request.Content = testCase.GetProperty("body_transfer").GetString() == "unknown-length"
                    ? new UnknownLengthContent(oversizedBody)
                    : new StringContent(oversizedBody, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request);
                var observed = await ObserveAsync(response);
                AssertExpectedResponse(fixture.RootElement, testCase, observed, host.BaseUri, host.WebBaseUri);
                if (observed.StatusCode == 413)
                {
                    var expectedBody = bodyCap.GetProperty("too_large_body_template").GetString()!
                        .Replace("{{request_id}}", testCase.GetProperty("request_id").GetString(), StringComparison.Ordinal);
                    Assert.Equal(expectedBody, observed.Body);
                }
                else
                {
                    var hostAuthBody = fixture.RootElement.GetProperty("host_service_token_cases")[0]
                        .GetProperty("expected_body_exact").GetString();
                    Assert.Equal(hostAuthBody, observed.Body);
                }
            }
            finally
            {
                File.Delete(tokenPath);
            }
        }
    }

    [Fact]
    public async Task ProductRetryAfterHeaderAndRateLimitBodyMatchFixture()
    {
        using var fixture = LoadFixture();
        var rateLimit = fixture.RootElement.GetProperty("rate_limit");
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            RequestLimitPerMinute = rateLimit.GetProperty("request_limit_per_minute").GetInt32(),
            RequestBurstLimit = rateLimit.GetProperty("request_burst_limit").GetInt32(),
            RetryAfterSeconds = rateLimit.GetProperty("retry_after_seconds").GetInt32()
        });
        using var client = new HttpClient();
        using var first = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
        request.Headers.TryAddWithoutValidation("X-PCV-Request-Id", rateLimit.GetProperty("request_id").GetString());
        using var response = await client.SendAsync(request);
        var observed = await ObserveAsync(response);

        AssertExpectedResponse(fixture.RootElement, rateLimit, observed, host.BaseUri, host.WebBaseUri);
        Assert.Equal(rateLimit.GetProperty("expected_retry_after").GetString(), observed.Headers["Retry-After"]);
    }

    [Fact]
    public async Task NoVncDisabledAndDedicatedAuthenticationErrorsMatchFixture()
    {
        using var fixture = LoadFixture();
        var noVnc = fixture.RootElement.GetProperty("novnc");
        var websocketPath = noVnc.GetProperty("websocket_path").GetString()!;
        Assert.Equal(
            noVnc.GetProperty("decoded_vm_id").GetString(),
            Uri.UnescapeDataString(websocketPath.Split('/').Last()));
        var webSocketHeaders = WebSocketHandshakeHeaders();

        using (var disabledHost = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/"
        }))
        {
            var disabled = await SendRawHttpAsync(disabledHost.BaseUri, "GET", websocketPath, webSocketHeaders);
            AssertExpectedResponse(
                fixture.RootElement,
                noVnc.GetProperty("disabled"),
                disabled,
                disabledHost.BaseUri,
                disabledHost.WebBaseUri);
        }

        var tokenPath = UniqueTempPath("pcv-http-transport-novnc-token", ".txt");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);
        try
        {
            using var configuredHost = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                ApiTokenFile = tokenPath,
                NoVncTargetHost = "127.0.0.1",
                NoVncTargetPort = GetUnusedLoopbackPort(),
                NoVncWebSocketPath = "/api/v1/console/novnc/{vm_id}"
            });

            foreach (var testCase in noVnc.GetProperty("configured_auth_cases").EnumerateArray())
            {
                var headers = WebSocketHandshakeHeaders();
                if (testCase.GetProperty("authorization").ValueKind == JsonValueKind.String)
                {
                    headers["Authorization"] = Expand(testCase.GetProperty("authorization").GetString()!, configuredHost.BaseUri);
                }

                var observed = await SendRawHttpAsync(configuredHost.BaseUri, "GET", websocketPath, headers);
                AssertExpectedResponse(
                    fixture.RootElement,
                    testCase,
                    observed,
                    configuredHost.BaseUri,
                    configuredHost.WebBaseUri);
            }
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task NoVncHandshakeBinaryProxyAndNormalCloseMatchFixture()
    {
        using var fixture = LoadFixture();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var noVnc = fixture.RootElement.GetProperty("novnc");
        var success = noVnc.GetProperty("success");
        var payload = Encoding.ASCII.GetBytes(success.GetProperty("payload_ascii").GetString()!);
        using var target = new TcpListener(IPAddress.Loopback, 0);
        target.Start();
        var targetPort = ((IPEndPoint)target.LocalEndpoint).Port;
        var targetTask = Task.Run(async () =>
        {
            using var tcp = await target.AcceptTcpClientAsync(timeout.Token);
            await using var stream = tcp.GetStream();
            var received = new byte[payload.Length];
            await stream.ReadExactlyAsync(received, timeout.Token);
            Assert.Equal(payload, received);
            await stream.WriteAsync(received, timeout.Token);
            await stream.FlushAsync(timeout.Token);
        }, timeout.Token);

        var tokenPath = UniqueTempPath("pcv-http-transport-novnc-success-token", ".txt");
        await File.WriteAllTextAsync(tokenPath, ServiceToken);
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
            webSocket.Options.SetRequestHeader(
                "Authorization",
                Expand(success.GetProperty("authorization").GetString()!, host.BaseUri));
            var bridgeUri = new UriBuilder(host.BaseUri)
            {
                Scheme = "ws",
                Path = noVnc.GetProperty("websocket_path").GetString()
            }.Uri;

            await webSocket.ConnectAsync(bridgeUri, timeout.Token);
            Assert.Equal(101, success.GetProperty("handshake_status").GetInt32());
            Assert.Equal(WebSocketState.Open, webSocket.State);
            await webSocket.SendAsync(payload, WebSocketMessageType.Binary, true, timeout.Token);

            var receiveBuffer = new byte[payload.Length + 16];
            var message = await webSocket.ReceiveAsync(receiveBuffer, timeout.Token);
            Assert.Equal(success.GetProperty("message_type").GetString(), message.MessageType.ToString().ToLowerInvariant());
            Assert.True(message.EndOfMessage);
            Assert.Equal(payload, receiveBuffer.AsSpan(0, message.Count).ToArray());

            var close = await webSocket.ReceiveAsync(receiveBuffer, timeout.Token);
            Assert.Equal(WebSocketMessageType.Close, close.MessageType);
            Assert.Equal(success.GetProperty("close_status").GetInt32(), (int?)close.CloseStatus);
            Assert.Equal(success.GetProperty("close_description").GetString(), close.CloseStatusDescription);
            await targetTask.WaitAsync(timeout.Token);
        }
        finally
        {
            target.Stop();
            File.Delete(tokenPath);
        }
    }

    private static DesktopNodeAccountAuthOptions CreateAccountAuthOptions()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-http-transport-fixture",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    Id: "operator",
                    Username: "operator",
                    PasswordHash: DesktopNodeAccountPassword.HashPassword("operator-password", "pcv-http-transport-salt"),
                    Role: "operator",
                    DisplayName: "Operator")
            ]);
    }

    private static async Task<ObservedResponse> ObserveAsync(HttpResponseMessage response)
    {
        var headers = response.Headers
            .Concat(response.Content.Headers)
            .ToDictionary(header => header.Key, header => string.Join(", ", header.Value), StringComparer.OrdinalIgnoreCase);
        return new ObservedResponse(
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.MediaType,
            await response.Content.ReadAsStringAsync(),
            headers);
    }

    private static void AssertExpectedResponse(
        JsonElement fixtureRoot,
        JsonElement expected,
        ObservedResponse observed,
        Uri apiBaseUri,
        Uri? webBaseUri = null)
    {
        var caseId = expected.TryGetProperty("id", out var id) ? id.GetString() : "fixture-section";
        Assert.True(
            observed.StatusCode == expected.GetProperty("expected_status").GetInt32(),
            $"{caseId}: expected HTTP {expected.GetProperty("expected_status").GetInt32()}, observed {observed.StatusCode}. Body: {observed.Body}");
        Assert.Equal(expected.GetProperty("expected_content_type").GetString(), observed.ContentType);

        if (expected.TryGetProperty("expected_body_exact", out var exactBody))
        {
            Assert.Equal(Expand(exactBody.GetString()!, apiBaseUri, webBaseUri), observed.Body);
        }

        if (expected.TryGetProperty("expected_json_operation", out var operation))
        {
            using var document = JsonDocument.Parse(observed.Body);
            Assert.Equal(operation.GetString(), document.RootElement.GetProperty("operation").GetString());
        }

        if (expected.TryGetProperty("expected_error_code", out var errorCode))
        {
            using var document = JsonDocument.Parse(observed.Body);
            Assert.Equal(errorCode.GetString(), ReadErrorCode(document.RootElement));
        }

        if (observed.StatusCode != 204)
        {
            Assert.True(observed.Headers.TryGetValue("Content-Length", out var contentLength));
            Assert.Equal(Encoding.UTF8.GetByteCount(observed.Body), int.Parse(contentLength, CultureInfo.InvariantCulture));
        }

        AssertHeadersAreContractedOrTransportOwned(fixtureRoot, observed.Headers.Keys);
    }

    private static string? ReadErrorCode(JsonElement root)
    {
        if (root.TryGetProperty("error", out var error) &&
            error.ValueKind == JsonValueKind.Object &&
            error.TryGetProperty("code", out var nestedCode))
        {
            return nestedCode.GetString();
        }

        return root.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private static void AssertHeadersAreContractedOrTransportOwned(JsonElement fixtureRoot, IEnumerable<string> headerNames)
    {
        var headers = fixtureRoot.GetProperty("response_headers");
        var allowed = headers.GetProperty("contract_headers").EnumerateArray()
            .Concat(headers.GetProperty("transport_owned_non_contract_allowlist").EnumerateArray())
            .Select(value => value.GetString()!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var headerName in headerNames)
        {
            Assert.True(allowed.Contains(headerName), $"Response header '{headerName}' is neither contracted nor transport-owned/allowlisted.");
        }
    }

    private static async Task<ObservedResponse> SendRawHttpAsync(
        Uri baseUri,
        string method,
        string rawTarget,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(IPAddress.Loopback, baseUri.Port, timeout.Token);
        await using var stream = tcp.GetStream();
        var requestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Host"] = baseUri.IsDefaultPort ? baseUri.Host : $"{baseUri.Host}:{baseUri.Port}",
            ["Connection"] = "close"
        };
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                requestHeaders[header.Key] = header.Value;
            }
        }

        if (method is "POST" or "PUT" or "PATCH" &&
            !requestHeaders.ContainsKey("Content-Length") &&
            !requestHeaders.ContainsKey("Transfer-Encoding"))
        {
            requestHeaders["Content-Length"] = "0";
        }

        var request = new StringBuilder()
            .Append(method).Append(' ').Append(rawTarget).Append(" HTTP/1.1\r\n");
        foreach (var header in requestHeaders)
        {
            request.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
        }
        request.Append("\r\n");
        await stream.WriteAsync(Encoding.ASCII.GetBytes(request.ToString()), timeout.Token);
        await stream.FlushAsync(timeout.Token);

        var headerBytes = new List<byte>();
        var singleByte = new byte[1];
        while (!EndsWithHeaderTerminator(headerBytes))
        {
            var read = await stream.ReadAsync(singleByte, timeout.Token);
            if (read == 0)
            {
                throw new IOException("HTTP response ended before the header terminator.");
            }

            headerBytes.Add(singleByte[0]);
            if (headerBytes.Count > 64 * 1024)
            {
                throw new IOException("HTTP response headers exceeded the characterization limit.");
            }
        }

        var headerText = Encoding.ASCII.GetString(headerBytes.ToArray(), 0, headerBytes.Count - 4);
        var lines = headerText.Split("\r\n", StringSplitOptions.None);
        var statusParts = lines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        var statusCode = int.Parse(statusParts[1], CultureInfo.InvariantCulture);
        var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines.Skip(1))
        {
            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            responseHeaders[line[..separator]] = line[(separator + 1)..].Trim();
        }

        var bodyLength = responseHeaders.TryGetValue("Content-Length", out var rawLength)
            ? int.Parse(rawLength, CultureInfo.InvariantCulture)
            : 0;
        var bodyBytes = new byte[bodyLength];
        if (bodyLength > 0)
        {
            await stream.ReadExactlyAsync(bodyBytes, timeout.Token);
        }

        var contentType = responseHeaders.TryGetValue("Content-Type", out var rawContentType)
            ? rawContentType.Split(';', 2)[0]
            : null;
        return new ObservedResponse(statusCode, contentType, Encoding.UTF8.GetString(bodyBytes), responseHeaders);
    }

    private static bool EndsWithHeaderTerminator(IReadOnlyList<byte> bytes)
    {
        return bytes.Count >= 4 &&
            bytes[^4] == (byte)'\r' &&
            bytes[^3] == (byte)'\n' &&
            bytes[^2] == (byte)'\r' &&
            bytes[^1] == (byte)'\n';
    }

    private static Dictionary<string, string> WebSocketHandshakeHeaders()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Connection"] = "Upgrade",
            ["Upgrade"] = "websocket",
            ["Sec-WebSocket-Key"] = "dGhlIHNhbXBsZSBub25jZQ==",
            ["Sec-WebSocket-Version"] = "13"
        };
    }

    private static JsonDocument LoadFixture()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "packaging",
                "windows-desktop-node",
                "tests",
                "fixtures",
                "http-transport-contract-v1.json");
            if (File.Exists(candidate))
            {
                return JsonDocument.Parse(File.ReadAllText(candidate));
            }
        }

        throw new FileNotFoundException("Could not locate the tracked http-transport-contract-v1 fixture from the test output directory.");
    }

    private static string Expand(
        string value,
        Uri apiBaseUri,
        Uri? webBaseUri = null,
        string? accountAccessToken = null)
    {
        return value
            .Replace("{{api_origin}}", apiBaseUri.GetLeftPart(UriPartial.Authority), StringComparison.Ordinal)
            .Replace("{{web_origin}}", (webBaseUri ?? apiBaseUri).GetLeftPart(UriPartial.Authority), StringComparison.Ordinal)
            .Replace("{{service_token}}", ServiceToken, StringComparison.Ordinal)
            .Replace("{{account_access_token}}", accountAccessToken ?? string.Empty, StringComparison.Ordinal);
    }

    private static string UniqueTempPath(string prefix, string suffix = "")
    {
        return Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}{suffix}");
    }

    private static int GetUnusedLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed record ObservedResponse(
        int StatusCode,
        string? ContentType,
        string Body,
        IReadOnlyDictionary<string, string> Headers);

    private sealed class UnknownLengthContent(string body) : HttpContent
    {
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(body));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}

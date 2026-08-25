using DesktopNode.Cli;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliApplicationTests
{
    [Fact]
    public async Task UsesInjectedMissingDefaultProtectedTokenPathWithoutReadingMachineState()
    {
        var transport = new RecordingTransport(
            new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));
        var missingPath = MissingDefaultProtectedTokenPath();

        var result = await DesktopNodeCliApplication.RunAsync(
            ["host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: missingPath,
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.True(transport.Called);
        Assert.Null(transport.BearerToken);
        Assert.False(File.Exists(missingPath));
    }

    [Fact]
    public async Task ReturnsRedactedInvalidErrorForMalformedInjectedDefaultProtectedToken()
    {
        var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "api-token.dpapi.json");
        const string payload = "{not-json";
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, payload);
        var transport = new RecordingTransport(
            new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));

        try
        {
            var result = await DesktopNodeCliApplication.RunAsync(
                ["host", "status"],
                transport,
                environment: _ => null,
                defaultProtectedTokenFilePath: path,
                cancellationToken: CancellationToken.None);

            Assert.Equal(2, result.ExitCode);
            Assert.Contains("PCV_CLI_PROTECTED_TOKEN_INVALID", result.StandardError, StringComparison.Ordinal);
            Assert.False(transport.Called);
            Assert.DoesNotContain(path, result.StandardError, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(payload, result.StandardError, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SendsBearerTokenAndReturnsJsonOutput()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            "{\"ok\":true,\"version\":\"0.39.1-admin-smoke\"}"));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--format", "json", "--token", "secret-token", "host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("\"ok\":true", result.StandardOutput);
        Assert.Equal("GET", transport.Request!.Method);
        Assert.Equal("/api/v1/host/status", transport.Request.Path);
        Assert.Equal("secret-token", transport.BearerToken);
        Assert.DoesNotContain("secret-token", result.StandardError);
    }

    [Fact]
    public async Task RejectsAmbiguousTokenSourcesBeforeTransport()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--token", "inline-secret", "--token-file", "D:\\missing-token.txt", "host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(2, result.ExitCode);
        Assert.False(transport.Called);
        Assert.Contains("PCV_CLI_TOKEN_SOURCE_CONFLICT", result.StandardError);
        Assert.DoesNotContain("inline-secret", result.StandardError);
    }

    [Fact]
    public async Task WritesDiagnosticBundleDownloadToOutputPath()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "pcv-cli-tests", Guid.NewGuid().ToString("N"), "bundle.json");
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            "{\"ok\":true,\"bundle_id\":\"bundle-123\"}"));

        try
        {
            var result = await DesktopNodeCliApplication.RunAsync(
                ["diagnostics", "bundle", "download", "bundle-123", "--output", outputPath],
                transport,
                environment: _ => null,
                defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
                cancellationToken: CancellationToken.None);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal("{\"ok\":true,\"bundle_id\":\"bundle-123\"}", File.ReadAllText(outputPath));
            Assert.Contains(outputPath, result.StandardOutput);
            Assert.Equal("/api/v1/diagnostics/bundles/bundle-123/download", transport.Request!.Path);
        }
        finally
        {
            var root = Path.GetDirectoryName(Path.GetDirectoryName(outputPath));
            if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData("table")]
    [InlineData("plain")]
    [InlineData("csv")]
    [InlineData("json")]
    public async Task DiagnosticBundleListRendersRowsAndPageMetadataInEveryFormat(string format)
    {
        const string body = """
            {"ok":true,"operation":"diagnostic.bundle.list","request_id":"req-diagnostic-list","data":{
              "bundles":[
                {"bundle_id":"bundle-20260812-alpha","file_name":"bundle-alpha.zip","created_at":"2026-08-12T01:02:03Z","last_write_time_utc":"2026-08-12T01:02:04Z","size_bytes":4096,"download_url":"/api/v1/diagnostics/bundles/bundle-20260812-alpha/download","archive_status":"ready","redaction_status":"complete","api_token":"must-not-render"},
                {"bundle_id":"bundle-20260812-beta","file_name":"bundle-beta.zip","created_at":"2026-08-12T02:03:04Z","last_write_time_utc":"2026-08-12T02:03:05Z","size_bytes":8192,"download_url":"/api/v1/diagnostics/bundles/bundle-20260812-beta/download","archive_status":"ready","redaction_status":"complete"}
              ],
              "count":2,"returned":2,"limit":10,"offset":0,"next_offset":20,
              "retention":{"max_count":50,"max_age_days":30}
            },"error":null}
            """;
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", body));
        var globalArguments = format switch
        {
            "table" => new[] { "--no-color" },
            "plain" => new[] { "--plain" },
            "csv" => new[] { "--csv" },
            "json" => new[] { "--json" },
            _ => throw new InvalidOperationException("Unknown test format.")
        };
        var arguments = globalArguments
            .Concat(new[] { "diagnostics", "bundle", "list", "--limit", "10", "--offset", "0" })
            .ToArray();

        var result = await DesktopNodeCliApplication.RunAsync(
            arguments,
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("GET", transport.Request!.Method);
        Assert.Equal("/api/v1/diagnostics/bundles?limit=10&offset=0", transport.Request.Path);
        var alphaValues = new[]
        {
            "bundle-20260812-alpha",
            "bundle-alpha.zip",
            "2026-08-12T01:02:03Z",
            "2026-08-12T01:02:04Z",
            "4096",
            "/api/v1/diagnostics/bundles/bundle-20260812-alpha/download",
            "ready",
            "complete"
        };
        var betaValues = new[]
        {
            "bundle-20260812-beta",
            "bundle-beta.zip",
            "2026-08-12T02:03:04Z",
            "2026-08-12T02:03:05Z",
            "8192",
            "/api/v1/diagnostics/bundles/bundle-20260812-beta/download",
            "ready",
            "complete"
        };
        Assert.All(alphaValues, value => Assert.Contains(value, result.StandardOutput, StringComparison.Ordinal));
        Assert.All(betaValues, value => Assert.Contains(value, result.StandardOutput, StringComparison.Ordinal));
        Assert.Equal(2, result.StandardOutput.Split("ready", StringSplitOptions.None).Length - 1);
        Assert.Equal(2, result.StandardOutput.Split("complete", StringSplitOptions.None).Length - 1);
        Assert.Contains("next_offset", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("20", result.StandardOutput, StringComparison.Ordinal);
        if (format != "json")
        {
            Assert.DoesNotContain("must-not-render", result.StandardOutput, StringComparison.Ordinal);
        }

        switch (format)
        {
            case "table":
                Assert.Contains("Diagnostic Bundles", result.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("Bundle 1", result.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("Bundle 2", result.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("FIELD", result.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("VALUE", result.StandardOutput, StringComparison.Ordinal);
                AssertDiagnosticBundleFieldOrder(result.StandardOutput, "Bundle 1", "Bundle 2", "bundle-20260812-alpha");
                AssertDiagnosticBundleFieldOrder(result.StandardOutput, "Bundle 2", "Page:", "bundle-20260812-beta");
                Assert.DoesNotContain("BUNDLE_ID    | FILE_NAME", result.StandardOutput, StringComparison.Ordinal);
                Assert.DoesNotContain("\u001b[", result.StandardOutput, StringComparison.Ordinal);
                var maxLineLength = result.StandardOutput
                    .Split(Environment.NewLine, StringSplitOptions.None)
                    .Max(static line => line.Length);
                Assert.True(maxLineLength <= 120, $"Expected every table line to fit within 120 characters, but observed {maxLineLength}.");
                break;
            case "plain":
                Assert.Contains("bundle_id=", result.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("next_offset=20", result.StandardOutput, StringComparison.Ordinal);
                break;
            case "csv":
                Assert.Contains("bundle_id,file_name,created_at", result.StandardOutput, StringComparison.Ordinal);
                Assert.DoesNotContain("summary", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
                break;
            case "json":
                Assert.Equal(body.Trim(), result.StandardOutput.Trim());
                Assert.Contains("\"bundles\"", result.StandardOutput, StringComparison.Ordinal);
                break;
        }
    }

    private static void AssertDiagnosticBundleFieldOrder(
        string output,
        string sectionStart,
        string sectionEnd,
        string expectedBundleId)
    {
        var startIndex = output.IndexOf(sectionStart, StringComparison.Ordinal);
        var endIndex = output.IndexOf(sectionEnd, startIndex + sectionStart.Length, StringComparison.Ordinal);
        Assert.True(startIndex >= 0, $"Missing table section '{sectionStart}'.");
        Assert.True(endIndex > startIndex, $"Missing table section boundary '{sectionEnd}'.");
        var section = output[startIndex..endIndex];
        Assert.Contains(expectedBundleId, section, StringComparison.Ordinal);
        var previousIndex = -1;
        foreach (var field in new[]
        {
            "bundle_id",
            "file_name",
            "created_at",
            "last_write_time_utc",
            "size_bytes",
            "download_url",
            "archive_status",
            "redaction_status"
        })
        {
            var fieldIndex = section.IndexOf(field, StringComparison.Ordinal);
            Assert.True(fieldIndex > previousIndex, $"Expected field '{field}' in stable order within '{sectionStart}'.");
            previousIndex = fieldIndex;
        }
    }

    [Fact]
    public async Task DiagnosticBundleListTableRendersUsefulEmptyPage()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"diagnostic.bundle.list","request_id":"req-diagnostic-empty","data":{
              "bundles":[],"count":0,"returned":0,"limit":10,"offset":0,"next_offset":null
            },"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "diagnostics", "bundle", "list", "--limit", "10", "--offset", "0"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("/api/v1/diagnostics/bundles?limit=10&offset=0", transport.Request!.Path);
        Assert.Contains("Diagnostic Bundles", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No diagnostic bundles found.", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("count", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("returned", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("limit", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("offset", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("next_offset", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=diagnostic.bundle.list", result.StandardOutput, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not-json", "not-json")]
    [InlineData("{\"ok\":true,\"operation\":\"diagnostic.bundle.list\",\"request_id\":\"req-missing-bundles\",\"data\":{\"count\":0}}", "ok=True | operation=diagnostic.bundle.list")]
    public async Task DiagnosticBundleListTableFallsBackForMalformedOrNonmatchingPayload(string body, string expectedOutput)
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(200, "application/json", body));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "diagnostics", "bundle", "list"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains(expectedOutput, result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Diagnostic Bundles", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VmListTableRendersActualVmRowsWithNeonColor()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"vm.list","request_id":"req-vm-list","data":[
              {"id":"4a209d2e-d9ab-4c0c-83bd-c3a5aef6f207","name":"LeeSiEun","state":"running"},
              {"id":"4935e2fc-6926-4d4b-b26f-709419760352","name":"juHyeonLee","state":"stopped"}
            ],"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["vm", "list"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("SYS_UUID", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("ENTITY_ID", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("LIFELINE", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("4a209d2e-d9ab-4c0c-83bd-c3a5aef6f207", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("LeeSiEun", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("running", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("4935e2fc-6926-4d4b-b26f-709419760352", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("juHyeonLee", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;51m", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;226m", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("\u001b[38;5;46m", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=vm.list", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VmListTableHonorsNoColor()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """{"ok":true,"operation":"vm.list","data":[{"id":"alpha-id","name":"alpha","state":"running"}],"error":null}"""));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "vm", "list"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("alpha-id", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("alpha", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("running", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("\u001b[", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RuntimePolicyTableRendersPolicyFields()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"runtime.policy","request_id":"req-runtime","data":{
              "runtime_core":{"owner":"DesktopNode.Api","auth_session":{"boundary":"loopback-default-token-required-nonloopback-static-assets-bearer-required","token_storage":"windows-credential-manager"},"job_runtime":{"state_store":"json-file-snapshot","worker":"bounded-synchronous-worker-tick"},"diagnostics":{"bundle_root":"configured-diagnostics-root"}},
              "job_runtime":{"dispatch":{"mode":"bounded-synchronous-worker-tick","mutation_dispatch":"native-vm-qos-preview-and-mutation-create-lifecycle-media-resource-delete-checkpoint-mutation"},"control":{"cancel":{"running_interrupt":true,"running_interrupt_operations":["vm.guest.exec","vm.guest.channel.verify"]}},"native_core":{"status":"read-route-vm-qos-preview-guestservice-create-lifecycle-resource-checkpoint-and-qos-mutation-started"}},
              "auth":{"mode":"account_rbac_jwt_not_configured","roles":["viewer","operator","admin"],"token_storage":"windows-credential-manager"},
              "network":{"current_exposure":"loopback","lan_mode":"preview-admin-opt-in"},
              "console":{"mode":"windows-hyperv-console-handoff","novnc":"not_configured"}
            },"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "runtime", "policy"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Runtime Policy", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("auth.mode", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("account_rbac_jwt_not_configured", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("auth.roles", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("viewer, operator, admin", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("network.current_exposure", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("loopback", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("job.dispatch.mode", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("job.running_interrupt_operations", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("vm.guest.exec, vm.guest.channel.verify", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=runtime.policy", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VmConsoleTableRendersConsoleAccessCard()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"console.session","request_id":"req-console","data":{
              "vm_id":"alpha",
              "console_access":{
                "contract":"console-access-card.v1",
                "account":{"required_permission":"console.view","auth_surface":"service-token-or-account-jwt","token_output":"redacted"},
                "windows_console":{"available":true,"type":"vmconnect","transport":"local-handoff"},
                "novnc":{"enabled":true,"status":"available","bridge_mode":"websocket-to-vnc-tcp","websocket_path":"/api/v1/console/novnc/alpha","reason":"noVNC bridge is configured."},
                "status":"browser-streaming-available",
                "next_action":"Open the noVNC browser session for this VM, or use vmconnect from the host console.",
                "host_mutation_performed":false
              }
            },"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "vm", "console", "alpha"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Console Access", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("vm.id", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("alpha", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("account.permission", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("console.view", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("windows_console.type", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("vmconnect", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("windows_console.transport", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("local-handoff", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("novnc.status", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("available", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("novnc.bridge_mode", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("websocket-to-vnc-tcp", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("novnc.websocket_path", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("/api/v1/console/novnc/alpha", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("status", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("browser-streaming-available", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("next_action", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Open the noVNC browser session for this VM, or use vmconnect from the host console.", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("host_mutation_performed", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("false", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=console.session", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VmVncJsonReturnsOriginalConsolePayload()
    {
        const string body = """
            {"ok":true,"operation":"console.session","request_id":"req-console","data":{
              "vm_id":"alpha",
              "console_access":{
                "contract":"console-access-card.v1",
                "account":{"required_permission":"console.view"},
                "windows_console":{"type":"vmconnect"},
                "novnc":{"status":"not_configured","reason":"Windows VNC/WebSocket bridge is not configured."},
                "next_action":"Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.",
                "host_mutation_performed":false
              }
            },"error":null}
            """;
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            body));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--json", "vm", "vnc", "alpha"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("/api/v1/vms/alpha/console", transport.Request!.Path);
        Assert.Equal(body.Trim(), result.StandardOutput.Trim());
    }

    [Fact]
    public async Task OpsSummaryTableRendersSignalsCountsAndEvidence()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"ops.summary","request_id":"req-ops","data":{
              "signals":[
                {"key":"host-readiness","label":"Host readiness","tone":"ok","value":"Ready"},
                {"key":"failed-jobs","label":"Failed jobs","tone":"error","value":2}
              ],
              "vm_counts":{"total":5,"running":3,"checkpoint_warnings":1},
              "job_counts":{"queued":1,"running":2,"failed":2,"succeeded":14,"canceled":0},
              "host":{"windows":{"edition":"Pro","version":"25H2"},"admin":{"elevated":true},"hyperv":{"feature_enabled":true,"vmms_running":true,"default_switch_present":true}},
              "installed_runtime":{"version":"0.42.43-admin-smoke","service_state":"Running","evidence_status":"available"},
              "batch_evidence":{"status":"available","latest":{"batch_id":"full-admin-host-mutation-gate-20260522-04241"}},
              "current_evidence":{"public_boundary":{"latest_main_push":{"run_id":"26578120570","head_sha":"7a7d5de822bdb058b04149eeeef0a7eb462828b5","status":"tracked-in-documentation"}},"full_admin_host_mutation":{"latest":{"version":"0.42.41-admin-smoke","status":"available"}},"manual_admin":{"latest_package_pair":{"package_pair":"0.42.55-admin-smoke -> 0.42.56-admin-smoke","status":"artifact-discovered"},"next_package_pair":{"package_pair":"0.42.56-admin-smoke -> 0.42.57-admin-smoke","decision":"opened-public-boundary-current-evidence-rollup-payload","status":"candidate-selected-public-boundary-current-rollup"}}}
            },"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "ops", "summary"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Ops Summary", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Host readiness", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Ready", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("vm.total", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("5", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("job.failed", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("full-admin-host-mutation-gate-20260522-04241", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("0.42.55-admin-smoke -> 0.42.56-admin-smoke", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("current.manual_admin_next_package_pair", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("0.42.56-admin-smoke -> 0.42.57-admin-smoke", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("opened-public-boundary-current-evidence-rollup-payload", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("current.public_boundary_main_push", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("26578120570", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("current.public_boundary_head_sha", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("7a7d5de822bdb058b04149eeeef0a7eb462828b5", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=ops.summary", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NetworkInventoryTableRendersSwitchRows()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            200,
            "application/json",
            """
            {"ok":true,"operation":"network.inventory","request_id":"req-network","data":{"source":"hyperv","mutating":false,"switches":[
              {"name":"Default Switch","type":"internal","is_default":true,"allow_management_os":true,"net_adapter_interface_description":null},
              {"name":"External Lab","type":"external","is_default":false,"allow_management_os":false,"net_adapter_interface_description":"Intel Ethernet"}
            ]},"error":null}
            """));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["--no-color", "network", "list"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("/api/v1/network/inventory", transport.Request!.Path);
        Assert.Contains("Network Inventory", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("NAME", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("TYPE", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("DEFAULT", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Default Switch", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("internal", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("External Lab", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Intel Ethernet", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ok=True | operation=network.inventory", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsFailureForNestedApiProblemJson()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            429,
            "application/problem+json",
            "{\"ok\":false,\"error\":{\"code\":\"PCV_RATE_LIMIT_EXCEEDED\",\"message\":\"Too many requests.\",\"detail\":\"Request limit was exceeded.\",\"recommended_action\":\"Wait for Retry-After, then retry.\"}}"));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("code=PCV_RATE_LIMIT_EXCEEDED", result.StandardError);
        Assert.Contains("message=Too many requests.", result.StandardError);
        Assert.Contains("detail=Request limit was exceeded.", result.StandardError);
        Assert.Contains("Next action: Wait for Retry-After, then retry.", result.StandardError);
    }

    [Fact]
    public async Task ReturnsFailureForRootApiProblemJson()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            504,
            "application/problem+json",
            "{\"code\":\"PCV_ROUTE_TIMEOUT\",\"message\":\"Route timed out.\",\"detail\":\"host.status exceeded the route timeout.\",\"recommended_action\":\"Check service health, then retry.\"}"));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("code=PCV_ROUTE_TIMEOUT", result.StandardError);
        Assert.Contains("message=Route timed out.", result.StandardError);
        Assert.Contains("detail=host.status exceeded the route timeout.", result.StandardError);
        Assert.Contains("Next action: Check service health, then retry.", result.StandardError);
    }

    [Fact]
    public async Task ReturnsFailureForNullApiProblemBodyWithoutThrowing()
    {
        var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
            502,
            "application/problem+json",
            null!));

        var result = await DesktopNodeCliApplication.RunAsync(
            ["host", "status"],
            transport,
            environment: _ => null,
            defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath(),
            cancellationToken: CancellationToken.None);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("PCV_CLI_HTTP_502", result.StandardError);
    }

    private static string MissingDefaultProtectedTokenPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "pcv-cli-default-token-tests",
            Guid.NewGuid().ToString("N"),
            "api-token.dpapi.json");
    }

    private sealed class RecordingTransport(DesktopNodeCliTransportResponse response) : IDesktopNodeCliTransport
    {
        public bool Called { get; private set; }

        public DesktopNodeCliRequest? Request { get; private set; }

        public string? BearerToken { get; private set; }

        public Task<DesktopNodeCliTransportResponse> SendAsync(
            DesktopNodeCliRequest request,
            DesktopNodeCliOptions options,
            string? bearerToken,
            CancellationToken cancellationToken)
        {
            Called = true;
            Request = request;
            BearerToken = bearerToken;
            return Task.FromResult(response);
        }
    }
}

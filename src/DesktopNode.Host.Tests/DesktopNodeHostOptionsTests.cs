using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostOptionsTests
{
    [Fact]
    public void ListenOptionsParseLoopbackPrefixAndProtectedTokenFile()
    {
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--web-root",
            "web",
            "--job-store",
            "jobs.json",
            "--event-log",
            "events.jsonl",
            "--event-log-provider-source",
            "PureCVisor Desktop Node",
            "--event-log-provider-log",
            "Application",
            "--event-log-writer",
            "windows-event-log",
            "--event-log-schema-version",
            "1",
            "--diagnostics-root",
            "diagnostics",
            "--api-token-protected-file",
            "api-token.dpapi.json",
            "--account-file",
            "accounts.json",
            "--jwt-signing-key-file",
            "jwt-signing-key.txt",
            "--route-timeout-seconds",
            "25",
            "--request-limit-per-minute",
            "60",
            "--request-burst-limit",
            "5",
            "--retry-after-seconds",
            "9",
            "--max-request-body-bytes",
            "2097152",
            "--controlled-route-timeout-probe-delay-ms",
            "2500"
        ]);

        Assert.Equal(DesktopNodeHostMode.Listen, options.Mode);
        Assert.Equal("http://127.0.0.1:7777/", options.Prefix);
        Assert.Equal("web", options.WebRootPath);
        Assert.Equal("jobs.json", options.JobStorePath);
        Assert.Equal("events.jsonl", options.EventLogPath);
        Assert.Equal("PureCVisor Desktop Node", options.EventLogProviderSource);
        Assert.Equal("Application", options.EventLogProviderLogName);
        Assert.Equal("windows-event-log", options.EventLogWriter);
        Assert.Equal(1, options.EventLogSchemaVersion);
        Assert.Equal("diagnostics", options.DiagnosticsRootPath);
        Assert.Equal("api-token.dpapi.json", options.ApiTokenProtectedFile);
        Assert.Equal("accounts.json", options.AccountFilePath);
        Assert.Equal("jwt-signing-key.txt", options.JwtSigningKeyFilePath);
        Assert.Null(options.ApiTokenFile);
        Assert.False(options.AllowLan);
        Assert.Equal(25, options.RouteTimeoutSeconds);
        Assert.Equal(60, options.RequestLimitPerMinute);
        Assert.Equal(5, options.RequestBurstLimit);
        Assert.Equal(9, options.RetryAfterSeconds);
        Assert.Equal(2_097_152, options.MaxRequestBodyBytes);
        Assert.Equal(2_500, options.ControlledRouteTimeoutProbeDelayMilliseconds);
    }

    [Fact]
    public void ListenOptionsParseTrackedAsyncAdmissionDefaultsAndOverrides()
    {
        var defaults = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--request-lifetime-mode",
            "tracked_async_serialized"
        ]);

        Assert.Equal(DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized, defaults.RequestLifetimeMode);
        Assert.Equal(32, defaults.RequestAdmissionActiveLimit);
        Assert.Equal(64, defaults.RequestAdmissionWaitingLimit);

        var overrides = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--request-lifetime-mode",
            "tracked-async-serialized",
            "--request-admission-active",
            "4",
            "--request-admission-waiting",
            "9"
        ]);

        Assert.Equal(DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized, overrides.RequestLifetimeMode);
        Assert.Equal(4, overrides.RequestAdmissionActiveLimit);
        Assert.Equal(9, overrides.RequestAdmissionWaitingLimit);
    }

    [Fact]
    public void ListenOptionsRejectUnknownRequestLifetimeMode()
    {
        var error = Assert.Throws<ArgumentException>(() => DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--request-lifetime-mode",
            "future"
        ]));

        Assert.Contains("PCV_HOST_ARGUMENT_VALUE_INVALID", error.Message);
        Assert.Contains("--request-lifetime-mode", error.Message);
    }

    [Fact]
    public void ServiceActionOptionsParseControlledRouteTimeoutProbeOverride()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "repair-installed",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--route-timeout-seconds",
            "1",
            "--controlled-route-timeout-probe-delay-ms",
            "2500"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("repair-installed", options.ServiceAction);
        Assert.Equal(1, options.RouteTimeoutSeconds);
        Assert.Equal(2_500, options.ControlledRouteTimeoutProbeDelayMilliseconds);
    }

    [Fact]
    public void ServiceActionOptionsParseBatchEvidenceRoot()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "repair-installed",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--batch-evidence-root",
            "D:\\PureCVisorEvidence\\batch-runs"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("repair-installed", options.ServiceAction);
        Assert.Equal("D:\\PureCVisorEvidence\\batch-runs", options.BatchEvidenceRootPath);
    }

    [Fact]
    public void ServiceActionOptionsParseEventLogDefaultTransitionTimeout()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "eventlog-default-transition",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--service-exe",
            "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            "--eventlog-default-transition-timeout-seconds",
            "45"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("eventlog-default-transition", options.ServiceAction);
        Assert.Equal(45, options.EventLogDefaultTransitionTimeoutSeconds);
    }

    [Theory]
    [InlineData("1023")]
    [InlineData("67108865")]
    [InlineData("not-an-int")]
    public void ListenOptionsRejectInvalidMaxRequestBodyBytes(string value)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://127.0.0.1:7777/",
                "--max-request-body-bytes",
                value
            ]));

        Assert.Contains("PCV_HOST_ARGUMENT_VALUE_INVALID", error.Message);
        Assert.Contains("--max-request-body-bytes", error.Message);
    }

    [Fact]
    public void ListenOptionsParseLegacyTokenFile()
    {
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--api-token-file",
            "api-token.txt"
        ]);

        Assert.Equal("api-token.txt", options.ApiTokenFile);
        Assert.Null(options.ApiTokenProtectedFile);
    }

    [Fact]
    public void ListenOptionsParseCredentialManagerTargetForLanBearer()
    {
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://0.0.0.0:7777/",
            "--allow-lan",
            "--api-token-credential-target",
            "PureCVisor/PureCVisorDesktopNode/api-token"
        ]);

        Assert.True(options.AllowLan);
        Assert.Equal("PureCVisor/PureCVisorDesktopNode/api-token", options.ApiTokenCredentialTarget);
        Assert.Null(options.ApiTokenFile);
        Assert.Null(options.ApiTokenProtectedFile);
    }

    [Fact]
    public void ListenOptionsParseBatchEvidenceRoot()
    {
        var expectedRoot = Path.GetFullPath("artifacts\\batch-runs");
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--batch-evidence-root",
            "artifacts\\batch-runs"
        ]);

        Assert.Equal(expectedRoot, options.BatchEvidenceRootPath);
    }

    [Fact]
    public void ServiceActionOptionsNormalizeRelativeBatchEvidenceRootToAbsolutePath()
    {
        var expectedRoot = Path.GetFullPath("artifacts");
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "repair-installed",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--batch-evidence-root",
            "artifacts"
        ]);

        Assert.Equal(expectedRoot, options.BatchEvidenceRootPath);
    }

    [Fact]
    public void ListenOptionsParseSeparateWebPrefix()
    {
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--web-prefix",
            "http://127.0.0.1:80/",
            "--web-root",
            "web"
        ]);

        Assert.Equal("http://127.0.0.1:7777/", options.Prefix);
        Assert.Equal("http://127.0.0.1:80/", options.WebPrefix);
        Assert.Equal("web", options.WebRootPath);
    }

    [Fact]
    public void ListenOptionsParseNoVncBridgeTarget()
    {
        var options = DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--novnc-target-host",
            "127.0.0.1",
            "--novnc-target-port",
            "5901",
            "--novnc-websocket-path",
            "/api/v1/console/novnc/{vm_id}"
        ]);

        Assert.True(options.NoVncBridgeEnabled);
        Assert.Equal("127.0.0.1", options.NoVncTargetHost);
        Assert.Equal(5901, options.NoVncTargetPort);
        Assert.Equal("/api/v1/console/novnc/{vm_id}", options.NoVncWebSocketPath);
    }

    [Fact]
    public void ListenOptionsRejectNonLoopbackNoVncTargetWithoutLanApproval()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://127.0.0.1:7777/",
                "--novnc-target-host",
                // public-safety: synthetic-rfc1918
                "192.168.1.20",
                "--novnc-target-port",
                "5901"
            ]));

        Assert.Contains("PCV_HOST_NOVNC_TARGET_NOT_LOOPBACK", error.Message);
    }

    [Fact]
    public void ListenOptionsRejectNonLoopbackWebPrefixWithoutLanApproval()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://127.0.0.1:7777/",
                "--web-prefix",
                // public-safety: synthetic-rfc1918
                "http://192.168.1.17:80/",
                "--web-root",
                "web"
            ]));

        Assert.Contains("PCV_HOST_WEB_PREFIX_NOT_LOOPBACK", error.Message);
    }

    [Fact]
    public void ListenOptionsRejectInlineTokenValues()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://127.0.0.1:7777/",
                "--api-token",
                "secret"
            ]));

        Assert.Contains("PCV_HOST_INLINE_TOKEN_FORBIDDEN", error.Message);
    }

    [Fact]
    public void ListenOptionsRejectRetiredHelperScriptPath()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://127.0.0.1:7777/",
                "--helper-script",
                "C:\\Program Files\\PureCVisor\\DesktopNode\\hyperv\\Invoke-PcvHyperV.ps1"
            ]));

        Assert.Contains("PCV_HOST_HELPER_SCRIPT_RETIRED", error.Message);
    }

    [Fact]
    public void ListenOptionsRejectLanWithoutTokenSource()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostOptions.Parse([
                "listen",
                "--prefix",
                "http://0.0.0.0:7777/",
                "--allow-lan"
            ]));

        Assert.Contains("PCV_HOST_LAN_TOKEN_REQUIRED", error.Message);
    }

    [Fact]
    public void ServiceActionOptionsParseConfigureInstalled()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "configure-installed",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--service-exe",
            "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("configure-installed", options.ServiceAction);
        Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode", options.ProductRoot);
        Assert.Equal("C:\\ProgramData\\PureCVisor\\desktop-node", options.DataRoot);
        Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe", options.ServiceExecutablePath);
    }

    [Fact]
    public void ServiceActionOptionsParseStatusStartStop()
    {
        foreach (var action in new[] { "status", "start", "stop" })
        {
            var options = DesktopNodeHostOptions.Parse([
                "service-action",
                action,
                "--product-root",
                "C:\\Program Files\\PureCVisor\\DesktopNode",
                "--service-exe",
                "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe"
            ]);

            Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
            Assert.Equal(action, options.ServiceAction);
            Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode", options.ProductRoot);
            Assert.Null(options.DataRoot);
            Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe", options.ServiceExecutablePath);
        }
    }

    [Fact]
    public void ServiceActionOptionsParseCredentialTarget()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "credential-manager-system-proof",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--service-exe",
            "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            "--credential-target",
            "PureCVisor/PureCVisorDesktopNode/api-token"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("credential-manager-system-proof", options.ServiceAction);
        Assert.Equal("PureCVisor/PureCVisorDesktopNode/api-token", options.CredentialTarget);
    }

    [Fact]
    public void ServiceActionOptionsParseDataRootRemove()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "data-root-remove",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--service-exe",
            "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            "--remove-data"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("data-root-remove", options.ServiceAction);
        Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode", options.ProductRoot);
        Assert.Equal("C:\\ProgramData\\PureCVisor\\desktop-node", options.DataRoot);
        Assert.Equal("C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe", options.ServiceExecutablePath);
        Assert.True(options.RemoveData);
    }

    [Fact]
    public void ServiceActionOptionsParseConfigMigrationApplyPlanIdentity()
    {
        var options = DesktopNodeHostOptions.Parse([
            "service-action",
            "config-migration-apply",
            "--product-root",
            "C:\\Program Files\\PureCVisor\\DesktopNode",
            "--data-root",
            "C:\\ProgramData\\PureCVisor\\desktop-node",
            "--service-exe",
            "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            "--migration-plan-id",
            "product-config-v1-to-v2",
            "--migration-plan-version",
            "1"
        ]);

        Assert.Equal(DesktopNodeHostMode.ServiceAction, options.Mode);
        Assert.Equal("config-migration-apply", options.ServiceAction);
        Assert.Equal("product-config-v1-to-v2", options.MigrationPlanId);
        Assert.Equal(1, options.MigrationPlanVersion);
    }
}

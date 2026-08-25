using DesktopNode.Api;

namespace DesktopNode.Host;

public enum DesktopNodeHostMode
{
    Listen,
    ServiceAction
}

public enum DesktopNodeRequestLifetimeMode
{
    Legacy,
    TrackedAsyncSerialized
}

public sealed class DesktopNodeHostOptions
{
    public const int DefaultMaxRequestBodyBytes = 1_048_576;
    public const int MinimumMaxRequestBodyBytes = 1_024;
    public const int MaximumMaxRequestBodyBytes = 67_108_864;

    public DesktopNodeHostMode Mode { get; init; } = DesktopNodeHostMode.Listen;
    public string Prefix { get; init; } = "http://127.0.0.1:7777/";
    public string? WebPrefix { get; init; }
    public string? WebRootPath { get; init; }
    public string? JobStorePath { get; init; }
    public string? EventLogPath { get; init; }
    public string? EventLogProviderSource { get; init; }
    public string? EventLogProviderLogName { get; init; }
    public string? EventLogWriter { get; init; }
    public int EventLogSchemaVersion { get; init; } = 1;
    public string? DiagnosticsRootPath { get; init; }
    public string? BatchEvidenceRootPath { get; init; }
    public string? ApiTokenFile { get; init; }
    public string? ApiTokenProtectedFile { get; init; }
    public string? AccountFilePath { get; init; }
    public string? JwtSigningKeyFilePath { get; init; }
    public DesktopNodeAccountAuthOptions? AccountAuthOptions { get; init; }
    public string? NoVncTargetHost { get; init; }
    public int? NoVncTargetPort { get; init; }
    public string NoVncWebSocketPath { get; init; } = "/api/v1/console/novnc/{vm_id}";
    public bool NoVncBridgeEnabled => !string.IsNullOrWhiteSpace(NoVncTargetHost) && NoVncTargetPort.HasValue;
    public string? ApiTokenCredentialTarget { get; init; }
    public int RouteTimeoutSeconds { get; init; } = 30;
    public int RequestLimitPerMinute { get; init; } = 120;
    public int RequestBurstLimit { get; init; } = 20;
    public int RetryAfterSeconds { get; init; } = 15;
    public int ControlledRouteTimeoutProbeDelayMilliseconds { get; init; }
    public int MaxRequestBodyBytes { get; init; } = DefaultMaxRequestBodyBytes;
    public DesktopNodeRequestLifetimeMode RequestLifetimeMode { get; init; } = DesktopNodeRequestLifetimeMode.Legacy;
    public int RequestAdmissionActiveLimit { get; init; } = 32;
    public int RequestAdmissionWaitingLimit { get; init; } = 64;
    public int EventLogDefaultTransitionTimeoutSeconds { get; init; } = 60;
    public bool AllowLan { get; init; }
    public string? ServiceAction { get; init; }
    public string? ProductRoot { get; init; }
    public string? DataRoot { get; init; }
    public string? ServiceExecutablePath { get; init; }
    public bool RemoveData { get; init; }
    public bool DryRun { get; init; }
    public bool ReleaseApproved { get; init; }
    public string? FirewallRuleName { get; init; }
    public int? FirewallLocalPort { get; init; }
    public string? FirewallProfile { get; init; }
    public string? FirewallRemoteAddress { get; init; }
    public string? TrustRootCertificatePath { get; init; }
    public string? TrustRootCertificateThumbprint { get; init; }
    public string? TrustPublisherCertificatePath { get; init; }
    public string? TrustPublisherCertificateThumbprint { get; init; }
    public string? MigrationPlanId { get; init; }
    public int? MigrationPlanVersion { get; init; }
    public string? CredentialTarget { get; init; }

    public static DesktopNodeHostOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || string.Equals(args[0], "listen", StringComparison.OrdinalIgnoreCase))
        {
            var values = ParseNamedArguments(args.Count == 0 ? [] : args.Skip(1).ToArray());
            var prefix = GetValue(values, "--prefix") ?? "http://127.0.0.1:7777/";
            var webPrefix = GetValue(values, "--web-prefix");
            var webRoot = GetValue(values, "--web-root");
            var apiToken = GetValue(values, "--api-token");
            var protectedToken = GetValue(values, "--api-token-protected-file");
            var tokenFile = GetValue(values, "--api-token-file");
            var accountFile = GetValue(values, "--account-file");
            var jwtSigningKeyFile = GetValue(values, "--jwt-signing-key-file");
            var noVncTargetHost = GetValue(values, "--novnc-target-host");
            var noVncTargetPort = ParseOptionalInt(values, "--novnc-target-port");
            var noVncWebSocketPath = GetValue(values, "--novnc-websocket-path") ?? "/api/v1/console/novnc/{vm_id}";
            var credentialTarget = GetValue(values, "--api-token-credential-target");
            var allowLan = values.ContainsKey("--allow-lan");
            var requestLifetimeMode = ParseRequestLifetimeMode(values, "--request-lifetime-mode");

            if (values.ContainsKey("--helper-script"))
            {
                throw new ArgumentException("PCV_HOST_HELPER_SCRIPT_RETIRED|The .NET product runtime no longer accepts a Hyper-V helper script path.|Use the native C# Hyper-V adapter built into DesktopNode.Host.exe.");
            }

            if (!string.IsNullOrWhiteSpace(apiToken))
            {
                throw new ArgumentException("PCV_HOST_INLINE_TOKEN_FORBIDDEN|Inline API token values are forbidden for the .NET service host.|Use --api-token-credential-target for service mode.");
            }

            var apiHost = new Uri(prefix).DnsSafeHost.ToLowerInvariant();
            if (!IsLoopbackHost(apiHost) && !allowLan)
            {
                throw new ArgumentException($"PCV_HOST_PREFIX_NOT_LOOPBACK|The .NET service host prefix must stay on loopback unless LAN mode is explicit.|Rejected host '{apiHost}'.");
            }

            if (!string.IsNullOrWhiteSpace(webPrefix))
            {
                if (string.IsNullOrWhiteSpace(webRoot))
                {
                    throw new ArgumentException("PCV_HOST_WEB_ROOT_REQUIRED|A separate Web Console prefix requires a static web root.|Pass --web-root with --web-prefix.");
                }

                var webHost = new Uri(webPrefix).DnsSafeHost.ToLowerInvariant();
                if (!IsLoopbackHost(webHost) && !allowLan)
                {
                    throw new ArgumentException($"PCV_HOST_WEB_PREFIX_NOT_LOOPBACK|The Web Console prefix must stay on loopback unless LAN mode is explicit.|Rejected host '{webHost}'.");
                }
            }

            if (allowLan &&
                string.IsNullOrWhiteSpace(protectedToken) &&
                string.IsNullOrWhiteSpace(tokenFile) &&
                string.IsNullOrWhiteSpace(credentialTarget))
            {
                throw new ArgumentException("PCV_HOST_LAN_TOKEN_REQUIRED|LAN mode requires a token source.|Use --api-token-credential-target for service mode.");
            }

            if (string.IsNullOrWhiteSpace(noVncTargetHost) != !noVncTargetPort.HasValue)
            {
                throw new ArgumentException("PCV_HOST_NOVNC_TARGET_INCOMPLETE|noVNC bridge requires both --novnc-target-host and --novnc-target-port.|Pass both values or neither.");
            }

            if (!string.IsNullOrWhiteSpace(noVncTargetHost))
            {
                if (noVncTargetPort is < 1 or > 65535)
                {
                    throw new ArgumentException("PCV_HOST_NOVNC_TARGET_PORT_INVALID|noVNC target port must be between 1 and 65535.|Pass a valid TCP port.");
                }

                if (!noVncWebSocketPath.StartsWith("/", StringComparison.Ordinal) ||
                    !noVncWebSocketPath.Contains("{vm_id}", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("PCV_HOST_NOVNC_WEBSOCKET_PATH_INVALID|noVNC websocket path must start with / and include {vm_id}.|Use /api/v1/console/novnc/{vm_id}.");
                }

                if (!IsLoopbackHost(noVncTargetHost.ToLowerInvariant()) && !allowLan)
                {
                    throw new ArgumentException($"PCV_HOST_NOVNC_TARGET_NOT_LOOPBACK|The noVNC bridge target must stay on loopback unless LAN mode is explicit.|Rejected host '{noVncTargetHost}'.");
                }
            }

            return new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = prefix,
                WebPrefix = webPrefix,
                WebRootPath = webRoot,
                JobStorePath = GetValue(values, "--job-store"),
                EventLogPath = GetValue(values, "--event-log"),
                EventLogProviderSource = GetValue(values, "--event-log-provider-source"),
                EventLogProviderLogName = GetValue(values, "--event-log-provider-log"),
                EventLogWriter = GetValue(values, "--event-log-writer"),
                EventLogSchemaVersion = ParseRangedInt(values, "--event-log-schema-version", 1, 1, 100),
                DiagnosticsRootPath = GetValue(values, "--diagnostics-root"),
                BatchEvidenceRootPath = NormalizeOptionalPath(GetValue(values, "--batch-evidence-root")),
                ApiTokenFile = tokenFile,
                ApiTokenProtectedFile = protectedToken,
                AccountFilePath = accountFile,
                JwtSigningKeyFilePath = jwtSigningKeyFile,
                NoVncTargetHost = noVncTargetHost,
                NoVncTargetPort = noVncTargetPort,
                NoVncWebSocketPath = noVncWebSocketPath,
                ApiTokenCredentialTarget = credentialTarget,
                RouteTimeoutSeconds = ParseRangedInt(values, "--route-timeout-seconds", 30, 1, 3600),
                RequestLimitPerMinute = ParseRangedInt(values, "--request-limit-per-minute", 120, 1, 100000),
                RequestBurstLimit = ParseRangedInt(values, "--request-burst-limit", 20, 0, 10000),
                RetryAfterSeconds = ParseRangedInt(values, "--retry-after-seconds", 15, 1, 3600),
                ControlledRouteTimeoutProbeDelayMilliseconds = ParseRangedInt(values, "--controlled-route-timeout-probe-delay-ms", 0, 0, 600000),
                MaxRequestBodyBytes = ParseRangedInt(
                    values,
                    "--max-request-body-bytes",
                    DefaultMaxRequestBodyBytes,
                    MinimumMaxRequestBodyBytes,
                    MaximumMaxRequestBodyBytes),
                RequestLifetimeMode = requestLifetimeMode,
                RequestAdmissionActiveLimit = ParseRangedInt(values, "--request-admission-active", 32, 1, 1024),
                RequestAdmissionWaitingLimit = ParseRangedInt(values, "--request-admission-waiting", 64, 0, 4096),
                AllowLan = allowLan
            };
        }

        if (string.Equals(args[0], "service-action", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Count < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new ArgumentException("PCV_HOST_SERVICE_ACTION_REQUIRED|A service action name is required.|Use configure-installed, repair-installed, or remove-installed.");
            }

            var values = ParseNamedArguments(args.Skip(2).ToArray());
            return new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.ServiceAction,
                ServiceAction = args[1].ToLowerInvariant(),
                ProductRoot = GetValue(values, "--product-root"),
                DataRoot = GetValue(values, "--data-root"),
                ServiceExecutablePath = GetValue(values, "--service-exe"),
                AllowLan = values.ContainsKey("--allow-lan"),
                RemoveData = values.ContainsKey("--remove-data"),
                DryRun = values.ContainsKey("--dry-run"),
                ReleaseApproved = values.ContainsKey("--release-approved"),
                FirewallRuleName = GetValue(values, "--firewall-rule-name"),
                FirewallLocalPort = ParseOptionalInt(values, "--firewall-local-port"),
                FirewallProfile = GetValue(values, "--firewall-profile"),
                FirewallRemoteAddress = GetValue(values, "--firewall-remote-address"),
                TrustRootCertificatePath = GetValue(values, "--trust-root-certificate"),
                TrustRootCertificateThumbprint = GetValue(values, "--trust-root-thumbprint"),
                TrustPublisherCertificatePath = GetValue(values, "--trust-publisher-certificate"),
                TrustPublisherCertificateThumbprint = GetValue(values, "--trust-publisher-thumbprint"),
                MigrationPlanId = GetValue(values, "--migration-plan-id"),
                MigrationPlanVersion = ParseOptionalInt(values, "--migration-plan-version"),
                CredentialTarget = GetValue(values, "--credential-target"),
                BatchEvidenceRootPath = NormalizeOptionalPath(GetValue(values, "--batch-evidence-root")),
                RouteTimeoutSeconds = ParseRangedInt(values, "--route-timeout-seconds", 30, 1, 3600),
                RequestLimitPerMinute = ParseRangedInt(values, "--request-limit-per-minute", 120, 1, 100000),
                RequestBurstLimit = ParseRangedInt(values, "--request-burst-limit", 20, 0, 10000),
                RetryAfterSeconds = ParseRangedInt(values, "--retry-after-seconds", 15, 1, 3600),
                ControlledRouteTimeoutProbeDelayMilliseconds = ParseRangedInt(values, "--controlled-route-timeout-probe-delay-ms", 0, 0, 600000),
                EventLogDefaultTransitionTimeoutSeconds = ParseRangedInt(values, "--eventlog-default-transition-timeout-seconds", 60, 1, 600),
                MaxRequestBodyBytes = ParseRangedInt(
                    values,
                    "--max-request-body-bytes",
                    DefaultMaxRequestBodyBytes,
                    MinimumMaxRequestBodyBytes,
                    MaximumMaxRequestBodyBytes)
            };
        }

        throw new ArgumentException($"PCV_HOST_MODE_INVALID|The .NET service host mode is invalid.|Unsupported mode '{args[0]}'.");
    }

    private static Dictionary<string, string?> ParseNamedArguments(IReadOnlyList<string> args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Count; index++)
        {
            var name = args[index];
            if (!name.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"PCV_HOST_ARGUMENT_INVALID|Unexpected argument '{name}'.|Arguments must use --name value format.");
            }

            if (name is "--allow-lan" or "--remove-data" or "--dry-run" or "--release-approved")
            {
                values[name] = "true";
                continue;
            }

            if (index + 1 >= args.Count)
            {
                throw new ArgumentException($"PCV_HOST_ARGUMENT_VALUE_REQUIRED|Argument '{name}' requires a value.|Pass a non-empty value after {name}.");
            }

            values[name] = args[++index];
        }

        return values;
    }

    private static string? GetValue(IReadOnlyDictionary<string, string?> values, string name)
    {
        return values.TryGetValue(name, out var value) ? value : null;
    }

    private static string? NormalizeOptionalPath(string? path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? null
            : Path.GetFullPath(path);
    }

    private static bool IsLoopbackHost(string host)
    {
        return host is "127.0.0.1" or "localhost" or "::1";
    }

    private static int? ParseOptionalInt(IReadOnlyDictionary<string, string?> values, string name)
    {
        var value = GetValue(values, name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!int.TryParse(value, out var parsed))
        {
            throw new ArgumentException($"PCV_HOST_ARGUMENT_VALUE_INVALID|Argument '{name}' must be an integer.|Rejected value '{value}'.");
        }

        return parsed;
    }

    private static int ParseRangedInt(
        IReadOnlyDictionary<string, string?> values,
        string name,
        int defaultValue,
        int minimum,
        int maximum)
    {
        var value = GetValue(values, name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var parsed) || parsed < minimum || parsed > maximum)
        {
            throw new ArgumentException($"PCV_HOST_ARGUMENT_VALUE_INVALID|Argument '{name}' must be an integer between {minimum} and {maximum}.|Rejected value '{value}'.");
        }

        return parsed;
    }

    private static DesktopNodeRequestLifetimeMode ParseRequestLifetimeMode(
        IReadOnlyDictionary<string, string?> values,
        string name)
    {
        var value = GetValue(values, name);
        if (string.IsNullOrWhiteSpace(value) ||
            string.Equals(value, "legacy", StringComparison.OrdinalIgnoreCase))
        {
            return DesktopNodeRequestLifetimeMode.Legacy;
        }

        if (string.Equals(value, "tracked_async_serialized", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "tracked-async-serialized", StringComparison.OrdinalIgnoreCase))
        {
            return DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized;
        }

        throw new ArgumentException(
            $"PCV_HOST_ARGUMENT_VALUE_INVALID|Argument '{name}' must be 'legacy' or 'tracked_async_serialized'.|Rejected value '{value}'.");
    }
}

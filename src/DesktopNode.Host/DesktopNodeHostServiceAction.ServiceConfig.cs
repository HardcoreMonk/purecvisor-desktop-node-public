using System.Text;

namespace DesktopNode.Host;

public static partial class DesktopNodeHostServiceAction
{
    internal static bool IsStopped(DesktopNodeWindowsServiceSnapshot snapshot)
    {
        return snapshot.Exists && string.Equals(snapshot.Status, "stopped", StringComparison.OrdinalIgnoreCase);
    }

    internal static DesktopNodeWindowsServiceConfiguration CreateServiceConfiguration(
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeHostOptions options,
        bool useCredentialManagerToken = false,
        string? credentialTargetOverride = null,
        string? batchEvidenceRootOverride = null)
    {
        var productRoot = Require(options.ProductRoot, "PCV_HOST_PRODUCT_ROOT_REQUIRED");
        var dataRoot = Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED");
        var prefix = "http://127.0.0.1:7777/";
        var webPrefix = "http://127.0.0.1:80/";
        var protectedToken = Path.Combine(dataRoot, "api-token.dpapi.json");
        var accountFile = Path.Combine(dataRoot, "accounts.json");
        var jwtSigningKeyFile = Path.Combine(dataRoot, "jwt-signing-key.txt");
        var credentialTarget = credentialTargetOverride ?? plan.CredentialTarget ?? "PureCVisor/PureCVisorDesktopNode/api-token";
        var jobStore = Path.Combine(dataRoot, "jobs.json");
        var eventLog = Path.Combine(dataRoot, "events.jsonl");
        var diagnosticsRoot = Path.Combine(dataRoot, "diagnostics");
        var webRoot = Path.Combine(productRoot, "web");
        var batchEvidenceRoot = string.IsNullOrWhiteSpace(batchEvidenceRootOverride)
            ? options.BatchEvidenceRootPath
            : batchEvidenceRootOverride;
        batchEvidenceRoot = NormalizeOptionalPath(batchEvidenceRoot);
        var tokenArguments = useCredentialManagerToken
            ? new[] { "--api-token-credential-target", Quote(credentialTarget) }
            : ["--api-token-protected-file", Quote(protectedToken)];
        var routeTimeoutSeconds = options.RouteTimeoutSeconds > 0 ? options.RouteTimeoutSeconds : 30;
        var requestLimitPerMinute = options.RequestLimitPerMinute > 0 ? options.RequestLimitPerMinute : 120;
        var requestBurstLimit = options.RequestBurstLimit >= 0 ? options.RequestBurstLimit : 20;
        var retryAfterSeconds = options.RetryAfterSeconds > 0 ? options.RetryAfterSeconds : 15;
        var maxRequestBodyBytes = Math.Clamp(
            options.MaxRequestBodyBytes,
            DesktopNodeHostOptions.MinimumMaxRequestBodyBytes,
            DesktopNodeHostOptions.MaximumMaxRequestBodyBytes);
        var controlledRouteTimeoutProbeDelayMilliseconds = Math.Clamp(
            options.ControlledRouteTimeoutProbeDelayMilliseconds,
            0,
            600_000);
        var arguments = new List<string>
        {
            Quote(plan.ServiceExecutablePath),
            "listen",
            "--prefix",
            Quote(prefix),
            "--web-prefix",
            Quote(webPrefix),
            "--web-root",
            Quote(webRoot),
            "--job-store",
            Quote(jobStore),
            "--event-log",
            Quote(eventLog),
            "--event-log-provider-source",
            Quote("PureCVisor Desktop Node"),
            "--event-log-provider-log",
            Quote("Application"),
            "--event-log-writer",
            Quote("windows-event-log"),
            "--event-log-schema-version",
            "1",
            "--diagnostics-root",
            Quote(diagnosticsRoot),
            tokenArguments[0],
            tokenArguments[1],
            "--account-file",
            Quote(accountFile),
            "--jwt-signing-key-file",
            Quote(jwtSigningKeyFile),
            "--route-timeout-seconds",
            routeTimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--request-limit-per-minute",
            requestLimitPerMinute.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--request-burst-limit",
            requestBurstLimit.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--retry-after-seconds",
            retryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--max-request-body-bytes",
            maxRequestBodyBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        AddOptionalQuotedArgument(arguments, "--batch-evidence-root", batchEvidenceRoot);
        if (controlledRouteTimeoutProbeDelayMilliseconds > 0)
        {
            arguments.Add("--controlled-route-timeout-probe-delay-ms");
            arguments.Add(controlledRouteTimeoutProbeDelayMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        var binPath = string.Join(" ", arguments);

        return new DesktopNodeWindowsServiceConfiguration(
            ServiceName: plan.ServiceName,
            DisplayName: "PureCVisor Desktop Node",
            Description: "PureCVisor Desktop Node Local API service.",
            BinaryPathName: binPath,
            ServiceAccount: "LocalSystem",
            AutoStart: true,
            FailureResetPeriodSeconds: 86400,
            FailureActions: [
                new DesktopNodeWindowsServiceFailureAction("restart", TimeSpan.FromSeconds(60)),
                new DesktopNodeWindowsServiceFailureAction("restart", TimeSpan.FromSeconds(60)),
                new DesktopNodeWindowsServiceFailureAction("none", TimeSpan.FromSeconds(60))
            ]);
    }

    private static void AddOptionalQuotedArgument(ICollection<string> arguments, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        arguments.Add(name);
        arguments.Add(Quote(value));
    }

    private static string? NormalizeOptionalPath(string? path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? null
            : Path.GetFullPath(path);
    }

    internal static string? ExtractNamedArgumentValue(string? commandLine, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return null;
        }

        var index = 0;
        while (index < commandLine.Length)
        {
            var matchIndex = commandLine.IndexOf(argumentName, index, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                return null;
            }

            var beforeOk = matchIndex == 0 || char.IsWhiteSpace(commandLine[matchIndex - 1]);
            var valueIndex = matchIndex + argumentName.Length;
            var afterOk = valueIndex >= commandLine.Length || char.IsWhiteSpace(commandLine[valueIndex]);
            if (!beforeOk || !afterOk)
            {
                index = valueIndex;
                continue;
            }

            while (valueIndex < commandLine.Length && char.IsWhiteSpace(commandLine[valueIndex]))
            {
                valueIndex++;
            }

            if (valueIndex >= commandLine.Length)
            {
                return null;
            }

            if (commandLine[valueIndex] == '"')
            {
                var closingQuote = commandLine.IndexOf('"', valueIndex + 1);
                return closingQuote > valueIndex
                    ? commandLine[(valueIndex + 1)..closingQuote]
                    : null;
            }

            var endIndex = valueIndex;
            while (endIndex < commandLine.Length && !char.IsWhiteSpace(commandLine[endIndex]))
            {
                endIndex++;
            }

            return commandLine[valueIndex..endIndex];
        }

        return null;
    }
}

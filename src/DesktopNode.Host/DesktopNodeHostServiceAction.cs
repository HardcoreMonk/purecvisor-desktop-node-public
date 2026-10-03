using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host;

public sealed record DesktopNodeHostCommand(string FileName, IReadOnlyList<string> Arguments);

public sealed record DesktopNodeHostServiceActionPlan(
    string ServiceMode,
    string OperationFamily,
    string ServiceName,
    string ServiceExecutablePath,
    string ServiceBinaryPathName,
    IReadOnlyList<DesktopNodeHostCommand> Commands,
    bool RemoveData,
    IReadOnlyList<string> RemoveDataPaths,
    string? NativeServiceOperation,
    string? NativeServiceTokenOperation = null,
    string? NativeEventLogOperation = null,
    string? EventLogName = null,
    string? EventLogSourceName = null,
    string? NativeFirewallOperation = null,
    DesktopNodeWindowsFirewallRuleSpec? FirewallRule = null,
    bool LanExposureApproved = false,
    string? NativeTrustStoreOperation = null,
    IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> TrustStoreCertificates = null!,
    bool ReleaseApproved = false,
    string? NativeConfigMigrationOperation = null,
    string? NativeJobStoreMigrationOperation = null,
    string? NativeCredentialManagerOperation = null,
    string? NativeDataRootLifecycleOperation = null,
    string? CredentialTarget = null,
    string? NativeSwitchOperation = null);

public sealed record DesktopNodeHostCommandResult(
    string FileName,
    IReadOnlyList<string> Arguments,
    int ExitCode,
    string Stdout,
    string Stderr,
    bool Ok);

public sealed record DesktopNodeHostRemoveDataHandoff(
    string Operation,
    string DataRoot,
    IReadOnlyList<string> Paths,
    string RequiredAction,
    string Reason);

public sealed record DesktopNodeHostServiceActionResult(
    bool Ok,
    string Action,
    DesktopNodeHostServiceActionPlan Plan,
    IReadOnlyList<DesktopNodeHostCommandResult> Commands,
    IReadOnlyList<string> RemovedPaths,
    string? PreparedTokenPath,
    DesktopNodeWindowsServiceSnapshot? Service,
    bool ServiceOwnerVerified,
    string? ErrorCode,
    string? ErrorMessage,
    DesktopNodeHostRemoveDataHandoff? RemoveDataHandoff = null,
    DesktopNodeWindowsEventLogSnapshot? EventLog = null,
    DesktopNodeWindowsFirewallRuleSnapshot? FirewallRule = null,
    IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot>? TrustStoreCertificates = null,
    DesktopNodeHostConfigMigrationDescriptor? ConfigMigration = null,
    DesktopNodeHostJobStoreMigrationDescriptor? JobStoreMigration = null,
    DesktopNodeHostServiceTokenRotationDescriptor? ServiceTokenRotation = null,
    DesktopNodeHostEventLogHardeningDescriptor? EventLogHardening = null,
    DesktopNodeHostCredentialManagerProofDescriptor? CredentialManagerProof = null,
    DesktopNodeHostCredentialManagerTransitionDescriptor? CredentialManagerTransition = null,
    DesktopNodeHyperVSwitchMutationSnapshot? HyperVSwitch = null);

public static partial class DesktopNodeHostServiceAction
{
    public static DesktopNodeHostServiceActionPlan CreatePlan(DesktopNodeHostOptions options)
    {
        var serviceName = "PureCVisorDesktopNode";
        var productRoot = Require(options.ProductRoot, "PCV_HOST_PRODUCT_ROOT_REQUIRED");
        var serviceExe = Require(options.ServiceExecutablePath, "PCV_HOST_SERVICE_EXE_REQUIRED");
        var action = Require(options.ServiceAction, "PCV_HOST_SERVICE_ACTION_REQUIRED");
        var isNativeServiceAction = IsNativeServiceAction(action);
        var isNativeServiceTokenAction = IsNativeServiceTokenAction(action);
        var isNativeEventLogAction = IsNativeEventLogAction(action);
        var isNativeFirewallAction = IsNativeFirewallAction(action);
        var isNativeTrustStoreAction = IsNativeTrustStoreAction(action);
        var isNativeConfigMigrationAction = IsNativeConfigMigrationAction(action);
        var isNativeJobStoreMigrationAction = IsNativeJobStoreMigrationAction(action);
        var isNativeCredentialManagerAction = IsNativeCredentialManagerAction(action);
        var isNativeDataRootLifecycleAction = IsNativeDataRootLifecycleAction(action);
        var isNativeSwitchAction = IsNativeSwitchAction(action);
        var dataRoot = RequiresDataRoot(action) ? Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED") : null;
        var credentialTarget = string.IsNullOrWhiteSpace(options.CredentialTarget)
            ? "PureCVisor/PureCVisorDesktopNode/api-token"
            : options.CredentialTarget;

        var prefix = "http://127.0.0.1:7777/";
        var webPrefix = "http://127.0.0.1:80/";
        var protectedToken = dataRoot is null ? null : Path.Combine(dataRoot, "api-token.dpapi.json");
        var accountFile = dataRoot is null ? null : Path.Combine(dataRoot, "accounts.json");
        var jwtSigningKeyFile = dataRoot is null ? null : Path.Combine(dataRoot, "jwt-signing-key.txt");
        var jobStore = dataRoot is null ? null : Path.Combine(dataRoot, "jobs.json");
        var eventLog = dataRoot is null ? null : Path.Combine(dataRoot, "events.jsonl");
        var diagnosticsRoot = dataRoot is null ? null : Path.Combine(dataRoot, "diagnostics");
        var webRoot = Path.Combine(productRoot, "web");
        var routeTimeoutSeconds = options.RouteTimeoutSeconds > 0 ? options.RouteTimeoutSeconds : 30;
        var requestLimitPerMinute = options.RequestLimitPerMinute > 0 ? options.RequestLimitPerMinute : 120;
        var requestBurstLimit = options.RequestBurstLimit >= 0 ? options.RequestBurstLimit : 20;
        var retryAfterSeconds = options.RetryAfterSeconds > 0 ? options.RetryAfterSeconds : 15;
        var maxRequestBodyBytes = Math.Clamp(
            options.MaxRequestBodyBytes,
            DesktopNodeHostOptions.MinimumMaxRequestBodyBytes,
            DesktopNodeHostOptions.MaximumMaxRequestBodyBytes);
        var batchEvidenceRoot = NormalizeOptionalPath(options.BatchEvidenceRootPath);
        var controlledRouteTimeoutProbeDelayMilliseconds = Math.Clamp(
            options.ControlledRouteTimeoutProbeDelayMilliseconds,
            0,
            600_000);

        var binPathArguments = dataRoot is null
            ? []
            : new List<string>
            {
                Quote(serviceExe),
                "listen",
                "--prefix",
                Quote(prefix),
                "--web-prefix",
                Quote(webPrefix),
                "--web-root",
                Quote(webRoot),
                "--job-store",
                Quote(jobStore!),
                "--event-log",
                Quote(eventLog!),
                "--event-log-provider-source",
                Quote("PureCVisor Desktop Node"),
                "--event-log-provider-log",
                Quote("Application"),
                "--event-log-writer",
                Quote("windows-event-log"),
                "--event-log-schema-version",
                "1",
                "--diagnostics-root",
                Quote(diagnosticsRoot!),
                "--api-token-protected-file",
                Quote(protectedToken!),
                "--account-file",
                Quote(accountFile!),
                "--jwt-signing-key-file",
                Quote(jwtSigningKeyFile!),
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
        AddOptionalQuotedArgument(binPathArguments, "--batch-evidence-root", batchEvidenceRoot);
        if (controlledRouteTimeoutProbeDelayMilliseconds > 0)
        {
            binPathArguments.Add("--controlled-route-timeout-probe-delay-ms");
            binPathArguments.Add(controlledRouteTimeoutProbeDelayMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        var binPath = string.Join(" ", binPathArguments);

        IReadOnlyList<DesktopNodeHostCommand> commands = action switch
        {
            "status" or "start" or "stop" or "configure-installed" or "repair-installed" or "remove-installed" or "data-root-remove" or "service-token-rotation-revoke" or "credential-manager-default-transition" or
            "credential-manager-system-proof" or
            "eventlog-register" or "eventlog-remove" or "eventlog-repair" or "eventlog-write-test" or "eventlog-volume-guard" or "eventlog-default-transition" or
            "firewall-enable" or "firewall-remove" or
            "switch-create" or "switch-remove" or
            "trust-store-install" or "trust-store-remove" or
            "config-migration-apply" or "job-store-migration-apply" => [],
            _ => throw new ArgumentException($"PCV_HOST_SERVICE_ACTION_INVALID|The service action is not supported.|{action}")
        };
        var removeDataPaths = dataRoot is null
            ? []
            : new[]
            {
                Path.Combine(dataRoot, "api-token.txt"),
                Path.Combine(dataRoot, "api-token.dpapi.json"),
                Path.Combine(dataRoot, "accounts.json"),
                Path.Combine(dataRoot, "jwt-signing-key.txt"),
                Path.Combine(dataRoot, "jobs.json"),
                Path.Combine(dataRoot, "jobs.json.tmp"),
                Path.Combine(dataRoot, "jobs.json.commit-pending"),
                Path.Combine(dataRoot, "events.jsonl"),
                Path.Combine(dataRoot, "install.jsonl"),
                Path.Combine(dataRoot, "diagnostics")
            };

        return new DesktopNodeHostServiceActionPlan(
            ServiceMode: "dotnet-windows-service",
            OperationFamily: ResolveOperationFamily(action),
            ServiceName: serviceName,
            ServiceExecutablePath: serviceExe,
            ServiceBinaryPathName: binPath,
            Commands: commands,
            RemoveData: options.RemoveData,
            RemoveDataPaths: removeDataPaths,
            NativeServiceOperation: isNativeServiceAction ? action : null,
            NativeServiceTokenOperation: isNativeServiceTokenAction ? action : null,
            NativeEventLogOperation: isNativeEventLogAction ? action : null,
            EventLogName: isNativeEventLogAction ? "Application" : null,
            EventLogSourceName: isNativeEventLogAction ? "PureCVisor Desktop Node" : null,
            NativeFirewallOperation: isNativeFirewallAction ? action : null,
            FirewallRule: isNativeFirewallAction ? CreateFirewallRuleSpec(options) : null,
            LanExposureApproved: options.AllowLan,
            NativeTrustStoreOperation: isNativeTrustStoreAction ? action : null,
            TrustStoreCertificates: isNativeTrustStoreAction ? CreateTrustStoreCertificateSpecs(options, action) : [],
            ReleaseApproved: options.ReleaseApproved,
            NativeConfigMigrationOperation: isNativeConfigMigrationAction ? action : null,
            NativeJobStoreMigrationOperation: isNativeJobStoreMigrationAction ? action : null,
            NativeCredentialManagerOperation: isNativeCredentialManagerAction ? action : null,
            NativeDataRootLifecycleOperation: isNativeDataRootLifecycleAction ? action : null,
            CredentialTarget: (isNativeCredentialManagerAction || string.Equals(action, "credential-manager-default-transition", StringComparison.Ordinal)) ? credentialTarget : null,
            NativeSwitchOperation: isNativeSwitchAction ? action : null);
    }

    public static async Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
        DesktopNodeHostOptions options,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(options, serviceController: null, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
        DesktopNodeHostOptions options,
        IDesktopNodeWindowsServiceController? serviceController,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            options,
            serviceController,
            eventLogController: null,
            firewallController: null,
            trustStoreController: null,
            credentialManagerController: null,
            cancellationToken).ConfigureAwait(false);
    }

    public static Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
        DesktopNodeHostOptions options,
        IDesktopNodeHyperVSwitchController switchController,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(switchController);
        return ExecuteAsync(
            options,
            serviceController: null,
            eventLogController: null,
            firewallController: null,
            trustStoreController: null,
            credentialManagerController: null,
            DesktopNodeHostFileAclHardener.Instance,
            switchController,
            cancellationToken);
    }

    public static async Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
        DesktopNodeHostOptions options,
        IDesktopNodeWindowsServiceController? serviceController,
        IDesktopNodeWindowsEventLogController? eventLogController,
        IDesktopNodeWindowsFirewallController? firewallController = null,
        IDesktopNodeWindowsTrustStoreController? trustStoreController = null,
        IDesktopNodeWindowsCredentialManagerController? credentialManagerController = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            options,
            serviceController,
            eventLogController,
            firewallController,
            trustStoreController,
            credentialManagerController,
            DesktopNodeHostFileAclHardener.Instance,
            switchController: null,
            cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
        DesktopNodeHostOptions options,
        IDesktopNodeWindowsServiceController? serviceController,
        IDesktopNodeWindowsEventLogController? eventLogController,
        IDesktopNodeWindowsFirewallController? firewallController,
        IDesktopNodeWindowsTrustStoreController? trustStoreController,
        IDesktopNodeWindowsCredentialManagerController? credentialManagerController,
        IDesktopNodeHostFileAclHardener fileAclHardener,
        IDesktopNodeHyperVSwitchController? switchController = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileAclHardener);

        var plan = CreatePlan(options);
        var results = new List<DesktopNodeHostCommandResult>();
        var removedPaths = new List<string>();

        if (plan.NativeEventLogOperation is not null)
        {
            return Ops.DesktopNodeEventLogOps.Execute(
                options,
                plan,
                eventLogController ?? new DesktopNodeWindowsEventLogController());
        }

        if (plan.NativeFirewallOperation is not null)
        {
            return Ops.DesktopNodeFirewallOps.Execute(
                options,
                plan,
                firewallController ?? new DesktopNodeWindowsFirewallController());
        }

        if (plan.NativeSwitchOperation is not null)
        {
            return Ops.DesktopNodeHyperVSwitchOps.Execute(
                options,
                plan,
                switchController ?? new DesktopNodeHyperVSwitchController());
        }

        if (plan.NativeTrustStoreOperation is not null)
        {
            return Ops.DesktopNodeTrustStoreOps.Execute(
                options,
                plan,
                trustStoreController ?? new DesktopNodeWindowsTrustStoreController());
        }

        if (plan.NativeCredentialManagerOperation is not null)
        {
            return Ops.DesktopNodeCredentialManagerOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController(),
                credentialManagerController ?? new DesktopNodeWindowsCredentialManagerController(),
                fileAclHardener);
        }

        if (plan.NativeDataRootLifecycleOperation is not null)
        {
            return Ops.DesktopNodeDataRootLifecycleOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController());
        }

        if (plan.NativeConfigMigrationOperation is not null)
        {
            return Ops.DesktopNodeConfigMigrationOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController());
        }

        if (plan.NativeJobStoreMigrationOperation is not null)
        {
            return Ops.DesktopNodeJobStoreMigrationOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController());
        }

        if (plan.NativeServiceTokenOperation is not null)
        {
            return Ops.DesktopNodeServiceTokenOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController(),
                fileAclHardener);
        }

        if (plan.NativeServiceOperation is not null)
        {
            return Ops.DesktopNodeServiceLifecycleOps.Execute(
                options,
                plan,
                serviceController ?? new DesktopNodeWindowsServiceController(),
                credentialManagerController ?? new DesktopNodeWindowsCredentialManagerController(),
                fileAclHardener);
        }

        if (!options.DryRun)
        {
            var preparedTokenPath = default(string);
            if (options.ServiceAction is "configure-installed" or "repair-installed")
            {
                preparedTokenPath = EnsureProtectedTokenFile(options.DataRoot!, fileAclHardener);
            }

            foreach (var command in plan.Commands)
            {
                var result = await InvokeCommandAsync(command, cancellationToken).ConfigureAwait(false);
                results.Add(result);
                if (!IsAllowed(command, result.ExitCode))
                {
                    return new DesktopNodeHostServiceActionResult(
                        Ok: false,
                        Action: options.ServiceAction ?? string.Empty,
                        Plan: plan,
                        Commands: results,
                        RemovedPaths: removedPaths,
                        PreparedTokenPath: preparedTokenPath,
                        Service: null,
                        ServiceOwnerVerified: false,
                        ErrorCode: "PCV_HOST_SERVICE_ACTION_COMMAND_FAILED",
                        ErrorMessage: $"{command.FileName} {string.Join(' ', command.Arguments)} exited with {result.ExitCode}.");
                }

                if (IsStopCommand(command) && result.ExitCode == 0)
                {
                    var waitResult = await WaitForServiceStoppedAsync(plan.ServiceName, cancellationToken).ConfigureAwait(false);
                    results.Add(waitResult);
                    if (!waitResult.Ok)
                    {
                        return new DesktopNodeHostServiceActionResult(
                            Ok: false,
                            Action: options.ServiceAction ?? string.Empty,
                            Plan: plan,
                            Commands: results,
                            RemovedPaths: removedPaths,
                            PreparedTokenPath: preparedTokenPath,
                            Service: null,
                            ServiceOwnerVerified: false,
                            ErrorCode: "PCV_HOST_SERVICE_STOP_TIMEOUT",
                            ErrorMessage: "Desktop Node service did not stop before the timeout.");
                    }
                }
            }

        }

        return new DesktopNodeHostServiceActionResult(
            Ok: true,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: results,
            RemovedPaths: removedPaths,
            PreparedTokenPath: options.DryRun ? null : EnsureResultTokenPath(options),
            Service: null,
            ServiceOwnerVerified: false,
            ErrorCode: null,
            ErrorMessage: null);
    }
}

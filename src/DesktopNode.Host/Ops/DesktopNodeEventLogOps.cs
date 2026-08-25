using System.Text;
using System.Text.Json;
using DesktopNode.Host;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeEventLogOps
{
    public const string OperationFamily = "event-log";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsEventLogController eventLogController)
    {
        if (options.DryRun)
        {
            return new DesktopNodeHostServiceActionResult(
                Ok: true,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: null,
                ServiceOwnerVerified: false,
                ErrorCode: null,
                ErrorMessage: null);
        }

        var logName = plan.EventLogName ?? "Application";
        var sourceName = plan.EventLogSourceName ?? "PureCVisor Desktop Node";
        var current = eventLogController.Query(logName, sourceName, plan.ServiceExecutablePath);

        if (current.Exists && !current.Owned)
        {
            return NativeEventLogFailure(
                options,
                plan,
                current,
                "PCV_HOST_EVENTLOG_SOURCE_OWNERSHIP_MISMATCH",
                $"Windows Event Log source '{sourceName}' exists but is not owned by '{plan.ServiceExecutablePath}'.");
        }

        try
        {
            if (string.Equals(plan.NativeEventLogOperation, "eventlog-register", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.Exists
                    ? current
                    : eventLogController.Register(logName, sourceName, plan.ServiceExecutablePath);
                var ok = next.Exists && next.Owned;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_SOURCE_REGISTER_FAILED",
                    ErrorMessage: ok ? null : $"Windows Event Log source '{sourceName}' was not registered with the expected binding.",
                    EventLog: next);
            }

            if (string.Equals(plan.NativeEventLogOperation, "eventlog-remove", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.Exists
                    ? eventLogController.Remove(logName, sourceName, plan.ServiceExecutablePath)
                    : current;
                var ok = !next.Exists;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_SOURCE_REMOVE_FAILED",
                    ErrorMessage: ok ? null : $"Windows Event Log source '{sourceName}' still exists after remove.",
                    EventLog: next);
            }

            if (string.Equals(plan.NativeEventLogOperation, "eventlog-repair", StringComparison.OrdinalIgnoreCase))
            {
                var next = eventLogController.Register(logName, sourceName, plan.ServiceExecutablePath);
                var ok = next.Exists && next.Owned;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_REPAIR_FAILED",
                    ErrorMessage: ok ? null : $"Windows Event Log source '{sourceName}' was not repaired with the expected binding.",
                    EventLog: next,
                    EventLogHardening: new DesktopNodeHostEventLogHardeningDescriptor(
                        Operation: "windows-event-log-provider-hardening",
                        Ok: ok,
                        ProviderRepairStatus: ok ? "provider-repair-pass" : "provider-repair-failed",
                        EventWriteStatus: "not-run",
                        VolumeGuardStatus: "not-run",
                        EventId: null,
                        HostMutationPerformed: true));
            }

            if (string.Equals(plan.NativeEventLogOperation, "eventlog-write-test", StringComparison.OrdinalIgnoreCase))
            {
                if (!current.Exists || !current.Owned)
                {
                    return NativeEventLogFailure(
                        options,
                        plan,
                        current,
                        "PCV_HOST_EVENTLOG_SOURCE_REQUIRED",
                        $"Windows Event Log source '{sourceName}' must be registered before writing a test event.");
                }

                const int eventId = 39100;
                var writeStatus = eventLogController.WriteTestEvent(
                    logName,
                    sourceName,
                    eventId,
                    "PureCVisor Desktop Node Event Log provider hardening write test.");
                var ok = string.Equals(writeStatus, "write-query-pass", StringComparison.OrdinalIgnoreCase);
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_WRITE_TEST_FAILED",
                    ErrorMessage: ok ? null : $"Windows Event Log write test returned '{writeStatus}'.",
                    EventLog: current,
                    EventLogHardening: new DesktopNodeHostEventLogHardeningDescriptor(
                        Operation: "windows-event-log-provider-hardening",
                        Ok: ok,
                        ProviderRepairStatus: "not-run",
                        EventWriteStatus: writeStatus,
                        VolumeGuardStatus: "not-run",
                        EventId: eventId,
                        HostMutationPerformed: true));
            }

            if (string.Equals(plan.NativeEventLogOperation, "eventlog-volume-guard", StringComparison.OrdinalIgnoreCase))
            {
                var volumePolicy = eventLogController.QueryVolumePolicy(logName);
                var ok = volumePolicy.VolumeGuarded;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_VOLUME_GUARD_FAILED",
                    ErrorMessage: ok ? null : $"Windows Event Log '{logName}' does not have a bounded overwrite policy.",
                    EventLog: current,
                    EventLogHardening: new DesktopNodeHostEventLogHardeningDescriptor(
                        Operation: "windows-event-log-provider-hardening",
                        Ok: ok,
                        ProviderRepairStatus: "not-run",
                        EventWriteStatus: "not-run",
                        VolumeGuardStatus: ok ? "volume-guard-pass" : "volume-guard-failed",
                        EventId: null,
                        HostMutationPerformed: false,
                        VolumePolicy: volumePolicy));
            }

            if (string.Equals(plan.NativeEventLogOperation, "eventlog-default-transition", StringComparison.OrdinalIgnoreCase))
            {
                return ExecuteEventLogDefaultTransitionWithTimeout(
                    options,
                    plan,
                    eventLogController,
                    current,
                    logName,
                    sourceName);
            }

            return NativeEventLogFailure(
                options,
                plan,
                current,
                "PCV_HOST_EVENTLOG_ACTION_INVALID",
                $"Windows Event Log action '{plan.NativeEventLogOperation}' is not supported.");
        }
        catch (DesktopNodeWindowsEventLogControllerException error)
        {
            return NativeEventLogFailure(options, plan, current, error.Code, error.Message);
        }
    }

    private static DesktopNodeHostServiceActionResult ExecuteEventLogDefaultTransitionWithTimeout(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsEventLogController eventLogController,
        DesktopNodeWindowsEventLogSnapshot current,
        string logName,
        string sourceName)
    {
        var timeoutSeconds = Math.Clamp(options.EventLogDefaultTransitionTimeoutSeconds, 1, 600);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var transitionStarted = new ManualResetEventSlim(false);
        var transitionTask = Task.Factory.StartNew(
            () =>
            {
                transitionStarted.Set();
                return ExecuteEventLogDefaultTransitionCore(
                    options,
                    plan,
                    eventLogController,
                    current,
                    logName,
                    sourceName,
                    timeoutSeconds);
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        _ = transitionTask.ContinueWith(
            task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        if (transitionStarted.Wait(timeout) && transitionTask.Wait(timeout))
        {
            return transitionTask.GetAwaiter().GetResult();
        }

        var hardening = new DesktopNodeHostEventLogHardeningDescriptor(
            Operation: "windows-event-log-default-transition",
            Ok: false,
            ProviderRepairStatus: "provider-repair-timeout",
            EventWriteStatus: "write-timeout",
            VolumeGuardStatus: "not-run",
            EventId: 39101,
            HostMutationPerformed: true,
            FinalProviderStatus: "unknown-after-timeout",
            TimeoutSeconds: timeoutSeconds,
            TimeoutGuardStatus: "timed-out");
        WriteEventLogDefaultTransitionEvidence(options.DataRoot, hardening, current);
        return NativeEventLogFailure(
            options,
            plan,
            current,
            "PCV_HOST_EVENTLOG_DEFAULT_TRANSITION_TIMEOUT",
            $"Windows Event Log default writer transition exceeded the {timeoutSeconds} second timeout.",
            hardening);
    }

    private static DesktopNodeHostServiceActionResult ExecuteEventLogDefaultTransitionCore(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsEventLogController eventLogController,
        DesktopNodeWindowsEventLogSnapshot current,
        string logName,
        string sourceName,
        int timeoutSeconds)
    {
        const int schemaVersion = 1;
        const int eventId = 39101;
        var repaired = eventLogController.Register(logName, sourceName, plan.ServiceExecutablePath);
        var repairOk = repaired.Exists && repaired.Owned;
        var writeStatus = "not-run";
        if (repairOk)
        {
            writeStatus = eventLogController.WriteTestEvent(
                logName,
                sourceName,
                eventId,
                JsonSerializer.Serialize(new
                {
                    schema_version = schemaVersion,
                    operation = "windows-event-log-default-writer",
                    source = sourceName,
                    log_name = logName,
                    writer = "windows-event-log"
                }));
        }

        var volumePolicy = eventLogController.QueryVolumePolicy(logName);
        var removed = repairOk
            ? eventLogController.Remove(logName, sourceName, plan.ServiceExecutablePath)
            : current;
        var removeOk = repairOk && !removed.Exists;
        var final = repairOk
            ? eventLogController.Register(logName, sourceName, plan.ServiceExecutablePath)
            : repaired;
        var finalOk = final.Exists && final.Owned;
        var writeOk = string.Equals(writeStatus, "write-query-pass", StringComparison.OrdinalIgnoreCase);
        var ok = repairOk && writeOk && volumePolicy.VolumeGuarded && removeOk && finalOk;
        var hardening = new DesktopNodeHostEventLogHardeningDescriptor(
            Operation: "windows-event-log-default-transition",
            Ok: ok,
            ProviderRepairStatus: repairOk ? "provider-repair-pass" : "provider-repair-failed",
            EventWriteStatus: writeStatus,
            VolumeGuardStatus: volumePolicy.VolumeGuarded ? "volume-guard-pass" : "volume-guard-failed",
            EventId: eventId,
            HostMutationPerformed: true,
            VolumePolicy: volumePolicy,
            DefaultWriterStatus: writeOk ? "default-writer-pass" : "default-writer-failed",
            ProviderRemoveStatus: removeOk ? "provider-remove-pass" : "provider-remove-failed",
            FinalProviderStatus: finalOk ? "provider-present" : "provider-missing",
            SchemaVersion: schemaVersion,
            TimeoutSeconds: timeoutSeconds,
            TimeoutGuardStatus: "completed-within-timeout");
        WriteEventLogDefaultTransitionEvidence(options.DataRoot, hardening, final);
        return new DesktopNodeHostServiceActionResult(
            Ok: ok,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: null,
            Service: null,
            ServiceOwnerVerified: false,
            ErrorCode: ok ? null : "PCV_HOST_EVENTLOG_DEFAULT_TRANSITION_FAILED",
            ErrorMessage: ok ? null : "Windows Event Log default writer transition failed repair/write/volume/remove/restore verification.",
            EventLog: final,
            EventLogHardening: hardening);
    }

    private static void WriteEventLogDefaultTransitionEvidence(
        string? dataRoot,
        DesktopNodeHostEventLogHardeningDescriptor hardening,
        DesktopNodeWindowsEventLogSnapshot finalProvider)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            return;
        }

        Directory.CreateDirectory(dataRoot);
        var path = Path.Combine(dataRoot, "eventlog-default-transition.json");
        var payload = new
        {
            schema_version = hardening.SchemaVersion ?? 1,
            ok = hardening.Ok,
            operation = hardening.Operation,
            provider_repair_status = hardening.ProviderRepairStatus,
            event_write_status = hardening.EventWriteStatus,
            volume_guard_status = hardening.VolumeGuardStatus,
            default_writer_status = hardening.DefaultWriterStatus,
            provider_remove_status = hardening.ProviderRemoveStatus,
            final_provider_status = hardening.FinalProviderStatus,
            event_id = hardening.EventId,
            host_mutation_performed = hardening.HostMutationPerformed,
            timeout_seconds = hardening.TimeoutSeconds,
            timeout_guard_status = hardening.TimeoutGuardStatus,
            public_trusted_signing = hardening.PublicTrustedSigning,
            external_stable_publication = hardening.ExternalStablePublication,
            event_log = new
            {
                log_name = finalProvider.LogName,
                source_name = finalProvider.SourceName,
                exists = finalProvider.Exists,
                owned = finalProvider.Owned,
                event_message_file = finalProvider.EventMessageFile
            },
            volume_policy = hardening.VolumePolicy is null
                ? null
                : new
                {
                    log_name = hardening.VolumePolicy.LogName,
                    maximum_size_bytes = hardening.VolumePolicy.MaximumSizeBytes,
                    retention_policy = hardening.VolumePolicy.RetentionPolicy,
                    volume_guarded = hardening.VolumePolicy.VolumeGuarded
                }
        };
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            Encoding.UTF8);
    }

    private static DesktopNodeHostServiceActionResult NativeEventLogFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsEventLogSnapshot eventLog,
        string errorCode,
        string errorMessage,
        DesktopNodeHostEventLogHardeningDescriptor? eventLogHardening = null)
    {
        return new DesktopNodeHostServiceActionResult(
            Ok: false,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: null,
            Service: null,
            ServiceOwnerVerified: false,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            EventLog: eventLog,
            EventLogHardening: eventLogHardening);
    }
}

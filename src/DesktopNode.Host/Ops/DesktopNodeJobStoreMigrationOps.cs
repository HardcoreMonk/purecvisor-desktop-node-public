using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Host;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeJobStoreMigrationOps
{
    public const string OperationFamily = "job-store-migration";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController)
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

        return ExecuteNativeJobStoreMigrationAction(options, plan, serviceController);
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeJobStoreMigrationAction(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController)
    {
        var current = serviceController.Query(plan.ServiceName);
        var ownerVerified = DesktopNodeHostServiceAction.IsOwnedService(current, plan.ServiceExecutablePath);
        if (current.Exists && !ownerVerified)
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath: Path.Combine(DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED"), "jobs.json"),
                owned: false,
                currentSchemaVersion: null,
                jobCount: 0,
                queueCount: 0,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot: options.DataRoot!,
                "PCV_JOB_STORE_SERVICE_OWNERSHIP_MISMATCH",
                $"Desktop Node service '{plan.ServiceName}' is not owned by '{plan.ServiceExecutablePath}'.");
        }

        var dataRoot = DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED");
        var jobStorePath = Path.Combine(dataRoot, "jobs.json");
        if (!current.Exists || !DesktopNodeHostServiceAction.IsStopped(current))
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath,
                owned: File.Exists(jobStorePath),
                currentSchemaVersion: null,
                jobCount: 0,
                queueCount: 0,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_JOB_STORE_WRITER_RUNNING",
                $"Desktop Node service '{plan.ServiceName}' must be stopped before job store migration apply.");
        }

        var pendingCommitPath = jobStorePath + ".commit-pending";
        try
        {
            if (DesktopNodeHostServiceAction.OwnedFileExists(pendingCommitPath))
            {
                return NativeJobStoreMigrationFailure(
                    options,
                    plan,
                    current,
                    ownerVerified,
                    jobStorePath,
                    owned: File.Exists(jobStorePath),
                    currentSchemaVersion: null,
                    jobCount: 0,
                    queueCount: 0,
                    serviceStopped: true,
                    dataRoot,
                    "PCV_JOB_STORE_PENDING_COMMIT_UNRESOLVED",
                    "Job store migration is blocked until the current runtime reconciles jobs.json.commit-pending.");
            }
        }
        catch (Exception error) when (DesktopNodeHostServiceAction.IsOwnedFileAccessFailure(error))
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath,
                owned: false,
                currentSchemaVersion: null,
                jobCount: 0,
                queueCount: 0,
                serviceStopped: true,
                dataRoot,
                "PCV_JOB_STORE_PENDING_COMMIT_CHECK_FAILED",
                "Job store migration could not verify that the pending-commit guard is absent.");
        }

        if (!TryReadJobStore(jobStorePath, out var schemaVersion, out var jobCount, out var queueCount))
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath,
                owned: File.Exists(jobStorePath),
                currentSchemaVersion: null,
                jobCount: 0,
                queueCount: 0,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_JOB_STORE_MIGRATION_PRECONDITION_MISSING",
                "Job store migration apply is blocked because ownership, schema, or runtime writer evidence is incomplete.");
        }

        if (schemaVersion != 1)
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath,
                owned: true,
                currentSchemaVersion: schemaVersion,
                jobCount,
                queueCount,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_JOB_STORE_MIGRATION_SOURCE_SCHEMA_UNSUPPORTED",
                $"Job store migration apply only supports source schema version 1; found {schemaVersion}.");
        }

        if (!DesktopNodeHostServiceAction.IsSupportedMigrationPlan(options, "job-store-v1-to-v2", planVersion: 1))
        {
            return NativeJobStoreMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                jobStorePath,
                owned: true,
                currentSchemaVersion: schemaVersion,
                jobCount,
                queueCount,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_JOB_STORE_MIGRATION_PLAN_UNSUPPORTED",
                "Job store migration apply is blocked because a supported migration plan id/version was not provided.");
        }

        return ApplyNativeJobStoreMigration(
            options,
            plan,
            current,
            ownerVerified,
            jobStorePath,
            schemaVersion,
            jobCount,
            queueCount,
            dataRoot);
    }

    private static DesktopNodeHostServiceActionResult ApplyNativeJobStoreMigration(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
        string jobStorePath,
        int sourceSchemaVersion,
        int jobCount,
        int queueCount,
        string dataRoot)
    {
        var targetSchemaVersion = 2;
        var backupRoot = Path.Combine(dataRoot, "backups", "jobs", options.MigrationPlanId!);
        var backupDirectory = Path.Combine(backupRoot, DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff"));
        var backupPath = Path.Combine(backupDirectory, "jobs.json");
        var tempPath = jobStorePath + ".tmp";
        var rollbackAttempted = false;
        var rollbackSucceeded = false;
        var originalJobStoreRestored = false;

        try
        {
            Directory.CreateDirectory(backupDirectory);
            File.Copy(jobStorePath, backupPath, overwrite: false);

            var jobStore = JsonNode.Parse(File.ReadAllText(jobStorePath, Encoding.UTF8))?.AsObject()
                ?? throw new InvalidOperationException("Job store root must be a JSON object.");
            jobStore["version"] = targetSchemaVersion;
            jobStore["migration"] = new JsonObject
            {
                ["plan_id"] = options.MigrationPlanId,
                ["plan_version"] = options.MigrationPlanVersion,
                ["source_schema_version"] = sourceSchemaVersion,
                ["target_schema_version"] = targetSchemaVersion,
                ["applied_at"] = DateTimeOffset.UtcNow.ToString("O")
            };

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            File.WriteAllText(tempPath, jobStore.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, Encoding.UTF8);
            using var validated = JsonDocument.Parse(File.ReadAllText(tempPath, Encoding.UTF8));
            File.Move(tempPath, jobStorePath, overwrite: true);

            return new DesktopNodeHostServiceActionResult(
                Ok: true,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: service,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: null,
                ErrorMessage: null,
                JobStoreMigration: new DesktopNodeHostJobStoreMigrationDescriptor(
                    Operation: "job.store.migration.apply",
                    Ok: true,
                    JobStorePath: jobStorePath,
                    Owned: true,
                    CurrentSchemaVersion: targetSchemaVersion,
                    JobCount: jobCount,
                    QueueCount: queueCount,
                    RuntimeWriter: "DesktopNode.Host",
                    DataRoot: dataRoot,
                    ServiceName: plan.ServiceName,
                    MigrationPlanId: options.MigrationPlanId,
                    MigrationPlanVersion: options.MigrationPlanVersion,
                    MutationPlanned: true,
                    MutationPerformed: true,
                    ServiceStopped: DesktopNodeHostServiceAction.IsStopped(service),
                    BackupRoot: backupRoot,
                    ErrorCode: null,
                    ErrorMessage: null,
                    BackupPath: backupPath,
                    TempPath: tempPath,
                    SourceSchemaVersion: sourceSchemaVersion,
                    TargetSchemaVersion: targetSchemaVersion));
        }
        catch (Exception error) when (
            error is JsonException or
            IOException or
            UnauthorizedAccessException or
            InvalidOperationException or
            System.Security.SecurityException)
        {
            rollbackAttempted = File.Exists(backupPath);
            if (rollbackAttempted)
            {
                try
                {
                    File.Copy(backupPath, jobStorePath, overwrite: true);
                    rollbackSucceeded = true;
                    originalJobStoreRestored = true;
                }
                catch
                {
                    rollbackSucceeded = false;
                    originalJobStoreRestored = false;
                }
            }

            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }

            var partialJobStorePresent = File.Exists(tempPath);
            var descriptor = new DesktopNodeHostJobStoreMigrationDescriptor(
                Operation: "job.store.migration.apply",
                Ok: false,
                JobStorePath: jobStorePath,
                Owned: true,
                CurrentSchemaVersion: sourceSchemaVersion,
                JobCount: jobCount,
                QueueCount: queueCount,
                RuntimeWriter: "DesktopNode.Host",
                DataRoot: dataRoot,
                ServiceName: plan.ServiceName,
                MigrationPlanId: options.MigrationPlanId,
                MigrationPlanVersion: options.MigrationPlanVersion,
                MutationPlanned: true,
                MutationPerformed: false,
                ServiceStopped: DesktopNodeHostServiceAction.IsStopped(service),
                BackupRoot: backupRoot,
                ErrorCode: "PCV_JOB_STORE_MIGRATION_APPLY_FAILED",
                ErrorMessage: error.Message,
                BackupPath: File.Exists(backupPath) ? backupPath : null,
                TempPath: tempPath,
                SourceSchemaVersion: sourceSchemaVersion,
                TargetSchemaVersion: targetSchemaVersion,
                RollbackAttempted: rollbackAttempted,
                RollbackSucceeded: rollbackSucceeded,
                OriginalJobStoreRestored: originalJobStoreRestored,
                PartialJobStorePresent: partialJobStorePresent,
                RecoveryRequired: !originalJobStoreRestored);

            return new DesktopNodeHostServiceActionResult(
                Ok: false,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: service,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: "PCV_JOB_STORE_MIGRATION_APPLY_FAILED",
                ErrorMessage: error.Message,
                JobStoreMigration: descriptor);
        }
    }

    private static DesktopNodeHostServiceActionResult NativeJobStoreMigrationFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
        string jobStorePath,
        bool owned,
        int? currentSchemaVersion,
        int jobCount,
        int queueCount,
        bool serviceStopped,
        string dataRoot,
        string errorCode,
        string errorMessage)
    {
        var descriptor = new DesktopNodeHostJobStoreMigrationDescriptor(
            Operation: "job.store.migration.apply",
            Ok: false,
            JobStorePath: jobStorePath,
            Owned: owned,
            CurrentSchemaVersion: currentSchemaVersion,
            JobCount: jobCount,
            QueueCount: queueCount,
            RuntimeWriter: "DesktopNode.Host",
            DataRoot: dataRoot,
            ServiceName: plan.ServiceName,
            MigrationPlanId: options.MigrationPlanId,
            MigrationPlanVersion: options.MigrationPlanVersion,
            MutationPlanned: false,
            MutationPerformed: false,
            ServiceStopped: serviceStopped,
            BackupRoot: Path.Combine(dataRoot, "backups", "jobs"),
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            SourceSchemaVersion: currentSchemaVersion,
            TargetSchemaVersion: 2);

        return new DesktopNodeHostServiceActionResult(
            Ok: false,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: null,
            Service: service,
            ServiceOwnerVerified: ownerVerified,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            JobStoreMigration: descriptor);
    }

    private static bool TryReadJobStore(string path, out int schemaVersion, out int jobCount, out int queueCount)
    {
        schemaVersion = 0;
        jobCount = 0;
        queueCount = 0;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var root = document.RootElement;
            schemaVersion = root.TryGetProperty("version", out var versionElement) && versionElement.TryGetInt32(out var parsedVersion)
                ? parsedVersion
                : 1;
            if (schemaVersion is not (1 or 2))
            {
                return false;
            }

            jobCount = root.TryGetProperty("jobs", out var jobsElement) && jobsElement.ValueKind == JsonValueKind.Array
                ? jobsElement.GetArrayLength()
                : 0;
            queueCount = root.TryGetProperty("queue", out var queueElement) && queueElement.ValueKind == JsonValueKind.Array
                ? queueElement.GetArrayLength()
                : 0;
            return true;
        }
        catch (Exception error) when (
            error is JsonException or
            IOException or
            UnauthorizedAccessException or
            System.Security.SecurityException)
        {
            return false;
        }
    }
}

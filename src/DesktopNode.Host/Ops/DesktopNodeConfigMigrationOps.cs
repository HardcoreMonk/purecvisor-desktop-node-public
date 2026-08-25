using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Host;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeConfigMigrationOps
{
    public const string OperationFamily = "config-migration";

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

        return ExecuteNativeConfigMigrationAction(options, plan, serviceController);
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeConfigMigrationAction(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController)
    {
        var current = serviceController.Query(plan.ServiceName);
        var ownerVerified = DesktopNodeHostServiceAction.IsOwnedService(current, plan.ServiceExecutablePath);
        if (current.Exists && !ownerVerified)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_CONFIG_MIGRATION_SERVICE_OWNERSHIP_MISMATCH",
                $"Desktop Node service '{plan.ServiceName}' is not owned by '{plan.ServiceExecutablePath}'.");
        }

        var productRoot = DesktopNodeHostServiceAction.Require(options.ProductRoot, "PCV_HOST_PRODUCT_ROOT_REQUIRED");
        var dataRoot = DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED");
        var manifestPath = Path.Combine(productRoot, "product-manifest.json");
        var manifestValid = TryReadProductManifest(manifestPath, out var schemaVersion, out var version);
        if (!manifestValid)
        {
            return NativeConfigMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                manifestPath,
                manifestOwned: false,
                schemaVersion: null,
                version: null,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_CONFIG_MIGRATION_PRECONDITION_MISSING",
                "Product config migration apply is blocked because config ownership or schema evidence is incomplete.");
        }

        if (!current.Exists || !DesktopNodeHostServiceAction.IsStopped(current))
        {
            return NativeConfigMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                manifestPath,
                manifestOwned: true,
                schemaVersion,
                version,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_CONFIG_MIGRATION_SERVICE_RUNNING",
                $"Desktop Node service '{plan.ServiceName}' must be stopped before product config migration apply.");
        }

        if (schemaVersion != 1)
        {
            return NativeConfigMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                manifestPath,
                manifestOwned: true,
                schemaVersion,
                version,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_CONFIG_MIGRATION_SOURCE_SCHEMA_UNSUPPORTED",
                $"Product config migration apply only supports source schema version 1; found {schemaVersion}.");
        }

        if (!DesktopNodeHostServiceAction.IsSupportedMigrationPlan(options, "product-config-v1-to-v2", planVersion: 1))
        {
            return NativeConfigMigrationFailure(
                options,
                plan,
                current,
                ownerVerified,
                manifestPath,
                manifestOwned: true,
                schemaVersion,
                version,
                serviceStopped: DesktopNodeHostServiceAction.IsStopped(current),
                dataRoot,
                "PCV_CONFIG_MIGRATION_PLAN_UNSUPPORTED",
                "Product config migration apply is blocked because a supported migration plan id/version was not provided.");
        }

        return ApplyNativeConfigMigration(
            options,
            plan,
            current,
            ownerVerified,
            manifestPath,
            schemaVersion,
            version,
            dataRoot);
    }

    private static DesktopNodeHostServiceActionResult ApplyNativeConfigMigration(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
        string manifestPath,
        int sourceSchemaVersion,
        string version,
        string dataRoot)
    {
        var targetSchemaVersion = 2;
        var backupRoot = Path.Combine(dataRoot, "backups", "config", options.MigrationPlanId!);
        var backupDirectory = Path.Combine(backupRoot, DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff"));
        var backupPath = Path.Combine(backupDirectory, "product-manifest.json");
        var tempPath = manifestPath + ".tmp";
        var rollbackAttempted = false;
        var rollbackSucceeded = false;
        var originalConfigRestored = false;

        try
        {
            Directory.CreateDirectory(backupDirectory);
            File.Copy(manifestPath, backupPath, overwrite: false);

            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath, Encoding.UTF8))?.AsObject()
                ?? throw new InvalidOperationException("Product manifest root must be a JSON object.");
            manifest["schema_version"] = targetSchemaVersion;
            manifest["migration"] = new JsonObject
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

            File.WriteAllText(tempPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, Encoding.UTF8);
            File.Move(tempPath, manifestPath, overwrite: true);

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
                ConfigMigration: new DesktopNodeHostConfigMigrationDescriptor(
                    Operation: "product.config.migration.apply",
                    Ok: true,
                    ConfigSources:
                    [
                        new DesktopNodeHostConfigMigrationSource(
                            Name: "product-manifest",
                            Path: manifestPath,
                            Owned: true,
                            SchemaVersion: targetSchemaVersion,
                            Version: version)
                    ],
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
                    File.Copy(backupPath, manifestPath, overwrite: true);
                    rollbackSucceeded = true;
                    originalConfigRestored = true;
                }
                catch
                {
                    rollbackSucceeded = false;
                    originalConfigRestored = false;
                }
            }

            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }

            var partialConfigPresent = File.Exists(tempPath);
            var descriptor = new DesktopNodeHostConfigMigrationDescriptor(
                Operation: "product.config.migration.apply",
                Ok: false,
                ConfigSources:
                [
                    new DesktopNodeHostConfigMigrationSource(
                        Name: "product-manifest",
                        Path: manifestPath,
                        Owned: true,
                        SchemaVersion: sourceSchemaVersion,
                        Version: version)
                ],
                DataRoot: dataRoot,
                ServiceName: plan.ServiceName,
                MigrationPlanId: options.MigrationPlanId,
                MigrationPlanVersion: options.MigrationPlanVersion,
                MutationPlanned: true,
                MutationPerformed: false,
                ServiceStopped: DesktopNodeHostServiceAction.IsStopped(service),
                BackupRoot: backupRoot,
                ErrorCode: "PCV_CONFIG_MIGRATION_APPLY_FAILED",
                ErrorMessage: error.Message,
                BackupPath: File.Exists(backupPath) ? backupPath : null,
                TempPath: tempPath,
                SourceSchemaVersion: sourceSchemaVersion,
                TargetSchemaVersion: targetSchemaVersion,
                RollbackAttempted: rollbackAttempted,
                RollbackSucceeded: rollbackSucceeded,
                OriginalConfigRestored: originalConfigRestored,
                PartialConfigPresent: partialConfigPresent);

            return new DesktopNodeHostServiceActionResult(
                Ok: false,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: service,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: "PCV_CONFIG_MIGRATION_APPLY_FAILED",
                ErrorMessage: error.Message,
                ConfigMigration: descriptor);
        }
    }

    private static DesktopNodeHostServiceActionResult NativeConfigMigrationFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
        string manifestPath,
        bool manifestOwned,
        int? schemaVersion,
        string? version,
        bool serviceStopped,
        string dataRoot,
        string errorCode,
        string errorMessage)
    {
        var descriptor = new DesktopNodeHostConfigMigrationDescriptor(
            Operation: "product.config.migration.apply",
            Ok: false,
            ConfigSources:
            [
                new DesktopNodeHostConfigMigrationSource(
                    Name: "product-manifest",
                    Path: manifestPath,
                    Owned: manifestOwned,
                    SchemaVersion: schemaVersion,
                    Version: version)
            ],
            DataRoot: dataRoot,
            ServiceName: plan.ServiceName,
            MigrationPlanId: options.MigrationPlanId,
            MigrationPlanVersion: options.MigrationPlanVersion,
            MutationPlanned: false,
            MutationPerformed: false,
            ServiceStopped: serviceStopped,
            BackupRoot: Path.Combine(dataRoot, "backups", "config"),
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            SourceSchemaVersion: schemaVersion,
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
            ConfigMigration: descriptor);
    }

    private static bool TryReadProductManifest(string path, out int schemaVersion, out string version)
    {
        schemaVersion = 0;
        version = string.Empty;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var root = document.RootElement;
            if (!root.TryGetProperty("schema_version", out var schemaElement) ||
                !schemaElement.TryGetInt32(out schemaVersion) ||
                schemaVersion is not (1 or 2))
            {
                return false;
            }

            if (!root.TryGetProperty("product", out var productElement) ||
                productElement.GetString() != "PureCVisor Desktop Node")
            {
                return false;
            }

            if (!root.TryGetProperty("version", out var versionElement))
            {
                return false;
            }

            version = versionElement.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(version);
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

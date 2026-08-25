using System.Security.AccessControl;
using System.Security.Principal;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeDataRootLifecycleOps
{
    public const string OperationFamily = "data-root";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController)
    {
        return ExecuteNativeDataRootLifecycleAction(options, plan, serviceController);
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeDataRootLifecycleAction(
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

        var current = serviceController.Query(plan.ServiceName);
        var ownerVerified = DesktopNodeHostServiceAction.IsOwnedService(current, plan.ServiceExecutablePath);
        if (string.Equals(plan.NativeDataRootLifecycleOperation, "data-root-remove", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteNativeDataRootRemove(options, plan, current, ownerVerified);
        }

        return DesktopNodeHostServiceAction.NativeServiceFailure(
            options,
            plan,
            current,
            ownerVerified,
            "PCV_HOST_DATA_ROOT_ACTION_INVALID",
            $"Desktop Node data-root action '{plan.NativeDataRootLifecycleOperation}' is not supported.");
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeDataRootRemove(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot current,
        bool ownerVerified)
    {
        if (!options.RemoveData)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_REMOVE_DATA_REQUIRED",
                "Data root removal requires explicit --remove-data opt-in.");
        }

        if (current.Exists)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS",
                $"Desktop Node service '{plan.ServiceName}' must be deleted before data root removal.");
        }

        var removedPaths = new List<string>();
        try
        {
            RemoveDataPaths(plan, removedPaths);
            return new DesktopNodeHostServiceActionResult(
                Ok: true,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: removedPaths,
                PreparedTokenPath: null,
                Service: current,
                ServiceOwnerVerified: false,
                ErrorCode: null,
                ErrorMessage: null);
        }
        catch (Exception error) when (
            error is IOException or
            UnauthorizedAccessException or
            System.Security.SecurityException or
            InvalidOperationException)
        {
            return new DesktopNodeHostServiceActionResult(
                Ok: false,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: removedPaths,
                PreparedTokenPath: null,
                Service: current,
                ServiceOwnerVerified: false,
                ErrorCode: "PCV_HOST_DATA_ROOT_REMOVE_FAILED",
                ErrorMessage: $"Desktop Node data root removal failed: {error.Message}");
        }
    }

    private static void RemoveDataPaths(
        DesktopNodeHostServiceActionPlan plan,
        List<string> removedPaths)
    {
        foreach (var path in plan.RemoveDataPaths)
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(path);
            }
            catch (FileNotFoundException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                PrepareFileForDelete(path);
                File.Delete(path);
                removedPaths.Add(path);
                continue;
            }

            PrepareDirectoryForDelete(path);
            Directory.Delete(path, recursive: true);
            removedPaths.Add(path);
        }

        RemoveOwnedJobStoreTempFiles(plan, removedPaths);
    }

    private static void RemoveOwnedJobStoreTempFiles(
        DesktopNodeHostServiceActionPlan plan,
        List<string> removedPaths)
    {
        var jobStorePath = plan.RemoveDataPaths.FirstOrDefault(path =>
            string.Equals(Path.GetFileName(path), "jobs.json", StringComparison.OrdinalIgnoreCase));
        var dataRoot = jobStorePath is null ? null : Path.GetDirectoryName(jobStorePath);
        if (string.IsNullOrWhiteSpace(dataRoot) || !Directory.Exists(dataRoot))
        {
            return;
        }

        foreach (var prefix in new[] { "jobs.json.tmp.", "jobs.json.commit-pending.tmp." })
        {
            foreach (var path in Directory.EnumerateFiles(dataRoot, prefix + "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(path);
                if (!fileName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var suffix = fileName[prefix.Length..];
                if (!Guid.TryParseExact(suffix, "N", out _))
                {
                    continue;
                }

                PrepareFileForDelete(path);
                File.Delete(path);
                removedPaths.Add(path);
            }
        }
    }

    private static void PrepareDirectoryForDelete(string path)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
        {
            if (File.Exists(entry))
            {
                PrepareFileForDelete(entry);
            }
            else if (Directory.Exists(entry))
            {
                RestoreDirectoryDeleteAcl(entry);
                File.SetAttributes(entry, FileAttributes.Normal);
            }
        }

        RestoreDirectoryDeleteAcl(path);
        File.SetAttributes(path, FileAttributes.Normal);
    }

    private static void PrepareFileForDelete(string path)
    {
        RestoreFileDeleteAcl(path);
        File.SetAttributes(path, FileAttributes.Normal);
    }

    private static void RestoreFileDeleteAcl(string path)
    {
        var fileInfo = new FileInfo(path);
        var security = fileInfo.GetAccessControl();
        AllowDeleteForServiceAdministrators(security);
        fileInfo.SetAccessControl(security);
    }

    private static void RestoreDirectoryDeleteAcl(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        var security = directoryInfo.GetAccessControl();
        AllowDeleteForServiceAdministrators(security);
        directoryInfo.SetAccessControl(security);
    }

    private static void AllowDeleteForServiceAdministrators(FileSystemSecurity security)
    {
        security.SetAccessRuleProtection(isProtected: false, preserveInheritance: true);
        foreach (var sid in new[]
        {
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null)
        })
        {
            security.AddAccessRule(new FileSystemAccessRule(
                sid,
                FileSystemRights.FullControl,
                AccessControlType.Allow));
        }
    }
}

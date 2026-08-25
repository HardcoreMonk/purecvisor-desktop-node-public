namespace DesktopNode.Host.Ops;

internal static class DesktopNodeServiceLifecycleOps
{
    public const string OperationFamily = "service-lifecycle";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
        IDesktopNodeWindowsCredentialManagerController credentialManagerController,
        IDesktopNodeHostFileAclHardener fileAclHardener)
    {
        return ExecuteNativeServiceAction(
            options,
            plan,
            serviceController,
            credentialManagerController,
            fileAclHardener);
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeServiceAction(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
        IDesktopNodeWindowsCredentialManagerController credentialManagerController,
        IDesktopNodeHostFileAclHardener fileAclHardener)
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
        if (string.Equals(plan.NativeServiceOperation, "status", StringComparison.OrdinalIgnoreCase))
        {
            return new DesktopNodeHostServiceActionResult(
                Ok: true,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: current,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: null,
                ErrorMessage: null);
        }

        if (plan.NativeServiceOperation is "configure-installed" or "repair-installed")
        {
            return ExecuteNativeConfigureOrRepair(
                options,
                plan,
                serviceController,
                current,
                ownerVerified,
                fileAclHardener);
        }

        if (string.Equals(plan.NativeServiceOperation, "remove-installed", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteNativeRemove(options, plan, serviceController, current, ownerVerified);
        }

        if (!current.Exists)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_SERVICE_NOT_FOUND",
                $"Desktop Node service '{plan.ServiceName}' was not found.");
        }

        if (!ownerVerified)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_SERVICE_OWNERSHIP_MISMATCH",
                $"Desktop Node service '{plan.ServiceName}' is not owned by '{plan.ServiceExecutablePath}'.");
        }

        try
        {
            var next = plan.NativeServiceOperation switch
            {
                "start" => serviceController.Start(plan.ServiceName, TimeSpan.FromSeconds(30)),
                "stop" => serviceController.Stop(plan.ServiceName, TimeSpan.FromSeconds(30)),
                _ => current
            };
            var operation = plan.NativeServiceOperation ?? string.Empty;
            var expected = operation == "start" ? "running" : "stopped";
            var ok = string.Equals(next.Status, expected, StringComparison.OrdinalIgnoreCase);
            return new DesktopNodeHostServiceActionResult(
                Ok: ok,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: next,
                ServiceOwnerVerified: DesktopNodeHostServiceAction.IsOwnedService(next, plan.ServiceExecutablePath),
                ErrorCode: ok ? null : $"PCV_HOST_SERVICE_{operation.ToUpperInvariant()}_FAILED",
                ErrorMessage: ok ? null : $"Desktop Node service did not reach '{expected}' state.");
        }
        catch (DesktopNodeWindowsServiceControllerException error)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(options, plan, current, ownerVerified, error.Code, error.Message);
        }
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeConfigureOrRepair(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
        DesktopNodeWindowsServiceSnapshot current,
        bool ownerVerified,
        IDesktopNodeHostFileAclHardener fileAclHardener)
    {
        if (current.Exists && !ownerVerified)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_SERVICE_OWNERSHIP_MISMATCH",
                $"Desktop Node service '{plan.ServiceName}' is not owned by '{plan.ServiceExecutablePath}'.");
        }

        var preparedTokenPath = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(options.DataRoot!, fileAclHardener);
        DesktopNodeHostServiceAction.EnsureAccountAuthBootstrapFiles(options.DataRoot!, fileAclHardener);
        try
        {
            var credentialTarget = DesktopNodeHostServiceAction.ExtractNamedArgumentValue(current.BinaryPathName, "--api-token-credential-target");
            var useCredentialManagerToken = !string.IsNullOrWhiteSpace(credentialTarget);
            var batchEvidenceRoot = string.IsNullOrWhiteSpace(options.BatchEvidenceRootPath)
                ? DesktopNodeHostServiceAction.ExtractNamedArgumentValue(current.BinaryPathName, "--batch-evidence-root")
                : options.BatchEvidenceRootPath;
            var configuration = DesktopNodeHostServiceAction.CreateServiceConfiguration(
                plan,
                options,
                useCredentialManagerToken,
                credentialTarget,
                batchEvidenceRoot);
            if (string.Equals(plan.NativeServiceOperation, "repair-installed", StringComparison.OrdinalIgnoreCase) &&
                current.Exists &&
                ownerVerified &&
                string.Equals(current.Status, "running", StringComparison.OrdinalIgnoreCase) &&
                ServiceBinaryPathMatches(current.BinaryPathName, configuration.BinaryPathName))
            {
                return new DesktopNodeHostServiceActionResult(
                    Ok: true,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: preparedTokenPath,
                    Service: current,
                    ServiceOwnerVerified: true,
                    ErrorCode: null,
                    ErrorMessage: null);
            }

            if (string.Equals(plan.NativeServiceOperation, "repair-installed", StringComparison.OrdinalIgnoreCase) &&
                current.Exists &&
                !string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
            {
                current = serviceController.Stop(plan.ServiceName, TimeSpan.FromSeconds(30));
            }

            _ = serviceController.Configure(configuration, TimeSpan.FromSeconds(30));
            var started = serviceController.Start(plan.ServiceName, TimeSpan.FromSeconds(30));
            var ok = string.Equals(started.Status, "running", StringComparison.OrdinalIgnoreCase);
            return new DesktopNodeHostServiceActionResult(
                Ok: ok,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: preparedTokenPath,
                Service: started,
                ServiceOwnerVerified: DesktopNodeHostServiceAction.IsOwnedService(started, plan.ServiceExecutablePath),
                ErrorCode: ok ? null : "PCV_HOST_SERVICE_CONFIGURE_FAILED",
                ErrorMessage: ok ? null : "Desktop Node service did not reach 'running' state after configure.");
        }
        catch (DesktopNodeWindowsServiceControllerException error)
        {
            return new DesktopNodeHostServiceActionResult(
                Ok: false,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: preparedTokenPath,
                Service: current,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: error.Code,
                ErrorMessage: error.Message);
        }
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeRemove(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
        DesktopNodeWindowsServiceSnapshot current,
        bool ownerVerified)
    {
        if (current.Exists && !ownerVerified)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_SERVICE_OWNERSHIP_MISMATCH",
                $"Desktop Node service '{plan.ServiceName}' is not owned by '{plan.ServiceExecutablePath}'.");
        }

        var removedPaths = new List<string>();
        try
        {
            var next = current;
            if (current.Exists)
            {
                if (!string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
                {
                    next = serviceController.Stop(plan.ServiceName, TimeSpan.FromSeconds(30));
                }

                if (!plan.RemoveData)
                {
                    var pendingCommitPath = Path.Combine(
                        DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED"),
                        "jobs.json.commit-pending");
                    try
                    {
                        if (DesktopNodeHostServiceAction.OwnedFileExists(pendingCommitPath))
                        {
                            return DesktopNodeHostServiceAction.NativeServiceFailure(
                                options,
                                plan,
                                next,
                                DesktopNodeHostServiceAction.IsOwnedService(next, plan.ServiceExecutablePath),
                                "PCV_JOB_STORE_PENDING_COMMIT_UNRESOLVED",
                                "Service removal with preserved data is blocked until the current runtime reconciles jobs.json.commit-pending.");
                        }
                    }
                    catch (Exception error) when (DesktopNodeHostServiceAction.IsOwnedFileAccessFailure(error))
                    {
                        return DesktopNodeHostServiceAction.NativeServiceFailure(
                            options,
                            plan,
                            next,
                            DesktopNodeHostServiceAction.IsOwnedService(next, plan.ServiceExecutablePath),
                            "PCV_JOB_STORE_PENDING_COMMIT_CHECK_FAILED",
                            "Service removal could not verify that the pending-commit guard is absent.");
                    }
                }

                next = serviceController.Delete(plan.ServiceName, TimeSpan.FromSeconds(30));
            }
            else if (!plan.RemoveData)
            {
                var pendingCommitPath = Path.Combine(
                    DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED"),
                    "jobs.json.commit-pending");
                try
                {
                    if (DesktopNodeHostServiceAction.OwnedFileExists(pendingCommitPath))
                    {
                        return DesktopNodeHostServiceAction.NativeServiceFailure(
                            options,
                            plan,
                            next,
                            ownerVerified,
                            "PCV_JOB_STORE_PENDING_COMMIT_UNRESOLVED",
                            "Service removal with preserved data is blocked until the current runtime reconciles jobs.json.commit-pending.");
                    }
                }
                catch (Exception error) when (DesktopNodeHostServiceAction.IsOwnedFileAccessFailure(error))
                {
                    return DesktopNodeHostServiceAction.NativeServiceFailure(
                        options,
                        plan,
                        next,
                        ownerVerified,
                        "PCV_JOB_STORE_PENDING_COMMIT_CHECK_FAILED",
                        "Service removal could not verify that the pending-commit guard is absent.");
                }
            }

            var ok = !next.Exists;
            var removeDataHandoff = ok && plan.RemoveData ? CreateRemoveDataHandoff(options, plan) : null;
            return new DesktopNodeHostServiceActionResult(
                Ok: ok,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: removedPaths,
                PreparedTokenPath: null,
                Service: next,
                ServiceOwnerVerified: false,
                ErrorCode: ok ? null : "PCV_HOST_SERVICE_DELETE_FAILED",
                ErrorMessage: ok ? null : "Desktop Node service still exists after delete.",
                RemoveDataHandoff: removeDataHandoff);
        }
        catch (DesktopNodeWindowsServiceControllerException error)
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(options, plan, current, ownerVerified, error.Code, error.Message);
        }
    }

    private static bool ServiceBinaryPathMatches(string? currentBinaryPathName, string expectedBinaryPathName)
    {
        return !string.IsNullOrWhiteSpace(currentBinaryPathName) &&
            string.Equals(
                currentBinaryPathName.Trim(),
                expectedBinaryPathName.Trim(),
                StringComparison.OrdinalIgnoreCase);
    }

    private static DesktopNodeHostRemoveDataHandoff CreateRemoveDataHandoff(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan)
    {
        return new DesktopNodeHostRemoveDataHandoff(
            Operation: "data-root-remove",
            DataRoot: DesktopNodeHostServiceAction.Require(options.DataRoot, "PCV_HOST_DATA_ROOT_REQUIRED"),
            Paths: plan.RemoveDataPaths,
            RequiredAction: "data root remove",
            Reason: "RemoveData was requested after service deletion; data root mutation is delegated to the data-root lifecycle gate.");
    }
}

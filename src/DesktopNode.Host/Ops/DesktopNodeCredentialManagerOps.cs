using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeCredentialManagerOps
{
    public const string OperationFamily = "credential-manager";

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
        return ExecuteNativeCredentialManagerActionForOps(
            options,
            plan,
            serviceController,
            credentialManagerController,
            fileAclHardener);
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeCredentialManagerActionForOps(
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

        if (string.Equals(plan.NativeCredentialManagerOperation, "credential-manager-default-transition", StringComparison.OrdinalIgnoreCase))
        {
            var current = serviceController.Query(plan.ServiceName);
            var ownerVerified = DesktopNodeHostServiceAction.IsOwnedService(current, plan.ServiceExecutablePath);
            return ExecuteNativeCredentialManagerDefaultTransition(
                options,
                plan,
                serviceController,
                credentialManagerController,
                current,
                ownerVerified,
                fileAclHardener);
        }

        var credentialTarget = plan.CredentialTarget ?? "PureCVisor/PureCVisorDesktopNode/api-token";
        try
        {
            var proof = credentialManagerController.WriteReadDeleteProof(credentialTarget);
            var isSystem = string.Equals(proof.Identity, @"NT AUTHORITY\SYSTEM", StringComparison.OrdinalIgnoreCase);
            var lifecyclePassed =
                string.Equals(proof.CredentialWriteStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(proof.CredentialReadStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(proof.CredentialDeleteStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                !proof.TokenValueObserved;
            var ok = isSystem && lifecyclePassed;

            return CredentialManagerResult(
                options,
                plan,
                ok,
                ok ? null : "PCV_HOST_CREDENTIAL_MANAGER_SYSTEM_PROOF_FAILED",
                ok ? null : "Credential Manager proof must pass write/read/delete under NT AUTHORITY\\SYSTEM without observing token value.",
                proof);
        }
        catch (DesktopNodeWindowsCredentialManagerControllerException error)
        {
            return CredentialManagerResult(
                options,
                plan,
                ok: false,
                error.Code,
                error.Message,
                new DesktopNodeWindowsCredentialManagerProofSnapshot(
                    Identity: System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    CredentialTarget: credentialTarget,
                    CredentialWriteStatus: "failed",
                    CredentialReadStatus: "not-run",
                    CredentialDeleteStatus: "not-run",
                    TokenValueObserved: false,
                    NewTokenValueCreated: true));
        }
    }

    private static DesktopNodeHostServiceActionResult CredentialManagerResult(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        bool ok,
        string? errorCode,
        string? errorMessage,
        DesktopNodeWindowsCredentialManagerProofSnapshot proof)
    {
        return new DesktopNodeHostServiceActionResult(
            Ok: ok,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: null,
            Service: null,
            ServiceOwnerVerified: false,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            CredentialManagerProof: new DesktopNodeHostCredentialManagerProofDescriptor(
                Operation: "windows-credential-manager-service-default-transition",
                Ok: ok,
                ProofStatus: ok ? "system-context-proof-pass" : "system-context-proof-failed",
                Identity: proof.Identity,
                CredentialTarget: proof.CredentialTarget,
                CredentialWriteStatus: proof.CredentialWriteStatus,
                CredentialReadStatus: proof.CredentialReadStatus,
                CredentialDeleteStatus: proof.CredentialDeleteStatus,
                TokenValueObserved: proof.TokenValueObserved,
                NewTokenValueCreated: proof.NewTokenValueCreated,
                HostMutationPerformed: true,
                PublicTrustedSigning: "not-claimed",
                ExternalStablePublication: "not-claimed"));
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeCredentialManagerDefaultTransition(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
        IDesktopNodeWindowsCredentialManagerController credentialManagerController,
        DesktopNodeWindowsServiceSnapshot current,
        bool ownerVerified,
        IDesktopNodeHostFileAclHardener fileAclHardener)
    {
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

        var dataRoot = Path.GetFullPath(options.DataRoot!);
        Directory.CreateDirectory(dataRoot);
        var protectedTokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        var protectedTokenExisted = File.Exists(protectedTokenPath);
        var credentialTarget = plan.CredentialTarget ?? "PureCVisor/PureCVisorDesktopNode/api-token";
        var rollbackDiagnosticsPath = Path.Combine(dataRoot, "credential-manager-transition.rollback.json");
        var previousBinaryPath = current.BinaryPathName;
        if (!DesktopNodeHostServiceAction.UsesProtectedFileTokenSource(current.BinaryPathName))
        {
            if (protectedTokenExisted && UsesCredentialManagerTokenSource(current.BinaryPathName, credentialTarget))
            {
                try
                {
                    var protectedToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
                    {
                        ApiTokenProtectedFile = protectedTokenPath
                    });
                    var credentialToken = credentialManagerController.ReadToken(credentialTarget);
                    if (FixedTimeEquals(protectedToken.Value!, credentialToken))
                    {
                        var idempotentRollbackDiagnosticsStatus = WriteCredentialManagerTransitionRollbackDiagnostics(
                            rollbackDiagnosticsPath,
                            dataRoot,
                            protectedTokenPath,
                            credentialTarget,
                            previousBinaryPath,
                            previousBinaryPath,
                            rollbackAttempted: false,
                            rollbackSucceeded: false,
                            errorCode: null,
                            errorMessage: null);
                        var descriptor = CreateCredentialManagerTransitionDescriptor(
                            ok: string.Equals(idempotentRollbackDiagnosticsStatus, "written", StringComparison.OrdinalIgnoreCase),
                            identity: WindowsIdentity.GetCurrent().Name,
                            credentialTarget: credentialTarget,
                            dataRoot: dataRoot,
                            protectedTokenPath: protectedTokenPath,
                            rollbackDiagnosticsPath: rollbackDiagnosticsPath,
                            serviceName: plan.ServiceName,
                            systemProofStatus: "not-required-already-credential-manager",
                            credentialWriteStatus: "not-run",
                            credentialReadStatus: "pass",
                            credentialDeleteStatus: "not-run",
                            tokenSourceMigration: "already-credential-manager",
                            serviceReloadStatus: "not-required",
                            oldSourceRejectionStatus: "already-credential-manager",
                            rollbackDiagnosticsStatus: idempotentRollbackDiagnosticsStatus,
                            newTokenValueCreated: false,
                            hostMutationPerformed: false,
                            previousBinaryPath: previousBinaryPath,
                            nextBinaryPath: previousBinaryPath);

                        return CredentialManagerTransitionResult(
                            options,
                            plan,
                            current,
                            ownerVerified,
                            descriptor,
                            descriptor.Ok ? null : "PCV_HOST_CREDENTIAL_MANAGER_IDEMPOTENT_DIAGNOSTICS_FAILED",
                            descriptor.Ok ? null : "Credential Manager default transition was already applied, but rollback diagnostics could not be written.");
                    }
                }
                catch (Exception error) when (
                    error is IOException or
                    UnauthorizedAccessException or
                    InvalidOperationException or
                    System.Security.SecurityException or
                    CryptographicException or
                    JsonException or
                    KeyNotFoundException or
                    DesktopNodeWindowsCredentialManagerControllerException)
                {
                    return DesktopNodeHostServiceAction.NativeServiceFailure(
                        options,
                        plan,
                        current,
                        ownerVerified,
                        "PCV_HOST_CREDENTIAL_MANAGER_TOKEN_SOURCE_MISMATCH",
                        $"Credential Manager default transition found an existing credential-manager token source, but the token could not be verified: {error.Message}");
                }
            }

            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_CREDENTIAL_MANAGER_TOKEN_SOURCE_MISMATCH",
                "Credential Manager default transition requires the installed service to use the protected-file token source before migration.");
        }

        var nextBinaryPath = default(string);
        var identity = WindowsIdentity.GetCurrent().Name;
        var proofStatus = "not-run";
        var credentialWriteStatus = "not-run";
        var credentialReadStatus = "not-run";
        var credentialDeleteStatus = "not-run";
        var tokenSourceMigration = "not-run";
        var serviceReloadStatus = "not-run";
        var oldSourceRejectionStatus = "not-run";
        var rollbackDiagnosticsStatus = "not-run";
        var rollbackAttempted = false;
        var rollbackSucceeded = false;
        var hostMutationPerformed = false;
        var next = current;

        try
        {
            var proof = credentialManagerController.WriteReadDeleteProof($"{credentialTarget}/system-proof-{Guid.NewGuid():N}");
            identity = proof.Identity;
            credentialWriteStatus = proof.CredentialWriteStatus;
            credentialReadStatus = proof.CredentialReadStatus;
            credentialDeleteStatus = proof.CredentialDeleteStatus;
            var systemProofOk =
                string.Equals(proof.Identity, @"NT AUTHORITY\SYSTEM", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(proof.CredentialWriteStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(proof.CredentialReadStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(proof.CredentialDeleteStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
                !proof.TokenValueObserved;
            proofStatus = systemProofOk ? "system-context-proof-pass" : "system-context-proof-failed";
            if (!systemProofOk)
            {
                var descriptor = CreateCredentialManagerTransitionDescriptor(
                    ok: false,
                    identity,
                    credentialTarget,
                    dataRoot,
                    protectedTokenPath,
                    rollbackDiagnosticsPath,
                    plan.ServiceName,
                    proofStatus,
                    credentialWriteStatus,
                    credentialReadStatus,
                    credentialDeleteStatus,
                    tokenSourceMigration,
                    serviceReloadStatus,
                    oldSourceRejectionStatus,
                    rollbackDiagnosticsStatus,
                    newTokenValueCreated: false,
                    hostMutationPerformed: false,
                    previousBinaryPath,
                    nextBinaryPath);

                return CredentialManagerTransitionResult(
                    options,
                    plan,
                    current,
                    ownerVerified,
                    descriptor,
                    "PCV_HOST_CREDENTIAL_MANAGER_SYSTEM_PROOF_FAILED",
                    "Credential Manager default transition must run under NT AUTHORITY\\SYSTEM before the service token source is migrated.");
            }

            _ = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, fileAclHardener);
            var protectedToken = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = protectedTokenPath
            });

            credentialManagerController.WriteToken(credentialTarget, protectedToken.Value!);
            credentialWriteStatus = "pass";
            var credentialToken = credentialManagerController.ReadToken(credentialTarget);
            if (!FixedTimeEquals(protectedToken.Value!, credentialToken))
            {
                throw new DesktopNodeWindowsCredentialManagerControllerException(
                    "PCV_HOST_CREDENTIAL_MANAGER_MIGRATION_READ_MISMATCH",
                    $"Windows Credential Manager target '{credentialTarget}' did not return the migrated protected token.");
            }

            credentialReadStatus = "pass";
            tokenSourceMigration = "protected-file-to-credential-manager";

            if (!string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
            {
                _ = serviceController.Stop(plan.ServiceName, TimeSpan.FromSeconds(30));
            }

            var batchEvidenceRoot = string.IsNullOrWhiteSpace(options.BatchEvidenceRootPath)
                ? DesktopNodeHostServiceAction.ExtractNamedArgumentValue(current.BinaryPathName, "--batch-evidence-root")
                : options.BatchEvidenceRootPath;
            var configuration = DesktopNodeHostServiceAction.CreateServiceConfiguration(
                plan,
                options,
                useCredentialManagerToken: true,
                batchEvidenceRootOverride: batchEvidenceRoot);
            nextBinaryPath = configuration.BinaryPathName;
            _ = serviceController.Configure(configuration, TimeSpan.FromSeconds(30));
            hostMutationPerformed = true;
            next = serviceController.Start(plan.ServiceName, TimeSpan.FromSeconds(30));
            serviceReloadStatus = string.Equals(next.Status, "running", StringComparison.OrdinalIgnoreCase)
                ? "restarted"
                : "restart-failed";
            oldSourceRejectionStatus =
                !nextBinaryPath.Contains("--api-token-protected-file", StringComparison.OrdinalIgnoreCase) &&
                nextBinaryPath.Contains("--api-token-credential-target", StringComparison.OrdinalIgnoreCase)
                    ? "protected-file-source-rejected-after-reload"
                    : "not-rejected";
            rollbackDiagnosticsStatus = WriteCredentialManagerTransitionRollbackDiagnostics(
                rollbackDiagnosticsPath,
                dataRoot,
                protectedTokenPath,
                credentialTarget,
                previousBinaryPath,
                nextBinaryPath,
                rollbackAttempted,
                rollbackSucceeded,
                errorCode: null,
                errorMessage: null);

            var ok =
                string.Equals(serviceReloadStatus, "restarted", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(oldSourceRejectionStatus, "protected-file-source-rejected-after-reload", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(rollbackDiagnosticsStatus, "written", StringComparison.OrdinalIgnoreCase);
            var successDescriptor = CreateCredentialManagerTransitionDescriptor(
                ok,
                identity,
                credentialTarget,
                dataRoot,
                protectedTokenPath,
                rollbackDiagnosticsPath,
                plan.ServiceName,
                proofStatus,
                credentialWriteStatus,
                credentialReadStatus,
                credentialDeleteStatus,
                tokenSourceMigration,
                serviceReloadStatus,
                oldSourceRejectionStatus,
                rollbackDiagnosticsStatus,
                newTokenValueCreated: !protectedTokenExisted,
                hostMutationPerformed,
                previousBinaryPath,
                nextBinaryPath);

            return CredentialManagerTransitionResult(
                options,
                plan,
                next,
                DesktopNodeHostServiceAction.IsOwnedService(next, plan.ServiceExecutablePath),
                successDescriptor,
                ok ? null : "PCV_HOST_CREDENTIAL_MANAGER_TRANSITION_RELOAD_NOT_VERIFIED",
                ok ? null : "Credential Manager transition completed but reload, old source rejection, or rollback diagnostics could not be verified.");
        }
        catch (Exception error) when (
            error is IOException or
            UnauthorizedAccessException or
            InvalidOperationException or
            System.Security.SecurityException or
            CryptographicException or
            JsonException or
            DesktopNodeWindowsCredentialManagerControllerException or
            DesktopNodeWindowsServiceControllerException)
        {
            if (hostMutationPerformed && !string.IsNullOrWhiteSpace(previousBinaryPath))
            {
                rollbackAttempted = true;
                try
                {
                    var rollbackConfiguration = CreateServiceConfigurationFromBinaryPath(plan, previousBinaryPath);
                    _ = serviceController.Configure(rollbackConfiguration, TimeSpan.FromSeconds(30));
                    if (!string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
                    {
                        next = serviceController.Start(plan.ServiceName, TimeSpan.FromSeconds(30));
                    }

                    rollbackSucceeded = string.Equals(next.Status, "running", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase);
                }
                catch (DesktopNodeWindowsServiceControllerException)
                {
                    rollbackSucceeded = false;
                }
            }

            rollbackDiagnosticsStatus = WriteCredentialManagerTransitionRollbackDiagnostics(
                rollbackDiagnosticsPath,
                dataRoot,
                protectedTokenPath,
                credentialTarget,
                previousBinaryPath,
                nextBinaryPath,
                rollbackAttempted,
                rollbackSucceeded,
                error is DesktopNodeWindowsCredentialManagerControllerException credentialError
                    ? credentialError.Code
                    : error is DesktopNodeWindowsServiceControllerException serviceError
                        ? serviceError.Code
                        : "PCV_HOST_CREDENTIAL_MANAGER_TRANSITION_FAILED",
                error.Message);
            var descriptor = CreateCredentialManagerTransitionDescriptor(
                ok: false,
                identity,
                credentialTarget,
                dataRoot,
                protectedTokenPath,
                rollbackDiagnosticsPath,
                plan.ServiceName,
                proofStatus,
                credentialWriteStatus,
                credentialReadStatus,
                credentialDeleteStatus,
                tokenSourceMigration,
                serviceReloadStatus,
                oldSourceRejectionStatus,
                rollbackDiagnosticsStatus,
                newTokenValueCreated: !protectedTokenExisted,
                hostMutationPerformed,
                previousBinaryPath,
                nextBinaryPath);

            return CredentialManagerTransitionResult(
                options,
                plan,
                next,
                ownerVerified,
                descriptor,
                error is DesktopNodeWindowsCredentialManagerControllerException credentialError2
                    ? credentialError2.Code
                    : error is DesktopNodeWindowsServiceControllerException serviceError2
                        ? serviceError2.Code
                        : "PCV_HOST_CREDENTIAL_MANAGER_TRANSITION_FAILED",
                $"Desktop Node Credential Manager transition failed: {error.Message}");
        }
    }

    private static DesktopNodeWindowsServiceConfiguration CreateServiceConfigurationFromBinaryPath(
        DesktopNodeHostServiceActionPlan plan,
        string binaryPathName)
    {
        return new DesktopNodeWindowsServiceConfiguration(
            ServiceName: plan.ServiceName,
            DisplayName: "PureCVisor Desktop Node",
            Description: "PureCVisor Desktop Node Local API service.",
            BinaryPathName: binaryPathName,
            ServiceAccount: "LocalSystem",
            AutoStart: true,
            FailureResetPeriodSeconds: 86400,
            FailureActions: [
                new DesktopNodeWindowsServiceFailureAction("restart", TimeSpan.FromSeconds(60)),
                new DesktopNodeWindowsServiceFailureAction("restart", TimeSpan.FromSeconds(60)),
                new DesktopNodeWindowsServiceFailureAction("none", TimeSpan.FromSeconds(60))
            ]);
    }

    private static DesktopNodeHostCredentialManagerTransitionDescriptor CreateCredentialManagerTransitionDescriptor(
        bool ok,
        string identity,
        string credentialTarget,
        string dataRoot,
        string protectedTokenPath,
        string rollbackDiagnosticsPath,
        string serviceName,
        string systemProofStatus,
        string credentialWriteStatus,
        string credentialReadStatus,
        string credentialDeleteStatus,
        string tokenSourceMigration,
        string serviceReloadStatus,
        string oldSourceRejectionStatus,
        string rollbackDiagnosticsStatus,
        bool newTokenValueCreated,
        bool hostMutationPerformed,
        string? previousBinaryPath,
        string? nextBinaryPath)
    {
        return new DesktopNodeHostCredentialManagerTransitionDescriptor(
            Operation: "windows-credential-manager-default-transition",
            Ok: ok,
            Identity: identity,
            CredentialTarget: credentialTarget,
            DataRoot: dataRoot,
            ProtectedTokenPath: protectedTokenPath,
            TransitionEvidencePath: Path.Combine(dataRoot, "credential-manager-transition.json"),
            RollbackDiagnosticsPath: rollbackDiagnosticsPath,
            ServiceName: serviceName,
            SystemProofStatus: systemProofStatus,
            CredentialWriteStatus: credentialWriteStatus,
            CredentialReadStatus: credentialReadStatus,
            CredentialDeleteStatus: credentialDeleteStatus,
            TokenSourceMigration: tokenSourceMigration,
            ServiceReloadStatus: serviceReloadStatus,
            OldSourceRejectionStatus: oldSourceRejectionStatus,
            RollbackDiagnosticsStatus: rollbackDiagnosticsStatus,
            TokenValueObserved: false,
            NewTokenValueCreated: newTokenValueCreated,
            HostMutationPerformed: hostMutationPerformed,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed",
            PreviousBinaryPath: previousBinaryPath,
            NextBinaryPath: nextBinaryPath);
    }

    private static DesktopNodeHostServiceActionResult CredentialManagerTransitionResult(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
        DesktopNodeHostCredentialManagerTransitionDescriptor descriptor,
        string? errorCode,
        string? errorMessage)
    {
        WriteCredentialManagerTransitionEvidence(descriptor.TransitionEvidencePath, descriptor);
        return new DesktopNodeHostServiceActionResult(
            Ok: descriptor.Ok,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: descriptor.ProtectedTokenPath,
            Service: service,
            ServiceOwnerVerified: ownerVerified,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            CredentialManagerTransition: descriptor);
    }

    private static void WriteCredentialManagerTransitionEvidence(
        string path,
        DesktopNodeHostCredentialManagerTransitionDescriptor descriptor)
    {
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(descriptor, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }),
            Encoding.UTF8);
    }

    private static string WriteCredentialManagerTransitionRollbackDiagnostics(
        string path,
        string dataRoot,
        string protectedTokenPath,
        string credentialTarget,
        string? previousBinaryPath,
        string? nextBinaryPath,
        bool rollbackAttempted,
        bool rollbackSucceeded,
        string? errorCode,
        string? errorMessage)
    {
        var record = new SortedDictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["operation"] = "windows-credential-manager-default-transition",
            ["data_root"] = dataRoot,
            ["protected_token_path"] = protectedTokenPath,
            ["credential_target"] = credentialTarget,
            ["previous_binary_path"] = previousBinaryPath,
            ["next_binary_path"] = nextBinaryPath,
            ["rollback_attempted"] = rollbackAttempted,
            ["rollback_succeeded"] = rollbackSucceeded,
            ["fallback_token_source"] = "dpapi-local-machine-protected-file",
            ["token_value_observed"] = false,
            ["public_trusted_signing"] = "not-claimed",
            ["external_stable_publication"] = "not-claimed",
            ["error_code"] = errorCode,
            ["error_message"] = errorMessage
        };
        File.WriteAllText(path, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
        return "written";
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        try
        {
            return leftBytes.Length == rightBytes.Length &&
                CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
        finally
        {
            Array.Clear(leftBytes);
            Array.Clear(rightBytes);
        }
    }

    private static bool UsesCredentialManagerTokenSource(string? binaryPathName, string credentialTarget)
    {
        if (string.IsNullOrWhiteSpace(binaryPathName) ||
            !binaryPathName.Contains("--api-token-credential-target", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var currentCredentialTarget = DesktopNodeHostServiceAction.ExtractNamedArgumentValue(binaryPathName, "--api-token-credential-target");
        return string.Equals(currentCredentialTarget, credentialTarget, StringComparison.Ordinal);
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeServiceTokenOps
{
    public const string OperationFamily = "service-token";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
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
        if (string.Equals(plan.NativeServiceTokenOperation, "service-token-rotation-revoke", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteNativeServiceTokenRotationRevoke(
                options,
                plan,
                serviceController,
                current,
                ownerVerified,
                fileAclHardener);
        }

        return DesktopNodeHostServiceAction.NativeServiceFailure(
            options,
            plan,
            current,
            ownerVerified,
            "PCV_HOST_SERVICE_TOKEN_ACTION_INVALID",
            $"Desktop Node service-token action '{plan.NativeServiceTokenOperation}' is not supported.");
    }

    private static DesktopNodeHostServiceActionResult ExecuteNativeServiceTokenRotationRevoke(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsServiceController serviceController,
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

        if (!DesktopNodeHostServiceAction.UsesProtectedFileTokenSource(current.BinaryPathName))
        {
            return DesktopNodeHostServiceAction.NativeServiceFailure(
                options,
                plan,
                current,
                ownerVerified,
                "PCV_HOST_SERVICE_TOKEN_SOURCE_MISMATCH",
                "Service token rotation requires the installed service to use the protected-file token source before mutation.");
        }

        var dataRoot = Path.GetFullPath(options.DataRoot!);
        Directory.CreateDirectory(dataRoot);
        var tokenPath = Path.Combine(dataRoot, "api-token.dpapi.json");
        var backupRoot = Path.Combine(dataRoot, "backups", "service-token-rotation");
        Directory.CreateDirectory(backupRoot);
        var auditPath = Path.Combine(dataRoot, "service-token-rotation.audit.jsonl");
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ");
        var backupPath = File.Exists(tokenPath)
            ? Path.Combine(backupRoot, $"api-token-{timestamp}.dpapi.json")
            : null;
        var tempPath = Path.Combine(dataRoot, $"api-token.dpapi.{Guid.NewGuid():N}.tmp");
        var oldHash = DesktopNodeHostServiceAction.ReadProtectedTokenSha256(tokenPath);
        var newToken = DesktopNodeHostServiceAction.CreateToken();
        var newHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(newToken))).ToLowerInvariant();
        var backupWriteStatus = "not-needed";
        var atomicReplaceStatus = "not-run";
        var next = current;
        var serviceReloadStatus = "not-run";
        var auditStatus = "not-run";

        try
        {
            // Write the replacement token to a unique temp path first, then atomically
            // promote it. Prefer File.Replace with an explicit destination backup path over
            // File.Copy+File.Replace(null): the two-step form opens the live token for the
            // backup copy and then asks Replace to delete that same path, which has been
            // observed to fail intermittently under full Host.Tests load with
            // IOException "Unable to remove the file to be replaced" /
            // "바꿀 파일을 제거할 수 없습니다" (backup_write_status=written,
            // atomic_replace_status=not-run). A short IO retry covers residual AV/scanner
            // exclusive locks without switching to File.Move, which is strictly weaker under
            // realistic ReadWrite|Delete share modes (see followup-work-record §12.3).
            DesktopNodeHostServiceAction.WriteProtectedTokenFile(tempPath, newToken, DateTimeOffset.UtcNow, fileAclHardener: null);
            if (File.Exists(tokenPath))
            {
                ReplaceProtectedTokenFileWithRetry(tempPath, tokenPath, backupPath);
                backupWriteStatus = backupPath is null ? "not-needed" : "written";
            }
            else
            {
                File.Move(tempPath, tokenPath);
                backupWriteStatus = "not-needed";
            }

            fileAclHardener.Harden(tokenPath);
            atomicReplaceStatus = "completed";

            if (!string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
            {
                _ = serviceController.Stop(plan.ServiceName, TimeSpan.FromSeconds(30));
            }

            next = serviceController.Start(plan.ServiceName, TimeSpan.FromSeconds(30));
            serviceReloadStatus = string.Equals(next.Status, "running", StringComparison.OrdinalIgnoreCase)
                ? "restarted"
                : "restart-failed";
            var oldTokenRejected = !string.IsNullOrWhiteSpace(oldHash) &&
                !string.Equals(oldHash, newHash, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(serviceReloadStatus, "restarted", StringComparison.OrdinalIgnoreCase);
            var descriptor = new DesktopNodeHostServiceTokenRotationDescriptor(
                Operation: "service-token-rotation-revoke",
                Ok: oldTokenRejected,
                DataRoot: dataRoot,
                TokenPath: tokenPath,
                BackupRoot: backupRoot,
                BackupPath: backupPath,
                AuditPath: auditPath,
                ServiceName: plan.ServiceName,
                ServiceTokenMutation: "performed",
                TokenValueObserved: false,
                NewTokenValueCreated: true,
                ServiceReloadStatus: serviceReloadStatus,
                OldTokenRejectionStatus: oldTokenRejected ? "old-token-rejected-after-reload" : "not-verified",
                TokenRotationAuditStatus: "written",
                HostMutationPerformed: true,
                PublicTrustedSigning: "not-claimed",
                ExternalStablePublication: "not-claimed",
                OldTokenSha256: oldHash,
                NewTokenSha256: newHash,
                BackupWriteStatus: backupWriteStatus,
                AtomicReplaceStatus: atomicReplaceStatus);
            WriteServiceTokenRotationAudit(auditPath, descriptor);
            auditStatus = "written";

            return new DesktopNodeHostServiceActionResult(
                Ok: descriptor.Ok,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: tokenPath,
                Service: next,
                ServiceOwnerVerified: DesktopNodeHostServiceAction.IsOwnedService(next, plan.ServiceExecutablePath),
                ErrorCode: descriptor.Ok ? null : "PCV_HOST_SERVICE_TOKEN_ROTATION_RELOAD_NOT_VERIFIED",
                ErrorMessage: descriptor.Ok ? null : "Service token rotation completed but service reload or old-token rejection could not be verified.",
                ServiceTokenRotation: descriptor with { TokenRotationAuditStatus = auditStatus });
        }
        catch (Exception error) when (
            error is IOException or
            UnauthorizedAccessException or
            System.Security.Cryptography.CryptographicException or
            DesktopNodeWindowsServiceControllerException or
            InvalidOperationException)
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            var descriptor = new DesktopNodeHostServiceTokenRotationDescriptor(
                Operation: "service-token-rotation-revoke",
                Ok: false,
                DataRoot: dataRoot,
                TokenPath: tokenPath,
                BackupRoot: backupRoot,
                BackupPath: backupPath,
                AuditPath: auditPath,
                ServiceName: plan.ServiceName,
                ServiceTokenMutation: atomicReplaceStatus == "completed" ? "performed" : "failed",
                TokenValueObserved: false,
                NewTokenValueCreated: true,
                ServiceReloadStatus: serviceReloadStatus,
                OldTokenRejectionStatus: "not-verified",
                TokenRotationAuditStatus: auditStatus,
                HostMutationPerformed: atomicReplaceStatus == "completed",
                PublicTrustedSigning: "not-claimed",
                ExternalStablePublication: "not-claimed",
                OldTokenSha256: oldHash,
                NewTokenSha256: newHash,
                BackupWriteStatus: backupWriteStatus,
                AtomicReplaceStatus: atomicReplaceStatus);

            return new DesktopNodeHostServiceActionResult(
                Ok: false,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: File.Exists(tokenPath) ? tokenPath : null,
                Service: next,
                ServiceOwnerVerified: ownerVerified,
                ErrorCode: error is DesktopNodeWindowsServiceControllerException controllerError
                    ? controllerError.Code
                    : "PCV_HOST_SERVICE_TOKEN_ROTATION_FAILED",
                ErrorMessage: $"Desktop Node service token rotation failed: {error.Message}",
                ServiceTokenRotation: descriptor);
        }
    }

    private static void ReplaceProtectedTokenFileWithRetry(
        string tempPath,
        string tokenPath,
        string? backupPath)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                File.Replace(tempPath, tokenPath, backupPath, ignoreMetadataErrors: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts && File.Exists(tempPath))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(25 * attempt));
            }
        }
    }

    private static void WriteServiceTokenRotationAudit(string auditPath, DesktopNodeHostServiceTokenRotationDescriptor descriptor)
    {
        var audit = new SortedDictionary<string, object?>
        {
            ["operation"] = descriptor.Operation,
            ["ok"] = descriptor.Ok,
            ["created_at"] = DateTimeOffset.UtcNow.ToString("o"),
            ["service_name"] = descriptor.ServiceName,
            ["service_token_mutation"] = descriptor.ServiceTokenMutation,
            ["token_value_observed"] = descriptor.TokenValueObserved,
            ["new_token_value_created"] = descriptor.NewTokenValueCreated,
            ["service_reload_status"] = descriptor.ServiceReloadStatus,
            ["old_token_rejection_status"] = descriptor.OldTokenRejectionStatus,
            ["token_rotation_audit_status"] = descriptor.TokenRotationAuditStatus,
            ["old_token_sha256"] = descriptor.OldTokenSha256,
            ["new_token_sha256"] = descriptor.NewTokenSha256,
            ["backup_path"] = descriptor.BackupPath,
            ["atomic_replace_status"] = descriptor.AtomicReplaceStatus,
            ["public_trusted_signing"] = descriptor.PublicTrustedSigning,
            ["external_stable_publication"] = descriptor.ExternalStablePublication
        };
        File.AppendAllText(auditPath, JsonSerializer.Serialize(audit) + Environment.NewLine, Encoding.UTF8);
    }
}

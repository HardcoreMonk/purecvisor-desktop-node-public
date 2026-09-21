namespace DesktopNode.Contracts;

public static class VmExportImportProblemCodes
{
    public const string Forbidden = "PCV_VM_EXPORT_FORBIDDEN";
    public const string VmRequired = "PCV_VM_EXPORT_VM_REQUIRED";
    public const string ConfirmationMismatch = "PCV_VM_EXPORT_CONFIRMATION_MISMATCH";
    public const string NotManaged = "PCV_VM_NOT_MANAGED_BY_PURECVISOR";
    public const string GenerationUnsupported = "PCV_VM_GENERATION_UNSUPPORTED";
    public const string SourceNotOff = "PCV_VM_EXPORT_SOURCE_NOT_OFF";
    public const string SecurityFeaturesUnsupported = "PCV_VM_EXPORT_SECURITY_FEATURES_UNSUPPORTED";
    public const string PathRequired = "PCV_VM_EXPORT_PATH_REQUIRED";
    public const string PathNotAllowed = "PCV_VM_EXPORT_PATH_NOT_ALLOWED";
    public const string ImportNameRequired = "PCV_VM_IMPORT_NAME_REQUIRED";
    public const string ImportConfirmationMismatch = "PCV_VM_IMPORT_CONFIRMATION_MISMATCH";
    public const string OvfForbidden = "PCV_VM_IMPORT_OVF_FORBIDDEN";
    public const string PackageInvalid = "PCV_VM_IMPORT_PACKAGE_INVALID";
    public const string ImportSecurityFeaturesUnsupported = "PCV_VM_IMPORT_SECURITY_FEATURES_UNSUPPORTED";
    public const string InPlaceForbidden = "PCV_VM_IMPORT_INPLACE_FORBIDDEN";
    public const string AlreadyExists = "PCV_VM_ALREADY_EXISTS";
    public const string NameInvalid = "PCV_VM_NAME_INVALID";
    public const string ImportPathRequired = "PCV_VM_IMPORT_PATH_REQUIRED";
    public const string ImportPathNotAllowed = "PCV_VM_IMPORT_PATH_NOT_ALLOWED";
}

public sealed record VmExportImportAuthContext(
    bool HasOperate = false,
    bool HasServiceBearer = false);

public sealed record VmExportRequest(
    string? VmName,
    string? ConfirmName,
    string? Directory,
    string? AllowedRoot,
    VmExportImportAuthContext Auth,
    bool Managed = false,
    int Generation = 0,
    string? PowerState = null,
    bool SecurityFeaturesPresent = false);

public sealed record VmImportRequest(
    string? TargetName,
    string? ConfirmName,
    string? Directory,
    string? AllowedRoot,
    VmExportImportAuthContext Auth,
    string? PackageKind = null,
    bool HasVmcx = false,
    bool SecurityFeaturesPresent = false,
    bool GenerateNewId = false,
    bool TargetExists = false);

public sealed record VmExportImportEvaluation(
    bool Ok,
    string? ErrorCode,
    string? VmName,
    string? Directory,
    string? PackageKind,
    bool GenerateNewId,
    bool ApplyManagedMarker,
    string? Action);

public static class VmExportImportPolicy
{
    public const string Schema = "pcv-vm-export-import-v1";
    public const string ActionExportPreview = "export-preview";
    public const string ActionExport = "export";
    public const string ActionImportPreview = "import-preview";
    public const string ActionImport = "import";
    public const string PackageHyperVExport = "hyperv-export";
    public const string PackageOvf = "ovf";
    public const string PermissionOperate = "operate";
    public const int RequiredGeneration = 2;
    public const string RequiredPowerState = "Off";

    public static string DefaultExportRoot => Path.GetFullPath(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PureCVisor",
            "desktop-node",
            "exports"));

    public static VmExportImportEvaluation EvaluateExportPreview(VmExportRequest request)
    {
        return EvaluateExport(request, ActionExportPreview);
    }

    public static VmExportImportEvaluation EvaluateExport(VmExportRequest request)
    {
        return EvaluateExport(request, ActionExport);
    }

    public static VmExportImportEvaluation EvaluateImportPreview(VmImportRequest request)
    {
        return EvaluateImport(request, ActionImportPreview);
    }

    public static VmExportImportEvaluation EvaluateImport(VmImportRequest request)
    {
        return EvaluateImport(request, ActionImport);
    }

    private static VmExportImportEvaluation EvaluateExport(VmExportRequest request, string action)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var vmName = NormalizeName(request.VmName);
        if (vmName is null)
        {
            return Reject(VmExportImportProblemCodes.VmRequired);
        }

        if (!string.Equals(request.ConfirmName, vmName, StringComparison.Ordinal))
        {
            return Reject(VmExportImportProblemCodes.ConfirmationMismatch);
        }

        if (!request.Managed)
        {
            return Reject(VmExportImportProblemCodes.NotManaged);
        }

        if (request.Generation != RequiredGeneration)
        {
            return Reject(VmExportImportProblemCodes.GenerationUnsupported);
        }

        if (!string.Equals(request.PowerState, RequiredPowerState, StringComparison.OrdinalIgnoreCase))
        {
            return Reject(VmExportImportProblemCodes.SourceNotOff);
        }

        if (request.SecurityFeaturesPresent)
        {
            return Reject(VmExportImportProblemCodes.SecurityFeaturesUnsupported);
        }

        if (string.IsNullOrWhiteSpace(request.Directory) || string.IsNullOrWhiteSpace(request.AllowedRoot))
        {
            return Reject(VmExportImportProblemCodes.PathRequired);
        }

        if (!TryNormalizeAllowedDirectory(request.Directory, request.AllowedRoot, out var directory))
        {
            return Reject(VmExportImportProblemCodes.PathNotAllowed);
        }

        return new VmExportImportEvaluation(
            true,
            null,
            vmName,
            directory,
            PackageHyperVExport,
            false,
            true,
            action);
    }

    private static VmExportImportEvaluation EvaluateImport(VmImportRequest request, string action)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var targetName = NormalizeName(request.TargetName);
        if (targetName is null)
        {
            return Reject(VmExportImportProblemCodes.ImportNameRequired);
        }

        if (!string.Equals(request.ConfirmName, targetName, StringComparison.Ordinal))
        {
            return Reject(VmExportImportProblemCodes.ImportConfirmationMismatch);
        }

        if (IsReservedName(targetName))
        {
            return Reject(VmExportImportProblemCodes.NameInvalid);
        }

        var packageKind = (request.PackageKind ?? string.Empty).Trim();
        if (string.Equals(packageKind, PackageOvf, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(packageKind, "ova", StringComparison.OrdinalIgnoreCase))
        {
            return Reject(VmExportImportProblemCodes.OvfForbidden);
        }

        if (!string.Equals(packageKind, PackageHyperVExport, StringComparison.OrdinalIgnoreCase) ||
            !request.HasVmcx)
        {
            return Reject(VmExportImportProblemCodes.PackageInvalid);
        }

        if (request.SecurityFeaturesPresent)
        {
            return Reject(VmExportImportProblemCodes.ImportSecurityFeaturesUnsupported);
        }

        if (!request.GenerateNewId)
        {
            return Reject(VmExportImportProblemCodes.InPlaceForbidden);
        }

        if (request.TargetExists)
        {
            return Reject(VmExportImportProblemCodes.AlreadyExists);
        }

        if (string.IsNullOrWhiteSpace(request.Directory) || string.IsNullOrWhiteSpace(request.AllowedRoot))
        {
            return Reject(VmExportImportProblemCodes.ImportPathRequired);
        }

        if (!TryNormalizeAllowedDirectory(request.Directory, request.AllowedRoot, out var directory))
        {
            return Reject(VmExportImportProblemCodes.ImportPathNotAllowed);
        }

        return new VmExportImportEvaluation(
            true,
            null,
            targetName,
            directory,
            PackageHyperVExport,
            true,
            true,
            action);
    }

    private static string? RejectAuth(VmExportImportAuthContext auth)
    {
        return auth.HasOperate || auth.HasServiceBearer
            ? null
            : VmExportImportProblemCodes.Forbidden;
    }

    private static string? NormalizeName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsReservedName(string name)
    {
        return string.Equals(name, ".", StringComparison.Ordinal) ||
            string.Equals(name, "..", StringComparison.Ordinal);
    }

    public static bool TryNormalizeAllowedDirectory(string? path, string? allowedRoot, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(allowedRoot))
        {
            return false;
        }

        var trimmed = path.Trim();
        if (trimmed.IndexOfAny(['*', '?', '"', '<', '>', '|']) >= 0)
        {
            return false;
        }

        if (trimmed.StartsWith(@"\\", StringComparison.Ordinal) ||
            trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        if (!Path.IsPathRooted(trimmed))
        {
            return false;
        }

        try
        {
            var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(allowedRoot.Trim()));
            var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(trimmed));
            var relative = Path.GetRelativePath(fullRoot, fullPath);
            if (string.IsNullOrWhiteSpace(relative) ||
                relative == "." ||
                relative.Equals("..", StringComparison.Ordinal) ||
                relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
                Path.IsPathRooted(relative))
            {
                return false;
            }

            normalized = fullPath;
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static VmExportImportEvaluation Reject(string code)
    {
        return new VmExportImportEvaluation(false, code, null, null, null, false, false, null);
    }
}

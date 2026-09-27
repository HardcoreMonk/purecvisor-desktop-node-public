using System.Globalization;
using System.Management;
using DesktopNode.Contracts;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmImportProvider : IDesktopNodeHyperVVmImportProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    public const string SecuritySettingClass = "Msvm_SecuritySettingData";
    public const string ImportSystemDefinitionMethod = "ImportSystemDefinition";
    public const string RealizePlannedSystemMethod = "RealizePlannedSystem";
    public const string ModifySystemSettingsMethod = "ModifySystemSettings";
    public const string DestroySystemMethod = "DestroySystem";

    public DesktopNodeHyperVVmImportInfo Invoke(
        DesktopNodeHyperVVmImportRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!request.GenerateNewId)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.InPlaceForbidden,
                "In-place Hyper-V import is not allowed.",
                "Pass generate_new_id=true so the imported VM receives a new identity.",
                false);
        }

        if (!VmExportImportPolicy.TryNormalizeAllowedDirectory(request.Directory, request.AllowedRoot, out var directory))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.ImportPathNotAllowed,
                "The import directory is not inside the allowlist root.",
                "Use a subdirectory of the configured export root. UNC and parent-path escape are rejected.",
                false);
        }

        ValidatePackageContent(directory);
        var vmcxPath = FindVmcx(directory);
        var snapshotFolder = Path.Combine(directory, "Snapshots");
        if (!Directory.Exists(snapshotFolder))
        {
            snapshotFolder = string.Empty;
        }

        var scope = CreateScope();
        if (FindVm(scope, request.TargetName, cancellationToken) is { } existing)
        {
            existing.Dispose();
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.AlreadyExists,
                $"VM '{request.TargetName}' already exists.",
                "Choose a new display name. In-place import is not allowed.",
                false);
        }

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var importIn = service.GetMethodParameters(ImportSystemDefinitionMethod);
        importIn["SystemDefinitionFile"] = vmcxPath;
        importIn["SnapshotFolder"] = snapshotFolder;
        importIn["GenerateNewSystemIdentifier"] = true;
        cancellationToken.ThrowIfCancellationRequested();
        using var importOut = service.InvokeMethod(ImportSystemDefinitionMethod, importIn, null);
        WaitForMethodResult(importOut, "vm.import", cancellationToken);
        using var planned = RequireOutputObject(importOut, "ImportedSystem", "vm.import");
        try
        {
            using var plannedSettings = FindCurrentSettings(planned, cancellationToken)
                ?? throw new DesktopNodeHyperVNativeOperationException(
                    "PCV_VM_SETTINGS_NOT_FOUND",
                    $"Imported planned VM '{request.TargetName}' settings were not found.",
                    "Msvm_VirtualSystemSettingData was not available for the planned system.",
                    true);

            if (SecurityFeaturesPresent(plannedSettings, cancellationToken))
            {
                throw new DesktopNodeHyperVNativeOperationException(
                    VmExportImportProblemCodes.ImportSecurityFeaturesUnsupported,
                    $"Imported VM '{request.TargetName}' has TPM, key protector, or shielded security features.",
                    "This import path does not copy security key material.",
                    false);
            }

            plannedSettings["ElementName"] = request.TargetName;
            plannedSettings["Notes"] = new[] { DesktopNodeHyperVManagedNotes.Marker };
            ApplySystemSettings(service, plannedSettings, "vm.import.rename", cancellationToken);

            using var realizeIn = service.GetMethodParameters(RealizePlannedSystemMethod);
            realizeIn["PlannedSystem"] = planned;
            cancellationToken.ThrowIfCancellationRequested();
            using var realizeOut = service.InvokeMethod(RealizePlannedSystemMethod, realizeIn, null);
            WaitForMethodResult(realizeOut, "vm.import.realize", cancellationToken);
        }
        catch
        {
            TryDestroy(service, planned, CancellationToken.None);
            throw;
        }

        using var realized = FindVm(scope, request.TargetName, cancellationToken)
            ?? throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_LOOKUP_FAILED",
                $"VM '{request.TargetName}' lookup failed after import.",
                "The VM was not visible after RealizePlannedSystem completed.",
                true);
        using var realizedSettings = FindCurrentSettings(realized, cancellationToken)
            ?? throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_SETTINGS_NOT_FOUND",
                $"VM '{request.TargetName}' settings were not found after import.",
                "Msvm_VirtualSystemSettingData was not available for the realized VM.",
                true);
        var notes = GetStringProperty(realizedSettings, "Notes");
        if (!DesktopNodeHyperVManagedNotes.IsManagedNotes(notes) ||
            DesktopNodeHyperVManagedNotes.IsTemplateLocked(notes) ||
            DesktopNodeHyperVManagedNotes.OperatorNotes(notes) is not null)
        {
            realizedSettings["Notes"] = new[] { DesktopNodeHyperVManagedNotes.Marker };
            ApplySystemSettings(service, realizedSettings, "vm.import.marker", cancellationToken);
        }

        return new DesktopNodeHyperVVmImportInfo(
            "import",
            directory,
            VmExportImportPolicy.PackageHyperVExport,
            request.TargetName,
            true,
            true);
    }

    // Generation 2 exports always carry a .vmgs guest-state file, so the file itself is not a rejection
    // signal. TPM, shielding, and key-protector material is rejected from the planned VM security settings.
    public static void ValidatePackageContent(string directory)
    {
        if (HasPackageExtension(directory, ".ovf") || HasPackageExtension(directory, ".ova"))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.OvfForbidden,
                "OVF/OVA packages are not allowed.",
                "Import only a Hyper-V export folder that contains a .vmcx definition.",
                false);
        }
    }

    private static string FindVmcx(string directory)
    {
        var matches = EnumeratePackageFiles(directory, "*.vmcx", includeSnapshots: false).ToArray();
        if (matches.Length != 1)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.PackageInvalid,
                "The import directory is not a Hyper-V export folder with a single .vmcx.",
                "Export a managed Generation 2 VM into the allowlist root, then import that folder.",
                false);
        }

        return matches[0];
    }

    private static bool HasPackageExtension(string directory, string extension)
    {
        var pattern = "*" + extension;
        return EnumeratePackageFiles(directory, pattern, includeSnapshots: true).Any();
    }

    private static IEnumerable<string> EnumeratePackageFiles(string directory, string pattern, bool includeSnapshots)
    {
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly))
        {
            yield return file;
        }

        var virtualMachines = Path.Combine(directory, "Virtual Machines");
        if (Directory.Exists(virtualMachines))
        {
            foreach (var file in Directory.EnumerateFiles(virtualMachines, pattern, SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }

        if (!includeSnapshots)
        {
            yield break;
        }

        var snapshots = Path.Combine(directory, "Snapshots");
        if (Directory.Exists(snapshots))
        {
            foreach (var file in Directory.EnumerateFiles(snapshots, pattern, SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }
    }

    private static ManagementObject RequireOutputObject(ManagementBaseObject outParams, string propertyName, string operation)
    {
        var value = outParams.Properties[propertyName]?.Value;
        if (value is ManagementObject embedded)
        {
            return new ManagementObject(embedded.Path);
        }

        var path = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(path))
        {
            var item = new ManagementObject(path);
            item.Get();
            return item;
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_IMPORT_FAILED",
            $"Native Hyper-V import did not return '{propertyName}'.",
            $"WMI {operation} completed without an object path for {propertyName}.",
            true);
    }

    private static void ApplySystemSettings(
        ManagementObject service,
        ManagementObject settings,
        string operation,
        CancellationToken cancellationToken)
    {
        using var inParams = service.GetMethodParameters(ModifySystemSettingsMethod);
        inParams.Properties["SystemSettings"].Value = settings.GetText(TextFormat.CimDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifySystemSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static void TryDestroy(ManagementObject service, ManagementObject system, CancellationToken cancellationToken)
    {
        try
        {
            using var inParams = service.GetMethodParameters(DestroySystemMethod);
            inParams["AffectedSystem"] = system.Path.Path;
            using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
            WaitForMethodResult(outParams, "vm.import.cleanup", cancellationToken);
        }
        catch
        {
        }
    }

    private static ManagementObject? FindCurrentSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = vm.GetRelated(VirtualSystemSettingDataClass);
        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Properties["VirtualSystemType"]?.Value is string type &&
                type.Contains("Snapshot", StringComparison.OrdinalIgnoreCase))
            {
                item.Dispose();
                continue;
            }

            return item;
        }

        return null;
    }

    private static bool SecurityFeaturesPresent(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var related = settings.GetRelated(SecuritySettingClass);
            foreach (ManagementObject item in related)
            {
                using (item)
                {
                    if (IsEnabledFlag(item, "TpmEnabled") ||
                        IsEnabledFlag(item, "ShieldingRequested") ||
                        IsEnabledFlag(item, "EncryptStateAndVmMigrationTraffic") ||
                        IsEnabledFlag(item, "DataProtectionRequested") ||
                        !string.IsNullOrWhiteSpace(GetStringProperty(item, "KeyProtector")))
                    {
                        return true;
                    }
                }
            }
        }
        catch (ManagementException)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.ImportSecurityFeaturesUnsupported,
                "Hyper-V security settings could not be inspected.",
                "Import refuses to proceed when TPM/key protector state cannot be proven absent.",
                false);
        }
        catch (UnauthorizedAccessException)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.ImportSecurityFeaturesUnsupported,
                "Hyper-V security settings could not be inspected.",
                "Import refuses to proceed when TPM/key protector state cannot be proven absent.",
                false);
        }

        return false;
    }

    private static bool IsEnabledFlag(ManagementBaseObject item, string propertyName)
    {
        var value = item.Properties[propertyName]?.Value;
        return value is bool flag && flag ||
            value is not null &&
            Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0;
    }
}

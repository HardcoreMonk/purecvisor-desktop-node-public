using System.Globalization;
using System.Management;
using DesktopNode.Contracts;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmExportProvider : IDesktopNodeHyperVVmExportProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    public const string SecuritySettingClass = "Msvm_SecuritySettingData";
    public const string ExportSettingDataClass = "Msvm_VirtualSystemExportSettingData";
    public const string ExportSystemDefinitionMethod = "ExportSystemDefinition";

    public DesktopNodeHyperVVmExportInfo Invoke(
        DesktopNodeHyperVVmExportRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!VmExportImportPolicy.TryNormalizeAllowedDirectory(request.Directory, request.AllowedRoot, out var directory))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.PathNotAllowed,
                "The export directory is not inside the allowlist root.",
                "Use a subdirectory of the configured export root. UNC and parent-path escape are rejected.",
                false);
        }

        var scope = CreateScope();
        using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{request.VmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        var power = MapEnabledState(vm.Properties["EnabledState"]?.Value);
        if (!string.Equals(power, "stopped", StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.SourceNotOff,
                $"VM '{request.VmName}' is '{power}', not Off.",
                "Power off the VM, then retry export.",
                false);
        }

        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{request.VmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);

        var notes = GetStringProperty(settings, "Notes");
        if (!DesktopNodeHyperVManagedNotes.IsManagedNotes(notes))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.NotManaged,
                $"VM '{request.VmName}' is not managed by PureCVisor Desktop Node.",
                $"Refusing export for a VM without the {DesktopNodeHyperVManagedNotes.Marker} marker.",
                false);
        }

        if (MapGeneration(GetStringProperty(settings, "VirtualSystemSubType")) != VmExportImportPolicy.RequiredGeneration)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.GenerationUnsupported,
                $"VM '{request.VmName}' is not Generation 2.",
                "Export only Generation 2 managed VMs.",
                false);
        }

        if (SecurityFeaturesPresent(settings, cancellationToken))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.SecurityFeaturesUnsupported,
                $"VM '{request.VmName}' has TPM, key protector, or shielded security features.",
                "This export path does not copy security key material.",
                false);
        }

        Directory.CreateDirectory(directory);
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var exportSettings = new ManagementClass(scope, new ManagementPath(ExportSettingDataClass), null).CreateInstance()
            ?? throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_EXPORT_FAILED",
                "Hyper-V export settings could not be created.",
                "Msvm_VirtualSystemExportSettingData was unavailable.",
                true);
        exportSettings["CopySnapshotConfiguration"] = 0;
        exportSettings["CopyVmRuntimeInformation"] = false;
        exportSettings["CopyVmStorage"] = true;
        exportSettings["CreateVmExportSubdirectory"] = false;
        using var inParams = service.GetMethodParameters(ExportSystemDefinitionMethod);
        inParams["ComputerSystem"] = vm;
        inParams["ExportDirectory"] = directory;
        inParams["ExportSettingData"] = exportSettings.GetText(TextFormat.CimDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ExportSystemDefinitionMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.export", cancellationToken);
        return new DesktopNodeHyperVVmExportInfo("export", directory, VmExportImportPolicy.PackageHyperVExport, request.VmName);
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

    private static int? MapGeneration(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype))
        {
            return null;
        }

        var trimmed = subtype.Trim();
        if (trimmed.EndsWith(":2", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (trimmed.EndsWith(":1", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
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
                VmExportImportProblemCodes.SecurityFeaturesUnsupported,
                "Hyper-V security settings could not be inspected.",
                "Export refuses to proceed when TPM/key protector state cannot be proven absent.",
                false);
        }
        catch (UnauthorizedAccessException)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmExportImportProblemCodes.SecurityFeaturesUnsupported,
                "Hyper-V security settings could not be inspected.",
                "Export refuses to proceed when TPM/key protector state cannot be proven absent.",
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

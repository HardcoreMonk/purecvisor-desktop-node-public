using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmDeleteProvider : IDesktopNodeHyperVVmDeleteProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string DestroySystemMethod = "DestroySystem";
    private const string VirtualHardDiskSubtype = "Microsoft:Hyper-V:Virtual Hard Disk";

    private readonly IDesktopNodeHyperVVmStorageFileSystem fileSystem;

    public DesktopNodeHyperVWmiVmDeleteProvider()
        : this(new DesktopNodeHyperVPhysicalVmStorageFileSystem())
    {
    }

    public DesktopNodeHyperVWmiVmDeleteProvider(IDesktopNodeHyperVVmStorageFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public DesktopNodeHyperVVmDeleteInfo Invoke(string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope(connect: true);

        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        var (configurationRoot, attachedDisks) = ReadStorageLayout(vm, cancellationToken);

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(DestroySystemMethod);
        inParams["AffectedSystem"] = vm.Path.Path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.delete", cancellationToken);

        // The VM is gone at this point; storage cleanup reports what it kept instead of failing the delete.
        DesktopNodeHyperVVmStorageCleanupInfo cleanup;
        try
        {
            cleanup = DesktopNodeHyperVVmStorageCleanup.Run(
                configurationRoot,
                attachedDisks,
                ReadRemainingVmDisks(scope),
                fileSystem);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ManagementException)
        {
            cleanup = new DesktopNodeHyperVVmStorageCleanupInfo(
                configurationRoot,
                [],
                [],
                attachedDisks
                    .Select(disk => new DesktopNodeHyperVRetainedStorage(disk, DesktopNodeHyperVVmStorageCleanup.DeleteFailedPrefix + ex.Message))
                    .ToArray());
        }

        return new DesktopNodeHyperVVmDeleteInfo(vmName, "delete", cleanup);
    }

    private static (string? ConfigurationRoot, IReadOnlyList<string> AttachedDisks) ReadStorageLayout(
        ManagementObject vm,
        CancellationToken cancellationToken)
    {
        try
        {
            using var settings = GetRealizedSettings(vm, cancellationToken);
            if (settings is null)
            {
                return (null, []);
            }

            var disks = new List<string>();
            using var resources = settings.GetRelated(null, "Msvm_VirtualSystemSettingDataComponent", null, null, "PartComponent", "GroupComponent", false, null);
            foreach (ManagementObject item in resources)
            {
                using (item)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.Equals(GetStringProperty(item, "ResourceSubType"), VirtualHardDiskSubtype, StringComparison.OrdinalIgnoreCase) &&
                        GetFirstHostResource(item) is { } path)
                    {
                        disks.Add(path);
                    }
                }
            }

            return (GetStringProperty(settings, "ConfigurationDataRoot"), disks);
        }
        catch (ManagementException)
        {
            return (null, []);
        }
    }

    private static ManagementObject? GetRealizedSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        foreach (ManagementObject item in vm.GetRelated("Msvm_VirtualSystemSettingData", "Msvm_SettingsDefineState", null, null, "SettingData", "ManagedElement", false, null))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(GetStringProperty(item, "VirtualSystemType"), "Microsoft:Hyper-V:System:Realized", StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    // Every virtual hard disk still attached to a remaining VM or checkpoint, or null when unreadable
    // (the cleanup then keeps everything).
    internal static IReadOnlyList<string>? ReadRemainingVmDisks(ManagementScope scope)
    {
        try
        {
            var disks = new List<string>();
            using var searcher = new ManagementObjectSearcher(
                scope,
                new ObjectQuery($"SELECT HostResource FROM Msvm_StorageAllocationSettingData WHERE ResourceSubType = '{VirtualHardDiskSubtype}'"));
            foreach (ManagementObject item in searcher.Get())
            {
                using (item)
                {
                    if (GetFirstHostResource(item) is { } path)
                    {
                        disks.Add(path);
                    }
                }
            }

            return disks;
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static string? GetFirstHostResource(ManagementBaseObject item)
    {
        try
        {
            var value = item.Properties["HostResource"]?.Value;
            var path = value is string[] values ? values.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate)) : value as string;
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }
        catch (ManagementException)
        {
            return null;
        }
    }
}

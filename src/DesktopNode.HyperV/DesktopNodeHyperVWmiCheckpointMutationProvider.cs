using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiCheckpointMutationProvider : IDesktopNodeHyperVCheckpointMutationProvider
{
    private const string SnapshotSettingClass = "Msvm_VirtualSystemSettingData";
    private const string SnapshotAssociationClass = "Msvm_SnapshotOfVirtualSystem";
    private const string SnapshotServiceQuery = "SELECT * FROM Msvm_VirtualSystemSnapshotService";
    private const string ManagementServiceQuery = "SELECT * FROM Msvm_VirtualSystemManagementService";
    public const string ApplySnapshotMethod = "ApplySnapshot";

    public const ushort FullSnapshotType = 2;
    public const uint Completed = DesktopNodeHyperVWmiCommon.Completed;
    public const uint JobStarted = DesktopNodeHyperVWmiCommon.JobStarted;

    public DesktopNodeHyperVCheckpointMutationInfo Invoke(string operation, string vmName, string checkpointName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (operation is not ("checkpoint.create" or "checkpoint.restore" or "checkpoint.delete"))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_OPERATION_NOT_ALLOWED",
                $"Operation '{operation}' is not a native checkpoint mutation operation.",
                "Use checkpoint.create, checkpoint.restore, or checkpoint.delete for this native mutation slice.",
                false);
        }

        var scope = CreateScope();
        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        if (operation == "checkpoint.create")
        {
            CreateCheckpoint(scope, vm, vmName, checkpointName, cancellationToken);
            return new DesktopNodeHyperVCheckpointMutationInfo(vmName, checkpointName, null);
        }

        if (operation == "checkpoint.restore")
        {
            RestoreCheckpoint(scope, vm, vmName, checkpointName, cancellationToken);
            return new DesktopNodeHyperVCheckpointMutationInfo(vmName, checkpointName, "restore");
        }

        DeleteCheckpoint(scope, vm, vmName, checkpointName, cancellationToken);
        return new DesktopNodeHyperVCheckpointMutationInfo(vmName, checkpointName, "delete");
    }

    private static void CreateCheckpoint(ManagementScope scope, ManagementObject vm, string vmName, string checkpointName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (var existing = FindSnapshot(vm, checkpointName, cancellationToken))
        {
            if (existing is not null)
            {
                throw new DesktopNodeHyperVNativeOperationException(
                    "PCV_CHECKPOINT_ALREADY_EXISTS",
                    $"Checkpoint '{checkpointName}' already exists for VM '{vmName}'.",
                    "Choose a unique checkpoint name before creating a checkpoint.",
                    false);
            }
        }

        using var snapshotService = GetSingleService(scope, SnapshotServiceQuery, "Msvm_VirtualSystemSnapshotService", cancellationToken);
        var beforeSnapshotIds = GetSnapshotInstanceIds(vm, cancellationToken);
        using var inParams = snapshotService.GetMethodParameters("CreateSnapshot");
        inParams.Properties["AffectedSystem"].Value = vm.Path.Path;
        inParams.Properties["SnapshotSettings"].Value = null;
        inParams.Properties["SnapshotType"].Value = FullSnapshotType;
        cancellationToken.ThrowIfCancellationRequested();

        using var outParams = snapshotService.InvokeMethod("CreateSnapshot", inParams, null);
        WaitForMethodResult(outParams, "checkpoint.create", cancellationToken);

        using var created = FindCreatedSnapshot(vm, beforeSnapshotIds, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_CHECKPOINT_NOT_VISIBLE",
            $"Checkpoint '{checkpointName}' was not visible after creation.",
            "Native WMI snapshot creation completed but no new snapshot row was visible.",
            true);

        RenameSnapshot(scope, created, checkpointName, cancellationToken);

        using var renamed = FindSnapshot(vm, checkpointName, cancellationToken);
        if (renamed is null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_CHECKPOINT_NOT_VISIBLE",
                $"Checkpoint '{checkpointName}' was not visible after rename.",
                "Native WMI snapshot rename completed but checkpoint.list could not find the requested name.",
                true);
        }
    }

    private static void DeleteCheckpoint(ManagementScope scope, ManagementObject vm, string vmName, string checkpointName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var snapshot = FindSnapshot(vm, checkpointName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_CHECKPOINT_NOT_FOUND",
            $"Checkpoint '{checkpointName}' was not found for VM '{vmName}'.",
            "The checkpoint was not present in the native Hyper-V snapshot inventory response.",
            false);

        using var snapshotService = GetSingleService(scope, SnapshotServiceQuery, "Msvm_VirtualSystemSnapshotService", cancellationToken);
        using var inParams = snapshotService.GetMethodParameters("DestroySnapshot");
        inParams.Properties["AffectedSnapshot"].Value = snapshot.Path.Path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = snapshotService.InvokeMethod("DestroySnapshot", inParams, null);
        WaitForMethodResult(outParams, "checkpoint.delete", cancellationToken);

        using var remaining = FindSnapshot(vm, checkpointName, cancellationToken);
        if (remaining is not null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_CHECKPOINT_DELETE_FAILED",
                $"Checkpoint '{checkpointName}' was still visible after delete.",
                "Native WMI snapshot deletion completed but checkpoint.list still found the requested name.",
                true);
        }
    }

    private static void RestoreCheckpoint(ManagementScope scope, ManagementObject vm, string vmName, string checkpointName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var snapshot = FindSnapshot(vm, checkpointName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_CHECKPOINT_NOT_FOUND",
            $"Checkpoint '{checkpointName}' was not found for VM '{vmName}'.",
            "The checkpoint was not present in the native Hyper-V snapshot inventory response.",
            false);

        using var snapshotService = GetSingleService(scope, SnapshotServiceQuery, "Msvm_VirtualSystemSnapshotService", cancellationToken);
        using var inParams = snapshotService.GetMethodParameters(ApplySnapshotMethod);
        inParams.Properties["Snapshot"].Value = snapshot.Path.Path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = snapshotService.InvokeMethod(ApplySnapshotMethod, inParams, null);
        WaitForMethodResult(outParams, "checkpoint.restore", cancellationToken);
    }

    private static void RenameSnapshot(ManagementScope scope, ManagementObject snapshot, string checkpointName, CancellationToken cancellationToken)
    {
        using var managementService = GetSingleService(scope, ManagementServiceQuery, "Msvm_VirtualSystemManagementService", cancellationToken);
        snapshot.Properties["ElementName"].Value = checkpointName;
        using var inParams = managementService.GetMethodParameters("ModifySystemSettings");
        inParams.Properties["SystemSettings"].Value = snapshot.GetText(TextFormat.CimDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = managementService.InvokeMethod("ModifySystemSettings", inParams, null);
        WaitForMethodResult(outParams, "checkpoint.create", cancellationToken);
    }

    private static ManagementObject? FindCreatedSnapshot(ManagementObject vm, ISet<string> beforeSnapshotIds, CancellationToken cancellationToken)
    {
        var snapshots = GetSnapshots(vm, cancellationToken);
        try
        {
            var created = snapshots
                .Where(item =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return !beforeSnapshotIds.Contains(GetStringProperty(item, "InstanceID") ?? string.Empty);
                })
                .OrderByDescending(item => GetStringProperty(item, "CreationTime"), StringComparer.Ordinal)
                .FirstOrDefault();

            if (created is not null)
            {
                snapshots.Remove(created);
            }

            return created;
        }
        finally
        {
            foreach (var snapshot in snapshots)
            {
                snapshot.Dispose();
            }
        }
    }

    private static ISet<string> GetSnapshotInstanceIds(ManagementObject vm, CancellationToken cancellationToken)
    {
        var snapshots = GetSnapshots(vm, cancellationToken);
        try
        {
            return snapshots
                .Select(item =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return GetStringProperty(item, "InstanceID");
                })
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
        }
        finally
        {
            foreach (var snapshot in snapshots)
            {
                snapshot.Dispose();
            }
        }
    }

    private static ManagementObject? FindSnapshot(ManagementObject vm, string checkpointName, CancellationToken cancellationToken)
    {
        var snapshots = GetSnapshots(vm, cancellationToken);
        foreach (var snapshot in snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(GetStringProperty(snapshot, "ElementName"), checkpointName, StringComparison.OrdinalIgnoreCase))
            {
                snapshots.Remove(snapshot);
                foreach (var remaining in snapshots)
                {
                    remaining.Dispose();
                }

                return snapshot;
            }
        }

        foreach (var snapshot in snapshots)
        {
            snapshot.Dispose();
        }

        return null;
    }

    private static List<ManagementObject> GetSnapshots(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var snapshots = vm.GetRelated(
            SnapshotSettingClass,
            SnapshotAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "Dependent",
            thisRole: "Antecedent",
            classDefinitionsOnly: false,
            options: null);

        var result = new List<ManagementObject>();
        foreach (ManagementObject snapshot in snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(snapshot);
        }

        return result;
    }

}

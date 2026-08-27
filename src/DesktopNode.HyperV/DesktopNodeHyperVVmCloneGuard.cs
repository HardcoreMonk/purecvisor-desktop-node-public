namespace DesktopNode.HyperV;

public static class DesktopNodeHyperVVmCloneGuard
{
    public static bool TryPlan(
        DesktopNodeHyperVVmCloneSourceSnapshot source,
        DesktopNodeHyperVVmCloneRequest request,
        bool targetExists,
        out DesktopNodeHyperVVmClonePlan plan,
        out DesktopNodeHyperVNativeOperationException? error)
    {
        plan = null!;
        error = null;

        if (!source.Managed)
        {
            error = Reject(
                "PCV_VM_NOT_MANAGED_BY_PURECVISOR",
                $"VM '{source.Name}' is not managed by PureCVisor Desktop Node.",
                $"Refusing clone for a VM without the {DesktopNodeHyperVManagedNotes.Marker} marker.");
            return false;
        }

        if (source.Generation != 2)
        {
            error = Reject(
                "PCV_VM_GENERATION_UNSUPPORTED",
                $"VM '{source.Name}' generation '{source.Generation}' is unsupported.",
                "Clone only Generation 2 managed VMs.");
            return false;
        }

        if (!string.Equals(source.PowerState, "Off", StringComparison.Ordinal))
        {
            error = Reject(
                "PCV_VM_CLONE_SOURCE_NOT_OFF",
                $"VM '{source.Name}' is '{source.PowerState}', not Off.",
                "Power off the source VM, then retry clone.");
            return false;
        }

        if (source.CheckpointCount > 0)
        {
            error = Reject(
                "PCV_VM_CLONE_CHECKPOINTS_PRESENT",
                $"VM '{source.Name}' has {source.CheckpointCount} checkpoint(s).",
                "Delete checkpoints before cloning. Flatten is not supported.");
            return false;
        }

        foreach (var disk in source.Disks)
        {
            if (!disk.IndependentVhdx)
            {
                error = Reject(
                    "PCV_VM_CLONE_DISK_NOT_INDEPENDENT",
                    $"VM '{source.Name}' has a disk that is not an independent VHDX.",
                    "Clone only sources whose disks are independent VHDX files.");
                return false;
            }
        }

        if (source.SecurityFeaturesPresent)
        {
            error = Reject(
                "PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED",
                $"VM '{source.Name}' has TPM, key protector, or shielded security features.",
                "This clone path does not copy security key material.");
            return false;
        }

        if (targetExists)
        {
            error = Reject(
                "PCV_VM_ALREADY_EXISTS",
                $"VM '{request.TargetName}' already exists.",
                "Choose a different VM name or remove the existing Hyper-V VM.");
            return false;
        }

        if (string.Equals(request.TargetName, request.SourceName, StringComparison.Ordinal))
        {
            error = Reject(
                "PCV_VM_CLONE_NAME_CONFLICT",
                $"Target name '{request.TargetName}' matches the source display name.",
                "Choose a target display name different from the source.");
            return false;
        }

        var directory = Path.Combine(request.VmRoot, request.TargetName);
        var disks = new DesktopNodeHyperVVmCloneDiskPlan[source.Disks.Count];
        long plannedCopyBytes = 0;
        for (var index = 0; index < source.Disks.Count; index++)
        {
            var disk = source.Disks[index];
            plannedCopyBytes += disk.FileLength;
            disks[index] = new DesktopNodeHyperVVmCloneDiskPlan(
                disk.SourcePath,
                Path.Combine(directory, $"disk{index}.vhdx"));
        }

        plan = new DesktopNodeHyperVVmClonePlan(
            request.SourceName,
            request.TargetName,
            "preview",
            source.Generation,
            directory,
            disks.Length,
            plannedCopyBytes,
            disks);
        return true;
    }

    private static DesktopNodeHyperVNativeOperationException Reject(string code, string message, string detail)
    {
        return new DesktopNodeHyperVNativeOperationException(code, message, detail, false);
    }
}

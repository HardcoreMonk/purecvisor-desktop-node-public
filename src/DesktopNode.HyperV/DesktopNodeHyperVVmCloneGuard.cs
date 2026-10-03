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

        if (string.IsNullOrWhiteSpace(request.TargetName) || IsReservedCloneTargetName(request.TargetName))
        {
            error = InvalidCloneTargetName(request.TargetName);
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

        if (!TryResolveContainedCloneDirectory(request.VmRoot, request.TargetName, out var directory))
        {
            error = InvalidCloneTargetName(request.TargetName);
            return false;
        }

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

    internal static bool IsReservedCloneTargetName(string? name)
    {
        return string.Equals(name, ".", StringComparison.Ordinal) ||
            string.Equals(name, "..", StringComparison.Ordinal);
    }

    internal static bool TryResolveContainedCloneDirectory(string vmRoot, string targetName, out string directory)
    {
        directory = string.Empty;
        if (string.IsNullOrWhiteSpace(vmRoot) || string.IsNullOrWhiteSpace(targetName) || IsReservedCloneTargetName(targetName))
        {
            return false;
        }

        try
        {
            directory = Path.GetFullPath(Path.Combine(vmRoot, targetName));
            return IsContainedCloneDirectory(vmRoot, directory);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            directory = string.Empty;
            return false;
        }
    }

    internal static bool IsContainedCloneDirectory(string vmRoot, string directory)
    {
        if (string.IsNullOrWhiteSpace(vmRoot) || string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        try
        {
            var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(vmRoot));
            var fullDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
            var relative = Path.GetRelativePath(fullRoot, fullDirectory);
            return !string.IsNullOrWhiteSpace(relative) &&
                relative != "." &&
                !relative.Equals("..", StringComparison.Ordinal) &&
                !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
                !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) &&
                !Path.IsPathRooted(relative);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    internal static DesktopNodeHyperVNativeOperationException InvalidCloneTargetName(string? targetName)
    {
        return Reject(
            "PCV_VM_NAME_INVALID",
            $"VM name '{targetName ?? string.Empty}' is invalid.",
            "Use a VM display name that is not '.' or '..' and resolves to a subdirectory of the VM root.");
    }

    private static DesktopNodeHyperVNativeOperationException Reject(string code, string message, string detail)
    {
        return new DesktopNodeHyperVNativeOperationException(code, message, detail, false);
    }
}

using System.Diagnostics;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

// Create failure cleanup and interrupted-create residue handling (design pcv-interrupted-create-residue-v1).
public sealed partial class DesktopNodeHyperVWmiVmCreateProvider
{
    private static readonly DesktopNodeHyperVPhysicalCreateResidueFileSystem ResidueFileSystem = new();
    private static readonly DesktopNodeHyperVSystemProcessProbe ProcessProbe = new();

    // Written before the disk so an interrupted create leaves proof that PureCVisor owns disk0.vhdx.
    private static bool WriteCreateMarker(string markerPath, string vmName, bool directoryCreated)
    {
        using var self = Process.GetCurrentProcess();
        ResidueFileSystem.WriteAllText(
            markerPath,
            DesktopNodeHyperVVmCreateResidue.NewMarker(vmName, directoryCreated, self.Id, self.StartTime.ToUniversalTime(), DateTime.UtcNow));
        return true;
    }

    private static bool TryDeleteCreateMarker(string markerPath)
    {
        try
        {
            ResidueFileSystem.DeleteFile(markerPath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void Cleanup(
        ManagementScope scope,
        string vmName,
        bool vmCreated,
        bool vmDirectoryCreated,
        bool vmDirectoryPreExisting,
        string vmDirectory,
        bool vhdCreated,
        bool vhdPreExisting,
        string vhdPath,
        bool markerWritten,
        string markerPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (vmCreated)
        {
            try
            {
                using var vm = FindVm(scope, vmName, cancellationToken);
                if (vm is not null)
                {
                    using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
                    using var inParams = service.GetMethodParameters(DestroySystemMethod);
                    inParams["AffectedSystem"] = vm.Path.Path;
                    cancellationToken.ThrowIfCancellationRequested();
                    using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
                    WaitForMethodResult(outParams, "vm.create.cleanup", cancellationToken);
                }
            }
            catch
            {
                // Best-effort cleanup preserves the primary create failure detail.
            }
        }

        if (vmDirectoryCreated && !vmDirectoryPreExisting && Directory.Exists(vmDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { Directory.Delete(vmDirectory, recursive: true); } catch { }
        }
        else if (vhdCreated && !vhdPreExisting && File.Exists(vhdPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { File.Delete(vhdPath); } catch { }
        }

        if (markerWritten && File.Exists(markerPath))
        {
            try { File.Delete(markerPath); } catch { }
        }
    }

    // Removes residue a create interrupted before DefineSystem left behind, or rejects with the reason and deletes
    // nothing (design pcv-interrupted-create-residue-v1). Disk references are read only when residue exists.
    private static DesktopNodeHyperVRecoveredResidue? RecoverInterruptedCreate(
        ManagementScope scope,
        string vmName,
        string vmDirectory,
        string vhdPath,
        string markerPath)
    {
        if (!File.Exists(vhdPath) && !File.Exists(markerPath))
        {
            return null;
        }

        var decision = DesktopNodeHyperVVmCreateResidue.TryRecover(
            vmDirectory,
            vmName,
            DesktopNodeHyperVWmiVmDeleteProvider.ReadRemainingVmDisks(scope),
            ResidueFileSystem,
            ProcessProbe);
        if (decision.RejectReason is not null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VHD_ALREADY_EXISTS",
                $"VHD '{vhdPath}' already exists.",
                $"Choose an empty VM root or remove the existing disk file. Residue recovery refused: {decision.RejectReason}.",
                false);
        }

        return decision.Recovered;
    }
}

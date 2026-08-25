using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiCheckpointProvider : IDesktopNodeHyperVCheckpointProvider
{
    private const string SnapshotSettingClass = "Msvm_VirtualSystemSettingData";

    public const string VmQuery = DesktopNodeHyperVWmiCommon.VmQuery;
    public const string SnapshotAssociationClass = "Msvm_SnapshotOfVirtualSystem";
    public const string CurrentSnapshotAssociationClass = "Msvm_MostCurrentSnapshotInBranch";

    public IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, vmName, cancellationToken);
        if (vm is null)
        {
            return [];
        }

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

        var result = new List<DesktopNodeHyperVCheckpointInfo>();
        foreach (ManagementObject item in snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (item)
            {
                var instanceId = GetStringProperty(item, "InstanceID");
                var name = GetStringProperty(item, "ElementName") ??
                    instanceId ??
                    string.Empty;
                result.Add(new DesktopNodeHyperVCheckpointInfo(
                    Name: name,
                    VmName: vmName,
                    CreatedAt: GetDateTimeProperty(item, "CreationTime"),
                    InstanceId: instanceId));
            }
        }

        return MarkCurrent(result, TryReadCurrentSnapshotInstanceId(vm, cancellationToken));
    }

    public static IReadOnlyList<DesktopNodeHyperVCheckpointInfo> MarkCurrent(
        IReadOnlyList<DesktopNodeHyperVCheckpointInfo> rows,
        string? currentInstanceId)
    {
        if (rows.Count == 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(currentInstanceId))
        {
            return CopyWithCurrent(rows, isCurrent: null);
        }

        var matchCount = 0;
        var matchIndex = -1;
        for (var i = 0; i < rows.Count; i++)
        {
            if (!string.Equals(rows[i].InstanceId, currentInstanceId, StringComparison.Ordinal))
            {
                continue;
            }

            matchCount++;
            matchIndex = i;
        }

        if (matchCount != 1)
        {
            return CopyWithCurrent(rows, isCurrent: null);
        }

        var marked = new DesktopNodeHyperVCheckpointInfo[rows.Count];
        for (var i = 0; i < rows.Count; i++)
        {
            marked[i] = rows[i] with { IsCurrent = i == matchIndex };
        }

        return marked;
    }

    private static IReadOnlyList<DesktopNodeHyperVCheckpointInfo> CopyWithCurrent(
        IReadOnlyList<DesktopNodeHyperVCheckpointInfo> rows,
        bool? isCurrent)
    {
        var copies = new DesktopNodeHyperVCheckpointInfo[rows.Count];
        for (var i = 0; i < rows.Count; i++)
        {
            copies[i] = rows[i] with { IsCurrent = isCurrent };
        }

        return copies;
    }

    private static string? TryReadCurrentSnapshotInstanceId(ManagementObject vm, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var currentSnapshots = vm.GetRelated(
                SnapshotSettingClass,
                CurrentSnapshotAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "Dependent",
                thisRole: "Antecedent",
                classDefinitionsOnly: false,
                options: null);

            string? found = null;
            var count = 0;
            foreach (ManagementObject item in currentSnapshots)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (item)
                {
                    count++;
                    found = GetStringProperty(item, "InstanceID");
                }
            }

            return count == 1 ? found : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

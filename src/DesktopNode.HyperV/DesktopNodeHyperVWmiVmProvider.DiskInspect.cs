namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider
{
    // disk-resize 의 축소 방지와 같은 GetVirtualHardDiskSettingData(MaxInternalSize) 조회다.
    public ulong? GetVirtualDiskMaxInternalSize(string diskPath, CancellationToken cancellationToken) =>
        new DesktopNodeWmiVirtualDiskOperations().GetMaxInternalSize(diskPath, cancellationToken);
}

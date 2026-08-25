using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVWmiVmResourceMutationProviderTests
{
    private const ulong Gibibyte = 1024UL * 1024UL * 1024UL;

    [Fact]
    public void ResizeDiskRejectsShrinkBeforeCallingWmiResize()
    {
        var operations = new FakeVirtualDiskOperations(100UL * Gibibyte);
        var provider = new DesktopNodeHyperVWmiVmResourceMutationProvider(operations);
        var request = CreateResizeRequest(8);

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => provider.Invoke(request, CancellationToken.None));

        Assert.Equal("PCV_VM_DISK_SHRINK_NOT_SUPPORTED", error.Code);
        Assert.Equal(0, operations.ResizeCallCount);
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(8, 12)]
    public void ResizeDiskAllowsEqualOrLargerTarget(int currentDiskGb, int requestedDiskGb)
    {
        var operations = new FakeVirtualDiskOperations((ulong)currentDiskGb * Gibibyte);
        var provider = new DesktopNodeHyperVWmiVmResourceMutationProvider(operations);
        var request = CreateResizeRequest(requestedDiskGb);

        var result = provider.Invoke(request, CancellationToken.None);

        Assert.Equal("disk-resize", result.Action);
        Assert.Equal(1, operations.ResizeCallCount);
        Assert.Equal((ulong)requestedDiskGb * Gibibyte, operations.LastRequestedBytes);
    }

    private static DesktopNodeHyperVVmResourceMutationRequest CreateResizeRequest(int diskGb)
    {
        return new DesktopNodeHyperVVmResourceMutationRequest(
            Operation: "vm.disk-resize",
            Name: "pcv-test-vm",
            MemoryMb: null,
            Cpu: null,
            DiskGb: diskGb,
            DiskPath: @"C:\pcv-test\disk.vhdx");
    }

    private sealed class FakeVirtualDiskOperations(ulong maxInternalSize) : IDesktopNodeVirtualDiskOperations
    {
        public int ResizeCallCount { get; private set; }

        public ulong? LastRequestedBytes { get; private set; }

        public ulong GetMaxInternalSize(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return maxInternalSize;
        }

        public void Resize(string path, ulong requestedBytes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ResizeCallCount++;
            LastRequestedBytes = requestedBytes;
        }
    }
}

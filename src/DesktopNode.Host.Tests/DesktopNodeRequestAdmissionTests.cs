using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeRequestAdmissionTests
{
    [Fact]
    public async Task ActiveAndWaitingLimitsRejectAdditionalRequests()
    {
        using var admission = new DesktopNodeRequestAdmission(activeLimit: 1, waitingLimit: 1);
        using var active = await admission.TryEnterAsync(CancellationToken.None);
        Assert.NotNull(active);
        Assert.Equal(1, admission.ActiveCount);

        var waitingTask = admission.TryEnterAsync(CancellationToken.None).AsTask();
        var waitingDeadline = DateTimeOffset.UtcNow.AddSeconds(2);
        while (admission.WaitingCount != 1 && DateTimeOffset.UtcNow < waitingDeadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal(1, admission.WaitingCount);
        Assert.Null(await admission.TryEnterAsync(CancellationToken.None));

        active!.Dispose();
        using var released = await waitingTask;
        Assert.NotNull(released);
        Assert.Equal(1, admission.ActiveCount);
    }

    [Fact]
    public async Task CanceledWaitingRequestDoesNotLeakWaitingCapacity()
    {
        using var admission = new DesktopNodeRequestAdmission(activeLimit: 1, waitingLimit: 1);
        using var active = await admission.TryEnterAsync(CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        var waitingTask = admission.TryEnterAsync(cancellation.Token).AsTask();

        var waitingDeadline = DateTimeOffset.UtcNow.AddSeconds(2);
        while (admission.WaitingCount != 1 && DateTimeOffset.UtcNow < waitingDeadline)
        {
            await Task.Delay(10);
        }

        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await waitingTask);
        Assert.Equal(0, admission.WaitingCount);
    }
}

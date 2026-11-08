namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private static readonly TimeSpan VmListSummaryParityRetryDelay = TimeSpan.FromMilliseconds(400);

    private Func<TimeSpan, CancellationToken, bool> vmListSummaryParityDelay =
        static (span, token) => token.WaitHandle.WaitOne(span);

    internal void UseVmListSummaryParityDelayForTests(Func<TimeSpan, CancellationToken, bool> delay)
    {
        ArgumentNullException.ThrowIfNull(delay);
        vmListSummaryParityDelay = delay;
    }

    // 삭제 직후 summary parity만 잠시 비는 경우 한 번 다시 읽는다. identity가 비면 재시도하지 않는다.
    private bool TryRereadVmListAfterSummaryParityGap(
        string operation,
        CancellationToken cancellationToken,
        out IReadOnlyList<DesktopNodeHyperVVmInfo> vms,
        out DesktopNodeHyperVOperationResult? failure)
    {
        vms = [];
        failure = null;
        if (vmListSummaryParityDelay(VmListSummaryParityRetryDelay, cancellationToken) ||
            cancellationToken.IsCancellationRequested)
        {
            failure = CanceledResult(operation);
            return false;
        }

        vms = vmProvider.GetVms(cancellationToken);
        if (!HasCompleteVmIdentityState(vms))
        {
            failure = VmListIdentityIncomplete(operation);
            return false;
        }

        if (!HasCompleteVmSummaryParity(vms))
        {
            failure = VmListSummaryParityIncomplete(operation);
            return false;
        }

        return true;
    }

    private static DesktopNodeHyperVOperationResult VmListIdentityIncomplete(string operation)
    {
        return DesktopNodeHyperVOperationResult.Failure(
            operation,
            "PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE",
            "Native Hyper-V VM inventory identity/state is incomplete.",
            "The native adapter could not preserve VM id, name, and state for every row.",
            false);
    }

    private static DesktopNodeHyperVOperationResult VmListSummaryParityIncomplete(string operation)
    {
        return DesktopNodeHyperVOperationResult.Failure(
            operation,
            "PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE",
            "Native Hyper-V VM inventory summary parity is incomplete.",
            "The native adapter could not preserve VM platform, CPU, startup memory, generation, checkpoint, or console summary fields for every row.",
            false);
    }
}

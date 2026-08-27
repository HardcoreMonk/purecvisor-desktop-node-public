using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

// InvokeHyperVOperation은 라우팅 경로, 큐잉된 뮤테이션 경로, 재조정(reconciliation) 경로,
// 그리고 워커에서 각각 호출된다. 별도의 소유자가 없으면 이 네 도메인 모두가 다시
// DesktopNodeApiRequestProcessor로 되돌아와야 했을 것이다.
internal sealed class DesktopNodeApiHyperVOperationInvoker
{
    private readonly IDesktopNodeHyperVNativeAdapter nativeAdapter;

    public DesktopNodeApiHyperVOperationInvoker(IDesktopNodeHyperVNativeAdapter nativeAdapter)
    {
        this.nativeAdapter = nativeAdapter;
    }

    public DesktopNodeHyperVOperationResult Invoke(
        string operation,
        JsonElement parameters,
        CancellationToken cancellationToken = default)
    {
        if (IsNativeOperationCandidate(operation))
        {
            nativeAdapter.TryInvoke(operation, parameters, cancellationToken, out var result);
            return result;
        }

        return DesktopNodeHyperVOperationResult.Failure(
            operation,
            "PCV_NATIVE_OPERATION_NOT_SUPPORTED",
            "The product runtime does not support this operation.",
            "The active product runtime is C#/.NET native-only and does not dispatch to a PowerShell fallback process.",
            retryable: false);
    }

    internal static bool IsNativeOperationCandidate(string operation)
    {
        return operation is "host.status" or
            "network.inventory" or
            "vm.list" or
            "vm.memory-stats" or
            "vm.cpu-stats" or
            "vm.blkio-get" or
            "vm.bandwidth" or
            "vm.qos.storage.preview" or
            "vm.qos.network.preview" or
            "vm.guest-agent-status" or
            "vm.guest-ping" or
            "vm.guest.exec" or
            "vm.guest.channel.verify" or
            "vm.guest.channel.ensure" or
            "checkpoint.list" or
            "vm.create" or
            "vm.start" or
            "vm.shutdown" or
            "vm.poweroff" or
            "vm.restart" or
            "vm.pause" or
            "vm.resume" or
            "vm.save" or
            "vm.resume-saved" or
            "vm.rename" or
            "vm.manage" or
            "vm.clone.preview" or
            "vm.clone" or
            "vm.eject" or
            "vm.attach" or
            "vm.limit" or
            "vm.qos.storage.set" or
            "vm.qos.network.set" or
            "vm.set-memory" or
            "vm.set-vcpu" or
            "vm.disk-resize" or
            "vm.delete" or
            "checkpoint.create" or
            "checkpoint.restore" or
            "checkpoint.delete";
    }
}

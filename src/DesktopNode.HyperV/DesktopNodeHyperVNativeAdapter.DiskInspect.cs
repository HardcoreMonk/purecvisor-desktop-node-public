using System.Text.Json;

namespace DesktopNode.HyperV;

// vm.disk.inspect 는 공개 route 가 없는 내부 read operation 이다. reconcile 이 disk-resize 결과를 확인할 때 쓴다.
// path 는 그 VM 의 vm.list storage 에 있는 VHD/VHDX 여야 해서, 임의 host 파일을 조회하는 통로가 되지 않는다.
public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeVmDiskInspect(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "name") ?? GetStringProperty(parameters, "vm_name");
        var diskPath = GetStringProperty(parameters, "path");
        if (string.IsNullOrWhiteSpace(vmName) || string.IsNullOrWhiteSpace(diskPath))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_DISK_INSPECT_INPUT_REQUIRED",
                "VM disk inspect requires name and path.",
                "Pass the VM name and a VHD/VHDX path listed in that VM's vm.list storage.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var vm = FindVm(vmProvider.GetVms(cancellationToken), vmName);
            if (vm is null)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_NOT_FOUND",
                    $"VM '{vmName}' was not found.",
                    "The VM was not present in the native Hyper-V VM inventory response.",
                    false);
                return true;
            }

            if (!vm.Storage.Any(disk => string.Equals(disk.Path, diskPath, StringComparison.OrdinalIgnoreCase)))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_DISK_NOT_FOUND",
                    $"VM '{vmName}' has no disk '{diskPath}' in inventory.",
                    "Inspect only a VHD/VHDX path listed in the VM's vm.list storage.",
                    false);
                return true;
            }

            var bytes = vmProvider.GetVirtualDiskMaxInternalSize(diskPath, cancellationToken);
            if (bytes is null)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_DISK_INSPECT_UNAVAILABLE",
                    "The VM disk size could not be read.",
                    "The VM provider does not report virtual disk size.",
                    false);
                return true;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(
                    new SortedDictionary<string, object?>
                    {
                        ["name"] = vm.Name,
                        ["path"] = diskPath,
                        ["max_internal_size_bytes"] = bytes.Value
                    },
                    JsonOptions),
                Error: null);
            return true;
        }
        catch (DesktopNodeHyperVNativeOperationException ex)
        {
            result = DesktopNodeHyperVOperationResult.Failure(operation, ex.Code, ex.Message, ex.Detail, ex.Retryable);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = CanceledResult(operation);
            return true;
        }
        catch (Exception ex)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_DISK_INSPECT_FAILED",
                "VM disk inspect failed.",
                ex.Message,
                false);
            return true;
        }
    }
}

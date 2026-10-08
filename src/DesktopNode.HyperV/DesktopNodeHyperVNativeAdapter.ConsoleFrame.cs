using System.Text.Json;

namespace DesktopNode.HyperV;

// vm.console.frame 은 S1 브라우저 콘솔의 화면 읽기다(설계 pcv-s1-browser-console-v1 §2).
// GetVirtualSystemThumbnailImage 가 주는 RGB565 원본을 width×height×2 byte 로 잘라 base64 로 돌려준다. 이미지 변환은 브라우저가 한다.
public sealed partial class DesktopNodeHyperVNativeAdapter
{
    internal const int ConsoleFrameDefaultWidth = 640;
    internal const int ConsoleFrameDefaultHeight = 480;
    private const int ConsoleFrameMinWidth = 160;
    private const int ConsoleFrameMaxWidth = 1024;
    private const int ConsoleFrameMinHeight = 120;
    private const int ConsoleFrameMaxHeight = 768;

    private bool TryInvokeVmConsoleFrame(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "name") ?? GetStringProperty(parameters, "vm_name");
        var width = GetIntProperty(parameters, "width") ?? ConsoleFrameDefaultWidth;
        var height = GetIntProperty(parameters, "height") ?? ConsoleFrameDefaultHeight;
        if (string.IsNullOrWhiteSpace(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation, "PCV_CONSOLE_FRAME_INPUT_REQUIRED", "VM console frame requires name.", "Pass the VM name.", false);
            return true;
        }

        if (width is < ConsoleFrameMinWidth or > ConsoleFrameMaxWidth || height is < ConsoleFrameMinHeight or > ConsoleFrameMaxHeight)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CONSOLE_FRAME_SIZE_INVALID",
                $"Console frame size {width}x{height} is outside the supported range.",
                $"Use width {ConsoleFrameMinWidth}-{ConsoleFrameMaxWidth} and height {ConsoleFrameMinHeight}-{ConsoleFrameMaxHeight}.",
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

            if (!string.Equals(vm.State, "running", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_CONSOLE_VM_NOT_RUNNING",
                    $"VM '{vm.Name}' is {vm.State}, not running.",
                    "Start the VM before reading its console frame.",
                    false);
                return true;
            }

            var expected = width * height * 2;
            var frame = vmProvider.GetConsoleFrame(vm.Id, width, height, cancellationToken);
            if (frame is null || frame.Length < expected)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_CONSOLE_FRAME_FAILED",
                    "The VM console frame could not be read.",
                    frame is null ? "The VM provider returned no thumbnail image." : $"The thumbnail image had {frame.Length} bytes; expected {expected}.",
                    true);
                return true;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(
                    new SortedDictionary<string, object?>
                    {
                        ["name"] = vm.Name,
                        ["width"] = width,
                        ["height"] = height,
                        ["format"] = "rgb565le",
                        ["byte_length"] = expected,
                        ["frame_base64"] = Convert.ToBase64String(frame, 0, expected)
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
                operation, "PCV_CONSOLE_FRAME_FAILED", "The VM console frame could not be read.", ex.Message, true);
            return true;
        }
    }

    private static int? GetIntProperty(JsonElement parameters, string name) =>
        parameters.ValueKind == JsonValueKind.Object &&
        parameters.TryGetProperty(name, out var value) &&
        value.ValueKind == JsonValueKind.Number &&
        value.TryGetInt32(out var number)
            ? number
            : null;
}

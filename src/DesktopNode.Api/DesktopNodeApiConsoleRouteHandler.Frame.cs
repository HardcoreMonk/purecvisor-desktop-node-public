using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

// S1 브라우저 콘솔의 화면 route(설계 pcv-s1-browser-console-v1 §2). host 가 query string 을 넘기지 않으므로 크기는
// 경로의 {size}(예: 640x480)로 받는다. RGB565 원본을 zlib(Compression Streams 'deflate')으로 압축해 base64 로 낸다.
internal sealed partial class DesktopNodeApiConsoleRouteHandler
{
    internal const int FrameMinIntervalMilliseconds = 100;

    private static readonly Regex FrameSize = new("^([0-9]{3,4})x([0-9]{3,4})$", RegexOptions.CultureInvariant);

    private readonly ConcurrentDictionary<string, long> lastFrameTicks = new(StringComparer.OrdinalIgnoreCase);

    private DesktopNodeApiResponse HandleVmConsoleFrame(string encodedVmId, string encodedSize)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, "console.frame");
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var size = FrameSize.Match(Uri.UnescapeDataString(encodedSize));
        if (!size.Success)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "console.frame",
                "PCV_CONSOLE_FRAME_SIZE_INVALID",
                "Console frame size must look like 640x480.",
                "Use /api/v1/vms/{vmId}/console/frame/{width}x{height} with width 160-1024 and height 120-768.",
                false);
        }

        var now = Environment.TickCount64;
        var vm = routeId.Value!;
        if (lastFrameTicks.TryGetValue(vm, out var last) && now - last < FrameMinIntervalMilliseconds)
        {
            return DesktopNodeApiResponseFactory.Failure(
                429,
                "console.frame",
                "PCV_CONSOLE_RATE_LIMITED",
                "Console frames for this VM are requested too often.",
                $"Wait at least {FrameMinIntervalMilliseconds} ms between frames of the same VM.",
                true);
        }

        lastFrameTicks[vm] = now;
        var result = operationInvoker.Invoke(
            "vm.console.frame",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["vm_name"] = vm,
                ["width"] = int.Parse(size.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                ["height"] = int.Parse(size.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)
            }));
        if (!result.Ok || result.Data is not { } data)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(result);
        }

        var raw = Convert.FromBase64String(data.GetProperty("frame_base64").GetString() ?? string.Empty);
        using var buffer = new MemoryStream();
        using (var zlib = new ZLibStream(buffer, CompressionLevel.Fastest, leaveOpen: true))
        {
            zlib.Write(raw);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "console.frame", new SortedDictionary<string, object?>
        {
            ["vm"] = data.GetProperty("name").GetString(),
            ["width"] = data.GetProperty("width").GetInt32(),
            ["height"] = data.GetProperty("height").GetInt32(),
            ["format"] = "rgb565le",
            ["encoding"] = "deflate",
            ["byte_length"] = raw.Length,
            ["frame_base64"] = Convert.ToBase64String(buffer.ToArray()),
            ["captured_at"] = DateTimeOffset.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture)
        }, null));
    }
}

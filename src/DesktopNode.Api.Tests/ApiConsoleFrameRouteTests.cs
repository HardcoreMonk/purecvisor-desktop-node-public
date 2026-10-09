using System.IO.Compression;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

// 설계 pcv-s1-browser-console-v1 §2: 크기는 경로로 받고, 화면은 zlib 으로 압축한 RGB565 다.
public sealed class ApiConsoleFrameRouteTests
{
    [Fact]
    public void FrameRouteCompressesTheAdapterFrame()
    {
        var calls = new List<JsonElement>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: new FrameNativeAdapter(calls));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET", "/api/v1/vms/lab-vm/console/frame/160x120", ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        var call = Assert.Single(calls);
        Assert.Equal("lab-vm", call.GetProperty("vm_name").GetString());
        Assert.Equal(160, call.GetProperty("width").GetInt32());
        Assert.Equal(120, call.GetProperty("height").GetInt32());
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("console.frame", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("deflate", data.GetProperty("encoding").GetString());
        Assert.Equal(160 * 120 * 2, data.GetProperty("byte_length").GetInt32());
        using var compressed = new MemoryStream(Convert.FromBase64String(data.GetProperty("frame_base64").GetString()!));
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        zlib.CopyTo(raw);
        Assert.Equal(160 * 120 * 2, raw.Length);
        Assert.Equal(0x1F, raw.ToArray()[0]);
    }

    [Theory]
    [InlineData("/api/v1/vms/lab-vm/console/frame/large", 400, "PCV_CONSOLE_FRAME_SIZE_INVALID")]
    [InlineData("/api/v1/vms/stopped-vm/console/frame/640x480", 409, "PCV_CONSOLE_VM_NOT_RUNNING")]
    [InlineData("/api/v1/vms/missing-vm/console/frame/640x480", 404, "PCV_VM_NOT_FOUND")]
    public void FrameRouteMapsSizeAndAdapterErrors(string path, int status, string code)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: new FrameNativeAdapter([]));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", path, ServiceBearerAccepted: true));

        Assert.Equal(status, response.StatusCode);
        Assert.Contains(code, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void FrameRouteLimitsTheSameVmTo100Milliseconds()
    {
        // throttle 은 hardening options 의 clock 으로 잰다. 고정 clock 이므로 느린 runner 에서도 결과가 같다.
        var now = new DateTimeOffset(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
        var calls = new List<JsonElement>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new FrameNativeAdapter(calls),
            hardeningOptions: new DesktopNodeApiHardeningOptions(Clock: () => now));
        var request = new DesktopNodeApiRequest("GET", "/api/v1/vms/lab-vm/console/frame/160x120", ServiceBearerAccepted: true);

        var first = processor.Handle(request);
        var second = processor.Handle(request);
        now = now.AddMilliseconds(DesktopNodeApiConsoleRouteHandler.FrameMinIntervalMilliseconds - 1);
        var third = processor.Handle(request);
        now = now.AddMilliseconds(1);
        var fourth = processor.Handle(request);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(429, second.StatusCode);
        Assert.Contains("PCV_CONSOLE_RATE_LIMITED", second.Body, StringComparison.Ordinal);
        Assert.Equal(429, third.StatusCode);
        Assert.Equal(200, fourth.StatusCode);
        Assert.Equal(2, calls.Count);
    }

    private sealed class FrameNativeAdapter(List<JsonElement> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            if (operation != "vm.console.frame")
            {
                result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_NATIVE_ROUTE_NOT_HANDLED", "not handled", "test", false);
                return false;
            }

            calls.Add(parameters.Clone());
            var vm = parameters.GetProperty("vm_name").GetString();
            if (vm == "stopped-vm")
            {
                result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_CONSOLE_VM_NOT_RUNNING", "stopped", "test", false);
                return true;
            }

            if (vm == "missing-vm")
            {
                result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_VM_NOT_FOUND", "missing", "test", false);
                return true;
            }

            var width = parameters.GetProperty("width").GetInt32();
            var height = parameters.GetProperty("height").GetInt32();
            var frame = new byte[width * height * 2];
            frame[0] = 0x1F;
            result = new DesktopNodeHyperVOperationResult(
                true,
                operation,
                JsonSerializer.SerializeToElement(new Dictionary<string, object?>
                {
                    ["name"] = vm,
                    ["width"] = width,
                    ["height"] = height,
                    ["format"] = "rgb565le",
                    ["byte_length"] = frame.Length,
                    ["frame_base64"] = Convert.ToBase64String(frame)
                }),
                null);
            return true;
        }
    }
}

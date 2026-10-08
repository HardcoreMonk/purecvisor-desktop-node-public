using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

// 설계 pcv-s1-browser-console-v1 §3~§5: 입력은 동기 처리이고, 보내기 전에 내용 없는 audit 한 줄을 남긴다.
public sealed class ApiConsoleInputRouteTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("pcv-console-input-").FullName;

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public void TextInputIsSentAndAuditedWithoutItsContent()
    {
        var calls = new List<JsonElement>();
        var processor = CreateProcessor(calls);

        var response = Send(processor, "/api/v1/vms/lab-vm/console/input", """{"kind":"text","text":"secret-pass"}""");

        Assert.Equal(200, response.StatusCode);
        var call = Assert.Single(calls);
        Assert.Equal("secret-pass", call.GetProperty("text").GetString());
        using var document = JsonDocument.Parse(response.Body);
        Assert.True(document.RootElement.GetProperty("data").GetProperty("accepted").GetBoolean());
        Assert.DoesNotContain("secret-pass", response.Body, StringComparison.Ordinal);
        var audit = File.ReadAllText(Path.Combine(root, DesktopNodeApiConsoleRouteHandler.ConsoleInputAuditFileName));
        Assert.DoesNotContain("secret-pass", audit, StringComparison.Ordinal);
        using var line = JsonDocument.Parse(audit.Trim());
        Assert.Equal(11, line.RootElement.GetProperty("text_length").GetInt32());
        Assert.Equal("loopback", line.RootElement.GetProperty("origin").GetString());
        Assert.Equal("text", line.RootElement.GetProperty("kind").GetString());
    }

    [Theory]
    [InlineData("""{"kind":"key","action":"type","key_code":0}""")]
    [InlineData("""{"kind":"key","action":"hold","key_code":13}""")]
    [InlineData("""{"kind":"text","text":""}""")]
    [InlineData("""{"kind":"text","text":"line\nbreak"}""")]
    [InlineData("""{"kind":"mouse"}""")]
    public void InvalidInputIsRejectedBeforeAuditAndAdapter(string body)
    {
        var calls = new List<JsonElement>();
        var processor = CreateProcessor(calls);

        var response = Send(processor, "/api/v1/vms/lab-vm/console/input", body);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("PCV_CONSOLE_INPUT_INVALID", response.Body, StringComparison.Ordinal);
        Assert.Empty(calls);
        Assert.False(File.Exists(Path.Combine(root, DesktopNodeApiConsoleRouteHandler.ConsoleInputAuditFileName)));
    }

    [Fact]
    public void RemoteServiceBearerCannotSendInput()
    {
        var calls = new List<JsonElement>();
        var processor = CreateProcessor(calls);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST", "/api/v1/vms/lab-vm/console/input", """{"kind":"ctrl-alt-del"}""", ServiceBearerAccepted: true, RemoteIsLoopback: false));

        Assert.Equal(403, response.StatusCode);
        Assert.Contains("PCV_CONSOLE_INPUT_ACCOUNT_REQUIRED", response.Body, StringComparison.Ordinal);
        Assert.Empty(calls);
    }

    [Fact]
    public void InputWithoutAuditPathIsNotSent()
    {
        var calls = new List<JsonElement>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: new InputNativeAdapter(calls));

        var response = Send(processor, "/api/v1/vms/lab-vm/console/input", """{"kind":"key","action":"type","key_code":13}""");

        Assert.Equal(503, response.StatusCode);
        Assert.Contains("PCV_CONSOLE_AUDIT_UNAVAILABLE", response.Body, StringComparison.Ordinal);
        Assert.Empty(calls);
    }

    [Fact]
    public void InputIsLimitedToFiftyPerSecondPerVm()
    {
        var calls = new List<JsonElement>();
        var processor = CreateProcessor(calls);

        var statuses = Enumerable.Range(0, 51)
            .Select(_ => Send(processor, "/api/v1/vms/lab-vm/console/input", """{"kind":"key","action":"press","key_code":16}""").StatusCode)
            .ToList();

        Assert.Equal(50, statuses.Count(status => status == 200));
        Assert.Equal(429, statuses[^1]);
        Assert.Equal(50, calls.Count);
    }

    private DesktopNodeApiRequestProcessor CreateProcessor(List<JsonElement> calls) =>
        DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new InputNativeAdapter(calls),
            consoleOptions: new DesktopNodeConsoleOptions(NoVncTargetFilePath: Path.Combine(root, "novnc-target.json")));

    private static DesktopNodeApiResponse Send(DesktopNodeApiRequestProcessor processor, string path, string body) =>
        processor.Handle(new DesktopNodeApiRequest("POST", path, body, ServiceBearerAccepted: true, RemoteIsLoopback: true));

    private sealed class InputNativeAdapter(List<JsonElement> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            if (operation != "vm.console.input")
            {
                result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_NATIVE_ROUTE_NOT_HANDLED", "not handled", "test", false);
                return false;
            }

            calls.Add(parameters.Clone());
            result = new DesktopNodeHyperVOperationResult(
                true,
                operation,
                JsonSerializer.SerializeToElement(new Dictionary<string, object?>
                {
                    ["name"] = parameters.GetProperty("vm_name").GetString(),
                    ["kind"] = parameters.GetProperty("kind").GetString(),
                    ["action"] = parameters.GetProperty("action").GetString(),
                    ["accepted"] = true
                }),
                null);
            return true;
        }
    }
}

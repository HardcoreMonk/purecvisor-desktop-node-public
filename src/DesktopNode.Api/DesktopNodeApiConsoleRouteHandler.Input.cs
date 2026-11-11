using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

// S1 브라우저 콘솔의 키보드 입력 route(설계 pcv-s1-browser-console-v1 §3~§5). job 이 아니라 동기 처리다.
// 보내기 전에 data root 의 console-input-audit.jsonl 에 한 줄을 남기고, 못 남기면 보내지 않는다. text 내용과 hash 는 남기지 않는다.
internal sealed partial class DesktopNodeApiConsoleRouteHandler
{
    internal const string ConsoleInputAuditFileName = "console-input-audit.jsonl";
    internal const string ConsoleInputAuditSchema = "pcv-console-input-audit/v1";
    internal const long ConsoleInputAuditMaxBytes = 1024 * 1024;
    internal const int ConsoleInputMaxPerSecond = 50;

    private static readonly object ConsoleInputAuditLock = new();
    private readonly ConcurrentDictionary<string, (long WindowStart, int Count)> inputWindows = new(StringComparer.OrdinalIgnoreCase);

    internal string? ConsoleInputAuditPath =>
        noVncTargetStore.FilePath is { } path && Path.GetDirectoryName(path) is { Length: > 0 } directory
            ? Path.Combine(directory, ConsoleInputAuditFileName)
            : null;

    private DesktopNodeApiResponse HandleVmConsoleInput(DesktopNodeApiRequest request, string encodedVmId, CancellationToken cancellationToken = default)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, "console.input");
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var body = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "console.input");
        if (!body.Ok)
        {
            return body.Response!;
        }

        var json = body.Value!.Value;
        var kind = ReadString(json, "kind");
        var action = ReadString(json, "action") ?? (kind == "key" ? null : "type");
        int? keyCode = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("key_code", out var code) && code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var number) ? number : null;
        var text = ReadString(json, "text");
        var plan = DesktopNodeHyperVNativeAdapter.PlanConsoleInput(kind, action, keyCode, text);
        if (plan is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "console.input",
                "PCV_CONSOLE_INPUT_INVALID",
                "Console input is not valid.",
                "Send kind=key with action type|press|release and key_code 1-254, kind=text with 1-256 printable ASCII characters, or kind=ctrl-alt-del.",
                false);
        }

        if (!request.RemoteIsLoopback && request.ServiceBearerAccepted)
        {
            return DesktopNodeApiResponseFactory.Failure(
                403,
                "console.input",
                "PCV_CONSOLE_INPUT_ACCOUNT_REQUIRED",
                "Remote console input needs an account session.",
                "Sign in with an account that has console.input; the service bearer sends console input only from loopback.",
                false);
        }

        var vm = routeId.Value!;
        if (!TryTakeInputSlot(vm))
        {
            return DesktopNodeApiResponseFactory.Failure(
                429,
                "console.input",
                "PCV_CONSOLE_RATE_LIMITED",
                "Console input for this VM is sent too often.",
                $"Send at most {ConsoleInputMaxPerSecond} inputs per second to the same VM.",
                true);
        }

        if (!TryWriteConsoleInputAudit(request, vm, plan.Value.Kind, plan.Value.Action, keyCode, text))
        {
            return DesktopNodeApiResponseFactory.Failure(
                503,
                "console.input",
                "PCV_CONSOLE_AUDIT_UNAVAILABLE",
                "Console input audit could not be written.",
                "The input was not sent. Check that the service data root is writable.",
                true);
        }

        var result = operationInvoker.Invoke(
            "vm.console.input",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["vm_name"] = vm,
                ["kind"] = plan.Value.Kind,
                ["action"] = plan.Value.Action,
                ["key_code"] = keyCode,
                ["text"] = text
            }),
            cancellationToken);
        if (!result.Ok || result.Data is not { } data)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(result);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "console.input", new SortedDictionary<string, object?>
        {
            ["vm"] = data.GetProperty("name").GetString(),
            ["kind"] = plan.Value.Kind,
            ["action"] = plan.Value.Action,
            ["accepted"] = true
        }, null));
    }

    private bool TryTakeInputSlot(string vm)
    {
        // frame throttle 과 같이 hardening options 의 clock(ms since epoch)으로 1초 창을 잰다. 시험은 clock 을 고정해
        // limiter 를 결정적으로 검증하고, 실제 서비스는 UtcNow 를 쓴다.
        // AddOrUpdate 의 갱신 함수는 두 번 돌 수 있어서, 바깥 변수에 허용 여부를 적으면 동시에 한도를 넘긴다.
        var now = hardeningOptions.Now().ToUnixTimeMilliseconds();
        while (true)
        {
            if (!inputWindows.TryGetValue(vm, out var window))
            {
                if (inputWindows.TryAdd(vm, (now, 1)))
                {
                    return true;
                }

                continue;
            }

            (long WindowStart, int Count) next;
            if (now - window.WindowStart >= 1000)
            {
                next = (now, 1);
            }
            else if (window.Count < ConsoleInputMaxPerSecond)
            {
                next = (window.WindowStart, window.Count + 1);
            }
            else if (inputWindows.TryUpdate(vm, window, window))
            {
                return false;
            }
            else
            {
                continue;
            }

            if (inputWindows.TryUpdate(vm, next, window))
            {
                return true;
            }
        }
    }

    private bool TryWriteConsoleInputAudit(DesktopNodeApiRequest request, string vm, string kind, string action, int? keyCode, string? text)
    {
        var path = ConsoleInputAuditPath;
        if (path is null)
        {
            return false;
        }

        var line = JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["schema"] = ConsoleInputAuditSchema,
            ["ts"] = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            ["request_id"] = request.RequestId,
            ["principal"] = authSessionHandler.ResolveActor(request),
            ["origin"] = request.RemoteIsLoopback ? "loopback" : "remote",
            ["vm"] = vm,
            ["kind"] = kind,
            ["action"] = action,
            ["key_code"] = kind == "key" ? keyCode : null,
            ["text_length"] = kind == "text" ? text?.Length : null
        });
        try
        {
            lock (ConsoleInputAuditLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                if (File.Exists(path) && new FileInfo(path).Length > ConsoleInputAuditMaxBytes)
                {
                    File.Move(path, Path.ChangeExtension(path, ".1.jsonl"), overwrite: true);
                }

                File.AppendAllText(path, line + "\n");
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static string? ReadString(JsonElement json, string name) =>
        json.ValueKind == JsonValueKind.Object && json.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

using DesktopNode.Contracts;

namespace DesktopNode.Api;

// console 경로가 지금은 Func 두 개를 processor 로 되돌려 보내는
// callback adapter 로 dispatch 된다. wave 1 이 diagnostics/auth/ops 에서 없앤 그 형태다.
// 이 소유자가 라우팅과 구현을 함께 갖는다.
internal sealed class DesktopNodeApiConsoleRouteHandler
{
    private readonly DesktopNodeConsoleOptions consoleOptions;

    public DesktopNodeApiConsoleRouteHandler(DesktopNodeConsoleOptions consoleOptions)
    {
        this.consoleOptions = consoleOptions;
    }

    public DesktopNodeApiResponse? TryHandle(string method, string normalizedPath)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "GetConsoleCapabilities", out _))
        {
            return HandleConsoleCapabilities();
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "GetVmConsoleSession", out var consoleMatch))
        {
            return HandleVmConsoleSession(consoleMatch.Parameters["vmId"]);
        }

        return null;
    }

    public RuntimePolicyConsolePolicy CreateRuntimePolicy()
    {
        return new RuntimePolicyConsolePolicy(
            Mode: consoleOptions.Enabled ? "windows-hyperv-console-handoff" : "disabled",
            WindowsConsole: "vmconnect",
            NoVnc: consoleOptions.NoVncEnabled ? "available" : "not_configured",
            Transport: consoleOptions.NoVncEnabled ? "websocket-vnc-bridge" : "local-handoff");
    }

    private DesktopNodeApiResponse HandleConsoleCapabilities()
    {
        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "console.capabilities", BuildConsoleCapabilities(), null));
    }

    private DesktopNodeApiResponse HandleVmConsoleSession(string encodedVmId)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, "console.session");
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "console.session", BuildVmConsoleSession(routeId.Value!), null));
    }

    private object BuildConsoleCapabilities()
    {
        return new SortedDictionary<string, object?>
        {
            ["actual_execution"] = "capability-read",
            ["console_access"] = DesktopNodeApiConsoleAccessProjection.ForCapabilities(consoleOptions),
            ["host_mutation_performed"] = false,
            ["novnc"] = new SortedDictionary<string, object?>
            {
                ["enabled"] = consoleOptions.NoVncEnabled,
                ["status"] = consoleOptions.NoVncEnabled ? "available" : "not_configured",
                ["bridge_mode"] = consoleOptions.NoVncEnabled ? consoleOptions.NoVncBridgeMode : "disabled",
                ["transport"] = consoleOptions.NoVncEnabled ? "websocket-vnc-bridge" : "none",
                ["websocket_path_template"] = consoleOptions.NoVncEnabled ? consoleOptions.NoVncWebSocketPath : null,
                ["reason"] = consoleOptions.NoVncEnabled ? null : "No Windows VNC/WebSocket bridge is configured for this listener."
            },
            ["operation"] = "console.capabilities",
            ["windows_console"] = new SortedDictionary<string, object?>
            {
                ["available_local"] = consoleOptions.Enabled,
                ["launch_mode"] = "operator-local-handoff",
                ["type"] = "vmconnect"
            }
        };
    }

    private object BuildVmConsoleSession(string vmId)
    {
        var noVncWebSocketPath = FormatNoVncWebSocketPath(vmId);
        return new SortedDictionary<string, object?>
        {
            ["actual_execution"] = "capability-read",
            ["console"] = new SortedDictionary<string, object?>
            {
                ["launch_hint"] = "Use the local Hyper-V vmconnect handoff until a noVNC bridge is configured.",
                ["transport"] = consoleOptions.NoVncEnabled ? "websocket-vnc-bridge" : "vmconnect-handoff",
                ["type"] = "vmconnect"
            },
            ["console_access"] = DesktopNodeApiConsoleAccessProjection.ForSession(consoleOptions, noVncWebSocketPath),
            ["host_mutation_performed"] = false,
            ["novnc"] = new SortedDictionary<string, object?>
            {
                ["enabled"] = consoleOptions.NoVncEnabled,
                ["status"] = consoleOptions.NoVncEnabled ? "available" : "not_configured",
                ["bridge_mode"] = consoleOptions.NoVncEnabled ? consoleOptions.NoVncBridgeMode : "disabled",
                ["websocket_path"] = consoleOptions.NoVncEnabled ? noVncWebSocketPath : null
            },
            ["vm_id"] = vmId
        };
    }

    private string? FormatNoVncWebSocketPath(string vmId)
    {
        if (string.IsNullOrWhiteSpace(consoleOptions.NoVncWebSocketPath))
        {
            return null;
        }

        return consoleOptions.NoVncWebSocketPath.Replace(
            "{vm_id}",
            Uri.EscapeDataString(vmId),
            StringComparison.OrdinalIgnoreCase);
    }
}

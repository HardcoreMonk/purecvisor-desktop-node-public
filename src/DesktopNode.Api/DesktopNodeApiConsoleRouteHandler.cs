using DesktopNode.Contracts;

namespace DesktopNode.Api;

// console 경로가 지금은 Func 두 개를 processor 로 되돌려 보내는
// callback adapter 로 dispatch 된다. wave 1 이 diagnostics/auth/ops 에서 없앤 그 형태다.
// 이 소유자가 라우팅과 구현을 함께 갖는다.
internal sealed class DesktopNodeApiConsoleRouteHandler
{
    private readonly DesktopNodeConsoleOptions consoleOptions;
    private readonly DesktopNodeApiAuthSessionHandler authSessionHandler;

    public DesktopNodeApiConsoleRouteHandler(
        DesktopNodeConsoleOptions consoleOptions,
        DesktopNodeApiAuthSessionHandler authSessionHandler)
    {
        this.consoleOptions = consoleOptions;
        this.authSessionHandler = authSessionHandler;
    }

    public DesktopNodeApiResponse? TryHandle(DesktopNodeApiRequest request, string method, string normalizedPath)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "GetConsoleCapabilities", out _))
        {
            return HandleConsoleCapabilities();
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "GetVmConsoleSession", out var consoleMatch))
        {
            return HandleVmConsoleSession(consoleMatch.Parameters["vmId"]);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "PreviewNoVncTarget", out _))
        {
            return HandleNoVncTargetPreview(request);
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

    private DesktopNodeApiResponse HandleNoVncTargetPreview(DesktopNodeApiRequest request)
    {
        const string operation = "console.novnc-target.preview";
        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var evaluation = NoVncTargetPolicy.EvaluatePreview(new NoVncTargetRequest(
            Host: DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "host"),
            Port: DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "port"),
            Auth: authSessionHandler.ResolveNoVncAuth(request),
            AllowLanTarget: DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "allow_lan_target"),
            ListenerAllowLan: consoleOptions.AllowLan,
            Reason: DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "reason")));
        if (!evaluation.Ok)
        {
            var forbidden = string.Equals(
                evaluation.ErrorCode,
                NoVncTargetProblemCodes.ConfigureForbidden,
                StringComparison.Ordinal);
            return DesktopNodeApiResponseFactory.Failure(
                forbidden ? 403 : 400,
                operation,
                evaluation.ErrorCode!,
                forbidden
                    ? "The current account role is not allowed to configure the noVNC target."
                    : "The noVNC target preview was rejected.",
                forbidden
                    ? "Grant console.configure or use the service bearer."
                    : "Pass a loopback host and port, or satisfy the LAN target gates.",
                false);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(
            true,
            operation,
            new SortedDictionary<string, object?>
            {
                ["action"] = evaluation.Action,
                ["allow_lan_target"] = evaluation.AllowLanTarget,
                ["dry_run"] = true,
                ["host"] = evaluation.Host,
                ["host_mutation_performed"] = false,
                ["loopback"] = evaluation.Loopback,
                ["port"] = evaluation.Port,
                ["reason"] = evaluation.Reason,
                ["schema"] = NoVncTargetPolicy.Schema
            },
            null));
    }
}

namespace DesktopNode.Api;

internal static class DesktopNodeApiConsoleAccessProjection
{
    public const string Contract = "console-access-card.v1";
    private const string RequiredPermission = "console.view";

    public static SortedDictionary<string, object?> ForCapabilities(DesktopNodeConsoleOptions options)
    {
        return Build(options, websocketPath: null, includeWebSocketPathTemplate: true);
    }

    public static SortedDictionary<string, object?> ForSession(DesktopNodeConsoleOptions options, string? websocketPath)
    {
        return Build(options, websocketPath, includeWebSocketPathTemplate: false);
    }

    private static SortedDictionary<string, object?> Build(
        DesktopNodeConsoleOptions options,
        string? websocketPath,
        bool includeWebSocketPathTemplate)
    {
        var noVncEnabled = options.NoVncEnabled;
        var noVncStatus = ResolveNoVncStatus(options);
        var resolvedWebSocketPath = noVncEnabled ? websocketPath : null;

        return new SortedDictionary<string, object?>
        {
            ["account"] = new SortedDictionary<string, object?>
            {
                ["auth_surface"] = "service-token-or-account-jwt",
                ["required_permission"] = RequiredPermission,
                ["token_output"] = "redacted"
            },
            ["contract"] = Contract,
            ["host_mutation_performed"] = false,
            ["next_action"] = ResolveNextAction(options, noVncEnabled),
            ["novnc"] = new SortedDictionary<string, object?>
            {
                ["bridge_mode"] = noVncEnabled ? options.NoVncBridgeMode : "disabled",
                ["enabled"] = noVncEnabled,
                ["reason"] = ResolveNoVncReason(noVncStatus),
                ["reason_code"] = noVncStatus,
                ["status"] = noVncStatus,
                ["websocket_path"] = resolvedWebSocketPath,
                ["websocket_path_template"] = noVncEnabled && includeWebSocketPathTemplate ? options.NoVncWebSocketPath : null
            },
            ["status"] = ResolveCardStatus(options, noVncEnabled),
            ["windows_console"] = new SortedDictionary<string, object?>
            {
                ["available"] = options.Enabled,
                ["transport"] = "local-handoff",
                ["type"] = "vmconnect"
            }
        };
    }

    private static string ResolveCardStatus(DesktopNodeConsoleOptions options, bool noVncEnabled)
    {
        if (!options.Enabled)
        {
            return "disabled";
        }

        return noVncEnabled ? "browser-streaming-available" : "local-console-handoff-ready";
    }

    private static string ResolveNextAction(DesktopNodeConsoleOptions options, bool noVncEnabled)
    {
        if (!options.Enabled)
        {
            return "Enable console access on the listener before opening VM console handoff.";
        }

        return noVncEnabled
            ? "Open the noVNC browser session for this VM, or use vmconnect from the host console."
            : "Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.";
    }

    private static string ResolveNoVncStatus(DesktopNodeConsoleOptions options)
    {
        if (options.NoVncEnabled)
        {
            return "available";
        }

        return options.Enabled ? "not_configured" : "disabled";
    }

    private static string ResolveNoVncReason(string status)
    {
        return status switch
        {
            "available" => "noVNC bridge is configured.",
            "disabled" => "Console access is disabled for this listener.",
            _ => "Windows VNC/WebSocket bridge is not configured."
        };
    }
}

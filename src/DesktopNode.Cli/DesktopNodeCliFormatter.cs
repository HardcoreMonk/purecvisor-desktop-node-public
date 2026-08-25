using System.Text.Json;

namespace DesktopNode.Cli;

public static class DesktopNodeCliFormatter
{
    private const string Reset = "\u001b[0m";
    private const string Cyan = "\u001b[38;5;51m";
    private const string Green = "\u001b[38;5;46m";
    private const string Yellow = "\u001b[38;5;226m";
    private const string Magenta = "\u001b[38;5;198m";
    private const string Red = "\u001b[38;5;196m";
    private const string Dim = "\u001b[38;5;240m";

    public static string FormatSuccess(
        DesktopNodeCliTransportResponse response,
        DesktopNodeCliOutputFormat format,
        bool noColor = false)
    {
        return format switch
        {
            DesktopNodeCliOutputFormat.Json => response.Body.Trim(),
            DesktopNodeCliOutputFormat.Csv => FormatCsv(response),
            DesktopNodeCliOutputFormat.Plain => FormatPlain(response),
            _ => FormatTable(response, noColor)
        };
    }

    public static string FormatProblem(DesktopNodeCliTransportResponse response)
    {
        var body = response.Body ?? string.Empty;
        if (TryParseProblem(body, out var problem))
        {
            var lines = new List<string>
            {
                "code=" + problem.Code
            };

            if (!string.IsNullOrWhiteSpace(problem.Message))
            {
                lines.Add("message=" + problem.Message);
            }

            if (!string.IsNullOrWhiteSpace(problem.Detail))
            {
                lines.Add("detail=" + problem.Detail);
            }

            if (!string.IsNullOrWhiteSpace(problem.RecommendedAction))
            {
                lines.Add("Next action: " + problem.RecommendedAction);
            }

            return string.Join(Environment.NewLine, lines);
        }

        return $"PCV_CLI_HTTP_{response.StatusCode}: {body.Trim()}";
    }

    private static string FormatTable(DesktopNodeCliTransportResponse response, bool noColor)
    {
        if (TryFormatVmListTable(response.Body, noColor, out var vmList))
        {
            return vmList;
        }

        if (TryFormatRuntimePolicyTable(response.Body, noColor, out var runtimePolicy))
        {
            return runtimePolicy;
        }

        if (TryFormatOpsSummaryTable(response.Body, noColor, out var opsSummary))
        {
            return opsSummary;
        }

        if (TryFormatNetworkInventoryTable(response.Body, noColor, out var networkInventory))
        {
            return networkInventory;
        }

        if (TryFormatConsoleSessionTable(response.Body, noColor, out var consoleSession))
        {
            return consoleSession;
        }

        if (TryFormatDiagnosticBundleListTable(response.Body, noColor, out var diagnosticBundles))
        {
            return diagnosticBundles;
        }

        if (TryParseOkSummary(response.Body, out var summary))
        {
            return summary;
        }

        return response.Body.Trim();
    }

    private static string FormatPlain(DesktopNodeCliTransportResponse response)
    {
        if (TryFormatVmListPlain(response.Body, out var vmList))
        {
            return vmList;
        }

        if (TryFormatDiagnosticBundleListPlain(response.Body, out var diagnosticBundles))
        {
            return diagnosticBundles;
        }

        if (TryParseOkSummary(response.Body, out var summary))
        {
            return summary.Replace(" | ", Environment.NewLine, StringComparison.Ordinal);
        }

        return response.Body.Trim();
    }

    private static string FormatCsv(DesktopNodeCliTransportResponse response)
    {
        if (TryFormatVmListCsv(response.Body, out var vmList))
        {
            return vmList;
        }

        if (TryFormatDiagnosticBundleListCsv(response.Body, out var diagnosticBundles))
        {
            return diagnosticBundles;
        }

        if (TryParseOkSummary(response.Body, out var summary))
        {
            return "summary" + Environment.NewLine + EscapeCsv(summary);
        }

        return "body" + Environment.NewLine + EscapeCsv(response.Body.Trim());
    }

    private static bool TryFormatVmListTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        if (!TryReadVmListRows(body, out var rows))
        {
            return false;
        }

        if (rows.Count == 0)
        {
            table = Color("No VMs found.", Dim, noColor);
            return true;
        }

        var idWidth = Math.Max("SYS_UUID".Length, rows.Max(static row => row.Id.Length));
        var nameWidth = Math.Max("ENTITY_ID".Length, rows.Max(static row => row.Name.Length));
        var stateWidth = Math.Max("LIFELINE".Length, rows.Max(static row => row.State.Length));
        var header =
            "SYS_UUID".PadRight(idWidth) + " | " +
            "ENTITY_ID".PadRight(nameWidth) + " | " +
            "LIFELINE".PadRight(stateWidth);
        var separator = new string('-', idWidth) + "-+-" +
            new string('-', nameWidth) + "-+-" +
            new string('-', stateWidth);

        var lines = new List<string>
        {
            Color(header, Cyan, noColor),
            Color(separator, Cyan, noColor)
        };

        foreach (var row in rows)
        {
            lines.Add(
                Color(row.Id.PadRight(idWidth), Dim, noColor) + " | " +
                Color(row.Name.PadRight(nameWidth), Yellow, noColor) + " | " +
                Color(row.State.PadRight(stateWidth), StateColor(row.State), noColor));
        }

        table = string.Join(Environment.NewLine, lines);
        return true;
    }

    private static bool TryFormatVmListPlain(string body, out string plain)
    {
        plain = string.Empty;
        if (!TryReadVmListRows(body, out var rows))
        {
            return false;
        }

        plain = rows.Count == 0
            ? "No VMs found."
            : string.Join(Environment.NewLine, rows.Select(static row =>
                $"sys_uuid={row.Id} | entity_id={row.Name} | lifeline={row.State}"));
        return true;
    }

    private static bool TryFormatVmListCsv(string body, out string csv)
    {
        csv = string.Empty;
        if (!TryReadVmListRows(body, out var rows))
        {
            return false;
        }

        var lines = new List<string>
        {
            "sys_uuid,entity_id,lifeline"
        };
        lines.AddRange(rows.Select(static row =>
            EscapeCsv(row.Id) + "," + EscapeCsv(row.Name) + "," + EscapeCsv(row.State)));
        csv = string.Join(Environment.NewLine, lines);
        return true;
    }

    private static bool TryFormatDiagnosticBundleListTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        if (!TryReadDiagnosticBundleList(body, out var projection))
        {
            return false;
        }

        var lines = new List<string>
        {
            Color("Diagnostic Bundles", Cyan, noColor)
        };
        if (projection.Rows.Count == 0)
        {
            lines.Add(Color("No diagnostic bundles found.", Dim, noColor));
        }
        else
        {
            for (var index = 0; index < projection.Rows.Count; index++)
            {
                var row = projection.Rows[index];
                var fields = new KeyValueRow[]
                {
                    new("bundle_id", DisplayValue(row.BundleId)),
                    new("file_name", DisplayValue(row.FileName)),
                    new("created_at", DisplayValue(row.CreatedAt)),
                    new("last_write_time_utc", DisplayValue(row.LastWriteTimeUtc)),
                    new("size_bytes", DisplayValue(row.SizeBytes)),
                    new("download_url", DisplayValue(row.DownloadUrl)),
                    new("archive_status", DisplayValue(row.ArchiveStatus)),
                    new("redaction_status", DisplayValue(row.RedactionStatus))
                };
                var fieldWidth = Math.Max("FIELD".Length, fields.Max(static field => field.Key.Length));
                var valueWidth = Math.Max("VALUE".Length, fields.Max(static field => field.Value.Length));

                lines.Add(Color($"Bundle {index + 1}", Magenta, noColor));
                lines.Add(Color("FIELD".PadRight(fieldWidth) + " | " + "VALUE".PadRight(valueWidth), Cyan, noColor));
                lines.Add(Color(new string('-', fieldWidth) + "-+-" + new string('-', valueWidth), Cyan, noColor));
                lines.AddRange(fields.Select(field =>
                    Color(field.Key.PadRight(fieldWidth), Yellow, noColor) + " | " +
                    Color(field.Value.PadRight(valueWidth), Dim, noColor)));
                lines.Add(string.Empty);
            }
        }

        lines.Add(Color(FormatDiagnosticBundlePage(projection), Dim, noColor));
        table = string.Join(Environment.NewLine, lines);
        return true;
    }

    private static bool TryFormatDiagnosticBundleListPlain(string body, out string plain)
    {
        plain = string.Empty;
        if (!TryReadDiagnosticBundleList(body, out var projection))
        {
            return false;
        }

        var lines = projection.Rows.Select(static row =>
            $"bundle_id={DisplayValue(row.BundleId)} | file_name={DisplayValue(row.FileName)} | created_at={DisplayValue(row.CreatedAt)} | last_write_time_utc={DisplayValue(row.LastWriteTimeUtc)} | size_bytes={DisplayValue(row.SizeBytes)} | download_url={DisplayValue(row.DownloadUrl)} | archive_status={DisplayValue(row.ArchiveStatus)} | redaction_status={DisplayValue(row.RedactionStatus)}")
            .ToList();
        if (lines.Count == 0)
        {
            lines.Add("No diagnostic bundles found.");
        }

        lines.Add(FormatDiagnosticBundlePage(projection));
        plain = string.Join(Environment.NewLine, lines);
        return true;
    }

    private static bool TryFormatDiagnosticBundleListCsv(string body, out string csv)
    {
        csv = string.Empty;
        if (!TryReadDiagnosticBundleList(body, out var projection))
        {
            return false;
        }

        var lines = new List<string>
        {
            "bundle_id,file_name,created_at,last_write_time_utc,size_bytes,download_url,archive_status,redaction_status,count,returned,limit,offset,next_offset"
        };
        if (projection.Rows.Count == 0)
        {
            lines.Add(string.Join(",", Enumerable.Repeat(EscapeCsv(string.Empty), 8).Concat(PageCsvValues(projection))));
        }
        else
        {
            lines.AddRange(projection.Rows.Select(row => string.Join(",", new[]
            {
                EscapeCsv(row.BundleId),
                EscapeCsv(row.FileName),
                EscapeCsv(row.CreatedAt),
                EscapeCsv(row.LastWriteTimeUtc),
                EscapeCsv(row.SizeBytes),
                EscapeCsv(row.DownloadUrl),
                EscapeCsv(row.ArchiveStatus),
                EscapeCsv(row.RedactionStatus)
            }.Concat(PageCsvValues(projection)))));
        }

        csv = string.Join(Environment.NewLine, lines);
        return true;
    }

    private static bool TryReadDiagnosticBundleList(string body, out DiagnosticBundleListProjection projection)
    {
        projection = default!;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetOperationData(document.RootElement, "diagnostic.bundle.list", out var data) ||
                !data.TryGetProperty("bundles", out var bundles) ||
                bundles.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var rows = bundles
                .EnumerateArray()
                .Where(static item => item.ValueKind == JsonValueKind.Object)
                .Select(static item => new DiagnosticBundleRow(
                    ReadScalar(item, "bundle_id") ?? string.Empty,
                    ReadScalar(item, "file_name") ?? string.Empty,
                    ReadScalar(item, "created_at") ?? string.Empty,
                    ReadScalar(item, "last_write_time_utc") ?? string.Empty,
                    ReadScalar(item, "size_bytes") ?? string.Empty,
                    ReadScalar(item, "download_url") ?? string.Empty,
                    ReadScalar(item, "archive_status") ?? string.Empty,
                    ReadScalar(item, "redaction_status") ?? string.Empty))
                .ToArray();
            projection = new DiagnosticBundleListProjection(
                rows,
                ReadScalar(data, "count") ?? string.Empty,
                ReadScalar(data, "returned") ?? string.Empty,
                ReadScalar(data, "limit") ?? string.Empty,
                ReadScalar(data, "offset") ?? string.Empty,
                ReadScalar(data, "next_offset") ?? string.Empty);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string FormatDiagnosticBundlePage(DiagnosticBundleListProjection projection)
    {
        return $"Page: count={DisplayValue(projection.Count)} | returned={DisplayValue(projection.Returned)} | limit={DisplayValue(projection.Limit)} | offset={DisplayValue(projection.Offset)} | next_offset={DisplayValue(projection.NextOffset)}";
    }

    private static IEnumerable<string> PageCsvValues(DiagnosticBundleListProjection projection)
    {
        return new[]
        {
            EscapeCsv(projection.Count),
            EscapeCsv(projection.Returned),
            EscapeCsv(projection.Limit),
            EscapeCsv(projection.Offset),
            EscapeCsv(projection.NextOffset)
        };
    }

    private static bool TryFormatRuntimePolicyTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetOperationData(document.RootElement, "runtime.policy", out var data))
            {
                return false;
            }

            var rows = new List<KeyValueRow>();
            AddRow(rows, "runtime.owner", ReadScalar(data, "runtime_core", "owner"));
            AddRow(rows, "auth.mode", ReadScalar(data, "auth", "mode"));
            AddRow(rows, "auth.roles", ReadScalar(data, "auth", "roles"));
            AddRow(rows, "auth.token_storage", ReadScalar(data, "auth", "token_storage") ?? ReadScalar(data, "runtime_core", "auth_session", "token_storage"));
            AddRow(rows, "auth.boundary", ReadScalar(data, "runtime_core", "auth_session", "boundary"));
            AddRow(rows, "network.current_exposure", ReadScalar(data, "network", "current_exposure"));
            AddRow(rows, "network.lan_mode", ReadScalar(data, "network", "lan_mode"));
            AddRow(rows, "job.state_store", ReadScalar(data, "runtime_core", "job_runtime", "state_store") ?? ReadScalar(data, "job_runtime", "state_store", "persistence"));
            AddRow(rows, "job.dispatch.mode", ReadScalar(data, "job_runtime", "dispatch", "mode") ?? ReadScalar(data, "runtime_core", "job_runtime", "worker"));
            AddRow(rows, "job.mutation_dispatch", ReadScalar(data, "job_runtime", "dispatch", "mutation_dispatch"));
            AddRow(rows, "job.running_interrupt_operations", ReadScalar(data, "job_runtime", "control", "cancel", "running_interrupt_operations"));
            AddRow(rows, "native_core.status", ReadScalar(data, "job_runtime", "native_core", "status"));
            AddRow(rows, "diagnostics.bundle_root", ReadScalar(data, "runtime_core", "diagnostics", "bundle_root"));
            AddRow(rows, "console.mode", ReadScalar(data, "console", "mode"));
            AddRow(rows, "console.novnc", ReadScalar(data, "console", "novnc"));

            table = FormatKeyValueTable("Runtime Policy", rows, noColor);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryFormatOpsSummaryTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetOperationData(document.RootElement, "ops.summary", out var data))
            {
                return false;
            }

            var rows = new List<KeyValueRow>();
            if (TryReadPath(data, out var signals, "signals") && signals.ValueKind == JsonValueKind.Array)
            {
                foreach (var signal in signals.EnumerateArray().Where(static item => item.ValueKind == JsonValueKind.Object))
                {
                    var label = ReadScalar(signal, "label") ?? ReadScalar(signal, "key");
                    var value = ReadScalar(signal, "value");
                    var tone = ReadScalar(signal, "tone");
                    if (!string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(value))
                    {
                        AddRow(rows, "signal." + label, string.IsNullOrWhiteSpace(tone) ? value : value + " (" + tone + ")");
                    }
                }
            }

            AddRow(rows, "vm.total", ReadScalar(data, "vm_counts", "total"));
            AddRow(rows, "vm.running", ReadScalar(data, "vm_counts", "running"));
            AddRow(rows, "vm.checkpoint_warnings", ReadScalar(data, "vm_counts", "checkpoint_warnings"));
            AddRow(rows, "job.queued", ReadScalar(data, "job_counts", "queued"));
            AddRow(rows, "job.running", ReadScalar(data, "job_counts", "running"));
            AddRow(rows, "job.failed", ReadScalar(data, "job_counts", "failed"));
            AddRow(rows, "job.succeeded", ReadScalar(data, "job_counts", "succeeded"));
            AddRow(rows, "host.windows", JoinNonEmpty(" ", ReadScalar(data, "host", "windows", "edition"), ReadScalar(data, "host", "windows", "version")));
            AddRow(rows, "host.admin_elevated", ReadScalar(data, "host", "admin", "elevated"));
            AddRow(rows, "host.hyperv_feature", ReadScalar(data, "host", "hyperv", "feature_enabled"));
            AddRow(rows, "host.vmms_running", ReadScalar(data, "host", "hyperv", "vmms_running"));
            AddRow(rows, "host.default_switch", ReadScalar(data, "host", "hyperv", "default_switch_present"));
            AddRow(rows, "installed.version", ReadScalar(data, "installed_runtime", "version"));
            AddRow(rows, "installed.service_state", ReadScalar(data, "installed_runtime", "service_state"));
            AddRow(rows, "installed.evidence_status", ReadScalar(data, "installed_runtime", "evidence_status"));
            AddRow(rows, "batch_evidence.status", ReadScalar(data, "batch_evidence", "status"));
            AddRow(rows, "batch_evidence.latest_batch", ReadScalar(data, "batch_evidence", "latest", "batch_id"));
            AddRow(rows, "current.full_admin_host_mutation", JoinNonEmpty(" / ",
                ReadScalar(data, "current_evidence", "full_admin_host_mutation", "latest", "version"),
                ReadScalar(data, "current_evidence", "full_admin_host_mutation", "latest", "status")));
            AddRow(rows, "current.public_boundary_main_push", ReadScalar(data, "current_evidence", "public_boundary", "latest_main_push", "run_id"));
            AddRow(rows, "current.public_boundary_head_sha", ReadScalar(data, "current_evidence", "public_boundary", "latest_main_push", "head_sha"));
            AddRow(rows, "current.manual_admin_package_pair", ReadScalar(data, "current_evidence", "manual_admin", "latest_package_pair", "package_pair"));
            AddRow(rows, "current.manual_admin_next_package_pair", ReadScalar(data, "current_evidence", "manual_admin", "next_package_pair", "package_pair"));
            AddRow(rows, "current.manual_admin_next_decision", ReadScalar(data, "current_evidence", "manual_admin", "next_package_pair", "decision"));

            table = FormatKeyValueTable("Ops Summary", rows, noColor);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryFormatNetworkInventoryTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetOperationData(document.RootElement, "network.inventory", out var data))
            {
                return false;
            }

            if (!TryReadPath(data, out var switches, "switches") || switches.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var rows = switches
                .EnumerateArray()
                .Where(static item => item.ValueKind == JsonValueKind.Object)
                .Select(static item => new NetworkSwitchRow(
                    ReadScalar(item, "name") ?? "-",
                    ReadScalar(item, "type") ?? "-",
                    ReadBoolLabel(item, "is_default"),
                    ReadBoolLabel(item, "allow_management_os"),
                    ReadScalar(item, "net_adapter_interface_description") ?? "-"))
                .ToArray();

            if (rows.Length == 0)
            {
                table = Color("Network Inventory", Cyan, noColor) + Environment.NewLine +
                    Color("No Hyper-V switches found.", Dim, noColor);
                return true;
            }

            var nameWidth = Math.Max("NAME".Length, rows.Max(static row => row.Name.Length));
            var typeWidth = Math.Max("TYPE".Length, rows.Max(static row => row.Type.Length));
            var defaultWidth = Math.Max("DEFAULT".Length, rows.Max(static row => row.IsDefault.Length));
            var managementWidth = Math.Max("MGMT_OS".Length, rows.Max(static row => row.AllowManagementOs.Length));
            var adapterWidth = Math.Max("ADAPTER".Length, rows.Max(static row => row.Adapter.Length));
            var header =
                "NAME".PadRight(nameWidth) + " | " +
                "TYPE".PadRight(typeWidth) + " | " +
                "DEFAULT".PadRight(defaultWidth) + " | " +
                "MGMT_OS".PadRight(managementWidth) + " | " +
                "ADAPTER".PadRight(adapterWidth);
            var separator = new string('-', nameWidth) + "-+-" +
                new string('-', typeWidth) + "-+-" +
                new string('-', defaultWidth) + "-+-" +
                new string('-', managementWidth) + "-+-" +
                new string('-', adapterWidth);
            var lines = new List<string>
            {
                Color("Network Inventory", Cyan, noColor),
                Color(header, Cyan, noColor),
                Color(separator, Cyan, noColor)
            };

            foreach (var row in rows)
            {
                lines.Add(
                    Color(row.Name.PadRight(nameWidth), Yellow, noColor) + " | " +
                    Color(row.Type.PadRight(typeWidth), Dim, noColor) + " | " +
                    Color(row.IsDefault.PadRight(defaultWidth), BoolColor(row.IsDefault), noColor) + " | " +
                    Color(row.AllowManagementOs.PadRight(managementWidth), BoolColor(row.AllowManagementOs), noColor) + " | " +
                    Color(row.Adapter.PadRight(adapterWidth), Dim, noColor));
            }

            table = string.Join(Environment.NewLine, lines);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryFormatConsoleSessionTable(string body, bool noColor, out string table)
    {
        table = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetOperationData(document.RootElement, "console.session", out var data) ||
                !TryReadPath(data, out var consoleAccess, "console_access") ||
                consoleAccess.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var rows = new List<KeyValueRow>
            {
                new("vm.id", DisplayValue(
                    ReadScalar(data, "vm_id") ??
                    ReadScalar(data, "id") ??
                    ReadScalar(data, "vm", "id") ??
                    ReadScalar(consoleAccess, "vm", "id"))),
                new("account.permission", DisplayValue(
                    ReadScalar(consoleAccess, "account", "required_permission") ??
                    ReadScalar(consoleAccess, "account", "permission"))),
                new("windows_console.type", DisplayValue(ReadScalar(consoleAccess, "windows_console", "type"))),
                new("windows_console.transport", DisplayValue(ReadScalar(consoleAccess, "windows_console", "transport"))),
                new("novnc.status", DisplayValue(ReadScalar(consoleAccess, "novnc", "status"))),
                new("novnc.bridge_mode", DisplayValue(ReadScalar(consoleAccess, "novnc", "bridge_mode"))),
                new("novnc.websocket_path", DisplayValue(ReadScalar(consoleAccess, "novnc", "websocket_path"))),
                new("status", DisplayValue(ReadScalar(consoleAccess, "status"))),
                new("next_action", DisplayValue(ReadScalar(consoleAccess, "next_action"))),
                new("host_mutation_performed", DisplayValue(ReadScalar(consoleAccess, "host_mutation_performed")))
            };

            table = FormatKeyValueTable("Console Access", rows, noColor);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadVmListRows(string body, out IReadOnlyList<VmListRow> rows)
    {
        rows = [];
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("operation", out var operation) ||
                operation.ValueKind != JsonValueKind.String ||
                !string.Equals(operation.GetString(), "vm.list", StringComparison.OrdinalIgnoreCase) ||
                !root.TryGetProperty("data", out var data) ||
                data.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            rows = data
                .EnumerateArray()
                .Where(static item => item.ValueKind == JsonValueKind.Object)
                .Select(static item =>
                {
                    var id = ReadString(item, "id") ??
                        ReadString(item, "sys_uuid") ??
                        ReadString(item, "uuid") ??
                        "-";
                    var name = ReadString(item, "name") ??
                        ReadString(item, "entity_id") ??
                        id;
                    var state = ReadString(item, "state") ??
                        ReadString(item, "lifeline") ??
                        "unknown";
                    return new VmListRow(id, name, state);
                })
                .ToArray();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string Color(string value, string color, bool noColor)
    {
        return noColor ? value : color + value + Reset;
    }

    private static string StateColor(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "running" => Green,
            "stopped" or "off" or "shutoff" => Red,
            "paused" or "saved" => Yellow,
            "starting" or "stopping" or "saving" or "pausing" or "resuming" => Magenta,
            _ => Cyan
        };
    }

    private static string BoolColor(string value)
    {
        return string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ? Green : Dim;
    }

    private static string FormatKeyValueTable(string title, IReadOnlyList<KeyValueRow> rows, bool noColor)
    {
        if (rows.Count == 0)
        {
            return Color(title, Cyan, noColor) + Environment.NewLine + Color("No data.", Dim, noColor);
        }

        var keyWidth = Math.Max("KEY".Length, rows.Max(static row => row.Key.Length));
        var valueWidth = Math.Max("VALUE".Length, rows.Max(static row => row.Value.Length));
        var header = "KEY".PadRight(keyWidth) + " | " + "VALUE".PadRight(valueWidth);
        var separator = new string('-', keyWidth) + "-+-" + new string('-', valueWidth);
        var lines = new List<string>
        {
            Color(title, Cyan, noColor),
            Color(header, Cyan, noColor),
            Color(separator, Cyan, noColor)
        };

        lines.AddRange(rows.Select(row =>
            Color(row.Key.PadRight(keyWidth), Yellow, noColor) + " | " +
            Color(row.Value.PadRight(valueWidth), Dim, noColor)));
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddRow(ICollection<KeyValueRow> rows, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            rows.Add(new KeyValueRow(key, value));
        }
    }

    private static bool TryGetOperationData(JsonElement root, string expectedOperation, out JsonElement data)
    {
        data = default;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("operation", out var operation) ||
            operation.ValueKind != JsonValueKind.String ||
            !string.Equals(operation.GetString(), expectedOperation, StringComparison.OrdinalIgnoreCase) ||
            !root.TryGetProperty("data", out data) ||
            data.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return true;
    }

    private static bool TryReadPath(JsonElement element, out JsonElement value, params string[] path)
    {
        value = element;
        foreach (var segment in path)
        {
            if (value.ValueKind != JsonValueKind.Object ||
                !value.TryGetProperty(segment, out value))
            {
                value = default;
                return false;
            }
        }

        return true;
    }

    private static string? ReadScalar(JsonElement element, params string[] path)
    {
        if (!TryReadPath(element, out var value, path))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Array => string.Join(", ", value.EnumerateArray().Select(ScalarValue).Where(static item => !string.IsNullOrWhiteSpace(item))),
            _ => null
        };
    }

    private static string? ScalarValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static string ReadBoolLabel(JsonElement element, string name)
    {
        if (!TryReadPath(element, out var value, name))
        {
            return "-";
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => "yes",
            JsonValueKind.False => "no",
            _ => ReadScalar(element, name) ?? "-"
        };
    }

    private static string? JoinNonEmpty(string separator, params string?[] values)
    {
        var parts = values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return parts.Length == 0 ? null : string.Join(separator, parts);
    }

    private static string DisplayValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static bool TryParseOkSummary(string body, out string summary)
    {
        summary = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var parts = new List<string>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                {
                    parts.Add(property.Name + "=" + property.Value.ToString());
                }
            }

            summary = parts.Count == 0 ? body.Trim() : string.Join(" | ", parts);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseProblem(string body, out CliProblem problem)
    {
        problem = new CliProblem("PCV_CLI_API_ERROR", null, null, null);

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var root = document.RootElement;
            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                problem = ReadProblem(error);
                return HasProblemFields(problem);
            }

            problem = ReadProblem(root);
            return HasProblemFields(problem);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static CliProblem ReadProblem(JsonElement element)
    {
        return new CliProblem(
            ReadString(element, "code") ?? "PCV_CLI_API_ERROR",
            ReadString(element, "message") ?? ReadString(element, "title"),
            ReadString(element, "detail"),
            ReadString(element, "recommended_action"));
    }

    private static bool HasProblemFields(CliProblem problem)
    {
        return problem.Code != "PCV_CLI_API_ERROR" ||
            !string.IsNullOrWhiteSpace(problem.Message) ||
            !string.IsNullOrWhiteSpace(problem.Detail) ||
            !string.IsNullOrWhiteSpace(problem.RecommendedAction);
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string EscapeCsv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private sealed record CliProblem(string Code, string? Message, string? Detail, string? RecommendedAction);

    private sealed record KeyValueRow(string Key, string Value);

    private sealed record DiagnosticBundleListProjection(
        IReadOnlyList<DiagnosticBundleRow> Rows,
        string Count,
        string Returned,
        string Limit,
        string Offset,
        string NextOffset);

    private sealed record DiagnosticBundleRow(
        string BundleId,
        string FileName,
        string CreatedAt,
        string LastWriteTimeUtc,
        string SizeBytes,
        string DownloadUrl,
        string ArchiveStatus,
        string RedactionStatus);

    private sealed record NetworkSwitchRow(string Name, string Type, string IsDefault, string AllowManagementOs, string Adapter);

    private sealed record VmListRow(string Id, string Name, string State);
}

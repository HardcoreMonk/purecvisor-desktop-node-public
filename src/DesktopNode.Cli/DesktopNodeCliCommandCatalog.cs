using System.Text.Json;

namespace DesktopNode.Cli;

public static class DesktopNodeCliCommandCatalog
{
    private const int MaxQosPolicyValue = 1_000_000_000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    public static DesktopNodeCliRequest CreateRequest(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            throw Usage("A command is required.");
        }

        return arguments[0].ToLowerInvariant() switch
        {
            "host" => HostRequest(arguments),
            "runtime" => RuntimeRequest(arguments),
            "ops" => OpsRequest(arguments),
            "network" => NetworkRequest(arguments),
            "vm" => VmRequest(arguments),
            "job" => JobRequest(arguments),
            "diagnostics" => DiagnosticsRequest(arguments),
            _ => throw Usage($"Unknown command group '{arguments[0]}'.")
        };
    }

    private static DesktopNodeCliRequest HostRequest(IReadOnlyList<string> args)
    {
        RequireShape(args, 2, "host status");
        if (!Is(args[1], "status"))
        {
            throw Usage("Use: host status.");
        }

        return new DesktopNodeCliRequest("GET", "/api/v1/host/status");
    }

    private static DesktopNodeCliRequest RuntimeRequest(IReadOnlyList<string> args)
    {
        RequireShape(args, 2, "runtime policy");
        if (!Is(args[1], "policy"))
        {
            throw Usage("Use: runtime policy.");
        }

        return new DesktopNodeCliRequest("GET", "/api/v1/runtime/policy");
    }

    private static DesktopNodeCliRequest OpsRequest(IReadOnlyList<string> args)
    {
        RequireShape(args, 2, "ops summary");
        if (!Is(args[1], "summary"))
        {
            throw Usage("Use: ops summary.");
        }

        return new DesktopNodeCliRequest("GET", "/api/v1/ops/summary");
    }

    private static DesktopNodeCliRequest NetworkRequest(IReadOnlyList<string> args)
    {
        RequireShape(args, 2, "network inventory");
        if (!Is(args[1], "inventory") && !Is(args[1], "list"))
        {
            throw Usage("Use: network inventory|list.");
        }

        return new DesktopNodeCliRequest("GET", "/api/v1/network/inventory");
    }

    private static DesktopNodeCliRequest VmRequest(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            throw Usage("Use: vm list|get|create|start|shutdown|poweroff|restart|pause|resume|save|resume-saved|rename|manage|delete|checkpoint|attach.");
        }

        return args[1].ToLowerInvariant() switch
        {
            "list" => Fixed(args, 2, new DesktopNodeCliRequest("GET", "/api/v1/vms"), "vm list"),
            "get" => VmGet(args),
            "create" => VmCreate(args),
            "start" or "shutdown" or "poweroff" or "restart" or "pause" or "resume" or "save" or "resume-saved" => VmLifecycle(args),
            "stop" => VmLifecycle(args, "poweroff"),
            "guest-shutdown" => VmLifecycle(args, "shutdown"),
            "console" or "vnc" => VmConsole(args),
            "memory-stats" => VmStats(args, "memory-stats"),
            "cpu-stats" => VmStats(args, "cpu-stats"),
            "manage" => VmManage(args),
            "delete" => VmDelete(args),
            "checkpoint" or "snapshot" => CheckpointRequest(args),
            "rename" => VmRename(args),
            "limit" => VmLimit(args),
            "set-memory" => VmResourceMutation(args, "set-memory", "memory_mb"),
            "set-vcpu" => VmResourceMutation(args, "set-vcpu", "cpu"),
            "eject" => VmEject(args),
            "attach" => VmAttach(args),
            "delete-status" => VmDeleteStatus(args),
            "disk-resize" => VmResourceMutation(args, "disk-resize", "disk_gb"),
            "guest-agent-status" => VmReadback(args, "guest-agent/status"),
            "guest-agent-ensure-channel" => VmGuestAgentEnsureChannel(args),
            "guest-ping" => VmReadback(args, "guest-agent/ping"),
            "guest-exec" => VmGuestExec(args),
            "blkio-set" => VmQosMutation(
                args,
                "qos/storage",
                "disk",
                "--disk",
                "maximum_iops",
                "--maximum-iops",
                "minimum_iops",
                "--minimum-iops"),
            "blkio-get" => VmReadback(args, "blkio"),
            "bandwidth" => VmReadback(args, "bandwidth"),
            "bandwidth-set" => VmQosMutation(
                args,
                "qos/network",
                "adapter",
                "--adapter",
                "maximum_kbps",
                "--maximum-kbps",
                "minimum_kbps",
                "--minimum-kbps"),
            _ => throw Usage("Use: vm list|get|create|start|stop|shutdown|guest-shutdown|poweroff|restart|pause|resume|save|resume-saved|rename|console|vnc|manage|delete|checkpoint|snapshot|attach.")
        };
    }

    private static DesktopNodeCliRequest VmGet(IReadOnlyList<string> args)
    {
        RequireShape(args, 3, "vm get <vm>");
        return new DesktopNodeCliRequest("GET", $"/api/v1/vms/{Segment(args[2])}");
    }

    private static DesktopNodeCliRequest VmLifecycle(IReadOnlyList<string> args, string? routeAction = null)
    {
        RequireShape(args, 3, $"vm {args[1]} <vm>");
        return new DesktopNodeCliRequest("POST", $"/api/v1/vms/{Segment(args[2])}/{routeAction ?? args[1].ToLowerInvariant()}");
    }

    private static DesktopNodeCliRequest VmConsole(IReadOnlyList<string> args)
    {
        RequireShape(args, 3, $"vm {args[1]} <vm>");
        return new DesktopNodeCliRequest("GET", $"/api/v1/vms/{Segment(args[2])}/console");
    }

    private static DesktopNodeCliRequest VmStats(IReadOnlyList<string> args, string routeSuffix)
    {
        RequireShape(args, 3, $"vm {args[1]} <vm>");
        return new DesktopNodeCliRequest("GET", $"/api/v1/vms/{Segment(args[2])}/{routeSuffix}");
    }

    private static DesktopNodeCliRequest VmReadback(IReadOnlyList<string> args, string routeSuffix)
    {
        RequireShape(args, 3, $"vm {args[1]} <vm>");
        return new DesktopNodeCliRequest("GET", $"/api/v1/vms/{Segment(args[2])}/{routeSuffix}");
    }

    private static DesktopNodeCliRequest VmRename(IReadOnlyList<string> args)
    {
        RequireShape(args, 4, "vm rename <vm> <new_name>");
        var body = new SortedDictionary<string, object?>
        {
            ["new_name"] = args[3]
        };

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/rename",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmEject(IReadOnlyList<string> args)
    {
        RequireShape(args, 3, "vm eject <vm>");
        return new DesktopNodeCliRequest("POST", $"/api/v1/vms/{Segment(args[2])}/eject");
    }

    private static DesktopNodeCliRequest VmAttach(IReadOnlyList<string> args)
    {
        if (args.Count < 3)
        {
            throw Usage("Use: vm attach <vm> --iso <path>");
        }

        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: false);
        var isoPath = Required(parsed.Options, "--iso", "--iso_path");
        var body = new SortedDictionary<string, object?>
        {
            ["iso_path"] = isoPath
        };

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/attach",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmDeleteStatus(IReadOnlyList<string> args)
    {
        RequireShape(args, 3, "vm delete-status <vm>");
        return new DesktopNodeCliRequest("GET", $"/api/v1/vms/{Segment(args[2])}/delete-status");
    }

    private static DesktopNodeCliRequest VmResourceMutation(IReadOnlyList<string> args, string routeAction, string bodyProperty)
    {
        RequireShape(args, 4, $"vm {args[1]} <vm> <value>");
        var body = new SortedDictionary<string, object?>
        {
            [bodyProperty] = ParseInt(bodyProperty, args[3])
        };

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/{routeAction}",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmQosMutation(
        IReadOnlyList<string> args,
        string route,
        string targetProperty,
        string targetOption,
        string maximumProperty,
        string maximumOption,
        string minimumProperty,
        string minimumOption)
    {
        if (args.Count < 6)
        {
            throw Usage($"Use: vm {args[1]} <vm> {targetOption} VALUE {maximumOption} N [{minimumOption} N] --dry-run|--yes.");
        }

        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: true);
        var dryRun = HasFlag(parsed.Options, "--dry-run");
        var confirmed = HasFlag(parsed.Options, "--yes");
        if (dryRun == confirmed)
        {
            throw new ArgumentException(
                "PCV_CLI_CONFIRMATION_REQUIRED|" +
                $"VM QoS mutation '{args[1]}' requires exactly one execution mode.|" +
                $"Use --dry-run to preview or --yes to apply vm {args[1]}.");
        }

        var target = Required(parsed.Options, targetOption);
        var maximum = RequiredInt(parsed.Options, maximumOption);
        var minimum = FirstOption(parsed.Options, minimumOption);
        int? minimumValue = string.IsNullOrWhiteSpace(minimum) ? null : ParseInt(minimumOption, minimum);
        ValidateQosRange(
            command: args[1],
            maximum,
            minimumValue,
            isStorage: string.Equals(maximumProperty, "maximum_iops", StringComparison.Ordinal));

        var body = new SortedDictionary<string, object?>
        {
            [targetProperty] = target,
            [maximumProperty] = maximum
        };
        if (!string.IsNullOrWhiteSpace(minimum))
        {
            body[minimumProperty] = minimumValue!.Value;
        }

        if (dryRun)
        {
            body["dry_run"] = true;
        }

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/{route}{(dryRun ? "/preview" : string.Empty)}",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static void ValidateQosRange(string command, int maximum, int? minimum, bool isStorage)
    {
        if (maximum >= 0 &&
            maximum <= MaxQosPolicyValue &&
            minimum is null or >= 0 &&
            (minimum is null || minimum <= maximum))
        {
            return;
        }

        throw new ArgumentException(
            (isStorage ? "PCV_VM_QOS_STORAGE_RANGE_INVALID" : "PCV_VM_QOS_NETWORK_RANGE_INVALID") +
            $"|VM QoS mutation '{command}' values are outside the supported range.|" +
            (isStorage
                ? "Use non-negative IOPS values through 1000000000 and keep --minimum-iops less than or equal to --maximum-iops."
                : "Use non-negative Kbps values through 1000000000 and keep --minimum-kbps less than or equal to --maximum-kbps."));
    }

    private static DesktopNodeCliRequest VmGuestAgentEnsureChannel(IReadOnlyList<string> args)
    {
        if (args.Count == 4 && Is(args[3], "--dry-run"))
        {
            var body = new SortedDictionary<string, object?>
            {
                ["dry_run"] = true,
                ["mode"] = "dry-run"
            };

            return new DesktopNodeCliRequest(
                "POST",
                $"/api/v1/vms/{Segment(args[2])}/guest/channel/preview",
                JsonSerializer.Serialize(body, JsonOptions));
        }

        if (args.Count >= 5 && args.Skip(3).Any(item => Is(item, "--verify")))
        {
            var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: true);
            if (!HasFlag(parsed.Options, "--verify"))
            {
                throw Usage("Use: vm guest-agent-ensure-channel <vm> --verify --credential-ref REF [--timeout-sec N].");
            }

            var body = new SortedDictionary<string, object?>
            {
                ["credential_ref"] = Required(parsed.Options, "--credential-ref"),
                ["mode"] = "verify"
            };
            var timeoutSec = FirstOption(parsed.Options, "--timeout-sec", "--timeout");
            if (!string.IsNullOrWhiteSpace(timeoutSec))
            {
                body["timeout_sec"] = ParseInt("--timeout-sec", timeoutSec);
            }

            return new DesktopNodeCliRequest(
                "POST",
                $"/api/v1/vms/{Segment(args[2])}/guest/channel/verify",
                JsonSerializer.Serialize(body, JsonOptions));
        }

        if (args.Count == 5 && Is(args[3], "--repair") && Is(args[4], "--yes"))
        {
            var body = new SortedDictionary<string, object?>
            {
                ["mode"] = "repair",
                ["yes"] = true
            };

            return new DesktopNodeCliRequest(
                "POST",
                $"/api/v1/vms/{Segment(args[2])}/guest/channel",
                JsonSerializer.Serialize(body, JsonOptions));
        }

        if (args.Count < 4)
        {
            throw new ArgumentException(
                "PCV_CLI_CONFIRMATION_REQUIRED|" +
                "Guest channel ensure requires an explicit mode.|" +
                "Use: vm guest-agent-ensure-channel <vm> --dry-run|--verify --credential-ref REF|--repair --yes.");
        }

        throw Usage("Use: vm guest-agent-ensure-channel <vm> --dry-run|--verify --credential-ref REF [--timeout-sec N]|--repair --yes.");
    }

    private static DesktopNodeCliRequest VmGuestExec(IReadOnlyList<string> args)
    {
        const string usage = "vm guest-exec <vm> --dry-run [--credential-ref REF] [--timeout-sec N] -- <command...>";
        if (args.Count < 6)
        {
            throw Usage("Use: " + usage + ".");
        }

        var delimiter = -1;
        for (var index = 3; index < args.Count; index++)
        {
            if (Is(args[index], "--"))
            {
                delimiter = index;
                break;
            }
        }

        if (delimiter < 0 || delimiter == args.Count - 1)
        {
            throw Usage("Use: " + usage + ".");
        }

        var parsed = ParseOptions(args.Skip(3).Take(delimiter - 3).ToArray(), allowFlags: true);
        var dryRun = HasFlag(parsed.Options, "--dry-run");
        RejectUnsupportedGuestExecOptions(parsed.Options, dryRun);
        if (!dryRun && string.IsNullOrWhiteSpace(FirstOption(parsed.Options, "--credential-ref")))
        {
            throw new ArgumentException(
                "PCV_CLI_CREDENTIAL_REF_REQUIRED|" +
                "Guest execution requires a protected credential reference.|" +
                "Use --credential-ref with a Windows Credential Manager or DPAPI reference.");
        }

        var body = new SortedDictionary<string, object?>
        {
            ["command"] = args.Skip(delimiter + 1).ToArray(),
        };
        if (dryRun)
        {
            body["dry_run"] = true;
            body["mode"] = "dry-run";
        }

        var credentialRef = FirstOption(parsed.Options, "--credential-ref");
        if (!string.IsNullOrWhiteSpace(credentialRef))
        {
            body["credential_ref"] = credentialRef;
        }

        var timeoutSec = FirstOption(parsed.Options, "--timeout-sec", "--timeout");
        if (!string.IsNullOrWhiteSpace(timeoutSec))
        {
            body["timeout_sec"] = ParseInt("--timeout-sec", timeoutSec);
        }

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/guest/exec{(dryRun ? "/preview" : string.Empty)}",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static void RejectUnsupportedGuestExecOptions(IReadOnlyDictionary<string, string?> options, bool dryRun)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "--credential-ref",
            "--timeout-sec",
            "--timeout"
        };
        if (dryRun)
        {
            allowed.Add("--dry-run");
        }

        foreach (var name in options.Keys)
        {
            if (allowed.Contains(name))
            {
                continue;
            }

            if (IsSecretLikeOption(name))
            {
                throw new ArgumentException(
                    "PCV_CLI_CREDENTIAL_REF_REQUIRED|" +
                    "Guest execution does not accept raw secret-bearing CLI options.|" +
                    "Use --credential-ref with a protected Windows Credential Manager or DPAPI reference.");
            }

            throw Usage($"Unsupported guest-exec option {name}.");
        }
    }

    private static bool IsSecretLikeOption(string name)
    {
        return name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("key", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("credential", StringComparison.OrdinalIgnoreCase);
    }

    private static DesktopNodeCliRequest VmLimit(IReadOnlyList<string> args)
    {
        if (args.Count < 5)
        {
            throw Usage("Use: vm limit <vm> --cpu N [--memory-mb MB].");
        }

        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: false);
        var body = new SortedDictionary<string, object?>();
        var cpu = FirstOption(parsed.Options, "--cpu", "--vcpu");
        if (!string.IsNullOrWhiteSpace(cpu))
        {
            body["cpu"] = ParseInt("--cpu", cpu);
        }

        var memoryMb = FirstOption(parsed.Options, "--memory-mb", "--memory_mb");
        if (!string.IsNullOrWhiteSpace(memoryMb))
        {
            body["memory_mb"] = ParseInt("--memory-mb", memoryMb);
        }

        if (body.Count == 0)
        {
            throw Usage("Use: vm limit <vm> --cpu N [--memory-mb MB].");
        }

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/limit",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmManage(IReadOnlyList<string> args)
    {
        if (args.Count != 4 || !Is(args[3], "--yes"))
        {
            throw new ArgumentException("PCV_CLI_CONFIRMATION_REQUIRED|VM manage requires explicit confirmation.|Use: vm manage <vm> --yes.");
        }

        var body = new SortedDictionary<string, object?>
        {
            ["confirm_name"] = args[2]
        };

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{Segment(args[2])}/manage",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmDelete(IReadOnlyList<string> args)
    {
        if (args.Count != 4 || !Is(args[3], "--yes"))
        {
            throw new ArgumentException("PCV_CLI_CONFIRMATION_REQUIRED|VM delete requires explicit confirmation.|Use: vm delete <vm> --yes.");
        }

        return new DesktopNodeCliRequest("DELETE", $"/api/v1/vms/{Segment(args[2])}");
    }

    private static DesktopNodeCliRequest VmCreate(IReadOnlyList<string> args)
    {
        string? positionalName = null;
        var optionStart = 2;
        if (args.Count > 2 && !args[2].StartsWith("--", StringComparison.Ordinal))
        {
            positionalName = args[2];
            optionStart = 3;
        }

        var parsed = ParseOptions(args.Skip(optionStart).ToArray(), allowFlags: false);
        var body = new SortedDictionary<string, object?>
        {
            ["cpu"] = RequiredInt(parsed.Options, "--cpu", "--vcpu"),
            ["disk_gb"] = RequiredInt(parsed.Options, "--disk-gb", "--disk_size_gb"),
            ["iso_path"] = Required(parsed.Options, "--iso", "--iso_path"),
            ["memory_mb"] = RequiredInt(parsed.Options, "--memory-mb", "--memory_mb"),
            ["name"] = positionalName ?? Required(parsed.Options, "--name")
        };

        var vmRoot = FirstOption(parsed.Options, "--vm-root", "--image_dir");
        if (!string.IsNullOrWhiteSpace(vmRoot))
        {
            body["vm_root"] = vmRoot;
        }

        if (parsed.Options.TryGetValue("--generation", out var generation) && !string.IsNullOrWhiteSpace(generation))
        {
            body["generation"] = ParseInt("--generation", generation);
        }

        return new DesktopNodeCliRequest("POST", "/api/v1/vms", JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest CheckpointRequest(IReadOnlyList<string> args)
    {
        if (args.Count < 4)
        {
            throw Usage("Use: vm checkpoint list|create|restore|delete ...");
        }

        var action = args[2].ToLowerInvariant();
        var vm = Segment(args[3]);
        return action switch
        {
            "list" => Fixed(args, 4, new DesktopNodeCliRequest("GET", $"/api/v1/vms/{vm}/checkpoints"), "vm checkpoint list <vm>"),
            "create" => CheckpointCreate(args, vm),
            "restore" or "rollback" => CheckpointAction(args, vm, "POST", "restore"),
            "delete" => CheckpointAction(args, vm, "DELETE", null),
            _ => throw Usage("Use: vm checkpoint list|create|restore|delete ...")
        };
    }

    private static DesktopNodeCliRequest CheckpointCreate(IReadOnlyList<string> args, string vm)
    {
        var parsed = ParseOptions(args.Skip(4).ToArray(), allowFlags: false);
        var body = new SortedDictionary<string, object?>
        {
            ["name"] = Required(parsed.Options, "--name")
        };

        return new DesktopNodeCliRequest("POST", $"/api/v1/vms/{vm}/checkpoints", JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest CheckpointAction(IReadOnlyList<string> args, string vm, string method, string? suffix)
    {
        RequireShape(args, 5, $"vm checkpoint {(suffix ?? "delete")} <vm> <checkpoint>");
        var path = $"/api/v1/vms/{vm}/checkpoints/{Segment(args[4])}";
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            path += "/" + suffix;
        }

        return new DesktopNodeCliRequest(method, path);
    }

    private static DesktopNodeCliRequest JobRequest(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            throw Usage("Use: job list|get|cancel|retry|reconcile.");
        }

        return args[1].ToLowerInvariant() switch
        {
            "list" => JobList(args),
            "get" => JobItem(args, "GET", null),
            "cancel" => JobItem(args, "POST", "cancel"),
            "retry" => JobItem(args, "POST", "retry"),
            "reconcile" => JobItem(args, "POST", "reconcile"),
            _ => throw Usage("Use: job list|get|cancel|retry|reconcile.")
        };
    }

    private static DesktopNodeCliRequest JobList(IReadOnlyList<string> args)
    {
        var parsed = ParseOptions(args.Skip(2).ToArray(), allowFlags: false);
        var query = new List<string>();
        if (parsed.Options.TryGetValue("--limit", out var limit) && !string.IsNullOrWhiteSpace(limit))
        {
            query.Add("limit=" + ParseInt("--limit", limit));
        }

        if (parsed.Options.TryGetValue("--offset", out var offset) && !string.IsNullOrWhiteSpace(offset))
        {
            query.Add("offset=" + ParseInt("--offset", offset));
        }

        var path = "/api/v1/jobs" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return new DesktopNodeCliRequest("GET", path);
    }

    private static DesktopNodeCliRequest JobItem(IReadOnlyList<string> args, string method, string? suffix)
    {
        RequireShape(args, 3, $"job {args[1]} <job_id>");
        var path = $"/api/v1/jobs/{Segment(args[2])}";
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            path += "/" + suffix;
        }

        return new DesktopNodeCliRequest(method, path);
    }

    private static DesktopNodeCliRequest DiagnosticsRequest(IReadOnlyList<string> args)
    {
        if (args.Count < 3 || !Is(args[1], "bundle"))
        {
            throw Usage("Use: diagnostics bundle list|create|download.");
        }

        return args[2].ToLowerInvariant() switch
        {
            "list" => DiagnosticsList(args),
            "create" => Fixed(args, 3, new DesktopNodeCliRequest("POST", "/api/v1/diagnostics/bundles"), "diagnostics bundle create"),
            "download" => DiagnosticsDownload(args),
            _ => throw Usage("Use: diagnostics bundle list|create|download.")
        };
    }

    private static DesktopNodeCliRequest DiagnosticsList(IReadOnlyList<string> args)
    {
        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: false);
        foreach (var option in parsed.Options.Keys)
        {
            if (!string.Equals(option, "--limit", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(option, "--offset", StringComparison.OrdinalIgnoreCase))
            {
                throw Usage($"Unsupported diagnostics bundle list option {option}.");
            }
        }

        var query = new List<string>();
        if (parsed.Options.TryGetValue("--limit", out var limit) && !string.IsNullOrWhiteSpace(limit))
        {
            query.Add("limit=" + ParseInt("--limit", limit));
        }

        if (parsed.Options.TryGetValue("--offset", out var offset) && !string.IsNullOrWhiteSpace(offset))
        {
            query.Add("offset=" + ParseInt("--offset", offset));
        }

        var path = "/api/v1/diagnostics/bundles" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        return new DesktopNodeCliRequest("GET", path);
    }

    private static DesktopNodeCliRequest DiagnosticsDownload(IReadOnlyList<string> args)
    {
        if (args.Count < 4)
        {
            throw Usage("Use: diagnostics bundle download <bundle_id> --output <path>.");
        }

        var parsed = ParseOptions(args.Skip(4).ToArray(), allowFlags: false);
        var outputPath = Required(parsed.Options, "--output");
        return new DesktopNodeCliRequest(
            "GET",
            $"/api/v1/diagnostics/bundles/{Segment(args[3])}/download",
            OutputPath: outputPath);
    }

    private static DesktopNodeCliRequest Fixed(IReadOnlyList<string> args, int count, DesktopNodeCliRequest request, string usage)
    {
        RequireShape(args, count, usage);
        return request;
    }

    private static void RequireShape(IReadOnlyList<string> args, int count, string usage)
    {
        if (args.Count != count)
        {
            throw Usage("Use: " + usage + ".");
        }
    }

    private static ParsedOptions ParseOptions(IReadOnlyList<string> args, bool allowFlags)
    {
        var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Count; index++)
        {
            var name = args[index];
            if (!name.StartsWith("--", StringComparison.Ordinal))
            {
                throw Usage($"Unexpected positional argument '{name}'.");
            }

            if (allowFlags && (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal)))
            {
                options[name] = "true";
                continue;
            }

            if (index + 1 >= args.Count)
            {
                throw Usage($"Missing value for {name}.");
            }

            options[name] = args[++index];
        }

        return new ParsedOptions(options);
    }

    private static string Required(IReadOnlyDictionary<string, string?> options, string name, params string[] aliases)
    {
        var value = FirstOption(options, [name, .. aliases]);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Usage($"Missing required option {name}.");
        }

        return value;
    }

    private static int RequiredInt(IReadOnlyDictionary<string, string?> options, string name, params string[] aliases)
    {
        return ParseInt(name, Required(options, name, aliases));
    }

    private static string? FirstOption(IReadOnlyDictionary<string, string?> options, params string[] names)
    {
        foreach (var name in names)
        {
            if (options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool HasFlag(IReadOnlyDictionary<string, string?> options, string name)
    {
        return options.TryGetValue(name, out var value) &&
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(string name, string value)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw Usage($"Option {name} must be an integer.");
        }

        return parsed;
    }

    private static bool Is(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static string Segment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Usage("Route segment values must not be empty.");
        }

        return Uri.EscapeDataString(value);
    }

    private static ArgumentException Usage(string message)
    {
        return new ArgumentException("PCV_CLI_USAGE|" + message);
    }

    private static DesktopNodeCliRequest BackendNotExposed(string operation)
    {
        throw new ArgumentException(
            "PCV_CLI_BACKEND_NOT_EXPOSED|" +
            $"operation={operation}|" +
            "Desktop Node Local API does not expose this Linux pcvctl-compatible route yet.|" +
            "next_slice=pcvcli-backend-command-gap-slice-2026-05-19");
    }

    public static string GetUsage()
    {
        return string.Join(Environment.NewLine, [
            "Usage:",
            "  pcvcli",
            "  pcvcli --interactive|-i",
            "  pcvcli [--api URL] [--format table|json|plain|csv] [--token TOKEN | --token-file PATH | --token-env NAME | --protected-token-file PATH] host status",
            "  token source omitted: uses %ProgramData%\\PureCVisor\\desktop-node\\api-token.dpapi.json when present",
            "  pcvcli runtime policy",
            "  pcvcli ops summary",
            "  pcvcli network inventory|list",
            "  pcvcli vm list|get|create|start|stop|shutdown|guest-shutdown|poweroff|restart|pause|resume|save|resume-saved|rename|console|vnc|memory-stats|cpu-stats",
            "  pcvcli vm limit <vm> --cpu N [--memory-mb MB]",
            "  pcvcli vm blkio-set <vm> --disk DISK --maximum-iops N [--minimum-iops N] --dry-run|--yes",
            "  pcvcli vm bandwidth-set <vm> --adapter ADAPTER --maximum-kbps N [--minimum-kbps N] --dry-run|--yes",
            "  pcvcli vm blkio-get|bandwidth|guest-agent-status|guest-ping <vm>",
            "  pcvcli vm guest-agent-ensure-channel <vm> --dry-run|--verify --credential-ref REF [--timeout-sec N]|--repair --yes",
            "  pcvcli vm guest-exec <vm> --dry-run [--credential-ref REF] [--timeout-sec N] -- <command...>",
            "  pcvcli vm guest-exec <vm> --credential-ref REF [--timeout-sec N] -- <command...>",
            "  pcvcli vm set-memory|set-vcpu|disk-resize <vm> <value>",
            "  pcvcli vm attach <vm> --iso <path>",
            "  pcvcli vm eject|delete-status <vm>",
            "  pcvcli vm manage <vm> --yes",
            "  pcvcli vm delete <vm> --yes",
            "  pcvcli vm checkpoint list|create|restore|delete",
            "  pcvcli vm snapshot list|create|rollback|delete",
            "  pcvcli job list|get|cancel|retry|reconcile",
            "  pcvcli diagnostics bundle list [--limit N] [--offset N]",
            "  pcvcli diagnostics bundle create",
            "  pcvcli diagnostics bundle download <bundle_id> --output <path>"
        ]);
    }

    private sealed record ParsedOptions(IReadOnlyDictionary<string, string?> Options);
}

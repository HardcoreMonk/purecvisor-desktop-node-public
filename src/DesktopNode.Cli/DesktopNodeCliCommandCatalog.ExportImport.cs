using System.Text.Json;

namespace DesktopNode.Cli;

public static partial class DesktopNodeCliCommandCatalog
{
    private static DesktopNodeCliRequest VmExport(IReadOnlyList<string> args)
    {
        if (args.Count >= 4 && Is(args[2], "preview"))
        {
            var previewVm = Segment(args[3]);
            var previewParsed = ParseOptions(args.Skip(4).ToArray(), allowFlags: false);
            var previewBody = new SortedDictionary<string, object?>
            {
                ["confirm_name"] = previewVm,
                ["directory"] = Required(previewParsed.Options, "--directory")
            };
            var previewRoot = FirstOption(previewParsed.Options, "--allowed-root");
            if (!string.IsNullOrWhiteSpace(previewRoot))
            {
                previewBody["allowed_root"] = previewRoot;
            }

            return new DesktopNodeCliRequest(
                "POST",
                $"/api/v1/vms/{previewVm}/export/preview",
                JsonSerializer.Serialize(previewBody, JsonOptions));
        }

        const string usage = "vm export <vm> --directory PATH --yes";
        if (args.Count < 3 || args[2].StartsWith("--", StringComparison.Ordinal))
        {
            throw Usage("Use: vm export preview <vm> --directory PATH | " + usage + ".");
        }

        var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: true);
        if (!HasFlag(parsed.Options, "--yes"))
        {
            throw new ArgumentException(
                "PCV_CLI_CONFIRMATION_REQUIRED|" +
                "VM export requires explicit confirmation.|" +
                "Use: pcvcli " + usage + ".");
        }

        var vm = Segment(args[2]);
        var body = new SortedDictionary<string, object?>
        {
            ["confirm_name"] = vm,
            ["directory"] = Required(parsed.Options, "--directory")
        };
        var allowedRoot = FirstOption(parsed.Options, "--allowed-root");
        if (!string.IsNullOrWhiteSpace(allowedRoot))
        {
            body["allowed_root"] = allowedRoot;
        }

        return new DesktopNodeCliRequest(
            "POST",
            $"/api/v1/vms/{vm}/export",
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static DesktopNodeCliRequest VmImport(IReadOnlyList<string> args)
    {
        if (args.Count >= 3 && Is(args[2], "preview"))
        {
            var previewParsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: true);
            var previewName = Required(previewParsed.Options, "--name");
            var previewBody = new SortedDictionary<string, object?>
            {
                ["confirm_name"] = previewName,
                ["directory"] = Required(previewParsed.Options, "--directory"),
                ["has_vmcx"] = HasFlag(previewParsed.Options, "--has-vmcx"),
                ["name"] = previewName,
                ["package_kind"] = FirstOption(previewParsed.Options, "--package-kind") ?? "hyperv-export"
            };
            var previewRoot = FirstOption(previewParsed.Options, "--allowed-root");
            if (!string.IsNullOrWhiteSpace(previewRoot))
            {
                previewBody["allowed_root"] = previewRoot;
            }

            return new DesktopNodeCliRequest(
                "POST",
                "/api/v1/vms/import/preview",
                JsonSerializer.Serialize(previewBody, JsonOptions));
        }

        const string usage = "vm import --name TARGET --directory PATH --yes";
        var parsed = ParseOptions(args.Skip(2).ToArray(), allowFlags: true);
        if (!HasFlag(parsed.Options, "--yes"))
        {
            throw new ArgumentException(
                "PCV_CLI_CONFIRMATION_REQUIRED|" +
                "VM import requires explicit confirmation.|" +
                "Use: pcvcli " + usage + ".");
        }

        var name = Required(parsed.Options, "--name");
        var body = new SortedDictionary<string, object?>
        {
            ["confirm_name"] = name,
            ["directory"] = Required(parsed.Options, "--directory"),
            ["has_vmcx"] = true,
            ["name"] = name,
            ["package_kind"] = FirstOption(parsed.Options, "--package-kind") ?? "hyperv-export"
        };
        var allowedRoot = FirstOption(parsed.Options, "--allowed-root");
        if (!string.IsNullOrWhiteSpace(allowedRoot))
        {
            body["allowed_root"] = allowedRoot;
        }

        var vmRoot = FirstOption(parsed.Options, "--vm-root");
        if (!string.IsNullOrWhiteSpace(vmRoot))
        {
            body["vm_root"] = vmRoot;
        }

        return new DesktopNodeCliRequest(
            "POST",
            "/api/v1/vms/import",
            JsonSerializer.Serialize(body, JsonOptions));
    }
}

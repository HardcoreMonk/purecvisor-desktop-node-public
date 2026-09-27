using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiVmExportImportPreviewTests
{
    [Fact]
    public void ExportPreviewAcceptsManagedGen2OffVmWithoutExporting()
    {
        var nativeCalls = new List<string>();
        using var root = new TempExportRoot();
        var processor = CreateProcessor(nativeCalls);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/export/preview",
            JsonSerializer.Serialize(new
            {
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("vm.export.preview", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("export-preview", data.GetProperty("action").GetString());
        Assert.True(data.GetProperty("dry_run").GetBoolean());
        Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
        Assert.Equal("lab-vm", data.GetProperty("vm_name").GetString());
        Assert.Equal(VmExportImportPolicy.PackageHyperVExport, data.GetProperty("package_kind").GetString());
        Assert.Equal(VmExportImportPolicy.Schema, data.GetProperty("schema").GetString());
        Assert.False(Directory.EnumerateFileSystemEntries(root.Directory).Any());
    }

    [Fact]
    public void ExportPreviewRejectsUnmanagedAndTpmWithoutNativeExport()
    {
        using var root = new TempExportRoot();
        var unmanaged = CreateProcessor([], managed: false);
        var unmanagedResponse = unmanaged.Handle(ExportRequest(root));
        Assert.Equal(400, unmanagedResponse.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.NotManaged, unmanagedResponse.Body, StringComparison.Ordinal);

        var tpm = CreateProcessor([], securityFeatures: true);
        var tpmResponse = tpm.Handle(ExportRequest(root));
        Assert.Equal(400, tpmResponse.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.SecurityFeaturesUnsupported, tpmResponse.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportPreviewAcceptsHyperVExportFactsWithoutDefiningVm()
    {
        var nativeCalls = new List<string>();
        using var root = new TempExportRoot();
        var processor = CreateProcessor(nativeCalls);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import/preview",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm-restored",
                confirm_name = "lab-vm-restored",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "hyperv-export",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("vm.import.preview", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("import-preview", data.GetProperty("action").GetString());
        Assert.True(data.GetProperty("dry_run").GetBoolean());
        Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
        Assert.True(data.GetProperty("generate_new_id").GetBoolean());
        Assert.True(data.GetProperty("apply_managed_marker").GetBoolean());
        Assert.Equal("lab-vm-restored", data.GetProperty("vm_name").GetString());
    }

    [Fact]
    public void ImportPreviewRejectsOvfAndExistingTarget()
    {
        using var root = new TempExportRoot();
        var processor = CreateProcessor([]);
        var ovf = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import/preview",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm-restored",
                confirm_name = "lab-vm-restored",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "ovf",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));
        Assert.Equal(400, ovf.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.OvfForbidden, ovf.Body, StringComparison.Ordinal);

        var existing = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import/preview",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm",
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "hyperv-export",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));
        Assert.Equal(409, existing.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.AlreadyExists, existing.Body, StringComparison.Ordinal);
    }

    private static DesktopNodeApiRequest ExportRequest(TempExportRoot root)
    {
        return new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/export/preview",
            JsonSerializer.Serialize(new
            {
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory
            }),
            ServiceBearerAccepted: true);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        List<string> nativeCalls,
        bool managed = true,
        bool securityFeatures = false,
        string state = "off")
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"{{state}}","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":0},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"security_features_present":{{securityFeatures.ToString().ToLowerInvariant()}}}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingExportNativeAdapter(nativeCalls, vmJson));
    }

    private sealed class RecordingExportNativeAdapter(List<string> calls, string vmListJson) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(operation);
            if (string.Equals(operation, "vm.list", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(vmListJson);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Export/import preview must not invoke Hyper-V export or import.",
                false);
            return false;
        }
    }

    private sealed class TempExportRoot : IDisposable
    {
        public string Directory { get; }

        public TempExportRoot()
        {
            Directory = Path.Combine(Path.GetTempPath(), "pcv-export-preview-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Directory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}

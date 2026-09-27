using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVGuestFileJobTests
{
    [Fact]
    public void PreviewAcceptsAllowlistedHostFileWithoutCopying()
    {
        var hostRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PureCVisor",
            "desktop-node",
            "guest-files");
        Directory.CreateDirectory(hostRoot);
        var hostPath = Path.Combine(hostRoot, "pcv-guest-file-preview.bin");
        File.WriteAllBytes(hostPath, new byte[1024]);
        try
        {
            var provider = new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider();
            var info = provider.InvokeFile(
                new DesktopNodeHyperVGuestFileRequest(
                    "vm.guest.file.preview",
                    "alpha",
                    "wincred:PureCVisor/guest/admin",
                    hostPath,
                    @"C:\Users\Public\PureCVisor\payload.bin",
                    1024,
                    "host-to-guest",
                    null,
                    60),
                CancellationToken.None);

            Assert.Equal("vm.guest.file.preview", info.Operation);
            Assert.Equal("host-to-guest", info.Direction);
            Assert.Equal(1024, info.SizeBytes);
            Assert.False(info.Copied);
            Assert.False(info.HostMutationPerformed);
        }
        finally
        {
            File.Delete(hostPath);
        }
    }

    [Fact]
    public void CopyUsesInjectedCopierAfterAllowlist()
    {
        var hostPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PureCVisor",
            "desktop-node",
            "guest-files",
            "payload.bin");
        var copier = new RecordingFileCopier();
        var provider = new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider(
            new FixedCredentialResolver(),
            new UnusedTransport(),
            new FixedFileHost(2048),
            copier);

        var info = provider.InvokeFile(
            new DesktopNodeHyperVGuestFileRequest(
                "vm.guest.file",
                "alpha",
                "wincred:PureCVisor/guest/admin",
                hostPath,
                @"C:\Users\Public\PureCVisor\payload.bin",
                2048,
                "host-to-guest",
                null,
                30),
            CancellationToken.None);

        Assert.True(info.Copied);
        Assert.Equal(Path.GetFullPath(hostPath), copier.HostPath);
        Assert.Equal(Path.GetFullPath(@"C:\Users\Public\PureCVisor\payload.bin"), copier.GuestPath);
        Assert.Equal("alpha", copier.VmName);
    }

    [Fact]
    public void RejectsHgfsSharedFolder()
    {
        var provider = new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider(
            new FixedCredentialResolver(),
            new UnusedTransport(),
            new FixedFileHost(1024),
            new RecordingFileCopier());

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(() =>
            provider.InvokeFile(
                new DesktopNodeHyperVGuestFileRequest(
                    "vm.guest.file.preview",
                    "alpha",
                    "wincred:PureCVisor/guest/admin",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "PureCVisor",
                        "desktop-node",
                        "guest-files",
                        "payload.bin"),
                    @"C:\Users\Public\PureCVisor\payload.bin",
                    1024,
                    "host-to-guest",
                    "hgfs-share",
                    60),
                CancellationToken.None));

        Assert.Equal("PCV_GUEST_FILE_HGFS_FORBIDDEN", error.Code);
    }

    [Fact]
    public void AdapterDispatchesGuestFilePreview()
    {
        var hostRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PureCVisor",
            "desktop-node",
            "guest-files");
        Directory.CreateDirectory(hostRoot);
        var hostPath = Path.Combine(hostRoot, "pcv-guest-file-adapter.bin");
        File.WriteAllBytes(hostPath, new byte[512]);
        try
        {
            var adapter = DesktopNodeHyperVNativeAdapter.CreateDefault();
            var payload = JsonSerializer.Serialize(new
            {
                name = "alpha",
                credential_ref = "wincred:PureCVisor/guest/admin",
                host_path = hostPath,
                guest_path = @"C:\Users\Public\PureCVisor\payload.bin",
                size_bytes = 512
            });
            using var document = JsonDocument.Parse(payload);
            var handled = adapter.TryInvoke("vm.guest.file.preview", document.RootElement, CancellationToken.None, out var result);

            Assert.True(handled);
            Assert.True(result.Ok);
            Assert.Equal("vm.guest.file.preview", result.Operation);
            Assert.False(result.Data!.Value.GetProperty("copied").GetBoolean());
            Assert.Equal(512, result.Data.Value.GetProperty("size_bytes").GetInt64());
        }
        finally
        {
            File.Delete(hostPath);
        }
    }

    private sealed class RecordingFileCopier : IDesktopNodeHyperVGuestFileCopier
    {
        public string? VmName { get; private set; }
        public string? HostPath { get; private set; }
        public string? GuestPath { get; private set; }

        public void CopyToGuest(
            string vmName,
            string username,
            string password,
            string hostPath,
            string guestPath,
            int timeoutSeconds,
            CancellationToken cancellationToken)
        {
            VmName = vmName;
            HostPath = hostPath;
            GuestPath = guestPath;
        }
    }

    private sealed class FixedFileHost(long length) : IDesktopNodeHyperVGuestFileHost
    {
        public bool TryGetLength(string path, out long actual)
        {
            actual = length;
            return true;
        }
    }

    private sealed class FixedCredentialResolver : IDesktopNodeHyperVGuestCredentialResolver
    {
        public DesktopNodeHyperVGuestCredentialResolution Resolve(string? credentialRef)
        {
            return new DesktopNodeHyperVGuestCredentialResolution(
                true,
                new DesktopNodeHyperVGuestCredential("user", "secret"),
                null);
        }
    }

    private sealed class UnusedTransport : IDesktopNodeHyperVGuestExecutionTransport
    {
        public DesktopNodeHyperVGuestExecutionTransportResult Invoke(
            DesktopNodeHyperVGuestExecutionTransportRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}

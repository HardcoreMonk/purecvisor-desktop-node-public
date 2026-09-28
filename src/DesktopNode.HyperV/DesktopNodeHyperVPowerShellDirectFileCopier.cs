using System.Diagnostics;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.HyperV;

internal sealed class DesktopNodeHyperVPowerShellDirectFileCopier : IDesktopNodeHyperVGuestFileCopier
{
    // Copy-Item -ToSession does not create the destination directory, so the allowlisted parent is created in the
    // same PowerShell Direct session first. A fresh guest has no C:\Users\Public\PureCVisor\ yet.
    internal const string CopyBridgeScript = """
        $payload = $pcvIn.ReadToEnd() | ConvertFrom-Json
        $secure = ConvertTo-SecureString -String $payload.password -AsPlainText -Force
        $credential = [pscredential]::new([string]$payload.username, $secure)
        $session = New-PSSession -VMName ([string]$payload.vm_name) -Credential $credential
        try {
          Invoke-Command -Session $session -ArgumentList ([string]$payload.guest_parent) -ScriptBlock {
            param([string]$parent)
            [void][System.IO.Directory]::CreateDirectory($parent)
          } -ErrorAction Stop
          Copy-Item -ToSession $session -Path ([string]$payload.host_path) -Destination ([string]$payload.guest_path) -ErrorAction Stop
        }
        finally {
          if ($session) { Remove-PSSession $session }
        }
        """;

    public void CopyToGuest(
        string vmName,
        string username,
        string password,
        string hostPath,
        string guestPath,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            vm_name = vmName,
            username,
            password,
            host_path = hostPath,
            guest_path = guestPath,
            guest_parent = ResolveGuestParent(guestPath)
        });
        var bridgeScript = DesktopNodeHyperVPowerShellDirectTransport.WrapWithUtf8Streams(CopyBridgeScript);
        using var process = new Process();
        process.StartInfo = DesktopNodeHyperVPowerShellDirectTransport.BuildBridgeStartInfo(bridgeScript);
        process.Start();
        using var cancellationRegistration = cancellationToken.Register(static state =>
        {
            try
            {
                ((Process)state!).Kill(entireProcessTree: true);
            }
            catch
            {
            }
        }, process);

        process.StandardInput.Write(payload);
        process.StandardInput.Close();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        var timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 1, 600));
        if (!process.WaitForExit(timeout))
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            try { process.WaitForExit(); } catch { }
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_FILE_COPY_FAILED",
                "Guest file copy timed out.",
                "Increase timeout_sec or copy a smaller file.",
                false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (process.ExitCode != 0)
        {
            var stderr = stderrTask.GetAwaiter().GetResult();
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_FILE_COPY_FAILED",
                "PowerShell Direct Copy-Item to the guest failed.",
                string.IsNullOrWhiteSpace(stderr) ? stdoutTask.GetAwaiter().GetResult() : stderr,
                false);
        }
    }

    internal static string ResolveGuestParent(string guestPath)
    {
        var parent = Path.GetDirectoryName(guestPath);
        if (string.IsNullOrWhiteSpace(parent) ||
            !(parent.TrimEnd('\\') + "\\").StartsWith(GuestFileJobContract.GuestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                GuestFileJobProblemCodes.PathNotAllowed,
                "The guest destination directory is outside the allowlist prefix.",
                $"Copy only under {GuestFileJobContract.GuestPrefix}.",
                false);
        }

        return parent;
    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVPowerShellDirectGuestExecutionProvider : IDesktopNodeHyperVGuestExecutionProvider
{
    private const string TransportName = "windows-powershell-direct";
    private readonly IDesktopNodeHyperVGuestCredentialResolver credentialResolver;
    private readonly IDesktopNodeHyperVGuestExecutionTransport transport;

    public DesktopNodeHyperVPowerShellDirectGuestExecutionProvider()
        : this(new DesktopNodeHyperVGuestCredentialResolver(), new DesktopNodeHyperVPowerShellDirectTransport())
    {
    }

    internal DesktopNodeHyperVPowerShellDirectGuestExecutionProvider(
        IDesktopNodeHyperVGuestCredentialResolver credentialResolver,
        IDesktopNodeHyperVGuestExecutionTransport transport)
    {
        this.credentialResolver = credentialResolver;
        this.transport = transport;
    }

    public DesktopNodeHyperVGuestExecutionInfo Invoke(
        DesktopNodeHyperVGuestExecutionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        return request.Operation switch
        {
            "vm.guest.exec" => Execute(request, cancellationToken),
            "vm.guest.channel.verify" => Verify(request, cancellationToken),
            "vm.guest.channel.ensure" => Ensure(request),
            _ => throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_EXEC_OPERATION_UNSUPPORTED",
                $"Guest execution provider does not support operation '{request.Operation}'.",
                "Use vm.guest.exec, vm.guest.channel.verify, or vm.guest.channel.ensure.",
                false)
        };
    }

    private DesktopNodeHyperVGuestExecutionInfo Execute(
        DesktopNodeHyperVGuestExecutionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Command.Count == 0)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_EXEC_COMMAND_REQUIRED",
                "Guest execution requires a non-empty command.",
                "Pass a command array through the queued guest execution API route.",
                false);
        }

        var credential = ResolveCredential(request.CredentialRef);
        var output = transport.Invoke(
            new DesktopNodeHyperVGuestExecutionTransportRequest(
                request.Name,
                credential.Username,
                credential.Password,
                request.Command,
                request.Environment,
                request.TimeoutSeconds),
            cancellationToken);
        return BuildInfo(request, output);
    }

    private DesktopNodeHyperVGuestExecutionInfo Verify(
        DesktopNodeHyperVGuestExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var credential = ResolveCredential(request.CredentialRef);
        var output = transport.Invoke(
            new DesktopNodeHyperVGuestExecutionTransportRequest(
                request.Name,
                credential.Username,
                credential.Password,
                ["$PSVersionTable.PSVersion.ToString()"],
                new Dictionary<string, string>(),
                request.TimeoutSeconds),
            cancellationToken);
        return BuildInfo(request, output);
    }

    private static DesktopNodeHyperVGuestExecutionInfo Ensure(DesktopNodeHyperVGuestExecutionRequest request)
    {
        if (!request.RepairConfirmed)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_CHANNEL_REPAIR_CONFIRMATION_REQUIRED",
                "Guest channel repair requires explicit confirmation.",
                "Pass yes=true through the API or --repair --yes through pcvcli.",
                false);
        }

        return new DesktopNodeHyperVGuestExecutionInfo(
            request.Name,
            request.Operation,
            TransportName,
            "host_prerequisite_noop",
            HostMutationPerformed: false,
            GuestCommandPerformed: false,
            request.TimeoutSeconds,
            Evidence: new SortedDictionary<string, object?>
            {
                ["channel_contract"] = "powershell-direct-host-prerequisite-v1",
                ["reason"] = "PowerShell Direct does not require an in-guest qemu-agent channel or host-side repair in this product slice."
            });
    }

    private DesktopNodeHyperVGuestCredential ResolveCredential(string? credentialRef)
    {
        var credential = credentialResolver.Resolve(credentialRef);
        if (!credential.Ok || credential.Secret is null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                credential.ErrorCode ?? "PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED",
                "Guest execution requires a protected credential reference.",
                "Use wincred:<target> or dpapi:<path> containing a username/password JSON payload.",
                false);
        }

        return credential.Secret;
    }

    private static DesktopNodeHyperVGuestExecutionInfo BuildInfo(
        DesktopNodeHyperVGuestExecutionRequest request,
        DesktopNodeHyperVGuestExecutionTransportResult output)
    {
        var terminalState = output.TimedOut
            ? "timed_out"
            : output.ExitCode == 0 ? "succeeded" : "failed";
        return new DesktopNodeHyperVGuestExecutionInfo(
            request.Name,
            request.Operation,
            TransportName,
            terminalState,
            HostMutationPerformed: false,
            GuestCommandPerformed: true,
            request.TimeoutSeconds,
            output.ExitCode,
            output.StdoutByteCount,
            output.StderrByteCount,
            Hash(output.Stdout),
            Hash(output.Stderr),
            Audit: new SortedDictionary<string, object?>
            {
                ["schema_version"] = "guest-execution-audit-v1",
                ["request_id"] = request.RequestId,
                ["actor"] = request.Actor,
                ["terminal_state"] = terminalState,
                ["stdout_byte_count"] = output.StdoutByteCount,
                ["stderr_byte_count"] = output.StderrByteCount,
                ["stdout_digest"] = Hash(output.Stdout),
                ["stderr_digest"] = Hash(output.Stderr)
            });
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

internal interface IDesktopNodeHyperVGuestCredentialResolver
{
    DesktopNodeHyperVGuestCredentialResolution Resolve(string? credentialRef);
}

internal sealed record DesktopNodeHyperVGuestCredentialResolution(
    bool Ok,
    DesktopNodeHyperVGuestCredential? Secret,
    string? ErrorCode);

internal sealed record DesktopNodeHyperVGuestCredential(string Username, string Password);

internal sealed class DesktopNodeHyperVGuestCredentialResolver : IDesktopNodeHyperVGuestCredentialResolver
{
    public DesktopNodeHyperVGuestCredentialResolution Resolve(string? credentialRef)
    {
        if (string.IsNullOrWhiteSpace(credentialRef))
        {
            return Reject();
        }

        var trimmed = credentialRef.Trim();
        if (TryStripPrefix(trimmed, "wincred:", out var target) ||
            TryStripPrefix(trimmed, "credential-manager:", out target))
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return Reject();
            }

            return ParseSecret(ReadCredentialManagerSecret(target));
        }

        if (TryStripPrefix(trimmed, "dpapi:", out var path))
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return Reject();
            }

            var protectedBytes = Convert.FromBase64String(File.ReadAllText(path));
            var clearBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);
            try
            {
                return ParseSecret(Encoding.UTF8.GetString(clearBytes));
            }
            finally
            {
                Array.Clear(clearBytes);
            }
        }

        return Reject();
    }

    private static DesktopNodeHyperVGuestCredentialResolution ParseSecret(string secret)
    {
        try
        {
            using var document = JsonDocument.Parse(secret);
            var root = document.RootElement;
            var username = root.TryGetProperty("username", out var usernameElement)
                ? usernameElement.GetString()
                : null;
            var password = root.TryGetProperty("password", out var passwordElement)
                ? passwordElement.GetString()
                : null;
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrEmpty(password))
            {
                return new DesktopNodeHyperVGuestCredentialResolution(
                    true,
                    new DesktopNodeHyperVGuestCredential(username!, password!),
                    null);
            }
        }
        catch (JsonException)
        {
            var parts = secret.Split(["\r\n", "\n"], 2, StringSplitOptions.None);
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrEmpty(parts[1]))
            {
                return new DesktopNodeHyperVGuestCredentialResolution(
                    true,
                    new DesktopNodeHyperVGuestCredential(parts[0].Trim(), parts[1]),
                    null);
            }
        }

        return Reject();
    }

    private static bool TryStripPrefix(string value, string prefix, out string stripped)
    {
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            stripped = value[prefix.Length..];
            return true;
        }

        stripped = string.Empty;
        return false;
    }

    private static DesktopNodeHyperVGuestCredentialResolution Reject()
    {
        return new DesktopNodeHyperVGuestCredentialResolution(
            false,
            null,
            "PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED");
    }

    private static string ReadCredentialManagerSecret(string target)
    {
        if (!CredRead(target, 1, 0, out var credentialPointer))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED",
                "Windows Credential Manager could not read the guest credential reference.",
                $"Credential Manager target read failed: {new Win32Exception(Marshal.GetLastWin32Error()).Message}",
                false);
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
            var bytes = new byte[credential.CredentialBlobSize];
            Marshal.Copy(credential.CredentialBlob, bytes, 0, bytes.Length);
            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            finally
            {
                Array.Clear(bytes);
            }
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredRead(string targetName, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", SetLastError = false)]
    private static extern void CredFree(IntPtr credential);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }
}

internal interface IDesktopNodeHyperVGuestExecutionTransport
{
    DesktopNodeHyperVGuestExecutionTransportResult Invoke(
        DesktopNodeHyperVGuestExecutionTransportRequest request,
        CancellationToken cancellationToken);
}

internal sealed record DesktopNodeHyperVGuestExecutionTransportRequest(
    string VmName,
    string Username,
    string Password,
    IReadOnlyList<string> Command,
    IReadOnlyDictionary<string, string> Environment,
    int TimeoutSeconds);

internal sealed record DesktopNodeHyperVGuestExecutionTransportResult(
    int ExitCode,
    bool TimedOut,
    string Stdout,
    string Stderr)
{
    public int StdoutByteCount => Encoding.UTF8.GetByteCount(Stdout ?? string.Empty);

    public int StderrByteCount => Encoding.UTF8.GetByteCount(Stderr ?? string.Empty);
}

internal sealed class DesktopNodeHyperVPowerShellDirectTransport : IDesktopNodeHyperVGuestExecutionTransport
{
    // The credential and command payload crosses this boundary as stdin text, and the
    // guest's output comes back the same way. Without explicit encodings .NET falls back
    // to the console code page, which on Windows is an OEM page rather than UTF-8, so a
    // non-ASCII username, password, command or output can be mangled in transit. A
    // mangled password surfaces as an authentication failure, which is hard to trace back
    // to encoding. The bridge script itself is already safe because -EncodedCommand takes
    // UTF-16LE base64.
    // Pinning the .NET-side stream encodings is necessary but not sufficient: powershell.exe
    // still reads [Console]::In and writes [Console]::Out through the console code page, so a
    // non-ASCII payload is mangled on the PowerShell side even when the parent hands it clean
    // UTF-8. Both ends open explicit UTF-8 streams instead. Console.OutputEncoding is set on a
    // best-effort basis so PowerShell's own error records reach stderr as UTF-8 too.
    internal static string WrapWithUtf8Streams(string body) => $$"""
        $pcvIn = [System.IO.StreamReader]::new([System.Console]::OpenStandardInput(), [System.Text.UTF8Encoding]::new($false))
        $pcvOut = [System.IO.StreamWriter]::new([System.Console]::OpenStandardOutput(), [System.Text.UTF8Encoding]::new($false))
        try { [System.Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false) } catch { }
        try {
        {{body}}
        }
        finally { $pcvOut.Flush() }
        """;

    // The guest-side invocation. argv arrives as data and stays data: element 0 is the command
    // and the rest are its arguments, splatted so PowerShell never re-parses their contents.
    //
    // This replaced `[scriptblock]::Create($payload.command -join ' ')`, which concatenated argv
    // into one string and re-parsed it as a script in the guest. That silently split any argument
    // containing a space and executed anything a caller put in one: `$(...)` was evaluated and
    // `;` started a second statement. It is also what made FC-12(b)'s non-ASCII round trip return
    // a byte count matching neither UTF-8 nor the OEM page - the sample was being split on its
    // spaces into separate output lines, not mangled by an encoding.
    //
    // Kept as a named constant so the argv-fidelity contract can be exercised without a Hyper-V
    // guest: fidelity is decided here, before the guest is ever contacted.
    //
    // The length-1 branch is not redundant. PowerShell ranges count downward, so `$argv[1..0]` on
    // a single-element array yields @($argv[1], $argv[0]) - it would pass the command to itself.
    internal const string GuestArgvInvocation = """
        param([string[]]$argv)
        if ($argv.Length -eq 1) { & $argv[0] }
        else { & $argv[0] @($argv[1..($argv.Length - 1)]) }
        """;

    internal static ProcessStartInfo BuildBridgeStartInfo(string bridgeScript) =>
        new()
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " +
                Convert.ToBase64String(Encoding.Unicode.GetBytes(bridgeScript)),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            UseShellExecute = false,
            CreateNoWindow = true
        };

    public DesktopNodeHyperVGuestExecutionTransportResult Invoke(
        DesktopNodeHyperVGuestExecutionTransportRequest request,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            vm_name = request.VmName,
            username = request.Username,
            password = request.Password,
            command = request.Command,
            environment = request.Environment
        });
        var bridgeScript = WrapWithUtf8Streams($$"""
        $payload = $pcvIn.ReadToEnd() | ConvertFrom-Json
        $secure = ConvertTo-SecureString -String $payload.password -AsPlainText -Force
        $credential = [pscredential]::new([string]$payload.username, $secure)
        $pcvArgv = [string[]]$payload.command
        $pcvInvoke = {
        {{GuestArgvInvocation}}
        }
        $pcvOut.Write((Invoke-Command -VMName ([string]$payload.vm_name) -Credential $credential -ArgumentList (, $pcvArgv) -ScriptBlock $pcvInvoke | Out-String -Width 4096))
        """);
        using var process = new Process();
        process.StartInfo = BuildBridgeStartInfo(bridgeScript);

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

        try
        {
            process.StandardInput.Write(payload);
            process.StandardInput.Close();
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var timeout = TimeSpan.FromSeconds(Math.Clamp(request.TimeoutSeconds, 1, 600));
            if (!process.WaitForExit(timeout))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                try { process.WaitForExit(); } catch { }
                return new DesktopNodeHyperVGuestExecutionTransportResult(
                    ExitCode: -1,
                    TimedOut: true,
                    stdoutTask.GetAwaiter().GetResult(),
                    stderrTask.GetAwaiter().GetResult());
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new DesktopNodeHyperVGuestExecutionTransportResult(
                process.ExitCode,
                TimedOut: false,
                stdoutTask.GetAwaiter().GetResult(),
                stderrTask.GetAwaiter().GetResult());
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }
}

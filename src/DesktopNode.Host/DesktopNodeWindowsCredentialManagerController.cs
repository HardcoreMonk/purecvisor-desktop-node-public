using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace DesktopNode.Host;

public sealed record DesktopNodeWindowsCredentialManagerProofSnapshot(
    string Identity,
    string CredentialTarget,
    string CredentialWriteStatus,
    string CredentialReadStatus,
    string CredentialDeleteStatus,
    bool TokenValueObserved,
    bool NewTokenValueCreated);

public interface IDesktopNodeWindowsCredentialManagerController
{
    DesktopNodeWindowsCredentialManagerProofSnapshot WriteReadDeleteProof(string credentialTarget);
    void WriteToken(string credentialTarget, string token);
    string ReadToken(string credentialTarget);
    void DeleteToken(string credentialTarget);
}

public sealed class DesktopNodeWindowsCredentialManagerController : IDesktopNodeWindowsCredentialManagerController
{
    private const uint CredentialTypeGeneric = 1;
    private const uint CredentialPersistLocalMachine = 2;

    public DesktopNodeWindowsCredentialManagerProofSnapshot WriteReadDeleteProof(string credentialTarget)
    {
        var identity = WindowsIdentity.GetCurrent().Name;
        var token = CreateToken();
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var writeStatus = "not-run";
        var readStatus = "not-run";
        var deleteStatus = "not-run";

        try
        {
            WriteCredential(credentialTarget, tokenBytes);
            writeStatus = "pass";
            var readBytes = ReadCredentialBytes(credentialTarget);
            if (!CryptographicOperations.FixedTimeEquals(tokenBytes, readBytes))
            {
                throw new DesktopNodeWindowsCredentialManagerControllerException(
                    "PCV_HOST_CREDENTIAL_MANAGER_READ_MISMATCH",
                    $"Windows Credential Manager proof target '{credentialTarget}' returned an unexpected blob.");
            }

            readStatus = "pass";
        }
        finally
        {
            Array.Clear(tokenBytes);
            if (writeStatus == "pass")
            {
                DeleteToken(credentialTarget);
                deleteStatus = "pass";
            }
        }

        return new DesktopNodeWindowsCredentialManagerProofSnapshot(
            Identity: identity,
            CredentialTarget: credentialTarget,
            CredentialWriteStatus: writeStatus,
            CredentialReadStatus: readStatus,
            CredentialDeleteStatus: deleteStatus,
            TokenValueObserved: false,
            NewTokenValueCreated: true);
    }

    public void WriteToken(string credentialTarget, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DesktopNodeWindowsCredentialManagerControllerException(
                "PCV_HOST_CREDENTIAL_MANAGER_TOKEN_EMPTY",
                $"Windows Credential Manager target '{credentialTarget}' cannot be written with an empty token.");
        }

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        try
        {
            WriteCredential(credentialTarget, tokenBytes);
        }
        finally
        {
            Array.Clear(tokenBytes);
        }
    }

    public string ReadToken(string credentialTarget)
    {
        var tokenBytes = ReadCredentialBytes(credentialTarget);
        try
        {
            return Encoding.UTF8.GetString(tokenBytes);
        }
        finally
        {
            Array.Clear(tokenBytes);
        }
    }

    public void DeleteToken(string credentialTarget)
    {
        if (!CredDelete(credentialTarget, CredentialTypeGeneric, 0))
        {
            throw NewCredentialException("PCV_HOST_CREDENTIAL_MANAGER_DELETE_FAILED", credentialTarget);
        }
    }

    private static void WriteCredential(string credentialTarget, byte[] tokenBytes)
    {
        var credentialBlob = Marshal.AllocHGlobal(tokenBytes.Length);
        try
        {
            Marshal.Copy(tokenBytes, 0, credentialBlob, tokenBytes.Length);
            var credential = new NativeCredential
            {
                Type = CredentialTypeGeneric,
                TargetName = credentialTarget,
                CredentialBlobSize = (uint)tokenBytes.Length,
                CredentialBlob = credentialBlob,
                Persist = CredentialPersistLocalMachine,
                UserName = "PureCVisorDesktopNode"
            };

            if (!CredWrite(ref credential, 0))
            {
                throw NewCredentialException("PCV_HOST_CREDENTIAL_MANAGER_WRITE_FAILED", credentialTarget);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(credentialBlob);
        }
    }

    private static byte[] ReadCredentialBytes(string credentialTarget)
    {
        if (!CredRead(credentialTarget, CredentialTypeGeneric, 0, out var credentialPointer))
        {
            throw NewCredentialException("PCV_HOST_CREDENTIAL_MANAGER_READ_FAILED", credentialTarget);
        }

        try
        {
            var readCredential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
            var readBytes = new byte[readCredential.CredentialBlobSize];
            Marshal.Copy(readCredential.CredentialBlob, readBytes, 0, readBytes.Length);
            return readBytes;
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    private static string CreateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static DesktopNodeWindowsCredentialManagerControllerException NewCredentialException(string code, string credentialTarget)
    {
        return new DesktopNodeWindowsCredentialManagerControllerException(
            code,
            $"Windows Credential Manager operation failed for target '{credentialTarget}': {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredRead(string targetName, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredDelete(string targetName, uint type, uint flags);

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

public sealed class DesktopNodeWindowsCredentialManagerControllerException : Exception
{
    public DesktopNodeWindowsCredentialManagerControllerException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

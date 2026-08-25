using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVNativeOperationException(
    string code,
    string message,
    string detail,
    bool retryable) : Exception(message)
{
    public string Code { get; } = code;

    public string Detail { get; } = detail;

    public bool Retryable { get; } = retryable;
}

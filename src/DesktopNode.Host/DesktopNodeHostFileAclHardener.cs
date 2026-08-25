using System.Security.AccessControl;
using System.Security.Principal;

namespace DesktopNode.Host;

internal interface IDesktopNodeHostFileAclHardener
{
    void Harden(string path);
}

internal sealed class DesktopNodeHostFileAclHardener : IDesktopNodeHostFileAclHardener
{
    public static DesktopNodeHostFileAclHardener Instance { get; } = new();

    private DesktopNodeHostFileAclHardener()
    {
    }

    public void Harden(string path)
    {
        var fileInfo = new FileInfo(path);
        var security = new FileSecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            FileSystemRights.Read,
            AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            FileSystemRights.Read,
            AccessControlType.Allow));
        fileInfo.SetAccessControl(security);
    }
}

using System.Globalization;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider
{
    public const string ThumbnailImageMethod = "GetVirtualSystemThumbnailImage";

    // vm.console.frame: 실현된 설정의 화면을 요청 크기의 RGB565 이미지로 읽는다(2026-10-08 spike에서 640×480 37~59ms).
    public byte[]? GetConsoleFrame(string vmId, int width, int height, CancellationToken cancellationToken)
    {
        var scope = CreateScope();
        using var vm = FindVm(scope, vmId, cancellationToken);
        if (vm is null)
        {
            return null;
        }

        using var settings = GetRealizedSettings(vm, cancellationToken);
        if (settings is null)
        {
            return null;
        }

        using var service = GetService(scope, "Msvm_VirtualSystemManagementService", cancellationToken);
        using var inParams = service.GetMethodParameters(ThumbnailImageMethod);
        inParams["TargetSystem"] = settings.Path.Path;
        inParams["WidthPixels"] = (ushort)width;
        inParams["HeightPixels"] = (ushort)height;
        using var outParams = service.InvokeMethod(ThumbnailImageMethod, inParams, null);
        var returnValue = Convert.ToUInt32(outParams["ReturnValue"], CultureInfo.InvariantCulture);
        return returnValue == 0 ? outParams["ImageData"] as byte[] : null;
    }

    private static ManagementObject? GetRealizedSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        foreach (ManagementObject item in vm.GetRelated(VirtualSystemSettingClass, SettingsDefineStateAssociationClass, null, null, "SettingData", "ManagedElement", false, null))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(GetStringProperty(item, "VirtualSystemType"), "Microsoft:Hyper-V:System:Realized", StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }
}

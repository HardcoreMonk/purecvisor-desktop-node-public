using System.Globalization;
using System.Management;
using System.Xml;
using System.Xml.Linq;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

internal interface IDesktopNodeVirtualDiskOperations
{
    ulong GetMaxInternalSize(string path, CancellationToken cancellationToken);

    void Resize(string path, ulong requestedBytes, CancellationToken cancellationToken);
}

internal sealed class DesktopNodeWmiVirtualDiskOperations : IDesktopNodeVirtualDiskOperations
{
    private const string ImageManagementServiceClass = "Msvm_ImageManagementService";
    private const string GetVirtualHardDiskSettingDataMethod = "GetVirtualHardDiskSettingData";
    private const string ResizeVirtualHardDiskMethod = "ResizeVirtualHardDisk";

    public ulong GetMaxInternalSize(string path, CancellationToken cancellationToken)
    {
        var scope = CreateScope(connect: true);
        using var service = GetService(scope, ImageManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(GetVirtualHardDiskSettingDataMethod);
        inParams["Path"] = path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(GetVirtualHardDiskSettingDataMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.disk-resize.inspect", cancellationToken);

        var settingData = Convert.ToString(outParams["SettingData"], CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(settingData))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_DISK_SETTINGS_INVALID",
                "Hyper-V virtual disk settings did not include size data.",
                "GetVirtualHardDiskSettingData returned an empty SettingData instance.",
                true);
        }

        return ReadMaxInternalSize(settingData);
    }

    public void Resize(string path, ulong requestedBytes, CancellationToken cancellationToken)
    {
        var scope = CreateScope(connect: true);
        using var service = GetService(scope, ImageManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ResizeVirtualHardDiskMethod);
        inParams["Path"] = path;
        inParams["MaxInternalSize"] = requestedBytes;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ResizeVirtualHardDiskMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.disk-resize", cancellationToken);
    }

    private static ulong ReadMaxInternalSize(string settingData)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        using var textReader = new StringReader(settingData);
        using var xmlReader = XmlReader.Create(textReader, settings);
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        var property = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName.Equals("PROPERTY", StringComparison.OrdinalIgnoreCase) &&
                string.Equals((string?)element.Attribute("NAME"), "MaxInternalSize", StringComparison.OrdinalIgnoreCase));
        var value = property?
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName.Equals("VALUE", StringComparison.OrdinalIgnoreCase))?
            .Value;
        if (!ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var maxInternalSize))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_DISK_SETTINGS_INVALID",
                "Hyper-V virtual disk settings did not include a valid maximum internal size.",
                "Msvm_VirtualHardDiskSettingData.MaxInternalSize was missing or invalid.",
                true);
        }

        return maxInternalSize;
    }
}

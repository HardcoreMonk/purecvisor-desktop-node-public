using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider
{
    // Storage summaries list VHD paths only, so DVD drives (with or without ISO media) are counted separately
    // for the device-add guard. Null means the count could not be read.
    private static int? GetDvdDriveCount(ManagementObject setting)
    {
        try
        {
            using var related = setting.GetRelated(
                ResourceAllocationSettingClass,
                SettingDataComponentAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "PartComponent",
                thisRole: "GroupComponent",
                classDefinitionsOnly: false,
                options: null);

            var count = 0;
            foreach (ManagementObject item in related)
            {
                using (item)
                {
                    if (string.Equals(GetStringProperty(item, "ResourceSubType"), SyntheticDvdDriveSubtype, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        catch (ManagementException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static IReadOnlyList<DesktopNodeHyperVWmiVmNetworkSummary> GetNetworkSummaries(ManagementObject setting)
    {
        try
        {
            using var related = setting.GetRelated(
                EthernetPortAllocationSettingClass,
                SettingDataComponentAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "PartComponent",
                thisRole: "GroupComponent",
                classDefinitionsOnly: false,
                options: null);

            var result = new List<DesktopNodeHyperVWmiVmNetworkSummary>();
            foreach (ManagementObject item in related)
            {
                using (item)
                {
                    var switchName = GetStringProperty(item, "LastKnownSwitchName") ??
                        GetFirstStringArrayItem(item, "Connection", static value => !string.IsNullOrWhiteSpace(value));
                    if (!string.IsNullOrWhiteSpace(switchName))
                    {
                        result.Add(new DesktopNodeHyperVWmiVmNetworkSummary(switchName));
                    }
                }
            }

            return result;
        }
        catch (ManagementException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }
}

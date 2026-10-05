using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider : IDesktopNodeHyperVVmProvider
{
    public const string CimQuery = DesktopNodeHyperVWmiCommon.VmQuery;
    public const string VirtualSystemSettingClass = "Msvm_VirtualSystemSettingData";
    public const string SettingsDefineStateAssociationClass = "Msvm_SettingsDefineState";
    public const string SettingDataComponentAssociationClass = "Msvm_VirtualSystemSettingDataComponent";
    public const string ProcessorSettingClass = "Msvm_ProcessorSettingData";
    public const string MemorySettingClass = "Msvm_MemorySettingData";
    public const string SnapshotAssociationClass = "Msvm_SnapshotOfVirtualSystem";
    public const string StorageSettingClass = "Msvm_StorageAllocationSettingData";
    public const string VirtualDvdDiskSubtype = "Microsoft:Hyper-V:Virtual CD/DVD Disk";
    private const string DvdStorageKind = "dvd";
    public const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    public const string ResourceAllocationSettingClass = "Msvm_ResourceAllocationSettingData";
    public const string SyntheticDvdDriveSubtype = "Microsoft:Hyper-V:Synthetic DVD Drive";

    public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(
            CreateScope(),
            new ObjectQuery(CimQuery));

        var vms = new List<DesktopNodeHyperVVmInfo>();
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (item)
            {
                var name = GetStringProperty(item, "ElementName") ??
                    GetStringProperty(item, "Name") ??
                    string.Empty;
                vms.Add(MapSummary(ReadSummary(item, name)));
            }
        }

        return vms;
    }

    public static DesktopNodeHyperVVmInfo MapSummary(DesktopNodeHyperVWmiVmSummary summary)
    {
        var name = string.IsNullOrWhiteSpace(summary.Name) ? summary.Id : summary.Name;
        var id = string.IsNullOrWhiteSpace(summary.Id) ? name : summary.Id;
        var state = MapEnabledState(summary.EnabledState);
        return new DesktopNodeHyperVVmInfo(
            Id: id,
            Name: name,
            Platform: "hyperv",
            GuestFamily: MapGuestFamily(summary.Notes),
            State: state,
            Cpu: new DesktopNodeHyperVVmCpuInfo(ConvertToInt32(summary.ProcessorCount)),
            Memory: new DesktopNodeHyperVVmMemoryInfo(
                MapMemoryQuantityToMb(summary.StartupMemoryQuantity, summary.StartupMemoryQuantityUnits),
                null,
                false),
            Generation: MapGeneration(summary.GenerationSubtype),
            Storage: MapStorage(summary.Storage),
            Network: MapNetwork(summary.Network),
            Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(summary.CheckpointCount),
            Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
            ManagedByPurecvisor: DesktopNodeHyperVManagedNotes.IsManagedNotes(summary.Notes),
            CreatedAt: summary.CreationTime,
            LastPoweredOn: MapLastPoweredOn(state, summary.TimeOfLastStateChange),
            Notes: DesktopNodeHyperVManagedNotes.OperatorNotes(summary.Notes),
            TemplateLock: DesktopNodeHyperVManagedNotes.IsTemplateLocked(summary.Notes),
            DvdDrives: summary.DvdDriveCount is { } dvdDriveCount ? new DesktopNodeHyperVVmDvdDriveInfo(dvdDriveCount) : null,
            DvdMedia: summary.Storage is null ? null : MapDvdMedia(summary.Storage));
    }

    private static string? MapLastPoweredOn(string state, string? timeOfLastStateChange)
    {
        if (!string.Equals(state, "running", StringComparison.Ordinal))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(timeOfLastStateChange) ? null : timeOfLastStateChange;
    }

    private static DesktopNodeHyperVWmiVmSummary ReadSummary(ManagementObject vm, string name)
    {
        object? processorCount = null;
        object? startupMemoryQuantity = null;
        string? startupMemoryQuantityUnits = null;
        string? generationSubtype = null;
        string? notes = null;
        string? creationTime = null;
        IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary>? storage = [];
        IReadOnlyList<DesktopNodeHyperVWmiVmNetworkSummary> network = [];
        int? dvdDriveCount = null;

        try
        {
            using var settings = vm.GetRelated(
                VirtualSystemSettingClass,
                SettingsDefineStateAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "SettingData",
                thisRole: "ManagedElement",
                classDefinitionsOnly: false,
                options: null);

            foreach (ManagementObject setting in settings)
            {
                using (setting)
                {
                    processorCount = GetFirstRelatedProperty(setting, ProcessorSettingClass, "VirtualQuantity");
                    startupMemoryQuantity = GetFirstRelatedProperty(setting, MemorySettingClass, "VirtualQuantity");
                    startupMemoryQuantityUnits = GetFirstRelatedStringProperty(setting, MemorySettingClass, "VirtualQuantityUnits");
                    generationSubtype = GetStringProperty(setting, "VirtualSystemSubType");
                    notes = GetStringProperty(setting, "Notes");
                    creationTime = GetDateTimeProperty(setting, "CreationTime");
                    storage = GetStorageSummaries(setting);
                    network = GetNetworkSummaries(setting);
                    dvdDriveCount = GetDvdDriveCount(setting);
                }

                break;
            }
        }
        catch (ManagementException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return new DesktopNodeHyperVWmiVmSummary(
            Id: name,
            Name: name,
            EnabledState: vm.Properties["EnabledState"]?.Value,
            ProcessorCount: processorCount,
            StartupMemoryQuantity: startupMemoryQuantity,
            StartupMemoryQuantityUnits: startupMemoryQuantityUnits,
            GenerationSubtype: generationSubtype,
            CheckpointCount: GetCheckpointCount(vm),
            Notes: notes,
            Storage: storage,
            Network: network,
            CreationTime: creationTime,
            TimeOfLastStateChange: GetDateTimeProperty(vm, "TimeOfLastStateChange"),
            DvdDriveCount: dvdDriveCount);
    }

    private static int? ConvertToInt32(object? value)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            var number = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (number < int.MinValue || number > int.MaxValue)
            {
                return null;
            }

            return (int)number;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static int? MapMemoryQuantityToMb(object? quantity, string? units)
    {
        if (quantity is null || string.IsNullOrWhiteSpace(units))
        {
            return null;
        }

        double value;
        try
        {
            value = Convert.ToDouble(quantity, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }

        var normalizedUnits = new string(units
            .Trim()
            .ToLowerInvariant()
            .Where(static item => !char.IsWhiteSpace(item))
            .ToArray());
        var megabytes = normalizedUnits switch
        {
            "byte*2^20" => value,
            "mb" => value,
            "mib" => value,
            "megabyte" => value,
            "megabytes" => value,
            "byte*2^30" => value * 1024,
            "gb" => value * 1024,
            "gib" => value * 1024,
            "gigabyte" => value * 1024,
            "gigabytes" => value * 1024,
            "byte" => value / 1024 / 1024,
            "bytes" => value / 1024 / 1024,
            "byte*2^10" => value / 1024,
            "byte*10^3" => value / 1024,
            "kb" => value / 1024,
            "kib" => value / 1024,
            "kilobyte" => value / 1024,
            "kilobytes" => value / 1024,
            _ => double.NaN
        };

        if (double.IsNaN(megabytes) || megabytes < 0 || megabytes > int.MaxValue)
        {
            return null;
        }

        return (int)Math.Floor(megabytes);
    }

    private static int? MapGeneration(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype))
        {
            return null;
        }

        var trimmed = subtype.Trim();
        if (trimmed.EndsWith(":1", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (trimmed.EndsWith(":2", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
            parsed is 1 or 2
            ? parsed
            : null;
    }

    private static string MapGuestFamily(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return "unknown";
        }

        var explicitFamily = MapExplicitGuestFamily(notes);
        if (explicitFamily is not null)
        {
            return explicitFamily;
        }

        return MapFamilyFromText(notes) ?? "unknown";
    }

    private static string? MapExplicitGuestFamily(string notes)
    {
        foreach (var line in notes.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var delimiter = line.IndexOfAny(['=', ':']);
            if (delimiter <= 0)
            {
                continue;
            }

            var key = line[..delimiter].Trim().Replace("_", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
            if (!string.Equals(key, "guestfamily", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return MapFamilyFromText(line[(delimiter + 1)..]);
        }

        return null;
    }

    private static string? MapFamilyFromText(string text)
    {
        if (text.Contains("windows", StringComparison.OrdinalIgnoreCase))
        {
            return "windows";
        }

        return text.Contains("linux", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("ubuntu", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("debian", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("rhel", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("centos", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("suse", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("fedora", StringComparison.OrdinalIgnoreCase)
            ? "linux"
            : null;
    }

    private static IReadOnlyList<DesktopNodeHyperVVmDiskInfo> MapStorage(IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary>? storage)
    {
        if (storage is null || storage.Count == 0)
        {
            return [];
        }

        var disks = new List<DesktopNodeHyperVVmDiskInfo>();
        foreach (var item in storage)
        {
            if (item.Kind == DvdStorageKind || !IsVhdPath(item.Path))
            {
                continue;
            }

            disks.Add(new DesktopNodeHyperVVmDiskInfo("vhdx", item.Path, null, item.Attached));
        }

        return disks;
    }

    private static IReadOnlyList<DesktopNodeHyperVVmDvdMediaInfo> MapDvdMedia(IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary> storage)
    {
        return storage
            .Where(item => item.Kind == DvdStorageKind && !string.IsNullOrWhiteSpace(item.Path))
            .Select(item => new DesktopNodeHyperVVmDvdMediaInfo(item.Path!))
            .ToArray();
    }

    private static IReadOnlyList<DesktopNodeHyperVVmNetworkInfo> MapNetwork(IReadOnlyList<DesktopNodeHyperVWmiVmNetworkSummary>? network)
    {
        if (network is null || network.Count == 0)
        {
            return [];
        }

        var adapters = new List<DesktopNodeHyperVVmNetworkInfo>();
        foreach (var item in network)
        {
            if (string.IsNullOrWhiteSpace(item.SwitchName))
            {
                continue;
            }

            var switchName = item.SwitchName.Trim();
            adapters.Add(new DesktopNodeHyperVVmNetworkInfo(
                switchName,
                string.Equals(switchName, "Default Switch", StringComparison.OrdinalIgnoreCase)
                    ? "default-switch"
                    : "hyperv-switch"));
        }

        return adapters;
    }

    private static bool IsVhdPath(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) &&
            (path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase));
    }

    // 저장소 할당은 subtype 으로 나눈다. 가상 DVD 디스크의 HostResource 는 ISO 경로다. 읽기에 실패하면 null 을 돌려
    // dvd_media 를 생략하므로, 소비자는 "media 없음"(빈 목록)과 "모름"(필드 없음)을 구분할 수 있다.
    private static IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary>? GetStorageSummaries(ManagementObject setting)
    {
        try
        {
            using var related = setting.GetRelated(
                StorageSettingClass,
                SettingDataComponentAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "PartComponent",
                thisRole: "GroupComponent",
                classDefinitionsOnly: false,
                options: null);

            var result = new List<DesktopNodeHyperVWmiVmStorageSummary>();
            foreach (ManagementObject item in related)
            {
                using (item)
                {
                    if (string.Equals(item.Properties["ResourceSubType"]?.Value as string, VirtualDvdDiskSubtype, StringComparison.OrdinalIgnoreCase))
                    {
                        var iso = GetFirstStringArrayItem(item, "HostResource", value => !string.IsNullOrWhiteSpace(value));
                        if (iso is not null)
                        {
                            result.Add(new DesktopNodeHyperVWmiVmStorageSummary(iso, Attached: true, Kind: DvdStorageKind));
                        }

                        continue;
                    }

                    var path = GetFirstStringArrayItem(item, "HostResource", IsVhdPath);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        result.Add(new DesktopNodeHyperVWmiVmStorageSummary(path, Attached: true));
                    }
                }
            }

            return result;
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

    private static string? GetFirstStringArrayItem(
        ManagementBaseObject item,
        string propertyName,
        Func<string?, bool> predicate)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            if (value is string[] values)
            {
                return values.FirstOrDefault(predicate);
            }

            if (value is string text && predicate(text))
            {
                return text;
            }
        }
        catch (ManagementException)
        {
        }

        return null;
    }

    private static object? GetFirstRelatedProperty(ManagementObject item, string relatedClass, string propertyName)
    {
        try
        {
            using var related = item.GetRelated(
                relatedClass,
                SettingDataComponentAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "PartComponent",
                thisRole: "GroupComponent",
                classDefinitionsOnly: false,
                options: null);

            foreach (ManagementObject relatedItem in related)
            {
                using (relatedItem)
                {
                    return relatedItem.Properties[propertyName]?.Value;
                }
            }
        }
        catch (ManagementException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }

    private static string? GetFirstRelatedStringProperty(ManagementObject item, string relatedClass, string propertyName)
    {
        try
        {
            using var related = item.GetRelated(
                relatedClass,
                SettingDataComponentAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "PartComponent",
                thisRole: "GroupComponent",
                classDefinitionsOnly: false,
                options: null);

            foreach (ManagementObject relatedItem in related)
            {
                using (relatedItem)
                {
                    return GetStringProperty(relatedItem, propertyName);
                }
            }
        }
        catch (ManagementException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }

    private static int? GetCheckpointCount(ManagementObject vm)
    {
        try
        {
            using var snapshots = vm.GetRelated(
                VirtualSystemSettingClass,
                SnapshotAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "Dependent",
                thisRole: "Antecedent",
                classDefinitionsOnly: false,
                options: null);

            var count = 0;
            foreach (ManagementObject snapshot in snapshots)
            {
                using (snapshot)
                {
                    count++;
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
}

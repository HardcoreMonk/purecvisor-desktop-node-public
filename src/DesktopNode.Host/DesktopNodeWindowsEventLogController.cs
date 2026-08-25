using System.Diagnostics;
using Microsoft.Win32;

namespace DesktopNode.Host;

public sealed record DesktopNodeWindowsEventLogSnapshot(
    string LogName,
    string SourceName,
    bool Exists,
    string? EventMessageFile,
    bool Owned);

public sealed record DesktopNodeWindowsEventLogVolumePolicySnapshot(
    string LogName,
    long? MaximumSizeBytes,
    string? RetentionPolicy,
    bool VolumeGuarded);

public interface IDesktopNodeWindowsEventLogController
{
    DesktopNodeWindowsEventLogSnapshot Query(string logName, string sourceName, string expectedEventMessageFile);
    DesktopNodeWindowsEventLogSnapshot Register(string logName, string sourceName, string eventMessageFile);
    DesktopNodeWindowsEventLogSnapshot Remove(string logName, string sourceName, string expectedEventMessageFile);
    string WriteTestEvent(string logName, string sourceName, int eventId, string message);
    DesktopNodeWindowsEventLogVolumePolicySnapshot QueryVolumePolicy(string logName);
}

public sealed class DesktopNodeWindowsEventLogController : IDesktopNodeWindowsEventLogController
{
    private const string EventLogRoot = @"SYSTEM\CurrentControlSet\Services\EventLog";

    public DesktopNodeWindowsEventLogSnapshot Query(string logName, string sourceName, string expectedEventMessageFile)
    {
        using var sourceKey = Registry.LocalMachine.OpenSubKey(SourceKeyPath(logName, sourceName), writable: false);
        if (sourceKey is null)
        {
            return new DesktopNodeWindowsEventLogSnapshot(
                LogName: logName,
                SourceName: sourceName,
                Exists: false,
                EventMessageFile: null,
                Owned: false);
        }

        var eventMessageFile = sourceKey.GetValue("EventMessageFile") as string;
        return new DesktopNodeWindowsEventLogSnapshot(
            LogName: logName,
            SourceName: sourceName,
            Exists: true,
            EventMessageFile: eventMessageFile,
            Owned: IsOwned(eventMessageFile, expectedEventMessageFile));
    }

    public DesktopNodeWindowsEventLogSnapshot Register(string logName, string sourceName, string eventMessageFile)
    {
        try
        {
            using var sourceKey = Registry.LocalMachine.CreateSubKey(SourceKeyPath(logName, sourceName), writable: true);
            if (sourceKey is null)
            {
                throw new DesktopNodeWindowsEventLogControllerException(
                    "PCV_HOST_EVENTLOG_SOURCE_REGISTER_FAILED",
                    $"Windows Event Log source '{sourceName}' could not be created.");
            }

            sourceKey.SetValue("EventMessageFile", eventMessageFile, RegistryValueKind.String);
            sourceKey.SetValue("TypesSupported", 7, RegistryValueKind.DWord);
            return Query(logName, sourceName, eventMessageFile);
        }
        catch (Exception error) when (error is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            throw new DesktopNodeWindowsEventLogControllerException(
                "PCV_HOST_EVENTLOG_SOURCE_REGISTER_FAILED",
                $"Windows Event Log source '{sourceName}' could not be registered: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsEventLogSnapshot Remove(string logName, string sourceName, string expectedEventMessageFile)
    {
        var current = Query(logName, sourceName, expectedEventMessageFile);
        if (!current.Exists)
        {
            return current;
        }

        if (!current.Owned)
        {
            throw new DesktopNodeWindowsEventLogControllerException(
                "PCV_HOST_EVENTLOG_SOURCE_OWNERSHIP_MISMATCH",
                $"Windows Event Log source '{sourceName}' exists but is not owned by '{expectedEventMessageFile}'.");
        }

        try
        {
            using var logKey = Registry.LocalMachine.OpenSubKey(LogKeyPath(logName), writable: true);
            if (logKey is null)
            {
                return Query(logName, sourceName, expectedEventMessageFile);
            }

            logKey.DeleteSubKeyTree(sourceName, throwOnMissingSubKey: false);
            return Query(logName, sourceName, expectedEventMessageFile);
        }
        catch (Exception error) when (error is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            throw new DesktopNodeWindowsEventLogControllerException(
                "PCV_HOST_EVENTLOG_SOURCE_REMOVE_FAILED",
                $"Windows Event Log source '{sourceName}' could not be removed: {error.Message}",
                error);
        }
    }

    public string WriteTestEvent(string logName, string sourceName, int eventId, string message)
    {
        try
        {
            EventLog.WriteEntry(sourceName, message, EventLogEntryType.Information, eventId);
            return "write-query-pass";
        }
        catch (Exception error) when (error is InvalidOperationException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            throw new DesktopNodeWindowsEventLogControllerException(
                "PCV_HOST_EVENTLOG_WRITE_TEST_FAILED",
                $"Windows Event Log source '{sourceName}' could not write test event {eventId}: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsEventLogVolumePolicySnapshot QueryVolumePolicy(string logName)
    {
        using var logKey = Registry.LocalMachine.OpenSubKey(LogKeyPath(logName), writable: false);
        var maxSize = ToNullableInt64(logKey?.GetValue("MaxSize"));
        var retention = ToNullableInt64(logKey?.GetValue("Retention"));
        var retentionPolicy = retention switch
        {
            0 => "overwrite-as-needed",
            null => "unknown",
            _ => "retain"
        };
        var volumeGuarded = maxSize is > 0 and <= 33_554_432 && retention == 0;
        return new DesktopNodeWindowsEventLogVolumePolicySnapshot(
            LogName: logName,
            MaximumSizeBytes: maxSize,
            RetentionPolicy: retentionPolicy,
            VolumeGuarded: volumeGuarded);
    }

    private static string LogKeyPath(string logName)
    {
        return $@"{EventLogRoot}\{logName}";
    }

    private static string SourceKeyPath(string logName, string sourceName)
    {
        return $@"{EventLogRoot}\{logName}\{sourceName}";
    }

    private static bool IsOwned(string? eventMessageFile, string expectedEventMessageFile)
    {
        return !string.IsNullOrWhiteSpace(eventMessageFile) &&
            string.Equals(
                NormalizePath(eventMessageFile),
                NormalizePath(expectedEventMessageFile),
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return path.Trim().Trim('"').Replace('/', '\\').TrimEnd('\\');
    }

    private static long? ToNullableInt64(object? value)
    {
        return value switch
        {
            int intValue => intValue,
            long longValue => longValue,
            null => null,
            _ => long.TryParse(value.ToString(), out var parsed) ? parsed : null
        };
    }
}

public sealed class DesktopNodeWindowsEventLogControllerException : Exception
{
    public DesktopNodeWindowsEventLogControllerException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

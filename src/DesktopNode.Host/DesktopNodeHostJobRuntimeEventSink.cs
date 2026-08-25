using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Host;

internal sealed class DesktopNodeHostJobRuntimeEventSink : IDesktopNodeJobRuntimeEventSink
{
    private const int JobRuntimeEventId = 4210;
    internal const long DefaultMaxJsonLineBytes = 5_242_880;
    internal const int DefaultRetainedJsonLineFiles = 5;
    private readonly object writeSync = new();
    private readonly string? jsonLinePath;
    private readonly string eventLogSource;
    private readonly string eventLogName;
    private readonly string eventMessageFile;
    private readonly bool writeWindowsEventLog;
    private readonly long maxJsonLineBytes;
    private readonly int retainedJsonLineFiles;
    private readonly Func<string, string, string, bool> eventLogSourceOwned;
    private readonly Action<string, string, EventLogEntryType, int> eventLogWrite;

    public DesktopNodeHostJobRuntimeEventSink(DesktopNodeHostOptions options)
        : this(
            options,
            (logName, sourceName, expectedEventMessageFile) =>
                new DesktopNodeWindowsEventLogController()
                    .Query(logName, sourceName, expectedEventMessageFile)
                    .Owned,
            EventLog.WriteEntry,
            DefaultMaxJsonLineBytes,
            DefaultRetainedJsonLineFiles,
            null)
    {
    }

    internal DesktopNodeHostJobRuntimeEventSink(
        DesktopNodeHostOptions options,
        Func<string, string, string, bool> eventLogSourceOwned,
        Action<string, string, EventLogEntryType, int> eventLogWrite,
        long maxJsonLineBytes,
        int retainedJsonLineFiles,
        bool? writeWindowsEventLogOverride)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(eventLogSourceOwned);
        ArgumentNullException.ThrowIfNull(eventLogWrite);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxJsonLineBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(retainedJsonLineFiles, 1);
        ValidatePathSeparation(options, retainedJsonLineFiles);
        jsonLinePath = string.IsNullOrWhiteSpace(options.EventLogPath)
            ? null
            : Path.GetFullPath(options.EventLogPath);
        eventLogSource = string.IsNullOrWhiteSpace(options.EventLogProviderSource)
            ? "PureCVisor Desktop Node"
            : options.EventLogProviderSource;
        eventLogName = string.IsNullOrWhiteSpace(options.EventLogProviderLogName)
            ? "Application"
            : options.EventLogProviderLogName;
        eventMessageFile = ResolveEventMessageFile(Environment.ProcessPath, AppContext.BaseDirectory);
        writeWindowsEventLog = writeWindowsEventLogOverride ??
            (OperatingSystem.IsWindows() &&
             string.Equals(options.EventLogWriter, "windows-event-log", StringComparison.OrdinalIgnoreCase));
        this.eventLogSourceOwned = eventLogSourceOwned;
        this.eventLogWrite = eventLogWrite;
        this.maxJsonLineBytes = maxJsonLineBytes;
        this.retainedJsonLineFiles = retainedJsonLineFiles;
    }

    internal static string ResolveEventMessageFile(string? processPath, string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            return Path.GetFullPath(processPath);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        return Path.GetFullPath(Path.Combine(baseDirectory, "DesktopNode.Host.exe"));
    }

    internal static void ValidatePathSeparation(
        DesktopNodeHostOptions options,
        int retainedJsonLineFiles = DefaultRetainedJsonLineFiles)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(retainedJsonLineFiles, 1);
        if (string.IsNullOrWhiteSpace(options.JobStorePath) ||
            string.IsNullOrWhiteSpace(options.EventLogPath))
        {
            return;
        }

        var jobStorePath = Path.GetFullPath(options.JobStorePath);
        var eventPath = Path.GetFullPath(options.EventLogPath);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var jobStoreIdentity =
            JsonFileDesktopNodeJobStore.CreateSupportedPhysicalPathIdentity(jobStorePath);
        var eventWritePaths = Enumerable.Range(0, retainedJsonLineFiles + 1)
            .Select(index => index == 0 ? eventPath : eventPath + "." + index)
            .ToArray();
        var eventWriteIdentities = eventWritePaths
            .Select(JsonFileDesktopNodeJobStore.CreateSupportedPhysicalPathIdentity)
            .ToArray();
        var managedExactIdentities = new[]
        {
            jobStoreIdentity,
            jobStoreIdentity + ".commit-pending",
            jobStoreIdentity + ".tmp"
        };
        var conflicts = eventWriteIdentities.Any(identity =>
            managedExactIdentities.Any(managed =>
                PathsContainOneAnother(identity, managed, comparison)) ||
            identity.StartsWith(jobStoreIdentity + ".tmp.", comparison) ||
            identity.StartsWith(jobStoreIdentity + ".commit-pending.tmp.", comparison));
        if (!conflicts)
        {
            var existingManagedPaths = ExistingManagedJobStorePaths(jobStorePath);
            var managedIdentities = existingManagedPaths
                .Select(TryGetExistingFileIdentity)
                .Where(identity => identity is not null)
                .Cast<ExistingFileIdentity>()
                .ToHashSet();
            conflicts = eventWritePaths
                .Select(TryGetExistingFileIdentity)
                .Any(identity => identity is ExistingFileIdentity existing &&
                    managedIdentities.Contains(existing));
        }

        if (conflicts)
        {
            throw new ArgumentException(
                "PCV_HOST_EVENT_LOG_JOB_STORE_CONFLICT|The JSONL event path must not overlap jobs.json or a managed job-store sidecar.|Configure a separate event log file outside the job-store primary and commit-guard names.",
                nameof(options));
        }
    }

    private static bool PathsContainOneAnother(
        string first,
        string second,
        StringComparison comparison)
    {
        var separator = OperatingSystem.IsWindows() ? '\\' : Path.DirectorySeparatorChar;
        var normalizedFirst = first.TrimEnd(separator);
        var normalizedSecond = second.TrimEnd(separator);
        return string.Equals(normalizedFirst, normalizedSecond, comparison) ||
            normalizedFirst.StartsWith(normalizedSecond + separator, comparison) ||
            normalizedSecond.StartsWith(normalizedFirst + separator, comparison);
    }

    private static IReadOnlyList<string> ExistingManagedJobStorePaths(string jobStorePath)
    {
        var paths = new List<string>
        {
            jobStorePath,
            jobStorePath + ".commit-pending",
            jobStorePath + ".tmp"
        };
        var directory = Path.GetDirectoryName(jobStorePath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return paths;
        }

        var fileName = Path.GetFileName(jobStorePath);
        try
        {
            paths.AddRange(Directory.EnumerateFiles(directory, fileName + ".tmp.*"));
            paths.AddRange(Directory.EnumerateFiles(directory, fileName + ".commit-pending.tmp.*"));
            return paths;
        }
        catch (Exception exception)
        {
            throw new ArgumentException(
                "PCV_HOST_EVENT_LOG_JOB_STORE_PATH_UNVERIFIED|Managed job-store sidecars could not be inspected before the JSONL sink was enabled.|Restore data-root read access and retry service startup.",
                nameof(jobStorePath),
                exception);
        }
    }

    private static ExistingFileIdentity? TryGetExistingFileIdentity(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var handle = File.OpenHandle(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            if (!GetFileInformationByHandle(handle, out var information))
            {
                throw new IOException(
                    "GetFileInformationByHandle failed with Win32 error " +
                    Marshal.GetLastWin32Error().ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
            }

            return new ExistingFileIdentity(
                information.VolumeSerialNumber,
                information.FileIndexHigh,
                information.FileIndexLow);
        }
        catch (Exception exception)
        {
            throw new ArgumentException(
                "PCV_HOST_EVENT_LOG_JOB_STORE_PATH_UNVERIFIED|An existing event/job-store file identity could not be inspected before the JSONL sink was enabled.|Restore data-root read access and retry service startup.",
                nameof(path),
                exception);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        Microsoft.Win32.SafeHandles.SafeFileHandle hFile,
        out ByHandleFileInformation lpFileInformation);

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        public uint FileAttributes;
        public uint CreationTimeLow;
        public uint CreationTimeHigh;
        public uint LastAccessTimeLow;
        public uint LastAccessTimeHigh;
        public uint LastWriteTimeLow;
        public uint LastWriteTimeHigh;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    private readonly record struct ExistingFileIdentity(
        uint VolumeSerialNumber,
        uint FileIndexHigh,
        uint FileIndexLow);

    public void Write(DesktopNodeJobRuntimeObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        var payload = JsonSerializer.Serialize(observation, RuntimePolicyContract.JsonOptions);

        lock (writeSync)
        {
            WriteJsonLineBestEffort(payload);
            WriteWindowsEventBestEffort(payload);
        }
    }

    private void WriteJsonLineBestEffort(string payload)
    {
        if (jsonLinePath is null)
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(jsonLinePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var line = payload + Environment.NewLine;
            var lineBytes = Encoding.UTF8.GetByteCount(line);
            if (File.Exists(jsonLinePath) &&
                new FileInfo(jsonLinePath).Length + lineBytes > maxJsonLineBytes)
            {
                RotateJsonLineFiles();
            }

            using var stream = new FileStream(
                jsonLinePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read | FileShare.Delete);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(line);
        }
        catch
        {
            // Runtime persistence and recovery must not depend on telemetry storage.
        }
    }

    private void RotateJsonLineFiles()
    {
        var oldest = jsonLinePath + "." + retainedJsonLineFiles;
        if (File.Exists(oldest))
        {
            File.Delete(oldest);
        }

        for (var index = retainedJsonLineFiles; index >= 2; index--)
        {
            var source = jsonLinePath + "." + (index - 1);
            var target = jsonLinePath + "." + index;
            if (File.Exists(source))
            {
                File.Move(source, target, overwrite: true);
            }
        }

        File.Move(jsonLinePath!, jsonLinePath + ".1", overwrite: true);
    }

    private void WriteWindowsEventBestEffort(string payload)
    {
        if (!writeWindowsEventLog)
        {
            return;
        }

        try
        {
            if (!eventLogSourceOwned(eventLogName, eventLogSource, eventMessageFile))
            {
                return;
            }

            eventLogWrite(
                eventLogSource,
                payload,
                EventLogEntryType.Warning,
                JobRuntimeEventId);
        }
        catch
        {
            // The JSONL diagnostic channel remains available when the provider is not registered.
        }
    }
}

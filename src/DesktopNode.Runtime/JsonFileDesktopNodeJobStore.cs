using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

internal enum JsonFileDesktopNodeJobStoreWriteStage
{
    CandidateTempFlushed,
    PendingCommitTempFlushed,
    PendingCommitFlushed,
    PrimaryReplaced
}

internal sealed class DesktopNodeJobStoreCorruptSnapshotException(
    string message,
    Exception innerException) : Exception(message, innerException);

internal sealed class JsonFileDesktopNodeJobStore : IDesktopNodeJobStore
{
    private const int PendingCommitVersion = 1;
    private static readonly TimeSpan WriteLeaseTimeout = TimeSpan.FromSeconds(5);
    private static readonly object FileSystemSync = new();
    private readonly Action<JsonFileDesktopNodeJobStoreWriteStage, string>? writeCheckpoint;
    private SnapshotIdentity? expectedPrimaryIdentity;

    public JsonFileDesktopNodeJobStore(string location)
        : this(location, null)
    {
    }

    internal JsonFileDesktopNodeJobStore(
        string location,
        Action<JsonFileDesktopNodeJobStoreWriteStage, string>? writeCheckpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        Location = NormalizeSupportedLocation(location);
        this.writeCheckpoint = writeCheckpoint;
    }

    public string Location { get; }

    public bool Exists()
    {
        lock (FileSystemSync)
        {
            ResolvePendingCommitOrThrow();
            try
            {
                var identity = ReadSnapshotIdentity(Location);
                expectedPrimaryIdentity = identity;
                return identity.Exists;
            }
            catch (Exception exception)
            {
                throw IndeterminateReadException(
                    "The job store could not determine whether an authoritative snapshot exists.",
                    exception);
            }
        }
    }

    public string ReadSnapshot()
    {
        lock (FileSystemSync)
        {
            ResolvePendingCommitOrThrow();
            try
            {
                var bytes = File.ReadAllBytes(Location);
                expectedPrimaryIdentity = SnapshotIdentity.FromBytes(bytes);
                return DecodeUtf8(bytes);
            }
            catch (DecoderFallbackException exception)
            {
                throw new DesktopNodeJobStoreCorruptSnapshotException(
                    "The authoritative job snapshot is not valid UTF-8.",
                    exception);
            }
            catch (Exception exception) when (IsFileAccessFailure(exception))
            {
                throw IndeterminateReadException(
                    "The authoritative job snapshot could not be read.",
                    exception);
            }
        }
    }

    public DesktopNodeJobStoreWriteResult WriteSnapshot(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        lock (FileSystemSync)
        {
            return WriteSnapshotCore(json);
        }
    }

    private DesktopNodeJobStoreWriteResult WriteSnapshotCore(string json)
    {
        if (!TryAcquireWriteLease(out var writeLease, out var leaseFailure))
        {
            return DesktopNodeJobStoreWriteResult.NotCommitted(leaseFailure!);
        }

        using (writeLease)
        {
            return WriteSnapshotWithLease(json);
        }
    }

    private DesktopNodeJobStoreWriteResult WriteSnapshotWithLease(string json)
    {

        try
        {
            var directory = Path.GetDirectoryName(Location);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
            EnsurePrimaryHasSingleLinkIfPresent();
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.NotCommitted(exception);
        }

        var pendingResolution = TryResolvePendingCommit();
        if (pendingResolution.Outcome != DesktopNodeJobStoreCommitOutcome.Committed)
        {
            return pendingResolution;
        }

        BestEffortDelete(Location + ".tmp");

        var candidateBytes = Encoding.UTF8.GetBytes(json);
        SnapshotIdentity previousIdentity;
        try
        {
            previousIdentity = ReadSnapshotIdentity(Location);
            if (expectedPrimaryIdentity is SnapshotIdentity expectedIdentity &&
                expectedIdentity != previousIdentity)
            {
                return DesktopNodeJobStoreWriteResult.NotCommitted(
                    new DesktopNodeJobStoreConcurrencyException(
                        "The authoritative job snapshot changed after this runtime loaded its base identity."));
            }

            expectedPrimaryIdentity ??= previousIdentity;
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.NotCommitted(exception);
        }

        var tempPath = Location + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            WriteDurableFile(tempPath, candidateBytes, FileMode.CreateNew);
            writeCheckpoint?.Invoke(
                JsonFileDesktopNodeJobStoreWriteStage.CandidateTempFlushed,
                tempPath);
        }
        catch (Exception exception)
        {
            BestEffortDelete(tempPath);
            return DesktopNodeJobStoreWriteResult.NotCommitted(exception);
        }

        var pendingPath = PendingCommitPath;
        byte[] pendingBytes;
        try
        {
            pendingBytes = SerializePendingCommit(candidateBytes, previousIdentity);
        }
        catch (Exception exception)
        {
            BestEffortDelete(tempPath);
            return DesktopNodeJobStoreWriteResult.NotCommitted(exception);
        }

        var pendingCreated = false;
        var pendingTempPath = pendingPath + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            WriteDurableFile(pendingTempPath, pendingBytes, FileMode.CreateNew);
            writeCheckpoint?.Invoke(
                JsonFileDesktopNodeJobStoreWriteStage.PendingCommitTempFlushed,
                pendingTempPath);
            File.Move(pendingTempPath, pendingPath);
            pendingCreated = true;

            writeCheckpoint?.Invoke(
                JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed,
                tempPath);

            EnsurePrimaryHasSingleLinkIfPresent();
            File.Move(tempPath, Location, overwrite: true);
            writeCheckpoint?.Invoke(
                JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced,
                tempPath);

            var result = FinalizeCommitAttempt(
                ComparePrimary(candidateBytes, previousIdentity),
                new IOException("The authoritative job snapshot did not match the flushed candidate after replacement."));
            UpdateExpectedIdentity(result, candidateBytes, previousIdentity);
            return result;
        }
        catch (Exception exception)
        {
            if (!pendingCreated)
            {
                return DesktopNodeJobStoreWriteResult.NotCommitted(exception);
            }

            var result = FinalizeCommitAttempt(
                ComparePrimary(candidateBytes, previousIdentity),
                exception);
            UpdateExpectedIdentity(result, candidateBytes, previousIdentity);
            return result;
        }
        finally
        {
            BestEffortDelete(tempPath);
            BestEffortDelete(pendingTempPath);
        }
    }

    public void Quarantine(string suffix)
    {
        lock (FileSystemSync)
        {
            if (!TryAcquireWriteLease(out var writeLease, out var leaseFailure))
            {
                throw leaseFailure!;
            }

            using (writeLease)
            {
                ResolvePendingCommitOrThrow();
                EnsurePrimaryHasSingleLinkIfPresent();
                var currentIdentity = ReadSnapshotIdentity(Location);
                if (expectedPrimaryIdentity is SnapshotIdentity expectedIdentity &&
                    expectedIdentity != currentIdentity)
                {
                    throw new DesktopNodeJobStoreConcurrencyException(
                        "The authoritative job snapshot changed after this runtime loaded its base identity.");
                }

                File.Move(Location, Location + suffix, overwrite: true);
                expectedPrimaryIdentity = SnapshotIdentity.Missing;
            }
        }
    }

    private string PendingCommitPath => Location + ".commit-pending";

    private DesktopNodeJobStoreWriteResult FinalizeCommitAttempt(
        DesktopNodeJobStoreCommitOutcome outcome,
        Exception failure)
    {
        if (outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(failure);
        }

        if (!TryDelete(PendingCommitPath, out var cleanupFailure))
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(
                new IOException(
                    "The job snapshot outcome was known, but its pending-commit guard could not be removed.",
                    cleanupFailure));
        }

        return outcome == DesktopNodeJobStoreCommitOutcome.Committed
            ? DesktopNodeJobStoreWriteResult.Committed
            : DesktopNodeJobStoreWriteResult.NotCommitted(failure);
    }

    private void UpdateExpectedIdentity(
        DesktopNodeJobStoreWriteResult result,
        byte[] candidateBytes,
        SnapshotIdentity previousIdentity)
    {
        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Committed)
        {
            expectedPrimaryIdentity = SnapshotIdentity.FromBytes(candidateBytes);
        }
        else if (result.Outcome == DesktopNodeJobStoreCommitOutcome.NotCommitted)
        {
            expectedPrimaryIdentity = previousIdentity;
        }
    }

    private DesktopNodeJobStoreCommitOutcome ComparePrimary(
        byte[] candidateBytes,
        SnapshotIdentity previousIdentity)
    {
        try
        {
            var primaryBytes = File.ReadAllBytes(Location);
            if (primaryBytes.AsSpan().SequenceEqual(candidateBytes))
            {
                return DesktopNodeJobStoreCommitOutcome.Committed;
            }

            return SnapshotIdentity.FromBytes(primaryBytes) == previousIdentity
                ? DesktopNodeJobStoreCommitOutcome.NotCommitted
                : DesktopNodeJobStoreCommitOutcome.Indeterminate;
        }
        catch (Exception exception) when (IsMissingFile(exception))
        {
            return previousIdentity.Exists
                ? DesktopNodeJobStoreCommitOutcome.Indeterminate
                : DesktopNodeJobStoreCommitOutcome.NotCommitted;
        }
        catch
        {
            return DesktopNodeJobStoreCommitOutcome.Indeterminate;
        }
    }

    private void ResolvePendingCommitOrThrow()
    {
        var result = TryResolvePendingCommit();
        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Committed)
        {
            return;
        }

        throw new DesktopNodeJobStoreCommitException(
            result.Outcome,
            "The job store has an unresolved pending commit and cannot expose an authoritative snapshot.",
            result.Failure);
    }

    private DesktopNodeJobStoreWriteResult TryResolvePendingCommit()
    {
        byte[] pendingBytes;
        try
        {
            pendingBytes = File.ReadAllBytes(PendingCommitPath);
        }
        catch (Exception exception) when (IsMissingFile(exception))
        {
            return DesktopNodeJobStoreWriteResult.Committed;
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(exception);
        }

        PendingCommit pending;
        SnapshotIdentity primaryIdentity;
        try
        {
            pending = ParsePendingCommit(pendingBytes);
            primaryIdentity = ReadSnapshotIdentity(Location);
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(exception);
        }

        var candidateMatches = primaryIdentity.Exists &&
            primaryIdentity.Length == pending.CandidateLength &&
            string.Equals(
                primaryIdentity.Sha256,
                pending.CandidateSha256,
                StringComparison.OrdinalIgnoreCase);
        var previousMatches = primaryIdentity.Exists == pending.PreviousExists &&
            (!primaryIdentity.Exists ||
                (primaryIdentity.Length == pending.PreviousLength &&
                 string.Equals(
                     primaryIdentity.Sha256,
                     pending.PreviousSha256,
                     StringComparison.OrdinalIgnoreCase)));

        if (!candidateMatches && !previousMatches)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(
                new InvalidDataException(
                    "The primary job snapshot matches neither the pending candidate nor the previous snapshot identity."));
        }

        if (previousMatches && !primaryIdentity.Exists)
        {
            if (!TryDelete(PendingCommitPath, out var absentPreviousCleanupFailure))
            {
                return DesktopNodeJobStoreWriteResult.Indeterminate(
                    new IOException(
                        "The pending-commit guard for the confirmed absent previous snapshot could not be removed.",
                        absentPreviousCleanupFailure));
            }

            return DesktopNodeJobStoreWriteResult.Committed;
        }

        try
        {
            var primaryBytes = File.ReadAllBytes(Location);
            var validation = DesktopNodeJobStoreSnapshotValidator.Validate(DecodeUtf8(primaryBytes));
            if (validation.Kind != DesktopNodeJobStoreSnapshotValidationKind.Valid)
            {
                return DesktopNodeJobStoreWriteResult.Indeterminate(
                    new InvalidDataException(
                        "The identity-matched primary job snapshot failed semantic validation; the pending-commit guard was preserved."));
            }
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(exception);
        }

        if (!TryDelete(PendingCommitPath, out var cleanupFailure))
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(
                new IOException(
                    "The resolved pending-commit guard could not be removed.",
                    cleanupFailure));
        }

        return DesktopNodeJobStoreWriteResult.Committed;
    }

    private byte[] SerializePendingCommit(
        byte[] candidateBytes,
        SnapshotIdentity previousIdentity)
    {
        var candidateIdentity = SnapshotIdentity.FromBytes(candidateBytes);
        return JsonSerializer.SerializeToUtf8Bytes(
            new SortedDictionary<string, object?>
            {
                ["candidate_length"] = candidateIdentity.Length,
                ["candidate_sha256"] = candidateIdentity.Sha256,
                ["previous_exists"] = previousIdentity.Exists,
                ["previous_length"] = previousIdentity.Exists ? previousIdentity.Length : null,
                ["previous_sha256"] = previousIdentity.Exists ? previousIdentity.Sha256 : null,
                ["version"] = PendingCommitVersion
            },
            RuntimePolicyContract.JsonOptions);
    }

    internal static string CreateWriteLeaseName(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        var leaseIdentity = CreateSupportedPhysicalPathIdentity(location);

        var pathHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(leaseIdentity)))
            .ToLowerInvariant();
        return (OperatingSystem.IsWindows() ? @"Global\" : string.Empty) +
            "PureCVisor.DesktopNode.JobStore." + pathHash;
    }

    internal static string CreateSupportedPhysicalPathIdentity(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        var canonicalPath = NormalizeSupportedLocation(location)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return OperatingSystem.IsWindows()
            ? CreateWindowsVolumeLeaseIdentity(canonicalPath)
            : canonicalPath;
    }

    private static string CreateWindowsVolumeLeaseIdentity(string canonicalPath)
    {
        var root = Path.GetPathRoot(canonicalPath);
        if (string.IsNullOrWhiteSpace(root))
        {
            throw UnsupportedLocation(
                "The job-store path must resolve to a rooted local fixed drive.");
        }

        var deviceName = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var deviceTarget = new StringBuilder(1024);
        if (QueryDosDevice(deviceName, deviceTarget, deviceTarget.Capacity) == 0)
        {
            throw UnsupportedLocation(
                "The job-store drive mapping could not be verified.",
                new IOException(
                    "QueryDosDevice failed.",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())));
        }
        if (IsSubstDeviceTarget(deviceTarget.ToString()))
        {
            throw UnsupportedLocation(
                "SUBST and path-substitution drive aliases are not supported for the job-store path.");
        }

        var volumeName = new StringBuilder(64);
        if (!GetVolumeNameForVolumeMountPoint(root, volumeName, volumeName.Capacity))
        {
            throw UnsupportedLocation(
                "The job-store volume identity could not be verified.",
                new IOException(
                    "GetVolumeNameForVolumeMountPoint failed.",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())));
        }

        return CreateVolumeRelativeLeaseIdentity(
            volumeName.ToString(),
            canonicalPath[root.Length..]);
    }

    internal static string CreateVolumeRelativeLeaseIdentity(
        string volumeName,
        string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(volumeName);
        ArgumentNullException.ThrowIfNull(relativePath);
        return volumeName
            .Replace('/', '\\')
            .TrimEnd('\\')
            .ToUpperInvariant() + "\\" +
            relativePath
                .Replace('/', '\\')
                .TrimStart('\\')
                .ToUpperInvariant();
    }

    internal static bool IsSubstDeviceTarget(string deviceTarget)
    {
        return deviceTarget.StartsWith(@"\??\", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSupportedLocation(string location)
    {
        var fullPath = Path.GetFullPath(location);
        if (!OperatingSystem.IsWindows())
        {
            return fullPath;
        }

        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            throw UnsupportedLocation(
                "UNC and Windows device-namespace job-store paths are not supported.");
        }

        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root))
        {
            throw UnsupportedLocation(
                "The job-store path must resolve to a rooted local fixed drive.");
        }

        DriveType driveType;
        try
        {
            driveType = new DriveInfo(root).DriveType;
        }
        catch (Exception exception)
        {
            throw UnsupportedLocation(
                "The job-store drive type could not be verified.",
                exception);
        }

        if (!IsSupportedWindowsDriveType(driveType))
        {
            throw UnsupportedLocation(
                $"The job-store path must be on a local fixed drive; '{driveType}' is not supported.");
        }

        var relativePath = fullPath[root.Length..];
        if (relativePath.Contains(':'))
        {
            throw UnsupportedLocation(
                "Windows alternate data stream paths are not supported for job-store or event files.");
        }
        if (relativePath
            .Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries)
            .Any(IsObviousDosShortName))
        {
            throw UnsupportedLocation(
                "DOS 8.3 short-name aliases are not supported for the job-store path.");
        }

        RejectExistingReparsePoints(fullPath);
        return fullPath;
    }

    internal static bool IsSupportedWindowsDriveType(DriveType driveType)
    {
        return driveType == DriveType.Fixed;
    }

    private static bool IsObviousDosShortName(string component)
    {
        var lastDot = component.LastIndexOf('.');
        var stem = lastDot > 0 ? component[..lastDot] : component;
        var tilde = stem.LastIndexOf('~');
        if (tilde is <= 0 or > 6)
        {
            return false;
        }

        var numericTail = stem[(tilde + 1)..];
        return numericTail.Length is >= 1 and <= 6 &&
            numericTail.All(char.IsAsciiDigit);
    }

    private static void RejectExistingReparsePoints(string fullPath)
    {
        string? currentPath = fullPath;
        while (!string.IsNullOrWhiteSpace(currentPath))
        {
            try
            {
                var attributes = File.GetAttributes(currentPath);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw UnsupportedLocation(
                        $"The job-store path traverses the reparse point '{currentPath}'.");
                }
            }
            catch (Exception exception) when (
                exception is FileNotFoundException or DirectoryNotFoundException)
            {
                // Nonexistent descendants are allowed. Continue upward until an
                // existing ancestor is found so an alias cannot hide there.
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw UnsupportedLocation(
                    $"The job-store path component '{currentPath}' could not be verified.",
                    exception);
            }

            currentPath = Path.GetDirectoryName(
                currentPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar));
        }
    }

    private static ArgumentException UnsupportedLocation(
        string message,
        Exception? innerException = null)
    {
        return new ArgumentException(message, "location", innerException);
    }

    private void EnsurePrimaryHasSingleLinkIfPresent()
    {
        if (!OperatingSystem.IsWindows() || !File.Exists(Location))
        {
            return;
        }

        using var handle = File.OpenHandle(
            Location,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        if (!GetFileInformationByHandle(handle, out var information))
        {
            throw new IOException(
                "The authoritative job snapshot hard-link identity could not be verified.");
        }
        if (information.NumberOfLinks > 1)
        {
            throw new DesktopNodeJobStoreConcurrencyException(
                "The authoritative job snapshot has multiple hard links; hard-linked store aliases are not supported.");
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint QueryDosDevice(
        string lpDeviceName,
        StringBuilder lpTargetPath,
        int ucchMax);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeNameForVolumeMountPoint(
        string lpszVolumeMountPoint,
        StringBuilder lpszVolumeName,
        int cchBufferLength);

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

    private bool TryAcquireWriteLease(
        out NamedWriteLease? lease,
        out Exception? failure)
    {
        Mutex? mutex = null;
        try
        {
            mutex = new Mutex(
                initiallyOwned: false,
                CreateWriteLeaseName(Location));
            var acquired = false;
            try
            {
                acquired = mutex.WaitOne(WriteLeaseTimeout);
            }
            catch (AbandonedMutexException)
            {
                acquired = true;
            }

            if (!acquired)
            {
                mutex.Dispose();
                lease = null;
                failure = new DesktopNodeJobStoreConcurrencyException(
                    "Another process retained the canonical job-store write lease past the bounded wait.");
                return false;
            }

            lease = new NamedWriteLease(mutex);
            failure = null;
            return true;
        }
        catch (Exception exception)
        {
            mutex?.Dispose();
            lease = null;
            failure = new DesktopNodeJobStoreConcurrencyException(
                "The canonical job-store write lease could not be acquired.",
                exception);
            return false;
        }
    }

    private static PendingCommit ParsePendingCommit(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("version", out var versionElement) ||
            !versionElement.TryGetInt32(out var version) ||
            version != PendingCommitVersion ||
            !root.TryGetProperty("candidate_length", out var candidateLengthElement) ||
            !candidateLengthElement.TryGetInt64(out var candidateLength) ||
            candidateLength < 0 ||
            !root.TryGetProperty("candidate_sha256", out var candidateShaElement) ||
            candidateShaElement.ValueKind != JsonValueKind.String ||
            !IsSha256(candidateShaElement.GetString()) ||
            !root.TryGetProperty("previous_exists", out var previousExistsElement) ||
            previousExistsElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new InvalidDataException("The pending job-store commit guard is invalid.");
        }

        var previousExists = previousExistsElement.GetBoolean();
        long? previousLength = null;
        string? previousSha256 = null;
        if (previousExists)
        {
            if (!root.TryGetProperty("previous_length", out var previousLengthElement) ||
                !previousLengthElement.TryGetInt64(out var parsedPreviousLength) ||
                parsedPreviousLength < 0 ||
                !root.TryGetProperty("previous_sha256", out var previousShaElement) ||
                previousShaElement.ValueKind != JsonValueKind.String ||
                !IsSha256(previousShaElement.GetString()))
            {
                throw new InvalidDataException("The previous job snapshot identity is invalid.");
            }

            previousLength = parsedPreviousLength;
            previousSha256 = previousShaElement.GetString();
        }

        return new PendingCommit(
            candidateLength,
            candidateShaElement.GetString()!,
            previousExists,
            previousLength,
            previousSha256);
    }

    private static bool IsSha256(string? value)
    {
        return value is { Length: 64 } && value.All(Uri.IsHexDigit);
    }

    private static SnapshotIdentity ReadSnapshotIdentity(string path)
    {
        try
        {
            return SnapshotIdentity.FromBytes(File.ReadAllBytes(path));
        }
        catch (Exception exception) when (IsMissingFile(exception))
        {
            return SnapshotIdentity.Missing;
        }
    }

    private static void WriteDurableFile(string path, byte[] bytes, FileMode mode)
    {
        using var stream = OpenExclusiveWrite(path, mode);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private static string DecodeUtf8(byte[] bytes)
    {
        var offset = bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }) ? 3 : 0;
        return new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true).GetString(bytes, offset, bytes.Length - offset);
    }

    private static FileStream OpenExclusiveWrite(string path, FileMode mode)
    {
        return new FileStream(
            path,
            mode,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.None);
    }

    private static void BestEffortDelete(string path)
    {
        _ = TryDelete(path, out _);
    }

    private static bool TryDelete(string path, out Exception? failure)
    {
        try
        {
            File.Delete(path);

            failure = null;
            return true;
        }
        catch (Exception exception)
        {
            failure = exception;
            return false;
        }
    }

    private static DesktopNodeJobStoreCommitException IndeterminateReadException(
        string message,
        Exception innerException)
    {
        return new DesktopNodeJobStoreCommitException(
            DesktopNodeJobStoreCommitOutcome.Indeterminate,
            message,
            innerException);
    }

    private static bool IsMissingFile(Exception exception)
    {
        return exception is FileNotFoundException or DirectoryNotFoundException;
    }

    private static bool IsFileAccessFailure(Exception exception)
    {
        return exception is IOException or
            UnauthorizedAccessException or
            System.Security.SecurityException;
    }

    private sealed record PendingCommit(
        long CandidateLength,
        string CandidateSha256,
        bool PreviousExists,
        long? PreviousLength,
        string? PreviousSha256);

    private sealed class NamedWriteLease(Mutex mutex) : IDisposable
    {
        private Mutex? mutex = mutex;

        public void Dispose()
        {
            var ownedMutex = Interlocked.Exchange(ref mutex, null);
            if (ownedMutex is null)
            {
                return;
            }

            try
            {
                ownedMutex.ReleaseMutex();
            }
            finally
            {
                ownedMutex.Dispose();
            }
        }
    }

    private readonly record struct SnapshotIdentity(
        bool Exists,
        long Length,
        string? Sha256)
    {
        public static SnapshotIdentity Missing { get; } = new(false, 0, null);

        public static SnapshotIdentity FromBytes(byte[] bytes)
        {
            return new SnapshotIdentity(
                true,
                bytes.LongLength,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
    }
}

internal sealed class DesktopNodeJobStoreConcurrencyException : IOException
{
    public DesktopNodeJobStoreConcurrencyException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
